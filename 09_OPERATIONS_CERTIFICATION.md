# 09 — Operations Certification — Compliance 360

Date: 2026-07-03 · Method: config inspection + live health probes.

## Health checks

- **14 registered checks** (`Program.cs`): application, postgresql, storage, notification, per-provider
  notification (smtp/sendgrid/mailgun/resend), notification-queue, notification-dead-letter,
  data-protection, reporting, workflow — tagged `live`/`ready`.
- Exposed via `ObservabilityEndpoints`: `/health/live` (liveness) and `/health/ready` (readiness).
- Live probe: `GET /health/ready` → **200 Healthy** (all ready components healthy).
- `GET /health` → 200 (fast liveness ping); frontend polls `/health/ready`.

## Logging

- **Serilog** with `RenderedCompactJsonFormatter` (structured JSON to console), enriched with
  ServiceName, Environment, LogContext; `UseSerilogRequestLogging` for per-request logs incl. status +
  elapsed. EF command logging at Information (used to root-cause the update defect this cycle).

## Observability (OpenTelemetry)

- Traces: ASP.NET Core + HttpClient + EF Core instrumentation, custom ActivitySource.
- Metrics: ASP.NET Core + HttpClient + Runtime + custom Meter, **Prometheus scraping at `/metrics`**.
- Logs: OTel logging with scopes/formatted messages.
- App dashboards + alert rules exposed under `/api/v1/observability/*` (operational, performance,
  security, tenants) gated by `OBSERVABILITY.READ`.

## Startup / shutdown / recovery

- Fast-fail startup precheck: missing `Jwt:SigningKey` or connection string aborts boot with a clear
  fatal message (dev) or throws (non-dev).
- Development bootstrap runs idempotent RBAC/permission/role provisioning and reconciles existing
  tenants; runs a health check during bootstrap.
- Global exception middleware ensures no stack traces leak to clients (generic problem+json).

## Verdict

Health, logging and observability are production-grade. **Operations PASS.**
