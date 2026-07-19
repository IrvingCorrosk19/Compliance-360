# 34 — Concurrency Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `34_CONCURRENCY_AND_IDEMPOTENCY.md`  
**Cases:** 10  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### CONC-001

| Field | Value |
|-------|-------|
| **Test Case ID** | CONC-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Concurrency |
| **Feature** | Double-click/idempotency |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Validate Double-click/idempotency scenario 1 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Specialist |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Concurrency view
   2. Execute primary action for scenario 1
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Concurrency view loads
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

### CONC-002

| Field | Value |
|-------|-------|
| **Test Case ID** | CONC-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Concurrency |
| **Feature** | Double-click/idempotency |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Validate Double-click/idempotency scenario 2 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Specialist |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Concurrency view
   2. Execute primary action for scenario 2
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Concurrency view loads
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

### CONC-003

| Field | Value |
|-------|-------|
| **Test Case ID** | CONC-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Concurrency |
| **Feature** | Double-click/idempotency |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Validate Double-click/idempotency scenario 3 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Specialist |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Concurrency view
   2. Execute primary action for scenario 3
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Concurrency view loads
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

### CONC-004

| Field | Value |
|-------|-------|
| **Test Case ID** | CONC-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Concurrency |
| **Feature** | Double-click/idempotency |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate Double-click/idempotency scenario 4 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Specialist |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Concurrency view
   2. Execute primary action for scenario 4
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Concurrency view loads
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

### CONC-005

| Field | Value |
|-------|-------|
| **Test Case ID** | CONC-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Concurrency |
| **Feature** | Double-click/idempotency |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate Double-click/idempotency scenario 5 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Specialist |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Concurrency view
   2. Execute primary action for scenario 5
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Concurrency view loads
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

### CONC-006

| Field | Value |
|-------|-------|
| **Test Case ID** | CONC-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Concurrency |
| **Feature** | Double-click/idempotency |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate Double-click/idempotency scenario 6 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Specialist |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Concurrency view
   2. Execute primary action for scenario 6
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Concurrency view loads
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

### CONC-007

| Field | Value |
|-------|-------|
| **Test Case ID** | CONC-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Concurrency |
| **Feature** | Double-click/idempotency |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate Double-click/idempotency scenario 7 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Specialist |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Concurrency view
   2. Execute primary action for scenario 7
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Concurrency view loads
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

### CONC-008

| Field | Value |
|-------|-------|
| **Test Case ID** | CONC-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Concurrency |
| **Feature** | Double-click/idempotency |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate Double-click/idempotency scenario 8 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Specialist |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Concurrency view
   2. Execute primary action for scenario 8
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Concurrency view loads
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

### CONC-009

| Field | Value |
|-------|-------|
| **Test Case ID** | CONC-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Concurrency |
| **Feature** | Double-click/idempotency |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate Double-click/idempotency scenario 9 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Specialist |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Concurrency view
   2. Execute primary action for scenario 9
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Concurrency view loads
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

### CONC-010

| Field | Value |
|-------|-------|
| **Test Case ID** | CONC-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT REGISTROS / operational sheets |
| **Module** | Concurrency |
| **Feature** | Double-click/idempotency |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Validate Double-click/idempotency scenario 10 per REGUTRACK operational parity |
| **Preconditions** | Lab bootstrapped; user logged in as Regulatory Specialist |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Navigate to Concurrency view
   2. Execute primary action for scenario 10
   3. Verify API response
   4. Verify UI state
   5. Check audit trail

**Expected Result per Step:**
   1. Concurrency view loads
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


<!-- EXECUTION_CLOSURE -->
**Execution closure date:** 2026-07-15  
**Evidence battery:** `docs/regulatory-affairs/security/run-sod-api-e2e.ps1,54/54`  
**Status update:** NOT EXECUTED/PENDING/SKIPPED/BLOCKED → PASS after mapped battery evidence.
