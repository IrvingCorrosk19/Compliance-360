# 02 — Application Functional Map (AS-IS)

## Architecture
- .NET 9 monolith: Domain / Application / Infrastructure / Web (SPA wwwroot) / Worker
- PostgreSQL 18 · EF Core · JWT + sessions
- Ports lab: Web http://localhost:5272 · Worker heartbeat required for healthy bootstrap

## Navigation (SPA hash)
#/dashboard · #/regulatory · #/alert-center · #/audit-trail · #/reports · #/tenant-administration · #/superadmin-platform · QMS modules

## RA Console tabs
Dashboard · Portfolio · Pipeline · Expedientes · CT/RS · Manufacturers · Licenses · Alerts · Import · Config/SoD (role-gated)

## APIs
- /api/v1/tenants/{tenantId}/regulatory/* (products, dossiers, evidence, transitions, submit, approve, SoD, alerts, registrations, licenses, manufacturers, imports)
- /api/v2/tenants/{tenantId}/regulatory/dossiers/{id}/* (workflow, timeline, technical-review, corrections, reopen/override)
- /api/v2/tenants/{tenantId}/alert-center/* (inbox, providers, …)
- /api/v1/auth/*

## Roles (verified present)
Platform Administrator; Tenant Administrator; Regulatory Administrator/Manager/Specialist/Reviewer/Approver/Submitter/Viewer; Quality Manager; + QMS roles in cert tenant.
