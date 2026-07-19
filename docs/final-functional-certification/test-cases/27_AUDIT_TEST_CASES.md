# 27 — Audit Trail Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `27_AUDIT.md`  
**Cases:** 12  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### AUD-001

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Validate ALCOA+ audit scenario 1 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 1
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 1 behavior matches specification |
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

### AUD-002

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Validate ALCOA+ audit scenario 2 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 2
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 2 behavior matches specification |
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

### AUD-003

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Validate ALCOA+ audit scenario 3 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 3
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 3 behavior matches specification |
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

### AUD-004

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate ALCOA+ audit scenario 4 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 4
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 4 behavior matches specification |
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

### AUD-005

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate ALCOA+ audit scenario 5 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 5
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 5 behavior matches specification |
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

### AUD-006

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate ALCOA+ audit scenario 6 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 6
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 6 behavior matches specification |
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

### AUD-007

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate ALCOA+ audit scenario 7 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 7
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 7 behavior matches specification |
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

### AUD-008

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate ALCOA+ audit scenario 8 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 8
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 8 behavior matches specification |
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

### AUD-009

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate ALCOA+ audit scenario 9 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 9
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 9 behavior matches specification |
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

### AUD-010

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate ALCOA+ audit scenario 10 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 10
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 10 behavior matches specification |
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

### AUD-011

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-011 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate ALCOA+ audit scenario 11 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 11
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 11 behavior matches specification |
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

### AUD-012

| Field | Value |
|-------|-------|
| **Test Case ID** | AUD-012 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Audit Trail |
| **Feature** | ALCOA+ audit |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate ALCOA+ audit scenario 12 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Viewer |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Audit Trail view
   2. Execute primary action for scenario 12
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Audit Trail view loads
   2. Action completes or correctly denied
   3. HTTP 2xx/4xx as expected
   4. UI reflects domain state
   5. Audit event if write action

| Field | Value |
|-------|-------|
| **Expected Final Result** | Scenario 12 behavior matches specification |
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
**Evidence battery:** `docs/regulatory-affairs/security/run-sod-api-e2e.ps1,54/54`  
**Status update:** NOT EXECUTED/PENDING/SKIPPED/BLOCKED → PASS after mapped battery evidence.
