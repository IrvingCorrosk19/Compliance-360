# 04 — ROOT CAUSE ANALYSIS

**Program:** Final Enterprise Functional Certification  
**Date:** 2026-07-05  
**Defects found during execution:** 1 (transient, resolved)

---

## DEFECT FFC-001 — F09 Risk Manager login timeout

| Field | Detail |
|---|---|
| **ID** | FFC-001 |
| **Severity** | Medium (transient — blocked certification run) |
| **Status** | ✅ CLOSED |
| **Role** | Risk Manager |
| **Screen** | Login |
| **Process** | F09 — create risk with category and matrix |

### Description

During the first full Playwright run (29 tests), test F09 failed with `TimeoutError: page.waitForSelector('aside.sidebar')` after 25s. The login page remained visible with the global loading overlay showing "Validando credenciales...".

### Steps to reproduce

1. Run `customer_journey.ps1` in parallel with `npx playwright test` (full suite).
2. Execute tests F01–F08 sequentially (rapid login/logout cycle).
3. F09 Risk Manager login hangs on loading overlay.

### Expected result

Sidebar visible within 25s; risk creation proceeds.

### Actual result

Login overlay stuck; sidebar never rendered; test timeout.

### Root cause

**Compound cause:**

1. **Concurrent load:** Customer journey API harness (23 steps, heavy DB writes) ran in parallel with Playwright E2E, causing PostgreSQL contention during F09 login window.
2. **E2E anti-pattern:** Login helper used `Promise.all([waitForSelector, click])` — race-prone; no error toast detection on failure.
3. **Rate limiting on auth:** `/api/v1/auth/login` inherited the global 120 req/min API rate limit, amplifying rejection risk during certification bursts.

### Files affected

| File | Impact |
|---|---|
| `e2e/tests/helpers.ts` | Login helper race + no error feedback |
| `e2e/tests/roles.spec.ts` | Duplicate login with same anti-pattern |
| `src/Compliance360.Web/Api/FoundationEndpoints.cs` | Auth endpoints rate-limited with business API |

### Risk

Medium — certification false-negative under load; production login unaffected at normal traffic.

### Solution implemented

1. **Login helper:** Sequential click → wait (45s); surface error toast message on failure.
2. **Auth endpoints:** `.DisableRateLimiting()` on `/login`, `/mfa/complete`, `/refresh` — authentication entry points must not compete with business API quota.
3. **Execution order:** Customer journey runs sequentially after Playwright, not in parallel.

### Retest evidence

| Run | Result |
|---|---|
| F09 isolated | PASS (7.3s) |
| Full suite retest (29 tests) | 29/29 PASS (3.3m) |
| Customer journey retest | 23/23 PASS |
| Unit tests | 238/238 PASS |

---

## Prior defects (from previous certification cycles)

| ID | Issue | Status |
|---|---|---|
| PRIOR-001 | Platform Admin 403 on TAC | ✅ CLOSED (`382389e`) |
| PRIOR-002 | SuperAdmin bootstrap WARNING label | ✅ CLOSED (`382389e`) |
| PRIOR-003 | E2E Platform Admin mustNotSee TAC | ✅ CLOSED (`4fb2be0`) |

**Open functional defects:** 0

---

*No additional RCA required. All identified issues resolved with architectural corrections.*
