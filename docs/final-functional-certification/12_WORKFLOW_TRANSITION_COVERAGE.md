# 12 — Workflow Transition Coverage

**Program:** Final REGUTRACK Replacement Certification  
**Domain:** Registration Dossier — 16 states  
**Date:** 2026-07-14  
**Status:** DESIGN

---

## 1. State machine (authoritative)

```
Draft → Planning | Cancelled
Planning → WaitingManufacturerDocuments | OnHold | Cancelled
WaitingManufacturerDocuments → DocumentsReceived | OnHold | Cancelled
DocumentsReceived → Assembling
Assembling → ReadyForSubmission | WaitingManufacturerDocuments
ReadyForSubmission → Submitted | Assembling
Submitted → UnderAuthorityReview
UnderAuthorityReview → Observed | Approved | Rejected
Observed → CorrectingObservation
CorrectingObservation → Resubmitted
Resubmitted → UnderAuthorityReview
Approved → Closed
Rejected → Closed
OnHold → Planning | WaitingManufacturerDocuments | Cancelled
```

---

## 2. Allowed transitions — test coverage

| Transition | Gate | Role | Test Case | Status |
|------------|------|------|-----------|--------|
| Draft → Planning | Allowed | Specialist/Manager | WORKFLOW-001 | NOT EXECUTED |
| Draft → Cancelled | Allowed | Specialist/Manager | WORKFLOW-002 | NOT EXECUTED |
| Planning → WaitingManufacturerDocuments | Allowed | Specialist/Manager | WORKFLOW-003 | NOT EXECUTED |
| Planning → OnHold | Allowed | Specialist/Manager | WORKFLOW-004 | NOT EXECUTED |
| Planning → Cancelled | Allowed | Specialist/Manager | WORKFLOW-005 | NOT EXECUTED |
| WaitingManufacturerDocuments → DocumentsReceived | Allowed | Specialist/Manager | WORKFLOW-006 | NOT EXECUTED |
| WaitingManufacturerDocuments → OnHold | Allowed | Specialist/Manager | WORKFLOW-007 | NOT EXECUTED |
| WaitingManufacturerDocuments → Cancelled | Allowed | Specialist/Manager | WORKFLOW-008 | NOT EXECUTED |
| DocumentsReceived → Assembling | Allowed | Specialist/Manager | WORKFLOW-009 | NOT EXECUTED |
| Assembling → ReadyForSubmission | Allowed | Specialist/Manager | WORKFLOW-010 | NOT EXECUTED |
| Assembling → WaitingManufacturerDocuments | Allowed | Specialist/Manager | WORKFLOW-011 | NOT EXECUTED |
| ReadyForSubmission → Submitted | Allowed | Specialist/Manager | WORKFLOW-012 | NOT EXECUTED |
| ReadyForSubmission → Assembling | Allowed | Specialist/Manager | WORKFLOW-013 | NOT EXECUTED |
| Submitted → UnderAuthorityReview | Allowed | Specialist/Manager | WORKFLOW-014 | NOT EXECUTED |
| UnderAuthorityReview → Observed | Allowed | Specialist/Manager | WORKFLOW-015 | NOT EXECUTED |
| UnderAuthorityReview → Approved | Allowed | Specialist/Manager | WORKFLOW-016 | NOT EXECUTED |
| UnderAuthorityReview → Rejected | Allowed | Specialist/Manager | WORKFLOW-017 | NOT EXECUTED |
| Observed → CorrectingObservation | Allowed | Specialist/Manager | WORKFLOW-018 | NOT EXECUTED |
| CorrectingObservation → Resubmitted | Allowed | Specialist/Manager | WORKFLOW-019 | NOT EXECUTED |
| Resubmitted → UnderAuthorityReview | Allowed | Specialist/Manager | WORKFLOW-020 | NOT EXECUTED |
| Approved → Closed | Allowed | Specialist/Manager | WORKFLOW-021 | NOT EXECUTED |
| Rejected → Closed | Allowed | Specialist/Manager | WORKFLOW-022 | NOT EXECUTED |
| OnHold → Planning | Allowed | Specialist/Manager | WORKFLOW-023 | NOT EXECUTED |
| OnHold → WaitingManufacturerDocuments | Allowed | Specialist/Manager | WORKFLOW-024 | NOT EXECUTED |
| OnHold → Cancelled | Allowed | Specialist/Manager | WORKFLOW-025 | NOT EXECUTED |

---

## 3. Illegal transitions — negative coverage

| Transition | Expected | Role | Test Case | Status |
|------------|----------|------|-----------|--------|
| Planning → Approved | Denied | Any | WORKFLOW-NEG-001 | NOT EXECUTED |
| Draft → Submitted | Denied | Any | WORKFLOW-NEG-002 | NOT EXECUTED |
| Assembling → Submitted | Denied | Any | WORKFLOW-NEG-003 | NOT EXECUTED |
| Approved → Submitted | Denied | Any | WORKFLOW-NEG-004 | NOT EXECUTED |

---

## 4. Business gates coupled to workflow

| Gate | Rule | Test Cases |
|------|------|------------|
| Submit | All critical requirements Accepted | SUB-002, REQ-022 |
| DocumentsReceived | Waiver ≥8 chars if no evidence | REQ-015 |
| Internal Approval | Reviewer complete + Approver SoD | IAP-001, SOD-008 |
| External Approve | Only from UnderAuthorityReview | REG-003 |
| Specialist | Cannot approve external CT/RS | SOD-003 |

---

## 5. SoD multi-role journey mapping

The full journey in `35_FULL_BUSINESS_JOURNEY_TEST_CASES.md` must traverse all critical transitions with **distinct users** (no multi-role single user).

---

*Execution status for all transitions: NOT EXECUTED.*
