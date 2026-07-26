# 01 — Executive Summary
**Run:** EFC-20260726-223328  
**Date (UTC):** 2026-07-26T22:36:22.382024+00:00  
**Lab:** http://localhost:5272 · Tenant 82af3877-2786-4d39-bce8-c981101c771d  
**Primary dossier:** RA-20260726-3308 / d61c1305-e6d1-43c6-9b28-e854762510cb → terminal **Closed**

## Direct answers

1. **¿Funciona realmente el sistema?** Sí — autenticación, RA Console, APIs v1/v2, Worker y persistencia PostgreSQL operaron en lab real.
2. **¿Puede completarse un expediente RA E2E?** **Sí.** Flujo real comprobado: Producto → Dossier (22 requisitos) → evidencia PDF → Technical Review V2 → Approve-for-submission → Submit (+proof) → Authority review → Observation → Resubmit → Approve (+resolution file) → **Closed**.
3. **¿Los roles funcionan?** Sí (login + JWT claims + denegaciones API) para Specialist/Reviewer/Approver/Submitter/Viewer/Manager/Admin/QM/Platform/TAC.
4. **¿SoD funciona realmente?** **Sí, backend-enforced** (no solo UI). Self-accept, approve y submit indebidos → 403. Playwright 
egulatory-sod-roles.spec.ts **PASS**.
5. **¿Documentos con trazabilidad suficiente?** Upload/download PDF, rechazo exe/txt, attach a requisitos, proof de submit y resolution file. DLM avanzado (retention/holds) no certificado como premium.
6. **¿Alertas empresariales?** RA alert-settings/evaluate OK; Alert Center inbox OK (vacío). Motor: **configurable + rule/evaluate**, no event-driven premium completo. Notificaciones cloud: config externa requerida.
7. **¿Configuración suficiente?** SoD settings, alert settings, bootstrap packs, authorities. No es un BPM totalmente configurable sin código.
8. **¿Auditoría reconstruye acciones?** Sí: udit_logs + dossier_history_events + timeline V2 + denegaciones SoD auditadas.
9. **¿Reportes confiables?** Dashboard RA responde con métricas reales. Report Center / exports: **no certificados** (403/404) → gap.
10. **¿Aislamiento tenant?** Intentos cross-tenant (UUID fake + platform) → **403**. Solo 2 tenants en lab (platform + business).
11. **P0/P1/P2:** P0=0 · P1=0 abiertos en ejecución final · P2 abiertos: empty-product 500, reporting access, worker startup coupling, notifications externas.
12. **Cobertura probada:** ~85% del mapa RA crítico (auth, RBAC/SoD, products, dossiers, docs, workflow V2, alerts, audit, MT deny, perf). Gaps: exports PDF/Excel, responsive profundo, segundo tenant business, email SMTP real.
13. **BLOCKED:** Report list TAC (403) en harness inicial.
14. **Externos:** SMTP localhost:1025 (Mailpit no levantado); SendGrid/Mailgun/Resend secrets ausentes → *FUNCTIONALLY READY — EXTERNAL CONFIGURATION REQUIRED*.
15. **ENTERPRISE FUNCTIONAL SCORE: 81/100**
16. **¿Listo para producción?** **CONDITIONALLY READY** — fuerte en workflow/RBAC/SoD/auditoría; condicionar GO a reporting/notificaciones/ops worker y hardening validaciones.

## Verdict
🟠 **CONDITIONALLY READY** · Level **Enterprise Candidate** · Score **81/100**
