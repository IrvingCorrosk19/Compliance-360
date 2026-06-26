# SuperAdmin Platform Center - Operation Guide

## Acceso

1. Iniciar sesion con un usuario SuperAdmin.
2. Abrir `SuperAdmin Platform` en el grupo Enterprise.
3. Confirmar que `Global Health` y `Alert Center` cargan desde el backend.

## Operacion Diaria

- Revisar `Executive Dashboard` para volumen global de tenants, usuarios, modulos, documentos, auditorias, CAPA, riesgos, indicadores, providers, errores y jobs.
- Usar `Global Search` para filtrar filas visibles en los paneles.
- Revisar `Alert Center` antes de cambios operativos.
- Abrir `Tenants` y usar `Open TAC` para administrar un tenant especifico desde su Tenant Administration Center.
- Revisar `Providers` para SMTP y Storage providers tenant-scoped.
- Revisar `Audit` y exportar CSV cuando se requiera evidencia.
- Revisar `Database` solo como monitoreo. No se hacen cambios directos desde UI.

## Permisos

Asignar permisos `SUPERADMIN.*` al rol SuperAdmin mediante:

- `artifacts/migrations/clean-reset-for-testing.sql` para ambientes limpios.
- `artifacts/migrations/seed-tenant-administration-permissions.sql` para ambientes existentes.

## Validacion Operativa

Comandos usados:

```powershell
dotnet build Compliance360.sln
dotnet test Compliance360.sln
node --check "src/Compliance360.Web/wwwroot/app.js"
$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --no-build --no-launch-profile --project "src/Compliance360.Web" --urls "http://localhost:5272"
Invoke-WebRequest -Uri "http://localhost:5272/health" -UseBasicParsing
```

## Smoke SuperAdmin

1. Ejecutar la API con ambiente `Development`.
2. Hacer login local con `admin@compliance360.local`.
3. Llamar `/api/v1/superadmin/platform-center` con bearer token.
4. Confirmar 200 OK y payload con `metrics`, `tenants`, `providers`, `licenses`, `modules`, `health`, `backups`, `database`, `auditTimeline`, `alerts` y `quickActions`.

## Mantenimiento

- Las metricas son read-only y se derivan de tablas reales.
- Las acciones tenant-scoped deben seguir ejecutandose desde Tenant Administration Center.
- La UI global no debe modificar directamente PostgreSQL.
- Nuevos providers globales o capacidades AI reales deben introducir sus aggregates/servicios antes de mostrarse como administrables.
