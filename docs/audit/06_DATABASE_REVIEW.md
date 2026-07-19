# Revisión de base de datos PostgreSQL

## Resultado

Base de datos: **72/100** para diseño, **55/100** para operación Enterprise.

## Evidencia productiva

- 163 tablas y 163 claves primarias.
- 111 claves foráneas.
- 480 índices; 0 inválidos.
- Migraciones EF Core versionadas.
- Índices parciales únicos protegen correcciones, evidencias actuales, reaperturas y overrides.
- Triggers PostgreSQL hacen append-only los eventos de cambio e historial regulatorio.
- No se encontró Row Level Security habilitado.
- No se encontró scheduler de backups ni evidencia de restore periódico.

## Evaluación

| Área | Evidencia | Riesgo / impacto | Recomendación | Esfuerzo | Prioridad |
|---|---|---|---|---:|---|
| Normalización | Entidades y FK extensas | Buena integridad; joins crecen con módulos | Revisar agregados y vistas de lectura | 2–3 sem | P2 |
| Índices | 480, 0 inválidos | Sobreindexación posible y coste de escritura | `pg_stat_user_indexes`, eliminar no usados tras ventana | 2 sem | P1 |
| FK | 111 FK | No se verificó índice de soporte para cada FK | Auditoría automática de FK sin índice | 2 d | P1 |
| Multi-tenant | `TenantId` controlado por aplicación; RLS ausente | Un filtro omitido puede producir fuga entre tenants | RLS defense-in-depth o repositorios con guard obligatorio | 4–8 sem | P0 |
| Integridad tenant | Snapshot: 111 FK, 0 FK compuestas `(TenantId, Id)` detectadas | Referencias cross-tenant pueden persistirse por script/bug | Claves candidatas y FK compuestas por fases | 2–4 sem | P0 |
| Concurrencia | `Revision` como concurrency token | Correcto para edición simultánea | E2E concurrente y respuesta 409 uniforme | 1–2 sem | P1 |
| Auditoría | Triggers append-only | Fuerte evidencia de inmutabilidad | Roles DB separados y pruebas de tamper | 1 sem | P1 |
| Migraciones | Historial amplio e idempotencia reciente | Migración larga puede bloquear producción | Dry-run, lock timeout, backup y rollback ensayado | 1–2 sem | P0 |
| Continuidad | Dump manual, sin job automático encontrado | Pérdida de datos/RPO no garantizado | Backups cifrados, PITR, restore trimestral | 1–3 sem | P0 |

## Escalabilidad

PostgreSQL es adecuado para el producto. Antes de particionar o introducir otra base deben medirse tamaños, bloat, top SQL y crecimiento por tenant. Para escala multinacional: réplicas de lectura, pooling, PITR, observabilidad de locks y estrategia explícita de archivado.

## Límite de la certificación

La ausencia de RLS no prueba una vulnerabilidad por sí sola, pero elimina una defensa relevante. La existencia de archivos dump tampoco prueba recuperabilidad: solo un restore cronometrado demuestra RPO/RTO.

La migración `20260718115954_AddRegulatoryWorkflowV2ControlledFlexibility.cs` declara un `Down` no soportado. Los gates actuales generan SQL/manifiestos, pero la evidencia revisada no muestra aplicación del esquema a PostgreSQL efímero ni restore/downgrade real.
