using Compliance360.Domain.Audit;
using Compliance360.Domain.Reporting;
using Compliance360.Shared;

namespace Compliance360.Application.Reporting;

public interface IReportingEngineService
{
    Task<Result<ReportCategorySummary>> CreateCategoryAsync(CreateReportCategoryCommand command, CancellationToken cancellationToken = default);
    Task<Result<ReportDefinitionSummary>> CreateDefinitionAsync(CreateReportDefinitionCommand command, CancellationToken cancellationToken = default);
    Task<Result<ReportTemplateSummary>> AddTemplateAsync(AddReportTemplateCommand command, CancellationToken cancellationToken = default);
    Task<Result<ReportParameterSummary>> AddParameterAsync(AddReportParameterCommand command, CancellationToken cancellationToken = default);
    Task<Result<ReportPermissionSummary>> GrantPermissionAsync(GrantReportPermissionCommand command, CancellationToken cancellationToken = default);
    Task<Result> ActivateAsync(ReportActionCommand command, CancellationToken cancellationToken = default);
    Task<Result<ReportExecutionSummary>> ExecuteAsync(ExecuteReportCommand command, CancellationToken cancellationToken = default);
    Task<Result<ReportOutputSummary>> CompleteExecutionAsync(CompleteReportExecutionCommand command, CancellationToken cancellationToken = default);
    Task<Result<ReportExportSummary>> ExportAsync(ExportReportCommand command, CancellationToken cancellationToken = default);
    Task<Result<ReportScheduleSummary>> ScheduleAsync(ScheduleReportCommand command, CancellationToken cancellationToken = default);
    Task<Result<ReportSubscriptionSummary>> SubscribeAsync(SubscribeReportCommand command, CancellationToken cancellationToken = default);
    Task<Result<ReportDashboardBindingSummary>> BindDashboardAsync(BindReportDashboardCommand command, CancellationToken cancellationToken = default);
    Task<Result<ReportSearchResult>> SearchAsync(ReportSearchQuery query, CancellationToken cancellationToken = default);
    Task<Result<ReportingDashboardDatasetCatalog>> GetDashboardDatasetsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<StandardReportDescriptor>>> GetStandardReportsAsync(CancellationToken cancellationToken = default);
    Task<Result<SeedStandardReportsResult>> SeedStandardReportsAsync(SeedStandardReportsCommand command, CancellationToken cancellationToken = default);
}

public interface IReportingEngineRepository
{
    Task AddCategoryAsync(ReportCategory category, CancellationToken cancellationToken = default);
    Task<ReportCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default);
    Task<ReportCategory?> GetCategoryByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task<bool> CategoryCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task AddDefinitionAsync(ReportDefinition definition, CancellationToken cancellationToken = default);
    Task<ReportDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken = default);
    Task<bool> DefinitionCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task<ReportSearchResult> SearchAsync(ReportSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<ReportingDashboardDatasetCatalog> GetDashboardDatasetsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateReportCategoryCommand(Guid TenantId, string Name, string Code, ReportModule Module, Guid RequestedByUserId);
public sealed record CreateReportDefinitionCommand(Guid TenantId, Guid CategoryId, string Name, string Code, string Description, ReportModule Module, string DatasetKey, Guid RequestedByUserId);
public sealed record AddReportTemplateCommand(Guid TenantId, Guid ReportDefinitionId, string Name, ReportFormat Format, string Content, Guid RequestedByUserId);
public sealed record AddReportParameterCommand(Guid TenantId, Guid ReportDefinitionId, string Name, string Label, ReportParameterType Type, bool IsRequired, string? DefaultValue, Guid RequestedByUserId);
public sealed record GrantReportPermissionCommand(Guid TenantId, Guid ReportDefinitionId, ReportPermissionScope Scope, string Subject, bool CanExecute, bool CanExport, bool CanSchedule, Guid RequestedByUserId);
public sealed record ReportActionCommand(Guid TenantId, Guid ReportDefinitionId, Guid RequestedByUserId, IReadOnlyCollection<string> Permissions);
public sealed record ExecuteReportCommand(Guid TenantId, Guid ReportDefinitionId, string ParametersJson, Guid RequestedByUserId, IReadOnlyCollection<string> Permissions);
public sealed record CompleteReportExecutionCommand(Guid TenantId, Guid ReportDefinitionId, Guid ExecutionId, int RowCount, string DatasetDescriptorJson, Guid RequestedByUserId);
public sealed record ExportReportCommand(Guid TenantId, Guid ReportDefinitionId, Guid ExecutionId, ReportFormat Format, Guid RequestedByUserId, IReadOnlyCollection<string> Permissions);
public sealed record ScheduleReportCommand(Guid TenantId, Guid ReportDefinitionId, ReportScheduleFrequency Frequency, DateTimeOffset NextRunUtc, Guid RequestedByUserId, IReadOnlyCollection<string> Permissions);
public sealed record SubscribeReportCommand(Guid TenantId, Guid ReportDefinitionId, string Recipient, ReportFormat Format, Guid RequestedByUserId);
public sealed record BindReportDashboardCommand(Guid TenantId, Guid ReportDefinitionId, string DashboardKey, string DatasetKey, Guid RequestedByUserId);
public sealed record ReportSearchQuery(Guid TenantId, string? SearchText, ReportModule? Module, ReportDefinitionStatus? Status, int Page, int PageSize);
public sealed record ReportSearchCriteria(Guid TenantId, string? SearchText, ReportModule? Module, ReportDefinitionStatus? Status, int Page, int PageSize);
public sealed record SeedStandardReportsCommand(Guid TenantId, Guid RequestedByUserId);

public sealed record ReportCategorySummary(Guid Id, Guid TenantId, string Name, string Code, ReportModule Module);
public sealed record ReportDefinitionSummary(Guid Id, Guid TenantId, string Name, string Code, ReportModule Module, string DatasetKey, int Version, ReportDefinitionStatus Status);
public sealed record ReportTemplateSummary(Guid Id, Guid ReportDefinitionId, string Name, ReportFormat Format, int Version);
public sealed record ReportParameterSummary(Guid Id, Guid ReportDefinitionId, string Name, string Label, ReportParameterType Type, bool IsRequired, string? DefaultValue);
public sealed record ReportPermissionSummary(Guid Id, Guid ReportDefinitionId, ReportPermissionScope Scope, string Subject, bool CanExecute, bool CanExport, bool CanSchedule);
public sealed record ReportExecutionSummary(Guid Id, Guid ReportDefinitionId, ReportExecutionStatus Status, int RowCount, DateTimeOffset QueuedAtUtc, DateTimeOffset? CompletedAtUtc);
public sealed record ReportOutputSummary(Guid Id, Guid ReportDefinitionId, Guid ReportExecutionId, int RowCount, string DatasetDescriptorJson);
public sealed record ReportExportSummary(Guid Id, Guid ReportDefinitionId, Guid ReportExecutionId, ReportFormat Format, string FileName, string ContentType);
public sealed record ReportScheduleSummary(Guid Id, Guid ReportDefinitionId, ReportScheduleFrequency Frequency, DateTimeOffset NextRunUtc, bool IsActive);
public sealed record ReportSubscriptionSummary(Guid Id, Guid ReportDefinitionId, string Recipient, ReportFormat Format, bool IsActive);
public sealed record ReportDashboardBindingSummary(Guid Id, Guid ReportDefinitionId, string DashboardKey, string DatasetKey);
public sealed record ReportSearchResult(IReadOnlyCollection<ReportDefinitionSummary> Items, int TotalCount, int Page, int PageSize);
public sealed record ReportingDashboardDatasetCatalog(IReadOnlyCollection<ReportDashboardDatasetDescriptor> Datasets);
public sealed record ReportDashboardDatasetDescriptor(Guid ReportDefinitionId, string ReportCode, ReportModule Module, string DatasetKey, string DashboardKey);
public sealed record SeedStandardReportsResult(int CreatedCategories, int CreatedDefinitions, int TotalStandardReports);
