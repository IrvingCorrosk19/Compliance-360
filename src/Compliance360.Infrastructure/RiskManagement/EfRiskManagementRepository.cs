using Compliance360.Application.RiskManagement;
using Compliance360.Domain.Audit;
using Compliance360.Domain.RiskManagement;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.RiskManagement;

public sealed class EfRiskManagementRepository : IRiskManagementRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfRiskManagementRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddCategoryAsync(RiskCategory category, CancellationToken cancellationToken = default) => await _dbContext.RiskCategories.AddAsync(category, cancellationToken);
    public Task<RiskCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default) => _dbContext.RiskCategories.FirstOrDefaultAsync(category => category.TenantId == tenantId && category.Id == categoryId, cancellationToken);
    public Task<bool> CategoryCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => _dbContext.RiskCategories.AnyAsync(category => category.TenantId == tenantId && category.Code == code.ToUpperInvariant(), cancellationToken);
    public async Task AddMatrixAsync(RiskMatrix matrix, CancellationToken cancellationToken = default) => await _dbContext.RiskMatrices.AddAsync(matrix, cancellationToken);
    public Task<RiskMatrix?> GetMatrixAsync(Guid tenantId, Guid matrixId, CancellationToken cancellationToken = default) => _dbContext.RiskMatrices.FirstOrDefaultAsync(matrix => matrix.TenantId == tenantId && matrix.Id == matrixId, cancellationToken);
    public async Task AddRiskAsync(Risk risk, CancellationToken cancellationToken = default) => await _dbContext.Risks.AddAsync(risk, cancellationToken);

    public Task<Risk?> GetRiskAsync(Guid tenantId, Guid riskId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Risks
            .Include(risk => risk.Assessments)
            .Include(risk => risk.Treatments)
            .Include(risk => risk.MitigationPlans)
            .Include(risk => risk.Controls)
            .Include(risk => risk.Owners)
            .Include(risk => risk.Reviews)
            .Include(risk => risk.Evidence)
            .Include(risk => risk.Indicators)
            .Include(risk => risk.Attachments)
            .Include(risk => risk.History)
            .FirstOrDefaultAsync(risk => risk.TenantId == tenantId && risk.Id == riskId, cancellationToken);
    }

    public Task<bool> RiskCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => _dbContext.Risks.AnyAsync(risk => risk.TenantId == tenantId && risk.Code == code.ToUpperInvariant(), cancellationToken);

    public async Task<RiskSearchResult> SearchAsync(RiskSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Risks.AsNoTracking().Where(risk => risk.TenantId == criteria.TenantId);
        if (!string.IsNullOrWhiteSpace(criteria.SearchText)) query = query.Where(risk => risk.Title.Contains(criteria.SearchText) || risk.Code.Contains(criteria.SearchText));
        if (criteria.Status.HasValue) query = query.Where(risk => risk.Status == criteria.Status.Value);
        if (criteria.Type.HasValue) query = query.Where(risk => risk.Type == criteria.Type.Value);
        if (criteria.Level.HasValue) query = query.Where(risk => risk.ResidualLevel == criteria.Level.Value || risk.InherentLevel == criteria.Level.Value);
        if (!string.IsNullOrWhiteSpace(criteria.Area)) query = query.Where(risk => risk.Area == criteria.Area);
        if (criteria.SupplierId.HasValue) query = query.Where(risk => risk.SupplierId == criteria.SupplierId.Value);
        if (criteria.AuditId.HasValue) query = query.Where(risk => risk.AuditId == criteria.AuditId.Value);
        if (criteria.CapaId.HasValue) query = query.Where(risk => risk.CapaId == criteria.CapaId.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(risk => risk.CreatedAtUtc)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(risk => new RiskSummary(risk.Id, risk.TenantId, risk.Title, risk.Code, risk.Type, risk.Status, risk.InherentLevel, risk.ResidualLevel, risk.InherentScore, risk.ResidualScore, risk.Area, risk.Process, risk.SupplierId, risk.AuditId, risk.CapaId))
            .ToListAsync(cancellationToken);
        return new RiskSearchResult(items, total, criteria.Page, criteria.PageSize);
    }

    public async Task<RiskDashboardDto> GetDashboardAsync(Guid tenantId, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var risks = await _dbContext.Risks.AsNoTracking().Where(risk => risk.TenantId == tenantId).ToListAsync(cancellationToken);
        var critical = risks.Count(risk => risk.ResidualLevel == RiskLevel.Critical || risk.InherentLevel == RiskLevel.Critical);
        var high = risks.Count(risk => risk.ResidualLevel == RiskLevel.High || risk.InherentLevel == RiskLevel.High);
        var medium = risks.Count(risk => risk.ResidualLevel == RiskLevel.Medium || risk.InherentLevel == RiskLevel.Medium);
        var low = risks.Count(risk => risk.ResidualLevel == RiskLevel.Low && risk.InherentLevel == RiskLevel.Low);
        var overdue = risks.Count(risk => risk.Status != RiskStatus.Closed && risk.ReviewDueAtUtc.HasValue && risk.ReviewDueAtUtc.Value < now);
        return new RiskDashboardDto(critical, high, medium, low, overdue, risks.Select(risk => risk.Area).Distinct().Count(), risks.Count(risk => risk.SupplierId.HasValue), risks.Select(risk => risk.Process).Distinct().Count(), risks.Count, risks.Count(risk => risk.ResidualScore > 0));
    }

    public async Task<IReadOnlyCollection<RiskHeatMapPoint>> GetHeatMapAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Risks.AsNoTracking()
            .Where(risk => risk.TenantId == tenantId && risk.ResidualScore > 0)
            .Select(risk => new RiskHeatMapPoint(0, 0, risk.ResidualScore, risk.ResidualLevel))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default) => await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
}
