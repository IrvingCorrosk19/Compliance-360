# 10 — Database Certification — Compliance 360

Date: 2026-07-03 · Engine: PostgreSQL 18 (localhost) · Schema: `compliance360`.

## Structure (live counts)

| Metric | Value |
|---|---|
| Applied migrations | **15** (Initial → AddTenantAdministrationOmega) |
| Tables | **130** |
| Foreign keys | **92** |
| Indexes | **375** (of which **188 unique** — uniqueness enforced via unique indexes) |
| Tables without `TenantId` | **2** (`permissions`, `tenants` — correct, global tables) |

## Integrity

- All migrations present and applied cleanly (`__EFMigrationsHistory`).
- Referential integrity via 92 FKs; uniqueness (e.g. `(TenantId, Code)` per module) via unique indexes.
- Client-generated Guid keys now declared `ValueGeneratedNever` (aligns EF with domain; fixed the
  add-child-on-update defect) — **no schema change** (column type remains `uuid`).
- Timestamps stored as `timestamptz`; application now normalizes all `DateTimeOffset` to UTC on write.

## Backup / restore

- `pg_dump` of the `compliance360` schema succeeds (**~3.3 MB** logical dump produced during audit).
- Restore path: standard `psql -f dump.sql` (schema-scoped). Point-in-time recovery is a deployment/
  infrastructure concern (documented in Go-Live checklist).

## Audit trail

- `audit_logs` populated by both an explicit service path and a `SaveChanges` interceptor that records
  Added/Modified/Deleted entities (excluding `AuditLog` itself) with before/after snapshots. Default
  retention **2555 days (7 years)**.

## Verdict

Schema is consistent, indexed, referentially sound, and backup-capable. **Database PASS.** (Automated
scheduled backups + PITR to be configured at the infrastructure layer for go-live.)
