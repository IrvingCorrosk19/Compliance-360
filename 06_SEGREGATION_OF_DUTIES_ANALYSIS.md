# 06 — Segregation of Duties Analysis

## Marco de referencia

Evaluación contra principios de **Segregación de Funciones (SoD)** alineados con:

- ISO 27001 A.5.3 — segregación de deberes
- ISO 9001 — responsabilidades de elaboración, verificación y aprobación
- COBIT / ITIL — separación admin vs operación
- Patrones SaaS — control plane vs data plane

---

## Controles SoD evaluados

| ID | Control SoD | Estado | Severidad |
|----|-------------|--------|-----------|
| SoD-01 | Elaboración ≠ aprobación documentos | **FALLA** | Alta |
| SoD-02 | Administración usuarios ≠ operación negocio | **PARCIAL** | Media |
| SoD-03 | Plataforma ≠ operación tenant | **FALLA** | Crítica |
| SoD-04 | Auditoría ≠ corrección (CAPA) | **PARCIAL** | Media |
| SoD-05 | Configuración seguridad ≠ uso diario | **PARCIAL** | Media |
| SoD-06 | Creación reportes ≠ ejecución | **PARCIAL** | Baja |
| SoD-07 | Storage ≠ notificaciones | **FALLA** | Baja |
| SoD-08 | RBAC admin ≠ auto-asignación | **OK** | — |
| SoD-09 | Consulta (Viewer) ≠ mutación | **PARCIAL** | Media |
| SoD-10 | Cross-tenant isolation | **PARCIAL** | Alta |

---

## SoD-01: Documentos — elaboración vs aprobación

### Esperado
Document Controller elabora; Quality Manager (o rol Aprobador) aprueba con permiso distinto.

### Observado
- Un solo permiso `DOCUMENT.MANAGE` cubre crear, versionar, `submit`, `decision` (aprobar/rechazar), `obsolete`
- Endpoint `POST .../documents/{id}/decision` requiere `Document.Manage` — misma policy que creación
- Quality Manager y Document Controller comparten `DOCUMENT.MANAGE` en E2E

### Evidencia
`FoundationEndpoints.cs` — grupo `/documents` → `PermissionPolicies.DocumentManage`

### Riesgo
Usuario que elabora puede auto-aprobar — violación directa ISO 9001 cláusula 7.5.

### Recomendación (fase 2)
Introducir `DOCUMENT.APPROVE` y separar `decision` endpoint.

---

## SoD-02: Administración vs operación

### Esperado
Tenant Admin administra; no opera CAPA/documentos de producción.

### Observado
- Tenant Admin E2E **no** tiene `DOCUMENT.MANAGE`, `CAPA.MANAGE` — **correcto**
- Tenant Admin **sí** tiene `RBAC.MANAGE`, `IDENTITY.MANAGE` — **correcto**
- Tenant Admin tiene `AUDITMANAGEMENT.MANAGE` para dashboard — **desviación menor**

### Riesgo
Bajo para Tenant Admin; medio si se otorgan permisos operativos por conveniencia.

---

## SoD-03: Plataforma vs data plane (crítico)

### Esperado
SuperAdmin solo control plane: tenants, licencias, observabilidad. Sin acceso a documentos/CAPA de clientes.

### Observado
1. `HasPlatformSuperAdmin` — bypass total de policies (`PermissionPolicies.cs` L177–181)
2. Seed otorga los 76 permisos incluyendo `DOCUMENT.MANAGE`, `CAPA.MANAGE`, etc.
3. `ApiContext.TenantId` permite cross-tenant para SuperAdmin
4. UI muestra todos los módulos operativos al SuperAdmin

### Evidencia
`ApiContext.cs` L28–32; `DevelopmentBootstrap.cs` L767–845

### Riesgo
**Crítico** — acceso ilimitado a datos de todos los tenants; incompatible con SOC2/SaaS enterprise.

---

## SoD-04: Auditor vs CAPA

### Esperado
Auditor registra hallazgos; CAPA Manager ejecuta correcciones. Auditor no cierra su propia CAPA sin revisión.

### Observado
- Auditor E2E tiene `CAPA.MANAGE` y `AUDITMANAGEMENT.MANAGE`
- Puede crear auditoría y CAPA en mismo flujo (`e2e_auditor_visible.py`)
- `CAPA.CLOSE` no otorgado a Auditor en E2E — **parcialmente correcto**

### Riesgo
Medio — auditor puede implementar acciones correctivas sin segregación del ejecutor.

---

## SoD-05: Seguridad vs operación

### Esperado
Tenant Admin / Security Admin configura MFA, SSO; operadores no modifican políticas de seguridad.

### Observado
- `TENANT.SECURITY` separado de permisos operativos — **correcto en catálogo**
- Roles operativos E2E no tienen `TENANT.SECURITY` — **correcto**
- MFA verify endpoint: autenticado sin `IDENTITY.MANAGE` (`FoundationEndpoints.cs`) — revisar

### Riesgo
Bajo en configuración; verificar endpoint MFA verify.

---

## SoD-06: Diseño vs ejecución de reportes

### Esperado
Reporting Manager ejecuta; diseño de definiciones requiere `REPORT.MANAGE`.

### Observado
- Reporting Manager E2E: `REPORT.READ`, `REPORT.EXECUTE` sin `REPORT.MANAGE` — **correcto**
- Quality Manager tiene `REPORT.EXPORT` adicional — solapamiento menor

### Riesgo
Bajo.

---

## SoD-07: Storage vs Notifications

### Esperado
Roles separados para storage y email.

### Observado
- Notification Admin E2E incluye `STORAGE.MANAGE`
- Ambos usan pantalla `configuration` (`app.js`)
- `STORAGE.MANAGE` en `routeManagePermissions` para configuration

### Riesgo
Bajo-Medio — un Notification Admin puede gestionar storage providers.

---

## SoD-08: RBAC self-assignment

### Esperado
Usuario no puede auto-elevarse permisos.

### Observado
- `RbacService` requiere actor con permisos existentes vía API
- No hay endpoint "grant self" explícito
- Tenant Admin con `RBAC.MANAGE` puede asignarse roles — **aceptable** si es único admin

### Riesgo
Bajo con un solo Tenant Admin; alto si hay muchos `RBAC.MANAGE`.

---

## SoD-09: Viewer read-only

### Esperado
Viewer solo consulta; API y UI bloquean mutaciones.

### Observado
- **API:** POST sin `*.MANAGE` → 403 — **correcto**
- **UI:** Tras fix E2E, `canManageRoute` oculta formularios — **correcto**
- **Gaps:** `renderRoute` no valida `canNavigate` — hash directo carga pantalla
- Quick-switcher lista todas las rutas
- Enterprise workspaces permiten crear con solo `TENANT.READ`

### Evidencia
`app.js` L653–708 (sin guard), L2303–2316 (enterprise create sin gate)

### Riesgo
Medio — UI puede mostrar pantallas; API rechaza, pero UX confusa y posible información en layout.

---

## SoD-10: Aislamiento multi-tenant

### Esperado
Rol tenant A no accede tenant B.

### Observado
- `ApiContext.TenantId` valida claim — **correcto** para roles normales
- SuperAdmin bypass — **excepción documentada**
- `RbacService.AuthorizeAsync` valida `EntityTenantId` — **correcto**
- EF queries usan `TenantId` en entidades — **correcto** (asumido en repositories)

### Riesgo
Alto solo vía SuperAdmin o bug IDOR; tests en `RbacFoundationTests` verifican cross-tenant deny.

---

## Matriz SoD conflicto × rol

| Conflicto | Roles afectados | Permisos en conflicto |
|-----------|-----------------|---------------------|
| Elaborar + aprobar documento | Doc Ctrl + QM | DOCUMENT.MANAGE |
| Auditar + ejecutar CAPA | Auditor + CAPA Mgr | CAPA.MANAGE en ambos |
| Gestionar proveedor + CAPA | Supplier Mgr | SUPPLIER.MANAGE + CAPA.MANAGE |
| Operar + administrar plataforma | SuperAdmin | Bypass + todos |
| Gestionar KPI + aprobar KPI | Indicators Mgr | INDICATOR.MANAGE + INDICATOR.APPROVE (mismo rol posible) |
| Cerrar CAPA + crear CAPA | QM + CAPA Mgr | CAPA.MANAGE + CAPA.CLOSE en QM E2E |

---

## Comparación con plataformas enterprise

| Práctica | Microsoft 365 | Salesforce | Compliance 360 |
|----------|-----------------|------------|----------------|
| Admin global sin buzón | Sí | Sí (limitado) | No — acceso total data |
| Permission Sets composables | Sí | Sí | No — grants directos |
| Separación read/write | Extensa | Extensa | Parcial |
| SoD rules engine | Entra ID PIM | Event Monitoring | No existe |
| Break-glass auditado | Sí | Sí | No implementado |

---

## Calificación SoD

| Dimensión | Puntuación (1-10) |
|-----------|-------------------|
| Separación admin/operación tenant | 6 |
| Separación plataforma/tenant | 2 |
| Separación elaboración/aprobación | 3 |
| Separación auditor/ejecutor | 5 |
| Read-only enforcement | 6 |
| **Promedio SoD** | **4.4 / 10** |

---

## Priorización de remediación SoD

| Prioridad | Control | Esfuerzo |
|-----------|---------|----------|
| P0 | Eliminar SuperAdmin bypass en data plane | Alto |
| P1 | DOCUMENT.APPROVE separado | Medio |
| P2 | AUDITMANAGEMENT.READ para dashboard | Bajo |
| P2 | Route guard + enterprise workspace gate | Medio |
| P3 | Separar STORAGE de NOTIFICATION en Configuration UI | Bajo |
| P3 | Restringir CAPA.MANAGE en Auditor/Supplier | Bajo (config RBAC) |
