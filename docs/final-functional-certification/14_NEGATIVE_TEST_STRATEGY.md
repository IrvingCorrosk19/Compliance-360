# 14 — Negative Test Strategy

**Program:** Final REGUTRACK Replacement Certification  
**Date:** 2026-07-14

---

## 1. Objectives

Prove the system **fails safely** under invalid input, unauthorized access, workflow bypass, and adversarial manipulation — without corrupting dossier integrity or leaking tenant data.

---

## 2. Negative test categories

| Category | Scope | Primary File | Min Cases |
|----------|-------|--------------|-----------|
| Auth denial | Invalid credentials, stale token, wrong tenant | `01_AUTHENTICATION_TEST_CASES.md` | 8 |
| RBAC denial | Write without permission | `29_RBAC_NEGATIVE_TEST_CASES.md` | 14 |
| SoD denial | Self-approve, creator review, submitter approve | `30_SOD_REGRESSION_TEST_CASES.md` | 16 |
| Workflow bypass | Illegal transitions, API out-of-order | `12_WORKFLOW_TRANSITION_COVERAGE.md` | 4+ |
| Validation | Empty, null, GUID invalid, dates inverted | All domain files | Per endpoint |
| Multitenancy | Cross-tenant ID in URL/body | `28_MULTITENANCY_TEST_CASES.md` | 14 |
| Import abuse | Corrupt XLSX, missing sheet, bad mapping | `25_REGUTRACK_IMPORT_TEST_CASES.md` | 6 |
| Concurrency | Double-click, parallel edit | `34_CONCURRENCY_AND_IDEMPOTENCY_TEST_CASES.md` | 10 |

---

## 3. Adversarial inputs (mandatory sample)

- Unicode product names (ñ, 中文, emoji in lab only)
- HTML/script strings in text fields
- SQL-like strings (`'; DROP TABLE--`)
- 10,000+ character comments
- Empty file upload
- .exe renamed to .pdf
- GUID from tenant B used in tenant A API call

---

## 4. Expected negative behavior

| Situation | Expected |
|-----------|----------|
| 403 Forbidden | No state change; audit SoD.Denied if applicable |
| 400 Validation | Clear error message; no partial DB write |
| 404 Not Found | No leak of existence across tenants |
| Double submit | Idempotent or second call rejected |

---

## 5. Evidence per negative TC

- API status code + body
- DB unchanged verification (before/after query)
- Audit event if security-relevant
- UI error message screenshot

---

## 6. Stop rules

- P0 negative failure (e.g., cross-tenant 200) → STOP certification
- SoD bypass → STOP, re-run SoD baseline

---

*All negative test cases: Status = NOT EXECUTED.*
