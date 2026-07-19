# 04 — Functional Gap Report

## Gaps found → closed

| ID | Function | Manual expectation | System defect | Fix | Status |
|----|----------|--------------------|---------------|-----|--------|
| FN-01 | RA-ADM create product+dossier | Button allowed | Missing `DOSSIER.CREATE` on role | Added to `RoleCatalog` + provisioned | CLOSED |
| FN-02 | Role profile detection | TAC ≠ RA-ADM ≠ Specialist | Granting CREATE reclassified ADM as Specialist | `roleProfile()` prioritizes CONFIGURE | CLOSED |
| FN-03 | Prep transitions | Specialist (`DOSSIER.UPDATE`) | UI allowed CREATE as prep proxy; ADM could operate | Prep gated only by `DOSSIER.UPDATE` | CLOSED |
| FN-04 | API coarse policy | Action-specific permissions | `Regulatory.Manage` unlocked transition/obs/license/renewal for Reviewer | Granular policies on endpoints | CLOSED |
| FN-05 | Observation ownership | Manager records authority observation | Specialist with OBS.MANAGE saw the button | UI requires Manager profile (obs ∧ external approve) | CLOSED |
| FN-06 | Observation respond | Specialist responds | Button tied only to obsManage | Also requires `REQUIREMENT.MANAGE` (Specialist has it; Manager does not respond as prep) | CLOSED |
| FN-07 | License creation | Capture company + type | Silent hardcoded create | Modal + API | CLOSED |
| FN-08 | Manufacturer creation | Capture legal name + country | Native prompts | Modal + certificate side-effect preserved | CLOSED |
| FN-09 | External decision | Capture CT/RS + expiry | Native prompts | Modal; dossier may close after registration (Approved→Closed) — accepted by workflow | CLOSED |
| FN-10 | Security screen | TAC can open Security | 404-like empty / forbidden | Real Security center page | CLOSED |
| FN-11 | Audit Trail exposure | Only TAC/QM | All RA roles had AUDIT.READ | Catalog trimmed | CLOSED |

## Functional workflow certification

Executed `manual-workflow-cert.spec.ts` against live API + UI:

```
SPEC create → prep transitions → REV accept-all critical →
APPR internal → SUB submit → MGR observe → SPEC respond →
MGR resubmit + CT/RS → SPEC manufacturer → ADM license
```

**Result:** 21/21 steps PASS · 0 native dialogs · CT/RS listed.

## Negative functional controls

| Actor | Forbidden action | HTTP | Result |
|-------|------------------|------|--------|
| Viewer | Create/transition/review/approve/submit/observe/renew/mfr/license/SoD | 401/403 | PASS |
| Reviewer | Create product, transition, observe, renew, mfr, license, submit, approve-internal | 401/403 | PASS |
| Approver | Submit, transition, observe, requirement update | 401/403 | PASS |
| Submitter | Approve-internal, transition, observe, requirement update | 401/403 | PASS |
| Specialist | Bootstrap, approve-internal, submit, external approve, renewals, licenses | 401/403 | PASS |
| RA-ADM | Transition (operate), approve-internal, submit, external approve, requirement update | 401/403 | PASS |
| TAC | Create dossier, transition, approve-internal, submit, external approve | 401/403 | PASS |

## Open non-blocking items

| ID | Item | Severity | Notes |
|----|------|----------|-------|
| FN-ENH-01 | UI control for `MaximumReceptionOn` | Low | API supports it; optional in manual fields |
| FN-ENH-02 | Studio template `prompt()` | Low | Outside certified RA navigation scope |

## Certification

**All blocking functional gaps CLOSED. Functional behavior COINCIDE with manual.**
