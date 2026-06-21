using System.Diagnostics;
using Serilog.Context;

namespace Compliance360.Web.Observability;

public sealed class ObservabilityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ObservabilityMiddleware> _logger;

    public ObservabilityMiddleware(RequestDelegate next, ILogger<ObservabilityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext, IObservabilityTelemetry telemetry)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = ResolveCorrelationId(httpContext);
        var tenantId = ObservabilityContextReader.TenantId(httpContext);
        var userId = ObservabilityContextReader.UserId(httpContext);
        var sessionId = ObservabilityContextReader.SessionId(httpContext);
        var module = ObservabilityModuleResolver.Resolve(httpContext.Request.Path.Value ?? "/");

        httpContext.Items["CorrelationId"] = correlationId;
        httpContext.Response.Headers.TryAdd("X-Correlation-Id", correlationId);
        httpContext.Response.Headers.TryAdd("X-Request-Id", httpContext.TraceIdentifier);

        using var activity = telemetry.ActivitySource.StartActivity($"HTTP {httpContext.Request.Method} {module}", ActivityKind.Server);
        EnrichActivity(activity, httpContext, correlationId, tenantId, userId, sessionId, module);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestId", httpContext.TraceIdentifier))
        using (LogContext.PushProperty("TenantId", tenantId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("SessionId", sessionId))
        using (_logger.BeginScope(new Dictionary<string, object?>
               {
                   ["CorrelationId"] = correlationId,
                   ["RequestId"] = httpContext.TraceIdentifier,
                   ["TenantId"] = tenantId,
                   ["UserId"] = userId,
                   ["SessionId"] = sessionId,
                   ["TraceId"] = activity?.TraceId.ToString(),
                   ["SpanId"] = activity?.SpanId.ToString(),
                   ["Module"] = module
               }))
        {
            try
            {
                await _next(httpContext);
                activity?.SetTag("http.response.status_code", httpContext.Response.StatusCode);
            }
            catch (Exception exception)
            {
                activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                telemetry.RecordFailure(module, exception.GetType().Name, tenantId, userId);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                telemetry.RecordRequest(httpContext, stopwatch.Elapsed.TotalMilliseconds);
                _logger.LogInformation(
                    "HTTP {Method} {Path} completed with {StatusCode} in {DurationMs} ms",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    httpContext.Response.StatusCode,
                    Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2));
            }
        }
    }

    private static string ResolveCorrelationId(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var header) && !string.IsNullOrWhiteSpace(header))
        {
            return header.ToString();
        }

        return Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
    }

    private static void EnrichActivity(
        Activity? activity,
        HttpContext httpContext,
        string correlationId,
        Guid? tenantId,
        Guid? userId,
        Guid? sessionId,
        string module)
    {
        if (activity is null) return;

        activity.SetTag("correlation.id", correlationId);
        activity.SetTag("request.id", httpContext.TraceIdentifier);
        activity.SetTag("tenant.id", tenantId);
        activity.SetTag("user.id", userId);
        activity.SetTag("session.id", sessionId);
        activity.SetTag("module", module);
        activity.SetTag("http.request.method", httpContext.Request.Method);
        activity.SetTag("url.path", httpContext.Request.Path.Value);
    }
}
