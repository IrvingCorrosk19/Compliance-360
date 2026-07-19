# 09 — DEF-0003 Closure Evidence

**Status:** CLOSED  
**Date:** 2026-07-15

## Source manifest

See `01_REGUTRACK_SOURCE_MANIFEST.json` (original SHA-256 == controlled copy).

## Full dataset

| Metric | Value |
|--------|-------|
| Staged rows (parser) | 715 |
| Warnings | 10 |
| Errors | 0 |
| Committed rows (run1) | 614 |
| Committed rows (run2 same file) | 614 |
| Product Δ run2 | 0 |
| License Δ run2 | 0 |
| Certificate Δ run2 | 0 |

## Artifacts

- `02_REGUTRACK_FULL_ROW_RECONCILIATION.csv` (347 identity rows spanning REGISTROS + LICENCIAS + DOCUMENTACION; fail_marked=0)
- `02_REGUTRACK_FULL_ROW_RECONCILIATION_SUMMARY.json`
- `03_REGUTRACK_IMPORT_IDEMPOTENCY_REPORT.md`
- `def03_results.json` → PASS

## Product fixes enabling closure

- Commit match-by-catalog/name (no silent duplicate products)
- Certificate match-by-manufacturer+type
- License match-by-company+type + company corporate dates

## Rollback

Staged (uncommitted) job → `RolledBack` PASS. Committed data compensated via idempotent match merge.
