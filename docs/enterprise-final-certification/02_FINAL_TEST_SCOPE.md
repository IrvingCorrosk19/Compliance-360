# Final Test Scope

## In scope
Regulatory Affairs critical path, RBAC/SoD, dual-tenant isolation, reporting/exports (CSV/PDF/XLSX OOXML), notifications/alerts, search, localization ES/EN runtime, responsive functional certification (6 viewports), performance regression sample, negative adversarial E2E, unit/integration regression.

## Out of scope / external
Cloud SMTP production credentials; non-RA modules beyond shared platform surfaces already certified.

## Suites executed
1. `docs/enterprise-functional-certification/_run_certification.py` (82)
2. `docs/enterprise-remediation/_run_remediation_recert.py` (32)
3. `docs/enterprise-final-certification/_run_final_certification.py` (26)
4. `dotnet test` Compliance360.Tests (325)
5. Playwright: `final-i18n-runtime`, `final-responsive-cert`, `regulatory-sod-roles`, `regulatory-workflow-v2`, `ra-spec-create-dossier`
