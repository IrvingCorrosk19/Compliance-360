using Compliance360.Domain.Common;

namespace Compliance360.Domain.TenantManagement;

public enum TenantStatus
{
    Draft = 0,
    Active = 1,
    Suspended = 2,
    Archived = 3
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
        Settings = null!;
        Branding = null!;
        Subscription = null!;
    }

    public Tenant(string name, string slug)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Slug = Guard.AgainstNullOrWhiteSpace(slug, nameof(slug), 80).ToLowerInvariant();
        Status = TenantStatus.Draft;
        Settings = TenantSettings.CreateDefault(Id);
        Branding = TenantBranding.CreateDefault(Id, Name);
        Subscription = Subscription.CreateTrial(Id);
    }

    public string Name { get; private set; }

    public string Slug { get; private set; }

    public TenantStatus Status { get; private set; }

    public TenantSettings Settings { get; private set; }

    public TenantBranding Branding { get; private set; }

    public Subscription Subscription { get; private set; }

    public IReadOnlyCollection<Company> Companies => _companies.AsReadOnly();

    public void Activate()
    {
        if (Status == TenantStatus.Archived)
        {
            throw new DomainException("Archived tenants cannot be activated.");
        }

        Status = TenantStatus.Active;
    }

    public void Suspend()
    {
        if (Status != TenantStatus.Active)
        {
            throw new DomainException("Only active tenants can be suspended.");
        }

        Status = TenantStatus.Suspended;
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
    }

    public void UpdateBranding(TenantBranding branding)
    {
        if (branding.TenantId != Id)
        {
            throw new DomainException("Tenant branding must belong to the same tenant.");
        }

        Branding = branding;
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
        TaxIdentifier = Guard.AgainstNullOrWhiteSpace(taxIdentifier, nameof(taxIdentifier), 80);
        CountryCode = Guard.AgainstNullOrWhiteSpace(countryCode, nameof(countryCode), 2).ToUpperInvariant();
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
    }

    public void MarkActive()
    {
        Status = SubscriptionStatus.Active;
    }
}

public sealed class TenantSettings : TenantEntity
{
    private TenantSettings()
    {
        Culture = string.Empty;
        TimeZone = string.Empty;
    }

    private TenantSettings(Guid tenantId)
        : base(tenantId)
    {
        Culture = "es-PA";
        TimeZone = "America/Panama";
        RequireMfa = true;
        DocumentRetentionDays = 2_555;
    }

    public string Culture { get; private set; }

    public string TimeZone { get; private set; }

    public bool RequireMfa { get; private set; }

    public int DocumentRetentionDays { get; private set; }

    public static TenantSettings CreateDefault(Guid tenantId)
    {
        return new TenantSettings(tenantId);
    }

    public void Configure(string culture, string timeZone, bool requireMfa, int documentRetentionDays)
    {
        Culture = Guard.AgainstNullOrWhiteSpace(culture, nameof(culture), 12);
        TimeZone = Guard.AgainstNullOrWhiteSpace(timeZone, nameof(timeZone), 80);
        RequireMfa = requireMfa;
        DocumentRetentionDays = Guard.AgainstOutOfRange(documentRetentionDays, nameof(documentRetentionDays), 30, 18_250);
    }
}

public sealed class TenantBranding : TenantEntity
{
    private TenantBranding()
    {
        DisplayName = string.Empty;
        PrimaryColor = string.Empty;
        SecondaryColor = string.Empty;
    }

    private TenantBranding(Guid tenantId, string displayName)
        : base(tenantId)
    {
        DisplayName = Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName), 180);
        PrimaryColor = "#0F172A";
        SecondaryColor = "#2563EB";
    }

    public string DisplayName { get; private set; }

    public string? LogoUri { get; private set; }

    public string PrimaryColor { get; private set; }

    public string SecondaryColor { get; private set; }

    public static TenantBranding CreateDefault(Guid tenantId, string displayName)
    {
        return new TenantBranding(tenantId, displayName);
    }

    public void Configure(string displayName, string? logoUri, string primaryColor, string secondaryColor)
    {
        DisplayName = Guard.AgainstNullOrWhiteSpace(displayName, nameof(displayName), 180);
        LogoUri = string.IsNullOrWhiteSpace(logoUri) ? null : logoUri.Trim();
        PrimaryColor = Guard.AgainstNullOrWhiteSpace(primaryColor, nameof(primaryColor), 20);
        SecondaryColor = Guard.AgainstNullOrWhiteSpace(secondaryColor, nameof(secondaryColor), 20);
    }
}
