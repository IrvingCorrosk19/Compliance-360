using Compliance360.Domain.Audit;
using Compliance360.Domain.Identity;
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

    Task<Result<TenantAdministrationCenterState>> GetAdministrationCenterStateAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Result<TenantDomainSummary>> UpsertDomainAsync(UpsertTenantDomainCommand command, CancellationToken cancellationToken = default);

    Task<Result> DisableDomainAsync(TenantEntityActionCommand command, CancellationToken cancellationToken = default);

    Task<Result<TenantSsoConfigurationSummary>> UpsertSsoAsync(UpsertTenantSsoCommand command, CancellationToken cancellationToken = default);

    Task<Result> TestSsoAsync(TenantEntityActionCommand command, CancellationToken cancellationToken = default);

    Task<Result<TenantApiCredentialSummary>> CreateApiCredentialAsync(CreateTenantApiCredentialCommand command, CancellationToken cancellationToken = default);

    Task<Result<TenantApiCredentialSummary>> RotateApiCredentialAsync(RotateTenantApiCredentialCommand command, CancellationToken cancellationToken = default);

    Task<Result> RevokeApiCredentialAsync(TenantEntityActionCommand command, CancellationToken cancellationToken = default);

    Task<Result<TenantWebhookSummary>> UpsertWebhookAsync(UpsertTenantWebhookCommand command, CancellationToken cancellationToken = default);

    Task<Result<TenantWebhookDeliverySummary>> TestWebhookAsync(TenantEntityActionCommand command, CancellationToken cancellationToken = default);

    Task<Result> DisableWebhookAsync(TenantEntityActionCommand command, CancellationToken cancellationToken = default);

    Task<Result<TenantLicenseSummary>> UpsertLicenseAsync(UpsertTenantLicenseCommand command, CancellationToken cancellationToken = default);

    Task<Result<TenantHealthCenterSummary>> GetHealthCenterAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Result<TenantBackupSummary>> RecordBackupAsync(RecordTenantBackupCommand command, CancellationToken cancellationToken = default);

    Task<Result<TenantUserAdministrationSummary>> GetUsersAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<Result<TenantUserSummary>> CreateUserAsync(CreateTenantUserCommand command, CancellationToken cancellationToken = default);

    Task<Result<TenantUserSummary>> UpdateUserAsync(UpdateTenantUserCommand command, CancellationToken cancellationToken = default);

    Task<Result> ChangeUserStatusAsync(ChangeTenantUserStatusCommand command, CancellationToken cancellationToken = default);

    Task<Result> ResetUserMfaAsync(TenantUserActionCommand command, CancellationToken cancellationToken = default);

    Task<Result> ResetUserPasswordAsync(ResetTenantUserPasswordCommand command, CancellationToken cancellationToken = default);

    Task<Result> AssignUserRoleAsync(AssignTenantUserRoleCommand command, CancellationToken cancellationToken = default);

    Task<Result> RevokeUserRoleAsync(AssignTenantUserRoleCommand command, CancellationToken cancellationToken = default);

    Task<Result> CloseUserSessionsAsync(TenantUserActionCommand command, CancellationToken cancellationToken = default);
}

public interface ITenantManagementRepository
{
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(Guid excludeTenantId, string slug, CancellationToken cancellationToken = default);

    Task<bool> TaxIdentifierExistsAsync(string taxIdentifier, CancellationToken cancellationToken = default);

    Task<bool> TaxIdentifierExistsAsync(Guid excludeTenantId, string taxIdentifier, CancellationToken cancellationToken = default);

    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Tenant>> SearchAsync(string? searchText, TenantStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(string? searchText, TenantStatus? status, CancellationToken cancellationToken = default);

    Task<TenantAdministrationMetrics> GetAdministrationMetricsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantAuditTimelineItem>> GetAuditTimelineAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantDomain>> ListDomainsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<TenantDomain?> GetDomainAsync(Guid tenantId, Guid domainId, CancellationToken cancellationToken = default);

    Task<bool> DomainExistsAsync(Guid tenantId, string hostName, Guid? excludeDomainId, CancellationToken cancellationToken = default);

    Task AddDomainAsync(TenantDomain domain, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantSsoConfiguration>> ListSsoConfigurationsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<TenantSsoConfiguration?> GetSsoConfigurationAsync(Guid tenantId, Guid ssoId, CancellationToken cancellationToken = default);

    Task AddSsoConfigurationAsync(TenantSsoConfiguration configuration, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantApiCredential>> ListApiCredentialsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<TenantApiCredential?> GetApiCredentialAsync(Guid tenantId, Guid apiCredentialId, CancellationToken cancellationToken = default);

    Task AddApiCredentialAsync(TenantApiCredential credential, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantWebhookEndpoint>> ListWebhooksAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<TenantWebhookEndpoint?> GetWebhookAsync(Guid tenantId, Guid webhookId, CancellationToken cancellationToken = default);

    Task AddWebhookAsync(TenantWebhookEndpoint webhook, CancellationToken cancellationToken = default);

    Task AddWebhookDeliveryLogAsync(TenantWebhookDeliveryLog deliveryLog, CancellationToken cancellationToken = default);

    Task<TenantLicense?> GetLicenseAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task AddLicenseAsync(TenantLicense license, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantHealthSignal>> ListHealthSignalsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<TenantHealthSignal?> GetHealthSignalAsync(Guid tenantId, string component, CancellationToken cancellationToken = default);

    Task AddHealthSignalAsync(TenantHealthSignal signal, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantBackupRecord>> ListBackupRecordsAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task AddBackupRecordAsync(TenantBackupRecord backup, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<User>> ListUsersAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<User?> GetUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> UserEmailExistsAsync(Guid tenantId, string normalizedEmail, Guid? excludeUserId, CancellationToken cancellationToken = default);

    Task<Role?> GetRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Role>> ListRolesAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task AddUserAsync(User user, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserSessionSummary>> ListUserSessionsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

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
    Guid? RequestedByUserId,
    string? AdminEmail = null,
    string? AdminFullName = null,
    string? AdminPassword = null)
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

public sealed record TenantAdministrationCenterState(
    TenantDetail Tenant,
    TenantAdministrationMetrics Metrics,
    IReadOnlyCollection<TenantDomainSummary> Domains,
    IReadOnlyCollection<TenantSsoConfigurationSummary> SsoConfigurations,
    IReadOnlyCollection<TenantApiCredentialSummary> ApiCredentials,
    IReadOnlyCollection<TenantWebhookSummary> Webhooks,
    TenantLicenseSummary? License,
    TenantHealthCenterSummary Health,
    TenantUserAdministrationSummary Users,
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

public sealed record TenantEntityActionCommand(Guid TenantId, Guid EntityId, string? ChangeReason, Guid RequestedByUserId);

public sealed record TenantUserActionCommand(Guid TenantId, Guid UserId, string? ChangeReason, Guid RequestedByUserId);

public sealed record UpsertTenantDomainCommand(Guid TenantId, Guid? DomainId, string HostName, TenantDomainKind Kind, bool IsDefault, string? RedirectToHostName, string? ChangeReason, Guid RequestedByUserId);

public sealed record UpsertTenantSsoCommand(Guid TenantId, Guid? ConfigurationId, TenantSsoProviderType Provider, string Name, string Authority, string? MetadataUrl, string ClientId, string? SecretReference, string ClaimsMappingJson, string RoleMappingJson, bool JitProvisioningEnabled, bool ScimEnabled, string? CertificateThumbprint, bool Enabled, string? ChangeReason, Guid RequestedByUserId);

public sealed record CreateTenantApiCredentialCommand(Guid TenantId, string Name, string PlainTextSecret, string Scopes, DateTimeOffset? ExpiresAtUtc, string? ChangeReason, Guid RequestedByUserId);

public sealed record RotateTenantApiCredentialCommand(Guid TenantId, Guid ApiCredentialId, string PlainTextSecret, DateTimeOffset? ExpiresAtUtc, string? ChangeReason, Guid RequestedByUserId);

public sealed record UpsertTenantWebhookCommand(Guid TenantId, Guid? WebhookId, string Name, string Url, string Events, string PlainTextSecret, int MaxRetries, bool Enabled, string? ChangeReason, Guid RequestedByUserId);

public sealed record UpsertTenantLicenseCommand(Guid TenantId, string LicenseNumber, TenantLicenseStatus Status, string FeaturesJson, string ModulesJson, string EntitlementsJson, DateOnly PeriodStart, DateOnly PeriodEnd, DateOnly RenewalDate, int SeatsPurchased, int SeatsUsed, int StorageGbPurchased, long StorageBytesUsed, string? ChangeReason, Guid RequestedByUserId);

public sealed record RecordTenantBackupCommand(Guid TenantId, string BackupKind, string Result, DateTimeOffset StartedAtUtc, DateTimeOffset CompletedAtUtc, long SizeBytes, string Message, int RpoMinutes, int RtoMinutes, Guid RequestedByUserId);

public sealed record CreateTenantUserCommand(Guid TenantId, string Email, string FullName, string InitialPassword, bool ForcePasswordChange, Guid? RoleId, string? ChangeReason, Guid RequestedByUserId);

public sealed record UpdateTenantUserCommand(Guid TenantId, Guid UserId, string Email, string FullName, string? ChangeReason, Guid RequestedByUserId);

public sealed record ResetTenantUserPasswordCommand(Guid TenantId, Guid UserId, string NewPassword, bool ForcePasswordChange, string? ChangeReason, Guid RequestedByUserId);

public sealed record ChangeTenantUserStatusCommand(Guid TenantId, Guid UserId, UserStatus Status, string? ChangeReason, Guid RequestedByUserId);

public sealed record AssignTenantUserRoleCommand(Guid TenantId, Guid UserId, Guid RoleId, string? ChangeReason, Guid RequestedByUserId);

public sealed record TenantDomainSummary(Guid Id, Guid TenantId, string HostName, TenantDomainKind Kind, TenantDomainStatus Status, bool IsDefault, string VerificationToken, string DnsStatus, TenantCertificateStatus CertificateStatus, bool HttpsEnabled, string? RedirectToHostName, DateTimeOffset? VerifiedAtUtc, DateTimeOffset? LastCheckedAtUtc);

public sealed record TenantSsoConfigurationSummary(Guid Id, Guid TenantId, TenantSsoProviderType Provider, string Name, string Authority, string MetadataUrl, string ClientId, string ClaimsMappingJson, string RoleMappingJson, bool JitProvisioningEnabled, bool ScimEnabled, TenantIntegrationStatus Status, TenantHealthStatus HealthStatus, string HealthMessage, DateTimeOffset? LastTestedAtUtc);

public sealed record TenantApiCredentialSummary(Guid Id, Guid TenantId, string Name, string KeyPrefix, string Scopes, DateTimeOffset? ExpiresAtUtc, DateTimeOffset? LastUsedAtUtc, TenantSecretStatus Status);

public sealed record TenantWebhookSummary(Guid Id, Guid TenantId, string Name, string Url, string Events, string SigningAlgorithm, TenantIntegrationStatus Status, int MaxRetries, DateTimeOffset? LastDeliveryAtUtc, TenantWebhookDeliveryStatus LastDeliveryStatus, string? LastDeliveryMessage);

public sealed record TenantWebhookDeliverySummary(Guid Id, Guid TenantId, Guid WebhookId, TenantWebhookDeliveryStatus Status, string Message, DateTimeOffset OccurredAtUtc, int Attempt);

public sealed record TenantLicenseSummary(Guid Id, Guid TenantId, string LicenseNumber, TenantLicenseStatus Status, string FeaturesJson, string ModulesJson, string EntitlementsJson, DateOnly PeriodStart, DateOnly PeriodEnd, DateOnly RenewalDate, int SeatsPurchased, int SeatsUsed, int StorageGbPurchased, long StorageBytesUsed);

public sealed record TenantHealthSignalSummary(Guid Id, Guid TenantId, string Component, TenantHealthStatus Status, string Message, DateTimeOffset CheckedAtUtc, TimeSpan? Duration);

public sealed record TenantBackupSummary(Guid Id, Guid TenantId, string BackupKind, string Result, DateTimeOffset StartedAtUtc, DateTimeOffset CompletedAtUtc, TimeSpan Duration, long SizeBytes, string Message, TimeSpan Rpo, TimeSpan Rto);

public sealed record TenantHealthCenterSummary(TenantHealthStatus OverallStatus, IReadOnlyCollection<TenantHealthSignalSummary> Signals, IReadOnlyCollection<TenantBackupSummary> Backups);

public sealed record TenantUserSummary(Guid Id, Guid TenantId, string Email, string FullName, UserStatus Status, bool MfaEnabled, DateTimeOffset? LastLoginAtUtc, int AccessFailedCount, DateTimeOffset? LockoutEndAtUtc, IReadOnlyCollection<Guid> RoleIds, IReadOnlyCollection<UserSessionSummary> Sessions);

public sealed record UserSessionSummary(Guid Id, Guid TenantId, Guid UserId, DateTimeOffset CreatedAtUtc, DateTimeOffset ExpiresAtUtc, DateTimeOffset? RevokedAtUtc, bool IsActive);

public sealed record TenantUserAdministrationSummary(IReadOnlyCollection<TenantUserSummary> Users, IReadOnlyCollection<RoleSummary> Roles);

public sealed record RoleSummary(Guid Id, Guid TenantId, string Name, bool IsSystemRole);

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
