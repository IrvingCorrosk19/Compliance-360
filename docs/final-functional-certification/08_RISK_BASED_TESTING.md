# 08 — Risk-Based Testing

**Program:** Final Functional Certification — RA BC  
**Contract:** `REGUTRACK 02JUN26 MG.xlsx`  
**Date:** 2026-07-14  
**Status:** Risk model defined; mitigation tests NOT EXECUTED

---

## 1. Purpose

Prioritize functional test execution by **business and compliance risk** so certification effort concentrates on failures that would block REGUTRACK replacement or violate SoD / regulatory audit expectations.

---

## 2. Risk scoring model

Each test item receives:

**Risk Score = Likelihood (1–5) × Impact (1–5)**

| Score | Priority | Execution order |
|-------|----------|-----------------|
| 20–25 | P0 Critical | Day 1–2 |
| 12–19 | P1 High | Week 1 |
| 6–11 | P2 Medium | Week 2 |
| 1–5 | P3 Low | Week 3 or sample |

---

## 3. Impact definitions

| Impact | Description | Example |
|--------|-------------|---------|
| 5 | Regulatory submission wrong; SoD bypass; data loss | Wrong CT/RS issued |
| 4 | Workflow blocked; cannot submit | Checklist gate broken |
| 3 | Wrong field persisted; report incorrect | Opportunity $ wrong |
| 2 | UI workaround exists | Missing kanban column |
| 1 | Cosmetic | Label typo |

---

## 4. Likelihood definitions

| Likelihood | Description |
|------------|-------------|
| 5 | Recent code change; complex logic; partial impl |
| 4 | Multiple integration points |
| 3 | Standard CRUD |
| 2 | Stable, unit-tested |
| 1 | Read-only display |

---

## 5. P0 — Critical risk items (execute first)

| ID | Area | Risk | Score | Test refs |
|----|------|------|-------|-----------|
| R-P0-01 | Submit gate | Incomplete critical reqs allow submit | 25 | RA-CTT-018–039, RA-WF-001 step 6 |
| R-P0-02 | SoD | Same user prep + approve | 25 | RB-003, N1, doc 19 regression |
| R-P0-03 | External approve | CT/RS without number | 20 | RA-CTT-012, RA-WF-001 step 10 |
| R-P0-04 | Import | Data corruption on commit | 20 | SC-WF-004 |
| R-P0-05 | Expiry alerts | Missed renewal | 20 | RA-CTT-044, RA-CTT-055 |
| R-P0-06 | Observation loop | Cannot resubmit after obs | 20 | RA-CTT-048, RA-WF-001 steps 7–9 |
| R-P0-07 | Internal approve | Skip review state | 20 | RA-CTT-045–047 workflow |
| R-P0-08 | Tenant isolation | Cross-tenant data leak | 25 | API with wrong tenant id (negative) |

---

## 6. P1 — High risk items

| ID | Area | Risk | Score | Test refs |
|----|------|------|-------|-----------|
| R-P1-01 | Checklist 22 docs | Missing pack item | 16 | RA-CTT-018–039 each |
| R-P1-02 | Manufacturer certs | Expired cert not alerted | 16 | RA-DOC-005, RA-DOC-018 |
| R-P1-03 | Pipeline kanban | Wrong column | 12 | RA-SCR pipeline |
| R-P1-04 | MINSA vs CSS | Wrong authority on dossier | 12 | RA-CTT-011 |
| R-P1-05 | License renewal | Checklist not seeded | 12 | RA-LIC checklist |
| R-P1-06 | Max reception date | Alert not fired | 12 | RA-CTT-042, 044 |
| R-P1-07 | Registration list | Days remaining wrong | 12 | RA-CTT-055 |
| R-P1-08 | Role visibility | Viewer can mutate | 16 | RB-006 |

---

## 7. P2 — Medium risk items

| ID | Area | Score | Test refs |
|----|------|-------|-----------|
| R-P2-01 | Sales/Mkt input field | 9 | RA-CTT-051 |
| R-P2-02 | Priority field | 6 | RA-CTT-052 |
| R-P2-03 | Comments | 6 | RA-CTT-054 |
| R-P2-04 | Source line import | 9 | RA-CTT-088 |
| R-P2-05 | History events 56–87 | 9 | RA-CTT-HIST |
| R-P2-06 | Form/pack reference | 9 | RA-CTT-017 |
| R-P2-07 | Ficha técnica ref | 9 | RA-CTT-010 |
| R-P2-08 | Dashboard opportunity $ | 6 | RA-CTT-053 |

---

## 8. P3 — Lower risk / sample testing

| ID | Area | Approach |
|----|------|----------|
| R-P3-01 | Studio pack designer | WAIVED — PO sign-off |
| R-P3-02 | CompanyMetadata licenses | WAIVED until implemented |
| R-P3-03 | FADDI manual task | Single smoke test |
| R-P3-04 | Non-critical checklist items | Sample 3 of 22 |
| R-P3-05 | Config JSON formatting | Visual check |

---

## 9. Risk × implementation status

From `REGULATORY_COVERAGE_MATRIX.md`:

| Status | Risk treatment |
|--------|----------------|
| Implementado | Standard execution |
| Parcial | **Elevated** likelihood +1; must PASS or WAIVE with PO |
| Pendiente | WAIVE or block GO per charter §11 |

Partial items requiring extra scrutiny:

- Col 10 Ficha Técnica (file link)
- Col 17 Formulario / pack bridge
- DOCUMENTACION ESTATUS2, TRAMITE-REQUISITO, SEGUIMIENTO
- LICENCIAS milestone dates, CompanyMetadata

---

## 10. Test allocation by risk

| Tester days | P0 | P1 | P2 | P3 |
|-------------|----|----|----|-----|
| Week 1 (5d) | 60% | 30% | 10% | 0% |
| Week 2 (5d) | 10% | 50% | 30% | 10% |
| Week 3 (5d) | 5% | 20% | 40% | 35% |

---

## 11. Residual risk acceptance

| Residual risk | Acceptance criteria |
|---------------|---------------------|
| Studio disconnected | Document in certification report; pack via bootstrap only |
| 32 Excel date columns as history | SME sign-off on equivalence |
| Manual FADDI | Procedure documented for operators |
| No Sales Viewer | Out of scope v1 |

---

## 12. Risk register updates

During execution, add rows to `evidence/functional/risk-register.csv`:

```
risk_id,test_id,status,notes,date
R-P0-01,RA-WF-001,NOT EXECUTED,,2026-07-14
```

---

## 13. Exit gate (risk perspective)

Functional GO requires:

- 100% P0 tests PASS or approved WAIVE
- ≥95% P1 tests PASS
- P2/P3 per sample plan with documented rationale
- Zero open S1/S2 defects on P0/P1 items

---

## 14. Document control

| Version | Date | Change |
|---------|------|--------|
| 1.0 | 2026-07-14 | Initial risk model for certification restart |
