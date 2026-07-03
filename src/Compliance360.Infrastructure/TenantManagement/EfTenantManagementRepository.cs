using Compliance360.Application.TenantManagement;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Identity;
using Compliance360.Domain.TenantManagement;
using Compliance360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Compliance360.Infrastructure.TenantManagement;

public sealed class EfTenantManagementRepository : ITenantManagementRepository
{
    private readonly Compliance360DbContext _dbContext;

    public EfTenantManagementRepository(Compliance360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return _dbContext.Tenants.AnyAsync(tenant => tenant.Slug == normalizedSlug, cancellationToken);
    }

    public Task<bool> SlugExistsAsync(Guid excludeTenantId, string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return _dbContext.Tenants.AnyAsync(tenant => tenant.Id != excludeTenantId && tenant.Slug == normalizedSlug, cancellationToken);
    }

    public Task<bool> TaxIdentifierExistsAsync(string taxIdentifier, CancellationToken cancellationToken = default)
    {
        var normalizedTaxIdentifier = taxIdentifier.Trim().ToUpperInvariant();
        return _dbContext.Tenants.AnyAsync(tenant => tenant.TaxIdentifier == normalizedTaxIdentifier, cancellationToken);
    }

    public Task<bool> TaxIdentifierExistsAsync(Guid excludeTenantId, string taxIdentifier, CancellationToken cancellationToken = default)
    {
        var normalizedTaxIdentifier = taxIdentifier.Trim().ToUpperInvariant();
        return _dbContext.Tenants.AnyAsync(tenant => tenant.Id != excludeTenantId && tenant.TaxIdentifier == normalizedTaxIdentifier, cancellationToken);
    }

    public Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tenants
            .Include(tenant => tenant.Settings)
            .Include(tenant => tenant.Branding)
            .Include(tenant => tenant.Subscription)
            .Include(tenant => tenant.Companies)
            .AsSplitQuery()
            .FirstOrDefaultAsync(tenant => tenant.Id == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Tenant>> SearchAsync(string? searchText, TenantStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await ApplyTenantSearch(_dbContext.Tenants.AsNoTracking(), searchText, status)
            .Include(tenant => tenant.Settings)
            .Include(tenant => tenant.Branding)
            .Include(tenant => tenant.Subscription)
            .OrderBy(tenant => tenant.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> CountAsync(string? searchText, TenantStatus? status, CancellationToken cancellationToken = default)
    {
        return ApplyTenantSearch(_dbContext.Tenants.AsNoTracking(), searchText, status).CountAsync(cancellationToken);
    }

    public async Task<TenantAdministrationMetrics> GetAdministrationMetricsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var users = await _dbContext.Users.AsNoTracking().Where(user => user.TenantId == tenantId).CountAsync(cancellationToken);
        var activeUsers = await _dbContext.Users.AsNoTracking().Where(user => user.TenantId == tenantId && user.Status.ToString() == "Active").CountAsync(cancellationToken);
        var roles = await _dbContext.Roles.AsNoTracking().Where(role => role.TenantId == tenantId).CountAsync(cancellationToken);
        var documents = await _dbContext.Documents.AsNoTracking().Where(document => document.TenantId == tenantId).CountAsync(cancellationToken);
        var suppliers = await _dbContext.Suppliers.AsNoTracking().Where(supplier => supplier.TenantId == tenantId).CountAsync(cancellationToken);
        var audits = await _dbContext.ManagedAudits.AsNoTracking().Where(audit => audit.TenantId == tenantId).CountAsync(cancellationToken);
        var capas = await _dbContext.Capas.AsNoTracking().Where(capa => capa.TenantId == tenantId).CountAsync(cancellationToken);
        var risks = await _dbContext.Risks.AsNoTracking().Where(risk => risk.TenantId == tenantId).CountAsync(cancellationToken);
        var indicators = await _dbContext.QualityIndicators.AsNoTracking().Where(indicator => indicator.TenantId == tenantId).CountAsync(cancellationToken);
        var notifications = await _dbContext.NotificationMessages.AsNoTracking().Where(notification => notification.TenantId == tenantId).CountAsync(cancellationToken);
        var storageProviders = await _dbContext.StorageProviderConfigurations.AsNoTracking().Where(provider => provider.TenantId == tenantId).CountAsync(cancellationToken);
        var notificationProviders = await _dbContext.NotificationProviderConfigurations.AsNoTracking().Where(provider => provider.TenantId == tenantId).CountAsync(cancellationToken);
        var storageBytes = await _dbContext.StoredFiles.AsNoTracking().Where(file => file.TenantId == tenantId).SumAsync(file => (long?)file.SizeBytes, cancellationToken) ?? 0;
        var lastLogin = await _dbContext.AuditLogs.AsNoTracking()
            .Where(audit => audit.TenantId == tenantId && audit.Action == AuditAction.LoginSucceeded)
            .OrderByDescending(audit => audit.OccurredAtUtc)
            .Select(audit => (DateTimeOffset?)audit.OccurredAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return new TenantAdministrationMetrics(
            users,
            activeUsers,
            roles,
            storageBytes,
            documents,
            suppliers,
            audits,
            capas,
            risks,
            indicators,
            notifications,
            storageProviders,
            notificationProviders,
            lastLogin,
            LastBackupAtUtc: null,
            Health: true);
    }

    public async Task<IReadOnlyCollection<TenantAuditTimelineItem>> GetAuditTimelineAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditLogs
            .AsNoTracking()
            .Where(audit => audit.TenantId == tenantId && audit.Category == AuditCategory.TenantManagement)
            .OrderByDescending(audit => audit.OccurredAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(audit => new TenantAuditTimelineItem(
                audit.Id,
                audit.OccurredAtUtc,
                audit.Action.ToString(),
                audit.EntityName,
                audit.EntityId,
                audit.UserId,
                audit.UserName,
                audit.IpAddress,
                audit.CorrelationId,
                audit.BeforeValuesJson,
                audit.AfterValuesJson,
                audit.MetadataJson))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TenantDomain>> ListDomainsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TenantDomains.Where(domain => domain.TenantId == tenantId).OrderBy(domain => domain.Kind).ThenBy(domain => domain.HostName).ToArrayAsync(cancellationToken);
    }

    public Task<TenantDomain?> GetDomainAsync(Guid tenantId, Guid domainId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TenantDomains.FirstOrDefaultAsync(domain => domain.TenantId == tenantId && domain.Id == domainId, cancellationToken);
    }

    public Task<bool> DomainExistsAsync(Guid tenantId, string hostName, Guid? excludeDomainId, CancellationToken cancellationToken = default)
    {
        var normalized = TenantValueObjects.Domain(hostName);
        return _dbContext.TenantDomains.AnyAsync(domain => domain.HostName == normalized && (!excludeDomainId.HasValue || domain.Id != excludeDomainId.Value), cancellationToken);
    }

    public async Task AddDomainAsync(TenantDomain domain, CancellationToken cancellationToken = default)
    {
        await _dbContext.TenantDomains.AddAsync(domain, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TenantSsoConfiguration>> ListSsoConfigurationsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TenantSsoConfigurations.Where(sso => sso.TenantId == tenantId).OrderBy(sso => sso.Provider).ThenBy(sso => sso.Name).ToArrayAsync(cancellationToken);
    }

    public Task<TenantSsoConfiguration?> GetSsoConfigurationAsync(Guid tenantId, Guid ssoId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TenantSsoConfigurations.FirstOrDefaultAsync(sso => sso.TenantId == tenantId && sso.Id == ssoId, cancellationToken);
    }

    public async Task AddSsoConfigurationAsync(TenantSsoConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await _dbContext.TenantSsoConfigurations.AddAsync(configuration, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TenantApiCredential>> ListApiCredentialsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TenantApiCredentials.Where(apiKey => apiKey.TenantId == tenantId).OrderBy(apiKey => apiKey.Name).ToArrayAsync(cancellationToken);
    }

    public Task<TenantApiCredential?> GetApiCredentialAsync(Guid tenantId, Guid apiCredentialId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TenantApiCredentials.FirstOrDefaultAsync(apiKey => apiKey.TenantId == tenantId && apiKey.Id == apiCredentialId, cancellationToken);
    }

    public async Task AddApiCredentialAsync(TenantApiCredential credential, CancellationToken cancellationToken = default)
    {
        await _dbContext.TenantApiCredentials.AddAsync(credential, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TenantWebhookEndpoint>> ListWebhooksAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TenantWebhookEndpoints.Where(webhook => webhook.TenantId == tenantId).OrderBy(webhook => webhook.Name).ToArrayAsync(cancellationToken);
    }

    public Task<TenantWebhookEndpoint?> GetWebhookAsync(Guid tenantId, Guid webhookId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TenantWebhookEndpoints.FirstOrDefaultAsync(webhook => webhook.TenantId == tenantId && webhook.Id == webhookId, cancellationToken);
    }

    public async Task AddWebhookAsync(TenantWebhookEndpoint webhook, CancellationToken cancellationToken = default)
    {
        await _dbContext.TenantWebhookEndpoints.AddAsync(webhook, cancellationToken);
    }

    public async Task AddWebhookDeliveryLogAsync(TenantWebhookDeliveryLog deliveryLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.TenantWebhookDeliveryLogs.AddAsync(deliveryLog, cancellationToken);
    }

    public Task<TenantLicense?> GetLicenseAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TenantLicenses.FirstOrDefaultAsync(license => license.TenantId == tenantId, cancellationToken);
    }

    public async Task AddLicenseAsync(TenantLicense license, CancellationToken cancellationToken = default)
    {
        await _dbContext.TenantLicenses.AddAsync(license, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TenantHealthSignal>> ListHealthSignalsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TenantHealthSignals.AsNoTracking().Where(signal => signal.TenantId == tenantId).OrderBy(signal => signal.Component).ToArrayAsync(cancellationToken);
    }

    public Task<TenantHealthSignal?> GetHealthSignalAsync(Guid tenantId, string component, CancellationToken cancellationToken = default)
    {
        return _dbContext.TenantHealthSignals.FirstOrDefaultAsync(signal => signal.TenantId == tenantId && signal.Component == component, cancellationToken);
    }

    public async Task AddHealthSignalAsync(TenantHealthSignal signal, CancellationToken cancellationToken = default)
    {
        await _dbContext.TenantHealthSignals.AddAsync(signal, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TenantBackupRecord>> ListBackupRecordsAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TenantBackupRecords.AsNoTracking()
            .Where(backup => backup.TenantId == tenantId)
            .OrderByDescending(backup => backup.CompletedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddBackupRecordAsync(TenantBackupRecord backup, CancellationToken cancellationToken = default)
    {
        await _dbContext.TenantBackupRecords.AddAsync(backup, cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> ListUsersAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Include(user => user.Roles)
            .Include(user => user.Sessions)
            .Where(user => user.TenantId == tenantId)
            .OrderBy(user => user.Email)
            .AsSplitQuery()
            .ToArrayAsync(cancellationToken);
    }

    public Task<User?> GetUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .Include(user => user.Roles)
            .Include(user => user.Sessions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(user => user.TenantId == tenantId && user.Id == userId, cancellationToken);
    }

    public Task<Role?> GetRoleAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Roles.FirstOrDefaultAsync(role => role.TenantId == tenantId && role.Id == roleId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> ListRolesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles.AsNoTracking().Where(role => role.TenantId == tenantId).OrderBy(role => role.Name).ToArrayAsync(cancellationToken);
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserSessionSummary>> ListUserSessionsAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _dbContext.UserSessions.AsNoTracking()
            .Where(session => session.TenantId == tenantId && session.UserId == userId)
            .OrderByDescending(session => session.CreatedAt)
            .Select(session => new UserSessionSummary(session.Id, session.TenantId, session.UserId, session.CreatedAt, session.ExpiresAtUtc, session.RevokedAtUtc, session.RevokedAtUtc == null && session.ExpiresAtUtc > now))
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }

    public async Task AddCompanyAsync(Company company, CancellationToken cancellationToken = default)
    {
        await _dbContext.Companies.AddAsync(company, cancellationToken);
    }

    public async Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    private static IQueryable<Tenant> ApplyTenantSearch(IQueryable<Tenant> query, string? searchText, TenantStatus? status)
    {
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalized = searchText.Trim().ToLowerInvariant();
            query = query.Where(tenant =>
                tenant.Name.ToLower().Contains(normalized) ||
                tenant.Slug.ToLower().Contains(normalized) ||
                tenant.LegalName.ToLower().Contains(normalized) ||
                tenant.TaxIdentifier.ToLower().Contains(normalized));
        }

        if (status.HasValue)
        {
            query = query.Where(tenant => tenant.Status == status.Value);
        }

        return query;
    }
}
