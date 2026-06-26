# Compliance 360 - Tenant Management Enterprise Audit

Fecha: 2026-06-25

Modo: solo lectura.  
Alcance: dominio, application, infrastructure, API, frontend, base de datos, permisos, seguridad, auditoría, UI/UX, multitenancy e integraciones.

## Respuesta Principal

### ¿Un Tenant puede editarse actualmente?

Respuesta corta: **sí, pero solo parcialmente**.

La implementación actual **no permite editar el Tenant como entidad comercial completa**. No existe una operación general para modificar `Tenant.Name`, `Tenant.Slug`, razón social, datos fiscales, dirección, país, teléfono, email, website, moneda, dominios, facturación ni campos equivalentes.

Lo que sí existe actualmente es edición de subconfiguraciones asociadas al tenant:

- `TenantSettings`: cultura, zona horaria, MFA requerido, retención documental.
- `TenantBranding`: nombre visual, logo, color primario, color secundario.
- `Subscription`: plan, máximo de usuarios, máximo de storage.
- `Tenant.Status`: activar y suspender.
- `Company`: agregar empresa al tenant.

Evidencia:

- Dominio: `src/Compliance360.Domain/TenantManagement/TenantManagementModels.cs`
- Application: `src/Compliance360.Application/TenantManagement/TenantManagementService.cs`
- Contratos: `src/Compliance360.Application/TenantManagement/TenantManagementContracts.cs`
- API: `src/Compliance360.Web/Api/FoundationEndpoints.cs`
- DTOs API: `src/Compliance360.Web/Api/ApiContracts.cs`
- Mapping DB: `src/Compliance360.Infrastructure/Persistence/Compliance360DbContext.cs`
- Repositorio EF: `src/Compliance360.Infrastructure/TenantManagement/EfTenantManagementRepository.cs`
- UI: `src/Compliance360.Web/wwwroot/app.js`

## Evidencia De Modelo Actual

### Entidad `Tenant`

Campos reales:

- `Id`
- `Name`
- `Slug`
- `Status`
- `Settings`
- `Branding`
- `Subscription`
- `Companies`
- `CreatedAtUtc`
- `UpdatedAtUtc`

Reglas/invariantes actuales:

- `Name` obligatorio, máximo 180 caracteres.
- `Slug` obligatorio, máximo 80 caracteres, se normaliza a minúsculas en constructor.
- `Slug` único en base de datos.
- Estado inicial: `Draft`.
- `Activate()` no permite activar tenants `Archived`.
- `Suspend()` solo permite suspender tenants `Active`.
- `UpdateSettings()` y `UpdateBranding()` validan que el objeto pertenezca al mismo `TenantId`.

### Tablas Reales En Base De Datos

Columnas observadas vía `information_schema`:

- `tenants`: `Id`, `Name`, `Slug`, `Status`, `CreatedAtUtc`, `UpdatedAtUtc`.
- `tenant_settings`: `Id`, `Culture`, `TimeZone`, `RequireMfa`, `DocumentRetentionDays`, `CreatedAtUtc`, `UpdatedAtUtc`, `TenantId`.
- `tenant_branding`: `Id`, `DisplayName`, `LogoUri`, `PrimaryColor`, `SecondaryColor`, `CreatedAtUtc`, `UpdatedAtUtc`, `TenantId`.
- `subscriptions`: `Id`, `Plan`, `Status`, `MaxUsers`, `MaxStorageGb`, `ExpiresOn`, `CreatedAtUtc`, `UpdatedAtUtc`, `TenantId`.
- `companies`: `Id`, `LegalName`, `TaxIdentifier`, `CountryCode`, `IsActive`, `CreatedAtUtc`, `UpdatedAtUtc`, `TenantId`.

## CRUD Actual

| Operación | Existe | Evidencia | Observación |
|---|---:|---|---|
| Crear Tenant | Sí | `POST /api/v1/tenants` -> `CreateTenantAsync` | Crea `Tenant` en `Draft` con settings, branding y subscription default. |
| Consultar Tenant individual | No | No hay `MapGet` en grupo `/tenants` | El repositorio sí tiene `GetByIdAsync`, pero no está expuesto como endpoint. |
| Listar Tenants | No | No hay `GET /api/v1/tenants` | Bloquea operación SuperAdmin/Support tipo Enterprise. |
| Editar `Name` / `Slug` | No | No hay comando/método `UpdateTenant`, `Rename`, `ChangeSlug` | El dominio no expone métodos para cambiar identidad comercial. |
| Editar settings | Sí | `PUT /api/v1/tenants/{tenantId}/settings` | Cultura, zona horaria, MFA requerido, retención documental. |
| Editar branding | Sí | `PUT /api/v1/tenants/{tenantId}/branding` | DisplayName, logo, colores. |
| Editar suscripción | Sí | `PUT /api/v1/tenants/{tenantId}/subscription` | Plan, máximo usuarios, máximo storage. |
| Agregar empresa | Sí | `POST /api/v1/tenants/{tenantId}/companies` | No hay edición ni baja lógica de empresa por API. |
| Suspender | Sí | `POST /api/v1/tenants/{tenantId}/suspend` | Solo si estado actual es `Active`. |
| Reactivar | Sí | `POST /api/v1/tenants/{tenantId}/activate` | No permite activar si está `Archived`. |
| Eliminar lógico | No | No hay endpoint ni servicio | Existe enum `Archived`, pero no hay operación para archivar. |
| Eliminación física | No | No hay endpoint ni servicio | Correcto para SaaS Enterprise salvo proceso administrativo extremo. |
| Restauración | No | No hay endpoint ni servicio | No hay flujo de restore/unarchive. |

## Campos Del Tenant

| Campo | Existe | Editable | Debe ser editable | Justificación |
| ----- | ------ | -------- | ----------------- | ------------- |
| TenantId / `Id` | Sí | No | No | Identificador técnico; debe ser inmutable. |
| Name | Sí | No | Sí, con control | Nombre del tenant existe, pero no hay método de edición. En Enterprise puede requerir corrección legal/comercial. |
| Slug | Sí | No | Solo excepcional | Es único y puede afectar URLs/domains/integraciones; debería ser inmutable o editable con workflow especial. |
| CompanyName / LegalName | Parcial, en `Company.LegalName` | No por API de edición | Sí | Existe al agregar company, no como dato principal del tenant ni editable. |
| CommercialName | Parcial, `TenantBranding.DisplayName` | Sí | Sí | Sirve como nombre visual; no sustituye razón social. |
| TaxId | Parcial, `Company.TaxIdentifier` | No por API de edición | Sí | Existe en Company, no en Tenant; debería poder actualizarse con auditoría. |
| Logo | Sí, `TenantBranding.LogoUri` | Sí | Sí | Branding editable por endpoint. |
| Brand Colors | Sí, `PrimaryColor`, `SecondaryColor` | Sí | Sí | Branding editable. |
| Address | No | No | Sí | Falta para SaaS B2B real. |
| Country | Parcial, `Company.CountryCode` | No por API de edición | Sí | Solo existe al crear company. |
| City | No | No | Sí | Falta información general/fiscal. |
| Phone | No | No | Sí | Falta información administrativa. |
| Email | No | No | Sí | Falta email administrativo/comercial del tenant. |
| Website | No | No | Sí | Campo típico enterprise. |
| TimeZone | Sí, `TenantSettings.TimeZone` | Sí | Sí | Editable por settings. |
| Language | Parcial, `TenantSettings.Culture` | Sí | Sí | No hay campo separado idioma/locale; culture cubre parcialmente. |
| Currency | No | No | Sí | Necesario para billing/comercial futuro. |
| SMTP Configuration | Sí, como Notification Provider tenant-scoped | Sí por endpoints de notifications | Sí, en sección separada | No pertenece directamente a `Tenant`, pero sí está aislado por `TenantId`. |
| Storage Provider | Sí, `StorageProviderConfiguration` tenant-scoped | Sí | Sí, con permisos especiales | Configuración separada por tenant. |
| Notification Provider | Sí, `NotificationProviderConfiguration` tenant-scoped | Sí | Sí, con permisos especiales | Configuración separada por tenant. |
| Plan | Sí, `Subscription.Plan` | Sí | Sí, SuperAdmin/Billing | Editable por subscription endpoint. |
| License | Parcial | Parcial | Sí | Hay plan/límites, pero no licencia contractual, fechas, seats usados, estado comercial. |
| Max Users | Sí, `Subscription.MaxUsers` | Sí | Sí, SuperAdmin/Billing | Editable por subscription endpoint. |
| Max Storage | Sí, `Subscription.MaxStorageGb` | Sí | Sí, SuperAdmin/Billing | Editable por subscription endpoint. |
| Status | Sí, `Tenant.Status` | Parcial | Sí, con autorización especial | Activar/suspender existe; archive/restore no. |
| CreatedAt | Sí, `CreatedAtUtc` | No | No | Debe ser inmutable. |
| UpdatedAt | Sí, `UpdatedAtUtc` | Sistema | No manual | Debe actualizarlo infraestructura. |
| CreatedBy | No | No | Sí | Audit log conserva actor de creación, pero Tenant no tiene `CreatedBy`. |
| RequireMfa | Sí | Sí | Sí, con autorización especial | Impacta seguridad de todo el tenant. |
| DocumentRetentionDays | Sí | Sí | Sí, con control | Impacta compliance y retención. |
| SubscriptionStatus | Sí | No por API directa | Sí, Billing/Support | `MarkActive()` existe en dominio, pero no hay endpoint dedicado. |
| ExpiresOn | Sí | No | Sí, Billing | Falta endpoint de gestión. |

## Reglas De Negocio Recomendadas

### Inmutables

- `TenantId`
- `CreatedAtUtc`
- Slug, salvo proceso excepcional con validación de dominios/integraciones.
- Historial de auditoría.

### Editables Con Flujo Normal

- Nombre comercial/display name.
- Logo.
- Colores.
- TimeZone.
- Culture/idioma.
- Información general: dirección, país, ciudad, teléfono, email, website.
- Datos de compañía asociados, con controles fiscales.

### Editables Con Autorización Especial

- Nombre legal.
- TaxId.
- Slug.
- Estado del tenant.
- Plan/licencia/límites.
- Storage provider.
- Notification/SMTP provider.
- RequireMfa.
- Retención documental.
- Dominios personalizados.

## Seguridad Y Matriz De Roles

La implementación real no usa una matriz hardcoded por rol. Usa políticas basadas en claims de permiso.

Evidencia:

- `PermissionPolicies.TenantManage` requiere claim `permission = TENANT.MANAGE`.
- Los endpoints de `/tenants` usan `.RequireAuthorization(PermissionPolicies.TenantManage)`.
- `ApiContext.TenantId` valida que el `tenant_id` del token coincida con la ruta, excepto si el usuario tiene rol `SuperAdmin`.

| Rol | Puede editar Tenant hoy | Condición real | Riesgo/observación |
|---|---:|---|---|
| SuperAdmin | Sí, si tiene `TENANT.MANAGE` | Además puede operar cross-tenant por excepción de rol en `ApiContext` | Correcto para plataforma, pero falta UI/listado global. |
| Tenant Admin | Sí, si tiene `TENANT.MANAGE` | Solo dentro de su `tenant_id` | Puede cambiar settings/branding/subscription si se le concede permiso; demasiado amplio para suscripción/licencia. |
| Consultora | Depende | Si recibe `TENANT.MANAGE` | No hay separación funcional por rol consultora. |
| Support | Depende | Si recibe `TENANT.MANAGE` | No hay rol soporte con permisos limitados/read-only. |
| Otros roles | Depende | Cualquier rol con claim `TENANT.MANAGE` | Permiso demasiado grueso para operaciones sensibles. |

Riesgo principal: `TENANT.MANAGE` cubre creación, empresas, estado, settings, branding y suscripción. Para Enterprise debería separarse en permisos como `TENANT.READ`, `TENANT.CREATE`, `TENANT.UPDATE`, `TENANT.STATUS`, `TENANT.BILLING`, `TENANT.BRANDING`, `TENANT.SECURITY`, `TENANT.INTEGRATIONS`.

## Auditoría

### Lo que existe

Existen dos mecanismos:

1. Auditoría manual en `TenantManagementService.AppendAuditAsync`.
2. Interceptor EF global `AuditSaveChangesInterceptor` que registra cambios de entidades con snapshot antes/después.

El interceptor captura:

- Usuario.
- Tenant.
- IP.
- UserAgent.
- CorrelationId.
- RequestId.
- SessionId.
- Valores anteriores.
- Valores nuevos.

Evidencia:

- `src/Compliance360.Infrastructure/Audit/AuditSaveChangesInterceptor.cs`
- `src/Compliance360.Web/Audit/AuditContextMiddleware.cs`
- `src/Compliance360.Domain/Audit/AuditLog.cs`

### Debilidad importante

El servicio `TenantManagementService` usa `AuditAction.Updated` genérico para activar/suspender/settings/branding/subscription. `AuditLog.InferCategory(AuditAction.Updated)` cae en categoría `System`, no `TenantManagement`. Existe `AuditAction.TenantChanged`, pero no se usa en el servicio de tenant.

Además, la auditoría manual creada por `AuditLog.Create(...)` no incluye before/after, IP ni CorrelationId; esos datos vienen mejor cubiertos por el interceptor EF, no por la auditoría manual del servicio.

Evaluación:

| Requisito | Estado |
|---|---|
| Cada modificación queda registrada | Parcial/Sí, por interceptor EF y audit manual |
| Valor anterior | Sí por interceptor EF |
| Valor nuevo | Sí por interceptor EF |
| Usuario | Sí por middleware/interceptor; manual solo userId |
| Fecha | Sí |
| IP | Sí por interceptor/middleware |
| CorrelationId | Sí por middleware/interceptor |
| Categoría TenantManagement precisa | No, acciones usan `Updated` genérico |

## Multitenancy

Fortalezas:

- La mayoría de entidades operativas implementan o usan `TenantId`.
- Repositorios filtran por `TenantId`.
- Índices únicos incluyen `TenantId` donde corresponde, por ejemplo company tax id, users/email, roles, storage providers.
- `ApiContext.TenantId` bloquea acceso cross-tenant salvo `SuperAdmin`.
- Storage providers y notification providers están separados por `TenantId`.

Impacto de cambios:

| Cambio | Impacto esperado | Estado actual |
|---|---|---|
| Nombre del tenant | Bajo/medio | No editable; evita inconsistencias pero impide correcciones. |
| Slug | Alto | No editable; correcto como default seguro. |
| Logo/colores | Solo tenant | Editable en `tenant_branding`, aislado por `TenantId`. |
| Storage provider | Solo tenant | Editable por módulo storage, aislado por `TenantId`. |
| SMTP/Notification provider | Solo tenant | Editable por módulo notifications, aislado por `TenantId`. |
| Plan/licencia | Tenant | Editable en subscription, pero permiso demasiado amplio. |

## UI/UX

No existe una pantalla dedicada de Tenant Management.

Lo que existe en frontend:

- Login prellenado con tenant local.
- Shell muestra `Tenant: shortId(...)`.
- Pantalla `configuration` orientada a Provider Administration: storage providers, notification providers, health/failover.
- Enterprise workspace `configuration` existe, pero no es una edición real del tenant.

No existe UI para:

- Listar tenants.
- Ver ficha de tenant.
- Editar información general.
- Editar branding desde formulario dedicado.
- Editar seguridad de tenant.
- Editar licenciamiento.
- Gestionar dominios.
- Ver auditoría filtrada por tenant desde ficha.
- Ver billing/facturación.
- Gestionar empresa principal.

## Secciones De Configuración Enterprise

| Sección esperada | Existe actualmente | Observación |
|---|---:|---|
| Información General | No | Faltan campos y pantalla. |
| Branding | Backend sí, UI dedicada no | Endpoint existe; UI no expone formulario real. |
| Seguridad | Parcial | `RequireMfa` existe; no hay pantalla tenant security. |
| Usuarios | Parcial | RBAC/MFA endpoints existen; no hay gestión completa de usuarios en Tenant Management. |
| Licenciamiento | Backend parcial | Plan/max users/max storage existen; no hay UI ni modelo comercial completo. |
| Storage | Sí | Provider Administration existe. |
| Notificaciones | Sí | Provider Administration existe. |
| Integraciones | Parcial | Storage/notifications; falta dominios, SSO, webhooks, API keys. |
| Dominios | No | No hay modelo ni endpoint. |
| Auditoría | Parcial | Audit Trail global existe; no ficha de tenant. |
| Configuración Regional | Backend sí | Culture/timezone; UI dedicada no. |
| Facturación | No | Futuro. |
| Estado del Tenant | Backend parcial | Activate/suspend; no archive/restore ni UI. |

## Comparación Enterprise

Comparado con Salesforce, Microsoft 365, Atlassian, ServiceNow, SAP y Oracle Cloud:

| Dimensión | Estado actual | Gap frente a Enterprise |
|---|---|---|
| Modelo de tenant | Básico funcional | Falta perfil empresarial/legal completo. |
| Configuración | Parcial | Falta consola organizada por secciones. |
| Seguridad | Correcta base RBAC | Permiso `TENANT.MANAGE` demasiado amplio. |
| Auditoría | Buena base técnica | Acciones/categorías no son suficientemente semánticas. |
| Multitenancy | Buena base | Falta consola SuperAdmin para gestión cross-tenant. |
| UI/UX admin | Insuficiente | No hay ficha ni edición de tenant. |
| Operación SaaS | Parcial | Falta lifecycle completo: trial, active, suspended, archived, restore, billing. |
| Integraciones | Parcial | Storage/notifications sí; dominios/SSO/API keys/webhooks no. |

## Readiness Score

| Área | Score | Justificación |
|---|---:|---|
| Arquitectura | 72/100 | Buen DDD/Clean Architecture, pero agregado Tenant incompleto para SaaS real. |
| Backend | 68/100 | Crear/configurar/suspender funciona; falta read/list/update/delete lógico/restore. |
| Frontend | 35/100 | No hay pantalla real Tenant Management. |
| UX/UI | 32/100 | Sin consola dedicada por secciones. |
| Seguridad | 66/100 | RBAC existe, pero permisos son demasiado gruesos. |
| Auditoría | 70/100 | Interceptor sólido; acciones de tenant no son semánticas. |
| Multitenancy | 78/100 | Buen aislamiento técnico por TenantId. |
| Configuración | 58/100 | Settings/branding/subscription/providers existen de forma dispersa. |
| Operación | 45/100 | Falta lifecycle enterprise, soporte, restore, dominios, billing. |
| Producto Comercial | 38/100 | Falta perfil comercial/legal/licenciamiento usable por clientes. |

Score global estimado: **56/100**

Clasificación: **No listo para clientes Enterprise como módulo Tenant Management completo**.

## Recomendaciones Priorizadas

### Prioridad P0 - Indispensable

- Crear endpoint y servicio para consultar tenant por ID y listar tenants para SuperAdmin.
- Crear operación de edición general separada de settings/branding/subscription.
- Separar permisos: read/create/update/status/billing/branding/security/integrations.
- Crear pantalla Tenant Management con ficha del tenant.
- Implementar auditoría semántica con `TenantChanged` y snapshots claros por operación.

### Prioridad P1 - Alta

- Agregar campos empresariales: razón social, nombre comercial, tax id, dirección, país, ciudad, teléfono, email, website, moneda.
- Implementar lifecycle completo: Draft, Trial, Active, Suspended, Archived, Restore.
- Agregar dominios personalizados.
- Agregar ownership: createdBy, updatedBy, supportOwner, accountOwner.
- Agregar validaciones de color, URL de logo, culture/timezone válidos.

### Prioridad P2 - Media

- Agregar SSO/SAML/OIDC por tenant.
- Agregar API keys/webhooks por tenant.
- Agregar consola de billing/facturación futura.
- Agregar historial visual de cambios por tenant.
- Agregar pestaña de health/readiness por tenant.

## Conclusión

1. **¿El Tenant puede editarse actualmente?**  
   Parcialmente. Se pueden editar settings, branding, subscription y status, pero no la entidad Tenant principal (`Name`, `Slug`) ni una ficha empresarial completa.

2. **¿Debería poder editarse?**  
   Sí. En un SaaS Enterprise real debe poder editarse la información comercial, branding, regionalización, seguridad, licenciamiento e integraciones, con permisos diferenciados y auditoría completa.

3. **¿Qué campos deben ser editables?**  
   DisplayName, logo, colores, timeZone, culture/language, información general/legal, datos de compañía, email/teléfono/website, plan/límites con autorización especial, storage/notification providers, requireMfa y retención documental.

4. **¿Qué campos nunca deben modificarse?**  
   `TenantId`, `CreatedAtUtc`, auditoría histórica. `Slug` debería ser inmutable por defecto y editable solo mediante proceso especial.

5. **¿El módulo está listo para clientes Enterprise?**  
   No como módulo Tenant Management completo. Tiene buena base técnica, pero carece de consola administrativa, CRUD completo, modelo comercial/legal y permisos finos.

6. **¿Qué mejoras son indispensables antes de producción?**  
   Listado/consulta de tenants, edición general controlada, UI dedicada, permisos granulares, auditoría semántica, lifecycle completo y campos empresariales/fiscales mínimos.

## Decisión Final

Tenant Management actual: **NO GO como módulo Enterprise completo**.  
Base técnica: **aprovechable**.  
Acción recomendada: evolucionar a una consola Tenant Administration con permisos, secciones y lifecycle enterprise antes de exponerlo a clientes reales.
