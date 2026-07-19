/**
 * Playwright tests for the offline interactive user manual.
 */
const { test, expect } = require("@playwright/test");
const path = require("path");

const indexUrl =
  "file:///" +
  path.resolve(__dirname, "../../docs/user-manual/index.html").replace(/\\/g, "/");

test.describe("Compliance 360 interactive user manual", () => {
  test("opens index and shows roles", async ({ page }) => {
    const errors = [];
    page.on("pageerror", (e) => errors.push(String(e)));
    await page.goto(indexUrl);
    await expect(page.getByRole("heading", { name: "Manual Interactivo por Rol" })).toBeVisible();
    await expect(page.locator(".role-card")).toHaveCount(9);
    expect(errors).toEqual([]);
  });

  test("theme toggle light/dark", async ({ page }) => {
    await page.goto(indexUrl);
    await page.getByRole("button", { name: "Oscuro" }).click();
    await expect(page.locator("body")).toHaveAttribute("data-theme", "dark");
    await page.getByRole("button", { name: "Claro" }).click();
    await expect(page.locator("body")).toHaveAttribute("data-theme", "light");
  });

  test("search finds expediente", async ({ page }) => {
    await page.goto(indexUrl);
    await page.locator("#global-search").fill("expediente");
    await expect(page.locator("#search-results")).toContainText(
      /Rol|Pantalla|Tutorial|Campo|Botón|Glosario|Error/
    );
  });

  test("enter specialist role and complete tutorial", async ({ page }) => {
    await page.goto(indexUrl);
    await page.locator('.role-card[data-role="regulatory-specialist"] a').click();
    await expect(page.getByRole("heading", { name: "Regulatory Specialist" })).toBeVisible();
    await page.locator("[data-complete-tutorial]").first().click();
    await expect(page.locator("#c360-toast")).toContainText("Tutorial");
  });

  test("marker panel opens", async ({ page }) => {
    await page.goto(indexUrl.replace("index.html", "roles/regulatory-specialist.html"));
    await page.locator(".marker").first().click();
    await expect(page.locator("#marker-panel")).toContainText("Menú lateral");
    await page.getByRole("button", { name: "Siguiente elemento" }).click();
  });

  test("simulator advances", async ({ page }) => {
    await page.goto(indexUrl);
    await page.locator("#sim-do").click();
    await expect(page.locator("#guided-sim")).toContainText("Paso");
    await page.locator("#sim-next").click();
  });

  test("progress persists after reload", async ({ page }) => {
    await page.goto(indexUrl);
    await page.evaluate(() => {
      window.C360_MANUAL.progress.markTutorial("spec-product");
    });
    await page.reload();
    const stored = await page.evaluate(() => localStorage.getItem("c360.manual.progress.v1"));
    expect(stored).toContain("spec-product");
  });

  test("glossary and errors render", async ({ page }) => {
    await page.goto(indexUrl);
    await expect(page.locator("#glossary-list details")).toHaveCount(26);
    await expect(page.locator("#error-list article").first()).toBeVisible();
  });

  test("keyboard focus on marker", async ({ page }) => {
    await page.goto(indexUrl.replace("index.html", "roles/regulatory-approver.html"));
    await page.locator(".marker").first().focus();
    await page.keyboard.press("Enter");
    await expect(page.locator("#marker-panel")).toContainText("Rol");
  });

  test("mobile viewport loads", async ({ page }) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto(indexUrl);
    await expect(page.getByRole("heading", { name: "Manual Interactivo por Rol" })).toBeVisible();
  });
});
