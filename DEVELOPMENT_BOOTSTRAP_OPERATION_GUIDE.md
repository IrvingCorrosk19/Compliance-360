# Compliance 360 - Development Bootstrap Operation Guide

## Uso Diario

Para entorno local:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project "src/Compliance360.Web/Compliance360.Web.csproj"
```

Si se usa Visual Studio o `launchSettings.json`, el perfil ya define `ASPNETCORE_ENVIRONMENT=Development`.

## Qué Hace Automáticamente

Al iniciar, Compliance 360:

- Muestra diagnóstico del entorno local: SDK, runtime, versión, branch, commit, OS, arquitectura, usuario, fecha y hora.
- Verifica el puerto configurado.
- Evita una segunda instancia silenciosa.
- Valida PostgreSQL.
- Aplica migraciones.
- Valida esquema crítico: tablas, índices, constraints y versión de migración.
- Garantiza tenant técnico.
- Garantiza SuperAdmin.
- Garantiza roles y permisos.
- Sincroniza password hash en Development.
- Valida storage, SMTP, providers, observabilidad y health checks.

## Puerto Ocupado

Si Compliance 360 ya está corriendo:

```text
==================================================
Compliance 360 Enterprise

Ya existe una instancia ejecutandose.

PID .................... xxxx
Proceso ................ Compliance360.Web
Puerto ................. 5272
Ruta del ejecutable .... ...
URL .................... http://localhost:5272

No se iniciara una segunda instancia.
Utilice la instancia existente.
==================================================
```

Acción del desarrollador:

- Usar la instancia existente.
- O detenerla desde Visual Studio/terminal si desea reiniciar.

El bootstrap no mata procesos automáticamente.

## Estados De Validación

La salida usa estados explícitos:

- `OK`: validación correcta.
- `WARNING`: condición no bloqueante que requiere atención.
- `ERROR`: condición bloqueante; no se inicia Kestrel.
- `HEALTHY`: health checks correctos.

## Credenciales Locales

Las credenciales se definen en:

```text
BootstrapSuperAdmin
```

La UI no contiene ni muestra la contraseña. El desarrollador debe usar las credenciales de configuración Development autorizadas para el entorno local.

## Fallas Bloqueantes

El arranque se detiene si:

- PostgreSQL no responde.
- Faltan migraciones después del bootstrap.
- Faltan tablas, índices o constraints críticas.
- La migración actual no coincide con la esperada.
- Falta `BootstrapSuperAdmin:Password`.
- Falta configuración mínima de storage.
- Falta configuración mínima SMTP.
- Un health check requerido está `Unhealthy`.

La salida debe ser un mensaje claro, no un stacktrace.

## Seguridad Operativa

Nunca ejecutar bootstrap en:

- Production
- Staging
- QA

La implementación ya lo bloquea por código con `IsDevelopment()`.

## Limpieza

Si se usa un puerto alterno para pruebas:

```powershell
dotnet run --project "src/Compliance360.Web/Compliance360.Web.csproj" --urls "http://localhost:5999"
```

Al terminar, detener el proceso desde la terminal o IDE. El framework no elimina procesos por seguridad.
