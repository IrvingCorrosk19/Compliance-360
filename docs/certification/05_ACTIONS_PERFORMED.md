# 05 — Actions Performed

All changes adapt the **system** to the **manual**. The manual was not edited.

## Backend

| File | Change | Reason |
|------|--------|--------|
| `src/Compliance360.Domain/Identity/RoleCatalog.cs` | Added `RegulatoryDossierCreate` to Regulatory Administrator | Manual: RA-ADM can create product + dossier |
| `src/Compliance360.Domain/Identity/RoleCatalog.cs` | Removed `AuditRead` from ADM/MGR/SPEC/REV/APPR/SUB/VIEW | Manual: Audit Trail only TAC + QM |
| `src/Compliance360.Web/Security/PermissionPolicies.cs` | Added granular policies: DossierUpdate, RequirementUpdate, ObservationManage, ManufacturerManage, LicenseManage, DossierCreate, RegistrationManage | Manual: one permission per action |
| `src/Compliance360.Web/Api/FoundationEndpoints.cs` | Replaced coarse `RegulatoryManage` on write endpoints with granular policies | Prevent Reviewer/etc. from calling unauthorized APIs |

## Frontend

| File | Change | Reason |
|------|--------|--------|
| `wwwroot/regulatory-affairs.js` | Reworked `roleProfile()` (tac/admin/manager/qm/…) | Correct classification after ADM gains CREATE |
| `wwwroot/regulatory-affairs.js` | Added `PROFILE_VIEWS` exact tab sets | Manual screens per role |
| `wwwroot/regulatory-affairs.js` | Prep actions require `DOSSIER.UPDATE` only | ADM creates but does not operate |
| `wwwroot/regulatory-affairs.js` | Product modal: risk A/B/C + País field | Manual fields.json |
| `wwwroot/regulatory-affairs.js` | Added `openFormModal` + modals for manufacturer, license, observation, external CT/RS | Eliminate native prompts; match field contracts |
| `wwwroot/regulatory-affairs.js` | Observation button requires Manager (obs ∧ external) | Manual buttons.json |
| `wwwroot/app.js` | Removed `compliance` nav item | Not in manual |
| `wwwroot/app.js` | Security route permissions + `renderSecurityCenter` | Manual Security screen for TAC |
| `wwwroot/app.js` | Gate Bitácora quick action + filter quick switcher | Hide unauthorized routes |
| `wwwroot/index.html` | Cache bump `app.js?v=cert-1` | Force clients to load fixes |
| `wwwroot/locales/es.json` + `en.json` | New keys for modal subtitles / license fields / País EN | i18n coverage |

## Tests

| File | Change |
|------|--------|
| `e2e/tests/manual-roles-browser.spec.ts` | Full rewrite: exact sidebar, exact RA tabs, modal/field asserts, granular DENY probes, Security UI |
| `e2e/tests/manual-workflow-cert.spec.ts` | New: full dossier lifecycle via UI modals for all SoD roles |

## Runtime

| Action | Result |
|--------|--------|
| `dotnet build` | 0 errors / 0 warnings |
| Development bootstrap | Role permissions re-synced (INSERT/DELETE role_permissions observed) |
| JWT claim audit | DOSSIER.CREATE on RA-ADM; AUDIT.READ only TAC/QM |

## Method notes

- No password hashes were hand-edited; existing cert users (`OwnerStart!2026`) reused.
- No manual HTML/JSON modified.
- RBAC changes applied via catalog + provisioning service (not ad-hoc SQL grants).
