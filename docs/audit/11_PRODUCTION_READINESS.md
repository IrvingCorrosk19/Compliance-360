# Production readiness

## Estado

**FAIL — PRODUCTION NOT READY para uso Enterprise regulado crítico.**

## Gates observados

| Gate | Resultado | Evidencia |
|---|---|---|
| Build Release | PASS | 0 errores |
| Unit/integration .NET | PASS | 282/282 |
| Dependencias vulnerables | PASS en corte | .NET/npm sin hallazgos reportados |
| Roles responsive/i18n | PASS | 9/9 |
| Matriz manual vs pantallas/roles | PASS | TAC + 9 roles; sidebar, tabs, negativos y consola en 1.0 min |
| RA-ADM | PASS | escenario productivo |
| Quality Manager | PASS | escenario productivo |
| TAC | WARN/FLAKY | 1 fallo inicial; 1 repetición PASS |
| REGUTRACK escenarios 02–04 | PASS | 3/3 en 35 s |
| Platform Administrator | FAIL | usuario activo sin rol; acceso denegado |
| Cobertura CI | FAIL | 89.81% branch < 90%; reporte global 6.5% |
| Reproducibilidad CI | FAIL | pipelines invocan `build/ci/*.ps1`; `build/` está ignorado y esos scripts no están tracked |
| Playwright en CI | FAIL | 60 declaraciones E2E existen, pero GitHub/Azure no ejecutan Playwright |
| Format gate | FAIL | whitespace en 4 archivos |
| Docker build local | NOT VERIFIED | Docker daemon local no disponible |
| HTTPS | PASS | endpoint productivo accesible |
| HTTP seguro | FAIL | `http://164.68.99.83:8085` público |
| Container hardening | FAIL | root, sin read-only/limits |
| Backup/restore | FAIL | sin scheduler ni restore drill demostrado |
| Tenant DB defense-in-depth | FAIL | RLS ausente |

## VPS

Nginx actúa como reverse proxy y la aplicación interna se enlaza a loopback. Health checks y HTTPS responden. Esto demuestra disponibilidad puntual, no salud sostenida. Faltan evidencia de:

- alertas y SLO;
- backups programados/restaurados;
- rotación de logs;
- límites CPU/memoria/PID;
- escaneo y firma de imagen;
- despliegue automatizado con rollback probado.

## Criterios de salida

Para cambiar a PASS deben estar simultáneamente cerrados:

1. Platform Admin funcional y revisión completa de cuentas privilegiadas.
2. Todos los gates CI verdes sin reducir umbrales.
3. Suite E2E estable en al menos tres corridas limpias.
4. TLS obligatorio.
5. Runtime non-root endurecido.
6. Backup y restore exitoso con RPO/RTO medidos.
7. Controles cross-tenant y pruebas IDOR completas.
8. Workflow V1 mutante retirado.
9. Sin secretos activos en repo/historial.
10. Smoke post-deploy, monitoreo y rollback demostrados.
11. Versionar y ejecutar todos los scripts invocados por CI desde un checkout limpio.
12. Aplicar migraciones sobre PostgreSQL efímero y ejecutar Playwright en pipeline.

## Limitaciones

El build de imagen local no pudo ejecutarse porque Docker Desktop no estaba disponible. No se transforma esa limitación en PASS. Las pruebas productivas mutaron exclusivamente datos de certificación según los specs existentes.

Existe además un script destructivo, `scripts/remove-non-regutrack-tables.sql`, con múltiples `DROP TABLE ... CASCADE` y sin guardas de entorno/backup verificable. Debe retirarse de rutas operativas o envolverse con allowlist, dry-run y doble confirmación.
