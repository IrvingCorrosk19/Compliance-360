# Manual funcional — CAPA Manager

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** CAPA Manager

## 1. Propósito del rol

Gestiona **CAPAs** de extremo a extremo excepto **aprobación/cierre final** (Quality Manager — SoD). Incluye análisis 5-Why/Ishikawa parcial en UI.

## 2. Precondiciones

- Auditoría opcional (manual 10)
- Usuario `capa@alimentos-premium.test`

## 3. Credenciales

| Email | `capa@alimentos-premium.test` |
| Contraseña | `e2e/testdata.json` |

## 4. Datos de prueba

| Campo | Valor |
|-------|-------|
| Nombre / titulo | CAPA Desviacion Temperatura Camara MFT |
| Codigo | CAPA-MFT-001 |
| Descripcion | No conformidad detectada en cadena de frio — lote L-2026-001 |

## 5. Casos de prueba

### TC-CM-001 — Login CAPA Manager

**Pasos:** Login v2 → `#/capa`.

---

### TC-CM-002 — Navegar CAPA Module

| **Ruta** | `#/capa` |

**Pasos:**

1. `#/capa`.
2. Formulario: **Nombre / titulo**, **Codigo**, **Descripcion**.

---

### TC-CM-003 — Crear CAPA (Crítica)

**Pasos:**

1. Complete campos sección 4.
2. **Crear registro real**.
3. Anote CAPA ID para TC-QM-CAPA-006 (manual 07).

**Auditoría:** CapaCreated

---

### TC-CM-004 — Segunda CAPA

**Pasos:** CAPA-MFT-002, Descripcion: Hallazgo auditoria interna.

---

### TC-CM-005 — Código duplicado (negativo)

**Pasos:** CAPA-MFT-001 repetido → error.

---

### TC-CM-006 — Avanzar CAPA a PendingApproval (UI/API)

**Pasos:**

1. Abra CAPA creada.
2. Si UI permite, envíe a aprobación.
3. Si no, use `customer_journey.ps1` o Swagger para submit.
4. Estado → PendingApproval.

**Handoff:** Quality Manager manual 07.

---

### TC-CM-007 — Sin aprobar propia CAPA

**Pasos:**

1. Busque botón Aprobar/Cerrar como capa@.
2. Confirme no disponible (sin CAPA.APPROVE).

---

### TC-CM-008 — Consultar auditorías

**Pasos:** `#/audits` → lectura.

---

### TC-CM-009 — Consultar riesgos

**Pasos:** `#/risks` → lectura.

---

### TC-CM-010 — Reportes

**Pasos:** `#/reports` → lectura.

---

### TC-CM-011 — Audit Trail

**Pasos:** CapaCreated → **Exportar**.

---

### TC-CM-012 — Logout

**Pasos:** **Salir**.

## 6. Validaciones negativas

- Duplicado, auto-aprobación, tenant admin.

## 7. Validación visual

- Labels CAPA en Action Center.
- Estados CAPA en listado (Draft, PendingApproval, Closed).

## 8. Permisos esperados

**Debe:** CAPA.MANAGE, CAPA.READ, `#/capa`.

**No debe:** CAPA.APPROVE, CAPA.CLOSE (QM).

## 9. Auditoría

CapaCreated, CapaSubmitted (si aplica).

## 10. Criterio aprobación

TC-CM-003 + TC-CM-006 (handoff QM) PASS.

## 11. Qué aprendí

CAPA conecta hallazgos con acciones correctivas; el cierre requiere Quality Manager.

## 12. Referencias

- `#/capa`
- 5-Why/Ishikawa: UI parcial — `scripts/customer_journey.ps1`
