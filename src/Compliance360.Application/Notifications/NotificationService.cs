using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly INotificationDispatcher _dispatcher;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public NotificationService(
        INotificationRepository repository,
        INotificationDispatcher dispatcher,
        IApplicationDbContext dbContext,
        IClock clock)
    {
        _repository = repository;
        _dispatcher = dispatcher;
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<NotificationTemplateSummary>> CreateTemplateAsync(CreateNotificationTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _repository.GetTemplateAsync(command.TenantId, command.Code.ToUpperInvariant(), command.Channel, cancellationToken) is not null)
            {
                return Result<NotificationTemplateSummary>.Failure("Notification template already exists.");
            }

            var template = new NotificationTemplate(command.TenantId, command.Code, command.Channel, command.Subject, command.Body);
            await _repository.AddTemplateAsync(template, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationTemplate), template.Id, AuditAction.ConfigurationChanged, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<NotificationTemplateSummary>.Success(ToTemplateSummary(template));
        }
        catch (DomainException exception)
        {
            return Result<NotificationTemplateSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<NotificationMessageSummary>> QueueAsync(QueueNotificationCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var (subject, body, templateId) = await ResolveContentAsync(command, cancellationToken);
            var message = new NotificationMessage(command.TenantId, command.Channel, command.Recipient, subject, body, command.Priority, _clock.UtcNow);
            if (templateId.HasValue)
            {
                message.LinkTemplate(templateId.Value);
            }

            if (command.TargetUserId.HasValue)
            {
                message.TargetUser(command.TargetUserId.Value);
            }

            await _repository.AddMessageAsync(message, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationMessage), message.Id, AuditAction.NotificationQueued, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<NotificationMessageSummary>.Success(ToMessageSummary(message));
        }
        catch (DomainException exception)
        {
            return Result<NotificationMessageSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<NotificationMessageSummary>> SendAsync(SendNotificationCommand command, CancellationToken cancellationToken = default)
    {
        var message = await _repository.GetMessageAsync(command.TenantId, command.NotificationMessageId, cancellationToken);
        if (message is null)
        {
            return Result<NotificationMessageSummary>.Failure("Notification message not found.");
        }

        if (message.Status != NotificationStatus.Queued && message.Status != NotificationStatus.Failed)
        {
            return Result<NotificationMessageSummary>.Failure("Notification message is not dispatchable.");
        }

        var dispatchResult = await _dispatcher.DispatchAsync(
            new NotificationDispatchRequest(message.TenantId, message.Id, message.Channel, message.Recipient, message.Subject, message.Body),
            cancellationToken);

        if (dispatchResult.Success)
        {
            message.MarkSent(_clock.UtcNow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationMessage), message.Id, AuditAction.NotificationSent, true, null, cancellationToken);
        }
        else
        {
            message.MarkFailed(dispatchResult.FailureReason ?? "Notification dispatch failed.", _clock.UtcNow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationMessage), message.Id, AuditAction.NotificationFailed, false, message.FailureReason, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<NotificationMessageSummary>.Success(ToMessageSummary(message));
    }

    public async Task<Result> CancelAsync(CancelNotificationCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await _repository.GetMessageAsync(command.TenantId, command.NotificationMessageId, cancellationToken);
            if (message is null)
            {
                return Result.Failure("Notification message not found.");
            }

            message.Cancel();
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationMessage), message.Id, AuditAction.Updated, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private async Task<(string Subject, string Body, Guid? TemplateId)> ResolveContentAsync(QueueNotificationCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.TemplateCode))
        {
            var template = await _repository.GetTemplateAsync(command.TenantId, command.TemplateCode, command.Channel, cancellationToken)
                ?? throw new DomainException("Notification template not found.");
            if (!template.IsActive)
            {
                throw new DomainException("Notification template is inactive.");
            }

            return (Render(template.Subject, command.Variables), Render(template.Body, command.Variables), template.Id);
        }

        return (
            Guard.AgainstNullOrWhiteSpace(command.Subject, nameof(command.Subject), 250),
            Guard.AgainstNullOrWhiteSpace(command.Body, nameof(command.Body), 4_000),
            null);
    }

    private async Task AppendAuditAsync(Guid tenantId, Guid userId, string entityName, Guid entityId, AuditAction action, bool success, string? error, CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.FromEvent(
            new AuditEvent(
                entityName,
                entityId,
                action,
                AuditLog.InferCategory(action),
                new AuditContext(tenantId, userId, null, null, null, null, null, null, null),
                new AuditSnapshot(null, null),
                new AuditMetadata("{\"source\":\"notification\"}"),
                success,
                error),
            _clock.UtcNow);

        await _repository.AddAuditLogAsync(auditLog, cancellationToken);
    }

    private static string Render(string template, IReadOnlyDictionary<string, string> variables)
    {
        var rendered = template;
        foreach (var variable in variables)
        {
            rendered = rendered.Replace($"{{{{{variable.Key}}}}}", variable.Value, StringComparison.OrdinalIgnoreCase);
        }

        return rendered;
    }

    private static NotificationTemplateSummary ToTemplateSummary(NotificationTemplate template)
    {
        return new NotificationTemplateSummary(template.Id, template.TenantId, template.Code, template.Channel, template.Subject, template.IsActive);
    }

    private static NotificationMessageSummary ToMessageSummary(NotificationMessage message)
    {
        return new NotificationMessageSummary(
            message.Id,
            message.TenantId,
            message.Channel,
            message.Recipient,
            message.Subject,
            message.Priority,
            message.Status,
            message.QueuedAtUtc,
            message.SentAtUtc,
            message.FailureReason);
    }
}
