# Backup Verification

## Status
**NOT EXECUTED** — VPS SSH access unavailable from the certification workstation.

## Required before any deploy (operator)

```bash
# On VPS (example — use real compose project dir)
TS=$(date -u +%Y%m%d_%H%M%S)
mkdir -p /opt/backups
docker exec compliance360_postgres pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -Fc -f /tmp/c360-$TS.dump
docker cp compliance360_postgres:/tmp/c360-$TS.dump /opt/backups/compliance360-$TS.dump
sha256sum /opt/backups/compliance360-$TS.dump | tee /opt/backups/compliance360-$TS.sha256
ls -lh /opt/backups/compliance360-$TS.dump
```

Also back up (outside Git): `.env`, nginx site config, `compliance360_storage` volume snapshot if feasible.

Historical backup path referenced in prior runbook: `/opt/backups/compliance360-20260718_085002` — **must be re-verified** before cutover; do not assume freshness.
