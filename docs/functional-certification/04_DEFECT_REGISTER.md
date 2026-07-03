# 04 · DEFECT REGISTER

Defects follow the mandatory cycle: Detect → Analyze → Root Cause → Document → Design → Implement → Build → Retest → Close.

## Open defects (this program)

**None.** No new functional, RBAC, isolation or error-handling defects were detected during FASE 3–8 execution.

## Carried-over defects (detected in prior acceptance program, verified closed here)

| ID | Severity | Symptom | Status | Retest evidence |
|---|---|---|---|---|
| D-01 | High | Duplicate tenant tax identifier → HTTP 500 | **CLOSED** | Journey step 3: duplicate returns **400** |
| D-02 | High | Platform Admin could not activate tenant via API | **CLOSED** | Journey step 4: activate → **Active** |
| D-03 | High | CAPA effectiveness blocked (no way to complete action) | **CLOSED** | Journey step 14: effectiveness **OK** |
| D-04 | High | Multipart file upload → 400 Malformed request | **CLOSED** | Journey step 12: upload **OK** |
| D-05 | Low | Branding theme case-sensitivity unclear | **CLOSED** | Journey step 7: branding **OK** |

## Adversarial probe results (no defects)

| Probe | Expected | Actual |
|---|---|---|
| Viewer create document | 403 | 403 |
| Viewer read foreign tenant | 403 | 403 |
| Risk Manager close risk (SoD) | 403 | 403 |
| Malformed JSON body | 400 (not 500) | 400 |
| No token | 401 | 401 |

**Register verdict:** 0 open defects; 5 carried-over defects verified closed.
