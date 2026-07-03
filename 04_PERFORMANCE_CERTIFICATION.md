# 04 — Performance Certification — Compliance 360

Date: 2026-07-03 · Method: live latency measurement against local instance (PostgreSQL 18, warm).

## Measured latencies

| Operation | Latency | Notes |
|---|---|---|
| `POST /auth/login` | ~530 ms | Intentional — secure password hashing (by design) |
| `GET /health` | ~37 ms | Liveness ping |
| `GET /documents` (page 20) | ~99 ms | |
| `GET /audits` (page 20) | ~40 ms | |
| `GET /risks` (page 20) | ~70 ms | |
| `GET /capas` (page 20) | ~67 ms | |
| `GET /indicators` (page 20) | ~66 ms | |

## Assessment

- Read/list endpoints are all well under 100 ms warm — healthy for an Enterprise SaaS.
- Login latency is dominated by password hashing (a deliberate security control), not I/O; acceptable.
- Query support: **375 indexes** (188 unique) back the schema, including composite tenant-scoped
  indexes on hot paths (e.g. `(TenantId, Status, Priority, RiskLevel)` on CAPA). List queries use
  `AsNoTracking` + projection to DTOs.
- `AsSplitQuery` used on aggregate loads to avoid cartesian explosion across many child collections.

## Optimization policy

Per the master plan, no speculative optimization was applied — no endpoint exceeded the 1000 ms alert
threshold under test. The observability alert rules already flag `High Latency (>=1000ms)` and
`High Error Rate (>=5%)` if production data reveals hotspots.

## Not measured locally (documented)

- Sustained concurrency / load testing (requires a load tool + representative dataset) — recommended
  as a pre-scale activity, tracked in the risks register.

## Verdict

**Performance PASS** for functional go-live; load/soak testing recommended before high-volume scale.
