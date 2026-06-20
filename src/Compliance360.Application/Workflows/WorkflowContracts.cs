using Compliance360.Domain.Audit;
using Compliance360.Domain.Workflows;
using Compliance360.Shared;

namespace Compliance360.Application.Workflows;

public interface IWorkflowEngineService
{
    Task<Result<WorkflowSummary>> CreateWorkflowAsync(CreateWorkflowCommand command, CancellationToken cancellationToken = default);

    Task<Result<WorkflowStepSummary>> AddStepAsync(AddWorkflowStepCommand command, CancellationToken cancellationToken = default);

    Task<Result<WorkflowTransitionSummary>> AddTransitionAsync(AddWorkflowTransitionCommand command, CancellationToken cancellationToken = default);

    Task<Result<WorkflowRuleSummary>> AddRuleAsync(AddWorkflowRuleCommand command, CancellationToken cancellationToken = default);

    Task<Result> ActivateAsync(WorkflowActionCommand command, CancellationToken cancellationToken = default);

    Task<Result<WorkflowInstanceSummary>> StartAsync(StartWorkflowCommand command, CancellationToken cancellationToken = default);

    Task<Result<WorkflowAssignmentSummary>> AssignAsync(AssignWorkflowCommand command, CancellationToken cancellationToken = default);

    Task<Result> CompleteAssignmentAsync(CompleteWorkflowAssignmentCommand command, CancellationToken cancellationToken = default);

    Task<Result<WorkflowEscalationSummary>> EscalateAsync(EscalateWorkflowCommand command, CancellationToken cancellationToken = default);

    Task<Result<WorkflowNotificationSummary>> QueueReminderAsync(WorkflowInstanceActionCommand command, CancellationToken cancellationToken = default);

    Task<Result<WorkflowInstanceSearchResult>> SearchInstancesAsync(WorkflowInstanceSearchQuery query, CancellationToken cancellationToken = default);
}

public interface IWorkflowRepository
{
    Task AddWorkflowAsync(Workflow workflow, CancellationToken cancellationToken = default);

    Task<Workflow?> GetWorkflowAsync(Guid tenantId, Guid workflowId, CancellationToken cancellationToken = default);

    Task<bool> WorkflowCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);

    Task AddInstanceAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);

    Task<WorkflowInstance?> GetInstanceAsync(Guid tenantId, Guid workflowInstanceId, CancellationToken cancellationToken = default);

    Task<WorkflowInstanceSearchResult> SearchInstancesAsync(WorkflowInstanceSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateWorkflowCommand(Guid TenantId, string Name, string Code, string EntityName, Guid RequestedByUserId);

public sealed record AddWorkflowStepCommand(Guid TenantId, Guid WorkflowId, string Name, WorkflowStepType Type, int Sequence, int SlaHours, Guid? AssignedRoleId, Guid RequestedByUserId);

public sealed record AddWorkflowTransitionCommand(Guid TenantId, Guid WorkflowId, Guid FromStepId, Guid ToStepId, WorkflowDecision Decision, Guid RequestedByUserId);

public sealed record AddWorkflowRuleCommand(Guid TenantId, Guid WorkflowId, string FieldName, WorkflowRuleOperator Operator, string ExpectedValue, Guid RequestedByUserId);

public sealed record WorkflowActionCommand(Guid TenantId, Guid WorkflowId, Guid RequestedByUserId);

public sealed record StartWorkflowCommand(Guid TenantId, Guid WorkflowId, string EntityName, Guid EntityId, Guid RequestedByUserId);

public sealed record AssignWorkflowCommand(Guid TenantId, Guid WorkflowInstanceId, Guid StepId, Guid AssignedToUserId, DateTimeOffset DueAtUtc, Guid RequestedByUserId);

public sealed record CompleteWorkflowAssignmentCommand(Guid TenantId, Guid WorkflowInstanceId, Guid AssignmentId, WorkflowDecision Decision, Guid RequestedByUserId);

public sealed record EscalateWorkflowCommand(Guid TenantId, Guid WorkflowInstanceId, Guid AssignmentId, Guid EscalatedToUserId, Guid RequestedByUserId);

public sealed record WorkflowInstanceActionCommand(Guid TenantId, Guid WorkflowInstanceId, Guid RequestedByUserId);

public sealed record WorkflowInstanceSearchQuery(Guid TenantId, Guid? WorkflowId, WorkflowInstanceStatus? Status, string? EntityName, Guid? EntityId, int Page, int PageSize);

public sealed record WorkflowInstanceSearchCriteria(Guid TenantId, Guid? WorkflowId, WorkflowInstanceStatus? Status, string? EntityName, Guid? EntityId, int Page, int PageSize);

public sealed record WorkflowSummary(Guid Id, Guid TenantId, string Name, string Code, string EntityName, WorkflowStatus Status);

public sealed record WorkflowStepSummary(Guid Id, Guid WorkflowId, string Name, WorkflowStepType Type, int Sequence, int SlaHours, Guid? AssignedRoleId);

public sealed record WorkflowTransitionSummary(Guid Id, Guid WorkflowId, Guid FromStepId, Guid ToStepId, WorkflowDecision Decision);

public sealed record WorkflowRuleSummary(Guid Id, Guid WorkflowId, string FieldName, WorkflowRuleOperator Operator, string ExpectedValue);

public sealed record WorkflowInstanceSummary(Guid Id, Guid WorkflowId, string EntityName, Guid EntityId, Guid CurrentStepId, WorkflowInstanceStatus Status);

public sealed record WorkflowAssignmentSummary(Guid Id, Guid WorkflowInstanceId, Guid StepId, Guid AssignedToUserId, WorkflowAssignmentStatus Status, DateTimeOffset DueAtUtc);

public sealed record WorkflowEscalationSummary(Guid Id, Guid WorkflowInstanceId, Guid AssignmentId, Guid EscalatedToUserId);

public sealed record WorkflowNotificationSummary(Guid Id, Guid WorkflowInstanceId, WorkflowNotificationKind Kind, string Message);

public sealed record WorkflowInstanceSearchResult(IReadOnlyCollection<WorkflowInstanceSummary> Items, int TotalCount, int Page, int PageSize);
