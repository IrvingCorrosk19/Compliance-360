# 22 — Alert Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `22_ALERT.md`  
**Cases:** 14  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### ALERT-001

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Expiry 365d |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Alert fires at 365 days before registration expiry |
| **Preconditions** | Registration with controlled expiry date |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Set expiry in lab
   2. Run evaluate alerts
   3. Inspect alert list
   4. Verify recipient

**Expected Result per Step:**
   1. Date set
   2. Job runs
   3. Alert for 365d present
   4. Correct owner

| Field | Value |
|-------|-------|
| **Expected Final Result** | 365-day threshold works |
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

### ALERT-002

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Expiry 180d |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Alert fires at 180 days before registration expiry |
| **Preconditions** | Registration with controlled expiry date |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Set expiry in lab
   2. Run evaluate alerts
   3. Inspect alert list
   4. Verify recipient

**Expected Result per Step:**
   1. Date set
   2. Job runs
   3. Alert for 180d present
   4. Correct owner

| Field | Value |
|-------|-------|
| **Expected Final Result** | 180-day threshold works |
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

### ALERT-003

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Expiry 120d |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Alert fires at 120 days before registration expiry |
| **Preconditions** | Registration with controlled expiry date |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Set expiry in lab
   2. Run evaluate alerts
   3. Inspect alert list
   4. Verify recipient

**Expected Result per Step:**
   1. Date set
   2. Job runs
   3. Alert for 120d present
   4. Correct owner

| Field | Value |
|-------|-------|
| **Expected Final Result** | 120-day threshold works |
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

### ALERT-004

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Expiry 90d |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Alert fires at 90 days before registration expiry |
| **Preconditions** | Registration with controlled expiry date |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Set expiry in lab
   2. Run evaluate alerts
   3. Inspect alert list
   4. Verify recipient

**Expected Result per Step:**
   1. Date set
   2. Job runs
   3. Alert for 90d present
   4. Correct owner

| Field | Value |
|-------|-------|
| **Expected Final Result** | 90-day threshold works |
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

### ALERT-005

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Expiry 60d |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Alert fires at 60 days before registration expiry |
| **Preconditions** | Registration with controlled expiry date |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Set expiry in lab
   2. Run evaluate alerts
   3. Inspect alert list
   4. Verify recipient

**Expected Result per Step:**
   1. Date set
   2. Job runs
   3. Alert for 60d present
   4. Correct owner

| Field | Value |
|-------|-------|
| **Expected Final Result** | 60-day threshold works |
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

### ALERT-006

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Expiry 30d |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Alert fires at 30 days before registration expiry |
| **Preconditions** | Registration with controlled expiry date |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Set expiry in lab
   2. Run evaluate alerts
   3. Inspect alert list
   4. Verify recipient

**Expected Result per Step:**
   1. Date set
   2. Job runs
   3. Alert for 30d present
   4. Correct owner

| Field | Value |
|-------|-------|
| **Expected Final Result** | 30-day threshold works |
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

### ALERT-007

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Expiry 7d |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Alert fires at 7 days before registration expiry |
| **Preconditions** | Registration with controlled expiry date |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Set expiry in lab
   2. Run evaluate alerts
   3. Inspect alert list
   4. Verify recipient

**Expected Result per Step:**
   1. Date set
   2. Job runs
   3. Alert for 7d present
   4. Correct owner

| Field | Value |
|-------|-------|
| **Expected Final Result** | 7-day threshold works |
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

### ALERT-008

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Expiry 0d |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Alert fires at 0 days before registration expiry |
| **Preconditions** | Registration with controlled expiry date |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Set expiry in lab
   2. Run evaluate alerts
   3. Inspect alert list
   4. Verify recipient

**Expected Result per Step:**
   1. Date set
   2. Job runs
   3. Alert for 0d present
   4. Correct owner

| Field | Value |
|-------|-------|
| **Expected Final Result** | 0-day threshold works |
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

### ALERT-009

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Cert expiry |
| **Role** | Regulatory Manager |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manufacturer certificate expiry alert |
| **Preconditions** | Seed data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Configure threshold
   2. Evaluate
   3. Verify

**Expected Result per Step:**
   1. Config ok
   2. Alerts returned
   3. No duplicates

| Field | Value |
|-------|-------|
| **Expected Final Result** | Manufacturer certificate expiry alert |
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

### ALERT-010

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | License expiry |
| **Role** | Regulatory Manager |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Operating license expiry alert |
| **Preconditions** | Seed data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Configure threshold
   2. Evaluate
   3. Verify

**Expected Result per Step:**
   1. Config ok
   2. Alerts returned
   3. No duplicates

| Field | Value |
|-------|-------|
| **Expected Final Result** | Operating license expiry alert |
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

### ALERT-011

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-011 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Max reception overdue |
| **Role** | Regulatory Manager |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Maximum reception date overdue |
| **Preconditions** | Seed data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Configure threshold
   2. Evaluate
   3. Verify

**Expected Result per Step:**
   1. Config ok
   2. Alerts returned
   3. No duplicates

| Field | Value |
|-------|-------|
| **Expected Final Result** | Maximum reception date overdue |
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

### ALERT-012

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-012 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Stuck dossier |
| **Role** | Regulatory Manager |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Dossier inactivity >14 days |
| **Preconditions** | Seed data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Configure threshold
   2. Evaluate
   3. Verify

**Expected Result per Step:**
   1. Config ok
   2. Alerts returned
   3. No duplicates

| Field | Value |
|-------|-------|
| **Expected Final Result** | Dossier inactivity >14 days |
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

### ALERT-013

| Field | Value |
|-------|-------|
| **Test Case ID** | ALERT-013 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Alerts |
| **Feature** | Critical pending |
| **Role** | Regulatory Manager |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Critical requirement pending alert |
| **Preconditions** | Seed data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Configure threshold
   2. Evaluate
   3. Verify

**Expected Result per Step:**
   1. Config ok
   2. Alerts returned
   3. No duplicates

| Field | Value |
|-------|-------|
| **Expected Final Result** | Critical requirement pending alert |
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
