# 14 — Negative Test Results

**Status:** pending evidence from harness

## Required negatives

- `PROD-NEG-EMPTY` — empty product → 4xx not 500
- `NEG-EMPTY-PRODUCT-VIEWER` — viewer denied
- `NEG-EXPORT-UNAUTH` — unauthorized export not 2xx
- `MT-CROSS-TENANT` — foreign tenant denied
