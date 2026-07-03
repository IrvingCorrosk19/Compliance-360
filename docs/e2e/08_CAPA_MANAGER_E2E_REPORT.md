# 8 — CAPA Manager E2E Report

Date: 2026-07-03
URL: http://localhost:5272
Browser: Chromium (Playwright)
Tenant: Alimentos Premium Panamá S.A. (`ddcaf211-afe0-44a0-9c90-4fbda8fc4871`)

## Verdict: **PASS**

## RBAC / Navigation

- Permissions (7): AUDIT.READ, AUDITMANAGEMENT.READ, CAPA.MANAGE, CAPA.READ, REPORT.READ, RISK.READ, TENANT.READ
- Visible routes: dashboard, compliance, reports, audit-trail, audits, capa, risks, regulatory, training, customer-portal
- Console errors: 0
- HTTP 5xx: 0


## Functional steps
- **Create CAPA**: expected `Success`, got `E2E-CAPA-1783044927167` → PASS
- **No CAPA.APPROVE permission (SoD)**: expected `absent`, got `absent` → PASS

## Evidence
- `artifacts/e2e/CAPA_Manager/`
- Screenshots: 01-after-login.png, functional-final.png
- Trace/video: Playwright test-output

## Corrections applied during certification
- Configuration page: RBAC-gated Storage/Notification buttons and API calls (SoD fix in app.js)
