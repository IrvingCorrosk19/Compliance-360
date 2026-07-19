# 03 — Rule Engine Enterprise

## 1. Objetivo

Permitir que usuarios funcionales definan qué constituye una alerta, quién debe recibirla, cuándo debe ejecutarse y cómo debe escalar, sin código ni acceso directo a SQL.

## 2. Wizard de regla

1. Identidad: código, nombre, propósito, módulo, owner, backup owner y criticidad.
2. Evento o schedule.
3. Condiciones visuales.
4. Agregación, ventana y claves.
5. Dedupe y throttling.
6. Audiencia y fallbacks.
7. Canales y preferencias.
8. Templates/locales.
9. SLA y escalamiento.
10. Calendario, timezone y quiet hours.
11. Resolución automática.
12. Simulación y volumen estimado.
13. Resumen en lenguaje natural.
14. Envío a revisión.

La pantalla resume siempre:

```text
CUANDO [evento]
SI [condiciones]
AGRUPADO POR [claves] DURANTE [ventana]
NOTIFICAR A [audiencia]
POR [canales] USANDO [templates]
EXCEPTO [supresiones/quiet hours]
ESCALAR SI [SLA]
CERRAR CUANDO [condición]
```

## 3. AST permitido

Nodos:

- `All` (AND)
- `Any` (OR)
- `Not`
- `Compare`
- `Exists`
- `IsNull`
- `IsEmpty`
- `Changed`
- `Temporal`
- `CollectionAny`
- `CollectionAll`
- `CollectionNone`
- `Aggregate`
- `Lookup`
- `Constant`
- `VariableReference`

Prohibido:

- SQL, C#, JavaScript y scripts.
- Reflexión, archivos, red o secretos.
- Loops, recursión y user-defined functions.
- Campos no registrados.
- Consultas libres.
- Regex sin límites; usar patrón seguro compatible con RE2.

## 4. Operadores

### Comparación

Equal, NotEqual, GreaterThan, GreaterOrEqual, LessThan, LessOrEqual, Between, In y sus negaciones.

### Texto

Contains, StartsWith, EndsWith, EqualsIgnoreCase, SafePattern y Length.

### Fechas

Before, After, On, WithinLast, WithinNext, Age, BusinessDaysUntil y OverdueBy.

### Cambio

Changed, ChangedFrom, ChangedTo, Increased, Decreased, CrossedAbove, CrossedBelow, AddedToCollection y RemovedFromCollection.

### Colecciones

Contains, ContainsAny, ContainsAll, Count, Any/All/None con condición anidada.

### Agregaciones

Count, DistinctCount, Sum, Average, Min y Max.

Ventanas: tumbling, sliding, calendar y session. Se define late-arrival y eventos fuera de orden.

## 5. Tipado y semántica

- Lógica trivaluada: `true`, `false`, `unknown`.
- Cada regla declara política ante `unknown`.
- Decimales sin coerción flotante.
- Strings con locale explícito.
- Fechas con timezone explícito.
- Coerciones seguras y visibles.
- Límites configurados: profundidad, nodos, listas, cardinalidad y coste.
- Evaluación determinista para la misma versión y el mismo snapshot.

## 6. Catálogo inicial de eventos

### Regulatory Affairs

- Dossier creado/asignado/reasignado.
- Owner/responsable cambiado.
- Estado cambiado.
- Aprobación interna concedida/rechazada.
- Sometido/resometido/reabierto/cancelado.
- Corrección solicitada/enviada/aceptada/vencida.
- Revisión técnica iniciada/completada.
- Observación abierta/asignada/próxima a vencer/vencida/respondida/cerrada.
- Hito próximo a vencer/vencido/completado.
- Requisito faltante/cargado/validado/rechazado/dispensado.
- Registro sanitario, certificado y licencia próximos a vencer/vencidos/renovados.
- Evidencia cargada/revisada/reemplazada/anulada.
- Importación iniciada/completada/fallida.

### Documents y Workflow

- Documento creado, enviado, aprobado, rechazado, publicado, versionado, retirado o vencido.
- Firma solicitada, completada, rechazada o vencida.
- Workflow iniciado, completado, cancelado o fallido.
- Paso asignado, reasignado, próximo a vencer, vencido o escalado.

### QMS

- CAPA y acciones creadas, vencidas, verificadas o cerradas.
- Riesgo/score/control fuera de umbral.
- Auditoría/hallazgo/respuesta/evidencia.
- Supplier/document/evaluation vencido o degradado.
- Indicador target/tendencia/semáforo.

### Seguridad y operación

- Usuario/rol/permiso crítico.
- Conflicto SoD.
- Login sospechoso.
- Secreto/certificado/integración próximo a vencer.
- Provider degradado/recuperado.
- Backlog/DLQ/SLO fuera de objetivo.
- Bounce/complaint/suppression.
- Publicación/activación/rollback de configuración.

## 7. Variables

Namespaces:

- `event.*`
- `entity.*`
- `previous.*`
- `change.*`
- `actor.*`
- `owner.*`
- `assignee.*`
- `manager.*`
- `tenant.*`
- `authority.*`
- `jurisdiction.*`
- `product.*`
- `workflow.*`
- `alert.*`
- `sla.*`
- `schedule.*`
- `aggregate.*`
- `recipient.*`
- `branding.*`
- `system.*`

Cada variable registra tipo, formato, nullable, cardinalidad, fuente, freshness, sensibilidad, contextos, canales, masking, funciones permitidas, schema versions y reemplazo.

Variables creadas por el cliente son bindings declarativos sobre campos registrados, constantes o lookups aprobados. No contienen código.

## 8. Destinatarios

Selectores:

- usuario;
- actor/creator/owner/current or previous responsible;
- approver/reviewer/submitter;
- manager y cadena de managers;
- rol/grupo/equipo/departamento;
- miembros del expediente/workflow;
- owner por producto/país/autoridad;
- on-call;
- suscriptores;
- lista de distribución;
- contacto externo aprobado;
- expresión segura;
- fallback.

Operaciones: union, intersection, exclusion, fallback ordenado, first available, escalation chain, quorum y deduplicación.

El snapshot conserva por qué cada destinatario fue incluido/excluido y qué preferencia/fallback se aplicó.

## 9. Dedupe, throttling y suppressions

Cada regla define:

- clave determinista;
- ventana;
- estrategia: ignore, merge, increment counter, update existing o create new;
- límite por tenant/user/entity/channel;
- comportamiento al exceder;
- quiet hours;
- mandatory override;
- digest policy.

Precedencia:

1. prohibición legal/suppression;
2. residencia/jurisdicción;
3. mandato de seguridad;
4. regla;
5. delegación/ausencia;
6. preferencias;
7. fallback de canal;
8. fallback de destinatario.

## 10. Lifecycle y gobierno

Estado de versión:

`Draft → InReview → Approved → Published → Superseded → Retired`

Ramas: `InReview → ChangesRequested → Draft` y `InReview → Rejected`.

Solo Draft es editable. Published es inmutable.

Estado de deployment:

`NotDeployed → Scheduled → Active ↔ Paused → Disabled/RolledBack`.

SoD:

- maker no aprueba;
- checker no edita;
- publisher y activator pueden requerir separación;
- reglas críticas requieren dos aprobadores;
- break-glass requiere MFA, motivo, expiración y revisión.

## 11. Simulación

Modos:

- evento/entidad puntual;
- fixture sintético;
- replay histórico;
- comparación current/candidate;
- shadow production sin side effects;
- volumen/coste.

Resultado:

- árbol y valor por nodo;
- match/no-match/error;
- variables enmascaradas;
- audiencia y razones;
- template/locale;
- schedule/quiet hours;
- dedupe/throttle/digest;
- SLA/escalamientos;
- canales/providers;
- warnings de PII, cardinalidad y volumen.

La aprobación puede exigir casos positivos, negativos, nulos, DST, dedupe, audiencia vacía, locales y máximo volumen.

## 12. Servicios y contratos

- `IAlertEventPublisher`
- `IAlertEventSchemaRegistry`
- `IAlertDefinitionService`
- `IAlertRuleCompiler`
- `IAlertEvaluator`
- `IRecipientResolver`
- `IVariableCatalog`
- `ISimulationService`
- `IAlertLifecycleService`
- `IAlertApprovalService`
- `IAlertDeploymentService`

El compilador valida y convierte AST a un plan interno seguro. Nunca genera SQL dinámico desde la expresión del usuario.

## 13. APIs

Base `/api/v2/tenants/{tenantId}/alert-center`.

- CRUD y lifecycle `/events`.
- CRUD/versiones/lifecycle `/definitions`.
- `POST /definitions/{id}/simulate`.
- CRUD `/variables`.
- CRUD/versiones `/recipient-policies`.
- `POST /recipient-policies/{id}/simulate`.
- CRUD/versiones `/sla-policies`.
- CRUD/versiones `/escalation-policies`.
- Deployments `/definitions/{id}/deployments`.
- Promotion packages `/promotions`.

Mutaciones críticas usan `Idempotency-Key`, `If-Match`, ProblemDetails y auditoría.

## 14. Validaciones

Antes de review:

- owner y propósito;
- AST válido/tipado;
- evento/variables vigentes;
- audience o fallback;
- template por canal/locale;
- timezone/DST;
- SLA coherente;
- dedupe/throttle;
- no ciclos;
- sensibilidad compatible;
- simulación PASS.

Antes de publish:

- approvals y SoD;
- diff sin cambios posteriores;
- schemas compatibles;
- impact analysis;
- pruebas obligatorias;
- hash/firma.

Antes de activate:

- providers healthy;
- calendars/secrets/dependencies disponibles;
- cuotas;
- rollback target;
- no exclusividad incompatible.

