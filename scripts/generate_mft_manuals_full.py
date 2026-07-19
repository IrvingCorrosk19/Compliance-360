#!/usr/bin/env python3
"""Generate full Compliance 360 manual functional testing documentation."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "manual-functional-testing"

LOGIN_V2 = """
### Procedimiento de login (Enterprise v2)

1. Abra `http://localhost:5272/`
2. Confirme badge **Enterprise Login** y título **Compliance 360 Enterprise**
3. Ingrese el correo indicado en **Correo electronico**
4. Presione **Siguiente** (loader: *Identificando...*)
5. Si aparece selección de organización, elija el tenant correcto y **Continuar**
6. Ingrese contraseña (desde `e2e/testdata.json` o `appsettings.Development.json` — **no documentada aquí**)
7. Presione **Iniciar sesion**
8. **Resultado esperado:** toast **Sesion iniciada correctamente.**; shell con sidebar **Compliance 360 / Enterprise Edition**
"""

LOGIN_LEGACY = """
### Login legacy (solo si v2 falla en su entorno)

1. Consola navegador: `localStorage.setItem('c360.login.v2', 'false'); location.reload()`
2. Complete **Tenant ID**, **Email**, **Password**
3. Presione **Iniciar sesion**
"""


def tc(id_, name, obj, priority, tipo, pre, data, route, menu, screen, steps, expected, audit, evidence):
    steps_txt = "\n".join(f"{i+1}. {s}" for i, s in enumerate(steps))
    return f"""
### {id_} — {name}

| Campo | Valor |
|-------|-------|
| **Objetivo** | {obj} |
| **Prioridad** | {priority} |
| **Tipo** | {tipo} |
| **Precondiciones** | {pre} |
| **Datos** | {data} |
| **Ruta inicial** | `{route}` |
| **Menú** | {menu} |
| **Pantalla** | {screen} |

**Pasos:**

{steps_txt}

**Resultado final esperado:** {expected}

**Auditoría esperada:** {audit}

**Evidencia:** `{evidence}`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---
"""


def role_footer(neg, visual, perms_must, perms_must_not, audit, criteria, learned, refs):
    return f"""
## 6. Validaciones negativas

{neg}

## 7. Validación visual

{visual}

## 8. Permisos esperados

**Debe poder:** {perms_must}

**No debe poder:** {perms_must_not}

## 9. Auditoría

{audit}

## 10. Criterio de aprobación del rol

{criteria}

## 11. Qué aprendí

{learned}

## 12. Referencias y extensiones API

{refs}
"""


def write(path, content):
    p = OUT / path
    p.write_text(content.strip() + "\n", encoding="utf-8")
    count = content.count("### TC-") + content.count("### E2E-")
    print(f"Wrote {path} ({count} casos)")


# --- 01 Platform Administrator (complete) ---
write("01_PLATFORM_ADMINISTRATOR_FUNCTIONAL_TESTS.md", f"""# Manual funcional — Platform Administrator

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
| Slug | `cliente-manual-test-{{YYYYMMDD}}` (único) |
| Razon Social | Cliente Manual Test, S.A. |
| Nombre Comercial | Cliente Manual |
| RUC / Tax ID | `RUC-MFT-{{timestamp}}` |
| Pais | PA |
| Moneda | USD |
| Admin Email | `owner@cliente-manual-test.test` |

## 5. Casos de prueba

{LOGIN_V2}

{LOGIN_LEGACY}

{tc("TC-PA-001", "Login Platform Administrator", "Validar acceso al shell de plataforma con Login v2", "Crítica", "Positivo",
    "Bootstrap OK; app running", "Email admin@compliance360.local", "http://localhost:5272/", "Login", "Enterprise Login",
    ["Abra `http://localhost:5272/` en Chromium/Edge", "Verifique título **Compliance 360 Enterprise**", "Ingrese `admin@compliance360.local` en **Correo electronico**", "Presione **Siguiente**", "Si aparece selector de organización, elija tenant plataforma y **Continuar**", "Ingrese contraseña desde appsettings", "Presione **Iniciar sesion**", "Espere toast de éxito", "Confirme topbar con chip de tenant plataforma"],
    "Sesión activa; sidebar visible; menú orientado a gobierno de plataforma",
    "AuthenticationSuccess / LoginRequested en audit trail plataforma", "artifacts/manual-functional-testing/Platform-Administrator/TC-PA-001/")}

{tc("TC-PA-002", "Abrir SuperAdmin Platform Center", "Navegar al centro de gobierno global", "Alta", "Positivo",
    "TC-PA-001 PASS", "—", "#/superadmin-platform", "Enterprise → SuperAdmin Platform", "SuperAdmin Platform Center",
    ["Con sesión activa, localice en sidebar **SuperAdmin Platform**", "Haga clic en el enlace", "Espere desaparición del loader global", "Confirme encabezado **SuperAdmin Platform Center**", "Verifique widgets/métricas: Tenants, Usuarios, Documentos, CAPA, Riesgos", "Revise que no hay errores en consola del navegador"],
    "Pantalla carga sin error 5xx; métricas numéricas visibles",
    "PlatformDashboardViewed o equivalente", "artifacts/manual-functional-testing/Platform-Administrator/TC-PA-002/")}

{tc("TC-PA-003", "Crear tenant nuevo", "Onboarding completo de cliente", "Crítica", "Positivo",
    "TC-PA-002 PASS", "Datos sección 4", "#/superadmin-platform", "Tab Tenants", "Formulario Crear Tenant",
    ["Vaya a tab **Tenants**", "Complete **Tenant Name**: Cliente Manual Test S.A.", "Complete **Slug** único (ej. cliente-manual-test-20260709)", "Complete **Razon Social** y **Nombre Comercial**", "Complete **RUC / Tax ID** único", "Seleccione **Pais**: PA", "Seleccione **Moneda**: USD", "Ingrese **Admin Email**: owner@cliente-manual-test.test", "Presione **Crear tenant**", "Espere toast de confirmación", "Verifique nueva fila en tabla de tenants", "Anote Tenant ID generado"],
    "Tenant creado; aparece en listado; Tenant Administrator inicial provisionado",
    "TenantCreated; UserCreated con actor Platform Administrator", "artifacts/manual-functional-testing/Platform-Administrator/TC-PA-003/")}

{tc("TC-PA-004", "Validación slug duplicado", "Rechazo limpio de datos inválidos", "Alta", "Negativo",
    "TC-PA-003 PASS con slug conocido", "Slug ya existente", "#/superadmin-platform", "Tab Tenants", "Formulario Crear Tenant",
    ["Abra formulario **Crear tenant**", "Use **Slug** idéntico al tenant creado en TC-PA-003", "Complete demás campos válidos", "Presione **Crear tenant**", "Observe mensaje de error en UI", "Verifique en DevTools → Network que NO es HTTP 500"],
    "Error de validación claro; tenant NO creado; HTTP 400 o mensaje UI",
    "TenantCreateFailed o validación sin persistencia", "artifacts/manual-functional-testing/Platform-Administrator/TC-PA-004/")}

{tc("TC-PA-005", "Aislamiento cross-tenant (403)", "Platform Admin no accede datos operativos sin soporte", "Alta", "Seguridad",
    "Tenant negocio provisionado (e2e_provision.ps1)", "TenantId de `e2e/testdata.json`", "#/superadmin-platform", "—", "DevTools Network",
    ["Mantenga sesión Platform Administrator", "Abra DevTools → pestaña **Network**", "Ejecute GET manual a `/api/v1/tenants/{{tenantNegocioId}}/documents` (Fetch o curl con Bearer token)", "Observe código HTTP de respuesta", "Verifique cuerpo de error"],
    "HTTP 403 Forbidden (sin PLATFORM.SUPPORT.ACCESS para datos operativos del tenant negocio)",
    "AuthorizationDenied", "artifacts/manual-functional-testing/Platform-Administrator/TC-PA-005/")}

{tc("TC-PA-006", "Exportar auditoría global CSV", "Evidencia de gobierno y trazabilidad", "Media", "Positivo",
    "TC-PA-002 PASS", "—", "#/superadmin-platform", "Barra platform-command", "Export",
    ["En SuperAdmin Platform Center, localice botón **Exportar auditoria global CSV**", "Presione el botón", "Espere descarga o toast de éxito", "Abra archivo CSV descargado", "Confirme columnas: usuario, acción, timestamp, correlationId"],
    "Archivo CSV generado; contiene eventos de plataforma recientes",
    "AuditExported", "artifacts/manual-functional-testing/Platform-Administrator/TC-PA-006/")}

{tc("TC-PA-007", "Navegar Tenant Administration (plataforma)", "Acceso a administración de tenants", "Media", "Positivo",
    "TC-PA-001 PASS", "—", "#/tenant-administration", "Enterprise → Tenant Administration", "Tenant Administration Center",
    ["En sidebar, haga clic en **Tenant Administration**", "Espere carga del centro TAC", "Verifique tabs: Informacion General, Branding, Seguridad, Usuarios, etc.", "Confirme que muestra datos del tenant plataforma o selector"],
    "TAC accesible; tabs renderizados sin error",
    "TenantAdministrationViewed", "artifacts/manual-functional-testing/Platform-Administrator/TC-PA-007/")}

{tc("TC-PA-008", "Login con contraseña incorrecta", "Anti-enumeración y lockout coherente", "Alta", "Negativo",
    "App running", "Contraseña inválida deliberada", "http://localhost:5272/", "Login", "Enterprise Login",
    ["Abra login v2", "Ingrese email válido admin@compliance360.local", "Presione **Siguiente**", "Ingrese contraseña incorrecta", "Presione **Iniciar sesion**", "Lea mensaje de error mostrado"],
    "Mensaje genérico (no revela si email existe); NO hay sesión activa; NO error 500",
    "AuthenticationFailed", "artifacts/manual-functional-testing/Platform-Administrator/TC-PA-008/")}

{tc("TC-PA-009", "Logout seguro", "Cierre de sesión y limpieza de token", "Media", "Positivo",
    "Sesión activa TC-PA-001", "—", "cualquiera", "Topbar", "Salir",
    ["Localice botón **Salir** en topbar", "Presione **Salir**", "Confirme retorno a pantalla de login", "Abra DevTools → Application → localStorage", "Verifique que token/sesión no es reutilizable", "Intente navegar a `#/superadmin-platform` sin login"],
    "Pantalla login; acceso denegado sin re-autenticación",
    "Logout", "artifacts/manual-functional-testing/Platform-Administrator/TC-PA-009/")}

{tc("TC-PA-010", "Observabilidad — Audit Trail plataforma", "Consulta de eventos globales", "Media", "Positivo",
    "TC-PA-001 PASS; eventos previos en TC-PA-003", "—", "#/audit-trail", "Command Center → Audit Trail", "Audit Trail",
    ["Navegue a `#/audit-trail`", "Espere carga de timeline", "Busque evento TenantCreated de TC-PA-003", "Verifique columnas: usuario, acción, IP, CorrelationId", "Opcional: filtre por acción"],
    "Eventos de plataforma visibles; TenantCreated presente",
    "AuditRead", "artifacts/manual-functional-testing/Platform-Administrator/TC-PA-010/")}

{role_footer(
"- TC-PA-004: slug duplicado → validación, no 500\\n- TC-PA-008: contraseña incorrecta → mensaje genérico\\n- TC-PA-005: acceso API cross-tenant → 403",
"- Badge **Live** en sidebar\\n- Breadcrumbs **Compliance 360 / {grupo}**\\n- Loaders durante fetch\\n- Sin errores consola",
"superadmin-platform, tenant-administration, audit-trail, export auditoría global",
"documents, capa, risks, suppliers como operador rutinario del tenant negocio",
"Toda creación tenant → `TenantCreated` con actor, timestamp, correlationId",
"Todos TC-PA-001 a TC-PA-010 PASS; **TC-PA-003 crítico obligatorio PASS**",
"Platform Administrator es el **punto de entrada comercial**: crea el espacio aislado (tenant) donde operará el cliente. Sin esta etapa no existen usuarios ni datos de negocio.",
"- Roadmap: `00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md`\\n- Provision E2E: `scripts/e2e_provision.ps1`\\n- Platform Operations / Platform Security: reutilicen TC-PA-002, TC-PA-006, TC-PA-010 con alcance reducido"
)}
""")

# --- 02 Tenant Administrator (complete) ---
write("02_TENANT_ADMINISTRATOR_FUNCTIONAL_TESTS.md", f"""# Manual funcional — Tenant Administrator

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

{LOGIN_V2}

{tc("TC-TA-001", "Login Tenant Administrator", "Acceso al Tenant Administration Center", "Crítica", "Positivo",
    "e2e_provision.ps1 ejecutado", "tenantadmin@alimentos-premium.test", "http://localhost:5272/", "Login → Dashboard", "Enterprise Shell",
    ["Abra `http://localhost:5272/`", "Login v2 con email tenantadmin@alimentos-premium.test", "Seleccione tenant **Alimentos Premium** si aplica → **Continuar**", "Ingrese contraseña desde testdata.json", "Presione **Iniciar sesion**", "Verifique toast **Sesion iniciada correctamente.**", "Confirme menú lateral con **Tenant Administration**"],
    "Sesión activa en tenant negocio; TAC accesible",
    "AuthenticationSuccess", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-001/")}

{tc("TC-TA-002", "Abrir Tenant Administration Center", "Navegación y tabs del TAC", "Alta", "Positivo",
    "TC-TA-001 PASS", "—", "#/tenant-administration", "Enterprise → Tenant Administration", "TAC",
    ["Haga clic en **Tenant Administration** del sidebar", "Espere carga completa", "Verifique tabs: **Informacion General**, **Branding**, **Seguridad**, **Usuarios**, **Roles & Permisos**", "Confirme nombre tenant **Alimentos Premium** visible"],
    "TAC renderizado; todos los tabs accesibles",
    "TenantAdministrationViewed", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-002/")}

{tc("TC-TA-003", "Completar Información General", "Datos corporativos del tenant", "Alta", "Positivo",
    "TC-TA-002 PASS", "Datos sección 4", "#/tenant-administration", "Tab Informacion General", "Formulario general",
    ["Seleccione tab **Informacion General**", "Complete **City**: Panama", "Complete **Phone**: +507-6000-0000", "Complete **Email**: contacto@alimentos-premium.test", "Presione **Guardar informacion general**", "Espere toast de éxito", "Recargue página y verifique persistencia"],
    "Datos guardados y visibles tras recarga",
    "TenantUpdated (General)", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-003/")}

{tc("TC-TA-004", "Configurar Branding", "Identidad visual del tenant", "Media", "Positivo",
    "TC-TA-002 PASS", "Primary #1769aa", "#/tenant-administration", "Tab Branding", "Formulario branding",
    ["Seleccione tab **Branding**", "En **Color primario** seleccione #1769aa", "En **Logo** ingrese https://example.com/logo.png", "Complete **Nombre mostrado** si está vacío", "Presione **Guardar branding**", "Verifique preview de marca actualizado"],
    "Branding persistido; preview coherente",
    "TenantUpdated (Branding)", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-004/")}

{tc("TC-TA-005", "Crear usuario Document Controller", "Alta operativa de rol clave", "Crítica", "Positivo",
    "TC-TA-002 PASS", "doccontrol.mft@alimentos-premium.test", "#/tenant-administration", "Tab Usuarios", "User Administration",
    ["Seleccione tab **Usuarios**", "En **Email** ingrese doccontrol.mft@alimentos-premium.test", "En **Nombre completo** ingrese Document Controller MFT", "En **Password inicial** use contraseña compleja 12+ chars", "En **Rol inicial** seleccione **Document Controller**", "Presione **Crear / Invitar usuario**", "Verifique fila en tabla con status Active"],
    "Usuario creado; rol Document Controller asignado",
    "UserCreated", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-005/")}

{tc("TC-TA-006", "Asignar rol adicional a usuario existente", "Gestión RBAC", "Alta", "Positivo",
    "Usuario quality@alimentos-premium.test existe", "Quality Manager", "#/tenant-administration", "Tab Usuarios / Roles", "RBAC",
    ["Tab **Usuarios**", "Localice usuario quality@alimentos-premium.test", "Verifique rol **Quality Manager** asignado", "Tab **Roles & Permisos**", "Seleccione rol **Quality Manager**", "Confirme permisos incluyen DOCUMENT.APPROVE", "Documente permisos actuales"],
    "Rol y permisos coherentes con RoleCatalog",
    "RoleAssigned / RbacUpdated", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-006/")}

{tc("TC-TA-007", "Otorgar TECHNICALSHEET.CREATE (preparación manual 08)", "Permiso no incluido en rol estándar", "Alta", "Positivo",
    "TC-TA-006 PASS", "Permiso TECHNICALSHEET.CREATE", "#/tenant-administration", "Tab Roles & Permisos", "Editor permisos",
    ["Tab **Roles & Permisos**", "Edite rol **Tenant Administrator** o cree rol custom", "Agregue permiso **TECHNICALSHEET.CREATE**", "Guarde cambios", "Verifique en lista de permisos del rol"],
    "Permiso CREATE disponible para pruebas de fichas técnicas",
    "RbacPermissionGranted", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-007/")}

{tc("TC-TA-008", "Activar tenant (Trial → Active)", "Estado operativo del tenant", "Alta", "Positivo",
    "Tenant en Trial o Pending", "—", "#/tenant-administration", "Tab Estado / Informacion General", "Estado tenant",
    ["Localice indicador de estado del tenant", "Si status ≠ Active, use acción **Activar**", "Confirme diálogo si aparece", "Verifique badge **Active**", "Intente login con usuario operativo"],
    "Tenant Active; usuarios pueden operar módulos",
    "TenantStatusChanged → Active", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-008/")}

{tc("TC-TA-009", "Validación email duplicado en alta usuario", "Integridad de usuarios", "Media", "Negativo",
    "TC-TA-005 PASS", "Email ya existente", "#/tenant-administration", "Tab Usuarios", "Formulario usuario",
    ["Tab **Usuarios**", "Intente crear usuario con email doccontrol@alimentos-premium.test (ya provisionado)", "Complete demás campos", "Presione **Crear / Invitar usuario**", "Observe mensaje de error"],
    "Error claro; usuario NO duplicado; HTTP ≠ 500",
    "UserCreateFailed", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-009/")}

{tc("TC-TA-010", "Sin acceso a crear documentos operativos", "SoD — Tenant Admin no opera negocio", "Alta", "Permisos",
    "TC-TA-001 PASS", "—", "#/documents", "Operations → Documents", "Documents Module",
    ["Navegue a `#/documents`", "Observe Action Center / formulario", "Verifique si aparece **Modo solo lectura** o formulario deshabilitado", "Confirme ausencia de botón **Crear registro real** funcional"],
    "Tenant Administrator NO crea documentos desde UI (read-only o sin permiso CREATE)",
    "N/A — acción bloqueada por RBAC", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-010/")}

{tc("TC-TA-011", "Exportar auditoría del tenant", "Trazabilidad administrativa", "Media", "Positivo",
    "Eventos previos en TC-TA-003..005", "—", "#/audit-trail", "Command Center → Audit Trail", "Audit Trail",
    ["Navegue a `#/audit-trail`", "Presione **Exportar**", "Confirme descarga", "Busque eventos UserCreated de TC-TA-005"],
    "CSV/export con eventos administrativos del tenant",
    "AuditExported", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-011/")}

{tc("TC-TA-012", "Logout Tenant Administrator", "Cierre seguro", "Baja", "Positivo",
    "Sesión activa", "—", "cualquiera", "Topbar", "Salir",
    ["Presione **Salir**", "Confirme retorno a login"],
    "Sesión terminada",
    "Logout", "artifacts/manual-functional-testing/Tenant-Administrator/TC-TA-012/")}

{role_footer(
"- TC-TA-009: email duplicado → error validación\\n- TC-TA-010: sin CREATE en módulos operativos\\n- Password débil en alta usuario → rechazo",
"- Preview branding en tab Branding\\n- Status pills en usuarios (Active/Locked)\\n- Tabs TAC con scroll horizontal si viewport estrecho",
"tenant-administration (todas las tabs), audit-trail, dashboard, configuration (lectura parcial)",
"DOCUMENT.CREATE, CAPA.MANAGE, creación operativa en módulos de negocio",
"UserCreated, TenantUpdated, RbacUpdated con actor Tenant Administrator",
"TC-TA-001, TC-TA-005 críticos PASS; mínimo 10/12 casos PASS",
"Tenant Administrator **habilita** a todos los demás roles: sin usuarios y RBAC correcto, el resto del programa está BLOCKED.",
"- Provision: `scripts/e2e_provision.ps1`\\n- Siguiente manual: `03_TENANT_SECURITY_ADMINISTRATOR_FUNCTIONAL_TESTS.md`"
)}
""")

print("Base files done. Run extended generator for 03-21...")
