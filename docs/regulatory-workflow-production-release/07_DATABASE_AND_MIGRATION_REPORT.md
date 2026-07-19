# 07 — Base de datos y migraciones

## Migraciones relevantes

- `20260718115954_AddRegulatoryWorkflowV2ControlledFlexibility`: `Revision`, timeline V2, corrections/scope, evidence revisions, reopen, override y aprobaciones.
- `20260718142030_HardenRegulatoryWorkflowV2Governance`: endurecimiento de unicidad e inmutabilidad.

## Restricciones añadidas

- Única reopen request activa (`Pending`/`Approved`) por tenant+dossier.
- Única override request activa (`Pending`/`Approved`) por tenant+dossier.
- Única correction request activa (`Open`/`ResponseSubmitted`) por tenant+dossier.
- Única evidence revision `IsCurrent = TRUE` por tenant+requirement.
- Triggers `trg_dossier_change_events_append_only` y `trg_dossier_history_events_append_only` rechazan UPDATE/DELETE mediante `prevent_regulatory_history_mutation()`.

## Estrategia de despliegue

1. Confirmar backup `/opt/backups/compliance360-20260718_085002`.
2. Capturar versión/esquema actual y migrations aplicadas.
3. Desplegar imagen compatible.
4. Aplicar migraciones una sola vez con log.
5. Verificar índices, triggers y migration history.
6. Ejecutar `/health/ready` y pruebas de lectura/mutación controlada.

## Estado

La migración existe en código y el build Release actual compila con 0 errores/0 warnings. **No hay evidencia final en este paquete de que la migración `20260718142030` haya sido aplicada en VPS.**

## Rollback

El rollback preferido es restaurar backup y versión de aplicación coordinadamente. Ejecutar `Down` elimina los triggers/índices del endurecimiento y no debe usarse como sustituto de una restauración evaluada cuando ya existen datos regulatorios.
