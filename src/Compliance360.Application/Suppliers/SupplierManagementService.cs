using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Suppliers;
using Compliance360.Shared;
using Microsoft.Extensions.Options;

namespace Compliance360.Application.Suppliers;

public sealed class SupplierManagementService : ISupplierManagementService
{
    private readonly ISupplierRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;
    private readonly SupplierManagementOptions _options;

    public SupplierManagementService(ISupplierRepository repository, IApplicationDbContext dbContext, IClock clock, IOptions<SupplierManagementOptions> options)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<Result<SupplierSummary>> CreateSupplierAsync(CreateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var taxIdentifier = Guard.AgainstNullOrWhiteSpace(command.TaxIdentifier, nameof(command.TaxIdentifier), 80).ToUpperInvariant();
            if (await _repository.TaxIdentifierExistsAsync(command.TenantId, taxIdentifier, cancellationToken))
            {
                return Result<SupplierSummary>.Failure("Supplier tax identifier already exists.");
            }

            var supplier = new Supplier(command.TenantId, command.LegalName, taxIdentifier, command.CountryCode, command.RequestedByUserId, _clock.UtcNow);
            await _repository.AddAsync(supplier, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, supplier.Id, AuditAction.SupplierCreated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<SupplierSummary>.Success(ToSummary(supplier));
        }
        catch (DomainException exception)
        {
            return Result<SupplierSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<SupplierDocumentSummary>> AddDocumentAsync(AddSupplierDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var supplier = await _repository.GetAsync(command.TenantId, command.SupplierId, cancellationToken);
            if (supplier is null)
            {
                return Result<SupplierDocumentSummary>.Failure("Supplier not found.");
            }

            var document = supplier.AddDocument(command.Type, command.DocumentNumber, command.StoredFileId, command.IssuedAtUtc, command.ExpiresAtUtc, command.RequestedByUserId);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, supplier.Id, AuditAction.SupplierUpdated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<SupplierDocumentSummary>.Success(ToSummary(document));
        }
        catch (DomainException exception)
        {
            return Result<SupplierDocumentSummary>.Failure(exception.Message);
        }
    }

    public Task<Result> ValidateDocumentAsync(ReviewSupplierDocumentCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeSupplierAsync(command.TenantId, command.SupplierId, command.RequestedByUserId, supplier => supplier.ValidateDocument(command.SupplierDocumentId, command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public Task<Result> RejectDocumentAsync(RejectSupplierDocumentCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeSupplierAsync(command.TenantId, command.SupplierId, command.RequestedByUserId, supplier => supplier.RejectDocument(command.SupplierDocumentId, command.Reason, command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public async Task<Result<SupplierEvaluationSummary>> AddEvaluationAsync(AddSupplierEvaluationCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var supplier = await _repository.GetAsync(command.TenantId, command.SupplierId, cancellationToken);
            if (supplier is null)
            {
                return Result<SupplierEvaluationSummary>.Failure("Supplier not found.");
            }

            var evaluation = supplier.AddEvaluation(command.Score, command.Comments, command.RequestedByUserId, _clock.UtcNow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, supplier.Id, AuditAction.SupplierUpdated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<SupplierEvaluationSummary>.Success(new SupplierEvaluationSummary(evaluation.Id, evaluation.SupplierId, evaluation.Score, evaluation.Comments));
        }
        catch (DomainException exception)
        {
            return Result<SupplierEvaluationSummary>.Failure(exception.Message);
        }
    }

    public Task<Result> HomologateAsync(SupplierActionCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeSupplierAsync(command.TenantId, command.SupplierId, command.RequestedByUserId, supplier => supplier.Homologate(command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public async Task<Result<SupplierExpirationAlertSummary>> CreateExpirationAlertAsync(CreateSupplierExpirationAlertCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var supplier = await _repository.GetAsync(command.TenantId, command.SupplierId, cancellationToken);
            if (supplier is null)
            {
                return Result<SupplierExpirationAlertSummary>.Failure("Supplier not found.");
            }

            var alert = supplier.CreateExpirationAlert(command.SupplierDocumentId, _clock.UtcNow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, supplier.Id, AuditAction.SupplierUpdated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<SupplierExpirationAlertSummary>.Success(new SupplierExpirationAlertSummary(alert.Id, alert.SupplierId, alert.SupplierDocumentId, alert.DocumentType, alert.Status, alert.ExpiresAtUtc));
        }
        catch (DomainException exception)
        {
            return Result<SupplierExpirationAlertSummary>.Failure(exception.Message);
        }
    }

    public Task<Result> SuspendAsync(SuspendSupplierCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeSupplierAsync(command.TenantId, command.SupplierId, command.RequestedByUserId, supplier => supplier.Suspend(command.Reason, command.RequestedByUserId), cancellationToken);
    }

    public async Task<Result<SupplierSearchResult>> SearchAsync(SupplierSearchQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, _options.MaxPageSize);
        return Result<SupplierSearchResult>.Success(await _repository.SearchAsync(new SupplierSearchCriteria(query.TenantId, query.SearchText, query.Status, page, pageSize), cancellationToken));
    }

    private async Task<Result> ChangeSupplierAsync(Guid tenantId, Guid supplierId, Guid userId, Action<Supplier> change, CancellationToken cancellationToken)
    {
        try
        {
            var supplier = await _repository.GetAsync(tenantId, supplierId, cancellationToken);
            if (supplier is null)
            {
                return Result.Failure("Supplier not found.");
            }

            change(supplier);
            await AppendAuditAsync(tenantId, userId, supplier.Id, AuditAction.SupplierUpdated, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private async Task AppendAuditAsync(Guid tenantId, Guid userId, Guid supplierId, AuditAction action, CancellationToken cancellationToken)
    {
        await _repository.AddAuditLogAsync(AuditLog.FromEvent(
            new AuditEvent(
                nameof(Supplier),
                supplierId,
                action,
                AuditLog.InferCategory(action),
                new AuditContext(tenantId, userId, null, null, null, null, null, null, null),
                new AuditSnapshot(null, null),
                new AuditMetadata("{\"source\":\"supplier-management\"}"),
                true,
                null),
            _clock.UtcNow), cancellationToken);
    }

    private static SupplierSummary ToSummary(Supplier supplier)
    {
        return new SupplierSummary(supplier.Id, supplier.TenantId, supplier.LegalName, supplier.TaxIdentifier, supplier.CountryCode, supplier.Status, supplier.HomologatedAtUtc);
    }

    private static SupplierDocumentSummary ToSummary(SupplierDocument document)
    {
        return new SupplierDocumentSummary(document.Id, document.SupplierId, document.Type, document.DocumentNumber, document.Status, document.ExpiresAtUtc);
    }
}

public sealed class SupplierManagementOptions
{
    public const string SectionName = "SupplierManagement";

    public int MaxPageSize { get; set; } = 200;
}
