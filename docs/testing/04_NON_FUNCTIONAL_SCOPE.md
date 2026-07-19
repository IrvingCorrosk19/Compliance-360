# 04 — Non-Functional Scope
## v2.0 DISEÑO — sin ejecución

## 1. Seguridad y control de acceso

| ID | Requisito | Método |
|----|-----------|--------|
| NFR-SEC-01 | API sin Bearer → 401/403 | API |
| NFR-SEC-02 | Reviewer no crea producto | API+UI |
| NFR-SEC-03 | Specialist no aprueba / no importa | API+UI |
| NFR-SEC-04 | Viewer solo lectura | API+UI |
| NFR-SEC-05 | Cross-tenant IDOR denegado | API (2 tenants) |
| NFR-SEC-06 | Manipulación URL `#/regulatory` sin permiso → sin datos sensibles | UI |
| NFR-SEC-07 | Registration.Manage no abre Product.Manage (regresión BUG_003) | API |

## 2. Integridad de datos

| ID | Requisito |
|----|-----------|
| NFR-DI-01 | CatalogCode único por tenant; import no rompe con duplicados Excel |
| NFR-DI-02 | Country codes normalizados ≤8 |
| NFR-DI-03 | Commit no deja job en estado inconsistente sin mensaje |
| NFR-DI-04 | Audit / history en transiciones dossier |

## 3. Usabilidad / UX

| ID | Requisito |
|----|-----------|
| NFR-UX-01 | Consola RA identificable (no Workspace legacy como dueño) |
| NFR-UX-02 | Mensajes de error legibles (submit gate, waiver, 403) |
| NFR-UX-03 | Doble clic submit no crea doble CT |
| NFR-UX-04 | F5 / Back mantienen coherencia de sesión o re-login seguro |
| NFR-UX-05 | Prompts actuales documentados como riesgo UX (P2) — deben tener TC |

## 4. Responsive / accesibilidad básica

| ID | Requisito |
|----|-----------|
| NFR-RWD-01 | 1366×768 y 390×844: nav RA usable |
| NFR-RWD-02 | Tablas / kanban scrolleables sin pérdida de acciones críticas |

## 5. Performance (lab)

| ID | Requisito | Umbral lab |
|----|-----------|------------|
| NFR-PERF-01 | `GET /dashboard` | p95 < 3s lab |
| NFR-PERF-02 | Stage XLSX ~715 filas | Completa < 60s lab |
| NFR-PERF-03 | Commit batch (documentar N filas) | Completa sin 500; tiempo registrado |
| NFR-PERF-04 | Rate limit no bloquea certificación en Development | Dev permit↑ o pacing |

## 6. Confiabilidad

| ID | Requisito |
|----|-----------|
| NFR-REL-01 | `/health/live` 200 durante ventana de ejecución |
| NFR-REL-02 | Reinicio app no corrompe import jobs Simulated |

## 7. Fuera de NFR esta ronda

- Chaos / DR multi-AZ  
- Penetration test profesional tercero  
- Lighthouse score contractual  

Quedan en marco `12_PRODUCTION_CERTIFICATION.md`.
