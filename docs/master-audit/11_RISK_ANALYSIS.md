# 11 — Risk Analysis

Riesgos de **no** alinear el sistema al proceso REGUTRACK, y riesgos de una evolución mal hecha.

---

## 1. Riesgos de negocio (si se continúa con la arquitectura actual como “solución”)

| ID | Riesgo | Severidad | Evidencia | Impacto |
|----|--------|-----------|-----------|---------|
| B1 | Pérdida de control de vencimientos CT/RS | **Critical** | 173 filas con fecha vencimiento en Excel; sin entidad CT en C360 | Producto no vendible / sanción |
| B2 | Expedientes incompletos al someter | **Critical** | Checklist 22 docs; sin enforcement en sistema | Observaciones, rechazo, demora |
| B3 | Licencias corporativas vencidas | **High** | `CTT LICENCIAS OP` con expiraciones 2025–2029; sin módulo | Bloqueo operaciones Multimed/4H |
| B4 | Certificados fabricante vencidos (ISO/CLV/CE) | **High** | DOCUMENTACION ACTIVO + vencimientos | Invalida expediente / auditorías |
| B5 | Ceguera de pipeline (pocos sometidos vs muchos planificados) | **High** | somet=11, aprob=6 vs armado=173 | Forecast comercial erróneo |
| B6 | Falsa sensación de cobertura (“tenemos Regulatory + Studio”) | **High** | `#/regulatory` = workspace; Studio sin consumers | Decisiones de inversión incorrectas |
| B7 | Oportunidad \$ no gestionada | **Medium** | Col Oportunidad con montos grandes | Priorización RA vs Sales ciega |

---

## 2. Riesgos técnicos / arquitectónicos

| ID | Riesgo | Severidad | Evidencia | Mitigación |
|----|--------|-----------|-----------|------------|
| T1 | Soft-GUID spaghetti al forzar CT sobre CAPA/Risk | High | Links GUID sin FK | Nuevo BC limpio |
| T2 | Overload de TechnicalSheet nutricional | Medium | Ingredients/Nutrients | BC dispositivo o extensión tipada |
| T3 | FormTemplates huérfanos generan deuda UX | Medium | No consumers | Ligar a RequirementPack |
| T4 | Ausencia query filter global tenant en EF | Medium | Auditoría código previa | Mantener disciplina repo + tests |
| T5 | Big-bang rewrite | Critical | Costo / regresión IAM | Evolución incremental roadmap 10 |

---

## 3. Riesgos regulatorios / compliance officer

| ID | Riesgo | Severidad | Notas |
|----|--------|-----------|-------|
| R1 | No demostrar historial de actualizaciones del expediente | High | Excel cols 56–87 actúan como bitácora informal; AuditLog genérico no tipado a CT |
| R2 | No rastrear observaciones de autoridad | High | Col observación casi vacía pero proceso existe |
| R3 | Confundir evidencia QMS interna con evidencia de registro sanitario | Medium | Módulos Audit/CAPA no equivalen a CT |

---

## 4. Riesgos de producto / UX

| ID | Riesgo | Severidad |
|----|--------|-----------|
| U1 | Usuario RA abandona C360 y vuelve a Excel | Critical |
| U2 | Doble captura Excel + C360 | High |
| U3 | Studio percibido como “el producto” distrae del case mgmt | Medium |

---

## 5. Matriz probabilidad × impacto (cualitativa)

| | Impacto alto | Impacto medio |
|--|--------------|---------------|
| **Probabilidad alta** | B1, B2, B6, U1 | B7, T3 |
| **Probabilidad media** | B3, B4, B5, R1 | T2, R3 |
| **Probabilidad baja** | T5 (si se decide rewrite) | T4 |

---

## 6. Controles inmediatos (sin desarrollar dominio aún)

1. Seguir operando REGUTRACK como sistema de registro de verdad hasta MVP RA.  
2. No comunicar a stakeholders que Compliance 360 “ya cubre registros sanitarios”.  
3. Iniciar diseño de dominio + plan de migración (I2–I4) antes de más features Studio aisladas.
