using Compliance360.Application;
using Compliance360.Application.Mfa;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Identity;
using Compliance360.Infrastructure.Mfa;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class MfaFoundationTests
{
    [Fact]
    public async Task BeginSetupAsync_Creates_Disabled_Configuration_And_Audit()
    {
        var fixture = MfaFixture.Create();

        var result = await fixture.Service.BeginSetupAsync(new BeginMfaSetupCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, fixture.AdminUserId));

        Assert.True(result.IsSuccess);
        Assert.Equal(fixture.User.Id, result.Value!.UserId);
        Assert.Equal(result.Value.SharedSecret, result.Value.ManualEntryKey);
        Assert.Single(fixture.Repository.Configurations);
        Assert.False(fixture.Repository.Configurations.Single().IsEnabled);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.MfaConfigured);
    }

    [Fact]
    public async Task BeginSetupAsync_Rejects_Missing_User_And_Duplicate_Method()
    {
        var fixture = MfaFixture.Create();

        var missing = await fixture.Service.BeginSetupAsync(new BeginMfaSetupCommand(fixture.TenantId, Guid.NewGuid(), MfaMethod.Totp, fixture.AdminUserId));
        await fixture.Service.BeginSetupAsync(new BeginMfaSetupCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, fixture.AdminUserId));
        var duplicate = await fixture.Service.BeginSetupAsync(new BeginMfaSetupCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, fixture.AdminUserId));

        Assert.True(missing.IsFailure);
        Assert.True(duplicate.IsFailure);
    }

    [Fact]
    public async Task EnableAsync_Verifies_Code_And_Enables_User_Mfa()
    {
        var fixture = MfaFixture.Create();
        var setup = await fixture.Service.BeginSetupAsync(new BeginMfaSetupCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, fixture.AdminUserId));
        var code = fixture.TotpService.GenerateCode(setup.Value!.SharedSecret, fixture.Clock.UtcNow);

        var result = await fixture.Service.EnableAsync(new EnableMfaCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, code, fixture.AdminUserId));

        Assert.True(result.IsSuccess);
        Assert.True(fixture.User.MfaEnabled);
        Assert.True(fixture.Repository.Configurations.Single().IsEnabled);
        Assert.Equal(fixture.Clock.UtcNow, fixture.Repository.Configurations.Single().LastVerifiedAtUtc);
    }

    [Fact]
    public async Task EnableAsync_Rejects_Invalid_Code_And_Tracks_Failure()
    {
        var fixture = MfaFixture.Create();
        await fixture.Service.BeginSetupAsync(new BeginMfaSetupCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, fixture.AdminUserId));

        var result = await fixture.Service.EnableAsync(new EnableMfaCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, "000000", fixture.AdminUserId));

        Assert.True(result.IsFailure);
        Assert.Equal(1, fixture.Repository.Configurations.Single().FailedAttempts);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.SecurityEvent && !audit.Success);
    }

    [Fact]
    public async Task VerifyAsync_Validates_Enabled_Mfa_Challenge()
    {
        var fixture = MfaFixture.CreateEnabled();
        var secret = fixture.Protector.Unprotect(fixture.Repository.Configurations.Single().SecretEncrypted);
        var code = fixture.TotpService.GenerateCode(secret, fixture.Clock.UtcNow);

        var result = await fixture.Service.VerifyAsync(new VerifyMfaCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, code, "127.0.0.1", "test"));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, fixture.Repository.Configurations.Single().FailedAttempts);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.MfaConfigured && audit.Success);
    }

    [Fact]
    public async Task VerifyAsync_Rejects_Disabled_Missing_Or_Invalid_Mfa()
    {
        var fixture = MfaFixture.Create();
        var missing = await fixture.Service.VerifyAsync(new VerifyMfaCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, "123456", null, null));
        await fixture.Service.BeginSetupAsync(new BeginMfaSetupCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, fixture.AdminUserId));
        var disabled = await fixture.Service.VerifyAsync(new VerifyMfaCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, "123456", null, null));

        Assert.True(missing.IsFailure);
        Assert.True(disabled.IsFailure);
    }

    [Fact]
    public async Task DisableAsync_Disables_User_And_Configuration()
    {
        var fixture = MfaFixture.CreateEnabled();

        var result = await fixture.Service.DisableAsync(new DisableMfaCommand(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, fixture.AdminUserId));

        Assert.True(result.IsSuccess);
        Assert.False(fixture.User.MfaEnabled);
        Assert.False(fixture.Repository.Configurations.Single().IsEnabled);
    }

    [Fact]
    public async Task DisableAsync_Rejects_CrossTenant_Request()
    {
        var fixture = MfaFixture.CreateEnabled();

        var result = await fixture.Service.DisableAsync(new DisableMfaCommand(Guid.NewGuid(), fixture.User.Id, MfaMethod.Totp, fixture.AdminUserId));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void TotpService_Generates_And_Verifies_Codes_With_Drift()
    {
        var service = new TotpService();
        var timestamp = new DateTimeOffset(2026, 6, 20, 17, 0, 0, TimeSpan.Zero);
        var secret = service.GenerateSecret();
        var code = service.GenerateCode(secret, timestamp);

        Assert.True(service.VerifyCode(secret, code, timestamp));
        Assert.True(service.VerifyCode(secret, code, timestamp.AddSeconds(30)));
        Assert.False(service.VerifyCode(secret, "000000", timestamp));
        Assert.Throws<ArgumentException>(() => service.GenerateCode("invalid!", timestamp));
    }

    [Fact]
    public void SecretProtector_RoundTrips_Secret()
    {
        var protector = new Base64MfaSecretProtector();

        var protectedSecret = protector.Protect("SECRET");

        Assert.NotEqual("SECRET", protectedSecret);
        Assert.Equal("SECRET", protector.Unprotect(protectedSecret));
    }

    [Fact]
    public async Task EfMfaRepository_Persists_And_Loads_Configuration()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfMfaRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var user = new User(tenantId, "qa@example.com", "Quality Manager");
        var configuration = new MfaConfiguration(tenantId, user.Id, MfaMethod.Totp, "secret", DateTimeOffset.UtcNow);
        var audit = AuditLog.Create(tenantId, user.Id, nameof(MfaConfiguration), configuration.Id, AuditAction.MfaConfigured, DateTimeOffset.UtcNow);

        await dbContext.Users.AddAsync(user);
        await repository.AddConfigurationAsync(configuration);
        await repository.AddAuditLogAsync(audit);
        await dbContext.SaveChangesAsync();

        var loadedUser = await repository.GetUserAsync(tenantId, user.Id);
        var loadedConfiguration = await repository.GetConfigurationAsync(tenantId, user.Id, MfaMethod.Totp);

        Assert.NotNull(loadedUser);
        Assert.NotNull(loadedConfiguration);
        Assert.Single(dbContext.AuditLogs);
    }

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class MfaFixture
    {
        private MfaFixture()
        {
            TenantId = Guid.NewGuid();
            AdminUserId = Guid.NewGuid();
            Clock = new FixedClock();
            Repository = new InMemoryMfaRepository();
            Protector = new Base64MfaSecretProtector();
            TotpService = new TotpService();
            User = new User(TenantId, "qa@example.com", "Quality Manager");
            Repository.Users.Add(User);
            Service = new MfaService(Repository, new FakeApplicationDbContext(), Protector, TotpService, Clock);
        }

        public Guid TenantId { get; }
        public Guid AdminUserId { get; }
        public FixedClock Clock { get; }
        public InMemoryMfaRepository Repository { get; }
        public Base64MfaSecretProtector Protector { get; }
        public TotpService TotpService { get; }
        public User User { get; }
        public MfaService Service { get; }

        public static MfaFixture Create()
        {
            return new MfaFixture();
        }

        public static MfaFixture CreateEnabled()
        {
            var fixture = Create();
            var secret = fixture.TotpService.GenerateSecret();
            var configuration = new MfaConfiguration(fixture.TenantId, fixture.User.Id, MfaMethod.Totp, fixture.Protector.Protect(secret), fixture.Clock.UtcNow);
            configuration.RegisterSuccessfulVerification(fixture.Clock.UtcNow);
            fixture.User.EnableMfa(configuration.SecretEncrypted);
            fixture.Repository.Configurations.Add(configuration);
            return fixture;
        }
    }

    private sealed class InMemoryMfaRepository : IMfaRepository
    {
        public List<User> Users { get; } = [];
        public List<MfaConfiguration> Configurations { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];

        public Task<User?> GetUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Users.SingleOrDefault(user => user.TenantId == tenantId && user.Id == userId));
        }

        public Task<MfaConfiguration?> GetConfigurationAsync(Guid tenantId, Guid userId, MfaMethod method, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Configurations.SingleOrDefault(configuration => configuration.TenantId == tenantId && configuration.UserId == userId && configuration.Method == method));
        }

        public Task AddConfigurationAsync(MfaConfiguration configuration, CancellationToken cancellationToken = default)
        {
            Configurations.Add(configuration);
            return Task.CompletedTask;
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
