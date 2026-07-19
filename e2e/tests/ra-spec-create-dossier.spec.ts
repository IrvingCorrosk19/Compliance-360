import { test, expect, Page } from "@playwright/test";

const TENANT = "82af3877-2786-4d39-bce8-c981101c771d";
const PASS = "OwnerStart!2026";

async function login(page: Page, email: string) {
  await page.goto("/");
  await page.evaluate(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
  await page.reload();
  await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 20000 });
  await page.fill("#email", email);
  await page.click("#login-form button[type=submit]");
  const org = page.locator('input[name="organizationId"]');
  const passwordField = page.locator("#password");
  await Promise.race([
    org.first().waitFor({ state: "visible", timeout: 20000 }),
    passwordField.waitFor({ state: "visible", timeout: 20000 }),
  ]);
  if (await org.count()) {
    const preferred = page.locator(`input[name="organizationId"][value="${TENANT}"]`);
    if (await preferred.count()) await preferred.check();
    else await org.first().check();
    await page.click("#login-form button[type=submit]");
  }
  await passwordField.waitFor({ state: "visible", timeout: 20000 });
  await page.fill("#password", PASS);
  await page.click("#login-form button[type=submit]");
  await page.waitForSelector("aside.sidebar", { timeout: 45000 });
}

async function counts(page: Page) {
  return page.evaluate(async (tenant) => {
    const token = localStorage.getItem("c360.token");
    const headers = { Authorization: `Bearer ${token}` };
    const products = await (await fetch(`/api/v1/tenants/${tenant}/regulatory/products`, { headers })).json();
    const dossiers = await (await fetch(`/api/v1/tenants/${tenant}/regulatory/dossiers`, { headers })).json();
    return { products: products.length, dossiers: dossiers.length };
  }, TENANT);
}

test("RA Specialist creates product + dossier from Portafolio button (strict)", async ({ page }) => {
  const forbidden: string[] = [];
  page.on("response", (res) => {
    if (res.status() === 403 && res.url().includes("/regulatory/")) forbidden.push(`${res.status()} ${res.url()}`);
  });

  await login(page, "ra.spec@cert.local");
  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  await page.waitForSelector(".ra-shell", { timeout: 30000 });

  const before = await counts(page);

  const portfolioBtn = page.locator(".ra-shell button", { hasText: /Portafolio|Portfolio/i }).first();
  await portfolioBtn.click();
  const createBtn = page.locator("#ra-new-product");
  await expect(createBtn).toBeVisible({ timeout: 15000 });

  // STRICT: no native dialogs allowed anymore; the modal form MUST appear.
  const stamp = Date.now().toString().slice(-6);
  let nativeDialogs = 0;
  page.on("dialog", async (dialog) => {
    nativeDialogs++;
    await dialog.dismiss();
  });

  await createBtn.click();
  const modal = page.locator("#ra-new-product-modal .ra-modal");
  await expect(modal, "El modal de creación nunca apareció").toBeVisible({ timeout: 15000 });

  await page.fill("#ra-np-brand", "BRW-UI");
  await page.fill("#ra-np-name", `Producto UI ${stamp}`);
  await page.fill("#ra-np-code", `UI-${stamp}`);
  await page.selectOption("#ra-np-risk", "B");
  await page.fill("#ra-np-comments", "Creado por E2E via modal");
  await modal.locator("button[type=submit]").click();
  await expect(modal).toBeHidden({ timeout: 20000 });
  expect(nativeDialogs, "Se usaron prompts/alerts nativos").toBe(0);

  // STRICT: counts must increase by exactly 1.
  await expect
    .poll(async () => (await counts(page)).products, { timeout: 20000 })
    .toBe(before.products + 1);
  await expect
    .poll(async () => (await counts(page)).dossiers, { timeout: 20000 })
    .toBe(before.dossiers + 1);

  // The created product must be listed in the refreshed portfolio table.
  await expect(page.locator(".ra-table")).toContainText(`UI-${stamp}`, { timeout: 15000 });

  expect(forbidden, `Llamadas 403 detectadas: ${forbidden.join(", ")}`).toHaveLength(0);
  await page.screenshot({ path: "../docs/regulatory-affairs/security/evidence/ra-spec-create-ui.png", fullPage: true });
});
