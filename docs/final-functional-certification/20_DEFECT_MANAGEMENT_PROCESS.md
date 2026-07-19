# 20 — Defect Management Process

**Program:** Final REGUTRACK Replacement Certification  
**Date:** 2026-07-14

---

## 1. Defect lifecycle

```
Found → Logged (DEF-XXXX) → Triaged → Assigned → Fixed → Verified → Closed
                                    ↘ Waiver (signed) → Accepted Risk
```

---

## 2. Defect record location

`docs/final-functional-certification/defects/DEF-XXXX.md`

---

## 3. Required fields

| Field | Description |
|-------|-------------|
| ID | DEF-0001 sequential |
| Title | Short description |
| Severity | Critical / High / Medium / Low |
| Priority | P0–P3 |
| Module | RA domain |
| REGUTRACK impact | Sheet/column affected |
| Role | Actor during failure |
| Test Case ID | Link to failing TC |
| Steps to reproduce | Numbered |
| Expected vs Actual | Clear contrast |
| Evidence | Screenshot, API, DB, logs |
| Root cause | After analysis |
| Fix | PR/commit reference |
| Regression risk | Modules affected |
| Retest TCs | List |
| Status | Open / Fixed / Verified / Waived / Closed |

---

## 4. Severity definitions

| Severity | Definition | SLA |
|----------|------------|-----|
| **Critical** | Blocks REGUTRACK replacement, data loss, tenant leak, approval bypass | Fix immediately; STOP testing |
| **High** | Critical regulatory flow wrong/incomplete | Fix before GO |
| **Medium** | Important function with safe workaround | Fix or waiver before GO |
| **Low** | UX/cosmetic | Backlog |

---

## 5. Fix workflow

1. Register defect — do not hide  
2. Reproduce with evidence  
3. Root cause analysis  
4. Implement fix in repo  
5. `dotnet build` + targeted tests  
6. Re-run failing TC  
7. Re-run module regression  
8. Re-run SoD if RBAC/workflow touched  
9. Re-run journey if dossier flow touched  
10. Close only with execution evidence  

---

## 6. Waiver process

- Only PO + QA Manager can approve  
- Document in `WAIVERS.md` with expiry  
- Linked TC remains FAIL until waiver; GO may proceed if waiver covers gap  

---

## 7. Metrics

Track open Critical/High count daily during execution. **GO requires Critical=0, High=0.**

---

*Defect register starts empty at design phase.*
