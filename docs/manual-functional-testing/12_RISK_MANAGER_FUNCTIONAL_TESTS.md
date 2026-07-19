# Manual funcional — Risk Manager

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Risk Manager

## 1. Propósito del rol

Gestiona el **registro de riesgos**, tratamientos y controles. La **aprobación** pertenece al Quality Manager (SoD).

## 2. Precondiciones

- Usuario `risk@alimentos-premium.test`

## 3. Credenciales

| Email | `risk@alimentos-premium.test` |
| Contraseña | `e2e/testdata.json` |

## 4. Datos de prueba

| Campo | Valor |
|-------|-------|
| Nombre / titulo | Riesgo Contaminacion Cruzada MFT |
| Codigo | RISK-MFT-001 |
| Area | Production |
| Proceso | Packaging |

## 5. Casos de prueba

### TC-RM-001 — Login Risk Manager

**Pasos:** Login v2 → `#/risks`.

---

### TC-RM-002 — Navegar Risks Module

| **Ruta** | `#/risks` |

**Pasos:**

1. `#/risks`.
2. Campos: **Nombre / titulo**, **Codigo**, **Area**, **Proceso**.

---

### TC-RM-003 — Crear riesgo (Crítica)

**Pasos:**

1. Complete sección 4.
2. **Crear registro real**.
3. Anote Risk ID para TC-QM-RISK-008.

**Auditoría:** RiskCreated

---

### TC-RM-004 — Segundo riesgo

**Pasos:** RISK-MFT-002, Area: Quality, Proceso: Receiving.

---

### TC-RM-005 — Código duplicado (negativo)

**Pasos:** RISK-MFT-001 repetido → error.

---

### TC-RM-006 — Enviar a workflow aprobación

**Pasos:**

1. Transición riesgo a estado pendiente aprobación (UI o API).
2. Handoff a quality@.

---

### TC-RM-007 — Sin aprobar riesgo

**Pasos:** risk@ sin botón approve → confirmar SoD.

---

### TC-RM-008 — Indicators lectura

**Pasos:** `#/indicators` → lectura (INDICATOR.READ en Risk Manager).

---

### TC-RM-009 — Reportes

**Pasos:** `#/reports` → lectura.

---

### TC-RM-010 — Audit Trail

**Pasos:** RiskCreated → **Exportar**.

---

### TC-RM-011 — Consultar CAPA vinculada

**Pasos:** `#/capa` → lectura cruzada.

---

### TC-RM-012 — Logout

**Pasos:** **Salir**.

## 6. Validaciones negativas

- Duplicado, auto-aprobación, crear indicadores (sin INDICATOR.MANAGE).

## 7. Validación visual

- Campos **Area** y **Proceso** en formulario.
- Matriz/listado riesgos.

## 8. Permisos esperados

**Debe:** RISK.MANAGE, RISK.READ, `#/risks`.

**No debe:** RISK.APPROVE (QM).

## 9. Auditoría

RiskCreated, RiskSubmitted.

## 10. Criterio aprobación

TC-RM-003 PASS.

## 11. Qué aprendí

Riesgos alimentan indicadores y reportes de compliance; aprobación es control de calidad.

## 12. Referencias

- `#/risks`
- Aprobación QM: manual 07 TC-QM-RISK-008
