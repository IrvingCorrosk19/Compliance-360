# Revisión de arquitectura

## Dictamen

Arquitectura: **78/100**. Es una solución modular monolítica razonable para su etapa, no una plataforma distribuida clase mundial.

## Evidencia y hallazgos

| Evidencia | Evaluación | Riesgo / impacto | Recomendación | Esfuerzo | Prioridad |
|---|---|---|---|---:|---|
| `src/Compliance360.Domain`, `.Application`, `.Infrastructure`, `.Web` | Dependencias por capas y dominio aislado | Bajo; buena base evolutiva | Mantener reglas de referencia en CI | 1 d | P2 |
| `Compliance360.Domain/RegulatoryAffairs/*` | Estados y comportamiento dentro del dominio | Positivo; evita modelo anémico parcial | Completar invariantes en agregados | 2–4 sem | P2 |
| `Compliance360.Web/Api/FoundationEndpoints.cs` | Gran concentración de endpoints | Alto coste de cambio y revisión | Separar por bounded context/feature | 2–3 sem | P1 |
| `Compliance360.Web/wwwroot/app.js` | SPA Vanilla JS centralizada | Alto acoplamiento UI, regresiones y test difícil | Modularizar por feature; TypeScript gradual | 6–10 sem | P1 |
| `RegulatoryAffairsService.cs` y `RegulatoryWorkflowV2Service.cs` | V1 y V2 coexisten | Bypass accidental y doble semántica | Retirar V1 con telemetría y migración explícita | 3–5 sem | P0 |
| Ausencia de `BackgroundService`/`IHostedService` | Notificaciones/reintentos no tienen worker durable demostrado | Trabajo asíncrono puede depender de request | Outbox + worker idempotente | 3–4 sem | P1 |
| `Compliance360DbContext.cs` | Unidad de persistencia central | Crecimiento aumenta tiempo de migración y coupling | Separar configuraciones y ownership por módulo | 3–6 sem | P2 |

## Patrones observados

- Dependency Injection e interfaces de repositorio/servicio.
- Result pattern para errores esperables.
- EF Core con PostgreSQL y migraciones versionadas.
- Middleware transversal para excepciones, observabilidad y autenticación.
- Optimistic concurrency mediante `Revision`/`ExpectedRevision`.
- Auditoría append-only reforzada en base de datos para eventos críticos.

## Escalabilidad

La capa web puede escalar horizontalmente si las sesiones permanecen persistidas y el almacenamiento documental es compartido. No existe evidencia de pruebas de carga, sizing, backpressure, colas durables, cache distribuida o objetivos p95/p99. El diseño es suficiente para carga departamental; no está demostrado para volumen multinacional.

## Acción prioritaria

Eliminar rutas V1 mutantes, modularizar el frontend y convertir notificaciones/tareas diferidas en procesamiento durable. Estos cambios reducen más riesgo que migrar prematuramente a microservicios.
