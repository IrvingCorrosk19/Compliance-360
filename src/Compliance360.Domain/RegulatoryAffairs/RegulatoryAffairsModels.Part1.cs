using Compliance360.Domain.Common;

namespace Compliance360.Domain.RegulatoryAffairs;

// ── Enums (RiskClass is NOT RiskManagement) ──────────────────────────────────

public enum DeviceRiskClass
{
    A = 0,
    B = 1,
    C = 2,
    Unknown = 9
}

/// <summary>Lifecycle of REGUTRACK product artifacts: Ficha Técnica and Formulario.</summary>
public enum RegulatoryArtifactStatus
{
    Missing = 0,
    Pending = 1,
    Received = 2,
    UnderReview = 3,
    Accepted = 4,
    Rejected = 5,
    Observed = 6,
    Superseded = 7
}

public enum RegulatoryAuthorityType
{
    MinistryOfHealth = 0,
    SocialSecurity = 1,
    Other = 9
}

public enum SanitaryRegistrationStatus
{
    Draft = 0,
    InProcess = 1,
    Active = 2,
    Expiring = 3,
    Expired = 4,
    Suspended = 5,
    Cancelled = 6,
    Replaced = 7
}

public enum RegistrationProcessType
{
    NewRegistration = 0,
    Renewal = 1,
    Modification = 2,
    Extension = 3,
    ReRegistration = 4,
    Other = 9
}

public enum RegistrationDossierStatus
{
    Draft = 0,
    Planning = 1,
    WaitingManufacturerDocuments = 2,
    DocumentsReceived = 3,
    Assembling = 4,
    ReadyForSubmission = 5,
    Submitted = 6,
    UnderAuthorityReview = 7,
    Observed = 8,
    CorrectingObservation = 9,
    Resubmitted = 10,
    Approved = 11,
    Rejected = 12,
    Cancelled = 13,
    Closed = 14,
    OnHold = 15,
    /// <summary>Internal clearance to submit — not a MINSA/CSS decision.</summary>
    ApprovedForSubmission = 16,
    UnderTechnicalReview = 17,
    CorrectionRequested = 18,
    ResponseReady = 19,
    Archived = 20
}

public enum DossierMilestoneType
{
    FactoryRequest = 0,
    EstimatedReception = 1,
    MaximumReception = 2,
    Reception = 3,
    DossierAssembly = 4,
    EstimatedSubmission = 5,
    Submission = 6,
    Observation = 7,
    EstimatedApproval = 8,
    Approval = 9,
    Expiration = 10,
    Renewal = 11
}

public enum MilestoneCompletionStatus
{
    Planned = 0,
    InProgress = 1,
    Completed = 2,
    Missed = 3,
    Cancelled = 4
}

public enum DossierRequirementStatus
{
    NotRequired = 0,
    Pending = 1,
    Requested = 2,
    Received = 3,
    UnderReview = 4,
    Observed = 5,
    Accepted = 6,
    Rejected = 7,
    Expired = 8,
    Waived = 9
}

public enum RequirementValidationStatus
{
    NotValidated = 0,
    Valid = 1,
    Invalid = 2,
    NeedsClarification = 3
}

public enum AuthorityObservationStatus
{
    Open = 0,
    InProgress = 1,
    ResponseReady = 2,
    Submitted = 3,
    Closed = 4,
    Cancelled = 5
}

public enum ManufacturerCertificateType
{
    Iso13485 = 0,
    Clv = 1,
    Ce = 2,
    Fda = 3,
    Gmp = 4,
    Other = 9
}

public enum ManufacturerCertificateStatus
{
    Draft = 0,
    Active = 1,
    Expiring = 2,
    Expired = 3,
    Revoked = 4,
    Replaced = 5
}

public enum CertificateLegalFormat
{
    SimpleCopy = 0,
    Notarized = 1,
    Apostilled = 2,
    Original = 3,
    Other = 9
}

public enum OperatingLicenseStatus
{
    Draft = 0,
    Active = 1,
    Expiring = 2,
    Expired = 3,
    InRenewal = 4,
    Suspended = 5,
    Cancelled = 6
}

public enum LicenseRenewalCaseStatus
{
    Draft = 0,
    Assembling = 1,
    ReadyForSubmission = 2,
    Submitted = 3,
    Observed = 4,
    Approved = 5,
    Rejected = 6,
    Closed = 7,
    ManualPlatformUpdatePending = 8
}

public enum RequirementPackStatus
{
    Draft = 0,
    Published = 1,
    Retired = 2
}

public enum RegutrackImportJobStatus
{
    Uploaded = 0,
    Mapped = 1,
    Validated = 2,
    Simulated = 3,
    Committed = 4,
    Failed = 5,
    RolledBack = 6
}

// ── Authority ────────────────────────────────────────────────────────────────

public sealed class RegulatoryAuthority : TenantEntity
{
    private RegulatoryAuthority()
    {
        Code = string.Empty;
        Name = string.Empty;
        CountryCode = "PA";
    }

    public RegulatoryAuthority(
        Guid tenantId,
        string code,
        string name,
        string countryCode,
        RegulatoryAuthorityType authorityType)
        : base(tenantId)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 40).ToUpperInvariant();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 180);
        CountryCode = Guard.AgainstNullOrWhiteSpace(countryCode, nameof(countryCode), 8).ToUpperInvariant();
        AuthorityType = authorityType;
        IsActive = true;
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public string CountryCode { get; private set; }
    public RegulatoryAuthorityType AuthorityType { get; private set; }
    public bool IsActive { get; private set; }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

// ── Requirement Pack (definition; FormTemplate remains separate) ─────────────

public sealed class RegulatoryRequirementPack : TenantEntity
{
    private readonly List<RequirementDefinition> _definitions = [];

    private RegulatoryRequirementPack()
    {
        Code = string.Empty;
        Name = string.Empty;
        CountryCode = "PA";
        VersionLabel = "1.0";
    }

    public RegulatoryRequirementPack(
        Guid tenantId,
        string code,
        string name,
        string countryCode,
        Guid? authorityId,
        DeviceRiskClass? riskClass,
        RegistrationProcessType? processType,
        string? productCategory,
        Guid createdByUserId)
        : base(tenantId)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 80).ToUpperInvariant();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 220);
        CountryCode = Guard.AgainstNullOrWhiteSpace(countryCode, nameof(countryCode), 8).ToUpperInvariant();
        AuthorityId = authorityId;
        RiskClass = riskClass;
        ProcessType = processType;
        ProductCategory = string.IsNullOrWhiteSpace(productCategory) ? null : productCategory.Trim();
        VersionLabel = "1.0";
        Status = RequirementPackStatus.Draft;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public string CountryCode { get; private set; }
    public Guid? AuthorityId { get; private set; }
    public DeviceRiskClass? RiskClass { get; private set; }
    public RegistrationProcessType? ProcessType { get; private set; }
    public string? ProductCategory { get; private set; }
    public string VersionLabel { get; private set; }
    public RequirementPackStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset? PublishedAtUtc { get; private set; }
    public Guid? OptionalFormTemplateId { get; private set; }

    public IReadOnlyCollection<RequirementDefinition> Definitions => _definitions.AsReadOnly();

    public RequirementDefinition AddDefinition(
        string code,
        string name,
        string category,
        bool isRequired,
        bool isCritical,
        int order,
        string? description = null)
    {
        if (Status == RequirementPackStatus.Retired)
        {
            throw new DomainException("Cannot modify a retired requirement pack.");
        }

        var def = new RequirementDefinition(TenantId, Id, code, name, category, isRequired, isCritical, order, description);
        _definitions.Add(def);
        return def;
    }

    public void Publish(DateTimeOffset publishedAtUtc)
    {
        if (_definitions.Count == 0)
        {
            throw new DomainException("Cannot publish an empty requirement pack.");
        }

        Status = RequirementPackStatus.Published;
        PublishedAtUtc = publishedAtUtc;
    }

    public void Retire() => Status = RequirementPackStatus.Retired;
}

public sealed class RequirementDefinition : TenantEntity
{
    private RequirementDefinition()
    {
        Code = string.Empty;
        Name = string.Empty;
        Category = "General";
    }

    public RequirementDefinition(
        Guid tenantId,
        Guid packId,
        string code,
        string name,
        string category,
        bool isRequired,
        bool isCritical,
        int order,
        string? description)
        : base(tenantId)
    {
        PackId = Guard.AgainstEmpty(packId, nameof(packId));
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 80).ToUpperInvariant();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 220);
        Category = Guard.AgainstNullOrWhiteSpace(category, nameof(category), 80);
        IsRequired = isRequired;
        IsCritical = isCritical;
        Order = order;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public Guid PackId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public string? Description { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsCritical { get; private set; }
    public int Order { get; private set; }
}

/// <summary>Canonical REGUTRACK checklist codes — used for seeding packs, not hardcoded into dossier logic.</summary>
public static class RegutrackRequirementCatalog
{
    public static IReadOnlyList<(string Code, string Name, string Category, bool Critical)> Items { get; } =
    [
        ("LEGAL_ID", "Copia cédula/pasaporte representante legal", "Legal", true),
        ("OPS_LICENSE", "Copia licencia de operaciones", "Legal", true),
        ("PUBLIC_REGISTRY", "Certificado registro público", "Legal", true),
        ("OFFEROR_CERT", "Certificado de oferente", "Legal", true),
        ("TECH_SHEET", "Ficha técnica", "Technical", true),
        ("DEVICE_LITERATURE", "Literatura técnica del dispositivo médico", "Technical", false),
        ("IFU", "Instructivo de uso / inserto", "Technical", true),
        ("MFG_COMMITMENT", "Carta de compromiso del fabricante", "Manufacturer", true),
        ("ISO", "Certificado ISO", "Manufacturer", true),
        ("CLV_FDA", "Cert. Libre Venta (CLV) o FDA", "Manufacturer", true),
        ("PHOTOS", "Fotografías", "Labeling", false),
        ("LABELS", "Etiquetas del producto", "Labeling", true),
        ("STERILIZATION", "Método de esterilización", "Technical", false),
        ("CLINICAL", "Resumen estudios o ensayos clínicos", "Clinical", false),
        ("MFG_PACKAGING", "Descripción manufactura y empaque", "Technical", false),
        ("RISK_ANALYSIS", "Análisis de riesgo", "Technical", false),
        ("TRACEABILITY", "Protocolo de trazabilidad", "Technical", false),
        ("SAMPLES", "Muestras", "Logistics", false),
        ("OPS_MANUAL", "Manual de operación y/o mantenimiento", "Technical", false),
        ("LOCAL_SUPPORT", "Certificación soporte técnico local", "Service", false),
        ("STORAGE_TRANSPORT", "Datos almacenamiento y transporte", "Logistics", false),
        ("ACCESSORIES", "Listado accesorios, repuestos y consumibles", "Logistics", false)
    ];
}

// ── Manufacturer (projection; optional Supplier link) ────────────────────────

public sealed class ManufacturerProfile : TenantEntity
{
    private ManufacturerProfile()
    {
        LegalName = string.Empty;
        CountryCode = "XX";
    }

    public ManufacturerProfile(
        Guid tenantId,
        string legalName,
        string countryCode,
        string? commercialName = null,
        Guid? supplierId = null,
        string? contactEmail = null,
        string? contactPhone = null)
        : base(tenantId)
    {
        LegalName = Guard.AgainstNullOrWhiteSpace(legalName, nameof(legalName), 220);
        CountryCode = Guard.AgainstNullOrWhiteSpace(countryCode, nameof(countryCode), 8).ToUpperInvariant();
        CommercialName = string.IsNullOrWhiteSpace(commercialName) ? null : commercialName.Trim();
        SupplierId = supplierId;
        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim();
        ContactPhone = string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim();
        IsActive = true;
    }

    public string LegalName { get; private set; }
    public string? CommercialName { get; private set; }
    public string CountryCode { get; private set; }
    public Guid? SupplierId { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(string legalName, string countryCode, string? commercialName, string? contactEmail, string? contactPhone)
    {
        LegalName = Guard.AgainstNullOrWhiteSpace(legalName, nameof(legalName), 220);
        CountryCode = Guard.AgainstNullOrWhiteSpace(countryCode, nameof(countryCode), 8).ToUpperInvariant();
        CommercialName = string.IsNullOrWhiteSpace(commercialName) ? null : commercialName.Trim();
        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim();
        ContactPhone = string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim();
    }
}

public sealed class ManufacturerCertificate : TenantEntity
{
    private ManufacturerCertificate()
    {
        Number = string.Empty;
        IssuedBy = string.Empty;
    }

    public ManufacturerCertificate(
        Guid tenantId,
        Guid manufacturerId,
        ManufacturerCertificateType type,
        string number,
        string issuedBy,
        DateTimeOffset? issuedOn,
        DateTimeOffset? expiresOn,
        string? country,
        CertificateLegalFormat legalFormat,
        bool apostilled,
        bool notarized,
        Guid? storedFileId,
        string? notes)
        : base(tenantId)
    {
        ManufacturerId = Guard.AgainstEmpty(manufacturerId, nameof(manufacturerId));
        Type = type;
        Number = Guard.AgainstNullOrWhiteSpace(number, nameof(number), 120);
        IssuedBy = Guard.AgainstNullOrWhiteSpace(issuedBy, nameof(issuedBy), 180);
        IssuedOn = issuedOn;
        ExpiresOn = expiresOn;
        if (issuedOn.HasValue && expiresOn.HasValue && expiresOn < issuedOn)
        {
            throw new DomainException("Certificate expiration cannot be before issuance.");
        }

        Country = string.IsNullOrWhiteSpace(country) ? null : country.Trim().ToUpperInvariant();
        LegalFormat = legalFormat;
        Apostilled = apostilled;
        Notarized = notarized;
        StoredFileId = storedFileId;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        RequestedOn = null;
        Status = ManufacturerCertificateStatus.Active;
        RefreshExpirationStatus(DateTimeOffset.UtcNow);
    }

    public Guid ManufacturerId { get; private set; }
    public ManufacturerCertificateType Type { get; private set; }
    public string Number { get; private set; }
    public string IssuedBy { get; private set; }
    public DateTimeOffset? IssuedOn { get; private set; }
    public DateTimeOffset? ExpiresOn { get; private set; }
    public DateTimeOffset? RequestedOn { get; private set; }
    public string? Country { get; private set; }
    public CertificateLegalFormat LegalFormat { get; private set; }
    public bool Apostilled { get; private set; }
    public bool Notarized { get; private set; }
    public ManufacturerCertificateStatus Status { get; private set; }
    public Guid? StoredFileId { get; private set; }
    public string? Notes { get; private set; }

    public void RefreshExpirationStatus(DateTimeOffset now)
    {
        if (Status is ManufacturerCertificateStatus.Revoked or ManufacturerCertificateStatus.Replaced)
        {
            return;
        }

        if (!ExpiresOn.HasValue)
        {
            Status = ManufacturerCertificateStatus.Active;
            return;
        }

        if (ExpiresOn.Value.Date < now.Date)
        {
            Status = ManufacturerCertificateStatus.Expired;
        }
        else if (ExpiresOn.Value.Date <= now.Date.AddDays(90))
        {
            Status = ManufacturerCertificateStatus.Expiring;
        }
        else
        {
            Status = ManufacturerCertificateStatus.Active;
        }
    }

    public void SetRequestedOn(DateTimeOffset? requestedOn) => RequestedOn = requestedOn;
}

public sealed class ManufacturerCertificateDossierLink : TenantEntity
{
    private ManufacturerCertificateDossierLink()
    {
    }

    public ManufacturerCertificateDossierLink(Guid tenantId, Guid certificateId, Guid dossierId, Guid? requirementId)
        : base(tenantId)
    {
        CertificateId = Guard.AgainstEmpty(certificateId, nameof(certificateId));
        DossierId = Guard.AgainstEmpty(dossierId, nameof(dossierId));
        RequirementId = requirementId;
    }

    public Guid CertificateId { get; private set; }
    public Guid DossierId { get; private set; }
    public Guid? RequirementId { get; private set; }
}
