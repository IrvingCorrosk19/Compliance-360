# Manual funcional — Fichas Técnicas (Technical Sheets)

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Alcance:** RBAC + Quality Manager

## 1. Propósito del rol / módulo

Las **fichas técnicas** documentan especificaciones de producto. **Ningún rol estándar incluye TECHNICALSHEET.CREATE** — el Tenant Administrator debe otorgar el permiso o usar API. Quality Manager tiene TECHNICALSHEET.APPROVE.

## 2. Precondiciones

- TC-TA-007 completado (permiso TECHNICALSHEET.CREATE otorgado) O uso de API
- Usuario tenantadmin@ o quality@ según caso
- Etapa 06–07 recomendada

## 3. Credenciales de prueba

| Escenario | Usuario |
|-----------|---------|
| Crear (con permiso) | `tenantadmin@alimentos-premium.test` (post TC-TA-007) |
| Aprobar | `quality@alimentos-premium.test` |
| Solo lectura | `viewer@alimentos-premium.test` |

Contraseñas: `e2e/testdata.json`

## 4. Datos de prueba

| Campo UI | Valor |
|----------|-------|
| Nombre / titulo | Ficha Tecnica Yogurt Natural MFT |
| Codigo | TS-MFT-001 |
| Descripcion producto | Producto lacteo pasteurizado — prueba MFT |

## 5. Casos de prueba

### TC-TS-001 — Verificar rol estándar sin CREATE (negativo)

**Pasos:**

1. Login como `doccontrol@alimentos-premium.test`.
2. `#/technical-sheets`.
3. Verifique **Modo solo lectura** — sin **Crear registro real**.

**Resultado esperado:** Document Controller no crea fichas técnicas por defecto.

---

### TC-TS-002 — Otorgar TECHNICALSHEET.CREATE (Tenant Admin)

**Pasos:**

1. Login `tenantadmin@alimentos-premium.test`.
2. `#/tenant-administration` → **Roles & Permisos**.
3. Edite rol **Tenant Administrator** o rol custom.
4. Agregue **TECHNICALSHEET.CREATE**.
5. Guarde.

**Auditoría:** RbacPermissionGranted

---

### TC-TS-003 — Crear ficha técnica en UI (Crítica)

**Pasos:**

1. Login tenantadmin@ (con permiso CREATE).
2. `#/technical-sheets`.
3. Formulario `#module-action-form`:
   - **Nombre / titulo:** Ficha Tecnica Yogurt Natural MFT
   - **Codigo:** TS-MFT-001
   - **Descripcion producto:** Producto lacteo pasteurizado — prueba MFT
4. **Crear registro real**.
5. Verifique listado y anote ID.

**Auditoría:** TechnicalSheetCreated

---

### TC-TS-004 — Crear vía API (alternativa)

**Pasos:**

1. Swagger POST `/api/v1/tenants/{tenantId}/technical-sheets` con body name/code/description.
2. HTTP 201/200.
3. Verifique en UI.

---

### TC-TS-005 — Quality Manager — lectura sin create

**Pasos:**

1. Login quality@.
2. `#/technical-sheets` → Modo solo lectura para creación.
3. Ve ficha TS-MFT-001 en listado.

---

### TC-TS-006 — Aprobar ficha técnica vía API

**Pasos:**

1. Swagger POST decisión approve (endpoint technical-sheets decision).
2. quality@ token.
3. Verifique estado Approved.
4. Audit → TechnicalSheetApproved.

---

### TC-TS-007 — Viewer solo lectura

**Pasos:** viewer@ → `#/technical-sheets` → Modo solo lectura completo.

---

### TC-TS-008 — Código duplicado (negativo)

**Pasos:** Reintentar TS-MFT-001 → error validación.

---

### TC-TS-009 — Navegación menú Operations

**Pasos:** Sidebar **Technical Sheets** → ruta `#/technical-sheets` coherente.

---

### TC-TS-010 — Audit Trail

**Pasos:** `#/audit-trail` → TechnicalSheetCreated → **Exportar**.

---

### TC-TS-011 — Handoff a Reporting

**Pasos:** Documente TS-MFT-001 para reportes en manual 14.

---

### TC-TS-012 — Logout

**Pasos:** **Salir**.

## 6. Validaciones negativas

- Sin TECHNICALSHEET.CREATE → no formulario (TC-TS-001).
- Código duplicado (TC-TS-008).
- Viewer create → bloqueado.

## 7. Validación visual

- Campos: **Nombre / titulo**, **Codigo**, **Descripcion producto**.
- Botón **Crear registro real** solo con permiso.

## 8. Permisos esperados

| Rol | CREATE | APPROVE |
|-----|--------|---------|
| Tenant Admin (post grant) | Sí | No |
| Quality Manager | No UI | Sí API |
| Viewer | No | No |

## 9. Auditoría

TechnicalSheetCreated, TechnicalSheetApproved.

## 10. Criterio de aprobación

TC-TS-001 + TC-TS-003 o TC-TS-004 PASS.

## 11. Qué aprendí

Fichas técnicas demuestran que **RBAC es configurable** — los roles estándar no cubren todo; Tenant Admin debe otorgar permisos explícitos.

## 12. Referencias

- Ruta: `#/technical-sheets`
- RoleCatalog: Quality Manager tiene READ+APPROVE, no CREATE
- Manual 02 TC-TA-007
