"""Generate final functional certification planning docs and test case catalogs."""
from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parent
TC_DIR = ROOT / "test-cases"

REGUTRACK_22 = [
    ("LEGAL_ID", "Copia cédula/pasaporte representante legal", True),
    ("OPS_LICENSE", "Copia licencia de operaciones", True),
    ("PUBLIC_REGISTRY", "Certificado registro público", True),
    ("OFFEROR_CERT", "Certificado de oferente", True),
    ("TECH_SHEET", "Ficha técnica", True),
    ("DEVICE_LITERATURE", "Literatura técnica del dispositivo médico", False),
    ("IFU", "Instructivo de uso / inserto", True),
    ("MFG_COMMITMENT", "Carta de compromiso del fabricante", True),
    ("ISO", "Certificado ISO", True),
    ("CLV_FDA", "Cert. Libre Venta (CLV) o FDA", True),
    ("PHOTOS", "Fotografías", False),
    ("LABELS", "Etiquetas del producto", True),
    ("STERILIZATION", "Método de esterilización", False),
    ("CLINICAL", "Resumen estudios o ensayos clínicos", False),
    ("MFG_PACKAGING", "Descripción manufactura y empaque", False),
    ("RISK_ANALYSIS", "Análisis de riesgo", False),
    ("TRACEABILITY", "Protocolo de trazabilidad", False),
    ("SAMPLES", "Muestras", False),
    ("OPS_MANUAL", "Manual de operación y/o mantenimiento", False),
    ("LOCAL_SUPPORT", "Certificación soporte técnico local", False),
    ("STORAGE_TRANSPORT", "Datos almacenamiento y transporte", False),
    ("ACCESSORIES", "Listado accesorios, repuestos y consumibles", False),
]

WORKFLOW_TRANSITIONS = [
    ("Draft", "Planning"),
    ("Draft", "Cancelled"),
    ("Planning", "WaitingManufacturerDocuments"),
    ("Planning", "OnHold"),
    ("Planning", "Cancelled"),
    ("WaitingManufacturerDocuments", "DocumentsReceived"),
    ("WaitingManufacturerDocuments", "OnHold"),
    ("WaitingManufacturerDocuments", "Cancelled"),
    ("DocumentsReceived", "Assembling"),
    ("Assembling", "ReadyForSubmission"),
    ("Assembling", "WaitingManufacturerDocuments"),
    ("ReadyForSubmission", "Submitted"),
    ("ReadyForSubmission", "Assembling"),
    ("Submitted", "UnderAuthorityReview"),
    ("UnderAuthorityReview", "Observed"),
    ("UnderAuthorityReview", "Approved"),
    ("UnderAuthorityReview", "Rejected"),
    ("Observed", "CorrectingObservation"),
    ("CorrectingObservation", "Resubmitted"),
    ("Resubmitted", "UnderAuthorityReview"),
    ("Approved", "Closed"),
    ("Rejected", "Closed"),
    ("OnHold", "Planning"),
    ("OnHold", "WaitingManufacturerDocuments"),
    ("OnHold", "Cancelled"),
]

SOD_ROLES = [
    "Regulatory Specialist",
    "Regulatory Reviewer",
    "Regulatory Approver",
    "Regulatory Submitter",
    "Regulatory Manager",
    "Regulatory Administrator",
    "Regulatory Viewer",
]


def tc_block(
    tc_id: str,
    *,
    requirement_id: str = "N/A",
    regutrack_ref: str = "N/A",
    module: str,
    feature: str,
    role: str,
    permission: str = "N/A",
    risk: str = "Medium",
    priority: str = "P1",
    objective: str,
    preconditions: str,
    test_data: str = "Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871",
    initial_state: str = "Bootstrap completed; SoD baseline GO",
    steps: list[str],
    expected_steps: list[str],
    expected_final: str,
    expected_db: str = "Domain state consistent with action",
    expected_audit: str = "Audit event recorded with actor and entity",
    expected_notification: str = "None unless specified in objective",
    negative_variant: str = "N/A",
    evidence: str = "Screenshot + API response + audit trail sample",
) -> str:
    step_lines = "\n".join(f"   {i}. {s}" for i, s in enumerate(steps, 1))
    exp_lines = "\n".join(f"   {i}. {e}" for i, e in enumerate(expected_steps, 1))
    return (
        f"### {tc_id}\n\n"
        f"| Field | Value |\n|-------|-------|\n"
        f"| **Test Case ID** | {tc_id} |\n"
        f"| **Requirement ID** | {requirement_id} |\n"
        f"| **REGUTRACK Reference** | {regutrack_ref} |\n"
        f"| **Module** | {module} |\n"
        f"| **Feature** | {feature} |\n"
        f"| **Role** | {role} |\n"
        f"| **Permission** | {permission} |\n"
        f"| **Risk** | {risk} |\n"
        f"| **Priority** | {priority} |\n"
        f"| **Objective** | {objective} |\n"
        f"| **Preconditions** | {preconditions} |\n"
        f"| **Test Data** | {test_data} |\n"
        f"| **Initial State** | {initial_state} |\n\n"
        f"**Detailed Steps:**\n{step_lines}\n\n"
        f"**Expected Result per Step:**\n{exp_lines}\n\n"
        f"| Field | Value |\n|-------|-------|\n"
        f"| **Expected Final Result** | {expected_final} |\n"
        f"| **Expected DB Effect** | {expected_db} |\n"
        f"| **Expected Audit Event** | {expected_audit} |\n"
        f"| **Expected Notification** | {expected_notification} |\n"
        f"| **Negative Variant** | {negative_variant} |\n"
        f"| **Evidence Required** | {evidence} |\n"
        f"| **Actual Result** | |\n"
        f"| **Status** | NOT EXECUTED |\n"
        f"| **Defect ID** | |\n"
        f"| **Retest Status** | |\n\n---\n\n"
    )


def write_file(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content.strip() + "\n", encoding="utf-8")


# ── Planning documents 11-20 ────────────────────────────────────────────────

def gen_planning_docs() -> None:
    docs = {
        "11_API_FUNCTIONAL_COVERAGE_MATRIX.md": gen_api_matrix(),
        "12_WORKFLOW_TRANSITION_COVERAGE.md": gen_workflow_coverage(),
        "13_BUSINESS_RULE_COVERAGE.md": gen_business_rules(),
        "14_NEGATIVE_TEST_STRATEGY.md": gen_negative_strategy(),
        "15_MULTITENANCY_TEST_STRATEGY.md": gen_multitenancy_strategy(),
        "16_AUDIT_AND_NOTIFICATION_TEST_STRATEGY.md": gen_audit_notification_strategy(),
        "17_IMPORT_MIGRATION_TEST_STRATEGY.md": gen_import_strategy(),
        "18_ENTRY_EXIT_CRITERIA.md": gen_entry_exit(),
        "19_MASTER_TEST_EXECUTION_PLAN.md": gen_execution_plan(),
        "20_DEFECT_MANAGEMENT_PROCESS.md": gen_defect_process(),
    }
    for name, content in docs.items():
        write_file(ROOT / name, content)


def gen_api_matrix() -> str:
    rows = [
        ("Auth", "POST /api/v1/auth/login", "Any", "200 + JWT", "AUTH-001"),
        ("Auth", "POST /api/v1/auth/logout", "Any authenticated", "204", "AUTH-005"),
        ("Regulatory", "POST .../regulatory/bootstrap", "Regulatory Administrator", "200 pack+authorities", "PACK-001"),
        ("Regulatory", "GET .../regulatory/dashboard", "Regulatory Viewer", "200 KPIs", "DASH-001"),
        ("Regulatory", "POST .../regulatory/products", "Regulatory Specialist", "201 product", "PROD-001"),
        ("Regulatory", "POST .../regulatory/dossiers", "Regulatory Specialist", "201 dossier+22 reqs", "DOS-001"),
        ("Regulatory", "PUT .../dossiers/{id}/requirements/{rid}", "Regulatory Specialist", "200 status update", "REQ-001"),
        ("Regulatory", "POST .../dossiers/{id}/submit", "Regulatory Submitter", "200 after gates", "SUB-001"),
        ("Regulatory", "POST .../dossiers/{id}/approve", "Regulatory Approver", "200 CT/RS", "REG-001"),
        ("Regulatory", "POST .../imports/xlsx", "Regulatory Administrator", "200 job staged", "IMP-001"),
        ("Regulatory", "POST .../imports/{id}/commit", "Regulatory Administrator", "200 entities", "IMP-010"),
        ("RBAC", "POST .../dossiers (wrong tenant token)", "Any", "403", "MT-001"),
        ("SoD", "POST approve by Specialist", "Regulatory Specialist", "403", "SOD-003"),
    ]
    table = "\n".join(
        f"| {a} | {b} | {c} | {d} | {e} | NOT EXECUTED |" for a, b, c, d, e in rows
    )
    return f"""# 11 — API Functional Coverage Matrix

**Program:** Final REGUTRACK Replacement Certification  
**Date:** 2026-07-14  
**Base:** `/api/v1` · Regulatory Affairs under `/tenants/{{tenantId}}/regulatory`  
**Status:** DESIGN — execution pending

---

## 1. Purpose

Map every business-critical API to at least one executable test case. No endpoint is COVERED until its linked TC is executed with browser + API + DB evidence.

---

## 2. Coverage rules

| Rule | Description |
|------|-------------|
| R1 | Each write endpoint: ≥1 positive + ≥1 negative (auth/SoD/validation) |
| R2 | Each read endpoint used in UI: ≥1 role-positive + cross-tenant negative |
| R3 | Workflow transitions: dedicated TC in `12_WORKFLOW_TRANSITION_COVERAGE.md` |
| R4 | Import pipeline: stage → validate → commit each have TC |
| R5 | Status = NOT EXECUTED until Fase 10 evidence attached |

---

## 3. Matrix (sample — full catalog in test-cases/)

| Area | Endpoint | Required Role | Expected | Test Case | Execution Status |
|------|----------|---------------|----------|-----------|------------------|
{table}

---

## 4. API groups under certification

1. **Auth** — login, logout, refresh, MFA, password  
2. **Tenants / TAC** — users, roles, RBAC grants (minimum for RA lab)  
3. **Regulatory Bootstrap** — authorities, requirement packs (22 items)  
4. **Products** — CRUD, search, commercializable flag  
5. **Manufacturers & Certificates** — CRUD, expiry refresh  
6. **Dossiers** — CRUD, transitions, requirements, dates, observations  
7. **Internal Approval / Submission** — SoD-gated actions  
8. **Registrations & Renewals** — CT/RS lifecycle  
9. **Operating Licenses** — CRUD, renewals, checklist  
10. **Import** — JSON/XLSX stage, commit, reconciliation  
11. **Dashboard / Alerts** — evaluate thresholds  
12. **Audit** — search tenant-scoped events  
13. **Notifications** — templates, delivery log  

---

## 5. Cross-reference

| Document | Relationship |
|----------|--------------|
| `test-cases/01_AUTHENTICATION_TEST_CASES.md` | Auth APIs |
| `test-cases/04_PRODUCT_PORTFOLIO_TEST_CASES.md` | Product APIs |
| `test-cases/09_DOSSIER_CREATION_TEST_CASES.md` | Dossier APIs |
| `test-cases/25_REGUTRACK_IMPORT_TEST_CASES.md` | Import APIs |
| `test-cases/30_SOD_REGRESSION_TEST_CASES.md` | SoD denials |

---

*All linked test cases start at Status = NOT EXECUTED.*
"""


def gen_workflow_coverage() -> str:
    rows = "\n".join(
        f"| {f} → {t} | Allowed | Specialist/Manager | WORKFLOW-{i:03d} | NOT EXECUTED |"
        for i, (f, t) in enumerate(WORKFLOW_TRANSITIONS, 1)
    )
    illegal = [
        ("Planning", "Approved"),
        ("Draft", "Submitted"),
        ("Assembling", "Submitted"),
        ("Approved", "Submitted"),
    ]
    illegal_rows = "\n".join(
        f"| {f} → {t} | Denied | Any | WORKFLOW-NEG-{i:03d} | NOT EXECUTED |"
        for i, (f, t) in enumerate(illegal, 1)
    )
    return f"""# 12 — Workflow Transition Coverage

**Program:** Final REGUTRACK Replacement Certification  
**Domain:** Registration Dossier — 16 states  
**Date:** 2026-07-14  
**Status:** DESIGN

---

## 1. State machine (authoritative)

```
Draft → Planning | Cancelled
Planning → WaitingManufacturerDocuments | OnHold | Cancelled
WaitingManufacturerDocuments → DocumentsReceived | OnHold | Cancelled
DocumentsReceived → Assembling
Assembling → ReadyForSubmission | WaitingManufacturerDocuments
ReadyForSubmission → Submitted | Assembling
Submitted → UnderAuthorityReview
UnderAuthorityReview → Observed | Approved | Rejected
Observed → CorrectingObservation
CorrectingObservation → Resubmitted
Resubmitted → UnderAuthorityReview
Approved → Closed
Rejected → Closed
OnHold → Planning | WaitingManufacturerDocuments | Cancelled
```

---

## 2. Allowed transitions — test coverage

| Transition | Gate | Role | Test Case | Status |
|------------|------|------|-----------|--------|
{rows}

---

## 3. Illegal transitions — negative coverage

| Transition | Expected | Role | Test Case | Status |
|------------|----------|------|-----------|--------|
{illegal_rows}

---

## 4. Business gates coupled to workflow

| Gate | Rule | Test Cases |
|------|------|------------|
| Submit | All critical requirements Accepted | SUB-002, REQ-022 |
| DocumentsReceived | Waiver ≥8 chars if no evidence | REQ-015 |
| Internal Approval | Reviewer complete + Approver SoD | IAP-001, SOD-008 |
| External Approve | Only from UnderAuthorityReview | REG-003 |
| Specialist | Cannot approve external CT/RS | SOD-003 |

---

## 5. SoD multi-role journey mapping

The full journey in `35_FULL_BUSINESS_JOURNEY_TEST_CASES.md` must traverse all critical transitions with **distinct users** (no multi-role single user).

---

*Execution status for all transitions: NOT EXECUTED.*
"""


def gen_business_rules() -> str:
    crit_rows = "\n".join(
        f"| {code} | {name} | Critical | Accept before submit | REQ-{i:03d} | NOT EXECUTED |"
        for i, (code, name, crit) in enumerate(REGUTRACK_22, 1) if crit
    )
    noncrit = "\n".join(
        f"| {code} | {name} | Non-critical | Track status | REQ-{i:03d} | NOT EXECUTED |"
        for i, (code, name, crit) in enumerate(REGUTRACK_22, 1) if not crit
    )
    return f"""# 13 — Business Rule Coverage

**Program:** Final REGUTRACK Replacement Certification  
**Contract:** `REGUTRACK 02JUN26 MG.xlsx` + `REGUTRACK-PA-DEFAULT` (22 requirements)  
**Date:** 2026-07-14

---

## 1. Critical business rules

| ID | Rule | Enforcement | Test Case | Status |
|----|------|-------------|-----------|--------|
| BR-001 | Submit blocked if any critical requirement not Accepted | API + UI | REQ-022 | NOT EXECUTED |
| BR-002 | Specialist cannot approve for submission | SoD policy | SOD-003 | NOT EXECUTED |
| BR-003 | Creator cannot self-review requirements | PreventSelfReview | SOD-001 | NOT EXECUTED |
| BR-004 | Approver cannot submit | SeparateApproverAndSubmitter | SOD-004 | NOT EXECUTED |
| BR-005 | Active registration requires CT/RS number | Domain validation | REG-005 | NOT EXECUTED |
| BR-006 | Expiration date must be after issued date | Domain validation | REG-006 | NOT EXECUTED |
| BR-007 | Pack snapshot frozen at dossier creation | DB immutability | PACK-008 | NOT EXECUTED |
| BR-008 | Cross-tenant access returns 403 | Tenant filter | MT-001 | NOT EXECUTED |
| BR-009 | Import does not destroy data without report | Import service | IMP-012 | NOT EXECUTED |
| BR-010 | Dashboard KPI = API = DB calculation | Reconciliation | DASH-010 | NOT EXECUTED |

---

## 2. REGUTRACK 22-requirement checklist coverage

### 2.1 Critical requirements (must gate submit)

| Code | Name | Criticality | Rule | Test Case | Status |
|------|------|-------------|------|-----------|--------|
{crit_rows}

### 2.2 Non-critical requirements

| Code | Name | Criticality | Rule | Test Case | Status |
|------|------|-------------|------|-----------|--------|
{noncrit}

---

## 3. Excel column rules (selected)

| Excel Column | Business Rule | Test Case |
|--------------|---------------|-----------|
| Criterio Técnico/ Registro Sanitario No. | Maps to SanitaryRegistration.Number | REG-001 |
| Fecha Criterio /Registro | Maps to IssuedOn | MIL-010 |
| Clase de Riesgo | Product.RiskClass A/B/C | PROD-005 |
| Ficha Tecnica | Links TECH_SHEET requirement | REQ-005 |
| Entidad Emisora | Authority MINSA/CSS | AUTH-002 |

---

*All business rule test cases: NOT EXECUTED.*
"""


def gen_negative_strategy() -> str:
    return """# 14 — Negative Test Strategy

**Program:** Final REGUTRACK Replacement Certification  
**Date:** 2026-07-14

---

## 1. Objectives

Prove the system **fails safely** under invalid input, unauthorized access, workflow bypass, and adversarial manipulation — without corrupting dossier integrity or leaking tenant data.

---

## 2. Negative test categories

| Category | Scope | Primary File | Min Cases |
|----------|-------|--------------|-----------|
| Auth denial | Invalid credentials, stale token, wrong tenant | `01_AUTHENTICATION_TEST_CASES.md` | 8 |
| RBAC denial | Write without permission | `29_RBAC_NEGATIVE_TEST_CASES.md` | 14 |
| SoD denial | Self-approve, creator review, submitter approve | `30_SOD_REGRESSION_TEST_CASES.md` | 16 |
| Workflow bypass | Illegal transitions, API out-of-order | `12_WORKFLOW_TRANSITION_COVERAGE.md` | 4+ |
| Validation | Empty, null, GUID invalid, dates inverted | All domain files | Per endpoint |
| Multitenancy | Cross-tenant ID in URL/body | `28_MULTITENANCY_TEST_CASES.md` | 14 |
| Import abuse | Corrupt XLSX, missing sheet, bad mapping | `25_REGUTRACK_IMPORT_TEST_CASES.md` | 6 |
| Concurrency | Double-click, parallel edit | `34_CONCURRENCY_AND_IDEMPOTENCY_TEST_CASES.md` | 10 |

---

## 3. Adversarial inputs (mandatory sample)

- Unicode product names (ñ, 中文, emoji in lab only)
- HTML/script strings in text fields
- SQL-like strings (`'; DROP TABLE--`)
- 10,000+ character comments
- Empty file upload
- .exe renamed to .pdf
- GUID from tenant B used in tenant A API call

---

## 4. Expected negative behavior

| Situation | Expected |
|-----------|----------|
| 403 Forbidden | No state change; audit SoD.Denied if applicable |
| 400 Validation | Clear error message; no partial DB write |
| 404 Not Found | No leak of existence across tenants |
| Double submit | Idempotent or second call rejected |

---

## 5. Evidence per negative TC

- API status code + body
- DB unchanged verification (before/after query)
- Audit event if security-relevant
- UI error message screenshot

---

## 6. Stop rules

- P0 negative failure (e.g., cross-tenant 200) → STOP certification
- SoD bypass → STOP, re-run SoD baseline

---

*All negative test cases: Status = NOT EXECUTED.*
"""


def gen_multitenancy_strategy() -> str:
    return """# 15 — Multitenancy Test Strategy

**Program:** Final REGUTRACK Replacement Certification  
**Lab tenants:** `ddcaf211-afe0-44a0-9c90-4fbda8fc4871` (Tenant A) + isolated Tenant B  
**Date:** 2026-07-14

---

## 1. Scope

Every Regulatory Affairs entity must be isolated by `TenantId`. Testing is **not** limited to dossiers — all RA artifacts are in scope.

---

## 2. Entities under cross-tenant test

| Entity | UI Test | API Test | Direct URL |
|--------|---------|----------|------------|
| Products | Yes | Yes | Yes |
| Manufacturers | Yes | Yes | Yes |
| Manufacturer Certificates | Yes | Yes | Yes |
| Requirement Packs | Yes | Yes | N/A |
| Dossiers | Yes | Yes | Yes |
| Dossier Requirements | Yes | Yes | Yes |
| Documents / Storage | Yes | Yes | Yes |
| Observations | Yes | Yes | Yes |
| Sanitary Registrations | Yes | Yes | Yes |
| Operating Licenses | Yes | Yes | Yes |
| Import Jobs | Yes | Yes | N/A |
| Dashboard KPIs | Yes | Yes | N/A |
| Audit Trail | Yes | Yes | N/A |
| SoD Settings | Yes | Yes | N/A |

---

## 3. Attack vectors

1. JWT tenant A → API resource ID tenant B → expect 403/404  
2. Browser session tenant A → hash route with dossier ID tenant B  
3. Search/filter must not return cross-tenant rows  
4. Export/report must not include foreign tenant data  
5. Platform admin must not read tenant business data without break-glass  

---

## 4. Test cases

Primary catalog: `test-cases/28_MULTITENANCY_TEST_CASES.md` (MT-001 … MT-014)

---

## 5. Pass criteria

- Zero cross-tenant data exposure in UI, API, or audit  
- 403/404 consistent; no stack traces with foreign IDs  

---

*All multitenancy TC: NOT EXECUTED.*
"""


def gen_audit_notification_strategy() -> str:
    return """# 16 — Audit and Notification Test Strategy

**Program:** Final REGUTRACK Replacement Certification  
**Date:** 2026-07-14

---

## 1. Audit strategy

### 1.1 Critical actions requiring audit proof

| Action | Expected Event Type | Test Case |
|--------|---------------------|-----------|
| Product created | Product.Created | PROD-001 |
| Dossier created | Dossier.Created | DOS-001 |
| Requirement status change | Requirement.Updated | REQ-001 |
| Internal approval | Dossier.InternalApproval | IAP-001 |
| Submission | Dossier.Submitted | SUB-001 |
| Observation opened | Observation.Created | OBS-001 |
| External approval / CT | Registration.Approved | REG-001 |
| Import committed | Import.Committed | IMP-010 |
| SoD denied | SoD.Denied | SOD-001 |

### 1.2 Audit quality rules

- Human-readable description (not only "Entity Updated")
- Actor user ID + role context
- Tenant ID scoped
- Timestamp UTC
- Before/after values for critical fields where supported

---

## 2. Notification strategy

### 2.1 Event → recipient matrix

| Trigger | Recipient | Channel | Test Case |
|---------|-----------|---------|-----------|
| Review requested | Regulatory Reviewer | In-app/Email | NOTIF-001 |
| Returned for correction | Regulatory Specialist | In-app/Email | NOTIF-002 |
| Internal approval pending | Regulatory Approver | In-app/Email | NOTIF-003 |
| Ready for submission | Regulatory Submitter | In-app/Email | NOTIF-004 |
| Authority observation | Dossier owner + Specialist | In-app/Email | NOTIF-005 |
| Registration expiring | Regulatory Manager | In-app/Email | NOTIF-006 |
| Certificate expiring | Regulatory Specialist | In-app/Email | NOTIF-007 |

### 2.2 Negative notification rules

- Viewer must NOT receive operational task assignments
- No broadcast to all users
- No cross-tenant recipient

---

## 3. Test catalogs

- `test-cases/27_AUDIT_TEST_CASES.md`
- `test-cases/23_NOTIFICATION_TEST_CASES.md`

---

*All audit/notification TC: NOT EXECUTED.*
"""


def gen_import_strategy() -> str:
    return """# 17 — Import Migration Test Strategy

**Program:** Final REGUTRACK Replacement Certification  
**Contract file:** `REGUTRACK 02JUN26 MG.xlsx` (copy only — never modify original)  
**Date:** 2026-07-14

---

## 1. Purpose

Prove Compliance 360 can ingest historical REGUTRACK data for migration while daily operations no longer require Excel.

---

## 2. Import pipeline stages (each needs TC)

| Stage | API | Validation |
|-------|-----|------------|
| 1. Upload | POST `/imports/xlsx` | File received, job created |
| 2. Sheet detection | Job metadata | CTT REGISTROS, TUBERIA, DOCUMENTACION, LICENCIAS OP |
| 3. Preview / mapping | GET job rows | Column mapping matches decomposition |
| 4. Normalization | Row validation | Dates, enums, authority codes |
| 5. Duplicate detection | Row warnings | Same catalog code + authority |
| 6. Simulation | Dry-run counts | No commit |
| 7. Commit | POST `.../commit?maxRows=` | Entities created |
| 8. Reconciliation report | Export/log | Row → domain object trace |

---

## 3. Reconciliation format (mandatory)

| Sheet | Row | Source Identity | Expected Objects | Created/Matched | Warnings | Errors | Status |
|-------|-----|-----------------|------------------|-----------------|----------|--------|--------|

---

## 4. Negative import scenarios

- Corrupt XLSX
- Missing sheet
- Renamed column header
- Invalid date in Fecha Criterio
- Repeat import (idempotency)
- Cancel mid-commit
- Rollback if supported (or FAIL + WAIVER)

---

## 5. Test catalog

`test-cases/25_REGUTRACK_IMPORT_TEST_CASES.md` — IMP-001 … IMP-016

---

## 6. GO dependency

Import PASS is **required** for GO RETIRE EXCEL per `18_ENTRY_EXIT_CRITERIA.md` criterion G10.

---

*All import TC: NOT EXECUTED.*
"""


def gen_entry_exit() -> str:
    return """# 18 — Entry & Exit Criteria

**Program:** Final REGUTRACK Replacement Certification  
**Version:** 1.0  
**Date:** 2026-07-14

---

## 1. Entry criteria (start execution)

| # | Criterion | Evidence | Status |
|---|-----------|----------|--------|
| E1 | Planning docs `00`–`20` complete | This folder | COMPLETE |
| E2 | All 35 test-case catalogs generated | `test-cases/` | COMPLETE |
| E3 | SoD baseline smoke PASS | `24_FINAL_ROLE_AND_SOD_CERTIFICATION.md` | REQUIRED AT RUN |
| E4 | Lab environment UP `/health` 200 | Log | PENDING |
| E5 | Bootstrap + 22-requirement pack seeded | API | PENDING |
| E6 | Test users per role (distinct users for journey) | `07_TEST_USER_MATRIX.md` | PENDING |
| E7 | REGUTRACK Excel copy available (hash logged) | `06_TEST_DATA_STRATEGY.md` | PENDING |
| E8 | Second tenant for isolation tests | TAC | PENDING |
| E9 | QA Manager + PO sign-off | Below | PENDING |

**Rule:** Do not mark any functional TC PASS until E1–E9 satisfied at execution time.

---

## 2. Exit — GO RETIRE REGUTRACK

| # | Criterion |
|---|----------|
| G1 | Operator does not need Excel for daily work |
| G2 | All operational sheets represented; no critical PARTIAL without WAIVER |
| G3 | Critical Excel columns validated UI/API |
| G4 | Full multi-role journey PASS (`35_FULL_BUSINESS_JOURNEY_TEST_CASES.md`) |
| G5 | 22-requirement checklist fully exercised (`10_DOSSIER_REQUIREMENT_TEST_CASES.md`) |
| G6 | CT/RS + renewals PASS |
| G7 | Operating licenses PASS |
| G8 | Import reconciliation PASS (agreed volume) |
| G9 | Dashboard KPI = API = DB |
| G10 | Multitenancy PASS |
| G11 | SoD regression PASS |
| G12 | Critical defects = 0; High defects = 0 |
| G13 | `95_FINAL_REGUTRACK_REPLACEMENT_CERTIFICATE.md` issued |

---

## 3. Immediate NO GO

- Any P0 FAIL unmitigated
- SoD bypass
- Cross-tenant leak
- Submit with pending criticals succeeds
- Import destroys data without report
- SKIPPED used to avoid lab setup

---

## 4. Signatures

| Role | Name | Approved | Date |
|------|------|----------|------|
| QA Manager | | Pending | |
| Product Owner | | Pending | |
| Regulatory Affairs Specialist | | Pending | |

---

*Certification execution blocked until Entry Gate approved at run time.*
"""


def gen_execution_plan() -> str:
    return """# 19 — Master Test Execution Plan

**Program:** Final REGUTRACK Replacement Certification  
**Date:** 2026-07-14  
**Total test-case files:** 35  
**Status:** READY FOR EXECUTION (after Entry Gate)

---

## 1. Pre-execution gates

| Gate | Command / Action | Expected |
|------|------------------|----------|
| G1 | `dotnet build -c Release` | 0 errors |
| G2 | `dotnet test -c Release` | All pass |
| G3 | SoD smoke suite | GO |
| G4 | `GET /health` | 200 |
| G5 | Bootstrap regulatory tenant | 22 pack items |

---

## 2. Execution waves

| Wave | Domain | Files | Depends on |
|------|--------|-------|------------|
| W0 | SoD regression | `30_SOD_REGRESSION_TEST_CASES.md` | G1–G5 |
| W1 | Auth + IAM | `01`, `02` | W0 |
| W2 | Config + packs | `07`, `08` | W1 |
| W3 | Portfolio + manufacturers | `04`, `05`, `06` | W2 |
| W4 | Dossier + requirements + docs | `09`, `10`, `11` | W3 |
| W5 | Milestones + review + approval | `12`, `13`, `14` | W4 |
| W6 | Submission + observations | `15`, `16`, `17`, `18` | W5 |
| W7 | CT/RS + renewal + licenses | `19`, `20`, `24` | W6 |
| W8 | Pipeline + dashboard + alerts | `03`, `21`, `22` | W7 |
| W9 | Import migration | `25` | W3 (parallel ok) |
| W10 | Audit + notifications | `23`, `27` | W6 |
| W11 | Multitenancy + RBAC negative | `28`, `29` | W4 |
| W12 | UI/UX + responsive + errors | `31`, `32`, `33` | W8 |
| W13 | Concurrency | `34` | W6 |
| W14 | Reporting | `26` | W8 |
| W15 | **Full business journey** | `35` | W0–W13 ALL PASS |

---

## 3. Full business journey (critical path)

Execute `35_FULL_BUSINESS_JOURNEY_TEST_CASES.md` with **separate browser profiles per role**:

1. Regulatory Specialist — create product, dossier, requirements  
2. Regulatory Reviewer — review, reject/accept  
3. Regulatory Specialist — correct  
4. Regulatory Reviewer — technical complete  
5. Regulatory Approver — internal approval  
6. Regulatory Submitter — submission  
7. Regulatory Manager — authority observation  
8. Specialist → Reviewer → Approver → Submitter — resubmission round 2  
9. Regulatory Approver — external CT/RS approval  
10. Validate registration, dashboard, pipeline, audit  

---

## 4. Evidence package per TC

- Screenshot (UI steps)
- API request/response (HAR or curl log)
- DB query snapshot (where applicable)
- Audit trail excerpt
- Notification log (if applicable)

Store under: `docs/final-functional-certification/evidence/{TC-ID}/`

---

## 5. Stop-on-fail

| Severity | Action |
|----------|--------|
| P0 / Critical | STOP wave → defect → fix → retest wave |
| P1 / High | Continue only if waiver approved; else STOP |
| P2+ | Log, continue, fix before GO |

---

## 6. Completion metrics

Track in `09_FUNCTIONAL_CERTIFICATION_REPORT.md`:

- Total TC designed
- Total executed
- PASS / FAIL / BLOCKED
- Coverage % by REGUTRACK sheet

---

*No test case may be marked PASS during design phase.*
"""


def gen_defect_process() -> str:
    return """# 20 — Defect Management Process

**Program:** Final REGUTRACK Replacement Certification  
**Date:** 2026-07-14

---

## 1. Defect lifecycle

```
Found → Logged (DEF-XXXX) → Triaged → Assigned → Fixed → Verified → Closed
                                    ↘ Waiver (signed) → Accepted Risk
```

---

## 2. Defect record location

`docs/final-functional-certification/defects/DEF-XXXX.md`

---

## 3. Required fields

| Field | Description |
|-------|-------------|
| ID | DEF-0001 sequential |
| Title | Short description |
| Severity | Critical / High / Medium / Low |
| Priority | P0–P3 |
| Module | RA domain |
| REGUTRACK impact | Sheet/column affected |
| Role | Actor during failure |
| Test Case ID | Link to failing TC |
| Steps to reproduce | Numbered |
| Expected vs Actual | Clear contrast |
| Evidence | Screenshot, API, DB, logs |
| Root cause | After analysis |
| Fix | PR/commit reference |
| Regression risk | Modules affected |
| Retest TCs | List |
| Status | Open / Fixed / Verified / Waived / Closed |

---

## 4. Severity definitions

| Severity | Definition | SLA |
|----------|------------|-----|
| **Critical** | Blocks REGUTRACK replacement, data loss, tenant leak, approval bypass | Fix immediately; STOP testing |
| **High** | Critical regulatory flow wrong/incomplete | Fix before GO |
| **Medium** | Important function with safe workaround | Fix or waiver before GO |
| **Low** | UX/cosmetic | Backlog |

---

## 5. Fix workflow

1. Register defect — do not hide  
2. Reproduce with evidence  
3. Root cause analysis  
4. Implement fix in repo  
5. `dotnet build` + targeted tests  
6. Re-run failing TC  
7. Re-run module regression  
8. Re-run SoD if RBAC/workflow touched  
9. Re-run journey if dossier flow touched  
10. Close only with execution evidence  

---

## 6. Waiver process

- Only PO + QA Manager can approve  
- Document in `WAIVERS.md` with expiry  
- Linked TC remains FAIL until waiver; GO may proceed if waiver covers gap  

---

## 7. Metrics

Track open Critical/High count daily during execution. **GO requires Critical=0, High=0.**

---

*Defect register starts empty at design phase.*
"""


# ── Test case file generators ───────────────────────────────────────────────

def gen_auth_cases() -> str:
    cases = []
    specs = [
        ("AUTH-001", "P0", "Tenant Administrator", "Valid login returns JWT and dashboard", ["Open login", "Enter valid tenant/email/password", "Submit"], ["Login form visible", "Fields accepted", "Redirect to dashboard; token in storage"]),
        ("AUTH-002", "P0", "Any", "Invalid password rejected", ["Enter valid email, wrong password", "Submit"], ["Form accepts input", "Error message; no token"]),
        ("AUTH-003", "P0", "Any", "Nonexistent user rejected", ["Enter unknown email", "Submit"], ["Form accepts", "401/403; no token"]),
        ("AUTH-004", "P0", "Tenant Administrator", "Logout clears session", ["Login valid", "Click logout", "Navigate back"], ["Session active", "Redirect login", "Protected route redirects login"]),
        ("AUTH-005", "P0", "Any", "API call without token returns 401", ["Call GET dashboard API no header"], ["401 Unauthorized"]),
        ("AUTH-006", "P0", "Regulatory Specialist", "Stale token after role change rejected", ["Login", "Admin changes role", "Retry API without re-login"], ["Token issued", "Role updated in DB", "403 until fresh login"]),
        ("AUTH-007", "P1", "Any", "Inactive user cannot login", ["Deactivate user in TAC", "Attempt login"], ["User inactive", "Login denied"]),
        ("AUTH-008", "P1", "Any", "Wrong tenant ID rejected", ["Login with valid user wrong tenantId"], ["Login fails; no cross-tenant access"]),
        ("AUTH-009", "P1", "Regulatory Specialist", "Two tabs: logout in one invalidates other", ["Login two tabs", "Logout tab A", "Refresh tab B"], ["Both active", "Tab A logged out", "Tab B requires login"]),
        ("AUTH-010", "P1", "Regulatory Viewer", "Protected regulatory route without auth redirects", ["Open #/regulatory logged out"], ["Redirect to login"]),
        ("AUTH-011", "P0", "Regulatory Specialist", "Token from tenant A cannot access tenant B API", ["Login tenant A", "Call API with tenant B in URL"], ["Token valid A", "403/404"]),
        ("AUTH-012", "P1", "Tenant Administrator", "Session timeout returns to login", ["Login", "Wait past timeout", "Perform action"], ["Active", "Session expired message", "Redirect login"]),
    ]
    for tc_id, pri, role, obj, steps, exp in specs:
        cases.append(tc_block(tc_id, module="Authentication", feature="Session", role=role, priority=pri, risk="High" if pri == "P0" else "Medium", objective=obj, preconditions="Lab app running; test user exists", steps=steps, expected_steps=exp, expected_final="Authentication behavior matches enterprise policy"))
    return header("01_AUTHENTICATION_TEST_CASES", "Authentication & Session", len(specs)) + "".join(cases)


def header(filename: str, title: str, count: int) -> str:
    num = filename.split("_")[0]
    return f"""# {num} — {title} Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `{filename}.md`  
**Cases:** {count}  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

"""


def gen_all_test_cases() -> dict[str, str]:
    generators: dict[str, callable] = {
        "01_AUTHENTICATION_TEST_CASES.md": gen_auth_cases,
        "02_TENANT_AND_IAM_TEST_CASES.md": lambda: gen_simple_domain("02_TENANT_AND_IAM", "IAM", "IAM", "Tenant Administrator", 10, "TAC users/roles/RBAC"),
        "03_REGULATORY_DASHBOARD_TEST_CASES.md": lambda: gen_simple_domain("03_REGULATORY_DASHBOARD", "DASH", "Dashboard", "Regulatory Viewer", 10, "KPI reconciliation"),
        "04_PRODUCT_PORTFOLIO_TEST_CASES.md": lambda: gen_product_cases(),
        "05_MANUFACTURER_TEST_CASES.md": lambda: gen_simple_domain("05_MANUFACTURER", "MFR", "Manufacturers", "Regulatory Specialist", 12, "DOCUMENTACION sheet"),
        "06_MANUFACTURER_DOCUMENT_TEST_CASES.md": lambda: gen_mfr_doc_cases(),
        "07_AUTHORITY_TEST_CASES.md": lambda: gen_simple_domain("07_AUTHORITY", "AUTHZ", "Authorities", "Regulatory Administrator", 8, "MINSA/CSS"),
        "08_REQUIREMENT_PACK_TEST_CASES.md": lambda: gen_pack_cases(),
        "09_DOSSIER_CREATION_TEST_CASES.md": lambda: gen_dossier_creation_cases(),
        "10_DOSSIER_REQUIREMENT_TEST_CASES.md": lambda: gen_requirement_cases(),
        "11_DOCUMENT_TEST_CASES.md": lambda: gen_simple_domain("11_DOCUMENT", "DOC", "Documents", "Regulatory Specialist", 12, "File upload/download"),
        "12_MILESTONE_AND_DATE_TEST_CASES.md": lambda: gen_milestone_cases(),
        "13_REVIEW_TEST_CASES.md": lambda: gen_simple_domain("13_REVIEW", "REV", "Review", "Regulatory Reviewer", 12, "Technical review queue"),
        "14_INTERNAL_APPROVAL_TEST_CASES.md": lambda: gen_simple_domain("14_INTERNAL_APPROVAL", "IAP", "Internal Approval", "Regulatory Approver", 12, "SoD internal gate"),
        "15_SUBMISSION_TEST_CASES.md": lambda: gen_simple_domain("15_SUBMISSION", "SUB", "Submission", "Regulatory Submitter", 10, "Authority submission"),
        "16_AUTHORITY_OBSERVATION_TEST_CASES.md": lambda: gen_simple_domain("16_AUTHORITY_OBSERVATION", "OBS", "Observations", "Regulatory Manager", 12, "Authority observations"),
        "17_OBSERVATION_RESPONSE_TEST_CASES.md": lambda: gen_simple_domain("17_OBSERVATION_RESPONSE", "OBSR", "Observation Response", "Regulatory Specialist", 10, "Response to authority"),
        "18_RESUBMISSION_TEST_CASES.md": lambda: gen_simple_domain("18_RESUBMISSION", "RESUB", "Resubmission", "Regulatory Submitter", 10, "Resometimiento"),
        "19_SANITARY_REGISTRATION_TEST_CASES.md": lambda: gen_simple_domain("19_SANITARY_REGISTRATION", "REG", "CT/RS", "Regulatory Approver", 12, "Sanitary registration"),
        "20_RENEWAL_TEST_CASES.md": lambda: gen_simple_domain("20_RENEWAL", "REN", "Renewal", "Regulatory Administrator", 12, "Product renewal dossier"),
        "21_PIPELINE_TEST_CASES.md": lambda: gen_simple_domain("21_PIPELINE", "PIPE", "Pipeline", "Regulatory Viewer", 12, "TUBERIA kanban"),
        "22_ALERT_TEST_CASES.md": lambda: gen_alert_cases(),
        "23_NOTIFICATION_TEST_CASES.md": lambda: gen_simple_domain("23_NOTIFICATION", "NOTIF", "Notifications", "Regulatory Specialist", 12, "Event notifications"),
        "24_OPERATING_LICENSE_TEST_CASES.md": lambda: gen_license_cases(),
        "25_REGUTRACK_IMPORT_TEST_CASES.md": lambda: gen_import_cases(),
        "26_REPORTING_TEST_CASES.md": lambda: gen_simple_domain("26_REPORTING", "RPT", "Reporting", "Regulatory Viewer", 10, "RA reports"),
        "27_AUDIT_TEST_CASES.md": lambda: gen_simple_domain("27_AUDIT", "AUD", "Audit Trail", "Regulatory Viewer", 12, "ALCOA+ audit"),
        "28_MULTITENANCY_TEST_CASES.md": lambda: gen_multitenancy_cases(),
        "29_RBAC_NEGATIVE_TEST_CASES.md": lambda: gen_rbac_negative_cases(),
        "30_SOD_REGRESSION_TEST_CASES.md": lambda: gen_sod_cases(),
        "31_UI_UX_TEST_CASES.md": lambda: gen_simple_domain("31_UI_UX", "UX", "UI/UX", "Regulatory Specialist", 12, "Human usability"),
        "32_RESPONSIVE_TEST_CASES.md": lambda: gen_simple_domain("32_RESPONSIVE", "RSP", "Responsive", "Regulatory Viewer", 10, "Layout breakpoints"),
        "33_ERROR_RECOVERY_TEST_CASES.md": lambda: gen_simple_domain("33_ERROR_RECOVERY", "ERR", "Error Recovery", "Regulatory Specialist", 12, "Graceful errors"),
        "34_CONCURRENCY_AND_IDEMPOTENCY_TEST_CASES.md": lambda: gen_simple_domain("34_CONCURRENCY_AND_IDEMPOTENCY", "CONC", "Concurrency", "Regulatory Specialist", 10, "Double-click/idempotency"),
        "35_FULL_BUSINESS_JOURNEY_TEST_CASES.md": lambda: gen_journey_cases(),
    }
    return {name: fn() for name, fn in generators.items()}


def gen_simple_domain(file_prefix: str, id_prefix: str, module: str, default_role: str, count: int, feature: str) -> str:
    num = file_prefix.split("_")[0]
    title = module
    cases = []
    for i in range(1, count + 1):
        tc_id = f"{id_prefix}-{i:03d}"
        pri = "P0" if i <= 3 else "P1"
        cases.append(tc_block(
            tc_id,
            module=module,
            feature=feature,
            role=default_role,
            priority=pri,
            risk="High" if pri == "P0" else "Medium",
            objective=f"Validate {feature} scenario {i} per REGUTRACK operational parity",
            preconditions="Lab bootstrapped; user logged in as " + default_role,
            steps=[f"Navigate to {module} view", f"Execute primary action for scenario {i}", "Verify API response", "Verify UI state", "Check audit trail"],
            expected_steps=[f"{module} view loads", "Action completes or correctly denied", "HTTP 2xx/4xx as expected", "UI reflects domain state", "Audit event if write action"],
            expected_final=f"Scenario {i} behavior matches specification",
            regutrack_ref="CTT REGISTROS / operational sheets",
        ))
    return header(file_prefix, title, count) + "".join(cases)


def gen_product_cases() -> str:
    cases = []
    fields = [
        ("PROD-001", "Create product class A", "Regulatory Specialist", "P0", "Clase A"),
        ("PROD-002", "Create product class B", "Regulatory Specialist", "P1", "Clase B"),
        ("PROD-003", "Create product class C", "Regulatory Specialist", "P1", "Clase C"),
        ("PROD-004", "Reject duplicate catalog code", "Regulatory Specialist", "P0", "Catálogo / Código"),
        ("PROD-005", "Map brand and regulatory name from Excel", "Regulatory Specialist", "P0", "Marca / Nombre CT"),
        ("PROD-006", "Link manufacturer and distributor", "Regulatory Specialist", "P0", "Fabricante / Distribuidor"),
        ("PROD-007", "Set initiative NEGOCIO BASE", "Regulatory Specialist", "P1", "Iniciativa"),
        ("PROD-008", "Opportunity amount and currency validation", "Regulatory Specialist", "P1", "Oportunidad"),
        ("PROD-009", "Search and filter portfolio", "Regulatory Viewer", "P1", "CTT REGISTROS list"),
        ("PROD-010", "Product not commercializable without CT", "Regulatory Specialist", "P0", "Comercializable"),
        ("PROD-011", "Reject empty brand", "Regulatory Specialist", "P0", "Validation"),
        ("PROD-012", "Cross-tenant product create denied", "Regulatory Specialist", "P0", "Tenant isolation"),
        ("PROD-013", "Unicode in product name", "Regulatory Specialist", "P2", "International text"),
        ("PROD-014", "Pagination on large portfolio", "Regulatory Viewer", "P1", "Scale"),
        ("PROD-015", "Deactivate/reactivate product if supported", "Regulatory Administrator", "P2", "Lifecycle"),
    ]
    for tc_id, obj, role, pri, ref in fields:
        cases.append(tc_block(tc_id, module="Product Portfolio", feature="CRUD", role=role, priority=pri, regutrack_ref=ref, objective=obj, preconditions="Bootstrap complete", steps=["Open portfolio", "Perform action", "Save", "Verify list/detail"], expected_steps=["View loads", "Form/API accepts valid data", "Persisted", "Visible in search"], expected_final=obj))
    return header("04_PRODUCT_PORTFOLIO", "Product Portfolio", len(fields)) + "".join(cases)


def gen_mfr_doc_cases() -> str:
    types = ["ISO13485", "CLV", "FDA", "CE", "Apostille", "Notarized"]
    cases = []
    for i, cert in enumerate(types, 1):
        cases.append(tc_block(f"MFRDOC-{i:03d}", module="Manufacturer Documents", feature=cert, role="Regulatory Specialist", priority="P0" if i <= 4 else "P1", regutrack_ref="DOCUMENTACION", objective=f"Add {cert} certificate with expiry tracking", preconditions="Manufacturer exists", steps=["Open manufacturer", "Add certificate", "Set dates and number", "Upload document", "Save"], expected_steps=["Manufacturer detail", "Form open", "Validation pass", "File stored", "Certificate listed"], expected_final=f"{cert} certificate tracked with alerts"))
    for i in range(7, 13):
        cases.append(tc_block(f"MFRDOC-{i:03d}", module="Manufacturer Documents", feature="Certificate lifecycle", role="Regulatory Specialist", priority="P1", objective=f"Certificate scenario {i} (reuse, replace, expiry)", preconditions="Certificates exist", steps=["List certificates", "Execute action", "Verify alert", "Verify dossier reuse"], expected_steps=["List loads", "Action OK", "Alert if expiring", "Linked dossiers unchanged"], expected_final="Document reuse rules honored"))
    return header("06_MANUFACTURER_DOCUMENT", "Manufacturer Document", 12) + "".join(cases)


def gen_pack_cases() -> str:
    cases = []
    pack_specs = [
        ("PACK-001", "Bootstrap creates REGUTRACK-PA-DEFAULT with 22 items", "Regulatory Administrator", "P0"),
        ("PACK-002", "List requirement pack returns 22 definitions", "Regulatory Viewer", "P0"),
        ("PACK-003", "Critical flag matches catalog for LEGAL_ID", "Regulatory Viewer", "P0"),
        ("PACK-004", "Dossier A snapshots pack version V1", "Regulatory Specialist", "P0"),
        ("PACK-005", "Publish pack V2 does not alter dossier A requirements", "Regulatory Administrator", "P0"),
        ("PACK-006", "Dossier B uses pack V2 after publish", "Regulatory Specialist", "P0"),
        ("PACK-007", "Requirement order preserved 1-22", "Regulatory Viewer", "P1"),
        ("PACK-008", "Category Legal/Technical/Manufacturer mapped", "Regulatory Viewer", "P1"),
        ("PACK-009", "Viewer cannot publish pack", "Regulatory Viewer", "P0"),
        ("PACK-010", "Specialist cannot configure packs", "Regulatory Specialist", "P0"),
        ("PACK-011", "Pack tied to PA country", "Regulatory Administrator", "P1"),
        ("PACK-012", "API GET requirement-packs tenant scoped", "Regulatory Administrator", "P0"),
        ("PACK-013", "All 22 codes present in API response", "Regulatory Viewer", "P0"),
        ("PACK-014", "IFU marked critical; PHOTOS non-critical", "Regulatory Viewer", "P1"),
    ]
    for tc_id, obj, role, pri in pack_specs:
        cases.append(tc_block(tc_id, module="Requirement Pack", feature="REGUTRACK-PA-DEFAULT", role=role, priority=pri, regutrack_ref="Checklist cols 18-39", objective=obj, preconditions="Bootstrap executed", steps=["Call API or open config", "Inspect pack", "Compare to RegutrackRequirementCatalog", "Verify counts"], expected_steps=["Access granted/denied correctly", "Pack visible", "22 items match codes", "Flags correct"], expected_final=obj))
    return header("08_REQUIREMENT_PACK", "Requirement Pack", len(pack_specs)) + "".join(cases)


def gen_dossier_creation_cases() -> str:
    types = ["NewRegistration", "Renewal", "Modification", "Extension", "ReRegistration"]
    cases = []
    for i, ptype in enumerate(types, 1):
        cases.append(tc_block(f"DOS-{i:03d}", module="Dossier", feature="Creation", role="Regulatory Specialist", priority="P0", regutrack_ref="CTT REGISTROS TUBERIA", objective=f"Create dossier type {ptype} with 22 requirements", preconditions="Product and authority exist", steps=["Open dossiers", "Create new", f"Select {ptype}", "Select product/authority", "Save"], expected_steps=["List loads", "Wizard open", "Type set", "Linked entities", "Dossier Draft with 22 reqs"], expected_final=f"{ptype} dossier created with frozen checklist"))
    extras = [
        ("DOS-006", "Reject duplicate active dossier same product+authority", "P0"),
        ("DOS-007", "Case number generated unique", "P1"),
        ("DOS-008", "Assign responsible specialist", "P1"),
        ("DOS-009", "Sales/marketing input fields persist", "P1"),
        ("DOS-010", "Opportunity amount on dossier", "P1"),
        ("DOS-011", "Viewer cannot create dossier", "P0"),
        ("DOS-012", "Reviewer cannot create dossier", "P0"),
        ("DOS-013", "Cross-tenant dossier denied", "P0"),
        ("DOS-014", "Audit DossierCreated event", "P0"),
        ("DOS-015", "Initial state Draft only", "P0"),
    ]
    for tc_id, obj, pri in extras:
        cases.append(tc_block(tc_id, module="Dossier", feature="Creation", role="Regulatory Specialist", priority=pri, objective=obj, preconditions="Lab data ready", steps=["Setup precondition", "Attempt action", "Verify API", "Verify audit"], expected_steps=["Precondition met", "Correct allow/deny", "Consistent state", "Audit if create"], expected_final=obj))
    return header("09_DOSSIER_CREATION", "Dossier Creation", 15) + "".join(cases)


def gen_requirement_cases() -> str:
    cases = []
    for i, (code, name, crit) in enumerate(REGUTRACK_22, 1):
        cases.append(tc_block(
            f"REQ-{i:03d}",
            requirement_id=code,
            regutrack_ref=f"Checklist: {name}",
            module="Dossier Requirements",
            feature=code,
            role="Regulatory Specialist",
            priority="P0" if crit else "P1",
            risk="Critical" if crit else "Medium",
            permission="REGULATORY.DOSSIER.MANAGE",
            objective=f"Manage requirement {code} through Received → Under Review → Accepted",
            preconditions=f"Dossier with 22-pack; requirement {code} Pending",
            steps=[f"Open requirement {code}", "Mark Received", "Upload/link document", "Request review", "Reviewer accepts", "Verify critical gate"],
            expected_steps=[f"{code} visible in checklist", "Status Received", "Document linked", "Review queued", "Status Accepted", "Critical counted for submit gate" if crit else "Non-critical tracked"],
            expected_final=f"{code} ({name}) fully exercised",
            expected_audit=f"Requirement.{code}.Updated",
        ))
    extras = [
        ("REQ-023", "Submit blocked with pending critical", "P0", "Negative"),
        ("REQ-024", "Waive non-critical with evidence path", "P1", "Waiver"),
        ("REQ-025", "Reject requirement with notes", "P1", "Review"),
    ]
    for tc_id, obj, pri, feat in extras:
        cases.append(tc_block(tc_id, module="Dossier Requirements", feature=feat, role="Regulatory Reviewer" if "Reject" in obj else "Regulatory Specialist", priority=pri, objective=obj, preconditions="Dossier in Assembling", steps=["Prepare dossier state", "Execute gate action", "Attempt submit", "Verify block/allow"], expected_steps=["State set", "Action recorded", "Submit 400 if critical pending", "Clear message"], expected_final=obj))
    return header("10_DOSSIER_REQUIREMENT", "Dossier Requirement (22-pack)", 25) + "".join(cases)


def gen_milestone_cases() -> str:
    dates = [
        "RequestedFromFactory", "EstimatedReception", "MaximumReception", "Received",
        "Assembled", "EstimatedSubmission", "Submitted", "ObservationReceived",
        "EstimatedApproval", "Approved", "TargetExpiration", "Renewal",
    ]
    cases = []
    for i, d in enumerate(dates, 1):
        cases.append(tc_block(f"MIL-{i:03d}", module="Milestones", feature=d, role="Regulatory Specialist", priority="P0" if i <= 6 else "P1", regutrack_ref="CTT REGISTROS date columns", objective=f"Set and audit date {d}", preconditions="Dossier exists", steps=["Open dossier dates", f"Set {d}", "Save", "Verify timeline", "Trigger alert if applicable"], expected_steps=["Dates panel", "Value accepted", "Persisted", "Timeline updated", "Alert scheduled if overdue rule"], expected_final=f"{d} matches REGUTRACK semantics"))
    cases.append(tc_block("MIL-013", module="Milestones", feature="Validation", role="Regulatory Specialist", priority="P0", objective="Reject expiration before issued date", preconditions="Dossier with issued date", steps=["Set Approved date", "Set TargetExpiration earlier", "Save"], expected_steps=["Dates set", "Validation error", "No save"], expected_final="Date order enforced"))
    cases.append(tc_block("MIL-014", module="Milestones", feature="Timezone", role="Regulatory Specialist", priority="P2", objective="Dates stored UTC displayed local", preconditions="Dossier exists", steps=["Set date", "Save", "Reload", "Compare API"], expected_steps=["Input accepted", "Saved", "Same calendar day shown", "API ISO8601"], expected_final="Timezone consistent"))
    return header("12_MILESTONE_AND_DATE", "Milestone and Date", 14) + "".join(cases)


def gen_alert_cases() -> str:
    thresholds = ["365", "180", "120", "90", "60", "30", "7", "0"]
    cases = []
    for i, t in enumerate(thresholds, 1):
        cases.append(tc_block(f"ALERT-{i:03d}", module="Alerts", feature=f"Expiry {t}d", role="Regulatory Administrator", priority="P0" if int(t) <= 90 else "P1", objective=f"Alert fires at {t} days before registration expiry", preconditions="Registration with controlled expiry date", steps=["Set expiry in lab", "Run evaluate alerts", "Inspect alert list", "Verify recipient"], expected_steps=["Date set", "Job runs", f"Alert for {t}d present", "Correct owner"], expected_final=f"{t}-day threshold works"))
    for i, (feat, obj) in enumerate([
        ("Cert expiry", "Manufacturer certificate expiry alert"),
        ("License expiry", "Operating license expiry alert"),
        ("Max reception overdue", "Maximum reception date overdue"),
        ("Stuck dossier", "Dossier inactivity >14 days"),
        ("Critical pending", "Critical requirement pending alert"),
    ], 9):
        cases.append(tc_block(f"ALERT-{i:03d}", module="Alerts", feature=feat, role="Regulatory Manager", priority="P1", objective=obj, preconditions="Seed data", steps=["Configure threshold", "Evaluate", "Verify"], expected_steps=["Config ok", "Alerts returned", "No duplicates"], expected_final=obj))
    return header("22_ALERT", "Alert", 14) + "".join(cases)


def gen_license_cases() -> str:
    cases = []
    companies = ["Multimed", "4 Hospital", "Alimentos Premium"]
    for i, co in enumerate(companies, 1):
        cases.append(tc_block(f"LIC-{i:03d}", module="Operating License", feature="Create", role="Regulatory Administrator", priority="P0", regutrack_ref="CTT LICENCIAS OP", objective=f"Create operating license for {co}", preconditions="Bootstrap complete", steps=["Open licenses", "Create", f"Set company {co}", "Set authority/number/dates", "Save"], expected_steps=["View loads", "Form open", "Company set", "Validation pass", "License Active"], expected_final=f"{co} license tracked"))
    extras = [
        ("LIC-004", "Renewal seeds LicenseOpRequirementCatalog", "P0"),
        ("LIC-005", "Manual task: update FADDI platform noted", "P1"),
        ("LIC-006", "Manual task: attach comprobante", "P1"),
        ("LIC-007", "Expiry alert 90 days", "P0"),
        ("LIC-008", "List filters by authority", "P1"),
        ("LIC-009", "Cross-tenant license denied", "P0"),
        ("LIC-010", "Audit license created", "P1"),
        ("LIC-011", "Renewal workflow checklist", "P0"),
        ("LIC-012", "Comments and next action fields", "P1"),
        ("LIC-013", "External approval on license renewal", "P0"),
        ("LIC-014", "Representative companies from REGUTRACK sample", "P1"),
    ]
    for j, (tc_id, obj, pri) in enumerate(extras, 4):
        cases.append(tc_block(tc_id, module="Operating License", feature="Lifecycle", role="Regulatory Administrator", priority=pri, regutrack_ref="CTT LICENCIAS OP", objective=obj, preconditions="License exists", steps=["Open license", "Execute action", "Verify state", "Check audit"], expected_steps=["Detail loads", "Action OK", "State correct", "Audit recorded"], expected_final=obj))
    return header("24_OPERATING_LICENSE", "Operating License", 14) + "".join(cases)


def gen_import_cases() -> str:
    steps = [
        ("IMP-001", "Stage JSON valid simulated rows", "P0"),
        ("IMP-002", "Stage XLSX copy of REGUTRACK 02JUN26 MG.xlsx", "P0"),
        ("IMP-003", "Detect sheets CTT REGISTROS TUBERIA DOCUMENTACION LICENCIAS", "P0"),
        ("IMP-004", "Preview first 10 rows mapping", "P0"),
        ("IMP-005", "Validation error missing regulatoryName", "P0"),
        ("IMP-006", "Duplicate catalog code warning", "P1"),
        ("IMP-007", "Simulate without commit", "P1"),
        ("IMP-008", "Commit maxRows=50 smoke", "P0"),
        ("IMP-009", "Reconciliation row to product", "P0"),
        ("IMP-010", "Reconciliation row to dossier", "P0"),
        ("IMP-011", "Reconciliation manufacturer from DOCUMENTACION", "P0"),
        ("IMP-012", "Reconciliation license from LICENCIAS OP", "P0"),
        ("IMP-013", "Repeat import idempotency", "P1"),
        ("IMP-014", "Corrupt file rejected", "P0"),
        ("IMP-015", "Missing sheet error report", "P0"),
        ("IMP-016", "Cancel job before commit", "P1"),
    ]
    cases = []
    for tc_id, obj, pri in steps:
        cases.append(tc_block(tc_id, module="REGUTRACK Import", feature="Migration", role="Regulatory Administrator", priority=pri, regutrack_ref="REGUTRACK 02JUN26 MG.xlsx", objective=obj, preconditions="Excel copy in lab; import API up", steps=["Upload file", "Review job", "Validate mapping", "Commit or abort", "Export reconciliation"], expected_steps=["Job created", "Sheets detected", "Errors/warnings listed", "Entities match or error clear", "Report stored"], expected_final=obj))
    return header("25_REGUTRACK_IMPORT", "REGUTRACK Import", 16) + "".join(cases)


def gen_multitenancy_cases() -> str:
    entities = ["products", "manufacturers", "certificates", "dossiers", "requirements", "documents", "registrations", "licenses", "imports", "dashboard", "pipeline", "alerts", "audit", "packs"]
    cases = []
    for i, ent in enumerate(entities, 1):
        cases.append(tc_block(f"MT-{i:03d}", module="Multitenancy", feature=ent, role="Regulatory Specialist", priority="P0", risk="Critical", objective=f"Tenant A cannot read/write tenant B {ent}", preconditions="Two tenants with data", steps=["Login tenant A", f"API GET tenant B {ent} ID", "Attempt UI deep link", "Verify audit"], expected_steps=["Session A", "403/404", "No data leak", "SoD/tenant denial logged"], expected_final="Isolation confirmed"))
    return header("28_MULTITENANCY", "Multitenancy", 14) + "".join(cases)


def gen_rbac_negative_cases() -> str:
    denials = [
        ("RBAC-001", "Regulatory Viewer", "POST product", "P0"),
        ("RBAC-002", "Regulatory Viewer", "POST dossier", "P0"),
        ("RBAC-003", "Regulatory Reviewer", "POST product", "P0"),
        ("RBAC-004", "Regulatory Specialist", "POST approve", "P0"),
        ("RBAC-005", "Regulatory Specialist", "POST import", "P0"),
        ("RBAC-006", "Regulatory Submitter", "POST internal approval", "P0"),
        ("RBAC-007", "Regulatory Approver", "POST submit", "P0"),
        ("RBAC-008", "Document Controller", "GET regulatory dossiers", "P1"),
        ("RBAC-009", "CAPA Manager", "GET regulatory products", "P1"),
        ("RBAC-010", "Platform Administrator", "GET tenant dossiers", "P0"),
        ("RBAC-011", "Regulatory Specialist", "PUT pack publish", "P0"),
        ("RBAC-012", "Regulatory Reviewer", "POST bootstrap", "P0"),
        ("RBAC-013", "Unauthenticated", "GET dashboard", "P0"),
        ("RBAC-014", "Regulatory Manager", "DELETE tenant", "P0"),
    ]
    cases = []
    for tc_id, role, action, pri in denials:
        cases.append(tc_block(tc_id, module="RBAC Negative", feature="Permission denial", role=role, priority=pri, risk="High", objective=f"{role} denied for {action}", preconditions=f"User {role} logged in", steps=["Obtain token", f"Call {action}", "Check UI button hidden", "Verify no DB change"], expected_steps=["Token valid", "403 Forbidden", "UI consistent", "State unchanged"], expected_final="Access denied correctly"))
    return header("29_RBAC_NEGATIVE", "RBAC Negative", 14) + "".join(cases)


def gen_sod_cases() -> str:
    sod = [
        ("SOD-001", "Creator cannot accept own requirements", "Regulatory Specialist", "PreventSelfReview", "P0"),
        ("SOD-002", "Creator cannot internal approve", "Regulatory Specialist", "PreventSelfApproval", "P0"),
        ("SOD-003", "Specialist cannot external approve CT/RS", "Regulatory Specialist", "ExternalApprove", "P0"),
        ("SOD-004", "Approver cannot submit to authority", "Regulatory Approver", "SeparateApproverAndSubmitter", "P0"),
        ("SOD-005", "Submitter cannot internal approve", "Regulatory Submitter", "InternalApproval", "P0"),
        ("SOD-006", "Reviewer cannot approve for submission", "Regulatory Reviewer", "ApproveForSubmission", "P0"),
        ("SOD-007", "Submit blocked without internal clearance", "Regulatory Submitter", "InternalGate", "P0"),
        ("SOD-008", "Distinct users in journey (no multi-role)", "Multi-role", "JourneyIntegrity", "P0"),
        ("SOD-009", "API bypass transition denied", "Regulatory Specialist", "TransitionProtection", "P0"),
        ("SOD-010", "Approve while open observations denied", "Regulatory Approver", "ObservationGate", "P0"),
        ("SOD-011", "Self-review via API direct call denied", "Regulatory Specialist", "API", "P0"),
        ("SOD-012", "Audit logs SoD.Denied", "Regulatory Specialist", "Audit", "P0"),
        ("SOD-013", "TAC admin default no RA approve", "Tenant Administrator", "TAC", "P1"),
        ("SOD-014", "Quality Manager approve only not create", "Quality Manager", "QM", "P1"),
        ("SOD-015", "Regression after functional fix", "All RA roles", "Regression", "P0"),
        ("SOD-016", "Browser E2E multi-profile journey", "All RA roles", "Browser", "P0"),
    ]
    cases = []
    for tc_id, obj, role, policy, pri in sod:
        cases.append(tc_block(tc_id, module="SoD Regression", feature=policy, role=role, priority=pri, risk="Critical", permission=policy, objective=obj, preconditions="SoD policies enabled in tenant", steps=["Setup conflicting actor", "Attempt forbidden action via UI", "Repeat via API", "Check audit"], expected_steps=["Actor identified", "UI blocks or hides", "403 API", "SoD.Denied audit"], expected_final=obj, expected_audit="SoD.Denied"))
    return header("30_SOD_REGRESSION", "SoD Regression", 16) + "".join(cases)


def gen_journey_cases() -> str:
    journey_steps = [
        ("JOURNEY-001", "Specialist: login clean, create product", "Regulatory Specialist", "P0"),
        ("JOURNEY-002", "Specialist: create dossier + 22 requirements applied", "Regulatory Specialist", "P0"),
        ("JOURNEY-003", "Specialist: transition Draft→Planning→WaitingMfrDocs", "Regulatory Specialist", "P0"),
        ("JOURNEY-004", "Specialist: upload evidence, mark requirements Received", "Regulatory Specialist", "P0"),
        ("JOURNEY-005", "Specialist: request technical review, logout", "Regulatory Specialist", "P0"),
        ("JOURNEY-006", "Reviewer: login clean, reject 3 requirements with notes", "Regulatory Reviewer", "P0"),
        ("JOURNEY-007", "Reviewer: return dossier to Specialist, logout", "Regulatory Reviewer", "P0"),
        ("JOURNEY-008", "Specialist: correct docs, re-request review, logout", "Regulatory Specialist", "P0"),
        ("JOURNEY-009", "Reviewer: accept all critical requirements, logout", "Regulatory Reviewer", "P0"),
        ("JOURNEY-010", "Approver: internal approval for submission, logout", "Regulatory Approver", "P0"),
        ("JOURNEY-011", "Submitter: register submission date+reference, logout", "Regulatory Submitter", "P0"),
        ("JOURNEY-012", "Manager: record authority observation round 1, logout", "Regulatory Manager", "P0"),
        ("JOURNEY-013", "Specialist: prepare observation response, logout", "Regulatory Specialist", "P0"),
        ("JOURNEY-014", "Reviewer: review response, logout", "Regulatory Reviewer", "P0"),
        ("JOURNEY-015", "Approver: authorize resubmission, logout", "Regulatory Approver", "P0"),
        ("JOURNEY-016", "Submitter: register resubmission round 1, logout", "Regulatory Submitter", "P0"),
        ("JOURNEY-017", "Manager: observation round 2 + response cycle", "Regulatory Manager", "P0"),
        ("JOURNEY-018", "Approver: external approve CT/RS MINSA/CSS", "Regulatory Approver", "P0"),
        ("JOURNEY-019", "Viewer: verify registration active + days remaining", "Regulatory Viewer", "P0"),
        ("JOURNEY-020", "End-to-end: dashboard/pipeline/portfolio/audit reconcile", "Regulatory Viewer", "P0"),
    ]
    cases = []
    for tc_id, obj, role, pri in journey_steps:
        cases.append(tc_block(
            tc_id,
            module="Full Business Journey",
            feature="SoD multi-role",
            role=role,
            priority=pri,
            risk="Critical",
            regutrack_ref="Full REGUTRACK replacement path",
            objective=obj,
            preconditions="Distinct browser profile per role; prior journey steps completed",
            test_data="Dedicated journey product JOURNEY-{timestamp}",
            steps=["Logout any prior session", f"Login as {role} only", "Execute step action", "Verify state transition", "Capture evidence", "Logout"],
            expected_steps=["Clean session", f"{role} authenticated", "Action succeeds or correctly blocked", "Workflow state correct", "Screenshot+API saved", "Session cleared"],
            expected_final=obj,
            expected_audit="Full timeline from DossierCreated to Registration.Approved",
            expected_notification="Role-appropriate notification delivered",
            negative_variant="Same user attempting next role step must FAIL SoD",
        ))
    return header("35_FULL_BUSINESS_JOURNEY", "Full Business Journey (SoD-valid)", 20) + "".join(cases)


def main() -> None:
    gen_planning_docs()
    cases = gen_all_test_cases()
    total = 0
    for name, content in cases.items():
        write_file(TC_DIR / name, content)
        total += content.count("**Status** | NOT EXECUTED")
    print(f"Planning docs: 10")
    print(f"Test case files: {len(cases)}")
    print(f"Approximate test cases: {total}")


if __name__ == "__main__":
    main()
