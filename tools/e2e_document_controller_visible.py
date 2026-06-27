import json
from datetime import datetime, timezone
from pathlib import Path

from playwright.sync_api import sync_playwright


ROOT = Path(__file__).resolve().parents[1]
RUN_ID = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
ARTIFACTS = ROOT / "artifacts" / "e2e" / RUN_ID
REPORT = ROOT / "docs" / "e2e" / "03_DOCUMENT_CONTROLLER_E2E_REPORT.md"
BASE_URL = "http://localhost:5272"
TENANT_ID = "4c131edd-47dc-4bb4-908a-a4b5e34c85ac"
EMAIL = "document.controller@alimentos-premium.test"
PASSWORD = "DocumentCtrl!2026"


def screenshot(page, name):
    path = ARTIFACTS / f"document-controller-{name}.png"
    page.screenshot(path=str(path), full_page=True)
    return str(path.relative_to(ROOT)).replace("\\", "/")


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
            steps.append(("Abrir Document Management", "PASS", screenshot(page, "documents")))

            suffix = datetime.now(timezone.utc).strftime("%H%M%S")
            title = f"Procedimiento BPM E2E {suffix}"
            code = f"DOC-E2E-{suffix}"
            page.fill("#module-action-form #name", title)
            page.fill("#module-action-form #code", code)
            page.fill("#module-action-form #description", "Documento creado por Document Controller E2E visible.")
            page.locator("#module-action-form button[type='submit']").click()
            page.locator(".toast.success").first.wait_for(timeout=15000)
            page.get_by_text(code, exact=False).first.wait_for(timeout=15000)
            steps.append(("Crear tipo/categoria/documento", "PASS", screenshot(page, "create-document")))

            page.fill("#global-search", code)
            page.keyboard.press("Enter")
            page.get_by_text(code, exact=False).first.wait_for(timeout=15000)
            steps.append(("Buscar documento", "PASS", screenshot(page, "search-document")))

            page.locator('[data-route="audit-trail"]').first.click()
            page.get_by_text("Audit Trail", exact=False).first.wait_for(timeout=15000)
            steps.append(("Validar auditoria", "PASS", screenshot(page, "audit-trail")))

            page.locator("#logout").click()
            page.locator("#login-form").wait_for(timeout=10000)
            steps.append(("Logout", "PASS", screenshot(page, "logout")))

            errors = [item for item in console if item["type"] in {"error", "assert"}]
            if errors:
                raise RuntimeError(f"Console errors found: {errors}")
            if network:
                raise RuntimeError(f"Unexpected HTTP errors found: {network}")

            verdict = "PASS"
            final_state = "Document Controller visible browser flow completed without console or HTTP errors."
        except Exception as exc:
            final_state = str(exc)
            steps.append(("Blocking error", "FAIL", screenshot(page, "blocking-error")))
        finally:
            trace = ARTIFACTS / "document-controller-trace.zip"
            context.tracing.stop(path=str(trace))
            page.wait_for_timeout(3000)
            context.close()
            browser.close()

    lines = [
        "# Document Controller E2E Report",
        "",
        f"- Rol probado: Document Controller",
        f"- Usuario utilizado: {EMAIL}",
        f"- Fecha: {datetime.now(timezone.utc).isoformat()}",
        f"- URL: {BASE_URL}",
        "- Navegador: Chromium headed via Playwright Python",
        f"- Estado inicial: Tenant {TENANT_ID}; rol Document Controller E2E preparado.",
        f"- Trace: {str((ARTIFACTS / 'document-controller-trace.zip').relative_to(ROOT)).replace(chr(92), '/')}",
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
    (ARTIFACTS / "document-controller-evidence.json").write_text(json.dumps({
        "verdict": verdict,
        "steps": steps,
        "console": console,
        "network": network,
        "finalState": final_state,
    }, indent=2), encoding="utf-8")
    print(f"Document Controller: {verdict}")
    print(final_state)
    return 0 if verdict == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
