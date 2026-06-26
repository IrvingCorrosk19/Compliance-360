# DATABASE_CLEAN_RESET_REPORT

## Resultado

**Estado:** base local `compliance360` creada desde cero en PostgreSQL Docker.  
**Acción destructiva ejecutada:** ninguna.  
**Base limpiada:** no aplica; no existía una base `compliance360` previa en localhost.  
**Base preservada:** `autonomuscrm` no fue modificada.  
**Contenedor PostgreSQL usado:** `autonomuscrm-postgres`.  
**Host local:** `localhost:5432`.  
**Base creada:** `compliance360`.  
**Schema de aplicación:** `compliance360`.  
**Historial EF:** `public.__EFMigrationsHistory`.

## Backup

| Campo | Resultado |
| --- | --- |
| Backup requerido antes de borrar datos | No aplicó |
| Motivo | No se borró una base existente; se creó una base nueva vacía |
| Backup creado | No |
| Ruta del backup | N/A |
| Riesgo de pérdida de datos | Bajo; `autonomuscrm` y `postgres` no fueron alteradas |

## Migraciones aplicadas

| Métrica | Resultado |
| --- | ---: |
| Migraciones EF registradas | 13 |
| Tablas de dominio creadas | 122 |
| SQL generado | `artifacts/migrations/local-compliance360-bootstrap.sql` |
| SQL de validación | `artifacts/migrations/validate-local-compliance360.sql` |

## Registros actuales

| Tabla | Registros |
| --- | ---: |
| `compliance360.tenants` | 1 |
| `compliance360.users` | 1 |
| `compliance360.roles` | 1 |
| `compliance360.permissions` | 36 |

## SuperAdmin

| Validación | Resultado |
| --- | --- |
| Usuario SuperAdmin conservado | Creado |
| Email | `admin@compliance360.local` |
| Rol SuperAdmin conservado | Creado |
| Permisos SuperAdmin conservados | 36 permisos asignados |
| Próximo paso | Cambiar contraseña temporal después del primer acceso |

## Validaciones

| Validación | Resultado |
| --- | --- |
| Base `compliance360` existe | OK |
| Schema `compliance360` existe | OK |
| Migraciones aplicadas | OK |
| Build | OK, 0 warnings, 0 errors |
| Tests | OK, 218 passed |
| App inicia | OK en `http://localhost:5285` |
| Health check | OK, `200 Healthy` |
| Datos de prueba | OK, solo tenant platform, SuperAdmin, rol y permisos mínimos |

## Riesgos encontrados

| Riesgo | Estado | Mitigación |
| --- | --- | --- |
| No existía base `compliance360` en localhost | Resuelto | Se creó base nueva en PostgreSQL Docker |
| La única base previa era `autonomuscrm` | Controlado | No se modificó |
| No hay seed automático de SuperAdmin | Resuelto operativamente | Se creó bootstrap manual en la base local |
| `pg_dump`/`psql` no están instalados en Windows | Controlado | Se usó `psql` dentro del contenedor Docker |
| Puerto `5272` estaba ocupado por un proceso previo | Resuelto | Se validó en puerto alternativo `5285` y se detuvieron procesos de prueba |

## Estado final

La base local `compliance360` está lista en localhost con estructura completa y sin datos operativos de prueba.

Estado actual:

- Tenant mínimo `platform`.
- Usuario `admin@compliance360.local`.
- Rol `SuperAdmin`.
- 36 permisos administrativos.
- Asignación SuperAdmin -> Rol SuperAdmin.

La contraseña temporal no se guarda en este reporte por seguridad.
