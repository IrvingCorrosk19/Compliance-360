# 02 — Architecture Audit — Compliance 360

Date: 2026-07-03 · Method: static inspection + live probes.

## Build & code hygiene (evidence)

| Check | Command | Result |
|---|---|---|
| Release build | `dotnet build -c Release` | **Build succeeded — 0 Warning(s), 0 Error(s)** |
| Tech-debt markers | grep `TODO/FIXME/HACK/XXX/NotImplemented` in `src` | **0 matches** |
| Unit tests | `dotnet test` | **238 passed / 0 failed** |
| Solution layering | Domain ← Application ← Infrastructure ← Web | Clean dependency direction |

## API surface & protection

- `FoundationEndpoints.cs`: **100 mapped endpoints, 100 authorization declarations.**
- Only intentionally anonymous endpoints: `/auth/login`, `/auth/mfa/complete`, `/auth/refresh`,
  `/health`, `/health/live`, `/health/ready`, `/metrics` and static SPA files. Everything under
  `/api/v1/**` requires a granular permission policy (verified: anonymous request → **401**).
- No `AllowAnonymous` scattered across business endpoints.

## Middleware pipeline (Program.cs)

`GlobalExceptionMiddleware → SecurityHeadersMiddleware → Serilog request logging → localization →
static files → Swagger → HTTPS redirect → (HSTS prod) → CORS → RateLimiter → Authentication →
AuditContext → Observability → Authorization`. Order is correct (auth before authorization; audit/
observability after authentication so identity is available).

## Modules present

TenantManagement, Identity, Audit trail, Storage, Notifications, Documents, Workflows, TechnicalSheets,
Suppliers, AuditManagement, CAPA, Risk, QualityIndicators, Reporting, EnterpriseWorkspaces,
Observability. All wired through `AddInfrastructure` + catalog-driven RBAC.

## Defects found during audit (and fixed)

1. **CRITICAL — Add-child-on-update issued UPDATE instead of INSERT (all modules).** Domain entities
   assign their own `Guid Id` in the constructor, but EF convention treated Guid keys as
   store-generated, so new children added to a *tracked* aggregate during an update were mistaken for
   existing rows → `DbUpdateConcurrencyException` (0 rows) → HTTP 500. Latent because E2E only
   exercised creates. **Fixed** with a global `ValueGeneratedNever()` convention on all `Entity.Id`
   keys (no schema change). Re-validated on CAPA (classify/root-cause/5-why/corrective-action) and
   Risk (assessment).
2. **CRITICAL — Non-UTC `DateTimeOffset` write crash.** PostgreSQL `timestamptz` only accepts UTC;
   a client sending a local offset (e.g. −05:00) caused an Npgsql write failure (500). **Fixed** with
   a global EF value converter normalizing every `DateTimeOffset` to UTC on write.
3. **MINOR — Malformed request body returned 500.** `GlobalExceptionMiddleware` treated
   `BadHttpRequestException` as an unhandled error. **Fixed** to return **400 Bad Request**, keeping
   server-error metrics/alerts clean.

## Result

Architecture is clean and layered; the three defects above were corrected at the architectural level
(global, debt-reducing) and re-validated. **No open architectural defects.**
