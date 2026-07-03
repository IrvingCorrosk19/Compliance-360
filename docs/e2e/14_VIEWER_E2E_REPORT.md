# 14 — Viewer E2E Report

Date: 2026-07-03
URL: http://localhost:5272
Browser: Chromium (Playwright)
Tenant: Alimentos Premium Panamá S.A. (`ddcaf211-afe0-44a0-9c90-4fbda8fc4871`)

## Verdict: **PASS**

## RBAC / Navigation

- Permissions (11): AUDIT.READ, AUDITMANAGEMENT.READ, CAPA.READ, DOCUMENT.READ, INDICATOR.READ, NOTIFICATION.READ, REPORT.READ, RISK.READ…
- Visible routes: dashboard, compliance, reports, audit-trail, documents, technical-sheets, suppliers, audits, capa, risks, indicators, regulatory, training, supplier-portal, customer-portal, configuration
- Console errors: 0
- HTTP 5xx: 0


## Functional steps
- **Read-only documents**: expected `No action form`, got `form=0` → PASS
- **Read-only suppliers**: expected `No action form`, got `form=0` → PASS
- **Read-only capa**: expected `No action form`, got `form=0` → PASS
- **Read-only risks**: expected `No action form`, got `form=0` → PASS
- **Read-only indicators**: expected `No action form`, got `form=0` → PASS
- **Read-only audits**: expected `No action form`, got `form=0` → PASS
- **API create denied**: expected `403`, got `403` → PASS

## Evidence
- `artifacts/e2e/Viewer/`
- Screenshots: 01-after-login.png, functional-final.png
- Trace/video: Playwright test-output

## Corrections applied during certification
- Configuration page: RBAC-gated Storage/Notification buttons and API calls (SoD fix in app.js)
