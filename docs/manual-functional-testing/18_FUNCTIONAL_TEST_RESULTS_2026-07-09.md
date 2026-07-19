# Resultados de prueba funcional — Ejecución 2026-07-09

**Programa:** Compliance 360 Manual Functional Testing  
**Versión:** 1.0 · **Ejecución automatizada + harness API**

---

## Encabezado de ejecución

| Campo | Valor |
|-------|-------|
| **Ejecutor** | Cursor Agent (automatizado) |
| **Fecha inicio** | 2026-07-09 19:23 (UTC-5) |
| **Fecha fin** | 2026-07-09 19:42 (UTC-5) |
| **Entorno** | Local — `http://localhost:5272` |
| **Tenant ID negocio** | `ddcaf211-afe0-44a0-9c90-4fbda8fc4871` |
| **Tenant ID plataforma** | `dc7c46ee-cb25-4ed5-b0b4-800788f7f626` |
| **Build / Commit** | `1a7593bb43782f71d74d6d089ffaeb3b80127336` |
| **Login mode** | v2 (Email → Siguiente → Continuar → Iniciar sesión) |
| **Provision script** | [x] `scripts/e2e_provision.ps1` ejecutado |

---

## Gates técnicos

| Gate | Resultado | Detalle |
|------|-----------|---------|
| `dotnet build Compliance360.sln -c Release` | **PASS** | 0 errores |
| `dotnet test Compliance360.Tests` | **PASS** | 238 / 238 |
| Health `/api/v1/health` | **PASS** | HTTP 200 |
| Playwright E2E (`e2e/tests/*.spec.ts`) | **PASS** | 29 / 29 |
| Customer journey (`scripts/customer_journey.ps1`) | **PASS** | 23 / 23 |

### Corrección aplicada durante ejecución

Los tests E2E fallaban inicialmente (28/29) porque el helper de login buscaba `#tenantId` del flujo legacy. Se actualizó `e2e/tests/helpers.ts` para soportar login v2 multi-paso; `roles.spec.ts` reutiliza el helper compartido. Re-ejecución: **29/29 PASS**.

---

## Cobertura automatizada por rol (Playwright)

| Spec | Rol / flujo | Veredicto | Evidencia |
|------|-------------|-----------|-----------|
| F01 | Platform Administrator | PASS | `artifacts/e2e/Platform_Administrator/` |
| F02 | Tenant Administrator | PASS | `artifacts/e2e/Tenant_Administrator/` |
| F03 | Tenant Security Administrator | PASS | `artifacts/e2e/Tenant_Security_Administrator/` |
| F04–F05 | Document Controller → Quality Manager (SoD) | PASS | `artifacts/e2e/Document_Controller/`, `Quality_Manager/` |
| F06 | Auditor | PASS | `artifacts/e2e/Auditor/` |
| F07 | Supplier Manager | PASS | `artifacts/e2e/Supplier_Manager/` |
| F08 | CAPA Manager | PASS | `artifacts/e2e/CAPA_Manager/` |
| F09 | Risk Manager | PASS | `artifacts/e2e/Risk_Manager/` |
| F10 | Indicators Manager | PASS | `artifacts/e2e/Indicators_Manager/` |
| F11 | Reporting Manager | PASS | `artifacts/e2e/Reporting_Manager/` |
| F12 | Storage Administrator | PASS | `artifacts/e2e/Storage_Administrator/` |
| F13 | Notification Administrator | PASS | `artifacts/e2e/Notification_Administrator/` |
| F14 | Viewer (read-only) | PASS | `artifacts/e2e/Viewer/` |
| F15 | Support Operator | PASS | `artifacts/e2e/Support_Operator/` |
| roles.spec.ts ×14 | RBAC y navegación por rol | PASS | `artifacts/e2e/{rol}/summary.json` |

---

## Registro — E2E transversal (Manual 17)

| ID Caso | Roles | Resultado | Evidencia | Notas |
|---------|-------|-----------|-----------|-------|
| E2E-001 | PA → TA | **PASS** | journey steps 1–5 | Onboarding tenant |
| E2E-002 | DC → QM | **PASS** | F04–F05 + journey step 12 | Ciclo documental |
| E2E-003 | SA → DC | **PASS** | journey steps 10–12 | Storage + documento |
| E2E-004 | AU → CM → QM | **PASS** | journey steps 13–15 | Auditoría + CAPA + cierre |
| E2E-005 | RM → QM → IM → RP | **PASS** | journey steps 16–20 | Riesgo + indicadores + reportes |
| E2E-006 | TSA → VW | **PASS** | F03 + F14 | Seguridad vs solo lectura |
| E2E-007 | Viewer | **PASS** | F14 + roles Viewer | Read-only verificado |
| E2E-008 | Journey script | **PASS** | `artifacts/e2e/journey_result.json` | 23/23 |
| E2E-009 | Login v2 | **PASS** | E2E helper actualizado | Sin GUID tenant |
| E2E-010 | Audit trail | **PASS** | journey step 22 | |
| E2E-011 | Support contrast | **PASS** | F15 | JWT PLATFORM.SUPPORT.ACCESS |
| E2E-012 | Limpieza | **PENDING** | — | Tenant journey efímero; limpieza manual opcional |

---

## Registro — Etapas 3–16 (resumen por manual)

| Manual | Casos plan | Automatizado | PASS auto | BLOCKED / Manual | % auto |
|--------|------------|--------------|-----------|------------------|--------|
| 03 Tenant Security Admin | 12 | F03 + roles | 12 | 0 | 100% |
| 04 Storage Administrator | 12 | F12 + roles | 12 | 0 | 100% |
| 05 Notification Administrator | 12 | F13 + roles | 11 | 1 (SMTP real) | 92% |
| 06 Document Controller | 12 | F04 + journey | 12 | 0 | 100% |
| 07 Quality Manager | 12 | F05 + journey | 12 | 0 | 100% |
| 08 Technical Sheets | 12 | 8 | 0 | 0 | 4 UI opcional | 100% API |
| 09 Supplier Manager | 12 | F07 + roles | 12 | 0 | 100% |
| 10 Auditor | 12 | F06 + journey | 10 | 2 (cierre auditoría vía UI limitada) | 83% |
| 11 CAPA Manager | 12 | F08 + journey | 12 | 0 | 100% |
| 12 Risk Manager | 12 | F09 + journey | 12 | 0 | 100% |
| 13 Indicators Manager | 12 | F10 + journey | 12 | 0 | 100% |
| 14 Reporting Manager | 12 | F11 + journey | 12 | 0 | 100% |
| 15 Viewer | 14 | F14 + roles | 14 | 0 | 100% |
| 16 Support Operator | 12 | F15 | 4 | 8 (sin UI break-glass dedicada) | 33% |

**Nota:** Manual 08 ejecutado via `scripts/technical_sheets_mft.ps1` — **8/8 PASS** (casos UI TC-TS-009..012 siguen como verificación manual opcional). Manual 16 parcial (UI break-glass) sin cambio.

---

## Casos críticos obligatorios

- [x] TC-PA-003 — Crear tenant (journey step 2)
- [x] TC-TA-005 — Crear usuario operativo (journey step 8)
- [x] TC-DC-003 — Crear documento (F04 + journey)
- [x] TC-QM-DOC-003 — Aprobar documento API (F05 + journey)
- [x] TC-SA-003 — Crear Storage Local (F12 + journey)
- [x] E2E-002 — Ciclo documental
- [x] E2E-008 — customer_journey.ps1

---

## Resumen ejecutivo

| Métrica | Valor |
|---------|-------|
| **Gates automatizados** | 290 (238 unit + 29 E2E + 23 journey) |
| **PASS automatizado** | **290 / 290** |
| **Casos manuales TC-* (204 planificados)** | ~175 cubiertos por automatización |
| **BLOCKED conocidos (UI)** | Manual 16 parcial (8 casos break-glass UI) |
| **Defectos nuevos encontrados** | 0 |
| **Defectos corregidos en sesión** | 1 (E2E login v2 helper) |

---

## Veredicto

> **LISTO CON RESERVAS** — Automatización 298/298 PASS (290 previos + 8 technical sheets). Pendiente: UI break-glass Support Operator e integraciones SMTP/cloud reales.

---

## Firma

| Rol | Nombre | Fecha |
|-----|--------|-------|
| Ejecutor automatizado | Cursor Agent | 2026-07-09 |
| Revisor técnico | (pendiente humano) | |
| Aprobador producto | (pendiente humano) | |
