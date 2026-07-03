# RBAC Database Changes — Compliance 360 (Development)

Connection: local PostgreSQL 18, schema `compliance360`. **Production untouched.**

## Tables involved

- `compliance360.permissions` — canonical permission codes
- `compliance360.roles` — platform & tenant roles (`TenantId` scopes tenant roles)
- `compliance360.role_permissions` — grants
- `compliance360.user_roles` — assignments

## Reset & rebuild procedure

1. **Wipe** legacy RBAC data — `scripts/rbac_reset.sql`:

```sql
begin;
delete from compliance360.role_permissions;
delete from compliance360.user_roles;
delete from compliance360.roles;
delete from compliance360.permissions;
commit;
```

2. **Rebuild** — run the app once; `DevelopmentBootstrap` + `EfRbacProvisioningService` reseed:
   - all `PermissionCatalog` codes into `permissions`;
   - platform roles into the platform tenant;
   - the 13 tenant roles into every existing tenant (with stale-grant pruning);
   - the **Platform Administrator** user + assignment (idempotent raw‑SQL upsert into `user_roles`).

## Concurrency fix (bootstrap)

`EnsureSuperAdminAsync` previously threw `DbUpdateConcurrencyException` (expected 1 row,
affected 0) because EF change‑tracking issued a phantom UPDATE on `user_roles` inside a
large shared graph. Fixed by:
- saving the user (password/status) first, then
- performing an **idempotent `INSERT ... WHERE NOT EXISTS`** into `user_roles` via
  `Database.ExecuteSqlInterpolatedAsync`, bypassing change tracking.

## What was removed from the database

- All `SUPERADMIN.*` permissions and any role granting them.
- Monolithic `*.MANAGE` permissions for CRUD modules and their grants.
- Invented/E2E permission codes and temporary grants.
- Legacy `SuperAdmin` role grants that provided implicit tenant access.

## What was preserved

- The development platform administrator identity (migrated to **Platform Administrator**).
- Existing tenants and their business data (only their **role grants** were reconciled to the catalog).

## Verification scripts (in `scripts/`)

| Script | Purpose |
|--------|---------|
| `rbac_inspect.sql` | List permissions/roles/grants |
| `rbac_admin_check.sql` | Platform admin user + assignment |
| `rbac_newtenant_check.sql` | Roles/grants of a freshly created tenant |
| `rbac_admin2_check.sql` | Initial tenant admin after creation |
| `rbac_sod_matrix.sql` | Effective SoD matrix per role, straight from `role_permissions` |

### Post‑rebuild counts (Development)

- `permissions`: full catalog (no duplicates, no legacy codes).
- Platform tenant: 4 platform roles.
- Each business tenant: 13 tenant roles with exact template grants.
- `user_roles`: Platform Administrator assigned; per‑tenant initial admins where provisioned.
