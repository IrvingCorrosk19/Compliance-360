# 05 — Test Environment and Lab

**Lab tenant:** `82af3877-2786-4d39-bce8-c981101c771d`  
**Base URL:** `http://localhost:5272`  
**Date:** 2026-07-14  
**Status:** Lab configured for SoD; functional cert NOT EXECUTED

---

## 1. Purpose

Document the **fixed** test environment for REGUTRACK functional certification. All test cases in documents 01, 09, and 10 assume this configuration unless explicitly marked otherwise.

---

## 2. Environment topology

```
┌─────────────────────────────────────────────────────────┐
│  Browser (Chrome headful recommended)                    │
│  http://localhost:5272                                   │
└──────────────────────────┬──────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│  Compliance360.Web (ASP.NET)                             │
│  wwwroot/regulatory-affairs.js                           │
│  /api/v1/tenants/{tenantId}/regulatory/*                 │
└──────────────────────────┬──────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│  PostgreSQL (local)                                      │
│  Tenant-scoped RA schema                                 │
└─────────────────────────────────────────────────────────┘
```

---

## 3. Tenant configuration

| Setting | Value |
|---------|-------|
| Tenant ID | `82af3877-2786-4d39-bce8-c981101c771d` |
| Display name | Lab Irving (cert) |
| Module | Regulatory Affairs enabled |
| SoD policy | Default regulated (see `/sod-settings`) |
| Bootstrap | Required once per fresh DB |

### 3.1 Bootstrap procedure

1. Login as `ra.admin@cert.local`
2. Navigate to RA Console → **Configuración**
3. Click **Bootstrap regulatorio** OR `POST /api/v1/tenants/{tenantId}/regulatory/bootstrap`
4. Verify response includes:
   - Authorities: MINSA, CSS
   - Pack: `REGUTRACK-PA-DEFAULT` with 22 requirement definitions

**Test ID:** ENV-001  
**Status:** NOT EXECUTED

---

## 4. Application startup

### 4.1 Build and run

```powershell
cd "c:\Proyectos\Compliance 360"
dotnet build -c Release
dotnet run --project src/Compliance360.Web --urls "http://localhost:5272"
```

### 4.2 Health check

| Check | Command / URL | Expected |
|-------|---------------|----------|
| Web up | `GET http://localhost:5272` | 200 |
| API auth | Login + `GET .../regulatory/dashboard` | 200 with JWT |
| DB | Bootstrap succeeds | 200 + pack JSON |

**Test ID:** ENV-002  
**Status:** NOT EXECUTED

---

## 5. Authentication

| Parameter | Rule |
|-----------|------|
| Token storage | `localStorage.c360.token` |
| Tenant in token | `tenant_id` or `tid` claim |
| Logout | Clear localStorage + sessionStorage |
| Passwords | Managed offline (not in repo) |
| Fresh login | Required per role switch |

### 5.1 Login flow (manual)

1. Open application login page
2. Enter `@cert.local` user from matrix 07
3. Confirm tenant context matches lab UUID
4. Open Regulatory Affairs module
5. Verify `renderRegulatoryAffairs` loads nav buttons

**Test ID:** ENV-003 (per user)  
**Status:** NOT EXECUTED

---

## 6. API conventions

| Item | Value |
|------|-------|
| Base path | `/api/v1/tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory` |
| Auth header | `Authorization: Bearer {token}` |
| Content-Type | `application/json` (except multipart import) |
| Enum format | String or numeric accepted |

Example:

```http
GET /api/v1/tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers
Authorization: Bearer eyJ...
```

---

## 7. Browser requirements

| Setting | Recommendation |
|---------|----------------|
| Browser | Chrome (latest) |
| Playwright | `headless: false`, `channel: chrome` |
| Viewport | 1920×1080 minimum for kanban |
| Locale | es-PA or es (UI labels Spanish) |

---

## 8. Supporting files on disk

| Asset | Path |
|-------|------|
| REGUTRACK contract | Project copy of `REGUTRACK 02JUN26 MG.xlsx` |
| Decomposition | `docs/final-functional-certification/evidence/regutrack_decomposition.json` |
| SoD E2E script | `scripts/run-sod-api-e2e.ps1` (reference) |
| Evidence output | `docs/final-functional-certification/evidence/functional/` (create on run) |

---

## 9. Environment isolation

- **Do not** run destructive import commit against shared non-lab tenants.
- **Do not** use production Excel with PII in unsecured folders; store under `evidence/` with access control.
- Functional tests use prefix `CERT-` for synthetic case numbers to distinguish from import rows.

---

## 10. Reset and refresh

| Procedure | When | Steps |
|-----------|------|-------|
| Soft reset | Daily | Delete `CERT-*` dossiers via API; keep bootstrap |
| Import reset | After import test | Document job id; optional DB restore from snapshot |
| Full reset | Major schema change | Restore DB backup; re-run migrations; bootstrap; re-seed users |

Snapshot recommendation: pg_dump before SC-WF-004 import commit.

---

## 11. Monitoring during tests

| Signal | Tool |
|--------|------|
| API errors | Browser devtools Network tab |
| Server exceptions | Web console / logs |
| DB state | SQL client read-only queries |
| Audit trail | Compliance360 audit API / tables |

---

## 12. Environment verification checklist

| ID | Check | Status |
|----|-------|--------|
| ENV-001 | Bootstrap OK | NOT EXECUTED |
| ENV-002 | Build + health | NOT EXECUTED |
| ENV-003 | All 9 users login | NOT EXECUTED |
| ENV-004 | RA nav visible per role | NOT EXECUTED |
| ENV-005 | Excel file accessible | NOT EXECUTED |
| ENV-006 | Evidence folder writable | NOT EXECUTED |

---

## 13. Document control

| Version | Date | Change |
|---------|------|--------|
| 1.0 | 2026-07-14 | Initial lab baseline |
