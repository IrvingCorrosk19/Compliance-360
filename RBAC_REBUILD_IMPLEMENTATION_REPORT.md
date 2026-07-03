# RBAC Rebuild — Implementation Report

## Scope

Full enterprise RBAC rebuild of Compliance 360 in Development, per the approved audit
(`01_ROLE_DISCOVERY.md` … `09_EXECUTIVE_SUMMARY.md`). Frontend, backend and database were
authorized to be refactored, and were.

## What was implemented

### 1. Official permission catalog
- New `src/Compliance360.Domain/Identity/RbacCatalog.cs` (`PermissionCatalog`) — 89 codes.
- Uniform `MODULE(.SUBMODULE).ACTION` pattern (`READ/CREATE/UPDATE/DELETE/APPROVE/EXPORT`, coarse verbs only where a real single capability).
- Monolithic `DOCUMENT.MANAGE`, `SUPPLIER.MANAGE`, `WORKFLOW.MANAGE`, `TECHNICALSHEET.MANAGE`, `STORAGE.MANAGE`, `AUDIT.MANAGE` split into granular actions.
- `Permission.Define(code, action, description)` factory added so codes are explicit (no auto‑derivation collisions).

### 2. Official role catalog
- New `src/Compliance360.Domain/Identity/RoleCatalog.cs` — 17 code‑first roles (4 platform + 13 tenant), each with an explicit permission template. See `RBAC_ROLE_CATALOG.md`.

### 3. SuperAdmin bypass eliminated
- `PermissionPolicies.cs`: removed `HasPlatformSuperAdmin`; policies are now generated strictly from `PermissionCatalog.All`, with read‑superset semantics (higher action implies read, approval never implied).
- `ApiContext.cs`: removed `HasSuperAdminRole`; cross‑tenant access requires the explicit, audited `PLATFORM.SUPPORT.ACCESS` permission (break‑glass), held only by Support Operator.
- Platform Administrator manages the platform and is denied tenant business endpoints.

### 4. Segregation of Duties
- Quality Manager demoted from operational super‑user to approver/coordinator (`*.APPROVE`/`*.CLOSE`/`READ` only).
- Document creation vs approval separated; Auditor cannot manage/close CAPAs; Storage vs Notification administration mutually exclusive. Proven from DB in `RBAC_E2E_VALIDATION.md`.

### 5. Frontend (`wwwroot/app.js`)
- `routePermissions` / `routeManagePermissions` rewritten to granular catalog codes.
- `canManageRoute` uses `hasAnyPermission` to support multiple manage permissions per route.
- Dashboards/quick‑actions gated with granular `hasAnyPermission`.
- Removed references to non‑existent/legacy permissions; cosmetic `SUPERADMIN.TENANTS.CREATE` → `PLATFORM.TENANT.CREATE`.

### 6. Backend synchronization
- `FoundationEndpoints.cs`: per‑endpoint granular policies (e.g. `WorkflowManage` → `WorkflowUpdate`).
- `SecurityServices` JWT emits one `permission` claim per resolved code and one `role` claim per role.
- `EfIdentityRepository.GetPermissionCodesAsync` resolves `UserRoles → RolePermissions → Permission.Code`.

### 7. Tenant bootstrap
- `IRbacProvisioningService` + `EfRbacProvisioningService`: idempotent `EnsurePermissionCatalogAsync`, `EnsurePlatformRolesAsync`, `EnsureTenantRolesAsync` (with **stale‑grant pruning**).
- `TenantManagementService.CreateTenantAsync` auto‑provisions the 13 tenant roles and an initial Tenant Administrator (forced password change) — no manual intervention.
- `DevelopmentBootstrap` reconciles all existing tenants on startup.

### 8. Database rebuild
- `scripts/rbac_reset.sql` wipes RBAC; app reseeds from the catalog. See `RBAC_DATABASE_CHANGES.md`.
- Concurrency bug in `EnsureSuperAdminAsync` fixed with idempotent raw‑SQL `user_roles` upsert.

### 9. Tests & docs
- New `tests/Compliance360.Tests/RbacCatalogTests.cs` (13 invariant/SoD facts).
- Deliverable docs generated (this file + 7 others).

## Files changed / added (highlights)

| File | Change |
|------|--------|
| `Domain/Identity/RbacCatalog.cs` | **new** — permission catalog |
| `Domain/Identity/RoleCatalog.cs` | **new** — role catalog |
| `Domain/Identity/Permission.cs` | `Define` factory |
| `Web/Security/PermissionPolicies.cs` | catalog‑driven, bypass removed |
| `Web/Api/ApiContext.cs` | break‑glass permission, no role bypass |
| `Web/Api/FoundationEndpoints.cs` | granular per‑endpoint policies |
| `Application/Rbac/RbacContracts.cs` | `IRbacProvisioningService` |
| `Infrastructure/Rbac/EfRbacProvisioningService.cs` | **new** — idempotent seed + prune |
| `Application/TenantManagement/TenantManagementService.cs` | auto‑provision + initial admin |
| `Web/Development/DevelopmentBootstrap.cs` | provisioning + concurrency fix |
| `wwwroot/app.js` | granular UI guards |
| `tests/.../RbacCatalogTests.cs` | **new** — invariants/SoD |
| `scripts/rbac_*.sql` | reset/inspect/evidence |

## Result

Build clean, 238/238 tests green, DB consistent, SoD proven. See `RBAC_E2E_VALIDATION.md`.
