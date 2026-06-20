using Compliance360.Domain.Common;

namespace Compliance360.Domain.Suppliers;

public enum SupplierStatus
{
    Draft = 0,
    PendingHomologation = 1,
    Homologated = 2,
    Suspended = 3,
    Rejected = 4
}

public enum SupplierDocumentType
{
    Ruc = 0,
    OperationsNotice = 1,
    Bpm = 2,
    Haccp = 3,
    SanitaryRegistration = 4,
    Other = 5
}

public enum SupplierDocumentStatus
{
    Pending = 0,
    Valid = 1,
    Expired = 2,
    Rejected = 3
}

public enum SupplierAlertStatus
{
    Open = 0,
    Acknowledged = 1,
    Closed = 2
}

public sealed class Supplier : TenantEntity
{
    private readonly List<SupplierDocument> _documents = [];
    private readonly List<SupplierEvaluation> _evaluations = [];
    private readonly List<SupplierExpirationAlert> _alerts = [];

    private Supplier()
    {
        LegalName = string.Empty;
        TaxIdentifier = string.Empty;
        CountryCode = string.Empty;
    }

    public Supplier(Guid tenantId, string legalName, string taxIdentifier, string countryCode, Guid createdByUserId, DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        LegalName = Guard.AgainstNullOrWhiteSpace(legalName, nameof(legalName), 220);
        TaxIdentifier = Guard.AgainstNullOrWhiteSpace(taxIdentifier, nameof(taxIdentifier), 80).ToUpperInvariant();
        CountryCode = Guard.AgainstNullOrWhiteSpace(countryCode, nameof(countryCode), 2).ToUpperInvariant();
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
        Status = SupplierStatus.Draft;
    }

    public string LegalName { get; private set; }

    public string TaxIdentifier { get; private set; }

    public string CountryCode { get; private set; }

    public SupplierStatus Status { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset? HomologatedAtUtc { get; private set; }

    public IReadOnlyCollection<SupplierDocument> Documents => _documents.AsReadOnly();

    public IReadOnlyCollection<SupplierEvaluation> Evaluations => _evaluations.AsReadOnly();

    public IReadOnlyCollection<SupplierExpirationAlert> Alerts => _alerts.AsReadOnly();

    public SupplierDocument AddDocument(SupplierDocumentType type, string documentNumber, Guid storedFileId, DateTimeOffset issuedAtUtc, DateTimeOffset expiresAtUtc, Guid uploadedByUserId)
    {
        if (expiresAtUtc <= issuedAtUtc)
        {
            throw new DomainException("Supplier document expiration must be after issue date.");
        }

        var document = new SupplierDocument(TenantId, Id, type, documentNumber, storedFileId, issuedAtUtc, expiresAtUtc, uploadedByUserId);
        _documents.Add(document);
        Status = SupplierStatus.PendingHomologation;
        return document;
    }

    public void ValidateDocument(Guid documentId, Guid validatedByUserId, DateTimeOffset validatedAtUtc)
    {
        var document = GetDocument(documentId);
        document.MarkValid(validatedByUserId, validatedAtUtc);
    }

    public void RejectDocument(Guid documentId, string reason, Guid rejectedByUserId, DateTimeOffset rejectedAtUtc)
    {
        var document = GetDocument(documentId);
        document.MarkRejected(reason, rejectedByUserId, rejectedAtUtc);
        Status = SupplierStatus.Rejected;
    }

    public SupplierEvaluation AddEvaluation(int score, string comments, Guid evaluatedByUserId, DateTimeOffset evaluatedAtUtc)
    {
        var evaluation = new SupplierEvaluation(TenantId, Id, score, comments, evaluatedByUserId, evaluatedAtUtc);
        _evaluations.Add(evaluation);
        return evaluation;
    }

    public void Homologate(Guid requestedByUserId, DateTimeOffset homologatedAtUtc)
    {
        Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
        var requiredTypes = new[]
        {
            SupplierDocumentType.Ruc,
            SupplierDocumentType.OperationsNotice,
            SupplierDocumentType.Bpm,
            SupplierDocumentType.Haccp,
            SupplierDocumentType.SanitaryRegistration
        };

        if (requiredTypes.Any(required => !_documents.Any(document => document.Type == required && document.Status == SupplierDocumentStatus.Valid && document.ExpiresAtUtc > homologatedAtUtc)))
        {
            throw new DomainException("Supplier cannot be homologated until all required documents are valid.");
        }

        if (_evaluations.Count == 0 || _evaluations.Max(evaluation => evaluation.Score) < 70)
        {
            throw new DomainException("Supplier requires an evaluation score of at least 70.");
        }

        Status = SupplierStatus.Homologated;
        HomologatedAtUtc = homologatedAtUtc;
    }

    public SupplierExpirationAlert CreateExpirationAlert(Guid documentId, DateTimeOffset alertAtUtc)
    {
        var document = GetDocument(documentId);
        var alert = new SupplierExpirationAlert(TenantId, Id, document.Id, document.Type, document.ExpiresAtUtc, alertAtUtc);
        _alerts.Add(alert);
        return alert;
    }

    public void Suspend(string reason, Guid requestedByUserId)
    {
        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 500);
        Guard.AgainstEmpty(requestedByUserId, nameof(requestedByUserId));
        Status = SupplierStatus.Suspended;
    }

    private SupplierDocument GetDocument(Guid documentId)
    {
        return _documents.SingleOrDefault(document => document.Id == documentId)
            ?? throw new DomainException("Supplier document not found.");
    }
}

public sealed class SupplierDocument : TenantEntity
{
    private SupplierDocument()
    {
        DocumentNumber = string.Empty;
    }

    public SupplierDocument(Guid tenantId, Guid supplierId, SupplierDocumentType type, string documentNumber, Guid storedFileId, DateTimeOffset issuedAtUtc, DateTimeOffset expiresAtUtc, Guid uploadedByUserId)
        : base(tenantId)
    {
        SupplierId = Guard.AgainstEmpty(supplierId, nameof(supplierId));
        Type = type;
        DocumentNumber = Guard.AgainstNullOrWhiteSpace(documentNumber, nameof(documentNumber), 120).ToUpperInvariant();
        StoredFileId = Guard.AgainstEmpty(storedFileId, nameof(storedFileId));
        IssuedAtUtc = issuedAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        UploadedByUserId = Guard.AgainstEmpty(uploadedByUserId, nameof(uploadedByUserId));
        Status = SupplierDocumentStatus.Pending;
    }

    public Guid SupplierId { get; private set; }

    public SupplierDocumentType Type { get; private set; }

    public string DocumentNumber { get; private set; }

    public Guid StoredFileId { get; private set; }

    public DateTimeOffset IssuedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public Guid UploadedByUserId { get; private set; }

    public SupplierDocumentStatus Status { get; private set; }

    public string? RejectionReason { get; private set; }

    public Guid? ReviewedByUserId { get; private set; }

    public DateTimeOffset? ReviewedAtUtc { get; private set; }

    public void MarkValid(Guid validatedByUserId, DateTimeOffset validatedAtUtc)
    {
        ReviewedByUserId = Guard.AgainstEmpty(validatedByUserId, nameof(validatedByUserId));
        ReviewedAtUtc = validatedAtUtc;
        RejectionReason = null;
        Status = SupplierDocumentStatus.Valid;
    }

    public void MarkRejected(string reason, Guid rejectedByUserId, DateTimeOffset rejectedAtUtc)
    {
        RejectionReason = Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 500);
        ReviewedByUserId = Guard.AgainstEmpty(rejectedByUserId, nameof(rejectedByUserId));
        ReviewedAtUtc = rejectedAtUtc;
        Status = SupplierDocumentStatus.Rejected;
    }
}

public sealed class SupplierEvaluation : TenantEntity
{
    private SupplierEvaluation()
    {
        Comments = string.Empty;
    }

    public SupplierEvaluation(Guid tenantId, Guid supplierId, int score, string comments, Guid evaluatedByUserId, DateTimeOffset evaluatedAtUtc)
        : base(tenantId)
    {
        SupplierId = Guard.AgainstEmpty(supplierId, nameof(supplierId));
        Score = Guard.AgainstOutOfRange(score, nameof(score), 0, 100);
        Comments = Guard.AgainstNullOrWhiteSpace(comments, nameof(comments), 1_000);
        EvaluatedByUserId = Guard.AgainstEmpty(evaluatedByUserId, nameof(evaluatedByUserId));
        EvaluatedAtUtc = evaluatedAtUtc;
    }

    public Guid SupplierId { get; private set; }

    public int Score { get; private set; }

    public string Comments { get; private set; }

    public Guid EvaluatedByUserId { get; private set; }

    public DateTimeOffset EvaluatedAtUtc { get; private set; }
}

public sealed class SupplierExpirationAlert : TenantEntity
{
    private SupplierExpirationAlert()
    {
    }

    public SupplierExpirationAlert(Guid tenantId, Guid supplierId, Guid supplierDocumentId, SupplierDocumentType documentType, DateTimeOffset expiresAtUtc, DateTimeOffset alertAtUtc)
        : base(tenantId)
    {
        SupplierId = Guard.AgainstEmpty(supplierId, nameof(supplierId));
        SupplierDocumentId = Guard.AgainstEmpty(supplierDocumentId, nameof(supplierDocumentId));
        DocumentType = documentType;
        ExpiresAtUtc = expiresAtUtc;
        AlertAtUtc = alertAtUtc;
        Status = SupplierAlertStatus.Open;
    }

    public Guid SupplierId { get; private set; }

    public Guid SupplierDocumentId { get; private set; }

    public SupplierDocumentType DocumentType { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset AlertAtUtc { get; private set; }

    public SupplierAlertStatus Status { get; private set; }

    public void Acknowledge()
    {
        Status = SupplierAlertStatus.Acknowledged;
    }
}
