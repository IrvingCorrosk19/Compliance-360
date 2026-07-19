# Manual funcional — Auditor

**Versión:** 1.0 | **URL:** `http://localhost:5272`

## 1. Propósito del rol

Planifica y ejecuta **auditorías** y hallazgos. No cierra CAPAs (SoD).

## 2. Precondiciones

Usuario `auditor@alimentos-premium.test` provisionado.

## 3. Credenciales

Email `auditor@alimentos-premium.test` | Contraseña `e2e/testdata.json`

## 4. Datos de prueba

| Nombre / titulo | Auditoria Interna ISO 22000 MFT |
| Codigo | AUD-MFT-001 |
| Alcance | Sistema de gestion de calidad e inocuidad alimentaria |

## 5. Casos de prueba

Login v2: auditor@alimentos-premium.test → **Siguiente** → **Continuar** → **Iniciar sesion**


### TC-AU-001 — Login Auditor

| Campo | Valor |
|-------|-------|
| **Objetivo** | Login Auditor |
| **Prioridad** | Crítica |
| **Tipo** | Positivo |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. Login v2 auditor@
2. Verifique **Audits** en sidebar

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-001/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---

### TC-AU-002 — Navegar Audits

| Campo | Valor |
|-------|-------|
| **Objetivo** | Navegar Audits |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. `#/audits`
2. Formulario `#module-action-form`
3. Campos Nombre, Codigo, Alcance

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-002/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---

### TC-AU-003 — Crear auditoría

| Campo | Valor |
|-------|-------|
| **Objetivo** | Crear auditoría |
| **Prioridad** | Crítica |
| **Tipo** | Positivo |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. Nombre: Auditoria Interna ISO 22000 MFT
2. Codigo: AUD-MFT-001
3. Alcance: Sistema de gestion de calidad
4. **Crear registro real**
5. Anote Audit ID

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** AuditsCreated

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-003/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---

### TC-AU-004 — Segunda auditoría

| Campo | Valor |
|-------|-------|
| **Objetivo** | Segunda auditoría |
| **Prioridad** | Media |
| **Tipo** | Positivo |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. AUD-MFT-002
2. Alcance: Proveedores criticos
3. **Crear registro real**

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-004/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---

### TC-AU-005 — Código duplicado

| Campo | Valor |
|-------|-------|
| **Objetivo** | Código duplicado |
| **Prioridad** | Negativo |
| **Tipo** | Negativo |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. Repetir AUD-MFT-001
2. Error validación

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-005/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---

### TC-AU-006 — Suppliers lectura

| Campo | Valor |
|-------|-------|
| **Objetivo** | Suppliers lectura |
| **Prioridad** | Positivo |
| **Tipo** | Positivo |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. `#/suppliers`
2. Listado sin crear

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-006/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---

### TC-AU-007 — CAPA sin cierre

| Campo | Valor |
|-------|-------|
| **Objetivo** | CAPA sin cierre |
| **Prioridad** | Permisos |
| **Tipo** | Permisos |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. `#/capa`
2. Sin aprobar/cerrar

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-007/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---

### TC-AU-008 — Documents lectura

| Campo | Valor |
|-------|-------|
| **Objetivo** | Documents lectura |
| **Prioridad** | Positivo |
| **Tipo** | Positivo |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. `#/documents`
2. Evidencia auditoría

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-008/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---

### TC-AU-009 — Report Center

| Campo | Valor |
|-------|-------|
| **Objetivo** | Report Center |
| **Prioridad** | Positivo |
| **Tipo** | Positivo |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. `#/reports`
2. Lectura

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-009/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---

### TC-AU-010 — Audit Trail

| Campo | Valor |
|-------|-------|
| **Objetivo** | Audit Trail |
| **Prioridad** | Alta |
| **Tipo** | Positivo |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. AuditCreated
2. **Exportar**

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-010/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---

### TC-AU-011 — Extensión API program/close

| Campo | Valor |
|-------|-------|
| **Objetivo** | Extensión API program/close |
| **Prioridad** | Positivo |
| **Tipo** | Positivo |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. Referencia customer_journey.ps1
2. Documente UI parcial

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-011/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---

### TC-AU-012 — Logout

| Campo | Valor |
|-------|-------|
| **Objetivo** | Logout |
| **Prioridad** | Baja |
| **Tipo** | Positivo |
| **Precondiciones** | TC-AU-001 PASS |
| **Datos** | Ver sección 4 |
| **Ruta inicial** | `#/audits` |
| **Menú** | Operations → Audits |
| **Pantalla** | Audits |

**Pasos:**

1. **Salir**

**Resultado final esperado:** Según objetivo del caso

**Auditoría esperada:** N/A

**Evidencia:** `artifacts/manual-functional-testing/Auditor/TC-AU-012/`

**Estado:** [ ] PASS  [ ] FAIL  [ ] BLOCKED  [ ] PENDING THIRD-PARTY

---


## 6. Validaciones negativas

Ver casos marcados **Negativo** y **Permisos** en sección 5.

## 7. Validación visual

Loaders, toasts, labels exactos del Action Center, listados refrescados post-creación.

## 8. Permisos esperados

Ver `RoleCatalog.cs` para permisos oficiales del rol Auditor.

## 9. Auditoría

Cada mutación exitosa genera evento en `#/audit-trail` con actor, tenantId, correlationId.

## 10. Criterio de aprobación del rol

TC-AU-003 PASS obligatorio. Mínimo 10/12 casos PASS.

## 11. Qué aprendí

Este rol es pieza del programa MFT; ejecute en orden del roadmap `00`.

## 12. Referencias y extensiones API

- URL: `http://localhost:5272`
- Journey: `scripts/customer_journey.ps1`
- Credenciales: `e2e/testdata.json`
