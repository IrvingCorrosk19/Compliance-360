# Roadmap SuperAdmin Refactor

## Objetivo

Migrar el rol SuperAdmin hacia una arquitectura SaaS Multi-Tenant enterprise, separando control plane de data plane sin romper funcionalidades existentes.

## Principios de migración

- Cambios graduales.
- Compatibilidad temporal.
- Feature flags para cambios de autorización.
- Auditoría reforzada.
- Pruebas E2E por rol.
- No eliminar funcionalidades operativas hasta tener reemplazo tenant/support.

## Fase 0 - Baseline y pruebas

Duración estimada: 1 semana.

Objetivo:

- Congelar comportamiento actual con pruebas.
- Medir alcance de dependencias.

Acciones:

- Crear pruebas de autorización:
  - SuperAdmin platform endpoints.
  - TenantAdmin solo su tenant.
  - Usuario tenant no accede a otro tenant.
  - SuperAdmin acceso tenant actual documentado como legacy.
- Crear snapshot de menú actual.
- Agregar logs de uso de endpoints tenant por SuperAdmin.

Riesgo:

- Bajo.

Resultado:

- Base segura para refactor.

## Fase 1 - UX y navegación sin romper backend

Duración estimada: 1 semana.

Objetivo:

- Separar visualmente plataforma y tenant.

Acciones:

- Dejar `SuperAdmin Platform` solo en sección plataforma.
- Crear sidebar específico SuperAdmin.
- Mostrar `Crear Tenant` en tab `Tenants`.
- Renombrar `Abrir TAC` a `Acceso soporte` para SuperAdmin.
- Ocultar módulos operativos tenant cuando rol es `PlatformAdmin`.

Riesgo:

- Bajo.

Impacto:

- Alto en claridad UX.

## Fase 2 - Separación de permisos

Duración estimada: 1-2 semanas.

Objetivo:

- Separar `SUPERADMIN.*` de `TENANT.*`.

Acciones:

- Crear roles:
  - `PlatformAdmin`
  - `PlatformBillingAdmin`
  - `PlatformSecurityAdmin`
  - `PlatformOpsAdmin`
  - `SupportOperator`
- Migrar SuperAdmin actual a `PlatformAdmin`.
- Quitar asignaciones tenant-operativas del bootstrap por defecto.
- Mantener feature flag `LegacySuperAdminBypass` temporal.

Riesgo:

- Medio.

Mitigación:

- Mantener fallback temporal por configuración.
- Ejecutar E2E por rol.

## Fase 3 - Eliminar bypass universal

Duración estimada: 2 semanas.

Objetivo:

- Remover `HasPlatformSuperAdmin` como bypass para permisos tenant.

Acciones:

- Cambiar `PermissionPolicies.HasPermission(...)`.
- `PlatformAdmin` solo satisface `SUPERADMIN.*`.
- `TenantAdmin` satisface `TENANT.*`.
- Tests:
  - PlatformAdmin no puede `DOCUMENT.MANAGE`.
  - PlatformAdmin no puede `/tenants/{tenantId}/documents`.
  - PlatformAdmin sí puede `/superadmin/platform-center`.

Riesgo:

- Alto si no hay pruebas suficientes.

Mitigación:

- Feature flag.
- Rollout por entorno.

## Fase 4 - Support Access / Break Glass

Duración estimada: 2-3 semanas.

Objetivo:

- Permitir soporte sin romper aislamiento.

Acciones:

- Crear entidad `SupportAccessSession`.
- Campos:
  - `TenantId`
  - `RequestedByUserId`
  - `ApprovedByUserId`
  - `Reason`
  - `ExpiresAtUtc`
  - `Status`
  - `MfaVerifiedAtUtc`
- Emitir claims temporales:
  - `support_tenant_id`
  - `support_exp`
  - `support_reason_id`
- Banner UI: `Modo soporte activo`.
- Auditoría:
  - `SupportAccessStarted`
  - `SupportActionExecuted`
  - `SupportAccessEnded`

Riesgo:

- Medio-alto.

Mitigación:

- MFA obligatorio.
- Expiración corta.
- Approval workflow.

## Fase 5 - Tenant context estricto

Duración estimada: 1-2 semanas.

Objetivo:

- Fortalecer `ApiContext.TenantId(...)`.

Regla nueva:

```text
route tenantId == JWT tenant_id
OR
route tenantId == support_tenant_id AND support session valid
```

Acciones:

- Modificar `ApiContext.TenantId`.
- Remover excepción directa por `SuperAdmin`.
- Agregar tests IDOR.

Riesgo:

- Alto.

Mitigación:

- Implementar después de Support Access.

## Fase 6 - SuperAdmin Platform API dedicada

Duración estimada: 2 semanas.

Objetivo:

- Evitar que SuperAdmin use endpoints tenant para operaciones platform.

Acciones:

- Crear endpoints dedicados:
  - `POST /superadmin/tenants`
  - `POST /superadmin/tenants/{tenantId}/suspend`
  - `POST /superadmin/tenants/{tenantId}/activate`
  - `PATCH /superadmin/tenants/{tenantId}/plan`
  - `PUT /superadmin/tenants/{tenantId}/license`
- Mantener compatibilidad temporal con `/tenants`.
- Deprecar acceso platform por endpoints tenant.

Riesgo:

- Medio.

## Fase 7 - Observabilidad y redacción de datos

Duración estimada: 1 semana.

Objetivo:

- Reducir exposición global de información tenant.

Acciones:

- Redactar `MetadataJson` en auditoría global.
- Agregar filtros por severidad/categoría.
- Separar:
  - audit técnico global
  - audit operativo tenant
- Export global sin PII por defecto.

Riesgo:

- Medio.

## Fase 8 - Hardening final

Duración estimada: 1-2 semanas.

Acciones:

- Retirar feature flag legacy.
- Retirar permisos tenant del rol PlatformAdmin.
- Documentar runbooks.
- Ejecutar E2E completo:
  - SuperAdmin
  - TenantAdmin
  - SupportOperator
  - Viewer
- Revisión de seguridad.

## Priorización impacto/riesgo

| Prioridad | Cambio | Impacto | Riesgo |
|---:|---|---:|---:|
| 1 | Menú SuperAdmin separado | Alto | Bajo |
| 2 | Mostrar Crear Tenant en Tenants | Alto | Bajo |
| 3 | Instrumentar uso SuperAdmin en tenant endpoints | Alto | Bajo |
| 4 | Separar roles Platform/Tenant | Alto | Medio |
| 5 | Support Access con expiración | Alto | Medio |
| 6 | Eliminar bypass universal | Muy alto | Alto |
| 7 | Tenant context estricto | Muy alto | Alto |
| 8 | APIs platform dedicadas | Alto | Medio |
| 9 | Redacción auditoría global | Medio | Medio |

## Definition of Done

- PlatformAdmin no puede operar datos de negocio tenant por defecto.
- TenantAdmin no puede acceder a otros tenants.
- Soporte temporal queda auditado y expira.
- Menú SuperAdmin no muestra módulos operativos.
- E2E por rol pasa.
- Auditoría global no expone datos sensibles.
- Documentación de roles actualizada.

