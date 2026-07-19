# 06 — Scheduler Enterprise

La especificación integral de Scheduler, Outbox, Queue, Workers, retry, DLQ, idempotencia, leases, digests y escalaciones está en [06_SCHEDULER_AND_QUEUE.md](./06_SCHEDULER_AND_QUEUE.md).

## Decisión

El scheduler utilizará PostgreSQL como almacenamiento durable inicial y un Worker .NET independiente. Soportará una vez, intervalos, minuto/hora/día/semana/mes, cron validado, fecha específica, calendarios laborales, timezone IANA, DST, feriados, quiet hours, SLA, escalaciones y digests.

## Garantías

- Persistencia antes de ejecución.
- `FOR UPDATE SKIP LOCKED`.
- Lease con compare-and-set.
- Idempotencia por schedule e instante.
- Misfire policy explícita.
- Recuperación tras reinicio.
- Horizontal scaling.
- Backpressure y fairness tenant.
- Métricas de drift, lag, depth y age.

La UI ofrecerá un builder funcional y preview de próximas ejecuciones; cron no será la única experiencia.

