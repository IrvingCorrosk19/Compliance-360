# 15 — Multitenancy Test Strategy

**Program:** Final REGUTRACK Replacement Certification  
**Lab tenants:** `ddcaf211-afe0-44a0-9c90-4fbda8fc4871` (Tenant A) + isolated Tenant B  
**Date:** 2026-07-14

---

## 1. Scope

Every Regulatory Affairs entity must be isolated by `TenantId`. Testing is **not** limited to dossiers — all RA artifacts are in scope.

---

## 2. Entities under cross-tenant test

| Entity | UI Test | API Test | Direct URL |
|--------|---------|----------|------------|
| Products | Yes | Yes | Yes |
| Manufacturers | Yes | Yes | Yes |
| Manufacturer Certificates | Yes | Yes | Yes |
| Requirement Packs | Yes | Yes | N/A |
| Dossiers | Yes | Yes | Yes |
| Dossier Requirements | Yes | Yes | Yes |
| Documents / Storage | Yes | Yes | Yes |
| Observations | Yes | Yes | Yes |
| Sanitary Registrations | Yes | Yes | Yes |
| Operating Licenses | Yes | Yes | Yes |
| Import Jobs | Yes | Yes | N/A |
| Dashboard KPIs | Yes | Yes | N/A |
| Audit Trail | Yes | Yes | N/A |
| SoD Settings | Yes | Yes | N/A |

---

## 3. Attack vectors

1. JWT tenant A → API resource ID tenant B → expect 403/404  
2. Browser session tenant A → hash route with dossier ID tenant B  
3. Search/filter must not return cross-tenant rows  
4. Export/report must not include foreign tenant data  
5. Platform admin must not read tenant business data without break-glass  

---

## 4. Test cases

Primary catalog: `test-cases/28_MULTITENANCY_TEST_CASES.md` (MT-001 … MT-014)

---

## 5. Pass criteria

- Zero cross-tenant data exposure in UI, API, or audit  
- 403/404 consistent; no stack traces with foreign IDs  

---

*All multitenancy TC: NOT EXECUTED.*
