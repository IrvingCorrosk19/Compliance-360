using Compliance360.Application;
using Compliance360.Application.Reporting;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Reporting;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.Reporting;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class ReportingEngineTests
{
    [Fact]
    public async Task Reporting_Full_Flow_Executes_Exports_Schedules_Subscriptions_And_Audit()
    {
        var fixture = ReportingFixture.Create();
        var category = await fixture.Service.CreateCategoryAsync(new CreateReportCategoryCommand(fixture.TenantId, "Documents", "DOC", ReportModule.DocumentManagement, fixture.UserId));
        var definition = await fixture.Service.CreateDefinitionAsync(new CreateReportDefinitionCommand(fixture.TenantId, category.Value!.Id, "Documentos vigentes", "DOC-ACTIVE", "Active documents", ReportModule.DocumentManagement, "documents.active", fixture.UserId));
        var template = await fixture.Service.AddTemplateAsync(new AddReportTemplateCommand(fixture.TenantId, definition.Value!.Id, "PDF", ReportFormat.Pdf, "template-body", fixture.UserId));
        var parameter = await fixture.Service.AddParameterAsync(new AddReportParameterCommand(fixture.TenantId, definition.Value.Id, "area", "Area", ReportParameterType.Text, true, "Quality", fixture.UserId));
        var permission = await fixture.Service.GrantPermissionAsync(new GrantReportPermissionCommand(fixture.TenantId, definition.Value.Id, ReportPermissionScope.Permission, "REPORT.EXECUTE", true, true, true, fixture.UserId));
        var activate = await fixture.Service.ActivateAsync(new ReportActionCommand(fixture.TenantId, definition.Value.Id, fixture.UserId, fixture.ExecuteClaims));
        var execution = await fixture.Service.ExecuteAsync(new ExecuteReportCommand(fixture.TenantId, definition.Value.Id, "{\"area\":\"Quality\"}", fixture.UserId, fixture.ExecuteClaims));
        var output = await fixture.Service.CompleteExecutionAsync(new CompleteReportExecutionCommand(fixture.TenantId, definition.Value.Id, execution.Value!.Id, 25, "{\"dataset\":\"documents.active\"}", fixture.UserId));
        var export = await fixture.Service.ExportAsync(new ExportReportCommand(fixture.TenantId, definition.Value.Id, execution.Value.Id, ReportFormat.Excel, fixture.UserId, fixture.ExecuteClaims));
        var schedule = await fixture.Service.ScheduleAsync(new ScheduleReportCommand(fixture.TenantId, definition.Value.Id, ReportScheduleFrequency.Monthly, fixture.Clock.UtcNow.AddDays(1), fixture.UserId, fixture.ExecuteClaims));
        var subscription = await fixture.Service.SubscribeAsync(new SubscribeReportCommand(fixture.TenantId, definition.Value.Id, "qa@example.com", ReportFormat.Pdf, fixture.UserId));
        var binding = await fixture.Service.BindDashboardAsync(new BindReportDashboardCommand(fixture.TenantId, definition.Value.Id, "dashboard.documents", "documents.active", fixture.UserId));
        var datasets = await fixture.Service.GetDashboardDatasetsAsync(fixture.TenantId);

        Assert.True(template.IsSuccess);
        Assert.True(parameter.Value!.IsRequired);
        Assert.True(permission.Value!.CanExport);
        Assert.True(activate.IsSuccess);
        Assert.Equal(ReportExecutionStatus.Running, execution.Value!.Status);
        Assert.Equal(25, output.Value!.RowCount);
        Assert.Equal(ReportFormat.Excel, export.Value!.Format);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", export.Value.ContentType);
        Assert.True(schedule.Value!.IsActive);
        Assert.Equal("qa@example.com", subscription.Value!.Recipient);
        Assert.Equal("dashboard.documents", binding.Value!.DashboardKey);
        Assert.Single(datasets.Value!.Datasets);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.ReportCreated);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.ReportExecuted);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.ReportExported);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.ReportScheduled);
    }

    [Fact]
    public async Task Reporting_Standard_Reports_Seed_All_Required_Enterprise_Reports()
    {
        var fixture = ReportingFixture.Create();
        var catalog = await fixture.Service.GetStandardReportsAsync();
        var seed = await fixture.Service.SeedStandardReportsAsync(new SeedStandardReportsCommand(fixture.TenantId, fixture.UserId));
        var seedAgain = await fixture.Service.SeedStandardReportsAsync(new SeedStandardReportsCommand(fixture.TenantId, fixture.UserId));
        var search = await fixture.Service.SearchAsync(new ReportSearchQuery(fixture.TenantId, null, ReportModule.QualityIndicators, ReportDefinitionStatus.Active, 1, 50));
        var datasets = await fixture.Service.GetDashboardDatasetsAsync(fixture.TenantId);

        Assert.Equal(24, catalog.Value!.Count);
        Assert.Equal(24, seed.Value!.CreatedDefinitions);
        Assert.True(seed.Value.CreatedCategories >= 6);
        Assert.Equal(0, seedAgain.Value!.CreatedDefinitions);
        Assert.Equal(4, search.Value!.TotalCount);
        Assert.Equal(24, datasets.Value!.Datasets.Count);
        Assert.Contains(catalog.Value, report => report.Code == "DOC-EXPIRED");
        Assert.Contains(catalog.Value, report => report.Code == "SUP-SUSPENDED");
        Assert.Contains(catalog.Value, report => report.Code == "AUD-FINDINGS");
        Assert.Contains(catalog.Value, report => report.Code == "CAPA-EFFECTIVENESS");
        Assert.Contains(catalog.Value, report => report.Code == "RISK-MAP");
        Assert.Contains(catalog.Value, report => report.Code == "KPI-DEVIATIONS");
    }

    [Fact]
    public async Task Reporting_Service_Rejects_Duplicates_Invalid_States_And_Permission_Denials()
    {
        var fixture = ReportingFixture.Create();
        var category = await fixture.Service.CreateCategoryAsync(new CreateReportCategoryCommand(fixture.TenantId, "Risk", "RISK", ReportModule.RiskManagement, fixture.UserId));
        var duplicateCategory = await fixture.Service.CreateCategoryAsync(new CreateReportCategoryCommand(fixture.TenantId, "Risk 2", "RISK", ReportModule.RiskManagement, fixture.UserId));
        var missingCategory = await fixture.Service.CreateDefinitionAsync(new CreateReportDefinitionCommand(fixture.TenantId, Guid.NewGuid(), "Bad", "BAD", "Bad", ReportModule.RiskManagement, "bad", fixture.UserId));
        var definition = await fixture.Service.CreateDefinitionAsync(new CreateReportDefinitionCommand(fixture.TenantId, category.Value!.Id, "Riesgos criticos", "RISK-CRITICAL", "Critical risks", ReportModule.RiskManagement, "risks.critical", fixture.UserId));
        var duplicateDefinition = await fixture.Service.CreateDefinitionAsync(new CreateReportDefinitionCommand(fixture.TenantId, category.Value.Id, "Riesgos criticos 2", "RISK-CRITICAL", "Critical risks", ReportModule.RiskManagement, "risks.critical", fixture.UserId));
        var activateWithoutTemplate = await fixture.Service.ActivateAsync(new ReportActionCommand(fixture.TenantId, definition.Value!.Id, fixture.UserId, fixture.ExecuteClaims));
        await fixture.Service.AddTemplateAsync(new AddReportTemplateCommand(fixture.TenantId, definition.Value.Id, "CSV", ReportFormat.Csv, "template", fixture.UserId));
        var duplicateParameter = await fixture.Service.AddParameterAsync(new AddReportParameterCommand(fixture.TenantId, definition.Value.Id, "area", "Area", ReportParameterType.Text, false, null, fixture.UserId));
        duplicateParameter = await fixture.Service.AddParameterAsync(new AddReportParameterCommand(fixture.TenantId, definition.Value.Id, "area", "Area", ReportParameterType.Text, false, null, fixture.UserId));
        await fixture.Service.GrantPermissionAsync(new GrantReportPermissionCommand(fixture.TenantId, definition.Value.Id, ReportPermissionScope.Permission, "REPORT.EXECUTE", true, false, true, fixture.UserId));
        await fixture.Service.ActivateAsync(new ReportActionCommand(fixture.TenantId, definition.Value.Id, fixture.UserId, fixture.ExecuteClaims));
        var executeDenied = await fixture.Service.ExecuteAsync(new ExecuteReportCommand(fixture.TenantId, definition.Value.Id, "{}", fixture.UserId, []));
        var execution = await fixture.Service.ExecuteAsync(new ExecuteReportCommand(fixture.TenantId, definition.Value.Id, "{}", fixture.UserId, fixture.ExecuteClaims));
        var exportBeforeComplete = await fixture.Service.ExportAsync(new ExportReportCommand(fixture.TenantId, definition.Value.Id, execution.Value!.Id, ReportFormat.Pdf, fixture.UserId, fixture.ExecuteClaims));
        var invalidSchedule = await fixture.Service.ScheduleAsync(new ScheduleReportCommand(fixture.TenantId, definition.Value.Id, ReportScheduleFrequency.Daily, fixture.Clock.UtcNow.AddMinutes(-1), fixture.UserId, fixture.ExecuteClaims));
        var exportDenied = await fixture.Service.ExportAsync(new ExportReportCommand(fixture.TenantId, definition.Value.Id, execution.Value.Id, ReportFormat.Pdf, fixture.UserId, ["REPORT.EXECUTE"]));

        Assert.True(duplicateCategory.IsFailure);
        Assert.True(missingCategory.IsFailure);
        Assert.True(duplicateDefinition.IsFailure);
        Assert.True(activateWithoutTemplate.IsFailure);
        Assert.True(duplicateParameter.IsFailure);
        Assert.True(executeDenied.IsFailure);
        Assert.True(exportBeforeComplete.IsFailure);
        Assert.True(invalidSchedule.IsFailure);
        Assert.True(exportDenied.IsFailure);
    }

    [Fact]
    public async Task Reporting_Search_And_Datasets_Are_Tenant_Isolated()
    {
        var fixture = ReportingFixture.Create();
        var otherTenant = Guid.NewGuid();
        var category = await fixture.Service.CreateCategoryAsync(new CreateReportCategoryCommand(fixture.TenantId, "CAPA", "CAPA", ReportModule.Capa, fixture.UserId));
        var otherCategory = await fixture.Service.CreateCategoryAsync(new CreateReportCategoryCommand(otherTenant, "CAPA", "CAPA", ReportModule.Capa, fixture.UserId));
        var definition = await fixture.Service.CreateDefinitionAsync(new CreateReportDefinitionCommand(fixture.TenantId, category.Value!.Id, "CAPAs abiertas", "CAPA-OPEN", "Open CAPAs", ReportModule.Capa, "capas.open", fixture.UserId));
        var otherDefinition = await fixture.Service.CreateDefinitionAsync(new CreateReportDefinitionCommand(otherTenant, otherCategory.Value!.Id, "CAPAs abiertas", "CAPA-OPEN", "Open CAPAs", ReportModule.Capa, "capas.open", fixture.UserId));
        await fixture.Service.AddTemplateAsync(new AddReportTemplateCommand(fixture.TenantId, definition.Value!.Id, "JSON", ReportFormat.Json, "{}", fixture.UserId));
        await fixture.Service.AddTemplateAsync(new AddReportTemplateCommand(otherTenant, otherDefinition.Value!.Id, "JSON", ReportFormat.Json, "{}", fixture.UserId));
        await fixture.Service.ActivateAsync(new ReportActionCommand(fixture.TenantId, definition.Value.Id, fixture.UserId, fixture.ExecuteClaims));
        await fixture.Service.ActivateAsync(new ReportActionCommand(otherTenant, otherDefinition.Value.Id, fixture.UserId, fixture.ExecuteClaims));
        await fixture.Service.BindDashboardAsync(new BindReportDashboardCommand(fixture.TenantId, definition.Value.Id, "dashboard.capa", "capas.open", fixture.UserId));
        await fixture.Service.BindDashboardAsync(new BindReportDashboardCommand(otherTenant, otherDefinition.Value.Id, "dashboard.capa", "capas.open", fixture.UserId));

        var search = await fixture.Service.SearchAsync(new ReportSearchQuery(fixture.TenantId, "CAPA", ReportModule.Capa, ReportDefinitionStatus.Active, 0, 0));
        var datasets = await fixture.Service.GetDashboardDatasetsAsync(fixture.TenantId);
        var otherDatasets = await fixture.Service.GetDashboardDatasetsAsync(otherTenant);

        Assert.Single(search.Value!.Items);
        Assert.Equal(1, search.Value.Page);
        Assert.Single(datasets.Value!.Datasets);
        Assert.Single(otherDatasets.Value!.Datasets);
        Assert.NotEqual(datasets.Value.Datasets.Single().ReportDefinitionId, otherDatasets.Value.Datasets.Single().ReportDefinitionId);
    }

    [Fact]
    public void Reporting_Domain_Covers_Formats_Permissions_And_Execution_Failure()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var definition = new ReportDefinition(tenantId, Guid.NewGuid(), "Indicators", "KPI", "KPI report", ReportModule.QualityIndicators, "indicators.kpis", userId, now);
        definition.AddTemplate("Word", ReportFormat.Word, "word-template", userId, now);
        definition.Activate(userId, now);
        var rolePermission = definition.GrantPermission(ReportPermissionScope.Role, "QualityManager", true, true, false, userId, now);
        var userPermission = definition.GrantPermission(ReportPermissionScope.User, userId.ToString(), true, false, false, userId, now);
        var execution = definition.StartExecution("{}", userId, now);
        execution.MarkFailed("dataset unavailable", now.AddMinutes(1));

        Assert.True(rolePermission.Matches(["ROLE:QualityManager"], Guid.NewGuid()));
        Assert.True(userPermission.Matches([], userId));
        Assert.True(definition.CanExecute(["ROLE:QualityManager"], Guid.NewGuid()));
        Assert.False(definition.CanExport([], Guid.NewGuid()));
        Assert.Equal("application/pdf", ReportDefinition.ContentTypeFor(ReportFormat.Pdf));
        Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ReportDefinition.ContentTypeFor(ReportFormat.Word));
        Assert.Equal("application/json", ReportDefinition.ContentTypeFor(ReportFormat.Json));
        Assert.Equal(ReportExecutionStatus.Failed, execution.Status);
        Assert.Equal("dataset unavailable", execution.FailureReason);
    }

    [Fact]
    public void Reporting_Domain_Covers_Inactive_Required_Missing_And_Default_Branches()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var open = new ReportDefinition(tenantId, Guid.NewGuid(), "Open", "OPEN", "Open report", ReportModule.AuditLog, "audit.logs", userId, now);
        Assert.True(open.CanExecute([], userId));
        Assert.True(open.CanExport([], userId));
        Assert.Throws<DomainException>(() => open.Subscribe("audit@example.com", ReportFormat.Csv, userId, now));
        open.AddTemplate("CSV", ReportFormat.Csv, "csv-template", userId, now);
        open.AddParameter("tenantId", "Tenant", ReportParameterType.Guid, true, null, userId, now);
        open.Activate(userId, now);
        Assert.Throws<DomainException>(() => open.StartExecution("", userId, now));
        var execution = open.StartExecution("{\"tenantId\":\"value\"}", userId, now);
        open.CompleteExecution(execution.Id, 1, "{\"dataset\":\"audit.logs\"}", userId, now);
        var csvExport = open.Export(execution.Id, ReportFormat.Csv, userId, now);
        Assert.EndsWith(".csv", csvExport.FileName);
        Assert.Equal("text/csv", csvExport.ContentType);
        Assert.Throws<DomainException>(() => open.CompleteExecution(Guid.NewGuid(), 1, "{}", userId, now));

        var queued = new ReportExecution(tenantId, open.Id, userId, "", now);
        Assert.Equal("{}", queued.ParametersJson);
        Assert.Throws<DomainException>(() => queued.MarkCompleted(1, now));
    }

    [Fact]
    public async Task Reporting_Ef_Repository_Persists_Aggregate_And_Queries()
    {
        await using var dbContext = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var repository = new EfReportingEngineRepository(dbContext);
        var category = new ReportCategory(tenantId, "Audit", "AUD", ReportModule.AuditManagement, userId);
        await repository.AddCategoryAsync(category);
        var definition = new ReportDefinition(tenantId, category.Id, "Hallazgos", "AUD-FINDINGS", "Audit findings", ReportModule.AuditManagement, "audits.findings", userId, now);
        definition.AddTemplate("PDF", ReportFormat.Pdf, "template", userId, now);
        definition.AddParameter("from", "From", ReportParameterType.Date, false, null, userId, now);
        definition.Activate(userId, now);
        definition.BindDashboard("dashboard.audit", "audits.findings", userId, now);
        var execution = definition.StartExecution("{}", userId, now);
        definition.CompleteExecution(execution.Id, 10, "{\"dataset\":\"audits.findings\"}", userId, now);
        definition.Export(execution.Id, ReportFormat.Csv, userId, now);
        await repository.AddDefinitionAsync(definition);
        await repository.AddAuditLogAsync(AuditLog.FromEvent(new AuditEvent(nameof(ReportDefinition), definition.Id, AuditAction.ReportCreated, AuditCategory.ReportingEngine, new AuditContext(tenantId, userId, null, null, null, null, null, null, null), new AuditSnapshot(null, null), new AuditMetadata("{}"), true, null), now));
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetDefinitionAsync(tenantId, definition.Id);
        var exists = await repository.DefinitionCodeExistsAsync(tenantId, "AUD-FINDINGS");
        var search = await repository.SearchAsync(new ReportSearchCriteria(tenantId, "Hallazgos", ReportModule.AuditManagement, ReportDefinitionStatus.Active, 1, 10));
        var datasets = await repository.GetDashboardDatasetsAsync(tenantId);

        Assert.NotNull(loaded);
        Assert.True(exists);
        Assert.Single(search.Items);
        Assert.Single(datasets.Datasets);
        Assert.Single(dbContext.AuditLogs);
    }

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class ReportingFixture
    {
        private ReportingFixture()
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryReportingEngineRepository();
            Service = new ReportingEngineService(Repository, new FakeApplicationDbContext(), Clock);
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public FixedClock Clock { get; }
        public IReadOnlyCollection<string> ExecuteClaims { get; } = ["REPORT.EXECUTE", "REPORT.EXPORT", "REPORT.SCHEDULE"];
        public InMemoryReportingEngineRepository Repository { get; }
        public ReportingEngineService Service { get; }
        public static ReportingFixture Create() => new();
    }

    private sealed class InMemoryReportingEngineRepository : IReportingEngineRepository
    {
        public List<ReportCategory> Categories { get; } = [];
        public List<ReportDefinition> Definitions { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];
        public Task AddCategoryAsync(ReportCategory category, CancellationToken cancellationToken = default) { Categories.Add(category); return Task.CompletedTask; }
        public Task<ReportCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default) => Task.FromResult(Categories.SingleOrDefault(category => category.TenantId == tenantId && category.Id == categoryId));
        public Task<ReportCategory?> GetCategoryByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => Task.FromResult(Categories.SingleOrDefault(category => category.TenantId == tenantId && category.Code == code.ToUpperInvariant()));
        public Task<bool> CategoryCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => Task.FromResult(Categories.Any(category => category.TenantId == tenantId && category.Code == code.ToUpperInvariant()));
        public Task AddDefinitionAsync(ReportDefinition definition, CancellationToken cancellationToken = default) { Definitions.Add(definition); return Task.CompletedTask; }
        public Task<ReportDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken = default) => Task.FromResult(Definitions.SingleOrDefault(definition => definition.TenantId == tenantId && definition.Id == definitionId));
        public Task<bool> DefinitionCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => Task.FromResult(Definitions.Any(definition => definition.TenantId == tenantId && definition.Code == code.ToUpperInvariant()));
        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default) { AuditLogs.Add(auditLog); return Task.CompletedTask; }
        public Task NormalizeNewReportChildStatesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<ReportSearchResult> SearchAsync(ReportSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var items = Definitions.Where(definition => definition.TenantId == criteria.TenantId)
                .Where(definition => criteria.SearchText is null || definition.Name.Contains(criteria.SearchText) || definition.Code.Contains(criteria.SearchText))
                .Where(definition => !criteria.Module.HasValue || definition.Module == criteria.Module.Value)
                .Where(definition => !criteria.Status.HasValue || definition.Status == criteria.Status.Value)
                .Select(definition => new ReportDefinitionSummary(definition.Id, definition.TenantId, definition.Name, definition.Code, definition.Module, definition.DatasetKey, definition.Version, definition.Status))
                .ToArray();
            return Task.FromResult(new ReportSearchResult(items.Skip((criteria.Page - 1) * criteria.PageSize).Take(criteria.PageSize).ToArray(), items.Length, criteria.Page, criteria.PageSize));
        }

        public Task<ReportingDashboardDatasetCatalog> GetDashboardDatasetsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            var datasets = Definitions.Where(definition => definition.TenantId == tenantId && definition.Status == ReportDefinitionStatus.Active)
                .SelectMany(definition => definition.DashboardBindings.Select(binding => new ReportDashboardDatasetDescriptor(definition.Id, definition.Code, definition.Module, binding.DatasetKey, binding.DashboardKey)))
                .ToArray();
            return Task.FromResult(new ReportingDashboardDatasetCatalog(datasets));
        }
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext { public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1); }
    private sealed class FixedClock : IClock { public DateTimeOffset UtcNow { get; } = new(2026, 6, 20, 12, 0, 0, TimeSpan.Zero); }
}
