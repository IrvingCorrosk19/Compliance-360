# 11 — API Functional Coverage Matrix

**Program:** Final REGUTRACK Replacement Certification  
**Date:** 2026-07-14  
**Base:** `/api/v1` · Regulatory Affairs under `/tenants/{tenantId}/regulatory`  
**Status:** DESIGN — execution pending

---

## 1. Purpose

Map every business-critical API to at least one executable test case. No endpoint is COVERED until its linked TC is executed with browser + API + DB evidence.

---

## 2. Coverage rules

| Rule | Description |
|------|-------------|
| R1 | Each write endpoint: ≥1 positive + ≥1 negative (auth/SoD/validation) |
| R2 | Each read endpoint used in UI: ≥1 role-positive + cross-tenant negative |
| R3 | Workflow transitions: dedicated TC in `12_WORKFLOW_TRANSITION_COVERAGE.md` |
| R4 | Import pipeline: stage → validate → commit each have TC |
| R5 | Status = NOT EXECUTED until Fase 10 evidence attached |

---

## 3. Matrix (sample — full catalog in test-cases/)

| Area | Endpoint | Required Role | Expected | Test Case | Execution Status |
|------|----------|---------------|----------|-----------|------------------|
| Auth | POST /api/v1/auth/login | Any | 200 + JWT | AUTH-001 | NOT EXECUTED |
| Auth | POST /api/v1/auth/logout | Any authenticated | 204 | AUTH-005 | NOT EXECUTED |
| Regulatory | POST .../regulatory/bootstrap | Regulatory Administrator | 200 pack+authorities | PACK-001 | NOT EXECUTED |
| Regulatory | GET .../regulatory/dashboard | Regulatory Viewer | 200 KPIs | DASH-001 | NOT EXECUTED |
| Regulatory | POST .../regulatory/products | Regulatory Specialist | 201 product | PROD-001 | NOT EXECUTED |
| Regulatory | POST .../regulatory/dossiers | Regulatory Specialist | 201 dossier+22 reqs | DOS-001 | NOT EXECUTED |
| Regulatory | PUT .../dossiers/{id}/requirements/{rid} | Regulatory Specialist | 200 status update | REQ-001 | NOT EXECUTED |
| Regulatory | POST .../dossiers/{id}/submit | Regulatory Submitter | 200 after gates | SUB-001 | NOT EXECUTED |
| Regulatory | POST .../dossiers/{id}/approve | Regulatory Approver | 200 CT/RS | REG-001 | NOT EXECUTED |
| Regulatory | POST .../imports/xlsx | Regulatory Administrator | 200 job staged | IMP-001 | NOT EXECUTED |
| Regulatory | POST .../imports/{id}/commit | Regulatory Administrator | 200 entities | IMP-010 | NOT EXECUTED |
| RBAC | POST .../dossiers (wrong tenant token) | Any | 403 | MT-001 | NOT EXECUTED |
| SoD | POST approve by Specialist | Regulatory Specialist | 403 | SOD-003 | NOT EXECUTED |

---

## 4. API groups under certification

1. **Auth** — login, logout, refresh, MFA, password  
2. **Tenants / TAC** — users, roles, RBAC grants (minimum for RA lab)  
3. **Regulatory Bootstrap** — authorities, requirement packs (22 items)  
4. **Products** — CRUD, search, commercializable flag  
5. **Manufacturers & Certificates** — CRUD, expiry refresh  
6. **Dossiers** — CRUD, transitions, requirements, dates, observations  
7. **Internal Approval / Submission** — SoD-gated actions  
8. **Registrations & Renewals** — CT/RS lifecycle  
9. **Operating Licenses** — CRUD, renewals, checklist  
10. **Import** — JSON/XLSX stage, commit, reconciliation  
11. **Dashboard / Alerts** — evaluate thresholds  
12. **Audit** — search tenant-scoped events  
13. **Notifications** — templates, delivery log  

---

## 5. Cross-reference

| Document | Relationship |
|----------|--------------|
| `test-cases/01_AUTHENTICATION_TEST_CASES.md` | Auth APIs |
| `test-cases/04_PRODUCT_PORTFOLIO_TEST_CASES.md` | Product APIs |
| `test-cases/09_DOSSIER_CREATION_TEST_CASES.md` | Dossier APIs |
| `test-cases/25_REGUTRACK_IMPORT_TEST_CASES.md` | Import APIs |
| `test-cases/30_SOD_REGRESSION_TEST_CASES.md` | SoD denials |

---

*All linked test cases start at Status = NOT EXECUTED.*
