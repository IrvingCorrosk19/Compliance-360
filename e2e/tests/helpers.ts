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
  await page.evaluate(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
  await page.reload();
  await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 20000 });

  if (await page.locator("#tenantId").count()) {
    await page.fill("#tenantId", tenantId);
    await page.fill("#legacy-email, #email", email);
    await page.fill("#legacy-password, #password", password);
    await page.click("#legacy-login-form button[type=submit], #login-form button[type=submit]");
  } else {
    await page.fill("#email", email);
    await page.click("#login-form button[type=submit]");

    const orgRadio = page.locator('input[name="organizationId"]');
    const passwordField = page.locator("#password");
    await Promise.race([
      orgRadio.first().waitFor({ state: "visible", timeout: 20000 }),
      passwordField.waitFor({ state: "visible", timeout: 20000 }),
    ]);

    if (await orgRadio.count()) {
      const preferred = tenantId
        ? page.locator(`input[name="organizationId"][value="${tenantId}"]`)
        : orgRadio.first();
      if (tenantId && (await preferred.count())) {
        await preferred.check();
      } else {
        await orgRadio.first().check();
      }
      await page.click("#login-form button[type=submit]");
      await passwordField.waitFor({ state: "visible", timeout: 20000 });
    }

    await passwordField.waitFor({ state: "visible", timeout: 20000 });
    await page.fill("#password", password);
    await page.click("#login-form button[type=submit]");
  }

  try {
    await page.waitForSelector("aside.sidebar", { timeout: 45000 });
  } catch {
    const err = await page.locator(".toast.error").last().textContent().catch(() => null);
    throw new Error(`Login failed for ${email}: ${err ?? "shell not visible after 45s"}`);
  }
}

export async function logout(page: Page) {
  await page.click("#logout");
  await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 15000 });
  await page.evaluate(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
}

export async function go(page: Page, route: string) {
  await page.evaluate((rt) => { location.hash = `#/${rt}`; }, route);
  await page.waitForTimeout(2500);
}

export async function browserApi(page: Page, method: string, apiPath: string, body?: object) {
  return page.evaluate(async ({ method, apiPath, body }) => {
    const token = localStorage.getItem("c360.token");
    const url = apiPath.startsWith("/api/") ? apiPath : `/api/v1${apiPath}`;
    const res = await fetch(url, {
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

export async function createModuleRecord(page: Page, route: string): Promise<string> {
  await go(page, route);
  const form = page.locator("#module-action-form");
  await expect(form).toBeVisible();
  const code = `E2E-${route.toUpperCase()}-${Date.now()}`;
  await form.locator('[name="code"]').fill(code);
  await form.locator('button[type="submit"]').click();
  await expect(page.locator(".toast.success").last()).toBeVisible({ timeout: 20000 });
  return code;
}

export async function createEnterpriseItem(page: Page, route: string): Promise<string> {
  await go(page, route);
  const form = page.locator("#enterprise-action-form");
  await expect(form).toBeVisible();
  const code = `E2E-${route.toUpperCase()}-${Date.now()}`;
  await form.locator('[name="code"]').fill(code);
  await form.locator('button[type="submit"]').click();
  await expect(page.locator(".toast.success").last()).toBeVisible({ timeout: 20000 });
  return code;
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
