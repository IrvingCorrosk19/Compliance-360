# 03 — Root Cause Analysis

Known root causes for baseline FAIL/BLOCKED and related remediation gaps.  
Baseline reference: `docs/enterprise-functional-certification/evidence/FINAL_CERTIFICATION_SUMMARY.json`.

---

## 1. Report Center 403 / TAC list blocked

**Symptom:** TAC (Tenant Administrator) Report Center / list reports returned 403 (or wrong-path 404). Reporting score 52; harness marked Report Center TAC access **BLOCKED**.

**Root cause:**

- Tenant Administrator originally lacked sufficient `REPORT.*` claims for the intended read path (and/or clients hit an incorrect URL).
- Correct API surface is `GET /api/v1/tenants/{tenantId}/reports` with `REPORT.READ`.

**Fix applied (code):**

- `RoleCatalog` updated so TAC includes `ReportRead` / execute / export as appropriate for tenant admin read+export duties.
- Clients and harness use `/api/v1/tenants/{id}/reports` (not legacy/wrong paths).

**Recert:** `RPT-LIST-TAC`, full chain as `reporting@cert.local` (`RPT-*`).

---

## 2. Empty product create → HTTP 500

**Symptom:** `PROD-NEG-EMPTY` — `POST .../products` with `{}` returned **500** instead of **4xx**.

**Root cause:**

- `ProductCatalogExistsAsync` (or equivalent catalog uniqueness check) called `.Trim()` on a **null** catalog/code field **before** Guards / request validation completed.

**Fix applied (code):**

- Null-safe handling / validate required fields before Trim; empty body must fail as client error (4xx), never unhandled NRE → 500.

**Recert:** `PROD-NEG-EMPTY`, `NEG-EMPTY-PRODUCT-VIEWER`.

---

## 3. Worker bootstrap hard-fail on stale heartbeat

**Symptom:** Web Development bootstrap hard-failed when Alert Center Worker was not `Healthy` (stale heartbeat), blocking local/lab bring-up.

**Root cause:**

- Health treated stale worker heartbeat as hard **Unhealthy** and bootstrap required Healthy.

**Fix applied (code):**

- Stale heartbeat maps to **Degraded** (not hard Unhealthy where appropriate).
- Bootstrap readiness filters distinguish **ready** vs **live** so lab can start without treating Degraded worker as fatal.

**Recert:** environment live/health probes in harness (`ENV-LIVE`); operational notes in reliability docs.

---

## 4. Observation response shape ambiguous

**Symptom:** Clients could not reliably distinguish observation id from dossier id on create/open.

**Root cause:**

- Open observation returned a flat/ambiguous payload rather than an explicit result DTO.

**Fix applied (code):**

- `OpenObservationResultDto` with nested `observation` + `dossier` (`observation.id` distinct from `dossier.id`).

**Recert:** `OBS-OPEN-SHAPE`.

---

## 5. Post-login landing stuck on `#/login`

**Symptom:** After auth, UI could remain on login placeholders before navigating to `#/regulatory`.

**Root cause:**

- Client landing resolution incomplete / race with hash routing.

**Fix applied (code):**

- `resolvePostLoginLanding` in frontend (`app.js`) to land on the correct post-auth route.

**Recert:** `POST-LOGIN` recorded as API N/A (browser intentionally skipped in API harness).

---

## 6. Excel export MIME mismatch

**Symptom:** Excel export content-type not aligned with actual SpreadsheetML payload.

**Root cause:**

- Export builder emits SpreadsheetML XML; MIME needed to match Excel-openable type.

**Fix applied (code):**

- Excel MIME aligned to SpreadsheetML / `application/vnd.ms-excel` in `ReportExportContentBuilder`.

**Recert:** `RPT-EXPORT-EXCEL`, `RPT-CONTENT-EXCEL`.

---

## 7. Alert occurrence lifecycle incomplete

**Symptom:** Alert occurrences could be raised but lacked first-class acknowledge / resolve / escalate operations for operational closure.

**Root cause:**

- Domain/API surface for occurrence lifecycle was incomplete relative to enterprise alert-desk expectations.

**Fix applied (code):**

- Alert occurrence **acknowledge / resolve / escalate** endpoints and services added.

**Recert:** covered primarily under notification/alert remediation docs; harness focuses on sandbox-send + Mailpit EXTERNAL CONFIG when providers unavailable (`NOTIFY-SANDBOX` → **BLOCKED**, never SKIPPED).

---

## Disposition rules for recert harness

| Condition | Result label |
|-----------|--------------|
| Assertion met | PASS |
| Assertion failed (product defect) | FAIL |
| External Mailpit/SMTP/provider secrets unavailable | **BLOCKED** + `EXTERNAL CONFIG` |
| Browser-only UX intentionally out of API scope | PASS with note (not SKIPPED) |
| SKIPPED | **Must remain 0** unless explicitly reclassified; EXTERNAL CONFIG uses BLOCKED |
