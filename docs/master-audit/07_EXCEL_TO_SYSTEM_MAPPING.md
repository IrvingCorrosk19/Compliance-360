# 07 — Excel to System Mapping

Mapeo columna a columna de las hojas críticas de `REGUTRACK 02JUN26 MG.xlsx` hacia Compliance 360.  
**NO MAP** = sin entidad/pantalla/API/servicio/modelo/workflow equivalente.

---

## A. Hojas `CTT REGISTROS`, `CTT REGISTROS (2)`, `CTT REGISTROS TUBERIA`

Estructura de columnas equivalente (~88 cols). Filas de datos ≈ 187–192.

| Col | Campo Excel | Entidad | Pantalla | Tabla/DbSet | API | Servicio | Modelo | Workflow | Estado |
|-----|-------------|---------|----------|-------------|-----|----------|--------|----------|--------|
| 1 | País | — | — | — | — | — | — | — | **NO MAP** |
| 2 | Categoría | — | — | — | — | — | — | — | **NO MAP** |
| 3 | Marca | — | — | — | — | — | — | — | **NO MAP** |
| 4 | Nombre producto CT | Product.Name | technical-sheets | Products | technical-sheets | TechnicalSheetService | Product | — | **PARCIAL** (sin semántica CT) |
| 5 | Descripción | Product.Description | idem | Products | idem | idem | Product | — | PARCIAL |
| 6 | Catálogo / Código | Product.Sku | idem | Products | idem | idem | Product | — | PARCIAL |
| 7 | Fabricante y país | Supplier (+country?) | suppliers | Suppliers | suppliers | SupplierManagementService | Supplier | — | PARCIAL |
| 8 | Distribuidor / Importador | Company / Tenant | tenant-admin | Companies/Tenants | tenants | TenantManagementService | Company | — | PARCIAL |
| 9 | Iniciativa | — | — | — | — | — | — | — | **NO MAP** |
| 10 | Ficha Técnica | TechnicalSheet | technical-sheets | TechnicalSheets | technical-sheets | TechnicalSheetService | TechnicalSheet | approve propio | **PARCIAL / ≠** (nutrientes) |
| 11 | Entidad Emisora | — | — | — | — | — | — | — | **NO MAP** |
| 12 | CT / Registro Sanitario No. | — | — | — | — | — | — | — | **NO MAP** |
| 13 | Fecha Criterio / Registro | — | — | — | — | — | — | — | **NO MAP** |
| 14 | Proveedores Registrados | — / Supplier count | — | — | — | — | — | — | **NO MAP** |
| 15 | Clase de Riesgo A/B/C | — (≠ Risk) | — | — | — | — | — | — | **NO MAP** |
| 16 | Tipo de Proceso | — | — | — | — | — | — | — | **NO MAP** |
| 17 | Formulario | FormTemplate? | template-builder | FormTemplates | form-templates | FormTemplateService | FormTemplate | — | PARCIAL (no ligado a fila) |
| 18–39 | Checklist docs (22 ítems) | Document? | documents | Documents | documents | DocumentManagementService | Document | — | **NO MAP** caso; solo DMS genérico |
| 40 | Solicitud docs a fábrica | — | — | — | — | — | — | — | **NO MAP** |
| 41 | Fecha est. recepción | — | — | — | — | — | — | — | **NO MAP** |
| 42 | Fecha máx recepción | — | — | — | — | — | — | — | **NO MAP** |
| 43 | Fecha recepción | — | — | — | — | — | — | — | **NO MAP** |
| 44 | Alerta | Notifications / SupplierAlert | — | — | — | — | — | — | PARCIAL infraestructura |
| 45 | Fecha armado expediente | — | — | — | — | — | — | — | **NO MAP** |
| 46 | Fecha est. sometimiento | — | — | — | — | — | — | — | **NO MAP** |
| 47 | Fecha sometimiento | — | — | — | — | — | — | — | **NO MAP** |
| 48 | Fecha observación | — | — | — | — | — | — | — | **NO MAP** |
| 49 | Fecha est. aprobación | — | — | — | — | — | — | — | **NO MAP** |
| 50 | Fecha aprobación | — | — | — | — | — | — | — | **NO MAP** |
| 51 | Sales / Mkt input | — | — | — | — | — | — | — | **NO MAP** |
| 52 | Prioridad | — | — | — | — | — | — | — | **NO MAP** |
| 53 | Oportunidad | — | — | — | — | — | — | — | **NO MAP** |
| 54 | Comentarios | Workspace description? | regulatory | EnterpriseWorkspaceItems | enterprise-workspaces | EnterpriseWorkspaceService | EnterpriseWorkspaceItem | — | PARCIAL débil |
| 55 | Fecha vencimiento | dueAtUtc / cert expires | regulatory / sheets | — | — | — | — | — | PARCIAL débil |
| 56–87 | Fechas actualización 1–32 | AuditLog? / History | audit-trail | AuditLogs | audit | AuditService | AuditLog | — | PARCIAL (no tipado a CT) |
| 88 | Línea | — | — | — | — | — | — | — | **NO MAP** |

### Resumen hoja registros

| Categoría | Conteo aprox. |
|-----------|---------------|
| Columnas estructurales negocio | ~55 significativas (1–55 + línea) |
| NO MAP estricto | ~40+ |
| PARCIAL | ~10 |
| MAP completo semántico | **0** |

---

## B. Hoja `DOCUMENTACION`

| Col | Campo | Mapeo C360 | Estado |
|-----|-------|------------|--------|
| Fabricante | Supplier.Name | PARCIAL |
| Estatus ACTIVO/INACTIVO | SupplierStatus / doc status | PARCIAL |
| País fabricante | — / free text | PARCIAL débil |
| Documento (CLV, CE, ISO13485) | SupplierDocument.Type enum limitado | PARCIAL — tipos Excel no 1:1 garantizados |
| Fecha vencimiento | SupplierDocument / Document | PARCIAL |
| Formato apostillado | — | **NO MAP** |
| Fecha solicitud | — | **NO MAP** |
| Trámite-requisito | — | **NO MAP** |
| Comentarios / Seguimiento | free text fields | PARCIAL |
| Vigente | alertas | PARCIAL |
| Vínculo criterios CT/RS | — | **NO MAP** |

---

## C. Hoja `CTT LICENCIAS OP`

| Campo | Mapeo C360 | Estado |
|-------|------------|--------|
| Compañía Multimed / 4 Hospitals | Company | PARCIAL |
| Fecha constitución / inicio ops | — | **NO MAP** |
| Tipo documento licencia | — | **NO MAP** |
| Fecha expiración | — | **NO MAP** |
| Fechas armado/sometimiento/aprobación | — | **NO MAP** |
| Checklist (cédulas, timbres, croquis, listado DM, contratos…) | Documents genéricos | **NO MAP** caso |
| Estatus / comentarios Panamá Digital / FADDI | — | **NO MAP** |
| Integración HTTP FADDI | — | **NO MAP** |

---

## D. Lectura gerencial del mapeo

El Excel está **estructurado como un Case File relacional aplanado**. Compliance 360 está **estructurado como módulos de compliance desacoplados**.  
Porcentaje de columnas núcleo con MAP semántico completo ≈ **0%**.  
Infraestructura reusable (storage, users, files) no cuenta como mapeo de columna de negocio.
