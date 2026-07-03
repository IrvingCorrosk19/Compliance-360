# SuperAdmin Menu Redesign

## Problema actual

El menú actual mezcla capacidades de plataforma con módulos operativos tenant-scoped. `SuperAdmin Platform` aparece en el menú `Operations`, lo que aumenta la confusión entre:

- Administrar la plataforma SaaS.
- Administrar datos y procesos de un tenant.

En una arquitectura enterprise, el SuperAdmin debe entrar a un **control plane** separado y minimalista. Los módulos como Document Management, Suppliers, CAPA, Risks, Indicators y Reports pertenecen al **tenant data plane**.

## Riesgos UX actuales

| Riesgo | Impacto |
|---|---|
| Menú sobrecargado | El SuperAdmin puede interpretar que debe operar módulos tenant |
| Plataforma en Operations | Mezcla conceptual control plane/data plane |
| Acceso a TAC desde listado sin modo soporte | Puede parecer navegación normal a datos tenant |
| Tabs excesivos | Dificulta encontrar acciones clave como Crear Tenant |
| Métricas de negocio globales | Puede inducir a drill-down operativo no deseado |

## Principios de rediseño

1. Separar visualmente plataforma y tenant.
2. Reducir el menú SuperAdmin a tareas globales.
3. Hacer "Crear Tenant" obvio en `Tenants`.
4. El acceso a TAC debe ser explícito como administración/soporte, no como navegación normal.
5. Módulos operativos deben desaparecer del SuperAdmin estándar.
6. Si se requiere soporte, activar modo temporal con banner y auditoría.

## Menú SuperAdmin recomendado

```text
Platform Administration
  Overview
  Tenants
  Plans & Licenses
  Modules & Feature Flags
  Billing

Platform Operations
  Providers
  Observability
  Global Audit
  Backups
  Database
  DevOps

Platform Security
  Security Policies
  SSO / OAuth
  Global API Keys
  Support Access

Platform Intelligence
  AI Configuration
  Marketplace
```

## Navegación propuesta

### Overview

Contenido:

- Estado global.
- Tenants activos/suspendidos/trial.
- Incidentes.
- Health global.
- Alertas plataforma.

No debe incluir:

- Listados operativos de documentos, CAPA, riesgos, proveedores o reportes.

### Tenants

Contenido:

- Botón `Crear Tenant` visible arriba.
- Tabla tenant fleet.
- Estado tenant.
- Plan/licencia.
- Acciones:
  - Ver resumen.
  - Suspender/activar.
  - Cambiar plan.
  - Abrir configuración tenant.
  - Solicitar acceso soporte.

Acción peligrosa:

- `Abrir TAC` debe renombrarse a `Administrar configuración` o `Acceso soporte`.

### Plans & Licenses

Contenido:

- Planes.
- Límites.
- Storage.
- Usuarios.
- Entitlements.
- Renovaciones.

### Modules & Feature Flags

Contenido:

- Catálogo de módulos.
- Habilitación por tenant/plan.
- Feature flags globales.

### Billing

Contenido:

- Estado facturación.
- Periodos.
- Contratos.
- Métodos externos.

### Providers

Contenido:

- SMTP global.
- Storage global.
- Defaults por región.
- Health de providers.

Separar:

- Provider global administrado por plataforma.
- Provider tenant administrado por TenantAdmin.

### Observability

Contenido:

- Health.
- Métricas.
- Tracing.
- Jobs.
- Retry queues.

### Global Audit

Contenido:

- Eventos globales.
- Eventos cross-tenant redacted.
- Exportaciones controladas.

### Support Access

Nuevo módulo recomendado:

- Buscar tenant.
- Solicitar acceso.
- Motivo obligatorio.
- Duración.
- MFA.
- Aprobación.
- Banner visible.
- Cerrar acceso.
- Log completo.

## Menú Tenant Admin recomendado

```text
Tenant Administration
  Dashboard
  Company Profile
  Users
  Roles & Permissions
  Branding
  Security
  SSO
  API Keys
  Webhooks
  Audit Trail

Operations
  Documents
  Suppliers
  Audit Management
  CAPA
  Risks
  Indicators
  Reports
  Storage
  Notifications
```

## Cambios concretos sugeridos

| Cambio | Prioridad | Riesgo |
|---|---:|---:|
| Quitar `SuperAdmin Platform` de `Operations` | Alta | Bajo |
| Mostrar `Crear Tenant` en tab `Tenants` | Alta | Bajo |
| Renombrar `Abrir TAC` a `Acceso soporte` si usuario es SuperAdmin | Alta | Medio |
| Ocultar módulos operativos para rol PlatformAdmin | Alta | Medio |
| Crear sidebar dedicado para `/superadmin-platform` | Media | Medio |
| Crear módulo `Support Access` | Alta | Medio |
| Separar métricas plataforma vs negocio | Media | Medio |

## Wireframe conceptual

```text
-------------------------------------------------
Compliance 360 Platform Admin
-------------------------------------------------
Overview | Tenants | Licenses | Modules | Security
Providers | Observability | Audit | Backups | DevOps

[ Global Health ] [ Active Tenants ] [ Incidents ]

Tenants
-------------------------------------------------
[ + Create Tenant ] [ Search tenant... ]

Tenant             Status     Plan       Actions
Alimentos Premium  Active     Starter    View | Plan | Suspend | Support Access
Cliente Demo       Trial      Starter    View | Plan | Activate | Support Access
```

## Resultado esperado

El SuperAdmin queda orientado a administración SaaS de plataforma y deja de parecer un usuario operativo con acceso a todos los módulos tenant.

