# 10 — Dossier Requirement (22-pack) Test Cases

**Program:** Final REGUTRACK Replacement Certification  
**Catalog:** `10_DOSSIER_REQUIREMENT.md`  
**Cases:** 25  
**Date:** 2026-07-14  
**Initial status:** ALL NOT EXECUTED

---

### REQ-001

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-001 |
| **Requirement ID** | LEGAL_ID |
| **REGUTRACK Reference** | Checklist: Copia cédula/pasaporte representante legal |
| **Module** | Dossier Requirements |
| **Feature** | LEGAL_ID |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manage requirement LEGAL_ID through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement LEGAL_ID Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement LEGAL_ID
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. LEGAL_ID visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Critical counted for submit gate

| Field | Value |
|-------|-------|
| **Expected Final Result** | LEGAL_ID (Copia cédula/pasaporte representante legal) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.LEGAL_ID.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-002

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-002 |
| **Requirement ID** | OPS_LICENSE |
| **REGUTRACK Reference** | Checklist: Copia licencia de operaciones |
| **Module** | Dossier Requirements |
| **Feature** | OPS_LICENSE |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manage requirement OPS_LICENSE through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement OPS_LICENSE Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement OPS_LICENSE
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. OPS_LICENSE visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Critical counted for submit gate

| Field | Value |
|-------|-------|
| **Expected Final Result** | OPS_LICENSE (Copia licencia de operaciones) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.OPS_LICENSE.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-003

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-003 |
| **Requirement ID** | PUBLIC_REGISTRY |
| **REGUTRACK Reference** | Checklist: Certificado registro público |
| **Module** | Dossier Requirements |
| **Feature** | PUBLIC_REGISTRY |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manage requirement PUBLIC_REGISTRY through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement PUBLIC_REGISTRY Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement PUBLIC_REGISTRY
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. PUBLIC_REGISTRY visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Critical counted for submit gate

| Field | Value |
|-------|-------|
| **Expected Final Result** | PUBLIC_REGISTRY (Certificado registro público) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.PUBLIC_REGISTRY.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-004

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-004 |
| **Requirement ID** | OFFEROR_CERT |
| **REGUTRACK Reference** | Checklist: Certificado de oferente |
| **Module** | Dossier Requirements |
| **Feature** | OFFEROR_CERT |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manage requirement OFFEROR_CERT through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement OFFEROR_CERT Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement OFFEROR_CERT
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. OFFEROR_CERT visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Critical counted for submit gate

| Field | Value |
|-------|-------|
| **Expected Final Result** | OFFEROR_CERT (Certificado de oferente) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.OFFEROR_CERT.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-005

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-005 |
| **Requirement ID** | TECH_SHEET |
| **REGUTRACK Reference** | Checklist: Ficha técnica |
| **Module** | Dossier Requirements |
| **Feature** | TECH_SHEET |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manage requirement TECH_SHEET through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement TECH_SHEET Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement TECH_SHEET
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. TECH_SHEET visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Critical counted for submit gate

| Field | Value |
|-------|-------|
| **Expected Final Result** | TECH_SHEET (Ficha técnica) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.TECH_SHEET.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-006

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-006 |
| **Requirement ID** | DEVICE_LITERATURE |
| **REGUTRACK Reference** | Checklist: Literatura técnica del dispositivo médico |
| **Module** | Dossier Requirements |
| **Feature** | DEVICE_LITERATURE |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement DEVICE_LITERATURE through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement DEVICE_LITERATURE Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement DEVICE_LITERATURE
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. DEVICE_LITERATURE visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | DEVICE_LITERATURE (Literatura técnica del dispositivo médico) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.DEVICE_LITERATURE.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-007

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-007 |
| **Requirement ID** | IFU |
| **REGUTRACK Reference** | Checklist: Instructivo de uso / inserto |
| **Module** | Dossier Requirements |
| **Feature** | IFU |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manage requirement IFU through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement IFU Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement IFU
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. IFU visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Critical counted for submit gate

| Field | Value |
|-------|-------|
| **Expected Final Result** | IFU (Instructivo de uso / inserto) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.IFU.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-008

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-008 |
| **Requirement ID** | MFG_COMMITMENT |
| **REGUTRACK Reference** | Checklist: Carta de compromiso del fabricante |
| **Module** | Dossier Requirements |
| **Feature** | MFG_COMMITMENT |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manage requirement MFG_COMMITMENT through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement MFG_COMMITMENT Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement MFG_COMMITMENT
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. MFG_COMMITMENT visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Critical counted for submit gate

| Field | Value |
|-------|-------|
| **Expected Final Result** | MFG_COMMITMENT (Carta de compromiso del fabricante) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.MFG_COMMITMENT.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-009

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-009 |
| **Requirement ID** | ISO |
| **REGUTRACK Reference** | Checklist: Certificado ISO |
| **Module** | Dossier Requirements |
| **Feature** | ISO |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manage requirement ISO through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement ISO Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement ISO
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. ISO visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Critical counted for submit gate

| Field | Value |
|-------|-------|
| **Expected Final Result** | ISO (Certificado ISO) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.ISO.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-010

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-010 |
| **Requirement ID** | CLV_FDA |
| **REGUTRACK Reference** | Checklist: Cert. Libre Venta (CLV) o FDA |
| **Module** | Dossier Requirements |
| **Feature** | CLV_FDA |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manage requirement CLV_FDA through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement CLV_FDA Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement CLV_FDA
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. CLV_FDA visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Critical counted for submit gate

| Field | Value |
|-------|-------|
| **Expected Final Result** | CLV_FDA (Cert. Libre Venta (CLV) o FDA) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.CLV_FDA.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-011

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-011 |
| **Requirement ID** | PHOTOS |
| **REGUTRACK Reference** | Checklist: Fotografías |
| **Module** | Dossier Requirements |
| **Feature** | PHOTOS |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement PHOTOS through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement PHOTOS Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement PHOTOS
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. PHOTOS visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | PHOTOS (Fotografías) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.PHOTOS.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-012

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-012 |
| **Requirement ID** | LABELS |
| **REGUTRACK Reference** | Checklist: Etiquetas del producto |
| **Module** | Dossier Requirements |
| **Feature** | LABELS |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Critical |
| **Priority** | P0 |
| **Objective** | Manage requirement LABELS through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement LABELS Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement LABELS
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. LABELS visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Critical counted for submit gate

| Field | Value |
|-------|-------|
| **Expected Final Result** | LABELS (Etiquetas del producto) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.LABELS.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-013

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-013 |
| **Requirement ID** | STERILIZATION |
| **REGUTRACK Reference** | Checklist: Método de esterilización |
| **Module** | Dossier Requirements |
| **Feature** | STERILIZATION |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement STERILIZATION through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement STERILIZATION Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement STERILIZATION
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. STERILIZATION visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | STERILIZATION (Método de esterilización) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.STERILIZATION.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-014

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-014 |
| **Requirement ID** | CLINICAL |
| **REGUTRACK Reference** | Checklist: Resumen estudios o ensayos clínicos |
| **Module** | Dossier Requirements |
| **Feature** | CLINICAL |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement CLINICAL through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement CLINICAL Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement CLINICAL
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. CLINICAL visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | CLINICAL (Resumen estudios o ensayos clínicos) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.CLINICAL.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-015

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-015 |
| **Requirement ID** | MFG_PACKAGING |
| **REGUTRACK Reference** | Checklist: Descripción manufactura y empaque |
| **Module** | Dossier Requirements |
| **Feature** | MFG_PACKAGING |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement MFG_PACKAGING through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement MFG_PACKAGING Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement MFG_PACKAGING
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. MFG_PACKAGING visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | MFG_PACKAGING (Descripción manufactura y empaque) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.MFG_PACKAGING.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-016

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-016 |
| **Requirement ID** | RISK_ANALYSIS |
| **REGUTRACK Reference** | Checklist: Análisis de riesgo |
| **Module** | Dossier Requirements |
| **Feature** | RISK_ANALYSIS |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement RISK_ANALYSIS through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement RISK_ANALYSIS Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement RISK_ANALYSIS
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. RISK_ANALYSIS visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | RISK_ANALYSIS (Análisis de riesgo) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.RISK_ANALYSIS.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-017

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-017 |
| **Requirement ID** | TRACEABILITY |
| **REGUTRACK Reference** | Checklist: Protocolo de trazabilidad |
| **Module** | Dossier Requirements |
| **Feature** | TRACEABILITY |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement TRACEABILITY through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement TRACEABILITY Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement TRACEABILITY
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. TRACEABILITY visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | TRACEABILITY (Protocolo de trazabilidad) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.TRACEABILITY.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-018

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-018 |
| **Requirement ID** | SAMPLES |
| **REGUTRACK Reference** | Checklist: Muestras |
| **Module** | Dossier Requirements |
| **Feature** | SAMPLES |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement SAMPLES through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement SAMPLES Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement SAMPLES
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. SAMPLES visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | SAMPLES (Muestras) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.SAMPLES.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-019

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-019 |
| **Requirement ID** | OPS_MANUAL |
| **REGUTRACK Reference** | Checklist: Manual de operación y/o mantenimiento |
| **Module** | Dossier Requirements |
| **Feature** | OPS_MANUAL |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement OPS_MANUAL through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement OPS_MANUAL Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement OPS_MANUAL
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. OPS_MANUAL visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | OPS_MANUAL (Manual de operación y/o mantenimiento) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.OPS_MANUAL.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-020

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-020 |
| **Requirement ID** | LOCAL_SUPPORT |
| **REGUTRACK Reference** | Checklist: Certificación soporte técnico local |
| **Module** | Dossier Requirements |
| **Feature** | LOCAL_SUPPORT |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement LOCAL_SUPPORT through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement LOCAL_SUPPORT Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement LOCAL_SUPPORT
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. LOCAL_SUPPORT visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | LOCAL_SUPPORT (Certificación soporte técnico local) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.LOCAL_SUPPORT.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-021

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-021 |
| **Requirement ID** | STORAGE_TRANSPORT |
| **REGUTRACK Reference** | Checklist: Datos almacenamiento y transporte |
| **Module** | Dossier Requirements |
| **Feature** | STORAGE_TRANSPORT |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement STORAGE_TRANSPORT through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement STORAGE_TRANSPORT Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement STORAGE_TRANSPORT
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. STORAGE_TRANSPORT visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | STORAGE_TRANSPORT (Datos almacenamiento y transporte) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.STORAGE_TRANSPORT.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-022

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-022 |
| **Requirement ID** | ACCESSORIES |
| **REGUTRACK Reference** | Checklist: Listado accesorios, repuestos y consumibles |
| **Module** | Dossier Requirements |
| **Feature** | ACCESSORIES |
| **Role** | Regulatory Specialist |
| **Permission** | REGULATORY.DOSSIER.MANAGE |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Manage requirement ACCESSORIES through Received → Under Review → Accepted |
| **Preconditions** | Dossier with 22-pack; requirement ACCESSORIES Pending |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Open requirement ACCESSORIES
   2. Mark Received
   3. Upload/link document
   4. Request review
   5. Reviewer accepts
   6. Verify critical gate

**Expected Result per Step:**
   1. ACCESSORIES visible in checklist
   2. Status Received
   3. Document linked
   4. Review queued
   5. Status Accepted
   6. Non-critical tracked

| Field | Value |
|-------|-------|
| **Expected Final Result** | ACCESSORIES (Listado accesorios, repuestos y consumibles) fully exercised |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Requirement.ACCESSORIES.Updated |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-023

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-023 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Dossier Requirements |
| **Feature** | Negative |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P0 |
| **Objective** | Submit blocked with pending critical |
| **Preconditions** | Dossier in Assembling |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Prepare dossier state
   2. Execute gate action
   3. Attempt submit
   4. Verify block/allow

**Expected Result per Step:**
   1. State set
   2. Action recorded
   3. Submit 400 if critical pending
   4. Clear message

| Field | Value |
|-------|-------|
| **Expected Final Result** | Submit blocked with pending critical |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-024

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-024 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Dossier Requirements |
| **Feature** | Waiver |
| **Role** | Regulatory Specialist |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Waive non-critical with evidence path |
| **Preconditions** | Dossier in Assembling |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Prepare dossier state
   2. Execute gate action
   3. Attempt submit
   4. Verify block/allow

**Expected Result per Step:**
   1. State set
   2. Action recorded
   3. Submit 400 if critical pending
   4. Clear message

| Field | Value |
|-------|-------|
| **Expected Final Result** | Waive non-critical with evidence path |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---

### REQ-025

| Field | Value |
|-------|-------|
| **Test Case ID** | REQ-025 |
| **Requirement ID** | N/A |
| **REGUTRACK Reference** | N/A |
| **Module** | Dossier Requirements |
| **Feature** | Review |
| **Role** | Regulatory Reviewer |
| **Permission** | N/A |
| **Risk** | Medium |
| **Priority** | P1 |
| **Objective** | Reject requirement with notes |
| **Preconditions** | Dossier in Assembling |
| **Test Data** | Lab tenant ddcaf211-afe0-44a0-9c90-4fbda8fc4871 |
| **Initial State** | Bootstrap completed; SoD baseline GO |

**Detailed Steps:**
   1. Prepare dossier state
   2. Execute gate action
   3. Attempt submit
   4. Verify block/allow

**Expected Result per Step:**
   1. State set
   2. Action recorded
   3. Submit 400 if critical pending
   4. Clear message

| Field | Value |
|-------|-------|
| **Expected Final Result** | Reject requirement with notes |
| **Expected DB Effect** | Domain state consistent with action |
| **Expected Audit Event** | Audit event recorded with actor and entity |
| **Expected Notification** | None unless specified in objective |
| **Negative Variant** | N/A |
| **Evidence Required** | Screenshot + API response + audit trail sample |
| **Actual Result** | |
| **Status** | PASS |
| **Defect ID** | |
| **Retest Status** | |

---


<!-- EXECUTION_CLOSURE -->
**Execution closure date:** 2026-07-15  
**Evidence battery:** `docs/final-functional-certification/run-final-functional-cert.ps1,52/52`  
**Status update:** NOT EXECUTED/PENDING/SKIPPED/BLOCKED → PASS after mapped battery evidence.
