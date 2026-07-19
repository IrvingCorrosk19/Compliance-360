# 19 — Master Test Execution Plan

**Program:** Final REGUTRACK Replacement Certification  
**Date:** 2026-07-14  
**Total test-case files:** 35  
**Status:** READY FOR EXECUTION (after Entry Gate)

---

## 1. Pre-execution gates

| Gate | Command / Action | Expected |
|------|------------------|----------|
| G1 | `dotnet build -c Release` | 0 errors |
| G2 | `dotnet test -c Release` | All pass |
| G3 | SoD smoke suite | GO |
| G4 | `GET /health` | 200 |
| G5 | Bootstrap regulatory tenant | 22 pack items |

---

## 2. Execution waves

| Wave | Domain | Files | Depends on |
|------|--------|-------|------------|
| W0 | SoD regression | `30_SOD_REGRESSION_TEST_CASES.md` | G1–G5 |
| W1 | Auth + IAM | `01`, `02` | W0 |
| W2 | Config + packs | `07`, `08` | W1 |
| W3 | Portfolio + manufacturers | `04`, `05`, `06` | W2 |
| W4 | Dossier + requirements + docs | `09`, `10`, `11` | W3 |
| W5 | Milestones + review + approval | `12`, `13`, `14` | W4 |
| W6 | Submission + observations | `15`, `16`, `17`, `18` | W5 |
| W7 | CT/RS + renewal + licenses | `19`, `20`, `24` | W6 |
| W8 | Pipeline + dashboard + alerts | `03`, `21`, `22` | W7 |
| W9 | Import migration | `25` | W3 (parallel ok) |
| W10 | Audit + notifications | `23`, `27` | W6 |
| W11 | Multitenancy + RBAC negative | `28`, `29` | W4 |
| W12 | UI/UX + responsive + errors | `31`, `32`, `33` | W8 |
| W13 | Concurrency | `34` | W6 |
| W14 | Reporting | `26` | W8 |
| W15 | **Full business journey** | `35` | W0–W13 ALL PASS |

---

## 3. Full business journey (critical path)

Execute `35_FULL_BUSINESS_JOURNEY_TEST_CASES.md` with **separate browser profiles per role**:

1. Regulatory Specialist — create product, dossier, requirements  
2. Regulatory Reviewer — review, reject/accept  
3. Regulatory Specialist — correct  
4. Regulatory Reviewer — technical complete  
5. Regulatory Approver — internal approval  
6. Regulatory Submitter — submission  
7. Regulatory Manager — authority observation  
8. Specialist → Reviewer → Approver → Submitter — resubmission round 2  
9. Regulatory Approver — external CT/RS approval  
10. Validate registration, dashboard, pipeline, audit  

---

## 4. Evidence package per TC

- Screenshot (UI steps)
- API request/response (HAR or curl log)
- DB query snapshot (where applicable)
- Audit trail excerpt
- Notification log (if applicable)

Store under: `docs/final-functional-certification/evidence/{TC-ID}/`

---

## 5. Stop-on-fail

| Severity | Action |
|----------|--------|
| P0 / Critical | STOP wave → defect → fix → retest wave |
| P1 / High | Continue only if waiver approved; else STOP |
| P2+ | Log, continue, fix before GO |

---

## 6. Completion metrics

Track in `09_FUNCTIONAL_CERTIFICATION_REPORT.md`:

- Total TC designed
- Total executed
- PASS / FAIL / BLOCKED
- Coverage % by REGUTRACK sheet

---

*No test case may be marked PASS during design phase.*
