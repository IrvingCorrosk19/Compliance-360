# 06 — Test Data Strategy

**Tenant:** `82af3877-2786-4d39-bce8-c981101c771d`  
**Contract reference:** `REGUTRACK 02JUN26 MG.xlsx`  
**Date:** 2026-07-14  
**Status:** Strategy defined; datasets NOT EXECUTED

---

## 1. Purpose

Define **what data** functional tests use, how it is created, maintained, and reconciled against REGUTRACK—without contaminating production or breaking SoD evidence from the security certification pass.

---

## 2. Data categories

| Category | ID prefix | Source | Use |
|----------|-----------|--------|-----|
| Synthetic workflow | `CERT-WF-` | UI/API manual create | Golden-path E2E, SoD spot checks |
| Synthetic column | `CERT-COL-` | Targeted field population | Matrix 09 row execution |
| Imported historical | `REG-IMP-` | XLSX commit | Parity, counts, spot rows |
| Manufacturer vault | `CERT-MFR-` | UI/API | DOCUMENTACION tests |
| License | `CERT-LIC-` | UI/API | LICENCIAS OP tests |
| Bootstrap seed | (system) | `/bootstrap` | Authorities, pack, catalogs |

---

## 3. Synthetic product template

Used by Specialist for workflow tests:

| Field | Value | Excel col |
|-------|-------|-----------|
| CountryCode | `PA` | 1 |
| Category | `Insumos Médicos` | 2 |
| Brand | `4HOSPITALS CERT` | 3 |
| RegulatoryName | `CERT TEST PRODUCT {timestamp}` | 4 |
| Description | Controlled test description | 5 |
| CatalogCode | `CERT-{nnnn}` | 6 |
| DistributorName | `4 Hospital, Inc.` | 8 |
| Initiative | `NEGOCIO BASE` | 9 |
| DeviceRiskClass | `A` or `B` | 15 |
| RegisteredSuppliersCount | 3 | 14 |
| SalesMarketingInput | `Cert test input` | 51 |
| OpportunityAmount | 10000 | 53 |
| SourceLineNumber | null (synthetic) | 88 |

**Manufacturer link:** Use existing lab manufacturer or create `CERT-MFR-{name}` with country `CN`.

---

## 4. Dossier lifecycle data

| Milestone field | Populated when | Excel col |
|-----------------|----------------|-----------|
| RequestedFromFactoryOn | Transition WaitingManufacturerDocuments | 40 |
| EstimatedReceptionOn | Manual / dates API | 41 |
| MaximumReceptionOn | Manual | 42 |
| ReceivedOn | Transition DocumentsReceived | 43 |
| AssembledOn | Transition Assembling → Ready | 45 |
| EstimatedSubmissionOn | Manual | 46 |
| SubmittedOn | Submit action | 47 |
| ObservationReceivedOn | Observation registered | 48 |
| EstimatedApprovalOn | Manual | 49 |
| ApprovedOn | External approve | 50 |
| Comments | Free text on dossier | 54 |

History events (cols 56–87): verify ≥1 event per transition with correct `EventType` and timestamp.

---

## 5. Requirement checklist data

Pack `REGUTRACK-PA-DEFAULT` — 22 items. For submit gate tests:

| Scenario | Critical items state | Expected |
|----------|---------------------|----------|
| TD-REQ-001 | All critical Accepted | Submit allowed |
| TD-REQ-002 | One critical Pending | Submit blocked 400/422 |
| TD-REQ-003 | Critical Rejected | Submit blocked |

Map each item to Excel cols 18–39 per catalog code (LEGAL_ID … ACCESSORIES).

---

## 6. CT/RS registration data

External approve payload:

```json
{
  "registrationNumber": "MQ-CERT-9999-07-26",
  "issuedOn": "2026-07-14T00:00:00Z",
  "expiresOn": "2031-07-13T00:00:00Z",
  "notes": "Functional cert test"
}
```

Validate:

- `SanitaryRegistration` row created
- Appears in Registros view
- Days remaining computed
- Alert engine picks up 90/60/30/15/7/1/0 day windows (use backdated expires for accelerated test)

---

## 7. Manufacturer / DOCUMENTACION data

| Record | Fields | Purpose |
|--------|--------|---------|
| MFR-001 | LegalName, CN, active | Basic list |
| CERT-001 | Type Iso13485, expires +400d | VIGENTE Active |
| CERT-002 | Type CLV, expires +30d | Expiring alert |
| CERT-003 | Expired backdate | Expired status |

Format flags: Apostilled=true for Panama submissions.

---

## 8. License / LICENCIAS OP data

| Record | CompanyName | LicenseType | Purpose |
|--------|-------------|-------------|---------|
| LIC-001 | Multimed | Licencia de Operaciones Dispositivos Medicos | Basic CRUD |
| LIC-002 | 4 Hospitals | Same | Multi-company |

Renewal case: start renewal → verify checklist items seeded from `LicenseOpRequirementCatalog`.

**Gap:** CompanyMetadata (constitution date) — no test data until implemented; mark WAIVED.

---

## 9. Import data strategy

### 9.1 Full commit test

- File: `REGUTRACK 02JUN26 MG.xlsx`
- Pre: DB snapshot
- Steps: stage → validate → simulate → commit
- Post: reconcile counts vs `regutrack_decomposition.json`:
  - CTT REGISTROS (2): ~191 data rows
  - Header count: 88

### 9.2 Spot-check rows

Select 10 rows stratified by:

- With CT/RS number vs N/A
- MINSA vs CSS authority
- Submitted vs Planning status in Excel

Compare field-by-field to UI after import.

### 9.3 Rollback

If commit fails mid-test, restore from pg_dump; do not partial-delete without script.

---

## 10. Data privacy

- Lab tenant only
- Real Excel may contain commercial product names — acceptable internally; do not export evidence to public channels
- Passwords never in test data docs

---

## 11. Refresh cadence

| Event | Action |
|-------|--------|
| Start of test week | Soft reset CERT-* entities |
| After import test | Full DB restore or documented commit id |
| Defect retest | Reuse same dossier id if state allows; else new CERT-WF-* |

---

## 12. Test data IDs registry

Maintain during execution: `evidence/functional/test-data-registry.json`

```json
{
  "products": ["uuid..."],
  "dossiers": ["uuid..."],
  "registrations": ["uuid..."],
  "importJobId": null
}
```

---

## 13. Reconciliation queries (reference)

Post-import spot checks (read-only SQL):

- Count `medical_device_products` where tenant_id = lab
- Count `registration_dossiers` by status
- Match max `source_line_number` to Excel row count

---

## 14. Document control

| Version | Date | Change |
|---------|------|--------|
| 1.0 | 2026-07-14 | Initial strategy |
