using System.Diagnostics.CodeAnalysis;
using Compliance360.Domain.Common;

namespace Compliance360.Domain.AuditManagement;

public enum ManagedAuditType
{
    Internal = 0,
    External = 1,
    Iso9001 = 2,
    Bpm = 3,
    Haccp = 4,
    Regulatory = 5,
    Supplier = 6
}

public enum ManagedAuditStatus
{
    Draft = 0,
    Planned = 1,
    Scheduled = 2,
    InProgress = 3,
    Completed = 4,
    Closed = 5,
    Reopened = 6,
    Cancelled = 7
}

public enum AuditFindingSeverity
{
    Critical = 0,
    Major = 1,
    Minor = 2,
    Observation = 3,
    OpportunityForImprovement = 4
}

public enum AuditChecklistType
{
    Iso9001 = 0,
    Bpm = 1,
    Haccp = 2,
    Supplier = 3,
    Custom = 4
}

public enum AuditEvidenceType
{
    Pdf = 0,
    Excel = 1,
    Word = 2,
    Image = 3,
    Video = 4,
    Document = 5,
    Attachment = 6
}

public enum AuditParticipantRole
{
    LeadAuditor = 0,
    Auditor = 1,
    Auditee = 2,
    Observer = 3
}

public sealed class AuditProgram : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditProgram()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public AuditProgram(Guid tenantId, string name, string code, int year, Guid createdByUserId)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 80).ToUpperInvariant();
        Year = Guard.AgainstOutOfRange(year, nameof(year), 2000, 2100);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        IsActive = true;
    }

    public string Name { get; private set; }
    public string Code { get; private set; }
    public int Year { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsActive { get; private set; }
}

public sealed class AuditChecklist : TenantEntity
{
    private readonly List<AuditChecklistItem> _items = [];

    [ExcludeFromCodeCoverage]
    private AuditChecklist()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public AuditChecklist(Guid tenantId, string name, string code, AuditChecklistType type, int version, Guid createdByUserId)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 80).ToUpperInvariant();
        Type = type;
        Version = Guard.AgainstOutOfRange(version, nameof(version), 1, 10_000);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        IsTemplate = true;
    }

    public string Name { get; private set; }
    public string Code { get; private set; }
    public AuditChecklistType Type { get; private set; }
    public int Version { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsTemplate { get; private set; }
    public IReadOnlyCollection<AuditChecklistItem> Items => _items.AsReadOnly();

    public AuditChecklistItem AddItem(string clause, string question, int weight)
    {
        var item = new AuditChecklistItem(TenantId, Id, clause, question, weight);
        _items.Add(item);
        return item;
    }
}

public sealed class AuditChecklistItem : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditChecklistItem()
    {
        Clause = string.Empty;
        Question = string.Empty;
    }

    public AuditChecklistItem(Guid tenantId, Guid checklistId, string clause, string question, int weight)
        : base(tenantId)
    {
        ChecklistId = Guard.AgainstEmpty(checklistId, nameof(checklistId));
        Clause = Guard.AgainstNullOrWhiteSpace(clause, nameof(clause), 120);
        Question = Guard.AgainstNullOrWhiteSpace(question, nameof(question), 1_000);
        Weight = Guard.AgainstOutOfRange(weight, nameof(weight), 1, 100);
    }

    public Guid ChecklistId { get; private set; }
    public string Clause { get; private set; }
    public string Question { get; private set; }
    public int Weight { get; private set; }
}

public sealed class AuditPlan : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditPlan()
    {
        Scope = string.Empty;
        Criteria = string.Empty;
    }

    public AuditPlan(Guid tenantId, Guid auditProgramId, string scope, string criteria, DateTimeOffset plannedStartUtc, DateTimeOffset plannedEndUtc)
        : base(tenantId)
    {
        if (plannedEndUtc <= plannedStartUtc)
        {
            throw new DomainException("Audit plan end date must be after start date.");
        }

        AuditProgramId = Guard.AgainstEmpty(auditProgramId, nameof(auditProgramId));
        Scope = Guard.AgainstNullOrWhiteSpace(scope, nameof(scope), 2_000);
        Criteria = Guard.AgainstNullOrWhiteSpace(criteria, nameof(criteria), 2_000);
        PlannedStartUtc = plannedStartUtc;
        PlannedEndUtc = plannedEndUtc;
    }

    public Guid AuditProgramId { get; private set; }
    public string Scope { get; private set; }
    public string Criteria { get; private set; }
    public DateTimeOffset PlannedStartUtc { get; private set; }
    public DateTimeOffset PlannedEndUtc { get; private set; }
}

public sealed class ManagedAudit : TenantEntity
{
    private readonly List<AuditSchedule> _schedules = [];
    private readonly List<AuditParticipant> _participants = [];
    private readonly List<AuditArea> _areas = [];
    private readonly List<AuditFinding> _findings = [];
    private readonly List<AuditEvidence> _evidence = [];
    private readonly List<AuditObservation> _observations = [];
    private readonly List<AuditNonConformity> _nonConformities = [];
    private readonly List<AuditRecommendation> _recommendations = [];
    private readonly List<AuditCorrectiveActionLink> _correctiveActionLinks = [];
    private readonly List<AuditHistory> _history = [];
    private readonly List<AuditAttachment> _attachments = [];

    [ExcludeFromCodeCoverage]
    private ManagedAudit()
    {
        Title = string.Empty;
        Code = string.Empty;
    }

    public ManagedAudit(Guid tenantId, Guid auditProgramId, Guid auditPlanId, string title, string code, ManagedAuditType type, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        AuditProgramId = Guard.AgainstEmpty(auditProgramId, nameof(auditProgramId));
        AuditPlanId = Guard.AgainstEmpty(auditPlanId, nameof(auditPlanId));
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title), 220);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 100).ToUpperInvariant();
        Type = type;
        Status = ManagedAuditStatus.Draft;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
        AddHistory("Audit created.", createdByUserId, createdAtUtc);
    }

    public Guid AuditProgramId { get; private set; }
    public Guid AuditPlanId { get; private set; }
    public Guid? ChecklistId { get; private set; }
    public string Title { get; private set; }
    public string Code { get; private set; }
    public ManagedAuditType Type { get; private set; }
    public ManagedAuditStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public IReadOnlyCollection<AuditSchedule> Schedules => _schedules.AsReadOnly();
    public IReadOnlyCollection<AuditParticipant> Participants => _participants.AsReadOnly();
    public IReadOnlyCollection<AuditArea> Areas => _areas.AsReadOnly();
    public IReadOnlyCollection<AuditFinding> Findings => _findings.AsReadOnly();
    public IReadOnlyCollection<AuditEvidence> Evidence => _evidence.AsReadOnly();
    public IReadOnlyCollection<AuditObservation> Observations => _observations.AsReadOnly();
    public IReadOnlyCollection<AuditNonConformity> NonConformities => _nonConformities.AsReadOnly();
    public IReadOnlyCollection<AuditRecommendation> Recommendations => _recommendations.AsReadOnly();
    public IReadOnlyCollection<AuditCorrectiveActionLink> CorrectiveActionLinks => _correctiveActionLinks.AsReadOnly();
    public IReadOnlyCollection<AuditHistory> History => _history.AsReadOnly();
    public IReadOnlyCollection<AuditAttachment> Attachments => _attachments.AsReadOnly();

    public void AssignChecklist(Guid checklistId, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        ChecklistId = Guard.AgainstEmpty(checklistId, nameof(checklistId));
        AddHistory("Checklist assigned.", requestedByUserId, occurredAtUtc);
    }

    public AuditSchedule Schedule(DateTimeOffset startUtc, DateTimeOffset endUtc, string location, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (endUtc <= startUtc)
        {
            throw new DomainException("Audit schedule end date must be after start date.");
        }

        var schedule = new AuditSchedule(TenantId, Id, startUtc, endUtc, location);
        _schedules.Add(schedule);
        Status = ManagedAuditStatus.Scheduled;
        AddHistory("Audit scheduled.", requestedByUserId, occurredAtUtc);
        return schedule;
    }

    public AuditParticipant AddParticipant(Guid userId, AuditParticipantRole role, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (_participants.Any(participant => participant.UserId == userId && participant.Role == role))
        {
            return _participants.First(participant => participant.UserId == userId && participant.Role == role);
        }

        var participant = new AuditParticipant(TenantId, Id, userId, role);
        _participants.Add(participant);
        AddHistory("Participant assigned.", requestedByUserId, occurredAtUtc);
        return participant;
    }

    public AuditAuditor AddAuditor(Guid userId, bool isLead)
    {
        AddParticipant(userId, isLead ? AuditParticipantRole.LeadAuditor : AuditParticipantRole.Auditor, userId, DateTimeOffset.UtcNow);
        return new AuditAuditor(TenantId, Id, userId, isLead);
    }

    public AuditArea AddArea(string name, string process)
    {
        var area = new AuditArea(TenantId, Id, name, process);
        _areas.Add(area);
        return area;
    }

    public void Start(Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != ManagedAuditStatus.Scheduled && Status != ManagedAuditStatus.Planned)
        {
            throw new DomainException("Audit must be planned or scheduled before starting.");
        }

        Status = ManagedAuditStatus.InProgress;
        AddHistory("Audit started.", requestedByUserId, occurredAtUtc);
    }

    public AuditFinding AddFinding(string title, string description, AuditFindingSeverity severity, Guid? checklistItemId, Guid reportedByUserId, DateTimeOffset occurredAtUtc)
    {
        EnsureInProgress();
        var finding = new AuditFinding(TenantId, Id, title, description, severity, checklistItemId, reportedByUserId, occurredAtUtc);
        _findings.Add(finding);
        AddHistory("Finding registered.", reportedByUserId, occurredAtUtc);
        return finding;
    }

    public AuditEvidence AddEvidence(Guid findingId, AuditEvidenceType type, Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
    {
        EnsureFindingExists(findingId);
        var evidence = new AuditEvidence(TenantId, Id, findingId, type, storedFileId, fileName, contentType, sizeBytes, sha256Hash, uploadedByUserId, uploadedAtUtc);
        _evidence.Add(evidence);
        AddHistory("Evidence attached.", uploadedByUserId, uploadedAtUtc);
        return evidence;
    }

    public AuditObservation AddObservation(string description, Guid reportedByUserId, DateTimeOffset occurredAtUtc)
    {
        EnsureInProgress();
        var observation = new AuditObservation(TenantId, Id, description, reportedByUserId, occurredAtUtc);
        _observations.Add(observation);
        AddHistory("Observation registered.", reportedByUserId, occurredAtUtc);
        return observation;
    }

    public AuditNonConformity AddNonConformity(Guid findingId, string requirement, Guid reportedByUserId, DateTimeOffset occurredAtUtc)
    {
        EnsureFindingExists(findingId);
        var nonConformity = new AuditNonConformity(TenantId, Id, findingId, requirement, reportedByUserId, occurredAtUtc);
        _nonConformities.Add(nonConformity);
        AddHistory("Non conformity registered.", reportedByUserId, occurredAtUtc);
        return nonConformity;
    }

    public AuditRecommendation AddRecommendation(Guid findingId, string recommendation, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        EnsureFindingExists(findingId);
        var item = new AuditRecommendation(TenantId, Id, findingId, recommendation, requestedByUserId, occurredAtUtc);
        _recommendations.Add(item);
        AddHistory("Recommendation created.", requestedByUserId, occurredAtUtc);
        return item;
    }

    public AuditCorrectiveActionLink LinkCorrectiveAction(Guid findingId, Guid correctiveActionId, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        EnsureFindingExists(findingId);
        var link = new AuditCorrectiveActionLink(TenantId, Id, findingId, correctiveActionId);
        _correctiveActionLinks.Add(link);
        AddHistory("Corrective action linked.", requestedByUserId, occurredAtUtc);
        return link;
    }

    public AuditAttachment AddAttachment(Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
    {
        var attachment = new AuditAttachment(TenantId, Id, storedFileId, fileName, contentType, sizeBytes, sha256Hash, uploadedByUserId, uploadedAtUtc);
        _attachments.Add(attachment);
        AddHistory("Attachment added.", uploadedByUserId, uploadedAtUtc);
        return attachment;
    }

    public void Complete(Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        EnsureInProgress();
        Status = ManagedAuditStatus.Completed;
        AddHistory("Audit completed.", requestedByUserId, occurredAtUtc);
    }

    public void Close(Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != ManagedAuditStatus.Completed)
        {
            throw new DomainException("Only completed audits can be closed.");
        }

        Status = ManagedAuditStatus.Closed;
        ClosedAtUtc = occurredAtUtc;
        AddHistory("Audit closed.", requestedByUserId, occurredAtUtc);
    }

    public void Reopen(Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != ManagedAuditStatus.Closed)
        {
            throw new DomainException("Only closed audits can be reopened.");
        }

        Status = ManagedAuditStatus.Reopened;
        AddHistory("Audit reopened.", requestedByUserId, occurredAtUtc);
    }

    public AuditDashboardSnapshot Dashboard()
    {
        return new AuditDashboardSnapshot(
            Status is ManagedAuditStatus.Draft or ManagedAuditStatus.Planned or ManagedAuditStatus.Scheduled or ManagedAuditStatus.InProgress or ManagedAuditStatus.Reopened ? 1 : 0,
            Status == ManagedAuditStatus.Closed ? 1 : 0,
            _findings.Count(finding => finding.Severity == AuditFindingSeverity.Critical),
            _findings.Count(finding => finding.Severity == AuditFindingSeverity.Major),
            _correctiveActionLinks.Count,
            _findings.Count == 0 ? 100 : Math.Max(0, 100 - (_findings.Count(finding => finding.Severity is AuditFindingSeverity.Critical or AuditFindingSeverity.Major) * 10)),
            _findings.Count);
    }

    private void EnsureInProgress()
    {
        if (Status != ManagedAuditStatus.InProgress)
        {
            throw new DomainException("Audit must be in progress.");
        }
    }

    private void EnsureFindingExists(Guid findingId)
    {
        if (_findings.All(finding => finding.Id != findingId))
        {
            throw new DomainException("Audit finding not found.");
        }
    }

    private void AddHistory(string action, Guid userId, DateTimeOffset occurredAtUtc)
    {
        _history.Add(new AuditHistory(TenantId, Id, action, userId, occurredAtUtc));
    }
}

public sealed record AuditDashboardSnapshot(
    int OpenAudits,
    int ClosedAudits,
    int CriticalFindings,
    int MajorFindings,
    int PendingActions,
    int ComplianceScore,
    int TrendTotalFindings);

public sealed class AuditSchedule : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditSchedule()
    {
        Location = string.Empty;
    }

    public AuditSchedule(Guid tenantId, Guid auditId, DateTimeOffset startUtc, DateTimeOffset endUtc, string location)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        StartUtc = startUtc;
        EndUtc = endUtc;
        Location = Guard.AgainstNullOrWhiteSpace(location, nameof(location), 250);
    }

    public Guid AuditId { get; private set; }
    public DateTimeOffset StartUtc { get; private set; }
    public DateTimeOffset EndUtc { get; private set; }
    public string Location { get; private set; }
}

public sealed class AuditParticipant : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditParticipant()
    {
    }

    public AuditParticipant(Guid tenantId, Guid auditId, Guid userId, AuditParticipantRole role)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        Role = role;
    }

    public Guid AuditId { get; private set; }
    public Guid UserId { get; private set; }
    public AuditParticipantRole Role { get; private set; }
}

public sealed class AuditAuditor : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditAuditor()
    {
    }

    public AuditAuditor(Guid tenantId, Guid auditId, Guid userId, bool isLead)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        IsLead = isLead;
    }

    public Guid AuditId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsLead { get; private set; }
}

public sealed class AuditArea : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditArea()
    {
        Name = string.Empty;
        Process = string.Empty;
    }

    public AuditArea(Guid tenantId, Guid auditId, string name, string process)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Process = Guard.AgainstNullOrWhiteSpace(process, nameof(process), 180);
    }

    public Guid AuditId { get; private set; }
    public string Name { get; private set; }
    public string Process { get; private set; }
}

public sealed class AuditFinding : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditFinding()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public AuditFinding(Guid tenantId, Guid auditId, string title, string description, AuditFindingSeverity severity, Guid? checklistItemId, Guid reportedByUserId, DateTimeOffset reportedAtUtc)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title), 220);
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 2_000);
        Severity = severity;
        ChecklistItemId = checklistItemId;
        ReportedByUserId = Guard.AgainstEmpty(reportedByUserId, nameof(reportedByUserId));
        ReportedAtUtc = reportedAtUtc;
    }

    public Guid AuditId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public AuditFindingSeverity Severity { get; private set; }
    public Guid? ChecklistItemId { get; private set; }
    public Guid ReportedByUserId { get; private set; }
    public DateTimeOffset ReportedAtUtc { get; private set; }
}

public sealed class AuditEvidence : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditEvidence()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        Sha256Hash = string.Empty;
    }

    public AuditEvidence(Guid tenantId, Guid auditId, Guid findingId, AuditEvidenceType type, Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        FindingId = Guard.AgainstEmpty(findingId, nameof(findingId));
        Type = type;
        StoredFileId = Guard.AgainstEmpty(storedFileId, nameof(storedFileId));
        FileName = Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName), 260);
        ContentType = Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 120);
        if (sizeBytes <= 0)
        {
            throw new DomainException("Evidence size must be greater than zero.");
        }

        SizeBytes = sizeBytes;
        Sha256Hash = Guard.AgainstNullOrWhiteSpace(sha256Hash, nameof(sha256Hash), 128);
        UploadedByUserId = Guard.AgainstEmpty(uploadedByUserId, nameof(uploadedByUserId));
        UploadedAtUtc = uploadedAtUtc;
    }

    public Guid AuditId { get; private set; }
    public Guid FindingId { get; private set; }
    public AuditEvidenceType Type { get; private set; }
    public Guid StoredFileId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public string Sha256Hash { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
}

public sealed class AuditObservation : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditObservation()
    {
        Description = string.Empty;
    }

    public AuditObservation(Guid tenantId, Guid auditId, string description, Guid reportedByUserId, DateTimeOffset reportedAtUtc)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 2_000);
        ReportedByUserId = Guard.AgainstEmpty(reportedByUserId, nameof(reportedByUserId));
        ReportedAtUtc = reportedAtUtc;
    }

    public Guid AuditId { get; private set; }
    public string Description { get; private set; }
    public Guid ReportedByUserId { get; private set; }
    public DateTimeOffset ReportedAtUtc { get; private set; }
}

public sealed class AuditNonConformity : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditNonConformity()
    {
        Requirement = string.Empty;
    }

    public AuditNonConformity(Guid tenantId, Guid auditId, Guid findingId, string requirement, Guid reportedByUserId, DateTimeOffset reportedAtUtc)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        FindingId = Guard.AgainstEmpty(findingId, nameof(findingId));
        Requirement = Guard.AgainstNullOrWhiteSpace(requirement, nameof(requirement), 1_000);
        ReportedByUserId = Guard.AgainstEmpty(reportedByUserId, nameof(reportedByUserId));
        ReportedAtUtc = reportedAtUtc;
    }

    public Guid AuditId { get; private set; }
    public Guid FindingId { get; private set; }
    public string Requirement { get; private set; }
    public Guid ReportedByUserId { get; private set; }
    public DateTimeOffset ReportedAtUtc { get; private set; }
}

public sealed class AuditRecommendation : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditRecommendation()
    {
        Recommendation = string.Empty;
    }

    public AuditRecommendation(Guid tenantId, Guid auditId, Guid findingId, string recommendation, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        FindingId = Guard.AgainstEmpty(findingId, nameof(findingId));
        Recommendation = Guard.AgainstNullOrWhiteSpace(recommendation, nameof(recommendation), 1_000);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
    }

    public Guid AuditId { get; private set; }
    public Guid FindingId { get; private set; }
    public string Recommendation { get; private set; }
    public Guid CreatedByUserId { get; private set; }
}

public sealed class AuditCorrectiveActionLink : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditCorrectiveActionLink()
    {
    }

    public AuditCorrectiveActionLink(Guid tenantId, Guid auditId, Guid findingId, Guid correctiveActionId)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        FindingId = Guard.AgainstEmpty(findingId, nameof(findingId));
        CorrectiveActionId = Guard.AgainstEmpty(correctiveActionId, nameof(correctiveActionId));
    }

    public Guid AuditId { get; private set; }
    public Guid FindingId { get; private set; }
    public Guid CorrectiveActionId { get; private set; }
}

public sealed class AuditHistory : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditHistory()
    {
        Action = string.Empty;
    }

    public AuditHistory(Guid tenantId, Guid auditId, string action, Guid userId, DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        Action = Guard.AgainstNullOrWhiteSpace(action, nameof(action), 500);
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid AuditId { get; private set; }
    public string Action { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}

public sealed class AuditAttachment : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private AuditAttachment()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        Sha256Hash = string.Empty;
    }

    public AuditAttachment(Guid tenantId, Guid auditId, Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
        : base(tenantId)
    {
        AuditId = Guard.AgainstEmpty(auditId, nameof(auditId));
        StoredFileId = Guard.AgainstEmpty(storedFileId, nameof(storedFileId));
        FileName = Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName), 260);
        ContentType = Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 120);
        if (sizeBytes <= 0)
        {
            throw new DomainException("Attachment size must be greater than zero.");
        }

        SizeBytes = sizeBytes;
        Sha256Hash = Guard.AgainstNullOrWhiteSpace(sha256Hash, nameof(sha256Hash), 128);
        UploadedByUserId = Guard.AgainstEmpty(uploadedByUserId, nameof(uploadedByUserId));
        UploadedAtUtc = uploadedAtUtc;
    }

    public Guid AuditId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public string Sha256Hash { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
}
