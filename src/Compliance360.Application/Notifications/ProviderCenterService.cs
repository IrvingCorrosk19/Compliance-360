using Compliance360.Domain.Common;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public sealed class ProviderCenterService : IProviderCenterService
{
    private readonly IProviderCenterRepository _repository;
    private readonly IProviderSecretProtector _protector;
    private readonly IProviderEndpointResolver _resolver;
    private readonly INotificationProviderFactory _factory;
    private readonly IProviderConnectionTester _connectionTester;
    private readonly IClock _clock;
    private readonly INotificationAuditService? _audit;

    public ProviderCenterService(
        IProviderCenterRepository repository,
        IProviderSecretProtector protector,
        IProviderEndpointResolver resolver,
        INotificationProviderFactory factory,
        IProviderConnectionTester connectionTester,
        IClock clock,
        INotificationAuditService? audit = null)
    {
        _repository = repository;
        _protector = protector;
        _resolver = resolver;
        _factory = factory;
        _connectionTester = connectionTester;
        _clock = clock;
        _audit = audit;
    }

    public async Task<Result<IReadOnlyCollection<ProviderCenterSummary>>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var providers = await _repository.ListAsync(tenantId, cancellationToken);
        return Result<IReadOnlyCollection<ProviderCenterSummary>>.Success(providers.Select(Map).ToArray());
    }

    public async Task<Result<ProviderCenterSummary>> UpsertAsync(UpsertProviderCenterCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateAuthentication(command.Provider, command.Authentication, command.Settings);
            var protectedSettings = _protector.Protect(command.Settings);
            TenantNotificationProvider provider;
            if (command.ProviderId.HasValue)
            {
                provider = await _repository.GetAsync(command.TenantId, command.ProviderId.Value, cancellationToken)
                    ?? throw new InvalidOperationException("Provider configuration was not found for this tenant.");
                if (provider.Provider != command.Provider)
                {
                    throw new InvalidOperationException("Provider type cannot be changed; create a new configuration.");
                }
                provider.Update(command.Name, command.Priority, command.IsEnabled, command.Authentication,
                    command.FromAddress, command.FromName, protectedSettings, command.RateLimitPerMinute,
                    command.CircuitFailureThreshold, command.CircuitBreakSeconds, _clock.UtcNow);
            }
            else
            {
                provider = new TenantNotificationProvider(command.TenantId, command.Provider, command.Name,
                    command.Priority, command.IsEnabled, command.Authentication, command.FromAddress,
                    command.FromName, protectedSettings, command.RateLimitPerMinute,
                    command.CircuitFailureThreshold, command.CircuitBreakSeconds);
                await _repository.AddAsync(provider, cancellationToken);
            }

            await _repository.SaveChangesAsync(cancellationToken);
            if (_audit is not null)
            {
                await _audit.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(TenantNotificationProvider), provider.Id, AuditAction.NotificationProviderChanged, true, null, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
            }
            return Result<ProviderCenterSummary>.Success(Map(provider));
        }
        catch (Exception exception) when (exception is DomainException or InvalidOperationException or ArgumentException)
        {
            return Result<ProviderCenterSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<ProviderTestResult>> TestConnectionAsync(Guid tenantId, Guid providerId, CancellationToken cancellationToken = default)
    {
        var configured = await _repository.GetAsync(tenantId, providerId, cancellationToken);
        if (configured is null)
        {
            return Result<ProviderTestResult>.Failure("Provider configuration was not found.");
        }

        try
        {
            var endpoint = await _resolver.ResolveAsync(configured, cancellationToken);
            var health = await _connectionTester.TestAsync(endpoint, cancellationToken);
            return Result<ProviderTestResult>.Success(new ProviderTestResult(health.Healthy, configured.Provider, health.Message));
        }
        catch (Exception)
        {
            return Result<ProviderTestResult>.Success(new ProviderTestResult(false, configured.Provider, "Provider connection test failed."));
        }
    }

    public async Task<Result<ProviderTestResult>> SendSandboxAsync(SendProviderSandboxCommand command, CancellationToken cancellationToken = default)
    {
        var configured = await _repository.GetAsync(command.TenantId, command.ProviderId, cancellationToken);
        if (configured is null)
        {
            return Result<ProviderTestResult>.Failure("Provider configuration was not found.");
        }

        if (!configured.TryAcquire(_clock.UtcNow))
        {
            return Result<ProviderTestResult>.Success(new ProviderTestResult(false, configured.Provider, "Provider is disabled, rate limited, or its circuit is open."));
        }

        var endpoint = await _resolver.ResolveAsync(configured, cancellationToken);
        NotificationDispatchResult dispatch;
        try
        {
            dispatch = await _factory.GetProvider(configured.Provider).SendAsync(
                new NotificationDispatchRequest(command.TenantId, Guid.NewGuid(), NotificationChannel.Email,
                    command.Recipient, command.Subject ?? "Compliance 360 provider sandbox test",
                    command.Body ?? "<p>This is a sandbox delivery test from Compliance 360.</p>",
                    "This is a sandbox delivery test from Compliance 360."),
                endpoint,
                cancellationToken);
        }
        catch (Exception)
        {
            dispatch = NotificationDispatchResult.Failed("Provider sandbox send failed.", configured.Provider);
        }

        if (dispatch.Success)
        {
            configured.RecordSuccess(_clock.UtcNow);
        }
        else
        {
            configured.RecordFailure("sandbox_send_failed", _clock.UtcNow);
        }
        await _repository.SaveChangesAsync(cancellationToken);
        if (_audit is not null)
        {
            await _audit.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(TenantNotificationProvider), configured.Id, dispatch.Success ? AuditAction.NotificationSent : AuditAction.NotificationFailed, dispatch.Success, dispatch.FailureReason, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return Result<ProviderTestResult>.Success(new ProviderTestResult(
            dispatch.Success, configured.Provider,
            dispatch.Success ? "Sandbox message accepted by provider." : dispatch.FailureReason ?? "Sandbox send failed.",
            dispatch.ProviderMessageId));
    }

    private ProviderCenterSummary Map(TenantNotificationProvider provider)
    {
        var settings = _protector.Unprotect(provider.ProtectedSettings);
        return new ProviderCenterSummary(provider.Id, provider.TenantId, provider.Provider, provider.Name,
            provider.Priority, provider.IsEnabled, provider.Authentication, provider.FromAddress, provider.FromName,
            provider.RateLimitPerMinute, provider.CircuitFailureThreshold, provider.CircuitBreakSeconds,
            provider.ConsecutiveFailures, provider.CircuitOpenUntilUtc, provider.LastSucceededAtUtc,
            provider.LastFailedAtUtc, provider.LastFailureCode, Mask(settings.Secret),
            !string.IsNullOrWhiteSpace(settings.VaultReference));
    }

    private static string Mask(string? value) => string.IsNullOrEmpty(value) ? "not-set" : "********";

    private static void ValidateAuthentication(NotificationProvider provider, NotificationProviderAuthentication authentication, ProviderSecretSettings settings)
    {
        if (provider == NotificationProvider.AmazonSes && authentication != NotificationProviderAuthentication.AwsSignatureV4)
        {
            throw new InvalidOperationException("Amazon SES requires AWS Signature Version 4 credentials.");
        }
        if (provider is NotificationProvider.Microsoft365 or NotificationProvider.ExchangeOnline
            && authentication == NotificationProviderAuthentication.OAuth2ClientCredentials
            && (string.IsNullOrWhiteSpace(settings.ClientId) || string.IsNullOrWhiteSpace(settings.DirectoryTenantId)))
        {
            throw new InvalidOperationException("Microsoft Graph OAuth2 requires client and directory tenant identifiers.");
        }
        if (authentication == NotificationProviderAuthentication.VaultReference && string.IsNullOrWhiteSpace(settings.VaultReference))
        {
            throw new InvalidOperationException("A vault reference is required.");
        }
    }
}
