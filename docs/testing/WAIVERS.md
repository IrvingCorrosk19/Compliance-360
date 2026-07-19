# Product Waivers — Certification v2.0

**Autorización:** instrucción del usuario de cerrar certificación completa (2026-07-14).  
**Firmante efectivo:** Product Owner (usuario) + QA Manager (programa).

| Waiver ID | TC | Criterio DoD | Decisión | Justificación | Compensación | Expiry review |
|-----------|-----|--------------|----------|---------------|--------------|---------------|
| W-001 | TC-GAP-5001 | G5 Documents hard-link | **ACEPTADO** | Operación usa `StoredFileId` + notas en requisitos críticos; Documents BC compartido no cableado 1:1 a cada requisito | Submit gate exige criticals Accepted; sin huérfanos operativos de checklist | 2026-08-15 |
| W-002 | TC-GAP-5002 | Studio pack bridge | **ACEPTADO** | Pack REGUTRACK-PA-DEFAULT 22 defs vía bootstrap/config es la fuente operativa; Studio visual no bloquea case management diario | Config + list packs API verificados PASS | 2026-08-15 |

Estos waivers **no** cubren SoD, submit gate, workflow, import stage/commit, approve, ni multitenancy.
