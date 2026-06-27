import json
from datetime import datetime, timezone
from pathlib import Path

from playwright.sync_api import sync_playwright


ROOT = Path(__file__).resolve().parents[1]
RUN_ID = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
ARTIFACTS = ROOT / "artifacts" / "e2e" / RUN_ID
REPORT = ROOT / "docs" / "e2e" / "04_QUALITY_MANAGER_E2E_REPORT.md"
BASE_URL = "http://localhost:5272"
TENANT_ID = "4c131edd-47dc-4bb4-908a-a4b5e34c85ac"
EMAIL = "quality.manager@alimentos-premium.test"
PASSWORD = "QualityMgr!2026"


def screenshot(page, name):
    path = ARTIFACTS / f"quality-manager-{name}.png"
    page.screenshot(path=str(path), full_page=True)
    return str(path.relative_to(ROOT)).replace("\\", "/")


def fill_common_module_form(page, title, code, description=None):
    page.fill("#module-action-form #name", title)
    page.fill("#module-action-form #code", code)
    if description and page.locator("#module-action-form #description").count():
        page.fill("#module-action-form #description", description)
    page.locator("#module-action-form button[type='submit']").click()
    page.locator(".toast.success").first.wait_for(timeout=15000)
    page.get_by_text(code, exact=False).first.wait_for(timeout=15000)


def main():
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    console = []
    network = []
    steps = []
    verdict = "FAIL"
    final_state = "Not completed."

    with sync_playwright() as p:
        browser = p.chromium.launch(headless=False, slow_mo=700)
        context = browser.new_context(
            base_url=BASE_URL,
            viewport={"width": 1440, "height": 1000},
            record_video_dir=str(ARTIFACTS / "videos"),
        )
        context.tracing.start(screenshots=True, snapshots=True, sources=True)
        page = context.new_page()
        page.on("console", lambda message: console.append({"type": message.type, "text": message.text}))

        def on_response(response):
            if response.status >= 400:
                network.append({"method": response.request.method, "url": response.url, "status": response.status})

        page.on("response", on_response)

        try:
            suffix = datetime.now(timezone.utc).strftime("%H%M%S")

            page.goto("/", wait_until="domcontentloaded")
            page.fill("#tenantId", TENANT_ID)
            page.fill("#email", EMAIL)
            page.fill("#password", PASSWORD)
            page.get_by_role("button", name="Iniciar sesion").click()
            page.wait_for_selector(".layout", timeout=15000)
            steps.append(("Login", "PASS", screenshot(page, "login")))

            page.get_by_text("Centro de comando", exact=False).first.wait_for(timeout=15000)
            steps.append(("Dashboard", "PASS", screenshot(page, "dashboard")))

            page.locator('[data-route="documents"]').first.click()
            page.get_by_text("Document Management", exact=False).first.wait_for(timeout=15000)
            doc_code = f"QM-DOC-{suffix}"
            fill_common_module_form(page, f"Politica Calidad E2E {suffix}", doc_code, "Documento para revision Quality Manager.")
            steps.append(("Revisar/crear documento", "PASS", screenshot(page, "document")))

            page.locator('[data-route="capa"]').first.click()
            page.get_by_text("CAPA", exact=False).first.wait_for(timeout=15000)
            capa_code = f"QM-CAPA-{suffix}"
            fill_common_module_form(page, f"CAPA Calidad E2E {suffix}", capa_code, "CAPA creada por Quality Manager.")
            steps.append(("Crear CAPA", "PASS", screenshot(page, "capa")))

            page.locator('[data-route="risks"]').first.click()
            page.get_by_text("Risk Management", exact=False).first.wait_for(timeout=15000)
            risk_code = f"QM-RISK-{suffix}"
            fill_common_module_form(page, f"Riesgo Calidad E2E {suffix}", risk_code, "Riesgo creado por Quality Manager.")
            steps.append(("Crear riesgo", "PASS", screenshot(page, "risk")))

            page.locator('[data-route="indicators"]').first.click()
            page.get_by_text("Quality Indicators", exact=False).first.wait_for(timeout=15000)
            kpi_code = f"QM-KPI-{suffix}"
            fill_common_module_form(page, f"KPI Calidad E2E {suffix}", kpi_code, "Indicador creado por Quality Manager.")
            steps.append(("Crear indicador", "PASS", screenshot(page, "indicator")))

            page.locator('[data-route="reports"]').first.click()
            page.get_by_text("Report Center", exact=False).first.wait_for(timeout=15000)
            steps.append(("Revisar reportes", "PASS", screenshot(page, "reports")))

            page.locator('[data-route="audit-trail"]').first.click()
            page.get_by_text("Audit Trail", exact=False).first.wait_for(timeout=15000)
            steps.append(("Validar trazabilidad", "PASS", screenshot(page, "audit-trail")))

            page.locator("#logout").click()
            page.locator("#login-form").wait_for(timeout=10000)
            steps.append(("Logout", "PASS", screenshot(page, "logout")))

            errors = [item for item in console if item["type"] in {"error", "assert"}]
            if errors:
                raise RuntimeError(f"Console errors found: {errors}")
            if network:
                raise RuntimeError(f"Unexpected HTTP errors found: {network}")

            verdict = "PASS"
            final_state = "Quality Manager visible browser flow completed without console or HTTP errors."
        except Exception as exc:
            final_state = str(exc)
            steps.append(("Blocking error", "FAIL", screenshot(page, "blocking-error")))
        finally:
            trace = ARTIFACTS / "quality-manager-trace.zip"
            context.tracing.stop(path=str(trace))
            page.wait_for_timeout(3000)
            context.close()
            browser.close()

    lines = [
        "# Quality Manager E2E Report",
        "",
        f"- Rol probado: Quality Manager",
        f"- Usuario utilizado: {EMAIL}",
        f"- Fecha: {datetime.now(timezone.utc).isoformat()}",
        f"- URL: {BASE_URL}",
        "- Navegador: Chromium headed via Playwright Python",
        f"- Estado inicial: Tenant {TENANT_ID}; rol Quality Manager E2E preparado.",
        f"- Trace: {str((ARTIFACTS / 'quality-manager-trace.zip').relative_to(ROOT)).replace(chr(92), '/')}",
        f"- Veredicto: {verdict}",
        "",
        "## Pasos Ejecutados",
        "",
    ]
    for index, (name, status, shot) in enumerate(steps, start=1):
        lines.extend([f"### {index}. {name}", f"- Estado: {status}", f"- Screenshot: {shot}", ""])
    lines.extend([
        "## Console Logs",
        "",
        *(f"- [{item['type']}] {item['text']}" for item in console),
        "" if console else "- Sin mensajes de consola.",
        "",
        "## Network Logs >= 400",
        "",
        *(f"- {item['method']} {item['url']} -> HTTP {item['status']}" for item in network),
        "" if network else "- Sin errores HTTP inesperados.",
        "",
        "## Estado Final",
        "",
        final_state,
    ])
    REPORT.write_text("\n".join(lines), encoding="utf-8")
    (ARTIFACTS / "quality-manager-evidence.json").write_text(json.dumps({
        "verdict": verdict,
        "steps": steps,
        "console": console,
        "network": network,
        "finalState": final_state,
    }, indent=2), encoding="utf-8")
    print(f"Quality Manager: {verdict}")
    print(final_state)
    return 0 if verdict == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
