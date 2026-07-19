# 11 — Test Execution Plan
## v2.0 DISEÑO — ejecutable solo post–Entry Gate

## 1. Condición de inicio

Solo si `10_ENTRY_EXIT_CRITERIA.md` §5 está firmado.

## 2. Modalidad

1. **Humano:** navegador real, mouse/teclado, uploads.  
2. **API:** evidencias de contrato/RBAC en paralelo documentado.  
3. **Uno por uno** según prioridad `08`.  
4. **Stop on P0.**  
5. Tras bloque P0 manual PASS → activar Fase 9 Playwright como regresión.

## 3. Waves

| Wave | Contenido | Roles |
|------|-----------|-------|
| W0 | Qualificación E3–E7 | TAC Admin |
| W1 | Auth + seed users | Todos emails `07` |
| W2 | Bootstrap, packs, authorities | RA Admin |
| W3 | SoD deny matrix | Spec / Rev / View |
| W4 | Product + manufacturer + license smoke | Spec / Admin |
| W5 | Dossier lifecycle + submit gate + approve | Spec + Rev |
| W6 | Observation cycle | Spec |
| W7 | WF-NEG cartesian (por lotes de origen) | Admin |
| W8 | Dashboard + alerts data-driven | Admin |
| W9 | Import stage Excel real | Admin |
| W10 | Import commit (plan volumen + full book controlado) | Admin |
| W11 | Gaps Parcial/Pendiente | Admin + PO |
| W12 | NFR UX/RWD/F5/doble-clic | UX + Manual |
| W13 | Playwright regresión suite aprobada | Automation |
| W14 | Master Report + firmas GO/NO GO | QA Manager |

## 4. Registro por TC

Campos obligatorios (Fase 5 plantilla): Resultado obtenido, Evidencia path, Estado, Observaciones, Executor, Timestamp.

## 5. Bugs

Plantilla mínima en `bugs/BUG_xxx.md`: Descripción, Impacto, Pasos, Capturas/Logs, Causa raíz, Corrección, Retest.

## 6. Prohibiciones

- No “batch PASS” sin IDs.  
- No reutilizar pilot v1 como cierre v2.  
- No declarar GO con evidencia incompleta.
