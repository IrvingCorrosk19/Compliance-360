using Compliance360.Domain.RegulatoryAffairs;
using Compliance360.Shared;

namespace Compliance360.Application.RegulatoryAffairs;

public interface IRegulatoryWorkflowV2Service
{
    Task<Result<WorkflowSnapshotV2Dto>> GetWorkflowSnapshotAsync(Guid tenantId, Guid dossierId, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> UpdateMetadataAsync(UpdateDossierMetadataV2Command command, CancellationToken ct = default);
    Task<Result<CorrectionRequestV2Dto>> ReturnForCorrectionAsync(ReturnForCorrectionV2Command command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> SubmitCorrectionAsync(SubmitCorrectionV2Command command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> StartTechnicalReviewAsync(StartTechnicalReviewV2Command command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> CompleteTechnicalReviewAsync(CompleteTechnicalReviewV2Command command, CancellationToken ct = default);
    Task<Result<EvidenceRevisionV2Dto>> AddEvidenceRevisionAsync(AddEvidenceRevisionV2Command command, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<EvidenceRevisionV2Dto>>> ListEvidenceVersionsAsync(Guid tenantId, Guid dossierId, Guid requirementId, CancellationToken ct = default);
    Task<Result<GovernanceRequestV2Dto>> RequestReopenAsync(RequestGovernanceV2Command command, CancellationToken ct = default);
    Task<Result<GovernanceRequestV2Dto>> ApproveReopenAsync(DecideGovernanceV2Command command, CancellationToken ct = default);
    Task<Result<GovernanceRequestV2Dto>> RejectReopenAsync(DecideGovernanceV2Command command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> ExecuteReopenAsync(ExecuteGovernanceV2Command command, CancellationToken ct = default);
    Task<Result<GovernanceRequestV2Dto>> RequestOverrideAsync(RequestOverrideV2Command command, CancellationToken ct = default);
    Task<Result<GovernanceRequestV2Dto>> ApproveOverrideAsync(DecideGovernanceV2Command command, CancellationToken ct = default);
    Task<Result<GovernanceRequestV2Dto>> RejectOverrideAsync(DecideGovernanceV2Command command, CancellationToken ct = default);
    Task<Result<GovernanceRequestV2Dto>> ConsumeOverrideAsync(ConsumeOverrideV2Command command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> CancelAsync(CancelDossierV2Command command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> ArchiveAsync(ArchiveDossierV2Command command, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<ChangeEventV2Dto>>> GetTimelineAsync(Guid tenantId, Guid dossierId, CancellationToken ct = default);
}

public interface IRegulatoryWorkflowV2Repository
{
    Task<RegistrationDossier?> GetDossierAsync(Guid tenantId, Guid dossierId, CancellationToken ct);
    Task AddCorrectionAsync(DossierCorrectionRequest entity, CancellationToken ct);
    Task<DossierCorrectionRequest?> GetOpenCorrectionAsync(Guid tenantId, Guid dossierId, CancellationToken ct);
    Task<DossierCorrectionRequest?> GetCorrectionAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task AddEvidenceAsync(DossierEvidenceRevision entity, CancellationToken ct);
    Task<IReadOnlyList<DossierEvidenceRevision>> ListEvidenceAsync(Guid tenantId, Guid dossierId, Guid requirementId, CancellationToken ct);
    Task AddReopenAsync(DossierReopenRequest entity, CancellationToken ct);
    Task<DossierReopenRequest?> GetReopenAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task AddOverrideAsync(DossierOverrideRequest entity, CancellationToken ct);
    Task<DossierOverrideRequest?> GetOverrideAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task AddChangeEventAsync(DossierChangeEvent entity, CancellationToken ct);
    Task<long> NextSequenceAsync(Guid tenantId, Guid dossierId, CancellationToken ct);
    Task<IReadOnlyList<DossierChangeEvent>> ListTimelineAsync(Guid tenantId, Guid dossierId, CancellationToken ct);
}

public sealed record UpdateDossierMetadataV2Command(Guid TenantId, Guid DossierId, long ExpectedRevision, string Reason, Guid RequestedByUserId,
    string? Priority, Guid? OwnerUserId, string? SalesMarketingInput, decimal? OpportunityAmount, string? Currency, string? Comments,
    DateTimeOffset? RequestedFromFactoryOn, DateTimeOffset? EstimatedReceptionOn, DateTimeOffset? MaximumReceptionOn,
    DateTimeOffset? EstimatedSubmissionOn, DateTimeOffset? EstimatedApprovalOn, DateTimeOffset? TargetExpirationOn,
    Guid? CorrectionRequestId = null, string? ActorRole = null, string? CorrelationId = null);
public sealed record ReturnForCorrectionV2Command(Guid TenantId, Guid DossierId, long ExpectedRevision, string Reason,
    DossierCorrectionSeverity Severity, IReadOnlyCollection<Guid> RequirementIds, IReadOnlyCollection<string>? FieldPaths,
    IReadOnlyCollection<Guid>? DocumentIds, Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);
public sealed record SubmitCorrectionV2Command(Guid TenantId, Guid DossierId, Guid CorrectionRequestId, long ExpectedRevision,
    IReadOnlyCollection<Guid> RequirementIds, IReadOnlyCollection<string>? FieldPaths, IReadOnlyCollection<Guid>? DocumentIds,
    string Reason, Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);
public sealed record StartTechnicalReviewV2Command(Guid TenantId, Guid DossierId, long ExpectedRevision, string Reason,
    Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);
public sealed record CompleteTechnicalReviewV2Command(Guid TenantId, Guid DossierId, Guid? CorrectionRequestId, long ExpectedRevision, string Reason,
    Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);
public sealed record AddEvidenceRevisionV2Command(Guid TenantId, Guid DossierId, Guid RequirementId, Guid? CorrectionRequestId,
    Guid? DocumentId, Guid StoredFileId, string Sha256, string FileName, string Reason, long ExpectedRevision,
    Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);
public sealed record RequestGovernanceV2Command(Guid TenantId, Guid DossierId, long ExpectedRevision, string Reason,
    Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);
public sealed record RequestOverrideV2Command(Guid TenantId, Guid DossierId, long ExpectedRevision, string Action, string Reason,
    Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);
public sealed record DecideGovernanceV2Command(Guid TenantId, Guid DossierId, Guid RequestId, long ExpectedRevision,
    string? Reason, Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);
public sealed record ExecuteGovernanceV2Command(Guid TenantId, Guid DossierId, Guid RequestId, long ExpectedRevision,
    Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);
public sealed record ConsumeOverrideV2Command(Guid TenantId, Guid DossierId, Guid RequestId, long ExpectedRevision,
    string Action, Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);
public sealed record CancelDossierV2Command(Guid TenantId, Guid DossierId, long ExpectedRevision, string Reason,
    Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);
public sealed record ArchiveDossierV2Command(Guid TenantId, Guid DossierId, long ExpectedRevision, string Reason,
    Guid RequestedByUserId, string? ActorRole = null, string? CorrelationId = null);

public sealed record WorkflowSnapshotV2Dto(Guid DossierId, RegistrationDossierStatus Status, long Revision,
    IReadOnlyCollection<string> AvailableActions, IReadOnlyCollection<string> Locks, CorrectionRequestV2Dto? OpenCorrection);
public sealed record CorrectionRequestV2Dto(Guid Id, Guid DossierId, string Reason, DossierCorrectionSeverity Severity,
    DossierCorrectionStatus Status, IReadOnlyCollection<Guid> RequirementIds, IReadOnlyCollection<string> FieldPaths,
    IReadOnlyCollection<Guid> DocumentIds);
public sealed record EvidenceRevisionV2Dto(Guid Id, Guid RequirementId, int VersionNumber, Guid? DocumentId, Guid StoredFileId,
    string Sha256, string FileName, string Reason, Guid UploadedByUserId, DateTimeOffset UploadedAtUtc, bool IsCurrent,
    DossierEvidenceRevisionStatus Status, long Revision);
public sealed record GovernanceRequestV2Dto(Guid Id, Guid DossierId, DossierGovernanceRequestStatus Status,
    int ApprovalCount, DateTimeOffset? ConsumedAtUtc, long Revision);
public sealed record ChangeEventV2Dto(long Sequence, string EventType, Guid ActorUserId, string? ActorRole,
    RegistrationDossierStatus? FromStatus, RegistrationDossierStatus? ToStatus, string Field, string? BeforeJson,
    string? AfterJson, string Reason, string CorrelationId, DateTimeOffset OccurredAtUtc);
