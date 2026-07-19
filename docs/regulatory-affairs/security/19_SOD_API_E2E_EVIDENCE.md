# 19 — SoD API E2E Evidence

**Script:** `docs/regulatory-affairs/security/run-sod-api-e2e.ps1`  
**Log:** `evidence/sod-api-console.txt` · `evidence/sod-api-results.json` · `evidence/sod-api-evidence.json`  
**Fecha lab:** 2026-07-14

## Resumen

| Métrica | Valor |
|---------|-------|
| Casos registrados | 54 |
| PASS | **54** |
| FAIL | **0** |

## Casos obligatorios

| Case | Resultado | HTTP / detalle |
|------|-----------|----------------|
| SOD-001 | PASS | 400 Requirement review / PreventSelfReview |
| SOD-002 | PASS | 403 Specialist approve-for-submission |
| SOD-003 | PASS | 403 Reviewer approve-for-submission |
| SOD-004 | PASS | 403 Approver submit |
| SOD-008 | PASS | 403 TAC approve-for-submission |
| SOD-009 | PASS | 403 Viewer create dossier |
| SOD-011 | PASS | 403 cross-tenant |
| SOD-012 | PASS | 403 Submitter approve-for-submission |
| SOD-013 | PASS | 403 Specialist submit |
| SOD-014 | PASS | 400 external approve sin número |
| SOD-015 | PASS | override corto denegado; override requiere permiso dedicado |
| SOD-016 | PASS | transition Approved/Submitted/ApprovedForSubmission → 400 |
| SOD-017 | PASS | 403 Reviewer submit |
| Happy path | PASS | Spec→Rev→Appr→Sub→Mgr obs→Ext CT/RS |

## Tokens frescos

Cada actor usa `POST /auth/login` independiente; no se reutilizan access tokens entre roles en el script.
