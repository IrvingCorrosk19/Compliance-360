# FUNCTIONAL TEST EXECUTION PLAN (FASE 2)

**Convention:** cada caso = Objetivo · Precondiciones · Datos · Usuario/Rol · Flujo · Resultado esperado · Prioridad · Dependencias.
**Resultado obtenido** se completa en FASE 3–8 (`07_RETEST_RESULTS.md`).
**Execution mode:** Playwright `channel: chrome`, **headed** (navegador real observable) + harness API (`customer_journey.ps1`) para el ciclo de negocio profundo.

---

## TC-01 · Platform Administrator
- **Objetivo:** administrar plataforma y ciclo de vida de tenants sin operar datos de negocio.
- **Precondiciones:** app healthy, admin@compliance360.local.
- **Datos:** tenant "Alimentos Premium Panamá S.A.", taxId único.
- **Flujo:** login → SuperAdmin Platform Center → crear tenant → activar → suspender/restaurar → verificar NO acceso a datos de negocio de tenant → logout.
- **Esperado:** dashboard plataforma OK; lifecycle 200; datos de negocio no accesibles (aislamiento). **Prioridad:** Alta.

## TC-02 · Tenant Administrator
- **Objetivo:** administrar su tenant (empresa, usuarios, roles) y solo el suyo.
- **Flujo:** login → tenant-administration → editar empresa (PUT general info) → crear usuario → asignar rol → intentar acceder a otro tenant (403) → logout.
- **Esperado:** config guardada; cross-tenant 403. **Prioridad:** Alta.

## TC-03 · Tenant Security Administrator
- **Objetivo:** gestionar seguridad (MFA, sesión, lockout, IP whitelist).
- **Flujo:** login → security → editar política (PUT security) → ver → logout.
- **Esperado:** política persistida. **Prioridad:** Alta.

## TC-04 · Document Controller
- **Objetivo:** ciclo documental sin aprobar (SoD).
- **Flujo:** login → documents → crear tipo/categoría/documento → crear versión → enviar a revisión → intentar aprobar (403) → logout.
- **Esperado:** creación OK; aprobación 403. **Prioridad:** Alta.

## TC-05 · Quality Manager
- **Objetivo:** aprobaciones y supervisión (documentos, CAPA, riesgos).
- **Flujo:** login → dashboards → aprobar documento (submitted por DC) → cerrar CAPA → aprobar/cerrar riesgo → reportes → logout.
- **Esperado:** aprobaciones 200; trazabilidad en audit trail. **Prioridad:** Alta.

## TC-06 · Auditor
- **Objetivo:** ciclo de auditoría.
- **Flujo:** login → audits → programa → checklist → plan → auditoría → iniciar → hallazgo → cerrar → logout.
- **Esperado:** ciclo completo 200. **Prioridad:** Alta.

## TC-07 · Supplier Manager
- **Objetivo:** expediente y homologación de proveedor.
- **Flujo:** login → suppliers → crear → cargar documento → validar → evaluar → homologar → logout.
- **Esperado:** homologación 200. **Prioridad:** Media.

## TC-08 · CAPA Manager
- **Objetivo:** ciclo CAPA completo.
- **Flujo:** login → capa → crear → causa raíz/5-Why → acción correctiva → completar acción → verificar efectividad → logout.
- **Esperado:** efectividad 200 tras completar acción. **Prioridad:** Alta.

## TC-09 · Risk Manager
- **Objetivo:** gestión de riesgo sin aprobar (SoD).
- **Flujo:** login → risks → crear → evaluar (inherente/residual) → matriz/heat-map → intentar cerrar (403) → logout.
- **Esperado:** gestión OK; cierre 403 (lo cierra Quality). **Prioridad:** Alta.

## TC-10 · Indicators Manager
- **Objetivo:** indicadores y medición.
- **Flujo:** login → indicators → categoría → indicador → periodo → medición → semáforo/tendencia → logout.
- **Esperado:** medición y trend 200. **Prioridad:** Media.

## TC-11 · Reporting Manager
- **Objetivo:** Report Center.
- **Flujo:** login → reports → listar → ejecutar → completar → exportar → logout.
- **Esperado:** ejecución 200; export disponible. **Prioridad:** Media.

## TC-12 · Storage Administrator
- **Objetivo:** proveedores de storage y upload.
- **Flujo:** login → configuration → crear provider local → activar → subir archivo → metadata → logout.
- **Esperado:** upload 200 (multipart). **Prioridad:** Alta.

## TC-13 · Notification Administrator
- **Objetivo:** configuración de notificaciones.
- **Flujo:** login → configuration → crear provider → plantilla → dashboard → logout.
- **Esperado:** config 200; envío real = PENDING THIRD-PARTY. **Prioridad:** Media.

## TC-14 · Viewer
- **Objetivo:** solo lectura.
- **Flujo:** login → recorrer módulos permitidos → verificar ausencia de botones crear/editar → intentar crear vía API (403) → logout.
- **Esperado:** lectura OK; toda escritura 403. **Prioridad:** Alta.

## TC-15 · Support Operator
- **Objetivo:** soporte plataforma (break-glass auditable).
- **Flujo:** login plataforma → acceso soporte → verificar auditoría de acceso → logout.
- **Esperado:** acceso auditado; sin escritura de negocio. **Prioridad:** Media.

---

## TC-TRANSVERSAL
| ID | Objetivo | Esperado |
|---|---|---|
| TX-01 | Tenant isolation | cross-tenant 403 en documentos/capa/risk/storage |
| TX-02 | Sin errores de consola | 0 errores JS en cada ruta |
| TX-03 | Sin stack traces visibles | errores 4xx con mensaje amigable, no 500 |
| TX-04 | Audit trail | acciones críticas registradas |
| TX-05 | Logout | sesión cerrada, vuelta a login |

---

## Matriz de prioridad de ejecución
1. Onboarding + Platform (TC-01..03) — Alta
2. Ciclo de negocio profundo (TC-04..12) — Alta/Media vía harness `customer_journey.ps1`
3. Read-only + SoD (TC-14, TC-09) — Alta
4. Reportes/Notificaciones/Support (TC-11,13,15) — Media
5. Transversales (TX-01..05) — Alta

*FASE 2 completada. Ejecución inicia en FASE 3.*
