# SuperAdmin Platform Center - Architecture

## Principio

El SuperAdmin Platform Center es un read model global de operacion SaaS. No reemplaza el Tenant Administration Center. Lo complementa como consola de gobierno de toda la plataforma.

## Capas

### Domain

No se agregaron nuevos aggregates porque la primera version usa datos productivos existentes. Esto evita duplicar estado y respeta el modelo multitenant actual.

### Application

`ISuperAdminPlatformService` orquesta:

- Dashboard global.
- Busqueda global de tenants.
- Timeline global de auditoria.
- Alertas.
- Quick actions.

`ISuperAdminPlatformRepository` define los read models requeridos.

### Infrastructure

`EfSuperAdminPlatformRepository` consulta `Compliance360DbContext` con `AsNoTracking` y agrega metricas globales desde:

- `Tenants`
- `Users`
- `Documents`
- `ManagedAudits`
- `Capas`
- `Risks`
- `QualityIndicators`
- `StoredFiles`
- `StorageProviderConfigurations`
- `NotificationProviderConfigurations`
- `TenantLicenses`
- `TenantHealthSignals`
- `TenantBackupRecords`
- `AuditLogs`
- `NotificationRetries`

### Web API

Grupo: `/api/v1/superadmin/platform-center`

Endpoints:

- `GET /`
- `GET /tenants`
- `GET /audit-timeline`
- `GET /audit-timeline/export`

### Frontend

Ruta SPA: `superadmin-platform`

El modulo renderiza widgets y paneles especializados, no una tabla unica. Las acciones profundas abren el Tenant Administration Center con el tenant seleccionado, preservando el boundary tenant-scoped.

## Seguridad

Los permisos se definen por operacion y area:

- `SUPERADMIN.DASHBOARD.READ`
- `SUPERADMIN.TENANTS.READ`
- `SUPERADMIN.TENANTS.CREATE`
- `SUPERADMIN.TENANTS.UPDATE`
- `SUPERADMIN.TENANTS.STATUS`
- `SUPERADMIN.TENANTS.DELETE`
- `SUPERADMIN.LICENSES.MANAGE`
- `SUPERADMIN.MODULES.MANAGE`
- `SUPERADMIN.PROVIDERS.MANAGE`
- `SUPERADMIN.PROVIDERS.HEALTH`
- `SUPERADMIN.SECURITY.MANAGE`
- `SUPERADMIN.OBSERVABILITY.READ`
- `SUPERADMIN.AUDIT.READ`
- `SUPERADMIN.AUDIT.EXPORT`
- `SUPERADMIN.DATABASE.READ`
- `SUPERADMIN.AI.MANAGE`
- `SUPERADMIN.CONFIGURATION.MANAGE`
- `SUPERADMIN.BACKUPS.READ`
- `SUPERADMIN.DEVOPS.READ`
- `SUPERADMIN.SEARCH`

## Database Safety

El modulo no ejecuta modificaciones directas contra PostgreSQL. La seccion Database es read-only y reporta conectividad, provider EF, volumen de filas, audit rows, stored files y backlog operacional.
