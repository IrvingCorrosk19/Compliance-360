# 08 — Certificación funcional RA-ADM

| ID | Control ejecutado | Resultado real | Evidencia | Estado |
|---|---|---|---|---|
| ADM-01 | Validar accesos | Portfolio, fabricantes, licencias, alertas, importación, configuración y SoD visibles; áreas operativas restringidas | `manual-vs-system-role-matrix.json`; `cert-role-ra-adm.png` | PASS |
| ADM-02 | Crear producto y expediente | Modal operativo; expediente en `Planning` | matriz de roles, `CREATE-PRODUCT-DOSSIER` | PASS |
| ADM-03 | Crear licencia | `Cert Distribution 983332` persistida | `manual-workflow-steps.json`, `ADM-LICENSE-MODAL` | PASS |
| ADM-04 | Guardar alert settings | Persistencia y lectura verificadas | suite `ra-admin` 10/10 | PASS |
| ADM-05 | Intentar acciones incompatibles | Transición, revisión, aprobación y sometimiento: HTTP 403 | matriz de roles, `DENY-*` | PASS |

Actor certificado: `ra.admin@cert.local`. Resultado del rol: **PASS**.
