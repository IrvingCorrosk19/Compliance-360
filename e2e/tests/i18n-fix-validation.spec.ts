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
  if (await page.locator("#tenantId").count()) {
    await page.fill("#tenantId", TENANT);
    await page.fill("#legacy-email, #email", email);
    await page.fill("#legacy-password, #password", PASS);
    await page.click("#legacy-login-form button[type=submit], #login-form button[type=submit]");
  } else {
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
  }
  await page.waitForSelector("aside.sidebar", { timeout: 45000 });
}

async function setLanguage(page: Page, lang: "es" | "en") {
  await page.evaluate(async (l) => {
    await (window as any).I18n.setLanguage(l);
  }, lang);
  // trigger full re-render like the selector does
  await page.evaluate(() => {
    location.hash = location.hash; // noop
  });
  await page.waitForTimeout(600);
}

test("language switch translates shell, dashboard, RA console and admin", async ({ page }) => {
  await login(page, "irvingcorrosk19@gmail.com");

  // --- Spanish baseline via selector (persists preference) ---
  await page.locator("[data-c360-language-selector]").first().selectOption("es");
  await page.waitForTimeout(1500);
  const sidebarEs = await page.locator("aside.sidebar").innerText();
  expect(sidebarEs.toLowerCase()).toContain("centro de comando");

  // --- Switch to English via selector ---
  await page.locator("[data-c360-language-selector]").first().selectOption("en");
  await page.waitForTimeout(1500);

  const sidebarEn = await page.locator("aside.sidebar").innerText();
  expect(sidebarEn.toLowerCase()).toContain("command center");
  expect(sidebarEn).not.toContain("Centro de comando");

  // --- RA console in English ---
  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  await page.waitForSelector(".ra-shell", { timeout: 30000 });
  await page.waitForTimeout(1200);
  const raEn = await page.locator(".ra-shell").innerText();
  expect(raEn).toContain("Regulatory Affairs Console");

  // --- Tenant administration in English ---
  await page.evaluate(() => {
    location.hash = "#/tenant-administration";
  });
  await expect(page.locator("#content")).toContainText("Tenant Administration Center", { timeout: 30000 });
  await expect(page.locator("#content")).toContainText("Reports", { timeout: 30000 });
  const tacEn = await page.locator("#content").innerText();
  expect(tacEn).toContain("Tenant Administration Center");
  expect(tacEn).toContain("Reports");

  // --- Back to Spanish ---
  const selector2 = page.locator("[data-c360-language-selector]").first();
  await selector2.selectOption("es");
  await expect(page.locator("#content")).toContainText("Centro de administración de inquilinos", { timeout: 30000 });
  await expect(page.locator("#content")).toContainText("Reportes", { timeout: 30000 });
  const tacEs = await page.locator("#content").innerText();
  expect(tacEs).toContain("Centro de administración de inquilinos");
  expect(tacEs).toContain("Reportes");

  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  await page.waitForSelector(".ra-shell", { timeout: 30000 });
  await page.waitForTimeout(1200);
  const raEs = await page.locator(".ra-shell").innerText();
  expect(raEs).toContain("Consola de Asuntos Regulatorios");

  const sidebarEs2 = await page.locator("aside.sidebar").innerText();
  expect(sidebarEs2.toLowerCase()).toContain("centro de comando");
});
