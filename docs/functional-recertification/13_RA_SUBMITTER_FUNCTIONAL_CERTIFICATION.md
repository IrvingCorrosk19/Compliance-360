# 13 — Certificación funcional RA-SUB

| ID | Control ejecutado | Resultado real | Evidencia | Estado |
|---|---|---|---|---|
| SUB-01 | Buscar expediente aprobado internamente | `ApprovedForSubmission` | `journey-ra-sub.json`, `FIND-APPROVED` | PASS |
| SUB-02 | Someter con datos y comprobante | `Submitted`; `submittedOn=2026-07-18` | mismo JSON, `SUBMIT`; captura asociada | PASS |
| SUB-03 | Intentar editar datos bloqueados | Requisitos, fechas y transición: HTTP 403 | mismo JSON, `NO-EDIT-LOCKED` | PASS |
| SUB-04 | Intentar aprobar u observar | HTTP 403 | mismo JSON, `NEG-API-*` | PASS |

Actor certificado: `ra.sub@cert.local`. Resultado del rol: **PASS**.
