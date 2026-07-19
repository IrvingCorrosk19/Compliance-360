# Manual funcional — Storage Administrator

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Storage Administrator

## 1. Propósito del rol

Administra **proveedores de almacenamiento** del tenant (local, Azure Blob, etc.): creación, prueba, activación y eliminación. Separación de funciones (SoD): **no administra SMTP/notificaciones**.

- **Resuelve:** infraestructura para adjuntos documentales y archivos.
- **Handoff hacia:** Document Controller, Supplier Manager (adjuntos).

## 2. Precondiciones

- Tenant Active (`e2e_provision.ps1`)
- Usuario `storage@alimentos-premium.test`
- Etapas 02–03 completadas

## 3. Credenciales de prueba

| Campo | Valor |
|-------|-------|
| **Email** | `storage@alimentos-premium.test` |
| **Contraseña** | `e2e/testdata.json` |
| **Tenant** | ID en `e2e/testdata.json` |

## 4. Datos de prueba

| Elemento | Valor |
|----------|-------|
| Provider Local | Nombre: `Storage Local MFT`; ruta base por defecto |
| Provider Azure | PENDING THIRD-PARTY — requiere connection string real |

## 5. Casos de prueba

### Login v2

1. `storage@alimentos-premium.test` → **Siguiente** → **Continuar** → contraseña → **Iniciar sesion**

---

### TC-SA-001 — Login Storage Administrator

**Pasos:**

1. Login v2 completo.
2. Verifique acceso a `#/configuration`.
3. Confirme menú **Configuration** o **Enterprise → Configuration** visible.

**Resultado esperado:** Sesión activa; Configuration accesible.

**Auditoría:** AuthenticationSuccess

---

### TC-SA-002 — Abrir Configuration Center

| **Ruta** | `#/configuration` |

**Pasos:**

1. Navegue a `#/configuration`.
2. Verifique encabezado de configuración de proveedores.
3. Localice sección **Storage Providers**.
4. Confirme botón **Crear Storage Local** visible.

**Resultado esperado:** Pantalla configuration sin error 5xx.

---

### TC-SA-003 — Crear Storage Local

| **Prioridad** | Crítica |

**Pasos:**

1. En `#/configuration`, presione **Crear Storage Local**.
2. Complete nombre del provider si se solicita (ej. `Storage Local MFT`).
3. Confirme creación con toast de éxito.
4. Verifique fila nueva en tabla Storage Providers.
5. Anote ID del provider si se muestra.

**Resultado esperado:** Provider local creado y listado.

**Auditoría:** StorageProviderCreated

---

### TC-SA-004 — Probar primer Storage Provider

**Pasos:**

1. Con al menos un provider en lista, presione **Probar primer Storage Provider**.
2. Espere resultado de prueba (toast ok/error).
3. Si botón estaba disabled sin providers, ejecute TC-SA-003 primero.

**Resultado esperado:** Prueba de conectividad exitosa para storage local.

**Auditoría:** StorageProviderTested

---

### TC-SA-005 — Verificar botón Email SMTP NO disponible (SoD)

**Pasos:**

1. En `#/configuration`, busque botón **Crear Email SMTP**.
2. Confirme que **NO** está visible para Storage Administrator.
3. Documente screenshot.

**Resultado esperado:** SoD — solo storage, no notification admin.

---

### TC-SA-006 — Sin acceso crear documentos

**Pasos:**

1. Navegue a `#/documents`.
2. Verifique **Modo solo lectura** o formulario `#module-action-form` ausente.

**Resultado esperado:** No crea documentos.

---

### TC-SA-007 — Listar providers existentes

**Pasos:**

1. `#/configuration` → sección Storage.
2. Cuente providers activos.
3. Verifique columnas: nombre, tipo, estado, acciones.

**Resultado esperado:** Listado coherente post TC-SA-003.

---

### TC-SA-008 — Intento eliminar provider en uso (negativo)

**Pasos:**

1. Si existe provider vinculado a archivos, intente eliminar.
2. Observe mensaje de error o bloqueo.

**Resultado esperado:** Protección contra borrado destructivo; mensaje claro.

---

### TC-SA-009 — Audit Trail storage

**Pasos:**

1. `#/audit-trail`.
2. Busque StorageProviderCreated / StorageProviderTested.
3. **Exportar** evidencia.

**Resultado esperado:** Eventos trazables.

---

### TC-SA-010 — Sin acceso Tenant Administration usuarios

**Pasos:**

1. `#/tenant-administration` → tab **Usuarios**.
2. Verifique lectura restringida o tab no editable.

**Resultado esperado:** Sin TENANT.USERS manage.

---

### TC-SA-011 — Navegación directa API storage (positivo)

**Pasos:**

1. DevTools → GET `/api/v1/tenants/{tenantId}/storage/providers` con Bearer token.
2. Confirme HTTP 200 y lista incluye provider de TC-SA-003.

**Resultado esperado:** API coherente con UI.

---

### TC-SA-012 — Logout

**Pasos:** **Salir** → login screen.

## 6. Validaciones negativas

- Crear storage duplicado → validación.
- Probar sin providers → botón disabled.
- Crear Email SMTP → no visible (TC-SA-005).
- Crear documento → bloqueado.

## 7. Validación visual

- Botones **Crear Storage Local** (primary) y **Probar primer Storage Provider**.
- Tabla providers con status pills.
- Loaders durante prueba de provider.

## 8. Permisos esperados

**Debe:** STORAGE.CREATE/UPDATE/DELETE, TENANT.STORAGE, `#/configuration`.

**No debe:** NOTIFICATION.ADMIN, DOCUMENT.CREATE, TENANT.USERS.

## 9. Auditoría

StorageProviderCreated, StorageProviderTested, StorageProviderDeleted con actor storage@.

## 10. Criterio de aprobación

TC-SA-001, TC-SA-003, TC-SA-004, TC-SA-005 PASS obligatorios.

## 11. Qué aprendí

Sin storage configurado, los adjuntos documentales fallan silenciosamente o en upload. Esta etapa es **prerrequisito** para Document Controller con archivos.

## 12. Referencias

- UI: `#/configuration`, botones exactos en `app.js`
- Siguiente: `05_NOTIFICATION_ADMINISTRATOR_FUNCTIONAL_TESTS.md`
- Journey upload: `scripts/customer_journey.ps1` (Upload-File)
