# 03 — Functional Scope
## Sistema real · Regulatory Affairs · v2.0 DISEÑO

**Fuente:** as-built API/UI/dominio + `REGUTRACK 02JUN26 MG.xlsx` + `REGULATORY_COVERAGE_MATRIX.md`.  
**Estado:** DISEÑO — sin ejecución.

---

## 1. Frontera del producto bajo certificación

| Incluye | Excluye |
|---------|---------|
| BC Regulatory Affairs (`#/regulatory`) | CAPA/Risk/Audit como “expediente” |
| TAC mínimo: users/roles para sembrar RA | Super Admin cosmético platform-only |
| Documents **solo** en cuanto afectan requisitos/orphan rule | Rediseño Template Builder completo |
| Auth login enterprise (email → org → password) | Otros módulos sin grants RA |

---

## 2. Módulos funcionales (consola RA)

| Módulo UI | Hash view | Capacidades |
|-----------|-----------|-------------|
| Dashboard | `dashboard` | KPIs negocio + breakdowns |
| Portafolio | `portfolio` | Alta producto, listado, campos Excel |
| Pipeline | `pipeline` | Kanban por estado (subset UI) |
| Expedientes | `dossiers` | Detalle, checklist, fechas, obs, submit, approve |
| CT / RS | `registrations` | Lectura registros activos |
| Fabricantes | `manufacturers` | Alta fabricante + certificados |
| Licencias OP | `licenses` | Alta + renovación |
| Alertas | `alerts` | Evaluate thresholds |
| Importación | `import` | Stage XLSX/JSON + Commit |
| Config | `config` | Bootstrap + packs |

---

## 3. API under test (inventario as-built)

Base: `/api/v1/tenants/{tenantId}/regulatory`

| Área | Endpoints (resumen) |
|------|---------------------|
| Bootstrap / packs / authorities | `POST /bootstrap`, `GET /requirement-packs`, `GET /authorities` |
| Dashboard / alerts | `GET /dashboard`, `GET /alerts/evaluate` |
| Manufacturers | `GET|POST /manufacturers`, `GET|POST /manufacturer-certificates` |
| Products | `GET|POST /products`, `GET /products/{id}` — create exige `REGULATORY.PRODUCT.MANAGE` |
| Dossiers | CRUD search, transition, dates, requirements, submit, observations, approve |
| Renewals product | `POST /renewals` |
| Registrations | `GET /registrations` |
| Licenses | `GET|POST /operating-licenses`, `POST .../renewals` |
| Import | `GET /imports`, `POST /imports/stage`, `POST /imports/xlsx`, `POST /imports/{id}/commit?maxRows=` |

Cada endpoint = ≥1 TC positivo + ≥1 negativo de auth/permiso donde aplique.

---

## 4. Workflow expediente (dominio)

**16 estados.** Transiciones permitidas (única verdad):

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

**Gates de negocio:**

- Submit bloqueado si critical requirements ≠ Accepted.  
- DocumentsReceived sin evidencia requiere **waiver ≥ 8 caracteres**.  
- Approve solo desde UnderAuthorityReview (no desde Observed con obs abiertas).  
- Specialist **no** Approve; Reviewer **no** ProductManage / Configure/Import.

---

## 5. Hojas REGUTRACK → módulos

| Hoja Excel | Módulo C360 |
|------------|-------------|
| CTT REGISTROS / (2) / TUBERIA | Productos + Dossiers + Pipeline + Registrations + History |
| DOCUMENTACION | Manufacturers + Certificates + Alerts |
| CTT LICENCIAS OP | Operating licenses + renewals + checklist catálogo |

---

## 6. Items Parcial/Pendiente (obligatorios en alcance de prueba)

| Ítem | Expectativa de certificación |
|------|------------------------------|
| Ficha técnica archivo Documents | TC evidencia estado real (PASS o FAIL/P1) |
| Studio pack bridge | TC; FAIL→P1/P2 según DoD |
| Import rollback | TC; si no existe = FAIL P1 o Pendiente con WAIVER |
| Kanban Vencido/Renovación | TC UI; gap conocido |
| Commit full Excel sin límite | TC P0/P1 performance+integridad separado de smoke `maxRows` |
| CompanyMetadata | TC Pendiente → FAIL o WAIVER |

---

## 7. Fuera de alcance funcional detallado

Ver `01` §3.2. No se generarán TC inventados para FADDI automático.
