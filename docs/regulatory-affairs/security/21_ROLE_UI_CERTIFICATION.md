# 21 — Role UI Certification

**Archivo:** `wwwroot/regulatory-affairs.js` (reescrito contexual SoD)

| Rol | Nav | Acciones visibles clave | PASS |
|-----|-----|-------------------------|------|
| Specialist | Dashboard cola prep, portfolio create | Prep transitions, marcar recibido; **sin** approve internal/submit/CT | PASS (browser) |
| Reviewer | Cola review | Accept/Reject; **sin** create/approve/submit | PASS |
| Approver | Cola ReadyForSubmission | Approve-for-submission; **sin** submit/CT | PASS |
| Submitter | Cola ApprovedForSubmission | Submit; **sin** internal approve | PASS |
| Manager | Pipeline + obs + CT | Observation + external approve | PASS (API+UI capabilities) |
| Admin | Config/Import/SoD | Bootstrap; **sin** operate dossier default | PASS (grants+UI filter) |
| Viewer | Read views | Sin mutaciones | PASS |
| TAC | Grants read/configure | Sin botones operate si JWT sin perms | PASS |

Flujo visual distingue:

- `Aprobado internamente para sometimiento`
- `Aprobación registrada de MINSA/CSS (externa)`
