# Rollback Plan

## READY (documented) — restore not rehearsed in this session

### Triggers
P0/P1, auth down, DB corruption, migration failure, cross-tenant leak, continuous 500s, critical workflow unavailable.

### Steps
1. Stop new traffic (nginx).
2. Capture logs (`docker compose logs --tail=500`).
3. Redeploy previous known-good image/commit (`afd491a` prior HEAD).
4. If schema changed: restore `/opt/backups/compliance360-<timestamp>.dump` with `pg_restore` into a **new** DB name first when possible; swap only after validation.
5. **Never** DROP DATABASE / EnsureDeleted on production.
6. Verify `/health/ready`, login, one dossier read, one document read.
7. Reopen traffic.

### Previous remote HEAD before this release
`afd491a` — `Fix evidence remove jsonb payload and add ra.spec functional coverage.`
