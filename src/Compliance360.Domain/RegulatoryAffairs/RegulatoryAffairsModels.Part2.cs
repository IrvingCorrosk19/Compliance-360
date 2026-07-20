using Compliance360.Domain.Common;

namespace Compliance360.Domain.RegulatoryAffairs;

// ── Medical Device Product ───────────────────────────────────────────────────

public sealed class MedicalDeviceProduct : TenantEntity
{
    private MedicalDeviceProduct()
    {
        CountryCode = "PA";
        Category = string.Empty;
        Brand = string.Empty;
        RegulatoryName = string.Empty;
        CatalogCode = string.Empty;
        Currency = "USD";
    }

    public MedicalDeviceProduct(
        Guid tenantId,
        string countryCode,
        string category,
        string brand,
        string regulatoryName,
        string? commercialName,
        string? description,
        string catalogCode,
        string? internalCode,
        string? productType,
        DeviceRiskClass riskClass,
        Guid? manufacturerId,
        Guid? distributorCompanyId,
        string? initiative,
        string? priority,
        string? salesMarketingInput,
        decimal? opportunityAmount,
        string? currency,
        Guid createdByUserId)
        : base(tenantId)
    {
        CountryCode = Guard.AgainstNullOrWhiteSpace(countryCode, nameof(countryCode), 8).ToUpperInvariant();
        Category = Guard.AgainstNullOrWhiteSpace(category, nameof(category), 120);
        Brand = Guard.AgainstNullOrWhiteSpace(brand, nameof(brand), 120);
        RegulatoryName = Guard.AgainstNullOrWhiteSpace(regulatoryName, nameof(regulatoryName), 320);
        CommercialName = string.IsNullOrWhiteSpace(commercialName) ? null : commercialName.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        CatalogCode = Guard.AgainstNullOrWhiteSpace(catalogCode, nameof(catalogCode), 120).ToUpperInvariant();
        InternalCode = string.IsNullOrWhiteSpace(internalCode) ? null : internalCode.Trim();
        ProductType = string.IsNullOrWhiteSpace(productType) ? null : productType.Trim();
        RiskClass = riskClass;
        ManufacturerId = manufacturerId;
        DistributorCompanyId = distributorCompanyId;
        DistributorName = null;
        Initiative = string.IsNullOrWhiteSpace(initiative) ? null : initiative.Trim();
        Priority = string.IsNullOrWhiteSpace(priority) ? null : priority.Trim();
        SalesMarketingInput = string.IsNullOrWhiteSpace(salesMarketingInput) ? null : salesMarketingInput.Trim();
        OpportunityAmount = opportunityAmount;
        Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        IsActive = true;
        IsCommercializable = false;
        IsDeleted = false;
        RegisteredSuppliersCount = null;
        SourceLineNumber = null;
        TechnicalSheetReference = null;
        FormReference = null;
        TechnicalSheetStatus = RegulatoryArtifactStatus.Missing;
        FormStatus = RegulatoryArtifactStatus.Missing;
        TechnicalSheetDocumentId = null;
        TechnicalSheetStoredFileId = null;
        FormDocumentId = null;
        FormStoredFileId = null;
        TechnicalSheetUpdatedAtUtc = null;
        FormUpdatedAtUtc = null;
        TechnicalSheetUpdatedByUserId = null;
        FormUpdatedByUserId = null;
    }

    public string CountryCode { get; private set; }
    public string Category { get; private set; }
    public string Brand { get; private set; }
    public string RegulatoryName { get; private set; }
    public string? CommercialName { get; private set; }
    public string? Description { get; private set; }
    public string CatalogCode { get; private set; }
    public string? InternalCode { get; private set; }
    public string? ProductType { get; private set; }
    public DeviceRiskClass RiskClass { get; private set; }
    public Guid? ManufacturerId { get; private set; }
    public Guid? DistributorCompanyId { get; private set; }
    public string? DistributorName { get; private set; }
    public string? Initiative { get; private set; }
    public string? Priority { get; private set; }
    public string? SalesMarketingInput { get; private set; }
    public decimal? OpportunityAmount { get; private set; }
    public string Currency { get; private set; }
    public int? RegisteredSuppliersCount { get; private set; }
    public int? SourceLineNumber { get; private set; }
    /// <summary>REGUTRACK "Ficha Tecnica" registry/reference id (e.g. 102300).</summary>
    public string? TechnicalSheetReference { get; private set; }
    public RegulatoryArtifactStatus TechnicalSheetStatus { get; private set; }
    public Guid? TechnicalSheetDocumentId { get; private set; }
    public Guid? TechnicalSheetStoredFileId { get; private set; }
    public DateTimeOffset? TechnicalSheetUpdatedAtUtc { get; private set; }
    public Guid? TechnicalSheetUpdatedByUserId { get; private set; }
    /// <summary>REGUTRACK "Formulario" — authority submission form reference/code when present.</summary>
    public string? FormReference { get; private set; }
    public RegulatoryArtifactStatus FormStatus { get; private set; }
    public Guid? FormDocumentId { get; private set; }
    public Guid? FormStoredFileId { get; private set; }
    public DateTimeOffset? FormUpdatedAtUtc { get; private set; }
    public Guid? FormUpdatedByUserId { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsCommercializable { get; private set; }
    public bool IsDeleted { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public void UpdateCommercialFields(
        string category,
        string brand,
        string regulatoryName,
        string? commercialName,
        string? description,
        DeviceRiskClass riskClass,
        Guid? manufacturerId,
        Guid? distributorCompanyId,
        string? distributorName,
        string? initiative,
        string? priority,
        string? salesMarketingInput,
        decimal? opportunityAmount,
        string? currency,
        int? registeredSuppliersCount,
        string? technicalSheetReference,
        string? formReference)
    {
        Category = Guard.AgainstNullOrWhiteSpace(category, nameof(category), 120);
        Brand = Guard.AgainstNullOrWhiteSpace(brand, nameof(brand), 120);
        RegulatoryName = Guard.AgainstNullOrWhiteSpace(regulatoryName, nameof(regulatoryName), 320);
        CommercialName = string.IsNullOrWhiteSpace(commercialName) ? null : commercialName.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        RiskClass = riskClass;
        ManufacturerId = manufacturerId;
        DistributorCompanyId = distributorCompanyId;
        DistributorName = string.IsNullOrWhiteSpace(distributorName) ? null : distributorName.Trim();
        Initiative = string.IsNullOrWhiteSpace(initiative) ? null : initiative.Trim();
        Priority = string.IsNullOrWhiteSpace(priority) ? null : priority.Trim();
        SalesMarketingInput = string.IsNullOrWhiteSpace(salesMarketingInput) ? null : salesMarketingInput.Trim();
        OpportunityAmount = opportunityAmount;
        if (!string.IsNullOrWhiteSpace(currency))
        {
            Currency = currency.Trim().ToUpperInvariant();
        }

        RegisteredSuppliersCount = registeredSuppliersCount;
        TechnicalSheetReference = string.IsNullOrWhiteSpace(technicalSheetReference) ? null : technicalSheetReference.Trim();
        FormReference = string.IsNullOrWhiteSpace(formReference) ? null : formReference.Trim();
        if (!string.IsNullOrWhiteSpace(TechnicalSheetReference) && TechnicalSheetStatus == RegulatoryArtifactStatus.Missing)
        {
            TechnicalSheetStatus = RegulatoryArtifactStatus.Received;
        }

        if (!string.IsNullOrWhiteSpace(FormReference) && FormStatus == RegulatoryArtifactStatus.Missing)
        {
            FormStatus = RegulatoryArtifactStatus.Received;
        }
    }

    public void SetSourceLine(int? lineNumber) => SourceLineNumber = lineNumber;

    public void AttachTechnicalSheet(
        string? reference,
        Guid? documentId,
        Guid? storedFileId,
        RegulatoryArtifactStatus status,
        Guid userId,
        DateTimeOffset now)
    {
        if (!string.IsNullOrWhiteSpace(reference))
        {
            TechnicalSheetReference = reference.Trim();
        }

        if (TechnicalSheetDocumentId.HasValue && documentId.HasValue && TechnicalSheetDocumentId != documentId)
        {
            TechnicalSheetStatus = RegulatoryArtifactStatus.Superseded;
        }

        TechnicalSheetDocumentId = documentId ?? TechnicalSheetDocumentId;
        TechnicalSheetStoredFileId = storedFileId ?? TechnicalSheetStoredFileId;
        TechnicalSheetStatus = status;
        TechnicalSheetUpdatedAtUtc = now;
        TechnicalSheetUpdatedByUserId = userId;
    }

    public void AttachAuthorityForm(
        string? reference,
        Guid? documentId,
        Guid? storedFileId,
        RegulatoryArtifactStatus status,
        Guid userId,
        DateTimeOffset now)
    {
        if (!string.IsNullOrWhiteSpace(reference))
        {
            FormReference = reference.Trim();
        }

        if (FormDocumentId.HasValue && documentId.HasValue && FormDocumentId != documentId)
        {
            FormStatus = RegulatoryArtifactStatus.Superseded;
        }

        FormDocumentId = documentId ?? FormDocumentId;
        FormStoredFileId = storedFileId ?? FormStoredFileId;
        FormStatus = status;
        FormUpdatedAtUtc = now;
        FormUpdatedByUserId = userId;
    }

    public void SetCommercializable(bool value) => IsCommercializable = value && IsActive && !IsDeleted;

    public void SoftDelete(DateTimeOffset deletedAtUtc)
    {
        IsDeleted = true;
        IsActive = false;
        IsCommercializable = false;
        DeletedAtUtc = deletedAtUtc;
    }
}

// ── Sanitary Registration ────────────────────────────────────────────────────

public sealed class SanitaryRegistration : TenantEntity
{
    private SanitaryRegistration()
    {
        RegistrationNumber = string.Empty;
        RegistrationType = "CT/RS";
    }

    public SanitaryRegistration(
        Guid tenantId,
        Guid productId,
        Guid authorityId,
        string? registrationNumber,
        DateTimeOffset? issuedOn,
        DateTimeOffset? expiresOn,
        string? notes,
        Guid createdByUserId,
        bool activate)
        : base(tenantId)
    {
        RegistrationNumber = string.Empty;
        ProductId = Guard.AgainstEmpty(productId, nameof(productId));
        AuthorityId = Guard.AgainstEmpty(authorityId, nameof(authorityId));
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        IsCurrent = false;
        RegistrationType = "CT/RS";

        if (activate)
        {
            Activate(registrationNumber!, issuedOn!.Value, expiresOn, DateTimeOffset.UtcNow);
        }
        else
        {
            RegistrationNumber = string.IsNullOrWhiteSpace(registrationNumber) ? "PENDING" : registrationNumber.Trim();
            IssuedOn = issuedOn;
            ExpiresOn = expiresOn;
            Status = SanitaryRegistrationStatus.Draft;
            ValidateDates();
        }
    }

    public Guid ProductId { get; private set; }
    public Guid AuthorityId { get; private set; }
    public string RegistrationType { get; private set; }
    public string RegistrationNumber { get; private set; }
    public DateTimeOffset? IssuedOn { get; private set; }
    public DateTimeOffset? ExpiresOn { get; private set; }
    public SanitaryRegistrationStatus Status { get; private set; }
    public bool IsCurrent { get; private set; }
    public DateTimeOffset? RenewalDueOn { get; private set; }
    public string? Notes { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? ReplacesRegistrationId { get; private set; }

    public void Activate(string registrationNumber, DateTimeOffset issuedOn, DateTimeOffset? expiresOn, DateTimeOffset now)
    {
        RegistrationNumber = Guard.AgainstNullOrWhiteSpace(registrationNumber, nameof(registrationNumber), 120);
        IssuedOn = issuedOn;
        ExpiresOn = expiresOn;
        ValidateDates();
        Status = SanitaryRegistrationStatus.Active;
        IsCurrent = true;
        RenewalDueOn = expiresOn?.AddDays(-180);
        RefreshExpirationStatus(now);
    }

    public void MarkReplaced(Guid newRegistrationId)
    {
        IsCurrent = false;
        Status = SanitaryRegistrationStatus.Replaced;
        // newRegistrationId tracked by caller via ReplacesRegistrationId on the new entity
        _ = newRegistrationId;
    }

    public void SetReplaces(Guid previousId) => ReplacesRegistrationId = previousId;

    public void RefreshExpirationStatus(DateTimeOffset now)
    {
        if (Status is SanitaryRegistrationStatus.Cancelled or SanitaryRegistrationStatus.Replaced or SanitaryRegistrationStatus.Suspended or SanitaryRegistrationStatus.Draft)
        {
            return;
        }

        if (!ExpiresOn.HasValue)
        {
            return;
        }

        if (ExpiresOn.Value.Date < now.Date)
        {
            Status = SanitaryRegistrationStatus.Expired;
            IsCurrent = false;
        }
        else if (ExpiresOn.Value.Date <= now.Date.AddDays(90))
        {
            Status = SanitaryRegistrationStatus.Expiring;
        }
        else if (Status is SanitaryRegistrationStatus.Expiring or SanitaryRegistrationStatus.Expired or SanitaryRegistrationStatus.InProcess)
        {
            Status = SanitaryRegistrationStatus.Active;
        }
    }

    private void ValidateDates()
    {
        if (IssuedOn.HasValue && ExpiresOn.HasValue && ExpiresOn < IssuedOn)
        {
            throw new DomainException("Registration expiration cannot be before issuance.");
        }
    }
}

// ── Registration Dossier (aggregate root) ────────────────────────────────────

public sealed class RegistrationDossier : TenantEntity
{
    private readonly List<DossierMilestone> _milestones = [];
    private readonly List<DossierRequirement> _requirements = [];
    private readonly List<AuthorityObservation> _observations = [];
    private readonly List<DossierHistoryEvent> _history = [];

    private static readonly HashSet<(RegistrationDossierStatus From, RegistrationDossierStatus To)> AllowedTransitions =
    [
        (RegistrationDossierStatus.Draft, RegistrationDossierStatus.Planning),
        (RegistrationDossierStatus.Draft, RegistrationDossierStatus.Cancelled),
        (RegistrationDossierStatus.Planning, RegistrationDossierStatus.WaitingManufacturerDocuments),
        (RegistrationDossierStatus.Planning, RegistrationDossierStatus.OnHold),
        (RegistrationDossierStatus.Planning, RegistrationDossierStatus.Cancelled),
        (RegistrationDossierStatus.WaitingManufacturerDocuments, RegistrationDossierStatus.DocumentsReceived),
        (RegistrationDossierStatus.WaitingManufacturerDocuments, RegistrationDossierStatus.OnHold),
        (RegistrationDossierStatus.WaitingManufacturerDocuments, RegistrationDossierStatus.Cancelled),
        (RegistrationDossierStatus.DocumentsReceived, RegistrationDossierStatus.Assembling),
        (RegistrationDossierStatus.DocumentsReceived, RegistrationDossierStatus.Cancelled),
        (RegistrationDossierStatus.Assembling, RegistrationDossierStatus.ReadyForSubmission),
        (RegistrationDossierStatus.Assembling, RegistrationDossierStatus.UnderTechnicalReview),
        (RegistrationDossierStatus.Assembling, RegistrationDossierStatus.WaitingManufacturerDocuments),
        (RegistrationDossierStatus.Assembling, RegistrationDossierStatus.Cancelled),
        (RegistrationDossierStatus.UnderTechnicalReview, RegistrationDossierStatus.ReadyForSubmission),
        (RegistrationDossierStatus.UnderTechnicalReview, RegistrationDossierStatus.CorrectionRequested),
        (RegistrationDossierStatus.UnderTechnicalReview, RegistrationDossierStatus.Cancelled),
        (RegistrationDossierStatus.CorrectionRequested, RegistrationDossierStatus.UnderTechnicalReview),
        (RegistrationDossierStatus.CorrectionRequested, RegistrationDossierStatus.Cancelled),
        (RegistrationDossierStatus.ReadyForSubmission, RegistrationDossierStatus.ApprovedForSubmission),
        (RegistrationDossierStatus.ReadyForSubmission, RegistrationDossierStatus.Submitted), // only when tenant disables RequireInternalApprovalBeforeSubmission
        (RegistrationDossierStatus.ReadyForSubmission, RegistrationDossierStatus.Assembling),
        (RegistrationDossierStatus.ApprovedForSubmission, RegistrationDossierStatus.Submitted),
        (RegistrationDossierStatus.ApprovedForSubmission, RegistrationDossierStatus.ReadyForSubmission),
        (RegistrationDossierStatus.Submitted, RegistrationDossierStatus.UnderAuthorityReview),
        (RegistrationDossierStatus.UnderAuthorityReview, RegistrationDossierStatus.Observed),
        (RegistrationDossierStatus.UnderAuthorityReview, RegistrationDossierStatus.Approved),
        (RegistrationDossierStatus.UnderAuthorityReview, RegistrationDossierStatus.Rejected),
        (RegistrationDossierStatus.Observed, RegistrationDossierStatus.CorrectingObservation),
        (RegistrationDossierStatus.CorrectingObservation, RegistrationDossierStatus.Resubmitted),
        (RegistrationDossierStatus.CorrectingObservation, RegistrationDossierStatus.ResponseReady),
        (RegistrationDossierStatus.ResponseReady, RegistrationDossierStatus.Resubmitted),
        (RegistrationDossierStatus.Resubmitted, RegistrationDossierStatus.UnderAuthorityReview),
        (RegistrationDossierStatus.Approved, RegistrationDossierStatus.Closed),
        (RegistrationDossierStatus.Rejected, RegistrationDossierStatus.Closed),
        (RegistrationDossierStatus.Closed, RegistrationDossierStatus.Archived),
        (RegistrationDossierStatus.OnHold, RegistrationDossierStatus.Planning),
        (RegistrationDossierStatus.OnHold, RegistrationDossierStatus.WaitingManufacturerDocuments),
        (RegistrationDossierStatus.OnHold, RegistrationDossierStatus.Cancelled)
    ];

    private RegistrationDossier()
    {
        CaseNumber = string.Empty;
        Currency = "USD";
    }

    public RegistrationDossier(
        Guid tenantId,
        string caseNumber,
        Guid productId,
        Guid authorityId,
        RegistrationProcessType processType,
        Guid? existingRegistrationId,
        string? priority,
        Guid? regulatoryOwnerUserId,
        string? salesMarketingInput,
        decimal? opportunityAmount,
        string? currency,
        string? comments,
        Guid? requirementPackId,
        string? requirementPackVersionLabel,
        Guid createdByUserId)
        : base(tenantId)
    {
        CaseNumber = Guard.AgainstNullOrWhiteSpace(caseNumber, nameof(caseNumber), 60).ToUpperInvariant();
        ProductId = Guard.AgainstEmpty(productId, nameof(productId));
        AuthorityId = Guard.AgainstEmpty(authorityId, nameof(authorityId));
        ProcessType = processType;
        ExistingRegistrationId = existingRegistrationId;
        Priority = string.IsNullOrWhiteSpace(priority) ? null : priority.Trim();
        RegulatoryOwnerUserId = regulatoryOwnerUserId;
        SalesMarketingInput = string.IsNullOrWhiteSpace(salesMarketingInput) ? null : salesMarketingInput.Trim();
        OpportunityAmount = opportunityAmount;
        Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();
        Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
        RequirementPackId = requirementPackId;
        RequirementPackVersionLabel = requirementPackVersionLabel;
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        Status = RegistrationDossierStatus.Draft;
        Revision = 0;
        IsDeleted = false;
        SeedDefaultMilestones();
    }

    public string CaseNumber { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid AuthorityId { get; private set; }
    public Guid? ExistingRegistrationId { get; private set; }
    public Guid? ResultingRegistrationId { get; private set; }
    public RegistrationProcessType ProcessType { get; private set; }
    public RegistrationDossierStatus Status { get; private set; }
    public long Revision { get; private set; }
    public string? Priority { get; private set; }
    public Guid? RegulatoryOwnerUserId { get; private set; }
    public string? SalesMarketingInput { get; private set; }
    public decimal? OpportunityAmount { get; private set; }
    public string Currency { get; private set; }
    public string? Comments { get; private set; }
    public Guid? RequirementPackId { get; private set; }
    public string? RequirementPackVersionLabel { get; private set; }
    public Guid? WorkflowInstanceId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    /// <summary>User who granted internal clearance (ApprovedForSubmission). Distinct from authority approval.</summary>
    public Guid? InternallyApprovedByUserId { get; private set; }
    public DateTimeOffset? InternallyApprovedAtUtc { get; private set; }
    public Guid? SubmittedByUserId { get; private set; }
    public string? SubmissionProcedureNumber { get; private set; }
    public string? SubmissionExternalNumber { get; private set; }
    public Guid? SubmissionProofStoredFileId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? ClosedAtUtc { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    // Denormalized key dates (performance / REGUTRACK parity)
    public DateTimeOffset? RequestedFromFactoryOn { get; private set; }
    public DateTimeOffset? EstimatedReceptionOn { get; private set; }
    public DateTimeOffset? MaximumReceptionOn { get; private set; }
    public DateTimeOffset? ReceivedOn { get; private set; }
    public DateTimeOffset? AssembledOn { get; private set; }
    public DateTimeOffset? EstimatedSubmissionOn { get; private set; }
    public DateTimeOffset? SubmittedOn { get; private set; }
    public DateTimeOffset? ObservationReceivedOn { get; private set; }
    public DateTimeOffset? EstimatedApprovalOn { get; private set; }
    public DateTimeOffset? ApprovedOn { get; private set; }
    public DateTimeOffset? TargetExpirationOn { get; private set; }

    public IReadOnlyCollection<DossierMilestone> Milestones => _milestones.AsReadOnly();
    public IReadOnlyCollection<DossierRequirement> Requirements => _requirements.AsReadOnly();
    public IReadOnlyCollection<AuthorityObservation> Observations => _observations.AsReadOnly();
    public IReadOnlyCollection<DossierHistoryEvent> History => _history.AsReadOnly();

    public void EnsureExpectedRevision(long expectedRevision)
    {
        if (Revision != expectedRevision)
        {
            throw new DomainException($"Revision conflict. Expected {expectedRevision}, current {Revision}.");
        }
    }

    public void IncrementRevision() => Revision++;

    public void UpdateMetadataAndDates(
        string? priority,
        Guid? regulatoryOwnerUserId,
        string? salesMarketingInput,
        decimal? opportunityAmount,
        string? currency,
        string? comments,
        DateTimeOffset? requestedFromFactoryOn,
        DateTimeOffset? estimatedReceptionOn,
        DateTimeOffset? maximumReceptionOn,
        DateTimeOffset? estimatedSubmissionOn,
        DateTimeOffset? estimatedApprovalOn,
        DateTimeOffset? targetExpirationOn,
        string reason,
        bool controlledCorrection = false)
    {
        if (Status is not (RegistrationDossierStatus.Draft or RegistrationDossierStatus.Planning or RegistrationDossierStatus.Assembling)
            && !(controlledCorrection && Status == RegistrationDossierStatus.CorrectionRequested))
        {
            throw new DomainException("Dossier metadata can only be updated in Draft, Planning, Assembling or an authorized controlled correction.");
        }

        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason), 2000);
        Priority = string.IsNullOrWhiteSpace(priority) ? null : priority.Trim();
        RegulatoryOwnerUserId = regulatoryOwnerUserId;
        SalesMarketingInput = string.IsNullOrWhiteSpace(salesMarketingInput) ? null : salesMarketingInput.Trim();
        OpportunityAmount = opportunityAmount;
        Currency = string.IsNullOrWhiteSpace(currency) ? Currency : currency.Trim().ToUpperInvariant();
        Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
        UpdateKeyDates(requestedFromFactoryOn, estimatedReceptionOn, maximumReceptionOn, estimatedSubmissionOn, estimatedApprovalOn, targetExpirationOn);
    }

    public void ReopenForCorrection(DateTimeOffset now)
    {
        if (Status is not (RegistrationDossierStatus.Closed or RegistrationDossierStatus.Rejected or RegistrationDossierStatus.Cancelled))
        {
            throw new DomainException("Only Closed, Rejected or Cancelled dossiers can be reopened.");
        }

        Status = RegistrationDossierStatus.CorrectionRequested;
        ClosedAtUtc = null;
        RecordHistory("ReopenedForCorrection", "Dossier reopened through approved V2 request.", null, now);
    }

    public void Archive(DateTimeOffset now)
    {
        if (Status != RegistrationDossierStatus.Closed)
        {
            throw new DomainException("Only a Closed dossier can be archived.");
        }

        TransitionTo(RegistrationDossierStatus.Archived, now);
        IsDeleted = true;
        DeletedAtUtc = now;
    }

    public void RecordHistory(string eventType, string summary, Guid? userId, DateTimeOffset atUtc)
    {
        _history.Add(new DossierHistoryEvent(
            TenantId,
            Id,
            eventType,
            summary,
            userId,
            atUtc));
    }

    public void ApplyRequirementPack(RegulatoryRequirementPack pack)
    {
        if (Status is not (RegistrationDossierStatus.Draft or RegistrationDossierStatus.Planning))
        {
            throw new DomainException("Requirement pack can only be applied in Draft or Planning.");
        }

        if (pack.Status != RequirementPackStatus.Published)
        {
            throw new DomainException("Only published requirement packs can be applied.");
        }

        if (_requirements.Count > 0)
        {
            throw new DomainException("Dossier already has requirements. Use explicit reconciliation to change packs.");
        }

        RequirementPackId = pack.Id;
        RequirementPackVersionLabel = pack.VersionLabel;
        var order = 0;
        foreach (var def in pack.Definitions.OrderBy(d => d.Order))
        {
            _requirements.Add(new DossierRequirement(
                TenantId,
                Id,
                def.Id,
                def.Code,
                def.Name,
                def.Description,
                def.Category,
                def.IsRequired,
                def.IsCritical,
                order++,
                pack.Id,
                pack.VersionLabel));
        }
    }

    public void TransitionTo(RegistrationDossierStatus next, DateTimeOffset now)
    {
        if (!AllowedTransitions.Contains((Status, next)))
        {
            throw new DomainException($"Transition from {Status} to {next} is not allowed.");
        }

        if (next == RegistrationDossierStatus.Submitted || next == RegistrationDossierStatus.Resubmitted)
        {
            EnsureCriticalRequirementsReady();
        }

        if (next == RegistrationDossierStatus.Approved)
        {
            if (!SubmittedOn.HasValue && Status != RegistrationDossierStatus.UnderAuthorityReview && Status != RegistrationDossierStatus.Resubmitted)
            {
                // still allow from UnderAuthorityReview
            }

            ApprovedOn ??= now;
            UpsertMilestone(DossierMilestoneType.Approval, planned: null, actual: ApprovedOn);
        }

        if (next == RegistrationDossierStatus.Closed)
        {
            if (_observations.Any(o => o.Status is AuthorityObservationStatus.Open or AuthorityObservationStatus.InProgress or AuthorityObservationStatus.ResponseReady))
            {
                throw new DomainException("Cannot close a dossier with open observations.");
            }

            ClosedAtUtc = now;
        }

        if (next == RegistrationDossierStatus.Submitted)
        {
            SubmittedOn ??= now;
            UpsertMilestone(DossierMilestoneType.Submission, planned: EstimatedSubmissionOn, actual: SubmittedOn);
        }

        if (next == RegistrationDossierStatus.DocumentsReceived)
        {
            ReceivedOn ??= now;
            UpsertMilestone(DossierMilestoneType.Reception, planned: EstimatedReceptionOn, actual: ReceivedOn);
        }

        if (next == RegistrationDossierStatus.ReadyForSubmission)
        {
            AssembledOn ??= now;
            UpsertMilestone(DossierMilestoneType.DossierAssembly, planned: null, actual: AssembledOn);
            InternallyApprovedByUserId = null;
            InternallyApprovedAtUtc = null;
        }

        if (next == RegistrationDossierStatus.ApprovedForSubmission)
        {
            EnsureCriticalRequirementsReady();
        }

        Status = next;
        RecordHistory("StatusTransition", $"Transitioned to {next}", null, now);
    }

    public void MarkInternallyApproved(Guid userId, DateTimeOffset atUtc)
    {
        if (Status != RegistrationDossierStatus.ApprovedForSubmission)
        {
            throw new DomainException("Internal approval marker requires ApprovedForSubmission status.");
        }

        InternallyApprovedByUserId = Guard.AgainstEmpty(userId, nameof(userId));
        InternallyApprovedAtUtc = atUtc;
        RecordHistory("InternalApproval", "Dossier approved for submission (internal)", userId, atUtc);
    }

    public void RecordSubmission(
        string procedureNumber,
        string externalNumber,
        DateTimeOffset submittedOn,
        Guid proofStoredFileId,
        Guid userId)
    {
        if (Status != RegistrationDossierStatus.ApprovedForSubmission)
        {
            throw new DomainException("Only an internally approved dossier can be submitted.");
        }

        SubmissionProcedureNumber = Guard.AgainstNullOrWhiteSpace(
            procedureNumber,
            nameof(procedureNumber),
            120);
        SubmissionExternalNumber = Guard.AgainstNullOrWhiteSpace(
            externalNumber,
            nameof(externalNumber),
            120);
        SubmissionProofStoredFileId = Guard.AgainstEmpty(
            proofStoredFileId,
            nameof(proofStoredFileId));
        SubmittedByUserId = Guard.AgainstEmpty(userId, nameof(userId));
        SubmittedOn = submittedOn;
        RecordHistory(
            "AuthoritySubmission",
            $"Submission recorded. Procedure={SubmissionProcedureNumber}; External={SubmissionExternalNumber}; Proof={SubmissionProofStoredFileId:N}",
            userId,
            submittedOn);
    }

    public void RecordResubmission(
        string procedureNumber,
        string externalNumber,
        DateTimeOffset submittedOn,
        Guid proofStoredFileId,
        Guid userId)
    {
        if (Status is not (RegistrationDossierStatus.CorrectingObservation or RegistrationDossierStatus.ResponseReady))
        {
            throw new DomainException("Only a corrected authority response can be resubmitted.");
        }

        SubmissionProcedureNumber = Guard.AgainstNullOrWhiteSpace(procedureNumber, nameof(procedureNumber), 120);
        SubmissionExternalNumber = Guard.AgainstNullOrWhiteSpace(externalNumber, nameof(externalNumber), 120);
        SubmissionProofStoredFileId = Guard.AgainstEmpty(proofStoredFileId, nameof(proofStoredFileId));
        SubmittedByUserId = Guard.AgainstEmpty(userId, nameof(userId));
        SubmittedOn = submittedOn;
        RecordHistory(
            "AuthorityResubmission",
            $"Resubmission recorded. Procedure={SubmissionProcedureNumber}; External={SubmissionExternalNumber}; Proof={SubmissionProofStoredFileId:N}",
            userId,
            submittedOn);
    }

    public void MarkDocumentsReceivedWithoutEvidence(string waiverReason, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(waiverReason) || waiverReason.Trim().Length < 8)
        {
            throw new DomainException("Audited exception reason is required to mark documents received without evidence.");
        }

        Comments = $"{Comments}\n[WAIVER {userId:N} {DateTimeOffset.UtcNow:u}] {waiverReason.Trim()}".Trim();
        TransitionTo(RegistrationDossierStatus.DocumentsReceived, DateTimeOffset.UtcNow);
    }

    public void UpdateKeyDates(
        DateTimeOffset? requestedFromFactoryOn,
        DateTimeOffset? estimatedReceptionOn,
        DateTimeOffset? maximumReceptionOn,
        DateTimeOffset? estimatedSubmissionOn,
        DateTimeOffset? estimatedApprovalOn,
        DateTimeOffset? targetExpirationOn)
    {
        RequestedFromFactoryOn = requestedFromFactoryOn;
        EstimatedReceptionOn = estimatedReceptionOn;
        MaximumReceptionOn = maximumReceptionOn;
        EstimatedSubmissionOn = estimatedSubmissionOn;
        EstimatedApprovalOn = estimatedApprovalOn;
        TargetExpirationOn = targetExpirationOn;

        UpsertMilestone(DossierMilestoneType.FactoryRequest, requestedFromFactoryOn, requestedFromFactoryOn);
        UpsertMilestone(DossierMilestoneType.EstimatedReception, estimatedReceptionOn, null);
        UpsertMilestone(DossierMilestoneType.MaximumReception, maximumReceptionOn, null);
        UpsertMilestone(DossierMilestoneType.EstimatedSubmission, estimatedSubmissionOn, null);
        UpsertMilestone(DossierMilestoneType.EstimatedApproval, estimatedApprovalOn, null);
        UpsertMilestone(DossierMilestoneType.Expiration, targetExpirationOn, null);
    }

    public AuthorityObservation OpenObservation(string description, DateTimeOffset receivedOn, DateTimeOffset? dueOn, Guid? responsibleUserId)
    {
        if (Status is not (RegistrationDossierStatus.Submitted or RegistrationDossierStatus.UnderAuthorityReview or RegistrationDossierStatus.Resubmitted or RegistrationDossierStatus.Observed))
        {
            if (Status != RegistrationDossierStatus.CorrectingObservation)
            {
                TransitionTo(RegistrationDossierStatus.UnderAuthorityReview, receivedOn);
            }
        }

        var number = _observations.Count + 1;
        var obs = new AuthorityObservation(TenantId, Id, number, receivedOn, dueOn, description, responsibleUserId);
        _observations.Add(obs);
        ObservationReceivedOn = receivedOn;
        UpsertMilestone(DossierMilestoneType.Observation, dueOn, receivedOn);
        if (Status != RegistrationDossierStatus.Observed && Status != RegistrationDossierStatus.CorrectingObservation)
        {
            Status = RegistrationDossierStatus.Observed;
        }

        return obs;
    }

    public void AttachWorkflow(Guid workflowInstanceId) =>
        WorkflowInstanceId = Guard.AgainstEmpty(workflowInstanceId, nameof(workflowInstanceId));

    public void SetResultingRegistration(Guid registrationId) =>
        ResultingRegistrationId = Guard.AgainstEmpty(registrationId, nameof(registrationId));

    public DossierRequirement GetRequirement(Guid requirementId)
    {
        var req = _requirements.FirstOrDefault(r => r.Id == requirementId)
            ?? throw new DomainException("Requirement not found on dossier.");
        return req;
    }

    private void EnsureCriticalRequirementsReady()
    {
        var blockers = _requirements
            .Where(r => r.IsCritical && r.Status is not (DossierRequirementStatus.Accepted or DossierRequirementStatus.Waived or DossierRequirementStatus.NotRequired))
            .Select(r => r.Code)
            .ToList();
        if (blockers.Count > 0)
        {
            throw new DomainException($"Cannot submit: critical requirements incomplete: {string.Join(", ", blockers)}");
        }
    }

    private void SeedDefaultMilestones()
    {
        foreach (DossierMilestoneType type in Enum.GetValues(typeof(DossierMilestoneType)))
        {
            _milestones.Add(new DossierMilestone(TenantId, Id, type));
        }
    }

    private void UpsertMilestone(DossierMilestoneType type, DateTimeOffset? planned, DateTimeOffset? actual)
    {
        var ms = _milestones.FirstOrDefault(m => m.MilestoneType == type);
        if (ms is null)
        {
            ms = new DossierMilestone(TenantId, Id, type);
            _milestones.Add(ms);
        }

        ms.UpdateDates(planned, actual);
    }
}

public sealed class DossierMilestone : TenantEntity
{
    private DossierMilestone()
    {
    }

    public DossierMilestone(Guid tenantId, Guid dossierId, DossierMilestoneType milestoneType)
        : base(tenantId)
    {
        DossierId = Guard.AgainstEmpty(dossierId, nameof(dossierId));
        MilestoneType = milestoneType;
        Status = MilestoneCompletionStatus.Planned;
    }

    public Guid DossierId { get; private set; }
    public DossierMilestoneType MilestoneType { get; private set; }
    public DateTimeOffset? PlannedDate { get; private set; }
    public DateTimeOffset? ActualDate { get; private set; }
    public MilestoneCompletionStatus Status { get; private set; }
    public Guid? ResponsibleUserId { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public void UpdateDates(DateTimeOffset? planned, DateTimeOffset? actual)
    {
        if (planned.HasValue)
        {
            PlannedDate = planned;
        }

        if (actual.HasValue)
        {
            ActualDate = actual;
            Status = MilestoneCompletionStatus.Completed;
            CompletedAtUtc = actual;
        }
    }

    public void Assign(Guid? userId, string? notes)
    {
        ResponsibleUserId = userId;
        Notes = string.IsNullOrWhiteSpace(notes) ? Notes : notes.Trim();
    }
}

public sealed class DossierRequirement : TenantEntity
{
    private DossierRequirement()
    {
        Code = string.Empty;
        Name = string.Empty;
        Category = "General";
    }

    public DossierRequirement(
        Guid tenantId,
        Guid dossierId,
        Guid? requirementDefinitionId,
        string code,
        string name,
        string? description,
        string category,
        bool isRequired,
        bool isCritical,
        int order,
        Guid? sourcePackId,
        string? sourcePackVersion)
        : base(tenantId)
    {
        DossierId = Guard.AgainstEmpty(dossierId, nameof(dossierId));
        RequirementDefinitionId = requirementDefinitionId;
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 80).ToUpperInvariant();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 220);
        Description = description;
        Category = Guard.AgainstNullOrWhiteSpace(category, nameof(category), 80);
        IsRequired = isRequired;
        IsCritical = isCritical;
        Order = order;
        SourceRequirementPackId = sourcePackId;
        SourceRequirementPackVersionLabel = sourcePackVersion;
        Status = isRequired ? DossierRequirementStatus.Pending : DossierRequirementStatus.NotRequired;
        ValidationStatus = RequirementValidationStatus.NotValidated;
    }

    public Guid DossierId { get; private set; }
    public Guid? RequirementDefinitionId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Category { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsCritical { get; private set; }
    public DossierRequirementStatus Status { get; private set; }
    public Guid? ResponsibleUserId { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public DateTimeOffset? CompletedOn { get; private set; }
    public RequirementValidationStatus ValidationStatus { get; private set; }
    public string? ValidationNotes { get; private set; }
    public Guid? CurrentDocumentId { get; private set; }
    public Guid? StoredFileId { get; private set; }
    public int Order { get; private set; }
    public Guid? SourceRequirementPackId { get; private set; }
    public string? SourceRequirementPackVersionLabel { get; private set; }
    public Guid? LastStatusChangedByUserId { get; private set; }

    public void AttachFile(Guid? documentId, Guid? storedFileId, DateTimeOffset now)
    {
        CurrentDocumentId = documentId;
        StoredFileId = storedFileId;
        if (Status is DossierRequirementStatus.Pending or DossierRequirementStatus.Requested)
        {
            Status = DossierRequirementStatus.Received;
        }

        CompletedOn ??= now;
    }

    public void ClearEvidence(Guid actorUserId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 8)
        {
            throw new DomainException("An audited reason of at least 8 characters is required to remove evidence.");
        }

        if (Status is DossierRequirementStatus.Accepted or DossierRequirementStatus.Waived)
        {
            throw new DomainException("Evidence cannot be removed after the requirement has been accepted or waived.");
        }

        if (!StoredFileId.HasValue && !CurrentDocumentId.HasValue)
        {
            throw new DomainException("This requirement has no evidence to remove.");
        }

        StoredFileId = null;
        CurrentDocumentId = null;
        CompletedOn = null;
        LastStatusChangedByUserId = actorUserId;
        ValidationNotes = string.IsNullOrWhiteSpace(ValidationNotes)
            ? $"Evidence removed: {reason.Trim()}"
            : $"{ValidationNotes.Trim()} | Evidence removed: {reason.Trim()}";
        if (Status == DossierRequirementStatus.Received)
        {
            Status = DossierRequirementStatus.Pending;
            ValidationStatus = RequirementValidationStatus.NotValidated;
        }
    }

    public void SetStatus(DossierRequirementStatus status, string? notes, Guid? userId)
    {
        if (status == DossierRequirementStatus.Waived && string.IsNullOrWhiteSpace(notes))
        {
            throw new DomainException("Waiver requires notes.");
        }

        Status = status;
        LastStatusChangedByUserId = userId;
        ValidationNotes = string.IsNullOrWhiteSpace(notes) ? ValidationNotes : notes.Trim();
        ResponsibleUserId = userId ?? ResponsibleUserId;
        if (status is DossierRequirementStatus.Accepted or DossierRequirementStatus.Waived)
        {
            ValidationStatus = RequirementValidationStatus.Valid;
            CompletedOn ??= DateTimeOffset.UtcNow;
        }
    }
}

public sealed class AuthorityObservation : TenantEntity
{
    private readonly List<AuthorityObservationRequirement> _linkedRequirements = [];

    private AuthorityObservation()
    {
        Description = string.Empty;
    }

    public AuthorityObservation(
        Guid tenantId,
        Guid dossierId,
        int observationNumber,
        DateTimeOffset receivedOn,
        DateTimeOffset? dueOn,
        string description,
        Guid? responsibleUserId)
        : base(tenantId)
    {
        DossierId = Guard.AgainstEmpty(dossierId, nameof(dossierId));
        ObservationNumber = Guard.AgainstOutOfRange(observationNumber, nameof(observationNumber), 1, 1000);
        ReceivedOn = receivedOn;
        DueOn = dueOn;
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), 4000);
        ResponsibleUserId = responsibleUserId;
        Status = AuthorityObservationStatus.Open;
    }

    public Guid DossierId { get; private set; }
    public int ObservationNumber { get; private set; }
    public DateTimeOffset ReceivedOn { get; private set; }
    public DateTimeOffset? DueOn { get; private set; }
    public string Description { get; private set; }
    public AuthorityObservationStatus Status { get; private set; }
    public Guid? ResponsibleUserId { get; private set; }
    public DateTimeOffset? ResponseSubmittedOn { get; private set; }
    public DateTimeOffset? ClosedOn { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<AuthorityObservationRequirement> LinkedRequirements => _linkedRequirements.AsReadOnly();

    public void LinkRequirement(Guid requirementId)
    {
        if (_linkedRequirements.Any(l => l.RequirementId == requirementId))
        {
            return;
        }

        _linkedRequirements.Add(new AuthorityObservationRequirement(TenantId, Id, requirementId));
    }

    public void SubmitResponse(string notes, DateTimeOffset now)
    {
        Notes = string.IsNullOrWhiteSpace(notes) ? Notes : notes.Trim();
        ResponseSubmittedOn = now;
        Status = AuthorityObservationStatus.Submitted;
    }

    public void Close(DateTimeOffset now)
    {
        Status = AuthorityObservationStatus.Closed;
        ClosedOn = now;
    }
}

public sealed class AuthorityObservationRequirement : TenantEntity
{
    private AuthorityObservationRequirement()
    {
    }

    public AuthorityObservationRequirement(Guid tenantId, Guid observationId, Guid requirementId)
        : base(tenantId)
    {
        ObservationId = Guard.AgainstEmpty(observationId, nameof(observationId));
        RequirementId = Guard.AgainstEmpty(requirementId, nameof(requirementId));
    }

    public Guid ObservationId { get; private set; }
    public Guid RequirementId { get; private set; }
}

/// <summary>Tipado equivalente a columnas Fecha de Actualización1..N del Excel REGUTRACK.</summary>
public sealed class DossierHistoryEvent : TenantEntity
{
    private DossierHistoryEvent()
    {
        EventType = string.Empty;
        Summary = string.Empty;
    }

    public DossierHistoryEvent(
        Guid tenantId,
        Guid dossierId,
        string eventType,
        string summary,
        Guid? actorUserId,
        DateTimeOffset occurredAtUtc)
        : base(tenantId)
    {
        DossierId = Guard.AgainstEmpty(dossierId, nameof(dossierId));
        EventType = Guard.AgainstNullOrWhiteSpace(eventType, nameof(eventType), 80);
        Summary = Guard.AgainstNullOrWhiteSpace(summary, nameof(summary), 2000);
        ActorUserId = actorUserId;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid DossierId { get; private set; }
    public string EventType { get; private set; }
    public string Summary { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}

/// <summary>Checklist de renovación de licencias según hoja CTT LICENCIAS OP.</summary>
public static class LicenseOpRequirementCatalog
{
    public static IReadOnlyList<(string Code, string Name, bool Required)> Items { get; } =
    [
        ("SOLICITUD", "Solicitud", true),
        ("CEDULA_REP_LEGAL", "Cédula de Rep. Legal", true),
        ("CEDULA_REGENTE", "Cédula Regente", true),
        ("REGISTRO_PUBLICO", "Registro Público", true),
        ("AVISO_OPERACIONES", "Aviso de Operaciones", true),
        ("LIC_FARMACIA_DROGAS", "Licencia de operaciones de Farmacias y Drogas", false),
        ("TIMBRES", "Timbres", true),
        ("TASA", "Tasa", true),
        ("DECLARACION_JURADA", "Declaración Jurada", true),
        ("CROQUIS", "Croquis o mapa de ubicación", true),
        ("LISTA_DM", "Lista de Dispositivos médicos que se Comercializan", true),
        ("CONTRATO_3ERO", "Contrato con el 3ero para almacenamiento", false),
        ("AUT_ACONDICIONAMIENTO", "Autorización para acondicionamiento de Dispositivos Médicos", false),
        ("LIC_OPS_DM", "Licencia de Operaciones de Dispositivos Médicos", true),
        ("SOLVENCIA", "Carta de Solvencia Económica", true),
        ("CATALOGO_MINSA", "Catálogo del Minsa", false),
        ("OFERENTE", "Certificado de Oferente", true),
        ("PLATFORM_UPDATE", "Actualizar plataforma Panamá Digital / FADDI / oferente (manual)", true)
    ];
}
