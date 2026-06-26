using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.TenantManagement;
using Compliance360.Shared;

namespace Compliance360.Application.TenantManagement;

public sealed class TenantManagementService : ITenantManagementService
{
    private readonly ITenantManagementRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public TenantManagementService(
        ITenantManagementRepository repository,
        IApplicationDbContext dbContext,
        IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _clock = clock;
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
}
