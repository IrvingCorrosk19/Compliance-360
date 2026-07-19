using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Notifications;

public sealed class EfNotificationInboxRepository : INotificationInboxRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfNotificationInboxRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddIfMissingAsync(NotificationInboxItem inboxItem, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.NotificationInboxItems.AnyAsync(
            item => item.TenantId == inboxItem.TenantId
                && item.NotificationMessageId == inboxItem.NotificationMessageId
                && item.UserId == inboxItem.UserId,
            cancellationToken);
        if (!exists)
        {
            await _dbContext.NotificationInboxItems.AddAsync(inboxItem, cancellationToken);
        }
    }

    public Task<NotificationInboxItem?> GetAsync(Guid tenantId, Guid userId, Guid inboxItemId, CancellationToken cancellationToken = default)
    {
        return _dbContext.NotificationInboxItems.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.UserId == userId && item.Id == inboxItemId,
            cancellationToken);
    }

    public async Task<(IReadOnlyCollection<NotificationInboxItemRecord> Items, long Total)> SearchAsync(NotificationInboxQuery query, CancellationToken cancellationToken = default)
    {
        var source =
            from inboxItem in _dbContext.NotificationInboxItems.AsNoTracking()
            join message in _dbContext.NotificationMessages.AsNoTracking()
                on new { inboxItem.TenantId, MessageId = inboxItem.NotificationMessageId }
                equals new { message.TenantId, MessageId = message.Id }
            where inboxItem.TenantId == query.TenantId && inboxItem.UserId == query.UserId
            select new { InboxItem = inboxItem, Message = message };

        if (query.State.HasValue)
        {
            source = source.Where(row => row.InboxItem.State == query.State.Value);
        }
        else
        {
            source = source.Where(row => row.InboxItem.State != NotificationInboxState.Deleted);
        }

        if (query.Favorite.HasValue)
        {
            source = source.Where(row => row.InboxItem.IsFavorite == query.Favorite.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{EscapeLikePattern(query.Search)}%";
            source = source.Where(row =>
                EF.Functions.ILike(row.Message.Subject, pattern, "\\")
                || EF.Functions.ILike(row.Message.Body, pattern, "\\"));
        }

        var total = await source.LongCountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(row => row.InboxItem.SortAtUtc)
            .ThenByDescending(row => row.InboxItem.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(row => new NotificationInboxItemRecord(
                row.InboxItem.Id,
                row.InboxItem.TenantId,
                row.InboxItem.UserId,
                row.InboxItem.NotificationMessageId,
                row.InboxItem.State,
                row.InboxItem.IsFavorite,
                row.Message.Channel,
                row.Message.Priority,
                row.Message.Subject,
                row.Message.Body,
                row.InboxItem.ReceivedAtUtc,
                row.InboxItem.SortAtUtc,
                row.InboxItem.ReadAtUtc))
            .ToArrayAsync(cancellationToken);

        return (items, total);
    }

    public async Task<NotificationInboxCounts> GetCountsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var source = _dbContext.NotificationInboxItems.AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.UserId == userId);
        var unread = await source.LongCountAsync(item => item.State == NotificationInboxState.Unread, cancellationToken);
        var read = await source.LongCountAsync(item => item.State == NotificationInboxState.Read, cancellationToken);
        var archived = await source.LongCountAsync(item => item.State == NotificationInboxState.Archived, cancellationToken);
        var favorites = await source.LongCountAsync(item => item.IsFavorite && item.State != NotificationInboxState.Deleted, cancellationToken);
        var totalActive = await source.LongCountAsync(item => item.State != NotificationInboxState.Deleted, cancellationToken);
        return new NotificationInboxCounts(unread, read, archived, favorites, totalActive);
    }

    public async Task<IReadOnlyCollection<NotificationInboxItem>> GetManyAsync(
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<Guid> inboxItemIds,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationInboxItems
            .Where(item => item.TenantId == tenantId && item.UserId == userId && inboxItemIds.Contains(item.Id))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificationInboxItem>> GetAllActiveAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationInboxItems
            .Where(item => item.TenantId == tenantId && item.UserId == userId && item.State != NotificationInboxState.Deleted)
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string EscapeLikePattern(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
