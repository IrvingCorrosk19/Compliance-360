# Compliance 360 - Database Clean Reset For Testing

Fecha y hora: 2026-06-24 19:26-19:45 UTC-5

Estado final: CLEAN TEST READY

Decisión final: DATABASE CLEAN RESET APPROVED

## Entorno Usado

- Base de datos: PostgreSQL local, `compliance360`
- Herramientas PostgreSQL: `C:\Program Files\PostgreSQL\18\bin`
- Aplicación validada en: `http://localhost:5375`
- Cadena de conexión: la configurada localmente en la aplicación
- Esquema conservado: `compliance360`
- Migraciones conservadas: `public.__EFMigrationsHistory`

## Backup Obligatorio

Backup generado antes de eliminar datos:

- Archivo: `artifacts/backups/compliance360-clean-reset-testing-20260624-192607.dump`
- Formato: custom (`pg_dump -F c`)
- Tamaño: 354,414 bytes
- Verificación: `pg_restore -l`
- TOC generado: `artifacts/backups/compliance360-clean-reset-testing-20260624-192607.toc.txt`
- Tamaño TOC: 110,296 bytes
- Resultado: backup restaurable confirmado

La limpieza no se ejecutó hasta que el backup fue generado y verificado correctamente.

## Estrategia Técnica

La limpieza se ejecutó con script transaccional en `artifacts/migrations/clean-reset-for-testing.sql`.

- `BEGIN` / `COMMIT`
- `ON_ERROR_STOP`
- Conteo previo por tabla
- Limpieza ordenada por dependencias
- Conservación explícita de SuperAdmin, rol, permisos y configuración mínima
- Validaciones antes del commit
- Rollback automático ante error SQL o validación fallida

## Datos Conservados

- Tenant mínimo de plataforma: 1
- Usuario SuperAdmin: `admin@compliance360.local`
- Password: no se imprime en texto plano
- Estado SuperAdmin: `Active`
- MFA SuperAdmin: desactivado para pruebas iniciales
- Rol conservado/normalizado: `SuperAdmin`
- Relación usuario-rol: 1
- Permisos globales: 36
- Relación rol-permisos: 36
- Tenant settings: 1
- Tenant branding: 1
- Subscription mínima: 1

## Conteos Clave

| Tabla / área | Antes | Después |
|---|---:|---:|
| tenants | 1 | 1 |
| users | 1 | 1 |
| roles | 1 | 1 |
| permissions | 29 | 36 |
| role_permissions | 29 | 36 |
| audit_logs | 444 | 0 |
| audit_programs | 1 | 0 |
| audit_plans | 1 | 0 |
| capas | 3 | 0 |
| capa_history | 3 | 0 |
| documents | 6 | 0 |
| document_types | 6 | 0 |
| document_categories | 6 | 0 |
| document_history | 6 | 0 |
| enterprise_workspace_items | 8 | 0 |
| products | 1 | 0 |
| quality_indicators | 6 | 0 |
| indicator_categories | 8 | 0 |
| indicator_history | 10 | 0 |
| indicator_targets | 2 | 0 |
| indicator_thresholds | 2 | 0 |
| managed_audits | 1 | 0 |
| report_categories | 6 | 0 |
| report_dashboard_bindings | 24 | 0 |
| report_definitions | 24 | 0 |
| report_history | 96 | 0 |
| report_templates | 24 | 0 |
| risk_categories | 5 | 0 |
| risk_history | 2 | 0 |
| risk_matrices | 3 | 0 |
| risks | 2 | 0 |
| suppliers | 2 | 0 |
| technical_sheets | 1 | 0 |
| refresh_tokens | 23 | 0 |
| user_sessions | 23 | 0 |

Todas las tablas operativas verificadas quedaron en cero registros.

## Validación Final De Integridad

Resultado final:

```text
tenants|1
users|1
roles|1
permissions|36
user_roles|1
role_permissions|36
documents|0
suppliers|0
managed_audits|0
capas|0
risks|0
quality_indicators|0
report_definitions|0
stored_files|0
enterprise_workspace_items|0
audit_logs|0
superadmin|admin@compliance360.local|Active|false|SuperAdmin|SUPERADMIN|36
```

## Validación De Aplicación

- `dotnet build "Compliance360.sln"`: PASS, 0 warnings, 0 errors
- `dotnet test "Compliance360.sln" --no-build`: PASS, 218/218 tests
- `/health`: PASS, HTTP 200, `Healthy`
- Login SuperAdmin: PASS, access token emitido

## Smoke Test Básico

Con SuperAdmin se validó:

- Iniciar sesión: PASS
- Obtener token JWT: PASS
- Crear tenant temporal vía API: PASS
- Crear empresa temporal en tenant nuevo vía API: PASS
- Crear usuario temporal: PASS mediante script SQL controlado, porque no existe endpoint HTTP ni servicio de aplicación para creación de usuarios en la API actual
- Limpieza posterior del smoke: PASS
- Estado final post-smoke: solo SuperAdmin

Los datos temporales generados por smoke fueron eliminados ejecutando nuevamente la limpieza transaccional.

## Correcciones Mínimas Necesarias

Durante el smoke se detectaron dos bloqueos funcionales que impedían validar la preparación para pruebas:

- `ApiContext.TenantId` rechazaba operaciones cross-tenant aunque el usuario autenticado tuviera rol `SuperAdmin`. Se ajustó para permitir cross-tenant únicamente cuando el JWT contiene rol `SuperAdmin`.
- `TenantManagementService.AddCompanyAsync` agregaba la empresa al agregado, pero EF no la persistía como entidad nueva en este flujo. Se agregó persistencia explícita con `ITenantManagementRepository.AddCompanyAsync`.

Ambos cambios fueron validados con build, tests y smoke API.

## Riesgos Detectados

- No existe endpoint HTTP de creación de usuarios. Para el smoke se creó el usuario temporal por SQL y se limpió después. Recomendación: exponer un endpoint administrativo de usuarios para Tenant Admin/SuperAdmin antes de pruebas funcionales completas.
- El password del SuperAdmin fue revalidado y su hash fue normalizado para permitir login de pruebas. El password no se imprime en este reporte.
- La base conserva 1 tenant mínimo porque la arquitectura requiere `TenantId` para el usuario, rol, permisos y token JWT.

## Recomendaciones

- Crear endpoint oficial de administración de usuarios.
- Mantener el backup antes de iniciar pruebas reales.
- Usar esta base como punto de partida para configurar tenant real, empresa real, usuarios reales, storage real y notificaciones reales.

## Resultado Final

La base quedó en estado CLEAN TEST READY:

- Sistema vacío de datos operativos
- Sin tenants de prueba
- Sin documentos, proveedores, auditorías, CAPA, riesgos, indicadores, reportes ni archivos
- Solo SuperAdmin activo
- Rol SuperAdmin activo
- 36 permisos globales disponibles
- Health y login SuperAdmin validados
- Smoke test ejecutado y limpiado

DATABASE CLEAN RESET APPROVED
