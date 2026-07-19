# 09 — Certificación funcional RA-MGR

| ID | Control ejecutado | Resultado real | Evidencia | Estado |
|---|---|---|---|---|
| MGR-01 | Registrar observación | `Submitted → Observed` | `manual-workflow-steps.json`, `MGR-OBSERVE-MODAL` | PASS |
| MGR-02 | Coordinar respuesta | Dossier `CorrectingObservation` | mismo JSON, `SPEC-RESPOND` | PASS |
| MGR-03 | Resometer | HTTP 200; `Resubmitted` | mismo JSON, `MGR-RESUBMIT` | PASS |
| MGR-04 | Registrar resolución y CT/RS | `Closed`; `MQ-983332-07-26` | mismo JSON, `MGR-EXTERNAL-MODAL`, `CTRS-LISTED` | PASS |
| MGR-05 | Decisión externa E2E | `Closed`; CT/RS `MQ-E2E-28094` | `journey-ra-mgr.json`; `ra-mgr-external-decision.png` | PASS |

Actor certificado: `ra.mgr@cert.local`. Resultado del rol: **PASS**.
