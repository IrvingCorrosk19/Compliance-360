using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Documents;
using Compliance360.Shared;
using Microsoft.Extensions.Options;

namespace Compliance360.Application.Documents;

public sealed class DocumentManagementService : IDocumentManagementService
{
    private readonly IDocumentRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;
    private readonly DocumentManagementOptions _options;

    public DocumentManagementService(
        IDocumentRepository repository,
        IApplicationDbContext dbContext,
        IClock clock,
        IOptions<DocumentManagementOptions> options)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<Result<DocumentTypeSummary>> CreateTypeAsync(CreateDocumentTypeCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var documentType = new DocumentType(command.TenantId, command.Name, command.Code, command.RetentionDays);
            await _repository.AddTypeAsync(documentType, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, documentType.Id, AuditAction.ConfigurationChanged, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<DocumentTypeSummary>.Success(new DocumentTypeSummary(documentType.Id, documentType.TenantId, documentType.Name, documentType.Code, documentType.RetentionDays));
        }
        catch (DomainException exception)
        {
            return Result<DocumentTypeSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<DocumentCategorySummary>> CreateCategoryAsync(CreateDocumentCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = new DocumentCategory(command.TenantId, command.Name, command.Code);
            await _repository.AddCategoryAsync(category, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, category.Id, AuditAction.ConfigurationChanged, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<DocumentCategorySummary>.Success(new DocumentCategorySummary(category.Id, category.TenantId, category.Name, category.Code));
        }
        catch (DomainException exception)
        {
            return Result<DocumentCategorySummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<DocumentSummary>> CreateDocumentAsync(CreateDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.GetTypeAsync(command.TenantId, command.DocumentTypeId, cancellationToken) is null)
            {
                return Result<DocumentSummary>.Failure("Document type not found.");
            }

            if (await _repository.GetCategoryAsync(command.TenantId, command.CategoryId, cancellationToken) is null)
            {
                return Result<DocumentSummary>.Failure("Document category not found.");
            }

            var normalizedCode = Guard.AgainstNullOrWhiteSpace(command.Code, nameof(command.Code), 120).ToUpperInvariant();
            if (await _repository.DocumentCodeExistsAsync(command.TenantId, normalizedCode, cancellationToken))
            {
                return Result<DocumentSummary>.Failure("Document code already exists.");
            }

            var document = new Document(command.TenantId, command.DocumentTypeId, command.CategoryId, command.Title, normalizedCode, command.RequestedByUserId, _clock.UtcNow);
            await _repository.AddDocumentAsync(document, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, document.Id, AuditAction.DocumentCreated, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<DocumentSummary>.Success(ToSummary(document));
        }
        catch (DomainException exception)
        {
            return Result<DocumentSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<DocumentVersionSummary>> AddVersionAsync(AddDocumentVersionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _repository.GetDocumentAsync(command.TenantId, command.DocumentId, cancellationToken);
            if (document is null)
            {
                return Result<DocumentVersionSummary>.Failure("Document not found.");
            }

            var version = document.AddVersion(command.ChangeSummary, command.StoredFileId, command.RequestedByUserId, _clock.UtcNow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, document.Id, AuditAction.DocumentUpdated, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<DocumentVersionSummary>.Success(new DocumentVersionSummary(version.Id, version.DocumentId, version.VersionNumber, version.StoredFileId, version.ChangeSummary));
        }
        catch (DomainException exception)
        {
            return Result<DocumentVersionSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result> SubmitForReviewAsync(DocumentActionCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeDocumentAsync(command, document => document.SubmitForReview(command.RequestedByUserId, _clock.UtcNow), AuditAction.WorkflowStarted, cancellationToken);
    }

    public async Task<Result<DocumentApprovalSummary>> DecideAsync(DecideDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _repository.GetDocumentAsync(command.TenantId, command.DocumentId, cancellationToken);
            if (document is null)
            {
                return Result<DocumentApprovalSummary>.Failure("Document not found.");
            }

            var approval = document.Decide(command.Decision, command.Comments, command.RequestedByUserId, _clock.UtcNow);
            var action = command.Decision == DocumentApprovalDecision.Approved ? AuditAction.DocumentApproved : AuditAction.DocumentRejected;
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, document.Id, action, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<DocumentApprovalSummary>.Success(new DocumentApprovalSummary(approval.Id, approval.DocumentId, approval.DocumentVersionId, approval.Decision, approval.Comments));
        }
        catch (DomainException exception)
        {
            return Result<DocumentApprovalSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result> MarkObsoleteAsync(DocumentActionCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeDocumentAsync(command, document => document.MarkObsolete(command.RequestedByUserId, _clock.UtcNow), AuditAction.DocumentUpdated, cancellationToken);
    }

    public async Task<Result> GrantPermissionAsync(GrantDocumentPermissionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _repository.GetDocumentAsync(command.TenantId, command.DocumentId, cancellationToken);
            if (document is null)
            {
                return Result.Failure("Document not found.");
            }

            document.GrantPermission(command.PrincipalId, command.Level, command.RequestedByUserId, _clock.UtcNow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, document.Id, AuditAction.PermissionChanged, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result<DocumentSearchResult>> SearchAsync(DocumentSearchQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, _options.MaxPageSize);
        return Result<DocumentSearchResult>.Success(await _repository.SearchAsync(
            new DocumentSearchCriteria(query.TenantId, query.SearchText, query.Status, query.DocumentTypeId, query.CategoryId, page, pageSize),
            cancellationToken));
    }

    private async Task<Result> ChangeDocumentAsync(DocumentActionCommand command, Action<Document> change, AuditAction action, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _repository.GetDocumentAsync(command.TenantId, command.DocumentId, cancellationToken);
            if (document is null)
            {
                return Result.Failure("Document not found.");
            }

            change(document);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, document.Id, action, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private async Task AppendAuditAsync(Guid tenantId, Guid userId, Guid documentId, AuditAction action, bool success, string? error, CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.FromEvent(
            new AuditEvent(
                nameof(Document),
                documentId,
                action,
                AuditLog.InferCategory(action),
                new AuditContext(tenantId, userId, null, null, null, null, null, null, null),
                new AuditSnapshot(null, null),
                new AuditMetadata("{\"source\":\"document-management\"}"),
                success,
                error),
            _clock.UtcNow);

        await _repository.AddAuditLogAsync(auditLog, cancellationToken);
    }

    private static DocumentSummary ToSummary(Document document)
    {
        return new DocumentSummary(
            document.Id,
            document.TenantId,
            document.DocumentTypeId,
            document.CategoryId,
            document.Title,
            document.Code,
            document.Status,
            document.CurrentVersionId,
            document.ApprovedAtUtc,
            document.ExpiresAtUtc);
    }
}

public sealed class DocumentManagementOptions
{
    public const string SectionName = "DocumentManagement";

    public int MaxPageSize { get; set; } = 200;
}
