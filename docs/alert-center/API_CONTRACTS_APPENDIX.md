# Apéndice — Contratos API V2

## 1. Convenciones

Base tenant:

```text
/api/v2/tenants/{tenantId}/alert-center
```

Base plataforma:

```text
/api/v2/platform/alert-center
```

Reglas comunes:

- `application/json`; timestamps ISO-8601 UTC.
- `ProblemDetails` con `code`, `correlationId` y errores de campo.
- `Idempotency-Key` en creates/actions con side effects.
- `If-Match`/ETag en updates y lifecycle.
- Keyset pagination: `limit`, `after`, `sort`.
- Filtros allowlisted.
- `X-Correlation-Id` aceptado como diagnóstico, nunca como autorización.
- Toda respuesta preserva tenant isolation y masking.
- `202 Accepted` para jobs/envíos; `Location` al recurso.
- `409` para lifecycle/concurrency/idempotency conflicts.
- `422` para regla/configuración semánticamente inválida.
- `429` con `Retry-After`.

## 2. Recursos

### Dashboard y analytics

- `GET /dashboard`
- `GET /analytics/delivery`
- `GET /analytics/effectiveness`
- `GET /analytics/sla`
- `POST /exports`
- `GET /exports/{id}`
- `GET /exports/{id}/download`
- `POST /exports/{id}/cancel`

### Eventos y variables

- `GET/POST /event-types`
- `GET/PATCH /event-types/{id}`
- `POST /event-types/{id}/versions`
- `POST /event-type-versions/{id}/validate`
- `POST /event-type-versions/{id}/publish|deprecate`
- `POST /events/ingest`
- `GET/POST /variables`
- `GET/PATCH /variables/{id}`
- `POST /variables/{id}/validate|deprecate`

### Reglas

- `GET/POST /definitions`
- `GET/PATCH /definitions/{id}`
- `POST /definitions/{id}/versions`
- `GET/PATCH /definition-versions/{id}`
- `POST /definition-versions/{id}/validate|simulate|submit|request-changes|approve|reject|publish`
- `POST /definitions/{id}/activate|pause|resume|rollback|retire`
- `GET /definitions/{id}/usage|executions|metrics`
- `POST /simulations`
- `GET /simulations/{id}`

### Audiencias

- `GET/POST /audiences`
- `GET/PATCH /audiences/{id}`
- `POST /audiences/{id}/versions`
- `POST /audience-versions/{id}/validate|simulate|submit|approve|publish`
- `POST /audiences/{id}/refresh|snapshot|retire`
- `GET /audiences/{id}/members|usage`
- `POST /recipient-resolution/simulate`

### Templates

- `GET/POST /templates`
- `GET/PATCH /templates/{id}`
- `POST /templates/{id}/versions`
- `GET/PATCH /template-versions/{id}`
- `POST /template-versions/{id}/validate|preview|test-send|submit|request-changes|approve|reject|publish`
- `POST /templates/{id}/retire|rollback`
- `GET /templates/{id}/usage`
- CRUD `/layouts`, `/blocks`, `/themes`, `/assets`

### Providers y canales

- `GET/POST /channels`
- `GET/PATCH /channels/{id}`
- `POST /channels/{id}/pause|resume`
- `GET/POST /providers`
- `GET/PATCH /providers/{id}`
- `POST /providers/{id}/test|health-check|submit|approve|activate|pause|retire`
- `POST /providers/{id}/secrets|rotate-secret`
- `GET/POST /routing-policies`
- `GET/PATCH /routing-policies/{id}`
- `POST /routing-policies/{id}/simulate|publish`
- CRUD `/senders`, `/domains`
- `GET /provider-callbacks`
- `POST /provider-callbacks/{id}/reprocess`

### Schedules, SLA y escalación

- `GET/POST /schedules`
- `GET/PATCH /schedules/{id}`
- `POST /schedules/{id}/validate|preview|run-now|pause|resume|retire`
- `GET /schedule-runs`
- `GET/POST /business-calendars`
- `POST /business-calendars/{id}/versions`
- CRUD/versiones/lifecycle `/sla-policies`
- CRUD/versiones/lifecycle `/escalation-policies`
- CRUD/versiones/lifecycle `/digest-policies`

### Inbox y alertas

- `GET /inbox`
- `GET /inbox/counts`
- `GET /inbox/{id}`
- `POST /inbox/{id}/read|unread|archive|restore|pin|unpin|snooze`
- `POST /inbox/bulk`
- `GET/PATCH /preferences`
- `GET /team-inbox`
- `GET /alerts`
- `GET /alerts/{id}`
- `POST /alerts/{id}/acknowledge|assign|delegate|resolve|close|reopen`
- `POST /alerts/{id}/comments`
- `GET /alerts/{id}/timeline`
- `POST /alerts/{id}/evidence-pack`

### Operación

- `GET /operations/health|queues|workers`
- `POST /operations/queues/{name}/pause|resume`
- `GET /messages`
- `GET /messages/{id}`
- `GET /messages/{id}/timeline`
- `POST /messages/{id}/cancel|retry|resend`
- `GET /dead-letters`
- `GET /dead-letters/{id}`
- `POST /dead-letters/{id}/assign|repair|requeue|discard`
- `GET /outbox`
- `GET /callbacks`

### Auditoría y gobierno

- `GET /audit`
- `GET /audit/{id}`
- `POST /audit/exports`
- `POST /audit/verify`
- CRUD/lifecycle `/retention-policies`
- CRUD `/legal-holds`
- `GET/POST /promotion-packages`
- `POST /promotion-packages/{id}/validate|submit|approve|promote|rollback`
- CRUD `/feature-flags`

### Plataforma

- `GET /fleet`
- `GET/PATCH /tenants/{tenantId}/limits`
- `GET/PATCH /tenants/{tenantId}/features`
- `POST /tenants/{tenantId}/support-sessions`
- `POST /support-sessions/{id}/close`
- CRUD `/shared-providers`
- CRUD `/incidents`

## 3. DTOs transversales

### Page

```json
{
  "items": [],
  "nextCursor": "opaque",
  "hasMore": false,
  "snapshotAtUtc": "2026-07-19T16:00:00Z"
}
```

### Action

```json
{
  "reason": "Required business reason",
  "expectedVersion": 7,
  "comment": "Optional",
  "effectiveAtUtc": null
}
```

### Job

```json
{
  "id": "uuid",
  "status": "Queued",
  "resourceType": "Export",
  "resourceId": "uuid",
  "submittedAtUtc": "2026-07-19T16:00:00Z",
  "correlationId": "uuid"
}
```

## 4. Contratos de eventos

Envelope:

```json
{
  "eventId": "uuid",
  "eventType": "regulatory.dossier.status-changed",
  "schemaVersion": 1,
  "tenantId": "uuid",
  "occurredAtUtc": "2026-07-19T16:00:00Z",
  "producer": "Compliance360.RegulatoryAffairs",
  "aggregateType": "Dossier",
  "aggregateId": "uuid",
  "correlationId": "uuid",
  "causationId": "uuid",
  "actor": { "type": "User", "id": "uuid" },
  "data": {}
}
```

El productor no incluye secretos. Los campos PII se clasifican en el registry.

## 5. Versionado y compatibilidad

- APIs V2 son aditivas.
- Nunca se cambia semántica de un campo publicado.
- Campos nuevos son opcionales o usan nueva versión.
- Event schemas usan compatibility checks.
- V1 se mantiene por dos releases después del cutover.
- Deprecation incluye headers, telemetría, fecha y migration guide.

## 6. OpenAPI

La implementación debe generar OpenAPI y comprobar:

- operation IDs estables;
- security scopes;
- examples;
- ProblemDetails;
- pagination;
- idempotency/ETag headers;
- deprecations;
- schema compatibility en CI.

