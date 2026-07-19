# 04 — Matriz de acciones por rol

## Criterio

Fuente exclusiva: `docs/user-manual/**`. `P` = permitida; `F` = prohibida/no asignada; `L` = lectura/navegación; `A` = asignación ambigua o incompleta en el manual. No representa ejecución ni certificación PASS.

| Acción | TAC | RA-ADM | RA-MGR | RA-SPEC | RA-REV | RA-APPR | RA-SUB | RA-VIEW | QM |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Login | P | P | P | P | P | P | P | P | P |
| Crear/desactivar usuarios | P | F | F | F | F | F | F | F | F |
| Asignar roles | P | F | F | F | F | F | F | F | F |
| Bootstrap regulatorio | P | P | F | F | F | F | F | F | F |
| Stage XLSX REGUTRACK | F | P | F | F | F | F | F | F | F |
| Crear producto + expediente | F | P/A | F | P | F | F | F | F | F |
| Alta fabricante | F | P | F | P | F | F | F | F | F |
| Nueva licencia | F | P | F | F | F | F | F | F | F |
| Pedir docs fábrica | F | F | F | P | F | F | F | F | F |
| Docs recibidos | F | F | F | P | F | F | F | F | F |
| Armar expediente | F | F | F | P | F | F | F | F | F |
| Marcar requisito recibido | F | F | F | P | F | F | F | F | F |
| Aceptar requisito | F | F | F | F | P | F | F | F | F |
| Rechazar requisito | F | F | F | F | P | F | F | F | F |
| Declarar técnicamente completo | F | F | F | P | F | F | F | F | F |
| Aprobar internamente | F | F | F | F | F | P | F | F | F |
| Registrar sometimiento | F | F | F | F | F | F | P | F | F |
| Registrar observación autoridad | F | F | P | F | F | F | F | F | F |
| Responder observación | F | F | F | P | F | F | F | F | F |
| Resometer | F | F | F | F | F | F | P/A | F | F |
| Registrar aprobación externa + CT/RS | F | F | P | F | F | F | F | F | P |
| Iniciar renovación | F | F | P/A | F | F | F | F | F | F |
| Gestionar SoD / override | P/A | L | P/A | F | F | F | F | F | F |
| Consultar datos RA | L | L | L | L | L | L | L | L | L |
| Mutación genérica no asignada | F | F | F | F | F | F | F | F | F |

## Contratos esperados por acción

| Acción | Permiso indicado | Precondición indicada | API/operación esperada | Resultado esperado |
|---|---|---|---|---|
| Login | — | Credenciales válidas | `POST /api/v1/auth/login` | Token y redirección |
| Nuevo producto + expediente | `PRODUCT.MANAGE + DOSSIER.CREATE` | Bootstrap/autoridad disponible | `POST product + POST dossier` | Producto y expediente `Planning` |
| Pedir docs | `DOSSIER.UPDATE` | `Planning` u otro permitido | `POST …/transition → WaitingManufacturerDocuments` | Espera de documentos |
| Docs recibidos | `DOSSIER.UPDATE` | No especificada | `POST …/transition → DocumentsReceived` | Documentos recibidos |
| Armar | `DOSSIER.UPDATE` | No especificada | `POST …/transition → Assembling` | Expediente en armado |
| Marcar recibido | `REQUIREMENT.MANAGE / UPDATE` | No especificada | `PUT requirement status Received` | Requisito recibido |
| Aceptar/Rechazar | `DOSSIER.REVIEW` | No especificada | `PUT requirement status Accepted/Rejected` | Dictamen del requisito |
| Declarar completo | `DOSSIER.UPDATE` | No especificada | `POST …/transition → ReadyForSubmission` | Listo para aprobación interna |
| Aprobar internamente | `APPROVE_FOR_SUBMISSION` | `ReadyForSubmission` | `POST …/approve-for-submission` | `ApprovedForSubmission` |
| Someter | `DOSSIER.SUBMIT` | `ApprovedForSubmission` y SoD activo | `POST …/submit` | `Submitted` |
| Observar | `OBSERVATION.MANAGE` | `Submitted / UnderAuthorityReview` | `POST …/observations` | `Observed` |
| Responder | `OBSERVATION.MANAGE` | Observación abierta | `POST …/observations/{id}/respond` | `CorrectingObservation` |
| Resometer | `DOSSIER.SUBMIT` por asociación | No definida | `POST …/submit` reutilizado en simulador | `Resubmitted` |
| Aprobar externamente | `DOSSIER.APPROVE` | Número CT/RS + vencimiento | `POST …/approve` | `Approved + CT/RS activo` |
| Alta fabricante | `MANUFACTURER_DOCUMENT.MANAGE` | No especificada | `POST manufacturer` | Fabricante listado |
| Nueva licencia | `LICENSE.MANAGE` | No especificada | `POST license` | Licencia listada |
| Stage XLSX | `CONFIGURE` | No especificada | `POST import stage` | Job en staging |
| Bootstrap | `CONFIGURE` | No especificada | Seed authorities + pack | MINSA/CSS + pack |
| Renovación | `REGISTRATION.MANAGE` por descripción | No definida | `StartRenewal` sin contrato | Renovación iniciada |

## Acciones esperadas individualmente

- **TAC:** IAM, roles, perfil, Bootstrap y gestión/consulta SoD; ninguna acción operativa de expediente.
- **RA-ADM:** Bootstrap, importación, producto+expediente, fabricante y licencia; ninguna transición operativa posterior.
- **RA-MGR:** observación, decisión externa, registros/renovación y override auditado; no preparación/aprobación interna/sometimiento ordinario.
- **RA-SPEC:** preparación y respuesta; no revisión, aprobación interna, sometimiento ni decisión externa.
- **RA-REV:** aceptar/rechazar requisitos; ninguna transición de expediente.
- **RA-APPR:** una mutación: aprobación interna.
- **RA-SUB:** sometimiento y resometimiento esperado.
- **RA-VIEW:** solo navegación/lectura.
- **QM:** decisión externa+CT/RS y acciones QMS declaradas, estas últimas sin contrato funcional detallado.

## Contradicciones y vacíos

1. RA-ADM recibe explícitamente `Nuevo producto + expediente`, aunque se declara que no opera como Specialist por defecto y no tiene Portafolio en su acceso listado.
2. El resometimiento aparece en workflow/simulador, pero el botón `submit` solo documenta precondición `ApprovedForSubmission`; no se define contrato para `CorrectingObservation`.
3. Renovación figura como responsabilidad de RA-MGR y paso del simulador, pero falta en `buttons.json`.
4. La transición a `UnderAuthorityReview` no tiene acción/API en el diccionario.
5. Permisos no uniformes: `REGULATORY.CONFIGURE`/`CONFIGURE`, `DOSSIER.READ + REVIEW`/`DOSSIER.REVIEW`, `REQUIREMENT.MANAGE / UPDATE`.
6. El simulador permite avanzar y ejecutar acciones sin validar rol, permiso, precondición ni orden; es material didáctico, no evidencia de enforcement.
