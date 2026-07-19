# 05 — Business vs System Gap Analysis

Matriz de cobertura proceso ↔ sistema.  
Estados: **Existe** | **Parcial** | **No existe**.  
Toda celda exige evidencia.

---

## Matriz de cobertura funcional

| Proceso del negocio (Excel) | Existe | Parcial | No existe | Evidencia |
|-----------------------------|:------:|:-------:|:---------:|-----------|
| P1 Portafolio productos + CT/RS | | | **X** | Excel cols CT/RS, clase, autoridad; Domain sin `SanitaryRegistration`; `Product` sin esos campos (`TechnicalSheetModels.cs`) |
| P2 Tubería de sometimiento (fechas) | | | **X** | Excel cols 40–50; no hay milestones API; `#/regulatory` solo dueAtUtc generico |
| P3 Checklist expediente (22 docs) | | **X** | | Documents/FormTemplates podrían almacenar archivos/defs; **no** hay caso que exija el set por tipo proceso/clase |
| P4 Solicitud docs a fábrica + alertas | | **X** | | `SupplierDocument` + `SupplierExpirationAlert` / Notifications existen; **no** enlazados a CT ni a fechas estimadas/máximas del Excel |
| P5 Documentación fabricante vigencia (ISO/CLV/CE) | | **X** | | `DOCUMENTACION` Excel; Suppliers/Documents parcialmente; sin catálogo obligatorio ISO13485/CLV/CE tipado al proceso PA |
| P6 Licencias operación corporativas | | | **X** | `CTT LICENCIAS OP`; ningún aggregate licencia MINSA/FADDI |
| Renovación CT/RS | | | **X** | Tipo proceso RENOVACION en Excel; no hay state machine de renovación |
| Observaciones autoridad + corrección | | | **X** | Col Fecha observación (1 dato); sin entidad Observation cycle |
| Vencimiento + renovación planificada | | **X** | | Due dates en workspace/supplier alerts; sin calendario unificado CT+licencias+docs fábrica |
| Oportunidad comercial por producto | | | **X** | Col Oportunidad; reporting genérico no modela este KPI |
| Autoridad MINSA/CSS | | | **X** | Valores embebidos en Excel; no master data Authority |
| Clase riesgo dispositivo A/B/C | | | **X** | Distinto de `RiskManagement` organizacional |
| Auditoría interna QMS | **X** | | | `AuditManagement` completo — **no** requerido por Excel núcleo |
| CAPA | **X** | | | Presente — post-market / no en Excel |
| Risk org | **X** | | | Presente — distinto concepto Excel |
| Control documental genérico | **X** | | | `Documents` |
| Multiusuario / permisos | **X** | | | RBAC |
| Workflow genérico | **X** | | | Engine sin definición RA |
| Form designer | **X** | | | FormTemplates no wired |

---

## Cobertura cuantitativa (procesos P1–P6)

| Métrica | Valor |
|---------|-------|
| Procesos núcleo Excel | 6 |
| Cubiertos (Existe) | 0 |
| Parciales | 3 (P3, P4, P5) |
| No existen | 3 (P1, P2, P6) + renovación/observación como subprocesos |
| **Cobertura proceso núcleo (estricto)** | **~0–15%** (solo piezas reutilizables laterales) |
| Cobertura plataforma/QMS genérica | Alta (fuera del proceso raíz) |

---

## Hallazgos de diseño vs negocio

1. **Root entity incorrecta para el cliente actual:** el valor sale del **CT/RS + expediente**, no del Tenant ni del FormTemplate.  
2. **Technical Sheets desalineadas:** ingredients/nutrients ≠ dispositivo médico.  
3. **Regulatory Management UI es isla tracker**, no Case Management.  
4. **Clase de riesgo del Excel ≠ módulo Risk.**  
5. **Workflow y Form Engine son infraestructura sin orquestación del proceso MINSA/CSS.**  
6. **Módulos QMS no son “basura”:** son **futuros** (post-market / ISO) pero **no sustituyen** REGUTRACK.

---

## Escenarios de decisión

| Opción | Cuándo aplica | Evidencia a favor/en contra |
|--------|---------------|-----------------------------|
| Reescribir todo | Solo si núcleo platform fallara | **En contra:** Identity/tenant/docs/workflow funcionan |
| Configurar solo Workspace/Forms | Si Excel cupiera en title/code/due | **En contra:** 50+ columnas estructurales |
| **Evolución: BC Regulatory Affairs** | Gap P1–P6 | **A favor:** reutiliza capas; modela expediente |
| Forzar negocio a CAPA/Audit | — | **En contra:** Excel no opera así |
