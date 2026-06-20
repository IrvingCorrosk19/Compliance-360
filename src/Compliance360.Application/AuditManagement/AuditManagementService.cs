using Compliance360.Domain.Audit;
using Compliance360.Domain.AuditManagement;
using Compliance360.Domain.Common;
using Compliance360.Shared;

namespace Compliance360.Application.AuditManagement;

public sealed class AuditManagementService : IAuditManagementService
{
    private readonly IAuditManagementRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public AuditManagementService(IAuditManagementRepository repository, IApplicationDbContext dbContext, IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<AuditProgramSummary>> CreateProgramAsync(CreateAuditProgramCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.ProgramCodeExistsAsync(command.TenantId, command.Code.ToUpperInvariant(), cancellationToken))
            {
                return Result<AuditProgramSummary>.Failure("Audit program code already exists.");
            }

            var program = new AuditProgram(command.TenantId, command.Name, command.Code, command.Year, command.RequestedByUserId);
            await _repository.AddProgramAsync(program, cancellationToken);
            await AuditAsync(program.TenantId, program.Id, AuditAction.AuditCreated, command.RequestedByUserId, "Audit program created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditProgramSummary>.Success(ToSummary(program));
        }
        catch (DomainException exception)
        {
            return Result<AuditProgramSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditChecklistSummary>> CreateChecklistAsync(CreateAuditChecklistCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.ChecklistCodeVersionExistsAsync(command.TenantId, command.Code.ToUpperInvariant(), command.Version, cancellationToken))
            {
                return Result<AuditChecklistSummary>.Failure("Audit checklist code and version already exist.");
            }

            var checklist = new AuditChecklist(command.TenantId, command.Name, command.Code, command.Type, command.Version, command.RequestedByUserId);
            await _repository.AddChecklistAsync(checklist, cancellationToken);
            await AuditAsync(checklist.TenantId, checklist.Id, AuditAction.AuditCreated, command.RequestedByUserId, "Audit checklist created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditChecklistSummary>.Success(ToSummary(checklist));
        }
        catch (DomainException exception)
        {
            return Result<AuditChecklistSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditChecklistItemSummary>> AddChecklistItemAsync(AddAuditChecklistItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var checklist = await _repository.GetChecklistAsync(command.TenantId, command.ChecklistId, cancellationToken);
            if (checklist is null)
            {
                return Result<AuditChecklistItemSummary>.Failure("Audit checklist not found.");
            }

            var item = checklist.AddItem(command.Clause, command.Question, command.Weight);
            await AuditAsync(command.TenantId, checklist.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Audit checklist item added.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditChecklistItemSummary>.Success(ToSummary(item));
        }
        catch (DomainException exception)
        {
            return Result<AuditChecklistItemSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditPlanSummary>> CreatePlanAsync(CreateAuditPlanCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.GetProgramAsync(command.TenantId, command.AuditProgramId, cancellationToken) is null)
            {
                return Result<AuditPlanSummary>.Failure("Audit program not found.");
            }

            var plan = new AuditPlan(command.TenantId, command.AuditProgramId, command.Scope, command.Criteria, command.PlannedStartUtc, command.PlannedEndUtc);
            await _repository.AddPlanAsync(plan, cancellationToken);
            await AuditAsync(command.TenantId, plan.Id, AuditAction.AuditCreated, command.RequestedByUserId, "Audit plan created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditPlanSummary>.Success(ToSummary(plan));
        }
        catch (DomainException exception)
        {
            return Result<AuditPlanSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<ManagedAuditSummary>> CreateAuditAsync(CreateManagedAuditCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.GetProgramAsync(command.TenantId, command.AuditProgramId, cancellationToken) is null)
            {
                return Result<ManagedAuditSummary>.Failure("Audit program not found.");
            }

            if (await _repository.GetPlanAsync(command.TenantId, command.AuditPlanId, cancellationToken) is null)
            {
                return Result<ManagedAuditSummary>.Failure("Audit plan not found.");
            }

            if (await _repository.AuditCodeExistsAsync(command.TenantId, command.Code.ToUpperInvariant(), cancellationToken))
            {
                return Result<ManagedAuditSummary>.Failure("Audit code already exists.");
            }

            var audit = new ManagedAudit(command.TenantId, command.AuditProgramId, command.AuditPlanId, command.Title, command.Code, command.Type, command.RequestedByUserId, _clock.UtcNow);
            await _repository.AddAuditAsync(audit, cancellationToken);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditCreated, command.RequestedByUserId, "Audit created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<ManagedAuditSummary>.Success(ToSummary(audit));
        }
        catch (DomainException exception)
        {
            return Result<ManagedAuditSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result> AssignChecklistAsync(AssignAuditChecklistCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
            if (audit is null)
            {
                return Result.Failure("Audit not found.");
            }

            if (await _repository.GetChecklistAsync(command.TenantId, command.ChecklistId, cancellationToken) is null)
            {
                return Result.Failure("Audit checklist not found.");
            }

            audit.AssignChecklist(command.ChecklistId, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Checklist assigned.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditScheduleSummary>> ScheduleAsync(ScheduleAuditCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
            if (audit is null)
            {
                return Result<AuditScheduleSummary>.Failure("Audit not found.");
            }

            var schedule = audit.Schedule(command.StartUtc, command.EndUtc, command.Location, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Audit scheduled.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditScheduleSummary>.Success(ToSummary(schedule));
        }
        catch (DomainException exception)
        {
            return Result<AuditScheduleSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditParticipantSummary>> AddParticipantAsync(AddAuditParticipantCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
            if (audit is null)
            {
                return Result<AuditParticipantSummary>.Failure("Audit not found.");
            }

            var participant = audit.AddParticipant(command.UserId, command.Role, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Participant added.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditParticipantSummary>.Success(ToSummary(participant));
        }
        catch (DomainException exception)
        {
            return Result<AuditParticipantSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditAreaSummary>> AddAreaAsync(AddAuditAreaCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
            if (audit is null)
            {
                return Result<AuditAreaSummary>.Failure("Audit not found.");
            }

            var area = audit.AddArea(command.Name, command.Process);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Area added.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditAreaSummary>.Success(ToSummary(area));
        }
        catch (DomainException exception)
        {
            return Result<AuditAreaSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result> StartAsync(ManagedAuditActionCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeAudit(command, audit => audit.Start(command.RequestedByUserId, _clock.UtcNow), AuditAction.AuditUpdated, "Audit started.", cancellationToken);
    }

    public async Task<Result<AuditFindingSummary>> AddFindingAsync(AddAuditFindingCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
            if (audit is null)
            {
                return Result<AuditFindingSummary>.Failure("Audit not found.");
            }

            var finding = audit.AddFinding(command.Title, command.Description, command.Severity, command.ChecklistItemId, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Finding added.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditFindingSummary>.Success(ToSummary(finding));
        }
        catch (DomainException exception)
        {
            return Result<AuditFindingSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditEvidenceSummary>> AddEvidenceAsync(AddAuditEvidenceCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
            if (audit is null)
            {
                return Result<AuditEvidenceSummary>.Failure("Audit not found.");
            }

            var evidence = audit.AddEvidence(command.FindingId, command.Type, command.StoredFileId, command.FileName, command.ContentType, command.SizeBytes, command.Sha256Hash, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Evidence added.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditEvidenceSummary>.Success(ToSummary(evidence));
        }
        catch (DomainException exception)
        {
            return Result<AuditEvidenceSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditObservationSummary>> AddObservationAsync(AddAuditObservationCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
            if (audit is null)
            {
                return Result<AuditObservationSummary>.Failure("Audit not found.");
            }

            var observation = audit.AddObservation(command.Description, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Observation added.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditObservationSummary>.Success(ToSummary(observation));
        }
        catch (DomainException exception)
        {
            return Result<AuditObservationSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditNonConformitySummary>> AddNonConformityAsync(AddAuditNonConformityCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
            if (audit is null)
            {
                return Result<AuditNonConformitySummary>.Failure("Audit not found.");
            }

            var nonConformity = audit.AddNonConformity(command.FindingId, command.Requirement, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Non conformity added.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditNonConformitySummary>.Success(ToSummary(nonConformity));
        }
        catch (DomainException exception)
        {
            return Result<AuditNonConformitySummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditRecommendationSummary>> AddRecommendationAsync(AddAuditRecommendationCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
            if (audit is null)
            {
                return Result<AuditRecommendationSummary>.Failure("Audit not found.");
            }

            var recommendation = audit.AddRecommendation(command.FindingId, command.Recommendation, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Recommendation added.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditRecommendationSummary>.Success(ToSummary(recommendation));
        }
        catch (DomainException exception)
        {
            return Result<AuditRecommendationSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditCorrectiveActionLinkSummary>> LinkCorrectiveActionAsync(LinkAuditCorrectiveActionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
            if (audit is null)
            {
                return Result<AuditCorrectiveActionLinkSummary>.Failure("Audit not found.");
            }

            var link = audit.LinkCorrectiveAction(command.FindingId, command.CorrectiveActionId, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Corrective action linked.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditCorrectiveActionLinkSummary>.Success(ToSummary(link));
        }
        catch (DomainException exception)
        {
            return Result<AuditCorrectiveActionLinkSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<AuditAttachmentSummary>> AddAttachmentAsync(AddAuditAttachmentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
            if (audit is null)
            {
                return Result<AuditAttachmentSummary>.Failure("Audit not found.");
            }

            var attachment = audit.AddAttachment(command.StoredFileId, command.FileName, command.ContentType, command.SizeBytes, command.Sha256Hash, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, audit.Id, AuditAction.AuditUpdated, command.RequestedByUserId, "Attachment added.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<AuditAttachmentSummary>.Success(ToSummary(attachment));
        }
        catch (DomainException exception)
        {
            return Result<AuditAttachmentSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result> CompleteAsync(ManagedAuditActionCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeAudit(command, audit => audit.Complete(command.RequestedByUserId, _clock.UtcNow), AuditAction.AuditUpdated, "Audit completed.", cancellationToken);
    }

    public async Task<Result> CloseAsync(ManagedAuditActionCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeAudit(command, audit => audit.Close(command.RequestedByUserId, _clock.UtcNow), AuditAction.AuditClosed, "Audit closed.", cancellationToken);
    }

    public async Task<Result> ReopenAsync(ManagedAuditActionCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeAudit(command, audit => audit.Reopen(command.RequestedByUserId, _clock.UtcNow), AuditAction.AuditReopened, "Audit reopened.", cancellationToken);
    }

    public Task<Result<ManagedAuditSearchResult>> SearchAsync(ManagedAuditSearchQuery query, CancellationToken cancellationToken = default)
    {
        return SearchInternalAsync(query, cancellationToken);
    }

    public async Task<Result<AuditDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return Result<AuditDashboardDto>.Success(await _repository.GetDashboardAsync(tenantId, cancellationToken));
    }

    public async Task<Result<AuditExportDescriptor>> ExportAsync(ManagedAuditExportQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _repository.SearchAsync(new ManagedAuditSearchCriteria(query.TenantId, null, query.Type, query.Status, 1, 10_000), cancellationToken);
        var format = string.IsNullOrWhiteSpace(query.Format) ? "csv" : query.Format.Trim().ToLowerInvariant();
        var contentType = format switch
        {
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "json" => "application/json",
            _ => "text/csv"
        };
        await AuditAsync(query.TenantId, null, AuditAction.Exported, query.RequestedByUserId, "Audit export generated.", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<AuditExportDescriptor>.Success(new AuditExportDescriptor(query.TenantId, format, $"audits-{query.TenantId:N}.{format}", contentType, result.TotalCount));
    }

    private async Task<Result<ManagedAuditSearchResult>> SearchInternalAsync(ManagedAuditSearchQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 25 : Math.Min(query.PageSize, 100);
        var result = await _repository.SearchAsync(new ManagedAuditSearchCriteria(query.TenantId, query.SearchText, query.Type, query.Status, page, pageSize), cancellationToken);
        return Result<ManagedAuditSearchResult>.Success(result);
    }

    private async Task<Result> ChangeAudit(ManagedAuditActionCommand command, Action<ManagedAudit> change, AuditAction action, string auditMessage, CancellationToken cancellationToken)
    {
        var audit = await GetAudit(command.TenantId, command.AuditId, cancellationToken);
        if (audit is null)
        {
            return Result.Failure("Audit not found.");
        }

        try
        {
            change(audit);
            await AuditAsync(command.TenantId, audit.Id, action, command.RequestedByUserId, auditMessage, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private Task<ManagedAudit?> GetAudit(Guid tenantId, Guid auditId, CancellationToken cancellationToken)
    {
        return _repository.GetAuditAsync(tenantId, auditId, cancellationToken);
    }

    private Task AuditAsync(Guid tenantId, Guid? entityId, AuditAction action, Guid userId, string message, CancellationToken cancellationToken)
    {
        var log = AuditLog.Create(tenantId, userId, "AuditManagement", entityId, action, _clock.UtcNow)
            .WithMetadata($"{{\"message\":\"{message}\"}}");
        return _repository.AddAuditLogAsync(log, cancellationToken);
    }

    private static AuditProgramSummary ToSummary(AuditProgram program) => new(program.Id, program.TenantId, program.Name, program.Code, program.Year);
    private static AuditChecklistSummary ToSummary(AuditChecklist checklist) => new(checklist.Id, checklist.TenantId, checklist.Name, checklist.Code, checklist.Type, checklist.Version);
    private static AuditChecklistItemSummary ToSummary(AuditChecklistItem item) => new(item.Id, item.ChecklistId, item.Clause, item.Question, item.Weight);
    private static AuditPlanSummary ToSummary(AuditPlan plan) => new(plan.Id, plan.TenantId, plan.AuditProgramId, plan.Scope, plan.Criteria, plan.PlannedStartUtc, plan.PlannedEndUtc);
    private static ManagedAuditSummary ToSummary(ManagedAudit audit) => new(audit.Id, audit.TenantId, audit.Title, audit.Code, audit.Type, audit.Status, audit.ChecklistId);
    private static AuditScheduleSummary ToSummary(AuditSchedule schedule) => new(schedule.Id, schedule.AuditId, schedule.StartUtc, schedule.EndUtc, schedule.Location);
    private static AuditParticipantSummary ToSummary(AuditParticipant participant) => new(participant.Id, participant.AuditId, participant.UserId, participant.Role);
    private static AuditAreaSummary ToSummary(AuditArea area) => new(area.Id, area.AuditId, area.Name, area.Process);
    private static AuditFindingSummary ToSummary(AuditFinding finding) => new(finding.Id, finding.AuditId, finding.Title, finding.Severity);
    private static AuditEvidenceSummary ToSummary(AuditEvidence evidence) => new(evidence.Id, evidence.AuditId, evidence.FindingId, evidence.Type, evidence.FileName, evidence.Sha256Hash);
    private static AuditObservationSummary ToSummary(AuditObservation observation) => new(observation.Id, observation.AuditId, observation.Description);
    private static AuditNonConformitySummary ToSummary(AuditNonConformity nonConformity) => new(nonConformity.Id, nonConformity.AuditId, nonConformity.FindingId, nonConformity.Requirement);
    private static AuditRecommendationSummary ToSummary(AuditRecommendation recommendation) => new(recommendation.Id, recommendation.AuditId, recommendation.FindingId, recommendation.Recommendation);
    private static AuditCorrectiveActionLinkSummary ToSummary(AuditCorrectiveActionLink link) => new(link.Id, link.AuditId, link.FindingId, link.CorrectiveActionId);
    private static AuditAttachmentSummary ToSummary(AuditAttachment attachment) => new(attachment.Id, attachment.AuditId, attachment.FileName, attachment.Sha256Hash);
}
