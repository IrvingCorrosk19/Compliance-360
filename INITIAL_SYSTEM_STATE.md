# Compliance 360 - Initial System State

## Estado General

Compliance 360 queda como una instalación nueva para pruebas funcionales del Product Owner.

La aplicación conserva la estructura completa, migraciones aplicadas, esquema PostgreSQL y configuración mínima necesaria para arrancar. No existen datos funcionales de prueba.

## Acceso Inicial

| Elemento | Estado |
| --- | --- |
| URL local | `http://localhost:5272` |
| Health | `http://localhost:5272/health` |
| Usuario inicial | `admin@compliance360.local` |
| Rol inicial | `SuperAdmin` |
| MFA | Desactivado para el primer acceso |
| Cambio de contraseña | Requerido en el primer uso |

La contraseña temporal no se almacena en este documento por seguridad.

## Datos Conservados

| Área | Estado |
| --- | --- |
| Tenant mínimo de plataforma | Conservado |
| SuperAdmin | Conservado |
| Rol SuperAdmin | Conservado |
| Permisos | 36 conservados |
| Configuración mínima | Conservada |
| Migraciones | Conservadas |
| Esquema | Conservado |

## Datos Eliminados

| Área | Estado |
| --- | ---: |
| Empresas | 0 |
| Usuarios no SuperAdmin | 0 |
| Roles de prueba | 0 |
| Documentos | 0 |
| Workflows | 0 |
| Technical Sheets | 0 |
| Productos | 0 |
| Suppliers | 0 |
| Audits | 0 |
| CAPA | 0 |
| Risk | 0 |
| Indicators | 0 |
| Reports | 0 |
| Notifications | 0 |
| Storage Files | 0 |
| Logs operativos | 0 |

## Qué Significa Este Estado

El sistema puede arrancar y autenticar al SuperAdmin, pero no tiene contenido funcional cargado. Esto es intencional.

Debes crear manualmente el primer tenant, la primera empresa, usuarios, roles operativos, branding, storage, notificaciones y módulos funcionales durante tu recorrido de aprendizaje.

## No Hacer Antes Del Onboarding

- No ejecutar seeds de demo.
- No importar datos masivos.
- No crear tenants automáticamente.
- No crear usuarios de prueba fuera del flujo funcional.
- No modificar migraciones ni esquema.
- No cambiar permisos base del SuperAdmin.

## Punto De Partida Correcto

Tu siguiente paso es entrar como SuperAdmin y seguir `PRODUCT_OWNER_START_HERE.md`.
