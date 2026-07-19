# 08 — Workflow Gate Analysis

## AS-IS (dominio)

```
Draft → Planning → WaitingMfrDocs → DocumentsReceived → Assembling
  → ReadyForSubmission → Submitted → UnderAuthorityReview
  → Observed | Approved | Rejected → Closed
```

`Approved` = decisión de autoridad + CT/RS vía `ApproveDossierAsync`.  
**Hueco:** no hay gate de autorización interna.

## TARGET (v1)

```
… → ReadyForSubmission
       → ApprovedForSubmission   «APPROVE_FOR_SUBMISSION / Approver»
       → Submitted               «SUBMIT / Submitter»
       → UnderAuthorityReview
       → Observed | Approved* | Rejected
*Approved = EXTERNAL only; crea SanitaryRegistration
```

Observaciones: se mantienen estados existentes (Observed → Correcting → Resubmitted). Sub-flujo RESPONSE_* diferido a tareas/history (evitar inflación de enum sin UI).

## Gates de servicio

| De → A | Permiso | SoD |
|--------|---------|-----|
| * → Accepted/Rejected req | REVIEW | PreventSelfReview |
| Ready → ApprovedForSubmission | APPROVE_FOR_SUBMISSION | PreventSelfApproval |
| ApprovedForSubmission → Submitted | SUBMIT | SeparateApproverAndSubmitter |
| UAR → Approved (API approve) | APPROVE | no self si PreventSelfApproval |
| transition → Approved/Rejected | **DENIED** genérico Manage | Forzar `/approve` o rechazo explícito futuro |

`POST /transition` **no** puede saltar a `Approved`/`Rejected`/`Submitted`/`ApprovedForSubmission` sin el endpoint/permiso correcto.
