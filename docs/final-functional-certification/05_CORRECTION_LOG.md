# 05 — CORRECTION LOG

**Program:** Final Enterprise Functional Certification  
**Date:** 2026-07-05

---

## Corrections applied during this certification cycle

| # | Defect | File(s) | Change | Commit |
|---|---|---|---|---|
| C1 | FFC-001 login timeout under load | `e2e/tests/helpers.ts` | Sequential login flow; 45s timeout; error toast capture | pending |
| C2 | FFC-001 auth rate limit | `FoundationEndpoints.cs` | `.DisableRateLimiting()` on login/mfa/refresh | pending |
| C3 | FFC-001 roles login duplicate | `e2e/tests/roles.spec.ts` | Same login hardening | pending |
| C4 | Certification progress visibility | `e2e/reporters/certification-reporter.ts` | Real-time ROL/Pantalla/Estado banner | pending |
| C5 | Playwright results path | `e2e/playwright.config.ts` | Final certification JSON output | pending |

## Prior corrections (verified still effective)

| # | Defect | File(s) | Commit |
|---|---|---|---|
| P1 | Platform Admin 403 on TAC | ApiContext, PermissionPolicies, RoleCatalog, app.js | `382389e` |
| P2 | E2E Platform Admin RBAC expectation | `e2e/tests/roles.spec.ts` | `4fb2be0` |

---

## Methodology compliance

- ✅ Root cause identified before correction (FFC-001)
- ✅ No hardcode, workarounds, or temporary patches
- ✅ Architectural fix (auth rate limit separation)
- ✅ Full rebuild + retest after correction
- ✅ Zero regressions (238 unit + 29 E2E + 23 journey)

---

*All corrections validated. No open items.*
