using Compliance360.Application.QualityIndicators;
using Compliance360.Domain.Audit;
using Compliance360.Domain.QualityIndicators;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Compliance360.Infrastructure.QualityIndicators;

[ExcludeFromCodeCoverage]
public sealed class EfQualityIndicatorRepository : IQualityIndicatorRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfQualityIndicatorRepository(Compliance360DbContext dbContext) => _dbContext = dbContext;
    public async Task AddCategoryAsync(IndicatorCategory category, CancellationToken cancellationToken = default) => await _dbContext.IndicatorCategories.AddAsync(category, cancellationToken);
    public Task<IndicatorCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default) => _dbContext.IndicatorCategories.FirstOrDefaultAsync(category => category.TenantId == tenantId && category.Id == categoryId, cancellationToken);
    public Task<bool> CategoryCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => _dbContext.IndicatorCategories.AnyAsync(category => category.TenantId == tenantId && category.Code == code.ToUpperInvariant(), cancellationToken);
    public async Task AddIndicatorAsync(QualityIndicator indicator, CancellationToken cancellationToken = default) => await _dbContext.QualityIndicators.AddAsync(indicator, cancellationToken);
    public Task<bool> IndicatorCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => _dbContext.QualityIndicators.AnyAsync(indicator => indicator.TenantId == tenantId && indicator.Code == code.ToUpperInvariant(), cancellationToken);

    public Task<QualityIndicator?> GetIndicatorAsync(Guid tenantId, Guid indicatorId, CancellationToken cancellationToken = default)
    {
        return _dbContext.QualityIndicators
            .Include(indicator => indicator.Formulas)
            .Include(indicator => indicator.Targets)
            .Include(indicator => indicator.Thresholds)
            .Include(indicator => indicator.Measurements)
            .Include(indicator => indicator.Results)
            .Include(indicator => indicator.Periods)
            .Include(indicator => indicator.Processes)
            .Include(indicator => indicator.Alerts)
            .Include(indicator => indicator.Trends)
            .Include(indicator => indicator.History)
            .Include(indicator => indicator.Attachments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(indicator => indicator.TenantId == tenantId && indicator.Id == indicatorId, cancellationToken);
    }

    public async Task<IndicatorSearchResult> SearchAsync(IndicatorSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.QualityIndicators.AsNoTracking().Where(indicator => indicator.TenantId == criteria.TenantId);
        if (!string.IsNullOrWhiteSpace(criteria.SearchText)) query = query.Where(indicator => indicator.Name.Contains(criteria.SearchText) || indicator.Code.Contains(criteria.SearchText));
        if (criteria.Status.HasValue) query = query.Where(indicator => indicator.Status == criteria.Status.Value);
        if (criteria.Type.HasValue) query = query.Where(indicator => indicator.Type == criteria.Type.Value);
        if (criteria.Frequency.HasValue) query = query.Where(indicator => indicator.Frequency == criteria.Frequency.Value);
        if (criteria.SupplierId.HasValue) query = query.Where(indicator => indicator.SupplierId == criteria.SupplierId.Value);
        if (criteria.AuditId.HasValue) query = query.Where(indicator => indicator.AuditId == criteria.AuditId.Value);
        if (criteria.CapaId.HasValue) query = query.Where(indicator => indicator.CapaId == criteria.CapaId.Value);
        if (criteria.RiskId.HasValue) query = query.Where(indicator => indicator.RiskId == criteria.RiskId.Value);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(indicator => indicator.Name).Skip((criteria.Page - 1) * criteria.PageSize).Take(criteria.PageSize)
            .Select(indicator => new QualityIndicatorSummary(indicator.Id, indicator.TenantId, indicator.Name, indicator.Code, indicator.Type, indicator.Frequency, indicator.CalculationType, indicator.Status, indicator.Unit))
            .ToListAsync(cancellationToken);
        return new IndicatorSearchResult(items, total, criteria.Page, criteria.PageSize);
    }

    public async Task<IndicatorDashboardDto> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var total = await _dbContext.QualityIndicators.CountAsync(indicator => indicator.TenantId == tenantId, cancellationToken);
        var approved = await _dbContext.QualityIndicators.CountAsync(indicator => indicator.TenantId == tenantId && indicator.Status == IndicatorStatus.Approved, cancellationToken);
        var critical = await _dbContext.IndicatorResults.CountAsync(result => result.TenantId == tenantId && result.Status == IndicatorResultStatus.CriticalDeviation, cancellationToken);
        var alerts = await _dbContext.IndicatorAlerts.CountAsync(alert => alert.TenantId == tenantId && !alert.IsAcknowledged, cancellationToken);
        var resultCount = await _dbContext.IndicatorResults.CountAsync(result => result.TenantId == tenantId, cancellationToken);
        var compliantResults = await _dbContext.IndicatorResults.CountAsync(
            result => result.TenantId == tenantId && (result.Status == IndicatorResultStatus.OnTarget || result.Status == IndicatorResultStatus.AboveTarget),
            cancellationToken);
        var compliance = resultCount == 0 ? 100 : (int)Math.Round(compliantResults * 100m / resultCount);
        var negative = await _dbContext.IndicatorTrends.CountAsync(trend => trend.TenantId == tenantId && trend.Direction == IndicatorTrendDirection.Negative, cancellationToken);
        var criticalProcesses = await _dbContext.IndicatorProcesses.CountAsync(process => process.TenantId == tenantId, cancellationToken);
        return new IndicatorDashboardDto(total, approved, critical, alerts, compliance, negative, criticalProcesses);
    }

    public async Task<IReadOnlyCollection<IndicatorTrendSummary>> GetTrendsAsync(Guid tenantId, Guid? indicatorId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.IndicatorTrends.AsNoTracking().Where(trend => trend.TenantId == tenantId);
        if (indicatorId.HasValue) query = query.Where(trend => trend.IndicatorId == indicatorId.Value);
        return await query.OrderBy(trend => trend.CreatedAtUtc).Select(trend => new IndicatorTrendSummary(trend.Id, trend.IndicatorId, trend.PeriodId, trend.Direction, trend.Value, trend.PreviousValue)).ToListAsync(cancellationToken);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default) => await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
}
