# RBAC Final Architecture — Compliance 360

## Layers

```
Domain            PermissionCatalog + RoleCatalog        (single source of truth)
  │               Permission.Define(code, action, desc)
  ▼
Application       IRbacProvisioningService                (seed contract)
  │               TenantManagementService                 (auto-provision on create)
  ▼
Infrastructure    EfRbacProvisioningService               (idempotent seed + prune)
  │               EfIdentityRepository.GetPermissionCodesAsync
  ▼
Web               PermissionPolicies (catalog-driven)      (authorization)
                  SecurityServices.CreateAccessToken       (JWT permission claims)
                  ApiContext (break-glass via PLATFORM.SUPPORT.ACCESS)
                  FoundationEndpoints (per-endpoint granular policies)
  ▼
Frontend          app.js routePermissions / routeManagePermissions / canManageRoute
```

## End-to-end flow

1. **Definition.** Every permission and role lives in code (`RbacCatalog.cs`, `RoleCatalog.cs`).
2. **Seeding.** `EfRbacProvisioningService`:
   - `EnsurePermissionCatalogAsync()` upserts all permission codes (dedup by code).
   - `EnsurePlatformRolesAsync(platformTenantId)` seeds platform roles.
   - `EnsureTenantRolesAsync(tenantId)` seeds tenant roles and **prunes stale grants** so a role always matches its template (no orphan permissions).
3. **Bootstrap (Development).** `DevelopmentBootstrap` calls the provisioning service, ensures the **Platform Administrator** role/user, and reconciles every existing tenant.
4. **Tenant creation.** `TenantManagementService.CreateTenantAsync` auto-provisions the 13 tenant roles and, when admin credentials are supplied, an initial **Tenant Administrator** (with forced password change).
5. **Authentication.** On login, `GetPermissionCodesAsync` resolves `UserRoles → RolePermissions → Permission.Code`; `CreateAccessToken` emits one `permission` claim per code and one `role` claim per role name.
6. **Authorization.** `PermissionPolicies.AddCompliancePolicies` registers one strict policy per catalog code plus friendly named policies used by endpoints. **There is no role-name bypass.**
7. **Tenant isolation.** `ApiContext` only allows cross-tenant access when the caller holds `PLATFORM.SUPPORT.ACCESS` (auditable break-glass) — never via a magic role.
8. **Frontend.** `app.js` gates menus/routes/actions with the exact same catalog codes; read‑only routes render a read‑only experience, write affordances require the module's CREATE/UPDATE/MANAGE permission.

## Key design decisions

- **No SuperAdmin bypass.** Removed `HasPlatformSuperAdmin` (policies) and `HasSuperAdminRole` (ApiContext). Authorization is always claim‑based.
- **Platform vs. Tenant separation.** Platform Administrator manages the platform and is **denied** tenant business endpoints (verified: `403` on tenant documents).
- **Break‑glass.** Support access is an explicit, single, audited permission held only by the Support Operator role.
- **Superset reads.** Higher actions imply read within the same module; approval is never implied.
- **Idempotent + self‑healing.** Provisioning can run repeatedly; it adds missing grants and prunes legacy ones.

## Contract guarantee

`No permission exists in the frontend that does not exist in the backend, and vice‑versa.`
The frontend references only `PermissionCatalog` codes; `RbacCatalogTests` fails the
build if a role references an unknown code or if a removed legacy code reappears.
