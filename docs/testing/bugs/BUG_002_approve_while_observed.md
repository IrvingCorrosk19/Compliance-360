# BUG_002 — Approve blocked while dossier Observed (by design)

| Campo | Valor |
|-------|--------|
| Severidad | P1 (spec incorrecta / UX) |
| Estado | Closed — test aligned to domain rule |

## Descripción
`ApproveDossierAsync` retorna 400 si status es Observed/CorrectingObservation: debe cerrar observación y resometer.

## Impacto
SCN-06 Playwright falló al abrir observación antes de approve.

## Causa raíz
Orden de escenario incorrecto vs regla de negocio válida (protege integridad).

## Corrección
Separar caminos: approve feliz sin obs abierta; ciclo observación en segundo dossier.
