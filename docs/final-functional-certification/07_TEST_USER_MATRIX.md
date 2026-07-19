# 07 — Test User Matrix

**Tenant:** `82af3877-2786-4d39-bce8-c981101c771d`  
**Source of truth (security):** `docs/regulatory-affairs/security/17_ROLE_TEST_USER_MATRIX.md`  
**Password policy:** Managed offline — no passwords stored in repository  
**Date:** 2026-07-14  
**Functional execution status:** NOT EXECUTED

---

## 1. Purpose

Extend the security certification user matrix with **functional test execution** columns: scenarios assigned, last login verification, and evidence links. All users must be active before functional certification starts.

---

## 2. Lab users (@cert.local)

| User ID | Email | Role | JWT purpose | Security status | Functional scenarios | Login verified | Evidence |
|---------|-------|------|-------------|-----------------|---------------------|----------------|----------|
| U-RA-ADM | ra.admin@cert.local | Regulatory Administrator | Config, import, bootstrap, SoD read | ACTIVO (doc 17) | ENV-003, RB-007, RA-IMP-*, RA-CFG-* | NOT EXECUTED | — |
| U-RA-MGR | ra.mgr@cert.local | Regulatory Manager | External approve, observations, license manage | ACTIVO | RB-005, RB-009, RA-WF-001 steps 7–10, RA-LIC-* | NOT EXECUTED | — |
| U-RA-SPEC | ra.spec@cert.local | Regulatory Specialist | Prep, transitions, requirements received | ACTIVO | RB-001, RA-WF-001 steps 1–3, RA-PORT-*, RA-DOS-* | NOT EXECUTED | — |
| U-RA-REV | ra.rev@cert.local | Regulatory Reviewer | Requirement accept/reject | ACTIVO | RB-002, RA-WF-001 step 4 | NOT EXECUTED | — |
| U-RA-APPR | ra.appr@cert.local | Regulatory Approver | Internal approve for submission | ACTIVO | RB-003, RA-WF-001 step 5, SoD negative N1 | NOT EXECUTED | — |
| U-RA-SUB | ra.sub@cert.local | Regulatory Submitter | Submit / resubmit | ACTIVO | RB-004, RA-WF-001 step 6, SoD negative N2 | NOT EXECUTED | — |
| U-RA-VIEW | ra.view@cert.local | Regulatory Viewer | Read-only all permitted views | ACTIVO | RB-006, RA-SCR read-only sweep | NOT EXECUTED | — |
| U-RA-QM | ra.qm@cert.local | Quality Manager | CT/RS transversal QMS perspective | ACTIVO | RB-009, external approve variant | NOT EXECUTED | — |
| U-TAC | irvingcorrosk19@gmail.com | Tenant Administrator | IAM / seed; no RA operate | ACTIVO | RB-010 boundary only | NOT EXECUTED | — |

---

## 3. Excluded roles (v1)

| Role | Reason | Functional status |
|------|--------|-------------------|
| Sales Viewer | Not implemented | N/A — no test user |
| Document Contributor | Not implemented | N/A — no test user |

Per `24_FINAL_ROLE_AND_SOD_CERTIFICATION.md`: consciously excluded; not marked PASS.

---

## 4. Session rules

1. **Logout** between role switches (`localStorage` + `sessionStorage` clear).
2. **Fresh login** emits new JWT with current permission grants.
3. **Never** reuse token across users in same browser profile without logout.
4. Document login timestamp in evidence filename.
5. Playwright: separate storage state file per user optional (`evidence/functional/auth/ra.spec.json`).

---

## 5. Permission verification (quick)

After login as each user, run in browser console:

```javascript
// Expect true/false per role — sample for Specialist
window.hasPermission?.("REGULATORY.DOSSIER.CREATE")
```

Or decode JWT `permission` claim and compare to `05_ATOMIC_PERMISSION_MATRIX.md`.

**Test ID:** USER-PERM-{role}  
**Status:** NOT EXECUTED (functional pass; SoD script already validated in doc 18/19)

---

## 6. Role → workflow step mapping

| Workflow step | Primary user | Alternate |
|---------------|--------------|-----------|
| Prep / Planning | U-RA-SPEC | — |
| Mark requirements received | U-RA-SPEC | — |
| Technical review | U-RA-REV | — |
| Internal approval | U-RA-APPR | — |
| Submit | U-RA-SUB | — |
| Authority observation | U-RA-MGR | — |
| Observation response | U-RA-SPEC | — |
| Resubmit | U-RA-SUB | — |
| External CT/RS approve | U-RA-MGR | U-RA-QM |
| Import / bootstrap | U-RA-ADM | — |
| Read-only audit | U-RA-VIEW | — |

---

## 7. Multi-user orchestration (E2E)

Recommended order for SC-WF-001:

```
SPEC → REV → APPR → SUB → MGR → SPEC → SUB → MGR
```

Each handoff: note dossier UUID on shared test-data-registry.json.

---

## 8. Defect attribution

When logging defects, always record:

- User ID (e.g. U-RA-SUB)
- Email
- Role name
- JWT iat/exp if token-related

---

## 9. User provisioning checklist

| Step | Owner | Status |
|------|-------|--------|
| Users exist in lab tenant | TAC | DONE (doc 17) |
| Roles assigned per matrix | TAC | DONE |
| Passwords distributed offline | QA | Assumed |
| Functional login test | QA | NOT EXECUTED |
| Nav visibility per user | QA | NOT EXECUTED |

---

## 10. Document control

| Version | Date | Change |
|---------|------|--------|
| 1.0 | 2026-07-14 | Functional columns added to security matrix 17 |

**Cross-reference:** `04_ROLE_BASED_TEST_STRATEGY.md`, `05_TEST_ENVIRONMENT_AND_LAB.md`
