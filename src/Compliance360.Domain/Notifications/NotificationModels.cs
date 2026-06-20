using Compliance360.Domain.Common;

namespace Compliance360.Domain.Notifications;

public enum NotificationChannel
{
    Email = 0,
    InApp = 1,
    Sms = 2
}

public enum NotificationStatus
{
    Queued = 0,
    Sent = 1,
    Failed = 2,
    Cancelled = 3
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public sealed class NotificationTemplate : TenantEntity
{
    private NotificationTemplate()
    {
        Code = string.Empty;
        Subject = string.Empty;
        Body = string.Empty;
    }

    public NotificationTemplate(Guid tenantId, string code, NotificationChannel channel, string subject, string body)
        : base(tenantId)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 120).ToUpperInvariant();
        Channel = channel;
        Subject = Guard.AgainstNullOrWhiteSpace(subject, nameof(subject), 250);
        Body = Guard.AgainstNullOrWhiteSpace(body, nameof(body), 4_000);
        IsActive = true;
    }

    public string Code { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public string Subject { get; private set; }

    public string Body { get; private set; }

    public bool IsActive { get; private set; }

    public void UpdateContent(string subject, string body)
    {
        Subject = Guard.AgainstNullOrWhiteSpace(subject, nameof(subject), 250);
        Body = Guard.AgainstNullOrWhiteSpace(body, nameof(body), 4_000);
    }

    public void Disable()
    {
        IsActive = false;
    }
}

public sealed class NotificationMessage : TenantEntity
{
    private NotificationMessage()
    {
        Recipient = string.Empty;
        Subject = string.Empty;
        Body = string.Empty;
    }

    public NotificationMessage(
        Guid tenantId,
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        NotificationPriority priority,
        DateTimeOffset queuedAtUtc)
        : base(tenantId)
    {
        Channel = channel;
        Recipient = Guard.AgainstNullOrWhiteSpace(recipient, nameof(recipient), 320);
        Subject = Guard.AgainstNullOrWhiteSpace(subject, nameof(subject), 250);
        Body = Guard.AgainstNullOrWhiteSpace(body, nameof(body), 4_000);
        Priority = priority;
        QueuedAtUtc = queuedAtUtc;
        Status = NotificationStatus.Queued;
    }

    public Guid? TemplateId { get; private set; }

    public Guid? TargetUserId { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public string Recipient { get; private set; }

    public string Subject { get; private set; }

    public string Body { get; private set; }

    public NotificationPriority Priority { get; private set; }

    public NotificationStatus Status { get; private set; }

    public DateTimeOffset QueuedAtUtc { get; private set; }

    public DateTimeOffset? SentAtUtc { get; private set; }

    public DateTimeOffset? FailedAtUtc { get; private set; }

    public string? FailureReason { get; private set; }

    public void LinkTemplate(Guid templateId)
    {
        TemplateId = Guard.AgainstEmpty(templateId, nameof(templateId));
    }

    public void TargetUser(Guid targetUserId)
    {
        TargetUserId = Guard.AgainstEmpty(targetUserId, nameof(targetUserId));
    }

    public void MarkSent(DateTimeOffset sentAtUtc)
    {
        if (Status == NotificationStatus.Cancelled)
        {
            throw new DomainException("Cancelled notifications cannot be sent.");
        }

        Status = NotificationStatus.Sent;
        SentAtUtc = sentAtUtc;
        FailureReason = null;
    }

    public void MarkFailed(string reason, DateTimeOffset failedAtUtc)
    {
        Status = NotificationStatus.Failed;
        FailedAtUtc = failedAtUtc;
        FailureReason = Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 1_000);
    }

    public void Cancel()
    {
        if (Status == NotificationStatus.Sent)
        {
            throw new DomainException("Sent notifications cannot be cancelled.");
        }

        Status = NotificationStatus.Cancelled;
    }
}
