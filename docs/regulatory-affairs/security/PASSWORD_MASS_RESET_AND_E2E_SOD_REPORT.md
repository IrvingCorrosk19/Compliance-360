# Informe Ejecutivo — Cambio masivo de contraseñas + E2E SoD por roles

**Fecha:** 2026-07-17  
**Ambiente:** Local (`http://localhost:5272`)  
**Tenant:** `82af3877-2786-4d39-bce8-c981101c771d`  
**Expediente único:** `c525eafc-f1ae-4a31-83c7-5b32e172de71`  
**Veredicto final:** **PASS** (listo para lab de certificación funcional SoD; ver nota de resolución externa)

---

## 1. Archivos modificados

| Archivo | Motivo |
|---|---|
| `e2e/tests/regulatory-sod-roles.spec.ts` | Contraseña E2E → `OwnerStart!2026`; transiciones Specialist verificadas; Viewer reforzado (read + bloqueo create/edit/review/approve/submit/delete) |
| `scripts/provision-ra-sod-users.ps1` | Alinear password de laboratorio SoD a `OwnerStart!2026` para futuros reprovisionados |

**Sin cambios** en Identity, PasswordHasher, Domain, Application ni políticas RBAC. Las contraseñas se cambiaron **por API oficial**, no por UPDATE SQL manual.

---

## 2. Método utilizado para cambiar las contraseñas

### Análisis previo (mecanismo real del sistema)

- **No** ASP.NET Identity / UserManager.
- Identity **custom**: `User` + `Pbkdf2PasswordHasher` (`PBKDF2-SHA256`, 210 000 iteraciones).
- Reset administrativo oficial:
  - Endpoint: `POST /api/v1/tenants/{tenantId}/users/{userId}/reset-password`
  - Servicio: `TenantManagementService.ResetUserPasswordAsync`
  - Dominio: `User.ChangePassword(hash, now)` → historial en `password_history`, limpia lockout vía `SetPassword`, opcional `ForcePasswordChange`, revoca sesiones activas.
- Política de contraseña: ≥12 caracteres, mayúscula, minúscula, dígito y símbolo.

### Ejecución

1. Login como administrador de tenant `irvingcorrosk19@gmail.com`.
2. Para cada usuario RA: llamada a `reset-password` con:
   - `newPassword = OwnerStart!2026`
   - `forcePasswordChange = false`
   - `changeReason = Mass password alignment OwnerStart!2026 - local E2E certification`
3. Validación inmediata identify+login + inspección de JWT (rol + permissions).

Evidencia: `docs/regulatory-affairs/security/evidence/password-reset-validation.json`

### Integridad post-reset (PostgreSQL 18)

| Email | ForcePasswordChange | AccessFailedCount | Lockout | PasswordHash |
|---|---|---|---|---|
| ra.spec@cert.local | false | 0 | null | PBKDF2-SHA256.210000.… |
| ra.rev@cert.local | false | 0 | null | PBKDF2-SHA256.210000.… |
| ra.appr@cert.local | false | 0 | null | PBKDF2-SHA256.210000.… |
| ra.sub@cert.local | false | 0 | null | PBKDF2-SHA256.210000.… |
| ra.view@cert.local | false | 0 | null | PBKDF2-SHA256.210000.… |

Roles y permisos **no** fueron alterados.

---

## 3. Confirmación de autenticación (5/5)

Todos autenticaron con `OwnerStart!2026`:

| Usuario | Login | Rol JWT | Permisos JWT |
|---|---|---|---|
| ra.spec@cert.local | OK | Regulatory Specialist | 14 (incluye PRODUCT.MANAGE, DOSSIER.CREATE/UPDATE, REQUIREMENT.MANAGE) |
| ra.rev@cert.local | OK | Regulatory Reviewer | 8 (incluye DOSSIER.REVIEW, REQUIREMENT.MANAGE) |
| ra.appr@cert.local | OK | Regulatory Approver | 7 (incluye APPROVE_FOR_SUBMISSION) |
| ra.sub@cert.local | OK | Regulatory Submitter | 7 (incluye DOSSIER.SUBMIT) |
| ra.view@cert.local | OK | Regulatory Viewer | 8 (solo READ + AUDIT/TENANT/REPORT) |

Evidencia: `docs/regulatory-affairs/security/evidence/role-permission-claims.json`

---

## 4. Resultado del flujo E2E por rol

Ejecutado con **Playwright real** (Chrome), no simulado:

```text
npx playwright test tests/regulatory-sod-roles.spec.ts
→ 1 passed (13.5s)
```

### ROL 1 — RA Specialist (`ra.spec`)

| Acción | Resultado |
|---|---|
| Login UI | PASS |
| Crear producto + dossier | PASS → estado `Planning` |
| Avanzar flujo docs/ensamble | PASS → `WaitingManufacturerDocuments` → `DocumentsReceived` → `Assembling` → `ReadyForSubmission` |
| Self-review denegado (SoD) | PASS HTTP 400 |
| Approve/submit denegados | PASS HTTP 403 |
| Logout | PASS |

### ROL 2 — RA Reviewer (`ra.rev`)

| Acción | Resultado |
|---|---|
| Login UI | PASS |
| Localizar mismo dossier | PASS |
| Aceptar requisitos críticos | PASS |
| Approve/submit denegados | PASS HTTP 403 |
| Logout | PASS |

### ROL 3 — RA Approver (`ra.appr`)

| Acción | Resultado |
|---|---|
| Login UI | PASS |
| Aprobación interna | PASS → `ApprovedForSubmission` |
| Submit denegado (SoD) | PASS HTTP 403 |
| Logout | PASS |

### ROL 4 — RA Submitter (`ra.sub`)

| Acción | Resultado |
|---|---|
| Login UI | PASS |
| Approve interno denegado | PASS HTTP 403 |
| Registrar sometimiento | PASS → `Submitted` |
| Logout | PASS |

### ROL 5 — RA Viewer (`ra.view`)

| Acción | Resultado |
|---|---|
| Login UI | PASS |
| Consultar expediente | PASS HTTP 200 (`Submitted`) |
| Crear / editar / revisar / aprobar / someter / cancelar | **TODOS bloqueados** HTTP 403 |
| Logout | PASS |

Evidencia pasos: `docs/regulatory-affairs/security/evidence/browser-sod-steps.json` (`failed: 0`)  
Screenshots: `browser-specialist|reviewer|approver|submitter|viewer.png`  
Artifacts Playwright: `artifacts/e2e/test-output/regulatory-sod-roles-…` (screenshot, video.webm, trace.zip)

---

## 5. Evidencia de transición de estados (mismo expediente)

```text
Planning
→ WaitingManufacturerDocuments
→ DocumentsReceived
→ Assembling
→ ReadyForSubmission
→ ApprovedForSubmission
→ Submitted
```

Dossier ID: `c525eafc-f1ae-4a31-83c7-5b32e172de71`

### Nota arquitectónica (resolución CT/RS)

Registrar la **resolución externa** (`Submitted` → `UnderAuthorityReview` → `Approved` → `Closed`) **no** es permiso del Submitter ni del Viewer. Requiere rol Manager/Quality Manager (`REGULATORY.DOSSIER.APPROVE` / decisión externa) por diseño SoD.  
El prompt pedía “registrar resolución” al Submitter; el sistema lo bloquea correctamente. El flujo SoD de certificación termina en `Submitted` + Viewer read-only, que es el happy-path oficial del suite `regulatory-sod-roles`.

---

## 6. Validación RBAC / permisos

| Control | Evidencia |
|---|---|
| Specialist no se auto-revisa | 400 |
| Specialist/Reviewer no aprueban internamente | 403 |
| Reviewer/Approver no someten (cuando no corresponde) | 403 |
| Approver no somete | 403 |
| Submitter no aprueba internamente | 403 |
| Viewer solo lectura | 200 read / 403 mutate |
| Claims JWT alineados a `RoleCatalog` | ver `role-permission-claims.json` |

---

## 7. Validación Viewer (solo lectura)

Bloqueos verificados:

- create dossier → 403  
- edit/transition → 403  
- review requirement → 403  
- approve-for-submission → 403  
- submit → 403  
- cancel/delete-like transition → 403  

Consulta del mismo expediente → 200.

---

## 8. Problemas encontrados y correcciones

| Ítem | Severidad | Acción |
|---|---|---|
| Contraseñas RA desalineadas / reset previo sobre usuario incorrecto | Operativo | Corregido con reset oficial API para los 5 usuarios |
| E2E aún usaba `CertRaPass!2026` | Test debt | Actualizado a `OwnerStart!2026` |
| Cobertura Viewer incompleta (solo create) | Test gap | Ampliada a edit/review/approve/submit/delete |
| Resolución externa fuera del rol Submitter | Diseño (no defect) | Documentado; no se alteró RBAC |

**No** se deshabilitó seguridad ni se alteraron permisos de producción.

---

## 9. Evidencia de compilación

```text
dotnet build src/Compliance360.Web/Compliance360.Web.csproj -c Release
→ Build succeeded. 0 Warning(s). 0 Error(s).
```

App local levantada con Development Bootstrap → **HEALTHY / Ready for Functional Testing**.

---

## 10. Evidencia Browser Automation

| Artefacto | Ubicación |
|---|---|
| Resultado Playwright | `1 passed` — `regulatory-sod-roles.spec.ts` (11.1s test / 13.5s suite) |
| Steps JSON | `docs/regulatory-affairs/security/evidence/browser-sod-steps.json` |
| Screenshots por rol | `docs/regulatory-affairs/security/evidence/browser-*.png` |
| Video + trace | `artifacts/e2e/test-output/regulatory-sod-roles-…/` |
| Password reset report | `docs/regulatory-affairs/security/evidence/password-reset-validation.json` |
| JWT claims | `docs/regulatory-affairs/security/evidence/role-permission-claims.json` |

---

## 11. Veredicto

| Criterio | Estado |
|---|---|
| Cambio de contraseñas por mecanismo correcto | PASS |
| Login 5/5 con `OwnerStart!2026` | PASS |
| Flujo E2E un solo expediente por roles SoD | PASS |
| Transiciones hasta `Submitted` | PASS |
| RBAC / SoD negativos | PASS |
| Viewer read-only | PASS |
| Compilación Release | PASS |
| Resolución CT/RS por Submitter | **N/A por diseño** (requiere Manager) |

### Veredicto final: **PASS**

**Nivel de preparación:** apto para **certificación funcional local SoD / lab UAT** del flujo Specialist→Reviewer→Approver→Submitter→Viewer.  
Para cierre productivo end-to-end incluyendo CT/RS, falta un paso adicional con rol Manager (fuera del set de 5 usuarios RA de este prompt), sin que ello invalide el PASS del alcance SoD solicitado.
