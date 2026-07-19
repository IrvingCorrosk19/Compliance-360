# 18 — RBAC Persisted Grant Evidence

**Fuente:** PostgreSQL `compliance360.roles` / `role_permissions` / `permissions`  
**Tenant:** `82af3877-2786-4d39-bce8-c981101c771d`  
**Tras:** migración `AddRegulatorySoDControls` + restart Web (`EnsureTenantRolesAsync`).

## Migración

| Check | Resultado |
|-------|-----------|
| MigrationId aplicada | `20260714234240_AddRegulatorySoDControls` |
| Tabla `regulatory_sod_settings` | EXISTS · UNIQUE(`TenantId`) |
| Columnas dossier | `InternallyApprovedByUserId`, `InternallyApprovedAtUtc` |
| Columna requisitos | `LastStatusChangedByUserId` |
| Datos preservados | dossiers=64+, regs=56+, products=149+ (pre-run) |
| SoD defaults | PreventSelfReview/Approval/SeparateApproverAndSubmitter/RequireInternalApproval = **true** |

## Matriz grants REGULATORY.* (persistidos)

| Rol | Permission clave | Persistido | Esperado | PASS/FAIL |
|-----|------------------|------------|----------|-----------|
| Regulatory Administrator | CONFIGURE, SOD.MANAGE, READs | Sí | Sin SUBMIT/APPROVE* | PASS |
| Regulatory Administrator | APPROVE_FOR_SUBMISSION | No | Ausente | PASS |
| Regulatory Administrator | SUBMIT | No | Ausente | PASS |
| Regulatory Administrator | APPROVE (externo) | No | Ausente | PASS |
| Regulatory Manager | APPROVE + SOD.EMERGENCY_OVERRIDE | Sí | Sí | PASS |
| Regulatory Specialist | CREATE/UPDATE/REQ/OBS | Sí | Sin SUBMIT/APPROVE* | PASS |
| Regulatory Reviewer | REVIEW + REQ.MANAGE | Sí | Sin SUBMIT/APPROVE* | PASS |
| Regulatory Approver | APPROVE_FOR_SUBMISSION | Sí | Sin SUBMIT | PASS |
| Regulatory Submitter | SUBMIT | Sí | Sin APPROVE_FOR_SUBMISSION | PASS |
| Regulatory Viewer | READ* | Sí | Solo lectura | PASS |
| Tenant Administrator | CONFIGURE + SOD.MANAGE + READ* | Sí | Sin operate | PASS |
| Quality Manager | APPROVE + REGISTRATION.MANAGE | Sí | Sin preparar | PASS |

Evidencia dump: script `_db_dump_grants.py` + JWT claims en `evidence/sod-api-results.json`.
