using Compliance360.Domain.Audit;
using Compliance360.Domain.RiskManagement;
using Compliance360.Shared;

namespace Compliance360.Application.RiskManagement;

public interface IRiskManagementService
{
    Task<Result<RiskCategorySummary>> CreateCategoryAsync(CreateRiskCategoryCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskMatrixSummary>> CreateMatrixAsync(CreateRiskMatrixCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskSummary>> CreateRiskAsync(CreateRiskCommand command, CancellationToken cancellationToken = default);
    Task<Result> ClassifyAsync(ClassifyRiskCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskOwnerSummary>> AssignOwnerAsync(AssignRiskOwnerCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskAssessmentSummary>> AssessAsync(AssessRiskCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskTreatmentSummary>> AddTreatmentAsync(AddRiskTreatmentCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskMitigationPlanSummary>> AddMitigationPlanAsync(AddRiskMitigationPlanCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskControlSummary>> AddControlAsync(AddRiskControlCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskEvidenceSummary>> AddEvidenceAsync(AddRiskEvidenceCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskAttachmentSummary>> AddAttachmentAsync(AddRiskAttachmentCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskReviewSummary>> ScheduleReviewAsync(ScheduleRiskReviewCommand command, CancellationToken cancellationToken = default);
    Task<Result> CompleteReviewAsync(CompleteRiskReviewCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskIndicatorSummary>> AddIndicatorAsync(AddRiskIndicatorCommand command, CancellationToken cancellationToken = default);
    Task<Result> EscalateCriticalAsync(RiskActionCommand command, CancellationToken cancellationToken = default);
    Task<Result> AttachWorkflowAsync(AttachRiskWorkflowCommand command, CancellationToken cancellationToken = default);
    Task<Result> CloseAsync(RiskActionCommand command, CancellationToken cancellationToken = default);
    Task<Result> ReopenAsync(ReopenRiskCommand command, CancellationToken cancellationToken = default);
    Task<Result<RiskSearchResult>> SearchAsync(RiskSearchQuery query, CancellationToken cancellationToken = default);
    Task<Result<RiskDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<RiskHeatMapPoint>>> GetHeatMapAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<RiskExportDescriptor>> ExportAsync(RiskExportQuery query, CancellationToken cancellationToken = default);
}

public interface IRiskManagementRepository
{
    Task AddCategoryAsync(RiskCategory category, CancellationToken cancellationToken = default);
    Task<RiskCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> CategoryCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task AddMatrixAsync(RiskMatrix matrix, CancellationToken cancellationToken = default);
    Task<RiskMatrix?> GetMatrixAsync(Guid tenantId, Guid matrixId, CancellationToken cancellationToken = default);
    Task AddRiskAsync(Risk risk, CancellationToken cancellationToken = default);
    Task<Risk?> GetRiskAsync(Guid tenantId, Guid riskId, CancellationToken cancellationToken = default);
    Task<bool> RiskCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task<RiskSearchResult> SearchAsync(RiskSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<RiskDashboardDto> GetDashboardAsync(Guid tenantId, DateTimeOffset now, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RiskHeatMapPoint>> GetHeatMapAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateRiskCategoryCommand(Guid TenantId, string Name, string Code, Guid RequestedByUserId);
public sealed record CreateRiskMatrixCommand(Guid TenantId, string Name, int ToleranceScore, Guid RequestedByUserId);
public sealed record CreateRiskCommand(Guid TenantId, Guid CategoryId, string Title, string Code, string Description, RiskType Type, string Area, string Process, Guid? SupplierId, Guid? DocumentId, Guid? AuditId, Guid? CapaId, Guid RequestedByUserId);
public sealed record ClassifyRiskCommand(Guid TenantId, Guid RiskId, RiskType Type, string Area, string Process, Guid RequestedByUserId);
public sealed record AssignRiskOwnerCommand(Guid TenantId, Guid RiskId, Guid OwnerUserId, Guid RequestedByUserId);
public sealed record AssessRiskCommand(Guid TenantId, Guid RiskId, RiskProbability Probability, RiskImpact Impact, RiskProbability ResidualProbability, RiskImpact ResidualImpact, int ToleranceScore, Guid RequestedByUserId);
public sealed record AddRiskTreatmentCommand(Guid TenantId, Guid RiskId, RiskTreatmentStrategy Strategy, string Rationale, Guid RequestedByUserId);
public sealed record AddRiskMitigationPlanCommand(Guid TenantId, Guid RiskId, string Description, Guid ResponsibleUserId, DateTimeOffset DueAtUtc, Guid RequestedByUserId);
public sealed record AddRiskControlCommand(Guid TenantId, Guid RiskId, string Name, RiskControlType Type, string Description, bool IsEffective, Guid RequestedByUserId);
public sealed record AddRiskEvidenceCommand(Guid TenantId, Guid RiskId, Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash, Guid RequestedByUserId);
public sealed record AddRiskAttachmentCommand(Guid TenantId, Guid RiskId, Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash, Guid RequestedByUserId);
public sealed record ScheduleRiskReviewCommand(Guid TenantId, Guid RiskId, DateTimeOffset DueAtUtc, Guid RequestedByUserId);
public sealed record CompleteRiskReviewCommand(Guid TenantId, Guid RiskId, Guid ReviewId, string Summary, Guid RequestedByUserId);
public sealed record AddRiskIndicatorCommand(Guid TenantId, Guid RiskId, string Name, decimal Value, decimal Threshold, Guid RequestedByUserId);
public sealed record AttachRiskWorkflowCommand(Guid TenantId, Guid RiskId, Guid WorkflowInstanceId, Guid RequestedByUserId);
public sealed record RiskActionCommand(Guid TenantId, Guid RiskId, Guid RequestedByUserId);
public sealed record ReopenRiskCommand(Guid TenantId, Guid RiskId, string Reason, Guid RequestedByUserId);
public sealed record RiskSearchQuery(Guid TenantId, string? SearchText, RiskStatus? Status, RiskType? Type, RiskLevel? Level, string? Area, Guid? SupplierId, Guid? AuditId, Guid? CapaId, int Page, int PageSize);
public sealed record RiskSearchCriteria(Guid TenantId, string? SearchText, RiskStatus? Status, RiskType? Type, RiskLevel? Level, string? Area, Guid? SupplierId, Guid? AuditId, Guid? CapaId, int Page, int PageSize);
public sealed record RiskExportQuery(Guid TenantId, RiskStatus? Status, RiskType? Type, RiskLevel? Level, string Format, Guid RequestedByUserId);

public sealed record RiskCategorySummary(Guid Id, Guid TenantId, string Name, string Code);
public sealed record RiskMatrixSummary(Guid Id, Guid TenantId, string Name, int ToleranceScore);
public sealed record RiskSummary(Guid Id, Guid TenantId, string Title, string Code, RiskType Type, RiskStatus Status, RiskLevel InherentLevel, RiskLevel ResidualLevel, int InherentScore, int ResidualScore, string Area, string Process, Guid? SupplierId, Guid? AuditId, Guid? CapaId);
public sealed record RiskOwnerSummary(Guid Id, Guid RiskId, Guid UserId, bool IsActive);
public sealed record RiskAssessmentSummary(Guid Id, Guid RiskId, int InherentScore, RiskLevel InherentLevel, int ResidualScore, RiskLevel ResidualLevel, bool IsWithinTolerance);
public sealed record RiskTreatmentSummary(Guid Id, Guid RiskId, RiskTreatmentStrategy Strategy, string Rationale);
public sealed record RiskMitigationPlanSummary(Guid Id, Guid RiskId, string Description, Guid ResponsibleUserId, DateTimeOffset DueAtUtc, bool IsCompleted);
public sealed record RiskControlSummary(Guid Id, Guid RiskId, string Name, RiskControlType Type, bool IsEffective);
public sealed record RiskEvidenceSummary(Guid Id, Guid RiskId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);
public sealed record RiskAttachmentSummary(Guid Id, Guid RiskId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);
public sealed record RiskReviewSummary(Guid Id, Guid RiskId, DateTimeOffset DueAtUtc, RiskReviewStatus Status);
public sealed record RiskIndicatorSummary(Guid Id, Guid RiskId, string Name, decimal Value, decimal Threshold, bool IsBreached);
public sealed record RiskSearchResult(IReadOnlyCollection<RiskSummary> Items, int TotalCount, int Page, int PageSize);
public sealed record RiskDashboardDto(int CriticalRisks, int HighRisks, int MediumRisks, int LowRisks, int OverdueRisks, int RisksByArea, int RisksBySupplier, int RisksByProcess, int TrendTotalRisks, int HeatMapPoints);
public sealed record RiskExportDescriptor(Guid TenantId, string Format, string FileName, string ContentType, int TotalRecords);
