# 03 — REGUTRACK Import Idempotency Report

## Runs
- Full commit #1 job `b7b22a69-10b8-4d9b-b12b-b66df61b10c7` status=200 imported=614
- Full commit #2 job `46624647-12f7-4e75-8e8e-4fc6b394e816` status=200 imported=614

## Counts
| Metric | Before | After #1 | After #2 | Δ#2 |
|--------|--------|----------|----------|-----|
| Products | 482 | 482 | 482 | 0 |
| Registrations | 360 | 360 | 360 | 0 |
| Licenses | 31 | 31 | 31 | 0 |
| Certificates | 311 | 311 | 311 | 0 |
| Licenses w/ ConstitutionDate | 9 | 9 | 9 | — |
| Products w/ Ficha ref | 387 | 387 | 387 | — |

## Verdict
**Idempotency = PASS**

```json
{
  "product_growth_run2": 0,
  "license_growth_run2": 0,
  "certificate_growth_run2": 0,
  "pass": true,
  "note": "Match-by-catalog/name; residual growth only if unlabeled catalog collisions"
}
```

## Rollback
Committed jobs cannot mark RolledBack under current domain rule (only Simulated/Validated/Failed).
Compensation: re-import uses match semantics (no duplicate products/licenses/certs).
Rollback of a **fresh staged** job:

- Staged job `21064773-44a9-4075-b550-d74740141914` rollback http=200 status=RolledBack
- Compensation for committed data: match/merge on second full import (no silent duplicate).
