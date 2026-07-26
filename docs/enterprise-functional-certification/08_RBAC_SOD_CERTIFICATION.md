# 08 — RBAC / SoD Certification

## Verdict: **PASS (Enterprise-grade enforcement)**

- UI states backend is source of truth; API denials confirm it.
- JWT permission claims match role expectations.
- SoD settings persisted (preventSelfReview, preventSelfApproval, separate approver/submitter, uploader/reviewer).
- Audit events include RegulatorySoDActionDenied.
- Playwright multi-role browser SoD journey **PASS**.

Residual risk: TAC JWT without operational RA approve/submit (by design) — verified.
