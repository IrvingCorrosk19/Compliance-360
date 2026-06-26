# Tenant Administration Center Enterprise - Implementation

Fecha: 2026-06-25

## Resumen

Se reconstruyo Tenant Management como **Tenant Administration Center**, ubicado funcionalmente en:

Enterprise -> Administration -> Tenant Administration Center

El modulo deja de operar como CRUD simple y pasa a ser una consola administrativa enterprise con dashboard ejecutivo, tabs especializadas, permisos granulares, auditoria semantica y lifecycle SaaS completo.

## Arquitectura

Se mantuvo la arquitectura existente:

- **Domain:** se extendio el agregado `Tenant` y sus entidades hijas `TenantSettings`, `TenantBranding` y `Subscription`.
- **Application:** se ampliaron contratos y casos de uso en `ITenantManagementService`.
- **Infrastructure:** se extendio `EfTenantManagementRepository` con consultas de dashboard, busqueda, metricas y timeline.
- **Web/API:** se agregaron endpoints Minimal API bajo `/api/v1/tenants`.
- **SPA:** se agrego la ruta `tenant-administration` dentro del grupo Enterprise.
- **Database:** se genero migracion EF `AddTenantAdministrationCenter` con backfill seguro.

No se creo arquitectura paralela.

## Pantallas

La nueva pantalla `Tenant Administration Center` incluye:

- Hero enterprise con tenant, razon social, tax id, pais, moneda, estado y health.
- Dashboard ejecutivo con Tenant, Estado, Plan, Licencia, Usuarios, Storage, Documentos, Proveedores, Auditorias, CAPA, Riesgos, Indicadores, Notificaciones, Health, Ultimo Backup y Ultimo Login.
- Alertas operativas por security score, storage provider, notification provider y estado del tenant.
- Navegacion por 10 tabs:
  - Informacion General
  - Branding
  - Seguridad
  - Usuarios
  - Licenciamiento
  - Storage
  - Notificaciones
  - Integraciones
  - Auditoria
  - Estado

La UI usa cards, tabs, timeline, indicadores, alertas, responsive layout, dark mode y los loading states enterprise ya existentes.

Capturas: no se generaron imagenes porque el entorno no expuso una herramienta de screenshot/browser en esta ejecucion. La vista fue validada por `node --check`, build, tests y smoke API real.

## Flujos

Flujos implementados:

- Consultar tenant individual.
- Listar/buscar tenants.
- Ver dashboard ejecutivo del tenant.
- Editar informacion general empresarial.
- Editar branding.
- Editar seguridad.
- Editar licenciamiento.
- Cambiar estado a Trial, Active, Suspended, Archived y Restore.
- Ver timeline de auditoria tenant-scoped.
- Acceder desde el shell Enterprise.

## Permisos

Se reemplazo el uso operativo de `TENANT.MANAGE` por permisos granulares:

- `TENANT.READ`
- `TENANT.CREATE`
- `TENANT.UPDATE`
- `TENANT.STATUS`
- `TENANT.BRANDING`
- `TENANT.SECURITY`
- `TENANT.STORAGE`
- `TENANT.NOTIFICATIONS`
- `TENANT.INTEGRATIONS`
- `TENANT.BILLING`
- `TENANT.USERS`
- `TENANT.ROLES`
- `TENANT.AUDIT`
- `TENANT.DELETE`
- `TENANT.RESTORE`

El rol plataforma `SuperAdmin` puede operar como administrador global, pero los demas roles requieren permisos especificos.

## APIs

Endpoints principales:

- `GET /api/v1/tenants`
- `POST /api/v1/tenants`
- `GET /api/v1/tenants/{tenantId}`
- `GET /api/v1/tenants/{tenantId}/administration-dashboard`
- `PUT /api/v1/tenants/{tenantId}/general-information`
- `PUT /api/v1/tenants/{tenantId}/security`
- `PUT /api/v1/tenants/{tenantId}/branding`
- `PUT /api/v1/tenants/{tenantId}/subscription`
- `POST /api/v1/tenants/{tenantId}/trial`
- `POST /api/v1/tenants/{tenantId}/activate`
- `POST /api/v1/tenants/{tenantId}/suspend`
- `POST /api/v1/tenants/{tenantId}/archive`
- `POST /api/v1/tenants/{tenantId}/restore`
- `GET /api/v1/tenants/{tenantId}/audit-timeline`

## Base De Datos

La migracion `20260625235648_AddTenantAdministrationCenter` agrega:

- Perfil empresarial en `tenants`: legal name, commercial name, tax id, industria, descripcion, direccion, ciudad, provincia, pais, codigo postal, telefono, email, website, moneda y created by.
- Seguridad en `tenant_settings`: language, session timeout, password expiration, lockout, IP whitelist, trusted devices y security score.
- Branding avanzado en `tenant_branding`: favicon, theme, login background, corporate email y footer.
- Indice unico en `tenants.TaxIdentifier`.
- Backfill seguro para tenants existentes antes de crear el indice unico.

## Auditoria

Las operaciones nuevas usan `AuditAction.TenantChanged`, lo que clasifica correctamente los eventos como `TenantManagement`.

Cada cambio queda cubierto por:

- Auditoria manual con motivo opcional en metadata.
- Interceptor EF con before/after snapshots, usuario, IP, correlation id y request context cuando aplica.

## Validaciones

Implementado:

- `TenantId` no editable.
- `CreatedAtUtc` no editable.
- `CreatedByUserId` no editable por UI.
- Slug no expuesto como edicion normal.
- `TaxIdentifier` unico.
- Backfill evita duplicidad fiscal en migracion.
- Slug unico existente se conserva.
- Lifecycle protegido por permisos especiales.
- Campos sensibles separados por permisos: seguridad, billing, branding, status, delete/restore.

## Pruebas Ejecutadas

- `dotnet build "Compliance360.sln"`: exitoso.
- `dotnet test "Compliance360.sln"`: 223/223 exitosas.
- `node --check "src/Compliance360.Web/wwwroot/app.js"`: exitoso.
- Migracion EF local aplicada: exitosa.
- Smoke health: exitoso.
- Smoke login SuperAdmin: exitoso.
- Smoke dashboard Tenant Administration Center: exitoso.
- Smoke update general-information: exitoso.
- Smoke audit timeline: exitoso.

## Cambios Realizados

Archivos principales modificados:

- `src/Compliance360.Domain/TenantManagement/TenantManagementModels.cs`
- `src/Compliance360.Application/TenantManagement/TenantManagementContracts.cs`
- `src/Compliance360.Application/TenantManagement/TenantManagementService.cs`
- `src/Compliance360.Infrastructure/TenantManagement/EfTenantManagementRepository.cs`
- `src/Compliance360.Infrastructure/Persistence/Compliance360DbContext.cs`
- `src/Compliance360.Infrastructure/Persistence/Migrations/20260625235648_AddTenantAdministrationCenter.cs`
- `src/Compliance360.Web/Api/FoundationEndpoints.cs`
- `src/Compliance360.Web/Api/ApiContracts.cs`
- `src/Compliance360.Web/Security/PermissionPolicies.cs`
- `src/Compliance360.Web/wwwroot/app.js`
- `src/Compliance360.Web/wwwroot/styles.css`
- `tests/Compliance360.Tests/TenantManagementServiceTests.cs`
- `artifacts/migrations/clean-reset-for-testing.sql`
- `artifacts/migrations/seed-tenant-administration-permissions.sql`

## Decisiones Tecnicas

- Mantener `Tenant` como agregado principal.
- No exponer cambio normal de `Slug` en UI para evitar ruptura accidental de URLs/integraciones.
- Usar `TaxIdentifier` unico a nivel plataforma.
- Mantener provider administration existente para storage/notificaciones y enlazarlo desde tabs.
- Exponer SSO/API keys/webhooks/LDAP/AD como seccion enterprise protegida por permiso, dejando conectores funcionales para fases especializadas.
- Mantener SuperAdmin como operador global, sin reintroducir `TENANT.MANAGE` como requisito operativo.
