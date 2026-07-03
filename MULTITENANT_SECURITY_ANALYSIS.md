# Multitenant Security Analysis

## Resumen

Compliance 360 implementa rutas tenant-scoped y validaciones de `tenant_id`, pero el rol `SuperAdmin` introduce un bypass global que reduce el aislamiento lógico. El mayor riesgo no es que falte `TenantId`, sino que el rol de plataforma pueda satisfacer cualquier policy y cualquier `tenantId` por URL.

## Hallazgos principales

| ID | Hallazgo | Evidencia | Severidad | Recomendación |
|---|---|---|---|---|
| SEC-001 | Bypass global por rol `SuperAdmin` | `PermissionPolicies.HasPermission(...)` retorna true si `HasPlatformSuperAdmin(...)` | Alta | Eliminar bypass universal; usar políticas explícitas `SUPERADMIN.*` |
| SEC-002 | SuperAdmin puede saltar tenant claim mismatch | `ApiContext.TenantId(...)` permite mismatch si `HasSuperAdminRole(...)` | Alta | Requerir claim temporal de soporte para acceder a tenant ajeno |
| SEC-003 | SuperAdmin puede operar endpoints de negocio tenant | Policies tenant pasan por bypass | Alta | Separar control plane vs data plane |
| SEC-004 | Menú mezcla plataforma y operaciones | `SuperAdmin Platform` aparece en `Operations` y `Enterprise` | Media | Menú de plataforma minimalista separado |
| SEC-005 | Auditoría global puede incluir metadata tenant | `GetGlobalAuditTimelineAsync` devuelve entity names, ids, user, ip, metadata | Media-alta | Redactar metadata sensible y aplicar scopes |
| SEC-006 | Métricas globales cuentan datos operativos | SuperAdmin repo cuenta documentos, CAPA, riesgos, indicadores | Media | Mostrar agregados sin drill-down ni datos sensibles |
| SEC-007 | RBAC permite crear permisos desde tenant endpoint | `/tenants/{tenantId}/rbac/permissions` | Alta | Catálogo de permisos debe ser platform-owned; tenant solo asigna subset |
| SEC-008 | API keys/webhooks tenant accesibles por bypass | `/tenants/{tenantId}/api-keys`, `/webhooks` | Alta | TenantAdmin o soporte break-glass solamente |
| SEC-009 | Storage/file endpoints tenant accesibles por bypass | `/tenants/{tenantId}/storage/files` | Alta | Bloquear para SuperAdmin estándar |
| SEC-010 | Report exports tenant accesibles por bypass | `/tenants/{tenantId}/reports`, `/capas/export`, `/risks/export` | Alta | Requerir tenant role o soporte temporal |

## Riesgos de fuga entre tenants

### Riesgo 1: Acceso por URL directa

Un `SuperAdmin` autenticado puede cambiar manualmente `{tenantId}` en una URL/API y pasar `ApiContext.TenantId(...)`. Esto es intencional en código actual, pero contrario al principio de mínimo privilegio para plataforma.

Impacto:

- Lectura/modificación de datos tenant.
- Operación accidental en tenant equivocado.
- Exportación de datos de negocio.

Mitigación:

- `SuperAdmin` no debe validar `TenantId` automáticamente.
- Crear flujo explícito "Support Access" con `support_tenant_id`, expiración, reason, approval y audit.

### Riesgo 2: Permisos excesivos por `HasPlatformSuperAdmin`

`SuperAdmin` satisface cualquier permiso, incluyendo `DOCUMENT.MANAGE`, `SUPPLIER.MANAGE`, `CAPA.MANAGE`, `RISK.MANAGE`, `REPORT.MANAGE`, `STORAGE.MANAGE`, etc.

Impacto:

- Escalamiento lateral total.
- Acceso a datos de clientes.
- Riesgo legal/compliance por acceso no autorizado a información tenant.

Mitigación:

- Quitar bypass.
- Asignar explícitamente solo `SUPERADMIN.*`.
- Crear roles técnicos separados.

### Riesgo 3: RBAC tenant administrado desde plataforma

El endpoint RBAC permite crear roles, crear permisos, otorgar permisos y asignar roles por tenant. El SuperAdmin puede usarlo en cualquier tenant.

Impacto:

- Escalamiento persistente dentro del tenant.
- Creación de permisos no gobernados.
- Dificultad de auditoría.

Mitigación:

- Catálogo de permisos immutable/platform-owned.
- TenantAdmin solo asigna roles y permisos permitidos por plan.
- Toda elevación temporal debe expirar.

### Riesgo 4: Auditoría global con metadata operacional

`EfSuperAdminPlatformRepository.GetGlobalAuditTimelineAsync(...)` retorna `MetadataJson`, `EntityName`, `EntityId`, `UserName`, `IpAddress`.

Impacto:

- Posible exposición de PII/metadatos de negocio.

Mitigación:

- Redactar metadata por defecto.
- Mostrar detalle solo con permiso especial y reason.
- Separar audit global técnico de audit tenant operativo.

## IDOR

### Estado actual

La mayoría de endpoints usan:

```text
ApiContext.TenantId(httpContext, tenantId)
```

Esto protege usuarios normales contra cambiar el `tenantId` de la ruta. El riesgo aparece por la excepción `SuperAdmin`.

### Riesgo

Para `SuperAdmin`, cualquier `{tenantId}` es aceptado. Esto es un IDOR permitido por diseño, pero debe tratarse como "support impersonation" y no como comportamiento estándar.

### Control recomendado

- `ApiContext.TenantId` debe exigir:
  - `tenant_id` == route `tenantId`, o
  - claim `support_tenant_id` == route `tenantId`, y
  - claim `support_exp` vigente, y
  - claim `support_reason_id`, y
  - MFA reciente.

## Escalamiento de privilegios

| Vector | Riesgo | Control recomendado |
|---|---|---|
| SuperAdmin crea rol tenant con permisos elevados | Alto | Tenant permissions catalog + approval |
| SuperAdmin asigna rol a usuario tenant | Alto | Solo TenantAdmin; soporte temporal |
| SuperAdmin rota API key tenant | Alto | Bloquear salvo break-glass |
| SuperAdmin descarga archivos tenant | Alto | Bloquear salvo break-glass |
| SuperAdmin exporta reportes tenant | Alto | Bloquear salvo break-glass |

## Aislamiento de datos

### Fortalezas

- Modelo de datos incluye `TenantId` en entidades operativas.
- Endpoints tenant-scoped reciben `tenantId` explícito.
- Servicios operativos construyen queries con `TenantId`.
- JWT incluye `tenant_id`.
- Auditoría registra tenant/user/contexto.

### Debilidades

- No hay filtro global EF Core de tenant.
- Aislamiento depende de disciplina por repositorio/servicio.
- SuperAdmin bypass reduce garantía de aislamiento.
- SuperAdmin repository consulta datos globales sin tenant filter por diseño.

## Recomendaciones técnicas

1. Separar claims:
   - `role=PlatformAdmin`
   - `role=TenantAdmin`
   - `role=SupportOperator`
   - `support_tenant_id`
   - `support_exp`
   - `support_reason`

2. Separar policies:
   - `Platform.*` solo para `/superadmin/*`.
   - `Tenant.*` solo para `/tenants/{tenantId}/*`.
   - `Support.*` para acceso temporal.

3. Eliminar bypass:
   - `HasPlatformSuperAdmin` no debe retornar true para permisos tenant.

4. Agregar pruebas:
   - Tenant user no puede acceder a otro tenant.
   - PlatformAdmin no puede llamar módulos tenant sin soporte.
   - SupportOperator expira.
   - Export tenant data requiere tenant role o support.

5. Auditar:
   - Toda entrada cross-tenant debe crear evento `SupportAccessStarted`, `SupportActionExecuted`, `SupportAccessEnded`.

## Calificación de seguridad multi-tenant

**58 / 100**

La base de tenant scoping es buena, pero el bypass global del rol `SuperAdmin` es incompatible con un modelo enterprise de menor privilegio.

