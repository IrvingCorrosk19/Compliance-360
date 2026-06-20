using System.Diagnostics.CodeAnalysis;
using Compliance360.Domain.Common;

namespace Compliance360.Domain.QualityIndicators;

public enum IndicatorType
{
    Iso9001 = 0,
    Bpm = 1,
    Haccp = 2,
    Operational = 3,
    Strategic = 4,
    Process = 5,
    Supplier = 6,
    Audit = 7,
    Capa = 8,
    Risk = 9
}

public enum IndicatorFrequency
{
    Monthly = 0,
    Quarterly = 1,
    SemiAnnual = 2,
    Annual = 3
}

public enum IndicatorCalculationType
{
    Accumulated = 0,
    Average = 1,
    Percentage = 2,
    Ratio = 3,
    Custom = 4
}

public enum IndicatorStatus
{
    Draft = 0,
    Active = 1,
    PendingApproval = 2,
    Approved = 3,
    Archived = 4
}

public enum IndicatorResultStatus
{
    MissingData = 0,
    BelowTarget = 1,
    OnTarget = 2,
    AboveTarget = 3,
    CriticalDeviation = 4
}

public enum IndicatorAlertType
{
    TargetMissed = 0,
    TargetReached = 1,
    TargetExceeded = 2,
    NegativeTrend = 3,
    IndicatorOverdue = 4,
    MissingData = 5
}

public enum IndicatorTrendDirection
{
    Stable = 0,
    Positive = 1,
    Negative = 2
}

public sealed class IndicatorCategory : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorCategory()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public IndicatorCategory(Guid tenantId, string name, string code, Guid createdByUserId)
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

public sealed class QualityIndicator : TenantEntity
{
    private readonly List<IndicatorFormula> _formulas = [];
    private readonly List<IndicatorTarget> _targets = [];
    private readonly List<IndicatorThreshold> _thresholds = [];
    private readonly List<IndicatorMeasurement> _measurements = [];
    private readonly List<IndicatorResult> _results = [];
    private readonly List<IndicatorPeriod> _periods = [];
    private readonly List<IndicatorProcess> _processes = [];
    private readonly List<IndicatorAlert> _alerts = [];
    private readonly List<IndicatorTrend> _trends = [];
    private readonly List<IndicatorHistory> _history = [];
    private readonly List<IndicatorAttachment> _attachments = [];

    [ExcludeFromCodeCoverage]
    private QualityIndicator()
    {
        Name = string.Empty;
        Code = string.Empty;
        Description = string.Empty;
        Unit = string.Empty;
    }

    public QualityIndicator(Guid tenantId, Guid categoryId, string name, string code, string description, IndicatorType type, IndicatorFrequency frequency, IndicatorCalculationType calculationType, string unit, Guid? supplierId, Guid? auditId, Guid? capaId, Guid? riskId, Guid? documentId, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        CategoryId = Guard.AgainstEmpty(categoryId, nameof(categoryId));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 220);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 100).ToUpperInvariant();
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 2_000);
        Type = type;
        Frequency = frequency;
        CalculationType = calculationType;
        Unit = Guard.AgainstNullOrWhiteSpace(unit, nameof(unit), 40);
        SupplierId = supplierId;
        AuditId = auditId;
        CapaId = capaId;
        RiskId = riskId;
        DocumentId = documentId;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        Status = IndicatorStatus.Draft;
        CreatedAtUtc = createdAtUtc;
        AddHistory("Indicator created.", createdByUserId, createdAtUtc);
    }

    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public IndicatorType Type { get; private set; }
    public IndicatorFrequency Frequency { get; private set; }
    public IndicatorCalculationType CalculationType { get; private set; }
    public string Unit { get; private set; }
    public IndicatorStatus Status { get; private set; }
    public Guid? SupplierId { get; private set; }
    public Guid? AuditId { get; private set; }
    public Guid? CapaId { get; private set; }
    public Guid? RiskId { get; private set; }
    public Guid? DocumentId { get; private set; }
    public Guid? WorkflowInstanceId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public IReadOnlyCollection<IndicatorFormula> Formulas => _formulas.AsReadOnly();
    public IReadOnlyCollection<IndicatorTarget> Targets => _targets.AsReadOnly();
    public IReadOnlyCollection<IndicatorThreshold> Thresholds => _thresholds.AsReadOnly();
    public IReadOnlyCollection<IndicatorMeasurement> Measurements => _measurements.AsReadOnly();
    public IReadOnlyCollection<IndicatorResult> Results => _results.AsReadOnly();
    public IReadOnlyCollection<IndicatorPeriod> Periods => _periods.AsReadOnly();
    public IReadOnlyCollection<IndicatorProcess> Processes => _processes.AsReadOnly();
    public IReadOnlyCollection<IndicatorAlert> Alerts => _alerts.AsReadOnly();
    public IReadOnlyCollection<IndicatorTrend> Trends => _trends.AsReadOnly();
    public IReadOnlyCollection<IndicatorHistory> History => _history.AsReadOnly();
    public IReadOnlyCollection<IndicatorAttachment> Attachments => _attachments.AsReadOnly();

    public void Activate(Guid userId, DateTimeOffset occurredAtUtc)
    {
        Status = IndicatorStatus.Active;
        AddHistory("Indicator activated.", userId, occurredAtUtc);
    }

    public IndicatorFormula DefineFormula(string expression, IndicatorCalculationType calculationType, Guid userId, DateTimeOffset occurredAtUtc)
    {
        var formula = new IndicatorFormula(TenantId, Id, expression, calculationType);
        _formulas.Add(formula);
        CalculationType = calculationType;
        AddHistory("Formula defined.", userId, occurredAtUtc);
        return formula;
    }

    public IndicatorTarget DefineTarget(decimal targetValue, DateTimeOffset effectiveFromUtc, Guid userId, DateTimeOffset occurredAtUtc)
    {
        var target = new IndicatorTarget(TenantId, Id, targetValue, effectiveFromUtc);
        _targets.Add(target);
        AddHistory("Target defined.", userId, occurredAtUtc);
        return target;
    }

    public IndicatorThreshold DefineThreshold(decimal warningMinimum, decimal criticalMinimum, decimal excellentMinimum, Guid userId, DateTimeOffset occurredAtUtc)
    {
        if (criticalMinimum > warningMinimum || warningMinimum > excellentMinimum)
        {
            throw new DomainException("Indicator thresholds must follow critical <= warning <= excellent.");
        }

        var threshold = new IndicatorThreshold(TenantId, Id, warningMinimum, criticalMinimum, excellentMinimum);
        _thresholds.Add(threshold);
        AddHistory("Threshold defined.", userId, occurredAtUtc);
        return threshold;
    }

    public IndicatorPeriod AddPeriod(int year, int periodNumber, DateTimeOffset startUtc, DateTimeOffset endUtc, Guid userId, DateTimeOffset occurredAtUtc)
    {
        if (endUtc <= startUtc)
        {
            throw new DomainException("Indicator period end date must be after start date.");
        }

        var period = new IndicatorPeriod(TenantId, Id, year, periodNumber, startUtc, endUtc);
        _periods.Add(period);
        AddHistory("Period added.", userId, occurredAtUtc);
        return period;
    }

    public IndicatorProcess AssociateProcess(string processName, string area, Guid userId, DateTimeOffset occurredAtUtc)
    {
        var process = new IndicatorProcess(TenantId, Id, processName, area);
        _processes.Add(process);
        AddHistory("Process associated.", userId, occurredAtUtc);
        return process;
    }

    public IndicatorMeasurement CaptureMeasurement(Guid periodId, decimal numerator, decimal? denominator, bool isAutomatic, Guid capturedByUserId, DateTimeOffset capturedAtUtc)
    {
        EnsurePeriod(periodId);
        var measurement = new IndicatorMeasurement(TenantId, Id, periodId, numerator, denominator, isAutomatic, capturedByUserId, capturedAtUtc);
        _measurements.Add(measurement);
        AddHistory(isAutomatic ? "Automatic measurement captured." : "Manual measurement captured.", capturedByUserId, capturedAtUtc);
        return measurement;
    }

    public IndicatorResult CalculateResult(Guid periodId, Guid measurementId, Guid userId, DateTimeOffset occurredAtUtc)
    {
        var measurement = _measurements.FirstOrDefault(item => item.Id == measurementId && item.PeriodId == periodId);
        if (measurement is null)
        {
            throw new DomainException("Indicator measurement not found.");
        }

        var target = _targets.OrderByDescending(item => item.EffectiveFromUtc).FirstOrDefault();
        if (target is null)
        {
            throw new DomainException("Indicator target is required before calculating results.");
        }

        var value = CalculateValue(measurement);
        var status = ResolveStatus(value, target.TargetValue);
        var result = new IndicatorResult(TenantId, Id, periodId, measurementId, value, target.TargetValue, status);
        _results.Add(result);
        CreateAlertForResult(result, userId, occurredAtUtc);
        AddTrend(periodId, value, userId, occurredAtUtc);
        AddHistory("Result calculated.", userId, occurredAtUtc);
        return result;
    }

    public IndicatorAttachment AddAttachment(Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
    {
        var attachment = new IndicatorAttachment(TenantId, Id, storedFileId, fileName, contentType, sizeBytes, sha256Hash, uploadedByUserId, uploadedAtUtc);
        _attachments.Add(attachment);
        AddHistory("Attachment added.", uploadedByUserId, uploadedAtUtc);
        return attachment;
    }

    public void AttachWorkflow(Guid workflowInstanceId, Guid userId, DateTimeOffset occurredAtUtc)
    {
        WorkflowInstanceId = Guard.AgainstEmpty(workflowInstanceId, nameof(workflowInstanceId));
        AddHistory("Workflow attached.", userId, occurredAtUtc);
    }

    public void Approve(Guid userId, DateTimeOffset occurredAtUtc)
    {
        if (Status != IndicatorStatus.Active && Status != IndicatorStatus.PendingApproval)
        {
            throw new DomainException("Only active or pending indicators can be approved.");
        }

        Status = IndicatorStatus.Approved;
        AddHistory("Indicator approved.", userId, occurredAtUtc);
    }

    public IndicatorDashboard Dashboard()
    {
        var latest = _results.OrderByDescending(result => result.CreatedAtUtc).FirstOrDefault();
        var critical = _results.Count(result => result.Status == IndicatorResultStatus.CriticalDeviation || result.Status == IndicatorResultStatus.BelowTarget);
        var compliance = _results.Count == 0 ? 100 : (int)Math.Round(_results.Count(result => result.Status is IndicatorResultStatus.OnTarget or IndicatorResultStatus.AboveTarget) * 100m / _results.Count);
        return new IndicatorDashboard(TenantId, Id, latest?.Value ?? 0, latest?.TargetValue ?? 0, compliance, critical, _trends.LastOrDefault()?.Direction ?? IndicatorTrendDirection.Stable);
    }

    private decimal CalculateValue(IndicatorMeasurement measurement)
    {
        return CalculationType switch
        {
            IndicatorCalculationType.Percentage => RequireDenominator(measurement) == 0 ? 0 : Math.Round(measurement.Numerator * 100 / RequireDenominator(measurement), 4),
            IndicatorCalculationType.Ratio => RequireDenominator(measurement) == 0 ? 0 : Math.Round(measurement.Numerator / RequireDenominator(measurement), 4),
            IndicatorCalculationType.Average => measurement.Denominator.HasValue && measurement.Denominator.Value > 0 ? Math.Round(measurement.Numerator / measurement.Denominator.Value, 4) : measurement.Numerator,
            _ => measurement.Numerator
        };
    }

    private static decimal RequireDenominator(IndicatorMeasurement measurement) => measurement.Denominator ?? throw new DomainException("Indicator denominator is required for this calculation.");

    private IndicatorResultStatus ResolveStatus(decimal value, decimal target)
    {
        var threshold = _thresholds.OrderByDescending(item => item.CreatedAtUtc).FirstOrDefault();
        if (threshold is not null && value < threshold.CriticalMinimum)
        {
            return IndicatorResultStatus.CriticalDeviation;
        }

        if (value < target)
        {
            return IndicatorResultStatus.BelowTarget;
        }

        if (value == target)
        {
            return IndicatorResultStatus.OnTarget;
        }

        return IndicatorResultStatus.AboveTarget;
    }

    private void CreateAlertForResult(IndicatorResult result, Guid userId, DateTimeOffset occurredAtUtc)
    {
        var type = result.Status switch
        {
            IndicatorResultStatus.BelowTarget or IndicatorResultStatus.CriticalDeviation => IndicatorAlertType.TargetMissed,
            IndicatorResultStatus.OnTarget => IndicatorAlertType.TargetReached,
            IndicatorResultStatus.AboveTarget => IndicatorAlertType.TargetExceeded,
            _ => IndicatorAlertType.MissingData
        };
        _alerts.Add(new IndicatorAlert(TenantId, Id, result.Id, type, userId, occurredAtUtc));
    }

    private void AddTrend(Guid periodId, decimal value, Guid userId, DateTimeOffset occurredAtUtc)
    {
        var previous = _results.Where(result => result.PeriodId != periodId).OrderByDescending(result => result.CreatedAtUtc).FirstOrDefault();
        var direction = previous is null || previous.Value == value ? IndicatorTrendDirection.Stable : value > previous.Value ? IndicatorTrendDirection.Positive : IndicatorTrendDirection.Negative;
        _trends.Add(new IndicatorTrend(TenantId, Id, periodId, direction, value, previous?.Value, userId, occurredAtUtc));
        if (direction == IndicatorTrendDirection.Negative)
        {
            _alerts.Add(new IndicatorAlert(TenantId, Id, null, IndicatorAlertType.NegativeTrend, userId, occurredAtUtc));
        }
    }

    private void EnsurePeriod(Guid periodId)
    {
        if (_periods.All(period => period.Id != periodId))
        {
            throw new DomainException("Indicator period not found.");
        }
    }

    private void AddHistory(string action, Guid userId, DateTimeOffset occurredAtUtc)
    {
        _history.Add(new IndicatorHistory(TenantId, Id, action, userId, occurredAtUtc));
    }
}

public sealed record IndicatorDashboard(Guid TenantId, Guid IndicatorId, decimal LatestValue, decimal TargetValue, int CompliancePercent, int CriticalResults, IndicatorTrendDirection Trend);

public sealed class IndicatorFormula : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorFormula()
    {
        Expression = string.Empty;
    }

    public IndicatorFormula(Guid tenantId, Guid indicatorId, string expression, IndicatorCalculationType calculationType)
        : base(tenantId)
    {
        IndicatorId = Guard.AgainstEmpty(indicatorId, nameof(indicatorId));
        Expression = Guard.AgainstNullOrWhiteSpace(expression, nameof(expression), 1_000);
        CalculationType = calculationType;
    }

    public Guid IndicatorId { get; private set; }
    public string Expression { get; private set; }
    public IndicatorCalculationType CalculationType { get; private set; }
}

public sealed class IndicatorTarget : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorTarget()
    {
    }

    public IndicatorTarget(Guid tenantId, Guid indicatorId, decimal targetValue, DateTimeOffset effectiveFromUtc)
        : base(tenantId)
    {
        IndicatorId = Guard.AgainstEmpty(indicatorId, nameof(indicatorId));
        TargetValue = targetValue;
        EffectiveFromUtc = effectiveFromUtc;
    }

    public Guid IndicatorId { get; private set; }
    public decimal TargetValue { get; private set; }
    public DateTimeOffset EffectiveFromUtc { get; private set; }
}

public sealed class IndicatorThreshold : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorThreshold()
    {
    }

    public IndicatorThreshold(Guid tenantId, Guid indicatorId, decimal warningMinimum, decimal criticalMinimum, decimal excellentMinimum)
        : base(tenantId)
    {
        IndicatorId = Guard.AgainstEmpty(indicatorId, nameof(indicatorId));
        WarningMinimum = warningMinimum;
        CriticalMinimum = criticalMinimum;
        ExcellentMinimum = excellentMinimum;
    }

    public Guid IndicatorId { get; private set; }
    public decimal WarningMinimum { get; private set; }
    public decimal CriticalMinimum { get; private set; }
    public decimal ExcellentMinimum { get; private set; }
}

public sealed class IndicatorPeriod : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorPeriod()
    {
    }

    public IndicatorPeriod(Guid tenantId, Guid indicatorId, int year, int periodNumber, DateTimeOffset startUtc, DateTimeOffset endUtc)
        : base(tenantId)
    {
        IndicatorId = Guard.AgainstEmpty(indicatorId, nameof(indicatorId));
        Year = Guard.AgainstOutOfRange(year, nameof(year), 2000, 2100);
        PeriodNumber = Guard.AgainstOutOfRange(periodNumber, nameof(periodNumber), 1, 12);
        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    public Guid IndicatorId { get; private set; }
    public int Year { get; private set; }
    public int PeriodNumber { get; private set; }
    public DateTimeOffset StartUtc { get; private set; }
    public DateTimeOffset EndUtc { get; private set; }
}

public sealed class IndicatorProcess : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorProcess()
    {
        ProcessName = string.Empty;
        Area = string.Empty;
    }

    public IndicatorProcess(Guid tenantId, Guid indicatorId, string processName, string area)
        : base(tenantId)
    {
        IndicatorId = Guard.AgainstEmpty(indicatorId, nameof(indicatorId));
        ProcessName = Guard.AgainstNullOrWhiteSpace(processName, nameof(processName), 180);
        Area = Guard.AgainstNullOrWhiteSpace(area, nameof(area), 160);
    }

    public Guid IndicatorId { get; private set; }
    public string ProcessName { get; private set; }
    public string Area { get; private set; }
}

public sealed class IndicatorMeasurement : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorMeasurement()
    {
    }

    public IndicatorMeasurement(Guid tenantId, Guid indicatorId, Guid periodId, decimal numerator, decimal? denominator, bool isAutomatic, Guid capturedByUserId, DateTimeOffset capturedAtUtc)
        : base(tenantId)
    {
        IndicatorId = Guard.AgainstEmpty(indicatorId, nameof(indicatorId));
        PeriodId = Guard.AgainstEmpty(periodId, nameof(periodId));
        Numerator = numerator;
        Denominator = denominator;
        IsAutomatic = isAutomatic;
        CapturedByUserId = Guard.AgainstEmpty(capturedByUserId, nameof(capturedByUserId));
        CapturedAtUtc = capturedAtUtc;
    }

    public Guid IndicatorId { get; private set; }
    public Guid PeriodId { get; private set; }
    public decimal Numerator { get; private set; }
    public decimal? Denominator { get; private set; }
    public bool IsAutomatic { get; private set; }
    public Guid CapturedByUserId { get; private set; }
    public DateTimeOffset CapturedAtUtc { get; private set; }
}

public sealed class IndicatorResult : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorResult()
    {
    }

    public IndicatorResult(Guid tenantId, Guid indicatorId, Guid periodId, Guid measurementId, decimal value, decimal targetValue, IndicatorResultStatus status)
        : base(tenantId)
    {
        IndicatorId = Guard.AgainstEmpty(indicatorId, nameof(indicatorId));
        PeriodId = Guard.AgainstEmpty(periodId, nameof(periodId));
        MeasurementId = Guard.AgainstEmpty(measurementId, nameof(measurementId));
        Value = value;
        TargetValue = targetValue;
        Status = status;
    }

    public Guid IndicatorId { get; private set; }
    public Guid PeriodId { get; private set; }
    public Guid MeasurementId { get; private set; }
    public decimal Value { get; private set; }
    public decimal TargetValue { get; private set; }
    public IndicatorResultStatus Status { get; private set; }
}

public sealed class IndicatorAlert : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorAlert()
    {
    }

    public IndicatorAlert(Guid tenantId, Guid indicatorId, Guid? resultId, IndicatorAlertType type, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        IndicatorId = Guard.AgainstEmpty(indicatorId, nameof(indicatorId));
        ResultId = resultId;
        Type = type;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
        IsAcknowledged = false;
    }

    public Guid IndicatorId { get; private set; }
    public Guid? ResultId { get; private set; }
    public IndicatorAlertType Type { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public bool IsAcknowledged { get; private set; }
}

public sealed class IndicatorTrend : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorTrend()
    {
    }

    public IndicatorTrend(Guid tenantId, Guid indicatorId, Guid periodId, IndicatorTrendDirection direction, decimal value, decimal? previousValue, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        IndicatorId = Guard.AgainstEmpty(indicatorId, nameof(indicatorId));
        PeriodId = Guard.AgainstEmpty(periodId, nameof(periodId));
        Direction = direction;
        Value = value;
        PreviousValue = previousValue;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
    }

    public Guid IndicatorId { get; private set; }
    public Guid PeriodId { get; private set; }
    public IndicatorTrendDirection Direction { get; private set; }
    public decimal Value { get; private set; }
    public decimal? PreviousValue { get; private set; }
    public Guid CreatedByUserId { get; private set; }
}

public sealed class IndicatorAttachment : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorAttachment()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        Sha256Hash = string.Empty;
    }

    public IndicatorAttachment(Guid tenantId, Guid indicatorId, Guid storedFileId, string fileName, string contentType, long sizeBytes, string sha256Hash, Guid uploadedByUserId, DateTimeOffset uploadedAtUtc)
        : base(tenantId)
    {
        IndicatorId = Guard.AgainstEmpty(indicatorId, nameof(indicatorId));
        StoredFileId = Guard.AgainstEmpty(storedFileId, nameof(storedFileId));
        FileName = Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName), 260);
        ContentType = Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 120);
        if (sizeBytes <= 0)
        {
            throw new DomainException("Indicator attachment size must be greater than zero.");
        }

        SizeBytes = sizeBytes;
        Sha256Hash = Guard.AgainstNullOrWhiteSpace(sha256Hash, nameof(sha256Hash), 128);
        UploadedByUserId = Guard.AgainstEmpty(uploadedByUserId, nameof(uploadedByUserId));
        UploadedAtUtc = uploadedAtUtc;
    }

    public Guid IndicatorId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public string Sha256Hash { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAtUtc { get; private set; }
}

public sealed class IndicatorHistory : TenantEntity
{
    [ExcludeFromCodeCoverage]
    private IndicatorHistory()
    {
        Action = string.Empty;
    }

    public IndicatorHistory(Guid tenantId, Guid indicatorId, string action, Guid userId, DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        IndicatorId = Guard.AgainstEmpty(indicatorId, nameof(indicatorId));
        Action = Guard.AgainstNullOrWhiteSpace(action, nameof(action), 1_200);
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid IndicatorId { get; private set; }
    public string Action { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
