# BUG_001 — Playwright login / testdata Tenant Admin invalid

| Campo | Valor |
|-------|--------|
| Severidad | P0 (bloquea automatización UI) |
| Detectado | 2026-07-14 Fase 9/10 |
| Estado | **Cerrado** — re-run Playwright 3/3 PASS (2026-07-14 run4) |

## Descripción
Los specs `regulatory-affairs.spec.ts` fallan en `helpers.login` esperando `#password`. Screenshot muestra login enterprise aún en paso email ("Siguiente"). Además API login para `tenantadmin@alimentos-premium.test` / tenant `ddcaf211-...` retorna **400** “Correo o contraseña incorrectos”.

## Impacto
No se puede certificar RA vía Playwright con `e2e/testdata.json` actual.

## Pasos
1. `npx playwright test tests/regulatory-affairs.spec.ts`
2. Timeout 15s en `#password`
3. API probe confirma credenciales alimentos inválidas; Irving OK

## Causa raíz
1. Dataset E2E (alimentos-premium) desalineado con DB actual.
2. Flujo login email→org→password sensible a fallo de identificación de email.

## Corrección
1. Specs RA usan tenant/usuario Irving validado por API.
2. Login helper: tras identify, aceptar **org radios O** paso password directo (single-org tenant).
3. Re-run requerido para cerrar bug.

## Evidencia
- `artifacts/e2e/test-output/.../test-failed-1.png`
- `docs/testing/evidence/playwright-ra-run.txt`
