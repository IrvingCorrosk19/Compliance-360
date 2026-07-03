# 01 — Production Readiness Master Plan — Compliance 360

Date: 2026-07-03 · Environment: Development (authorized for full hardening) · DB: PostgreSQL 18 (localhost).

## Objectives

Certify Compliance 360 as an Enterprise product that can **objectively demonstrate** production readiness:
stability, security, performance, consistency, maintainability and operability. No new modules; no scope
expansion. Every claim must be backed by reproducible evidence.

## Scope

- **In scope:** Frontend (SPA), Backend, API, Identity/JWT, RBAC, multi-tenant isolation, Documents,
  Workflow, Suppliers, Audits, CAPA, Risk, Indicators, Reporting, Storage, Notifications, Audit trail,
  Dashboards, Configuration, Health, Logging, Observability, DB (migrations/constraints/indexes/backup),
  Bootstrap, tests, scripts, docs.
- **Out of scope (documented as PENDING THIRD-PARTY):** real SMTP delivery, cloud storage (Azure/S3/MinIO),
  SSO/OIDC/SAML, LDAP/AD, external AI, digital signature, payment gateways.

## Methodology

Evidence-driven. For each phase: (1) inspect implementation, (2) execute a concrete probe (command,
Playwright test, SQL, HTTP request), (3) record actual result, (4) fix root cause architecturally if a
defect is found, (5) re-run. Fixes must reduce technical debt, never add workarounds.

## Order of work

0. Master plan (this document).
1. Architecture audit (warnings, dead code, TODO/FIXME/HACK, unprotected endpoints, orphan permissions).
2. E2E validation (build + test + Playwright, all roles).
3. UAT full lifecycle (create → version → submit → approve; CAPA open→close; audit; risk; indicator; report).
4. RBAC certification (permissions, policies, claims, JWT, SoD, escalation, IDOR).
5. Multi-tenant certification (attempt cross-tenant breaches; expect denial).
6. Security certification (OWASP Top 10, headers, JWT, rate limiting, secrets, password policy, MFA, lockout, HTTPS).
7. Performance (measure real latencies; optimize only with evidence).
8. Database (migrations, integrity, constraints, indexes, FK, soft delete, audit, seeds, backup/restore).
9. Operations (health checks, OpenTelemetry, Serilog, startup/shutdown, background services, bootstrap).
10. Third parties (probe; classify missing credentials as PENDING with exact setup steps).
11. Cleanup (dead code, unused deps, temp scripts, warnings, duplicates — no compatibility breakage).
12. Documentation (align docs to implementation; no duplicates).

## Dependencies

.NET 9 SDK, PostgreSQL 18 (`C:\Program Files\PostgreSQL\18\bin`), Node/Playwright + Chromium, connection
string already configured via user secrets.

## Risks

- Deep business lifecycle flows may reveal gaps not covered by shallow create tests.
- Dev security relaxations (MFA off, force-password off for testing) must not mask real onboarding flow.
- Third-party integrations cannot be fully certified locally.
- Backup/restore and load/concurrency require deliberate probes.

## PASS / FAIL criteria

- **PASS (phase):** probe executed, expected result observed, no critical defect open.
- **FAIL:** any critical defect (broken access control, tenant leak, data loss, unhandled 5xx on core flow,
  build/test failure) left uncorrected.
- **PENDING THIRD-PARTY:** capability requires external credentials/infra not available locally.

## Final verdict (one of)

`PRODUCTION READY` · `PRODUCTION READY WITH THIRD-PARTY PENDING CONFIGURATION` · `NOT READY FOR PRODUCTION`.
No intermediate states. Justified with evidence.
