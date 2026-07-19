using Compliance360.Domain.Audit;
using Compliance360.Application.Notifications;
using Compliance360.Domain.AuditManagement;
using Compliance360.Domain.CapaManagement;
using Compliance360.Domain.Documents;
using Compliance360.Domain.Enterprise;
using Compliance360.Domain.FormTemplates;
using Compliance360.Domain.RegulatoryAffairs;
using Compliance360.Domain.Identity;
using Compliance360.Domain.Notifications;
using Compliance360.Domain.QualityIndicators;
using Compliance360.Domain.Reporting;
using Compliance360.Domain.RiskManagement;
using Compliance360.Domain.Storage;
using Compliance360.Domain.Suppliers;
using Compliance360.Domain.TenantManagement;
using Compliance360.Domain.TechnicalSheets;
using Compliance360.Domain.Workflows;

namespace Compliance360.Web.Api;

public sealed record IdentifyRequest(string Email);

public sealed record IdentifyOrganizationSummary(Guid Id, string Name, string? LogoUri, string? PrimaryColor, string? Description);

public sealed record IdentifyResponse(
    string ResolverToken,
    bool RequiresOrganizationSelection,
    Guid? PreselectedOrganizationId,
    IReadOnlyCollection<IdentifyOrganizationSummary> Organizations);

public sealed record LoginRequest(
    Guid? TenantId,
    string Email,
    string Password,
    string? ResolverToken,
    Guid? OrganizationId,
    bool? RememberMe);

public sealed record CompleteMfaChallengeRequest(string ChallengeToken, MfaMethod Method, string VerificationCode);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record LogoutRequest(Guid TenantId, Guid UserId, string RefreshTokenHash);

public sealed record ChangePasswordRequest(Guid TenantId, string CurrentPassword, string NewPassword);

public sealed record UpdateUserPreferredLanguageRequest(string? PreferredLanguage);

public sealed record CreateTenantRequest(
    string Name,
    string Slug,
    string? LegalName,
    string? CommercialName,
    string? TaxIdentifier,
    string? CountryCode,
    string? Currency,
    string? AdminEmail = null,
    string? AdminFullName = null,
    string? AdminPassword = null);

public sealed record UpdateTenantGeneralInformationRequest(
    string Name,
    string LegalName,
    string CommercialName,
    string TaxIdentifier,
    string Industry,
    string? Description,
    string? AddressLine1,
    string? City,
    string? Province,
    string CountryCode,
    string? PostalCode,
    string? Phone,
    string? Email,
    string? Website,
    string Currency,
    string? ChangeReason);

public sealed record AddCompanyRequest(string LegalName, string TaxIdentifier, string CountryCode);

public sealed record ConfigureTenantSettingsRequest(string Culture, string TimeZone, bool RequireMfa, int DocumentRetentionDays);

public sealed record ConfigureTenantSecurityRequest(
    bool RequireMfa,
    int SessionTimeoutMinutes,
    int PasswordExpirationDays,
    int LockoutMaxFailedAttempts,
    int LockoutMinutes,
    string? IpWhitelist,
    bool TrustedDevicesEnabled,
    int SecurityScore,
    string? ChangeReason);

public sealed record ConfigureTenantBrandingRequest(
    string DisplayName,
    string? LogoUri,
    string? FaviconUri,
    string PrimaryColor,
    string SecondaryColor,
    string Theme,
    string? LoginBackgroundUri,
    string? CorporateEmail,
    string? FooterText,
    string? ChangeReason);

public sealed record ChangeSubscriptionRequest(
    SubscriptionPlan Plan,
    int MaxUsers,
    int MaxStorageGb,
    SubscriptionStatus Status,
    DateOnly? ExpiresOn,
    string? ChangeReason);

public sealed record UpsertTenantDomainRequest(string HostName, TenantDomainKind Kind, bool IsDefault, string? RedirectToHostName, string? ChangeReason);

public sealed record UpsertTenantSsoRequest(
    TenantSsoProviderType Provider,
    string Name,
    string Authority,
    string? MetadataUrl,
    string ClientId,
    string? SecretReference,
    string ClaimsMappingJson,
    string RoleMappingJson,
    bool JitProvisioningEnabled,
    bool ScimEnabled,
    string? CertificateThumbprint,
    bool Enabled,
    string? ChangeReason);

public sealed record CreateTenantApiCredentialRequest(string Name, string PlainTextSecret, string Scopes, DateTimeOffset? ExpiresAtUtc, string? ChangeReason);

public sealed record RotateTenantApiCredentialRequest(string PlainTextSecret, DateTimeOffset? ExpiresAtUtc, string? ChangeReason);

public sealed record UpsertTenantWebhookRequest(string Name, string Url, string Events, string PlainTextSecret, int MaxRetries, bool Enabled, string? ChangeReason);

public sealed record UpsertTenantLicenseRequest(
    string LicenseNumber,
    TenantLicenseStatus Status,
    string FeaturesJson,
    string ModulesJson,
    string EntitlementsJson,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly RenewalDate,
    int SeatsPurchased,
    int SeatsUsed,
    int StorageGbPurchased,
    long StorageBytesUsed,
    string? ChangeReason);

public sealed record RecordTenantBackupRequest(string BackupKind, string Result, DateTimeOffset StartedAtUtc, DateTimeOffset CompletedAtUtc, long SizeBytes, string Message, int RpoMinutes, int RtoMinutes);

public sealed record CreateTenantUserRequest(string Email, string FullName, string InitialPassword, bool ForcePasswordChange, Guid? RoleId, string? ChangeReason);

public sealed record UpdateTenantUserRequest(string Email, string FullName, string? ChangeReason);

public sealed record ResetTenantUserPasswordRequest(string NewPassword, bool ForcePasswordChange, string? ChangeReason);

public sealed record ChangeTenantUserStatusRequest(UserStatus Status, string? ChangeReason);

public sealed record AssignTenantUserRoleRequest(Guid RoleId, string? ChangeReason);

public sealed record TenantActionRequest(string? ChangeReason);

public sealed record SuperAdminTenantSearchRequest(string? SearchText, string? Status, int Page, int PageSize);

public sealed record CreateRoleRequest(string Name, bool IsSystemRole);

public sealed record CreatePermissionRequest(string Module, PermissionAction Action, string Description);

public sealed record AssignRoleRequest(Guid UserId, Guid RoleId);

public sealed record GrantPermissionRequest(Guid RoleId, Guid PermissionId);

public sealed record BeginMfaSetupRequest(MfaMethod Method);

public sealed record EnableMfaRequest(MfaMethod Method, string VerificationCode);

public sealed record VerifyMfaRequest(MfaMethod Method, string VerificationCode);

public sealed record DisableMfaRequest(MfaMethod Method);

public sealed record AuditSearchRequest(
    AuditAction? Action,
    AuditCategory? Category,
    string? EntityName,
    Guid? EntityId,
    string? SearchText,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    int Page,
    int PageSize);

public sealed record CreateEnterpriseWorkspaceItemRequest(
    EnterpriseWorkspaceType Type,
    string Title,
    string Code,
    string Description,
    Guid? OwnerUserId,
    DateTimeOffset? DueAtUtc,
    string? MetadataJson);

public sealed record CreateFormTemplateRequest(
    string Name,
    string Code,
    string Category,
    FormTemplateKind Kind,
    string Description,
    string? InitialSchemaJson);

public sealed record UpdateFormTemplateHeaderRequest(
    string Name,
    string Category,
    FormTemplateKind Kind,
    string Description);

public sealed record SaveFormTemplateDraftRequest(
    Guid? VersionId,
    string SchemaJson,
    string ChangeLog);

public sealed record FormTemplatePublishRequest(Guid? VersionId);

public sealed record DuplicateFormTemplateRequest(string NewName, string NewCode);

public sealed record UpsertManufacturerRequest(Guid? ManufacturerId, string LegalName, string CountryCode, string? CommercialName, Guid? SupplierId, string? ContactEmail, string? ContactPhone);
public sealed record AddManufacturerCertificateRequest(Guid ManufacturerId, ManufacturerCertificateType Type, string Number, string IssuedBy, DateTimeOffset? IssuedOn, DateTimeOffset? ExpiresOn, string? Country, CertificateLegalFormat LegalFormat, bool Apostilled, bool Notarized, Guid? StoredFileId, string? Notes);
public sealed record CreateMedicalDeviceProductRequest(string CountryCode, string Category, string Brand, string RegulatoryName, string? CommercialName, string? Description, string CatalogCode, string? InternalCode, string? ProductType, DeviceRiskClass RiskClass, Guid? ManufacturerId, Guid? DistributorCompanyId, string? DistributorName, string? Initiative, string? Priority, string? SalesMarketingInput, decimal? OpportunityAmount, string? Currency, int? RegisteredSuppliersCount, string? TechnicalSheetReference, string? FormReference, int? SourceLineNumber);
public sealed record CreateRegistrationDossierRequest(Guid ProductId, Guid AuthorityId, RegistrationProcessType ProcessType, Guid? ExistingRegistrationId, string? Priority, Guid? OwnerUserId, string? SalesMarketingInput, decimal? OpportunityAmount, string? Currency, string? Comments, Guid? RequirementPackId, bool SaveAsDraft = false);
public sealed record TransitionDossierRequest(RegistrationDossierStatus TargetStatus, string? WaiverReason, string? EmergencyOverrideReason = null);
public sealed record ReturnForCorrectionV2Request(long ExpectedRevision, string Reason, DossierCorrectionSeverity Severity, IReadOnlyCollection<Guid> RequirementIds, IReadOnlyCollection<string>? FieldPaths, IReadOnlyCollection<Guid>? DocumentIds);
public sealed record SubmitCorrectionV2Request(long ExpectedRevision, Guid CorrectionRequestId, string Reason, IReadOnlyCollection<Guid> RequirementIds, IReadOnlyCollection<string>? FieldPaths, IReadOnlyCollection<Guid>? DocumentIds);
public sealed record StartTechnicalReviewV2Request(long ExpectedRevision, string Reason);
public sealed record CompleteTechnicalReviewV2Request(long ExpectedRevision, Guid? CorrectionRequestId, string Reason);
public sealed record EvidenceRevisionV2Request(long ExpectedRevision, Guid RequirementId, Guid? CorrectionRequestId, Guid? DocumentId, Guid StoredFileId, string Sha256, string FileName, string Reason);
public sealed record GovernanceV2Request(long ExpectedRevision, string Reason);
public sealed record GovernanceDecisionV2Request(long ExpectedRevision, string? Reason);
public sealed record OverrideV2Request(long ExpectedRevision, string Action, string Reason);
public sealed record ConsumeOverrideV2Request(long ExpectedRevision, string Action);
public sealed record UpdateDossierMetadataV2Request(long ExpectedRevision, string Reason, string? Priority, Guid? OwnerUserId, string? SalesMarketingInput, decimal? OpportunityAmount, string? Currency, string? Comments, DateTimeOffset? RequestedFromFactoryOn, DateTimeOffset? EstimatedReceptionOn, DateTimeOffset? MaximumReceptionOn, DateTimeOffset? EstimatedSubmissionOn, DateTimeOffset? EstimatedApprovalOn, DateTimeOffset? TargetExpirationOn, Guid? CorrectionRequestId = null);
public sealed record UpdateDossierDatesRequest(DateTimeOffset? RequestedFromFactoryOn, DateTimeOffset? EstimatedReceptionOn, DateTimeOffset? MaximumReceptionOn, DateTimeOffset? EstimatedSubmissionOn, DateTimeOffset? EstimatedApprovalOn, DateTimeOffset? TargetExpirationOn);
public sealed record UpdateDossierRequirementRequest(DossierRequirementStatus Status, Guid? DocumentId, Guid? StoredFileId, string? Notes, string? EmergencyOverrideReason = null);
public sealed record OpenObservationRequest(string Description, DateTimeOffset ReceivedOn, DateTimeOffset? DueOn, Guid? ResponsibleUserId, IReadOnlyList<Guid>? RequirementIds);
public sealed record RespondObservationRequest(string Notes, bool Close);
public sealed record ApproveDossierRequest(
    string RegistrationNumber,
    DateTimeOffset IssuedOn,
    DateTimeOffset? ExpiresOn,
    string? Notes,
    Guid ResolutionStoredFileId,
    string? EmergencyOverrideReason = null);
public sealed record ApproveForSubmissionRequest(string? Notes = null, string? EmergencyOverrideReason = null);
public sealed record SubmitDossierRequest(
    string? ProcedureNumber,
    string? ExternalNumber,
    DateTimeOffset? SubmittedOn,
    Guid? ProofStoredFileId,
    string? EmergencyOverrideReason = null);
public sealed record RejectDossierRequest(
    string Reason,
    string ResolutionNumber,
    DateTimeOffset DecidedOn,
    Guid? ResolutionStoredFileId,
    string? EmergencyOverrideReason = null);
public sealed record UpdateSoDSettingsRequest(
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
public sealed record StartRenewalRequest(Guid ProductId, Guid AuthorityId, Guid? RequirementPackId);
public sealed record CreateOperatingLicenseRequest(string CompanyName, Guid? CompanyId, string LicenseType, Guid? AuthorityId, string? LicenseNumber, DateTimeOffset? IssuedOn, DateTimeOffset? ExpiresOn, string? Comments, DateOnly? CompanyConstitutedOn = null, DateOnly? OperationsStartedOn = null);
public sealed record UpdateOperatingLicenseCompanyDatesRequest(DateOnly? CompanyConstitutedOn, DateOnly? OperationsStartedOn, bool ClearConstitution = false, bool ClearOperationsStart = false);
public sealed record AttachProductArtifactRequest(string ArtifactKind, string? Reference, Guid? DocumentId, Guid? StoredFileId, string Status);
public sealed record StartLicenseRenewalRequest(string? Comments);
public sealed record StageRegutrackImportRequest(string SourceFileName, string RowsJson);
public sealed record RollbackImportRequest(string? Reason);
public sealed record UpdateRegulatoryAlertSettingsRequest(string ThresholdsCsv);

public sealed record CreateNotificationTemplateRequest(string Code, NotificationChannel Channel, string Subject, string Body, string? TextBody, string? Locale, string? BrandingJson);

public sealed record PreviewNotificationTemplateRequest(string Subject, string Body, string? TextBody, IReadOnlyDictionary<string, string> Variables, TenantNotificationBranding? Branding);

public sealed record QueueNotificationRequest(
    NotificationChannel Channel,
    string Recipient,
    string? Subject,
    string? Body,
    string? TemplateCode,
    IReadOnlyDictionary<string, string> Variables,
    NotificationPriority Priority,
    Guid? TargetUserId);

public sealed record ConfigureNotificationProviderRequest(NotificationProvider Provider, string Name, int Priority, bool IsDefault, bool IsEnabled);

public sealed record UpsertProviderCenterRequest(
    Guid? ProviderId,
    NotificationProvider Provider,
    string Name,
    int Priority,
    bool IsEnabled,
    NotificationProviderAuthentication Authentication,
    string FromAddress,
    string? FromName,
    ProviderSecretSettings Settings,
    int RateLimitPerMinute = 60,
    int CircuitFailureThreshold = 5,
    int CircuitBreakSeconds = 300);

public sealed record SendProviderSandboxRequest(string Recipient, string? Subject, string? Body);

public sealed record NotificationInboxActionRequest(NotificationInboxAction Action);

public sealed record NotificationInboxBulkActionRequest(IReadOnlyCollection<Guid>? InboxItemIds, NotificationInboxAction Action, bool All);

public sealed record CreateNotificationTemplateVersionRequest(string Locale, string Subject, string HtmlBody, string? TextBody, string? BrandingJson);

public sealed record CreateNotificationTemplateDefinitionRequest(string Code, NotificationChannel Channel, string Locale, string Subject, string HtmlBody, string? TextBody, string? BrandingJson);

public sealed record DuplicateNotificationTemplateVersionRequest(string Locale);

public sealed record NotificationTemplateLifecycleActionRequest(NotificationTemplateLifecycleAction Action);

public sealed record PreviewNotificationTemplateVersionRequest(IReadOnlyDictionary<string, string>? Variables, TenantNotificationBranding? Branding);

public sealed record SendNotificationTemplateTestRequest(string Recipient, Guid? TargetUserId, IReadOnlyDictionary<string, string>? Variables);

public sealed record PreviewAlertRecipientsRequest(NotificationChannel Channel, string? Topic, IReadOnlyDictionary<RecipientKind, Guid?>? RelationshipUsers);

public sealed record SetRecipientPreferenceRequest(NotificationChannel Channel, bool Enabled);

public sealed record CreateRecipientGroupRequest(string Name);

public sealed record AddRecipientGroupMemberRequest(Guid UserId);

public sealed record CreateRecipientDepartmentRequest(string Name);

public sealed record SetRecipientDirectoryProfileRequest(Guid? DepartmentId, Guid? SupervisorUserId);

public sealed record AuthorizeExternalRecipientRequest(string Email, string DisplayName);

public sealed record CreateRecipientDistributionListRequest(string Name);

public sealed record AddRecipientDistributionListMemberRequest(Guid? UserId, Guid? ExternalRecipientId);

public sealed record SetRecipientFallbackRequest(RecipientFallbackMode Mode, Guid? TargetId, RecipientRouting Routing);

public sealed record CreateAlertDefinitionRequest(
    Guid EventTypeId,
    string Code,
    string Name,
    string Description,
    Guid? OwnerUserId,
    NotificationPriority Priority,
    string ConditionJson,
    string RecipientRulesJson,
    string ChannelPoliciesJson,
    string DedupeExpression,
    int SilenceWindowMinutes,
    int? SlaMinutes,
    AlertUnknownPolicy UnknownPolicy);

public sealed record CreateAlertDefinitionVersionRequest(
    string ConditionJson,
    string RecipientRulesJson,
    string ChannelPoliciesJson,
    string DedupeExpression,
    int SilenceWindowMinutes,
    int? SlaMinutes,
    AlertUnknownPolicy UnknownPolicy);

public sealed record AlertDefinitionLifecycleActionRequest(string Action);

public sealed record SimulateAlertRuleRequest(
    Guid? DefinitionId,
    Guid? VersionId,
    string? ConditionJson,
    AlertUnknownPolicy UnknownPolicy,
    string EventPayloadJson);

public sealed record CreateAlertScheduleRequest(
    Guid DefinitionId,
    string Code,
    string Name,
    string CronExpression,
    string TimeZoneId,
    string BusinessCalendarJson,
    string QuietHoursJson,
    AlertScheduleCatchUpPolicy CatchUpPolicy,
    int MaxCatchUpExecutions,
    AlertScheduleDigest Digest);

public sealed record PreviewAlertScheduleRequest(
    string CronExpression,
    string TimeZoneId,
    string BusinessCalendarJson,
    string QuietHoursJson,
    DateTimeOffset? FromUtc,
    int Count);

public sealed record ChangeAlertScheduleStateRequest(bool IsActive);

public sealed record AlertMessageOperationRequest(string Action);

public sealed record IngestAlertEventRequest(
    string EventCode,
    string PayloadJson,
    string SourceModule,
    string EntityType,
    Guid? EntityId,
    string CorrelationId,
    DateTimeOffset? OccurredAtUtc);

public sealed record ConfigureStorageProviderRequest(StorageProviderKind Provider, string Name, string ContainerName, int Priority, bool IsDefault, bool IsEnabled, string SettingsJson);

public sealed record CreateDocumentTypeRequest(string Name, string Code, int RetentionDays);

public sealed record CreateDocumentCategoryRequest(string Name, string Code);

public sealed record CreateDocumentRequest(Guid DocumentTypeId, Guid CategoryId, string Title, string Code);

public sealed record AddDocumentVersionRequest(Guid StoredFileId, string ChangeSummary);

public sealed record DecideDocumentRequest(DocumentApprovalDecision Decision, string Comments);

public sealed record GrantDocumentPermissionRequest(Guid PrincipalId, DocumentPermissionLevel Level);

public sealed record CreateWorkflowRequest(string Name, string Code, string EntityName);

public sealed record AddWorkflowStepRequest(string Name, WorkflowStepType Type, int Sequence, int SlaHours, Guid? AssignedRoleId);

public sealed record AddWorkflowTransitionRequest(Guid FromStepId, Guid ToStepId, WorkflowDecision Decision);

public sealed record AddWorkflowRuleRequest(string FieldName, WorkflowRuleOperator Operator, string ExpectedValue);

public sealed record StartWorkflowRequest(string EntityName, Guid EntityId);

public sealed record AssignWorkflowRequest(Guid StepId, Guid AssignedToUserId, DateTimeOffset DueAtUtc);

public sealed record CompleteWorkflowAssignmentRequest(Guid AssignmentId, WorkflowDecision Decision);

public sealed record EscalateWorkflowRequest(Guid AssignmentId, Guid EscalatedToUserId);

public sealed record CreateProductRequest(string Name, string Sku, string? Description);

public sealed record CreateTechnicalSheetRequest(Guid ProductId, string Title);

public sealed record CreateTechnicalSheetVersionRequest(string ChangeSummary);

public sealed record AddTechnicalSheetIngredientRequest(string Name, decimal Percentage, string? Allergen);

public sealed record AddTechnicalSheetNutrientRequest(string Name, decimal Amount, string Unit);

public sealed record AddTechnicalSheetCertificationRequest(string Name, string Issuer, DateTimeOffset ExpiresAtUtc);

public sealed record DecideTechnicalSheetRequest(TechnicalSheetApprovalDecision Decision, string Comments);

public sealed record AttachTechnicalSheetPdfRequest(string PdfObjectKey);

public sealed record CreateSupplierRequest(string LegalName, string TaxIdentifier, string CountryCode);

public sealed record AddSupplierDocumentRequest(SupplierDocumentType Type, string DocumentNumber, Guid StoredFileId, DateTimeOffset IssuedAtUtc, DateTimeOffset ExpiresAtUtc);

public sealed record RejectSupplierDocumentRequest(string Reason);

public sealed record AddSupplierEvaluationRequest(int Score, string Comments);

public sealed record SuspendSupplierRequest(string Reason);

public sealed record CreateAuditProgramRequest(string Name, string Code, int Year);

public sealed record CreateAuditChecklistRequest(string Name, string Code, AuditChecklistType Type, int Version);

public sealed record AddAuditChecklistItemRequest(string Clause, string Question, int Weight);

public sealed record CreateAuditPlanRequest(Guid AuditProgramId, string Scope, string Criteria, DateTimeOffset PlannedStartUtc, DateTimeOffset PlannedEndUtc);

public sealed record CreateManagedAuditRequest(Guid AuditProgramId, Guid AuditPlanId, string Title, string Code, ManagedAuditType Type);

public sealed record AssignAuditChecklistRequest(Guid ChecklistId);

public sealed record ScheduleAuditRequest(DateTimeOffset StartUtc, DateTimeOffset EndUtc, string Location);

public sealed record AddAuditParticipantRequest(Guid UserId, AuditParticipantRole Role);

public sealed record AddAuditAreaRequest(string Name, string Process);

public sealed record AddAuditFindingRequest(string Title, string Description, AuditFindingSeverity Severity, Guid? ChecklistItemId);

public sealed record AddAuditEvidenceRequest(Guid FindingId, AuditEvidenceType Type, Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);

public sealed record AddAuditObservationRequest(string Description);

public sealed record AddAuditNonConformityRequest(Guid FindingId, string Requirement);

public sealed record AddAuditRecommendationRequest(Guid FindingId, string Recommendation);

public sealed record LinkAuditCorrectiveActionRequest(Guid FindingId, Guid CorrectiveActionId);

public sealed record AddAuditAttachmentRequest(Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);

public sealed record CreateCapaRequest(string Title, string Code, string Description, CapaPriority Priority, CapaRiskLevel RiskLevel, CapaSourceType SourceType, Guid? SourceEntityId, Guid? SupplierId, Guid? DocumentId, Guid? AuditId);

public sealed record ClassifyCapaRequest(CapaPriority Priority, CapaRiskLevel RiskLevel, DateTimeOffset? CommitmentDueAtUtc);

public sealed record AssignCapaOwnerRequest(Guid OwnerUserId, DateTimeOffset DueAtUtc);

public sealed record AddCapaApproverRequest(Guid ApproverUserId);

public sealed record DefineCapaRootCauseRequest(string Description, CapaRootCauseMethod Method);

public sealed record AddCapaFiveWhyRequest(string Why1, string Why2, string Why3, string Why4, string Why5);

public sealed record AddCapaIshikawaRequest(string People, string Process, string Equipment, string Material, string Environment, string Measurement);

public sealed record AddCapaActionRequest(string Description, Guid ResponsibleUserId, DateTimeOffset DueAtUtc);

public sealed record CompleteCapaActionRequest(Guid ActionId);

public sealed record AddCapaEvidenceRequest(Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);

public sealed record AddCapaAttachmentRequest(Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);

public sealed record RegisterCapaFollowUpRequest(string Notes);

public sealed record VerifyCapaEffectivenessRequest(bool IsEffective, string VerificationSummary);

public sealed record AttachCapaWorkflowRequest(Guid WorkflowInstanceId);

public sealed record ReopenCapaRequest(string Reason);

public sealed record CreateRiskCategoryRequest(string Name, string Code);

public sealed record CreateRiskMatrixRequest(string Name, int ToleranceScore);

public sealed record CreateRiskRequest(Guid CategoryId, string Title, string Code, string Description, RiskType Type, string Area, string Process, Guid? SupplierId, Guid? DocumentId, Guid? AuditId, Guid? CapaId);

public sealed record ClassifyRiskRequest(RiskType Type, string Area, string Process);

public sealed record AssignRiskOwnerRequest(Guid OwnerUserId);

public sealed record AssessRiskRequest(RiskProbability Probability, RiskImpact Impact, RiskProbability ResidualProbability, RiskImpact ResidualImpact, int ToleranceScore);

public sealed record AddRiskTreatmentRequest(RiskTreatmentStrategy Strategy, string Rationale);

public sealed record AddRiskMitigationPlanRequest(string Description, Guid ResponsibleUserId, DateTimeOffset DueAtUtc);

public sealed record AddRiskControlRequest(string Name, RiskControlType Type, string Description, bool IsEffective);

public sealed record AddRiskEvidenceRequest(Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);

public sealed record AddRiskAttachmentRequest(Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);

public sealed record ScheduleRiskReviewRequest(DateTimeOffset DueAtUtc);

public sealed record CompleteRiskReviewRequest(Guid ReviewId, string Summary);

public sealed record AddRiskIndicatorRequest(string Name, decimal Value, decimal Threshold);

public sealed record AttachRiskWorkflowRequest(Guid WorkflowInstanceId);

public sealed record ReopenRiskRequest(string Reason);

public sealed record CreateIndicatorCategoryRequest(string Name, string Code);

public sealed record CreateQualityIndicatorRequest(Guid CategoryId, string Name, string Code, string Description, IndicatorType Type, IndicatorFrequency Frequency, IndicatorCalculationType CalculationType, string Unit, Guid? SupplierId, Guid? AuditId, Guid? CapaId, Guid? RiskId, Guid? DocumentId);

public sealed record DefineIndicatorFormulaRequest(string Expression, IndicatorCalculationType CalculationType);

public sealed record DefineIndicatorTargetRequest(decimal TargetValue, DateTimeOffset EffectiveFromUtc);

public sealed record DefineIndicatorThresholdRequest(decimal WarningMinimum, decimal CriticalMinimum, decimal ExcellentMinimum);

public sealed record AddIndicatorPeriodRequest(int Year, int PeriodNumber, DateTimeOffset StartUtc, DateTimeOffset EndUtc);

public sealed record AssociateIndicatorProcessRequest(string ProcessName, string Area);

public sealed record CaptureIndicatorMeasurementRequest(Guid PeriodId, decimal Numerator, decimal? Denominator, bool IsAutomatic);

public sealed record CalculateIndicatorResultRequest(Guid PeriodId, Guid MeasurementId);

public sealed record AddIndicatorAttachmentRequest(Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);

public sealed record AttachIndicatorWorkflowRequest(Guid WorkflowInstanceId);

public sealed record CreateReportCategoryRequest(string Name, string Code, ReportModule Module);

public sealed record CreateReportDefinitionRequest(Guid CategoryId, string Name, string Code, string Description, ReportModule Module, string DatasetKey);

public sealed record AddReportTemplateRequest(string Name, ReportFormat Format, string Content);

public sealed record AddReportParameterRequest(string Name, string Label, ReportParameterType Type, bool IsRequired, string? DefaultValue);

public sealed record GrantReportPermissionRequest(ReportPermissionScope Scope, string Subject, bool CanExecute, bool CanExport, bool CanSchedule);

public sealed record ExecuteReportRequest(string ParametersJson);

public sealed record CompleteReportExecutionRequest(Guid ExecutionId, int RowCount, string DatasetDescriptorJson);

public sealed record ExportReportRequest(Guid ExecutionId, ReportFormat Format);

public sealed record ScheduleReportRequest(ReportScheduleFrequency Frequency, DateTimeOffset NextRunUtc);

public sealed record SubscribeReportRequest(string Recipient, ReportFormat Format);

public sealed record BindReportDashboardRequest(string DashboardKey, string DatasetKey);
