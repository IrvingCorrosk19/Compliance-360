# 19 — Riesgos abiertos y dependencias externas

## Riesgos bloqueantes

Ninguno abierto para este release.

## Riesgos técnicos

- En producción, tokens sin `session_id` son rechazados; desarrollo conserva compatibilidad controlada.
- Rate limiting depende del proxy confiable configurado; Nginx y forwarded headers fueron verificados.
- Notificaciones V2 son best-effort: una falla de transporte no revierte la transición.
- Validación de firma por magic bytes reduce spoofing, pero no reemplaza antimalware ni análisis profundo de contenedores Office.
- Índices parciales pueden fallar al aplicarse si ya existen múltiples filas activas incompatibles; requiere preflight de datos.
- Triggers append-only cambian operaciones administrativas; retención/corrección debe diseñarse sin UPDATE/DELETE.

## Dependencias externas

PostgreSQL, volumen de storage, Data Protection keys, reverse proxy/TLS, proveedor de notificaciones y configuración secreta JWT.

## Dependencia operacional no bloqueante

No hay credenciales configuradas para SMTP, SendGrid, Mailgun o Resend. `/health/notifications` lo muestra como `Degraded`; `/health/ready` permanece `Healthy`. Las notificaciones internas/cola funcionan, pero el correo externo requiere secretos suministrados por operación.

## Tratamiento

Monitorear cola/dead letters, renovar TLS antes del 16-oct-2026 y configurar/probar un proveedor externo antes de habilitar entrega de correo.
