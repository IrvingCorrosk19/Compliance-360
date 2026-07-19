# 16 — Audit and Notification Test Strategy

**Program:** Final REGUTRACK Replacement Certification  
**Date:** 2026-07-14

---

## 1. Audit strategy

### 1.1 Critical actions requiring audit proof

| Action | Expected Event Type | Test Case |
|--------|---------------------|-----------|
| Product created | Product.Created | PROD-001 |
| Dossier created | Dossier.Created | DOS-001 |
| Requirement status change | Requirement.Updated | REQ-001 |
| Internal approval | Dossier.InternalApproval | IAP-001 |
| Submission | Dossier.Submitted | SUB-001 |
| Observation opened | Observation.Created | OBS-001 |
| External approval / CT | Registration.Approved | REG-001 |
| Import committed | Import.Committed | IMP-010 |
| SoD denied | SoD.Denied | SOD-001 |

### 1.2 Audit quality rules

- Human-readable description (not only "Entity Updated")
- Actor user ID + role context
- Tenant ID scoped
- Timestamp UTC
- Before/after values for critical fields where supported

---

## 2. Notification strategy

### 2.1 Event → recipient matrix

| Trigger | Recipient | Channel | Test Case |
|---------|-----------|---------|-----------|
| Review requested | Regulatory Reviewer | In-app/Email | NOTIF-001 |
| Returned for correction | Regulatory Specialist | In-app/Email | NOTIF-002 |
| Internal approval pending | Regulatory Approver | In-app/Email | NOTIF-003 |
| Ready for submission | Regulatory Submitter | In-app/Email | NOTIF-004 |
| Authority observation | Dossier owner + Specialist | In-app/Email | NOTIF-005 |
| Registration expiring | Regulatory Manager | In-app/Email | NOTIF-006 |
| Certificate expiring | Regulatory Specialist | In-app/Email | NOTIF-007 |

### 2.2 Negative notification rules

- Viewer must NOT receive operational task assignments
- No broadcast to all users
- No cross-tenant recipient

---

## 3. Test catalogs

- `test-cases/27_AUDIT_TEST_CASES.md`
- `test-cases/23_NOTIFICATION_TEST_CASES.md`

---

*All audit/notification TC: NOT EXECUTED.*
