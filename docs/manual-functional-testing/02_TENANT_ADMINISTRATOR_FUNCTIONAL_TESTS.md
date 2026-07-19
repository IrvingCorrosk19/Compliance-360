# Manual funcional — Tenant Administrator

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Tenant Administrator

## 1. Propósito del rol

Administra el perfil del tenant, usuarios, roles, branding, dominios y licenciamiento. **No crea documentos, CAPA ni riesgos** por defecto (separación de funciones).

- **Resuelve:** alta de usuarios operativos, RBAC, identidad visual, estado del tenant.
- **Handoff a:** Tenant Security Administrator, Storage/Notification Administrators, roles operativos.

## 2. Precondiciones

- Tenant negocio provisionado: `powershell -ExecutionPolicy Bypass -File scripts/e2e_provision.ps1`
- `e2e/testdata.json` generado con tenantId y usuarios
- Etapa 01 (Platform Admin) completada o tenant ya existente

## 3. Credenciales de prueba

| Campo | Valor |
|-------|-------|
| **Rol** | Tenant Administrator |
| **Email** | `tenantadmin@alimentos-premium.test` |
| **Tenant** | ID en `e2e/testdata.json` (slug `alimentos-premium-pa`) |
| **Contraseña** | Ver `e2e/testdata.json` (sección users → Tenant Administrator) |

## 4. Datos de prueba

| Caso | Datos |
|------|-------|
| Información General | City: Panama; Phone: +507-6000-0000; Email: contacto@alimentos-premium.test |
| Branding | Primary Color: #1769aa; Logo URI: https://example.com/logo.png |
| Usuario nuevo | Email: doccontrol.mft@alimentos-premium.test; Rol: Document Controller |
| Permiso extra | TECHNICALSHEET.CREATE para rol Quality Manager (manual 08) |

## 5. Casos de prueba


### Procedimiento de login (Enterprise v2)

1. Abra `http://localhost:5272/`
2. Confirme badge **Enterprise Login** y título **Compliance 360 Enterprise**
3. Ingrese el correo indicado en **Correo electronico**
4. Presione **Siguiente** (loader: *Identificando...*)
5. Si aparece selección de organización, elija el tenant correcto y **Continuar**
6. Ingrese contraseña (desde `e2e/testdata.json` o `appsettings.Development.json` — **no documentada aquí**)
7. Presione **Iniciar sesion**
8. **Resultado esperado:** toast **Sesion iniciada correctamente.**; shell con sidebar **Compliance 360 / Enterprise Edition**



### TC-TA-001 — Login Tenant Administrator

| Campo | Valor |
|-------|-------|
| **Objetivo** | Acceso al Tenant Administration Center |
| **Prioridad** | Crítica |
| **Tipo** | Positivo |
| **Precondiciones** | e2e_provision.ps1 ejecutado |
| **Datos** | tenantadmin@alimentos-premium.test |
| **Ruta inicial** | `http://localhost:5272/` |
| **Menú** | Login → Dashboard |
| **Pantalla** | Enterprise Shell |

**Pasos:**

1. Abra `http://localhost:5272/`
2. Login v2 con email tenantadmin@alimentos-premium.test
3. Seleccione tenant **Alimentos Premium** si aplica → **Continuar**
4. Ingrese contraseña desde testdata.json
5. Presione **Iniciar sesion**
6. Verifique toast **Sesion iniciada correctamente.**
7. Confirme menú lateral con **Tenant Administration**

**Resultado final esperado:** Sesión activa en tenant negocio; TAC accesible

**Auditoría esperada:** AuthenticationSuccess

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-001/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-TA-002 — Abrir Tenant Administration Center

| Campo | Valor |
|-------|-------|
| **Objetivo** | Navegación y tabs del TAC |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Precondiciones** | TC-TA-001 PASS |
| **Datos** | — |
| **Ruta inicial** | `#/tenant-administration` |
| **Menú** | Enterprise → Tenant Administration |
| **Pantalla** | TAC |

**Pasos:**

1. Haga clic en **Tenant Administration** del sidebar
2. Espere carga completa
3. Verifique tabs: **Informacion General**, **Branding**, **Seguridad**, **Usuarios**, **Roles & Permisos**
4. Confirme nombre tenant **Alimentos Premium** visible

**Resultado final esperado:** TAC renderizado; todos los tabs accesibles

**Auditoría esperada:** TenantAdministrationViewed

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-002/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-TA-003 — Completar Información General

| Campo | Valor |
|-------|-------|
| **Objetivo** | Datos corporativos del tenant |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Precondiciones** | TC-TA-002 PASS |
| **Datos** | Datos sección 4 |
| **Ruta inicial** | `#/tenant-administration` |
| **Menú** | Tab Informacion General |
| **Pantalla** | Formulario general |

**Pasos:**

1. Seleccione tab **Informacion General**
2. Complete **City**: Panama
3. Complete **Phone**: +507-6000-0000
4. Complete **Email**: contacto@alimentos-premium.test
5. Presione **Guardar informacion general**
6. Espere toast de éxito
7. Recargue página y verifique persistencia

**Resultado final esperado:** Datos guardados y visibles tras recarga

**Auditoría esperada:** TenantUpdated (General)

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-003/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-TA-004 — Configurar Branding

| Campo | Valor |
|-------|-------|
| **Objetivo** | Identidad visual del tenant |
| **Prioridad** | Media |
| **Tipo** | Positivo |
| **Precondiciones** | TC-TA-002 PASS |
| **Datos** | Primary #1769aa |
| **Ruta inicial** | `#/tenant-administration` |
| **Menú** | Tab Branding |
| **Pantalla** | Formulario branding |

**Pasos:**

1. Seleccione tab **Branding**
2. En **Color primario** seleccione #1769aa
3. En **Logo** ingrese https://example.com/logo.png
4. Complete **Nombre mostrado** si está vacío
5. Presione **Guardar branding**
6. Verifique preview de marca actualizado

**Resultado final esperado:** Branding persistido; preview coherente

**Auditoría esperada:** TenantUpdated (Branding)

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-004/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-TA-005 — Crear usuario Document Controller

| Campo | Valor |
|-------|-------|
| **Objetivo** | Alta operativa de rol clave |
| **Prioridad** | Crítica |
| **Tipo** | Positivo |
| **Precondiciones** | TC-TA-002 PASS |
| **Datos** | doccontrol.mft@alimentos-premium.test |
| **Ruta inicial** | `#/tenant-administration` |
| **Menú** | Tab Usuarios |
| **Pantalla** | User Administration |

**Pasos:**

1. Seleccione tab **Usuarios**
2. En **Email** ingrese doccontrol.mft@alimentos-premium.test
3. En **Nombre completo** ingrese Document Controller MFT
4. En **Password inicial** use contraseña compleja 12+ chars
5. En **Rol inicial** seleccione **Document Controller**
6. Presione **Crear / Invitar usuario**
7. Verifique fila en tabla con status Active

**Resultado final esperado:** Usuario creado; rol Document Controller asignado

**Auditoría esperada:** UserCreated

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-005/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-TA-006 — Asignar rol adicional a usuario existente

| Campo | Valor |
|-------|-------|
| **Objetivo** | Gestión RBAC |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Precondiciones** | Usuario quality@alimentos-premium.test existe |
| **Datos** | Quality Manager |
| **Ruta inicial** | `#/tenant-administration` |
| **Menú** | Tab Usuarios / Roles |
| **Pantalla** | RBAC |

**Pasos:**

1. Tab **Usuarios**
2. Localice usuario quality@alimentos-premium.test
3. Verifique rol **Quality Manager** asignado
4. Tab **Roles & Permisos**
5. Seleccione rol **Quality Manager**
6. Confirme permisos incluyen DOCUMENT.APPROVE
7. Documente permisos actuales

**Resultado final esperado:** Rol y permisos coherentes con RoleCatalog

**Auditoría esperada:** RoleAssigned / RbacUpdated

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-006/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-TA-007 — Otorgar TECHNICALSHEET.CREATE (preparación manual 08)

| Campo | Valor |
|-------|-------|
| **Objetivo** | Permiso no incluido en rol estándar |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Precondiciones** | TC-TA-006 PASS |
| **Datos** | Permiso TECHNICALSHEET.CREATE |
| **Ruta inicial** | `#/tenant-administration` |
| **Menú** | Tab Roles & Permisos |
| **Pantalla** | Editor permisos |

**Pasos:**

1. Tab **Roles & Permisos**
2. Edite rol **Tenant Administrator** o cree rol custom
3. Agregue permiso **TECHNICALSHEET.CREATE**
4. Guarde cambios
5. Verifique en lista de permisos del rol

**Resultado final esperado:** Permiso CREATE disponible para pruebas de fichas técnicas

**Auditoría esperada:** RbacPermissionGranted

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-007/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-TA-008 — Activar tenant (Trial → Active)

| Campo | Valor |
|-------|-------|
| **Objetivo** | Estado operativo del tenant |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Precondiciones** | Tenant en Trial o Pending |
| **Datos** | — |
| **Ruta inicial** | `#/tenant-administration` |
| **Menú** | Tab Estado / Informacion General |
| **Pantalla** | Estado tenant |

**Pasos:**

1. Localice indicador de estado del tenant
2. Si status ≠ Active, use acción **Activar**
3. Confirme diálogo si aparece
4. Verifique badge **Active**
5. Intente login con usuario operativo

**Resultado final esperado:** Tenant Active; usuarios pueden operar módulos

**Auditoría esperada:** TenantStatusChanged → Active

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-008/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-TA-009 — Validación email duplicado en alta usuario

| Campo | Valor |
|-------|-------|
| **Objetivo** | Integridad de usuarios |
| **Prioridad** | Media |
| **Tipo** | Negativo |
| **Precondiciones** | TC-TA-005 PASS |
| **Datos** | Email ya existente |
| **Ruta inicial** | `#/tenant-administration` |
| **Menú** | Tab Usuarios |
| **Pantalla** | Formulario usuario |

**Pasos:**

1. Tab **Usuarios**
2. Intente crear usuario con email doccontrol@alimentos-premium.test (ya provisionado)
3. Complete demás campos
4. Presione **Crear / Invitar usuario**
5. Observe mensaje de error

**Resultado final esperado:** Error claro; usuario NO duplicado; HTTP ≠ 500

**Auditoría esperada:** UserCreateFailed

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-009/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-TA-010 — Sin acceso a crear documentos operativos

| Campo | Valor |
|-------|-------|
| **Objetivo** | SoD — Tenant Admin no opera negocio |
| **Prioridad** | Alta |
| **Tipo** | Permisos |
| **Precondiciones** | TC-TA-001 PASS |
| **Datos** | — |
| **Ruta inicial** | `#/documents` |
| **Menú** | Operations → Documents |
| **Pantalla** | Documents Module |

**Pasos:**

1. Navegue a `#/documents`
2. Observe Action Center / formulario
3. Verifique si aparece **Modo solo lectura** o formulario deshabilitado
4. Confirme ausencia de botón **Crear registro real** funcional

**Resultado final esperado:** Tenant Administrator NO crea documentos desde UI (read-only o sin permiso CREATE)

**Auditoría esperada:** N/A — acción bloqueada por RBAC

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-010/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-TA-011 — Exportar auditoría del tenant

| Campo | Valor |
|-------|-------|
| **Objetivo** | Trazabilidad administrativa |
| **Prioridad** | Media |
| **Tipo** | Positivo |
| **Precondiciones** | Eventos previos en TC-TA-003..005 |
| **Datos** | — |
| **Ruta inicial** | `#/audit-trail` |
| **Menú** | Command Center → Audit Trail |
| **Pantalla** | Audit Trail |

**Pasos:**

1. Navegue a `#/audit-trail`
2. Presione **Exportar**
3. Confirme descarga
4. Busque eventos UserCreated de TC-TA-005

**Resultado final esperado:** CSV/export con eventos administrativos del tenant

**Auditoría esperada:** AuditExported

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-011/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-TA-012 — Logout Tenant Administrator

| Campo | Valor |
|-------|-------|
| **Objetivo** | Cierre seguro |
| **Prioridad** | Baja |
| **Tipo** | Positivo |
| **Precondiciones** | Sesión activa |
| **Datos** | — |
| **Ruta inicial** | `cualquiera` |
| **Menú** | Topbar |
| **Pantalla** | Salir |

**Pasos:**

1. Presione **Salir**
2. Confirme retorno a login

**Resultado final esperado:** Sesión terminada

**Auditoría esperada:** Logout

**Evidencia:** `artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-012/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



## 6. Validaciones negativas

- TC-TA-009: email duplicado → error validación\n- TC-TA-010: sin CREATE en módulos operativos\n- Password débil en alta usuario → rechazo

## 7. Validación visual

- Preview branding en tab Branding\n- Status pills en usuarios (Active/Locked)\n- Tabs TAC con scroll horizontal si viewport estrecho

## 8. Permisos esperados

**Debe poder:** tenant-administration (todas las tabs), audit-trail, dashboard, configuration (lectura parcial)

**No debe poder:** DOCUMENT.CREATE, CAPA.MANAGE, creación operativa en módulos de negocio

## 9. Auditoría

UserCreated, TenantUpdated, RbacUpdated con actor Tenant Administrator

## 10. Criterio de aprobación del rol

TC-TA-001, TC-TA-005 críticos PASS; mínimo 10/12 casos PASS

## 11. Qué aprendí

Tenant Administrator **habilita** a todos los demás roles: sin usuarios y RBAC correcto, el resto del programa está BLOCKED.

## 12. Referencias y extensiones API

- Provision: `scripts/e2e_provision.ps1`\n- Siguiente manual: `03_TENANT_SECURITY_ADMINISTRATOR_FUNCTIONAL_TESTS.md`
