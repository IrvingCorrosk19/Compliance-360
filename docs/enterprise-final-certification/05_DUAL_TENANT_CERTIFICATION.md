# REM-MT-DUAL-BUSINESS — CLOSED — VERIFIED

## Root cause
Prior residual used platform tenant as foreign proxy; lacked two populated business tenants with search/export mutual isolation.

## Fix / test
Harness creates TENANT_ALPHA / TENANT_BETA with independent products + users; adversarial GET/search/path + report export no-leak; unit `DualTenantIsolationTests`.

## Evidence
Final harness PASS: MT-ISO-* search 0 hits, GET foreign product/dossier **404**, foreign path **403**, Excel/CSV exports contain no peer tenant marker.
Also: ApiResult maps "not found" to **404** (was 400).

## Final status
**CLOSED — VERIFIED** — Multitenancy **100/100**
