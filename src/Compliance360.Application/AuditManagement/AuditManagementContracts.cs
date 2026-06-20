using Compliance360.Domain.Audit;
using Compliance360.Domain.AuditManagement;
using Compliance360.Shared;

namespace Compliance360.Application.AuditManagement;

public interface IAuditManagementService
{
    Task<Result<AuditProgramSummary>> CreateProgramAsync(CreateAuditProgramCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditChecklistSummary>> CreateChecklistAsync(CreateAuditChecklistCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditChecklistItemSummary>> AddChecklistItemAsync(AddAuditChecklistItemCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditPlanSummary>> CreatePlanAsync(CreateAuditPlanCommand command, CancellationToken cancellationToken = default);
    Task<Result<ManagedAuditSummary>> CreateAuditAsync(CreateManagedAuditCommand command, CancellationToken cancellationToken = default);
    Task<Result> AssignChecklistAsync(AssignAuditChecklistCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditScheduleSummary>> ScheduleAsync(ScheduleAuditCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditParticipantSummary>> AddParticipantAsync(AddAuditParticipantCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditAreaSummary>> AddAreaAsync(AddAuditAreaCommand command, CancellationToken cancellationToken = default);
    Task<Result> StartAsync(ManagedAuditActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditFindingSummary>> AddFindingAsync(AddAuditFindingCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditEvidenceSummary>> AddEvidenceAsync(AddAuditEvidenceCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditObservationSummary>> AddObservationAsync(AddAuditObservationCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditNonConformitySummary>> AddNonConformityAsync(AddAuditNonConformityCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditRecommendationSummary>> AddRecommendationAsync(AddAuditRecommendationCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditCorrectiveActionLinkSummary>> LinkCorrectiveActionAsync(LinkAuditCorrectiveActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<AuditAttachmentSummary>> AddAttachmentAsync(AddAuditAttachmentCommand command, CancellationToken cancellationToken = default);
    Task<Result> CompleteAsync(ManagedAuditActionCommand command, CancellationToken cancellationToken = default);
    Task<Result> CloseAsync(ManagedAuditActionCommand command, CancellationToken cancellationToken = default);
    Task<Result> ReopenAsync(ManagedAuditActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<ManagedAuditSearchResult>> SearchAsync(ManagedAuditSearchQuery query, CancellationToken cancellationToken = default);
    Task<Result<AuditDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<AuditExportDescriptor>> ExportAsync(ManagedAuditExportQuery query, CancellationToken cancellationToken = default);
}

public interface IAuditManagementRepository
{
    Task AddProgramAsync(AuditProgram program, CancellationToken cancellationToken = default);
    Task<AuditProgram?> GetProgramAsync(Guid tenantId, Guid programId, CancellationToken cancellationToken = default);
    Task<bool> ProgramCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task AddChecklistAsync(AuditChecklist checklist, CancellationToken cancellationToken = default);
    Task<AuditChecklist?> GetChecklistAsync(Guid tenantId, Guid checklistId, CancellationToken cancellationToken = default);
    Task<bool> ChecklistCodeVersionExistsAsync(Guid tenantId, string code, int version, CancellationToken cancellationToken = default);
    Task AddPlanAsync(AuditPlan plan, CancellationToken cancellationToken = default);
    Task<AuditPlan?> GetPlanAsync(Guid tenantId, Guid planId, CancellationToken cancellationToken = default);
    Task AddAuditAsync(ManagedAudit audit, CancellationToken cancellationToken = default);
    Task<ManagedAudit?> GetAuditAsync(Guid tenantId, Guid auditId, CancellationToken cancellationToken = default);
    Task<bool> AuditCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task<ManagedAuditSearchResult> SearchAsync(ManagedAuditSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<AuditDashboardDto> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateAuditProgramCommand(Guid TenantId, string Name, string Code, int Year, Guid RequestedByUserId);
public sealed record CreateAuditChecklistCommand(Guid TenantId, string Name, string Code, AuditChecklistType Type, int Version, Guid RequestedByUserId);
public sealed record AddAuditChecklistItemCommand(Guid TenantId, Guid ChecklistId, string Clause, string Question, int Weight, Guid RequestedByUserId);
public sealed record CreateAuditPlanCommand(Guid TenantId, Guid AuditProgramId, string Scope, string Criteria, DateTimeOffset PlannedStartUtc, DateTimeOffset PlannedEndUtc, Guid RequestedByUserId);
public sealed record CreateManagedAuditCommand(Guid TenantId, Guid AuditProgramId, Guid AuditPlanId, string Title, string Code, ManagedAuditType Type, Guid RequestedByUserId);
public sealed record AssignAuditChecklistCommand(Guid TenantId, Guid AuditId, Guid ChecklistId, Guid RequestedByUserId);
public sealed record ScheduleAuditCommand(Guid TenantId, Guid AuditId, DateTimeOffset StartUtc, DateTimeOffset EndUtc, string Location, Guid RequestedByUserId);
public sealed record AddAuditParticipantCommand(Guid TenantId, Guid AuditId, Guid UserId, AuditParticipantRole Role, Guid RequestedByUserId);
public sealed record AddAuditAreaCommand(Guid TenantId, Guid AuditId, string Name, string Process, Guid RequestedByUserId);
public sealed record ManagedAuditActionCommand(Guid TenantId, Guid AuditId, Guid RequestedByUserId);
public sealed record AddAuditFindingCommand(Guid TenantId, Guid AuditId, string Title, string Description, AuditFindingSeverity Severity, Guid? ChecklistItemId, Guid RequestedByUserId);
public sealed record AddAuditEvidenceCommand(Guid TenantId, Guid AuditId, Guid FindingId, AuditEvidenceType Type, Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash, Guid RequestedByUserId);
public sealed record AddAuditObservationCommand(Guid TenantId, Guid AuditId, string Description, Guid RequestedByUserId);
public sealed record AddAuditNonConformityCommand(Guid TenantId, Guid AuditId, Guid FindingId, string Requirement, Guid RequestedByUserId);
public sealed record AddAuditRecommendationCommand(Guid TenantId, Guid AuditId, Guid FindingId, string Recommendation, Guid RequestedByUserId);
public sealed record LinkAuditCorrectiveActionCommand(Guid TenantId, Guid AuditId, Guid FindingId, Guid CorrectiveActionId, Guid RequestedByUserId);
public sealed record AddAuditAttachmentCommand(Guid TenantId, Guid AuditId, Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash, Guid RequestedByUserId);
public sealed record ManagedAuditSearchQuery(Guid TenantId, string? SearchText, ManagedAuditType? Type, ManagedAuditStatus? Status, int Page, int PageSize);
public sealed record ManagedAuditSearchCriteria(Guid TenantId, string? SearchText, ManagedAuditType? Type, ManagedAuditStatus? Status, int Page, int PageSize);
public sealed record ManagedAuditExportQuery(Guid TenantId, ManagedAuditType? Type, ManagedAuditStatus? Status, string Format, Guid RequestedByUserId);

public sealed record AuditProgramSummary(Guid Id, Guid TenantId, string Name, string Code, int Year);
public sealed record AuditChecklistSummary(Guid Id, Guid TenantId, string Name, string Code, AuditChecklistType Type, int Version);
public sealed record AuditChecklistItemSummary(Guid Id, Guid ChecklistId, string Clause, string Question, int Weight);
public sealed record AuditPlanSummary(Guid Id, Guid TenantId, Guid AuditProgramId, string Scope, string Criteria, DateTimeOffset PlannedStartUtc, DateTimeOffset PlannedEndUtc);
public sealed record ManagedAuditSummary(Guid Id, Guid TenantId, string Title, string Code, ManagedAuditType Type, ManagedAuditStatus Status, Guid? ChecklistId);
public sealed record AuditScheduleSummary(Guid Id, Guid AuditId, DateTimeOffset StartUtc, DateTimeOffset EndUtc, string Location);
public sealed record AuditParticipantSummary(Guid Id, Guid AuditId, Guid UserId, AuditParticipantRole Role);
public sealed record AuditAreaSummary(Guid Id, Guid AuditId, string Name, string Process);
public sealed record AuditFindingSummary(Guid Id, Guid AuditId, string Title, AuditFindingSeverity Severity);
public sealed record AuditEvidenceSummary(Guid Id, Guid AuditId, Guid FindingId, AuditEvidenceType Type, string FileName, string Sha256Hash);
public sealed record AuditObservationSummary(Guid Id, Guid AuditId, string Description);
public sealed record AuditNonConformitySummary(Guid Id, Guid AuditId, Guid FindingId, string Requirement);
public sealed record AuditRecommendationSummary(Guid Id, Guid AuditId, Guid FindingId, string Recommendation);
public sealed record AuditCorrectiveActionLinkSummary(Guid Id, Guid AuditId, Guid FindingId, Guid CorrectiveActionId);
public sealed record AuditAttachmentSummary(Guid Id, Guid AuditId, string FileName, string Sha256Hash);
public sealed record ManagedAuditSearchResult(IReadOnlyCollection<ManagedAuditSummary> Items, int TotalCount, int Page, int PageSize);
public sealed record AuditDashboardDto(int OpenAudits, int ClosedAudits, int CriticalFindings, int MajorFindings, int PendingActions, int ComplianceScore, int TrendTotalFindings);
public sealed record AuditExportDescriptor(Guid TenantId, string Format, string FileName, string ContentType, int TotalRecords);
