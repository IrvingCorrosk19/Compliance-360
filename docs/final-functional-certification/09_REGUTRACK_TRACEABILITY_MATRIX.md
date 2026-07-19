# 09 — REGUTRACK Traceability Matrix

**Contract:** `REGUTRACK 02JUN26 MG.xlsx`  
**Inventory source:** `docs/regulatory-affairs/REGULATORY_COVERAGE_MATRIX.md`  
**Header metadata:** `evidence/regutrack_decomposition.json`  
**Lab tenant:** `82af3877-2786-4d39-bce8-c981101c771d`  
**Date:** 2026-07-14  
**Global execution status:** NOT EXECUTED

---

## How to use

1. Execute test case ID listed per row.
2. Attach evidence under `evidence/functional/traceability/{TestCase}/`.
3. Update **Status** to `PASS`, `FAIL`, or `WAIVED` (PO approval required for WAIVED).
4. Initial status for all rows: **NOT EXECUTED**.

**Column legend:** Criticality = `Critical` | `High` | `Medium` per business impact.

---

## Section A — CTT REGISTROS / CTT REGISTROS (2) / CTT REGISTROS TUBERIA (columns 1–55)

| Excel Sheet | Excel Column | Business Meaning | Criticality | Domain Entity | Property/Rule | DB Table/Column | API | Screen | Role | Workflow | Test Case | Evidence | Status |
|-------------|--------------|------------------|-------------|---------------|---------------|-----------------|-----|--------|------|----------|-----------|----------|--------|
| CTT REGISTROS | País | Jurisdiction of commercialization | Critical | MedicalDeviceProduct | CountryCode required PA | medical_device_products.country_code | GET/POST /products | Portafolio | Specialist | — | RA-CTT-001 | — | NOT EXECUTED |
| CTT REGISTROS | Categoría | Product category (insumos, equipo, etc.) | Critical | MedicalDeviceProduct | Category enum/text | medical_device_products.category | GET/POST /products | Portafolio | Specialist | — | RA-CTT-002 | — | NOT EXECUTED |
| CTT REGISTROS | Marca | Commercial brand | Critical | MedicalDeviceProduct | Brand max 220 | medical_device_products.brand | GET/POST /products | Portafolio | Specialist | — | RA-CTT-003 | — | NOT EXECUTED |
| CTT REGISTROS | Nombre del Producto Como aparece en el CT | Regulatory trade name on CT/RS | Critical | MedicalDeviceProduct | RegulatoryName | medical_device_products.regulatory_name | GET/POST /products | Portafolio / Expediente | Specialist | — | RA-CTT-004 | — | NOT EXECUTED |
| CTT REGISTROS | Descripción | Product description variants | Critical | MedicalDeviceProduct | Description nullable | medical_device_products.description | GET/POST /products | Portafolio | Specialist | — | RA-CTT-005 | — | NOT EXECUTED |
| CTT REGISTROS | Catálogo / Código de Producto | SKU / catalog codes | Critical | MedicalDeviceProduct | CatalogCode multi-line OK | medical_device_products.catalog_code | GET/POST /products | Portafolio | Specialist | — | RA-CTT-006 | — | NOT EXECUTED |
| CTT REGISTROS | Fabricante y País de Origen | Manufacturer legal name + origin country | Critical | ManufacturerProfile + Product | ManufacturerId FK | manufacturer_profiles + medical_device_products.manufacturer_id | GET/POST /manufacturers /products | Fabricantes / Expediente | Specialist | — | RA-CTT-007 | — | NOT EXECUTED |
| CTT REGISTROS | Distribuidor / Importador | Local distributor entity | Critical | MedicalDeviceProduct | DistributorName + CompanyId | medical_device_products.distributor_name | GET/POST /products | Portafolio | Specialist | — | RA-CTT-008 | — | NOT EXECUTED |
| CTT REGISTROS | Iniciativa | Business initiative tag | Critical | MedicalDeviceProduct | Initiative | medical_device_products.initiative | GET/POST /products | Portafolio | Specialist | — | RA-CTT-009 | — | NOT EXECUTED |
| CTT REGISTROS | Ficha Tecnica | Technical sheet reference number | Critical | MedicalDeviceProduct + DossierRequirement | TechnicalSheetReference + TECH_SHEET req | medical_device_products.technical_sheet_reference | GET/POST /products /dossiers/{id}/requirements | Portafolio / Expediente | Specialist | Prep | RA-CTT-010 | — | NOT EXECUTED |
| CTT REGISTROS | Entidad Emisora | Issuing authority MINSA/CSS | Critical | RegulatoryAuthority + Dossier | AuthorityId required before submit | regulatory_authorities + registration_dossiers.authority_id | GET /authorities POST /dossiers | Expediente / Config | Specialist / Admin | Prep | RA-CTT-011 | — | NOT EXECUTED |
| CTT REGISTROS | Criterio Técnico/ Registro Sanitario No. | CT/RS registration number | Critical | SanitaryRegistration | RegistrationNumber unique per tenant | sanitary_registrations.registration_number | GET /registrations POST /dossiers/{id}/approve | Registros CT/RS | Manager | External Decision | RA-CTT-012 | — | NOT EXECUTED |
| CTT REGISTROS | Fecha Criterio /Registro | Issue date of CT/RS | Critical | SanitaryRegistration | IssuedOn ≤ ExpiresOn | sanitary_registrations.issued_on | POST /dossiers/{id}/approve | Registros / Expediente | Manager | External Decision | RA-CTT-013 | — | NOT EXECUTED |
| CTT REGISTROS | Proveedores Registrados | Count of registered suppliers | Critical | MedicalDeviceProduct | RegisteredSuppliersCount int | medical_device_products.registered_suppliers_count | GET/POST /products | Portafolio | Specialist | — | RA-CTT-014 | — | NOT EXECUTED |
| CTT REGISTROS | Clase de Riesgo | Device risk class A/B/C/D | Critical | MedicalDeviceProduct | DeviceRiskClass ≠ QMS Risk | medical_device_products.device_risk_class | GET/POST /products | Portafolio / Pipeline | Specialist | — | RA-CTT-015 | — | NOT EXECUTED |
| CTT REGISTROS | Tipo de Proceso | New reg vs renewal vs amendment | Critical | RegistrationDossier | RegistrationProcessType | registration_dossiers.process_type | GET/POST /dossiers | Expediente / Pipeline | Specialist | Prep | RA-CTT-016 | — | NOT EXECUTED |
| CTT REGISTROS | Formulario | Form reference / requirement pack code | Critical | Product + Pack | FormReference + REGUTRACK-PA-DEFAULT | products + requirement_packs | GET /requirement-packs POST /dossiers | Expediente | Specialist / Admin | Prep | RA-CTT-017 | — | NOT EXECUTED |
| CTT REGISTROS | Copia simple de la cedula o pasaporte del representante legal | Legal rep ID copy | Critical | DossierRequirement | LEGAL_ID critical Accepted gate | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist / Reviewer | Prep→Review | RA-CTT-018 | — | NOT EXECUTED |
| CTT REGISTROS | Copia simple de la Licencia de operaciones | Operating license copy | Critical | DossierRequirement | OPS_LICENSE critical | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist / Reviewer | Prep→Review | RA-CTT-019 | — | NOT EXECUTED |
| CTT REGISTROS | Copia Simple del certificado del registro publico | Public registry certificate | Critical | DossierRequirement | PUBLIC_REGISTRY critical | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist / Reviewer | Prep→Review | RA-CTT-020 | — | NOT EXECUTED |
| CTT REGISTROS | Copia simple del certificado de oferente | Offeror certificate | Critical | DossierRequirement | OFFEROR_CERT critical | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist / Reviewer | Prep→Review | RA-CTT-021 | — | NOT EXECUTED |
| CTT REGISTROS | Copia de la ficha Tecnica | Technical sheet document | Critical | DossierRequirement | TECH_SHEET critical | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist / Reviewer | Prep→Review | RA-CTT-022 | — | NOT EXECUTED |
| CTT REGISTROS | Literatura Técnica del Dispositivo Médico | Device literature | High | DossierRequirement | DEVICE_LITERATURE | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist / Reviewer | Prep→Review | RA-CTT-023 | — | NOT EXECUTED |
| CTT REGISTROS | Instructivo de Uso / Inserto | IFU / insert | Critical | DossierRequirement | IFU critical | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist / Reviewer | Prep→Review | RA-CTT-024 | — | NOT EXECUTED |
| CTT REGISTROS | Carta de compromiso del fabricante | Manufacturer commitment letter | Critical | DossierRequirement | MFG_COMMITMENT critical | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist / Reviewer | Prep→Review | RA-CTT-025 | — | NOT EXECUTED |
| CTT REGISTROS | ISO | ISO 13485 certificate | Critical | DossierRequirement | ISO critical | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist / Reviewer | Prep→Review | RA-CTT-026 | — | NOT EXECUTED |
| CTT REGISTROS | Cert. De Libre Venta (CLV ) o FDA | CLV or FDA cert | Critical | DossierRequirement | CLV_FDA critical | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist / Reviewer | Prep→Review | RA-CTT-027 | — | NOT EXECUTED |
| CTT REGISTROS | Fotos | Product photos | Medium | DossierRequirement | PHOTOS optional | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist | Prep | RA-CTT-028 | — | NOT EXECUTED |
| CTT REGISTROS | Etiquetas del producto | Product labels | Critical | DossierRequirement | LABELS critical | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist / Reviewer | Prep→Review | RA-CTT-029 | — | NOT EXECUTED |
| CTT REGISTROS | Metodo de esterilización | Sterilization method | Medium | DossierRequirement | STERILIZATION | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist | Prep | RA-CTT-030 | — | NOT EXECUTED |
| CTT REGISTROS | Resumen de estudios o ensayos clinicos | Clinical summary | Medium | DossierRequirement | CLINICAL | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist | Prep | RA-CTT-031 | — | NOT EXECUTED |
| CTT REGISTROS | Descripcion de manufactura y empaque | Manufacturing / packaging desc | Medium | DossierRequirement | MFG_PACKAGING | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist | Prep | RA-CTT-032 | — | NOT EXECUTED |
| CTT REGISTROS | Análisis de Riesgo | Risk analysis doc | Medium | DossierRequirement | RISK_ANALYSIS | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist | Prep | RA-CTT-033 | — | NOT EXECUTED |
| CTT REGISTROS | Protocolo de trazabilidad | Traceability protocol | Medium | DossierRequirement | TRACEABILITY | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist | Prep | RA-CTT-034 | — | NOT EXECUTED |
| CTT REGISTROS | Muestras | Samples | Medium | DossierRequirement | SAMPLES | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist | Prep | RA-CTT-035 | — | NOT EXECUTED |
| CTT REGISTROS | Manual de Operación y/o Matenimiento | O&M manual | Medium | DossierRequirement | OPS_MANUAL | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist | Prep | RA-CTT-036 | — | NOT EXECUTED |
| CTT REGISTROS | Certificación de Soporte Técnico Local | Local tech support cert | Medium | DossierRequirement | LOCAL_SUPPORT | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist | Prep | RA-CTT-037 | — | NOT EXECUTED |
| CTT REGISTROS | Datos de Almacenamiento y Transporte | Storage / transport data | Medium | DossierRequirement | STORAGE_TRANSPORT | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist | Prep | RA-CTT-038 | — | NOT EXECUTED |
| CTT REGISTROS | Listado de Accesorios, Repuestos y Consumibles | Accessories list | Medium | DossierRequirement | ACCESSORIES | dossier_requirements | PUT /dossiers/{id}/requirements/{rid} | Expediente | Specialist | Prep | RA-CTT-039 | — | NOT EXECUTED |
| CTT REGISTROS | Solicitud de documentos a Fabrica | Factory document request sent | Critical | RegistrationDossier / Milestone | RequestedFromFactoryOn + status WaitingManufacturerDocuments | registration_dossiers + dossier_milestones | POST /dossiers/{id}/transition | Expediente | Specialist | Prep | RA-CTT-040 | — | NOT EXECUTED |
| CTT REGISTROS | Fecha estimada de Recepción de Documentos | Estimated doc receipt | Critical | RegistrationDossier | EstimatedReceptionOn | registration_dossiers.estimated_reception_on | PUT /dossiers/{id}/dates | Expediente | Specialist | Prep | RA-CTT-041 | — | NOT EXECUTED |
| CTT REGISTROS | Fecha máxima para recepción de documentos | Latest acceptable receipt | Critical | RegistrationDossier | MaximumReceptionOn triggers alert | registration_dossiers.maximum_reception_on | PUT dates + GET /alerts/evaluate | Expediente / Alertas | Specialist | Prep | RA-CTT-042 | — | NOT EXECUTED |
| CTT REGISTROS | Fecha de recepción de documentos | Actual docs received date | Critical | RegistrationDossier | ReceivedOn on DocumentsReceived | registration_dossiers.received_on | POST /dossiers/{id}/transition | Expediente | Specialist | Prep | RA-CTT-043 | — | NOT EXECUTED |
| CTT REGISTROS | Alerta | Human-readable alert flag | Critical | RegulatoryAlertLog | Engine 90/60/30/15/7/1/0 + max reception | regulatory_alert_logs | GET /alerts/evaluate | Alertas / Dashboard | Viewer+ | — | RA-CTT-044 | — | NOT EXECUTED |
| CTT REGISTROS | Fecha de Armado de Expediente | Dossier assembly complete | Critical | RegistrationDossier | AssembledOn | registration_dossiers.assembled_on | POST transition Assembling→Ready | Expediente | Specialist | Prep | RA-CTT-045 | — | NOT EXECUTED |
| CTT REGISTROS | Fecha Estimada de Sometimiento | Estimated submission date | Critical | RegistrationDossier | EstimatedSubmissionOn | registration_dossiers.estimated_submission_on | PUT /dossiers/{id}/dates | Expediente | Specialist | Prep | RA-CTT-046 | — | NOT EXECUTED |
| CTT REGISTROS | Fecha de Sometimiento | Actual submission to authority | Critical | RegistrationDossier | SubmittedOn + status Submitted | registration_dossiers.submitted_on | POST /dossiers/{id}/submit | Expediente / Pipeline | Submitter | Submit | RA-CTT-047 | — | NOT EXECUTED |
| CTT REGISTROS | Fecha de observación | Authority observation received | Critical | AuthorityObservation | ObservationReceivedOn | authority_observations + dossiers | POST /dossiers/{id}/observations | Expediente | Manager | Obs | RA-CTT-048 | — | NOT EXECUTED |
| CTT REGISTROS | Fecha estimada para la aprobación | Estimated approval date | Critical | RegistrationDossier | EstimatedApprovalOn | registration_dossiers.estimated_approval_on | PUT /dossiers/{id}/dates | Expediente | Specialist | Authority | RA-CTT-049 | — | NOT EXECUTED |
| CTT REGISTROS | Fecha de la aprobación | External approval date | Critical | RegistrationDossier | ApprovedOn | registration_dossiers.approved_on | POST /dossiers/{id}/approve | Expediente | Manager | External Decision | RA-CTT-050 | — | NOT EXECUTED |
| CTT REGISTROS | Sales / Mkt input | Commercial / marketing notes | High | MedicalDeviceProduct / Dossier | SalesMarketingInput | products + dossiers | GET/PUT /products /dossiers | Portafolio / Expediente | Specialist | — | RA-CTT-051 | — | NOT EXECUTED |
| CTT REGISTROS | Prioridad | Case priority | High | RegistrationDossier | Priority enum | registration_dossiers.priority | GET/PUT /dossiers | Pipeline / Portafolio | Specialist | — | RA-CTT-052 | — | NOT EXECUTED |
| CTT REGISTROS | Oportunidad | Revenue opportunity $ | High | MedicalDeviceProduct / Dossier | OpportunityAmount | products + dossiers | GET /dashboard | Dashboard / Portafolio | Viewer+ | — | RA-CTT-053 | — | NOT EXECUTED |
| CTT REGISTROS | Comentarios | Free-text case comments | High | RegistrationDossier | Comments | registration_dossiers.comments | GET/PUT /dossiers | Expediente | Specialist | — | RA-CTT-054 | — | NOT EXECUTED |
| CTT REGISTROS | Fecha de vencimiento | CT/RS expiry / target expiry | Critical | SanitaryRegistration | ExpiresOn / TargetExpirationOn | sanitary_registrations.expires_on | GET /registrations + alerts | Registros / Alertas | Manager / Viewer | Renewal | RA-CTT-055 | — | NOT EXECUTED |

---

## Section B — DOCUMENTACION (manufacturer vault)

| Excel Sheet | Excel Column | Business Meaning | Criticality | Domain Entity | Property/Rule | DB Table/Column | API | Screen | Role | Workflow | Test Case | Evidence | Status |
|-------------|--------------|------------------|-------------|---------------|---------------|-----------------|-----|--------|------|----------|-----------|----------|--------|
| DOCUMENTACION | FABRICANTE | Manufacturer legal name | Critical | ManufacturerProfile | LegalName | manufacturer_profiles.legal_name | GET/POST /manufacturers | Fabricantes | Specialist / Admin | — | RA-DOC-001 | — | NOT EXECUTED |
| DOCUMENTACION | ESTATUS | Active manufacturer flag | Critical | ManufacturerProfile | IsActive | manufacturer_profiles.is_active | GET /manufacturers | Fabricantes | Viewer+ | — | RA-DOC-002 | — | NOT EXECUTED |
| DOCUMENTACION | PAIS | Manufacturer country | Critical | ManufacturerProfile | CountryCode ISO | manufacturer_profiles.country_code | GET/POST /manufacturers | Fabricantes | Specialist | — | RA-DOC-003 | — | NOT EXECUTED |
| DOCUMENTACION | DOCUMENTO | Certificate type (ISO, CLV…) | Critical | ManufacturerCertificate | Type enum | manufacturer_certificates.type | GET/POST /manufacturer-certificates | Fabricantes | Specialist | — | RA-DOC-004 | — | NOT EXECUTED |
| DOCUMENTACION | FECHA VENCIMIENTO | Certificate expiry | Critical | ManufacturerCertificate | ExpiresOn | manufacturer_certificates.expires_on | GET certificates | Fabricantes / Alertas | Specialist | — | RA-DOC-005 | — | NOT EXECUTED |
| DOCUMENTACION | FORMATO | Legal format apostille/notary | Critical | ManufacturerCertificate | LegalFormat Apostilled Notarized | manufacturer_certificates | POST /manufacturer-certificates | Fabricantes | Specialist | — | RA-DOC-006 | — | NOT EXECUTED |
| DOCUMENTACION | ESTATUS2 | Derived certificate status | High | ManufacturerCertificate | Status computed Active/Expiring/Expired | manufacturer_certificates.status | GET certificates | Fabricantes | Viewer+ | — | RA-DOC-007 | — | NOT EXECUTED |
| DOCUMENTACION | FECHA SOLICITUD | Certificate request date | High | ManufacturerCertificate | RequestedOn | manufacturer_certificates.requested_on | POST certificates | Fabricantes | Specialist | — | RA-DOC-008 | — | NOT EXECUTED |
| DOCUMENTACION | TRAMITE-REQUISITO | Link cert to dossier requirement | High | ManufacturerCertificate | Notes / usage link | manufacturer_certificate_dossier_links | POST links | Fabricantes / Expediente | Specialist | Prep | RA-DOC-009 | — | NOT EXECUTED |
| DOCUMENTACION | COMENTARIOS | Certificate notes | Medium | ManufacturerCertificate | Notes | manufacturer_certificates.notes | POST certificates | Fabricantes | Specialist | — | RA-DOC-010 | — | NOT EXECUTED |
| DOCUMENTACION | SEGUIMIENTO | Follow-up tracking | Medium | ManufacturerCertificate | Notes / history | manufacturer_certificates | GET certificates | Fabricantes | Specialist | — | RA-DOC-011 | — | NOT EXECUTED |
| DOCUMENTACION | VIGENTE | Validity flag for operations | Critical | ManufacturerCertificate | Active/Expiring/Expired | manufacturer_certificates + alerts | GET /alerts/evaluate | Fabricantes / Alertas | Viewer+ | — | RA-DOC-012 | — | NOT EXECUTED |
| DOCUMENTACION | CRITERIO TÉCNICO RS… (criterios vinculados) | Link cert to registration criteria | High | CertificateUsageLink | Registration link | manufacturer_certificate_dossier_links | POST links | Fabricantes / Expediente | Specialist | — | RA-DOC-013 | — | NOT EXECUTED |

---

## Section C — CTT LICENCIAS OP

| Excel Sheet | Excel Column | Business Meaning | Criticality | Domain Entity | Property/Rule | DB Table/Column | API | Screen | Role | Workflow | Test Case | Evidence | Status |
|-------------|--------------|------------------|-------------|---------------|---------------|-----------------|-----|--------|------|----------|-----------|----------|--------|
| CTT LICENCIAS OP | Compañía (Multimed / 4 Hospitals) | Licensed operating company | Critical | OperatingLicense | CompanyName | operating_licenses.company_name | GET/POST /operating-licenses | Licencias | Manager / Admin | — | RA-LIC-001 | — | NOT EXECUTED |
| CTT LICENCIAS OP | Fecha constitución / inicio operaciones | Company incorporation / ops start | High | CompanyMetadata | **Not implemented** — gap | — | — | Licencias Config | Admin | — | RA-LIC-002 | — | NOT EXECUTED |
| CTT LICENCIAS OP | Tipo documento licencia | License document type | Critical | OperatingLicense | LicenseType | operating_licenses.license_type | POST /operating-licenses | Licencias | Manager | — | RA-LIC-003 | — | NOT EXECUTED |
| CTT LICENCIAS OP | Fecha expiración | License expiry | Critical | OperatingLicense | ExpiresOn | operating_licenses.expires_on | GET /operating-licenses | Licencias / Alertas | Manager | Renewal | RA-LIC-004 | — | NOT EXECUTED |
| CTT LICENCIAS OP | Fechas armado / sometimiento / aprobación | Renewal milestone dates | High | LicenseMilestone + LicenseRenewalCase | Milestone timestamps | license_milestones | POST renewals | Licencias | Manager | Renewal | RA-LIC-005 | — | NOT EXECUTED |
| CTT LICENCIAS OP | Checklist: cédulas, regente, registro público | Legal rep ID renewal item | Critical | LicenseRequirement | Catalog item seeded | license_requirements | POST StartLicenseRenewal | Licencias | Manager | Renewal | RA-LIC-006 | — | NOT EXECUTED |
| CTT LICENCIAS OP | Checklist: aviso ops, F&D, timbres, tasa, DJ | Operations notice + fees | Critical | LicenseRequirement | Multiple catalog codes | license_requirements | POST renewals | Licencias | Manager | Renewal | RA-LIC-007 | — | NOT EXECUTED |
| CTT LICENCIAS OP | Checklist: croquis, listado DM, contrato 3ero | Facility / device list items | Critical | LicenseRequirement | Catalog items | license_requirements | POST renewals | Licencias | Manager | Renewal | RA-LIC-008 | — | NOT EXECUTED |
| CTT LICENCIAS OP | Checklist: acondicionamiento, licencia DM, solvencia, catálogo MINSA, oferente | Remaining renewal checklist | Critical | LicenseRequirement | ~18 items total catalog | license_requirements | POST renewals | Licencias | Manager | Renewal | RA-LIC-009 | — | NOT EXECUTED |
| CTT LICENCIAS OP | Estatus / comentarios | License status and comments | Critical | OperatingLicense | Status + Comments | operating_licenses | GET /operating-licenses | Licencias | Viewer+ | — | RA-LIC-010 | — | NOT EXECUTED |
| CTT LICENCIAS OP | Actualizar Panamá Digital / FADDI | Manual platform update task | Medium | LicenseRenewalCase | ManualPlatformUpdate flag | license_renewal_cases | POST renewals | Licencias | Manager | Renewal | RA-LIC-011 | — | NOT EXECUTED |

---

## Section D — Supplementary (non-critical band, recommended)

| Excel Sheet | Excel Column | Business Meaning | Criticality | Domain Entity | Property/Rule | DB Table/Column | API | Screen | Role | Workflow | Test Case | Evidence | Status |
|-------------|--------------|------------------|-------------|---------------|---------------|-----------------|-----|--------|------|----------|-----------|----------|--------|
| CTT REGISTROS | Fecha de Actualización 1–32 (cols 56–87) | Status change history | High | DossierHistoryEvent | Typed events vs 32 Excel cols | dossier_history_events | GET /dossiers/{id} | Expediente | Viewer+ | All transitions | RA-CTT-HIST | — | NOT EXECUTED |
| CTT REGISTROS | Línea | Excel source row number | High | MedicalDeviceProduct | SourceLineNumber | medical_device_products.source_line_number | POST /imports/xlsx | Importación / Portafolio | Admin | Import | RA-CTT-088 | — | NOT EXECUTED |

---

## Execution summary (update during test run)

| Section | Row count | NOT EXECUTED | PASS | FAIL | WAIVED |
|---------|-----------|--------------|------|------|--------|
| A — CTT 1–55 | 55 | 55 | 0 | 0 | 0 |
| B — DOCUMENTACION | 13 | 13 | 0 | 0 | 0 |
| C — LICENCIAS OP | 11 | 11 | 0 | 0 | 0 |
| D — Supplementary | 2 | 2 | 0 | 0 | 0 |
| **Total** | **81** | **81** | **0** | **0** | **0** |

---

## Document control

| Version | Date | Change |
|---------|------|--------|
| 1.0 | 2026-07-14 | Initial matrix from REGULATORY_COVERAGE_MATRIX; all NOT EXECUTED |
