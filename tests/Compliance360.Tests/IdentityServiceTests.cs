using Compliance360.Application;
using Compliance360.Application.Identity;
using Compliance360.Application.Mfa;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Identity;
using Compliance360.Infrastructure.Identity;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.Security;
using Compliance360.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class IdentityServiceTests
{
    [Fact]
    public async Task LoginAsync_Returns_Tokens_And_Creates_Audit()
    {
        var fixture = IdentityFixture.Create();
        var result = await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "Password1!", "127.0.0.1", "test"));

        Assert.True(result.IsSuccess);
        Assert.Equal(fixture.User.Id, result.Value!.UserId);
        Assert.Equal(fixture.TenantId, result.Value.TenantId);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
        Assert.False(result.Value.MfaRequired);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.LoginSucceeded);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.Created && audit.EntityName == nameof(UserSession));
    }

    [Fact]
    public async Task LoginAsync_With_User_Mfa_Returns_Challenge_Without_Tokens()
    {
        var fixture = IdentityFixture.Create();
        fixture.EnableTotpMfa();

        var result = await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "Password1!", "127.0.0.1", "test"));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.MfaRequired);
        Assert.NotNull(result.Value.MfaChallengeToken);
        Assert.Equal(string.Empty, result.Value.AccessToken);
        Assert.Equal(string.Empty, result.Value.RefreshToken);
        Assert.Empty(fixture.Repository.RefreshTokens);
        Assert.Empty(fixture.Repository.UserSessions);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.MfaChallengeRequired);
    }

    [Fact]
    public async Task LoginAsync_With_Tenant_RequireMfa_Returns_Challenge()
    {
        var fixture = IdentityFixture.Create();
        fixture.Repository.TenantRequiresMfa = true;
        fixture.EnableTotpMfa(enableUserFlag: false);

        var result = await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "Password1!", null, null));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.MfaRequired);
        Assert.NotNull(result.Value.MfaChallengeToken);
        Assert.Equal(string.Empty, result.Value.AccessToken);
    }

    [Fact]
    public async Task CompleteMfaChallengeAsync_With_Valid_Code_Issues_Final_Tokens()
    {
        var fixture = IdentityFixture.Create();
        fixture.EnableTotpMfa();
        var login = await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "Password1!", null, null));

        var completed = await fixture.Service.CompleteMfaChallengeAsync(new CompleteMfaChallengeCommand(login.Value!.MfaChallengeToken!, MfaMethod.Totp, TestTotpService.ValidCode, "127.0.0.1", "test"));

        Assert.True(completed.IsSuccess);
        Assert.False(completed.Value!.MfaRequired);
        Assert.False(string.IsNullOrWhiteSpace(completed.Value.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(completed.Value.RefreshToken));
        Assert.Single(fixture.Repository.RefreshTokens);
        Assert.Single(fixture.Repository.UserSessions);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.MfaChallengeSucceeded);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.LoginSucceeded);
    }

    [Fact]
    public async Task CompleteMfaChallengeAsync_With_Invalid_Code_Does_Not_Issue_Tokens()
    {
        var fixture = IdentityFixture.Create();
        fixture.EnableTotpMfa();
        var login = await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "Password1!", null, null));

        var completed = await fixture.Service.CompleteMfaChallengeAsync(new CompleteMfaChallengeCommand(login.Value!.MfaChallengeToken!, MfaMethod.Totp, "000000", null, null));

        Assert.True(completed.IsFailure);
        Assert.Empty(fixture.Repository.RefreshTokens);
        Assert.Empty(fixture.Repository.UserSessions);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.MfaChallengeFailed);
    }

    [Fact]
    public async Task LoginAsync_Rejects_Wrong_Tenant()
    {
        var fixture = IdentityFixture.Create();
        var result = await fixture.Service.LoginAsync(new LoginCommand(Guid.NewGuid(), fixture.User.Email, "Password1!", null, null));

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid credentials.", result.Error);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.LoginFailed);
    }

    [Fact]
    public async Task LoginAsync_Locks_Account_After_Failed_Attempts()
    {
        var fixture = IdentityFixture.Create(maxFailedAttempts: 2);

        await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "bad", null, null));
        var secondFailure = await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "bad", null, null));
        var lockedLogin = await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "Password1!", null, null));

        Assert.True(secondFailure.IsFailure);
        Assert.True(lockedLogin.IsFailure);
        Assert.Equal(UserStatus.Locked, fixture.User.Status);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.AccountLocked);
    }

    [Fact]
    public async Task RefreshTokenAsync_Rotates_Refresh_Token()
    {
        var fixture = IdentityFixture.Create();
        var login = await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "Password1!", null, null));

        var refresh = await fixture.Service.RefreshTokenAsync(new RefreshTokenCommand(login.Value!.RefreshToken, null, null));
        var oldToken = fixture.Repository.RefreshTokens.Single(token => token.TokenHash == login.Value.RefreshTokenHash);

        Assert.True(refresh.IsSuccess);
        Assert.NotEqual(login.Value.RefreshTokenHash, refresh.Value!.RefreshTokenHash);
        Assert.NotNull(oldToken.RevokedAtUtc);
        Assert.Equal(refresh.Value.RefreshTokenHash, oldToken.ReplacedByTokenHash);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.TokenRefreshed);
    }

    [Fact]
    public async Task LogoutAsync_Revokes_Refresh_Token()
    {
        var fixture = IdentityFixture.Create();
        var login = await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "Password1!", null, null));

        var logout = await fixture.Service.LogoutAsync(new LogoutCommand(fixture.TenantId, fixture.User.Id, login.Value!.RefreshTokenHash, null, null));

        Assert.True(logout.IsSuccess);
        Assert.False(fixture.Repository.RefreshTokens.Single(token => token.TokenHash == login.Value.RefreshTokenHash).IsActive(fixture.Clock.UtcNow));
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.Logout);
    }

    [Fact]
    public async Task ChangePasswordAsync_Validates_Current_Password_And_History()
    {
        var fixture = IdentityFixture.Create();

        var changed = await fixture.Service.ChangePasswordAsync(new ChangePasswordCommand(
            fixture.TenantId,
            fixture.User.Id,
            "Password1!",
            "Password2!",
            null,
            null));
        var reused = await fixture.Service.ChangePasswordAsync(new ChangePasswordCommand(
            fixture.TenantId,
            fixture.User.Id,
            "Password2!",
            "Password1!",
            null,
            null));

        Assert.True(changed.IsSuccess);
        Assert.True(reused.IsFailure);
        Assert.Equal("Password was used recently.", reused.Error);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.PasswordChanged);
    }

    [Fact]
    public async Task ChangePasswordAsync_Rejects_Weak_Or_Invalid_Current_Password()
    {
        var fixture = IdentityFixture.Create();

        var invalidCurrent = await fixture.Service.ChangePasswordAsync(new ChangePasswordCommand(fixture.TenantId, fixture.User.Id, "bad", "Password2!", null, null));
        var weakPassword = await fixture.Service.ChangePasswordAsync(new ChangePasswordCommand(fixture.TenantId, fixture.User.Id, "Password1!", "weak", null, null));

        Assert.True(invalidCurrent.IsFailure);
        Assert.True(weakPassword.IsFailure);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.LoginFailed);
    }

    [Fact]
    public async Task AssignRoleAsync_And_GrantPermissionAsync_Update_Rbac()
    {
        var fixture = IdentityFixture.Create();

        var assignRole = await fixture.Service.AssignRoleAsync(new AssignRoleCommand(fixture.TenantId, fixture.User.Id, fixture.Role.Id, fixture.AdminUserId));
        var grantPermission = await fixture.Service.GrantPermissionAsync(new GrantPermissionCommand(fixture.TenantId, fixture.Role.Id, fixture.Permission.Id, fixture.AdminUserId));

        Assert.True(assignRole.IsSuccess);
        Assert.True(grantPermission.IsSuccess);
        Assert.Single(fixture.User.Roles);
        Assert.Single(fixture.Role.Permissions);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.RoleAssigned);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.PermissionChanged);
    }

    [Fact]
    public async Task ConfigureMfaAsync_Enables_Mfa_And_Creates_Configuration()
    {
        var fixture = IdentityFixture.Create();

        var result = await fixture.Service.ConfigureMfaAsync(new ConfigureMfaCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, "encrypted-secret", fixture.AdminUserId));

        Assert.True(result.IsSuccess);
        Assert.True(fixture.User.MfaEnabled);
        Assert.Single(fixture.Repository.MfaConfigurations);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.MfaConfigured);
    }

    [Fact]
    public async Task UnlockAccountAsync_Clears_Lockout()
    {
        var fixture = IdentityFixture.Create(maxFailedAttempts: 1);
        await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "bad", null, null));

        var result = await fixture.Service.UnlockAccountAsync(new UnlockAccountCommand(fixture.TenantId, fixture.User.Id, fixture.AdminUserId));

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Active, fixture.User.Status);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.AccountUnlocked);
    }

    [Fact]
    public async Task Service_Returns_Failure_For_Missing_Rbac_Mfa_And_Session_Resources()
    {
        var fixture = IdentityFixture.Create();

        var assignRole = await fixture.Service.AssignRoleAsync(new AssignRoleCommand(fixture.TenantId, Guid.NewGuid(), fixture.Role.Id, fixture.AdminUserId));
        var grantPermission = await fixture.Service.GrantPermissionAsync(new GrantPermissionCommand(fixture.TenantId, Guid.NewGuid(), fixture.Permission.Id, fixture.AdminUserId));
        var configureMfa = await fixture.Service.ConfigureMfaAsync(new ConfigureMfaCommand(fixture.TenantId, Guid.NewGuid(), MfaMethod.Totp, "secret", fixture.AdminUserId));
        var unlock = await fixture.Service.UnlockAccountAsync(new UnlockAccountCommand(fixture.TenantId, Guid.NewGuid(), fixture.AdminUserId));
        var logout = await fixture.Service.LogoutAsync(new LogoutCommand(fixture.TenantId, fixture.User.Id, "missing-hash", null, null));
        var refresh = await fixture.Service.RefreshTokenAsync(new RefreshTokenCommand("missing-token", null, null));

        Assert.True(assignRole.IsFailure);
        Assert.True(grantPermission.IsFailure);
        Assert.True(configureMfa.IsFailure);
        Assert.True(unlock.IsFailure);
        Assert.True(logout.IsFailure);
        Assert.True(refresh.IsFailure);
    }

    [Fact]
    public async Task RefreshTokenAsync_Rejects_Disabled_Account()
    {
        var fixture = IdentityFixture.Create();
        var login = await fixture.Service.LoginAsync(new LoginCommand(fixture.TenantId, fixture.User.Email, "Password1!", null, null));
        fixture.User.Disable();

        var refresh = await fixture.Service.RefreshTokenAsync(new RefreshTokenCommand(login.Value!.RefreshToken, null, null));

        Assert.True(refresh.IsFailure);
        Assert.Equal("Account is not available.", refresh.Error);
    }

    [Fact]
    public void PasswordHasher_Hashes_And_Verifies_Securely()
    {
        var hasher = new Pbkdf2PasswordHasher();

        var hash = hasher.HashPassword("Password1!");

        Assert.Equal(PasswordVerificationResult.Success, hasher.Verify("Password1!", hash));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify("wrong", hash));
        Assert.Equal(PasswordVerificationResult.Failed, hasher.Verify("Password1!", "invalid"));
    }

    [Fact]
    public void JwtTokenService_Generates_Access_Token()
    {
        var service = new JwtTokenService(
            Options.Create(new JwtOptions
            {
                SigningKey = "0123456789012345678901234567890123456789012345678901234567890123",
                AccessTokenMinutes = 15
            }),
            new FixedClock());

        var token = service.CreateAccessToken(new AuthenticatedUser(Guid.NewGuid(), Guid.NewGuid(), "qa@example.com", "QA", ["Admin"], ["TENANTS.READ"]));

        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
        Assert.Equal(new FixedClock().UtcNow.AddMinutes(15), token.ExpiresAtUtc);
    }

    [Fact]
    public void MfaChallengeTokenService_Validates_Signed_Expiry_And_Tamper_Paths()
    {
        var clock = new MutableClock(new DateTimeOffset(2026, 6, 20, 15, 0, 0, TimeSpan.Zero));
        var service = new MfaChallengeTokenService(
            Options.Create(new JwtOptions { SigningKey = "0123456789012345678901234567890123456789012345678901234567890123" }),
            Options.Create(new MfaChallengeOptions { LifetimeMinutes = 5 }),
            clock);
        var principal = new MfaChallengePrincipal(Guid.NewGuid(), Guid.NewGuid(), MfaMethod.Totp, clock.UtcNow.AddMinutes(5));

        var token = service.Create(principal);
        var valid = service.Validate(token);
        var tampered = service.Validate(token.Replace(token[0], token[0] == 'A' ? 'B' : 'A'));
        clock.UtcNow = clock.UtcNow.AddMinutes(6);
        var expired = service.Validate(token);
        var malformed = service.Validate("not-a-token");
        var empty = service.Validate("");
        var renewedExpiredPrincipalToken = service.Create(principal with { ExpiresAtUtc = clock.UtcNow.AddMinutes(-1) });
        var renewedExpiredPrincipal = service.Validate(renewedExpiredPrincipalToken);
        var invalidShape = service.Validate(SignMfaChallengePayload("too|short"));
        var invalidTenant = service.Validate(SignMfaChallengePayload($"bad|{principal.UserId}|0|{clock.UtcNow.AddMinutes(5).ToUnixTimeSeconds()}"));
        var invalidUser = service.Validate(SignMfaChallengePayload($"{principal.TenantId}|bad|0|{clock.UtcNow.AddMinutes(5).ToUnixTimeSeconds()}"));
        var invalidMethod = service.Validate(SignMfaChallengePayload($"{principal.TenantId}|{principal.UserId}|99|{clock.UtcNow.AddMinutes(5).ToUnixTimeSeconds()}"));
        var invalidExpiry = service.Validate(SignMfaChallengePayload($"{principal.TenantId}|{principal.UserId}|0|bad"));

        Assert.True(valid.IsSuccess);
        Assert.Equal(principal.UserId, valid.Value!.UserId);
        Assert.True(tampered.IsFailure);
        Assert.True(expired.IsFailure);
        Assert.True(malformed.IsFailure);
        Assert.True(empty.IsFailure);
        Assert.True(renewedExpiredPrincipal.IsSuccess);
        Assert.True(renewedExpiredPrincipal.Value!.ExpiresAtUtc > clock.UtcNow);
        Assert.True(invalidShape.IsFailure);
        Assert.True(invalidTenant.IsFailure);
        Assert.True(invalidUser.IsFailure);
        Assert.True(invalidMethod.IsFailure);
        Assert.True(invalidExpiry.IsFailure);
    }

    [Fact]
    public void RefreshTokenGenerator_Creates_Hashed_Token()
    {
        var generator = new RefreshTokenGenerator(new FixedClock(), Options.Create(new RefreshTokenOptions { LifetimeDays = 10 }));

        var token = generator.Generate();

        Assert.False(string.IsNullOrWhiteSpace(token.PlainTextToken));
        Assert.Equal(64, token.TokenHash.Length);
        Assert.Equal(new FixedClock().UtcNow.AddDays(10), token.ExpiresAtUtc);
    }

    [Fact]
    public void PasswordPolicyValidator_Returns_Specific_Failures()
    {
        var validator = new PasswordPolicyValidator(Options.Create(new PasswordPolicyOptions { MinimumLength = 8 }));

        Assert.True(validator.Validate("").IsFailure);
        Assert.True(validator.Validate("Short1!").IsFailure);
        Assert.True(validator.Validate("password1!").IsFailure);
        Assert.True(validator.Validate("PASSWORD1!").IsFailure);
        Assert.True(validator.Validate("Password!").IsFailure);
        Assert.True(validator.Validate("Password1").IsFailure);
        Assert.True(validator.Validate("Password1!").IsSuccess);
    }

    [Fact]
    public void User_Domain_Rules_Protect_Tenant_Scoped_Children()
    {
        var user = new User(Guid.NewGuid(), "qa@example.com", "Quality Manager");
        var otherTenant = Guid.NewGuid();
        var refreshToken = new RefreshToken(otherTenant, user.Id, "hash", DateTimeOffset.UtcNow.AddDays(1));
        var session = new UserSession(otherTenant, user.Id, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));

        Assert.Throws<DomainException>(() => user.AssignToCompany(Guid.Empty));
        Assert.Throws<DomainException>(() => user.AssignRole(Guid.Empty));
        Assert.Throws<DomainException>(() => user.AddRefreshToken(refreshToken));
        Assert.Throws<DomainException>(() => user.AddSession(session));
    }

    [Fact]
    public void Identity_Domain_Objects_Expose_State_Transitions()
    {
        var tenantId = Guid.NewGuid();
        var user = new User(tenantId, "qa@example.com", "Quality Manager");
        var role = new Role(tenantId, "Approver", isSystemRole: true);
        var permission = new Permission("Documents", PermissionAction.Approve, "Approve documents");
        var session = new UserSession(tenantId, user.Id, new FixedClock().UtcNow, new FixedClock().UtcNow.AddMinutes(30));
        var mfa = new MfaConfiguration(tenantId, user.Id, MfaMethod.Email, "secret", new FixedClock().UtcNow);

        user.AssignToCompany(Guid.NewGuid());
        user.SetPasswordHash("hash");
        user.DisableMfa();
        user.Disable();
        user.Unlock();
        role.GrantPermission(permission.Id);
        role.GrantPermission(permission.Id);
        session.Revoke(new FixedClock().UtcNow);
        mfa.Disable();

        Assert.Equal(UserStatus.Active, user.Status);
        Assert.NotNull(user.CompanyId);
        Assert.False(user.MfaEnabled);
        Assert.True(role.IsSystemRole);
        Assert.Equal("DOCUMENTS.APPROVE", permission.Code);
        Assert.Single(role.Permissions);
        Assert.False(session.IsActive(new FixedClock().UtcNow));
        Assert.False(mfa.IsEnabled);
    }

    [Fact]
    public void RefreshToken_Reports_Expired_And_Linked_State()
    {
        var refreshToken = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), "hash", new FixedClock().UtcNow.AddMinutes(-1));
        var sessionId = Guid.NewGuid();

        refreshToken.LinkSession(sessionId);

        Assert.False(refreshToken.IsActive(new FixedClock().UtcNow));
        Assert.Equal(sessionId, refreshToken.SessionId);
    }

    [Fact]
    public void SystemClock_Returns_Current_Utc_Time()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var now = new SystemClock().UtcNow;
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        Assert.InRange(now, before, after);
    }

    [Fact]
    public async Task EfIdentityRepository_Persists_And_Loads_Identity_Graph()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new Compliance360DbContext(options, new FixedClock());
        var repository = new EfIdentityRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var user = new User(tenantId, "qa@example.com", "Quality Manager");
        user.SetPasswordHash("hash");
        var role = new Role(tenantId, "Admin");
        var permission = new Permission("Identity", PermissionAction.Manage, "Manage identity");
        user.AssignRole(role.Id);
        role.GrantPermission(permission.Id);
        var refreshToken = new RefreshToken(tenantId, user.Id, "refresh-hash", new FixedClock().UtcNow.AddDays(1));
        var session = new UserSession(tenantId, user.Id, new FixedClock().UtcNow, new FixedClock().UtcNow.AddDays(1));
        var mfa = new MfaConfiguration(tenantId, user.Id, MfaMethod.Totp, "secret", new FixedClock().UtcNow);
        var audit = AuditLog.Create(tenantId, user.Id, nameof(User), user.Id, AuditAction.LoginSucceeded, new FixedClock().UtcNow);

        await dbContext.Users.AddAsync(user);
        await dbContext.Roles.AddAsync(role);
        await dbContext.Permissions.AddAsync(permission);
        await repository.AddRefreshTokenAsync(refreshToken);
        await repository.AddSessionAsync(session);
        await repository.AddMfaConfigurationAsync(mfa);
        await repository.AddAuditLogAsync(audit);
        await dbContext.SaveChangesAsync();

        var byEmail = await repository.GetUserByEmailAsync(tenantId, user.NormalizedEmail);
        var byId = await repository.GetUserByIdAsync(tenantId, user.Id);
        var loadedRole = await repository.GetRoleByIdAsync(tenantId, role.Id);
        var loadedPermission = await repository.GetPermissionByIdAsync(permission.Id);
        var loadedToken = await repository.GetRefreshTokenByHashAsync("refresh-hash");
        var roles = await repository.GetRoleNamesAsync(tenantId, user.Id);
        var permissions = await repository.GetPermissionCodesAsync(tenantId, user.Id);

        Assert.NotNull(byEmail);
        Assert.NotNull(byId);
        Assert.NotNull(loadedRole);
        Assert.NotNull(loadedPermission);
        Assert.NotNull(loadedToken);
        Assert.Contains("Admin", roles);
        Assert.Contains("IDENTITY.MANAGE", permissions);
        Assert.Single(dbContext.MfaConfigurations);
        Assert.Single(dbContext.AuditLogs);
    }

    private sealed class IdentityFixture
    {
        private IdentityFixture(int maxFailedAttempts)
        {
            TenantId = Guid.NewGuid();
            AdminUserId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryIdentityRepository();
            PasswordHasher = new TestPasswordHasher();
            User = new User(TenantId, "qa@example.com", "Quality Manager");
            User.SetPasswordHash(PasswordHasher.HashPassword("Password1!"));
            Role = new Role(TenantId, "Tenant Admin");
            Permission = new Permission("Tenants", PermissionAction.Manage, "Manage tenants");
            Repository.Users.Add(User);
            Repository.Roles.Add(Role);
            Repository.Permissions.Add(Permission);
            Service = new IdentityService(
                Repository,
                new FakeApplicationDbContext(),
                PasswordHasher,
                new PasswordPolicyValidator(Options.Create(new PasswordPolicyOptions { MinimumLength = 8 })),
                new TestJwtTokenService(Clock),
                new TestRefreshTokenGenerator(Clock),
                new TestMfaChallengeTokenService(Clock),
                new TestMfaSecretProtector(),
                new TestTotpService(),
                Clock,
                Options.Create(new LockoutOptions { MaxFailedAttempts = maxFailedAttempts, LockoutMinutes = 15 }),
                Options.Create(new PasswordPolicyOptions { MinimumLength = 8, PasswordHistoryLimit = 5 }));
        }

        public Guid TenantId { get; }

        public Guid AdminUserId { get; }

        public FixedClock Clock { get; }

        public InMemoryIdentityRepository Repository { get; }

        public TestPasswordHasher PasswordHasher { get; }

        public User User { get; }

        public Role Role { get; }

        public Permission Permission { get; }

        public IdentityService Service { get; }

        public static IdentityFixture Create(int maxFailedAttempts = 5)
        {
            return new IdentityFixture(maxFailedAttempts);
        }

        public void EnableTotpMfa(bool enableUserFlag = true)
        {
            var configuration = new MfaConfiguration(TenantId, User.Id, MfaMethod.Totp, "secret", Clock.UtcNow);
            Repository.MfaConfigurations.Add(configuration);
            if (enableUserFlag)
            {
                User.EnableMfa("secret");
            }
        }
    }

    private sealed class InMemoryIdentityRepository : IIdentityRepository
    {
        public List<User> Users { get; } = [];

        public List<Role> Roles { get; } = [];

        public List<Permission> Permissions { get; } = [];

        public List<RefreshToken> RefreshTokens { get; } = [];

        public List<UserSession> UserSessions { get; } = [];

        public List<MfaConfiguration> MfaConfigurations { get; } = [];

        public List<AuditLog> AuditLogs { get; } = [];

        public bool TenantRequiresMfa { get; set; }

        public Task<User?> GetUserByEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Users.SingleOrDefault(user => user.TenantId == tenantId && user.NormalizedEmail == normalizedEmail));
        }

        public Task<User?> GetUserByIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Users.SingleOrDefault(user => user.TenantId == tenantId && user.Id == userId));
        }

        public Task<Role?> GetRoleByIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Roles.SingleOrDefault(role => role.TenantId == tenantId && role.Id == roleId));
        }

        public Task<Permission?> GetPermissionByIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Permissions.SingleOrDefault(permission => permission.Id == permissionId));
        }

        public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(RefreshTokens.SingleOrDefault(token => token.TokenHash == tokenHash));
        }

        public Task<bool> IsTenantMfaRequiredAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TenantRequiresMfa);
        }

        public Task<MfaConfiguration?> GetEnabledMfaConfigurationAsync(Guid tenantId, Guid userId, MfaMethod method, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(MfaConfigurations.SingleOrDefault(configuration =>
                configuration.TenantId == tenantId
                && configuration.UserId == userId
                && configuration.Method == method
                && configuration.IsEnabled));
        }

        public Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshTokens.Add(refreshToken);
            return Task.CompletedTask;
        }

        public Task AddSessionAsync(UserSession session, CancellationToken cancellationToken = default)
        {
            UserSessions.Add(session);
            return Task.CompletedTask;
        }

        public Task AddMfaConfigurationAsync(MfaConfiguration configuration, CancellationToken cancellationToken = default)
        {
            MfaConfigurations.Add(configuration);
            return Task.CompletedTask;
        }

        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            AuditLogs.Add(auditLog);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<string>> GetRoleNamesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        {
            var roleNames = Users.Single(user => user.Id == userId).Roles
                .Join(Roles, userRole => userRole.RoleId, role => role.Id, (_, role) => role.Name)
                .ToList();
            return Task.FromResult<IReadOnlyCollection<string>>(roleNames);
        }

        public Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        {
            var roleIds = Users.Single(user => user.Id == userId).Roles.Select(userRole => userRole.RoleId).ToHashSet();
            var permissionIds = Roles
                .Where(role => roleIds.Contains(role.Id))
                .SelectMany(role => role.Permissions)
                .Select(rolePermission => rolePermission.PermissionId)
                .ToHashSet();
            var permissions = Permissions.Where(permission => permissionIds.Contains(permission.Id)).Select(permission => permission.Code).ToList();
            return Task.FromResult<IReadOnlyCollection<string>>(permissions);
        }
    }

    private sealed class TestPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return $"hashed:{password}";
        }

        public PasswordVerificationResult Verify(string password, string passwordHash)
        {
            return passwordHash == HashPassword(password)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }

    private sealed class TestJwtTokenService : IJwtTokenService
    {
        private readonly IClock _clock;

        public TestJwtTokenService(IClock clock)
        {
            _clock = clock;
        }

        public JwtTokenResult CreateAccessToken(AuthenticatedUser user)
        {
            return new JwtTokenResult($"access:{user.UserId}:{user.TenantId}", _clock.UtcNow.AddMinutes(15));
        }
    }

    private sealed class TestRefreshTokenGenerator : IRefreshTokenGenerator
    {
        private int _counter;
        private readonly IClock _clock;

        public TestRefreshTokenGenerator(IClock clock)
        {
            _clock = clock;
        }

        public GeneratedRefreshToken Generate()
        {
            _counter++;
            var plainText = $"refresh-token-{_counter}";
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plainText)));
            return new GeneratedRefreshToken(plainText, hash, _clock.UtcNow.AddDays(30));
        }
    }

    private sealed class TestMfaChallengeTokenService : IMfaChallengeTokenService
    {
        private readonly IClock _clock;

        public TestMfaChallengeTokenService(IClock clock)
        {
            _clock = clock;
        }

        public string Create(MfaChallengePrincipal principal)
        {
            return $"{principal.TenantId}|{principal.UserId}|{(int)principal.Method}|{principal.ExpiresAtUtc.ToUnixTimeSeconds()}";
        }

        public Result<MfaChallengePrincipal> Validate(string challengeToken)
        {
            var parts = challengeToken.Split('|');
            if (parts.Length != 4
                || !Guid.TryParse(parts[0], out var tenantId)
                || !Guid.TryParse(parts[1], out var userId)
                || !int.TryParse(parts[2], out var method)
                || !long.TryParse(parts[3], out var expiresUnix))
            {
                return Result<MfaChallengePrincipal>.Failure("Invalid MFA challenge.");
            }

            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresUnix);
            return expiresAt <= _clock.UtcNow
                ? Result<MfaChallengePrincipal>.Failure("MFA challenge expired.")
                : Result<MfaChallengePrincipal>.Success(new MfaChallengePrincipal(tenantId, userId, (MfaMethod)method, expiresAt));
        }
    }

    private static string SignMfaChallengePayload(string payload)
    {
        const string signingKey = "0123456789012345678901234567890123456789012345678901234567890123";
        var payloadSegment = Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(payload));
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(signingKey));
        var signatureSegment = Base64UrlEncode(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payloadSegment)));
        return $"{payloadSegment}.{signatureSegment}";
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private sealed class TestMfaSecretProtector : IMfaSecretProtector
    {
        public string Protect(string secret) => secret;

        public string Unprotect(string encryptedSecret) => encryptedSecret;
    }

    private sealed class TestTotpService : ITotpService
    {
        public const string ValidCode = "123456";

        public string GenerateSecret() => "secret";

        public string GenerateCode(string secret, DateTimeOffset timestampUtc) => ValidCode;

        public bool VerifyCode(string secret, string code, DateTimeOffset timestampUtc, int allowedDriftSteps = 1) =>
            code == ValidCode;
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
        public DateTimeOffset UtcNow => new(2026, 6, 20, 15, 0, 0, TimeSpan.Zero);
    }

    private sealed class MutableClock : IClock
    {
        public MutableClock(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; set; }
    }
}
