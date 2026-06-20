using Compliance360.Domain.Common;

namespace Compliance360.Domain.Documents;

public enum DocumentStatus
{
    Draft = 0,
    InReview = 1,
    Approved = 2,
    Rejected = 3,
    Obsolete = 4,
    Archived = 5
}

public enum DocumentApprovalDecision
{
    Approved = 0,
    Rejected = 1
}

public enum DocumentPermissionLevel
{
    Read = 0,
    Review = 1,
    Approve = 2,
    Manage = 3
}

public sealed class DocumentType : TenantEntity
{
    private DocumentType()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public DocumentType(Guid tenantId, string name, string code, int retentionDays)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 160);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 80).ToUpperInvariant();
        RetentionDays = Guard.AgainstOutOfRange(retentionDays, nameof(retentionDays), 30, 18_250);
        IsActive = true;
    }

    public string Name { get; private set; }

    public string Code { get; private set; }

    public int RetentionDays { get; private set; }

    public bool IsActive { get; private set; }
}

public sealed class DocumentCategory : TenantEntity
{
    private DocumentCategory()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public DocumentCategory(Guid tenantId, string name, string code)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 160);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 80).ToUpperInvariant();
        IsActive = true;
    }

    public string Name { get; private set; }

    public string Code { get; private set; }

    public bool IsActive { get; private set; }
}

public sealed class Document : TenantEntity
{
    private readonly List<DocumentVersion> _versions = [];
    private readonly List<DocumentApproval> _approvals = [];
    private readonly List<DocumentHistory> _history = [];
    private readonly List<DocumentPermission> _permissions = [];

    private Document()
    {
        Title = string.Empty;
        Code = string.Empty;
    }

    public Document(Guid tenantId, Guid documentTypeId, Guid categoryId, string title, string code, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        DocumentTypeId = Guard.AgainstEmpty(documentTypeId, nameof(documentTypeId));
        CategoryId = Guard.AgainstEmpty(categoryId, nameof(categoryId));
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title), 220);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 120).ToUpperInvariant();
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        Status = DocumentStatus.Draft;
        ReviewCycleMonths = 12;
        AddHistory("Document created.", createdByUserId, createdAtUtc);
    }

    public Guid DocumentTypeId { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Title { get; private set; }

    public string Code { get; private set; }

    public DocumentStatus Status { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public Guid? CurrentVersionId { get; private set; }

    public DateTimeOffset? ApprovedAtUtc { get; private set; }

    public DateTimeOffset? ExpiresAtUtc { get; private set; }

    public int ReviewCycleMonths { get; private set; }

    public IReadOnlyCollection<DocumentVersion> Versions => _versions.AsReadOnly();

    public IReadOnlyCollection<DocumentApproval> Approvals => _approvals.AsReadOnly();

    public IReadOnlyCollection<DocumentHistory> History => _history.AsReadOnly();

    public IReadOnlyCollection<DocumentPermission> Permissions => _permissions.AsReadOnly();

    public DocumentVersion AddVersion(string changeSummary, Guid storedFileId, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        if (Status is DocumentStatus.Obsolete or DocumentStatus.Archived)
        {
            throw new DomainException("Obsolete or archived documents cannot receive new versions.");
        }

        var versionNumber = _versions.Count == 0 ? 1 : _versions.Max(version => version.VersionNumber) + 1;
        var version = new DocumentVersion(TenantId, Id, versionNumber, storedFileId, changeSummary, createdByUserId, createdAtUtc);
        _versions.Add(version);
        CurrentVersionId = version.Id;
        Status = DocumentStatus.Draft;
        AddHistory($"Version {versionNumber} created.", createdByUserId, createdAtUtc);
        return version;
    }

    public void SubmitForReview(Guid requestedByUserId, DateTimeOffset submittedAtUtc)
    {
        if (CurrentVersionId is null)
        {
            throw new DomainException("Document requires at least one version before review.");
        }

        Status = DocumentStatus.InReview;
        AddHistory("Document submitted for review.", requestedByUserId, submittedAtUtc);
    }

    public DocumentApproval Decide(DocumentApprovalDecision decision, string comments, Guid decidedByUserId, DateTimeOffset decidedAtUtc)
    {
        if (Status != DocumentStatus.InReview)
        {
            throw new DomainException("Only documents in review can receive approval decisions.");
        }

        var approval = new DocumentApproval(TenantId, Id, CurrentVersionId!.Value, decision, comments, decidedByUserId, decidedAtUtc);
        _approvals.Add(approval);

        if (decision == DocumentApprovalDecision.Approved)
        {
            Status = DocumentStatus.Approved;
            ApprovedAtUtc = decidedAtUtc;
            ExpiresAtUtc = decidedAtUtc.AddMonths(ReviewCycleMonths);
            AddHistory("Document approved.", decidedByUserId, decidedAtUtc);
        }
        else
        {
            Status = DocumentStatus.Rejected;
            AddHistory("Document rejected.", decidedByUserId, decidedAtUtc);
        }

        return approval;
    }

    public void MarkObsolete(Guid requestedByUserId, DateTimeOffset obsoleteAtUtc)
    {
        Status = DocumentStatus.Obsolete;
        AddHistory("Document marked obsolete.", requestedByUserId, obsoleteAtUtc);
    }

    public void GrantPermission(Guid principalId, DocumentPermissionLevel level, Guid grantedByUserId, DateTimeOffset grantedAtUtc)
    {
        if (_permissions.Any(permission => permission.PrincipalId == principalId && permission.Level == level))
        {
            return;
        }

        _permissions.Add(new DocumentPermission(TenantId, Id, principalId, level, grantedByUserId, grantedAtUtc));
    }

    private void AddHistory(string action, Guid userId, DateTimeOffset occurredAtUtc)
    {
        _history.Add(new DocumentHistory(TenantId, Id, action, userId, occurredAtUtc));
    }
}

public sealed class DocumentVersion : TenantEntity
{
    private DocumentVersion()
    {
        ChangeSummary = string.Empty;
    }

    public DocumentVersion(Guid tenantId, Guid documentId, int versionNumber, Guid storedFileId, string changeSummary, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        DocumentId = Guard.AgainstEmpty(documentId, nameof(documentId));
        VersionNumber = Guard.AgainstOutOfRange(versionNumber, nameof(versionNumber), 1, 10_000);
        StoredFileId = Guard.AgainstEmpty(storedFileId, nameof(storedFileId));
        ChangeSummary = Guard.AgainstNullOrWhiteSpace(changeSummary, nameof(changeSummary), 1_000);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
    }

    public Guid DocumentId { get; private set; }

    public int VersionNumber { get; private set; }

    public Guid StoredFileId { get; private set; }

    public string ChangeSummary { get; private set; }

    public Guid CreatedByUserId { get; private set; }

}

public sealed class DocumentApproval : TenantEntity
{
    private DocumentApproval()
    {
        Comments = string.Empty;
    }

    public DocumentApproval(Guid tenantId, Guid documentId, Guid documentVersionId, DocumentApprovalDecision decision, string comments, Guid decidedByUserId, DateTimeOffset decidedAtUtc)
        : base(tenantId)
    {
        DocumentId = Guard.AgainstEmpty(documentId, nameof(documentId));
        DocumentVersionId = Guard.AgainstEmpty(documentVersionId, nameof(documentVersionId));
        Decision = decision;
        Comments = Guard.AgainstNullOrWhiteSpace(comments, nameof(comments), 1_000);
        DecidedByUserId = Guard.AgainstEmpty(decidedByUserId, nameof(decidedByUserId));
        DecidedAtUtc = decidedAtUtc;
    }

    public Guid DocumentId { get; private set; }

    public Guid DocumentVersionId { get; private set; }

    public DocumentApprovalDecision Decision { get; private set; }

    public string Comments { get; private set; }

    public Guid DecidedByUserId { get; private set; }

    public DateTimeOffset DecidedAtUtc { get; private set; }
}

public sealed class DocumentHistory : TenantEntity
{
    private DocumentHistory()
    {
        Action = string.Empty;
    }

    public DocumentHistory(Guid tenantId, Guid documentId, string action, Guid userId, DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        DocumentId = Guard.AgainstEmpty(documentId, nameof(documentId));
        Action = Guard.AgainstNullOrWhiteSpace(action, nameof(action), 500);
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid DocumentId { get; private set; }

    public string Action { get; private set; }

    public Guid UserId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }
}

public sealed class DocumentPermission : TenantEntity
{
    private DocumentPermission()
    {
    }

    public DocumentPermission(Guid tenantId, Guid documentId, Guid principalId, DocumentPermissionLevel level, Guid grantedByUserId, DateTimeOffset grantedAtUtc)
        : base(tenantId)
    {
        DocumentId = Guard.AgainstEmpty(documentId, nameof(documentId));
        PrincipalId = Guard.AgainstEmpty(principalId, nameof(principalId));
        Level = level;
        GrantedByUserId = Guard.AgainstEmpty(grantedByUserId, nameof(grantedByUserId));
        GrantedAtUtc = grantedAtUtc;
    }

    public Guid DocumentId { get; private set; }

    public Guid PrincipalId { get; private set; }

    public DocumentPermissionLevel Level { get; private set; }

    public Guid GrantedByUserId { get; private set; }

    public DateTimeOffset GrantedAtUtc { get; private set; }
}
