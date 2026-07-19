using System.Text.Json;
using Compliance360.Domain.Notifications;
using Microsoft.Extensions.Options;

namespace Compliance360.Application.Notifications;

public sealed class AlertSchedulerProcessor : IAlertSchedulerProcessor
{
    private readonly IAlertSchedulerRepository _repository;
    private readonly IAlertScheduleCalculator _calculator;
    private readonly IClock _clock;
    private readonly NotificationWorkerOptions _options;

    public AlertSchedulerProcessor(
        IAlertSchedulerRepository repository,
        IAlertScheduleCalculator calculator,
        IClock clock,
        IOptions<NotificationWorkerOptions> options)
    {
        _repository = repository;
        _calculator = calculator;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<AlertSchedulerBatchResult> ProcessDueAsync(
        string workerId,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var schedules = await _repository.ClaimDueAsync(
            workerId,
            now,
            now.AddSeconds(Math.Clamp(_options.LeaseDurationSeconds, 10, 3_600)),
            Math.Clamp(_options.BatchSize, 1, 500),
            cancellationToken);
        var created = 0;
        var skipped = 0;
        var failed = 0;

        foreach (var schedule in schedules)
        {
            try
            {
                var definition = await _repository.GetDefinitionAsync(schedule.TenantId, schedule.DefinitionId, cancellationToken);
                var due = CalculateDue(schedule, now, out var nextScheduled, out var nextEffective);
                var executions = new List<AlertScheduleExecution>();
                var occurrences = new List<AlertOccurrence>();
                var outbox = new List<NotificationOutboxEvent>();
                var selected = SelectExecutions(schedule, due);

                foreach (var dueOccurrence in due.Except(selected))
                {
                    if (await _repository.ExecutionExistsAsync(schedule.TenantId, schedule.Id, dueOccurrence.ScheduledAtUtc, cancellationToken))
                    {
                        continue;
                    }

                    var execution = new AlertScheduleExecution(schedule.TenantId, schedule.Id, dueOccurrence.ScheduledAtUtc, workerId, now);
                    execution.Skip("Catch-up policy skipped this scheduled execution.", now);
                    executions.Add(execution);
                    skipped++;
                }

                foreach (var dueOccurrence in selected)
                {
                    if (await _repository.ExecutionExistsAsync(schedule.TenantId, schedule.Id, dueOccurrence.ScheduledAtUtc, cancellationToken))
                    {
                        continue;
                    }

                    var execution = new AlertScheduleExecution(schedule.TenantId, schedule.Id, dueOccurrence.ScheduledAtUtc, workerId, now);
                    if (definition is null
                        || definition.Lifecycle != AlertDefinitionLifecycle.Published
                        || !definition.CurrentPublishedVersionId.HasValue)
                    {
                        execution.Skip("Alert definition does not have an active published version.", now);
                        executions.Add(execution);
                        skipped++;
                        continue;
                    }

                    var correlationId = $"schedule:{schedule.Id:N}:{dueOccurrence.ScheduledAtUtc.UtcTicks}";
                    var payload = JsonSerializer.Serialize(new
                    {
                        scheduleId = schedule.Id,
                        scheduleCode = schedule.Code,
                        definitionId = definition.Id,
                        scheduledAtUtc = dueOccurrence.ScheduledAtUtc,
                        effectiveAtUtc = dueOccurrence.EffectiveAtUtc,
                        digest = schedule.Digest.ToString()
                    });
                    var occurrence = new AlertOccurrence(
                        schedule.TenantId,
                        definition.Id,
                        definition.CurrentPublishedVersionId.Value,
                        definition.EventTypeId,
                        $"{schedule.Id:N}:{dueOccurrence.ScheduledAtUtc.UtcTicks}",
                        payload,
                        correlationId,
                        "AlertCenter.Scheduler",
                        nameof(AlertSchedule),
                        schedule.Id,
                        dueOccurrence.ScheduledAtUtc);
                    var outboxEvent = new NotificationOutboxEvent(
                        schedule.TenantId,
                        "AlertOccurrenceCreated",
                        nameof(AlertOccurrence),
                        occurrence.Id,
                        payload,
                        correlationId,
                        now);
                    execution.Complete(occurrence.Id, now);
                    executions.Add(execution);
                    occurrences.Add(occurrence);
                    outbox.Add(outboxEvent);
                    created++;
                }

                schedule.Advance(due.Last().ScheduledAtUtc, nextScheduled, nextEffective, now);
                await _repository.PersistBatchAsync(schedule, executions, occurrences, outbox, cancellationToken);
            }
            catch
            {
                failed++;
            }
        }

        return new AlertSchedulerBatchResult(schedules.Count, created, skipped, failed);
    }

    private IReadOnlyCollection<AlertSchedulePreviewOccurrence> CalculateDue(
        AlertSchedule schedule,
        DateTimeOffset nowUtc,
        out DateTimeOffset nextScheduled,
        out DateTimeOffset nextEffective)
    {
        var due = new List<AlertSchedulePreviewOccurrence>();
        var cursor = schedule.NextScheduledAtUtc;
        for (var guard = 0; guard < 10_000; guard++)
        {
            var effective = _calculator.ApplyCalendar(
                cursor,
                schedule.TimeZoneId,
                schedule.BusinessCalendarJson,
                schedule.QuietHoursJson);
            if (effective.EffectiveAtUtc > nowUtc)
            {
                nextScheduled = cursor;
                nextEffective = effective.EffectiveAtUtc;
                return due;
            }

            due.Add(effective);
            cursor = _calculator.NextOccurrence(schedule.CronExpression, schedule.TimeZoneId, cursor);
        }

        throw new InvalidOperationException("Alert schedule catch-up exceeds 10,000 occurrences.");
    }

    private static IReadOnlyCollection<AlertSchedulePreviewOccurrence> SelectExecutions(
        AlertSchedule schedule,
        IReadOnlyCollection<AlertSchedulePreviewOccurrence> due)
    {
        if (due.Count == 0)
        {
            return [];
        }

        return schedule.CatchUpPolicy switch
        {
            AlertScheduleCatchUpPolicy.Skip when due.Count > 1 => [],
            AlertScheduleCatchUpPolicy.RunLatest => [due.Last()],
            AlertScheduleCatchUpPolicy.RunAll => due.Take(schedule.MaxCatchUpExecutions).ToArray(),
            _ => [due.First()]
        };
    }
}
