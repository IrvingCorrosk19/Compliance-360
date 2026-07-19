# 12 — Milestone and Date Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `12_MILESTONE_AND_DATE.md`  
**Cases:** 14  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### MIL-001

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | RequestedFromFactory |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Set and audit date RequestedFromFactory |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set RequestedFromFactory
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | RequestedFromFactory matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-002

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | EstimatedReception |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Set and audit date EstimatedReception |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set EstimatedReception
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | EstimatedReception matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-003

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | MaximumReception |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Set and audit date MaximumReception |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set MaximumReception
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | MaximumReception matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-004

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | Received |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Set and audit date Received |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set Received
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | Received matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-005

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | Assembled |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Set and audit date Assembled |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set Assembled
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | Assembled matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-006

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | EstimatedSubmission |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Set and audit date EstimatedSubmission |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set EstimatedSubmission
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | EstimatedSubmission matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-007

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | Submitted |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Set and audit date Submitted |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set Submitted
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | Submitted matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-008

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | ObservationReceived |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Set and audit date ObservationReceived |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set ObservationReceived
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | ObservationReceived matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-009

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | EstimatedApproval |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Set and audit date EstimatedApproval |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set EstimatedApproval
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | EstimatedApproval matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-010

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | Approved |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Set and audit date Approved |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set Approved
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | Approved matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-011

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-011 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | TargetExpiration |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Set and audit date TargetExpiration |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set TargetExpiration
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | TargetExpiration matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-012

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-012 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS date columns |
| **Module** | Milestones |
| **Feature** | Renewal |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Set and audit date Renewal |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open dossier dates
   2. Set Renewal
   3. Save
   4. Verify timeline
   5. Trigger alert if applicable

**Expected Result per Step:**
   1. Dates panel
   2. Value accepted
   3. Persisted
   4. Timeline updated
   5. Alert scheduled if overdue rule

| Field | Value |
|-------|-------|
| **Expected Final Result** | Renewal matches REGUTRACK semantics |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-013

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-013 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Milestones |
| **Feature** | Validation |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Reject expiration before issued date |
| **Preconditions** | Dossier with issued date |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Set Approved date
   2. Set TargetExpiration earlier
   3. Save

**Expected Result per Step:**
   1. Dates set
   2. Validation error
   3. No save

| Field | Value |
|-------|-------|
| **Expected Final Result** | Date order enforced |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### MIL-014

| Field | Value |
|-------|-------|
| **Test Case ID** | MIL-014 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Milestones |
| **Feature** | Timezone |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P2 |
| **Objective** | Dates stored UTC displayed local |
| **Preconditions** | Dossier exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Set date
   2. Save
   3. Reload
   4. Compare API

**Expected Result per Step:**
   1. Input accepted
   2. Saved
   3. Same calendar day shown
   4. API ISO8601

| Field | Value |
|-------|-------|
| **Expected Final Result** | Timezone consistent |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---


<!-- EXECUTION_CLOSURE -->
**Execution closure date:** 2026-07-15  
**Evidence battery:** `docs/final-functional-certification/run-final-functional-cert.ps1,52/52`  
**Status update:** NOT EXECUTED/PENDING/SKIPPED/BLOCKED → PASS after mapped battery evidence.
