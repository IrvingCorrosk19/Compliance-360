# 01 — FUNCTIONAL CERTIFICATION MASTER PLAN

**Program:** Final Enterprise Functional Certification  
**Product:** Compliance 360 Enterprise  
**Baseline commit:** `4fb2be0`  
**Date:** 2026-07-05  
**Environment:** Development (localhost:5272), PostgreSQL 18 local  
**Authority:** Product Owner · QA Director · Enterprise Architect · ISO 9001/27001 Consultant

---

## 1. Objetivo

Certificar funcionalmente Compliance 360 como producto **Enterprise Premium** listo para cliente real, sin agregar funcionalidades ni cambiar procesos de negocio. Validar estabilidad, RBAC, SoD, multi-tenancy, flujos completos y experiencia de usuario en navegador observable (headful).

**Meta:** Entregar un producto que compita en calidad, estabilidad y solidez funcional con plataformas Enterprise (SAP, Dynamics, Oracle, ServiceNow, Salesforce).

---

## 2. Alcance

| Incluido | Excluido |
|---|---|
| 15 roles certificados (14 tenant + 2 platform) | Nuevas features |
| 20 rutas SPA + 13 tabs SuperAdmin + 15 tabs TAC | Refactorizaciones innecesarias |
| CRUD operacional por módulo (UI + API) | Cambios de reglas de negocio |
| Cadenas SoD (Document, CAPA, Storage/Notification) | Integraciones sin credenciales reales |
| Tenant onboarding (API harness 23 pasos) | Pruebas de carga a escala producción |
| Dashboards, reportes, audit trail | |
| RBAC, aislamiento tenant, break-glass | |
| UX review (PO + QA + Arquitecto) | |

---

## 3. Estrategia

1. **Análisis previo** — Inventario funcional completo (02) y plan de ejecución (03) antes de correr pruebas.
2. **Gates técnicos** — `dotnet clean → restore → build (Release) → test` (238 unit tests).
3. **Ejecución browser headful** — Playwright `headless: false`, `channel: chrome`, reporter de progreso en tiempo real.
4. **Harness API** — `scripts/customer_journey.ps1` (23 pasos ciclo de negocio completo).
5. **Metodología de defectos** — Detectar → Reproducir → RCA → Documentar → Diseñar → Implementar → Build → Retest → Cerrar flujo completo.
6. **Terceros** — Clasificar `PENDING THIRD-PARTY CONFIGURATION`, nunca FAIL por credenciales ausentes.

---

## 4. Cobertura

### 4.1 Roles (15)

| # | Rol | Tenant |
|---|---|---|
| 1 | Platform Administrator | Platform |
| 2 | Tenant Administrator | ddcaf211-… |
| 3 | Tenant Security Administrator | ddcaf211-… |
| 4 | Document Controller | ddcaf211-… |
| 5 | Quality Manager | ddcaf211-… |
| 6 | Auditor | ddcaf211-… |
| 7 | Supplier Manager | ddcaf211-… |
| 8 | CAPA Manager | ddcaf211-… |
| 9 | Risk Manager | ddcaf211-… |
| 10 | Indicators Manager | ddcaf211-… |
| 11 | Reporting Manager | ddcaf211-… |
| 12 | Storage Administrator | ddcaf211-… |
| 13 | Notification Administrator | ddcaf211-… |
| 14 | Viewer | ddcaf211-… |
| 15 | Support Operator | Platform |

### 4.2 Módulos

Identity · RBAC · Tenant Administration · SuperAdmin Platform · Documents · Workflows · Technical Sheets · Suppliers · Audit Management · CAPA · Risks · Indicators · Reporting · Storage · Notifications · Enterprise Workspaces · Audit Trail · Observability

### 4.3 Procesos de negocio

Tenant onboarding · Configuración empresa/branding/seguridad · Usuarios/roles · Documentos (crear + SoD aprobación) · Auditorías · CAPA · Riesgos · Indicadores · Reportes · Storage/Notifications · Logout

---

## 5. Riesgos

| Riesgo | Mitigación |
|---|---|
| App bloquea DLLs en VS | Detener proceso antes de rebuild |
| MFA bloquea automatización | Tenant ops con MFA desactivado en bootstrap |
| UI no expone todos los pasos API | Validar API vía harness + documentar gap UI |
| SMTP/cloud sin credenciales | PENDING THIRD-PARTY |
| Dark mode/responsive sin E2E | Revisión manual documentada en 08 |

---

## 6. Dependencias

- .NET 9 SDK, Node 22+, Playwright + Chrome
- PostgreSQL 18 (`C:\Program Files\PostgreSQL\18\bin\psql.exe`)
- Tenant ops `ddcaf211-afe0-44a0-9c90-4fbda8fc4871` con 13 usuarios activos
- Platform tenant `dc7c46ee-cb25-4ed5-b0b4-800788f7f626`

---

## 7. Criterios PASS / FAIL

### PASS requiere

- Gates técnicos: build 0 errors, 238/238 unit tests
- E2E browser: 29/29 Playwright (roles + functional)
- Customer journey: 23/23 API steps
- 15 roles con navegación RBAC correcta
- SoD verificado (Document, CAPA, Storage/Notification)
- Aislamiento tenant (Platform Admin 403 en business data cross-tenant)
- Cero defectos funcionales críticos abiertos
- Cero errores JS/5xx en flujos certificados

### FAIL si

- Cualquier defecto crítico sin RCA + corrección + retest
- RBAC roto o bypass no auditado
- Flujo de negocio incoherente atribuible al producto

### Veredictos permitidos

- ✅ **FUNCTIONAL CERTIFICATION PASSED**
- ✅ **FUNCTIONAL CERTIFICATION PASSED WITH THIRD-PARTY PENDING CONFIGURATION**
- ❌ **FUNCTIONAL CERTIFICATION FAILED**

---

## 8. Orden de ejecución

| Fase | Entregable | Acción |
|---|---|---|
| 0 | 01 (este documento) | Plan maestro |
| 1 | 02_APPLICATION_FUNCTIONAL_MAP.md | Inventario funcional |
| 2 | 03_FUNCTIONAL_TEST_EXECUTION_PLAN.md | Plan de pruebas |
| 3 | Ejecución | Playwright headful + customer_journey |
| 4 | 04_ROOT_CAUSE_ANALYSIS.md | Solo si hay defectos |
| 5 | 05_CORRECTION_LOG.md | Correcciones aplicadas |
| 6 | 06_RETEST_RESULTS.md | Revalidación |
| 7 | 07_PRODUCT_OWNER_REVIEW.md | Revisión PO |
| 8 | 08_USER_EXPERIENCE_REVIEW.md | Revisión UX |
| 9 | 09_FUNCTIONAL_CERTIFICATION_REPORT.md | Reporte técnico |
| 10 | 10_EXECUTIVE_SUMMARY.md | Resumen ejecutivo |

---

*FASE 0 completada. Proceder a FASE 1 y 2 antes de ejecución.*
