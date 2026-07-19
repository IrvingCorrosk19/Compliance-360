# 12 — Certificación funcional RA-APPR

| ID | Control ejecutado | Resultado real | Evidencia | Estado |
|---|---|---|---|---|
| APPR-01 | Validar precondición | `ReadyForSubmission`; 35 indicadores | `journey-ra-appr.json`, `VALIDATE-STATE` | PASS |
| APPR-02 | Aprobar internamente | `ApprovedForSubmission` | mismo JSON, `APPROVE-INTERNAL`; captura asociada | PASS |
| APPR-03 | Intentar someter | Botón ausente; API HTTP 403 | mismo JSON, `NO-SUBMIT-BTN`, `NEG-API-SUBMIT` | PASS |
| APPR-04 | Intentar editar/decidir externamente | HTTP 403 | mismo JSON, `NO-EDIT-RESTRICTED`, `NEG-API-APPROVE-EXT` | PASS |

Actor certificado: `ra.appr@cert.local`. Resultado del rol: **PASS**.
