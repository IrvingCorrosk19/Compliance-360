# RBAC Permission Catalog — Compliance 360

> Single source of truth: `src/Compliance360.Domain/Identity/RbacCatalog.cs` (`PermissionCatalog`).
> Backend authorization policies, JWT `permission` claims and the frontend guard all reference **these codes and nothing else**.

## Naming convention

All permissions follow a uniform pattern:

```
MODULE(.SUBMODULE).ACTION
```

Allowed actions: `READ`, `CREATE`, `UPDATE`, `DELETE`, `APPROVE`, `EXPORT`, and `MANAGE`/`CLOSE`/`SEND`/`ADMIN` **only where a coarser capability is genuinely a single operation**. Monolithic `*.MANAGE` catch‑alls for CRUD modules were eliminated.

## Platform scope (administered from the platform tenant)

| Code | Action | Description |
|------|--------|-------------|
| `PLATFORM.DASHBOARD.READ` | Read | Platform command center dashboard |
| `PLATFORM.TENANT.READ` | Read | List/inspect tenants |
| `PLATFORM.TENANT.CREATE` | Create | Provision a tenant |
| `PLATFORM.TENANT.UPDATE` | Update | Global tenant profile & limits |
| `PLATFORM.TENANT.STATUS` | Update | Suspend / activate / change lifecycle |
| `PLATFORM.TENANT.DELETE` | Delete | Archive / decommission a tenant |
| `PLATFORM.TENANT.RESTORE` | Update | Restore an archived tenant |
| `PLATFORM.LICENSE.MANAGE` | Manage | Licenses, plans, subscriptions |
| `PLATFORM.MODULE.MANAGE` | Manage | Available modules & feature flags |
| `PLATFORM.PROVIDER.READ` | Read | Global provider configuration |
| `PLATFORM.PROVIDER.MANAGE` | Manage | Global provider configuration |
| `PLATFORM.SECURITY.MANAGE` | Manage | Global security (SSO, OAuth, API keys) |
| `PLATFORM.OBSERVABILITY.READ` | Read | Global metrics & health |
| `PLATFORM.AUDIT.READ` | Read | Global audit trail |
| `PLATFORM.AUDIT.EXPORT` | Export | Export global audit trail |
| `PLATFORM.DATABASE.READ` | Read | DB health & migrations |
| `PLATFORM.AI.MANAGE` | Manage | Global AI configuration |
| `PLATFORM.CONFIGURATION.MANAGE` | Manage | Global system configuration |
| `PLATFORM.BACKUP.READ` | Read | Backup status |
| `PLATFORM.DEVOPS.READ` | Read | DevOps / release info |
| `PLATFORM.SEARCH` | Read | Global platform search |
| `PLATFORM.SUPPORT.ACCESS` | Manage | **Break‑glass** explicit, audited tenant access for support |

## Tenant administration scope

| Code | Action | Description |
|------|--------|-------------|
| `TENANT.READ` | Read | Tenant profile & dashboards |
| `TENANT.UPDATE` | Update | Company profile |
| `TENANT.BRANDING` | Update | Branding |
| `TENANT.SECURITY` | Manage | MFA / security policies |
| `TENANT.STORAGE` | Manage | Storage settings |
| `TENANT.NOTIFICATIONS` | Manage | Notification settings |
| `TENANT.INTEGRATIONS` | Manage | Integrations |
| `TENANT.BILLING` | Read | Billing information |
| `TENANT.USERS` | Manage | Tenant users |
| `TENANT.ROLES` | Manage | Tenant roles |
| `TENANT.AUDIT` | Read | Tenant audit trail |
| `TENANT.DOMAINS` | Manage | Domains |
| `TENANT.SSO` | Manage | SSO |
| `TENANT.WEBHOOKS` | Manage | Webhooks |
| `TENANT.API_KEYS` | Manage | API keys |
| `TENANT.HEALTH` | Read | Tenant health |
| `TENANT.BACKUP` | Read | Tenant backup status |
| `IDENTITY.MANAGE` | Manage | Identities (users, credentials, MFA enrolment) |
| `RBAC.MANAGE` | Manage | Roles & permission grants inside the tenant |
| `AUDIT.READ` | Read | Audit trail |

## Business modules (granular, SoD‑ready)

| Module | READ | CREATE | UPDATE | APPROVE | Other |
|--------|------|--------|--------|---------|-------|
| Document | `DOCUMENT.READ` | `DOCUMENT.CREATE` | `DOCUMENT.UPDATE` | `DOCUMENT.APPROVE` | — |
| Workflow | `WORKFLOW.READ` | `WORKFLOW.CREATE` | `WORKFLOW.UPDATE` | `WORKFLOW.APPROVE` | — |
| Technical Sheet | `TECHNICALSHEET.READ` | `TECHNICALSHEET.CREATE` | `TECHNICALSHEET.UPDATE` | `TECHNICALSHEET.APPROVE` | — |
| Supplier | `SUPPLIER.READ` | `SUPPLIER.CREATE` | `SUPPLIER.UPDATE` | `SUPPLIER.APPROVE` | — |
| Audit Management | `AUDITMANAGEMENT.READ` | — | — | — | `AUDITMANAGEMENT.MANAGE` |
| CAPA | `CAPA.READ` | — | — | `CAPA.APPROVE` | `CAPA.MANAGE`, `CAPA.CLOSE` |
| Risk | `RISK.READ` | — | — | `RISK.APPROVE` | `RISK.MANAGE`, `RISK.CLOSE` |
| Indicator | `INDICATOR.READ` | — | — | `INDICATOR.APPROVE` | `INDICATOR.MANAGE`, `INDICATOR.EXPORT` |
| Report | `REPORT.READ` | — | — | — | `REPORT.MANAGE`, `REPORT.EXECUTE`, `REPORT.EXPORT`, `REPORT.SCHEDULE` |
| Storage | `STORAGE.READ` | `STORAGE.CREATE` | `STORAGE.UPDATE` | — | `STORAGE.DELETE` |
| Notification | `NOTIFICATION.READ` | `NOTIFICATION.SEND`, `NOTIFICATION.TEMPLATE` | — | — | `NOTIFICATION.MANAGE`, `NOTIFICATION.ADMIN` |
| Observability | `OBSERVABILITY.READ` | — | — | — | `OBSERVABILITY.MANAGE`, `OBSERVABILITY.ADMIN` |

## Superset semantics (backend policies)

Read policies accept any higher action within the same module, so a creator/approver
can always read without an extra grant (e.g. `DOCUMENT.CREATE` implicitly satisfies a
`DOCUMENT.READ` requirement). This is implemented in
`src/Compliance360.Web/Security/PermissionPolicies.cs` and does **not** widen SoD:
approval remains a distinct, non‑implied permission.

## Removed (legacy) permissions

Eliminated during the rebuild and pruned from the database:

- Monolithic: `DOCUMENT.MANAGE`, `SUPPLIER.MANAGE`, `WORKFLOW.MANAGE`, `TECHNICALSHEET.MANAGE`, `STORAGE.MANAGE`, `AUDIT.MANAGE`
- Legacy platform namespace: `SUPERADMIN.*` (replaced by `PLATFORM.*`)
- Tenant lifecycle mislocated on the tenant namespace: `TENANT.CREATE/DELETE/RESTORE/STATUS/MANAGE` (now `PLATFORM.TENANT.*`)
- Invented E2E codes: `QUALITY_E2E_*`, `API_RBAC_*`, `API_DEBUG_*`

**Automated guard:** `tests/Compliance360.Tests/RbacCatalogTests.cs` fails the build if any of the removed codes reappear or if a role references an unknown permission.
