# 05 — Matriz de estados y flujo

## Criterio

Derivada exclusivamente de `docs/user-manual/data/workflows.json`, `buttons.json`, `assets/js/simulator.js`, páginas por rol y documentos del manual. Es una especificación esperada pendiente de ejecución; no se declara PASS.

## Flujo principal esperado

`Planning → WaitingManufacturerDocuments → DocumentsReceived → Assembling → ReadyForSubmission → ApprovedForSubmission → Submitted → UnderAuthorityReview → Observed → CorrectingObservation → Resubmitted → Approved → CT/RS activo → Renovación`

`Draft` y `Rejected` aparecen en las etiquetas de estado, pero el manual no define cómo entran o salen del flujo. `SanitaryRegistration` aparece como estado de CT/RS en el workflow, pero no está en `statusLabels`.

## Matriz de transición

| Origen | Acción | Destino/efecto | Rol esperado | Permiso | API/operación | Permitida para | Prohibida para |
|---|---|---|---|---|---|---|---|
| — | Crear producto+expediente | `Planning` | RA-SPEC; RA-ADM según botón | `PRODUCT.MANAGE + DOSSIER.CREATE` | `POST product + POST dossier` | RA-SPEC, RA-ADM | TAC, RA-MGR, RA-REV, RA-APPR, RA-SUB, RA-VIEW, QM |
| `Planning` | Pedir docs | `WaitingManufacturerDocuments` | RA-SPEC | `DOSSIER.UPDATE` | `POST …/transition` | RA-SPEC | Todos los demás |
| No definida | Docs recibidos | `DocumentsReceived` | RA-SPEC | `DOSSIER.UPDATE` | `POST …/transition` | RA-SPEC | Todos los demás |
| No definida | Armar | `Assembling` | RA-SPEC | `DOSSIER.UPDATE` | `POST …/transition` | RA-SPEC | Todos los demás |
| Requisito pendiente | Marcar recibido | Requisito `Received` | RA-SPEC | `REQUIREMENT.MANAGE / UPDATE` | `PUT requirement` | RA-SPEC | Todos los demás |
| Requisito recibido/en revisión | Aceptar | Requisito `Accepted` | RA-REV | `DOSSIER.REVIEW` | `PUT requirement` | RA-REV, sujeto a SoD | Todos los demás |
| Requisito recibido/en revisión | Rechazar | Requisito `Rejected` | RA-REV | `DOSSIER.REVIEW` | `PUT requirement` | RA-REV, sujeto a SoD | Todos los demás |
| `Assembling`/precondición no detallada | Declarar completo | `ReadyForSubmission` | RA-SPEC | `DOSSIER.UPDATE` | `POST …/transition` | RA-SPEC | Todos los demás |
| `ReadyForSubmission` | Aprobar internamente | `ApprovedForSubmission` | RA-APPR | `APPROVE_FOR_SUBMISSION` | `POST …/approve-for-submission` | RA-APPR | TAC, RA-ADM, RA-MGR, RA-SPEC, RA-REV, RA-SUB, RA-VIEW, QM |
| `ApprovedForSubmission` | Registrar sometimiento | `Submitted` | RA-SUB | `DOSSIER.SUBMIT` | `POST …/submit` | RA-SUB | Todos los demás |
| `Submitted` | Seguimiento/transición | `UnderAuthorityReview` | RA-MGR | No especificado | No especificada | RA-MGR esperado | No definido |
| `Submitted`/`UnderAuthorityReview` | Registrar observación | `Observed` | RA-MGR | `OBSERVATION.MANAGE` | `POST …/observations` | RA-MGR | Todos los demás |
| `Observed` con observación abierta | Responder | `CorrectingObservation` | RA-SPEC | `OBSERVATION.MANAGE` | `POST …/observations/{id}/respond` | RA-SPEC | Todos los demás |
| `CorrectingObservation` | Resometer | `Resubmitted` | RA-SUB | `DOSSIER.SUBMIT` por asociación | `POST …/submit` reutilizado | RA-SUB esperado | Todos los demás |
| Sometido/revisado; estado exacto no definido | Registrar decisión externa | `Approved` + CT/RS | RA-MGR o QM | `DOSSIER.APPROVE` | `POST …/approve` | RA-MGR, QM | TAC, RA-ADM, RA-SPEC, RA-REV, RA-APPR, RA-SUB, RA-VIEW |
| `Approved` | Activar/consultar CT/RS | `SanitaryRegistration` según workflow | RA-MGR/QM | `REGISTRATION.MANAGE` solo declarado | No especificada | RA-MGR, QM | No definido |
| CT/RS activo | Iniciar renovación | `Renovacion` (etiqueta no normalizada) | RA-MGR | `REGISTRATION.MANAGE` por descripción | `StartRenewal` sin contrato | RA-MGR | Todos los demás |

## Visibilidad y modificación de estados por rol

| Rol | Estados esperados visibles | Estados/requisitos modificables |
|---|---|---|
| TAC | Lectura RA no delimitada | Ninguno |
| RA-ADM | Lecturas no delimitadas; `Planning` tras creación | Solo creación en `Planning` |
| RA-MGR | Pipeline completo, sometidos, observados, CT/RS y renovación | `Observed`, `Approved`, renovación; `UnderAuthorityReview` esperado pero sin contrato |
| RA-SPEC | Preparación, expedientes, pipeline y observaciones | `Planning` a `ReadyForSubmission`; requisito `Received`; `CorrectingObservation` |
| RA-REV | `Assembling` / `ReadyForSubmission` y consultas | Requisito `Accepted`/`Rejected`, no estado de expediente definido |
| RA-APPR | `ReadyForSubmission` y consultas | `ApprovedForSubmission` |
| RA-SUB | `ApprovedForSubmission`, sometidos y consultas | `Submitted`; `Resubmitted` esperado |
| RA-VIEW | Estados transversales visibles | Ninguno |
| QM | Sometidos, decisión externa y CT/RS | `Approved` + CT/RS |

## Reglas SoD vinculadas al flujo

1. RA-SPEC no revisa su propio expediente (`PreventSelfReview`).
2. RA-APPR y RA-SUB deben ser personas/funciones separadas (`SeparateApproverAndSubmitter`).
3. TAC no sustituye roles operativos.
4. RA-MGR no concentra Specialist+Approver+Submitter sin override de emergencia auditado.
5. La aprobación interna (`ApprovedForSubmission`) nunca debe presentarse como decisión externa (`Approved`).
6. RA-VIEW no modifica estados ni requisitos.

## Transiciones prohibidas por rol

- **TAC:** todas las transiciones del expediente.
- **RA-ADM:** todas salvo creación inicial en `Planning`.
- **RA-MGR:** preparación, revisión, aprobación interna y sometimiento ordinario; override solo según política no detallada.
- **RA-SPEC:** revisión, aprobación interna, sometimiento/resometimiento y decisión externa.
- **RA-REV:** toda transición de expediente; únicamente cambia dictamen de requisitos.
- **RA-APPR:** todas salvo `ReadyForSubmission → ApprovedForSubmission`.
- **RA-SUB:** todas salvo sometimiento y resometimiento esperado.
- **RA-VIEW:** todas.
- **QM:** todas salvo decisión externa/CT-RS.

## Contradicciones y vacíos internos

1. `Docs recibidos`, `Armar` y `Declarar técnicamente completo` carecen de precondición explícita en `buttons.json`, aunque el flujo implica secuencia.
2. Revisión figura en `Assembling / ReadyForSubmission`, pero RA-SPEC es quien declara `ReadyForSubmission`; el manual no define una transición formal de entrada/salida de revisión.
3. `Submitted → UnderAuthorityReview` no tiene botón, permiso ni API.
4. Resometimiento usa conceptualmente `POST …/submit`, pero la precondición publicada de esa acción es únicamente `ApprovedForSubmission`.
5. Aprobación externa no tiene estado origen documentado en el diccionario, solo campos CT/RS.
6. `Approved + CT/RS activo` se presenta como un resultado único, pero el workflow agrega `SanitaryRegistration` como estado separado.
7. `Draft`, `Rejected`, `SanitaryRegistration` y `Renovacion` no tienen transiciones completas ni un conjunto uniforme de etiquetas.
8. El simulador no valida precondiciones y hace que «Marcar recibido» también cambie el expediente a `DocumentsReceived`, mientras el diccionario trata estado de requisito y transición de expediente como acciones distintas.
