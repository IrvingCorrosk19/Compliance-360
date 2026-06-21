using Compliance360.Domain.Audit;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public interface INotificationService
{
    Task<Result<NotificationTemplateSummary>> CreateTemplateAsync(CreateNotificationTemplateCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationTemplateSummary>> PreviewTemplateAsync(PreviewNotificationTemplateCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationMessageSummary>> QueueAsync(QueueNotificationCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationMessageSummary>> SendAsync(SendNotificationCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationMessageSummary>> RetryAsync(RetryNotificationCommand command, CancellationToken cancellationToken = default);

    Task<Result> CancelAsync(CancelNotificationCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationDashboardSummary>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<NotificationMessageSummary>>> GetHistoryAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<NotificationDeadLetterSummary>>> GetDeadLettersAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Result<NotificationProviderConfigurationSummary>> ConfigureProviderAsync(ConfigureNotificationProviderCommand command, CancellationToken cancellationToken = default);
}

public interface INotificationRepository
{
    Task AddTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);

    Task<NotificationTemplate?> GetTemplateAsync(Guid tenantId, string code, NotificationChannel channel, CancellationToken cancellationToken = default);

    Task AddMessageAsync(NotificationMessage message, CancellationToken cancellationToken = default);

    Task<NotificationMessage?> GetMessageAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<NotificationMessage>> ListMessagesAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task AddDeliveryAsync(NotificationDelivery delivery, CancellationToken cancellationToken = default);

    Task AddRetryAsync(NotificationRetry retry, CancellationToken cancellationToken = default);

    Task AddHistoryAsync(NotificationHistory history, CancellationToken cancellationToken = default);

    Task AddDeadLetterAsync(NotificationDeadLetter deadLetter, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<NotificationDeadLetter>> ListDeadLettersAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task AddProviderConfigurationAsync(NotificationProviderConfiguration configuration, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<NotificationProviderConfiguration>> ListProviderConfigurationsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public interface INotificationDispatcher
{
    Task<NotificationDispatchResult> DispatchAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default);
}

public interface INotificationProvider
{
    NotificationProvider Provider { get; }

    Task<NotificationDispatchResult> SendAsync(NotificationDispatchRequest request, NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default);

    Task<NotificationProviderHealth> CheckHealthAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default);
}

public interface INotificationProviderFactory
{
    INotificationProvider GetProvider(NotificationProvider provider);
}

public interface INotificationTemplateEngine
{
    NotificationRenderedTemplate Render(string subject, string htmlBody, string? textBody, IReadOnlyDictionary<string, string> variables, TenantNotificationBranding? branding);
}

public interface INotificationRetryService
{
    DateTimeOffset GetNextRetryAt(DateTimeOffset utcNow, int retryCount);

    bool ShouldDeadLetter(int retryCount);
}

public interface INotificationTrackingService
{
    Task TrackAsync(Guid tenantId, Guid messageId, NotificationDeliveryStatus status, string eventName, CancellationToken cancellationToken = default);
}

public interface INotificationAuditService
{
    Task AppendAsync(Guid tenantId, Guid userId, string entityName, Guid entityId, AuditAction action, bool success, string? error, CancellationToken cancellationToken = default);
}

public sealed record CreateNotificationTemplateCommand(
    Guid TenantId,
    Guid RequestedByUserId,
    string Code,
    NotificationChannel Channel,
    string Subject,
    string Body,
    string? TextBody = null,
    string? Locale = null,
    string? BrandingJson = null);

public sealed record PreviewNotificationTemplateCommand(
    Guid TenantId,
    string Subject,
    string Body,
    string? TextBody,
    IReadOnlyDictionary<string, string> Variables,
    TenantNotificationBranding? Branding);

public sealed record QueueNotificationCommand(
    Guid TenantId,
    Guid RequestedByUserId,
    NotificationChannel Channel,
    string Recipient,
    string? Subject,
    string? Body,
    string? TemplateCode,
    IReadOnlyDictionary<string, string> Variables,
    NotificationPriority Priority,
    Guid? TargetUserId);

public sealed record SendNotificationCommand(Guid TenantId, Guid NotificationMessageId, Guid RequestedByUserId);

public sealed record RetryNotificationCommand(Guid TenantId, Guid NotificationMessageId, Guid RequestedByUserId);

public sealed record CancelNotificationCommand(Guid TenantId, Guid NotificationMessageId, Guid RequestedByUserId);

public sealed record ConfigureNotificationProviderCommand(
    Guid TenantId,
    Guid RequestedByUserId,
    NotificationProvider Provider,
    string Name,
    int Priority,
    bool IsDefault,
    bool IsEnabled);

public sealed record NotificationDispatchRequest(
    Guid TenantId,
    Guid MessageId,
    NotificationChannel Channel,
    string Recipient,
    string Subject,
    string Body,
    string? TextBody = null,
    NotificationPriority Priority = NotificationPriority.Normal);

public sealed record NotificationDispatchResult(bool Success, string? FailureReason, NotificationProvider? Provider = null, string? ProviderMessageId = null)
{
    public static NotificationDispatchResult Sent()
    {
        return new NotificationDispatchResult(true, null);
    }

    public static NotificationDispatchResult Sent(NotificationProvider provider, string? providerMessageId)
    {
        return new NotificationDispatchResult(true, null, provider, providerMessageId);
    }

    public static NotificationDispatchResult Failed(string reason, NotificationProvider? provider = null)
    {
        return new NotificationDispatchResult(false, reason, provider);
    }
}

public sealed record NotificationProviderEndpoint(
    NotificationProvider Provider,
    string? Host,
    int? Port,
    string? Username,
    string? Secret,
    string? FromAddress,
    string? FromName,
    string? BaseUrl,
    string? Domain,
    bool UseSsl);

public sealed record NotificationProviderHealth(NotificationProvider Provider, bool Healthy, string Message);

public sealed record NotificationRenderedTemplate(string Subject, string HtmlBody, string? TextBody);

public sealed record TenantNotificationBranding(string? LogoUrl, string? PrimaryColor, string? SecondaryColor, string? TenantName);

public sealed record NotificationTemplateSummary(
    Guid Id,
    Guid TenantId,
    string Code,
    NotificationChannel Channel,
    string Subject,
    bool IsActive);

public sealed record NotificationMessageSummary(
    Guid Id,
    Guid TenantId,
    NotificationChannel Channel,
    string Recipient,
    string Subject,
    NotificationPriority Priority,
    NotificationStatus Status,
    DateTimeOffset QueuedAtUtc,
    DateTimeOffset? SentAtUtc,
    string? FailureReason);

public sealed record NotificationDeadLetterSummary(Guid Id, Guid TenantId, Guid NotificationMessageId, string Reason, DateTimeOffset DeadLetteredAtUtc);

public sealed record NotificationProviderConfigurationSummary(Guid Id, Guid TenantId, NotificationProvider Provider, string Name, int Priority, bool IsDefault, bool IsEnabled);

public sealed record NotificationDashboardSummary(
    long Sent,
    long Delivered,
    long Failed,
    long RetryCount,
    long DeadLetters,
    double DeliveryRatePercent,
    IReadOnlyDictionary<NotificationProvider, bool> ProviderHealth,
    IReadOnlyDictionary<Guid, long> TenantNotificationUsage);

public sealed class NotificationProviderOptions
{
    public const string SectionName = "Notifications";

    public NotificationProvider DefaultProvider { get; set; } = NotificationProvider.Smtp;

    public NotificationProvider? FailoverProvider { get; set; }

    public int MaxRetryCount { get; set; } = 3;

    public int RetryBaseDelaySeconds { get; set; } = 60;

    public Dictionary<NotificationProvider, NotificationProviderEndpointOptions> Providers { get; set; } = [];
}

public sealed class NotificationProviderEndpointOptions
{
    public string? Host { get; set; }

    public int? Port { get; set; }

    public string? Username { get; set; }

    public string? Secret { get; set; }

    public string? FromAddress { get; set; }

    public string? FromName { get; set; }

    public string? BaseUrl { get; set; }

    public string? Domain { get; set; }

    public bool UseSsl { get; set; } = true;
}
