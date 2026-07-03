# 03 — Security Certification — Compliance 360

Date: 2026-07-03 · Basis: OWASP Top 10 + live probes.

## Response security headers (live, verified via curl)

```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Referrer-Policy: no-referrer
Permissions-Policy: camera=(), microphone=(), geolocation=()
Content-Security-Policy: default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:;
  connect-src 'self'; frame-ancestors 'none'; object-src 'none'; base-uri 'self'; form-action 'self'
Strict-Transport-Security: max-age=31536000; includeSubDomains   (on HTTPS)
Cache-Control: no-store, no-cache, max-age=0   (on /api and /auth)
```

## Authentication & session

- JWT with **full validation**: issuer, audience, lifetime, signing key (`Program.cs`).
- Access token 15 min; refresh token 30 days (rotating).
- Signing key & connection string are **empty in `appsettings.json`** → supplied via user secrets /
  environment (no secrets in source). Startup **fails fast** if `Jwt:SigningKey` is missing.
- Password policy: min length **12**, upper+lower+digit+symbol, history **5**.
- Lockout: **5** failed attempts → **15 min** lock.
- MFA supported (`/auth/mfa/complete`, challenge flow).

## Access control (OWASP A01)

- Broken Access Control: **not present** — every business endpoint enforces a granular permission
  policy; anonymous → 401; wrong-permission → 403 (verified).
- Privilege escalation: SuperAdmin functional bypass removed; cross-tenant access requires auditable
  `PLATFORM.SUPPORT.ACCESS` break-glass.
- IDOR / tenant leakage: cross-tenant read of documents and users returned **403** (see §05).

## Injection / XSS / CSRF

- Injection (A03): EF Core parameterized queries throughout; no string-concatenated SQL in app code.
- XSS: strict CSP (`script-src 'self'`, no inline), `X-Content-Type-Options: nosniff`.
- CSRF: token-based auth via `Authorization` header (not cookies) → CSRF surface minimal; SPA is
  same-origin with restrictive CORS (`AllowedOrigins` allow-list, empty by default = same-origin only).

## Rate limiting & transport

- Fixed-window limiter: **120 req/min per IP**, 429 on exceed.
- HTTPS redirect enabled; HSTS in non-development.

## Input robustness (fixed this cycle)

- Malformed JSON body now returns **400** (was 500), verified live.

## Residual items

- `AllowedHosts: "*"` and `Cors:AllowedOrigins: []` must be set to concrete production values at deploy
  time (documented in Go-Live checklist). Not a code defect; deployment configuration.

## Verdict

No critical vulnerabilities. **Security PASS** (with deploy-time host/CORS/secret configuration).
