# 05 · ROOT CAUSE ANALYSIS

No new defects were found in this program, so no new RCA is required. This document records the RCA for the carried-over defects (already corrected architecturally) for traceability and to demonstrate the methodology.

## D-01 — Duplicate tax identifier returned 500
- **Root cause:** creation path relied on DB unique constraint without pre-validation; the constraint violation surfaced as an unhandled 500.
- **Architecture:** validation belongs in the application service, not the DB error path.
- **Fix:** `ITenantManagementRepository.TaxIdentifierExistsAsync` + pre-check in `TenantManagementService.CreateTenantAsync` → returns 400 with a clear message.

## D-02 — Platform Admin could not activate tenant
- **Root cause:** lifecycle transitions were only exposed on tenant-scoped routes guarded by "tenant context must match authenticated user", which a platform admin never satisfies.
- **Architecture:** platform-level tenant lifecycle is a platform responsibility, not tenant-scoped.
- **Fix:** dedicated `/superadmin/platform-center/tenants/{id}/{trial|activate|suspend|archive|restore}` endpoints gated by `PLATFORM.TENANT.STATUS`, bypassing tenant-context match by design.

## D-03 — CAPA effectiveness blocked
- **Root cause:** domain supported completing an action, but no API endpoint exposed it, so effectiveness precondition ("≥1 action completed") could never be met via API.
- **Fix:** `CompleteAction` domain method exposed through `CompleteActionAsync` service + `POST .../actions/{actionId}/complete`.

## D-04 — Multipart upload 400 Malformed
- **Root cause:** Minimal API `IFormFile` binding failed for the multipart payload shape used.
- **Fix:** manual `ReadFormAsync` binding with explicit field extraction + default `ContentType`.

## D-05 — Branding theme case-sensitivity
- **Root cause:** payload sent lowercase `light`; API contract requires `System|Light|Dark`. Not a product defect — client payload error; message already descriptive.

**Method note:** every fix targeted the architectural root cause; no patches, hardcode or workarounds were introduced.
