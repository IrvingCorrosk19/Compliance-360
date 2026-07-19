# 10 — Matriz de Permisos por Rol (E2E observada)

| Campo | Valor |
|---|---|
| Fecha | 2026-07-17T19:16:05.953Z |
| Origen | Resultados reales de Playwright (no teóricos) |
| Filas | 108 |

| Rol | Pantalla | Acción | Permitido (Manual) | Denegado (Manual) | Resultado Sistema | Veredicto |
|---|---|---|---|---|---|---|
| RA-SPEC | Auth | Login con credenciales del laboratorio | Sí | — | shell visible | **PASS** |
| RA-SPEC | Shell | Validar sidebar (menús visibles y ocultos) | Sí | — | [dashboard, regulatory] | **PASS** |
| RA-SPEC | Shell | Validar navbar (sesión, rol, tenant, breadcrumb) | Sí | — | chip="RA Specialist Regulatory Specialist" tenant="Tenant: 82af3877..." | **PASS** |
| RA-SPEC | Shell | Validar dashboard (widgets/KPIs/cards) | Sí | — | bloques=8 | **PASS** |
| RA-SPEC | Shell | Validar pestañas RA visibles/ocultas | Sí | — | [dashboard, portfolio, pipeline, dossiers, registrations, manufacturers] | **PASS** |
| RA-SPEC | Regulatory | Crear producto + expediente (modal enterprise) | Sí | — | riesgo=[A,B,C] país=PA producto=OK expediente=Planning | **PASS** |
| RA-SPEC | Regulatory | Cargar requisitos/documentos (Marcar recibido con evidencia) | Sí | — | requisitos Received=3 | **PASS** |
| RA-SPEC | Regulatory | Editar mientras esté permitido (fechas plan) | Sí | — | http=200 | **PASS** |
| RA-SPEC | Regulatory | Transiciones de preparación y envío a revisión | Sí | — | status=ReadyForSubmission | **PASS** |
| RA-SPEC | SoD/RBAC | SoD: intentar auto-aceptar requisito propio | — | Sí (debe fallar) | http=400 | **PASS** |
| RA-SPEC | SoD/RBAC | Verificar que NO pueda aprobar (UI) | — | Sí (debe fallar) | botones=0 | **PASS** |
| RA-SPEC | SoD/RBAC | Verificar que NO pueda someter (UI) | — | Sí (debe fallar) | botones=0 | **PASS** |
| RA-SPEC | API | APPROVE-INT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-SPEC | API | SUBMIT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-SPEC | API | APPROVE-EXT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-SPEC | API | BOOTSTRAP | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-SPEC | API | LICENSE | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-SPEC | #/tenant-administration | Acceso URL directa | — | Sí (debe fallar) | Acceso denegado renderizado | **PASS** |
| RA-SPEC | #/security | Acceso URL directa | — | Sí (debe fallar) | Acceso denegado renderizado | **PASS** |
| RA-SPEC | #/audit-trail | Acceso URL directa | — | Sí (debe fallar) | Acceso denegado renderizado | **PASS** |
| RA-SPEC | Regulatory | Historial/auditoría del expediente | Sí | — | eventos=6 | **PASS** |
| RA-SPEC | Auth | Cerrar sesión | Sí | — | login visible | **PASS** |
| RA-REV | Auth | Login | Sí | — | shell visible | **PASS** |
| RA-REV | Shell | Validar sidebar (menús visibles y ocultos) | Sí | — | [dashboard, regulatory] | **PASS** |
| RA-REV | Shell | Validar navbar (sesión, rol, tenant, breadcrumb) | Sí | — | chip="RA Reviewer Regulatory Reviewer" tenant="Tenant: 82af3877..." | **PASS** |
| RA-REV | Shell | Validar dashboard (widgets/KPIs/cards) | Sí | — | bloques=8 | **PASS** |
| RA-REV | Shell | Validar pestañas RA visibles/ocultas | Sí | — | [dashboard, pipeline, dossiers, registrations] | **PASS** |
| RA-REV | Regulatory | Abrir expediente recibido | Sí | — | aceptar=22 rechazar=22 | **PASS** |
| RA-REV | Regulatory | Devolver un requisito (Rechazar con comentario) | Sí | — | status=Rejected | **PASS** |
| RA-REV | Regulatory | Revisar documentos y aprobar revisión (con comentarios) | Sí | — | críticos pendientes=0 | **PASS** |
| RA-REV | SoD/RBAC | Confirmar que NO puede aprobar internamente (UI) | — | Sí (debe fallar) | botones=0 | **PASS** |
| RA-REV | SoD/RBAC | Confirmar que NO puede someter (UI) | — | Sí (debe fallar) | botones=0 | **PASS** |
| RA-REV | API | APPROVE-INT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-REV | API | SUBMIT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-REV | API | CREATE-PRODUCT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-REV | API | CREATE-DOSSIER | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-REV | API | TRANSITION | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-REV | API | OBSERVE | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-REV | #/tenant-administration | Acceso URL directa | — | Sí (debe fallar) | Acceso denegado renderizado | **PASS** |
| RA-REV | UI/DOM | Forzar "Nuevo producto + expediente" vía JavaScript | — | Sí (debe fallar) | botón ausente del DOM | **PASS** |
| RA-REV | Auth | Cerrar sesión | Sí | — | login visible | **PASS** |
| RA-APPR | Auth | Login | Sí | — | shell visible | **PASS** |
| RA-APPR | Shell | Validar sidebar (menús visibles y ocultos) | Sí | — | [dashboard, regulatory] | **PASS** |
| RA-APPR | Shell | Validar navbar (sesión, rol, tenant, breadcrumb) | Sí | — | chip="RA Approver Regulatory Approver" tenant="Tenant: 82af3877..." | **PASS** |
| RA-APPR | Shell | Validar dashboard (widgets/KPIs/cards) | Sí | — | bloques=8 | **PASS** |
| RA-APPR | Shell | Validar pestañas RA visibles/ocultas | Sí | — | [dashboard, pipeline, dossiers, registrations] | **PASS** |
| RA-APPR | Regulatory | Revisar expediente y validar estado | Sí | — | status=ReadyForSubmission indicadores=35 | **PASS** |
| RA-APPR | SoD/RBAC | Confirmar que NO puede modificar información restringida | — | Sí (debe fallar) | http=[403,403,403] | **PASS** |
| RA-APPR | Regulatory | Aprobar internamente (UI) | Sí | — | status=ApprovedForSubmission | **PASS** |
| RA-APPR | SoD/RBAC | Confirmar que NO puede someter (UI) | — | Sí (debe fallar) | botones=0 | **PASS** |
| RA-APPR | API | SUBMIT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-APPR | API | APPROVE-EXT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-APPR | API | CREATE-PRODUCT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-APPR | API | OBSERVE | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-APPR | #/tenant-administration | Acceso URL directa | — | Sí (debe fallar) | Acceso denegado renderizado | **PASS** |
| RA-APPR | UI/DOM | Forzar "Registrar sometimiento" vía JavaScript | — | Sí (debe fallar) | botón ausente del DOM | **PASS** |
| RA-APPR | Auth | Cerrar sesión | Sí | — | login visible | **PASS** |
| RA-SUB | Auth | Login | Sí | — | shell visible | **PASS** |
| RA-SUB | Shell | Validar sidebar (menús visibles y ocultos) | Sí | — | [dashboard, regulatory] | **PASS** |
| RA-SUB | Shell | Validar navbar (sesión, rol, tenant, breadcrumb) | Sí | — | chip="RA Submitter Regulatory Submitter" tenant="Tenant: 82af3877..." | **PASS** |
| RA-SUB | Shell | Validar dashboard (widgets/KPIs/cards) | Sí | — | bloques=8 | **PASS** |
| RA-SUB | Shell | Validar pestañas RA visibles/ocultas | Sí | — | [dashboard, pipeline, dossiers, registrations] | **PASS** |
| RA-SUB | Regulatory | Buscar expediente aprobado internamente | Sí | — | status=ApprovedForSubmission | **PASS** |
| RA-SUB | Regulatory | Someter a la autoridad (registra fecha de sometimiento) | Sí | — | status=Submitted submittedOn=2026-07-17 | **PASS** |
| RA-SUB | SoD/RBAC | Confirmar que NO puede editar información bloqueada | — | Sí (debe fallar) | http=[403,403,403] | **PASS** |
| RA-SUB | API | APPROVE-INT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-SUB | API | APPROVE-EXT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-SUB | API | OBSERVE | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-SUB | API | CREATE-DOSSIER | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-SUB | #/tenant-administration | Acceso URL directa | — | Sí (debe fallar) | Acceso denegado renderizado | **PASS** |
| RA-SUB | UI/DOM | Forzar "Aprobar internamente" vía JavaScript | — | Sí (debe fallar) | botón ausente del DOM | **PASS** |
| RA-SUB | Auth | Cerrar sesión | Sí | — | login visible | **PASS** |
| RA-MGR | Auth | Login | Sí | — | shell visible | **PASS** |
| RA-MGR | Regulatory | Registrar número CT/RS, resolución y fechas (modal) | Sí | — | status=Closed ctrs=MQ-E2E-61753 | **PASS** |
| RA-MGR | Auth | Cerrar sesión | Sí | — | login visible | **PASS** |
| RA-VIEW | Auth | Login | Sí | — | shell visible | **PASS** |
| RA-VIEW | Shell | Validar sidebar (menús visibles y ocultos) | Sí | — | [dashboard, regulatory] | **PASS** |
| RA-VIEW | Shell | Validar navbar (sesión, rol, tenant, breadcrumb) | Sí | — | chip="RA Viewer Regulatory Viewer" tenant="Tenant: 82af3877..." | **PASS** |
| RA-VIEW | Shell | Validar dashboard (widgets/KPIs/cards) | Sí | — | bloques=8 | **PASS** |
| RA-VIEW | Shell | Validar pestañas RA visibles/ocultas | Sí | — | [dashboard, portfolio, pipeline, dossiers, registrations, alerts] | **PASS** |
| RA-VIEW | Regulatory | Navegar por todos los módulos permitidos | Sí | — | dashboard=OK portfolio=OK pipeline=OK dossiers=OK registrations=OK alerts=OK | **PASS** |
| RA-VIEW | Regulatory | Abrir registro (expediente en lectura) | Sí | — | botones mutación=0 aviso sin acciones=true | **PASS** |
| RA-VIEW | Regulatory | Buscar y filtrar registros (API de lectura) | Sí | — | http=[200,200,200] | **PASS** |
| RA-VIEW | SoD/RBAC | Exportar | — | Sí (debe fallar) | botones export=0 | **PASS** |
| RA-VIEW | API | CREATE | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | CREATE-DOSSIER | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | EDIT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | REVIEW | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | APPROVE-INT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | SUBMIT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | APPROVE-EXT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | STATE | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | IMPORT | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | OBSERVE | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | SOD | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | MFR | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | LICENSE | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | API | TAMPER-ID | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | #/tenant-administration | Acceso URL directa | — | Sí (debe fallar) | Acceso denegado renderizado | **PASS** |
| RA-VIEW | #/security | Acceso URL directa | — | Sí (debe fallar) | Acceso denegado renderizado | **PASS** |
| RA-VIEW | #/audit-trail | Acceso URL directa | — | Sí (debe fallar) | Acceso denegado renderizado | **PASS** |
| RA-VIEW | #/superadmin-platform | Acceso URL directa | — | Sí (debe fallar) | Acceso denegado renderizado | **PASS** |
| RA-VIEW | #/configuration | Acceso URL directa | — | Sí (debe fallar) | Acceso denegado renderizado | **PASS** |
| RA-VIEW | UI/DOM | Forzar "Nuevo producto" vía JavaScript | — | Sí (debe fallar) | botón ausente del DOM | **PASS** |
| RA-VIEW | UI/DOM | Forzar "Alta fabricante" vía JavaScript | — | Sí (debe fallar) | botón ausente del DOM | **PASS** |
| RA-VIEW | UI/DOM | Forzar "Nueva licencia" vía JavaScript | — | Sí (debe fallar) | botón ausente del DOM | **PASS** |
| RA-VIEW | UI/DOM | Forzar mutación vía JavaScript (fetch manipulado con IDs alterados) | — | Sí (debe fallar) | http=403 | **PASS** |
| RA-VIEW | Auth | Cerrar sesión | Sí | — | login visible | **PASS** |

## Resumen de cobertura negativa

| Tipo de prueba negativa | Cantidad | PASS |
|---|---:|---:|
| Negativas (URL / API / JS / SoD / botones ocultos) | 60 | 60 |

**VEREDICTO DEL DOCUMENTO 10: PASS**
