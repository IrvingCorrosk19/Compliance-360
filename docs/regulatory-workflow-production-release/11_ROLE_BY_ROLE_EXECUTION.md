# 11 — Ejecución rol por rol

## Evidencia ejecutada

- **RA-SPEC:** creación UI de producto+dossier Draft; metadata; inicio de revisión técnica; rechazo fuera de scope; carga V1/V2; envío de corrección.
- **RA-REV:** solicitud de corrección y finalización de revisión técnica.
- **Regulatory Viewer:** seis mutaciones V2 rechazadas con HTTP 403.
- **TAC y RA-ADM:** review, approve y override rechazados con HTTP 403.
- **Roles de gobierno:** el escenario V2 registra reopen y override con solicitante segregado y dos aprobadores distintos.

Fuente: `workflow-v2-final.json`, `viewer-v2-negative-results.json`, `admin-v2-negative-results.json` y `regulatory-workflow-v2.spec.ts`.

## Ejecución productiva final

- TAC, RA-ADM, RA-MGR, RA-SPEC, RA-REV, RA-APPR, RA-SUB, RA-VIEW y QM: recorridos positivos y negativos ejecutados.
- Aprobación interna, submission, resubmission, decisión externa CT/RS, renovación, reapertura, override y archivo: PASS.
- Responsive, idioma, navegación, RBAC, SoD y llamadas directas protegidas: PASS.
- Fuente final: regresión Playwright productiva **72/72 PASS**, finalizada `2026-07-19T00:31:31Z`.

## Estado

**PASS — matriz productiva completa ejecutada sin omisiones.**
