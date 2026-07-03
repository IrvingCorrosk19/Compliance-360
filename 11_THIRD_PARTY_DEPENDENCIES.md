# 11 — Third-Party Dependencies — Compliance 360

Date: 2026-07-03 · Status legend: ✅ working locally · ⏳ PENDING THIRD-PARTY CONFIGURATION.

All external integrations are implemented with provider abstractions and health checks; the following
require **real credentials / external infrastructure** that are not available in the local environment.
None are code defects — each is a configuration/onboarding task.

## Storage

| Provider | Status | What is needed |
|---|---|---|
| Local filesystem | ✅ | Works (`Storage:Provider=Local`, `RootPath=storage`); health check green |
| Azure Blob | ⏳ | Connection string / account key + container |
| AWS S3 | ⏳ | Access key, secret, region, bucket |
| MinIO | ⏳ | Endpoint, access/secret key, bucket |

## Notifications / Email

| Provider | Status | What is needed |
|---|---|---|
| SMTP (generic) | ⏳ | `Host`, `Username`, `Secret`, `FromAddress` (all empty in config) |
| Gmail SMTP | ⏳ | App password + from address |
| Microsoft 365 / Exchange Online | ⏳ | Mailbox credentials + from address |
| SendGrid / Mailgun / Resend / Amazon SES | ⏳ | API key (`Secret`) + verified sender/domain |

Per-provider health checks (`notification-smtp`, `-sendgrid`, `-mailgun`, `-resend`) plus queue and
dead-letter checks are registered and report ready; live send requires credentials above.

## Identity federation & signing

| Capability | Status | What is needed |
|---|---|---|
| OIDC / SAML SSO | ⏳ | IdP metadata, client id/secret, redirect URIs |
| LDAP / Active Directory | ⏳ | Directory endpoint + bind credentials |
| Digital signature | ⏳ | Certificate / signing service credentials |
| External AI | ⏳ | Provider endpoint + API key |

## How to configure (pattern)

Set values via environment variables / user secrets (never in `appsettings.json`). Example (SMTP):
`Notifications__Providers__Smtp__Host`, `__Username`, `__Secret`, `__FromAddress`. After configuring,
re-run the matching health check (`/health/ready`) and a live send/connection test.

## Pending tests

Live email delivery, cloud storage upload/download, SSO login round-trip, LDAP bind, and signature
verification — all blocked only on credentials/infrastructure.
