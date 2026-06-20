using Compliance360.Domain.Common;

namespace Compliance360.Domain.Workflows;

public enum WorkflowStatus
{
    Draft = 0,
    Active = 1,
    Inactive = 2
}

public enum WorkflowInstanceStatus
{
    Running = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3,
    Escalated = 4
}

public enum WorkflowStepType
{
    Start = 0,
    Approval = 1,
    Review = 2,
    End = 3
}

public enum WorkflowAssignmentStatus
{
    Pending = 0,
    Completed = 1,
    Escalated = 2,
    Cancelled = 3
}

public enum WorkflowDecision
{
    Approved = 0,
    Rejected = 1
}

public enum WorkflowRuleOperator
{
    Equals = 0,
    NotEquals = 1,
    GreaterThan = 2,
    LessThan = 3
}

public enum WorkflowNotificationKind
{
    Reminder = 0,
    Escalation = 1,
    Completed = 2
}

public sealed class Workflow : TenantEntity
{
    private readonly List<WorkflowStep> _steps = [];
    private readonly List<WorkflowTransition> _transitions = [];
    private readonly List<WorkflowRule> _rules = [];

    private Workflow()
    {
        Name = string.Empty;
        Code = string.Empty;
        EntityName = string.Empty;
    }

    public Workflow(Guid tenantId, string name, string code, string entityName, Guid createdByUserId)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 100).ToUpperInvariant();
        EntityName = Guard.AgainstNullOrWhiteSpace(entityName, nameof(entityName), 160);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        Status = WorkflowStatus.Draft;
    }

    public string Name { get; private set; }

    public string Code { get; private set; }

    public string EntityName { get; private set; }

    public WorkflowStatus Status { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public IReadOnlyCollection<WorkflowStep> Steps => _steps.AsReadOnly();

    public IReadOnlyCollection<WorkflowTransition> Transitions => _transitions.AsReadOnly();

    public IReadOnlyCollection<WorkflowRule> Rules => _rules.AsReadOnly();

    public WorkflowStep AddStep(string name, WorkflowStepType type, int sequence, int slaHours, Guid? assignedRoleId)
    {
        if (_steps.Any(step => step.Sequence == sequence))
        {
            throw new DomainException("Workflow step sequence already exists.");
        }

        var step = new WorkflowStep(TenantId, Id, name, type, sequence, slaHours, assignedRoleId);
        _steps.Add(step);
        return step;
    }

    public WorkflowTransition AddTransition(Guid fromStepId, Guid toStepId, WorkflowDecision decision)
    {
        if (!_steps.Any(step => step.Id == fromStepId) || !_steps.Any(step => step.Id == toStepId))
        {
            throw new DomainException("Workflow transition steps must belong to the workflow.");
        }

        var transition = new WorkflowTransition(TenantId, Id, fromStepId, toStepId, decision);
        _transitions.Add(transition);
        return transition;
    }

    public WorkflowRule AddRule(string fieldName, WorkflowRuleOperator ruleOperator, string expectedValue)
    {
        var rule = new WorkflowRule(TenantId, Id, fieldName, ruleOperator, expectedValue);
        _rules.Add(rule);
        return rule;
    }

    public void Activate()
    {
        if (!_steps.Any(step => step.Type == WorkflowStepType.Start) || !_steps.Any(step => step.Type == WorkflowStepType.End))
        {
            throw new DomainException("Workflow requires start and end steps before activation.");
        }

        if (_transitions.Count == 0)
        {
            throw new DomainException("Workflow requires at least one transition before activation.");
        }

        Status = WorkflowStatus.Active;
    }
}

public sealed class WorkflowStep : TenantEntity
{
    private WorkflowStep()
    {
        Name = string.Empty;
    }

    public WorkflowStep(Guid tenantId, Guid workflowId, string name, WorkflowStepType type, int sequence, int slaHours, Guid? assignedRoleId)
        : base(tenantId)
    {
        WorkflowId = Guard.AgainstEmpty(workflowId, nameof(workflowId));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 160);
        Type = type;
        Sequence = Guard.AgainstOutOfRange(sequence, nameof(sequence), 1, 1_000);
        SlaHours = Guard.AgainstOutOfRange(slaHours, nameof(slaHours), 1, 8_760);
        AssignedRoleId = assignedRoleId;
    }

    public Guid WorkflowId { get; private set; }

    public string Name { get; private set; }

    public WorkflowStepType Type { get; private set; }

    public int Sequence { get; private set; }

    public int SlaHours { get; private set; }

    public Guid? AssignedRoleId { get; private set; }
}

public sealed class WorkflowTransition : TenantEntity
{
    private WorkflowTransition()
    {
    }

    public WorkflowTransition(Guid tenantId, Guid workflowId, Guid fromStepId, Guid toStepId, WorkflowDecision decision)
        : base(tenantId)
    {
        WorkflowId = Guard.AgainstEmpty(workflowId, nameof(workflowId));
        FromStepId = Guard.AgainstEmpty(fromStepId, nameof(fromStepId));
        ToStepId = Guard.AgainstEmpty(toStepId, nameof(toStepId));
        Decision = decision;
    }

    public Guid WorkflowId { get; private set; }

    public Guid FromStepId { get; private set; }

    public Guid ToStepId { get; private set; }

    public WorkflowDecision Decision { get; private set; }
}

public sealed class WorkflowRule : TenantEntity
{
    private WorkflowRule()
    {
        FieldName = string.Empty;
        ExpectedValue = string.Empty;
    }

    public WorkflowRule(Guid tenantId, Guid workflowId, string fieldName, WorkflowRuleOperator ruleOperator, string expectedValue)
        : base(tenantId)
    {
        WorkflowId = Guard.AgainstEmpty(workflowId, nameof(workflowId));
        FieldName = Guard.AgainstNullOrWhiteSpace(fieldName, nameof(fieldName), 120);
        Operator = ruleOperator;
        ExpectedValue = Guard.AgainstNullOrWhiteSpace(expectedValue, nameof(expectedValue), 250);
    }

    public Guid WorkflowId { get; private set; }

    public string FieldName { get; private set; }

    public WorkflowRuleOperator Operator { get; private set; }

    public string ExpectedValue { get; private set; }
}

public sealed class WorkflowInstance : TenantEntity
{
    private readonly List<WorkflowAssignment> _assignments = [];
    private readonly List<WorkflowHistory> _history = [];
    private readonly List<WorkflowEscalation> _escalations = [];
    private readonly List<WorkflowNotification> _notifications = [];

    private WorkflowInstance()
    {
        EntityName = string.Empty;
    }

    public WorkflowInstance(Guid tenantId, Guid workflowId, string entityName, Guid entityId, Guid currentStepId, Guid startedByUserId, DateTimeOffset startedAtUtc)
        : base(tenantId)
    {
        WorkflowId = Guard.AgainstEmpty(workflowId, nameof(workflowId));
        EntityName = Guard.AgainstNullOrWhiteSpace(entityName, nameof(entityName), 160);
        EntityId = Guard.AgainstEmpty(entityId, nameof(entityId));
        CurrentStepId = Guard.AgainstEmpty(currentStepId, nameof(currentStepId));
        StartedByUserId = Guard.AgainstEmpty(startedByUserId, nameof(startedByUserId));
        StartedAtUtc = startedAtUtc;
        Status = WorkflowInstanceStatus.Running;
        AddHistory("Workflow started.", startedByUserId, startedAtUtc);
    }

    public Guid WorkflowId { get; private set; }

    public string EntityName { get; private set; }

    public Guid EntityId { get; private set; }

    public Guid CurrentStepId { get; private set; }

    public WorkflowInstanceStatus Status { get; private set; }

    public Guid StartedByUserId { get; private set; }

    public DateTimeOffset StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public IReadOnlyCollection<WorkflowAssignment> Assignments => _assignments.AsReadOnly();

    public IReadOnlyCollection<WorkflowHistory> History => _history.AsReadOnly();

    public IReadOnlyCollection<WorkflowEscalation> Escalations => _escalations.AsReadOnly();

    public IReadOnlyCollection<WorkflowNotification> Notifications => _notifications.AsReadOnly();

    public WorkflowAssignment Assign(Guid stepId, Guid assignedToUserId, DateTimeOffset dueAtUtc, Guid assignedByUserId)
    {
        EnsureRunning();
        var assignment = new WorkflowAssignment(TenantId, Id, stepId, assignedToUserId, dueAtUtc, assignedByUserId);
        _assignments.Add(assignment);
        AddHistory("Workflow assignment created.", assignedByUserId, DateTimeOffset.UtcNow);
        return assignment;
    }

    public void CompleteStep(Guid assignmentId, WorkflowDecision decision, Guid decidedByUserId, Guid nextStepId, bool isTerminal, DateTimeOffset decidedAtUtc)
    {
        EnsureRunning();
        var assignment = _assignments.SingleOrDefault(item => item.Id == assignmentId);
        if (assignment is null)
        {
            throw new DomainException("Workflow assignment not found.");
        }

        assignment.Complete(decision, decidedByUserId, decidedAtUtc);
        CurrentStepId = nextStepId;
        AddHistory($"Workflow step {decision}.", decidedByUserId, decidedAtUtc);

        if (isTerminal)
        {
            Status = decision == WorkflowDecision.Approved ? WorkflowInstanceStatus.Approved : WorkflowInstanceStatus.Rejected;
            CompletedAtUtc = decidedAtUtc;
            AddNotification(WorkflowNotificationKind.Completed, "Workflow completed.", decidedAtUtc);
        }
    }

    public WorkflowEscalation Escalate(Guid assignmentId, Guid escalatedToUserId, Guid requestedByUserId, DateTimeOffset escalatedAtUtc)
    {
        EnsureRunning();
        var assignment = _assignments.SingleOrDefault(item => item.Id == assignmentId);
        if (assignment is null)
        {
            throw new DomainException("Workflow assignment not found.");
        }

        assignment.MarkEscalated();
        Status = WorkflowInstanceStatus.Escalated;
        var escalation = new WorkflowEscalation(TenantId, Id, assignmentId, escalatedToUserId, requestedByUserId, escalatedAtUtc);
        _escalations.Add(escalation);
        AddHistory("Workflow escalated.", requestedByUserId, escalatedAtUtc);
        AddNotification(WorkflowNotificationKind.Escalation, "Workflow escalation required.", escalatedAtUtc);
        return escalation;
    }

    public WorkflowNotification AddReminder(Guid requestedByUserId, DateTimeOffset queuedAtUtc)
    {
        EnsureRunning();
        AddHistory("Workflow reminder queued.", requestedByUserId, queuedAtUtc);
        return AddNotification(WorkflowNotificationKind.Reminder, "Workflow reminder.", queuedAtUtc);
    }

    private WorkflowNotification AddNotification(WorkflowNotificationKind kind, string message, DateTimeOffset queuedAtUtc)
    {
        var notification = new WorkflowNotification(TenantId, Id, kind, message, queuedAtUtc);
        _notifications.Add(notification);
        return notification;
    }

    private void AddHistory(string action, Guid userId, DateTimeOffset occurredAtUtc)
    {
        _history.Add(new WorkflowHistory(TenantId, Id, action, userId, occurredAtUtc));
    }

    private void EnsureRunning()
    {
        if (Status is WorkflowInstanceStatus.Approved or WorkflowInstanceStatus.Rejected or WorkflowInstanceStatus.Cancelled)
        {
            throw new DomainException("Completed workflow instances cannot be changed.");
        }
    }
}

public sealed class WorkflowAssignment : TenantEntity
{
    private WorkflowAssignment()
    {
    }

    public WorkflowAssignment(Guid tenantId, Guid workflowInstanceId, Guid stepId, Guid assignedToUserId, DateTimeOffset dueAtUtc, Guid assignedByUserId)
        : base(tenantId)
    {
        WorkflowInstanceId = Guard.AgainstEmpty(workflowInstanceId, nameof(workflowInstanceId));
        StepId = Guard.AgainstEmpty(stepId, nameof(stepId));
        AssignedToUserId = Guard.AgainstEmpty(assignedToUserId, nameof(assignedToUserId));
        DueAtUtc = dueAtUtc;
        AssignedByUserId = Guard.AgainstEmpty(assignedByUserId, nameof(assignedByUserId));
        Status = WorkflowAssignmentStatus.Pending;
    }

    public Guid WorkflowInstanceId { get; private set; }

    public Guid StepId { get; private set; }

    public Guid AssignedToUserId { get; private set; }

    public Guid AssignedByUserId { get; private set; }

    public DateTimeOffset DueAtUtc { get; private set; }

    public WorkflowAssignmentStatus Status { get; private set; }

    public WorkflowDecision? Decision { get; private set; }

    public Guid? DecidedByUserId { get; private set; }

    public DateTimeOffset? DecidedAtUtc { get; private set; }

    public void Complete(WorkflowDecision decision, Guid decidedByUserId, DateTimeOffset decidedAtUtc)
    {
        if (Status != WorkflowAssignmentStatus.Pending && Status != WorkflowAssignmentStatus.Escalated)
        {
            throw new DomainException("Workflow assignment is not pending.");
        }

        Decision = decision;
        DecidedByUserId = Guard.AgainstEmpty(decidedByUserId, nameof(decidedByUserId));
        DecidedAtUtc = decidedAtUtc;
        Status = WorkflowAssignmentStatus.Completed;
    }

    public void MarkEscalated()
    {
        if (Status != WorkflowAssignmentStatus.Pending)
        {
            throw new DomainException("Only pending assignments can be escalated.");
        }

        Status = WorkflowAssignmentStatus.Escalated;
    }
}

public sealed class WorkflowHistory : TenantEntity
{
    private WorkflowHistory()
    {
        Action = string.Empty;
    }

    public WorkflowHistory(Guid tenantId, Guid workflowInstanceId, string action, Guid userId, DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        WorkflowInstanceId = Guard.AgainstEmpty(workflowInstanceId, nameof(workflowInstanceId));
        Action = Guard.AgainstNullOrWhiteSpace(action, nameof(action), 500);
        UserId = Guard.AgainstEmpty(userId, nameof(userId));
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid WorkflowInstanceId { get; private set; }

    public string Action { get; private set; }

    public Guid UserId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }
}

public sealed class WorkflowEscalation : TenantEntity
{
    private WorkflowEscalation()
    {
    }

    public WorkflowEscalation(Guid tenantId, Guid workflowInstanceId, Guid assignmentId, Guid escalatedToUserId, Guid requestedByUserId, DateTimeOffset escalatedAtUtc)
        : base(tenantId)
    {
        WorkflowInstanceId = Guard.AgainstEmpty(workflowInstanceId, nameof(workflowInstanceId));
        AssignmentId = Guard.AgainstEmpty(assignmentId, nameof(assignmentId));
        EscalatedToUserId = Guard.AgainstEmpty(escalatedToUserId, nameof(escalatedToUserId));
        RequestedByUserId = Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
        EscalatedAtUtc = escalatedAtUtc;
    }

    public Guid WorkflowInstanceId { get; private set; }

    public Guid AssignmentId { get; private set; }

    public Guid EscalatedToUserId { get; private set; }

    public Guid RequestedByUserId { get; private set; }

    public DateTimeOffset EscalatedAtUtc { get; private set; }
}

public sealed class WorkflowNotification : TenantEntity
{
    private WorkflowNotification()
    {
        Message = string.Empty;
    }

    public WorkflowNotification(Guid tenantId, Guid workflowInstanceId, WorkflowNotificationKind kind, string message, DateTimeOffset queuedAtUtc)
        : base(tenantId)
    {
        WorkflowInstanceId = Guard.AgainstEmpty(workflowInstanceId, nameof(workflowInstanceId));
        Kind = kind;
        Message = Guard.AgainstNullOrWhiteSpace(message, nameof(message), 500);
        QueuedAtUtc = queuedAtUtc;
    }

    public Guid WorkflowInstanceId { get; private set; }

    public WorkflowNotificationKind Kind { get; private set; }

    public string Message { get; private set; }

    public DateTimeOffset QueuedAtUtc { get; private set; }
}
