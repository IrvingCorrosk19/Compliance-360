# 09 — Traceability Matrix
## v2.0 DISEÑO — Excel ↔ Dominio ↔ UI ↔ API ↔ TC

**Contrato:** `REGUTRACK 02JUN26 MG.xlsx`  
**Negocio:** `docs/regulatory-affairs/REGULATORY_COVERAGE_MATRIX.md`  
**Casos:** `phase-05/TC_INDEX.csv` (IDs)

Convención ID TC:

| Prefijo | Dominio |
|---------|---------|
| TC-AUTH-* | Login/sesión |
| TC-TAC-* | Users/roles seed |
| TC-RA-00** | Bootstrap/config/packs |
| TC-RA-01** | Products |
| TC-RA-02** | Workflow allowed |
| TC-RA-03** | Dossier/checklist/submit |
| TC-RA-04** | Observations |
| TC-RA-05** | Approve / SoD approve |
| TC-RA-06** | Dashboard/alerts/history |
| TC-RA-07** | Manufacturers/certs |
| TC-RA-08** | Licenses |
| TC-RA-09** | Import |
| TC-WF-NEG-* | Transiciones ilegales |
| TC-SEC-* | Seguridad |
| TC-UX-* | UX/NFR |
| TC-LEGACY-* | Anti-Workspace-as-dossier |
| SCN-* | Escenarios E2E Fase 6 |

---

## 1. Hojas → módulos → familias TC

| Hoja Excel | Módulo | Familias TC |
|------------|--------|-------------|
| CTT REGISTROS | Portfolio + Product API | TC-RA-01** |
| CTT REGISTROS checklist 18–39 | Dossier requirements | TC-RA-03** |
| Fechas 40–55 + hist 56–87 | Dates + history | TC-RA-03**, TC-RA-06** |
| TUBERIA / estados | Pipeline + transitions | TC-RA-02**, TC-WF-NEG-*, SCN-06 |
| DOCUMENTACION | Mfr + certs + alerts | TC-RA-07**, TC-RA-06** |
| CTT LICENCIAS OP | Licenses + renew | TC-RA-08** |
| Transversal import | Import views/API | TC-RA-09** |
| Transversal dashboard | Dashboard | TC-RA-06** |

---

## 2. Columnas críticas (muestra P0) → TC ancla

| Excel # | Campo | API/UI | TC ancla (diseño) |
|---------|-------|--------|-------------------|
| 4 | Nombre producto CT | products / portfolio | TC-RA-0100 |
| 6 | Catálogo único | products | TC-RA-0101 / dup |
| 8 | Distribuidor | products.distributorName | TC-RA-0103 |
| 12–13 | CT/RS + fecha | approve / registrations | TC-RA-0500 |
| 15 | Clase riesgo | riskClass | TC-RA-0104 |
| 18–39 | 22 requisitos | requirements + submit gate | TC-RA-0300/0302 |
| 42/44 | Máx recepción / alerta | dates + alerts | TC-RA-0604 |
| 47–50 | Sometido/Obs/Aprob | submit/obs/approve | TC-RA-0303/0400/0500 |
| 55 | Vencimiento | registrations / alerts | TC-RA-0604 |
| 88 | Línea | sourceLineNumber / import | TC-RA-0900 |
| DOC | Certificados | manufacturer-certificates | TC-RA-0701 |
| LIC | Renovación OP | license renewals | TC-RA-0800 |

*(Matriz completa columna-a-columna vive en REGULATORY_COVERAGE_MATRIX; cada fila Parcial/Pendiente exige TC de verificación de estado real.)*

---

## 3. Permisos → TC

| Permiso | Roles | TC |
|---------|-------|-----|
| REGULATORY.PRODUCT.MANAGE | Admin/Spec; **deny** Rev/View | SOD-*-CREATE-PRODUCT |
| REGULATORY.DOSSIER.APPROVE | Admin/Rev/QM; **deny** Spec/View | TC-RA-0501 / 0500-REVIEWER |
| REGULATORY.CONFIGURE | Admin; deny Spec/Rev/View | TC-RA-0904 |
| REGULATORY.*.READ | All RA roles | SOD-READ-* |

---

## 4. Completitud

No se cierra Fase 4/5 mientras exista:

- Endpoint RA sin TC  
- Vista hash sin TC de humo  
- Transición allowed/illegal sin TC  
- Rol RA sin script Fase 8  

Estado de cobertura numérica: ver `phase-04/COVERAGE_MATRIX.md` y conteo `TC_INDEX.csv`.
