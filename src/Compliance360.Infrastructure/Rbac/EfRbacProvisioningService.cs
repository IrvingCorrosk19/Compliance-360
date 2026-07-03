using Compliance360.Application.Rbac;
using Compliance360.Domain.Identity;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Rbac;

/// <summary>
/// Seeds the official RBAC catalog into PostgreSQL. Permissions are global and
/// deduplicated by code; roles are tenant-scoped and materialized from the
/// <see cref="RoleCatalog"/> templates. All operations are idempotent.
/// </summary>
public sealed class EfRbacProvisioningService : IRbacProvisioningService
{
    private readonly Compliance360DbContext _dbContext;

    public EfRbacProvisioningService(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsurePermissionCatalogAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Permissions
            .Select(permission => permission.Code)
            .ToListAsync(cancellationToken);
        var existingSet = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);

        var added = false;
        foreach (var definition in PermissionCatalog.All)
        {
            if (existingSet.Contains(definition.Code))
            {
                continue;
            }

            await _dbContext.Permissions.AddAsync(
                Permission.Define(definition.Code, definition.Action, definition.Description),
                cancellationToken);
            existingSet.Add(definition.Code);
            added = true;
        }

        if (added)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public Task<int> EnsurePlatformRolesAsync(Guid platformTenantId, CancellationToken cancellationToken = default) =>
        EnsureRolesAsync(platformTenantId, RoleCatalog.PlatformRoles, cancellationToken);

    public Task<int> EnsureTenantRolesAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        EnsureRolesAsync(tenantId, RoleCatalog.TenantRoles, cancellationToken);

    private async Task<int> EnsureRolesAsync(Guid tenantId, IReadOnlyList<RoleDefinition> roleDefinitions, CancellationToken cancellationToken)
    {
        await EnsurePermissionCatalogAsync(cancellationToken);

        var permissionIdByCode = await _dbContext.Permissions
            .ToDictionaryAsync(permission => permission.Code, permission => permission.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var rolesCreated = 0;
        foreach (var definition in roleDefinitions)
        {
            var normalizedName = definition.Name.ToUpperInvariant();
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(item => item.TenantId == tenantId && item.NormalizedName == normalizedName, cancellationToken);

            if (role is null)
            {
                role = new Role(tenantId, definition.Name, isSystemRole: true);
                await _dbContext.Roles.AddAsync(role, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                rolesCreated++;
            }

            var currentGrants = await _dbContext.RolePermissions
                .Where(item => item.TenantId == tenantId && item.RoleId == role.Id)
                .ToListAsync(cancellationToken);
            var currentSet = new HashSet<Guid>(currentGrants.Select(grant => grant.PermissionId));

            var expectedIds = new HashSet<Guid>();
            foreach (var code in definition.PermissionCodes)
            {
                if (permissionIdByCode.TryGetValue(code, out var permissionId))
                {
                    expectedIds.Add(permissionId);
                }
            }

            var changed = false;

            // Add missing grants declared by the template.
            foreach (var permissionId in expectedIds.Where(id => !currentSet.Contains(id)))
            {
                await _dbContext.RolePermissions.AddAsync(new RolePermission(tenantId, role.Id, permissionId), cancellationToken);
                changed = true;
            }

            // Prune stale grants so catalog roles match their template exactly
            // (removes legacy/monolithic permissions no longer in the catalog).
            var staleGrants = currentGrants.Where(grant => !expectedIds.Contains(grant.PermissionId)).ToList();
            if (staleGrants.Count > 0)
            {
                _dbContext.RolePermissions.RemoveRange(staleGrants);
                changed = true;
            }

            if (changed)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return rolesCreated;
    }
}
