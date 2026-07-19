# 05 — Provider Center

Este documento forma parte de la línea base de diseño. La especificación exhaustiva de canales, adapters, providers, configuración efectiva, routing, failover, callbacks, secretos, APIs, permisos y pruebas está en [05_PROVIDER_ARCHITECTURE.md](./05_PROVIDER_ARCHITECTURE.md).

## Decisión

Provider Center será un módulo independiente y tenant-scoped. La base de datos será la fuente funcional de verdad; `appsettings` quedará limitado al bootstrap de plataforma. Los secretos serán write-only y se almacenarán mediante vault o envelope encryption.

## Capacidades aprobables

- Email: SMTP, Microsoft Graph, SendGrid, Mailgun, SES y Resend.
- InApp persistente.
- Adapters certificables para SMS, WhatsApp, Push, Teams, Slack, Webhook y API.
- Health, connection test, remitentes, dominios, cuotas, prioridad, weighted routing, residencia, circuit breaker y failover.
- Callback firmado, idempotente y protegido contra replay.
- Rotación de secretos sin downtime.
- Operación y observabilidad por tenant/canal/provider.

Ningún provider se declara soportado hasta tener adapter, contract tests, sandbox, runbook y evidencia de entrega real.

