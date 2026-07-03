# 07 · RETEST RESULTS

Full revalidation after establishing baseline and running the certification (FASE 8 discipline: build + test + full-flow re-execution).

## Technical gates
| Gate | Result |
|---|---|
| `dotnet build -c Release` | 0 warnings / 0 errors |
| `dotnet test -c Release` | 238 / 238 PASS |
| Health `/health`, `/health/ready` | 200 Healthy |

## E2E functional (real Chrome, headed) — 29/29 PASS
- F01 Platform Administrator · F02 Tenant Administrator · F03 Tenant Security Administrator
- F04–F05 Document SoD chain (Controller creates / Quality approves, Controller cannot approve)
- F06 Auditor · F07 Supplier Manager · F08 CAPA Manager (cannot approve closure)
- F09 Risk Manager · F10 Indicators Manager · F11 Reporting Manager
- F12 Storage Administrator (UI upload) · F13 Notification Administrator
- F14 Viewer (read-only across modules) · F15 Support Operator (limited platform)
- + 14 RBAC & navigation specs (one per role)

## Business cycle harness — 23/23 PASS
Onboarding (create tenant → activate → company → branding → security → storage → notifications → users/roles) and full lifecycle (documents, audits, CAPA incl. complete-action + effectiveness + closure, risks incl. assess/treat/close by Quality, indicators, reporting, dashboards, audit trail, logout).

## Adversarial probes — 5/5 as expected
Viewer write 403 · cross-tenant read 403 · SoD close 403 · malformed body 400 · no token 401.

## Per-role result matrix
| Role | E2E | Business flow | Verdict |
|---|---|---|---|
| Platform Administrator | PASS | PASS | ✅ |
| Tenant Administrator | PASS | PASS | ✅ |
| Tenant Security Administrator | PASS | PASS | ✅ |
| Document Controller | PASS | PASS | ✅ |
| Quality Manager | PASS | PASS | ✅ |
| Auditor | PASS | PASS | ✅ |
| Supplier Manager | PASS | PASS | ✅ |
| CAPA Manager | PASS | PASS | ✅ |
| Risk Manager | PASS | PASS | ✅ |
| Indicators Manager | PASS | PASS | ✅ |
| Reporting Manager | PASS | PASS | ✅ |
| Storage Administrator | PASS | PASS | ✅ |
| Notification Administrator | PASS | PASS (send=PENDING 3rd-party) | ✅ |
| Viewer | PASS | PASS | ✅ |
| Support Operator | PASS | n/a | ✅ |

**Retest verdict:** no regressions; all roles and flows PASS.
