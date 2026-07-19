# Manual funcional — Quality Manager

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Quality Manager

## 1. Propósito del rol

**Aprobador y coordinador** de calidad. Aprueba documentos, workflows, CAPAs, riesgos y fichas técnicas. **No crea datos de negocio** por defecto en UI (SoD). Tiene `DOCUMENT.APPROVE` pero la UI de creación es **solo lectura**.

## 2. Precondiciones

- Documento creado por Document Controller (TC-DC-003) con ID anotado
- CAPA en PendingApproval (manual 11) para TC-QM-CAPA-*
- Riesgo en workflow (manual 12) para TC-QM-RISK-*
- Usuario `quality@alimentos-premium.test`

## 3. Credenciales de prueba

| Campo | Valor |
|-------|-------|
| **Email** | `quality@alimentos-premium.test` |
| **Contraseña** | `e2e/testdata.json` |
| **Tenant ID** | `e2e/testdata.json` |

## 4. Datos de prueba

| Artefacto | Origen |
|-----------|--------|
| Document ID | TC-DC-003 (DOC-MFT-001) |
| Decisión aprobación | `{ "decision": "Approved", "comments": "Aprobado MFT QM" }` |
| CAPA ID | TC-CM-* (manual 11) |
| Risk ID | TC-RM-* (manual 12) |

## 5. Casos de prueba

### TC-QM-001 — Login Quality Manager

**Pasos:**

1. Login v2 con `quality@alimentos-premium.test`.
2. Verifique acceso lectura a `#/documents`, `#/capa`, `#/risks`.
3. Confirme permisos de aprobación en JWT (DevTools decode) — DOCUMENT.APPROVE presente.

**Auditoría:** AuthenticationSuccess

---

### TC-QM-002 — Documents en modo solo lectura para creación

| **Ruta** | `#/documents` |

**Pasos:**

1. Navegue a `#/documents`.
2. Observe Action Center.
3. Verifique mensaje **Modo solo lectura** O ausencia de formulario `#module-action-form` activo.
4. Confirme texto: *Tu rol puede consultar Documents, pero no crear, editar, aprobar ni eliminar registros en este modulo* (o variante).
5. Verifique que botón **Crear registro real** NO está disponible.

**Resultado esperado:** UI create read-only para Quality Manager.

---

### TC-QM-DOC-003 — Aprobar documento vía Swagger/API (Crítica)

| **Prioridad** | Crítica |
| **Tipo** | Positivo (Extensión API) |

**Pasos:**

1. Obtenga Bearer token: login v2 o POST `/api/v1/auth/login` con tenantId + quality@ + contraseña desde testdata.
2. Abra Swagger UI: `http://localhost:5272/swagger`.
3. Autorice con Bearer token.
4. Localice endpoint **POST** `/api/v1/tenants/{tenantId}/documents/{documentId}/decision`.
5. Use tenantId de testdata y documentId de TC-DC-003.
6. Body JSON: `{ "decision": "Approved", "comments": "Aprobado en prueba MFT QM" }`.
7. Ejecute request.
8. Confirme HTTP 200/204.
9. En UI como quality@, recargue `#/documents` y verifique estado Approved.
10. `#/audit-trail` → busque DocumentApproved.

**Resultado esperado:** Documento aprobado; auditoría coherente.

**Auditoría:** DocumentApproved / DocumentDecisionRecorded

---

### TC-QM-DOC-004 — Rechazar documento vía API (negativo controlado)

**Pasos:**

1. Cree documento DOC-MFT-REJ con doccontrol@ (etapa 06).
2. Como QM, POST decision `{ "decision": "Rejected", "comments": "Requiere revision" }`.
3. Verifique estado Rejected en listado.

**Auditoría:** DocumentRejected

---

### TC-QM-005 — Consultar CAPA pendiente de aprobación

| **Ruta** | `#/capa` |

**Pasos:**

1. `#/capa` — liste CAPAs.
2. Localice CAPA en estado PendingApproval (creada en manual 11).
3. Verifique que UI de cierre/aprobación puede ser parcial.
4. Documente CAPA ID para TC-QM-CAPA-006.

---

### TC-QM-CAPA-006 — Aprobar/cerrar CAPA (API si UI parcial)

**Pasos:**

1. Si UI no expone aprobación, use Swagger POST decisión CAPA (endpoint equivalente en API).
2. Body con decisión Approved.
3. Verifique estado en `#/capa`.
4. Audit trail → CapaApproved / CapaClosed.

**Resultado esperado:** CAPA cerrada o aprobada según workflow.

---

### TC-QM-007 — Consultar riesgos pendientes

**Pasos:** `#/risks` → identifique riesgo en workflow de manual 12.

---

### TC-QM-RISK-008 — Aprobar riesgo vía API

**Pasos:**

1. Swagger POST `/api/v1/tenants/{tenantId}/risks/{riskId}/decision` (o endpoint documentado).
2. Decision Approved.
3. Verifique en UI y audit trail.

**Auditoría:** RiskApproved

---

### TC-QM-009 — Report Center lectura y ejecutar

| **Ruta** | `#/reports` |

**Pasos:**

1. `#/reports` — Report Center visible.
2. Verifique botón **Ejecutar** disponible (REPORT.EXECUTE).
3. Ejecute un reporte estándar si existe seed.
4. **No** requiere REPORT.MANAGE para ejecutar.

---

### TC-QM-010 — Sin acceso tenant administration usuarios

**Pasos:** `#/tenant-administration` tab Usuarios → sin permisos de alta.

---

### TC-QM-011 — Audit Trail decisiones QM

**Pasos:** `#/audit-trail` → filtrar DocumentApproved, CapaApproved, RiskApproved → **Exportar**.

---

### TC-QM-012 — Logout

**Pasos:** **Salir**.

## 6. Validaciones negativas

- Crear documento en UI → Modo solo lectura (TC-QM-002).
- Aprobar sin permiso (probar con viewer@) → 403.
- API decision sin token → 401.

## 7. Validación visual

- Empty state **Modo solo lectura** en módulos create.
- Listados documentos/CAPA/riesgos legibles.
- Report Center con **Ejecutar**.

## 8. Permisos esperados

**Debe:** DOCUMENT.APPROVE, CAPA.APPROVE, CAPA.CLOSE, RISK.APPROVE, REPORT.EXECUTE, lectura transversal.

**No debe:** DOCUMENT.CREATE en UI, TENANT.USERS, STORAGE.CREATE.

## 9. Auditoría

Toda decisión API → evento *Approved/Rejected* con actor quality@, comments, entityId.

## 10. Criterio de aprobación

**TC-QM-DOC-003 crítico PASS.** TC-QM-002 confirma SoD UI.

## 11. Qué aprendí

Quality Manager es el **segundo par de ojos** obligatorio. La UI prioriza SoD (no crear); las aprobaciones críticas usan API/Swagger hasta que la UI exponga decisiones.

## 12. Referencias y extensiones API

- **Aprobar documento:** POST `/api/v1/tenants/{id}/documents/{docId}/decision`
- Swagger: `http://localhost:5272/swagger`
- Journey completo: `scripts/customer_journey.ps1`
- Manual relacionado: `08_TECHNICAL_SHEETS_FUNCTIONAL_TESTS.md` (TECHNICALSHEET.APPROVE)
