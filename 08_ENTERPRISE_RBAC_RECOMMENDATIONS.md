# 08 — Enterprise RBAC Recommendations

## Principios rectores (fase 2 — no implementar aún)

1. **Control plane ≠ data plane** — SuperAdmin nunca opera datos de negocio tenant por defecto.
2. **Permisos pequeños y composables** — preferir READ / CREATE / APPROVE sobre MONOLITHIC `*.MANAGE`.
3. **Roles plantilla por tenant** — catálogo versionado en código o seed migration, no solo Development.
4. **SoD por diseño** — elaboración, aprobación y auditoría con permisos disjoint.
5. **Un solo lugar de verdad** — códigos permiso idénticos en bootstrap, policies, JWT y `app.js`.

---

## R1 — Arquitectura de roles

### R1.1 Definir roles sistema en código

Crear artefacto `RoleTemplates` (constantes o seed migration) con los 13 roles operativos + 4 roles plataforma:

| Plataforma | Tenant operativos |
|------------|-------------------|
| PlatformAdmin | TenantAdmin |
| PlatformSecurityAdmin | TenantSecurityAdmin |
| PlatformOpsAdmin | DocumentController |
| SupportOperator (break-glass) | QualityManager, Auditor, ... |

**Evidencia que lo exige:** G1 — solo SuperAdmin sembrado (`DevelopmentBootstrap.cs`).

### R1.2 Plantillas de permisos por rol (baseline)

Propuesta basada en E2E corregido + SoD:

| Rol | Permisos baseline propuestos |
|-----|------------------------------|
| TenantAdmin | TENANT.* (sin DELETE), RBAC.MANAGE, IDENTITY.MANAGE, AUDIT.READ |
| DocumentController | TENANT.READ, DOCUMENT.CREATE, DOCUMENT.READ, DOCUMENT.UPDATE, WORKFLOW.READ, AUDIT.READ |
| QualityManager | TENANT.READ, DOCUMENT.APPROVE, CAPA.APPROVE, CAPA.CLOSE, RISK.APPROVE, INDICATOR.APPROVE, REPORT.EXECUTE, AUDIT.READ |
| Auditor | TENANT.READ, AUDITMANAGEMENT.MANAGE, AUDIT.READ, CAPA.CREATE (nuevo — solo originar), REPORT.READ |
| SupplierManager | TENANT.READ, SUPPLIER.MANAGE, AUDIT.READ, REPORT.READ |
| CapaManager | TENANT.READ, CAPA.MANAGE, CAPA.READ, RISK.READ, INDICATOR.READ, REPORT.READ |
| RiskManager | TENANT.READ, RISK.MANAGE, RISK.READ, REPORT.READ |
| IndicatorsManager | TENANT.READ, INDICATOR.MANAGE, INDICATOR.READ, REPORT.READ |
| ReportingManager | TENANT.READ, REPORT.READ, REPORT.EXECUTE, REPORT.EXPORT, REPORT.SCHEDULE |
| StorageAdmin | TENANT.READ, TENANT.STORAGE, STORAGE.MANAGE, AUDIT.READ |
| NotificationAdmin | TENANT.READ, TENANT.NOTIFICATIONS, NOTIFICATION.* (sin STORAGE) |
| Viewer | TENANT.READ, *.READ (todos los módulos visibles), AUDIT.READ |

### R1.3 Reducir Quality Manager

**Cambio:** QM debe ser rol de **aprobación y coordinación**, no de `*.MANAGE` en todos los módulos.

**Evidencia:** E2E otorgó 20 permisos — solapa 4 roles especialistas (`07_ROLE_GAP_ANALYSIS.md` G3).

---

## R2 — Catálogo de permisos

### R2.1 Añadir permisos READ faltantes

| Nuevo permiso | Módulo API | Policy nueva |
|---------------|------------|--------------|
| DOCUMENT.READ | GET `/documents` | Document.Read |
| DOCUMENT.APPROVE | POST `.../decision` | Document.Approve |
| TECHNICALSHEET.READ | GET technical-sheets | TechnicalSheet.Read |
| SUPPLIER.READ | GET suppliers | Supplier.Read |
| AUDITMANAGEMENT.READ | GET audit-management, dashboard | AuditManagement.Read |
| WORKFLOW.READ | GET workflows | Workflow.Read |

**Evidencia:** `app.js` L145–166 referencia permisos inexistentes.

### R2.2 Dividir `*.MANAGE` monolíticos

Prioridad:

1. DOCUMENT — CREATE, READ, UPDATE, APPROVE, OBSOLETE
2. SUPPLIER — mantener MANAGE o dividir homologación vs evaluación
3. STORAGE — READ vs MANAGE para metadata
4. WORKFLOW — READ vs MANAGE

### R2.3 Alinear SuperAdmin permisos UI

- Renombrar en frontend `SUPERADMIN.DASHBOARD` → `SUPERADMIN.DASHBOARD.READ`
- O añadir alias en policy

---

## R3 — SuperAdmin y plataforma

### R3.1 Eliminar bypass por rol (prioridad P0)

Reemplazar `HasPlatformSuperAdmin` por:

- Claims `permission` explícitos por endpoint platform
- Rol `PlatformAdmin` con solo `SUPERADMIN.*` y `TENANT.CREATE/READ/STATUS`

**Evidencia:** `PermissionPolicies.cs` L160–181; calificación SoD 2/10 en plataforma.

### R3.2 Cross-tenant con break-glass

- Quitar bypass en `ApiContext` para operaciones de negocio
- Mantener cross-tenant solo para `/superadmin/*` y `/tenants` search
- SupportOperator: acceso temporal auditado con ticket

**Evidencia:** `ApiContext.cs` L28–32.

### R3.3 Wire o ocultar tabs huérfanas

Tabs Platform Center sin API: Licencias, Módulos, IA, DevOps — implementar endpoint o ocultar hasta existir.

---

## R4 — Frontend RBAC

### R4.1 Route guard obligatorio

En `renderRoute()`, si `!canNavigate(route)` → pantalla 403 amigable.

### R4.2 TAC granular

Cada tab con `hasPermission` matching backend labels ya documentados en UI.

### R4.3 Dashboard desacoplado

Endpoint `/dashboard/summary` que devuelve solo métricas autorizadas — eliminar necesidad de `AUDITMANAGEMENT.MANAGE` para contador "Audit Open".

**Evidencia:** `app.js` L713 — métrica audit requiere MANAGE.

### R4.4 Enterprise workspaces

Aplicar `canManageRoute` o permiso específico por workspace type.

---

## R5 — Segregación de funciones

| Control | Acción |
|---------|--------|
| Documento elaborar ≠ aprobar | DOCUMENT.CREATE vs DOCUMENT.APPROVE |
| Auditor ≠ cerrar CAPA | Quitar CAPA.MANAGE de Auditor; CAPA.CREATE from finding |
| Supplier ≠ gestionar CAPA completa | Workflow trigger sin CAPA.MANAGE |
| Viewer | Solo `*.READ`; nunca TENANT.READ para enterprise create |
| Notification ≠ Storage | Split configuration route |

---

## R6 — Gobernanza y operación

### R6.1 Role templates al crear tenant

Al POST `/tenants`, opcionalmente sembrar roles baseline del tenant.

### R6.2 Permission catalog versioning

Tabla `permission_catalog_version`; Tenant Admin no puede crear permisos arbitrarios fuera de catálogo (solo grant).

**Evidencia:** `CreatePermissionAsync` permite módulos custom.

### R6.3 Auditoría RBAC

Ya existe `PermissionDenied` en `RbacService` — extender a denegaciones policy middleware.

### R6.4 Documentación viva

Generar matriz permisos desde código (test que compare bootstrap ↔ policies ↔ app.js).

---

## R7 — Comparación benchmark

| Capacidad | M365 | Salesforce | ServiceNow | Compliance 360 hoy | Target |
|-----------|------|------------|------------|-------------------|--------|
| Role templates | ✓ | ✓ | ✓ | ✗ | ✓ |
| Permission sets | ✓ | ✓ | ✓ | ✗ | ✓ |
| SoD engine | ✓ | Parcial | ✓ | ✗ | Parcial |
| Admin sin data access | ✓ | ✓ | ✓ | ✗ | ✓ |
| Granular READ | ✓ | ✓ | ✓ | Parcial | ✓ |
| Break-glass | PIM | — | Elevated | ✗ | ✓ |

---

## Plan de implementación sugerido (fase 2)

| Fase | Entregables | Riesgo |
|------|-------------|--------|
| **2.1 Baseline** | Alinear códigos permiso UI/backend; añadir READ faltantes | Bajo |
| **2.2 Templates** | Seed 13 roles tenant al crear tenant; doc matriz oficial | Medio |
| **2.3 SuperAdmin** | Quitar bypass; PlatformAdmin scoped | Alto |
| **2.4 SoD** | DOCUMENT.APPROVE; reducir QM; fix Auditor/Supplier | Medio |
| **2.5 UI hardening** | Route guard, TAC tabs, dashboard API | Bajo |
| **2.6 Hardening** | Tests contrato RBAC; SoD rules básicas | Medio |

---

## Qué NO cambiar todavía (explícito)

- No modificar grants en base de datos de producción sin ventana de cambio.
- No eliminar `SuperAdmin` sin migración a PlatformAdmin.
- No reducir permisos E2E en tenant de prueba hasta tener plantillas.

---

## Métricas de éxito post-refactor

| KPI | Actual | Target |
|-----|--------|--------|
| Roles definidos en código | 1 | 17 |
| Permisos solo en UI sin backend | 4+ | 0 |
| Roles con >15 permisos MANAGE | 2 (SA, QM) | 0 |
| SoD score | 4.4/10 | 8/10 |
| RBAC enterprise score | 48/100 | 85/100 |
