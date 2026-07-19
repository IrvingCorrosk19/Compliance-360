# 17 — Pruebas negativas de seguridad

| Área | Prueba negativa confirmada | Resultado | Evidencia | Estado |
|---|---|---|---|---|
| Auto-revisión | Specialist intenta revisar su propio expediente | HTTP 400 | `browser-sod-steps.json`, `SPEC-SELF-REVIEW-DENIED` | PASS |
| Specialist | Aprobar, someter y decidir externamente | HTTP 403 | `journey-ra-spec.json`, `NEG-API-*` | PASS |
| Reviewer | Aprobar, someter, transicionar y observar | HTTP 403 | `journey-ra-rev.json`, `NEG-API-*` | PASS |
| Approver | Someter y decidir externamente | Botón ausente; HTTP 403 | `journey-ra-appr.json` | PASS |
| Submitter | Aprobar, observar y editar datos bloqueados | HTTP 403 | `journey-ra-sub.json` | PASS |
| Viewer | Crear, editar, revisar, aprobar, someter, eliminar y acceder a URLs no asignadas | HTTP 403 o denegación UI | `journey-ra-view.json`; capturas negativas | PASS |
| TAC | Ejecutar acciones operativas RA | HTTP 403 | `manual-vs-system-role-matrix.json`, `DENY-*` | PASS |
| RA-ADM | Ejecutar transiciones y decisiones operativas | HTTP 403 | matriz de roles, `DENY-*` | PASS |
| QM | Ejecutar preparación, aprobación interna o sometimiento | HTTP 403 | matriz de roles, `DENY-*` | PASS |

La suite SoD confirmada es 10/10 PASS. Los artefactos positivos y finales están presentes en `artifacts/e2e`: 20 `functional-summary.json` y 20 `functional-final.png`. No se usa un conteo agregado inventado de toda la suite Playwright.
