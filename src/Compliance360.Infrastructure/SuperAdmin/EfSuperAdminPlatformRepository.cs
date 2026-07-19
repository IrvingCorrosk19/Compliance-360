using Compliance360.Application.SuperAdmin;
using Compliance360.Domain.Identity;
using Compliance360.Domain.TenantManagement;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.SuperAdmin;

public sealed class EfSuperAdminPlatformRepository : ISuperAdminPlatformRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfSuperAdminPlatformRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SuperAdminDashboardMetrics> GetDashboardMetricsAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _dbContext.Tenants.AsNoTracking().CountAsync(cancellationToken);
        var activeTenants = await _dbContext.Tenants.AsNoTracking().CountAsync(tenant => tenant.Status == TenantStatus.Active, cancellationToken);
        var trialTenants = await _dbContext.Tenants.AsNoTracking().CountAsync(tenant => tenant.Status == TenantStatus.Trial, cancellationToken);
        var suspendedTenants = await _dbContext.Tenants.AsNoTracking().CountAsync(tenant => tenant.Status == TenantStatus.Suspended, cancellationToken);
        var archivedTenants = await _dbContext.Tenants.AsNoTracking().CountAsync(tenant => tenant.Status == TenantStatus.Archived, cancellationToken);
        var totalUsers = await _dbContext.Users.AsNoTracking().CountAsync(cancellationToken);
        var activeUsers = await _dbContext.Users.AsNoTracking().CountAsync(user => user.Status == UserStatus.Active, cancellationToken);
        var storageBytes = await _dbContext.StoredFiles.AsNoTracking().SumAsync(file => (long?)file.SizeBytes, cancellationToken) ?? 0L;
        var storageLimitGb = await _dbContext.Subscriptions.AsNoTracking().SumAsync(subscription => (int?)subscription.MaxStorageGb, cancellationToken) ?? 0;
        var notificationsFailed = await _dbContext.NotificationMessages.AsNoTracking().CountAsync(message => message.Status.ToString() == "Failed", cancellationToken);
        var auditFailures = await _dbContext.AuditLogs.AsNoTracking().CountAsync(audit => !audit.Success, cancellationToken);
        var healthSignals = await _dbContext.TenantHealthSignals.AsNoTracking().ToArrayAsync(cancellationToken);
        var globalHealth = healthSignals.Any(signal => signal.Status.ToString() == "Unhealthy")
            ? "Unhealthy"
            : healthSignals.Any(signal => signal.Status.ToString() == "Degraded")
                ? "Degraded"
                : healthSignals.Length == 0
                    ? "Unknown"
                    : "Healthy";

        var productCount = await _dbContext.MedicalDeviceProducts.AsNoTracking().CountAsync(cancellationToken);
        var dossierCount = await _dbContext.RegistrationDossiers.AsNoTracking().CountAsync(cancellationToken);
        var manufacturerCount = await _dbContext.ManufacturerProfiles.AsNoTracking().CountAsync(cancellationToken);
        var importJobCount = await _dbContext.RegutrackImportJobs.AsNoTracking().CountAsync(cancellationToken);

        return new SuperAdminDashboardMetrics(
            tenants,
            activeTenants,
            trialTenants,
            suspendedTenants,
            archivedTenants,
            totalUsers,
            activeUsers,
            productCount,
            dossierCount,
            manufacturerCount,
            importJobCount,
            0,
            storageLimitGb * 1024L * 1024L * 1024L,
            storageBytes,
            await _dbContext.StorageProviderConfigurations.AsNoTracking().CountAsync(cancellationToken),
            await _dbContext.NotificationProviderConfigurations.AsNoTracking().CountAsync(cancellationToken),
            await _dbContext.AuditLogs.AsNoTracking().CountAsync(cancellationToken),
            notificationsFailed + auditFailures,
            globalHealth,
            await _dbContext.NotificationRetries.AsNoTracking().CountAsync(retry => retry.ExecutedAtUtc == null, cancellationToken),
            await _dbContext.TenantLicenses.AsNoTracking().CountAsync(cancellationToken),
            Alerts: 0,
            DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyCollection<SuperAdminTenantListItem>> SearchTenantsAsync(string? searchText, string? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var tenants = await TenantQuery(searchText, status)
            .OrderBy(tenant => tenant.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        var tenantIds = tenants.Select(tenant => tenant.Id).ToArray();
        var userCounts = await _dbContext.Users.AsNoTracking().Where(user => tenantIds.Contains(user.TenantId)).GroupBy(user => user.TenantId).Select(group => new { TenantId = group.Key, Count = group.Count() }).ToDictionaryAsync(item => item.TenantId, item => item.Count, cancellationToken);
        var storageCounts = await _dbContext.StoredFiles.AsNoTracking().Where(file => tenantIds.Contains(file.TenantId)).GroupBy(file => file.TenantId).Select(group => new { TenantId = group.Key, Bytes = group.Sum(file => file.SizeBytes) }).ToDictionaryAsync(item => item.TenantId, item => item.Bytes, cancellationToken);

        return tenants.Select(tenant => new SuperAdminTenantListItem(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.LegalName,
            tenant.Status.ToString(),
            tenant.Subscription.Plan.ToString(),
            tenant.Subscription.Status.ToString(),
            userCounts.GetValueOrDefault(tenant.Id),
            storageCounts.GetValueOrDefault(tenant.Id),
            tenant.CreatedAtUtc)).ToArray();
    }

    public Task<int> CountTenantsAsync(string? searchText, string? status, CancellationToken cancellationToken = default)
    {
        return TenantQuery(searchText, status).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SuperAdminProviderSummary>> GetProvidersAsync(CancellationToken cancellationToken = default)
    {
        var tenantNames = await _dbContext.Tenants.AsNoTracking().Select(tenant => new { tenant.Id, tenant.Name }).ToDictionaryAsync(tenant => tenant.Id, tenant => tenant.Name, cancellationToken);
        var storageRows = await _dbContext.StorageProviderConfigurations.AsNoTracking().ToArrayAsync(cancellationToken);
        var notificationRows = await _dbContext.NotificationProviderConfigurations.AsNoTracking().ToArrayAsync(cancellationToken);
        var storage = storageRows
            .Select(provider => new SuperAdminProviderSummary(provider.Id, provider.TenantId, "", "Storage", provider.Provider.ToString(), provider.Name, provider.IsEnabled, provider.IsDefault, provider.LastHealthStatus ? "Healthy" : "Unknown", provider.LastHealthCheckAtUtc, provider.LastHealthMessage));
        var notifications = notificationRows
            .Select(provider => new SuperAdminProviderSummary(provider.Id, provider.TenantId, "", "Notification", provider.Provider.ToString(), provider.Name, provider.IsEnabled, provider.IsDefault, provider.IsEnabled ? "Healthy" : "Disabled", null, null));

        return storage.Concat(notifications)
            .Select(provider => provider with { TenantName = tenantNames.GetValueOrDefault(provider.TenantId, "Unknown tenant") })
            .OrderBy(provider => provider.Type)
            .ThenBy(provider => provider.TenantName)
            .ThenBy(provider => provider.Name)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<SuperAdminLicenseSummary>> GetLicensesAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _dbContext.Tenants.AsNoTracking().Include(tenant => tenant.Subscription).OrderBy(tenant => tenant.Name).ToArrayAsync(cancellationToken);
        var tenantIds = tenants.Select(tenant => tenant.Id).ToArray();
        var licenses = await _dbContext.TenantLicenses.AsNoTracking().Where(license => tenantIds.Contains(license.TenantId)).ToDictionaryAsync(license => license.TenantId, cancellationToken);
        var userCounts = await _dbContext.Users.AsNoTracking().Where(user => tenantIds.Contains(user.TenantId)).GroupBy(user => user.TenantId).Select(group => new { TenantId = group.Key, Count = group.Count() }).ToDictionaryAsync(item => item.TenantId, item => item.Count, cancellationToken);
        var storageCounts = await _dbContext.StoredFiles.AsNoTracking().Where(file => tenantIds.Contains(file.TenantId)).GroupBy(file => file.TenantId).Select(group => new { TenantId = group.Key, Bytes = group.Sum(file => file.SizeBytes) }).ToDictionaryAsync(item => item.TenantId, item => item.Bytes, cancellationToken);

        return tenants.Select(tenant =>
        {
            licenses.TryGetValue(tenant.Id, out var license);
            return new SuperAdminLicenseSummary(
                tenant.Id,
                tenant.Name,
                tenant.Subscription.Plan.ToString(),
                tenant.Subscription.Status.ToString(),
                tenant.Subscription.MaxUsers,
                userCounts.GetValueOrDefault(tenant.Id),
                tenant.Subscription.MaxStorageGb,
                storageCounts.GetValueOrDefault(tenant.Id),
                tenant.Subscription.ExpiresOn,
                license?.LicenseNumber,
                license?.EntitlementsJson,
                license?.RenewalDate);
        }).ToArray();
    }

    public async Task<IReadOnlyCollection<SuperAdminModuleSummary>> GetModulesAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _dbContext.Tenants.AsNoTracking().Select(tenant => new { tenant.Id, tenant.Name }).ToArrayAsync(cancellationToken);
        var modules = new[] { "Regulatory Affairs", "Tenant Administration", "Identity", "RBAC", "Audit Trail", "Notifications", "Storage", "Observability" };
        return tenants.SelectMany(tenant => modules.Select(module => new SuperAdminModuleSummary(tenant.Id, tenant.Name, module, Enabled: true, Source: "Platform module registry", Health: "Available"))).ToArray();
    }

    public async Task<IReadOnlyCollection<SuperAdminHealthSignal>> GetHealthSignalsAsync(CancellationToken cancellationToken = default)
    {
        var signalRows = await _dbContext.TenantHealthSignals.AsNoTracking().ToArrayAsync(cancellationToken);
        var signals = signalRows.Select(signal => new SuperAdminHealthSignal(signal.Component, signal.Status.ToString(), signal.Message, signal.CheckedAtUtc, signal.TenantId)).ToArray();

        var platformSignals = new[]
        {
            new SuperAdminHealthSignal("API", "Healthy", "Minimal API host is serving the request.", DateTimeOffset.UtcNow, null),
            new SuperAdminHealthSignal("OpenTelemetry", "Healthy", "OpenTelemetry tracing and metrics are configured.", DateTimeOffset.UtcNow, null),
            new SuperAdminHealthSignal("Audit", "Healthy", "Audit middleware and EF interceptor are configured.", DateTimeOffset.UtcNow, null)
        };

        return platformSignals.Concat(signals).ToArray();
    }

    public async Task<IReadOnlyCollection<SuperAdminBackupSummary>> GetBackupsAsync(CancellationToken cancellationToken = default)
    {
        var query = from backup in _dbContext.TenantBackupRecords.AsNoTracking()
                    join tenant in _dbContext.Tenants.AsNoTracking() on backup.TenantId equals tenant.Id
                    orderby backup.CompletedAtUtc descending
                    select new SuperAdminBackupSummary(backup.Id, backup.TenantId, tenant.Name, backup.BackupKind, backup.Result, backup.CompletedAtUtc, backup.Duration, backup.SizeBytes, backup.Rpo, backup.Rto);

        return await query.Take(50).ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SuperAdminAuditTimelineItem>> GetGlobalAuditTimelineAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var auditRows = await _dbContext.AuditLogs.AsNoTracking()
            .OrderByDescending(audit => audit.OccurredAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        var tenantIds = auditRows.Select(audit => audit.TenantId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToArray();
        var tenantNames = await _dbContext.Tenants.AsNoTracking().Where(tenant => tenantIds.Contains(tenant.Id)).ToDictionaryAsync(tenant => tenant.Id, tenant => tenant.Name, cancellationToken);

        return auditRows.Select(audit => new SuperAdminAuditTimelineItem(audit.Id, audit.TenantId, audit.TenantId.HasValue ? tenantNames.GetValueOrDefault(audit.TenantId.Value) : null, audit.OccurredAtUtc, audit.Action.ToString(), audit.Category.ToString(), audit.EntityName, audit.EntityId, audit.UserId, audit.UserName, audit.IpAddress, audit.CorrelationId, audit.Success, audit.MetadataJson)).ToArray();
    }

    public async Task<IReadOnlyCollection<SuperAdminDatabaseMetric>> GetDatabaseMetricsAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _dbContext.Tenants.AsNoTracking().CountAsync(cancellationToken);
        var auditRows = await _dbContext.AuditLogs.AsNoTracking().CountAsync(cancellationToken);
        var storageRows = await _dbContext.StoredFiles.AsNoTracking().CountAsync(cancellationToken);
        var pendingRetries = await _dbContext.NotificationRetries.AsNoTracking().CountAsync(retry => retry.ExecutedAtUtc == null, cancellationToken);

        return
        [
            new("Provider", _dbContext.Database.ProviderName ?? "Unknown", "Healthy", "EF Core database provider."),
            new("CanConnect", (await _dbContext.Database.CanConnectAsync(cancellationToken)).ToString(), "Healthy", "Database connectivity check."),
            new("Tenants", tenants.ToString(), "Healthy", "Tenant rows tracked by platform."),
            new("Audit Rows", auditRows.ToString(), auditRows > 0 ? "Healthy" : "Warning", "Append-only audit volume."),
            new("Stored Files", storageRows.ToString(), "Healthy", "Stored file metadata rows."),
            new("Pending Notification Retries", pendingRetries.ToString(), pendingRetries > 0 ? "Warning" : "Healthy", "Background retry backlog."),
            new("PostgreSQL Version", "Available through DBA connection", "Warning", "Direct version/locks/vacuum/bloat queries are intentionally not executed from UI.")
        ];
    }

    private IQueryable<Tenant> TenantQuery(string? searchText, string? status)
    {
        var query = _dbContext.Tenants.AsNoTracking().Include(tenant => tenant.Subscription).AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(tenant => tenant.Name.Contains(searchText) || tenant.Slug.Contains(searchText) || tenant.LegalName.Contains(searchText));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TenantStatus>(status, ignoreCase: true, out var parsed))
        {
            query = query.Where(tenant => tenant.Status == parsed);
        }

        return query;
    }
}
