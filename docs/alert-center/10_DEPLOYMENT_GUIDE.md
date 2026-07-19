# 10 — Roadmap, migración y deployment

## 1. Principio

La migración es progresiva: expand, dual-write, shadow, canary, cutover y contract. No se elimina funcionalidad ni evidencia legacy durante las fases de compatibilidad.

Estimación de planificación, no compromiso: 20–30 semanas para un equipo dedicado con Product, UX, Architecture, Backend, Frontend, QA, Security, DevOps y Regulatory Validation.

## 2. Fase 0 — Baseline y contención

Objetivo: preparar una base medible.

- aprobar documentos 01–12;
- ADRs y threat model;
- inventario de eventos/proveedores/templates;
- congelar nuevos envíos ad hoc;
- corregir GET con side effects;
- schema expand inicial;
- feature flags;
- observabilidad baseline;
- eliminar secretos del repo/historial operativo;
- backups y rehearsal de restore.

Gate: diseño aprobado, zero secret exposure y plan rollback.

## 3. Fase 1 — Delivery durable e Inbox

Módulos:

1. Outbox y contratos de eventos.
2. Queue/worker/leases/idempotency.
3. Provider Center email + InApp.
4. Retry/failover/DLQ/callbacks.
5. Inbox/campana/history.
6. Dashboard operacional inicial.

Compatibilidad:

- V1 sigue leyendo/escribiendo;
- dual-write;
- Regulatory/Workflow publican outbox;
- templates legacy migran como versión no aprobada;
- provider DB se vuelve efectivo por flag.

Gate:

- reinicio sin pérdida;
- entrega real email e InApp;
- retry/DLQ;
- inbox;
- tenant isolation;
- canary tenant.

## 4. Fase 2 — Configuración funcional

Módulos:

1. Event Catalog.
2. Rule Builder y simulation.
3. Audience/recipient resolver.
4. Variable Catalog.
5. Template Center multicanal/multidioma.
6. Approvals y versioning.

Gate:

- crear regla/template/audience sin código;
- maker-checker;
- simulation/replay;
- XSS/PII controls;
- migration de primeros flujos regulatorios.

## 5. Fase 3 — Scheduler, SLA y canales

Módulos:

1. Scheduler/calendars/DST.
2. SLA/escalations/digests.
3. SMS/WhatsApp/Push/Teams/Slack/Webhook adapters aprobados.
4. Routing, circuit breaker, quota/cost.
5. Analytics/rollups/exports.

Gate:

- schedules y DST;
- channel contract/delivery tests;
- provider outage/recovery;
- load targets;
- evidence packs.

## 6. Fase 4 — Gobierno y escala

Módulos:

1. Promotion packages.
2. Platform fleet console.
3. Data residency/retention/legal hold.
4. SLO/capacity/fatigue analytics.
5. Horizontal scale y, solo si métricas lo justifican, broker dedicado.
6. Validación regulatoria final.

Gate:

- DR/restore;
- 10 M+ dataset;
- multi-region/residency según producto contratado;
- security/accessibility sign-off;
- full UAT y release certification.

## 7. Orden de implementación por módulo

Cada módulo termina con:

1. domain/data/API;
2. migration/backfill;
3. UI/RBAC/audit;
4. unit/integration/E2E/security;
5. docs/runbook;
6. build/test clean;
7. demo funcional;
8. commit local solo si el usuario mantiene esa autorización;
9. gate formal.

No se inicia cutover de un flujo hasta que el módulo precedente es durable.

## 8. Feature flags

- `AlertCenter.Enabled`
- `AlertCenter.OutboxCapture`
- `AlertCenter.RuleEvaluationShadow`
- `AlertCenter.DualWrite`
- `AlertCenter.ProviderConfigFromDatabase`
- `AlertCenter.DeliveryWorker`
- `AlertCenter.Inbox`
- `AlertCenter.LegacyReadAdapter`

Scope: environment → tenant → module → channel → rule. Toda modificación queda auditada y puede expirar.

## 9. Topología Docker/VPS

```text
Internet
  → Nginx/TLS
      → compliance360_web (API/UI)
      → public callback endpoints

Internal network
  → compliance360_worker
  → compliance360_postgres
  → object storage
  → secret store
  → mailpit (sandbox only)
```

Requisitos:

- imágenes versionadas/inmutables;
- web/worker non-root;
- read-only rootfs donde sea viable;
- CPU/memory/pids limits;
- health checks;
- graceful stop;
- persistent DB backups fuera del host;
- secret mounts;
- TLS;
- logs/metrics/traces;
- no publicar PostgreSQL.

Sandbox SMTP local:

```powershell
docker compose --profile sandbox up -d mailpit
```

- SMTP: `mailpit:1025` desde la red Compose o `127.0.0.1:1025` desde el host.
- UI de inspección: `http://127.0.0.1:8025`.
- El perfil `sandbox` no se inicia en producción por defecto.

Procesos:

```powershell
dotnet ef database update --project src/Compliance360.Infrastructure --startup-project src/Compliance360.Web
dotnet run --project src/Compliance360.Web
dotnet run --project src/Compliance360.Worker
```

## 10. Pipeline

1. Restore/install reproducible.
2. Format/lint.
3. Build Release.
4. Unit/contract.
5. PostgreSQL integration/migrations.
6. API/E2E.
7. Security/dependency/secrets.
8. Container build/scan/SBOM/sign.
9. Deploy ephemeral.
10. Smoke/resilience.
11. Approval.
12. Promote same artifact.

## 11. Migración DB

Pre-deploy:

- backup verificado;
- restore rehearsal;
- migration dry-run sobre copia;
- size/locks/timeout;
- invalid indexes;
- checkpoint.

Expand:

- tablas/columnas nullable;
- indexes concurrently cuando sea necesario;
- constraints NOT VALID;
- backfill batches;
- validate;
- RLS tras confirmar contexto.

No se usan `Down` destructivos como rollback de producción.

## 12. Canary y cutover

Rings:

- internal tenant;
- sandbox customer;
- low-volume customer;
- 5%;
- 25%;
- 50%;
- 100%.

Por ring:

- event count;
- occurrence/message reconciliation;
- delivery success;
- queue age/DLQ;
- legacy parity;
- latency/error;
- support feedback.

Rollback:

- detener nuevos claims;
- desactivar flags;
- drenar o preservar leases;
- volver a legacy adapter;
- reconciliar in-flight;
- conservar schema/data V2.

## 13. Deployment checklist

Antes:

- [ ] artifact/hash/SBOM aprobados
- [ ] backup/restore verificados
- [ ] secrets/providers configurados
- [ ] migration dry-run
- [ ] canary y rollback owner
- [ ] dashboards/alerts/runbooks
- [ ] change window/on-call

Durante:

- [ ] pause workers si migration lo exige
- [ ] expand migration
- [ ] deploy web/worker
- [ ] health/readiness
- [ ] canary flags
- [ ] smoke/event-to-delivery
- [ ] callback/inbox/history

Después:

- [ ] row/event/message reconciliation
- [ ] queue age/DLQ
- [ ] provider health
- [ ] security headers/RBAC/tenant
- [ ] no secrets/PII logs
- [ ] acceptance sign-off

## 14. Riesgos

| Riesgo | Mitigación |
|---|---|
| Duplicación V1/V2 | idempotencia, dual-write ledger, reconciliation |
| Pérdida durante crash | outbox y durable queue |
| Resultado provider ambiguo | Unknown + reconciliation |
| RLS rompe procesos | roles/context tests y rollout por tabla |
| Backfill/locks | batches, indexes concurrent, timeout |
| Fatiga | dedupe/throttle/digest/analytics |
| Configuración insegura | approvals, simulation, limits |
| Secret leak | vault/write-only/scanning |
| UI monolítica | módulos lazy-loaded y contratos |
| Proveedor degradado | health/routing/circuit/failover |
| Scope excesivo | gates por módulo y canales certificados |

## 15. Operación

SLO iniciales:

- disponibilidad API/worker;
- event-to-queued;
- queue age;
- delivery latency;
- callback lag;
- DLQ ratio;
- inbox projection lag.

Runbooks:

- queue backlog;
- worker unhealthy;
- provider outage/auth/quota;
- callback invalid;
- DLQ spike;
- tenant noisy;
- database lock/storage;
- secret rotation;
- rollback;
- disaster recovery.

