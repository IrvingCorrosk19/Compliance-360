# 16 — Runbook de despliegue VPS

## Precondiciones

- Gates completos aprobados para el mismo código/imagen.
- Backup verificado: `/opt/backups/compliance360-20260718_085002`.
- Variables `.env`, `JWT_SIGNING_KEY`, credenciales DB y configuración del proxy disponibles sin registrarlas en logs.
- Ventana, responsables y criterio de rollback confirmados.

## Procedimiento

1. Registrar versión actual de contenedores, imagen, migration history y health.
2. Verificar integridad/lectura del backup.
3. Construir o transferir la imagen Release identificada.
4. Mantener PostgreSQL solo en red `compliance360_net`.
5. Mantener web publicada en `127.0.0.1:8085`; el proxy es el único ingreso externo.
6. Levantar PostgreSQL y esperar `pg_isready`.
7. Aplicar migraciones EF, incluyendo `20260718142030_HardenRegulatoryWorkflowV2Governance`, con log.
8. Levantar web y revisar logs de startup.
9. Probar localmente en host `/health/live` y `/health/ready`.
10. Probar dominio HTTPS a través del reverse proxy.
11. Ejecutar smoke de `18_POST_DEPLOYMENT_SMOKE_TESTS.md`.

## Controles

- No exponer 5432.
- No publicar 8085 en `0.0.0.0`.
- Confirmar forwarded headers/protocolo HTTPS.
- Confirmar persistencia de storage y Data Protection keys.
- Verificar índices parciales y triggers append-only.

## Estado

**NO EJECUTADO para el corte final actual.**
