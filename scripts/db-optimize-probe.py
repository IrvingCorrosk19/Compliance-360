"""Probe localhost PostgreSQL for optimization opportunities."""
from __future__ import annotations

import json
from pathlib import Path

import psycopg2

OUT = Path(__file__).resolve().parents[1] / "docs" / "manual-testing-reset"
OUT.mkdir(parents=True, exist_ok=True)

conn = psycopg2.connect(
    host="localhost",
    port=5432,
    dbname="compliance360",
    user="postgres",
    password="Panama2020$",
)
cur = conn.cursor()

cur.execute(
    """
    SELECT n.nspname || '.' || c.relname AS tbl,
           pg_size_pretty(pg_total_relation_size(c.oid)) AS total,
           pg_total_relation_size(c.oid) AS bytes
    FROM pg_class c
    JOIN pg_namespace n ON n.oid = c.relnamespace
    WHERE n.nspname = 'compliance360' AND c.relkind = 'r'
    ORDER BY pg_total_relation_size(c.oid) DESC
    LIMIT 30
    """
)
top_tables = [{"table": a, "total": b, "bytes": c} for a, b, c in cur.fetchall()]

cur.execute(
    """
    SELECT schemaname, relname, seq_scan, idx_scan, n_live_tup, n_dead_tup,
           last_vacuum::text, last_analyze::text
    FROM pg_stat_user_tables
    WHERE schemaname = 'compliance360'
    ORDER BY seq_scan DESC NULLS LAST
    LIMIT 25
    """
)
seq = [
    {
        "schema": a,
        "table": b,
        "seq_scan": c,
        "idx_scan": d,
        "live": e,
        "dead": f,
        "last_vacuum": g,
        "last_analyze": h,
    }
    for a, b, c, d, e, f, g, h in cur.fetchall()
]

cur.execute(
    """
    SELECT tablename, indexname, indexdef
    FROM pg_indexes
    WHERE schemaname = 'compliance360'
      AND tablename IN (
        'medical_device_products','registration_dossiers','sanitary_registrations',
        'manufacturer_profiles','manufacturer_certificates','operating_licenses',
        'dossier_requirements','authority_observations','users','roles','audit_logs',
        'regutrack_import_rows','regutrack_import_jobs','regulatory_authorities',
        'regulatory_requirement_packs'
      )
    ORDER BY tablename, indexname
    """
)
indexes = [{"table": a, "index": b, "def": c} for a, b, c in cur.fetchall()]

cur.execute(
    """
    SELECT name, setting, unit
    FROM pg_settings
    WHERE name IN (
      'shared_buffers','work_mem','effective_cache_size','random_page_cost',
      'maintenance_work_mem','max_parallel_workers_per_gather','max_connections',
      'checkpoint_completion_target','wal_buffers','default_statistics_target'
    )
    ORDER BY name
    """
)
settings = [{"name": a, "setting": b, "unit": c} for a, b, c in cur.fetchall()]

# bloat / unused indexes hint
cur.execute(
    """
    SELECT schemaname, relname AS table, indexrelname AS index,
           idx_scan, pg_size_pretty(pg_relation_size(indexrelid)) AS size
    FROM pg_stat_user_indexes
    WHERE schemaname = 'compliance360' AND idx_scan = 0
    ORDER BY pg_relation_size(indexrelid) DESC
    LIMIT 20
    """
)
unused = [{"schema": a, "table": b, "index": c, "scans": d, "size": e} for a, b, c, d, e in cur.fetchall()]

report = {
    "top_tables": top_tables,
    "seq_scans": seq,
    "indexes_hot": indexes,
    "settings": settings,
    "unused_indexes_sample": unused,
}
(OUT / "_db_optimize_probe.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps({"top5": top_tables[:5], "seq_top10": seq[:10], "settings": settings, "index_count_hot": len(indexes)}, indent=2))
conn.close()
