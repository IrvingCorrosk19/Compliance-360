# 01 — Master Functional Test Plan (Regulatory Affairs)

**Program:** Final Functional Certification — RA BC  
**Contract:** `REGUTRACK 02JUN26 MG.xlsx`  
**Version:** 1.0  
**Date:** 2026-07-14  
**Prerequisite:** Role/SoD GO (`13`, `24`)  
**Execution status:** NOT STARTED

---

## 1. Purpose

Define the **complete** functional test approach for certifying Compliance 360 Regulatory Affairs against REGUTRACK. This plan supersedes the high-level notes in `docs/regulatory-affairs/09_TEST_PLAN.md` and aligns with the authorized workflow and lab configuration in documents 00, 05–08.

---

## 2. Test objectives

1. Prove each critical REGUTRACK column (CTT REGISTROS 1–55, DOCUMENTACION, LICENCIAS OP) maps to correct persistence, API, screen, role, and workflow behavior.
2. Prove RA Console views and actions work for each `@cert.local` role without SoD violations.
3. Prove the golden-path dossier lifecycle from product creation through CT/RS issuance and renewal.
4. Prove XLSX import can migrate historical data with auditable validation output.
5. Produce evidence sufficient for Product Owner acceptance and external audit sampling.

---

## 3. Test levels

| Level | Scope | Tooling | Owner |
|-------|-------|---------|-------|
| L0 Unit | Domain invariants, transitions, alert rules | `dotnet test` RegulatoryAffairsDomainTests | Dev |
| L1 API | REST contracts, RBAC, SoD HTTP codes | PowerShell / REST Client / existing SoD E2E | QA |
| L2 UI component | RA Console buttons, visibility, error toasts | Manual + Playwright headful | QA |
| L3 Workflow E2E | Multi-role golden paths | Manual orchestration + API audit | QA + SME |
| L4 Data parity | Import commit vs Excel row counts | Import job + SQL reconciliation | QA |
| L5 Regression spot | SoD not broken by functional fixes | `run-sod-api-e2e.ps1` sample | Security |

**Certification minimum:** L1 + L2 + L3 for all Critical items; L4 for import; L0/L5 on each build.

---

## 4. Test design techniques

- **Requirements traceability:** One-to-one from Excel column → test case ID in `09_REGUTRACK_TRACEABILITY_MATRIX.md`.
- **Role-based testing:** Each permission boundary exercised per `04_ROLE_BASED_TEST_STRATEGY.md`.
- **Risk-based prioritization:** Critical path first (workflow + submit gate + CT/RS); see `08_RISK_BASED_TESTING.md`.
- **Equivalence partitioning:** Product categories, authorities (MINSA/CSS), process types (new vs renewal).
- **Negative testing:** SoD denial, incomplete checklist submit block, invalid dates, wrong role UI hidden.

---

## 5. Test environment

See `05_TEST_ENVIRONMENT_AND_LAB.md`. Summary:

- Tenant: `82af3877-2786-4d39-bce8-c981101c771d`
- URL: `http://localhost:5272`
- Fresh JWT per role login; logout clears storage
- Bootstrap before test cycle

---

## 6. Test data

See `06_TEST_DATA_STRATEGY.md`. Summary:

- Synthetic dossiers for controlled workflow tests (prefix `CERT-`)
- Subset of real Excel rows for import parity (sanitized if needed)
- Manufacturer and license fixtures linked to dossiers under test
- Reset procedure documented before each major cycle

---

## 7. Test users

See `07_TEST_USER_MATRIX.md`. Nine lab identities including Tenant Admin (non-RA operate).

---

## 8. Workflow test scenarios (mandatory)

### 8.1 SC-WF-001 — New registration (happy path)

| Step | Actor | Action | Expected |
|------|-------|--------|----------|
| 1 | U-RA-SPEC | Create product + dossier from Portfolio | Dossier `Planning` |
| 2 | U-RA-SPEC | Transition through factory wait → docs received → assembling → ready | Status progression |
| 3 | U-RA-SPEC | Mark all critical requirements received | Requirements `Accepted` or prep complete |
| 4 | U-RA-REV | Accept/reject requirements | Review recorded |
| 5 | U-RA-APPR | Approve for submission | `ApprovedForSubmission` |
| 6 | U-RA-SUB | Submit | `Submitted`; SoD OK |
| 7 | U-RA-MGR | Register authority observation | `Observed` |
| 8 | U-RA-SPEC | Respond to observation | Response logged |
| 9 | U-RA-SUB | Resubmit (if applicable) | `Resubmitted` |
| 10 | U-RA-MGR | External approve + CT/RS number | `Approved`; registration active |

### 8.2 SC-WF-002 — SoD negative matrix (sample)

| Attempt | Actor | Action | Expected |
|---------|-------|--------|----------|
| N1 | U-RA-SPEC | Approve for submission on own dossier | 403 / button hidden |
| N2 | U-RA-APPR | Submit same dossier they approved | 403 |
| N3 | U-RA-VIEW | Any mutating action | 403 / no buttons |

### 8.3 SC-WF-003 — Renewal

| Step | Actor | Action | Expected |
|------|-------|--------|----------|
| 1 | U-RA-MGR / SPEC | Start renewal from registration nearing expiry | New dossier or renewal case |
| 2 | — | Complete shortened path to new CT/RS | Updated `ExpiresOn` |

### 8.4 SC-WF-004 — Import migration

| Step | Actor | Action | Expected |
|------|-------|--------|----------|
| 1 | U-RA-ADM | Stage `REGUTRACK 02JUN26 MG.xlsx` | Job staged |
| 2 | U-RA-ADM | Validate → simulate → commit | Job success |
| 3 | QA | Reconcile row counts vs decomposition JSON | Within documented tolerance |

---

## 9. Module test suites

### 9.1 Portfolio & products (RA-PORT-*)

- CRUD product fields cols 1–9, 14–15, 51, 53, 88
- Link manufacturer col 7
- New product + dossier wizard

### 9.2 Dossier workspace (RA-DOS-*)

- Requirements checklist cols 18–39
- Milestone dates cols 40–50
- Comments col 54
- History events cols 56–87 equivalence

### 9.3 Pipeline & dashboard (RA-PIP-*)

- Kanban columns match `PIPELINE_COLUMNS`
- Dashboard KPIs: stuck, bottleneck, opportunity $
- Alert evaluation col 44

### 9.4 Registrations (RA-REG-*)

- CT/RS list cols 12–13, 55
- Days remaining calculation

### 9.5 Manufacturers (RA-MFR-*)

- DOCUMENTACION sheet parity
- Certificate expiry alerts

### 9.6 Licenses (RA-LIC-*)

- Operating license CRUD
- Renewal checklist seed on `StartLicenseRenewal`

### 9.7 Import & config (RA-IMP-*, RA-CFG-*)

- XLSX stage/upload
- Bootstrap authorities + REGUTRACK-PA-DEFAULT pack
- SoD settings read

---

## 10. Traceability

| Source | Target |
|--------|--------|
| Excel columns | `09_REGUTRACK_TRACEABILITY_MATRIX.md` |
| UI actions | `10_SCREEN_ACTION_COVERAGE_MATRIX.md` |
| Roles | `07_TEST_USER_MATRIX.md` |
| Decomposition metadata | `evidence/regutrack_decomposition.json` |

Test case ID format:

- `RA-CTT-NNN` — CTT REGISTROS column tests
- `RA-DOC-NNN` — DOCUMENTACION
- `RA-LIC-NNN` — LICENCIAS OP
- `RA-SCR-NNN` — Screen actions
- `RA-WF-NNN` — Workflow scenarios

---

## 11. Entry / exit criteria

### Entry

- Charter 00 signed
- Lab environment up (05)
- Users provisioned (07)
- Build green

### Exit

- All Critical/High rows in 09 = PASS or waived with PO approval
- All Critical actions in 10 = PASS
- SC-WF-001 through SC-WF-004 complete
- No open Sev-1/2 defects
- Evidence archived under `evidence/functional/`

---

## 12. Defect management

| Severity | Definition | Blocks GO? |
|----------|------------|------------|
| S1 Critical | Data loss, SoD bypass, wrong CT/RS | Yes |
| S2 High | Workflow blocked, wrong status | Yes |
| S3 Medium | UI defect with workaround | No |
| S4 Low | Cosmetic | No |

Log in `05_CORRECTION_LOG.md`; retest in `06_RETEST_RESULTS.md`.

---

## 13. Schedule (indicative)

| Week | Activity |
|------|----------|
| W1 | P1 smoke + 09 cols 1–30 |
| W2 | 09 cols 31–55 + DOCUMENTACION + LIC |
| W3 | 10 screen matrix + workflow E2E |
| W4 | Import + defect fix + retest + sign-off |

---

## 14. Reporting

Daily: execution % (09/10 status columns).  
Final: `09_FUNCTIONAL_CERTIFICATION_REPORT.md` + `10_EXECUTIVE_SUMMARY.md`.

---

## 15. Approvals

| Role | Name | Date | Status |
|------|------|------|--------|
| QA Lead | | | Pending |
| Product Owner | | | Pending |
| Engineering | | | Pending |
