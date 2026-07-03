# 12 — Production Go-Live Checklist — Compliance 360

Date: 2026-07-03. Legend: [x] done/verified · [ ] deploy-time action.

## Application

- [x] Release build clean (0 warnings / 0 errors)
- [x] Unit tests green (238/238)
- [x] E2E green (29/29, real browser)
- [x] No TODO/FIXME/HACK in `src`
- [x] Global exception handling (no stack traces to clients)
- [x] Critical defects found this cycle fixed and re-validated

## Configuration (deploy-time)

- [ ] Set `Jwt:SigningKey` (strong secret) via env/secret store
- [ ] Set `ConnectionStrings:Compliance360` via env/secret store
- [ ] Set `Cors:AllowedOrigins` to the real frontend origin(s)
- [ ] Restrict `AllowedHosts` from `*` to the production host(s)
- [ ] Configure third-party providers as needed (see §11)

## Security

- [x] Security headers (CSP, HSTS, X-Frame-Options DENY, nosniff, no-store on API)
- [x] JWT full validation; 15-min access / 30-day refresh
- [x] Password policy (12+, complexity, history 5) and lockout (5/15min)
- [x] Rate limiting (120/min/IP)
- [ ] TLS certificate provisioned at the edge; force HTTPS

## Data

- [x] Migrations applied and consistent (15)
- [x] Referential integrity (92 FKs) + uniqueness (188 unique indexes)
- [x] Logical backup verified (`pg_dump`)
- [ ] Automated scheduled backups + point-in-time recovery configured
- [ ] Restore drill executed in staging

## Operations

- [x] Health `/health/live` + `/health/ready` (14 checks) green
- [x] Structured logging (Serilog JSON) + request logging
- [x] OpenTelemetry traces/metrics/logs; Prometheus `/metrics`
- [ ] Log/metric sinks wired to central platform (Loki/ELK, Prometheus/Grafana, OTLP collector)
- [ ] Alerting connected to on-call (alert rules already defined in-app)

## Pre-scale (recommended)

- [ ] Load/soak test with representative dataset
- [ ] Extend E2E to full per-module closure lifecycles
