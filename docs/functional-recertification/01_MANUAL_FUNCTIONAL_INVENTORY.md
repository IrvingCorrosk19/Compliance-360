# 01 — Inventario funcional derivado del manual

## Alcance y criterio

Fuente funcional exclusiva: `docs/user-manual/**` (índice, páginas por rol, JSON, JavaScript del simulador y documentos del manual). Este inventario expresa comportamiento **esperado según el manual**; no certifica implementación ni ejecución. Cuando el manual no define un dato, se indica «no especificado». No se emite veredicto PASS.

El universo de pantallas usado para identificar elementos ocultos es: Inicio de sesión, Tenant Administration, Security, Audit Trail, Dashboard RA, Portafolio, Pipeline, Expedientes/Workspace, Registros CT/RS, Fabricantes, Licencias, Alertas, Importación, Configuración y SoD Settings.

## TAC

- **código:** TAC
- **nombre:** Tenant Administrator
- **objetivo:** Administrar usuarios, roles y configuración del tenant sin operar expedientes por defecto.
- **etapa:** IAM y configuración previa; fuera del flujo operativo del expediente.
- **menús/pantallas visibles/ocultos:** Visibles: Inicio de sesión, Tenant Administration, Security, Audit Trail, Dashboard RA, Configuración y SoD Settings. Ocultos/no asignados por la lista del rol: Portafolio, Pipeline, Expedientes/Workspace, Registros CT/RS, Fabricantes, Licencias, Alertas e Importación. El texto libre, sin embargo, afirma lectura de productos, expedientes, CT/RS y licencias.
- **acciones permitidas/prohibidas:** Permitidas: crear/desactivar usuarios, asignar roles, configurar branding/perfil, ejecutar Bootstrap, consultar SoD, lecturas RA y auditoría. Prohibidas: crear expedientes, aprobar internamente, someter, registrar decisión externa/CT-RS y sustituir roles operativos.
- **permisos:** `TENANT.USERS`, `TENANT.ROLES`, `RBAC.MANAGE`, `REGULATORY.CONFIGURE`/`CONFIGURE`, `REGULATORY.SOD.MANAGE`, `AUDIT.READ`, `TENANT.AUDIT`.
- **estados visibles/modificables:** Visibles: estados RA de forma declarada pero sin delimitación por estado. Modificables: ninguno del expediente; Bootstrap modifica catálogos/configuración.
- **transiciones permitidas/prohibidas:** Permitida: ninguna transición de expediente. Prohibidas: todas las transiciones operativas de preparación, revisión, aprobación interna, sometimiento, observación, decisión externa y renovación.
- **SoD:** Debe habilitar roles separados y no operar como Specialist, Reviewer, Approver o Submitter.
- **APIs esperadas:** `POST /api/v1/auth/login`; Bootstrap «Seed authorities + pack» (ruta no especificada); APIs IAM/SoD no especificadas.
- **solo lectura:** Sí para datos RA y SoD; no para IAM, perfil/configuración y Bootstrap.
- **resultado esperado:** Usuarios y roles correctos, SoD habilitado, MINSA/CSS y pack disponibles, sin mutación de expedientes por TAC.

## RA-ADM

- **código:** RA-ADM
- **nombre:** Regulatory Administrator
- **objetivo:** Configurar Regulatory Affairs: autoridades, packs, alertas, importación REGUTRACK, productos, fabricantes y licencias.
- **etapa:** Configuración previa al flujo del expediente.
- **menús/pantallas visibles/ocultos:** Visibles: Inicio de sesión, Dashboard RA, Fabricantes, Licencias, Alertas, Importación, Configuración y SoD Settings. Ocultos/no asignados: Tenant Administration, Security, Audit Trail, Portafolio, Pipeline, Expedientes/Workspace y Registros CT/RS. No obstante, «Nuevo producto + expediente» se asigna al rol en Portafolio.
- **acciones permitidas/prohibidas:** Permitidas: Bootstrap, Stage XLSX, nuevo producto+expediente, alta de fabricante, nueva licencia y consultas. Prohibidas: aprobación interna, sometimiento, decisión externa y operar como Specialist por defecto.
- **permisos:** `REGULATORY.CONFIGURE`/`CONFIGURE`, `PRODUCT.MANAGE`, `DOSSIER.CREATE`, `MANUFACTURER_DOCUMENT.MANAGE`, `LICENSE.MANAGE`; permiso de lectura no especificado.
- **estados visibles/modificables:** Visibles: lecturas del módulo sin estados delimitados. Modificable: creación deja expediente en `Planning`; no se le asignan transiciones posteriores.
- **transiciones permitidas/prohibidas:** Permitida: creación en `Planning`. Prohibidas: preparación posterior, revisión, aprobación interna, sometimiento, observación/respuesta, decisión externa y renovación.
- **SoD:** Configura insumos, pero no debe asumir las etapas Specialist/Approver/Submitter/Manager.
- **APIs esperadas:** `POST /api/v1/auth/login`; `POST product + POST dossier`; `POST manufacturer`; `POST license`; `POST import stage`; «Seed authorities + pack».
- **solo lectura:** No para configuración, importación, productos, fabricantes y licencias; sí para SoD y demás lecturas.
- **resultado esperado:** Autoridades y pack disponibles, importación en staging, catálogos operativos y, si usa la acción documentada, producto/expediente creado en `Planning`.

## RA-MGR

- **código:** RA-MGR
- **nombre:** Regulatory Manager
- **objetivo:** Supervisar pipeline, observaciones de autoridad, decisión externa, CT/RS y renovaciones.
- **etapa:** Seguimiento de autoridad, observación/respuesta, decisión externa, CT/RS y renovación.
- **menús/pantallas visibles/ocultos:** Visibles: Inicio de sesión, Dashboard RA, Pipeline, Expedientes/Workspace, Registros CT/RS, Alertas y SoD Settings. Ocultos/no asignados: Tenant Administration, Security, Audit Trail, Portafolio, Fabricantes, Licencias, Importación y Configuración. El texto libre sí afirma que puede ver Portafolio.
- **acciones permitidas/prohibidas:** Permitidas: actualizar expedientes, registrar observación, registrar aprobación externa+CT/RS, gestionar registros/renovaciones y override de emergencia auditado. Prohibidas: concentrar Specialist+Approver+Submitter sin override y confundir aprobación interna con decisión externa.
- **permisos:** `DOSSIER.UPDATE`, `OBSERVATION.MANAGE`, `DOSSIER.APPROVE`, `REGISTRATION.MANAGE`; permiso concreto de SoD/override no especificado.
- **estados visibles/modificables:** Visibles: pipeline completo. Modificables documentados: `Submitted`/`UnderAuthorityReview` hacia `Observed`; decisión externa hacia `Approved`; renovación sin estado final definido.
- **transiciones permitidas/prohibidas:** Permitidas/esperadas: seguimiento `Submitted → UnderAuthorityReview`, observación a `Observed`, decisión externa a `Approved`, inicio de renovación. Prohibidas por separación: preparación, revisión, aprobación interna y sometimiento ordinarios, salvo override documentado pero no detallado.
- **SoD:** Override de emergencia debe ser auditado; no debe acumular Specialist, Approver y Submitter sin él.
- **APIs esperadas:** `POST /api/v1/auth/login`; `POST …/observations`; `POST …/approve`; `StartRenewal` y actualización de expediente sin rutas especificadas.
- **solo lectura:** No en seguimiento/observación/decisión/renovación; sí en vistas no operadas.
- **resultado esperado:** Observación registrada, decisión externa diferenciada de la interna, CT/RS activo y renovación iniciada con excepción SoD auditada cuando corresponda.

## RA-SPEC

- **código:** RA-SPEC
- **nombre:** Regulatory Specialist
- **objetivo:** Preparar productos y expedientes, completar requisitos y responder observaciones.
- **etapa:** `Planning → WaitingManufacturerDocuments → DocumentsReceived → Assembling → ReadyForSubmission`; respuesta de observaciones.
- **menús/pantallas visibles/ocultos:** Visibles: Inicio de sesión, Dashboard RA, Portafolio, Pipeline, Expedientes/Workspace y Fabricantes. Ocultos/no asignados: Tenant Administration, Security, Audit Trail, Registros CT/RS, Licencias, Alertas, Importación, Configuración y SoD Settings. El texto libre afirma lectura de CT/RS.
- **acciones permitidas/prohibidas:** Permitidas: crear producto+expediente, pedir/recibir documentos, armar, marcar requisito recibido, declarar completo, alta fabricante y responder observación. Prohibidas: autorrevisión, aprobación interna, sometimiento y decisión externa/CT-RS.
- **permisos:** `PRODUCT.MANAGE`, `DOSSIER.CREATE`, `DOSSIER.UPDATE`, `REQUIREMENT.MANAGE / UPDATE`, `MANUFACTURER_DOCUMENT.MANAGE`, `OBSERVATION.MANAGE`.
- **estados visibles/modificables:** Visibles: pipeline/expedientes y CT/RS según texto libre. Modificables: estados de preparación, requisito `Received` y respuesta `CorrectingObservation`.
- **transiciones permitidas/prohibidas:** Permitidas: preparación hasta `ReadyForSubmission` y respuesta hacia `CorrectingObservation`. Prohibidas: aceptación/rechazo técnico, `ReadyForSubmission → ApprovedForSubmission`, sometimiento/resometimiento y aprobación externa.
- **SoD:** `PreventSelfReview`: no revisar el expediente propio.
- **APIs esperadas:** `POST product + POST dossier`; `POST …/transition` a `WaitingManufacturerDocuments`, `DocumentsReceived`, `Assembling`, `ReadyForSubmission`; `PUT requirement status Received`; `POST manufacturer`; `POST …/observations/{id}/respond`.
- **solo lectura:** No en preparación/requisitos/respuesta/fabricantes; sí en Dashboard y CT/RS.
- **resultado esperado:** Expediente técnicamente completo para Reviewer/Approver y respuestas de observación registradas, sin ejecutar etapas incompatibles.

## RA-REV

- **código:** RA-REV
- **nombre:** Regulatory Reviewer
- **objetivo:** Revisar técnicamente cada requisito mediante aceptación o rechazo.
- **etapa:** Revisión técnica en `Assembling / ReadyForSubmission`, antes de la aprobación interna.
- **menús/pantallas visibles/ocultos:** Visibles: Inicio de sesión, Dashboard RA, Pipeline y Expedientes/Workspace. Ocultos/no asignados: Tenant Administration, Security, Audit Trail, Portafolio, Registros CT/RS, Fabricantes, Licencias, Alertas, Importación, Configuración y SoD Settings. El texto libre afirma consulta de CT/RS.
- **acciones permitidas/prohibidas:** Permitidas: abrir expedientes, aceptar o rechazar requisitos con comentario y consultar. Prohibidas: crear/preparar, aprobar internamente, someter y preparar el mismo caso cuando `PreventSelfReview` está activo.
- **permisos:** `DOSSIER.READ + REVIEW` en la descripción; `DOSSIER.REVIEW` en el diccionario de botones.
- **estados visibles/modificables:** Visibles: expedientes en revisión y consultas. Modificables: estado del requisito a `Accepted` o `Rejected`; el estado del expediente no se define como modificado por estas acciones.
- **transiciones permitidas/prohibidas:** No se le asigna transición de expediente; solo cambios de requisitos. Prohibidas: preparación, aprobación interna, sometimiento, observación/respuesta, decisión externa y renovación.
- **SoD:** Separación preparación-revisión; `PreventSelfReview` impide revisar el caso preparado por la misma persona.
- **APIs esperadas:** `PUT requirement status Accepted`; `PUT requirement status Rejected`; lectura de expedientes sin ruta especificada.
- **solo lectura:** No para dictamen de requisitos; sí para Dashboard/CT-RS y resto de datos.
- **resultado esperado:** Requisitos aceptados o rechazados; rechazo vuelve al Specialist para corrección; no se ejecuta aprobación interna.

## RA-APPR

- **código:** RA-APPR
- **nombre:** Regulatory Approver
- **objetivo:** Autorizar internamente el sometimiento sin representar la decisión de MINSA/CSS.
- **etapa:** `ReadyForSubmission → ApprovedForSubmission`.
- **menús/pantallas visibles/ocultos:** Visibles: Inicio de sesión, Dashboard RA, Pipeline y Expedientes/Workspace. Ocultos/no asignados: Tenant Administration, Security, Audit Trail, Portafolio, Registros CT/RS, Fabricantes, Licencias, Alertas, Importación, Configuración y SoD Settings.
- **acciones permitidas/prohibidas:** Permitida: aprobar internamente y consultar lecturas RA. Prohibidas: preparar, someter y registrar decisión externa/CT-RS.
- **permisos:** `APPROVE_FOR_SUBMISSION`; permisos de lectura no especificados.
- **estados visibles/modificables:** Visible/modificable: `ReadyForSubmission` a `ApprovedForSubmission`; demás estados en consulta no delimitada.
- **transiciones permitidas/prohibidas:** Permitida: aprobación interna. Prohibidas: preparación, revisión, sometimiento/resometimiento, observación/respuesta y decisión externa.
- **SoD:** `SeparateApproverAndSubmitter`: quien aprueba internamente no debe someter.
- **APIs esperadas:** `POST …/approve-for-submission`; lecturas sin ruta especificada.
- **solo lectura:** No en aprobación interna; sí fuera de esa acción.
- **resultado esperado:** Expediente `ApprovedForSubmission`, claramente distinto de aprobación externa, listo para RA-SUB.

## RA-SUB

- **código:** RA-SUB
- **nombre:** Regulatory Submitter
- **objetivo:** Registrar el sometimiento real ante la autoridad.
- **etapa:** `ApprovedForSubmission → Submitted`; el flujo también le atribuye resometimiento a `Resubmitted`.
- **menús/pantallas visibles/ocultos:** Visibles: Inicio de sesión, Dashboard RA, Pipeline y Expedientes/Workspace. Ocultos/no asignados: Tenant Administration, Security, Audit Trail, Portafolio, Registros CT/RS, Fabricantes, Licencias, Alertas, Importación, Configuración y SoD Settings.
- **acciones permitidas/prohibidas:** Permitidas: registrar sometimiento y consultar. Prohibidas: aprobación interna, decisión externa/CT-RS y preparación/revisión.
- **permisos:** `DOSSIER.SUBMIT`; permisos de lectura no especificados.
- **estados visibles/modificables:** Visible/modificable: `ApprovedForSubmission → Submitted`; `CorrectingObservation → Resubmitted` aparece en flujo/simulador, pero no queda definido en el diccionario de botón.
- **transiciones permitidas/prohibidas:** Permitidas: sometimiento y resometimiento esperado. Prohibidas: preparación, revisión, aprobación interna, observación/respuesta y decisión externa.
- **SoD:** Separación Approver-Submitter; el sometimiento ordinario requiere aprobación interna y SoD activo.
- **APIs esperadas:** `POST …/submit`; el manual reutiliza conceptualmente esta acción para resometer, sin especificar contrato distinto.
- **solo lectura:** No para someter/resometer; sí para el resto.
- **resultado esperado:** Expediente `Submitted` o `Resubmitted`, entregado al RA-MGR para seguimiento.

## RA-VIEW

- **código:** RA-VIEW
- **nombre:** Regulatory Viewer
- **objetivo:** Consulta transversal del portafolio regulatorio, pipeline, expedientes, CT/RS y alertas.
- **etapa:** Sin etapa de escritura.
- **menús/pantallas visibles/ocultos:** Visibles: Inicio de sesión, Dashboard RA, Portafolio, Pipeline, Expedientes/Workspace, Registros CT/RS y Alertas. Ocultos/no asignados: Tenant Administration, Security, Audit Trail, Fabricantes, Licencias, Importación, Configuración y SoD Settings.
- **acciones permitidas/prohibidas:** Permitidas: navegar, abrir registros y consultar. Prohibido: todo botón de mutación, incluidos Nuevo, Aprobar, Someter, Stage y Bootstrap.
- **permisos:** Permisos de lectura no especificados en el manual.
- **estados visibles/modificables:** Visibles: todos los estados que aparezcan en portafolio/pipeline/expedientes/CT-RS; modificables: ninguno.
- **transiciones permitidas/prohibidas:** Ninguna permitida; todas las transiciones y cambios de requisito están prohibidos.
- **SoD:** Control de solo lectura; ausencia de botones de mutación.
- **APIs esperadas:** `POST /api/v1/auth/login` y lecturas no especificadas; ninguna API de mutación.
- **solo lectura:** Sí, integral.
- **resultado esperado:** Información consultable sin cambios ni entrega operativa.

## QM

- **código:** QM
- **nombre:** Quality Manager
- **objetivo:** Coordinar QMS y apoyar/ejecutar el registro de decisión externa y CT/RS sin preparar expedientes.
- **etapa:** Decisión externa/CT-RS junto o en apoyo al RA-MGR.
- **menús/pantallas visibles/ocultos:** Visibles: Inicio de sesión, Audit Trail, Dashboard RA, Expedientes/Workspace y Registros CT/RS. Ocultos/no asignados: Tenant Administration, Security, Portafolio, Pipeline, Fabricantes, Licencias, Alertas, Importación, Configuración y SoD Settings. El manual menciona acciones QMS, pero no inventaría sus pantallas porque no están en el inventario.
- **acciones permitidas/prohibidas:** Permitidas: registrar aprobación externa/CT-RS, leer expedientes/reportes/auditoría y aprobar documentos/CAPA/riesgos/fichas en QMS. Prohibidas: preparar expedientes y sustituir supervisión diaria del Manager salvo permiso de aprobación.
- **permisos:** `DOSSIER.APPROVE`, `REGISTRATION.MANAGE`; permisos QMS y de lectura/auditoría no especificados.
- **estados visibles/modificables:** Visible: expediente sometido y registros CT/RS. Modificable: decisión externa hacia `Approved` y creación/activación CT/RS.
- **transiciones permitidas/prohibidas:** Permitida: decisión externa. Prohibidas: preparación, revisión, aprobación interna, sometimiento, observación/respuesta y renovación, salvo que otro permiso no documentado las habilite.
- **SoD:** Puede apoyar la decisión externa, pero no preparar ni asumir la supervisión cotidiana del Manager.
- **APIs esperadas:** `POST …/approve`; lecturas y acciones QMS no especificadas.
- **solo lectura:** No para decisión externa ni acciones QMS declaradas; sí para expedientes/reportes/auditoría fuera de esas acciones.
- **resultado esperado:** Aprobación externa registrada y CT/RS activo con trazabilidad QMS, sin preparación del expediente.

## Contradicciones y vacíos internos que afectan el inventario

1. RA-ADM tiene «Nuevo producto + expediente» y `DOSSIER.CREATE`, pero su lista de pantallas omite Portafolio y a la vez indica que no opera como Specialist por defecto.
2. TAC declara lectura de productos, expedientes, CT/RS y licencias, pero su lista de pantallas no incluye Portafolio, Expedientes, Registros CT/RS ni Licencias.
3. RA-MGR declara ver Portafolio, pero Portafolio no aparece entre sus pantallas.
4. RA-SPEC y RA-REV declaran lectura de CT/RS, pero Registros CT/RS no aparece entre sus pantallas.
5. El flujo atribuye `StartRenewal` al RA-MGR, pero no existe botón/API correspondiente en el diccionario.
6. El flujo incluye `Submitted → UnderAuthorityReview` y `CorrectingObservation → Resubmitted`, pero el diccionario no define botones/contratos específicos; `POST …/submit` solo documenta precondición `ApprovedForSubmission`.
7. La decisión externa produce «`Approved + CT/RS activo`», mientras el flujo añade un estado `SanitaryRegistration` que no figura en `statusLabels`.
8. Nombres de permisos no uniformes: `REGULATORY.CONFIGURE` frente a `CONFIGURE`; `DOSSIER.READ + REVIEW` frente a `DOSSIER.REVIEW`; `REQUIREMENT.MANAGE / UPDATE` no identifica un nombre único.
9. QM declara acciones en módulos QMS, pero el inventario de pantallas del manual solo enumera pantallas Enterprise/RA/Audit.
10. La API de aprobación externa exige número y vencimiento, pero no documenta precondición de estado, aunque el flujo y la entrega a QM/Manager parten de expedientes sometidos.

## Reconciliación con evidencia real — 2026-07-18

Este apartado no modifica el manual ni redefine su expectativa: identifica qué elementos del inventario ya tienen ejecución real. La comparación detallada está en `02_MANUAL_VS_SYSTEM_MATRIX.md`.

- Flujo RA ejecutado con expediente `ef732409-4161-4cd8-8235-6f30b9eb3429`, catálogo `FLOW-983332`, sin pasos fallidos (`manual-workflow-steps.json`, `2026-07-18T05:37:06.091Z`).
- SoD ejecutado con expediente `52b09851-c122-4046-9350-89f805328d6e`, sin pasos fallidos (`browser-sod-steps.json`, `2026-07-18T05:37:30.107Z`).
- Viajes reales de RA-SPEC, RA-REV, RA-APPR, RA-SUB, RA-MGR y RA-VIEW: todos `pass=true`, entre `2026-07-18T05:37:53.132Z` y `2026-07-18T05:39:12.000Z`.
- TAC, RA-ADM y QM tienen controles UI/API reales en `manual-vs-system-role-matrix.json`; las capturas TAC/QM están indexadas en `18_BROWSER_EVIDENCE_INDEX.md`.
- Funcionalidad corregida y verificada: evidencia almacenada, rechazo comentado, sometimiento con trámite/número externo/fecha/comprobante, observaciones/respuestas/resolución, alert settings, edición TAC, `UserStatus.Disabled=3` y reactivación QMS.
