using Compliance360.Application.Notifications;
using Microsoft.Extensions.Options;

namespace Compliance360.Worker;

public sealed class AlertCenterWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NotificationWorkerOptions _options;
    private readonly ILogger<AlertCenterWorker> _logger;
    private readonly string _workerId = $"alert-center:{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";
    private readonly string _instanceName = Environment.MachineName;

    public AlertCenterWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<NotificationWorkerOptions> options,
        ILogger<AlertCenterWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("Alert Center worker {WorkerId} is disabled by configuration.", _workerId);
            return;
        }

        _logger.LogInformation(
            "Alert Center worker {WorkerId} started with batch size {BatchSize}, lease {LeaseSeconds}s and poll interval {PollMilliseconds}ms.",
            _workerId,
            _options.BatchSize,
            _options.LeaseDurationSeconds,
            _options.PollIntervalMilliseconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var scheduler = scope.ServiceProvider.GetRequiredService<IAlertSchedulerProcessor>();
                var schedulerResult = await scheduler.ProcessDueAsync(_workerId, stoppingToken);
                var processor = scope.ServiceProvider.GetRequiredService<INotificationQueueProcessor>();
                var result = await processor.ProcessBatchAsync(_workerId, _instanceName, stoppingToken);
                if (schedulerResult.SchedulesClaimed > 0)
                {
                    _logger.LogInformation(
                        "Alert Center scheduler batch completed: schedules={Schedules}, created={Created}, skipped={Skipped}, failed={Failed}.",
                        schedulerResult.SchedulesClaimed,
                        schedulerResult.ExecutionsCreated,
                        schedulerResult.ExecutionsSkipped,
                        schedulerResult.ExecutionsFailed);
                }
                if (result.MessagesClaimed > 0 || result.OutboxPublished > 0)
                {
                    _logger.LogInformation(
                        "Alert Center batch completed: outbox={OutboxPublished}, claimed={Claimed}, succeeded={Succeeded}, retried={Retried}, deadLetters={DeadLetters}, failed={Failed}.",
                        result.OutboxPublished,
                        result.MessagesClaimed,
                        result.MessagesSucceeded,
                        result.MessagesRetried,
                        result.MessagesDeadLettered,
                        result.MessagesFailed);
                }

                if (result.MessagesClaimed == 0 && result.OutboxPublished == 0 && schedulerResult.SchedulesClaimed == 0)
                {
                    await Task.Delay(Math.Clamp(_options.PollIntervalMilliseconds, 100, 60_000), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Alert Center worker {WorkerId} batch failed.", _workerId);
                await Task.Delay(Math.Clamp(_options.PollIntervalMilliseconds, 1_000, 60_000), stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<INotificationQueueProcessor>();
            await processor.MarkStoppedAsync(_workerId, _instanceName, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Alert Center worker {WorkerId} could not persist its stopped heartbeat.", _workerId);
        }

        _logger.LogInformation("Alert Center worker {WorkerId} stopped.", _workerId);
        await base.StopAsync(cancellationToken);
    }
}
