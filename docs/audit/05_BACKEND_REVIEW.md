# Revisión de backend

## Resultado

Backend: **74/100**. La base .NET 9 es sólida, tipada y testeable; los principales riesgos son tamaño de servicios/endpoints, coexistencia de workflows y baja cobertura de infraestructura.

## Fortalezas verificadas

- Minimal APIs con autorización por políticas en `Compliance360.Web/Api`.
- Servicios de aplicación separados del dominio y de EF Core.
- `GlobalExceptionMiddleware` entrega semántica HTTP explícita para 400/403/409/499.
- JWT incorpora `session_id` y producción rechaza tokens sin sesión.
- Uploads validan tamaño, tipo, firma y disponibilidad; archivos usan SHA-256.
- Readiness, liveness y salud de notificaciones están separadas.
- `CancellationToken` y operaciones async se usan ampliamente.

## Hallazgos

| Archivo/carpeta | Evidencia | Riesgo e impacto | Recomendación | Esfuerzo | Prioridad |
|---|---|---|---|---:|---|
| `Web/Api/FoundationEndpoints.cs` | Múltiples dominios en un archivo extenso | Revisiones y ownership difíciles | Endpoint modules por feature | 2–3 sem | P1 |
| `Application/RegulatoryAffairs/RegulatoryAffairsService.cs` | Workflow V1 aún existe | Bypass o divergencia de invariantes | Deprecar y eliminar mutaciones V1 | 3–5 sem | P0 |
| `Application/RegulatoryAffairs/RegulatoryWorkflowV2Service.cs` | Servicio orquesta numerosas capacidades | Complejidad ciclomática y pruebas costosas | Handlers por command/capability | 3–4 sem | P1 |
| `Infrastructure/*` | Cobertura observada ~1.67% | Fallos reales de DB/storage no detectados | Testcontainers PostgreSQL y contract tests | 4–6 sem | P0 |
| `tests/Compliance360.Tests` | 18 fixtures usan EF InMemory; 0 Testcontainers/UseNpgsql | No reproduce FK, jsonb, índices parciales, triggers ni locking | Suite relacional PostgreSQL 18 | 1–2 sem | P0 |
| `Web/wwwroot/*.js` + contratos API | Contratos duplicados manualmente | Drift frontend/backend | OpenAPI y cliente generado/validado | 2–4 sem | P1 |
| Notificaciones | Retry/dead-letter existe, worker durable no demostrado | Entrega tardía o perdida | Outbox, worker, métricas y replay | 3–4 sem | P1 |
| `NotificationService.cs:97-144` | Proveedor externo antes de persistir `Sent` | Dos ejecutores pueden enviar duplicado | Claim atómico Queued→Processing + idempotency key | 1–2 sem | P1 |

## Calidad

`dotnet build -c Release` finaliza sin errores y 282 pruebas pasan. Sin embargo, `dotnet format --verify-no-changes` reporta whitespace en `RegulatoryAffairsService.cs`, `FoundationEndpoints.cs`, `ObservabilityMiddleware.cs` y `Program.cs`; por tanto el gate de estilo no está limpio.

## Performance

EF Core usa índices y consultas async, y PostgreSQL no reportó índices inválidos. Faltan pruebas reproducibles de carga, perfiles de consultas N+1, límites por endpoint y métricas de latencia p95/p99. La calificación no asume rendimiento que no fue medido.

Varias lecturas materializan colecciones completas o usan topes fijos (`EfNotificationRepository.cs`, `EfRegulatoryWorkflowV2Repository.cs`, `EfSuperAdminPlatformRepository.cs`; Regulatory Affairs usa `Take(500)`). Deben adoptar paginación keyset, proyecciones y límites explícitos.
