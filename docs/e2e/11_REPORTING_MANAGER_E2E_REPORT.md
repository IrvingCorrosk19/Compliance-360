# 11 — Reporting Manager E2E Report

Date: 2026-07-03
URL: http://localhost:5272
Browser: Chromium (Playwright)
Tenant: Alimentos Premium Panamá S.A. (`ddcaf211-afe0-44a0-9c90-4fbda8fc4871`)

## Verdict: **PASS**

## RBAC / Navigation

- Permissions (11): AUDIT.READ, AUDITMANAGEMENT.READ, CAPA.READ, INDICATOR.READ, REPORT.EXECUTE, REPORT.EXPORT, REPORT.MANAGE, REPORT.READ…
- Visible routes: dashboard, compliance, reports, audit-trail, audits, capa, risks, indicators, regulatory, training, customer-portal
- Console errors: 0
- HTTP 5xx: 0


## Functional steps
- **Report Center loads**: expected `Visible`, got `OK` → PASS
- **Seed standard reports**: expected `Success toast`, got `OK` → PASS
- **Execute report**: expected `No error`, got `Executed` → PASS

## Evidence
- `artifacts/e2e/Reporting_Manager/`
- Screenshots: 01-after-login.png, functional-final.png
- Trace/video: Playwright test-output

## Corrections applied during certification
- Configuration page: RBAC-gated Storage/Notification buttons and API calls (SoD fix in app.js)
