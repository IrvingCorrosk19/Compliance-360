using Compliance360.Domain.Audit;
using Compliance360.Domain.TechnicalSheets;
using Compliance360.Shared;

namespace Compliance360.Application.TechnicalSheets;

public interface ITechnicalSheetService
{
    Task<Result<ProductSummary>> CreateProductAsync(CreateProductCommand command, CancellationToken cancellationToken = default);

    Task<Result<TechnicalSheetSummary>> CreateSheetAsync(CreateTechnicalSheetCommand command, CancellationToken cancellationToken = default);

    Task<Result<TechnicalSheetVersionSummary>> CreateVersionAsync(CreateTechnicalSheetVersionCommand command, CancellationToken cancellationToken = default);

    Task<Result> AddIngredientAsync(AddIngredientCommand command, CancellationToken cancellationToken = default);

    Task<Result> AddNutrientAsync(AddNutrientCommand command, CancellationToken cancellationToken = default);

    Task<Result> AddCertificationAsync(AddCertificationCommand command, CancellationToken cancellationToken = default);

    Task<Result> SubmitAsync(TechnicalSheetActionCommand command, CancellationToken cancellationToken = default);

    Task<Result<TechnicalSheetApprovalSummary>> DecideAsync(DecideTechnicalSheetCommand command, CancellationToken cancellationToken = default);

    Task<Result> AttachPdfAsync(AttachTechnicalSheetPdfCommand command, CancellationToken cancellationToken = default);

    Task<Result> MarkObsoleteAsync(TechnicalSheetActionCommand command, CancellationToken cancellationToken = default);

    Task<Result<TechnicalSheetSearchResult>> SearchAsync(TechnicalSheetSearchQuery query, CancellationToken cancellationToken = default);
}

public interface ITechnicalSheetRepository
{
    Task AddProductAsync(Product product, CancellationToken cancellationToken = default);

    Task<Product?> GetProductAsync(Guid tenantId, Guid productId, CancellationToken cancellationToken = default);

    Task<bool> ProductSkuExistsAsync(Guid tenantId, string sku, CancellationToken cancellationToken = default);

    Task AddSheetAsync(TechnicalSheet sheet, CancellationToken cancellationToken = default);

    Task<TechnicalSheet?> GetSheetAsync(Guid tenantId, Guid sheetId, CancellationToken cancellationToken = default);

    Task<TechnicalSheetSearchResult> SearchAsync(TechnicalSheetSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateProductCommand(Guid TenantId, string Name, string Sku, string? Description, Guid RequestedByUserId);

public sealed record CreateTechnicalSheetCommand(Guid TenantId, Guid ProductId, string Title, Guid RequestedByUserId);

public sealed record CreateTechnicalSheetVersionCommand(Guid TenantId, Guid TechnicalSheetId, string ChangeSummary, Guid RequestedByUserId);

public sealed record AddIngredientCommand(Guid TenantId, Guid TechnicalSheetId, string Name, decimal Percentage, string? Allergen, Guid RequestedByUserId);

public sealed record AddNutrientCommand(Guid TenantId, Guid TechnicalSheetId, string Name, decimal Amount, string Unit, Guid RequestedByUserId);

public sealed record AddCertificationCommand(Guid TenantId, Guid TechnicalSheetId, string Name, string Issuer, DateTimeOffset ExpiresAtUtc, Guid RequestedByUserId);

public sealed record TechnicalSheetActionCommand(Guid TenantId, Guid TechnicalSheetId, Guid RequestedByUserId);

public sealed record DecideTechnicalSheetCommand(Guid TenantId, Guid TechnicalSheetId, TechnicalSheetApprovalDecision Decision, string Comments, Guid RequestedByUserId);

public sealed record AttachTechnicalSheetPdfCommand(Guid TenantId, Guid TechnicalSheetId, string PdfObjectKey, Guid RequestedByUserId);

public sealed record TechnicalSheetSearchQuery(Guid TenantId, string? SearchText, TechnicalSheetStatus? Status, Guid? ProductId, int Page, int PageSize);

public sealed record TechnicalSheetSearchCriteria(Guid TenantId, string? SearchText, TechnicalSheetStatus? Status, Guid? ProductId, int Page, int PageSize);

public sealed record ProductSummary(Guid Id, Guid TenantId, string Name, string Sku, bool IsActive);

public sealed record TechnicalSheetSummary(Guid Id, Guid TenantId, Guid ProductId, string Title, TechnicalSheetStatus Status, int CurrentVersionNumber, string? PdfObjectKey);

public sealed record TechnicalSheetVersionSummary(Guid Id, Guid TechnicalSheetId, int VersionNumber, string ChangeSummary);

public sealed record TechnicalSheetApprovalSummary(Guid Id, Guid TechnicalSheetId, int VersionNumber, TechnicalSheetApprovalDecision Decision, string Comments);

public sealed record TechnicalSheetSearchResult(IReadOnlyCollection<TechnicalSheetSummary> Items, int TotalCount, int Page, int PageSize);
