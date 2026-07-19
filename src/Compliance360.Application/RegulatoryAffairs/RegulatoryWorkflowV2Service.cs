using Compliance360.Application.Audit;
using Compliance360.Application.Notifications;
using Compliance360.Application.Storage;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Domain.RegulatoryAffairs;
using Compliance360.Domain.Storage;
using Compliance360.Shared;

namespace Compliance360.Application.RegulatoryAffairs;

public sealed class RegulatoryWorkflowV2Service : IRegulatoryWorkflowV2Service
{
    private readonly IRegulatoryWorkflowV2Repository _repo;
    private readonly IApplicationDbContext _db;
    private readonly IAuditRepository _audit;
    private readonly IClock _clock;
    private readonly IStorageRepository? _storage;
    private readonly INotificationService? _notifications;
    private readonly IAuditContextAccessor? _auditContext;
    private readonly IAlertEventIngestionService? _alertEvents;

    public RegulatoryWorkflowV2Service(IRegulatoryWorkflowV2Repository repo, IApplicationDbContext db, IAuditRepository audit, IClock clock)
        : this(repo, db, audit, clock, null)
    {
    }

    public RegulatoryWorkflowV2Service(IRegulatoryWorkflowV2Repository repo, IApplicationDbContext db, IAuditRepository audit, IClock clock, IStorageRepository? storage)
        : this(repo, db, audit, clock, storage, null)
    {
    }

    public RegulatoryWorkflowV2Service(IRegulatoryWorkflowV2Repository repo, IApplicationDbContext db, IAuditRepository audit, IClock clock, IStorageRepository? storage, INotificationService? notifications)
        : this(repo, db, audit, clock, storage, notifications, null)
    {
    }

    public RegulatoryWorkflowV2Service(IRegulatoryWorkflowV2Repository repo, IApplicationDbContext db, IAuditRepository audit, IClock clock, IStorageRepository? storage, INotificationService? notifications, IAuditContextAccessor? auditContext)
        : this(repo, db, audit, clock, storage, notifications, auditContext, null)
    {
    }

    public RegulatoryWorkflowV2Service(
        IRegulatoryWorkflowV2Repository repo,
        IApplicationDbContext db,
        IAuditRepository audit,
        IClock clock,
        IStorageRepository? storage,
        INotificationService? notifications,
        IAuditContextAccessor? auditContext,
        IAlertEventIngestionService? alertEvents)
    {
        _repo = repo;
        _db = db;
        _audit = audit;
        _clock = clock;
        _storage = storage;
        _notifications = notifications;
        _auditContext = auditContext;
        _alertEvents = alertEvents;
    }

    public async Task<Result<WorkflowSnapshotV2Dto>> GetWorkflowSnapshotAsync(Guid tenantId, Guid dossierId, CancellationToken ct = default)
    {
        var d = await _repo.GetDossierAsync(tenantId, dossierId, ct);
        if (d is null) return Result<WorkflowSnapshotV2Dto>.Failure("Dossier not found.");
        var correction = await _repo.GetOpenCorrectionAsync(tenantId, dossierId, ct);
        var actions = d.Status switch
        {
            RegistrationDossierStatus.Draft or RegistrationDossierStatus.Planning
                or RegistrationDossierStatus.WaitingManufacturerDocuments or RegistrationDossierStatus.DocumentsReceived =>
                new[] { "update-metadata", "cancel" },
            RegistrationDossierStatus.Assembling => new[] { "return-for-correction", "technical-review", "update-metadata", "cancel" },
            RegistrationDossierStatus.UnderTechnicalReview => new[] { "return-for-correction", "ready-for-submission", "cancel" },
            RegistrationDossierStatus.CorrectionRequested => new[] { "add-evidence", "submit-correction", "cancel" },
            RegistrationDossierStatus.Closed => new[] { "request-reopen", "archive" },
            RegistrationDossierStatus.Rejected or RegistrationDossierStatus.Cancelled => new[] { "request-reopen" },
            _ => Array.Empty<string>()
        };
        var locks = correction is null ? Array.Empty<string>() : new[] { "correction-scope-enforced" };
        return Result<WorkflowSnapshotV2Dto>.Success(new(d.Id, d.Status, d.Revision, actions, locks, correction is null ? null : Map(correction)));
    }

    public async Task<Result<DossierDetailDto>> UpdateMetadataAsync(UpdateDossierMetadataV2Command c, CancellationToken ct = default)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            var changed = ChangedMetadataPaths(d, c);
            var controlled = d.Status == RegistrationDossierStatus.CorrectionRequested;
            if (controlled)
            {
                var correction = await _repo.GetOpenCorrectionAsync(c.TenantId, d.Id, ct)
                    ?? throw new DomainException("Open correction scope not found.");
                if (!c.CorrectionRequestId.HasValue || c.CorrectionRequestId.Value != correction.Id)
                    throw new DomainException("Metadata change must reference the active correction request.");
                var outsideScope = changed.Where(path => !correction.IncludesField(path)).ToList();
                if (outsideScope.Count > 0)
                    throw new DomainException($"Metadata fields outside the controlled correction scope: {string.Join(", ", outsideScope)}.");
            }

            d.UpdateMetadataAndDates(c.Priority, c.OwnerUserId, c.SalesMarketingInput, c.OpportunityAmount, c.Currency,
                c.Comments, c.RequestedFromFactoryOn, c.EstimatedReceptionOn, c.MaximumReceptionOn,
                c.EstimatedSubmissionOn, c.EstimatedApprovalOn, c.TargetExpirationOn, c.Reason, controlled);
            d.IncrementRevision();
            await Record(c.TenantId, d, "MetadataUpdated", c.RequestedByUserId, c.ActorRole, null, null,
                changed.Count == 0 ? "metadata" : string.Join(",", changed), c.Reason, c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, d.Id, ct);
            return Result<DossierDetailDto>.Success(Map(d));
        }
        catch (DomainException ex) { return Result<DossierDetailDto>.Failure(ex.Message); }
    }

    public async Task<Result<CorrectionRequestV2Dto>> ReturnForCorrectionAsync(ReturnForCorrectionV2Command c, CancellationToken ct = default)
    {
        try
        {
            if (c.RequirementIds.Count == 0) return Result<CorrectionRequestV2Dto>.Failure("At least one requirement is required.");
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            if (d.Status != RegistrationDossierStatus.UnderTechnicalReview) return Result<CorrectionRequestV2Dto>.Failure("Dossier must be UnderTechnicalReview.");
            foreach (var id in c.RequirementIds) _ = d.GetRequirement(id);
            var request = new DossierCorrectionRequest(c.TenantId, c.DossierId, c.Reason, c.Severity, c.RequestedByUserId, _clock.UtcNow);
            foreach (var id in c.RequirementIds) request.AddRequirement(id);
            foreach (var path in c.FieldPaths ?? []) request.AddField(path);
            foreach (var id in c.DocumentIds ?? []) request.AddDocument(id);
            request.ValidateHasScope();
            await _repo.AddCorrectionAsync(request, ct);
            var from = d.Status; d.TransitionTo(RegistrationDossierStatus.CorrectionRequested, _clock.UtcNow); d.IncrementRevision();
            await Record(c.TenantId, d, "CorrectionRequested", c.RequestedByUserId, c.ActorRole, from, d.Status, "correction", c.Reason, c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, d.Id, ct);
            await Notify(c.TenantId, c.RequestedByUserId, d.RegulatoryOwnerUserId ?? d.CreatedByUserId,
                "Corrección regulatoria solicitada", $"El expediente {d.CaseNumber} fue devuelto para corrección controlada.", ct,
                "regulatory.dossier.correction_requested", d, c.CorrelationId());
            return Result<CorrectionRequestV2Dto>.Success(Map(request));
        }
        catch (DomainException ex) { return Result<CorrectionRequestV2Dto>.Failure(ex.Message); }
    }

    public async Task<Result<DossierDetailDto>> SubmitCorrectionAsync(SubmitCorrectionV2Command c, CancellationToken ct = default)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            var correction = await _repo.GetCorrectionAsync(c.TenantId, c.CorrectionRequestId, ct) ?? throw new DomainException("Correction request not found.");
            if (correction.DossierId != d.Id || correction.Status != DossierCorrectionStatus.Open) throw new DomainException("Correction request is not open for this dossier.");
            if (c.RequirementIds.Any(x => !correction.IncludesRequirement(x)) ||
                (c.FieldPaths ?? []).Any(x => !correction.IncludesField(x)) ||
                (c.DocumentIds ?? []).Any(x => !correction.IncludesDocument(x)))
                throw new DomainException("Correction submission contains items outside the controlled scope.");
            var scopedRequirements = correction.ScopeItems.Where(x => x.ScopeType == "Requirement").Select(x => x.TargetId!.Value).ToHashSet();
            var scopedFields = correction.ScopeItems.Where(x => x.ScopeType == "Field").Select(x => x.FieldPath!).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var scopedDocuments = correction.ScopeItems.Where(x => x.ScopeType == "Document").Select(x => x.TargetId!.Value).ToHashSet();
            if (!scopedRequirements.SetEquals(c.RequirementIds)
                || !scopedFields.SetEquals(c.FieldPaths ?? [])
                || !scopedDocuments.SetEquals(c.DocumentIds ?? []))
                throw new DomainException("Correction submission must explicitly cover every item in the approved correction scope.");
            foreach (var requirementId in correction.ScopeItems.Where(x => x.ScopeType == "Requirement").Select(x => x.TargetId!.Value))
            {
                var versions = await _repo.ListEvidenceAsync(c.TenantId, d.Id, requirementId, ct);
                if (!versions.Any(x => x.IsCurrent && x.Status == DossierEvidenceRevisionStatus.Active && x.CorrectionRequestId == correction.Id))
                    throw new DomainException($"Active evidence is required for scoped requirement {requirementId}.");
            }
            correction.MarkResponseSubmitted(_clock.UtcNow);
            var from = d.Status; d.TransitionTo(RegistrationDossierStatus.UnderTechnicalReview, _clock.UtcNow); d.IncrementRevision();
            await Record(c.TenantId, d, "CorrectionSubmitted", c.RequestedByUserId, c.ActorRole, from, d.Status, "correction", c.Reason, c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, d.Id, ct);
            await Notify(c.TenantId, c.RequestedByUserId, correction.RequestedByUserId,
                "Corrección regulatoria enviada", $"La corrección del expediente {d.CaseNumber} fue enviada a revisión técnica.", ct,
                "regulatory.dossier.status_changed", d, c.CorrelationId());
            return Result<DossierDetailDto>.Success(Map(d));
        }
        catch (DomainException ex) { return Result<DossierDetailDto>.Failure(ex.Message); }
    }

    public async Task<Result<DossierDetailDto>> StartTechnicalReviewAsync(StartTechnicalReviewV2Command c, CancellationToken ct = default)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            if (d.Status != RegistrationDossierStatus.Assembling)
                throw new DomainException("Dossier must be Assembling before technical review.");

            var blockers = d.Requirements
                .Where(requirement => requirement.IsRequired
                    && requirement.Status is not (
                        DossierRequirementStatus.Received
                        or DossierRequirementStatus.Accepted
                        or DossierRequirementStatus.Waived
                        or DossierRequirementStatus.NotRequired))
                .Select(requirement => requirement.Code)
                .ToList();
            if (_storage is null)
                throw new DomainException("Stored-file verification is unavailable.");
            foreach (var requirement in d.Requirements.Where(requirement =>
                         requirement.IsRequired
                         && requirement.Status is DossierRequirementStatus.Received or DossierRequirementStatus.Accepted))
            {
                if (!requirement.StoredFileId.HasValue)
                {
                    blockers.Add(requirement.Code);
                    continue;
                }

                var storedFile = await _storage.GetByIdAsync(c.TenantId, requirement.StoredFileId.Value, ct);
                if (storedFile is null
                    || !string.Equals(storedFile.OwnerEntityName, nameof(RegistrationDossier), StringComparison.Ordinal)
                    || storedFile.OwnerEntityId != d.Id
                    || storedFile.Status != StoredFileStatus.Available)
                {
                    blockers.Add(requirement.Code);
                }
            }

            if (blockers.Count > 0)
                throw new DomainException($"Required evidence is incomplete or invalid: {string.Join(", ", blockers.Distinct())}.");

            var from = d.Status;
            d.TransitionTo(RegistrationDossierStatus.UnderTechnicalReview, _clock.UtcNow);
            d.IncrementRevision();
            await Record(c.TenantId, d, "TechnicalReviewStarted", c.RequestedByUserId, c.ActorRole,
                from, d.Status, "technical-review", c.Reason, c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, d.Id, ct);
            return Result<DossierDetailDto>.Success(Map(d));
        }
        catch (DomainException ex) { return Result<DossierDetailDto>.Failure(ex.Message); }
    }

    public async Task<Result<DossierDetailDto>> CompleteTechnicalReviewAsync(CompleteTechnicalReviewV2Command c, CancellationToken ct = default)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            if (d.Status != RegistrationDossierStatus.UnderTechnicalReview)
                throw new DomainException("Dossier must be UnderTechnicalReview.");

            var blockers = d.Requirements
                .Where(x => x.IsRequired && x.Status is not (DossierRequirementStatus.Accepted
                    or DossierRequirementStatus.Waived or DossierRequirementStatus.NotRequired))
                .Select(x => x.Code)
                .ToList();
            if (blockers.Count > 0)
                throw new DomainException($"Technical review is incomplete: {string.Join(", ", blockers)}.");

            var activeCorrection = await _repo.GetOpenCorrectionAsync(c.TenantId, d.Id, ct);
            if (activeCorrection is not null)
            {
                if (!c.CorrectionRequestId.HasValue || c.CorrectionRequestId.Value != activeCorrection.Id)
                    throw new DomainException("The active correction request must be supplied to complete technical review.");
                if (activeCorrection.Status != DossierCorrectionStatus.ResponseSubmitted)
                    throw new DomainException("The active correction response must be submitted before technical review can be completed.");
                activeCorrection.Close();
            }
            else if (c.CorrectionRequestId.HasValue)
            {
                throw new DomainException("No active correction request exists for this dossier.");
            }

            var from = d.Status;
            d.TransitionTo(RegistrationDossierStatus.ReadyForSubmission, _clock.UtcNow);
            d.IncrementRevision();
            await Record(c.TenantId, d, "TechnicalReviewCompleted", c.RequestedByUserId, c.ActorRole,
                from, d.Status, "technical-review", c.Reason, c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, d.Id, ct);
            await Notify(c.TenantId, c.RequestedByUserId, d.RegulatoryOwnerUserId ?? d.CreatedByUserId,
                "Revisión técnica completada", $"El expediente {d.CaseNumber} está listo para aprobación interna.", ct,
                "regulatory.dossier.status_changed", d, c.CorrelationId());
            return Result<DossierDetailDto>.Success(Map(d));
        }
        catch (DomainException ex) { return Result<DossierDetailDto>.Failure(ex.Message); }
    }

    public async Task<Result<EvidenceRevisionV2Dto>> AddEvidenceRevisionAsync(AddEvidenceRevisionV2Command c, CancellationToken ct = default)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            _ = d.GetRequirement(c.RequirementId);
            if (d.Status is not (
                RegistrationDossierStatus.Draft
                or RegistrationDossierStatus.Planning
                or RegistrationDossierStatus.WaitingManufacturerDocuments
                or RegistrationDossierStatus.DocumentsReceived
                or RegistrationDossierStatus.Assembling
                or RegistrationDossierStatus.CorrectionRequested))
                throw new DomainException("Evidence versions can only be added during preparation or controlled correction.");
            if (d.Status == RegistrationDossierStatus.CorrectionRequested)
            {
                var correction = await _repo.GetOpenCorrectionAsync(c.TenantId, d.Id, ct) ?? throw new DomainException("Open correction scope not found.");
                if (!correction.IncludesRequirement(c.RequirementId)) throw new DomainException("Requirement is outside the controlled correction scope.");
                if (c.DocumentId.HasValue && !correction.IncludesDocument(c.DocumentId.Value)) throw new DomainException("Document is outside the controlled correction scope.");
                if (c.CorrectionRequestId != correction.Id) throw new DomainException("Evidence must reference the active correction request.");
            }
            if (_storage is null)
                throw new DomainException("Stored-file verification is unavailable.");
            var storedFile = await _storage.GetByIdAsync(c.TenantId, c.StoredFileId, ct)
                ?? throw new DomainException("Stored file not found.");
            if (!string.Equals(storedFile.OwnerEntityName, nameof(RegistrationDossier), StringComparison.Ordinal)
                || storedFile.OwnerEntityId != d.Id)
                throw new DomainException("Stored file does not belong to this dossier.");
            if (storedFile.Status is StoredFileStatus.Quarantined or StoredFileStatus.Deleted)
                throw new DomainException("Stored file is not available for regulatory evidence.");
            var versions = await _repo.ListEvidenceAsync(c.TenantId, d.Id, c.RequirementId, ct);
            foreach (var current in versions.Where(x => x.IsCurrent)) current.Supersede();
            var evidence = new DossierEvidenceRevision(c.TenantId, d.Id, c.RequirementId, c.CorrectionRequestId, c.DocumentId,
                storedFile.Id, storedFile.Sha256Hash, storedFile.OriginalFileName, c.Reason, c.RequestedByUserId, _clock.UtcNow,
                versions.Count == 0 ? 1 : versions.Max(x => x.VersionNumber) + 1);
            await _repo.AddEvidenceAsync(evidence, ct);
            d.IncrementRevision();
            await Record(c.TenantId, d, "EvidenceRevisionAdded", c.RequestedByUserId, c.ActorRole, null, null,
                $"requirements/{c.RequirementId}/evidence", c.Reason, c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, evidence.Id, ct);
            return Result<EvidenceRevisionV2Dto>.Success(Map(evidence, d.Revision));
        }
        catch (DomainException ex) { return Result<EvidenceRevisionV2Dto>.Failure(ex.Message); }
    }

    public async Task<Result<IReadOnlyCollection<EvidenceRevisionV2Dto>>> ListEvidenceVersionsAsync(Guid tenantId, Guid dossierId, Guid requirementId, CancellationToken ct = default)
    {
        var d = await _repo.GetDossierAsync(tenantId, dossierId, ct);
        if (d is null) return Result<IReadOnlyCollection<EvidenceRevisionV2Dto>>.Failure("Dossier not found.");
        var rows = await _repo.ListEvidenceAsync(tenantId, dossierId, requirementId, ct);
        return Result<IReadOnlyCollection<EvidenceRevisionV2Dto>>.Success(rows.Select(x => Map(x, d.Revision)).ToList());
    }

    public async Task<Result<GovernanceRequestV2Dto>> RequestReopenAsync(RequestGovernanceV2Command c, CancellationToken ct = default)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            if (d.Status is not (RegistrationDossierStatus.Closed or RegistrationDossierStatus.Rejected or RegistrationDossierStatus.Cancelled)) throw new DomainException("Only terminal dossiers can request reopening.");
            var request = new DossierReopenRequest(c.TenantId, d.Id, c.Reason, c.RequestedByUserId, _clock.UtcNow);
            await _repo.AddReopenAsync(request, ct); d.IncrementRevision();
            await Record(c.TenantId, d, "ReopenRequested", c.RequestedByUserId, c.ActorRole, null, null, "reopen", c.Reason, c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, request.Id, ct);
            return Result<GovernanceRequestV2Dto>.Success(Map(request, d.Revision));
        }
        catch (DomainException ex) { return Result<GovernanceRequestV2Dto>.Failure(ex.Message); }
    }

    public Task<Result<GovernanceRequestV2Dto>> ApproveReopenAsync(DecideGovernanceV2Command c, CancellationToken ct = default) =>
        DecideReopen(c, false, ct);
    public Task<Result<GovernanceRequestV2Dto>> RejectReopenAsync(DecideGovernanceV2Command c, CancellationToken ct = default) =>
        DecideReopen(c, true, ct);

    private async Task<Result<GovernanceRequestV2Dto>> DecideReopen(DecideGovernanceV2Command c, bool reject, CancellationToken ct)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            var request = await _repo.GetReopenAsync(c.TenantId, c.RequestId, ct) ?? throw new DomainException("Reopen request not found.");
            if (request.DossierId != d.Id) throw new DomainException("Reopen request does not belong to dossier.");
            if (reject) request.Reject(c.RequestedByUserId, c.Reason ?? "", _clock.UtcNow); else request.Approve(c.RequestedByUserId, _clock.UtcNow);
            d.IncrementRevision();
            await Record(c.TenantId, d, reject ? "ReopenRejected" : "ReopenApproved", c.RequestedByUserId, c.ActorRole, null, null, "reopen", c.Reason ?? "Governance approval", c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, request.Id, ct);
            return Result<GovernanceRequestV2Dto>.Success(Map(request, d.Revision));
        }
        catch (DomainException ex) { return Result<GovernanceRequestV2Dto>.Failure(ex.Message); }
    }

    public async Task<Result<DossierDetailDto>> ExecuteReopenAsync(ExecuteGovernanceV2Command c, CancellationToken ct = default)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            var request = await _repo.GetReopenAsync(c.TenantId, c.RequestId, ct) ?? throw new DomainException("Reopen request not found.");
            if (request.DossierId != d.Id) throw new DomainException("Reopen request does not belong to dossier.");
            var correction = new DossierCorrectionRequest(
                c.TenantId,
                d.Id,
                request.Reason,
                DossierCorrectionSeverity.High,
                c.RequestedByUserId,
                _clock.UtcNow);
            foreach (var requirement in d.Requirements)
                correction.AddRequirement(requirement.Id);
            foreach (var field in new[]
            {
                "metadata.priority", "metadata.ownerUserId", "metadata.salesMarketingInput", "metadata.opportunityAmount",
                "metadata.currency", "metadata.comments", "metadata.requestedFromFactoryOn", "metadata.estimatedReceptionOn",
                "metadata.maximumReceptionOn", "metadata.estimatedSubmissionOn", "metadata.estimatedApprovalOn",
                "metadata.targetExpirationOn"
            })
                correction.AddField(field);
            correction.ValidateHasScope();
            await _repo.AddCorrectionAsync(correction, ct);
            var from = d.Status; request.Execute(_clock.UtcNow); d.ReopenForCorrection(_clock.UtcNow); d.IncrementRevision();
            await Record(c.TenantId, d, "DossierReopened", c.RequestedByUserId, c.ActorRole, from, d.Status, "status", request.Reason, c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, d.Id, ct);
            await Notify(c.TenantId, c.RequestedByUserId, d.RegulatoryOwnerUserId ?? d.CreatedByUserId,
                "Expediente reabierto", $"El expediente {d.CaseNumber} fue reabierto bajo corrección controlada.", ct,
                "regulatory.dossier.status_changed", d, c.CorrelationId());
            return Result<DossierDetailDto>.Success(Map(d));
        }
        catch (DomainException ex) { return Result<DossierDetailDto>.Failure(ex.Message); }
    }

    public async Task<Result<GovernanceRequestV2Dto>> RequestOverrideAsync(RequestOverrideV2Command c, CancellationToken ct = default)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            var request = new DossierOverrideRequest(c.TenantId, d.Id, c.Action, c.Reason, c.RequestedByUserId, _clock.UtcNow);
            await _repo.AddOverrideAsync(request, ct); d.IncrementRevision();
            await Record(c.TenantId, d, "OverrideRequested", c.RequestedByUserId, c.ActorRole, null, null, "override", c.Reason, c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, request.Id, ct);
            return Result<GovernanceRequestV2Dto>.Success(Map(request, d.Revision));
        }
        catch (DomainException ex) { return Result<GovernanceRequestV2Dto>.Failure(ex.Message); }
    }

    public Task<Result<GovernanceRequestV2Dto>> ApproveOverrideAsync(DecideGovernanceV2Command c, CancellationToken ct = default) => DecideOverride(c, false, ct);
    public Task<Result<GovernanceRequestV2Dto>> RejectOverrideAsync(DecideGovernanceV2Command c, CancellationToken ct = default) => DecideOverride(c, true, ct);
    private async Task<Result<GovernanceRequestV2Dto>> DecideOverride(DecideGovernanceV2Command c, bool reject, CancellationToken ct)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            var request = await _repo.GetOverrideAsync(c.TenantId, c.RequestId, ct) ?? throw new DomainException("Override request not found.");
            if (request.DossierId != d.Id) throw new DomainException("Override request does not belong to dossier.");
            if (reject) request.Reject(c.RequestedByUserId, c.Reason ?? "", _clock.UtcNow); else request.Approve(c.RequestedByUserId, _clock.UtcNow);
            d.IncrementRevision();
            await Record(c.TenantId, d, reject ? "OverrideRejected" : "OverrideApproved", c.RequestedByUserId, c.ActorRole, null, null, "override", c.Reason ?? "Governance approval", c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, request.Id, ct);
            return Result<GovernanceRequestV2Dto>.Success(Map(request, d.Revision));
        }
        catch (DomainException ex) { return Result<GovernanceRequestV2Dto>.Failure(ex.Message); }
    }

    public async Task<Result<GovernanceRequestV2Dto>> ConsumeOverrideAsync(ConsumeOverrideV2Command c, CancellationToken ct = default)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            var request = await _repo.GetOverrideAsync(c.TenantId, c.RequestId, ct) ?? throw new DomainException("Override request not found.");
            if (request.DossierId != d.Id) throw new DomainException("Override request does not belong to dossier.");
            request.Consume(c.RequestedByUserId, c.Action, _clock.UtcNow); d.IncrementRevision();
            await Record(c.TenantId, d, "OverrideConsumed", c.RequestedByUserId, c.ActorRole, null, null, "override", request.Reason, c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, request.Id, ct);
            return Result<GovernanceRequestV2Dto>.Success(Map(request, d.Revision));
        }
        catch (DomainException ex) { return Result<GovernanceRequestV2Dto>.Failure(ex.Message); }
    }

    public async Task<Result<DossierDetailDto>> CancelAsync(CancelDossierV2Command c, CancellationToken ct = default)
    {
        try
        {
            var d = await RequireDossier(c.TenantId, c.DossierId, c.ExpectedRevision, ct);
            var reason = string.IsNullOrWhiteSpace(c.Reason) ? string.Empty : c.Reason.Trim();
            if (reason.Length < 8 || reason.Length > 2000)
                throw new DomainException("Cancellation reason must contain between 8 and 2000 characters.");
            if (d.Status is not (
                RegistrationDossierStatus.Draft
                or RegistrationDossierStatus.Planning
                or RegistrationDossierStatus.WaitingManufacturerDocuments
                or RegistrationDossierStatus.DocumentsReceived
                or RegistrationDossierStatus.Assembling
                or RegistrationDossierStatus.UnderTechnicalReview
                or RegistrationDossierStatus.CorrectionRequested))
                throw new DomainException("Only a dossier that has not been submitted can be cancelled.");

            var from = d.Status;
            d.TransitionTo(RegistrationDossierStatus.Cancelled, _clock.UtcNow);
            d.IncrementRevision();
            await Record(c.TenantId, d, "DossierCancelled", c.RequestedByUserId, c.ActorRole,
                from, d.Status, "status", reason, c.CorrelationId(), ct);
            await SaveAudit(c.TenantId, c.RequestedByUserId, d.Id, ct);
            await Notify(c.TenantId, c.RequestedByUserId, d.RegulatoryOwnerUserId ?? d.CreatedByUserId,
                "Expediente cancelado", $"El expediente {d.CaseNumber} fue cancelado sin eliminar su evidencia.", ct,
                "regulatory.dossier.status_changed", d, c.CorrelationId());
            return Result<DossierDetailDto>.Success(Map(d));
        }
        catch (DomainException ex) { return Result<DossierDetailDto>.Failure(ex.Message); }
    }

    public Task<Result<DossierDetailDto>> ArchiveAsync(ArchiveDossierV2Command c, CancellationToken ct = default) =>
        MutateDossier(c.TenantId, c.DossierId, c.ExpectedRevision, c.RequestedByUserId, c.ActorRole, c.CorrelationId(),
            "DossierArchived", "status", c.Reason, RegistrationDossierStatus.Closed, RegistrationDossierStatus.Archived,
            d => d.Archive(_clock.UtcNow), ct);

    public async Task<Result<IReadOnlyCollection<ChangeEventV2Dto>>> GetTimelineAsync(Guid tenantId, Guid dossierId, CancellationToken ct = default)
    {
        var rows = await _repo.ListTimelineAsync(tenantId, dossierId, ct);
        return Result<IReadOnlyCollection<ChangeEventV2Dto>>.Success(rows.Select(x => new ChangeEventV2Dto(x.Sequence, x.EventType,
            x.ActorUserId, x.ActorRole, x.FromStatus, x.ToStatus, x.Field, x.BeforeJson, x.AfterJson, x.Reason, x.CorrelationId, x.OccurredAtUtc)).ToList());
    }

    private async Task<Result<DossierDetailDto>> MutateDossier(Guid tenantId, Guid dossierId, long expected, Guid actor, string? role,
        string correlation, string type, string field, string reason, RegistrationDossierStatus? from, RegistrationDossierStatus? to,
        Action<RegistrationDossier> action, CancellationToken ct)
    {
        try
        {
            var d = await RequireDossier(tenantId, dossierId, expected, ct); action(d); d.IncrementRevision();
            await Record(tenantId, d, type, actor, role, from, to, field, reason, correlation, ct);
            await SaveAudit(tenantId, actor, d.Id, ct);
            return Result<DossierDetailDto>.Success(Map(d));
        }
        catch (DomainException ex) { return Result<DossierDetailDto>.Failure(ex.Message); }
    }

    private async Task<RegistrationDossier> RequireDossier(Guid tenantId, Guid id, long expected, CancellationToken ct)
    {
        var d = await _repo.GetDossierAsync(tenantId, id, ct);
        if (d is null || d.IsDeleted) throw new DomainException("Dossier not found.");
        d.EnsureExpectedRevision(expected); return d;
    }

    private async Task Record(Guid tenantId, RegistrationDossier d, string type, Guid actor, string? role,
        RegistrationDossierStatus? from, RegistrationDossierStatus? to, string field, string reason, string correlation, CancellationToken ct)
    {
        var sequence = await _repo.NextSequenceAsync(tenantId, d.Id, ct);
        await _repo.AddChangeEventAsync(new DossierChangeEvent(tenantId, d.Id, sequence, type, actor, role, from, to,
            field, null, null, reason, correlation, _clock.UtcNow), ct);
    }

    private async Task SaveAudit(Guid tenantId, Guid actor, Guid entityId, CancellationToken ct)
    {
        var context = _auditContext?.Current
            ?? new AuditContext(tenantId, actor, null, null, null, null, null, null, null);
        var evt = new AuditEvent(nameof(RegistrationDossier), entityId, AuditAction.RegulatoryDossierUpdated,
            AuditCategory.RegulatoryAffairs, context, new AuditSnapshot(null, null), new AuditMetadata(null), true, null);
        await _audit.AddAsync(AuditLog.FromEvent(evt, _clock.UtcNow), ct);
        await _db.SaveChangesAsync(ct);
    }

    private async Task Notify(
        Guid tenantId,
        Guid actorUserId,
        Guid? targetUserId,
        string subject,
        string body,
        CancellationToken ct,
        string? eventCode = null,
        RegistrationDossier? dossier = null,
        string? correlationId = null)
    {
        if (_alertEvents is not null && !string.IsNullOrWhiteSpace(eventCode) && dossier is not null)
        {
            try
            {
                var payload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    entityId = dossier.Id,
                    dossierId = dossier.Id,
                    caseNumber = dossier.CaseNumber,
                    status = dossier.Status.ToString(),
                    ownerUserId = dossier.RegulatoryOwnerUserId ?? dossier.CreatedByUserId,
                    responsibleUserId = targetUserId,
                    creatorUserId = dossier.CreatedByUserId,
                    actorUserId,
                    subject
                });
                await _alertEvents.IngestAsync(new IngestAlertEventCommand(
                    tenantId,
                    eventCode,
                    payload,
                    "RegulatoryWorkflowV2",
                    nameof(RegistrationDossier),
                    dossier.Id,
                    string.IsNullOrWhiteSpace(correlationId) ? $"rw:{dossier.Id:N}:{Guid.NewGuid():N}" : correlationId,
                    _clock.UtcNow), ct);
            }
            catch
            {
                // Progressive dual-write cannot invalidate an audited workflow transition.
            }
        }

        if (_notifications is null || !targetUserId.HasValue || targetUserId.Value == Guid.Empty)
            return;

        try
        {
            await _notifications.QueueAsync(new QueueNotificationCommand(
                tenantId,
                actorUserId,
                NotificationChannel.InApp,
                $"user:{targetUserId.Value:N}",
                subject,
                body,
                null,
                new Dictionary<string, string>
                {
                    ["module"] = "RegulatoryWorkflowV2",
                    ["subject"] = subject
                },
                NotificationPriority.High,
                targetUserId), ct);
        }
        catch
        {
            // Notification transport cannot invalidate an already audited regulatory transition.
        }
    }

    private static IReadOnlyCollection<string> ChangedMetadataPaths(RegistrationDossier d, UpdateDossierMetadataV2Command c)
    {
        var paths = new List<string>();
        static string N(string? value) => value?.Trim() ?? string.Empty;
        if (!string.Equals(N(d.Priority), N(c.Priority), StringComparison.Ordinal)) paths.Add("metadata.priority");
        if (d.RegulatoryOwnerUserId != c.OwnerUserId) paths.Add("metadata.ownerUserId");
        if (!string.Equals(N(d.SalesMarketingInput), N(c.SalesMarketingInput), StringComparison.Ordinal)) paths.Add("metadata.salesMarketingInput");
        if (d.OpportunityAmount != c.OpportunityAmount) paths.Add("metadata.opportunityAmount");
        if (!string.Equals(N(d.Currency), N(c.Currency).ToUpperInvariant(), StringComparison.Ordinal)) paths.Add("metadata.currency");
        if (!string.Equals(N(d.Comments), N(c.Comments), StringComparison.Ordinal)) paths.Add("metadata.comments");
        if (d.RequestedFromFactoryOn != c.RequestedFromFactoryOn) paths.Add("metadata.requestedFromFactoryOn");
        if (d.EstimatedReceptionOn != c.EstimatedReceptionOn) paths.Add("metadata.estimatedReceptionOn");
        if (d.MaximumReceptionOn != c.MaximumReceptionOn) paths.Add("metadata.maximumReceptionOn");
        if (d.EstimatedSubmissionOn != c.EstimatedSubmissionOn) paths.Add("metadata.estimatedSubmissionOn");
        if (d.EstimatedApprovalOn != c.EstimatedApprovalOn) paths.Add("metadata.estimatedApprovalOn");
        if (d.TargetExpirationOn != c.TargetExpirationOn) paths.Add("metadata.targetExpirationOn");
        return paths;
    }

    private static CorrectionRequestV2Dto Map(DossierCorrectionRequest x) => new(x.Id, x.DossierId, x.Reason, x.Severity, x.Status,
        x.ScopeItems.Where(s => s.ScopeType == "Requirement").Select(s => s.TargetId!.Value).ToList(),
        x.ScopeItems.Where(s => s.ScopeType == "Field").Select(s => s.FieldPath!).ToList(),
        x.ScopeItems.Where(s => s.ScopeType == "Document").Select(s => s.TargetId!.Value).ToList());
    private static EvidenceRevisionV2Dto Map(DossierEvidenceRevision x, long revision) => new(x.Id, x.RequirementId, x.VersionNumber,
        x.DocumentId, x.StoredFileId, x.Sha256, x.FileName, x.Reason, x.UploadedByUserId, x.UploadedAtUtc, x.IsCurrent, x.Status, revision);
    private static GovernanceRequestV2Dto Map(DossierReopenRequest x, long revision) => new(x.Id, x.DossierId, x.Status, x.Approvals.Count, null, revision);
    private static GovernanceRequestV2Dto Map(DossierOverrideRequest x, long revision) => new(x.Id, x.DossierId, x.Status, x.Approvals.Count, x.ConsumedAtUtc, revision);
    private static DossierDetailDto Map(RegistrationDossier d) => new(d.Id, d.CaseNumber, d.ProductId, d.AuthorityId, d.ProcessType, d.ExistingRegistrationId, d.Status,
        d.Priority, d.RegulatoryOwnerUserId, d.Comments, d.SalesMarketingInput, d.OpportunityAmount, d.Currency,
        d.RequirementPackId, d.RequirementPackVersionLabel, d.ResultingRegistrationId,
        d.RequestedFromFactoryOn, d.EstimatedReceptionOn, d.MaximumReceptionOn, d.ReceivedOn, d.AssembledOn, d.EstimatedSubmissionOn,
        d.SubmittedOn, d.SubmittedByUserId, d.SubmissionProcedureNumber, d.SubmissionExternalNumber, d.SubmissionProofStoredFileId,
        d.ObservationReceivedOn, d.EstimatedApprovalOn, d.ApprovedOn, d.TargetExpirationOn,
        d.Requirements.Select(r => new RequirementDto(r.Id, r.Code, r.Name, r.Category, r.IsRequired, r.IsCritical, r.Status, r.StoredFileId, r.CurrentDocumentId, r.ValidationNotes, r.Order)).ToList(),
        d.Milestones.Select(m => new MilestoneDto(m.Id, m.MilestoneType, m.PlannedDate, m.ActualDate, m.Status)).ToList(),
        d.Observations.Select(o => new ObservationDto(o.Id, o.ObservationNumber, o.ReceivedOn, o.DueOn, o.Description, o.Status, o.ResponseSubmittedOn, o.ClosedOn, o.Notes)).ToList(),
        d.History.Select(h => new DossierHistoryDto(h.Id, h.EventType, h.Summary, h.ActorUserId, h.OccurredAtUtc)).ToList(), d.Revision);
}

internal static class RegulatoryWorkflowV2CommandExtensions
{
    public static string CorrelationId(this object command)
    {
        var value = command.GetType().GetProperty("CorrelationId")?.GetValue(command) as string;
        return string.IsNullOrWhiteSpace(value) ? Guid.NewGuid().ToString("N") : value;
    }
}
