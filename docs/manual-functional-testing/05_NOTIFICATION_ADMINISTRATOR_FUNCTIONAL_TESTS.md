# Manual funcional — Notification Administrator

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Notification Administrator

## 1. Propósito del rol

Administra **notificaciones, plantillas y proveedores SMTP** del tenant. SoD: **no administra storage**. Habilita alertas de workflow, vencimientos y reportes programados.

## 2. Precondiciones

- Tenant Active; usuario `notifications@alimentos-premium.test`
- Etapa 04 completada (storage independiente)

## 3. Credenciales de prueba

| Campo | Valor |
|-------|-------|
| **Email** | `notifications@alimentos-premium.test` |
| **Contraseña** | `e2e/testdata.json` |

## 4. Datos de prueba

| Elemento | Valor |
|----------|-------|
| SMTP Local/Dev | Host: localhost o sandbox; Puerto: 1025 (MailHog) si disponible |
| Plantilla | Nombre: `MFT-Alert-Test` |

## 5. Casos de prueba

### TC-NA-001 — Login Notification Administrator

**Pasos:**

1. Login v2 con `notifications@alimentos-premium.test`.
2. Verifique `#/configuration` accesible.
3. Confirme acceso a sección notificaciones si existe ruta dedicada.

**Auditoría:** AuthenticationSuccess

---

### TC-NA-002 — Abrir Configuration — Email Providers

**Pasos:**

1. `#/configuration`.
2. Localice sección **Email Providers** / notificaciones.
3. Verifique botón **Crear Email SMTP** visible.

**Resultado esperado:** Botón SMTP visible (contraste con Storage Admin).

---

### TC-NA-003 — Crear Email SMTP (dev/sandbox)

| **Prioridad** | Crítica |
| **Tipo** | Positivo / PENDING THIRD-PARTY |

**Pasos:**

1. Presione **Crear Email SMTP**.
2. Complete host, puerto, usuario, remitente según entorno dev.
3. Guarde provider.
4. Verifique en tabla Email Providers.

**Resultado esperado:** Provider creado; o PENDING si no hay SMTP local.

**Auditoría:** EmailProviderCreated

---

### TC-NA-004 — Verificar botón Storage NO disponible (SoD)

**Pasos:**

1. `#/configuration`.
2. Confirme **Crear Storage Local** NO visible para este rol.

**Resultado esperado:** SoD respetado.

---

### TC-NA-005 — Probar envío de notificación (si UI expone)

**Pasos:**

1. Busque acción **Probar** o **Enviar prueba** en provider SMTP.
2. Ejecute prueba a email del tester.
3. Verifique en bandeja sandbox o logs.

**Resultado esperado:** Toast éxito o PENDING THIRD-PARTY.

---

### TC-NA-006 — Sin crear documentos

**Pasos:** `#/documents` → **Modo solo lectura**.

---

### TC-NA-007 — Consultar plantillas de notificación

**Pasos:**

1. Navegue sección templates en configuration o módulo notifications.
2. Liste plantillas existentes.
3. Documente nombres y tipos.

**Resultado esperado:** Lectura/gestión según NOTIFICATION.TEMPLATE.

---

### TC-NA-008 — Validación SMTP inválido (negativo)

**Pasos:**

1. Crear provider con host `invalid-host-xyz`.
2. Ejecutar prueba de envío.
3. Observe error claro, no HTTP 500.

**Resultado esperado:** Fallo controlado.

---

### TC-NA-009 — Audit Trail notificaciones

**Pasos:** `#/audit-trail` → buscar EmailProviderCreated → **Exportar**.

---

### TC-NA-010 — Sin acceso superadmin-platform

**Pasos:** `#/superadmin-platform` → acceso denegado.

---

### TC-NA-011 — API notifications (lectura)

**Pasos:** DevTools GET providers/notifications → HTTP 200.

---

### TC-NA-012 — Logout

**Pasos:** **Salir**.

## 6. Validaciones negativas

- SMTP inválido → error controlado.
- Storage create → no visible.
- Document create → bloqueado.

## 7. Validación visual

- **Crear Email SMTP** visible; **Crear Storage Local** ausente.
- Tabla email providers con estado.

## 8. Permisos esperados

**Debe:** NOTIFICATION.*, TENANT.NOTIFICATIONS, configuration email section.

**No debe:** STORAGE.CREATE, DOCUMENT.CREATE.

## 9. Auditoría

EmailProviderCreated, NotificationSent (si aplica).

## 10. Criterio de aprobación

TC-NA-001, TC-NA-002, TC-NA-004 PASS obligatorios; TC-NA-003 o PENDING aceptable.

## 11. Qué aprendí

Las alertas de aprobación documental y reportes programados dependen de un SMTP funcional o sandbox.

## 12. Referencias

- `#/configuration` — botón **Crear Email SMTP**
- Siguiente: `06_DOCUMENT_CONTROLLER_FUNCTIONAL_TESTS.md`
