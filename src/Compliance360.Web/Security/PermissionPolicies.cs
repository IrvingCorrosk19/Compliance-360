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
    public const string StorageManage = "Storage.Manage";
    public const string NotificationManage = "Notification.Manage";
    public const string DocumentManage = "Document.Manage";
    public const string WorkflowManage = "Workflow.Manage";
    public const string TechnicalSheetManage = "TechnicalSheet.Manage";
    public const string SupplierManage = "Supplier.Manage";

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
        options.AddPolicy(StorageManage, policy => policy.RequireAssertion(context => HasPermission(context, "STORAGE.MANAGE")));
        options.AddPolicy(NotificationManage, policy => policy.RequireAssertion(context => HasPermission(context, "NOTIFICATION.MANAGE")));
        options.AddPolicy(DocumentManage, policy => policy.RequireAssertion(context => HasPermission(context, "DOCUMENT.MANAGE")));
        options.AddPolicy(WorkflowManage, policy => policy.RequireAssertion(context => HasPermission(context, "WORKFLOW.MANAGE")));
        options.AddPolicy(TechnicalSheetManage, policy => policy.RequireAssertion(context => HasPermission(context, "TECHNICALSHEET.MANAGE")));
        options.AddPolicy(SupplierManage, policy => policy.RequireAssertion(context => HasPermission(context, "SUPPLIER.MANAGE")));
    }

    private static bool HasPermission(AuthorizationHandlerContext context, string permission)
    {
        return context.User.Claims.Any(claim =>
            string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase)
            && string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));
    }
}
