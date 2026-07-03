# 05 — Multi-Tenant Isolation Certification — Compliance 360

Date: 2026-07-03 · Method: active breach attempts with a real tenant user token.

## Breach attempts (live)

Logged in as a **Tenant A** user (`quality@alimentos-premium.test`) and attempted to reach another
tenant's data:

| Attempt | Endpoint | Result |
|---|---|---|
| Read Tenant B documents | `GET /tenants/{B}/documents` | **403 Forbidden** |
| Read Tenant B users | `GET /tenants/{B}/users` | **403 Forbidden** |
| Read own (Tenant A) documents | `GET /tenants/{A}/documents` | **200 OK** |
| Anonymous read Tenant A documents | no token | **401 Unauthorized** |

## Enforcement design

- Every tenant-scoped endpoint is under `/tenants/{tenantId}` and resolves the caller's tenant from
  the JWT; `ApiContext` rejects mismatches. The former SuperAdmin bypass was removed — cross-tenant
  access now requires the auditable `PLATFORM.SUPPORT.ACCESS` break-glass permission.
- Schema level: of 130 tables, **only `permissions` and `tenants` lack a `TenantId`** column (correct —
  they are global/system tables). All business tables are tenant-scoped and indexed by `TenantId`.

## Verdict

No cross-tenant leakage observed across documents and users; enforcement is uniform and identity-driven.
**Tenant Isolation PASS.**
