using Compliance360.Application.TenantManagement;
using Compliance360.Domain.Audit;
using Compliance360.Domain.TenantManagement;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.TenantManagement;

public sealed class EfTenantManagementRepository : ITenantManagementRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfTenantManagementRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return _dbContext.Tenants.AnyAsync(tenant => tenant.Slug == normalizedSlug, cancellationToken);
    }

    public Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tenants
            .Include(tenant => tenant.Settings)
            .Include(tenant => tenant.Branding)
            .Include(tenant => tenant.Subscription)
            .Include(tenant => tenant.Companies)
            .FirstOrDefaultAsync(tenant => tenant.Id == tenantId, cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
