using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Application.Reporting;
using Compliance360.Application.Storage;
using Compliance360.Application.Workflows;
using Compliance360.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
