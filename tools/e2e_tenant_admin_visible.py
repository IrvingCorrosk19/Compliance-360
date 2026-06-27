import json
from datetime import datetime, timezone
from pathlib import Path

from playwright.sync_api import sync_playwright


ROOT = Path(__file__).resolve().parents[1]
RUN_ID = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
ARTIFACTS = ROOT / "artifacts" / "e2e" / RUN_ID
REPORT = ROOT / "docs" / "e2e" / "02_TENANT_ADMIN_E2E_REPORT.md"
BASE_URL = "http://localhost:5272"
TENANT_ID = "4c131edd-47dc-4bb4-908a-a4b5e34c85ac"
EMAIL = "tenant.admin@alimentos-premium.test"
PASSWORD = "TenantAdmin!2026"


def screenshot(page, name):
    path = ARTIFACTS / f"tenant-admin-{name}.png"
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

            page.locator('[data-route="tenant-administration"]').first.click()
            page.get_by_text("Tenant Administration Center", exact=False).first.wait_for(timeout=15000)
            page.get_by_text("Alimentos Premium", exact=False).first.wait_for(timeout=15000)
            steps.append(("Abrir TAC propio", "PASS", screenshot(page, "tenant-administration")))

            page.fill("#city", "Panama")
            page.fill("#province", "Panama")
            page.fill("#generalChangeReason", "Tenant Admin visible E2E update")
            page.locator("#tenant-general-form button[type='submit']").click()
            page.locator(".toast.success").first.wait_for(timeout=15000)
            steps.append(("Configurar empresa", "PASS", screenshot(page, "general-information")))

            page.locator('.tenant-tab[data-tab="users"]').click()
            page.get_by_text("User Administration", exact=False).first.wait_for(timeout=15000)
            suffix = datetime.now(timezone.utc).strftime("%H%M%S")
            viewer_email = f"viewer.visible.{suffix}@alimentos-premium.test"
            page.fill("#tenant-user-form #email", viewer_email)
            page.fill("#tenant-user-form #fullName", "Viewer Visible E2E")
            page.fill("#tenant-user-form #initialPassword", "ViewerVisible!2026")
            page.fill("#tenant-user-form #changeReason", "Tenant Admin visible E2E user")
            page.locator("#tenant-user-form button[type='submit']").click()
            page.locator(".toast.success").first.wait_for(timeout=15000)
            page.locator('.tenant-tab[data-tab="users"]').click()
            page.get_by_text(viewer_email, exact=False).first.wait_for(timeout=15000)
            steps.append(("Crear usuario", "PASS", screenshot(page, "create-user")))

            page.locator('.tenant-tab[data-tab="rbac"]').click()
            page.get_by_text("RBAC - Roles y Permisos", exact=False).first.wait_for(timeout=15000)
            role_name = f"Viewer Visible {suffix}"
            page.fill("#tenant-role-form #name", role_name)
            page.locator("#tenant-role-form button[type='submit']").click()
            page.locator(".toast.success").first.wait_for(timeout=15000)
            page.locator("td", has_text=role_name).first.wait_for(timeout=15000)
            steps.append(("Crear rol", "PASS", screenshot(page, "create-role")))

            page.locator('.tenant-tab[data-tab="security"]').click()
            page.get_by_text("Seguridad", exact=False).first.wait_for(timeout=15000)
            page.fill("#securityChangeReason", "Tenant Admin visible E2E security review")
            page.locator("#tenant-security-form button[type='submit']").click()
            page.locator(".toast.success").first.wait_for(timeout=15000)
            steps.append(("Configurar MFA/seguridad", "PASS", screenshot(page, "security")))

            page.locator('.tenant-tab[data-tab="audit"]').click()
            page.get_by_text("Timeline", exact=False).first.wait_for(timeout=15000)
            steps.append(("Ver auditoria", "PASS", screenshot(page, "audit")))

            page.locator("#logout").click()
            page.locator("#login-form").wait_for(timeout=10000)
            steps.append(("Logout", "PASS", screenshot(page, "logout")))

            errors = [item for item in console if item["type"] in {"error", "assert"}]
            if errors:
                raise RuntimeError(f"Console errors found: {errors}")
            if network:
                raise RuntimeError(f"Unexpected HTTP errors found: {network}")

            verdict = "PASS"
            final_state = "Tenant Admin visible browser flow completed without console or HTTP errors."
        except Exception as exc:
            final_state = str(exc)
            steps.append(("Blocking error", "FAIL", screenshot(page, "blocking-error")))
        finally:
            trace = ARTIFACTS / "tenant-admin-trace.zip"
            context.tracing.stop(path=str(trace))
            page.wait_for_timeout(3000)
            context.close()
            browser.close()

    lines = [
        "# Tenant Admin E2E Report",
        "",
        f"- Rol probado: Tenant Admin",
        f"- Usuario utilizado: {EMAIL}",
        f"- Fecha: {datetime.now(timezone.utc).isoformat()}",
        f"- URL: {BASE_URL}",
        "- Navegador: Chromium headed via Playwright Python",
        f"- Estado inicial: Tenant {TENANT_ID}; rol Tenant Admin E2E preparado.",
        f"- Trace: {str((ARTIFACTS / 'tenant-admin-trace.zip').relative_to(ROOT)).replace(chr(92), '/')}",
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
    (ARTIFACTS / "tenant-admin-evidence.json").write_text(json.dumps({
        "verdict": verdict,
        "steps": steps,
        "console": console,
        "network": network,
        "finalState": final_state,
    }, indent=2), encoding="utf-8")
    print(f"Tenant Admin: {verdict}")
    print(final_state)
    return 0 if verdict == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
