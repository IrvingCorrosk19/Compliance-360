using System.Text.Json;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public sealed class NotificationTemplateCenterService : INotificationTemplateCenterService
{
    private readonly INotificationTemplateCenterRepository _repository;
    private readonly INotificationContentSecurity _contentSecurity;
    private readonly INotificationTemplateEngine _templateEngine;
    private readonly INotificationService _notificationService;
    private readonly INotificationAuditService _auditService;
    private readonly IClock _clock;

    public NotificationTemplateCenterService(
        INotificationTemplateCenterRepository repository,
        INotificationContentSecurity contentSecurity,
        INotificationTemplateEngine templateEngine,
        INotificationService notificationService,
        INotificationAuditService auditService,
        IClock clock)
    {
        _repository = repository;
        _contentSecurity = contentSecurity;
        _templateEngine = templateEngine;
        _notificationService = notificationService;
        _auditService = auditService;
        _clock = clock;
    }

    public async Task<Result<NotificationTemplateVersionDetail>> CreateTemplateAsync(
        CreateNotificationTemplateDefinitionCommand command,
        CancellationToken cancellationToken = default)
    {
        var sanitizedHtml = _contentSecurity.SanitizeHtml(command.HtmlBody);
        var created = await _notificationService.CreateTemplateAsync(
            new CreateNotificationTemplateCommand(
                command.TenantId,
                command.RequestedByUserId,
                command.Code,
                command.Channel,
                command.Subject,
                sanitizedHtml,
                command.TextBody,
                command.Locale,
                command.BrandingJson),
            cancellationToken);
        if (created.IsFailure)
        {
            return Result<NotificationTemplateVersionDetail>.Failure(created.Error ?? "Notification template could not be created.");
        }

        return await CreateVersionAsync(
            new CreateNotificationTemplateVersionCommand(
                command.TenantId,
                created.Value!.Id,
                command.Locale,
                command.Subject,
                sanitizedHtml,
                command.TextBody,
                command.BrandingJson,
                command.RequestedByUserId),
            cancellationToken);
    }

    public async Task<Result<NotificationTemplateCenterPage>> SearchAsync(NotificationTemplateCenterQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = query with
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            Locale = string.IsNullOrWhiteSpace(query.Locale) ? null : query.Locale.Trim(),
            Page = Math.Max(1, query.Page),
            PageSize = Math.Clamp(query.PageSize, 1, 100)
        };
        var (items, total) = await _repository.SearchAsync(normalized, cancellationToken);
        return Result<NotificationTemplateCenterPage>.Success(new NotificationTemplateCenterPage(items, total, normalized.Page, normalized.PageSize));
    }

    public async Task<Result<NotificationTemplateVersionDetail>> GetVersionAsync(Guid tenantId, Guid versionId, CancellationToken cancellationToken = default)
    {
        var version = await _repository.GetVersionAsync(tenantId, versionId, cancellationToken);
        return version is null
            ? Result<NotificationTemplateVersionDetail>.Failure("Template version not found.")
            : Result<NotificationTemplateVersionDetail>.Success(ToDetail(version));
    }

    public async Task<Result<NotificationTemplateVersionDetail>> CreateVersionAsync(CreateNotificationTemplateVersionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _repository.GetTemplateAsync(command.TenantId, command.TemplateId, cancellationToken);
            if (template is null)
            {
                return Result<NotificationTemplateVersionDetail>.Failure("Notification template not found.");
            }

            var sanitizedHtml = _contentSecurity.SanitizeHtml(command.HtmlBody);
            var variables = _contentSecurity.ExtractVariables(command.Subject, sanitizedHtml, command.TextBody);
            var nextVersion = await _repository.GetNextVersionAsync(command.TenantId, command.TemplateId, command.Locale, cancellationToken);
            var version = new NotificationTemplateVersion(
                command.TenantId,
                command.TemplateId,
                nextVersion,
                command.Locale,
                command.Subject,
                sanitizedHtml,
                command.TextBody,
                JsonSerializer.Serialize(variables),
                command.BrandingJson,
                command.RequestedByUserId,
                _clock.UtcNow);
            await _repository.AddVersionAsync(version, cancellationToken);
            await _auditService.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationTemplateVersion), version.Id, AuditAction.NotificationTemplateCreated, true, null, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result<NotificationTemplateVersionDetail>.Success(ToDetail(version));
        }
        catch (DomainException exception)
        {
            return Result<NotificationTemplateVersionDetail>.Failure(exception.Message);
        }
    }

    public async Task<Result<NotificationTemplateVersionDetail>> DuplicateVersionAsync(DuplicateNotificationTemplateVersionCommand command, CancellationToken cancellationToken = default)
    {
        var source = await _repository.GetVersionAsync(command.TenantId, command.SourceVersionId, cancellationToken);
        if (source is null)
        {
            return Result<NotificationTemplateVersionDetail>.Failure("Source template version not found.");
        }

        return await CreateVersionAsync(
            new CreateNotificationTemplateVersionCommand(
                command.TenantId,
                source.NotificationTemplateId,
                command.Locale,
                source.Subject,
                source.HtmlBody,
                source.TextBody,
                source.BrandingJson,
                command.RequestedByUserId),
            cancellationToken);
    }

    public async Task<Result<NotificationTemplateVersionDetail>> ApplyLifecycleActionAsync(NotificationTemplateLifecycleCommand command, CancellationToken cancellationToken = default)
    {
        var version = await _repository.GetVersionAsync(command.TenantId, command.VersionId, cancellationToken);
        if (version is null)
        {
            return Result<NotificationTemplateVersionDetail>.Failure("Template version not found.");
        }

        try
        {
            switch (command.Action)
            {
                case NotificationTemplateLifecycleAction.SubmitForReview:
                    version.SubmitForReview(_clock.UtcNow);
                    break;
                case NotificationTemplateLifecycleAction.RecordReview:
                    version.RecordReview(command.RequestedByUserId, _clock.UtcNow);
                    break;
                case NotificationTemplateLifecycleAction.Approve:
                    version.Approve(command.RequestedByUserId, _clock.UtcNow);
                    break;
                case NotificationTemplateLifecycleAction.Publish:
                    await _repository.RetirePublishedVersionsAsync(command.TenantId, version.NotificationTemplateId, version.Locale, version.Id, _clock.UtcNow, cancellationToken);
                    version.Publish(command.RequestedByUserId, _clock.UtcNow);
                    break;
                case NotificationTemplateLifecycleAction.Retire:
                    version.Retire(_clock.UtcNow);
                    break;
                case NotificationTemplateLifecycleAction.Archive:
                    version.Archive(_clock.UtcNow);
                    break;
                default:
                    return Result<NotificationTemplateVersionDetail>.Failure("Unsupported template lifecycle action.");
            }

            await _auditService.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationTemplateVersion), version.Id, AuditAction.NotificationTemplateUpdated, true, null, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result<NotificationTemplateVersionDetail>.Success(ToDetail(version));
        }
        catch (DomainException exception)
        {
            return Result<NotificationTemplateVersionDetail>.Failure(exception.Message);
        }
    }

    public async Task<Result<NotificationRenderedTemplate>> PreviewVersionAsync(PreviewNotificationTemplateVersionCommand command, CancellationToken cancellationToken = default)
    {
        var version = await _repository.GetVersionAsync(command.TenantId, command.VersionId, cancellationToken);
        if (version is null)
        {
            return Result<NotificationRenderedTemplate>.Failure("Template version not found.");
        }

        return Result<NotificationRenderedTemplate>.Success(_templateEngine.Render(
            version.Subject,
            version.HtmlBody,
            version.TextBody,
            command.Variables,
            command.Branding));
    }

    public async Task<Result<NotificationMessageSummary>> SendTestAsync(SendNotificationTemplateTestCommand command, CancellationToken cancellationToken = default)
    {
        var version = await _repository.GetVersionAsync(command.TenantId, command.VersionId, cancellationToken);
        if (version is null)
        {
            return Result<NotificationMessageSummary>.Failure("Template version not found.");
        }

        var template = await _repository.GetTemplateAsync(command.TenantId, version.NotificationTemplateId, cancellationToken);
        if (template is null)
        {
            return Result<NotificationMessageSummary>.Failure("Notification template not found.");
        }

        var rendered = _templateEngine.Render(version.Subject, version.HtmlBody, version.TextBody, command.Variables, null);
        return await _notificationService.QueueAsync(
            new QueueNotificationCommand(
                command.TenantId,
                command.RequestedByUserId,
                template.Channel,
                command.Recipient,
                rendered.Subject,
                rendered.HtmlBody,
                null,
                new Dictionary<string, string>(),
                NotificationPriority.Normal,
                command.TargetUserId),
            cancellationToken);
    }

    private static NotificationTemplateVersionDetail ToDetail(NotificationTemplateVersion version)
    {
        IReadOnlyCollection<string> variables;
        try
        {
            variables = JsonSerializer.Deserialize<string[]>(version.VariablesJson) ?? [];
        }
        catch (JsonException)
        {
            variables = [];
        }

        return new NotificationTemplateVersionDetail(
            version.Id,
            version.NotificationTemplateId,
            version.Version,
            version.Locale,
            version.Subject,
            version.HtmlBody,
            version.TextBody,
            variables,
            version.BrandingJson,
            version.Lifecycle,
            version.CreatedByUserId,
            version.ReviewedByUserId,
            version.ApprovedByUserId,
            version.PublishedByUserId,
            version.CreatedAtUtc,
            version.PublishedAtUtc);
    }
}
