# Fase 7 — Pruebas negativas / abuso
## Certification v2.0 · DISEÑO · sin ejecución

Inventario detallado también en `TC_INDEX.csv` (Type=Negative). Resumen operativo:


## 1. Entrada de datos
- Brand/name/catalog vacíos
- Catalog duplicado
- Opportunity no numérica
- ExpiresOn < IssuedOn (cert/CT)
- Observation description vacía
- Registration number vacío en approve

## 2. Caracteres especiales / injection
- XSS en comments/observation → escaped en UI (`esc()`)
- SQL meta en searchText → no error 500 / no data leak
- Path traversal en filename import

## 3. AuthZ / AuthN
- Sin Bearer → 401
- Token tenant A + URL tenant B → deny
- Specialist approve → 403
- Specialist/Viewer import → 403
- Reviewer create product → 403 (**BUG_003 / TC-RBAC-0310**)
- Reviewer import → 403
- Waiver DocumentsReceived < 8 chars → 400 (TC-RA-0205)
- Hash route `#/regulatory` sin permiso → oculto

## 4. Workflow abuse
- Todas ilegal transitions (TC-WF-NEG-*)
- Double submit
- Approve desde Planning
- Submit con críticos pending

## 5. UX / sesión
- F5 mid-transition
- Back botón browser
- Doble clic botones
- Multi-tab same user
- Session expiry mid-form

## 6. Import abuse
- Empty file
- Non-xlsx bytes
- Commit job not Simulated
- Commit twice
- Rows without regulatoryName

## 7. Concurrencia
- Dos usuarios editan mismo requirement (last-write / error claro)

## 8. Documentos inválidos
- Tipo no permitido (si valida storage)
- 0-byte upload

**Ejecución:** stop-on-fail P0 igual que positivos.
