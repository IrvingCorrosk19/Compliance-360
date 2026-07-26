# 06 — Notification / Alert Remediation

**Status:** pending evidence from harness

## Scope

- Mailpit probe at `http://127.0.0.1:8025/api/v1/messages`
- Alert-center provider sandbox-send when available
- Otherwise record **BLOCKED** with label **EXTERNAL CONFIG** (never SKIPPED)

## Evidence

- Harness test: `NOTIFY-SANDBOX`
- Alert occurrence acknowledge/resolve/escalate: see RCA §7
