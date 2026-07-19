# 08 — Risk-Based Testing
## v2.0 DISEÑO

## 1. Metodología

Riesgo = **Impacto de negocio (reemplazo Excel)** × **Probabilidad de fallo / detección tardía**.

Prioridades P0–P3 gobiernan orden de Fase 10 y stop conditions.

---

## 2. Riesgos principales (as-built)

| ID | Riesgo | Impacto | Prob. | Pri | Controles de prueba |
|----|--------|---------|-------|-----|---------------------|
| R-01 | Specialist puede aprobar CT | Integridad SoD / ISO Sod | Media (prev BUG_003 pattern) | **P0** | TC-RA-0501, SOD scripts |
| R-02 | Reviewer crea productos / importa | SoD | Media | **P0** | SOD-REV-*, NFR-SEC-07 |
| R-03 | Submit sin críticos Accepted | Calidad dossier / auditor | Alta si no gated | **P0** | TC-RA-0302 |
| R-04 | Transición ilegal muta estado | Integridad workflow | Media | **P0** | TC-WF-NEG batch |
| R-05 | Approve desde Observed/obs abierta | Estado inconsistente | Media | **P0** | TC obs+approve |
| R-06 | Cross-tenant leak | Banca/legal | Baja-Media | **P0** | TC-SEC-IDOR |
| R-07 | Import commit corrompe/duplica catálogo | Migración histórica | Alta volumétrica | **P0/P1** | TC-RA-0903 + full |
| R-08 | Country/código inválido aborta commit | Migración | Alta (histórico) | **P1** | Normalize + row skip |
| R-09 | Dashboard miente KPIs | Decisión gerencial | Media | **P1** | TC-RA-0603 data-driven |
| R-10 | Alertas no disparan thresholds | Cumplimiento vencimientos | Media | **P1** | TC-RA-0604 |
| R-11 | Excel columnas Parcial (ficha/docs) | Paridad REGUTRACK | Alta gap | **P1** | Matriz + TC gap |
| R-12 | UI prompt-only UX errors | Adopción | Alta | **P2** | TC-UX |
| R-13 | Kanban sin Vencido/Renovación | Operación | Media | **P2** | TC pipeline |
| R-14 | Studio bridge ausente | Config packs | Media | **P2** | TC config |
| R-15 | Rollback import inexistente | Controles | Media | **P1** | TC import neg |
| R-16 | Rate limit corta certificación | Falsa evidencia | Media lab | **P2** | Env Q |
| R-17 | DeviceRiskClass confundido con Risk Mgmt | Semántica | Baja | **P1** | TC glossary UI |
| R-18 | Workspace legacy usado como dossier | Arquitectura | Baja tras RA console | **P0** | TC-LEGACY-0100 |

---

## 3. Orden de ataque Fase 10 (post-Gate)

1. Auth + Tenant seed users  
2. Bootstrap + pack 22  
3. SoD negaciones por rol  
4. Lifecycle dossier P0  
5. WF ilegal + gates  
6. Manufacturers / licenses / alerts  
7. Import stage Excel real  
8. Import commit (volumetría planificada)  
9. Dashboard  
10. Gaps Parcial/Pendiente (esperados FAIL o WAIVER)  
11. NFR UX/responsive  
12. Playwright regresión (Fase 9 puede intercalarse tras bloque P0 manual PASS)

---

## 4. Stop / Go condicional

- **STOP:** cualquier P0 FAIL.  
- **GO STAGING:** X1–X6 en `10` + cero P0 abiertos.  
- **GO RETIRE EXCEL:** G1–G12 + P1 cerrados o WAIVER directorio.
