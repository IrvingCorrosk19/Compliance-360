# 14 — Certificación funcional RA-VIEW

| ID | Control ejecutado | Resultado real | Evidencia | Estado |
|---|---|---|---|---|
| VIEW-01 | Navegar módulos permitidos | Seis módulos accesibles | `journey-ra-view.json`, `BROWSE-ALL`; captura asociada | PASS |
| VIEW-02 | Abrir registro | 0 botones de mutación; aviso de solo lectura | mismo JSON, `OPEN-RECORD` | PASS |
| VIEW-03 | Invocar APIs mutantes | Todas HTTP 403 | mismo JSON, `NEG-API-*` | PASS |
| VIEW-04 | Acceder a URLs no asignadas | Denegación renderizada | mismo JSON; capturas `ra-view-neg-url-*.png` | PASS |
| VIEW-05 | Leer y mutar dossier SoD | GET 200; seis mutaciones HTTP 403 | `browser-sod-steps.json`, `VIEW-*` | PASS |

Actor certificado: `ra.view@cert.local`. Resultado del rol: **PASS**.
