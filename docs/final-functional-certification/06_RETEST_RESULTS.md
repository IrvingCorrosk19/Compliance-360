# 06 — RETEST RESULTS

**Program:** Final Enterprise Functional Certification  
**Date:** 2026-07-05  
**Environment:** localhost:5272, PostgreSQL 18, Release build

---

## Technical gates

| Gate | Command | Result |
|---|---|---|
| Clean | `dotnet clean Compliance360.sln` | ✅ PASS |
| Restore | `dotnet restore Compliance360.sln` | ✅ PASS |
| Build Release | `dotnet build -c Release` | ✅ 0 errors, 0 warnings |
| Unit tests | `dotnet test -c Release` | ✅ **238/238 PASS** |
| Health | `GET /health` | ✅ Healthy |
| Bootstrap | Development bootstrap | ✅ Ready for Functional Testing |

---

## Playwright E2E (headful, Chrome)

| Suite | Tests | Result | Duration |
|---|---|---|---|
| functional.spec.ts (F01–F15) | 15 | ✅ 15/15 PASS | ~1.5m |
| roles.spec.ts (R01–R14) | 14 | ✅ 14/14 PASS | ~1.8m |
| **Total** | **29** | ✅ **29/29 PASS** | **3.3m** |

**Mode:** `headless: false`, `channel: chrome`, certification reporter active  
**Evidence:** `artifacts/e2e/html-report`, `artifacts/e2e/final-certification-results.json`, screenshots/videos/traces per test

---

## Customer Journey API harness

| Part | Steps | Result |
|---|---|---|
| A — New customer onboarding | 1–11 | ✅ 11/11 PASS |
| B — Business lifecycle | 12–23 | ✅ 12/12 PASS |
| **Total** | **23** | ✅ **23/23 PASS** |

**Evidence:** `artifacts/e2e/journey_result.json`

---

## Security probes (embedded in E2E)

| Probe | Result |
|---|---|
| Platform Admin cross-tenant documents → 403 | ✅ PASS |
| Document Controller self-approve → 403 | ✅ PASS |
| Viewer API create → 403 | ✅ PASS |
| Storage Admin no email button (SoD) | ✅ PASS |
| Notification Admin no storage button (SoD) | ✅ PASS |
| Support Operator break-glass limited menu | ✅ PASS |

---

## Retest after FFC-001 correction

| Run | Before fix | After fix |
|---|---|---|
| Full Playwright (29) | 28/29 (F09 FAIL) | ✅ 29/29 PASS |
| F09 isolated | PASS | ✅ PASS |
| Customer journey | 23/23 (parallel) | ✅ 23/23 (sequential) |
| Unit tests | 238/238 | ✅ 238/238 |

---

## Regression summary

**Zero regressions detected** after corrections.

---

*Retest complete. All gates green.*
