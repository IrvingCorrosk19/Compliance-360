using Compliance360.Application.Enterprise;
using Compliance360.Domain.Enterprise;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Compliance360.Infrastructure.Enterprise;

[ExcludeFromCodeCoverage]
public sealed class EfEnterpriseWorkspaceRepository : IEnterpriseWorkspaceRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfEnterpriseWorkspaceRepository(Compliance360DbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(EnterpriseWorkspaceItem item, CancellationToken cancellationToken = default) =>
        await _dbContext.EnterpriseWorkspaceItems.AddAsync(item, cancellationToken);

    public Task<EnterpriseWorkspaceItem?> GetAsync(Guid tenantId, Guid itemId, CancellationToken cancellationToken = default) =>
        _dbContext.EnterpriseWorkspaceItems.FirstOrDefaultAsync(item => item.TenantId == tenantId && item.Id == itemId, cancellationToken);

    public Task<bool> CodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) =>
        _dbContext.EnterpriseWorkspaceItems.AnyAsync(item => item.TenantId == tenantId && item.Code == code.ToUpperInvariant(), cancellationToken);

    public async Task<IReadOnlyCollection<EnterpriseWorkspaceItemSummary>> SearchAsync(EnterpriseWorkspaceSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.EnterpriseWorkspaceItems.AsNoTracking().Where(item => item.TenantId == criteria.TenantId);
        if (criteria.Type.HasValue) query = query.Where(item => item.Type == criteria.Type.Value);
        if (criteria.Status.HasValue) query = query.Where(item => item.Status == criteria.Status.Value);
        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            query = query.Where(item => item.Title.Contains(criteria.SearchText) || item.Code.Contains(criteria.SearchText));
        }

        return await query
            .OrderBy(item => item.Type)
            .ThenBy(item => item.Title)
            .Select(item => new EnterpriseWorkspaceItemSummary(item.Id, item.TenantId, item.Type, item.Title, item.Code, item.Description, item.Status, item.OwnerUserId, item.DueAtUtc, item.CompletedAtUtc, item.MetadataJson))
            .ToListAsync(cancellationToken);
    }

    public async Task<EnterpriseWorkspaceDashboardDto> GetDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var items = await _dbContext.EnterpriseWorkspaceItems.AsNoTracking().Where(item => item.TenantId == tenantId).ToListAsync(cancellationToken);
        var byType = items.GroupBy(item => item.Type).ToDictionary(group => group.Key, group => group.Count());
        return new EnterpriseWorkspaceDashboardDto(
            items.Count,
            items.Count(item => item.Status == EnterpriseWorkspaceStatus.Active),
            items.Count(item => item.Status == EnterpriseWorkspaceStatus.Completed),
            items.Count(item => item.Status == EnterpriseWorkspaceStatus.Active && item.DueAtUtc.HasValue && item.DueAtUtc < now),
            byType);
    }
}
