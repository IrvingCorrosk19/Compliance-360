# REGUTRACK Production GO

**Verdict:** GO for REGUTRACK replacement (lab certified 2026-07-16).

## Scope in production

| In | Out |
|----|-----|
| Auth / JWT / MFA | Template Builder |
| RBAC + SoD Regulatory roles | Documents / CAPA / Risk / Indicators |
| Tenant Administration Center | Reports / Training / Portals |
| SuperAdmin platform | Technical Sheets / Suppliers |
| Regulatory Affairs + XLSX import | Audit Management / Workflows |
| Audit trail, storage, notifications | Enterprise workspaces / Form templates |

## Certified E2E (5/5 PASS)

- SoD multi-role happy path + negatives
- Platform Admin — legacy QMS nav/APIs gone (404 JSON)
- Tenant Admin — password reset UI + RA console
- Specialist — Product → Dossier (22 checklist) → submit blocked
- Tenant Admin — REGUTRACK XLSX stage via UI

Artifacts: `artifacts/e2e/`

## Deploy checklist

1. Connection string + `Jwt:SigningKey` + storage/SMTP secrets
2. `dotnet ef database update` then run `scripts/remove-non-regutrack-tables.sql` (Development bootstrap already drops QMS tables after migrate)
3. Bootstrap Platform Admin; provision tenant SoD users as needed
4. Smoke: `/health`, login, `#/regulatory`, import XLSX

## Known residual debt (non-blocking)

- Domain/EF still models retired QMS entities; APIs and DI are unregistered; migration drops tables on apply
- Do not delete `REGUTRACK 02JUN26 MG.xlsx`

## Lab credentials (dev only — rotate for prod)

| Role | Email |
|------|-------|
| Platform Admin | `admin@compliance360.local` |
| Tenant Admin | `irvingcorrosk19@gmail.com` |
| RA Specialist | `ra.spec@cert.local` |
| RA Reviewer / Approver / Submitter / Viewer | `ra.rev@` / `ra.appr@` / `ra.sub@` / `ra.view@cert.local` |
