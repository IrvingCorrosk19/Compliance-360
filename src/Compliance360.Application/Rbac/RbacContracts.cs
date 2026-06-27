using Compliance360.Domain.Audit;
using Compliance360.Domain.Identity;
using Compliance360.Shared;

namespace Compliance360.Application.Rbac;

public interface IRbacService
{
    Task<Result<RoleSummary>> CreateRoleAsync(CreateRoleCommand command, CancellationToken cancellationToken = default);

    Task<Result<PermissionSummary>> CreatePermissionAsync(CreatePermissionCommand command, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<PermissionSummary>>> ListPermissionsAsync(CancellationToken cancellationToken = default);

    Task<Result> AssignRoleAsync(RbacAssignRoleCommand command, CancellationToken cancellationToken = default);

    Task<Result> GrantPermissionAsync(RbacGrantPermissionCommand command, CancellationToken cancellationToken = default);

    Task<Result<RbacAccessDecision>> AuthorizeAsync(RbacAuthorizationQuery query, CancellationToken cancellationToken = default);

    Task<Result<UserPermissionSet>> GetUserPermissionsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}

public interface IRbacRepository
{
    Task<User?> GetUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<Role?> GetRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);

    Task<Permission?> GetPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default);

    Task<Permission?> GetPermissionByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Permission>> ListPermissionsAsync(CancellationToken cancellationToken = default);

    Task<bool> RoleNameExistsAsync(Guid tenantId, string normalizedName, CancellationToken cancellationToken = default);

    Task<bool> UserRoleExistsAsync(Guid tenantId, Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    Task<bool> RolePermissionExistsAsync(Guid tenantId, Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);

    Task AddRoleAsync(Role role, CancellationToken cancellationToken = default);

    Task AddPermissionAsync(Permission permission, CancellationToken cancellationToken = default);

    Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken = default);

    Task AddRolePermissionAsync(RolePermission rolePermission, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetRoleNamesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateRoleCommand(Guid TenantId, string Name, bool IsSystemRole, Guid RequestedByUserId);

public sealed record CreatePermissionCommand(string Module, PermissionAction Action, string Description, Guid RequestedByUserId, Guid? TenantId);

public sealed record RbacAssignRoleCommand(Guid TenantId, Guid UserId, Guid RoleId, Guid RequestedByUserId);

public sealed record RbacGrantPermissionCommand(Guid TenantId, Guid RoleId, Guid PermissionId, Guid RequestedByUserId);

public sealed record RbacAuthorizationQuery(
    Guid TenantId,
    Guid UserId,
    string PermissionCode,
    Guid? EntityTenantId,
    Guid? CompanyId);

public sealed record RbacAccessDecision(bool IsAllowed, string Reason, IReadOnlyCollection<string> MatchedPermissions);

public sealed record UserPermissionSet(Guid TenantId, Guid UserId, IReadOnlyCollection<string> Roles, IReadOnlyCollection<string> Permissions);

public sealed record RoleSummary(Guid Id, Guid TenantId, string Name, bool IsSystemRole);

public sealed record PermissionSummary(Guid Id, string Module, PermissionAction Action, string Code, string Description);
