# 03 — Permission Matrix

## Convenciones

| Símbolo | Significado |
|---------|-------------|
| **C** | Crear |
| **R** | Consultar / leer |
| **U** | Editar / actualizar |
| **D** | Eliminar |
| **A** | Aprobar |
| **X** | Exportar |
| **Adm** | Administrar (configuración del módulo) |
| **Cfg** | Configurar integraciones/providers |
| **—** | Sin acceso |
| **\*** | Acceso total vía `*.MANAGE` o bypass SuperAdmin |
| **(E2E)** | Permiso otorgado en preparación E2E del tenant de prueba |

Fuente API: `PermissionPolicies.cs` + `FoundationEndpoints.cs`. Fuente UI: `app.js` L145–185.

---

## Matriz por módulo y rol

### Plataforma y administración

| Módulo | SuperAdmin | Tenant Admin | Quality Mgr | Doc Controller | Auditor | Supplier Mgr | CAPA Mgr | Risk Mgr | Indicators Mgr | Reporting Mgr | Storage Admin | Notification Admin | Viewer |
|--------|:----------:|:------------:|:-----------:|:--------------:|:-------:|:------------:|:--------:|:--------:|:--------------:|:-------------:|:-------------:|:------------------:|:------:|
| **Tenants (plataforma)** | C R U D Adm | — | — | — | — | — | — | — | — | — | — | — | — |
| **TAC / config tenant** | * | C R U Adm | — | — | — | — | — | — | — | — | — | — | — |
| **Usuarios** | * | C R U D | — | — | — | — | — | — | — | — | — | — | — |
| **RBAC** | * | Adm | — | — | — | — | — | — | — | — | — | — | — |
| **Branding** | * | U | — | — | — | — | — | — | — | — | — | — | — |
| **Seguridad tenant** | * | Cfg | — | — | — | — | — | — | — | — | — | — | — |
| **Dominios/SSO/Keys** | * | Cfg | — | — | — | — | — | — | — | — | — | — | — |
| **SuperAdmin Platform** | R Adm | — | — | — | — | — | — | — | — | — | — | — | — |
| **Observabilidad** | R | — | — | — | — | — | — | — | — | — | — | — | — |

### Operación de calidad

| Módulo | SuperAdmin | Tenant Admin | Quality Mgr | Doc Controller | Auditor | Supplier Mgr | CAPA Mgr | Risk Mgr | Indicators Mgr | Reporting Mgr | Storage Admin | Notification Admin | Viewer |
|--------|:----------:|:------------:|:-----------:|:--------------:|:-------:|:------------:|:--------:|:--------:|:--------------:|:-------------:|:-------------:|:------------------:|:------:|
| **Dashboard** | R | R (E2E) | R | R | R | R | R | R | R | R | R | R | R |
| **Documents** | * | — | * (E2E) | * (E2E) | — | — | — | — | — | — | — | — | —¹ |
| **Workflows** | * | — | * (E2E) | * (E2E) | — | — | — | — | — | — | — | — | — |
| **Technical Sheets** | * | — | —² | — | — | — | — | — | — | — | — | — | —¹ |
| **Suppliers** | * | — | — | — | — | * (E2E) | — | — | — | — | — | — | —¹ |
| **Audit Management** | * | R³ | R³ | R³ | * (E2E) | R³ | R³ | R³ | R³ | R³ | R³ | R³ | — |
| **CAPA** | * | R | * (E2E) | R | * (E2E) | *⁴ | * (E2E) | R | R | R | R | R | R (E2E) |
| **Risks** | * | R | * (E2E) | R | R | R | R | * (E2E) | R | R | R | R | R (E2E) |
| **Indicators** | * | R | * (E2E) | R | R | R | R | R | * (E2E) | R | R | R | R (E2E) |
| **Reports** | * | R X | R X (E2E) | R | R | R | R | R | R | R X (E2E) | R | R | R (E2E) |
| **Audit Trail** | R | R (E2E) | R (E2E) | R (E2E) | R (E2E) | R (E2E) | R (E2E) | R (E2E) | R (E2E) | R (E2E) | R (E2E) | R (E2E) | R (E2E) |

### Infraestructura

| Módulo | SuperAdmin | Tenant Admin | Quality Mgr | Doc Controller | Auditor | Supplier Mgr | CAPA Mgr | Risk Mgr | Indicators Mgr | Reporting Mgr | Storage Admin | Notification Admin | Viewer |
|--------|:----------:|:------------:|:-----------:|:--------------:|:-------:|:------------:|:--------:|:--------:|:--------------:|:-------------:|:-------------:|:------------------:|:------:|
| **Storage** | * | Cfg | — | — | — | — | — | — | — | — | * (E2E) | *⁵ (E2E) | — |
| **Notifications** | * | Cfg | — | — | — | — | — | — | — | — | — | * (E2E) | R (E2E) |
| **Enterprise Workspaces** | * | R⁶ | R⁶ | R⁶ | R⁶ | R⁶ | R⁶ | R⁶ | R⁶ | R⁶ | R⁶ | R⁶ | — |

¹ Viewer: frontend espera `DOCUMENT.READ` / `SUPPLIER.READ` — **no existen en catálogo**; menú oculto correctamente.  
² Quality Manager no tiene `TECHNICALSHEET.MANAGE` en E2E.  
³ `AUDITMANAGEMENT.MANAGE` otorgado ad hoc para métricas dashboard — no es rol de auditoría.  
⁴ Supplier Manager recibió `CAPA.MANAGE` en fix E2E.  
⁵ Notification Admin con `STORAGE.MANAGE` comparte Configuration.  
⁶ Cualquier usuario con `TENANT.READ` accede a enterprise workspaces con formulario de creación **sin gate** (`app.js` L2303–2316).

---

## Matriz permiso → policy → endpoint (referencia)

| Permiso | Policy ASP.NET | Endpoints representativos |
|---------|----------------|---------------------------|
| TENANT.READ | Tenant.Read | GET `/tenants`, GET `/tenants/{id}` |
| TENANT.CREATE | Tenant.Create | POST `/tenants` |
| TENANT.USERS | Tenant.Users | POST `/tenants/{id}/users` |
| RBAC.MANAGE | Rbac.Manage | `/tenants/{id}/rbac/*` |
| DOCUMENT.MANAGE | Document.Manage | Todo el grupo `/documents` (C R U — sin DELETE dedicado) |
| AUDITMANAGEMENT.MANAGE | AuditManagement.Manage | Todo `/audit-management` incl. dashboard |
| CAPA.READ | Capa.Read | GET `/capas`, dashboard, export |
| CAPA.MANAGE | Capa.Manage | POST mutaciones CAPA |
| CAPA.APPROVE | Capa.Approve | POST approvers |
| CAPA.CLOSE | Capa.Close | POST approve-closure |
| RISK.APPROVE | Risk.Approve | treatments, escalate-critical |
| REPORT.EXECUTE | Report.Execute | POST execute/complete |
| STORAGE.MANAGE | Storage.Manage | providers + files |
| NOTIFICATION.ADMIN | Notification.Admin | POST providers |
| SUPERADMIN.DASHBOARD.READ | SuperAdmin.Dashboard | GET platform-center |

---

## Permisos del catálogo sin uso claro en UI

| Permiso | Sembrado | Policy | UI/Endpoint |
|---------|----------|--------|-------------|
| TENANT.INTEGRATIONS | Sí | Tenant.Integrations | Sin ruta dedicada en `app.js` |
| TENANT.DELETE / RESTORE | Sí | Tenant.Delete/Restore | TAC lifecycle |
| SUPERADMIN.LICENSES.MANAGE | Sí | SuperAdmin.Licenses | Tab UI; sin API wired |
| SUPERADMIN.MODULES.MANAGE | Sí | SuperAdmin.Modules | Tab UI; sin API wired |
| SUPERADMIN.AI.MANAGE | Sí | SuperAdmin.Ai | Tab UI; sin API wired |
| DOCUMENT.READ | **No** | — | Referenciado en `app.js` |
| TECHNICALSHEET.READ | **No** | — | Referenciado en `app.js` |
| SUPPLIER.READ | **No** | — | Referenciado en `app.js` |

---

## Matriz de acciones CRUD por permiso `*.MANAGE`

El diseño actual concentra **todas** las operaciones en un solo permiso:

| Módulo | C | R | U | D | A | X | Con permiso MANAGE |
|--------|---|---|---|---|---|---|-------------------|
| DOCUMENT | ✓ | ✓ | ✓ | ✓¹ | ✓² | — | Todo |
| SUPPLIER | ✓ | ✓ | ✓ | — | ✓³ | — | Todo |
| CAPA | ✓ | ✓³ | ✓ | — | ✓⁴ | ✓³ | MANAGE + APPROVE + CLOSE separados |
| RISK | ✓ | ✓³ | ✓ | — | ✓⁴ | ✓³ | Parcialmente granular |
| INDICATOR | ✓ | ✓³ | ✓ | — | ✓⁴ | ✓⁵ | Parcialmente granular |
| REPORT | ✓ | ✓ | ✓ | — | — | ✓ | EXECUTE/EXPORT/SCHEDULE separados |
| STORAGE | ✓ | ✓ | ✓ | ✓ | — | — | Monolítico |
| NOTIFICATION | ✓ | ✓ | ✓ | — | — | — | SEND/TEMPLATE/ADMIN separados |

¹ Obsolete endpoint, no delete literal.  
² `decision` endpoint con mismo permiso que creación.  
³ Permisos READ/EXPORT separados existen para CAPA/RISK/INDICATOR.  
⁴ APPROVE/CLOSE separados.  
⁵ INDICATOR.EXPORT.

---

## Permisos efectivos SuperAdmin

| Mecanismo | Efecto |
|-----------|--------|
| 76 claims `permission` en JWT | Acceso nominal a todo catálogo |
| `HasPlatformSuperAdmin` | Bypass de **todas** las policies aunque falte un claim |
| `ApiContext` bypass | Cualquier `{tenantId}` en URL |

**Resultado:** matriz irrelevante para SuperAdmin — siempre **\***.

---

## Permisos E2E por rol (evidencia runtime)

| Rol | Permisos otorgados |
|-----|-------------------|
| SuperAdmin | 76 (bootstrap) |
| Tenant Admin E2E | TENANT.READ/UPDATE/SECURITY/USERS/ROLES/STORAGE/NOTIFICATIONS/HEALTH, RBAC.MANAGE, IDENTITY.MANAGE, AUDIT.READ, RISK.READ, CAPA.READ, AUDITMANAGEMENT.MANAGE, REPORT.READ/EXECUTE, INDICATOR.READ |
| Document Controller E2E | TENANT.READ, DOCUMENT.MANAGE, WORKFLOW.MANAGE, AUDIT.READ, RISK.READ, CAPA.READ, AUDITMANAGEMENT.MANAGE, REPORT.READ/EXECUTE, INDICATOR.READ |
| Quality Manager E2E | TENANT.READ, DOCUMENT.MANAGE, WORKFLOW.MANAGE, CAPA.* (4), RISK.* (4), INDICATOR.* (4), REPORT.READ/EXECUTE/EXPORT, AUDIT.READ, AUDITMANAGEMENT.MANAGE |
| Auditor E2E | TENANT.READ, AUDIT.READ/MANAGE, AUDITMANAGEMENT.MANAGE, CAPA.MANAGE/READ, RISK.READ, INDICATOR.READ, REPORT.READ/EXECUTE |
| Supplier Manager E2E | TENANT.READ, SUPPLIER.MANAGE, AUDIT.READ, RISK.READ, CAPA.READ, AUDITMANAGEMENT.MANAGE, REPORT.READ/EXECUTE, INDICATOR.READ, **CAPA.MANAGE** |
| CAPA Manager E2E | TENANT.READ, CAPA.MANAGE/READ, AUDIT.READ, RISK.READ, INDICATOR.READ, REPORT.READ/EXECUTE, **AUDITMANAGEMENT.MANAGE** |
| Risk Manager E2E | TENANT.READ, RISK.MANAGE/READ, CAPA.READ, INDICATOR.READ, AUDIT.READ, AUDITMANAGEMENT.MANAGE, REPORT.READ/EXECUTE |
| Indicators Manager E2E | TENANT.READ, INDICATOR.MANAGE/READ, RISK.READ, CAPA.READ, AUDIT.READ, AUDITMANAGEMENT.MANAGE, REPORT.READ/EXECUTE |
| Reporting Manager E2E | TENANT.READ, REPORT.READ/EXECUTE, INDICATOR.READ, RISK.READ, CAPA.READ, AUDIT.READ, AUDITMANAGEMENT.MANAGE |
| Storage Admin E2E | TENANT.READ, TENANT.STORAGE, STORAGE.MANAGE, AUDIT.READ, AUDITMANAGEMENT.MANAGE + lecturas dashboard |
| Notification Admin E2E | TENANT.READ, TENANT.NOTIFICATIONS, NOTIFICATION.* (5), STORAGE.MANAGE, lecturas dashboard |
| Viewer E2E | TENANT.READ, CAPA.READ, RISK.READ, INDICATOR.READ, REPORT.READ/EXECUTE, AUDIT.READ, NOTIFICATION.READ |

---

## Hallazgos de la matriz

1. **Granularidad inconsistente:** CAPA/RISK/INDICATOR/REPORT tienen READ/APPROVE; DOCUMENT/SUPPLIER/WORKFLOW no.
2. **Dashboard fuerza permisos transversales** — casi todos los roles necesitan `AUDITMANAGEMENT.MANAGE` o múltiples `*.READ`.
3. **Frontend asume permisos inexistentes** para menú de documents/suppliers/technical-sheets en modo lectura.
4. **SuperAdmin invalida la matriz** como control de seguridad.
5. **No hay matriz canónica en repositorio** — cada tenant define la suya vía RBAC UI.
