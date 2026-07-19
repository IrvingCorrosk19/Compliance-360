# 10 — Certificación funcional RA-SPEC

| ID | Control ejecutado | Resultado real | Evidencia | Estado |
|---|---|---|---|---|
| SPEC-01 | Crear producto/expediente | Expediente `Planning` | `journey-ra-spec.json`, `CREATE-PRODUCT-DOSSIER`; captura asociada | PASS |
| SPEC-02 | Cargar evidencia | 3 requisitos `Received` con `storedFileId` | mismo JSON, `UPLOAD-REQUIREMENTS` | PASS |
| SPEC-03 | Preparar y enviar | `ReadyForSubmission` | mismo JSON, `SEND-TO-REVIEW` | PASS |
| SPEC-04 | Intentar auto-revisión | Bloqueo SoD HTTP 400 | mismo JSON, `SOD-SELF-REVIEW`; `browser-sod-steps.json` | PASS |
| SPEC-05 | Intentar aprobar/someter/decidir | HTTP 403 en las tres APIs | mismo JSON, `NEG-API-*` | PASS |

Actor certificado: `ra.spec@cert.local`. Resultado del rol: **PASS**.
