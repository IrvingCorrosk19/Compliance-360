using Compliance360.Application.AuditManagement;
using Compliance360.Domain.Audit;
using Compliance360.Domain.AuditManagement;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.AuditManagement;

public sealed class EfAuditManagementRepository : IAuditManagementRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfAuditManagementRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddProgramAsync(AuditProgram program, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditPrograms.AddAsync(program, cancellationToken);
    }

    public Task<AuditProgram?> GetProgramAsync(Guid tenantId, Guid programId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuditPrograms.FirstOrDefaultAsync(program => program.TenantId == tenantId && program.Id == programId, cancellationToken);
    }

    public Task<bool> ProgramCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuditPrograms.AnyAsync(program => program.TenantId == tenantId && program.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task AddChecklistAsync(AuditChecklist checklist, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditChecklists.AddAsync(checklist, cancellationToken);
    }

    public Task<AuditChecklist?> GetChecklistAsync(Guid tenantId, Guid checklistId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuditChecklists
            .Include(checklist => checklist.Items)
            .FirstOrDefaultAsync(checklist => checklist.TenantId == tenantId && checklist.Id == checklistId, cancellationToken);
    }

    public Task<bool> ChecklistCodeVersionExistsAsync(Guid tenantId, string code, int version, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuditChecklists.AnyAsync(checklist => checklist.TenantId == tenantId && checklist.Code == code.ToUpperInvariant() && checklist.Version == version, cancellationToken);
    }

    public async Task AddPlanAsync(AuditPlan plan, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditPlans.AddAsync(plan, cancellationToken);
    }

    public Task<AuditPlan?> GetPlanAsync(Guid tenantId, Guid planId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuditPlans.FirstOrDefaultAsync(plan => plan.TenantId == tenantId && plan.Id == planId, cancellationToken);
    }

    public async Task AddAuditAsync(ManagedAudit audit, CancellationToken cancellationToken = default)
    {
        await _dbContext.ManagedAudits.AddAsync(audit, cancellationToken);
    }

    public Task<ManagedAudit?> GetAuditAsync(Guid tenantId, Guid auditId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ManagedAudits
            .Include(audit => audit.Schedules)
            .Include(audit => audit.Participants)
            .Include(audit => audit.Areas)
            .Include(audit => audit.Findings)
            .Include(audit => audit.Evidence)
            .Include(audit => audit.Observations)
            .Include(audit => audit.NonConformities)
            .Include(audit => audit.Recommendations)
            .Include(audit => audit.CorrectiveActionLinks)
            .Include(audit => audit.Attachments)
            .Include(audit => audit.History)
            .AsSplitQuery()
            .FirstOrDefaultAsync(audit => audit.TenantId == tenantId && audit.Id == auditId, cancellationToken);
    }

    public Task<bool> AuditCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
    {
        return _dbContext.ManagedAudits.AnyAsync(audit => audit.TenantId == tenantId && audit.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task<ManagedAuditSearchResult> SearchAsync(ManagedAuditSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ManagedAudits.AsNoTracking().Where(audit => audit.TenantId == criteria.TenantId);

        if (criteria.Type.HasValue)
        {
            query = query.Where(audit => audit.Type == criteria.Type.Value);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(audit => audit.Status == criteria.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            query = query.Where(audit => audit.Title.Contains(criteria.SearchText) || audit.Code.Contains(criteria.SearchText));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(audit => audit.CreatedAtUtc)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(audit => new ManagedAuditSummary(audit.Id, audit.TenantId, audit.Title, audit.Code, audit.Type, audit.Status, audit.ChecklistId))
            .ToListAsync(cancellationToken);

        return new ManagedAuditSearchResult(items, total, criteria.Page, criteria.PageSize);
    }

    public async Task<AuditDashboardDto> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var open = await _dbContext.ManagedAudits.CountAsync(audit => audit.TenantId == tenantId && audit.Status != ManagedAuditStatus.Closed && audit.Status != ManagedAuditStatus.Cancelled, cancellationToken);
        var closed = await _dbContext.ManagedAudits.CountAsync(audit => audit.TenantId == tenantId && audit.Status == ManagedAuditStatus.Closed, cancellationToken);
        var critical = await _dbContext.AuditFindings.CountAsync(finding => finding.TenantId == tenantId && finding.Severity == AuditFindingSeverity.Critical, cancellationToken);
        var major = await _dbContext.AuditFindings.CountAsync(finding => finding.TenantId == tenantId && finding.Severity == AuditFindingSeverity.Major, cancellationToken);
        var pending = await _dbContext.AuditCorrectiveActionLinks.CountAsync(link => link.TenantId == tenantId, cancellationToken);
        var totalFindings = await _dbContext.AuditFindings.CountAsync(finding => finding.TenantId == tenantId, cancellationToken);
        var compliance = totalFindings == 0 ? 100 : Math.Max(0, 100 - ((critical + major) * 10));

        return new AuditDashboardDto(open, closed, critical, major, pending, compliance, totalFindings);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
