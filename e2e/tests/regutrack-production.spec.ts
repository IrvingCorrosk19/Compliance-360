import { test, expect } from "@playwright/test";
import * as path from "path";
import * as fs from "fs";
import {
  TENANT, PLATFORM_TENANT, user, data,
  login, logout, go, browserApi, saveFunctionalReport,
  StepResult,
} from "./helpers";

const XLSX = path.resolve(__dirname, "..", "..", "REGUTRACK 02JUN26 MG.xlsx");

test.describe.configure({ mode: "serial" });

test.describe("REGUTRACK Production E2E", () => {
  test("01 Platform Admin — QMS hidden and tenant APIs protected", async ({ page }) => {
    const u = data.platform;
    const steps: StepResult[] = [];
    await login(page, PLATFORM_TENANT, u.email, u.password);
    steps.push({ step: "Login platform", expected: "OK", actual: "OK", pass: true });

    const visible = await page.$$eval(".nav-button", els => els.map(e => (e as HTMLElement).dataset.route));
    const businessHidden = !visible.includes("template-builder")
      && !visible.includes("documents")
      && !visible.includes("capa")
      && !visible.includes("risks")
      && !visible.includes("reports");
    steps.push({ step: "Tenant business modules hidden", expected: "hidden", actual: visible.join(","), pass: businessHidden });
    expect(businessHidden).toBe(true);

    await go(page, "superadmin-platform");
    await expect(page.locator("h2").filter({ hasText: /Global Governance Center|Centro de Gobierno Global/i })).toBeVisible({ timeout: 20000 });
    steps.push({ step: "Platform center", expected: "Visible", actual: "OK", pass: true });

    for (const protectedApi of [
      `/tenants/${TENANT}/documents?page=1&pageSize=1`,
      `/tenants/${TENANT}/capas/dashboard`,
      `/tenants/${TENANT}/risks/dashboard`,
      `/tenants/${TENANT}/form-templates`,
      `/tenants/${TENANT}/enterprise-workspaces`,
    ]) {
      const res = await browserApi(page, "GET", protectedApi);
      const ok = res.status === 403;
      steps.push({ step: `Tenant API protected ${protectedApi.split("/").slice(-2).join("/")}`, expected: "403", actual: String(res.status), pass: ok });
      expect(ok).toBe(true);
    }

    await logout(page);
    const summary = await saveFunctionalReport(u.role, u.email, steps, page);
    expect(summary.verdict).toBe("PASS");
  });

  test("02 Tenant Admin — TAC password reset + RA console", async ({ page }) => {
    const u = user("Tenant Administrator");
    const steps: StepResult[] = [];
    await login(page, TENANT, u.email, u.password);
    steps.push({ step: "Login tenant admin", expected: "OK", actual: "OK", pass: true });

    await go(page, "tenant-administration");
    await page.click('button[data-tab="users"]');
    await expect(page.locator('[data-panel="users"]')).toBeVisible();
    const resetCount = await page.locator("[data-user-password]").count();
    steps.push({ step: "Reset password button", expected: ">0", actual: String(resetCount), pass: resetCount > 0 });
    expect(resetCount).toBeGreaterThan(0);

    const confirmField = page.locator("#newUserConfirmPassword");
    steps.push({ step: "Confirm password on create", expected: "present", actual: String(await confirmField.count()), pass: (await confirmField.count()) === 1 });

    await go(page, "regulatory");
    await expect(page.locator(".ra-shell")).toBeVisible({ timeout: 30000 });
    steps.push({ step: "RA console", expected: "Visible", actual: "OK", pass: true });

    const dash = await browserApi(page, "GET", `/tenants/${TENANT}/regulatory/dashboard`);
    steps.push({ step: "RA dashboard API", expected: "200", actual: String(dash.status), pass: dash.status === 200 });
    expect(dash.status).toBe(200);

    await logout(page);
    const summary = await saveFunctionalReport(u.role, u.email, steps, page);
    expect(summary.verdict).toBe("PASS");
  });

  test("03 Product → Dossier → checklist 22 → SoD submit block", async ({ page }) => {
    test.setTimeout(180_000);
    const u = user("Regulatory Specialist");
    const steps: StepResult[] = [];
    await login(page, TENANT, u.email, u.password);

    const boot = await browserApi(page, "POST", `/tenants/${TENANT}/regulatory/bootstrap`, {});
    // Bootstrap needs CONFIGURE; specialist may get 403 if packs already seeded — continue either way
    steps.push({ step: "Bootstrap authorities/packs", expected: "2xx or already seeded", actual: String(boot.status), pass: boot.status < 300 || boot.status === 403 });

    const code = `E2E-PROD-${Date.now()}`;
    const product = await browserApi(page, "POST", `/tenants/${TENANT}/regulatory/products`, {
      countryCode: "PA",
      category: "Dispositivo Médico",
      brand: "E2E Brand",
      regulatoryName: `Producto E2E ${code}`,
      commercialName: `Comercial ${code}`,
      catalogCode: code,
      riskClass: "A",
      distributorName: "Multimed",
      opportunityAmount: 1000,
      currency: "USD",
    });
    steps.push({ step: "Create product", expected: "2xx", actual: String(product.status), pass: product.status < 300 });
    expect(product.status).toBeLessThan(300);

    const auths = await browserApi(page, "GET", `/tenants/${TENANT}/regulatory/authorities`);
    const authorityId = (Array.isArray(auths.body) ? auths.body : [])[0]?.id as string;
    steps.push({ step: "Authority available", expected: "id", actual: authorityId || "none", pass: !!authorityId });

    const dossier = await browserApi(page, "POST", `/tenants/${TENANT}/regulatory/dossiers`, {
      productId: (product.body as { id: string }).id,
      authorityId,
      processType: "NewRegistration",
      comments: "E2E production dossier",
      currency: "USD",
      opportunityAmount: 1000,
    });
    const reqs = (dossier.body as { requirements?: unknown[]; id: string })?.requirements ?? [];
    steps.push({ step: "Dossier checklist", expected: "22", actual: String(reqs.length), pass: reqs.length === 22 });
    expect(reqs.length).toBe(22);

    const dossierId = (dossier.body as { id: string }).id;
    const submit = await browserApi(page, "POST", `/tenants/${TENANT}/regulatory/dossiers/${dossierId}/submit`, {});
    steps.push({ step: "Submit blocked (criticals)", expected: "4xx", actual: String(submit.status), pass: submit.status >= 400 });
    expect(submit.status).toBeGreaterThanOrEqual(400);

    await go(page, "regulatory");
    await expect(page.locator(".ra-shell")).toBeVisible({ timeout: 30000 });
    steps.push({ step: "RA UI after dossier", expected: "Visible", actual: "OK", pass: true });

    await logout(page);
    const summary = await saveFunctionalReport("RA Journey", u.email, steps, page, { dossierId, catalogCode: code });
    expect(summary.verdict).toBe("PASS");
  });

  test("04 Import REGUTRACK XLSX via UI", async ({ page }) => {
    const importer = user("Regulatory Administrator");
    const steps: StepResult[] = [];
    expect(fs.existsSync(XLSX)).toBe(true);
    await login(page, TENANT, importer.email, importer.password);

    await go(page, "regulatory");
    await expect(page.locator(".ra-shell")).toBeVisible({ timeout: 30000 });
    await page.click('#ra-nav button[data-view="import"]');
    await expect(page.locator("#ra-xlsx")).toBeVisible({ timeout: 10000 });
    await page.setInputFiles("#ra-xlsx", XLSX);
    await page.click("#ra-stage-xlsx");
    await expect(page.locator(".toast.success").filter({ hasText: /Stage OK|OK/i }).last()).toBeVisible({ timeout: 120000 });
    await page.waitForTimeout(2000);
    const bodyText = (await page.locator("#ra-body").textContent()) ?? "";
    const ok = /REGUTRACK|Simulated|Validated|Staged|Committed|warn=|\.xlsx/i.test(bodyText);
    steps.push({ step: "Stage XLSX UI", expected: "staged job visible", actual: bodyText.slice(0, 220), pass: ok });

    await logout(page);
    const summary = await saveFunctionalReport("RA Import", importer.email, steps, page);
    expect(summary.verdict).toBe("PASS");
  });
});
