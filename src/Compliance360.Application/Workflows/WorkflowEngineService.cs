using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Workflows;
using Compliance360.Shared;
using Microsoft.Extensions.Options;

namespace Compliance360.Application.Workflows;

public sealed class WorkflowEngineService : IWorkflowEngineService
{
    private readonly IWorkflowRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;
    private readonly WorkflowEngineOptions _options;

    public WorkflowEngineService(IWorkflowRepository repository, IApplicationDbContext dbContext, IClock clock, IOptions<WorkflowEngineOptions> options)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<Result<WorkflowSummary>> CreateWorkflowAsync(CreateWorkflowCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var code = Guard.AgainstNullOrWhiteSpace(command.Code, nameof(command.Code), 100).ToUpperInvariant();
            if (await _repository.WorkflowCodeExistsAsync(command.TenantId, code, cancellationToken))
            {
                return Result<WorkflowSummary>.Failure("Workflow code already exists.");
            }

            var workflow = new Workflow(command.TenantId, command.Name, code, command.EntityName, command.RequestedByUserId);
            await _repository.AddWorkflowAsync(workflow, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, workflow.Id, AuditAction.ConfigurationChanged, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<WorkflowSummary>.Success(ToSummary(workflow));
        }
        catch (DomainException exception)
        {
            return Result<WorkflowSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<WorkflowStepSummary>> AddStepAsync(AddWorkflowStepCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var workflow = await _repository.GetWorkflowAsync(command.TenantId, command.WorkflowId, cancellationToken);
            if (workflow is null)
            {
                return Result<WorkflowStepSummary>.Failure("Workflow not found.");
            }

            var step = workflow.AddStep(command.Name, command.Type, command.Sequence, command.SlaHours, command.AssignedRoleId);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, workflow.Id, AuditAction.ConfigurationChanged, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<WorkflowStepSummary>.Success(new WorkflowStepSummary(step.Id, step.WorkflowId, step.Name, step.Type, step.Sequence, step.SlaHours, step.AssignedRoleId));
        }
        catch (DomainException exception)
        {
            return Result<WorkflowStepSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<WorkflowTransitionSummary>> AddTransitionAsync(AddWorkflowTransitionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var workflow = await _repository.GetWorkflowAsync(command.TenantId, command.WorkflowId, cancellationToken);
            if (workflow is null)
            {
                return Result<WorkflowTransitionSummary>.Failure("Workflow not found.");
            }

            var transition = workflow.AddTransition(command.FromStepId, command.ToStepId, command.Decision);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, workflow.Id, AuditAction.ConfigurationChanged, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<WorkflowTransitionSummary>.Success(new WorkflowTransitionSummary(transition.Id, transition.WorkflowId, transition.FromStepId, transition.ToStepId, transition.Decision));
        }
        catch (DomainException exception)
        {
            return Result<WorkflowTransitionSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<WorkflowRuleSummary>> AddRuleAsync(AddWorkflowRuleCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var workflow = await _repository.GetWorkflowAsync(command.TenantId, command.WorkflowId, cancellationToken);
            if (workflow is null)
            {
                return Result<WorkflowRuleSummary>.Failure("Workflow not found.");
            }

            var rule = workflow.AddRule(command.FieldName, command.Operator, command.ExpectedValue);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, workflow.Id, AuditAction.ConfigurationChanged, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<WorkflowRuleSummary>.Success(new WorkflowRuleSummary(rule.Id, rule.WorkflowId, rule.FieldName, rule.Operator, rule.ExpectedValue));
        }
        catch (DomainException exception)
        {
            return Result<WorkflowRuleSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result> ActivateAsync(WorkflowActionCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeWorkflowAsync(command, workflow => workflow.Activate(), AuditAction.ConfigurationChanged, cancellationToken);
    }

    public async Task<Result<WorkflowInstanceSummary>> StartAsync(StartWorkflowCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var workflow = await _repository.GetWorkflowAsync(command.TenantId, command.WorkflowId, cancellationToken);
            if (workflow is null)
            {
                return Result<WorkflowInstanceSummary>.Failure("Workflow not found.");
            }

            if (workflow.Status != WorkflowStatus.Active)
            {
                return Result<WorkflowInstanceSummary>.Failure("Workflow is not active.");
            }

            var startStep = workflow.Steps.OrderBy(step => step.Sequence).First(step => step.Type == WorkflowStepType.Start);
            var initialTransition = workflow.Transitions.FirstOrDefault(transition => transition.FromStepId == startStep.Id && transition.Decision == WorkflowDecision.Approved);
            if (initialTransition is null)
            {
                return Result<WorkflowInstanceSummary>.Failure("Workflow start transition not found.");
            }

            var instance = new WorkflowInstance(command.TenantId, workflow.Id, command.EntityName, command.EntityId, initialTransition.ToStepId, command.RequestedByUserId, _clock.UtcNow);
            await _repository.AddInstanceAsync(instance, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, instance.Id, AuditAction.WorkflowStarted, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<WorkflowInstanceSummary>.Success(ToSummary(instance));
        }
        catch (DomainException exception)
        {
            return Result<WorkflowInstanceSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<WorkflowAssignmentSummary>> AssignAsync(AssignWorkflowCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var instance = await _repository.GetInstanceAsync(command.TenantId, command.WorkflowInstanceId, cancellationToken);
            if (instance is null)
            {
                return Result<WorkflowAssignmentSummary>.Failure("Workflow instance not found.");
            }

            var assignment = instance.Assign(command.StepId, command.AssignedToUserId, command.DueAtUtc, command.RequestedByUserId);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, instance.Id, AuditAction.Updated, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<WorkflowAssignmentSummary>.Success(new WorkflowAssignmentSummary(assignment.Id, assignment.WorkflowInstanceId, assignment.StepId, assignment.AssignedToUserId, assignment.Status, assignment.DueAtUtc));
        }
        catch (DomainException exception)
        {
            return Result<WorkflowAssignmentSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result> CompleteAssignmentAsync(CompleteWorkflowAssignmentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var instance = await _repository.GetInstanceAsync(command.TenantId, command.WorkflowInstanceId, cancellationToken);
            if (instance is null)
            {
                return Result.Failure("Workflow instance not found.");
            }

            var workflow = await _repository.GetWorkflowAsync(command.TenantId, instance.WorkflowId, cancellationToken);
            if (workflow is null)
            {
                return Result.Failure("Workflow not found.");
            }

            var transition = workflow.Transitions.FirstOrDefault(item => item.FromStepId == instance.CurrentStepId && item.Decision == command.Decision);
            if (transition is null)
            {
                return Result.Failure("Workflow transition not found.");
            }

            var targetStep = workflow.Steps.Single(step => step.Id == transition.ToStepId);
            instance.CompleteStep(command.AssignmentId, command.Decision, command.RequestedByUserId, targetStep.Id, targetStep.Type == WorkflowStepType.End, _clock.UtcNow);
            var action = command.Decision == WorkflowDecision.Approved ? AuditAction.WorkflowApproved : AuditAction.WorkflowRejected;
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, instance.Id, action, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result<WorkflowEscalationSummary>> EscalateAsync(EscalateWorkflowCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var instance = await _repository.GetInstanceAsync(command.TenantId, command.WorkflowInstanceId, cancellationToken);
            if (instance is null)
            {
                return Result<WorkflowEscalationSummary>.Failure("Workflow instance not found.");
            }

            var escalation = instance.Escalate(command.AssignmentId, command.EscalatedToUserId, command.RequestedByUserId, _clock.UtcNow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, instance.Id, AuditAction.Updated, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<WorkflowEscalationSummary>.Success(new WorkflowEscalationSummary(escalation.Id, escalation.WorkflowInstanceId, escalation.AssignmentId, escalation.EscalatedToUserId));
        }
        catch (DomainException exception)
        {
            return Result<WorkflowEscalationSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<WorkflowNotificationSummary>> QueueReminderAsync(WorkflowInstanceActionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var instance = await _repository.GetInstanceAsync(command.TenantId, command.WorkflowInstanceId, cancellationToken);
            if (instance is null)
            {
                return Result<WorkflowNotificationSummary>.Failure("Workflow instance not found.");
            }

            var notification = instance.AddReminder(command.RequestedByUserId, _clock.UtcNow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, instance.Id, AuditAction.NotificationQueued, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<WorkflowNotificationSummary>.Success(new WorkflowNotificationSummary(notification.Id, notification.WorkflowInstanceId, notification.Kind, notification.Message));
        }
        catch (DomainException exception)
        {
            return Result<WorkflowNotificationSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<WorkflowInstanceSearchResult>> SearchInstancesAsync(WorkflowInstanceSearchQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, _options.MaxPageSize);
        return Result<WorkflowInstanceSearchResult>.Success(await _repository.SearchInstancesAsync(
            new WorkflowInstanceSearchCriteria(query.TenantId, query.WorkflowId, query.Status, query.EntityName, query.EntityId, page, pageSize),
            cancellationToken));
    }

    private async Task<Result> ChangeWorkflowAsync(WorkflowActionCommand command, Action<Workflow> change, AuditAction action, CancellationToken cancellationToken)
    {
        try
        {
            var workflow = await _repository.GetWorkflowAsync(command.TenantId, command.WorkflowId, cancellationToken);
            if (workflow is null)
            {
                return Result.Failure("Workflow not found.");
            }

            change(workflow);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, workflow.Id, action, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private async Task AppendAuditAsync(Guid tenantId, Guid userId, Guid entityId, AuditAction action, bool success, string? error, CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.FromEvent(
            new AuditEvent(
                nameof(Workflow),
                entityId,
                action,
                AuditLog.InferCategory(action),
                new AuditContext(tenantId, userId, null, null, null, null, null, null, null),
                new AuditSnapshot(null, null),
                new AuditMetadata("{\"source\":\"workflow-engine\"}"),
                success,
                error),
            _clock.UtcNow);

        await _repository.AddAuditLogAsync(auditLog, cancellationToken);
    }

    private static WorkflowSummary ToSummary(Workflow workflow)
    {
        return new WorkflowSummary(workflow.Id, workflow.TenantId, workflow.Name, workflow.Code, workflow.EntityName, workflow.Status);
    }

    private static WorkflowInstanceSummary ToSummary(WorkflowInstance instance)
    {
        return new WorkflowInstanceSummary(instance.Id, instance.WorkflowId, instance.EntityName, instance.EntityId, instance.CurrentStepId, instance.Status);
    }
}

public sealed class WorkflowEngineOptions
{
    public const string SectionName = "WorkflowEngine";

    public int MaxPageSize { get; set; } = 200;
}
