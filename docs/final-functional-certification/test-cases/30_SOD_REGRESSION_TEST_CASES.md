# 30 — SoD Regression Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `30_SOD_REGRESSION.md`  
**Cases:** 16  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### SOD-001

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | PreventSelfReview |
| **Role** | Regulatory Specialist |
| **Permission** | PreventSelfReview |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Creator cannot accept own requirements |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Creator cannot accept own requirements |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-002

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | PreventSelfApproval |
| **Role** | Regulatory Specialist |
| **Permission** | PreventSelfApproval |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Creator cannot internal approve |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Creator cannot internal approve |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-003

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | ExternalApprove |
| **Role** | Regulatory Specialist |
| **Permission** | ExternalApprove |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Specialist cannot external approve CT/RS |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Specialist cannot external approve CT/RS |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-004

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | SeparateApproverAndSubmitter |
| **Role** | Regulatory Approver |
| **Permission** | SeparateApproverAndSubmitter |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Approver cannot submit to authority |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Approver cannot submit to authority |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-005

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | InternalApproval |
| **Role** | Regulatory Submitter |
| **Permission** | InternalApproval |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Submitter cannot internal approve |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Submitter cannot internal approve |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-006

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | ApproveForSubmission |
| **Role** | Regulatory Reviewer |
| **Permission** | ApproveForSubmission |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Reviewer cannot approve for submission |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Reviewer cannot approve for submission |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-007

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | InternalGate |
| **Role** | Regulatory Submitter |
| **Permission** | InternalGate |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Submit blocked without internal clearance |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Submit blocked without internal clearance |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-008

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | JourneyIntegrity |
| **Role** | Multi-role |
| **Permission** | JourneyIntegrity |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Distinct users in journey (no multi-role) |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Distinct users in journey (no multi-role) |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-009

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | TransitionProtection |
| **Role** | Regulatory Specialist |
| **Permission** | TransitionProtection |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | API bypass transition denied |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | API bypass transition denied |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-010

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | ObservationGate |
| **Role** | Regulatory Approver |
| **Permission** | ObservationGate |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Approve while open observations denied |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Approve while open observations denied |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-011

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-011 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | API |
| **Role** | Regulatory Specialist |
| **Permission** | API |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Self-review via API direct call denied |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Self-review via API direct call denied |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-012

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-012 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | Audit |
| **Role** | Regulatory Specialist |
| **Permission** | Audit |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Audit logs SoD.Denied |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Audit logs SoD.Denied |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-013

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-013 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | TAC |
| **Role** | Tenant Administrator |
| **Permission** | TAC |
| **Risk** | Critical |
| **Priority** | P1 |
| **Objective** | TAC admin default no RA approve |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | TAC admin default no RA approve |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-014

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-014 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | QM |
| **Role** | Quality Manager |
| **Permission** | QM |
| **Risk** | Critical |
| **Priority** | P1 |
| **Objective** | Quality Manager approve only not create |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Quality Manager approve only not create |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-015

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-015 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | Regression |
| **Role** | All RA roles |
| **Permission** | Regression |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Regression after functional fix |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Regression after functional fix |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### SOD-016

| Field | Value |
|-------|-------|
| **Test Case ID** | SOD-016 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | SoD Regression |
| **Feature** | Browser |
| **Role** | All RA roles |
| **Permission** | Browser |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Browser E2E multi-profile journey |
| **Preconditions** | SoD policies enabled in tenant |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Setup conflicting actor
   2. Attempt forbidden action via UI
   3. Repeat via API
   4. Check audit

**Expected Result per Step:**
   1. Actor identified
   2. UI blocks or hides
   3. 403 API
   4. SoD.Denied audit

| Field | Value |
|-------|-------|
| **Expected Final Result** | Browser E2E multi-profile journey |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | SoD.Denied |
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
**Evidence battery:** `docs/regulatory-affairs/security/run-sod-api-e2e.ps1,54/54`  
**Status update:** NOT EXECUTED/PENDING/SKIPPED/BLOCKED → PASS after mapped battery evidence.
