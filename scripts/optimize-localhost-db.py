"""Optimize localhost PostgreSQL for Compliance 360 lab.

Applies VACUUM ANALYZE on compliance360 schema and safe session/system
tunables for SSD localhost (does not drop data or roles).
"""
from __future__ import annotations

import argparse
import json
from datetime import datetime, timezone
from pathlib import Path

import psycopg2

DEFAULT_DSN = "host=localhost port=5432 dbname=compliance360 user=postgres password=Panama2020$"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dsn", default=DEFAULT_DSN)
    parser.add_argument("--alter-system", action="store_true", help="Persist SSD-friendly settings via ALTER SYSTEM")
    args = parser.parse_args()

    conn = psycopg2.connect(args.dsn)
    conn.autocommit = True
    cur = conn.cursor()

    cur.execute("SELECT current_database(), inet_server_addr(), version()")
    db, addr, version = cur.fetchone()
    if db != "compliance360":
        raise SystemExit(f"Refusing to optimize unexpected database: {db}")

    report: dict = {
        "at": datetime.now(timezone.utc).isoformat(),
        "database": db,
        "server_addr": str(addr),
        "version": version.split(",")[0],
        "actions": [],
    }

    if args.alter_system:
        # Lab SSD defaults; reload required for some, restart for shared_buffers.
        tunables = {
            "random_page_cost": "1.1",
            "effective_io_concurrency": "200",
            "checkpoint_completion_target": "0.9",
            "default_statistics_target": "200",
            "maintenance_work_mem": "256MB",
            "work_mem": "16MB",
            "effective_cache_size": "2GB",
        }
        for name, value in tunables.items():
            cur.execute(f"ALTER SYSTEM SET {name} = %s", (value,))
            report["actions"].append(f"ALTER SYSTEM SET {name}={value}")
        cur.execute("SELECT pg_reload_conf()")
        report["actions"].append("pg_reload_conf()")

    cur.execute(
        """
        SELECT format('%I.%I', n.nspname, c.relname)
        FROM pg_class c
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = 'compliance360' AND c.relkind = 'r'
        ORDER BY c.relname
        """
    )
    tables = [r[0] for r in cur.fetchall()]
    for table in tables:
        cur.execute(f"VACUUM (ANALYZE) {table}")
        report["actions"].append(f"VACUUM ANALYZE {table}")

    # Refresh planner stats summary
    cur.execute(
        """
        SELECT COUNT(*) FILTER (WHERE last_analyze IS NOT NULL) AS analyzed,
               COUNT(*) AS total
        FROM pg_stat_user_tables
        WHERE schemaname = 'compliance360'
        """
    )
    analyzed, total = cur.fetchone()
    report["tables_analyzed"] = analyzed
    report["tables_total"] = total

    cur.execute(
        """
        SELECT indexname
        FROM pg_indexes
        WHERE schemaname = 'compliance360'
          AND (
            indexname ILIKE '%IsDeleted%'
            OR indexname ILIKE '%ExpiresOn%'
            OR indexname ILIKE '%ManufacturerId%'
            OR indexname ILIKE '%CreatedAtUtc%'
            OR indexname ILIKE '%IsCritical%'
            OR indexname ILIKE '%IsActive%'
          )
        ORDER BY indexname
        """
    )
    report["new_style_indexes"] = [r[0] for r in cur.fetchall()]

    out = Path(__file__).resolve().parents[1] / "docs" / "manual-testing-reset" / "_db_optimize_report.json"
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps({
        "ok": True,
        "tables_analyzed": analyzed,
        "tables_total": total,
        "alter_system": args.alter_system,
        "index_hits": len(report["new_style_indexes"]),
        "report": str(out),
    }, indent=2))
    conn.close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
