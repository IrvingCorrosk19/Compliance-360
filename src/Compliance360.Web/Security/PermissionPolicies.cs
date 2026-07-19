using Compliance360.Domain.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Compliance360.Web.Security;

/// <summary>
/// Registers one authorization policy per official permission code plus a set of
/// friendly named policies used by the API endpoints. There is no role-based
/// bypass: authorization is always evaluated against the permission claims that
/// were issued from the official RBAC catalog. Platform-wide access is granted
/// through explicit PLATFORM.* permissions, never through a magic role name.
/// </summary>
public static class PermissionPolicies
{
    // ---- Platform (friendly names kept for endpoint stability) ------------
    public const string SuperAdminDashboard = "Platform.Dashboard";
    public const string SuperAdminTenantsRead = "Platform.Tenants.Read";
    public const string SuperAdminTenantsCreate = "Platform.Tenants.Create";
    public const string SuperAdminTenantsUpdate = "Platform.Tenants.Update";
    public const string SuperAdminTenantsStatus = "Platform.Tenants.Status";
    public const string SuperAdminTenantsDelete = "Platform.Tenants.Delete";
    public const string SuperAdminLicenses = "Platform.Licenses";
    public const string SuperAdminModules = "Platform.Modules";
    public const string SuperAdminProviders = "Platform.Providers";
    public const string SuperAdminSecurity = "Platform.Security";
    public const string SuperAdminObservability = "Platform.Observability";
    public const string SuperAdminAudit = "Platform.Audit";
    public const string SuperAdminDatabase = "Platform.Database";
    public const string SuperAdminAi = "Platform.Ai";
    public const string SuperAdminConfiguration = "Platform.Configuration";
    public const string SuperAdminBackups = "Platform.Backups";
    public const string SuperAdminDevOps = "Platform.DevOps";
    public const string SuperAdminSearch = "Platform.Search";
    public const string PlatformSupportAccess = "Platform.Support.Access";

    // ---- Tenant administration --------------------------------------------
    public const string TenantManage = "Tenant.Manage";
    public const string TenantRead = "Tenant.Read";
    public const string TenantCreate = "Tenant.Create";
    public const string TenantUpdate = "Tenant.Update";
    public const string TenantStatus = "Tenant.Status";
    public const string TenantBranding = "Tenant.Branding";
    public const string TenantSecurity = "Tenant.Security";
    public const string TenantStorage = "Tenant.Storage";
    public const string TenantNotifications = "Tenant.Notifications";
    public const string TenantIntegrations = "Tenant.Integrations";
    public const string TenantBilling = "Tenant.Billing";
    public const string TenantUsers = "Tenant.Users";
    public const string TenantRoles = "Tenant.Roles";
    public const string TenantAudit = "Tenant.Audit";
    public const string TenantDelete = "Tenant.Delete";
    public const string TenantRestore = "Tenant.Restore";
    public const string TenantDomains = "Tenant.Domains";
    public const string TenantSso = "Tenant.Sso";
    public const string TenantWebhooks = "Tenant.Webhooks";
    public const string TenantApiKeys = "Tenant.ApiKeys";
    public const string TenantHealth = "Tenant.Health";
    public const string TenantBackup = "Tenant.Backup";
    public const string TemplateRead = "Template.Read";
    public const string TemplateManage = "Template.Manage";
    public const string RegulatoryRead = "Regulatory.Read";
    public const string RegulatoryManage = "Regulatory.Manage";
    public const string RegulatoryPrepare = "Regulatory.Prepare";
    public const string RegulatoryDossierUpdate = "Regulatory.Dossier.Update";
    public const string RegulatoryRequirementUpdate = "Regulatory.Requirement.Update";
    public const string RegulatoryObservationManage = "Regulatory.Observation.Manage";
    public const string RegulatoryManufacturerManage = "Regulatory.Manufacturer.Manage";
    public const string RegulatoryLicenseManage = "Regulatory.License.Manage";
    public const string RegulatoryDossierCreate = "Regulatory.Dossier.Create";
    public const string RegulatoryRegistrationManage = "Regulatory.Registration.Manage";
    public const string RegulatoryEvidenceUpload = "Regulatory.Evidence.Upload";
    public const string RegulatorySubmit = "Regulatory.Submit";
    public const string RegulatoryApprove = "Regulatory.Approve";
    public const string RegulatoryApproveForSubmission = "Regulatory.ApproveForSubmission";
    public const string RegulatoryReview = "Regulatory.Review";
    public const string RegulatoryConfigure = "Regulatory.Configure";
    public const string RegulatorySoDManage = "Regulatory.SoD.Manage";
    public const string RegulatorySoDOverride = "Regulatory.SoD.Override";
    public const string IdentityManage = "Identity.Manage";
    public const string RbacManage = "Rbac.Manage";
    public const string AuditRead = "Audit.Read";

    // ---- Business modules --------------------------------------------------
    public const string AuditManagementRead = "AuditManagement.Read";
    public const string AuditManagementManage = "AuditManagement.Manage";
    public const string CapaManage = "Capa.Manage";
    public const string CapaRead = "Capa.Read";
    public const string CapaApprove = "Capa.Approve";
    public const string CapaClose = "Capa.Close";
    public const string RiskManage = "Risk.Manage";
    public const string RiskRead = "Risk.Read";
    public const string RiskApprove = "Risk.Approve";
    public const string RiskClose = "Risk.Close";
    public const string IndicatorManage = "Indicator.Manage";
    public const string IndicatorRead = "Indicator.Read";
    public const string IndicatorApprove = "Indicator.Approve";
    public const string IndicatorExport = "Indicator.Export";
    public const string ReportManage = "Report.Manage";
    public const string ReportRead = "Report.Read";
    public const string ReportExecute = "Report.Execute";
    public const string ReportExport = "Report.Export";
    public const string ReportSchedule = "Report.Schedule";
    public const string NotificationManage = "Notification.Manage";
    public const string NotificationSend = "Notification.Send";
    public const string NotificationRead = "Notification.Read";
    public const string NotificationTemplate = "Notification.Template";
    public const string NotificationAdmin = "Notification.Admin";

    // Split (granular) module policies. Values equal their catalog code so the
    // per-code policy registration below covers them directly.
    public const string DocumentRead = PermissionCatalog.DocumentRead;
    public const string DocumentCreate = PermissionCatalog.DocumentCreate;
    public const string DocumentUpdate = PermissionCatalog.DocumentUpdate;
    public const string DocumentApprove = PermissionCatalog.DocumentApprove;
    public const string WorkflowRead = PermissionCatalog.WorkflowRead;
    public const string WorkflowCreate = PermissionCatalog.WorkflowCreate;
    public const string WorkflowUpdate = PermissionCatalog.WorkflowUpdate;
    public const string WorkflowApprove = PermissionCatalog.WorkflowApprove;
    public const string TechnicalSheetRead = PermissionCatalog.TechnicalSheetRead;
    public const string TechnicalSheetCreate = PermissionCatalog.TechnicalSheetCreate;
    public const string TechnicalSheetUpdate = PermissionCatalog.TechnicalSheetUpdate;
    public const string TechnicalSheetApprove = PermissionCatalog.TechnicalSheetApprove;
    public const string SupplierRead = PermissionCatalog.SupplierRead;
    public const string SupplierCreate = PermissionCatalog.SupplierCreate;
    public const string SupplierUpdate = PermissionCatalog.SupplierUpdate;
    public const string SupplierApprove = PermissionCatalog.SupplierApprove;
    public const string StorageRead = PermissionCatalog.StorageRead;
    public const string StorageCreate = PermissionCatalog.StorageCreate;
    public const string StorageUpdate = PermissionCatalog.StorageUpdate;
    public const string StorageDelete = PermissionCatalog.StorageDelete;

    // ---- Observability -----------------------------------------------------
    public const string ObservabilityRead = "Observability.Read";
    public const string ObservabilityManage = "Observability.Manage";
    public const string ObservabilityAdmin = "Observability.Admin";

    public static void AddCompliancePolicies(this AuthorizationOptions options)
    {
        // 1) Strict policy per official permission code (name == code).
        foreach (var permission in PermissionCatalog.All)
        {
            var code = permission.Code;
            options.AddPolicy(code, policy => policy.RequireAssertion(context => HasPermission(context, code)));
        }

        // 2) Read policies accept any higher action within the same module so a
        //    creator/approver can always read, without extra grants.
        AddAny(options, PermissionCatalog.DocumentRead, PermissionCatalog.DocumentRead, PermissionCatalog.DocumentCreate, PermissionCatalog.DocumentUpdate, PermissionCatalog.DocumentApprove);
        AddAny(options, PermissionCatalog.WorkflowRead, PermissionCatalog.WorkflowRead, PermissionCatalog.WorkflowCreate, PermissionCatalog.WorkflowUpdate, PermissionCatalog.WorkflowApprove);
        AddAny(options, PermissionCatalog.TechnicalSheetRead, PermissionCatalog.TechnicalSheetRead, PermissionCatalog.TechnicalSheetCreate, PermissionCatalog.TechnicalSheetUpdate, PermissionCatalog.TechnicalSheetApprove);
        AddAny(options, PermissionCatalog.SupplierRead, PermissionCatalog.SupplierRead, PermissionCatalog.SupplierCreate, PermissionCatalog.SupplierUpdate, PermissionCatalog.SupplierApprove);
        AddAny(options, PermissionCatalog.StorageRead, PermissionCatalog.StorageRead, PermissionCatalog.StorageCreate, PermissionCatalog.StorageUpdate, PermissionCatalog.StorageDelete);

        // 3) Friendly named policies used by the API endpoints.
        AddAny(options, TenantManage,
            PermissionCatalog.TenantRead, PermissionCatalog.TenantUpdate, PermissionCatalog.TenantBranding,
            PermissionCatalog.TenantSecurity, PermissionCatalog.TenantStorage, PermissionCatalog.TenantNotifications,
            PermissionCatalog.TenantIntegrations, PermissionCatalog.TenantBilling, PermissionCatalog.TenantUsers,
            PermissionCatalog.TenantRoles, PermissionCatalog.TenantAudit, PermissionCatalog.TenantDomains,
            PermissionCatalog.TenantSso, PermissionCatalog.TenantWebhooks, PermissionCatalog.TenantApiKeys,
            PermissionCatalog.TenantHealth, PermissionCatalog.TenantBackup);
        AddAny(options, TenantRead, PermissionCatalog.TenantRead, PermissionCatalog.PlatformTenantRead);
        AddAny(options, TenantCreate, PermissionCatalog.PlatformTenantCreate);
        AddAny(options, TenantUpdate, PermissionCatalog.TenantUpdate, PermissionCatalog.PlatformTenantUpdate);
        AddAny(options, TenantStatus, PermissionCatalog.PlatformTenantStatus);
        AddAny(options, TenantBranding, PermissionCatalog.TenantBranding);
        AddAny(options, TenantSecurity, PermissionCatalog.TenantSecurity, PermissionCatalog.PlatformSecurityManage);
        AddAny(options, TenantStorage, PermissionCatalog.TenantStorage);
        AddAny(options, TenantNotifications, PermissionCatalog.TenantNotifications);
        AddAny(options, TenantIntegrations, PermissionCatalog.TenantIntegrations);
        AddAny(options, TenantBilling, PermissionCatalog.TenantBilling, PermissionCatalog.PlatformLicenseManage);
        AddAny(options, TenantUsers, PermissionCatalog.TenantUsers);
        AddAny(options, TenantRoles, PermissionCatalog.TenantRoles);
        AddAny(options, TenantAudit, PermissionCatalog.TenantAudit, PermissionCatalog.AuditRead, PermissionCatalog.PlatformAuditRead);
        AddAny(options, TenantDelete, PermissionCatalog.PlatformTenantDelete);
        AddAny(options, TenantRestore, PermissionCatalog.PlatformTenantRestore);
        AddAny(options, TenantDomains, PermissionCatalog.TenantDomains);
        AddAny(options, TenantSso, PermissionCatalog.TenantSso);
        AddAny(options, TenantWebhooks, PermissionCatalog.TenantWebhooks);
        AddAny(options, TenantApiKeys, PermissionCatalog.TenantApiKeys);
        AddAny(options, TenantHealth, PermissionCatalog.TenantHealth);
        AddAny(options, TenantBackup, PermissionCatalog.TenantBackup);
        AddAny(options, IdentityManage, PermissionCatalog.IdentityManage);
        AddAny(options, RbacManage, PermissionCatalog.RbacManage);
        AddAny(options, AuditRead, PermissionCatalog.AuditRead);

        AddAny(options, AuditManagementRead, PermissionCatalog.AuditManagementRead, PermissionCatalog.AuditManagementManage);
        AddAny(options, AuditManagementManage, PermissionCatalog.AuditManagementManage);
        AddAny(options, CapaManage, PermissionCatalog.CapaManage);
        AddAny(options, CapaRead, PermissionCatalog.CapaRead, PermissionCatalog.CapaManage, PermissionCatalog.CapaApprove);
        AddAny(options, CapaApprove, PermissionCatalog.CapaApprove);
        AddAny(options, CapaClose, PermissionCatalog.CapaClose, PermissionCatalog.CapaApprove);
        AddAny(options, RiskManage, PermissionCatalog.RiskManage);
        AddAny(options, RiskRead, PermissionCatalog.RiskRead, PermissionCatalog.RiskManage, PermissionCatalog.RiskApprove);
        AddAny(options, RiskApprove, PermissionCatalog.RiskApprove);
        AddAny(options, RiskClose, PermissionCatalog.RiskClose, PermissionCatalog.RiskApprove);
        AddAny(options, IndicatorManage, PermissionCatalog.IndicatorManage);
        AddAny(options, IndicatorRead, PermissionCatalog.IndicatorRead, PermissionCatalog.IndicatorManage);
        AddAny(options, IndicatorApprove, PermissionCatalog.IndicatorApprove);
        AddAny(options, IndicatorExport, PermissionCatalog.IndicatorExport, PermissionCatalog.IndicatorManage);
        AddAny(options, ReportManage, PermissionCatalog.ReportManage);
        AddAny(options, ReportRead, PermissionCatalog.ReportRead, PermissionCatalog.ReportManage);
        AddAny(options, ReportExecute, PermissionCatalog.ReportExecute, PermissionCatalog.ReportManage);
        AddAny(options, ReportExport, PermissionCatalog.ReportExport, PermissionCatalog.ReportManage);
        AddAny(options, ReportSchedule, PermissionCatalog.ReportSchedule, PermissionCatalog.ReportManage);
        AddAny(options, NotificationManage, PermissionCatalog.NotificationManage, PermissionCatalog.NotificationAdmin);
        AddAny(options, NotificationSend, PermissionCatalog.NotificationSend, PermissionCatalog.NotificationManage, PermissionCatalog.NotificationAdmin);
        AddAny(options, NotificationRead, PermissionCatalog.NotificationRead, PermissionCatalog.NotificationManage, PermissionCatalog.NotificationAdmin);
        AddAny(options, NotificationTemplate, PermissionCatalog.NotificationTemplate, PermissionCatalog.NotificationManage, PermissionCatalog.NotificationAdmin);
        AddAny(options, NotificationAdmin, PermissionCatalog.NotificationAdmin);

        AddAny(options, ObservabilityRead, PermissionCatalog.ObservabilityRead, PermissionCatalog.ObservabilityManage, PermissionCatalog.ObservabilityAdmin);
        AddAny(options, ObservabilityManage, PermissionCatalog.ObservabilityManage, PermissionCatalog.ObservabilityAdmin);
        AddAny(options, ObservabilityAdmin, PermissionCatalog.ObservabilityAdmin);

        AddAny(options, TemplateRead, PermissionCatalog.TemplateRead, PermissionCatalog.TemplateManage);
        AddAny(options, TemplateManage, PermissionCatalog.TemplateManage);
        AddAny(options, RegulatoryRead,
            PermissionCatalog.RegulatoryProductRead, PermissionCatalog.RegulatoryDossierRead,
            PermissionCatalog.RegulatoryRegistrationRead, PermissionCatalog.RegulatoryManufacturerDocumentRead,
            PermissionCatalog.RegulatoryLicenseRead, PermissionCatalog.RegulatoryReportRead,
            PermissionCatalog.RegulatoryProductManage, PermissionCatalog.RegulatoryDossierCreate,
            PermissionCatalog.RegulatoryDossierUpdate, PermissionCatalog.RegulatoryDossierSubmit,
            PermissionCatalog.RegulatoryDossierApprove, PermissionCatalog.RegulatoryDossierReview,
            PermissionCatalog.RegulatoryDossierApproveForSubmission, PermissionCatalog.RegulatoryConfigure);
        // Registration.Manage must NOT unlock write ops on products/dossiers (Reviewer SoD).
        AddAny(options, RegulatoryManage,
            PermissionCatalog.RegulatoryProductManage, PermissionCatalog.RegulatoryDossierCreate,
            PermissionCatalog.RegulatoryDossierUpdate, PermissionCatalog.RegulatoryRequirementManage,
            PermissionCatalog.RegulatoryObservationManage,
            PermissionCatalog.RegulatoryManufacturerDocumentManage, PermissionCatalog.RegulatoryLicenseManage,
            PermissionCatalog.RegulatoryConfigure);
        // Granular action policies (manual: each button maps to exactly one permission).
        options.AddPolicy(RegulatoryPrepare, policy => policy.RequireAssertion(context =>
            HasPermission(context, PermissionCatalog.RegulatoryProductManage)
            && HasPermission(context, PermissionCatalog.RegulatoryDossierUpdate)));
        AddAny(options, RegulatoryDossierUpdate, PermissionCatalog.RegulatoryDossierUpdate);
        AddAny(options, RegulatoryRequirementUpdate,
            PermissionCatalog.RegulatoryRequirementManage, PermissionCatalog.RegulatoryDossierReview);
        AddAny(options, RegulatoryObservationManage, PermissionCatalog.RegulatoryObservationManage);
        AddAny(options, RegulatoryManufacturerManage, PermissionCatalog.RegulatoryManufacturerDocumentManage);
        AddAny(options, RegulatoryLicenseManage, PermissionCatalog.RegulatoryLicenseManage);
        AddAny(options, RegulatoryDossierCreate, PermissionCatalog.RegulatoryDossierCreate);
        AddAny(options, RegulatoryRegistrationManage, PermissionCatalog.RegulatoryRegistrationManage);
        AddAny(options, RegulatoryEvidenceUpload,
            PermissionCatalog.RegulatoryRequirementManage,
            PermissionCatalog.RegulatoryObservationManage,
            PermissionCatalog.RegulatoryDossierSubmit,
            PermissionCatalog.RegulatoryDossierApprove);
        // Submit must NOT be implied by Approve (SoD — Approver ≠ Submitter).
        AddAny(options, RegulatorySubmit, PermissionCatalog.RegulatoryDossierSubmit);
        AddAny(options, RegulatoryApproveForSubmission, PermissionCatalog.RegulatoryDossierApproveForSubmission);
        AddAny(options, RegulatoryReview, PermissionCatalog.RegulatoryDossierReview);
        AddAny(options, RegulatoryApprove, PermissionCatalog.RegulatoryDossierApprove, PermissionCatalog.RegulatoryRegistrationManage);
        AddAny(options, RegulatoryConfigure, PermissionCatalog.RegulatoryConfigure);
        AddAny(options, RegulatorySoDManage, PermissionCatalog.RegulatorySoDManage, PermissionCatalog.RegulatoryConfigure);
        AddAny(options, RegulatorySoDOverride, PermissionCatalog.RegulatorySoDEmergencyOverride);

        // Platform
        AddAny(options, SuperAdminDashboard, PermissionCatalog.PlatformDashboardRead);
        AddAny(options, SuperAdminTenantsRead, PermissionCatalog.PlatformTenantRead);
        AddAny(options, SuperAdminTenantsCreate, PermissionCatalog.PlatformTenantCreate);
        AddAny(options, SuperAdminTenantsUpdate, PermissionCatalog.PlatformTenantUpdate);
        AddAny(options, SuperAdminTenantsStatus, PermissionCatalog.PlatformTenantStatus);
        AddAny(options, SuperAdminTenantsDelete, PermissionCatalog.PlatformTenantDelete);
        AddAny(options, SuperAdminLicenses, PermissionCatalog.PlatformLicenseManage);
        AddAny(options, SuperAdminModules, PermissionCatalog.PlatformModuleManage);
        AddAny(options, SuperAdminProviders, PermissionCatalog.PlatformProviderManage, PermissionCatalog.PlatformProviderRead);
        AddAny(options, SuperAdminSecurity, PermissionCatalog.PlatformSecurityManage);
        AddAny(options, SuperAdminObservability, PermissionCatalog.PlatformObservabilityRead);
        AddAny(options, SuperAdminAudit, PermissionCatalog.PlatformAuditRead, PermissionCatalog.PlatformAuditExport);
        AddAny(options, SuperAdminDatabase, PermissionCatalog.PlatformDatabaseRead);
        AddAny(options, SuperAdminAi, PermissionCatalog.PlatformAiManage);
        AddAny(options, SuperAdminConfiguration, PermissionCatalog.PlatformConfigurationManage);
        AddAny(options, SuperAdminBackups, PermissionCatalog.PlatformBackupRead);
        AddAny(options, SuperAdminDevOps, PermissionCatalog.PlatformDevOpsRead);
        AddAny(options, SuperAdminSearch, PermissionCatalog.PlatformSearch);
        AddAny(options, PlatformSupportAccess, PermissionCatalog.PlatformSupportAccess);
    }

    private static void AddAny(AuthorizationOptions options, string policyName, params string[] permissionCodes)
    {
        options.AddPolicy(policyName, policy => policy.RequireAssertion(context => HasAnyPermission(context, permissionCodes)));
    }

    private static bool HasPermission(AuthorizationHandlerContext context, string permission)
    {
        return context.User.Claims.Any(claim =>
            string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase)
            && string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasAnyPermission(AuthorizationHandlerContext context, params string[] permissions)
    {
        return permissions.Any(permission => HasPermission(context, permission));
    }
}
