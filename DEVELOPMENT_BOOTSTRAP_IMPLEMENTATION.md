# Compliance 360 - Development Bootstrap Implementation

## Archivos Modificados

- `src/Compliance360.Web/Program.cs`
- `src/Compliance360.Web/Development/DevelopmentBootstrap.cs`
- `src/Compliance360.Web/appsettings.Development.json`
- `src/Compliance360.Web/wwwroot/app.js`

## Implementación

### Precheck De Puerto

El precheck corre antes de `builder.Build()` y antes de que Kestrel intente bindear sockets.

Resuelve puertos desde:

- `--urls`
- `--urls=value`
- `ASPNETCORE_URLS`
- `DOTNET_URLS`
- fallback `http://localhost:5272`

Cuando detecta una instancia existente de Compliance 360:

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

No lanza excepción, no muestra stacktrace y no mata procesos automáticamente.

### Diagnóstico Del Entorno

Antes del bootstrap operativo se emite un diagnóstico Serilog con .NET SDK, .NET Runtime, versión de aplicación, branch, commit, environment, sistema operativo, arquitectura, usuario, fecha y hora.

### Bootstrap De Base De Datos

El runner valida:

- `Database.CanConnectAsync()`
- `select version()`
- `Database.MigrateAsync()`
- `GetPendingMigrationsAsync()`
- Migración actual vs migración esperada
- Tablas críticas
- Índices críticos
- Constraints críticas

Si la base no está disponible o quedan migraciones pendientes, el proceso se detiene con mensaje claro.

### Bootstrap De Identidad

El proceso garantiza:

- Tenant técnico activo.
- Rol `SuperAdmin`.
- Permisos requeridos por políticas.
- Relación rol-permisos.
- Usuario SuperAdmin.
- Relación usuario-rol.
- Password hash sincronizado con `BootstrapSuperAdmin:Password`.

La contraseña nunca se imprime.

### Permisos

Los permisos se insertan por `Code` con SQL idempotente:

```sql
on conflict ("Code") do nothing
```

Esto permite cubrir permisos cuyo código no se puede crear directamente con el constructor de dominio porque las políticas usan códigos más específicos, por ejemplo:

```text
SUPERADMIN.DASHBOARD.READ
TENANT.BRANDING
TENANT.SECURITY
```

### Configuración Mínima

El bootstrap valida:

- `IFileStorageService`
- `INotificationDispatcher`
- `IObservabilityTelemetry`
- `IPasswordHasher`
- `IJwtTokenService`
- `StorageOptions.Provider`
- `StorageOptions.ContainerName`
- `Jwt:SigningKey`
- SMTP local Development con `Host`, `Port`, `FromAddress`

Cada validación emite estado `OK`, `WARNING` o `ERROR`.

### Frontend

Se eliminó:

```javascript
DEFAULT_PASSWORD
```

El input `password` queda vacío y requerido. La UI muestra solo una indicación segura.

## Protección Por Entorno

`Program.cs` ejecuta todo el framework solo bajo:

```csharp
builder.Environment.IsDevelopment()
```

En Production/Staging/QA no se ejecuta:

- Precheck Development.
- Bootstrap de datos.
- Reseteo de password.
- Seed de permisos.

En pruebas automatizadas con `WebApplicationFactory` / `testhost`, el bootstrap también se omite para no exigir PostgreSQL local real dentro del TestServer.

## Salida Esperada

```text
==================================================
Compliance 360 Enterprise
Development Bootstrap

Environment............. Development
Application ............ OK
Database................ OK
Schema ................. OK
Migrations.............. OK
Identity ............... OK
SuperAdmin.............. OK
Permissions............. OK
SMTP ................... OK
Storage................. OK
Providers............... OK
Observability........... OK
Health ................. HEALTHY

Ready for Functional Testing
==================================================
```
