using P = Compliance360.Domain.Identity.PermissionCatalog;

namespace Compliance360.Domain.Identity;

/// <summary>
/// Official role catalog for Compliance 360. Roles exist here as code-first
/// definitions (not ad-hoc strings) with an explicit, auditable permission
/// template. Platform roles are seeded once in the platform tenant; tenant
/// roles are provisioned automatically inside every business tenant.
/// </summary>
public static class RoleCatalog
{
    // Platform roles
    public const string PlatformAdministrator = "Platform Administrator";
    public const string PlatformOperations = "Platform Operations";
    public const string PlatformSecurity = "Platform Security";
    public const string SupportOperator = "Support Operator";

    // Tenant roles
    public const string TenantAdministrator = "Tenant Administrator";
    public const string TenantSecurityAdministrator = "Tenant Security Administrator";
    public const string DocumentController = "Document Controller";
    public const string QualityManager = "Quality Manager";
    public const string Auditor = "Auditor";
    public const string SupplierManager = "Supplier Manager";
    public const string CapaManager = "CAPA Manager";
    public const string RiskManager = "Risk Manager";
    public const string IndicatorsManager = "Indicators Manager";
    public const string ReportingManager = "Reporting Manager";
    public const string StorageAdministrator = "Storage Administrator";
    public const string NotificationAdministrator = "Notification Administrator";
    public const string Viewer = "Viewer";

    private static readonly RoleDefinition[] Definitions =
    [
        // ---------------------------- Platform ----------------------------
        new(PlatformAdministrator, PlatformAdministrator, RoleScope.Platform,
            "Administers the whole SaaS platform. Does not operate tenant business data.",
            [
                P.PlatformDashboardRead, P.PlatformTenantRead, P.PlatformTenantCreate, P.PlatformTenantUpdate,
                P.PlatformTenantStatus, P.PlatformTenantDelete, P.PlatformTenantRestore, P.PlatformLicenseManage,
                P.PlatformModuleManage, P.PlatformProviderRead, P.PlatformProviderManage, P.PlatformSecurityManage,
                P.PlatformObservabilityRead, P.PlatformAuditRead, P.PlatformAuditExport, P.PlatformDatabaseRead,
                P.PlatformAiManage, P.PlatformConfigurationManage, P.PlatformBackupRead, P.PlatformDevOpsRead,
                P.PlatformSearch,
            ]),
        new(PlatformOperations, PlatformOperations, RoleScope.Platform,
            "Runs day-to-day platform operations: tenant lifecycle, providers, modules and licenses.",
            [
                P.PlatformDashboardRead, P.PlatformTenantRead, P.PlatformTenantCreate, P.PlatformTenantUpdate,
                P.PlatformTenantStatus, P.PlatformTenantRestore, P.PlatformLicenseManage, P.PlatformModuleManage,
                P.PlatformProviderRead, P.PlatformProviderManage, P.PlatformObservabilityRead, P.PlatformDatabaseRead,
                P.PlatformBackupRead, P.PlatformDevOpsRead, P.PlatformSearch,
            ]),
        new(PlatformSecurity, PlatformSecurity, RoleScope.Platform,
            "Owns global security configuration and the platform audit trail.",
            [
                P.PlatformDashboardRead, P.PlatformSecurityManage, P.PlatformAuditRead, P.PlatformAuditExport,
                P.PlatformObservabilityRead, P.PlatformSearch,
            ]),
        new(SupportOperator, SupportOperator, RoleScope.Platform,
            "Break-glass support role. Only gains tenant access through the explicit, audited support mechanism.",
            [
                P.PlatformDashboardRead, P.PlatformSupportAccess, P.PlatformAuditRead, P.PlatformSearch,
            ]),

        // ----------------------------- Tenant -----------------------------
        new(TenantAdministrator, TenantAdministrator, RoleScope.Tenant,
            "Administers the tenant: profile, users, roles and general settings. Does not operate business data by default.",
            [
                P.TenantRead, P.TenantUpdate, P.TenantBranding, P.TenantBilling, P.TenantIntegrations,
                P.TenantHealth, P.TenantBackup, P.TenantUsers, P.TenantRoles, P.IdentityManage, P.RbacManage,
                P.TenantAudit, P.AuditRead,
            ]),
        new(TenantSecurityAdministrator, TenantSecurityAdministrator, RoleScope.Tenant,
            "Owns tenant security: MFA, SSO, domains, webhooks and API keys.",
            [
                P.TenantRead, P.TenantSecurity, P.TenantDomains, P.TenantSso, P.TenantWebhooks, P.TenantApiKeys,
                P.TenantAudit, P.AuditRead,
            ]),
        new(DocumentController, DocumentController, RoleScope.Tenant,
            "Creates and maintains controlled documents and their workflows. Cannot approve documents (SoD).",
            [
                P.TenantRead, P.DocumentRead, P.DocumentCreate, P.DocumentUpdate,
                P.WorkflowRead, P.WorkflowCreate, P.WorkflowUpdate, P.AuditRead,
            ]),
        new(QualityManager, QualityManager, RoleScope.Tenant,
            "Approver and coordinator. Approves documents, workflows, CAPAs, risks and technical sheets. Does not create business data.",
            [
                P.TenantRead, P.DocumentRead, P.DocumentApprove, P.WorkflowRead, P.WorkflowApprove,
                P.TechnicalSheetRead, P.TechnicalSheetApprove, P.CapaRead, P.CapaApprove, P.CapaClose,
                P.RiskRead, P.RiskApprove, P.RiskClose, P.IndicatorRead, P.IndicatorApprove,
                P.AuditManagementRead, P.ReportRead, P.ReportExecute, P.AuditRead,
            ]),
        new(Auditor, Auditor, RoleScope.Tenant,
            "Plans and executes audits and records findings. Cannot manage or close the CAPAs raised from findings (SoD).",
            [
                P.TenantRead, P.AuditManagementRead, P.AuditManagementManage, P.CapaRead,
                P.DocumentRead, P.SupplierRead, P.ReportRead, P.AuditRead,
            ]),
        new(SupplierManager, SupplierManager, RoleScope.Tenant,
            "Manages suppliers, their documents, evaluations and homologation.",
            [
                P.TenantRead, P.SupplierRead, P.SupplierCreate, P.SupplierUpdate, P.SupplierApprove,
                P.DocumentRead, P.ReportRead, P.AuditRead,
            ]),
        new(CapaManager, CapaManager, RoleScope.Tenant,
            "Manages CAPAs end to end except final approval, which belongs to the Quality Manager (SoD).",
            [
                P.TenantRead, P.CapaRead, P.CapaManage, P.RiskRead, P.AuditManagementRead,
                P.ReportRead, P.AuditRead,
            ]),
        new(RiskManager, RiskManager, RoleScope.Tenant,
            "Manages the risk register, treatments and controls. Risk approval belongs to the Quality Manager (SoD).",
            [
                P.TenantRead, P.RiskRead, P.RiskManage, P.IndicatorRead, P.AuditManagementRead,
                P.ReportRead, P.AuditRead,
            ]),
        new(IndicatorsManager, IndicatorsManager, RoleScope.Tenant,
            "Manages quality indicators, formulas, thresholds and measurements.",
            [
                P.TenantRead, P.IndicatorRead, P.IndicatorManage, P.IndicatorExport, P.RiskRead,
                P.ReportRead, P.AuditRead,
            ]),
        new(ReportingManager, ReportingManager, RoleScope.Tenant,
            "Owns the report center: execution, export and scheduling.",
            [
                P.TenantRead, P.ReportRead, P.ReportManage, P.ReportExecute, P.ReportExport, P.ReportSchedule,
                P.IndicatorRead, P.RiskRead, P.CapaRead, P.AuditManagementRead, P.AuditRead,
            ]),
        new(StorageAdministrator, StorageAdministrator, RoleScope.Tenant,
            "Administers document storage and providers. Does not administer notifications/SMTP (SoD).",
            [
                P.TenantRead, P.TenantStorage, P.StorageRead, P.StorageCreate, P.StorageUpdate, P.StorageDelete,
                P.AuditRead,
            ]),
        new(NotificationAdministrator, NotificationAdministrator, RoleScope.Tenant,
            "Administers notifications, templates and SMTP providers. Does not administer storage (SoD).",
            [
                P.TenantRead, P.TenantNotifications, P.NotificationRead, P.NotificationSend, P.NotificationTemplate,
                P.NotificationManage, P.NotificationAdmin, P.AuditRead,
            ]),
        new(Viewer, Viewer, RoleScope.Tenant,
            "Read-only access across the tenant modules. Cannot create, edit, approve or configure anything.",
            [
                P.TenantRead, P.DocumentRead, P.TechnicalSheetRead, P.SupplierRead, P.AuditManagementRead,
                P.CapaRead, P.RiskRead, P.IndicatorRead, P.ReportRead, P.NotificationRead, P.AuditRead,
            ]),
    ];

    public static IReadOnlyList<RoleDefinition> All => Definitions;

    public static IReadOnlyList<RoleDefinition> PlatformRoles { get; } =
        Definitions.Where(d => d.Scope == RoleScope.Platform).ToArray();

    public static IReadOnlyList<RoleDefinition> TenantRoles { get; } =
        Definitions.Where(d => d.Scope == RoleScope.Tenant).ToArray();

    public static RoleDefinition? Find(string name) =>
        Definitions.FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
}
