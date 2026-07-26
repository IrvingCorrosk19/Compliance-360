using Compliance360.Application;
using Compliance360.Application.Reporting;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Reporting;

namespace Compliance360.Tests;

/// <summary>
/// Dual-tenant isolation gates for reporting export content (REM-MT-DUAL-BUSINESS).
/// Shared in-memory store proves tenant filters prevent cross-tenant export download.
/// </summary>
public sealed class DualTenantIsolationTests
{
    [Fact]
    public async Task Report_Export_Content_Is_Denied_For_Foreign_Tenant()
    {
        var shared = new SharedReportingStore();
        var alpha = DualTenantReportingFixture.Create(shared, Guid.NewGuid());
        var beta = DualTenantReportingFixture.Create(shared, Guid.NewGuid());

        var category = await alpha.Service.CreateCategoryAsync(
            new CreateReportCategoryCommand(alpha.TenantId, "Documents", "DOC", ReportModule.DocumentManagement, alpha.UserId));
        var definition = await alpha.Service.CreateDefinitionAsync(
            new CreateReportDefinitionCommand(
                alpha.TenantId,
                category.Value!.Id,
                "Active documents",
                "DOC-ACTIVE",
                "Active documents",
                ReportModule.DocumentManagement,
                "documents.active",
                alpha.UserId));
        await alpha.Service.AddTemplateAsync(
            new AddReportTemplateCommand(alpha.TenantId, definition.Value!.Id, "XLSX", ReportFormat.Excel, "template", alpha.UserId));
        await alpha.Service.GrantPermissionAsync(
            new GrantReportPermissionCommand(
                alpha.TenantId, definition.Value.Id, ReportPermissionScope.Permission, "REPORT.EXECUTE", true, true, true, alpha.UserId));
        await alpha.Service.ActivateAsync(new ReportActionCommand(alpha.TenantId, definition.Value.Id, alpha.UserId, alpha.ExecuteClaims));
        var execution = await alpha.Service.ExecuteAsync(
            new ExecuteReportCommand(alpha.TenantId, definition.Value.Id, "{}", alpha.UserId, alpha.ExecuteClaims));
        await alpha.Service.CompleteExecutionAsync(
            new CompleteReportExecutionCommand(
                alpha.TenantId, definition.Value.Id, execution.Value!.Id, 2, """{"marker":"ALPHA-ONLY"}""", alpha.UserId));
        var export = await alpha.Service.ExportAsync(
            new ExportReportCommand(
                alpha.TenantId, definition.Value.Id, execution.Value.Id, ReportFormat.Excel, alpha.UserId, alpha.ExecuteClaims));

        var alphaContent = await alpha.Service.GetExportContentAsync(
            alpha.TenantId, definition.Value.Id, export.Value!.Id, alpha.ExecuteClaims, alpha.UserId);
        Assert.True(alphaContent.IsSuccess);
        Assert.Equal((byte)'P', alphaContent.Value!.Content[0]);
        Assert.Equal((byte)'K', alphaContent.Value.Content[1]);
        using (var zip = new System.IO.Compression.ZipArchive(new MemoryStream(alphaContent.Value.Content), System.IO.Compression.ZipArchiveMode.Read))
        {
            var sst = zip.GetEntry("xl/sharedStrings.xml");
            Assert.NotNull(sst);
            using var reader = new StreamReader(sst!.Open());
            var xml = await reader.ReadToEndAsync();
            Assert.Contains("ALPHA-ONLY", xml, StringComparison.Ordinal);
            Assert.Contains(alpha.TenantId.ToString("D"), xml, StringComparison.Ordinal);
        }

        var crossByTenant = await beta.Service.GetExportContentAsync(
            beta.TenantId, definition.Value.Id, export.Value.Id, beta.ExecuteClaims, beta.UserId);
        Assert.True(crossByTenant.IsFailure);

        var searchBeta = await beta.Service.SearchAsync(
            new ReportSearchQuery(beta.TenantId, "ALPHA-ONLY", null, null, 1, 20));
        Assert.True(searchBeta.IsSuccess);
        Assert.Empty(searchBeta.Value!.Items);

        var searchAlpha = await alpha.Service.SearchAsync(
            new ReportSearchQuery(alpha.TenantId, "Active", null, null, 1, 20));
        Assert.True(searchAlpha.IsSuccess);
        Assert.Single(searchAlpha.Value!.Items);
    }

    private sealed class DualTenantReportingFixture
    {
        private DualTenantReportingFixture(SharedReportingStore store, Guid tenantId)
        {
            TenantId = tenantId;
            UserId = Guid.NewGuid();
            Clock = new DualClock();
            Service = new ReportingEngineService(store, new DualDb(), Clock);
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public DualClock Clock { get; }
        public IReadOnlyCollection<string> ExecuteClaims { get; } = ["REPORT.EXECUTE", "REPORT.EXPORT", "REPORT.SCHEDULE"];
        public ReportingEngineService Service { get; }
        public static DualTenantReportingFixture Create(SharedReportingStore store, Guid tenantId) => new(store, tenantId);
    }

    private sealed class SharedReportingStore : IReportingEngineRepository
    {
        public List<ReportCategory> Categories { get; } = [];
        public List<ReportDefinition> Definitions { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];
        public Task AddCategoryAsync(ReportCategory category, CancellationToken cancellationToken = default) { Categories.Add(category); return Task.CompletedTask; }
        public Task<ReportCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Categories.SingleOrDefault(c => c.TenantId == tenantId && c.Id == categoryId));
        public Task<ReportCategory?> GetCategoryByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(Categories.SingleOrDefault(c => c.TenantId == tenantId && c.Code == code.ToUpperInvariant()));
        public Task<bool> CategoryCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(Categories.Any(c => c.TenantId == tenantId && c.Code == code.ToUpperInvariant()));
        public Task AddDefinitionAsync(ReportDefinition definition, CancellationToken cancellationToken = default) { Definitions.Add(definition); return Task.CompletedTask; }
        public Task<ReportDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Definitions.SingleOrDefault(d => d.TenantId == tenantId && d.Id == definitionId));
        public Task<bool> DefinitionCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(Definitions.Any(d => d.TenantId == tenantId && d.Code == code.ToUpperInvariant()));
        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default) { AuditLogs.Add(auditLog); return Task.CompletedTask; }
        public Task NormalizeNewReportChildStatesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<ReportSearchResult> SearchAsync(ReportSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var items = Definitions.Where(d => d.TenantId == criteria.TenantId)
                .Where(d => criteria.SearchText is null || d.Name.Contains(criteria.SearchText) || d.Code.Contains(criteria.SearchText))
                .Where(d => !criteria.Module.HasValue || d.Module == criteria.Module.Value)
                .Where(d => !criteria.Status.HasValue || d.Status == criteria.Status.Value)
                .Select(d => new ReportDefinitionSummary(d.Id, d.TenantId, d.Name, d.Code, d.Module, d.DatasetKey, d.Version, d.Status))
                .ToArray();
            return Task.FromResult(new ReportSearchResult(items.Skip((criteria.Page - 1) * criteria.PageSize).Take(criteria.PageSize).ToArray(), items.Length, criteria.Page, criteria.PageSize));
        }

        public Task<ReportingDashboardDatasetCatalog> GetDashboardDatasetsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            var datasets = Definitions.Where(d => d.TenantId == tenantId && d.Status == ReportDefinitionStatus.Active)
                .SelectMany(d => d.DashboardBindings.Select(b => new ReportDashboardDatasetDescriptor(d.Id, d.Code, d.Module, b.DatasetKey, b.DashboardKey)))
                .ToArray();
            return Task.FromResult(new ReportingDashboardDatasetCatalog(datasets));
        }
    }

    private sealed class DualDb : IApplicationDbContext
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class DualClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 7, 26, 12, 0, 0, TimeSpan.Zero);
    }
}
