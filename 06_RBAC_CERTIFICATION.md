# 06 — RBAC Certification — Compliance 360

Date: 2026-07-03 · Method: DB queries + catalog unit tests + live permission probes.

## Catalog

- **89 permissions** in the catalog (`compliance360.permissions`), all following
  `MODULE(.SUBMODULE).ACTION` (READ/CREATE/UPDATE/DELETE/APPROVE/EXPORT, MANAGE only where justified).
- **17 roles** (platform + tenant) defined in code (`RoleCatalog`) and provisioned idempotently.
- **0 orphan permission grants** (`role_permissions` with no matching permission).

## Role permission distribution (aggregated across tenant copies)

Platform Security 6 · Support Operator 4 (break-glass) · Platform Operations 15 · Platform Administrator
21 · CAPA/Risk/Indicators/Storage 119 · Auditor/Document Controller/Notification/Supplier/Tenant
Security 136 · Reporting/Viewer 187 · Tenant Administrator 221 · Quality Manager 323.

## Segregation of Duties (SoD) — DB-verified

| Invariant | Query result |
|---|---|
| No role has both `DOCUMENT.CREATE` and `DOCUMENT.APPROVE` | **0 roles** (PASS) |
| No role has both Storage admin and Notification admin | **0 roles** (PASS) |
| Document Controller creates but cannot approve | verified (unit + live 403) |
| Quality Manager approves but does not create business data | verified |
| CAPA Manager creates CAPA but lacks `CAPA.APPROVE` | verified live (create OK, approve absent) |
| Viewer is read-only | verified (unit tests + live 403 on writes) |

Additional: the RBAC catalog unit suite (`RbacCatalogTests`) encodes these invariants and passes as
part of the **238/238** unit tests.

## Live enforcement probes

- Quality Manager attempting `POST /capas` → **403** (correctly lacks `CAPA.CREATE`).
- CAPA Manager `POST /capas` → **201/200** then full lifecycle transitions succeed.
- Anonymous → 401; cross-tenant → 403 (see §05).

## Verdict

Permissions, policies, claims and JWT are consistent front-to-back; SoD invariants hold; no orphan or
excessive grants detected. **RBAC PASS.**
