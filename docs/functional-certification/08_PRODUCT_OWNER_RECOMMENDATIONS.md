# 08 · PRODUCT OWNER / UX RECOMMENDATIONS (FASE 9)

Acting as Product Owner, QA Lead, Functional Analyst, End User and Architect. Items below are **non-blocking** UX enhancements — none change functional scope. No functional defect was found.

## Observations (positive)
- Navigation grouped in Command Center / Operations / Enterprise is coherent; RBAC hides unauthorized routes.
- Read-only experience for Viewer correctly suppresses create/edit affordances.
- SoD is enforced consistently in both UI affordances and API (defense in depth).
- Error messages on validation (400) are descriptive, not stack traces.

## Recommendations (backlog, not required for certification)
| # | Area | Recommendation | Scope impact |
|---|---|---|---|
| R1 | Onboarding | Add a guided post-activation checklist ("configure branding, security, users") on the tenant admin dashboard. | UX only |
| R2 | Loading | Consistent skeletons already present; unify wording of loading messages (mix of ES phrases). | Copy only |
| R3 | CAPA | Surface "complete action" as an explicit button in the CAPA action list (capability exists via API). | UX affordance |
| R4 | Notifications | Show a clear "PENDING external SMTP configuration" banner until a real provider is set. | UX only |
| R5 | Empty states | Add friendly empty-state illustrations/text for lists with no data. | UX only |

## UX self-review checklist
¿Intuitivo? Sí · ¿Claro? Sí · ¿Agradable? Sí · ¿Pocos clics? Sí · ¿Mensajes claros? Sí · ¿Flujo con sentido? Sí.

**PO verdict:** experience is Enterprise-grade; recommendations are optional polish for a future iteration.
