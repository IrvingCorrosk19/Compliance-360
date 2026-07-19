# Plantilla — Resultados de prueba funcional

**Programa:** Compliance 360 Manual Functional Testing  
**Versión plantilla:** 1.0

---

## Encabezado de ejecución

| Campo | Valor |
|-------|-------|
| **Ejecutor** | |
| **Fecha inicio** | |
| **Fecha fin** | |
| **Entorno** | Local — `http://localhost:5272` |
| **Tenant ID negocio** | (desde `e2e/testdata.json`) |
| **Tenant ID plataforma** | `dc7c46ee-cb25-4ed5-b0b4-800788f7f626` |
| **Build / Commit** | `git rev-parse HEAD` |
| **Login mode** | v2 / legacy |
| **Provision script** | [ ] `e2e_provision.ps1` ejecutado |

---

## Registro por caso — Etapa 1 Plataforma

| ID Caso | Rol | Resultado | Fecha | Evidencia | Notas |
|---------|-----|-----------|-------|-----------|-------|
| TC-PA-001 | Platform Administrator | PASS / FAIL / BLOCKED / PENDING | | | |
| TC-PA-002 | Platform Administrator | | | | |
| TC-PA-003 | Platform Administrator | | | | |
| TC-PA-004 | Platform Administrator | | | | |
| TC-PA-005 | Platform Administrator | | | | |
| TC-PA-006 | Platform Administrator | | | | |
| TC-PA-007 | Platform Administrator | | | | |
| TC-PA-008 | Platform Administrator | | | | |
| TC-PA-009 | Platform Administrator | | | | |
| TC-PA-010 | Platform Administrator | | | | |

## Registro — Etapa 2 Tenant

| ID Caso | Rol | Resultado | Fecha | Evidencia | Notas |
|---------|-----|-----------|-------|-----------|-------|
| TC-TA-001 | Tenant Administrator | | | | |
| TC-TA-002 | Tenant Administrator | | | | |
| TC-TA-003 | Tenant Administrator | | | | |
| TC-TA-004 | Tenant Administrator | | | | |
| TC-TA-005 | Tenant Administrator | | | | |
| TC-TA-006 | Tenant Administrator | | | | |
| TC-TA-007 | Tenant Administrator | | | | |
| TC-TA-008 | Tenant Administrator | | | | |
| TC-TA-009 | Tenant Administrator | | | | |
| TC-TA-010 | Tenant Administrator | | | | |
| TC-TA-011 | Tenant Administrator | | | | |
| TC-TA-012 | Tenant Administrator | | | | |

## Registro — Etapas 3–16 (resumen por manual)

| Manual | Casos totales | PASS | FAIL | BLOCKED | PENDING | % PASS |
|--------|---------------|------|------|---------|---------|--------|
| 03 Tenant Security Admin | 12 | | | | | |
| 04 Storage Administrator | 12 | | | | | |
| 05 Notification Administrator | 12 | | | | | |
| 06 Document Controller | 12 | | | | | |
| 07 Quality Manager | 12 | | | | | |
| 08 Technical Sheets | 12 | | | | | |
| 09 Supplier Manager | 12 | | | | | |
| 10 Auditor | 12 | | | | | |
| 11 CAPA Manager | 12 | | | | | |
| 12 Risk Manager | 12 | | | | | |
| 13 Indicators Manager | 12 | | | | | |
| 14 Reporting Manager | 12 | | | | | |
| 15 Viewer | 14 | | | | | |
| 16 Support Operator | 12 | | | | | |

## Registro — E2E transversal (Manual 17)

| ID Caso | Roles | Resultado | Fecha | Evidencia | Notas |
|---------|-------|-----------|-------|-----------|-------|
| E2E-001 | PA → TA | | | | |
| E2E-002 | DC → QM | | | | |
| E2E-003 | SA → DC | | | | |
| E2E-004 | AU → CM → QM | | | | |
| E2E-005 | RM → QM → IM → RP | | | | |
| E2E-006 | TSA → VW | | | | |
| E2E-007 | Viewer | | | | |
| E2E-008 | Journey script | | | | |
| E2E-009 | Login v2/legacy | | | | |
| E2E-010 | Audit trail | | | | |
| E2E-011 | Support contrast | | | | |
| E2E-012 | Limpieza | | | | |

---

## Resumen ejecutivo

| Métrica | Valor |
|---------|-------|
| **Total casos planificados** | ~170+ |
| **PASS** | |
| **FAIL** | |
| **BLOCKED** | |
| **PENDING THIRD-PARTY** | |
| **Tasa PASS** | % |
| **Defectos abiertos (ver 19)** | |
| **Casos críticos FAIL** | |

---

## Casos críticos obligatorios PASS

- [ ] TC-PA-003 — Crear tenant
- [ ] TC-TA-005 — Crear usuario operativo
- [ ] TC-DC-003 — Crear documento
- [ ] TC-QM-DOC-003 — Aprobar documento API
- [ ] TC-SA-003 — Crear Storage Local
- [ ] E2E-002 — Ciclo documental
- [ ] E2E-008 — customer_journey.ps1

---

## Notas del ejecutor

(Espacio libre para observaciones de entorno, flaky tests, dependencias externas SMTP/Azure, etc.)

---

## Firma

| Rol | Nombre | Fecha | Firma |
|-----|--------|-------|-------|
| Ejecutor QA | | | |
| Revisor técnico | | | |
| Aprobador producto | | | |
