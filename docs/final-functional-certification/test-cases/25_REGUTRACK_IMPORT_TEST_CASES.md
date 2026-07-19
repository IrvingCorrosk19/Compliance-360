# 25 — REGUTRACK Import Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `25_REGUTRACK_IMPORT.md`  
**Cases:** 16  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### IMP-001

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Stage JSON valid simulated rows |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Stage JSON valid simulated rows |
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

### IMP-002

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Stage XLSX copy of REGUTRACK 02JUN26 MG.xlsx |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Stage XLSX copy of REGUTRACK 02JUN26 MG.xlsx |
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

### IMP-003

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Detect sheets CTT REGISTROS TUBERIA DOCUMENTACION LICENCIAS |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Detect sheets CTT REGISTROS TUBERIA DOCUMENTACION LICENCIAS |
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

### IMP-004

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Preview first 10 rows mapping |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Preview first 10 rows mapping |
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

### IMP-005

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Validation error missing regulatoryName |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Validation error missing regulatoryName |
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

### IMP-006

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Duplicate catalog code warning |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Duplicate catalog code warning |
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

### IMP-007

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Simulate without commit |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Simulate without commit |
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

### IMP-008

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Commit maxRows=50 smoke |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Commit maxRows=50 smoke |
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

### IMP-009

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Reconciliation row to product |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Reconciliation row to product |
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

### IMP-010

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Reconciliation row to dossier |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Reconciliation row to dossier |
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

### IMP-011

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-011 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Reconciliation manufacturer from DOCUMENTACION |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Reconciliation manufacturer from DOCUMENTACION |
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

### IMP-012

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-012 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Reconciliation license from LICENCIAS OP |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Reconciliation license from LICENCIAS OP |
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

### IMP-013

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-013 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Repeat import idempotency |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Repeat import idempotency |
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

### IMP-014

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-014 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Corrupt file rejected |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Corrupt file rejected |
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

### IMP-015

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-015 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Missing sheet error report |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Missing sheet error report |
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

### IMP-016

| Field | Value |
|-------|-------|
| **Test Case ID** | IMP-016 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | REGUTRACK 02JUN26 MG.xlsx |
| **Module** | REGUTRACK Import |
| **Feature** | Migration |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Cancel job before commit |
| **Preconditions** | Excel copy in lab; import API up |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Upload file
   2. Review job
   3. Validate mapping
   4. Commit or abort
   5. Export reconciliation

**Expected Result per Step:**
   1. Job created
   2. Sheets detected
   3. Errors/warnings listed
   4. Entities match or error clear
   5. Report stored

| Field | Value |
|-------|-------|
| **Expected Final Result** | Cancel job before commit |
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
**Evidence battery:** `docs/final-functional-certification/no-go-closure/def03_results.json,02_REGUTRACK_FULL_ROW_RECONCILIATION.csv`  
**Status update:** NOT EXECUTED/PENDING/SKIPPED/BLOCKED → PASS after mapped battery evidence.
