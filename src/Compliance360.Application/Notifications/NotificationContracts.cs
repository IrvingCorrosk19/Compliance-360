using Compliance360.Domain.Audit;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public interface INotificationService
{
    Task<Result<NotificationTemplateSummary>> CreateTemplateAsync(CreateNotificationTemplateCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationMessageSummary>> QueueAsync(QueueNotificationCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationMessageSummary>> SendAsync(SendNotificationCommand command, CancellationToken cancellationToken = default);

    Task<Result> CancelAsync(CancelNotificationCommand command, CancellationToken cancellationToken = default);
}

public interface INotificationRepository
{
    Task AddTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);

    Task<NotificationTemplate?> GetTemplateAsync(Guid tenantId, string code, NotificationChannel channel, CancellationToken cancellationToken = default);

    Task AddMessageAsync(NotificationMessage message, CancellationToken cancellationToken = default);

    Task<NotificationMessage?> GetMessageAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public interface INotificationDispatcher
{
    Task<NotificationDispatchResult> DispatchAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default);
}

public sealed record CreateNotificationTemplateCommand(
    Guid TenantId,
    Guid RequestedByUserId,
    string Code,
    NotificationChannel Channel,
    string Subject,
    string Body);

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

public sealed record CancelNotificationCommand(Guid TenantId, Guid NotificationMessageId, Guid RequestedByUserId);

public sealed record NotificationDispatchRequest(
    Guid TenantId,
    Guid MessageId,
    NotificationChannel Channel,
    string Recipient,
    string Subject,
    string Body);

public sealed record NotificationDispatchResult(bool Success, string? FailureReason)
{
    public static NotificationDispatchResult Sent()
    {
        return new NotificationDispatchResult(true, null);
    }

    public static NotificationDispatchResult Failed(string reason)
    {
        return new NotificationDispatchResult(false, reason);
    }
}

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
