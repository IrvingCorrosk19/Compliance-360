# Matriz de permisos — Administrador del Tenant

**Fuente:** `RoleCatalog.cs` + `app.js` (permisos de ruta y políticas de la API).

## Permisos del rol (13)

| Código | Uso principal |
|--------|----------------|
| TENANT.READ | Paneles y portales de solo lectura |
| TENANT.UPDATE | Información general |
| TENANT.BRANDING | Identidad visual |
| TENANT.BILLING | Licenciamiento |
| TENANT.INTEGRATIONS | Integraciones (según endpoint) |
| TENANT.HEALTH | Centro de salud |
| TENANT.BACKUP | Registrar respaldos |
| TENANT.USERS | Crear/invitar, desbloquear, desactivar, cerrar sesiones |
| TENANT.ROLES | Roles del tenant |
| IDENTITY.MANAGE | Identidad de usuarios |
| RBAC.MANAGE | Crear roles/permisos y asignar |
| TENANT.AUDIT | Auditoría del tenant + exportar |
| AUDIT.READ | Bitácora de auditoría |

## Matriz por pantalla

| Módulo | Pantalla | Ver | Crear | Editar | Eliminar | Exportar | Configurar | Permiso técnico |
|--------|----------|-----|-------|--------|----------|----------|------------|-----------------|
| Acceso | Inicio de sesión | ✓ | — | — | — | — | — | público |
| Comando | Panel ejecutivo | ✓ | — | — | — | — | — | TENANT.READ |
| Comando | Panel de cumplimiento | ✓ | — | — | — | — | — | TENANT.READ |
| Comando | Bitácora de auditoría | ✓ | — | — | — | ✓ | — | AUDIT.READ / TENANT.AUDIT |
| Administración | Información general | ✓ | — | ✓ | — | — | ✓ | TENANT.UPDATE |
| Administración | Identidad visual | ✓ | — | ✓ | — | — | ✓ | TENANT.BRANDING |
| Administración | Seguridad | ✓ vista | — | ✗ API | — | — | ✗ | TENANT.SECURITY (no tiene) |
| Administración | Usuarios | ✓ | ✓ | estado | — | — | ✓ | TENANT.USERS |
| Administración | Roles y permisos | ✓ | ✓ | asignar | — | — | ✓ | RBAC.MANAGE |
| Administración | Licenciamiento | ✓ | — | ✓ | — | — | ✓ | TENANT.BILLING |
| Administración | Dominios | ✓ vista | ✗ | ✗ | ✗ | — | ✗ | TENANT.DOMAINS |
| Administración | SSO | ✓ vista | ✗ | ✗ | — | — | ✗ | TENANT.SSO |
| Administración | Claves de API | ✓ vista | ✗ | ✗ | ✗ | — | ✗ | TENANT.API_KEYS |
| Administración | Webhooks | ✓ vista | ✗ | ✗ | ✗ | — | ✗ | TENANT.WEBHOOKS |
| Administración | Almacenamiento | ✓ stub | ✗ | ✗ | — | — | ✗ | TENANT.STORAGE |
| Administración | Notificaciones | ✓ stub | ✗ | ✗ | — | — | ✗ | TENANT.NOTIFICATIONS |
| Administración | Salud y respaldos | ✓ | respaldo | — | — | — | ✓ | TENANT.HEALTH/BACKUP |
| Administración | Auditoría | ✓ | — | — | — | ✓ | — | TENANT.AUDIT |
| Administración | Estado | ✓ vista | — | ✗ | — | — | ✗ | PLATFORM.TENANT.STATUS |
| Empresa | Plantillas / Regulatorio / Capacitaciones / Portal | ✓ parcial | var. | — | — | — | — | TENANT.READ/UPDATE |

✓ = permitido · ✗ = bloqueado · vista = visible pero la API rechaza la operación
