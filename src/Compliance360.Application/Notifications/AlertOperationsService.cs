using System.Text;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public sealed class AlertOperationsService : IAlertOperationsService
{
    private readonly IAlertOperationsRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;

    public AlertOperationsService(
        IAlertOperationsRepository repository,
        INotificationService notificationService,
        IClock clock)
    {
        _repository = repository;
        _notificationService = notificationService;
        _clock = clock;
    }

    public async Task<Result<AlertOperationsDashboard>> GetDashboardAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        Result<AlertOperationsDashboard>.Success(
            await _repository.GetDashboardAsync(tenantId, _clock.UtcNow, cancellationToken));

    public async Task<Result<AlertOperationsSearchResult>> SearchAsync(
        AlertOperationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalized = query with { Page = Math.Max(1, query.Page), PageSize = Math.Clamp(query.PageSize, 1, 500) };
        var (items, total) = await _repository.SearchAsync(normalized, cancellationToken);
        return Result<AlertOperationsSearchResult>.Success(
            new AlertOperationsSearchResult(items.Select(Map).ToArray(), total, normalized.Page, normalized.PageSize));
    }

    public async Task<Result<AlertMessageDetail>> GetDetailAsync(
        Guid tenantId,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var message = await _repository.GetMessageAsync(tenantId, messageId, cancellationToken);
        if (message is null)
        {
            return Result<AlertMessageDetail>.Failure("Notification message was not found.");
        }

        var deliveriesTask = _repository.ListDeliveriesAsync(tenantId, messageId, cancellationToken);
        var retriesTask = _repository.ListRetriesAsync(tenantId, messageId, cancellationToken);
        var historyTask = _repository.ListHistoryAsync(tenantId, messageId, cancellationToken);
        var deadLetterTask = _repository.GetDeadLetterAsync(tenantId, messageId, cancellationToken);
        await Task.WhenAll(deliveriesTask, retriesTask, historyTask, deadLetterTask);
        var deadLetter = await deadLetterTask;
        return Result<AlertMessageDetail>.Success(new AlertMessageDetail(
            Map(message),
            (await deliveriesTask).Select(item => new NotificationDeliverySummary(item.Provider, item.Status, item.ProviderMessageId, item.OccurredAtUtc)).ToArray(),
            (await retriesTask).Select(item => new NotificationRetrySummary(item.Attempt, item.ScheduledAtUtc, item.ExecutedAtUtc, item.FailureReason)).ToArray(),
            (await historyTask).Select(item => new NotificationHistorySummary(item.Status, item.EventName, item.OccurredAtUtc)).ToArray(),
            deadLetter is null
                ? null
                : new NotificationDeadLetterSummary(deadLetter.Id, deadLetter.TenantId, deadLetter.NotificationMessageId, deadLetter.Reason, deadLetter.DeadLetteredAtUtc)));
    }

    public async Task<Result<NotificationMessageSummary>> ExecuteAsync(
        AlertMessageOperationCommand command,
        CancellationToken cancellationToken = default)
    {
        var message = await _repository.GetMessageAsync(command.TenantId, command.MessageId, cancellationToken);
        if (message is null)
        {
            return Result<NotificationMessageSummary>.Failure("Notification message was not found.");
        }

        switch (command.Action.Trim().ToLowerInvariant())
        {
            case "retry":
                return await _notificationService.RetryAsync(
                    new RetryNotificationCommand(command.TenantId, command.MessageId, command.RequestedByUserId),
                    cancellationToken);
            case "cancel":
            {
                var cancelled = await _notificationService.CancelAsync(
                    new CancelNotificationCommand(command.TenantId, command.MessageId, command.RequestedByUserId),
                    cancellationToken);
                if (cancelled.IsFailure)
                {
                    return Result<NotificationMessageSummary>.Failure(cancelled.Error ?? "Notification could not be cancelled.");
                }

                var updated = await _repository.GetMessageAsync(command.TenantId, command.MessageId, cancellationToken);
                return Result<NotificationMessageSummary>.Success(ToLegacySummary(updated!));
            }
            case "resend":
            case "reprocess":
                return await _notificationService.QueueAsync(
                    new QueueNotificationCommand(
                        command.TenantId,
                        command.RequestedByUserId,
                        message.Channel,
                        message.Recipient,
                        message.Subject,
                        message.Body,
                        null,
                        new Dictionary<string, string>(),
                        message.Priority,
                        message.TargetUserId,
                        message.AlertOccurrenceId,
                        message.AlertDefinitionId,
                        message.AlertDefinitionVersionId),
                    cancellationToken);
            default:
                return Result<NotificationMessageSummary>.Failure("Unsupported operational action.");
        }
    }

    public async Task<Result<string>> ExportCsvAsync(
        AlertOperationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var builder = new StringBuilder();
        builder.AppendLine("MessageId,Status,Channel,Priority,Recipient,Subject,Provider,RetryCount,QueuedAtUtc,CompletedAtUtc,DefinitionId,OccurrenceId,FailureReason");
        var page = 1;
        var exported = 0;
        while (exported < 10_000)
        {
            var batchQuery = query with { Page = page, PageSize = Math.Min(500, 10_000 - exported) };
            var (items, total) = await _repository.SearchAsync(batchQuery, cancellationToken);
            foreach (var item in items)
            {
                var row = Map(item);
                builder.AppendLine(string.Join(",", new[]
                {
                    Csv(row.Id.ToString()),
                    Csv(row.Status.ToString()),
                    Csv(row.Channel.ToString()),
                    Csv(row.Priority.ToString()),
                    Csv(row.Recipient),
                    Csv(row.Subject),
                    Csv(row.Provider?.ToString()),
                    Csv(row.RetryCount.ToString()),
                    Csv(row.QueuedAtUtc.ToString("O")),
                    Csv(row.CompletedAtUtc?.ToString("O")),
                    Csv(row.AlertDefinitionId?.ToString()),
                    Csv(row.AlertOccurrenceId?.ToString()),
                    Csv(row.FailureReason)
                }));
                exported++;
            }

            if (items.Count == 0 || exported >= total)
            {
                break;
            }
            page++;
        }

        return Result<string>.Success(builder.ToString());
    }

    private static AlertOperationsMessageSummary Map(NotificationMessage message) =>
        new(
            message.Id,
            message.Status,
            message.Channel,
            message.Priority,
            message.Recipient,
            message.Subject,
            message.LastProvider,
            message.RetryCount,
            message.QueuedAtUtc,
            message.CompletedAtUtc,
            message.FailureReason,
            message.TemplateId,
            message.AlertDefinitionId,
            message.AlertDefinitionVersionId,
            message.AlertOccurrenceId);

    private static NotificationMessageSummary ToLegacySummary(NotificationMessage message) =>
        new(
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

    private static string Csv(string? value)
    {
        var safe = value ?? string.Empty;
        if (safe.Length > 0 && "=+-@".Contains(safe[0]))
        {
            safe = $"'{safe}";
        }
        return $"\"{safe.Replace("\"", "\"\"")}\"";
    }
}
