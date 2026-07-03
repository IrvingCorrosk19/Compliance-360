# SuperAdmin Enterprise Audit

## Objetivo

Auditar si el rol `SuperAdmin` de Compliance 360 cumple una arquitectura SaaS Multi-Tenant enterprise o si asume responsabilidades que deben pertenecer a cada tenant.

## Veredicto Ejecutivo

La arquitectura tiene una base multi-tenant funcional: casi todos los módulos operativos exponen rutas bajo `/api/v1/tenants/{tenantId}` y pasan por `ApiContext.TenantId(...)`; los repositorios usan `TenantId` en la mayoría de consultas operativas; y existe una superficie separada para plataforma en `/api/v1/superadmin/platform-center`.

Sin embargo, el rol `SuperAdmin` actualmente funciona como **superusuario con bypass transversal**:

- `PermissionPolicies.HasPlatformSuperAdmin(...)` hace que cualquier usuario con rol `SuperAdmin` pase cualquier policy.
- `ApiContext.TenantId(...)` permite que `SuperAdmin` use cualquier `tenantId` por URL.
- El menú expone `SuperAdmin Platform` junto con operaciones de tenant.
- El dashboard global muestra métricas agregadas de datos de negocio: documentos, auditorías, CAPA, riesgos, indicadores, usuarios, proveedores, storage y notificaciones.
- Los endpoints de tenant permiten que el `SuperAdmin`, por su bypass, administre información operativa de tenants.

Esto es útil para desarrollo y soporte interno, pero no representa una separación estricta tipo Salesforce, Dynamics, HubSpot, Zoho o Atlassian Cloud, donde el administrador de plataforma normalmente gestiona configuración global, licencias, tenants, seguridad global y observabilidad, pero no opera datos de negocio de clientes salvo flujos explícitos de soporte con elevación temporal, auditoría reforzada y consentimiento.

## Calificación Enterprise SaaS Multi-Tenant

**62 / 100**

### Razones de la calificación

- Base técnica multi-tenant: fuerte.
- Separación inicial de endpoints SuperAdmin: presente.
- Tenant scoping por URL y servicios: presente.
- Bypass global del rol `SuperAdmin`: alto riesgo.
- Mezcla de administración de plataforma y operación de tenant en UX: alto riesgo.
- Falta de modelo de soporte/elevación temporal: alto riesgo.
- Falta de separación clara entre "platform admin" y "tenant impersonation/support": medio-alto.
- Reportes globales agregan métricas de negocio tenant-scoped: medio.

## Arquitectura actual observada

### Backend

La aplicación usa ASP.NET Core Minimal APIs en:

- `src/Compliance360.Web/Api/FoundationEndpoints.cs`

Superficie global:

- `/api/v1/superadmin/platform-center`
- `/api/v1/superadmin/platform-center/tenants`
- `/api/v1/superadmin/platform-center/audit-timeline`
- `/api/v1/superadmin/platform-center/audit-timeline/export`

Superficie tenant-scoped:

- `/api/v1/tenants`
- `/api/v1/tenants/{tenantId}`
- `/api/v1/tenants/{tenantId}/administration-center`
- `/api/v1/tenants/{tenantId}/users`
- `/api/v1/tenants/{tenantId}/rbac`
- `/api/v1/tenants/{tenantId}/documents`
- `/api/v1/tenants/{tenantId}/suppliers`
- `/api/v1/tenants/{tenantId}/audit-management`
- `/api/v1/tenants/{tenantId}/capas`
- `/api/v1/tenants/{tenantId}/risks`
- `/api/v1/tenants/{tenantId}/indicators`
- `/api/v1/tenants/{tenantId}/reports`
- `/api/v1/tenants/{tenantId}/storage`
- `/api/v1/tenants/{tenantId}/notifications`
- `/api/v1/tenants/{tenantId}/enterprise-workspaces`

### Autorización

Archivo:

- `src/Compliance360.Web/Security/PermissionPolicies.cs`

Hallazgo clave:

```text
HasPermission(...) retorna true si HasPlatformSuperAdmin(...) es true.
```

Esto convierte al `SuperAdmin` en un bypass universal sobre policies tenant y plataforma.

### Tenant Context

Archivo:

- `src/Compliance360.Web/Api/ApiContext.cs`

Hallazgo clave:

```text
TenantId(...) rechaza mismatch tenant claim vs route tenantId excepto cuando el usuario tiene rol SuperAdmin.
```

Esto permite que `SuperAdmin` cambie el `tenantId` por URL y opere dentro de cualquier tenant.

### Frontend / UX

Archivo:

- `src/Compliance360.Web/wwwroot/app.js`

El menú actual tiene:

- Command Center
- Operations
- Enterprise

`SuperAdmin Platform` está expuesto en `Operations` y `Enterprise`. La plataforma global incluye tabs como Executive, Tenants, Licencias, Módulos, Providers, Seguridad, Observability, Auditoría, Database, IA, Configuración, Backups, DevOps.

### Repositorio SuperAdmin

Archivo:

- `src/Compliance360.Infrastructure/SuperAdmin/EfSuperAdminPlatformRepository.cs`

El repositorio obtiene métricas globales sin filtrar tenant:

- total users
- stored files
- documents
- audits
- CAPA
- risks
- indicators
- providers
- notification retries
- tenant licenses
- audit timeline global

Esto es aceptable para observabilidad agregada, pero debe evitar exponer información operativa sensible o permitir acción directa sobre datos de tenant.

## Comparación con SaaS Enterprise

| Plataforma | Patrón recomendado | Comparación con Compliance 360 |
|---|---|---|
| Salesforce | Admin global administra orgs, licencias, seguridad global; datos de clientes pertenecen a cada org. Acceso soporte es controlado. | Compliance 360 separa plataforma y tenants, pero `SuperAdmin` puede operar tenants directamente por bypass. |
| Microsoft Dynamics | Admin center gestiona ambientes, capacidad, usuarios/licencias, seguridad global; datos de negocio se gestionan dentro del environment. | Compliance 360 mezcla métricas/datos de negocio en el centro global y permite operación por SuperAdmin. |
| HubSpot | Super admin de cuenta opera su cuenta; staff interno tiene herramientas separadas de soporte/ops con controles. | Falta separación entre platform staff y tenant admin. |
| Zoho | Consola global administra organización/suscripción; módulos CRM/operativos quedan dentro de org. | La UI actual permite navegar entre plataforma y módulos tenant. |
| Atlassian Cloud | Admin central gestiona sites, billing, products, usuarios; contenido de Jira/Confluence es administrado por admins del site/proyecto. | Compliance 360 necesita distinguir "platform administration" vs "tenant content administration". |

## Funcionalidad actual del SuperAdmin

| Funcionalidad | Controller/Endpoint | Acción | ¿Debe pertenecer al SuperAdmin? | ¿Debe pertenecer al Tenant? | Riesgo |
|---|---|---|---|---|---|
| Dashboard plataforma | `GET /superadmin/platform-center` | Ver métricas globales | Sí, agregadas | No | Medio: expone conteos de negocio por plataforma |
| Buscar tenants | `GET /superadmin/platform-center/tenants` | Listar tenants | Sí | No | Bajo |
| Auditoría global | `GET /superadmin/platform-center/audit-timeline` | Ver eventos globales | Sí, con redacción | No | Medio: puede exponer metadata tenant |
| Exportar auditoría global | `GET /superadmin/platform-center/audit-timeline/export` | CSV global | Sí, restringido | No | Medio-alto |
| Crear tenant | `POST /tenants` | Alta tenant | Sí | No | Bajo |
| Activar/suspender tenant | `/tenants/{tenantId}/activate|suspend` | Estado tenant | Sí | No | Bajo |
| Archivar/restaurar tenant | `/tenants/{tenantId}/archive|restore` | Lifecycle tenant | Sí | No | Medio |
| Cambiar suscripción/licencia | `/tenants/{tenantId}/subscription`, `/license` | Plan, límites, licencia | Sí | No | Bajo-medio |
| Configurar dominios/SSO/API keys/webhooks tenant | `/tenants/{tenantId}/domains|sso|api-keys|webhooks` | Integraciones tenant | Compartido: plataforma define límites, Tenant Admin opera configuración | Sí | Medio |
| Configurar branding tenant | `/tenants/{tenantId}/branding` | Marca tenant | No por defecto | Sí | Medio |
| Configurar seguridad tenant | `/tenants/{tenantId}/security` | MFA, lockout, sesión, IPs | Compartido: guardrails globales por plataforma, valores tenant por Tenant Admin | Sí | Medio |
| Administrar usuarios tenant | `/tenants/{tenantId}/users` | Crear, estado, MFA reset, sesiones | No por defecto | Sí | Alto |
| Administrar roles/permisos tenant | `/tenants/{tenantId}/rbac` | Crear roles, permisos, grants | No por defecto | Sí | Alto |
| Gestionar documentos | `/tenants/{tenantId}/documents` | CRUD documental | No | Sí | Alto |
| Gestionar proveedores | `/tenants/{tenantId}/suppliers` | CRUD proveedor | No | Sí | Alto |
| Gestionar auditorías | `/tenants/{tenantId}/audit-management` | Programas, planes, hallazgos | No | Sí | Alto |
| Gestionar CAPA | `/tenants/{tenantId}/capas` | Acciones correctivas | No | Sí | Alto |
| Gestionar riesgos | `/tenants/{tenantId}/risks` | Matriz/controles/tratamientos | No | Sí | Alto |
| Gestionar indicadores | `/tenants/{tenantId}/indicators` | KPIs/mediciones | No | Sí | Alto |
| Ejecutar reportes operativos | `/tenants/{tenantId}/reports` | Ejecutar/exportar/programar | No por defecto | Sí | Alto |
| Storage tenant | `/tenants/{tenantId}/storage` | Providers y archivos | Providers puede ser compartido; archivos no | Sí | Alto |
| Notificaciones tenant | `/tenants/{tenantId}/notifications` | Templates, mensajes, tracking | Config global no; templates tenant sí | Sí | Medio-alto |
| Enterprise workspaces | `/tenants/{tenantId}/enterprise-workspaces` | Items operativos | No | Sí | Alto |

## Funcionalidades que deben ser únicamente SuperAdmin

El SuperAdmin debe administrar:

- Gestión de tenants.
- Alta/baja de tenants.
- Suspender/reactivar tenants.
- Planes, suscripciones y límites.
- Licencias globales.
- Facturación global.
- Catálogo de módulos/feature flags.
- Límites de usuarios y almacenamiento.
- Configuración global de correo/proveedores.
- Configuración IA global.
- Integraciones globales.
- Auditoría global redacted.
- Logs globales.
- Monitoreo, métricas y salud de servidores.
- Backups/migraciones/versiones.
- Seguridad global: políticas base, OAuth/SSO global, API keys globales.
- Marketplace/configuración del sistema.

## Funcionalidades incorrectamente accesibles por SuperAdmin

Por el bypass global, el SuperAdmin puede acceder a funciones que deberían pertenecer al tenant:

- Usuarios internos del tenant.
- Roles y permisos tenant-scoped.
- Documentos.
- Proveedores.
- Auditorías operativas.
- CAPA.
- Riesgos.
- Indicadores.
- Reportes operativos.
- Archivos y descargas.
- Plantillas/notificaciones tenant.
- Workflows y workspaces operativos.

## Recomendación arquitectónica

Separar tres identidades:

1. `PlatformAdmin`: administra plataforma, tenants, licencias, infraestructura, feature flags y observabilidad.
2. `TenantAdmin`: administra su tenant, usuarios, roles, branding, seguridad tenant y módulos operativos.
3. `SupportOperator` o `BreakGlassAdmin`: acceso temporal, justificado, auditado y aprobado para soporte dentro de tenant.

## Plan de migración por fases

### Fase 1: Bajo riesgo, alto impacto

- Quitar `SuperAdmin Platform` del menú `Operations`; dejarlo solo en consola de plataforma.
- Separar permisos `SUPERADMIN.*` de `TENANT.*` en UI.
- Ocultar módulos operativos al SuperAdmin si está en contexto plataforma.
- Documentar soporte break-glass.

### Fase 2: Seguridad de autorización

- Reemplazar bypass universal `HasPlatformSuperAdmin` por políticas explícitas.
- Hacer que `SuperAdmin` no satisfaga `TENANT.*` por defecto.
- Crear policy `PlatformCanAccessTenantForSupport` con claims temporales.

### Fase 3: Tenant context fuerte

- Bloquear `ApiContext.TenantId` para SuperAdmin salvo claim de soporte activo.
- Registrar reason/correlation/expiration para soporte.
- Agregar pruebas IDOR por tenant.

### Fase 4: UX enterprise

- Crear consola SuperAdmin minimalista.
- Mover operaciones de negocio al TAC.
- Crear modo "Entrar como soporte" separado, visible y auditado.

### Fase 5: Observabilidad y auditoría

- Redactar datos sensibles de auditoría global.
- Crear exportaciones globales agregadas.
- Agregar alertas por acceso cross-tenant.

