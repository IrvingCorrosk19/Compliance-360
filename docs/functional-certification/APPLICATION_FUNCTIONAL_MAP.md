# APPLICATION FUNCTIONAL MAP (FASE 1)

**Source of truth:** `wwwroot/app.js` (navigation, routes, RBAC), `Api/FoundationEndpoints.cs` (229 endpoints), RBAC catalog.
**API surface:** 176 POST · 38 GET · 10 PUT · 5 DELETE = **229 endpoints**.

---

## 1. Navegación (menús / grupos)

| Grupo | Ruta | Pantalla | Permiso requerido (routePermissions) |
|---|---|---|---|
| Command Center | `dashboard` | Executive Dashboard | TENANT.READ |
| Command Center | `compliance` | Compliance Dashboard | TENANT.READ |
| Command Center | `reports` | Report Center | REPORT.READ/EXECUTE/MANAGE |
| Command Center | `audit-trail` | Audit Trail | AUDIT.READ, TENANT.AUDIT |
| Operations | `superadmin-platform` | SuperAdmin Platform | PLATFORM.DASHBOARD.READ |
| Operations | `documents` | Document Management | DOCUMENT.READ/CREATE/UPDATE/APPROVE |
| Operations | `technical-sheets` | Technical Sheets | TECHNICALSHEET.* |
| Operations | `suppliers` | Supplier Management | SUPPLIER.* |
| Operations | `audits` | Audit Management | AUDITMANAGEMENT.READ/MANAGE |
| Operations | `capa` | CAPA | CAPA.READ/MANAGE/APPROVE |
| Operations | `risks` | Risk Management | RISK.READ/MANAGE/APPROVE |
| Operations | `indicators` | Quality Indicators | INDICATOR.READ/MANAGE |
| Enterprise | `tenant-administration` | Tenant Administration | TENANT.USERS/ROLES/UPDATE |
| Enterprise | `template-builder` | Template Builder | TENANT.UPDATE |
| Enterprise | `regulatory` | Regulatory Management | TENANT.READ |
| Enterprise | `training` | Training Management | TENANT.READ |
| Enterprise | `supplier-portal` | Supplier Portal | SUPPLIER.* |
| Enterprise | `customer-portal` | Customer Portal | TENANT.READ |
| Enterprise | `security` | Security | TENANT.SECURITY |
| Enterprise | `configuration` | Configuration | TENANT.STORAGE, STORAGE.READ, TENANT.NOTIFICATIONS, NOTIFICATION.* |

**Manage affordances (write/create buttons)** gated by `routeManagePermissions` for: documents, technical-sheets, suppliers, audits, capa, risks, indicators, configuration.

---

## 2. Módulos operativos (CRUD + tabla + acción de creación)

| Módulo | Endpoint listado | Columnas tabla | Acción de creación |
|---|---|---|---|
| documents | `/tenants/{t}/documents` | title, code, status, expiresAtUtc | createDocumentFoundation |
| technical-sheets | `/tenants/{t}/technical-sheets` | title, code, status, productName | createTechnicalSheetFoundation |
| suppliers | `/tenants/{t}/suppliers` | legalName, taxIdentifier, status, score | createSupplier |
| audits | `/tenants/{t}/audit-management` | title, code, status, type | createAuditFoundation |
| capa | `/tenants/{t}/capas` | title, code, status, priority | createCapa |
| risks | `/tenants/{t}/risks` | title, code, status, residualLevel | createRiskFoundation |
| indicators | `/tenants/{t}/indicators` | name, code, status, type | createIndicatorFoundation |

## 3. Enterprise workspaces (type-based CRUD)

`template-builder`(0), `regulatory`(1), `training`(2), `supplier-portal`(3), `customer-portal`(4), `security`(5), `configuration`(6) — endpoint `/tenants/{t}/enterprise-workspaces`, gated by TENANT.MANAGE.

---

## 4. Dashboards

| Dashboard | Fuente | Permiso |
|---|---|---|
| Executive Dashboard | agregados tenant | TENANT.READ |
| Compliance Dashboard | agregados compliance | TENANT.READ |
| CAPA dashboard | `/capas/dashboard` | CAPA.READ |
| Risk dashboard + heat-map | `/risks/dashboard`, `/risks/heat-map` | RISK.READ |
| Indicators dashboard + trends | `/indicators/dashboard`, `/indicators/trends` | INDICATOR.READ |
| Notifications dashboard | `/notifications/dashboard` | NOTIFICATION.READ |
| Tenant Administration dashboard | `/tenants/{t}/administration-dashboard` | TENANT.* |
| SuperAdmin Platform Center | `/superadmin/platform-center` | PLATFORM.DASHBOARD.READ |

---

## 5. Workflows / ciclos de vida (transiciones clave)

- **Document:** Draft → (version) → InReview (submit) → Approved/Rejected (decision) → Obsolete/Archived.
- **CAPA:** Draft → Open → InProgress (classify/root-cause/actions) → complete action → verify effectiveness → PendingApproval → Closed → Reopened.
- **Risk:** Draft → classify → assess (inherent+residual) → treatment → close → reopen.
- **Audit:** program → checklist → plan → audit → assign checklist → schedule → start → finding/evidence → complete → close → reopen.
- **Indicator:** create → activate → formula → target → threshold → period → measurement → result → approve.
- **Report:** seed → create category/definition → template/parameter → activate → execute → complete → export → schedule → subscribe.
- **Supplier:** create → add document → validate/reject → evaluate → homologate → suspend.
- **Tenant lifecycle (platform-center):** create → trial → activate → suspend → archive → restore.

---

## 6. Formularios / controles clave

- **Login:** tenantId, email, password (+ MFA challenge).
- **Change password:** current, new.
- **Create Tenant:** name, slug, legalName, commercialName, taxIdentifier, country, currency, adminEmail/FullName/Password.
- **General information (PUT):** 15 campos + changeReason.
- **Branding (PUT):** displayName, colors, theme(System/Light/Dark), corporateEmail, footer.
- **Security (PUT):** MFA, session timeout, password expiration, lockout, IP whitelist, trusted devices, score.
- **Storage provider:** kind, name, container, priority, isDefault, isEnabled, settingsJson.
- **Notification provider:** provider, name, priority, isDefault, isEnabled.
- **Module creation forms:** `#module-action-form` (code, name/title) per operational module.
- **Enterprise forms:** `#enterprise-action-form` (code, title).
- **Filters/search/pagination:** each list endpoint accepts searchText, status filters, page, pageSize.

---

## 7. RBAC — matriz rol → nº permisos (tenant ops `ddcaf211…`, evidencia SQL)

| Rol | Permisos |
|---|---|
| Quality Manager | 19 |
| Tenant Administrator | 13 |
| Reporting Manager | 11 |
| Viewer | 11 (todos READ) |
| Auditor | 8 |
| Document Controller | 8 |
| Notification Administrator | 8 |
| Supplier Manager | 8 |
| Tenant Security Administrator | 8 |
| CAPA Manager | 7 |
| Indicators Manager | 7 |
| Risk Manager | 7 |
| Storage Administrator | 7 |

Platform roles (Platform Administrator, Platform Operations, Platform Security, Support Operator) viven en el tenant plataforma.

---

## 8. Segregation of Duties (invariantes a verificar)

- Document Controller crea documentos pero **no aprueba** (sin DOCUMENT.APPROVE).
- Quality Manager aprueba documentos/CAPA/riesgos pero **no crea** datos de negocio base.
- Storage Administrator y Notification Administrator son **mutuamente excluyentes**.
- Risk Manager gestiona pero **no aprueba/cierra** (RISK.APPROVE/CLOSE en Quality Manager).
- Viewer es **solo lectura** en todos los módulos.
- Platform Administrator **no opera datos de negocio** de un tenant; cross-tenant solo vía Support break-glass.

---

*FASE 1 completada. Cobertura mapeada: 20 rutas, 7 módulos CRUD, 7 enterprise workspaces, 8 dashboards, 8 workflows, 229 endpoints, 13 roles tenant + 4 plataforma.*
