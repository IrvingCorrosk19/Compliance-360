# 17 — Backup y rollback

## Backup disponible

Backup VPS verificado en:

`/opt/backups/compliance360-20260718_085002`

La verificación informada confirma existencia; antes del cambio debe registrarse además tamaño, checksum, contenido esperado y una prueba de lectura/restauración cuando sea operativamente posible.

## Disparadores de rollback

- Migración falla o deja schema inconsistente.
- `/health/ready` no se estabiliza.
- Login/JWT/sesiones dejan de funcionar.
- Pérdida de acceso a storage/evidencia.
- Regresión crítica RBAC/SoD.
- Trigger/índice bloquea datos válidos o no protege historia.
- Smoke regulatorio crítico falla.

## Procedimiento

1. Detener tráfico en el proxy.
2. Capturar logs y estado para análisis.
3. Detener la versión nueva.
4. Restaurar base desde el backup validado si hubo cambio de schema/datos no reversible.
5. Restaurar imagen/configuración anterior compatible.
6. Levantar dependencias y verificar health.
7. Ejecutar smoke mínimo de login, lectura y evidencia.
8. Reabrir tráfico solo con aprobación.

## Advertencia

No usar `git reset`, eliminación de volúmenes ni un `Down` automático como rollback de datos. Aplicación y schema deben restaurarse como unidad compatible.

## Estado

**Plan documentado; rollback no ensayado en este corte.**
