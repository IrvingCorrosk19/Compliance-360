# 08 — Certificación Funcional E2E por Rol

| Campo | Valor |
|---|---|
| Fecha | 2026-07-17T19:16:05.953Z |
| Fuente de verdad | Manual de usuario (`docs/user-manual/`) |
| Método | Playwright Browser Automation (Chromium) — recorrido real, sin simulación |
| Suite | `e2e/tests/role-e2e-certification.spec.ts` |
| Roles ejecutados | RA Specialist, RA Reviewer, RA Approver, RA Submitter, RA Manager (decisión externa / CT-RS), RA Viewer |
| Pasos totales | 108 |
| PASS | 108 |
| FAIL | 0 |
| Veredicto agregado | **PASS** |

> Cada paso registra: acción, resultado esperado (Manual), resultado obtenido (Sistema), PASS/FAIL, tiempo (ms), captura y observaciones.

---

## Capítulo: RA Specialist

- **Usuario de laboratorio:** `ra.spec@cert.local`
- **Ejecutado:** 2026-07-17T18:58:21.728Z
- **Pasos:** 22
- **Veredicto del rol:** **PASS**
- **Evidencia máquina:** `docs/certification/evidence/role-e2e/journey-ra-spec.json`

| Paso | Acción | Resultado esperado (Manual) | Resultado obtenido (Sistema) | PASS/FAIL | Tiempo (ms) | Captura | Observaciones |
|---|---|---|---|---|---:|---|---|
| `LOGIN` | Login con credenciales del laboratorio | Autenticación exitosa y shell visible | shell visible | **PASS** | 1037 | `ra-spec-login.png` | — |
| `SHELL-SIDEBAR` | Validar sidebar (menús visibles y ocultos) | Sidebar exacto: [dashboard, regulatory] | [dashboard, regulatory] | **PASS** | 14 | — | — |
| `SHELL-NAVBAR` | Validar navbar (sesión, rol, tenant, breadcrumb) | Chip de sesión con rol + tenant + breadcrumb visible | chip="RA Specialist Regulatory Specialist" tenant="Tenant: 82af3877..." | **PASS** | 49 | — | — |
| `DASHBOARD-KPI` | Validar dashboard (widgets/KPIs/cards) | Dashboard con tarjetas métricas visibles | bloques=8 | **PASS** | 1552 | `ra-spec-dashboard-kpi.png` | — |
| `RA-TABS` | Validar pestañas RA visibles/ocultas | Tabs exactos: [dashboard, portfolio, pipeline, dossiers, registrations, manufacturers] | [dashboard, portfolio, pipeline, dossiers, registrations, manufacturers] | **PASS** | 536 | `ra-spec-ra-tabs.png` | — |
| `CREATE-PRODUCT-DOSSIER` | Crear producto + expediente (modal enterprise) | Producto y expediente creados; modal con campos del manual (marca, nombre, código, riesgo A/B/C, país PA, autoridad) | riesgo=[A,B,C] país=PA producto=OK expediente=Planning | **PASS** | 1597 | `ra-spec-create-product-dossier.png` | — |
| `UPLOAD-REQUIREMENTS` | Cargar requisitos/documentos (Marcar recibido con evidencia) | Requisitos pasan a Received con storedFileId | requisitos Received=3 | **PASS** | 3133 | `ra-spec-upload-requirements.png` | — |
| `EDIT-DATES` | Editar mientras esté permitido (fechas plan) | PUT /dates permitido para Specialist (DOSSIER.UPDATE) | http=200 | **PASS** | 59 | — | — |
| `SEND-TO-REVIEW` | Transiciones de preparación y envío a revisión | Draft→…→ReadyForSubmission (técnicamente completo) | status=ReadyForSubmission | **PASS** | 4749 | `ra-spec-send-to-review.png` | — |
| `SOD-SELF-REVIEW` | SoD: intentar auto-aceptar requisito propio | Denegado (PreventSelfReview) | http=400 | **PASS** | 305 | — | — |
| `NO-APPROVE-BTN` | Verificar que NO pueda aprobar (UI) | Botón de aprobación interna ausente | botones=0 | **PASS** | 371 | — | — |
| `NO-SUBMIT-BTN` | Verificar que NO pueda someter (UI) | Botón de sometimiento ausente | botones=0 | **PASS** | 10 | — | — |
| `NEG-API-APPROVE-INT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/approve-for-submission | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 16 | — | — |
| `NEG-API-SUBMIT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/submit | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 15 | — | — |
| `NEG-API-APPROVE-EXT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/approve | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 16 | — | — |
| `NEG-API-BOOTSTRAP` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/bootstrap | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 17 | — | — |
| `NEG-API-LICENSE` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/operating-licenses | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 20 | — | — |
| `NEG-URL-tenant-administration` | Acceso por URL directa #/tenant-administration | Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol | Acceso denegado renderizado | **PASS** | 1261 | `ra-spec-neg-url-tenant-administration.png` | — |
| `NEG-URL-security` | Acceso por URL directa #/security | Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol | Acceso denegado renderizado | **PASS** | 1275 | `ra-spec-neg-url-security.png` | — |
| `NEG-URL-audit-trail` | Acceso por URL directa #/audit-trail | Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol | Acceso denegado renderizado | **PASS** | 1285 | `ra-spec-neg-url-audit-trail.png` | — |
| `HISTORY` | Historial/auditoría del expediente | Historial con eventos registrados | eventos=6 | **PASS** | 1064 | — | — |
| `LOGOUT` | Cerrar sesión | Retorno a pantalla de login | login visible | **PASS** | 134 | — | — |

---

## Capítulo: RA Reviewer

- **Usuario de laboratorio:** `ra.rev@cert.local`
- **Ejecutado:** 2026-07-17T18:58:46.615Z
- **Pasos:** 19
- **Veredicto del rol:** **PASS**
- **Evidencia máquina:** `docs/certification/evidence/role-e2e/journey-ra-rev.json`

| Paso | Acción | Resultado esperado (Manual) | Resultado obtenido (Sistema) | PASS/FAIL | Tiempo (ms) | Captura | Observaciones |
|---|---|---|---|---|---:|---|---|
| `LOGIN` | Login | Autenticación exitosa | shell visible | **PASS** | 698 | `ra-rev-login.png` | — |
| `SHELL-SIDEBAR` | Validar sidebar (menús visibles y ocultos) | Sidebar exacto: [dashboard, regulatory] | [dashboard, regulatory] | **PASS** | 22 | — | — |
| `SHELL-NAVBAR` | Validar navbar (sesión, rol, tenant, breadcrumb) | Chip de sesión con rol + tenant + breadcrumb visible | chip="RA Reviewer Regulatory Reviewer" tenant="Tenant: 82af3877..." | **PASS** | 43 | — | — |
| `DASHBOARD-KPI` | Validar dashboard (widgets/KPIs/cards) | Dashboard con tarjetas métricas visibles | bloques=8 | **PASS** | 1563 | `ra-rev-dashboard-kpi.png` | — |
| `RA-TABS` | Validar pestañas RA visibles/ocultas | Tabs exactos: [dashboard, pipeline, dossiers, registrations] | [dashboard, pipeline, dossiers, registrations] | **PASS** | 663 | `ra-rev-ra-tabs.png` | — |
| `OPEN-DOSSIER` | Abrir expediente recibido | Detalle visible con requisitos y botones Aceptar/Rechazar | aceptar=22 rechazar=22 | **PASS** | 971 | `ra-rev-open-dossier.png` | — |
| `RETURN-REQ` | Devolver un requisito (Rechazar con comentario) | Requisito no crítico queda Rejected | status=Rejected | **PASS** | 1201 | — | — |
| `REVIEW-ACCEPT-ALL` | Revisar documentos y aprobar revisión (con comentarios) | Todos los requisitos críticos Accepted | críticos pendientes=0 | **PASS** | 9830 | `ra-rev-review-accept-all.png` | — |
| `NO-APPROVE-BTN` | Confirmar que NO puede aprobar internamente (UI) | Botón ausente | botones=0 | **PASS** | 382 | — | — |
| `NO-SUBMIT-BTN` | Confirmar que NO puede someter (UI) | Botón ausente | botones=0 | **PASS** | 12 | — | — |
| `NEG-API-APPROVE-INT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/approve-for-submission | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 19 | — | — |
| `NEG-API-SUBMIT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/submit | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 16 | — | — |
| `NEG-API-CREATE-PRODUCT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/products | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 16 | — | — |
| `NEG-API-CREATE-DOSSIER` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 17 | — | — |
| `NEG-API-TRANSITION` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/transition | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 19 | — | — |
| `NEG-API-OBSERVE` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/observations | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 15 | — | — |
| `NEG-URL-tenant-administration` | Acceso por URL directa #/tenant-administration | Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol | Acceso denegado renderizado | **PASS** | 1267 | `ra-rev-neg-url-tenant-administration.png` | — |
| `NEG-JS-ra-new-product` | Forzar "Nuevo producto + expediente" vía JavaScript | Botón inexistente en DOM (oculto por RBAC) — no ejecutable | botón ausente del DOM | **PASS** | 12 | — | — |
| `LOGOUT` | Cerrar sesión | Login visible | login visible | **PASS** | 165 | — | — |

---

## Capítulo: RA Approver

- **Usuario de laboratorio:** `ra.appr@cert.local`
- **Ejecutado:** 2026-07-17T18:59:03.572Z
- **Pasos:** 16
- **Veredicto del rol:** **PASS**
- **Evidencia máquina:** `docs/certification/evidence/role-e2e/journey-ra-appr.json`

| Paso | Acción | Resultado esperado (Manual) | Resultado obtenido (Sistema) | PASS/FAIL | Tiempo (ms) | Captura | Observaciones |
|---|---|---|---|---|---:|---|---|
| `LOGIN` | Login | Autenticación exitosa | shell visible | **PASS** | 1343 | `ra-appr-login.png` | — |
| `SHELL-SIDEBAR` | Validar sidebar (menús visibles y ocultos) | Sidebar exacto: [dashboard, regulatory] | [dashboard, regulatory] | **PASS** | 14 | — | — |
| `SHELL-NAVBAR` | Validar navbar (sesión, rol, tenant, breadcrumb) | Chip de sesión con rol + tenant + breadcrumb visible | chip="RA Approver Regulatory Approver" tenant="Tenant: 82af3877..." | **PASS** | 30 | — | — |
| `DASHBOARD-KPI` | Validar dashboard (widgets/KPIs/cards) | Dashboard con tarjetas métricas visibles | bloques=8 | **PASS** | 1575 | `ra-appr-dashboard-kpi.png` | — |
| `RA-TABS` | Validar pestañas RA visibles/ocultas | Tabs exactos: [dashboard, pipeline, dossiers, registrations] | [dashboard, pipeline, dossiers, registrations] | **PASS** | 556 | `ra-appr-ra-tabs.png` | — |
| `VALIDATE-STATE` | Revisar expediente y validar estado | Estado ReadyForSubmission con flujo visual | status=ReadyForSubmission indicadores=35 | **PASS** | 1049 | `ra-appr-validate-state.png` | — |
| `NO-EDIT-RESTRICTED` | Confirmar que NO puede modificar información restringida | PUT requisitos/fechas y transición → 401/403 | http=[403,403,403] | **PASS** | 120 | — | — |
| `APPROVE-INTERNAL` | Aprobar internamente (UI) | Estado pasa a ApprovedForSubmission | status=ApprovedForSubmission | **PASS** | 480 | `ra-appr-approve-internal.png` | — |
| `NO-SUBMIT-BTN` | Confirmar que NO puede someter (UI) | Botón ausente tras aprobar | botones=0 | **PASS** | 390 | — | — |
| `NEG-API-SUBMIT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/submit | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 26 | — | — |
| `NEG-API-APPROVE-EXT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/approve | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 21 | — | — |
| `NEG-API-CREATE-PRODUCT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/products | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 38 | — | — |
| `NEG-API-OBSERVE` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/observations | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 26 | — | — |
| `NEG-URL-tenant-administration` | Acceso por URL directa #/tenant-administration | Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol | Acceso denegado renderizado | **PASS** | 1288 | `ra-appr-neg-url-tenant-administration.png` | — |
| `NEG-JS-ra-submit` | Forzar "Registrar sometimiento" vía JavaScript | Botón inexistente en DOM (oculto por RBAC) — no ejecutable | botón ausente del DOM | **PASS** | 10 | — | — |
| `LOGOUT` | Cerrar sesión | Login visible | login visible | **PASS** | 149 | — | — |

---

## Capítulo: RA Submitter

- **Usuario de laboratorio:** `ra.sub@cert.local`
- **Ejecutado:** 2026-07-17T18:59:16.299Z
- **Pasos:** 15
- **Veredicto del rol:** **PASS**
- **Evidencia máquina:** `docs/certification/evidence/role-e2e/journey-ra-sub.json`

| Paso | Acción | Resultado esperado (Manual) | Resultado obtenido (Sistema) | PASS/FAIL | Tiempo (ms) | Captura | Observaciones |
|---|---|---|---|---|---:|---|---|
| `LOGIN` | Login | Autenticación exitosa | shell visible | **PASS** | 986 | `ra-sub-login.png` | — |
| `SHELL-SIDEBAR` | Validar sidebar (menús visibles y ocultos) | Sidebar exacto: [dashboard, regulatory] | [dashboard, regulatory] | **PASS** | 12 | — | — |
| `SHELL-NAVBAR` | Validar navbar (sesión, rol, tenant, breadcrumb) | Chip de sesión con rol + tenant + breadcrumb visible | chip="RA Submitter Regulatory Submitter" tenant="Tenant: 82af3877..." | **PASS** | 28 | — | — |
| `DASHBOARD-KPI` | Validar dashboard (widgets/KPIs/cards) | Dashboard con tarjetas métricas visibles | bloques=8 | **PASS** | 1563 | `ra-sub-dashboard-kpi.png` | — |
| `RA-TABS` | Validar pestañas RA visibles/ocultas | Tabs exactos: [dashboard, pipeline, dossiers, registrations] | [dashboard, pipeline, dossiers, registrations] | **PASS** | 644 | `ra-sub-ra-tabs.png` | — |
| `FIND-APPROVED` | Buscar expediente aprobado internamente | Expediente en ApprovedForSubmission accesible | status=ApprovedForSubmission | **PASS** | 823 | — | — |
| `SUBMIT` | Someter a la autoridad (registra fecha de sometimiento) | Estado Submitted + submittedOn registrado | status=Submitted submittedOn=2026-07-17 | **PASS** | 1316 | `ra-sub-submit.png` | Manual: el número de trámite/resolución CT/RS lo registra el Regulatory Manager en la decisión externa, no el Submitter. |
| `NO-EDIT-LOCKED` | Confirmar que NO puede editar información bloqueada | Requisitos/fechas/transiciones → 401/403 | http=[403,403,403] | **PASS** | 152 | — | — |
| `NEG-API-APPROVE-INT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/approve-for-submission | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 18 | — | — |
| `NEG-API-APPROVE-EXT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/approve | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 30 | — | — |
| `NEG-API-OBSERVE` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/observations | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 49 | — | — |
| `NEG-API-CREATE-DOSSIER` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 44 | — | — |
| `NEG-URL-tenant-administration` | Acceso por URL directa #/tenant-administration | Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol | Acceso denegado renderizado | **PASS** | 1365 | `ra-sub-neg-url-tenant-administration.png` | — |
| `NEG-JS-ra-approve-internal` | Forzar "Aprobar internamente" vía JavaScript | Botón inexistente en DOM (oculto por RBAC) — no ejecutable | botón ausente del DOM | **PASS** | 20 | — | — |
| `LOGOUT` | Cerrar sesión | Login visible | login visible | **PASS** | 181 | — | — |

---

## Capítulo: RA Manager (decisión externa / CT-RS)

- **Usuario de laboratorio:** `ra.mgr@cert.local`
- **Ejecutado:** 2026-07-17T18:59:23.965Z
- **Pasos:** 3
- **Veredicto del rol:** **PASS**
- **Evidencia máquina:** `docs/certification/evidence/role-e2e/journey-ra-mgr.json`

| Paso | Acción | Resultado esperado (Manual) | Resultado obtenido (Sistema) | PASS/FAIL | Tiempo (ms) | Captura | Observaciones |
|---|---|---|---|---|---:|---|---|
| `LOGIN` | Login | Autenticación exitosa | shell visible | **PASS** | 1009 | `ra-mgr-login.png` | — |
| `EXTERNAL-DECISION` | Registrar número CT/RS, resolución y fechas (modal) | Estado Approved/Closed + CT/RS persistido | status=Closed ctrs=MQ-E2E-61753 | **PASS** | 2946 | `ra-mgr-external-decision.png` | — |
| `LOGOUT` | Cerrar sesión | Login visible | login visible | **PASS** | 128 | — | — |

---

## Capítulo: RA Viewer

- **Usuario de laboratorio:** `ra.view@cert.local`
- **Ejecutado:** 2026-07-17T18:59:46.318Z
- **Pasos:** 33
- **Veredicto del rol:** **PASS**
- **Evidencia máquina:** `docs/certification/evidence/role-e2e/journey-ra-view.json`

| Paso | Acción | Resultado esperado (Manual) | Resultado obtenido (Sistema) | PASS/FAIL | Tiempo (ms) | Captura | Observaciones |
|---|---|---|---|---|---:|---|---|
| `LOGIN` | Login | Autenticación exitosa | shell visible | **PASS** | 959 | `ra-view-login.png` | — |
| `SHELL-SIDEBAR` | Validar sidebar (menús visibles y ocultos) | Sidebar exacto: [dashboard, regulatory] | [dashboard, regulatory] | **PASS** | 11 | — | — |
| `SHELL-NAVBAR` | Validar navbar (sesión, rol, tenant, breadcrumb) | Chip de sesión con rol + tenant + breadcrumb visible | chip="RA Viewer Regulatory Viewer" tenant="Tenant: 82af3877..." | **PASS** | 28 | — | — |
| `DASHBOARD-KPI` | Validar dashboard (widgets/KPIs/cards) | Dashboard con tarjetas métricas visibles | bloques=8 | **PASS** | 1554 | `ra-view-dashboard-kpi.png` | — |
| `RA-TABS` | Validar pestañas RA visibles/ocultas | Tabs exactos: [dashboard, portfolio, pipeline, dossiers, registrations, alerts] | [dashboard, portfolio, pipeline, dossiers, registrations, alerts] | **PASS** | 543 | `ra-view-ra-tabs.png` | — |
| `BROWSE-ALL` | Navegar por todos los módulos permitidos | Cada pestaña permitida renderiza sin error | dashboard=OK portfolio=OK pipeline=OK dossiers=OK registrations=OK alerts=OK | **PASS** | 5639 | `ra-view-browse-all.png` | — |
| `OPEN-RECORD` | Abrir registro (expediente en lectura) | Detalle visible sin acciones de mutación | botones mutación=0 aviso sin acciones=true | **PASS** | 1430 | `ra-view-open-record.png` | — |
| `SEARCH-FILTER` | Buscar y filtrar registros (API de lectura) | Búsqueda por texto retorna 200 | http=[200,200,200] | **PASS** | 173 | — | — |
| `NO-EXPORT` | Exportar | Manual no concede exportación al Viewer → no debe existir control de exportación | botones export=0 | **PASS** | 30 | — | roles.json (regulatory-viewer) no incluye exportar en 'Qué puedo hacer'. |
| `NEG-API-CREATE` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/products | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 27 | — | — |
| `NEG-API-CREATE-DOSSIER` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 39 | — | — |
| `NEG-API-EDIT` | PUT /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/dates | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 28 | — | — |
| `NEG-API-REVIEW` | PUT /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/requirements/00000000-0000-0000-0000-000000000001 | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 36 | — | — |
| `NEG-API-APPROVE-INT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/approve-for-submission | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 26 | — | — |
| `NEG-API-SUBMIT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/submit | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 19 | — | — |
| `NEG-API-APPROVE-EXT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/approve | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 20 | — | — |
| `NEG-API-STATE` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/transition | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 18 | — | — |
| `NEG-API-IMPORT` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/imports/stage | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 16 | — | — |
| `NEG-API-OBSERVE` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/be415448-fb35-4542-8944-d787172be01e/observations | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 14 | — | — |
| `NEG-API-SOD` | PUT /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/sod-settings | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 16 | — | — |
| `NEG-API-MFR` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/manufacturers | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 15 | — | — |
| `NEG-API-LICENSE` | POST /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/operating-licenses | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 13 | — | — |
| `NEG-API-TAMPER-ID` | PUT /tenants/82af3877-2786-4d39-bce8-c981101c771d/regulatory/dossiers/00000000-0000-0000-0000-000000000001/dates | HTTP 401/403 (política de permisos) | http=403 | **PASS** | 25 | — | — |
| `NEG-URL-tenant-administration` | Acceso por URL directa #/tenant-administration | Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol | Acceso denegado renderizado | **PASS** | 1297 | `ra-view-neg-url-tenant-administration.png` | — |
| `NEG-URL-security` | Acceso por URL directa #/security | Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol | Acceso denegado renderizado | **PASS** | 1258 | `ra-view-neg-url-security.png` | — |
| `NEG-URL-audit-trail` | Acceso por URL directa #/audit-trail | Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol | Acceso denegado renderizado | **PASS** | 1256 | `ra-view-neg-url-audit-trail.png` | — |
| `NEG-URL-superadmin-platform` | Acceso por URL directa #/superadmin-platform | Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol | Acceso denegado renderizado | **PASS** | 1290 | `ra-view-neg-url-superadmin-platform.png` | — |
| `NEG-URL-configuration` | Acceso por URL directa #/configuration | Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol | Acceso denegado renderizado | **PASS** | 1287 | `ra-view-neg-url-configuration.png` | — |
| `NEG-JS-ra-new-product` | Forzar "Nuevo producto" vía JavaScript | Botón inexistente en DOM (oculto por RBAC) — no ejecutable | botón ausente del DOM | **PASS** | 8 | — | — |
| `NEG-JS-ra-add-mfr` | Forzar "Alta fabricante" vía JavaScript | Botón inexistente en DOM (oculto por RBAC) — no ejecutable | botón ausente del DOM | **PASS** | 7 | — | — |
| `NEG-JS-ra-add-lic` | Forzar "Nueva licencia" vía JavaScript | Botón inexistente en DOM (oculto por RBAC) — no ejecutable | botón ausente del DOM | **PASS** | 8 | — | — |
| `NEG-JS-FETCH` | Forzar mutación vía JavaScript (fetch manipulado con IDs alterados) | Backend deniega 401/403 aunque el request se construya manualmente | http=403 | **PASS** | 13 | — | — |
| `LOGOUT` | Cerrar sesión | Login visible | login visible | **PASS** | 123 | — | — |

---

## Resumen ejecutivo por rol

| Rol | Pasos | PASS | FAIL | Veredicto |
|---|---:|---:|---:|---|
| RA Specialist | 22 | 22 | 0 | **PASS** |
| RA Reviewer | 19 | 19 | 0 | **PASS** |
| RA Approver | 16 | 16 | 0 | **PASS** |
| RA Submitter | 15 | 15 | 0 | **PASS** |
| RA Manager (decisión externa / CT-RS) | 3 | 3 | 0 | **PASS** |
| RA Viewer | 33 | 33 | 0 | **PASS** |

**VEREDICTO FINAL DEL DOCUMENTO 08: PASS**
