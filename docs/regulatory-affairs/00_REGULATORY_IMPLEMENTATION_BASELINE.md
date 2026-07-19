# 00 — Regulatory Implementation Baseline

**Fecha:** 2026-07-13  
**Alcance:** MVP vertical slice *Regulatory Dossier*  
**Fuente de verdad funcional:** `REGUTRACK 02JUN26 MG.xlsx` + `docs/master-audit/*`

---

## 1. Arquitectura actual (reutilizable)

| Capa | Patrón a clonar |
|------|-----------------|
| Domain | `TenantEntity` + `Guard` + private setters (`FormTemplates` / CAPA) |
| Application | `I*Service` + `I*Repository` + Commands/DTOs + `Result<T>` |
| Infrastructure | `Ef*Repository` + `Configure*` en `Compliance360DbContext` + migración EF |
| Web | `Map*` en `FoundationEndpoints` + `ApiContext` + `PermissionPolicies` |
| Frontend | Hash route en `app.js` + script dedicado (estilo Compliance Studio) |
| DI | `AddScoped` en `DependencyInjection.AddInfrastructure` |

**Confirmación expresa:** el expediente regulatorio **NO** usará `EnterpriseWorkspaceItem`. La ruta `#/regulatory` se redirigirá a la RA Console real. El workspace type Regulatory queda marcado como **legacy**.

---

## 2. Componentes reutilizables (no tocar semántica)

- Identity / RBAC / MFA  
- Tenant / Company  
- Storage (`StoredFileId` en requirements/certificates)  
- Documents (link opcional `CurrentDocumentId`)  
- Notifications (alertas de vencimiento — fase MVP básica vía dashboard + registros)  
- AuditLog + `IAuditRepository`  
- Workflow Engine (attach opcional; estado de dossier es state machine propia en el aggregate)  
- Form Engine (pack de requisitos es entidad propia `RegulatoryRequirementPack`; no se convierte FormTemplate en aggregate raíz)

---

## 3. Convenciones técnicas

- Tablas `snake_case` schema `compliance360`  
- Enums como `string` en EF  
- Soft delete con `IsDeleted` donde aplique  
- Sin RowVersion global (el monorepo no lo usa; se documenta concurrencia vía UpdatedAt)  
- Permisos `REGULATORY.*` en `PermissionCatalog`  
- Auditoría con nuevos `AuditAction` + categoría `RegulatoryAffairs`

---

## 4. Dependencias que no se modifican estructuralmente

- CAPA, Risk, AuditManagement, TechnicalSheets (nutricional), EnterpriseWorkspace  
- No integración FADDI / Panamá Digital  

---

## 5. Riesgos de implementación

| Riesgo | Mitigación |
|--------|------------|
| Scope creep licencias + importador | MVP incluye licencias CRUD + importador asistido staging |
| Supplier vs Manufacturer | `ManufacturerProfile` con `SupplierId` opcional |
| Migración RBAC en tenants existentes | constantes en catalog; seeding usa RoleCatalog al provisionar |

---

## 6. Plan incremental (este delivery)

1. Domain RegulatoryAffairs  
2. Application + Repository  
3. EF + Migration  
4. Permissions + API  
5. RA Console UI  
6. Importador staging  
7. Tests + docs  

---

## 7. Archivos creados (previstos)

```
docs/regulatory-affairs/00_…13_*.md
Domain/RegulatoryAffairs/*.cs
Application/RegulatoryAffairs/*.cs
Infrastructure/RegulatoryAffairs/*.cs
Infrastructure/Persistence/Migrations/*AddRegulatoryAffairs*
wwwroot/regulatory-affairs.js
wwwroot/regulatory-affairs.css
tests/.../RegulatoryAffairsTests.cs
```

## 8. Archivos modificados (previstos)

```
Compliance360DbContext.cs
DependencyInjection.cs
FoundationEndpoints.cs
ApiContracts.cs
PermissionPolicies.cs
RbacCatalog.cs
RoleCatalog.cs
AuditLog.cs (enums)
app.js (ruta #/regulatory)
index.html (script/css)
```

---

## 9. No-go explícitos

- ❌ EnterpriseWorkspaceItem como dossier  
- ❌ RiskManagement para RiskClass A/B/C  
- ❌ CAPA / ManagedAudit como registro sanitario  
- ❌ FormTemplate como aggregate raíz del caso  
- ❌ TechnicalSheet nutricional como ficha de dispositivo  
