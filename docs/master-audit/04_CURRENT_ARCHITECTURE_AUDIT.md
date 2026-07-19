# 04 — Current Architecture Audit

**Alcance:** arquitectura *tal como está codificada* (no aspiracional).

---

## 1. Forma del sistema

| Aspecto | Evidencia |
|---------|-----------|
| Estilo | Monolito layered .NET 9 + SPA vanilla |
| Solution | `Compliance360.sln` |
| Capas | Domain ← Application ← Infrastructure ← Web |
| UI | `wwwroot/app.js` (hash router) + `form-template-builder.js` |
| API | `/api/v1` en `FoundationEndpoints.cs` |
| Persistencia | EF Core `Compliance360DbContext`, PostgreSQL, migraciones por módulo |
| Multitenancy | Shared DB + `TenantId` + claim JWT (`ApiContext`) |
| Seguridad | RBAC `PermissionCatalog` / `RoleCatalog` |
| Auditoría | `AuditLog` append-only |

---

## 2. Módulos existentes — qué hacen / qué no

| Módulo | Entidad principal | Proceso que soporta | Actor típico | Cubre REGUTRACK | No cubre |
|--------|-------------------|---------------------|--------------|-----------------|----------|
| Identity / RBAC | User, Role, Permission | AuthN/Z | Admin | Plataforma | Negocio RA |
| Tenant Admin | Tenant, Company, SSO… | Operación SaaS | Tenant Admin | Plataforma | Licencias MINSA |
| Documents | Document + approval | Control documental genérico | Document Controller | Repositorio posible | Checklist CT + fechas de sometimiento |
| Technical Sheets | Product + Sheet nutricional | Aprobación ficha (ingredients/nutrients) | Quality | Nombre “ficha” | Dispositivo médico / CT |
| Suppliers | Supplier + docs | Homologación proveedor | Supplier Manager | Fabricante parcial | Pipeline CT |
| Audit Management | ManagedAudit | Auditorías internas/programas | Auditor | Post-market QMS | Registro sanitario |
| CAPA | Capa | Acciones correctivas | CAPA Manager | Post-no-conformidad | Expediente RA |
| Risk | Risk | Riesgo organizacional | Risk Manager | ≠ clase A/B/C dispositivo | — |
| Indicators | QualityIndicator | KPIs | Indicators Mgr | Posible overlay | No hay KPI RA out-of-box |
| Reporting | ReportDefinition | Reportes | Reporting Mgr | Genérico | Vencimientos CT |
| Workflows | WorkflowInstance | Motor BPM genérico | Configurable | Motor usable | Definición proceso CT |
| Form Templates | FormTemplate | Diseñador de formularios | TEMPLATE.* | Checklist designer potencial | No orquesta caso |
| Enterprise Workspaces | EnterpriseWorkspaceItem | Tracker Plan→Close | Varios | UI `#/regulatory` | Solo título/código/due |
| Notifications / Storage / AuditLog | — | Transversales | Sistema | Sí | — |
| Training / Portals (UI) | Workspace | Stub tracker | — | No | No hay LMS/portal real |

Evidencia UI Regulatory (`app.js` `renderEnterpriseWorkspace`): campos `title`, `code`, `description`, `dueAtUtc` — **sin** CT, autoridad, checklist, fabricante, clase de riesgo.

---

## 3. Integraciones cross-module (realidad)

- Enlaces por **GUID opcionales** (CAPA←AuditId, Risk←CapaId, etc.), **sin FK EF** entre dominios.
- Workflow attach a CAPA/Risk/Indicator; Documents usan aprobación propia.
- **FormTemplates no son consumidos** por AuditChecklist, CAPA, Documents ni Technical Sheets.
- No hay entidad que una Producto ↔ CT ↔ Expediente ↔ Docs fabricante.

**Conclusión:** módulos **cooperan débilmente**; predominan **islas funcionales** con infra compartida.

---

## 4. Enfoque arquitectónico — respuestas forzadas

| Pregunta | Respuesta basada en código |
|----------|----------------------------|
| ¿Entidad raíz del sistema? | **Tenant + User** (plataforma) + múltiples aggregates QMS; **no** Hay `SanitaryRegistration` |
| ¿Es correcta para este negocio? | **No** para REGUTRACK; sí como plataforma horizontal |
| ¿Módulos independientes? | **Sí** (alto grado) |
| ¿Aggregate Root claro del negocio? | **No** |
| ¿Dominio bien definido (RA)? | **No** |
| ¿Proceso orquestador del registro? | **No** (Workflow genérico sin definición RA) |
| ¿Expediente Regulatorio? | **No** |
| ¿Case Management? | Solo `EnterpriseWorkspaceItem` genérico |
| ¿Motor Workflow? | **Sí** (`WorkflowEngineService`) |
| ¿Motor documental? | **Parcial** (Documents + Storage) |
| ¿Form Engine? | **Sí** (FormTemplates + Compliance Studio) |
| ¿Visión transversal del negocio REGUTRACK? | **No** |

---

## 5. Template Builder / Compliance Studio

| Pregunta | Evidencia |
|----------|-----------|
| ¿Es pieza central del sistema? | Ruta reciente + permisos `TEMPLATE.*`; **no** referenciado por otros BC |
| ¿Debe serlo para Multimed/4H? | **No**. Central debe ser **Expediente/Caso CT-RS** |
| ¿Process Designer? | Hoy diseña **formularios/layout/rules UI**, no processos BPM de sometimiento |
| ¿Qué debería generar? | Formularios + checklists versionados **consumidos por** RegistrationDossier |
| ¿Sustituye REGUTRACK? | **No** |

---

## 6. Fortalezas a preservar (evidencia)

1. Multitenancy + RBAC + AuditLog maduros  
2. Storage y notificaciones  
3. Workflow engine genérico  
4. Documents versionados con aprobación  
5. Form Engine extensible  
6. Separación Domain/Application clara  

Estas piezas **sí** sirven como cimientos de un dominio RA — no sobran.
