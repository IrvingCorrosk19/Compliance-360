# Compliance 360 - Development Bootstrap Test Report

## Fecha

2026-06-27

## Alcance Validado

Se validó el Development Bootstrap Framework V2 con evidencia técnica:

- Compilación del proyecto web.
- Precheck de puerto ocupado.
- Bootstrap completo en puerto alterno.
- Diagnóstico del entorno local.
- Validación de esquema crítico.
- Resumen final Enterprise.
- Eliminación de contraseña hardcodeada en JavaScript.
- Salida limpia sin stacktrace en puerto ocupado.
- Compatibilidad con `WebApplicationFactory` / `testhost`.

## Evidencia 1 - Compilación

Comando:

```powershell
dotnet build
```

Resultado:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Nota:

La compilación normal pasó después de liberar procesos locales previos que bloqueaban `bin\Debug`.

## Evidencia 2 - Puerto Ocupado

Comando:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet "artifacts/build-validation/web/Compliance360.Web.dll"
```

Resultado:

```text
==================================================
Compliance 360 Enterprise

Ya existe una instancia ejecutandose.

PID .................... 5340
Proceso ................ Compliance360.Web
Puerto ................. 5272
Ruta del ejecutable .... C:\Proyectos\Compliance 360\src\Compliance360.Web\bin\Debug\net9.0\Compliance360.Web.exe
URL .................... http://localhost:5272

No se iniciara una segunda instancia.
Utilice la instancia existente.
==================================================
```

Conclusión:

El precheck identifica la instancia existente y sale de forma elegante, sin excepción ni stacktrace.

## Evidencia 3 - Bootstrap Completo

Comando:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet "Compliance360.Web.dll" --urls "http://localhost:5999"
```

Ejecutado desde:

```text
artifacts/build-validation/web
```

Resultado relevante:

```text
Development Environment Diagnostics

.NET SDK ............... 9.0.306
.NET Runtime ........... .NET 9.0.10
Application Version .... 0.20.0-development-bootstrap
Git Branch ............. master
Commit ................. 99b55c6
Environment ............ Development
Operating System ....... Microsoft Windows 10.0.22000
Architecture ........... X64
User ................... irvin
Date ................... 2026-06-27
Time ................... 06:18:38 -05:00
```

Resumen final:

```text
==================================================
Compliance 360 Enterprise
Development Bootstrap

Environment ............ Development
Application ............ OK
Database ............... OK
Schema ................. OK
Migrations ............. OK
Identity ............... OK
SuperAdmin ............. OK
Permissions ............ OK
SMTP ................... OK
Storage ................ OK
Providers .............. OK
Observability .......... OK
Health ................. HEALTHY

Ready for Functional Testing
==================================================
```

Conclusión:

El bootstrap completó DB, migraciones, tenant, permisos, rol, SuperAdmin, configuración mínima y health checks antes de iniciar Kestrel.

## Evidencia 4 - Frontend Sin Password Hardcodeado

Búsqueda:

```text
DEFAULT_PASSWORD
<redacted-development-password>
Admin2026Aa
```

Resultado:

```text
No matches found in src/Compliance360.Web/wwwroot/*.js
```

Conclusión:

No quedan contraseñas hardcodeadas en el frontend.

## Evidencia 5 - Pruebas Automatizadas

Comando:

```powershell
dotnet test
```

Resultado:

```text
Passed! - Failed: 0, Passed: 225, Skipped: 0, Total: 225
```

Conclusión:

El bootstrap V2 no interfiere con `WebApplicationFactory` / `testhost`. La guarda de testhost evita que las pruebas de integración intenten ejecutar bootstrap de PostgreSQL real.

## Escenarios Del Criterio De Aceptación

- Puerto ocupado: PASA. Identifica PID, proceso, ruta y sale limpio.
- Puerto libre: PASA. Bootstrap completo validado en `5999`.
- Base existente: PASA. Tenant, rol, permisos y SuperAdmin son idempotentes.
- Migraciones: PASA. Resultado `Migrations OK`.
- Esquema: PASA. Resultado `Schema OK`.
- SuperAdmin existente: PASA. Resultado `SuperAdmin OK`.
- Password desincronizada: CUBIERTO POR IMPLEMENTACION. `ResetPasswordWhenOutOfSync=true` solo opera en Development.
- Storage configurado: PASA. Resultado `Storage OK`.
- SMTP configurado: PASA. Resultado `SMTP OK`.
- Providers configurados: PASA. Resultado `Providers OK`.
- Health checks: PASA. Resultado `Health Checks OK`.
- Frontend sin secretos: PASA. Busqueda sin matches.
- Pruebas automatizadas: PASA. `225/225`.

## Riesgos Residuales

- La validación de base limpia no fue ejecutada destruyendo la base actual para no alterar datos locales existentes.
- El bootstrap está preparado para crear faltantes idempotentemente en base limpia.
- La compilación normal seguirá fallando si Visual Studio o instancias existentes mantienen bloqueado `bin\Debug`; el precheck evita que el desarrollador intente arrancar una segunda instancia, pero MSBuild no puede sobrescribir binarios que ya están en uso.
