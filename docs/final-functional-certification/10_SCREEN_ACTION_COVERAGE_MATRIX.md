# 10 — Screen Action Coverage Matrix (RA Console)

**Source UI:** `src/Compliance360.Web/wwwroot/regulatory-affairs.js`  
**Lab tenant:** `82af3877-2786-4d39-bce8-c981101c771d`  
**Base URL:** `http://localhost:5272`  
**Date:** 2026-07-14  
**Global execution status:** NOT EXECUTED

---

## How to use

1. Log in as specified **Required role** (or verify denial for **Denied roles**).
2. Navigate to **View** and perform **Action**.
3. Confirm **Expected result** via UI + Network tab.
4. Save screenshot/trace to `evidence/functional/screens/{TestCase}/`.
5. Update **Status**: `NOT EXECUTED` → `PASS` | `FAIL` | `WAIVED`.

---

## 1. Global navigation

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Nav | Open Dashboard | Viewer+ | — | renderRegulatoryAffairs loads | RA-SCR-001 | — | NOT EXECUTED |
| Nav | Open Portafolio | Viewer+ | — | showPortfolio | RA-SCR-002 | — | NOT EXECUTED |
| Nav | Open Pipeline | Viewer+ | — | showPipeline kanban | RA-SCR-003 | — | NOT EXECUTED |
| Nav | Open Expedientes | Viewer+ | — | showDossiers list | RA-SCR-004 | — | NOT EXECUTED |
| Nav | Open Registros CT/RS | Viewer+ | — | showRegistrations | RA-SCR-005 | — | NOT EXECUTED |
| Nav | Open Fabricantes | Viewer+ / mfr read | — | showManufacturers | RA-SCR-006 | — | NOT EXECUTED |
| Nav | Open Licencias | Viewer+ / license read | Viewer if no grant | showLicenses | RA-SCR-007 | — | NOT EXECUTED |
| Nav | Open Alertas | Viewer+ | — | showAlerts | RA-SCR-008 | — | NOT EXECUTED |
| Nav | Open Importación | Admin (CONFIGURE) | Specialist, Viewer, Submitter | showImport gate | RA-SCR-009 | — | NOT EXECUTED |
| Nav | Open Configuración | Admin (CONFIGURE) | Non-admin | showConfig gate | RA-SCR-010 | — | NOT EXECUTED |
| Nav | Open SoD Settings | Admin / Manager SOD | Viewer | showSod gate | RA-SCR-011 | — | NOT EXECUTED |

---

## 2. Dashboard actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Dashboard | Load KPI cards | Viewer+ | — | GET /dashboard | RA-SCR-020 | — | NOT EXECUTED |
| Dashboard | Load dossier list (filtered) | Specialist | — | GET /dossiers + role filter | RA-SCR-021 | — | NOT EXECUTED |
| Dashboard | Click stuck case chip → open dossier | Viewer+ | — | Nav to Expediente detail | RA-SCR-022 | — | NOT EXECUTED |
| Dashboard | Verify bottleneck section renders | Viewer+ | — | Dashboard JSON bottleneck field | RA-SCR-023 | — | NOT EXECUTED |
| Dashboard | Verify opportunity $ display | Viewer+ | — | Dashboard aggregates | RA-SCR-024 | — | NOT EXECUTED |

---

## 3. Portfolio actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Portafolio | Load product table | Viewer+ | — | GET /products | RA-SCR-030 | — | NOT EXECUTED |
| Portafolio | **Nuevo producto + expediente** | Specialist | Viewer, Approver, Submitter | POST /products + POST /dossiers | RA-SCR-031 | — | NOT EXECUTED |
| Portafolio | Verify new dossier has REGUTRACK pack | Specialist | — | Dossier requirements count = 22 | RA-SCR-032 | — | NOT EXECUTED |
| Portafolio | Product fields editable via create prompt | Specialist | Viewer | Prompt UI + POST body | RA-SCR-033 | — | NOT EXECUTED |

---

## 4. Pipeline actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Pipeline | Render 13 kanban columns | Viewer+ | — | PIPELINE_COLUMNS | RA-SCR-040 | — | NOT EXECUTED |
| Pipeline | Dossier card in correct column by status | Viewer+ | — | Status === column | RA-SCR-041 | — | NOT EXECUTED |
| Pipeline | Click card → dossier detail | Viewer+ | — | selectedDossierId set | RA-SCR-042 | — | NOT EXECUTED |
| Pipeline | Columns Vencido / Renovacion present | Viewer+ | — | UI includes expiry cols | RA-SCR-043 | — | NOT EXECUTED |

---

## 5. Expedientes — list actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Expedientes | Load dossier table | Viewer+ | — | GET /dossiers | RA-SCR-050 | — | NOT EXECUTED |
| Expedientes | Row click → detail | Viewer+ | — | showDossierDetail | RA-SCR-051 | — | NOT EXECUTED |
| Expedientes | **← Expedientes** back button | Viewer+ | — | Return to list | RA-SCR-052 | — | NOT EXECUTED |

---

## 6. Expedientes — detail / workflow actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Expediente detail | **Pedir docs fábrica** | Specialist | Viewer | POST transition → WaitingManufacturerDocuments | RA-SCR-060 | — | NOT EXECUTED |
| Expediente detail | **Docs recibidos** | Specialist | Viewer | POST transition → DocumentsReceived | RA-SCR-061 | — | NOT EXECUTED |
| Expediente detail | **Armar** | Specialist | Viewer | POST transition → Assembling | RA-SCR-062 | — | NOT EXECUTED |
| Expediente detail | **Declarar técnicamente completo** | Specialist | Viewer | POST transition → ReadyForSubmission | RA-SCR-063 | — | NOT EXECUTED |
| Expediente detail | **Marcar recibido** (requirement) | Specialist | Reviewer, Viewer | PUT requirement status Received | RA-SCR-064 | — | NOT EXECUTED |
| Expediente detail | **Aceptar** (requirement) | Reviewer | Specialist, Viewer | PUT requirement Accepted | RA-SCR-065 | — | NOT EXECUTED |
| Expediente detail | **Rechazar** (requirement) | Reviewer | Viewer | PUT requirement Rejected | RA-SCR-066 | — | NOT EXECUTED |
| Expediente detail | **Aprobar internamente para sometimiento** | Approver | Specialist, Submitter | POST /approve-for-submission | RA-SCR-067 | — | NOT EXECUTED |
| Expediente detail | **Registrar sometimiento** | Submitter | Specialist, Viewer | POST /submit | RA-SCR-068 | — | NOT EXECUTED |
| Expediente detail | Submit blocked if critical reqs incomplete | Submitter | — | 400/422 error toast | RA-SCR-069 | — | NOT EXECUTED |
| Expediente detail | **Registrar observación autoridad** | Manager | Specialist, Submitter | POST /observations | RA-SCR-070 | — | NOT EXECUTED |
| Expediente detail | **Responder** (observation) | Specialist | Viewer | POST observation response | RA-SCR-071 | — | NOT EXECUTED |
| Expediente detail | **Registrar aprobación MINSA/CSS + CT/RS** | Manager / QM | Submitter, Viewer | POST /approve + prompts | RA-SCR-072 | — | NOT EXECUTED |
| Expediente detail | Flow step indicator matches status | Viewer+ | — | FLOW_STEPS highlight | RA-SCR-073 | — | NOT EXECUTED |
| Expediente detail | Waiver reason prompt on transition | Specialist | — | waiverReason in POST body | RA-SCR-074 | — | NOT EXECUTED |
| Expediente detail | SoD: Specialist cannot internal approve | Specialist | — | Button hidden / 403 | RA-SCR-075 | — | NOT EXECUTED |
| Expediente detail | SoD: Approver cannot submit | Approver | — | Button hidden / 403 | RA-SCR-076 | — | NOT EXECUTED |

---

## 7. Registros CT/RS actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Registros | Load registration table | Viewer+ | — | GET /registrations | RA-SCR-080 | — | NOT EXECUTED |
| Registros | Display CT/RS number | Viewer+ | — | registrationNumber column | RA-SCR-081 | — | NOT EXECUTED |
| Registros | Display issued / expires / days remaining | Viewer+ | — | computed daysRemaining | RA-SCR-082 | — | NOT EXECUTED |

---

## 8. Fabricantes actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Fabricantes | Load manufacturers list | Viewer+ | — | GET /manufacturers | RA-SCR-090 | — | NOT EXECUTED |
| Fabricantes | Load certificates list | Viewer+ | — | GET /manufacturer-certificates | RA-SCR-091 | — | NOT EXECUTED |
| Fabricantes | **Alta fabricante** (+ default ISO cert) | Specialist / mfrManage | Viewer | POST /manufacturers + certificates | RA-SCR-092 | — | NOT EXECUTED |
| Fabricantes | Button hidden without mfrManage | Viewer | — | No #ra-add-mfr | RA-SCR-093 | — | NOT EXECUTED |

---

## 9. Licencias actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Licencias | Load license table | Viewer+ | — | GET /operating-licenses | RA-SCR-100 | — | NOT EXECUTED |
| Licencias | **Nueva licencia** | Manager / licenseManage | Viewer | POST /operating-licenses | RA-SCR-101 | — | NOT EXECUTED |
| Licencias | Button hidden without licenseManage | Viewer | — | No #ra-add-lic | RA-SCR-102 | — | NOT EXECUTED |

---

## 10. Alertas actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Alertas | **Evaluate alerts** on view load | Viewer+ | — | GET /alerts/evaluate | RA-SCR-110 | — | NOT EXECUTED |
| Alertas | Display CT/RS expiry alert | Viewer+ | — | alertType message | RA-SCR-111 | — | NOT EXECUTED |
| Alertas | Display cert expiry alert | Viewer+ | — | manufacturer cert alert | RA-SCR-112 | — | NOT EXECUTED |
| Alertas | Empty state when no alerts | Viewer+ | — | "Sin alertas nuevas" | RA-SCR-113 | — | NOT EXECUTED |

---

## 11. Importación actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Importación | View denied without CONFIGURE | Specialist | — | Static message | RA-SCR-120 | — | NOT EXECUTED |
| Importación | Load import job history | Admin | — | GET /imports | RA-SCR-121 | — | NOT EXECUTED |
| Importación | Select XLSX file | Admin | — | File input accept .xlsx | RA-SCR-122 | — | NOT EXECUTED |
| Importación | **Stage XLSX** upload | Admin | Viewer | POST /imports/xlsx multipart | RA-SCR-123 | — | NOT EXECUTED |
| Importación | Auto bootstrap before stage | Admin | — | ensureBootstrap called | RA-SCR-124 | — | NOT EXECUTED |
| Importación | Toast success / error | Admin | — | toast() feedback | RA-SCR-125 | — | NOT EXECUTED |

---

## 12. Configuración actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Configuración | View denied without CONFIGURE | Viewer | — | Static message | RA-SCR-130 | — | NOT EXECUTED |
| Configuración | **Bootstrap regulatorio** | Admin | — | POST /bootstrap | RA-SCR-131 | — | NOT EXECUTED |
| Configuración | Display bootstrap JSON (authorities + pack) | Admin | — | Pre#ra-boot-out | RA-SCR-132 | — | NOT EXECUTED |

---

## 13. SoD Settings actions

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| SoD Settings | View denied without SOD/CONFIGURE | Viewer | — | Static message | RA-SCR-140 | — | NOT EXECUTED |
| SoD Settings | Load policy JSON | Manager / Admin | — | GET /sod-settings | RA-SCR-141 | — | NOT EXECUTED |
| SoD Settings | Display PUT API note | Manager / Admin | — | Help text visible | RA-SCR-142 | — | NOT EXECUTED |

---

## 14. Cross-cutting UX

| View | Action | Required role | Denied roles | API / behavior | Test Case | Evidence | Status |
|------|--------|---------------|--------------|----------------|-----------|----------|--------|
| Global | API error surfaces as toast | Any | — | toast(msg, error) | RA-SCR-150 | — | NOT EXECUTED |
| Global | HTML escape on user content | Viewer+ | — | esc() no XSS | RA-SCR-151 | — | NOT EXECUTED |
| Global | Tenant id from JWT when state missing | Any | — | api() tenant fallback | RA-SCR-152 | — | NOT EXECUTED |
| Global | Status labels Spanish | Viewer+ | — | STATUS_LABELS map | RA-SCR-153 | — | NOT EXECUTED |

---

## Execution summary (update during test run)

| Section | Actions | NOT EXECUTED | PASS | FAIL | WAIVED |
|---------|---------|--------------|------|------|--------|
| 1 Nav | 11 | 11 | 0 | 0 | 0 |
| 2 Dashboard | 5 | 5 | 0 | 0 | 0 |
| 3 Portfolio | 4 | 4 | 0 | 0 | 0 |
| 4 Pipeline | 4 | 4 | 0 | 0 | 0 |
| 5 Expedientes list | 3 | 3 | 0 | 0 | 0 |
| 6 Expediente detail | 17 | 17 | 0 | 0 | 0 |
| 7 Registros | 3 | 3 | 0 | 0 | 0 |
| 8 Fabricantes | 4 | 4 | 0 | 0 | 0 |
| 9 Licencias | 3 | 3 | 0 | 0 | 0 |
| 10 Alertas | 4 | 4 | 0 | 0 | 0 |
| 11 Import | 6 | 6 | 0 | 0 | 0 |
| 12 Config | 3 | 3 | 0 | 0 | 0 |
| 13 SoD | 3 | 3 | 0 | 0 | 0 |
| 14 UX | 4 | 4 | 0 | 0 | 0 |
| **Total** | **74** | **74** | **0** | **0** | **0** |

---

## Document control

| Version | Date | Change |
|---------|------|--------|
| 1.0 | 2026-07-14 | Initial matrix from regulatory-affairs.js; all NOT EXECUTED |
