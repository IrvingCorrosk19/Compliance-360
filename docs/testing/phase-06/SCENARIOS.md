# Fase 6 — Escenarios end-to-end
## Certification v2.0 · DISEÑO · sin ejecución

Cada escenario agrupa múltiples TC. Estado: **Designed**. No ejecutar hasta Entry Gate.


## SCN-01 Login / Logout / Sesión
- TC-AUTH-0100..0105
- Logout limpia token; API posterior 401; re-login OK.

## SCN-02 Permisos y roles
- Asignación TAC (TC-TAC-0100)
- Recorrido menú por rol (SCN-ROL-*)
- 403 API SoD approve/import.

## SCN-03 Tenant & Multitenancy
- Operar solo datos del tenant del token.
- Manipular URL tenantId ajeno → 403/vacío (TC-NFR IDOR).

## SCN-04 Portafolio productos (REGUTRACK fila)
- Crear productos A/B/C; campos Excel; búsqueda; duplicado.
- UI Nuevo producto crea dossier automáticamente.

## SCN-05 Fabricantes & DOCUMENTACION
- Alta mfr; ISO/CLV/CE/FDA; RequestedOn; listado; alerta vencimiento.

## SCN-06 Expediente completo (happy path)
1. Bootstrap  
2. Producto + dossier  
3. Fechas fábrica/recepción  
4. Accept criticals  
5. Pipeline transitions → ReadyForSubmission  
6. Submit  
7. Observation + corrección + resubmit  
8. Approve CT/RS  
9. Ver registros + historial + dashboard  

Trace: SCN-06 ≡ reemplazo operativo de una fila REGUTRACK.

## SCN-07 Checklist & Gate
- 22 reqs; bloqueo submit; accept item-by-item; waiver DocumentsReceived.

## SCN-08 Workflow exhaustivo
- Todas allowed (TC-RA-0200+)
- Todas illegal (TC-WF-NEG-*)

## SCN-09 Pipeline / Tubería
- Chips por columna; navegación a detalle; estados visibles vs Excel tubería.

## SCN-10 Dashboard & Alertas
- KPIs mes/stuck/bottleneck/$ 
- evaluate thresholds 90..0

## SCN-11 Importador profesional
- JSON stage/commit
- XLSX real stage
- Commit mixtos product/cert/license
- Negativos empty/invalid/403

## SCN-12 Licencias OP
- Create Multimed/4H
- Renewal → catalog requirements
- Alertas expiry

## SCN-13 Renovación CT/RS
- POST renewals sobre producto aprobado

## SCN-14 Documentos
- Subir evidencia; verificar no huérfano ideal (P1)
- Link conceptual a requisito

## SCN-15 Notificaciones / Reportes
- Si canal alertas InApp solamente: documentar
- Report Center no sustituye dashboard RA

## SCN-16 Búsquedas / filtros
- products searchText; dossiers by status; registrations search

## SCN-17 Responsive / tema
- 1280 / 1920; mobile degrade; dark si existe

## SCN-18 Errores / validaciones
- Campos vacíos, fechas invertidas, chars especiales escaped

## SCN-19 Auditoría
- Approve/import generan audit REGULATORY

## SCN-20 API security pack
- 401 sin token en todos endpoints
- 403 cross-role

## SCN-21 Anti-legacy
- Workspace Regulatory no es sistema de registro
- Technical Sheets ≠ DM dossier
- DeviceRiskClass ≠ RiskManagement module

## SCN-22 Gaps de paridad (obligatorios)
- TC-GAP-5001 Documents hard-link
- TC-GAP-5002 Studio pack bridge
- TC-GAP-5003 Kanban Vencido/Renovación
- Resultado esperado: PASS del producto **o** FAIL documentado + WAIVER PO (no SKIPPED)

## SCN-23 Import integridad volumétrica
- Stage Excel real 0 errors
- Commit maxRows controlado + plan full-book
- Uniquify catalog (TC-RA-0908)
- Rollback comportamiento (TC-RA-0907)
