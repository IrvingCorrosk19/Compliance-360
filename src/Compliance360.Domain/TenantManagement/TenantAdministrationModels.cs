using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Compliance360.Domain.Common;

namespace Compliance360.Domain.TenantManagement;

public enum TenantDomainKind
{
    Default = 0,
    PrimaryCustom = 1,
    SecondaryCustom = 2,
    Subdomain = 3,
    Alias = 4
}

public enum TenantDomainStatus
{
    PendingVerification = 0,
    Verified = 1,
    DnsFailed = 2,
    CertificateFailed = 3,
    Disabled = 4
}

public enum TenantCertificateStatus
{
    NotRequested = 0,
    Pending = 1,
    Issued = 2,
    Expired = 3,
    Failed = 4
}

public enum TenantSsoProviderType
{
    Oidc = 0,
    OAuth2 = 1,
    Saml = 2,
    Ldap = 3,
    ActiveDirectory = 4
}

public enum TenantIntegrationStatus
{
    Draft = 0,
    Enabled = 1,
    Disabled = 2,
    Failed = 3,
    Revoked = 4
}

public enum TenantSecretStatus
{
    Active = 0,
    Expired = 1,
    Revoked = 2,
    Rotated = 3
}

public enum TenantWebhookDeliveryStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2,
    Retrying = 3,
    DeadLetter = 4
}

public enum TenantLicenseStatus
{
    Draft = 0,
    Trial = 1,
    Active = 2,
    PastDue = 3,
    Suspended = 4,
    Cancelled = 5,
    Expired = 6
}

public enum TenantHealthStatus
{
    Healthy = 0,
    Degraded = 1,
    Unhealthy = 2,
    Unknown = 3
}

public sealed record TenantValidatedValue(string Value)
{
    public override string ToString() => Value;
}

public static class TenantValueObjects
{
    private static readonly Regex EmailRegex = new("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex DomainRegex = new("^(?=.{1,253}$)([a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\\.)+[a-z]{2,63}$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex SlugRegex = new("^[a-z0-9][a-z0-9-]{1,78}[a-z0-9]$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex ColorRegex = new("^#(?:[0-9a-fA-F]{3}){1,2}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex CurrencyRegex = new("^[A-Z]{3}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex CountryRegex = new("^[A-Z]{2}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string Email(string? value, string parameterName = "email")
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, parameterName, 320).ToLowerInvariant();
        if (!EmailRegex.IsMatch(normalized))
        {
            throw new DomainException($"{parameterName} must be a valid email address.");
        }

        return normalized;
    }

    public static string OptionalEmail(string? value, string parameterName = "email")
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : Email(value, parameterName);
    }

    public static string Url(string? value, string parameterName = "url")
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, parameterName, 500);
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http"))
        {
            throw new DomainException($"{parameterName} must be a valid absolute URL.");
        }

        return uri.ToString();
    }

    public static string OptionalUrl(string? value, string parameterName = "url")
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : Url(value, parameterName);
    }

    public static string Domain(string? value, string parameterName = "domain")
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, parameterName, 253).TrimEnd('.').ToLowerInvariant();
        if (!DomainRegex.IsMatch(normalized))
        {
            throw new DomainException($"{parameterName} must be a valid DNS domain.");
        }

        return normalized;
    }

    public static string Slug(string? value, string parameterName = "slug")
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, parameterName, 80).ToLowerInvariant();
        if (!SlugRegex.IsMatch(normalized))
        {
            throw new DomainException($"{parameterName} must be a lowercase URL-safe slug.");
        }

        return normalized;
    }

    public static string Color(string? value, string parameterName = "color")
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, parameterName, 7);
        if (!ColorRegex.IsMatch(normalized))
        {
            throw new DomainException($"{parameterName} must be a valid hex color.");
        }

        return normalized.ToUpperInvariant();
    }

    public static string Currency(string? value, string parameterName = "currency")
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, parameterName, 3).ToUpperInvariant();
        if (!CurrencyRegex.IsMatch(normalized))
        {
            throw new DomainException($"{parameterName} must be an ISO-4217 currency code.");
        }

        return normalized;
    }

    public static string Country(string? value, string parameterName = "country")
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, parameterName, 2).ToUpperInvariant();
        if (!CountryRegex.IsMatch(normalized))
        {
            throw new DomainException($"{parameterName} must be an ISO-3166 alpha-2 country code.");
        }

        return normalized;
    }

    public static string TimeZone(string? value, string parameterName = "timeZone")
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, parameterName, 80);
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(normalized, out _))
        {
            throw new DomainException($"{parameterName} must be a valid time zone id.");
        }

        return normalized;
    }

    public static string CidrList(string? value, string parameterName = "cidr")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();
        foreach (var item in normalized.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = item.Split('/');
            if (parts.Length != 2 || !System.Net.IPAddress.TryParse(parts[0], out _) || !int.TryParse(parts[1], out var mask) || mask is < 0 or > 128)
            {
                throw new DomainException($"{parameterName} must contain valid CIDR ranges.");
            }
        }

        return normalized;
    }

    public static string Phone(string? value, string parameterName = "phone")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = Guard.AgainstNullOrWhiteSpace(value, parameterName, 40);
        if (normalized.Count(char.IsDigit) < 7)
        {
            throw new DomainException($"{parameterName} must contain at least 7 digits.");
        }

        return normalized;
    }

    public static string TaxId(string? value, string parameterName = "taxIdentifier")
    {
        return Guard.AgainstNullOrWhiteSpace(value, parameterName, 80).ToUpperInvariant();
    }
}

public sealed class TenantDomain : TenantEntity
{
    private TenantDomain()
    {
        HostName = string.Empty;
        VerificationToken = string.Empty;
        DnsStatus = string.Empty;
    }

    public TenantDomain(Guid tenantId, string hostName, TenantDomainKind kind, bool isDefault, Guid requestedByUserId)
        : base(tenantId)
    {
        HostName = TenantValueObjects.Domain(hostName, nameof(hostName));
        Kind = kind;
        IsDefault = isDefault;
        Status = TenantDomainStatus.PendingVerification;
        CertificateStatus = TenantCertificateStatus.NotRequested;
        VerificationToken = CreateToken();
        RequestedByUserId = Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
        DnsStatus = "Pending";
    }

    public string HostName { get; private set; }

    public TenantDomainKind Kind { get; private set; }

    public TenantDomainStatus Status { get; private set; }

    public bool IsDefault { get; private set; }

    public string VerificationToken { get; private set; }

    public string DnsStatus { get; private set; }

    public TenantCertificateStatus CertificateStatus { get; private set; }

    public bool HttpsEnabled { get; private set; }

    public string? RedirectToHostName { get; private set; }

    public DateTimeOffset? VerifiedAtUtc { get; private set; }

    public DateTimeOffset? LastCheckedAtUtc { get; private set; }

    public Guid RequestedByUserId { get; private set; }

    public void Configure(string hostName, TenantDomainKind kind, bool isDefault, string? redirectToHostName)
    {
        HostName = TenantValueObjects.Domain(hostName, nameof(hostName));
        Kind = kind;
        IsDefault = isDefault;
        RedirectToHostName = string.IsNullOrWhiteSpace(redirectToHostName) ? null : TenantValueObjects.Domain(redirectToHostName, nameof(redirectToHostName));
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void MarkDnsCheck(bool verified, string status, DateTimeOffset checkedAtUtc)
    {
        DnsStatus = Guard.AgainstNullOrWhiteSpace(status, nameof(status), 500);
        LastCheckedAtUtc = checkedAtUtc;
        Status = verified ? TenantDomainStatus.Verified : TenantDomainStatus.DnsFailed;
        VerifiedAtUtc = verified ? checkedAtUtc : VerifiedAtUtc;
        MarkUpdated(checkedAtUtc);
    }

    public void ConfigureCertificate(TenantCertificateStatus status, bool httpsEnabled)
    {
        CertificateStatus = status;
        HttpsEnabled = httpsEnabled;
        if (Status == TenantDomainStatus.Verified && status == TenantCertificateStatus.Issued && httpsEnabled)
        {
            DnsStatus = "Verified with HTTPS";
        }

        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void Disable()
    {
        Status = TenantDomainStatus.Disabled;
        IsDefault = false;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    private static string CreateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
    }
}

public sealed class TenantSsoConfiguration : TenantEntity
{
    private TenantSsoConfiguration()
    {
        Name = string.Empty;
        Authority = string.Empty;
        MetadataUrl = string.Empty;
        ClientId = string.Empty;
        ClaimsMappingJson = string.Empty;
        RoleMappingJson = string.Empty;
        HealthMessage = string.Empty;
    }

    public TenantSsoConfiguration(Guid tenantId, TenantSsoProviderType provider, string name, string authority, string? metadataUrl, string clientId, string? secretReference, Guid requestedByUserId)
        : base(tenantId)
    {
        Provider = provider;
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 120);
        Authority = Guard.AgainstNullOrWhiteSpace(authority, nameof(authority), 500);
        MetadataUrl = string.IsNullOrWhiteSpace(metadataUrl) ? string.Empty : TenantValueObjects.Url(metadataUrl, nameof(metadataUrl));
        ClientId = Guard.AgainstNullOrWhiteSpace(clientId, nameof(clientId), 250);
        SecretReference = string.IsNullOrWhiteSpace(secretReference) ? null : Guard.AgainstNullOrWhiteSpace(secretReference, nameof(secretReference), 500);
        RequestedByUserId = Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
        ClaimsMappingJson = "{}";
        RoleMappingJson = "{}";
        Status = TenantIntegrationStatus.Draft;
        HealthStatus = TenantHealthStatus.Unknown;
        HealthMessage = "Not tested";
    }

    public TenantSsoProviderType Provider { get; private set; }

    public string Name { get; private set; }

    public string Authority { get; private set; }

    public string MetadataUrl { get; private set; }

    public string ClientId { get; private set; }

    public string? SecretReference { get; private set; }

    public string? CertificateThumbprint { get; private set; }

    public string ClaimsMappingJson { get; private set; }

    public string RoleMappingJson { get; private set; }

    public bool JitProvisioningEnabled { get; private set; }

    public bool ScimEnabled { get; private set; }

    public TenantIntegrationStatus Status { get; private set; }

    public TenantHealthStatus HealthStatus { get; private set; }

    public string HealthMessage { get; private set; }

    public DateTimeOffset? LastTestedAtUtc { get; private set; }

    public Guid RequestedByUserId { get; private set; }

    public void ConfigureMappings(string claimsMappingJson, string roleMappingJson, bool jitProvisioningEnabled, bool scimEnabled)
    {
        ClaimsMappingJson = Guard.AgainstNullOrWhiteSpace(claimsMappingJson, nameof(claimsMappingJson), 8_000);
        RoleMappingJson = Guard.AgainstNullOrWhiteSpace(roleMappingJson, nameof(roleMappingJson), 8_000);
        JitProvisioningEnabled = jitProvisioningEnabled;
        ScimEnabled = scimEnabled;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void RotateCertificate(string certificateThumbprint)
    {
        CertificateThumbprint = Guard.AgainstNullOrWhiteSpace(certificateThumbprint, nameof(certificateThumbprint), 250);
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void SetEnabled(bool enabled)
    {
        Status = enabled ? TenantIntegrationStatus.Enabled : TenantIntegrationStatus.Disabled;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void RecordHealth(TenantHealthStatus status, string message, DateTimeOffset testedAtUtc)
    {
        HealthStatus = status;
        HealthMessage = Guard.AgainstNullOrWhiteSpace(message, nameof(message), 1_000);
        LastTestedAtUtc = testedAtUtc;
        Status = status == TenantHealthStatus.Unhealthy ? TenantIntegrationStatus.Failed : Status;
        MarkUpdated(testedAtUtc);
    }
}

public sealed class TenantApiCredential : TenantEntity
{
    private TenantApiCredential()
    {
        Name = string.Empty;
        KeyPrefix = string.Empty;
        KeyHash = string.Empty;
        Scopes = string.Empty;
    }

    public TenantApiCredential(Guid tenantId, string name, string keyPrefix, string keyHash, string scopes, DateTimeOffset? expiresAtUtc, Guid requestedByUserId)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 160);
        KeyPrefix = Guard.AgainstNullOrWhiteSpace(keyPrefix, nameof(keyPrefix), 32);
        KeyHash = Guard.AgainstNullOrWhiteSpace(keyHash, nameof(keyHash), 512);
        Scopes = Guard.AgainstNullOrWhiteSpace(scopes, nameof(scopes), 2_000);
        ExpiresAtUtc = expiresAtUtc;
        RequestedByUserId = Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
        Status = TenantSecretStatus.Active;
    }

    public string Name { get; private set; }

    public string KeyPrefix { get; private set; }

    public string KeyHash { get; private set; }

    public string Scopes { get; private set; }

    public DateTimeOffset? ExpiresAtUtc { get; private set; }

    public DateTimeOffset? LastUsedAtUtc { get; private set; }

    public TenantSecretStatus Status { get; private set; }

    public Guid RequestedByUserId { get; private set; }

    public void Rotate(string keyPrefix, string keyHash, DateTimeOffset? expiresAtUtc)
    {
        KeyPrefix = Guard.AgainstNullOrWhiteSpace(keyPrefix, nameof(keyPrefix), 32);
        KeyHash = Guard.AgainstNullOrWhiteSpace(keyHash, nameof(keyHash), 512);
        ExpiresAtUtc = expiresAtUtc;
        Status = TenantSecretStatus.Rotated;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void MarkUsed(DateTimeOffset usedAtUtc)
    {
        LastUsedAtUtc = usedAtUtc;
        MarkUpdated(usedAtUtc);
    }

    public void Revoke()
    {
        Status = TenantSecretStatus.Revoked;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public static string HashSecret(string secret)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(secret, nameof(secret), 512);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized))).ToLowerInvariant();
    }
}

public sealed class TenantWebhookEndpoint : TenantEntity
{
    private TenantWebhookEndpoint()
    {
        Name = string.Empty;
        Url = string.Empty;
        Events = string.Empty;
        SecretHash = string.Empty;
        SigningAlgorithm = string.Empty;
    }

    public TenantWebhookEndpoint(Guid tenantId, string name, string url, string events, string secretHash, Guid requestedByUserId)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 160);
        Url = TenantValueObjects.Url(url, nameof(url));
        Events = Guard.AgainstNullOrWhiteSpace(events, nameof(events), 2_000);
        SecretHash = Guard.AgainstNullOrWhiteSpace(secretHash, nameof(secretHash), 512);
        SigningAlgorithm = "HMAC-SHA256";
        Status = TenantIntegrationStatus.Enabled;
        RequestedByUserId = Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
    }

    public string Name { get; private set; }

    public string Url { get; private set; }

    public string Events { get; private set; }

    public string SecretHash { get; private set; }

    public string SigningAlgorithm { get; private set; }

    public TenantIntegrationStatus Status { get; private set; }

    public int MaxRetries { get; private set; } = 5;

    public DateTimeOffset? LastDeliveryAtUtc { get; private set; }

    public TenantWebhookDeliveryStatus LastDeliveryStatus { get; private set; }

    public string? LastDeliveryMessage { get; private set; }

    public Guid RequestedByUserId { get; private set; }

    public void Configure(string name, string url, string events, int maxRetries)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 160);
        Url = TenantValueObjects.Url(url, nameof(url));
        Events = Guard.AgainstNullOrWhiteSpace(events, nameof(events), 2_000);
        MaxRetries = Guard.AgainstOutOfRange(maxRetries, nameof(maxRetries), 0, 25);
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void RotateSecret(string secretHash)
    {
        SecretHash = Guard.AgainstNullOrWhiteSpace(secretHash, nameof(secretHash), 512);
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void SetEnabled(bool enabled)
    {
        Status = enabled ? TenantIntegrationStatus.Enabled : TenantIntegrationStatus.Disabled;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public TenantWebhookDeliveryLog RecordDelivery(TenantWebhookDeliveryStatus status, string message, DateTimeOffset occurredAtUtc, int attempt)
    {
        LastDeliveryAtUtc = occurredAtUtc;
        LastDeliveryStatus = status;
        LastDeliveryMessage = Guard.AgainstNullOrWhiteSpace(message, nameof(message), 1_000);
        MarkUpdated(occurredAtUtc);
        return new TenantWebhookDeliveryLog(TenantId, Id, status, message, occurredAtUtc, attempt);
    }
}

public sealed class TenantWebhookDeliveryLog : TenantEntity
{
    private TenantWebhookDeliveryLog()
    {
        Message = string.Empty;
    }

    public TenantWebhookDeliveryLog(Guid tenantId, Guid webhookId, TenantWebhookDeliveryStatus status, string message, DateTimeOffset occurredAtUtc, int attempt)
        : base(tenantId)
    {
        WebhookId = Guard.AgainstEmpty(webhookId, nameof(webhookId));
        Status = status;
        Message = Guard.AgainstNullOrWhiteSpace(message, nameof(message), 1_000);
        OccurredAtUtc = occurredAtUtc;
        Attempt = Guard.AgainstOutOfRange(attempt, nameof(attempt), 1, 100);
    }

    public Guid WebhookId { get; private set; }

    public TenantWebhookDeliveryStatus Status { get; private set; }

    public string Message { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public int Attempt { get; private set; }
}

public sealed class TenantLicense : TenantEntity
{
    private TenantLicense()
    {
        LicenseNumber = string.Empty;
        FeaturesJson = string.Empty;
        ModulesJson = string.Empty;
        EntitlementsJson = string.Empty;
    }

    public TenantLicense(Guid tenantId, string licenseNumber, string featuresJson, string modulesJson, DateOnly periodStart, DateOnly periodEnd, DateOnly renewalDate, Guid requestedByUserId)
        : base(tenantId)
    {
        LicenseNumber = Guard.AgainstNullOrWhiteSpace(licenseNumber, nameof(licenseNumber), 120).ToUpperInvariant();
        FeaturesJson = Guard.AgainstNullOrWhiteSpace(featuresJson, nameof(featuresJson), 8_000);
        ModulesJson = Guard.AgainstNullOrWhiteSpace(modulesJson, nameof(modulesJson), 8_000);
        EntitlementsJson = "{}";
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        RenewalDate = renewalDate;
        Status = TenantLicenseStatus.Active;
        RequestedByUserId = Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
    }

    public string LicenseNumber { get; private set; }

    public TenantLicenseStatus Status { get; private set; }

    public string FeaturesJson { get; private set; }

    public string ModulesJson { get; private set; }

    public string EntitlementsJson { get; private set; }

    public DateOnly PeriodStart { get; private set; }

    public DateOnly PeriodEnd { get; private set; }

    public DateOnly RenewalDate { get; private set; }

    public int SeatsPurchased { get; private set; }

    public int SeatsUsed { get; private set; }

    public int StorageGbPurchased { get; private set; }

    public long StorageBytesUsed { get; private set; }

    public Guid RequestedByUserId { get; private set; }

    public void Configure(string featuresJson, string modulesJson, string entitlementsJson, TenantLicenseStatus status, DateOnly periodStart, DateOnly periodEnd, DateOnly renewalDate)
    {
        FeaturesJson = Guard.AgainstNullOrWhiteSpace(featuresJson, nameof(featuresJson), 8_000);
        ModulesJson = Guard.AgainstNullOrWhiteSpace(modulesJson, nameof(modulesJson), 8_000);
        EntitlementsJson = Guard.AgainstNullOrWhiteSpace(entitlementsJson, nameof(entitlementsJson), 8_000);
        Status = status;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        RenewalDate = renewalDate;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void UpdateConsumption(int seatsPurchased, int seatsUsed, int storageGbPurchased, long storageBytesUsed)
    {
        SeatsPurchased = Guard.AgainstOutOfRange(seatsPurchased, nameof(seatsPurchased), 0, 1_000_000);
        SeatsUsed = Guard.AgainstOutOfRange(seatsUsed, nameof(seatsUsed), 0, 1_000_000);
        StorageGbPurchased = Guard.AgainstOutOfRange(storageGbPurchased, nameof(storageGbPurchased), 0, 10_000_000);
        StorageBytesUsed = storageBytesUsed >= 0 ? storageBytesUsed : throw new DomainException("storageBytesUsed cannot be negative.");
        MarkUpdated(DateTimeOffset.UtcNow);
    }
}

public sealed class TenantHealthSignal : TenantEntity
{
    private TenantHealthSignal()
    {
        Component = string.Empty;
        Message = string.Empty;
    }

    public TenantHealthSignal(Guid tenantId, string component, TenantHealthStatus status, string message, DateTimeOffset checkedAtUtc)
        : base(tenantId)
    {
        Component = Guard.AgainstNullOrWhiteSpace(component, nameof(component), 120);
        Status = status;
        Message = Guard.AgainstNullOrWhiteSpace(message, nameof(message), 1_000);
        CheckedAtUtc = checkedAtUtc;
    }

    public string Component { get; private set; }

    public TenantHealthStatus Status { get; private set; }

    public string Message { get; private set; }

    public DateTimeOffset CheckedAtUtc { get; private set; }

    public TimeSpan? Duration { get; private set; }

    public void Update(TenantHealthStatus status, string message, DateTimeOffset checkedAtUtc, TimeSpan? duration)
    {
        Status = status;
        Message = Guard.AgainstNullOrWhiteSpace(message, nameof(message), 1_000);
        CheckedAtUtc = checkedAtUtc;
        Duration = duration;
        MarkUpdated(checkedAtUtc);
    }
}

public sealed class TenantBackupRecord : TenantEntity
{
    private TenantBackupRecord()
    {
        BackupKind = string.Empty;
        Result = string.Empty;
        Message = string.Empty;
    }

    public TenantBackupRecord(Guid tenantId, string backupKind, string result, DateTimeOffset startedAtUtc, DateTimeOffset completedAtUtc, long sizeBytes, string message, TimeSpan rpo, TimeSpan rto)
        : base(tenantId)
    {
        BackupKind = Guard.AgainstNullOrWhiteSpace(backupKind, nameof(backupKind), 80);
        Result = Guard.AgainstNullOrWhiteSpace(result, nameof(result), 80);
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        Duration = completedAtUtc - startedAtUtc;
        SizeBytes = sizeBytes >= 0 ? sizeBytes : throw new DomainException("Backup size cannot be negative.");
        Message = Guard.AgainstNullOrWhiteSpace(message, nameof(message), 1_000);
        Rpo = rpo;
        Rto = rto;
    }

    public string BackupKind { get; private set; }

    public string Result { get; private set; }

    public DateTimeOffset StartedAtUtc { get; private set; }

    public DateTimeOffset CompletedAtUtc { get; private set; }

    public TimeSpan Duration { get; private set; }

    public long SizeBytes { get; private set; }

    public string Message { get; private set; }

    public TimeSpan Rpo { get; private set; }

    public TimeSpan Rto { get; private set; }
}
