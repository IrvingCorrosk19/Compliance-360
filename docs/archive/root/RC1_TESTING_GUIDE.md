# RC-1 Testing Guide

## Objetivo

Guiar las pruebas funcionales completas del Product Owner sobre Compliance 360 RC-1.

## Preparacion

Configurar variables locales antes de ejecutar la API:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:ConnectionStrings__Compliance360='Host=localhost;Port=5432;Database=compliance360;Username=postgres;Password=<local-password>'
$env:Jwt__SigningKey='<local-signing-key-minimum-32-characters>'
dotnet run --no-build --no-launch-profile --project "src/Compliance360.Web" --urls "http://localhost:5272"
```

## Credenciales Locales

- Tenant local: `dc7c46ee-cb25-4ed5-b0b4-800788f7f626`
- Usuario: `admin@compliance360.local`

Usar la password local vigente definida para el entorno de desarrollo.

## Checklist General

1. Login SuperAdmin.
2. Abrir SuperAdmin Platform Center.
3. Crear tenant de prueba funcional.
4. Configurar informacion general del tenant.
5. Configurar branding, seguridad y licenciamiento.
6. Crear usuario tenant.
7. Crear documento.
8. Crear workflow.
9. Crear ficha tecnica.
10. Crear supplier.
11. Crear auditoria.
12. Crear CAPA.
13. Crear riesgo.
14. Crear indicador.
15. Seed/ejecutar/exportar reporte.
16. Configurar provider de storage.
17. Revisar audit trail.
18. Revisar observability/health.

## Criterios Por Pantalla

Validar en cada pantalla:

- Carga inicial sin errores.
- Botones visibles ejecutan accion o estan deshabilitados correctamente.
- Formularios validan campos requeridos.
- Mensajes de exito/error son claros.
- Tablas muestran datos reales o estado vacio.
- Filtros y busquedas no rompen navegacion.
- Exportaciones protegidas requieren sesion.
- No hay placeholders ni mensajes temporales.
- Dark mode y responsive no rompen layout.

## Registro de Defectos

Formato recomendado:

```text
Modulo:
Pantalla:
Severidad: Critica / Alta / Media / Baja
Pasos:
Resultado esperado:
Resultado obtenido:
Datos usados:
Evidencia:
```

## Evidencia RC-1 Ya Ejecutada

- Smoke automatico creo tenant: `d2d7816b-e9b7-44b2-a655-8f09b61cb3af`.
- Smoke valido documento, workflow, auditoria, CAPA, riesgo, indicador, provider y reporte.

## Regla De Cierre

RC-1 solo pasa a una candidata posterior cuando todos los defectos criticos y altos encontrados por el Product Owner esten corregidos y revalidados.
