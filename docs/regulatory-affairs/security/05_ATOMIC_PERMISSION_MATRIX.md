# 05 — Atomic Permission Matrix (Adopted v1)

## Antes → Después

| Antes (17) | Decisión |
|------------|---------|
| PRODUCT.READ/MANAGE | Mantener |
| DOSSIER.READ/CREATE/UPDATE | Mantener |
| DOSSIER.SUBMIT | Mantener — **solo** submit policy |
| DOSSIER.APPROVE | Mantener = **decisión externa + CT/RS** (renombrar semántica en descripción) |
| REQUIREMENT.MANAGE | Mantener prep; Accept/Reject/Waive gateados por REVIEW + SoD |
| OBSERVATION.MANAGE | Mantener |
| REGISTRATION.READ/MANAGE | Mantener |
| MFR_DOC / LICENSE / REPORT / CONFIGURE | Mantener |

## Nuevos (v1)

| Código | Uso |
|--------|-----|
| REGULATORY.DOSSIER.REVIEW | Accept/Reject requisitos; revisión técnica |
| REGULATORY.DOSSIER.APPROVE_FOR_SUBMISSION | Autorización **interna** → `ApprovedForSubmission` |
| REGULATORY.SOD.EMERGENCY_OVERRIDE | Break-glass con motivo + auditoría |
| REGULATORY.SOD.MANAGE | Configurar política SoD del tenant |

## Diferimiento (fase 2, sin bloquear SoD crítico)
Atomicidad completa REQUIREMENT.*/DOCUMENT.*/OBSERVATION.*/LICENSE.* listada en el brief — mapear en UI y APIs granulares cuando existan endpoints dedicados. Hoy se aplica SoD sobre los endpoints reales + permisos v1.

## Policies

| Policy | Accepts |
|--------|---------|
| Regulatory.Submit | **solo** DOSSIER.SUBMIT |
| Regulatory.ApproveForSubmission | APPROVE_FOR_SUBMISSION |
| Regulatory.Approve | APPROVE ∨ REGISTRATION.MANAGE (= externo) |
| Regulatory.Review | REVIEW ∨ APPROVE_FOR_SUBMISSION (lectura elev.) |
| Regulatory.Manage | prep writes — **sin** terminal Authority Approved/Rejected |
| Regulatory.SoDOverride | EMERGENCY_OVERRIDE |
