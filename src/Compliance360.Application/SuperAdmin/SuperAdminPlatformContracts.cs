using Compliance360.Shared;

namespace Compliance360.Application.SuperAdmin;

public interface ISuperAdminPlatformService
{
    Task<Result<SuperAdminPlatformCenter>> GetCenterAsync(CancellationToken cancellationToken = default);

    Task<Result<SuperAdminTenantSearchResult>> SearchTenantsAsync(SuperAdminTenantSearchQuery query, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<SuperAdminAuditTimelineItem>>> GetGlobalAuditTimelineAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

public interface ISuperAdminPlatformRepository
{
    Task<SuperAdminDashboardMetrics> GetDashboardMetricsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SuperAdminTenantListItem>> SearchTenantsAsync(string? searchText, string? status, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountTenantsAsync(string? searchText, string? status, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SuperAdminProviderSummary>> GetProvidersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SuperAdminLicenseSummary>> GetLicensesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SuperAdminModuleSummary>> GetModulesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SuperAdminHealthSignal>> GetHealthSignalsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SuperAdminBackupSummary>> GetBackupsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SuperAdminAuditTimelineItem>> GetGlobalAuditTimelineAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SuperAdminDatabaseMetric>> GetDatabaseMetricsAsync(CancellationToken cancellationToken = default);
}

public sealed record SuperAdminTenantSearchQuery(string? SearchText, string? Status, int Page, int PageSize);

public sealed record SuperAdminPlatformCenter(
    SuperAdminDashboardMetrics Metrics,
    IReadOnlyCollection<SuperAdminTenantListItem> Tenants,
    IReadOnlyCollection<SuperAdminProviderSummary> Providers,
    IReadOnlyCollection<SuperAdminLicenseSummary> Licenses,
    IReadOnlyCollection<SuperAdminModuleSummary> Modules,
    IReadOnlyCollection<SuperAdminHealthSignal> Health,
    IReadOnlyCollection<SuperAdminBackupSummary> Backups,
    IReadOnlyCollection<SuperAdminDatabaseMetric> Database,
    IReadOnlyCollection<SuperAdminAuditTimelineItem> AuditTimeline,
    IReadOnlyCollection<SuperAdminAlert> Alerts,
    IReadOnlyCollection<SuperAdminQuickAction> QuickActions);

public sealed record SuperAdminDashboardMetrics(
    int Tenants,
    int ActiveTenants,
    int TrialTenants,
    int SuspendedTenants,
    int ArchivedTenants,
    int TotalUsers,
    int ActiveUsers,
    int Documents,
    int Audits,
    int Capas,
    int Risks,
    int Indicators,
    long StorageBytes,
    long StorageUsedBytes,
    int StorageProviders,
    int SmtpProviders,
    int ApiRequests,
    int Errors,
    string GlobalHealth,
    int BackgroundJobs,
    int Licenses,
    int Alerts,
    DateTimeOffset GeneratedAtUtc);

public sealed record SuperAdminTenantSearchResult(IReadOnlyCollection<SuperAdminTenantListItem> Items, int TotalCount, int Page, int PageSize);

public sealed record SuperAdminTenantListItem(Guid Id, string Name, string Slug, string LegalName, string Status, string Plan, string LicenseStatus, int Users, long StorageBytes, DateTimeOffset CreatedAtUtc);

public sealed record SuperAdminProviderSummary(Guid Id, Guid TenantId, string TenantName, string Type, string Provider, string Name, bool IsEnabled, bool IsDefault, string Health, DateTimeOffset? LastHealthCheckAtUtc, string? LastHealthMessage);

public sealed record SuperAdminLicenseSummary(Guid TenantId, string TenantName, string Plan, string Status, int MaxUsers, int UsersUsed, int MaxStorageGb, long StorageUsedBytes, DateOnly? ExpiresOn, string? LicenseNumber, string? EntitlementsJson, DateOnly? RenewalDate);

public sealed record SuperAdminModuleSummary(Guid TenantId, string TenantName, string Module, bool Enabled, string Source, string Health);

public sealed record SuperAdminHealthSignal(string Component, string Status, string Message, DateTimeOffset CheckedAtUtc, Guid? TenantId);

public sealed record SuperAdminBackupSummary(Guid Id, Guid TenantId, string TenantName, string BackupKind, string Result, DateTimeOffset CompletedAtUtc, TimeSpan Duration, long SizeBytes, TimeSpan Rpo, TimeSpan Rto);

public sealed record SuperAdminDatabaseMetric(string Name, string Value, string Status, string Description);

public sealed record SuperAdminAuditTimelineItem(Guid Id, Guid? TenantId, string? TenantName, DateTimeOffset OccurredAtUtc, string Action, string Category, string EntityName, Guid? EntityId, Guid? UserId, string? UserName, string? IpAddress, string? CorrelationId, bool Success, string? MetadataJson);

public sealed record SuperAdminAlert(string Severity, string Title, string Message, string Area);

public sealed record SuperAdminQuickAction(string Key, string Label, string Description, string Permission, string Route);
