# 09 — Certificación de audit trail

## Trazabilidad implementada

Las mutaciones V2 generan `DossierChangeEvent` con secuencia, tipo, actor, rol opcional, estado origen/destino, campo, motivo, correlation ID y timestamp. En paralelo, `SaveAudit` agrega `AuditLog` con contexto de request; el middleware propaga tenant, usuario, sesión, IP, user-agent y correlación cuando están disponibles.

## Inmutabilidad

- Índice único por secuencia evita duplicados dentro del dossier.
- Triggers PostgreSQL rechazan UPDATE y DELETE tanto en `dossier_change_events` como en `dossier_history_events`.
- Archivo y cancelación son lógicos; no eliminan la evidencia histórica.

## Evidencia real

- `timeline.json`: secuencias 1–8 para metadata, inicio técnico, corrección, dos revisiones de evidencia, envío y cierre técnico.
- `archived-timeline.json`: conserva evento previo y agrega `DossierArchived`.
- `workflow-v2-final.json`: fase local “timeline is append-only, sequential and complete” marcada PASS.
- `stale-revision-response.json`: conflicto HTTP 409 con revisión esperada/actual.

## Verificación productiva

La regresión productiva 72/72 consultó timeline, versionado, reopen, override y archive. El VPS registra las migraciones `AddRegulatoryWorkflowV2ControlledFlexibility` y `HardenRegulatoryWorkflowV2Governance`; no se observaron excepciones no controladas ni pérdida de persistencia.

## Veredicto

**PASS — audit trail y timeline certificados en producción.**
