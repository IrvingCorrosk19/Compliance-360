# 04 — Role-Based Test Strategy

**Prerequisite:** Role/SoD certification GO (`docs/regulatory-affairs/security/13`, `24`)  
**Users:** `07_TEST_USER_MATRIX.md`  
**Permissions:** `docs/regulatory-affairs/security/05_ATOMIC_PERMISSION_MATRIX.md`  
**Date:** 2026-07-14  
**Execution status:** Strategy defined; tests NOT EXECUTED

---

## 1. Purpose

Define how functional certification validates **role-appropriate behavior** in RA Console and API, building on the completed SoD program. Functional testing confirms that permissions translate to correct UI visibility, API HTTP semantics, and workflow participation—not re-proving the entire SoD matrix unless regressions are suspected.

---

## 2. Principles

1. **One person, one role per session** — Fresh login; no shared JWT across roles.
2. **Positive and negative pairs** — Every mutating action has an allowed role test and a denied role test.
3. **Separation of duties in workflow** — Prep (Specialist) ≠ Review (Reviewer) ≠ Internal Approve (Approver) ≠ Submit (Submitter) ≠ External decision (Manager).
4. **Viewer is read-only everywhere** — No create, transition, approve, or import.
5. **Admin configures, does not operate** — U-RA-ADM: bootstrap, import, SoD read; not primary dossier operator.
6. **Tenant Admin excluded from RA operate** — U-TAC validates module access boundaries only.

---

## 3. Role catalog (lab)

| ID | Email | Role | Functional persona |
|----|-------|------|-------------------|
| U-RA-ADM | ra.admin@cert.local | Regulatory Administrator | Config, import, SoD view |
| U-RA-MGR | ra.mgr@cert.local | Regulatory Manager | External approval, observations, overrides |
| U-RA-SPEC | ra.spec@cert.local | Regulatory Specialist | Prepare dossier, requirements, transitions (prep) |
| U-RA-REV | ra.rev@cert.local | Regulatory Reviewer | Requirement accept/reject |
| U-RA-APPR | ra.appr@cert.local | Regulatory Approver | Internal approve for submission |
| U-RA-SUB | ra.sub@cert.local | Regulatory Submitter | Submit / resubmit |
| U-RA-VIEW | ra.view@cert.local | Regulatory Viewer | Read all permitted views |
| U-RA-QM | ra.qm@cert.local | Quality Manager | CT/RS transversal read + external approve where granted |
| U-TAC | irvingcorrosk19@gmail.com | Tenant Administrator | IAM; must NOT operate RA workflow |

Excluded from PASS (v1): Sales Viewer, Document Contributor.

---

## 4. Test dimensions per role

For each role R, execute:

| Dimension | Method | Pass criteria |
|-----------|--------|---------------|
| D1 Nav visibility | Open RA Console | Only `visibleViews()` for R |
| D2 Read API | GET endpoints | 200 for allowed reads |
| D3 Write API | POST/PUT/DELETE | 200/201 allowed; 403 denied |
| D4 UI actions | Button presence | Matches permission matrix |
| D5 Workflow step | Golden path participation | R can perform assigned step only |
| D6 Audit | Audit log entry | Actor = R's user id |

---

## 5. Role × capability matrix (summary)

Legend: ✅ allowed · ❌ denied · 👁 read-only · ⚙ config

| Capability | SPEC | REV | APPR | SUB | MGR | ADM | VIEW | QM |
|------------|------|-----|------|-----|-----|-----|------|-----|
| View dashboard/portfolio/pipeline | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 👁 | 👁 |
| Create product + dossier | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Transition prep states | ✅ | ❌ | ❌ | ❌ | ⚠️ | ❌ | ❌ | ❌ |
| Mark requirement received | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Accept/reject requirement | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Approve for submission | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Submit | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Register observation | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| External approve + CT/RS | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ | ✅ |
| Manufacturer manage | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| License manage | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ | ❌ | ❌ |
| Import XLSX | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| Bootstrap | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| SoD settings read | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ | ❌ | ❌ |

⚠️ Manager override transitions: test only if exposed; default deny for prep transitions.

---

## 6. Scenario catalog

### 6.1 RB-001 — Specialist happy path (prep only)

**Actor:** U-RA-SPEC  
**Steps:** Create product → dossier → advance to ReadyForSubmission → mark requirements  
**Assert:** No approve/submit buttons visible.

### 6.2 RB-002 — Reviewer gate

**Actor:** U-RA-REV  
**Precondition:** Dossier ReadyForSubmission with pending requirements  
**Assert:** Accept/Reject visible; cannot submit.

### 6.3 RB-003 — Approver internal gate

**Actor:** U-RA-APPR  
**Assert:** Approve for submission works; submit button absent.

### 6.4 RB-004 — Submitter gate

**Actor:** U-RA-SUB  
**Precondition:** ApprovedForSubmission, checklist complete  
**Assert:** Submit succeeds; cannot external approve.

### 6.5 RB-005 — Manager external decision

**Actor:** U-RA-MGR  
**Assert:** Observation + external approve; cannot create dossier.

### 6.6 RB-006 — Viewer read-only

**Actor:** U-RA-VIEW  
**Assert:** All lists load; zero primary action buttons.

### 6.7 RB-007 — Admin import/config

**Actor:** U-RA-ADM  
**Assert:** Import + bootstrap; dossier create denied.

### 6.8 RB-008 — Cross-role SoD regression (sample)

Re-run 5 API SoD cases from doc 19 on dossier under functional test data.

### 6.9 RB-009 — QM external approve

**Actor:** U-RA-QM  
**Assert:** Can register CT/RS where granted; cannot prep dossier.

### 6.10 RB-010 — Tenant admin boundary

**Actor:** U-TAC  
**Assert:** No REGULATORY.* operate permissions unless explicitly granted.

---

## 7. Evidence requirements

Per scenario:

- Screenshot of RA Console with user email visible in shell
- Network capture or saved API request/response (status code + body snippet)
- Dossier id and case number in filename: `evidence/functional/roles/RB-00X_{caseNumber}_{role}.png`

---

## 8. Relationship to SoD certification

| SoD program | Functional program |
|-------------|-------------------|
| 54/54 API E2E PASS | Spot-check RB-008 on functional data |
| 12/12 unit PASS | Run on each build |
| Browser 1/1 PASS | Expand to full 10 matrix per role |
| Critical/High = 0 | Must remain 0 after functional defects fixed |

Do **not** re-execute full SoD suite unless a functional change touches authorization middleware or workflow gates.

---

## 9. Entry / exit criteria

**Entry:** Users active in lab tenant; JWT fresh login procedure documented.  
**Exit:** RB-001 through RB-010 PASS; all role×action cells in `10` verified.

---

## 10. Schedule

Execute role scenarios **after** column-level tests (09) for core data, **in parallel** with screen matrix (10) during week 3 of master plan.

---

## 11. Document control

| Version | Date | Change |
|---------|------|--------|
| 1.0 | 2026-07-14 | Initial strategy post SoD GO |
