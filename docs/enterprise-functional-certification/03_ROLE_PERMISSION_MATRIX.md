# 03 — Role / Permission Matrix (Executed)

| Role | Login | RA Console | Create Product/Dossier | Review/Accept req | Approve-for-submission | Submit | External approve | Viewer read |
|------|-------|------------|------------------------|-------------------|------------------------|--------|------------------|-------------|
| Regulatory Specialist | PASS | PASS | PASS | Self-accept DENY | DENY 403 | DENY 403 | — | — |
| Regulatory Reviewer | PASS | — | — | PASS (required) | DENY 403 | DENY 403 | — | — |
| Regulatory Approver | PASS | — | — | — | PASS | DENY 403 | — | — |
| Regulatory Submitter | PASS | — | — | — | DENY 403 | PASS (+proof) | — | — |
| Regulatory Manager | PASS | — | — | — | — | — | PASS (+resolution after obs closed) | — |
| Regulatory Viewer | PASS | — | DENY 403 | — | — | — | — | PASS |
| Regulatory Administrator | PASS | bootstrap PASS | — | — | — | — | — | — |
| Tenant Administrator | PASS | — | — | — | JWT without RA ops claims | — | — | — |

JWT claim assertions: all planned CLAIM-* tests **PASS**.
SoD settings: preventSelfReview=true (+ related flags present).
