# 00 — Pre-Reset Backup Evidence

**Date:** 2026-07-14 (local)  
**Result:** **PASS**

## Environment (sanitized)

| Field | Value |
|-------|-------|
| Environment | **local-lab** |
| Engine | PostgreSQL 18 (Windows) |
| Host | `localhost` / `::1` |
| Port | `5432` |
| Database | `compliance360` |
| Username | `postgres` |
| Production blocked | **NO** (localhost, non-prod name) |
| Migrations | 20 intact |

## Pre-reset operational snapshot

| Metric | Count |
|--------|------:|
| Tenants | 2 |
| Users | 11 |
| Products (RA) | 486 |
| Dossiers | 97 |
| Sanitary registrations | 362 |
| Manufacturers | 217 |
| Operating licenses | 32 |
| Import jobs | 37 |

## Backup

| Field | Value |
|-------|-------|
| Filename | `compliance360_pre_manual_testing_reset_20260714_203157.dump` |
| Location | `backups/` (repo) |
| Format | PostgreSQL custom (`-F c`) |
| File size | **15,080,873** bytes (> 0) |
| SHA-256 | `69750D06FCF0A6EDFE3D60B1E9A271C83EF44D6F435876FD593455CDE1102C10` |
| Tool | `C:\Program Files\PostgreSQL\18\bin\pg_dump.exe` |
| Exit code | 0 |

**Backup result: PASS** — authorized to continue reset on local-lab only.
