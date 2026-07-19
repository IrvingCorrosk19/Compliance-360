using Compliance360.Domain.Common;

namespace Compliance360.Domain.RegulatoryAffairs;

// ── Corporate Operating Licenses ─────────────────────────────────────────────

public sealed class OperatingLicense : TenantEntity
{
    private OperatingLicense()
    {
        LicenseType = string.Empty;
        CompanyName = string.Empty;
    }

    public OperatingLicense(
        Guid tenantId,
        string companyName,
        Guid? companyId,
        string licenseType,
        Guid? authorityId,
        string? licenseNumber,
        DateTimeOffset? issuedOn,
        DateTimeOffset? expiresOn,
        string? comments,
        Guid createdByUserId,
        DateOnly? companyConstitutedOn = null,
        DateOnly? operationsStartedOn = null)
        : base(tenantId)
    {
        CompanyName = Guard.AgainstNullOrWhiteSpace(companyName, nameof(companyName), 180);
        CompanyId = companyId;
        LicenseType = Guard.AgainstNullOrWhiteSpace(licenseType, nameof(licenseType), 220);
        AuthorityId = authorityId;
        LicenseNumber = string.IsNullOrWhiteSpace(licenseNumber) ? null : licenseNumber.Trim();
        IssuedOn = issuedOn;
        ExpiresOn = expiresOn;
        Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        Status = OperatingLicenseStatus.Active;
        SetCompanyCorporateDates(companyConstitutedOn, operationsStartedOn);
        RefreshExpirationStatus(DateTimeOffset.UtcNow);
    }

    public string CompanyName { get; private set; }
    public Guid? CompanyId { get; private set; }
    /// <summary>REGUTRACK "Fecha de Constitución" — company corporate date, not license IssuedOn.</summary>
    public DateOnly? CompanyConstitutedOn { get; private set; }
    /// <summary>REGUTRACK "Inicio de Operaciones" — company ops start, not license IssuedOn.</summary>
    public DateOnly? OperationsStartedOn { get; private set; }
    public string LicenseType { get; private set; }
    public Guid? AuthorityId { get; private set; }
    public string? LicenseNumber { get; private set; }
    public DateTimeOffset? IssuedOn { get; private set; }
    public DateTimeOffset? ExpiresOn { get; private set; }
    public OperatingLicenseStatus Status { get; private set; }
    public string? Comments { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? ActiveRenewalCaseId { get; private set; }

    public void SetCompanyCorporateDates(DateOnly? companyConstitutedOn, DateOnly? operationsStartedOn)
    {
        // Independent semantics; either may be null. Do not overwrite one when only the other is supplied.
        if (companyConstitutedOn.HasValue)
        {
            CompanyConstitutedOn = companyConstitutedOn;
        }

        if (operationsStartedOn.HasValue)
        {
            OperationsStartedOn = operationsStartedOn;
        }
    }

    public void ClearCompanyCorporateDates(bool clearConstitution, bool clearOperationsStart)
    {
        if (clearConstitution)
        {
            CompanyConstitutedOn = null;
        }

        if (clearOperationsStart)
        {
            OperationsStartedOn = null;
        }
    }

    public void RefreshExpirationStatus(DateTimeOffset now)
    {
        if (Status is OperatingLicenseStatus.Cancelled or OperatingLicenseStatus.Suspended or OperatingLicenseStatus.InRenewal)
        {
            return;
        }

        if (!ExpiresOn.HasValue)
        {
            Status = OperatingLicenseStatus.Active;
            return;
        }

        if (ExpiresOn.Value.Date < now.Date)
        {
            Status = OperatingLicenseStatus.Expired;
        }
        else if (ExpiresOn.Value.Date <= now.Date.AddDays(90))
        {
            Status = OperatingLicenseStatus.Expiring;
        }
        else
        {
            Status = OperatingLicenseStatus.Active;
        }
    }

    public void AttachRenewalCase(Guid caseId)
    {
        ActiveRenewalCaseId = caseId;
        Status = OperatingLicenseStatus.InRenewal;
    }
}

public sealed class LicenseRenewalCase : TenantEntity
{
    private readonly List<LicenseRequirement> _requirements = [];
    private readonly List<LicenseMilestone> _milestones = [];

    private LicenseRenewalCase()
    {
        CaseNumber = string.Empty;
    }

    public LicenseRenewalCase(
        Guid tenantId,
        Guid operatingLicenseId,
        string caseNumber,
        Guid? ownerUserId,
        string? comments,
        Guid createdByUserId)
        : base(tenantId)
    {
        OperatingLicenseId = Guard.AgainstEmpty(operatingLicenseId, nameof(operatingLicenseId));
        CaseNumber = Guard.AgainstNullOrWhiteSpace(caseNumber, nameof(caseNumber), 60).ToUpperInvariant();
        OwnerUserId = ownerUserId;
        Comments = comments;
        CreatedByUserId = createdByUserId;
        Status = LicenseRenewalCaseStatus.Draft;
        _milestones.Add(new LicenseMilestone(tenantId, Id, "Assembly"));
        _milestones.Add(new LicenseMilestone(tenantId, Id, "Submission"));
        _milestones.Add(new LicenseMilestone(tenantId, Id, "Approval"));
        _milestones.Add(new LicenseMilestone(tenantId, Id, "ManualPlatformUpdate"));
    }

    public Guid OperatingLicenseId { get; private set; }
    public string CaseNumber { get; private set; }
    public LicenseRenewalCaseStatus Status { get; private set; }
    public Guid? OwnerUserId { get; private set; }
    public string? Comments { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset? AssembledOn { get; private set; }
    public DateTimeOffset? SubmittedOn { get; private set; }
    public DateTimeOffset? ApprovedOn { get; private set; }
    public string? ManualPlatformTaskNotes { get; private set; }

    public IReadOnlyCollection<LicenseRequirement> Requirements => _requirements.AsReadOnly();
    public IReadOnlyCollection<LicenseMilestone> Milestones => _milestones.AsReadOnly();

    public void AddRequirement(string code, string name, bool isRequired)
    {
        _requirements.Add(new LicenseRequirement(TenantId, Id, code, name, isRequired));
    }

    public void MarkManualPlatformUpdate(string notes)
    {
        ManualPlatformTaskNotes = Guard.AgainstNullOrWhiteSpace(notes, nameof(notes), 2000);
        Status = LicenseRenewalCaseStatus.ManualPlatformUpdatePending;
    }

    public void Transition(LicenseRenewalCaseStatus next, DateTimeOffset now)
    {
        Status = next;
        if (next == LicenseRenewalCaseStatus.Submitted)
        {
            SubmittedOn = now;
        }

        if (next == LicenseRenewalCaseStatus.Approved)
        {
            ApprovedOn = now;
        }

        if (next == LicenseRenewalCaseStatus.Assembling)
        {
            AssembledOn ??= now;
        }
    }
}

public sealed class LicenseRequirement : TenantEntity
{
    private LicenseRequirement()
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    public LicenseRequirement(Guid tenantId, Guid caseId, string code, string name, bool isRequired)
        : base(tenantId)
    {
        LicenseRenewalCaseId = caseId;
        Code = Guard.AgainstNullOrWhiteSpace(code, nameof(code), 80).ToUpperInvariant();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 220);
        IsRequired = isRequired;
        Status = DossierRequirementStatus.Pending;
    }

    public Guid LicenseRenewalCaseId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public bool IsRequired { get; private set; }
    public DossierRequirementStatus Status { get; private set; }
    public Guid? StoredFileId { get; private set; }

    public void Complete(Guid? storedFileId)
    {
        StoredFileId = storedFileId;
        Status = DossierRequirementStatus.Accepted;
    }
}

public sealed class LicenseMilestone : TenantEntity
{
    private LicenseMilestone()
    {
        Name = string.Empty;
    }

    public LicenseMilestone(Guid tenantId, Guid caseId, string name)
        : base(tenantId)
    {
        LicenseRenewalCaseId = caseId;
        Name = name;
        Status = MilestoneCompletionStatus.Planned;
    }

    public Guid LicenseRenewalCaseId { get; private set; }
    public string Name { get; private set; }
    public DateTimeOffset? PlannedDate { get; private set; }
    public DateTimeOffset? ActualDate { get; private set; }
    public MilestoneCompletionStatus Status { get; private set; }
}

// ── Import staging ───────────────────────────────────────────────────────────

public sealed class RegutrackImportJob : TenantEntity
{
    private RegutrackImportJob()
    {
        SourceFileName = string.Empty;
    }

    public RegutrackImportJob(Guid tenantId, string sourceFileName, Guid uploadedByUserId, string? stagingPayloadJson)
        : base(tenantId)
    {
        SourceFileName = Guard.AgainstNullOrWhiteSpace(sourceFileName, nameof(sourceFileName), 260);
        UploadedByUserId = Guard.AgainstEmpty(uploadedByUserId, nameof(uploadedByUserId));
        StagingPayloadJson = stagingPayloadJson;
        Status = RegutrackImportJobStatus.Uploaded;
        WarningCount = 0;
        ErrorCount = 0;
        ImportedRowCount = 0;
    }

    public string SourceFileName { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public RegutrackImportJobStatus Status { get; private set; }
    public string? StagingPayloadJson { get; private set; }
    public string? ValidationReportJson { get; private set; }
    public int WarningCount { get; private set; }
    public int ErrorCount { get; private set; }
    public int ImportedRowCount { get; private set; }
    public DateTimeOffset? CommittedAtUtc { get; private set; }

    public void SetValidation(string reportJson, int warnings, int errors)
    {
        ValidationReportJson = reportJson;
        WarningCount = warnings;
        ErrorCount = errors;
        Status = errors > 0 ? RegutrackImportJobStatus.Failed : RegutrackImportJobStatus.Validated;
    }

    public void MarkSimulated() => Status = RegutrackImportJobStatus.Simulated;

    public void MarkCommitted(int importedRows, DateTimeOffset now)
    {
        ImportedRowCount = importedRows;
        CommittedAtUtc = now;
        Status = RegutrackImportJobStatus.Committed;
    }

    public void MarkRolledBack(string reason)
    {
        if (Status is not (RegutrackImportJobStatus.Simulated or RegutrackImportJobStatus.Validated or RegutrackImportJobStatus.Failed))
        {
            throw new DomainException("Only simulated, validated or failed import jobs can be rolled back.");
        }

        var safeReason = string.IsNullOrWhiteSpace(reason) ? "operator rollback" : reason.Trim();
        if (safeReason.Length > 500)
        {
            safeReason = safeReason[..500];
        }

        // Column is JSONB — never concatenate free text or Postgres rejects the update.
        ValidationReportJson = $"{{\"rolledBack\":true,\"reason\":{System.Text.Json.JsonSerializer.Serialize(safeReason)},\"previous\":{ValidationReportJson ?? "null"}}}";
        Status = RegutrackImportJobStatus.RolledBack;
        StagingPayloadJson = null;
    }
}

public sealed class RegutrackImportRow : TenantEntity
{
    private RegutrackImportRow()
    {
        SheetName = string.Empty;
        RawJson = "{}";
    }

    public RegutrackImportRow(
        Guid tenantId,
        Guid jobId,
        string sheetName,
        int rowNumber,
        string rawJson,
        string? normalizedJson,
        string? errorMessage)
        : base(tenantId)
    {
        JobId = jobId;
        SheetName = sheetName;
        RowNumber = rowNumber;
        RawJson = rawJson;
        NormalizedJson = normalizedJson;
        ErrorMessage = errorMessage;
        IsValid = string.IsNullOrWhiteSpace(errorMessage);
    }

    public Guid JobId { get; private set; }
    public string SheetName { get; private set; }
    public int RowNumber { get; private set; }
    public string RawJson { get; private set; }
    public string? NormalizedJson { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsValid { get; private set; }
    public Guid? CreatedProductId { get; private set; }
    public Guid? CreatedDossierId { get; private set; }

    public void LinkCreated(Guid? productId, Guid? dossierId)
    {
        CreatedProductId = productId;
        CreatedDossierId = dossierId;
    }

    public void MarkFailed(string errorMessage)
    {
        IsValid = false;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Commit failed." : errorMessage.Trim();
        if (ErrorMessage.Length > 2000)
        {
            ErrorMessage = ErrorMessage[..2000];
        }
    }
}

/// <summary>Configurable alert thresholds per tenant (days before expiry).</summary>
public sealed class RegulatoryAlertSettings : TenantEntity
{
    private RegulatoryAlertSettings()
    {
        ThresholdsCsv = "90,60,30,15,7,1,0";
    }

    public RegulatoryAlertSettings(Guid tenantId, string thresholdsCsv)
        : base(tenantId)
    {
        ThresholdsCsv = string.IsNullOrWhiteSpace(thresholdsCsv) ? "90,60,30,15,7,1,0" : thresholdsCsv.Trim();
    }

    public string ThresholdsCsv { get; private set; }

    public IReadOnlyList<int> ThresholdsDays =>
        ThresholdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var n) ? n : 0)
            .Where(n => n >= 0)
            .Distinct()
            .OrderByDescending(n => n)
            .ToArray();

    public void UpdateThresholds(string thresholdsCsv)
    {
        var parsed = (thresholdsCsv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var days) ? days : -1)
            .ToArray();
        if (parsed.Length == 0 || parsed.Any(days => days < 0 || days > 3650))
        {
            throw new DomainException("Alert thresholds must be comma-separated day values between 0 and 3650.");
        }

        ThresholdsCsv = string.Join(",", parsed.Distinct().OrderByDescending(days => days));
        MarkUpdated(DateTimeOffset.UtcNow);
    }
}

public sealed class RegulatoryAlertLog : TenantEntity
{
    private RegulatoryAlertLog()
    {
        AlertType = string.Empty;
        EntityName = string.Empty;
        Channel = "InApp";
    }

    public RegulatoryAlertLog(
        Guid tenantId,
        string alertType,
        string entityName,
        Guid entityId,
        int daysRemaining,
        Guid? recipientUserId,
        string channel)
        : base(tenantId)
    {
        AlertType = alertType;
        EntityName = entityName;
        EntityId = entityId;
        DaysRemaining = daysRemaining;
        RecipientUserId = recipientUserId;
        Channel = channel;
        DeliveredAtUtc = DateTimeOffset.UtcNow;
        Success = true;
    }

    public string AlertType { get; private set; }
    public string EntityName { get; private set; }
    public Guid EntityId { get; private set; }
    public int DaysRemaining { get; private set; }
    public Guid? RecipientUserId { get; private set; }
    public string Channel { get; private set; }
    public DateTimeOffset DeliveredAtUtc { get; private set; }
    public bool Success { get; private set; }
}

/// <summary>Tenant-configurable Regulatory Affairs segregation-of-duties controls.</summary>
public sealed class RegulatorySoDSettings : TenantEntity
{
    private RegulatorySoDSettings()
    {
        ApplyDefaults();
    }

    public RegulatorySoDSettings(Guid tenantId)
        : base(tenantId)
    {
        ApplyDefaults();
    }

    public bool PreventSelfReview { get; private set; }
    public bool PreventSelfApproval { get; private set; }
    public bool SeparateApproverAndSubmitter { get; private set; }
    public bool SeparateDocumentUploaderAndReviewer { get; private set; }
    public bool RequireSecondApprovalForCriticalWaiver { get; private set; }
    public bool RequireApprovalForCriticalityChange { get; private set; }
    public bool RequireApprovalForExternalDecisionRecording { get; private set; }
    public bool AllowEmergencyOverride { get; private set; }
    public bool EmergencyOverrideRequiresReason { get; private set; }
    public bool EmergencyOverrideRequiresSecondaryReview { get; private set; }
    public bool RequireInternalApprovalBeforeSubmission { get; private set; }

    public static RegulatorySoDSettings CreateRegulatedDefaults(Guid tenantId) => new(tenantId);

    public void Update(
        bool preventSelfReview,
        bool preventSelfApproval,
        bool separateApproverAndSubmitter,
        bool separateDocumentUploaderAndReviewer,
        bool requireSecondApprovalForCriticalWaiver,
        bool requireApprovalForCriticalityChange,
        bool requireApprovalForExternalDecisionRecording,
        bool allowEmergencyOverride,
        bool emergencyOverrideRequiresReason,
        bool emergencyOverrideRequiresSecondaryReview,
        bool requireInternalApprovalBeforeSubmission)
    {
        PreventSelfReview = preventSelfReview;
        PreventSelfApproval = preventSelfApproval;
        SeparateApproverAndSubmitter = separateApproverAndSubmitter;
        SeparateDocumentUploaderAndReviewer = separateDocumentUploaderAndReviewer;
        RequireSecondApprovalForCriticalWaiver = requireSecondApprovalForCriticalWaiver;
        RequireApprovalForCriticalityChange = requireApprovalForCriticalityChange;
        RequireApprovalForExternalDecisionRecording = requireApprovalForExternalDecisionRecording;
        AllowEmergencyOverride = allowEmergencyOverride;
        EmergencyOverrideRequiresReason = emergencyOverrideRequiresReason;
        EmergencyOverrideRequiresSecondaryReview = emergencyOverrideRequiresSecondaryReview;
        RequireInternalApprovalBeforeSubmission = requireInternalApprovalBeforeSubmission;
    }

    private void ApplyDefaults()
    {
        PreventSelfReview = true;
        PreventSelfApproval = true;
        SeparateApproverAndSubmitter = true;
        SeparateDocumentUploaderAndReviewer = true;
        RequireSecondApprovalForCriticalWaiver = true;
        RequireApprovalForCriticalityChange = true;
        RequireApprovalForExternalDecisionRecording = false;
        AllowEmergencyOverride = true;
        EmergencyOverrideRequiresReason = true;
        EmergencyOverrideRequiresSecondaryReview = true;
        RequireInternalApprovalBeforeSubmission = true;
    }
}
