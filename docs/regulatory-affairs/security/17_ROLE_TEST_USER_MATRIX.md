# 17 — Role Test User Matrix (lab Irving)

**Tenant:** `82af3877-2786-4d39-bce8-c981101c771d`  
**Password:** managed offline (no passwords in this file)  
**Tokens:** emitted fresh per login; lab logout clears `localStorage`/`sessionStorage`.

| Usuario lógico | Email | Rol | Propósito | Estado |
|----------------|-------|-----|-----------|--------|
| U-RA-ADM | ra.admin@cert.local | Regulatory Administrator | Config / SoD settings / import | ACTIVO |
| U-RA-MGR | ra.mgr@cert.local | Regulatory Manager | Observaciones / CT/RS externo / override | ACTIVO |
| U-RA-SPEC | ra.spec@cert.local | Regulatory Specialist | Preparación | ACTIVO |
| U-RA-REV | ra.rev@cert.local | Regulatory Reviewer | Revisión técnica | ACTIVO |
| U-RA-APPR | ra.appr@cert.local | Regulatory Approver | Aprobación interna | ACTIVO |
| U-RA-SUB | ra.sub@cert.local | Regulatory Submitter | Sometimiento | ACTIVO |
| U-RA-VIEW | ra.view@cert.local | Regulatory Viewer | Solo lectura | ACTIVO |
| U-RA-QM | ra.qm@cert.local | Quality Manager | CT/RS transversal QMS | ACTIVO |
| U-TAC | irvingcorrosk19@gmail.com | Tenant Administrator | IAM / seed / Sin operate RA | ACTIVO |

Sales Viewer / Document Contributor: **no implementados** → no PASS.

Grants JWT validados en ejecución `run-sod-api-e2e.ps1` (54/54).
