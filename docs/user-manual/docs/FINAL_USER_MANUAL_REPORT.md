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
| Roles documentados | 9 | TAC, RA Admin, Manager, Specialist, Reviewer, Approver, Submitter, Viewer, Quality Manager |
| Pantallas inventariadas | 17 | Login, TAC, Audit, shell RA + 11 vistas internas |
| Campos explicados | 16 | Incluye nombre regulatorio, clase riesgo, CT/RS, oportunidad, fechas |
| Botones explicados | 19 | Prep, Aceptar/Rechazar, Aprobar internamente, Someter, Observación, Aprobación externa, Stage, Bootstrap |
| Tutoriales | 40+ | Por rol, paso concretos |
| Pasos simulador | 15 | Specialist → … → CT/RS → Renovación |
| Errores simulados | 11 | SoD + validación + sesión |
| Términos glosario | 26 | Incluye aprobación interna ≠ externa |
| Pruebas Playwright | 10/10 PASS | Ver `MANUAL_FUNCTIONAL_TEST_REPORT.md` |

## Fuentes reales (sin invención)

- `src/Compliance360.Domain/Identity/RoleCatalog.cs`
- `src/Compliance360.Domain/Identity/RbacCatalog.cs`
- `src/Compliance360.Web/wwwroot/regulatory-affairs.js` (STATUS_LABELS, botones, vistas)
- `src/Compliance360.Web/wwwroot/app.js` (rutas hash)
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

## Estructura

```text
docs/user-manual/
├── index.html
├── assets/css|js|icons|images
├── roles/*.html (9)
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
