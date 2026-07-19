# 23 — Regulatory Notification Certification

## Implementación mínima multirol

`RegulatoryAffairsService.NotifyAsync` → `INotificationService.QueueAsync` canal **InApp** dirigido a `TargetUserId` (owner/creator/responsible), **no** broadcast global.

| Evento | Subject | Evidencia |
|--------|---------|-----------|
| Approve-for-submission | `Aprobado para sometimiento` | `notification_messages` Status=Queued Channel=InApp (smoke-notify.ps1) |
| Submit | `Sometimiento registrado` | Código path activo |
| Observación abierta | `Observación recibida de autoridad` | Código path activo |

Smoke:

```
APPROVE_STATUS=ApprovedForSubmission
notification_messages_count >= 1
('Aprobado para sometimiento', 'InApp', 'Queued', ...)
```

Destinatario = owner/creator del dossier (no todos los usuarios RA).
