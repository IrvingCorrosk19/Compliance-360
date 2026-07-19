# 08 — DEF-0002 Closure Evidence

**Status:** CLOSED  
**Date:** 2026-07-15

## Semantics (from Excel)

### Ficha Técnica
Numeric/registry references (e.g. `102300`) on product rows — identity of the regulatory technical sheet, plus checklist req `TECH_SHEET` (copy of ficha).

### Formulario
Authority submission form reference/code; often empty historically → status `Missing`.

## Implementation

- `RegulatoryArtifactStatus` lifecycle on `MedicalDeviceProduct`
- Fields: reference, status, documentId, storedFileId, updatedAt/By for Ficha and Formulario
- `POST /products/{id}/artifacts` (ficha_tecnica | formulario)
- Versioning: attaching a new documentId marks prior as replaced/superseded path
- Dossier create auto-links TECH_SHEET requirement to product technical sheet file when present
- Import maps Excel columns into `technicalSheetReference` / `formReference` and sets Received when present

## Evidence

`def01_02_api_results.json` — DEF2-* PASS; full import `products_with_ficha` = 387.

## Matrix

- `Ficha Técnica` = **COVERED**
- `Formulario` = **COVERED**
