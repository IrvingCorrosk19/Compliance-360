using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public interface INotificationTemplateCenterService
{
    Task<Result<NotificationTemplateVersionDetail>> CreateTemplateAsync(CreateNotificationTemplateDefinitionCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationTemplateCenterPage>> SearchAsync(NotificationTemplateCenterQuery query, CancellationToken cancellationToken = default);

    Task<Result<NotificationTemplateVersionDetail>> GetVersionAsync(Guid tenantId, Guid versionId, CancellationToken cancellationToken = default);

    Task<Result<NotificationTemplateVersionDetail>> CreateVersionAsync(CreateNotificationTemplateVersionCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationTemplateVersionDetail>> DuplicateVersionAsync(DuplicateNotificationTemplateVersionCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationTemplateVersionDetail>> ApplyLifecycleActionAsync(NotificationTemplateLifecycleCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationRenderedTemplate>> PreviewVersionAsync(PreviewNotificationTemplateVersionCommand command, CancellationToken cancellationToken = default);

    Task<Result<NotificationMessageSummary>> SendTestAsync(SendNotificationTemplateTestCommand command, CancellationToken cancellationToken = default);
}

public interface INotificationTemplateCenterRepository
{
    Task<NotificationTemplate?> GetTemplateAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default);

    Task<NotificationTemplateVersion?> GetVersionAsync(Guid tenantId, Guid versionId, CancellationToken cancellationToken = default);

    Task<int> GetNextVersionAsync(Guid tenantId, Guid templateId, string locale, CancellationToken cancellationToken = default);

    Task AddVersionAsync(NotificationTemplateVersion version, CancellationToken cancellationToken = default);

    Task<(IReadOnlyCollection<NotificationTemplateCenterRecord> Items, long Total)> SearchAsync(NotificationTemplateCenterQuery query, CancellationToken cancellationToken = default);

    Task RetirePublishedVersionsAsync(Guid tenantId, Guid templateId, string locale, Guid exceptVersionId, DateTimeOffset nowUtc, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface INotificationContentSecurity
{
    string SanitizeHtml(string html);

    IReadOnlyCollection<string> ExtractVariables(params string?[] values);
}

public enum NotificationTemplateLifecycleAction
{
    SubmitForReview = 0,
    RecordReview = 1,
    Approve = 2,
    Publish = 3,
    Retire = 4,
    Archive = 5
}

public sealed record NotificationTemplateCenterQuery(
    Guid TenantId,
    string? Search,
    NotificationChannel? Channel,
    NotificationTemplateLifecycle? Lifecycle,
    string? Locale,
    int Page = 1,
    int PageSize = 25);

public sealed record CreateNotificationTemplateVersionCommand(
    Guid TenantId,
    Guid TemplateId,
    string Locale,
    string Subject,
    string HtmlBody,
    string? TextBody,
    string? BrandingJson,
    Guid RequestedByUserId);

public sealed record CreateNotificationTemplateDefinitionCommand(
    Guid TenantId,
    string Code,
    NotificationChannel Channel,
    string Locale,
    string Subject,
    string HtmlBody,
    string? TextBody,
    string? BrandingJson,
    Guid RequestedByUserId);

public sealed record DuplicateNotificationTemplateVersionCommand(
    Guid TenantId,
    Guid SourceVersionId,
    string Locale,
    Guid RequestedByUserId);

public sealed record NotificationTemplateLifecycleCommand(
    Guid TenantId,
    Guid VersionId,
    NotificationTemplateLifecycleAction Action,
    Guid RequestedByUserId);

public sealed record PreviewNotificationTemplateVersionCommand(
    Guid TenantId,
    Guid VersionId,
    IReadOnlyDictionary<string, string> Variables,
    TenantNotificationBranding? Branding);

public sealed record SendNotificationTemplateTestCommand(
    Guid TenantId,
    Guid VersionId,
    string Recipient,
    Guid? TargetUserId,
    IReadOnlyDictionary<string, string> Variables,
    Guid RequestedByUserId);

public sealed record NotificationTemplateCenterRecord(
    Guid TemplateId,
    string Code,
    NotificationChannel Channel,
    Guid VersionId,
    int Version,
    string Locale,
    string Subject,
    NotificationTemplateLifecycle Lifecycle,
    bool IsReviewed,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PublishedAtUtc);

public sealed record NotificationTemplateCenterPage(
    IReadOnlyCollection<NotificationTemplateCenterRecord> Items,
    long Total,
    int Page,
    int PageSize);

public sealed record NotificationTemplateVersionDetail(
    Guid Id,
    Guid TemplateId,
    int Version,
    string Locale,
    string Subject,
    string HtmlBody,
    string? TextBody,
    IReadOnlyCollection<string> Variables,
    string? BrandingJson,
    NotificationTemplateLifecycle Lifecycle,
    Guid CreatedByUserId,
    Guid? ReviewedByUserId,
    Guid? ApprovedByUserId,
    Guid? PublishedByUserId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PublishedAtUtc);
