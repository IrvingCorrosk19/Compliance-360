# 22 — Regulatory Audit Event Certification

**DB:** `audit_logs.Action` (varchar)

| Evento | Evidencia DB / código | PASS |
|--------|----------------------|------|
| RegulatorySoDActionDenied | 3+ filas en lab | PASS |
| RegulatoryInternalApprovalGranted | 2+ filas; `InternallyApprovedByUserId` poblado | PASS |
| RegulatoryRequirementUpdated | 352 (incluye Accept/Reject) | PASS |
| RegulatoryDossierTransitioned | Submitted path | PASS |
| RegulatoryObservationOpened / Responded | Lab E2E | PASS |
| RegulatoryRegistrationApproved | External CT/RS | PASS |

Human-readable nombres de Action (no solo “Entity Updated”).  
Campos: Tenant vía contexto, EntityName, EntityId, OccurredAtUtc, Action.

Nota: nomenclatura request ReviewRequested / etc. se materializa como Actions Regulatory* + history dossier (`InternalApproval`, `StatusTransition`). Suficiente para trazabilidad SoD.
