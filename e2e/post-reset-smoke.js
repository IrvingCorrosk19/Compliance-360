const { chromium } = require("playwright");
const fs = require("fs");
const path = require("path");

const BASE = process.env.C360_BASE_URL || "http://localhost:5272";
const TENANT = "82af3877-2786-4d39-bce8-c981101c771d";
const EMAIL = "irvingcorrosk19@gmail.com";
const PASSWORD = process.env.C360_ADMIN_PASSWORD || "";
const out = path.join(__dirname, "..", "docs", "manual-testing-reset", "_browser_post_reset.json");
const results = [];

function rec(name, ok, detail = "") {
  results.push({ check: name, pass: !!ok, detail: String(detail).slice(0, 300) });
  console.log(`[${ok ? "PASS" : "FAIL"}] ${name} :: ${String(detail).slice(0, 180)}`);
}

(async () => {
  if (!PASSWORD) {
    rec("password_env", false, "C360_ADMIN_PASSWORD missing");
    fs.writeFileSync(out, JSON.stringify(results, null, 2));
    process.exit(1);
  }
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  await page.goto(BASE + "/", { waitUntil: "domcontentloaded", timeout: 60000 });
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
    try {
      await Promise.race([
        org.first().waitFor({ state: "visible", timeout: 8000 }),
        passwordField.waitFor({ state: "visible", timeout: 8000 }),
      ]);
    } catch {}
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

  try {
    await page.waitForSelector("aside.sidebar", { timeout: 45000 });
    rec("login", true, "sidebar");
  } catch (e) {
    rec("login", false, e.message);
    await browser.close();
    fs.writeFileSync(out, JSON.stringify(results, null, 2));
    process.exit(1);
  }

  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  try {
    await page.waitForSelector(".ra-shell", { timeout: 20000 });
    const txt = (await page.locator(".ra-shell").innerText()) || "";
    const dirty = /CERT-|MQ-FFC|ra\.spec@|Imported from REGUTRACK|Licencia Closure/i.test(txt);
    rec("ra_shell", true, "loaded");
    rec("ra_no_cert_data_visible", !dirty, txt.slice(0, 100));
  } catch (e) {
    rec("ra_shell", false, e.message);
  }

  const checks = await page.evaluate(async (tenantId) => {
    const token = localStorage.getItem("c360.token");
    const h = { Authorization: `Bearer ${token}`, "Content-Type": "application/json" };
    const get = async (p) => {
      const r = await fetch(`/api/v1${p}`, { headers: h });
      const t = await r.text();
      let b = null;
      try {
        b = JSON.parse(t);
      } catch {
        b = t;
      }
      return { status: r.status, body: b };
    };
    const len = (x) => (Array.isArray(x) ? x.length : -1);
    const products = await get(`/tenants/${tenantId}/regulatory/products`);
    const dossiers = await get(`/tenants/${tenantId}/regulatory/dossiers`);
    const regs = await get(`/tenants/${tenantId}/regulatory/registrations`);
    const lics = await get(`/tenants/${tenantId}/regulatory/operating-licenses`);
    const sod = await get(`/tenants/${tenantId}/regulatory/sod-settings`);
    const users = await get(`/tenants/${tenantId}/users`).catch(() => ({ status: 0, body: null }));
    return {
      products: { status: products.status, n: len(products.body) },
      dossiers: { status: dossiers.status, n: len(dossiers.body) },
      regs: { status: regs.status, n: len(regs.body) },
      lics: { status: lics.status, n: len(lics.body) },
      sod: { status: sod.status, ok: !!sod.body },
      users: { status: users.status },
    };
  }, TENANT);

  rec("api_products_empty", checks.products.status < 300 && checks.products.n === 0, JSON.stringify(checks.products));
  rec("api_dossiers_empty", checks.dossiers.status < 300 && checks.dossiers.n === 0, JSON.stringify(checks.dossiers));
  rec("api_regs_empty", checks.regs.status < 300 && checks.regs.n === 0, JSON.stringify(checks.regs));
  rec("api_lics_empty", checks.lics.status < 300 && checks.lics.n === 0, JSON.stringify(checks.lics));
  rec("api_sod_loads", checks.sod.status < 300 && checks.sod.ok, JSON.stringify(checks.sod));
  rec("users_api_reachable", checks.users.status > 0, JSON.stringify(checks.users));

  await browser.close();
  fs.writeFileSync(out, JSON.stringify(results, null, 2));
  process.exit(results.every((r) => r.pass) ? 0 : 1);
})().catch((e) => {
  console.error(e);
  process.exit(1);
});
