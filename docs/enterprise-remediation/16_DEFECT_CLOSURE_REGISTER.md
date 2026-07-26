# 16 — Defect Closure Register

Baseline source: `docs/enterprise-functional-certification/16_DEFECT_REGISTER.md` (immutable).

| Defect | Baseline | AFTER | Fix | Test | Evidence |
|--------|----------|-------|-----|------|----------|
| DEF-PROD-EMPTY-500 | OPEN P2 | **CLOSED — VERIFIED** | Guards before catalog Trim; null-safe repo | PROD-NEG-EMPTY; RegulatoryProductValidationTests | HTTP 400 |
| DEF-REPORT-CENTER-ACCESS | OPEN P2 | **CLOSED — VERIFIED** | TAC REPORT.*; content download; page defaults; v1 list | RPT-LIST; RPT-CONTENT-* | EFC+REM |
| DEF-STARTUP-WORKER-DEPENDENCY | OPEN P2 | **CLOSED — VERIFIED** | Worker Degraded; bootstrap ready/live | Lab start without Unhealthy block | Bootstrap logs |
| DEF-NOTIFY-EXTERNAL | OPEN P2 | **PARTIAL** | Mailpit sandbox PASS; cloud still EXTERNAL | NOTIFY-SANDBOX | Mailpit subjectHit |
| DEF-OBS-HARNESS-ID | OPEN P3 | **CLOSED — VERIFIED** | OpenObservationResultDto + legacy Id | OBS-OPEN-SHAPE; OBS-CREATE | observation≠dossier |
| DEF-I18N-RUNTIME-MIX | OPEN P3 | **OPEN** | Partial keying | Locale parity only | Residual P3 |
| DEF-UX-POSTLOGIN-LOGIN-HASH | OPEN P4 | **CLOSED — VERIFIED** | resolvePostLoginLanding | POST-LOGIN note + code | app.js |

## Newly found during remediation

| ID | Sev | Status | Notes |
|----|-----|--------|-------|
| REM-REPORTS-QUERY-REQUIRED | P2 | CLOSED — VERIFIED | Non-nullable page/pageSize caused BadHttpRequest on bare GET |
| REM-ALERT-CENTER-V1-404 | P2 | CLOSED — VERIFIED | Alert Center only on v2; mirrored to v1 |
| REM-APPROVE-EMPTY-PROOF-GUID | P2 | CLOSED — VERIFIED | Guid.Empty treated as missing file; optional proof |
| REM-APPROVE-AFTER-CLOSED-OBS | P1 | CLOSED — VERIFIED | Auto-advance CorrectingObservation→…→UnderAuthorityReview when all obs closed |
| REM-OCCURRENCE-LIST-MISSING | P2 | CLOSED — VERIFIED | GET /alert-center/occurrences |
| REM-ILIKE-INMEMORY | P3 | CLOSED — VERIFIED | DbSearch dual path for unit tests |
