# Compliance 360 — Enterprise Functional Certification Program

| Campo | Valor |
|-------|--------|
| Producto | Compliance 360 · Business Capability **Regulatory Affairs** |
| Contrato de negocio | `REGUTRACK 02JUN26 MG.xlsx` |
| Matriz de cobertura negocio | [`../regulatory-affairs/REGULATORY_COVERAGE_MATRIX.md`](../regulatory-affairs/REGULATORY_COVERAGE_MATRIX.md) |
| Nivel | Big Four · FDA evidence mindset · ISO 13485 QMS · Banking control · QA Enterprise |
| Regla activa | **DISEÑO PRIMERO — SIN EJECUCIÓN DE PRUEBAS** |
| Versión programa | **2.0 — Certification Restart** |
| Fecha | 2026-07-14 |

---

## PRIMERA REGLA (obligatoria)

1. **No se ejecuta ninguna prueba** (manual, API, Playwright, scripts) hasta que las Fases **1–8** de documentación estén cerradas y el Entry Gate (`10_ENTRY_EXIT_CRITERIA.md`) esté en **APROBADO**.
2. **No se escribe automatización Playwright** hasta que los casos de Fase 5 estén aprobados (Fase 9).
3. La certificación **no** se declara GO por “parece funcionar”, por lectura de código, ni por un único run de Playwright.
4. Cualquier oleada previa (smoke / pilot / `run-full-cert.ps1` / Playwright 3/3) es **evidencia de descubrimiento**, **no** evidencia de certificación enterprise, salvo que se re-ejecute bajo este plan tras el Entry Gate.

---

## Índice Fase 1 — Master Test Documentation

| # | Documento | Propósito |
|---|-----------|-----------|
| 01 | [MASTER_TEST_PLAN](./01_MASTER_TEST_PLAN.md) | Propósito, alcance, organización, fases, gobernanza |
| 02 | [TEST_STRATEGY](./02_TEST_STRATEGY.md) | Pirámide, técnicas, evidencia ALCOA+, SoD de prueba |
| 03 | [FUNCTIONAL_SCOPE](./03_FUNCTIONAL_SCOPE.md) | In / Out — módulos y pantallas del sistema real |
| 04 | [NON_FUNCTIONAL_SCOPE](./04_NON_FUNCTIONAL_SCOPE.md) | Seguridad, UX, rendimiento, responsive, integridad |
| 05 | [TEST_ENVIRONMENT](./05_TEST_ENVIRONMENT.md) | Ambiente cualificado, build, DB, secrets de prueba |
| 06 | [TEST_DATA](./06_TEST_DATA.md) | Excel contrato, seeds, datasets por escenario |
| 07 | [TEST_USERS](./07_TEST_USERS.md) | Usuarios y `RoleCatalog` real |
| 08 | [RISK_BASED_TESTING](./08_RISK_BASED_TESTING.md) | Riesgos, Prioridad P0–P3, stop-on-P0 |
| 09 | [TRACEABILITY_MATRIX](./09_TRACEABILITY_MATRIX.md) | Excel ↔ Entidad ↔ UI ↔ API ↔ TC |
| 10 | [ENTRY_EXIT_CRITERIA](./10_ENTRY_EXIT_CRITERIA.md) | Entry Gate / Exit parcial / GO retiro Excel |
| 11 | [TEST_EXECUTION_PLAN](./11_TEST_EXECUTION_PLAN.md) | Orden humano de ejecución (post-Gate) |
| 12 | [PRODUCTION_CERTIFICATION](./12_PRODUCTION_CERTIFICATION.md) | Marco post-staging (no sustituye lab cert) |

---

## Fases 2–8 — Catálogos de diseño (sin ejecución)

| Fase | Entrega |
|------|---------|
| 2 | [phase-02/SYSTEM_ANALYSIS.md](./phase-02/SYSTEM_ANALYSIS.md) |
| 3 | [phase-03/ROLE_MATRICES.md](./phase-03/ROLE_MATRICES.md) |
| 4 | [phase-04/COVERAGE_MATRIX.md](./phase-04/COVERAGE_MATRIX.md) |
| 5 | [phase-05/](./phase-05/) — índice de casos (`TC_INDEX.csv`) + P0 detallados |
| 6 | [phase-06/SCENARIOS.md](./phase-06/SCENARIOS.md) |
| 7 | [phase-07/NEGATIVE_TESTS.md](./phase-07/NEGATIVE_TESTS.md) |
| 8 | [phase-08/ROLE_CERTIFICATION_SCRIPTS.md](./phase-08/ROLE_CERTIFICATION_SCRIPTS.md) |

---

## Fases 9–10 — Bloqueadas

| Fase | Estado |
|------|--------|
| 9 Playwright | **BLOQUEADA** hasta aprobación Fase 5 |
| 10 Ejecución uno-a-uno + `FUNCTIONAL_CERTIFICATION_MASTER_REPORT.md` | **BLOQUEADA** hasta Entry Gate |

Carpeta `evidence/` y `bugs/` pueden contener artefactos de **pilot previo**; no cuentan para GO v2.0 hasta revalidación.

---

## Definición de éxito de negocio

Compliance 360 puede **reemplazar REGUTRACK** para operación diaria cuando el operador **no necesita Excel** excepto como fuente de **migración histórica** vía importador, con evidencia firmada bajo criterios G1–G12 en `10_ENTRY_EXIT_CRITERIA.md`.
