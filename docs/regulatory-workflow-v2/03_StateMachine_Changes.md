# 03 — State Machine Changes

> **Estado: IMPLEMENTED / CERTIFIED.**

## Estados

Los valores existentes 0–16 se preservaron y la extensión es aditiva:

```text
17 UnderTechnicalReview
18 CorrectionRequested
19 ResponseReady
20 Archived
```

La certificación funcional ejercitó `UnderTechnicalReview`, `CorrectionRequested` y `Archived`. `ResponseReady` existe en el modelo, pero su circuito completo de respuesta/resometimiento no fue parte del escenario certificado y no se declara validado.

## Transiciones certificadas

- `Assembling → UnderTechnicalReview`
- `UnderTechnicalReview → CorrectionRequested`
- `CorrectionRequested → UnderTechnicalReview`
- `UnderTechnicalReview → ReadyForSubmission`
- `Closed → Archived`
- Estado terminal aprobado por reopen → `CorrectionRequested`

Las transiciones inválidas continúan rechazándose por el aggregate. El dossier creado con pack permanece en `Draft`.

## Guards efectivos

1. Tenant y existencia del dossier.
2. Dossier no eliminado para mutaciones ordinarias.
3. `expectedRevision` contra `Revision`.
4. Política de autorización del endpoint.
5. Estado origen permitido.
6. Scope de corrección.
7. Evidencia activa para requirements corregidos.
8. Separación de solicitante y aprobadores en reopen/override.

Una mutación exitosa incrementa la revisión y registra un evento V2. Una revisión obsoleta retorna HTTP `409`, no `412`; este documento refleja el contrato realmente implementado.

## Timeline

Los eventos se numeran secuencialmente por tenant y dossier. La evidencia ejecutada contiene `MetadataUpdated`, `CorrectionRequested`, dos `EvidenceRevisionAdded`, `CorrectionSubmitted`, `TechnicalReviewCompleted` y `DossierArchived`, con razón, actor y correlation ID.

## Archivo

`Archived` es un estado administrativo con soft delete. El archivo no borra la historia: la prueba comparó el timeline antes y después y confirmó que los eventos previos permanecen idénticos y que solo se agrega `DossierArchived`.
