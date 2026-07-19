using Compliance360.Application;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class AlertSchedulerTests
{
    [Fact]
    public void Preview_Defers_Quiet_Hours_Weekend_And_Holiday_In_Tenant_TimeZone()
    {
        var calculator = new AlertScheduleCalculator();
        var scheduled = new DateTimeOffset(2026, 7, 17, 22, 30, 0, TimeSpan.FromHours(-5));

        var result = calculator.ApplyCalendar(
            scheduled.ToUniversalTime(),
            "America/Panama",
            """{"days":["Monday","Tuesday","Wednesday","Thursday","Friday"],"start":"08:00","end":"17:00","holidays":["2026-07-20"]}""",
            """{"start":"22:00","end":"06:00"}""");

        var local = TimeZoneInfo.ConvertTime(result.EffectiveAtUtc, TimeZoneInfo.FindSystemTimeZoneById("America/Panama"));
        Assert.Equal(new DateTime(2026, 7, 21, 8, 0, 0), local.DateTime);
        Assert.Contains("quiet-hours", result.Reason);
        Assert.Contains("non-business-day", result.Reason);
        Assert.Contains("holiday", result.Reason);
    }

    [Fact]
    public void Cron_Uses_TimeZone_And_Handles_Daylight_Saving_Transition()
    {
        var calculator = new AlertScheduleCalculator();
        var from = new DateTimeOffset(2026, 3, 8, 0, 0, 0, TimeSpan.Zero);

        var next = calculator.NextOccurrence("30 2 * * *", "America/New_York", from);

        Assert.True(next > from);
        Assert.Equal(TimeSpan.Zero, next.Offset);
    }

    [Fact]
    public async Task Processor_Persists_CatchUp_Atomically_And_Advances_Schedule()
    {
        var tenantId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 7, 19, 12, 0, 0, TimeSpan.Zero);
        var definition = new AlertDefinition(
            tenantId,
            Guid.NewGuid(),
            "SCHEDULED_RULE",
            "Scheduled rule",
            "Rule invoked by the durable scheduler.",
            Guid.NewGuid(),
            NotificationPriority.Normal);
        definition.SetPublishedVersion(Guid.NewGuid(), now.AddDays(-1));
        var schedule = new AlertSchedule(
            tenantId,
            definition.Id,
            "HOURLY_RULE",
            "Hourly rule",
            "0 * * * *",
            "UTC",
            "{}",
            "{}",
            AlertScheduleCatchUpPolicy.RunLatest,
            24,
            AlertScheduleDigest.None,
            now.AddHours(-2),
            now.AddHours(-2),
            Guid.NewGuid());
        var repository = new FakeSchedulerRepository(schedule, definition);
        var processor = new AlertSchedulerProcessor(
            repository,
            new AlertScheduleCalculator(),
            new FixedClock(now),
            Options.Create(new NotificationWorkerOptions { BatchSize = 10, LeaseDurationSeconds = 60 }));

        var result = await processor.ProcessDueAsync("worker-1");

        Assert.Equal(1, result.SchedulesClaimed);
        Assert.Equal(1, result.ExecutionsCreated);
        Assert.Equal(2, result.ExecutionsSkipped);
        Assert.Equal(3, repository.Executions.Count);
        Assert.Single(repository.Occurrences);
        Assert.Single(repository.Outbox);
        Assert.Equal(now.AddHours(1), schedule.NextScheduledAtUtc);
        Assert.Equal(now.AddHours(1), schedule.NextExecutionAtUtc);
        Assert.Null(schedule.LeaseUntilUtc);
    }

    private sealed class FakeSchedulerRepository : IAlertSchedulerRepository
    {
        private readonly AlertSchedule _schedule;
        private readonly AlertDefinition _definition;

        public FakeSchedulerRepository(AlertSchedule schedule, AlertDefinition definition)
        {
            _schedule = schedule;
            _definition = definition;
        }

        public List<AlertScheduleExecution> Executions { get; } = [];
        public List<AlertOccurrence> Occurrences { get; } = [];
        public List<NotificationOutboxEvent> Outbox { get; } = [];

        public Task<AlertDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken) =>
            Task.FromResult<AlertDefinition?>(_definition.TenantId == tenantId && _definition.Id == definitionId ? _definition : null);

        public Task AddAsync(AlertSchedule schedule, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyCollection<AlertSchedule>> ListAsync(Guid tenantId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<AlertSchedule>>([_schedule]);

        public Task<AlertSchedule?> GetAsync(Guid tenantId, Guid scheduleId, CancellationToken cancellationToken) =>
            Task.FromResult<AlertSchedule?>(_schedule.TenantId == tenantId && _schedule.Id == scheduleId ? _schedule : null);

        public Task<IReadOnlyCollection<AlertSchedule>> ClaimDueAsync(
            string workerId,
            DateTimeOffset nowUtc,
            DateTimeOffset leaseUntilUtc,
            int batchSize,
            CancellationToken cancellationToken)
        {
            _schedule.AcquireLease(workerId, nowUtc, leaseUntilUtc);
            return Task.FromResult<IReadOnlyCollection<AlertSchedule>>([_schedule]);
        }

        public Task<bool> ExecutionExistsAsync(Guid tenantId, Guid scheduleId, DateTimeOffset scheduledForUtc, CancellationToken cancellationToken) =>
            Task.FromResult(Executions.Any(item => item.TenantId == tenantId && item.ScheduleId == scheduleId && item.ScheduledForUtc == scheduledForUtc));

        public Task PersistBatchAsync(
            AlertSchedule schedule,
            IReadOnlyCollection<AlertScheduleExecution> executions,
            IReadOnlyCollection<AlertOccurrence> occurrences,
            IReadOnlyCollection<NotificationOutboxEvent> outboxEvents,
            CancellationToken cancellationToken)
        {
            Executions.AddRange(executions);
            Occurrences.AddRange(occurrences);
            Outbox.AddRange(outboxEvents);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset utcNow) => UtcNow = utcNow;
        public DateTimeOffset UtcNow { get; }
    }
}
