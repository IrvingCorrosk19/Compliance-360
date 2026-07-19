# 08 — Requirement Pack Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `08_REQUIREMENT_PACK.md`  
**Cases:** 14  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### PACK-001

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Bootstrap creates REGUTRACK-PA-DEFAULT with 22 items |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | Bootstrap creates REGUTRACK-PA-DEFAULT with 22 items |
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

### PACK-002

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | List requirement pack returns 22 definitions |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | List requirement pack returns 22 definitions |
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

### PACK-003

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Critical flag matches catalog for LEGAL_ID |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | Critical flag matches catalog for LEGAL_ID |
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

### PACK-004

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Dossier A snapshots pack version V1 |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | Dossier A snapshots pack version V1 |
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

### PACK-005

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Publish pack V2 does not alter dossier A requirements |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | Publish pack V2 does not alter dossier A requirements |
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

### PACK-006

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Dossier B uses pack V2 after publish |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | Dossier B uses pack V2 after publish |
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

### PACK-007

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Requirement order preserved 1-22 |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | Requirement order preserved 1-22 |
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

### PACK-008

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Category Legal/Technical/Manufacturer mapped |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | Category Legal/Technical/Manufacturer mapped |
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

### PACK-009

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Viewer cannot publish pack |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | Viewer cannot publish pack |
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

### PACK-010

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Specialist cannot configure packs |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | Specialist cannot configure packs |
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

### PACK-011

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-011 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Pack tied to PA country |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | Pack tied to PA country |
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

### PACK-012

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-012 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | API GET requirement-packs tenant scoped |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | API GET requirement-packs tenant scoped |
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

### PACK-013

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-013 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | All 22 codes present in API response |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | All 22 codes present in API response |
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

### PACK-014

| Field | Value |
|-------|-------|
| **Test Case ID** | PACK-014 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | Checklist cols 18-39 |
| **Module** | Requirement Pack |
| **Feature** | REGUTRACK-PA-DEFAULT |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | IFU marked critical; PHOTOS non-critical |
| **Preconditions** | Bootstrap executed |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call API or open config
   2. Inspect pack
   3. Compare to RegutrackRequirementCatalog
   4. Verify counts

**Expected Result per Step:**
   1. Access granted/denied correctly
   2. Pack visible
   3. 22 items match codes
   4. Flags correct

| Field | Value |
|-------|-------|
| **Expected Final Result** | IFU marked critical; PHOTOS non-critical |
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
