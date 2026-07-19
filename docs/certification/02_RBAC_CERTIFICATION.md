# 02 — RBAC Certification

**Catalog source:** `RoleCatalog.cs` + `PermissionCatalog` / `RbacCatalog`  
**Provisioning:** `EfRbacProvisioningService` (idempotent sync on Development bootstrap)  
**Verified:** JWT claims after login for all 9 certification users · 2026-07-17  

## Official tenant roles in scope

| Role | Code | Scope |
|------|------|-------|
| Tenant Administrator | TAC | Tenant |
| Regulatory Administrator | RA-ADM | Tenant |
| Regulatory Manager | RA-MGR | Tenant |
| Regulatory Specialist | RA-SPEC | Tenant |
| Regulatory Reviewer | RA-REV | Tenant |
| Regulatory Approver | RA-APPR | Tenant |
| Regulatory Submitter | RA-SUB | Tenant |
| Regulatory Viewer | RA-VIEW | Tenant |
| Quality Manager | QM | Tenant |

## Permission matrix (Regulatory + Audit)

| Permission | TAC | ADM | MGR | SPEC | REV | APPR | SUB | VIEW | QM |
|------------|-----|-----|-----|------|-----|------|-----|------|-----|
| TENANT.READ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| TENANT.USERS / ROLES / AUDIT | ✓ | — | — | — | — | — | — | — | — |
| AUDIT.READ | ✓ | — | — | — | — | — | — | — | ✓ |
| REGULATORY.CONFIGURE | ✓ | ✓ | — | — | — | — | — | — | — |
| REGULATORY.SOD.MANAGE | ✓ | ✓ | ✓ | — | — | — | — | — | — |
| REGULATORY.PRODUCT.READ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | —* |
| REGULATORY.PRODUCT.MANAGE | — | ✓ | — | ✓ | — | — | — | — | — |
| REGULATORY.DOSSIER.READ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| REGULATORY.DOSSIER.CREATE | — | ✓ | — | ✓ | — | — | — | — | — |
| REGULATORY.DOSSIER.UPDATE | — | — | ✓ | ✓ | — | — | — | — | — |
| REGULATORY.DOSSIER.REVIEW | — | — | — | — | ✓ | — | — | — | — |
| REGULATORY.REQUIREMENT.MANAGE | — | — | — | ✓ | ✓ | — | — | — | — |
| REGULATORY.DOSSIER.APPROVE_FOR_SUBMISSION | — | — | — | — | — | ✓ | — | — | — |
| REGULATORY.DOSSIER.SUBMIT | — | — | — | — | — | — | ✓ | — | — |
| REGULATORY.DOSSIER.APPROVE | — | — | ✓ | — | — | — | — | — | ✓ |
| REGULATORY.OBSERVATION.MANAGE | — | — | ✓ | ✓ | — | — | — | — | — |
| REGULATORY.REGISTRATION.MANAGE | — | — | ✓ | — | — | — | — | — | ✓ |
| REGULATORY.MANUFACTURER_DOCUMENT.MANAGE | — | ✓ | — | ✓ | — | — | — | — | — |
| REGULATORY.LICENSE.MANAGE | — | ✓ | — | — | — | — | — | — | — |
| REGULATORY.REPORT.READ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |

\* QM reads dossiers/registrations/reports; product manage not granted (manual).

## Corrections applied to match the manual

1. **RA-ADM gained `REGULATORY.DOSSIER.CREATE`** — manual button `new-product` includes Regulatory Administrator.
2. **`AUDIT.READ` removed from operational RA roles** (ADM/MGR/SPEC/REV/APPR/SUB/VIEW) — manual assigns Audit Trail only to TAC and QM.
3. **Frontend `roleProfile()`** distinguishes TAC (`CONFIGURE` without `PRODUCT.MANAGE`) from RA-ADM (`CONFIGURE` + `PRODUCT.MANAGE`) so RA-ADM is not misclassified as Specialist after gaining CREATE.
4. **API policies granularized** — Reviewer can no longer call transition/observation/license/renewal endpoints via coarse `Regulatory.Manage`.

## Live claim verification (post-bootstrap)

| User | Role claim | DOSSIER.CREATE | AUDIT.READ |
|------|------------|----------------|------------|
| irvingcorrosk19@gmail.com | Tenant Administrator | false | true |
| ra.admin@cert.local | Regulatory Administrator | true | false |
| ra.spec@cert.local | Regulatory Specialist | true | false |
| ra.rev@cert.local | Regulatory Reviewer | false | false |
| ra.appr@cert.local | Regulatory Approver | false | false |
| ra.sub@cert.local | Regulatory Submitter | false | false |
| ra.view@cert.local | Regulatory Viewer | false | false |
| ra.mgr@cert.local | Regulatory Manager | false | false |
| ra.qm@cert.local | Quality Manager | false | true |

## SoD controls (still enforced)

- Prevent self-review / self-approval
- Separate Approver and Submitter
- Separate document uploader and reviewer
- Require internal approval before submission
- Emergency override gated by `REGULATORY.SOD.EMERGENCY_OVERRIDE`

Validated by `e2e/tests/regulatory-sod-roles.spec.ts` (PASS).

## Certification

**RBAC CERTIFIED — COINCIDE with manual.**
