import { test, expect } from "@playwright/test";
import * as fs from "fs";
import * as path from "path";
import { TENANT, login, logout, user } from "./helpers";

const baseURL = process.env.E2E_BASE_URL || "http://localhost:5272";

const VIEWPORTS = [
  { width: 1440, height: 900 },
  { width: 1366, height: 768 },
  { width: 1024, height: 768 },
  { width: 768, height: 1024 },
  { width: 390, height: 844 },
  { width: 360, height: 800 },
] as const;

const evidenceDir = path.join(__dirname, "..", "..", "docs", "enterprise-final-certification", "evidence");

async function assertNoHorizontalOverflow(page: import("@playwright/test").Page) {
  const metrics = await page.evaluate(() => ({
    scrollWidth: document.documentElement.scrollWidth,
    innerWidth: window.innerWidth,
  }));
  expect(
    metrics.scrollWidth,
    `scrollWidth ${metrics.scrollWidth} must be <= innerWidth ${metrics.innerWidth} + 1`
  ).toBeLessThanOrEqual(metrics.innerWidth + 1);
  return metrics;
}

test.describe("Final responsive certification", () => {
  test("viewports: login + regulatory/reports overflow + screenshots", async ({ page }) => {
    test.setTimeout(240_000);
    fs.mkdirSync(evidenceDir, { recursive: true });
    const account = user("Regulatory Specialist");
    const overflowLog: Array<Record<string, unknown>> = [];

    for (const vp of VIEWPORTS) {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.goto(baseURL + "/");
      await page.evaluate(() => {
        localStorage.clear();
        sessionStorage.clear();
      });
      await page.reload();
      await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 20000 });

      const emailField = page.locator("#email, #legacy-email").first();
      if (await emailField.isVisible().catch(() => false)) {
        await emailField.fill(account.email);
      }

      const nextOrSubmit = page
        .locator("#login-form button[type=submit], #legacy-login-form button[type=submit]")
        .first();
      await expect(nextOrSubmit).toBeVisible();

      const loginOverflow = await assertNoHorizontalOverflow(page);
      overflowLog.push({ phase: "login", ...vp, ...loginOverflow });
    }

    await page.setViewportSize({ width: 1440, height: 900 });
    await login(page, TENANT, account.email, account.password);

    for (const vp of VIEWPORTS) {
      await page.setViewportSize({ width: vp.width, height: vp.height });
      await page.waitForTimeout(400);

      for (const route of [
        "dashboard",
        "regulatory",
        "reports",
        "alert-center",
        "documents",
        "users",
      ] as const) {
        await page.evaluate((rt) => {
          location.hash = `#/${rt}`;
        }, route);
        await page.waitForTimeout(1000);

        const main = page.locator("main, #app-main, .workspace, .content, aside.sidebar").first();
        await expect.soft(main).toBeVisible({ timeout: 15000 });

        const overflow = await assertNoHorizontalOverflow(page);
        overflowLog.push({ phase: route, ...vp, ...overflow, result: "PASS" });
      }

      await page.screenshot({
        path: path.join(evidenceDir, `responsive-${vp.width}x${vp.height}.png`),
        fullPage: true,
      });
    }

    fs.writeFileSync(
      path.join(evidenceDir, "final-responsive-cert.json"),
      JSON.stringify(
        {
          email: account.email,
          tenantId: TENANT,
          viewports: VIEWPORTS,
          overflowLog,
          when: new Date().toISOString(),
        },
        null,
        2
      )
    );

    await logout(page);
  });
});
