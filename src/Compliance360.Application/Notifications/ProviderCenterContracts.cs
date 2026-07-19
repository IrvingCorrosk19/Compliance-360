using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public interface IProviderCenterService
{
    Task<Result<IReadOnlyCollection<ProviderCenterSummary>>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<ProviderCenterSummary>> UpsertAsync(UpsertProviderCenterCommand command, CancellationToken cancellationToken = default);
    Task<Result<ProviderTestResult>> TestConnectionAsync(Guid tenantId, Guid providerId, CancellationToken cancellationToken = default);
    Task<Result<ProviderTestResult>> SendSandboxAsync(SendProviderSandboxCommand command, CancellationToken cancellationToken = default);
}

public interface IProviderCenterRepository
{
    Task<IReadOnlyCollection<TenantNotificationProvider>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantNotificationProvider?> GetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TenantNotificationProvider provider, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IProviderSecretProtector
{
    string Protect(ProviderSecretSettings settings);
    ProviderSecretSettings Unprotect(string protectedSettings);
}

public interface IProviderEndpointResolver
{
    Task<NotificationProviderEndpoint> ResolveAsync(TenantNotificationProvider provider, CancellationToken cancellationToken = default);
}

public interface IProviderVaultSecretResolver
{
    Task<string?> ResolveAsync(string reference, CancellationToken cancellationToken = default);
}

public interface IProviderConnectionTester
{
    Task<NotificationProviderHealth> TestAsync(NotificationProviderEndpoint endpoint, CancellationToken cancellationToken = default);
}

public sealed record ProviderSecretSettings(
    string? Host = null,
    int? Port = null,
    string? Username = null,
    string? Secret = null,
    string? BaseUrl = null,
    string? Domain = null,
    bool UseSsl = true,
    string? ClientId = null,
    string? DirectoryTenantId = null,
    string? Region = null,
    string? VaultReference = null);

public sealed record UpsertProviderCenterCommand(
    Guid TenantId,
    Guid RequestedByUserId,
    Guid? ProviderId,
    NotificationProvider Provider,
    string Name,
    int Priority,
    bool IsEnabled,
    NotificationProviderAuthentication Authentication,
    string FromAddress,
    string? FromName,
    ProviderSecretSettings Settings,
    int RateLimitPerMinute = 60,
    int CircuitFailureThreshold = 5,
    int CircuitBreakSeconds = 300);

public sealed record SendProviderSandboxCommand(
    Guid TenantId,
    Guid RequestedByUserId,
    Guid ProviderId,
    string Recipient,
    string? Subject = null,
    string? Body = null);

public sealed record ProviderCenterSummary(
    Guid Id,
    Guid TenantId,
    NotificationProvider Provider,
    string Name,
    int Priority,
    bool IsEnabled,
    NotificationProviderAuthentication Authentication,
    string FromAddress,
    string? FromName,
    int RateLimitPerMinute,
    int CircuitFailureThreshold,
    int CircuitBreakSeconds,
    int ConsecutiveFailures,
    DateTimeOffset? CircuitOpenUntilUtc,
    DateTimeOffset? LastSucceededAtUtc,
    DateTimeOffset? LastFailedAtUtc,
    string? LastFailureCode,
    string SecretMask,
    bool UsesVaultReference);

public sealed record ProviderTestResult(bool Success, NotificationProvider Provider, string Message, string? ProviderMessageId = null);
