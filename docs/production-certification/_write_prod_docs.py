from pathlib import Path
from datetime import datetime, timezone
import json

OUT = Path(r"c:\Proyectos\Compliance 360\docs\production-certification")
EV = OUT / "evidence"
EV.mkdir(parents=True, exist_ok=True)

CERT = "1dd34a6860827fe39e6f3b9c84bcede6c653b054"
SHORT = "1dd34a6"

docs = {
"01_RELEASE_MANIFEST.md": f"""# Release Manifest

| Field | Value |
|-------|-------|
| Functional certification | **100/100** (`docs/enterprise-final-certification/`) |
| CERTIFIED_RELEASE_COMMIT | `{CERT}` |
| Short SHA | `{SHORT}` |
| Branch | `master` |
| Remote | `origin/master` @ `{CERT}` |
| Commit parity local↔remote | **YES** |
| Production commit | **UNKNOWN / NOT DEPLOYED** (SSH unavailable) |
| Commit parity certified↔production | **NO** |
| Release tag | **NOT CREATED** (requires production deploy PASS) |
| Timestamp (UTC) | {datetime.now(timezone.utc).isoformat()} |

## Commit message
`release: Compliance 360 Regulatory Affairs Enterprise Premium certification`
""",

"02_PRE_DEPLOYMENT_BASELINE.md": """# Pre-Deployment Baseline

Captured against **currently running** production (pre-certified-commit deploy).

| Check | Result |
|-------|--------|
| HTTPS `/health` | HEALTHY |
| HTTPS `/health/live` | HEALTHY |
| HTTPS `/health/ready` | HEALTHY |
| HTTP `:8085` health | HEALTHY (also publicly reachable — hardening note) |
| nginx | 1.24.0 Ubuntu |
| HSTS / security headers | Present |
| Login (7 roles) | PASS |
| Dashboard / products / search / dossiers | PASS |
| Viewer create deny (403) | PASS |
| Foreign tenant path deny (403) | PASS |
| Report list/execute | PASS |
| Locale EN/ES assets | PASS |
| 404 sanitization | PASS |

Evidence: `evidence/predeploy-production-smoke.json` (22/22 PASS).

**Note:** This proves current production is operational; it does **not** prove the certified SHA `{SHORT}` is running.
""".replace("{SHORT}", SHORT),

"03_BACKUP_VERIFICATION.md": """# Backup Verification

## Status
**NOT EXECUTED** — VPS SSH access unavailable from the certification workstation.

## Required before any deploy (operator)

```bash
# On VPS (example — use real compose project dir)
TS=$(date -u +%Y%m%d_%H%M%S)
mkdir -p /opt/backups
docker exec compliance360_postgres pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -Fc -f /tmp/c360-$TS.dump
docker cp compliance360_postgres:/tmp/c360-$TS.dump /opt/backups/compliance360-$TS.dump
sha256sum /opt/backups/compliance360-$TS.dump | tee /opt/backups/compliance360-$TS.sha256
ls -lh /opt/backups/compliance360-$TS.dump
```

Also back up (outside Git): `.env`, nginx site config, `compliance360_storage` volume snapshot if feasible.

Historical backup path referenced in prior runbook: `/opt/backups/compliance360-20260718_085002` — **must be re-verified** before cutover; do not assume freshness.
""",

"04_MIGRATION_ASSESSMENT.md": """# Migration Assessment

## Code vs certified commit
No new EF migration files were introduced in certified commit `1dd34a6` relative to prior `afd491a`.

## Conclusion (code analysis)
**NO MIGRATION REQUIRED** for schema delta of this release **if** production `__EFMigrationsHistory` already contains the Alert Center / Workflow V2 migrations through the Jul-2026 series.

## Gate before start
Operator MUST confirm on VPS:

```bash
docker exec -i compliance360_postgres psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c \\
  'SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY 1 DESC LIMIT 20;'
```

If any pending migrations appear after image upgrade, apply only via official EF tooling — never EnsureDeleted/EnsureCreated/DROP DATABASE.
""",

"05_DEPLOYMENT_EXECUTION.md": f"""# Deployment Execution

## Status
**NOT EXECUTED — BLOCKED**

### Blocker
SSH to `164.68.99.83` failed for `root`, `ubuntu`, `deploy`, `admin` (`Permission denied (publickey,password)`). No private key available on the operator workstation; `ssh-agent` empty.

## What completed
1. Pre-commit gates PASS (build/unit/regression/playwright)
2. Certified commit created: `{CERT}`
3. Push to `origin/master` PASS — remote HEAD matches local HEAD

## Required operator steps (when SSH available)

```bash
cd /opt/compliance360   # or actual compose path
git fetch origin
git checkout {CERT}
# verify
test "$(git rev-parse HEAD)" = "{CERT}"
# backup first (see 03)
docker compose build web worker
docker compose up -d postgres
# migrate only if assessment requires
docker compose up -d web worker
curl -sk https://compliance360.164.68.99.83.nip.io/health/ready
```

Tag images as `compliance360:{SHORT}` for traceability (avoid `latest`-only).
""",

"06_SERVICE_HEALTH.md": """# Service Health

## Current production (pre-deploy of certified SHA)

| Service | Status |
|---------|--------|
| Web/API (via HTTPS) | HEALTHY |
| Health ready | HEALTHY |
| Database (inferred via ready) | HEALTHY |
| Worker / Alert Center | Assumed running if ready includes worker checks — **confirm via SSH logs** |
| HTTPS / nginx | HEALTHY |
| Direct :8085 | HEALTHY (exposure note) |

## Post-deploy of certified SHA
**NOT AVAILABLE** — deploy not executed.
""",

"07_PRODUCTION_SMOKE_TESTS.md": """# Production Smoke Tests

## Pre-deploy smoke (current production)
**22/22 PASS** — `evidence/predeploy-production-smoke.json`

Covered: health, headers, multi-role login, dashboard, products, search, dossiers, RBAC deny, MT path deny, reports, locales, error sanitization.

## Post-deploy smoke (certified SHA)
**NOT RUN** — deployment blocked.
""",

"08_PRODUCTION_RBAC_SOD.md": """# Production RBAC / SoD

## Pre-deploy sanity
- Viewer product create → **403 PASS**
- Specialist / Reviewer / Approver / Submitter / Viewer / Admin / Reporting logins → **PASS**

## Full SoD multi-role E2E on production after certified deploy
**NOT RUN** — requires deploy + controlled dossier; lab SoD already certified against same source.

## Post-deploy requirement
Re-run minimal SoD deny/allow matrix against production after SHA parity YES.
""",

"09_PRODUCTION_MULTITENANT_SANITY.md": """# Production Multitenancy Sanity

## Pre-deploy
Foreign tenant path with CERT specialist token → **403 PASS**.

## Dual business tenant adversarial suite
Certified in lab (`docs/enterprise-final-certification`). Not re-run destructively in production.

## Post-deploy
Repeat foreign-path + search isolation sanity only after certified SHA is live.
""",

"10_PRODUCTION_REPORT_EXPORT.md": """# Production Report / Export

## Pre-deploy
- Report list PASS (20 items)
- Report execute HTTP 200 PASS

## Full CSV/PDF/XLSX OOXML download validation on production
**PARTIAL / PENDING post-deploy** — content download/OOXML parse should be re-checked after certified image is live (builder changed in this release).
""",

"11_PRODUCTION_NOTIFICATION_STATUS.md": """# Production Notification Status

## Lab
Mailpit certified.

## Production
**EXTERNAL CONFIGURATION REQUIRED** for cloud SMTP (SendGrid/Mailgun/Resend) unless already configured in VPS `.env` (not inspectable without SSH).

Do not place API keys in Git.
""",

"12_PRODUCTION_SECURITY_SANITY.md": """# Production Security Sanity

| Check | Result |
|-------|--------|
| HTTPS | PASS |
| HSTS | PASS |
| CSP / XFO / nosniff | PASS |
| 404 does not leak stack/SQL/paths | PASS |
| Swagger public | Not probed deeply; confirm post-SSH |
| Development mode | Ready health suggests Production env |

## Hardening note
`http://164.68.99.83:8085` responds publicly. Prefer loopback-only bind + nginx TLS termination only.
""",

"13_PRODUCTION_PERFORMANCE_SANITY.md": """# Production Performance Sanity

Pre-deploy timings (sample):

| Op | ms |
|----|---:|
| Dashboard | ~560 |
| Products list | ~1069 |
| Search | ~685 |

No stress testing performed. Acceptable for smoke.
""",

"14_ROLLBACK_PLAN.md": f"""# Rollback Plan

## READY (documented) — restore not rehearsed in this session

### Triggers
P0/P1, auth down, DB corruption, migration failure, cross-tenant leak, continuous 500s, critical workflow unavailable.

### Steps
1. Stop new traffic (nginx).
2. Capture logs (`docker compose logs --tail=500`).
3. Redeploy previous known-good image/commit (`afd491a` prior HEAD).
4. If schema changed: restore `/opt/backups/compliance360-<timestamp>.dump` with `pg_restore` into a **new** DB name first when possible; swap only after validation.
5. **Never** DROP DATABASE / EnsureDeleted on production.
6. Verify `/health/ready`, login, one dossier read, one document read.
7. Reopen traffic.

### Previous remote HEAD before this release
`afd491a` — `Fix evidence remove jsonb payload and add ra.spec functional coverage.`
""",

"15_FINAL_PRODUCTION_CERTIFICATION.md": f"""# Final Production Certification

## Functional certification (lab)
**100/100** preserved — Enterprise Premium Fully Certified.

## Deployment certification
**FAIL / BLOCKED** — certified source is on GitHub, but VPS was **not** updated to `{CERT}` due to missing SSH credentials.

| Gate | Status |
|------|--------|
| Certified commit | `{CERT}` |
| Remote match | YES |
| Production match | NO |
| Backup verified this session | FAIL (not run) |
| Migration | NOT REQUIRED (code) / unverified on VPS |
| Deploy | FAIL (blocked) |
| Post-deploy smoke | NOT RUN |
| Release tag | NOT CREATED |

## Verdict
**PRODUCTION DEPLOYMENT FAILED**

Resume when SSH key/access is provided: backup → checkout `{CERT}` → build/tag → up → health → post-deploy smoke → tag `v1.0.0-enterprise-premium` only after PASS.
""",
}

for name, body in docs.items():
    (OUT / name).write_text(body, encoding="utf-8")
    print("wrote", name)

# Update manifest
manifest = (OUT / "CERTIFIED_SOURCE_MANIFEST.md").read_text(encoding="utf-8")
manifest = manifest.replace(
    "## Intended CERTIFIED_RELEASE_COMMIT\n\nFilled after commit creation.\n",
    f"## CERTIFIED_RELEASE_COMMIT\n\n`{CERT}` (`{SHORT}`) — pushed to `origin/master`.\n\nLOCAL=REMOTE=YES. PRODUCTION=NOT DEPLOYED (SSH blocked).\n",
)
(OUT / "CERTIFIED_SOURCE_MANIFEST.md").write_text(manifest, encoding="utf-8")

summary = {
    "generatedAt": datetime.now(timezone.utc).isoformat(),
    "functionalCertification": "100/100",
    "certifiedReleaseCommit": CERT,
    "remoteCommit": CERT,
    "productionCommit": None,
    "commitParityCertifiedToRemote": True,
    "commitParityCertifiedToProduction": False,
    "deployment": "FAIL_BLOCKED_SSH",
    "preDeployProductionSmoke": {"PASS": 22, "FAIL": 0},
    "backupVerified": False,
    "migration": "NOT_REQUIRED_CODE_ANALYSIS",
    "releaseTag": None,
    "finalVerdict": "PRODUCTION DEPLOYMENT FAILED",
    "blocker": "No SSH private key for VPS 164.68.99.83",
}
(EV / "PRODUCTION_DEPLOYMENT_SUMMARY.json").write_text(json.dumps(summary, indent=2), encoding="utf-8")
print("summary written")
