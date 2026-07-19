# 03 — Domain Model Analysis

Entidades derivadas **solo** de evidencia del Excel REGUTRACK y del código Compliance 360.  
Leyenda: **B** = aparece en el negocio (Excel) · **S** = existe en el sistema (código) · **BS** = ambos con semántica comparable · **≠** = nombre similar, semántica distinta.

---

## 1. Entidades del negocio (Excel)

| Entidad de negocio | Evidencia | Atributos clave observados |
|--------------------|-----------|----------------------------|
| **País regulatorio** | Col *País* = PA | Mercado de autorización |
| **Categoría de producto** | Insumos Médicos / Equipos Médicos | Clasificación comercial |
| **Marca** | REBSTOCK, STERIKING, OSSUR… | Branding del portafolio |
| **Producto regulado** | Nombre como aparece en CT + catálogo/código | Identidad ante autoridad |
| **Fabricante** | Nombre + país origen | Emisor de docs técnicos |
| **Distribuidor / Importador** | 4 Hospital / Multimed | Titular del registro comercial |
| **Entidad emisora** | MINSA / CSS | Autoridad sanitaria |
| **Criterio Técnico / Registro Sanitario** | Nº + fecha criterio/registro | Autorización |
| **Clase de riesgo** | A / B / C | Clase del dispositivo |
| **Tipo de proceso** | NUEVO / RENOVACION | Caso de trámite |
| **Expediente de registro** | Fechas armado/sometimiento/observación/aprobación | Caso |
| **Requisito documental del expediente** | Cols 18–39 | Ítem checklist |
| **Documento de fabricante** | Hoja DOCUMENTACION | Artefacto con vencimiento |
| **Licencia / permiso corporativo** | CTT LICENCIAS OP | Autorización de la compañía |
| **Compañía** | Multimed / 4 Hospitals | Persona jurídica |
| **Proveedor registrado (conteo/id)** | Col proveedores | Relación comercial cuantificada |
| **Oportunidad comercial** | Col Oportunidad \$ | Valor de negocio |
| **Línea** | Col Linea | Identificador interno de fila/caso |

**No aparecen como entidades operativas en el Excel** (no inventarlas como núcleo): CAPA, Training, Portal, Form Template Builder, Quality Indicator dashboard, managed internal audit program. Pueden ser útiles *después*, pero no son el dominio raíz REGUTRACK.

---

## 2. Entidades del sistema (código Domain)

| Carpetas Domain | Aggregates / entidades principales | ¿Mapa a REGUTRACK? |
|-----------------|------------------------------------|--------------------|
| `TechnicalSheets` | `Product`, `TechnicalSheet`, Ingredient, Nutrient, Certification | **≠** Producto médico; ficha es **nutricional** |
| `Suppliers` | `Supplier`, `SupplierDocument`, Evaluation, Alert | **Parcial** ≈ fabricante/docs, pero no expediente CT/RS |
| `Documents` | Document, Version, Approval, Type, Category | **Parcial** repositorio genérico |
| `Workflows` | Workflow, Instance, Step, Rule… | **Infra** reutilizable, no expediente |
| `FormTemplates` | FormTemplate, Version | **Infra** formularios; no caso regulatorio |
| `AuditManagement` | ManagedAudit, Finding… | Isla QMS |
| `CapaManagement` | Capa + actions | Isla QMS |
| `RiskManagement` | Risk + matrix | Isla QMS (≠ clase riesgo dispositivo A/B/C del Excel) |
| `QualityIndicators` | Indicator… | Isla QMS |
| `Enterprise` | `EnterpriseWorkspaceItem` | Tracker genérico (UI Regulatory) |
| `Identity` / `TenantManagement` | User, Role, Tenant, Company | **BS** plataforma |
| `Storage` / `Notifications` / `Audit` | StoredFile, Notification*, AuditLog | **Infra** transversal |
| `Reporting` | Report* | Transversal |

Evidencia `TechnicalSheetModels.cs`: `Product` solo tiene `Name`, `Sku`, `Description`, `IsActive`. `TechnicalSheet` añade **ingredients/nutrients** — modelo de **alimento/ingrediente**, no dispositivo médico.

---

## 3. Modelo de dominio objetivo (derivado del Excel)

### Aggregate Roots propuestos (diseño; no implementado)

```text
Company (Distribuidora)
 └── OperatingLicenseCase
Product (Dispositivo/Insumo)
 └── SanitaryRegistration (CT/RS) ── issuedBy Authority
 └── RegistrationDossier (Case) ── processType NEW|RENEWAL
      ├── DossierRequirementItem (checklist)
      ├── DossierMilestone (fechas)
      └── LinkedManufacturerDocument*
Manufacturer
 └── ManufacturerDocument (ISO, CLV, CE…) ── expiresAt
```

### Value Objects observados

- RiskClass {A,B,C}  
- Authority {MINSA, CSS}  
- CountryCode {PA}  
- CatalogCode  
- OpportunityAmount  
- ApostilleFormat  

### Eventos de dominio (implícitos en columnas)

`DocumentsRequestedFromFactory`, `DocumentsReceived`, `DossierAssembled`, `SubmittedToAuthority`, `ObservationReceived`, `RegistrationApproved`, `RegistrationExpired`, `RenewalStarted`, `ManufacturerDocumentExpired`, `OperatingLicenseExpired`

---

## 4. Matriz de coincidencia B ↔ S

| Concepto Excel | ¿Entity en C360? | Match |
|----------------|------------------|-------|
| Producto CT | `Product` | **≠** (falta marca, clase riesgo, autoridad, CT) |
| Fabricante | `Supplier` (posible) | Parcial |
| Distribuidor | `Company` / Tenant branding | Parcial |
| CT / RS | — | **No existe** |
| Expediente / tubería | — / Workspace item | **No existe** (workspace ≠ dossier) |
| Checklist docs 18–39 | Document types genéricos / FormTemplate | **No existe** como caso |
| Doc fabricante + vencimiento | `SupplierDocument` / `Document` | Parcial |
| Licencia operaciones | — | **No existe** |
| Clase riesgo A/B/C | `Risk` module | **≠** (riesgo ISO QMS ≠ clase dispositivo) |
| Análisis de riesgo (doc) | checklist col 33 | Documento, no módulo Risk |
| Workflow engine | `Workflow*` | Existe como motor, **sin definición RA** |
| Form Engine | `FormTemplate*` | Existe, **no consumido** por dossier |
| Auditoría interna | `ManagedAudit` | Existe, fuera del Excel |
| CAPA | `Capa` | Existe, fuera del Excel |

---

## 5. Conclusión de modelado

El dominio real es **Regulatory Product Authorization**.  
El dominio implementado es **Enterprise Quality & Compliance Toolkit**.  
Hay solapamiento útil en **infraestructura** (tenant, docs, archivos, workflow, notificaciones), no en el **aggregate raíz del negocio**.
