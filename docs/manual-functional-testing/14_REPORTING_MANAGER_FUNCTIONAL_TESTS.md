# Manual funcional — Reporting Manager

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Rol:** Reporting Manager

## 1. Propósito del rol

Dueño del **Report Center**: ejecutar, exportar, programar y administrar reportes empresariales que consumen datos de todos los módulos.

## 2. Precondiciones

- Datos en módulos previos (docs, CAPA, riesgos, indicadores)
- Usuario `reporting@alimentos-premium.test`

## 3. Credenciales

| Email | `reporting@alimentos-premium.test` |
| Contraseña | `e2e/testdata.json` |

## 4. Datos de prueba

| Acción | Detalle |
|--------|---------|
| Seed | Reportes estándar vía **Seed standard reports** |
| Schedule | Programación mensual |
| Export | PDF/Excel vía Report Center |

## 5. Casos de prueba

### TC-RP-001 — Login Reporting Manager

**Pasos:** Login v2 → `#/reports`.

---

### TC-RP-002 — Abrir Report Center

| **Ruta** | `#/reports` |

**Pasos:**

1. Navegue a `#/reports`.
2. Confirme encabezado **Report Center**.
3. Subtítulo: *Busca, ejecuta, programa y exporta reportes empresariales.*
4. Localice botones: **Ejecutar**, **Programar mensual**, **Seed standard reports**.

---

### TC-RP-003 — Seed standard reports (Crítica)

**Pasos:**

1. Presione **Seed standard reports**.
2. Espere toast éxito.
3. Verifique filas de reportes en tabla/listado.
4. Confirme botón **Ejecutar** habilitado si hay filas.

**Auditoría:** ReportsSeeded

---

### TC-RP-004 — Ejecutar reporte

**Pasos:**

1. Seleccione primer reporte del listado.
2. Presione **Ejecutar**.
3. Espere resultado (preview, descarga o toast).
4. Verifique sin error 5xx.

**Auditoría:** ReportExecuted

---

### TC-RP-005 — Programar mensual

**Pasos:**

1. Con reportes en listado, presione **Programar mensual**.
2. Confirme schedule creado o toast.
3. Documente próxima ejecución si se muestra.

**Auditoría:** ReportScheduled

---

### TC-RP-006 — Exportar via reportes

**Pasos:**

1. Desde otro módulo (ej. dashboard), localice **Exportar via reportes** (data-route reports).
2. O desde Report Center, exporte PDF/Excel si botón disponible.
3. Guarde archivo en evidencias.

**Resultado esperado:** Archivo exportado o enlace descarga.

---

### TC-RP-007 — Búsqueda reportes

**Pasos:**

1. Use campo búsqueda del Report Center.
2. Filtre por nombre parcial.
3. Verifique resultados filtrados.

---

### TC-RP-008 — Lectura transversal módulos

**Pasos:**

1. `#/documents`, `#/capa`, `#/risks` → solo lectura.
2. Confirma acceso REPORT.READ a datos agregados.

---

### TC-RP-009 — Sin crear registros operativos

**Pasos:** `#/documents` → **Modo solo lectura**; sin **Crear registro real**.

---

### TC-RP-010 — Audit Trail reportes

**Pasos:** `#/audit-trail` → ReportExecuted, ReportScheduled → **Exportar**.

---

### TC-RP-011 — Programar sin reportes (negativo)

**Pasos:**

1. En tenant vacío sin seed, **Programar mensual** disabled.
2. Confirme UX coherente.

---

### TC-RP-012 — Logout

**Pasos:** **Salir**.

## 6. Validaciones negativas

- Ejecutar sin reportes, crear documento, tenant admin.

## 7. Validación visual

- Botones **Ejecutar**, **Programar mensual**, **Seed standard reports** (primary en seed).
- Tabla reportes con estados.

## 8. Permisos esperados

**Debe:** REPORT.MANAGE, REPORT.EXECUTE, REPORT.EXPORT, REPORT.SCHEDULE.

**No debe:** DOCUMENT.CREATE, CAPA.MANAGE.

## 9. Auditoría

ReportsSeeded, ReportExecuted, ReportScheduled, ReportExported.

## 10. Criterio aprobación

TC-RP-003 + TC-RP-004 PASS.

## 11. Qué aprendí

Report Center consolida evidencia de compliance para auditorías externas y dirección.

## 12. Referencias

- `#/reports` — Report Center
- Botón dashboard: **Exportar via reportes**
