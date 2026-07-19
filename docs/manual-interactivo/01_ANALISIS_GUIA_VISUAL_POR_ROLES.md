# 01 — Análisis: Guía visual paso a paso por rol

**Fecha:** 2026-07-10  
**Objetivo:** Manual HTML autocontenido (como `COMPLIANCE360_GUIA_ADMIN_PASO_A_PASO.html`) para **los 17 roles**, unificado con **selector de rol** y recorrido animado pantalla a pantalla.  
**Salida:** `COMPLIANCE360_GUIA_VISUAL_TODOS_LOS_ROLES.html`  
**Generador:** `scripts/generate_guia_visual_roles.py`

---

## 1. Relación con entregables existentes

| Artefacto | Propósito | Diferencia |
|-----------|-----------|------------|
| `COMPLIANCE360_GUIA_ADMIN_PASO_A_PASO.html` | Solo Platform + Tenant Admin (login → tenant → TAC) | 18 pasos fijos |
| `COMPLIANCE360_MANUAL_INTERACTIVO_POR_ROLES_Y_FLUJOS.html` | 88 casos TC, modos Aprendizaje/Prueba, PASS/FAIL | Texto + checklist, no mock UI completa |
| **Nuevo unificado** | Elegir rol → flujo visual idéntico a `app.js` | Mock UI + coach + hotspots |

---

## 2. Inventario de roles y flujos visuales

| # | Rol | Email E2E | Pasos visuales | Pantalla ancla |
|---|-----|-----------|----------------|----------------|
| 1 | Platform Administrator | admin@compliance360.local | 14 | `#/superadmin-platform` |
| 2 | Platform Operations | — (proxy demo) | 8 | `#/superadmin-platform` Tenants/Providers |
| 3 | Platform Security | — (proxy demo) | 8 | `#/superadmin-platform` Seguridad/Auditoría |
| 4 | Support Operator | support@compliance360.local | 9 | `#/superadmin-platform` (sin break-glass UI) |
| 5 | Tenant Administrator | tenantadmin@… | 12 | `#/tenant-administration` |
| 6 | Tenant Security Administrator | security@… | 10 | `#/security` + TAC Seguridad/Dominios |
| 7 | Document Controller | doccontrol@… | 9 | `#/documents` Crear registro real |
| 8 | Quality Manager | quality@… | 8 | `#/documents` Modo solo lectura + API |
| 9 | Auditor | auditor@… | 9 | `#/audits` |
| 10 | Supplier Manager | supplier@… | 9 | `#/suppliers` |
| 11 | CAPA Manager | capa@… | 9 | `#/capa` |
| 12 | Risk Manager | risk@… | 9 | `#/risks` |
| 13 | Indicators Manager | indicators@… | 9 | `#/indicators` + `#/dashboard` |
| 14 | Reporting Manager | reporting@… | 9 | `#/reports` |
| 15 | Storage Administrator | storage@… | 8 | `#/configuration` Storage |
| 16 | Notification Administrator | notifications@… | 8 | `#/configuration` SMTP |
| 17 | Viewer | viewer@… | 8 | Modo solo lectura transversal |

**Total:** ~163 pasos visuales agregados (promedio 8–12 por rol).

---

## 3. Plantilla común de pasos (tenant operativo)

```
1. Intro rol (propósito, permisos, handoffs)
2. Login — Correo → Siguiente
3. Login — Organización Alimentos Premium → Continuar
4. Login — Contraseña → Iniciar sesion
5. Shell post-login (sidebar resaltado)
6. Módulo landing — hero + Action Center
7. Crear registro real (campos verificados app.js)
8. Acción secundaria / SoD / workflow
9. Audit Trail o handoff
10. Checklist cierre rol
```

---

## 4. Pantallas mock (clases CSS reales)

Réplica de `styles.css` / `app.js`:

| Mock | Clases / IDs |
|------|----------------|
| Login v2 | `login-page`, `login-panel`, `login-hero`, `org-option`, `btn primary` |
| Shell | `layout`, `sidebar`, `nav-button.active`, `topbar`, `tenant-chip` |
| Módulo operativo | `module-page`, `module-hero`, `workflow-strip`, `#module-action-form`, **Crear registro real** |
| Solo lectura | `read-only-notice`, heading **Modo solo lectura** |
| SuperAdmin | `tenant-admin-shell`, `tenant-tab`, `platform-command` |
| TAC | `tenant-panel`, tabs 1–15 |
| Configuration | `Provider Administration`, **Crear Storage Local**, **Crear Email SMTP** |
| Report Center | seed, execute, export |
| Enterprise Security | `#/security`, `#enterprise-action-form` |

---

## 5. Rutas hash por rol

| Rol | Ruta principal | Rutas secundarias |
|-----|----------------|-------------------|
| Platform Admin | `#/superadmin-platform` | `#/tenant-administration` (post-create) |
| Tenant Admin | `#/tenant-administration` | tabs general…state |
| Document Controller | `#/documents` | `#/technical-sheets` (read-only) |
| Quality Manager | `#/documents` | Swagger/API (coach, no mock API) |
| Auditor | `#/audits` | `#/capa` (SoD) |
| Supplier Manager | `#/suppliers` | — |
| CAPA Manager | `#/capa` | `#/audit-trail` |
| Risk Manager | `#/risks` | — |
| Indicators Manager | `#/indicators` | `#/dashboard` |
| Reporting Manager | `#/reports` | `#/compliance` |
| Storage Admin | `#/configuration` | storage panel only |
| Notification Admin | `#/configuration` | email panel only |
| Tenant Security | `#/security` | TAC seguridad, dominios |
| Viewer | `#/dashboard` | documents, capa, risks (read-only) |
| Support | `#/superadmin-platform` | `#/audit-trail` |

---

## 6. Limitaciones documentadas (no inventar UI)

- **QM / CAPA / Risk approve:** vía API Swagger, sin botón UI dedicado.
- **Support Operator:** sin pantalla break-glass; validación API.
- **Platform Operations / Security:** sin usuario E2E; flujo con nota + proxy visual.
- **TECHNICALSHEET.CREATE:** grant RBAC previo (Tenant Admin).
- **SMTP producción:** PENDING third-party.
- **Contraseñas:** no van en HTML → `e2e/testdata.json`.

---

## 7. Arquitectura HTML unificado

```
┌─────────────────────────────────────────────────────────┐
│ Header: título + rol activo + chip ruta + Anterior/Sig. │
├─────────────────────────────────────────────────────────┤
│ Rail pasos (numerado, clickeable)                        │
├──────────────────────────┬──────────────────────────────┤
│ Mock frame (app replica) │ Coach panel (explicación)    │
│ + hotspots animados      │ + action steps               │
└──────────────────────────┴──────────────────────────────┘

Pantalla 0: Selector de rol (17 tarjetas Platform / Tenant)
→ al elegir: carga flujo del rol + reinicia paso 0
```

**Persistencia:** `localStorage` clave `c360.guia.visual.v1` (último rol + paso).

---

## 8. Criterios de aceptación

- [ ] Abrir con doble clic (`file://`) sin servidor
- [ ] Selector muestra 17 roles agrupados
- [ ] Cada rol: login v2 + pantallas = app real (textos `app.js`)
- [ ] Navegación ← →, rail, modo Auto
- [ ] Regenerable: `python scripts/generate_guia_visual_roles.py`

---

## 9. Orden de implementación

1. Este análisis ✓  
2. `generate_guia_visual_roles.py` (datos + template JS mock)  
3. `COMPLIANCE360_GUIA_VISUAL_TODOS_LOS_ROLES.html`  
4. Actualizar `docs/manual-interactivo/README.md`
