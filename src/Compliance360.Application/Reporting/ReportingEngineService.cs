using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Reporting;
using Compliance360.Shared;

namespace Compliance360.Application.Reporting;

public sealed class ReportingEngineService : IReportingEngineService
{
    private readonly IReportingEngineRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public ReportingEngineService(IReportingEngineRepository repository, IApplicationDbContext dbContext, IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<ReportCategorySummary>> CreateCategoryAsync(CreateReportCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.CategoryCodeExistsAsync(command.TenantId, command.Code, cancellationToken))
            {
                return Result<ReportCategorySummary>.Failure("Report category code already exists.");
            }

            var category = new ReportCategory(command.TenantId, command.Name, command.Code, command.Module, command.RequestedByUserId);
            await _repository.AddCategoryAsync(category, cancellationToken);
            await AuditAsync(command.TenantId, category.Id, AuditAction.ReportCreated, command.RequestedByUserId, "Report category created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<ReportCategorySummary>.Success(ToSummary(category));
        }
        catch (DomainException exception)
        {
            return Result<ReportCategorySummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<ReportDefinitionSummary>> CreateDefinitionAsync(CreateReportDefinitionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.GetCategoryAsync(command.TenantId, command.CategoryId, cancellationToken) is null)
            {
                return Result<ReportDefinitionSummary>.Failure("Report category not found.");
            }

            if (await _repository.DefinitionCodeExistsAsync(command.TenantId, command.Code, cancellationToken))
            {
                return Result<ReportDefinitionSummary>.Failure("Report definition code already exists.");
            }

            var definition = new ReportDefinition(command.TenantId, command.CategoryId, command.Name, command.Code, command.Description, command.Module, command.DatasetKey, command.RequestedByUserId, _clock.UtcNow);
            await _repository.AddDefinitionAsync(definition, cancellationToken);
            await AuditAsync(command.TenantId, definition.Id, AuditAction.ReportCreated, command.RequestedByUserId, "Report definition created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<ReportDefinitionSummary>.Success(ToSummary(definition));
        }
        catch (DomainException exception)
        {
            return Result<ReportDefinitionSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<ReportTemplateSummary>> AddTemplateAsync(AddReportTemplateCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.ReportDefinitionId, command.RequestedByUserId, AuditAction.ReportUpdated, "Report template added.", definition => ToSummary(definition.AddTemplate(command.Name, command.Format, command.Content, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<ReportParameterSummary>> AddParameterAsync(AddReportParameterCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.ReportDefinitionId, command.RequestedByUserId, AuditAction.ReportUpdated, "Report parameter added.", definition => ToSummary(definition.AddParameter(command.Name, command.Label, command.Type, command.IsRequired, command.DefaultValue, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<ReportPermissionSummary>> GrantPermissionAsync(GrantReportPermissionCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.ReportDefinitionId, command.RequestedByUserId, AuditAction.ReportUpdated, "Report permission granted.", definition => ToSummary(definition.GrantPermission(command.Scope, command.Subject, command.CanExecute, command.CanExport, command.CanSchedule, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result> ActivateAsync(ReportActionCommand command, CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetDefinitionAsync(command.TenantId, command.ReportDefinitionId, cancellationToken);
        if (definition is null)
        {
            return Result.Failure("Report definition not found.");
        }

        try
        {
            definition.Activate(command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, definition.Id, AuditAction.ReportUpdated, command.RequestedByUserId, "Report definition activated.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result<ReportExecutionSummary>> ExecuteAsync(ExecuteReportCommand command, CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetDefinitionAsync(command.TenantId, command.ReportDefinitionId, cancellationToken);
        if (definition is null)
        {
            return Result<ReportExecutionSummary>.Failure("Report definition not found.");
        }

        if (!definition.CanExecute(command.Permissions, command.RequestedByUserId))
        {
            return Result<ReportExecutionSummary>.Failure("Report execution permission denied.");
        }

        try
        {
            var execution = definition.StartExecution(command.ParametersJson, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, definition.Id, AuditAction.ReportExecuted, command.RequestedByUserId, "Report execution started.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<ReportExecutionSummary>.Success(ToSummary(execution));
        }
        catch (DomainException exception)
        {
            return Result<ReportExecutionSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<ReportOutputSummary>> CompleteExecutionAsync(CompleteReportExecutionCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.ReportDefinitionId, command.RequestedByUserId, AuditAction.ReportExecuted, "Report execution completed.", definition => ToSummary(definition.CompleteExecution(command.ExecutionId, command.RowCount, command.DatasetDescriptorJson, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<ReportExportSummary>> ExportAsync(ExportReportCommand command, CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetDefinitionAsync(command.TenantId, command.ReportDefinitionId, cancellationToken);
        if (definition is null)
        {
            return Result<ReportExportSummary>.Failure("Report definition not found.");
        }

        if (!definition.CanExport(command.Permissions, command.RequestedByUserId))
        {
            return Result<ReportExportSummary>.Failure("Report export permission denied.");
        }

        try
        {
            var export = definition.Export(command.ExecutionId, command.Format, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, definition.Id, AuditAction.ReportExported, command.RequestedByUserId, "Report export generated.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<ReportExportSummary>.Success(ToSummary(export));
        }
        catch (DomainException exception)
        {
            return Result<ReportExportSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<ReportScheduleSummary>> ScheduleAsync(ScheduleReportCommand command, CancellationToken cancellationToken = default)
    {
        var definition = await _repository.GetDefinitionAsync(command.TenantId, command.ReportDefinitionId, cancellationToken);
        if (definition is null)
        {
            return Result<ReportScheduleSummary>.Failure("Report definition not found.");
        }

        if (!definition.CanExecute(command.Permissions, command.RequestedByUserId))
        {
            return Result<ReportScheduleSummary>.Failure("Report schedule permission denied.");
        }

        try
        {
            var schedule = definition.Schedule(command.Frequency, command.NextRunUtc, command.RequestedByUserId, _clock.UtcNow);
            await AuditAsync(command.TenantId, definition.Id, AuditAction.ReportScheduled, command.RequestedByUserId, "Report schedule created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<ReportScheduleSummary>.Success(ToSummary(schedule));
        }
        catch (DomainException exception)
        {
            return Result<ReportScheduleSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<ReportSubscriptionSummary>> SubscribeAsync(SubscribeReportCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.ReportDefinitionId, command.RequestedByUserId, AuditAction.ReportScheduled, "Report subscription created.", definition => ToSummary(definition.Subscribe(command.Recipient, command.Format, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<ReportDashboardBindingSummary>> BindDashboardAsync(BindReportDashboardCommand command, CancellationToken cancellationToken = default) =>
        await ChangeWithValueAsync(command.TenantId, command.ReportDefinitionId, command.RequestedByUserId, AuditAction.ReportUpdated, "Report dashboard binding created.", definition => ToSummary(definition.BindDashboard(command.DashboardKey, command.DatasetKey, command.RequestedByUserId, _clock.UtcNow)), cancellationToken);

    public async Task<Result<ReportSearchResult>> SearchAsync(ReportSearchQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 25 : Math.Min(query.PageSize, 100);
        return Result<ReportSearchResult>.Success(await _repository.SearchAsync(new ReportSearchCriteria(query.TenantId, query.SearchText, query.Module, query.Status, page, pageSize), cancellationToken));
    }

    public async Task<Result<ReportingDashboardDatasetCatalog>> GetDashboardDatasetsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Result<ReportingDashboardDatasetCatalog>.Success(await _repository.GetDashboardDatasetsAsync(tenantId, cancellationToken));

    public Task<Result<IReadOnlyCollection<StandardReportDescriptor>>> GetStandardReportsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<IReadOnlyCollection<StandardReportDescriptor>>.Success(ReportDefinition.StandardReports()));

    public async Task<Result<SeedStandardReportsResult>> SeedStandardReportsAsync(SeedStandardReportsCommand command, CancellationToken cancellationToken = default)
    {
        var createdCategories = 0;
        var createdDefinitions = 0;
        var categoriesByCode = new Dictionary<string, ReportCategory>(StringComparer.OrdinalIgnoreCase);
        foreach (var descriptor in ReportDefinition.StandardReports())
        {
            var categoryCode = $"STD-{descriptor.Module}";
            if (!categoriesByCode.TryGetValue(categoryCode, out var category))
            {
                category = await _repository.GetCategoryByCodeAsync(command.TenantId, categoryCode.ToUpperInvariant(), cancellationToken);
                if (category is null)
                {
                    category = new ReportCategory(command.TenantId, $"{descriptor.Module} Reports", categoryCode, descriptor.Module, command.RequestedByUserId);
                    await _repository.AddCategoryAsync(category, cancellationToken);
                    createdCategories++;
                }

                categoriesByCode[categoryCode] = category;
            }

            if (await _repository.DefinitionCodeExistsAsync(command.TenantId, descriptor.Code, cancellationToken))
            {
                continue;
            }

            var definition = new ReportDefinition(command.TenantId, category.Id, descriptor.Name, descriptor.Code, $"Enterprise standard report for {descriptor.Name}.", descriptor.Module, descriptor.DatasetKey, command.RequestedByUserId, _clock.UtcNow);
            definition.AddTemplate("Enterprise default", ReportFormat.Pdf, $"Template for {descriptor.Name}", command.RequestedByUserId, _clock.UtcNow);
            definition.Activate(command.RequestedByUserId, _clock.UtcNow);
            definition.BindDashboard($"dashboard.{descriptor.Module}", descriptor.DatasetKey, command.RequestedByUserId, _clock.UtcNow);
            await _repository.AddDefinitionAsync(definition, cancellationToken);
            createdDefinitions++;
        }

        await AuditAsync(command.TenantId, null, AuditAction.ReportCreated, command.RequestedByUserId, "Standard enterprise reports seeded.", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<SeedStandardReportsResult>.Success(new SeedStandardReportsResult(createdCategories, createdDefinitions, ReportDefinition.StandardReports().Count));
    }

    private async Task<Result<T>> ChangeWithValueAsync<T>(Guid tenantId, Guid definitionId, Guid userId, AuditAction action, string message, Func<ReportDefinition, T> change, CancellationToken cancellationToken)
    {
        var definition = await _repository.GetDefinitionAsync(tenantId, definitionId, cancellationToken);
        if (definition is null)
        {
            return Result<T>.Failure("Report definition not found.");
        }

        try
        {
            var value = change(definition);
            await AuditAsync(tenantId, definition.Id, action, userId, message, cancellationToken);
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
        return _repository.AddAuditLogAsync(AuditLog.FromEvent(new AuditEvent(nameof(ReportDefinition), entityId, action, AuditLog.InferCategory(action), new AuditContext(tenantId, userId, null, null, null, null, null, null, null), new AuditSnapshot(null, null), new AuditMetadata($"{{\"source\":\"reporting-engine\",\"message\":\"{message}\"}}"), true, null), _clock.UtcNow), cancellationToken);
    }

    private static ReportCategorySummary ToSummary(ReportCategory category) => new(category.Id, category.TenantId, category.Name, category.Code, category.Module);
    private static ReportDefinitionSummary ToSummary(ReportDefinition definition) => new(definition.Id, definition.TenantId, definition.Name, definition.Code, definition.Module, definition.DatasetKey, definition.Version, definition.Status);
    private static ReportTemplateSummary ToSummary(ReportTemplate template) => new(template.Id, template.ReportDefinitionId, template.Name, template.Format, template.Version);
    private static ReportParameterSummary ToSummary(ReportParameter parameter) => new(parameter.Id, parameter.ReportDefinitionId, parameter.Name, parameter.Label, parameter.Type, parameter.IsRequired, parameter.DefaultValue);
    private static ReportPermissionSummary ToSummary(ReportPermission permission) => new(permission.Id, permission.ReportDefinitionId, permission.Scope, permission.Subject, permission.CanExecute, permission.CanExport, permission.CanSchedule);
    private static ReportExecutionSummary ToSummary(ReportExecution execution) => new(execution.Id, execution.ReportDefinitionId, execution.Status, execution.RowCount, execution.QueuedAtUtc, execution.CompletedAtUtc);
    private static ReportOutputSummary ToSummary(ReportOutput output) => new(output.Id, output.ReportDefinitionId, output.ReportExecutionId, output.RowCount, output.DatasetDescriptorJson);
    private static ReportExportSummary ToSummary(ReportExport export) => new(export.Id, export.ReportDefinitionId, export.ReportExecutionId, export.Format, export.FileName, export.ContentType);
    private static ReportScheduleSummary ToSummary(ReportSchedule schedule) => new(schedule.Id, schedule.ReportDefinitionId, schedule.Frequency, schedule.NextRunUtc, schedule.IsActive);
    private static ReportSubscriptionSummary ToSummary(ReportSubscription subscription) => new(subscription.Id, subscription.ReportDefinitionId, subscription.Recipient, subscription.Format, subscription.IsActive);
    private static ReportDashboardBindingSummary ToSummary(ReportDashboardBinding binding) => new(binding.Id, binding.ReportDefinitionId, binding.DashboardKey, binding.DatasetKey);
}
