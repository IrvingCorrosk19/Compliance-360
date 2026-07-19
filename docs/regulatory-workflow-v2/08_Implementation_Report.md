# 08 — Implementation Report

> **Estado: IMPLEMENTED.**  
> Fecha de cierre local: 18 de julio de 2026.

## Resumen

Workflow V2 fue implementado en dominio, aplicación, persistencia, API y frontend. La entrega añade flexibilidad controlada al ciclo regulatorio sin reemplazar `RegistrationDossier` como fuente de verdad.

## Backend

- Modelos V2 para correction requests/scope, revisiones de evidencia, reopen, override, aprobaciones y change events.
- Servicio V2 para snapshot, metadata, correcciones, revisión técnica, evidencia, gobierno, archivo y timeline.
- Repository EF con aislamiento por tenant.
- API `/api/v2/tenants/{tenantId}/regulatory/dossiers`.
- Conversión explícita de conflictos de revisión a HTTP `409`.
- Incremento de `Revision` también en operaciones V1 que mutan el mismo dossier, evitando desalineación al combinar rutas V1 y V2.

## Frontend

- Integración del snapshot y timeline V2 en Regulatory Affairs.
- Creación real de producto+dossier como `Draft`.
- Visualización de las etapas `UnderTechnicalReview`, `CorrectionRequested` y `ReadyForSubmission`.
- Recursos localizados en español e inglés.
- Uso de revisión del servidor para comandos mutables.

## Base de datos

La migración `20260718115954_AddRegulatoryWorkflowV2ControlledFlexibility` agrega `Revision` y las tablas de eventos, correcciones/scope, evidencia versionada, reapertura, override y aprobaciones. Las relaciones históricas usan `Restrict`; las restricciones e índices protegen secuencia, versiones y separación de aprobadores. El downgrade destructivo está deshabilitado.

## Capacidades entregadas

- Draft persistente.
- Concurrencia optimista y `409 Conflict`.
- Revisión técnica.
- Corrección de alcance limitado.
- Evidencia versionada con SHA-256.
- Timeline append-only.
- Reopen y override con dos aprobadores distintos del solicitante.
- Override consumible una vez.
- Soft archive con conservación histórica.
- RBAC y SoD en backend.

## Defectos cerrados durante regresión

1. El conflicto de revisión, inicialmente tratable como error funcional genérico, quedó mapeado al contrato exacto HTTP `409`.
2. Las mutaciones V1 del dossier quedaron sincronizadas con `Revision`, evitando que una transición o actualización V1 dejara una revisión V2 obsoleta sin señal.
3. La aplicación de requirement pack dejó de forzar avance: el dossier permanece en `Draft`.
4. La corrección quedó cerrada contra escrituras fuera de scope y contra envíos sin evidencia activa asociada.
5. El reemplazo de evidencia dejó de comportarse como sobrescritura: V1 queda superseded y V2 activa, ambas con SHA-256 preservado.
6. Se cerraron brechas de autorización verificando `403` para Viewer, TAC y RA-ADM en mutaciones incompatibles.
7. El archivo quedó como soft archive y el timeline anterior se conserva sin alteración.

## Exclusiones

No se declara implementada una outbox dedicada, manifests V2, step-up authentication, expiración de solicitudes ni despliegue remoto. Estas exclusiones no invalidan el alcance funcional certificado, pero no deben inferirse como completadas.
