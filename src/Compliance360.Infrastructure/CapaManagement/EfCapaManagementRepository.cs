using Compliance360.Application.CapaManagement;
using Compliance360.Domain.Audit;
using Compliance360.Domain.CapaManagement;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.CapaManagement;

public sealed class EfCapaManagementRepository : ICapaManagementRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfCapaManagementRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Capa capa, CancellationToken cancellationToken = default)
    {
        await _dbContext.Capas.AddAsync(capa, cancellationToken);
    }

    public Task<Capa?> GetAsync(Guid tenantId, Guid capaId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Capas
            .Include(capa => capa.Owners)
            .Include(capa => capa.Approvers)
            .Include(capa => capa.RootCauses)
            .Include(capa => capa.CauseAnalyses)
            .Include(capa => capa.ContainmentActions)
            .Include(capa => capa.CorrectiveActions)
            .Include(capa => capa.PreventiveActions)
            .Include(capa => capa.EffectivenessChecks)
            .Include(capa => capa.Evidence)
            .Include(capa => capa.Attachments)
            .Include(capa => capa.History)
            .AsSplitQuery()
            .FirstOrDefaultAsync(capa => capa.TenantId == tenantId && capa.Id == capaId, cancellationToken);
    }

    public Task<bool> CodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
    {
        return _dbContext.Capas.AnyAsync(capa => capa.TenantId == tenantId && capa.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task<CapaSearchResult> SearchAsync(CapaSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Capas.AsNoTracking().Where(capa => capa.TenantId == criteria.TenantId);

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            query = query.Where(capa => capa.Title.Contains(criteria.SearchText) || capa.Code.Contains(criteria.SearchText));
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(capa => capa.Status == criteria.Status.Value);
        }

        if (criteria.Priority.HasValue)
        {
            query = query.Where(capa => capa.Priority == criteria.Priority.Value);
        }

        if (criteria.RiskLevel.HasValue)
        {
            query = query.Where(capa => capa.RiskLevel == criteria.RiskLevel.Value);
        }

        if (criteria.SupplierId.HasValue)
        {
            query = query.Where(capa => capa.SupplierId == criteria.SupplierId.Value);
        }

        if (criteria.AuditId.HasValue)
        {
            query = query.Where(capa => capa.AuditId == criteria.AuditId.Value);
        }

        if (criteria.OwnerUserId.HasValue)
        {
            query = query.Where(capa => capa.Owners.Any(owner => owner.UserId == criteria.OwnerUserId.Value && owner.IsActive));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(capa => capa.CreatedAtUtc)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(capa => new CapaSummary(capa.Id, capa.TenantId, capa.Title, capa.Code, capa.Status, capa.Priority, capa.RiskLevel, capa.SourceType, capa.SupplierId, capa.DocumentId, capa.AuditId, capa.CommitmentDueAtUtc, capa.ClosedAtUtc))
            .ToListAsync(cancellationToken);

        return new CapaSearchResult(items, total, criteria.Page, criteria.PageSize);
    }

    public async Task<CapaDashboardDto> GetDashboardAsync(Guid tenantId, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var open = await _dbContext.Capas.CountAsync(capa => capa.TenantId == tenantId && capa.Status != CapaStatus.Closed && capa.Status != CapaStatus.Cancelled, cancellationToken);
        var overdue = await _dbContext.Capas.CountAsync(capa => capa.TenantId == tenantId && capa.Status != CapaStatus.Closed && capa.CommitmentDueAtUtc.HasValue && capa.CommitmentDueAtUtc < now, cancellationToken);
        var critical = await _dbContext.Capas.CountAsync(capa => capa.TenantId == tenantId && (capa.Priority == CapaPriority.Critical || capa.RiskLevel == CapaRiskLevel.Critical), cancellationToken);
        var owners = await _dbContext.CapaOwners.CountAsync(owner => owner.TenantId == tenantId && owner.IsActive, cancellationToken);
        var suppliers = await _dbContext.Capas.CountAsync(capa => capa.TenantId == tenantId && capa.SupplierId.HasValue, cancellationToken);
        var audits = await _dbContext.Capas.CountAsync(capa => capa.TenantId == tenantId && capa.AuditId.HasValue, cancellationToken);
        var closed = await _dbContext.Capas
            .Where(capa => capa.TenantId == tenantId && capa.Status == CapaStatus.Closed && capa.ClosedAtUtc.HasValue)
            .Select(capa => new { capa.CreatedAtUtc, ClosedAtUtc = capa.ClosedAtUtc!.Value })
            .ToListAsync(cancellationToken);
        var averageClosureDays = closed.Count == 0 ? 0 : Math.Round((decimal)closed.Average(capa => Math.Max(0, (capa.ClosedAtUtc - capa.CreatedAtUtc).TotalDays)), 2);
        var effectivenessCheckCount = await _dbContext.CapaEffectivenessChecks.CountAsync(check => check.TenantId == tenantId, cancellationToken);
        var effectiveCount = await _dbContext.CapaEffectivenessChecks.CountAsync(check => check.TenantId == tenantId && check.IsEffective, cancellationToken);
        var effective = effectivenessCheckCount == 0 ? 100 : (int)Math.Round(effectiveCount * 100m / effectivenessCheckCount);
        var recurrence = effectivenessCheckCount - effectiveCount;

        return new CapaDashboardDto(open, overdue, critical, owners, suppliers, audits, averageClosureDays, effective, recurrence);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
