# 05 — Role Interaction Map

## Propósito

Documentar cómo los roles se intersectan en procesos de negocio, qué datos fluyen entre ellos y dónde el modelo actual crea dependencias o fricciones.

---

## Mapa de interacción por proceso

### Ciclo documental

```
Document Controller ──elabora──► Documento (DOCUMENT.MANAGE)
        │                              │
        │                              ▼
        │                    Quality Manager ──aprueba──► (mismo permiso DOCUMENT.MANAGE)
        │                              │
        ▼                              ▼
   Storage Admin ◄──archivo──   Workflow (WORKFLOW.MANAGE)
        │
        ▼
   Viewer ◄──consulta── (sin DOCUMENT.READ en catálogo — gap)
```

| De | A | Artefacto | Permiso puente |
|----|---|-----------|----------------|
| Document Controller | Quality Manager | Documento en revisión | DOCUMENT.MANAGE compartido |
| Document Controller | Storage Admin | Archivo binario | STORAGE.MANAGE |
| Quality Manager | Reporting Manager | Reporte estado documental | REPORT.EXECUTE |
| Cualquier mutador | Viewer | Datos read-only | *.READ |

**Fricción:** no hay rol "Aprobador documental" separado del elaborador a nivel permiso.

---

### Ciclo auditoría → CAPA

```
Auditor ──crea auditoría──► Audit Management
        │
        └──hallazgo/NC──► CAPA Manager ◄──Supplier Manager (NC proveedor)
                              │
                              ▼
                    Quality Manager ──aprueba cierre──► CAPA.CLOSE
                              │
                              ▼
                    Risk Manager ◄──riesgo asociado──► CAPA
```

| De | A | Trigger | Roles con acceso API |
|----|---|---------|---------------------|
| Auditor | CAPA Manager | Hallazgo | AUDITMANAGEMENT.MANAGE + CAPA.MANAGE |
| Supplier Manager | CAPA Manager | Proveedor no conforme | SUPPLIER.MANAGE + CAPA.MANAGE (añadido E2E) |
| CAPA Manager | Quality Manager | Cierre CAPA | CAPA.CLOSE vs CAPA.MANAGE |
| CAPA Manager | Risk Manager | Riesgo emergente | Lectura cruzada |

---

### Ciclo indicadores y reportes

```
Indicators Manager ──define KPI──► Indicador
        │                              │
        │                              ▼
        └────────────────────► Reporting Manager ──ejecuta──► Reporte
                                          │
                                          ▼
                                    Viewer (consulta)
```

| De | A | Dependencia |
|----|---|-------------|
| Indicators Manager | Quality Manager | Aprobación indicador (`INDICATOR.APPROVE`) |
| Indicators Manager | Reporting Manager | Datasets dashboard (`REPORT.READ`) |
| Risk Manager | Reporting Manager | Heat map / export |

---

### Ciclo administración tenant

```
SuperAdmin ──provisiona──► Tenant + Tenant Admin user
        │
        ▼
Tenant Admin ──crea──► Roles operativos + Usuarios
        │
        ├──► Document Controller, QM, Auditor, ...
        │
        └──► Viewer (solo lectura)

SuperAdmin ──puede intervenir──► Cualquier tenant (bypass ApiContext)
```

| Relación | Tipo | Riesgo |
|----------|------|--------|
| SuperAdmin → Tenant Admin | Jerárquica plataforma | Correcto |
| SuperAdmin → datos operativos tenant | Acceso directo | **Alto** — no debería en enterprise |
| Tenant Admin → roles operativos | Delegación | Correcto |
| Tenant Admin → SuperAdmin | Ninguna | Correcto |

---

### Ciclo infraestructura

```
Storage Admin ──configura──► Storage Provider
        │
        ▼
Document Controller / Auditor ──suben evidencia──► /storage/files

Notification Admin ──configura──► SMTP Provider
        │
        ▼
Todos los roles ◄──notificaciones── (workflow, CAPA, etc.)
```

**Solapamiento:** Notification Admin tiene `STORAGE.MANAGE` — comparte pantalla Configuration con Storage Admin.

---

## Matriz de interacción rol × rol

|  | SuperAdmin | Tenant Admin | Doc Ctrl | QM | Auditor | Supplier | CAPA | Risk | Indicators | Reporting | Storage | Notif | Viewer |
|--|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| **SuperAdmin** | — | provisiona | puede operar | puede operar | puede operar | puede operar | puede operar | puede operar | puede operar | puede operar | puede operar | puede operar | puede operar |
| **Tenant Admin** | reporta | — | crea usuario | crea usuario | crea usuario | crea usuario | crea usuario | crea usuario | crea usuario | crea usuario | crea usuario | crea usuario | crea usuario |
| **Doc Ctrl** | — | — | — | entrega docs | — | — | — | — | — | — | usa storage | — | — |
| **QM** | — | — | recibe docs | — | recibe audit | — | supervisa | supervisa | supervisa | consume | — | — | — |
| **Auditor** | — | — | audita docs | escala NC | — | audita prov | origina CAPA | identifica riesgo | — | reporta | adjunta | — | — |
| **Supplier** | — | — | — | — | — | — | origina CAPA | — | — | — | — | — | — |
| **CAPA** | — | — | — | cierra | recibe | recibe | — | vincula | — | — | evidencia | — | — |
| **Risk** | — | — | — | alinea | input audit | — | vincula | — | correlaciona | reporta | — | — | — |
| **Indicators** | — | — | — | alinea | — | — | — | correlaciona | — | alimenta | — | — | — |
| **Reporting** | — | — | — | consume | consume | consume | consume | consume | consume | — | — | — | entrega |
| **Storage** | — | soporte | soporte | — | soporte | — | soporte | — | — | — | — | comparte UI | — |
| **Notif** | — | soporte | — | — | — | — | — | — | — | — | comparte UI | — | — |
| **Viewer** | — | — | consulta | consulta | consulta | — | consulta | consulta | consulta | consulta | — | — | — |

---

## Puntos de handoff críticos

| Handoff | Emisor | Receptor | Mecanismo actual | Gap |
|---------|--------|----------|------------------|-----|
| Documento listo para revisión | Document Controller | Quality Manager | Mismo permiso MANAGE | Sin estado workflow RBAC |
| Hallazgo auditoría | Auditor | CAPA Manager | Manual — crear CAPA | Sin permiso linkage |
| NC proveedor | Supplier Manager | CAPA Manager | Requiere CAPA.MANAGE en supplier | Permiso cruzado |
| Cierre CAPA | CAPA Manager | Quality Manager | CAPA.CLOSE vs CAPA.MANAGE | QM tiene ambos en E2E |
| KPI fuera de meta | Indicators Manager | Quality Manager | INDICATOR.APPROVE | OK en API |
| Usuario nuevo | Tenant Admin | Rol operativo | RBAC assign | OK |
| Tenant nuevo | SuperAdmin | Tenant Admin | Crear tenant + usuario | OK |

---

## Dependencias técnicas entre roles

| Dependencia | Descripción |
|-------------|-------------|
| Dashboard → múltiples módulos | Casi todos los roles necesitan permisos de lectura cruzada para evitar 403 en métricas (`app.js` L713–718) |
| JWT permissions → menú | `canNavigate` filtra sidebar; quick-switcher **no** filtra |
| SuperAdmin bypass | Rompe modelo de interacción — puede sustituir cualquier rol |
| Catálogo permisos global | Tenant Admin otorga solo permisos existentes en `permissions` table |
| `Tenant.Manage` policy | Cualquier permiso TENANT.* abre enterprise-workspaces |

---

## Flujo de datos e información sensible

| Tipo dato | Roles que crean | Roles que modifican | Roles que solo leen | Roles excluidos |
|-----------|-----------------|-------------------|---------------------|-----------------|
| PII usuarios | Tenant Admin | Tenant Admin | Tenant Admin, Auditor (audit) | Viewer, operativos |
| Documentos calidad | Doc Ctrl, QM | Doc Ctrl, QM | Viewer (si READ existiera) | — |
| Expediente proveedor | Supplier Mgr | Supplier Mgr | Auditor, Viewer | — |
| CAPA / NC | CAPA, Auditor, QM | CAPA Mgr | Viewer | — |
| Config SSO/API keys | Tenant Admin | Tenant Admin | — | Todos operativos |
| Auditoría global | SuperAdmin | — | SuperAdmin | Tenant roles |

---

## Conclusión del mapa

El sistema comporta una **red densa de solapamientos** centrada en Quality Manager y en permisos transversales para dashboard. SuperAdmin puede acortar cualquier cadena de handoff accediendo directamente al data plane. La interacción **ideal** (roles especializados con handoffs explícitos) no coincide con la **implementación** (permisos `*.MANAGE` amplios y grants ad hoc en E2E).
