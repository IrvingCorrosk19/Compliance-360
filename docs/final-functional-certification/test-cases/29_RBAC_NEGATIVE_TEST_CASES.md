# 29 — RBAC Negative Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `29_RBAC_NEGATIVE.md`  
**Cases:** 14  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### RBAC-001

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Regulatory Viewer denied for POST product |
| **Preconditions** | User Regulatory Viewer logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call POST product
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-002

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Regulatory Viewer denied for POST dossier |
| **Preconditions** | User Regulatory Viewer logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call POST dossier
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-003

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Regulatory Reviewer |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Regulatory Reviewer denied for POST product |
| **Preconditions** | User Regulatory Reviewer logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call POST product
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-004

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Regulatory Specialist denied for POST approve |
| **Preconditions** | User Regulatory Specialist logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call POST approve
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-005

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Regulatory Specialist denied for POST import |
| **Preconditions** | User Regulatory Specialist logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call POST import
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-006

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Regulatory Submitter |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Regulatory Submitter denied for POST internal approval |
| **Preconditions** | User Regulatory Submitter logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call POST internal approval
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-007

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Regulatory Approver |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Regulatory Approver denied for POST submit |
| **Preconditions** | User Regulatory Approver logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call POST submit
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-008

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Document Controller |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P1 |
| **Objective** | Document Controller denied for GET regulatory dossiers |
| **Preconditions** | User Document Controller logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call GET regulatory dossiers
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-009

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | CAPA Manager |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P1 |
| **Objective** | CAPA Manager denied for GET regulatory products |
| **Preconditions** | User CAPA Manager logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call GET regulatory products
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-010

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Platform Administrator |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Platform Administrator denied for GET tenant dossiers |
| **Preconditions** | User Platform Administrator logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call GET tenant dossiers
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-011

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-011 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Regulatory Specialist denied for PUT pack publish |
| **Preconditions** | User Regulatory Specialist logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call PUT pack publish
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-012

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-012 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Regulatory Reviewer |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Regulatory Reviewer denied for POST bootstrap |
| **Preconditions** | User Regulatory Reviewer logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call POST bootstrap
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-013

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-013 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Unauthenticated |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Unauthenticated denied for GET dashboard |
| **Preconditions** | User Unauthenticated logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call GET dashboard
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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

### RBAC-014

| Field | Value |
|-------|-------|
| **Test Case ID** | RBAC-014 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | RBAC Negative |
| **Feature** | Permission denial |
| **Role** | Regulatory Manager |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Regulatory Manager denied for DELETE tenant |
| **Preconditions** | User Regulatory Manager logged in |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Obtain token
   2. Call DELETE tenant
   3. Check UI button hidden
   4. Verify no DB change

**Expected Result per Step:**
   1. Token valid
   2. 403 Forbidden
   3. UI consistent
   4. State unchanged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Access denied correctly |
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
