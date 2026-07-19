using Compliance360.Domain.Common;

namespace Compliance360.Domain.Notifications;

public enum NotificationOutboxStatus
{
    Pending = 0,
    Processing = 1,
    Published = 2,
    Failed = 3
}

public sealed class NotificationOutboxEvent : TenantEntity
{
    private NotificationOutboxEvent()
    {
        EventType = string.Empty;
        AggregateType = string.Empty;
        PayloadJson = string.Empty;
        CorrelationId = string.Empty;
    }

    public NotificationOutboxEvent(
        Guid tenantId,
        string eventType,
        string aggregateType,
        Guid aggregateId,
        string payloadJson,
        string correlationId,
        DateTimeOffset occurredAtUtc,
        DateTimeOffset? availableAtUtc = null)
        : base(tenantId)
    {
        EventType = Guard.AgainstNullOrWhiteSpace(eventType, nameof(eventType), 160);
        AggregateType = Guard.AgainstNullOrWhiteSpace(aggregateType, nameof(aggregateType), 120);
        AggregateId = Guard.AgainstEmpty(aggregateId, nameof(aggregateId));
        PayloadJson = Guard.AgainstNullOrWhiteSpace(payloadJson, nameof(payloadJson), 32_000);
        CorrelationId = Guard.AgainstNullOrWhiteSpace(correlationId, nameof(correlationId), 120);
        OccurredAtUtc = occurredAtUtc;
        AvailableAtUtc = availableAtUtc ?? occurredAtUtc;
        Status = NotificationOutboxStatus.Pending;
    }

    public string EventType { get; private set; }

    public string AggregateType { get; private set; }

    public Guid AggregateId { get; private set; }

    public string PayloadJson { get; private set; }

    public string CorrelationId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public DateTimeOffset AvailableAtUtc { get; private set; }

    public NotificationOutboxStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public string? LeaseToken { get; private set; }

    public string? LeaseOwner { get; private set; }

    public DateTimeOffset? LeaseUntilUtc { get; private set; }

    public DateTimeOffset? PublishedAtUtc { get; private set; }

    public string? LastError { get; private set; }

    public void AcquireLease(string leaseToken, string leaseOwner, DateTimeOffset nowUtc, DateTimeOffset leaseUntilUtc)
    {
        if (Status is not (NotificationOutboxStatus.Pending or NotificationOutboxStatus.Failed or NotificationOutboxStatus.Processing))
        {
            throw new DomainException("Outbox event is not claimable.");
        }

        if (Status == NotificationOutboxStatus.Processing && LeaseUntilUtc.HasValue && LeaseUntilUtc > nowUtc)
        {
            throw new DomainException("Outbox event already has an active lease.");
        }

        if (AvailableAtUtc > nowUtc || leaseUntilUtc <= nowUtc)
        {
            throw new DomainException("Outbox event is not available for the requested lease.");
        }

        LeaseToken = Guard.AgainstNullOrWhiteSpace(leaseToken, nameof(leaseToken), 120);
        LeaseOwner = Guard.AgainstNullOrWhiteSpace(leaseOwner, nameof(leaseOwner), 160);
        LeaseUntilUtc = leaseUntilUtc;
        Status = NotificationOutboxStatus.Processing;
        AttemptCount++;
    }

    public void MarkPublished(string leaseToken, DateTimeOffset publishedAtUtc)
    {
        EnsureLease(leaseToken);
        Status = NotificationOutboxStatus.Published;
        PublishedAtUtc = publishedAtUtc;
        LastError = null;
        ClearLease();
    }

    public void MarkFailed(string leaseToken, string error, DateTimeOffset availableAtUtc)
    {
        EnsureLease(leaseToken);
        Status = NotificationOutboxStatus.Failed;
        LastError = Guard.AgainstNullOrWhiteSpace(error, nameof(error), 1_000);
        AvailableAtUtc = availableAtUtc;
        ClearLease();
    }

    private void EnsureLease(string leaseToken)
    {
        if (string.IsNullOrWhiteSpace(LeaseToken) || !string.Equals(LeaseToken, leaseToken, StringComparison.Ordinal))
        {
            throw new DomainException("Outbox lease token is invalid.");
        }
    }

    private void ClearLease()
    {
        LeaseToken = null;
        LeaseOwner = null;
        LeaseUntilUtc = null;
    }
}

public sealed class NotificationWorkerHeartbeat : Entity
{
    private NotificationWorkerHeartbeat()
    {
        WorkerId = string.Empty;
        InstanceName = string.Empty;
        Status = string.Empty;
    }

    public NotificationWorkerHeartbeat(string workerId, string instanceName, DateTimeOffset startedAtUtc)
    {
        WorkerId = Guard.AgainstNullOrWhiteSpace(workerId, nameof(workerId), 160);
        InstanceName = Guard.AgainstNullOrWhiteSpace(instanceName, nameof(instanceName), 160);
        StartedAtUtc = startedAtUtc;
        LastSeenAtUtc = startedAtUtc;
        Status = "Starting";
    }

    public string WorkerId { get; private set; }

    public string InstanceName { get; private set; }

    public DateTimeOffset StartedAtUtc { get; private set; }

    public DateTimeOffset LastSeenAtUtc { get; private set; }

    public string Status { get; private set; }

    public int ActiveLeases { get; private set; }

    public long ProcessedCount { get; private set; }

    public long FailureCount { get; private set; }

    public string? LastError { get; private set; }

    public void Beat(DateTimeOffset nowUtc, int activeLeases, long processedCount, long failureCount, string? lastError)
    {
        LastSeenAtUtc = nowUtc;
        ActiveLeases = Math.Max(0, activeLeases);
        ProcessedCount = Math.Max(0, processedCount);
        FailureCount = Math.Max(0, failureCount);
        LastError = string.IsNullOrWhiteSpace(lastError) ? null : Guard.AgainstNullOrWhiteSpace(lastError, nameof(lastError), 1_000);
        Status = LastError is null ? "Healthy" : "Degraded";
        MarkUpdated(nowUtc);
    }

    public void Stop(DateTimeOffset nowUtc)
    {
        LastSeenAtUtc = nowUtc;
        ActiveLeases = 0;
        Status = "Stopped";
        MarkUpdated(nowUtc);
    }
}
