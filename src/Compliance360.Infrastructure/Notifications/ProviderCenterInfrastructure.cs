using System.Text.Json;
using System.Net.Sockets;
using Compliance360.Application.Notifications;
using Compliance360.Domain.Notifications;
using Compliance360.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Compliance360.Infrastructure.Notifications;

public sealed class EfProviderCenterRepository : IProviderCenterRepository
{
    private readonly Compliance360DbContext _db;

    public EfProviderCenterRepository(Compliance360DbContext db) => _db = db;

    public async Task<IReadOnlyCollection<TenantNotificationProvider>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _db.TenantNotificationProviders.Where(x => x.TenantId == tenantId).OrderBy(x => x.Priority).ThenBy(x => x.Name).ToArrayAsync(cancellationToken);

    public Task<TenantNotificationProvider?> GetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default) =>
        _db.TenantNotificationProviders.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken);

    public async Task AddAsync(TenantNotificationProvider provider, CancellationToken cancellationToken = default) =>
        await _db.TenantNotificationProviders.AddAsync(provider, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}

public sealed class DataProtectionProviderSecretProtector : IProviderSecretProtector
{
    private readonly IDataProtector _protector;

    public DataProtectionProviderSecretProtector(IDataProtectionProvider provider) =>
        _protector = provider.CreateProtector("Compliance360.AlertCenter.ProviderSecrets.v1");

    public string Protect(ProviderSecretSettings settings) =>
        _protector.Protect(JsonSerializer.Serialize(settings));

    public ProviderSecretSettings Unprotect(string protectedSettings) =>
        JsonSerializer.Deserialize<ProviderSecretSettings>(_protector.Unprotect(protectedSettings))
        ?? throw new InvalidOperationException("Provider settings could not be decrypted.");
}

public sealed class ConfigurationProviderVaultSecretResolver : IProviderVaultSecretResolver
{
    private readonly IConfiguration _configuration;

    public ConfigurationProviderVaultSecretResolver(IConfiguration configuration) => _configuration = configuration;

    public Task<string?> ResolveAsync(string reference, CancellationToken cancellationToken = default)
    {
        if (!reference.StartsWith("config://", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("No vault adapter is registered for this reference scheme.");
        }

        var key = reference["config://".Length..].Replace('/', ':');
        return Task.FromResult<string?>(_configuration[key]);
    }
}

public sealed class ProviderEndpointResolver : IProviderEndpointResolver
{
    private readonly IProviderSecretProtector _protector;
    private readonly IProviderVaultSecretResolver _vault;

    public ProviderEndpointResolver(IProviderSecretProtector protector, IProviderVaultSecretResolver vault)
    {
        _protector = protector;
        _vault = vault;
    }

    public async Task<NotificationProviderEndpoint> ResolveAsync(TenantNotificationProvider provider, CancellationToken cancellationToken = default)
    {
        var settings = _protector.Unprotect(provider.ProtectedSettings);
        var secret = settings.Secret;
        if (!string.IsNullOrWhiteSpace(settings.VaultReference))
        {
            secret = await _vault.ResolveAsync(settings.VaultReference, cancellationToken)
                ?? throw new InvalidOperationException("The configured vault reference did not resolve a secret.");
        }

        return new NotificationProviderEndpoint(provider.Provider, settings.Host, settings.Port, settings.Username,
            secret, provider.FromAddress, provider.FromName, settings.BaseUrl, settings.Domain, settings.UseSsl,
            provider.Authentication, settings.ClientId, settings.DirectoryTenantId, settings.Region);
    }
}

public sealed class ProviderConnectionTester : IProviderConnectionTester
{
    private readonly INotificationProviderFactory _factory;

    public ProviderConnectionTester(INotificationProviderFactory factory) => _factory = factory;

    public async Task<NotificationProviderHealth> TestAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        if (endpoint.Provider is not (NotificationProvider.Smtp or NotificationProvider.GmailSmtp or NotificationProvider.ExchangeOnline))
        {
            return await _factory.GetProvider(endpoint.Provider).CheckHealthAsync(endpoint, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(endpoint.Host) || !endpoint.Port.HasValue)
        {
            return new NotificationProviderHealth(endpoint.Provider, false, "SMTP host and port are required.");
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));
            using var client = new TcpClient();
            await client.ConnectAsync(endpoint.Host, endpoint.Port.Value, timeout.Token);
            return new NotificationProviderHealth(endpoint.Provider, true, "SMTP endpoint accepted a TCP connection.");
        }
        catch (Exception exception) when (exception is SocketException or OperationCanceledException)
        {
            return new NotificationProviderHealth(endpoint.Provider, false, "SMTP endpoint connection failed.");
        }
    }
}
