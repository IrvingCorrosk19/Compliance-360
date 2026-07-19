using Compliance360.Domain.Common;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public sealed class AlertSchedulerService : IAlertSchedulerService
{
    private readonly IAlertSchedulerRepository _repository;
    private readonly IAlertScheduleCalculator _calculator;
    private readonly IClock _clock;
    private readonly INotificationAuditService? _audit;

    public AlertSchedulerService(
        IAlertSchedulerRepository repository,
        IAlertScheduleCalculator calculator,
        IClock clock,
        INotificationAuditService? audit = null)
    {
        _repository = repository;
        _calculator = calculator;
        _clock = clock;
        _audit = audit;
    }

    public async Task<Result<AlertScheduleSummary>> CreateAsync(
        CreateAlertScheduleCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var definition = await _repository.GetDefinitionAsync(command.TenantId, command.DefinitionId, cancellationToken);
            if (definition is null
                || definition.Lifecycle != AlertDefinitionLifecycle.Published
                || !definition.CurrentPublishedVersionId.HasValue)
            {
                return Result<AlertScheduleSummary>.Failure("Published alert definition was not found for this tenant.");
            }

            var next = _calculator.NextOccurrence(command.CronExpression, command.TimeZoneId, _clock.UtcNow);
            var effective = _calculator.ApplyCalendar(next, command.TimeZoneId, command.BusinessCalendarJson, command.QuietHoursJson);
            var schedule = new AlertSchedule(
                command.TenantId,
                command.DefinitionId,
                command.Code,
                command.Name,
                command.CronExpression,
                command.TimeZoneId,
                command.BusinessCalendarJson,
                command.QuietHoursJson,
                command.CatchUpPolicy,
                command.MaxCatchUpExecutions,
                command.Digest,
                next,
                effective.EffectiveAtUtc,
                command.RequestedByUserId);
            await _repository.AddAsync(schedule, cancellationToken);
            if (_audit is not null)
            {
                await _audit.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(AlertSchedule), schedule.Id, AuditAction.AlertConfigurationChanged, true, null, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
            }
            return Result<AlertScheduleSummary>.Success(Map(schedule));
        }
        catch (Exception exception) when (exception is DomainException or InvalidOperationException or TimeZoneNotFoundException or FormatException)
        {
            return Result<AlertScheduleSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<IReadOnlyCollection<AlertScheduleSummary>>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var schedules = await _repository.ListAsync(tenantId, cancellationToken);
        return Result<IReadOnlyCollection<AlertScheduleSummary>>.Success(schedules.Select(Map).ToArray());
    }

    public async Task<Result<AlertScheduleSummary>> SetActiveAsync(
        ChangeAlertScheduleStateCommand command,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _repository.GetAsync(command.TenantId, command.ScheduleId, cancellationToken);
        if (schedule is null)
        {
            return Result<AlertScheduleSummary>.Failure("Alert schedule was not found.");
        }

        if (command.IsActive)
        {
            var next = _calculator.NextOccurrence(schedule.CronExpression, schedule.TimeZoneId, _clock.UtcNow);
            var effective = _calculator.ApplyCalendar(next, schedule.TimeZoneId, schedule.BusinessCalendarJson, schedule.QuietHoursJson);
            schedule.Enable(next, effective.EffectiveAtUtc, _clock.UtcNow);
        }
        else
        {
            schedule.Disable(_clock.UtcNow);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        if (_audit is not null)
        {
            await _audit.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(AlertSchedule), schedule.Id, AuditAction.AlertConfigurationChanged, true, command.IsActive ? "Enabled" : "Disabled", cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }
        return Result<AlertScheduleSummary>.Success(Map(schedule));
    }

    public Result<AlertSchedulePreview> Preview(AlertSchedulePreviewCommand command)
    {
        try
        {
            var count = Math.Clamp(command.Count, 1, 100);
            var cursor = command.FromUtc;
            var results = new List<AlertSchedulePreviewOccurrence>(count);
            for (var index = 0; index < count; index++)
            {
                var scheduled = _calculator.NextOccurrence(command.CronExpression, command.TimeZoneId, cursor);
                results.Add(_calculator.ApplyCalendar(
                    scheduled,
                    command.TimeZoneId,
                    command.BusinessCalendarJson,
                    command.QuietHoursJson));
                cursor = scheduled;
            }

            return Result<AlertSchedulePreview>.Success(new AlertSchedulePreview(results));
        }
        catch (Exception exception) when (exception is InvalidOperationException or TimeZoneNotFoundException or FormatException)
        {
            return Result<AlertSchedulePreview>.Failure(exception.Message);
        }
    }

    private static AlertScheduleSummary Map(AlertSchedule schedule) =>
        new(
            schedule.Id,
            schedule.DefinitionId,
            schedule.Code,
            schedule.Name,
            schedule.CronExpression,
            schedule.TimeZoneId,
            schedule.CatchUpPolicy,
            schedule.Digest,
            schedule.IsActive,
            schedule.LastExecutionAtUtc,
            schedule.NextScheduledAtUtc,
            schedule.NextExecutionAtUtc);
}
