# 06 — Database Impact

> **Estado: IMPLEMENTED / LOCALLY VALIDATED.**  
> Migración implementada: `20260718115954_AddRegulatoryWorkflowV2ControlledFlexibility`.

## Cambios aplicados por la migración

- Columna `Revision bigint` en `registration_dossiers`.
- `dossier_change_events` para timeline secuencial.
- `dossier_correction_requests` y `dossier_correction_scope_items`.
- `dossier_evidence_revisions`.
- `dossier_reopen_requests` y `dossier_reopen_approvals`.
- `dossier_override_requests` y `dossier_override_approvals`.

Las foreign keys históricas usan `Restrict`. Existen índices por tenant/dossier/estado, unicidad de secuencia del timeline, unicidad de versión por requirement y unicidad de aprobador/etapa en solicitudes de gobierno.

## Concurrencia

`Revision` se incrementa en mutaciones V2 y los comandos requieren `expectedRevision`. El conflicto se traduce a HTTP `409`. La regresión confirmó ausencia de cambios parciales al enviar una revisión obsoleta.

## Evidencia versionada

`dossier_evidence_revisions` conserva `RequirementId`, correction request opcional, `DocumentId` opcional, `StoredFileId`, SHA-256, nombre, razón, actor, fecha, número de versión, `IsCurrent` y estado. Una sustitución marca la versión previa como `Superseded`; no la sobrescribe.

## Timeline append-only

`dossier_change_events` tiene secuencia única por `(TenantId, DossierId, Sequence)`, tipo, estados origen/destino, actor, campo, razón, correlación y fecha. La aplicación agrega eventos y la migración no ofrece un downgrade destructivo: `Down` falla de forma explícita para proteger historia regulatoria.

## Reopen, override y archivo

Las solicitudes y aprobaciones se persisten por separado. Los índices únicos impiden repetir aprobador o etapa. El archivo usa el estado `Archived` y soft delete del dossier; las relaciones `Restrict` y el endpoint de timeline permiten conservar la evidencia histórica.

## Alcance

La migración no crea una outbox regulatoria dedicada ni manifests V2 de sometimiento. Tampoco se afirma que haya sido aplicada en un ambiente remoto; la certificación corresponde al ambiente local validado.
