# 02 — Matriz manual vs. sistema

Fecha de corte: 2026-07-18. Fuente esperada: `01_MANUAL_FUNCTIONAL_INVENTORY.md` y `docs/user-manual/**`. Fuente real: JSON, logs y capturas de `18_BROWSER_EVIDENCE_INDEX.md`.

| ID | Rol/módulo | Requisito | Resultado actual | Evidencia | Estado |
|---|---|---|---|---|---|
| GAP-01 | RA-SPEC | Requisito recibido con evidencia | 3 requisitos `Received` con `storedFileId` | `journey-ra-spec.json`, `UPLOAD-REQUIREMENTS` | PASS |
| GAP-02 | RA-REV | Rechazo con comentario | `Rejected`; críticos posteriormente aceptados | `journey-ra-rev.json`, `RETURN-REQ`, `REVIEW-ACCEPT-ALL` | PASS |
| GAP-03 | RA-SUB | Sometimiento comprobable | Trámite, número externo, fecha y comprobante persistidos; estado `Submitted` | `journey-ra-sub.json`, `SUBMIT` | PASS |
| GAP-04 | RA-MGR | Ciclo de observación | `Observed → CorrectingObservation → Resubmitted → Closed` | `manual-workflow-steps.json` | PASS |
| GAP-05 | RA-MGR/QM | Resolución externa y CT/RS | Resolución y CT/RS persistidos | `journey-ra-mgr.json`, `EXTERNAL-DECISION` | PASS |
| GAP-06 | RA-ADM | Alert settings | Configuración activa y protegida por RBAC | suite `ra-admin` 10/10 | PASS |
| GAP-07 | TAC | Editar/desactivar usuarios | Flujo operativo; `UserStatus.Disabled=3` | TAC/QM 2/2; .NET 252/252 | PASS |
| GAP-08 | QM/QMS | Operación QMS | Backend, navegación y aprobación documental activos | `cert-role-qm.png`; TAC/QM 2/2 | PASS |
| GAP-09 | SoD | Separación de funciones | Denegaciones 400/403 y transiciones autorizadas verificadas | `browser-sod-steps.json`; SoD 10/10 | PASS |
| GAP-10 | Evidencia E2E | Persistencia por escenario | `artifacts/e2e` contiene 20 `functional-summary.json` y 20 `functional-final.png` | `18_BROWSER_EVIDENCE_INDEX.md` | PASS |

No se extiende el cierre a funcionalidades no ejecutadas. Las correcciones confirmadas se detallan en `19_CORRECTIONS_APPLIED.md`.
