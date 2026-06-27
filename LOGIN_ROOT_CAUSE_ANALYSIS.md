# Compliance 360 - Login Root Cause Analysis

## Resumen Ejecutivo

Se investigaron dos fallas críticas: arranque de la aplicación con puerto ocupado y login con `HTTP 400 Invalid credentials`.

El puerto `5272` estaba ocupado porque ya existía una instancia activa de `Compliance360.Web.exe` escuchando en ese puerto. No se encontró evidencia de Docker, IIS Express, `ASPNETCORE_URLS`, `DOTNET_URLS` ni doble configuración Kestrel que explique el conflicto. El conflicto ocurre cuando se intenta iniciar una segunda instancia con el mismo perfil HTTP/HTTPS definido en `launchSettings.json`.

El login fallaba porque la UI tenía una contraseña local prellenada diferente a la contraseña cuyo hash estaba almacenado en PostgreSQL. El backend sí encontraba el usuario por `TenantId + NormalizedEmail`, pero fallaba exactamente en la verificación de password en `IdentityService.LoginAsync`.

## Cronología Técnica

1. Se intentó iniciar Compliance 360 en `http://127.0.0.1:5272`.
2. Kestrel devolvió `AddressInUseException`: el puerto ya estaba ocupado.
3. Se comprobó el puerto con `Get-NetTCPConnection`.
4. El puerto `5272` estaba ocupado por `Compliance360.Web.exe`.
5. Se intentó login desde la pantalla.
6. Los logs EF mostraron que el usuario era encontrado por `TenantId` y `NormalizedEmail`.
7. El backend devolvió `400 Invalid credentials`.
8. Se observó incremento de `AccessFailedCount`, lo que solo ocurre después de fallar la comparación de password.
9. Se comparó la UI con la base: la UI prellenaba un password distinto al hash vigente.
10. Al alinear el valor enviado por la UI y el hash de la base, el endpoint `/api/v1/auth/login` devolvió `200`.

## Hallazgo Principal

### H-001 - Contraseña prellenada en UI no coincidía con PostgreSQL

Archivo:

- `src/Compliance360.Web/wwwroot/app.js`

Evidencia:

- La UI definía `DEFAULT_PASSWORD`.
- El diff local muestra que el valor previo era `Admin2026Aa`.
- El valor validado contra el hash vigente fue `<redacted-development-password>`.

Flujo afectado:

`Login UI -> app.js -> POST /api/v1/auth/login -> IdentityService.LoginAsync -> Pbkdf2PasswordHasher.Verify`

Punto exacto de falla:

- `src/Compliance360.Application/Identity/IdentityService.cs`, método `LoginAsync`.
- Rama de fallo: verificación de contraseña.

Código relevante:

```csharp
if (_passwordHasher.Verify(command.Password, user.PasswordHash) != PasswordVerificationResult.Success)
{
    var locked = user.RegisterFailedLogin(...);
    ...
    return Result<AuthenticationResult>.Failure("Invalid credentials.");
}
```

Conclusión:

El usuario existía y el tenant era correcto. El login fallaba por mismatch entre contraseña enviada y `PasswordHash`.

## Hallazgos Secundarios

### H-002 - Puerto 5272 ocupado por una instancia existente de la misma aplicación

Evidencia de proceso:

```text
PID=1480
Process=Compliance360.Web
Path=C:\Proyectos\Compliance 360\src\Compliance360.Web\bin\Debug\net9.0\Compliance360.Web.exe
```

Evidencia de health:

```text
GET /health -> 200 Healthy
```

Conclusión:

El puerto no estaba ocupado por Docker, IIS Express ni otro producto. Estaba ocupado por otra instancia de Compliance 360.

### H-003 - Ambos perfiles de ejecución usan el mismo puerto HTTP

Archivo:

- `src/Compliance360.Web/Properties/launchSettings.json`

Evidencia:

```json
"http": {
  "applicationUrl": "http://localhost:5272"
}
```

```json
"https": {
  "applicationUrl": "https://localhost:7215;http://localhost:5272"
}
```

Impacto:

Si Visual Studio inicia el perfil `https` y una terminal inicia el perfil `http`, ambos compiten por `http://localhost:5272`.

### H-004 - `UseHttpsRedirection` advierte cuando solo se levanta HTTP

Archivo:

- `src/Compliance360.Web/Program.cs`

Evidencia de log:

```text
Failed to determine the https port for redirect.
```

Causa:

La app se ejecutó con `--urls http://localhost:5272`, mientras `UseHttpsRedirection()` está activo.

Impacto:

No causa el `Invalid credentials`, pero genera ruido en logs de arranque.

### H-005 - No hay evidencia de Docker/IIS Express como causa del puerto

Docker:

```text
docker ps -> error connecting to Docker Desktop pipe
```

IIS Express:

```text
Get-Process iisexpress -> sin procesos relevantes
```

Variables:

```text
ASPNETCORE_URLS=<not set>
DOTNET_URLS=<not set>
ASPNETCORE_ENVIRONMENT=<not set>
DOTNET_ENVIRONMENT=<not set>
```

Conclusión:

No hay doble URL por variables de entorno ni Docker/IIS Express usando `5272`.

## Evidencias De Base De Datos

Consulta de SuperAdmin:

```text
Email=admin@compliance360.local
NormalizedEmail=ADMIN@COMPLIANCE360.LOCAL
Status=Active
MfaEnabled=false
TenantId=dc7c46ee-cb25-4ed5-b0b4-800788f7f626
Role=SuperAdmin
Permissions=36
PasswordHash algorithm=PBKDF2-SHA256
Iterations=210000
```

Interpretación:

- El usuario existe.
- Está activo.
- No está bloqueado.
- Tiene tenant.
- Tiene rol.
- Tiene permisos.
- Tiene hash válido.
- El algoritmo coincide con `Pbkdf2PasswordHasher`.

## Trazabilidad Del Login

### 1. UI

Archivo:

- `src/Compliance360.Web/wwwroot/app.js`

La pantalla toma:

- `tenantId`
- `email`
- `password`

Y ejecuta:

```javascript
request("/auth/login", {
  method: "POST",
  body: {
    tenantId: form.get("tenantId"),
    email: form.get("email"),
    password: form.get("password")
  }
})
```

### 2. API

Archivo:

- `src/Compliance360.Web/Api/FoundationEndpoints.cs`

Endpoint:

```csharp
auth.MapPost("/login", async (LoginRequest request, ... ) =>
    ApiResult.From(await service.LoginAsync(
        new LoginCommand(request.TenantId, request.Email, request.Password, ...))));
```

### 3. Application

Archivo:

- `src/Compliance360.Application/Identity/IdentityService.cs`

Secuencia:

1. Normaliza email a uppercase.
2. Busca usuario por tenant y email.
3. Verifica estado.
4. Verifica password.
5. Evalúa MFA.
6. Crea JWT, refresh token y sesión.

### 4. Repository / EF

Archivo:

- `src/Compliance360.Infrastructure/Identity/EfIdentityRepository.cs`

Consulta:

```csharp
FirstOrDefaultAsync(user =>
    user.TenantId == tenantId &&
    user.NormalizedEmail == normalizedEmail)
```

Los logs EF confirmaron que esta consulta sí encontraba usuario, porque después cargó roles, refresh tokens, password history y sesiones del usuario.

### 5. Password Hash

Archivo:

- `src/Compliance360.Infrastructure/Security/SecurityServices.cs`

Algoritmo:

```text
PBKDF2-SHA256.210000.<salt>.<key>
```

La base usa ese algoritmo.

### 6. Punto Exacto Del Fallo

Archivo:

- `src/Compliance360.Application/Identity/IdentityService.cs`

Línea lógica:

```csharp
_passwordHasher.Verify(command.Password, user.PasswordHash)
```

Cuando el password enviado era incorrecto, se ejecutaba:

```csharp
user.RegisterFailedLogin(...)
return Failure("Invalid credentials.")
```

Esto coincide con el log que muestra `UPDATE compliance360.users SET "AccessFailedCount" = ...`.

## Causa Raíz

### Causa raíz de arranque

La causa raíz del error de arranque es **gestión no controlada de instancias locales**: Visual Studio y/o terminal pueden iniciar el mismo proyecto usando el mismo puerto HTTP `5272`. Como ambos perfiles (`http` y `https`) incluyen `http://localhost:5272`, cualquier instancia previa de `Compliance360.Web.exe` impide iniciar otra.

No es un problema de Kestrel iniciando dos veces dentro del mismo proceso. Es un segundo proceso intentando usar el mismo socket.

### Causa raíz de login

La causa raíz del `Invalid credentials` es **desalineación entre credencial visible/prellenada en frontend y contraseña real almacenada como hash PBKDF2 en PostgreSQL**.

El backend funcionó correctamente: encontró el usuario, validó tenant/estado, y rechazó la contraseña al no coincidir con `PasswordHash`.

## Impacto

Criticidad: **Alta**

Impacto operativo:

- El Product Owner no puede iniciar el onboarding si hay una instancia previa usando el puerto.
- El usuario cree que el SuperAdmin no existe, aunque sí existe.
- Se incrementa `AccessFailedCount`, con riesgo de lockout si se repite.
- Los errores visibles son genéricos y no explican la diferencia entre usuario inexistente, password incorrecto o cuenta bloqueada.

## Respuestas Al Criterio De Éxito

1. **¿Por qué el puerto estaba ocupado?**  
   Porque ya existía un proceso `Compliance360.Web.exe` escuchando en `5272`.

2. **¿Qué proceso lo ocupaba?**  
   `Compliance360.Web.exe`, ruta `src\Compliance360.Web\bin\Debug\net9.0\Compliance360.Web.exe`.

3. **¿Por qué el login devuelve Invalid credentials?**  
   Porque el password enviado no coincidía con el `PasswordHash` almacenado.

4. **¿Dónde exactamente falla el flujo?**  
   En `IdentityService.LoginAsync`, condición `_passwordHasher.Verify(...) != Success`.

5. **¿Cuál es la causa raíz?**  
   Desalineación de credenciales locales y falta de gestión única de proceso/puerto en desarrollo.

6. **¿Cuál es la corrección definitiva?**  
   Centralizar credenciales bootstrap locales y controlar el ciclo de vida de la app local para evitar múltiples instancias.

7. **¿Cómo evitar que vuelva a ocurrir?**  
   Ver `LOGIN_CORRECTION_PLAN.md`.
