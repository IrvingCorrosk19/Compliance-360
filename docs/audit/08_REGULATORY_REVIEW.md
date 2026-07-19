# Revisión de Regulatory Affairs

## Resultado

Regulatory Affairs: **73/100**. El dominio es sustancial y supera un gestor documental básico, pero aún no constituye evidencia suficiente de un sistema GxP validado.

## Capacidades verificadas en código

- Dossiers/expedientes y requisitos por autoridad.
- Draft real y preparación controlada.
- Technical review, aprobación interna, submission y decisión externa.
- Return for correction, scope explícito, respuesta y resubmission.
- Versionado de evidencia con versión actual única.
- Historial, timeline y eventos de cambio append-only.
- Reopening y override con aprobaciones separadas.
- Cancelación/archivado como estados preservados, no borrado físico.
- Registro CT/RS y renovación.
- Optimistic locking mediante revisión esperada.
- Notificaciones de eventos V2.

## Evidencia por archivo

| Archivo/carpeta | Evidencia | Riesgo / impacto | Recomendación | Esfuerzo | Prioridad |
|---|---|---|---|---:|---|
| `Domain/RegulatoryAffairs/RegulatoryAffairsModels*.cs` | Estados, transiciones e invariantes | Positivo; reglas dispersas entre V1/V2 | State machine formal única | 3–5 sem | P0 |
| `Application/.../RegulatoryWorkflowV2Service.cs` | Corrección, reopen, override, timeline | Complejidad y dependencia de orquestador | Commands específicos + decision tables | 3–4 sem | P1 |
| `Application/.../RegulatorySoDGate.cs` | Gates por actor/acción | Correcto; requiere matriz viva | Tests generativos de pares incompatibles | 2 sem | P1 |
| `Infrastructure/Persistence/Migrations/*WorkflowV2*` | Índices únicos y append-only | Buena defensa DB | Tamper tests con rol de app real | 1 sem | P1 |
| `wwwroot/regulatory-affairs.js` | Acciones V2 expuestas por estado/rol | UI densa y riesgo de drift | Máquina de estados compartida/contrato | 4–6 sem | P1 |
| `e2e/tests/regulatory-workflow-v2.spec.ts` | Flujo y negativos | Cobertura importante, no validación regulatoria formal | IQ/OQ/PQ trazable a URS | 6–12 sem | P0 |

## Brechas frente a producto regulado clase mundial

1. No se demostró firma electrónica con significado, reautenticación, manifest y vínculo inseparable registro-firma.
2. No se demostró paquete formal URS/FRS/DS, matriz de riesgos, IQ/OQ/PQ aprobada y control de desviaciones.
3. No se demostró retención legal, legal hold, export legible completo ni verificación independiente de audit trail.
4. La coexistencia V1/V2 crea ambigüedad regulatoria.
5. No se demostró que toda notificación sea durable, reconciliable y reintentable fuera del request.
6. `RegulatoryWorkflowV2Service.cs:483-488` persiste `BeforeJson`/`AfterJson` como null aunque el esquema los contempla; la secuencia existe, pero no reconstruye exactamente qué cambió.
7. FDA, EMA, EU MDR e ISO 13485 aparecen como conceptos/certificados parciales; no hay evidencia de workflows tipados completos ni validación normativa independiente para afirmar cumplimiento.

## Veredicto regulatorio

Apto para piloto y validación formal. No debe comercializarse aún como “21 CFR Part 11 compliant” o equivalente sin completar controles técnicos y evidencia procedimental.
