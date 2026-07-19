# 01 — Architecture Changes

> **Estado: IMPLEMENTED / LOCALLY CERTIFIED.**  
> Implementación validada localmente el 18 de julio de 2026. El despliegue remoto todavía debe verificarse.

## Resultado

Workflow V2 quedó implementado como una extensión regulatoria especializada alrededor de `RegistrationDossier`. El aggregate continúa siendo la fuente autoritativa del estado; el servicio V2 aplica reglas de dominio, revisión esperada, alcance de corrección y gobierno antes de persistir.

## Componentes implementados

- Dominio: `RegulatoryWorkflowV2Models.cs` y extensiones de `RegistrationDossier`.
- Aplicación: `RegulatoryWorkflowV2Service.cs` y `RegulatoryWorkflowV2Contracts.cs`.
- Persistencia: `EfRegulatoryWorkflowV2Repository.cs` y mappings de EF Core.
- API aditiva: `/api/v2/tenants/{tenantId}/regulatory/dossiers/...`.
- Frontend: integración V2 en `regulatory-affairs.js`, con traducciones en español e inglés.
- Base de datos: migración `20260718115954_AddRegulatoryWorkflowV2ControlledFlexibility`.

## Controles arquitectónicos certificados

- Creación real en `Draft`, sin avance automático.
- Concurrencia optimista mediante `Revision`/`expectedRevision`; una revisión obsoleta retorna HTTP `409 Conflict` sin persistencia parcial.
- Flujo de revisión técnica con `UnderTechnicalReview` y `CorrectionRequested`.
- Correcciones restringidas por requirements, fields y documents.
- Evidencia versionada con SHA-256, versión activa y versiones supersedidas preservadas.
- Timeline V2 secuencial y append-only con actor, razón, correlación y transición.
- Reapertura y override modelados como solicitudes separadas, con solicitante segregado y dos aprobadores distintos.
- Archivo lógico hacia `Archived`, preservando la línea de tiempo.
- Autorización de servidor por políticas existentes y separación funcional comprobada.

## Compatibilidad y límites

V1 permanece disponible para operaciones regulatorias existentes y V2 añade endpoints controlados sin renumerar estados históricos. La certificación cubre el comportamiento implementado y ejecutado por las pruebas; no certifica componentes originalmente propuestos que no forman parte de esta entrega, como outbox regulatoria dedicada, manifests V2 de sometimiento, step-up authentication o despliegue remoto.

## Evidencia

La evidencia ejecutable y sus capturas se conserva en `docs/regulatory-workflow-v2/evidence`. Los resultados consolidados están en `08_Implementation_Report.md`, `09_Test_Evidence.md` y `10_Final_Certification.md`.
