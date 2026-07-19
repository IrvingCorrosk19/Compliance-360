# 07 — Inbox Enterprise

La especificación completa de campana, inbox personal/equipo, lifecycle, realtime, historial y evidence packs está en [07_INBOX_AND_HISTORY.md](./07_INBOX_AND_HISTORY.md).

## Decisión

El inbox será una proyección persistente por usuario. SignalR será una aceleración recuperable, no la fuente de verdad.

## Capacidades

- No leídas, leídas, archivadas, eliminadas lógicamente, favoritas y pospuestas.
- Buscar, filtrar, ordenar, vistas guardadas y acciones masivas.
- Read, acknowledge, action y resolve como estados distintos.
- Inbox de equipo con assignment, reassignment y escalación.
- Deep-links a entidades de Regulatory, Workflow, Documents y QMS.
- Timeline completo y evidence pack.
- Responsive, dark mode y WCAG 2.2 AA.

La autorización se valida por tenant, destinatario, scope organizativo, recurso y permiso en cada operación.

