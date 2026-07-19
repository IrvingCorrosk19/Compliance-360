# Manual funcional — Viewer

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Viewer

## 1. Propósito del rol

Acceso **solo lectura** transversal a módulos del tenant. No crea, edita, aprueba ni configura nada.

## 2. Precondiciones

- Datos existentes de etapas 06–14
- Usuario `viewer@alimentos-premium.test`

## 3. Credenciales

| Email | `viewer@alimentos-premium.test` |
| Contraseña | `e2e/testdata.json` |

## 4. Datos de prueba

Documentos, proveedores, CAPA, riesgos creados en manuales previos.

## 5. Casos de prueba

### TC-VW-001 — Login Viewer

**Pasos:** Login v2 → shell con menú Operations visible.

---

### TC-VW-002 — Documents Modo solo lectura

| **Ruta** | `#/documents` |

**Pasos:**

1. `#/documents`.
2. Verifique empty-state **Modo solo lectura**.
3. Texto esperado: *Tu rol puede consultar Documents, pero no crear, editar, aprobar ni eliminar registros en este modulo.*
4. Confirme ausencia de `#module-action-form` activo y botón **Crear registro real**.

**Resultado esperado:** Solo consulta listado existente.

---

### TC-VW-003 — Technical Sheets solo lectura

**Pasos:** `#/technical-sheets` → **Modo solo lectura**.

---

### TC-VW-004 — Suppliers solo lectura

**Pasos:** `#/suppliers` → **Modo solo lectura**; ve proveedores MFT si existen.

---

### TC-VW-005 — Audits solo lectura

**Pasos:** `#/audits` → **Modo solo lectura**.

---

### TC-VW-006 — CAPA solo lectura

**Pasos:** `#/capa` → **Modo solo lectura**.

---

### TC-VW-007 — Risks solo lectura

**Pasos:** `#/risks` → **Modo solo lectura**.

---

### TC-VW-008 — Indicators solo lectura

**Pasos:** `#/indicators` → **Modo solo lectura**.

---

### TC-VW-009 — Reports lectura sin ejecutar manage

**Pasos:**

1. `#/reports` → Report Center visible.
2. Verifique si **Seed standard reports** / **Programar mensual** están ocultos o disabled (sin REPORT.MANAGE).
3. **Ejecutar** puede estar disabled.

**Resultado esperado:** Sin administración de reportes.

---

### TC-VW-010 — Sin tenant administration

**Pasos:** `#/tenant-administration` → acceso denegado o solo lectura mínima.

---

### TC-VW-011 — API POST documento (negativo)

**Pasos:**

1. DevTools POST `/api/v1/tenants/{id}/documents` con token viewer@.
2. HTTP 403 Forbidden.

**Auditoría:** AuthorizationDenied

---

### TC-VW-012 — Audit Trail lectura

**Pasos:** `#/audit-trail` → timeline visible; **Exportar** según AUDIT.READ.

---

### TC-VW-013 — Dashboard lectura

**Pasos:** `#/dashboard` → métricas/widgets sin acciones de mutación.

---

### TC-VW-014 — Logout

**Pasos:** **Salir**.

## 6. Validaciones negativas

- Todos los módulos operativos → Modo solo lectura.
- API mutaciones → 403.
- Configuration/storage → sin acceso admin.

## 7. Validación visual

- Empty-state **Modo solo lectura** consistente en cada módulo.
- Sin botones primary de creación.

## 8. Permisos esperados

**Debe:** *.READ en módulos operativos, AUDIT.READ.

**No debe:** *.CREATE, *.UPDATE, *.APPROVE, *.MANAGE, TENANT.*.

## 9. Auditoría

Viewer no genera eventos de mutación; intentos denegados → AuthorizationDenied.

## 10. Criterio aprobación

TC-VW-002 + TC-VW-011 PASS; mínimo 12/14 casos verificando read-only.

## 11. Qué aprendí

Viewer es el rol de **consulta segura** para dirección, auditores externos o partners sin riesgo de cambio.

## 12. Referencias

- UI: `readOnlyNotice()` en `app.js`
- Siguiente: `16_SUPPORT_OPERATOR_FUNCTIONAL_TESTS.md`
