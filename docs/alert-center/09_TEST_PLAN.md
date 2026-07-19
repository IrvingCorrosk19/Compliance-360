# 09 — Plan maestro de pruebas

## 1. Regla de certificación

Una pantalla existente, una API que responde o una prueba con mocks no certifican Alert Center. La aceptación requiere evidencia del flujo real:

`evento → outbox → regla → audiencia → template → cola → worker → provider/inbox → callback → historial`.

No se afirmará PASS antes de implementar y ejecutar este plan.

## 2. Pirámide de pruebas

- Dominio/unitarias: AST, lifecycle, retry, schedule, dedupe, routing, rendering.
- Contract: schemas de eventos, APIs y adapters de provider.
- Integración PostgreSQL real: RLS, constraints, SKIP LOCKED, migrations.
- Component: worker + DB + provider sandbox.
- API: auth, tenant, ProblemDetails, idempotency y concurrency.
- E2E browser: 51 superficies y roles.
- Delivery real: provider sandbox/controlado y callbacks.
- Performance/resilience/security/accessibility.
- UAT y validation evidence.

EF InMemory no valida persistencia empresarial; las pruebas críticas usan PostgreSQL compatible con producción.

## 3. Suites funcionales

### Dashboard

- métricas pendientes/enviadas/fallidas/canceladas/retry/DLQ;
- SLA, backlog, queue age y throughput;
- dimensiones módulo/canal/provider/prioridad/tenant/país/autoridad/usuario/workflow;
- filtros, drill-down y export;
- reconciliación contra consultas de control.

### Eventos y reglas

- catálogo, schema versions y deprecation;
- builder AND/OR/NOT/groups;
- tipos, null/unknown, fechas, colecciones y cambios;
- aggregation/window;
- simulation/replay/shadow;
- dedupe/throttle/quiet hours;
- lifecycle, diff, approval, publish, activate y rollback.

### Audiencias

- owner/creator/current/previous responsible;
- user/role/group/department/manager/on-call;
- approver/reviewer/submitter;
- dynamic expression;
- To/CC/BCC;
- preferences, consent, suppression y fallbacks;
- explicación de inclusión/exclusión.

### Templates

- visual/HTML/text/JSON;
- variables, locales, timezone, RTL;
- preview/test allowlist;
- branding/dark mode;
- versioning/approval;
- escaping/XSS/CRLF;
- attachments.

### Providers

- configuración efectiva DB;
- secret write-only/rotation;
- health/test;
- routing/residency/failover;
- retry/circuit breaker;
- delivery real;
- callback signature/replay/out-of-order.

### Scheduler/queue

- recurrencias, cron builder, DST y holidays;
- SLA/escalation/digest;
- outbox y worker restart;
- competing workers/lease;
- retry/backoff/DLQ/requeue/cancel;
- idempotency;
- backlog recovery.

### Inbox/history

- campana/counts;
- unread/read/archive/favorite/delete/snooze;
- acknowledge/action/resolve/reopen;
- own/team scope;
- realtime reconnect;
- timeline y evidence pack.

## 4. Matriz por rol

Cada rol se prueba con navegación, operación permitida y negativa:

- End User.
- Team Supervisor.
- Alert Operator.
- Functional Rule Designer.
- Template Designer.
- Compliance Reviewer.
- Alert Approver.
- Publisher.
- Production Activator.
- Provider Administrator.
- Secret Administrator.
- Auditor.
- Tenant Administrator.
- Platform SRE.

Negativas mínimas: route directa, API directa, resource de otro tenant, acción fuera de scope, self-approval, secret read y export sensible.

## 5. Integración con módulos

Se certifican productores reales:

- Regulatory Affairs V1 y V2.
- Workflow.
- Documents/versioning/signatures.
- CAPA/QMS.
- Risk.
- Audit findings.
- Suppliers.
- Indicators.
- Identity/RBAC.

Por productor:

1. transacción persiste;
2. outbox existe en el mismo commit;
3. evento cumple schema;
4. regla match/no-match;
5. no hay duplicación;
6. deep-link autorizado;
7. timeline correlacionado.

## 6. Persistencia y migraciones

- migrate desde producción snapshot;
- backfill reanudable;
- row counts/hashes;
- no huérfanos;
- constraints y RLS;
- dual-write reconciliation;
- legacy read/write compatibility;
- expand rollback;
- zero data loss;
- query plans con datos representativos.

## 7. Concurrencia y resiliencia

Escenarios:

- 2, 10 y N workers.
- Crash antes/después de provider call.
- Lease expiry/heartbeat.
- Duplicate event/callback.
- Cancel vs dispatch.
- Retry vs callback.
- Approve/publish races.
- Provider timeout ambiguo.
- DB restart.
- Worker rolling restart.
- Network partition.
- Provider outage/recovery.
- Queue saturation y noisy tenant.
- Disk/storage pressure.

Invariante: ningún evento elegible se pierde; ningún duplicate genera más de una intención lógica.

## 8. Performance

Objetivos iniciales a ratificar con negocio:

- API list p95 < 500 ms y p99 < 1 s a volumen nominal.
- Inbox count p95 < 250 ms.
- Claim p95 < 100 ms.
- Event-to-queued p95 < 2 s.
- Queue-to-attempt p95 < 5 s sin throttling/provider limit.
- Dashboard p95 < 2 s con rollups.
- UI LCP < 2.5 s e INP < 200 ms en perfil objetivo.

Cargas:

- 10 M mensajes;
- 50 M timeline rows;
- 5 M inbox items;
- burst 1,000 events/s;
- 1,000 tenants;
- 10,000 sesiones concurrentes;
- recuperación de backlog 1 M.

Se reporta hardware, dataset, scripts, percentiles y errores; no promedios aislados.

## 9. Seguridad

- OWASP ASVS.
- IDOR/multi-tenant/RLS.
- privilege escalation/SoD.
- auth/session/MFA.
- injection y XSS.
- SSRF/DNS rebinding.
- callback spoofing/replay.
- secrets/logs/exports.
- service account scopes.
- mass assignment.
- DoS/rate limit.
- file/attachment security.
- dependency/container/SBOM scans.

Se cierra todo hallazgo Critical/High; Medium requiere riesgo aprobado y plan.

## 10. Accesibilidad y UX

- WCAG 2.2 AA automated + manual.
- teclado completo.
- lectores NVDA/VoiceOver.
- zoom 200/400%.
- contrast/light/dark.
- responsive 360 px–desktop.
- reduced motion.
- status not color-only.
- timeout/error/offline/reconnect.
- 51 superficies con empty/loading/error/403/404/409.

## 11. Observabilidad

- traces end-to-end.
- correlation/causation.
- dashboards y alerts operacionales.
- logs sin PII/secrets.
- health liveness/readiness/startup.
- runbooks.
- audit reconciliation.

## 12. Evidencia

Por prueba:

- ID y requisito.
- Ambiente/build/commit.
- actor/tenant.
- precondición/data IDs.
- pasos.
- resultado esperado/real.
- API/DB/log/trace correlation.
- screenshot/video/trace.
- timestamps UTC.
- defect ID.
- estado y aprobador.

Evidencia se almacena fuera del código fuente cuando es voluminosa y conserva hash.

## 13. Gates

### Gate módulo

- build 0 errors;
- unit/integration/contract green;
- RBAC/audit/migration/UI;
- docs;
- no Critical/High.

### Gate release candidate

- full regression;
- provider sandbox delivery;
- concurrency/recovery;
- performance;
- security/accessibility;
- migration/rollback rehearsal;
- DR/backup restore;
- UAT.

### Gate producción

- approvals;
- immutable artifact/SBOM;
- backup;
- change window;
- feature flags/canary;
- monitoring/runbooks/on-call;
- post-deploy smoke;
- reconciliation;
- rollback criteria.

## 14. Criterio final

PASS requiere:

- 0 fallas;
- 0 tests skipped sin waiver aprobado;
- 0 defectos Critical/High abiertos;
- 100% permisos y tenant negatives;
- 100% journeys P0/P1;
- delivery real demostrada;
- worker/retry/DLQ/recovery demostrados;
- reconciliación sin pérdida;
- rollback ensayado;
- evidencia firmada.

