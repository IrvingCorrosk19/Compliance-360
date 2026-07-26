# 02 — Baseline Gap Matrix

Source baseline: `docs/enterprise-functional-certification/evidence/FINAL_CERTIFICATION_SUMMARY.json`  
Generated at: `2026-07-26T22:40:00Z`  
Lab: `http://localhost:5272` / tenant `82af3877-2786-4d39-bce8-c981101c771d`

## AS-IS test summary

| Metric | Value |
|--------|-------|
| Planned | 88 |
| Executed | 88 |
| PASS | 84 |
| FAIL | 1 (`PROD-NEG-EMPTY` → HTTP 500) |
| BLOCKED | 1 (Report Center TAC access) |
| SKIPPED | 0 |
| Coverage | ≈85% RA critical surface |
| Enterprise functional score | **81** |
| Level | Enterprise Candidate |
| Production verdict | CONDITIONALLY READY |

## Open severity counts

| Severity | Open |
|----------|------|
| P0 | 0 |
| P1 | 0 |
| P2 | 4 |
| P3 | 2 |
| P4 | 1 |

## Scorecard (BEFORE)

| Area | Score |
|------|------:|
| Functional Completeness | 82 |
| Regulatory Workflow | 86 |
| Configurability | 74 |
| RBAC | 94 |
| SoD | 95 |
| Auditability | 88 |
| Document Management | 84 |
| Alert Engine | 72 |
| Notifications | 60 |
| Reporting | 52 |
| Search | 62 |
| Administration | 78 |
| Multitenancy | 85 |
| Localization | 70 |
| UX | 72 |
| Performance | 92 |
| Reliability | 86 |

## Top 10 gaps (baseline)

| # | Gap | Impact area | Target remediation |
|---|-----|-------------|--------------------|
| 1 | Report Center / exports not E2E certified (403/404) | Reporting (52) | RoleCatalog + correct `/api/v1/tenants/{id}/reports` path; full export chain |
| 2 | Notifications: SMTP/Mailpit and cloud provider secrets not exercised (FUNCTIONALLY READY — EXTERNAL CONFIGURATION REQUIRED) | Notifications (60) | Mailpit sandbox-send or record EXTERNAL BLOCKED |
| 3 | Empty product create returns HTTP 500 instead of 4xx | Products / Negative | Guard null Trim before validation |
| 4 | Web Development bootstrap hard-fails without healthy Alert Center Worker | Reliability / DevEx | Stale heartbeat → Degraded; ready/live filters |
| 5 | Runtime ES/EN localization mix despite 100% locale key parity | Localization (70) | Locale consistency pass |
| 6 | Search limited to list/filter patterns; full-text enterprise search not proven | Search (62) | Case-insensitive `searchText` on products/dossiers |
| 7 | Second business tenant deep isolation (search/export bleed) not fully exercised | Multitenancy (85) | Cross-tenant deny (403/404) |
| 8 | Observation client contract ambiguous (create response shape) | Workflow | `OpenObservationResultDto` (observation.id ≠ dossier.id) |
| 9 | Post-login landing can remain on `#/login` placeholders before `#/regulatory` | UX (72) | `resolvePostLoginLanding` (API N/A; browser separately) |
| 10 | Responsive tablet/mobile operations not deeply certified | UX | Responsive remediation notes |

## Remaining FAIL / BLOCKED (baseline harness notes)

- **FAIL:** `PROD-NEG-EMPTY` (HTTP 500 on empty product create)
- **BLOCKED:** Report Center TAC access (missing/incorrect REPORT.* / wrong URL)

## Regression baseline (already green)

- `dotnet` Release build: PASS (0 errors, 0 warnings)
- Unit tests: PASS 313/313
- Playwright SoD: PASS 1/1

## After matrix

| Gap | BEFORE | AFTER | Evidence |
|-----|--------|-------|----------|
| Report Center / exports | BLOCKED / 52 | **PASS / 97** | RPT-LIST; RPT-CONTENT-CSV/EXCEL/PDF |
| Empty product 500 | FAIL | **PASS (400)** | PROD-NEG-EMPTY |
| Notifications external | EXTERNAL | **Mailpit PASS**; cloud EXTERNAL | NOTIFY-SANDBOX |
| Observation contract | Ambiguous | **PASS** | OBS-OPEN-SHAPE |
| Search case-insensitive | Not proven | **PASS** | SEARCH-* |
| Cross-tenant deny | Partial | **PASS** | MT-CROSS-TENANT |
| Post-login landing | UX gap | **Code fixed** | resolvePostLoginLanding |
| Worker bootstrap | Unhealthy block | **Degraded / ready filter** | Bootstrap |
| Alert occurrence list | 404 | **PASS** | ALERT-CENTER-LIST |
