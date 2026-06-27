# Compliance 360 - Login Correction Plan

## Objetivo

Corregir definitivamente los problemas de arranque local y autenticación SuperAdmin sin depender de parches manuales, cambios de puerto improvisados o reseteos repetidos de contraseña.

## Alcance

Este plan no aplica cambios todavía. Define qué debe modificarse, por qué, impacto, riesgos y pruebas requeridas.

## Corrección 1 - Definir una única fuente de verdad para credenciales bootstrap locales

### Qué se modificará

Crear una configuración explícita para credenciales bootstrap locales, por ejemplo:

```json
"BootstrapSuperAdmin": {
  "TenantId": "...",
  "Email": "admin@compliance360.local",
  "Password": "..."
}
```

Solo debe estar disponible en entorno `Development`.

### Por qué

Actualmente la UI puede tener un valor hardcodeado que no necesariamente coincide con el hash de PostgreSQL.

### Impacto

- El frontend obtiene el valor de una fuente controlada o deja de mostrar contraseña hardcodeada.
- El backend puede validar/sembrar el usuario local de forma idempotente.

### Riesgos

- Exponer contraseña en producción si no se limita a `Development`.
- Crear dependencia accidental de credenciales demo.

### Pruebas

- Verificar que en `Development` el usuario inicial existe.
- Verificar que en `Production` no se expone contraseña.
- Verificar que el login funciona después de reiniciar PostgreSQL y la app.

## Corrección 2 - Bootstrap idempotente del SuperAdmin local

### Qué se modificará

Agregar un proceso explícito de inicialización local que:

- Verifique tenant técnico.
- Verifique usuario SuperAdmin.
- Verifique rol SuperAdmin.
- Verifique permisos.
- Verifique asignación usuario-rol.
- Verifique hash de contraseña si está habilitado el modo local.

Debe ejecutarse solo bajo una condición clara, por ejemplo:

```text
ASPNETCORE_ENVIRONMENT=Development
BootstrapSuperAdmin:Enabled=true
```

### Por qué

Evita que el estado de la base y la UI se desalineen después de limpiezas, resets o pruebas funcionales.

### Impacto

- Onboarding local repetible.
- Menos dependencia de SQL manual.

### Riesgos

- Si se habilita en producción, podría sobrescribir credenciales reales.

### Pruebas

- Base limpia: crea/valida SuperAdmin.
- Base existente: no duplica usuario, rol ni permisos.
- Password incorrecto: lo corrige solo si `Enabled=true`.
- Producción: no ejecuta bootstrap.

## Corrección 3 - Controlar ejecución local y puerto

### Qué se modificará

Agregar documentación/script de arranque local que haga preflight:

1. Revisar si `5272` está ocupado.
2. Si está ocupado por `Compliance360.Web`, informar que la app ya está corriendo.
3. Si está ocupado por otro proceso, mostrar PID y ruta.
4. No iniciar una segunda instancia.

Ejemplo de script:

```powershell
Get-NetTCPConnection -LocalPort 5272 -State Listen
```

### Por qué

El problema no es Kestrel duplicado dentro del mismo proceso. Es iniciar otra instancia contra un puerto ya ocupado.

### Impacto

- Evita `AddressInUseException`.
- Reduce confusión entre Visual Studio, terminal y browser.

### Riesgos

- Un script demasiado agresivo podría matar procesos que el usuario quería mantener.

### Pruebas

- Puerto libre: inicia app.
- Puerto ocupado por Compliance360: informa estado y no inicia otra.
- Puerto ocupado por otro proceso: informa PID sin matar automáticamente.

## Corrección 4 - Separar perfiles de Visual Studio

### Qué se modificará

Revisar `launchSettings.json`.

Opción recomendada:

- Perfil `http`: `http://localhost:5272`
- Perfil `https`: `https://localhost:7215`

Evitar que ambos perfiles incluyan el mismo endpoint HTTP cuando se usan en paralelo.

### Por qué

Actualmente:

```json
"http": "http://localhost:5272"
"https": "https://localhost:7215;http://localhost:5272"
```

Si se ejecutan ambos, compiten por `5272`.

### Impacto

- Menos conflictos entre perfiles.
- HTTPS sigue disponible.

### Riesgos

- Algunos flujos locales podrían depender de HTTP aunque se lance perfil HTTPS.

### Pruebas

- Ejecutar perfil `http`.
- Ejecutar perfil `https`.
- Confirmar que no se pisan entre sí si se ejecutan separadamente.
- Confirmar CORS para ambos orígenes.

## Corrección 5 - Mejorar mensaje de error de login sin filtrar seguridad

### Qué se modificará

Mantener respuesta pública genérica:

```text
Invalid credentials.
```

Pero agregar logs internos diferenciados:

- Usuario no encontrado.
- Password incorrecto.
- Cuenta bloqueada.
- Cuenta deshabilitada.
- MFA requerido sin configuración.

### Por qué

El usuario ve un mensaje seguro, pero soporte necesita saber dónde falla.

### Impacto

- Mejor diagnóstico.
- No se filtra información sensible al cliente.

### Riesgos

- Logs demasiado detallados pueden exponer datos si no se sanitizan.

### Pruebas

- Login usuario inexistente.
- Login password incorrecto.
- Login cuenta deshabilitada.
- Login cuenta bloqueada.
- Login correcto.

## Corrección 6 - Evitar contraseñas hardcodeadas en la UI productiva

### Qué se modificará

La UI no debe hardcodear password productivo. Para desarrollo se puede:

- Mostrar botón “Usar credenciales locales”.
- Solo si `localhost`.
- Solo si entorno `Development`.
- Nunca en build productivo.

### Por qué

Hardcodear contraseña en `app.js` crea desalineación y riesgo de exposición.

### Impacto

- Login local sigue siendo fácil.
- Producción no expone secretos.

### Riesgos

- Necesita una forma segura de detectar ambiente local desde frontend.

### Pruebas

- `localhost`: muestra ayuda local.
- dominio no local: no muestra password.
- build production: no incluye credencial.

## Orden Recomendado

1. Implementar bootstrap idempotente local.
2. Separar perfiles de `launchSettings.json`.
3. Crear script preflight de puerto.
4. Quitar password hardcodeado permanente del frontend.
5. Añadir logs internos diferenciados en Identity.
6. Validar con pruebas automatizadas.

## Criterio De Aceptación

La corrección estará completa cuando:

- La app no intente iniciar dos instancias en `5272` sin advertencia.
- El SuperAdmin local sea reproducible desde base limpia.
- El frontend no pueda quedar desalineado con la contraseña real.
- Un login fallido deje evidencia interna clara sin revelar datos al usuario.
- `dotnet build` y pruebas de login pasen.
