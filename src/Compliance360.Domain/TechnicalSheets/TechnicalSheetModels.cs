using Compliance360.Domain.Common;

namespace Compliance360.Domain.TechnicalSheets;

public enum TechnicalSheetStatus
{
    Draft = 0,
    InReview = 1,
    Approved = 2,
    Rejected = 3,
    Obsolete = 4
}

public enum TechnicalSheetApprovalDecision
{
    Approved = 0,
    Rejected = 1
}

public sealed class Product : TenantEntity
{
    private Product()
    {
        Name = string.Empty;
        Sku = string.Empty;
    }

    public Product(Guid tenantId, string name, string sku, string? description)
        : base(tenantId)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Sku = Guard.AgainstNullOrWhiteSpace(sku, nameof(sku), 80).ToUpperInvariant();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsActive = true;
    }

    public string Name { get; private set; }

    public string Sku { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }
}

public sealed class TechnicalSheet : TenantEntity
{
    private readonly List<TechnicalSheetIngredient> _ingredients = [];
    private readonly List<TechnicalSheetNutrient> _nutrients = [];
    private readonly List<TechnicalSheetCertification> _certifications = [];
    private readonly List<TechnicalSheetVersion> _versions = [];
    private readonly List<TechnicalSheetApproval> _approvals = [];

    private TechnicalSheet()
    {
        Title = string.Empty;
    }

    public TechnicalSheet(Guid tenantId, Guid productId, string title, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        ProductId = Guard.AgainstEmpty(productId, nameof(productId));
        Title = Guard.AgainstNullOrWhiteSpace(title, nameof(title), 220);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        Status = TechnicalSheetStatus.Draft;
        CurrentVersionNumber = 0;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid ProductId { get; private set; }

    public string Title { get; private set; }

    public TechnicalSheetStatus Status { get; private set; }

    public int CurrentVersionNumber { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset? ApprovedAtUtc { get; private set; }

    public string? PdfObjectKey { get; private set; }

    public IReadOnlyCollection<TechnicalSheetIngredient> Ingredients => _ingredients.AsReadOnly();

    public IReadOnlyCollection<TechnicalSheetNutrient> Nutrients => _nutrients.AsReadOnly();

    public IReadOnlyCollection<TechnicalSheetCertification> Certifications => _certifications.AsReadOnly();

    public IReadOnlyCollection<TechnicalSheetVersion> Versions => _versions.AsReadOnly();

    public IReadOnlyCollection<TechnicalSheetApproval> Approvals => _approvals.AsReadOnly();

    public TechnicalSheetVersion CreateVersion(string changeSummary, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        if (Status == TechnicalSheetStatus.Obsolete)
        {
            throw new DomainException("Obsolete technical sheets cannot be versioned.");
        }

        CurrentVersionNumber++;
        Status = TechnicalSheetStatus.Draft;
        var version = new TechnicalSheetVersion(TenantId, Id, CurrentVersionNumber, changeSummary, createdByUserId, createdAtUtc);
        _versions.Add(version);
        return version;
    }

    public void AddIngredient(string name, decimal percentage, string? allergen)
    {
        _ingredients.Add(new TechnicalSheetIngredient(TenantId, Id, name, percentage, allergen));
    }

    public void AddNutrient(string name, decimal amount, string unit)
    {
        _nutrients.Add(new TechnicalSheetNutrient(TenantId, Id, name, amount, unit));
    }

    public void AddCertification(string name, string issuer, DateTimeOffset expiresAtUtc)
    {
        _certifications.Add(new TechnicalSheetCertification(TenantId, Id, name, issuer, expiresAtUtc));
    }

    public void SubmitForApproval(Guid requestedByUserId)
    {
        if (CurrentVersionNumber == 0)
        {
            throw new DomainException("Technical sheet requires at least one version.");
        }

        Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
        Status = TechnicalSheetStatus.InReview;
    }

    public TechnicalSheetApproval Decide(TechnicalSheetApprovalDecision decision, string comments, Guid decidedByUserId, DateTimeOffset decidedAtUtc)
    {
        if (Status != TechnicalSheetStatus.InReview)
        {
            throw new DomainException("Only technical sheets in review can be approved or rejected.");
        }

        var approval = new TechnicalSheetApproval(TenantId, Id, CurrentVersionNumber, decision, comments, decidedByUserId, decidedAtUtc);
        _approvals.Add(approval);
        Status = decision == TechnicalSheetApprovalDecision.Approved ? TechnicalSheetStatus.Approved : TechnicalSheetStatus.Rejected;
        ApprovedAtUtc = decision == TechnicalSheetApprovalDecision.Approved ? decidedAtUtc : null;
        return approval;
    }

    public void AttachPdf(string objectKey)
    {
        PdfObjectKey = Guard.AgainstNullOrWhiteSpace(objectKey, nameof(objectKey), 500);
    }

    public void MarkObsolete(Guid requestedByUserId)
    {
        Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
        Status = TechnicalSheetStatus.Obsolete;
    }
}

public sealed class TechnicalSheetIngredient : TenantEntity
{
    private TechnicalSheetIngredient()
    {
        Name = string.Empty;
    }

    public TechnicalSheetIngredient(Guid tenantId, Guid technicalSheetId, string name, decimal percentage, string? allergen)
        : base(tenantId)
    {
        TechnicalSheetId = Guard.AgainstEmpty(technicalSheetId, nameof(technicalSheetId));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        if (percentage < 0 || percentage > 100)
        {
            throw new DomainException("Ingredient percentage must be between 0 and 100.");
        }

        Percentage = percentage;
        Allergen = string.IsNullOrWhiteSpace(allergen) ? null : allergen.Trim();
    }

    public Guid TechnicalSheetId { get; private set; }

    public string Name { get; private set; }

    public decimal Percentage { get; private set; }

    public string? Allergen { get; private set; }
}

public sealed class TechnicalSheetNutrient : TenantEntity
{
    private TechnicalSheetNutrient()
    {
        Name = string.Empty;
        Unit = string.Empty;
    }

    public TechnicalSheetNutrient(Guid tenantId, Guid technicalSheetId, string name, decimal amount, string unit)
        : base(tenantId)
    {
        TechnicalSheetId = Guard.AgainstEmpty(technicalSheetId, nameof(technicalSheetId));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        if (amount < 0)
        {
            throw new DomainException("Nutrient amount cannot be negative.");
        }

        Amount = amount;
        Unit = Guard.AgainstNullOrWhiteSpace(unit, nameof(unit), 40);
    }

    public Guid TechnicalSheetId { get; private set; }

    public string Name { get; private set; }

    public decimal Amount { get; private set; }

    public string Unit { get; private set; }
}

public sealed class TechnicalSheetCertification : TenantEntity
{
    private TechnicalSheetCertification()
    {
        Name = string.Empty;
        Issuer = string.Empty;
    }

    public TechnicalSheetCertification(Guid tenantId, Guid technicalSheetId, string name, string issuer, DateTimeOffset expiresAtUtc)
        : base(tenantId)
    {
        TechnicalSheetId = Guard.AgainstEmpty(technicalSheetId, nameof(technicalSheetId));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        Issuer = Guard.AgainstNullOrWhiteSpace(issuer, nameof(issuer), 180);
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid TechnicalSheetId { get; private set; }

    public string Name { get; private set; }

    public string Issuer { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }
}

public sealed class TechnicalSheetVersion : TenantEntity
{
    private TechnicalSheetVersion()
    {
        ChangeSummary = string.Empty;
    }

    public TechnicalSheetVersion(Guid tenantId, Guid technicalSheetId, int versionNumber, string changeSummary, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        TechnicalSheetId = Guard.AgainstEmpty(technicalSheetId, nameof(technicalSheetId));
        VersionNumber = Guard.AgainstOutOfRange(versionNumber, nameof(versionNumber), 1, 10_000);
        ChangeSummary = Guard.AgainstNullOrWhiteSpace(changeSummary, nameof(changeSummary), 1_000);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
    }

    public Guid TechnicalSheetId { get; private set; }

    public int VersionNumber { get; private set; }

    public string ChangeSummary { get; private set; }

    public Guid CreatedByUserId { get; private set; }
}

public sealed class TechnicalSheetApproval : TenantEntity
{
    private TechnicalSheetApproval()
    {
        Comments = string.Empty;
    }

    public TechnicalSheetApproval(Guid tenantId, Guid technicalSheetId, int versionNumber, TechnicalSheetApprovalDecision decision, string comments, Guid decidedByUserId, DateTimeOffset decidedAtUtc)
        : base(tenantId)
    {
        TechnicalSheetId = Guard.AgainstEmpty(technicalSheetId, nameof(technicalSheetId));
        VersionNumber = Guard.AgainstOutOfRange(versionNumber, nameof(versionNumber), 1, 10_000);
        Decision = decision;
        Comments = Guard.AgainstNullOrWhiteSpace(comments, nameof(comments), 1_000);
        DecidedByUserId = Guard.AgainstEmpty(decidedByUserId, nameof(decidedByUserId));
        DecidedAtUtc = decidedAtUtc;
    }

    public Guid TechnicalSheetId { get; private set; }

    public int VersionNumber { get; private set; }

    public TechnicalSheetApprovalDecision Decision { get; private set; }

    public string Comments { get; private set; }

    public Guid DecidedByUserId { get; private set; }

    public DateTimeOffset DecidedAtUtc { get; private set; }
}
