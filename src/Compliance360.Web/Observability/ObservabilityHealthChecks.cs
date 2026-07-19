using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Application.Reporting;
using Compliance360.Application.Storage;
using Compliance360.Application.Workflows;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Compliance360.Web.Observability;

public sealed class ApplicationHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(HealthCheckResult.Healthy("Application is accepting requests."));
}

public sealed class PostgreSqlHealthCheck : IHealthCheck
{
    private readonly Compliance360DbContext _dbContext;

    public PostgreSqlHealthCheck(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy("PostgreSQL connection is available.")
            : HealthCheckResult.Unhealthy("PostgreSQL connection is unavailable.");
    }
}

public sealed class StorageHealthCheck : IHealthCheck
{
    private readonly IFileStorageService _fileStorageService;

    public StorageHealthCheck(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(_fileStorageService is not null
            ? HealthCheckResult.Healthy("Storage service is registered.")
            : HealthCheckResult.Unhealthy("Storage service is unavailable."));
}

public sealed class NotificationHealthCheck : IHealthCheck
{
    private readonly INotificationDispatcher _dispatcher;

    public NotificationHealthCheck(INotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(_dispatcher is not null
            ? HealthCheckResult.Healthy("Notification dispatcher is registered.")
            : HealthCheckResult.Unhealthy("Notification dispatcher is unavailable."));
}

public sealed class NotificationProviderHealthCheck : IHealthCheck
{
    private readonly INotificationProviderFactory _providerFactory;
    private readonly IOptions<NotificationProviderOptions> _options;
    private readonly NotificationProvider _provider;

    public NotificationProviderHealthCheck(INotificationProviderFactory providerFactory, IOptions<NotificationProviderOptions> options, NotificationProvider provider)
    {
        _providerFactory = providerFactory;
        _options = options;
        _provider = provider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        _options.Value.Providers.TryGetValue(_provider, out var providerOptions);
        var endpoint = new NotificationProviderEndpoint(_provider, providerOptions?.Host, providerOptions?.Port, providerOptions?.Username, providerOptions?.Secret, providerOptions?.FromAddress, providerOptions?.FromName, providerOptions?.BaseUrl, providerOptions?.Domain, providerOptions?.UseSsl ?? true);
        var result = await _providerFactory.GetProvider(_provider).CheckHealthAsync(endpoint, cancellationToken);
        return result.Healthy ? HealthCheckResult.Healthy(result.Message) : HealthCheckResult.Degraded(result.Message);
    }
}

public sealed class NotificationQueueHealthCheck : IHealthCheck
{
    private readonly Compliance360DbContext _dbContext;

    public NotificationQueueHealthCheck(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var queued = await _dbContext.NotificationMessages.CountAsync(
            message => message.Status == NotificationStatus.Queued
                || message.Status == NotificationStatus.Retried
                || (message.Status == NotificationStatus.Processing && message.LeaseUntilUtc < nowUtc),
            cancellationToken);
        return queued < 10_000
            ? HealthCheckResult.Healthy($"Notification queue depth is {queued}.")
            : HealthCheckResult.Degraded($"Notification queue depth is high: {queued}.");
    }
}

public sealed class AlertCenterWorkerHealthCheck : IHealthCheck
{
    private readonly Compliance360DbContext _dbContext;
    private readonly IOptions<NotificationWorkerOptions> _options;

    public AlertCenterWorkerHealthCheck(Compliance360DbContext dbContext, IOptions<NotificationWorkerOptions> options)
    {
        _dbContext = dbContext;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!_options.Value.Enabled)
        {
            return HealthCheckResult.Degraded("Alert Center worker is disabled by configuration.");
        }

        var newestHeartbeat = await _dbContext.NotificationWorkerHeartbeats
            .AsNoTracking()
            .OrderByDescending(heartbeat => heartbeat.LastSeenAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        if (newestHeartbeat is null)
        {
            return HealthCheckResult.Degraded("No Alert Center worker heartbeat has been recorded.");
        }

        var maximumAge = TimeSpan.FromSeconds(Math.Clamp(_options.Value.LeaseDurationSeconds * 2, 30, 1_800));
        var age = DateTimeOffset.UtcNow - newestHeartbeat.LastSeenAtUtc;
        if (age > maximumAge || newestHeartbeat.Status == "Stopped")
        {
            return HealthCheckResult.Unhealthy(
                $"Alert Center worker heartbeat is stale ({Math.Round(age.TotalSeconds)} seconds) or stopped.");
        }

        return newestHeartbeat.Status == "Healthy"
            ? HealthCheckResult.Healthy($"Alert Center worker is healthy; last heartbeat {Math.Round(age.TotalSeconds)} seconds ago.")
            : HealthCheckResult.Degraded($"Alert Center worker status is {newestHeartbeat.Status}; last heartbeat {Math.Round(age.TotalSeconds)} seconds ago.");
    }
}

public sealed class NotificationDeadLetterHealthCheck : IHealthCheck
{
    private readonly Compliance360DbContext _dbContext;

    public NotificationDeadLetterHealthCheck(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var deadLetters = await _dbContext.NotificationDeadLetters.CountAsync(cancellationToken);
        return deadLetters < 1_000
            ? HealthCheckResult.Healthy($"Notification dead letter count is {deadLetters}.")
            : HealthCheckResult.Degraded($"Notification dead letter count is high: {deadLetters}.");
    }
}

public sealed class DataProtectionHealthCheck : IHealthCheck
{
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public DataProtectionHealthCheck(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtectionProvider = dataProtectionProvider;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var protector = _dataProtectionProvider.CreateProtector("Compliance360.HealthChecks.DataProtection");
        var payload = protector.Protect("ok");
        return Task.FromResult(protector.Unprotect(payload) == "ok"
            ? HealthCheckResult.Healthy("Data Protection can protect and unprotect payloads.")
            : HealthCheckResult.Unhealthy("Data Protection roundtrip failed."));
    }
}

public sealed class ReportingHealthCheck : IHealthCheck
{
    private readonly IReportingEngineService _reportingEngineService;

    public ReportingHealthCheck(IReportingEngineService reportingEngineService)
    {
        _reportingEngineService = reportingEngineService;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(_reportingEngineService is not null
            ? HealthCheckResult.Healthy("Reporting engine service is registered.")
            : HealthCheckResult.Unhealthy("Reporting engine service is unavailable."));
}

public sealed class WorkflowHealthCheck : IHealthCheck
{
    private readonly IWorkflowEngineService _workflowEngineService;

    public WorkflowHealthCheck(IWorkflowEngineService workflowEngineService)
    {
        _workflowEngineService = workflowEngineService;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(_workflowEngineService is not null
            ? HealthCheckResult.Healthy("Workflow engine service is registered.")
            : HealthCheckResult.Unhealthy("Workflow engine service is unavailable."));
}
