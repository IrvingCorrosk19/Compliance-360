# FINAL_USER_MANUAL_REPORT

## Veredicto

**INTERACTIVE ROLE-BASED USER MANUAL: PASS**

## Entregable

Abrir con doble clic:

`docs/user-manual/index.html`

No requiere npm, servidor, base de datos ni autenticación real.

## Cobertura

| Dimensión | Cantidad | Notas |
|-----------|----------|-------|
| Roles documentados | 10 | TAC, **Notification Administrator**, RA Admin, Manager, Specialist, Reviewer, Approver, Submitter, Viewer, Quality Manager |
| Pantallas inventariadas | 24 | Login, TAC, Audit, shell RA + vistas RA + **7 pantallas Alert Center** |
| Campos explicados | 19+ | Incluye nombre regulatorio, clase riesgo, CT/RS, búsqueda Inbox, código de regla, tipo provider |
| Botones explicados | 28 | Prep/SoD RA + campana, Inbox, Template Center, Reglas, Providers, Operations |
| Tutoriales | 50+ | Por rol; incluye Inbox y configuración Alert Center (NA) |
| Pasos simulador | 15 | Specialist → … → CT/RS → Renovación |
| Errores simulados | 14 | SoD + validación + sesión + 401 Alert Center + provider + SoD Storage |
| Términos glosario | 32+ | Incluye aprobación interna ≠ externa + Alert Center / Inbox / NOTIFICATION.* |
| Pruebas Playwright | 10/10 PASS (baseline RA) | Ver `MANUAL_FUNCTIONAL_TEST_REPORT.md` — ampliar NA en siguiente ciclo |

## Fuentes reales (sin invención)

- `src/Compliance360.Domain/Identity/RoleCatalog.cs`
- `src/Compliance360.Domain/Identity/RbacCatalog.cs`
- `src/Compliance360.Web/wwwroot/regulatory-affairs.js` (STATUS_LABELS, botones, vistas)
- `src/Compliance360.Web/wwwroot/alert-center.js` (Inbox, Template Center, Rules, Scheduler, Providers, Operations)
- `src/Compliance360.Web/wwwroot/app.js` (rutas hash, campana)
- `docs/alert-center/` (arquitectura, RBAC, manual administrador)
- `docs/regulatory-affairs/security/04_TARGET_ROLE_MODEL.md`
- `docs/regulatory-affairs/security/17_ROLE_TEST_USER_MATRIX.md`
- `docs/regulatory-affairs/security/21_ROLE_UI_CERTIFICATION.md`
- `docs/regulatory-affairs/security/24_FINAL_ROLE_AND_SOD_CERTIFICATION.md`

## Distinción crítica enseñada

| Texto UI | Significado | Rol |
|----------|-------------|-----|
| Aprobado internamente para sometimiento | Autorización interna | Approver |
| Sometido ante autoridad / Sometimiento registrado | Envío real | Submitter |
| Observado por autoridad | Observación recibida | Manager registra |
| Aprobación registrada de MINSA/CSS (externa) | Decisión externa | Manager / QM |
| CT/RS activo | Número emitido por autoridad | Manager / QM |
| Alert Center Inbox | Notificaciones persistentes del usuario | Roles con NOTIFICATION.READ |
| Configuración Alert Center | Templates/reglas/providers/ops | Notification Administrator |

## Estructura

```text
docs/user-manual/
├── index.html
├── assets/css|js|icons|images
├── roles/*.html (10)
├── data/*.json
├── docs/*.md (trazabilidad + reportes)
└── README.md
```

## Limitaciones

- La UI real de creación usa `prompt()` del navegador; el manual explica esos campos y valores exactos, y el simulador practica el flujo sin backend.
- Algunos módulos Enterprise (usuarios TAC) se documentan a nivel de ruta `#/tenant-administration` según permisos reales; el detalle de cada subpanel IAM depende de la build local.
- Sales Viewer / Document Contributor no están implementados (docs) y no se documentan como disponibles.
- Regeneración: `python scripts/generate-user-manual.py` (luego reaplicar ajuste de `search.js` basePrefix si se regenera sin merge).

## App real para practicar después del manual

- `http://localhost:5272`
- Login `#/login`
- Consola `#/regulatory`
- Alert Center `#/alert-center`
