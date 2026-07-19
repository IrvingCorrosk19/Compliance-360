# 00 — Certification Charter (Regulatory Affairs Functional)

**Program:** Final Functional Certification — Regulatory Affairs BC  
**Product:** Compliance 360 Enterprise · RA Console  
**Contract baseline:** `REGUTRACK 02JUN26 MG.xlsx`  
**Date:** 2026-07-14  
**Status:** AUTHORIZED — Role/SoD **GO**; functional suite **NOT EXECUTED**

---

## 1. Executive summary

This charter authorizes a **from-zero** enterprise functional certification of Compliance 360 Regulatory Affairs against the REGUTRACK contract. Security and SoD certification is complete (`docs/regulatory-affairs/security/13_ROLE_CERTIFICATION_READINESS.md`, `24_FINAL_ROLE_AND_SOD_CERTIFICATION.md`). Functional validation against Excel parity, workflow fidelity, and RA Console UX has **not** been executed under the new model and must be completed before production cutover of REGUTRACK replacement.

**Decision gate:** Role certification = **GO**. Functional certification = **PENDING EXECUTION**.

---

## 2. Business objective

Replace the operational Excel workbook (`REGUTRACK 02JUN26 MG.xlsx`) with Compliance 360 as the system of record for:

- Portfolio of medical device products and sanitary registrations (CT/RS)
- Registration pipeline (tubería) and dossier lifecycle
- Manufacturer documentation vault (DOCUMENTACION sheet)
- Operating licenses and renewal checklists (CTT LICENCIAS OP)
- Alerts, dashboard KPIs, and historical import

Success means a regulatory affairs team can run day-to-day operations without the spreadsheet, with auditable workflow, SoD-enforced approvals, and traceability from every Excel column to a tested system behavior.

---

## 3. Scope

### 3.1 In scope

| Area | Description |
|------|-------------|
| RA Console SPA | All views in `regulatory-affairs.js`: dashboard, portfolio, pipeline, dossiers, registrations, manufacturers, licenses, alerts, import, config, sod |
| API surface | `/api/v1/tenants/{tenantId}/regulatory/*` per `03_API_CONTRACTS.md` |
| Workflow | Prep → Review → Internal Approval → Submit → Authority → Obs → Resubmit → External Decision → CT/RS → Renewal |
| REGUTRACK parity | Critical columns 1–55 (CTT REGISTROS), DOCUMENTACION, CTT LICENCIAS OP |
| Import | XLSX stage → validate → simulate → commit |
| RBAC / SoD | Lab users `@cert.local`; SoD gates on submit, internal approve, external approve |
| Evidence | Screenshots, API payloads, DB snapshots, Playwright traces |

### 3.2 Out of scope

| Area | Rationale |
|------|-----------|
| New product features | Certification only; defects fixed under change control |
| Sales Viewer / Document Contributor roles | Not implemented in v1 model (documented exclusion in doc 24) |
| FADDI / Panamá Digital integration | Manual task only; no external API |
| Compliance Studio as pack designer | Bridge partial; pack API tested, Studio UI excluded |
| Load / penetration testing | Separate security program |
| Non-RA modules (CAPA, Documents core, etc.) | Covered by prior enterprise certification |

---

## 4. References and artifacts

| Artifact | Location |
|----------|----------|
| Functional contract | `REGUTRACK 02JUN26 MG.xlsx` (source of truth) |
| Column decomposition | `evidence/regutrack_decomposition.json` |
| Implementation inventory | `docs/regulatory-affairs/REGULATORY_COVERAGE_MATRIX.md` |
| Traceability matrix (executable) | `09_REGUTRACK_TRACEABILITY_MATRIX.md` |
| Screen/action matrix | `10_SCREEN_ACTION_COVERAGE_MATRIX.md` |
| SoD GO evidence | `docs/regulatory-affairs/security/13`, `24`, `17`–`23` |
| Domain / API / workflow | `docs/regulatory-affairs/01`, `03`, `05` |
| RA Console implementation | `src/Compliance360.Web/wwwroot/regulatory-affairs.js` |

---

## 5. Valid workflow (certification path)

Every end-to-end dossier test must traverse the **authorized** state machine:

```
Planning (Prep)
  → WaitingManufacturerDocuments
  → DocumentsReceived
  → Assembling
  → ReadyForSubmission (Review complete)
  → ApprovedForSubmission (Internal Approval — SoD: ≠ preparer)
  → Submitted (Submitter — SoD: ≠ approver)
  → UnderAuthorityReview
  → [Observed → CorrectingObservation → Resubmitted] (optional branch)
  → Approved (External Decision + CT/RS creation)
  → [Renewal via registrations / StartRenewal]
```

Negative tests must prove illegal transitions and SoD violations are blocked at API and UI.

---

## 6. Environment

| Parameter | Value |
|-----------|-------|
| Lab tenant ID | `82af3877-2786-4d39-bce8-c981101c771d` |
| Base URL | `http://localhost:5272` |
| API prefix | `/api/v1/tenants/{tenantId}/regulatory` |
| Test users | `@cert.local` per `07_TEST_USER_MATRIX.md` |
| Bootstrap | `POST .../regulatory/bootstrap` before first run |

---

## 7. Roles and responsibilities

| Role | Responsibility |
|------|----------------|
| Product Owner (Regulatory) | Accept/reject functional parity vs REGUTRACK |
| QA Lead | Own master plan, execution schedule, defect triage |
| Regulatory SME | Validate business meaning of columns and workflow |
| Engineering | Fix defects; no scope creep during cert window |
| Security | Confirm SoD fixes do not regress (spot-check only; full SoD already GO) |
| ISO / Compliance consultant | Review evidence pack for audit readiness |

---

## 8. Entry criteria

1. Role/SoD certification **GO** (docs 13 + 24).
2. Application builds: `dotnet build -c Release` without errors.
3. Unit tests: Regulatory + SoD suites green.
4. Lab tenant seeded with `@cert.local` users (doc 17).
5. Planning documents 00–10 approved (this pack).
6. `REGUTRACK 02JUN26 MG.xlsx` available for import and spot-check.

---

## 9. Exit criteria (functional GO)

| # | Criterion | Measure |
|---|-----------|---------|
| E1 | Traceability | 100% of critical rows in `09` executed with evidence |
| E2 | Screen coverage | 100% of actions in `10` executed or formally waived |
| E3 | Workflow E2E | ≥3 dossiers: new registration, observation/resubmit, renewal |
| E4 | Import | Full XLSX commit with validation report archived |
| E5 | SoD regression | 0 new Critical/High SoD findings in functional pass |
| E6 | Defects | 0 open Critical/High functional defects |
| E7 | PO sign-off | `07_PRODUCT_OWNER_REVIEW.md` updated |

---

## 10. Phases and deliverables

| Phase | Duration (est.) | Deliverable |
|-------|-----------------|-------------|
| P0 Planning | Complete | Documents 00–10 |
| P1 Environment smoke | 0.5 day | Lab login, bootstrap, API health |
| P2 Column-level tests | 3–5 days | Matrix 09 evidence folder |
| P3 Screen/action tests | 2–3 days | Matrix 10 + Playwright |
| P4 Workflow E2E | 2 days | 3 golden-path dossiers |
| P5 Import migration | 1 day | Commit job + reconciliation report |
| P6 Defect fix & retest | TBD | `05_CORRECTION_LOG.md`, `06_RETEST_RESULTS.md` |
| P7 Sign-off | 0.5 day | `09_FUNCTIONAL_CERTIFICATION_REPORT.md` |

---

## 11. Risk acceptance

| Risk | Acceptance |
|------|------------|
| CompanyMetadata (licenses) pending | Document as known gap; non-blocking if PO waives |
| TECH_SHEET file hard-link partial | Test textual ref + requirement gate; waive orphan UI link |
| Excel Fecha Actualización 2–32 as typed history | Accept equivalence per coverage matrix |
| Studio pack designer disconnected | Test bootstrap pack only |

---

## 12. Authorization

| Name | Role | Signature | Date |
|------|------|-----------|------|
| _TBD_ | Product Owner | | |
| _TBD_ | QA Lead | | |
| _TBD_ | Engineering Lead | | |

**Charter status:** APPROVED FOR EXECUTION (planning complete; execution not started).
