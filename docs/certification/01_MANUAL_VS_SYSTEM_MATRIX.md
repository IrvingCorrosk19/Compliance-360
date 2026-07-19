# 01 — Manual vs System Matrix

**Source of truth:** `docs/user-manual/` (`data/roles.json`, `screens.json`, `buttons.json`, `fields.json`, `workflows.json`)  
**System under test:** Compliance 360 / REGUTRACK (local Development)  
**Evidence:** `docs/certification/evidence/manual-vs-system-role-matrix.json`  
**Date:** 2026-07-17  

## Verdict summary

| Role | Manual contract | System result | Checks | Result |
|------|-----------------|---------------|--------|--------|
| Tenant Administrator (TAC) | Sidebar + RA tabs + TAC/Security UI | Exact match | 24/24 | **COINCIDE** |
| Regulatory Administrator (RA-ADM) | Config + Import + Product create | Exact match | 27/27 | **COINCIDE** |
| Regulatory Manager (RA-MGR) | Ops + Observation + External CT/RS | Exact match | 22/22 | **COINCIDE** |
| Regulatory Specialist (RA-SPEC) | Create + Prepare + Manufacturers | Exact match | 27/27 | **COINCIDE** |
| Regulatory Reviewer (RA-REV) | Review only | Exact match | 25/25 | **COINCIDE** |
| Regulatory Approver (RA-APPR) | Internal approval only | Exact match | 21/21 | **COINCIDE** |
| Regulatory Submitter (RA-SUB) | Submit only | Exact match | 21/21 | **COINCIDE** |
| Regulatory Viewer (RA-VIEW) | Read-only | Exact match | 30/30 | **COINCIDE** |
| Quality Manager (QM) | External decision + Audit Trail | Exact match | 22/22 | **COINCIDE** |

**Total:** 219/219 checks PASS · 9/9 roles COINCIDE

---

## Sidebar (global navigation)

| Route | Manual | System (after correction) | Result |
|-------|--------|---------------------------|--------|
| Dashboard | Yes (all RA + TAC + QM) | Yes | COINCIDE |
| Audit Trail / Bitácora | TAC + QM only | Gated by `AUDIT.READ` | COINCIDE |
| Regulatory Affairs | All RA roles + TAC + QM | Gated by REGULATORY.* READ | COINCIDE |
| Tenant Administration | TAC | `TENANT.USERS/ROLES/UPDATE` | COINCIDE |
| Security | TAC | Implemented + gated | COINCIDE |
| compliance (duplicate) | Not in manual | **Removed** | COINCIDE |

---

## RA console tabs (per role)

| Tab | TAC | RA-ADM | RA-MGR | SPEC | REV | APPR | SUB | VIEW | QM |
|-----|-----|--------|--------|------|-----|------|-----|------|-----|
| Dashboard | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Portafolio | ✓ | ✓ | ✓ | ✓ | — | — | — | ✓ | — |
| Pipeline | — | — | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | — |
| Expedientes | ✓ | — | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Registros CT/RS | ✓ | — | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Fabricantes | — | ✓ | — | ✓ | — | — | — | — | — |
| Licencias | ✓ | ✓ | — | — | — | — | — | — | — |
| Alertas | — | ✓ | ✓ | — | — | — | — | ✓ | — |
| Importación | — | ✓ | — | — | — | — | — | — | — |
| Configuración | ✓ | ✓ | — | — | — | — | — | — | — |
| SoD Settings | ✓ | ✓ | ✓ | — | — | — | — | — | — |

All cells verified by Playwright (`TAB-YES-*` / `TAB-NO-*`). Result: **COINCIDE**.

---

## Buttons (manual `buttons.json`)

| Button | Manual roles | System gate | Result |
|--------|--------------|-------------|--------|
| Nuevo producto + expediente | Specialist + RA-ADM | `PRODUCT.MANAGE` ∧ `DOSSIER.CREATE` | COINCIDE |
| Alta fabricante | Specialist + RA-ADM | `MANUFACTURER_DOCUMENT.MANAGE` + modal | COINCIDE |
| Nueva licencia | RA-ADM (+ TAC config) | `LICENSE.MANAGE` + modal | COINCIDE |
| Marcar recibido / transiciones prep | Specialist | `DOSSIER.UPDATE` | COINCIDE |
| Aceptar / Rechazar requisito | Reviewer | `DOSSIER.REVIEW` | COINCIDE |
| Aprobar internamente | Approver | `APPROVE_FOR_SUBMISSION` | COINCIDE |
| Registrar sometimiento | Submitter | `DOSSIER.SUBMIT` | COINCIDE |
| Registrar observación autoridad | Manager | `OBSERVATION.MANAGE` ∧ `DOSSIER.APPROVE` | COINCIDE |
| Registrar aprobación MINSA/CSS + CT/RS | Manager / QM | `DOSSIER.APPROVE` + modal | COINCIDE |
| Bootstrap / Import | TAC / RA-ADM | `REGULATORY.CONFIGURE` | COINCIDE |

---

## Fields (manual `fields.json`)

| Field | Manual | System | Result |
|-------|--------|--------|--------|
| brand, regulatoryName, catalogCode | Required | Modal required | COINCIDE |
| riskClass | A / B / C only | Dropdown A,B,C (D removed) | COINCIDE |
| countryCode | Required, default PA | Input required, default PA | COINCIDE |
| authorityId | Required | Select required | COINCIDE |
| manufacturer.legalName + country | Required (CN default) | Modal | COINCIDE |
| license.companyName + licenseType | Required | Modal | COINCIDE |
| observation.description | Required ≤4000 | Textarea modal | COINCIDE |
| CT/RS number + expiresOn | Required | Modal | COINCIDE |

---

## Workflow (manual `workflows.json`)

Certified end-to-end in `manual-workflow-cert.spec.ts` (21/21 steps PASS).  
Evidence: `evidence/manual-workflow-steps.json` + `flow-*.png`.

| Step | Role | Result |
|------|------|--------|
| Create product + dossier (enterprise modal) | Specialist | COINCIDE |
| Prep transitions | Specialist | COINCIDE |
| Accept critical requirements | Reviewer | COINCIDE |
| Approve for submission | Approver | COINCIDE |
| Submit | Submitter | COINCIDE |
| Open observation (modal) | Manager | COINCIDE |
| Respond observation | Specialist | COINCIDE |
| Resubmit + external CT/RS (modal) | Manager | COINCIDE |
| Manufacturer modal | Specialist | COINCIDE |
| License modal | RA-ADM | COINCIDE |
| Zero native `prompt()`/`alert()` | All | COINCIDE |

---

## API authorization (granular)

Endpoints previously gated by coarse `Regulatory.Manage` now use action-specific policies aligned to the manual:

| Endpoint | Policy |
|----------|--------|
| `POST /dossiers` | `Regulatory.Dossier.Create` |
| `POST /dossiers/{id}/transition` | `Regulatory.Dossier.Update` |
| `PUT /dossiers/{id}/dates` | `Regulatory.Dossier.Update` |
| `PUT .../requirements/{id}` | `Regulatory.Requirement.Update` |
| `POST .../observations` (+ respond) | `Regulatory.Observation.Manage` |
| `POST /manufacturers` (+ certificates) | `Regulatory.Manufacturer.Manage` |
| `POST /operating-licenses` (+ renewals) | `Regulatory.License.Manage` |
| `POST /renewals` | `Regulatory.Registration.Manage` |
| `POST .../artifacts` | `REGULATORY.PRODUCT.MANAGE` |

Denied probes for every non-authorized role returned HTTP 401/403. Result: **COINCIDE**.
