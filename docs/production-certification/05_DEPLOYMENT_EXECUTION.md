# Deployment Execution

## Status
**NOT EXECUTED — BLOCKED**

### Blocker
SSH to `164.68.99.83` failed for `root`, `ubuntu`, `deploy`, `admin` (`Permission denied (publickey,password)`). No private key available on the operator workstation; `ssh-agent` empty.

## What completed
1. Pre-commit gates PASS (build/unit/regression/playwright)
2. Certified commit created: `1dd34a6860827fe39e6f3b9c84bcede6c653b054`
3. Push to `origin/master` PASS — remote HEAD matches local HEAD

## Required operator steps (when SSH available)

```bash
cd /opt/compliance360   # or actual compose path
git fetch origin
git checkout 1dd34a6860827fe39e6f3b9c84bcede6c653b054
# verify
test "$(git rev-parse HEAD)" = "1dd34a6860827fe39e6f3b9c84bcede6c653b054"
# backup first (see 03)
docker compose build web worker
docker compose up -d postgres
# migrate only if assessment requires
docker compose up -d web worker
curl -sk https://compliance360.164.68.99.83.nip.io/health/ready
```

Tag images as `compliance360:1dd34a6` for traceability (avoid `latest`-only).
