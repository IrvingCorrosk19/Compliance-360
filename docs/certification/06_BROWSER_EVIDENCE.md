# 06 — Browser Evidence

**Runner:** Playwright Chromium  
**Base URL:** `http://localhost:5272`  
**Password (lab):** `OwnerStart!2026`  
**Date:** 2026-07-17  

## Suites executed

| Spec | Purpose | Result | Duration |
|------|---------|--------|----------|
| `e2e/tests/manual-roles-browser.spec.ts` | 9 roles × sidebar/tabs/buttons/API deny/Security | **PASS** | ~46s |
| `e2e/tests/manual-workflow-cert.spec.ts` | Full certified dossier lifecycle via UI modals | **PASS** | ~1.1m |
| `e2e/tests/regulatory-sod-roles.spec.ts` | SoD happy path + negatives (regression) | **PASS** | included in 52.7s pair |

## Aggregate metrics

| Metric | Value |
|--------|-------|
| Roles certified | 9/9 |
| Role matrix checks | 219/219 PASS |
| Workflow steps | 21/21 PASS |
| Native dialogs during workflow | 0 |
| Console/page errors (role matrix) | 0 (noise filtered: favicon / intentional 4xx) |

## Evidence artifacts

### Role screenshots

| Role | File |
|------|------|
| TAC | `evidence/cert-role-tac.png` |
| RA-ADM | `evidence/cert-role-ra-adm.png` |
| RA-MGR | `evidence/cert-role-ra-mgr.png` |
| RA-SPEC | `evidence/cert-role-ra-spec.png` |
| RA-REV | `evidence/cert-role-ra-rev.png` |
| RA-APPR | `evidence/cert-role-ra-appr.png` |
| RA-SUB | `evidence/cert-role-ra-sub.png` |
| RA-VIEW | `evidence/cert-role-ra-view.png` |
| QM | `evidence/cert-role-qm.png` |

### Workflow screenshots

| Step | File |
|------|------|
| Specialist prep | `evidence/flow-1-specialist.png` |
| Reviewer accept | `evidence/flow-2-reviewer.png` |
| Approver internal | `evidence/flow-3-approver.png` |
| Submitter submit | `evidence/flow-4-submitter.png` |
| Manager observation | `evidence/flow-5-manager-observe.png` |
| Manager CT/RS | `evidence/flow-6-manager-ctrs.png` |
| Manufacturer modal | `evidence/flow-7-manufacturer.png` |
| License modal | `evidence/flow-8-license.png` |

### Machine-readable logs

| File | Content |
|------|---------|
| `evidence/manual-vs-system-role-matrix.json` | Per-role check list with pass/fail + details |
| `evidence/manual-workflow-steps.json` | Workflow step log (`dossierId`, `catalogCode`, all steps) |

## Sample workflow log (excerpt)

```json
{
  "dossierId": "662cc139-6b7e-48c8-8482-871508eb4c6b",
  "catalogCode": "FLOW-762373",
  "failed": 0,
  "highlights": [
    "SPEC-CREATE-MODAL",
    "SPEC-UI-ReadyForSubmission",
    "REV-ACCEPT-ALL",
    "APPR-INTERNAL-UI",
    "SUB-SUBMIT-UI",
    "MGR-OBSERVE-MODAL",
    "SPEC-RESPOND",
    "MGR-EXTERNAL-MODAL",
    "CTRS-LISTED",
    "SPEC-MFR-MODAL",
    "ADM-LICENSE-MODAL",
    "NO-NATIVE-DIALOGS"
  ]
}
```

## Re-run commands

```powershell
cd "C:\Proyectos\Compliance 360\e2e"
npx playwright test tests/manual-roles-browser.spec.ts tests/manual-workflow-cert.spec.ts tests/regulatory-sod-roles.spec.ts --reporter=line
```
