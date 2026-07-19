# 20 — SoD Browser E2E Evidence

**Runner:** Playwright `e2e/tests/regulatory-sod-roles.spec.ts`  
**Resultado:** **1 passed (11.2s)**  
**Artefactos:**

- `evidence/browser-sod-console.txt`
- `evidence/browser-sod-steps.json`
- `evidence/browser-specialist.png`
- `evidence/browser-reviewer.png`
- `evidence/browser-approver.png`
- `evidence/browser-submitter.png`

## Flujo ejecutado en Chromium real

1. Specialist login limpio → RA Console perfil specialist → create product/dossier → self-review DENIED → approve-for-submission DENIED → prep transitions → logout (clear storage).
2. Reviewer login limpio → accept criticals → approve/submit DENIED → logout.
3. Approver login limpio → ApprovedForSubmission → submit DENIED → logout.
4. Submitter login limpio → internal DENIED → Submitted → logout.
5. Viewer login → create DENIED.

UI contextual: badge de perfil + botones ocultos por permiso (`regulatory-affairs.js` v2).
