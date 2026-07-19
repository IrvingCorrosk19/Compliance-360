# 35 — Full Business Journey (SoD-valid) Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `35_FULL_BUSINESS_JOURNEY.md`  
**Cases:** 20  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### JOURNEY-001

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Specialist: login clean, create product |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Specialist only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Specialist authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Specialist: login clean, create product |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-002

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Specialist: create dossier + 22 requirements applied |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Specialist only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Specialist authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Specialist: create dossier + 22 requirements applied |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-003

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Specialist: transition Draft→Planning→WaitingMfrDocs |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Specialist only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Specialist authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Specialist: transition Draft→Planning→WaitingMfrDocs |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-004

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Specialist: upload evidence, mark requirements Received |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Specialist only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Specialist authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Specialist: upload evidence, mark requirements Received |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-005

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Specialist: request technical review, logout |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Specialist only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Specialist authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Specialist: request technical review, logout |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-006

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Reviewer |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Reviewer: login clean, reject 3 requirements with notes |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Reviewer only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Reviewer authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Reviewer: login clean, reject 3 requirements with notes |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-007

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Reviewer |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Reviewer: return dossier to Specialist, logout |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Reviewer only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Reviewer authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Reviewer: return dossier to Specialist, logout |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-008

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Specialist: correct docs, re-request review, logout |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Specialist only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Specialist authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Specialist: correct docs, re-request review, logout |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-009

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Reviewer |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Reviewer: accept all critical requirements, logout |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Reviewer only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Reviewer authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Reviewer: accept all critical requirements, logout |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-010

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Approver |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Approver: internal approval for submission, logout |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Approver only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Approver authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Approver: internal approval for submission, logout |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-011

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-011 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Submitter |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Submitter: register submission date+reference, logout |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Submitter only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Submitter authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Submitter: register submission date+reference, logout |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-012

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-012 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Manager |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manager: record authority observation round 1, logout |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Manager only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Manager authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Manager: record authority observation round 1, logout |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-013

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-013 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Specialist: prepare observation response, logout |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Specialist only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Specialist authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Specialist: prepare observation response, logout |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-014

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-014 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Reviewer |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Reviewer: review response, logout |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Reviewer only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Reviewer authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Reviewer: review response, logout |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-015

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-015 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Approver |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Approver: authorize resubmission, logout |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Approver only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Approver authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Approver: authorize resubmission, logout |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-016

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-016 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Submitter |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Submitter: register resubmission round 1, logout |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Submitter only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Submitter authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Submitter: register resubmission round 1, logout |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-017

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-017 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Manager |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manager: observation round 2 + response cycle |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Manager only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Manager authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Manager: observation round 2 + response cycle |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-018

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-018 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Approver |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Approver: external approve CT/RS MINSA/CSS |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Approver only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Approver authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Approver: external approve CT/RS MINSA/CSS |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-019

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-019 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Viewer: verify registration active + days remaining |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Viewer only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Viewer authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | Viewer: verify registration active + days remaining |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### JOURNEY-020

| Field | Value |
|-------|-------|
| **Test Case ID** | JOURNEY-020 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Full REGUTRACK replacement path |
| **Module** | Full Business Journey |
| **Feature** | SoD multi-role |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | End-to-end: dashboard/pipeline/portfolio/audit reconcile |
| **Preconditions** | Distinct browser profile per role; prior journey steps completed |
| **Test Data** | Dedicated journey product JOURNEY-{timestamp} |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Logout any prior session
   2. Login as Regulatory Viewer only
   3. Execute step action
   4. Verify state transition
   5. Capture evidence
   6. Logout

**Expected Result per Step:**
   1. Clean session
   2. Regulatory Viewer authenticated
   3. Action succeeds or correctly blocked
   4. Workflow state correct
   5. Screenshot+API saved
   6. Session cleared

| Field | Value |
|-------|-------|
| **Expected Final Result** | End-to-end: dashboard/pipeline/portfolio/audit reconcile |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Full timeline from DossierCreated to Registration.Approved |
| **Expected Notification** | Role-appropriate notification delivered |
| **Negative Variant** | Same user attempting next role step must FAIL SoD |
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
