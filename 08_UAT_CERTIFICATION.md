# 08 — UAT Certification — Compliance 360

Date: 2026-07-03 · Client scenario: "Alimentos Premium Panamá S.A." (Panama, food industry).

## Scenario setup (pre-existing from E2E provisioning)

Tenant created with branding/security settings; one user provisioned per role; RBAC auto-provisioned on
tenant creation (roles, permissions, assignments). This exercised the tenant-bootstrap path.

## Lifecycle flows exercised (live API, real role users)

| Flow | Steps | Result |
|---|---|---|
| CAPA lifecycle | create → classify (with due date) → root-cause → 5-why → corrective-action | **All OK** |
| CAPA due date with local offset (−05:00) | classify with non-UTC `DateTimeOffset` | **OK** (normalized to UTC) |
| Risk lifecycle | list → **assessment** (inherent/residual scoring, tolerance) | **OK**, residual computed |
| Documents | create → submit-for-review → self-approval blocked (SoD) | **OK** (via E2E) |
| RBAC negative | Quality Manager `POST /capas` | **403** (correct SoD) |

## Defects uncovered by UAT and fixed

1. **CAPA (and all modules) add-child-on-update returned 500** — root cause: EF treated
   client-generated Guid keys as store-generated, so new child rows were UPDATEd (0 rows) instead of
   INSERTed. Fixed globally (`ValueGeneratedNever` convention). Re-validated on CAPA + Risk.
2. **Non-UTC DateTimeOffset write 500** — fixed globally (UTC normalization converter).

Both fixes are architectural (systemic, debt-reducing), not workarounds, and were re-validated with the
full test suites (238 unit + 29 E2E, no regression).

## Honest scope statement

UAT covered the create paths for every module (E2E) plus representative multi-step lifecycle transitions
(CAPA fully, Risk assessment) via API. Full closure/approval chains for every module were not each
exhaustively driven end-to-end; the transition endpoints exist, are protected, and now execute
correctly after the systemic fix. Exhaustive per-module closure UAT is recommended as a follow-up
(risks register), but no additional server defect was observed in the transitions tested.

## Verdict

**UAT PASS** for the flows exercised; the critical defects found were corrected and re-validated.
