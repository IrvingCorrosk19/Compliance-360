# 02 — APPLICATION FUNCTIONAL MAP

**Program:** Final Enterprise Functional Certification  
**Source:** `FoundationEndpoints.cs`, `app.js`, `RoleCatalog.cs`, `PermissionPolicies.cs`  
**Date:** 2026-07-05

---

## 1. Arquitectura funcional

| Capa | Tecnología | Responsabilidad |
|---|---|---|
| SPA | Vanilla JS hash router | Navegación, RBAC UI, formularios |
| API | ASP.NET Core Minimal APIs | Autorización por permiso, multi-tenant |
| RBAC | JWT claims + policies | 17 roles catálogo, 15 certificados |
| Datos | PostgreSQL + EF Core | Aislamiento por TenantId |

**API base:** `/api/v1` · **Routing SPA:** `#/route`

---

## 2. Pantallas y rutas SPA (20)

### 2.1 Autenticación

| Pantalla | Elementos |
|---|---|
| Login | `#login-form`, tenantId, email, password, submit |
| MFA Challenge | Código verificación (si aplica) |

### 2.2 Command Center

| Ruta | Pantalla | Permisos | Renderer |
|---|---|---|---|
| `#/dashboard` | Executive Dashboard | TENANT.READ | Métricas agregadas + tiles |
| `#/compliance` | Compliance Dashboard | TENANT.READ | Misma vista que dashboard |
| `#/reports` | Report Center | REPORT.* | Seed, execute, schedule, export |
| `#/audit-trail` | Audit Trail | AUDIT.READ | Tabla eventos |

### 2.3 Operations (módulos de negocio)

| Ruta | Módulo | CRUD UI | API profundidad |
|---|---|---|---|
| `#/documents` | Document Management | Create (type+category+doc) | Full lifecycle API |
| `#/technical-sheets` | Technical Sheets | Create (product+sheet) | Full API |
| `#/suppliers` | Supplier Management | Create supplier | Full API |
| `#/audits` | Audit Management | Create (program+plan+audit) | Full API |
| `#/capa` | CAPA | Create CAPA | Full lifecycle API |
| `#/risks` | Risk Management | Create (category+matrix+risk) | Full API + heat map |
| `#/indicators` | Quality Indicators | Create (+ target/threshold API) | Full API |
| `#/superadmin-platform` | SuperAdmin Platform Center | 13 tabs | Platform APIs |

### 2.4 Enterprise

| Ruta | Pantalla | Tabs/Formularios |
|---|---|---|
| `#/tenant-administration` | Tenant Administration Center | 15 tabs (General, Branding, Security, Users, RBAC, Licensing, Domains, SSO, API Keys, Webhooks, Storage, Notifications, Health, Audit, Lifecycle) |
| `#/configuration` | Provider Administration | Storage providers + Email providers |
| `#/template-builder` | Enterprise Workspace | Create/complete items |
| `#/regulatory` | Regulatory Management | Workspace CRUD |
| `#/training` | Training Management | Workspace CRUD |
| `#/supplier-portal` | Supplier Portal | Workspace CRUD |
| `#/customer-portal` | Customer Portal | Workspace CRUD |
| `#/security` | Security Workspace | Workspace CRUD |

---

## 3. SuperAdmin Platform Center (13 tabs)

Executive · Tenants (+ create form) · Licencias · Módulos · Providers · Seguridad Global · Observability · Auditoría Global · Database · IA · Configuración Global · Backups · DevOps

**Acciones clave:** Crear tenant, lifecycle (trial/activate/suspend/archive/restore), export audit CSV.

---

## 4. Tenant Administration Center (15 tabs)

| Tab | Formularios / Acciones |
|---|---|
| General | Información empresarial, PUT general-information |
| Branding | Logo, colores, tema, favicon |
| Seguridad | MFA, session timeout, lockout, IP whitelist |
| Usuarios | CRUD usuarios, reset MFA, asignar roles |
| Roles & Permisos | RBAC management |
| Licenciamiento | Plan, seats, storage |
| Dominios | Upsert/disable domains |
| SSO | OIDC/SAML/LDAP config + test |
| API Keys | Create/rotate/revoke |
| Webhooks | Upsert/test/disable |
| Storage | Provider config |
| Notificaciones | SMTP provider config |
| Health & Backups | Health center, record backup |
| Auditoría | Timeline tenant-scoped |
| Estado | Trial/Activate/Suspend/Archive/Restore |

---

## 5. API — Grupos de endpoints (19)

1. **Auth** — login, MFA, refresh, logout, password  
2. **SuperAdmin Platform** — center, tenants search, audit export, lifecycle  
3. **Tenants** — 40+ endpoints administración  
4. **RBAC** — roles, permissions, assign, grant  
5. **MFA** — setup, enable, verify, disable  
6. **Audit** — search  
7. **Storage** — files upload/download, providers  
8. **Notifications** — templates, messages, providers, dashboard  
9. **Documents** — types, categories, versions, submit, decision, obsolete  
10. **Workflows** — definitions, instances, assignments, complete  
11. **Technical Sheets** — products, versions, ingredients, decision  
12. **Suppliers** — CRUD, validate, homologate, evaluations  
13. **Audit Management** — programs, checklists, findings, evidence, close  
14. **CAPA** — classify, 5-Why, Ishikawa, actions, effectiveness, closure  
15. **Risks** — assessments, treatments, controls, heat-map  
16. **Indicators** — formula, target, threshold, measurements  
17. **Reports** — seed, execute, export, schedules  
18. **Enterprise Workspaces** — generic items  
19. **Observability** — telemetry, dashboards, alerts  

---

## 6. Matriz RBAC por rol (navegación)

| Rol | Ve | No ve |
|---|---|---|
| Platform Administrator | superadmin-platform, tenant-administration | documents, capa, suppliers, risks |
| Tenant Administrator | dashboard, tenant-administration, audit-trail | superadmin, business modules |
| Tenant Security Admin | dashboard, security, audit-trail | tenant-administration, capa |
| Document Controller | dashboard, documents, audit-trail | superadmin, capa, suppliers |
| Quality Manager | dashboard, documents, capa, risks, indicators, audits, reports | superadmin, tenant-administration |
| Auditor | dashboard, audits, capa (read), documents, suppliers, reports | superadmin, risks, tenant-administration |
| Supplier Manager | dashboard, suppliers, supplier-portal, documents, reports | superadmin, capa, risks |
| CAPA Manager | dashboard, capa, risks, audits, reports | superadmin, documents, suppliers |
| Risk Manager | dashboard, risks, indicators, audits, reports | superadmin, documents, capa |
| Indicators Manager | dashboard, indicators, risks, reports | superadmin, documents, capa |
| Reporting Manager | dashboard, reports, indicators, risks, capa, audits | superadmin, tenant-administration |
| Storage Admin | dashboard, configuration (storage only) | superadmin, documents, tenant-administration |
| Notification Admin | dashboard, configuration (email only) | superadmin, documents, tenant-administration |
| Viewer | dashboard, all read modules | superadmin, security, tenant-administration |
| Support Operator | superadmin-platform | documents, business modules |

---

## 7. Procesos de negocio — Cobertura UI vs API

| Proceso | API | UI SPA | Certificación |
|---|---|---|---|
| Tenant onboarding | ✅ Full | ✅ TAC + SuperAdmin | E2E + Journey |
| Document create | ✅ | ✅ Form | E2E |
| Document approve (SoD) | ✅ | ❌ No UI | API E2E |
| Audit program create | ✅ | ✅ Form | E2E |
| Audit execution (findings) | ✅ | ❌ No UI | Journey API |
| CAPA create | ✅ | ✅ Form | E2E |
| CAPA 5-Why / Ishikawa | ✅ | ❌ No UI | Journey API |
| CAPA closure (SoD) | ✅ | ❌ No UI | API permission check |
| Risk create + heat map | ✅ | ✅ Form + dashboard | E2E |
| Indicator create | ✅ | ✅ Form | E2E |
| Report seed/execute | ✅ | ✅ UI buttons | E2E |
| Storage provider | ✅ | ✅ UI button | E2E |
| Email provider | ✅ | ✅ UI button | E2E |
| Enterprise workspace | ✅ | ✅ Form | E2E |
| File upload | ✅ | ❌ No UI | Journey API |

---

## 8. Elementos transversales

| Elemento | Estado | Notas |
|---|---|---|
| Loading / skeleton | ✅ | `loadingView`, spinners |
| Toast notifications | ✅ | success/error/info |
| Breadcrumbs | ✅ | Topbar |
| Theme toggle (dark mode) | ✅ Parcial | CSS `[data-theme=dark]`; strings inline |
| Responsive | ✅ Parcial | Breakpoints 1100px/820px |
| i18n ES/EN | ✅ Parcial | Framework + strings inline |
| Paginación | ✅ | Tablas módulos |
| Exportaciones | ✅ | CSV audit, report export |
| Tooltips | ❌ | No implementados |
| Modales / Wizards | ❌ | Flujos inline; sin `<dialog>` |
| Filtros / buscadores | ✅ | Global search, module filters |

---

## 9. Dependencias externas (PENDING posibles)

SMTP · SendGrid/Mailgun/Resend · Azure/AWS S3/MinIO · OIDC/SAML/LDAP · Firma digital · IA providers · Pasarela de pago

---

*Inventario funcional completo. Ningún módulo principal queda sin mapear.*
