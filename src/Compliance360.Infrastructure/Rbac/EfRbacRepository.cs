using Compliance360.Application.Rbac;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Identity;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Rbac;

public sealed class EfRbacRepository : IRbacRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfRbacRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .Include(user => user.Roles)
            .FirstOrDefaultAsync(user => user.TenantId == tenantId && user.Id == userId, cancellationToken);
    }

    public Task<Role?> GetRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Roles
            .Include(role => role.Permissions)
            .FirstOrDefaultAsync(role => role.TenantId == tenantId && role.Id == roleId, cancellationToken);
    }

    public Task<Permission?> GetPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Permissions.FirstOrDefaultAsync(permission => permission.Id == permissionId, cancellationToken);
    }

    public Task<Permission?> GetPermissionByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return _dbContext.Permissions.FirstOrDefaultAsync(permission => permission.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public Task<bool> RoleNameExistsAsync(Guid tenantId, string normalizedName, CancellationToken cancellationToken = default)
    {
        return _dbContext.Roles.AnyAsync(role => role.TenantId == tenantId && role.NormalizedName == normalizedName, cancellationToken);
    }

    public async Task AddRoleAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _dbContext.Roles.AddAsync(role, cancellationToken);
    }

    public async Task AddPermissionAsync(Permission permission, CancellationToken cancellationToken = default)
    {
        await _dbContext.Permissions.AddAsync(permission, cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetRoleNamesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .Where(userRole => userRole.TenantId == tenantId && userRole.UserId == userId)
            .Join(_dbContext.Roles, userRole => userRole.RoleId, role => role.Id, (_, role) => role.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var roleIds = _dbContext.UserRoles
            .Where(userRole => userRole.TenantId == tenantId && userRole.UserId == userId)
            .Select(userRole => userRole.RoleId);

        return await _dbContext.RolePermissions
            .Where(rolePermission => rolePermission.TenantId == tenantId && roleIds.Contains(rolePermission.RoleId))
            .Join(_dbContext.Permissions, rolePermission => rolePermission.PermissionId, permission => permission.Id, (_, permission) => permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
