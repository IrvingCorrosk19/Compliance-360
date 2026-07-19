# 08 — Versionado documental

## Modelo observado

Cada `DossierEvidenceRevision` conserva requirement, correction request opcional, document opcional, `StoredFileId`, SHA-256, nombre, motivo, usuario, fecha, número de versión, indicador current y estado.

Una sustitución no sobrescribe la evidencia anterior: marca la previa `Superseded` y crea una nueva `Active`. El índice parcial único impide dos versiones current para el mismo tenant+requirement.

## Evidencia real

`docs/regulatory-workflow-v2/evidence/evidence-version-history.json` contiene:

- Versión 1, archivo `workflow-v2-evidence-v1.pdf`, estado `Superseded`, `isCurrent: false`, hash SHA-256 persistido.
- Versión 2, archivo `workflow-v2-evidence-v2.pdf`, estado `Active`, `isCurrent: true`, hash SHA-256 distinto.

`workflow-v2-final.json` registra PASS local de carga real V1/V2 y rechazo de evidencia fuera del scope de corrección.

## Controles de upload

Antes de almacenamiento, `FileUploadSecurity` valida tamaño, nombre, extensión, MIME y firma. El storage calcula SHA-256; el servicio V2 verifica que `StoredFileId`, nombre y SHA-256 correspondan al archivo almacenado.

## Estado

**PASS local para el escenario versionado.** Falta repetir el escenario contra el artefacto desplegado y verificar acceso/retención del volumen productivo.
