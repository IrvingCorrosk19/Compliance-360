using System.Diagnostics.CodeAnalysis;
using Compliance360.Domain.Common;

namespace Compliance360.Domain.CapaManagement;

public enum CapaStatus
{
    Draft = 0,
    Open = 1,
    InProgress = 2,
    PendingEffectiveness = 3,
    PendingApproval = 4,
    Closed = 5,
    Reopened = 6,
    Cancelled = 7
}

public enum CapaPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum CapaRiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum CapaActionType
{
    Containment = 0,
    Corrective = 1,
    Preventive = 2,
    FollowUp = 3
}

public enum CapaActionStatus
{
    Planned = 0,
    InProgress = 1,
    Completed = 2,
    Overdue = 3,
    Cancelled = 4
}

public enum CapaSourceType
{
    AuditFinding = 0,
    AuditNonConformity = 1,
    AuditRecommendation = 2,
    Supplier = 3,
    Document = 4,
    Manual = 5
}

public enum CapaRootCauseMethod
{
    FiveWhy = 0,
    Ishikawa = 1,
    Other = 2
}

public sealed class Capa : TenantEntity
{
    private readonly List<CapaOwner> _owners = [];
    private readonly List<CapaApprover> _approvers = [];
    private readonly List<CapaRootCause> _rootCauses = [];
    private readonly List<CapaCauseAnalysis> _causeAnalyses = [];
    private readonly List<CapaContainmentAction> _containmentActions = [];
    private readonly List<CapaCorrectiveAction> _correctiveActions = [];
    private readonly List<CapaPreventiveAction> _preventiveActions = [];
    private readonly List<CapaEffectivenessCheck> _effectivenessChecks = [];
    private readonly List<CapaEvidence> _evidence = [];
    private readonly List<CapaAttachment> _attachments = [];
    private readonly List<CapaHistory> _history = [];

    [ExcludeFromCodeCoverage]
    private Capa()
    {
        Title = string.Empty;
        Code = string.Empty;
        Description = string.Empty;
    }

    public Capa(
        Guid tenantId,
        string title,
        string code,
        string description,
        CapaPriority priority,
        CapaRiskLevel riskLevel,
        CapaSourceType sourceType,
        Guid? sourceEntityId,
        Guid? supplierId,
        Guid? documentId,
        Guid? auditId,
        Guid createdByUserId,
        DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title), 220);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 100).ToUpperInvariant();
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 2_000);
        Priority = priority;
        RiskLevel = riskLevel;
        SourceType = sourceType;
        SourceEntityId = sourceEntityId;
        SupplierId = supplierId;
        DocumentId = documentId;
        AuditId = auditId;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        Status = CapaStatus.Draft;
        CreatedAtUtc = createdAtUtc;
        AddHistory("CAPA created.", createdByUserId, createdAtUtc);
    }

    public string Title { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public CapaStatus Status { get; private set; }
    public CapaPriority Priority { get; private set; }
    public CapaRiskLevel RiskLevel { get; private set; }
    public CapaSourceType SourceType { get; private set; }
    public Guid? SourceEntityId { get; private set; }
    public Guid? SupplierId { get; private set; }
    public Guid? DocumentId { get; private set; }
    public Guid? AuditId { get; private set; }
    public Guid? WorkflowInstanceId { get; private set; }
    public DateTimeOffset? CommitmentDueAtUtc { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? ClosedByUserId { get; private set; }
    public IReadOnlyCollection<CapaOwner> Owners => _owners.AsReadOnly();
    public IReadOnlyCollection<CapaApprover> Approvers => _approvers.AsReadOnly();
    public IReadOnlyCollection<CapaRootCause> RootCauses => _rootCauses.AsReadOnly();
    public IReadOnlyCollection<CapaCauseAnalysis> CauseAnalyses => _causeAnalyses.AsReadOnly();
    public IReadOnlyCollection<CapaContainmentAction> ContainmentActions => _containmentActions.AsReadOnly();
    public IReadOnlyCollection<CapaCorrectiveAction> CorrectiveActions => _correctiveActions.AsReadOnly();
    public IReadOnlyCollection<CapaPreventiveAction> PreventiveActions => _preventiveActions.AsReadOnly();
    public IReadOnlyCollection<CapaEffectivenessCheck> EffectivenessChecks => _effectivenessChecks.AsReadOnly();
    public IReadOnlyCollection<CapaEvidence> Evidence => _evidence.AsReadOnly();
    public IReadOnlyCollection<CapaAttachment> Attachments => _attachments.AsReadOnly();
    public IReadOnlyCollection<CapaHistory> History => _history.AsReadOnly();

    public void Classify(CapaPriority priority, CapaRiskLevel riskLevel, DateTimeOffset? commitmentDueAtUtc, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        Priority = priority;
        RiskLevel = riskLevel;
        CommitmentDueAtUtc = commitmentDueAtUtc;
        Status = Status == CapaStatus.Draft ? CapaStatus.Open : Status;
        AddHistory("CAPA classified.", requestedByUserId, occurredAtUtc);
    }

    public CapaOwner AssignOwner(Guid ownerUserId, DateTimeOffset dueAtUtc, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (dueAtUtc <= occurredAtUtc)
        {
            throw new DomainException("CAPA owner due date must be in the future.");
        }

        var existing = _owners.FirstOrDefault(owner => owner.UserId == ownerUserId && owner.IsActive);
        if (existing is not null)
        {
            return existing;
        }

        var owner = new CapaOwner(TenantId, Id, ownerUserId, dueAtUtc);
        _owners.Add(owner);
        Status = CapaStatus.InProgress;
        AddHistory("CAPA owner assigned.", requestedByUserId, occurredAtUtc);
        return owner;
    }

    public CapaApprover AddApprover(Guid approverUserId, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        var existing = _approvers.FirstOrDefault(approver => approver.UserId == approverUserId);
        if (existing is not null)
        {
            return existing;
        }

        var approver = new CapaApprover(TenantId, Id, approverUserId);
        _approvers.Add(approver);
        AddHistory("CAPA approver assigned.", requestedByUserId, occurredAtUtc);
        return approver;
    }

    public CapaRootCause DefineRootCause(string description, CapaRootCauseMethod method, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        var rootCause = new CapaRootCause(TenantId, Id, description, method, requestedByUserId, occurredAtUtc);
        _rootCauses.Add(rootCause);
        AddHistory("Root cause defined.", requestedByUserId, occurredAtUtc);
        return rootCause;
    }

    public CapaCauseAnalysis AddFiveWhyAnalysis(string why1, string why2, string why3, string why4, string why5, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        var analysis = CapaCauseAnalysis.FiveWhy(TenantId, Id, why1, why2, why3, why4, why5, requestedByUserId, occurredAtUtc);
        _causeAnalyses.Add(analysis);
        AddHistory("5 Why analysis registered.", requestedByUserId, occurredAtUtc);
        return analysis;
    }

    public CapaCauseAnalysis AddIshikawaAnalysis(string people, string process, string equipment, string material, string environment, string measurement, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        var analysis = CapaCauseAnalysis.Ishikawa(TenantId, Id, people, process, equipment, material, environment, measurement, requestedByUserId, occurredAtUtc);
        _causeAnalyses.Add(analysis);
        AddHistory("Ishikawa analysis registered.", requestedByUserId, occurredAtUtc);
        return analysis;
    }

    public CapaContainmentAction AddContainmentAction(string description, Guid responsibleUserId, DateTimeOffset dueAtUtc, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        var action = new CapaContainmentAction(TenantId, Id, description, responsibleUserId, dueAtUtc);
        _containmentActions.Add(action);
        AddHistory("Containment action registered.", requestedByUserId, occurredAtUtc);
        return action;
    }

    public CapaCorrectiveAction AddCorrectiveAction(string description, Guid responsibleUserId, DateTimeOffset dueAtUtc, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        var action = new CapaCorrectiveAction(TenantId, Id, description, responsibleUserId, dueAtUtc);
        _correctiveActions.Add(action);
        AddHistory("Corrective action registered.", requestedByUserId, occurredAtUtc);
        return action;
    }

    public CapaPreventiveAction AddPreventiveAction(string description, Guid responsibleUserId, DateTimeOffset dueAtUtc, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        var action = new CapaPreventiveAction(TenantId, Id, description, responsibleUserId, dueAtUtc);
        _preventiveActions.Add(action);
        AddHistory("Preventive action registered.", requestedByUserId, occurredAtUtc);
        return action;
    }

    public CapaEvidence AddEvidence(Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
    {
        var evidence = new CapaEvidence(TenantId, Id, storedFileId, fileName, contentType, sizeBytes, sha256Hash, uploadedByUserId, uploadedAtUtc);
        _evidence.Add(evidence);
        AddHistory("CAPA evidence attached.", uploadedByUserId, uploadedAtUtc);
        return evidence;
    }

    public CapaAttachment AddAttachment(Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
    {
        var attachment = new CapaAttachment(TenantId, Id, storedFileId, fileName, contentType, sizeBytes, sha256Hash, uploadedByUserId, uploadedAtUtc);
        _attachments.Add(attachment);
        AddHistory("CAPA attachment added.", uploadedByUserId, uploadedAtUtc);
        return attachment;
    }

    public void RegisterFollowUp(string notes, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        Guard.AgainstNullOrWhiteSpace(notes, nameof(notes), 1_000);
        AddHistory($"Follow-up: {notes}", requestedByUserId, occurredAtUtc);
    }

    public void EscalateOverdue(Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (!IsOverdue(occurredAtUtc))
        {
            throw new DomainException("CAPA is not overdue.");
        }

        Priority = CapaPriority.Critical;
        RiskLevel = CapaRiskLevel.Critical;
        AddHistory("CAPA overdue escalation.", requestedByUserId, occurredAtUtc);
    }

    public CapaEffectivenessCheck VerifyEffectiveness(bool isEffective, string verificationSummary, Guid verifiedByUserId, DateTimeOffset verifiedAtUtc)
    {
        if (!_correctiveActions.Any(action => action.Status == CapaActionStatus.Completed) && !_preventiveActions.Any(action => action.Status == CapaActionStatus.Completed))
        {
            throw new DomainException("At least one corrective or preventive action must be completed before effectiveness verification.");
        }

        var check = new CapaEffectivenessCheck(TenantId, Id, isEffective, verificationSummary, verifiedByUserId, verifiedAtUtc);
        _effectivenessChecks.Add(check);
        Status = isEffective ? CapaStatus.PendingApproval : CapaStatus.InProgress;
        AddHistory("Effectiveness verified.", verifiedByUserId, verifiedAtUtc);
        return check;
    }

    public void AttachWorkflow(Guid workflowInstanceId, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        WorkflowInstanceId = Guard.AgainstEmpty(workflowInstanceId, nameof(workflowInstanceId));
        AddHistory("Workflow attached.", requestedByUserId, occurredAtUtc);
    }

    public void ApproveClosure(Guid approverUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != CapaStatus.PendingApproval)
        {
            throw new DomainException("CAPA must be pending approval before closure approval.");
        }

        if (_approvers.Count > 0 && _approvers.All(approver => approver.UserId != approverUserId))
        {
            throw new DomainException("User is not a CAPA approver.");
        }

        Status = CapaStatus.Closed;
        ClosedByUserId = approverUserId;
        ClosedAtUtc = occurredAtUtc;
        AddHistory("CAPA closure approved.", approverUserId, occurredAtUtc);
    }

    public void Reopen(string reason, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != CapaStatus.Closed)
        {
            throw new DomainException("Only closed CAPAs can be reopened.");
        }

        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 1_000);
        Status = CapaStatus.Reopened;
        ClosedAtUtc = null;
        ClosedByUserId = null;
        AddHistory($"CAPA reopened: {reason}", requestedByUserId, occurredAtUtc);
    }

    public bool IsOverdue(DateTimeOffset now)
    {
        return Status != CapaStatus.Closed && CommitmentDueAtUtc.HasValue && CommitmentDueAtUtc.Value < now;
    }

    public CapaDashboardContribution Dashboard(DateTimeOffset now)
    {
        var latestEffectiveness = _effectivenessChecks.OrderByDescending(check => check.VerifiedAtUtc).FirstOrDefault();
        return new CapaDashboardContribution(
            Status != CapaStatus.Closed && Status != CapaStatus.Cancelled ? 1 : 0,
            IsOverdue(now) ? 1 : 0,
            RiskLevel == CapaRiskLevel.Critical || Priority == CapaPriority.Critical ? 1 : 0,
            SupplierId.HasValue ? 1 : 0,
            AuditId.HasValue ? 1 : 0,
            Status == CapaStatus.Closed && ClosedAtUtc.HasValue ? Math.Max(0, (int)(ClosedAtUtc.Value - CreatedAtUtc).TotalDays) : 0,
            latestEffectiveness?.IsEffective == true ? 1 : 0,
            latestEffectiveness?.IsEffective == false ? 1 : 0);
    }

    private void AddHistory(string action, Guid userId, DateTimeOffset occurredAtUtc)
    {
        _history.Add(new CapaHistory(TenantId, Id, action, userId, occurredAtUtc));
    }
}

public sealed record CapaDashboardContribution(
    int Open,
    int Overdue,
    int Critical,
    int SupplierLinked,
    int AuditLinked,
    int ClosureDays,
    int Effective,
    int Recurrence);

public abstract class CapaAction : TenantEntity
{
    [ExcludeFromCodeCoverage]
    protected CapaAction()
    {
        Description = string.Empty;
    }

    protected CapaAction(Guid tenantId, Guid capaId, string description, Guid responsibleUserId, DateTimeOffset dueAtUtc, CapaActionType type)
        : base(tenantId)
    {
        CapaId = Guard.AgainstEmpty(capaId, nameof(capaId));
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 1_000);
        ResponsibleUserId = Guard.AgainstEmpty(responsibleUserId, nameof(responsibleUserId));
        DueAtUtc = dueAtUtc;
        Type = type;
        Status = CapaActionStatus.Planned;
    }

    public Guid CapaId { get; protected set; }
    public string Description { get; protected set; }
    public Guid ResponsibleUserId { get; protected set; }
    public DateTimeOffset DueAtUtc { get; protected set; }
    public CapaActionType Type { get; protected set; }
    public CapaActionStatus Status { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public void Start()
    {
        if (Status != CapaActionStatus.Planned)
        {
            throw new DomainException("Only planned CAPA actions can be started.");
        }

        Status = CapaActionStatus.InProgress;
    }

    public void Complete(DateTimeOffset completedAtUtc)
    {
        if (Status is CapaActionStatus.Cancelled or CapaActionStatus.Completed)
        {
            throw new DomainException("CAPA action cannot be completed from its current status.");
        }

        Status = CapaActionStatus.Completed;
        CompletedAtUtc = completedAtUtc;
    }

    public void MarkOverdue(DateTimeOffset now)
    {
        if (Status != CapaActionStatus.Completed && DueAtUtc < now)
        {
            Status = CapaActionStatus.Overdue;
        }
    }
}

public sealed class CapaContainmentAction : CapaAction
{
    [ExcludeFromCodeCoverage]
    private CapaContainmentAction()
    {
    }

    public CapaContainmentAction(Guid tenantId, Guid capaId, string description, Guid responsibleUserId, DateTimeOffset dueAtUtc)
        : base(tenantId, capaId, description, responsibleUserId, dueAtUtc, CapaActionType.Containment)
    {
    }
}

public sealed class CapaCorrectiveAction : CapaAction
{
    [ExcludeFromCodeCoverage]
    private CapaCorrectiveAction()
    {
    }

    public CapaCorrectiveAction(Guid tenantId, Guid capaId, string description, Guid responsibleUserId, DateTimeOffset dueAtUtc)
        : base(tenantId, capaId, description, responsibleUserId, dueAtUtc, CapaActionType.Corrective)
    {
    }
}

public sealed class CapaPreventiveAction : CapaAction
{
    [ExcludeFromCodeCoverage]
    private CapaPreventiveAction()
    {
    }

    public CapaPreventiveAction(Guid tenantId, Guid capaId, string description, Guid responsibleUserId, DateTimeOffset dueAtUtc)
        : base(tenantId, capaId, description, responsibleUserId, dueAtUtc, CapaActionType.Preventive)
    {
    }
}

public sealed class CapaRootCause : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private CapaRootCause()
    {
        Description = string.Empty;
    }

    public CapaRootCause(Guid tenantId, Guid capaId, string description, CapaRootCauseMethod method, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        CapaId = Guard.AgainstEmpty(capaId, nameof(capaId));
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 2_000);
        Method = method;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
    }

    public Guid CapaId { get; private set; }
    public string Description { get; private set; }
    public CapaRootCauseMethod Method { get; private set; }
    public Guid CreatedByUserId { get; private set; }
}

public sealed class CapaCauseAnalysis : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private CapaCauseAnalysis()
    {
        Method = CapaRootCauseMethod.Other;
    }

    private CapaCauseAnalysis(Guid tenantId, Guid capaId, CapaRootCauseMethod method, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        CapaId = Guard.AgainstEmpty(capaId, nameof(capaId));
        Method = method;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
    }

    public Guid CapaId { get; private set; }
    public CapaRootCauseMethod Method { get; private set; }
    public string? Why1 { get; private set; }
    public string? Why2 { get; private set; }
    public string? Why3 { get; private set; }
    public string? Why4 { get; private set; }
    public string? Why5 { get; private set; }
    public string? People { get; private set; }
    public string? Process { get; private set; }
    public string? Equipment { get; private set; }
    public string? Material { get; private set; }
    public string? Environment { get; private set; }
    public string? Measurement { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    public static CapaCauseAnalysis FiveWhy(Guid tenantId, Guid capaId, string why1, string why2, string why3, string why4, string why5, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new CapaCauseAnalysis(tenantId, capaId, CapaRootCauseMethod.FiveWhy, createdByUserId, createdAtUtc)
        {
            Why1 = Guard.AgainstNullOrWhiteSpace(why1, nameof(why1), 500),
            Why2 = Guard.AgainstNullOrWhiteSpace(why2, nameof(why2), 500),
            Why3 = Guard.AgainstNullOrWhiteSpace(why3, nameof(why3), 500),
            Why4 = Guard.AgainstNullOrWhiteSpace(why4, nameof(why4), 500),
            Why5 = Guard.AgainstNullOrWhiteSpace(why5, nameof(why5), 500)
        };
    }

    public static CapaCauseAnalysis Ishikawa(Guid tenantId, Guid capaId, string people, string process, string equipment, string material, string environment, string measurement, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new CapaCauseAnalysis(tenantId, capaId, CapaRootCauseMethod.Ishikawa, createdByUserId, createdAtUtc)
        {
            People = Guard.AgainstNullOrWhiteSpace(people, nameof(people), 500),
            Process = Guard.AgainstNullOrWhiteSpace(process, nameof(process), 500),
            Equipment = Guard.AgainstNullOrWhiteSpace(equipment, nameof(equipment), 500),
            Material = Guard.AgainstNullOrWhiteSpace(material, nameof(material), 500),
            Environment = Guard.AgainstNullOrWhiteSpace(environment, nameof(environment), 500),
            Measurement = Guard.AgainstNullOrWhiteSpace(measurement, nameof(measurement), 500)
        };
    }
}

public sealed class CapaEffectivenessCheck : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private CapaEffectivenessCheck()
    {
        VerificationSummary = string.Empty;
    }

    public CapaEffectivenessCheck(Guid tenantId, Guid capaId, bool isEffective, string verificationSummary, Guid verifiedByUserId, DateTimeOffset verifiedAtUtc)
        : base(tenantId)
    {
        CapaId = Guard.AgainstEmpty(capaId, nameof(capaId));
        IsEffective = isEffective;
        VerificationSummary = Guard.AgainstNullOrWhiteSpace(verificationSummary, nameof(verificationSummary), 1_000);
        VerifiedByUserId = Guard.AgainstEmpty(verifiedByUserId, nameof(verifiedByUserId));
        VerifiedAtUtc = verifiedAtUtc;
    }

    public Guid CapaId { get; private set; }
    public bool IsEffective { get; private set; }
    public string VerificationSummary { get; private set; }
    public Guid VerifiedByUserId { get; private set; }
    public DateTimeOffset VerifiedAtUtc { get; private set; }
}

public sealed class CapaEvidence : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private CapaEvidence()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        Sha256Hash = string.Empty;
    }

    public CapaEvidence(Guid tenantId, Guid capaId, Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
        : base(tenantId)
    {
        CapaId = Guard.AgainstEmpty(capaId, nameof(capaId));
        StoredFileId = Guard.AgainstEmpty(storedFileId, nameof(storedFileId));
        FileName = Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName), 260);
        ContentType = Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 120);
        if (sizeBytes <= 0)
        {
            throw new DomainException("CAPA evidence size must be greater than zero.");
        }

        SizeBytes = sizeBytes;
        Sha256Hash = Guard.AgainstNullOrWhiteSpace(sha256Hash, nameof(sha256Hash), 128);
        UploadedByUserId = Guard.AgainstEmpty(uploadedByUserId, nameof(uploadedByUserId));
        UploadedAtUtc = uploadedAtUtc;
    }

    public Guid CapaId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public string Sha256Hash { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
}

public sealed class CapaAttachment : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private CapaAttachment()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        Sha256Hash = string.Empty;
    }

    public CapaAttachment(Guid tenantId, Guid capaId, Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
        : base(tenantId)
    {
        CapaId = Guard.AgainstEmpty(capaId, nameof(capaId));
        StoredFileId = Guard.AgainstEmpty(storedFileId, nameof(storedFileId));
        FileName = Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName), 260);
        ContentType = Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 120);
        if (sizeBytes <= 0)
        {
            throw new DomainException("CAPA attachment size must be greater than zero.");
        }

        SizeBytes = sizeBytes;
        Sha256Hash = Guard.AgainstNullOrWhiteSpace(sha256Hash, nameof(sha256Hash), 128);
        UploadedByUserId = Guard.AgainstEmpty(uploadedByUserId, nameof(uploadedByUserId));
        UploadedAtUtc = uploadedAtUtc;
    }

    public Guid CapaId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public string Sha256Hash { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
}

public sealed class CapaOwner : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private CapaOwner()
    {
    }

    public CapaOwner(Guid tenantId, Guid capaId, Guid userId, DateTimeOffset dueAtUtc)
        : base(tenantId)
    {
        CapaId = Guard.AgainstEmpty(capaId, nameof(capaId));
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        DueAtUtc = dueAtUtc;
        IsActive = true;
    }

    public Guid CapaId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset DueAtUtc { get; private set; }
    public bool IsActive { get; private set; }
}

public sealed class CapaApprover : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private CapaApprover()
    {
    }

    public CapaApprover(Guid tenantId, Guid capaId, Guid userId)
        : base(tenantId)
    {
        CapaId = Guard.AgainstEmpty(capaId, nameof(capaId));
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
    }

    public Guid CapaId { get; private set; }
    public Guid UserId { get; private set; }
}

public sealed class CapaHistory : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private CapaHistory()
    {
        Action = string.Empty;
    }

    public CapaHistory(Guid tenantId, Guid capaId, string action, Guid userId, DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        CapaId = Guard.AgainstEmpty(capaId, nameof(capaId));
        Action = Guard.AgainstNullOrWhiteSpace(action, nameof(action), 1_200);
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid CapaId { get; private set; }
    public string Action { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
