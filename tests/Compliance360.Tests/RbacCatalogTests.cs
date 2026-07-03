using Compliance360.Domain.Identity;

namespace Compliance360.Tests;

/// <summary>
/// Locks the invariants of the official Enterprise RBAC catalog so future
/// changes cannot silently reintroduce monolithic permissions, break the
/// backend/frontend contract, or violate Segregation of Duties (SoD).
/// </summary>
public sealed class RbacCatalogTests
{
    [Fact]
    public void PermissionCatalog_Has_No_Duplicate_Codes()
    {
        var duplicates = PermissionCatalog.AllCodes
            .GroupBy(code => code, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void PermissionCatalog_Codes_Are_Uppercase_And_Namespaced()
    {
        foreach (var code in PermissionCatalog.AllCodes)
        {
            Assert.Equal(code.ToUpperInvariant(), code);
            Assert.Contains('.', code);
        }
    }

    [Fact]
    public void PermissionCatalog_Removed_Legacy_Monolithic_Permissions()
    {
        // The rebuild split these monolithic permissions; they must not exist.
        string[] legacy =
        [
            "DOCUMENT.MANAGE", "SUPPLIER.MANAGE", "WORKFLOW.MANAGE",
            "TECHNICALSHEET.MANAGE", "STORAGE.MANAGE"
        ];

        foreach (var code in legacy)
        {
            Assert.Null(PermissionCatalog.Find(code));
        }
    }

    [Fact]
    public void PermissionCatalog_Removed_SuperAdmin_Namespaced_Codes()
    {
        Assert.DoesNotContain(PermissionCatalog.AllCodes, code => code.StartsWith("SUPERADMIN.", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RoleCatalog_Every_Role_Grants_Only_Catalog_Permissions()
    {
        var known = new HashSet<string>(PermissionCatalog.AllCodes, StringComparer.OrdinalIgnoreCase);

        foreach (var role in RoleCatalog.All)
        {
            foreach (var code in role.PermissionCodes)
            {
                Assert.True(known.Contains(code), $"Role '{role.Name}' references unknown permission '{code}'.");
            }
        }
    }

    [Fact]
    public void RoleCatalog_Contains_All_Required_Enterprise_Roles()
    {
        string[] required =
        [
            RoleCatalog.PlatformAdministrator, RoleCatalog.PlatformOperations, RoleCatalog.PlatformSecurity,
            RoleCatalog.SupportOperator, RoleCatalog.TenantAdministrator, RoleCatalog.TenantSecurityAdministrator,
            RoleCatalog.DocumentController, RoleCatalog.QualityManager, RoleCatalog.Auditor, RoleCatalog.SupplierManager,
            RoleCatalog.CapaManager, RoleCatalog.RiskManager, RoleCatalog.IndicatorsManager, RoleCatalog.ReportingManager,
            RoleCatalog.StorageAdministrator, RoleCatalog.NotificationAdministrator, RoleCatalog.Viewer
        ];

        foreach (var name in required)
        {
            Assert.NotNull(RoleCatalog.Find(name));
        }
    }

    [Fact]
    public void PlatformAdministrator_Does_Not_Operate_Tenant_Business_Data()
    {
        var platformAdmin = RoleCatalog.Find(RoleCatalog.PlatformAdministrator)!;

        // No tenant business permissions and no implicit break-glass access.
        Assert.DoesNotContain(PermissionCatalog.DocumentRead, platformAdmin.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.DocumentApprove, platformAdmin.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.CapaManage, platformAdmin.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.PlatformSupportAccess, platformAdmin.PermissionCodes);
    }

    [Fact]
    public void SupportOperator_Is_The_Only_BreakGlass_Role()
    {
        var breakGlassRoles = RoleCatalog.All
            .Where(role => role.PermissionCodes.Contains(PermissionCatalog.PlatformSupportAccess))
            .Select(role => role.Name)
            .ToList();

        Assert.Equal([RoleCatalog.SupportOperator], breakGlassRoles);
    }

    [Fact]
    public void SoD_DocumentController_Creates_But_Cannot_Approve()
    {
        var role = RoleCatalog.Find(RoleCatalog.DocumentController)!;

        Assert.Contains(PermissionCatalog.DocumentCreate, role.PermissionCodes);
        Assert.Contains(PermissionCatalog.DocumentUpdate, role.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.DocumentApprove, role.PermissionCodes);
    }

    [Fact]
    public void SoD_QualityManager_Approves_But_Does_Not_Create_Business_Data()
    {
        var role = RoleCatalog.Find(RoleCatalog.QualityManager)!;

        Assert.Contains(PermissionCatalog.DocumentApprove, role.PermissionCodes);
        Assert.Contains(PermissionCatalog.CapaApprove, role.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.DocumentCreate, role.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.CapaManage, role.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.RiskManage, role.PermissionCodes);
    }

    [Fact]
    public void SoD_Auditor_Cannot_Manage_Or_Close_Capas_From_Findings()
    {
        var role = RoleCatalog.Find(RoleCatalog.Auditor)!;

        Assert.Contains(PermissionCatalog.AuditManagementManage, role.PermissionCodes);
        Assert.Contains(PermissionCatalog.CapaRead, role.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.CapaManage, role.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.CapaClose, role.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.CapaApprove, role.PermissionCodes);
    }

    [Fact]
    public void SoD_Storage_And_Notification_Administration_Are_Mutually_Exclusive()
    {
        var storage = RoleCatalog.Find(RoleCatalog.StorageAdministrator)!;
        var notifications = RoleCatalog.Find(RoleCatalog.NotificationAdministrator)!;

        Assert.Contains(PermissionCatalog.StorageCreate, storage.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.NotificationAdmin, storage.PermissionCodes);

        Assert.Contains(PermissionCatalog.NotificationAdmin, notifications.PermissionCodes);
        Assert.DoesNotContain(PermissionCatalog.StorageCreate, notifications.PermissionCodes);
    }

    [Fact]
    public void Viewer_Is_Read_Only()
    {
        var viewer = RoleCatalog.Find(RoleCatalog.Viewer)!;
        string[] writeActions = ["CREATE", "UPDATE", "DELETE", "APPROVE", "MANAGE", "CLOSE", "SEND", "ADMIN"];

        foreach (var code in viewer.PermissionCodes)
        {
            var action = code[(code.LastIndexOf('.') + 1)..];
            Assert.DoesNotContain(action, writeActions);
        }
    }
}
