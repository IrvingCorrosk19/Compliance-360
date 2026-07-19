using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public interface IAlertSchedulerService
{
    Task<Result<AlertScheduleSummary>> CreateAsync(CreateAlertScheduleCommand command, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyCollection<AlertScheduleSummary>>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<AlertScheduleSummary>> SetActiveAsync(ChangeAlertScheduleStateCommand command, CancellationToken cancellationToken = default);
    Result<AlertSchedulePreview> Preview(AlertSchedulePreviewCommand command);
}

public interface IAlertSchedulerProcessor
{
    Task<AlertSchedulerBatchResult> ProcessDueAsync(string workerId, CancellationToken cancellationToken = default);
}

public interface IAlertSchedulerRepository
{
    Task<AlertDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken);
    Task AddAsync(AlertSchedule schedule, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AlertSchedule>> ListAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<AlertSchedule?> GetAsync(Guid tenantId, Guid scheduleId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AlertSchedule>> ClaimDueAsync(string workerId, DateTimeOffset nowUtc, DateTimeOffset leaseUntilUtc, int batchSize, CancellationToken cancellationToken);
    Task<bool> ExecutionExistsAsync(Guid tenantId, Guid scheduleId, DateTimeOffset scheduledForUtc, CancellationToken cancellationToken);
    Task PersistBatchAsync(
        AlertSchedule schedule,
        IReadOnlyCollection<AlertScheduleExecution> executions,
        IReadOnlyCollection<AlertOccurrence> occurrences,
        IReadOnlyCollection<NotificationOutboxEvent> outboxEvents,
        CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record CreateAlertScheduleCommand(
    Guid TenantId,
    Guid DefinitionId,
    string Code,
    string Name,
    string CronExpression,
    string TimeZoneId,
    string BusinessCalendarJson,
    string QuietHoursJson,
    AlertScheduleCatchUpPolicy CatchUpPolicy,
    int MaxCatchUpExecutions,
    AlertScheduleDigest Digest,
    Guid RequestedByUserId);

public sealed record ChangeAlertScheduleStateCommand(Guid TenantId, Guid ScheduleId, bool IsActive, Guid RequestedByUserId);

public sealed record AlertSchedulePreviewCommand(
    string CronExpression,
    string TimeZoneId,
    string BusinessCalendarJson,
    string QuietHoursJson,
    DateTimeOffset FromUtc,
    int Count);

public sealed record AlertSchedulePreviewOccurrence(DateTimeOffset ScheduledAtUtc, DateTimeOffset EffectiveAtUtc, string Reason);

public sealed record AlertSchedulePreview(IReadOnlyCollection<AlertSchedulePreviewOccurrence> Occurrences);

public sealed record AlertScheduleSummary(
    Guid Id,
    Guid DefinitionId,
    string Code,
    string Name,
    string CronExpression,
    string TimeZoneId,
    AlertScheduleCatchUpPolicy CatchUpPolicy,
    AlertScheduleDigest Digest,
    bool IsActive,
    DateTimeOffset? LastExecutionAtUtc,
    DateTimeOffset NextScheduledAtUtc,
    DateTimeOffset NextExecutionAtUtc);

public sealed record AlertSchedulerBatchResult(int SchedulesClaimed, int ExecutionsCreated, int ExecutionsSkipped, int ExecutionsFailed);
