# 10 — Implementation Report

**Actualizado:** 2026-07-14  
**Contrato:** `REGUTRACK 02JUN26 MG.xlsx`  
**Matriz viva:** [`REGULATORY_COVERAGE_MATRIX.md`](./REGULATORY_COVERAGE_MATRIX.md)

## Construido (Case Management · Registration Dossier)

- BC Regulatory Affairs (Domain / Application / Infrastructure) + API `/api/v1/tenants/{id}/regulatory/*`
- Centro: **RegistrationDossier** (no Template Builder, no EnterpriseWorkspace)
- RA Console `#/regulatory`: Dashboard, Portafolio, Pipeline, Expedientes (historial tipado), CT/RS, Fabricantes, Licencias, Alertas, Importación XLSX, Config
- Pack REGUTRACK 22 requisitos + `LicenseOpRequirementCatalog` en renovaciones OP
- Importador ClosedXML: lee hojas REGISTROS / DOCUMENTACION / LICENCIAS OP → stage → validate → simulate → commit
- Alertas 90/60/30/15/7/1/0 (CT/RS, certificados, licencias, max recepción)
- Dashboard: vencen este mes, detenidos >14d, bottleneck, $ por estado
- Migraciones: `AddRegulatoryAffairs` + `AddRegulatoryAffairsParityFields`
- 17 permisos `REGULATORY.*` + roles RA; Tenant Administrator con grants

## Evidencia de esta iteración

| Prueba | Resultado |
|--------|-----------|
| `dotnet build` | PASS |
| Unit tests RegulatoryAffairs (5) | PASS |
| Stage XLSX real `REGUTRACK 02JUN26 MG.xlsx` | PASS — **715 filas**, 10 warnings, 0 errors |
| Commit import mixto (producto + cert + licencia) | PASS — campos Distributor / suppliers / line / RequestedOn |
| Historial tipado en create dossier | PASS — `DossierCreated` + `StatusTransition` persistidos |
| Dashboard campos nuevos | PASS — `registrationsExpiringThisMonth`, `dossiersStuckOver14Days`, `bottleneckStatus` |

## Cobertura vs Excel (ver matriz)

| Área | Estado |
|------|--------|
| Críticas CTT REGISTROS 1–55 + Línea + historial 56–87 (equivalente tipado) | Implementado / pocas Parciales (ficha/formulario archivo, Studio bridge) |
| DOCUMENTACION fabricante | Implementado (RequestedOn incluido) |
| LICENCIAS OP checklist | Implementado al iniciar renovación |
| Import XLSX | Implementado (rollback formal / reporte extendido aún backlog) |
| FADDI integración | No aplica (tarea manual) |

## Aún NO cierra el Excel (DoD pendiente)

1. E2E browser certificado (consola completa con operadores Multimed)
2. Commit masivo del Excel histórico + QA de duplicados / normalización país-fabricante
3. Rollback import + reporte post-carga formal
4. Studio = Requirement Pack Designer (bridge oficial)
5. Documents sin huérfanos (UI/API de linkage duro Producto↔Expediente↔Requisito)
6. Columnas kanban dedicadas Vencido / Renovación (hoy vía registros + StartRenewal)

## Producción

**NO GO** como retiro definitivo de REGUTRACK (faltan E2E + migración histórica certificada).

**GO** para piloto staging de Case Management regulatorio: operar expediente, checklist, CT/RS, fabricantes, licencias, alertas e importar el Excel real vía stage.
