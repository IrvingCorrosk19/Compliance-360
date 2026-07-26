# Service Health

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
