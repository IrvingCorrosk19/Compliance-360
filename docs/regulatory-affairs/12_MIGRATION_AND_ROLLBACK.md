# 12 — Migration and Rollback

Forward: `dotnet ef database update` (AddRegulatoryAffairs).  
Rollback: `dotnet ef migrations remove` solo si no aplicada; si aplicada, migration Down correspondiente.  
RBAC: `EnsurePermissionCatalogAsync` + `EnsureTenantRolesAsync` sincronizan grants al bootstrap.
