using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.RiskManagement;
using Compliance360.Shared;

namespace Compliance360.Application.RiskManagement;

public sealed class RiskManagementService : IRiskManagementService
{
    private readonly IRiskManagementRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public RiskManagementService(IRiskManagementRepository repository, IApplicationDbContext dbContext, IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<RiskCategorySummary>> CreateCategoryAsync(CreateRiskCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.CategoryCodeExistsAsync(command.TenantId, command.Code.ToUpperInvariant(), cancellationToken))
            {
                return Result<RiskCategorySummary>.Failure("Risk category code already exists.");
            }

            var category = new RiskCategory(command.TenantId, command.Name, command.Code, command.RequestedByUserId);
            await _repository.AddCategoryAsync(category, cancellationToken);
            await AuditAsync(command.TenantId, category.Id, AuditAction.RiskCreated, command.RequestedByUserId, "Risk category created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<RiskCategorySummary>.Success(new RiskCategorySummary(category.Id, category.TenantId, category.Name, category.Code));
        }
        catch (DomainException exception)
        {
            return Result<RiskCategorySummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<RiskMatrixSummary>> CreateMatrixAsync(CreateRiskMatrixCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var matrix = new RiskMatrix(command.TenantId, command.Name, command.ToleranceScore, command.RequestedByUserId);
            await _repository.AddMatrixAsync(matrix, cancellationToken);
            await AuditAsync(command.TenantId, matrix.Id, AuditAction.RiskCreated, command.RequestedByUserId, "Risk matrix created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<RiskMatrixSummary>.Success(new RiskMatrixSummary(matrix.Id, matrix.TenantId, matrix.Name, matrix.ToleranceScore));
        }
        catch (DomainException exception)
        {
            return Result<RiskMatrixSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<RiskSummary>> CreateRiskAsync(CreateRiskCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.GetCategoryAsync(command.TenantId, command.CategoryId, cancellationToken) is null)
            {
                return Result<RiskSummary>.Failure("Risk category not found.");
            }

            if (await _repository.RiskCodeExistsAsync(command.TenantId, command.Code.ToUpperInvariant(), cancellationToken))
            {
                return Result<RiskSummary>.Failure("Risk code already exists.");
            }

            var risk = new Risk(command.TenantId, command.CategoryId, command.Title, command.Code, command.Description, command.Type, command.Area, command.Process, command.SupplierId, command.DocumentId, command.AuditId, command.CapaId, command.RequestedByUserId, _clock.UtcNow);
            await _repository.AddRiskAsync(risk, cancellationToken);
            await AuditAsync(command.TenantId, risk.Id, AuditAction.RiskCreated, command.RequestedByUserId, "Risk created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<RiskSummary>.Success(ToSummary(risk));
        }
        catch (DomainException exception)
        {
            return Result<RiskSummary>.Failure(exception.Message);
        }
    }

    public Task<Result> ClassifyAsync(ClassifyRiskCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.RiskId, command.RequestedByUserId, AuditAction.RiskUpdated, "Risk classified.", risk => risk.Classify(command.Type, command.Area, command.Process, command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public async Task<Result<RiskOwnerSummary>> AssignOwnerAsync(AssignRiskOwnerCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.RiskId, command.RequestedByUserId, "Risk owner assigned.", risk => ToSummary(risk.AssignOwner(command.OwnerUserId, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<RiskAssessmentSummary>> AssessAsync(AssessRiskCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.RiskId, command.RequestedByUserId, "Risk assessed.", risk =>
        {
            var assessment = risk.Assess(command.Probability, command.Impact, command.ResidualProbability, command.ResidualImpact, command.ToleranceScore, command.RequestedByUserId, _clock.UtcNow);
            return ToSummary(assessment);
        }, cancellationToken);
    }

    public async Task<Result<RiskTreatmentSummary>> AddTreatmentAsync(AddRiskTreatmentCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.RiskId, command.RequestedByUserId, "Risk treatment registered.", risk => ToSummary(risk.AddTreatment(command.Strategy, command.Rationale, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<RiskMitigationPlanSummary>> AddMitigationPlanAsync(AddRiskMitigationPlanCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.RiskId, command.RequestedByUserId, "Risk mitigation registered.", risk => ToSummary(risk.AddMitigationPlan(command.Description, command.ResponsibleUserId, command.DueAtUtc, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<RiskControlSummary>> AddControlAsync(AddRiskControlCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.RiskId, command.RequestedByUserId, "Risk control registered.", risk => ToSummary(risk.AddControl(command.Name, command.Type, command.Description, command.IsEffective, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<RiskEvidenceSummary>> AddEvidenceAsync(AddRiskEvidenceCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.RiskId, command.RequestedByUserId, "Risk evidence attached.", risk => ToSummary(risk.AddEvidence(command.StoredFileId, command.FileName, command.ContentType, command.SizeBytes, command.Sha256Hash, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<RiskAttachmentSummary>> AddAttachmentAsync(AddRiskAttachmentCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.RiskId, command.RequestedByUserId, "Risk attachment added.", risk => ToSummary(risk.AddAttachment(command.StoredFileId, command.FileName, command.ContentType, command.SizeBytes, command.Sha256Hash, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public async Task<Result<RiskReviewSummary>> ScheduleReviewAsync(ScheduleRiskReviewCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.RiskId, command.RequestedByUserId, "Risk review scheduled.", risk => ToSummary(risk.ScheduleReview(command.DueAtUtc, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public Task<Result> CompleteReviewAsync(CompleteRiskReviewCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.RiskId, command.RequestedByUserId, AuditAction.RiskUpdated, "Risk review completed.", risk => risk.CompleteReview(command.ReviewId, command.Summary, command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public async Task<Result<RiskIndicatorSummary>> AddIndicatorAsync(AddRiskIndicatorCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWithValueAsync(command.TenantId, command.RiskId, command.RequestedByUserId, "Risk indicator registered.", risk => ToSummary(risk.AddIndicator(command.Name, command.Value, command.Threshold, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);
    }

    public Task<Result> EscalateCriticalAsync(RiskActionCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.RiskId, command.RequestedByUserId, AuditAction.RiskUpdated, "Critical risk escalated.", risk => risk.EscalateCritical(command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public Task<Result> AttachWorkflowAsync(AttachRiskWorkflowCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.RiskId, command.RequestedByUserId, AuditAction.RiskUpdated, "Risk workflow attached.", risk => risk.AttachWorkflow(command.WorkflowInstanceId, command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public Task<Result> CloseAsync(RiskActionCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.RiskId, command.RequestedByUserId, AuditAction.RiskClosed, "Risk closed.", risk => risk.Close(command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public Task<Result> ReopenAsync(ReopenRiskCommand command, CancellationToken cancellationToken = default)
    {
        return ChangeAsync(command.TenantId, command.RiskId, command.RequestedByUserId, AuditAction.RiskReopened, "Risk reopened.", risk => risk.Reopen(command.Reason, command.RequestedByUserId, _clock.UtcNow), cancellationToken);
    }

    public async Task<Result<RiskSearchResult>> SearchAsync(RiskSearchQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 25 : Math.Min(query.PageSize, 100);
        return Result<RiskSearchResult>.Success(await _repository.SearchAsync(new RiskSearchCriteria(query.TenantId, query.SearchText, query.Status, query.Type, query.Level, query.Area, query.SupplierId, query.AuditId, query.CapaId, page, pageSize), cancellationToken));
    }

    public async Task<Result<RiskDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return Result<RiskDashboardDto>.Success(await _repository.GetDashboardAsync(tenantId, _clock.UtcNow, cancellationToken));
    }

    public async Task<Result<IReadOnlyCollection<RiskHeatMapPoint>>> GetHeatMapAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return Result<IReadOnlyCollection<RiskHeatMapPoint>>.Success(await _repository.GetHeatMapAsync(tenantId, cancellationToken));
    }

    public async Task<Result<RiskExportDescriptor>> ExportAsync(RiskExportQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _repository.SearchAsync(new RiskSearchCriteria(query.TenantId, null, query.Status, query.Type, query.Level, null, null, null, null, 1, 10_000), cancellationToken);
        var format = string.IsNullOrWhiteSpace(query.Format) ? "csv" : query.Format.Trim().ToLowerInvariant();
        var contentType = format switch
        {
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "json" => "application/json",
            _ => "text/csv"
        };
        await AuditAsync(query.TenantId, null, AuditAction.Exported, query.RequestedByUserId, "Risk export generated.", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<RiskExportDescriptor>.Success(new RiskExportDescriptor(query.TenantId, format, $"risks-{query.TenantId:N}.{format}", contentType, result.TotalCount));
    }

    private async Task<Result> ChangeAsync(Guid tenantId, Guid riskId, Guid userId, AuditAction action, string message, Action<Risk> change, CancellationToken cancellationToken)
    {
        var risk = await _repository.GetRiskAsync(tenantId, riskId, cancellationToken);
        if (risk is null)
        {
            return Result.Failure("Risk not found.");
        }

        try
        {
            change(risk);
            await AuditAsync(tenantId, risk.Id, action, userId, message, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private async Task<Result<T>> ChangeWithValueAsync<T>(Guid tenantId, Guid riskId, Guid userId, string message, Func<Risk, T> change, CancellationToken cancellationToken)
    {
        var risk = await _repository.GetRiskAsync(tenantId, riskId, cancellationToken);
        if (risk is null)
        {
            return Result<T>.Failure("Risk not found.");
        }

        try
        {
            var value = change(risk);
            await AuditAsync(tenantId, risk.Id, AuditAction.RiskUpdated, userId, message, cancellationToken);
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
        return _repository.AddAuditLogAsync(AuditLog.FromEvent(new AuditEvent(nameof(Risk), entityId, action, AuditLog.InferCategory(action), new AuditContext(tenantId, userId, null, null, null, null, null, null, null), new AuditSnapshot(null, null), new AuditMetadata($"{{\"source\":\"risk-management\",\"message\":\"{message}\"}}"), true, null), _clock.UtcNow), cancellationToken);
    }

    private static RiskSummary ToSummary(Risk risk) => new(risk.Id, risk.TenantId, risk.Title, risk.Code, risk.Type, risk.Status, risk.InherentLevel, risk.ResidualLevel, risk.InherentScore, risk.ResidualScore, risk.Area, risk.Process, risk.SupplierId, risk.AuditId, risk.CapaId);
    private static RiskOwnerSummary ToSummary(RiskOwner owner) => new(owner.Id, owner.RiskId, owner.UserId, owner.IsActive);
    private static RiskAssessmentSummary ToSummary(RiskAssessment assessment) => new(assessment.Id, assessment.RiskId, assessment.InherentScore, assessment.InherentLevel, assessment.ResidualScore, assessment.ResidualLevel, assessment.ResidualScore <= assessment.ToleranceScore);
    private static RiskTreatmentSummary ToSummary(RiskTreatment treatment) => new(treatment.Id, treatment.RiskId, treatment.Strategy, treatment.Rationale);
    private static RiskMitigationPlanSummary ToSummary(RiskMitigationPlan plan) => new(plan.Id, plan.RiskId, plan.Description, plan.ResponsibleUserId, plan.DueAtUtc, plan.IsCompleted);
    private static RiskControlSummary ToSummary(RiskControl control) => new(control.Id, control.RiskId, control.Name, control.Type, control.IsEffective);
    private static RiskEvidenceSummary ToSummary(RiskEvidence evidence) => new(evidence.Id, evidence.RiskId, evidence.FileName, evidence.ContentType, evidence.SizeBytes, evidence.Sha256Hash);
    private static RiskAttachmentSummary ToSummary(RiskAttachment attachment) => new(attachment.Id, attachment.RiskId, attachment.FileName, attachment.ContentType, attachment.SizeBytes, attachment.Sha256Hash);
    private static RiskReviewSummary ToSummary(RiskReview review) => new(review.Id, review.RiskId, review.DueAtUtc, review.Status);
    private static RiskIndicatorSummary ToSummary(RiskIndicator indicator) => new(indicator.Id, indicator.RiskId, indicator.Name, indicator.Value, indicator.Threshold, indicator.IsBreached);
}
