using Compliance360.Domain.Common;

namespace Compliance360.Domain.Notifications;

public enum AlertDefinitionLifecycle
{
    Draft = 0,
    Review = 1,
    Approved = 2,
    Published = 3,
    Disabled = 4,
    Archived = 5
}

public enum AlertOccurrenceStatus
{
    Pending = 0,
    Matched = 1,
    Suppressed = 2,
    NoRecipients = 3,
    Queued = 4,
    Completed = 5,
    Failed = 6
}

public enum AlertUnknownPolicy
{
    TreatAsFalse = 0,
    TreatAsTrue = 1,
    FailEvaluation = 2
}

public sealed class AlertEventType : TenantEntity
{
    private AlertEventType()
    {
        Code = string.Empty;
        Name = string.Empty;
        Module = string.Empty;
        SchemaJson = "{}";
    }

    public AlertEventType(Guid tenantId, string code, string name, string module, string schemaJson, int schemaVersion)
        : base(tenantId)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 160).ToLowerInvariant();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 200);
        Module = Guard.AgainstNullOrWhiteSpace(module, nameof(module), 120);
        SchemaJson = Guard.AgainstNullOrWhiteSpace(schemaJson, nameof(schemaJson), 32_000);
        SchemaVersion = Guard.AgainstOutOfRange(schemaVersion, nameof(schemaVersion), 1, 10_000);
        IsActive = true;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public string Module { get; private set; }

    public string SchemaJson { get; private set; }

    public int SchemaVersion { get; private set; }

    public bool IsActive { get; private set; }

    public void Deactivate(DateTimeOffset nowUtc)
    {
        IsActive = false;
        MarkUpdated(nowUtc);
    }
}

public sealed class AlertDefinition : TenantEntity
{
    private AlertDefinition()
    {
        Code = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
    }

    public AlertDefinition(
        Guid tenantId,
        Guid eventTypeId,
        string code,
        string name,
        string description,
        Guid ownerUserId,
        NotificationPriority priority)
        : base(tenantId)
    {
        EventTypeId = Guard.AgainstEmpty(eventTypeId, nameof(eventTypeId));
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 160).ToUpperInvariant();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 200);
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 2_000);
        OwnerUserId = Guard.AgainstEmpty(ownerUserId, nameof(ownerUserId));
        Priority = priority;
        Lifecycle = AlertDefinitionLifecycle.Draft;
    }

    public Guid EventTypeId { get; private set; }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public Guid? BackupOwnerUserId { get; private set; }

    public NotificationPriority Priority { get; private set; }

    public AlertDefinitionLifecycle Lifecycle { get; private set; }

    public Guid? CurrentPublishedVersionId { get; private set; }

    public void SetPublishedVersion(Guid versionId, DateTimeOffset nowUtc)
    {
        CurrentPublishedVersionId = Guard.AgainstEmpty(versionId, nameof(versionId));
        Lifecycle = AlertDefinitionLifecycle.Published;
        MarkUpdated(nowUtc);
    }

    public void Disable(DateTimeOffset nowUtc)
    {
        if (Lifecycle != AlertDefinitionLifecycle.Published)
        {
            throw new DomainException("Only published alert definitions can be disabled.");
        }

        Lifecycle = AlertDefinitionLifecycle.Disabled;
        MarkUpdated(nowUtc);
    }

    public void Enable(DateTimeOffset nowUtc)
    {
        if (Lifecycle != AlertDefinitionLifecycle.Disabled || !CurrentPublishedVersionId.HasValue)
        {
            throw new DomainException("Only a disabled alert definition with a published version can be enabled.");
        }

        Lifecycle = AlertDefinitionLifecycle.Published;
        MarkUpdated(nowUtc);
    }
}

public sealed class AlertDefinitionVersion : TenantEntity
{
    private AlertDefinitionVersion()
    {
        ConditionJson = "{}";
        RecipientRulesJson = "[]";
        ChannelPoliciesJson = "[]";
        DedupeExpression = string.Empty;
    }

    public AlertDefinitionVersion(
        Guid tenantId,
        Guid definitionId,
        int version,
        string conditionJson,
        string recipientRulesJson,
        string channelPoliciesJson,
        string dedupeExpression,
        int silenceWindowMinutes,
        int? slaMinutes,
        AlertUnknownPolicy unknownPolicy,
        Guid createdByUserId)
        : base(tenantId)
    {
        DefinitionId = Guard.AgainstEmpty(definitionId, nameof(definitionId));
        Version = Guard.AgainstOutOfRange(version, nameof(version), 1, 100_000);
        ConditionJson = Guard.AgainstNullOrWhiteSpace(conditionJson, nameof(conditionJson), 64_000);
        RecipientRulesJson = Guard.AgainstNullOrWhiteSpace(recipientRulesJson, nameof(recipientRulesJson), 64_000);
        ChannelPoliciesJson = Guard.AgainstNullOrWhiteSpace(channelPoliciesJson, nameof(channelPoliciesJson), 64_000);
        DedupeExpression = Guard.AgainstNullOrWhiteSpace(dedupeExpression, nameof(dedupeExpression), 500);
        SilenceWindowMinutes = Guard.AgainstOutOfRange(silenceWindowMinutes, nameof(silenceWindowMinutes), 0, 525_600);
        if (slaMinutes.HasValue)
        {
            Guard.AgainstOutOfRange(slaMinutes.Value, nameof(slaMinutes), 1, 525_600);
        }

        SlaMinutes = slaMinutes;
        UnknownPolicy = unknownPolicy;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        Lifecycle = AlertDefinitionLifecycle.Draft;
    }

    public Guid DefinitionId { get; private set; }

    public int Version { get; private set; }

    public string ConditionJson { get; private set; }

    public string RecipientRulesJson { get; private set; }

    public string ChannelPoliciesJson { get; private set; }

    public string DedupeExpression { get; private set; }

    public int SilenceWindowMinutes { get; private set; }

    public int? SlaMinutes { get; private set; }

    public AlertUnknownPolicy UnknownPolicy { get; private set; }

    public AlertDefinitionLifecycle Lifecycle { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public Guid? ReviewedByUserId { get; private set; }

    public Guid? ApprovedByUserId { get; private set; }

    public Guid? PublishedByUserId { get; private set; }

    public DateTimeOffset? ReviewedAtUtc { get; private set; }

    public DateTimeOffset? ApprovedAtUtc { get; private set; }

    public DateTimeOffset? PublishedAtUtc { get; private set; }

    public void SubmitForReview(DateTimeOffset nowUtc)
    {
        EnsureLifecycle(AlertDefinitionLifecycle.Draft);
        Lifecycle = AlertDefinitionLifecycle.Review;
        MarkUpdated(nowUtc);
    }

    public void Review(Guid reviewerUserId, DateTimeOffset nowUtc)
    {
        EnsureLifecycle(AlertDefinitionLifecycle.Review);
        reviewerUserId = Guard.AgainstEmpty(reviewerUserId, nameof(reviewerUserId));
        if (reviewerUserId == CreatedByUserId)
        {
            throw new DomainException("Alert rule maker cannot review the same version.");
        }

        ReviewedByUserId = reviewerUserId;
        ReviewedAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }

    public void Approve(Guid approverUserId, DateTimeOffset nowUtc)
    {
        EnsureLifecycle(AlertDefinitionLifecycle.Review);
        approverUserId = Guard.AgainstEmpty(approverUserId, nameof(approverUserId));
        if (!ReviewedByUserId.HasValue || approverUserId == CreatedByUserId)
        {
            throw new DomainException("Alert rule requires independent review before approval.");
        }

        ApprovedByUserId = approverUserId;
        ApprovedAtUtc = nowUtc;
        Lifecycle = AlertDefinitionLifecycle.Approved;
        MarkUpdated(nowUtc);
    }

    public void Publish(Guid publisherUserId, DateTimeOffset nowUtc)
    {
        EnsureLifecycle(AlertDefinitionLifecycle.Approved);
        PublishedByUserId = Guard.AgainstEmpty(publisherUserId, nameof(publisherUserId));
        PublishedAtUtc = nowUtc;
        Lifecycle = AlertDefinitionLifecycle.Published;
        MarkUpdated(nowUtc);
    }

    private void EnsureLifecycle(AlertDefinitionLifecycle expected)
    {
        if (Lifecycle != expected)
        {
            throw new DomainException($"Alert rule version must be {expected} for this action.");
        }
    }
}

public sealed class AlertOccurrence : TenantEntity
{
    private AlertOccurrence()
    {
        DedupeKey = string.Empty;
        PayloadJson = "{}";
        CorrelationId = string.Empty;
        SourceModule = string.Empty;
        EntityType = string.Empty;
    }

    public AlertOccurrence(
        Guid tenantId,
        Guid definitionId,
        Guid definitionVersionId,
        Guid eventTypeId,
        string dedupeKey,
        string payloadJson,
        string correlationId,
        string sourceModule,
        string entityType,
        Guid? entityId,
        DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        DefinitionId = Guard.AgainstEmpty(definitionId, nameof(definitionId));
        DefinitionVersionId = Guard.AgainstEmpty(definitionVersionId, nameof(definitionVersionId));
        EventTypeId = Guard.AgainstEmpty(eventTypeId, nameof(eventTypeId));
        DedupeKey = Guard.AgainstNullOrWhiteSpace(dedupeKey, nameof(dedupeKey), 500);
        PayloadJson = Guard.AgainstNullOrWhiteSpace(payloadJson, nameof(payloadJson), 64_000);
        CorrelationId = Guard.AgainstNullOrWhiteSpace(correlationId, nameof(correlationId), 120);
        SourceModule = Guard.AgainstNullOrWhiteSpace(sourceModule, nameof(sourceModule), 120);
        EntityType = Guard.AgainstNullOrWhiteSpace(entityType, nameof(entityType), 160);
        EntityId = entityId;
        OccurredAtUtc = occurredAtUtc;
        Status = AlertOccurrenceStatus.Pending;
    }

    public Guid DefinitionId { get; private set; }
    public Guid DefinitionVersionId { get; private set; }
    public Guid EventTypeId { get; private set; }
    public string DedupeKey { get; private set; }
    public string PayloadJson { get; private set; }
    public string CorrelationId { get; private set; }
    public string SourceModule { get; private set; }
    public string EntityType { get; private set; }
    public Guid? EntityId { get; private set; }
    public AlertOccurrenceStatus Status { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public DateTimeOffset? EvaluatedAtUtc { get; private set; }
    public string? FailureReason { get; private set; }

    public void CompleteEvaluation(AlertOccurrenceStatus status, DateTimeOffset evaluatedAtUtc, string? failureReason = null)
    {
        if (status == AlertOccurrenceStatus.Pending)
        {
            throw new DomainException("Evaluation cannot complete with Pending status.");
        }

        Status = status;
        EvaluatedAtUtc = evaluatedAtUtc;
        FailureReason = string.IsNullOrWhiteSpace(failureReason) ? null : Guard.AgainstNullOrWhiteSpace(failureReason, nameof(failureReason), 1_000);
        MarkUpdated(evaluatedAtUtc);
    }
}
