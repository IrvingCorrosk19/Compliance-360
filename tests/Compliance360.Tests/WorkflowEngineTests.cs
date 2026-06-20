using Compliance360.Application;
using Compliance360.Application.Workflows;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Workflows;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class WorkflowEngineTests
{
    [Fact]
    public async Task Workflow_Engine_Configures_Activates_And_Completes_Instance()
    {
        var fixture = WorkflowFixture.Create();
        var workflow = await fixture.Service.CreateWorkflowAsync(new CreateWorkflowCommand(fixture.TenantId, "Document Approval", "DOC-APP", "Document", fixture.UserId));
        var start = await fixture.Service.AddStepAsync(new AddWorkflowStepCommand(fixture.TenantId, workflow.Value!.Id, "Start", WorkflowStepType.Start, 1, 1, null, fixture.UserId));
        var approval = await fixture.Service.AddStepAsync(new AddWorkflowStepCommand(fixture.TenantId, workflow.Value.Id, "QA Approval", WorkflowStepType.Approval, 2, 24, Guid.NewGuid(), fixture.UserId));
        var end = await fixture.Service.AddStepAsync(new AddWorkflowStepCommand(fixture.TenantId, workflow.Value.Id, "End", WorkflowStepType.End, 3, 1, null, fixture.UserId));
        await fixture.Service.AddTransitionAsync(new AddWorkflowTransitionCommand(fixture.TenantId, workflow.Value.Id, start.Value!.Id, approval.Value!.Id, WorkflowDecision.Approved, fixture.UserId));
        await fixture.Service.AddTransitionAsync(new AddWorkflowTransitionCommand(fixture.TenantId, workflow.Value.Id, approval.Value.Id, end.Value!.Id, WorkflowDecision.Approved, fixture.UserId));
        var rule = await fixture.Service.AddRuleAsync(new AddWorkflowRuleCommand(fixture.TenantId, workflow.Value.Id, "Amount", WorkflowRuleOperator.GreaterThan, "1000", fixture.UserId));

        var activated = await fixture.Service.ActivateAsync(new WorkflowActionCommand(fixture.TenantId, workflow.Value.Id, fixture.UserId));
        var instance = await fixture.Service.StartAsync(new StartWorkflowCommand(fixture.TenantId, workflow.Value.Id, "Document", Guid.NewGuid(), fixture.UserId));
        var assignment = await fixture.Service.AssignAsync(new AssignWorkflowCommand(fixture.TenantId, instance.Value!.Id, approval.Value.Id, fixture.ApproverId, fixture.Clock.UtcNow.AddHours(24), fixture.UserId));
        var completed = await fixture.Service.CompleteAssignmentAsync(new CompleteWorkflowAssignmentCommand(fixture.TenantId, instance.Value.Id, assignment.Value!.Id, WorkflowDecision.Approved, fixture.ApproverId));

        Assert.True(activated.IsSuccess);
        Assert.True(rule.IsSuccess);
        Assert.True(completed.IsSuccess);
        Assert.Equal(WorkflowInstanceStatus.Approved, fixture.Repository.Instances.Single().Status);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.WorkflowApproved);
    }

    [Fact]
    public async Task Workflow_Engine_Escalates_And_Queues_Reminders()
    {
        var fixture = WorkflowFixture.CreateActiveInstance();
        var assignment = await fixture.Service.AssignAsync(new AssignWorkflowCommand(fixture.TenantId, fixture.InstanceId, fixture.ApprovalStepId, fixture.ApproverId, fixture.Clock.UtcNow.AddHours(1), fixture.UserId));

        var reminder = await fixture.Service.QueueReminderAsync(new WorkflowInstanceActionCommand(fixture.TenantId, fixture.InstanceId, fixture.UserId));
        var escalation = await fixture.Service.EscalateAsync(new EscalateWorkflowCommand(fixture.TenantId, fixture.InstanceId, assignment.Value!.Id, Guid.NewGuid(), fixture.UserId));

        Assert.True(reminder.IsSuccess);
        Assert.True(escalation.IsSuccess);
        Assert.Equal(WorkflowInstanceStatus.Escalated, fixture.Repository.Instances.Single().Status);
        Assert.Contains(fixture.Repository.Instances.Single().Notifications, notification => notification.Kind == WorkflowNotificationKind.Escalation);
    }

    [Fact]
    public async Task Workflow_Engine_Rejects_Invalid_Configuration_And_Missing_Entities()
    {
        var fixture = WorkflowFixture.Create();
        var missing = Guid.NewGuid();

        var activation = await fixture.Service.ActivateAsync(new WorkflowActionCommand(fixture.TenantId, missing, fixture.UserId));
        var step = await fixture.Service.AddStepAsync(new AddWorkflowStepCommand(fixture.TenantId, missing, "Step", WorkflowStepType.Approval, 1, 24, null, fixture.UserId));
        var start = await fixture.Service.StartAsync(new StartWorkflowCommand(fixture.TenantId, missing, "Document", Guid.NewGuid(), fixture.UserId));
        var assignment = await fixture.Service.AssignAsync(new AssignWorkflowCommand(fixture.TenantId, missing, Guid.NewGuid(), fixture.UserId, fixture.Clock.UtcNow, fixture.UserId));
        var transition = await fixture.Service.AddTransitionAsync(new AddWorkflowTransitionCommand(fixture.TenantId, missing, Guid.NewGuid(), Guid.NewGuid(), WorkflowDecision.Approved, fixture.UserId));
        var rule = await fixture.Service.AddRuleAsync(new AddWorkflowRuleCommand(fixture.TenantId, missing, "Field", WorkflowRuleOperator.Equals, "Value", fixture.UserId));
        var duplicate = await fixture.Service.CreateWorkflowAsync(new CreateWorkflowCommand(fixture.TenantId, "Approval", "DUP", "Document", fixture.UserId));
        var duplicateAgain = await fixture.Service.CreateWorkflowAsync(new CreateWorkflowCommand(fixture.TenantId, "Approval 2", "DUP", "Document", fixture.UserId));

        Assert.True(activation.IsFailure);
        Assert.True(step.IsFailure);
        Assert.True(start.IsFailure);
        Assert.True(assignment.IsFailure);
        Assert.True(transition.IsFailure);
        Assert.True(rule.IsFailure);
        Assert.True(duplicate.IsSuccess);
        Assert.True(duplicateAgain.IsFailure);
    }

    [Fact]
    public async Task Workflow_Engine_Rejects_Inactive_And_Invalid_Transitions()
    {
        var fixture = WorkflowFixture.Create();
        var workflow = await fixture.Service.CreateWorkflowAsync(new CreateWorkflowCommand(fixture.TenantId, "Inactive", "INACTIVE", "Document", fixture.UserId));
        var inactiveStart = await fixture.Service.StartAsync(new StartWorkflowCommand(fixture.TenantId, workflow.Value!.Id, "Document", Guid.NewGuid(), fixture.UserId));
        var active = WorkflowFixture.CreateActiveInstance();
        var assignment = await active.Service.AssignAsync(new AssignWorkflowCommand(active.TenantId, active.InstanceId, active.ApprovalStepId, active.ApproverId, active.Clock.UtcNow.AddHours(1), active.UserId));
        var invalidDecision = await active.Service.CompleteAssignmentAsync(new CompleteWorkflowAssignmentCommand(active.TenantId, active.InstanceId, assignment.Value!.Id, WorkflowDecision.Rejected, active.ApproverId));
        var missingAssignment = await active.Service.CompleteAssignmentAsync(new CompleteWorkflowAssignmentCommand(active.TenantId, active.InstanceId, Guid.NewGuid(), WorkflowDecision.Approved, active.ApproverId));
        var missingEscalation = await active.Service.EscalateAsync(new EscalateWorkflowCommand(active.TenantId, active.InstanceId, Guid.NewGuid(), Guid.NewGuid(), active.UserId));
        var invalidAssign = await active.Service.AssignAsync(new AssignWorkflowCommand(active.TenantId, active.InstanceId, Guid.Empty, active.ApproverId, active.Clock.UtcNow, active.UserId));

        Assert.True(inactiveStart.IsFailure);
        Assert.True(invalidDecision.IsFailure);
        Assert.True(missingAssignment.IsFailure);
        Assert.True(missingEscalation.IsFailure);
        Assert.True(invalidAssign.IsFailure);
    }

    [Fact]
    public async Task Workflow_Engine_Returns_Failures_For_Invalid_Domain_Inputs()
    {
        var fixture = WorkflowFixture.Create();
        var workflow = await fixture.Service.CreateWorkflowAsync(new CreateWorkflowCommand(fixture.TenantId, "Approval", "APP-INVALID", "Document", fixture.UserId));
        var start = await fixture.Service.AddStepAsync(new AddWorkflowStepCommand(fixture.TenantId, workflow.Value!.Id, "Start", WorkflowStepType.Start, 1, 1, null, fixture.UserId));
        var invalidWorkflow = await fixture.Service.CreateWorkflowAsync(new CreateWorkflowCommand(fixture.TenantId, "", "BAD", "Document", fixture.UserId));
        var invalidStep = await fixture.Service.AddStepAsync(new AddWorkflowStepCommand(fixture.TenantId, workflow.Value.Id, "", WorkflowStepType.Approval, 2, 24, null, fixture.UserId));
        var invalidTransition = await fixture.Service.AddTransitionAsync(new AddWorkflowTransitionCommand(fixture.TenantId, workflow.Value.Id, start.Value!.Id, Guid.NewGuid(), WorkflowDecision.Approved, fixture.UserId));
        var invalidRule = await fixture.Service.AddRuleAsync(new AddWorkflowRuleCommand(fixture.TenantId, workflow.Value.Id, "", WorkflowRuleOperator.Equals, "x", fixture.UserId));
        var noStartTransition = await fixture.Service.ActivateAsync(new WorkflowActionCommand(fixture.TenantId, workflow.Value.Id, fixture.UserId));
        var missingReminder = await fixture.Service.QueueReminderAsync(new WorkflowInstanceActionCommand(fixture.TenantId, Guid.NewGuid(), fixture.UserId));

        Assert.True(invalidWorkflow.IsFailure);
        Assert.True(invalidStep.IsFailure);
        Assert.True(invalidTransition.IsFailure);
        Assert.True(invalidRule.IsFailure);
        Assert.True(noStartTransition.IsFailure);
        Assert.True(missingReminder.IsFailure);
    }

    [Fact]
    public async Task Workflow_Start_Rejects_Missing_Start_Transition()
    {
        var fixture = WorkflowFixture.Create();
        var workflow = new Workflow(fixture.TenantId, "Broken Active", "BROKEN", "Document", fixture.UserId);
        var start = workflow.AddStep("Start", WorkflowStepType.Start, 1, 1, null);
        var review = workflow.AddStep("Review", WorkflowStepType.Review, 2, 24, null);
        workflow.AddStep("End", WorkflowStepType.End, 3, 1, null);
        workflow.AddTransition(review.Id, start.Id, WorkflowDecision.Approved);
        workflow.Activate();
        fixture.Repository.Workflows.Add(workflow);

        var result = await fixture.Service.StartAsync(new StartWorkflowCommand(fixture.TenantId, workflow.Id, "Document", Guid.NewGuid(), fixture.UserId));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Workflow_Activation_Rejects_Start_And_End_Without_Transitions()
    {
        var fixture = WorkflowFixture.Create();
        var workflow = await fixture.Service.CreateWorkflowAsync(new CreateWorkflowCommand(fixture.TenantId, "No Transitions", "NO-TRN", "Document", fixture.UserId));
        await fixture.Service.AddStepAsync(new AddWorkflowStepCommand(fixture.TenantId, workflow.Value!.Id, "Start", WorkflowStepType.Start, 1, 1, null, fixture.UserId));
        await fixture.Service.AddStepAsync(new AddWorkflowStepCommand(fixture.TenantId, workflow.Value.Id, "End", WorkflowStepType.End, 2, 1, null, fixture.UserId));

        var activated = await fixture.Service.ActivateAsync(new WorkflowActionCommand(fixture.TenantId, workflow.Value.Id, fixture.UserId));

        Assert.True(activated.IsFailure);
    }

    [Fact]
    public async Task Workflow_Engine_Completes_Rejection_Terminal_Path()
    {
        var fixture = WorkflowFixture.Create();
        var workflow = await fixture.Service.CreateWorkflowAsync(new CreateWorkflowCommand(fixture.TenantId, "Rejection", "REJ", "Document", fixture.UserId));
        var start = await fixture.Service.AddStepAsync(new AddWorkflowStepCommand(fixture.TenantId, workflow.Value!.Id, "Start", WorkflowStepType.Start, 1, 1, null, fixture.UserId));
        var approval = await fixture.Service.AddStepAsync(new AddWorkflowStepCommand(fixture.TenantId, workflow.Value.Id, "Approval", WorkflowStepType.Approval, 2, 24, null, fixture.UserId));
        var end = await fixture.Service.AddStepAsync(new AddWorkflowStepCommand(fixture.TenantId, workflow.Value.Id, "End", WorkflowStepType.End, 3, 1, null, fixture.UserId));
        await fixture.Service.AddTransitionAsync(new AddWorkflowTransitionCommand(fixture.TenantId, workflow.Value.Id, start.Value!.Id, approval.Value!.Id, WorkflowDecision.Approved, fixture.UserId));
        await fixture.Service.AddTransitionAsync(new AddWorkflowTransitionCommand(fixture.TenantId, workflow.Value.Id, approval.Value.Id, end.Value!.Id, WorkflowDecision.Rejected, fixture.UserId));
        await fixture.Service.ActivateAsync(new WorkflowActionCommand(fixture.TenantId, workflow.Value.Id, fixture.UserId));
        var instance = await fixture.Service.StartAsync(new StartWorkflowCommand(fixture.TenantId, workflow.Value.Id, "Document", Guid.NewGuid(), fixture.UserId));
        var assignment = await fixture.Service.AssignAsync(new AssignWorkflowCommand(fixture.TenantId, instance.Value!.Id, approval.Value.Id, fixture.ApproverId, fixture.Clock.UtcNow.AddHours(1), fixture.UserId));

        var rejected = await fixture.Service.CompleteAssignmentAsync(new CompleteWorkflowAssignmentCommand(fixture.TenantId, instance.Value.Id, assignment.Value!.Id, WorkflowDecision.Rejected, fixture.ApproverId));

        Assert.True(rejected.IsSuccess);
        Assert.Equal(WorkflowInstanceStatus.Rejected, fixture.Repository.Instances.Single().Status);
    }

    [Fact]
    public void Workflow_Domain_Enforces_Rules()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workflow = new Workflow(tenantId, "Approval", "APP", "Document", userId);
        var start = workflow.AddStep("Start", WorkflowStepType.Start, 1, 1, null);

        Assert.Throws<DomainException>(() => workflow.AddStep("Duplicate", WorkflowStepType.Approval, 1, 24, null));
        Assert.Throws<DomainException>(() => workflow.AddTransition(start.Id, Guid.NewGuid(), WorkflowDecision.Approved));
        Assert.Throws<DomainException>(() => workflow.Activate());
        Assert.Throws<DomainException>(() => new Workflow(tenantId, "", "APP", "Document", userId));
        Assert.Throws<DomainException>(() => new WorkflowStep(tenantId, workflow.Id, "", WorkflowStepType.Approval, 2, 24, null));
        Assert.Throws<DomainException>(() => new WorkflowAssignment(tenantId, Guid.NewGuid(), Guid.Empty, userId, DateTimeOffset.UtcNow, userId));
        Assert.Throws<DomainException>(() => new WorkflowRule(tenantId, workflow.Id, "", WorkflowRuleOperator.Equals, "A"));
        Assert.Throws<DomainException>(() => new WorkflowTransition(tenantId, workflow.Id, Guid.Empty, Guid.NewGuid(), WorkflowDecision.Approved));
        Assert.Throws<DomainException>(() => new WorkflowEscalation(tenantId, Guid.NewGuid(), Guid.Empty, userId, userId, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => new WorkflowHistory(tenantId, Guid.NewGuid(), "", userId, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => new WorkflowNotification(tenantId, Guid.NewGuid(), WorkflowNotificationKind.Reminder, "", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Workflow_Domain_Constructors_Expose_Expected_State()
    {
        var tenantId = Guid.NewGuid();
        var workflowId = Guid.NewGuid();
        var instanceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var step = new WorkflowStep(tenantId, workflowId, "Review", WorkflowStepType.Review, 7, 48, Guid.NewGuid());
        var transition = new WorkflowTransition(tenantId, workflowId, step.Id, Guid.NewGuid(), WorkflowDecision.Rejected);
        var rule = new WorkflowRule(tenantId, workflowId, "Country", WorkflowRuleOperator.Equals, "PA");
        var history = new WorkflowHistory(tenantId, instanceId, "Created", userId, DateTimeOffset.UtcNow);
        var escalation = new WorkflowEscalation(tenantId, instanceId, Guid.NewGuid(), Guid.NewGuid(), userId, DateTimeOffset.UtcNow);
        var notification = new WorkflowNotification(tenantId, instanceId, WorkflowNotificationKind.Completed, "Done", DateTimeOffset.UtcNow);

        Assert.Equal(tenantId, step.TenantId);
        Assert.Equal(workflowId, step.WorkflowId);
        Assert.Equal("Review", step.Name);
        Assert.Equal(WorkflowStepType.Review, step.Type);
        Assert.Equal(7, step.Sequence);
        Assert.Equal(48, step.SlaHours);
        Assert.NotNull(step.AssignedRoleId);
        Assert.Equal(workflowId, transition.WorkflowId);
        Assert.Equal(step.Id, transition.FromStepId);
        Assert.Equal(WorkflowDecision.Rejected, transition.Decision);
        Assert.Equal("Country", rule.FieldName);
        Assert.Equal(workflowId, rule.WorkflowId);
        Assert.Equal(WorkflowRuleOperator.Equals, rule.Operator);
        Assert.Equal("PA", rule.ExpectedValue);
        Assert.Equal("Created", history.Action);
        Assert.Equal(instanceId, history.WorkflowInstanceId);
        Assert.Equal(userId, history.UserId);
        Assert.Equal(instanceId, escalation.WorkflowInstanceId);
        Assert.NotEqual(Guid.Empty, escalation.AssignmentId);
        Assert.Equal(userId, escalation.RequestedByUserId);
        Assert.Equal(WorkflowNotificationKind.Completed, notification.Kind);
        Assert.Equal(instanceId, notification.WorkflowInstanceId);
        Assert.Equal("Done", notification.Message);
    }

    [Fact]
    public void Workflow_Instance_Rejects_Changes_After_Terminal_State()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var instance = new WorkflowInstance(tenantId, Guid.NewGuid(), "Document", Guid.NewGuid(), stepId, userId, DateTimeOffset.UtcNow);
        var assignment = instance.Assign(stepId, userId, DateTimeOffset.UtcNow.AddHours(1), userId);

        instance.CompleteStep(assignment.Id, WorkflowDecision.Approved, userId, Guid.NewGuid(), true, DateTimeOffset.UtcNow);

        Assert.Throws<DomainException>(() => instance.Assign(stepId, userId, DateTimeOffset.UtcNow, userId));
        Assert.Throws<DomainException>(() => instance.AddReminder(userId, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => assignment.Complete(WorkflowDecision.Approved, userId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Workflow_Instance_Can_Advance_NonTerminal_And_Reject_Terminal()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var nextStepId = Guid.NewGuid();
        var instance = new WorkflowInstance(tenantId, Guid.NewGuid(), "Document", Guid.NewGuid(), stepId, userId, DateTimeOffset.UtcNow);
        var firstAssignment = instance.Assign(stepId, userId, DateTimeOffset.UtcNow.AddHours(1), userId);

        instance.CompleteStep(firstAssignment.Id, WorkflowDecision.Approved, userId, nextStepId, false, DateTimeOffset.UtcNow);
        var secondAssignment = instance.Assign(nextStepId, userId, DateTimeOffset.UtcNow.AddHours(1), userId);
        instance.CompleteStep(secondAssignment.Id, WorkflowDecision.Rejected, userId, Guid.NewGuid(), true, DateTimeOffset.UtcNow);

        Assert.Equal(WorkflowInstanceStatus.Rejected, instance.Status);
        Assert.NotNull(instance.CompletedAtUtc);
    }

    [Fact]
    public async Task Workflow_Service_Rejects_Reminder_When_Instance_Completed_And_Workflow_Missing_On_Complete()
    {
        var fixture = WorkflowFixture.CreateActiveInstance();
        var assignment = await fixture.Service.AssignAsync(new AssignWorkflowCommand(fixture.TenantId, fixture.InstanceId, fixture.ApprovalStepId, fixture.ApproverId, fixture.Clock.UtcNow.AddHours(1), fixture.UserId));
        fixture.Repository.Workflows.Clear();

        var missingWorkflow = await fixture.Service.CompleteAssignmentAsync(new CompleteWorkflowAssignmentCommand(fixture.TenantId, fixture.InstanceId, assignment.Value!.Id, WorkflowDecision.Approved, fixture.UserId));

        fixture.Repository.Workflows.Add(new Workflow(fixture.TenantId, "Replacement", "REPL", "Document", fixture.UserId));
        fixture.Repository.Instances.Single().CompleteStep(assignment.Value.Id, WorkflowDecision.Approved, fixture.UserId, Guid.NewGuid(), true, fixture.Clock.UtcNow);
        var reminder = await fixture.Service.QueueReminderAsync(new WorkflowInstanceActionCommand(fixture.TenantId, fixture.InstanceId, fixture.UserId));

        Assert.True(missingWorkflow.IsFailure);
        Assert.True(reminder.IsFailure);
    }

    [Fact]
    public async Task Workflow_Search_Is_Tenant_Isolated()
    {
        var fixture = WorkflowFixture.CreateActiveInstance();
        fixture.Repository.Instances.Add(new WorkflowInstance(Guid.NewGuid(), fixture.WorkflowId, "Document", Guid.NewGuid(), fixture.StartStepId, fixture.UserId, fixture.Clock.UtcNow));

        var result = await fixture.Service.SearchInstancesAsync(new WorkflowInstanceSearchQuery(fixture.TenantId, fixture.WorkflowId, WorkflowInstanceStatus.Running, "Document", null, 1, 10));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Items);
        Assert.Equal(fixture.InstanceId, result.Value.Items.Single().Id);
    }

    [Fact]
    public async Task EfWorkflowRepository_Persists_Workflow_Graph()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfWorkflowRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workflow = new Workflow(tenantId, "Approval", "APP", "Document", userId);
        var start = workflow.AddStep("Start", WorkflowStepType.Start, 1, 1, null);
        var end = workflow.AddStep("End", WorkflowStepType.End, 2, 1, null);
        workflow.AddTransition(start.Id, end.Id, WorkflowDecision.Approved);
        workflow.Activate();
        var instance = new WorkflowInstance(tenantId, workflow.Id, "Document", Guid.NewGuid(), start.Id, userId, DateTimeOffset.UtcNow);

        await repository.AddWorkflowAsync(workflow);
        await repository.AddInstanceAsync(instance);
        await repository.AddAuditLogAsync(AuditLog.Create(tenantId, userId, nameof(Workflow), workflow.Id, AuditAction.WorkflowStarted, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();

        var loadedWorkflow = await repository.GetWorkflowAsync(tenantId, workflow.Id);
        var loadedInstance = await repository.GetInstanceAsync(tenantId, instance.Id);
        var search = await repository.SearchInstancesAsync(new WorkflowInstanceSearchCriteria(tenantId, workflow.Id, WorkflowInstanceStatus.Running, "Document", instance.EntityId, 1, 10));
        var exists = await repository.WorkflowCodeExistsAsync(tenantId, "app");

        Assert.NotNull(loadedWorkflow);
        Assert.NotNull(loadedInstance);
        Assert.Single(search.Items);
        Assert.True(exists);
        Assert.Single(dbContext.AuditLogs);
    }

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class WorkflowFixture
    {
        private WorkflowFixture()
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            ApproverId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryWorkflowRepository();
            Service = new WorkflowEngineService(Repository, new FakeApplicationDbContext(), Clock, Options.Create(new WorkflowEngineOptions()));
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public Guid ApproverId { get; }
        public Guid WorkflowId { get; private set; }
        public Guid InstanceId { get; private set; }
        public Guid StartStepId { get; private set; }
        public Guid ApprovalStepId { get; private set; }
        public FixedClock Clock { get; }
        public InMemoryWorkflowRepository Repository { get; }
        public WorkflowEngineService Service { get; }

        public static WorkflowFixture Create()
        {
            return new WorkflowFixture();
        }

        public static WorkflowFixture CreateActiveInstance()
        {
            var fixture = Create();
            var workflow = new Workflow(fixture.TenantId, "Approval", "APP", "Document", fixture.UserId);
            var start = workflow.AddStep("Start", WorkflowStepType.Start, 1, 1, null);
            var approval = workflow.AddStep("Approval", WorkflowStepType.Approval, 2, 24, null);
            var end = workflow.AddStep("End", WorkflowStepType.End, 3, 1, null);
            workflow.AddTransition(start.Id, approval.Id, WorkflowDecision.Approved);
            workflow.AddTransition(approval.Id, end.Id, WorkflowDecision.Approved);
            workflow.Activate();
            var instance = new WorkflowInstance(fixture.TenantId, workflow.Id, "Document", Guid.NewGuid(), start.Id, fixture.UserId, fixture.Clock.UtcNow);
            fixture.WorkflowId = workflow.Id;
            fixture.StartStepId = start.Id;
            fixture.ApprovalStepId = approval.Id;
            fixture.InstanceId = instance.Id;
            fixture.Repository.Workflows.Add(workflow);
            fixture.Repository.Instances.Add(instance);
            return fixture;
        }
    }

    private sealed class InMemoryWorkflowRepository : IWorkflowRepository
    {
        public List<Workflow> Workflows { get; } = [];
        public List<WorkflowInstance> Instances { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];

        public Task AddWorkflowAsync(Workflow workflow, CancellationToken cancellationToken = default)
        {
            Workflows.Add(workflow);
            return Task.CompletedTask;
        }

        public Task<Workflow?> GetWorkflowAsync(Guid tenantId, Guid workflowId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Workflows.SingleOrDefault(workflow => workflow.TenantId == tenantId && workflow.Id == workflowId));
        }

        public Task<bool> WorkflowCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Workflows.Any(workflow => workflow.TenantId == tenantId && workflow.Code == code));
        }

        public Task AddInstanceAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
        {
            Instances.Add(instance);
            return Task.CompletedTask;
        }

        public Task<WorkflowInstance?> GetInstanceAsync(Guid tenantId, Guid workflowInstanceId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Instances.SingleOrDefault(instance => instance.TenantId == tenantId && instance.Id == workflowInstanceId));
        }

        public Task<WorkflowInstanceSearchResult> SearchInstancesAsync(WorkflowInstanceSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var instances = Instances
                .Where(instance => instance.TenantId == criteria.TenantId)
                .Where(instance => !criteria.WorkflowId.HasValue || instance.WorkflowId == criteria.WorkflowId.Value)
                .Where(instance => !criteria.Status.HasValue || instance.Status == criteria.Status.Value)
                .Where(instance => criteria.EntityName is null || instance.EntityName == criteria.EntityName)
                .Where(instance => !criteria.EntityId.HasValue || instance.EntityId == criteria.EntityId.Value)
                .Select(instance => new WorkflowInstanceSummary(instance.Id, instance.WorkflowId, instance.EntityName, instance.EntityId, instance.CurrentStepId, instance.Status))
                .ToArray();

            return Task.FromResult(new WorkflowInstanceSearchResult(instances, instances.Length, criteria.Page, criteria.PageSize));
        }

        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            AuditLogs.Add(auditLog);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 6, 20, 18, 45, 0, TimeSpan.Zero);
    }
}
