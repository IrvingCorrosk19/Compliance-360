# Validación de contenido — Manual Interactivo

**Fecha:** 2026-07-10  
**Generador:** `scripts/generate_manual_interactivo.py`  
**Salida:** `COMPLIANCE360_MANUAL_INTERACTIVO_POR_ROLES_Y_FLUJOS.html`

## Resumen

| Elemento | Cantidad | Fuente |
|----------|----------|--------|
| Roles | 17 | `RoleCatalog.cs` |
| Casos de prueba | 88 | Manuales MFT 01–16 |
| Secuencia maestra | 22 pasos | Roadmap MFT §9 + análisis 00 |
| Flujos transversales | 8 | Manual 17 E2E-001..008 |
| Validaciones negativas | 8 | MFT + app.js |
| Third-party PENDING | 5 | Roadmap §7 |

## Verificación cruzada

| Requisito | Estado | Evidencia |
|-----------|--------|-----------|
| Login v2 Siguiente → Continuar → Iniciar sesion | OK | `app.js` L440–464 |
| Crear registro real | OK | `app.js` L2578 |
| Modo solo lectura | OK | `app.js` L2588 |
| Salir logout | OK | `app.js` L567 |
| 19 rutas navegación | OK | `app.js` navigation L218–246 |
| Sin CDN / deps externas | OK | HTML autocontenido |
| localStorage c360.manual.interactivo.v1 | OK | JS embebido |
| Sin contraseñas en HTML | OK | MANUAL_DATA emails only |
| QM approve vía API documentado | OK | ui_gaps + TC-QM-DOC-003 |
| Support sin break-glass UI | OK | TC-SO-002, ui_gaps |
| TECHNICALSHEET.CREATE RBAC | OK | TC-TA-004, CF-03 |
| SMTP PENDING | OK | third_party_pending TP-SMTP |

## Roles sin usuario E2E

- Platform Operations — derivado de catálogo; asignar rol manualmente en tenant plataforma.
- Platform Security — idem.

## Prueba visual funcional E2E (2026-07-10)

**Entorno:** `http://localhost:8801/COMPLIANCE360_MANUAL_INTERACTIVO_POR_ROLES_Y_FLUJOS.html` (Python `http.server` en `docs/manual-interactivo/`).  
**App de referencia:** `http://localhost:5272` (levantada con `dotnet run` + `e2e_provision.ps1`).

### Bug corregido durante la prueba

| Problema | Causa | Fix |
|----------|-------|-----|
| Al navegar (p. ej. Secuencia 22 pasos), el sidebar marcaba activo pero el contenido seguía en Intro | `renderMain()` reemplazaba todo `#main.innerHTML`, destruyendo `#progressPct` y provocando error en la segunda navegación | Contenedor `#main-content`; solo se reemplaza su innerHTML |

### Resultados

| Área | Resultado | Notas |
|------|-----------|-------|
| Navegación 23 secciones (Intro + 17 roles + 5 transversales) | **PASS** | Cada sección muestra h2 y contenido correcto; barra de progreso persiste |
| Modo Aprendizaje | **PASS** | Contexto completo visible |
| Modo Ejecución | **PASS** | Controles `.test-only` ocultos |
| Modo Prueba | **PASS** | 20 botones PASS/FAIL/PENDING por rol; stats actualizan (1 PASS → 2%, 1 FAIL) |
| Modo Presentación | **PASS** | `body.present-mode`; sidebar/toolbar ocultos |
| Búsqueda Enter (`CAPA`) | **PASS** | Navega a TC-CM-001 en CAPA Manager |
| Tema claro/oscuro | **PASS** | `data-theme="dark"` alterna correctamente |
| Teclado ← → en pasos de caso | **PASS** | Paso 1 ↔ Paso 2 en TC con múltiples steps |
| Exportar JSON | **PASS** | `exportJson()` genera blob sin error |
| Reset progreso | **PASS** | Confirm → stats 0% / 88 NOT_EXECUTED |
| localStorage `c360.manual.interactivo.v1` | **PASS** | Persiste `results`, `lastSection`, `mode` |

**Veredicto visual funcional:** **PASS** (manual usable de inicio a fin tras fix de navegación).


Tras cambios en `app.js` o manuales MFT, actualice `MANUAL_DATA` en el script Python y ejecute:

```powershell
python scripts/generate_manual_interactivo.py
```
