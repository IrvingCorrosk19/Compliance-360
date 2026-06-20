# Changelog

All notable changes to Compliance 360 will be documented in this file.

## v0.6.0-storage-foundation - 2026-06-20

### Added

- Implemented Storage Foundation service for tenant-scoped file upload, metadata retrieval, download registration, availability, quarantine, and soft deletion flows.
- Added EF storage repository, audit integration, SHA-256 integrity tracking, local storage size accounting, and metadata status transitions.
- Added Storage tests covering upload, tenant isolation, download blocking, status changes, local file persistence, EF repository behavior, and domain rules.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully.
- Storage module coverage reached 92.20%.

## v0.5.0-mfa-foundation - 2026-06-20

### Added

- Implemented MFA Foundation service for setup, enable, verification, and disable flows.
- Added TOTP generation and verification, MFA secret protection, tenant-scoped MFA repository, audit integration, and failed verification tracking.
- Added MFA coverage for setup, duplicate prevention, invalid challenge handling, enable/disable flows, tenant isolation, TOTP behavior, secret protection, and EF repository persistence.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully.
- MFA module coverage reached 92.50%.

## v0.4.0-rbac-foundation - 2026-06-20

### Added

- Implemented RBAC Foundation service for role creation, permission creation, role assignment, permission grants, authorization decisions, and user permission set queries.
- Added tenant-scoped RBAC repository with EF Core implementation and audit integration.
- Added ABAC-style tenant/entity isolation checks during authorization.
- Added RBAC coverage for role and permission management, duplicate validation, authorization allow/deny, tenant isolation, repository behavior, and audit creation.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully.
- RBAC module coverage reached 95.62%.

## v0.3.0-audit-foundation - 2026-06-20

### Added

- Implemented Audit Foundation domain model with AuditLog, AuditEvent, AuditCategory, AuditMetadata, AuditContext, and AuditSnapshot.
- Added append-only audit rules, tenant-scoped search, RBAC-based audit read authorization, retention evaluation, and sensitive payload redaction.
- Added EF Core audit repository, indexed audit mappings, SaveChanges audit interceptor, request audit context middleware, and dependency injection wiring.
- Added audit coverage for creation, query, tenant isolation, security validation, search, retention, append-only enforcement, repository behavior, and automatic EF interception.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully.
- Audit module coverage reached 91.94%.

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
