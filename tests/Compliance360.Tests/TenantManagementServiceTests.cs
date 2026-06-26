using Compliance360.Application;
using Compliance360.Application.TenantManagement;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Identity;
using Compliance360.Domain.TenantManagement;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.TenantManagement;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Tests;

public sealed class TenantManagementServiceTests
{
    [Fact]
    public async Task CreateTenantAsync_Creates_Tenant_With_AuditLog()
    {
        var repository = new InMemoryTenantManagementRepository();
        var dbContext = new FakeApplicationDbContext();
        var service = new TenantManagementService(repository, dbContext, new FixedClock());

        var result = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "ACME", Guid.NewGuid()));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Equal("Acme Quality", result.Value.Name);
        Assert.Equal("acme", result.Value!.Slug);
        Assert.Equal(TenantStatus.Draft, result.Value.Status);
        Assert.Equal("es-PA", result.Value.Culture);
        Assert.Equal("America/Panama", result.Value.TimeZone);
        Assert.True(result.Value.RequireMfa);
        Assert.Equal("Acme Quality", result.Value.BrandingDisplayName);
        Assert.Equal(SubscriptionPlan.Starter, result.Value.Plan);
        Assert.Equal(SubscriptionStatus.Trial, result.Value.SubscriptionStatus);
        Assert.Single(repository.Tenants);
        Assert.Single(repository.AuditLogs);
        Assert.Equal(1, dbContext.SaveChangesCount);
    }

    [Fact]
    public async Task CreateTenantAsync_Rejects_Duplicate_Slug()
    {
        var repository = new InMemoryTenantManagementRepository();
        var dbContext = new FakeApplicationDbContext();
        var service = new TenantManagementService(repository, dbContext, new FixedClock());

        await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", null));
        var duplicate = await service.CreateTenantAsync(new CreateTenantCommand("Acme Duplicate", "ACME", null));

        Assert.True(duplicate.IsFailure);
        Assert.Equal("Tenant slug already exists.", duplicate.Error);
        Assert.Single(repository.Tenants);
    }

    [Fact]
    public async Task AddCompanyAsync_Adds_Company_To_Tenant()
    {
        var repository = new InMemoryTenantManagementRepository();
        var dbContext = new FakeApplicationDbContext();
        var service = new TenantManagementService(repository, dbContext, new FixedClock());
        var created = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", null));

        var result = await service.AddCompanyAsync(new AddCompanyCommand(
            created.Value!.Id,
            "Acme Panama S.A.",
            "RUC-123",
            "pa",
            Guid.NewGuid()));

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Equal("PA", result.Value!.CountryCode);
        Assert.Equal("Acme Panama S.A.", result.Value.LegalName);
        Assert.Equal("RUC-123", result.Value.TaxIdentifier);
        Assert.True(result.Value.IsActive);
        Assert.Equal(created.Value.Id, result.Value.TenantId);
        Assert.Equal(2, repository.AuditLogs.Count);
    }

    [Fact]
    public async Task ConfigureSettingsAsync_Updates_Mfa_And_Retention()
    {
        var repository = new InMemoryTenantManagementRepository();
        var dbContext = new FakeApplicationDbContext();
        var service = new TenantManagementService(repository, dbContext, new FixedClock());
        var created = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", null));

        var result = await service.ConfigureSettingsAsync(new ConfigureTenantSettingsCommand(
            created.Value!.Id,
            "es-PA",
            "America/Panama",
            RequireMfa: false,
            DocumentRetentionDays: 365,
            RequestedByUserId: null));

        var tenant = repository.Tenants.Single();
        Assert.True(result.IsSuccess);
        Assert.False(tenant.Settings.RequireMfa);
        Assert.Equal(365, tenant.Settings.DocumentRetentionDays);
    }

    [Fact]
    public async Task ConfigureBrandingAsync_Updates_Tenant_Branding()
    {
        var repository = new InMemoryTenantManagementRepository();
        var service = new TenantManagementService(repository, new FakeApplicationDbContext(), new FixedClock());
        var created = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", null));

        var result = await service.ConfigureBrandingAsync(new ConfigureTenantBrandingCommand(
            created.Value!.Id,
            "Acme Compliance",
            "https://cdn.example.com/logo.svg",
            "#111111",
            "#222222",
            Guid.NewGuid()));

        var tenant = repository.Tenants.Single();
        Assert.True(result.IsSuccess);
        Assert.Equal("Acme Compliance", tenant.Branding.DisplayName);
        Assert.Equal("https://cdn.example.com/logo.svg", tenant.Branding.LogoUri);
        Assert.Equal("#111111", tenant.Branding.PrimaryColor);
        Assert.Equal("#222222", tenant.Branding.SecondaryColor);
    }

    [Fact]
    public async Task ChangeSubscriptionAsync_Updates_Plan_And_Limits()
    {
        var repository = new InMemoryTenantManagementRepository();
        var service = new TenantManagementService(repository, new FakeApplicationDbContext(), new FixedClock());
        var created = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", null));

        var result = await service.ChangeSubscriptionAsync(new ChangeSubscriptionCommand(
            created.Value!.Id,
            SubscriptionPlan.Enterprise,
            MaxUsers: 500,
            MaxStorageGb: 2_000,
            Status: SubscriptionStatus.Active,
            ExpiresOn: null,
            ChangeReason: "Enterprise test subscription update.",
            RequestedByUserId: null));

        var tenant = repository.Tenants.Single();
        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionPlan.Enterprise, tenant.Subscription.Plan);
        Assert.Equal(500, tenant.Subscription.MaxUsers);
        Assert.Equal(2_000, tenant.Subscription.MaxStorageGb);
    }

    [Fact]
    public async Task UpdateGeneralInformationAsync_Updates_Enterprise_Profile_And_Audits()
    {
        var repository = new InMemoryTenantManagementRepository();
        var service = new TenantManagementService(repository, new FakeApplicationDbContext(), new FixedClock());
        var created = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", null));

        var result = await service.UpdateGeneralInformationAsync(new UpdateTenantGeneralInformationCommand(
            created.Value!.Id,
            "Acme Group",
            "Acme Panama S.A.",
            "Acme Compliance",
            "RUC-ACME-001",
            "Food Safety",
            "Tenant enterprise profile",
            "Calle 50",
            "Panama",
            "Panama",
            "PA",
            "0801",
            "+507 0000-0000",
            "admin@acme.test",
            "https://acme.test",
            "USD",
            "Correccion legal para onboarding.",
            Guid.NewGuid()));

        var tenant = repository.Tenants.Single();
        Assert.True(result.IsSuccess);
        Assert.Equal("Acme Panama S.A.", tenant.LegalName);
        Assert.Equal("RUC-ACME-001", tenant.TaxIdentifier);
        Assert.Equal("Food Safety", tenant.Industry);
        Assert.Equal("admin@acme.test", tenant.Email);
        Assert.Equal(AuditAction.TenantChanged, repository.AuditLogs.Last().Action);
    }

    [Fact]
    public async Task UpdateGeneralInformationAsync_Rejects_Duplicate_TaxIdentifier()
    {
        var repository = new InMemoryTenantManagementRepository();
        var service = new TenantManagementService(repository, new FakeApplicationDbContext(), new FixedClock());
        await service.CreateTenantAsync(new CreateTenantCommand("Tenant One", "one", "Tenant One", "Tenant One", "RUC-001", "PA", "USD", null));
        var second = await service.CreateTenantAsync(new CreateTenantCommand("Tenant Two", "two", "Tenant Two", "Tenant Two", "RUC-002", "PA", "USD", null));

        var result = await service.UpdateGeneralInformationAsync(new UpdateTenantGeneralInformationCommand(
            second.Value!.Id,
            "Tenant Two",
            "Tenant Two",
            "Tenant Two",
            "RUC-001",
            "Compliance",
            null,
            null,
            null,
            null,
            "PA",
            null,
            null,
            null,
            null,
            "USD",
            null,
            null));

        Assert.True(result.IsFailure);
        Assert.Equal("Tenant tax identifier already exists.", result.Error);
    }

    [Fact]
    public async Task ConfigureSecurityAsync_Updates_Sensitive_Security_Settings()
    {
        var repository = new InMemoryTenantManagementRepository();
        var service = new TenantManagementService(repository, new FakeApplicationDbContext(), new FixedClock());
        var created = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", null));

        var result = await service.ConfigureSecurityAsync(new ConfigureTenantSecurityCommand(
            created.Value!.Id,
            RequireMfa: true,
            SessionTimeoutMinutes: 45,
            PasswordExpirationDays: 120,
            LockoutMaxFailedAttempts: 4,
            LockoutMinutes: 30,
            IpWhitelist: "10.0.0.0/24",
            TrustedDevicesEnabled: true,
            SecurityScore: 92,
            ChangeReason: "Security hardening",
            RequestedByUserId: null));

        var settings = repository.Tenants.Single().Settings;
        Assert.True(result.IsSuccess);
        Assert.True(settings.TrustedDevicesEnabled);
        Assert.Equal(45, settings.SessionTimeoutMinutes);
        Assert.Equal("10.0.0.0/24", settings.IpWhitelist);
        Assert.Equal(92, settings.SecurityScore);
    }

    [Fact]
    public async Task Dashboard_And_AuditTimeline_Return_Tenant_Context()
    {
        var repository = new InMemoryTenantManagementRepository();
        var service = new TenantManagementService(repository, new FakeApplicationDbContext(), new FixedClock());
        var created = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", null));

        var dashboard = await service.GetAdministrationDashboardAsync(created.Value!.Id);
        var timeline = await service.GetAuditTimelineAsync(created.Value.Id, page: 1, pageSize: 10);

        Assert.True(dashboard.IsSuccess);
        Assert.Equal(created.Value.Id, dashboard.Value!.Tenant.Id);
        Assert.True(dashboard.Value.Alerts.Count >= 1);
        Assert.True(timeline.IsSuccess);
        Assert.NotEmpty(timeline.Value!);
    }

    [Fact]
    public async Task ActivateTenantAsync_And_SuspendTenantAsync_Change_Status()
    {
        var repository = new InMemoryTenantManagementRepository();
        var service = new TenantManagementService(repository, new FakeApplicationDbContext(), new FixedClock());
        var created = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", null));

        var activated = await service.ActivateTenantAsync(created.Value!.Id, Guid.NewGuid());
        var suspended = await service.SuspendTenantAsync(created.Value.Id, Guid.NewGuid());

        Assert.True(activated.IsSuccess);
        Assert.True(suspended.IsSuccess);
        Assert.Equal(TenantStatus.Suspended, repository.Tenants.Single().Status);
    }

    [Fact]
    public async Task ArchiveTenantAsync_And_RestoreTenantAsync_Complete_Lifecycle()
    {
        var repository = new InMemoryTenantManagementRepository();
        var service = new TenantManagementService(repository, new FakeApplicationDbContext(), new FixedClock());
        var created = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", null));

        var trial = await service.StartTrialAsync(created.Value!.Id, null);
        var archived = await service.ArchiveTenantAsync(created.Value.Id, null);
        var restored = await service.RestoreTenantAsync(created.Value.Id, null);

        Assert.True(trial.IsSuccess);
        Assert.True(archived.IsSuccess);
        Assert.True(restored.IsSuccess);
        Assert.Equal(TenantStatus.Suspended, repository.Tenants.Single().Status);
    }

    [Fact]
    public async Task Omega_Capabilities_Are_TenantScoped_Audited_And_Functional()
    {
        var repository = new InMemoryTenantManagementRepository();
        var service = new TenantManagementService(repository, new FakeApplicationDbContext(), new FixedClock());
        var actorId = Guid.NewGuid();
        var created = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", actorId));
        var tenantId = created.Value!.Id;
        var role = new Role(tenantId, "Tenant Admin");
        repository.Roles.Add(role);

        var domain = await service.UpsertDomainAsync(new UpsertTenantDomainCommand(tenantId, null, "admin.acme.test", TenantDomainKind.PrimaryCustom, true, null, "Enterprise domain onboarding", actorId));
        var sso = await service.UpsertSsoAsync(new UpsertTenantSsoCommand(tenantId, null, TenantSsoProviderType.Oidc, "Corporate OIDC", "https://login.acme.test", "https://login.acme.test/.well-known/openid-configuration", "client-id", "vault://tenant/acme/sso", """{"email":"email"}""", """{"Admin":"Tenant Admin"}""", true, true, "ABC123", true, "SSO onboarding", actorId));
        var ssoTest = await service.TestSsoAsync(new TenantEntityActionCommand(tenantId, sso.Value!.Id, "Connection validation", actorId));
        var apiKey = await service.CreateApiCredentialAsync(new CreateTenantApiCredentialCommand(tenantId, "erp-sync", "plain-secret-value", "tenant.read tenant.audit", null, "ERP integration", actorId));
        var webhook = await service.UpsertWebhookAsync(new UpsertTenantWebhookCommand(tenantId, null, "Audit outbound", "https://hooks.acme.test/compliance", "audit.created,tenant.updated", "webhook-secret", 3, true, "Audit integration", actorId));
        var webhookTest = await service.TestWebhookAsync(new TenantEntityActionCommand(tenantId, webhook.Value!.Id, "Synthetic delivery", actorId));
        var license = await service.UpsertLicenseAsync(new UpsertTenantLicenseCommand(tenantId, "LIC-ACME-001", TenantLicenseStatus.Active, """{"advancedAudit":true}""", """{"documents":true,"risk":true}""", """{"maxApiKeys":10}""", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), new DateOnly(2026, 11, 30), 100, 1, 500, 1024, "Annual contract", actorId));
        var backup = await service.RecordBackupAsync(new RecordTenantBackupCommand(tenantId, "Database", "Succeeded", new FixedClock().UtcNow.AddMinutes(-5), new FixedClock().UtcNow, 2048, "Backup completed", 60, 240, actorId));
        var user = await service.CreateUserAsync(new CreateTenantUserCommand(tenantId, "admin@acme.test", "Acme Admin", "Temporary123!", true, role.Id, "Admin invite", actorId));
        var center = await service.GetAdministrationCenterStateAsync(tenantId);

        Assert.True(domain.IsSuccess);
        Assert.True(ssoTest.IsSuccess);
        Assert.True(apiKey.IsSuccess);
        Assert.NotEqual("plain-secret-value", repository.ApiCredentials.Single().KeyHash);
        Assert.True(webhookTest.IsSuccess);
        Assert.True(license.IsSuccess);
        Assert.True(backup.IsSuccess);
        Assert.True(user.IsSuccess);
        Assert.True(center.IsSuccess);
        Assert.Single(center.Value!.Domains);
        Assert.Single(center.Value.SsoConfigurations);
        Assert.Single(center.Value.ApiCredentials);
        Assert.Single(center.Value.Webhooks);
        Assert.NotNull(center.Value.License);
        Assert.Contains(center.Value.Health.Signals, signal => signal.Component == "Backups");
        Assert.Contains(center.Value.Users.Users, item => item.Email == "admin@acme.test");
        Assert.True(repository.AuditLogs.Count >= 8);
    }

    [Fact]
    public async Task Service_Returns_Failure_When_Tenant_Does_Not_Exist()
    {
        var service = new TenantManagementService(
            new InMemoryTenantManagementRepository(),
            new FakeApplicationDbContext(),
            new FixedClock());

        var addCompany = await service.AddCompanyAsync(new AddCompanyCommand(Guid.NewGuid(), "Missing", "RUC", "PA", null));
        var activate = await service.ActivateTenantAsync(Guid.NewGuid(), null);
        var settings = await service.ConfigureSettingsAsync(new ConfigureTenantSettingsCommand(Guid.NewGuid(), "es-PA", "America/Panama", true, 365, null));
        var branding = await service.ConfigureBrandingAsync(new ConfigureTenantBrandingCommand(Guid.NewGuid(), "Missing", null, "#111", "#222", null));
        var subscription = await service.ChangeSubscriptionAsync(new ChangeSubscriptionCommand(Guid.NewGuid(), SubscriptionPlan.Starter, 10, 10, null));

        Assert.True(addCompany.IsFailure);
        Assert.True(activate.IsFailure);
        Assert.True(settings.IsFailure);
        Assert.True(branding.IsFailure);
        Assert.True(subscription.IsFailure);
    }

    [Fact]
    public void Tenant_Domain_Rules_Prevent_Invalid_State_And_CrossTenant_Configuration()
    {
        var tenant = new Tenant("Acme Quality", "acme");
        var otherTenantSettings = TenantSettings.CreateDefault(Guid.NewGuid());
        var otherTenantBranding = TenantBranding.CreateDefault(Guid.NewGuid(), "Other");

        Assert.Throws<DomainException>(() => tenant.Suspend());
        Assert.Throws<DomainException>(() => tenant.UpdateSettings(otherTenantSettings));
        Assert.Throws<DomainException>(() => tenant.UpdateBranding(otherTenantBranding));
    }

    [Fact]
    public void Company_Can_Be_Deactivated()
    {
        var company = new Company(Guid.NewGuid(), "Acme Panama S.A.", "RUC-123", "pa");

        company.Deactivate();

        Assert.False(company.IsActive);
        Assert.Equal("PA", company.CountryCode);
    }

    [Fact]
    public async Task Service_Returns_Failure_When_Domain_Validation_Fails()
    {
        var repository = new InMemoryTenantManagementRepository();
        var service = new TenantManagementService(repository, new FakeApplicationDbContext(), new FixedClock());
        var created = await service.CreateTenantAsync(new CreateTenantCommand("Acme Quality", "acme", null));

        var invalidTenant = await service.CreateTenantAsync(new CreateTenantCommand("", "invalid", null));
        var invalidCompany = await service.AddCompanyAsync(new AddCompanyCommand(created.Value!.Id, "", "RUC", "PA", null));
        var invalidSettings = await service.ConfigureSettingsAsync(new ConfigureTenantSettingsCommand(created.Value.Id, "", "America/Panama", true, 365, null));
        var invalidBranding = await service.ConfigureBrandingAsync(new ConfigureTenantBrandingCommand(created.Value.Id, "", null, "#111", "#222", null));
        var invalidSubscription = await service.ChangeSubscriptionAsync(new ChangeSubscriptionCommand(created.Value.Id, SubscriptionPlan.Starter, 0, 10, null));

        Assert.True(invalidTenant.IsFailure);
        Assert.True(invalidCompany.IsFailure);
        Assert.True(invalidSettings.IsFailure);
        Assert.True(invalidBranding.IsFailure);
        Assert.True(invalidSubscription.IsFailure);
    }

    [Fact]
    public void Tenant_Domain_Rules_Allow_Valid_Configuration()
    {
        var tenant = new Tenant("Acme Quality", "acme");
        var settings = TenantSettings.CreateDefault(tenant.Id);
        var branding = TenantBranding.CreateDefault(tenant.Id, "Acme");

        settings.Configure("es-PA", "America/Panama", true, 730);
        branding.Configure("Acme", null, "#000000", "#FFFFFF");
        tenant.UpdateSettings(settings);
        tenant.UpdateBranding(branding);
        tenant.Activate();
        tenant.Subscription.MarkActive();

        Assert.Equal(TenantStatus.Active, tenant.Status);
        Assert.Equal(SubscriptionStatus.Active, tenant.Subscription.Status);
        Assert.Null(tenant.Branding.LogoUri);
    }

    [Fact]
    public async Task EfRepository_Persists_And_Loads_Tenant_With_AuditLog()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var dbContext = new Compliance360DbContext(options, new FixedClock());
        var repository = new EfTenantManagementRepository(dbContext);
        var tenant = new Tenant("Acme Quality", "acme");
        var auditLog = AuditLog.Create(tenant.Id, null, nameof(Tenant), tenant.Id, AuditAction.Created, new FixedClock().UtcNow);

        await repository.AddAsync(tenant);
        await repository.AddAuditLogAsync(auditLog);
        await dbContext.SaveChangesAsync();

        var exists = await repository.SlugExistsAsync("ACME");
        var loaded = await repository.GetByIdAsync(tenant.Id);

        Assert.True(exists);
        Assert.NotNull(loaded);
        Assert.Equal(tenant.Id, loaded!.Settings.TenantId);
        Assert.Single(dbContext.AuditLogs);
    }

    private sealed class InMemoryTenantManagementRepository : ITenantManagementRepository
    {
        public List<Tenant> Tenants { get; } = [];

        public List<AuditLog> AuditLogs { get; } = [];

        public List<TenantDomain> Domains { get; } = [];

        public List<TenantSsoConfiguration> SsoConfigurations { get; } = [];

        public List<TenantApiCredential> ApiCredentials { get; } = [];

        public List<TenantWebhookEndpoint> Webhooks { get; } = [];

        public List<TenantWebhookDeliveryLog> WebhookLogs { get; } = [];

        public List<TenantLicense> Licenses { get; } = [];

        public List<TenantHealthSignal> HealthSignals { get; } = [];

        public List<TenantBackupRecord> Backups { get; } = [];

        public List<User> Users { get; } = [];

        public List<Role> Roles { get; } = [];

        public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Tenants.Any(tenant => tenant.Slug == slug.Trim().ToLowerInvariant()));
        }

        public Task<bool> SlugExistsAsync(Guid excludeTenantId, string slug, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Tenants.Any(tenant => tenant.Id != excludeTenantId && tenant.Slug == slug.Trim().ToLowerInvariant()));
        }

        public Task<bool> TaxIdentifierExistsAsync(Guid excludeTenantId, string taxIdentifier, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Tenants.Any(tenant => tenant.Id != excludeTenantId && tenant.TaxIdentifier == taxIdentifier.Trim().ToUpperInvariant()));
        }

        public Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Tenants.SingleOrDefault(tenant => tenant.Id == tenantId));
        }

        public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            Tenants.Add(tenant);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Tenant>> SearchAsync(string? searchText, TenantStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            IEnumerable<Tenant> query = Tenants;
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(tenant => tenant.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) || tenant.Slug.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                query = query.Where(tenant => tenant.Status == status.Value);
            }

            return Task.FromResult<IReadOnlyCollection<Tenant>>(query.Skip((page - 1) * pageSize).Take(pageSize).ToArray());
        }

        public Task<int> CountAsync(string? searchText, TenantStatus? status, CancellationToken cancellationToken = default)
        {
            IEnumerable<Tenant> query = Tenants;
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(tenant => tenant.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) || tenant.Slug.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                query = query.Where(tenant => tenant.Status == status.Value);
            }

            return Task.FromResult(query.Count());
        }

        public Task<TenantAdministrationMetrics> GetAdministrationMetricsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TenantAdministrationMetrics(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, null, Health: true));
        }

        public Task<IReadOnlyCollection<TenantAuditTimelineItem>> GetAuditTimelineAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var timeline = AuditLogs
                .Where(audit => audit.TenantId == tenantId)
                .OrderByDescending(audit => audit.OccurredAtUtc)
                .Select(audit => new TenantAuditTimelineItem(audit.Id, audit.OccurredAtUtc, audit.Action.ToString(), audit.EntityName, audit.EntityId, audit.UserId, audit.UserName, audit.IpAddress, audit.CorrelationId, audit.BeforeValuesJson, audit.AfterValuesJson, audit.MetadataJson))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArray();
            return Task.FromResult<IReadOnlyCollection<TenantAuditTimelineItem>>(timeline);
        }

        public Task AddCompanyAsync(Company company, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<TenantDomain>> ListDomainsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<TenantDomain>>(Domains.Where(domain => domain.TenantId == tenantId).ToArray());
        }

        public Task<TenantDomain?> GetDomainAsync(Guid tenantId, Guid domainId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Domains.SingleOrDefault(domain => domain.TenantId == tenantId && domain.Id == domainId));
        }

        public Task<bool> DomainExistsAsync(Guid tenantId, string hostName, Guid? excludeDomainId, CancellationToken cancellationToken = default)
        {
            var normalized = TenantValueObjects.Domain(hostName);
            return Task.FromResult(Domains.Any(domain => domain.HostName == normalized && domain.Id != excludeDomainId));
        }

        public Task AddDomainAsync(TenantDomain domain, CancellationToken cancellationToken = default)
        {
            Domains.Add(domain);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<TenantSsoConfiguration>> ListSsoConfigurationsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<TenantSsoConfiguration>>(SsoConfigurations.Where(sso => sso.TenantId == tenantId).ToArray());
        }

        public Task<TenantSsoConfiguration?> GetSsoConfigurationAsync(Guid tenantId, Guid ssoId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SsoConfigurations.SingleOrDefault(sso => sso.TenantId == tenantId && sso.Id == ssoId));
        }

        public Task AddSsoConfigurationAsync(TenantSsoConfiguration configuration, CancellationToken cancellationToken = default)
        {
            SsoConfigurations.Add(configuration);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<TenantApiCredential>> ListApiCredentialsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<TenantApiCredential>>(ApiCredentials.Where(apiKey => apiKey.TenantId == tenantId).ToArray());
        }

        public Task<TenantApiCredential?> GetApiCredentialAsync(Guid tenantId, Guid apiCredentialId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ApiCredentials.SingleOrDefault(apiKey => apiKey.TenantId == tenantId && apiKey.Id == apiCredentialId));
        }

        public Task AddApiCredentialAsync(TenantApiCredential credential, CancellationToken cancellationToken = default)
        {
            ApiCredentials.Add(credential);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<TenantWebhookEndpoint>> ListWebhooksAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<TenantWebhookEndpoint>>(Webhooks.Where(webhook => webhook.TenantId == tenantId).ToArray());
        }

        public Task<TenantWebhookEndpoint?> GetWebhookAsync(Guid tenantId, Guid webhookId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Webhooks.SingleOrDefault(webhook => webhook.TenantId == tenantId && webhook.Id == webhookId));
        }

        public Task AddWebhookAsync(TenantWebhookEndpoint webhook, CancellationToken cancellationToken = default)
        {
            Webhooks.Add(webhook);
            return Task.CompletedTask;
        }

        public Task AddWebhookDeliveryLogAsync(TenantWebhookDeliveryLog deliveryLog, CancellationToken cancellationToken = default)
        {
            WebhookLogs.Add(deliveryLog);
            return Task.CompletedTask;
        }

        public Task<TenantLicense?> GetLicenseAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Licenses.SingleOrDefault(license => license.TenantId == tenantId));
        }

        public Task AddLicenseAsync(TenantLicense license, CancellationToken cancellationToken = default)
        {
            Licenses.Add(license);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<TenantHealthSignal>> ListHealthSignalsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<TenantHealthSignal>>(HealthSignals.Where(signal => signal.TenantId == tenantId).ToArray());
        }

        public Task<TenantHealthSignal?> GetHealthSignalAsync(Guid tenantId, string component, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthSignals.SingleOrDefault(signal => signal.TenantId == tenantId && signal.Component == component));
        }

        public Task AddHealthSignalAsync(TenantHealthSignal signal, CancellationToken cancellationToken = default)
        {
            HealthSignals.Add(signal);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<TenantBackupRecord>> ListBackupRecordsAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<TenantBackupRecord>>(Backups.Where(backup => backup.TenantId == tenantId).Skip((page - 1) * pageSize).Take(pageSize).ToArray());
        }

        public Task AddBackupRecordAsync(TenantBackupRecord backup, CancellationToken cancellationToken = default)
        {
            Backups.Add(backup);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<User>> ListUsersAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<User>>(Users.Where(user => user.TenantId == tenantId).ToArray());
        }

        public Task<User?> GetUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Users.SingleOrDefault(user => user.TenantId == tenantId && user.Id == userId));
        }

        public Task<Role?> GetRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Roles.SingleOrDefault(role => role.TenantId == tenantId && role.Id == roleId));
        }

        public Task<IReadOnlyCollection<Role>> ListRolesAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Role>>(Roles.Where(role => role.TenantId == tenantId).ToArray());
        }

        public Task AddUserAsync(User user, CancellationToken cancellationToken = default)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<UserSessionSummary>> ListUserSessionsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<UserSessionSummary>>([]);
        }

        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            AuditLogs.Add(auditLog);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext
    {
        public int SaveChangesCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 6, 20, 15, 0, 0, TimeSpan.Zero);
    }
}
