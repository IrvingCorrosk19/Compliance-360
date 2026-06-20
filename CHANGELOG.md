# Changelog

All notable changes to Compliance 360 will be documented in this file.

## v0.2.0-identity-foundation - 2026-06-20

### Added

- Implemented Identity Foundation application services for login, logout, token refresh, password changes, role assignment, permission changes, MFA configuration, and account unlock.
- Added domain support for PasswordHistory, UserSession, MfaConfiguration, account lockout, refresh token session linking, and session revocation.
- Added secure password policy validation, PBKDF2 password hashing, short JWT access token generation, and rotating refresh token flow.
- Added EF Core repository and persistence mappings for Identity Foundation.
- Added Identity module tests covering authentication, authorization, RBAC, token rotation, password policy/history, lockout, session revocation, tenant isolation, audit creation, and MFA readiness.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully.
- Identity module coverage reached 91.51%.

## v0.1.0-tenant-management - 2026-06-20

### Added

- Created the .NET 9 solution structure for Clean Architecture.
- Added foundation projects for Domain, Application, Infrastructure, Web, Shared, and Tests.
- Implemented Tenant Management domain entities: Tenant, Company, Subscription, TenantSettings, and TenantBranding.
- Added Tenant Management application contracts, service, repository abstraction, EF Core repository, audit integration, and dependency injection.
- Added foundation support for audit logs, storage metadata, identity primitives, JWT services, refresh token generation, password hashing, and PostgreSQL-ready persistence configuration.
- Added unit tests for foundation domain rules and Tenant Management use cases.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully.
