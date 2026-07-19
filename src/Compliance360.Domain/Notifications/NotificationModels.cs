using Compliance360.Domain.Common;

namespace Compliance360.Domain.Notifications;

public enum NotificationChannel
{
    Email = 0,
    InApp = 1,
    Sms = 2,
    WhatsApp = 3,
    Push = 4
}

public enum NotificationStatus
{
    Queued = 0,
    Sent = 1,
    Failed = 2,
    Cancelled = 3,
    Delivered = 4,
    Retried = 5,
    DeadLetter = 6,
    Processing = 7
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public enum NotificationProvider
{
    Smtp = 0,
    SendGrid = 1,
    Mailgun = 2,
    Resend = 3,
    GmailSmtp = 4,
    Microsoft365 = 5,
    ExchangeOnline = 6,
    AmazonSes = 7,
    Internal = 8
}

public enum NotificationDeliveryStatus
{
    Queued = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3,
    Retried = 4,
    DeadLetter = 5,
    Cancelled = 6
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
        Body = Guard.AgainstNullOrWhiteSpace(body, nameof(body), 64_000);
        IsActive = true;
    }

    public string Code { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public string Subject { get; private set; }

    public string Body { get; private set; }

    public string? TextBody { get; private set; }

    public string? Locale { get; private set; }

    public int Version { get; private set; } = 1;

    public string? BrandingJson { get; private set; }

    public bool IsActive { get; private set; }

    public void UpdateContent(string subject, string body)
    {
        Subject = Guard.AgainstNullOrWhiteSpace(subject, nameof(subject), 250);
        Body = Guard.AgainstNullOrWhiteSpace(body, nameof(body), 64_000);
        Version++;
    }

    public void ConfigureEnterpriseContent(string? textBody, string? locale, string? brandingJson)
    {
        TextBody = string.IsNullOrWhiteSpace(textBody) ? null : Guard.AgainstNullOrWhiteSpace(textBody, nameof(textBody), 16_000);
        Locale = string.IsNullOrWhiteSpace(locale) ? null : Guard.AgainstNullOrWhiteSpace(locale, nameof(locale), 20);
        BrandingJson = string.IsNullOrWhiteSpace(brandingJson) ? null : Guard.AgainstNullOrWhiteSpace(brandingJson, nameof(brandingJson), 16_000);
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
        Body = Guard.AgainstNullOrWhiteSpace(body, nameof(body), 64_000);
        Priority = priority;
        QueuedAtUtc = queuedAtUtc;
        AvailableAtUtc = queuedAtUtc;
        IdempotencyKey = $"message:{Id:N}";
        MaxAttempts = 3;
        Status = NotificationStatus.Queued;
        Routing = RecipientRouting.To;
    }

    public Guid? TemplateId { get; private set; }

    public Guid? TargetUserId { get; private set; }

    public Guid? AlertOccurrenceId { get; private set; }

    public Guid? AlertDefinitionId { get; private set; }

    public Guid? AlertDefinitionVersionId { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public string Recipient { get; private set; }

    public RecipientRouting Routing { get; private set; }

    public string Subject { get; private set; }

    public string Body { get; private set; }

    public string? TextBody { get; private set; }

    public NotificationPriority Priority { get; private set; }

    public NotificationStatus Status { get; private set; }

    public DateTimeOffset QueuedAtUtc { get; private set; }

    public DateTimeOffset? SentAtUtc { get; private set; }

    public DateTimeOffset? FailedAtUtc { get; private set; }

    public DateTimeOffset? DeliveredAtUtc { get; private set; }

    public DateTimeOffset? NextRetryAtUtc { get; private set; }

    public string? FailureReason { get; private set; }

    public int RetryCount { get; private set; }

    public NotificationProvider? LastProvider { get; private set; }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public DateTimeOffset AvailableAtUtc { get; private set; }

    public int MaxAttempts { get; private set; }

    public string? LeaseToken { get; private set; }

    public string? LeaseOwner { get; private set; }

    public DateTimeOffset? LeaseUntilUtc { get; private set; }

    public DateTimeOffset? ProcessingStartedAtUtc { get; private set; }

    public DateTimeOffset? LastAttemptAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public void ConfigureDurability(string? idempotencyKey, int maxAttempts, DateTimeOffset? availableAtUtc = null)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            IdempotencyKey = Guard.AgainstNullOrWhiteSpace(idempotencyKey, nameof(idempotencyKey), 200);
        }

        MaxAttempts = Guard.AgainstOutOfRange(maxAttempts, nameof(maxAttempts), 1, 20);
        AvailableAtUtc = availableAtUtc ?? QueuedAtUtc;
    }

    public void AcquireLease(string leaseToken, string leaseOwner, DateTimeOffset nowUtc, DateTimeOffset leaseUntilUtc)
    {
        if (Status is not (NotificationStatus.Queued or NotificationStatus.Retried or NotificationStatus.Processing))
        {
            throw new DomainException("Notification message is not claimable.");
        }

        if (Status == NotificationStatus.Processing && LeaseUntilUtc.HasValue && LeaseUntilUtc > nowUtc)
        {
            throw new DomainException("Notification message already has an active lease.");
        }

        if (AvailableAtUtc > nowUtc || NextRetryAtUtc > nowUtc)
        {
            throw new DomainException("Notification message is not available yet.");
        }

        if (leaseUntilUtc <= nowUtc)
        {
            throw new DomainException("Lease expiration must be in the future.");
        }

        LeaseToken = Guard.AgainstNullOrWhiteSpace(leaseToken, nameof(leaseToken), 120);
        LeaseOwner = Guard.AgainstNullOrWhiteSpace(leaseOwner, nameof(leaseOwner), 160);
        LeaseUntilUtc = leaseUntilUtc;
        ProcessingStartedAtUtc = nowUtc;
        LastAttemptAtUtc = nowUtc;
        Status = NotificationStatus.Processing;
    }

    public void RenewLease(string leaseToken, DateTimeOffset nowUtc, DateTimeOffset leaseUntilUtc)
    {
        EnsureLease(leaseToken, nowUtc);
        if (leaseUntilUtc <= nowUtc)
        {
            throw new DomainException("Lease expiration must be in the future.");
        }

        LeaseUntilUtc = leaseUntilUtc;
    }

    public void LinkTemplate(Guid templateId)
    {
        TemplateId = Guard.AgainstEmpty(templateId, nameof(templateId));
    }

    public void TargetUser(Guid targetUserId)
    {
        TargetUserId = Guard.AgainstEmpty(targetUserId, nameof(targetUserId));
    }

    public void SetRouting(RecipientRouting routing)
    {
        Routing = routing;
    }

    public void LinkAlertContext(Guid occurrenceId, Guid definitionId, Guid definitionVersionId)
    {
        AlertOccurrenceId = Guard.AgainstEmpty(occurrenceId, nameof(occurrenceId));
        AlertDefinitionId = Guard.AgainstEmpty(definitionId, nameof(definitionId));
        AlertDefinitionVersionId = Guard.AgainstEmpty(definitionVersionId, nameof(definitionVersionId));
    }

    public void MarkSent(DateTimeOffset sentAtUtc, string? leaseToken = null)
    {
        if (Status == NotificationStatus.Cancelled)
        {
            throw new DomainException("Cancelled notifications cannot be sent.");
        }

        if (Status == NotificationStatus.Processing)
        {
            EnsureLease(leaseToken, sentAtUtc, allowExpired: true);
        }

        Status = NotificationStatus.Sent;
        SentAtUtc = sentAtUtc;
        CompletedAtUtc = sentAtUtc;
        FailureReason = null;
        ClearLease();
    }

    public void MarkDelivered(DateTimeOffset deliveredAtUtc)
    {
        if (Status != NotificationStatus.Sent)
        {
            throw new DomainException("Only sent notifications can be marked as delivered.");
        }

        Status = NotificationStatus.Delivered;
        DeliveredAtUtc = deliveredAtUtc;
        CompletedAtUtc = deliveredAtUtc;
    }

    public void MarkFailed(string reason, DateTimeOffset failedAtUtc, NotificationProvider? provider = null, string? leaseToken = null)
    {
        if (Status == NotificationStatus.Processing)
        {
            EnsureLease(leaseToken, failedAtUtc, allowExpired: true);
        }

        Status = NotificationStatus.Failed;
        FailedAtUtc = failedAtUtc;
        FailureReason = Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 1_000);
        LastProvider = provider;
        ClearLease();
    }

    public void MarkRetried(DateTimeOffset nextRetryAtUtc)
    {
        Status = NotificationStatus.Retried;
        RetryCount++;
        NextRetryAtUtc = nextRetryAtUtc;
        AvailableAtUtc = nextRetryAtUtc;
        CompletedAtUtc = null;
        ClearLease();
    }

    public void MoveToDeadLetter(string reason, DateTimeOffset deadLetteredAtUtc)
    {
        Status = NotificationStatus.DeadLetter;
        FailedAtUtc = deadLetteredAtUtc;
        FailureReason = Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 1_000);
        NextRetryAtUtc = null;
        CompletedAtUtc = deadLetteredAtUtc;
        ClearLease();
    }

    public void Cancel(DateTimeOffset cancelledAtUtc = default)
    {
        if (Status is NotificationStatus.Sent or NotificationStatus.Delivered)
        {
            throw new DomainException("Sent or delivered notifications cannot be cancelled.");
        }

        cancelledAtUtc = cancelledAtUtc == default ? DateTimeOffset.UtcNow : cancelledAtUtc;
        Status = NotificationStatus.Cancelled;
        CompletedAtUtc = cancelledAtUtc;
        ClearLease();
    }

    private void EnsureLease(string? leaseToken, DateTimeOffset nowUtc, bool allowExpired = false)
    {
        if (string.IsNullOrWhiteSpace(leaseToken) || !string.Equals(LeaseToken, leaseToken, StringComparison.Ordinal))
        {
            throw new DomainException("Notification lease token is invalid.");
        }

        if (!allowExpired && (!LeaseUntilUtc.HasValue || LeaseUntilUtc <= nowUtc))
        {
            throw new DomainException("Notification lease has expired.");
        }
    }

    private void ClearLease()
    {
        LeaseToken = null;
        LeaseOwner = null;
        LeaseUntilUtc = null;
    }
}

public sealed class NotificationDelivery : TenantEntity
{
    private NotificationDelivery()
    {
        ProviderMessageId = string.Empty;
    }

    public NotificationDelivery(Guid tenantId, Guid messageId, NotificationProvider provider, NotificationDeliveryStatus status, string? providerMessageId, DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        NotificationMessageId = Guard.AgainstEmpty(messageId, nameof(messageId));
        Provider = provider;
        Status = status;
        ProviderMessageId = string.IsNullOrWhiteSpace(providerMessageId) ? string.Empty : Guard.AgainstNullOrWhiteSpace(providerMessageId, nameof(providerMessageId), 250);
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid NotificationMessageId { get; private set; }

    public NotificationProvider Provider { get; private set; }

    public NotificationDeliveryStatus Status { get; private set; }

    public string ProviderMessageId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }
}

public sealed class NotificationRetry : TenantEntity
{
    private NotificationRetry()
    {
        FailureReason = string.Empty;
    }

    public NotificationRetry(Guid tenantId, Guid messageId, int attempt, DateTimeOffset scheduledAtUtc, string failureReason)
        : base(tenantId)
    {
        NotificationMessageId = Guard.AgainstEmpty(messageId, nameof(messageId));
        Attempt = Guard.AgainstOutOfRange(attempt, nameof(attempt), 1, 20);
        ScheduledAtUtc = scheduledAtUtc;
        FailureReason = Guard.AgainstNullOrWhiteSpace(failureReason, nameof(failureReason), 1_000);
    }

    public Guid NotificationMessageId { get; private set; }

    public int Attempt { get; private set; }

    public DateTimeOffset ScheduledAtUtc { get; private set; }

    public DateTimeOffset? ExecutedAtUtc { get; private set; }

    public string FailureReason { get; private set; }

    public void MarkExecuted(DateTimeOffset executedAtUtc)
    {
        ExecutedAtUtc = executedAtUtc;
    }
}

public sealed class NotificationSubscription : TenantEntity
{
    private NotificationSubscription()
    {
        Topic = string.Empty;
        Recipient = string.Empty;
    }

    public NotificationSubscription(Guid tenantId, string topic, NotificationChannel channel, string recipient)
        : base(tenantId)
    {
        Topic = Guard.AgainstNullOrWhiteSpace(topic, nameof(topic), 120);
        Channel = channel;
        Recipient = Guard.AgainstNullOrWhiteSpace(recipient, nameof(recipient), 320);
        IsActive = true;
    }

    public string Topic { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public string Recipient { get; private set; }

    public bool IsActive { get; private set; }
}

public sealed class NotificationPreference : TenantEntity
{
    private NotificationPreference()
    {
    }

    public NotificationPreference(Guid tenantId, Guid userId, NotificationChannel channel, bool enabled)
        : base(tenantId)
    {
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        Channel = channel;
        Enabled = enabled;
    }

    public Guid UserId { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public bool Enabled { get; private set; }

    public void SetEnabled(bool enabled, DateTimeOffset nowUtc)
    {
        Enabled = enabled;
        MarkUpdated(nowUtc);
    }
}

public sealed class NotificationHistory : TenantEntity
{
    private NotificationHistory()
    {
        EventName = string.Empty;
    }

    public NotificationHistory(Guid tenantId, Guid messageId, NotificationDeliveryStatus status, string eventName, DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        NotificationMessageId = Guard.AgainstEmpty(messageId, nameof(messageId));
        Status = status;
        EventName = Guard.AgainstNullOrWhiteSpace(eventName, nameof(eventName), 160);
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid NotificationMessageId { get; private set; }

    public NotificationDeliveryStatus Status { get; private set; }

    public string EventName { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }
}

public sealed class NotificationAttachment : TenantEntity
{
    private NotificationAttachment()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        StorageObjectKey = string.Empty;
    }

    public NotificationAttachment(Guid tenantId, Guid messageId, string fileName, string contentType, string storageObjectKey)
        : base(tenantId)
    {
        NotificationMessageId = Guard.AgainstEmpty(messageId, nameof(messageId));
        FileName = Guard.AgainstNullOrWhiteSpace(fileName, nameof(fileName), 220);
        ContentType = Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 120);
        StorageObjectKey = Guard.AgainstNullOrWhiteSpace(storageObjectKey, nameof(storageObjectKey), 500);
    }

    public Guid NotificationMessageId { get; private set; }

    public string FileName { get; private set; }

    public string ContentType { get; private set; }

    public string StorageObjectKey { get; private set; }
}

public sealed class NotificationProviderConfiguration : TenantEntity
{
    private NotificationProviderConfiguration()
    {
        Name = string.Empty;
    }

    public NotificationProviderConfiguration(Guid tenantId, NotificationProvider provider, string name, int priority, bool isDefault, bool isEnabled)
        : base(tenantId)
    {
        Provider = provider;
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 120);
        Priority = Guard.AgainstOutOfRange(priority, nameof(priority), 1, 100);
        IsDefault = isDefault;
        IsEnabled = isEnabled;
    }

    public NotificationProvider Provider { get; private set; }

    public string Name { get; private set; }

    public int Priority { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsEnabled { get; private set; }
}

public sealed class NotificationDeadLetter : TenantEntity
{
    private NotificationDeadLetter()
    {
        Reason = string.Empty;
        PayloadJson = string.Empty;
    }

    public NotificationDeadLetter(Guid tenantId, Guid messageId, string reason, string payloadJson, DateTimeOffset deadLetteredAtUtc)
        : base(tenantId)
    {
        NotificationMessageId = Guard.AgainstEmpty(messageId, nameof(messageId));
        Reason = Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 1_000);
        PayloadJson = Guard.AgainstNullOrWhiteSpace(payloadJson, nameof(payloadJson), 8_000);
        DeadLetteredAtUtc = deadLetteredAtUtc;
    }

    public Guid NotificationMessageId { get; private set; }

    public string Reason { get; private set; }

    public string PayloadJson { get; private set; }

    public DateTimeOffset DeadLetteredAtUtc { get; private set; }
}
