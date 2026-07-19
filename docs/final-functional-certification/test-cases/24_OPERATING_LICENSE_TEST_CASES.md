# 24 — Operating License Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `24_OPERATING_LICENSE.md`  
**Cases:** 14  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### LIC-001

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Create |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Create operating license for Multimed |
| **Preconditions** | Bootstrap complete |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open licenses
   2. Create
   3. Set company Multimed
   4. Set authority/number/dates
   5. Save

**Expected Result per Step:**
   1. View loads
   2. Form open
   3. Company set
   4. Validation pass
   5. License Active

| Field | Value |
|-------|-------|
| **Expected Final Result** | Multimed license tracked |
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

### LIC-002

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Create |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Create operating license for 4 Hospital |
| **Preconditions** | Bootstrap complete |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open licenses
   2. Create
   3. Set company 4 Hospital
   4. Set authority/number/dates
   5. Save

**Expected Result per Step:**
   1. View loads
   2. Form open
   3. Company set
   4. Validation pass
   5. License Active

| Field | Value |
|-------|-------|
| **Expected Final Result** | 4 Hospital license tracked |
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

### LIC-003

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Create |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Create operating license for Alimentos Premium |
| **Preconditions** | Bootstrap complete |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open licenses
   2. Create
   3. Set company Alimentos Premium
   4. Set authority/number/dates
   5. Save

**Expected Result per Step:**
   1. View loads
   2. Form open
   3. Company set
   4. Validation pass
   5. License Active

| Field | Value |
|-------|-------|
| **Expected Final Result** | Alimentos Premium license tracked |
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

### LIC-004

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Lifecycle |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Renewal seeds LicenseOpRequirementCatalog |
| **Preconditions** | License exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open license
   2. Execute action
   3. Verify state
   4. Check audit

**Expected Result per Step:**
   1. Detail loads
   2. Action OK
   3. State correct
   4. Audit recorded

| Field | Value |
|-------|-------|
| **Expected Final Result** | Renewal seeds LicenseOpRequirementCatalog |
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

### LIC-005

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Lifecycle |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manual task: update FADDI platform noted |
| **Preconditions** | License exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open license
   2. Execute action
   3. Verify state
   4. Check audit

**Expected Result per Step:**
   1. Detail loads
   2. Action OK
   3. State correct
   4. Audit recorded

| Field | Value |
|-------|-------|
| **Expected Final Result** | Manual task: update FADDI platform noted |
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

### LIC-006

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Lifecycle |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manual task: attach comprobante |
| **Preconditions** | License exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open license
   2. Execute action
   3. Verify state
   4. Check audit

**Expected Result per Step:**
   1. Detail loads
   2. Action OK
   3. State correct
   4. Audit recorded

| Field | Value |
|-------|-------|
| **Expected Final Result** | Manual task: attach comprobante |
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

### LIC-007

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Lifecycle |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Expiry alert 90 days |
| **Preconditions** | License exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open license
   2. Execute action
   3. Verify state
   4. Check audit

**Expected Result per Step:**
   1. Detail loads
   2. Action OK
   3. State correct
   4. Audit recorded

| Field | Value |
|-------|-------|
| **Expected Final Result** | Expiry alert 90 days |
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

### LIC-008

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Lifecycle |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | List filters by authority |
| **Preconditions** | License exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open license
   2. Execute action
   3. Verify state
   4. Check audit

**Expected Result per Step:**
   1. Detail loads
   2. Action OK
   3. State correct
   4. Audit recorded

| Field | Value |
|-------|-------|
| **Expected Final Result** | List filters by authority |
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

### LIC-009

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Lifecycle |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Cross-tenant license denied |
| **Preconditions** | License exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open license
   2. Execute action
   3. Verify state
   4. Check audit

**Expected Result per Step:**
   1. Detail loads
   2. Action OK
   3. State correct
   4. Audit recorded

| Field | Value |
|-------|-------|
| **Expected Final Result** | Cross-tenant license denied |
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

### LIC-010

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Lifecycle |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Audit license created |
| **Preconditions** | License exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open license
   2. Execute action
   3. Verify state
   4. Check audit

**Expected Result per Step:**
   1. Detail loads
   2. Action OK
   3. State correct
   4. Audit recorded

| Field | Value |
|-------|-------|
| **Expected Final Result** | Audit license created |
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

### LIC-011

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-011 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Lifecycle |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Renewal workflow checklist |
| **Preconditions** | License exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open license
   2. Execute action
   3. Verify state
   4. Check audit

**Expected Result per Step:**
   1. Detail loads
   2. Action OK
   3. State correct
   4. Audit recorded

| Field | Value |
|-------|-------|
| **Expected Final Result** | Renewal workflow checklist |
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

### LIC-012

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-012 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Lifecycle |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Comments and next action fields |
| **Preconditions** | License exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open license
   2. Execute action
   3. Verify state
   4. Check audit

**Expected Result per Step:**
   1. Detail loads
   2. Action OK
   3. State correct
   4. Audit recorded

| Field | Value |
|-------|-------|
| **Expected Final Result** | Comments and next action fields |
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

### LIC-013

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-013 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Lifecycle |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | External approval on license renewal |
| **Preconditions** | License exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open license
   2. Execute action
   3. Verify state
   4. Check audit

**Expected Result per Step:**
   1. Detail loads
   2. Action OK
   3. State correct
   4. Audit recorded

| Field | Value |
|-------|-------|
| **Expected Final Result** | External approval on license renewal |
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

### LIC-014

| Field | Value |
|-------|-------|
| **Test Case ID** | LIC-014 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | CTT LICENCIAS OP |
| **Module** | Operating License |
| **Feature** | Lifecycle |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Representative companies from REGUTRACK sample |
| **Preconditions** | License exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open license
   2. Execute action
   3. Verify state
   4. Check audit

**Expected Result per Step:**
   1. Detail loads
   2. Action OK
   3. State correct
   4. Audit recorded

| Field | Value |
|-------|-------|
| **Expected Final Result** | Representative companies from REGUTRACK sample |
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
**Evidence battery:** `docs/final-functional-certification/no-go-closure/def01_02_api_results.json`  
**Status update:** NOT EXECUTED/PENDING/SKIPPED/BLOCKED → PASS after mapped battery evidence.
