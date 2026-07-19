# 07 — Certificación funcional TAC

| ID | Control ejecutado | Resultado real | Evidencia | Estado |
|---|---|---|---|---|
| TAC-01 | Validar navegación administrativa | `dashboard,audit-trail,regulatory,tenant-administration,security` | `manual-vs-system-role-matrix.json`; `cert-role-tac.png` | PASS |
| TAC-02 | Ejecutar Bootstrap | HTTP 200 | matriz de roles, `API-BOOTSTRAP` | PASS |
| TAC-03 | Intentar operaciones RA | Crear, transicionar, aprobar, someter y decidir externamente: HTTP 403 | matriz de roles, `DENY-*` | PASS |
| TAC-04 | Editar/desactivar usuario | Operación disponible; `UserStatus.Disabled=3` | TAC/QM 2/2; .NET 252/252 | PASS |

Actor certificado: `irvingcorrosk19@gmail.com`. Resultado del rol: **PASS**.
