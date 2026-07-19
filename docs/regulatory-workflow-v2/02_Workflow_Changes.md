# 02 — Workflow Changes

> **Estado: IMPLEMENTED / CERTIFIED FOR LOCAL FUNCTIONAL USE.**  
> La verificación de despliegue remoto queda pendiente.

## Flujo implementado

```text
Draft
  → Planning
  → WaitingManufacturerDocuments
  → DocumentsReceived
  → Assembling
  → UnderTechnicalReview
  ├→ CorrectionRequested → UnderTechnicalReview
  └→ ReadyForSubmission
```

La creación desde la UI produce un dossier `Draft` con pack de requisitos. El draft puede persistirse y actualizar metadatos sin simular una transición. La regresión confirmó el avance controlado hasta `UnderTechnicalReview`.

## Revisión y corrección controlada

- El Reviewer crea una solicitud desde `UnderTechnicalReview`.
- La solicitud exige motivo, severidad y scope no vacío.
- El scope admite requirements, fields y documents concretos.
- En `CorrectionRequested`, el Specialist solo puede modificar elementos incluidos.
- Una carga fuera de scope se rechaza y no incrementa la revisión.
- Para enviar la corrección, los requirements incluidos requieren evidencia activa asociada a la solicitud.
- El envío retorna a `UnderTechnicalReview`; el Reviewer cierra la corrección y lleva el dossier a `ReadyForSubmission` cuando los requisitos obligatorios están aceptados, waived o no aplican.

## Evidencia

Cada reemplazo crea una nueva `DossierEvidenceRevision`. La versión anterior queda `Superseded`, la nueva queda `Active`, ambas mantienen `StoredFileId`, SHA-256, actor, fecha, nombre y razón. La ejecución certificada creó V1 y V2 con hashes reales y confirmó que ambas versiones siguen consultables.

## Concurrencia

Los comandos V2 reciben `expectedRevision`. Si no coincide con la revisión persistida, la API devuelve `409 Conflict`; la prueba confirmó que el payload obsoleto no se guarda y que la revisión permanece intacta.

## Gobierno

- Reopen: solo para dossiers terminales; el solicitante no puede aprobar, un aprobador no puede ocupar ambas etapas y la ejecución exige dos aprobaciones distintas. La reapertura certificada terminó en `CorrectionRequested`.
- Override: solicitud separada por acción y razón; solicitante segregado, dos aprobadores distintos y consumo de una sola vez.
- Archive: desde `Closed` a `Archived` mediante soft archive, sin eliminar eventos anteriores.

## Alcance no certificado

`ResponseReady`, manifests V2 de sometimiento, outbox dedicada, expiración de solicitudes y step-up authentication pertenecían al diseño objetivo, pero no se presentan como implementados ni certificados por esta entrega.
