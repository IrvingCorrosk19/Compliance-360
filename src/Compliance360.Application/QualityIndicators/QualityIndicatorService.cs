using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.QualityIndicators;
using Compliance360.Shared;

namespace Compliance360.Application.QualityIndicators;

public sealed class QualityIndicatorService : IQualityIndicatorService
{
    private readonly IQualityIndicatorRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public QualityIndicatorService(IQualityIndicatorRepository repository, IApplicationDbContext dbContext, IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<IndicatorCategorySummary>> CreateCategoryAsync(CreateIndicatorCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.CategoryCodeExistsAsync(command.TenantId, command.Code.ToUpperInvariant(), cancellationToken))
            {
                return Result<IndicatorCategorySummary>.Failure("Indicator category code already exists.");
            }

            var category = new IndicatorCategory(command.TenantId, command.Name, command.Code, command.RequestedByUserId);
            await _repository.AddCategoryAsync(category, cancellationToken);
            await AuditAsync(command.TenantId, category.Id, AuditAction.IndicatorCreated, command.RequestedByUserId, "Indicator category created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<IndicatorCategorySummary>.Success(new IndicatorCategorySummary(category.Id, category.TenantId, category.Name, category.Code));
        }
        catch (DomainException exception)
        {
            return Result<IndicatorCategorySummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<QualityIndicatorSummary>> CreateIndicatorAsync(CreateQualityIndicatorCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.GetCategoryAsync(command.TenantId, command.CategoryId, cancellationToken) is null)
            {
                return Result<QualityIndicatorSummary>.Failure("Indicator category not found.");
            }

            if (await _repository.IndicatorCodeExistsAsync(command.TenantId, command.Code.ToUpperInvariant(), cancellationToken))
            {
                return Result<QualityIndicatorSummary>.Failure("Indicator code already exists.");
            }

            var indicator = new QualityIndicator(command.TenantId, command.CategoryId, command.Name, command.Code, command.Description, command.Type, command.Frequency, command.CalculationType, command.Unit, command.SupplierId, command.AuditId, command.CapaId, command.RiskId, command.DocumentId, command.RequestedByUserId, _clock.UtcNow);
            await _repository.AddIndicatorAsync(indicator, cancellationToken);
            await AuditAsync(command.TenantId, indicator.Id, AuditAction.IndicatorCreated, command.RequestedByUserId, "Indicator created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<QualityIndicatorSummary>.Success(ToSummary(indicator));
        }
        catch (DomainException exception)
        {
            return Result<QualityIndicatorSummary>.Failure(exception.Message);
        }
    }

    public Task<Result> ActivateAsync(IndicatorActionCommand command, CancellationToken cancellationToken = default) =>
        ChangeAsync(command.TenantId, command.IndicatorId, command.RequestedByUserId, AuditAction.IndicatorUpdated, "Indicator activated.", indicator => indicator.Activate(command.RequestedByUserId, _clock.UtcNow), cancellationToken);

    public async Task<Result<IndicatorFormulaSummary>> DefineFormulaAsync(DefineIndicatorFormulaCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.IndicatorId, command.RequestedByUserId, "Formula defined.", indicator => ToSummary(indicator.DefineFormula(command.Expression, command.CalculationType, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<IndicatorTargetSummary>> DefineTargetAsync(DefineIndicatorTargetCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.IndicatorId, command.RequestedByUserId, "Target defined.", indicator => ToSummary(indicator.DefineTarget(command.TargetValue, command.EffectiveFromUtc, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<IndicatorThresholdSummary>> DefineThresholdAsync(DefineIndicatorThresholdCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.IndicatorId, command.RequestedByUserId, "Threshold defined.", indicator => ToSummary(indicator.DefineThreshold(command.WarningMinimum, command.CriticalMinimum, command.ExcellentMinimum, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<IndicatorPeriodSummary>> AddPeriodAsync(AddIndicatorPeriodCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.IndicatorId, command.RequestedByUserId, "Period added.", indicator => ToSummary(indicator.AddPeriod(command.Year, command.PeriodNumber, command.StartUtc, command.EndUtc, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<IndicatorProcessSummary>> AssociateProcessAsync(AssociateIndicatorProcessCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.IndicatorId, command.RequestedByUserId, "Process associated.", indicator => ToSummary(indicator.AssociateProcess(command.ProcessName, command.Area, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<IndicatorMeasurementSummary>> CaptureMeasurementAsync(CaptureIndicatorMeasurementCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.IndicatorId, command.RequestedByUserId, "Measurement captured.", indicator => ToSummary(indicator.CaptureMeasurement(command.PeriodId, command.Numerator, command.Denominator, command.IsAutomatic, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<IndicatorResultSummary>> CalculateResultAsync(CalculateIndicatorResultCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.IndicatorId, command.RequestedByUserId, "Result calculated.", indicator => ToSummary(indicator.CalculateResult(command.PeriodId, command.MeasurementId, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<IndicatorAttachmentSummary>> AddAttachmentAsync(AddIndicatorAttachmentCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.IndicatorId, command.RequestedByUserId, "Attachment added.", indicator => ToSummary(indicator.AddAttachment(command.StoredFileId, command.FileName, command.ContentType, command.SizeBytes, command.Sha256Hash, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public Task<Result> AttachWorkflowAsync(AttachIndicatorWorkflowCommand command, CancellationToken cancellationToken = default) =>
        ChangeAsync(command.TenantId, command.IndicatorId, command.RequestedByUserId, AuditAction.IndicatorUpdated, "Indicator workflow attached.", indicator => indicator.AttachWorkflow(command.WorkflowInstanceId, command.RequestedByUserId, _clock.UtcNow), cancellationToken);

    public Task<Result> ApproveAsync(IndicatorActionCommand command, CancellationToken cancellationToken = default) =>
        ChangeAsync(command.TenantId, command.IndicatorId, command.RequestedByUserId, AuditAction.IndicatorApproved, "Indicator approved.", indicator => indicator.Approve(command.RequestedByUserId, _clock.UtcNow), cancellationToken);

    public async Task<Result<IndicatorSearchResult>> SearchAsync(IndicatorSearchQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 25 : Math.Min(query.PageSize, 100);
        return Result<IndicatorSearchResult>.Success(await _repository.SearchAsync(new IndicatorSearchCriteria(query.TenantId, query.SearchText, query.Status, query.Type, query.Frequency, query.SupplierId, query.AuditId, query.CapaId, query.RiskId, page, pageSize), cancellationToken));
    }

    public async Task<Result<IndicatorDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Result<IndicatorDashboardDto>.Success(await _repository.GetDashboardAsync(tenantId, cancellationToken));

    public async Task<Result<IReadOnlyCollection<IndicatorTrendSummary>>> GetTrendsAsync(Guid tenantId, Guid? indicatorId, CancellationToken cancellationToken = default) =>
        Result<IReadOnlyCollection<IndicatorTrendSummary>>.Success(await _repository.GetTrendsAsync(tenantId, indicatorId, cancellationToken));

    public async Task<Result<IndicatorExportDescriptor>> ExportAsync(IndicatorExportQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _repository.SearchAsync(new IndicatorSearchCriteria(query.TenantId, null, query.Status, query.Type, null, null, null, null, null, 1, 10_000), cancellationToken);
        var format = string.IsNullOrWhiteSpace(query.Format) ? "csv" : query.Format.Trim().ToLowerInvariant();
        var contentType = format switch
        {
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "json" => "application/json",
            _ => "text/csv"
        };
        await AuditAsync(query.TenantId, null, AuditAction.IndicatorExported, query.RequestedByUserId, "Indicator export generated.", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<IndicatorExportDescriptor>.Success(new IndicatorExportDescriptor(query.TenantId, format, $"indicators-{query.TenantId:N}.{format}", contentType, result.TotalCount));
    }

    private async Task<Result> ChangeAsync(Guid tenantId, Guid indicatorId, Guid userId, AuditAction action, string message, Action<QualityIndicator> change, CancellationToken cancellationToken)
    {
        var indicator = await _repository.GetIndicatorAsync(tenantId, indicatorId, cancellationToken);
        if (indicator is null) return Result.Failure("Indicator not found.");
        try
        {
            change(indicator);
            await AuditAsync(tenantId, indicator.Id, action, userId, message, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception) { return Result.Failure(exception.Message); }
    }

    private async Task<Result<T>> ChangeWithValueAsync<T>(Guid tenantId, Guid indicatorId, Guid userId, string message, Func<QualityIndicator, T> change, CancellationToken cancellationToken)
    {
        var indicator = await _repository.GetIndicatorAsync(tenantId, indicatorId, cancellationToken);
        if (indicator is null) return Result<T>.Failure("Indicator not found.");
        try
        {
            var value = change(indicator);
            await AuditAsync(tenantId, indicator.Id, AuditAction.IndicatorUpdated, userId, message, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<T>.Success(value);
        }
        catch (DomainException exception) { return Result<T>.Failure(exception.Message); }
    }

    private Task AuditAsync(Guid tenantId, Guid? entityId, AuditAction action, Guid userId, string message, CancellationToken cancellationToken)
    {
        return _repository.AddAuditLogAsync(AuditLog.FromEvent(new AuditEvent(nameof(QualityIndicator), entityId, action, AuditLog.InferCategory(action), new AuditContext(tenantId, userId, null, null, null, null, null, null, null), new AuditSnapshot(null, null), new AuditMetadata($"{{\"source\":\"quality-indicators\",\"message\":\"{message}\"}}"), true, null), _clock.UtcNow), cancellationToken);
    }

    private static QualityIndicatorSummary ToSummary(QualityIndicator indicator) => new(indicator.Id, indicator.TenantId, indicator.Name, indicator.Code, indicator.Type, indicator.Frequency, indicator.CalculationType, indicator.Status, indicator.Unit);
    private static IndicatorFormulaSummary ToSummary(IndicatorFormula formula) => new(formula.Id, formula.IndicatorId, formula.Expression, formula.CalculationType);
    private static IndicatorTargetSummary ToSummary(IndicatorTarget target) => new(target.Id, target.IndicatorId, target.TargetValue, target.EffectiveFromUtc);
    private static IndicatorThresholdSummary ToSummary(IndicatorThreshold threshold) => new(threshold.Id, threshold.IndicatorId, threshold.WarningMinimum, threshold.CriticalMinimum, threshold.ExcellentMinimum);
    private static IndicatorPeriodSummary ToSummary(IndicatorPeriod period) => new(period.Id, period.IndicatorId, period.Year, period.PeriodNumber, period.StartUtc, period.EndUtc);
    private static IndicatorProcessSummary ToSummary(IndicatorProcess process) => new(process.Id, process.IndicatorId, process.ProcessName, process.Area);
    private static IndicatorMeasurementSummary ToSummary(IndicatorMeasurement measurement) => new(measurement.Id, measurement.IndicatorId, measurement.PeriodId, measurement.Numerator, measurement.Denominator, measurement.IsAutomatic);
    private static IndicatorResultSummary ToSummary(IndicatorResult result) => new(result.Id, result.IndicatorId, result.PeriodId, result.Value, result.TargetValue, result.Status);
    private static IndicatorAttachmentSummary ToSummary(IndicatorAttachment attachment) => new(attachment.Id, attachment.IndicatorId, attachment.FileName, attachment.ContentType, attachment.SizeBytes, attachment.Sha256Hash);
}
