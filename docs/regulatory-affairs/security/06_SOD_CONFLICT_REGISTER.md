# 06 — SoD Conflict Register (AS-IS → FIX)

| ID | ¿Posible hoy? | Endpoint / permiso / rol | Backend vs UI | Sev | Corrección |
|----|---------------|--------------------------|---------------|-----|------------|
| SOD-001 | **Sí** | Specialist CREATE + REQUIREMENT.MANAGE Accept propio | Solo claims | Critical | PreventSelfReview + REVIEW separado |
| SOD-002 | **Sí** vía Admin/Specialist submit sin gate interno | Admin tiene APPROVE; Specialist SUBMIT sin ApprovedForSubmission | Backend ausente gate interno | Critical | Estado `ApprovedForSubmission` + Approver |
| SOD-003 | **Parcial** | Reviewer APPROVE externo ≠ “aprobar revisión”; falta Approver interno | Semántica confusa | High | Separar REVIEW vs APPROVE_FOR_SUBMISSION vs APPROVE |
| SOD-004 | **Sí** | Regulatory.Submit acepta APPROVE → Reviewer puede `/submit` | Policy leak | Critical | Submit = SUBMIT only + SeparateApproverAndSubmitter |
| SOD-005 | **Sí** | ApproveDossier crea CT/RS en un paso | Diseño intencional débil | High | Exigir UAR + evidencia número; Manager only |
| SOD-006 | **Sí** Admin | CONFIGURE + APPROVE misma identidad | Backend | High | Admin solo CONFIGURE |
| SOD-007 | **Sí** | REQUIREMENT.MANAGE → Waived + mismo user APPROVE | Sin second check | High | RequireSecondApprovalForCriticalWaiver |
| SOD-008 | N/A parcial | Docs RA soft-link; DOCUMENT.APPROVE QMS separado | — | Med | Policy SeparateDocumentUploaderAndReviewer cuando hard-link |
| SOD-009 | **Sí** | OBS.MANAGE open+respond+close misma id | Sin separación | High | PreventSelfCloseObservation default ON soft / CONDITIONAL |
| SOD-010 | Parcial | Update dates + alert evaluate sin evidencia | Low-Med | Motivo auditado en waiver fechas |
| SOD-011 | **No auto-approve** | Import Configure only; status Planning | OK | — | Mantener; no auto Approved |
| SOD-012 | **Sí** | Tenant Admin = 17 perms | Critical | Strip operational RA from TAC |

## Severidades abiertas al cierre del diseño
Ver `12_SOD_EXECUTION_REPORT.md` tras pruebas.
