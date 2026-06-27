# MEMORY_OPTIMIZATION_REPORT

## Baseline observado

| Métrica | Resultado |
| --- | ---: |
| Working set local | 133.59 MB |
| Private memory local | 116.94 MB |

## Hallazgos

- No se detectaron llamadas bloqueantes obvias.
- Hay uso de `MemoryStream` y `ToArray()` en storage provider local para cálculo de hash y persistencia.
- Dashboards Risk/CAPA/Indicators materializaban listas para cálculos agregados.

## Optimizaciones aplicadas

- CAPA dashboard reemplazó carga completa de effectiveness checks por conteos server-side.
- Indicator dashboard reemplazó carga completa de results por conteos server-side.
- Risk dashboard reemplazó carga completa de risks por agregaciones server-side.
- `AsSplitQuery()` reduce explosión de filas duplicadas en agregados con colecciones.

## Pendientes

- Revisar streaming de storage para archivos grandes.
- Medir asignaciones con profiler/dotnet-counters bajo carga.
- Evaluar pooling/buffers únicamente en hotspots confirmados.
