import { test, expect, Page } from "@playwright/test";
import * as fs from "fs";
import * as path from "path";

const dataPath = path.join(__dirname, "..", "testdata.json");
const raw = fs.readFileSync(dataPath, "utf-8").replace(/^\uFEFF/, "");
const data = JSON.parse(raw);
const PLATFORM_TENANT = data.platform.tenantId as string;
const TENANT = data.tenantId as string;
const evidenceRoot = path.join(__dirname, "..", "..", "artifacts", "e2e");

type RoleExpectation = {
  role: string;
  email: string;
  password: string;
  tenantId: string;
  mustSee: string[];
  mustNotSee: string[];
  canManage?: Record<string, boolean | string>;
  landingRoute?: string;
};

function pass(role: string) {
  return data.users.find((u: any) => u.role === role);
}

const tenantUser = (role: string, mustSee: string[], mustNotSee: string[], canManage?: Record<string, boolean | string>, landingRoute?: string): RoleExpectation => {
  const u = pass(role);
  return { role, email: u.email, password: u.password, tenantId: TENANT, mustSee, mustNotSee, canManage, landingRoute };
};

const roles: RoleExpectation[] = [
  {
    role: "Platform Administrator",
    email: data.platform.email,
    password: data.platform.password,
    tenantId: PLATFORM_TENANT,
    mustSee: ["superadmin-platform", "tenant-administration"],
    mustNotSee: ["documents", "suppliers", "capa", "risks"],
    landingRoute: "superadmin-platform",
  },
  tenantUser("Tenant Administrator",
    ["dashboard", "tenant-administration", "audit-trail"],
    ["superadmin-platform", "documents", "suppliers", "capa", "risks", "indicators", "security", "configuration"],
    undefined, "tenant-administration"),
  tenantUser("Tenant Security Administrator",
    ["dashboard", "security", "audit-trail"],
    ["superadmin-platform", "documents", "tenant-administration", "capa"],
    undefined, "security"),
  tenantUser("Document Controller",
    ["dashboard", "documents", "audit-trail"],
    ["superadmin-platform", "suppliers", "capa", "risks", "indicators", "security", "tenant-administration"],
    { documents: true }, "documents"),
  tenantUser("Quality Manager",
    ["dashboard", "documents", "technical-sheets", "capa", "risks", "indicators", "audits", "reports", "audit-trail"],
    ["superadmin-platform", "suppliers", "security", "configuration", "tenant-administration"],
    { documents: false, capa: false }, "documents"),
  tenantUser("Auditor",
    ["dashboard", "audits", "capa", "documents", "suppliers", "reports", "audit-trail"],
    ["superadmin-platform", "risks", "indicators", "security", "configuration", "tenant-administration"],
    { audits: true, capa: false }, "audits"),
  tenantUser("Supplier Manager",
    ["dashboard", "suppliers", "supplier-portal", "documents", "reports", "audit-trail"],
    ["superadmin-platform", "capa", "risks", "indicators", "security", "configuration", "tenant-administration"],
    { suppliers: true }, "suppliers"),
  tenantUser("CAPA Manager",
    ["dashboard", "capa", "risks", "audits", "reports", "audit-trail"],
    ["superadmin-platform", "documents", "suppliers", "indicators", "security", "configuration"],
    { capa: true }, "capa"),
  tenantUser("Risk Manager",
    ["dashboard", "risks", "indicators", "audits", "reports", "audit-trail"],
    ["superadmin-platform", "documents", "suppliers", "capa", "security", "configuration"],
    { risks: true }, "risks"),
  tenantUser("Indicators Manager",
    ["dashboard", "indicators", "risks", "reports", "audit-trail"],
    ["superadmin-platform", "documents", "suppliers", "capa", "audits", "security", "configuration"],
    { indicators: true }, "indicators"),
  tenantUser("Reporting Manager",
    ["dashboard", "reports", "indicators", "risks", "capa", "audits", "audit-trail"],
    ["superadmin-platform", "documents", "suppliers", "security", "configuration", "tenant-administration"],
    undefined, "reports"),
  tenantUser("Storage Administrator",
    ["dashboard", "configuration", "audit-trail"],
    ["superadmin-platform", "documents", "suppliers", "capa", "risks", "indicators", "security", "tenant-administration"],
    { configuration: "storage" }, "configuration"),
  tenantUser("Notification Administrator",
    ["dashboard", "configuration", "audit-trail"],
    ["superadmin-platform", "documents", "suppliers", "capa", "risks", "security", "tenant-administration"],
    { configuration: "notification" }, "configuration"),
  tenantUser("Viewer",
    ["dashboard", "documents", "technical-sheets", "suppliers", "audits", "capa", "risks", "indicators", "reports", "audit-trail"],
    ["superadmin-platform", "security", "tenant-administration"],
    { documents: false, suppliers: false, capa: false, risks: false, indicators: false, audits: false }, "documents"),
];

async function login(page: Page, tenantId: string, email: string, password: string) {
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

async function jwtPermissions(page: Page): Promise<string[]> {
  return await page.evaluate(() => {
    const token = localStorage.getItem("c360.token");
    if (!token) return [];
    try {
      const payload = JSON.parse(atob(token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")));
      const p = payload.permission || [];
      return Array.isArray(p) ? p : [p];
    } catch { return []; }
  });
}

async function hasActionForm(page: Page): Promise<boolean> {
  return (await page.locator("#module-action-form").count()) > 0;
}

async function visibleRoutes(page: Page): Promise<string[]> {
  return await page.$$eval("aside.sidebar .nav-button", (els) =>
    Array.from(new Set(els.map((e) => (e as HTMLElement).dataset.route || "")))
  );
}

for (const r of roles) {
  test(`${r.role} — RBAC & navigation`, async ({ page }, testInfo) => {
    const dir = path.join(evidenceRoot, r.role.replace(/[^a-z0-9]+/gi, "_"));
    fs.mkdirSync(dir, { recursive: true });

    const consoleErrors: string[] = [];
    const httpErrors: string[] = [];
    page.on("console", (m) => { if (m.type() === "error") consoleErrors.push(m.text()); });
    page.on("response", (res) => {
      const s = res.status();
      if (s >= 500) httpErrors.push(`${s} ${res.url()}`);
    });

    // 1. Login
    await login(page, r.tenantId, r.email, r.password);
    await page.screenshot({ path: path.join(dir, "01-after-login.png"), fullPage: true });

    // 2. Authoritative RBAC state from JWT claims
    const permissions = await jwtPermissions(page);

    // 3. Visible navigation reflects RBAC
    const visible = await visibleRoutes(page);
    for (const must of r.mustSee) {
      expect(visible, `${r.role} must see route '${must}'`).toContain(must);
    }
    for (const forbidden of r.mustNotSee) {
      expect(visible, `${r.role} must NOT see route '${forbidden}'`).not.toContain(forbidden);
    }

    // 4. Manage affordances (read vs write)
    const manageResults: Record<string, boolean> = {};
    if (r.canManage) {
      for (const [route, expected] of Object.entries(r.canManage)) {
        await page.evaluate((rt) => { location.hash = `#/${rt}`; }, route);
        await page.waitForTimeout(2000);
        let actual: boolean;
        if (expected === "storage") {
          actual = (await page.locator("#create-storage-provider").count()) > 0;
          expect(await page.locator("#create-email-provider").count(), `${r.role} must NOT see email provider button`).toBe(0);
        } else if (expected === "notification") {
          actual = (await page.locator("#create-email-provider").count()) > 0;
          expect(await page.locator("#create-storage-provider").count(), `${r.role} must NOT see storage provider button`).toBe(0);
        } else {
          actual = await hasActionForm(page);
        }
        manageResults[route] = actual;
        expect(actual, `${r.role} manage affordance on '${route}' expected ${expected}`).toBe(Boolean(expected));
      }
    }

    // 5. Navigate to landing/business route and screenshot
    if (r.landingRoute) {
      await page.evaluate((rt) => { location.hash = `#/${rt}`; }, r.landingRoute);
      await page.waitForTimeout(2000);
      await page.screenshot({ path: path.join(dir, `02-${r.landingRoute}.png`), fullPage: true });
    }

    // 6. Logout
    await page.click("#logout");
    await page.waitForSelector("#login-form", { timeout: 15000 });
    await page.screenshot({ path: path.join(dir, "03-after-logout.png") });

    // 7. Evidence file
    const summary = {
      role: r.role,
      email: r.email,
      tenantId: r.tenantId,
      permissions,
      visibleRoutes: visible,
      mustSee: r.mustSee,
      mustNotSee: r.mustNotSee,
      manageResults,
      consoleErrors,
      httpErrors,
      verdict: consoleErrors.length === 0 && httpErrors.length === 0 ? "PASS" : "REVIEW",
    };
    fs.writeFileSync(path.join(dir, "summary.json"), JSON.stringify(summary, null, 2));
    await testInfo.attach("summary", { body: JSON.stringify(summary, null, 2), contentType: "application/json" });

    expect(consoleErrors, `${r.role} console errors`).toEqual([]);
    expect(httpErrors, `${r.role} 5xx errors`).toEqual([]);
  });
}
