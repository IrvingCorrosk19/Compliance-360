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
    PermissionDenied = 9,
    Logout = 10,
    PasswordChanged = 11,
    RoleAssigned = 12,
    PermissionChanged = 13,
    SessionRevoked = 14,
    TokenRefreshed = 15,
    MfaConfigured = 16,
    AccountLocked = 17,
    AccountUnlocked = 18,
    DocumentCreated = 19,
    DocumentUpdated = 20,
    DocumentApproved = 21,
    DocumentRejected = 22,
    WorkflowStarted = 23,
    WorkflowApproved = 24,
    WorkflowRejected = 25,
    SupplierCreated = 26,
    SupplierUpdated = 27,
    TechnicalSheetCreated = 28,
    TechnicalSheetUpdated = 29,
    TenantChanged = 30,
    UserChanged = 31,
    ConfigurationChanged = 32,
    Exported = 33,
    Imported = 34,
    FileUploaded = 35,
    FileDownloaded = 36,
    SecurityEvent = 37,
    AdministrativeEvent = 38,
    NotificationQueued = 39,
    NotificationSent = 40,
    NotificationFailed = 41,
    AuditCreated = 42,
    AuditUpdated = 43,
    AuditClosed = 44,
    AuditReopened = 45,
    CapaCreated = 46,
    CapaUpdated = 47,
    CapaApproved = 48,
    CapaClosed = 49,
    CapaReopened = 50,
    RiskCreated = 51,
    RiskUpdated = 52,
    RiskApproved = 53,
    RiskClosed = 54,
    RiskReopened = 55,
    IndicatorCreated = 56,
    IndicatorUpdated = 57,
    IndicatorApproved = 58,
    IndicatorExported = 59,
    ReportCreated = 60,
    ReportUpdated = 61,
    ReportExecuted = 62,
    ReportExported = 63,
    ReportScheduled = 64,
    MfaChallengeRequired = 65,
    MfaChallengeSucceeded = 66,
    MfaChallengeFailed = 67,
    TelemetryConfigurationChanged = 68,
    MonitoringConfigurationChanged = 69,
    AlertConfigurationChanged = 70,
    NotificationDelivered = 71,
    NotificationRetried = 72,
    NotificationDeadLettered = 73,
    NotificationCancelled = 74,
    NotificationTemplateCreated = 75,
    NotificationTemplateUpdated = 76,
    NotificationProviderChanged = 77,
    NotificationProviderFailover = 78,
    StorageProviderChanged = 79,
    StorageProviderFailover = 80
}

public enum AuditCategory
{
    Authentication = 0,
    Authorization = 1,
    TenantManagement = 2,
    Identity = 3,
    DocumentManagement = 4,
    Workflow = 5,
    SupplierManagement = 6,
    TechnicalSheets = 7,
    Configuration = 8,
    DataExchange = 9,
    FileStorage = 10,
    Security = 11,
    Administration = 12,
    System = 13,
    AuditManagement = 14,
    CapaManagement = 15,
    RiskManagement = 16,
    QualityIndicators = 17,
    ReportingEngine = 18,
    Observability = 19
}

public sealed record AuditContext(
    Guid? TenantId,
    Guid? UserId,
    string? UserName,
    string? Role,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId,
    string? RequestId,
    Guid? SessionId);

public sealed record AuditSnapshot(string? PreviousValuesJson, string? NewValuesJson);

public sealed record AuditMetadata(string? Json);

public sealed record AuditEvent(
    string EntityName,
    Guid? EntityId,
    AuditAction Action,
    AuditCategory Category,
    AuditContext Context,
    AuditSnapshot Snapshot,
    AuditMetadata Metadata,
    bool Success,
    string? ErrorMessage);

public sealed class AuditLog : Entity
{
    private static readonly string[] SensitiveKeywords =
    [
        "password",
        "secret",
        "token",
        "apikey",
        "api_key",
        "authorization",
        "credential"
    ];

    private AuditLog()
    {
        EntityName = string.Empty;
        Action = AuditAction.Created;
        Category = AuditCategory.System;
    }

    private AuditLog(
        Guid? tenantId,
        Guid? userId,
        string? userName,
        string? role,
        string entityName,
        Guid? entityId,
        AuditAction action,
        AuditCategory category,
        DateTimeOffset occurredAtUtc)
    {
        TenantId = tenantId;
        UserId = userId;
        UserName = NormalizeOptional(userName, 180);
        Role = NormalizeOptional(role, 120);
        EntityName = Guard.AgainstNullOrWhiteSpace(entityName, nameof(entityName), 160);
        EntityId = entityId;
        Action = action;
        Category = category;
        OccurredAtUtc = occurredAtUtc;
        Success = true;
    }

    public Guid? TenantId { get; private set; }

    public Guid? UserId { get; private set; }

    public string? UserName { get; private set; }

    public string? Role { get; private set; }

    public string EntityName { get; private set; }

    public Guid? EntityId { get; private set; }

    public AuditAction Action { get; private set; }

    public AuditCategory Category { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public string? CorrelationId { get; private set; }

    public string? RequestId { get; private set; }

    public Guid? SessionId { get; private set; }

    public bool Success { get; private set; }

    public string? ErrorMessage { get; private set; }

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
        return new AuditLog(tenantId, userId, null, null, entityName, entityId, action, InferCategory(action), occurredAtUtc);
    }

    public static AuditLog FromEvent(AuditEvent auditEvent, DateTimeOffset occurredAtUtc)
    {
        var log = new AuditLog(
            auditEvent.Context.TenantId,
            auditEvent.Context.UserId,
            auditEvent.Context.UserName,
            auditEvent.Context.Role,
            auditEvent.EntityName,
            auditEvent.EntityId,
            auditEvent.Action,
            auditEvent.Category,
            occurredAtUtc);

        return log
            .WithRequestContext(
                auditEvent.Context.IpAddress,
                auditEvent.Context.UserAgent,
                auditEvent.Context.CorrelationId,
                auditEvent.Context.RequestId,
                auditEvent.Context.SessionId)
            .WithChanges(auditEvent.Snapshot.PreviousValuesJson, auditEvent.Snapshot.NewValuesJson)
            .WithMetadata(auditEvent.Metadata.Json)
            .WithOutcome(auditEvent.Success, auditEvent.ErrorMessage);
    }

    public AuditLog WithRequestContext(string? ipAddress, string? userAgent, string? correlationId)
    {
        return WithRequestContext(ipAddress, userAgent, correlationId, null, null);
    }

    public AuditLog WithRequestContext(string? ipAddress, string? userAgent, string? correlationId, string? requestId, Guid? sessionId)
    {
        IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
        UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim();
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
        RequestId = string.IsNullOrWhiteSpace(requestId) ? null : requestId.Trim();
        SessionId = sessionId;
        return this;
    }

    public AuditLog WithChanges(string? beforeValuesJson, string? afterValuesJson)
    {
        BeforeValuesJson = SanitizePayload(beforeValuesJson);
        AfterValuesJson = SanitizePayload(afterValuesJson);
        return this;
    }

    public AuditLog WithMetadata(string? metadataJson)
    {
        MetadataJson = SanitizePayload(metadataJson);
        return this;
    }

    public AuditLog WithOutcome(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = NormalizeOptional(errorMessage, 1_000);
        return this;
    }

    public static AuditCategory InferCategory(AuditAction action)
    {
        return action switch
        {
            AuditAction.LoginSucceeded or AuditAction.LoginFailed or AuditAction.Logout => AuditCategory.Authentication,
            AuditAction.PermissionDenied or AuditAction.RoleAssigned or AuditAction.PermissionChanged => AuditCategory.Authorization,
            AuditAction.PasswordChanged or AuditAction.UserChanged or AuditAction.MfaConfigured or AuditAction.AccountLocked or AuditAction.AccountUnlocked => AuditCategory.Identity,
            AuditAction.DocumentCreated or AuditAction.DocumentUpdated or AuditAction.DocumentApproved or AuditAction.DocumentRejected => AuditCategory.DocumentManagement,
            AuditAction.WorkflowStarted or AuditAction.WorkflowApproved or AuditAction.WorkflowRejected => AuditCategory.Workflow,
            AuditAction.SupplierCreated or AuditAction.SupplierUpdated => AuditCategory.SupplierManagement,
            AuditAction.TechnicalSheetCreated or AuditAction.TechnicalSheetUpdated => AuditCategory.TechnicalSheets,
            AuditAction.TenantChanged => AuditCategory.TenantManagement,
            AuditAction.ConfigurationChanged => AuditCategory.Configuration,
            AuditAction.Exported or AuditAction.Imported => AuditCategory.DataExchange,
            AuditAction.FileUploaded or AuditAction.FileDownloaded or AuditAction.Downloaded => AuditCategory.FileStorage,
            AuditAction.SecurityEvent or AuditAction.TokenRefreshed => AuditCategory.Security,
            AuditAction.AdministrativeEvent => AuditCategory.Administration,
            AuditAction.NotificationQueued or AuditAction.NotificationSent or AuditAction.NotificationFailed
                or AuditAction.NotificationDelivered or AuditAction.NotificationRetried or AuditAction.NotificationDeadLettered
                or AuditAction.NotificationCancelled or AuditAction.NotificationTemplateCreated or AuditAction.NotificationTemplateUpdated
                or AuditAction.NotificationProviderChanged or AuditAction.NotificationProviderFailover => AuditCategory.System,
            AuditAction.StorageProviderChanged or AuditAction.StorageProviderFailover => AuditCategory.FileStorage,
            AuditAction.AuditCreated or AuditAction.AuditUpdated or AuditAction.AuditClosed or AuditAction.AuditReopened => AuditCategory.AuditManagement,
            AuditAction.CapaCreated or AuditAction.CapaUpdated or AuditAction.CapaApproved or AuditAction.CapaClosed or AuditAction.CapaReopened => AuditCategory.CapaManagement,
            AuditAction.RiskCreated or AuditAction.RiskUpdated or AuditAction.RiskApproved or AuditAction.RiskClosed or AuditAction.RiskReopened => AuditCategory.RiskManagement,
            AuditAction.IndicatorCreated or AuditAction.IndicatorUpdated or AuditAction.IndicatorApproved or AuditAction.IndicatorExported => AuditCategory.QualityIndicators,
            AuditAction.ReportCreated or AuditAction.ReportUpdated or AuditAction.ReportExecuted or AuditAction.ReportExported or AuditAction.ReportScheduled => AuditCategory.ReportingEngine,
            _ => AuditCategory.System
        };
    }

    private static string? SanitizePayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        var trimmed = payload.Trim();
        if (SensitiveKeywords.Any(keyword => trimmed.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return "{\"redacted\":true}";
        }

        return trimmed;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
