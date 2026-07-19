# 02 — Preserved Bootstrap Data

**No passwords, hashes, tokens, or connection strings in this document.**

## Tenants preserved

| Id | Name | Slug | Role |
|----|------|------|------|
| `dc7c46ee-cb25-4ed5-b0b4-800788f7f626` | Compliance 360 | compliance360 | Platform / system |
| `82af3877-2786-4d39-bce8-c981101c771d` | Irving Corro S.A | irving-corro-s-a | Principal organization for manual testing |

## Users preserved

| Email | Tenant | Role |
|-------|--------|------|
| `irvingcorrosk19@gmail.com` | Irving Corro S.A | Tenant Administrator |
| `admin@compliance360.local` | Compliance 360 | Platform Administrator |

## Membership / RBAC preserved

- `user_roles` rows for the two preserved users only.
- Full `permissions` catalog.
- Full `roles` definitions (including Regulatory * role templates for assignment).
- Full `role_permissions` grants for system/tenant roles.

## SoD preserved

- `regulatory_sod_settings` for both preserved tenants (defaults: PreventSelfReview/Approval, SeparateApproverAndSubmitter, RequireInternalApprovalBeforeSubmission = true, plus related flags).

## Global / technical config preserved

- Migrations history
- Storage provider configuration
- Notification templates / provider configurations
- Tenant settings, branding, domains, subscriptions, tenant licenses

## Intentionally NOT preserved

- All certification / Playwright / SoD lab users (`ra.*@cert.local`, support operator)
- All RA operational data (products, dossiers, CT/RS, licenses, imports, observations, packs, authorities seeded data, alerts, lab notifications, lab audit ops rows)
