# 11 — Certificación funcional RA-REV

| ID | Control ejecutado | Resultado real | Evidencia | Estado |
|---|---|---|---|---|
| REV-01 | Abrir expediente listo | 22 controles Aceptar y 22 Rechazar visibles | `journey-ra-rev.json`, `OPEN-DOSSIER`; captura asociada | PASS |
| REV-02 | Rechazar con comentario | Requisito `Rejected` | mismo JSON, `RETURN-REQ` | PASS |
| REV-03 | Aceptar requisitos críticos | 0 críticos pendientes | mismo JSON, `REVIEW-ACCEPT-ALL` | PASS |
| REV-04 | Intentar aprobar/someter/transicionar/observar | HTTP 403 | mismo JSON, `NEG-API-*` | PASS |

Actor certificado: `ra.rev@cert.local`. Resultado del rol: **PASS**.
