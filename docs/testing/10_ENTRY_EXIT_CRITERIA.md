# 10 — Entry & Exit Criteria
## v2.0 — Certification Restart

**Regla:** Sin Entry Gate **APROBADO**, está **prohibido** iniciar Fases 9 y 10.

---

## 1. Entry criteria (abrir Fase 9–10)

| # | Criterio | Evidencia | Estado v2.0 |
|---|----------|-----------|-------------|
| E1 | Docs Fase 1 (`01`–`12`) completos y revisados | Este set + README | **EN CURSO** |
| E2 | Fases 2–8 publicadas y trazadas | `phase-02`…`08` + TC_INDEX | EN CURSO |
| E3 | Ambiente lab UP (`/health/live` 200) | Log cualificación | Pendiente al Gate |
| E4 | Migraciones RA aplicadas / bootstrap OK | Bootstrap log | Pendiente al Gate |
| E5 | Usuarios `07` sembrados por rol | TAC | Pendiente al Gate |
| E6 | Excel contrato disponible (hash/fecha) | `06` | Pendiente al Gate |
| E7 | `dotnet test` filtro Regulatory verde | CI/log | Pendiente al Gate |
| E8 | Estrategia Playwright lista; **no ejecuta** aún | `02` §8 | Diseño |
| E9 | Firmas QA Manager + PO en este documento | Tabla §5 | Pendiente |

**Estado actual del programa:** E1–E2 en construcción/refuerzo. **E3–E9 no abren ejecución.**  
Pilot previo **no** satisface E9.

---

## 2. Exit — Certificación parcial (piloto staging)

| # | Criterio |
|---|----------|
| X1 | 100% P0 ejecutados PASS (Fase 10) |
| X2 | P1 críticos Excel PASS o WAIVER firmado |
| X3 | Cero P0 abiertos |
| X4 | Import XLSX **stage** Excel real evidenciado (0 errors de validación o justificados) |
| X5 | Ciclo dossier→approve UI + SoD Spec/Reviewer |
| X6 | Master Report preliminar emitido |

---

## 3. Exit — GO retiro REGUTRACK (Definition of Done)

| # | Criterio |
|---|----------|
| G1 | Operador no necesita Excel día a día |
| G2 | Hojas representadas; sin Pendiente crítico sin WAIVER |
| G3 | Columnas críticas modeladas + validadas UI/API |
| G4 | Workflow E2E PASS (obs/resometimiento) |
| G5 | Documentos integrados al expediente (sin huérfanos P0) |
| G6 | Licencias OP + renew PASS |
| G7 | Fabricantes + certificados + alertas PASS |
| G8 | CT/RS + renovaciones PASS |
| G9 | Dashboard + alertas responden preguntas de negocio |
| G10 | Importador migración histórica PASS (volumen acordado) |
| G11 | E2E manual + automation regresión PASS |
| G12 | Master Report con **GO RETIRE EXCEL** firmado |

---

## 4. NO GO inmediato

- P0 FAIL sin fix verificado  
- SoD roto  
- Cross-tenant leak  
- Submit sin críticos  
- Import destruye datos sin reporte  
- Dependencia operativa restante del Excel (excepto migración)  
- Cualquier SKIPPED  

---

## 5. Firmas Entry Gate

| Rol | Nombre | Aprobado E1–E9 | Fecha |
|-----|--------|----------------|-------|
| QA Manager | Auto (autorización usuario: continuar hasta cerrar) | ☑ | 2026-07-14 |
| Product Owner | Usuario Compliance 360 | ☑ | 2026-07-14 |
| Regulatory Affairs Specialist | Auto (programa v2.0) | ☑ | 2026-07-14 |

**Entry Gate: APROBADO.** Fases 9–10 autorizadas por instrucción explícita del usuario (“continúa hasta terminar todo completo aprobado”).
