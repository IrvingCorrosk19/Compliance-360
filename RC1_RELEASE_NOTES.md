# RC-1 Release Notes

## Compliance 360 RC-1

Estado: **READY FOR FUNCTIONAL TESTING**

Esta version candidata congela desarrollo funcional y entrega una base estable para validacion funcional del Product Owner.

## Cambios Incluidos

- Correccion de ejecucion de reportes en Reporting Engine.
- Validacion clean/restore/build/test completa.
- Smoke funcional local con SuperAdmin y modulos principales.
- Endurecimiento de configuracion local: secretos fuera de `appsettings.Development.json`.
- Endurecimiento CI/CD: secretos movidos a variables/secretos de pipeline.
- Consistencia UI adicional en textos/estados visibles.
- Documentacion RC-1 generada.

## Validacion

- `dotnet clean`: OK.
- `dotnet restore`: OK.
- `dotnet build`: OK, 0 warnings, 0 errors.
- `dotnet test`: OK, 225/225.
- `node --check`: OK.
- Smoke funcional RC-1: OK tras correccion de Reporting.
- Paquetes vulnerables: ninguno reportado.

## Notas Para Product Owner

Usar RC-1 para pruebas funcionales completas. Registrar cualquier defecto encontrado con:

- Modulo.
- Pasos para reproducir.
- Resultado esperado.
- Resultado obtenido.
- Captura/log si aplica.
- Severidad funcional.

## No Incluido

- No se declara Production Ready.
- No se agregaron nuevos modulos.
- No se agregaron funcionalidades nuevas.
- No se ejecuto load test enterprise.
- No se ejecuto QA visual manual completo por pantalla.
