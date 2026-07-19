# BUTTON_DICTIONARY

| Screen | Button | Action | Role | Permission | Precondition | Result |
|--------|--------|--------|------|------------|--------------|--------|
| login | Iniciar sesion / Completar login seguro | POST /api/v1/auth/login | * | — | Credenciales válidas | Token + redirección al dashboard |
| regulatory-portfolio | Nuevo producto + expediente | POST product + POST dossier | regulatory-specialist, regulatory-administrator | PRODUCT.MANAGE + DOSSIER.CREATE | Bootstrap/autoridad disponible | Producto en portafolio y expediente en Planning |
| regulatory-dossier-detail | Pedir docs fábrica | POST …/transition → WaitingManufacturerDocuments | regulatory-specialist | DOSSIER.UPDATE | Estado Planning (u permitido) | Estado Espera docs fábrica |
| regulatory-dossier-detail | Docs recibidos | POST …/transition → DocumentsReceived | regulatory-specialist | DOSSIER.UPDATE | — | Estado Docs recibidos |
| regulatory-dossier-detail | Armar | POST …/transition → Assembling | regulatory-specialist | DOSSIER.UPDATE | — | Estado Armando expediente |
| regulatory-dossier-detail | Declarar técnicamente completo | POST …/transition → ReadyForSubmission | regulatory-specialist | DOSSIER.UPDATE | — | Listo para aprobación interna |
| regulatory-dossier-detail | Marcar recibido | PUT requirement status Received | regulatory-specialist | REQUIREMENT.MANAGE / UPDATE | — | Requisito marcado recibido |
| regulatory-dossier-detail | Aceptar | PUT requirement status Accepted | regulatory-reviewer | DOSSIER.REVIEW | — | Requisito aceptado |
| regulatory-dossier-detail | Rechazar | PUT requirement status Rejected | regulatory-reviewer | DOSSIER.REVIEW | — | Requisito rechazado; Specialist corrige |
| regulatory-dossier-detail | Aprobar internamente para sometimiento | POST …/approve-for-submission | regulatory-approver | APPROVE_FOR_SUBMISSION | ReadyForSubmission | ApprovedForSubmission — NO es aprobación MINSA/CSS |
| regulatory-dossier-detail | Registrar sometimiento | POST …/submit | regulatory-submitter | DOSSIER.SUBMIT | ApprovedForSubmission (SoD ON) | Submitted — sometimiento registrado |
| regulatory-dossier-detail | Registrar observación autoridad | POST …/observations | regulatory-manager | OBSERVATION.MANAGE | Submitted / UnderAuthorityReview | Observed — observación recibida de la autoridad |
| regulatory-dossier-detail | Responder | POST …/observations/{id}/respond | regulatory-specialist | OBSERVATION.MANAGE | Observación abierta | CorrectingObservation / respuesta registrada |
| regulatory-dossier-detail | Registrar aprobación MINSA/CSS + CT/RS | POST …/approve | regulatory-manager, quality-manager | DOSSIER.APPROVE | Número CT/RS + vencimiento | Approved + CT/RS activo |
| regulatory-manufacturers | Alta fabricante | POST manufacturer | regulatory-specialist, regulatory-administrator | MANUFACTURER_DOCUMENT.MANAGE | — | Fabricante listado (+ cert ISO13485 en flujo demo) |
| regulatory-licenses | Nueva licencia | POST license | regulatory-administrator | LICENSE.MANAGE | — | Licencia en listado |
| regulatory-import | Stage XLSX | POST import stage | regulatory-administrator | CONFIGURE | — | Job de importación en staging |
| regulatory-config | Bootstrap regulatorio | Seed authorities + pack | regulatory-administrator, tenant-administrator | CONFIGURE | — | MINSA/CSS + pack de requisitos |
| regulatory-dossier-detail | ← Expedientes | Volver al listado | * | — | — | Vista Expedientes |
