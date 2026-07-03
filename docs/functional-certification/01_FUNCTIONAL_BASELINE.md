# 01 · FUNCTIONAL BASELINE

**Date:** 2026-07-03 · **Commit:** `8d6f964` · **Env:** Development (localhost:5272), PostgreSQL 18 local.

## Baseline gates (evidence)

| Gate | Command | Result |
|---|---|---|
| Health | `GET /health` | **200 Healthy** |
| Readiness | `GET /health/ready` | **200 Healthy** |
| Release build | `dotnet build -c Release` | **0 warnings / 0 errors** |
| Unit tests | `dotnet test -c Release` | **238 / 238 PASS** |
| E2E (headed, real Chrome) | `npx playwright test` | **29 / 29 PASS** |
| Business cycle harness | `scripts/customer_journey.ps1` | **23 / 23 PASS** |

## System inventory

- API surface: **229 endpoints** (176 POST · 38 GET · 10 PUT · 5 DELETE).
- Routes/screens: **20**; operational CRUD modules: **7**; enterprise workspaces: **7**; dashboards: **8**.
- RBAC: 13 tenant roles (ops tenant) + platform roles; permission catalog enforced.
- Ops tenant: `ddcaf211-afe0-44a0-9c90-4fbda8fc4871` with 13 role users.

## Initial DB state

- Repeated journey runs create additional Draft/Active tenants (documented residual risk R3). Core catalogs and ops tenant intact.

**Baseline verdict:** stable and reproducible. Certification execution authorized.
