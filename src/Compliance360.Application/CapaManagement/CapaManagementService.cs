using Compliance360.Domain.Audit;
using Compliance360.Domain.CapaManagement;
using Compliance360.Domain.Common;
using Compliance360.Shared;

namespace Compliance360.Application.CapaManagement;

public sealed class CapaManagementService : ICapaManagementService
{
    private readonly ICapaManagementRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public CapaManagementService(ICapaManagementRepository repository, IApplicationDbContext dbContext, IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<CapaSummary>> CreateAsync(CreateCapaCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.CodeExistsAsync(command.TenantId, command.Code.ToUpperInvariant(), cancellationToken))
            {
                return Result<CapaSummary>.Failure("CAPA code already exists.");
            }

            var capa = new Capa(command.TenantId, command.Title, command.Code, command.Description, command.Priority, command.RiskLevel, command.SourceType, command.SourceEntityId, command.SupplierId, command.DocumentId, command.AuditId, command.RequestedByUserId, _clock.UtcNow);
            await _repository.AddAsync(capa, cancellationToken);
            await AuditAsync(command.TenantId, capa.Id, AuditAction.CapaCreated, command.RequestedByUserId, "CAPA created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<CapaSummary>.Success(ToSummary(capa));
        }
        catch (DomainException exception)
        {
            return Result<CapaSummary>.Failure(exception.Message);
        }
    }

    public Task<Result> ClassifyAsync(ClassifyCapaCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.CapaId, command.RequestedByUserId, AuditAction.CapaUpdated, "CAPA classified.", capa => capa.Classify(command.Priority, command.RiskLevel, command.CommitmentDueAtUtc, command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public async Task<Result<CapaOwnerSummary>> AssignOwnerAsync(AssignCapaOwnerCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.CapaId, command.RequestedByUserId, "CAPA owner assigned.", capa => ToSummary(capa.AssignOwner(command.OwnerUserId, command.DueAtUtc, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<CapaApproverSummary>> AddApproverAsync(AddCapaApproverCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.CapaId, command.RequestedByUserId, "CAPA approver assigned.", capa => ToSummary(capa.AddApprover(command.ApproverUserId, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<CapaRootCauseSummary>> DefineRootCauseAsync(DefineCapaRootCauseCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.CapaId, command.RequestedByUserId, "Root cause defined.", capa => ToSummary(capa.DefineRootCause(command.Description, command.Method, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<CapaCauseAnalysisSummary>> AddFiveWhyAsync(AddCapaFiveWhyCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.CapaId, command.RequestedByUserId, "5 Why analysis registered.", capa => ToSummary(capa.AddFiveWhyAnalysis(command.Why1, command.Why2, command.Why3, command.Why4, command.Why5, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<CapaCauseAnalysisSummary>> AddIshikawaAsync(AddCapaIshikawaCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.CapaId, command.RequestedByUserId, "Ishikawa analysis registered.", capa => ToSummary(capa.AddIshikawaAnalysis(command.People, command.Process, command.Equipment, command.Material, command.Environment, command.Measurement, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<CapaActionSummary>> AddContainmentActionAsync(AddCapaActionCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.CapaId, command.RequestedByUserId, "Containment action registered.", capa => ToSummary(capa.AddContainmentAction(command.Description, command.ResponsibleUserId, command.DueAtUtc, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<CapaActionSummary>> AddCorrectiveActionAsync(AddCapaActionCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.CapaId, command.RequestedByUserId, "Corrective action registered.", capa => ToSummary(capa.AddCorrectiveAction(command.Description, command.ResponsibleUserId, command.DueAtUtc, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<CapaActionSummary>> AddPreventiveActionAsync(AddCapaActionCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.CapaId, command.RequestedByUserId, "Preventive action registered.", capa => ToSummary(capa.AddPreventiveAction(command.Description, command.ResponsibleUserId, command.DueAtUtc, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<CapaEvidenceSummary>> AddEvidenceAsync(AddCapaEvidenceCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.CapaId, command.RequestedByUserId, "CAPA evidence attached.", capa => ToSummary(capa.AddEvidence(command.StoredFileId, command.FileName, command.ContentType, command.SizeBytes, command.Sha256Hash, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<CapaAttachmentSummary>> AddAttachmentAsync(AddCapaAttachmentCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.CapaId, command.RequestedByUserId, "CAPA attachment added.", capa => ToSummary(capa.AddAttachment(command.StoredFileId, command.FileName, command.ContentType, command.SizeBytes, command.Sha256Hash, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public Task<Result> RegisterFollowUpAsync(CapaFollowUpCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.CapaId, command.RequestedByUserId, AuditAction.CapaUpdated, "CAPA follow-up registered.", capa => capa.RegisterFollowUp(command.Notes, command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public Task<Result> EscalateOverdueAsync(CapaActionCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.CapaId, command.RequestedByUserId, AuditAction.CapaUpdated, "CAPA overdue escalated.", capa => capa.EscalateOverdue(command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public async Task<Result<CapaEffectivenessCheckSummary>> VerifyEffectivenessAsync(VerifyCapaEffectivenessCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.CapaId, command.RequestedByUserId, "CAPA effectiveness verified.", capa => ToSummary(capa.VerifyEffectiveness(command.IsEffective, command.VerificationSummary, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public Task<Result> AttachWorkflowAsync(AttachCapaWorkflowCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.CapaId, command.RequestedByUserId, AuditAction.CapaUpdated, "CAPA workflow attached.", capa => capa.AttachWorkflow(command.WorkflowInstanceId, command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public Task<Result> ApproveClosureAsync(CapaActionCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.CapaId, command.RequestedByUserId, AuditAction.CapaClosed, "CAPA closure approved.", capa => capa.ApproveClosure(command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public Task<Result> ReopenAsync(ReopenCapaCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.CapaId, command.RequestedByUserId, AuditAction.CapaReopened, "CAPA reopened.", capa => capa.Reopen(command.Reason, command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public async Task<Result<CapaSearchResult>> SearchAsync(CapaSearchQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 25 : Math.Min(query.PageSize, 100);
        return Result<CapaSearchResult>.Success(await _repository.SearchAsync(new CapaSearchCriteria(query.TenantId, query.SearchText, query.Status, query.Priority, query.RiskLevel, query.OwnerUserId, query.SupplierId, query.AuditId, page, pageSize), cancellationToken));
    }

    public async Task<Result<CapaDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return Result<CapaDashboardDto>.Success(await _repository.GetDashboardAsync(tenantId, _clock.UtcNow, cancellationToken));
    }

    public async Task<Result<CapaExportDescriptor>> ExportAsync(CapaExportQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _repository.SearchAsync(new CapaSearchCriteria(query.TenantId, null, query.Status, query.Priority, query.RiskLevel, null, null, null, 1, 10_000), cancellationToken);
        var format = string.IsNullOrWhiteSpace(query.Format) ? "csv" : query.Format.Trim().ToLowerInvariant();
        var contentType = format switch
        {
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "json" => "application/json",
            _ => "text/csv"
        };
        await AuditAsync(query.TenantId, null, AuditAction.Exported, query.RequestedByUserId, "CAPA export generated.", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<CapaExportDescriptor>.Success(new CapaExportDescriptor(query.TenantId, format, $"capas-{query.TenantId:N}.{format}", contentType, result.TotalCount));
    }

    private async Task<Result> ChangeAsync(Guid tenantId, Guid capaId, Guid userId, AuditAction action, string message, Action<Capa> change, CancellationToken cancellationToken)
    {
        var capa = await _repository.GetAsync(tenantId, capaId, cancellationToken);
        if (capa is null)
        {
            return Result.Failure("CAPA not found.");
        }

        try
        {
            change(capa);
            await AuditAsync(tenantId, capa.Id, action, userId, message, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private async Task<Result<T>> ChangeWithValueAsync<T>(Guid tenantId, Guid capaId, Guid userId, string message, Func<Capa, T> change, CancellationToken cancellationToken)
    {
        var capa = await _repository.GetAsync(tenantId, capaId, cancellationToken);
        if (capa is null)
        {
            return Result<T>.Failure("CAPA not found.");
        }

        try
        {
            var value = change(capa);
            await AuditAsync(tenantId, capa.Id, AuditAction.CapaUpdated, userId, message, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<T>.Success(value);
        }
        catch (DomainException exception)
        {
            return Result<T>.Failure(exception.Message);
        }
    }

    private Task AuditAsync(Guid tenantId, Guid? entityId, AuditAction action, Guid userId, string message, CancellationToken cancellationToken)
    {
        var log = AuditLog.FromEvent(
            new AuditEvent(
                nameof(Capa),
                entityId,
                action,
                AuditLog.InferCategory(action),
                new AuditContext(tenantId, userId, null, null, null, null, null, null, null),
                new AuditSnapshot(null, null),
                new AuditMetadata($"{{\"source\":\"capa-management\",\"message\":\"{message}\"}}"),
                true,
                null),
            _clock.UtcNow);
        return _repository.AddAuditLogAsync(log, cancellationToken);
    }

    private static CapaSummary ToSummary(Capa capa)
    {
        return new CapaSummary(capa.Id, capa.TenantId, capa.Title, capa.Code, capa.Status, capa.Priority, capa.RiskLevel, capa.SourceType, capa.SupplierId, capa.DocumentId, capa.AuditId, capa.CommitmentDueAtUtc, capa.ClosedAtUtc);
    }

    private static CapaOwnerSummary ToSummary(CapaOwner owner) => new(owner.Id, owner.CapaId, owner.UserId, owner.DueAtUtc, owner.IsActive);
    private static CapaApproverSummary ToSummary(CapaApprover approver) => new(approver.Id, approver.CapaId, approver.UserId);
    private static CapaRootCauseSummary ToSummary(CapaRootCause rootCause) => new(rootCause.Id, rootCause.CapaId, rootCause.Description, rootCause.Method);
    private static CapaCauseAnalysisSummary ToSummary(CapaCauseAnalysis analysis) => new(analysis.Id, analysis.CapaId, analysis.Method);
    private static CapaActionSummary ToSummary(CapaAction action) => new(action.Id, action.CapaId, action.Description, action.ResponsibleUserId, action.DueAtUtc, action.Type, action.Status);
    private static CapaEvidenceSummary ToSummary(CapaEvidence evidence) => new(evidence.Id, evidence.CapaId, evidence.FileName, evidence.ContentType, evidence.SizeBytes, evidence.Sha256Hash);
    private static CapaAttachmentSummary ToSummary(CapaAttachment attachment) => new(attachment.Id, attachment.CapaId, attachment.FileName, attachment.ContentType, attachment.SizeBytes, attachment.Sha256Hash);
    private static CapaEffectivenessCheckSummary ToSummary(CapaEffectivenessCheck check) => new(check.Id, check.CapaId, check.IsEffective, check.VerificationSummary, check.VerifiedByUserId);
}
