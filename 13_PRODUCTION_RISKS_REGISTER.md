# 13 — Production Risks Register — Compliance 360

Date: 2026-07-03. Severity: 🔴 high · 🟠 medium · 🟢 low. All critical defects found this cycle are
CLOSED (see below).

## Closed this cycle

| # | Risk | Sev | Status |
|---|---|---|---|
| C1 | Add-child-on-update issued UPDATE→500 across all modules (EF key convention) | 🔴 | **CLOSED** — global `ValueGeneratedNever`; re-validated CAPA + Risk + full suites |
| C2 | Non-UTC `DateTimeOffset` write → 500 | 🔴 | **CLOSED** — global UTC normalization converter |
| C3 | Malformed request body → 500 instead of 400 | 🟠 | **CLOSED** — middleware returns 400 |

## Open risks (mitigations)

| # | Risk | Sev | Mitigation / Owner action |
|---|---|---|---|
| R1 | Third-party integrations (SMTP, cloud storage, SSO, LDAP, signature, AI) unconfigured/untested | 🟠 | Provide credentials, run live tests (§11); providers + health checks already implemented |
| R2 | No automated backups / PITR / restore drill at infra layer | 🔴 | Configure managed backups + PITR; run staging restore drill before go-live |
| R3 | Deploy-time config (`Jwt:SigningKey`, connection string, CORS, AllowedHosts) not yet set for prod | 🔴 | Set via secret store; verified fast-fail if missing |
| R4 | No load/soak testing performed | 🟠 | Run load test with representative data before high-volume scale |
| R5 | E2E emphasises create + SoD; full per-module closure chains not each browser-driven | 🟠 | Extend Playwright suite; transitions verified functional via API this cycle |
| R6 | Log/metric sinks not wired to central platform | 🟢 | Wire OTLP/Prometheus/log sink; in-app alert rules ready |
| R7 | TLS termination/HSTS effective only under HTTPS | 🟢 | Terminate TLS at edge; HSTS auto-enables in non-dev |

## Overall

No open **critical code** defects. Remaining critical items (R2, R3) are infrastructure/deploy-time
configuration, not application defects.
