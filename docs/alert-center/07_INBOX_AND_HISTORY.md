# 07 — Inbox, lifecycle e historial

## 1. Principio

El inbox es persistente y consultable. SignalR, push o polling solo aceleran la actualización; nunca son la fuente de verdad.

Se separan:

- estado de la alerta de negocio;
- estado de la entrega por canal;
- estado de engagement por usuario;
- estado del trabajo/acción requerida.

## 2. Superficies

### Campana

- badge no leídas;
- críticas, vencidas y menciones;
- vista rápida;
- abrir, marcar leída, acknowledge y snooze;
- actualización en tiempo real recuperable.

### Mi Inbox

Pestañas:

- Todas.
- No leídas.
- Requieren acción.
- Críticas.
- Pospuestas.
- Archivadas.
- Favoritas.

Filtros: estado, prioridad, severidad, módulo, categoría, regla, entidad, canal, rango, SLA y requiere acción.

### Inbox de equipo

- owner, equipo, usuario, sin asignar;
- vencidas y próximas;
- asignar/reasignar;
- recordar/escalar;
- métricas de carga y acknowledgement.

### Detalle de alerta

- encabezado, severidad, status y SLA;
- contenido renderizado seguro;
- entidad y deep-link;
- recipients/channels autorizados;
- timeline;
- acciones;
- comentarios/adjuntos si se habilitan;
- explicación de la regla y resolución de audiencia;
- evidencia/export.

## 3. Lifecycle de alerta

Estados:

- `Open`
- `Acknowledged`
- `InProgress`
- `Resolved`
- `Closed`
- `Expired`
- `Cancelled`
- `Suppressed`
- `Reopened`

Reglas:

- Read no equivale a Acknowledged.
- Acknowledged no equivale a Resolved.
- Archive solo afecta la vista del usuario.
- Delete es soft delete del inbox, no de evidencia.
- Reopen requiere motivo y permiso.
- Close puede ser automático o manual según regla.
- Expired conserva acciones permitidas por política.

## 4. Engagement por usuario

- `Unread`
- `Read`
- `Archived`
- `DeletedFromInbox`
- `Pinned`
- `Snoozed`
- `Acknowledged`
- `Actioned`

Timestamps independientes. Las operaciones son idempotentes.

## 5. Acciones

- Abrir/leer.
- Marcar leída/no leída.
- Añadir/quitar favorito.
- Fijar/desfijar.
- Archivar/restaurar.
- Eliminar/restaurar de inbox.
- Snooze.
- Acknowledge.
- Asumir.
- Asignar/reasignar/delegar.
- Comentar.
- Completar acción.
- Resolver/cerrar/reabrir.
- Abrir entidad relacionada.
- Copiar deep-link.
- Exportar evidencia.

Toda acción valida tenant, destinatario/scope organizativo, permiso y estado.

## 6. Snooze

Opciones:

- duración;
- fecha/hora;
- siguiente día hábil;
- después de evento;
- hasta deadline.

Una alerta crítica puede prohibir o limitar snooze. Snooze no detiene SLA salvo política explícita.

## 7. Realtime

SignalR:

- grupo por tenant y usuario;
- grupo de equipo autorizado;
- eventos mínimos: item created/updated/removed y counter changed;
- payload sin contenido sensible;
- reconnect con cursor/version;
- invalidación de sesión revoca conexión.

Fallback: polling incremental con `sinceVersion`.

## 8. Historial canónico

Timeline append-only:

1. Evento capturado.
2. Regla/version evaluada.
3. Match/suppression/dedupe.
4. Destinatarios resueltos/excluidos.
5. Template/version/locale resuelto.
6. Mensaje queued/leased/rendered.
7. Intento/provider/resultado.
8. Retry/failover/DLQ.
9. Callback accepted/delivered/read/bounce/complaint.
10. Inbox read/ack/action.
11. SLA/escalamiento.
12. Resolución/cierre/reopen.
13. Export/acceso privilegiado.

Cada entrada: event ID, tenant, resource, actor, timestamp UTC, correlation, causation, source, before/after redacted, reason y integrity metadata.

## 9. Evidence pack

Generación asíncrona:

- manifest y hash;
- alert/rule/template versions;
- event snapshot redacted;
- recipient resolution;
- delivery attempts y callbacks;
- lifecycle/engagement;
- approvals/signatures;
- audit entries;
- timezone/locale;
- criterios de retención.

Formatos PDF + JSON/CSV según permiso. Descarga expira y queda auditada.

## 10. Privacidad

- destinatarios enmascarados;
- contenido sensible condicionado por permiso;
- minimización en listados;
- redaction independiente de metadata;
- legal hold;
- subject/body no se envían en eventos realtime;
- búsqueda sensible separada;
- no se indexan secrets;
- support no ve contenido sin JIT/break-glass.

## 11. APIs

Inbox:

- `GET /inbox`
- `GET /inbox/counts`
- `GET /inbox/{id}`
- `POST /inbox/{id}/read|unread|archive|restore|pin|unpin|snooze`
- `POST /inbox/bulk`
- `GET/PATCH /inbox/preferences`

Alertas:

- `GET /alerts`
- `GET /alerts/{id}`
- `POST /alerts/{id}/acknowledge|assign|delegate|resolve|close|reopen`
- `POST /alerts/{id}/comments`
- `GET /alerts/{id}/timeline`
- `POST /alerts/{id}/evidence-pack`

Operación:

- `GET /messages`
- `GET /messages/{id}`
- `GET /messages/{id}/timeline`
- `POST /messages/{id}/cancel|retry|resend`

Listados usan keyset pagination y ETag/version.

## 12. Permisos

- `ALERT.INBOX.READ_SELF`
- `ALERT.INBOX.MANAGE_SELF`
- `ALERT.INBOX.READ_TEAM`
- `ALERT.INBOX.MANAGE_TEAM`
- `ALERT.ALERT.READ`
- `ALERT.ALERT.READ_SENSITIVE`
- `ALERT.ALERT.ACKNOWLEDGE`
- `ALERT.ALERT.ASSIGN`
- `ALERT.ALERT.RESOLVE`
- `ALERT.ALERT.REOPEN`
- `ALERT.MESSAGE.READ`
- `ALERT.MESSAGE.READ_SENSITIVE`
- `ALERT.MESSAGE.CANCEL`
- `ALERT.MESSAGE.RETRY`
- `ALERT.MESSAGE.RESEND`
- `ALERT.EXPORT.CREATE`

## 13. Accesibilidad y responsive

- Lista/detalle en desktop; tarjetas y drawer en móvil.
- Foco vuelve al elemento de origen al cerrar detalle.
- Nuevos items se anuncian sin mover foco.
- Countdown SLA no actualiza agresivamente `aria-live`.
- Prioridad/estado usan texto e icono además de color.
- Timeline tiene representación lineal.
- Acciones masivas tienen resumen textual.
- Atajos configurables, nunca exclusivos.

## 14. Pruebas

- unread counter con concurrencia;
- read/archive/delete independientes;
- own/team/tenant authorization e IDOR;
- SignalR reconnect/session revocation;
- polling cursor;
- acknowledge/resolve/reopen transitions;
- snooze/SLA interaction;
- bulk atomicity/partial results;
- evidence integrity;
- PII masking;
- legal hold/retention;
- mobile/keyboard/screen reader/dark mode;
- performance con millones de inbox items.

