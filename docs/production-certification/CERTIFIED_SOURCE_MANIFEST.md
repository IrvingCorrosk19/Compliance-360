# CERTIFIED_SOURCE_MANIFEST

## Identity

| Field | Value |
|-------|-------|
| Date (UTC) | 2026-07-26 |
| Branch | `master` |
| Base HEAD (pre-commit) | `afd491a` |
| Remote | `origin` → `https://github.com/IrvingCorrosk19/Compliance-360.git` |
| Working tree | Dirty → pending certified release commit |
| Build target | `Release` / `net9.0` |
| Lab base URL | `http://localhost:5272` |

## Certification evidence (preserved)

| Stage | Score | Path |
|-------|------:|------|
| AS-IS | 81/100 | `docs/enterprise-functional-certification/` |
| Remediation | 95/100 | `docs/enterprise-remediation/` |
| Final | 100/100 | `docs/enterprise-final-certification/` especially `evidence/FINAL_100_CERTIFICATION_SUMMARY.json` |

## Test counts (certified lab)

| Suite | Result |
|-------|--------|
| Unit | 325/325 PASS |
| AS-IS harness | 82/82 PASS |
| Remediation harness | 32/32 PASS |
| Final harness | 26/26 PASS |
| Playwright critical | 5/5 PASS (i18n, responsive, SoD, workflow V2, RA create) |
| Build Release | 0 errors |

## Migration state

- No new EF migrations in this working tree vs `afd491a`.
- Latest known migrations already in codebase under `src/Compliance360.Infrastructure/Persistence/Migrations/` (Alert Center / Workflow V2 series).
- **Assessment for deploy:** expect **NO NEW MIGRATION REQUIRED** if production already applied migrations through `20260719204100` family; verify on VPS `__EFMigrationsHistory` before applying any `database update`.

## Secret scan (pre-commit)

- `.env` gitignored.
- New lab harness DB password moved to env vars `COMPLIANCE360_PGPASSWORD` / `PGPASSWORD` (not hardcoded in new enterprise certification scripts).
- Lab app password `OwnerStart!2026` remains as established **lab/test** credential pattern (already used by e2e); not a production secret.
- No JWT/SMTP/API production keys found in staged paths.

## SSH / deploy access (blocking note)

- Production host historically: `164.68.99.83` / `https://compliance360.164.68.99.83.nip.io`
- Local machine has **no usable SSH private key** for `root@164.68.99.83` (`Permission denied (publickey,password)`).
- Deploy/backup steps require operator-provided SSH access after push.

## Intended CERTIFIED_RELEASE_COMMIT

Filled after commit creation.
