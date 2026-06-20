using Compliance360.Domain.Common;

namespace Compliance360.Domain.Audit;

public enum AuditAction
{
    Created = 0,
    Updated = 1,
    Deleted = 2,
    Viewed = 3,
    Downloaded = 4,
    Approved = 5,
    Rejected = 6,
    LoginSucceeded = 7,
    LoginFailed = 8,
    PermissionDenied = 9
}

public sealed class AuditLog : Entity
{
    private AuditLog()
    {
        EntityName = string.Empty;
        Action = AuditAction.Created;
    }

    private AuditLog(
        Guid? tenantId,
        Guid? userId,
        string entityName,
        Guid? entityId,
        AuditAction action,
        DateTimeOffset occurredAtUtc)
    {
        TenantId = tenantId;
        UserId = userId;
        EntityName = Guard.AgainstNullOrWhiteSpace(entityName, nameof(entityName), 160);
        EntityId = entityId;
        Action = action;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid? TenantId { get; private set; }

    public Guid? UserId { get; private set; }

    public string EntityName { get; private set; }

    public Guid? EntityId { get; private set; }

    public AuditAction Action { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public string? CorrelationId { get; private set; }

    public string? BeforeValuesJson { get; private set; }

    public string? AfterValuesJson { get; private set; }

    public string? MetadataJson { get; private set; }

    public static AuditLog Create(
        Guid? tenantId,
        Guid? userId,
        string entityName,
        Guid? entityId,
        AuditAction action,
        DateTimeOffset occurredAtUtc)
    {
        return new AuditLog(tenantId, userId, entityName, entityId, action, occurredAtUtc);
    }

    public AuditLog WithRequestContext(string? ipAddress, string? userAgent, string? correlationId)
    {
        IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
        UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim();
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
        return this;
    }

    public AuditLog WithChanges(string? beforeValuesJson, string? afterValuesJson)
    {
        BeforeValuesJson = string.IsNullOrWhiteSpace(beforeValuesJson) ? null : beforeValuesJson.Trim();
        AfterValuesJson = string.IsNullOrWhiteSpace(afterValuesJson) ? null : afterValuesJson.Trim();
        return this;
    }

    public AuditLog WithMetadata(string? metadataJson)
    {
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson.Trim();
        return this;
    }
}
