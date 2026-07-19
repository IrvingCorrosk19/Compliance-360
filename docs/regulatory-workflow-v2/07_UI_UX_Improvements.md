# 07 — UI/UX Improvements

> **Estado: IMPLEMENTED / FUNCTIONALLY VALIDATED.**

## Experiencia implementada

La consola Regulatory Affairs integra Workflow V2 sobre la UI existente. El escenario certificado utilizó la interfaz real para crear producto y dossier, sin fallback de preparación por API, y confirmó que el dossier persiste como `Draft`.

La vista de dossier muestra el contexto Workflow V2, consulta snapshot/timeline y conserva la separación por rol. Los textos incorporados tienen claves en `locales/es.json` y `locales/en.json`.

## Comportamientos validados

- Creación de producto y dossier desde modal real.
- Dossier inicial `Draft` con requisitos.
- Visualización del caso en `UnderTechnicalReview`.
- Visualización de `CorrectionRequested`.
- Finalización visible de revisión técnica en `ReadyForSubmission`.
- Timeline V2 consultable por un Viewer.
- Acciones mutables denegadas por backend aunque se invoquen directamente.
- Capturas de las etapas principales en `evidence/*.png`.

## Concurrencia y edición

Los writes V2 envían `expectedRevision`. Un conflicto obsoleto produce HTTP `409` y no sobrescribe el trabajo persistido. La UI consume la revisión devuelta por el servidor para las acciones siguientes.

## Correcciones y evidencia

El flujo distingue el requirement incluido del que queda fuera de scope. La API bloquea la carga fuera de alcance; las versiones permitidas quedan consultables con número, SHA-256, estado actual/superseded y razón del reemplazo.

## Seguridad de interfaz

La UI no es una barrera de autorización. Viewer, TAC y RA-ADM fueron probados contra endpoints mutables y recibieron `403` según el rol. Los controles de reopen y override dependen de políticas y validaciones de dominio, no de ocultamiento visual.

## Límite de certificación

La certificación funcional cubre chromium y los recorridos capturados. No constituye una certificación independiente WCAG 2.2 AA, responsive exhaustiva ni despliegue remoto.
