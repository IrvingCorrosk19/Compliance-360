import json
from datetime import datetime, timezone
from pathlib import Path

from playwright.sync_api import sync_playwright


ROOT = Path(__file__).resolve().parents[1]
BASE_URL = "http://localhost:5272"
TENANT_ID = "4c131edd-47dc-4bb4-908a-a4b5e34c85ac"
EMAIL = "viewer@alimentos-premium.test"
PASSWORD = "ViewerRole!2026"


def main():
    run_id = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    artifacts = ROOT / "artifacts" / "e2e" / run_id
    artifacts.mkdir(parents=True, exist_ok=True)
    console = []
    network = []
    steps = []
    verdict = "FAIL"
    final_state = ""

    def shot(page, name):
        path = artifacts / f"viewer-{name}.png"
        page.screenshot(path=str(path), full_page=True)
        return str(path.relative_to(ROOT)).replace("\\", "/")

    with sync_playwright() as p:
        browser = p.chromium.launch(headless=False, slow_mo=700)
        context = browser.new_context(base_url=BASE_URL, viewport={"width": 1440, "height": 1000}, record_video_dir=str(artifacts / "videos"))
        context.tracing.start(screenshots=True, snapshots=True, sources=True)
        page = context.new_page()
        page.on("console", lambda message: console.append({"type": message.type, "text": message.text}))
        page.on("response", lambda response: network.append({"method": response.request.method, "url": response.url, "status": response.status}) if response.status >= 400 else None)
        try:
            page.goto("/", wait_until="domcontentloaded")
            page.fill("#tenantId", TENANT_ID)
            page.fill("#email", EMAIL)
            page.fill("#password", PASSWORD)
            page.get_by_role("button", name="Iniciar sesion").click()
            page.wait_for_selector(".layout", timeout=15000)
            steps.append(("Login", "PASS", shot(page, "login")))

            hidden_routes = ["documents", "suppliers", "configuration", "tenant-administration", "superadmin-platform"]
            for route in hidden_routes:
                if page.locator(f'[data-route="{route}"]').count() > 0:
                    raise RuntimeError(f"Viewer should not see route {route}")
            steps.append(("Menu solo lectura", "PASS", shot(page, "menu")))

            for index, route in enumerate(["reports", "capa", "risks", "indicators", "audit-trail"], start=1):
                page.locator(f'[data-route="{route}"]').first.click()
                page.wait_for_load_state("domcontentloaded")
                page.wait_for_timeout(1000)
                if route in {"capa", "risks", "indicators"}:
                    page.get_by_text("Modo solo lectura", exact=False).first.wait_for(timeout=10000)
                    if page.locator("#module-action-form").count() != 0:
                        raise RuntimeError(f"Viewer should not see create form in {route}")
                steps.append((f"Consultar {route}", "PASS", shot(page, f"step-{index}-{route}")))

            page.locator("#logout").click()
            page.locator("#login-form").wait_for(timeout=10000)
            steps.append(("Logout", "PASS", shot(page, "logout")))

            errors = [item for item in console if item["type"] in {"error", "assert"}]
            if errors:
                raise RuntimeError(f"Console errors found: {errors}")
            if network:
                raise RuntimeError(f"Unexpected HTTP errors found: {network}")
            verdict = "PASS"
            final_state = "Viewer solo lectura validado sin formularios de creacion, configuracion ni errores HTTP/consola."
        except Exception as exc:
            final_state = str(exc)
            steps.append(("Blocking error", "FAIL", shot(page, "blocking-error")))
        finally:
            trace = artifacts / "viewer-trace.zip"
            context.tracing.stop(path=str(trace))
            page.wait_for_timeout(3000)
            context.close()
            browser.close()

    report = ROOT / "docs" / "e2e" / "13_VIEWER_E2E_REPORT.md"
    lines = [
        "# Viewer E2E Report",
        "",
        "- Rol probado: Viewer",
        f"- Usuario utilizado: {EMAIL}",
        f"- Fecha: {datetime.now(timezone.utc).isoformat()}",
        f"- URL: {BASE_URL}",
        "- Navegador: Chromium headed via Playwright Python",
        "- Estado inicial: permisos de solo lectura asignados.",
        f"- Trace: {str(trace.relative_to(ROOT)).replace(chr(92), '/')}",
        f"- Veredicto: {verdict}",
        "",
        "## Pasos Ejecutados",
        "",
    ]
    for index, (name, status, screenshot) in enumerate(steps, start=1):
        lines.extend([f"### {index}. {name}", f"- Estado: {status}", f"- Screenshot: {screenshot}", ""])
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
    report.write_text("\n".join(lines), encoding="utf-8")
    (artifacts / "viewer-evidence.json").write_text(json.dumps({"verdict": verdict, "steps": steps, "console": console, "network": network, "finalState": final_state}, indent=2), encoding="utf-8")
    print(f"Viewer: {verdict}")
    print(final_state)
    return 0 if verdict == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
