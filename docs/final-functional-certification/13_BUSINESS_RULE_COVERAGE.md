# 13 — Business Rule Coverage

**Program:** Final REGUTRACK Replacement Certification  
**Contract:** `REGUTRACK 02JUN26 MG.xlsx` + `REGUTRACK-PA-DEFAULT` (22 requirements)  
**Date:** 2026-07-14

---

## 1. Critical business rules

| ID | Rule | Enforcement | Test Case | Status |
|----|------|-------------|-----------|--------|
| BR-001 | Submit blocked if any critical requirement not Accepted | API + UI | REQ-022 | NOT EXECUTED |
| BR-002 | Specialist cannot approve for submission | SoD policy | SOD-003 | NOT EXECUTED |
| BR-003 | Creator cannot self-review requirements | PreventSelfReview | SOD-001 | NOT EXECUTED |
| BR-004 | Approver cannot submit | SeparateApproverAndSubmitter | SOD-004 | NOT EXECUTED |
| BR-005 | Active registration requires CT/RS number | Domain validation | REG-005 | NOT EXECUTED |
| BR-006 | Expiration date must be after issued date | Domain validation | REG-006 | NOT EXECUTED |
| BR-007 | Pack snapshot frozen at dossier creation | DB immutability | PACK-008 | NOT EXECUTED |
| BR-008 | Cross-tenant access returns 403 | Tenant filter | MT-001 | NOT EXECUTED |
| BR-009 | Import does not destroy data without report | Import service | IMP-012 | NOT EXECUTED |
| BR-010 | Dashboard KPI = API = DB calculation | Reconciliation | DASH-010 | NOT EXECUTED |

---

## 2. REGUTRACK 22-requirement checklist coverage

### 2.1 Critical requirements (must gate submit)

| Code | Name | Criticality | Rule | Test Case | Status |
|------|------|-------------|------|-----------|--------|
| LEGAL_ID | Copia cédula/pasaporte representante legal | Critical | Accept before submit | REQ-001 | NOT EXECUTED |
| OPS_LICENSE | Copia licencia de operaciones | Critical | Accept before submit | REQ-002 | NOT EXECUTED |
| PUBLIC_REGISTRY | Certificado registro público | Critical | Accept before submit | REQ-003 | NOT EXECUTED |
| OFFEROR_CERT | Certificado de oferente | Critical | Accept before submit | REQ-004 | NOT EXECUTED |
| TECH_SHEET | Ficha técnica | Critical | Accept before submit | REQ-005 | NOT EXECUTED |
| IFU | Instructivo de uso / inserto | Critical | Accept before submit | REQ-007 | NOT EXECUTED |
| MFG_COMMITMENT | Carta de compromiso del fabricante | Critical | Accept before submit | REQ-008 | NOT EXECUTED |
| ISO | Certificado ISO | Critical | Accept before submit | REQ-009 | NOT EXECUTED |
| CLV_FDA | Cert. Libre Venta (CLV) o FDA | Critical | Accept before submit | REQ-010 | NOT EXECUTED |
| LABELS | Etiquetas del producto | Critical | Accept before submit | REQ-012 | NOT EXECUTED |

### 2.2 Non-critical requirements

| Code | Name | Criticality | Rule | Test Case | Status |
|------|------|-------------|------|-----------|--------|
| DEVICE_LITERATURE | Literatura técnica del dispositivo médico | Non-critical | Track status | REQ-006 | NOT EXECUTED |
| PHOTOS | Fotografías | Non-critical | Track status | REQ-011 | NOT EXECUTED |
| STERILIZATION | Método de esterilización | Non-critical | Track status | REQ-013 | NOT EXECUTED |
| CLINICAL | Resumen estudios o ensayos clínicos | Non-critical | Track status | REQ-014 | NOT EXECUTED |
| MFG_PACKAGING | Descripción manufactura y empaque | Non-critical | Track status | REQ-015 | NOT EXECUTED |
| RISK_ANALYSIS | Análisis de riesgo | Non-critical | Track status | REQ-016 | NOT EXECUTED |
| TRACEABILITY | Protocolo de trazabilidad | Non-critical | Track status | REQ-017 | NOT EXECUTED |
| SAMPLES | Muestras | Non-critical | Track status | REQ-018 | NOT EXECUTED |
| OPS_MANUAL | Manual de operación y/o mantenimiento | Non-critical | Track status | REQ-019 | NOT EXECUTED |
| LOCAL_SUPPORT | Certificación soporte técnico local | Non-critical | Track status | REQ-020 | NOT EXECUTED |
| STORAGE_TRANSPORT | Datos almacenamiento y transporte | Non-critical | Track status | REQ-021 | NOT EXECUTED |
| ACCESSORIES | Listado accesorios, repuestos y consumibles | Non-critical | Track status | REQ-022 | NOT EXECUTED |

---

## 3. Excel column rules (selected)

| Excel Column | Business Rule | Test Case |
|--------------|---------------|-----------|
| Criterio Técnico/ Registro Sanitario No. | Maps to SanitaryRegistration.Number | REG-001 |
| Fecha Criterio /Registro | Maps to IssuedOn | MIL-010 |
| Clase de Riesgo | Product.RiskClass A/B/C | PROD-005 |
| Ficha Tecnica | Links TECH_SHEET requirement | REQ-005 |
| Entidad Emisora | Authority MINSA/CSS | AUTH-002 |

---

*All business rule test cases: NOT EXECUTED.*
