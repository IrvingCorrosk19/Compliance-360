# Changelog

All notable changes to Compliance 360 will be documented in this file.

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
