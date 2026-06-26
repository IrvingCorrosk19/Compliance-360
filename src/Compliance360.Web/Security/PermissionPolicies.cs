using Microsoft.AspNetCore.Authorization;

namespace Compliance360.Web.Security;

public static class PermissionPolicies
{
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
    public const string IdentityManage = "Identity.Manage";
    public const string RbacManage = "Rbac.Manage";
    public const string AuditRead = "Audit.Read";
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
    public const string StorageManage = "Storage.Manage";
    public const string NotificationManage = "Notification.Manage";
    public const string NotificationSend = "Notification.Send";
    public const string NotificationRead = "Notification.Read";
    public const string NotificationTemplate = "Notification.Template";
    public const string NotificationAdmin = "Notification.Admin";
    public const string DocumentManage = "Document.Manage";
    public const string WorkflowManage = "Workflow.Manage";
    public const string TechnicalSheetManage = "TechnicalSheet.Manage";
    public const string SupplierManage = "Supplier.Manage";
    public const string ObservabilityRead = "Observability.Read";
    public const string ObservabilityManage = "Observability.Manage";
    public const string ObservabilityAdmin = "Observability.Admin";

    public static void AddCompliancePolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(TenantManage, policy => policy.RequireAssertion(context => HasAnyPermission(context, "TENANT.READ", "TENANT.CREATE", "TENANT.UPDATE", "TENANT.STATUS", "TENANT.BRANDING", "TENANT.SECURITY", "TENANT.STORAGE", "TENANT.NOTIFICATIONS", "TENANT.INTEGRATIONS", "TENANT.BILLING", "TENANT.USERS", "TENANT.ROLES", "TENANT.AUDIT", "TENANT.DELETE", "TENANT.RESTORE", "TENANT.DOMAINS", "TENANT.SSO", "TENANT.WEBHOOKS", "TENANT.API_KEYS", "TENANT.HEALTH", "TENANT.BACKUP")));
        options.AddPolicy(TenantRead, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.READ")));
        options.AddPolicy(TenantCreate, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.CREATE")));
        options.AddPolicy(TenantUpdate, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.UPDATE")));
        options.AddPolicy(TenantStatus, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.STATUS")));
        options.AddPolicy(TenantBranding, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.BRANDING")));
        options.AddPolicy(TenantSecurity, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.SECURITY")));
        options.AddPolicy(TenantStorage, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.STORAGE")));
        options.AddPolicy(TenantNotifications, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.NOTIFICATIONS")));
        options.AddPolicy(TenantIntegrations, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.INTEGRATIONS")));
        options.AddPolicy(TenantBilling, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.BILLING")));
        options.AddPolicy(TenantUsers, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.USERS")));
        options.AddPolicy(TenantRoles, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.ROLES")));
        options.AddPolicy(TenantAudit, policy => policy.RequireAssertion(context => HasAnyPermission(context, "TENANT.AUDIT", "AUDIT.READ", "AUDIT.MANAGE")));
        options.AddPolicy(TenantDelete, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.DELETE")));
        options.AddPolicy(TenantRestore, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.RESTORE")));
        options.AddPolicy(TenantDomains, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.DOMAINS")));
        options.AddPolicy(TenantSso, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.SSO")));
        options.AddPolicy(TenantWebhooks, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.WEBHOOKS")));
        options.AddPolicy(TenantApiKeys, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.API_KEYS")));
        options.AddPolicy(TenantHealth, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.HEALTH")));
        options.AddPolicy(TenantBackup, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.BACKUP")));
        options.AddPolicy(IdentityManage, policy => policy.RequireAssertion(context => HasPermission(context, "IDENTITY.MANAGE")));
        options.AddPolicy(RbacManage, policy => policy.RequireAssertion(context => HasPermission(context, "RBAC.MANAGE")));
        options.AddPolicy(AuditRead, policy => policy.RequireAssertion(context => HasPermission(context, "AUDIT.READ") || HasPermission(context, "AUDIT.MANAGE")));
        options.AddPolicy(AuditManagementManage, policy => policy.RequireAssertion(context => HasPermission(context, "AUDITMANAGEMENT.MANAGE") || HasPermission(context, "AUDIT.MANAGE")));
        options.AddPolicy(CapaManage, policy => policy.RequireAssertion(context => HasPermission(context, "CAPA.MANAGE")));
        options.AddPolicy(CapaRead, policy => policy.RequireAssertion(context => HasPermission(context, "CAPA.READ") || HasPermission(context, "CAPA.MANAGE")));
        options.AddPolicy(CapaApprove, policy => policy.RequireAssertion(context => HasPermission(context, "CAPA.APPROVE") || HasPermission(context, "CAPA.MANAGE")));
        options.AddPolicy(CapaClose, policy => policy.RequireAssertion(context => HasPermission(context, "CAPA.CLOSE") || HasPermission(context, "CAPA.MANAGE")));
        options.AddPolicy(RiskManage, policy => policy.RequireAssertion(context => HasPermission(context, "RISK.MANAGE")));
        options.AddPolicy(RiskRead, policy => policy.RequireAssertion(context => HasPermission(context, "RISK.READ") || HasPermission(context, "RISK.MANAGE")));
        options.AddPolicy(RiskApprove, policy => policy.RequireAssertion(context => HasPermission(context, "RISK.APPROVE") || HasPermission(context, "RISK.MANAGE")));
        options.AddPolicy(RiskClose, policy => policy.RequireAssertion(context => HasPermission(context, "RISK.CLOSE") || HasPermission(context, "RISK.MANAGE")));
        options.AddPolicy(IndicatorManage, policy => policy.RequireAssertion(context => HasPermission(context, "INDICATOR.MANAGE")));
        options.AddPolicy(IndicatorRead, policy => policy.RequireAssertion(context => HasPermission(context, "INDICATOR.READ") || HasPermission(context, "INDICATOR.MANAGE")));
        options.AddPolicy(IndicatorApprove, policy => policy.RequireAssertion(context => HasPermission(context, "INDICATOR.APPROVE") || HasPermission(context, "INDICATOR.MANAGE")));
        options.AddPolicy(IndicatorExport, policy => policy.RequireAssertion(context => HasPermission(context, "INDICATOR.EXPORT") || HasPermission(context, "INDICATOR.MANAGE")));
        options.AddPolicy(ReportManage, policy => policy.RequireAssertion(context => HasPermission(context, "REPORT.MANAGE")));
        options.AddPolicy(ReportRead, policy => policy.RequireAssertion(context => HasPermission(context, "REPORT.READ") || HasPermission(context, "REPORT.MANAGE")));
        options.AddPolicy(ReportExecute, policy => policy.RequireAssertion(context => HasPermission(context, "REPORT.EXECUTE") || HasPermission(context, "REPORT.MANAGE")));
        options.AddPolicy(ReportExport, policy => policy.RequireAssertion(context => HasPermission(context, "REPORT.EXPORT") || HasPermission(context, "REPORT.MANAGE")));
        options.AddPolicy(ReportSchedule, policy => policy.RequireAssertion(context => HasPermission(context, "REPORT.SCHEDULE") || HasPermission(context, "REPORT.MANAGE")));
        options.AddPolicy(StorageManage, policy => policy.RequireAssertion(context => HasPermission(context, "STORAGE.MANAGE")));
        options.AddPolicy(NotificationManage, policy => policy.RequireAssertion(context => HasPermission(context, "NOTIFICATION.MANAGE")));
        options.AddPolicy(NotificationSend, policy => policy.RequireAssertion(context => HasPermission(context, "NOTIFICATION.SEND") || HasPermission(context, "NOTIFICATION.MANAGE") || HasPermission(context, "NOTIFICATION.ADMIN")));
        options.AddPolicy(NotificationRead, policy => policy.RequireAssertion(context => HasPermission(context, "NOTIFICATION.READ") || HasPermission(context, "NOTIFICATION.MANAGE") || HasPermission(context, "NOTIFICATION.ADMIN")));
        options.AddPolicy(NotificationTemplate, policy => policy.RequireAssertion(context => HasPermission(context, "NOTIFICATION.TEMPLATE") || HasPermission(context, "NOTIFICATION.MANAGE") || HasPermission(context, "NOTIFICATION.ADMIN")));
        options.AddPolicy(NotificationAdmin, policy => policy.RequireAssertion(context => HasPermission(context, "NOTIFICATION.ADMIN")));
        options.AddPolicy(DocumentManage, policy => policy.RequireAssertion(context => HasPermission(context, "DOCUMENT.MANAGE")));
        options.AddPolicy(WorkflowManage, policy => policy.RequireAssertion(context => HasPermission(context, "WORKFLOW.MANAGE")));
        options.AddPolicy(TechnicalSheetManage, policy => policy.RequireAssertion(context => HasPermission(context, "TECHNICALSHEET.MANAGE")));
        options.AddPolicy(SupplierManage, policy => policy.RequireAssertion(context => HasPermission(context, "SUPPLIER.MANAGE")));
        options.AddPolicy(ObservabilityRead, policy => policy.RequireAssertion(context => HasPermission(context, "OBSERVABILITY.READ") || HasPermission(context, "OBSERVABILITY.MANAGE") || HasPermission(context, "OBSERVABILITY.ADMIN")));
        options.AddPolicy(ObservabilityManage, policy => policy.RequireAssertion(context => HasPermission(context, "OBSERVABILITY.MANAGE") || HasPermission(context, "OBSERVABILITY.ADMIN")));
        options.AddPolicy(ObservabilityAdmin, policy => policy.RequireAssertion(context => HasPermission(context, "OBSERVABILITY.ADMIN")));
    }

    private static bool HasPermission(AuthorizationHandlerContext context, string permission)
    {
        if (HasPlatformSuperAdmin(context))
        {
            return true;
        }

        return context.User.Claims.Any(claim =>
            string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase)
            && string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasAnyPermission(AuthorizationHandlerContext context, params string[] permissions)
    {
        return permissions.Any(permission => HasPermission(context, permission));
    }

    private static bool HasPlatformSuperAdmin(AuthorizationHandlerContext context)
    {
        return context.User.Claims.Any(claim =>
            string.Equals(claim.Type, System.Security.Claims.ClaimTypes.Role, StringComparison.OrdinalIgnoreCase)
            && string.Equals(claim.Value, "SuperAdmin", StringComparison.OrdinalIgnoreCase));
    }
}
