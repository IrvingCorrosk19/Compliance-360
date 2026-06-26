using System.Diagnostics.CodeAnalysis;
using Compliance360.Application.Reporting;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Reporting;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Reporting;

[ExcludeFromCodeCoverage]
public sealed class EfReportingEngineRepository : IReportingEngineRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfReportingEngineRepository(Compliance360DbContext dbContext) => _dbContext = dbContext;

    public async Task AddCategoryAsync(ReportCategory category, CancellationToken cancellationToken = default) => await _dbContext.ReportCategories.AddAsync(category, cancellationToken);
    public Task<ReportCategory?> GetCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default) => _dbContext.ReportCategories.FirstOrDefaultAsync(category => category.TenantId == tenantId && category.Id == categoryId, cancellationToken);
    public Task<ReportCategory?> GetCategoryByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => _dbContext.ReportCategories.FirstOrDefaultAsync(category => category.TenantId == tenantId && category.Code == code.ToUpperInvariant(), cancellationToken);
    public Task<bool> CategoryCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => _dbContext.ReportCategories.AnyAsync(category => category.TenantId == tenantId && category.Code == code.ToUpperInvariant(), cancellationToken);
    public async Task AddDefinitionAsync(ReportDefinition definition, CancellationToken cancellationToken = default) => await _dbContext.ReportDefinitions.AddAsync(definition, cancellationToken);
    public Task<bool> DefinitionCodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) => _dbContext.ReportDefinitions.AnyAsync(definition => definition.TenantId == tenantId && definition.Code == code.ToUpperInvariant(), cancellationToken);

    public Task<ReportDefinition?> GetDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ReportDefinitions
            .Include(definition => definition.Templates)
            .Include(definition => definition.Parameters)
            .Include(definition => definition.Executions).ThenInclude(execution => execution.Outputs)
            .Include(definition => definition.Executions).ThenInclude(execution => execution.Exports)
            .Include(definition => definition.Schedules)
            .Include(definition => definition.Subscriptions)
            .Include(definition => definition.History)
            .Include(definition => definition.Permissions)
            .Include(definition => definition.DashboardBindings)
            .AsSplitQuery()
            .FirstOrDefaultAsync(definition => definition.TenantId == tenantId && definition.Id == definitionId, cancellationToken);
    }

    public async Task<ReportSearchResult> SearchAsync(ReportSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ReportDefinitions.AsNoTracking().Where(definition => definition.TenantId == criteria.TenantId);
        if (!string.IsNullOrWhiteSpace(criteria.SearchText)) query = query.Where(definition => definition.Name.Contains(criteria.SearchText) || definition.Code.Contains(criteria.SearchText));
        if (criteria.Module.HasValue) query = query.Where(definition => definition.Module == criteria.Module.Value);
        if (criteria.Status.HasValue) query = query.Where(definition => definition.Status == criteria.Status.Value);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(definition => definition.Module).ThenBy(definition => definition.Name)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(definition => new ReportDefinitionSummary(definition.Id, definition.TenantId, definition.Name, definition.Code, definition.Module, definition.DatasetKey, definition.Version, definition.Status))
            .ToListAsync(cancellationToken);
        return new ReportSearchResult(items, total, criteria.Page, criteria.PageSize);
    }

    public async Task<ReportingDashboardDatasetCatalog> GetDashboardDatasetsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var definitions = await _dbContext.ReportDefinitions.AsNoTracking()
            .Include(definition => definition.DashboardBindings)
            .Where(definition => definition.TenantId == tenantId && definition.Status == ReportDefinitionStatus.Active)
            .ToListAsync(cancellationToken);
        var datasets = definitions
            .SelectMany(definition => definition.DashboardBindings.Select(binding => new ReportDashboardDatasetDescriptor(definition.Id, definition.Code, definition.Module, binding.DatasetKey, binding.DashboardKey)))
            .ToArray();
        return new ReportingDashboardDatasetCatalog(datasets);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default) => await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);

    public async Task NormalizeNewReportChildStatesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in _dbContext.ChangeTracker.Entries().Where(entry => entry.State == EntityState.Modified).ToArray())
        {
            var exists = entry.Entity switch
            {
                ReportTemplate template => await _dbContext.ReportTemplates.AsNoTracking().AnyAsync(item => item.Id == template.Id, cancellationToken),
                ReportParameter parameter => await _dbContext.ReportParameters.AsNoTracking().AnyAsync(item => item.Id == parameter.Id, cancellationToken),
                ReportExecution execution => await _dbContext.ReportExecutions.AsNoTracking().AnyAsync(item => item.Id == execution.Id, cancellationToken),
                ReportOutput output => await _dbContext.ReportOutputs.AsNoTracking().AnyAsync(item => item.Id == output.Id, cancellationToken),
                ReportExport export => await _dbContext.ReportExports.AsNoTracking().AnyAsync(item => item.Id == export.Id, cancellationToken),
                ReportSchedule schedule => await _dbContext.ReportSchedules.AsNoTracking().AnyAsync(item => item.Id == schedule.Id, cancellationToken),
                ReportSubscription subscription => await _dbContext.ReportSubscriptions.AsNoTracking().AnyAsync(item => item.Id == subscription.Id, cancellationToken),
                ReportPermission permission => await _dbContext.ReportPermissions.AsNoTracking().AnyAsync(item => item.Id == permission.Id, cancellationToken),
                ReportDashboardBinding binding => await _dbContext.ReportDashboardBindings.AsNoTracking().AnyAsync(item => item.Id == binding.Id, cancellationToken),
                ReportHistory history => await _dbContext.ReportHistory.AsNoTracking().AnyAsync(item => item.Id == history.Id, cancellationToken),
                _ => true
            };

            if (!exists)
            {
                entry.State = EntityState.Added;
            }
        }
    }
}
