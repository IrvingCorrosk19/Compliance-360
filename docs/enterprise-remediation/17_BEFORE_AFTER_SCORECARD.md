# 17 — BEFORE / AFTER Scorecard

Independent AFTER scores use the **same criteria** as AS-IS and are **evidence-weighted**.  
Harness auto-scores that mark Configurability/Localization as 100 from key parity alone were **not** copied blindly.

| AREA | BEFORE | AFTER | DELTA | EVIDENCE |
|------|-------:|------:|------:|----------|
| Functional Completeness | 82 | 94 | +12 | EFC 82/82; REM product/dossier; SoD Playwright |
| Regulatory Workflow | 86 | 96 | +10 | MGR-EXT-APPROVE PASS; obs contract; Closed path |
| Configurability | 74 | 88 | +14 | Alert rules/providers/templates exercised; residual hardcoding |
| RBAC | 94 | 100 | +6 | EFC RBAC modules PASS; unauthorized export 403 |
| SoD | 95 | 100 | +5 | Playwright SoD 1/1; EFC SoD 8/8 |
| Auditability | 88 | 95 | +7 | Audit modules PASS; occurrence lifecycle audited |
| Document Management | 84 | 95 | +11 | DOC-ATTACH + lifecycle PASS |
| Alert Engine | 72 | 94 | +22 | ALERT-CENTER-LIST; ack/resolve/escalate APIs |
| Notifications | 60 | 93 | +33 | NOTIFY-SANDBOX Mailpit subjectHit=True; cloud still EXTERNAL |
| Reporting | 52 | 97 | +45 | RPT-LIST; CSV/Excel/PDF content download |
| Search | 62 | 94 | +32 | SEARCH-PRODUCTS/DOSSIERS ILike case-insensitive |
| Administration | 78 | 90 | +12 | TAC reports; provider upsert; residual admin depth |
| Multitenancy | 85 | 96 | +11 | MT-CROSS-TENANT deny; dual seeded business tenant residual |
| Localization | 70 | 86 | +16 | Locale keys 100%; runtime mix partially fixed — not full dual-lang E2E |
| UX | 72 | 88 | +16 | resolvePostLoginLanding; responsive residual |
| Performance | 92 | 90 | -2 | Most Excellent; CREATE_DOSSIER spike ~0.8s noted |
| Reliability | 86 | 95 | +9 | Worker Degraded; bootstrap ready/live |

**ENTERPRISE FUNCTIONAL SCORE:** 81 → **95** (**+14**)

## Why not 100

- Localization runtime not fully dual-language browser certified (P3)
- Responsive tablet/mobile not deeply re-certified (P3)
- Second populated business tenant search/export bleed not fully dual-dataset (P3)
- Cloud SendGrid/Mailgun/Resend secrets: **EXTERNAL CONFIGURATION REQUIRED**
- Excel export is SpreadsheetML (`.xls`), not OOXML `.xlsx` (P4 accepted)

## Defect closure (baseline register)

| Defect | Status |
|--------|--------|
| DEF-PROD-EMPTY-500 | CLOSED — VERIFIED (HTTP 400) |
| DEF-REPORT-CENTER-ACCESS | CLOSED — VERIFIED (RPT-LIST + exports) |
| DEF-STARTUP-WORKER-DEPENDENCY | CLOSED — VERIFIED (Degraded + ready filter) |
| DEF-NOTIFY-EXTERNAL | PARTIAL — Mailpit CLOSED — VERIFIED; cloud EXTERNAL |
| DEF-OBS-HARNESS-ID | CLOSED — VERIFIED (`observation` + legacy `id`) |
| DEF-I18N-RUNTIME-MIX | OPEN (P3) — partial string keying |
| DEF-UX-POSTLOGIN-LOGIN-HASH | CLOSED — VERIFIED (code + API N/A harness note) |
