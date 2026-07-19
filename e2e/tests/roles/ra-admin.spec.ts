import { test, expect } from "@playwright/test";
import * as path from "path";
import { TENANT, browserApi, evidenceDir, login, logout, saveFunctionalReport, user, type StepResult } from "../helpers";

test("RA-ADM — configuración, alertas e importación REGUTRACK", async ({ page }) => {
  test.setTimeout(600_000);
  const account = user("Regulatory Administrator");
  const out = evidenceDir("RA-ADM");
  const steps: StepResult[] = [];
  const ra = `/tenants/${TENANT}/regulatory`;
  const stamp = Date.now().toString().slice(-8);

  await login(page, TENANT, account.email, account.password);
  steps.push({ step: "LOGIN", expected: "Shell autenticado", actual: "Shell visible", pass: true });
  await page.evaluate(() => { location.hash = "#/regulatory"; });
  await expect(page.locator(".ra-shell")).toBeVisible({ timeout: 30_000 });

  const tabs = await page.locator("#ra-nav [data-view]").evaluateAll(nodes => nodes.map(node => (node as HTMLElement).dataset.view));
  for (const required of ["dashboard", "portfolio", "manufacturers", "licenses", "alerts", "import", "config", "sod"]) {
    expect(tabs).toContain(required);
  }
  steps.push({ step: "MENUS", expected: "Pestañas de configuración RA", actual: tabs.join(","), pass: true });

  await page.locator('#ra-nav [data-view="config"]').click();
  await expect(page.locator("#ra-boot")).toBeVisible();
  await page.locator("#ra-boot").click();
  await expect(page.locator("#ra-boot-out pre")).toContainText("REGUTRACK", { timeout: 30_000 });
  const authorities = await browserApi(page, "GET", `${ra}/authorities`);
  const packs = await browserApi(page, "GET", `${ra}/requirement-packs`);
  const authorityCodes = (authorities.body as any[]).map(item => item.code);
  const defaultPack = (packs.body as any[]).find(item => item.code === "REGUTRACK-PA-DEFAULT");
  steps.push({
    step: "BOOTSTRAP",
    expected: "MINSA, CSS y pack REGUTRACK de 22 requisitos",
    actual: `autoridades=${authorityCodes.join(",")}; requisitos=${defaultPack?.definitions?.length ?? 0}`,
    pass: authorities.ok && packs.ok && authorityCodes.includes("MINSA") && authorityCodes.includes("CSS") && defaultPack?.definitions?.length === 22,
  });

  await page.locator('#ra-nav [data-view="alerts"]').click();
  await expect(page.locator("#ra-alert-settings-form")).toBeVisible();
  const thresholds = "120,90,60,30,7,0";
  await page.fill("#ra-alert-thresholds", thresholds);
  await page.locator("#ra-alert-settings-form button[type=submit]").click();
  await expect(page.locator("#ra-alert-thresholds")).toHaveValue(thresholds, { timeout: 30_000 });
  const alertSettings = await browserApi(page, "GET", `${ra}/alert-settings`);
  steps.push({
    step: "ALERT-SETTINGS",
    expected: "Umbrales persistidos",
    actual: JSON.stringify(alertSettings.body),
    pass: alertSettings.ok && String((alertSettings.body as any)?.thresholdsCsv) === thresholds,
  });

  await page.locator('#ra-nav [data-view="manufacturers"]').click();
  await page.locator("#ra-add-mfr").click();
  await expect(page.locator("#ra-add-mfr-modal .ra-modal")).toBeVisible();
  const manufacturer = `RA Admin Manufacturer ${stamp}`;
  await page.fill('#ra-add-mfr-modal input[name="legalName"]', manufacturer);
  await page.locator("#ra-add-mfr-modal button[type=submit]").click();
  await expect(page.locator("#ra-body")).toContainText(manufacturer, { timeout: 30_000 });
  steps.push({ step: "MANUFACTURER", expected: "Fabricante creado y consultable", actual: manufacturer, pass: true });

  await page.locator('#ra-nav [data-view="portfolio"]').click();
  await expect(page.locator("#ra-new-product")).toBeVisible();
  await page.locator("#ra-new-product").click();
  await expect(page.locator("#ra-new-product-modal .ra-modal")).toBeVisible();
  const catalogCode = `ADM-${stamp}`;
  await page.fill('#ra-new-product-modal input[name="brand"]', `Brand ${stamp}`);
  await page.fill('#ra-new-product-modal input[name="regulatoryName"]', `Producto RA Admin ${stamp}`);
  await page.fill('#ra-new-product-modal input[name="catalogCode"]', catalogCode);
  await page.locator("#ra-new-product-modal button[type=submit]").click();
  await expect(page.locator("#ra-body")).toContainText(catalogCode, { timeout: 30_000 });
  steps.push({ step: "PRODUCT", expected: "Producto/expediente Planning creado", actual: catalogCode, pass: true });

  await page.locator('#ra-nav [data-view="import"]').click();
  await expect(page.locator("#ra-xlsx")).toBeVisible();
  const workbook = path.resolve(__dirname, "..", "..", "..", "REGUTRACK 02JUN26 MG.xlsx");
  await page.locator("#ra-xlsx").setInputFiles(workbook);
  await page.locator("#ra-stage-xlsx").click();
  await expect(page.locator("#ra-import-jobs")).toContainText("REGUTRACK 02JUN26 MG.xlsx", { timeout: 180_000 });
  const jobs = await browserApi(page, "GET", `${ra}/imports`);
  const staged = (jobs.body as any[]).find(item => item.sourceFileName === "REGUTRACK 02JUN26 MG.xlsx");
  steps.push({
    step: "REGUTRACK-STAGE",
    expected: "XLSX procesado, validado y job persistido con reporte",
    actual: `status=${staged?.status ?? "NO"} imported=${staged?.importedRowCount ?? "n/a"} warnings=${staged?.warningCount ?? "n/a"} errors=${staged?.errorCount ?? "n/a"}`,
    pass: jobs.ok && staged?.status === "Simulated" && typeof staged?.validationReportJson === "string" && staged.validationReportJson.length > 0,
  });

  const forbidden = [
    ["APPROVE-INTERNAL", "approve-for-submission"],
    ["SUBMIT", "submit"],
    ["DECISION-EXTERNAL", "approve"],
  ] as const;
  for (const [id, suffix] of forbidden) {
    const result = await browserApi(
      page,
      "POST",
      `${ra}/dossiers/00000000-0000-0000-0000-000000000001/${suffix}`,
      {}
    );
    steps.push({
      step: `DENY-${id}`,
      expected: "HTTP 401/403",
      actual: `HTTP ${result.status}`,
      pass: result.status === 401 || result.status === 403,
    });
  }

  await page.screenshot({ path: `${out}\\ra-admin-final.png`, fullPage: true });
  await saveFunctionalReport("RA-ADM", account.email, steps, page, { tenantId: TENANT, catalogCode, manufacturer });
  expect(steps.filter(step => !step.pass)).toEqual([]);
  await logout(page);
});
