# 16 — Ejecución E2E regulatoria

Fecha de ejecución confirmada: 2026-07-18.

| Capa | Suite | Resultado confirmado | Evidencia |
|---|---|---|---|
| Playwright | colección completa `e2e/tests/**` | **71/71 PASS; 0 FAIL; 0 SKIP** | ejecución final `npx playwright test --reporter=line`, exit code 0 |
| Playwright | manual roles | 10/10 PASS | `manual-vs-system-role-matrix.json`; capturas de rol |
| Playwright | workflow | 10/10 PASS | `manual-workflow-steps.json`, `failed=0` |
| Playwright | SoD | 10/10 PASS | `browser-sod-steps.json`, `failed=0` |
| Playwright | role-e2e | 10/10 PASS | `journey-ra-*.json`; `playwright-run.log` |
| Playwright | ra-admin | 10/10 PASS | ejecución confirmada de la suite RA Admin |
| Playwright | TAC/QM | 2/2 PASS | capturas TAC/QM y matriz de roles |
| Playwright | responsive | 10/10 PASS | ejecución confirmada |
| Playwright | i18n | 10/10 PASS | ejecución confirmada |
| Playwright | regulatory-affairs | 3/3 PASS | ejecución confirmada |
| .NET | pruebas | 252/252 PASS | ejecución confirmada |
| .NET | build | PASS | 0 errores, 0 advertencias |

Además, `artifacts/e2e` contiene 20 archivos `functional-summary.json` y 20 archivos `functional-final.png`; se indexan en `18_BROWSER_EVIDENCE_INDEX.md`.
