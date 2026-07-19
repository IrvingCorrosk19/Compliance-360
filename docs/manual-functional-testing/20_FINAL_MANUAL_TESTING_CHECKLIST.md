# Checklist final — Pruebas funcionales manuales

**Programa:** Compliance 360 Manual Functional Testing  
**Versión:** 1.0  
**URL:** `http://localhost:5272`

---

## A. Preparación del entorno

- [x] Aplicación responde en `http://localhost:5272/api/v1/health`
- [x] PostgreSQL accesible (`compliance360`)
- [x] `scripts/e2e_provision.ps1` ejecutado
- [x] `e2e/testdata.json` generado con usuarios `@alimentos-premium.test`
- [x] Credenciales obtenidas desde testdata/appsettings (**no documentadas en manuales**)
- [x] Carpeta `artifacts/manual-functional-testing/` creada
- [x] Login v2 verificado (Email → Siguiente → Continuar → Iniciar sesion)

---

## B. Etapas por manual

### Etapa 1 — Plataforma
- [ ] `01_PLATFORM_ADMINISTRATOR_FUNCTIONAL_TESTS.md` — 10 casos
- [ ] TC-PA-003 (crear tenant) PASS

### Etapa 2 — Tenant
- [ ] `02_TENANT_ADMINISTRATOR_FUNCTIONAL_TESTS.md` — 12 casos
- [ ] TC-TA-005 (crear usuario) PASS

### Etapa 3 — Seguridad tenant
- [ ] `03_TENANT_SECURITY_ADMINISTRATOR_FUNCTIONAL_TESTS.md` — 12 casos

### Etapa 4 — Infraestructura
- [ ] `04_STORAGE_ADMINISTRATOR_FUNCTIONAL_TESTS.md` — 12 casos
- [ ] TC-SA-003 (Crear Storage Local) PASS
- [ ] `05_NOTIFICATION_ADMINISTRATOR_FUNCTIONAL_TESTS.md` — 12 casos

### Etapa 5 — Control documental
- [ ] `06_DOCUMENT_CONTROLLER_FUNCTIONAL_TESTS.md` — 12 casos
- [ ] TC-DC-003 PASS
- [ ] `07_QUALITY_MANAGER_FUNCTIONAL_TESTS.md` — 12 casos
- [ ] TC-QM-DOC-003 (aprobar vía API) PASS

### Etapa 6 — Fichas técnicas
- [x] `08_TECHNICAL_SHEETS_FUNCTIONAL_TESTS.md` — 8/8 API harness PASS (`scripts/technical_sheets_mft.ps1`)

### Etapa 7–12 — Operaciones
- [ ] `09_SUPPLIER_MANAGER_FUNCTIONAL_TESTS.md` — 12 casos
- [ ] `10_AUDITOR_FUNCTIONAL_TESTS.md` — 12 casos
- [ ] `11_CAPA_MANAGER_FUNCTIONAL_TESTS.md` — 12 casos
- [ ] `12_RISK_MANAGER_FUNCTIONAL_TESTS.md` — 12 casos
- [ ] `13_INDICATORS_MANAGER_FUNCTIONAL_TESTS.md` — 12 casos
- [ ] `14_REPORTING_MANAGER_FUNCTIONAL_TESTS.md` — 12 casos

### Etapa 13–14 — Consulta y soporte
- [ ] `15_VIEWER_FUNCTIONAL_TESTS.md` — 14 casos
- [ ] Modo solo lectura verificado en todos los módulos
- [ ] `16_SUPPORT_OPERATOR_FUNCTIONAL_TESTS.md` — 12 casos
- [ ] Confirmado: NO UI break-glass dedicada

### Etapa 15 — E2E transversal
- [x] `17_END_TO_END_BUSINESS_PROCESS_TESTS.md` — 12 escenarios (automatizado)
- [x] `scripts/customer_journey.ps1` ejecutado (E2E-008)
- [x] E2E-002 ciclo documental PASS

---

## C. Documentación y cierre

- [x] `18_FUNCTIONAL_TEST_RESULTS_2026-07-09.md` completado
- [ ] Defectos registrados en `19_DEFECT_REPORT_TEMPLATE.md` (0 defectos nuevos)
- [ ] Defectos Críticos/Altos: 0 abiertos / 0 cerrados
- [x] `21_DOCUMENTATION_QUALITY_REPORT.md` revisado
- [x] Evidencias bajo `artifacts/e2e/` y `artifacts/manual-functional-testing/`

---

## D. Criterios de calidad del programa

- [ ] Todos los casos críticos PASS (ver sección E)
- [ ] Tasa PASS global ≥ ___% (definir umbral interno, ej. 95%)
- [ ] Cero defectos P0 abiertos
- [ ] Auditoría coherente en acciones críticas (crear, aprobar, configurar)
- [ ] SoD verificado: DC no aprueba, QM no crea en UI, Viewer read-only
- [ ] Limitaciones UI documentadas en roadmap aceptadas (no confundidas con defectos)

---

## E. Casos críticos obligatorios

| ID | Descripción | PASS |
|----|-------------|------|
| TC-PA-003 | Crear tenant | [x] |
| TC-TA-005 | Crear usuario Document Controller | [x] |
| TC-SA-003 | Crear Storage Local | [x] |
| TC-DC-003 | Crear documento Action Center | [x] |
| TC-QM-DOC-003 | Aprobar documento Swagger/API | [x] |
| TC-CM-003 | Crear CAPA | [x] |
| TC-RP-003 | Seed standard reports | [x] |
| E2E-002 | Handoff DC → QM | [x] |
| E2E-008 | customer_journey.ps1 | [x] |

---

## F. Veredicto final

| Opción | Marcar |
|--------|--------|
| **LISTO PARA PRODUCCIÓN** | [ ] |
| **LISTO CON RESERVAS** (documentar) | [x] |
| **REQUIERE REMEDIACIÓN** | [ ] |

**Reservas / condiciones:** Manual 16 (UI break-glass), integraciones SMTP/cloud pendientes. Automatización 298/298 PASS incl. fichas técnicas.

**Aprobado por:** _________________ **Fecha:** _________

---

## G. Referencias rápidas

| Elemento | Valor |
|----------|-------|
| URL | `http://localhost:5272` |
| Platform tenant | `dc7c46ee-cb25-4ed5-b0b4-800788f7f626` |
| Platform admin | `admin@compliance360.local` |
| Support operator | `support@compliance360.local` |
| Tenant negocio slug | `alimentos-premium-pa` |
| Rutas clave | `#/documents`, `#/configuration`, `#/reports`, `#/superadmin-platform` |
| Formulario módulos | `#module-action-form` → **Crear registro real** |
| Journey API | `scripts/customer_journey.ps1` |
