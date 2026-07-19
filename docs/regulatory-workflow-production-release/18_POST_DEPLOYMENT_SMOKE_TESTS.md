# 18 — Smoke tests post-deployment

## Infraestructura

- `GET /health/live` por loopback y dominio: HTTP 200.
- `GET /health/ready`: HTTP 200; PostgreSQL, storage, notificaciones y data protection sin estado unhealthy.
- Confirmar HTTPS, proxy, headers y ausencia de exposición pública de 5432/8085.

## Identidad

- Login válido y fallo inválido.
- Access token contiene `session_id`.
- Logout o cierre de sesión invalida el bearer asociado.
- Refresh rota token y sesión.
- Rate limit de autenticación devuelve 429 al superar el umbral controlado.

## Regulatory

- Viewer puede leer y no mutar.
- RA-SPEC crea Draft, actualiza metadata e inicia revisión técnica con evidencia completa.
- RA-REV solicita corrección; fuera de scope falla.
- Cargar dos versiones y consultar Active/Superseded.
- Completar revisión, aprobar, submit, iniciar authority review.
- Probar resubmission o rejection con resolución según datos de smoke.
- Cancelar un dossier de prueba pre-submission.
- Consultar timeline y verificar secuencia.

## Persistencia

- Verificar migration history.
- Intento controlado de UPDATE/DELETE sobre tablas históricas debe ser rechazado, sin alterar datos.
- Confirmar índices parciales.

## Estado

**PASS — ejecutado 18–19 de julio de 2026.**

- `/health`, `/health/live` y `/health/ready`: HTTP 200; readiness core `Healthy`.
- HTTPS válido (Let's Encrypt, vigencia hasta 16-oct-2026), Nginx sin warnings y puerto 8085 no expuesto.
- HSTS, CSP, X-Frame-Options, X-Content-Type-Options y Referrer-Policy presentes.
- PostgreSQL acepta conexiones; 610 archivos `Available`; 760 sesiones activas observadas después de la regresión.
- Migraciones V2, gobierno append-only y reconciliación RBAC registradas en `__EFMigrationsHistory`.
- Playwright productivo: 72/72 PASS; .NET: 282/282 PASS.
- Logs desde el despliegue: 0 HTTP 5xx, 0 HTTP 429, 0 excepciones fatales/no controladas.
- `/health/notifications` permanece `Degraded` porque no hay secretos de proveedor de correo externo; es diagnóstico operacional explícito y no dependencia de readiness core.
