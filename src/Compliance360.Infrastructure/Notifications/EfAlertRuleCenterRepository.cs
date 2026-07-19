using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Notifications;

public sealed class EfAlertRuleCenterRepository : IAlertRuleCenterRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfAlertRuleCenterRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<AlertEventType>> ListEventTypesAsync(
        Guid tenantId,
        string? module,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.AlertEventTypes
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(item => item.Module == module.Trim());
        }

        return await query
            .OrderBy(item => item.Module)
            .ThenBy(item => item.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<(IReadOnlyCollection<AlertDefinition> Items, int Total)> SearchAsync(
        AlertDefinitionSearchQuery query,
        CancellationToken cancellationToken)
    {
        var source = _dbContext.AlertDefinitions
            .AsNoTracking()
            .Where(item => item.TenantId == query.TenantId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(item =>
                EF.Functions.ILike(item.Code, pattern)
                || EF.Functions.ILike(item.Name, pattern)
                || EF.Functions.ILike(item.Description, pattern));
        }

        if (query.EventTypeId.HasValue)
        {
            source = source.Where(item => item.EventTypeId == query.EventTypeId.Value);
        }

        if (query.Lifecycle.HasValue)
        {
            source = source.Where(item => item.Lifecycle == query.Lifecycle.Value);
        }

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderByDescending(item => item.UpdatedAtUtc ?? item.CreatedAtUtc)
            .ThenBy(item => item.Code)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken);
        return (items, total);
    }

    public Task<AlertDefinition?> GetDefinitionAsync(
        Guid tenantId,
        Guid definitionId,
        CancellationToken cancellationToken) =>
        _dbContext.AlertDefinitions.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.Id == definitionId,
            cancellationToken);

    public Task<AlertDefinitionVersion?> GetVersionAsync(
        Guid tenantId,
        Guid definitionId,
        Guid versionId,
        CancellationToken cancellationToken) =>
        _dbContext.AlertDefinitionVersions.SingleOrDefaultAsync(
            item => item.TenantId == tenantId && item.DefinitionId == definitionId && item.Id == versionId,
            cancellationToken);

    public async Task<IReadOnlyCollection<AlertDefinitionVersion>> ListVersionsAsync(
        Guid tenantId,
        Guid definitionId,
        CancellationToken cancellationToken) =>
        await _dbContext.AlertDefinitionVersions
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId && item.DefinitionId == definitionId)
            .OrderByDescending(item => item.Version)
            .ToArrayAsync(cancellationToken);

    public async Task<int> NextVersionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken)
    {
        var latest = await _dbContext.AlertDefinitionVersions
            .Where(item => item.TenantId == tenantId && item.DefinitionId == definitionId)
            .MaxAsync(item => (int?)item.Version, cancellationToken);
        return (latest ?? 0) + 1;
    }

    public async Task AddAsync(
        AlertDefinition definition,
        AlertDefinitionVersion version,
        CancellationToken cancellationToken)
    {
        await _dbContext.AlertDefinitions.AddAsync(definition, cancellationToken);
        await _dbContext.AlertDefinitionVersions.AddAsync(version, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddVersionAsync(AlertDefinitionVersion version, CancellationToken cancellationToken)
    {
        await _dbContext.AlertDefinitionVersions.AddAsync(version, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
