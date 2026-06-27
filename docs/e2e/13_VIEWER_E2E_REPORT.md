# Viewer E2E Report

- Rol probado: Viewer
- Usuario utilizado: viewer@alimentos-premium.test
- Fecha: 2026-06-27T16:43:59.806923+00:00
- URL: http://localhost:5272
- Navegador: Chromium headed via Playwright Python
- Estado inicial: permisos de solo lectura asignados.
- Trace: artifacts/e2e/20260627T164338Z/viewer-trace.zip
- Veredicto: PASS

## Pasos Ejecutados

### 1. Login
- Estado: PASS
- Screenshot: artifacts/e2e/20260627T164338Z/viewer-login.png

### 2. Menu solo lectura
- Estado: PASS
- Screenshot: artifacts/e2e/20260627T164338Z/viewer-menu.png

### 3. Consultar reports
- Estado: PASS
- Screenshot: artifacts/e2e/20260627T164338Z/viewer-step-1-reports.png

### 4. Consultar capa
- Estado: PASS
- Screenshot: artifacts/e2e/20260627T164338Z/viewer-step-2-capa.png

### 5. Consultar risks
- Estado: PASS
- Screenshot: artifacts/e2e/20260627T164338Z/viewer-step-3-risks.png

### 6. Consultar indicators
- Estado: PASS
- Screenshot: artifacts/e2e/20260627T164338Z/viewer-step-4-indicators.png

### 7. Consultar audit-trail
- Estado: PASS
- Screenshot: artifacts/e2e/20260627T164338Z/viewer-step-5-audit-trail.png

### 8. Logout
- Estado: PASS
- Screenshot: artifacts/e2e/20260627T164338Z/viewer-logout.png

## Console Logs

- [verbose] [DOM] Input elements should have autocomplete attributes (suggested: "username"): (More info: https://goo.gl/9p2vKq) %o
- [verbose] [DOM] Input elements should have autocomplete attributes (suggested: "username"): (More info: https://goo.gl/9p2vKq) %o


## Network Logs >= 400

- Sin errores HTTP inesperados.

## Estado Final

Viewer solo lectura validado sin formularios de creacion, configuracion ni errores HTTP/consola.