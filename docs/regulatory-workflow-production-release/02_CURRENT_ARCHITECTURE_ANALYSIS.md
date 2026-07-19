# 02 — Análisis de arquitectura actual

## Estructura observada

- **Dominio:** `RegistrationDossier` conserva la máquina de estados y reglas en `src/Compliance360.Domain/RegulatoryAffairs/RegulatoryAffairsModels.Part2.cs`.
- **Aplicación V1:** `RegulatoryAffairsService` cubre preparación, aprobación, submission/resubmission y decisiones de autoridad.
- **Aplicación V2:** `RegulatoryWorkflowV2Service` agrega revisión optimista, correcciones acotadas, evidencia versionada, gobierno, cancelación, archivo y timeline.
- **API:** V1 bajo `/api/v1/tenants/{tenantId}/regulatory`; V2 aditiva bajo `/api/v2/tenants/{tenantId}/regulatory/dossiers`.
- **Persistencia:** EF Core/PostgreSQL; migraciones V2 para tablas regulatorias y endurecimiento de unicidad/historia.
- **Frontend:** `wwwroot/regulatory-affairs.js` consume V1 y V2 y usa recursos `locales/en.json` y `locales/es.json`.

## Límites de confianza

El aggregate es la autoridad sobre transiciones; las políticas del backend son la autoridad sobre permisos. `Revision` y `expectedRevision` coordinan mutaciones V1/V2 y los conflictos V2 se exponen como HTTP 409. El tenant del JWT se valida contra la ruta.

## Seguridad y operación

- `FileUploadSecurity` valida tamaño máximo de 25 MB, nombre, extensión, MIME y firma.
- Los JWT nuevos incluyen `session_id`; la validación consulta sesión, usuario y tenant activos.
- Login, identify, MFA complete y refresh tienen rate limiting dedicado.
- `/health/live` separa liveness; `/health/ready` incluye PostgreSQL, storage, notificaciones, colas, dead letters y data protection.
- Docker publica la web solo en `127.0.0.1:8085`, prevista para reverse proxy; PostgreSQL no publica 5432.

## Riesgos arquitectónicos pendientes de operación

- La compatibilidad temporal acepta access tokens antiguos sin `session_id`; debe retirarse mediante política de expiración/rollout.
- Las notificaciones V2 son best-effort y no revierten una transición auditada si falla el transporte.
- No existe evidencia en este corte de aplicación de la última migración ni de validación ready en VPS.
