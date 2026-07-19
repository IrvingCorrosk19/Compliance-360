# 03 — Database Reset Execution Report

**Verdict:** `DATABASE RESET FOR MANUAL TESTING: PASS`

**Date:** 2026-07-14 / 2026-07-15 UTC lab window  
**Script:** `scripts/reset-manual-testing-data.py --confirm-lab-reset`

---

## Environment

| Field | Value |
|-------|-------|
| Environment | **local-lab** |
| Engine | PostgreSQL 18 |
| Host | `localhost` |
| Database | `compliance360` |
| Username | `postgres` |
| Production | **NOT detected** — reset authorized |

---

## Gates

| Gate | Result |
|------|--------|
| Backup PASS/FAIL | **PASS** (`compliance360_pre_manual_testing_reset_20260714_203157.dump`, 15,080,873 bytes, SHA-256 `69750D06…102C10`) |
| Reset PASS/FAIL | **PASS** |
| Bootstrap preservation PASS/FAIL | **PASS** |
| Browser login PASS/FAIL | **PASS** (`e2e/tests/post-reset-manual.spec.ts`) |
| Admin can create users / assign roles | **PASS** (Tenant Administrator + Platform Administrator preserved; roles/permissions catalogs intact — 24 roles / 112 permissions) |
| Regulatory Affairs loads PASS/FAIL | **PASS** (`.ra-shell` + empty APIs) |
| Final DB ready for manual testing | **YES** |

---

## Pre-reset counts

| Metric | Count |
|--------|------:|
| Tenants | 2 |
| Users | 11 |
| RA Products | 486 |
| Manufacturers | 217 |
| Dossiers | 97 |
| Requirements | 2134 |
| Sanitary registrations | 362 |
| Operating licenses | 32 |
| Observations | 24 |
| Import jobs | 37 |
| Import rows | 15038 |
| Alerts | 10 |
| Notifications | 47 |
| Audit logs | 29731 |
| Roles | 24 |
| Permissions | 112 |
| SoD settings | 1 |
| Migrations | 20 |

## Post-reset counts

| Metric | Count |
|--------|------:|
| Tenants | **2** (preserved) |
| Users | **2** (preserved admins only) |
| RA Products | **0** |
| Manufacturers | **0** |
| Dossiers | **0** |
| Requirements | **0** |
| Sanitary registrations | **0** |
| Operating licenses | **0** |
| Observations | **0** |
| Import jobs | **0** |
| Import rows | **0** |
| Alerts | **0** |
| Notifications | **0** |
| Audit logs (lab ops wiped) | **0** |
| Authorities | **0** |
| Requirement packs | **0** |
| Roles | **24** |
| Permissions | **112** |
| SoD settings | **2** (defaults both tenants) |
| Migrations | **20** |

---

## Bootstrap preserved

- Tenants: `Compliance 360` (platform) + `Irving Corro S.A` (principal)
- Users: `irvingcorrosk19@gmail.com` (Tenant Administrator), `admin@compliance360.local` (Platform Administrator)
- RBAC catalog preserved
- SoD defaults preserved/reseeded
- No cert passwords documented in this report

## Browser / API smoke (post-reset)

- Login as principal Tenant Administrator → sidebar OK  
- Regulatory Affairs shell loads  
- Products / dossiers / registrations / licenses APIs return **empty arrays**  
- SoD settings API loads  
- Release build: **0 errors**

## Notes

- Lab audit events were removable (not an immutable production retention store) and cleared with operational wipe.
- E2E certification suites were **not** re-run (would re-seed data). Smoke only.
- Backup retained under `backups/` for certification evidence recovery if needed.

---

# DATABASE RESET FOR MANUAL TESTING: PASS
