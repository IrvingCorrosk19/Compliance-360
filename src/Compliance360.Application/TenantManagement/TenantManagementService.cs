using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Identity;
using Compliance360.Domain.TenantManagement;
using Compliance360.Shared;

namespace Compliance360.Application.TenantManagement;

public sealed class TenantManagementService : ITenantManagementService
{
    private readonly ITenantManagementRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;
    private readonly IPasswordHasher _passwordHasher;
    private readonly Compliance360.Application.Rbac.IRbacProvisioningService? _rbacProvisioning;

    public TenantManagementService(
        ITenantManagementRepository repository,
        IApplicationDbContext dbContext,
        IClock clock,
        IPasswordHasher? passwordHasher = null,
        Compliance360.Application.Rbac.IRbacProvisioningService? rbacProvisioning = null)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
        _passwordHasher = passwordHasher ?? new Sha256PasswordHasher();
        _rbacProvisioning = rbacProvisioning;
    }

    public async Task<Result<TenantSummary>> CreateTenantAsync(CreateTenantCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedSlug = NormalizeSlug(command.Slug);
            if (await _repository.SlugExistsAsync(normalizedSlug, cancellationToken))
            {
                return Result<TenantSummary>.Failure("Tenant slug already exists.");
            }

            if (!string.IsNullOrWhiteSpace(command.TaxIdentifier)
                && await _repository.TaxIdentifierExistsAsync(command.TaxIdentifier.Trim().ToUpperInvariant(), cancellationToken))
            {
                return Result<TenantSummary>.Failure("Tenant tax identifier already exists.");
            }

            var tenant = new Tenant(
                command.Name,
                normalizedSlug,
                command.LegalName ?? command.Name,
                command.CommercialName ?? command.Name,
                command.TaxIdentifier ?? $"TAX-{Guid.NewGuid():N}"[..20],
                command.CountryCode ?? "PA",
                command.Currency ?? "USD",
                command.RequestedByUserId);
            await _repository.AddAsync(tenant, cancellationToken);
            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(Tenant), tenant.Id, AuditAction.TenantChanged, "Tenant created.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Provision the full Enterprise RBAC (roles + permission grants) so a
            // new tenant is immediately usable without any manual setup.
            if (_rbacProvisioning is not null)
            {
                await _rbacProvisioning.EnsureTenantRolesAsync(tenant.Id, cancellationToken);
            }

            // Provision an initial Tenant Administrator so the tenant can be
            // administered without the platform operator touching business data.
            await TryProvisionInitialTenantAdministratorAsync(tenant.Id, command, cancellationToken);

            return Result<TenantSummary>.Success(ToSummary(tenant));
        }
        catch (DomainException exception)
        {
            return Result<TenantSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<TenantAdministrationDashboard>> GetAdministrationDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await GetTenantOrNullAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<TenantAdministrationDashboard>.Failure("Tenant not found.");
        }

        var metrics = await _repository.GetAdministrationMetricsAsync(tenantId, cancellationToken);
        var timeline = await _repository.GetAuditTimelineAsync(tenantId, page: 1, pageSize: 12, cancellationToken);
        var alerts = BuildAlerts(tenant, metrics);
        return Result<TenantAdministrationDashboard>.Success(new TenantAdministrationDashboard(ToDetail(tenant), metrics, alerts, timeline));
    }

    public async Task<Result<TenantDetail>> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await GetTenantOrNullAsync(tenantId, cancellationToken);
        return tenant is null
            ? Result<TenantDetail>.Failure("Tenant not found.")
            : Result<TenantDetail>.Success(ToDetail(tenant));
    }

    public async Task<Result<TenantSearchResult>> SearchTenantsAsync(TenantSearchQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var tenants = await _repository.SearchAsync(query.SearchText, query.Status, page, pageSize, cancellationToken);
        var total = await _repository.CountAsync(query.SearchText, query.Status, cancellationToken);
        return Result<TenantSearchResult>.Success(new TenantSearchResult(tenants.Select(ToSummary).ToArray(), total, page, pageSize));
    }

    public async Task<Result> UpdateGeneralInformationAsync(UpdateTenantGeneralInformationCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(command.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Result.Failure("Tenant not found.");
            }

            if (await _repository.TaxIdentifierExistsAsync(command.TenantId, command.TaxIdentifier.Trim().ToUpperInvariant(), cancellationToken))
            {
                return Result.Failure("Tenant tax identifier already exists.");
            }

            tenant.UpdateGeneralInformation(
                command.Name,
                command.LegalName,
                command.CommercialName,
                command.TaxIdentifier,
                command.Industry,
                command.Description,
                command.AddressLine1,
                command.City,
                command.Province,
                command.CountryCode,
                command.PostalCode,
                command.Phone,
                command.Email,
                command.Website,
                command.Currency);

            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(Tenant), tenant.Id, AuditAction.TenantChanged, command.ChangeReason ?? "Tenant general information updated.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result<CompanySummary>> AddCompanyAsync(AddCompanyCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(command.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Result<CompanySummary>.Failure("Tenant not found.");
            }

            var company = tenant.AddCompany(command.LegalName, command.TaxIdentifier, command.CountryCode);
            await _repository.AddCompanyAsync(company, cancellationToken);
            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(Company), company.Id, AuditAction.TenantChanged, "Company added to tenant.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<CompanySummary>.Success(ToSummary(company));
        }
        catch (DomainException exception)
        {
            return Result<CompanySummary>.Failure(exception.Message);
        }
    }

    public async Task<Result> ActivateTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default)
    {
        return await ChangeTenantStateAsync(
            tenantId,
            requestedByUserId,
            tenant => tenant.Activate(),
            "Tenant activated.",
            cancellationToken);
    }

    public async Task<Result> SuspendTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default)
    {
        return await ChangeTenantStateAsync(
            tenantId,
            requestedByUserId,
            tenant => tenant.Suspend(),
            "Tenant suspended.",
            cancellationToken);
    }

    public async Task<Result> StartTrialAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default)
    {
        return await ChangeTenantStateAsync(
            tenantId,
            requestedByUserId,
            tenant => tenant.StartTrial(),
            "Tenant moved to trial.",
            cancellationToken);
    }

    public async Task<Result> ArchiveTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default)
    {
        return await ChangeTenantStateAsync(
            tenantId,
            requestedByUserId,
            tenant => tenant.Archive(),
            "Tenant archived.",
            cancellationToken);
    }

    public async Task<Result> RestoreTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default)
    {
        return await ChangeTenantStateAsync(
            tenantId,
            requestedByUserId,
            tenant => tenant.Restore(),
            "Tenant restored.",
            cancellationToken);
    }

    public async Task<Result> ConfigureSettingsAsync(ConfigureTenantSettingsCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(command.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Result.Failure("Tenant not found.");
            }

            tenant.Settings.Configure(command.Culture, command.TimeZone, command.RequireMfa, command.DocumentRetentionDays);
            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(TenantSettings), tenant.Settings.Id, AuditAction.TenantChanged, "Tenant regional settings updated.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result> ConfigureSecurityAsync(ConfigureTenantSecurityCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(command.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Result.Failure("Tenant not found.");
            }

            tenant.Settings.ConfigureSecurity(
                command.RequireMfa,
                command.SessionTimeoutMinutes,
                command.PasswordExpirationDays,
                command.LockoutMaxFailedAttempts,
                command.LockoutMinutes,
                command.IpWhitelist,
                command.TrustedDevicesEnabled,
                command.SecurityScore);

            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(TenantSettings), tenant.Settings.Id, AuditAction.TenantChanged, command.ChangeReason ?? "Tenant security settings updated.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result> ConfigureBrandingAsync(ConfigureTenantBrandingCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(command.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Result.Failure("Tenant not found.");
            }

            tenant.Branding.Configure(command.DisplayName, command.LogoUri, command.FaviconUri, command.PrimaryColor, command.SecondaryColor, command.Theme, command.LoginBackgroundUri, command.CorporateEmail, command.FooterText);
            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(TenantBranding), tenant.Branding.Id, AuditAction.TenantChanged, command.ChangeReason ?? "Tenant branding updated.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result> ChangeSubscriptionAsync(ChangeSubscriptionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(command.TenantId, cancellationToken);
            if (tenant is null)
            {
                return Result.Failure("Tenant not found.");
            }

            tenant.Subscription.ChangePlan(command.Plan, command.MaxUsers, command.MaxStorageGb);
            tenant.Subscription.ConfigureCommercialState(command.Status, command.ExpiresOn);
            await AppendAuditAsync(tenant.Id, command.RequestedByUserId, nameof(Subscription), tenant.Subscription.Id, AuditAction.TenantChanged, command.ChangeReason ?? "Tenant subscription updated.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result<IReadOnlyCollection<TenantAuditTimelineItem>>> GetAuditTimelineAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (await GetTenantOrNullAsync(tenantId, cancellationToken) is null)
        {
            return Result<IReadOnlyCollection<TenantAuditTimelineItem>>.Failure("Tenant not found.");
        }

        var timeline = await _repository.GetAuditTimelineAsync(tenantId, Math.Max(1, page), Math.Clamp(pageSize, 1, 100), cancellationToken);
        return Result<IReadOnlyCollection<TenantAuditTimelineItem>>.Success(timeline);
    }

    public async Task<Result<TenantAdministrationCenterState>> GetAdministrationCenterStateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await GetTenantOrNullAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<TenantAdministrationCenterState>.Failure("Tenant not found.");
        }

        var metrics = await _repository.GetAdministrationMetricsAsync(tenantId, cancellationToken);
        var domains = await _repository.ListDomainsAsync(tenantId, cancellationToken);
        var sso = await _repository.ListSsoConfigurationsAsync(tenantId, cancellationToken);
        var apiKeys = await _repository.ListApiCredentialsAsync(tenantId, cancellationToken);
        var webhooks = await _repository.ListWebhooksAsync(tenantId, cancellationToken);
        var license = await _repository.GetLicenseAsync(tenantId, cancellationToken);
        var health = await BuildHealthCenterAsync(tenantId, cancellationToken);
        var users = await BuildUserAdministrationAsync(tenantId, cancellationToken);
        var timeline = await _repository.GetAuditTimelineAsync(tenantId, 1, 20, cancellationToken);

        return Result<TenantAdministrationCenterState>.Success(new TenantAdministrationCenterState(
            ToDetail(tenant),
            metrics,
            domains.Select(ToSummary).ToArray(),
            sso.Select(ToSummary).ToArray(),
            apiKeys.Select(ToSummary).ToArray(),
            webhooks.Select(ToSummary).ToArray(),
            license is null ? null : ToSummary(license),
            health,
            users,
            timeline));
    }

    public async Task<Result<TenantDomainSummary>> UpsertDomainAsync(UpsertTenantDomainCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await GetTenantOrNullAsync(command.TenantId, cancellationToken) is null)
            {
                return Result<TenantDomainSummary>.Failure("Tenant not found.");
            }

            if (await _repository.DomainExistsAsync(command.TenantId, command.HostName, command.DomainId, cancellationToken))
            {
                return Result<TenantDomainSummary>.Failure("Domain already belongs to another tenant or domain record.");
            }

            TenantDomain domain;
            if (command.DomainId.HasValue)
            {
                domain = await _repository.GetDomainAsync(command.TenantId, command.DomainId.Value, cancellationToken)
                    ?? throw new DomainException("Domain not found.");
                domain.Configure(command.HostName, command.Kind, command.IsDefault, command.RedirectToHostName);
            }
            else
            {
                domain = new TenantDomain(command.TenantId, command.HostName, command.Kind, command.IsDefault, command.RequestedByUserId);
                domain.Configure(command.HostName, command.Kind, command.IsDefault, command.RedirectToHostName);
                await _repository.AddDomainAsync(domain, cancellationToken);
            }

            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantDomain), domain.Id, AuditAction.TenantChanged, $"Domain upserted: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<TenantDomainSummary>.Success(ToSummary(domain));
        }
        catch (DomainException exception)
        {
            return Result<TenantDomainSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result> DisableDomainAsync(TenantEntityActionCommand command, CancellationToken cancellationToken = default)
    {
        var domain = await _repository.GetDomainAsync(command.TenantId, command.EntityId, cancellationToken);
        if (domain is null)
        {
            return Result.Failure("Domain not found.");
        }

        domain.Disable();
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantDomain), domain.Id, AuditAction.TenantChanged, $"Domain disabled: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<TenantSsoConfigurationSummary>> UpsertSsoAsync(UpsertTenantSsoCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await GetTenantOrNullAsync(command.TenantId, cancellationToken) is null)
            {
                return Result<TenantSsoConfigurationSummary>.Failure("Tenant not found.");
            }

            TenantSsoConfiguration sso;
            if (command.ConfigurationId.HasValue)
            {
                sso = await _repository.GetSsoConfigurationAsync(command.TenantId, command.ConfigurationId.Value, cancellationToken)
                    ?? throw new DomainException("SSO configuration not found.");
            }
            else
            {
                sso = new TenantSsoConfiguration(command.TenantId, command.Provider, command.Name, command.Authority, command.MetadataUrl, command.ClientId, command.SecretReference, command.RequestedByUserId);
                await _repository.AddSsoConfigurationAsync(sso, cancellationToken);
            }

            sso.ConfigureMappings(command.ClaimsMappingJson, command.RoleMappingJson, command.JitProvisioningEnabled, command.ScimEnabled);
            if (!string.IsNullOrWhiteSpace(command.CertificateThumbprint))
            {
                sso.RotateCertificate(command.CertificateThumbprint);
            }

            sso.SetEnabled(command.Enabled);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantSsoConfiguration), sso.Id, AuditAction.TenantChanged, $"SSO configuration changed: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<TenantSsoConfigurationSummary>.Success(ToSummary(sso));
        }
        catch (DomainException exception)
        {
            return Result<TenantSsoConfigurationSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result> TestSsoAsync(TenantEntityActionCommand command, CancellationToken cancellationToken = default)
    {
        var sso = await _repository.GetSsoConfigurationAsync(command.TenantId, command.EntityId, cancellationToken);
        if (sso is null)
        {
            return Result.Failure("SSO configuration not found.");
        }

        var status = !string.IsNullOrWhiteSpace(sso.Authority) && !string.IsNullOrWhiteSpace(sso.ClientId)
            ? TenantHealthStatus.Healthy
            : TenantHealthStatus.Unhealthy;
        sso.RecordHealth(status, status == TenantHealthStatus.Healthy ? "Configuration metadata is syntactically complete." : "Missing authority or client id.", _clock.UtcNow);
        await UpsertHealthSignalAsync(command.TenantId, $"SSO:{sso.Provider}", status, sso.HealthMessage, TimeSpan.FromMilliseconds(1), cancellationToken);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantSsoConfiguration), sso.Id, AuditAction.TenantChanged, $"SSO connection tested: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<TenantApiCredentialSummary>> CreateApiCredentialAsync(CreateTenantApiCredentialCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await GetTenantOrNullAsync(command.TenantId, cancellationToken) is null)
            {
                return Result<TenantApiCredentialSummary>.Failure("Tenant not found.");
            }

            var secret = TenantApiCredential.HashSecret(command.PlainTextSecret);
            var prefix = command.PlainTextSecret.Length >= 8 ? command.PlainTextSecret[..8] : command.PlainTextSecret;
            var credential = new TenantApiCredential(command.TenantId, command.Name, prefix, secret, command.Scopes, command.ExpiresAtUtc, command.RequestedByUserId);
            await _repository.AddApiCredentialAsync(credential, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantApiCredential), credential.Id, AuditAction.TenantChanged, $"API credential created: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<TenantApiCredentialSummary>.Success(ToSummary(credential));
        }
        catch (DomainException exception)
        {
            return Result<TenantApiCredentialSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<TenantApiCredentialSummary>> RotateApiCredentialAsync(RotateTenantApiCredentialCommand command, CancellationToken cancellationToken = default)
    {
        var credential = await _repository.GetApiCredentialAsync(command.TenantId, command.ApiCredentialId, cancellationToken);
        if (credential is null)
        {
            return Result<TenantApiCredentialSummary>.Failure("API credential not found.");
        }

        var prefix = command.PlainTextSecret.Length >= 8 ? command.PlainTextSecret[..8] : command.PlainTextSecret;
        credential.Rotate(prefix, TenantApiCredential.HashSecret(command.PlainTextSecret), command.ExpiresAtUtc);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantApiCredential), credential.Id, AuditAction.TenantChanged, $"API credential rotated: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<TenantApiCredentialSummary>.Success(ToSummary(credential));
    }

    public async Task<Result> RevokeApiCredentialAsync(TenantEntityActionCommand command, CancellationToken cancellationToken = default)
    {
        var credential = await _repository.GetApiCredentialAsync(command.TenantId, command.EntityId, cancellationToken);
        if (credential is null)
        {
            return Result.Failure("API credential not found.");
        }

        credential.Revoke();
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantApiCredential), credential.Id, AuditAction.TenantChanged, $"API credential revoked: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<TenantWebhookSummary>> UpsertWebhookAsync(UpsertTenantWebhookCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            TenantWebhookEndpoint webhook;
            var secretHash = TenantApiCredential.HashSecret(command.PlainTextSecret);
            if (command.WebhookId.HasValue)
            {
                webhook = await _repository.GetWebhookAsync(command.TenantId, command.WebhookId.Value, cancellationToken)
                    ?? throw new DomainException("Webhook not found.");
                webhook.Configure(command.Name, command.Url, command.Events, command.MaxRetries);
                webhook.RotateSecret(secretHash);
            }
            else
            {
                webhook = new TenantWebhookEndpoint(command.TenantId, command.Name, command.Url, command.Events, secretHash, command.RequestedByUserId);
                webhook.Configure(command.Name, command.Url, command.Events, command.MaxRetries);
                await _repository.AddWebhookAsync(webhook, cancellationToken);
            }

            webhook.SetEnabled(command.Enabled);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantWebhookEndpoint), webhook.Id, AuditAction.TenantChanged, $"Webhook changed: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<TenantWebhookSummary>.Success(ToSummary(webhook));
        }
        catch (DomainException exception)
        {
            return Result<TenantWebhookSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<TenantWebhookDeliverySummary>> TestWebhookAsync(TenantEntityActionCommand command, CancellationToken cancellationToken = default)
    {
        var webhook = await _repository.GetWebhookAsync(command.TenantId, command.EntityId, cancellationToken);
        if (webhook is null)
        {
            return Result<TenantWebhookDeliverySummary>.Failure("Webhook not found.");
        }

        var status = webhook.Status == TenantIntegrationStatus.Enabled ? TenantWebhookDeliveryStatus.Succeeded : TenantWebhookDeliveryStatus.Failed;
        var log = webhook.RecordDelivery(status, status == TenantWebhookDeliveryStatus.Succeeded ? "Synthetic test delivery recorded." : "Webhook is disabled.", _clock.UtcNow, 1);
        await _repository.AddWebhookDeliveryLogAsync(log, cancellationToken);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantWebhookEndpoint), webhook.Id, AuditAction.TenantChanged, $"Webhook tested: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<TenantWebhookDeliverySummary>.Success(ToSummary(log));
    }

    public async Task<Result> DisableWebhookAsync(TenantEntityActionCommand command, CancellationToken cancellationToken = default)
    {
        var webhook = await _repository.GetWebhookAsync(command.TenantId, command.EntityId, cancellationToken);
        if (webhook is null)
        {
            return Result.Failure("Webhook not found.");
        }

        webhook.SetEnabled(false);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantWebhookEndpoint), webhook.Id, AuditAction.TenantChanged, $"Webhook disabled: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<TenantLicenseSummary>> UpsertLicenseAsync(UpsertTenantLicenseCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var license = await _repository.GetLicenseAsync(command.TenantId, cancellationToken);
            if (license is null)
            {
                license = new TenantLicense(command.TenantId, command.LicenseNumber, command.FeaturesJson, command.ModulesJson, command.PeriodStart, command.PeriodEnd, command.RenewalDate, command.RequestedByUserId);
                await _repository.AddLicenseAsync(license, cancellationToken);
            }

            license.Configure(command.FeaturesJson, command.ModulesJson, command.EntitlementsJson, command.Status, command.PeriodStart, command.PeriodEnd, command.RenewalDate);
            license.UpdateConsumption(command.SeatsPurchased, command.SeatsUsed, command.StorageGbPurchased, command.StorageBytesUsed);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantLicense), license.Id, AuditAction.TenantChanged, $"License changed: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<TenantLicenseSummary>.Success(ToSummary(license));
        }
        catch (DomainException exception)
        {
            return Result<TenantLicenseSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<TenantHealthCenterSummary>> GetHealthCenterAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (await GetTenantOrNullAsync(tenantId, cancellationToken) is null)
        {
            return Result<TenantHealthCenterSummary>.Failure("Tenant not found.");
        }

        return Result<TenantHealthCenterSummary>.Success(await BuildHealthCenterAsync(tenantId, cancellationToken));
    }

    public async Task<Result<TenantBackupSummary>> RecordBackupAsync(RecordTenantBackupCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var backup = new TenantBackupRecord(command.TenantId, command.BackupKind, command.Result, command.StartedAtUtc, command.CompletedAtUtc, command.SizeBytes, command.Message, TimeSpan.FromMinutes(command.RpoMinutes), TimeSpan.FromMinutes(command.RtoMinutes));
            await _repository.AddBackupRecordAsync(backup, cancellationToken);
            await UpsertHealthSignalAsync(command.TenantId, "Backups", string.Equals(command.Result, "Succeeded", StringComparison.OrdinalIgnoreCase) ? TenantHealthStatus.Healthy : TenantHealthStatus.Unhealthy, command.Message, backup.Duration, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(TenantBackupRecord), backup.Id, AuditAction.TenantChanged, "Backup status recorded.", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<TenantBackupSummary>.Success(ToSummary(backup));
        }
        catch (DomainException exception)
        {
            return Result<TenantBackupSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<TenantUserAdministrationSummary>> GetUsersAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (await GetTenantOrNullAsync(tenantId, cancellationToken) is null)
        {
            return Result<TenantUserAdministrationSummary>.Failure("Tenant not found.");
        }

        return Result<TenantUserAdministrationSummary>.Success(await BuildUserAdministrationAsync(tenantId, cancellationToken));
    }

    private async Task TryProvisionInitialTenantAdministratorAsync(Guid tenantId, CreateTenantCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.AdminEmail) || string.IsNullOrWhiteSpace(command.AdminPassword))
        {
            return;
        }

        if (ValidateInitialPassword(command.AdminPassword) is not null)
        {
            return;
        }

        var roles = await _repository.ListRolesAsync(tenantId, cancellationToken);
        var administratorRole = roles.FirstOrDefault(role =>
            string.Equals(role.Name, RoleCatalog.TenantAdministrator, StringComparison.OrdinalIgnoreCase));
        if (administratorRole is null)
        {
            return;
        }

        var admin = new User(tenantId, command.AdminEmail, command.AdminFullName ?? command.AdminEmail);
        admin.SetPasswordHash(_passwordHasher.HashPassword(command.AdminPassword));
        admin.RequirePasswordChange();
        admin.AssignRole(administratorRole.Id);

        await _repository.AddUserAsync(admin, cancellationToken);
        await AppendAuditAsync(tenantId, command.RequestedByUserId, nameof(User), admin.Id, AuditAction.TenantChanged, "Initial Tenant Administrator provisioned.", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<TenantUserSummary>> CreateUserAsync(CreateTenantUserCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var passwordValidation = ValidateInitialPassword(command.InitialPassword);
            if (passwordValidation is not null)
            {
                return Result<TenantUserSummary>.Failure(passwordValidation);
            }

            var user = new User(command.TenantId, command.Email, command.FullName);
            user.SetPasswordHash(_passwordHasher.HashPassword(command.InitialPassword));
            if (command.ForcePasswordChange)
            {
                user.RequirePasswordChange();
            }

            if (command.RoleId.HasValue)
            {
                if (await _repository.GetRoleAsync(command.TenantId, command.RoleId.Value, cancellationToken) is null)
                {
                    return Result<TenantUserSummary>.Failure("Role not found.");
                }

                user.AssignRole(command.RoleId.Value);
            }

            await _repository.AddUserAsync(user, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(User), user.Id, AuditAction.TenantChanged, $"Tenant user created: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<TenantUserSummary>.Success(ToSummary(user));
        }
        catch (DomainException exception)
        {
            return Result<TenantUserSummary>.Failure(exception.Message);
        }
    }

    private static string? ValidateInitialPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return "Initial password is required.";
        }

        if (password.Length < 12)
        {
            return "Initial password must be at least 12 characters.";
        }

        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit) || !password.Any(char.IsPunctuation))
        {
            return "Initial password must include uppercase, lowercase, number and symbol.";
        }

        return null;
    }

    public async Task<Result> ChangeUserStatusAsync(ChangeTenantUserStatusCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserAsync(command.TenantId, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        switch (command.Status)
        {
            case UserStatus.Active:
                user.Unlock();
                break;
            case UserStatus.Disabled:
                user.Disable();
                break;
            case UserStatus.Locked:
                user.RegisterFailedLogin(1, _clock.UtcNow.AddYears(10));
                break;
            case UserStatus.Invited:
                break;
            default:
                return Result.Failure("Unsupported user status.");
        }

        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(User), user.Id, AuditAction.TenantChanged, $"Tenant user status changed to {command.Status}: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ResetUserMfaAsync(TenantUserActionCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserAsync(command.TenantId, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        user.DisableMfa();
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(User), user.Id, AuditAction.TenantChanged, $"Tenant user MFA reset: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> AssignUserRoleAsync(AssignTenantUserRoleCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserAsync(command.TenantId, command.UserId, cancellationToken);
        if (user is null || await _repository.GetRoleAsync(command.TenantId, command.RoleId, cancellationToken) is null)
        {
            return Result.Failure("User or role not found.");
        }

        user.AssignRole(command.RoleId);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(User), user.Id, AuditAction.TenantChanged, $"Role assigned: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RevokeUserRoleAsync(AssignTenantUserRoleCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserAsync(command.TenantId, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        user.RevokeRole(command.RoleId);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(User), user.Id, AuditAction.TenantChanged, $"Role revoked: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> CloseUserSessionsAsync(TenantUserActionCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetUserAsync(command.TenantId, command.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure("User not found.");
        }

        foreach (var session in user.Sessions.Where(session => session.IsActive(_clock.UtcNow)))
        {
            session.Revoke(_clock.UtcNow);
        }

        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, nameof(User), user.Id, AuditAction.TenantChanged, $"User sessions closed: {command.ChangeReason ?? "No reason supplied"}", cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result> ChangeTenantStateAsync(
        Guid tenantId,
        Guid? requestedByUserId,
        Action<Tenant> changeState,
        string auditMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await GetTenantOrNullAsync(tenantId, cancellationToken);
            if (tenant is null)
            {
                return Result.Failure("Tenant not found.");
            }

            changeState(tenant);
            await AppendAuditAsync(tenant.Id, requestedByUserId, nameof(Tenant), tenant.Id, AuditAction.TenantChanged, auditMessage, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private Task<Tenant?> GetTenantOrNullAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        if (tenantId == Guid.Empty)
        {
            return Task.FromResult<Tenant?>(null);
        }

        return _repository.GetByIdAsync(tenantId, cancellationToken);
    }

    private async Task AppendAuditAsync(
        Guid tenantId,
        Guid? requestedByUserId,
        string entityName,
        Guid entityId,
        AuditAction action,
        string? metadata,
        CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.Create(tenantId, requestedByUserId, entityName, entityId, action, _clock.UtcNow);
        if (!string.IsNullOrWhiteSpace(metadata))
        {
            auditLog.WithMetadata($$"""{"reason":"{{metadata.Replace("\"", "\\\"")}}"}""");
        }
        await _repository.AddAuditLogAsync(auditLog, cancellationToken);
    }

    private static TenantSummary ToSummary(Tenant tenant)
    {
        return new TenantSummary(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.LegalName,
            tenant.CommercialName,
            tenant.TaxIdentifier,
            tenant.CountryCode,
            tenant.Status,
            tenant.Settings.Culture,
            tenant.Settings.TimeZone,
            tenant.Settings.RequireMfa,
            tenant.Branding.DisplayName,
            tenant.Subscription.Plan,
            tenant.Subscription.Status,
            tenant.Subscription.MaxUsers,
            tenant.Subscription.MaxStorageGb);
    }

    private static TenantDetail ToDetail(Tenant tenant)
    {
        return new TenantDetail(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.LegalName,
            tenant.CommercialName,
            tenant.TaxIdentifier,
            tenant.Industry,
            tenant.Description,
            tenant.AddressLine1,
            tenant.City,
            tenant.Province,
            tenant.CountryCode,
            tenant.PostalCode,
            tenant.Phone,
            tenant.Email,
            tenant.Website,
            tenant.Currency,
            tenant.CreatedByUserId,
            tenant.Status,
            tenant.CreatedAtUtc,
            tenant.UpdatedAtUtc,
            new TenantSettingsSummary(
                tenant.Settings.Culture,
                tenant.Settings.Language,
                tenant.Settings.TimeZone,
                tenant.Settings.RequireMfa,
                tenant.Settings.DocumentRetentionDays,
                tenant.Settings.SessionTimeoutMinutes,
                tenant.Settings.PasswordExpirationDays,
                tenant.Settings.LockoutMaxFailedAttempts,
                tenant.Settings.LockoutMinutes,
                tenant.Settings.IpWhitelist,
                tenant.Settings.TrustedDevicesEnabled,
                tenant.Settings.SecurityScore),
            new TenantBrandingSummary(
                tenant.Branding.DisplayName,
                tenant.Branding.LogoUri,
                tenant.Branding.FaviconUri,
                tenant.Branding.PrimaryColor,
                tenant.Branding.SecondaryColor,
                tenant.Branding.Theme,
                tenant.Branding.LoginBackgroundUri,
                tenant.Branding.CorporateEmail,
                tenant.Branding.FooterText),
            new TenantSubscriptionSummary(
                tenant.Subscription.Plan,
                tenant.Subscription.Status,
                tenant.Subscription.MaxUsers,
                tenant.Subscription.MaxStorageGb,
                tenant.Subscription.ExpiresOn),
            tenant.Companies.Select(ToSummary).ToArray());
    }

    private static CompanySummary ToSummary(Company company)
    {
        return new CompanySummary(
            company.Id,
            company.TenantId,
            company.LegalName,
            company.TaxIdentifier,
            company.CountryCode,
            company.IsActive);
    }

    private async Task<TenantHealthCenterSummary> BuildHealthCenterAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var signals = await _repository.ListHealthSignalsAsync(tenantId, cancellationToken);
        var backups = await _repository.ListBackupRecordsAsync(tenantId, 1, 25, cancellationToken);
        var signalSummaries = signals.Select(ToSummary).ToArray();
        var backupSummaries = backups.Select(ToSummary).ToArray();
        var requiredComponents = new[] { "Database", "SMTP", "Storage", "Providers", "Jobs", "Background Services", "Queues", "Integrations", "License", "Space", "Backups", "OpenTelemetry" };
        var missingComponents = requiredComponents.Where(component => signals.All(signal => !string.Equals(signal.Component, component, StringComparison.OrdinalIgnoreCase))).ToArray();

        var overall = missingComponents.Length > 0
            ? TenantHealthStatus.Degraded
            : signals.Any(signal => signal.Status == TenantHealthStatus.Unhealthy)
                ? TenantHealthStatus.Unhealthy
                : signals.Any(signal => signal.Status == TenantHealthStatus.Degraded)
                    ? TenantHealthStatus.Degraded
                    : TenantHealthStatus.Healthy;

        var syntheticMissingSignals = missingComponents
            .Select(component => new TenantHealthSignalSummary(Guid.Empty, tenantId, component, TenantHealthStatus.Degraded, "No health signal has been reported for this component.", _clock.UtcNow, null));

        return new TenantHealthCenterSummary(overall, signalSummaries.Concat(syntheticMissingSignals).ToArray(), backupSummaries);
    }

    private async Task<TenantUserAdministrationSummary> BuildUserAdministrationAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var users = await _repository.ListUsersAsync(tenantId, cancellationToken);
        var roles = await _repository.ListRolesAsync(tenantId, cancellationToken);
        return new TenantUserAdministrationSummary(users.Select(ToSummary).ToArray(), roles.Select(ToSummary).ToArray());
    }

    private async Task UpsertHealthSignalAsync(Guid tenantId, string component, TenantHealthStatus status, string message, TimeSpan? duration, CancellationToken cancellationToken)
    {
        var signal = await _repository.GetHealthSignalAsync(tenantId, component, cancellationToken);
        if (signal is null)
        {
            signal = new TenantHealthSignal(tenantId, component, status, message, _clock.UtcNow);
            await _repository.AddHealthSignalAsync(signal, cancellationToken);
        }
        else
        {
            signal.Update(status, message, _clock.UtcNow, duration);
        }
    }

    private static TenantDomainSummary ToSummary(TenantDomain domain)
    {
        return new TenantDomainSummary(domain.Id, domain.TenantId, domain.HostName, domain.Kind, domain.Status, domain.IsDefault, domain.VerificationToken, domain.DnsStatus, domain.CertificateStatus, domain.HttpsEnabled, domain.RedirectToHostName, domain.VerifiedAtUtc, domain.LastCheckedAtUtc);
    }

    private static TenantSsoConfigurationSummary ToSummary(TenantSsoConfiguration sso)
    {
        return new TenantSsoConfigurationSummary(sso.Id, sso.TenantId, sso.Provider, sso.Name, sso.Authority, sso.MetadataUrl, sso.ClientId, sso.ClaimsMappingJson, sso.RoleMappingJson, sso.JitProvisioningEnabled, sso.ScimEnabled, sso.Status, sso.HealthStatus, sso.HealthMessage, sso.LastTestedAtUtc);
    }

    private static TenantApiCredentialSummary ToSummary(TenantApiCredential credential)
    {
        return new TenantApiCredentialSummary(credential.Id, credential.TenantId, credential.Name, credential.KeyPrefix, credential.Scopes, credential.ExpiresAtUtc, credential.LastUsedAtUtc, credential.Status);
    }

    private static TenantWebhookSummary ToSummary(TenantWebhookEndpoint webhook)
    {
        return new TenantWebhookSummary(webhook.Id, webhook.TenantId, webhook.Name, webhook.Url, webhook.Events, webhook.SigningAlgorithm, webhook.Status, webhook.MaxRetries, webhook.LastDeliveryAtUtc, webhook.LastDeliveryStatus, webhook.LastDeliveryMessage);
    }

    private static TenantWebhookDeliverySummary ToSummary(TenantWebhookDeliveryLog log)
    {
        return new TenantWebhookDeliverySummary(log.Id, log.TenantId, log.WebhookId, log.Status, log.Message, log.OccurredAtUtc, log.Attempt);
    }

    private static TenantLicenseSummary ToSummary(TenantLicense license)
    {
        return new TenantLicenseSummary(license.Id, license.TenantId, license.LicenseNumber, license.Status, license.FeaturesJson, license.ModulesJson, license.EntitlementsJson, license.PeriodStart, license.PeriodEnd, license.RenewalDate, license.SeatsPurchased, license.SeatsUsed, license.StorageGbPurchased, license.StorageBytesUsed);
    }

    private static TenantHealthSignalSummary ToSummary(TenantHealthSignal signal)
    {
        return new TenantHealthSignalSummary(signal.Id, signal.TenantId, signal.Component, signal.Status, signal.Message, signal.CheckedAtUtc, signal.Duration);
    }

    private static TenantBackupSummary ToSummary(TenantBackupRecord backup)
    {
        return new TenantBackupSummary(backup.Id, backup.TenantId, backup.BackupKind, backup.Result, backup.StartedAtUtc, backup.CompletedAtUtc, backup.Duration, backup.SizeBytes, backup.Message, backup.Rpo, backup.Rto);
    }

    private static TenantUserSummary ToSummary(User user)
    {
        var now = DateTimeOffset.UtcNow;
        return new TenantUserSummary(
            user.Id,
            user.TenantId,
            user.Email,
            user.FullName,
            user.Status,
            user.MfaEnabled,
            user.LastLoginAtUtc,
            user.AccessFailedCount,
            user.LockoutEndAtUtc,
            user.Roles.Select(role => role.RoleId).ToArray(),
            user.Sessions.Select(session => new UserSessionSummary(session.Id, session.TenantId, session.UserId, session.CreatedAt, session.ExpiresAtUtc, session.RevokedAtUtc, session.IsActive(now))).ToArray());
    }

    private static RoleSummary ToSummary(Role role)
    {
        return new RoleSummary(role.Id, role.TenantId, role.Name, role.IsSystemRole);
    }

    private static string NormalizeSlug(string slug)
    {
        return Guard.AgainstNullOrWhiteSpace(slug, nameof(slug), 80).ToLowerInvariant();
    }

    private static IReadOnlyCollection<TenantAlert> BuildAlerts(Tenant tenant, TenantAdministrationMetrics metrics)
    {
        var alerts = new List<TenantAlert>();
        if (tenant.Settings.SecurityScore < 80)
        {
            alerts.Add(new TenantAlert("warning", "Security score below enterprise target", "Review MFA, lockout, password expiration and trusted device policies."));
        }

        if (metrics.StorageProviders == 0)
        {
            alerts.Add(new TenantAlert("warning", "No storage provider configured", "Configure at least one tenant-scoped storage provider before production use."));
        }

        if (metrics.NotificationProviders == 0)
        {
            alerts.Add(new TenantAlert("info", "No notification provider configured", "Configure SMTP or a transactional email provider for real alerts."));
        }

        if (tenant.Status is TenantStatus.Suspended or TenantStatus.Archived)
        {
            alerts.Add(new TenantAlert("critical", "Tenant not active", "Restore or activate the tenant before onboarding real users."));
        }

        return alerts;
    }

    private sealed class Sha256PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return TenantApiCredential.HashSecret(password);
        }

        public PasswordVerificationResult Verify(string password, string passwordHash)
        {
            return string.Equals(HashPassword(password), passwordHash, StringComparison.OrdinalIgnoreCase)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }
}
