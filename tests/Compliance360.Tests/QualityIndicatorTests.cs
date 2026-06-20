using Compliance360.Application;
using Compliance360.Application.QualityIndicators;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.QualityIndicators;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.QualityIndicators;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class QualityIndicatorTests
{
    private const string Hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    [Fact]
    public async Task Indicator_Full_Flow_Calculates_Trends_Dashboard_And_Audit()
    {
        var fixture = QualityFixture.Create();
        var category = await fixture.Service.CreateCategoryAsync(new CreateIndicatorCategoryCommand(fixture.TenantId, "ISO 9001", "ISO", fixture.UserId));
        var indicator = await fixture.Service.CreateIndicatorAsync(new CreateQualityIndicatorCommand(fixture.TenantId, category.Value!.Id, "Supplier on-time delivery", "SUP-OTD", "Supplier operational quality KPI", IndicatorType.Supplier, IndicatorFrequency.Monthly, IndicatorCalculationType.Percentage, "%", fixture.SupplierId, fixture.AuditId, fixture.CapaId, fixture.RiskId, fixture.DocumentId, fixture.UserId));
        var formula = await fixture.Service.DefineFormulaAsync(new DefineIndicatorFormulaCommand(fixture.TenantId, indicator.Value!.Id, "(delivered_on_time / total_deliveries) * 100", IndicatorCalculationType.Percentage, fixture.UserId));
        var target = await fixture.Service.DefineTargetAsync(new DefineIndicatorTargetCommand(fixture.TenantId, indicator.Value.Id, 95, fixture.Clock.UtcNow, fixture.UserId));
        var threshold = await fixture.Service.DefineThresholdAsync(new DefineIndicatorThresholdCommand(fixture.TenantId, indicator.Value.Id, 90, 80, 98, fixture.UserId));
        var process = await fixture.Service.AssociateProcessAsync(new AssociateIndicatorProcessCommand(fixture.TenantId, indicator.Value.Id, "Supplier qualification", "Quality", fixture.UserId));
        var period1 = await fixture.Service.AddPeriodAsync(new AddIndicatorPeriodCommand(fixture.TenantId, indicator.Value.Id, 2026, 1, fixture.Clock.UtcNow, fixture.Clock.UtcNow.AddMonths(1), fixture.UserId));
        var measurement1 = await fixture.Service.CaptureMeasurementAsync(new CaptureIndicatorMeasurementCommand(fixture.TenantId, indicator.Value.Id, period1.Value!.Id, 97, 100, false, fixture.UserId));
        var result1 = await fixture.Service.CalculateResultAsync(new CalculateIndicatorResultCommand(fixture.TenantId, indicator.Value.Id, period1.Value.Id, measurement1.Value!.Id, fixture.UserId));
        var period2 = await fixture.Service.AddPeriodAsync(new AddIndicatorPeriodCommand(fixture.TenantId, indicator.Value.Id, 2026, 2, fixture.Clock.UtcNow.AddMonths(1), fixture.Clock.UtcNow.AddMonths(2), fixture.UserId));
        var measurement2 = await fixture.Service.CaptureMeasurementAsync(new CaptureIndicatorMeasurementCommand(fixture.TenantId, indicator.Value.Id, period2.Value!.Id, 79, 100, true, fixture.UserId));
        var result2 = await fixture.Service.CalculateResultAsync(new CalculateIndicatorResultCommand(fixture.TenantId, indicator.Value.Id, period2.Value.Id, measurement2.Value!.Id, fixture.UserId));
        var attachment = await fixture.Service.AddAttachmentAsync(new AddIndicatorAttachmentCommand(fixture.TenantId, indicator.Value.Id, Guid.NewGuid(), "kpi.pdf", "application/pdf", 512, Hash, fixture.UserId));
        var workflow = await fixture.Service.AttachWorkflowAsync(new AttachIndicatorWorkflowCommand(fixture.TenantId, indicator.Value.Id, Guid.NewGuid(), fixture.UserId));
        var activate = await fixture.Service.ActivateAsync(new IndicatorActionCommand(fixture.TenantId, indicator.Value.Id, fixture.UserId));
        var approve = await fixture.Service.ApproveAsync(new IndicatorActionCommand(fixture.TenantId, indicator.Value.Id, fixture.UserId));
        var dashboard = await fixture.Service.GetDashboardAsync(fixture.TenantId);
        var trends = await fixture.Service.GetTrendsAsync(fixture.TenantId, indicator.Value.Id);
        var export = await fixture.Service.ExportAsync(new IndicatorExportQuery(fixture.TenantId, IndicatorStatus.Approved, IndicatorType.Supplier, "xlsx", fixture.UserId));

        Assert.True(formula.IsSuccess);
        Assert.Equal(95, target.Value!.TargetValue);
        Assert.Equal(80, threshold.Value!.CriticalMinimum);
        Assert.Equal("Quality", process.Value!.Area);
        Assert.Equal(97, result1.Value!.Value);
        Assert.Equal(IndicatorResultStatus.AboveTarget, result1.Value.Status);
        Assert.Equal(IndicatorResultStatus.CriticalDeviation, result2.Value!.Status);
        Assert.True(attachment.IsSuccess);
        Assert.True(workflow.IsSuccess);
        Assert.True(activate.IsSuccess);
        Assert.True(approve.IsSuccess);
        Assert.Equal(1, dashboard.Value!.TotalIndicators);
        Assert.Equal(1, dashboard.Value.CriticalIndicators);
        Assert.Contains(trends.Value!, trend => trend.Direction == IndicatorTrendDirection.Negative);
        Assert.Equal("xlsx", export.Value!.Format);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.IndicatorCreated);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.IndicatorApproved);
        Assert.Contains(fixture.Repository.AuditLogs, log => log.Action == AuditAction.IndicatorExported);
    }

    [Fact]
    public async Task Indicator_Service_Rejects_Duplicates_Missing_And_Invalid_Rules()
    {
        var fixture = QualityFixture.Create();
        var category = await fixture.Service.CreateCategoryAsync(new CreateIndicatorCategoryCommand(fixture.TenantId, "BPM", "BPM", fixture.UserId));
        var duplicateCategory = await fixture.Service.CreateCategoryAsync(new CreateIndicatorCategoryCommand(fixture.TenantId, "BPM 2", "BPM", fixture.UserId));
        var missingCategory = await fixture.Service.CreateIndicatorAsync(new CreateQualityIndicatorCommand(fixture.TenantId, Guid.NewGuid(), "Bad", "BAD", "Bad indicator", IndicatorType.Bpm, IndicatorFrequency.Monthly, IndicatorCalculationType.Average, "u", null, null, null, null, null, fixture.UserId));
        var indicator = await fixture.Service.CreateIndicatorAsync(new CreateQualityIndicatorCommand(fixture.TenantId, category.Value!.Id, "HACCP incidents", "HACCP-INC", "HACCP deviations", IndicatorType.Haccp, IndicatorFrequency.Quarterly, IndicatorCalculationType.Ratio, "ratio", null, null, null, null, null, fixture.UserId));
        var duplicateIndicator = await fixture.Service.CreateIndicatorAsync(new CreateQualityIndicatorCommand(fixture.TenantId, category.Value.Id, "HACCP incidents 2", "HACCP-INC", "HACCP deviations", IndicatorType.Haccp, IndicatorFrequency.Quarterly, IndicatorCalculationType.Ratio, "ratio", null, null, null, null, null, fixture.UserId));
        var invalidThreshold = await fixture.Service.DefineThresholdAsync(new DefineIndicatorThresholdCommand(fixture.TenantId, indicator.Value!.Id, 70, 90, 95, fixture.UserId));
        var invalidPeriod = await fixture.Service.AddPeriodAsync(new AddIndicatorPeriodCommand(fixture.TenantId, indicator.Value.Id, 2026, 1, fixture.Clock.UtcNow, fixture.Clock.UtcNow.AddDays(-1), fixture.UserId));
        var missingMeasurement = await fixture.Service.CaptureMeasurementAsync(new CaptureIndicatorMeasurementCommand(fixture.TenantId, indicator.Value.Id, Guid.NewGuid(), 1, 2, false, fixture.UserId));
        var missingTarget = await fixture.Service.CalculateResultAsync(new CalculateIndicatorResultCommand(fixture.TenantId, indicator.Value.Id, Guid.NewGuid(), Guid.NewGuid(), fixture.UserId));
        var invalidAttachment = await fixture.Service.AddAttachmentAsync(new AddIndicatorAttachmentCommand(fixture.TenantId, indicator.Value.Id, Guid.NewGuid(), "bad.pdf", "application/pdf", 0, Hash, fixture.UserId));
        var invalidApprove = await fixture.Service.ApproveAsync(new IndicatorActionCommand(fixture.TenantId, indicator.Value.Id, fixture.UserId));

        Assert.True(duplicateCategory.IsFailure);
        Assert.True(missingCategory.IsFailure);
        Assert.True(duplicateIndicator.IsFailure);
        Assert.True(invalidThreshold.IsFailure);
        Assert.True(invalidPeriod.IsFailure);
        Assert.True(missingMeasurement.IsFailure);
        Assert.True(missingTarget.IsFailure);
        Assert.True(invalidAttachment.IsFailure);
        Assert.True(invalidApprove.IsFailure);
    }

    [Fact]
    public async Task Indicator_Search_Dashboard_Trends_And_Export_Are_Tenant_Isolated()
    {
        var fixture = QualityFixture.Create();
        var otherTenant = Guid.NewGuid();
        var category = await fixture.Service.CreateCategoryAsync(new CreateIndicatorCategoryCommand(fixture.TenantId, "Risk KPIs", "RISK", fixture.UserId));
        var otherCategory = await fixture.Service.CreateCategoryAsync(new CreateIndicatorCategoryCommand(otherTenant, "Risk KPIs", "RISK", fixture.UserId));
        var indicator = await fixture.Service.CreateIndicatorAsync(new CreateQualityIndicatorCommand(fixture.TenantId, category.Value!.Id, "Risk closure rate", "RISK-CLOSE", "Risk KPI", IndicatorType.Risk, IndicatorFrequency.Annual, IndicatorCalculationType.Accumulated, "count", null, null, null, fixture.RiskId, null, fixture.UserId));
        await fixture.Service.CreateIndicatorAsync(new CreateQualityIndicatorCommand(otherTenant, otherCategory.Value!.Id, "Other risk closure rate", "RISK-CLOSE", "Risk KPI", IndicatorType.Risk, IndicatorFrequency.Annual, IndicatorCalculationType.Accumulated, "count", null, null, null, fixture.RiskId, null, fixture.UserId));
        await fixture.Service.ActivateAsync(new IndicatorActionCommand(fixture.TenantId, indicator.Value!.Id, fixture.UserId));
        await fixture.Service.ApproveAsync(new IndicatorActionCommand(fixture.TenantId, indicator.Value.Id, fixture.UserId));

        var search = await fixture.Service.SearchAsync(new IndicatorSearchQuery(fixture.TenantId, "Risk", IndicatorStatus.Approved, IndicatorType.Risk, IndicatorFrequency.Annual, null, null, null, fixture.RiskId, 0, 0));
        var otherSearch = await fixture.Service.SearchAsync(new IndicatorSearchQuery(otherTenant, "Other", null, null, null, null, null, null, null, 1, 10));
        var dashboard = await fixture.Service.GetDashboardAsync(fixture.TenantId);
        var export = await fixture.Service.ExportAsync(new IndicatorExportQuery(fixture.TenantId, null, null, "json", fixture.UserId));

        Assert.Single(search.Value!.Items);
        Assert.Equal(1, search.Value.TotalCount);
        Assert.Equal(1, search.Value.Page);
        Assert.Single(otherSearch.Value!.Items);
        Assert.Equal(1, dashboard.Value!.ApprovedIndicators);
        Assert.Equal("application/json", export.Value!.ContentType);
    }

    [Fact]
    public async Task Indicator_Ef_Repository_Persists_Aggregate_And_Queries()
    {
        await using var dbContext = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var repository = new EfQualityIndicatorRepository(dbContext);
        var category = new IndicatorCategory(tenantId, "Operations", "OPS", userId);
        await repository.AddCategoryAsync(category);
        var indicator = new QualityIndicator(tenantId, category.Id, "Audit closure", "AUD-CLOSE", "Audit KPI", IndicatorType.Audit, IndicatorFrequency.SemiAnnual, IndicatorCalculationType.Percentage, "%", null, Guid.NewGuid(), null, null, null, userId, now);
        indicator.Activate(userId, now);
        indicator.Approve(userId, now);
        indicator.DefineTarget(90, now, userId, now);
        indicator.DefineThreshold(80, 70, 95, userId, now);
        var period = indicator.AddPeriod(2026, 1, now, now.AddMonths(1), userId, now);
        var measurement = indicator.CaptureMeasurement(period.Id, 95, 100, false, userId, now);
        indicator.CalculateResult(period.Id, measurement.Id, userId, now);
        indicator.AssociateProcess("Audits", "Quality", userId, now);
        await repository.AddIndicatorAsync(indicator);
        await repository.AddAuditLogAsync(AuditLog.FromEvent(new AuditEvent(nameof(QualityIndicator), indicator.Id, AuditAction.IndicatorCreated, AuditCategory.QualityIndicators, new AuditContext(tenantId, userId, null, null, null, null, null, null, null), new AuditSnapshot(null, null), new AuditMetadata("{}"), true, null), now));
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetIndicatorAsync(tenantId, indicator.Id);
        var exists = await repository.IndicatorCodeExistsAsync(tenantId, "AUD-CLOSE");
        var search = await repository.SearchAsync(new IndicatorSearchCriteria(tenantId, "Audit", IndicatorStatus.Approved, IndicatorType.Audit, IndicatorFrequency.SemiAnnual, null, indicator.AuditId, null, null, 1, 10));
        var dashboard = await repository.GetDashboardAsync(tenantId);
        var trends = await repository.GetTrendsAsync(tenantId, indicator.Id);

        Assert.NotNull(loaded);
        Assert.True(exists);
        Assert.Single(search.Items);
        Assert.Equal(1, dashboard.ApprovedIndicators);
        Assert.Single(trends);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task Indicator_Branch_Rules_Cover_Calculation_Status_And_Export_Paths()
    {
        var fixture = QualityFixture.Create();
        var category = await fixture.Service.CreateCategoryAsync(new CreateIndicatorCategoryCommand(fixture.TenantId, "Operations", "OPS", fixture.UserId));
        var average = await fixture.Service.CreateIndicatorAsync(new CreateQualityIndicatorCommand(fixture.TenantId, category.Value!.Id, "Average defects", "AVG-DEF", "Average defects", IndicatorType.Operational, IndicatorFrequency.Monthly, IndicatorCalculationType.Average, "avg", null, null, null, null, null, fixture.UserId));
        await fixture.Service.DefineTargetAsync(new DefineIndicatorTargetCommand(fixture.TenantId, average.Value!.Id, 10, fixture.Clock.UtcNow, fixture.UserId));
        var period = await fixture.Service.AddPeriodAsync(new AddIndicatorPeriodCommand(fixture.TenantId, average.Value.Id, 2026, 3, fixture.Clock.UtcNow, fixture.Clock.UtcNow.AddMonths(1), fixture.UserId));
        var averageMeasurement = await fixture.Service.CaptureMeasurementAsync(new CaptureIndicatorMeasurementCommand(fixture.TenantId, average.Value.Id, period.Value!.Id, 20, 2, false, fixture.UserId));
        var averageResult = await fixture.Service.CalculateResultAsync(new CalculateIndicatorResultCommand(fixture.TenantId, average.Value.Id, period.Value.Id, averageMeasurement.Value!.Id, fixture.UserId));
        var averageNoDenominator = await fixture.Service.CreateIndicatorAsync(new CreateQualityIndicatorCommand(fixture.TenantId, category.Value.Id, "Average direct", "AVG-DIR", "Average direct", IndicatorType.Operational, IndicatorFrequency.Monthly, IndicatorCalculationType.Average, "avg", null, null, null, null, null, fixture.UserId));
        await fixture.Service.DefineTargetAsync(new DefineIndicatorTargetCommand(fixture.TenantId, averageNoDenominator.Value!.Id, 15, fixture.Clock.UtcNow, fixture.UserId));
        var directPeriod = await fixture.Service.AddPeriodAsync(new AddIndicatorPeriodCommand(fixture.TenantId, averageNoDenominator.Value.Id, 2026, 4, fixture.Clock.UtcNow, fixture.Clock.UtcNow.AddMonths(1), fixture.UserId));
        var directMeasurement = await fixture.Service.CaptureMeasurementAsync(new CaptureIndicatorMeasurementCommand(fixture.TenantId, averageNoDenominator.Value.Id, directPeriod.Value!.Id, 15, null, false, fixture.UserId));
        var directResult = await fixture.Service.CalculateResultAsync(new CalculateIndicatorResultCommand(fixture.TenantId, averageNoDenominator.Value.Id, directPeriod.Value.Id, directMeasurement.Value!.Id, fixture.UserId));
        var ratio = await fixture.Service.CreateIndicatorAsync(new CreateQualityIndicatorCommand(fixture.TenantId, category.Value.Id, "Ratio defects", "RATIO-DEF", "Ratio defects", IndicatorType.Process, IndicatorFrequency.Quarterly, IndicatorCalculationType.Ratio, "ratio", null, null, null, null, null, fixture.UserId));
        await fixture.Service.DefineTargetAsync(new DefineIndicatorTargetCommand(fixture.TenantId, ratio.Value!.Id, 0.5m, fixture.Clock.UtcNow, fixture.UserId));
        var ratioPeriod = await fixture.Service.AddPeriodAsync(new AddIndicatorPeriodCommand(fixture.TenantId, ratio.Value.Id, 2026, 5, fixture.Clock.UtcNow, fixture.Clock.UtcNow.AddMonths(1), fixture.UserId));
        var ratioMeasurement = await fixture.Service.CaptureMeasurementAsync(new CaptureIndicatorMeasurementCommand(fixture.TenantId, ratio.Value.Id, ratioPeriod.Value!.Id, 1, 0, false, fixture.UserId));
        var ratioResult = await fixture.Service.CalculateResultAsync(new CalculateIndicatorResultCommand(fixture.TenantId, ratio.Value.Id, ratioPeriod.Value.Id, ratioMeasurement.Value!.Id, fixture.UserId));
        var missingDenominatorMeasurement = await fixture.Service.CaptureMeasurementAsync(new CaptureIndicatorMeasurementCommand(fixture.TenantId, ratio.Value.Id, ratioPeriod.Value.Id, 1, null, false, fixture.UserId));
        var missingDenominatorResult = await fixture.Service.CalculateResultAsync(new CalculateIndicatorResultCommand(fixture.TenantId, ratio.Value.Id, ratioPeriod.Value.Id, missingDenominatorMeasurement.Value!.Id, fixture.UserId));
        var accumulated = await fixture.Service.CreateIndicatorAsync(new CreateQualityIndicatorCommand(fixture.TenantId, category.Value.Id, "Accumulated actions", "ACC-ACT", "Accumulated actions", IndicatorType.Capa, IndicatorFrequency.Annual, IndicatorCalculationType.Accumulated, "count", null, null, fixture.CapaId, null, null, fixture.UserId));
        await fixture.Service.DefineTargetAsync(new DefineIndicatorTargetCommand(fixture.TenantId, accumulated.Value!.Id, 5, fixture.Clock.UtcNow, fixture.UserId));
        var accPeriod = await fixture.Service.AddPeriodAsync(new AddIndicatorPeriodCommand(fixture.TenantId, accumulated.Value.Id, 2026, 6, fixture.Clock.UtcNow, fixture.Clock.UtcNow.AddMonths(1), fixture.UserId));
        var accMeasurement = await fixture.Service.CaptureMeasurementAsync(new CaptureIndicatorMeasurementCommand(fixture.TenantId, accumulated.Value.Id, accPeriod.Value!.Id, 3, null, false, fixture.UserId));
        var accResult = await fixture.Service.CalculateResultAsync(new CalculateIndicatorResultCommand(fixture.TenantId, accumulated.Value.Id, accPeriod.Value.Id, accMeasurement.Value!.Id, fixture.UserId));
        var csvExport = await fixture.Service.ExportAsync(new IndicatorExportQuery(fixture.TenantId, null, null, "", fixture.UserId));

        Assert.Equal(10, averageResult.Value!.Value);
        Assert.Equal(IndicatorResultStatus.OnTarget, averageResult.Value.Status);
        Assert.Equal(15, directResult.Value!.Value);
        Assert.Equal(IndicatorResultStatus.OnTarget, directResult.Value.Status);
        Assert.Equal(0, ratioResult.Value!.Value);
        Assert.True(missingDenominatorResult.IsFailure);
        Assert.Equal(IndicatorResultStatus.BelowTarget, accResult.Value!.Status);
        Assert.Equal("csv", csvExport.Value!.Format);
        Assert.Equal("text/csv", csvExport.Value.ContentType);
    }

    [Fact]
    public void Indicator_Domain_Covers_Dashboard_And_Alternate_Calculation_Branches()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var empty = new QualityIndicator(tenantId, Guid.NewGuid(), "Empty", "EMPTY", "Empty KPI", IndicatorType.Iso9001, IndicatorFrequency.Monthly, IndicatorCalculationType.Accumulated, "count", null, null, null, null, null, userId, now);
        var emptyDashboard = empty.Dashboard();
        var ratio = new QualityIndicator(tenantId, Guid.NewGuid(), "Ratio", "RATIO", "Ratio KPI", IndicatorType.Process, IndicatorFrequency.Monthly, IndicatorCalculationType.Ratio, "ratio", null, null, null, null, null, userId, now);
        ratio.DefineTarget(1, now, userId, now);
        var ratioPeriod = ratio.AddPeriod(2026, 1, now, now.AddMonths(1), userId, now);
        var ratioMeasurement = ratio.CaptureMeasurement(ratioPeriod.Id, 3, 2, false, userId, now);
        var ratioResult = ratio.CalculateResult(ratioPeriod.Id, ratioMeasurement.Id, userId, now);
        var percentage = new QualityIndicator(tenantId, Guid.NewGuid(), "Percentage", "PCT", "Percentage KPI", IndicatorType.Operational, IndicatorFrequency.Monthly, IndicatorCalculationType.Percentage, "%", null, null, null, null, null, userId, now);
        percentage.DefineTarget(1, now, userId, now);
        var pctPeriod = percentage.AddPeriod(2026, 1, now, now.AddMonths(1), userId, now);
        var pctMeasurement = percentage.CaptureMeasurement(pctPeriod.Id, 1, 0, false, userId, now);
        var pctResult = percentage.CalculateResult(pctPeriod.Id, pctMeasurement.Id, userId, now);
        var custom = new QualityIndicator(tenantId, Guid.NewGuid(), "Custom", "CUSTOM", "Custom KPI", IndicatorType.Strategic, IndicatorFrequency.Annual, IndicatorCalculationType.Custom, "score", null, null, null, null, null, userId, now);
        custom.DefineTarget(5, now, userId, now);
        var customPeriod1 = custom.AddPeriod(2026, 1, now, now.AddMonths(1), userId, now);
        var customMeasurement1 = custom.CaptureMeasurement(customPeriod1.Id, 4, null, false, userId, now);
        custom.CalculateResult(customPeriod1.Id, customMeasurement1.Id, userId, now);
        var customPeriod2 = custom.AddPeriod(2026, 2, now.AddMonths(1), now.AddMonths(2), userId, now);
        var customMeasurement2 = custom.CaptureMeasurement(customPeriod2.Id, 6, null, false, userId, now);
        var customResult2 = custom.CalculateResult(customPeriod2.Id, customMeasurement2.Id, userId, now);

        Assert.Equal(100, emptyDashboard.CompliancePercent);
        Assert.Equal(0, emptyDashboard.LatestValue);
        Assert.Equal(IndicatorTrendDirection.Stable, emptyDashboard.Trend);
        Assert.Equal(1.5m, ratioResult.Value);
        Assert.Equal(0, pctResult.Value);
        Assert.Equal(6, customResult2.Value);
        Assert.Contains(custom.Trends, trend => trend.Direction == IndicatorTrendDirection.Positive);
    }

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class QualityFixture
    {
        private QualityFixture()
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            SupplierId = Guid.NewGuid();
            DocumentId = Guid.NewGuid();
            AuditId = Guid.NewGuid();
            CapaId = Guid.NewGuid();
            RiskId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryQualityIndicatorRepository();
            Service = new QualityIndicatorService(Repository, new FakeApplicationDbContext(), Clock);
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public Guid SupplierId { get; }
        public Guid DocumentId { get; }
        public Guid AuditId { get; }
        public Guid CapaId { get; }
        public Guid RiskId { get; }
        public FixedClock Clock { get; }
        public InMemoryQualityIndicatorRepository Repository { get; }
        public QualityIndicatorService Service { get; }
        public static QualityFixture Create() => new();
    }

    private sealed class InMemoryQualityIndicatorRepository : IQualityIndicatorRepository
    {
        public List<IndicatorCategory> Categories { get; } = [];
        public List<QualityIndicator> Indicators { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];
        public Task AddCategoryAsync(IndicatorCategory category, CancellationToken cancellationToken = default) { Categories.Add(category); return Task.CompletedTask; }
        public Task<IndicatorCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default) => Task.FromResult(Categories.SingleOrDefault(category => category.TenantId == tenantId && category.Id == categoryId));
        public Task<bool> CategoryCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => Task.FromResult(Categories.Any(category => category.TenantId == tenantId && category.Code == code.ToUpperInvariant()));
        public Task AddIndicatorAsync(QualityIndicator indicator, CancellationToken cancellationToken = default) { Indicators.Add(indicator); return Task.CompletedTask; }
        public Task<QualityIndicator?> GetIndicatorAsync(Guid tenantId, Guid indicatorId, CancellationToken cancellationToken = default) => Task.FromResult(Indicators.SingleOrDefault(indicator => indicator.TenantId == tenantId && indicator.Id == indicatorId));
        public Task<bool> IndicatorCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => Task.FromResult(Indicators.Any(indicator => indicator.TenantId == tenantId && indicator.Code == code.ToUpperInvariant()));
        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default) { AuditLogs.Add(auditLog); return Task.CompletedTask; }

        public Task<IndicatorSearchResult> SearchAsync(IndicatorSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var items = Indicators.Where(indicator => indicator.TenantId == criteria.TenantId)
                .Where(indicator => criteria.SearchText is null || indicator.Name.Contains(criteria.SearchText) || indicator.Code.Contains(criteria.SearchText))
                .Where(indicator => !criteria.Status.HasValue || indicator.Status == criteria.Status.Value)
                .Where(indicator => !criteria.Type.HasValue || indicator.Type == criteria.Type.Value)
                .Where(indicator => !criteria.Frequency.HasValue || indicator.Frequency == criteria.Frequency.Value)
                .Where(indicator => !criteria.SupplierId.HasValue || indicator.SupplierId == criteria.SupplierId.Value)
                .Where(indicator => !criteria.AuditId.HasValue || indicator.AuditId == criteria.AuditId.Value)
                .Where(indicator => !criteria.CapaId.HasValue || indicator.CapaId == criteria.CapaId.Value)
                .Where(indicator => !criteria.RiskId.HasValue || indicator.RiskId == criteria.RiskId.Value)
                .Select(indicator => new QualityIndicatorSummary(indicator.Id, indicator.TenantId, indicator.Name, indicator.Code, indicator.Type, indicator.Frequency, indicator.CalculationType, indicator.Status, indicator.Unit))
                .ToArray();
            return Task.FromResult(new IndicatorSearchResult(items.Skip((criteria.Page - 1) * criteria.PageSize).Take(criteria.PageSize).ToArray(), items.Length, criteria.Page, criteria.PageSize));
        }

        public Task<IndicatorDashboardDto> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            var indicators = Indicators.Where(indicator => indicator.TenantId == tenantId).ToArray();
            var results = indicators.SelectMany(indicator => indicator.Results).ToArray();
            var alerts = indicators.SelectMany(indicator => indicator.Alerts).Count();
            var compliance = results.Length == 0 ? 100 : (int)Math.Round(results.Count(result => result.Status is IndicatorResultStatus.OnTarget or IndicatorResultStatus.AboveTarget) * 100m / results.Length);
            return Task.FromResult(new IndicatorDashboardDto(indicators.Length, indicators.Count(indicator => indicator.Status == IndicatorStatus.Approved), results.Count(result => result.Status == IndicatorResultStatus.CriticalDeviation), alerts, compliance, indicators.SelectMany(indicator => indicator.Trends).Count(trend => trend.Direction == IndicatorTrendDirection.Negative), indicators.SelectMany(indicator => indicator.Processes).Select(process => process.Area).Distinct().Count()));
        }

        public Task<IReadOnlyCollection<IndicatorTrendSummary>> GetTrendsAsync(Guid tenantId, Guid? indicatorId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<IndicatorTrendSummary>>(Indicators.Where(indicator => indicator.TenantId == tenantId && (!indicatorId.HasValue || indicator.Id == indicatorId.Value)).SelectMany(indicator => indicator.Trends).Select(trend => new IndicatorTrendSummary(trend.Id, trend.IndicatorId, trend.PeriodId, trend.Direction, trend.Value, trend.PreviousValue)).ToArray());
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext { public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1); }
    private sealed class FixedClock : IClock { public DateTimeOffset UtcNow { get; } = new(2026, 6, 20, 12, 0, 0, TimeSpan.Zero); }
}
