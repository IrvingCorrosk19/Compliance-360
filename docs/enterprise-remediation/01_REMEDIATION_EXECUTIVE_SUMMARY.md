# 01 — Remediation Executive Summary

**Phase:** Enterprise Remediation & Recertification  
**Baseline preserved:** `docs/enterprise-functional-certification/` (AS-IS **81/100**)  
**Lab:** `http://localhost:5272` · tenant `82af3877-2786-4d39-bce8-c981101c771d`

## Verdict

| | BEFORE | AFTER |
|--|-------:|------:|
| Enterprise Functional Score | **81** | **95** |
| Level | Enterprise Candidate | **Enterprise Premium** |
| Production | CONDITIONALLY READY | **PRODUCTION READY — residual hardening** |

100/100 was **not** awarded. Remaining residuals are real (runtime i18n depth, responsive depth, dual business-tenant export bleed, cloud email secrets, SpreadsheetML vs OOXML).

## Gates executed

| Gate | Result |
|------|--------|
| Build Release | PASS (0 errors) |
| Unit tests | **317/317 PASS** (was 313) |
| Playwright SoD | **1/1 PASS** |
| AS-IS harness regression | **82/82 PASS**, 0 FAIL, 0 BLOCKED, 0 SKIPPED |
| Remediation harness | **32/32 PASS**, 0 FAIL, 0 BLOCKED, 0 SKIPPED |
| Mailpit SMTP sandbox | PASS (subject received) |
| Report export binary CSV/Excel/PDF | PASS (size > 0, content-type validated) |
| Empty product | HTTP **400** (was 500) |

## Highest-impact closures

1. **Reporting** — TAC `REPORT.*`, export content download, default `page/pageSize`, MIME aligned to SpreadsheetML.
2. **Product validation** — Guards before catalog `Trim`; empty → 400.
3. **Notifications** — Alert Center on `/api/v1` + `/api/v2`; Mailpit sandbox-send verified.
4. **Bootstrap** — Worker stale heartbeat **Degraded**; ready/live filter.
5. **Observations** — `OpenObservationResultDto` + legacy `id`; approve advances when all obs closed.
6. **Alerts** — Occurrence list + acknowledge / resolve / escalate.
7. **Search** — PostgreSQL `ILike` with InMemory fallback.
8. **Post-login** — `resolvePostLoginLanding()` in SPA.

## Evidence roots

- Baseline: `docs/enterprise-functional-certification/evidence/FINAL_CERTIFICATION_SUMMARY.json`
- Remediation: `docs/enterprise-remediation/evidence/`
- AS-IS rerun: `results-EFC-20260726-231320.json` (82/82)
- Remediation run: `results-REM-20260726-230720.json` / later 32/32 runs
