# Tenant Administration Center - Changelog

Fecha: 2026-06-25

## Added

- Tenant domain administration with DNS verification token, status, certificate state, HTTPS flag and redirects.
- Enterprise SSO configuration model for OIDC, OAuth2, SAML, LDAP and Active Directory.
- API credential model with secure hashing, scopes, expiration, rotation and revocation.
- Webhook endpoint model with signing secret hash, events, retry configuration and delivery logs.
- Tenant license model with features, modules, entitlements, consumption, renewal and period.
- Health center and backup record model with RPO/RTO, duration, result and component signals.
- Tenant user administration APIs for create/invite, disable, unlock, reset MFA, assign/revoke role and close sessions.
- Granular TAC permissions for domains, SSO, webhooks, API keys, health and backups.
- Backend CSV audit timeline export.
- Frontend functional TAC tabs for domains, SSO, API keys, webhooks, users and health/backups.
- EF migration `AddTenantAdministrationOmega`.

## Changed

- `Tenant.Read` policy is now strict instead of granting read through any tenant permission.
- Audit context middleware runs after authentication so claims are available.
- User model includes `ForcePasswordChangeRequired`.
- TAC frontend now loads `/administration-center` instead of only dashboard data.

## Validation

- `dotnet build Compliance360.sln`: passed.
- `dotnet test Compliance360.sln`: 224 passed, 0 failed.
- `node --check src/Compliance360.Web/wwwroot/app.js`: passed.
- `dotnet ef migrations script --idempotent`: passed and generated `artifacts/migrations/tenant-administration-omega.sql`.
- `dotnet ef database update`: applied `20260626004152_AddTenantAdministrationOmega` to local PostgreSQL.
- Live HTTP smoke: not completed because local server startup hung before binding the URL.
