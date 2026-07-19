# 12 — Production Certification Framework
## v2.0 — No sustituye certificación de lab

Este documento define el **marco** para promover una build ya certificada en lab/staging hacia producción.  
**No autoriza** declarar GO producción durante Fase 1–10 de lab.

---

## 1. Pre-condiciones producción

| # | Condición |
|---|-----------|
| P1 | Master Report lab/staging con GO STAGING o GO RETIRE EXCEL |
| P2 | Cero P0; P1 waivers revalidados en staging |
| P3 | Secrets/JWT/DB gestionados fuera de repo |
| P4 | Backup + RPO/RTO definidos |
| P5 | RBAC productivo sembrado (no usuarios `@cert.local`) |
| P6 | Import histórico ejecutado en ventana controlada |
| P7 | Monitoreo health + alertas ops |

---

## 2. Controles tipo banca / ISO 13485

- Change control ticket por release  
- Traceability build ↔ Master Report version  
- Segregation: quien certifica ≠ quien despliega (ideal)  
- Audit trail retentions  

---

## 3. Evidence pack producción

- Release notes  
- Master Report adjunto  
- Resultados smoke prod post-deploy (subset W1/W5/W9)  
- Contacto rollback  

---

## 4. Plantilla BUG

```markdown
# BUG_xxx — título
| Severity | P0/P1/P2/P3 |
| Status | Open/Fixed/Retest/Closed |
| Found in | TC-ID / Wave |
## Descripción
## Impacto (negocio REGUTRACK)
## Pasos
## Evidencia (paths)
## Logs
## Causa raíz
## Corrección
## Retest (TC + resultado)
```

---

## 5. Relación con FDA evidence mindset

No implica clearances FDA del dispositivo.  
Implica: evidencia attributable, reproducible, independiente del “me parece”, adecuada para auditoría de QMS software.
