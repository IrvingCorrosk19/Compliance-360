# 04 — RBAC Impact

> **Estado: IMPLEMENTED / FUNCTIONALLY CERTIFIED.**

## Modelo aplicado

Workflow V2 usa autorización de servidor sobre cada endpoint. La UI puede mostrar acciones por contexto, pero no concede autoridad. Las rutas V2 reutilizan políticas regulatorias granulares para lectura, actualización, revisión, carga de evidencia, aprobación y override.

## Roles funcionales

La implementación mantiene separados los perfiles Regulatory Specialist, Regulatory Reviewer, Regulatory Approver, Regulatory Submitter, Regulatory Manager, Quality Manager, Tenant Administrator (TAC), Regulatory Administrator (RA-ADM) y Regulatory Viewer.

## Controles verificados

- Viewer obtuvo `403 Forbidden` en todas las familias mutables probadas: metadata, correcciones, evidencia, reopen, override y archive.
- TAC obtuvo `403` al intentar review, approval y override.
- RA-ADM obtuvo `403` al intentar review, approval y override.
- Specialist ejecutó draft, metadata, evidencia y corrección.
- Reviewer solicitó corrección y completó revisión técnica.
- Manager y Quality Manager participaron en aprobaciones de gobierno según las políticas de los endpoints.
- Approver participó como segundo aprobador independiente de override.

La acumulación de UI o visibilidad no sustituye las políticas de backend. Los resultados negativos están versionados en `evidence/viewer-v2-negative-results.json` y `evidence/admin-v2-negative-results.json`.

## SoD relacionado

RBAC limita quién puede invocar cada comando y el dominio agrega controles de identidad: el solicitante no puede aprobar su propia solicitud y la misma persona no puede completar las dos aprobaciones. Los dos niveles se validaron conjuntamente.

## Límite

Esta certificación prueba los roles, usuarios y rutas del escenario local. No afirma haber auditado todas las asignaciones custom posibles ni tokens de un ambiente remoto; esa verificación corresponde al despliegue.
