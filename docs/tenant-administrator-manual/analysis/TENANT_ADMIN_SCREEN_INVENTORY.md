# Inventario de pantallas — Administrador del Tenant (solo lo operable)

**Sistema:** Compliance 360 · filtro por permiso en `app.js` (`tenantAdminTabs` + `canNavigate`)  
**Fecha:** 2026-07-13

## Menú que verá (Administrador del Tenant)

| Grupo | Ruta | Nombre |
|-------|------|--------|
| Centro de comando | `#/dashboard` | Panel ejecutivo |
| Centro de comando | `#/compliance` | Panel de cumplimiento |
| Centro de comando | `#/audit-trail` | Bitácora de auditoría |
| Empresa | `#/tenant-administration` | Administración del tenant |
| Empresa | `#/template-builder` | Constructor de plantillas (diseñador visual Form Templates) |

## Pestañas del centro de administración (7)

| Pestaña | Permiso |
|---------|---------|
| Información general | TENANT.UPDATE |
| Branding | TENANT.BRANDING |
| Usuarios | TENANT.USERS |
| Roles y permisos | RBAC.MANAGE / TENANT.ROLES |
| Licenciamiento | TENANT.BILLING |
| Salud y respaldos | TENANT.HEALTH / BACKUP |
| Auditoría | TENANT.AUDIT |

## Oculto para este rol (ya no aparece)

API Keys, SSO, Dominios, Seguridad, Webhooks, Storage, Notificaciones, Estado, Regulatory, Training, Customer Portal, SuperAdmin, Documentos, CAPA, etc.
