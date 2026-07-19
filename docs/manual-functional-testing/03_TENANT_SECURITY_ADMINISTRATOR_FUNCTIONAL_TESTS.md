# Manual funcional — Tenant Security Administrator

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Tenant Security Administrator

## 1. Propósito del rol

Gestiona la **postura de seguridad del tenant**: MFA, políticas de contraseña, timeouts de sesión, lockout, IP whitelist, SSO, dominios, webhooks y API keys. No administra usuarios de negocio ni opera módulos documentales.

- **Resuelve:** hardening del tenant, cumplimiento de políticas de acceso.
- **Handoff desde:** Tenant Administrator (usuario creado).
- **Handoff hacia:** todos los roles (políticas aplican al login).

## 2. Precondiciones

- `scripts/e2e_provision.ps1` ejecutado
- Usuario `security@alimentos-premium.test` activo
- Etapa 02 completada (tenant Active)
- Tenant ID en `e2e/testdata.json`

## 3. Credenciales de prueba

| Campo | Valor |
|-------|-------|
| **Rol** | Tenant Security Administrator |
| **Email** | `security@alimentos-premium.test` |
| **Tenant** | ID en `e2e/testdata.json` |
| **Contraseña** | Ver `e2e/testdata.json` |

## 4. Datos de prueba

| Campo | Valor sugerido |
|-------|----------------|
| requireMfa | false (para pruebas) / true (validación MFA) |
| sessionTimeoutMinutes | 60 |
| lockoutMaxFailedAttempts | 5 |
| ipWhitelist | `192.168.1.0/24, 10.0.0.1/32` |
| changeReason | Ajuste manual MFT |

## 5. Casos de prueba

### Procedimiento de login (Enterprise v2)

1. Abra `http://localhost:5272/`
2. Ingrese `security@alimentos-premium.test` → **Siguiente**
3. Seleccione tenant **Alimentos Premium** → **Continuar**
4. Contraseña desde `e2e/testdata.json` → **Iniciar sesion**

---

### TC-TSA-001 — Login Tenant Security Administrator

| Campo | Valor |
|-------|-------|
| **Objetivo** | Validar acceso al centro de seguridad del tenant |
| **Prioridad** | Crítica |
| **Tipo** | Positivo |
| **Precondiciones** | Usuario provisionado |
| **Datos** | security@alimentos-premium.test |
| **Ruta inicial** | `http://localhost:5272/` |
| **Menú** | Login |
| **Pantalla** | Enterprise Login |

**Pasos:**

1. Complete login v2 con credenciales del rol.
2. Verifique toast **Sesion iniciada correctamente.**
3. En sidebar confirme acceso a **Security** y/o **Tenant Administration** (tab Seguridad).
4. Confirme que NO ve formularios de creación en `#/documents`.

**Resultado final esperado:** Sesión activa; menú de seguridad accesible.

**Auditoría esperada:** AuthenticationSuccess

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Security-Administrator/TC-TSA-001/`

---

### TC-TSA-002 — Abrir panel Seguridad en TAC

| Campo | Valor |
|-------|-------|
| **Objetivo** | Navegar al formulario de políticas de seguridad |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Precondiciones** | TC-TSA-001 PASS |
| **Ruta inicial** | `#/tenant-administration` |
| **Pantalla** | Tab **Seguridad** |

**Pasos:**

1. Navegue a `#/tenant-administration`.
2. Haga clic en tab **Seguridad**.
3. Verifique encabezado **Seguridad** con score de seguridad.
4. Confirme campos visibles: **MFA obligatorio**, **Session Timeout (min)**, **Lockout intentos**, **IP Whitelist**.
5. Localice botón **Guardar seguridad**.

**Resultado final esperado:** Formulario `#tenant-security-form` completo y editable.

**Auditoría esperada:** TenantSecurityViewed

---

### TC-TSA-003 — Configurar session timeout y lockout

| Campo | Valor |
|-------|-------|
| **Objetivo** | Persistir políticas de sesión y bloqueo |
| **Prioridad** | Crítica |
| **Tipo** | Positivo |
| **Datos** | sessionTimeoutMinutes=60; lockoutMaxFailedAttempts=5 |

**Pasos:**

1. Tab **Seguridad**.
2. Desmarque **MFA obligatorio** si está activo (entorno de prueba).
3. En **Session Timeout (min)** ingrese `60`.
4. En **Lockout intentos** ingrese `5`.
5. En **Motivo del cambio** ingrese `Ajuste MFT TC-TSA-003`.
6. Presione **Guardar seguridad**.
7. Espere toast de éxito.
8. Recargue página y confirme valores persistidos.

**Resultado final esperado:** Políticas guardadas; score de seguridad actualizado.

**Auditoría esperada:** TenantSecurityUpdated

---

### TC-TSA-004 — Configurar IP Whitelist

| Campo | Valor |
|-------|-------|
| **Objetivo** | Restricción de acceso por IP (CIDR) |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Datos** | `192.168.1.0/24, 10.0.0.1/32` |

**Pasos:**

1. Tab **Seguridad**.
2. En **IP Whitelist** pegue `192.168.1.0/24, 10.0.0.1/32`.
3. Presione **Guardar seguridad**.
4. Verifique persistencia tras recarga.
5. Documente IP local del tester para prueba negativa posterior.

**Resultado final esperado:** Whitelist guardada.

**Auditoría esperada:** TenantSecurityUpdated (IpWhitelist)

---

### TC-TSA-005 — Acceso a ruta #/security

| Campo | Valor |
|-------|-------|
| **Objetivo** | Validar Security Center dedicado |
| **Prioridad** | Media |
| **Ruta inicial** | `#/security` |

**Pasos:**

1. Navegue directamente a `#/security`.
2. Espere carga sin error 5xx.
3. Verifique paneles de MFA, SSO, dominios o API keys según permisos.
4. Confirme coherencia con tab Seguridad del TAC.

**Resultado final esperado:** Security Center accesible.

---

### TC-TSA-006 — Sin permiso crear documentos

| Campo | Valor |
|-------|-------|
| **Objetivo** | SoD — rol seguridad no opera negocio |
| **Prioridad** | Alta |
| **Tipo** | Permisos / Negativo |
| **Ruta inicial** | `#/documents` |

**Pasos:**

1. Navegue a `#/documents`.
2. Observe Action Center.
3. Verifique **Modo solo lectura** o ausencia de `#module-action-form` activo.
4. Confirme que botón **Crear registro real** NO está disponible.

**Resultado final esperado:** Solo lectura en módulo documentos.

---

### TC-TSA-007 — Validación session timeout fuera de rango

| Campo | Valor |
|-------|-------|
| **Objetivo** | Validación de límites numéricos |
| **Prioridad** | Media |
| **Tipo** | Negativo |
| **Datos** | sessionTimeoutMinutes=2 (menor a mínimo 5) |

**Pasos:**

1. Tab **Seguridad**.
2. Ingrese `2` en **Session Timeout (min)**.
3. Presione **Guardar seguridad**.
4. Observe mensaje de validación o rechazo del navegador (min=5).

**Resultado final esperado:** Valor no persistido; error claro.

---

### TC-TSA-008 — Habilitar MFA obligatorio (opcional entorno aislado)

| Campo | Valor |
|-------|-------|
| **Objetivo** | Verificar toggle MFA |
| **Prioridad** | Media |
| **Tipo** | Positivo / PENDING si sin TOTP |
| **Datos** | requireMfa=true |

**Pasos:**

1. Marque **MFA obligatorio**.
2. Presione **Guardar seguridad**.
3. Cierre sesión (**Salir**).
4. Intente login con usuario de prueba.
5. Documente si solicita código MFA.

**Resultado final esperado:** MFA requerido en login siguiente; o PENDING THIRD-PARTY si TOTP no configurado.

**Auditoría esperada:** TenantSecurityUpdated (RequireMfa=true)

---

### TC-TSA-009 — Consultar Audit Trail de cambios de seguridad

| Campo | Valor |
|-------|-------|
| **Objetivo** | Trazabilidad de cambios |
| **Prioridad** | Alta |
| **Ruta inicial** | `#/audit-trail` |

**Pasos:**

1. Navegue a `#/audit-trail`.
2. Busque eventos de TC-TSA-003 y TC-TSA-004.
3. Verifique actor `security@alimentos-premium.test`.
4. Presione **Exportar** y guarde evidencia.

**Resultado final esperado:** TenantSecurityUpdated visible en timeline.

---

### TC-TSA-010 — Sin acceso SuperAdmin Platform

| Campo | Valor |
|-------|-------|
| **Objetivo** | Aislamiento rol tenant vs plataforma |
| **Prioridad** | Alta |
| **Tipo** | Seguridad |
| **Ruta inicial** | `#/superadmin-platform` |

**Pasos:**

1. Intente navegar a `#/superadmin-platform`.
2. Observe redirección, pantalla vacía o mensaje de acceso denegado.
3. En DevTools, GET `/api/v1/platform/tenants` → espere 403.

**Resultado final esperado:** Sin acceso a gobierno de plataforma.

---

### TC-TSA-011 — Logout

**Pasos:** **Salir** → confirmar pantalla login.

### TC-TSA-012 — Restaurar whitelist vacía (limpieza)

**Pasos:** Tab Seguridad → borrar **IP Whitelist** → **Guardar seguridad** → verificar acceso normal.

## 6. Validaciones negativas

- Session timeout &lt; 5 o &gt; 1440 → rechazo.
- IP whitelist con formato inválido → error validación.
- Intento crear documento → bloqueado (TC-TSA-006).
- Acceso plataforma → 403 (TC-TSA-010).

## 7. Validación visual

- Score de seguridad con pill ok/warn.
- Toggle MFA y Trusted Devices alineados.
- Textarea IP Whitelist con help text CIDR.
- Botón **Guardar seguridad** primario visible.

## 8. Permisos esperados

**Debe poder:** `#/tenant-administration` (Seguridad), `#/security`, `#/audit-trail`, lectura tenant.

**No debe poder:** DOCUMENT.CREATE, PLATFORM.*, superadmin-platform operativo.

## 9. Auditoría

Cada **Guardar seguridad** → `TenantSecurityUpdated` con actor, changeReason, snapshot de settings.

## 10. Criterio de aprobación del rol

TC-TSA-001, TC-TSA-003, TC-TSA-006, TC-TSA-009 PASS obligatorios. Mínimo 10/12 PASS.

## 11. Qué aprendí

Tenant Security Administrator define **las reglas del juego** para todos los demás usuarios. Cambios aquí afectan login, lockout y MFA de todo el tenant.

## 12. Referencias y extensiones API

- Campos API: `requireMfa`, `sessionTimeoutMinutes`, `lockoutMaxFailedAttempts`, `ipWhitelist`
- Siguiente: `04_STORAGE_ADMINISTRATOR_FUNCTIONAL_TESTS.md`
- Roadmap: `00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md`
