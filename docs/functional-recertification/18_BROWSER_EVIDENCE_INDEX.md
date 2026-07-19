# 18 — Índice de evidencia de navegador

## Artefactos E2E finales

`artifacts/e2e` contiene y forma parte de la evidencia vigente:

- 20 archivos con nombre `functional-summary.json`.
- 20 archivos con nombre `functional-final.png`.

Cada resumen JSON se vincula con su captura final dentro del escenario correspondiente. Las capturas históricas `*-error.png` no se usan como evidencia positiva.

## JSON y logs funcionales

| Archivo | Cobertura |
|---|---|
| `docs/certification/evidence/manual-workflow-steps.json` | Flujo regulatorio completo; `failed=0` |
| `docs/regulatory-affairs/security/evidence/browser-sod-steps.json` | SoD real; `failed=0` |
| `docs/certification/evidence/manual-vs-system-role-matrix.json` | Acceso UI/API de TAC, RA-ADM, roles RA y QM |
| `docs/certification/evidence/role-e2e/journey-ra-spec.json` | RA Specialist |
| `docs/certification/evidence/role-e2e/journey-ra-rev.json` | RA Reviewer |
| `docs/certification/evidence/role-e2e/journey-ra-appr.json` | RA Approver |
| `docs/certification/evidence/role-e2e/journey-ra-sub.json` | RA Submitter |
| `docs/certification/evidence/role-e2e/journey-ra-mgr.json` | RA Manager y CT/RS |
| `docs/certification/evidence/role-e2e/journey-ra-view.json` | RA Viewer y pruebas negativas |
| `docs/regulatory-affairs/security/evidence/sod-api-results.json` | Casos SoD API |
| `docs/regulatory-affairs/security/evidence/sod-api-evidence.json` | Actor, tenant, HTTP y antes/después |

## Capturas principales

- Roles: `cert-role-tac.png`, `cert-role-ra-adm.png`, `cert-role-ra-mgr.png`, `cert-role-ra-spec.png`, `cert-role-ra-rev.png`, `cert-role-ra-appr.png`, `cert-role-ra-sub.png`, `cert-role-ra-view.png`, `cert-role-qm.png`.
- Flujo: `flow-1-specialist.png` a `flow-8-license.png`.
- Role E2E: capturas `ra-spec-*`, `ra-rev-*`, `ra-appr-*`, `ra-sub-*`, `ra-mgr-*` y `ra-view-*` bajo `docs/certification/evidence/role-e2e`.

La correspondencia documental por rol está en `07_TAC_FUNCTIONAL_CERTIFICATION.md` a `15_QM_FUNCTIONAL_CERTIFICATION.md`.
