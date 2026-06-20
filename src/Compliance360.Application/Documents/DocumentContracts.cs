using Compliance360.Domain.Audit;
using Compliance360.Domain.Documents;
using Compliance360.Shared;

namespace Compliance360.Application.Documents;

public interface IDocumentManagementService
{
    Task<Result<DocumentTypeSummary>> CreateTypeAsync(CreateDocumentTypeCommand command, CancellationToken cancellationToken = default);

    Task<Result<DocumentCategorySummary>> CreateCategoryAsync(CreateDocumentCategoryCommand command, CancellationToken cancellationToken = default);

    Task<Result<DocumentSummary>> CreateDocumentAsync(CreateDocumentCommand command, CancellationToken cancellationToken = default);

    Task<Result<DocumentVersionSummary>> AddVersionAsync(AddDocumentVersionCommand command, CancellationToken cancellationToken = default);

    Task<Result> SubmitForReviewAsync(DocumentActionCommand command, CancellationToken cancellationToken = default);

    Task<Result<DocumentApprovalSummary>> DecideAsync(DecideDocumentCommand command, CancellationToken cancellationToken = default);

    Task<Result> MarkObsoleteAsync(DocumentActionCommand command, CancellationToken cancellationToken = default);

    Task<Result> GrantPermissionAsync(GrantDocumentPermissionCommand command, CancellationToken cancellationToken = default);

    Task<Result<DocumentSearchResult>> SearchAsync(DocumentSearchQuery query, CancellationToken cancellationToken = default);
}

public interface IDocumentRepository
{
    Task AddDocumentAsync(Document document, CancellationToken cancellationToken = default);

    Task<Document?> GetDocumentAsync(Guid tenantId, Guid documentId, CancellationToken cancellationToken = default);

    Task<bool> DocumentCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);

    Task AddTypeAsync(DocumentType documentType, CancellationToken cancellationToken = default);

    Task<DocumentType?> GetTypeAsync(Guid tenantId, Guid documentTypeId, CancellationToken cancellationToken = default);

    Task AddCategoryAsync(DocumentCategory category, CancellationToken cancellationToken = default);

    Task<DocumentCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default);

    Task<DocumentSearchResult> SearchAsync(DocumentSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateDocumentCommand(Guid TenantId, Guid DocumentTypeId, Guid CategoryId, string Title, string Code, Guid RequestedByUserId);

public sealed record CreateDocumentTypeCommand(Guid TenantId, string Name, string Code, int RetentionDays, Guid RequestedByUserId);

public sealed record CreateDocumentCategoryCommand(Guid TenantId, string Name, string Code, Guid RequestedByUserId);

public sealed record AddDocumentVersionCommand(Guid TenantId, Guid DocumentId, Guid StoredFileId, string ChangeSummary, Guid RequestedByUserId);

public sealed record DocumentActionCommand(Guid TenantId, Guid DocumentId, Guid RequestedByUserId);

public sealed record DecideDocumentCommand(Guid TenantId, Guid DocumentId, DocumentApprovalDecision Decision, string Comments, Guid RequestedByUserId);

public sealed record GrantDocumentPermissionCommand(Guid TenantId, Guid DocumentId, Guid PrincipalId, DocumentPermissionLevel Level, Guid RequestedByUserId);

public sealed record DocumentSearchQuery(Guid TenantId, string? SearchText, DocumentStatus? Status, Guid? DocumentTypeId, Guid? CategoryId, int Page, int PageSize);

public sealed record DocumentSearchCriteria(Guid TenantId, string? SearchText, DocumentStatus? Status, Guid? DocumentTypeId, Guid? CategoryId, int Page, int PageSize);

public sealed record DocumentSearchResult(IReadOnlyCollection<DocumentSummary> Items, int TotalCount, int Page, int PageSize);

public sealed record DocumentSummary(
    Guid Id,
    Guid TenantId,
    Guid DocumentTypeId,
    Guid CategoryId,
    string Title,
    string Code,
    DocumentStatus Status,
    Guid? CurrentVersionId,
    DateTimeOffset? ApprovedAtUtc,
    DateTimeOffset? ExpiresAtUtc);

public sealed record DocumentVersionSummary(Guid Id, Guid DocumentId, int VersionNumber, Guid StoredFileId, string ChangeSummary);

public sealed record DocumentApprovalSummary(Guid Id, Guid DocumentId, Guid DocumentVersionId, DocumentApprovalDecision Decision, string Comments);

public sealed record DocumentTypeSummary(Guid Id, Guid TenantId, string Name, string Code, int RetentionDays);

public sealed record DocumentCategorySummary(Guid Id, Guid TenantId, string Name, string Code);
