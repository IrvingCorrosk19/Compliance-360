# 15 — Certificación funcional QM

| ID | Control ejecutado | Resultado real | Evidencia | Estado |
|---|---|---|---|---|
| QM-01 | Validar navegación | Audit, RA y QMS visibles, incluidos Documents, Technical Sheets, CAPA, Risks e Indicators | `manual-vs-system-role-matrix.json`; `cert-role-qm.png` | PASS |
| QM-02 | Navegar y aprobar documento | UI y endpoint de decisión operativos | TAC/QM 2/2; captura QM | PASS |
| QM-03 | Resolver servicios QMS | Servicios y repositorios registrados | build sin errores; .NET 252/252 | PASS |
| QM-04 | Intentar acciones RA incompatibles | Bootstrap, producto, dossier, transición, aprobación interna y sometimiento: HTTP 403 | matriz de roles, `DENY-*` | PASS |
| QM-05 | Preservar persistencia QMS | Modelo compila y las tablas existentes se conservan | build sin errores; .NET 252/252 | PASS |

Actor certificado: `ra.qm@cert.local`. Resultado del rol: **PASS**.
