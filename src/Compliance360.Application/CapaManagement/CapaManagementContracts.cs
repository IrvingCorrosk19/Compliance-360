using Compliance360.Domain.Audit;
using Compliance360.Domain.CapaManagement;
using Compliance360.Shared;

namespace Compliance360.Application.CapaManagement;

public interface ICapaManagementService
{
    Task<Result<CapaSummary>> CreateAsync(CreateCapaCommand command, CancellationToken cancellationToken = default);
    Task<Result> ClassifyAsync(ClassifyCapaCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaOwnerSummary>> AssignOwnerAsync(AssignCapaOwnerCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaApproverSummary>> AddApproverAsync(AddCapaApproverCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaRootCauseSummary>> DefineRootCauseAsync(DefineCapaRootCauseCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaCauseAnalysisSummary>> AddFiveWhyAsync(AddCapaFiveWhyCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaCauseAnalysisSummary>> AddIshikawaAsync(AddCapaIshikawaCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaActionSummary>> AddContainmentActionAsync(AddCapaActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaActionSummary>> AddCorrectiveActionAsync(AddCapaActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaActionSummary>> AddPreventiveActionAsync(AddCapaActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaEvidenceSummary>> AddEvidenceAsync(AddCapaEvidenceCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaAttachmentSummary>> AddAttachmentAsync(AddCapaAttachmentCommand command, CancellationToken cancellationToken = default);
    Task<Result> RegisterFollowUpAsync(CapaFollowUpCommand command, CancellationToken cancellationToken = default);
    Task<Result> EscalateOverdueAsync(CapaActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaEffectivenessCheckSummary>> VerifyEffectivenessAsync(VerifyCapaEffectivenessCommand command, CancellationToken cancellationToken = default);
    Task<Result> AttachWorkflowAsync(AttachCapaWorkflowCommand command, CancellationToken cancellationToken = default);
    Task<Result> ApproveClosureAsync(CapaActionCommand command, CancellationToken cancellationToken = default);
    Task<Result> ReopenAsync(ReopenCapaCommand command, CancellationToken cancellationToken = default);
    Task<Result<CapaSearchResult>> SearchAsync(CapaSearchQuery query, CancellationToken cancellationToken = default);
    Task<Result<CapaDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<CapaExportDescriptor>> ExportAsync(CapaExportQuery query, CancellationToken cancellationToken = default);
}

public interface ICapaManagementRepository
{
    Task AddAsync(Capa capa, CancellationToken cancellationToken = default);
    Task<Capa?> GetAsync(Guid tenantId, Guid capaId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task<CapaSearchResult> SearchAsync(CapaSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<CapaDashboardDto> GetDashboardAsync(Guid tenantId, DateTimeOffset now, CancellationToken cancellationToken = default);
    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateCapaCommand(
    Guid TenantId,
    string Title,
    string Code,
    string Description,
    CapaPriority Priority,
    CapaRiskLevel RiskLevel,
    CapaSourceType SourceType,
    Guid? SourceEntityId,
    Guid? SupplierId,
    Guid? DocumentId,
    Guid? AuditId,
    Guid RequestedByUserId);

public sealed record ClassifyCapaCommand(Guid TenantId, Guid CapaId, CapaPriority Priority, CapaRiskLevel RiskLevel, DateTimeOffset? CommitmentDueAtUtc, Guid RequestedByUserId);
public sealed record AssignCapaOwnerCommand(Guid TenantId, Guid CapaId, Guid OwnerUserId, DateTimeOffset DueAtUtc, Guid RequestedByUserId);
public sealed record AddCapaApproverCommand(Guid TenantId, Guid CapaId, Guid ApproverUserId, Guid RequestedByUserId);
public sealed record DefineCapaRootCauseCommand(Guid TenantId, Guid CapaId, string Description, CapaRootCauseMethod Method, Guid RequestedByUserId);
public sealed record AddCapaFiveWhyCommand(Guid TenantId, Guid CapaId, string Why1, string Why2, string Why3, string Why4, string Why5, Guid RequestedByUserId);
public sealed record AddCapaIshikawaCommand(Guid TenantId, Guid CapaId, string People, string Process, string Equipment, string Material, string Environment, string Measurement, Guid RequestedByUserId);
public sealed record AddCapaActionCommand(Guid TenantId, Guid CapaId, string Description, Guid ResponsibleUserId, DateTimeOffset DueAtUtc, Guid RequestedByUserId);
public sealed record AddCapaEvidenceCommand(Guid TenantId, Guid CapaId, Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash, Guid RequestedByUserId);
public sealed record AddCapaAttachmentCommand(Guid TenantId, Guid CapaId, Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash, Guid RequestedByUserId);
public sealed record CapaFollowUpCommand(Guid TenantId, Guid CapaId, string Notes, Guid RequestedByUserId);
public sealed record VerifyCapaEffectivenessCommand(Guid TenantId, Guid CapaId, bool IsEffective, string VerificationSummary, Guid RequestedByUserId);
public sealed record AttachCapaWorkflowCommand(Guid TenantId, Guid CapaId, Guid WorkflowInstanceId, Guid RequestedByUserId);
public sealed record CapaActionCommand(Guid TenantId, Guid CapaId, Guid RequestedByUserId);
public sealed record ReopenCapaCommand(Guid TenantId, Guid CapaId, string Reason, Guid RequestedByUserId);
public sealed record CapaSearchQuery(Guid TenantId, string? SearchText, CapaStatus? Status, CapaPriority? Priority, CapaRiskLevel? RiskLevel, Guid? OwnerUserId, Guid? SupplierId, Guid? AuditId, int Page, int PageSize);
public sealed record CapaSearchCriteria(Guid TenantId, string? SearchText, CapaStatus? Status, CapaPriority? Priority, CapaRiskLevel? RiskLevel, Guid? OwnerUserId, Guid? SupplierId, Guid? AuditId, int Page, int PageSize);
public sealed record CapaExportQuery(Guid TenantId, CapaStatus? Status, CapaPriority? Priority, CapaRiskLevel? RiskLevel, string Format, Guid RequestedByUserId);

public sealed record CapaSummary(Guid Id, Guid TenantId, string Title, string Code, CapaStatus Status, CapaPriority Priority, CapaRiskLevel RiskLevel, CapaSourceType SourceType, Guid? SupplierId, Guid? DocumentId, Guid? AuditId, DateTimeOffset? CommitmentDueAtUtc, DateTimeOffset? ClosedAtUtc);
public sealed record CapaOwnerSummary(Guid Id, Guid CapaId, Guid UserId, DateTimeOffset DueAtUtc, bool IsActive);
public sealed record CapaApproverSummary(Guid Id, Guid CapaId, Guid UserId);
public sealed record CapaRootCauseSummary(Guid Id, Guid CapaId, string Description, CapaRootCauseMethod Method);
public sealed record CapaCauseAnalysisSummary(Guid Id, Guid CapaId, CapaRootCauseMethod Method);
public sealed record CapaActionSummary(Guid Id, Guid CapaId, string Description, Guid ResponsibleUserId, DateTimeOffset DueAtUtc, CapaActionType Type, CapaActionStatus Status);
public sealed record CapaEvidenceSummary(Guid Id, Guid CapaId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);
public sealed record CapaAttachmentSummary(Guid Id, Guid CapaId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);
public sealed record CapaEffectivenessCheckSummary(Guid Id, Guid CapaId, bool IsEffective, string VerificationSummary, Guid VerifiedByUserId);
public sealed record CapaSearchResult(IReadOnlyCollection<CapaSummary> Items, int TotalCount, int Page, int PageSize);
public sealed record CapaDashboardDto(int OpenCapas, int OverdueCapas, int CriticalCapas, int CapasByOwner, int CapasBySupplier, int CapasByAudit, decimal AverageClosureDays, int EffectivenessPercent, int RecurrenceCount);
public sealed record CapaExportDescriptor(Guid TenantId, string Format, string FileName, string ContentType, int TotalRecords);
