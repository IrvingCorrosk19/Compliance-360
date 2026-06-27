import json
from datetime import datetime, timezone
from pathlib import Path

from playwright.sync_api import sync_playwright


ROOT = Path(__file__).resolve().parents[1]
BASE_URL = "http://localhost:5272"
TENANT_ID = "4c131edd-47dc-4bb4-908a-a4b5e34c85ac"


def run_role(role_key, role_name, report_file, email, password, steps):
    run_id = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    artifacts = ROOT / "artifacts" / "e2e" / run_id
    report = ROOT / "docs" / "e2e" / report_file
    artifacts.mkdir(parents=True, exist_ok=True)
    console = []
    network = []
    executed = []
    verdict = "FAIL"
    final_state = "Not completed."

    def screenshot(page, name):
        path = artifacts / f"{role_key}-{name}.png"
        page.screenshot(path=str(path), full_page=True)
        return str(path.relative_to(ROOT)).replace("\\", "/")

    with sync_playwright() as p:
        browser = p.chromium.launch(headless=False, slow_mo=700)
        context = browser.new_context(
            base_url=BASE_URL,
            viewport={"width": 1440, "height": 1000},
            record_video_dir=str(artifacts / "videos"),
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
            page.fill("#email", email)
            page.fill("#password", password)
            page.get_by_role("button", name="Iniciar sesion").click()
            page.wait_for_selector(".layout", timeout=15000)
            executed.append(("Login", "PASS", screenshot(page, "login")))

            page.get_by_text("Centro de comando", exact=False).first.wait_for(timeout=15000)
            executed.append(("Dashboard", "PASS", screenshot(page, "dashboard")))

            for index, step in enumerate(steps, start=1):
                page.locator(f'[data-route="{step["route"]}"]').first.click()
                page.get_by_text(step["expect"], exact=False).first.wait_for(timeout=15000)
                if step.get("create"):
                    code = f"{step['code_prefix']}-{suffix}"
                    page.fill("#module-action-form #name", f"{step['title']} {suffix}")
                    page.fill("#module-action-form #code", code)
                    if page.locator("#module-action-form #description").count():
                        page.fill("#module-action-form #description", step.get("description", f"{role_name} visible E2E"))
                    page.locator("#module-action-form button[type='submit']").click()
                    page.locator(".toast.success").first.wait_for(timeout=15000)
                    page.get_by_text(code, exact=False).first.wait_for(timeout=15000)
                for click in step.get("clicks", []):
                    page.locator(click["selector"]).click()
                    page.locator(".toast.success").first.wait_for(timeout=15000)
                    if click.get("expect"):
                        page.get_by_text(click["expect"], exact=False).first.wait_for(timeout=15000)
                executed.append((step["name"], "PASS", screenshot(page, f"step-{index}")))

            page.locator("#logout").click()
            page.locator("#login-form").wait_for(timeout=10000)
            executed.append(("Logout", "PASS", screenshot(page, "logout")))

            errors = [item for item in console if item["type"] in {"error", "assert"}]
            if errors:
                raise RuntimeError(f"Console errors found: {errors}")
            if network:
                raise RuntimeError(f"Unexpected HTTP errors found: {network}")

            verdict = "PASS"
            final_state = f"{role_name} visible browser flow completed without console or HTTP errors."
        except Exception as exc:
            final_state = str(exc)
            executed.append(("Blocking error", "FAIL", screenshot(page, "blocking-error")))
        finally:
            trace = artifacts / f"{role_key}-trace.zip"
            context.tracing.stop(path=str(trace))
            page.wait_for_timeout(3000)
            context.close()
            browser.close()

    lines = [
        f"# {role_name} E2E Report",
        "",
        f"- Rol probado: {role_name}",
        f"- Usuario utilizado: {email}",
        f"- Fecha: {datetime.now(timezone.utc).isoformat()}",
        f"- URL: {BASE_URL}",
        "- Navegador: Chromium headed via Playwright Python",
        f"- Estado inicial: Tenant {TENANT_ID}; rol {role_name} preparado.",
        f"- Trace: {str((artifacts / f'{role_key}-trace.zip').relative_to(ROOT)).replace(chr(92), '/')}",
        f"- Veredicto: {verdict}",
        "",
        "## Pasos Ejecutados",
        "",
    ]
    for index, (name, status, shot) in enumerate(executed, start=1):
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
    report.write_text("\n".join(lines), encoding="utf-8")
    (artifacts / f"{role_key}-evidence.json").write_text(json.dumps({
        "verdict": verdict,
        "steps": executed,
        "console": console,
        "network": network,
        "finalState": final_state,
    }, indent=2), encoding="utf-8")
    print(f"{role_name}: {verdict}")
    print(final_state)
    return 0 if verdict == "PASS" else 1
