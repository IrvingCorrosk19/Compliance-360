# 01 — Database Reset Plan

**Environment:** local-lab (`localhost` / `compliance360`)  
**Backup:** PASS (`00_PRE_RESET_BACKUP_EVIDENCE.md`)  
**Script:** `scripts/reset-manual-testing-data.py --confirm-lab-reset`

## Preserve vs delete (summary)

| Category | Action | Reason |
|----------|--------|--------|
| `__EFMigrationsHistory` | PRESERVE | Schema integrity |
| `permissions`, `roles`, `role_permissions` | PRESERVE | RBAC catalog for assign-from-UI |
| `tenants` (2 structural) | PRESERVE rows for platform + Irving | Bootstrap orgs |
| `users` (2) | PRESERVE irving + platform admin | Login bootstrap |
| `user_roles` for preserved users | PRESERVE | TAC / Platform Admin grants |
| `regulatory_sod_settings` | PRESERVE / reseed defaults | SoD baseline GO |
| `tenant_settings` / branding / domains / subscriptions / licenses | PRESERVE | Tenant bootstrap config |
| `storage_provider_configurations`, notification provider/templates | PRESERVE | Technical startup |
| All other `TenantId` operational tables (153−preserve) | DELETE ALL rows | Clean install |
| Cert users `ra.*@cert.local`, `support@…` | DELETE | Lab users |
| Storage folders under `storage/` for tenant GUIDs | DELETE dirs | No orphan blobs |

## Delete order

1. Validate localhost + bootstrap users/tenants/admin roles.  
2. `session_replication_role = replica` (lab FK bypass inside transaction).  
3. Null `CompanyId` on preserved users.  
4. `DELETE` operational tenant tables (reverse alpha, all rows).  
5. Delete identity sidecars + rows for non-preserved users; delete those users.  
6. Delete any tenant outside keep list.  
7. Reseed SoD if missing.  
8. Commit; validate counts; clean storage dirs.

## Pre-reset counts (from probe)

See `00_PRE_RESET_BACKUP_EVIDENCE.md` snapshot.

## Gate

`PRESERVED ADMIN CAN REBUILD THE TENANT FROM UI = YES` (Tenant Administrator + Platform Administrator retained with permission grants intact).
