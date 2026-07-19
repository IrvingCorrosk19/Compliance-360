using Compliance360.Domain.Common;

namespace Compliance360.Domain.TenantManagement;

public enum TenantStatus
{
    Draft = 0,
    Trial = 1,
    Active = 2,
    Suspended = 3,
    Archived = 4
}

public enum SubscriptionStatus
{
    Trial = 0,
    Active = 1,
    PastDue = 2,
    Suspended = 3,
    Cancelled = 4
}

public enum SubscriptionPlan
{
    Starter = 0,
    Professional = 1,
    Enterprise = 2,
    Dedicated = 3
}

public sealed class Tenant : Entity
{
    private readonly List<Company> _companies = [];

    private Tenant()
    {
        Name = string.Empty;
        Slug = string.Empty;
        LegalName = string.Empty;
        CommercialName = string.Empty;
        TaxIdentifier = string.Empty;
        Industry = string.Empty;
        CountryCode = string.Empty;
        Currency = string.Empty;
        Settings = null!;
        Branding = null!;
        Subscription = null!;
    }

    public Tenant(string name, string slug)
        : this(name, slug, legalName: name, commercialName: name, taxIdentifier: $"TAX-{Guid.NewGuid():N}"[..20], countryCode: "PA", currency: "USD", createdByUserId: null)
    {
    }

    public Tenant(
        string name,
        string slug,
        string legalName,
        string commercialName,
        string taxIdentifier,
        string countryCode,
        string currency,
        Guid? createdByUserId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Slug = Guard.AgainstNullOrWhiteSpace(slug, nameof(slug), 80).ToLowerInvariant();
        LegalName = Guard.AgainstNullOrWhiteSpace(legalName, nameof(legalName), 220);
        CommercialName = Guard.AgainstNullOrWhiteSpace(commercialName, nameof(commercialName), 180);
        TaxIdentifier = TenantValueObjects.TaxId(taxIdentifier, nameof(taxIdentifier));
        Industry = string.Empty;
        Description = null;
        AddressLine1 = null;
        City = null;
        Province = null;
        CountryCode = TenantValueObjects.Country(countryCode, nameof(countryCode));
        PostalCode = null;
        Phone = null;
        Email = null;
        Website = null;
        Currency = TenantValueObjects.Currency(currency, nameof(currency));
        CreatedByUserId = createdByUserId;
        Status = TenantStatus.Draft;
        Settings = TenantSettings.CreateDefault(Id);
        Branding = TenantBranding.CreateDefault(Id, CommercialName);
        Subscription = Subscription.CreateTrial(Id);
    }

    public string Name { get; private set; }

    public string Slug { get; private set; }

    public string LegalName { get; private set; }

    public string CommercialName { get; private set; }

    public string TaxIdentifier { get; private set; }

    public string Industry { get; private set; }

    public string? Description { get; private set; }

    public string? AddressLine1 { get; private set; }

    public string? City { get; private set; }

    public string? Province { get; private set; }

    public string CountryCode { get; private set; }

    public string? PostalCode { get; private set; }

    public string? Phone { get; private set; }

    public string? Email { get; private set; }

    public string? Website { get; private set; }

    public string Currency { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public TenantStatus Status { get; private set; }

    public TenantSettings Settings { get; private set; }

    public TenantBranding Branding { get; private set; }

    public Subscription Subscription { get; private set; }

    public IReadOnlyCollection<Company> Companies => _companies.AsReadOnly();

    public void UpdateGeneralInformation(
        string name,
        string legalName,
        string commercialName,
        string taxIdentifier,
        string industry,
        string? description,
        string? addressLine1,
        string? city,
        string? province,
        string countryCode,
        string? postalCode,
        string? phone,
        string? email,
        string? website,
        string currency)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        LegalName = Guard.AgainstNullOrWhiteSpace(legalName, nameof(legalName), 220);
        CommercialName = Guard.AgainstNullOrWhiteSpace(commercialName, nameof(commercialName), 180);
        TaxIdentifier = TenantValueObjects.TaxId(taxIdentifier, nameof(taxIdentifier));
        Industry = Guard.AgainstNullOrWhiteSpace(industry, nameof(industry), 120);
        Description = NormalizeOptional(description, 1_000);
        AddressLine1 = NormalizeOptional(addressLine1, 220);
        City = NormalizeOptional(city, 120);
        Province = NormalizeOptional(province, 120);
        CountryCode = TenantValueObjects.Country(countryCode, nameof(countryCode));
        PostalCode = NormalizeOptional(postalCode, 20);
        Phone = TenantValueObjects.Phone(phone, nameof(phone));
        Email = TenantValueObjects.OptionalEmail(email, nameof(email));
        Website = TenantValueObjects.OptionalUrl(website, nameof(website));
        Currency = TenantValueObjects.Currency(currency, nameof(currency));
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void ChangeSlug(string slug)
    {
        Slug = Guard.AgainstNullOrWhiteSpace(slug, nameof(slug), 80).ToLowerInvariant();
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void StartTrial()
    {
        if (Status == TenantStatus.Archived)
        {
            throw new DomainException("Archived tenants cannot start trial.");
        }

        Status = TenantStatus.Trial;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void Activate()
    {
        if (Status == TenantStatus.Archived)
        {
            throw new DomainException("Archived tenants cannot be activated.");
        }

        Status = TenantStatus.Active;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void Suspend()
    {
        if (Status != TenantStatus.Active)
        {
            throw new DomainException("Only active tenants can be suspended.");
        }

        Status = TenantStatus.Suspended;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void Archive()
    {
        if (Status == TenantStatus.Archived)
        {
            throw new DomainException("Tenant is already archived.");
        }

        Status = TenantStatus.Archived;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void Restore()
    {
        if (Status != TenantStatus.Archived)
        {
            throw new DomainException("Only archived tenants can be restored.");
        }

        Status = TenantStatus.Suspended;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public Company AddCompany(string legalName, string taxIdentifier, string countryCode)
    {
        var company = new Company(Id, legalName, taxIdentifier, countryCode);
        _companies.Add(company);
        return company;
    }

    public void UpdateSettings(TenantSettings settings)
    {
        if (settings.TenantId != Id)
        {
            throw new DomainException("Tenant settings must belong to the same tenant.");
        }

        Settings = settings;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void UpdateBranding(TenantBranding branding)
    {
        if (branding.TenantId != Id)
        {
            throw new DomainException("Tenant branding must belong to the same tenant.");
        }

        Branding = branding;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{nameof(value)} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }
}

public sealed class Company : TenantEntity
{
    private Company()
    {
        LegalName = string.Empty;
        TaxIdentifier = string.Empty;
        CountryCode = string.Empty;
    }

    public Company(Guid tenantId, string legalName, string taxIdentifier, string countryCode)
        : base(tenantId)
    {
        LegalName = Guard.AgainstNullOrWhiteSpace(legalName, nameof(legalName), 220);
        TaxIdentifier = TenantValueObjects.TaxId(taxIdentifier, nameof(taxIdentifier));
        CountryCode = TenantValueObjects.Country(countryCode, nameof(countryCode));
        IsActive = true;
    }

    public string LegalName { get; private set; }

    public string TaxIdentifier { get; private set; }

    public string CountryCode { get; private set; }

    public bool IsActive { get; private set; }

    public void Deactivate()
    {
        IsActive = false;
    }
}

public sealed class Subscription : TenantEntity
{
    private Subscription()
    {
    }

    private Subscription(Guid tenantId, SubscriptionPlan plan, SubscriptionStatus status)
        : base(tenantId)
    {
        Plan = plan;
        Status = status;
        MaxUsers = 25;
        MaxStorageGb = 10;
    }

    public SubscriptionPlan Plan { get; private set; }

    public SubscriptionStatus Status { get; private set; }

    public int MaxUsers { get; private set; }

    public int MaxStorageGb { get; private set; }

    public DateOnly? ExpiresOn { get; private set; }

    public static Subscription CreateTrial(Guid tenantId)
    {
        return new Subscription(tenantId, SubscriptionPlan.Starter, SubscriptionStatus.Trial)
        {
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))
        };
    }

    public void ChangePlan(SubscriptionPlan plan, int maxUsers, int maxStorageGb)
    {
        Plan = plan;
        MaxUsers = Guard.AgainstOutOfRange(maxUsers, nameof(maxUsers), 1, 100_000);
        MaxStorageGb = Guard.AgainstOutOfRange(maxStorageGb, nameof(maxStorageGb), 1, 1_000_000);
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void MarkActive()
    {
        Status = SubscriptionStatus.Active;
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void ConfigureCommercialState(SubscriptionStatus status, DateOnly? expiresOn)
    {
        Status = status;
        ExpiresOn = expiresOn;
        MarkUpdated(DateTimeOffset.UtcNow);
    }
}

public sealed class TenantSettings : TenantEntity
{
    private TenantSettings()
    {
        Culture = string.Empty;
        TimeZone = string.Empty;
        Language = string.Empty;
        IpWhitelist = string.Empty;
    }

    private TenantSettings(Guid tenantId)
        : base(tenantId)
    {
        Culture = "es-PA";
        Language = "es";
        TimeZone = "America/Panama";
        RequireMfa = false;
        DocumentRetentionDays = 2_555;
        SessionTimeoutMinutes = 30;
        PasswordExpirationDays = 90;
        LockoutMaxFailedAttempts = 5;
        LockoutMinutes = 15;
        IpWhitelist = string.Empty;
        TrustedDevicesEnabled = false;
        SecurityScore = 75;
    }

    public string Culture { get; private set; }

    public string Language { get; private set; }

    public string TimeZone { get; private set; }

    public bool RequireMfa { get; private set; }

    public int DocumentRetentionDays { get; private set; }

    public int SessionTimeoutMinutes { get; private set; }

    public int PasswordExpirationDays { get; private set; }

    public int LockoutMaxFailedAttempts { get; private set; }

    public int LockoutMinutes { get; private set; }

    public string IpWhitelist { get; private set; }

    public bool TrustedDevicesEnabled { get; private set; }

    public int SecurityScore { get; private set; }

    public static TenantSettings CreateDefault(Guid tenantId)
    {
        return new TenantSettings(tenantId);
    }

    public void Configure(string culture, string timeZone, bool requireMfa, int documentRetentionDays)
    {
        Culture = Guard.AgainstNullOrWhiteSpace(culture, nameof(culture), 12);
        Language = culture.Split('-', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "es";
        TimeZone = TenantValueObjects.TimeZone(timeZone, nameof(timeZone));
        RequireMfa = requireMfa;
        DocumentRetentionDays = Guard.AgainstOutOfRange(documentRetentionDays, nameof(documentRetentionDays), 30, 18_250);
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    public void ConfigureSecurity(
        bool requireMfa,
        int sessionTimeoutMinutes,
        int passwordExpirationDays,
        int lockoutMaxFailedAttempts,
        int lockoutMinutes,
        string? ipWhitelist,
        bool trustedDevicesEnabled,
        int securityScore)
    {
        RequireMfa = requireMfa;
        SessionTimeoutMinutes = Guard.AgainstOutOfRange(sessionTimeoutMinutes, nameof(sessionTimeoutMinutes), 5, 1_440);
        PasswordExpirationDays = Guard.AgainstOutOfRange(passwordExpirationDays, nameof(passwordExpirationDays), 0, 730);
        LockoutMaxFailedAttempts = Guard.AgainstOutOfRange(lockoutMaxFailedAttempts, nameof(lockoutMaxFailedAttempts), 1, 25);
        LockoutMinutes = Guard.AgainstOutOfRange(lockoutMinutes, nameof(lockoutMinutes), 1, 1_440);
        IpWhitelist = TenantValueObjects.CidrList(ipWhitelist, nameof(ipWhitelist));
        TrustedDevicesEnabled = trustedDevicesEnabled;
        SecurityScore = Guard.AgainstOutOfRange(securityScore, nameof(securityScore), 0, 100);
        MarkUpdated(DateTimeOffset.UtcNow);
    }
}

public sealed class TenantBranding : TenantEntity
{
    private TenantBranding()
    {
        DisplayName = string.Empty;
        PrimaryColor = string.Empty;
        SecondaryColor = string.Empty;
        Theme = string.Empty;
        CorporateEmail = string.Empty;
        FooterText = string.Empty;
    }

    private TenantBranding(Guid tenantId, string displayName)
        : base(tenantId)
    {
        DisplayName = Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName), 180);
        PrimaryColor = "#0F172A";
        SecondaryColor = "#2563EB";
        Theme = "System";
        CorporateEmail = string.Empty;
        FooterText = "Compliance 360";
    }

    public string DisplayName { get; private set; }

    public string? LogoUri { get; private set; }

    public string? FaviconUri { get; private set; }

    public string PrimaryColor { get; private set; }

    public string SecondaryColor { get; private set; }

    public string Theme { get; private set; }

    public string? LoginBackgroundUri { get; private set; }

    public string CorporateEmail { get; private set; }

    public string FooterText { get; private set; }

    public static TenantBranding CreateDefault(Guid tenantId, string displayName)
    {
        return new TenantBranding(tenantId, displayName);
    }

    public void Configure(string displayName, string? logoUri, string primaryColor, string secondaryColor)
    {
        Configure(displayName, logoUri, FaviconUri, primaryColor, secondaryColor, Theme, LoginBackgroundUri, CorporateEmail, FooterText);
    }

    public void Configure(
        string displayName,
        string? logoUri,
        string? faviconUri,
        string primaryColor,
        string secondaryColor,
        string theme,
        string? loginBackgroundUri,
        string? corporateEmail,
        string? footerText)
    {
        DisplayName = Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName), 180);
        LogoUri = NormalizeOptionalUrl(logoUri, nameof(logoUri));
        FaviconUri = NormalizeOptionalUrl(faviconUri, nameof(faviconUri));
        PrimaryColor = TenantValueObjects.Color(primaryColor, nameof(primaryColor));
        SecondaryColor = TenantValueObjects.Color(secondaryColor, nameof(secondaryColor));
        Theme = NormalizeTheme(theme);
        LoginBackgroundUri = NormalizeOptionalUrl(loginBackgroundUri, nameof(loginBackgroundUri));
        CorporateEmail = TenantValueObjects.OptionalEmail(corporateEmail, nameof(corporateEmail));
        FooterText = string.IsNullOrWhiteSpace(footerText) ? string.Empty : Guard.AgainstNullOrWhiteSpace(footerText, nameof(footerText), 160);
        MarkUpdated(DateTimeOffset.UtcNow);
    }

    private static string? NormalizeOptionalUrl(string? value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value) ? null : TenantValueObjects.Url(value, parameterName);
    }

    private static string NormalizeTheme(string? theme)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(theme, nameof(theme), 40);
        return normalized switch
        {
            "System" or "Light" or "Dark" => normalized,
            _ => throw new DomainException("theme must be System, Light or Dark.")
        };
    }
}
