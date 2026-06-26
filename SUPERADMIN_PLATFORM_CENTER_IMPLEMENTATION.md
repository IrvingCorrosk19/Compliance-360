# SuperAdmin Platform Center - Implementation

Fecha: 2026-06-25

## Implementacion

Se agrego un modulo global independiente del Tenant Administration Center. El SuperAdmin Platform Center administra la plataforma Compliance 360 como operador SaaS, no como administrador de una empresa.

## Backend

- Nuevo slice Application: `Compliance360.Application.SuperAdmin`.
- Nuevo repositorio EF: `EfSuperAdminPlatformRepository`.
- Nuevo servicio: `SuperAdminPlatformService`.
- Nuevo endpoint global: `/api/v1/superadmin/platform-center`.
- Export global de auditoria: `/api/v1/superadmin/platform-center/audit-timeline/export`.
- Busqueda global de tenants: `/api/v1/superadmin/platform-center/tenants`.

## Datos Reales

El dashboard global lee datos desde entidades existentes:

- Tenants y sus estados.
- Usuarios y usuarios activos.
- Documentos.
- Auditorias gestionadas.
- CAPA.
- Riesgos.
- Indicadores.
- Stored files y consumo de storage.
- Storage providers.
- Notification providers.
- Tenant licenses.
- Tenant health signals.
- Tenant backup records.
- Audit logs.
- Notification retries como backlog de jobs.

## Frontend

Se agrego la ruta `superadmin-platform` con:

- Executive dashboard.
- Global search.
- Quick actions.
- Alert center.
- Tenants fleet.
- Licencias.
- Modulos.
- Providers.
- Seguridad global.
- Observability.
- Auditoria global.
- Database monitoring read-only.
- IA read model.
- Configuracion global.
- Backups.
- DevOps.

## Seguridad

No se uso `SUPERADMIN.MANAGE`. Se agregaron permisos granulares `SUPERADMIN.*` por area: dashboard, tenants, licenses, modules, providers, security, observability, audit, database, AI, configuration, backups, DevOps y search.

## Validacion

- `dotnet build Compliance360.sln`: passed.
- `dotnet test Compliance360.sln`: 225 passed.
- `node --check src/Compliance360.Web/wwwroot/app.js`: passed.
- Smoke local `/health`: 200 OK.
- Smoke local login SuperAdmin: 200 OK.
- Smoke local `/api/v1/superadmin/platform-center`: 200 OK.
