# Roadmap a clase mundial

Los niveles son acumulativos. “100” significa evidencia continua dentro de un alcance definido, no ausencia matemática de defectos.

## De 64.5 a 95/100 — 4 a 8 meses

### P0 (0–60 días)
1. Restaurar y certificar IAM productivo; Platform Admin con rol mínimo y break-glass.
2. Cerrar HTTP `:8085`, rotar secretos y habilitar secret manager/scanning.
3. Ejecutar contenedores non-root, read-only, capabilities drop y límites.
4. Automatizar backups/PITR y completar restore drill con RPO/RTO.
5. Retirar mutaciones Workflow V1.
6. Elevar cobertura real de Infrastructure/Web con PostgreSQL Testcontainers.
7. Implementar aislamiento tenant central y evaluar RLS.
8. Corregir gate CI, formato y flakiness E2E sin relajar expectativas.

### P1 (60–180 días)
1. Outbox + worker durable para notificaciones.
2. DAST autenticado, pentest independiente, SBOM y firma de imágenes.
3. SLO/SLI, métricas p95/p99, alertas y runbooks.
4. Modularización TypeScript del frontend y design system WCAG 2.2 AA.
5. Paquete CSV: URS/FRS/DS, risk assessment, IQ/OQ/PQ y traceability.

## De 95 a 97/100 — 3 a 5 meses adicionales

- Multi-region/disaster recovery ensayado.
- Pruebas de carga y caos con objetivos aprobados.
- Revisión periódica automatizada de accesos/SoD.
- Firma electrónica formal y audit trail review.
- Integraciones Enterprise con contratos, idempotencia y replay.
- Telemetría de producto y UX basada en tareas.

## De 97 a 99/100 — 4 a 8 meses adicionales

- Operación 24x7 con SRE, error budgets y postmortems.
- Certificación independiente ISO 27001 y programa secure SDLC.
- Validación regulatoria por release totalmente automatizada y aprobable.
- Accesibilidad auditada externamente.
- DR regional con ejercicios regulares y evidencia inmutable.
- Performance predictiva y capacity planning por tenant.

## De 99 a 100/100 — continuo

- Cero excepciones vencidas de riesgo alto/crítico.
- Supply-chain reproducible y provenance verificable de cada artefacto.
- Red-team recurrente y remediación dentro de SLA.
- Evidencia operacional continua enlazada a controles y releases.
- Benchmark anual contra competidores y objetivos de satisfacción.

## Orden de inversión

1. Seguridad, datos y continuidad.
2. Corrección regulatoria y eliminación de bypass.
3. Calidad/gates y observabilidad.
4. UX/accesibilidad.
5. Escala avanzada e integraciones.

No se recomienda microservicios como primer paso: aumentaría superficie operativa antes de cerrar los controles básicos.
