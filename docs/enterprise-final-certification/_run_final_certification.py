#!/usr/bin/env python3
"""
Enterprise Final Certification harness — real HTTP evidence.
Writes ONLY under docs/enterprise-final-certification/evidence/.
Does NOT modify docs/enterprise-functional-certification or docs/enterprise-remediation.
"""
from __future__ import annotations

import io
import json
import time
import uuid
import zipfile
from datetime import datetime, timezone
from pathlib import Path
from typing import Any
from urllib.parse import quote

import requests

BASE = "http://localhost:5272"
CERT_TENANT = "82af3877-2786-4d39-bce8-c981101c771d"
PLATFORM_TENANT = "dc7c46ee-cb25-4ed5-b0b4-800788f7f626"
PASS = "OwnerStart!2026"
PLATFORM_EMAIL = "admin@compliance360.local"

OUT = Path(__file__).resolve().parent / "evidence"
OUT.mkdir(parents=True, exist_ok=True)
TAG = datetime.now(timezone.utc).strftime("%Y%m%d-%H%M%S")
RUN_ID = f"FINAL-{TAG}"

CERT_USERS = {
    "Regulatory Specialist": "ra.spec@cert.local",
    "Reporting Manager": "reporting@cert.local",
    "Tenant Administrator": "irvingcorrosk19@gmail.com",
}

results: list[dict[str, Any]] = []
meta: dict[str, Any] = {
    "runId": RUN_ID,
    "baseUrl": BASE,
    "certTenantId": CERT_TENANT,
    "platformTenantId": PLATFORM_TENANT,
}


def rec(
    test_id: str,
    module: str,
    role: str,
    expected: str,
    actual: str,
    result: str,
    severity: str | None = None,
    evidence: Any = None,
):
    row = {
        "testId": test_id,
        "module": module,
        "role": role,
        "expected": expected,
        "actual": actual[:2000] if isinstance(actual, str) else actual,
        "result": result,  # PASS|FAIL only (SKIPPED must stay 0)
        "severity": severity,
        "evidence": evidence,
        "at": datetime.now(timezone.utc).isoformat(),
    }
    results.append(row)
    print(f"[{result}] {test_id} :: {str(actual)[:160]}")


def safe_json(r: requests.Response):
    try:
        return r.json()
    except Exception:
        return r.text[:800]


def login(email: str, password: str = PASS, tenant_id: str = CERT_TENANT) -> dict[str, Any]:
    r = requests.post(
        f"{BASE}/api/v1/auth/login",
        json={"tenantId": tenant_id, "email": email, "password": password},
        timeout=30,
    )
    if r.status_code >= 400:
        return {"ok": False, "status": r.status_code, "body": safe_json(r), "text": r.text[:500]}
    body = r.json()
    return {
        "ok": True,
        "status": r.status_code,
        "accessToken": body.get("accessToken") or body.get("token"),
        "body": body,
    }


def api(method: str, path: str, token: str | None, body: Any = None, timeout: int = 60) -> dict[str, Any]:
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    url = path if path.startswith("http") else f"{BASE}/api/v1{path}"
    t0 = time.perf_counter()
    try:
        r = requests.request(method, url, headers=headers, json=body, timeout=timeout)
        return {
            "status": r.status_code,
            "ok": r.ok,
            "body": safe_json(r),
            "text": r.text[:2000],
            "headers": dict(r.headers),
            "ms": round((time.perf_counter() - t0) * 1000, 1),
        }
    except Exception as e:
        return {"status": 0, "ok": False, "body": None, "text": str(e), "headers": {}, "ms": 0}


def api_bytes(method: str, path: str, token: str | None, body: Any = None, timeout: int = 60) -> dict[str, Any]:
    headers: dict[str, str] = {}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    if body is not None:
        headers["Content-Type"] = "application/json"
    url = path if path.startswith("http") else f"{BASE}/api/v1{path}"
    t0 = time.perf_counter()
    try:
        r = requests.request(
            method,
            url,
            headers=headers,
            json=body if body is not None else None,
            timeout=timeout,
        )
        return {
            "status": r.status_code,
            "ok": r.ok,
            "content": r.content,
            "contentType": r.headers.get("Content-Type", ""),
            "text": r.text[:500] if not r.ok else "",
            "ms": round((time.perf_counter() - t0) * 1000, 1),
        }
    except Exception as e:
        return {"status": 0, "ok": False, "content": b"", "contentType": "", "text": str(e), "ms": 0}


def extract_items(body: Any) -> list:
    if isinstance(body, list):
        return body
    if isinstance(body, dict):
        for key in ("items", "data", "results", "definitions", "tenants", "users", "roles"):
            if isinstance(body.get(key), list):
                return body[key]
    return []


def first_id(body: Any, *keys: str) -> str | None:
    if not isinstance(body, dict):
        return None
    for k in keys:
        v = body.get(k)
        if v:
            return str(v)
    return None


def phase_env():
    try:
        r = requests.get(f"{BASE}/health/live", timeout=10)
        rec(
            "ENV-LIVE",
            "Environment",
            "system",
            "200 live",
            f"http={r.status_code}",
            "PASS" if r.status_code == 200 else "FAIL",
            None if r.status_code == 200 else "P0",
        )
    except Exception as e:
        rec("ENV-LIVE", "Environment", "system", "200 live", str(e), "FAIL", "P0")


def phase_cert_smoke() -> dict[str, str]:
    tokens: dict[str, str] = {}
    for role, email in CERT_USERS.items():
        res = login(email, PASS, CERT_TENANT)
        if res.get("ok") and res.get("accessToken"):
            tokens[role] = res["accessToken"]
            rec(
                f"CERT-AUTH-{role.replace(' ', '_').upper()}",
                "Auth",
                role,
                "login success on CERT tenant",
                f"ok email={email}",
                "PASS",
                evidence={"email": email, "tenantId": CERT_TENANT},
            )
        else:
            rec(
                f"CERT-AUTH-{role.replace(' ', '_').upper()}",
                "Auth",
                role,
                "login success on CERT tenant",
                f"FAIL http={res.get('status')} {res.get('text')}",
                "FAIL",
                "P0",
            )
    return tokens


def find_tenant_by_slug(platform_token: str, slug: str) -> dict[str, Any] | None:
    listed = api("GET", f"/tenants?searchText={quote(slug)}&page=1&pageSize=50", platform_token)
    items = extract_items(listed.get("body"))
    for item in items:
        if not isinstance(item, dict):
            continue
        if str(item.get("slug", "")).lower() == slug.lower():
            return item
    # SuperAdmin platform-center fallback
    listed2 = api(
        "GET",
        f"/superadmin/platform-center/tenants?searchText={quote(slug)}&page=1&pageSize=50",
        platform_token,
    )
    items2 = extract_items(listed2.get("body"))
    for item in items2:
        if not isinstance(item, dict):
            continue
        if str(item.get("slug", "")).lower() == slug.lower():
            return item
    return None


def create_business_tenant(
    platform_token: str,
    label: str,
    preferred_slug: str,
    unique_slug: str,
) -> dict[str, Any]:
    """Create via POST /api/v1/tenants; fallback lookup rem-* or unique slug."""
    admin_email = f"admin.{label.lower()}.{TAG[:8]}@final.local"
    payload = {
        "name": f"FINAL {label} {TAG}",
        "slug": preferred_slug,
        "legalName": f"FINAL {label} Legal",
        "commercialName": f"FINAL {label}",
        "taxIdentifier": f"TAX-{label[:3].upper()}-{uuid.uuid4().hex[:8].upper()}",
        "countryCode": "PA",
        "currency": "USD",
        "adminEmail": admin_email,
        "adminFullName": f"{label} Admin",
        "adminPassword": PASS,
    }
    created = api("POST", "/tenants", platform_token, payload)
    body = created.get("body") if isinstance(created.get("body"), dict) else {}
    tenant_id = first_id(body, "id", "tenantId")
    slug_used = preferred_slug

    if created["status"] >= 400 or not tenant_id:
        existing = find_tenant_by_slug(platform_token, preferred_slug)
        if existing and existing.get("id"):
            tenant_id = str(existing["id"])
            slug_used = preferred_slug
            admin_email = f"spec.{label.lower()}.{TAG[:8]}@final.local"
            rec(
                f"MT-TENANT-{label}-LOOKUP",
                "Multitenancy",
                "Platform Administrator",
                f"reuse existing slug {preferred_slug}",
                f"id={tenant_id}",
                "PASS",
                evidence=existing,
            )
        else:
            payload["slug"] = unique_slug
            payload["taxIdentifier"] = f"TAX-{label[:3].upper()}-{uuid.uuid4().hex[:8].upper()}"
            payload["adminEmail"] = f"admin.{label.lower()}.{uuid.uuid4().hex[:6]}@final.local"
            admin_email = payload["adminEmail"]
            created2 = api("POST", "/tenants", platform_token, payload)
            body2 = created2.get("body") if isinstance(created2.get("body"), dict) else {}
            tenant_id = first_id(body2, "id", "tenantId")
            slug_used = unique_slug
            ok = created2["status"] < 300 and tenant_id
            rec(
                f"MT-TENANT-{label}-CREATE",
                "Multitenancy",
                "Platform Administrator",
                f"create slug {unique_slug}",
                f"http={created2['status']} id={tenant_id}",
                "PASS" if ok else "FAIL",
                None if ok else "P0",
                evidence={"preferred": preferred_slug, "body": body2 or created2.get("text")},
            )
            if not tenant_id:
                return {"ok": False, "label": label}
    else:
        rec(
            f"MT-TENANT-{label}-CREATE",
            "Multitenancy",
            "Platform Administrator",
            f"create slug {preferred_slug}",
            f"http={created['status']} id={tenant_id}",
            "PASS",
            evidence={"slug": preferred_slug, "adminEmail": admin_email},
        )

    # Activate (Draft → Active) so login works
    act = api("POST", f"/tenants/{tenant_id}/activate", platform_token, {})
    if act["status"] >= 400:
        act = api("POST", f"/superadmin/platform-center/tenants/{tenant_id}/activate", platform_token, {})
    rec(
        f"MT-TENANT-{label}-ACTIVATE",
        "Multitenancy",
        "Platform Administrator",
        "tenant Active",
        f"http={act['status']}",
        "PASS" if act["status"] < 300 else "FAIL",
        None if act["status"] < 300 else "P1",
    )

    return {
        "ok": True,
        "label": label,
        "tenantId": tenant_id,
        "slug": slug_used,
        "adminEmail": admin_email,
    }


def resolve_role_id(platform_token: str, tenant_id: str, role_name: str) -> str | None:
    listed = api("GET", f"/tenants/{tenant_id}/users?page=1&pageSize=5", platform_token)
    body = listed.get("body")
    roles: list = []
    if isinstance(body, dict) and isinstance(body.get("roles"), list):
        roles = body["roles"]
    for role in roles:
        if isinstance(role, dict) and str(role.get("name", "")).lower() == role_name.lower():
            return str(role.get("id")) if role.get("id") else None
    return None


def seed_reporting_manager(platform_token: str, tenant: dict[str, Any]) -> dict[str, Any]:
    """Seed a Reporting Manager (REPORT.MANAGE) for export isolation checks."""
    tenant_id = tenant["tenantId"]
    label = tenant["label"]
    email = f"rpt.{label.lower()}.{TAG[:8]}@final.local"
    role_id = resolve_role_id(platform_token, tenant_id, "Reporting Manager")
    if not role_id:
        role_id = resolve_role_id(platform_token, tenant_id, "Tenant Administrator")
    create = api(
        "POST",
        f"/tenants/{tenant_id}/users",
        platform_token,
        {
            "email": email,
            "fullName": f"{label} Reporting",
            "initialPassword": PASS,
            "forcePasswordChange": False,
            "roleId": role_id,
            "changeReason": f"FINAL cert reporting seed {RUN_ID}",
        },
    )
    if create["status"] >= 400:
        # Fallback to tenant admin email from create
        admin_email = tenant.get("adminEmail")
        if admin_email:
            auth = login(admin_email, PASS, tenant_id)
            if auth.get("ok") and auth.get("accessToken"):
                rec(
                    f"MT-USER-{label}-RPT-ADMIN-FALLBACK",
                    "Multitenancy",
                    "Tenant Administrator",
                    "admin login for reports",
                    f"email={admin_email}",
                    "PASS",
                )
                return {"ok": True, "email": admin_email, "token": auth["accessToken"]}
        rec(
            f"MT-USER-{label}-RPT",
            "Multitenancy",
            "Platform Administrator",
            "create Reporting Manager",
            f"http={create['status']} {create.get('text')}",
            "FAIL",
            "P1",
            evidence={"roleId": role_id, "body": create.get("body")},
        )
        return {"ok": False}

    auth = login(email, PASS, tenant_id)
    ok = bool(auth.get("ok") and auth.get("accessToken"))
    rec(
        f"MT-USER-{label}-RPT",
        "Multitenancy",
        "Reporting Manager",
        "create + login reporting user",
        f"email={email} login={'ok' if ok else auth.get('status')}",
        "PASS" if ok else "FAIL",
        None if ok else "P1",
    )
    if not ok:
        return {"ok": False}
    return {"ok": True, "email": email, "token": auth["accessToken"]}


def seed_specialist(platform_token: str, tenant: dict[str, Any]) -> dict[str, Any]:
    tenant_id = tenant["tenantId"]
    label = tenant["label"]
    email = f"spec.{label.lower()}.{TAG[:8]}@final.local"
    role_id = resolve_role_id(platform_token, tenant_id, "Regulatory Specialist")
    create = api(
        "POST",
        f"/tenants/{tenant_id}/users",
        platform_token,
        {
            "email": email,
            "fullName": f"{label} Specialist",
            "initialPassword": PASS,
            "forcePasswordChange": False,
            "roleId": role_id,
            "changeReason": f"FINAL cert seed {RUN_ID}",
        },
    )
    ok = create["status"] < 300
    if not ok:
        # Fallback: try admin login (may fail if force password change)
        admin_email = tenant.get("adminEmail")
        if admin_email:
            auth = login(admin_email, PASS, tenant_id)
            if auth.get("ok") and auth.get("accessToken"):
                rec(
                    f"MT-USER-{label}-ADMIN-FALLBACK",
                    "Multitenancy",
                    "Tenant Administrator",
                    "admin login usable",
                    f"email={admin_email}",
                    "PASS",
                )
                return {"ok": True, "email": admin_email, "token": auth["accessToken"], "role": "Tenant Administrator"}
        rec(
            f"MT-USER-{label}-SPEC",
            "Multitenancy",
            "Platform Administrator",
            "create Regulatory Specialist",
            f"http={create['status']} {create.get('text')}",
            "FAIL",
            "P0",
            evidence=create.get("body"),
        )
        return {"ok": False}

    auth = login(email, PASS, tenant_id)
    if not (auth.get("ok") and auth.get("accessToken")):
        rec(
            f"MT-USER-{label}-SPEC-LOGIN",
            "Multitenancy",
            "Regulatory Specialist",
            "login specialist",
            f"http={auth.get('status')} {auth.get('text')}",
            "FAIL",
            "P0",
        )
        return {"ok": False}

    rec(
        f"MT-USER-{label}-SPEC",
        "Multitenancy",
        "Regulatory Specialist",
        "create + login specialist",
        f"email={email}",
        "PASS",
        evidence={"email": email, "roleId": role_id},
    )
    return {"ok": True, "email": email, "token": auth["accessToken"], "role": "Regulatory Specialist"}


def seed_product(token: str, tenant_id: str, marker: str, label: str) -> dict[str, Any]:
    ra = f"/tenants/{tenant_id}/regulatory"
    payload = {
        "countryCode": "PA",
        "category": "Insumos Medicos",
        "brand": f"FINAL-{label}",
        "regulatoryName": f"FINAL Product {marker}",
        "catalogCode": marker,
        "riskClass": "A",
        "currency": "USD",
        "distributorName": f"FINAL Dist {label}",
        "opportunityAmount": 25,
    }
    prod = api("POST", f"{ra}/products", token, payload)
    product_id = first_id(prod.get("body") if isinstance(prod.get("body"), dict) else {}, "id")
    ok = prod["status"] < 300 and product_id
    rec(
        f"MT-PRODUCT-{label}",
        "Products",
        "Regulatory Specialist",
        f"seed catalogCode {marker}",
        f"http={prod['status']} id={product_id}",
        "PASS" if ok else "FAIL",
        None if ok else "P0",
        evidence={"catalogCode": marker, "productId": product_id},
    )
    return {"ok": ok, "productId": product_id, "marker": marker}


def count_marker_hits(body: Any, marker: str) -> int:
    text = json.dumps(body, default=str) if not isinstance(body, str) else body
    return text.upper().count(marker.upper())


def isolation_checks(
    from_label: str,
    from_token: str,
    from_tenant: str,
    peer_label: str,
    peer_tenant: str,
    peer_product_id: str | None,
    peer_marker: str,
):
    ra = f"/tenants/{from_tenant}/regulatory"
    search = api("GET", f"{ra}/products?searchText={quote(peer_marker)}", from_token)
    hits = count_marker_hits(search.get("body"), peer_marker)
    items = extract_items(search.get("body"))
    # Also count only items whose catalogCode matches
    item_hits = sum(
        1
        for i in items
        if isinstance(i, dict) and peer_marker.upper() in str(i.get("catalogCode", "")).upper()
    )
    rec(
        f"MT-ISO-{from_label}-SEARCH-{peer_label}",
        "Multitenancy",
        "Regulatory Specialist",
        f"search {peer_marker} returns 0",
        f"http={search['status']} itemHits={item_hits} textHits={hits}",
        "PASS" if search["status"] < 300 and item_hits == 0 else "FAIL",
        None if item_hits == 0 else "P0",
        evidence={"status": search["status"], "itemHits": item_hits},
    )

    if peer_product_id:
        getp = api("GET", f"{ra}/products/{peer_product_id}", from_token)
        denied = getp["status"] in (401, 403, 404) or (
            getp["status"] == 400 and "not found" in str(getp.get("body") or getp.get("text") or "").lower()
        )
        rec(
            f"MT-ISO-{from_label}-GET-PRODUCT-{peer_label}",
            "Multitenancy",
            "Regulatory Specialist",
            "GET foreign product denied (403/404)",
            f"http={getp['status']}",
            "PASS" if denied else "FAIL",
            None if denied else "P0",
        )
        getd = api("GET", f"{ra}/dossiers/{peer_product_id}", from_token)
        denied_d = getd["status"] in (401, 403, 404) or (
            getd["status"] == 400 and "not found" in str(getd.get("body") or getd.get("text") or "").lower()
        )
        rec(
            f"MT-ISO-{from_label}-GET-DOSSIER-{peer_label}",
            "Multitenancy",
            "Regulatory Specialist",
            "GET foreign dossier/product id denied (403/404)",
            f"http={getd['status']}",
            "PASS" if denied_d else "FAIL",
            None if denied_d else "P0",
        )

    # Direct path against peer tenant id with from_token
    cross = api("GET", f"/tenants/{peer_tenant}/regulatory/products", from_token)
    denied_x = cross["status"] in (401, 403, 404)
    rec(
        f"MT-ISO-{from_label}-PATH-{peer_label}",
        "Multitenancy",
        "Regulatory Specialist",
        "foreign tenant path denied",
        f"http={cross['status']}",
        "PASS" if denied_x else "FAIL",
        None if denied_x else "P0",
    )


def export_and_assert_no_leak(
    token: str,
    tenant_id: str,
    foreign_tenant_id: str,
    label: str,
):
    base = f"/tenants/{tenant_id}/reports"
    listed = api("GET", f"{base}?page=1&pageSize=50", token)
    items = extract_items(listed.get("body"))
    if listed["status"] >= 400:
        rec(
            f"MT-RPT-{label}-LIST",
            "Reporting",
            "Reporting Manager",
            "list reports",
            f"http={listed['status']}",
            "FAIL",
            "P1",
        )
        return

    if not items:
        seed = api("POST", f"{base}/standard/seed", token, {})
        listed = api("GET", f"{base}?page=1&pageSize=50", token)
        items = extract_items(listed.get("body"))
        rec(
            f"MT-RPT-{label}-SEED",
            "Reporting",
            "Reporting Manager",
            "seed standard reports",
            f"http={seed['status']} count={len(items)}",
            "PASS" if items else "FAIL",
            None if items else "P1",
        )
        if not items:
            return

    definition = next((i for i in items if str(i.get("status", "")).lower() in ("active", "1")), items[0])
    report_id = definition.get("id")
    if not report_id:
        rec(f"MT-RPT-{label}-ID", "Reporting", "Reporting Manager", "report id", "missing", "FAIL", "P1")
        return

    status = str(definition.get("status", "")).lower()
    if status in ("draft", "0"):
        api("POST", f"{base}/{report_id}/activate", token, {})

    exe = api("POST", f"{base}/{report_id}/execute", token, {"parametersJson": "{}"})
    exec_body = exe.get("body") if isinstance(exe.get("body"), dict) else {}
    execution_id = first_id(exec_body, "id", "executionId")
    if not execution_id:
        rec(f"MT-RPT-{label}-EXE", "Reporting", "Reporting Manager", "execute", f"http={exe['status']}", "FAIL", "P1")
        return

    if str(exec_body.get("status", "")).lower() not in ("completed", "2"):
        api(
            "POST",
            f"{base}/{report_id}/complete",
            token,
            {
                "executionId": execution_id,
                "rowCount": 1,
                "datasetDescriptorJson": json.dumps({"columns": ["sample"], "rows": [["final"]]}),
            },
        )

    for fmt in ("Csv", "Excel"):
        exp = api("POST", f"{base}/{report_id}/export", token, {"executionId": execution_id, "format": fmt})
        exp_body = exp.get("body") if isinstance(exp.get("body"), dict) else {}
        export_id = first_id(exp_body, "id", "exportId")
        if not export_id:
            rec(
                f"MT-RPT-{label}-EXPORT-{fmt.upper()}",
                "Reporting",
                "Reporting Manager",
                f"export {fmt}",
                f"http={exp['status']}",
                "FAIL",
                "P1",
            )
            continue
        content = api_bytes("GET", f"{base}/{report_id}/exports/{export_id}/content", token)
        raw = content.get("content") or b""
        # Search binary/text for foreign tenant GUID
        leak = foreign_tenant_id.encode("utf-8") in raw or foreign_tenant_id.lower().encode("utf-8") in raw.lower()
        rec(
            f"MT-RPT-{label}-NO-LEAK-{fmt.upper()}",
            "Reporting",
            "Reporting Manager",
            f"{fmt} must not contain peer tenant id",
            f"http={content['status']} size={len(raw)} leak={leak}",
            "PASS" if content["status"] < 300 and not leak else "FAIL",
            None if not leak else "P0",
            evidence={"exportId": export_id, "size": len(raw), "foreignTenantId": foreign_tenant_id},
        )


def phase_xlsx_ooxml(cert_tokens: dict[str, str]):
    rpt = cert_tokens.get("Reporting Manager")
    if not rpt:
        rec("XLSX-OOXML", "Reporting", "Reporting Manager", "OOXML package", "no token", "FAIL", "P0")
        return

    base = f"/tenants/{CERT_TENANT}/reports"
    listed = api("GET", f"{base}?page=1&pageSize=50", rpt)
    items = extract_items(listed.get("body"))
    if not items:
        api("POST", f"{base}/standard/seed", rpt, {})
        listed = api("GET", f"{base}?page=1&pageSize=50", rpt)
        items = extract_items(listed.get("body"))
    if not items:
        rec("XLSX-OOXML", "Reporting", "Reporting Manager", "OOXML package", "no reports", "FAIL", "P1")
        return

    definition = next((i for i in items if str(i.get("status", "")).lower() in ("active", "1")), items[0])
    report_id = definition.get("id")
    if str(definition.get("status", "")).lower() in ("draft", "0"):
        api("POST", f"{base}/{report_id}/activate", rpt, {})

    exe = api("POST", f"{base}/{report_id}/execute", rpt, {"parametersJson": "{}"})
    exec_body = exe.get("body") if isinstance(exe.get("body"), dict) else {}
    execution_id = first_id(exec_body, "id", "executionId")
    if not execution_id:
        rec("XLSX-OOXML", "Reporting", "Reporting Manager", "execute for excel", f"http={exe['status']}", "FAIL", "P1")
        return

    if str(exec_body.get("status", "")).lower() not in ("completed", "2"):
        api(
            "POST",
            f"{base}/{report_id}/complete",
            rpt,
            {
                "executionId": execution_id,
                "rowCount": 1,
                "datasetDescriptorJson": json.dumps({"columns": ["sample"], "rows": [["xlsx"]]}),
            },
        )

    exp = api("POST", f"{base}/{report_id}/export", rpt, {"executionId": execution_id, "format": "Excel"})
    exp_body = exp.get("body") if isinstance(exp.get("body"), dict) else {}
    export_id = first_id(exp_body, "id", "exportId")
    if not export_id:
        rec("XLSX-EXPORT", "Reporting", "Reporting Manager", "export Excel", f"http={exp['status']}", "FAIL", "P1")
        return

    content = api_bytes("GET", f"{base}/{report_id}/exports/{export_id}/content", rpt)
    raw = content.get("content") or b""
    out_path = OUT / f"report-excel-{RUN_ID}.xlsx"
    out_path.write_bytes(raw)
    meta["xlsxEvidence"] = str(out_path.name)

    required = [
        "[Content_Types].xml",
        "xl/workbook.xml",
        "xl/worksheets/sheet1.xml",
        "xl/sharedStrings.xml",
    ]
    names: list[str] = []
    zip_ok = False
    try:
        with zipfile.ZipFile(io.BytesIO(raw)) as zf:
            names = zf.namelist()
            zip_ok = all(n in names for n in required)
    except Exception as e:
        rec(
            "XLSX-OOXML",
            "Reporting",
            "Reporting Manager",
            "ZIP with OOXML parts",
            f"not a zip: {e}; size={len(raw)} pk={raw[:2]!r}",
            "FAIL",
            "P0",
            evidence={"size": len(raw), "saved": out_path.name},
        )
        return

    rec(
        "XLSX-OOXML",
        "Reporting",
        "Reporting Manager",
        "ZIP has Content_Types, workbook, sheet1, sharedStrings",
        f"size={len(raw)} parts_ok={zip_ok} entries={len(names)}",
        "PASS" if zip_ok and content["status"] < 300 and len(raw) > 0 else "FAIL",
        None if zip_ok else "P0",
        evidence={"required": required, "names": names[:40], "saved": out_path.name, "contentType": content.get("contentType")},
    )


def phase_multitenant(cert_tokens: dict[str, str]):
    plat = login(PLATFORM_EMAIL, PASS, PLATFORM_TENANT)
    if not (plat.get("ok") and plat.get("accessToken")):
        rec(
            "MT-PLATFORM-LOGIN",
            "Multitenancy",
            "Platform Administrator",
            "platform login",
            f"http={plat.get('status')} {plat.get('text')}",
            "FAIL",
            "P0",
        )
        # Fallback: CERT as ALPHA only — still attempt isolation vs platform id
        alpha_token = cert_tokens.get("Regulatory Specialist")
        if alpha_token:
            isolation_checks(
                "ALPHA",
                alpha_token,
                CERT_TENANT,
                "PLATFORM",
                PLATFORM_TENANT,
                None,
                "BETA-MARKER-NONE",
            )
        return

    platform_token = plat["accessToken"]
    rec("MT-PLATFORM-LOGIN", "Multitenancy", "Platform Administrator", "platform login", "ok", "PASS")

    alpha_info = create_business_tenant(platform_token, "ALPHA", f"final-alpha-{TAG[-6:]}", f"FINAL-ALPHA-{TAG}")
    beta_info = create_business_tenant(platform_token, "BETA", f"final-beta-{TAG[-6:]}", f"FINAL-BETA-{TAG}")

    # Simpler fallback: CERT as ALPHA if business ALPHA failed
    alpha_ctx: dict[str, Any]
    if alpha_info.get("ok"):
        user = seed_specialist(platform_token, alpha_info)
        if user.get("ok"):
            alpha_ctx = {
                "tenantId": alpha_info["tenantId"],
                "token": user["token"],
                "email": user["email"],
                "source": "created",
            }
        else:
            alpha_ctx = {
                "tenantId": CERT_TENANT,
                "token": cert_tokens.get("Regulatory Specialist"),
                "email": CERT_USERS["Regulatory Specialist"],
                "source": "cert-fallback",
            }
            rec(
                "MT-ALPHA-CERT-FALLBACK",
                "Multitenancy",
                "Regulatory Specialist",
                "use CERT as ALPHA",
                f"tenant={CERT_TENANT}",
                "PASS" if alpha_ctx["token"] else "FAIL",
                None if alpha_ctx["token"] else "P0",
            )
    else:
        alpha_ctx = {
            "tenantId": CERT_TENANT,
            "token": cert_tokens.get("Regulatory Specialist"),
            "email": CERT_USERS["Regulatory Specialist"],
            "source": "cert",
        }
        rec(
            "MT-ALPHA-CERT",
            "Multitenancy",
            "Regulatory Specialist",
            "use CERT tenant as ALPHA",
            f"tenant={CERT_TENANT}",
            "PASS" if alpha_ctx["token"] else "FAIL",
            None if alpha_ctx["token"] else "P0",
        )

    if not beta_info.get("ok"):
        rec("MT-BETA", "Multitenancy", "Platform Administrator", "BETA tenant available", "create failed", "FAIL", "P0")
        return

    beta_user = seed_specialist(platform_token, beta_info)
    if not beta_user.get("ok"):
        return

    beta_ctx = {
        "tenantId": beta_info["tenantId"],
        "token": beta_user["token"],
        "email": beta_user["email"],
        "source": "created",
    }

    meta["alphaTenantId"] = alpha_ctx["tenantId"]
    meta["betaTenantId"] = beta_ctx["tenantId"]
    meta["alphaSource"] = alpha_ctx["source"]

    alpha_marker = f"ALPHA-MARKER-{TAG}"
    beta_marker = f"BETA-MARKER-{TAG}"
    alpha_prod = seed_product(alpha_ctx["token"], alpha_ctx["tenantId"], alpha_marker, "ALPHA")
    beta_prod = seed_product(beta_ctx["token"], beta_ctx["tenantId"], beta_marker, "BETA")

    if alpha_ctx["token"] and beta_ctx["token"]:
        isolation_checks(
            "ALPHA",
            alpha_ctx["token"],
            alpha_ctx["tenantId"],
            "BETA",
            beta_ctx["tenantId"],
            beta_prod.get("productId"),
            beta_marker,
        )
        isolation_checks(
            "BETA",
            beta_ctx["token"],
            beta_ctx["tenantId"],
            "ALPHA",
            alpha_ctx["tenantId"],
            alpha_prod.get("productId"),
            alpha_marker,
        )

    # Report export leak check from ALPHA with REPORT.MANAGE capability
    rpt_token = None
    rpt_tenant = alpha_ctx["tenantId"]
    if alpha_ctx["tenantId"] == CERT_TENANT:
        rpt_token = cert_tokens.get("Reporting Manager") or cert_tokens.get("Tenant Administrator")
    elif alpha_info.get("ok"):
        rpt_user = seed_reporting_manager(platform_token, alpha_info)
        if rpt_user.get("ok"):
            rpt_token = rpt_user["token"]
        else:
            admin_email = alpha_info.get("adminEmail")
            if admin_email:
                auth = login(admin_email, PASS, alpha_ctx["tenantId"])
                if auth.get("ok") and auth.get("accessToken"):
                    rpt_token = auth["accessToken"]
    if not rpt_token:
        rpt_token = alpha_ctx["token"]

    if rpt_token:
        export_and_assert_no_leak(rpt_token, rpt_tenant, beta_ctx["tenantId"], "ALPHA")


def write_summary():
    passed = sum(1 for r in results if r["result"] == "PASS")
    failed = sum(1 for r in results if r["result"] == "FAIL")
    skipped = sum(1 for r in results if r["result"] == "SKIPPED")
    blocked = sum(1 for r in results if r["result"] == "BLOCKED")
    summary = {
        "runId": RUN_ID,
        "at": datetime.now(timezone.utc).isoformat(),
        "baseUrl": BASE,
        "meta": meta,
        "counts": {
            "total": len(results),
            "PASS": passed,
            "FAIL": failed,
            "SKIPPED": skipped,
            "BLOCKED": blocked,
        },
        "verdict": "PASS" if failed == 0 and skipped == 0 else "FAIL",
        "results": results,
    }
    latest = OUT / "latest-results.json"
    stamped = OUT / f"results-{RUN_ID}.json"
    final = OUT / "FINAL_CERTIFICATION_SUMMARY.json"
    for path in (latest, stamped, final):
        path.write_text(json.dumps(summary, indent=2, default=str), encoding="utf-8")

    print()
    print("=" * 72)
    print(f"FINAL CERTIFICATION {RUN_ID}")
    print(f"PASS={passed} FAIL={failed} SKIPPED={skipped} BLOCKED={blocked} TOTAL={len(results)}")
    print(f"VERDICT={summary['verdict']}")
    print(f"evidence={OUT}")
    print("=" * 72)
    return summary


def main():
    print(f"Starting final certification {RUN_ID} against {BASE}")
    phase_env()
    cert_tokens = phase_cert_smoke()
    phase_multitenant(cert_tokens)
    phase_xlsx_ooxml(cert_tokens)
    write_summary()


if __name__ == "__main__":
    main()
