using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public interface IAlertEventProcessor
{
    Task<Result<int>> ProcessAsync(NotificationOutboxEvent outboxEvent, CancellationToken cancellationToken = default);
}

public interface IAlertEventIngestionService
{
    Task<Result<AlertEventIngestionResult>> IngestAsync(IngestAlertEventCommand command, CancellationToken cancellationToken = default);
}

public interface IAlertEventRepository
{
    Task<AlertOccurrence?> GetOccurrenceAsync(Guid tenantId, Guid occurrenceId, CancellationToken cancellationToken);
    Task<AlertDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken);
    Task<AlertDefinitionVersion?> GetVersionAsync(Guid tenantId, Guid definitionId, Guid versionId, CancellationToken cancellationToken);
    Task<NotificationTemplateVersion?> GetPublishedTemplateAsync(Guid tenantId, string code, NotificationChannel channel, string? locale, CancellationToken cancellationToken);
    Task<bool> HasRecentOccurrenceAsync(Guid tenantId, Guid definitionVersionId, string dedupeKey, Guid excludedOccurrenceId, DateTimeOffset sinceUtc, CancellationToken cancellationToken);
    Task<bool> MessageExistsAsync(Guid tenantId, string idempotencyKey, CancellationToken cancellationToken);
    Task<AlertEventType?> GetEventTypeAsync(Guid tenantId, string eventCode, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<(AlertDefinition Definition, AlertDefinitionVersion Version)>> ListPublishedRulesAsync(
        Guid tenantId,
        Guid eventTypeId,
        CancellationToken cancellationToken);
    Task AddOccurrencesAsync(
        IReadOnlyCollection<AlertOccurrence> occurrences,
        IReadOnlyCollection<NotificationOutboxEvent> outboxEvents,
        CancellationToken cancellationToken);
    Task CompleteOccurrenceIfTerminalAsync(Guid tenantId, Guid occurrenceId, DateTimeOffset completedAtUtc, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record AlertChannelPolicy(
    NotificationChannel Channel,
    string TemplateCode,
    string? Locale = null,
    bool Enabled = true);

public sealed record IngestAlertEventCommand(
    Guid TenantId,
    string EventCode,
    string PayloadJson,
    string SourceModule,
    string EntityType,
    Guid? EntityId,
    string CorrelationId,
    DateTimeOffset? OccurredAtUtc = null);

public sealed record AlertEventIngestionResult(Guid EventTypeId, int RulesMatchedForEvaluation, IReadOnlyCollection<Guid> OccurrenceIds);
