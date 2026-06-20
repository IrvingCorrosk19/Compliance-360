using Compliance360.Domain.Audit;
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
