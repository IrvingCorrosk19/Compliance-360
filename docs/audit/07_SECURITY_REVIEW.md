# Revisión de seguridad

## Resultado

Seguridad: **62/100 — controles de aplicación relevantes, endurecimiento operativo insuficiente**.

## OWASP y controles

| Control | Evidencia | Estado / riesgo | Acción | Prioridad |
|---|---|---|---|---|
| Autenticación | JWT, refresh/session revocation, `session_id` | Buena base; identidad admin productiva sin rol | Reconciliación IAM y break-glass controlado | P0 |
| Token/logout | `wwwroot/app.js:832-854,907-924` | Bearer en `localStorage`; logout UI no llama revocación servidor | BFF/cookie HttpOnly o token en memoria; logout remoto | P0 |
| Autorización | `PermissionPolicies`, checks de servicio, SoD | Granular; riesgo de drift UI/policy/migración | Matriz executable y deny-by-default | P0 |
| IDOR/multitenancy | TenantId y ownership de stored files | Mejorado; sin RLS | Tests de IDOR exhaustivos + DB defense-in-depth | P0 |
| SQL Injection | EF Core/consultas parametrizadas | Sin evidencia de concatenación vulnerable en rutas auditadas | SAST continuo | P1 |
| XSS | helper `safe()` en SPA | HTML dinámico extenso eleva riesgo de omisión | CSP estricta + DOM sinks lint + tests | P0 |
| CSRF | Bearer JWT reduce cookie-CSRF | Cookies/session/header deben verificarse por flujo | Documentar modelo y SameSite/anti-forgery donde aplique | P1 |
| Upload | firma/MIME/tamaño/hash/ownership/Available | Control fuerte | AV/CDR, cuarentena y object storage privado | P1 |
| Rate limit | Sesión para API; IP+endpoint para auth | 429 previos corregidos | Prueba de carga y límites por riesgo | P1 |
| Auditoría | Append-only DB en eventos críticos | Fuerte; retención/export no formalizados | WORM externo, firma y alertas de tamper | P1 |
| Secretos | Credenciales detectadas en docs/scripts/dev/testdata | Exposición y reutilización | Rotar, secret manager y escaneo histórico | P0 |
| Transporte | HTTPS disponible y HTTP público `:8085` | Credenciales/tokens pueden viajar sin TLS | Cerrar HTTP o redirigir obligatoriamente | P0 |
| Contenedores | Ejecutan como root, sin read-only/limits | Escalada e impacto DoS | Non-root, capabilities drop, limits, read-only | P0 |

## Evidencia adicional

- Dependencias .NET y npm no reportaron vulnerabilidades conocidas.
- Forwarded headers están limitados a proxies/red Docker conocidos.
- El puerto de aplicación se enlaza a loopback detrás de Nginx.
- No existe evidencia completa de DAST autenticado, pentest independiente, SBOM firmado o provenance de imágenes.

## Riesgos críticos

1. **HTTP público 8085:** contradice una certificación de transporte seguro.
2. **Secretos históricos/hardcoded:** deben rotarse; ignorar el archivo no elimina su historial.
3. **Aislamiento tenant solo en aplicación:** una consulta defectuosa puede superar el control.
4. **Runtime root:** aumenta el blast radius de una vulnerabilidad web.
5. **Sink XSS concreto:** `wwwroot/form-template-builder.js:1193-1197` inserta un mensaje de excepción mediante `innerHTML` sin escape.
6. **Upload sin antimalware:** `FoundationEndpoints.cs:542-580` marca el archivo como `Available` inmediatamente; la validación ZIP/OOXML no sustituye AV/CDR ni limita todo payload embebido.
7. **Enumeración de identidad:** `/auth/identify` revela membresías/metadatos de organizaciones antes de autenticar (`IdentityService.cs:58-102`).

## Cumplimiento

La solución tiene controles compatibles con un sistema regulado, pero el código por sí solo no demuestra 21 CFR Part 11, Annex 11, ISO 27001 o GxP. Faltan validación formal, control de cambios, evidencia de operación, revisión periódica de acceso y pruebas de recuperación.
