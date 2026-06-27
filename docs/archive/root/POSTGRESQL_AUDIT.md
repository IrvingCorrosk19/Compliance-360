# POSTGRESQL_AUDIT

## Backup

| Elemento | Resultado |
| --- | --- |
| Backup | `artifacts/backups/compliance360-20260624-191441.dump` |
| Tamaño | 353,953 bytes |
| Verificación | `pg_restore -l` OK |
| TOC generado | `artifacts/backups/last-backup-toc.txt` |

## Configuración auditada

| Elemento | Resultado |
| --- | --- |
| Servidor | localhost:5432 |
| Versión | PostgreSQL 18.0 |
| Usuario | postgres |
| DB | compliance360 |
| Herramientas | `C:\Program Files\PostgreSQL\18\bin` |

## Evidencia generada

Salida completa:

`artifacts/migrations/performance-postgresql-audit-output.txt`

SQL utilizado:

`artifacts/migrations/performance-postgresql-audit.sql`

## Hallazgos

- `audit_logs` es la tabla más grande del dataset local con 824 kB.
- `report_history` tiene el mayor conteo `seq_scan` observado, pero con solo 96 filas.
- Hay muchos índices con `idx_scan = 0`, esperable en dataset local pequeño y base recién creada.
- No se detectó bloat ni locks con evidencia suficiente en esta muestra.
- El dataset no es representativo de producción; no permite certificar planes de ejecución enterprise.

## Recomendaciones

- Ejecutar `ANALYZE` después de cargas iniciales reales.
- Medir nuevamente con datos volumétricos por tenant.
- Evaluar `pg_stat_statements` para top SQL real si se habilita extensión.
- Revisar índices no usados solo después de tráfico real; no eliminarlos con dataset local.
- Mantener filtros `TenantId` en todas las queries multitenant.
