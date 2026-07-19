#!/usr/bin/env python3
"""
Controlled operational data reset for Compliance 360 manual testing.

Safety:
- Blocks non-localhost targets
- Requires --confirm-lab-reset
- Preserves migrations, RBAC catalog, tenants, primary admins, SoD settings
- Does NOT DROP DATABASE / EnsureDeleted

Usage:
  python scripts/reset-manual-testing-data.py --confirm-lab-reset
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import shutil
import sys
from datetime import datetime, timezone
from pathlib import Path

import psycopg2
import psycopg2.extras

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "manual-testing-reset"
SCHEMA = "compliance360"

# Structural preserve
KEEP_TENANT_IDS = {
    "dc7c46ee-cb25-4ed5-b0b4-800788f7f626",  # Compliance 360 platform
    "82af3877-2786-4d39-bce8-c981101c771d",  # Irving Corro S.A (principal org)
}
KEEP_USER_EMAILS = {
    "irvingcorrosk19@gmail.com",  # principal Tenant Administrator
    "admin@compliance360.local",  # Platform Administrator bootstrap
}

# Full-table preserve (structure / catalogs) — never DELETE ALL
PRESERVE_TABLES = {
    "__EFMigrationsHistory",  # public
    "permissions",
    "roles",
    "role_permissions",
    "tenants",
    "users",
    "user_roles",
    "storage_provider_configurations",
    "notification_templates",
    "notification_provider_configurations",
    "regulatory_sod_settings",
    "tenant_settings",
    "tenant_branding",
    "tenant_domains",
    "tenant_licenses",
    "subscriptions",
}

# Identity sidecar tables — never bulk-wiped; cleaned only for deleted users
IDENTITY_SIDECARS = {
    "refresh_tokens",
    "user_sessions",
    "password_history",
    "mfa_configurations",
}

# Tables to wipe completely if they exist (lab operational / children)
# Prefer dynamic: all TenantId tables except preserve list, plus user-adjacent ops.


def connect():
    cs = os.environ.get("ConnectionStrings__Compliance360")
    if cs:
        # parse
        kv = {}
        for part in cs.split(";"):
            if "=" in part:
                k, v = part.split("=", 1)
                kv[k.strip().lower()] = v.strip()
        host = kv.get("host", "localhost")
        port = int(kv.get("port", "5432"))
        db = kv.get("database", "compliance360")
        user = kv.get("username", "postgres")
        pwd = kv.get("password")
    else:
        host, port, db, user, pwd = "localhost", 5432, "compliance360", "postgres", "Panama2020$"

    if host.lower() not in {"localhost", "127.0.0.1", "::1"}:
        raise SystemExit(f"RESET BLOCKED — PRODUCTION DATABASE DETECTED (host={host})")
    if any(x in db.lower() for x in ("prod", "production", "live")):
        raise SystemExit(f"RESET BLOCKED — PRODUCTION DATABASE DETECTED (database={db})")

    return psycopg2.connect(host=host, port=port, dbname=db, user=user, password=pwd), {
        "host": host,
        "port": port,
        "database": db,
        "username": user,
    }


def q(cur, sql, params=None):
    cur.execute(sql, params or ())
    try:
        return cur.fetchall()
    except psycopg2.ProgrammingError:
        return None


def count_map(cur) -> dict:
    metrics = {}
    pairs = [
        ("tenants", f'SELECT COUNT(*) FROM {SCHEMA}.tenants'),
        ("users", f'SELECT COUNT(*) FROM {SCHEMA}.users'),
        ("products_ra", f'SELECT COUNT(*) FROM {SCHEMA}.medical_device_products'),
        ("manufacturers", f'SELECT COUNT(*) FROM {SCHEMA}.manufacturer_profiles'),
        ("dossiers", f'SELECT COUNT(*) FROM {SCHEMA}.registration_dossiers'),
        ("requirements", f'SELECT COUNT(*) FROM {SCHEMA}.dossier_requirements'),
        ("registrations", f'SELECT COUNT(*) FROM {SCHEMA}.sanitary_registrations'),
        ("licenses", f'SELECT COUNT(*) FROM {SCHEMA}.operating_licenses'),
        ("observations", f'SELECT COUNT(*) FROM {SCHEMA}.authority_observations'),
        ("import_jobs", f'SELECT COUNT(*) FROM {SCHEMA}.regutrack_import_jobs'),
        ("import_rows", f'SELECT COUNT(*) FROM {SCHEMA}.regutrack_import_rows'),
        ("alerts", f'SELECT COUNT(*) FROM {SCHEMA}.regulatory_alert_logs'),
        ("notifications", f'SELECT COUNT(*) FROM {SCHEMA}.notification_messages'),
        ("audit_logs", f'SELECT COUNT(*) FROM {SCHEMA}.audit_logs'),
        ("roles", f'SELECT COUNT(*) FROM {SCHEMA}.roles'),
        ("permissions", f'SELECT COUNT(*) FROM {SCHEMA}.permissions'),
        ("sod_settings", f'SELECT COUNT(*) FROM {SCHEMA}.regulatory_sod_settings'),
        ("migrations", 'SELECT COUNT(*) FROM "__EFMigrationsHistory"'),
    ]
    for name, sql in pairs:
        try:
            cur.execute(sql)
            metrics[name] = cur.fetchone()[0]
        except Exception:
            cur.connection.rollback()
            metrics[name] = None
    return metrics


def list_tenant_tables(cur) -> list[str]:
    cur.execute(
        """
        SELECT c.table_name
        FROM information_schema.columns c
        JOIN information_schema.tables t
          ON t.table_schema=c.table_schema AND t.table_name=c.table_name AND t.table_type='BASE TABLE'
        WHERE c.table_schema=%s AND c.column_name='TenantId'
        ORDER BY 1
        """,
        (SCHEMA,),
    )
    return [r[0] for r in cur.fetchall()]


def table_exists(cur, name: str) -> bool:
    cur.execute(
        """
        SELECT 1 FROM information_schema.tables
        WHERE table_schema=%s AND table_name=%s
        """,
        (SCHEMA, name),
    )
    return cur.fetchone() is not None


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--confirm-lab-reset", action="store_true", required=True)
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()

    OUT.mkdir(parents=True, exist_ok=True)
    conn, target = connect()
    conn.autocommit = False
    cur = conn.cursor()

    print("TARGET", json.dumps(target))
    before = count_map(cur)
    print("BEFORE", json.dumps(before))

    # Validate preserved bootstrap exists
    cur.execute(
        f'SELECT "Id","Email","Status"::text,"TenantId" FROM {SCHEMA}.users WHERE lower("Email") = ANY(%s)',
        ([e.lower() for e in KEEP_USER_EMAILS],),
    )
    keep_users = cur.fetchall()
    if len(keep_users) < 1:
        conn.rollback()
        raise SystemExit("ABORT: primary administrator user not found")
    keep_user_ids = {str(r[0]) for r in keep_users}
    print("KEEP_USERS", [{"id": str(a), "email": b, "status": c, "tenant": str(d)} for a, b, c, d in keep_users])

    cur.execute(f'SELECT "Id","Name" FROM {SCHEMA}.tenants WHERE "Id" = ANY(%s::uuid[])', (list(KEEP_TENANT_IDS),))
    keep_tenants = cur.fetchall()
    if len(keep_tenants) < 1:
        conn.rollback()
        raise SystemExit("ABORT: principal tenant not found")
    print("KEEP_TENANTS", [{"id": str(a), "name": b} for a, b in keep_tenants])

    # Admin capability: has Tenant Administrator or Platform Administrator
    cur.execute(
        f"""
        SELECT u."Email", r."Name"
        FROM {SCHEMA}.users u
        JOIN {SCHEMA}.user_roles ur ON ur."UserId"=u."Id"
        JOIN {SCHEMA}.roles r ON r."Id"=ur."RoleId"
        WHERE u."Id" = ANY(%s::uuid[])
        """,
        (list(keep_user_ids),),
    )
    admin_roles = cur.fetchall()
    role_names = {b for _, b in admin_roles}
    if not any(x in role_names for x in ("Tenant Administrator", "Platform Administrator", "Owner")):
        conn.rollback()
        raise SystemExit(f"ABORT: preserved user lacks admin role; found={role_names}")
    print("ADMIN_ROLES_OK", sorted(role_names))
    print("PRESERVED ADMIN CAN REBUILD THE TENANT FROM UI = YES")

    if args.dry_run:
        conn.rollback()
        print("DRY RUN complete — no changes")
        return 0

    tenant_tables = list_tenant_tables(cur)
    wipe_tables = [
        t
        for t in tenant_tables
        if t not in PRESERVE_TABLES and t not in IDENTITY_SIDECARS
    ]

    # Prefer child-first: disable FK checks in session for lab delete
    cur.execute("SET LOCAL session_replication_role = replica")

    deleted = {}

    # Null CompanyId on preserved users so company wipe cannot orphan them
    if table_exists(cur, "users"):
        cur.execute(
            f'UPDATE {SCHEMA}.users SET "CompanyId" = NULL WHERE "Id" = ANY(%s::uuid[])',
            (list(keep_user_ids),),
        )

    # 1) Delete operational tenant-scoped rows for ALL tenants (clean install), except preserve tables
    for table in sorted(wipe_tables, reverse=True):
        try:
            cur.execute(f'DELETE FROM {SCHEMA}."{table}"')
            deleted[table] = cur.rowcount
        except Exception as ex:
            conn.rollback()
            raise SystemExit(f"FAIL deleting {table}: {ex}") from ex

    # 2) Delete non-preserved users and their memberships/tokens
    cur.execute(
        f"""
        SELECT "Id" FROM {SCHEMA}.users
        WHERE lower("Email") <> ALL(%s)
        """,
        ([e.lower() for e in KEEP_USER_EMAILS],),
    )
    drop_user_ids = [str(r[0]) for r in cur.fetchall()]
    if drop_user_ids:
        for table, col in [
            ("user_roles", "UserId"),
            ("refresh_tokens", "UserId"),
            ("user_sessions", "UserId"),
            ("password_history", "UserId"),
            ("mfa_configurations", "UserId"),
        ]:
            if table_exists(cur, table):
                cur.execute(
                    f'DELETE FROM {SCHEMA}."{table}" WHERE "{col}" = ANY(%s::uuid[])',
                    (drop_user_ids,),
                )
                deleted[f"{table}:dropped_users"] = cur.rowcount
        cur.execute(
            f'DELETE FROM {SCHEMA}.users WHERE "Id" = ANY(%s::uuid[])',
            (drop_user_ids,),
        )
        deleted["users"] = cur.rowcount

    # 3) Ensure no extra tenants
    cur.execute(
        f'DELETE FROM {SCHEMA}.tenants WHERE "Id" <> ALL(%s::uuid[])',
        (list(KEEP_TENANT_IDS),),
    )
    deleted["tenants_extra"] = cur.rowcount

    # 4) Ensure SoD settings exist for kept tenants
    for tid in KEEP_TENANT_IDS:
        cur.execute(
            f'SELECT 1 FROM {SCHEMA}.regulatory_sod_settings WHERE "TenantId"=%s::uuid',
            (tid,),
        )
        if cur.fetchone() is None:
            cur.execute(
                f"""
                INSERT INTO {SCHEMA}.regulatory_sod_settings
                ("Id","TenantId",
                 "PreventSelfReview","PreventSelfApproval","SeparateApproverAndSubmitter",
                 "SeparateDocumentUploaderAndReviewer","RequireSecondApprovalForCriticalWaiver",
                 "RequireApprovalForCriticalityChange","RequireApprovalForExternalDecisionRecording",
                 "AllowEmergencyOverride","EmergencyOverrideRequiresReason",
                 "EmergencyOverrideRequiresSecondaryReview","RequireInternalApprovalBeforeSubmission",
                 "CreatedAtUtc","UpdatedAtUtc")
                VALUES (gen_random_uuid(), %s::uuid,
                        true, true, true,
                        true, true,
                        true, true,
                        true, true,
                        true, true,
                        NOW(), NOW())
                """,
                (tid,),
            )
            deleted["sod_reseed"] = deleted.get("sod_reseed", 0) + 1

    cur.execute("SET LOCAL session_replication_role = DEFAULT")
    conn.commit()

    after = count_map(cur)
    print("AFTER", json.dumps(after))

    # Structural assertions
    asserts = {
        "migrations_intact": after.get("migrations") == before.get("migrations"),
        "tenants_ge_1": (after.get("tenants") or 0) >= 1,
        "users_eq_keep": after.get("users") == len(KEEP_USER_EMAILS) or after.get("users") == len(keep_user_ids),
        "products_0": after.get("products_ra") == 0,
        "dossiers_0": after.get("dossiers") == 0,
        "registrations_0": after.get("registrations") == 0,
        "licenses_0": after.get("licenses") == 0,
        "imports_0": after.get("import_jobs") == 0,
        "roles_kept": (after.get("roles") or 0) > 0,
        "permissions_kept": (after.get("permissions") or 0) > 0,
        "sod_kept": (after.get("sod_settings") or 0) >= 1,
    }
    print("ASSERTS", json.dumps(asserts))
    if not all(asserts.values()):
        raise SystemExit("POST-RESET ASSERTIONS FAILED")

    # Clean physical storage tenant folders (lab)
    storage_roots = [ROOT / "storage", ROOT / "src" / "Compliance360.Web" / "storage"]
    cleaned_dirs = []
    for root in storage_roots:
        if not root.exists():
            continue
        for tid in KEEP_TENANT_IDS:
            seg = tid.replace("-", "")
            for child in root.iterdir():
                if child.is_dir() and (child.name.lower() == seg.lower() or child.name.lower() == tid.lower()):
                    shutil.rmtree(child, ignore_errors=True)
                    cleaned_dirs.append(str(child))

    report = {
        "at": datetime.now(timezone.utc).isoformat(),
        "target": target,
        "before": before,
        "after": after,
        "asserts": asserts,
        "deleted_rowcounts_sample": {k: deleted[k] for k in sorted(deleted)[:40]},
        "deleted_tables": len(deleted),
        "storage_cleaned": cleaned_dirs,
        "keep_user_emails": sorted(KEEP_USER_EMAILS),
        "keep_tenant_ids": sorted(KEEP_TENANT_IDS),
    }
    (OUT / "_reset_execution.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print("RESET_OK")
    conn.close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
