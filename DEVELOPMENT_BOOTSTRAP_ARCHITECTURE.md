# Compliance 360 - Development Bootstrap Architecture

## Propósito

El Development Bootstrap Framework prepara automáticamente Compliance 360 para trabajo local. Su objetivo es eliminar configuración manual repetitiva, credenciales desincronizadas, datos incompletos y errores de puerto sin diagnóstico.

## Principio De Seguridad

El bootstrap solo se ejecuta cuando:

```text
ASPNETCORE_ENVIRONMENT=Development
```

Fuera de `Development`, el comportamiento productivo permanece intacto. Si falta una configuración crítica en producción, la aplicación mantiene las excepciones existentes.

## Componentes

### `Program.cs`

Integra el framework en dos puntos:

1. Precheck antes de construir/bindear Kestrel.
2. Bootstrap después de construir servicios y antes de configurar middleware / `RunAsync`.

### `DevelopmentBootstrapPrecheck`

Responsable de:

- Resolver puertos desde `--urls`, `ASPNETCORE_URLS`, `DOTNET_URLS` o fallback local `5272`.
- Detectar si el puerto está ocupado.
- Identificar PID, proceso, puerto, ruta del ejecutable y URL.
- Salir con código 0 si Compliance 360 ya está ejecutándose, sin stacktrace y sin matar procesos.

### `DevelopmentBootstrapLogging`

Inicializa un logger Serilog temprano para que incluso el precheck previo a Kestrel use mensajes estructurados y niveles explícitos:

- `Information`
- `Warning`
- `Error`
- `Critical` cuando aplique en errores no recuperables futuros

### `DevelopmentBootstrapRunner`

Responsable de:

- Diagnóstico del entorno local: .NET SDK, runtime, versión, branch, commit, OS, arquitectura, usuario, fecha y hora.
- Conectividad PostgreSQL.
- Versión PostgreSQL.
- Migraciones EF Core.
- Validación de esquema: tablas, índices, constraints, migración actual y migración esperada.
- Tenant técnico.
- SuperAdmin.
- Rol SuperAdmin.
- Permisos.
- Usuario-Rol.
- Password hash sincronizado.
- Configuración mínima de storage, SMTP, providers y observabilidad.
- Health checks antes de arrancar la app.
- Resumen final Enterprise con estados `OK`, `WARNING`, `ERROR` y `HEALTHY`.

### `appsettings.Development.json`

Contiene configuración de desarrollo para:

- `DevelopmentBootstrap`
- `BootstrapSuperAdmin`
- Providers locales/ficticios para health checks

La contraseña no se expone en JavaScript ni en consola.

### `wwwroot/app.js`

La UI ya no contiene `DEFAULT_PASSWORD` ni prellena el password. Solo muestra ayuda visual:

```text
Development: utilice las credenciales definidas en BootstrapSuperAdmin.
```

## Flujo De Arranque

```text
dotnet run
  |
  v
Environment == Development?
  |
  +-- no --> startup normal
  |
  +-- yes
       |
       v
Port Precheck
       |
       +-- ocupado por Compliance360 --> mensaje claro y exit 0
       +-- ocupado por otro proceso --> mensaje claro y exit 0
       +-- libre
            |
            v
Build service provider
            |
            v
Database + migrations + bootstrap
            |
            v
Schema validation
            |
            v
Health checks
            |
            v
Ready for Functional Testing
            |
            v
Kestrel RunAsync
```

## Idempotencia

El framework usa búsquedas por claves naturales y `ON CONFLICT` para evitar duplicados:

- Tenant por `TenantId` o `Slug`.
- Usuario por `TenantId + NormalizedEmail`.
- Rol por `TenantId + NormalizedName`.
- Permisos por `Code`.
- Role-permissions por existencia previa.
- User-role por existencia previa.

## Límites

- No mata procesos automáticamente.
- No imprime secretos.
- No ejecuta bootstrap fuera de Development.
- No intenta reparar ambientes QA/Staging/Production.
- No usa `Console.WriteLine` disperso; la salida del bootstrap pasa por Serilog.
