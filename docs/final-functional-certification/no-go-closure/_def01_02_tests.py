"""DEF-0001 / DEF-0002 closure tests + full REGUTRACK import recon scaffolding."""
from __future__ import annotations

import hashlib
import json
import shutil
import urllib.error
import urllib.request
from datetime import date
from pathlib import Path

OUT = Path(r"c:\Proyectos\Compliance 360\docs\final-functional-certification\no-go-closure")
OUT.mkdir(parents=True, exist_ok=True)
BASE = "http://localhost:5272/api/v1"
TID = "82af3877-2786-4d39-bce8-c981101c771d"
XLSX = Path(r"c:\Proyectos\Compliance 360\REGUTRACK 02JUN26 MG.xlsx")
COPY = OUT / "REGUTRACK_02JUN26_MG_CLOSURE_COPY.xlsx"
RA_PASS = "CertRaPass!2026"
results: list[dict] = []


def rec(testid: str, ok: bool, detail: str) -> None:
    results.append({"id": testid, "pass": ok, "detail": detail})
    print(f"[{'PASS' if ok else 'FAIL'}] {testid} :: {detail}")


def login(email: str, password: str) -> str:
    req = urllib.request.Request(
        f"{BASE}/auth/login",
        data=json.dumps({"tenantId": TID, "email": email, "password": password}).encode(),
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req) as r:
        return json.loads(r.read())["accessToken"]


def api(method: str, path: str, token: str | None, body=None):
    data = None if body is None else json.dumps(body).encode()
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    req = urllib.request.Request(f"{BASE}{path}", data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req) as r:
            raw = r.read()
            return r.status, json.loads(raw) if raw else None
    except urllib.error.HTTPError as e:
        raw = e.read().decode(errors="replace")
        try:
            parsed = json.loads(raw)
        except Exception:
            parsed = raw
        return e.code, parsed


def sha256(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def main() -> None:
    spec = login("ra.spec@cert.local", RA_PASS)
    admin = login("ra.admin@cert.local", RA_PASS)
    view = login("ra.view@cert.local", RA_PASS)
    ra = f"/tenants/{TID}/regulatory"

    # ---- DEF-0001 ----
    st, lic = api(
        "POST",
        f"{ra}/operating-licenses",
        spec,
        {
            "companyName": "Multimed",
            "companyId": None,
            "licenseType": f"Licencia Closure {date.today().isoformat()}",
            "authorityId": None,
            "licenseNumber": "LOP-CLOSE-001",
            "issuedOn": None,
            "expiresOn": "2029-11-21T00:00:00Z",
            "comments": "DEF-0001",
            "companyConstitutedOn": "2003-01-17",
            "operationsStartedOn": "2003-01-01",
        },
    )
    rec("DEF1-CREATE-BOTH", st < 300 and lic and lic.get("companyConstitutedOn") == "2003-01-17" and lic.get("operationsStartedOn") == "2003-01-01", f"http={st} body={lic}")
    lic_id = lic.get("id") if isinstance(lic, dict) else None

    st2, lic2 = api(
        "POST",
        f"{ra}/operating-licenses",
        spec,
        {
            "companyName": "4 Hospitals",
            "companyId": None,
            "licenseType": f"Licencia NoDates {date.today().isoformat()}",
            "authorityId": None,
            "licenseNumber": None,
            "issuedOn": None,
            "expiresOn": "2029-11-20T00:00:00Z",
            "comments": "no company dates",
            "companyConstitutedOn": None,
            "operationsStartedOn": None,
        },
    )
    rec("DEF1-CREATE-NULL", st2 < 300 and lic2.get("companyConstitutedOn") is None, f"http={st2}")

    st3, lic3 = api(
        "PUT",
        f"{ra}/operating-licenses/{lic_id}/company-dates",
        spec,
        {"companyConstitutedOn": "2003-01-17", "operationsStartedOn": "2003-02-01", "clearConstitution": False, "clearOperationsStart": False},
    )
    rec("DEF1-EDIT-OPS", st3 < 300 and lic3.get("operationsStartedOn") == "2003-02-01" and lic3.get("companyConstitutedOn") == "2003-01-17", f"http={st3} {lic3}")

    st4, listed = api("GET", f"{ra}/operating-licenses", spec)
    found = [x for x in (listed or []) if x.get("id") == lic_id]
    rec("DEF1-LIST-READ", st4 < 300 and found and found[0].get("companyConstitutedOn") == "2003-01-17", f"n={len(found)}")

    st5, _ = api(
        "PUT",
        f"{ra}/operating-licenses/{lic_id}/company-dates",
        view,
        {"companyConstitutedOn": "1999-01-01", "operationsStartedOn": None, "clearConstitution": False, "clearOperationsStart": False},
    )
    rec("DEF1-VIEW-DENY", st5 >= 400, f"http={st5}")

    fake = "11111111-1111-1111-1111-111111111111"
    st6, _ = api("GET", f"/tenants/{fake}/regulatory/operating-licenses", spec)
    rec("DEF1-X-TENANT", st6 >= 400, f"http={st6}")

    # ---- DEF-0002 ----
    code = f"FT-{date.today().strftime('%Y%m%d%H%M%S')}"
    st, prod = api(
        "POST",
        f"{ra}/products",
        spec,
        {
            "countryCode": "PA",
            "category": "Insumos Medicos",
            "brand": "FTTEST",
            "regulatoryName": "Producto Ficha Closure",
            "catalogCode": code,
            "riskClass": "A",
            "distributorName": "Multimed",
            "technicalSheetReference": "102300",
            "formReference": None,
            "currency": "USD",
        },
    )
    rec("DEF2-CREATE-FICHA-REF", st < 300 and prod.get("technicalSheetReference") == "102300" and prod.get("technicalSheetStatus") == "Received", f"http={st} status={prod.get('technicalSheetStatus') if prod else None}")
    pid = prod.get("id") if isinstance(prod, dict) else None

    st, prod2 = api(
        "POST",
        f"{ra}/products/{pid}/artifacts",
        spec,
        {
            "artifactKind": "ficha_tecnica",
            "reference": "102300",
            "documentId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
            "storedFileId": "ffffffff-1111-2222-3333-444444444444",
            "status": "Accepted",
        },
    )
    rec("DEF2-ATTACH-FICHA", st < 300 and prod2.get("technicalSheetDocumentId") and prod2.get("technicalSheetStatus") == "Accepted", f"http={st}")

    st, prod3 = api(
        "POST",
        f"{ra}/products/{pid}/artifacts",
        spec,
        {
            "artifactKind": "formulario",
            "reference": "FORM-MINSA-A",
            "documentId": "bbbbbbbb-bbbb-cccc-dddd-eeeeeeeeeeee",
            "storedFileId": None,
            "status": "Received",
        },
    )
    rec("DEF2-ATTACH-FORM", st < 300 and prod3.get("formReference") == "FORM-MINSA-A" and prod3.get("formStatus") == "Received", f"http={st}")

    st, prod4 = api(
        "POST",
        f"{ra}/products/{pid}/artifacts",
        spec,
        {
            "artifactKind": "ficha_tecnica",
            "reference": "102300-V2",
            "documentId": "cccccccc-bbbb-cccc-dddd-eeeeeeeeeeee",
            "storedFileId": "dddddddd-1111-2222-3333-444444444444",
            "status": "Accepted",
        },
    )
    rec("DEF2-VERSION-FICHA", st < 300 and prod4.get("technicalSheetReference") == "102300-V2", f"http={st} ref={prod4.get('technicalSheetReference') if prod4 else None}")

    st, _ = api(
        "POST",
        f"{ra}/products/{pid}/artifacts",
        view,
        {"artifactKind": "ficha_tecnica", "reference": "X", "documentId": None, "storedFileId": None, "status": "Pending"},
    )
    rec("DEF2-VIEW-DENY", st >= 400, f"http={st}")

    st, got = api("GET", f"{ra}/products/{pid}", spec)
    rec("DEF2-PRODUCT-READ", st < 300 and got.get("formReference") == "FORM-MINSA-A" and got.get("technicalSheetDocumentId"), f"http={st}")

    # ---- Manifest for DEF-0003 ----
    shutil.copy2(XLSX, COPY)
    manifest = {
        "original": {"path": str(XLSX), "bytes": XLSX.stat().st_size, "sha256": sha256(XLSX)},
        "copy": {"path": str(COPY), "bytes": COPY.stat().st_size, "sha256": sha256(COPY)},
        "sha256Match": sha256(XLSX) == sha256(COPY),
    }
    (OUT / "01_REGUTRACK_SOURCE_MANIFEST.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    rec("DEF3-MANIFEST", manifest["sha256Match"], manifest["copy"]["sha256"][:16])

    (OUT / "def01_02_api_results.json").write_text(json.dumps(results, indent=2), encoding="utf-8")
    fails = sum(1 for r in results if not r["pass"])
    print(f"=== DEF1/2 SUMMARY PASS={len(results)-fails} FAIL={fails} ===")
    raise SystemExit(1 if fails else 0)


if __name__ == "__main__":
    main()
