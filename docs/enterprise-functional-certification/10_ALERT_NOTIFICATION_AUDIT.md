# 10 — Alert / Notification Audit

## RA Alerts
- GET /alert-settings PASS (admin)
- GET /alerts/evaluate PASS

## Alert Center
- GET /api/v2/.../alert-center/inbox PASS (200, empty for actors tested)
- Wrong path /occurrences → 404 (test error; not product missing inbox)

## Notifications
- SMTP configured → localhost:1025
- SendGrid/Mailgun/Resend **Degraded** (missing secrets)
- Worker running and required for bootstrap health
- Classification: **CONFIGURABLE / partially RULE-BASED**; email delivery **FUNCTIONALLY READY — EXTERNAL CONFIGURATION REQUIRED**
