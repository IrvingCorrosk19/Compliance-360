using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly INotificationDispatcher _dispatcher;
    private readonly INotificationTemplateEngine _templateEngine;
    private readonly INotificationRetryService _retryService;
    private readonly INotificationTrackingService _trackingService;
    private readonly INotificationAuditService _auditService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public NotificationService(
        INotificationRepository repository,
        INotificationDispatcher dispatcher,
        INotificationTemplateEngine templateEngine,
        INotificationRetryService retryService,
        INotificationTrackingService trackingService,
        INotificationAuditService auditService,
        IApplicationDbContext dbContext,
        IClock clock)
    {
        _repository = repository;
        _dispatcher = dispatcher;
        _templateEngine = templateEngine;
        _retryService = retryService;
        _trackingService = trackingService;
        _auditService = auditService;
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
            template.ConfigureEnterpriseContent(command.TextBody, command.Locale, command.BrandingJson);
            await _repository.AddTemplateAsync(template, cancellationToken);
            await _auditService.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationTemplate), template.Id, AuditAction.NotificationTemplateCreated, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<NotificationTemplateSummary>.Success(ToTemplateSummary(template));
        }
        catch (DomainException exception)
        {
            return Result<NotificationTemplateSummary>.Failure(exception.Message);
        }
    }

    public Task<Result<NotificationTemplateSummary>> PreviewTemplateAsync(PreviewNotificationTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var rendered = _templateEngine.Render(command.Subject, command.Body, command.TextBody, command.Variables, command.Branding);
        var preview = new NotificationTemplate(command.TenantId, "PREVIEW", NotificationChannel.Email, rendered.Subject, rendered.HtmlBody);
        preview.ConfigureEnterpriseContent(rendered.TextBody, null, null);
        return Task.FromResult(Result<NotificationTemplateSummary>.Success(ToTemplateSummary(preview)));
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
            await _trackingService.TrackAsync(command.TenantId, message.Id, NotificationDeliveryStatus.Queued, "Queued", cancellationToken);
            await _auditService.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationMessage), message.Id, AuditAction.NotificationQueued, true, null, cancellationToken);
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

        if (message.Status != NotificationStatus.Queued && message.Status != NotificationStatus.Failed && message.Status != NotificationStatus.Retried)
        {
            return Result<NotificationMessageSummary>.Failure("Notification message is not dispatchable.");
        }

        var dispatchResult = await _dispatcher.DispatchAsync(
            new NotificationDispatchRequest(message.TenantId, message.Id, message.Channel, message.Recipient, message.Subject, message.Body, message.TextBody, message.Priority),
            cancellationToken);

        if (dispatchResult.Success)
        {
            message.MarkSent(_clock.UtcNow);
            await _repository.AddDeliveryAsync(new NotificationDelivery(message.TenantId, message.Id, dispatchResult.Provider ?? NotificationProvider.Smtp, NotificationDeliveryStatus.Sent, dispatchResult.ProviderMessageId, _clock.UtcNow), cancellationToken);
            await _trackingService.TrackAsync(command.TenantId, message.Id, NotificationDeliveryStatus.Sent, "Sent", cancellationToken);
            await _auditService.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationMessage), message.Id, AuditAction.NotificationSent, true, null, cancellationToken);
        }
        else
        {
            var reason = dispatchResult.FailureReason ?? "Notification dispatch failed.";
            message.MarkFailed(reason, _clock.UtcNow, dispatchResult.Provider);
            await _repository.AddDeliveryAsync(new NotificationDelivery(message.TenantId, message.Id, dispatchResult.Provider ?? NotificationProvider.Smtp, NotificationDeliveryStatus.Failed, null, _clock.UtcNow), cancellationToken);
            await _trackingService.TrackAsync(command.TenantId, message.Id, NotificationDeliveryStatus.Failed, "Failed", cancellationToken);
            if (_retryService.ShouldDeadLetter(message.RetryCount))
            {
                message.MoveToDeadLetter(reason, _clock.UtcNow);
                await _repository.AddDeadLetterAsync(new NotificationDeadLetter(message.TenantId, message.Id, reason, $"{{\"messageId\":\"{message.Id}\"}}", _clock.UtcNow), cancellationToken);
                await _trackingService.TrackAsync(command.TenantId, message.Id, NotificationDeliveryStatus.DeadLetter, "DeadLetter", cancellationToken);
            }
            else
            {
                var nextRetryAt = _retryService.GetNextRetryAt(_clock.UtcNow, message.RetryCount);
                message.MarkRetried(nextRetryAt);
                await _repository.AddRetryAsync(new NotificationRetry(message.TenantId, message.Id, message.RetryCount, nextRetryAt, reason), cancellationToken);
                await _trackingService.TrackAsync(command.TenantId, message.Id, NotificationDeliveryStatus.Retried, "Retried", cancellationToken);
            }

            await _auditService.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationMessage), message.Id, AuditAction.NotificationFailed, false, message.FailureReason, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<NotificationMessageSummary>.Success(ToMessageSummary(message));
    }

    public Task<Result<NotificationMessageSummary>> RetryAsync(RetryNotificationCommand command, CancellationToken cancellationToken = default)
    {
        return SendAsync(new SendNotificationCommand(command.TenantId, command.NotificationMessageId, command.RequestedByUserId), cancellationToken);
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
            await _trackingService.TrackAsync(command.TenantId, message.Id, NotificationDeliveryStatus.Cancelled, "Cancelled", cancellationToken);
            await _auditService.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationMessage), message.Id, AuditAction.NotificationCancelled, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result<NotificationDashboardSummary>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var messages = await _repository.ListMessagesAsync(tenantId, cancellationToken);
        var deadLetters = await _repository.ListDeadLettersAsync(tenantId, cancellationToken);
        var sent = messages.LongCount(message => message.Status is NotificationStatus.Sent or NotificationStatus.Delivered);
        var delivered = messages.LongCount(message => message.Status == NotificationStatus.Delivered);
        var failed = messages.LongCount(message => message.Status is NotificationStatus.Failed or NotificationStatus.DeadLetter);
        var total = Math.Max(1, messages.Count);
        var providerConfigurations = await _repository.ListProviderConfigurationsAsync(tenantId, cancellationToken);
        return Result<NotificationDashboardSummary>.Success(new NotificationDashboardSummary(
            sent,
            delivered,
            failed,
            messages.Sum(message => message.RetryCount),
            deadLetters.Count,
            Math.Round((double)sent / total * 100, 2),
            providerConfigurations.ToDictionary(configuration => configuration.Provider, configuration => configuration.IsEnabled),
            new Dictionary<Guid, long> { [tenantId] = messages.Count }));
    }

    public async Task<Result<IReadOnlyCollection<NotificationMessageSummary>>> GetHistoryAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var messages = await _repository.ListMessagesAsync(tenantId, cancellationToken);
        return Result<IReadOnlyCollection<NotificationMessageSummary>>.Success(messages.Select(ToMessageSummary).ToArray());
    }

    public async Task<Result<IReadOnlyCollection<NotificationDeadLetterSummary>>> GetDeadLettersAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var deadLetters = await _repository.ListDeadLettersAsync(tenantId, cancellationToken);
        return Result<IReadOnlyCollection<NotificationDeadLetterSummary>>.Success(deadLetters.Select(deadLetter => new NotificationDeadLetterSummary(deadLetter.Id, deadLetter.TenantId, deadLetter.NotificationMessageId, deadLetter.Reason, deadLetter.DeadLetteredAtUtc)).ToArray());
    }

    public async Task<Result<NotificationProviderConfigurationSummary>> ConfigureProviderAsync(ConfigureNotificationProviderCommand command, CancellationToken cancellationToken = default)
    {
        var configuration = new NotificationProviderConfiguration(command.TenantId, command.Provider, command.Name, command.Priority, command.IsDefault, command.IsEnabled);
        await _repository.AddProviderConfigurationAsync(configuration, cancellationToken);
        await _auditService.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(NotificationProviderConfiguration), configuration.Id, AuditAction.NotificationProviderChanged, true, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<NotificationProviderConfigurationSummary>.Success(ToProviderConfigurationSummary(configuration));
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

            var rendered = _templateEngine.Render(template.Subject, template.Body, template.TextBody, command.Variables, null);
            return (rendered.Subject, rendered.HtmlBody, template.Id);
        }

        return (
            Guard.AgainstNullOrWhiteSpace(command.Subject, nameof(command.Subject), 250),
            Guard.AgainstNullOrWhiteSpace(command.Body, nameof(command.Body), 4_000),
            null);
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

    private static NotificationProviderConfigurationSummary ToProviderConfigurationSummary(NotificationProviderConfiguration configuration)
    {
        return new NotificationProviderConfigurationSummary(configuration.Id, configuration.TenantId, configuration.Provider, configuration.Name, configuration.Priority, configuration.IsDefault, configuration.IsEnabled);
    }
}
