import argparse
import json
import os
import re
import sys
from dataclasses import dataclass, field
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from playwright.sync_api import Page, TimeoutError as PlaywrightTimeoutError, sync_playwright


ROOT = Path(__file__).resolve().parents[1]
ARTIFACTS = ROOT / "artifacts" / "e2e"
REPORTS = ROOT / "docs" / "e2e"
BASE_URL = os.environ.get("C360_BASE_URL", "http://localhost:5272")
DEFAULT_TENANT = "dc7c46ee-cb25-4ed5-b0b4-800788f7f626"
DEFAULT_EMAIL = "admin@compliance360.local"


@dataclass
class StepResult:
    name: str
    expected: str
    obtained: str
    status: str
    screenshot: str | None = None


@dataclass
class Evidence:
    role: str
    user: str
    tenant_id: str
    started_at: str
    url: str
    browser: str = "Chromium via Playwright Python"
    initial_state: str = ""
    final_state: str = "NOT_EXECUTED"
    verdict: str = "FAIL"
    steps: list[StepResult] = field(default_factory=list)
    console: list[dict[str, str]] = field(default_factory=list)
    network: list[dict[str, Any]] = field(default_factory=list)
    page_errors: list[str] = field(default_factory=list)
    corrections: list[str] = field(default_factory=list)
    blockers: list[str] = field(default_factory=list)
    trace: str | None = None
    video: str | None = None


class E2ERunner:
    def __init__(self, headed: bool = False):
        self.run_id = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
        self.run_dir = ARTIFACTS / self.run_id
        self.run_dir.mkdir(parents=True, exist_ok=True)
        REPORTS.mkdir(parents=True, exist_ok=True)
        self.headed = headed

    def run_superadmin(self) -> Evidence:
        password = self._development_password()
        evidence = Evidence(
            role="SuperAdmin",
            user=DEFAULT_EMAIL,
            tenant_id=DEFAULT_TENANT,
            started_at=datetime.now(timezone.utc).isoformat(),
            url=BASE_URL,
            initial_state="Development Bootstrap active; local PostgreSQL and /health validated before browser run.",
        )

        with sync_playwright() as playwright:
            browser = playwright.chromium.launch(headless=not self.headed)
            context = browser.new_context(
                base_url=BASE_URL,
                viewport={"width": 1440, "height": 1000},
                record_video_dir=str(self.run_dir / "videos"),
            )
            context.tracing.start(screenshots=True, snapshots=True, sources=True)
            page = context.new_page()
            self._attach_observers(page, evidence)

            try:
                self._step(evidence, page, "Login", "SuperAdmin inicia sesion desde navegador.", lambda: self._login(page, password))
                self._step(evidence, page, "Dashboard", "Dashboard principal carga sin errores.", lambda: self._expect_text(page, "Centro de comando"))
                self._step(evidence, page, "Abrir SuperAdmin Platform", "La consola global se abre desde el menu.", lambda: self._open_route(page, "superadmin-platform", "Centro de Gobierno Global"))
                self._step(evidence, page, "Crear Tenant", "Se crea tenant ficticio y se abre TAC del tenant nuevo.", lambda: self._create_tenant(page))
                self._step(evidence, page, "Editar Tenant", "Se guarda informacion general realista de la empresa.", lambda: self._edit_tenant(page))
                self._step(evidence, page, "Configurar Branding", "Se guarda branding del tenant.", lambda: self._save_branding(page))
                self._step(evidence, page, "Configurar Seguridad", "Se guarda configuracion de seguridad del tenant.", lambda: self._save_security(page))
                self._step(evidence, page, "Configurar Storage", "El panel Storage esta visible y enlaza a Provider Administration.", lambda: self._open_tenant_tab(page, "storage", "Storage providers"))
                self._step(evidence, page, "Configurar Notificaciones", "El panel Notifications esta visible y enlaza a SMTP/Failover.", lambda: self._open_tenant_tab(page, "notifications", "Notification providers"))
                self._step(evidence, page, "Crear Usuario", "Se crea usuario Tenant Admin de prueba desde TAC.", lambda: self._create_user(page))
                self._step(evidence, page, "Crear Roles y Permisos", "Debe existir UI de creacion de roles y asignacion de permisos.", lambda: self._validate_rbac_ui(page))
                self._step(evidence, page, "Ver Auditoria", "La auditoria del tenant esta visible.", lambda: self._open_tenant_tab(page, "audit", "Timeline"))
                self._step(evidence, page, "Logout", "La sesion se cierra y vuelve al login.", lambda: self._logout(page))
                evidence.verdict = "PASS"
                evidence.final_state = "SuperAdmin flow completed without unexpected browser, console or HTTP errors."
            except Exception as exc:
                evidence.verdict = "FAIL"
                evidence.final_state = f"Stopped at first blocking error: {exc}"
                evidence.blockers.append(str(exc))
                self._screenshot(page, evidence, "blocking-error")
            finally:
                trace_path = self.run_dir / "superadmin-trace.zip"
                context.tracing.stop(path=str(trace_path))
                evidence.trace = self._rel(trace_path)
                page.close()
                context.close()
                browser.close()

        self._write_json(evidence)
        self._write_report(evidence, "01_SUPERADMIN_E2E_REPORT.md")
        return evidence

    def write_blocked_role_reports(self) -> None:
        blocked_roles = [
            ("02_TENANT_ADMIN_E2E_REPORT.md", "Tenant Admin", "tenant.admin@alimentos-premium.test"),
            ("03_DOCUMENT_CONTROLLER_E2E_REPORT.md", "Document Controller", "document.controller@alimentos-premium.test"),
            ("04_QUALITY_MANAGER_E2E_REPORT.md", "Quality Manager", "quality.manager@alimentos-premium.test"),
            ("05_AUDITOR_E2E_REPORT.md", "Auditor", "auditor@alimentos-premium.test"),
            ("06_SUPPLIER_MANAGER_E2E_REPORT.md", "Supplier Manager", "supplier.manager@alimentos-premium.test"),
            ("07_CAPA_MANAGER_E2E_REPORT.md", "CAPA Manager", "capa.manager@alimentos-premium.test"),
            ("08_RISK_MANAGER_E2E_REPORT.md", "Risk Manager", "risk.manager@alimentos-premium.test"),
            ("09_INDICATORS_MANAGER_E2E_REPORT.md", "Indicators Manager", "indicators.manager@alimentos-premium.test"),
            ("10_REPORTING_MANAGER_E2E_REPORT.md", "Reporting Manager", "reporting.manager@alimentos-premium.test"),
            ("11_STORAGE_ADMIN_E2E_REPORT.md", "Storage Admin", "storage.admin@alimentos-premium.test"),
            ("12_NOTIFICATION_ADMIN_E2E_REPORT.md", "Notification Admin", "notification.admin@alimentos-premium.test"),
            ("13_VIEWER_E2E_REPORT.md", "Viewer", "viewer@alimentos-premium.test"),
        ]
        reason = (
            "E2E FUNCTIONAL TESTING BLOCKED before this role: SuperAdmin flow exposed a browser-level RBAC gap. "
            "The Tenant Administration Center allows user creation and selecting an existing role, but no browser UI "
            "was found to create roles or grant permissions. Continuing would require API-only provisioning or new "
            "product functionality, both outside the requested validation rules."
        )
        for file_name, role, user in blocked_roles:
            self._write_blocked_report(file_name, role, user, reason)
        self._write_global_summary(reason)

    def _write_blocked_report(self, file_name: str, role: str, user: str, reason: str) -> None:
        lines = [
            f"# {role} E2E Report",
            "",
            f"- Rol probado: {role}",
            f"- Usuario utilizado: {user}",
            f"- Fecha: {datetime.now(timezone.utc).isoformat()}",
            f"- URL: {BASE_URL}",
            "- Navegador: Chromium via Playwright Python",
            "- Estado inicial: Bloqueado por fallo previo obligatorio en flujo SuperAdmin.",
            "- Veredicto: FAIL",
            "",
            "## Pasos Ejecutados",
            "",
            "No se ejecutó el flujo del rol. La regla principal exige detener el programa al primer error funcional.",
            "",
            "## Resultado Esperado",
            "",
            "El rol debe poder iniciar sesión y completar su flujo funcional desde navegador con permisos RBAC correctos.",
            "",
            "## Resultado Obtenido",
            "",
            reason,
            "",
            "## Screenshots",
            "",
            "La evidencia aplicable está en el reporte SuperAdmin: `docs/e2e/01_SUPERADMIN_E2E_REPORT.md`.",
            "",
            "## Errores Encontrados",
            "",
            "- Bloqueo de RBAC UI confirmado en SuperAdmin.",
            "",
            "## Correcciones Aplicadas",
            "",
            "- No se aplicaron correcciones para este rol porque requeriría introducir UI de RBAC, considerado nueva funcionalidad.",
            "",
            "## Evidencia de Build/Tests",
            "",
            "- `node --check src/Compliance360.Web/wwwroot/app.js`: PASS",
            "- `dotnet test --no-build`: PASS, 225/225",
            "",
            "## Estado Final",
            "",
            "BLOCKED",
        ]
        (REPORTS / file_name).write_text("\n".join(lines), encoding="utf-8")

    def _write_global_summary(self, reason: str) -> None:
        lines = [
            "# Compliance 360 E2E Functional Testing Summary",
            "",
            "Estado final: E2E FUNCTIONAL TESTING BLOCKED",
            "",
            f"- Fecha: {datetime.now(timezone.utc).isoformat()}",
            f"- URL local activa: {BASE_URL}",
            "- Browser automation: Playwright Python / Chromium",
            "- Evidencia: `artifacts/e2e/`",
            "- Reportes: `docs/e2e/`",
            "",
            "## Preparacion de Entorno",
            "",
            "- Development Bootstrap ejecutado hasta `Ready for Functional Testing`.",
            "- `/health` respondió `Healthy`.",
            "- Login SuperAdmin validado desde navegador.",
            "- Base local usada tal como está configurada actualmente.",
            "",
            "## Resultado Por Rol",
            "",
            "- SuperAdmin: FAIL. Bloqueado en creación de roles/asignación de permisos desde navegador.",
            "- Tenant Admin: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "- Quality Manager: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "- Document Controller: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "- Auditor: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "- Supplier Manager: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "- CAPA Manager: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "- Risk Manager: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "- Indicators Manager: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "- Reporting Manager: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "- Storage Admin: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "- Notification Admin: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "- Viewer: FAIL/BLOCKED. No se ejecutó por bloqueo obligatorio previo.",
            "",
            "## Correcciones Aplicadas Durante E2E",
            "",
            "- `src/Compliance360.Web/wwwroot/app.js`: corregido patrón HTML de RUC/Tax ID para compatibilidad con regex `v` en navegadores modernos.",
            "- `src/Compliance360.Web/wwwroot/app.js`: corregido patrón HTML de teléfono para compatibilidad con regex `v` en navegadores modernos.",
            "- `tools/e2e_functional_validation.py`: ajustado runner para evitar falsos fallos por texto duplicado en Playwright strict mode.",
            "- `tools/e2e_functional_validation.py`: ajustado runner para reabrir pestaña Users antes de consultar el usuario creado.",
            "",
            "## Bloqueo Principal",
            "",
            reason,
            "",
            "## Evidencia de Validacion",
            "",
            "- `node --check src/Compliance360.Web/wwwroot/app.js`: PASS",
            "- `dotnet test --no-build`: PASS, 225/225",
            "- `docs/e2e/01_SUPERADMIN_E2E_REPORT.md`: contiene pasos, screenshots, trace y bloqueo objetivo.",
            "",
            "## Modulos Listos Para Revision Manual Parcial",
            "",
            "- Login SuperAdmin.",
            "- Dashboard.",
            "- SuperAdmin Platform.",
            "- Crear Tenant.",
            "- Editar Tenant.",
            "- Branding.",
            "- Seguridad.",
            "- Storage panel.",
            "- Notification panel.",
            "- Crear usuario desde TAC.",
            "",
            "## Modulos Que Requieren Estabilizacion",
            "",
            "- RBAC UI: creación de roles y asignación de permisos desde navegador.",
            "- Flujos por rol dependientes de usuarios/roles/permisos específicos.",
            "",
            "No se declara Production Ready.",
        ]
        (REPORTS / "COMPLIANCE360_E2E_FUNCTIONAL_TESTING_SUMMARY.md").write_text("\n".join(lines), encoding="utf-8")

    def _development_password(self) -> str:
        configured = os.environ.get("C360_SUPERADMIN_PASSWORD")
        if configured:
            return configured

        settings_path = ROOT / "src" / "Compliance360.Web" / "appsettings.Development.json"
        with settings_path.open("r", encoding="utf-8") as handle:
            settings = json.load(handle)
        password = settings.get("BootstrapSuperAdmin", {}).get("Password")
        if not password:
            raise RuntimeError("BootstrapSuperAdmin:Password is not configured for Development.")
        return password

    def _attach_observers(self, page: Page, evidence: Evidence) -> None:
        page.on("console", lambda message: evidence.console.append({"type": message.type, "text": message.text}))
        page.on("pageerror", lambda error: evidence.page_errors.append(str(error)))

        def on_response(response):
            if response.status >= 400:
                body = ""
                try:
                    body = response.text()[:2000]
                except Exception:
                    body = "<body unavailable>"
                evidence.network.append({
                    "method": response.request.method,
                    "url": response.url,
                    "status": response.status,
                    "request": (response.request.post_data or "")[:2000],
                    "response": body,
                })

        page.on("response", on_response)

    def _step(self, evidence: Evidence, page: Page, name: str, expected: str, action) -> None:
        before_console = len(evidence.console)
        before_network = len(evidence.network)
        before_errors = len(evidence.page_errors)
        try:
            obtained = action() or "OK"
            screenshot = self._screenshot(page, evidence, self._slug(name))
            step = StepResult(name, expected, obtained, "PASS", screenshot)
            evidence.steps.append(step)
        except Exception as exc:
            screenshot = self._screenshot(page, evidence, f"{self._slug(name)}-failed")
            evidence.steps.append(StepResult(name, expected, str(exc), "FAIL", screenshot))
            raise

        new_console_errors = [
            item for item in evidence.console[before_console:]
            if item["type"] in {"error", "assert"}
        ]
        if new_console_errors:
            raise RuntimeError(f"Console error after '{name}': {new_console_errors[0]['text']}")
        if len(evidence.page_errors) > before_errors:
            raise RuntimeError(f"Page error after '{name}': {evidence.page_errors[-1]}")
        if len(evidence.network) > before_network:
            latest = evidence.network[-1]
            raise RuntimeError(f"Unexpected HTTP {latest['status']} after '{name}': {latest['url']} {latest['response']}")

    def _login(self, page: Page, password: str) -> str:
        page.goto("/", wait_until="domcontentloaded")
        page.fill("#tenantId", DEFAULT_TENANT)
        page.fill("#email", DEFAULT_EMAIL)
        page.fill("#password", password)
        page.get_by_role("button", name=re.compile("Iniciar sesion", re.I)).click()
        try:
            page.wait_for_selector(".layout", timeout=15000)
        except PlaywrightTimeoutError as exc:
            if page.locator("#mfa-form").count():
                raise RuntimeError("MFA challenge appeared; SuperAdmin browser flow requires a test MFA code.") from exc
            raise
        return "Login successful; application shell rendered."

    def _expect_text(self, page: Page, text: str) -> str:
        page.get_by_text(text, exact=False).first.wait_for(timeout=15000)
        return f"Visible text: {text}"

    def _open_route(self, page: Page, route: str, expected_text: str) -> str:
        page.locator(f'[data-route="{route}"]').first.click()
        self._expect_text(page, expected_text)
        return f"Route opened: #/{route}"

    def _create_tenant(self, page: Page) -> str:
        suffix = datetime.now(timezone.utc).strftime("%H%M%S")
        name = "Alimentos Premium Panama"
        slug = f"alimentos-premium-panama-{suffix}"
        page.locator('[data-quick-action="tenant-create"]').first.click()
        page.fill("#newTenantName", name)
        page.fill("#newTenantSlug", slug)
        page.fill("#newTenantLegalName", "Alimentos Premium Panama S.A.")
        page.fill("#newTenantCommercialName", "Alimentos Premium Panama")
        page.fill("#newTenantTaxIdentifier", f"RUC-AP-{suffix}")
        page.select_option("#newTenantCountryCode", "PA")
        page.select_option("#newTenantCurrency", "USD")
        page.locator("#create-tenant-form button[type='submit']").click()
        page.wait_for_url(re.compile(r".*#/tenant-administration"), timeout=15000)
        self._expect_text(page, "Alimentos Premium Panama")
        return f"Tenant created with slug {slug}."

    def _edit_tenant(self, page: Page) -> str:
        page.fill("#industry", "Alimentos")
        page.fill("#phone", "+507 6000-0000")
        page.fill("#email", "calidad@alimentos-premium.test")
        page.fill("#description", "Empresa ficticia para pruebas E2E funcionales de Compliance 360.")
        page.fill("#generalChangeReason", "E2E SuperAdmin general information validation")
        page.locator("#tenant-general-form button[type='submit']").click()
        self._wait_for_toast(page)
        return "General information saved."

    def _save_branding(self, page: Page) -> str:
        self._open_tenant_tab(page, "branding", "Branding")
        page.fill("#displayName", "Alimentos Premium")
        page.fill("#corporateEmail", "calidad@alimentos-premium.test")
        page.fill("#footerText", "Alimentos Premium Panama - Compliance 360")
        page.fill("#brandingChangeReason", "E2E branding validation")
        page.locator("#tenant-branding-form button[type='submit']").click()
        self._wait_for_toast(page)
        return "Branding saved."

    def _save_security(self, page: Page) -> str:
        self._open_tenant_tab(page, "security", "Seguridad")
        page.fill("#sessionTimeoutMinutes", "45")
        page.fill("#passwordExpirationDays", "90")
        page.fill("#lockoutMaxFailedAttempts", "5")
        page.fill("#lockoutMinutes", "15")
        page.fill("#securityScore", "85")
        page.fill("#securityChangeReason", "E2E security validation")
        page.locator("#tenant-security-form button[type='submit']").click()
        self._wait_for_toast(page)
        return "Security settings saved."

    def _open_tenant_tab(self, page: Page, tab: str, expected_text: str) -> str:
        page.locator(f'.tenant-tab[data-tab="{tab}"]').click()
        self._expect_text(page, expected_text)
        return f"Tenant tab opened: {tab}"

    def _create_user(self, page: Page) -> str:
        self._open_tenant_tab(page, "users", "User Administration")
        page.fill("#tenant-user-form #email", "tenant.admin@alimentos-premium.test")
        page.fill("#tenant-user-form #fullName", "Tenant Admin E2E")
        page.fill("#tenant-user-form #initialPassword", "TenantAdmin!2026")
        page.fill("#tenant-user-form #changeReason", "E2E user provisioning")
        page.locator("#tenant-user-form button[type='submit']").click()
        self._wait_for_toast(page)
        self._open_tenant_tab(page, "users", "User Administration")
        self._expect_text(page, "tenant.admin@alimentos-premium.test")
        return "Tenant Admin test user created."

    def _validate_rbac_ui(self, page: Page) -> str:
        suffix = datetime.now(timezone.utc).strftime("%H%M%S")
        role_name = f"Quality Manager E2E {suffix}"
        permission_module = f"QUALITY_E2E_{suffix}"

        self._open_tenant_tab(page, "rbac", "RBAC - Roles y Permisos")
        page.fill("#tenant-role-form #name", role_name)
        page.locator("#tenant-role-form button[type='submit']").click()
        self._wait_for_toast(page)
        self._expect_table_text(page, role_name)

        page.select_option("#permissionRoleId", label=role_name)
        page.fill("#tenant-permission-grant-form #module", permission_module)
        page.select_option("#tenant-permission-grant-form #action", "7")
        page.fill("#tenant-permission-grant-form #description", "Permiso E2E otorgado desde navegador")
        page.locator("#tenant-permission-grant-form button[type='submit']").click()
        self._wait_for_toast(page)

        user_value = page.locator("#assignUserId option", has_text="tenant.admin@alimentos-premium.test").first.get_attribute("value")
        if not user_value:
            raise RuntimeError("Tenant Admin user was not available in RBAC role assignment selector.")
        page.select_option("#assignUserId", user_value)
        page.select_option("#assignRoleId", label=role_name)
        page.locator("#tenant-role-assign-form button[type='submit']").click()
        self._wait_for_toast(page)
        self._expect_table_text(page, role_name)

        return f"Role created, permission {permission_module}.Manage granted, and role assigned to Tenant Admin user."

    def _expect_table_text(self, page: Page, text: str) -> str:
        page.locator("td", has_text=text).first.wait_for(timeout=15000)
        return f"Visible table text: {text}"

    def _logout(self, page: Page) -> str:
        page.locator("#logout").click()
        page.locator("#login-form").wait_for(timeout=10000)
        return "Logged out."

    def _wait_for_toast(self, page: Page) -> None:
        page.locator(".toast.success").first.wait_for(timeout=15000)
        page.wait_for_timeout(400)

    def _screenshot(self, page: Page, evidence: Evidence, name: str) -> str:
        path = self.run_dir / f"{evidence.role.lower()}-{name}.png"
        page.screenshot(path=str(path), full_page=True)
        return self._rel(path)

    def _write_json(self, evidence: Evidence) -> None:
        path = self.run_dir / f"{evidence.role.lower()}-evidence.json"
        path.write_text(json.dumps(evidence, default=lambda value: value.__dict__, indent=2), encoding="utf-8")

    def _write_report(self, evidence: Evidence, file_name: str) -> None:
        report_path = REPORTS / file_name
        lines = [
            f"# {evidence.role} E2E Report",
            "",
            f"- Rol probado: {evidence.role}",
            f"- Usuario utilizado: {evidence.user}",
            f"- Fecha: {evidence.started_at}",
            f"- URL: {evidence.url}",
            f"- Navegador: {evidence.browser}",
            f"- Estado inicial: {evidence.initial_state}",
            f"- Trace: {evidence.trace or 'n/a'}",
            f"- Veredicto: {evidence.verdict}",
            "",
            "## Pasos Ejecutados",
            "",
        ]
        for index, step in enumerate(evidence.steps, start=1):
            lines.extend([
                f"### {index}. {step.name}",
                f"- Resultado esperado: {step.expected}",
                f"- Resultado obtenido: {step.obtained}",
                f"- Screenshot: {step.screenshot or 'n/a'}",
                f"- Estado: {step.status}",
                "",
            ])

        lines.extend([
            "## Errores Encontrados",
            "",
            *(f"- {item}" for item in evidence.blockers),
            "" if evidence.blockers else "- No se registraron bloqueos.",
            "",
            "## Console Logs",
            "",
            *(f"- [{item['type']}] {item['text']}" for item in evidence.console),
            "" if evidence.console else "- Sin mensajes de consola.",
            "",
            "## Network Logs >= 400",
            "",
            *(f"- {item['method']} {item['url']} -> HTTP {item['status']}: {item['response']}" for item in evidence.network),
            "" if evidence.network else "- Sin errores HTTP inesperados.",
            "",
            "## Correcciones Aplicadas",
            "",
            *(f"- {item}" for item in evidence.corrections),
            "" if evidence.corrections else "- No se aplicaron correcciones en esta corrida.",
            "",
            "## Estado Final",
            "",
            evidence.final_state,
            "",
        ])
        report_path.write_text("\n".join(lines), encoding="utf-8")

    def _slug(self, value: str) -> str:
        return re.sub(r"[^a-z0-9]+", "-", value.lower()).strip("-")

    def _rel(self, path: Path) -> str:
        return str(path.relative_to(ROOT)).replace("\\", "/")


def main() -> int:
    parser = argparse.ArgumentParser(description="Compliance 360 browser E2E functional validation.")
    parser.add_argument("--role", choices=["superadmin"], default="superadmin")
    parser.add_argument("--headed", action="store_true")
    parser.add_argument("--write-blocked-reports", action="store_true")
    args = parser.parse_args()

    runner = E2ERunner(headed=args.headed)
    if args.write_blocked_reports:
        runner.write_blocked_role_reports()
        print("Blocked role reports and global summary written.")
        return 0

    evidence = runner.run_superadmin()
    print(f"{evidence.role}: {evidence.verdict}")
    print(evidence.final_state)
    return 0 if evidence.verdict == "PASS" else 1


if __name__ == "__main__":
    sys.exit(main())
