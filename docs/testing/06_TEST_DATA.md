# 06 — Test Data
## v2.0 DISEÑO

## 1. Contrato maestro

| Artefacto | Uso |
|-----------|-----|
| `REGUTRACK 02JUN26 MG.xlsx` | Stage XLSX obligatorío; sheets REGISTROS/(2)/TUBERIA, DOCUMENTACION, LICENCIAS OP |
| Pack `REGUTRACK-PA-DEFAULT` | 22 requisitos críticos/no críticos |
| Authorities | MINSA, CSS (bootstrap) |

Registrar en evidencia: nombre, bytes, last-write, hash opcional.

## 2. Datasets sintéticos (controlados)

| Dataset ID | Contenido | Uso |
|------------|-----------|-----|
| DS-PROD-A | Producto clase A, Multimed, catalog único | Happy path |
| DS-PROD-B | Clase B/C, manufacturer CN | Fabricante+cert |
| DS-DOS-HAPPY | Checklist críticos Accepted + waiver documents | Submit/approve |
| DS-DOS-OBS | Post-submit observation cycle | Obs→Resubmit |
| DS-DOS-NEG | Planning para transiciones ilegales | TC-WF-NEG |
| DS-IMP-JSON | 3 filas: product+cert+license | Commit smoke |
| DS-IMP-XLSX | Archivo completo | Stage; commit volumétrico planificado |
| DS-LIC | Licencia OP expira +15d | Alertas + renew |
| DS-ALERT | Cert/reg/license en cada threshold 90…0 | Data-driven |

## 3. Reglas de datos

1. `catalogCode` siempre único (`CERT-…` + guid corto).  
2. Waiver DocumentsReceived ≥ 8 caracteres (`CERT waiver evidence N/A`).  
3. Password usuarios prueba ≥12 + complejidad.  
4. No usar emails de clientes reales.  
5. Tras commit XLSX: documentar `importedRowCount`, `maxRows` si aplica, fallos de fila.

## 4. Limpieza

- Preferir datos prefijo `CERT-` para identificación.  
- No `TRUNCATE` ciego en medio de corrida certificada.  
- Snapshot DB opcional antes de commit full-book.

## 5. Trazabilidad Excel → campos

Campos mínimos obligatorios en DS-PROD-* (matriz A.1–A.55 selectos):

País, Categoría, Marca, Nombre CT, Catálogo, Fabricante/País, Distribuidor, Clase riesgo, Proveedores registrados, Ficha/Form ref, Oportunidad, Línea.
