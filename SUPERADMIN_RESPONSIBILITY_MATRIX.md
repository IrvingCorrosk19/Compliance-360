# SuperAdmin Responsibility Matrix

## Principio rector

El SuperAdmin debe administrar la **plataforma SaaS**, no el negocio interno de cada tenant. Su alcance natural es control plane, no data plane.

## Matriz de responsabilidades recomendadas

| Dominio | Responsabilidad SuperAdmin | Estado actual | Recomendación | Riesgo |
|---|---|---|---|---|
| Tenants | Crear, suspender, activar, archivar, restaurar tenants | Disponible por `TENANT.*` y `SUPERADMIN.*` | Mantener, pero preferir endpoints `SUPERADMIN.TENANTS.*` dedicados | Bajo |
| Lifecycle tenant | Trial, activo, suspendido, archivado | Disponible | Mantener en SuperAdmin | Bajo |
| Planes/suscripciones | Plan, límites usuarios/storage, estado subscription | Disponible en `/tenants/{tenantId}/subscription` | Mantener, mover a namespace platform billing | Medio |
| Licencias | Licencia, módulos, entitlements | Disponible en `/tenants/{tenantId}/license` y vista SuperAdmin | Mantener en SuperAdmin | Bajo |
| Facturación global | Billing y límites globales | Parcial | Completar en control plane | Bajo |
| Catálogo de módulos | Módulos por tenant, feature flags | Simulado/derivado en SuperAdmin repo | Implementar catálogo real | Medio |
| Feature flags | Habilitar/deshabilitar capacidades por plan/tenant | Parcial | SuperAdmin only | Medio |
| Providers globales | SMTP/storage globales o defaults | Vista global agregada | Separar provider global vs provider tenant | Medio |
| Seguridad global | Políticas base, rate limits, headers, global SSO/OAuth | Parcial | SuperAdmin only | Medio |
| SSO global | Configuración de proveedores globales | Parcial/tenant-scoped | Separar global SSO del tenant SSO | Medio |
| SSO tenant | Configuración propia de un tenant | Actualmente accesible por SuperAdmin | TenantAdmin; SuperAdmin solo soporte temporal | Medio |
| API keys globales | API keys de plataforma | No claramente separado | SuperAdmin only | Medio |
| API keys tenant | Credenciales de tenant | Accesible por SuperAdmin | TenantAdmin; soporte temporal | Alto |
| Auditoría global | Eventos agregados/redacted | Disponible | Mantener con redacción | Medio |
| Exportar auditoría global | CSV global | Disponible | Mantener con controles y redacción | Medio-alto |
| Logs globales | Logs técnicos del sistema | Parcial | SuperAdmin only | Bajo |
| Monitoreo | Health, métricas, servicios | Disponible | Mantener | Bajo |
| Base de datos | Estado DB, conectividad, métricas | Disponible | Mantener read-only | Bajo |
| Backups | Backups, RPO/RTO, estado | Disponible | Mantener | Bajo |
| Migraciones/versiones | Versión app, migraciones, deploys | Parcial | SuperAdmin only | Bajo |
| IA global | Proveedores/consumo IA | Vista placeholder | SuperAdmin only | Bajo-medio |
| Marketplace | Add-ons, módulos disponibles | No implementado | SuperAdmin only | Bajo |
| Usuarios tenant | Crear/editar/bloquear usuarios tenant | Accesible por bypass | No debe ser normal SuperAdmin | Alto |
| Roles tenant | Crear roles/grants tenant | Accesible por bypass | TenantAdmin; soporte break-glass | Alto |
| Documentos | Crear/aprobar/versionar documentos | Accesible por bypass | Tenant role only | Alto |
| Proveedores | Homologación/evaluación proveedores | Accesible por bypass | Tenant role only | Alto |
| Auditorías operativas | Programas, planes, hallazgos | Accesible por bypass | Tenant role only | Alto |
| CAPA | Causa raíz, acciones, efectividad | Accesible por bypass | Tenant role only | Alto |
| Riesgos | Matriz, tratamientos, controles | Accesible por bypass | Tenant role only | Alto |
| Indicadores | KPIs, metas, mediciones | Accesible por bypass | Tenant role only | Alto |
| Reportes operativos | Ejecutar/exportar reportes tenant | Accesible por bypass | Tenant role only | Alto |
| Archivos tenant | Upload/download/delete | Accesible por bypass | Tenant role only | Alto |
| Notificaciones tenant | Templates, mensajes, tracking | Accesible por bypass | TenantAdmin/Notification Admin | Medio-alto |

## Responsabilidades que deben conservarse en SuperAdmin

- Tenant lifecycle.
- Billing/licensing.
- Platform module catalog.
- Global provider defaults.
- Global security baseline.
- Global observability.
- Global audit redacted.
- Infrastructure health.
- Backup/migration/version visibility.
- System configuration.

## Responsabilidades que deben removerse del SuperAdmin estándar

- Administración de usuarios tenant.
- RBAC tenant.
- CRUD documental.
- CRUD proveedores.
- Auditorías operativas.
- CAPA.
- Riesgos.
- Indicadores.
- Reportes operativos.
- Workflows.
- Archivos.
- Plantillas y mensajes tenant.

## Roles recomendados

| Rol | Propósito | Permisos |
|---|---|---|
| `PlatformAdmin` | Administración SaaS/control plane | Solo `SUPERADMIN.*` y operaciones globales explícitas |
| `PlatformBillingAdmin` | Licencias, planes, facturación | `SUPERADMIN.TENANTS.READ`, `SUPERADMIN.LICENSES.MANAGE`, billing |
| `PlatformSecurityAdmin` | Seguridad global, SSO/OAuth global, políticas base | `SUPERADMIN.SECURITY.MANAGE`, audit read |
| `PlatformOpsAdmin` | Health, backups, DB metrics, DevOps | Observabilidad, DB, backups, DevOps |
| `SupportOperator` | Soporte temporal en tenant | Claim temporal `support_tenant_id`, reason, expiry, approval |
| `BreakGlassAdmin` | Emergencia auditada | Elevación temporal con MFA fuerte y logging reforzado |

