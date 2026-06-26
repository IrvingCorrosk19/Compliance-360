using Compliance360.Domain.Audit;
using Compliance360.Domain.TenantManagement;
using Compliance360.Shared;

namespace Compliance360.Application.TenantManagement;

public interface ITenantManagementService
{
    Task<Result<TenantSummary>> CreateTenantAsync(CreateTenantCommand command, CancellationToken cancellationToken = default);

    Task<Result<TenantAdministrationDashboard>> GetAdministrationDashboardAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Result<TenantDetail>> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Result<TenantSearchResult>> SearchTenantsAsync(TenantSearchQuery query, CancellationToken cancellationToken = default);

    Task<Result> UpdateGeneralInformationAsync(UpdateTenantGeneralInformationCommand command, CancellationToken cancellationToken = default);

    Task<Result<CompanySummary>> AddCompanyAsync(AddCompanyCommand command, CancellationToken cancellationToken = default);

    Task<Result> ActivateTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default);

    Task<Result> SuspendTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default);

    Task<Result> StartTrialAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default);

    Task<Result> ArchiveTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default);

    Task<Result> RestoreTenantAsync(Guid tenantId, Guid? requestedByUserId, CancellationToken cancellationToken = default);

    Task<Result> ConfigureSettingsAsync(ConfigureTenantSettingsCommand command, CancellationToken cancellationToken = default);

    Task<Result> ConfigureSecurityAsync(ConfigureTenantSecurityCommand command, CancellationToken cancellationToken = default);

    Task<Result> ConfigureBrandingAsync(ConfigureTenantBrandingCommand command, CancellationToken cancellationToken = default);

    Task<Result> ChangeSubscriptionAsync(ChangeSubscriptionCommand command, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<TenantAuditTimelineItem>>> GetAuditTimelineAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default);
}

public interface ITenantManagementRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(Guid excludeTenantId, string slug, CancellationToken cancellationToken = default);

    Task<bool> TaxIdentifierExistsAsync(Guid excludeTenantId, string taxIdentifier, CancellationToken cancellationToken = default);

    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Tenant>> SearchAsync(string? searchText, TenantStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(string? searchText, TenantStatus? status, CancellationToken cancellationToken = default);

    Task<TenantAdministrationMetrics> GetAdministrationMetricsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantAuditTimelineItem>> GetAuditTimelineAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    Task AddCompanyAsync(Company company, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    string? LegalName,
    string? CommercialName,
    string? TaxIdentifier,
    string? CountryCode,
    string? Currency,
    Guid? RequestedByUserId)
{
    public CreateTenantCommand(string name, string slug, Guid? requestedByUserId)
        : this(name, slug, null, null, null, null, null, requestedByUserId)
    {
    }
}

public sealed record TenantSearchQuery(
    string? SearchText,
    TenantStatus? Status,
    int Page,
    int PageSize);

public sealed record UpdateTenantGeneralInformationCommand(
    Guid TenantId,
    string Name,
    string LegalName,
    string CommercialName,
    string TaxIdentifier,
    string Industry,
    string? Description,
    string? AddressLine1,
    string? City,
    string? Province,
    string CountryCode,
    string? PostalCode,
    string? Phone,
    string? Email,
    string? Website,
    string Currency,
    string? ChangeReason,
    Guid? RequestedByUserId);

public sealed record AddCompanyCommand(
    Guid TenantId,
    string LegalName,
    string TaxIdentifier,
    string CountryCode,
    Guid? RequestedByUserId);

public sealed record ConfigureTenantSettingsCommand(
    Guid TenantId,
    string Culture,
    string TimeZone,
    bool RequireMfa,
    int DocumentRetentionDays,
    Guid? RequestedByUserId);

public sealed record ConfigureTenantSecurityCommand(
    Guid TenantId,
    bool RequireMfa,
    int SessionTimeoutMinutes,
    int PasswordExpirationDays,
    int LockoutMaxFailedAttempts,
    int LockoutMinutes,
    string? IpWhitelist,
    bool TrustedDevicesEnabled,
    int SecurityScore,
    string? ChangeReason,
    Guid? RequestedByUserId);

public sealed record ConfigureTenantBrandingCommand(
    Guid TenantId,
    string DisplayName,
    string? LogoUri,
    string? FaviconUri,
    string PrimaryColor,
    string SecondaryColor,
    string Theme,
    string? LoginBackgroundUri,
    string? CorporateEmail,
    string? FooterText,
    string? ChangeReason,
    Guid? RequestedByUserId)
{
    public ConfigureTenantBrandingCommand(Guid tenantId, string displayName, string? logoUri, string primaryColor, string secondaryColor, Guid? requestedByUserId)
        : this(tenantId, displayName, logoUri, null, primaryColor, secondaryColor, "System", null, null, null, null, requestedByUserId)
    {
    }
}

public sealed record ChangeSubscriptionCommand(
    Guid TenantId,
    SubscriptionPlan Plan,
    int MaxUsers,
    int MaxStorageGb,
    SubscriptionStatus Status,
    DateOnly? ExpiresOn,
    string? ChangeReason,
    Guid? RequestedByUserId)
{
    public ChangeSubscriptionCommand(Guid tenantId, SubscriptionPlan plan, int maxUsers, int maxStorageGb, Guid? requestedByUserId)
        : this(tenantId, plan, maxUsers, maxStorageGb, SubscriptionStatus.Active, null, null, requestedByUserId)
    {
    }
}

public sealed record TenantSummary(
    Guid Id,
    string Name,
    string Slug,
    string LegalName,
    string CommercialName,
    string TaxIdentifier,
    string CountryCode,
    TenantStatus Status,
    string Culture,
    string TimeZone,
    bool RequireMfa,
    string BrandingDisplayName,
    SubscriptionPlan Plan,
    SubscriptionStatus SubscriptionStatus,
    int MaxUsers,
    int MaxStorageGb);

public sealed record TenantDetail(
    Guid Id,
    string Name,
    string Slug,
    string LegalName,
    string CommercialName,
    string TaxIdentifier,
    string Industry,
    string? Description,
    string? AddressLine1,
    string? City,
    string? Province,
    string CountryCode,
    string? PostalCode,
    string? Phone,
    string? Email,
    string? Website,
    string Currency,
    Guid? CreatedByUserId,
    TenantStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    TenantSettingsSummary Settings,
    TenantBrandingSummary Branding,
    TenantSubscriptionSummary Subscription,
    IReadOnlyCollection<CompanySummary> Companies);

public sealed record TenantSettingsSummary(
    string Culture,
    string Language,
    string TimeZone,
    bool RequireMfa,
    int DocumentRetentionDays,
    int SessionTimeoutMinutes,
    int PasswordExpirationDays,
    int LockoutMaxFailedAttempts,
    int LockoutMinutes,
    string IpWhitelist,
    bool TrustedDevicesEnabled,
    int SecurityScore);

public sealed record TenantBrandingSummary(
    string DisplayName,
    string? LogoUri,
    string? FaviconUri,
    string PrimaryColor,
    string SecondaryColor,
    string Theme,
    string? LoginBackgroundUri,
    string CorporateEmail,
    string FooterText);

public sealed record TenantSubscriptionSummary(
    SubscriptionPlan Plan,
    SubscriptionStatus Status,
    int MaxUsers,
    int MaxStorageGb,
    DateOnly? ExpiresOn);

public sealed record TenantSearchResult(
    IReadOnlyCollection<TenantSummary> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record TenantAdministrationDashboard(
    TenantDetail Tenant,
    TenantAdministrationMetrics Metrics,
    IReadOnlyCollection<TenantAlert> Alerts,
    IReadOnlyCollection<TenantAuditTimelineItem> Timeline);

public sealed record TenantAdministrationMetrics(
    int Users,
    int ActiveUsers,
    int Roles,
    long StorageBytes,
    int Documents,
    int Suppliers,
    int Audits,
    int Capas,
    int Risks,
    int Indicators,
    int Notifications,
    int StorageProviders,
    int NotificationProviders,
    DateTimeOffset? LastLoginAtUtc,
    DateTimeOffset? LastBackupAtUtc,
    bool Health);

public sealed record TenantAlert(string Severity, string Title, string Message);

public sealed record TenantAuditTimelineItem(
    Guid Id,
    DateTimeOffset OccurredAtUtc,
    string Action,
    string EntityName,
    Guid? EntityId,
    Guid? UserId,
    string? UserName,
    string? IpAddress,
    string? CorrelationId,
    string? BeforeValuesJson,
    string? AfterValuesJson,
    string? MetadataJson);

public sealed record CompanySummary(
    Guid Id,
    Guid TenantId,
    string LegalName,
    string TaxIdentifier,
    string CountryCode,
    bool IsActive);
