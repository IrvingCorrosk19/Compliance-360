# 05 — SoD Impact

> **Estado: IMPLEMENTED / CERTIFIED FOR TESTED CONTROLS.**

## Controles efectivos

### Preparación y revisión

El Specialist prepara y atiende la corrección; el Reviewer solicita el scope y completa la revisión técnica. Las políticas de API separan ambas capacidades.

### Corrección

- El Reviewer define scope explícito por requirements, fields y documents.
- El Specialist no puede persistir evidencia fuera de scope.
- La corrección no se completa sin evidencia activa para cada requirement incluido.
- El Reviewer, no el Specialist, cierra la corrección al finalizar la revisión.

### Reapertura

- El solicitante no puede aprobar su propia solicitud.
- La primera y segunda aprobación deben pertenecer a usuarios distintos.
- Dos aprobaciones son obligatorias antes de ejecutar.
- El uso de una revisión vigente evita que una decisión obsoleta gane una carrera.

### Override

- El override es una solicitud, no una razón textual que habilita bypass inmediato.
- El solicitante no puede aprobar.
- Se exigen dos aprobadores distintos.
- La acción aprobada debe coincidir al consumirla.
- El consumo es de una sola vez.

### Administración

Las pruebas negativas confirmaron que TAC y RA-ADM no pueden revisar, aprobar ni solicitar override por el solo hecho de administrar.

## Auditoría

Cada operación V2 aceptada genera un evento secuencial append-only con actor, motivo y correlación. Solicitudes, aprobaciones, ejecución, consumo y archivo forman parte de la trazabilidad persistida.

## Resultado

Los controles ejecutados pasaron en pruebas de dominio y en el escenario Playwright multirol. No se declara implementado step-up authentication ni expiración temporal de solicitudes, porque no forman parte del comportamiento certificado de esta entrega.
