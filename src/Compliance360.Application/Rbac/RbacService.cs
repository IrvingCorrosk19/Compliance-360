using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Identity;
using Compliance360.Shared;

namespace Compliance360.Application.Rbac;

public sealed class RbacService : IRbacService
{
    private readonly IRbacRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public RbacService(IRbacRepository repository, IApplicationDbContext dbContext, IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<Result<RoleSummary>> CreateRoleAsync(CreateRoleCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedName = Guard.AgainstNullOrWhiteSpace(command.Name, nameof(command.Name), 120).ToUpperInvariant();
            if (await _repository.RoleNameExistsAsync(command.TenantId, normalizedName, cancellationToken))
            {
                return Result<RoleSummary>.Failure("Role name already exists for this tenant.");
            }

            var role = new Role(command.TenantId, command.Name, command.IsSystemRole);
            await _repository.AddRoleAsync(role, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(Role), role.Id, AuditAction.AdministrativeEvent, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<RoleSummary>.Success(new RoleSummary(role.Id, role.TenantId, role.Name, role.IsSystemRole));
        }
        catch (DomainException exception)
        {
            return Result<RoleSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<PermissionSummary>> CreatePermissionAsync(CreatePermissionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var permission = new Permission(command.Module, command.Action, command.Description);
            if (await _repository.GetPermissionByCodeAsync(permission.Code, cancellationToken) is { } existingPermission)
            {
                return Result<PermissionSummary>.Success(new PermissionSummary(
                    existingPermission.Id,
                    existingPermission.Module,
                    existingPermission.Action,
                    existingPermission.Code,
                    existingPermission.Description));
            }

            await _repository.AddPermissionAsync(permission, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(Permission), permission.Id, AuditAction.PermissionChanged, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<PermissionSummary>.Success(new PermissionSummary(permission.Id, permission.Module, permission.Action, permission.Code, permission.Description));
        }
        catch (DomainException exception)
        {
            return Result<PermissionSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<IReadOnlyCollection<PermissionSummary>>> ListPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _repository.ListPermissionsAsync(cancellationToken);
        return Result<IReadOnlyCollection<PermissionSummary>>.Success(permissions
            .OrderBy(permission => permission.Code)
            .Select(permission => new PermissionSummary(permission.Id, permission.Module, permission.Action, permission.Code, permission.Description))
            .ToArray());
    }

    public async Task<Result> AssignRoleAsync(RbacAssignRoleCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserAsync(command.TenantId, command.UserId, cancellationToken);
        var role = await _repository.GetRoleAsync(command.TenantId, command.RoleId, cancellationToken);
        if (user is null || role is null)
        {
            return Result.Failure("User or role not found in tenant.");
        }

        if (await _repository.UserRoleExistsAsync(command.TenantId, user.Id, role.Id, cancellationToken))
        {
            return Result.Success();
        }

        await _repository.AddUserRoleAsync(new UserRole(command.TenantId, user.Id, role.Id), cancellationToken);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(Role), role.Id, AuditAction.RoleAssigned, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> GrantPermissionAsync(RbacGrantPermissionCommand command, CancellationToken cancellationToken = default)
    {
        var role = await _repository.GetRoleAsync(command.TenantId, command.RoleId, cancellationToken);
        var permission = await _repository.GetPermissionAsync(command.PermissionId, cancellationToken);
        if (role is null || permission is null)
        {
            return Result.Failure("Role or permission not found.");
        }

        if (await _repository.RolePermissionExistsAsync(command.TenantId, role.Id, permission.Id, cancellationToken))
        {
            return Result.Success();
        }

        await _repository.AddRolePermissionAsync(new RolePermission(command.TenantId, role.Id, permission.Id), cancellationToken);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(Permission), permission.Id, AuditAction.PermissionChanged, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<RbacAccessDecision>> AuthorizeAsync(RbacAuthorizationQuery query, CancellationToken cancellationToken = default)
    {
        if (query.EntityTenantId.HasValue && query.EntityTenantId.Value != query.TenantId)
        {
            return Result<RbacAccessDecision>.Success(new RbacAccessDecision(false, "Entity belongs to another tenant.", []));
        }

        var permissionCodes = await _repository.GetPermissionCodesAsync(query.TenantId, query.UserId, cancellationToken);
        var matched = permissionCodes
            .Where(permission => string.Equals(permission, query.PermissionCode, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matched.Length == 0)
        {
            await AppendAuditAsync(query.TenantId, query.UserId, "RBAC", null, AuditAction.PermissionDenied, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<RbacAccessDecision>.Success(new RbacAccessDecision(false, "Required permission was not granted.", []));
        }

        return Result<RbacAccessDecision>.Success(new RbacAccessDecision(true, "Access granted.", matched));
    }

    public async Task<Result<UserPermissionSet>> GetUserPermissionsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (await _repository.GetUserAsync(tenantId, userId, cancellationToken) is null)
        {
            return Result<UserPermissionSet>.Failure("User not found in tenant.");
        }

        var roles = await _repository.GetRoleNamesAsync(tenantId, userId, cancellationToken);
        var permissions = await _repository.GetPermissionCodesAsync(tenantId, userId, cancellationToken);
        return Result<UserPermissionSet>.Success(new UserPermissionSet(tenantId, userId, roles, permissions));
    }

    private async Task AppendAuditAsync(Guid? tenantId, Guid? userId, string entityName, Guid? entityId, AuditAction action, CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.FromEvent(
            new AuditEvent(
                entityName,
                entityId,
                action,
                AuditLog.InferCategory(action),
                new AuditContext(tenantId, userId, null, null, null, null, null, null, null),
                new AuditSnapshot(null, null),
                new AuditMetadata("{\"source\":\"rbac\"}"),
                Success: action != AuditAction.PermissionDenied,
                ErrorMessage: action == AuditAction.PermissionDenied ? "Access denied by RBAC." : null),
            _clock.UtcNow);

        await _repository.AddAuditLogAsync(auditLog, cancellationToken);
    }
}
