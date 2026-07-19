# 19 — Correcciones aplicadas

| Corrección | Situación corregida | Verificación |
|---|---|---|
| Usuarios de regresión | Usuarios de pruebas provisionados en el tenant `cert` | Suites autenticadas por rol PASS |
| Datos E2E | `e2e/testdata.json` completado con los datos requeridos por regresión | Suites RA confirmadas PASS |
| Navegación QMS | Restauradas las rutas `suppliers`, `supplier-portal`, `audits` y `reports` | Navegación QMS activa; TAC/QM 2/2 PASS |
| Permisos de auditoría | Sustituidos códigos inexistentes `AUDIT_MANAGEMENT.*` por los códigos reales `AUDITMANAGEMENT.*` | Acceso de auditoría y RBAC verificados |
| Readiness | `/health/ready` corregido para no recibir el prefijo `/api/v1` | Health check resuelve en la ruta correcta |
| Helpers Playwright | Restaurados los helpers de creación requeridos por los escenarios | Suites regulatorias confirmadas PASS |
| Pruebas obsoletas | Alineadas con QMS activo y con el título i18n vigente | responsive 10/10; i18n 10/10; regulatory-affairs 3/3 |
| Contrato de menú QM | El test manual omitía Auditorías y Reportes pese a `AUDITMANAGEMENT.READ` y `REPORT.READ` | Menú exacto corregido; colección Playwright 71/71 PASS |
| Importador REGUTRACK | Una prueba antigua usaba TAC, contrario al manual que asigna importación a RA-ADM | Actor cambiado a Regulatory Administrator; Stage XLSX PASS |
| Baseline post-reset | El caso dependía de una variable local y exigía una base vacía, incompatible con una regresión repetible | Credencial de certificación y validación estructural; PASS |
| Omisiones Playwright | Dos escenarios opcionales terminaban en `skip` | Contratos convertidos a verificaciones ejecutables; 0 SKIP |
| Carga de evidencia | Requisito enlazado al archivo almacenado | `UPLOAD-REQUIREMENTS`: 3 `Received` con `storedFileId` |
| Rechazo Reviewer | Comentario y dictamen persistidos | `RETURN-REQ`; workflow 10/10 |
| Sometimiento | Trámite, número externo, fecha y comprobante agregados | `SUBMIT`; role-e2e 10/10 |
| Ciclo de autoridad | Observación, respuesta, resometimiento y resolución completados | `Observed → CorrectingObservation → Resubmitted → Closed` |
| Alert settings | Contrato, persistencia y UI conectados | ra-admin 10/10 |
| TAC | Edición/desactivación alineada; `UserStatus.Disabled=3` | TAC/QM 2/2; .NET 252/252 |
| QMS | Backend, DI, repositorios, endpoints, navegación y aprobación reactivados sin eliminación de tablas | build PASS; .NET 252/252 |

## Verificación consolidada

Resultados confirmados: colección Playwright completa **71/71 PASS, 0 FAIL, 0 SKIP**; manual roles 10/10, workflow 10/10, SoD 10/10, role-e2e 10/10, ra-admin 10/10, TAC/QM 2/2, responsive 10/10, i18n 10/10, regulatory-affairs 3/3; build **0 errores/0 advertencias** y .NET **252/252 PASS**.

`artifacts/e2e` conserva 20 `functional-summary.json` y 20 `functional-final.png`; véase `18_BROWSER_EVIDENCE_INDEX.md`.
