# 07 — Role Gap Analysis

## Metodología

Comparación entre: (A) diseño recomendado en `TENANT_ADMIN_RESPONSIBILITY_MATRIX.md`, (B) catálogo de permisos en código, (C) implementación API/UI, (D) configuración E2E validada.

---

## Gaps por categoría

### G1 — Roles sin definición canónica en código

| Gap | Evidencia | Impacto |
|-----|-----------|---------|
| Solo `SuperAdmin` está sembrado | `DevelopmentBootstrap.cs` | Cada tenant define roles distintos — inconsistencia entre clientes |
| No hay enum ni constantes de roles de negocio | `IdentityModels.cs` — `Role` es string | Imposible validar roles en compile-time |
| Nombres E2E con sufijo "E2E" | Scripts `tools/e2e_*` | No hay plantillas de rol reutilizables |

**Severidad:** Alta  
**Tipo:** Diseño / gobernanza

---

### G2 — Permisos faltantes en catálogo

| Permiso esperado (UI/docs) | Existe en bootstrap | Consecuencia |
|----------------------------|---------------------|--------------|
| `DOCUMENT.READ` | No | Menú documents invisible para Viewer; solo MANAGE funciona |
| `TECHNICALSHEET.READ` | No | Misma situación |
| `SUPPLIER.READ` | No | Misma situación |
| `AUDITMANAGEMENT.READ` | No | Dashboard requiere MANAGE |
| `SUPERADMIN.DASHBOARD` | No (existe `.READ`) | Menú SuperAdmin puede no mostrarse |
| `DOCUMENT.APPROVE` | No | SoD imposible |
| `WORKFLOW.READ` | No | Solo MANAGE |

**Severidad:** Alta  
**Tipo:** Catálogo incompleto

---

### G3 — Permisos excesivos / solapados

| Rol | Permiso excesivo | Evidencia |
|-----|------------------|-----------|
| SuperAdmin | Los 76 + bypass | Bootstrap + `HasPlatformSuperAdmin` |
| Quality Manager | Todos los `*.MANAGE` de calidad | E2E grants — 20 permisos |
| Auditor | `CAPA.MANAGE` | E2E — debería solo originar vía workflow |
| Supplier Manager | `CAPA.MANAGE` (añadido en fix) | E2E transcript L673 |
| Notification Admin | `STORAGE.MANAGE` | E2E L726 |
| 10/12 roles tenant | `AUDITMANAGEMENT.MANAGE` | Solo para métrica dashboard |

**Severidad:** Media-Alta  
**Tipo:** Sobre-privilegio

---

### G4 — Gaps de enforcement frontend

| Gap | Ubicación | Riesgo |
|-----|-----------|--------|
| Sin route guard en `renderRoute` | `app.js` L653–708 | Pantallas cargan sin permiso |
| Quick-switcher sin filtro RBAC | `app.js` L494–496 | Navegación a rutas prohibidas |
| TAC tabs sin `hasPermission` | `app.js` L1499–1859 | Formularios visibles; API 403 |
| SuperAdmin quick actions sin filtro | `app.js` L1167–1168 | Acciones mostradas sin permiso |
| Enterprise workspaces sin `canManageRoute` | `app.js` L2303–2316 | Crear con TENANT.READ |
| Configuration sin split storage/email | `app.js` configuration route | Permiso amplio |

**Severidad:** Media  
**Tipo:** UX / defense in depth

---

### G5 — Gaps de enforcement backend

| Gap | Evidencia |
|-----|-----------|
| SuperAdmin bypass en todas las policies | `PermissionPolicies.cs` |
| `Tenant.Manage` demasiado amplio | Cualquier `TENANT.*` abre enterprise-workspaces |
| `RbacService` sin bypass pero poco usado en endpoints | Mayoría usa policies, no servicio |
| Endpoints anónimos `/health`, `/metrics` | `Program.cs` — aceptable |
| MFA verify sin policy específica | `FoundationEndpoints.cs` |
| Muchas policies `SuperAdmin.*` sin endpoint | Licencias, Modules, AI, etc. |

**Severidad:** Media-Alta  
**Tipo:** Seguridad API

---

### G6 — Roles duplicados / fusionables

| Observación | Recomendación |
|-------------|---------------|
| Quality Manager ≈ union de Doc Ctrl + CAPA + Risk + Indicators | Dividir o reducir QM a rol "aprobador/coordinador" |
| `compliance` route = mismo `renderDashboard()` que `dashboard` | Fusionar rutas UI o diferenciar |
| `superadmin-platform` duplicado en menú Operations y Enterprise | Eliminar duplicado |
| Tenant Admin vs Tenant Security Admin (doc) | Security Admin no existe en E2E |
| `Auditor` test role vs `Auditor E2E` | Mismo concepto, distinto provisioning |

**Severidad:** Baja (claridad)  
**Tipo:** Modelo conceptual

---

### G7 — Módulos sin responsable único

| Módulo | Responsables en práctica E2E |
|--------|------------------------------|
| CAPA | CAPA Mgr, QM, Auditor, Supplier Mgr |
| Dashboard métricas | Todos (con permisos cruzados) |
| Audit Management | Auditor + cualquiera con MANAGE |
| Configuration | Storage Admin + Notification Admin |
| Enterprise workspaces | Cualquiera con TENANT.READ |
| Workflows | Doc Ctrl + QM |

**Severidad:** Media  
**Tipo:** Gobernanza funcional

---

### G8 — Flujos incorrectos detectados en E2E

| Flujo | Problema | Corrección aplicada |
|-------|----------|---------------------|
| Supplier → CAPA | 403 CAPA.MANAGE | Grant ad hoc |
| CAPA Manager dashboard | 403 audit-management/dashboard | Grant AUDITMANAGEMENT.MANAGE |
| Storage Admin dashboard | 403 métricas | Grants lectura cruzada |
| Viewer menú | Veía rutas de creación | Filtro UI por JWT |
| Viewer DOCUMENT.READ | Permiso inexistente | Usar solo *.READ existentes |
| Tenant Admin login | MFA required sin TOTP | Desactivar requireMfa en tenant prueba |
| RBAC grant | 500 concurrency | Fix idempotencia repositorio |

**Severidad:** Alta (durante E2E) — revela gaps estructurales, no solo bugs

---

### G9 — Permisos insuficientes por rol (diseño ideal vs E2E)

| Rol | Falta para diseño ideal | Notas |
|-----|-------------------------|-------|
| Document Controller | `DOCUMENT.READ` para consultas sin MANAGE | Hoy MANAGE implica todo |
| Auditor | `AUDITMANAGEMENT.READ` sin MANAGE | Hoy necesita MANAGE para UI |
| Reporting Manager | `REPORT.SCHEDULE` si debe programar | No en E2E |
| Viewer | `DOCUMENT.READ`, `SUPPLIER.READ` | No existen en catálogo |
| Tenant Admin | `TENANT.BRANDING`, `TENANT.BILLING`, `TENANT.DOMAINS` | No en E2E base (solo subset) |
| SuperAdmin | Permisos de plataforma sin APIs | UI tabs huérfanas |

---

## Matriz gap × rol

| Rol | G1 | G2 | G3 | G4 | G5 | G6 | G7 | G8 | G9 |
|-----|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| SuperAdmin | ✓ | ✓ | ✓ | ✓ | ✓ | | ✓ | | |
| Tenant Admin | ✓ | | | ✓ | | | | ✓ | ✓ |
| Document Controller | ✓ | ✓ | | | | | | | ✓ |
| Quality Manager | ✓ | ✓ | ✓ | | | ✓ | ✓ | | |
| Auditor | ✓ | ✓ | ✓ | | | | ✓ | | ✓ |
| Supplier Manager | ✓ | ✓ | ✓ | | | | ✓ | ✓ | |
| CAPA Manager | ✓ | ✓ | | | | | ✓ | ✓ | ✓ |
| Risk Manager | ✓ | | | | | | ✓ | | |
| Indicators Manager | ✓ | | | | | | ✓ | | |
| Reporting Manager | ✓ | | | | | | | | ✓ |
| Storage Admin | ✓ | | | ✓ | | ✓ | ✓ | ✓ | |
| Notification Admin | ✓ | | ✓ | ✓ | | ✓ | ✓ | | |
| Viewer | ✓ | ✓ | | ✓ | | | | ✓ | ✓ |

---

## Roles que deberían dividirse

| Rol actual | Dividir en | Razón |
|------------|------------|-------|
| Quality Manager | Quality Approver + módulos especializados | Demasiados MANAGE |
| SuperAdmin | PlatformAdmin + BreakGlassSupport | SoD plataforma |
| Tenant Admin | TenantAdmin + TenantSecurityAdmin | Doc ya lo recomienda |
| Notification Admin | NotificationAdmin (sin STORAGE) | SoD-07 |

---

## Roles que podrían fusionarse

| Roles | Condición |
|-------|-----------|
| dashboard + compliance | Misma implementación `renderDashboard()` |
| Reporting Manager + Viewer (reportes) | Si solo ejecuta sin diseñar — subconjunto |
| CAPA Manager + Quality Manager (CAPA) | Solo si se reduce alcance QM |

---

## Resumen cuantitativo

| Métrica | Valor |
|---------|-------|
| Gaps críticos (G3, G5 SuperAdmin) | 2 |
| Gaps altos (G1, G2, G8) | 4 |
| Gaps medios (G4, G7) | 2 |
| Permisos en catálogo | 76 |
| Permisos referenciados solo en UI | 4+ |
| Roles con plantilla E2E | 13 |
| Roles con plantilla en código | 1 |

**Calificación gap analysis: 42/100** — modelo funcional en pruebas pero sin gobernanza RBAC enterprise cerrada.
