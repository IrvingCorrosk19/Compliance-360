# 07 — SoD Policy Design

## Entidad: `RegulatorySoDSettings` (1 fila / tenant)

| Flag | Default regulado | Efecto |
|------|------------------|--------|
| PreventSelfReview | true | Creator/owner ≠ Accept/Reject |
| PreventSelfApproval | true | Creator/reviewer ≠ ApproveForSubmission |
| SeparateApproverAndSubmitter | true | InternallyApprovedBy ≠ Submitter |
| SeparateDocumentUploaderAndReviewer | true | (fase 2 hard-link) |
| RequireSecondApprovalForCriticalWaiver | true | Waive Critical exige Approver/Manager distinto |
| RequireApprovalForCriticalityChange | true | (Manager + audit) |
| RequireApprovalForExternalDecisionRecording | false | Manager/QM con APPROVE |
| AllowEmergencyOverride | true | Con permiso SOD.EMERGENCY_OVERRIDE |
| EmergencyOverrideRequiresReason | true | ≥ 15 chars |
| EmergencyOverrideRequiresSecondaryReview | true | Flag `PendingSecondaryReview` en audit |
| RequireInternalApprovalBeforeSubmission | true | Solo `ApprovedForSubmission` → `Submitted` |

## Ecuación de autorización

```text
Permission claim  ∧  Resource (tenant + existence)
  ∧  Workflow state allowed  ∧  SoD policy  ∧  (¬override ∨ audited override)
  → ALLOW | DENY(SoDActionDenied)
```

Config API: `GET/PUT .../regulatory/sod-settings` · policy `Regulatory.Configure` o `SOD.MANAGE`.
