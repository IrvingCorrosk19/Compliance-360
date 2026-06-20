using Compliance360.Domain.Audit;
using Compliance360.Domain.AuditManagement;
using Compliance360.Domain.CapaManagement;
using Compliance360.Domain.Documents;
using Compliance360.Domain.Identity;
using Compliance360.Domain.Notifications;
using Compliance360.Domain.Suppliers;
using Compliance360.Domain.TenantManagement;
using Compliance360.Domain.TechnicalSheets;
using Compliance360.Domain.Workflows;

namespace Compliance360.Web.Api;

public sealed record LoginRequest(Guid TenantId, string Email, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record LogoutRequest(Guid TenantId, Guid UserId, string RefreshTokenHash);

public sealed record ChangePasswordRequest(Guid TenantId, string CurrentPassword, string NewPassword);

public sealed record CreateTenantRequest(string Name, string Slug);

public sealed record AddCompanyRequest(string LegalName, string TaxIdentifier, string CountryCode);

public sealed record ConfigureTenantSettingsRequest(string Culture, string TimeZone, bool RequireMfa, int DocumentRetentionDays);

public sealed record ConfigureTenantBrandingRequest(string DisplayName, string? LogoUri, string PrimaryColor, string SecondaryColor);

public sealed record ChangeSubscriptionRequest(SubscriptionPlan Plan, int MaxUsers, int MaxStorageGb);

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

public sealed record CreateNotificationTemplateRequest(string Code, NotificationChannel Channel, string Subject, string Body);

public sealed record QueueNotificationRequest(
    NotificationChannel Channel,
    string Recipient,
    string? Subject,
    string? Body,
    string? TemplateCode,
    IReadOnlyDictionary<string, string> Variables,
    NotificationPriority Priority,
    Guid? TargetUserId);

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

public sealed record AddCapaEvidenceRequest(Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);

public sealed record AddCapaAttachmentRequest(Guid StoredFileId, string FileName, string ContentType, long SizeBytes, string Sha256Hash);

public sealed record RegisterCapaFollowUpRequest(string Notes);

public sealed record VerifyCapaEffectivenessRequest(bool IsEffective, string VerificationSummary);

public sealed record AttachCapaWorkflowRequest(Guid WorkflowInstanceId);

public sealed record ReopenCapaRequest(string Reason);
