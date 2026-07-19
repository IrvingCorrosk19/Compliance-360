# 03 — Functional Actor Model
## Sin inventar cargos — actores del dominio REGUTRACK + as-built

## 1. Actores humanos internos (tenant)

| Actor funcional | Qué hace en el negocio | ¿Rol sistema hoy? |
|-----------------|------------------------|-------------------|
| Operador de expediente / preparador | Arma caso, checklist, fechas, docs fábrica | ≈ Regulatory Specialist |
| Revisador documental/técnico | Acepta/rechaza requisitos, declara listo | **Parcial** — mezclado en Specialist vía REQUIREMENT.MANAGE |
| Autorizador interno de sometimiento | “Listo para enviar a MINSA/CSS” | **Ausente** |
| Registrador de sometimiento | Anota fecha/comprobante de envío | ≈ Specialist con SUBMIT (misma persona puede preparar+someter) |
| Registrador de decisión externa | Carga N° CT/RS, fechas emisión/vencimiento | ≈ Reviewer / QM / Admin vía APPROVE |
| Configurador RA | Packs, autoridades, import REGUTRACK | Regulatory Administrator / Tenant Admin |
| Consulta | Pipeline / KPI | Viewer / Regulatory Viewer |
| Gestor QMS | Aprobaciones transversales | Quality Manager (aprobar CT/RS sin preparar) |

## 2. Actores externos (NO son usuarios del tenant)

| Actor | Rol |
|-------|-----|
| MINSA / CSS / otra autoridad | Emite decisión / observación / CT/RS |
| Fabricante | Envía documental (vía Specialist; no login propio RA) |

No existe portal de autoridad externa. Toda “decisión MINSA” es **registro interno** de un hecho externo.

## 3. Actores sistémicos

| Actor | Acción |
|-------|--------|
| Importador REGUTRACK | Crea productos/dossiers/certs/licenses |
| Motor de alertas | Evalúa umbrales 90…0 |
| Identity/RBAC seed | Provisiona roles/permisos |

## 4. Distinción crítica

```text
APROBACIÓN INTERNA          ≠     DECISIÓN DE AUTORIDAD
(autorizar sometimiento)          (CT/RS / Observed / Rejected)
```

Hoy el código solo implementa con fuerza la segunda (`ApproveDossierAsync` → `SanitaryRegistration`).
