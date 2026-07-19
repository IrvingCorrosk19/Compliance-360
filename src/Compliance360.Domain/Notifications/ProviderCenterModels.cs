using Compliance360.Domain.Common;

namespace Compliance360.Domain.Notifications;

public enum NotificationProviderAuthentication
{
    None = 0,
    UsernamePassword = 1,
    ApiKey = 2,
    OAuth2ClientCredentials = 3,
    AwsSignatureV4 = 4,
    VaultReference = 5
}

public sealed class TenantNotificationProvider : TenantEntity
{
    private TenantNotificationProvider()
    {
        Name = string.Empty;
        FromAddress = string.Empty;
        ProtectedSettings = string.Empty;
    }

    public TenantNotificationProvider(
        Guid tenantId,
        NotificationProvider provider,
        string name,
        int priority,
        bool enabled,
        NotificationProviderAuthentication authentication,
        string fromAddress,
        string? fromName,
        string protectedSettings,
        int rateLimitPerMinute,
        int circuitFailureThreshold,
        int circuitBreakSeconds)
        : base(tenantId)
    {
        Provider = provider;
        Update(name, priority, enabled, authentication, fromAddress, fromName, protectedSettings,
            rateLimitPerMinute, circuitFailureThreshold, circuitBreakSeconds, DateTimeOffset.UtcNow);
    }

    public NotificationProvider Provider { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Priority { get; private set; }
    public bool IsEnabled { get; private set; }
    public NotificationProviderAuthentication Authentication { get; private set; }
    public string FromAddress { get; private set; } = string.Empty;
    public string? FromName { get; private set; }
    public string ProtectedSettings { get; private set; } = string.Empty;
    public int RateLimitPerMinute { get; private set; }
    public DateTimeOffset? RateWindowStartedAtUtc { get; private set; }
    public int RateWindowCount { get; private set; }
    public int CircuitFailureThreshold { get; private set; }
    public int CircuitBreakSeconds { get; private set; }
    public int ConsecutiveFailures { get; private set; }
    public DateTimeOffset? CircuitOpenUntilUtc { get; private set; }
    public DateTimeOffset? LastSucceededAtUtc { get; private set; }
    public DateTimeOffset? LastFailedAtUtc { get; private set; }
    public string? LastFailureCode { get; private set; }

    public void Update(
        string name,
        int priority,
        bool enabled,
        NotificationProviderAuthentication authentication,
        string fromAddress,
        string? fromName,
        string protectedSettings,
        int rateLimitPerMinute,
        int circuitFailureThreshold,
        int circuitBreakSeconds,
        DateTimeOffset nowUtc)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 120);
        Priority = Guard.AgainstOutOfRange(priority, nameof(priority), 1, 1000);
        IsEnabled = enabled;
        Authentication = authentication;
        FromAddress = Guard.AgainstNullOrWhiteSpace(fromAddress, nameof(fromAddress), 320);
        FromName = string.IsNullOrWhiteSpace(fromName) ? null : Guard.AgainstNullOrWhiteSpace(fromName, nameof(fromName), 160);
        ProtectedSettings = Guard.AgainstNullOrWhiteSpace(protectedSettings, nameof(protectedSettings), 16_000);
        RateLimitPerMinute = Guard.AgainstOutOfRange(rateLimitPerMinute, nameof(rateLimitPerMinute), 1, 100_000);
        CircuitFailureThreshold = Guard.AgainstOutOfRange(circuitFailureThreshold, nameof(circuitFailureThreshold), 1, 100);
        CircuitBreakSeconds = Guard.AgainstOutOfRange(circuitBreakSeconds, nameof(circuitBreakSeconds), 1, 86_400);
        MarkUpdated(nowUtc);
    }

    public bool TryAcquire(DateTimeOffset nowUtc)
    {
        if (!IsEnabled || CircuitOpenUntilUtc > nowUtc)
        {
            return false;
        }

        if (!RateWindowStartedAtUtc.HasValue || nowUtc - RateWindowStartedAtUtc.Value >= TimeSpan.FromMinutes(1))
        {
            RateWindowStartedAtUtc = nowUtc;
            RateWindowCount = 0;
        }

        if (RateWindowCount >= RateLimitPerMinute)
        {
            return false;
        }

        RateWindowCount++;
        MarkUpdated(nowUtc);
        return true;
    }

    public void RecordSuccess(DateTimeOffset nowUtc)
    {
        ConsecutiveFailures = 0;
        CircuitOpenUntilUtc = null;
        LastFailureCode = null;
        LastSucceededAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }

    public void RecordFailure(string failureCode, DateTimeOffset nowUtc)
    {
        ConsecutiveFailures++;
        LastFailedAtUtc = nowUtc;
        LastFailureCode = string.IsNullOrWhiteSpace(failureCode) ? "provider_failure" : failureCode[..Math.Min(failureCode.Length, 120)];
        if (ConsecutiveFailures >= CircuitFailureThreshold)
        {
            CircuitOpenUntilUtc = nowUtc.AddSeconds(CircuitBreakSeconds);
        }
        MarkUpdated(nowUtc);
    }
}
