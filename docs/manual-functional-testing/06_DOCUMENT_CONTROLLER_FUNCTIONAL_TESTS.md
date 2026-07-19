# Manual funcional — Document Controller

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Document Controller

## 1. Propósito del rol

Crea y mantiene **documentos controlados** y sus workflows. **No aprueba documentos** (SoD con Quality Manager). Es el primer rol operativo del ciclo documental.

## 2. Precondiciones

- Infraestructura lista (etapas 04–05 recomendadas)
- Usuario `doccontrol@alimentos-premium.test`
- Storage local creado (etapa 04)

## 3. Credenciales de prueba

| Campo | Valor |
|-------|-------|
| **Email** | `doccontrol@alimentos-premium.test` |
| **Contraseña** | `e2e/testdata.json` |

## 4. Datos de prueba — Crear documento

| Campo UI | Valor |
|----------|-------|
| Nombre / titulo | Procedimiento HACCP Manual MFT |
| Codigo | DOC-MFT-001 |
| Resumen de cambio | Creacion inicial para prueba funcional manual |

## 5. Casos de prueba

### Login v2

`doccontrol@alimentos-premium.test` → **Siguiente** → **Continuar** → contraseña → **Iniciar sesion**

---

### TC-DC-001 — Login Document Controller

**Pasos:**

1. Login v2 completo.
2. Verifique menú **Operations → Documents** o ruta `#/documents`.
3. Confirme que NO ve `#/superadmin-platform`.

**Auditoría:** AuthenticationSuccess

---

### TC-DC-002 — Navegar a Documents Module

| **Ruta** | `#/documents` |

**Pasos:**

1. Haga clic en **Documents** del sidebar o navegue a `#/documents`.
2. Espere carga del listado (puede estar vacío).
3. Localice **Action Center** con formulario.
4. Verifique presencia de `#module-action-form`.

**Resultado esperado:** Módulo documentos cargado; formulario de creación visible.

---

### TC-DC-003 — Crear documento vía Action Center (Crítica)

**Pasos:**

1. En `#/documents`, localice formulario `#module-action-form`.
2. En **Nombre / titulo** ingrese `Procedimiento HACCP Manual MFT`.
3. En **Codigo** ingrese `DOC-MFT-001` (use sufijo `-RUN2` en re-ejecución).
4. En **Resumen de cambio** ingrese `Creacion inicial para prueba funcional manual`.
5. Presione **Crear registro real**.
6. Espere toast de éxito (registro creado correctamente).
7. Verifique que el documento aparece en el listado del tenant.
8. Anote Document ID desde DevTools Network (POST response) si está disponible.

**Resultado esperado:** Documento creado y listado.

**Auditoría:** DocumentCreated

**Evidencia:** Screenshot formulario + listado

---

### TC-DC-004 — Crear segundo documento (código único)

**Pasos:**

1. Repita TC-DC-003 con Codigo `DOC-MFT-002`.
2. Nombre: `Instruccion de Trabajo Limpieza`.

**Resultado esperado:** Segundo registro en listado.

---

### TC-DC-005 — Validación código duplicado (negativo)

**Pasos:**

1. Intente crear documento con Codigo `DOC-MFT-001` ya existente.
2. Presione **Crear registro real**.
3. Observe mensaje de error.

**Resultado esperado:** Error validación; no duplicado; HTTP ≠ 500.

---

### TC-DC-006 — Campos obligatorios vacíos (negativo)

**Pasos:**

1. Deje **Nombre / titulo** vacío.
2. Presione **Crear registro real**.
3. Observe validación HTML5 o mensaje UI.

**Resultado esperado:** No se crea registro.

---

### TC-DC-007 — Sin permiso aprobar documentos (SoD)

**Pasos:**

1. Abra documento creado en TC-DC-003.
2. Busque botón **Aprobar** o acción de decisión.
3. Confirme que NO está disponible para Document Controller.
4. Opcional: DevTools POST `/decision` → espere 403.

**Resultado esperado:** Sin DOCUMENT.APPROVE en UI.

**Auditoría:** N/A — acción bloqueada

---

### TC-DC-008 — Modo lectura en configuración tenant

**Pasos:**

1. `#/tenant-administration`.
2. Verifique acceso limitado o denegado a tabs Usuarios/Roles.

**Resultado esperado:** Sin administración de tenant.

---

### TC-DC-009 — Consultar Audit Trail

**Pasos:**

1. `#/audit-trail`.
2. Busque DocumentCreated de TC-DC-003.
3. Verifique actor doccontrol@.
4. **Exportar** evidencia.

---

### TC-DC-010 — Búsqueda en listado documentos

**Pasos:**

1. Use campo búsqueda del módulo si existe.
2. Busque `HACCP`.
3. Confirme filtrado correcto.

---

### TC-DC-011 — Handoff a Quality Manager

**Pasos:**

1. Documente estado del documento post-creación (Draft/Pending).
2. Anote Document ID para manual 07 (TC-QM-DOC-*).
3. Cierre sesión (**Salir**).

**Resultado esperado:** Documento listo para aprobación por Quality Manager vía API.

---

### TC-DC-012 — Logout

**Pasos:** **Salir** → pantalla login.

## 6. Validaciones negativas

- Código duplicado (TC-DC-005).
- Campos vacíos (TC-DC-006).
- Aprobar documento (TC-DC-007) → bloqueado.

## 7. Validación visual

- Labels exactos: **Nombre / titulo**, **Codigo**, **Resumen de cambio**.
- Botón **Crear registro real** (primary).
- Listado refresca tras creación.

## 8. Permisos esperados

**Debe:** DOCUMENT.READ, DOCUMENT.CREATE, DOCUMENT.UPDATE, `#/documents`.

**No debe:** DOCUMENT.APPROVE, TENANT.USERS, superadmin.

## 9. Auditoría

DocumentCreated con name, code, actor, tenantId, correlationId.

## 10. Criterio de aprobación

**TC-DC-003 crítico PASS.** Mínimo 10/12 PASS.

## 11. Qué aprendí

Document Controller inicia el **ciclo de vida documental**. La aprobación es responsabilidad exclusiva del Quality Manager (SoD).

## 12. Referencias y extensiones API

- Formulario: `#module-action-form`, submit **Crear registro real**
- Ciclo completo: `scripts/customer_journey.ps1`
- Aprobación (otro rol): POST `/api/v1/tenants/{id}/documents/{docId}/decision`
- Siguiente: `07_QUALITY_MANAGER_FUNCTIONAL_TESTS.md`
