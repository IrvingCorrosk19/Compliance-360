using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.TechnicalSheets;
using Compliance360.Shared;
using Microsoft.Extensions.Options;

namespace Compliance360.Application.TechnicalSheets;

public sealed class TechnicalSheetService : ITechnicalSheetService
{
    private readonly ITechnicalSheetRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;
    private readonly TechnicalSheetOptions _options;

    public TechnicalSheetService(ITechnicalSheetRepository repository, IApplicationDbContext dbContext, IClock clock, IOptions<TechnicalSheetOptions> options)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<Result<ProductSummary>> CreateProductAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sku = Guard.AgainstNullOrWhiteSpace(command.Sku, nameof(command.Sku), 80).ToUpperInvariant();
            if (await _repository.ProductSkuExistsAsync(command.TenantId, sku, cancellationToken))
            {
                return Result<ProductSummary>.Failure("Product SKU already exists.");
            }

            var product = new Product(command.TenantId, command.Name, sku, command.Description);
            await _repository.AddProductAsync(product, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, product.Id, AuditAction.TechnicalSheetCreated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<ProductSummary>.Success(new ProductSummary(product.Id, product.TenantId, product.Name, product.Sku, product.IsActive));
        }
        catch (DomainException exception)
        {
            return Result<ProductSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<TechnicalSheetSummary>> CreateSheetAsync(CreateTechnicalSheetCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.GetProductAsync(command.TenantId, command.ProductId, cancellationToken) is null)
            {
                return Result<TechnicalSheetSummary>.Failure("Product not found.");
            }

            var sheet = new TechnicalSheet(command.TenantId, command.ProductId, command.Title, command.RequestedByUserId, _clock.UtcNow);
            await _repository.AddSheetAsync(sheet, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, sheet.Id, AuditAction.TechnicalSheetCreated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<TechnicalSheetSummary>.Success(ToSummary(sheet));
        }
        catch (DomainException exception)
        {
            return Result<TechnicalSheetSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<TechnicalSheetVersionSummary>> CreateVersionAsync(CreateTechnicalSheetVersionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sheet = await _repository.GetSheetAsync(command.TenantId, command.TechnicalSheetId, cancellationToken);
            if (sheet is null)
            {
                return Result<TechnicalSheetVersionSummary>.Failure("Technical sheet not found.");
            }

            var version = sheet.CreateVersion(command.ChangeSummary, command.RequestedByUserId, _clock.UtcNow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, sheet.Id, AuditAction.TechnicalSheetUpdated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<TechnicalSheetVersionSummary>.Success(new TechnicalSheetVersionSummary(version.Id, version.TechnicalSheetId, version.VersionNumber, version.ChangeSummary));
        }
        catch (DomainException exception)
        {
            return Result<TechnicalSheetVersionSummary>.Failure(exception.Message);
        }
    }

    public Task<Result> AddIngredientAsync(AddIngredientCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeSheetAsync(command.TenantId, command.TechnicalSheetId, command.RequestedByUserId, sheet => sheet.AddIngredient(command.Name, command.Percentage, command.Allergen), cancellationToken);
    }

    public Task<Result> AddNutrientAsync(AddNutrientCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeSheetAsync(command.TenantId, command.TechnicalSheetId, command.RequestedByUserId, sheet => sheet.AddNutrient(command.Name, command.Amount, command.Unit), cancellationToken);
    }

    public Task<Result> AddCertificationAsync(AddCertificationCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeSheetAsync(command.TenantId, command.TechnicalSheetId, command.RequestedByUserId, sheet => sheet.AddCertification(command.Name, command.Issuer, command.ExpiresAtUtc), cancellationToken);
    }

    public Task<Result> SubmitAsync(TechnicalSheetActionCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeSheetAsync(command.TenantId, command.TechnicalSheetId, command.RequestedByUserId, sheet => sheet.SubmitForApproval(command.RequestedByUserId), cancellationToken);
    }

    public async Task<Result<TechnicalSheetApprovalSummary>> DecideAsync(DecideTechnicalSheetCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sheet = await _repository.GetSheetAsync(command.TenantId, command.TechnicalSheetId, cancellationToken);
            if (sheet is null)
            {
                return Result<TechnicalSheetApprovalSummary>.Failure("Technical sheet not found.");
            }

            var approval = sheet.Decide(command.Decision, command.Comments, command.RequestedByUserId, _clock.UtcNow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, sheet.Id, AuditAction.TechnicalSheetUpdated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<TechnicalSheetApprovalSummary>.Success(new TechnicalSheetApprovalSummary(approval.Id, approval.TechnicalSheetId, approval.VersionNumber, approval.Decision, approval.Comments));
        }
        catch (DomainException exception)
        {
            return Result<TechnicalSheetApprovalSummary>.Failure(exception.Message);
        }
    }

    public Task<Result> AttachPdfAsync(AttachTechnicalSheetPdfCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeSheetAsync(command.TenantId, command.TechnicalSheetId, command.RequestedByUserId, sheet => sheet.AttachPdf(command.PdfObjectKey), cancellationToken);
    }

    public Task<Result> MarkObsoleteAsync(TechnicalSheetActionCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeSheetAsync(command.TenantId, command.TechnicalSheetId, command.RequestedByUserId, sheet => sheet.MarkObsolete(command.RequestedByUserId), cancellationToken);
    }

    public async Task<Result<TechnicalSheetSearchResult>> SearchAsync(TechnicalSheetSearchQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, _options.MaxPageSize);
        return Result<TechnicalSheetSearchResult>.Success(await _repository.SearchAsync(new TechnicalSheetSearchCriteria(query.TenantId, query.SearchText, query.Status, query.ProductId, page, pageSize), cancellationToken));
    }

    private async Task<Result> ChangeSheetAsync(Guid tenantId, Guid sheetId, Guid userId, Action<TechnicalSheet> change, CancellationToken cancellationToken)
    {
        try
        {
            var sheet = await _repository.GetSheetAsync(tenantId, sheetId, cancellationToken);
            if (sheet is null)
            {
                return Result.Failure("Technical sheet not found.");
            }

            change(sheet);
            await AppendAuditAsync(tenantId, userId, sheet.Id, AuditAction.TechnicalSheetUpdated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private async Task AppendAuditAsync(Guid tenantId, Guid userId, Guid entityId, AuditAction action, CancellationToken cancellationToken)
    {
        await _repository.AddAuditLogAsync(AuditLog.FromEvent(
            new AuditEvent(
                nameof(TechnicalSheet),
                entityId,
                action,
                AuditLog.InferCategory(action),
                new AuditContext(tenantId, userId, null, null, null, null, null, null, null),
                new AuditSnapshot(null, null),
                new AuditMetadata("{\"source\":\"technical-sheets\"}"),
                true,
                null),
            _clock.UtcNow), cancellationToken);
    }

    private static TechnicalSheetSummary ToSummary(TechnicalSheet sheet)
    {
        return new TechnicalSheetSummary(sheet.Id, sheet.TenantId, sheet.ProductId, sheet.Title, sheet.Status, sheet.CurrentVersionNumber, sheet.PdfObjectKey);
    }
}

public sealed class TechnicalSheetOptions
{
    public const string SectionName = "TechnicalSheets";

    public int MaxPageSize { get; set; } = 200;
}
