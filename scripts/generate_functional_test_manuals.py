#!/usr/bin/env python3
"""Generate Compliance 360 manual functional testing documentation."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "manual-functional-testing"

COMMON_LOGIN_V2 = """
### Procedimiento de login (Enterprise v2)

1. Abra `http://localhost:5272/`
2. Confirme badge **Enterprise Login** y título **Compliance 360 Enterprise**
3. Ingrese el correo indicado en **Correo electronico**
4. Presione **Siguiente** (loader: *Identificando...*)
5. Si aparece selección de organización, elija el tenant correcto y **Continuar**
6. Ingrese contraseña (desde `e2e/testdata.json` o `appsettings.Development.json`)
7. Presione **Iniciar sesion**
8. **Resultado esperado:** toast **Sesion iniciada correctamente.**; shell con sidebar **Compliance 360 / Enterprise Edition**
"""

COMMON_LOGIN_LEGACY = """
### Login legacy (solo si v2 falla en su entorno)

1. Consola navegador: `localStorage.setItem('c360.login.v2', 'false'); location.reload()`
2. Complete **Tenant ID**, **Email**, **Password**
3. **Iniciar sesion**
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

**Evidencia:** {evidence}

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

**Observaciones:**

---
"""

def write(path: str, content: str):
    (OUT / path).write_text(content.strip() + "\n", encoding="utf-8")
    print(f"Wrote {path}")

# --- 01 Platform Administrator ---
write("01_PLATFORM_ADMINISTRATOR_FUNCTIONAL_TESTS.md", f"""# Manual funcional — Platform Administrator

## 1. Propósito del rol

Administra la plataforma SaaS completa: tenants, licencias, observabilidad y gobierno global. **No opera datos de negocio** del tenant cliente (documentos, CAPA, etc.).

- **Resuelve:** onboarding de clientes, salud global, auditoría de plataforma.
- **No debe:** crear documentos/CAPA en tenant negocio sin mecanismo de soporte.
- **Interactúa con:** Tenant Administrator (entrega tenant), Support Operator.

## 2. Precondiciones

- App en `http://localhost:5272`
- Bootstrap ejecutado (`DevelopmentBootstrap`)
- PostgreSQL activo

## 3. Credenciales de prueba

| Campo | Valor |
|-------|-------|
| **Rol** | Platform Administrator |
| **Email** | `admin@compliance360.local` |
| **Tenant** | `dc7c46ee-cb25-4ed5-b0b4-800788f7f626` |
| **Contraseña** | Ver `appsettings.Development.json` → `BootstrapSuperAdmin:Password` |

## 4. Datos de prueba — Crear tenant

| Campo | Valor |
|-------|-------|
| Tenant Name | Cliente Manual Test S.A. |
| Slug | `cliente-manual-test-{{fecha}}` (único) |
| Razon Social | Cliente Manual Test, S.A. |
| Nombre Comercial | Cliente Manual |
| RUC / Tax ID | RUC-MFT-{{timestamp}} |
| Pais | PA |
| Moneda | USD |
| Admin Email | `owner@cliente-manual-test.test` |

## 5. Casos de prueba

{COMMON_LOGIN_V2}

{tc("TC-PA-001", "Login Platform Administrator", "Validar acceso al shell de plataforma", "Alta", "Positivo",
    "Bootstrap OK", "Email admin@compliance360.local", "http://localhost:5272/", "Login", "Enterprise Login",
    ["Abra la URL base", "Complete login v2", "Verifique topbar con tenant chip", "Confirme menú lateral sin módulos operativos de negocio (documents, capa) visibles como acceso directo habitual"],
    "Sesión activa; menú orientado a SuperAdmin Platform / Tenant Administration",
    "LoginRequested / AuthenticationSuccess en audit trail plataforma", "Screenshot login + dashboard plataforma")}

{tc("TC-PA-002", "Abrir SuperAdmin Platform Center", "Navegar al centro de gobierno global", "Alta", "Positivo",
    "TC-PA-001 PASS", "—", "#/superadmin-platform", "Operations o Enterprise → SuperAdmin Platform", "SuperAdmin Platform Center",
    ["En sidebar, localice **SuperAdmin Platform** (ES: Plataforma SuperAdmin)", "Haga clic", "Espere desaparición del loader global", "Confirme h1 **SuperAdmin Platform Center**", "Verifique métricas: Tenants, Usuarios, Documentos, CAPA, Riesgos"],
    "Pantalla carga sin error 5xx; métricas numéricas visibles",
    "Viewed platform center", "Screenshot hero + grid métricas")}

{tc("TC-PA-003", "Crear tenant nuevo", "Onboarding de cliente", "Crítica", "Positivo",
    "TC-PA-002 PASS", "Datos sección 4", "#/superadmin-platform", "Tab **Tenants**", "Formulario Crear Tenant",
    ["Vaya a tab **Tenants**", "Complete **Tenant Name**: Cliente Manual Test S.A.", "Complete **Slug** único", "Complete RUC, país PA, moneda USD", "Email admin: owner@cliente-manual-test.test", "Presione **Crear tenant**", "Espere confirmación toast", "Verifique fila en tabla Tenant fleet"],
    "Tenant creado; aparece en listado; Tenant Administrator inicial provisionado",
    "TenantCreated; UserCreated", "Screenshot formulario + fila tenant + ID anotado")}

{tc("TC-PA-004", "Aislamiento cross-tenant (403)", "Platform Admin no accede datos negocio ajenos sin soporte", "Alta", "Seguridad",
    "Tenant negocio existente (alimentos-premium)", "TenantId negocio de testdata.json", "#/superadmin-platform", "—", "DevTools Network",
    ["Con sesión Platform Admin, abra DevTools → Network", "Intente GET manual `/api/v1/tenants/{{tenantNegocio}}/documents`", "Observe código HTTP"],
    "HTTP 403 Forbidden (sin PLATFORM.SUPPORT.ACCESS para datos operativos)",
    "AuthorizationDenied", "Screenshot response 403")}

{tc("TC-PA-005", "Exportar auditoría global CSV", "Evidencia de gobierno", "Media", "Positivo",
    "TC-PA-002 PASS", "—", "#/superadmin-platform", "Barra platform-command", "Export",
    ["Localice campo búsqueda global plataforma", "Presione **Exportar auditoria global CSV**", "Confirme descarga o respuesta exitosa"],
    "Archivo CSV generado o toast éxito",
    "AuditExported", "Archivo CSV guardado en evidencias")}

{tc("TC-PA-006", "Logout", "Cierre seguro de sesión", "Media", "Positivo",
    "Sesión activa", "—", "cualquiera", "Topbar", "Salir",
    ["Presione **Salir**", "Confirme retorno a pantalla login", "Verifique localStorage sin token usable"],
    "Pantalla login; sesión terminada",
    "Logout", "Screenshot login post-logout")}

## 6. Validaciones negativas

- TC-PA-003 con **Slug duplicado** → error validación, no 500.
- TC-PA-003 con **Tax ID duplicado** → HTTP 400 mensaje claro.
- Login con contraseña incorrecta → mensaje genérico anti-enumeración.

## 7. Validación visual

- Badge **Live** en sidebar
- Breadcrumbs **Compliance 360 / {grupo}**
- Loaders/skeleton durante fetch
- Sin errores consola

## 8. Permisos esperados

**Debe ver:** superadmin-platform, tenant-administration, audit-trail (según JWT)  
**No debe ver:** documents, capa, risks como operador rutinario

## 9. Auditoría

Toda creación tenant → `TenantCreated` con actor Platform Administrator

## 10. Criterio aprobación rol

Todos TC-PA-001 a TC-PA-006 PASS; TC-PA-003 crítico PASS

## 11. Qué aprendí

Platform Administrator es el **punto de entrada comercial**: crea el espacio aislado (tenant) donde operará el cliente. Sin esta etapa no existen usuarios ni datos de negocio.
""")

# Continue with more files - I'll generate key ones programmatically
roles_content = {
"02_TENANT_ADMINISTRATOR_FUNCTIONAL_TESTS.md": ("Tenant Administrator", "tenantadmin@alimentos-premium.test", "ddcaf211 (ver testdata.json)", "#/tenant-administration", [
    ("TC-TA-001", "Login Tenant Administrator", "Acceso TAC", "Alta", "#/tenant-administration", ["Login con email tenant admin", "Verifique menú: dashboard, tenant-administration, audit-trail", "Abra **Tenant Administration Center**"], "TAC visible con tabs"),
    ("TC-TA-002", "Completar Información General", "Datos empresa", "Alta", "Tab Informacion General", ["Tab **Informacion General**", "Complete City: Panama", "Phone: +507-6000-0000", "Email: contacto@alimentos-premium.test", "**Guardar informacion general**"], "Datos persistidos"),
    ("TC-TA-003", "Configurar Branding", "Identidad visual", "Media", "Tab Branding", ["Tab **Branding**", "Primary Color: #1769aa", "Logo URI: https://example.com/logo.png", "**Guardar branding**"], "Preview branding actualizado"),
    ("TC-TA-004", "Crear usuario Document Controller", "Alta operativa", "Crítica", "Tab Usuarios", ["Tab **Usuarios**", "Email: doccontrol.mft@alimentos-premium.test", "Nombre: Document Controller MFT", "Password inicial (12+ chars compleja)", "Rol: Document Controller", "**Crear / Invitar usuario**"], "Usuario en tabla Active"),
    ("TC-TA-005", "Asignar rol Quality Manager", "RBAC", "Alta", "Tab Roles & Permisos", ["Tab **Roles & Permisos**", "Asignar rol Quality Manager a quality@...", "**Asignar rol**"], "Rol asignado"),
    ("TC-TA-006", "Activar tenant Trial→Active", "Estado operativo", "Alta", "Tab Estado", ["Tab **Estado**", "**Activar**", "Confirme status Active"], "Tenant Active"),
]),
}

for fname, (role, email, tenant, route, cases) in roles_content.items():
    blocks = ""
    for c in cases:
        cid, name, obj, pri, screen, steps, exp = c
        blocks += tc(cid, name, obj, pri, "Positivo", "Etapas previas PASS", f"Email {email}", route, "Enterprise → Tenant Administration", screen, steps, exp, f"UserCreated/Updated por {role}", f"artifacts/manual-functional-testing/Tenant-Administrator/{cid}/")
    write(fname, f"""# Manual funcional — {role}

## 1. Propósito del rol
Administra perfil del tenant, usuarios, roles, branding, dominios, licenciamiento. No crea documentos/CAPA por defecto.

## 2. Precondiciones
Tenant negocio provisionado (`scripts/e2e_provision.ps1`).

## 3. Credenciales
| Rol | {role} |
| Email | `{email}` |
| Tenant | {tenant} |
| Password | `e2e/testdata.json` |

## 4. Datos de prueba
Ver casos individuales.

## 5. Casos de prueba
{COMMON_LOGIN_V2}
{blocks}

## 6–11. Validaciones, permisos, auditoría, criterio, aprendizaje
Ver `00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md`. Rol aprueba cuando TC-TA-001..006 PASS.
""")

# Templates 18-20
write("18_FUNCTIONAL_TEST_RESULTS_TEMPLATE.md", """# Plantilla — Resultados de prueba funcional

| Campo | Valor |
|-------|-------|
| **Programa** | Compliance 360 Manual Functional Testing |
| **Ejecutor** | |
| **Fecha** | |
| **Entorno** | Local / URL |
| **Tenant ID** | |
| **Build/Commit** | |

## Registro por caso

| ID Caso | Rol | PASS/FAIL/BLOCKED/PENDING | Fecha | Evidencia | Notas |
|---------|-----|---------------------------|-------|-----------|-------|
| TC-PA-001 | Platform Administrator | | | | |
| TC-TA-001 | Tenant Administrator | | | | |

## Resumen

- Total casos:
- PASS:
- FAIL:
- BLOCKED:
- PENDING THIRD-PARTY:
""")

write("19_DEFECT_REPORT_TEMPLATE.md", """# Plantilla — Reporte de defecto

| Campo | Valor |
|-------|-------|
| **ID Defecto** | DEF-MFT- |
| **Caso relacionado** | TC- |
| **Rol** | |
| **Severidad** | Crítica / Alta / Media / Baja |
| **Estado** | Abierto / En progreso / Cerrado |

## Descripción

## Pasos para reproducir

1.

## Resultado esperado

## Resultado actual

## Evidencia

- Screenshot:
- Network response:
- Audit log:

## Impacto en negocio
""")

write("20_FINAL_MANUAL_TESTING_CHECKLIST.md", """# Checklist final — Pruebas funcionales manuales

## Etapa 1 — Plataforma
- [ ] 01 Platform Administrator completo

## Etapa 2 — Tenant
- [ ] 02 Tenant Administrator completo

## Etapa 3 — Seguridad
- [ ] 03 Tenant Security Administrator completo

## Etapa 4 — Infraestructura
- [ ] 04 Storage Administrator
- [ ] 05 Notification Administrator

## Etapa 5–12 — Operaciones
- [ ] 06 Document Controller
- [ ] 07 Quality Manager
- [ ] 08 Technical Sheets
- [ ] 09 Supplier Manager
- [ ] 10 Auditor
- [ ] 11 CAPA Manager
- [ ] 12 Risk Manager
- [ ] 13 Indicators Manager
- [ ] 14 Reporting Manager

## Etapa 13–14
- [ ] 15 Viewer
- [ ] 16 Support Operator

## E2E
- [ ] 17 Procesos E2E (E2E-001..006)

## Cierre
- [ ] 18 Resultados consolidados
- [ ] 19 Defectos críticos/altos resueltos o aceptados
- [ ] 21 Quality report revisado

**Veredicto final:** [ ] LISTO PARA PRODUCCIÓN  [ ] REQUIERE REMEDIACIÓN
""")

print("Partial generation done — extend manually for remaining role files")
