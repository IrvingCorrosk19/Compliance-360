# 08 — Seguridad, RBAC, SoD y auditoría

## 1. Modelo de confianza

Amenazas prioritarias:

- acceso cross-tenant/IDOR;
- publicación o envío no autorizado;
- autoaprobación;
- robo/exposición de secretos;
- template/HTML/header injection;
- SSRF vía providers/webhooks;
- abuso de campañas;
- manipulación/replay de callbacks;
- fuga de PII en logs/exports;
- bypass de audit/retención;
- privilegios de soporte;
- agotamiento de cola/recursos.

Controles: deny-by-default, autorización por recurso, RLS, maker-checker, secretos write-only, validación estricta, rate limits, auditoría append-only y observabilidad.

## 2. Aislamiento tenant

Capas:

1. tenant derivado del token/session y validado contra route;
2. policies server-side;
3. query filters EF;
4. FK compuestas con TenantId;
5. RLS PostgreSQL;
6. cache keys, storage paths y realtime groups incluyen tenant;
7. workers reclaman y procesan con tenant explícito;
8. tests automatizados de IDOR.

`TenantId` del body nunca es autoridad. Platform support requiere scope y sesión JIT.

## 3. Catálogo de permisos

Formato `ALERT.<RESOURCE>.<ACTION>`.

Permisos mínimos:

- Dashboard/analytics: READ, EXPORT.
- Inbox: READ_SELF, MANAGE_SELF, READ_TEAM, MANAGE_TEAM.
- Alert: READ, READ_SENSITIVE, ACKNOWLEDGE, ASSIGN, RESOLVE, REOPEN.
- Message: READ, READ_SENSITIVE, CREATE, SEND, CANCEL, RETRY, RESEND, EXPORT.
- Event: READ, CREATE, UPDATE, DEPRECATE.
- Rule: READ, CREATE, UPDATE, SIMULATE, SUBMIT, REVIEW, APPROVE, PUBLISH, ACTIVATE, PAUSE, ROLLBACK, RETIRE.
- Template: READ, READ_SENSITIVE, CREATE, UPDATE, TEST, SUBMIT, REVIEW, APPROVE, PUBLISH, RETIRE.
- Audience/Variable: READ, CREATE, UPDATE, SIMULATE, RETIRE.
- Schedule/SLA/Escalation: READ, CREATE, UPDATE, APPROVE, OPERATE.
- Channel/Provider: READ, CREATE, UPDATE, TEST, ACTIVATE, PAUSE, RETIRE, SECRET_MANAGE.
- Queue/DLQ: READ, RETRY, CANCEL, REQUEUE, DISCARD.
- Audit: READ, EXPORT, VERIFY.
- Configuration/Promotion: READ, MANAGE, APPROVE, PROMOTE, ROLLBACK.
- Platform: FLEET_READ, TENANT_LIMIT_MANAGE, PROVIDER_MANAGE, BREAKGLASS.

## 4. Roles sugeridos

| Rol | Alcance |
|---|---|
| Alert End User | Inbox propio |
| Alert Team Supervisor | Inbox/equipo, SLA y assignment |
| Alert Operator | Queue, retry, cancel y DLQ |
| Functional Rule Designer | Draft, simulation y submit |
| Template Designer | Draft, preview y test |
| Compliance Reviewer | Review/comment/request changes |
| Alert Approver | Approve/reject |
| Alert Publisher | Publish approved versions |
| Production Activator | Activate/pause/rollback |
| Provider Administrator | Metadata/routing/test |
| Secret Administrator | Secret create/rotate |
| Alert Auditor | Audit/evidence read/export |
| Tenant Alert Administrator | Catálogos/roles tenant |
| Platform SRE | Fleet health sin contenido |

Son bundles configurables; no se programa lógica por nombre de rol.

## 5. Matriz SoD

Conflictos:

- Maker de regla ↔ approver de la misma versión.
- Maker de template ↔ approver de la misma versión.
- Approver ↔ publisher cuando la policy lo exige.
- Publisher ↔ production activator en tenants regulados.
- Provider admin ↔ secret admin.
- Campaign author ↔ campaign approver/sender.
- DLQ operator ↔ auditor de su propia remediación.
- Platform support ↔ tenant approver.
- Audit administrator ↔ persona auditada.

Excepciones:

- policy tenant;
- ticket;
- motivo;
- MFA step-up;
- duración;
- scope mínimo;
- aprobador;
- auditoría y revisión posterior.

## 6. Autenticación y sesiones

- JWT con session binding.
- MFA step-up para secrets, activación, campañas críticas, break-glass y exports sensibles.
- revocación inmediata;
- tokens cortos;
- service accounts con scopes mínimos;
- OAuth client credentials/certificates;
- no API keys perpetuas;
- CSRF cuando aplique al mecanismo de auth;
- rate limiting por sesión/client/endpoint.

## 7. Secretos

- Vault preferido; envelope encryption como fallback.
- No secretos en repo, appsettings, API responses, UI, logs, AuditLog o exports.
- Write-only UI.
- Rotación versionada y dual-read durante transición.
- Expiración, owner y alertas.
- Access audit.
- KEK fuera de la base.
- Masking no se considera cifrado.

## 8. Seguridad de templates y payloads

- contextual escaping;
- HTML sanitizer allowlist;
- CSP;
- CRLF/header injection prevention;
- link scheme/domain policy;
- JSON serialization;
- attachment scanning y allowlist;
- file hashes;
- no secrets en variables;
- PII classification y channel policy;
- preview/test allowlist.

## 9. Providers, webhook y SSRF

- URLs HTTPS y normalizadas;
- deny private/link-local/metadata IPs;
- DNS rebinding protection;
- egress allowlist;
- redirect control;
- mTLS opcional;
- signature/HMAC;
- timestamp/replay window;
- payload limits;
- header allowlist;
- secret rotation;
- callback idempotency.

## 10. Auditoría

Eventos obligatorios:

- CRUD/lifecycle de toda configuración;
- diff before/after;
- simulation;
- approval/signature/publication/activation/rollback;
- provider test y secret rotate;
- send/campaign;
- cancel/retry/resend/DLQ;
- read-sensitive/export;
- permission/role/SoD change;
- support/break-glass;
- pause/resume;
- retention/legal hold/redaction;
- callback replay/invalid signature.

Campos: tenant, actor/session, action, resource/version, timestamp UTC, outcome, reason, correlation, IP/user-agent seguros, before/after redacted e integrity hash.

AuditLog es append-only; el acceso también se audita.

## 11. Privacidad y retención

- minimización;
- purpose limitation;
- masking en listados;
- cifrado en tránsito/reposo;
- configurable retention dentro de mínimos regulatorios;
- legal hold;
- redaction preservando hash/metadata;
- exports expiran y pueden cifrarse;
- consent/suppression;
- data residency;
- DSAR mediante proceso gobernado.

No se afirma cumplimiento legal automático; cada cliente valida jurisdicción y intended use.

## 12. Seguridad de workers

- usuario/container no root;
- DB role mínimo;
- claims vía stored procedures o queries restringidas;
- ningún endpoint público de administración;
- graceful shutdown;
- resource limits;
- dependency/image scanning;
- signed images/SBOM;
- secrets mounted, no env cuando el runtime permita;
- outbound restrictions;
- no logging de bodies.

## 13. API security

- authn/authz por endpoint y recurso;
- request schemas strict;
- size/depth limits;
- idempotency;
- optimistic concurrency;
- rate limit;
- pagination caps;
- ProblemDetails sin internals;
- no GET con side effects;
- CORS allowlist;
- security headers;
- correlation IDs no controlan autorización.

## 14. Promoción y supply chain

- paquetes firmados con manifest/hash;
- secrets excluidos;
- dependencies verificadas;
- aprobaciones;
- diff source/target;
- drift detection;
- immutable audit;
- rollback target;
- CI con SAST, dependency, secrets, container e IaC scans.

## 15. Pruebas de seguridad

- tenant matrix/IDOR horizontal y vertical;
- permission matrix;
- SoD/self-approval;
- token/session revocation;
- MFA step-up;
- SQL/HTML/template/header/JSON injection;
- SSRF/DNS rebinding;
- callback signature/replay;
- secrets/logs/exports;
- mass assignment;
- rate-limit/resource exhaustion;
- race de approvals, cancel y retry;
- RLS bypass con roles DB;
- object storage path isolation;
- SignalR group isolation;
- break-glass expiry;
- audit tamper evidence;
- OWASP ASVS y threat model sign-off.

