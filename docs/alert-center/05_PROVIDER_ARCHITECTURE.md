# 05 — Arquitectura de canales y proveedores

## 1. Separación conceptual

- **Canal:** Email, InApp, SMS, WhatsApp, Push, Teams, Slack, Webhook, API.
- **Provider:** implementación concreta: SMTP, Microsoft Graph, SendGrid, Mailgun, SES, Resend, Twilio, Meta, FCM, APNs, Teams, Slack o HTTP firmado.
- **Provider Configuration:** instancia tenant/environment con remitente, región, límites y referencia de secreto.
- **Routing Policy:** selección, prioridad, peso, residencia, health y failover.

## 2. Contratos

- `INotificationChannelAdapter`
- `INotificationProvider`
- `INotificationProviderFactory`
- `IProviderConfigurationRepository`
- `IProviderSecretStore`
- `IProviderHealthService`
- `IProviderRouter`
- `IRateLimiter`
- `ICircuitBreaker`
- `IDeliveryCallbackVerifier`
- `IDeliveryStatusNormalizer`

Contrato mínimo del provider:

```text
ValidateConfiguration
CheckHealth
SendAsync(idempotencyKey, renderedPayload, cancellationToken)
NormalizeResponse
VerifyAndParseCallback
GetCapabilities
```

## 3. Proveedores por canal

### Email

- SMTP con STARTTLS/TLS, auth segura y timeouts.
- Microsoft 365 Graph con OAuth2/client credentials y certificados.
- SendGrid API.
- Mailgun API.
- Amazon SES API/SigV4.
- Resend API.

### Otros

- InApp persistente, sin provider externo.
- SMS/WhatsApp: adapters certificados antes de habilitar.
- Push: FCM/APNs.
- Teams: Graph o webhook soportado.
- Slack: app OAuth/webhook soportado.
- Webhook/API: HTTPS, firma, mTLS opcional y allowlist.

“Soportado” exige implementation + contract tests + sandbox + callback + operations runbook.

## 4. Wizard de configuración

1. Canal y tipo.
2. Scope tenant/plataforma, ambiente y residencia.
3. Nombre, owner y tags.
4. Endpoint/region.
5. Sender identity/domain/app.
6. Referencia de secreto.
7. Timeouts, cuotas y rate limit.
8. Routing/failover.
9. Health check.
10. Test a allowlist.
11. Revisión/aprobación.
12. Activación gradual.

Secretos son write-only y la API responde únicamente `configured`, versión, expiry y last rotated.

## 5. Configuración efectiva

La base de datos es la fuente funcional de verdad. `appsettings` contiene solo:

- bootstrap del vault;
- claves de plataforma no tenant-scoped;
- flags operacionales de emergencia;
- límites máximos no configurables por cliente.

Precedencia:

1. kill switch de plataforma;
2. política de residencia/seguridad;
3. provider tenant activo;
4. routing tenant;
5. fallback de plataforma autorizado;
6. fallo explícito.

La UI no puede declarar una configuración sana si el dispatcher no la consume.

## 6. Routing y failover

Inputs:

- tenant, canal y ambiente;
- residencia/país/autoridad;
- prioridad;
- provider health;
- cuota/rate limit;
- coste;
- sender/domain;
- payload capabilities;
- reglas de cliente.

Algoritmos:

- priority ordered;
- weighted;
- geo/residency;
- least loaded;
- cost-aware dentro de SLO;
- sticky por conversation/idempotency key.

Failover no se ejecuta cuando el resultado es ambiguo sin antes reconciliar. Si el provider pudo aceptar el mensaje pero no respondió, el estado es `Unknown`, no `Failed`.

## 7. Retry

Clasificación:

- transient: timeout seguro, 408, 429, 5xx, provider unavailable;
- permanent: address invalid, policy rejection, unsubscribed, payload invalid;
- authentication/configuration: circuit open e incidente, no retry infinito;
- unknown outcome: reconcile antes de retry.

Backoff:

```text
delay = min(maxDelay, random(baseDelay, previousDelay * 3))
```

Respeta `Retry-After`, expiry y prioridad. El límite se aplica por mensaje/provider/canal.

## 8. Circuit breaker

Estados Closed/Open/HalfOpen. Métricas por provider config y canal:

- error ratio;
- consecutive failures;
- latency;
- auth failures;
- quota exhaustion.

Acciones auditadas: open automático, override manual, test half-open, recover y forced disable.

## 9. Callbacks

Endpoint público por provider:

`POST /api/v2/alert-center/providers/{providerCode}/callbacks/{tenantPublicId}`

Controles:

- HTTPS;
- HMAC/signature/certificate validation;
- timestamp y replay window;
- unique provider event ID;
- body size/content type;
- rate limit;
- raw payload cifrado con retención corta;
- ACK rápido y procesamiento asíncrono;
- normalización a Accepted/Delivered/Read/Bounce/Complaint/Rejected;
- correlation por provider message ID;
- unknown callback quarantine.

## 10. Salud y observabilidad

Health separado:

- configuration valid;
- secret reachable;
- network reachable;
- auth valid;
- sender verified;
- quota;
- last real success;
- callback freshness.

Una prueba sintética nunca sustituye delivery real.

Métricas:

- attempts, accepted, delivered, failed;
- latency p50/p95/p99;
- rate-limit/quota;
- failover;
- circuit transitions;
- callback lag;
- unknown outcomes;
- coste.

Logs/traces excluyen body, recipients, tokens y secrets.

## 11. Seguridad

- Vault o envelope encryption con AES-256-GCM.
- DEK por secret/version y KEK externalizable.
- Rotación sin downtime.
- OAuth2/certificates sobre passwords donde sea posible.
- TLS 1.2+ y validación estricta.
- SSRF prevention para HTTP/webhook.
- DNS/IP revalidation y bloqueo de redes internas.
- Header allowlist.
- egress control.
- sender/domain verification.
- PII minimization.
- mTLS opcional.

## 12. Permisos y SoD

- `ALERT.CHANNEL.READ/MANAGE/PAUSE`
- `ALERT.PROVIDER.READ/CREATE/UPDATE/TEST/ACTIVATE/RETIRE`
- `ALERT.PROVIDER.SECRET_MANAGE`
- `ALERT.PROVIDER.ROUTING_MANAGE`
- `ALERT.PROVIDER.CALLBACK_READ/REPROCESS`

Provider Administrator configura metadata; Secret Administrator gestiona credenciales; Approver activa. Ninguno necesita leer cuerpos de mensajes.

## 13. APIs

- CRUD `/channels`
- CRUD/lifecycle `/providers`
- `POST /providers/{id}/test`
- `POST /providers/{id}/health-check`
- `POST /providers/{id}/rotate-secret`
- CRUD/versiones `/routing-policies`
- CRUD `/senders` y `/domains`
- query/reprocess `/callbacks`
- `POST /channels/{id}/pause|resume`

## 14. Compatibilidad

- `INotificationProviderFactory` y dispatcher existentes se reutilizan detrás del router V2.
- `NotificationProviderConfiguration` se convierte en configuración efectiva.
- Provider options de `appsettings` se migran como defaults bootstrap sin copiar secretos a documentos.
- El sequential failover actual se reemplaza gradualmente por routing explícito.
- Los envíos V1 se etiquetan `Source=LegacyApi`.

## 15. Pruebas

- Contract test por provider.
- TLS/auth/secrets/rotation.
- Timeout antes/después de aceptación.
- 429/Retry-After/5xx/permanent.
- Routing, failover y residencia.
- Circuit breaker.
- Callback signature/replay/duplicate/out-of-order.
- Idempotency.
- SSRF/header injection.
- Sandbox delivery real.
- No secrets/PII en logs.
- Health y observabilidad.

