# 01 — Regulatory SoD Matrix (Target)
## Compliance 360 · Regulatory Affairs

**Legenda:** ALLOWED | DENIED | CONDITIONAL | CONFLICT  
**Preparer** = Specialist · **Approver** = internal clearance · **Submitter** = registry of submission · **Manager** = operational supervision · **Admin** = configuration · **Viewer** = read-only  
**Conflicts** assume tenant SoD defaults ON (`PreventSelfReview`, `PreventSelfApproval`, `SeparateApproverAndSubmitter`).

| Acción | Preparer | Reviewer | Approver | Submitter | Manager | Admin | Viewer | Conflicto SoD |
| ------ | -------- | -------- | -------- | --------- | ------- | ----- | ------ | ------------- |
| Ver producto / dossier / CT/RS | ALLOWED | ALLOWED | ALLOWED | ALLOWED | ALLOWED | ALLOWED | ALLOWED | — |
| Crear / editar producto | ALLOWED | DENIED | DENIED | DENIED | CONDITIONAL | CONDITIONAL | DENIED | Admin sin operar por defecto |
| Crear expediente | ALLOWED | DENIED | DENIED | DENIED | ALLOWED | DENIED | DENIED | SOD-001 si luego self-review |
| Preparar / fechas / checklist prep | ALLOWED | DENIED | DENIED | DENIED | CONDITIONAL | DENIED | DENIED | — |
| Aceptar / rechazar requisito | DENIED | ALLOWED | DENIED | DENIED | CONDITIONAL | DENIED | DENIED | SOD-001 self-review |
| Waive crítico | DENIED | CONDITIONAL | CONDITIONAL | DENIED | CONDITIONAL | DENIED | DENIED | SOD-007 second approval |
| Cambiar criticidad | DENIED | DENIED | DENIED | DENIED | ALLOWED | ALLOWED | DENIED | SOD-007 |
| Declarar ReadyForSubmission | ALLOWED | ALLOWED | DENIED | DENIED | CONDITIONAL | DENIED | DENIED | — |
| Aprobar para sometimiento (interno) | DENIED | DENIED | ALLOWED | DENIED | CONDITIONAL | DENIED | DENIED | SOD-002/003 |
| Registrar sometimiento | DENIED | DENIED | DENIED | ALLOWED | CONDITIONAL | DENIED | DENIED | SOD-004 |
| Abrir observación autoridad | CONDITIONAL | CONDITIONAL | DENIED | ALLOWED | ALLOWED | DENIED | DENIED | — |
| Responder observación | ALLOWED | DENIED | DENIED | DENIED | CONDITIONAL | DENIED | DENIED | SOD-009 |
| Registrar decisión externa + CT/RS | DENIED | DENIED | DENIED | DENIED | ALLOWED | DENIED | DENIED | ≠ aprobación interna |
| Activar / suspender CT/RS | DENIED | DENIED | DENIED | DENIED | ALLOWED | DENIED | DENIED | SOD-005 |
| Config packs / autoridades / import | DENIED | DENIED | DENIED | DENIED | DENIED | ALLOWED | DENIED | SOD-006 / SOD-011 |
| Emergency override SoD | DENIED | DENIED | DENIED | DENIED | ALLOWED | DENIED | DENIED | Break-glass auditado |
| Transition a Approved/Rejected sin CT/RS | DENIED | DENIED | DENIED | DENIED | DENIED | DENIED | DENIED | CONFLICT (tapado) |

CONDITIONAL Manager = puede asignar / escalar; no sustituye Approver/Submitter salvo override auditado.
