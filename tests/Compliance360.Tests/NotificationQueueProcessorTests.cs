using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class NotificationQueueProcessorTests
{
    [Fact]
    public async Task Processor_Delivers_Claimed_InApp_Message_Without_External_Provider()
    {
        var fixture = QueueFixture.Create();
        var message = fixture.AddMessage(NotificationChannel.InApp);

        var result = await fixture.Processor.ProcessBatchAsync("worker-1", "test");

        Assert.Equal(1, result.MessagesClaimed);
        Assert.Equal(1, result.MessagesSucceeded);
        Assert.Equal(NotificationStatus.Delivered, message.Status);
        Assert.Single(fixture.Repository.Deliveries);
        Assert.Equal(NotificationProvider.Internal, fixture.Repository.Deliveries[0].Provider);
        Assert.Contains(fixture.Repository.History, item => item.EventName == "InAppDelivered");
    }

    [Fact]
    public async Task Processor_Retries_Transient_Failure_And_DeadLetters_At_MaxAttempts()
    {
        var fixture = QueueFixture.Create(NotificationDispatchResult.Failed("provider unavailable"));
        var retryMessage = fixture.AddMessage(NotificationChannel.Email, maxAttempts: 3);
        var deadLetterMessage = fixture.AddMessage(NotificationChannel.Email, maxAttempts: 1);

        var result = await fixture.Processor.ProcessBatchAsync("worker-1", "test");

        Assert.Equal(2, result.MessagesClaimed);
        Assert.Equal(1, result.MessagesRetried);
        Assert.Equal(1, result.MessagesDeadLettered);
        Assert.Equal(NotificationStatus.Retried, retryMessage.Status);
        Assert.Equal(NotificationStatus.DeadLetter, deadLetterMessage.Status);
        Assert.Single(fixture.Repository.Retries);
        Assert.Single(fixture.Repository.DeadLetters);
    }

    [Fact]
    public async Task Processor_Publishes_Known_Outbox_Event_And_Rejects_Unknown_Handler()
    {
        var fixture = QueueFixture.Create();
        var known = fixture.AddOutbox("notification.message.queued");
        var unknown = fixture.AddOutbox("unsupported.event");

        var result = await fixture.Processor.ProcessBatchAsync("worker-1", "test");

        Assert.Equal(1, result.OutboxPublished);
        Assert.Equal(NotificationOutboxStatus.Published, known.Status);
        Assert.Equal(NotificationOutboxStatus.Failed, unknown.Status);
        Assert.Contains("No handler registered", unknown.LastError);
    }

    [Fact]
    public void Message_Lease_Prevents_Concurrent_Claim_And_Allows_Expired_Recovery()
    {
        var now = new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero);
        var message = new NotificationMessage(Guid.NewGuid(), NotificationChannel.Email, "qa@example.com", "Subject", "Body", NotificationPriority.Normal, now);
        message.AcquireLease("lease-1", "worker-1", now, now.AddMinutes(1));

        Assert.Throws<DomainException>(() => message.AcquireLease("lease-2", "worker-2", now.AddSeconds(30), now.AddMinutes(2)));

        message.AcquireLease("lease-2", "worker-2", now.AddMinutes(2), now.AddMinutes(3));
        message.MarkSent(now.AddMinutes(2), "lease-2");

        Assert.Equal(NotificationStatus.Sent, message.Status);
        Assert.Null(message.LeaseToken);
    }

    private sealed class QueueFixture
    {
        private QueueFixture(NotificationDispatchResult dispatchResult)
        {
            Clock = new MutableClock(new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero));
            Repository = new FakeQueueRepository(Clock);
            Processor = new NotificationQueueProcessor(
                Repository,
                new FixedDispatcher(dispatchResult),
                new NotificationRetryService(Options.Create(new NotificationProviderOptions
                {
                    MaxRetryCount = 3,
                    RetryBaseDelaySeconds = 1
                })),
                new NoOpAuditService(),
                Clock,
                Options.Create(new NotificationWorkerOptions
                {
                    BatchSize = 25,
                    OutboxBatchSize = 25,
                    LeaseDurationSeconds = 60
                }),
                NullLogger<NotificationQueueProcessor>.Instance);
        }

        public MutableClock Clock { get; }

        public FakeQueueRepository Repository { get; }

        public NotificationQueueProcessor Processor { get; }

        public static QueueFixture Create(NotificationDispatchResult? dispatchResult = null)
        {
            return new QueueFixture(dispatchResult ?? NotificationDispatchResult.Sent(NotificationProvider.Smtp, "provider-1"));
        }

        public NotificationMessage AddMessage(NotificationChannel channel, int maxAttempts = 3)
        {
            var message = new NotificationMessage(
                Guid.NewGuid(),
                channel,
                channel == NotificationChannel.InApp ? "user" : "qa@example.com",
                "Subject",
                "Body",
                NotificationPriority.Normal,
                Clock.UtcNow);
            if (channel == NotificationChannel.InApp)
            {
                message.TargetUser(Guid.NewGuid());
            }
            message.ConfigureDurability(null, maxAttempts);
            Repository.Messages.Add(message);
            return message;
        }

        public NotificationOutboxEvent AddOutbox(string eventType)
        {
            var outboxEvent = new NotificationOutboxEvent(
                Guid.NewGuid(),
                eventType,
                nameof(NotificationMessage),
                Guid.NewGuid(),
                "{}",
                Guid.NewGuid().ToString("N"),
                Clock.UtcNow);
            Repository.Outbox.Add(outboxEvent);
            return outboxEvent;
        }
    }

    private sealed class FakeQueueRepository : INotificationQueueRepository
    {
        private readonly IClock _clock;

        public FakeQueueRepository(IClock clock)
        {
            _clock = clock;
        }

        public List<NotificationOutboxEvent> Outbox { get; } = [];
        public List<NotificationMessage> Messages { get; } = [];
        public List<NotificationDelivery> Deliveries { get; } = [];
        public List<NotificationRetry> Retries { get; } = [];
        public List<NotificationHistory> History { get; } = [];
        public List<NotificationDeadLetter> DeadLetters { get; } = [];

        public Task<IReadOnlyCollection<NotificationOutboxEvent>> ClaimOutboxAsync(string workerId, int batchSize, DateTimeOffset nowUtc, TimeSpan leaseDuration, CancellationToken cancellationToken = default)
        {
            var claimed = Outbox
                .Where(item => item.Status is NotificationOutboxStatus.Pending or NotificationOutboxStatus.Failed)
                .Take(batchSize)
                .ToArray();
            foreach (var item in claimed)
            {
                item.AcquireLease(Guid.NewGuid().ToString("N"), workerId, nowUtc, nowUtc.Add(leaseDuration));
            }

            return Task.FromResult<IReadOnlyCollection<NotificationOutboxEvent>>(claimed);
        }

        public Task<IReadOnlyCollection<NotificationMessage>> ClaimMessagesAsync(string workerId, int batchSize, DateTimeOffset nowUtc, TimeSpan leaseDuration, CancellationToken cancellationToken = default)
        {
            var claimed = Messages
                .Where(item => item.Status is NotificationStatus.Queued or NotificationStatus.Retried)
                .Take(batchSize)
                .ToArray();
            foreach (var item in claimed)
            {
                item.AcquireLease(Guid.NewGuid().ToString("N"), workerId, nowUtc, nowUtc.Add(leaseDuration));
            }

            return Task.FromResult<IReadOnlyCollection<NotificationMessage>>(claimed);
        }

        public Task AddDeliveryAsync(NotificationDelivery delivery, CancellationToken cancellationToken = default) { Deliveries.Add(delivery); return Task.CompletedTask; }
        public Task AddRetryAsync(NotificationRetry retry, CancellationToken cancellationToken = default) { Retries.Add(retry); return Task.CompletedTask; }
        public Task AddHistoryAsync(NotificationHistory history, CancellationToken cancellationToken = default) { History.Add(history); return Task.CompletedTask; }
        public Task AddDeadLetterAsync(NotificationDeadLetter deadLetter, CancellationToken cancellationToken = default) { DeadLetters.Add(deadLetter); return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateHeartbeatAsync(string workerId, string instanceName, int activeLeases, long processedCount, long failureCount, string? lastError, bool stopping, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FixedDispatcher : INotificationDispatcher
    {
        private readonly NotificationDispatchResult _result;

        public FixedDispatcher(NotificationDispatchResult result)
        {
            _result = result;
        }

        public Task<NotificationDispatchResult> DispatchAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class NoOpAuditService : INotificationAuditService
    {
        public Task AppendAsync(Guid tenantId, Guid userId, string entityName, Guid entityId, AuditAction action, bool success, string? error, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class MutableClock : IClock
    {
        public MutableClock(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; set; }
    }
}
