# 00 — Análisis completo para Manual Interactivo HTML

**Versión:** 1.0  
**Fecha:** 2026-07-10  
**Alcance:** Compliance 360 — operación, aprendizaje y pruebas funcionales por rol  
**Fuente de verdad:** código (`app.js`, `FoundationEndpoints.cs`, `RoleCatalog.cs`) cruzado con `docs/manual-functional-testing/`, certificación, E2E y journey.

---

## 1. Inventario de roles reales (17)

| # | Rol | Ámbito | Usuario E2E provisionado | Notas |
|---|-----|--------|--------------------------|-------|
| 1 | Platform Administrator | Plataforma | `admin@compliance360.local` | Tenant plataforma `dc7c46ee-cb25-4ed5-b0b4-800788f7f626` |
| 2 | Platform Operations | Plataforma | — | Existe en `RoleCatalog.cs`; sin usuario demo por defecto |
| 3 | Platform Security | Plataforma | — | Existe en catálogo; sin usuario demo |
| 4 | Support Operator | Plataforma | `support@compliance360.local` | JWT `PLATFORM.SUPPORT.ACCESS`; sin UI break-glass dedicada |
| 5 | Tenant Administrator | Tenant | `tenantadmin@alimentos-premium.test` | TAC 15 tabs |
| 6 | Tenant Security Administrator | Tenant | `security@alimentos-premium.test` | Workspace `#/security` + TAC seguridad |
| 7 | Document Controller | Tenant | `doccontrol@alimentos-premium.test` | SoD: no aprueba documentos |
| 8 | Quality Manager | Tenant | `quality@alimentos-premium.test` | Aprueba vía API en varios flujos |
| 9 | Auditor | Tenant | `auditor@alimentos-premium.test` | No cierra CAPA |
| 10 | Supplier Manager | Tenant | `supplier@alimentos-premium.test` | |
| 11 | CAPA Manager | Tenant | `capa@alimentos-premium.test` | No aprueba cierre CAPA |
| 12 | Risk Manager | Tenant | `risk@alimentos-premium.test` | |
| 13 | Indicators Manager | Tenant | `indicators@alimentos-premium.test` | |
| 14 | Reporting Manager | Tenant | `reporting@alimentos-premium.test` | |
| 15 | Storage Administrator | Tenant | `storage@alimentos-premium.test` | `#/configuration` storage only |
| 16 | Notification Administrator | Tenant | `notifications@alimentos-premium.test` | `#/configuration` email only |
| 17 | Viewer | Tenant | `viewer@alimentos-premium.test` | Solo lectura transversal |

**Credenciales:** contraseñas en `e2e/testdata.json` y `appsettings.Development.json` — no documentar en HTML.

---

## 2. Inventario de módulos reales

### Operativos (CRUD + `#module-action-form`)

| Módulo | Ruta hash | Botón crear | Campos formulario |
|--------|-----------|-------------|-------------------|
| Document Management | `#/documents` | Crear registro real | name, code, description |
| Technical Sheets | `#/technical-sheets` | Crear registro real | name, code, description (producto) |
| Supplier Management | `#/suppliers` | Crear registro real | name (razón social), code (RUC), country |
| Audit Management | `#/audits` | Crear registro real | name, code, scope |
| CAPA | `#/capa` | Crear registro real | name, code, description |
| Risk Management | `#/risks` | Crear registro real | name, code, area, process |
| Quality Indicators | `#/indicators` | Crear registro real | name, code, unit |

### Enterprise workspaces (`#enterprise-action-form`)

| Módulo | Ruta | Botones |
|--------|------|---------|
| Template Builder | `#/template-builder` | Crear item enterprise |
| Regulatory Management | `#/regulatory` | Crear item enterprise |
| Training Management | `#/training` | Crear item enterprise |
| Supplier Portal | `#/supplier-portal` | Crear item enterprise |
| Customer Portal | `#/customer-portal` | Crear item enterprise |
| Security (workspace) | `#/security` | Crear item enterprise |
| Configuration | `#/configuration` | Crear Storage Local, Crear Email SMTP, Probar primer Storage Provider |

### Command Center

| Pantalla | Ruta |
|----------|------|
| Executive Dashboard | `#/dashboard` |
| Compliance Dashboard | `#/compliance` |
| Report Center | `#/reports` |
| Audit Trail | `#/audit-trail` |
| SuperAdmin Platform | `#/superadmin-platform` |
| Tenant Administration | `#/tenant-administration` |

---

## 3. Inventario de rutas (19 claves únicas)

`dashboard`, `compliance`, `reports`, `audit-trail`, `superadmin-platform`, `documents`, `technical-sheets`, `suppliers`, `audits`, `capa`, `risks`, `indicators`, `tenant-administration`, `template-builder`, `regulatory`, `training`, `supplier-portal`, `customer-portal`, `security`, `configuration`

**Grupos menú (`app.js`):** Command Center, Operations, Enterprise.

---

## 4. Inventario de pantallas principales

| Pantalla | Tabs / secciones clave |
|----------|------------------------|
| Login v2 | email → organization (opcional) → password → MFA (opcional) |
| Shell | sidebar, topbar, `#content`, `#logout` (Salir) |
| TAC | 15 tabs: general, branding, security, users, rbac, licensing, domains, sso, apikeys, webhooks, storage, notifications, health, audit, state |
| SuperAdmin Platform | 13 tabs: Executive, Tenants, Licencias, Modulos, Providers, Seguridad Global, Observability, Auditoria Global, Base de Datos, IA, Configuracion Global, Backups, DevOps |
| Module pages | hero + workflow strip + Action Center + tabla + búsqueda |
| Report Center | seed, execute, schedule, export |
| Provider Administration | storage vs notification SoD |

---

## 5. Formularios y botones principales

### Login v2 (texto exacto UI)

- `Siguiente` → `Continuar` / `Atras` → `Iniciar sesion` (sin acento en código)
- MFA: `Completar login seguro`, `Cancelar`

### TAC — botones submit

`Guardar informacion general`, `Guardar branding`, `Guardar seguridad`, `Crear / Invitar usuario`, `Crear rol`, `Crear permiso y otorgar`, `Asignar rol`, `Guardar licenciamiento`, `Guardar dominio`, `Guardar SSO`, `Crear API Key`, `Guardar webhook`, `Registrar backup`, `Exportar`, estados: `Mover a Trial`, `Activar`, `Suspender`, `Archivar`, `Restaurar`

### Platform — tenant

`Crear tenant`, `Cancelar`, `Abrir TAC`, `Exportar auditoria global CSV`

### Modo solo lectura

Heading `Modo solo lectura` cuando el rol no tiene permiso CREATE del módulo.

---

## 6. Estados y transiciones (resumen verificado)

| Dominio | Estados clave | Transiciones UI/API |
|---------|---------------|---------------------|
| Document | Draft, InReview, Approved, Rejected | submit → decision |
| CAPA | Draft → Open → InProgress → PendingApproval → Closed | classify, root-cause, actions, effectiveness, close |
| Risk | Draft → assessed → treatment → closed | assess, treat, close (QM) |
| Audit | program → plan → start → finding → complete → close | parcial en UI; journey API completo |
| Technical Sheet | Draft → InReview → Approved | version → submit → decision (QM) |
| Tenant | Trial, Active, Suspended, Archived | platform-center lifecycle |
| Report | seed → execute → complete → export | Report Center buttons |

---

## 7. Flujos por rol (resumen)

Cada rol: login v2 → landing según RBAC → módulos visibles en sidebar → acciones permitidas (create/approve/read-only) → handoff documentado en manual MFT correspondiente.

**SoD críticos verificados:**

- Document Controller crea / Quality Manager aprueba (no crea en UI)
- CAPA Manager gestiona / Quality Manager cierra
- Risk Manager gestiona / Quality Manager aprueba y cierra
- Storage Admin vs Notification Admin (configuration SoD)
- Platform Admin no ve módulos de negocio en menú

---

## 8. Dependencias entre roles

```
Platform Admin → Tenant Admin → TSA + Storage + Notification → roles operativos
Document Controller → Quality Manager → Viewer
Tenant Admin (grant TECHNICALSHEET.CREATE) → Technical Sheets → Quality Manager
Supplier Manager → CAPA Manager → Quality Manager → Reporting Manager
Auditor → CAPA Manager → Quality Manager
Risk Manager → Quality Manager → Indicators Manager → Reporting Manager
```

---

## 9. Orden correcto de implementación (secuencia maestra)

1. Preparación entorno (`localhost:5272`, PostgreSQL, `e2e_provision.ps1`)
2. Platform Admin — salud plataforma
3. Crear tenant (slug único)
4. Activar tenant
5. Tenant Admin — información general
6. Branding
7. Tenant Security — políticas MFA/lockout
8. Usuarios especialistas (Security, Storage, Notification, DC)
9. Roles y permisos (RBAC tab; TECHNICALSHEET.CREATE si aplica)
10. Storage provider local
11. Notification provider (SMTP demo = PENDING third-party real)
12. Documentos
13. Fichas técnicas (requiere grant CREATE)
14. Proveedores
15. Auditorías
16. CAPA
17. Riesgos
18. Indicadores
19. Reportes
20. Viewer — validación read-only
21. Support Operator — JWT/API (no UI dedicada)
22. Audit trail + cierre funcional

---

## 10. Material reutilizable

| Fuente | Uso en manual HTML |
|--------|-------------------|
| `docs/manual-functional-testing/01–17` | Casos TC-* y E2E-* paso a paso |
| `docs/functional-certification/APPLICATION_FUNCTIONAL_MAP.md` | Rutas, permisos, endpoints |
| `e2e/testdata.json` | Emails, tenant IDs |
| `scripts/customer_journey.ps1` | Flujos API completos |
| `demo/INPUT_DICTIONARY_BY_SCREEN.md` | Glosario de campos |
| `docs/demo/DEMO_README.md` | Narrativa Alimentos Premium |
| `artifacts/manual-functional-testing/technical_sheets_mft_result.json` | Evidencia TS |

---

## 11. Material obsoleto o no usar tal cual

| Material | Problema |
|----------|----------|
| `docs/training/` Academy | Roles legacy: SuperAdmin, Approver, Supplier User |
| `docs/e2e/` junio 2026 | Numeración duplicada; preferir MFT + certificación julio |
| `04_ROLE_FUNCTIONAL_FLOWS.md` (raíz) | Tenant ID antiguo; login legacy |
| Demo QM "aprueba en UI" | Simulado; producción = API para varios approve |

---

## 12. Contradicciones encontradas

| Tema | Implementación real | Documentación previa |
|------|---------------------|----------------------|
| Login submit | `Iniciar sesion` | Algunos docs con acento |
| TECHNICALSHEET.CREATE | No en rol estándar | Manual 08 asume grant previo |
| Support break-glass | Solo permiso JWT | Manuales implican pantalla |
| QM document approve | API `/documents/{id}/decision` | Demo sugiere UI |
| MFA tenant nuevo | Requiere SQL en journey | No scriptable en browser puro |
| Platform Ops/Security | Roles existen | Sin usuarios E2E |

**Resolución manual HTML:** documentar texto exacto de `app.js`; marcar API/journey donde UI incompleta; PENDING third-party para SMTP real.

---

## 13. Vacíos documentales

- Platform Operations / Platform Security: sin manual MFT dedicado → sección derivada de permisos en catálogo
- Support Operator: 8/12 casos dependen de API/Swagger
- Audit program → close: UI parcial; journey cubre API
- CAPA 5-Why / Ishikawa: sin UI dedicada en browser
- Upload documento adjunto: parcial en UI

---

## 14. Riesgos de confusión para el usuario

1. Confundir demo offline (`demo/index.html`) con app real (`localhost:5272`)
2. Intentar crear ficha técnica sin grant RBAC
3. Esperar que Quality Manager vea formulario de creación
4. Buscar pantalla break-glass para Support Operator
5. Mezclar tenant plataforma vs tenant negocio en login
6. Olvidar re-login tras grant de permisos (JWT stale)

---

## 15. Estructura propuesta del HTML

```
Encabezado corporativo + buscador + modo + tema
├── Introducción (qué es C360, arquitectura, multi-tenant)
├── Secuencia maestra (22 pasos navegables)
├── Roles (17 secciones)
│   ├── Identidad + permisos
│   ├── Flujo diario
│   └── Casos TC (modo aprendizaje / ejecución / prueba)
├── Flujos transversales (8)
├── Validaciones negativas
├── Dependencias third-party
└── Export / progreso / checklist global
```

**Modos:** Aprendizaje | Ejecución | Prueba (PASS/FAIL/PENDING) | Presentación  
**Persistencia:** `localStorage` clave `c360.manual.interactivo.v1`

---

## 16. Secuencia maestra recomendada

Ver sección 9 — alineada con `00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md` §4 y `customer_journey.ps1`.

**Datos demo coherentes:**

- Empresa: **Alimentos Premium Panamá S.A.**
- Slug tenant: `alimentos-premium-pa`
- Tenant negocio: `ddcaf211-afe0-44a0-9c90-4fbda8fc4871`
- Documentos: `BPM-LIM-001`, `POE-CCP-014`
- CAPA: `CAPA-2026-014`
- Riesgo: `RSK-CC-001`
- Indicador: `KPI-NC-RATE`
- Ficha técnica: `TS-MFT-001`

---

## 17. Veredicto de readiness para construcción HTML

| Criterio | Estado |
|----------|--------|
| Roles inventariados | ✅ 17/17 |
| Rutas verificadas en código | ✅ |
| Botones/campos verificados | ✅ muestra representativa |
| Casos MFT disponibles | ✅ 204 TC + 12 E2E |
| Flujos API de respaldo | ✅ journey + scripts |
| Contradicciones documentadas | ✅ |
| **Autorizado construir HTML** | ✅ |

---

*Siguiente entregable:* `COMPLIANCE360_MANUAL_INTERACTIVO_POR_ROLES_Y_FLUJOS.html` generado desde `scripts/generate_manual_interactivo.py` con datos verificados.
