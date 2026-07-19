import { test, expect } from "@playwright/test";
import * as path from "path";
import * as fs from "fs";
import { TENANT, evidenceDir, login, logout, user } from "../helpers";

const roles = [
  "Tenant Administrator",
  "Regulatory Administrator",
  "Regulatory Manager",
  "Regulatory Specialist",
  "Regulatory Reviewer",
  "Regulatory Approver",
  "Regulatory Submitter",
  "Regulatory Viewer",
  "Quality Manager",
];

for (const role of roles) {
  test(`${role} — responsive, tema oscuro y es/en`, async ({ page }) => {
    test.setTimeout(120_000);
    const account = user(role);
    const out = evidenceDir(role);
    await page.setViewportSize({ width: 1440, height: 900 });
    await login(page, TENANT, account.email, account.password);

    const targetRoute = role === "Tenant Administrator"
      ? "tenant-administration"
      : role === "Quality Manager"
        ? "documents"
        : "regulatory";
    await page.evaluate(route => { location.hash = `#/${route}`; }, targetRoute);
    await page.waitForTimeout(1_500);

    const selector = page.locator("[data-c360-language-selector]").first();
    await selector.selectOption("es");
    await page.waitForTimeout(600);
    await page.screenshot({ path: path.join(out, "desktop-es-light.png"), fullPage: true });

    await selector.selectOption("en");
    await page.waitForTimeout(600);
    const htmlLanguage = await page.locator("html").getAttribute("lang");
    expect(htmlLanguage).toBe("en");

    await page.locator("#theme-toggle").click();
    await expect(page.locator("html")).toHaveAttribute("data-theme", "dark");
    await page.screenshot({ path: path.join(out, "desktop-en-dark.png"), fullPage: true });

    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForTimeout(600);
    await expect(page.locator("#menu-toggle")).toBeVisible();
    const overflow = await page.evaluate(() => ({
      viewport: document.documentElement.clientWidth,
      content: document.documentElement.scrollWidth,
    }));
    expect(overflow.content).toBeLessThanOrEqual(overflow.viewport + 2);
    await page.screenshot({ path: path.join(out, "mobile-en-dark.png"), fullPage: true });

    fs.writeFileSync(path.join(out, "responsive-i18n.json"), JSON.stringify({
      role,
      email: account.email,
      route: targetRoute,
      desktop: { width: 1440, height: 900, language: "es/en", themes: "light/dark" },
      mobile: { width: 390, height: 844, language: "en", theme: "dark", overflow },
      verdict: "PASS",
      when: new Date().toISOString(),
    }, null, 2));
    await logout(page);
  });
}
