# Phase 02 — System Analysis (As-Built)
## Certification Program v2.0 · DISEÑO · sin ejecución

**Fuentes:** `FoundationEndpoints.MapRegulatoryAffairs`, `regulatory-affairs.js`, Domain RA, `REGULATORY_COVERAGE_MATRIX.md`, `10_IMPLEMENTATION_REPORT.md`.

---

## 1. Arquitectura relevante

| Capa | Componentes |
|------|-------------|
| UI | `#/regulatory` → `wwwroot/regulatory-affairs.js` (10 vistas) |
| API | `/api/v1/tenants/{tenantId}/regulatory/*` |
| App | `RegulatoryAffairsService` |
| Domain | Dossier workflow, products, mfr, licenses, import jobs, alerts |
| Infra | EF Core + ClosedXML parser |
| Identity | `RoleCatalog` + `PermissionPolicies` (+ product create con código permiso directo) |

**Regla dura:** expediente = `RegistrationDossier`, **no** `EnterpriseWorkspaceItem`.

---

## 2. Inventario API (31 rutas)

Ver también `03_FUNCTIONAL_SCOPE.md`. Destacados de control:

| Control | Detalle |
|---------|---------|
| Create product | `RequireAuthorization(PermissionCatalog.RegulatoryProductManage)` — evita SoD Reviewer vía `Regulatory.Manage` OR |
| Submit | `Regulatory.Submit` |
| Approve | `Regulatory.Approve` |
| Import * | `Regulatory.Configure` |
| Commit | `POST .../commit?maxRows=` opcional |

---

## 3. Inventario UI

| View | Riesgo de prueba |
|------|------------------|
| dashboard | KPIs vs API |
| portfolio | prompts Alta producto |
| pipeline | 10 columnas — faltan Vencido/Renovación / varios estados |
| dossiers | transición, submit, obs, approve, accept req |
| registrations | read |
| manufacturers | alta + cert |
| licenses | alta + renew |
| alerts | evaluate |
| import | file + JSON + commit |
| config | bootstrap |

---

## 4. Workflow (16 estados)

Allowed set en dominio `RegistrationDossier.AllowedTransitions` — toda combinación fuera = TC-WF-NEG.

Gates: critical requirements; waiver DocumentsReceived ≥8; approve estado; obs abiertas.

---

## 5. Import pipeline

Enum: Uploaded → Mapped → Validated → Simulated → Committed | Failed | RolledBack  

Runtime feliz: parse → validate → simulate (0 errors) → commit.  
`Mapped`/`RolledBack` poco/no ejercidos → gap TC.

Parser sheets: REGISTROS/(2)/TUBERIA, DOCUMENTACION, LICENCIAS OP.  
Normalización country; uniquify catalog en commit.

---

## 6. Alertas y dashboard

Thresholds default: `90,60,30,15,7,1,0`.  
Tipos: registration/cert/license expiring|expired; max reception overdue.  
KPIs: productsTotal, registrations*, dossiers*, stuck>14d, bottleneck, opportunity, breakdowns.

---

## 7. Gaps de producto (obligan TC)

| Gap | Sev diseño |
|-----|------------|
| Documents hard link | P1 |
| Studio pack bridge | P1/P2 |
| Import rollback formal | P1 |
| Kanban Vencido/Renovación | P2 |
| CompanyMetadata | P2 |
| Full-book commit sin maxRows / perf | P1 |
| UI prompts | P2 |
| Implementation report NO GO Excel retirement | Contexto GO |

---

## 8. Dependencias de certificación

- TAC `GET /users` (roles + users) para seed  
- Auth login multi-step  
- Health live  

---

## 9. Conclusión Fase 2

El sistema **existe y es certificable** como Case Management RA.  
La paridad total REGUTRACK para retiro Excel **aún tiene gaps P1 documentados** — el diseño de TC debe forzar evidencia PASS/FAIL/WAIVER, no omitirlos.
