# Manual funcional — Support Operator

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Support Operator

## 1. Propósito del rol

Rol **break-glass** de plataforma con permiso `PLATFORM.SUPPORT.ACCESS`. Acceso de soporte auditado al tenant. **No existe UI dedicada break-glass** — opera principalmente vía `#/superadmin-platform`.

## 2. Precondiciones

- `scripts/e2e_provision.ps1` (provisiona support via `e2e_provision_support.sql`)
- Usuario en **tenant plataforma** (no tenant negocio)

## 3. Credenciales de prueba

| Campo | Valor |
|-------|-------|
| **Rol** | Support Operator |
| **Email** | `support@compliance360.local` |
| **Tenant ID** | `dc7c46ee-cb25-4ed5-b0b4-800788f7f626` |
| **Contraseña** | Ver `e2e/testdata.json` → sección `support` o `appsettings.Development.json` |

## 4. Datos de prueba

| Elemento | Valor |
|----------|-------|
| Tenant negocio | ID en testdata.json (alimentos-premium-pa) |
| Permiso clave | PLATFORM.SUPPORT.ACCESS |

## 5. Casos de prueba

### TC-SO-001 — Login Support Operator

**Pasos:**

1. Login v2 con `support@compliance360.local`.
2. Tenant plataforma `dc7c46ee-cb25-4ed5-b0b4-800788f7f626`.
3. Verifique sesión en tenant **plataforma**, no Alimentos Premium.

**Auditoría:** AuthenticationSuccess

---

### TC-SO-002 — Menú limitado — solo superadmin-platform visible

**Pasos:**

1. Revise sidebar completo.
2. Confirme **SuperAdmin Platform** / `#/superadmin-platform` accesible.
3. Confirme que módulos operativos del tenant negocio (documents, capa) **NO** aparecen como acceso directo habitual.
4. Documente items visibles vs Platform Administrator.

**Resultado esperado:** Vista restringida a gobierno plataforma; **NO dedicated break-glass UI**.

---

### TC-SO-003 — Abrir SuperAdmin Platform Center

| **Ruta** | `#/superadmin-platform` |

**Pasos:**

1. Navegue a `#/superadmin-platform`.
2. Verifique carga sin error 5xx.
3. Métricas globales visibles.

---

### TC-SO-004 — Sin crear tenant (permiso reducido vs Platform Admin)

**Pasos:**

1. Tab **Tenants** si visible.
2. Intente **Crear tenant**.
3. Si botón ausente o acción 403, documente (Support Operator no tiene PLATFORM.TENANT.CREATE completo).

**Resultado esperado:** Alcance reducido vs Platform Administrator.

---

### TC-SO-005 — Acceso soporte tenant negocio vía API (PLATFORM.SUPPORT.ACCESS)

**Pasos:**

1. DevTools GET `/api/v1/tenants/{tenantNegocioId}/documents` con token support@.
2. Compare con Platform Admin sin SUPPORT (TC-PA-005 → 403).
3. Support Operator **puede** obtener 200 si break-glass activo en backend.

**Auditoría:** SupportAccessGranted / TenantDataRead

---

### TC-SO-006 — Audit Trail plataforma

**Pasos:** `#/audit-trail` → eventos de soporte auditados.

---

### TC-SO-007 — NO acceso tenant administration negocio directo

**Pasos:**

1. Intente login v2 como support@ seleccionando tenant Alimentos Premium.
2. Documente si login multi-tenant lo permite o restringe.

**Resultado esperado:** Support opera en contexto plataforma.

---

### TC-SO-008 — Sin UI break-glass dedicada

**Pasos:**

1. Busque en toda la app pantalla "Break Glass", "Support Access", "Impersonate".
2. Confirme **NO existe pantalla dedicada**.

**Resultado esperado:** Limitación documentada en roadmap — no es defecto.

---

### TC-SO-009 — Búsqueda global plataforma

**Pasos:** Campo búsqueda en superadmin-platform si PLATFORM.SEARCH.

---

### TC-SO-010 — Logout

**Pasos:** **Salir**.

---

### TC-SO-011 — JWT contiene PLATFORM.SUPPORT.ACCESS

**Pasos:** Decode JWT → claim permisos incluye PLATFORM.SUPPORT.ACCESS.

---

### TC-SO-012 — Exportar auditoría (si permitido)

**Pasos:** **Exportar auditoria global CSV** si visible; comparar con Platform Security scope.

## 6. Validaciones negativas

- Sin UI break-glass (TC-SO-008).
- Crear tenant puede estar bloqueado (TC-SO-004).
- Operar como usuario negocio sin mecanismo soporte → 403.

## 7. Validación visual

- Sidebar mínimo vs Platform Admin.
- SuperAdmin Platform Center sin tabs operativos de negocio.

## 8. Permisos esperados

**Debe:** PLATFORM.SUPPORT.ACCESS, PLATFORM.DASHBOARD.READ, PLATFORM.AUDIT.READ, PLATFORM.SEARCH.

**No debe:** PLATFORM.TENANT.CREATE (completo), DOCUMENT.CREATE, UI break-glass dedicada.

## 9. Auditoría

Todo acceso cross-tenant → eventos auditados con actor support@, correlationId.

## 10. Criterio aprobación

TC-SO-001, TC-SO-002, TC-SO-008 PASS.

## 11. Qué aprendí

Support Operator es **break-glass controlado**: poder especial sin pantalla dedicada; cada acceso debe quedar en audit trail.

## 12. Referencias

- Provision: `scripts/e2e_provision.ps1` + `e2e_provision_support.sql`
- Permiso: `PLATFORM.SUPPORT.ACCESS` en `RbacCatalog.cs`
- Manual 01 TC-PA-005 (contraste 403)
