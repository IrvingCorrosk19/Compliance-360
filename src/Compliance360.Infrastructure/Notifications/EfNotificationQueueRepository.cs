using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Notifications;

public sealed class EfNotificationQueueRepository : INotificationQueueRepository, INotificationOutboxWriter
{
    private readonly Compliance360DbContext _dbContext;
    private readonly IClock _clock;

    public EfNotificationQueueRepository(Compliance360DbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task AddAsync(NotificationOutboxEvent outboxEvent, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationOutbox.AddAsync(outboxEvent, cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificationOutboxEvent>> ClaimOutboxAsync(
        string workerId,
        int batchSize,
        DateTimeOffset nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var events = await _dbContext.NotificationOutbox
            .FromSqlInterpolated($"""
                SELECT *
                FROM compliance360.notification_outbox
                WHERE "AvailableAtUtc" <= {nowUtc}
                  AND (
                    "Status" IN ('Pending', 'Failed')
                    OR ("Status" = 'Processing' AND "LeaseUntilUtc" < {nowUtc})
                  )
                ORDER BY "AvailableAtUtc", "OccurredAtUtc", "Id"
                FOR UPDATE SKIP LOCKED
                LIMIT {Math.Clamp(batchSize, 1, 500)}
                """)
            .ToArrayAsync(cancellationToken);

        foreach (var outboxEvent in events)
        {
            outboxEvent.AcquireLease(Guid.NewGuid().ToString("N"), workerId, nowUtc, nowUtc.Add(leaseDuration));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return events;
    }

    public async Task<IReadOnlyCollection<NotificationMessage>> ClaimMessagesAsync(
        string workerId,
        int batchSize,
        DateTimeOffset nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var messages = await _dbContext.NotificationMessages
            .FromSqlInterpolated($"""
                SELECT *
                FROM compliance360.notification_messages
                WHERE "AvailableAtUtc" <= {nowUtc}
                  AND ("NextRetryAtUtc" IS NULL OR "NextRetryAtUtc" <= {nowUtc})
                  AND (
                    "Status" IN ('Queued', 'Retried')
                    OR ("Status" = 'Processing' AND "LeaseUntilUtc" < {nowUtc})
                  )
                ORDER BY
                  CASE "Priority"
                    WHEN 'Critical' THEN 4
                    WHEN 'High' THEN 3
                    WHEN 'Normal' THEN 2
                    ELSE 1
                  END DESC,
                  "AvailableAtUtc",
                  "QueuedAtUtc",
                  "Id"
                FOR UPDATE SKIP LOCKED
                LIMIT {Math.Clamp(batchSize, 1, 500)}
                """)
            .ToArrayAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.AcquireLease(Guid.NewGuid().ToString("N"), workerId, nowUtc, nowUtc.Add(leaseDuration));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return messages;
    }

    public async Task AddDeliveryAsync(NotificationDelivery delivery, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationDeliveries.AddAsync(delivery, cancellationToken);
    }

    public async Task AddRetryAsync(NotificationRetry retry, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationRetries.AddAsync(retry, cancellationToken);
    }

    public async Task AddHistoryAsync(NotificationHistory history, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationHistory.AddAsync(history, cancellationToken);
    }

    public async Task AddDeadLetterAsync(NotificationDeadLetter deadLetter, CancellationToken cancellationToken = default)
    {
        await _dbContext.NotificationDeadLetters.AddAsync(deadLetter, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateHeartbeatAsync(
        string workerId,
        string instanceName,
        int activeLeases,
        long processedCount,
        long failureCount,
        string? lastError,
        bool stopping,
        CancellationToken cancellationToken = default)
    {
        var heartbeat = await _dbContext.NotificationWorkerHeartbeats
            .SingleOrDefaultAsync(item => item.WorkerId == workerId, cancellationToken);
        if (heartbeat is null)
        {
            heartbeat = new NotificationWorkerHeartbeat(workerId, instanceName, _clock.UtcNow);
            await _dbContext.NotificationWorkerHeartbeats.AddAsync(heartbeat, cancellationToken);
        }

        if (stopping)
        {
            heartbeat.Stop(_clock.UtcNow);
        }
        else
        {
            heartbeat.Beat(_clock.UtcNow, activeLeases, processedCount, failureCount, lastError);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
