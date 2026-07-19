# 03 — UI Gap Report

**Rule:** Manual defines UI. System was adapted. Manual was never modified.

## Gaps found → closed

| ID | Area | Manual | System (before) | Correction | Status |
|----|------|--------|-----------------|------------|--------|
| UI-01 | Nav | No duplicate dashboard entry | `compliance` duplicated Dashboard | Removed from `navigation` in `app.js` | CLOSED |
| UI-02 | Nav | Security screen for TAC | Route existed but `TENANT.SECURITY` hid it; no page | Gate with `TENANT.USERS`; implemented `renderSecurityCenter` | CLOSED |
| UI-03 | Nav | Audit Trail only TAC/QM | Visible to all RA via `AUDIT.READ` on every RA role | Removed `AUDIT.READ` from operational RA roles; quick action gated | CLOSED |
| UI-04 | RA tabs | Strict per-role screen lists | Loose visibility (Viewer saw Fabricantes/Licencias; Reviewer saw Portafolio) | `PROFILE_VIEWS` map exact to manual | CLOSED |
| UI-05 | Product modal | Risk class A/B/C | Options A/B/C/D | Dropped D | CLOSED |
| UI-06 | Product modal | País required (PA) | Hardcoded country, no field | Added `#ra-np-country` | CLOSED |
| UI-07 | Product create | Enterprise form | Native `prompt()` historically; modal incomplete | Full modal (brand, name, code, risk, country, authority, comments) | CLOSED |
| UI-08 | Manufacturer | Formal fields | Native `prompt()` | `openFormModal` `#ra-add-mfr-modal` | CLOSED |
| UI-09 | License | Compañía + Tipo | Hardcoded payload, no form | `openFormModal` `#ra-add-lic-modal` | CLOSED |
| UI-10 | Observation | Textarea ≤4000 | Native `prompt()` | `#ra-observe-modal` | CLOSED |
| UI-11 | External CT/RS | Number + expiry | Dual `prompt()` | `#ra-approve-ext-modal` | CLOSED |
| UI-12 | Observation button | Manager only | Visible to any `OBSERVATION.MANAGE` (incl. Specialist) | Requires `obsManage` ∧ `approveExternal` | CLOSED |
| UI-13 | Quick switcher | Only permitted routes | Listed all routes | Filtered by `canNavigate` | CLOSED |
| UI-14 | Dashboard quick actions | Only permitted | Always showed Bitácora / Admin | Gated | CLOSED |

## Remaining intentional differences

| Item | Notes |
|------|-------|
| Studio / Form Template Builder | Not in primary manual navigation for RA certification scope. Out of this certification boundary. |
| MaximumReceptionOn date control | Optional specialist field; dates API exists (`PUT /dossiers/{id}/dates`). Not blocking certified workflow. Tracked as enhancement if Product Owner requires UI control on detail. |

## Visual evidence

Screenshots per role: `docs/certification/evidence/cert-role-*.png`  
Workflow screenshots: `docs/certification/evidence/flow-*.png`

## Certification

**All blocking UI gaps CLOSED. UI COINCIDE with manual for certified scope.**
