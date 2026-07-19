# TEMPLATE_BUILDER_ANALYSIS.md

**Fecha:** 2026-07-13  
**Sistema:** Compliance 360  
**Alcance:** Rediseño del módulo Template Builder (`#/template-builder`)

---

## 1. Estado actual (evidencia en código)

El “Template Builder” **no es un constructor de plantillas**. Es un discriminator `EnterpriseWorkspaceType.TemplateBuilder = 0` sobre la entidad genérica `EnterpriseWorkspaceItem`.

| Aspecto | Realidad |
|---------|----------|
| Almacenamiento | Tabla `compliance360.enterprise_workspace_items` |
| Campos | Title, Code, Description, Status, DueAtUtc, MetadataJson |
| UI | CRUD: crear ítem + listar + completar primero |
| Consumidores | Ningún módulo de Documents / Audit / CAPA / Risk lo referencia |
| Diseñador visual | No existe |
| Versionado de esquema | No existe |
| Auditoría de dominio | No escribe `AuditLog` |
| Soft delete / concurrencia | No |
| Permiso dedicado | No (`TENANT.UPDATE` en UI; `Tenant.Manage` en API) |

### Archivos clave

- Dominio: `src/Compliance360.Domain/Enterprise/EnterpriseWorkspaceModels.cs`
- Aplicación: `src/Compliance360.Application/Enterprise/EnterpriseWorkspaceService.cs`
- API: `src/Compliance360.Web/Api/FoundationEndpoints.cs` → `/enterprise-workspaces`
- UI: `src/Compliance360.Web/wwwroot/app.js` → `renderEnterpriseWorkspace`
- RBAC: `PermissionCatalog` / `RoleCatalog` en `RbacCatalog.cs` / `RoleCatalog.cs`

---

## 2. Limitaciones

1. Marketing UX (“constructor de plantillas”) vs implementación (tracker de ítems).
2. `MetadataJson` opaco sin esquema tipado de formularios.
3. Misma API/policy para Regulatory, Training, Portals, etc.
4. Sin vínculo a instancias de auditoría/CAPA/documentos.
5. `Draft`/`Archived` en enum casi muertos (constructor fuerza `Active`).
6. Sin reopen/complete por fila en UI; solo “completar primero”.

---

## 3. Arquitectura existente relevante

| Capacidad | Cómo funciona hoy |
|-----------|-------------------|
| Frontend | SPA vanilla `app.js` + `routePermissions` / `canNavigate` |
| Backend | ASP.NET Minimal APIs + Application services + EF Core |
| Multitenant | `TenantId` en `TenantEntity` + claims JWT |
| RBAC | `PermissionCatalog` + policies `PermissionPolicies` |
| Auditoría | `AuditLog` append-only vía servicios de dominio |

---

## 4. Cambios necesarios (diseño elegido)

**No evolucionar** `EnterpriseWorkspaceItem.MetadataJson` como diseñador.

Crear bounded context **Form Templates**:

| Entidad | Responsabilidad |
|---------|-----------------|
| `FormTemplate` | Cabecera: nombre, código, categoría, tipo, estado, autor, soft-delete, versión publicada |
| `FormTemplateVersion` | Esquema JSON (campos + reglas), número de versión, changelog, publicación |

### Schema JSON (versión)

- `fields[]`: tipo, label, validaciones, orden, ancho, visibles, opciones
- `rules[]`: si campo/operador/valor → show/hide/require otro campo

### API nueva

`/api/v1/tenants/{tenantId}/form-templates`  
CRUD + `GET {id}` + `PUT` borrador + `POST publish` + `POST archive` + versiones

### Permisos nuevos

- `TEMPLATE.READ`
- `TEMPLATE.MANAGE`  
Asignados a **Tenant Administrator** (y visibles en policies).

### Frontend

Reemplazar `renderEnterpriseWorkspace` **solo** para `template-builder` por diseñador:

- Paleta izquierda  
- Canvas central  
- Propiedades derecha  
- Toolbar: guardar, publicar, vista previa (desktop/tablet/móvil)  
- Rule builder simple  

Los demás tipos enterprise (Regulatory, Training…) **conservan** el CRUD actual.

---

## 5. Impacto

| Área | Impacto |
|------|---------|
| BD | Tablas nuevas `form_templates`, `form_template_versions` |
| Backend | Nuevo servicio + endpoints; workspace intacto |
| Frontend | Nueva vista; menú sigue `#/template-builder` |
| Seguridad | Isolation por TenantId; policies TEMPLATE.* |
| APIs | Aditivas; no rompen `/enterprise-workspaces` |
| Permisos | Seed RBAC + rol Tenant Administrator |
| Riesgos | Migración de datos: ítems TPL antiguos quedan en workspace (compatibilidad); no se migran automáticamente a formularios |

---

## 6. Plan de migración

1. Desplegar tablas/API/permisos.
2. Cambiar UI Template Builder al diseñador.
3. (Opcional fase 2) Script one-shot que convierta ítems `TemplateBuilder` en plantillas vacías Draft.
4. Fase 2+: binding real a CAPA / Audits / Documents (hooks de consumo).
5. Fase 3+: DnD nativo completo, firma/QR/geolocalización runtime, comparación visual de versiones.

---

## 7. Cobertura funcional objetivo (MVP producción-usable)

| Capacidad del prompt | MVP |
|----------------------|-----|
| Info general plantilla | Sí |
| Diseñador de campos | Sí (tipos core + rest de tipado) |
| Propiedades por campo | Sí |
| Rule builder | Sí (show/hide/require) |
| Versionado publish | Sí |
| Previsualización | Sí (3 breakpoints) |
| Reutilización en otros módulos | Contrato de lectura API; binding UI pendiente |
| Soft delete | Sí |
| Concurrencia | `xmin` / RowVersion si PG lo soporta vía token |
| Auditoría | Sí (Created/Updated/Published) |
| DnD nativo perfecto | MVP: add/reorder buttons + canvas select (fase 2: HTML5 DnD) |

---

## Veredicto del análisis

El módulo actual **debe dejar de presentarse como constructor**. La implementación correcta es un **contexto Form Templates** independiente, reutilizando patrones de Documents/Audit del monorepo, sin romper Enterprise Workspaces.
