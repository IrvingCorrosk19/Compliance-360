using Compliance360.Domain.Common;

namespace Compliance360.Domain.Notifications;

public enum AlertScheduleCatchUpPolicy
{
    Skip = 0,
    RunLatest = 1,
    RunAll = 2
}

public enum AlertScheduleDigest
{
    None = 0,
    Daily = 1,
    Weekly = 2
}

public enum AlertScheduleExecutionStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Skipped = 3,
    Failed = 4
}

public sealed class AlertSchedule : TenantEntity
{
    private AlertSchedule()
    {
        Code = string.Empty;
        Name = string.Empty;
        CronExpression = string.Empty;
        TimeZoneId = "UTC";
        BusinessCalendarJson = "{}";
        QuietHoursJson = "{}";
    }

    public AlertSchedule(
        Guid tenantId,
        Guid definitionId,
        string code,
        string name,
        string cronExpression,
        string timeZoneId,
        string businessCalendarJson,
        string quietHoursJson,
        AlertScheduleCatchUpPolicy catchUpPolicy,
        int maxCatchUpExecutions,
        AlertScheduleDigest digest,
        DateTimeOffset nextScheduledAtUtc,
        DateTimeOffset nextExecutionAtUtc,
        Guid createdByUserId)
        : base(tenantId)
    {
        DefinitionId = Guard.AgainstEmpty(definitionId, nameof(definitionId));
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 160).ToUpperInvariant();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 200);
        CronExpression = Guard.AgainstNullOrWhiteSpace(cronExpression, nameof(cronExpression), 120);
        TimeZoneId = Guard.AgainstNullOrWhiteSpace(timeZoneId, nameof(timeZoneId), 120);
        BusinessCalendarJson = Guard.AgainstNullOrWhiteSpace(businessCalendarJson, nameof(businessCalendarJson), 16_000);
        QuietHoursJson = Guard.AgainstNullOrWhiteSpace(quietHoursJson, nameof(quietHoursJson), 4_000);
        CatchUpPolicy = catchUpPolicy;
        MaxCatchUpExecutions = Guard.AgainstOutOfRange(maxCatchUpExecutions, nameof(maxCatchUpExecutions), 1, 1_000);
        Digest = digest;
        NextScheduledAtUtc = nextScheduledAtUtc;
        NextExecutionAtUtc = nextExecutionAtUtc;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        IsActive = true;
    }

    public Guid DefinitionId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string CronExpression { get; private set; }
    public string TimeZoneId { get; private set; }
    public string BusinessCalendarJson { get; private set; }
    public string QuietHoursJson { get; private set; }
    public AlertScheduleCatchUpPolicy CatchUpPolicy { get; private set; }
    public int MaxCatchUpExecutions { get; private set; }
    public AlertScheduleDigest Digest { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? LastExecutionAtUtc { get; private set; }
    public DateTimeOffset NextScheduledAtUtc { get; private set; }
    public DateTimeOffset NextExecutionAtUtc { get; private set; }
    public DateTimeOffset? LeaseUntilUtc { get; private set; }
    public string? LeaseOwner { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    public void AcquireLease(string workerId, DateTimeOffset nowUtc, DateTimeOffset leaseUntilUtc)
    {
        if (!IsActive || NextExecutionAtUtc > nowUtc || (LeaseUntilUtc.HasValue && LeaseUntilUtc > nowUtc))
        {
            throw new DomainException("Alert schedule is not available for lease.");
        }

        LeaseOwner = Guard.AgainstNullOrWhiteSpace(workerId, nameof(workerId), 200);
        LeaseUntilUtc = leaseUntilUtc;
    }

    public void Advance(DateTimeOffset scheduledAtUtc, DateTimeOffset nextScheduledAtUtc, DateTimeOffset nextExecutionAtUtc, DateTimeOffset nowUtc)
    {
        LastExecutionAtUtc = scheduledAtUtc;
        NextScheduledAtUtc = nextScheduledAtUtc;
        NextExecutionAtUtc = nextExecutionAtUtc;
        LeaseUntilUtc = null;
        LeaseOwner = null;
        MarkUpdated(nowUtc);
    }

    public void Disable(DateTimeOffset nowUtc)
    {
        IsActive = false;
        LeaseUntilUtc = null;
        LeaseOwner = null;
        MarkUpdated(nowUtc);
    }

    public void Enable(DateTimeOffset nextScheduledAtUtc, DateTimeOffset nextExecutionAtUtc, DateTimeOffset nowUtc)
    {
        IsActive = true;
        NextScheduledAtUtc = nextScheduledAtUtc;
        NextExecutionAtUtc = nextExecutionAtUtc;
        MarkUpdated(nowUtc);
    }
}

public sealed class AlertScheduleExecution : TenantEntity
{
    private AlertScheduleExecution()
    {
        WorkerId = string.Empty;
    }

    public AlertScheduleExecution(Guid tenantId, Guid scheduleId, DateTimeOffset scheduledForUtc, string workerId, DateTimeOffset nowUtc)
        : base(tenantId)
    {
        ScheduleId = Guard.AgainstEmpty(scheduleId, nameof(scheduleId));
        ScheduledForUtc = scheduledForUtc;
        WorkerId = Guard.AgainstNullOrWhiteSpace(workerId, nameof(workerId), 200);
        Status = AlertScheduleExecutionStatus.Processing;
        StartedAtUtc = nowUtc;
    }

    public Guid ScheduleId { get; private set; }
    public DateTimeOffset ScheduledForUtc { get; private set; }
    public AlertScheduleExecutionStatus Status { get; private set; }
    public string WorkerId { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public Guid? OccurrenceId { get; private set; }
    public string? FailureReason { get; private set; }

    public void Complete(Guid occurrenceId, DateTimeOffset nowUtc)
    {
        OccurrenceId = Guard.AgainstEmpty(occurrenceId, nameof(occurrenceId));
        Status = AlertScheduleExecutionStatus.Completed;
        CompletedAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }

    public void Skip(string reason, DateTimeOffset nowUtc)
    {
        FailureReason = Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 1_000);
        Status = AlertScheduleExecutionStatus.Skipped;
        CompletedAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }

    public void Fail(string reason, DateTimeOffset nowUtc)
    {
        FailureReason = Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 1_000);
        Status = AlertScheduleExecutionStatus.Failed;
        CompletedAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }
}
