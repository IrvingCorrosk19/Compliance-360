# 02 — Role Responsibility Matrix

## Principio

Cada proceso de negocio debe tener un **responsable principal** (R), participantes (P), consultores (C) y aprobadores (A). Esta matriz contrasta el diseño **recomendado** (ISO 9001 / segregación de funciones) con el **estado observado** en código y E2E.

**Leyenda:** R = Responsable principal | P = Participa | C = Consulta | A = Aprueba | — = Sin acceso esperado | ⚠ = Mal asignado / solapamiento detectado

---

## Matriz de procesos

| Proceso | SuperAdmin | Tenant Admin | Quality Manager | Document Controller | Auditor | Supplier Manager | CAPA Manager | Risk Manager | Indicators Manager | Reporting Manager | Storage Admin | Notification Admin | Viewer |
|---------|:------------:|:------------:|:---------------:|:-------------------:|:-------:|:----------------:|:------------:|:------------:|:------------------:|:-----------------:|:-------------:|:------------------:|:------:|
| **Tenants (plataforma)** | R | — | — | — | — | — | — | — | — | — | — | — | — |
| **Licencias / planes** | R | C | — | — | — | — | — | — | — | — | — | — | — |
| **Configuración empresa** | P | R | C | C | C | C | C | C | C | C | — | — | — |
| **Usuarios tenant** | P | R | — | — | — | — | — | — | — | — | — | — | — |
| **Roles / RBAC tenant** | P | R | — | — | — | — | — | — | — | — | — | — | — |
| **Branding tenant** | P | R | — | — | — | — | — | — | — | — | — | — | — |
| **Seguridad tenant (MFA, sesiones)** | P | R | — | — | — | — | — | — | — | — | — | — | — |
| **Dominios / SSO / API keys** | P | R | — | — | — | — | — | — | — | — | — | — | — |
| **Documentos** | ⚠ P | C | A | **R** | C | C | C | C | C | C | — | — | C |
| **Workflow documental** | ⚠ P | C | A | **R** | — | — | — | — | — | — | — | — | — |
| **Fichas técnicas** | ⚠ P | C | A | P | — | P | — | — | — | — | — | — | — |
| **Proveedores** | ⚠ P | C | C | — | C | **R** | C | — | — | — | — | — | — |
| **Auditorías operativas** | ⚠ P | C | C | — | **R** | C | P | C | C | C | — | — | C |
| **Hallazgos / NC auditoría** | ⚠ P | — | A | — | **R** | — | P | — | — | — | — | — | C |
| **CAPA** | ⚠ P | C | A | C | P | ⚠ P | **R** | C | C | C | — | — | C |
| **Riesgos** | ⚠ P | C | A | — | C | — | C | **R** | C | C | — | — | C |
| **Indicadores / KPIs** | ⚠ P | C | A | — | C | — | — | C | **R** | C | — | — | C |
| **Reportes operativos** | ⚠ P | C | P | C | C | C | C | C | C | **R** | — | — | C |
| **Storage tenant** | P | C | — | P | — | — | — | — | — | — | **R** | — | — |
| **Notificaciones / SMTP** | P | C | — | — | — | — | — | — | — | — | — | **R** | — |
| **Auditoría tenant (trail)** | C | R | C | C | C | C | C | C | C | C | C | C | C |
| **Auditoría global plataforma** | **R** | — | — | — | — | — | — | — | — | — | — | — | — |
| **Observabilidad plataforma** | **R** | — | — | — | — | — | — | — | — | — | — | — | — |

---

## Responsabilidades por dominio (detalle)

### Plataforma (solo SuperAdmin)

| Dominio | Responsable recomendado | Estado actual | Evidencia |
|---------|----------------------|---------------|-----------|
| Alta/baja/suspensión tenant | SuperAdmin | SuperAdmin + bypass | `FoundationEndpoints.cs` `/tenants`, `/suspend` |
| Licencias y módulos | SuperAdmin | Permisos sembrados; UI parcial | `SUPERADMIN.LICENSES.MANAGE` sin endpoint dedicado en mayoría |
| Auditoría global export | SuperAdmin | Implementado | `/superadmin/platform-center/audit-timeline/export` |
| Observabilidad cross-tenant | SuperAdmin / Ops | `OBSERVABILITY.READ` tenant también | `ObservabilityEndpoints.cs` |

### Administración tenant (Tenant Admin)

| Dominio | Responsable recomendado | Estado actual | Evidencia |
|---------|----------------------|---------------|-----------|
| Perfil empresa | Tenant Admin | Correcto | `/tenants/{id}/general-information` → `Tenant.Update` |
| Usuarios y roles | Tenant Admin | Correcto | `Tenant.Users`, `Tenant.Roles`, `Rbac.Manage` |
| Seguridad MFA | Tenant Admin | Correcto | `Tenant.Security`, reset MFA endpoint |
| Branding | Tenant Admin | Correcto | `Tenant.Branding` |
| Facturación tenant | Tenant Admin (lectura) / SuperAdmin (límites) | Ambiguo | `Tenant.Billing` accesible desde TAC |

### Operación de calidad

| Dominio | Responsable recomendado | Estado actual | Riesgo |
|---------|----------------------|---------------|--------|
| Control documental | Document Controller | Correcto en E2E | Quality Manager también tiene `DOCUMENT.MANAGE` |
| Aprobación documentos | Quality Manager / aprobador designado | Mismo permiso `DOCUMENT.MANAGE` para crear y aprobar | **SoD violada** — no hay `DOCUMENT.APPROVE` |
| CAPA | CAPA Manager | Quality Manager y Auditor tienen `CAPA.MANAGE` en E2E | Solapamiento |
| Riesgos | Risk Manager | Quality Manager tiene `RISK.MANAGE` completo | Solapamiento |
| Indicadores | Indicators Manager | Quality Manager tiene `INDICATOR.MANAGE` completo | Solapamiento |
| Auditorías | Auditor | `AUDITMANAGEMENT.MANAGE` único — sin rol solo-lectura auditoría | Granularidad insuficiente |

### Infraestructura tenant

| Dominio | Responsable recomendado | Estado actual | Riesgo |
|---------|----------------------|---------------|--------|
| Storage | Storage Admin | Correcto | Notification Admin también tiene `STORAGE.MANAGE` |
| Notificaciones | Notification Admin | Correcto | Comparte pantalla Configuration |
| Reportes | Reporting Manager | Sin `REPORT.MANAGE` en E2E — solo execute | Diseño de reportes sin dueño claro |

---

## Matriz de ownership de módulos API

| Módulo API | Policy group | Dueño recomendado | Quién puede hoy (si tiene permiso) |
|------------|--------------|-------------------|-------------------------------------|
| `/documents` | Document.Manage | Document Controller | Cualquier usuario con `DOCUMENT.MANAGE` |
| `/workflows` | Workflow.Manage | Document Controller / QM | Idem |
| `/technical-sheets` | TechnicalSheet.Manage | Regulatorio / Calidad | Idem |
| `/suppliers` | Supplier.Manage | Supplier Manager | Idem |
| `/audit-management` | AuditManagement.Manage | Auditor | Muchos roles E2E lo tienen para dashboard |
| `/capas` | Capa.* granular | CAPA Manager | QM, Auditor, Supplier Manager |
| `/risks` | Risk.* granular | Risk Manager | QM |
| `/indicators` | Indicator.* granular | Indicators Manager | QM |
| `/reports` | Report.* granular | Reporting Manager | QM con EXPORT |
| `/storage` | Storage.Manage | Storage Admin | Notification Admin |
| `/notifications` | Notification.* | Notification Admin | — |
| `/tenants/{id}/*` admin | Tenant.* | Tenant Admin | SuperAdmin (bypass) |
| `/rbac` | Rbac.Manage | Tenant Admin | SuperAdmin |

---

## Responsabilidades que NO deberían estar en cada rol

| Rol | NO debería | Evidencia de violación actual |
|-----|------------|------------------------------|
| SuperAdmin | Operar documentos, CAPA, proveedores de un tenant | Bypass + permisos completos en seed |
| Tenant Admin | Gestionar datos de otros tenants | Correcto en API (`ApiContext`) |
| Document Controller | Aprobar CAPA, cerrar riesgos | E2E no otorgó — OK |
| Quality Manager | Administrar usuarios, SSO, API keys | E2E no otorgó — OK |
| Auditor | Homologar proveedores | E2E no otorgó — OK |
| Supplier Manager | Gestionar CAPA completas | E2E añadió `CAPA.MANAGE` por necesidad de flujo |
| CAPA Manager | Configurar SMTP/storage | E2E no otorgó — OK |
| Viewer | Cualquier mutación | Corregido con filtro UI + solo `*.READ` |
| Reporting Manager | Diseñar workflows documentales | E2E sin `WORKFLOW.MANAGE` — OK |

---

## Comparación con responsabilidades enterprise (conceptual)

| Plataforma referencia | Patrón | Compliance 360 |
|----------------------|--------|----------------|
| Microsoft 365 | Global Admin vs User Admin vs rol funcional | SuperAdmin ≈ Global Admin; Tenant Admin ≈ User Admin; faltan roles Azure-style granulares |
| Salesforce | System Admin vs Standard User + Permission Sets | Similar intención; falta Permission Sets como artefacto |
| SAP | Roles compuestos por transacciones (T-codes) | Un permiso `*.MANAGE` cubre demasiadas transacciones |
| ISO 9001 | Separación elaboración / revisión / aprobación documentos | Un solo `DOCUMENT.MANAGE` |
| ISO 27001 | Segregación admin sistema vs operación | SuperAdmin no segregado del data plane |

---

## Resumen ejecutivo de la matriz

| Métrica | Valor |
|---------|-------|
| Procesos con responsable claro en docs | 21/21 |
| Procesos con responsable único en implementación | 8/21 |
| Solapamientos críticos detectados | 6 (QM vs especialistas, SuperAdmin vs tenant, Auditor vs CAPA) |
| Módulos sin permiso READ dedicado | Documents, Technical Sheets, Suppliers, Audit Management |
| Roles con permisos dashboard ad hoc | 10/12 tenant roles (necesitan `AUDITMANAGEMENT.MANAGE` o lecturas cruzadas) |
