using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public interface IAlertOperationsService
{
    Task<Result<AlertOperationsDashboard>> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<AlertOperationsSearchResult>> SearchAsync(AlertOperationsQuery query, CancellationToken cancellationToken = default);
    Task<Result<AlertMessageDetail>> GetDetailAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken = default);
    Task<Result<NotificationMessageSummary>> ExecuteAsync(AlertMessageOperationCommand command, CancellationToken cancellationToken = default);
    Task<Result<string>> ExportCsvAsync(AlertOperationsQuery query, CancellationToken cancellationToken = default);
}

public interface IAlertOperationsRepository
{
    Task<AlertOperationsDashboard> GetDashboardAsync(Guid tenantId, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<NotificationMessage> Items, int Total)> SearchAsync(AlertOperationsQuery query, CancellationToken cancellationToken);
    Task<NotificationMessage?> GetMessageAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<NotificationDelivery>> ListDeliveriesAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<NotificationRetry>> ListRetriesAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<NotificationHistory>> ListHistoryAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken);
    Task<NotificationDeadLetter?> GetDeadLetterAsync(Guid tenantId, Guid messageId, CancellationToken cancellationToken);
}

public sealed record AlertOperationsQuery(
    Guid TenantId,
    string? Search,
    NotificationStatus? Status,
    NotificationChannel? Channel,
    NotificationProvider? Provider,
    Guid? AlertDefinitionId,
    Guid? AlertOccurrenceId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    int Page,
    int PageSize);

public sealed record AlertOperationsDashboard(
    long Pending,
    long Processing,
    long Sent,
    long Delivered,
    long Failed,
    long Retried,
    long Cancelled,
    long DeadLetters,
    double OldestBacklogMinutes,
    double AverageLatencyMilliseconds,
    long ThroughputLastHour,
    long ThroughputLast24Hours,
    int ActiveWorkers,
    DateTimeOffset? LastWorkerHeartbeatUtc);

public sealed record AlertOperationsMessageSummary(
    Guid Id,
    NotificationStatus Status,
    NotificationChannel Channel,
    NotificationPriority Priority,
    string Recipient,
    string Subject,
    NotificationProvider? Provider,
    int RetryCount,
    DateTimeOffset QueuedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? FailureReason,
    Guid? TemplateId,
    Guid? AlertDefinitionId,
    Guid? AlertDefinitionVersionId,
    Guid? AlertOccurrenceId);

public sealed record AlertOperationsSearchResult(IReadOnlyCollection<AlertOperationsMessageSummary> Items, int Total, int Page, int PageSize);

public sealed record AlertMessageDetail(
    AlertOperationsMessageSummary Message,
    IReadOnlyCollection<NotificationDeliverySummary> Deliveries,
    IReadOnlyCollection<NotificationRetrySummary> Retries,
    IReadOnlyCollection<NotificationHistorySummary> History,
    NotificationDeadLetterSummary? DeadLetter);

public sealed record NotificationDeliverySummary(NotificationProvider Provider, NotificationDeliveryStatus Status, string ProviderMessageId, DateTimeOffset OccurredAtUtc);
public sealed record NotificationRetrySummary(int Attempt, DateTimeOffset ScheduledAtUtc, DateTimeOffset? ExecutedAtUtc, string FailureReason);
public sealed record NotificationHistorySummary(NotificationDeliveryStatus Status, string Detail, DateTimeOffset OccurredAtUtc);

public sealed record AlertMessageOperationCommand(Guid TenantId, Guid MessageId, string Action, Guid RequestedByUserId);
