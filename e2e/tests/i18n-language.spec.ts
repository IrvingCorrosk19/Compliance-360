import { test, expect } from "@playwright/test";

test.describe("i18n language switch", () => {
  test("login page switches es/en instantly", async ({ page }) => {
    await page.goto("/");
    await page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });
    await page.reload();
    await page.waitForSelector("[data-c360-language-selector]");

    await page.selectOption("[data-c360-language-selector]", "en");
    await expect(page.getByRole("button", { name: /Next/i })).toBeVisible({ timeout: 10000 });
    await expect(page.locator("html")).toHaveAttribute("lang", "en");

    await page.selectOption("[data-c360-language-selector]", "es");
    await expect(page.getByRole("button", { name: /Siguiente/i })).toBeVisible({ timeout: 10000 });
    await expect(page.locator("html")).toHaveAttribute("lang", "es");

    const cookie = await page.evaluate(() => document.cookie);
    expect(cookie).toMatch(/c360\.language=es/);
    const stored = await page.evaluate(() => localStorage.getItem("c360.language"));
    expect(stored).toBe("es");
  });
});
