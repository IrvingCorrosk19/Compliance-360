using Compliance360.Application.Identity;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Identity;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.Identity;

public sealed class EfIdentityRepository : IIdentityRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfIdentityRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetUserByEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return UserQuery()
            .FirstOrDefaultAsync(user => user.TenantId == tenantId && user.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public Task<User?> GetUserByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        return UserQuery()
            .FirstOrDefaultAsync(user => user.TenantId == tenantId && user.Id == userId, cancellationToken);
    }

    public Task<Role?> GetRoleByIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Roles
            .Include(role => role.Permissions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(role => role.TenantId == tenantId && role.Id == roleId, cancellationToken);
    }

    public Task<Permission?> GetPermissionByIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Permissions.FirstOrDefaultAsync(permission => permission.Id == permissionId, cancellationToken);
    }

    public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
    }

    public Task<UserSession?> GetSessionByIdAsync(Guid tenantId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.UserSessions.FirstOrDefaultAsync(
            session => session.TenantId == tenantId && session.Id == sessionId,
            cancellationToken);
    }

    public Task<bool> IsTenantActiveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tenants.AnyAsync(
            tenant => tenant.Id == tenantId && tenant.Status == Domain.TenantManagement.TenantStatus.Active,
            cancellationToken);
    }

    public async Task<bool> IsTenantMfaRequiredAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TenantSettings
            .Where(settings => settings.TenantId == tenantId)
            .Select(settings => settings.RequireMfa)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<MfaConfiguration?> GetEnabledMfaConfigurationAsync(Guid tenantId, Guid userId, MfaMethod method, CancellationToken cancellationToken = default)
    {
        return _dbContext.MfaConfigurations.FirstOrDefaultAsync(
            configuration =>
                configuration.TenantId == tenantId
                && configuration.UserId == userId
                && configuration.Method == method
                && configuration.IsEnabled,
            cancellationToken);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task AddSessionAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserSessions.AddAsync(session, cancellationToken);
    }

    public async Task AddMfaConfigurationAsync(MfaConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await _dbContext.MfaConfigurations.AddAsync(configuration, cancellationToken);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
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

    public async Task<IReadOnlyCollection<UserTenantMembership>> GetUserTenantMembershipsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Where(user => user.NormalizedEmail == normalizedEmail)
            .Join(_dbContext.Tenants, user => user.TenantId, tenant => tenant.Id, (user, tenant) => new { user, tenant })
            .Join(_dbContext.TenantBranding, x => x.tenant.Id, branding => branding.TenantId, (x, branding) => new UserTenantMembership(
                x.tenant.Id,
                x.user.Id,
                string.IsNullOrWhiteSpace(branding.DisplayName) ? x.tenant.Name : branding.DisplayName,
                branding.LogoUri,
                branding.PrimaryColor,
                x.tenant.Industry))
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid?> ResolveTenantByHostAsync(string hostName, CancellationToken cancellationToken = default)
    {
        var normalizedHost = hostName.Trim().ToLowerInvariant();
        return await _dbContext.TenantDomains
            .Where(domain => domain.HostName == normalizedHost && domain.HttpsEnabled)
            .OrderByDescending(domain => domain.IsDefault)
            .Select(domain => (Guid?)domain.TenantId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<User> UserQuery()
    {
        return _dbContext.Users
            .Include(user => user.Roles)
            .Include(user => user.RefreshTokens)
            .Include(user => user.PasswordHistory)
            .Include(user => user.Sessions)
            .AsSplitQuery();
    }
}
