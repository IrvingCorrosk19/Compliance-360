using Compliance360.Domain.Audit;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Web.Security;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Compliance360.Web.Observability;

public static class ObservabilityEndpoints
{
    public static void MapObservabilityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        endpoints.MapGet("/telemetry", (HttpContext httpContext, IObservabilityTelemetry telemetry) =>
        {
            var activity = System.Diagnostics.Activity.Current;
            return Results.Ok(new TelemetryContextDto(
                httpContext.Items["CorrelationId"]?.ToString() ?? httpContext.TraceIdentifier,
                httpContext.TraceIdentifier,
                ObservabilityContextReader.TenantId(httpContext),
                ObservabilityContextReader.UserId(httpContext),
                ObservabilityContextReader.SessionId(httpContext),
                activity?.TraceId.ToString(),
                activity?.SpanId.ToString(),
                telemetry.Snapshot.CapturedAtUtc));
        }).RequireAuthorization(PermissionPolicies.ObservabilityRead).WithTags("Observability");

        var observability = endpoints.MapGroup("/api/v1/observability")
            .WithTags("Observability")
            .RequireAuthorization(PermissionPolicies.ObservabilityRead);

        observability.MapGet("/telemetry", (HttpContext httpContext, IObservabilityTelemetry telemetry) =>
            Results.Ok(new
            {
                context = new TelemetryContextDto(
                    httpContext.Items["CorrelationId"]?.ToString() ?? httpContext.TraceIdentifier,
                    httpContext.TraceIdentifier,
                    ObservabilityContextReader.TenantId(httpContext),
                    ObservabilityContextReader.UserId(httpContext),
                    ObservabilityContextReader.SessionId(httpContext),
                    System.Diagnostics.Activity.Current?.TraceId.ToString(),
                    System.Diagnostics.Activity.Current?.SpanId.ToString(),
                    telemetry.Snapshot.CapturedAtUtc),
                resource = ObservabilityResource.Metadata
            }));

        observability.MapGet("/metrics/summary", (IObservabilityTelemetry telemetry) =>
            Results.Ok(new ObservabilityMetricsSummaryDto(
                telemetry.Snapshot.Metrics,
                telemetry.Snapshot.Tenants,
                telemetry.Snapshot.CapturedAtUtc)));

        observability.MapGet("/dashboards/operational", (IObservabilityTelemetry telemetry) =>
            Results.Ok(OperationalDashboard("Operational Dashboard", telemetry.Snapshot)));

        observability.MapGet("/dashboards/system", (IObservabilityTelemetry telemetry) =>
            Results.Ok(OperationalDashboard("System Dashboard", telemetry.Snapshot)));

        observability.MapGet("/dashboards/performance", (IObservabilityTelemetry telemetry) =>
            Results.Ok(PerformanceDashboard(telemetry.Snapshot)));

        observability.MapGet("/dashboards/security", (IObservabilityTelemetry telemetry) =>
            Results.Ok(SecurityDashboard(telemetry.Snapshot)));

        observability.MapGet("/dashboards/tenants", (IObservabilityTelemetry telemetry) =>
            Results.Ok(new TenantDashboardDto("Tenant Dashboard", telemetry.Snapshot.Tenants, telemetry.Snapshot.CapturedAtUtc)));

        observability.MapGet("/alerts", (IObservabilityTelemetry telemetry) =>
            Results.Ok(AlertRules(telemetry.Snapshot)));

        observability.MapPost("/configuration/audit", async (MonitoringConfigurationChangeRequest request, HttpContext httpContext, IObservabilityTelemetry telemetry, Compliance360DbContext dbContext, CancellationToken cancellationToken) =>
        {
            var tenantId = ObservabilityContextReader.TenantId(httpContext);
            var userId = ObservabilityContextReader.UserId(httpContext);
            var action = request.Type?.ToLowerInvariant() switch
            {
                "telemetry" => AuditAction.TelemetryConfigurationChanged,
                "alert" => AuditAction.AlertConfigurationChanged,
                _ => AuditAction.MonitoringConfigurationChanged
            };
            telemetry.RecordBusinessEvent("Observability", action.ToString(), tenantId, userId);
            await dbContext.AuditLogs.AddAsync(AuditLog.Create(tenantId, userId, "ObservabilityConfiguration", null, action, DateTimeOffset.UtcNow), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Results.Ok(new
            {
                action,
                request.Type,
                request.Component,
                request.Change,
                changedAtUtc = DateTimeOffset.UtcNow
            });
        }).RequireAuthorization(PermissionPolicies.ObservabilityManage);
    }

    private static OperationalDashboardDto OperationalDashboard(string name, ObservabilitySnapshot snapshot)
    {
        return new OperationalDashboardDto(
            name,
            snapshot.Metrics.Sum(metric => metric.Count),
            snapshot.Metrics.Sum(metric => metric.Failures),
            snapshot.Metrics.Any() ? Math.Round(snapshot.Metrics.Average(metric => metric.AverageDurationMs), 2) : 0,
            snapshot.Metrics.GroupBy(metric => metric.Module).ToDictionary(group => group.Key, group => group.Sum(metric => metric.Count)),
            snapshot.CapturedAtUtc);
    }

    private static PerformanceDashboardDto PerformanceDashboard(ObservabilitySnapshot snapshot)
    {
        return new PerformanceDashboardDto(
            "Performance Dashboard",
            snapshot.Metrics.OrderByDescending(metric => metric.MaxDurationMs).Take(10).ToArray(),
            snapshot.Metrics.Any() ? snapshot.Metrics.Max(metric => metric.MaxDurationMs) : 0,
            snapshot.CapturedAtUtc);
    }

    private static SecurityDashboardDto SecurityDashboard(ObservabilitySnapshot snapshot)
    {
        var securityMetrics = snapshot.Metrics
            .Where(metric => metric.Module is "Authentication" or "Authorization" or "Observability")
            .ToArray();
        return new SecurityDashboardDto(
            "Security Dashboard",
            securityMetrics.Sum(metric => metric.Count),
            securityMetrics.Sum(metric => metric.Failures),
            securityMetrics,
            snapshot.CapturedAtUtc);
    }

    private static IReadOnlyCollection<AlertRuleDto> AlertRules(ObservabilitySnapshot snapshot)
    {
        var highErrorRate = snapshot.Metrics.Any(metric => metric.FailureRatePercent >= 5);
        var highLatency = snapshot.Metrics.Any(metric => metric.MaxDurationMs >= 1_000);
        return
        [
            Alert("Application Down", false, "Application health check is unhealthy."),
            Alert("Database Down", false, "PostgreSQL readiness check is unhealthy."),
            Alert("Storage Down", false, "Storage readiness check is unhealthy."),
            Alert("Notification Failure", snapshot.Metrics.Any(metric => metric.Module == "Notifications" && metric.Failures > 0), "Notification failures detected."),
            Alert("High Error Rate", highErrorRate, "Any module has error rate >= 5%."),
            Alert("High Latency", highLatency, "Any module has max latency >= 1000 ms."),
            Alert("Authentication Failures", snapshot.Metrics.Any(metric => metric.Module == "Authentication" && metric.Failures > 0), "Authentication failures detected."),
            Alert("MFA Failures", snapshot.Metrics.Any(metric => metric.Key.Contains("Mfa", StringComparison.OrdinalIgnoreCase) && metric.Failures > 0), "MFA failures detected."),
            Alert("Workflow Failures", snapshot.Metrics.Any(metric => metric.Module == "Workflows" && metric.Failures > 0), "Workflow failures detected."),
            Alert("Report Failures", snapshot.Metrics.Any(metric => metric.Module == "Reporting" && metric.Failures > 0), "Report failures detected.")
        ];
    }

    private static AlertRuleDto Alert(string name, bool active, string condition) =>
        new(name, active ? "Active" : "Ready", condition, active ? "Warning" : "Information");
}

public static class ObservabilityResource
{
    public static object Metadata => new
    {
        service = ObservabilityTelemetry.ServiceName,
        version = "v0.19.0-observability-enterprise",
        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
        resourceAttributes = new[] { "service.name", "service.version", "deployment.environment" }
    };
}

public sealed record TelemetryContextDto(
    string CorrelationId,
    string RequestId,
    Guid? TenantId,
    Guid? UserId,
    Guid? SessionId,
    string? TraceId,
    string? SpanId,
    DateTimeOffset CapturedAtUtc);

public sealed record ObservabilityMetricsSummaryDto(
    IReadOnlyCollection<ObservabilityMetricDto> Metrics,
    IReadOnlyCollection<TenantObservabilityDto> Tenants,
    DateTimeOffset CapturedAtUtc);

public sealed record OperationalDashboardDto(
    string Name,
    long TotalRequests,
    long TotalFailures,
    double AverageDurationMs,
    IReadOnlyDictionary<string, long> RequestsByModule,
    DateTimeOffset CapturedAtUtc);

public sealed record PerformanceDashboardDto(
    string Name,
    IReadOnlyCollection<ObservabilityMetricDto> SlowestOperations,
    double MaxDurationMs,
    DateTimeOffset CapturedAtUtc);

public sealed record SecurityDashboardDto(
    string Name,
    long SecurityEvents,
    long SecurityFailures,
    IReadOnlyCollection<ObservabilityMetricDto> Metrics,
    DateTimeOffset CapturedAtUtc);

public sealed record TenantDashboardDto(
    string Name,
    IReadOnlyCollection<TenantObservabilityDto> Tenants,
    DateTimeOffset CapturedAtUtc);

public sealed record AlertRuleDto(string Name, string Status, string Condition, string Severity);

public sealed record MonitoringConfigurationChangeRequest(string? Type, string Component, string Change);
