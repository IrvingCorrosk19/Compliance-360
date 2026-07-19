namespace Compliance360.Domain.Identity;

/// <summary>
/// Scope of a role in the RBAC model.
/// Platform roles live in the platform tenant and administer the SaaS platform.
/// Tenant roles are provisioned automatically inside every business tenant.
/// </summary>
public enum RoleScope
{
    Platform,
    Tenant
}

/// <summary>
/// Immutable definition of a permission in the official catalog.
/// <see cref="Code"/> is the single source of truth used by JWT claims,
/// authorization policies and the frontend.
/// </summary>
public sealed record PermissionDefinition(string Code, PermissionAction Action, string Description);

/// <summary>
/// Immutable definition of a role in the official catalog, including the exact
/// set of permission codes granted to it.
/// </summary>
public sealed record RoleDefinition(
    string Key,
    string Name,
    RoleScope Scope,
    string Description,
    IReadOnlyList<string> PermissionCodes);

/// <summary>
/// Official permission catalog for Compliance 360. Every permission that exists
/// in the platform is declared here exactly once and follows a uniform
/// MODULE(.SUBMODULE).ACTION pattern. Backend policies, seeds and the frontend
/// must reference these codes and nothing else.
/// </summary>
public static class PermissionCatalog
{
    // ---- Platform administration (platform-scoped) ------------------------
    public const string PlatformDashboardRead = "PLATFORM.DASHBOARD.READ";
    public const string PlatformTenantRead = "PLATFORM.TENANT.READ";
    public const string PlatformTenantCreate = "PLATFORM.TENANT.CREATE";
    public const string PlatformTenantUpdate = "PLATFORM.TENANT.UPDATE";
    public const string PlatformTenantStatus = "PLATFORM.TENANT.STATUS";
    public const string PlatformTenantDelete = "PLATFORM.TENANT.DELETE";
    public const string PlatformTenantRestore = "PLATFORM.TENANT.RESTORE";
    public const string PlatformLicenseManage = "PLATFORM.LICENSE.MANAGE";
    public const string PlatformModuleManage = "PLATFORM.MODULE.MANAGE";
    public const string PlatformProviderRead = "PLATFORM.PROVIDER.READ";
    public const string PlatformProviderManage = "PLATFORM.PROVIDER.MANAGE";
    public const string PlatformSecurityManage = "PLATFORM.SECURITY.MANAGE";
    public const string PlatformObservabilityRead = "PLATFORM.OBSERVABILITY.READ";
    public const string PlatformAuditRead = "PLATFORM.AUDIT.READ";
    public const string PlatformAuditExport = "PLATFORM.AUDIT.EXPORT";
    public const string PlatformDatabaseRead = "PLATFORM.DATABASE.READ";
    public const string PlatformAiManage = "PLATFORM.AI.MANAGE";
    public const string PlatformConfigurationManage = "PLATFORM.CONFIGURATION.MANAGE";
    public const string PlatformBackupRead = "PLATFORM.BACKUP.READ";
    public const string PlatformDevOpsRead = "PLATFORM.DEVOPS.READ";
    public const string PlatformSearch = "PLATFORM.SEARCH";
    public const string PlatformSupportAccess = "PLATFORM.SUPPORT.ACCESS";

    // ---- Tenant administration (tenant-scoped) ----------------------------
    public const string TenantRead = "TENANT.READ";
    public const string TenantUpdate = "TENANT.UPDATE";
    public const string TenantBranding = "TENANT.BRANDING";
    public const string TenantSecurity = "TENANT.SECURITY";
    public const string TenantStorage = "TENANT.STORAGE";
    public const string TenantNotifications = "TENANT.NOTIFICATIONS";
    public const string TenantIntegrations = "TENANT.INTEGRATIONS";
    public const string TenantBilling = "TENANT.BILLING";
    public const string TenantUsers = "TENANT.USERS";
    public const string TenantRoles = "TENANT.ROLES";
    public const string TenantAudit = "TENANT.AUDIT";
    public const string TenantDomains = "TENANT.DOMAINS";
    public const string TenantSso = "TENANT.SSO";
    public const string TenantWebhooks = "TENANT.WEBHOOKS";
    public const string TenantApiKeys = "TENANT.API_KEYS";
    public const string TenantHealth = "TENANT.HEALTH";
    public const string TenantBackup = "TENANT.BACKUP";
    public const string IdentityManage = "IDENTITY.MANAGE";
    public const string RbacManage = "RBAC.MANAGE";
    public const string AuditRead = "AUDIT.READ";

    // ---- Document control -------------------------------------------------
    public const string DocumentRead = "DOCUMENT.READ";
    public const string DocumentCreate = "DOCUMENT.CREATE";
    public const string DocumentUpdate = "DOCUMENT.UPDATE";
    public const string DocumentApprove = "DOCUMENT.APPROVE";

    // ---- Workflow ---------------------------------------------------------
    public const string WorkflowRead = "WORKFLOW.READ";
    public const string WorkflowCreate = "WORKFLOW.CREATE";
    public const string WorkflowUpdate = "WORKFLOW.UPDATE";
    public const string WorkflowApprove = "WORKFLOW.APPROVE";

    // ---- Technical sheets -------------------------------------------------
    public const string TechnicalSheetRead = "TECHNICALSHEET.READ";
    public const string TechnicalSheetCreate = "TECHNICALSHEET.CREATE";
    public const string TechnicalSheetUpdate = "TECHNICALSHEET.UPDATE";
    public const string TechnicalSheetApprove = "TECHNICALSHEET.APPROVE";

    // ---- Suppliers --------------------------------------------------------
    public const string SupplierRead = "SUPPLIER.READ";
    public const string SupplierCreate = "SUPPLIER.CREATE";
    public const string SupplierUpdate = "SUPPLIER.UPDATE";
    public const string SupplierApprove = "SUPPLIER.APPROVE";

    // ---- Audit management (programs / findings) ---------------------------
    public const string AuditManagementRead = "AUDITMANAGEMENT.READ";
    public const string AuditManagementManage = "AUDITMANAGEMENT.MANAGE";

    // ---- CAPA -------------------------------------------------------------
    public const string CapaRead = "CAPA.READ";
    public const string CapaManage = "CAPA.MANAGE";
    public const string CapaApprove = "CAPA.APPROVE";
    public const string CapaClose = "CAPA.CLOSE";

    // ---- Risk -------------------------------------------------------------
    public const string RiskRead = "RISK.READ";
    public const string RiskManage = "RISK.MANAGE";
    public const string RiskApprove = "RISK.APPROVE";
    public const string RiskClose = "RISK.CLOSE";

    // ---- Indicators -------------------------------------------------------
    public const string IndicatorRead = "INDICATOR.READ";
    public const string IndicatorManage = "INDICATOR.MANAGE";
    public const string IndicatorApprove = "INDICATOR.APPROVE";
    public const string IndicatorExport = "INDICATOR.EXPORT";

    // ---- Reporting --------------------------------------------------------
    public const string ReportRead = "REPORT.READ";
    public const string ReportManage = "REPORT.MANAGE";
    public const string ReportExecute = "REPORT.EXECUTE";
    public const string ReportExport = "REPORT.EXPORT";
    public const string ReportSchedule = "REPORT.SCHEDULE";

    // ---- Storage ----------------------------------------------------------
    public const string StorageRead = "STORAGE.READ";
    public const string StorageCreate = "STORAGE.CREATE";
    public const string StorageUpdate = "STORAGE.UPDATE";
    public const string StorageDelete = "STORAGE.DELETE";

    // ---- Notifications ----------------------------------------------------
    public const string NotificationRead = "NOTIFICATION.READ";
    public const string NotificationSend = "NOTIFICATION.SEND";
    public const string NotificationTemplate = "NOTIFICATION.TEMPLATE";
    public const string NotificationManage = "NOTIFICATION.MANAGE";
    public const string NotificationAdmin = "NOTIFICATION.ADMIN";

    // ---- Observability ----------------------------------------------------
    public const string ObservabilityRead = "OBSERVABILITY.READ";
    public const string ObservabilityManage = "OBSERVABILITY.MANAGE";
    public const string ObservabilityAdmin = "OBSERVABILITY.ADMIN";

    // ---- Form templates (visual builder) ------------------------------------
    public const string TemplateRead = "TEMPLATE.READ";
    public const string TemplateManage = "TEMPLATE.MANAGE";

    // ---- Regulatory Affairs (Sanitary Registration Case Management) ---------
    public const string RegulatoryProductRead = "REGULATORY.PRODUCT.READ";
    public const string RegulatoryProductManage = "REGULATORY.PRODUCT.MANAGE";
    public const string RegulatoryDossierRead = "REGULATORY.DOSSIER.READ";
    public const string RegulatoryDossierCreate = "REGULATORY.DOSSIER.CREATE";
    public const string RegulatoryDossierUpdate = "REGULATORY.DOSSIER.UPDATE";
    public const string RegulatoryDossierSubmit = "REGULATORY.DOSSIER.SUBMIT";
    public const string RegulatoryDossierApprove = "REGULATORY.DOSSIER.APPROVE";
    public const string RegulatoryDossierReview = "REGULATORY.DOSSIER.REVIEW";
    public const string RegulatoryDossierApproveForSubmission = "REGULATORY.DOSSIER.APPROVE_FOR_SUBMISSION";
    public const string RegulatoryRequirementManage = "REGULATORY.REQUIREMENT.MANAGE";
    public const string RegulatoryObservationManage = "REGULATORY.OBSERVATION.MANAGE";
    public const string RegulatoryRegistrationRead = "REGULATORY.REGISTRATION.READ";
    public const string RegulatoryRegistrationManage = "REGULATORY.REGISTRATION.MANAGE";
    public const string RegulatoryManufacturerDocumentRead = "REGULATORY.MANUFACTURER_DOCUMENT.READ";
    public const string RegulatoryManufacturerDocumentManage = "REGULATORY.MANUFACTURER_DOCUMENT.MANAGE";
    public const string RegulatoryLicenseRead = "REGULATORY.LICENSE.READ";
    public const string RegulatoryLicenseManage = "REGULATORY.LICENSE.MANAGE";
    public const string RegulatoryReportRead = "REGULATORY.REPORT.READ";
    public const string RegulatoryConfigure = "REGULATORY.CONFIGURE";
    public const string RegulatorySoDManage = "REGULATORY.SOD.MANAGE";
    public const string RegulatorySoDEmergencyOverride = "REGULATORY.SOD.EMERGENCY_OVERRIDE";

    private static readonly PermissionDefinition[] Definitions =
    [
        // Platform
        new(PlatformDashboardRead, PermissionAction.Read, "Read the platform command center dashboard."),
        new(PlatformTenantRead, PermissionAction.Read, "List and inspect tenants across the platform."),
        new(PlatformTenantCreate, PermissionAction.Create, "Provision a new tenant."),
        new(PlatformTenantUpdate, PermissionAction.Update, "Update global tenant profile and limits."),
        new(PlatformTenantStatus, PermissionAction.Update, "Suspend, activate or change a tenant lifecycle state."),
        new(PlatformTenantDelete, PermissionAction.Delete, "Archive/decommission a tenant."),
        new(PlatformTenantRestore, PermissionAction.Update, "Restore an archived tenant."),
        new(PlatformLicenseManage, PermissionAction.Manage, "Manage licenses, plans and subscriptions."),
        new(PlatformModuleManage, PermissionAction.Manage, "Manage available platform modules and feature flags."),
        new(PlatformProviderRead, PermissionAction.Read, "Read global provider configuration."),
        new(PlatformProviderManage, PermissionAction.Manage, "Manage global provider configuration."),
        new(PlatformSecurityManage, PermissionAction.Manage, "Manage global security configuration (SSO, OAuth, API keys)."),
        new(PlatformObservabilityRead, PermissionAction.Read, "Read global observability metrics and health."),
        new(PlatformAuditRead, PermissionAction.Read, "Read the global audit trail."),
        new(PlatformAuditExport, PermissionAction.Export, "Export the global audit trail."),
        new(PlatformDatabaseRead, PermissionAction.Read, "Read database health and migration metrics."),
        new(PlatformAiManage, PermissionAction.Manage, "Manage global AI configuration."),
        new(PlatformConfigurationManage, PermissionAction.Manage, "Manage global system configuration."),
        new(PlatformBackupRead, PermissionAction.Read, "Read backup status."),
        new(PlatformDevOpsRead, PermissionAction.Read, "Read DevOps / release information."),
        new(PlatformSearch, PermissionAction.Read, "Use the global platform search."),
        new(PlatformSupportAccess, PermissionAction.Manage, "Break-glass explicit and audited access into a tenant for support."),

        // Tenant administration
        new(TenantRead, PermissionAction.Read, "Read the tenant profile and dashboards."),
        new(TenantUpdate, PermissionAction.Update, "Update the tenant company profile."),
        new(TenantBranding, PermissionAction.Update, "Manage tenant branding."),
        new(TenantSecurity, PermissionAction.Manage, "Manage tenant security settings (MFA, policies)."),
        new(TenantStorage, PermissionAction.Manage, "Manage tenant storage settings."),
        new(TenantNotifications, PermissionAction.Manage, "Manage tenant notification settings."),
        new(TenantIntegrations, PermissionAction.Manage, "Manage tenant integrations."),
        new(TenantBilling, PermissionAction.Read, "Read tenant billing information."),
        new(TenantUsers, PermissionAction.Manage, "Manage tenant users."),
        new(TenantRoles, PermissionAction.Manage, "Manage tenant roles."),
        new(TenantAudit, PermissionAction.Read, "Read the tenant audit trail."),
        new(TenantDomains, PermissionAction.Manage, "Manage tenant domains."),
        new(TenantSso, PermissionAction.Manage, "Manage tenant SSO."),
        new(TenantWebhooks, PermissionAction.Manage, "Manage tenant webhooks."),
        new(TenantApiKeys, PermissionAction.Manage, "Manage tenant API keys."),
        new(TenantHealth, PermissionAction.Read, "Read tenant health."),
        new(TenantBackup, PermissionAction.Read, "Read tenant backup status."),
        new(IdentityManage, PermissionAction.Manage, "Manage identities (users, credentials, MFA enrolment)."),
        new(RbacManage, PermissionAction.Manage, "Manage RBAC roles and permission grants inside the tenant."),
        new(AuditRead, PermissionAction.Read, "Read the audit trail."),

        // Documents
        new(DocumentRead, PermissionAction.Read, "Read and search documents."),
        new(DocumentCreate, PermissionAction.Create, "Create documents, types, categories and versions."),
        new(DocumentUpdate, PermissionAction.Update, "Update document lifecycle, permissions and submissions."),
        new(DocumentApprove, PermissionAction.Approve, "Approve or reject documents."),

        // Workflows
        new(WorkflowRead, PermissionAction.Read, "Read workflows and instances."),
        new(WorkflowCreate, PermissionAction.Create, "Create workflow definitions, steps, rules and instances."),
        new(WorkflowUpdate, PermissionAction.Update, "Activate, assign, escalate and remind on workflows."),
        new(WorkflowApprove, PermissionAction.Approve, "Complete workflow decisions."),

        // Technical sheets
        new(TechnicalSheetRead, PermissionAction.Read, "Read technical sheets."),
        new(TechnicalSheetCreate, PermissionAction.Create, "Create technical sheets, products and versions."),
        new(TechnicalSheetUpdate, PermissionAction.Update, "Update technical sheets (PDF, obsolete, submit)."),
        new(TechnicalSheetApprove, PermissionAction.Approve, "Approve or reject technical sheets."),

        // Suppliers
        new(SupplierRead, PermissionAction.Read, "Read suppliers."),
        new(SupplierCreate, PermissionAction.Create, "Create suppliers, documents, evaluations and alerts."),
        new(SupplierUpdate, PermissionAction.Update, "Update suppliers (suspend, reject documents)."),
        new(SupplierApprove, PermissionAction.Approve, "Validate documents and homologate suppliers."),

        // Audit management
        new(AuditManagementRead, PermissionAction.Read, "Read audit programs, plans and findings."),
        new(AuditManagementManage, PermissionAction.Manage, "Manage audit programs, checklists, plans and findings."),

        // CAPA
        new(CapaRead, PermissionAction.Read, "Read CAPAs."),
        new(CapaManage, PermissionAction.Manage, "Create and manage CAPAs (root cause, actions, evidence)."),
        new(CapaApprove, PermissionAction.Approve, "Approve CAPAs and verify effectiveness."),
        new(CapaClose, PermissionAction.Manage, "Close and reopen CAPAs."),

        // Risk
        new(RiskRead, PermissionAction.Read, "Read risks and matrices."),
        new(RiskManage, PermissionAction.Manage, "Create and manage risks, treatments and controls."),
        new(RiskApprove, PermissionAction.Approve, "Approve risk assessments."),
        new(RiskClose, PermissionAction.Manage, "Close risks."),

        // Indicators
        new(IndicatorRead, PermissionAction.Read, "Read indicators and trends."),
        new(IndicatorManage, PermissionAction.Manage, "Create and manage indicators, formulas and measurements."),
        new(IndicatorApprove, PermissionAction.Approve, "Approve indicator definitions."),
        new(IndicatorExport, PermissionAction.Export, "Export indicators."),

        // Reporting
        new(ReportRead, PermissionAction.Read, "Read reports."),
        new(ReportManage, PermissionAction.Manage, "Manage report definitions."),
        new(ReportExecute, PermissionAction.Read, "Execute reports."),
        new(ReportExport, PermissionAction.Export, "Export reports (PDF, Excel, Word)."),
        new(ReportSchedule, PermissionAction.Manage, "Schedule reports."),

        // Storage
        new(StorageRead, PermissionAction.Read, "Read stored files and provider configuration."),
        new(StorageCreate, PermissionAction.Create, "Upload files and create storage providers."),
        new(StorageUpdate, PermissionAction.Update, "Update file status and storage providers."),
        new(StorageDelete, PermissionAction.Delete, "Delete stored files."),

        // Notifications
        new(NotificationRead, PermissionAction.Read, "Read notification history and tracking."),
        new(NotificationSend, PermissionAction.Create, "Queue, send and retry notifications."),
        new(NotificationTemplate, PermissionAction.Create, "Manage notification templates."),
        new(NotificationManage, PermissionAction.Manage, "Manage notifications (cancel, dead letters)."),
        new(NotificationAdmin, PermissionAction.Manage, "Administer notification providers (SMTP)."),

        // Observability (tenant/support-facing)
        new(ObservabilityRead, PermissionAction.Read, "Read observability signals."),
        new(ObservabilityManage, PermissionAction.Manage, "Manage observability configuration."),
        new(ObservabilityAdmin, PermissionAction.Manage, "Administer observability."),

        new(TemplateRead, PermissionAction.Read, "Read form templates and published schemas."),
        new(TemplateManage, PermissionAction.Manage, "Create, design, version, publish and archive form templates."),
        new(RegulatoryProductRead, PermissionAction.Read, "Read medical device products in Regulatory Affairs."),
        new(RegulatoryProductManage, PermissionAction.Manage, "Create and update medical device products."),
        new(RegulatoryDossierRead, PermissionAction.Read, "Read registration dossiers and pipelines."),
        new(RegulatoryDossierCreate, PermissionAction.Create, "Create regulatory dossiers."),
        new(RegulatoryDossierUpdate, PermissionAction.Update, "Update dossiers, dates and checklist items."),
        new(RegulatoryDossierSubmit, PermissionAction.Update, "Record dossier submission to the sanitary authority (not internal clearance)."),
        new(RegulatoryDossierApprove, PermissionAction.Approve, "Record external authority approval and activate CT/RS (MINSA/CSS decision — not internal clearance)."),
        new(RegulatoryDossierReview, PermissionAction.Approve, "Technically review dossier requirements (accept/reject)."),
        new(RegulatoryDossierApproveForSubmission, PermissionAction.Approve, "Grant internal clearance authorizing submission (not MINSA/CSS approval)."),
        new(RegulatoryRequirementManage, PermissionAction.Manage, "Manage dossier requirements and packs."),
        new(RegulatoryObservationManage, PermissionAction.Manage, "Record and close authority observations."),
        new(RegulatoryRegistrationRead, PermissionAction.Read, "Read sanitary registrations."),
        new(RegulatoryRegistrationManage, PermissionAction.Manage, "Manage sanitary registrations and renewals."),
        new(RegulatoryManufacturerDocumentRead, PermissionAction.Read, "Read manufacturer profiles and certificates."),
        new(RegulatoryManufacturerDocumentManage, PermissionAction.Manage, "Manage manufacturer certificates."),
        new(RegulatoryLicenseRead, PermissionAction.Read, "Read corporate operating licenses."),
        new(RegulatoryLicenseManage, PermissionAction.Manage, "Manage operating licenses and renewals."),
        new(RegulatoryReportRead, PermissionAction.Read, "Read Regulatory Affairs dashboards and reports."),
        new(RegulatoryConfigure, PermissionAction.Manage, "Configure authorities, packs and regulatory settings."),
        new(RegulatorySoDManage, PermissionAction.Manage, "Configure Regulatory Affairs segregation-of-duties policy."),
        new(RegulatorySoDEmergencyOverride, PermissionAction.Manage, "Audited break-glass override of SoD controls."),
    ];

    public static IReadOnlyList<PermissionDefinition> All => Definitions;

    public static IReadOnlyList<string> AllCodes { get; } = Definitions.Select(d => d.Code).ToArray();

    public static PermissionDefinition? Find(string code) =>
        Definitions.FirstOrDefault(d => string.Equals(d.Code, code, StringComparison.OrdinalIgnoreCase));
}
