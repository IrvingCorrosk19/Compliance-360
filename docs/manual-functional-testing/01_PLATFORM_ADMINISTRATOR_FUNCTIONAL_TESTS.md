# Manual funcional — Platform Administrator

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Platform Administrator

## 1. Propósito del rol

Administra la plataforma SaaS completa: tenants, licencias, observabilidad y gobierno global. **No opera datos de negocio** del tenant cliente (documentos, CAPA, riesgos, etc.) de forma rutinaria.

- **Resuelve:** onboarding de clientes, salud global, auditoría de plataforma.
- **No debe:** crear documentos/CAPA en tenant negocio sin mecanismo de soporte explícito.
- **Interactúa con:** Tenant Administrator (entrega tenant), Support Operator (break-glass).

## 2. Precondiciones

- Aplicación en `http://localhost:5272` (`dotnet run --project src/Compliance360.Web`)
- PostgreSQL activo con base `compliance360`
- Bootstrap ejecutado (`DevelopmentBootstrap`)
- Health check OK: `http://localhost:5272/health/live`

## 3. Credenciales de prueba

| Campo | Valor |
|-------|-------|
| **Rol** | Platform Administrator |
| **Email** | `admin@compliance360.local` |
| **Tenant ID** | `dc7c46ee-cb25-4ed5-b0b4-800788f7f626` |
| **Contraseña** | Ver `src/Compliance360.Web/appsettings.Development.json` → `BootstrapSuperAdmin:Password` |

## 4. Datos de prueba — Crear tenant

| Campo | Valor sugerido |
|-------|----------------|
| Tenant Name | Cliente Manual Test S.A. |
| Slug | `cliente-manual-test-{YYYYMMDD}` (único) |
| Razon Social | Cliente Manual Test, S.A. |
| Nombre Comercial | Cliente Manual |
| RUC / Tax ID | `RUC-MFT-{timestamp}` |
| Pais | PA |
| Moneda | USD |
| Admin Email | `owner@cliente-manual-test.test` |

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



### Login legacy (solo si v2 falla en su entorno)

1. Consola navegador: `localStorage.setItem('c360.login.v2', 'false'); location.reload()`
2. Complete **Tenant ID**, **Email**, **Password**
3. Presione **Iniciar sesion**



### TC-PA-001 — Login Platform Administrator

| Campo | Valor |
|-------|-------|
| **Objetivo** | Validar acceso al shell de plataforma con Login v2 |
| **Prioridad** | Crítica |
| **Tipo** | Positivo |
| **Precondiciones** | Bootstrap OK; app running |
| **Datos** | Email admin@compliance360.local |
| **Ruta inicial** | `http://localhost:5272/` |
| **Menú** | Login |
| **Pantalla** | Enterprise Login |

**Pasos:**

1. Abra `http://localhost:5272/` en Chromium/Edge
2. Verifique título **Compliance 360 Enterprise**
3. Ingrese `admin@compliance360.local` en **Correo electronico**
4. Presione **Siguiente**
5. Si aparece selector de organización, elija tenant plataforma y **Continuar**
6. Ingrese contraseña desde appsettings
7. Presione **Iniciar sesion**
8. Espere toast de éxito
9. Confirme topbar con chip de tenant plataforma

**Resultado final esperado:** Sesión activa; sidebar visible; menú orientado a gobierno de plataforma

**Auditoría esperada:** AuthenticationSuccess / LoginRequested en audit trail plataforma

**Evidencia:** `artifacts/manual-functional-testing/Platform-Administrator/TC-PA-001/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-PA-002 — Abrir SuperAdmin Platform Center

| Campo | Valor |
|-------|-------|
| **Objetivo** | Navegar al centro de gobierno global |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Precondiciones** | TC-PA-001 PASS |
| **Datos** | — |
| **Ruta inicial** | `#/superadmin-platform` |
| **Menú** | Enterprise → SuperAdmin Platform |
| **Pantalla** | SuperAdmin Platform Center |

**Pasos:**

1. Con sesión activa, localice en sidebar **SuperAdmin Platform**
2. Haga clic en el enlace
3. Espere desaparición del loader global
4. Confirme encabezado **SuperAdmin Platform Center**
5. Verifique widgets/métricas: Tenants, Usuarios, Documentos, CAPA, Riesgos
6. Revise que no hay errores en consola del navegador

**Resultado final esperado:** Pantalla carga sin error 5xx; métricas numéricas visibles

**Auditoría esperada:** PlatformDashboardViewed o equivalente

**Evidencia:** `artifacts/manual-functional-testing/Platform-Administrator/TC-PA-002/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-PA-003 — Crear tenant nuevo

| Campo | Valor |
|-------|-------|
| **Objetivo** | Onboarding completo de cliente |
| **Prioridad** | Crítica |
| **Tipo** | Positivo |
| **Precondiciones** | TC-PA-002 PASS |
| **Datos** | Datos sección 4 |
| **Ruta inicial** | `#/superadmin-platform` |
| **Menú** | Tab Tenants |
| **Pantalla** | Formulario Crear Tenant |

**Pasos:**

1. Vaya a tab **Tenants**
2. Complete **Tenant Name**: Cliente Manual Test S.A.
3. Complete **Slug** único (ej. cliente-manual-test-20260709)
4. Complete **Razon Social** y **Nombre Comercial**
5. Complete **RUC / Tax ID** único
6. Seleccione **Pais**: PA
7. Seleccione **Moneda**: USD
8. Ingrese **Admin Email**: owner@cliente-manual-test.test
9. Presione **Crear tenant**
10. Espere toast de confirmación
11. Verifique nueva fila en tabla de tenants
12. Anote Tenant ID generado

**Resultado final esperado:** Tenant creado; aparece en listado; Tenant Administrator inicial provisionado

**Auditoría esperada:** TenantCreated; UserCreated con actor Platform Administrator

**Evidencia:** `artifacts/manual-functional-testing/Platform-Administrator/TC-PA-003/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-PA-004 — Validación slug duplicado

| Campo | Valor |
|-------|-------|
| **Objetivo** | Rechazo limpio de datos inválidos |
| **Prioridad** | Alta |
| **Tipo** | Negativo |
| **Precondiciones** | TC-PA-003 PASS con slug conocido |
| **Datos** | Slug ya existente |
| **Ruta inicial** | `#/superadmin-platform` |
| **Menú** | Tab Tenants |
| **Pantalla** | Formulario Crear Tenant |

**Pasos:**

1. Abra formulario **Crear tenant**
2. Use **Slug** idéntico al tenant creado en TC-PA-003
3. Complete demás campos válidos
4. Presione **Crear tenant**
5. Observe mensaje de error en UI
6. Verifique en DevTools → Network que NO es HTTP 500

**Resultado final esperado:** Error de validación claro; tenant NO creado; HTTP 400 o mensaje UI

**Auditoría esperada:** TenantCreateFailed o validación sin persistencia

**Evidencia:** `artifacts/manual-functional-testing/Platform-Administrator/TC-PA-004/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-PA-005 — Aislamiento cross-tenant (403)

| Campo | Valor |
|-------|-------|
| **Objetivo** | Platform Admin no accede datos operativos sin soporte |
| **Prioridad** | Alta |
| **Tipo** | Seguridad |
| **Precondiciones** | Tenant negocio provisionado (e2e_provision.ps1) |
| **Datos** | TenantId de `e2e/testdata.json` |
| **Ruta inicial** | `#/superadmin-platform` |
| **Menú** | — |
| **Pantalla** | DevTools Network |

**Pasos:**

1. Mantenga sesión Platform Administrator
2. Abra DevTools → pestaña **Network**
3. Ejecute GET manual a `/api/v1/tenants/{{tenantNegocioId}}/documents` (Fetch o curl con Bearer token)
4. Observe código HTTP de respuesta
5. Verifique cuerpo de error

**Resultado final esperado:** HTTP 403 Forbidden (sin PLATFORM.SUPPORT.ACCESS para datos operativos del tenant negocio)

**Auditoría esperada:** AuthorizationDenied

**Evidencia:** `artifacts/manual-functional-testing/Platform-Administrator/TC-PA-005/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-PA-006 — Exportar auditoría global CSV

| Campo | Valor |
|-------|-------|
| **Objetivo** | Evidencia de gobierno y trazabilidad |
| **Prioridad** | Media |
| **Tipo** | Positivo |
| **Precondiciones** | TC-PA-002 PASS |
| **Datos** | — |
| **Ruta inicial** | `#/superadmin-platform` |
| **Menú** | Barra platform-command |
| **Pantalla** | Export |

**Pasos:**

1. En SuperAdmin Platform Center, localice botón **Exportar auditoria global CSV**
2. Presione el botón
3. Espere descarga o toast de éxito
4. Abra archivo CSV descargado
5. Confirme columnas: usuario, acción, timestamp, correlationId

**Resultado final esperado:** Archivo CSV generado; contiene eventos de plataforma recientes

**Auditoría esperada:** AuditExported

**Evidencia:** `artifacts/manual-functional-testing/Platform-Administrator/TC-PA-006/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-PA-007 — Navegar Tenant Administration (plataforma)

| Campo | Valor |
|-------|-------|
| **Objetivo** | Acceso a administración de tenants |
| **Prioridad** | Media |
| **Tipo** | Positivo |
| **Precondiciones** | TC-PA-001 PASS |
| **Datos** | — |
| **Ruta inicial** | `#/tenant-administration` |
| **Menú** | Enterprise → Tenant Administration |
| **Pantalla** | Tenant Administration Center |

**Pasos:**

1. En sidebar, haga clic en **Tenant Administration**
2. Espere carga del centro TAC
3. Verifique tabs: Informacion General, Branding, Seguridad, Usuarios, etc.
4. Confirme que muestra datos del tenant plataforma o selector

**Resultado final esperado:** TAC accesible; tabs renderizados sin error

**Auditoría esperada:** TenantAdministrationViewed

**Evidencia:** `artifacts/manual-functional-testing/Platform-Administrator/TC-PA-007/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-PA-008 — Login con contraseña incorrecta

| Campo | Valor |
|-------|-------|
| **Objetivo** | Anti-enumeración y lockout coherente |
| **Prioridad** | Alta |
| **Tipo** | Negativo |
| **Precondiciones** | App running |
| **Datos** | Contraseña inválida deliberada |
| **Ruta inicial** | `http://localhost:5272/` |
| **Menú** | Login |
| **Pantalla** | Enterprise Login |

**Pasos:**

1. Abra login v2
2. Ingrese email válido admin@compliance360.local
3. Presione **Siguiente**
4. Ingrese contraseña incorrecta
5. Presione **Iniciar sesion**
6. Lea mensaje de error mostrado

**Resultado final esperado:** Mensaje genérico (no revela si email existe); NO hay sesión activa; NO error 500

**Auditoría esperada:** AuthenticationFailed

**Evidencia:** `artifacts/manual-functional-testing/Platform-Administrator/TC-PA-008/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-PA-009 — Logout seguro

| Campo | Valor |
|-------|-------|
| **Objetivo** | Cierre de sesión y limpieza de token |
| **Prioridad** | Media |
| **Tipo** | Positivo |
| **Precondiciones** | Sesión activa TC-PA-001 |
| **Datos** | — |
| **Ruta inicial** | `cualquiera` |
| **Menú** | Topbar |
| **Pantalla** | Salir |

**Pasos:**

1. Localice botón **Salir** en topbar
2. Presione **Salir**
3. Confirme retorno a pantalla de login
4. Abra DevTools → Application → localStorage
5. Verifique que token/sesión no es reutilizable
6. Intente navegar a `#/superadmin-platform` sin login

**Resultado final esperado:** Pantalla login; acceso denegado sin re-autenticación

**Auditoría esperada:** Logout

**Evidencia:** `artifacts/manual-functional-testing/Platform-Administrator/TC-PA-009/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



### TC-PA-010 — Observabilidad — Audit Trail plataforma

| Campo | Valor |
|-------|-------|
| **Objetivo** | Consulta de eventos globales |
| **Prioridad** | Media |
| **Tipo** | Positivo |
| **Precondiciones** | TC-PA-001 PASS; eventos previos en TC-PA-003 |
| **Datos** | — |
| **Ruta inicial** | `#/audit-trail` |
| **Menú** | Command Center → Audit Trail |
| **Pantalla** | Audit Trail |

**Pasos:**

1. Navegue a `#/audit-trail`
2. Espere carga de timeline
3. Busque evento TenantCreated de TC-PA-003
4. Verifique columnas: usuario, acción, IP, CorrelationId
5. Opcional: filtre por acción

**Resultado final esperado:** Eventos de plataforma visibles; TenantCreated presente

**Auditoría esperada:** AuditRead

**Evidencia:** `artifacts/manual-functional-testing/Platform-Administrator/TC-PA-010/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---



## 6. Validaciones negativas

- TC-PA-004: slug duplicado → validación, no 500\n- TC-PA-008: contraseña incorrecta → mensaje genérico\n- TC-PA-005: acceso API cross-tenant → 403

## 7. Validación visual

- Badge **Live** en sidebar\n- Breadcrumbs **Compliance 360 / {grupo}**\n- Loaders durante fetch\n- Sin errores consola

## 8. Permisos esperados

**Debe poder:** superadmin-platform, tenant-administration, audit-trail, export auditoría global

**No debe poder:** documents, capa, risks, suppliers como operador rutinario del tenant negocio

## 9. Auditoría

Toda creación tenant → `TenantCreated` con actor, timestamp, correlationId

## 10. Criterio de aprobación del rol

Todos TC-PA-001 a TC-PA-010 PASS; **TC-PA-003 crítico obligatorio PASS**

## 11. Qué aprendí

Platform Administrator es el **punto de entrada comercial**: crea el espacio aislado (tenant) donde operará el cliente. Sin esta etapa no existen usuarios ni datos de negocio.

## 12. Referencias y extensiones API

- Roadmap: `00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md`\n- Provision E2E: `scripts/e2e_provision.ps1`\n- Platform Operations / Platform Security: reutilicen TC-PA-002, TC-PA-006, TC-PA-010 con alcance reducido
