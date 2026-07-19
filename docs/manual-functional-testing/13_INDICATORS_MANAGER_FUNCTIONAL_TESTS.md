# Manual funcional — Indicators Manager

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Indicators Manager

## 1. Propósito del rol

Gestiona **indicadores de calidad**, fórmulas, umbrales y mediciones. Exporta datos para dashboards y reportes.

## 2. Precondiciones

- Usuario `indicators@alimentos-premium.test`
- Riesgos opcionales (manual 12)

## 3. Credenciales

| Email | `indicators@alimentos-premium.test` |
| Contraseña | `e2e/testdata.json` |

## 4. Datos de prueba

| Campo | Valor |
|-------|-------|
| Nombre / titulo | Indicador Conformidad Lotes MFT |
| Codigo | IND-MFT-001 |
| Unidad | % |

## 5. Casos de prueba

### TC-IM-001 — Login Indicators Manager

**Pasos:** Login v2 → `#/indicators`.

---

### TC-IM-002 — Navegar Indicators Module

| **Ruta** | `#/indicators` |

**Pasos:**

1. `#/indicators`.
2. Formulario: **Nombre / titulo**, **Codigo**, **Unidad**.

---

### TC-IM-003 — Crear indicador (Crítica)

**Pasos:**

1. Nombre: Indicador Conformidad Lotes MFT
2. Codigo: IND-MFT-001
3. Unidad: %
4. **Crear registro real**.

**Auditoría:** IndicatorCreated

---

### TC-IM-004 — Segundo indicador

**Pasos:** IND-MFT-002, Unidad: ppm, Nombre: Defectos por millon.

---

### TC-IM-005 — Código duplicado (negativo)

**Pasos:** IND-MFT-001 repetido → error.

---

### TC-IM-006 — Unidad vacía (negativo)

**Pasos:** Dejar **Unidad** vacío si required → validación.

---

### TC-IM-007 — Consultar riesgos

**Pasos:** `#/risks` → lectura.

---

### TC-IM-008 — Reportes lectura

**Pasos:** `#/reports` → datos alimentan reportes.

---

### TC-IM-009 — Sin crear documentos

**Pasos:** `#/documents` → read-only.

---

### TC-IM-010 — Audit Trail

**Pasos:** IndicatorCreated → **Exportar**.

---

### TC-IM-011 — Export indicador (si UI)

**Pasos:** Busque acción export/INDICATOR.EXPORT en módulo.

---

### TC-IM-012 — Logout

**Pasos:** **Salir**.

## 6. Validaciones negativas

- Duplicado, campos vacíos, create documento.

## 7. Validación visual

- Campo **Unidad** visible.
- Listado indicadores.

## 8. Permisos esperados

**Debe:** INDICATOR.MANAGE, INDICATOR.EXPORT, `#/indicators`.

**No debe:** DOCUMENT.CREATE, REPORT.SCHEDULE (Reporting Manager).

## 9. Auditoría

IndicatorCreated, IndicatorMeasurementRecorded (API).

## 10. Criterio aprobación

TC-IM-003 PASS.

## 11. Qué aprendí

Indicadores cuantifican el desempeño del SGCC y alimentan Report Center.

## 12. Referencias

- `#/indicators`
- Siguiente: `14_REPORTING_MANAGER_FUNCTIONAL_TESTS.md`
