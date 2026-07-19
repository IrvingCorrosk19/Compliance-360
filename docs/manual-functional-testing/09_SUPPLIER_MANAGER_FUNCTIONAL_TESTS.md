# Manual funcional — Supplier Manager

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Supplier Manager

## 1. Propósito del rol

Gestiona **proveedores**, evaluaciones, homologación y documentación asociada.

## 2. Precondiciones

Storage configurado (manual 04). Usuario `supplier@alimentos-premium.test`.

## 3. Credenciales

| Email | `supplier@alimentos-premium.test` | Contraseña | `e2e/testdata.json` |

## 4. Datos de prueba

| Campo UI | Valor |
|----------|-------|
| Razon social | Distribuidora Central MFT S.A. |
| Identificacion fiscal | RUC-SUP-MFT-001 |
| Pais | PA |

## 5. Casos de prueba

### Login v2: supplier@ → **Siguiente** → **Continuar** → contraseña → **Iniciar sesion**


### TC-SM-001 — Login Supplier Manager

| Campo | Valor |
|-------|-------|
| **Objetivo** | Acceso al módulo proveedores |
| **Prioridad** | Crítica |
| **Tipo** | Positivo |
| **Precondiciones** | e2e_provision.ps1 |
| **Datos** | supplier@alimentos-premium.test |
| **Ruta inicial** | `http://localhost:5272/` |
| **Menú** | Login |
| **Pantalla** | Enterprise Login |

**Pasos:**

1. Abra `http://localhost:5272/`
2. Ingrese supplier@alimentos-premium.test → **Siguiente**
3. Seleccione Alimentos Premium → **Continuar**
4. Contraseña desde testdata → **Iniciar sesion**
5. Verifique toast éxito
6. Confirme sidebar **Operations** con **Suppliers**

**Resultado final esperado:** Sesión activa

**Auditoría esperada:** AuthenticationSuccess

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-001/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



### TC-SM-002 — Navegar Suppliers Module

| Campo | Valor |
|-------|-------|
| **Objetivo** | Abrir Action Center proveedores |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Precondiciones** | TC-SM-001 PASS |
| **Datos** | — |
| **Ruta inicial** | `#/suppliers` |
| **Menú** | Operations → Suppliers |
| **Pantalla** | Suppliers Module |

**Pasos:**

1. Clic **Suppliers** o navegue `#/suppliers`
2. Espere loader
3. Localice `#module-action-form`
4. Verifique labels **Razon social**, **Identificacion fiscal**, **Pais**
5. Verifique botón **Crear registro real**

**Resultado final esperado:** Formulario proveedores visible

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-002/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



### TC-SM-003 — Crear proveedor

| Campo | Valor |
|-------|-------|
| **Objetivo** | Alta proveedor crítica |
| **Prioridad** | Crítica |
| **Tipo** | Positivo |
| **Precondiciones** | TC-SM-002 PASS |
| **Datos** | Sección 4 |
| **Ruta inicial** | `#/suppliers` |
| **Menú** | Suppliers |
| **Pantalla** | Action Center |

**Pasos:**

1. **Razon social:** Distribuidora Central MFT S.A.
2. **Identificacion fiscal:** RUC-SUP-MFT-001
3. **Pais:** PA
4. Presione **Crear registro real**
5. Espere toast éxito
6. Verifique fila en listado

**Resultado final esperado:** Proveedor creado

**Auditoría esperada:** SupplierCreated

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-003/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



### TC-SM-004 — Segundo proveedor

| Campo | Valor |
|-------|-------|
| **Objetivo** | Segundo registro |
| **Prioridad** | Media |
| **Tipo** | Positivo |
| **Precondiciones** | TC-SM-003 PASS |
| **Datos** | RUC-SUP-MFT-002 |
| **Ruta inicial** | `#/suppliers` |
| **Menú** | Suppliers |
| **Pantalla** | Action Center |

**Pasos:**

1. Razon social: Lacteos del Istmo MFT
2. Identificacion fiscal: RUC-SUP-MFT-002
3. Pais: PA
4. **Crear registro real**

**Resultado final esperado:** Segundo proveedor listado

**Auditoría esperada:** SupplierCreated

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-004/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



### TC-SM-005 — RUC duplicado

| Campo | Valor |
|-------|-------|
| **Objetivo** | Validación unicidad |
| **Prioridad** | Alta |
| **Tipo** | Negativo |
| **Precondiciones** | TC-SM-003 PASS |
| **Datos** | RUC-SUP-MFT-001 |
| **Ruta inicial** | `#/suppliers` |
| **Menú** | Suppliers |
| **Pantalla** | Action Center |

**Pasos:**

1. Reingrese RUC-SUP-MFT-001
2. **Crear registro real**
3. Observe error UI
4. Network ≠ 500

**Resultado final esperado:** Rechazo validación

**Auditoría esperada:** SupplierCreateFailed

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-005/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



### TC-SM-006 — País inválido

| Campo | Valor |
|-------|-------|
| **Objetivo** | maxlength ISO2 |
| **Prioridad** | Media |
| **Tipo** | Negativo |
| **Precondiciones** | TC-SM-002 PASS |
| **Datos** | PAN |
| **Ruta inicial** | `#/suppliers` |
| **Menú** | Suppliers |
| **Pantalla** | Action Center |

**Pasos:**

1. Pais: PAN (3 chars)
2. **Crear registro real**

**Resultado final esperado:** Validación maxlength=2

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-006/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



### TC-SM-007 — Documents lectura

| Campo | Valor |
|-------|-------|
| **Objetivo** | Consulta documentos |
| **Prioridad** | Media |
| **Tipo** | Positivo |
| **Precondiciones** | TC-SM-001 PASS |
| **Datos** | — |
| **Ruta inicial** | `#/documents` |
| **Menú** | Operations → Documents |
| **Pantalla** | Documents |

**Pasos:**

1. Navegue `#/documents`
2. Observe listado
3. Confirme sin **Crear registro real** o Modo solo lectura

**Resultado final esperado:** Solo lectura o sin CREATE

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-007/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



### TC-SM-008 — Sin tenant admin

| Campo | Valor |
|-------|-------|
| **Objetivo** | SoD administración |
| **Prioridad** | Alta |
| **Tipo** | Permisos |
| **Precondiciones** | TC-SM-001 PASS |
| **Datos** | — |
| **Ruta inicial** | `#/tenant-administration` |
| **Menú** | Enterprise |
| **Pantalla** | TAC |

**Pasos:**

1. Navegue `#/tenant-administration`
2. Verifique sin tab **Usuarios** editable

**Resultado final esperado:** Sin TENANT.USERS

**Auditoría esperada:** AuthorizationDenied

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-008/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



### TC-SM-009 — Report Center lectura

| Campo | Valor |
|-------|-------|
| **Objetivo** | Reportes |
| **Prioridad** | Media |
| **Tipo** | Positivo |
| **Precondiciones** | TC-SM-001 PASS |
| **Datos** | — |
| **Ruta inicial** | `#/reports` |
| **Menú** | Command Center → Reports |
| **Pantalla** | Report Center |

**Pasos:**

1. Navegue `#/reports`
2. Verifique **Report Center** visible

**Resultado final esperado:** Lectura reportes

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-009/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



### TC-SM-010 — Audit Trail

| Campo | Valor |
|-------|-------|
| **Objetivo** | Trazabilidad |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Precondiciones** | TC-SM-003 PASS |
| **Datos** | — |
| **Ruta inicial** | `#/audit-trail` |
| **Menú** | Command Center |
| **Pantalla** | Audit Trail |

**Pasos:**

1. Navegue `#/audit-trail`
2. Busque SupplierCreated
3. Presione **Exportar**

**Resultado final esperado:** Evento auditado

**Auditoría esperada:** AuditExported

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-010/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



### TC-SM-011 — Handoff CAPA

| Campo | Valor |
|-------|-------|
| **Objetivo** | Preparar NC |
| **Prioridad** | Baja |
| **Tipo** | Positivo |
| **Precondiciones** | TC-SM-003 PASS |
| **Datos** | Proveedor ID |
| **Ruta inicial** | `#/suppliers` |
| **Menú** | Suppliers |
| **Pantalla** | Listado |

**Pasos:**

1. Anote proveedor para manual 11
2. Documente en notas E2E

**Resultado final esperado:** Handoff documentado

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-011/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



### TC-SM-012 — Logout

| Campo | Valor |
|-------|-------|
| **Objetivo** | Cierre sesión |
| **Prioridad** | Baja |
| **Tipo** | Positivo |
| **Precondiciones** | Sesión activa |
| **Datos** | — |
| **Ruta inicial** | `cualquiera` |
| **Menú** | Topbar |
| **Pantalla** | Salir |

**Pasos:**

1. Presione **Salir**
2. Confirme login

**Resultado final esperado:** Sesión cerrada

**Auditoría esperada:** Logout

**Evidencia:** `artifacts/manual-functional-testing/Supplier-Manager/TC-SM-012/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---



## 6. Validaciones negativas

Ver casos marcados **Negativo** y **Permisos** en sección 5.

## 7. Validación visual

Loaders, toasts, labels exactos del Action Center, listados refrescados post-creación.

## 8. Permisos esperados

Ver `RoleCatalog.cs` para permisos oficiales del rol Supplier Manager.

## 9. Auditoría

Cada mutación exitosa genera evento en `#/audit-trail` con actor, tenantId, correlationId.

## 10. Criterio de aprobación del rol

TC-SM-003 PASS obligatorio. Mínimo 10/12 casos PASS.

## 11. Qué aprendí

Este rol es pieza del programa MFT; ejecute en orden del roadmap `00`.

## 12. Referencias y extensiones API

- URL: `http://localhost:5272`
- Journey: `scripts/customer_journey.ps1`
- Credenciales: `e2e/testdata.json`
