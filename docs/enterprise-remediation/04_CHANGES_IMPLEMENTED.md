# 04 — Changes Implemented

## Product / Validation
- `CreateProductAsync`: Guard required fields before catalog existence check
- `ProductCatalogExistsAsync`: null/whitespace safe (no Trim NRE)

## Reporting
- TAC RoleCatalog: `REPORT.READ/EXECUTE/EXPORT`
- `GetExportContentAsync` + `ReportExportContentBuilder` (CSV/JSON/SpreadsheetML/PDF)
- `GET .../exports/{id}/content`
- Excel MIME/extension aligned to SpreadsheetML
- List reports: default `page=1`, `pageSize=25`

## Notifications / Alert Center
- `MapAlertCenter` on `/api/v1` and `/api/v2`
- Occurrence list + acknowledge/resolve/escalate lifecycle service
- Mailpit sandbox verified via Provider Center

## Bootstrap / Health
- Worker stale → Degraded (not Unhealthy)
- Bootstrap filters ready/live tags

## Observations / Workflow
- `OpenObservationResultDto` with nested observation/dossier + legacy `Id`
- External approve: optional proof file; auto-advance when all observations closed

## Search / UX / i18n
- PostgreSQL ILike multi-field (+ InMemory fallback)
- `resolvePostLoginLanding` in `app.js`
- Partial hardcoded string → locale keys

## Tests
- RegulatoryProductValidationTests, AlertOccurrenceLifecycleTests
- ReportingEngineTests export content assertion
- Remediation harness `_run_remediation_recert.py`
