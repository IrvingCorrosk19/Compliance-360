import { Page, expect } from "@playwright/test";
import * as fs from "fs";
import * as path from "path";

const dataPath = path.join(__dirname, "..", "testdata.json");
const raw = fs.readFileSync(dataPath, "utf-8").replace(/^\uFEFF/, "");
export const data = JSON.parse(raw);
export const TENANT = data.tenantId as string;
export const PLATFORM_TENANT = data.platform.tenantId as string;
export const evidenceRoot = path.join(__dirname, "..", "..", "artifacts", "e2e");

export function user(role: string) {
  const u = data.users.find((x: { role: string }) => x.role === role);
  if (!u) throw new Error(`User not found for role: ${role}`);
  return u as { role: string; email: string; password: string };
}

export function evidenceDir(role: string) {
  const dir = path.join(evidenceRoot, role.replace(/[^a-z0-9]+/gi, "_"));
  fs.mkdirSync(dir, { recursive: true });
  return dir;
}

export async function login(page: Page, tenantId: string, email: string, password: string) {
  await page.goto("/");
  await page.waitForSelector("#login-form", { timeout: 20000 });
  await page.fill("#tenantId", tenantId);
  await page.fill("#email", email);
  await page.fill("#password", password);
  await Promise.all([
    page.waitForSelector("aside.sidebar", { timeout: 25000 }),
    page.click("#login-form button[type=submit]"),
  ]);
}

export async function logout(page: Page) {
  await page.click("#logout");
  await page.waitForSelector("#login-form", { timeout: 15000 });
}

export async function go(page: Page, route: string) {
  await page.evaluate((rt) => { location.hash = `#/${rt}`; }, route);
  await page.waitForTimeout(2000);
}

export async function browserApi(page: Page, method: string, apiPath: string, body?: object) {
  return page.evaluate(async ({ method, apiPath, body }) => {
    const token = localStorage.getItem("c360.token");
    const res = await fetch(`/api/v1${apiPath}`, {
      method,
      headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
      body: body ? JSON.stringify(body) : undefined,
    });
    const text = await res.text();
    let parsed: unknown = text;
    try { parsed = JSON.parse(text); } catch { /* raw text */ }
    return { status: res.status, ok: res.ok, body: parsed, text };
  }, { method, apiPath, body });
}

export async function jwtPermissions(page: Page): Promise<string[]> {
  return page.evaluate(() => {
    const token = localStorage.getItem("c360.token");
    if (!token) return [];
    try {
      const payload = JSON.parse(atob(token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")));
      const p = payload.permission || [];
      return Array.isArray(p) ? p : [p];
    } catch { return []; }
  });
}

export async function createModuleRecord(page: Page, route: string) {
  await go(page, route);
  await expect(page.locator("#module-action-form")).toBeVisible({ timeout: 10000 });
  const code = `E2E-${route.toUpperCase().replace(/[^A-Z]/g, "")}-${Date.now()}`;
  await page.fill("#code", code);
  const nameField = page.locator("#name");
  if (await nameField.count()) {
    await nameField.fill(`E2E ${route} ${Date.now()}`);
  }
  await page.click("#module-action-form button[type=submit]");
  await expect(page.locator(".toast.success").filter({ hasText: /creado|created/i }).last()).toBeVisible({ timeout: 20000 });
  return code;
}

export async function createEnterpriseItem(page: Page, route: string) {
  await go(page, route);
  await expect(page.locator("#enterprise-action-form")).toBeVisible({ timeout: 10000 });
  const code = `ENT-${Date.now()}`;
  await page.fill("#code", code);
  await page.fill("#title", `E2E Enterprise ${route}`);
  await page.click("#enterprise-action-form button[type=submit]");
  await expect(page.locator(".toast.success").filter({ hasText: /creado|created/i }).last()).toBeVisible({ timeout: 20000 });
  return code;
}

export type StepResult = { step: string; expected: string; actual: string; pass: boolean };

export async function saveFunctionalReport(
  role: string,
  email: string,
  steps: StepResult[],
  page: Page,
  extra: Record<string, unknown> = {}
) {
  const dir = evidenceDir(role);
  const verdict = steps.every(s => s.pass) ? "PASS" : "FAIL";
  const summary = { role, email, date: new Date().toISOString(), steps, verdict, ...extra };
  fs.writeFileSync(path.join(dir, "functional-summary.json"), JSON.stringify(summary, null, 2));
  await page.screenshot({ path: path.join(dir, "functional-final.png"), fullPage: true });
  return summary;
}
