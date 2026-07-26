import { test, expect } from "@playwright/test";
import * as fs from "fs";
import * as path from "path";
import { TENANT, login, logout, user } from "./helpers";

const baseURL = process.env.E2E_BASE_URL || "http://localhost:5272";

const SPANISH_MARKERS = [
  "Guardando...",
  "Identificando...",
  "Inbox de notificaciones",
];

test.describe("Final i18n runtime certification", () => {
  test("login language switch + authenticated EN/ES chrome", async ({ page }) => {
    test.setTimeout(120_000);
    await page.goto(baseURL + "/");
    await page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });
    await page.reload();
    await page.waitForSelector("[data-c360-language-selector]", { timeout: 20000 });

    await page.selectOption("[data-c360-language-selector]", "en");
    await expect(page.getByRole("button", { name: /Next/i })).toBeVisible({ timeout: 10000 });
    await expect(page.locator("html")).toHaveAttribute("lang", "en");

    await page.selectOption("[data-c360-language-selector]", "es");
    await expect(page.getByRole("button", { name: /Siguiente/i })).toBeVisible({ timeout: 10000 });
    await expect(page.locator("html")).toHaveAttribute("lang", "es");

    const cookieEs = await page.evaluate(() => document.cookie);
    expect(cookieEs).toMatch(/c360\.language=es/);
    expect(await page.evaluate(() => localStorage.getItem("c360.language"))).toBe("es");

    await page.reload();
    await page.waitForSelector("[data-c360-language-selector]", { timeout: 20000 });
    await expect(page.locator("html")).toHaveAttribute("lang", "es");
    await expect(page.getByRole("button", { name: /Siguiente/i })).toBeVisible({ timeout: 10000 });

    const account = user("Regulatory Specialist");
    await login(page, TENANT, account.email, account.password);

    const lang = page.locator("[data-c360-language-selector]").first();
    await lang.selectOption("en");
    await page.waitForTimeout(800);
    await expect(page.locator("html")).toHaveAttribute("lang", "en");

    const routes = ["dashboard", "regulatory", "reports", "alert-center"] as const;
    for (const route of routes) {
      await page.evaluate((rt) => {
        location.hash = `#/${rt}`;
      }, route);
      await page.waitForTimeout(1500);
      await expect.soft(page.locator("html")).toHaveAttribute("lang", "en");

      const body = page.locator("body");
      for (const marker of SPANISH_MARKERS) {
        await expect.soft(body, `EN #/${route} must not show "${marker}"`).not.toContainText(marker);
      }
    }

    await lang.selectOption("es");
    await page.waitForTimeout(800);
    await expect(page.locator("html")).toHaveAttribute("lang", "es");

    const esBody = page.locator("body");
    await expect.soft(esBody).toContainText(/Siguiente|Reportes|Expedientes|Panel|Notificaciones/i);

    await page.reload();
    await page.waitForSelector("aside.sidebar", { timeout: 45000 });
    await expect(page.locator("html")).toHaveAttribute("lang", "es");
    const stored = await page.evaluate(() => localStorage.getItem("c360.language"));
    expect(stored).toBe("es");

    const evidenceDir = path.join(__dirname, "..", "..", "docs", "enterprise-final-certification", "evidence");
    fs.mkdirSync(evidenceDir, { recursive: true });
    fs.writeFileSync(
      path.join(evidenceDir, "final-i18n-runtime.json"),
      JSON.stringify(
        {
          email: account.email,
          tenantId: TENANT,
          routes,
          spanishMarkersChecked: SPANISH_MARKERS,
          langAfterReload: stored,
          when: new Date().toISOString(),
        },
        null,
        2
      )
    );

    await logout(page);
  });
});
