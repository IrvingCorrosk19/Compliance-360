# 09 — Implementation Impact

| Área | Cambio |
|------|--------|
| Domain | `ApprovedForSubmission`; `InternallyApprovedByUserId`; `LastStatusChangedByUserId`; `RegulatorySoDSettings` |
| Permissions | +4 (`REVIEW`, `APPROVE_FOR_SUBMISSION`, `SOD.MANAGE`, `SOD.EMERGENCY_OVERRIDE`); APPROVE semántica = externa |
| Roles | +3 (Manager, Approver, Submitter); Admin/TAC sin operate APPROVE/SUBMIT; Specialist sin SUBMIT; Reviewer sin APPROVE externo |
| Policies | `Regulatory.Submit` = SUBMIT only; nuevas ApproveForSubmission / Review / SoD* |
| API | `POST .../approve-for-submission`; `SubmitDossierAsync`; `GET/PUT .../sod-settings`; transition bloquea terminales |
| Backend SoD | `RegulatorySoDGate` (RBAC claim + recurso + estado + política) |
| UI | Pendiente contextual dashboards por rol (fase UI) — seguridad no depende de UI |
| Tests | Domain + SoD gate unitarios |
| Migración | `AddRegulatorySoDControls` |
| Cert E2E | **NO GO** hasta role certification con nuevos roles y seed RBAC en lab |
