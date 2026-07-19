using Compliance360.Domain.Common;

namespace Compliance360.Domain.RegulatoryAffairs;

public enum DossierCorrectionSeverity { Low, Medium, High, Critical }
public enum DossierCorrectionStatus { Open, ResponseSubmitted, Closed, Cancelled }
public enum DossierEvidenceRevisionStatus { Active, Superseded, Voided }
public enum DossierApprovalStage { First, Second }
public enum DossierGovernanceRequestStatus { Pending, Approved, Rejected, Executed }

public sealed class DossierCorrectionRequest : TenantEntity
{
    private readonly List<DossierCorrectionScopeItem> _scopeItems = [];
    private DossierCorrectionRequest() { Reason = string.Empty; }

    public DossierCorrectionRequest(Guid tenantId, Guid dossierId, string reason, DossierCorrectionSeverity severity, Guid requestedByUserId, DateTimeOffset requestedAtUtc)
        : base(tenantId)
    {
        DossierId = Guard.AgainstEmpty(dossierId, nameof(dossierId));
        Reason = RequiredReason(reason);
        Severity = severity;
        RequestedByUserId = Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
        RequestedAtUtc = requestedAtUtc;
        Status = DossierCorrectionStatus.Open;
    }

    public Guid DossierId { get; private set; }
    public string Reason { get; private set; }
    public DossierCorrectionSeverity Severity { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public DossierCorrectionStatus Status { get; private set; }
    public DateTimeOffset? SubmittedAtUtc { get; private set; }
    public IReadOnlyCollection<DossierCorrectionScopeItem> ScopeItems => _scopeItems.AsReadOnly();

    public void AddRequirement(Guid requirementId) => AddScope("Requirement", requirementId, null);
    public void AddDocument(Guid documentId) => AddScope("Document", documentId, null);
    public void AddField(string fieldPath) => AddScope("Field", null, Guard.AgainstNullOrWhiteSpace(fieldPath, nameof(fieldPath), 300));

    public bool IncludesRequirement(Guid id) => _scopeItems.Any(x => x.ScopeType == "Requirement" && x.TargetId == id);
    public bool IncludesDocument(Guid id) => _scopeItems.Any(x => x.ScopeType == "Document" && x.TargetId == id);
    public bool IncludesField(string path) => _scopeItems.Any(x => x.ScopeType == "Field" && string.Equals(x.FieldPath, path, StringComparison.OrdinalIgnoreCase));

    public void ValidateHasScope()
    {
        if (_scopeItems.Count == 0) throw new DomainException("Correction request requires at least one scope item.");
    }

    public void MarkResponseSubmitted(DateTimeOffset now)
    {
        if (Status != DossierCorrectionStatus.Open) throw new DomainException("Correction request is not open.");
        Status = DossierCorrectionStatus.ResponseSubmitted;
        SubmittedAtUtc = now;
    }

    public void Close()
    {
        if (Status != DossierCorrectionStatus.ResponseSubmitted)
            throw new DomainException("Only a submitted correction can be closed by technical review.");
        Status = DossierCorrectionStatus.Closed;
    }

    private void AddScope(string type, Guid? targetId, string? fieldPath)
    {
        if (Status != DossierCorrectionStatus.Open) throw new DomainException("Cannot change scope after correction submission.");
        if (_scopeItems.Any(x => x.ScopeType == type && x.TargetId == targetId && x.FieldPath == fieldPath)) return;
        _scopeItems.Add(new DossierCorrectionScopeItem(TenantId, Id, type, targetId, fieldPath));
    }

    internal static string RequiredReason(string value)
    {
        var reason = Guard.AgainstNullOrWhiteSpace(value, nameof(value), 2000);
        if (reason.Length < 8) throw new DomainException("Reason must contain at least 8 characters.");
        return reason;
    }
}

public sealed class DossierCorrectionScopeItem : TenantEntity
{
    private DossierCorrectionScopeItem() { ScopeType = string.Empty; }
    internal DossierCorrectionScopeItem(Guid tenantId, Guid correctionRequestId, string scopeType, Guid? targetId, string? fieldPath) : base(tenantId)
    {
        CorrectionRequestId = correctionRequestId;
        ScopeType = scopeType;
        TargetId = targetId;
        FieldPath = fieldPath;
    }
    public Guid CorrectionRequestId { get; private set; }
    public string ScopeType { get; private set; }
    public Guid? TargetId { get; private set; }
    public string? FieldPath { get; private set; }
}

public sealed class DossierEvidenceRevision : TenantEntity
{
    private DossierEvidenceRevision() { Sha256 = FileName = Reason = string.Empty; }
    public DossierEvidenceRevision(Guid tenantId, Guid dossierId, Guid requirementId, Guid? correctionRequestId, Guid? documentId,
        Guid storedFileId, string sha256, string fileName, string reason, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc, int versionNumber)
        : base(tenantId)
    {
        DossierId = dossierId;
        RequirementId = requirementId;
        CorrectionRequestId = correctionRequestId;
        DocumentId = documentId;
        StoredFileId = Guard.AgainstEmpty(storedFileId, nameof(storedFileId));
        Sha256 = Guard.AgainstNullOrWhiteSpace(sha256, nameof(sha256), 128).ToLowerInvariant();
        if (Sha256.Length != 64 || Sha256.Any(c => !Uri.IsHexDigit(c))) throw new DomainException("A valid SHA-256 hash is required.");
        FileName = Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName), 300);
        if (versionNumber < 1) throw new DomainException("Evidence version must be positive.");
        if (versionNumber > 1 && string.IsNullOrWhiteSpace(reason)) throw new DomainException("Replacement evidence requires a reason.");
        Reason = string.IsNullOrWhiteSpace(reason) ? "Initial evidence" : reason.Trim();
        UploadedByUserId = Guard.AgainstEmpty(uploadedByUserId, nameof(uploadedByUserId));
        UploadedAtUtc = uploadedAtUtc;
        VersionNumber = versionNumber;
        IsCurrent = true;
        Status = DossierEvidenceRevisionStatus.Active;
    }
    public Guid DossierId { get; private set; }
    public Guid RequirementId { get; private set; }
    public Guid? CorrectionRequestId { get; private set; }
    public Guid? DocumentId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public string Sha256 { get; private set; }
    public string FileName { get; private set; }
    public string Reason { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
    public int VersionNumber { get; private set; }
    public bool IsCurrent { get; private set; }
    public DossierEvidenceRevisionStatus Status { get; private set; }
    public void Supersede() { IsCurrent = false; Status = DossierEvidenceRevisionStatus.Superseded; }
    public void Void() { IsCurrent = false; Status = DossierEvidenceRevisionStatus.Voided; }
}

public sealed class DossierReopenRequest : TenantEntity
{
    private readonly List<DossierReopenApproval> _approvals = [];
    private DossierReopenRequest() { Reason = string.Empty; }
    public DossierReopenRequest(Guid tenantId, Guid dossierId, string reason, Guid requesterUserId, DateTimeOffset requestedAtUtc) : base(tenantId)
    {
        DossierId = dossierId; Reason = DossierCorrectionRequest.RequiredReason(reason); RequesterUserId = requesterUserId;
        RequestedAtUtc = requestedAtUtc; Status = DossierGovernanceRequestStatus.Pending;
    }
    public Guid DossierId { get; private set; }
    public string Reason { get; private set; }
    public Guid RequesterUserId { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public DossierGovernanceRequestStatus Status { get; private set; }
    public IReadOnlyCollection<DossierReopenApproval> Approvals => _approvals.AsReadOnly();
    public void Approve(Guid actor, DateTimeOffset now)
    {
        EnsurePending();
        if (actor == RequesterUserId || _approvals.Any(x => x.ApproverUserId == actor)) throw new DomainException("Requester and prior approvers cannot approve this request.");
        var stage = _approvals.Count == 0 ? DossierApprovalStage.First : DossierApprovalStage.Second;
        _approvals.Add(new DossierReopenApproval(TenantId, Id, stage, actor, now));
        if (_approvals.Count == 2) Status = DossierGovernanceRequestStatus.Approved;
    }
    public void Reject(Guid actor, string reason, DateTimeOffset now) { EnsurePending(); Status = DossierGovernanceRequestStatus.Rejected; RejectedByUserId = actor; RejectionReason = DossierCorrectionRequest.RequiredReason(reason); DecidedAtUtc = now; }
    public void Execute(DateTimeOffset now) { if (Status != DossierGovernanceRequestStatus.Approved || _approvals.Count != 2) throw new DomainException("Two approvals are required."); Status = DossierGovernanceRequestStatus.Executed; DecidedAtUtc = now; }
    public Guid? RejectedByUserId { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    private void EnsurePending() { if (Status != DossierGovernanceRequestStatus.Pending) throw new DomainException("Request is no longer pending."); }
}

public sealed class DossierReopenApproval : TenantEntity
{
    private DossierReopenApproval() { }
    internal DossierReopenApproval(Guid tenantId, Guid requestId, DossierApprovalStage stage, Guid approver, DateTimeOffset at) : base(tenantId)
    { ReopenRequestId = requestId; Stage = stage; ApproverUserId = approver; ApprovedAtUtc = at; }
    public Guid ReopenRequestId { get; private set; }
    public DossierApprovalStage Stage { get; private set; }
    public Guid ApproverUserId { get; private set; }
    public DateTimeOffset ApprovedAtUtc { get; private set; }
}

public sealed class DossierOverrideRequest : TenantEntity
{
    private readonly List<DossierOverrideApproval> _approvals = [];
    private DossierOverrideRequest() { Reason = Action = string.Empty; }
    public DossierOverrideRequest(Guid tenantId, Guid dossierId, string action, string reason, Guid requester, DateTimeOffset at) : base(tenantId)
    { DossierId = dossierId; Action = Guard.AgainstNullOrWhiteSpace(action, nameof(action), 120); Reason = DossierCorrectionRequest.RequiredReason(reason); RequesterUserId = requester; RequestedAtUtc = at; Status = DossierGovernanceRequestStatus.Pending; }
    public Guid DossierId { get; private set; }
    public string Action { get; private set; }
    public string Reason { get; private set; }
    public Guid RequesterUserId { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public DossierGovernanceRequestStatus Status { get; private set; }
    public DateTimeOffset? ConsumedAtUtc { get; private set; }
    public Guid? ConsumedByUserId { get; private set; }
    public IReadOnlyCollection<DossierOverrideApproval> Approvals => _approvals.AsReadOnly();
    public void Approve(Guid actor, DateTimeOffset now)
    {
        if (Status != DossierGovernanceRequestStatus.Pending) throw new DomainException("Request is no longer pending.");
        if (actor == RequesterUserId || _approvals.Any(x => x.ApproverUserId == actor)) throw new DomainException("Requester and prior approvers cannot approve this request.");
        _approvals.Add(new DossierOverrideApproval(TenantId, Id, _approvals.Count == 0 ? DossierApprovalStage.First : DossierApprovalStage.Second, actor, now));
        if (_approvals.Count == 2) Status = DossierGovernanceRequestStatus.Approved;
    }
    public void Reject(Guid actor, string reason, DateTimeOffset now) { if (Status != DossierGovernanceRequestStatus.Pending) throw new DomainException("Request is no longer pending."); Status = DossierGovernanceRequestStatus.Rejected; RejectedByUserId = actor; RejectionReason = DossierCorrectionRequest.RequiredReason(reason); DecidedAtUtc = now; }
    public void Consume(Guid actor, string action, DateTimeOffset now)
    {
        if (Status != DossierGovernanceRequestStatus.Approved || _approvals.Count != 2) throw new DomainException("Approved two-stage override is required.");
        if (ConsumedAtUtc.HasValue) throw new DomainException("Override was already consumed.");
        if (!string.Equals(Action, action, StringComparison.Ordinal)) throw new DomainException("Override action does not match.");
        ConsumedByUserId = actor; ConsumedAtUtc = now; Status = DossierGovernanceRequestStatus.Executed;
    }
    public Guid? RejectedByUserId { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
}

public sealed class DossierOverrideApproval : TenantEntity
{
    private DossierOverrideApproval() { }
    internal DossierOverrideApproval(Guid tenantId, Guid requestId, DossierApprovalStage stage, Guid approver, DateTimeOffset at) : base(tenantId)
    { OverrideRequestId = requestId; Stage = stage; ApproverUserId = approver; ApprovedAtUtc = at; }
    public Guid OverrideRequestId { get; private set; }
    public DossierApprovalStage Stage { get; private set; }
    public Guid ApproverUserId { get; private set; }
    public DateTimeOffset ApprovedAtUtc { get; private set; }
}

public sealed class DossierChangeEvent : TenantEntity
{
    private DossierChangeEvent() { EventType = Field = Reason = CorrelationId = string.Empty; }
    public DossierChangeEvent(Guid tenantId, Guid dossierId, long sequence, string eventType, Guid actorUserId, string? actorRole,
        RegistrationDossierStatus? fromStatus, RegistrationDossierStatus? toStatus, string field, string? beforeJson, string? afterJson,
        string reason, string correlationId, DateTimeOffset occurredAtUtc) : base(tenantId)
    {
        DossierId = dossierId; Sequence = sequence; EventType = Guard.AgainstNullOrWhiteSpace(eventType, nameof(eventType), 100);
        ActorUserId = actorUserId; ActorRole = actorRole; FromStatus = fromStatus; ToStatus = toStatus;
        Field = Guard.AgainstNullOrWhiteSpace(field, nameof(field), 200); BeforeJson = beforeJson; AfterJson = afterJson;
        Reason = DossierCorrectionRequest.RequiredReason(reason); CorrelationId = Guard.AgainstNullOrWhiteSpace(correlationId, nameof(correlationId), 100);
        OccurredAtUtc = occurredAtUtc;
    }
    public Guid DossierId { get; private set; }
    public long Sequence { get; private set; }
    public string EventType { get; private set; }
    public Guid ActorUserId { get; private set; }
    public string? ActorRole { get; private set; }
    public RegistrationDossierStatus? FromStatus { get; private set; }
    public RegistrationDossierStatus? ToStatus { get; private set; }
    public string Field { get; private set; }
    public string? BeforeJson { get; private set; }
    public string? AfterJson { get; private set; }
    public string Reason { get; private set; }
    public string CorrelationId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
