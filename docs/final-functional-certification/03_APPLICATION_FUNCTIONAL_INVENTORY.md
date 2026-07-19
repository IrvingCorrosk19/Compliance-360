# 03 — Application Functional Inventory (RA Console)

**Source:** `src/Compliance360.Web/wwwroot/regulatory-affairs.js`  
**Entry point:** `window.renderRegulatoryAffairs()`  
**API base:** `/api/v1/tenants/{tenantId}/regulatory`  
**Date:** 2026-07-14  
**Execution status:** Inventory complete; functional verification NOT EXECUTED

---

## 1. Overview

The Regulatory Affairs Console is a single-page module embedded in Compliance 360. Navigation is view-based (no client router); state is held in closure variables (`view`, `selectedDossierId`). Permission gates use JWT claims via `window.hasPermission` or decoded `c360.token`.

### 1.1 Registered views (`ALL_VIEWS`)

| View key | Label (ES) | Primary purpose |
|----------|--------------|-----------------|
| `dashboard` | Dashboard | KPIs, stuck cases, bottleneck, quick open dossier |
| `portfolio` | Portafolio | Product catalog; create product + dossier |
| `pipeline` | Pipeline | Kanban by dossier status (tubería) |
| `dossiers` | Expedientes | Dossier list + detail workspace |
| `registrations` | Registros CT/RS | Active sanitary registrations |
| `manufacturers` | Fabricantes | Manufacturer + certificate vault |
| `licenses` | Licencias | Operating licenses |
| `alerts` | Alertas | Evaluate and list regulatory alerts |
| `import` | Importación | REGUTRACK XLSX staging |
| `config` | Configuración | Bootstrap authorities + requirement pack |
| `sod` | SoD Settings | Tenant SoD policy read |

---

## 2. Permission model (UI)

| Permission constant | Code | Typical roles |
|--------------------|------|---------------|
| `productManage` | REGULATORY.PRODUCT.MANAGE | Specialist, Manager |
| `dossierCreate` | REGULATORY.DOSSIER.CREATE | Specialist |
| `dossierUpdate` | REGULATORY.DOSSIER.UPDATE | Specialist |
| `dossierReview` | REGULATORY.DOSSIER.REVIEW | Reviewer |
| `approveInternal` | REGULATORY.DOSSIER.APPROVE_FOR_SUBMISSION | Approver |
| `submit` | REGULATORY.DOSSIER.SUBMIT | Submitter |
| `approveExternal` | REGULATORY.DOSSIER.APPROVE | Manager, QM |
| `reqManage` | REGULATORY.REQUIREMENT.MANAGE | Specialist |
| `obsManage` | REGULATORY.OBSERVATION.MANAGE | Manager |
| `configure` | REGULATORY.CONFIGURE | Admin |
| `sodManage` | REGULATORY.SOD.MANAGE | Manager, Admin |
| `mfrManage` | REGULATORY.MANUFACTURER_DOCUMENT.MANAGE | Specialist, Admin |
| `licenseManage` | REGULATORY.LICENSE.MANAGE | Admin, Manager |
| `reportRead` | REGULATORY.REPORT.READ | Viewer, all read roles |

### 2.1 Role profiles (`roleProfile()`)

Derived profile affects dashboard filtering and nav visibility:

| Profile | Detection heuristic |
|---------|---------------------|
| `admin` | configure without create/approve/submit |
| `manager` | approveExternal + sodManage without create |
| `approver` | approveInternal without submit/create |
| `submitter` | submit without approve/create |
| `reviewer` | dossierReview without create |
| `specialist` | dossierCreate |
| `viewer` | default read-only |

---

## 3. View inventory

### 3.1 Dashboard (`showDashboard`)

**API calls:** `GET /dashboard`, `GET /dossiers`

**Displays:**

- Summary cards (counts, opportunity value, expiring registrations)
- Stuck dossiers list (role-filtered)
- Pipeline bottleneck indicator
- Click-through chips → open dossier detail

**Role filters:**

- `specialist`: Planning, WaitingManufacturerDocuments, Assembling
- `reviewer`: ReadyForSubmission, Assembling, DocumentsReceived, Planning
- `approver`: ReadyForSubmission only
- `submitter`: ApprovedForSubmission
- `manager`: Submitted, UnderAuthorityReview, Observed
- `viewer`: all dossiers

**REGUTRACK mapping:** Implicit dashboard (cols 44, 52, 53); tubería summary.

---

### 3.2 Portfolio (`showPortfolio`)

**API calls:** `GET /products`, `GET /dossiers`, `POST /products`, `POST /dossiers`

**Displays:**

- Product table: country, category, brand, regulatory name, catalog, risk class, initiative, opportunity
- Action: **Nuevo producto + expediente** (if `dossierCreate`)

**Creates:**

1. Product with PA defaults (country `PA`, category prompt, etc.)
2. Linked dossier with `REGUTRACK-PA-DEFAULT` pack applied

**REGUTRACK mapping:** CTT REGISTROS cols 1–9, 14–15, 51, 53, 88.

---

### 3.3 Pipeline (`showPipeline`)

**API calls:** `GET /dossiers`

**Displays:**

- Kanban columns: `PIPELINE_COLUMNS` (13 columns including Vencido, Renovacion)
- Card per dossier: case number, status label
- Click → dossier detail

**REGUTRACK mapping:** CTT REGISTROS TUBERIA sheet semantics.

---

### 3.4 Dossiers — list (`showDossiers`)

**API calls:** `GET /dossiers`

**Displays:**

- Table: case number, status, process type
- Row click → detail

---

### 3.5 Dossiers — detail (`showDossierDetail`)

**API calls:** `GET /dossiers/{id}`, transitions, requirements, observations

**Displays:**

- Status, process type, authority, dates, comments
- Flow step indicator (`FLOW_STEPS`)
- **Workflow action buttons** (permission + status gated):
  - Pedir docs fábrica → `WaitingManufacturerDocuments`
  - Docs recibidos → `DocumentsReceived`
  - Armar → `Assembling`
  - Declarar técnicamente completo → `ReadyForSubmission`
  - Aprobar internamente → `POST /approve-for-submission`
  - Registrar sometimiento → `POST /submit`
  - Registrar observación → `POST /observations`
  - Registrar aprobación MINSA/CSS → `POST /approve` (CT/RS prompt)
- **Requirements list:** Marcar recibido (prep), Aceptar/Rechazar (review)
- **Observations list:** Responder

**REGUTRACK mapping:** Cols 16–17, 18–39, 40–54, workflow dates, observations col 48.

---

### 3.6 Registrations (`showRegistrations`)

**API calls:** `GET /registrations`

**Displays:**

- Table: registration number, status, issued on, expires on, days remaining

**REGUTRACK mapping:** Cols 12–13, 55.

---

### 3.7 Manufacturers (`showManufacturers`)

**API calls:** `GET /manufacturers`, `GET /manufacturer-certificates`, `POST` both

**Displays:**

- Manufacturer list (legal name, country)
- Certificate list (type, number, status)
- **Alta fabricante** (+ default ISO cert) if `mfrManage`

**REGUTRACK mapping:** DOCUMENTACION sheet.

---

### 3.8 Licenses (`showLicenses`)

**API calls:** `GET /operating-licenses`, `POST /operating-licenses`

**Displays:**

- Table: company, type, number, expires, status
- **Nueva licencia** if `licenseManage`

**REGUTRACK mapping:** CTT LICENCIAS OP.

---

### 3.9 Alerts (`showAlerts`)

**API calls:** `GET /alerts/evaluate`

**Displays:**

- List of alert type + message (CT/RS expiry, cert expiry, max reception, etc.)

**REGUTRACK mapping:** Col 44, DOCUMENTACION VIGENTE, license expiry.

---

### 3.10 Import (`showImport`)

**API calls:** `GET /imports`, `POST /imports/xlsx` (multipart)

**Displays:**

- File picker (.xlsx/.xlsm)
- **Stage XLSX** button
- Import job history list

**Requires:** `REGULATORY.CONFIGURE`

**REGUTRACK mapping:** Full workbook migration.

---

### 3.11 Config (`showConfig`)

**API calls:** `POST /bootstrap`

**Displays:**

- Bootstrap regulatorio button
- JSON output: authorities MINSA/CSS, pack seed result

**Requires:** `REGULATORY.CONFIGURE`

---

### 3.12 SoD Settings (`showSod`)

**API calls:** `GET /sod-settings`

**Displays:**

- JSON policy dump
- Note: changes via `PUT /sod-settings` (API only in v1 UI)

**Requires:** `REGULATORY.SOD.MANAGE` or `CONFIGURE`

---

## 4. Workflow states (`STATUS_LABELS`)

| Status | Spanish label |
|--------|---------------|
| Draft | Borrador |
| Planning | Planificación / Preparación |
| WaitingManufacturerDocuments | Espera docs fábrica |
| DocumentsReceived | Docs recibidos |
| Assembling | Armando expediente |
| ReadyForSubmission | Técnicamente completo |
| ApprovedForSubmission | Aprobado internamente |
| Submitted | Sometido |
| UnderAuthorityReview | En revisión autoridad |
| Observed | Observado |
| CorrectingObservation | Corrigiendo observación |
| Resubmitted | Resometido |
| Approved | Aprobación MINSA/CSS |
| Rejected | Rechazo externo |
| Cancelled | Cancelado |
| Closed | Cerrado |
| OnHold | En espera |

---

## 5. Navigation visibility rules (`visibleViews`)

| View | Hidden when |
|------|-------------|
| `import`, `config` | No `CONFIGURE` |
| `sod` | No `SOD.MANAGE` and no `CONFIGURE` |
| `manufacturers` | No mfr read/manage and not viewer/manager profile |
| `licenses` | No license read/manage and not viewer/manager |
| Others | Generally visible to all RA roles with module access |

---

## 6. Backend modules (reference)

| Module | Domain entities |
|--------|-----------------|
| Products | `MedicalDeviceProduct` |
| Dossiers | `RegistrationDossier`, `DossierRequirement`, `AuthorityObservation` |
| Registrations | `SanitaryRegistration` |
| Manufacturers | `ManufacturerProfile`, `ManufacturerCertificate` |
| Licenses | `OperatingLicense`, `LicenseRenewalCase` |
| Imports | `RegutrackImportJob`, `RegutrackImportRow` |
| Alerts | `RegulatoryAlertLog` |
| Config | `RegulatoryAuthority`, `RegulatoryRequirementPack` |

---

## 7. Certification notes

- This inventory is the **baseline** for `10_SCREEN_ACTION_COVERAGE_MATRIX.md`.
- Each view must be opened under each `@cert.local` role to verify visibility and read/write behavior.
- Dossier detail is the highest-risk surface (workflow + SoD + checklist gate on submit).

---

## 8. Document control

| Version | Date | Author | Change |
|---------|------|--------|--------|
| 1.0 | 2026-07-14 | QA | Initial inventory from regulatory-affairs.js |
