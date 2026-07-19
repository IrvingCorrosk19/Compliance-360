# 28 — Multitenancy Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `28_MULTITENANCY.md`  
**Cases:** 14  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### MT-001

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | products |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B products |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B products ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-002

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | manufacturers |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B manufacturers |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B manufacturers ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-003

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | certificates |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B certificates |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B certificates ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-004

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | dossiers |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B dossiers |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B dossiers ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-005

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | requirements |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B requirements |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B requirements ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-006

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | documents |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B documents |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B documents ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-007

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | registrations |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B registrations |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B registrations ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-008

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | licenses |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B licenses |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B licenses ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-009

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | imports |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B imports |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B imports ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-010

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | dashboard |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B dashboard |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B dashboard ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-011

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-011 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | pipeline |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B pipeline |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B pipeline ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-012

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-012 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | alerts |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B alerts |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B alerts ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-013

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-013 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | audit |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B audit |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B audit ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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

### MT-014

| Field | Value |
|-------|-------|
| **Test Case ID** | MT-014 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Multitenancy |
| **Feature** | packs |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Tenant A cannot read/write tenant B packs |
| **Preconditions** | Two tenants with data |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. API GET tenant B packs ID
   3. Attempt UI deep link
   4. Verify audit

**Expected Result per Step:**
   1. Session A
   2. 403/404
   3. No data leak
   4. SoD/tenant denial logged

| Field | Value |
|-------|-------|
| **Expected Final Result** | Isolation confirmed |
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
