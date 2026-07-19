# INPUT_DICTIONARY_BY_SCREEN

## Document Management

| Input | Qué significa | Ejemplo | Regla funcional |
|---|---|---|---|
| Code | Identificador único del documento | BPM-LIM-001 | No repetir códigos; seguir formato corporativo. |
| Title | Nombre claro del documento | Limpieza de línea de empaque | Debe ser entendible para operación y auditoría. |
| Category | Clasificación funcional | POE / Instructivo | Impacta reportes y aprobación. |
| Owner | Responsable del contenido | Document Controller | Debe tener permiso de mantenimiento. |

## CAPA

| Input | Qué significa | Ejemplo | Regla funcional |
|---|---|---|---|
| Issue | Problema detectado | Desviación en temperatura CCP | Vincular evidencia o hallazgo. |
| Root Cause | Causa raíz | Falla en calibración de sensor | Basada en análisis estructurado. |
| Action Owner | Responsable de ejecutar | Jefe de Producción | Con fecha y seguimiento. |
| Due Date | Fecha objetivo | 2026-08-15 | Debe cumplir SLA interno. |

## Risks

| Input | Qué significa | Ejemplo | Regla funcional |
|---|---|---|---|
| Risk Name | Nombre del riesgo | Contaminación cruzada | Debe describir evento real. |
| Probability | Probabilidad de ocurrencia | Media | Consistente con matriz corporativa. |
| Impact | Impacto potencial | Alto | Alineado a criterios de severidad. |
| Mitigation Plan | Plan de tratamiento | Barreras sanitarias + verificación diaria | Debe ser accionable y medible. |

## Audits

| Input | Qué significa | Ejemplo | Regla funcional |
|---|---|---|---|
| Audit Code | Código único de auditoría | AUD-INT-2026-01 | No duplicar y usar nomenclatura estándar. |
| Scope | Alcance auditado | Línea de empaque 1 | Delimitar procesos y áreas incluidas. |
| Auditor | Responsable de ejecución | Auditor Interno | Mantener independencia funcional. |
| Finding | Hallazgo detectado | Registro incompleto de limpieza | Debe incluir evidencia objetiva. |

## Reports

| Input | Qué significa | Ejemplo | Regla funcional |
|---|---|---|---|
| Report Type | Tipo de reporte | CAPA abiertas | Debe responder a una decisión de negocio. |
| Date Range | Periodo de análisis | Últimos 30 días | Definir desde/hasta consistente. |
| Audience | Audiencia destinataria | Gerencia de Calidad | Ajustar nivel de detalle y lenguaje. |
| Export Format | Formato de salida | PDF | Elegir formato según uso final. |

## Tenant Administration

| Input | Qué significa | Ejemplo | Regla funcional |
|---|---|---|---|
| User Email | Correo corporativo del usuario | quality@empresa.com | Único dentro del tenant. |
| Role Assignment | Rol asignado | Quality Manager | Respetar segregación de funciones. |
| Status | Estado de acceso | Activo | Desactivar acceso cuando no corresponda. |
| Permission Scope | Alcance de permisos | Read/Approve | Aplicar principio de mínimo privilegio. |

## Indicators

| Input | Qué significa | Ejemplo | Regla funcional |
|---|---|---|---|
| Indicator Code | Código del KPI | KPI-NC-RATE | Único por indicador. |
| Target | Meta esperada | <= 1.0% | Debe ser medible y realista. |
| Threshold | Umbral de alerta | 1.2% | Define disparo de alertas. |
| Frequency | Periodicidad de medición | Semanal | Consistencia para comparabilidad. |

