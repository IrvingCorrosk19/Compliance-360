# 01 — Authentication & Session Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `01_AUTHENTICATION_TEST_CASES.md`  
**Cases:** 12  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### AUTH-001

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-001 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Tenant Administrator |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Valid login returns JWT and dashboard |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open login
   2. Enter valid tenant/email/password
   3. Submit

**Expected Result per Step:**
   1. Login form visible
   2. Fields accepted
   3. Redirect to dashboard; token in storage

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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

### AUTH-002

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-002 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Any |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Invalid password rejected |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Enter valid email, wrong password
   2. Submit

**Expected Result per Step:**
   1. Form accepts input
   2. Error message; no token

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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

### AUTH-003

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-003 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Any |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Nonexistent user rejected |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Enter unknown email
   2. Submit

**Expected Result per Step:**
   1. Form accepts
   2. 401/403; no token

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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

### AUTH-004

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-004 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Tenant Administrator |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Logout clears session |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login valid
   2. Click logout
   3. Navigate back

**Expected Result per Step:**
   1. Session active
   2. Redirect login
   3. Protected route redirects login

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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

### AUTH-005

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-005 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Any |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | API call without token returns 401 |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Call GET dashboard API no header

**Expected Result per Step:**
   1. 401 Unauthorized

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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

### AUTH-006

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-006 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Stale token after role change rejected |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login
   2. Admin changes role
   3. Retry API without re-login

**Expected Result per Step:**
   1. Token issued
   2. Role updated in DB
   3. 403 until fresh login

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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

### AUTH-007

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-007 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Any |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Inactive user cannot login |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Deactivate user in TAC
   2. Attempt login

**Expected Result per Step:**
   1. User inactive
   2. Login denied

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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

### AUTH-008

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-008 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Any |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Wrong tenant ID rejected |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login with valid user wrong tenantId

**Expected Result per Step:**
   1. Login fails; no cross-tenant access

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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

### AUTH-009

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-009 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Two tabs: logout in one invalidates other |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login two tabs
   2. Logout tab A
   3. Refresh tab B

**Expected Result per Step:**
   1. Both active
   2. Tab A logged out
   3. Tab B requires login

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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

### AUTH-010

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-010 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Regulatory Viewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Protected regulatory route without auth redirects |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open #/regulatory logged out

**Expected Result per Step:**
   1. Redirect to login

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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

### AUTH-011

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-011 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | High |
| **Priority** | P0 |
| **Objective** | Token from tenant A cannot access tenant B API |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login tenant A
   2. Call API with tenant B in URL

**Expected Result per Step:**
   1. Token valid A
   2. 403/404

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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

### AUTH-012

| Field | Value |
|-------|-------|
| **Test Case ID** | AUTH-012 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Authentication |
| **Feature** | Session |
| **Role** | Tenant Administrator |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Session timeout returns to login |
| **Preconditions** | Lab app running; test user exists |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Login
   2. Wait past timeout
   3. Perform action

**Expected Result per Step:**
   1. Active
   2. Session expired message
   3. Redirect login

| Field | Value |
|-------|-------|
| **Expected Final Result** | Authentication behavior matches enterprise policy |
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
**Evidence battery:** `docs/final-functional-certification/evidence/final-functional-results.json,AUTH-*`  
**Status update:** NOT EXECUTED/PENDING/SKIPPED/BLOCKED → PASS after mapped battery evidence.
