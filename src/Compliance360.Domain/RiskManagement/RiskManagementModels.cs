using System.Diagnostics.CodeAnalysis;
using Compliance360.Domain.Common;

namespace Compliance360.Domain.RiskManagement;

public enum RiskStatus
{
    Draft = 0,
    Identified = 1,
    Assessed = 2,
    TreatmentPlanned = 3,
    Mitigating = 4,
    Monitoring = 5,
    Accepted = 6,
    Closed = 7,
    Reopened = 8
}

public enum RiskType
{
    Iso9001 = 0,
    Iso31000 = 1,
    Bpm = 2,
    Haccp = 3,
    Operational = 4,
    Regulatory = 5,
    Supplier = 6,
    Document = 7
}

public enum RiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum RiskTreatmentStrategy
{
    Avoid = 0,
    Reduce = 1,
    Transfer = 2,
    Accept = 3
}

public enum RiskControlType
{
    Preventive = 0,
    Detective = 1,
    Corrective = 2,
    Compensating = 3
}

public enum RiskReviewStatus
{
    Scheduled = 0,
    Completed = 1,
    Overdue = 2
}

public sealed class RiskCategory : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskCategory()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public RiskCategory(Guid tenantId, string name, string code, Guid createdByUserId)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 80).ToUpperInvariant();
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        IsActive = true;
    }

    public string Name { get; private set; }
    public string Code { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsActive { get; private set; }
}

public sealed class RiskMatrix : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskMatrix()
    {
        Name = string.Empty;
    }

    public RiskMatrix(Guid tenantId, string name, int toleranceScore, Guid createdByUserId)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        ToleranceScore = Guard.AgainstOutOfRange(toleranceScore, nameof(toleranceScore), 1, 25);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        IsDefault = true;
    }

    public string Name { get; private set; }
    public int ToleranceScore { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsDefault { get; private set; }

    public static int CalculateScore(RiskProbability probability, RiskImpact impact)
    {
        return Guard.AgainstOutOfRange((int)probability, nameof(probability), 1, 5)
            * Guard.AgainstOutOfRange((int)impact, nameof(impact), 1, 5);
    }

    public static RiskLevel CalculateLevel(int score)
    {
        return score switch
        {
            >= 20 => RiskLevel.Critical,
            >= 12 => RiskLevel.High,
            >= 6 => RiskLevel.Medium,
            _ => RiskLevel.Low
        };
    }

    public bool IsWithinTolerance(int score) => score <= ToleranceScore;
}

public enum RiskProbability
{
    Rare = 1,
    Unlikely = 2,
    Possible = 3,
    Likely = 4,
    AlmostCertain = 5
}

public enum RiskImpact
{
    Insignificant = 1,
    Minor = 2,
    Moderate = 3,
    Major = 4,
    Severe = 5
}

public sealed class Risk : TenantEntity
{
    private readonly List<RiskAssessment> _assessments = [];
    private readonly List<RiskTreatment> _treatments = [];
    private readonly List<RiskMitigationPlan> _mitigationPlans = [];
    private readonly List<RiskControl> _controls = [];
    private readonly List<RiskOwner> _owners = [];
    private readonly List<RiskReview> _reviews = [];
    private readonly List<RiskEvidence> _evidence = [];
    private readonly List<RiskIndicator> _indicators = [];
    private readonly List<RiskAttachment> _attachments = [];
    private readonly List<RiskHistory> _history = [];

    [ExcludeFromCodeCoverage]
    private Risk()
    {
        Title = string.Empty;
        Code = string.Empty;
        Description = string.Empty;
        Area = string.Empty;
        Process = string.Empty;
    }

    public Risk(Guid tenantId, Guid categoryId, string title, string code, string description, RiskType type, string area, string process, Guid? supplierId, Guid? documentId, Guid? auditId, Guid? capaId, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        CategoryId = Guard.AgainstEmpty(categoryId, nameof(categoryId));
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title), 220);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 100).ToUpperInvariant();
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 2_000);
        Type = type;
        Area = Guard.AgainstNullOrWhiteSpace(area, nameof(area), 160);
        Process = Guard.AgainstNullOrWhiteSpace(process, nameof(process), 160);
        SupplierId = supplierId;
        DocumentId = documentId;
        AuditId = auditId;
        CapaId = capaId;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        Status = RiskStatus.Draft;
        CreatedAtUtc = createdAtUtc;
        AddHistory("Risk created.", createdByUserId, createdAtUtc);
    }

    public Guid CategoryId { get; private set; }
    public string Title { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public RiskType Type { get; private set; }
    public RiskStatus Status { get; private set; }
    public string Area { get; private set; }
    public string Process { get; private set; }
    public Guid? SupplierId { get; private set; }
    public Guid? DocumentId { get; private set; }
    public Guid? AuditId { get; private set; }
    public Guid? CapaId { get; private set; }
    public Guid? WorkflowInstanceId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? ClosedByUserId { get; private set; }
    public DateTimeOffset? ReviewDueAtUtc { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public int InherentScore { get; private set; }
    public RiskLevel InherentLevel { get; private set; }
    public int ResidualScore { get; private set; }
    public RiskLevel ResidualLevel { get; private set; }
    public bool IsAccepted { get; private set; }
    public bool IsWithinTolerance { get; private set; }
    public IReadOnlyCollection<RiskAssessment> Assessments => _assessments.AsReadOnly();
    public IReadOnlyCollection<RiskTreatment> Treatments => _treatments.AsReadOnly();
    public IReadOnlyCollection<RiskMitigationPlan> MitigationPlans => _mitigationPlans.AsReadOnly();
    public IReadOnlyCollection<RiskControl> Controls => _controls.AsReadOnly();
    public IReadOnlyCollection<RiskOwner> Owners => _owners.AsReadOnly();
    public IReadOnlyCollection<RiskReview> Reviews => _reviews.AsReadOnly();
    public IReadOnlyCollection<RiskEvidence> Evidence => _evidence.AsReadOnly();
    public IReadOnlyCollection<RiskIndicator> Indicators => _indicators.AsReadOnly();
    public IReadOnlyCollection<RiskAttachment> Attachments => _attachments.AsReadOnly();
    public IReadOnlyCollection<RiskHistory> History => _history.AsReadOnly();

    public void Classify(RiskType type, string area, string process, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        Type = type;
        Area = Guard.AgainstNullOrWhiteSpace(area, nameof(area), 160);
        Process = Guard.AgainstNullOrWhiteSpace(process, nameof(process), 160);
        Status = Status == RiskStatus.Draft ? RiskStatus.Identified : Status;
        AddHistory("Risk classified.", requestedByUserId, occurredAtUtc);
    }

    public RiskOwner AssignOwner(Guid ownerUserId, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        var existing = _owners.FirstOrDefault(owner => owner.UserId == ownerUserId && owner.IsActive);
        if (existing is not null)
        {
            return existing;
        }

        var owner = new RiskOwner(TenantId, Id, ownerUserId);
        _owners.Add(owner);
        AddHistory("Risk owner assigned.", requestedByUserId, occurredAtUtc);
        return owner;
    }

    public RiskAssessment Assess(RiskProbability probability, RiskImpact impact, RiskProbability residualProbability, RiskImpact residualImpact, int toleranceScore, Guid assessedByUserId, DateTimeOffset assessedAtUtc)
    {
        var inherentScore = RiskMatrix.CalculateScore(probability, impact);
        var residualScore = RiskMatrix.CalculateScore(residualProbability, residualImpact);
        var assessment = new RiskAssessment(TenantId, Id, probability, impact, inherentScore, RiskMatrix.CalculateLevel(inherentScore), residualProbability, residualImpact, residualScore, RiskMatrix.CalculateLevel(residualScore), toleranceScore, assessedByUserId, assessedAtUtc);
        _assessments.Add(assessment);
        InherentScore = assessment.InherentScore;
        InherentLevel = assessment.InherentLevel;
        ResidualScore = assessment.ResidualScore;
        ResidualLevel = assessment.ResidualLevel;
        IsWithinTolerance = residualScore <= toleranceScore;
        Status = RiskStatus.Assessed;
        AddHistory("Risk assessed.", assessedByUserId, assessedAtUtc);
        return assessment;
    }

    public RiskTreatment AddTreatment(RiskTreatmentStrategy strategy, string rationale, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        var treatment = new RiskTreatment(TenantId, Id, strategy, rationale, requestedByUserId, occurredAtUtc);
        _treatments.Add(treatment);
        IsAccepted = strategy == RiskTreatmentStrategy.Accept;
        Status = strategy == RiskTreatmentStrategy.Accept ? RiskStatus.Accepted : RiskStatus.TreatmentPlanned;
        AddHistory("Risk treatment registered.", requestedByUserId, occurredAtUtc);
        return treatment;
    }

    public RiskMitigationPlan AddMitigationPlan(string description, Guid responsibleUserId, DateTimeOffset dueAtUtc, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (dueAtUtc <= occurredAtUtc)
        {
            throw new DomainException("Risk mitigation due date must be in the future.");
        }

        var plan = new RiskMitigationPlan(TenantId, Id, description, responsibleUserId, dueAtUtc);
        _mitigationPlans.Add(plan);
        Status = RiskStatus.Mitigating;
        AddHistory("Risk mitigation plan registered.", requestedByUserId, occurredAtUtc);
        return plan;
    }

    public RiskControl AddControl(string name, RiskControlType type, string description, bool isEffective, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        var control = new RiskControl(TenantId, Id, name, type, description, isEffective);
        _controls.Add(control);
        AddHistory("Risk control registered.", requestedByUserId, occurredAtUtc);
        return control;
    }

    public RiskEvidence AddEvidence(Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
    {
        var evidence = new RiskEvidence(TenantId, Id, storedFileId, fileName, contentType, sizeBytes, sha256Hash, uploadedByUserId, uploadedAtUtc);
        _evidence.Add(evidence);
        AddHistory("Risk evidence attached.", uploadedByUserId, uploadedAtUtc);
        return evidence;
    }

    public RiskAttachment AddAttachment(Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
    {
        var attachment = new RiskAttachment(TenantId, Id, storedFileId, fileName, contentType, sizeBytes, sha256Hash, uploadedByUserId, uploadedAtUtc);
        _attachments.Add(attachment);
        AddHistory("Risk attachment added.", uploadedByUserId, uploadedAtUtc);
        return attachment;
    }

    public RiskReview ScheduleReview(DateTimeOffset dueAtUtc, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (dueAtUtc <= occurredAtUtc)
        {
            throw new DomainException("Risk review due date must be in the future.");
        }

        var review = new RiskReview(TenantId, Id, dueAtUtc);
        _reviews.Add(review);
        ReviewDueAtUtc = dueAtUtc;
        Status = RiskStatus.Monitoring;
        AddHistory("Risk review scheduled.", requestedByUserId, occurredAtUtc);
        return review;
    }

    public void CompleteReview(Guid reviewId, string summary, Guid reviewedByUserId, DateTimeOffset reviewedAtUtc)
    {
        var review = _reviews.FirstOrDefault(item => item.Id == reviewId);
        if (review is null)
        {
            throw new DomainException("Risk review not found.");
        }

        review.Complete(summary, reviewedByUserId, reviewedAtUtc);
        AddHistory("Risk review completed.", reviewedByUserId, reviewedAtUtc);
    }

    public RiskIndicator AddIndicator(string name, decimal value, decimal threshold, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        var indicator = new RiskIndicator(TenantId, Id, name, value, threshold);
        _indicators.Add(indicator);
        AddHistory("Risk indicator registered.", requestedByUserId, occurredAtUtc);
        return indicator;
    }

    public void EscalateCritical(Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (ResidualLevel != RiskLevel.Critical && InherentLevel != RiskLevel.Critical)
        {
            throw new DomainException("Only critical risks can be escalated.");
        }

        Status = RiskStatus.Monitoring;
        AddHistory("Critical risk escalated.", requestedByUserId, occurredAtUtc);
    }

    public void AttachWorkflow(Guid workflowInstanceId, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        WorkflowInstanceId = Guard.AgainstEmpty(workflowInstanceId, nameof(workflowInstanceId));
        AddHistory("Risk workflow attached.", requestedByUserId, occurredAtUtc);
    }

    public void Close(Guid closedByUserId, DateTimeOffset closedAtUtc)
    {
        if (Status == RiskStatus.Closed)
        {
            throw new DomainException("Risk is already closed.");
        }

        Status = RiskStatus.Closed;
        ClosedByUserId = Guard.AgainstEmpty(closedByUserId, nameof(closedByUserId));
        ClosedAtUtc = closedAtUtc;
        AddHistory("Risk closed.", closedByUserId, closedAtUtc);
    }

    public void Reopen(string reason, Guid requestedByUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != RiskStatus.Closed)
        {
            throw new DomainException("Only closed risks can be reopened.");
        }

        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 1_000);
        Status = RiskStatus.Reopened;
        ClosedAtUtc = null;
        ClosedByUserId = null;
        AddHistory($"Risk reopened: {reason}", requestedByUserId, occurredAtUtc);
    }

    public bool IsOverdue(DateTimeOffset now)
    {
        return Status != RiskStatus.Closed && ReviewDueAtUtc.HasValue && ReviewDueAtUtc.Value < now;
    }

    public RiskHeatMapPoint HeatMapPoint()
    {
        return new RiskHeatMapPoint((int)(_assessments.LastOrDefault()?.ResidualProbability ?? RiskProbability.Rare), (int)(_assessments.LastOrDefault()?.ResidualImpact ?? RiskImpact.Insignificant), ResidualScore, ResidualLevel);
    }

    private void AddHistory(string action, Guid userId, DateTimeOffset occurredAtUtc)
    {
        _history.Add(new RiskHistory(TenantId, Id, action, userId, occurredAtUtc));
    }
}

public sealed record RiskHeatMapPoint(int Probability, int Impact, int Score, RiskLevel Level);

public sealed class RiskAssessment : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskAssessment()
    {
    }

    public RiskAssessment(Guid tenantId, Guid riskId, RiskProbability probability, RiskImpact impact, int inherentScore, RiskLevel inherentLevel, RiskProbability residualProbability, RiskImpact residualImpact, int residualScore, RiskLevel residualLevel, int toleranceScore, Guid assessedByUserId, DateTimeOffset assessedAtUtc)
        : base(tenantId)
    {
        RiskId = Guard.AgainstEmpty(riskId, nameof(riskId));
        Probability = probability;
        Impact = impact;
        InherentScore = Guard.AgainstOutOfRange(inherentScore, nameof(inherentScore), 1, 25);
        InherentLevel = inherentLevel;
        ResidualProbability = residualProbability;
        ResidualImpact = residualImpact;
        ResidualScore = Guard.AgainstOutOfRange(residualScore, nameof(residualScore), 1, 25);
        ResidualLevel = residualLevel;
        ToleranceScore = Guard.AgainstOutOfRange(toleranceScore, nameof(toleranceScore), 1, 25);
        AssessedByUserId = Guard.AgainstEmpty(assessedByUserId, nameof(assessedByUserId));
        AssessedAtUtc = assessedAtUtc;
    }

    public Guid RiskId { get; private set; }
    public RiskProbability Probability { get; private set; }
    public RiskImpact Impact { get; private set; }
    public int InherentScore { get; private set; }
    public RiskLevel InherentLevel { get; private set; }
    public RiskProbability ResidualProbability { get; private set; }
    public RiskImpact ResidualImpact { get; private set; }
    public int ResidualScore { get; private set; }
    public RiskLevel ResidualLevel { get; private set; }
    public int ToleranceScore { get; private set; }
    public Guid AssessedByUserId { get; private set; }
    public DateTimeOffset AssessedAtUtc { get; private set; }
}

public sealed class RiskTreatment : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskTreatment()
    {
        Rationale = string.Empty;
    }

    public RiskTreatment(Guid tenantId, Guid riskId, RiskTreatmentStrategy strategy, string rationale, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        RiskId = Guard.AgainstEmpty(riskId, nameof(riskId));
        Strategy = strategy;
        Rationale = Guard.AgainstNullOrWhiteSpace(rationale, nameof(rationale), 1_000);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
    }

    public Guid RiskId { get; private set; }
    public RiskTreatmentStrategy Strategy { get; private set; }
    public string Rationale { get; private set; }
    public Guid CreatedByUserId { get; private set; }
}

public sealed class RiskMitigationPlan : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskMitigationPlan()
    {
        Description = string.Empty;
    }

    public RiskMitigationPlan(Guid tenantId, Guid riskId, string description, Guid responsibleUserId, DateTimeOffset dueAtUtc)
        : base(tenantId)
    {
        RiskId = Guard.AgainstEmpty(riskId, nameof(riskId));
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 1_000);
        ResponsibleUserId = Guard.AgainstEmpty(responsibleUserId, nameof(responsibleUserId));
        DueAtUtc = dueAtUtc;
        IsCompleted = false;
    }

    public Guid RiskId { get; private set; }
    public string Description { get; private set; }
    public Guid ResponsibleUserId { get; private set; }
    public DateTimeOffset DueAtUtc { get; private set; }
    public bool IsCompleted { get; private set; }

    public void Complete() => IsCompleted = true;
}

public sealed class RiskControl : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskControl()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public RiskControl(Guid tenantId, Guid riskId, string name, RiskControlType type, string description, bool isEffective)
        : base(tenantId)
    {
        RiskId = Guard.AgainstEmpty(riskId, nameof(riskId));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Type = type;
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 1_000);
        IsEffective = isEffective;
    }

    public Guid RiskId { get; private set; }
    public string Name { get; private set; }
    public RiskControlType Type { get; private set; }
    public string Description { get; private set; }
    public bool IsEffective { get; private set; }
}

public sealed class RiskOwner : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskOwner()
    {
    }

    public RiskOwner(Guid tenantId, Guid riskId, Guid userId)
        : base(tenantId)
    {
        RiskId = Guard.AgainstEmpty(riskId, nameof(riskId));
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        IsActive = true;
    }

    public Guid RiskId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsActive { get; private set; }
}

public sealed class RiskReview : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskReview()
    {
        Summary = string.Empty;
    }

    public RiskReview(Guid tenantId, Guid riskId, DateTimeOffset dueAtUtc)
        : base(tenantId)
    {
        RiskId = Guard.AgainstEmpty(riskId, nameof(riskId));
        DueAtUtc = dueAtUtc;
        Status = RiskReviewStatus.Scheduled;
    }

    public Guid RiskId { get; private set; }
    public DateTimeOffset DueAtUtc { get; private set; }
    public RiskReviewStatus Status { get; private set; }
    public string? Summary { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }

    public void Complete(string summary, Guid reviewedByUserId, DateTimeOffset reviewedAtUtc)
    {
        Summary = Guard.AgainstNullOrWhiteSpace(summary, nameof(summary), 1_000);
        ReviewedByUserId = Guard.AgainstEmpty(reviewedByUserId, nameof(reviewedByUserId));
        ReviewedAtUtc = reviewedAtUtc;
        Status = RiskReviewStatus.Completed;
    }
}

public sealed class RiskEvidence : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskEvidence()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        Sha256Hash = string.Empty;
    }

    public RiskEvidence(Guid tenantId, Guid riskId, Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
        : base(tenantId)
    {
        RiskId = Guard.AgainstEmpty(riskId, nameof(riskId));
        StoredFileId = Guard.AgainstEmpty(storedFileId, nameof(storedFileId));
        FileName = Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName), 260);
        ContentType = Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 120);
        if (sizeBytes <= 0)
        {
            throw new DomainException("Risk evidence size must be greater than zero.");
        }

        SizeBytes = sizeBytes;
        Sha256Hash = Guard.AgainstNullOrWhiteSpace(sha256Hash, nameof(sha256Hash), 128);
        UploadedByUserId = Guard.AgainstEmpty(uploadedByUserId, nameof(uploadedByUserId));
        UploadedAtUtc = uploadedAtUtc;
    }

    public Guid RiskId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public string Sha256Hash { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
}

public sealed class RiskAttachment : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskAttachment()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        Sha256Hash = string.Empty;
    }

    public RiskAttachment(Guid tenantId, Guid riskId, Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
        : base(tenantId)
    {
        RiskId = Guard.AgainstEmpty(riskId, nameof(riskId));
        StoredFileId = Guard.AgainstEmpty(storedFileId, nameof(storedFileId));
        FileName = Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName), 260);
        ContentType = Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 120);
        if (sizeBytes <= 0)
        {
            throw new DomainException("Risk attachment size must be greater than zero.");
        }

        SizeBytes = sizeBytes;
        Sha256Hash = Guard.AgainstNullOrWhiteSpace(sha256Hash, nameof(sha256Hash), 128);
        UploadedByUserId = Guard.AgainstEmpty(uploadedByUserId, nameof(uploadedByUserId));
        UploadedAtUtc = uploadedAtUtc;
    }

    public Guid RiskId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public string Sha256Hash { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
}

public sealed class RiskIndicator : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskIndicator()
    {
        Name = string.Empty;
    }

    public RiskIndicator(Guid tenantId, Guid riskId, string name, decimal value, decimal threshold)
        : base(tenantId)
    {
        RiskId = Guard.AgainstEmpty(riskId, nameof(riskId));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Value = value;
        Threshold = threshold;
        IsBreached = value >= threshold;
    }

    public Guid RiskId { get; private set; }
    public string Name { get; private set; }
    public decimal Value { get; private set; }
    public decimal Threshold { get; private set; }
    public bool IsBreached { get; private set; }
}

public sealed class RiskHistory : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private RiskHistory()
    {
        Action = string.Empty;
    }

    public RiskHistory(Guid tenantId, Guid riskId, string action, Guid userId, DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        RiskId = Guard.AgainstEmpty(riskId, nameof(riskId));
        Action = Guard.AgainstNullOrWhiteSpace(action, nameof(action), 1_200);
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid RiskId { get; private set; }
    public string Action { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
