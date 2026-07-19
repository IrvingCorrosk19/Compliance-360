# Fase 5 — Catálogo de casos funcionales
## Certification v2.0 · DISEÑO · **NO EJECUTAR**

## Inventario

| Archivo | Contenido |
|---------|-----------|
| [`TC_INDEX.csv`](./TC_INDEX.csv) | **571** casos (Status=`Designed`) |
| [`TC_P0_DETAILED.md`](./TC_P0_DETAILED.md) | Plantillas P0 con pasos |
| Este README | Convención |

## Convención de ficha

ID · Nombre · Objetivo · Precondiciones · Rol · Datos · Pasos · Resultado esperado · Resultado obtenido *(vacío)* · Evidencia *(vacío)* · Severidad · Prioridad · Estado=`Designed` · Observaciones

## Prioridades

- **P0:** retiro Excel / SoD / state machine / import integridad / submit gate / IDOR  
- **P1:** alertas, dashboard, gaps Documents/Studio/rollback, commit full-book  
- **P2:** UX polish, columnas kanban opcionales  

## Reglas v2.0

1. Ningún TC pasa a `PASS/FAIL` hasta Entry Gate firmado (`10`).  
2. Pilot previo ≠ Status del índice.  
3. Casos añadidos v2.0: waiver≥8, BUG_003 SoD, maxRows, full commit, rollback, catalog uniquify, IDOR dossier, TC-GAP-*.  
4. AutomationCandidate=`Yes` solo tras aprobación para Fase 9.

## Expansión a ficha

Tomar fila CSV → completar pasos con Fase 2 → en Fase 10 llenar resultado + evidencia.
