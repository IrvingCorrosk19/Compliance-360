using Compliance360.Application;
using Compliance360.Application.Rbac;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Identity;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.Rbac;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class RbacFoundationTests
{
    [Fact]
    public async Task CreateRoleAsync_Creates_Tenant_Scoped_Role_And_Audit()
    {
        var fixture = RbacFixture.Create();

        var result = await fixture.Service.CreateRoleAsync(new CreateRoleCommand(fixture.TenantId, "Auditor", false, fixture.AdminUserId));

        Assert.True(result.IsSuccess);
        Assert.Equal(fixture.TenantId, result.Value!.TenantId);
        Assert.Equal("Auditor", result.Value.Name);
        Assert.Single(fixture.Repository.Roles);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.AdministrativeEvent);
    }

    [Fact]
    public async Task CreateRoleAsync_Rejects_Duplicate_Role_Name_Per_Tenant()
    {
        var fixture = RbacFixture.Create();

        await fixture.Service.CreateRoleAsync(new CreateRoleCommand(fixture.TenantId, "Auditor", false, fixture.AdminUserId));
        var duplicate = await fixture.Service.CreateRoleAsync(new CreateRoleCommand(fixture.TenantId, "AUDITOR", false, fixture.AdminUserId));

        Assert.True(duplicate.IsFailure);
        Assert.Equal("Role name already exists for this tenant.", duplicate.Error);
    }

    [Fact]
    public async Task CreatePermissionAsync_Creates_Global_Permission_And_Audit()
    {
        var fixture = RbacFixture.Create();

        var result = await fixture.Service.CreatePermissionAsync(new CreatePermissionCommand("Documents", PermissionAction.Approve, "Approve documents", fixture.AdminUserId, fixture.TenantId));

        Assert.True(result.IsSuccess);
        Assert.Equal("DOCUMENTS.APPROVE", result.Value!.Code);
        Assert.Single(fixture.Repository.Permissions);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.PermissionChanged);
    }

    [Fact]
    public async Task AssignRoleAsync_And_GrantPermissionAsync_Enable_Authorization()
    {
        var fixture = RbacFixture.CreateWithUserRolePermission();

        var assign = await fixture.Service.AssignRoleAsync(new RbacAssignRoleCommand(fixture.TenantId, fixture.User.Id, fixture.Role.Id, fixture.AdminUserId));
        var grant = await fixture.Service.GrantPermissionAsync(new RbacGrantPermissionCommand(fixture.TenantId, fixture.Role.Id, fixture.Permission.Id, fixture.AdminUserId));
        var decision = await fixture.Service.AuthorizeAsync(new RbacAuthorizationQuery(fixture.TenantId, fixture.User.Id, fixture.Permission.Code, fixture.TenantId, null));

        Assert.True(assign.IsSuccess);
        Assert.True(grant.IsSuccess);
        Assert.True(decision.IsSuccess);
        Assert.True(decision.Value!.IsAllowed);
        Assert.Contains(fixture.Permission.Code, decision.Value.MatchedPermissions);
    }

    [Fact]
    public async Task AuthorizeAsync_Denies_Missing_Permission_And_Audits()
    {
        var fixture = RbacFixture.CreateWithUserRolePermission();

        var decision = await fixture.Service.AuthorizeAsync(new RbacAuthorizationQuery(fixture.TenantId, fixture.User.Id, "DOCUMENTS.DELETE", fixture.TenantId, null));

        Assert.True(decision.IsSuccess);
        Assert.False(decision.Value!.IsAllowed);
        Assert.Equal("Required permission was not granted.", decision.Value.Reason);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.PermissionDenied);
    }

    [Fact]
    public async Task AuthorizeAsync_Denies_CrossTenant_Entity_Access()
    {
        var fixture = RbacFixture.CreateWithUserRolePermission();

        var decision = await fixture.Service.AuthorizeAsync(new RbacAuthorizationQuery(fixture.TenantId, fixture.User.Id, fixture.Permission.Code, Guid.NewGuid(), null));

        Assert.True(decision.IsSuccess);
        Assert.False(decision.Value!.IsAllowed);
        Assert.Equal("Entity belongs to another tenant.", decision.Value.Reason);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_Returns_Roles_And_Permissions()
    {
        var fixture = RbacFixture.CreateWithUserRolePermission();
        fixture.User.AssignRole(fixture.Role.Id);
        fixture.Role.GrantPermission(fixture.Permission.Id);

        var result = await fixture.Service.GetUserPermissionsAsync(fixture.TenantId, fixture.User.Id);

        Assert.True(result.IsSuccess);
        Assert.Contains(fixture.Role.Name, result.Value!.Roles);
        Assert.Contains(fixture.Permission.Code, result.Value.Permissions);
    }

    [Fact]
    public async Task Service_Returns_Failures_For_Missing_Resources_And_Invalid_Input()
    {
        var fixture = RbacFixture.Create();

        var invalidRole = await fixture.Service.CreateRoleAsync(new CreateRoleCommand(fixture.TenantId, "", false, fixture.AdminUserId));
        var invalidPermission = await fixture.Service.CreatePermissionAsync(new CreatePermissionCommand("", PermissionAction.Read, "", fixture.AdminUserId, fixture.TenantId));
        var assign = await fixture.Service.AssignRoleAsync(new RbacAssignRoleCommand(fixture.TenantId, Guid.NewGuid(), Guid.NewGuid(), fixture.AdminUserId));
        var grant = await fixture.Service.GrantPermissionAsync(new RbacGrantPermissionCommand(fixture.TenantId, Guid.NewGuid(), Guid.NewGuid(), fixture.AdminUserId));
        var permissions = await fixture.Service.GetUserPermissionsAsync(fixture.TenantId, Guid.NewGuid());

        Assert.True(invalidRole.IsFailure);
        Assert.True(invalidPermission.IsFailure);
        Assert.True(assign.IsFailure);
        Assert.True(grant.IsFailure);
        Assert.True(permissions.IsFailure);
    }

    [Fact]
    public async Task EfRbacRepository_Queries_Tenant_Scoped_Rbac_Graph()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfRbacRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var user = new User(tenantId, "qa@example.com", "Quality Manager");
        var role = new Role(tenantId, "Approver");
        var permission = new Permission("Documents", PermissionAction.Approve, "Approve documents");
        user.AssignRole(role.Id);
        role.GrantPermission(permission.Id);

        await dbContext.Users.AddAsync(user);
        await repository.AddRoleAsync(role);
        await repository.AddPermissionAsync(permission);
        await repository.AddAuditLogAsync(AuditLog.Create(tenantId, user.Id, nameof(Role), role.Id, AuditAction.RoleAssigned, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();

        var loadedUser = await repository.GetUserAsync(tenantId, user.Id);
        var loadedRole = await repository.GetRoleAsync(tenantId, role.Id);
        var loadedPermission = await repository.GetPermissionByCodeAsync(permission.Code);
        var roleNames = await repository.GetRoleNamesAsync(tenantId, user.Id);
        var permissionCodes = await repository.GetPermissionCodesAsync(tenantId, user.Id);

        Assert.NotNull(loadedUser);
        Assert.NotNull(loadedRole);
        Assert.NotNull(loadedPermission);
        Assert.Contains("Approver", roleNames);
        Assert.Contains("DOCUMENTS.APPROVE", permissionCodes);
        Assert.True(await repository.RoleNameExistsAsync(tenantId, "APPROVER"));
        Assert.Single(dbContext.AuditLogs);
    }

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class RbacFixture
    {
        private RbacFixture()
        {
            TenantId = Guid.NewGuid();
            AdminUserId = Guid.NewGuid();
            Repository = new InMemoryRbacRepository();
            Service = new RbacService(Repository, new FakeApplicationDbContext(), new FixedClock());
            User = new User(TenantId, "qa@example.com", "Quality Manager");
            Role = new Role(TenantId, "Approver");
            Permission = new Permission("Documents", PermissionAction.Approve, "Approve documents");
        }

        public Guid TenantId { get; }

        public Guid AdminUserId { get; }

        public InMemoryRbacRepository Repository { get; }

        public RbacService Service { get; }

        public User User { get; }

        public Role Role { get; }

        public Permission Permission { get; }

        public static RbacFixture Create()
        {
            return new RbacFixture();
        }

        public static RbacFixture CreateWithUserRolePermission()
        {
            var fixture = Create();
            fixture.Repository.Users.Add(fixture.User);
            fixture.Repository.Roles.Add(fixture.Role);
            fixture.Repository.Permissions.Add(fixture.Permission);
            return fixture;
        }
    }

    private sealed class InMemoryRbacRepository : IRbacRepository
    {
        public List<User> Users { get; } = [];
        public List<Role> Roles { get; } = [];
        public List<Permission> Permissions { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];

        public Task<User?> GetUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Users.SingleOrDefault(user => user.TenantId == tenantId && user.Id == userId));
        }

        public Task<Role?> GetRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Roles.SingleOrDefault(role => role.TenantId == tenantId && role.Id == roleId));
        }

        public Task<Permission?> GetPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Permissions.SingleOrDefault(permission => permission.Id == permissionId));
        }

        public Task<Permission?> GetPermissionByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Permissions.SingleOrDefault(permission => permission.Code == code.ToUpperInvariant()));
        }

        public Task<bool> RoleNameExistsAsync(Guid tenantId, string normalizedName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Roles.Any(role => role.TenantId == tenantId && role.NormalizedName == normalizedName));
        }

        public Task AddRoleAsync(Role role, CancellationToken cancellationToken = default)
        {
            Roles.Add(role);
            return Task.CompletedTask;
        }

        public Task AddPermissionAsync(Permission permission, CancellationToken cancellationToken = default)
        {
            Permissions.Add(permission);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<string>> GetRoleNamesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        {
            var roleNames = Users.SingleOrDefault(user => user.Id == userId && user.TenantId == tenantId)?.Roles
                .Join(Roles, userRole => userRole.RoleId, role => role.Id, (_, role) => role.Name)
                .ToList() ?? [];
            return Task.FromResult<IReadOnlyCollection<string>>(roleNames);
        }

        public Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        {
            var roleIds = Users.SingleOrDefault(user => user.Id == userId && user.TenantId == tenantId)?.Roles.Select(role => role.RoleId).ToHashSet() ?? [];
            var permissionIds = Roles.Where(role => roleIds.Contains(role.Id)).SelectMany(role => role.Permissions).Select(permission => permission.PermissionId).ToHashSet();
            var permissions = Permissions.Where(permission => permissionIds.Contains(permission.Id)).Select(permission => permission.Code).ToList();
            return Task.FromResult<IReadOnlyCollection<string>>(permissions);
        }

        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            AuditLogs.Add(auditLog);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 6, 20, 17, 0, 0, TimeSpan.Zero);
    }
}
