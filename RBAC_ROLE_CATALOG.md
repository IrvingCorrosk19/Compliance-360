# RBAC Role Catalog — Compliance 360

> Single source of truth: `src/Compliance360.Domain/Identity/RoleCatalog.cs` (`RoleCatalog`).
> Roles are **code‑first** (not ad‑hoc strings). Platform roles are seeded once in the
> platform tenant; tenant roles are provisioned automatically inside every business tenant.

## Platform roles (scope = Platform)

| Role | Purpose | Key permissions |
|------|---------|-----------------|
| **Platform Administrator** | Administers the whole SaaS platform. Does **not** operate tenant business data and has **no** implicit break‑glass. | All `PLATFORM.*` except `PLATFORM.SUPPORT.ACCESS` (21 grants) |
| **Platform Operations** | Day‑to‑day platform ops: tenant lifecycle, providers, modules, licenses. | `PLATFORM.TENANT.*` (no delete), `PLATFORM.LICENSE/MODULE/PROVIDER.*`, observability, DB, backups, devops (15) |
| **Platform Security** | Global security configuration & platform audit trail. | `PLATFORM.SECURITY.MANAGE`, `PLATFORM.AUDIT.READ/EXPORT`, observability, search (6) |
| **Support Operator (Break Glass)** | Only role that can enter a tenant through the explicit, audited support mechanism. | `PLATFORM.SUPPORT.ACCESS`, `PLATFORM.DASHBOARD.READ`, `PLATFORM.AUDIT.READ`, `PLATFORM.SEARCH` (4) |

## Tenant roles (scope = Tenant — auto‑provisioned per tenant)

| Role | Purpose | Grants |
|------|---------|:-----:|
| **Tenant Administrator** | Profile, users, roles, general settings. Does not operate business data by default. | 13 |
| **Tenant Security Administrator** | MFA, SSO, domains, webhooks, API keys. | 8 |
| **Document Controller** | Creates/maintains controlled documents & workflows. **Cannot approve documents.** | 8 |
| **Quality Manager** | Approver & coordinator: approves documents, workflows, technical sheets, CAPAs, risks, indicators. **Does not create business data.** | 19 |
| **Auditor** | Plans/executes audits, records findings. **Cannot manage or close CAPAs raised from findings.** | 8 |
| **Supplier Manager** | Manages suppliers, documents, evaluations, homologation. | 8 |
| **CAPA Manager** | Manages CAPAs end‑to‑end **except final approval** (Quality Manager). | 7 |
| **Risk Manager** | Manages risk register, treatments, controls. **Approval belongs to Quality Manager.** | 7 |
| **Indicators Manager** | Manages indicators, formulas, thresholds, measurements, export. | 7 |
| **Reporting Manager** | Report center: execution, export, scheduling. | 11 |
| **Storage Administrator** | Document storage & providers. **Does not administer notifications/SMTP.** | 7 |
| **Notification Administrator** | Notifications, templates, SMTP providers. **Does not administer storage.** | 8 |
| **Viewer** | Read‑only across tenant modules. Cannot create/edit/delete/approve/configure. | 11 |

Grant counts above are exactly what is persisted in `role_permissions` and what is
emitted as `permission` claims in the JWT (verified live — see `RBAC_E2E_VALIDATION.md`).

## Role changes vs. the original request

The 17 requested roles were implemented **1:1**; none were split or merged. The audit's
recommendation to demote Quality Manager from an operational "super user" to an
approver/coordinator was implemented (it holds only `*.APPROVE`/`*.CLOSE`/`READ`, never
`*.CREATE`/`*.MANAGE` for business data).
