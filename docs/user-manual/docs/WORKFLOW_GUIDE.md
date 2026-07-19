# WORKFLOW_GUIDE

Flujo certificado del expediente

```
Preparación → Revisión técnica → Aprobación interna para sometimiento → Sometimiento → Revisión de autoridad → Observación → Respuesta → Resometimiento → Decisión externa → CT/RS → Vigencia → Renovación
```

## Preparación
- Estado: `Planning…ReadyForSubmission`
- Rol: Regulatory Specialist
- UI: Pedir docs fábrica, Docs recibidos, Armar, Marcar recibido, Declarar técnicamente completo

## Revisión técnica
- Estado: `Assembling / ReadyForSubmission`
- Rol: Regulatory Reviewer
- UI: Aceptar, Rechazar

## Aprobación interna para sometimiento
- Estado: `ReadyForSubmission → ApprovedForSubmission`
- Rol: Regulatory Approver
- UI: Aprobar internamente para sometimiento

## Sometimiento
- Estado: `ApprovedForSubmission → Submitted`
- Rol: Regulatory Submitter
- UI: Registrar sometimiento

## Revisión de autoridad
- Estado: `Submitted → UnderAuthorityReview`
- Rol: Regulatory Manager (seguimiento)
- UI: (seguimiento / transición)

## Observación
- Estado: `Observed`
- Rol: Regulatory Manager
- UI: Registrar observación autoridad

## Respuesta
- Estado: `CorrectingObservation`
- Rol: Regulatory Specialist
- UI: Responder

## Resometimiento
- Estado: `Resubmitted`
- Rol: Regulatory Submitter / transición
- UI: Registrar sometimiento / transition Resubmitted

## Decisión externa
- Estado: `Approved`
- Rol: Regulatory Manager / Quality Manager
- UI: Registrar aprobación MINSA/CSS + CT/RS

## CT/RS activo
- Estado: `SanitaryRegistration`
- Rol: Manager / QM
- UI: Registros CT/RS

## Renovación
- Estado: `Renovacion (pipeline)`
- Rol: Regulatory Manager
- UI: Pipeline Renovacion / StartRenewal

## Alert Center (paralelo al expediente)

Configuración del canal (antes o en paralelo a la operación RA):

```
Tenant Administrator asigna NA → Notification Administrator configura
Templates / Reglas / Destinatarios / Scheduler / Providers
→ Eventos de dominio (RA dual-write) → Worker → Inbox / email
```

| Capacidad | Rol | Ruta |
|-----------|-----|------|
| Inbox (leer/marcar/archivar/favorito) | Roles con `NOTIFICATION.READ` (RA, QM, Viewer, NA…) | `#/alert-center` / campana |
| Template Center, Reglas, Destinatarios, Scheduler, Operaciones | Notification Administrator | `#/alert-center` → consolas |
| Provider Center (SMTP/cloud) | Notification Administrator | `#/alert-center` → Proveedores |
| Storage documental | Storage Administrator (**no** NA) | Storage |

**No confundir:** la vista RA «Alertas» (`#/regulatory → Alertas`) es riesgo/vencimiento del módulo regulatorio. El **Alert Center** es el canal empresarial de notificaciones.

## Etiquetas UI (regulatory-affairs.js STATUS_LABELS)
- `Draft` → Borrador
- `Planning` → Planificación / Preparación
- `WaitingManufacturerDocuments` → Espera docs fábrica
- `DocumentsReceived` → Docs recibidos
- `Assembling` → Armando expediente
- `ReadyForSubmission` → Técnicamente completo / Listo para aprobación interna
- `ApprovedForSubmission` → Aprobado internamente para sometimiento
- `Submitted` → Sometido ante autoridad
- `UnderAuthorityReview` → En revisión de la autoridad
- `Observed` → Observado por autoridad
- `CorrectingObservation` → Corrigiendo observación
- `Resubmitted` → Resometido
- `Approved` → Aprobación registrada de MINSA/CSS (externa)
- `Rejected` → Rechazo externo de la autoridad
