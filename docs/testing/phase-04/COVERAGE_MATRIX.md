# Fase 4 — Matriz de cobertura funcional
## Certification v2.0 · DISEÑO · sin ejecución

**Inventario:** [`../phase-05/TC_INDEX.csv`](../phase-05/TC_INDEX.csv) (**571+** casos Designed).  
Ninguna funcionalidad RA as-built queda sin familia de TC. Gaps de producto tienen TC-GAP-* (PASS/FAIL/WAIVER).

## 1. Cobertura por módulo

| Módulo | Elementos | Casos (orden) |
|--------|-----------|---------------|
| AUTH | Login/logout/sesión | 6+ |
| TAC / RBAC | Users seed + SoD (+ BUG_003) | 15+ |
| RA-CONFIG | Bootstrap, authorities, packs, Studio gap | 5+ |
| RA-PRODUCT | CRUD/dup/Excel fields | 10+ |
| RA-WORKFLOW | Allowed transitions + waiver ≥8 | 30+ |
| RA-WORKFLOW-NEG | Illegal pairs | ~215 |
| RA-CHECKLIST / REQ | 22 + submit gate | 50+ |
| RA-OBS / APPROVE | Ciclo obs + approve SoD | 15+ |
| RA-MFR / LIC | Fabricantes, certs, licencias | 15+ |
| RA-IMPORT | Stage/commit/maxRows/full/rollback/dup | 12+ |
| RA-ALERT / DASH | Thresholds + KPIs | 35+ |
| API | Cada endpoint + 401 | 60+ |
| RA-UI-CTRL / NAV | Controles hash views | 90+ |
| NFR / SEC | F5, IDOR, rate limit | 15+ |
| GAP | Documents, Studio, kanban columns | 3+ |
| LEGACY | Anti-Workspace-as-dossier | 1+ |

## 2. Criterio “sin huecos”

Checklist de cierre Fase 4:

- [x] 10 vistas UI listadas con TC  
- [x] 31 rutas API con familia positiva/401  
- [x] 16 estados × transiciones allowed/illegal  
- [x] 4 roles RA + TAC + QM parcial  
- [x] 4 hojas Excel  
- [x] Gaps implementación con TC-GAP  

## 3. Notas

UI usa `prompt()` en varios flujos → TC-UX obligatorios (no omitir por “feo”).
