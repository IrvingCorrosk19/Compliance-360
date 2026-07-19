# 04 — Matriz de estados del workflow

## Estados persistidos

- 0 `Draft`; 1 `Planning`; 2 `WaitingManufacturerDocuments`; 3 `DocumentsReceived`; 4 `Assembling`.
- 5 `ReadyForSubmission`; 16 `ApprovedForSubmission`; 6 `Submitted`; 7 `UnderAuthorityReview`.
- 8 `Observed`; 9 `CorrectingObservation`; 19 `ResponseReady`; 10 `Resubmitted`.
- 11 `Approved`; 12 `Rejected`; 13 `Cancelled`; 14 `Closed`; 15 `OnHold`; 20 `Archived`.
- 17 `UnderTechnicalReview`; 18 `CorrectionRequested`.

## Transiciones clave verificadas en código

- Preparación: `Draft → Planning → WaitingManufacturerDocuments → DocumentsReceived → Assembling`.
- Revisión técnica: `Assembling → UnderTechnicalReview`.
- Corrección: `UnderTechnicalReview → CorrectionRequested → UnderTechnicalReview`.
- Cierre técnico: `UnderTechnicalReview → ReadyForSubmission`.
- Submission: `ReadyForSubmission → ApprovedForSubmission → Submitted`; configuración sin aprobación interna permite `ReadyForSubmission → Submitted`.
- Autoridad: `Submitted/Resubmitted → UnderAuthorityReview → Observed|Approved|Rejected`.
- Respuesta: `Observed → CorrectingObservation → ResponseReady|Resubmitted`; `ResponseReady → Resubmitted`.
- Cierre: `Approved|Rejected → Closed`; `Closed → Archived`.
- Cancelación lógica: estados pre-submission admitidos por V2 → `Cancelled`.
- Reapertura gobernada: `Closed|Rejected|Cancelled → CorrectionRequested`.

## Guards relevantes

- Inicio técnico exige estado `Assembling`, revisión vigente y requirements obligatorios recibidos/aceptados/waived/no requeridos.
- Completar revisión exige requirements obligatorios aceptados/waived/no requeridos y correction response presentada cuando corresponda.
- Rechazo de autoridad exige estado de autoridad, motivo mínimo, número de resolución y archivo de resolución.
- Cancelación exige motivo de 8–2000 caracteres y solo opera antes de submission.
- Todas las mutaciones V2 exigen `expectedRevision`.

## Evidencia ejecutada

`workflow-v2-final.json` certifica localmente Draft, revisión técnica, corrección, ready, reopen y archive. Los circuitos completos de autoridad, resubmission y cancelación existen en código/pruebas focalizadas, pero no se declaran como smoke productivo.
