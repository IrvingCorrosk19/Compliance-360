# 06 — Scheduler, Outbox, Queue y Workers

## 1. Objetivo

Garantizar que eventos, schedules, retries, SLA, digests, callbacks y exports sobrevivan reinicios y se procesen sin pérdida, con concurrencia horizontal e idempotencia.

## 2. Procesos

- **Web:** APIs/UI; no ejecuta delivery durable.
- **Alert Worker:** outbox, evaluación de reglas y creación de occurrences.
- **Delivery Worker:** claim, render y envío.
- **Scheduler Worker:** schedules, SLA, escalaciones, digests y retention.
- **Callback Worker:** normaliza callbacks.
- **Projection Worker:** inbox, rollups, analytics y exports.

Inicialmente pueden vivir en un único executable con colas separadas. En producción corren en servicio/container independiente.

## 3. Outbox

La transacción de negocio escribe:

1. cambio de dominio;
2. evento de outbox;
3. commit único.

El worker:

1. reclama lote con `FOR UPDATE SKIP LOCKED`;
2. asigna lease;
3. evalúa reglas;
4. crea occurrence/message idempotente;
5. marca publicado.

Un crash antes del paso 5 repite el evento; constraints/idempotency impiden duplicación lógica.

## 4. Claim de cola

Patrón:

```sql
WITH candidate AS (
    SELECT "Id"
    FROM compliance360.notification_messages
    WHERE "Status" IN ('Queued', 'RetryScheduled')
      AND "NotBeforeUtc" <= now()
      AND ("LeaseUntilUtc" IS NULL OR "LeaseUntilUtc" < now())
    ORDER BY "PriorityRank" DESC, "NotBeforeUtc", "CreatedAtUtc", "Id"
    FOR UPDATE SKIP LOCKED
    LIMIT @batchSize
)
UPDATE compliance360.notification_messages m
SET "Status" = 'Leased',
    "LeaseToken" = @leaseToken,
    "LeaseOwner" = @workerId,
    "LeaseUntilUtc" = now() + @leaseDuration
FROM candidate
WHERE m."Id" = candidate."Id"
RETURNING m.*;
```

Finalización usa compare-and-set por LeaseToken. El heartbeat extiende lease solo mientras el intento está vivo.

## 5. Idempotencia

Claves:

- evento: producer + aggregate + event ID;
- occurrence: tenant + rule version + event + dedupe bucket;
- target: occurrence + endpoint + channel + class;
- message: occurrence target + template version;
- provider: idempotency key transmitida si se soporta;
- callback: provider config + external event ID;
- schedule: schedule + scheduled instant;
- API: tenant + operation + request key.

Una clave se vincula al hash del request; reutilizarla con payload diferente devuelve 409.

## 6. Estados de mensaje

```text
Queued → Leased → Rendering → Dispatching → Accepted → Delivered
  │         │           │           │
  ├─────────┴───────────┴──────────→ RetryScheduled → Queued
  ├────────────────────────────────→ FailedPermanent → DeadLetter
  └────────────────────────────────→ Cancelled/Expired
```

`Unknown` se usa cuando el proveedor pudo aceptar pero no respondió. Requiere reconciliación.

No se permite pasar de Accepted a Cancelled. Un callback tardío puede enriquecer Delivered/Read, no regresar estado.

## 7. Scheduler

Soporta:

- una vez;
- intervalo;
- minutely/hourly/daily/weekly/monthly;
- nth weekday;
- cron validado;
- business calendar;
- digest;
- date-offset sobre campos de entidad;
- SLA timers;
- expiraciones y housekeeping.

Configuración:

- timezone IANA;
- start/end;
- next N run preview;
- misfire policy: Skip, FireOnce, CatchUpLimited, Reschedule;
- max catch-up;
- jitter;
- blackout;
- business hours y holidays.

La UI no usa cron como única experiencia; ofrece builder y muestra equivalencia.

## 8. DST y calendarios

Políticas explícitas:

- hora inexistente: skip o siguiente instante válido;
- hora ambigua: first, second o once;
- holidays: skip, previous business day o next business day;
- tenant, user y entity timezones con precedencia declarada;
- cada run guarda scheduled local time, timezone y UTC result.

## 9. SLA y escalaciones

Al crear alerta:

- se calcula deadline desde versión de SLA/calendario;
- se crean timers idempotentes;
- acknowledge/resolve cancela timers futuros;
- cambios de policy no recalculan instancias existentes salvo migración aprobada;
- escalation step crea evento nuevo y conserva parent occurrence;
- no se permiten ciclos ni repetir el mismo destinatario sin política.

## 10. Digests

1. Acumular referencias, no cuerpos.
2. Bucket por tenant/user/policy/locale/timezone.
3. Close atomizado.
4. Resolver contenido vigente.
5. Enviar un mensaje con ítems y deep-links.
6. Si falla, retry del digest sin perder members.

Alerts mandatory/critical bypass digest según política.

## 11. Retry y DLQ

Retry:

- error clasificado;
- decorrelated jitter;
- `Retry-After`;
- TTL/expiry;
- límite de attempts;
- fallback provider;
- no retry de permanentes.

DLQ:

- creación automática al agotar política;
- owner/assignment;
- payload redacted;
- causa normalizada;
- reparación limitada;
- requeue como nueva ejecución correlacionada;
- descarte con motivo;
- evidencia original inmutable.

## 12. Backpressure y fairness

- límites globales, tenant, canal y provider;
- weighted fairness para evitar noisy neighbor;
- reserved capacity para prioridades críticas;
- batch size adaptativo;
- pausa tenant/canal/provider;
- circuit breaker;
- worker concurrency configurable con máximos de plataforma;
- monitoring de queue age, no solo depth.

## 13. Shutdown y recuperación

- readiness false antes de detener claims;
- grace period para finalizar;
- cancelar I/O;
- leases expiran y otro worker recupera;
- no marcar success sin resultado persistido;
- startup recupera leases vencidos, schedule misfires y callbacks pendientes.

## 14. Observabilidad

Métricas:

- outbox lag/depth;
- queue depth/age;
- leases active/expired;
- claim/throughput;
- dispatch latency;
- retries/DLQ;
- schedule drift/misfires;
- duplicate prevented;
- provider latency/error;
- callback lag;
- projection lag.

Tracing:

`business transaction → outbox → rule → occurrence → target → message → attempt → provider → callback → inbox`.

Correlation ID nunca sustituye tenant/resource authorization.

## 15. Administración

UI:

- worker health y heartbeat;
- queue/partition/channel/provider status;
- pause/resume;
- drain;
- retry/cancel/requeue;
- release expired lease;
- schedule run-now;
- DLQ remediation;
- no edición directa de estados.

Acciones peligrosas requieren permiso, motivo, preview, confirmación y AuditLog.

## 16. APIs operativas

- `GET /operations/health`
- `GET /operations/queues`
- `POST /operations/queues/{name}/pause|resume`
- `GET /messages`
- `POST /messages/{id}/cancel|retry|resend`
- `GET /dead-letters`
- `POST /dead-letters/{id}/assign|requeue|discard`
- CRUD `/schedules`
- `POST /schedules/{id}/run-now|pause|resume`
- `GET /schedule-runs`
- `GET /workers`

## 17. Deployment inicial

```text
postgres
web
worker
mailpit (sandbox profile only)
nginx
```

Web y worker usan la misma imagen, comandos distintos, usuarios no root, health checks, limits y graceful stop.

## 18. Pruebas obligatorias

- crash antes/después de side effect;
- worker restart;
- duplicate outbox;
- competing workers;
- lease expiry/heartbeat;
- idempotency conflict;
- provider timeout ambiguo;
- retry/backoff/expiry;
- DLQ/requeue;
- cancel race;
- callback duplicate/out-of-order;
- DST spring/fall;
- holidays/misfires;
- noisy tenant/fairness;
- backlog recovery;
- graceful shutdown;
- million-row query plans;
- loss/reconciliation invariant = 0.

