# 07 — Negative Test Results

| ID | Expected | Actual | Result |
|----|----------|--------|--------|
| SOD-001 self-accept | deny | 403 | PASS |
| SOD-002 specialist approve | deny | 403 | PASS |
| SOD-013 specialist submit | deny | 403 | PASS |
| SOD-003 reviewer approve | deny | 403 | PASS |
| Viewer create product/dossier | deny | 403 | PASS |
| Duplicate catalog code | reject | 400 | PASS |
| Empty product | 4xx | **500** | FAIL P2 |
| Invalid evidence .exe/.txt | reject | 400 | PASS |
| Empty registration approve | reject | 400 | PASS |
| Cross-tenant fake/platform | deny | 403 | PASS |
| Double approve/submit | controlled | 400 | PASS |
| Error body leak | no stack/SQL | detail message only | PASS |
