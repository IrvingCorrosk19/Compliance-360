# TEMPLATE_BUILDER_IMPLEMENTATION.md

**Fecha:** 2026-07-14  
**Producto UI:** Compliance Studio — Form Engine  
**Relacionado:** `docs/TEMPLATE_BUILDER_ANALYSIS.md`, `docs/TEMPLATE_BUILDER_PRODUCT_ARCHITECTURE.md`

---

## Entrega actual (producto comercial)

### Backend (Form Templates BC)
- Dominio `FormTemplate` / `FormTemplateVersion` (Draft → Published → Archived, soft delete)
- API `/api/v1/tenants/{tenantId}/form-templates`
  - List / Get / Create / Header / Draft / Publish / Archive / SoftDelete
  - `GET /published?kind=` — consumo por módulos
  - `POST /{id}/duplicate`
  - `POST /{id}/versions/{versionId}/restore`
- Permisos `TEMPLATE.READ` / `TEMPLATE.MANAGE` (Tenant Administrator)
- Auditoría: `FormTemplateCreated` / `Updated` / `Published`
- Multitenancy por `TenantId` + claims JWT

### Frontend — Compliance Studio
- `wwwroot/form-builder-studio.css` — chrome independiente (oscuro/claro)
- `wwwroot/form-template-builder.js` — estudio full-bleed
- Overlay `#compliance-studio-root` (no rompe layout de otros módulos)
- Home: starters (ISO, CAPA, Checklist, Riesgo, Incidente, Inspección, Evaluación, Regulatorio, Docs)
- Diseñador: toolbar · paleta 50+ · canvas DnD · inspector · footer · preview
- Schema v2 (`components`, `rules` AST, `expressions`, `workflow`) con migración v1 `fields[]`
- Undo/Redo, autosave, atajos, zoom, multi-select, clipboard, versiones/restaurar, duplicar, publicar
- Helper integración: `window.ComplianceStudio.getPublishedTemplates(kind)`

### Integración módulos
| Módulo | Cómo consume |
|--------|----------------|
| Audits / CAPA / Risk / Inspection / Documents / Training / Portal | `GET .../form-templates/published?kind=` + parser `ComplianceStudio.parseSchema` |
| Runtimes de submit | Futuro: `FormSubmission` (fuera de esta entrega) |

### Pruebas
- `FormTemplateBuilderTests`
  - create → publish → version bump + aislamiento tenant
  - duplicate + restore + SearchPublished por kind

### Build
- `dotnet build` Web + Tests

---

## Qué NO se tocó (aislamiento)
Audits, Documents, CAPA, Risk, Training, Portal, Workspace genérico, Users, Roles, Permissions catalogs distintos de `TEMPLATE.*`.

Enterprise Workspace `Type=TemplateBuilder` sigue existiendo como legado; la ruta `#/template-builder` **no** lo usa.

---

## Operación
1. Aplicar migraciones (`form_templates`).
2. Re-login del Tenant Administrator para claims `TEMPLATE.*`.
3. Abrir `#/template-builder` → Compliance Studio.
