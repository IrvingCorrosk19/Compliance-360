using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Notifications;

public sealed class EfAlertOperationsRepository : IAlertOperationsRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfAlertOperationsRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AlertOperationsDashboard> GetDashboardAsync(
        Guid tenantId,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        var counts = await _dbContext.NotificationMessages
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .GroupBy(item => item.Status)
            .Select(group => new { Status = group.Key, Count = group.LongCount() })
            .ToDictionaryAsync(item => item.Status, item => item.Count, cancellationToken);
        var oldest = await _dbContext.NotificationMessages
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId
                && (item.Status == NotificationStatus.Queued || item.Status == NotificationStatus.Retried || item.Status == NotificationStatus.Processing))
            .MinAsync(item => (DateTimeOffset?)item.QueuedAtUtc, cancellationToken);
        var latencyRows = await _dbContext.NotificationMessages
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.CompletedAtUtc.HasValue)
            .OrderByDescending(item => item.CompletedAtUtc)
            .Select(item => new { item.QueuedAtUtc, item.CompletedAtUtc })
            .Take(10_000)
            .ToArrayAsync(cancellationToken);
        var throughputHour = await _dbContext.NotificationMessages.LongCountAsync(
            item => item.TenantId == tenantId && item.CompletedAtUtc >= nowUtc.AddHours(-1),
            cancellationToken);
        var throughputDay = await _dbContext.NotificationMessages.LongCountAsync(
            item => item.TenantId == tenantId && item.CompletedAtUtc >= nowUtc.AddHours(-24),
            cancellationToken);
        var activeThreshold = nowUtc.AddMinutes(-2);
        var workers = await _dbContext.NotificationWorkerHeartbeats
            .AsNoTracking()
            .Where(item => item.LastSeenAtUtc >= activeThreshold && item.Status != "Stopped")
            .ToArrayAsync(cancellationToken);

        return new AlertOperationsDashboard(
            Count(NotificationStatus.Queued),
            Count(NotificationStatus.Processing),
            Count(NotificationStatus.Sent),
            Count(NotificationStatus.Delivered),
            Count(NotificationStatus.Failed),
            Count(NotificationStatus.Retried),
            Count(NotificationStatus.Cancelled),
            Count(NotificationStatus.DeadLetter),
            oldest.HasValue ? Math.Max(0, (nowUtc - oldest.Value).TotalMinutes) : 0,
            latencyRows.Length == 0 ? 0 : latencyRows.Average(item => (item.CompletedAtUtc!.Value - item.QueuedAtUtc).TotalMilliseconds),
            throughputHour,
            throughputDay,
            workers.Length,
            workers.Length == 0 ? null : workers.Max(item => item.LastSeenAtUtc));

        long Count(NotificationStatus status) => counts.GetValueOrDefault(status);
    }

    public async Task<(IReadOnlyCollection<NotificationMessage> Items, int Total)> SearchAsync(
        AlertOperationsQuery query,
        CancellationToken cancellationToken)
    {
        var source = _dbContext.NotificationMessages.AsNoTracking().Where(item => item.TenantId == query.TenantId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(item =>
                EF.Functions.ILike(item.Subject, pattern)
                || EF.Functions.ILike(item.Recipient, pattern)
                || EF.Functions.ILike(item.IdempotencyKey, pattern)
                || (item.FailureReason != null && EF.Functions.ILike(item.FailureReason, pattern)));
        }

        if (query.Status.HasValue) source = source.Where(item => item.Status == query.Status.Value);
        if (query.Channel.HasValue) source = source.Where(item => item.Channel == query.Channel.Value);
        if (query.Provider.HasValue) source = source.Where(item => item.LastProvider == query.Provider.Value);
        if (query.AlertDefinitionId.HasValue) source = source.Where(item => item.AlertDefinitionId == query.AlertDefinitionId.Value);
        if (query.AlertOccurrenceId.HasValue) source = source.Where(item => item.AlertOccurrenceId == query.AlertOccurrenceId.Value);
        if (query.FromUtc.HasValue) source = source.Where(item => item.QueuedAtUtc >= query.FromUtc.Value);
        if (query.ToUtc.HasValue) source = source.Where(item => item.QueuedAtUtc <= query.ToUtc.Value);

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(item => item.QueuedAtUtc)
            .Skip((Math.Max(1, query.Page) - 1) * Math.Clamp(query.PageSize, 1, 500))
            .Take(Math.Clamp(query.PageSize, 1, 500))
            .ToArrayAsync(cancellationToken);
        return (items, total);
    }

    public Task<NotificationMessage?> GetMessageAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken) =>
        _dbContext.NotificationMessages.AsNoTracking()
            .SingleOrDefaultAsync(item => item.TenantId == tenantId && item.Id == messageId, cancellationToken);

    public async Task<IReadOnlyCollection<NotificationDelivery>> ListDeliveriesAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken) =>
        await _dbContext.NotificationDeliveries.AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.NotificationMessageId == messageId)
            .OrderBy(item => item.OccurredAtUtc)
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<NotificationRetry>> ListRetriesAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken) =>
        await _dbContext.NotificationRetries.AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.NotificationMessageId == messageId)
            .OrderBy(item => item.Attempt)
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<NotificationHistory>> ListHistoryAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken) =>
        await _dbContext.NotificationHistory.AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.NotificationMessageId == messageId)
            .OrderBy(item => item.OccurredAtUtc)
            .ToArrayAsync(cancellationToken);

    public Task<NotificationDeadLetter?> GetDeadLetterAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken) =>
        _dbContext.NotificationDeadLetters.AsNoTracking()
            .OrderByDescending(item => item.DeadLetteredAtUtc)
            .FirstOrDefaultAsync(item => item.TenantId == tenantId && item.NotificationMessageId == messageId, cancellationToken);
}
