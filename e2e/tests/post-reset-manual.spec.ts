const { test, expect } = require("@playwright/test");
const TENANT = "82af3877-2786-4d39-bce8-c981101c771d";
const EMAIL = "irvingcorrosk19@gmail.com";
const PASSWORD = process.env.C360_ADMIN_PASSWORD || "OwnerStart!2026";
test("post-reset principal admin regulatory baseline", async ({ page }) => {
  await page.goto("/");
  await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 30000 });
  if (await page.locator("#tenantId").count()) {
    await page.fill("#tenantId", TENANT);
    await page.fill("#legacy-email, #email", EMAIL);
    await page.fill("#legacy-password, #password", PASSWORD);
    await page.click("#legacy-login-form button[type=submit], #login-form button[type=submit]");
  } else {
    await page.fill("#email", EMAIL);
    await page.click("#login-form button[type=submit]");
    const org = page.locator('input[name="organizationId"]');
    const passwordField = page.locator("#password");
    await Promise.race([
      org.first().waitFor({ state: "visible", timeout: 8000 }).catch(() => {}),
      passwordField.waitFor({ state: "visible", timeout: 8000 }).catch(() => {}),
    ]);
    if (await org.count()) {
      const preferred = page.locator(`input[name="organizationId"][value="${TENANT}"]`);
      if (await preferred.count()) await preferred.check();
      else await org.first().check();
      await page.click("#login-form button[type=submit]");
      await passwordField.waitFor({ state: "visible", timeout: 15000 });
    }
    await page.fill("#password", PASSWORD);
    await page.click("#login-form button[type=submit]");
  }
  await expect(page.locator("aside.sidebar")).toBeVisible({ timeout: 45000 });
  await page.evaluate(() => { location.hash = "#/regulatory"; });
  await expect(page.locator(".ra-shell")).toBeVisible({ timeout: 20000 });
  const token = await page.evaluate(() => localStorage.getItem("c360.token"));
  expect(token).toBeTruthy();
  const baseline = await page.evaluate(async (tenantId) => {
    const h = { Authorization: "Bearer " + localStorage.getItem("c360.token") };
    const get = async (p) => { const r = await fetch("/api/v1" + p, { headers: h }); return { s: r.status, b: await r.json() }; };
    const products = await get(`/tenants/${tenantId}/regulatory/products`);
    const dossiers = await get(`/tenants/${tenantId}/regulatory/dossiers`);
    const regs = await get(`/tenants/${tenantId}/regulatory/registrations`);
    const lics = await get(`/tenants/${tenantId}/regulatory/operating-licenses`);
    const sod = await get(`/tenants/${tenantId}/regulatory/sod-settings`);
    return {
      products: products.s < 300 && Array.isArray(products.b),
      dossiers: dossiers.s < 300 && Array.isArray(dossiers.b),
      regs: regs.s < 300 && Array.isArray(regs.b),
      lics: lics.s < 300 && Array.isArray(lics.b),
      sod: sod.s < 300 && !!sod.b,
    };
  }, TENANT);
  expect(baseline.products).toBeTruthy();
  expect(baseline.dossiers).toBeTruthy();
  expect(baseline.regs).toBeTruthy();
  expect(baseline.lics).toBeTruthy();
  expect(baseline.sod).toBeTruthy();
});
