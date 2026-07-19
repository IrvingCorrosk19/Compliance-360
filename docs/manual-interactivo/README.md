# Manual Interactivo — Compliance 360

Manual HTML **autocontenido** para aprendizaje, ejecución y pruebas funcionales por rol.

## Archivos

| Archivo | Descripción |
|---------|-------------|
| `COMPLIANCE360_MANUAL_INTERACTIVO_POR_ROLES_Y_FLUJOS.html` | Manual interactivo por roles (88 casos, modos Aprendizaje/Prueba) |
| `COMPLIANCE360_GUIA_ADMIN_PASO_A_PASO.html` | **Guía visual admin**: login v2 → crear tenant → TAC → activar (18 pasos, pantallas = app real) |
| `COMPLIANCE360_GUIA_VISUAL_TODOS_LOS_ROLES.html` | **Guía visual unificada**: selector de 17 roles → flujo paso a paso por rol (144 pasos) |
| `VALIDACION_CONTENIDO_MANUAL_INTERACTIVO.md` | Trazabilidad contenido ↔ código |
| `00_ANALISIS_COMPLETO_MANUAL_INTERACTIVO.md` | Análisis previo |

## Generación

```powershell
python scripts/generate_manual_interactivo.py
```

## Uso

### Manual interactivo (roles y pruebas)

1. Abra el HTML en Chrome/Edge (doble clic).
2. Elija modo: **Aprendizaje**, **Ejecución**, **Prueba**, **Presentación**.
3. En modo **Prueba**, marque PASS/FAIL/PENDING/NOT_EXECUTED y notas.
4. **Exportar JSON** guarda resultados; **Reset progreso** limpia `localStorage`.
5. Atajos: `/` buscar, `←` `→` navegar pasos.

### Guía visual unificada (17 roles)

1. Abra `COMPLIANCE360_GUIA_VISUAL_TODOS_LOS_ROLES.html` con **doble clic**.
2. **Elija un rol** en la pantalla inicial (Plataforma o Tenant).
3. Use **Siguiente →**, rail numérico, `←` `→` o **▶ Auto**.
4. Regenerar: `python scripts/generate_guia_visual_roles.py` (requiere análisis en `01_ANALISIS_GUIA_VISUAL_POR_ROLES.md`).

### Guía admin paso a paso (solo administradores)

1. Abra `COMPLIANCE360_GUIA_ADMIN_PASO_A_PASO.html` con **doble clic** (sin servidor).
2. Use **Siguiente →** o teclas `←` `→` — modo **▶ Auto** para recorrido animado.
3. Panel derecho explica cada paso; centro muestra pantallas idénticas a `app.js` / `styles.css`.
4. Siga el mismo orden en la app real (`http://localhost:5272`).

## Persistencia

Clave `localStorage`: `c360.manual.interactivo.v1`

## Datos verificados

- 17 roles (`RoleCatalog.cs`)
- Tenant Alimentos Premium Panamá S.A. (`ddcaf211-afe0-44a0-9c90-4fbda8fc4871`)
- Emails `@alimentos-premium.test` / plataforma `@compliance360.local`
- **Sin contraseñas** en el HTML — usar `e2e/testdata.json`

## Entorno

App en `http://localhost:5272` — ver `docs/manual-functional-testing/00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md`.
