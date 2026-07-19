"""Dashboard API vs DB reconciliation for lab tenant."""
from __future__ import annotations

import json
import urllib.request
from pathlib import Path

import psycopg2

TID = "82af3877-2786-4d39-bce8-c981101c771d"
BASE = "http://localhost:5272/api/v1"
OUT = Path(__file__).resolve().parent / "evidence" / "dashboard_db_reconcile.json"


def login(email: str, password: str) -> str:
    req = urllib.request.Request(
        f"{BASE}/auth/login",
        data=json.dumps({"tenantId": TID, "email": email, "password": password}).encode(),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req) as resp:
        return json.loads(resp.read())["accessToken"]


def get_json(path: str, token: str):
    req = urllib.request.Request(f"{BASE}{path}", headers={"Authorization": f"Bearer {token}"})
    with urllib.request.urlopen(req) as resp:
        return json.loads(resp.read())


def main() -> None:
    token = login("ra.mgr@cert.local", "CertRaPass!2026")
    dash = get_json(f"/tenants/{TID}/regulatory/dashboard", token)

    conn = psycopg2.connect(
        host="localhost", port=5432, dbname="compliance360", user="postgres", password="Panama2020$"
    )
    cur = conn.cursor()

    schema = "compliance360"
    cur.execute(
        f'SELECT COUNT(*) FROM {schema}.medical_device_products WHERE "TenantId"=%s AND "IsDeleted"=false',
        (TID,),
    )
    products = cur.fetchone()[0]

    # Status stored as string enum names (Active, Expiring, Expired, ...)
    cur.execute(
        f'SELECT "Status", COUNT(*) FROM {schema}.sanitary_registrations WHERE "TenantId"=%s GROUP BY 1 ORDER BY 1',
        (TID,),
    )
    status_dist = {str(s): int(c) for s, c in cur.fetchall()}

    cur.execute(
        f'''SELECT COUNT(*) FROM {schema}.sanitary_registrations
           WHERE "TenantId"=%s AND "IsCurrent"=true AND "Status" IN ('Active','Expiring')''',
        (TID,),
    )
    active = cur.fetchone()[0]

    cur.execute(
        f'''SELECT COUNT(*) FROM {schema}.sanitary_registrations
           WHERE "TenantId"=%s AND "IsCurrent"=true AND "Status"='Expiring' ''',
        (TID,),
    )
    expiring = cur.fetchone()[0]

    cur.execute(
        f'''SELECT COUNT(*) FROM {schema}.sanitary_registrations WHERE "TenantId"=%s AND "Status"='Expired' ''',
        (TID,),
    )
    expired = cur.fetchone()[0]

    cur.execute(
        f'SELECT COUNT(*) FROM {schema}.registration_dossiers WHERE "TenantId"=%s AND "IsDeleted"=false',
        (TID,),
    )
    dossiers_total = cur.fetchone()[0]

    cur.execute(
        f'SELECT "Status", COUNT(*) FROM {schema}.registration_dossiers WHERE "TenantId"=%s AND "IsDeleted"=false GROUP BY 1',
        (TID,),
    )
    dossier_status = {str(s): int(c) for s, c in cur.fetchall()}

    conn.close()

    checks = [
        ("productsTotal", dash.get("productsTotal"), products),
        ("registrationsActive", dash.get("registrationsActive"), active),
        ("registrationsExpiring", dash.get("registrationsExpiring"), expiring),
        ("registrationsExpired", dash.get("registrationsExpired"), expired),
    ]

    rows = []
    all_ok = True
    for name, api_v, db_v in checks:
        ok = api_v == db_v
        all_ok = all_ok and ok
        rows.append({"kpi": name, "api": api_v, "db": db_v, "match": ok})

    report = {
        "tenantId": TID,
        "pass": all_ok,
        "checks": rows,
        "apiDashboardScalars": {k: v for k, v in dash.items() if not isinstance(v, dict)},
        "dbStatusDistRegistrations": status_dist,
        "dbDossierStatus": dossier_status,
        "dbDossiersTotal": dossiers_total,
    }
    OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))
    raise SystemExit(0 if all_ok else 1)


if __name__ == "__main__":
    main()
