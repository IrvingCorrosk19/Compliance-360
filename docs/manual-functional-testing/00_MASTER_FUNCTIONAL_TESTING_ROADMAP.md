# Compliance 360 — Roadmap Maestro de Pruebas Funcionales Manuales

**Versión:** 1.0  
**Fecha:** 2026-07-09  
**Alcance:** Pruebas funcionales End-to-End por rol en entorno local  
**URL base:** `http://localhost:5272`  
**Fuente de verdad:** código en `src/Compliance360.Web/wwwroot/app.js`, `FoundationEndpoints.cs`, `RoleCatalog.cs`

---

## 1. Propósito de este programa

Este programa te guía **paso a paso desde el navegador** para:

1. Aprender a operar Compliance 360 por rol.
2. Validar flujos de negocio completos antes de producción.
3. Registrar PASS/FAIL con evidencia trazable.
4. Comprender cómo fluye la información entre roles (handoffs).

**No es documentación teórica.** Cada manual indica menús, rutas hash, botones y resultados esperados basados en la implementación real.

---

## 2. Cómo usar los manuales

| Paso | Acción |
|------|--------|
| 1 | Lea este roadmap completo una vez. |
| 2 | Prepare el entorno (Sección 3). |
| 3 | Ejecute los manuales **en el orden de la Sección 4**. |
| 4 | Registre resultados en `18_FUNCTIONAL_TEST_RESULTS_TEMPLATE.md`. |
| 5 | Reporte defectos con `19_DEFECT_REPORT_TEMPLATE.md`. |
| 6 | Al finalizar todas las etapas, complete `20_FINAL_MANUAL_TESTING_CHECKLIST.md`. |

**Regla de oro:** No salte etapas de configuración (Plataforma → Tenant → Seguridad → Infraestructura) antes de probar módulos operativos.

---

## 3. Preparación del entorno

### 3.1 Requisitos

- .NET SDK del proyecto compilado sin errores.
- PostgreSQL local con base `compliance360`.
- Navegador Chromium/Edge/Chrome actualizado.
- Resolución mínima recomendada: 1280×800.

### 3.2 Iniciar la aplicación

```powershell
cd "c:\Proyectos\Compliance 360"
dotnet run --project src/Compliance360.Web
```

Confirme que responde: `http://localhost:5272/health/live`

### 3.3 Provisionar datos de prueba E2E (recomendado)

```powershell
powershell -ExecutionPolicy Bypass -File scripts/e2e_provision.ps1
```

Genera/actualiza `e2e/testdata.json` con:

- **Tenant plataforma:** `dc7c46ee-cb25-4ed5-b0b4-800788f7f626`
- **Tenant negocio:** slug `alimentos-premium-pa` (ID en testdata.json)
- **Un usuario por rol** con dominio `@alimentos-premium.test`

### 3.4 Contraseñas

**No se documentan contraseñas en los manuales.**

Obtenga credenciales desde:

| Origen | Uso |
|--------|-----|
| `src/Compliance360.Web/appsettings.Development.json` → `BootstrapSuperAdmin` | Platform Administrator |
| `e2e/testdata.json` (post-provision) | Usuarios del tenant negocio |
| `scripts/e2e_provision.ps1` | Referencia de emails creados |

### 3.5 Modo de login

Por defecto la UI usa **Login Enterprise v2** (`LOGIN_V2_ENABLED = true`):

1. Correo electrónico → **Siguiente**
2. Organización (si aplica) → **Continuar**
3. Contraseña → **Iniciar sesion**

Login legacy (con Tenant ID visible): en consola del navegador ejecute `localStorage.setItem("c360.login.v2", "false")` y recargue.

### 3.6 Estructura de evidencias

```
artifacts/manual-functional-testing/
  {rol-normalizado}/
    TC-XXX-001/
      01-inicio.png
      02-resultado.png
      audit-log.txt
      notas.md
```

---

## 4. Orden obligatorio de ejecución

| Orden | Etapa | Manual | Rol principal | Depende de |
|-------|-------|--------|---------------|------------|
| 1 | Plataforma | `01_PLATFORM_ADMINISTRATOR_FUNCTIONAL_TESTS.md` | Platform Administrator | App running |
| 2 | Configuración tenant | `02_TENANT_ADMINISTRATOR_FUNCTIONAL_TESTS.md` | Tenant Administrator | Tenant creado o provisionado |
| 3 | Seguridad tenant | `03_TENANT_SECURITY_ADMINISTRATOR_FUNCTIONAL_TESTS.md` | Tenant Security Administrator | Usuario creado en etapa 2 |
| 4a | Storage | `04_STORAGE_ADMINISTRATOR_FUNCTIONAL_TESTS.md` | Storage Administrator | Tenant activo |
| 4b | Notificaciones | `05_NOTIFICATION_ADMINISTRATOR_FUNCTIONAL_TESTS.md` | Notification Administrator | Tenant activo |
| 5 | Control documental (crear) | `06_DOCUMENT_CONTROLLER_FUNCTIONAL_TESTS.md` | Document Controller | Infraestructura lista |
| 5b | Control documental (aprobar) | `07_QUALITY_MANAGER_FUNCTIONAL_TESTS.md` (TC-QM-DOC-*) | Quality Manager | Documento creado en etapa 5 |
| 6 | Fichas técnicas | `08_TECHNICAL_SHEETS_FUNCTIONAL_TESTS.md` | Quality Manager + Tenant Admin (RBAC) | Ver manual — permiso CREATE no está en roles estándar |
| 7 | Proveedores | `09_SUPPLIER_MANAGER_FUNCTIONAL_TESTS.md` | Supplier Manager | Storage recomendado |
| 8 | Auditorías | `10_AUDITOR_FUNCTIONAL_TESTS.md` | Auditor | — |
| 9 | CAPA | `11_CAPA_MANAGER_FUNCTIONAL_TESTS.md` | CAPA Manager | Hallazgo/auditoría opcional |
| 9b | CAPA cierre | `07_QUALITY_MANAGER_FUNCTIONAL_TESTS.md` (TC-QM-CAPA-*) | Quality Manager | CAPA en PendingApproval |
| 10 | Riesgos | `12_RISK_MANAGER_FUNCTIONAL_TESTS.md` | Risk Manager | — |
| 10b | Riesgos aprobación | `07_QUALITY_MANAGER_FUNCTIONAL_TESTS.md` (TC-QM-RISK-*) | Quality Manager | Riesgo en workflow |
| 11 | Indicadores | `13_INDICATORS_MANAGER_FUNCTIONAL_TESTS.md` | Indicators Manager | — |
| 12 | Reportes | `14_REPORTING_MANAGER_FUNCTIONAL_TESTS.md` | Reporting Manager | Datos en módulos previos |
| 13 | Consulta | `15_VIEWER_FUNCTIONAL_TESTS.md` | Viewer | Datos existentes |
| 14 | Soporte | `16_SUPPORT_OPERATOR_FUNCTIONAL_TESTS.md` | Support Operator | Platform tenant |
| 15 | E2E transversal | `17_END_TO_END_BUSINESS_PROCESS_TESTS.md` | Varios | Etapas 1–14 |

---

## 5. Mapa de dependencias entre módulos

```
Platform Admin → crea Tenant → Tenant Admin configura usuarios/RBAC
       ↓
Storage Admin + Notification Admin → adjuntos y alertas
       ↓
Document Controller → Quality Manager (aprobación)
Auditor → CAPA Manager → Quality Manager (cierre CAPA)
Supplier Manager → CAPA (opcional NC)
Risk Manager → Quality Manager (aprobación riesgo)
Indicators Manager → Dashboard/Reports
Reporting Manager → consume todos los módulos
Viewer → solo lectura transversal
```

---

## 6. Navegación real (referencia rápida)

Grupos del menú lateral (`app.js`):

| Grupo | Rutas hash |
|-------|------------|
| Command Center | `#/dashboard`, `#/compliance`, `#/reports`, `#/audit-trail` |
| Operations | `#/documents`, `#/technical-sheets`, `#/suppliers`, `#/audits`, `#/capa`, `#/risks`, `#/indicators` |
| Enterprise | `#/tenant-administration`, `#/security`, `#/configuration`, … |

**Nota:** Platform Administrator ve principalmente `#/superadmin-platform` y `#/tenant-administration`, no módulos operativos del tenant negocio.

---

## 7. Criterios PASS / FAIL

| Resultado | Criterio |
|-----------|----------|
| **PASS** | Todos los pasos producen el resultado esperado; permisos correctos; sin error 5xx; auditoría coherente cuando aplica. |
| **FAIL** | Resultado distinto al esperado, error visible, permiso incorrecto, dato no persistido, auditoría ausente en acción crítica. |
| **BLOCKED** | Precondición no cumplida (ej. tenant no creado). |
| **PENDING THIRD-PARTY** | Requiere credencial externa (SMTP real, Azure Blob, OIDC). No es FAIL; repetir cuando exista configuración. |

---

## 8. Cómo registrar resultados

Use `18_FUNCTIONAL_TEST_RESULTS_TEMPLATE.md` por cada caso:

- ID del caso (ej. `TC-TA-003`)
- Fecha/hora
- Ejecutor
- PASS / FAIL / BLOCKED / PENDING
- Evidencia (ruta a screenshots)
- Observaciones

---

## 9. Cómo reportar defectos

Use `19_DEFECT_REPORT_TEMPLATE.md`:

- Título claro
- Rol y caso donde ocurrió
- Pasos para reproducir (copiar del manual)
- Resultado esperado vs actual
- Severidad (Crítica / Alta / Media / Baja)
- Adjuntar screenshot y respuesta de red si aplica

---

## 10. Reinicio y limpieza de datos

| Escenario | Acción |
|-----------|--------|
| Re-ejecutar tenant limpio | Crear nuevo tenant desde Platform Admin con slug único, o re-provisionar con `e2e_provision.ps1` |
| Re-ejecutar un caso | Use códigos únicos con sufijo `-RUN2` (ej. `DOC-MFT-001-RUN2`) |
| Limpiar sesión | Botón **Salir** en topbar; borrar `localStorage` c360.token si es necesario |
| Journey completo automatizado | `scripts/customer_journey.ps1` (referencia API para ciclos no expuestos en UI) |

---

## 11. Limitaciones conocidas de la UI (importante)

Documentadas tras análisis del código — **no son defectos**, son límites actuales del frontend:

| Capacidad | UI navegador | API / Swagger / customer_journey |
|-----------|--------------|----------------------------------|
| Crear registro en módulos operativos | Sí (Action Center) | Sí |
| Ciclo documental completo (versión→submit→approve) | Parcial | Sí (`customer_journey.ps1`) |
| CAPA 5-Why, Ishikawa, cierre | Parcial | Sí |
| Auditoría program→close completa | Parcial | Sí |
| Fichas técnicas CREATE | Requiere permiso RBAC extra | Sí |
| Break-glass Support Operator UI dedicada | No existe pantalla dedicada | JWT `PLATFORM.SUPPORT.ACCESS` |
| Login v2 identify/multi-tenant | Sí | Sí |
| Export reportes PDF/Excel | Botones en Report Center | API completa |

Los manuales indican **Pasos UI** y, cuando corresponda, **Extensión API** con endpoint exacto.

---

## 12. Índice de documentos

| Archivo | Contenido |
|---------|-----------|
| 00 | Este roadmap |
| 01–16 | Manuales por rol |
| 17 | Procesos E2E transversales |
| 18 | Plantilla resultados |
| 19 | Plantilla defectos |
| 20 | Checklist final |
| 21 | Reporte calidad documentación |

---

## 13. Roles oficiales (17)

**Plataforma:** Platform Administrator, Platform Operations, Platform Security, Support Operator  

**Tenant:** Tenant Administrator, Tenant Security Administrator, Document Controller, Quality Manager, Auditor, Supplier Manager, CAPA Manager, Risk Manager, Indicators Manager, Reporting Manager, Storage Administrator, Notification Administrator, Viewer

Manuales 01–16 cubren los roles operativos del plan. Platform Operations y Platform Security pueden reutilizar casos de 01 con alcance reducido (documentado en 01, Sección final).

---

## 14. Veredicto de entrada al programa

Antes de comenzar el caso **TC-PA-001**, confirme:

- [ ] Aplicación en `http://localhost:5272`
- [ ] PostgreSQL accesible
- [ ] `e2e/testdata.json` generado o tenant manual creado
- [ ] Credenciales obtenidas desde configuración local (sin compartir en documentos)
- [ ] Carpeta `artifacts/manual-functional-testing/` creada

**Siguiente paso:** Abrir `01_PLATFORM_ADMINISTRATOR_FUNCTIONAL_TESTS.md`
