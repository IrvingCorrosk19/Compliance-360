using Compliance360.Domain.RegulatoryAffairs;
using Compliance360.Shared;

namespace Compliance360.Application.RegulatoryAffairs;

public interface IRegulatoryAffairsService
{
    Task<Result<IReadOnlyCollection<AuthorityDto>>> EnsureDefaultAuthoritiesAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<AuthorityDto>>> ListAuthoritiesAsync(Guid tenantId, CancellationToken ct = default);

    Task<Result<ManufacturerDto>> UpsertManufacturerAsync(UpsertManufacturerCommand command, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<ManufacturerDto>>> SearchManufacturersAsync(Guid tenantId, string? search, CancellationToken ct = default);
    Task<Result<ManufacturerCertificateDto>> AddCertificateAsync(AddManufacturerCertificateCommand command, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<ManufacturerCertificateDto>>> ListCertificatesAsync(Guid tenantId, Guid? manufacturerId, CancellationToken ct = default);

    Task<Result<ProductDto>> CreateProductAsync(CreateMedicalDeviceProductCommand command, CancellationToken ct = default);
    Task<Result<ProductDto>> UpdateProductAsync(UpdateProductCommand command, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<ProductDto>>> SearchProductsAsync(ProductSearchQuery query, CancellationToken ct = default);
    Task<Result<ProductDto>> GetProductAsync(Guid tenantId, Guid productId, CancellationToken ct = default);

    Task<Result<RequirementPackDto>> EnsureDefaultRequirementPackAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<RequirementPackDto>>> ListRequirementPacksAsync(Guid tenantId, CancellationToken ct = default);

    Task<Result<DossierDetailDto>> CreateDossierAsync(CreateDossierCommand command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> GetDossierAsync(Guid tenantId, Guid dossierId, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<DossierSummaryDto>>> SearchDossiersAsync(DossierSearchQuery query, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> TransitionDossierAsync(TransitionDossierCommand command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> ApproveForSubmissionAsync(ApproveForSubmissionCommand command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> SubmitDossierAsync(SubmitDossierCommand command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> ResubmitDossierAsync(ResubmitDossierCommand command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> StartAuthorityReviewAsync(StartAuthorityReviewCommand command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> RejectDossierAsync(RejectDossierCommand command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> UpdateDossierDatesAsync(UpdateDossierDatesCommand command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> UpdateRequirementAsync(UpdateRequirementCommand command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> OpenObservationAsync(OpenObservationCommand command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> RespondObservationAsync(RespondObservationCommand command, CancellationToken ct = default);
    Task<Result<RegistrationDto>> ApproveDossierAsync(ApproveDossierCommand command, CancellationToken ct = default);
    Task<Result<RegulatorySoDSettingsDto>> GetSoDSettingsAsync(Guid tenantId, CancellationToken ct = default);
    Task<Result<RegulatorySoDSettingsDto>> UpdateSoDSettingsAsync(UpdateSoDSettingsCommand command, CancellationToken ct = default);
    Task<Result<DossierDetailDto>> StartRenewalAsync(StartRenewalCommand command, CancellationToken ct = default);

    Task<Result<IReadOnlyCollection<RegistrationDto>>> SearchRegistrationsAsync(Guid tenantId, string? search, CancellationToken ct = default);
    Task<Result<OperatingLicenseDto>> CreateOperatingLicenseAsync(CreateOperatingLicenseCommand command, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<OperatingLicenseDto>>> ListOperatingLicensesAsync(Guid tenantId, CancellationToken ct = default);
    Task<Result<OperatingLicenseDto>> UpdateOperatingLicenseCompanyDatesAsync(UpdateOperatingLicenseCompanyDatesCommand command, CancellationToken ct = default);
    Task<Result<LicenseCaseDto>> StartLicenseRenewalAsync(StartLicenseRenewalCommand command, CancellationToken ct = default);
    Task<Result<ProductDto>> AttachProductArtifactAsync(AttachProductArtifactCommand command, CancellationToken ct = default);

    Task<Result<RegulatoryDashboardDto>> GetDashboardAsync(Guid tenantId, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<RegulatoryAlertDto>>> EvaluateAlertsAsync(Guid tenantId, Guid? userId, CancellationToken ct = default);
    Task<Result<RegulatoryAlertSettingsDto>> GetAlertSettingsAsync(Guid tenantId, CancellationToken ct = default);
    Task<Result<RegulatoryAlertSettingsDto>> UpdateAlertSettingsAsync(UpdateRegulatoryAlertSettingsCommand command, CancellationToken ct = default);

    Task<Result<ImportJobDto>> StageImportAsync(StageImportCommand command, CancellationToken ct = default);
    Task<Result<ImportJobDto>> StageImportXlsxAsync(StageImportXlsxCommand command, CancellationToken ct = default);
    Task<Result<ImportJobDto>> CommitImportAsync(CommitImportCommand command, CancellationToken ct = default);
    Task<Result<ImportJobDto>> RollbackImportAsync(RollbackImportCommand command, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<ImportJobDto>>> ListImportJobsAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IRegulatoryAffairsRepository
{
    Task AddAuthorityAsync(RegulatoryAuthority authority, CancellationToken ct = default);
    Task<IReadOnlyList<RegulatoryAuthority>> ListAuthoritiesAsync(Guid tenantId, CancellationToken ct = default);
    Task<RegulatoryAuthority?> GetAuthorityAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<RegulatoryAuthority?> GetAuthorityByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);

    Task AddManufacturerAsync(ManufacturerProfile manufacturer, CancellationToken ct = default);
    Task<ManufacturerProfile?> GetManufacturerAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ManufacturerProfile>> SearchManufacturersAsync(Guid tenantId, string? search, CancellationToken ct = default);
    Task AddCertificateAsync(ManufacturerCertificate certificate, CancellationToken ct = default);
    Task<IReadOnlyList<ManufacturerCertificate>> ListCertificatesAsync(Guid tenantId, Guid? manufacturerId, CancellationToken ct = default);
    Task AddCertificateLinkAsync(ManufacturerCertificateDossierLink link, CancellationToken ct = default);

    Task AddProductAsync(MedicalDeviceProduct product, CancellationToken ct = default);
    Task<MedicalDeviceProduct?> GetProductAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MedicalDeviceProduct>> SearchProductsAsync(ProductSearchQuery query, CancellationToken ct = default);
    Task<bool> ProductCatalogExistsAsync(Guid tenantId, string catalogCode, Guid? excludeId, CancellationToken ct = default);

    Task AddPackAsync(RegulatoryRequirementPack pack, CancellationToken ct = default);
    Task<RegulatoryRequirementPack?> GetPackAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<RegulatoryRequirementPack?> GetPublishedPackByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
    Task<IReadOnlyList<RegulatoryRequirementPack>> ListPacksAsync(Guid tenantId, CancellationToken ct = default);

    Task AddDossierAsync(RegistrationDossier dossier, CancellationToken ct = default);
    Task<RegistrationDossier?> GetDossierAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RegistrationDossier>> SearchDossiersAsync(DossierSearchQuery query, CancellationToken ct = default);
    Task<bool> CaseNumberExistsAsync(Guid tenantId, string caseNumber, CancellationToken ct = default);

    Task AddRegistrationAsync(SanitaryRegistration registration, CancellationToken ct = default);
    Task<IReadOnlyList<SanitaryRegistration>> ListRegistrationsAsync(Guid tenantId, string? search, CancellationToken ct = default);
    Task<SanitaryRegistration?> GetCurrentRegistrationAsync(Guid tenantId, Guid productId, Guid authorityId, CancellationToken ct = default);

    Task AddOperatingLicenseAsync(OperatingLicense license, CancellationToken ct = default);
    Task<IReadOnlyList<OperatingLicense>> ListOperatingLicensesAsync(Guid tenantId, CancellationToken ct = default);
    Task<OperatingLicense?> GetOperatingLicenseAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task AddLicenseCaseAsync(LicenseRenewalCase renewalCase, CancellationToken ct = default);

    Task AddImportJobAsync(RegutrackImportJob job, CancellationToken ct = default);
    Task AddImportRowAsync(RegutrackImportRow row, CancellationToken ct = default);
    Task<RegutrackImportJob?> GetImportJobAsync(Guid tenantId, Guid jobId, CancellationToken ct = default);
    Task<IReadOnlyList<RegutrackImportRow>> ListImportRowsAsync(Guid tenantId, Guid jobId, CancellationToken ct = default);
    Task<IReadOnlyList<RegutrackImportJob>> ListImportJobsAsync(Guid tenantId, CancellationToken ct = default);

    Task<RegulatoryAlertSettings?> GetAlertSettingsAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAlertSettingsAsync(RegulatoryAlertSettings settings, CancellationToken ct = default);
    Task AddAlertLogAsync(RegulatoryAlertLog log, CancellationToken ct = default);
    Task<bool> AlertExistsAsync(Guid tenantId, string alertType, Guid entityId, int daysRemaining, DateTimeOffset sinceUtc, CancellationToken ct = default);

    Task<RegulatorySoDSettings?> GetSoDSettingsAsync(Guid tenantId, CancellationToken ct = default);
    Task AddSoDSettingsAsync(RegulatorySoDSettings settings, CancellationToken ct = default);

    Task<RegulatoryDashboardDto> BuildDashboardAsync(Guid tenantId, DateTimeOffset now, CancellationToken ct = default);
}

// ── Commands / queries / DTOs ────────────────────────────────────────────────

public sealed record UpsertManufacturerCommand(Guid TenantId, Guid? ManufacturerId, string LegalName, string CountryCode, string? CommercialName, Guid? SupplierId, string? ContactEmail, string? ContactPhone, Guid RequestedByUserId);
public sealed record AddManufacturerCertificateCommand(Guid TenantId, Guid ManufacturerId, ManufacturerCertificateType Type, string Number, string IssuedBy, DateTimeOffset? IssuedOn, DateTimeOffset? ExpiresOn, string? Country, CertificateLegalFormat LegalFormat, bool Apostilled, bool Notarized, Guid? StoredFileId, string? Notes, Guid RequestedByUserId);
public sealed record CreateMedicalDeviceProductCommand(Guid TenantId, string CountryCode, string Category, string Brand, string RegulatoryName, string? CommercialName, string? Description, string CatalogCode, string? InternalCode, string? ProductType, DeviceRiskClass RiskClass, Guid? ManufacturerId, Guid? DistributorCompanyId, string? DistributorName, string? Initiative, string? Priority, string? SalesMarketingInput, decimal? OpportunityAmount, string? Currency, int? RegisteredSuppliersCount, string? TechnicalSheetReference, string? FormReference, int? SourceLineNumber, Guid RequestedByUserId);
public sealed record UpdateProductCommand(Guid TenantId, Guid ProductId, string Category, string Brand, string RegulatoryName, string? CommercialName, string? Description, DeviceRiskClass RiskClass, Guid? ManufacturerId, Guid? DistributorCompanyId, string? DistributorName, string? Initiative, string? Priority, string? SalesMarketingInput, decimal? OpportunityAmount, string? Currency, int? RegisteredSuppliersCount, string? TechnicalSheetReference, string? FormReference, Guid RequestedByUserId);
public sealed record ProductSearchQuery(Guid TenantId, string? SearchText, DeviceRiskClass? RiskClass, Guid? ManufacturerId, Guid? AuthorityId, bool? CommercializableOnly);
public sealed record CreateDossierCommand(Guid TenantId, Guid ProductId, Guid AuthorityId, RegistrationProcessType ProcessType, Guid? ExistingRegistrationId, string? Priority, Guid? OwnerUserId, string? SalesMarketingInput, decimal? OpportunityAmount, string? Currency, string? Comments, Guid? RequirementPackId, Guid RequestedByUserId, bool SaveAsDraft = false);
public sealed record TransitionDossierCommand(Guid TenantId, Guid DossierId, RegistrationDossierStatus TargetStatus, string? WaiverReason, Guid RequestedByUserId, string? EmergencyOverrideReason = null);
public sealed record ApproveForSubmissionCommand(Guid TenantId, Guid DossierId, Guid RequestedByUserId, string? Notes = null, string? EmergencyOverrideReason = null);
public sealed record SubmitDossierCommand(
    Guid TenantId,
    Guid DossierId,
    string? ProcedureNumber,
    string? ExternalNumber,
    DateTimeOffset? SubmittedOn,
    Guid? ProofStoredFileId,
    Guid RequestedByUserId,
    string? EmergencyOverrideReason = null);
public sealed record ResubmitDossierCommand(
    Guid TenantId,
    Guid DossierId,
    string? ProcedureNumber,
    string? ExternalNumber,
    DateTimeOffset? SubmittedOn,
    Guid? ProofStoredFileId,
    Guid RequestedByUserId);
public sealed record StartAuthorityReviewCommand(Guid TenantId, Guid DossierId, Guid RequestedByUserId);
public sealed record RejectDossierCommand(
    Guid TenantId,
    Guid DossierId,
    string Reason,
    string ResolutionNumber,
    DateTimeOffset DecidedOn,
    Guid? ResolutionStoredFileId,
    Guid RequestedByUserId,
    string? EmergencyOverrideReason = null);
public sealed record UpdateDossierDatesCommand(Guid TenantId, Guid DossierId, DateTimeOffset? RequestedFromFactoryOn, DateTimeOffset? EstimatedReceptionOn, DateTimeOffset? MaximumReceptionOn, DateTimeOffset? EstimatedSubmissionOn, DateTimeOffset? EstimatedApprovalOn, DateTimeOffset? TargetExpirationOn, Guid RequestedByUserId);
public sealed record UpdateRequirementCommand(Guid TenantId, Guid DossierId, Guid RequirementId, DossierRequirementStatus Status, Guid? DocumentId, Guid? StoredFileId, string? Notes, Guid RequestedByUserId, string? EmergencyOverrideReason = null);
public sealed record OpenObservationCommand(Guid TenantId, Guid DossierId, string Description, DateTimeOffset ReceivedOn, DateTimeOffset? DueOn, Guid? ResponsibleUserId, IReadOnlyList<Guid>? RequirementIds, Guid RequestedByUserId);
public sealed record RespondObservationCommand(Guid TenantId, Guid DossierId, Guid ObservationId, string Notes, bool Close, Guid RequestedByUserId);
public sealed record ApproveDossierCommand(
    Guid TenantId,
    Guid DossierId,
    string RegistrationNumber,
    DateTimeOffset IssuedOn,
    DateTimeOffset? ExpiresOn,
    string? Notes,
    Guid ResolutionStoredFileId,
    Guid RequestedByUserId,
    string? EmergencyOverrideReason = null);
public sealed record UpdateSoDSettingsCommand(
    Guid TenantId,
    Guid RequestedByUserId,
    bool PreventSelfReview,
    bool PreventSelfApproval,
    bool SeparateApproverAndSubmitter,
    bool SeparateDocumentUploaderAndReviewer,
    bool RequireSecondApprovalForCriticalWaiver,
    bool RequireApprovalForCriticalityChange,
    bool RequireApprovalForExternalDecisionRecording,
    bool AllowEmergencyOverride,
    bool EmergencyOverrideRequiresReason,
    bool EmergencyOverrideRequiresSecondaryReview,
    bool RequireInternalApprovalBeforeSubmission);
public sealed record StartRenewalCommand(Guid TenantId, Guid ProductId, Guid AuthorityId, Guid? RequirementPackId, Guid RequestedByUserId);
public sealed record CreateOperatingLicenseCommand(Guid TenantId, string CompanyName, Guid? CompanyId, string LicenseType, Guid? AuthorityId, string? LicenseNumber, DateTimeOffset? IssuedOn, DateTimeOffset? ExpiresOn, string? Comments, Guid RequestedByUserId, DateOnly? CompanyConstitutedOn = null, DateOnly? OperationsStartedOn = null);
public sealed record UpdateOperatingLicenseCompanyDatesCommand(Guid TenantId, Guid LicenseId, DateOnly? CompanyConstitutedOn, DateOnly? OperationsStartedOn, bool ClearConstitution, bool ClearOperationsStart, Guid RequestedByUserId);
public sealed record AttachProductArtifactCommand(Guid TenantId, Guid ProductId, string ArtifactKind, string? Reference, Guid? DocumentId, Guid? StoredFileId, RegulatoryArtifactStatus Status, Guid RequestedByUserId);
public sealed record StartLicenseRenewalCommand(Guid TenantId, Guid LicenseId, string? Comments, Guid RequestedByUserId);
public sealed record StageImportCommand(Guid TenantId, string SourceFileName, string RowsJson, Guid RequestedByUserId);
public sealed record UpdateRegulatoryAlertSettingsCommand(Guid TenantId, string ThresholdsCsv, Guid RequestedByUserId);
public sealed record StageImportXlsxCommand(Guid TenantId, string SourceFileName, byte[] FileBytes, Guid RequestedByUserId);
public sealed record CommitImportCommand(Guid TenantId, Guid JobId, Guid RequestedByUserId, int? MaxRows = null);
public sealed record RollbackImportCommand(Guid TenantId, Guid JobId, string? Reason, Guid RequestedByUserId);
public sealed record DossierSearchQuery(Guid TenantId, string? SearchText, RegistrationDossierStatus? Status, Guid? AuthorityId, Guid? ProductId, RegistrationProcessType? ProcessType);

public sealed record AuthorityDto(Guid Id, string Code, string Name, string CountryCode, RegulatoryAuthorityType AuthorityType, bool IsActive);
public sealed record ManufacturerDto(Guid Id, string LegalName, string? CommercialName, string CountryCode, Guid? SupplierId, string? ContactEmail, string? ContactPhone, bool IsActive);
public sealed record ManufacturerCertificateDto(Guid Id, Guid ManufacturerId, ManufacturerCertificateType Type, string Number, string IssuedBy, DateTimeOffset? IssuedOn, DateTimeOffset? ExpiresOn, DateTimeOffset? RequestedOn, ManufacturerCertificateStatus Status, CertificateLegalFormat LegalFormat, bool Apostilled, bool Notarized, Guid? StoredFileId);
public sealed record ProductDto(Guid Id, string CountryCode, string Category, string Brand, string RegulatoryName, string? CommercialName, string CatalogCode, DeviceRiskClass RiskClass, Guid? ManufacturerId, Guid? DistributorCompanyId, string? DistributorName, decimal? OpportunityAmount, string Currency, bool IsCommercializable, string? Priority, string? Initiative, int? RegisteredSuppliersCount, int? SourceLineNumber, string? TechnicalSheetReference, string? FormReference, RegulatoryArtifactStatus TechnicalSheetStatus, Guid? TechnicalSheetDocumentId, Guid? TechnicalSheetStoredFileId, RegulatoryArtifactStatus FormStatus, Guid? FormDocumentId, Guid? FormStoredFileId);
public sealed record RequirementDefDto(Guid Id, string Code, string Name, string Category, bool IsRequired, bool IsCritical, int Order);
public sealed record RequirementPackDto(Guid Id, string Code, string Name, string VersionLabel, RequirementPackStatus Status, IReadOnlyCollection<RequirementDefDto> Definitions);
public sealed record RequirementDto(Guid Id, string Code, string Name, string Category, bool IsRequired, bool IsCritical, DossierRequirementStatus Status, Guid? StoredFileId, Guid? CurrentDocumentId, string? ValidationNotes, int Order);
public sealed record MilestoneDto(Guid Id, DossierMilestoneType MilestoneType, DateTimeOffset? PlannedDate, DateTimeOffset? ActualDate, MilestoneCompletionStatus Status);
public sealed record ObservationDto(Guid Id, int ObservationNumber, DateTimeOffset ReceivedOn, DateTimeOffset? DueOn, string Description, AuthorityObservationStatus Status, DateTimeOffset? ResponseSubmittedOn, DateTimeOffset? ClosedOn, string? Notes);
public sealed record DossierSummaryDto(Guid Id, string CaseNumber, Guid ProductId, Guid AuthorityId, RegistrationProcessType ProcessType, RegistrationDossierStatus Status, string? Priority, decimal? OpportunityAmount, DateTimeOffset? SubmittedOn, DateTimeOffset? ApprovedOn, DateTimeOffset? MaximumReceptionOn, DateTimeOffset CreatedAtUtc, int DaysInStatus);
public sealed record DossierDetailDto(Guid Id, string CaseNumber, Guid ProductId, Guid AuthorityId, RegistrationProcessType ProcessType, Guid? ExistingRegistrationId, RegistrationDossierStatus Status, string? Priority, Guid? RegulatoryOwnerUserId, string? Comments, string? SalesMarketingInput, decimal? OpportunityAmount, string Currency, Guid? RequirementPackId, string? RequirementPackVersionLabel, Guid? ResultingRegistrationId, DateTimeOffset? RequestedFromFactoryOn, DateTimeOffset? EstimatedReceptionOn, DateTimeOffset? MaximumReceptionOn, DateTimeOffset? ReceivedOn, DateTimeOffset? AssembledOn, DateTimeOffset? EstimatedSubmissionOn, DateTimeOffset? SubmittedOn, Guid? SubmittedByUserId, string? SubmissionProcedureNumber, string? SubmissionExternalNumber, Guid? SubmissionProofStoredFileId, DateTimeOffset? ObservationReceivedOn, DateTimeOffset? EstimatedApprovalOn, DateTimeOffset? ApprovedOn, DateTimeOffset? TargetExpirationOn, IReadOnlyCollection<RequirementDto> Requirements, IReadOnlyCollection<MilestoneDto> Milestones, IReadOnlyCollection<ObservationDto> Observations, IReadOnlyCollection<DossierHistoryDto> History, long Revision = 0);
public sealed record DossierHistoryDto(Guid Id, string EventType, string Summary, Guid? ActorUserId, DateTimeOffset OccurredAtUtc);
public sealed record RegulatorySoDSettingsDto(
    bool PreventSelfReview,
    bool PreventSelfApproval,
    bool SeparateApproverAndSubmitter,
    bool SeparateDocumentUploaderAndReviewer,
    bool RequireSecondApprovalForCriticalWaiver,
    bool RequireApprovalForCriticalityChange,
    bool RequireApprovalForExternalDecisionRecording,
    bool AllowEmergencyOverride,
    bool EmergencyOverrideRequiresReason,
    bool EmergencyOverrideRequiresSecondaryReview,
    bool RequireInternalApprovalBeforeSubmission);
public sealed record RegistrationDto(Guid Id, Guid ProductId, Guid AuthorityId, string RegistrationNumber, DateTimeOffset? IssuedOn, DateTimeOffset? ExpiresOn, SanitaryRegistrationStatus Status, bool IsCurrent, int? DaysRemaining);
public sealed record OperatingLicenseDto(Guid Id, string CompanyName, string LicenseType, string? LicenseNumber, DateTimeOffset? IssuedOn, DateTimeOffset? ExpiresOn, OperatingLicenseStatus Status, Guid? ActiveRenewalCaseId, DateOnly? CompanyConstitutedOn, DateOnly? OperationsStartedOn);
public sealed record LicenseCaseDto(Guid Id, Guid OperatingLicenseId, string CaseNumber, LicenseRenewalCaseStatus Status, string? ManualPlatformTaskNotes);
public sealed record RegulatoryDashboardDto(
    int ProductsTotal,
    int RegistrationsActive,
    int RegistrationsExpiring,
    int RegistrationsExpired,
    int RegistrationsExpiringThisMonth,
    int DossiersInProgress,
    int DossiersOverdue,
    int DossiersStuckOver14Days,
    int PendingCriticalRequirements,
    int ManufacturerCertificatesExpiring,
    int LicensesExpiring,
    decimal OpportunityAmountTotal,
    string? BottleneckStatus,
    int BottleneckCount,
    IReadOnlyDictionary<string, int> DossiersByAuthority,
    IReadOnlyDictionary<string, int> ProductsByRiskClass,
    IReadOnlyDictionary<string, int> DossiersByStatus,
    IReadOnlyDictionary<string, decimal> OpportunityByStatus);
public sealed record RegulatoryAlertDto(string AlertType, string EntityName, Guid EntityId, int DaysRemaining, string Message);
public sealed record RegulatoryAlertSettingsDto(string ThresholdsCsv, IReadOnlyCollection<int> ThresholdsDays);
public sealed record ImportJobDto(Guid Id, string SourceFileName, RegutrackImportJobStatus Status, int WarningCount, int ErrorCount, int ImportedRowCount, DateTimeOffset CreatedAtUtc, string? ValidationReportJson);
