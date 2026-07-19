using System.Diagnostics;
using System.Diagnostics.Metrics;
using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Compliance360.Infrastructure.Notifications;

public sealed class NotificationQueueProcessor : INotificationQueueProcessor
{
    private const string QueuedEventType = "notification.message.queued";
    private static readonly ActivitySource ActivitySource = new("Compliance360.AlertCenter.Worker");
    private static readonly Meter Meter = new("Compliance360.AlertCenter.Worker", "1.0.0");
    private static readonly Counter<long> ProcessedCounter = Meter.CreateCounter<long>("alert_center_messages_processed_total");
    private static readonly Counter<long> FailedCounter = Meter.CreateCounter<long>("alert_center_messages_failed_total");
    private static readonly Counter<long> RetryCounter = Meter.CreateCounter<long>("alert_center_messages_retried_total");
    private static readonly Counter<long> DeadLetterCounter = Meter.CreateCounter<long>("alert_center_messages_dead_lettered_total");
    private static readonly Histogram<double> ProcessingDuration = Meter.CreateHistogram<double>("alert_center_message_processing_duration_ms", "ms");

    private readonly INotificationQueueRepository _repository;
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationRetryService _retryService;
    private readonly INotificationAuditService _auditService;
    private readonly IClock _clock;
    private readonly NotificationWorkerOptions _options;
    private readonly ILogger<NotificationQueueProcessor> _logger;
    private readonly INotificationInboxRepository? _inboxRepository;
    private readonly IAlertEventProcessor? _alertEventProcessor;
    private readonly IAlertEventRepository? _alertEventRepository;
    private long _processedCount;
    private long _failureCount;

    public NotificationQueueProcessor(
        INotificationQueueRepository repository,
        INotificationDispatcher dispatcher,
        INotificationRetryService retryService,
        INotificationAuditService auditService,
        IClock clock,
        IOptions<NotificationWorkerOptions> options,
        ILogger<NotificationQueueProcessor> logger,
        INotificationInboxRepository? inboxRepository = null,
        IAlertEventProcessor? alertEventProcessor = null,
        IAlertEventRepository? alertEventRepository = null)
    {
        _repository = repository;
        _dispatcher = dispatcher;
        _retryService = retryService;
        _auditService = auditService;
        _clock = clock;
        _options = options.Value;
        _logger = logger;
        _inboxRepository = inboxRepository;
        _alertEventProcessor = alertEventProcessor;
        _alertEventRepository = alertEventRepository;
    }

    public async Task<NotificationQueueBatchResult> ProcessBatchAsync(
        string workerId,
        string instanceName,
        CancellationToken cancellationToken = default)
    {
        var leaseDuration = TimeSpan.FromSeconds(Math.Clamp(_options.LeaseDurationSeconds, 10, 900));
        var outboxPublished = await PublishOutboxAsync(workerId, leaseDuration, cancellationToken);
        var messages = await _repository.ClaimMessagesAsync(
            workerId,
            Math.Clamp(_options.BatchSize, 1, 500),
            _clock.UtcNow,
            leaseDuration,
            cancellationToken);

        var succeeded = 0;
        var retried = 0;
        var deadLettered = 0;
        var failed = 0;
        string? lastError = null;

        foreach (var message in messages)
        {
            var started = Stopwatch.GetTimestamp();
            using var activity = ActivitySource.StartActivity("notification.process", ActivityKind.Consumer);
            activity?.SetTag("messaging.message.id", message.Id);
            activity?.SetTag("tenant.id", message.TenantId);
            activity?.SetTag("notification.channel", message.Channel.ToString());
            activity?.SetTag("notification.attempt", message.RetryCount + 1);

            try
            {
                var outcome = await ProcessMessageAsync(message, cancellationToken);
                if (message.AlertOccurrenceId.HasValue && _alertEventRepository is not null)
                {
                    await _alertEventRepository.CompleteOccurrenceIfTerminalAsync(
                        message.TenantId,
                        message.AlertOccurrenceId.Value,
                        _clock.UtcNow,
                        cancellationToken);
                }
                switch (outcome)
                {
                    case NotificationStatus.Delivered:
                    case NotificationStatus.Sent:
                        succeeded++;
                        break;
                    case NotificationStatus.Retried:
                        retried++;
                        break;
                    case NotificationStatus.DeadLetter:
                        deadLettered++;
                        break;
                    default:
                        failed++;
                        break;
                }

                Interlocked.Increment(ref _processedCount);
                ProcessedCounter.Add(1, new KeyValuePair<string, object?>("channel", message.Channel.ToString()));
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                failed++;
                lastError = exception.Message;
                Interlocked.Increment(ref _failureCount);
                FailedCounter.Add(1, new KeyValuePair<string, object?>("channel", message.Channel.ToString()));
                activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                _logger.LogError(
                    exception,
                    "Alert Center worker failed processing message {MessageId} for tenant {TenantId}; lease {LeaseToken} will be recovered after expiry.",
                    message.Id,
                    message.TenantId,
                    message.LeaseToken);
            }
            finally
            {
                ProcessingDuration.Record(
                    Stopwatch.GetElapsedTime(started).TotalMilliseconds,
                    new KeyValuePair<string, object?>("channel", message.Channel.ToString()));
            }
        }

        await _repository.UpdateHeartbeatAsync(
            workerId,
            instanceName,
            0,
            Interlocked.Read(ref _processedCount),
            Interlocked.Read(ref _failureCount),
            lastError,
            stopping: false,
            cancellationToken);

        return new NotificationQueueBatchResult(
            outboxPublished,
            messages.Count,
            succeeded,
            retried,
            deadLettered,
            failed);
    }

    public Task MarkStoppedAsync(string workerId, string instanceName, CancellationToken cancellationToken = default)
    {
        return _repository.UpdateHeartbeatAsync(
            workerId,
            instanceName,
            0,
            Interlocked.Read(ref _processedCount),
            Interlocked.Read(ref _failureCount),
            null,
            stopping: true,
            cancellationToken);
    }

    private async Task<int> PublishOutboxAsync(
        string workerId,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        var events = await _repository.ClaimOutboxAsync(
            workerId,
            Math.Clamp(_options.OutboxBatchSize, 1, 500),
            _clock.UtcNow,
            leaseDuration,
            cancellationToken);
        var published = 0;

        foreach (var outboxEvent in events)
        {
            var leaseToken = outboxEvent.LeaseToken!;
            if (string.Equals(outboxEvent.EventType, "AlertOccurrenceCreated", StringComparison.Ordinal)
                && _alertEventProcessor is not null)
            {
                var result = await _alertEventProcessor.ProcessAsync(outboxEvent, cancellationToken);
                if (result.IsFailure)
                {
                    outboxEvent.MarkFailed(leaseToken, result.Error ?? "Alert occurrence processing failed.", _clock.UtcNow.AddMinutes(1));
                    continue;
                }
            }
            else if (!string.Equals(outboxEvent.EventType, QueuedEventType, StringComparison.Ordinal))
            {
                outboxEvent.MarkFailed(leaseToken, $"No handler registered for event type '{outboxEvent.EventType}'.", _clock.UtcNow.AddMinutes(1));
                continue;
            }

            outboxEvent.MarkPublished(leaseToken, _clock.UtcNow);
            published++;
        }

        if (events.Count > 0)
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return published;
    }

    private async Task<NotificationStatus> ProcessMessageAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        var nowUtc = _clock.UtcNow;
        var leaseToken = message.LeaseToken!;

        if (message.Channel == NotificationChannel.InApp)
        {
            if (!message.TargetUserId.HasValue)
            {
                throw new InvalidOperationException($"In-app notification {message.Id} does not have a target user.");
            }

            if (_inboxRepository is not null)
            {
                await _inboxRepository.AddIfMissingAsync(
                    new NotificationInboxItem(message.TenantId, message.Id, message.TargetUserId.Value, nowUtc),
                    cancellationToken);
            }

            message.MarkSent(nowUtc, leaseToken);
            message.MarkDelivered(nowUtc);
            await _repository.AddDeliveryAsync(
                new NotificationDelivery(message.TenantId, message.Id, NotificationProvider.Internal, NotificationDeliveryStatus.Delivered, message.Id.ToString("N"), nowUtc),
                cancellationToken);
            await _repository.AddHistoryAsync(
                new NotificationHistory(message.TenantId, message.Id, NotificationDeliveryStatus.Delivered, "InAppDelivered", nowUtc),
                cancellationToken);
            await _auditService.AppendAsync(message.TenantId, Guid.Empty, nameof(NotificationMessage), message.Id, AuditAction.NotificationDelivered, true, null, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return message.Status;
        }

        var dispatchResult = await _dispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                message.TenantId,
                message.Id,
                message.Channel,
                message.Recipient,
                message.Subject,
                message.Body,
                message.TextBody,
                message.Priority),
            cancellationToken);

        if (dispatchResult.Success)
        {
            message.MarkSent(nowUtc, leaseToken);
            await _repository.AddDeliveryAsync(
                new NotificationDelivery(
                    message.TenantId,
                    message.Id,
                    dispatchResult.Provider ?? NotificationProvider.Smtp,
                    NotificationDeliveryStatus.Sent,
                    dispatchResult.ProviderMessageId,
                    nowUtc),
                cancellationToken);
            await _repository.AddHistoryAsync(
                new NotificationHistory(message.TenantId, message.Id, NotificationDeliveryStatus.Sent, "SentByWorker", nowUtc),
                cancellationToken);
            await _auditService.AppendAsync(message.TenantId, Guid.Empty, nameof(NotificationMessage), message.Id, AuditAction.NotificationSent, true, null, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return message.Status;
        }

        var reason = dispatchResult.FailureReason ?? "Notification dispatch failed.";
        message.MarkFailed(reason, nowUtc, dispatchResult.Provider, leaseToken);
        await _repository.AddDeliveryAsync(
            new NotificationDelivery(
                message.TenantId,
                message.Id,
                dispatchResult.Provider ?? NotificationProvider.Smtp,
                NotificationDeliveryStatus.Failed,
                null,
                nowUtc),
            cancellationToken);

        if (message.RetryCount + 1 >= message.MaxAttempts)
        {
            message.MoveToDeadLetter(reason, nowUtc);
            await _repository.AddDeadLetterAsync(
                new NotificationDeadLetter(message.TenantId, message.Id, reason, $"{{\"messageId\":\"{message.Id}\"}}", nowUtc),
                cancellationToken);
            await _repository.AddHistoryAsync(
                new NotificationHistory(message.TenantId, message.Id, NotificationDeliveryStatus.DeadLetter, "DeadLetteredByWorker", nowUtc),
                cancellationToken);
            await _auditService.AppendAsync(message.TenantId, Guid.Empty, nameof(NotificationMessage), message.Id, AuditAction.NotificationDeadLettered, false, reason, cancellationToken);
            DeadLetterCounter.Add(1);
        }
        else
        {
            var nextRetryAt = _retryService.GetNextRetryAt(nowUtc, message.RetryCount);
            message.MarkRetried(nextRetryAt);
            await _repository.AddRetryAsync(
                new NotificationRetry(message.TenantId, message.Id, message.RetryCount, nextRetryAt, reason),
                cancellationToken);
            await _repository.AddHistoryAsync(
                new NotificationHistory(message.TenantId, message.Id, NotificationDeliveryStatus.Retried, "RetryScheduledByWorker", nowUtc),
                cancellationToken);
            await _auditService.AppendAsync(message.TenantId, Guid.Empty, nameof(NotificationMessage), message.Id, AuditAction.NotificationRetried, false, reason, cancellationToken);
            RetryCounter.Add(1);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return message.Status;
    }
}
