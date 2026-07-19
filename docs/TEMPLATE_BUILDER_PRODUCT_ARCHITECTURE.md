# TEMPLATE BUILDER — ARQUITECTURA DE PRODUCTO (ANÁLISIS PREVIO)

**Fecha:** 2026-07-14  
**Autoría:** Rediseño de producto insignia Compliance 360  
**Regla aplicada:** cero líneas de implementación en este documento; solo evidencia + diseño.

---

## 1. Qué existe HOY (código real)

### 1.1 Camino viejo (legado, no es el motor)

| Pieza | Ruta |
|-------|------|
| Entidad | `EnterpriseWorkspaceItem` (`Type = TemplateBuilder`) |
| Tabla | `compliance360.enterprise_workspace_items` |
| API | `/enterprise-workspaces` |
| UI | `renderEnterpriseWorkspace` en `app.js` |

Sigue siendo un tracker título/código. **La UI de `#/template-builder` ya no lo usa.**

### 1.2 Motor nuevo (ya introducido)

| Capa | Evidencia |
|------|-----------|
| Dominio | `Domain/FormTemplates/FormTemplateModels.cs` — `FormTemplate`, `FormTemplateVersion` |
| App | `Application/FormTemplates/FormTemplateService.cs` |
| Infra | `Infrastructure/FormTemplates/EfFormTemplateRepository.cs` |
| Tablas | `form_templates`, `form_template_versions` (migración `20260714010000`) |
| Permisos | `TEMPLATE.READ`, `TEMPLATE.MANAGE` → Tenant Administrator |
| API | `/api/v1/tenants/{tenantId}/form-templates` |
| UI | `wwwroot/form-template-builder.js` |
| Shell | SPA `app.js` + `styles.css` + JWT + `routePermissions` |

### 1.3 Gap vs producto comercial (estado actual del .js)

| Capacidad PowerApps-class | Hoy |
|---------------------------|-----|
| Experiencia “producto independiente” | Embebido en layout genérico C360 |
| DnD real canvas | No (click + ↑↓) |
| Undo/Redo | No |
| Autosave | No |
| Layout grid/filas/columnas | Solo `width` full/half/third |
| Rule builder AND/OR/ELSE | Rules simples |
| Expresiones / calculados | No |
| Workflow bind | No |
| Marketplace de starters | No |
| Dark studio / Figma chrome | No |
| Consumo desde CAPA/Audits | No (solo API de listado) |
| Multi-select, clipboard, zoom | No |

---

## 2. Arquitectura de plataforma (inmutable)

No se reescribe la plataforma. Se **apoya** en ella:

| Concern | Mecanismo existente |
|---------|---------------------|
| Frontend | SPA vanilla (`app.js`, rutas hash, toast, `request`) |
| Backend | Minimal APIs + Application services + EF Core |
| Multitenancy | `TenantId` en entidad + claim JWT |
| RBAC | `PermissionCatalog` + `PermissionPolicies` + claims |
| Auditoría | `AuditLog` append-only |
| Soft delete | Flag `IsDeleted` en `FormTemplate` |
| Versionado | `FormTemplateVersion` inmutable al publicar |

**No se tocan** los bounded contexts Documents, CAPA, Risk, AuditManagement, Identity, Enterprise Workspace genérico.

---

## 3. Visión de producto: “Compliance Studio — Form Engine”

Nombre de producto interno en UI: **Compliance Studio** (submarca del Template Builder).

### 3.1 Principios

1. **Form Engine del tenant**, no un CRUD admin.
2. **Schema-first**: todo el diseñador serializa a `SchemaJson` versionado.
3. **Consumible**: cualquier módulo lee plantillas **Published** por `Kind`.
4. **UX de producto**: pantalla a pantalla completa, chrome propio, no “otra card del TAC”.

### 3.2 Schema canónico (v2)

```json
{
  "schemaVersion": 2,
  "meta": { "name": "...", "kind": "Audit", "locale": "es" },
  "layout": { "mode": "grid", "columns": 12, "gap": 12, "sections": [] },
  "components": [ /* árbol: container | field | layout */ ],
  "rules": [ /* AST: when all/any, then actions[] */ ],
  "expressions": [ /* id, formula, targetComponentId */ ],
  "workflow": { "steps": [ { "id":"start","type":"Start" }, ... ] },
  "theme": { "mode": "system", "density": "comfortable" }
}
```

Compatible hacia atrás: si `schemaVersion===1` o existen `fields[]`, el loader migra en memoria a `components[]`.

---

## 4. Plan de implementación (sin romper módulos)

| Fase | Entrega | Rompe otros módulos? |
|------|---------|----------------------|
| A | Análisis (este doc) | No |
| B | Studio UI premium + DnD + undo + autosave + starters + rules v2 + preview | No |
| C | API: duplicate, published-by-kind, restore version | No (aditivo) |
| D | Punto de integración read-only en módulos (helper JS + endpoint) | No (opt-in) |
| E | Runtime fill & submit instances | Futuro (entidad `FormSubmission`) — fuera de esta entrega si el tiempo no alcanza el submit completo |

---

## 5. Riesgos

| Riesgo | Mitigación |
|--------|------------|
| Prometer paridad 100% PowerApps | Documentar honesty; entregar producto usable + arquitectura extensible |
| Schema v1 vs v2 | Migración in-memory en loader |
| Permisos viejos en JWT | Requiere re-login (ya documentado) |
| Migración EF sin snapshot completo | Tabla ya creada; bootstrap Migrate |

---

## 6. Criterio de éxito de ESTA entrega

Un arquitecto abre `#/template-builder` y ve:

- Landing tipo producto (galería / starters / recientes).
- Studio a pantalla completa estilo Figma/PowerApps.
- Toolbar + paleta (>40 componentes) + canvas DnD + inspector + footer.
- Undo/Redo, autosave, preview multi-device, dark/light.
- Rules con ALL/ANY y acciones múltiples.
- Expresiones y workflow declarativo en schema.
- Versiones: publicar, historial, restaurar a borrador.
- Endpoint para que Audits/CAPA/Risk **lean** plantillas publicadas por tipo.

No se afirma “más grande que Mendix en 1 día”; se entrega un **producto comercial creíble** como Form Engine del tenant, listo para producción y para crecer 10 años.

---

## 7. Decisión

**Proceder a implementación** basándose en `FormTemplate` existente (no en `EnterpriseWorkspaceItem`), reemplazando por completo la experiencia UI actual de `form-template-builder.js` + CSS de producto dedicado.
