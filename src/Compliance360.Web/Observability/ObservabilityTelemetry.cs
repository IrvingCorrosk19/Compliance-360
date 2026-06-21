using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Claims;

namespace Compliance360.Web.Observability;

public interface IObservabilityTelemetry
{
    ActivitySource ActivitySource { get; }

    Meter Meter { get; }

    ObservabilitySnapshot Snapshot { get; }

    void RecordRequest(HttpContext httpContext, double durationMs);

    void RecordBusinessEvent(string module, string eventName, Guid? tenantId, Guid? userId, double durationMs = 0);

    void RecordFailure(string module, string failureType, Guid? tenantId, Guid? userId);
}

public sealed class ObservabilityTelemetry : IObservabilityTelemetry, IDisposable
{
    public const string ServiceName = "Compliance360.Enterprise";
    public const string MeterName = "Compliance360.Enterprise.Observability";
    public const string ActivitySourceName = "Compliance360.Enterprise";

    private readonly object _sync = new();
    private readonly Counter<long> _apiRequests;
    private readonly Counter<long> _apiFailures;
    private readonly Counter<long> _businessEvents;
    private readonly Counter<long> _businessFailures;
    private readonly Histogram<double> _requestDuration;
    private readonly Histogram<double> _businessDuration;
    private readonly Dictionary<string, ObservabilityMetricDto> _metrics = [];
    private readonly Dictionary<Guid, TenantObservabilityDto> _tenantMetrics = [];

    public ObservabilityTelemetry()
    {
        ActivitySource = new ActivitySource(ActivitySourceName);
        Meter = new Meter(MeterName, "1.0.0");
        _apiRequests = Meter.CreateCounter<long>("compliance360_api_requests_total", "requests", "Total API requests.");
        _apiFailures = Meter.CreateCounter<long>("compliance360_api_failures_total", "failures", "Total API failures.");
        _businessEvents = Meter.CreateCounter<long>("compliance360_business_events_total", "events", "Total business events.");
        _businessFailures = Meter.CreateCounter<long>("compliance360_business_failures_total", "failures", "Total business failures.");
        _requestDuration = Meter.CreateHistogram<double>("compliance360_api_request_duration_ms", "ms", "API request duration.");
        _businessDuration = Meter.CreateHistogram<double>("compliance360_business_duration_ms", "ms", "Business operation duration.");
    }

    public ActivitySource ActivitySource { get; }

    public Meter Meter { get; }

    public ObservabilitySnapshot Snapshot
    {
        get
        {
            lock (_sync)
            {
                return new ObservabilitySnapshot(
                    _metrics.Values.OrderBy(metric => metric.Name).ToArray(),
                    _tenantMetrics.Values.OrderBy(metric => metric.TenantId).ToArray(),
                    DateTimeOffset.UtcNow);
            }
        }
    }

    public void RecordRequest(HttpContext httpContext, double durationMs)
    {
        var route = httpContext.Request.Path.Value ?? "/";
        var module = ObservabilityModuleResolver.Resolve(route);
        var tenantId = ObservabilityContextReader.TenantId(httpContext);
        var userId = ObservabilityContextReader.UserId(httpContext);
        var statusCode = httpContext.Response.StatusCode;
        var tags = Tags(module, tenantId, userId, statusCode.ToString());

        _apiRequests.Add(1, tags);
        _requestDuration.Record(durationMs, tags);
        if (statusCode >= 400)
        {
            _apiFailures.Add(1, tags);
        }

        lock (_sync)
        {
            UpsertMetric($"api.{module}.requests", "API Requests", module).Increment(durationMs, statusCode >= 400);
            if (tenantId.HasValue)
            {
                UpsertTenant(tenantId.Value).Increment(module, durationMs, statusCode >= 400);
            }
        }
    }

    public void RecordBusinessEvent(string module, string eventName, Guid? tenantId, Guid? userId, double durationMs = 0)
    {
        var tags = Tags(module, tenantId, userId, eventName);
        _businessEvents.Add(1, tags);
        if (durationMs > 0)
        {
            _businessDuration.Record(durationMs, tags);
        }

        lock (_sync)
        {
            UpsertMetric($"business.{module}.{eventName}", "Business Events", module).Increment(durationMs, false);
            if (tenantId.HasValue)
            {
                UpsertTenant(tenantId.Value).Increment(module, durationMs, false);
            }
        }
    }

    public void RecordFailure(string module, string failureType, Guid? tenantId, Guid? userId)
    {
        var tags = Tags(module, tenantId, userId, failureType);
        _businessFailures.Add(1, tags);
        lock (_sync)
        {
            UpsertMetric($"business.{module}.{failureType}.failures", "Business Failures", module).Increment(0, true);
            if (tenantId.HasValue)
            {
                UpsertTenant(tenantId.Value).Increment(module, 0, true);
            }
        }
    }

    public void Dispose()
    {
        ActivitySource.Dispose();
        Meter.Dispose();
    }

    private static TagList Tags(string module, Guid? tenantId, Guid? userId, string outcome)
    {
        var tags = new TagList
        {
            { "service.name", ServiceName },
            { "module", module },
            { "outcome", outcome }
        };
        if (tenantId.HasValue) tags.Add("tenant.id", tenantId.Value);
        if (userId.HasValue) tags.Add("user.id", userId.Value);
        return tags;
    }

    private ObservabilityMetricDto UpsertMetric(string key, string name, string module)
    {
        if (!_metrics.TryGetValue(key, out var metric))
        {
            metric = new ObservabilityMetricDto(key, name, module, 0, 0, 0, 0);
            _metrics[key] = metric;
        }

        return metric;
    }

    private TenantObservabilityDto UpsertTenant(Guid tenantId)
    {
        if (!_tenantMetrics.TryGetValue(tenantId, out var metric))
        {
            metric = new TenantObservabilityDto(tenantId, 0, 0, 0, new Dictionary<string, long>());
            _tenantMetrics[tenantId] = metric;
        }

        return metric;
    }
}

public sealed record ObservabilitySnapshot(
    IReadOnlyCollection<ObservabilityMetricDto> Metrics,
    IReadOnlyCollection<TenantObservabilityDto> Tenants,
    DateTimeOffset CapturedAtUtc);

public sealed class ObservabilityMetricDto
{
    public ObservabilityMetricDto(string key, string name, string module, long count, long failures, double totalDurationMs, double maxDurationMs)
    {
        Key = key;
        Name = name;
        Module = module;
        Count = count;
        Failures = failures;
        TotalDurationMs = totalDurationMs;
        MaxDurationMs = maxDurationMs;
    }

    public string Key { get; }

    public string Name { get; }

    public string Module { get; }

    public long Count { get; private set; }

    public long Failures { get; private set; }

    public double TotalDurationMs { get; private set; }

    public double MaxDurationMs { get; private set; }

    public double AverageDurationMs => Count == 0 ? 0 : Math.Round(TotalDurationMs / Count, 2);

    public double FailureRatePercent => Count == 0 ? 0 : Math.Round((double)Failures / Count * 100, 2);

    public void Increment(double durationMs, bool failed)
    {
        Count++;
        if (failed) Failures++;
        TotalDurationMs += durationMs;
        MaxDurationMs = Math.Max(MaxDurationMs, durationMs);
    }
}

public sealed class TenantObservabilityDto
{
    public TenantObservabilityDto(Guid tenantId, long requestCount, long failureCount, double totalDurationMs, IReadOnlyDictionary<string, long> requestsByModule)
    {
        TenantId = tenantId;
        RequestCount = requestCount;
        FailureCount = failureCount;
        TotalDurationMs = totalDurationMs;
        RequestsByModule = requestsByModule;
    }

    public Guid TenantId { get; }

    public long RequestCount { get; private set; }

    public long FailureCount { get; private set; }

    public double TotalDurationMs { get; private set; }

    public IReadOnlyDictionary<string, long> RequestsByModule { get; private set; }

    public double AverageDurationMs => RequestCount == 0 ? 0 : Math.Round(TotalDurationMs / RequestCount, 2);

    public double FailureRatePercent => RequestCount == 0 ? 0 : Math.Round((double)FailureCount / RequestCount * 100, 2);

    public void Increment(string module, double durationMs, bool failed)
    {
        RequestCount++;
        if (failed) FailureCount++;
        TotalDurationMs += durationMs;
        var mutable = RequestsByModule as Dictionary<string, long> ?? new Dictionary<string, long>(RequestsByModule);
        mutable[module] = mutable.GetValueOrDefault(module) + 1;
        RequestsByModule = mutable;
    }
}

public static class ObservabilityContextReader
{
    public static Guid? TenantId(HttpContext httpContext) =>
        ReadGuid(httpContext.User.FindFirstValue("tenant_id")) ?? ReadGuid(httpContext.Request.Headers["X-Tenant-Id"].ToString());

    public static Guid? UserId(HttpContext httpContext) =>
        ReadGuid(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))
        ?? ReadGuid(httpContext.User.FindFirstValue("sub"))
        ?? ReadGuid(httpContext.Request.Headers["X-User-Id"].ToString());

    public static Guid? SessionId(HttpContext httpContext) =>
        ReadGuid(httpContext.User.FindFirstValue("session_id"));

    private static Guid? ReadGuid(string? value) => Guid.TryParse(value, out var parsed) ? parsed : null;
}

public static class ObservabilityModuleResolver
{
    public static string Resolve(string route)
    {
        var normalized = route.ToLowerInvariant();
        if (normalized.Contains("auth") || normalized.Contains("mfa")) return "Authentication";
        if (normalized.Contains("documents")) return "Documents";
        if (normalized.Contains("suppliers")) return "Suppliers";
        if (normalized.Contains("audit-management")) return "AuditManagement";
        if (normalized.Contains("capas")) return "CAPA";
        if (normalized.Contains("risks")) return "Risk";
        if (normalized.Contains("indicators")) return "Indicators";
        if (normalized.Contains("reports")) return "Reporting";
        if (normalized.Contains("storage")) return "Storage";
        if (normalized.Contains("notifications")) return "Notifications";
        if (normalized.Contains("workflows")) return "Workflows";
        if (normalized.Contains("technical-sheets")) return "TechnicalSheets";
        if (normalized.Contains("observability") || normalized.Contains("telemetry") || normalized.Contains("metrics")) return "Observability";
        if (normalized.Contains("health")) return "Health";
        if (normalized.Contains("rbac")) return "Authorization";
        return "Platform";
    }
}
