# Compliance 360 — Manuales de Prueba Funcional Manual

Programa oficial para **aprender, ejecutar y validar** Compliance 360 por rol desde el navegador.

## Inicio rápido

1. Lea [`00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md`](00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md)
2. Prepare entorno y ejecute [`scripts/e2e_provision.ps1`](../../scripts/e2e_provision.ps1)
3. Siga manuales **01 → 17** en orden
4. Registre en [`18_FUNCTIONAL_TEST_RESULTS_TEMPLATE.md`](18_FUNCTIONAL_TEST_RESULTS_TEMPLATE.md)

## Documentos (22)

| # | Archivo |
|---|---------|
| 00 | Roadmap maestro |
| 01–16 | Manuales por rol |
| 17 | Procesos E2E transversales |
| 18–20 | Plantillas y checklist |
| 21 | Reporte calidad documentación |

## Automatización complementaria

| Herramienta | Uso |
|-------------|-----|
| `e2e/tests/functional.spec.ts` | 15 flujos Playwright por rol |
| `e2e/tests/roles.spec.ts` | 14 pruebas RBAC navegación |
| `scripts/customer_journey.ps1` | 23 pasos API ciclo negocio completo |
| `dotnet test tests/Compliance360.Tests` | Unit/integration |

## Relación con certificación

El programa de certificación enterprise está en [`../functional-certification/`](../functional-certification/FUNCTIONAL_CERTIFICATION_MASTER_PLAN.md). Los manuales de esta carpeta son la **guía operativa paso a paso**; la certificación usa los mismos roles y flujos con evidencia formal.

## Evidencias

```
artifacts/manual-functional-testing/{rol}/TC-XXX/
```
