# Pre-Deployment Baseline

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

**Note:** This proves current production is operational; it does **not** prove the certified SHA `1dd34a6` is running.
