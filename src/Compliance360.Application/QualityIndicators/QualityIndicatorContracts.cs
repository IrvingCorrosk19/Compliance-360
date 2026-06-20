using Compliance360.Domain.Audit;
using Compliance360.Domain.QualityIndicators;
using Compliance360.Shared;

namespace Compliance360.Application.QualityIndicators;

public interface IQualityIndicatorService
{
    Task<Result<IndicatorCategorySummary>> CreateCategoryAsync(CreateIndicatorCategoryCommand command, CancellationToken cancellationToken = default);
    Task<Result<QualityIndicatorSummary>> CreateIndicatorAsync(CreateQualityIndicatorCommand command, CancellationToken cancellationToken = default);
    Task<Result> ActivateAsync(IndicatorActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<IndicatorFormulaSummary>> DefineFormulaAsync(DefineIndicatorFormulaCommand command, CancellationToken cancellationToken = default);
    Task<Result<IndicatorTargetSummary>> DefineTargetAsync(DefineIndicatorTargetCommand command, CancellationToken cancellationToken = default);
    Task<Result<IndicatorThresholdSummary>> DefineThresholdAsync(DefineIndicatorThresholdCommand command, CancellationToken cancellationToken = default);
    Task<Result<IndicatorPeriodSummary>> AddPeriodAsync(AddIndicatorPeriodCommand command, CancellationToken cancellationToken = default);
    Task<Result<IndicatorProcessSummary>> AssociateProcessAsync(AssociateIndicatorProcessCommand command, CancellationToken cancellationToken = default);
    Task<Result<IndicatorMeasurementSummary>> CaptureMeasurementAsync(CaptureIndicatorMeasurementCommand command, CancellationToken cancellationToken = default);
    Task<Result<IndicatorResultSummary>> CalculateResultAsync(CalculateIndicatorResultCommand command, CancellationToken cancellationToken = default);
    Task<Result<IndicatorAttachmentSummary>> AddAttachmentAsync(AddIndicatorAttachmentCommand command, CancellationToken cancellationToken = default);
    Task<Result> AttachWorkflowAsync(AttachIndicatorWorkflowCommand command, CancellationToken cancellationToken = default);
    Task<Result> ApproveAsync(IndicatorActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<IndicatorSearchResult>> SearchAsync(IndicatorSearchQuery query, CancellationToken cancellationToken = default);
    Task<Result<IndicatorDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<IndicatorTrendSummary>>> GetTrendsAsync(Guid tenantId, Guid? indicatorId, CancellationToken cancellationToken = default);
    Task<Result<IndicatorExportDescriptor>> ExportAsync(IndicatorExportQuery query, CancellationToken cancellationToken = default);
}

public interface IQualityIndicatorRepository
{
    Task AddCategoryAsync(IndicatorCategory category, CancellationToken cancellationToken = default);
    Task<IndicatorCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> CategoryCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task AddIndicatorAsync(QualityIndicator indicator, CancellationToken cancellationToken = default);
    Task<QualityIndicator?> GetIndicatorAsync(Guid tenantId, Guid indicatorId, CancellationToken cancellationToken = default);
    Task<bool> IndicatorCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task<IndicatorSearchResult> SearchAsync(IndicatorSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<IndicatorDashboardDto> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<IndicatorTrendSummary>> GetTrendsAsync(Guid tenantId, Guid? indicatorId, CancellationToken cancellationToken = default);
    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateIndicatorCategoryCommand(Guid TenantId, string Name, string Code, Guid RequestedByUserId);
public sealed record CreateQualityIndicatorCommand(Guid TenantId, Guid CategoryId, string Name, string Code, string Description, IndicatorType Type, IndicatorFrequency Frequency, IndicatorCalculationType CalculationType, string Unit, Guid? SupplierId, Guid? AuditId, Guid? CapaId, Guid? RiskId, Guid? DocumentId, Guid RequestedByUserId);
public sealed record IndicatorActionCommand(Guid TenantId, Guid IndicatorId, Guid RequestedByUserId);
public sealed record DefineIndicatorFormulaCommand(Guid TenantId, Guid IndicatorId, string Expression, IndicatorCalculationType CalculationType, Guid RequestedByUserId);
public sealed record DefineIndicatorTargetCommand(Guid TenantId, Guid IndicatorId, decimal TargetValue, DateTimeOffset EffectiveFromUtc, Guid RequestedByUserId);
public sealed record DefineIndicatorThresholdCommand(Guid TenantId, Guid IndicatorId, decimal WarningMinimum, decimal CriticalMinimum, decimal ExcellentMinimum, Guid RequestedByUserId);
public sealed record AddIndicatorPeriodCommand(Guid TenantId, Guid IndicatorId, int Year, int PeriodNumber, DateTimeOffset StartUtc, DateTimeOffset EndUtc, Guid RequestedByUserId);
public sealed record AssociateIndicatorProcessCommand(Guid TenantId, Guid IndicatorId, string ProcessName, string Area, Guid RequestedByUserId);
public sealed record CaptureIndicatorMeasurementCommand(Guid TenantId, Guid IndicatorId, Guid PeriodId, decimal Numerator, decimal? Denominator, bool IsAutomatic, Guid RequestedByUserId);
public sealed record CalculateIndicatorResultCommand(Guid TenantId, Guid IndicatorId, Guid PeriodId, Guid MeasurementId, Guid RequestedByUserId);
public sealed record AddIndicatorAttachmentCommand(Guid TenantId, Guid IndicatorId, Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash, Guid RequestedByUserId);
public sealed record AttachIndicatorWorkflowCommand(Guid TenantId, Guid IndicatorId, Guid WorkflowInstanceId, Guid RequestedByUserId);
public sealed record IndicatorSearchQuery(Guid TenantId, string? SearchText, IndicatorStatus? Status, IndicatorType? Type, IndicatorFrequency? Frequency, Guid? SupplierId, Guid? AuditId, Guid? CapaId, Guid? RiskId, int Page, int PageSize);
public sealed record IndicatorSearchCriteria(Guid TenantId, string? SearchText, IndicatorStatus? Status, IndicatorType? Type, IndicatorFrequency? Frequency, Guid? SupplierId, Guid? AuditId, Guid? CapaId, Guid? RiskId, int Page, int PageSize);
public sealed record IndicatorExportQuery(Guid TenantId, IndicatorStatus? Status, IndicatorType? Type, string Format, Guid RequestedByUserId);

public sealed record IndicatorCategorySummary(Guid Id, Guid TenantId, string Name, string Code);
public sealed record QualityIndicatorSummary(Guid Id, Guid TenantId, string Name, string Code, IndicatorType Type, IndicatorFrequency Frequency, IndicatorCalculationType CalculationType, IndicatorStatus Status, string Unit);
public sealed record IndicatorFormulaSummary(Guid Id, Guid IndicatorId, string Expression, IndicatorCalculationType CalculationType);
public sealed record IndicatorTargetSummary(Guid Id, Guid IndicatorId, decimal TargetValue, DateTimeOffset EffectiveFromUtc);
public sealed record IndicatorThresholdSummary(Guid Id, Guid IndicatorId, decimal WarningMinimum, decimal CriticalMinimum, decimal ExcellentMinimum);
public sealed record IndicatorPeriodSummary(Guid Id, Guid IndicatorId, int Year, int PeriodNumber, DateTimeOffset StartUtc, DateTimeOffset EndUtc);
public sealed record IndicatorProcessSummary(Guid Id, Guid IndicatorId, string ProcessName, string Area);
public sealed record IndicatorMeasurementSummary(Guid Id, Guid IndicatorId, Guid PeriodId, decimal Numerator, decimal? Denominator, bool IsAutomatic);
public sealed record IndicatorResultSummary(Guid Id, Guid IndicatorId, Guid PeriodId, decimal Value, decimal TargetValue, IndicatorResultStatus Status);
public sealed record IndicatorAttachmentSummary(Guid Id, Guid IndicatorId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);
public sealed record IndicatorTrendSummary(Guid Id, Guid IndicatorId, Guid PeriodId, IndicatorTrendDirection Direction, decimal Value, decimal? PreviousValue);
public sealed record IndicatorSearchResult(IReadOnlyCollection<QualityIndicatorSummary> Items, int TotalCount, int Page, int PageSize);
public sealed record IndicatorDashboardDto(int TotalIndicators, int ApprovedIndicators, int CriticalIndicators, int Alerts, int CompliancePercent, int NegativeTrends, int TopCriticalProcesses);
public sealed record IndicatorExportDescriptor(Guid TenantId, string Format, string FileName, string ContentType, int TotalRecords);
