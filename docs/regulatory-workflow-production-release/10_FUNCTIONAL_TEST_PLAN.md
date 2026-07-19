# 10 — Plan de pruebas funcionales

## Gate A — Build y unit/integration

1. `dotnet build` Release: exigir 0 errores y 0 warnings.
2. Suite focalizada de seguridad/identidad/workflow/migración.
3. Suite .NET completa posterior al último commit/cambio.
4. Registrar comando, SHA del código, fecha, duración y resultado.

## Gate B — Flujo regulatorio local

1. Crear producto+dossier Draft.
2. Editar metadata y provocar conflicto de revisión 409.
3. Completar requisitos e iniciar revisión técnica.
4. Solicitar corrección; rechazar escritura fuera de scope.
5. Cargar V1/V2 y comprobar hashes/versiones.
6. Enviar corrección y completar revisión.
7. Aprobar, submit, iniciar authority review, observar/responder/resubmit.
8. Rechazar con resolución y documento.
9. Cancelar expediente pre-submission y confirmar conservación.
10. Reopen/override con dos aprobadores y archive.

## Gate C — Roles y seguridad

- Ejecutar positivos por rol y negativos Viewer/TAC/RA-ADM.
- Verificar upload spoofing, path traversal, tamaño y extensión.
- Verificar sesión revocada, refresh rotado y rate limit 429.

## Gate D — Browser y producción

- Playwright spec dedicado.
- Regresión Playwright completa.
- Deploy VPS, migración, `/health/live`, `/health/ready`.
- Smoke productivo multirol y verificación de proxy/HTTPS.

## Criterio de salida

Todos los gates finalizaron: build 0/0, .NET 282/282, Playwright productivo 72/72, migraciones aplicadas, readiness Healthy y smoke HTTPS/proxy/BD/storage/RBAC/SoD sin 5xx ni 429.
