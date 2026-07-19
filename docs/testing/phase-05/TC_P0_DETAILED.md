# Fase 5 — Casos P0 detallados (pasos enterprise)

Estado inicial de todos: **Designed**. Resultado obtenido / Evidencia se llenan en Fase 10.

---

## TC-AUTH-0100 — Login válido Tenant Admin

| Campo | Valor |
|-------|-------|
| Objetivo | Autenticar operador y obtener JWT + tenant |
| Precondiciones | Usuario activo Tenant Administrator; app UP |
| Rol | Tenant Administrator |
| Datos | email/password válidos del ambiente |
| Pasos | 1. Abrir `/` 2. Ingresar tenant/email/password 3. Submit login |
| Esperado | Session token; UI principal; menú Regulatory visible |
| Severidad | Critical | Prioridad | P0 | Estado | Designed |

---

## TC-RA-0001 — Bootstrap regulatorio

| Campo | Valor |
|-------|-------|
| Objetivo | Sembrar autoridades + pack 22 |
| Precondiciones | Login con REGULATORY.CONFIGURE |
| Rol | Regulatory Administrator / Tenant Admin |
| Datos | — |
| Pasos | 1. `#/regulatory` → Configuración 2. Bootstrap 3. GET authorities + requirement-packs |
| Esperado | MINSA/CSS; pack published con 22 definitions |
| Severidad | Critical | P0 | Designed |

---

## TC-RA-0103 — Producto con campos Excel críticos

| Campo | Valor |
|-------|-------|
| Objetivo | Crear producto con distributor, suppliers, line, tech, form |
| Rol | Regulatory Specialist |
| Datos | CERT-PROD-*; riskClass A; distributor Multimed |
| Pasos | POST `/products` con campos extendidos; GET by id |
| Esperado | DTO refleja DistributorName, RegisteredSuppliersCount, SourceLineNumber, TechnicalSheetReference, FormReference |
| Severidad | High | P0 |

---

## TC-RA-0104 — Catálogo duplicado rechazado

| Campo | Valor |
|-------|-------|
| Tipo | Negative |
| Pasos | Crear producto código X; repetir mismo catalogCode |
| Esperado | Failure “Catalog code already exists” / HTTP error |
| P0 | Critical |

---

## TC-RA-0300 — Pack 22 en dossier nuevo

| Pasos | Crear dossier para producto; GET detail |
| Esperado | `requirements.length === 22`; criticals marcados |
| Trace | Checklist REGUTRACK |
| P0 | Critical |

---

## TC-RA-0302 — Submit bloqueado

| Precondiciones | Dossier ReadyForSubmission; critical Pending |
| Pasos | POST submit |
| Esperado | Error dominio; status no Submitted |
| P0 | Critical |

---

## TC-RA-0303 — Submit permitido

| Precondiciones | Todos críticos Accepted |
| Pasos | POST submit |
| Esperado | Status Submitted; SubmittedOn set; History StatusTransition |
| P0 | Critical |

---

## TC-RA-0400 / 0401 / 0402 — Observación ciclo

| Pasos | Open observation → Correcting → respond/close → Resubmit → UnderAuthorityReview |
| Esperado | Estados legales; observation cerrable |
| P0 | Critical |

---

## TC-RA-0500 — Approve CT/RS

| Rol | Regulatory Reviewer o Admin |
| Pasos | POST approve con número + issuedOn + expiresOn |
| Esperado | Registration activa; product commercializable; dossier Approved |
| P0 | Critical |

---

## TC-RA-0501 — Specialist no aprueba

| Rol | Regulatory Specialist |
| Pasos | POST approve |
| Esperado | 403 |
| P0 | Critical |

---

## TC-RA-0600 — Historial tipado

| Pasos | GET dossier tras create/transitions |
| Esperado | history contiene DossierCreated y StatusTransition persistidos en DB |
| Trace | cols 56–87 |
| P0 | High |

---

## TC-RA-0902 — Stage XLSX real

| Datos | `REGUTRACK 02JUN26 MG.xlsx` |
| Pasos | multipart POST `/imports/xlsx` |
| Esperado | status Simulated; rows ≫ 0; errors=0 (warnings OK) |
| P0 | Critical |

---

## TC-RA-0903 — Commit import

| Precondiciones | Job Simulated |
| Pasos | POST commit |
| Esperado | importedRowCount > 0; productos/certs/licencias según recordType |
| P0 | Critical |

---

## TC-RA-0904 — Specialist sin import

| Esperado | 403 en stage/xlsx |
| P0 | Critical |

---

## TC-LEGACY-0100 — No Workspace como expediente

| Pasos | Confirmar `#/regulatory` carga RA Console; no EnterpriseWorkspaceItem tracker |
| Esperado | Badge “Case Management”; texto legacy fuera de operación |
| P0 | Critical |

---

## Suite workflow TC-RA-0200+ / TC-WF-NEG-*

Ver CSV: cada arista allowed + cada ilegal. Ejecución puede hacerse vía API en lote con evidencia tabulada.

---

## TC-RA-0205 / 0206 — Waiver DocumentsReceived

| Objetivo | Exigir audited exception ≥ 8 chars |
| Precondiciones | Dossier en WaitingManufacturerDocuments |
| Rol | Regulatory Specialist |
| Pasos 0205 | transition DocumentsReceived con waiver `"short"` |
| Esperado 0205 | 400; estado permanece WaitingManufacturerDocuments |
| Pasos 0206 | waiver `"CERT waiver evidence N/A"` |
| Esperado 0206 | 200; status DocumentsReceived |
| Prioridad | P0 |

---

## TC-RBAC-0310 — Reviewer no crea producto (BUG_003)

| Objetivo | SoD: Registration.Manage ≠ Product.Manage |
| Rol | Regulatory Reviewer |
| Pasos | POST `/products` con body válido |
| Esperado | **403** |
| Prioridad | P0 |

---

## TC-RA-0908 — Uniquify catalog en commit

| Objetivo | Excel con catálogos duplicados no aborta commit ni viola UX unique index |
| Rol | Regulatory Administrator |
| Pasos | Stage filas con mismo catalogCode; commit |
| Esperado | imported ≥1; códigos sufijados; job Committed o filas MarkFailed sin 500 |
| Prioridad | P0 |

---

## TC-SEC-0400 — IDOR dossier

| Objetivo | Token tenant A no lee dossier tenant B |
| Pasos | GET dossierId ajeno con path tenant A |
| Esperado | 403/404; sin body sensible |
| Prioridad | P0 |

