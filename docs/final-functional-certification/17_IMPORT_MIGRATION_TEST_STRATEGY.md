# 17 — Import Migration Test Strategy

**Program:** Final REGUTRACK Replacement Certification  
**Contract file:** `REGUTRACK 02JUN26 MG.xlsx` (copy only — never modify original)  
**Date:** 2026-07-14

---

## 1. Purpose

Prove Compliance 360 can ingest historical REGUTRACK data for migration while daily operations no longer require Excel.

---

## 2. Import pipeline stages (each needs TC)

| Stage | API | Validation |
|-------|-----|------------|
| 1. Upload | POST `/imports/xlsx` | File received, job created |
| 2. Sheet detection | Job metadata | CTT REGISTROS, TUBERIA, DOCUMENTACION, LICENCIAS OP |
| 3. Preview / mapping | GET job rows | Column mapping matches decomposition |
| 4. Normalization | Row validation | Dates, enums, authority codes |
| 5. Duplicate detection | Row warnings | Same catalog code + authority |
| 6. Simulation | Dry-run counts | No commit |
| 7. Commit | POST `.../commit?maxRows=` | Entities created |
| 8. Reconciliation report | Export/log | Row → domain object trace |

---

## 3. Reconciliation format (mandatory)

| Sheet | Row | Source Identity | Expected Objects | Created/Matched | Warnings | Errors | Status |
|-------|-----|-----------------|------------------|-----------------|----------|--------|--------|

---

## 4. Negative import scenarios

- Corrupt XLSX
- Missing sheet
- Renamed column header
- Invalid date in Fecha Criterio
- Repeat import (idempotency)
- Cancel mid-commit
- Rollback if supported (or FAIL + WAIVER)

---

## 5. Test catalog

`test-cases/25_REGUTRACK_IMPORT_TEST_CASES.md` — IMP-001 … IMP-016

---

## 6. GO dependency

Import PASS is **required** for GO RETIRE EXCEL per `18_ENTRY_EXIT_CRITERIA.md` criterion G10.

---

*All import TC: NOT EXECUTED.*
