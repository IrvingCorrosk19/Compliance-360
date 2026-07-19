using Compliance360.Domain.Common;

namespace Compliance360.Domain.Notifications;

public enum NotificationInboxState
{
    Unread = 0,
    Read = 1,
    Archived = 2,
    Deleted = 3
}

public sealed class NotificationInboxItem : TenantEntity
{
    private NotificationInboxItem()
    {
    }

    public NotificationInboxItem(
        Guid tenantId,
        Guid notificationMessageId,
        Guid userId,
        DateTimeOffset receivedAtUtc)
        : base(tenantId)
    {
        NotificationMessageId = Guard.AgainstEmpty(notificationMessageId, nameof(notificationMessageId));
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        ReceivedAtUtc = receivedAtUtc;
        SortAtUtc = receivedAtUtc;
        State = NotificationInboxState.Unread;
    }

    public Guid NotificationMessageId { get; private set; }

    public Guid UserId { get; private set; }

    public NotificationInboxState State { get; private set; }

    public bool IsFavorite { get; private set; }

    public DateTimeOffset ReceivedAtUtc { get; private set; }

    public DateTimeOffset SortAtUtc { get; private set; }

    public DateTimeOffset? ReadAtUtc { get; private set; }

    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public void MarkRead(DateTimeOffset nowUtc)
    {
        if (State == NotificationInboxState.Deleted)
        {
            throw new DomainException("Deleted inbox items must be restored before they can be read.");
        }

        State = NotificationInboxState.Read;
        ReadAtUtc ??= nowUtc;
        ArchivedAtUtc = null;
        MarkUpdated(nowUtc);
    }

    public void MarkUnread(DateTimeOffset nowUtc)
    {
        if (State == NotificationInboxState.Deleted)
        {
            throw new DomainException("Deleted inbox items must be restored before they can be unread.");
        }

        State = NotificationInboxState.Unread;
        ReadAtUtc = null;
        ArchivedAtUtc = null;
        SortAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }

    public void Archive(DateTimeOffset nowUtc)
    {
        if (State == NotificationInboxState.Deleted)
        {
            throw new DomainException("Deleted inbox items cannot be archived.");
        }

        State = NotificationInboxState.Archived;
        ReadAtUtc ??= nowUtc;
        ArchivedAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }

    public void Delete(DateTimeOffset nowUtc)
    {
        State = NotificationInboxState.Deleted;
        DeletedAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }

    public void Restore(DateTimeOffset nowUtc)
    {
        if (State != NotificationInboxState.Deleted)
        {
            throw new DomainException("Only deleted inbox items can be restored.");
        }

        State = ReadAtUtc.HasValue ? NotificationInboxState.Read : NotificationInboxState.Unread;
        DeletedAtUtc = null;
        SortAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }

    public void SetFavorite(bool favorite, DateTimeOffset nowUtc)
    {
        if (State == NotificationInboxState.Deleted)
        {
            throw new DomainException("Deleted inbox items cannot be favorited.");
        }

        IsFavorite = favorite;
        MarkUpdated(nowUtc);
    }
}
