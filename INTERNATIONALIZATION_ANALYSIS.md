# Internationalization Analysis

## Estado actual

Compliance 360 usa una SPA JavaScript servida desde `src/Compliance360.Web/wwwroot/app.js` sobre ASP.NET Core Minimal APIs. No se encontro Angular ni Razor/Views como superficie principal de UI. Antes de esta migracion no existia un sistema i18n/l10n centralizado; los textos visibles estaban mayormente embebidos en plantillas JavaScript, mensajes toast, placeholders, breadcrumbs, estados y reportes E2E.

## Textos encontrados

Se detectaron textos hardcoded en:

- `src/Compliance360.Web/wwwroot/app.js`: login, MFA, menu lateral, dashboards, SuperAdmin Platform, Tenant Administration, modulos operativos, toasts, loading states, errores, placeholders y estados vacios.
- `src/Compliance360.Web/Program.cs`: mensajes de bootstrap/configuracion y health checks visibles en desarrollo/logs.
- `src/Compliance360.Application/*`: mensajes de `Result.Failure` y validaciones de dominio/aplicacion.
- `src/Compliance360.Web/Development/DevelopmentBootstrap.cs`: salida visible de consola durante bootstrap.
- Documentacion y reportes E2E ya existentes.

## Arquitectura implementada

Se implemento una primera arquitectura enterprise-compatible sin romper las plantillas actuales:

- Recursos estaticos en `src/Compliance360.Web/wwwroot/Resources/{culture}`.
- Idiomas iniciales:
  - `es` con cultura `es-PA`.
  - `en` con cultura `en-US`.
- Archivos de recursos:
  - `Common.json`
  - `Menu.json`
  - `Validation.json`
  - `Errors.json`
  - `Dashboard.json`
  - `Users.json`
  - `Reports.json`
- Runtime frontend:
  - Detecta idioma del navegador la primera vez.
  - Persiste preferencia en `localStorage`, `sessionStorage` y cookie `c360.language`.
  - Carga recursos por `fetch`.
  - Agrega selector de idioma en login, MFA y shell principal.
  - Cambia idioma sin cerrar sesion.
  - Re-renderiza y traduce inmediatamente texto, placeholders, titles y aria-labels.
  - Formatea fechas con `Intl.DateTimeFormat` usando `es-PA` o `en-US`.
- Backend:
  - Registra `AddLocalization`.
  - Activa `UseRequestLocalization`.
  - Soporta culturas `es-PA` y `en-US`.
  - Lee cookie simple `c360.language=es|en` y aplica cultura server-side.

## Recursos creados

- `src/Compliance360.Web/wwwroot/Resources/es/Common.json`
- `src/Compliance360.Web/wwwroot/Resources/es/Menu.json`
- `src/Compliance360.Web/wwwroot/Resources/es/Validation.json`
- `src/Compliance360.Web/wwwroot/Resources/es/Errors.json`
- `src/Compliance360.Web/wwwroot/Resources/es/Dashboard.json`
- `src/Compliance360.Web/wwwroot/Resources/es/Users.json`
- `src/Compliance360.Web/wwwroot/Resources/es/Reports.json`
- `src/Compliance360.Web/wwwroot/Resources/en/Common.json`
- `src/Compliance360.Web/wwwroot/Resources/en/Menu.json`
- `src/Compliance360.Web/wwwroot/Resources/en/Validation.json`
- `src/Compliance360.Web/wwwroot/Resources/en/Errors.json`
- `src/Compliance360.Web/wwwroot/Resources/en/Dashboard.json`
- `src/Compliance360.Web/wwwroot/Resources/en/Users.json`
- `src/Compliance360.Web/wwwroot/Resources/en/Reports.json`

## Archivos modificados

- `src/Compliance360.Web/wwwroot/app.js`
- `src/Compliance360.Web/wwwroot/styles.css`
- `src/Compliance360.Web/Program.cs`
- `INTERNATIONALIZATION_ANALYSIS.md`

## Cobertura de traduccion

Cobertura implementada:

- Login y MFA.
- Selector de idioma.
- Menu lateral y grupos principales.
- Dashboard principal.
- Acciones rapidas.
- SuperAdmin Platform y creacion de tenants.
- Mensajes de sesion/toasts principales.
- Modo solo lectura.
- Estados vacios y errores comunes.
- Fechas localizadas en UI.

Cobertura preparada por arquitectura:

- Validaciones.
- Errores.
- Usuarios/roles/permisos.
- Reportes/exportaciones.
- Nuevas pantallas mediante recursos JSON y runtime comun.

## Riesgos

- La aplicacion tiene muchas plantillas JavaScript existentes. La migracion completa clave-por-clave debe hacerse de forma progresiva para eliminar todo literal del codigo fuente.
- Esta primera fase traduce la UI renderizada mediante recursos, pero algunos literales siguen existiendo como fallback en plantillas antiguas para compatibilidad.
- Los mensajes backend de servicios (`Result.Failure`) aun requieren reemplazo por codigos localizables y resolucion por cultura.
- Correos/PDF/Excel deben integrarse al mismo catalogo cuando se implementen generadores finales de esos artefactos.
- La persistencia en base de datos de preferencia de idioma queda pendiente si se agrega campo de perfil/usuario.

## Mejoras implementadas

- Selector de idioma disponible sin cerrar sesion.
- Cambio inmediato por re-render.
- Preferencia persistida en navegador y cookie.
- Cultura server-side sincronizada con cookie.
- Formato de fechas por cultura:
  - Espanol: `es-PA`.
  - Ingles: `en-US`.
- Recursos separados por dominio para evitar duplicacion futura.
- Fallback seguro: si falta una clave, se conserva el texto existente y no se rompe la pantalla.

## Pendientes

- Migrar cada plantilla de `app.js` a claves explicitas `t("...")` y retirar literales visibles.
- Crear localizacion backend con codigos de error estables y `IStringLocalizer`/catalogos compartidos.
- Agregar preferencia de idioma persistida en perfil de usuario cuando el modelo lo soporte.
- Localizar correos, PDFs, Excel y reportes reales cuando existan generadores productivos finales.
- Crear pruebas Playwright especificas para conmutacion ES/EN por rol.
- Ampliar recursos con la totalidad de textos especializados de todos los modulos.

## Resultado final

Se dejo una base de internacionalizacion funcional y extensible para Espanol/Ingles, compatible con la SPA actual y con ASP.NET Core. La migracion no rompe el comportamiento existente y permite cambio inmediato de idioma. La eliminacion absoluta de todos los literales visibles del codigo queda identificada como fase incremental de hardening, porque la UI actual concentra cientos de textos heredados en un unico archivo JavaScript.

## Validacion ejecutada

- `node --check src/Compliance360.Web/wwwroot/app.js`: PASS.
- Validacion JSON de `src/Compliance360.Web/wwwroot/Resources/**/*.json`: PASS.
- `dotnet build "Compliance360.sln" -p:OutputPath="artifacts/build-validation/i18n/"`: PASS, 0 warnings, 0 errors.
- `dotnet test "Compliance360.sln" --no-build`: PASS, 225/225.
- Playwright smoke test:
  - Carga login.
  - Cambia selector a English.
  - Verifica texto `Sign in` y `Language`.
  - Cambia selector a Espanol.
  - Verifica texto `Iniciar sesion`.
  - Resultado: PASS.
