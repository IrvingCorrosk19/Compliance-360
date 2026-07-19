# 04 — Target Role Model

## Principio
Permisos atómicos primero. Roles = colecciones con propósito SoD claro.  
**Aprobación interna ≠ decisión MINSA/CSS.**

| Rol | Propósito | Puede | No puede |
|-----|-----------|-------|----------|
| Regulatory Administrator | Configurar módulo | Authorities, packs, alertas, import REGUTRACK, lecturas | Aprobar/someter/operar expediente por defecto |
| Regulatory Manager | Supervisión operativa | Ver todo RA, asignar, reportes, excepciones SoD (override), registrar decisión externa / CT/RS | Sustituir Specialist+Approver+Submitter sin override |
| Regulatory Specialist | Preparar | Productos, crear/editar dossier, prep requisitos, docs fabricante, responder obs | Self-review, approve-for-submission, submit (con SoD ON), CT/RS |
| Regulatory Reviewer | Revisión técnica | Accept/Reject requisitos, devolver a preparación | Preparar el mismo caso si PreventSelfReview; approve-for-submission; submit |
| Regulatory Approver | Autorización interna | Approbar/rechazar para sometimiento | Representar MINSA/CSS; submit si SeparateApproverAndSubmitter |
| Regulatory Submitter | Registro de envío | Submit + evidencias de envío | Approve interno o externo |
| Regulatory Viewer | Solo lectura | READ portfolio | Cualquier mutación |
| Quality Manager | QMS transversal | Puede conservar registro externo CT/RS (APPROVE) | No preparar por defecto |
| Tenant Administrator | IAM / tenant | Users/roles + CONFIGURE RA (bootstrap) | **Sin** SUBMIT/APPROVE/CREATE dossier por defecto |
| Sales Viewer / Document Contributor | *(fase 2 UI)* | Lectura comercial / upload acotado | Docs confidenciales / approve |

**No** se crea Observation Specialist ni Registration Manager como roles separados en v1: capacidades = Specialist (responder) + Manager (external decision).
