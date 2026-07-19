# 05 — Matriz RBAC por rol

## Capacidades observadas

- **Regulatory Specialist (RA-SPEC):** crear dossier/producto según permisos, actualizar metadata, cargar evidencia, iniciar revisión técnica, atender y enviar corrección, cancelar antes de submission.
- **Regulatory Reviewer (RA-REV):** decidir requirements, solicitar corrección acotada y completar revisión técnica.
- **Regulatory Approver (RA-APP):** aprobación interna, decisiones externas permitidas y etapas de gobierno asignadas.
- **Regulatory Submitter (RA-SUB):** submission y resubmission; no obtiene aprobación por implicación.
- **Regulatory Manager (RA-MGR):** gestión regulatoria y participación autorizada en gobierno, sujeta a permisos granulares.
- **Quality Manager (QM):** aprobación/gobierno cuando posee las políticas requeridas; no sustituye controles de identidad SoD.
- **Regulatory Administrator (RA-ADM):** configuración/administración; administrar no concede review, approve u override.
- **Tenant Administrator (TAC):** administración del tenant; no concede autoridad regulatoria por sí sola.
- **Regulatory Viewer:** lectura de dossier, snapshot, timeline y versiones; sin mutaciones.

## Políticas backend relevantes

`RegulatoryPrepare`, `RegulatoryDossierUpdate`, `RegulatoryRequirementUpdate`, `RegulatoryEvidenceUpload`, `RegulatoryReview`, `RegulatoryApproveForSubmission`, `RegulatorySubmit`, `RegulatoryApprove`, `RegulatoryConfigure`, `RegulatorySoDManage` y `RegulatorySoDOverride`.

La política `RegulatoryPrepare` exige simultáneamente gestión de producto y actualización de dossier. `RegulatorySubmit` no se deriva de approve. La UI no concede autoridad.

## Evidencia negativa disponible

- `viewer-v2-negative-results.json`: seis familias mutables retornaron 403.
- `admin-v2-negative-results.json`: TAC y RA-ADM retornaron 403 en review, approve y override.

## Estado

**PASS limitado al escenario local versionado.** La matriz de asignaciones del VPS y los tokens productivos aún deben verificarse después del despliegue.
