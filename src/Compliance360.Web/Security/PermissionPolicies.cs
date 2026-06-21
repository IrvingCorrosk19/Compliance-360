using Microsoft.AspNetCore.Authorization;

namespace Compliance360.Web.Security;

public static class PermissionPolicies
{
    public const string TenantManage = "Tenant.Manage";
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
        options.AddPolicy(TenantManage, policy => policy.RequireAssertion(context => HasPermission(context, "TENANT.MANAGE")));
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
        return context.User.Claims.Any(claim =>
            string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase)
            && string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));
    }
}
