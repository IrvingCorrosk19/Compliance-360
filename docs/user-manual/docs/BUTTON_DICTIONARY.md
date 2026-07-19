# BUTTON_DICTIONARY

| Label | Screen | Roles | Permission | Result |
|-------|--------|-------|------------|--------|
| Iniciar sesion / Completar login seguro | login | * | — | Token + redirección al dashboard |
| Nuevo producto + expediente | regulatory-portfolio | regulatory-specialist, regulatory-administrator | PRODUCT.MANAGE + DOSSIER.CREATE | Producto en portafolio y expediente en Planning |
| Pedir docs fábrica | regulatory-dossier-detail | regulatory-specialist | DOSSIER.UPDATE | Estado Espera docs fábrica |
| Docs recibidos | regulatory-dossier-detail | regulatory-specialist | DOSSIER.UPDATE | Estado Docs recibidos |
| Armar | regulatory-dossier-detail | regulatory-specialist | DOSSIER.UPDATE | Estado Armando expediente |
| Declarar técnicamente completo | regulatory-dossier-detail | regulatory-specialist | DOSSIER.UPDATE | Listo para aprobación interna |
| Marcar recibido | regulatory-dossier-detail | regulatory-specialist | REQUIREMENT.MANAGE / UPDATE | Requisito marcado recibido |
| Aceptar | regulatory-dossier-detail | regulatory-reviewer | DOSSIER.REVIEW | Requisito aceptado |
| Rechazar | regulatory-dossier-detail | regulatory-reviewer | DOSSIER.REVIEW | Requisito rechazado; Specialist corrige |
| Aprobar internamente para sometimiento | regulatory-dossier-detail | regulatory-approver | APPROVE_FOR_SUBMISSION | ApprovedForSubmission — NO es aprobación MINSA/CSS |
| Registrar sometimiento | regulatory-dossier-detail | regulatory-submitter | DOSSIER.SUBMIT | Submitted — sometimiento registrado |
| Registrar observación autoridad | regulatory-dossier-detail | regulatory-manager | OBSERVATION.MANAGE | Observed — observación recibida de la autoridad |
| Responder | regulatory-dossier-detail | regulatory-specialist | OBSERVATION.MANAGE | CorrectingObservation / respuesta registrada |
| Registrar aprobación MINSA/CSS + CT/RS | regulatory-dossier-detail | regulatory-manager, quality-manager | DOSSIER.APPROVE | Approved + CT/RS activo |
| Alta fabricante | regulatory-manufacturers | regulatory-specialist, regulatory-administrator | MANUFACTURER_DOCUMENT.MANAGE | Fabricante listado (+ cert ISO13485 en flujo demo) |
| Nueva licencia | regulatory-licenses | regulatory-administrator | LICENSE.MANAGE | Licencia en listado |
| Stage XLSX | regulatory-import | regulatory-administrator | CONFIGURE | Job de importación en staging |
| Bootstrap regulatorio | regulatory-config | regulatory-administrator, tenant-administrator | CONFIGURE | MINSA/CSS + pack de requisitos |
| ← Expedientes | regulatory-dossier-detail | * | — | Vista Expedientes |
| Campana Alert Center | alert-center-inbox | * | NOTIFICATION.READ | Abre Inbox de notificaciones |
| Marcar leída / Marcar todas leídas | alert-center-inbox | * | NOTIFICATION.READ | Estado Unread → Read |
| Marcar favorito | alert-center-inbox | * | NOTIFICATION.READ | Ítem marcado como favorito |
| Archivar | alert-center-inbox | * | NOTIFICATION.READ | Ítem archivado |
| Template Center | alert-center-templates | notification-administrator | NOTIFICATION.TEMPLATE / ADMIN | Lista de plantillas versionadas |
| Crear regla | alert-center-rules | notification-administrator | NOTIFICATION.MANAGE | Borrador de regla creado |
| Configurar proveedor | alert-center-providers | notification-administrator | NOTIFICATION.ADMIN | Proveedor configurado (secretos write-only) |
| Reintentar / acciones de mensaje | alert-center-operations | notification-administrator | NOTIFICATION.MANAGE | Mensaje reencolado o acción aplicada |
| Exportar CSV | alert-center-operations | notification-administrator | NOTIFICATION.READ / MANAGE | Descarga CSV operativo |
