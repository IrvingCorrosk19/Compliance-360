# PERFORMANCE_BASELINE

## Contexto

**Proyecto:** Compliance 360  
**Base de datos:** PostgreSQL 18 local  
**Database:** `compliance360`  
**Schema:** `compliance360`  
**Backup verificado:** `artifacts/backups/compliance360-20260624-191441.dump`  
**Backup TOC:** `artifacts/backups/last-backup-toc.txt`  

## Estado inicial medido

| Validación | Resultado |
| --- | --- |
| Backup completo | OK |
| Integridad backup con `pg_restore -l` | OK |
| Build | OK, 0 warnings, 0 errors |
| Tests | OK, 218 passed |
| Health check | OK |
| Lints archivos tocados | OK |

## Métricas HTTP locales

Prueba local sobre `GET /health`, 10 llamadas.

| Métrica | Resultado |
| --- | ---: |
| Cold start / primera llamada | 1007.94 ms |
| Promedio incluyendo cold start | 125.71 ms |
| Mínimo caliente | 23.55 ms |
| Máximo | 1007.94 ms |
| Rango caliente observado | 23-31 ms |

## Memoria local observada

| Métrica | Resultado |
| --- | ---: |
| Working set | 133.59 MB |
| Private memory | 116.94 MB |

## Baseline PostgreSQL

| Métrica | Resultado |
| --- | --- |
| Versión | PostgreSQL 18.0 |
| Tablas auditadas | 122 tablas schema `compliance360` |
| Tabla más grande observada | `audit_logs`, 824 kB |
| Mayor `seq_scan` observado | `report_history`, 32 |
| Dataset | Local pequeño, no volumétrico |

## Hallazgos principales

- Hay consultas EF con múltiples `Include` de colecciones que pueden generar cartesian product y warnings `MultipleCollectionIncludeWarning`.
- Varios dashboards cargaban colecciones completas para calcular conteos simples en memoria.
- La base local es pequeña; los tiempos no representan carga enterprise real.
- No se ejecutaron pruebas 100/500/1000/2000/5000 usuarios en esta fase.

## Estado de certificación

**No certificado todavía para producción de alto volumen.**  
Se completó baseline, backup y primera ola de optimización EF segura. Falta ejecutar pruebas de carga y medición antes/después con dataset volumétrico.
