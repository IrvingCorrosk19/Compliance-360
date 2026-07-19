using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Notifications;

public sealed class EfAlertSchedulerRepository : IAlertSchedulerRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfAlertSchedulerRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AlertDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken) =>
        _dbContext.AlertDefinitions
            .SingleOrDefaultAsync(item => item.TenantId == tenantId && item.Id == definitionId, cancellationToken);

    public async Task AddAsync(AlertSchedule schedule, CancellationToken cancellationToken)
    {
        await _dbContext.AlertSchedules.AddAsync(schedule, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AlertSchedule>> ListAsync(Guid tenantId, CancellationToken cancellationToken) =>
        await _dbContext.AlertSchedules
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId)
            .OrderBy(item => item.Name)
            .ToArrayAsync(cancellationToken);

    public Task<AlertSchedule?> GetAsync(Guid tenantId, Guid scheduleId, CancellationToken cancellationToken) =>
        _dbContext.AlertSchedules
            .SingleOrDefaultAsync(item => item.TenantId == tenantId && item.Id == scheduleId, cancellationToken);

    public async Task<IReadOnlyCollection<AlertSchedule>> ClaimDueAsync(
        string workerId,
        DateTimeOffset nowUtc,
        DateTimeOffset leaseUntilUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var schedules = await _dbContext.AlertSchedules
            .FromSqlInterpolated($"""
                SELECT *
                FROM compliance360.alert_schedules
                WHERE "IsActive" = TRUE
                  AND "NextExecutionAtUtc" <= {nowUtc}
                  AND ("LeaseUntilUtc" IS NULL OR "LeaseUntilUtc" < {nowUtc})
                ORDER BY "NextExecutionAtUtc", "Id"
                FOR UPDATE SKIP LOCKED
                LIMIT {Math.Clamp(batchSize, 1, 500)}
                """)
            .ToArrayAsync(cancellationToken);

        foreach (var schedule in schedules)
        {
            schedule.AcquireLease(workerId, nowUtc, leaseUntilUtc);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return schedules;
    }

    public Task<bool> ExecutionExistsAsync(
        Guid tenantId,
        Guid scheduleId,
        DateTimeOffset scheduledForUtc,
        CancellationToken cancellationToken) =>
        _dbContext.AlertScheduleExecutions.AnyAsync(
            item => item.TenantId == tenantId
                && item.ScheduleId == scheduleId
                && item.ScheduledForUtc == scheduledForUtc,
            cancellationToken);

    public async Task PersistBatchAsync(
        AlertSchedule schedule,
        IReadOnlyCollection<AlertScheduleExecution> executions,
        IReadOnlyCollection<AlertOccurrence> occurrences,
        IReadOnlyCollection<NotificationOutboxEvent> outboxEvents,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.AlertScheduleExecutions.AddRangeAsync(executions, cancellationToken);
        await _dbContext.AlertOccurrences.AddRangeAsync(occurrences, cancellationToken);
        await _dbContext.NotificationOutbox.AddRangeAsync(outboxEvents, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
