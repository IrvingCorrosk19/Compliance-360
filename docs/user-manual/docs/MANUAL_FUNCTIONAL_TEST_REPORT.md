# MANUAL_FUNCTIONAL_TEST_REPORT

**Suite:** `e2e/tests/user-manual.spec.js`  
**Config:** `e2e/playwright.user-manual.config.js`  
**Target:** `docs/user-manual/index.html` (file://)  
**Date:** 2026-07-16  
**Result:** **10 / 10 PASS**

| # | Case | Result |
|---|------|--------|
| 1 | Abrir index y mostrar 9 roles | PASS |
| 2 | Cambiar tema claro/oscuro | PASS |
| 3 | Buscador (“expediente”) | PASS |
| 4 | Entrar a Specialist + completar tutorial | PASS |
| 5 | Marcador interactivo + Siguiente elemento | PASS |
| 6 | Simulador avanza | PASS |
| 7 | Progreso persiste tras reload (localStorage) | PASS |
| 8 | Glosario (26) + errores | PASS |
| 9 | Teclado Enter en marcador | PASS |
| 10 | Viewport mobile 390×844 | PASS |

## Cómo re-ejecutar

```bash
cd e2e
npx playwright test --config=playwright.user-manual.config.js
```

## Notas

- Pruebas offline: no requieren API ni `localhost:5272`.
- Sin errores JavaScript de página en el caso 1.
- Duplicado del simulador corregido antes del PASS final.
