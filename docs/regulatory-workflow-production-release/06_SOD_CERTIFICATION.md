# 06 — Certificación SoD

## Controles implementados

- El creador/owner puede ser bloqueado para revisar requirements (`PreventSelfReview`).
- El creador/owner o reviewer puede ser bloqueado para aprobar su propio expediente (`PreventSelfApproval`).
- El aprobador interno puede ser separado del submitter y resubmitter (`SeparateApproverAndSubmitter`).
- Waivers críticos y decisiones externas aplican gates configurables.
- Emergency override depende de habilitación, motivo y auditoría; V2 modela override como solicitud consumible.
- Reopen y override requieren solicitante distinto y dos aprobadores distintos; una identidad no ocupa ambas etapas.

## Evidencia

- `workflow-v2-final.json` registra PASS local para reopen y override con separación del solicitante y dos aprobadores.
- `viewer-v2-negative-results.json` y `admin-v2-negative-results.json` demuestran denegaciones del backend.
- `RegulatorySoDGateTests.cs`, `RegulatoryWorkflowV2DomainTests.cs` y los escenarios multirol forman parte del corte final **282/282 .NET** y **72/72 Playwright productivo**.

## Verificación productiva

Los nueve roles, denegaciones directas de API, aislamiento de tenant, self-review/self-approval, aprobación, submission, reopen y override fueron ejecutados contra el VPS. Los logs finales registraron 0 HTTP 5xx y las denegaciones esperadas como 403/409.

## Veredicto

**PASS — SoD certificado en el corte productivo final.**
