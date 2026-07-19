# 08 — Seguridad

La especificación de RBAC, SoD, tenant isolation, secretos, auditoría, privacidad, API y pruebas de seguridad está en [08_SECURITY_AND_RBAC.md](./08_SECURITY_AND_RBAC.md).

## Decisión

Alert Center aplicará deny-by-default, autorización server-side por recurso, query filters, FK compuestas, PostgreSQL RLS, maker-checker, secretos write-only, MFA step-up, auditoría append-only y acceso de soporte JIT.

## Controles mínimos

- Catálogo granular `ALERT.<RESOURCE>.<ACTION>`.
- Sin autorización basada únicamente en nombres de rol.
- No self-approval para artefactos regulados.
- Provider Admin separado de Secret Admin.
- Templates con escaping/sanitización contextual.
- Providers/webhooks protegidos contra SSRF y replay.
- PII enmascarada y retención/legal hold.
- Workers no-root con DB role mínimo.
- Security, secrets, dependency y container scans en CI.
- Matriz automatizada de IDOR cross-tenant.

El cumplimiento regulatorio requiere validación del intended use y jurisdicción del cliente; la plataforma provee controles y evidencia, no certificación legal automática.

