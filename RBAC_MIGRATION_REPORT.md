# RBAC Migration Report — Compliance 360 (Development)

## From → To

| Aspect | Before | After |
|--------|--------|-------|
| Permissions | Mixed granular + monolithic `*.MANAGE`, invented E2E codes | 89 uniform granular codes, code‑first catalog |
| Roles | Ad‑hoc strings, `SuperAdmin` | 17 code‑first roles (4 platform + 13 tenant) |
| Platform access | `SuperAdmin` bypass (unconditional) | Claim‑based; Platform Administrator (no bypass) + audited break‑glass |
| Cross‑tenant | `HasSuperAdminRole` magic role | Explicit `PLATFORM.SUPPORT.ACCESS` (Support Operator only) |
| Tenant setup | Manual | Auto‑provisioned RBAC + initial Tenant Administrator |
| Enforcement | Partial, role‑name checks | Strict per‑permission policies + frontend parity |

## Migration steps executed

1. **Backup assumption:** Development only; production untouched.
2. **Wipe:** `scripts/rbac_reset.sql` cleared `role_permissions`, `user_roles`, `roles`, `permissions`.
3. **Reseed:** app startup (`DevelopmentBootstrap`) invoked `EfRbacProvisioningService` to:
   - seed the permission catalog (89 codes),
   - seed 4 platform roles in the platform tenant,
   - seed 13 tenant roles in each of the 16 business tenants (with stale‑grant pruning),
   - upsert the Platform Administrator user + assignment.
4. **Reconcile existing tenants:** every pre‑existing tenant had its role grants reset to the catalog template (removing legacy/temporary grants).
5. **Verify:** `scripts/rbac_final_evidence.sql` and `scripts/rbac_sod_evidence.sql`.

## Post‑migration state (measured)

```
permissions = 89   roles = 212   grants = 1998   user_roles = 2
tenants_with_roles = 17 (16 business + platform)
legacy_monolithic = 0   superadmin_codes = 0   invented_codes = 0   duplicates = none
```

## Data preserved

- Platform administrator identity (migrated to **Platform Administrator**).
- All tenants and their business data (only RBAC grants were reconciled).

## Data removed

- `SUPERADMIN.*` permissions and grants.
- Monolithic `*.MANAGE` CRUD permissions and grants.
- Invented/temporary E2E permission codes and grants.
- Legacy `SuperAdmin` implicit‑access grants.

## Rollback

Re‑running `scripts/rbac_reset.sql` + app startup deterministically rebuilds the exact
catalog state; provisioning is idempotent and self‑healing.

## Risk assessment

- **Low** for Development. Idempotent seeding, automated invariants guard against regression.
- For a future production migration: run reset in a maintenance window, back up `roles`/`role_permissions`/`user_roles` first, then reseed and re‑verify with the evidence scripts.
