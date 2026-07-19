# REGULATORY_COVERAGE_MATRIX

**Contrato funcional:** `REGUTRACK 02JUN26 MG.xlsx`  
**Implementación:** Compliance 360 Regulatory Affairs BC  
**Actualizado:** 2026-07-14  
**Estado global (críticas):** ver sección Resumen

Leyenda Estado: `Implementado` | `Parcial` | `Pendiente` | `No aplica`

---

## Resumen ejecutivo de cobertura

| Hoja | Columnas / campos | Implementado | Parcial | Pendiente | No aplica* |
|------|-------------------|-------------|---------|-----------|------------|
| CTT REGISTROS / (2) / TUBERIA | 55 críticas (1–55) + Línea | 55 | 0 | 0 | 32 fechas → historial tipado |
| DOCUMENTACION | 11+ | 9 | 2 | 0 | — |
| CTT LICENCIAS OP | metadatos + checklist ~18 | 14 | 3 | 0 | FADDI externo (manual task) |
| Capacidades transversales | import / dash / alerts | 4 | 0 | 0 | — |

\*Columnas `Fecha de Actualización2..32` se modelan como **historial tipado de eventos** (una entidad), no 32 campos sueltos — equivalente funcional REGUTRACK.

---

## A. CTT REGISTROS / CTT REGISTROS (2) / CTT REGISTROS TUBERIA

Misma estructura columnar. Las tres hojas se representan como:
- Portafolio (`MedicalDeviceProduct` + `SanitaryRegistration`)
- Pipeline / Tubería (`RegistrationDossier` por estado)
- Caso activo = dossier

| # | Columna Excel | Entidad | Pantalla | API | Tabla | Workflow | Estado |
|---|---------------|---------|----------|-----|-------|----------|--------|
| 1 | País | MedicalDeviceProduct.CountryCode | Portafolio / Dossier | `/products` | medical_device_products | — | Implementado |
| 2 | Categoría | MedicalDeviceProduct.Category | Portafolio | `/products` | medical_device_products | — | Implementado |
| 3 | Marca | MedicalDeviceProduct.Brand | Portafolio | `/products` | medical_device_products | — | Implementado |
| 4 | Nombre producto CT | MedicalDeviceProduct.RegulatoryName | Portafolio / Dossier | `/products` | medical_device_products | — | Implementado |
| 5 | Descripción | MedicalDeviceProduct.Description | Portafolio | `/products` | medical_device_products | — | Implementado |
| 6 | Catálogo / Código | MedicalDeviceProduct.CatalogCode | Portafolio | `/products` | medical_device_products | — | Implementado |
| 7 | Fabricante y País Origen | ManufacturerProfile + Product.ManufacturerId | Fabricantes / Dossier | `/manufacturers` | manufacturer_profiles | — | Implementado |
| 8 | Distribuidor / Importador | MedicalDeviceProduct.DistributorName (+ CompanyId) | Portafolio | `/products` | medical_device_products | — | Implementado |
| 9 | Iniciativa | MedicalDeviceProduct.Initiative | Portafolio | `/products` | medical_device_products | — | Implementado |
| 10 | Ficha Técnica | MedicalDeviceProduct technical sheet artifact (ref+status+document) + requisito TECH_SHEET | Portafolio / Dossier | `/products` `/products/{id}/artifacts` `/requirements` | medical_device_products | Prep | **COVERED** |
| 11 | Entidad Emisora | RegulatoryAuthority + Dossier.AuthorityId | Dossier / Config | `/authorities` | regulatory_authorities | — | Implementado |
| 12 | CT / RS No. | SanitaryRegistration.RegistrationNumber | Registros | `/registrations` | sanitary_registrations | Approve | Implementado |
| 13 | Fecha Criterio / Registro | SanitaryRegistration.IssuedOn | Registros | `/registrations` | sanitary_registrations | Approve | Implementado |
| 14 | Proveedores Registrados | MedicalDeviceProduct.RegisteredSuppliersCount | Portafolio | `/products` | medical_device_products | — | Implementado |
| 15 | Clase de Riesgo | DeviceRiskClass (≠ RiskManagement) | Portafolio / Pipeline | `/products` | medical_device_products | — | Implementado |
| 16 | Tipo de Proceso | RegistrationProcessType | Dossier | `/dossiers` | registration_dossiers | Transitions | Implementado |
| 17 | Formulario | Product Form artifact (ref+status+document) / pack | Dossier / Product | `/products/{id}/artifacts` `/requirement-packs` | medical_device_products | Prep | **COVERED** |
| 18–39 | Checklist 22 docs | DossierRequirement (pack REGUTRACK-PA-DEFAULT) | Dossier Workspace | `/dossiers/{id}/requirements` | dossier_requirements | Gate submit | Implementado |
| 40 | Solicitud docs a fábrica | Milestone FactoryRequest + RequestedFromFactoryOn | Dossier Fechas | `/dossiers/{id}/dates` | dossier_milestones | WaitingManufacturerDocuments | Implementado |
| 41 | Fecha est. recepción | EstimatedReceptionOn | Dossier | dates | registration_dossiers | — | Implementado |
| 42 | Fecha máx recepción | MaximumReceptionOn + alerta | Dossier / Alertas | dates + `/alerts` | registration_dossiers | — | Implementado |
| 43 | Fecha recepción | ReceivedOn | Dossier | transition DocumentsReceived | registration_dossiers | — | Implementado |
| 44 | Alerta | RegulatoryAlertLog + evaluate 90/60/30/15/7/1/0 | Alertas | `/alerts/evaluate` | regulatory_alert_logs | — | Implementado |
| 45 | Fecha armado | AssembledOn | Dossier | ReadyForSubmission | registration_dossiers | — | Implementado |
| 46 | Fecha est. sometimiento | EstimatedSubmissionOn | Dossier | dates | registration_dossiers | — | Implementado |
| 47 | Fecha sometimiento | SubmittedOn | Pipeline Sometido | `/submit` | registration_dossiers | Submitted | Implementado |
| 48 | Fecha observación | ObservationReceivedOn + AuthorityObservation | Dossier Observaciones | `/observations` | authority_observations | Observed | Implementado |
| 49 | Fecha est. aprobación | EstimatedApprovalOn | Dossier | dates | registration_dossiers | — | Implementado |
| 50 | Fecha aprobación | ApprovedOn | Dossier / Registros | `/approve` | registration_dossiers | Approved | Implementado |
| 51 | Sales / Mkt input | SalesMarketingInput | Dossier / Portafolio | `/products` `/dossiers` | both | — | Implementado |
| 52 | Prioridad | Priority | Pipeline / Portafolio | `/dossiers` | registration_dossiers | — | Implementado |
| 53 | Oportunidad | OpportunityAmount | Dashboard / Portafolio | dashboard | products/dossiers | — | Implementado |
| 54 | Comentarios | Comments | Dossier | `/dossiers` | registration_dossiers | — | Implementado |
| 55 | Fecha vencimiento | SanitaryRegistration.ExpiresOn / TargetExpirationOn | Registros / Alertas | `/registrations` | sanitary_registrations | — | Implementado |
| 56–87 | Fechas actualización 1–32 | DossierHistoryEvent (lista) | Dossier Historial | `/dossiers/{id}` (history[]) | dossier_history_events | transitions | Implementado (equivalente tipado) |
| 88 | Línea | MedicalDeviceProduct.SourceLineNumber | Import / Portafolio | `/products` import | medical_device_products | — | Implementado |

---

## B. DOCUMENTACION (fabricante)

| # | Campo Excel | Entidad | Pantalla | API | Tabla | Estado |
|---|-------------|---------|----------|-----|-------|--------|
| 1 | FABRICANTE | ManufacturerProfile.LegalName | Fabricantes | `/manufacturers` | manufacturer_profiles | Implementado |
| 2 | ESTATUS | ManufacturerProfile.IsActive + cert Status | Fabricantes | `/manufacturers` | manufacturer_profiles | Implementado |
| 3 | PAIS | CountryCode | Fabricantes | `/manufacturers` | manufacturer_profiles | Implementado |
| 4 | DOCUMENTO | ManufacturerCertificate.Type | Fabricantes | `/manufacturer-certificates` | manufacturer_certificates | Implementado |
| 5 | FECHA VENCIMIENTO | ExpiresOn | Fabricantes / Alertas | certificates | manufacturer_certificates | Implementado |
| 6 | FORMATO | LegalFormat / Apostilled / Notarized | Fabricantes | certificates | manufacturer_certificates | Implementado |
| 7 | ESTATUS2 | Status derivado | Fabricantes | certificates | manufacturer_certificates | Parcial |
| 8 | FECHA SOLICITUD | ManufacturerCertificate.RequestedOn | Fabricantes | certificates | manufacturer_certificates | Implementado |
| 9 | TRAMITE-REQUISITO | Notes / link dossier | Fabricantes | certificates | — | Parcial |
| 10 | COMENTARIOS | Notes | Fabricantes | certificates | manufacturer_certificates | Implementado |
| 11 | SEGUIMIENTO | Notes / History | Fabricantes | — | — | Parcial |
| 18 | VIGENTE | Status Active/Expiring/Expired | Alertas | `/alerts` | manufacturer_certificates | Implementado |
| 19 | Criterios vinculados | CertificateUsageLink / Registration link | Fabricantes / Dossier | links | manufacturer_certificate_dossier_links | Parcial |

---

## C. CTT LICENCIAS OP

| Campo Excel | Entidad | Pantalla | API | Tabla | Estado |
|-------------|---------|----------|-----|-------|--------|
| Compañía (Multimed / 4 Hospitals) | OperatingLicense.CompanyName | Licencias | `/operating-licenses` | operating_licenses | Implementado |
| Fecha constitución / inicio ops | OperatingLicense.CompanyConstitutedOn / OperationsStartedOn (DateOnly) | Licencias | create + PUT company-dates + import | operating_licenses | **COVERED** |
| Tipo documento licencia | LicenseType | Licencias | create | operating_licenses | Implementado |
| Fecha expiración | ExpiresOn | Licencias / Alertas | list | operating_licenses | Implementado |
| Fechas armado/sometimiento/aprobación | LicenseMilestone + LicenseRenewalCase | Licencias | renewals | license_milestones | Parcial |
| Checklist (cédulas, regente, registro público, aviso ops, F&D, timbres, tasa, DJ, croquis, listado DM, contrato 3ero, acondicionamiento, licencia DM, solvencia, catálogo MINSA, oferente…) | LicenseOpRequirementCatalog → LicenseRequirement | Licencias caso renovación | renewals | license_requirements | Implementado (siembra en StartLicenseRenewal) |
| Estatus / comentarios | Status + Comments | Licencias | — | operating_licenses | Implementado |
| Actualizar Panamá Digital / FADDI | ManualPlatformUpdate task | Licencias | renewals | license_renewal_cases | Implementado (manual, sin integración) |

---

## D. Capacidades transversales Excel → Sistema

| Capacidad | Excel | Sistema | Estado |
|-----------|-------|---------|--------|
| Tubería visual | Hoja TUBERIA | Pipeline kanban | Implementado |
| Portafolio vivo | REGISTROS | Portafolio | Implementado |
| Importar Excel | Archivo | Stage JSON + `POST /imports/xlsx` (ClosedXML) → validate → simulate → commit | Implementado |
| Dashboard vencimientos / \$ / cuellos | implícito | KPIs mes / stuck / bottleneck / \$ por estado | Implementado |
| Alertas 90–1–expirado | col Alerta + fórmulas humanas | Alert engine CT/RS + cert + licencia + max recepción | Implementado |
| Case-centric UI | 1 fila = 1 caso | Dossier workspace | Implementado |
| Requirement Pack Designer | col Formulario + checklist | Compliance Studio → packs | Parcial (pack sí; Studio como diseñador aún desconectado) |

---

## E. Backlog restante (solo gaps vs Excel)

1. **Rollback / reporte post-import** más formal (hoy: job status + validationReportJson)
2. **CompanyMetadata** (fecha constitución / inicio ops) en Licencias OP
3. **Studio = Requirement Pack Designer** bridge (pack API existe; Studio aún no es el diseñador oficial)
4. **E2E browser cert** + migración histórica completa del Excel real
5. **Documents** link duro Producto↔Expediente↔Requisito (sin huérfanos) en UI
6. Estados pipeline Excel `Vencido` / `Renovación` como columnas kanban dedicadas (hoy vía registrations + StartRenewal)

---

## F. Criterio de cierre del reemplazo

El Excel puede cerrarse cuando:
- Sección A críticas 1–55 = Implementado (sin Pendiente)
- 56–87 = historial tipado operativo
- DOCUMENTACION = sin Pendiente críticos
- LICENCIAS OP checklist = Implementado
- Importador XLSX = Implementado y usado para migración histórica
- E2E browser certificado
