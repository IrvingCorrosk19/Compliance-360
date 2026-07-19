# 01 — Master Test Plan
## Compliance 360 · Regulatory Affairs · Enterprise Functional Certification Program

| Campo | Valor |
|-------|--------|
| Programa | Enterprise Functional Certification Program v2.0 |
| Producto | Compliance 360 |
| BC bajo prueba | **Regulatory Affairs** (Registration Dossier–centric) |
| Objetivo de negocio | Demostrar con evidencia que Compliance 360 **reemplaza REGUTRACK** en operación diaria |
| Contrato funcional | `REGUTRACK 02JUN26 MG.xlsx` + `docs/regulatory-affairs/REGULATORY_COVERAGE_MATRIX.md` |
| Nivel | Big Four · FDA evidence mindset · ISO 13485 QMS · Banking control · QA Enterprise |
| Versión plan | **2.0** |
| Fecha | 2026-07-14 |
| Estado | **FASE 1 — DISEÑO. PROHIBIDA LA EJECUCIÓN.** |

---

## 1. Propósito

Este Master Test Plan define **cómo** se certificará Compliance 360 como reemplazo operativo de REGUTRACK.

No declara PASS/FAIL del producto.  
La certificación solo se cerrará cuando exista **evidencia ejecutada** (manual humana + automatización de regresión) suficiente para un veredicto **GO / NO GO** bajo `10_ENTRY_EXIT_CRITERIA.md`.

### 1.1 Lo que este plan NO es

| Prohibido | Motivo |
|-----------|--------|
| Smoke rápido / “parece OK” | No cumple nivel enterprise |
| Certificar leyendo código | El código no es evidencia operativa |
| Un único run Playwright = PASS global | Playwright es regresión, no certificación |
| Inventar casos sin ancla en sistema/Excel | Violación mandato Fase 2–5 |
| Declarar GO con P0 abiertos o SKIPPED | Reglas de programa |

### 1.2 Relación con pilot previo

Cualquier ejecución anterior a la **aprobación del Entry Gate v2.0** (scripts, Playwright 3/3, API smoke, bugs cerrados en discovery) se clasifica como **Pilot Discovery Evidence**. Puede informar el diseño de casos, **no** sustituye la Fase 10 de certificación.

---

## 2. Objetivos de certificación

1. **Dossier-centric:** el Registration Dossier es el centro operativo (no Template Builder, no CAPA, no `EnterpriseWorkspaceItem` como expediente).
2. **Paridad REGUTRACK:** hojas CTT REGISTROS / TUBERIA / DOCUMENTACION / CTT LICENCIAS OP representadas y operables.
3. **Case management:** pipeline, checklist 22, observaciones, aprobación CT/RS, renovaciones.
4. **RBAC real:** permisos de `RoleCatalog` + políticas `Regulatory.*` (+ `REGULATORY.PRODUCT.MANAGE` explícito).
5. **Importador profesional:** XLSX stage → validate → simulate → commit (migración histórica).
6. **Dashboard + alertas de negocio:** vencimientos 90/60/30/15/7/1/0, stuck >14d, bottleneck, $ por estado.
7. **Independencia del Excel:** el operador no necesita Excel para el día a día; el archivo queda solo para migración.

---

## 3. Alcance

### 3.1 En alcance (In Scope)

Ver detalle en `03_FUNCTIONAL_SCOPE.md` y `04_NON_FUNCTIONAL_SCOPE.md`.

- Auth / sesión / TAC (usuarios-roles mínimos para RA)
- Portafolio productos DM, fabricantes, certificados
- Expedientes, checklist, fechas, historial, observaciones, approve
- Pipeline UI + API search/filters
- Registros sanitarios y renovaciones de producto
- Licencias de operación + renovación + checklist catálogo
- Import JSON/XLSX
- Dashboard / alertas
- Config / bootstrap / requirement packs
- Multitenancy / 401 / SoD por rol RA
- UX de consola `#/regulatory`, responsive, mensajes, F5/back/doble-clic (NFR)

### 3.2 Fuera de alcance (Out of Scope) — explícito

| Ítem | Motivo |
|------|--------|
| Integración FADDI / Panamá Digital automática | Contrato: tarea manual |
| Certificación GCP/producción cloud completa | Tras lab/staging (`12`) |
| Rediseño UI cosmético no exigido por Excel | Regla de oro RA |
| CAPA / Risk / Audit Mgmt como sustituto del expediente | Fuera contrato REGUTRACK |
| Roles platform como operadores RA | No tienen permisos de negocio RA por defecto |

### 3.3 Gaps de producto conocidos (en alcance de prueba = deben fallar o tener WAIVER)

Documentados en matriz / implementación — **no se omiten**:

- Documents hard-link Product↔Requirement  
- Studio ↔ Requirement Pack Designer bridge  
- Import rollback formal + reporte post-carga  
- Columnas kanban Vencido / Renovación  
- CompanyMetadata (fecha constitución)  
- Commit masivo 100% filas históricas sin `maxRows`

Cada gap genera TC de severidad acorde; si FAIL en P0/P1, stop-on-fail o WAIVER firmado.

---

## 4. Organización del equipo de certificación (roles simulados)

| Rol | Responsabilidad | Artefacto |
|-----|-----------------|-----------|
| QA Manager | Entry/Exit, severidad, GO/NO GO, stop-on-P0 | `10`, Master Report |
| Functional Analyst | Alcance, trazabilidad Excel↔UI | `03`, `09` |
| Regulatory Affairs Specialist | Criterios negocio MINSA/CSS, dossier | Escenarios obs/approve |
| Product Owner | Prioridad P0 vs waiver | `08` |
| UX Specialist | Mensajes, responsive, flujo humano | NFR + scripts UI |
| Business Analyst | Preguntas dashboard/alertas | `06`, KPIs |
| Automation Engineer | Playwright **después** de casos aprobados | Fase 9 |
| Manual Tester | Ejecución humana uno-a-uno | Fase 10 |
| Enterprise Auditor | Evidencia ALCOA+, independencia lógica | Sample audit trail |

**Segregación de diseño vs ejecución:** en Fase 1–8 se diseña; en Fase 10 se ejecuta bajo Entry Gate. Automatización no aprueba casos que no pasaron manual crítico.

---

## 5. Fases del programa

| Fase | Entregable | Estado v2.0 |
|------|------------|-------------|
| 1 | Docs `01`–`12` | **EN CURSO** |
| 2 | Análisis sistema real | Actualizar `phase-02` |
| 3 | Matrices por rol (`RoleCatalog`) | Actualizar `phase-03` |
| 4 | Matriz cobertura (sin huecos) | Actualizar `phase-04` |
| 5 | Catálogo completo de TC | Actualizar `phase-05` |
| 6 | Escenarios E2E | Actualizar `phase-06` |
| 7 | Negativos / abuso | Actualizar `phase-07` |
| 8 | Scripts certificación por rol | Actualizar `phase-08` |
| **GATE** | Entry Criteria firmados | **Bloquea 9–10** |
| 9 | Playwright (regresión) | Bloqueada |
| 10 | Ejecución + bugs + Master Report | Bloqueada |

---

## 6. Gobernanza

### 6.1 Control de cambios durante certificación

- Congelar código de RA en rama de certificación al abrir Entry Gate, salvo **hotfix P0** documentado (`BUG_xxx` + retest).
- Todo cambio de producto reabre TC impactados (lista en BUG).

### 6.2 Severidad de defectos

| Sev | Definición | Acción |
|-----|------------|--------|
| P0 | Bloquea reemplazo Excel / integridad CT / SoD / cross-tenant / submit sin críticos / pérdida de datos | **STOP** ejecución, fix, retest |
| P1 | Función crítica Excel incumplida o workaround inaceptable | Fix antes de GO retiro Excel |
| P2 | Parcial / UX / columnas secundarias | Planificadas; waiver possible |
| P3 | Cosmético | Backlog |

### 6.3 Evidencia (ALCOA+)

Cada TC ejecutado en Fase 10 debe ser:

- **A**ttributable — usuario/rol/hora  
- **L**egible — pasos y resultado claro  
- **C**ontemporaneous — registrado al ejecutar  
- **O**riginal — logs/capturas/API response raw  
- **A**ccurate — sin “probablemente”

Ubicación: `docs/testing/evidence/{TC-ID}/` + índice en Master Report.

### 6.4 SKIPPED

**No existen.** Si un caso no puede ejecutarse: bloquear, documentar causa raíz, corregir ambiente/datos/producto, ejecutar.

---

## 7. Entregables finales

1. `FUNCTIONAL_CERTIFICATION_MASTER_REPORT.md` (solo tras Fase 10)  
2. Matriz de trazabilidad con estado Ejecutado/PASS/FAIL  
3. Bugs cerrados o WAIVER firmado  
4. Veredicto: **GO RETIRE EXCEL** | **GO STAGING** | **NO GO**

---

## 8. Aprobaciones (Fase 1)

| Rol | Firma | Fecha |
|-----|-------|-------|
| QA Manager | _pendiente cierre Fase 1–8_ | |
| Product Owner | _pendiente_ | |
| Regulatory Specialist | _pendiente_ | |

**Sin firmas Entry Gate → no hay Fase 9 ni 10.**
