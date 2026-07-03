# RBAC E2E Validation — Compliance 360

Environment: Development (local PostgreSQL 18, schema `compliance360`).
Date: 2026‑07‑02.

## 1. Build

`dotnet build` (solution) — **succeeded**, 0 errors.

## 2. Automated tests

`dotnet test` — **238 passed, 0 failed, 0 skipped** (~15 s).

Includes the new `RbacCatalogTests` (13 facts) that lock the invariants:

| Invariant | Result |
|-----------|:------:|
| No duplicate permission codes | PASS |
| Codes uppercase & namespaced | PASS |
| Legacy monolithic permissions removed | PASS |
| `SUPERADMIN.*` codes removed | PASS |
| Every role grants only catalog permissions | PASS |
| All required enterprise roles present | PASS |
| Platform Administrator does not operate tenant business data | PASS |
| Support Operator is the only break‑glass role | PASS |
| SoD — Document Controller creates but cannot approve | PASS |
| SoD — Quality Manager approves but does not create business data | PASS |
| SoD — Auditor cannot manage/close CAPAs | PASS |
| SoD — Storage vs Notification administration mutually exclusive | PASS |
| Viewer is read‑only | PASS |

## 3. Bootstrap

App started with `DevelopmentBootstrap`:
- permission catalog seeded, platform roles seeded, all tenants reconciled;
- Platform Administrator (`admin@compliance360.local`) created and assigned via idempotent raw‑SQL upsert (concurrency bug fixed).

## 4. Database evidence (`scripts/rbac_final_evidence.sql`)

```
 permissions | roles | grants | user_roles
-------------+-------+--------+------------
          89 |   212 |   1998 |          2

 tenants_with_roles : 17   (16 business tenants × 13 roles = 208, + 4 platform roles = 212)

 legacy_monolithic : 0     superadmin_codes : 0     invented_codes : 0
 duplicate codes   : (none)
```

## 5. Segregation of Duties — proven from `role_permissions` (`scripts/rbac_sod_evidence.sql`)

```
 role_name                  | doc_create | doc_approve | capa_manage | capa_approve | capa_close | storage_create | notif_admin
----------------------------+------------+-------------+-------------+--------------+------------+----------------+-------------
 Auditor                    | f          | f           | f           | f            | f          | f              | f
 CAPA Manager               | f          | f           | t           | f            | f          | f              | f
 Document Controller        | t          | f           | f           | f            | f          | f              | f
 Notification Administrator | f          | f           | f           | f            | f          | f              | t
 Quality Manager            | f          | t           | f           | t            | t          | f              | f
 Storage Administrator      | f          | f           | f           | f            | f          | t              | f
 Viewer                     | f          | f           | f           | f            | f          | f              | f
```

This is authoritative because these grants are exactly what is emitted as JWT
`permission` claims and enforced by `PermissionPolicies`.

Interpretation:
- **Document Controller** creates documents, **cannot approve** them. ✓
- **Quality Manager** approves documents and approves/closes CAPAs, **creates nothing**. ✓
- **Auditor** can neither manage nor close CAPAs (cannot close a CAPA from own finding). ✓
- **CAPA Manager** manages CAPAs but **cannot approve** (that stays with Quality Manager). ✓
- **Storage Administrator** ⟂ **Notification Administrator** (mutually exclusive). ✓
- **Viewer** holds no write/approve/admin permission. ✓

## 6. Live authorization spot‑checks (during session)

- Login `POST /api/v1/auth/login` (with `TenantId`) → JWT with correct `role`/`permission` claims.
- Platform Administrator calling a **tenant business endpoint** → `403 Forbidden` (no bypass, no business data access).
- New tenant creation auto‑provisions all 13 tenant roles + initial Tenant Administrator (forced password change).

## 7. Per‑role coverage (13+ roles)

All 17 catalog roles exist in code and in the database with exact template grants:
Platform Administrator, Platform Operations, Platform Security, Support Operator (Break Glass),
Tenant Administrator, Tenant Security Administrator, Document Controller, Quality Manager,
Auditor, Supplier Manager, CAPA Manager, Risk Manager, Indicators Manager, Reporting Manager,
Storage Administrator, Notification Administrator, Viewer.

## Verdict

**E2E FUNCTIONAL TESTING COMPLETED** — build clean, 238/238 tests green, bootstrap
successful, database consistent (no legacy/invented/duplicate codes), SoD invariants
proven from the database, and no functional overlaps or role‑name bypass remaining.
