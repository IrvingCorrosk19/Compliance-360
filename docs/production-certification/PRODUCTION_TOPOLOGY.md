# PRODUCTION_TOPOLOGY

## Discovery method
Public HTTP(S) probes + repository `docker-compose.yml` + prior runbooks. **No SSH shell access available from this workstation** at certification time (`Permission denied (publickey,password)` for tried users).

## Host

| Item | Value |
|------|-------|
| Public IP | `164.68.99.83` |
| Primary URL | `https://compliance360.164.68.99.83.nip.io` |
| Direct app (loopback-intended) | `http://164.68.99.83:8085` (currently reachable externally — note for hardening) |
| Reverse proxy | nginx/1.24.0 (Ubuntu) — observed via response headers |
| HTTPS | Present; HSTS `max-age=2592000` |
| Security headers | `X-Content-Type-Options`, `X-Frame-Options DENY`, `Referrer-Policy`, `Permissions-Policy`, CSP |

## Application stack (from compose + runtime)

| Service | Mechanism | Notes |
|---------|-----------|-------|
| Web/API | Docker `compliance360_web` | ASP.NET Core on `:8080` inside container; host bind historically `127.0.0.1:8086` (compose) / publicly seen via `:8085` proxy path |
| Worker | Docker `compliance360_worker` | Alert Center worker (`Compliance360.Worker.dll`) |
| PostgreSQL | Docker `compliance360_postgres` (postgres:18) | Not exposed publicly in compose design |
| Mailpit | Optional profile `sandbox` | Lab only; production SMTP external |
| Volumes | `compliance360_postgres_data`, `compliance360_storage`, `compliance360_dataprotection_keys` | Persist DB, documents, DP keys |
| Network | `compliance360_net` | Bridge |

## Environment configuration
Expected via VPS `.env` (not in Git): `POSTGRES_*`, `JWT_SIGNING_KEY`, SMTP provider secrets, connection strings.

## Health endpoints (verified 2026-07-26)

| Endpoint | Result |
|----------|--------|
| HTTPS `/health` | Healthy |
| HTTPS `/health/live` | Healthy |
| HTTPS `/health/ready` | Healthy |
| HTTP `:8085` health | Healthy |

## Deployment access status
**BLOCKED:** No SSH private key present on the operator workstation. Push of certified commit completed; VPS checkout/build/restart pending operator SSH credentials.
