# Trazabilidad código ↔ manual — Administrador del Tenant

| Pantalla documentada | Ruta | Archivo fuente | Componente / función | Permiso | Acción | Evidencia |
|----------------------|------|----------------|----------------------|---------|--------|-----------|
| Inicio de sesión (correo) | `#/login` | `wwwroot/app.js` | paso de correo | — | identificar | `POST /api/v1/auth/identify` |
| Inicio de sesión (organización) | `#/login` | `app.js` | paso organización | — | elegir org. | radio `organizationId` |
| Inicio de sesión (contraseña) | `#/login` | `app.js` | paso contraseña | — | iniciar sesión | `POST /api/v1/auth/login` |
| Entorno / menú | post-login | `app.js` | `shellView` + `canNavigate` | varios | navegar | `routePermissions` |
| Panel ejecutivo | `#/dashboard` | `app.js` | vista dashboard | TENANT.READ | ver indicadores | módulos / ruta |
| Panel de cumplimiento | `#/compliance` | `app.js` | vista compliance | TENANT.READ | ver | routePermissions |
| Bitácora de auditoría | `#/audit-trail` | `app.js` | búsqueda de auditoría | AUDIT.READ | buscar/exportar | `/audit/search` |
| Centro de administración | `#/tenant-administration` | `app.js` | `renderTenantAdministrationCenter` | TENANT.USERS/ROLES/UPDATE | pestañas | `/administration-center` |
| Información general | pestaña TAC | `tenantGeneralPanel` | `#tenant-general-form` | TENANT.UPDATE | guardar | FoundationEndpoints |
| Identidad visual | pestaña TAC | `tenantBrandingPanel` | `#tenant-branding-form` | TENANT.BRANDING | guardar | |
| Seguridad | pestaña TAC | `tenantSecurityPanel` | `#tenant-security-form` | TENANT.SECURITY | guardar | error 403 para este rol |
| Usuarios | pestaña TAC | `tenantUsersPanel` | `#tenant-user-form` | TENANT.USERS | crear usuarios | |
| Roles y permisos | pestaña TAC | `tenantRbacPanel` | formularios de rol/permiso | RBAC.MANAGE | crear/asignar | |
| Licenciamiento | pestaña TAC | `tenantLicensingPanel` | `#tenant-licensing-form` | TENANT.BILLING | suscripción | |
| Dominios | pestaña TAC | `tenantDomainsPanel` | `#tenant-domain-form` | TENANT.DOMAINS | 403 | |
| SSO | pestaña TAC | `tenantSsoPanel` | `#tenant-sso-form` | TENANT.SSO | 403 | |
| Claves de API | pestaña TAC | `tenantApiKeysPanel` | `#tenant-api-key-form` | TENANT.API_KEYS | 403 | |
| Webhooks | pestaña TAC | `tenantWebhooksPanel` | formulario webhook | TENANT.WEBHOOKS | 403 | |
| Almacenamiento | pestaña TAC | `tenantStoragePanel` | enlace a configuración | TENANT.STORAGE | sin menú | |
| Notificaciones | pestaña TAC | `tenantNotificationsPanel` | enlace a configuración | TENANT.NOTIFICATIONS | sin menú | |
| Salud y respaldos | pestaña TAC | `tenantHealthPanel` | `#tenant-backup-form` | TENANT.BACKUP | registrar respaldo | |
| Auditoría | pestaña TAC | `tenantAuditPanel` | Exportar | TENANT.AUDIT | exportar | |
| Estado | pestaña TAC | `tenantStatePanel` | Activar/Suspender… | PLATFORM.TENANT.STATUS | 403 | |
| Definición del rol | — | `RoleCatalog.cs` | TenantAdministrator | 13 códigos | semillas RBAC | Domain |
| Códigos de permiso | — | `RbacCatalog.cs` | PermissionCatalog | — | — | |

**Permisos del rol (catálogo):**  
`TENANT.READ`, `TENANT.UPDATE`, `TENANT.BRANDING`, `TENANT.BILLING`, `TENANT.INTEGRATIONS`, `TENANT.HEALTH`, `TENANT.BACKUP`, `TENANT.USERS`, `TENANT.ROLES`, `IDENTITY.MANAGE`, `RBAC.MANAGE`, `TENANT.AUDIT`, `AUDIT.READ`.
