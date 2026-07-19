# 06 — Certificación SoD ejecutada

## Estado y alcance

La matriz inferior conserva el plan derivado de `docs/user-manual/**`; su columna `PENDING` es la línea base histórica, no el resultado actual. La ejecución real del 2026-07-18 se certifica con `docs/regulatory-affairs/security/evidence/browser-sod-steps.json`: expediente `52b09851-c122-4046-9350-89f805328d6e`, marca `2026-07-18T05:37:30.107Z`, `failed=0`, 20/20 pasos PASS. La suite Playwright SoD está confirmada 10/10 PASS; el resto de resultados confirmados se consolida en `16_REGULATORY_E2E_EXECUTION.md`.

Controles materialmente ejecutados: auto-revisión del Specialist denegada HTTP 400; aprobación interna del Specialist HTTP 403; Reviewer sin aprobar/someter HTTP 403; Approver produce `ApprovedForSubmission` y no somete HTTP 403; Submitter no aprueba HTTP 403 y produce `Submitted`; Viewer lee y recibe HTTP 403 en crear, editar, revisar, aprobar, someter y eliminar. El detalle por rol y archivos está en 07–15.

## Controles transversales

| ID | Control a ejecutar | Resultado esperado según manual | Estado |
|---|---|---|---|
| SOD-G01 | Verificar que aprobación interna y decisión externa se muestren como conceptos/estados distintos | `ApprovedForSubmission` no se presenta como aprobación MINSA/CSS; `Approved` sí representa decisión externa | PENDING |
| SOD-G02 | Intentar ejecutar acciones sin permiso por UI y por API | Botón oculto/deshabilitado y API denegada; detalle de código HTTP no especificado | PENDING |
| SOD-G03 | Verificar aislamiento por tenant durante login y operaciones | Token/contexto corresponde al tenant autenticado | PENDING |
| SOD-G04 | Verificar trazabilidad de acciones y excepciones | Acciones sensibles y override dejan auditoría; contenido exacto no especificado | PENDING |
| SOD-G05 | Verificar precondiciones de estado, incluida secuencia completa | Transiciones fuera de secuencia son rechazadas | PENDING |
| SOD-G06 | Verificar que el simulador didáctico no se use como evidencia de enforcement | La certificación se basa en ejecución real, no en la máquina local del simulador | PENDING |

## TAC — Tenant Administrator

| ID | Control a ejecutar | Resultado esperado | Estado |
|---|---|---|---|
| SOD-TAC-01 | Crear/desactivar usuario y asignar rol autorizado | Operación IAM disponible | PENDING |
| SOD-TAC-02 | Ejecutar Bootstrap | Autoridades MINSA/CSS y pack creados | PENDING |
| SOD-TAC-03 | Consultar SoD Settings y Audit Trail | Lectura disponible | PENDING |
| SOD-TAC-04 | Intentar crear expediente | Acción ausente o denegada | PENDING |
| SOD-TAC-05 | Intentar preparar, revisar, aprobar internamente, someter y aprobar externamente | Todas las acciones operativas ausentes o denegadas | PENDING |
| SOD-TAC-06 | Verificar que no sustituya roles operativos mediante acumulación de acciones | Flujo mantiene separación funcional | PENDING |

## RA-ADM — Regulatory Administrator

| ID | Control a ejecutar | Resultado esperado | Estado |
|---|---|---|---|
| SOD-ADM-01 | Ejecutar Bootstrap y Stage XLSX | Configuración/importación permitidas | PENDING |
| SOD-ADM-02 | Crear fabricante y licencia | Altas permitidas | PENDING |
| SOD-ADM-03 | Verificar la acción documentada «Nuevo producto + expediente» | Resultado esperado `Planning`; acceso a Portafolio debe aclararse por contradicción del manual | PENDING |
| SOD-ADM-04 | Intentar transiciones de preparación posteriores a la creación | Ausentes o denegadas por no operar como Specialist por defecto | PENDING |
| SOD-ADM-05 | Intentar aprobación interna, sometimiento y decisión externa | Ausentes o denegadas | PENDING |

## RA-MGR — Regulatory Manager

| ID | Control a ejecutar | Resultado esperado | Estado |
|---|---|---|---|
| SOD-MGR-01 | Registrar observación desde `Submitted`/`UnderAuthorityReview` | Expediente queda `Observed` | PENDING |
| SOD-MGR-02 | Registrar aprobación externa con CT/RS y vencimiento | `Approved` y CT/RS activo | PENDING |
| SOD-MGR-03 | Intentar decisión externa sin número CT/RS o vencimiento | Operación rechazada | PENDING |
| SOD-MGR-04 | Iniciar renovación | Renovación iniciada; contrato pendiente de aclaración documental | PENDING |
| SOD-MGR-05 | Intentar preparar, revisar, aprobar internamente y someter sin override | Acciones ausentes o denegadas | PENDING |
| SOD-MGR-06 | Ejecutar override de emergencia autorizado | Excepción limitada y auditada; mecanismo exacto no especificado | PENDING |
| SOD-MGR-07 | Intentar concentrar Specialist+Approver+Submitter sin override | Operación bloqueada | PENDING |

## RA-SPEC — Regulatory Specialist

| ID | Control a ejecutar | Resultado esperado | Estado |
|---|---|---|---|
| SOD-SPEC-01 | Crear producto+expediente y completar preparación | Secuencia hasta `ReadyForSubmission` permitida | PENDING |
| SOD-SPEC-02 | Marcar requisito recibido y responder observación | Cambios permitidos | PENDING |
| SOD-SPEC-03 | Intentar aceptar/rechazar requisito del expediente propio | Bloqueado por `PreventSelfReview` | PENDING |
| SOD-SPEC-04 | Intentar aprobación interna | Ausente o denegada | PENDING |
| SOD-SPEC-05 | Intentar sometimiento/resometimiento | Ausente o denegado | PENDING |
| SOD-SPEC-06 | Intentar decisión externa/creación CT/RS | Ausente o denegada | PENDING |

## RA-REV — Regulatory Reviewer

| ID | Control a ejecutar | Resultado esperado | Estado |
|---|---|---|---|
| SOD-REV-01 | Aceptar requisito de expediente preparado por otra persona | Requisito `Accepted` | PENDING |
| SOD-REV-02 | Rechazar requisito con comentario | Requisito `Rejected` y devolución al Specialist | PENDING |
| SOD-REV-03 | Revisar un expediente preparado por el mismo usuario | Bloqueado cuando `PreventSelfReview` esté activo | PENDING |
| SOD-REV-04 | Intentar crear/preparar expediente | Ausente o denegado | PENDING |
| SOD-REV-05 | Intentar aprobación interna, sometimiento y decisión externa | Ausentes o denegados | PENDING |

## RA-APPR — Regulatory Approver

| ID | Control a ejecutar | Resultado esperado | Estado |
|---|---|---|---|
| SOD-APPR-01 | Aprobar internamente desde `ReadyForSubmission` | Estado `ApprovedForSubmission` | PENDING |
| SOD-APPR-02 | Intentar aprobar internamente desde otro estado | Operación rechazada | PENDING |
| SOD-APPR-03 | Intentar registrar sometimiento con la misma identidad | Bloqueado por `SeparateApproverAndSubmitter` | PENDING |
| SOD-APPR-04 | Intentar preparar/revisar el expediente | Ausente o denegado | PENDING |
| SOD-APPR-05 | Intentar decisión externa/CT-RS | Ausente o denegada | PENDING |

## RA-SUB — Regulatory Submitter

| ID | Control a ejecutar | Resultado esperado | Estado |
|---|---|---|---|
| SOD-SUB-01 | Someter desde `ApprovedForSubmission` con SoD activo | Estado `Submitted` | PENDING |
| SOD-SUB-02 | Intentar someter sin aprobación interna | Operación rechazada | PENDING |
| SOD-SUB-03 | Intentar aprobar internamente con la misma identidad | Bloqueado por `SeparateApproverAndSubmitter` | PENDING |
| SOD-SUB-04 | Resometer desde `CorrectingObservation` | Estado `Resubmitted`; contrato requiere aclaración | PENDING |
| SOD-SUB-05 | Intentar preparación/revisión/decisión externa | Ausentes o denegadas | PENDING |

## RA-VIEW — Regulatory Viewer

| ID | Control a ejecutar | Resultado esperado | Estado |
|---|---|---|---|
| SOD-VIEW-01 | Navegar Dashboard, Portafolio, Pipeline, Expedientes, CT/RS y Alertas | Lectura disponible | PENDING |
| SOD-VIEW-02 | Buscar controles Nuevo, Aprobar, Someter, Stage y Bootstrap | Controles de mutación ausentes | PENDING |
| SOD-VIEW-03 | Invocar directamente cada API de mutación documentada | Todas denegadas | PENDING |
| SOD-VIEW-04 | Verificar que ninguna consulta cambie estado o requisito | Datos sin modificación | PENDING |

## QM — Quality Manager

| ID | Control a ejecutar | Resultado esperado | Estado |
|---|---|---|---|
| SOD-QM-01 | Registrar decisión externa con CT/RS y vencimiento | `Approved` y CT/RS activo | PENDING |
| SOD-QM-02 | Intentar decisión externa sin datos CT/RS requeridos | Operación rechazada | PENDING |
| SOD-QM-03 | Intentar preparar expediente | Ausente o denegado | PENDING |
| SOD-QM-04 | Intentar aprobación interna y sometimiento | Ausentes o denegados | PENDING |
| SOD-QM-05 | Verificar lectura de expedientes/reportes y Audit Trail | Lectura disponible | PENDING |
| SOD-QM-06 | Verificar que no sustituya supervisión diaria de RA-MGR salvo permiso de aprobación | Solo decisión externa permitida en RA | PENDING |

## Controles sobre contradicciones del manual

| ID | Punto a resolver/ejecutar | Estado |
|---|---|---|
| SOD-C01 | Determinar si RA-ADM debe ver Portafolio/Expedientes para la creación que `buttons.json` le asigna | PENDING |
| SOD-C02 | Determinar cómo TAC accede a lecturas RA que no figuran en sus pantallas | PENDING |
| SOD-C03 | Determinar acceso de RA-MGR a Portafolio y de RA-SPEC/RA-REV a Registros CT/RS | PENDING |
| SOD-C04 | Normalizar `REGULATORY.CONFIGURE` vs `CONFIGURE` | PENDING |
| SOD-C05 | Normalizar `DOSSIER.READ + REVIEW` vs `DOSSIER.REVIEW` | PENDING |
| SOD-C06 | Resolver el permiso `REQUIREMENT.MANAGE / UPDATE` | PENDING |
| SOD-C07 | Especificar API/precondición de `Submitted → UnderAuthorityReview` | PENDING |
| SOD-C08 | Especificar API/precondición de resometimiento | PENDING |
| SOD-C09 | Especificar API/precondición de renovación | PENDING |
| SOD-C10 | Resolver relación entre `Approved`, CT/RS activo y `SanitaryRegistration` | PENDING |
| SOD-C11 | Especificar el estado origen exigido por aprobación externa | PENDING |
| SOD-C12 | Delimitar pantallas y permisos QMS mencionados para QM | PENDING |

## Resultado de certificación

**PASS.** Los controles obligatorios ejecutados no presentan fallos (`failed=0`). No se eleva a PASS ningún control histórico no cubierto por una prueba real; el veredicto se limita al alcance ejecutado y trazado en `browser-sod-steps.json`, `sod-api-results.json`, `sod-api-evidence.json` y los journeys de rol.
