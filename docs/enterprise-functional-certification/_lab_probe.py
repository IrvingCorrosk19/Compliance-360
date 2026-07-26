#!/usr/bin/env python3
"""Lab probe for enterprise functional certification — read-only."""
import json
import os
import sys
import psycopg2

CONN = dict(host="localhost", port=5432, dbname="compliance360", user="postgres", password=os.environ.get("COMPLIANCE360_PGPASSWORD") or os.environ.get("PGPASSWORD") or "")


def main():
    action = sys.argv[1] if len(sys.argv) > 1 else "summary"
    if not CONN["password"]:
        raise SystemExit("Set COMPLIANCE360_PGPASSWORD or PGPASSWORD for lab DB access.")
    conn = psycopg2.connect(**CONN)
    cur = conn.cursor()
    if action == "summary":
        cur.execute('SELECT "Id", "Name" FROM compliance360.tenants')
        tenants = [{"id": str(r[0]), "name": r[1]} for r in cur.fetchall()]
        cur.execute(
            'SELECT "Email", "FullName", "Status"::text FROM compliance360.users '
            "WHERE \"Email\" LIKE %s OR \"Email\" = %s ORDER BY \"Email\"",
            ("%@cert.local", "admin@compliance360.local"),
        )
        users = [{"email": r[0], "name": r[1], "status": r[2]} for r in cur.fetchall()]
        cur.execute(
            'SELECT "InstanceName", "Status", "LastSeenAtUtc" '
            "FROM compliance360.notification_worker_heartbeats "
            'ORDER BY "LastSeenAtUtc" DESC LIMIT 3'
        )
        hb = [{"instance": r[0], "status": r[1], "lastSeen": r[2].isoformat()} for r in cur.fetchall()]
        cur.execute('SELECT count(*) FROM compliance360.registration_dossiers')
        dossiers = cur.fetchone()[0]
        cur.execute('SELECT count(*) FROM compliance360.medical_device_products')
        products = cur.fetchone()[0]
        print(json.dumps({"tenants": tenants, "users": users, "heartbeats": hb, "dossiers": dossiers, "products": products}, indent=2))
    elif action == "sql":
        sql = sys.argv[2]
        cur.execute(sql)
        cols = [d[0] for d in cur.description] if cur.description else []
        rows = cur.fetchall()
        print(json.dumps({"cols": cols, "rows": [[str(x) if x is not None else None for x in r] for r in rows]}, indent=2, default=str))
    conn.close()


if __name__ == "__main__":
    main()
