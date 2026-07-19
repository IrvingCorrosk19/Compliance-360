# 07 — DEF-0001 Closure Evidence

**Status:** CLOSED  
**Date:** 2026-07-15

## Semantics (from Excel)

`CTT LICENCIAS OP` rows 2–3:

| Row label | Multimed | 4 Hospitals |
|-----------|----------|-------------|
| Fecha de Constitución | 2003-01-17 | 2012-01-06 |
| Inicio de Operaciones | 2003-01-01 | 2013-08-01 |

These are **company corporate calendar dates**, not license IssuedOn/ExpiresOn.

## Implementation

- `OperatingLicense.CompanyConstitutedOn` / `OperationsStartedOn` as `DateOnly?` (PostgreSQL `date`)
- Create + PUT `/operating-licenses/{id}/company-dates`
- Import parser embeds company header dates into each license row
- UI Licencias columns Constitución / Inicio ops
- Migration `20260715010201_AddCompanyCorporateDatesAndProductArtifacts`

## Evidence

`def01_02_api_results.json` — all DEF1-* PASS (create both/null, edit ops without clobbering constitution, list, viewer deny, cross-tenant).

DB: `licenses_with_constitution` ≥ 9 after full import.

## Matrix

`CTT LICENCIAS OP` company dates = **COVERED**
