# Tenant Admin Responsibility Matrix

## Principio rector

El Tenant Administrator administra la **operación y configuración interna de su tenant**. Su alcance es data plane tenant-scoped, limitado por `tenant_id`, permisos RBAC y políticas del plan/licencia.

## Matriz de responsabilidades recomendadas

| Dominio | Responsabilidad Tenant Admin | Endpoint/Área actual | ¿Debe ser Tenant Admin? | ¿Debe ser SuperAdmin? | Riesgo si lo maneja SuperAdmin |
|---|---|---|---|---|---|
| Perfil de empresa | Nombre, razón social, tax ID, industria, contacto | `/tenants/{tenantId}/general-information` | Sí | No | Medio |
| Empresas/subsidiarias | Agregar compañías del tenant | `/tenants/{tenantId}/companies` | Sí | No | Medio |
| Configuración regional | Cultura, zona horaria, retención documental | `/tenants/{tenantId}/settings` | Sí | Solo límites globales | Bajo |
| Branding tenant | Logo, colores, theme, email corporativo | `/tenants/{tenantId}/branding` | Sí | No por defecto | Medio |
| Seguridad tenant | MFA requerido, lockout, sesiones, IP whitelist | `/tenants/{tenantId}/security` | Sí, dentro de guardrails | Solo baseline global | Medio |
| Usuarios internos | Crear, invitar, deshabilitar, desbloquear | `/tenants/{tenantId}/users` | Sí | No por defecto | Alto |
| MFA de usuarios | Reset MFA tenant user | `/tenants/{tenantId}/users/{userId}/reset-mfa` | Sí | No por defecto | Alto |
| Sesiones usuario | Cerrar sesiones de usuarios del tenant | `/tenants/{tenantId}/users/{userId}/sessions/close` | Sí | No por defecto | Alto |
| Roles tenant | Crear roles, asignar/revocar | `/tenants/{tenantId}/rbac` y `/users/{userId}/roles` | Sí | No por defecto | Alto |
| Permisos tenant | Grants dentro del catálogo permitido | `/tenants/{tenantId}/rbac/permissions/grant` | Sí con límites | Plataforma define catálogo | Alto |
| Dominios tenant | Dominios propios, default, redirects | `/tenants/{tenantId}/domains` | Sí | Plataforma puede aprobar/verificar | Medio |
| SSO tenant | OIDC/SAML/LDAP por tenant | `/tenants/{tenantId}/sso` | Sí | Plataforma define proveedores permitidos | Medio |
| API keys tenant | Credenciales integración tenant | `/tenants/{tenantId}/api-keys` | Sí | No por defecto | Alto |
| Webhooks tenant | URLs, eventos, secrets, reintentos | `/tenants/{tenantId}/webhooks` | Sí | No por defecto | Alto |
| Auditoría tenant | Ver/exportar timeline tenant | `/tenants/{tenantId}/audit-timeline` | Sí | Global solo redacted | Medio |
| Storage providers tenant | Config provider si permitido por plan | `/tenants/{tenantId}/storage/providers` | Sí, si self-service | Plataforma define providers globales | Medio |
| Notification providers tenant | SMTP/Gmail/etc tenant | `/tenants/{tenantId}/notifications/providers` | Sí, si self-service | Plataforma define providers globales | Medio |
| Plantillas notificación | Templates tenant | `/tenants/{tenantId}/notifications/templates` | Sí | No | Medio |
| Historial notificaciones | Tracking/dead letters tenant | `/tenants/{tenantId}/notifications/history` | Sí | Global solo métricas | Medio |
| Documentos | Tipos, categorías, versiones, workflows, aprobaciones | `/tenants/{tenantId}/documents` | Sí | No | Alto |
| Workflows | Flujos tenant, pasos, transiciones | `/tenants/{tenantId}/workflows` | Sí | No | Alto |
| Fichas técnicas | Productos/fichas/certificaciones | `/tenants/{tenantId}/technical-sheets` | Sí | No | Alto |
| Proveedores | Expediente, documentos, evaluación, homologación | `/tenants/{tenantId}/suppliers` | Sí | No | Alto |
| Auditorías operativas | Programas, planes, checklist, hallazgos | `/tenants/{tenantId}/audit-management` | Sí | No | Alto |
| CAPA | Crear, clasificar, raíz, 5 Why, evidencia, cierre | `/tenants/{tenantId}/capas` | Sí | No | Alto |
| Riesgos | Categorías, matriz, evaluación, controles | `/tenants/{tenantId}/risks` | Sí | No | Alto |
| Indicadores | Categorías, KPIs, metas, mediciones | `/tenants/{tenantId}/indicators` | Sí | No | Alto |
| Reportes operativos | Ejecutar, exportar, programar | `/tenants/{tenantId}/reports` | Sí | No | Alto |
| Enterprise workspaces | Items operativos genéricos | `/tenants/{tenantId}/enterprise-workspaces` | Sí | No | Alto |

## Subroles tenant recomendados

| Subrol | Responsabilidad |
|---|---|
| `TenantAdmin` | Configuración general, usuarios, roles, branding, seguridad tenant |
| `TenantSecurityAdmin` | MFA, sesiones, SSO tenant, API keys, webhooks |
| `DocumentController` | Tipos, categorías, documentos, versiones |
| `QualityManager` | Aprobaciones, CAPA, indicadores, reportes de calidad |
| `Auditor` | Programas/planes/checklists/hallazgos |
| `SupplierManager` | Proveedores, documentos, evaluaciones, homologación |
| `CapaManager` | CAPA, causa raíz, acciones, efectividad |
| `RiskManager` | Riesgos, tratamientos, controles, matriz |
| `IndicatorsManager` | KPIs, mediciones, umbrales, tendencias |
| `ReportingManager` | Reportes, filtros, exportaciones, schedules |
| `StorageAdmin` | Storage tenant y metadata permitida |
| `NotificationAdmin` | SMTP/templates/tracking tenant |
| `Viewer` | Lectura tenant-scoped sin creación/edición |

## Reglas de autorización recomendadas

- `TenantAdmin` nunca debe poder acceder a otro `tenantId`.
- `TenantAdmin` no debe satisfacer permisos `SUPERADMIN.*`.
- `TenantAdmin` debe operar solo dentro de su `tenant_id` del JWT.
- Permisos de negocio deben estar separados por módulo y acción.
- RBAC tenant no debe poder crear permisos arbitrarios fuera de catálogo.
- Cambios sensibles deben exigir MFA reciente.

## Separación con SuperAdmin

| Decisión | Dueño |
|---|---|
| Crear tenant | SuperAdmin |
| Suspender tenant | SuperAdmin |
| Cambiar plan/límite | SuperAdmin |
| Crear usuario interno tenant | TenantAdmin |
| Reset MFA usuario tenant | TenantAdmin |
| Configurar SSO tenant | TenantAdmin con guardrails |
| Ver documentos tenant | Tenant roles |
| Ejecutar reporte tenant | Tenant roles |
| Exportar auditoría global | SuperAdmin |
| Exportar auditoría tenant | TenantAdmin / Auditor autorizado |

