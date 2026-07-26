# 06 — Regulatory E2E Evidence

## Controlled dossier
- **Case:** RA-20260726-3308
- **DossierId:** d61c1305-e6d1-43c6-9b28-e854762510cb
- **ProductId:** c4864567-7266-406b-a9fe-6fb1df3857da · code EFC-AE270A44
- **Terminal status:** **Closed** (confirmed)

## Flow executed (real)
1. Specialist creates product + dossier (22 requirements from pack)
2. SoD negatives: self-accept / approve / submit denied
3. Transitions: WaitingManufacturerDocuments → DocumentsReceived → Assembling
4. Upload PDF evidence; attach Received to all required requirements
5. V2 	echnical-review/start → UnderTechnicalReview
6. Reviewer accepts required; V2 	echnical-review/complete → ReadyForSubmission
7. Approver pprove-for-submission → ApprovedForSubmission
8. Submitter submit with proofStoredFileId → Submitted
9. Manager authority-review/start → UnderAuthorityReview
10. Observation opened → respond/close → CorrectingObservation → resubmit → UnderAuthorityReview
11. Manager pprove with resolutionStoredFileId → **Closed**

## Playwright

egulatory-sod-roles.spec.ts — **1 passed (16.9s)**

## Browser
Specialist login + #/regulatory console visible (role badge specialist, queue 109 dossiers). Evidence screenshots via MCP.
