# 18 — Entry & Exit Criteria

**Program:** Final REGUTRACK Replacement Certification  
**Version:** 1.0  
**Date:** 2026-07-14

---

## 1. Entry criteria (start execution)

| # | Criterion | Evidence | Status |
|---|-----------|----------|--------|
| E1 | Planning docs `00`–`20` complete | This folder | COMPLETE |
| E2 | All 35 test-case catalogs generated | `test-cases/` | COMPLETE |
| E3 | SoD baseline smoke PASS | `24_FINAL_ROLE_AND_SOD_CERTIFICATION.md` | REQUIRED AT RUN |
| E4 | Lab environment UP `/health` 200 | Log | PENDING |
| E5 | Bootstrap + 22-requirement pack seeded | API | PENDING |
| E6 | Test users per role (distinct users for journey) | `07_TEST_USER_MATRIX.md` | PENDING |
| E7 | REGUTRACK Excel copy available (hash logged) | `06_TEST_DATA_STRATEGY.md` | PENDING |
| E8 | Second tenant for isolation tests | TAC | PENDING |
| E9 | QA Manager + PO sign-off | Below | PENDING |

**Rule:** Do not mark any functional TC PASS until E1–E9 satisfied at execution time.

---

## 2. Exit — GO RETIRE REGUTRACK

| # | Criterion |
|---|----------|
| G1 | Operator does not need Excel for daily work |
| G2 | All operational sheets represented; no critical PARTIAL without WAIVER |
| G3 | Critical Excel columns validated UI/API |
| G4 | Full multi-role journey PASS (`35_FULL_BUSINESS_JOURNEY_TEST_CASES.md`) |
| G5 | 22-requirement checklist fully exercised (`10_DOSSIER_REQUIREMENT_TEST_CASES.md`) |
| G6 | CT/RS + renewals PASS |
| G7 | Operating licenses PASS |
| G8 | Import reconciliation PASS (agreed volume) |
| G9 | Dashboard KPI = API = DB |
| G10 | Multitenancy PASS |
| G11 | SoD regression PASS |
| G12 | Critical defects = 0; High defects = 0 |
| G13 | `95_FINAL_REGUTRACK_REPLACEMENT_CERTIFICATE.md` issued |

---

## 3. Immediate NO GO

- Any P0 FAIL unmitigated
- SoD bypass
- Cross-tenant leak
- Submit with pending criticals succeeds
- Import destroys data without report
- SKIPPED used to avoid lab setup

---

## 4. Signatures

| Role | Name | Approved | Date |
|------|------|----------|------|
| QA Manager | | Pending | |
| Product Owner | | Pending | |
| Regulatory Affairs Specialist | | Pending | |

---

*Certification execution blocked until Entry Gate approved at run time.*
