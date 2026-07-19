# 06 — Process Traceability Matrix

Trazabilidad **proceso de negocio → capacidad de sistema**.  
“—” = sin representación encontrada en código/API/UI.

---

## Procesos → Artefactos sistema

| ID | Proceso | UI / Ruta | API | Servicio | Modelo Domain | Tabla/DbSet (tema) | Workflow |
|----|---------|-----------|-----|----------|---------------|--------------------|----------|
| P1 | Portafolio CT/RS | `#/technical-sheets` (Product) / `#/regulatory` | `/technical-sheets`, `/enterprise-workspaces` | TechnicalSheetService, EnterpriseWorkspaceService | Product ≠ CT; WorkspaceItem | Products, EnterpriseWorkspaceItems | — |
| P2 | Tubería fechas sometimiento | `#/regulatory` (due único) | enterprise-workspaces | EnterpriseWorkspaceService | EnterpriseWorkspaceItem.DueAtUtc | EnterpriseWorkspaceItems | — |
| P3 | Checklist expediente | Documents / Studio | `/documents`, `/form-templates` | DocumentManagementService, FormTemplateService | Document, FormTemplate | Documents, FormTemplates | — no instancia por dossier |
| P4 | Docs a fábrica + alertas | `#/suppliers` | `/suppliers` | SupplierManagementService | SupplierDocument, SupplierExpirationAlert | Supplier* | — |
| P5 | Vigencia docs fabricante | `#/suppliers`, `#/documents` | suppliers, documents | Supplier/Document services | SupplierDocument / Document | — | — |
| P6 | Licencias operación | — | — | — | — | — | — |
| Q1 | Auditoría interna | `#/audits` | `/audit-management` | AuditManagementService | ManagedAudit… | ManagedAudits… | Attach opcional |
| Q2 | CAPA | `#/capa` | `/capas` | CapaManagementService | Capa… | Capas | AttachWorkflow |
| Q3 | Riesgo org | `#/risks` | `/risks` | RiskManagementService | Risk… | Risks | AttachWorkflow |
| Q4 | Training / portals | `#/training`, portals | enterprise-workspaces | EnterpriseWorkspaceService | WorkspaceItem | — | — |
| F1 | Form Builder | `#/template-builder` | `/form-templates` | FormTemplateService | FormTemplate | FormTemplates | Rules UI internas |
| X1 | Motor workflow | (admin APIs) | `/workflows` | WorkflowEngineService | Workflow* | Workflows | **Motor genérico** |
| X2 | Audit trail | `#/audit-trail` | audit endpoints | AuditService | AuditLog | AuditLogs | — |

---

## Subprocesos REGUTRACK detallados

| Subproceso Excel | Representación C360 | Gap |
|------------------|---------------------|-----|
| Clasificar riesgo A/B/C | — / Risk module distinto | Alto |
| Elegir autoridad MINSA/CSS | — | Alto |
| Solicitar docs fábrica | Comentarios supplier / manual | Alto |
| Control fecha máx recepción | — | Alto |
| Armado expediente | — | Alto |
| Sometimiento | — | Alto |
| Observación autoridad | — | Alto |
| Aprobación + número CT | — | Alto |
| Tracking vencimiento CT | due genérico / alerts supplier | Alto |
| Renovación | — | Alto |
| Actualizar oferente Panamá Digital | Comentarios Excel; sin integración | Alto |
| Solicitud FADDI digital | Comentario licencia; sin integración | Medio (integración externa) |

---

## Conclusión de trazabilidad de proceso

La trazabilidad **se corta** en el paso “caso regulatorio”. Existe infraestructura alrededor; **falta el objeto de negocio que atraviesa P1–P6**.
