# 09 — Test Evidence

> **Estado: CERTIFIED — ALL EXECUTED LOCAL GATES PASS.**

## Resultados consolidados

- Build Release: **PASS — 0 warnings, 0 errors**.
- Suite .NET: **PASS — 262/262**.
- Playwright chromium completo: **PASS — 72/72**, duración **13.4 min**.
- Nuevo spec `e2e/tests/regulatory-workflow-v2.spec.ts`: **PASS**.

## Escenario Workflow V2

`evidence/workflow-v2-final.json` registra `passed: true` y 14 fases exitosas:

1. Creación UI de producto y dossier `Draft`.
2. Persistencia y edición de metadata con revisión.
3. Revisión obsoleta rechazada con HTTP `409`.
4. Avance a `UnderTechnicalReview`.
5. Solicitud de corrección con scope.
6. Rechazo sin persistencia de evidencia fuera de scope.
7. Carga real de evidencia V1/V2 con SHA-256.
8. Envío de corrección y finalización por Reviewer.
9. Viewer denegado en toda familia mutable V2 probada.
10. TAC y RA-ADM denegados en review, approval y override.
11. Timeline append-only, secuencial y completo.
12. Reopen con solicitante segregado y dos aprobadores distintos.
13. Override con solicitante segregado y dos aprobadores distintos.
14. Soft archive con timeline completo preservado.

## Archivos de evidencia

- `evidence/workflow-v2-final.json`: veredicto y fases.
- `evidence/workflow-v2-execution.json`: bitácora de ejecución.
- `evidence/stale-revision-response.json`: respuesta HTTP `409` y detalle de revisión.
- `evidence/evidence-version-history.json`: V1 superseded y V2 activa con hashes SHA-256.
- `evidence/timeline.json`: eventos secuenciales del dossier principal.
- `evidence/archived-timeline.json`: conservación después del archivo.
- `evidence/viewer-v2-negative-results.json`: seis mutaciones con `403`.
- `evidence/admin-v2-negative-results.json`: TAC/RA-ADM con `403`.
- `evidence/01-ra-spec-created-draft.png`.
- `evidence/02-ra-spec-under-technical-review.png`.
- `evidence/03-ra-reviewer-correction-requested.png`.
- `evidence/04-ra-reviewer-completed-review.png`.

## Trazabilidad técnica

La evidencia de concurrencia demuestra que `Expected 1, current 2` produce `409` y no persiste el texto de prueba. La evidencia versionada conserva dos hashes diferentes, versión 1 `Superseded` y versión 2 `Active`. El timeline contiene secuencias consecutivas con actor, razón y correlation ID.

## Interpretación

Los resultados certifican compilación, regresión .NET, regresión browser chromium y el nuevo recorrido multirol en el ambiente local. No constituyen evidencia de que el release ya haya sido desplegado o probado en infraestructura remota.
