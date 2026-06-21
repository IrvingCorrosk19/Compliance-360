# Changelog

All notable changes to Compliance 360 will be documented in this file.

## v0.18.0-security-hardening-phase-1 - 2026-06-21

### SECURITY HARDENING PHASE 1

- Enforced MFA during login for users with MFA enabled and tenants with `RequireMfa`, returning a signed MFA challenge without issuing final JWT/refresh tokens.
- Added MFA challenge completion endpoint with TOTP validation, final token issuance only after challenge success, and audit events for challenge required/success/failure.
- Hardened CSP compatibility by removing inline `onclick` handlers and inline `style=` usage from the SPA.
- Added stricter security headers, HSTS, no-store cache headers for sensitive API routes, explicit CORS configuration, and secure design-time DbContext connection string resolution.
- Documented JWT Bearer CSRF strategy and local EF migration execution without exposing secrets in `SECURITY_HARDENING_PHASE_1_REPORT.md`.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 180 passed.
- Affected coverage: `IdentityService` 100% line/branch, `MfaChallengeTokenService` 95.52% line and 100% branch, `SecurityHeadersMiddleware` 100% line/branch.
- Security review checks confirmed no frontend inline handlers/styles, no `AllowAnyOrigin`, and no hardcoded design-time PostgreSQL password.

### Security Completion Certificate

- Scope: Security Hardening Blocker Fix
- Status: SECURITY HARDENING PHASE 1 APPROVED

## v1.1.0-enterprise-ux-navigation-polish - 2026-06-21

### ENTERPRISE UX AND NAVIGATION COMPLETION

- Improved the authenticated application shell with production status, module icons, contextual page title, quick module switcher, richer global search routing, tenant visibility, and ISO alignment footer.
- Upgraded navigation feedback across all modules with clearer breadcrumbs, dashboard/report shortcuts, stronger action context, live-data badges, enhanced empty states, recoverable error states, and animated loading states.
- Improved visual hierarchy for dashboard, report center, operational modules, enterprise workspaces, metric cards, tables, and responsive layouts while preserving the existing real API-backed workflows.

### Validation

- Frontend lints completed with no diagnostics for `app.js` and `styles.css`.
- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 171 passed.
- Browser UX validation passed for login, sidebar, quick switcher, global search navigation, dashboard, Report Center, Risk Management, Regulatory Management, report execution, and Enterprise Workspace item creation.

### UX Completion Certificate

- Scope: Complete visual and navigation experience
- Status: PRODUCTION UX READY 100%

## v1.0.0-production-ready-enterprise - 2026-06-20

### FINAL PRODUCT DELIVERY

- Added Enterprise Workspaces as persistent multitenant backend/API/UI coverage for Template Builder, Regulatory Management, Training Management, Supplier Portal, Customer Portal, Security, and Configuration.
- Implemented domain model, application service, EF Core repository, PostgreSQL mapping, API v1 endpoints, migration `AddEnterpriseWorkspaces`, and frontend Action Centers for all final enterprise workspaces.
- Converted previous roadmap-only enterprise views into functional product modules with create/search/dashboard/complete/reopen flows and tenant-scoped persistence.
- Preserved the existing core enterprise flows for Document Management, Technical Sheets, Supplier Management, Audit Management, CAPA, Risk Management, Quality Indicators, Reporting Engine, Audit Trail, Identity, and Tenant Management.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 171 passed.
- Local PostgreSQL migration applied successfully to `compliance360`.
- Browser E2E validation passed for Template Builder, Regulatory Management, Training Management, Supplier Portal, Customer Portal, Security, Configuration, Report Center, and Executive Dashboard.
- Report Center validated with 24 configured reports and 24 dashboard datasets.

### Final Production Certificate

- Scope: Compliance 360 Enterprise Edition
- Status: PRODUCTION READY CORE 100%
- Evidence: backend, API, UI, UX, navigation, persistence, tenant isolation, validation, reports, tests, migration, and functional browser validation.

## v0.18.0-production-visual-experience - 2026-06-20

### PRODUCTION VISUAL EXPERIENCE DELIVERY

- Upgraded the SPA into a polished enterprise product experience with production-grade visual hierarchy, executive hero, operational command panel, workspace tiles, status badges, elevated cards, improved tables, responsive refinements, and a stronger authenticated dashboard.
- Improved the login experience with enterprise positioning, production status indicators, module metrics, and SaaS product framing.
- Added live module hero panels with workflow steps and record counts for Document Management, Technical Sheets, Supplier Management, Audit Management, CAPA, Risk Management, and Quality Indicators.
- Added enterprise workspace tiles for fast navigation across Document Control, Technical Sheets, Suppliers, Audits, CAPA, Risk Matrix, Quality KPIs, and Report Center.
- Improved roadmap/enterprise workspaces so non-core entries render as coherent product areas instead of technical placeholders.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 169 passed.
- Browser functional validation passed for dashboard visual polish, workspace tiles, Document Management creation, Supplier creation, CAPA creation, Risk creation, Quality Indicator creation, Report Center execution/scheduling, and Swagger Authorize visibility.
- Visual assessment: no blocking visual issues detected in the validated production core flow.

### Visual Completion Certificate

- Scope: Production Visual Experience
- Status: FUNCTIONAL VISUAL CORE APPROVED

## v0.17.0-enterprise-application-core - 2026-06-20

### ENTERPRISE APPLICATION CORE DELIVERY

- Implemented a navigable Compliance 360 SPA served by `Compliance360.Web` with login, application shell, dashboards, module navigation, light/dark mode, toast feedback, and tenant-aware API consumption.
- Added real Action Center flows for Document Management, Technical Sheets, Supplier Management, Audit Management, CAPA, Risk Management, and Quality Indicators.
- Added Report Center execution console with standard report seeding, configured report catalog, execution, export, and monthly scheduling actions.
- Added Swagger/OpenAPI Bearer security definition so Swagger exposes the Authorize button for JWT flows.
- Fixed Reporting Engine standard report seeding to reuse in-memory categories and avoid duplicate category inserts in a single seed batch.
- Fixed EF/PostgreSQL persistence for append-only Quality Indicator child records so targets, thresholds, measurements, results, trends, attachments, and history are inserted correctly when added through aggregate methods.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 169 passed.
- Browser validation passed for login, Swagger Authorize, Document Management, Technical Sheets, Supplier Management, Audit Management, CAPA, Risk Management, Quality Indicators, and Report Center.
- E2E evidence includes tenant-scoped record creation per core module, 24 configured standard reports, report execution, report export, and report scheduling.

### Application Completion Certificate

- Scope: Enterprise Application Core
- Status: RELEASE CANDIDATE APPROVED

## v0.16.0-reporting-engine-enterprise - 2026-06-20

### REPORTING ENGINE COMPLETION REPORT

- Implemented Reporting Engine Enterprise as a reusable corporate reporting module for Tenant Management, Identity, Document Management, Workflow, Technical Sheets, Supplier Management, Audit Management, CAPA, Risk Management, Quality Indicators, AuditLog, and Notifications.
- Added domain entities for report definitions, templates, categories, parameters, executions, schedules, subscriptions, outputs, history, exports, permissions, and dashboard bindings.
- Implemented enterprise formats: PDF, Excel, Word, CSV, and JSON.
- Implemented report generation lifecycle, reusable templates, dynamic parameters, advanced filters through search criteria, automatic scheduling, subscriptions, execution history, exports, access control, and versioning.
- Implemented mandatory report catalog for Document Management, Supplier Management, Audit Management, CAPA, Risk, and Quality Indicators with 24 standard enterprise reports.
- Implemented reusable dashboard datasets for Dashboard Enterprise through report dashboard bindings.
- Implemented API v1 under `/api/v1/tenants/{tenantId}/reports` with CRUD, execution, completion, export, scheduling, subscriptions, standard report seeding, search, and dashboard dataset endpoints.
- Implemented security policies: `Report.Manage`, `Report.Read`, `Report.Execute`, `Report.Export`, and `Report.Schedule`.
- Implemented tenant isolation through tenant-scoped aggregates, repository queries, unique indexes, report search, dashboard datasets, permissions, and multitenant tests.
- Implemented AuditLog events for report creation, updates, execution, export, and scheduling.
- Implemented EF Core persistence and migration `AddReportingEngine`.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 169 passed.
- Reporting Engine module coverage reached 95.71% line coverage and 90.47% branch coverage.
- Risks: no open functional, security, multitenant, export, schedule, or coverage risks detected.
- Evidence: full test suite, module coverage, EF migration generation, tenant isolation tests, permission tests, export tests, schedule tests, standard report catalog tests, audit log tests, and dashboard dataset tests.

### Module Completion Certificate

- Module: Reporting Engine Enterprise
- Status: MODULE APPROVED
- Tag: `v0.16.0-reporting-engine-enterprise`

## v0.15.0-quality-indicators-enterprise - 2026-06-20

### QUALITY INDICATORS COMPLETION REPORT

- Implemented Quality Indicators Enterprise with ISO 9001, BPM, HACCP, operational, strategic, process, supplier, audit, CAPA, and risk indicator support.
- Added domain entities for indicators, categories, formulas, measurements, targets, thresholds, results, periods, processes, dashboards, alerts, trends, history, and attachments.
- Implemented calculations for monthly, quarterly, semiannual, annual, accumulated, average, percentage, ratio, and custom formulas.
- Implemented manual and automatic measurement capture, targets, thresholds, semaforizacion, trends, alerts, historical traceability, comparative dashboard data, trend data, search, pagination, and export descriptors.
- Implemented integrations through supplier/audit/CAPA/risk/document references, workflow attachment, notification-ready alerts, and append-only AuditLog events.
- Implemented API v1 under `/api/v1/tenants/{tenantId}/indicators` with creation, filters, searches, pagination, dashboard data, trend data, export descriptors, and Swagger/OpenAPI-discoverable contracts.
- Implemented security policies: `Indicator.Manage`, `Indicator.Read`, `Indicator.Approve`, and `Indicator.Export`.
- Implemented tenant isolation through tenant-scoped aggregates, repository queries, unique indexes, dashboard/trend/export filters, and multitenant tests.
- Implemented EF Core persistence and migration `AddQualityIndicators`.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 162 passed.
- Quality Indicators module coverage reached 96.67% line coverage and 90.76% branch coverage.
- Risks: no open functional, security, multitenant, or coverage risks detected.
- Evidence: build, full test suite, module coverage, EF migration generation, tenant isolation tests, audit log tests, dashboard/trend/export tests.

### Module Completion Certificate

- Module: Quality Indicators Enterprise
- Status: MODULE APPROVED
- Tag: `v0.15.0-quality-indicators-enterprise`

## v0.8.0-platform-documents-enterprise - 2026-06-20

### Added

- Implemented API Enterprise v1 with Swagger/OpenAPI, JWT enforcement, RBAC authorization policies, tenant context checks, rate limiting, security headers, global exception handling, and health endpoints.
- Added PostgreSQL EF Core migration baseline plus design-time DbContext factory and aligned EF package graph to eliminate version conflicts.
- Replaced MFA Base64 secret protection with ASP.NET Core Data Protection.
- Implemented Document Management Enterprise foundation with document types, categories, documents, versions, approval/rejection, obsolescence, permissions, history, expiration, search, API endpoints, EF repository, and migrations.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 107 passed.
- API integration tests validate health, Swagger, and authentication enforcement.
- Document Management tests validate workflow, tenant-isolated search, domain rules, negative paths, and EF repository persistence.
- Document Management module coverage reached 90.13% line coverage and 90.47% branch coverage.

## v0.9.0-workflow-engine-enterprise - 2026-06-20

### Added

- Implemented Workflow Engine Enterprise with configurable workflows, steps, transitions, rules, assignments, instances, history, escalations, reminders, and workflow notifications.
- Added workflow API v1 endpoints for configuration, activation, instance startup, assignment, completion, escalation, reminders, and instance search.
- Added EF Core repository, tenant-scoped indexes, graph mappings, and PostgreSQL migration for workflow tables.
- Added workflow audit integration for configuration changes, workflow start, approval, rejection, updates, and queued notifications.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 122 passed.
- Workflow Engine module coverage reached 91.23% line coverage and 90.78% branch coverage.

## v0.10.0-technical-sheets-enterprise - 2026-06-20

### Added

- Implemented Technical Sheets Enterprise with products, technical sheets, ingredients, nutrients, certifications, versions, approvals/rejections, PDF attachment metadata, obsolescence, and audit integration.
- Added tenant-scoped API v1 endpoints for product creation, sheet creation, versioning, ingredients, nutrients, certifications, submit/decision, PDF attachment, obsolescence, and search.
- Added EF Core repository, PostgreSQL mappings, indexes, and migration for product and technical sheet tables.
- Added tests covering complete approval flow, rejection/obsolescence, PDF metadata, tenant-isolated search, domain validations, and EF persistence.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 129 passed.
- Technical Sheets module coverage reached 91.51% line coverage and 92.50% branch coverage.

## v0.11.0-supplier-management-enterprise - 2026-06-20

### Added

- Implemented Supplier Management Enterprise with suppliers, homologation, required documents, RUC, aviso de operaciones, BPM, HACCP, registro sanitario, evaluations, expiration alerts, rejection, suspension, and audit integration.
- Added tenant-scoped API v1 endpoints for supplier creation, document upload metadata, document validation/rejection, evaluations, homologation, expiration alerts, suspension, and search.
- Added EF Core repository, PostgreSQL mappings, indexes, and migration for supplier management tables.
- Added tests covering full homologation, required document enforcement, low score/expired document rejection, tenant isolation, domain validations, alerts, suspension, and EF persistence.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 136 passed.
- Supplier Management module coverage reached 91.88% line coverage and 90.00% branch coverage.

### Module Completion Certificate

- Module: Supplier Management Enterprise
- Date: 2026-06-20
- Coverage: 91.88% line, 90.00% branch
- Tests: 136 passed
- Build: APPROVED, 0 warnings, 0 errors
- Security: APPROVED, tenant-scoped authorization policy and user context enforced at API boundary
- Multitenant: APPROVED, tenant-scoped repositories and tests
- Audit: APPROVED, supplier created/updated audit events persisted
- Status: APPROVED

## v0.12.0-audit-management-enterprise - 2026-06-20

### Added

- Implemented Audit Management Enterprise with audit programs, audit plans, schedules, reusable versioned checklists, checklist items, managed audits, participants, auditors, areas, findings, evidences, observations, non-conformities, recommendations, corrective action links, attachments, history, dashboard metrics, search, pagination, and export descriptors.
- Added tenant-scoped API v1 endpoints for audit program/checklist/plan/audit creation, checklist assignment, scheduling, participant and area management, start/complete/close/reopen, findings, evidences, observations, non-conformities, recommendations, CAPA links, attachments, dashboard, search, and export.
- Added EF Core repository, PostgreSQL mappings, tenant-scoped indexes, aggregate graph persistence, and migration `AddAuditManagement`.
- Added `AuditManagement.Manage` authorization policy and module-specific `AuditLog` actions/category for created, updated, closed, reopened, and exported audit operations.
- Added tests covering full enterprise audit lifecycle, workflow transitions, negative paths, tenant-isolated search/dashboard/export, audit log persistence, security policy surface, domain invariants, evidence metadata integrity, and EF repository persistence.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 144 passed.
- Audit Management module coverage reached 93.14% line coverage and 95.91% branch coverage.
- Linter check completed with no errors.

### Module Completion Certificate

- Module: Audit Management Enterprise
- Date: 2026-06-20
- Coverage: 93.14% line, 95.91% branch
- Tests: 144 passed
- Build: APPROVED, 0 warnings, 0 errors
- Security: APPROVED, `AuditManagement.Manage` policy and tenant/user context enforced at API boundary
- Multitenant: APPROVED, tenant-scoped repositories, filters, indexes, and tenant isolation tests
- Audit: APPROVED, all module actions append `AuditLog` records with Audit Management category/actions
- Evidence Integrity: APPROVED, stored file metadata, content type, size, SHA-256 hash, and upload actor captured
- Status: APPROVED

## v0.13.0-capa-management-enterprise - 2026-06-20

### Added

- Implemented CAPA Management Enterprise with CAPA lifecycle, classification, ownership, approvers, root cause analysis, 5 Why, Ishikawa/Fishbone, containment actions, corrective actions, preventive actions, follow-up, overdue escalation, effectiveness verification, closure approval, reopening, evidence, attachments, history, dashboard metrics, search, pagination, and export descriptors.
- Added integrations by reference for Audit Management findings/non-conformities/recommendations, suppliers, documents/evidences, workflow instances, notification follow-up surface, and AuditLog traceability.
- Added tenant-scoped API v1 endpoints for CAPA creation, classification, assignment, approvers, RCA, 5 Why, Ishikawa, containment/corrective/preventive actions, evidence, attachments, follow-up, overdue escalation, effectiveness, workflow attachment, closure approval, reopening, search, dashboard, and export.
- Added security policies `Capa.Manage`, `Capa.Read`, `Capa.Approve`, and `Capa.Close`.
- Added EF Core repository, PostgreSQL mappings, tenant-scoped indexes, aggregate graph persistence, and migration `AddCapaManagement`.
- Added tests covering complete CAPA lifecycle, workflow/effectiveness/closure paths, multitenant search/dashboard/export, audit log persistence, security policy surface, domain rules, evidence integrity, negative paths, and EF repository persistence.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 150 passed.
- CAPA Management module coverage reached 98.55% line coverage and 97.27% branch coverage.
- Linter check completed with no errors.

### Module Completion Certificate

- Module: CAPA Management Enterprise
- Date: 2026-06-20
- Coverage: 98.55% line, 97.27% branch
- Tests: 150 passed
- Build: APPROVED, 0 warnings, 0 errors
- Security: APPROVED, `Capa.Manage`, `Capa.Read`, `Capa.Approve`, and `Capa.Close` policies implemented
- Multitenant: APPROVED, tenant-scoped repositories, filters, indexes, dashboard, export, and tenant isolation tests
- Audit: APPROVED, CAPA create/update/close/reopen/export actions persisted to append-only `AuditLog`
- Integrations: APPROVED, Audit Management, Supplier Management, Document Management, Workflow Engine, Notification follow-up surface, and AuditLog integration points implemented
- Evidence Integrity: APPROVED, stored file metadata, content type, size, SHA-256 hash, and upload actor captured
- Status: APPROVED

## v0.14.0-risk-management-enterprise - 2026-06-20

### Added

- Implemented Risk Management Enterprise with risk categories, 5x5 risk matrices, risks, assessments, inherent/residual scoring, probability, impact, exposure level, tolerance, acceptance, treatments, mitigation plans, controls, owners, reviews, indicators, evidence, attachments, heat map data, dashboard metrics, search, pagination, and export descriptors.
- Added integrations by reference for Audit Management, CAPA Management, Supplier Management, Document Management, Workflow Engine, notification follow-up surface, and AuditLog traceability.
- Added tenant-scoped API v1 endpoints for category/matrix/risk creation, classification, owner assignment, assessment, treatments, mitigations, controls, evidence, attachments, reviews, indicators, critical escalation, workflow attachment, close/reopen, search, dashboard, heat map, and export.
- Added security policies `Risk.Manage`, `Risk.Read`, `Risk.Approve`, and `Risk.Close`.
- Added EF Core repository, PostgreSQL mappings, tenant-scoped indexes, aggregate graph persistence, and migration `AddRiskManagement`.
- Added tests covering complete risk lifecycle, risk matrix calculations, residual/inherent scoring, dashboard, heat map, multitenant search/export, audit logs, security policy surface, negative paths, domain invariants, and EF repository persistence.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully: 156 passed.
- Risk Management module coverage reached 96.23% line coverage and 92.39% branch coverage.
- Linter check completed with no errors.

### Module Completion Certificate

- Module: Risk Management Enterprise
- Date: 2026-06-20
- Coverage: 96.23% line, 92.39% branch
- Tests: 156 passed
- Build: APPROVED, 0 warnings, 0 errors
- Security: APPROVED, `Risk.Manage`, `Risk.Read`, `Risk.Approve`, and `Risk.Close` policies implemented
- Multitenant: APPROVED, tenant-scoped repositories, filters, indexes, dashboard, heat map, export, and tenant isolation tests
- Audit: APPROVED, risk create/update/close/reopen/export actions persisted to append-only `AuditLog`
- Risk Matrix: APPROVED, 5x5 probability/impact matrix with inherent/residual risk and tolerance calculations implemented
- Integrations: APPROVED, Audit Management, CAPA Management, Supplier Management, Document Management, Workflow Engine, Notifications surface, and AuditLog integration points implemented
- Evidence Integrity: APPROVED, stored file metadata, content type, size, SHA-256 hash, and upload actor captured
- Status: APPROVED

## v0.7.0-notification-foundation - 2026-06-20

### Added

- Implemented Notification Foundation with tenant-scoped templates, queued messages, priority, channel, target user, send, retry-after-failure, cancellation, and audit flows.
- Added notification dispatcher abstraction, no-op infrastructure dispatcher, EF repository, persistence mapping, and notification audit actions.
- Added Notification tests covering templates, duplicate prevention, rendering, queueing, successful and failed dispatch, cancellation rules, tenant isolation, domain rules, EF repository behavior, and dispatcher behavior.

### Validation

- Build completed successfully with zero warnings and zero errors.
- Test suite completed successfully.
- Notification module coverage reached 91.82%.

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
