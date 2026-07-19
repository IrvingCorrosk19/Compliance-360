using System.Net;
using System.Text.Json;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Notifications;

namespace Compliance360.Application.Notifications;

public sealed class NotificationTemplateEngine : INotificationTemplateEngine
{
    public NotificationRenderedTemplate Render(string subject, string htmlBody, string? textBody, IReadOnlyDictionary<string, string> variables, TenantNotificationBranding? branding)
    {
        var enrichedVariables = new Dictionary<string, string>(variables, StringComparer.OrdinalIgnoreCase);
        if (branding is not null)
        {
            AddIfPresent(enrichedVariables, "TenantLogo", branding.LogoUrl);
            AddIfPresent(enrichedVariables, "PrimaryColor", branding.PrimaryColor);
            AddIfPresent(enrichedVariables, "SecondaryColor", branding.SecondaryColor);
            AddIfPresent(enrichedVariables, "TenantName", branding.TenantName);
        }

        return new NotificationRenderedTemplate(
            RenderTokens(subject, enrichedVariables, htmlEncodeValues: false).Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal),
            RenderTokens(htmlBody, enrichedVariables, htmlEncodeValues: true),
            string.IsNullOrWhiteSpace(textBody) ? null : RenderTokens(textBody, enrichedVariables, htmlEncodeValues: false));
    }

    private static void AddIfPresent(Dictionary<string, string> variables, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            variables[key] = value;
        }
    }

    private static string RenderTokens(string template, IReadOnlyDictionary<string, string> variables, bool htmlEncodeValues)
    {
        var rendered = template;
        foreach (var variable in variables)
        {
            var value = htmlEncodeValues ? WebUtility.HtmlEncode(variable.Value) : variable.Value;
            rendered = rendered.Replace($"{{{{{variable.Key}}}}}", value, StringComparison.OrdinalIgnoreCase);
        }

        return rendered;
    }
}

public sealed class NotificationRetryService : INotificationRetryService
{
    private readonly NotificationProviderOptions _options;

    public NotificationRetryService(Microsoft.Extensions.Options.IOptions<NotificationProviderOptions> options)
    {
        _options = options.Value;
    }

    public DateTimeOffset GetNextRetryAt(DateTimeOffset utcNow, int retryCount)
    {
        var delay = Math.Pow(2, Math.Max(0, retryCount)) * Math.Max(1, _options.RetryBaseDelaySeconds);
        return utcNow.AddSeconds(delay);
    }

    public bool ShouldDeadLetter(int retryCount)
    {
        return retryCount >= Math.Max(1, _options.MaxRetryCount);
    }
}

public sealed class NotificationTrackingService : INotificationTrackingService
{
    private readonly INotificationRepository _repository;

    public NotificationTrackingService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task TrackAsync(Guid tenantId, Guid messageId, NotificationDeliveryStatus status, string eventName, CancellationToken cancellationToken = default)
    {
        await _repository.AddHistoryAsync(new NotificationHistory(tenantId, messageId, status, eventName, DateTimeOffset.UtcNow), cancellationToken);
    }
}

public sealed class NotificationAuditService : INotificationAuditService
{
    private readonly INotificationRepository _repository;
    private readonly IClock _clock;

    public NotificationAuditService(INotificationRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task AppendAsync(Guid tenantId, Guid userId, string entityName, Guid entityId, AuditAction action, bool success, string? error, CancellationToken cancellationToken = default)
    {
        var auditLog = AuditLog.FromEvent(
            new AuditEvent(
                entityName,
                entityId,
                action,
                AuditLog.InferCategory(action),
                new AuditContext(tenantId, userId, null, null, null, null, null, null, null),
                new AuditSnapshot(null, null),
                new AuditMetadata(JsonSerializer.Serialize(new { source = "notification-enterprise" })),
                success,
                error),
            _clock.UtcNow);

        await _repository.AddAuditLogAsync(auditLog, cancellationToken);
    }
}
