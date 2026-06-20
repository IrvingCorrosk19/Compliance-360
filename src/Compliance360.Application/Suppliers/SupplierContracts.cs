using Compliance360.Domain.Audit;
using Compliance360.Domain.Suppliers;
using Compliance360.Shared;

namespace Compliance360.Application.Suppliers;

public interface ISupplierManagementService
{
    Task<Result<SupplierSummary>> CreateSupplierAsync(CreateSupplierCommand command, CancellationToken cancellationToken = default);

    Task<Result<SupplierDocumentSummary>> AddDocumentAsync(AddSupplierDocumentCommand command, CancellationToken cancellationToken = default);

    Task<Result> ValidateDocumentAsync(ReviewSupplierDocumentCommand command, CancellationToken cancellationToken = default);

    Task<Result> RejectDocumentAsync(RejectSupplierDocumentCommand command, CancellationToken cancellationToken = default);

    Task<Result<SupplierEvaluationSummary>> AddEvaluationAsync(AddSupplierEvaluationCommand command, CancellationToken cancellationToken = default);

    Task<Result> HomologateAsync(SupplierActionCommand command, CancellationToken cancellationToken = default);

    Task<Result<SupplierExpirationAlertSummary>> CreateExpirationAlertAsync(CreateSupplierExpirationAlertCommand command, CancellationToken cancellationToken = default);

    Task<Result> SuspendAsync(SuspendSupplierCommand command, CancellationToken cancellationToken = default);

    Task<Result<SupplierSearchResult>> SearchAsync(SupplierSearchQuery query, CancellationToken cancellationToken = default);
}

public interface ISupplierRepository
{
    Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);

    Task<Supplier?> GetAsync(Guid tenantId, Guid supplierId, CancellationToken cancellationToken = default);

    Task<bool> TaxIdentifierExistsAsync(Guid tenantId, string taxIdentifier, CancellationToken cancellationToken = default);

    Task<SupplierSearchResult> SearchAsync(SupplierSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateSupplierCommand(Guid TenantId, string LegalName, string TaxIdentifier, string CountryCode, Guid RequestedByUserId);

public sealed record AddSupplierDocumentCommand(Guid TenantId, Guid SupplierId, SupplierDocumentType Type, string DocumentNumber, Guid StoredFileId, DateTimeOffset IssuedAtUtc, DateTimeOffset ExpiresAtUtc, Guid RequestedByUserId);

public sealed record ReviewSupplierDocumentCommand(Guid TenantId, Guid SupplierId, Guid SupplierDocumentId, Guid RequestedByUserId);

public sealed record RejectSupplierDocumentCommand(Guid TenantId, Guid SupplierId, Guid SupplierDocumentId, string Reason, Guid RequestedByUserId);

public sealed record AddSupplierEvaluationCommand(Guid TenantId, Guid SupplierId, int Score, string Comments, Guid RequestedByUserId);

public sealed record SupplierActionCommand(Guid TenantId, Guid SupplierId, Guid RequestedByUserId);

public sealed record CreateSupplierExpirationAlertCommand(Guid TenantId, Guid SupplierId, Guid SupplierDocumentId, Guid RequestedByUserId);

public sealed record SuspendSupplierCommand(Guid TenantId, Guid SupplierId, string Reason, Guid RequestedByUserId);

public sealed record SupplierSearchQuery(Guid TenantId, string? SearchText, SupplierStatus? Status, int Page, int PageSize);

public sealed record SupplierSearchCriteria(Guid TenantId, string? SearchText, SupplierStatus? Status, int Page, int PageSize);

public sealed record SupplierSummary(Guid Id, Guid TenantId, string LegalName, string TaxIdentifier, string CountryCode, SupplierStatus Status, DateTimeOffset? HomologatedAtUtc);

public sealed record SupplierDocumentSummary(Guid Id, Guid SupplierId, SupplierDocumentType Type, string DocumentNumber, SupplierDocumentStatus Status, DateTimeOffset ExpiresAtUtc);

public sealed record SupplierEvaluationSummary(Guid Id, Guid SupplierId, int Score, string Comments);

public sealed record SupplierExpirationAlertSummary(Guid Id, Guid SupplierId, Guid SupplierDocumentId, SupplierDocumentType DocumentType, SupplierAlertStatus Status, DateTimeOffset ExpiresAtUtc);

public sealed record SupplierSearchResult(IReadOnlyCollection<SupplierSummary> Items, int TotalCount, int Page, int PageSize);
