# 01 — Role Discovery

## Alcance y metodología

Auditoría **solo de lectura** del modelo RBAC de Compliance 360. Evidencia obtenida de:

- `src/Compliance360.Domain/Identity/IdentityModels.cs` — entidades `User`, `Role`, `Permission`, enum `PermissionAction`
- `src/Compliance360.Web/Development/DevelopmentBootstrap.cs` — único seed de rol y catálogo de permisos (76 códigos)
- `src/Compliance360.Web/Security/PermissionPolicies.cs` — 74 políticas ASP.NET → claims `permission`
- `src/Compliance360.Application/Rbac/RbacService.cs` — autorización programática sin bypass SuperAdmin
- `src/Compliance360.Web/Api/ApiContext.cs` — aislamiento por `tenant_id` con excepción SuperAdmin
- `src/Compliance360.Web/Api/FoundationEndpoints.cs` — ~214 endpoints tenant/platform
- `src/Compliance360.Web/wwwroot/app.js` — gating visual por JWT (`routePermissions`, `canNavigate`, `canManageRoute`)
- `docs/e2e/*` y scripts `tools/e2e_*_visible.py` — roles operativos validados en tenant de prueba
- `TENANT_ADMIN_RESPONSIBILITY_MATRIX.md` — subroles recomendados (documentación, no código)

**Hallazgo arquitectónico central:** el código **no define roles de negocio como constantes**. Solo `SuperAdmin` se siembra en Development. Los demás roles son **cadenas dinámicas** creadas en runtime vía `/tenants/{tenantId}/rbac/roles`. La matriz de permisos por rol **no existe en código**; se infiere de documentación, E2E y diseño de APIs.

---

## Inventario de roles

| # | Rol | Origen en código | Tenant-scoped | System role | Evidencia E2E |
|---|-----|------------------|---------------|-------------|---------------|
| 1 | **SuperAdmin** | Seed `DevelopmentBootstrap.cs` L541–570; `IsSystemRole=true` | Sí (tenant bootstrap) | Sí | `01_SUPERADMIN_E2E_REPORT.md` |
| 2 | **Tenant Admin** | Runtime / recomendado en docs | Sí | No | `02_TENANT_ADMIN_E2E_REPORT.md` |
| 3 | **Document Controller** | Runtime / recomendado | Sí | No | `03_DOCUMENT_CONTROLLER_E2E_REPORT.md` |
| 4 | **Quality Manager** | Runtime / recomendado | Sí | No | `04_QUALITY_MANAGER_E2E_REPORT.md` |
| 5 | **Auditor** | Runtime / recomendado | Sí | No | `05_AUDITOR_E2E_REPORT.md` |
| 6 | **Supplier Manager** | Runtime / recomendado | Sí | No | `06_SUPPLIER_MANAGER_E2E_REPORT.md` |
| 7 | **CAPA Manager** | Runtime / recomendado | Sí | No | `07_CAPA_MANAGER_E2E_REPORT.md` |
| 8 | **Risk Manager** | Runtime / recomendado | Sí | No | `08_RISK_MANAGER_E2E_REPORT.md` |
| 9 | **Indicators Manager** | Runtime / recomendado | Sí | No | `09_INDICATORS_MANAGER_E2E_REPORT.md` |
| 10 | **Reporting Manager** | Runtime / recomendado | Sí | No | `10_REPORTING_MANAGER_E2E_REPORT.md` |
| 11 | **Storage Admin** | Runtime / recomendado | Sí | No | `11_STORAGE_ADMIN_E2E_REPORT.md` |
| 12 | **Notification Admin** | Runtime / recomendado | Sí | No | `12_NOTIFICATION_ADMIN_E2E_REPORT.md` |
| 13 | **Viewer** | Runtime / recomendado | Sí | No | `13_VIEWER_E2E_REPORT.md` |

Roles de prueba en unit tests (no productivos): `Auditor`, `Approver`, `Admin`, `Tenant Admin` — `tests/Compliance360.Tests/RbacFoundationTests.cs`, `IdentityServiceTests.cs`.

---

## Catálogo de permisos (76 códigos sembrados)

Fuente: `DevelopmentBootstrap.cs` L767–845.

| Módulo | Permisos |
|--------|----------|
| TENANT | READ, CREATE, UPDATE, STATUS, BRANDING, SECURITY, STORAGE, NOTIFICATIONS, INTEGRATIONS, BILLING, USERS, ROLES, AUDIT, DELETE, RESTORE, DOMAINS, SSO, WEBHOOKS, API_KEYS, HEALTH, BACKUP |
| IDENTITY | MANAGE |
| RBAC | MANAGE |
| AUDIT | READ, MANAGE |
| AUDITMANAGEMENT | MANAGE |
| CAPA | MANAGE, READ, APPROVE, CLOSE |
| RISK | MANAGE, READ, APPROVE, CLOSE |
| INDICATOR | MANAGE, READ, APPROVE, EXPORT |
| REPORT | MANAGE, READ, EXECUTE, EXPORT, SCHEDULE |
| STORAGE | MANAGE |
| NOTIFICATION | MANAGE, SEND, READ, TEMPLATE, ADMIN |
| DOCUMENT | MANAGE |
| WORKFLOW | MANAGE |
| TECHNICALSHEET | MANAGE |
| SUPPLIER | MANAGE |
| OBSERVABILITY | READ, MANAGE, ADMIN |
| SUPERADMIN | DASHBOARD.READ, TENANTS.READ/CREATE/UPDATE/STATUS/DELETE, LICENSES.MANAGE, MODULES.MANAGE, PROVIDERS.MANAGE/HEALTH, SECURITY.MANAGE, OBSERVABILITY.READ, AUDIT.READ/EXPORT, DATABASE.READ, AI.MANAGE, CONFIGURATION.MANAGE, BACKUPS.READ, DEVOPS.READ, SEARCH |

**Permisos referenciados en frontend pero NO sembrados:** `DOCUMENT.READ`, `TECHNICALSHEET.READ`, `SUPPLIER.READ`, `SUPERADMIN.DASHBOARD` (`app.js` L145–166).

---

## Mecanismo de autorización

### JWT claims (`SecurityServices.cs`)

- `ClaimTypes.Role` — nombres de rol (ej. `SuperAdmin`)
- `permission` — códigos `MODULE.ACTION` (múltiples claims)
- `tenant_id` — tenant del usuario

### Políticas (`PermissionPolicies.cs`)

- Cada endpoint usa `.RequireAuthorization(PermissionPolicies.*)`
- `HasPlatformSuperAdmin`: rol `SuperAdmin` **satisface cualquier política** sin revisar claims `permission` (L160–181)
- Políticas granulares con escalación: ej. `Capa.Read` acepta `CAPA.READ` **o** `CAPA.MANAGE`

### Servicio RBAC (`RbacService.AuthorizeAsync`)

- Verifica `permission` exacto; **sin bypass SuperAdmin**
- Audita denegaciones (`PermissionDenied`)

### Tenant context (`ApiContext.TenantId`)

- Usuario normal: `tenant_id` claim debe coincidir con `{tenantId}` de ruta
- SuperAdmin: puede operar cualquier `{tenantId}` (L28–32)

---

## Fichas técnicas por rol

### 1. SuperAdmin

| Campo | Valor |
|-------|-------|
| **Nombre** | `SuperAdmin` |
| **Descripción** | Administrador de plataforma con bypass global de políticas y acceso cross-tenant |
| **Objetivo** | Gestionar tenants, licencias, observabilidad global y configuración de plataforma |
| **Responsabilidades** | CRUD tenants, estado/suspensión, auditoría global, platform center, RBAC bootstrap |
| **Alcance** | Control plane; puede leer/escribir en cualquier `tenantId` |
| **Restricciones** | Ninguna a nivel policy; limitado solo por implementación de endpoints |
| **Nivel de acceso** | Máximo (bypass + 76 permisos en seed) |
| **Módulos permitidos** | Todos los módulos tenant + SUPERADMIN.* + observabilidad |
| **Módulos restringidos** | Ninguno por RBAC |
| **Operaciones permitidas** | Todas las expuestas en API (~228 endpoints autenticados) |
| **Operaciones prohibidas** | Ninguna por diseño actual (riesgo arquitectónico) |
| **Permisos** | Los 76 de `DefaultPermissions` |
| **Policies** | Todas — bypass por rol |
| **Claims** | `role=SuperAdmin`, `permission=*`, `tenant_id` del tenant bootstrap |
| **Endpoints** | `/api/v1/superadmin/platform-center/*`, `/api/v1/tenants` (global), cualquier `/tenants/{id}/*` |
| **Pantallas** | `superadmin-platform`, `tenant-administration`, todos los módulos operativos |
| **Menús visibles** | Todos (bypass efectivo vía permisos completos; menú filtra por `SUPERADMIN.DASHBOARD` en frontend — mismatch) |
| **Dashboards** | Platform Center + Executive Dashboard con todas las métricas |
| **Acciones críticas** | Crear/suspender tenant, exportar auditoría global, asignar RBAC |
| **Acciones auditadas** | Sí — `AuditLog` en operaciones administrativas |
| **Dependencias** | Bootstrap Development, PostgreSQL |
| **Interacción** | Provisiona tenants y roles; Tenant Admin opera dentro del tenant |
| **Flujos principales** | Login → Platform Center → Crear tenant → TAC → RBAC → Auditoría global |
| **Frecuencia** | Baja (operaciones de plataforma) |

---

### 2. Tenant Admin

| Campo | Valor |
|-------|-------|
| **Nombre** | `Tenant Admin` / `Tenant Admin E2E` |
| **Descripción** | Administrador interno del tenant: usuarios, roles, seguridad, configuración |
| **Objetivo** | Operar y configurar el tenant sin acceso a otros tenants ni plataforma global |
| **Responsabilidades** | Empresa, usuarios, roles, MFA, branding, dominios, SSO, API keys, webhooks, auditoría tenant |
| **Alcance** | Un solo `tenant_id` |
| **Restricciones** | Sin `SUPERADMIN.*`; sin otro tenant |
| **Nivel de acceso** | Alto dentro del tenant |
| **Módulos permitidos** | TAC completo, enterprise workspaces, audit-trail |
| **Módulos restringidos** | SuperAdmin Platform, datos de otros tenants |
| **Operaciones permitidas** | CRUD usuarios/roles, configuración tenant, RBAC grant (catálogo global) |
| **Operaciones prohibidas** | Crear tenant, suspender tenant a nivel plataforma, auditoría global |
| **Permisos E2E** | `TENANT.READ/UPDATE/SECURITY/USERS/ROLES/STORAGE/NOTIFICATIONS/HEALTH`, `RBAC.MANAGE`, `IDENTITY.MANAGE`, `AUDIT.READ`, `RISK.READ`, `CAPA.READ`, `AUDITMANAGEMENT.MANAGE`, `REPORT.READ/EXECUTE`, `INDICATOR.READ` |
| **Policies** | `Tenant.*`, `Rbac.Manage`, `Identity.Manage`, `Tenant.Audit` |
| **Pantallas** | `tenant-administration`, `dashboard`, `audit-trail` |
| **Menús** | TAC si `TENANT.USERS` \| `TENANT.ROLES` \| `TENANT.UPDATE`; dashboard si `TENANT.READ` |
| **Acciones críticas** | Crear usuarios, asignar roles, reset MFA, cambiar seguridad |
| **Acciones auditadas** | Sí |
| **Interacción** | Crea roles para Document Controller, Quality Manager, etc. |
| **Flujos** | Login → Dashboard → TAC → Usuarios/RBAC/Seguridad → Auditoría |
| **Frecuencia** | Media-alta |

---

### 3. Document Controller

| Campo | Valor |
|-------|-------|
| **Objetivo** | Control documental: tipos, categorías, documentos, versiones |
| **Permisos E2E** | `TENANT.READ`, `DOCUMENT.MANAGE`, `WORKFLOW.MANAGE`, `AUDIT.READ`, `RISK.READ`, `CAPA.READ`, `AUDITMANAGEMENT.MANAGE`, `REPORT.READ/EXECUTE`, `INDICATOR.READ` |
| **Módulos** | Document Management, Audit Trail, Dashboard (métricas según permisos) |
| **Operaciones** | Crear tipos/categorías/documentos; buscar; **no** aprobar documentos (requiere `decision` con mismo `DOCUMENT.MANAGE`) |
| **Restricciones** | Sin TAC, sin proveedores, sin configuración global |
| **Pantallas** | `documents`, `dashboard`, `audit-trail` |
| **Menú** | `documents` si `DOCUMENT.READ` \| `DOCUMENT.MANAGE` |
| **Flujo E2E** | Login → Dashboard → Documents → Crear documento → Buscar → Audit Trail → Logout |
| **Frecuencia** | Alta (operación diaria) |

---

### 4. Quality Manager

| Campo | Valor |
|-------|-------|
| **Objetivo** | Gobernanza de calidad: documentos, CAPA, riesgos, indicadores, reportes |
| **Permisos E2E** | `DOCUMENT.MANAGE`, `WORKFLOW.MANAGE`, `CAPA.*` (4), `RISK.*` (4), `INDICATOR.*` (4), `REPORT.READ/EXECUTE/EXPORT`, `AUDIT.READ`, `AUDITMANAGEMENT.MANAGE`, `TENANT.READ` |
| **Módulos** | Documents, CAPA, Risks, Indicators, Reports, Audit Trail |
| **Operaciones** | Crear y aprobar/cerrar CAPA, riesgos, indicadores; ejecutar reportes |
| **Restricciones** | Sin administración de tenant; permisos amplios en múltiples módulos (solapamiento con roles especializados) |
| **Flujo E2E** | Login → Dashboard → Documento → CAPA → Riesgo → Indicador → Reportes → Trazabilidad |
| **Frecuencia** | Alta |

---

### 5. Auditor

| Campo | Valor |
|-------|-------|
| **Objetivo** | Planificar y ejecutar auditorías; registrar hallazgos; originar CAPA |
| **Permisos E2E** | `TENANT.READ`, `AUDIT.READ`, `AUDIT.MANAGE`, `AUDITMANAGEMENT.MANAGE`, `CAPA.MANAGE`, `CAPA.READ`, `RISK.READ`, `INDICATOR.READ`, `REPORT.READ/EXECUTE` |
| **Módulos** | Audit Management, CAPA, Audit Trail |
| **Operaciones** | Programas, planes, checklist, hallazgos, evidencia, CAPA desde hallazgo |
| **Restricciones** | Sin gestión de proveedores/documentos (salvo lectura dashboard) |
| **Nota** | Frontend exige `AUDITMANAGEMENT.MANAGE` para ruta `audits` — no existe `AUDITMANAGEMENT.READ` |
| **Flujo E2E** | Login → Audit Management → Crear auditoría → CAPA hallazgo → Audit Trail |
| **Frecuencia** | Media (ciclos de auditoría) |

---

### 6. Supplier Manager

| Campo | Valor |
|-------|-------|
| **Objetivo** | Gestión de proveedores y expediente regulatorio |
| **Permisos E2E (inicial + fix)** | `TENANT.READ`, `SUPPLIER.MANAGE`, `AUDIT.READ`, `RISK.READ`, `CAPA.READ`, `AUDITMANAGEMENT.MANAGE`, `REPORT.READ/EXECUTE`, `INDICATOR.READ` + **`CAPA.MANAGE`** (añadido en E2E por 403) |
| **Módulos** | Supplier Management, CAPA (asociada), Audit Trail |
| **Operaciones** | CRUD proveedor, documentos, evaluación, homologación, suspensión |
| **Solapamiento** | CAPA.MANAGE fuera de alcance nominal del rol |
| **Flujo E2E** | Login → Suppliers → Crear proveedor → CAPA → Audit Trail |
| **Frecuencia** | Media |

---

### 7. CAPA Manager

| Campo | Valor |
|-------|-------|
| **Objetivo** | Ciclo de vida CAPA: causa raíz, acciones, efectividad, cierre |
| **Permisos E2E (+ fix)** | `TENANT.READ`, `CAPA.MANAGE/READ`, `AUDIT.READ`, `RISK.READ`, `INDICATOR.READ`, `REPORT.READ/EXECUTE` + **`AUDITMANAGEMENT.MANAGE`** (dashboard 403) |
| **Módulos** | CAPA, Risks, Indicators, Reports, Audit Trail |
| **Operaciones** | Clasificar, 5 Why, Ishikawa, acciones, evidencia, cierre |
| **Flujo E2E** | Login → CAPA → Riesgos → Indicadores → Audit Trail |
| **Frecuencia** | Alta en entornos con NC frecuentes |

---

### 8. Risk Manager

| Campo | Valor |
|-------|-------|
| **Objetivo** | Identificación, evaluación, tratamiento y cierre de riesgos |
| **Permisos E2E** | `TENANT.READ`, `RISK.MANAGE/READ`, `CAPA.READ`, `INDICATOR.READ`, `AUDIT.READ`, `AUDITMANAGEMENT.MANAGE`, `REPORT.READ/EXECUTE` |
| **Módulos** | Risk Management, Reports, Audit Trail |
| **Operaciones** | Matriz, evaluaciones, tratamientos (`RISK.APPROVE`), controles, heat map |
| **Flujo E2E** | Login → Riesgo → CAPA → Reportes → Trazabilidad |
| **Frecuencia** | Media |

---

### 9. Indicators Manager

| Campo | Valor |
|-------|-------|
| **Objetivo** | KPIs de calidad: definición, mediciones, umbrales, tendencias |
| **Permisos E2E** | `TENANT.READ`, `INDICATOR.MANAGE/READ`, `RISK.READ`, `CAPA.READ`, `AUDIT.READ`, `AUDITMANAGEMENT.MANAGE`, `REPORT.READ/EXECUTE` |
| **Módulos** | Quality Indicators, Reports |
| **Operaciones** | Categorías, fórmulas, metas, mediciones, export |
| **Flujo E2E** | Login → Indicador → Reportes → Riesgos → Audit Trail |
| **Frecuencia** | Media-alta |

---

### 10. Reporting Manager

| Campo | Valor |
|-------|-------|
| **Objetivo** | Report Center: ejecutar, exportar, programar reportes |
| **Permisos E2E** | `TENANT.READ`, `REPORT.READ/EXECUTE`, `INDICATOR.READ`, `RISK.READ`, `CAPA.READ`, `AUDIT.READ`, `AUDITMANAGEMENT.MANAGE` |
| **Módulos** | Reports, Dashboard datasets |
| **Operaciones** | Execute, export, schedule (si `REPORT.SCHEDULE` otorgado) |
| **Restricciones** | Sin `REPORT.MANAGE` en E2E — no diseña definiciones de reporte |
| **Flujo E2E** | Login → Report Center → Indicadores → Riesgos → Audit Trail |
| **Frecuencia** | Media |

---

### 11. Storage Admin

| Campo | Valor |
|-------|-------|
| **Objetivo** | Proveedores de almacenamiento del tenant |
| **Permisos E2E (+ dashboard)** | `TENANT.READ`, `TENANT.STORAGE`, `STORAGE.MANAGE`, `AUDIT.READ`, `AUDITMANAGEMENT.MANAGE`, `CAPA.READ`, `RISK.READ`, `INDICATOR.READ`, `REPORT.READ/EXECUTE`, `NOTIFICATION.READ` |
| **Módulos** | Configuration (storage providers) |
| **Operaciones** | Crear provider, test, activar, upload/download |
| **Flujo E2E** | Login → Configuration → Crear storage local → Test → Audit Trail |
| **Frecuencia** | Baja (configuración) |

---

### 12. Notification Admin

| Campo | Valor |
|-------|-------|
| **Objetivo** | SMTP, plantillas, envío y tracking de notificaciones |
| **Permisos E2E** | `TENANT.READ`, `TENANT.NOTIFICATIONS`, `NOTIFICATION.*` (5), `STORAGE.MANAGE`, lecturas transversales dashboard |
| **Módulos** | Configuration (email providers), notifications API |
| **Solapamiento** | `STORAGE.MANAGE` — comparte pantalla Configuration con Storage Admin |
| **Flujo E2E** | Login → Configuration → SMTP → Audit Trail |
| **Frecuencia** | Baja |

---

### 13. Viewer

| Campo | Valor |
|-------|-------|
| **Objetivo** | Consulta transversal sin mutación |
| **Permisos E2E (final)** | `TENANT.READ`, `CAPA.READ`, `RISK.READ`, `INDICATOR.READ`, `REPORT.READ/EXECUTE`, `AUDIT.READ`, `NOTIFICATION.READ` |
| **Módulos** | Reports, CAPA, Risks, Indicators, Audit Trail (solo lectura) |
| **Operaciones prohibidas** | Cualquier `*.MANAGE`, crear/editar/aprobar |
| **Restricciones** | Sin documents/suppliers/configuration/TAC/superadmin en menú |
| **Frontend** | `readOnlyNotice` si `!canManageRoute`; filtrado menú post-fix E2E |
| **Flujo E2E** | Login → Menú filtrado → Consultar módulos → Modo solo lectura → Logout |
| **Frecuencia** | Alta (usuarios de consulta) |

---

## Rutas frontend y permisos (`app.js` L145–185)

| Ruta | Permiso navegación | Permiso gestión |
|------|-------------------|-----------------|
| dashboard | TENANT.READ | — |
| reports | REPORT.READ \| EXECUTE | — |
| audit-trail | AUDIT.READ | — |
| documents | DOCUMENT.READ \| MANAGE | DOCUMENT.MANAGE |
| technical-sheets | TECHNICALSHEET.READ \| MANAGE | TECHNICALSHEET.MANAGE |
| suppliers | SUPPLIER.READ \| MANAGE | SUPPLIER.MANAGE |
| audits | AUDITMANAGEMENT.MANAGE | AUDITMANAGEMENT.MANAGE |
| capa | CAPA.READ \| MANAGE | CAPA.MANAGE |
| risks | RISK.READ \| MANAGE | RISK.MANAGE |
| indicators | INDICATOR.READ \| MANAGE | INDICATOR.MANAGE |
| superadmin-platform | SUPERADMIN.DASHBOARD | — |
| tenant-administration | TENANT.USERS \| ROLES \| UPDATE | — |
| configuration | STORAGE \| NOTIFICATION.MANAGE \| ADMIN | STORAGE.MANAGE |

---

## Conclusión del discovery

1. **No hay modelo RBAC cerrado en código** — solo catálogo de permisos y un rol sistema.
2. **Los 13 roles operativos son convención** documentada y validada en E2E, no enums.
3. **SuperAdmin rompe el modelo** con bypass de políticas y cross-tenant.
4. **Frontend y backend divergen** en códigos de permiso y granularidad READ/MANAGE.
5. **E2E reveló permisos transversales ad hoc** (`AUDITMANAGEMENT.MANAGE`, `CAPA.MANAGE`) para que el dashboard no devuelva 403.
