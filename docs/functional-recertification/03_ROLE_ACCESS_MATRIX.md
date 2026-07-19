# 03 — Matriz de acceso por rol

## Criterio

Derivada exclusivamente de `docs/user-manual/**`. `V` = visible según `roles.json`/página del rol; `H` = no asignada al rol (esperada oculta); `V*` = acceso mencionado en texto pero ausente o inconsistente en la lista de pantallas. La matriz expresa expectativa documental, no evidencia ejecutada. No se emite PASS.

| Pantalla / menú | TAC | RA-ADM | RA-MGR | RA-SPEC | RA-REV | RA-APPR | RA-SUB | RA-VIEW | QM |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Inicio de sesión | V | V | V | V | V | V | V | V | V |
| Tenant Administration | V | H | H | H | H | H | H | H | H |
| Security | V | H | H | H | H | H | H | H | H |
| Audit Trail | V | H | H | H | H | H | H | H | V |
| Dashboard RA | V | V | V | V | V | V | V | V | V |
| Portafolio | V* | V* | V* | V | H | H | H | V | H |
| Pipeline | H | H | V | V | V | V | V | V | H |
| Expedientes / Workspace | V* | V* | V | V | V | V | V | V | V |
| Registros CT/RS | V* | H | V | V* | V* | H | H | V | V |
| Fabricantes | H | V | H | V | H | H | H | H | H |
| Licencias | V* | V | H | H | H | H | H | H | H |
| Alertas | H | V | V | H | H | H | H | V | H |
| Importación | H | V | H | H | H | H | H | H | H |
| Configuración | V | V | H | H | H | H | H | H | H |
| SoD Settings | V | V | V | H | H | H | H | H | H |

## Acceso esperado individual

- **TAC:** acceso de escritura a IAM, perfil/configuración y Bootstrap; lectura de Security, Audit, SoD y RA. Deben ocultarse controles operativos del expediente.
- **RA-ADM:** acceso de escritura a configuración, importación, fabricantes y licencias. El manual también le asigna creación de producto+expediente, aunque no lista Portafolio ni Expedientes.
- **RA-MGR:** acceso operativo a Dashboard, Pipeline, Expedientes, Registros, Alertas y SoD. Portafolio es mencionado, pero no listado.
- **RA-SPEC:** acceso operativo a Portafolio, Pipeline, Expedientes y Fabricantes; Dashboard y CT/RS en lectura. La pantalla de CT/RS no está listada.
- **RA-REV:** acceso operativo a Expedientes para aceptar/rechazar requisitos; Dashboard/Pipeline y CT/RS en lectura. Registros CT/RS no está listado.
- **RA-APPR:** Dashboard, Pipeline y Expedientes; solo muta la aprobación interna.
- **RA-SUB:** Dashboard, Pipeline y Expedientes; solo muta sometimiento/resometimiento.
- **RA-VIEW:** Dashboard, Portafolio, Pipeline, Expedientes, Registros CT/RS y Alertas en solo lectura.
- **QM:** Expedientes/Registros CT/RS para decisión externa; Dashboard y Audit Trail en lectura. Las pantallas QMS mencionadas no están inventariadas.

## Controles de acceso esperados

1. La visibilidad de pantalla no sustituye la autorización de acción; cada mutación debe exigir el permiso documentado.
2. RA-VIEW no debe mostrar controles de mutación.
3. TAC no debe acceder a acciones del flujo operativo aunque consulte datos RA.
4. RA-APPR no debe mostrar Someter; RA-SUB no debe mostrar Aprobar internamente.
5. RA-SPEC no debe mostrar Aceptar/Rechazar ni aprobación/sometimiento/decisión externa.
6. RA-REV no debe mostrar creación/preparación/aprobación/sometimiento.
7. RA-MGR y QM solo deben mostrar aprobación externa en el contexto funcional esperado.

## Contradicciones internas relevantes

- RA-ADM tiene acción en Portafolio, pero Portafolio no figura en sus pantallas; «no operar como Specialist» queda ambiguo frente a `DOSSIER.CREATE`.
- TAC, RA-MGR, RA-SPEC y RA-REV mencionan lecturas que no aparecen en sus listas de pantallas.
- El simulador visual presenta el mismo menú lateral (incluidos Portafolio y Registros CT/RS) para todos los roles, aunque las páginas por rol declaran accesos distintos. Debe tratarse como simulación genérica, no como prueba de autorización.
- QM declara funciones QMS sin pantallas QMS en `screens.json` ni en su lista de pantallas.

## Acceso real recertificado — 2026-07-18

La expectativa anterior permanece como transcripción del manual. La ejecución actual resolvió sus ambigüedades así:

| Rol | Acceso real verificado | Restricción real verificada | Evidencia | Resultado |
|---|---|---|---|---|
| TAC | `dashboard,audit-trail,regulatory,tenant-administration,security`; tabs RA de lectura/configuración | Nuevo producto oculto; mutaciones RA 403 | `manual-vs-system-role-matrix.json`, bloque TAC | PASS |
| RA-ADM | `dashboard,regulatory`; portfolio, fabricantes, licencias, alertas, import, config, SoD | pipeline/dossiers/registrations ocultos; acciones operativas 403 | mismo JSON, bloque RA-ADM | PASS |
| RA-MGR | portfolio, pipeline, dossiers, registrations, alerts, SoD | bootstrap/producto/aprobación interna/sometimiento 403 | mismo JSON, bloque RA-MGR | PASS |
| RA-SPEC | portfolio, pipeline, dossiers, registrations, manufacturers | administración/security/audit por URL denegados; aprobar/someter 403 | `journey-ra-spec.json` | PASS |
| RA-REV | pipeline, dossiers, registrations | portfolio oculto; crear/transicionar/aprobar/someter 403 | `journey-ra-rev.json` | PASS |
| RA-APPR | pipeline, dossiers, registrations | editar/someter/aprobar externamente 403 | `journey-ra-appr.json` | PASS |
| RA-SUB | pipeline, dossiers, registrations | editar/aprobar/observar 403 | `journey-ra-sub.json` | PASS |
| RA-VIEW | dashboard, portfolio, pipeline, dossiers, registrations, alerts | 0 botones de mutación y todas las APIs mutantes 403 | `journey-ra-view.json` | PASS |
| QM | dashboard, audit-trail, regulatory, documents, technical-sheets, capa, risks, indicators | bootstrap/productos/dossiers/transición/aprobación interna/sometimiento 403 | `manual-vs-system-role-matrix.json`, bloque QM | PASS |
