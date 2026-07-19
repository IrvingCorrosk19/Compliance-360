# 03 — Cambios implementados

## Seguridad

- Nuevo `FileUploadSecurity.cs`: perfiles por caso de uso, límite de 25 MB, rechazo de path traversal, allowlist de extensiones/MIME y magic bytes para PDF, PNG, JPEG, Office Open XML y Office legado.
- Validación integrada en evidencia regulatoria, documentos generales e importación REGUTRACK.
- JWT emitido con `session_id`; validación de bearer contra sesión no revocada, usuario activo y tenant activo.
- Refresh y logout revocan la sesión enlazada; cambio de contraseña revoca sesiones y refresh tokens activos.
- Rate limiting: política general API y política de autenticación por IP; producción limita autenticación a 10 solicitudes/minuto.

## Operación

- Health separados en `/health/live` y `/health/ready`.
- Ready incluye dependencias de datos, storage, proveedores/colas de notificación y data protection.
- `docker-compose.yml` no expone PostgreSQL y publica web en loopback `127.0.0.1:8085:8080`.
- Forwarded headers habilitados para operación detrás del proxy del host.

## Workflow regulatorio

- Inicio gobernado de revisión técnica desde `Assembling`, bloqueado si falta evidencia requerida.
- Corrección controlada por requirements, fields y documents; envío exige cubrir exactamente el scope y evidencia activa.
- Resubmission con comprobante, procedimiento, número externo y control SoD.
- Inicio explícito de revisión de autoridad y rechazo con motivo, resolución y documento.
- Cancelación lógica pre-submission con motivo obligatorio y conservación histórica.
- Notificaciones in-app V2 para corrección solicitada/enviada, revisión completada, reapertura y cancelación.
- DTO `DossierDetailDto` expone metadata, fechas, datos de submission y `Revision`.

## Persistencia y gobierno

- Índices parciales únicos para una solicitud activa de reopen, override, correction y una evidencia current por requirement.
- Triggers PostgreSQL impiden UPDATE/DELETE en `dossier_change_events` y `dossier_history_events`.
- V1 incrementa `Revision` al mutar el mismo dossier, evitando desincronización con V2.
