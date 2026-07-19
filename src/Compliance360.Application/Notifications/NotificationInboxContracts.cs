using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public interface INotificationInboxService
{
    Task<Result<NotificationInboxPage>> SearchAsync(NotificationInboxQuery query, CancellationToken cancellationToken = default);

    Task<Result<NotificationInboxCounts>> GetCountsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<Result> ApplyActionAsync(NotificationInboxActionCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationInboxBulkResult>> ApplyBulkActionAsync(NotificationInboxBulkActionCommand command, CancellationToken cancellationToken = default);
}

public interface INotificationInboxRepository
{
    Task AddIfMissingAsync(NotificationInboxItem inboxItem, CancellationToken cancellationToken = default);

    Task<NotificationInboxItem?> GetAsync(Guid tenantId, Guid userId, Guid inboxItemId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyCollection<NotificationInboxItemRecord> Items, long Total)> SearchAsync(NotificationInboxQuery query, CancellationToken cancellationToken = default);

    Task<NotificationInboxCounts> GetCountsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<NotificationInboxItem>> GetManyAsync(Guid tenantId, Guid userId, IReadOnlyCollection<Guid> inboxItemIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<NotificationInboxItem>> GetAllActiveAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public enum NotificationInboxAction
{
    MarkRead = 0,
    MarkUnread = 1,
    Archive = 2,
    Delete = 3,
    Restore = 4,
    Favorite = 5,
    Unfavorite = 6
}

public sealed record NotificationInboxQuery(
    Guid TenantId,
    Guid UserId,
    NotificationInboxState? State,
    bool? Favorite,
    string? Search,
    int Page = 1,
    int PageSize = 25);

public sealed record NotificationInboxActionCommand(
    Guid TenantId,
    Guid UserId,
    Guid InboxItemId,
    NotificationInboxAction Action);

public sealed record NotificationInboxBulkActionCommand(
    Guid TenantId,
    Guid UserId,
    IReadOnlyCollection<Guid> InboxItemIds,
    NotificationInboxAction Action,
    bool All = false);

public sealed record NotificationInboxItemRecord(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    Guid NotificationMessageId,
    NotificationInboxState State,
    bool IsFavorite,
    NotificationChannel Channel,
    NotificationPriority Priority,
    string Subject,
    string Body,
    DateTimeOffset ReceivedAtUtc,
    DateTimeOffset SortAtUtc,
    DateTimeOffset? ReadAtUtc);

public sealed record NotificationInboxPage(
    IReadOnlyCollection<NotificationInboxItemRecord> Items,
    long Total,
    int Page,
    int PageSize);

public sealed record NotificationInboxCounts(long Unread, long Read, long Archived, long Favorites, long TotalActive);

public sealed record NotificationInboxBulkResult(int Requested, int Updated);
