using Compliance360.Application.Workflows;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Workflows;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Workflows;

public sealed class EfWorkflowRepository : IWorkflowRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfWorkflowRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddWorkflowAsync(Workflow workflow, CancellationToken cancellationToken = default)
    {
        await _dbContext.Workflows.AddAsync(workflow, cancellationToken);
    }

    public Task<Workflow?> GetWorkflowAsync(Guid tenantId, Guid workflowId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Workflows
            .Include(workflow => workflow.Steps)
            .Include(workflow => workflow.Transitions)
            .Include(workflow => workflow.Rules)
            .AsSplitQuery()
            .FirstOrDefaultAsync(workflow => workflow.TenantId == tenantId && workflow.Id == workflowId, cancellationToken);
    }

    public Task<bool> WorkflowCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
    {
        return _dbContext.Workflows.AnyAsync(workflow => workflow.TenantId == tenantId && workflow.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task AddInstanceAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
    {
        await _dbContext.WorkflowInstances.AddAsync(instance, cancellationToken);
    }

    public Task<WorkflowInstance?> GetInstanceAsync(Guid tenantId, Guid workflowInstanceId, CancellationToken cancellationToken = default)
    {
        return _dbContext.WorkflowInstances
            .Include(instance => instance.Assignments)
            .Include(instance => instance.History)
            .Include(instance => instance.Escalations)
            .Include(instance => instance.Notifications)
            .AsSplitQuery()
            .FirstOrDefaultAsync(instance => instance.TenantId == tenantId && instance.Id == workflowInstanceId, cancellationToken);
    }

    public async Task<WorkflowInstanceSearchResult> SearchInstancesAsync(WorkflowInstanceSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.WorkflowInstances.AsNoTracking().Where(instance => instance.TenantId == criteria.TenantId);

        if (criteria.WorkflowId.HasValue)
        {
            query = query.Where(instance => instance.WorkflowId == criteria.WorkflowId.Value);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(instance => instance.Status == criteria.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.EntityName))
        {
            query = query.Where(instance => instance.EntityName == criteria.EntityName);
        }

        if (criteria.EntityId.HasValue)
        {
            query = query.Where(instance => instance.EntityId == criteria.EntityId.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(instance => instance.StartedAtUtc)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(instance => new WorkflowInstanceSummary(instance.Id, instance.WorkflowId, instance.EntityName, instance.EntityId, instance.CurrentStepId, instance.Status))
            .ToListAsync(cancellationToken);

        return new WorkflowInstanceSearchResult(items, total, criteria.Page, criteria.PageSize);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
