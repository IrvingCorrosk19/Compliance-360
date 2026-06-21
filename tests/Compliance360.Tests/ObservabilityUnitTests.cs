using Compliance360.Web.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;

namespace Compliance360.Tests;

public sealed class ObservabilityUnitTests
{
    [Theory]
    [InlineData("/api/v1/auth/login", "Authentication")]
    [InlineData("/api/v1/documents", "Documents")]
    [InlineData("/api/v1/suppliers", "Suppliers")]
    [InlineData("/api/v1/audit-management/plans", "AuditManagement")]
    [InlineData("/api/v1/capas", "CAPA")]
    [InlineData("/api/v1/risks", "Risk")]
    [InlineData("/api/v1/indicators", "Indicators")]
    [InlineData("/api/v1/reports", "Reporting")]
    [InlineData("/api/v1/storage", "Storage")]
    [InlineData("/api/v1/notifications", "Notifications")]
    [InlineData("/api/v1/workflows", "Workflows")]
    [InlineData("/api/v1/technical-sheets", "TechnicalSheets")]
    [InlineData("/api/v1/observability/metrics", "Observability")]
    [InlineData("/health/ready", "Health")]
    [InlineData("/api/v1/rbac/roles", "Authorization")]
    [InlineData("/api/v1/tenants", "Platform")]
    public void Module_Resolver_Covers_Enterprise_Modules(string route, string expectedModule)
    {
        Assert.Equal(expectedModule, ObservabilityModuleResolver.Resolve(route));
    }

    [Fact]
    public void Telemetry_Records_Business_Events_Failures_And_Tenant_Metrics()
    {
        using var telemetry = new ObservabilityTelemetry();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        telemetry.RecordBusinessEvent("Reporting", "ReportExecuted", tenantId, userId, 125);
        telemetry.RecordBusinessEvent("Reporting", "ReportScheduled", null, null);
        telemetry.RecordFailure("Reporting", "ReportFailed", tenantId, userId);

        var snapshot = telemetry.Snapshot;

        Assert.Contains(snapshot.Metrics, metric => metric.Module == "Reporting" && metric.Count > 0);
        Assert.Contains(snapshot.Metrics, metric => metric.Failures > 0);
        Assert.Contains(snapshot.Tenants, tenant => tenant.TenantId == tenantId && tenant.RequestsByModule.ContainsKey("Reporting"));
    }

    [Fact]
    public void Metric_Dtos_Report_Zero_And_NonZero_Averages()
    {
        var metric = new ObservabilityMetricDto("api.platform.requests", "API Requests", "Platform", 0, 0, 0, 0);
        var tenant = new TenantObservabilityDto(Guid.NewGuid(), 0, 0, 0, new Dictionary<string, long>());

        Assert.Equal(0, metric.AverageDurationMs);
        Assert.Equal(0, metric.FailureRatePercent);
        Assert.Equal(0, tenant.AverageDurationMs);
        Assert.Equal(0, tenant.FailureRatePercent);

        metric.Increment(250, true);
        tenant.Increment("Platform", 250, true);

        Assert.Equal(250, metric.AverageDurationMs);
        Assert.Equal(100, metric.FailureRatePercent);
        Assert.Equal(250, tenant.AverageDurationMs);
        Assert.Equal(100, tenant.FailureRatePercent);
    }

    [Fact]
    public async Task Middleware_Records_Failures_When_Downstream_Throws()
    {
        using var telemetry = new ObservabilityTelemetry();
        var middleware = new ObservabilityMiddleware(
            _ => throw new InvalidOperationException("boom"),
            NullLogger<ObservabilityMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/reports";

        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context, telemetry));

        Assert.Contains(telemetry.Snapshot.Metrics, metric => metric.Module == "Reporting" && metric.Failures > 0);
    }

    [Fact]
    public async Task Dependency_Health_Checks_Return_Unhealthy_When_Dependency_Is_Missing()
    {
        var healthContext = new HealthCheckContext();

        var storage = await new StorageHealthCheck(null!).CheckHealthAsync(healthContext);
        var notification = await new NotificationHealthCheck(null!).CheckHealthAsync(healthContext);
        var reporting = await new ReportingHealthCheck(null!).CheckHealthAsync(healthContext);
        var workflow = await new WorkflowHealthCheck(null!).CheckHealthAsync(healthContext);

        Assert.Equal(HealthStatus.Unhealthy, storage.Status);
        Assert.Equal(HealthStatus.Unhealthy, notification.Status);
        Assert.Equal(HealthStatus.Unhealthy, reporting.Status);
        Assert.Equal(HealthStatus.Unhealthy, workflow.Status);
    }
}
