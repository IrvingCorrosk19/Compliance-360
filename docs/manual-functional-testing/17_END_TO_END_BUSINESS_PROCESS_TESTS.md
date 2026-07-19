# Manual funcional — Procesos E2E transversales

**Versión:** 1.0 | **URL:** `http://localhost:5272` | **Alcance:** Multi-rol, ciclo de negocio completo

## 1. Propósito

Validar **handoffs entre roles** y flujos de negocio que ningún manual individual cubre por completo. Complementa manuales 01–16 con escenarios transversales.

## 2. Precondiciones

- Manuales 01–16 ejecutados o datos equivalentes provisionados
- `scripts/e2e_provision.ps1` + `scripts/customer_journey.ps1` disponibles
- Usuarios `@alimentos-premium.test` en `e2e/testdata.json`

## 3. Credenciales

Todos los usuarios por rol en `e2e/testdata.json`. Platform: `admin@compliance360.local` / tenant `dc7c46ee-cb25-4ed5-b0b4-800788f7f626`.

## 4. Datos de prueba E2E

| Entidad | Código sugerido |
|---------|-----------------|
| Documento | E2E-DOC-001 |
| Proveedor | E2E-SUP-001 |
| Auditoría | E2E-AUD-001 |
| CAPA | E2E-CAPA-001 |
| Riesgo | E2E-RISK-001 |
| Indicador | E2E-IND-001 |

## 5. Casos de prueba

### E2E-001 — Onboarding cliente completo (Plataforma → Tenant Admin)

| **Prioridad** | Crítica |
| **Roles** | Platform Administrator → Tenant Administrator |

**Pasos:**

1. **Platform Admin** (`admin@compliance360.local`): login v2 → `#/superadmin-platform` → **Crear tenant** con slug único `e2e-onboard-{fecha}`.
2. Anote Tenant ID y credenciales admin inicial.
3. **Salir**.
4. **Tenant Admin** del nuevo tenant: login v2 → `#/tenant-administration`.
5. Tab **Informacion General** → **Guardar informacion general**.
6. Tab **Usuarios** → crear Document Controller.
7. Tab **Estado** → **Activar** tenant Active.
8. Verifique handoff: tenant operativo.

**Auditoría:** TenantCreated → UserCreated → TenantStatusChanged

---

### E2E-002 — Ciclo documental (Document Controller → Quality Manager)

| **Prioridad** | Crítica |
| **Roles** | Document Controller → Quality Manager |

**Pasos:**

1. **doccontrol@**: `#/documents` → crear E2E-DOC-001 (**Crear registro real**).
2. Anote documentId (Network tab).
3. **Salir**.
4. **quality@**: `#/documents` → confirmar **Modo solo lectura** para create.
5. Swagger: POST `/api/v1/tenants/{tenantId}/documents/{documentId}/decision` body `{ "decision": "Approved", "comments": "E2E approve" }`.
6. **doccontrol@** o **quality@**: verificar estado Approved en listado.
7. `#/audit-trail`: DocumentCreated + DocumentApproved.

**Referencia automatizada:** `scripts/customer_journey.ps1`

---

### E2E-003 — Infraestructura → Operaciones (Storage → Documentos con adjunto)

| **Roles** | Storage Administrator → Document Controller |

**Pasos:**

1. **storage@**: `#/configuration` → **Crear Storage Local** → **Probar primer Storage Provider**.
2. **doccontrol@**: crear documento E2E-DOC-002.
3. Si UI expone upload, adjunte archivo; si no, `customer_journey.ps1` Upload-File.
4. Verifique archivo asociado sin error.

---

### E2E-004 — Auditoría → CAPA → Cierre QM

| **Roles** | Auditor → CAPA Manager → Quality Manager |

**Pasos:**

1. **auditor@**: `#/audits` → E2E-AUD-001 (**Crear registro real**).
2. **capa@**: `#/capa` → E2E-CAPA-001 vinculada conceptualmente al hallazgo.
3. Avanzar CAPA a PendingApproval (UI o API/journey).
4. **quality@**: aprobar/cerrar CAPA vía API (manual 07 TC-QM-CAPA-006).
5. Audit trail: AuditCreated → CapaCreated → CapaApproved/Closed.

---

### E2E-005 — Riesgo → Aprobación QM → Indicador → Reporte

| **Roles** | Risk Manager → Quality Manager → Indicators Manager → Reporting Manager |

**Pasos:**

1. **risk@**: E2E-RISK-001 en `#/risks`.
2. Submit a aprobación → **quality@** approve API.
3. **indicators@**: E2E-IND-001 en `#/indicators`.
4. **reporting@**: `#/reports` → **Seed standard reports** → **Ejecutar** reporte que incluya riesgos/indicadores.
5. **Exportar via reportes** o export desde Report Center.

---

### E2E-006 — Seguridad tenant afecta login

| **Roles** | Tenant Security Administrator → Viewer |

**Pasos:**

1. **security@**: `#/tenant-administration` tab **Seguridad** → lockoutMaxFailedAttempts=3 → **Guardar seguridad**.
2. **Salir**.
3. **viewer@**: login con contraseña incorrecta 3 veces.
4. Verifique lockout (mensaje coherente).
5. **security@**: restaurar lockout=5; **viewer@** login correcto.

---

### E2E-007 — Viewer no contamina datos

| **Rol** | Viewer |

**Pasos:**

1. **viewer@**: recorrer `#/documents`, `#/capa`, `#/risks`.
2. Confirmar **Modo solo lectura** en todos.
3. DevTools: POST create document → 403.
4. Audit: AuthorizationDenied, sin DocumentCreated por viewer.

---

### E2E-008 — Journey automatizado vs manual

**Pasos:**

1. Ejecute `powershell -ExecutionPolicy Bypass -File scripts/customer_journey.ps1`.
2. Revise JSON de evidencia generado en artifacts.
3. Compare pasos con E2E-001..005 — mismos endpoints y estados finales.

**Resultado:** PASS si journey script completa sin FAIL steps.

---

### E2E-009 — Login v2 multi-tenant vs legacy

**Pasos:**

1. Login v2 con usuario `@alimentos-premium.test` → selector org → **Continuar**.
2. `localStorage.setItem('c360.login.v2', 'false'); location.reload()`.
3. Login legacy con **Tenant ID** explícito + email + **Iniciar sesion**.
4. Restaurar v2: `localStorage.removeItem('c360.login.v2'); location.reload()`.

---

### E2E-010 — Trazabilidad audit trail transversal

**Pasos:**

1. Tras E2E-002..005, `#/audit-trail` como **reporting@** o **tenantadmin@**.
2. **Exportar**.
3. Verifique cadena correlacionada: Created → Submitted → Approved por módulo.

---

### E2E-011 — Support Operator acceso contrastado

**Pasos:**

1. **admin@** (Platform Admin): GET documents tenant negocio → 403.
2. **support@**: mismo GET → 200 si SUPPORT.ACCESS activo.
3. Documente en audit trail.

---

### E2E-012 — Limpieza y re-ejecución

**Pasos:**

1. Use códigos `-RUN2` para re-ejecutar sin colisiones.
2. O re-provision: `e2e_provision.ps1`.
3. Confirme tenant Active post-provision.

## 6. Validaciones negativas transversales

- Viewer mutación → 403 en todos los módulos.
- QM create documento UI → Modo solo lectura.
- Platform Admin sin support → 403 datos negocio.
- Slug/tax duplicado en onboarding → 400.

## 7. Validación visual transversal

- Toasts consistentes en handoffs.
- Breadcrumbs y loaders uniformes.
- Empty states vs listados poblados post-E2E.

## 8. Matriz de permisos E2E

| Transición | Permiso clave |
|------------|---------------|
| DC → QM doc | DOCUMENT.CREATE → DOCUMENT.APPROVE |
| CM → QM capa | CAPA.MANAGE → CAPA.APPROVE |
| RM → QM risk | RISK.MANAGE → RISK.APPROVE |
| SA → DC file | STORAGE.* → upload |
| Support → data | PLATFORM.SUPPORT.ACCESS |

## 9. Auditoría E2E

Cadena completa debe ser reconstruible desde `#/audit-trail` + export CSV.

## 10. Criterio aprobación E2E

**E2E-001, E2E-002, E2E-008 PASS obligatorios.** Mínimo 10/12 escenarios PASS.

## 11. Qué aprendí

Los manuales por rol son piezas; E2E demuestra que Compliance 360 funciona como **sistema integrado** con SoD y trazabilidad.

## 12. Referencias

- `scripts/customer_journey.ps1` — ciclo completo automatizado
- `scripts/e2e_provision.ps1` — usuarios @alimentos-premium.test
- Roadmap orden: `00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md` sección 4
- Plantillas: `18`, `19`, `20`
