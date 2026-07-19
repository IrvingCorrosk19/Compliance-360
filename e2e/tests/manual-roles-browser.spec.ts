import { test, expect, Page, ConsoleMessage } from "@playwright/test";
import * as fs from "fs";
import * as path from "path";

/**
 * ENTERPRISE FUNCTIONAL CERTIFICATION — MANUAL = SINGLE SOURCE OF TRUTH.
 * Contract source: docs/user-manual/data/{roles,screens,buttons,fields}.json
 * Each role's visible RA tabs, sidebar entries, buttons and API permissions
 * are asserted 1:1 against the manual.
 */

const TENANT = "82af3877-2786-4d39-bce8-c981101c771d";
const PASS = "OwnerStart!2026";
const OUT = path.join(__dirname, "..", "..", "docs", "certification", "evidence");

type RoleCase = {
  code: string;
  email: string;
  roleName: string;
  expectBadge?: RegExp;
  raTabs: string[]; // exact RA tabs per manual (order not enforced, set equality)
  sidebar: string[]; // exact sidebar routes per manual
  canSeeNewProductBtn: boolean;
  createProductViaModal?: boolean;
  canBootstrap: boolean;
  openTac?: boolean;
  tacMustShow?: RegExp;
  openSecurity?: boolean;
  forbiddenApis: Array<{ method: string; path: string; body?: object }>;
};

const RA = (p: string) => `/tenants/${TENANT}/regulatory${p}`;
const ZERO = "00000000-0000-0000-0000-000000000001";
const productBody = { countryCode: "PA", category: "Insumos Médicos", brand: "X", regulatoryName: "X", catalogCode: `DENY-${Date.now()}`, riskClass: "A", currency: "USD" };

const roles: RoleCase[] = [
  {
    code: "TAC",
    email: "irvingcorrosk19@gmail.com",
    roleName: "Tenant Administrator",
    expectBadge: /perfil tac/i,
    raTabs: ["dashboard", "portfolio", "dossiers", "registrations", "licenses", "config", "sod"],
    sidebar: ["dashboard", "audit-trail", "regulatory", "tenant-administration", "security"],
    canSeeNewProductBtn: false,
    canBootstrap: true,
    openTac: true,
    tacMustShow: /Tenant Administration|Administraci[oó]n de inquilinos|Usuarios del tenant|User Administration/i,
    openSecurity: true,
    forbiddenApis: [
      { method: "POST", path: RA("/dossiers"), body: { productId: ZERO, authorityId: ZERO, processType: "NewRegistration" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/transition`), body: { targetStatus: "Assembling" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve-for-submission`), body: { notes: "nope" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/submit`), body: {} },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve`), body: { registrationNumber: "X", issuedOn: new Date().toISOString() } },
    ],
  },
  {
    code: "RA-ADM",
    email: "ra.admin@cert.local",
    roleName: "Regulatory Administrator",
    expectBadge: /perfil admin/i,
    raTabs: ["dashboard", "portfolio", "manufacturers", "licenses", "alerts", "import", "config", "sod"],
    sidebar: ["dashboard", "regulatory"],
    canSeeNewProductBtn: true, // manual buttons.json: new-product roles incluye regulatory-administrator
    createProductViaModal: true,
    canBootstrap: true,
    forbiddenApis: [
      { method: "POST", path: RA(`/dossiers/${ZERO}/transition`), body: { targetStatus: "Assembling" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve-for-submission`), body: { notes: "nope" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/submit`), body: {} },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve`), body: { registrationNumber: "X", issuedOn: new Date().toISOString() } },
      { method: "PUT", path: RA(`/dossiers/${ZERO}/requirements/${ZERO}`), body: { status: "Received" } },
    ],
  },
  {
    code: "RA-MGR",
    email: "ra.mgr@cert.local",
    roleName: "Regulatory Manager",
    expectBadge: /perfil manager/i,
    raTabs: ["dashboard", "portfolio", "pipeline", "dossiers", "registrations", "alerts", "sod"],
    sidebar: ["dashboard", "regulatory"],
    canSeeNewProductBtn: false,
    canBootstrap: false,
    forbiddenApis: [
      { method: "POST", path: RA("/bootstrap"), body: {} },
      { method: "POST", path: RA("/products"), body: productBody },
      { method: "POST", path: RA("/dossiers"), body: { productId: ZERO, authorityId: ZERO, processType: "NewRegistration" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve-for-submission`), body: { notes: "nope" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/submit`), body: {} },
    ],
  },
  {
    code: "RA-SPEC",
    email: "ra.spec@cert.local",
    roleName: "Regulatory Specialist",
    expectBadge: /perfil specialist/i,
    raTabs: ["dashboard", "portfolio", "pipeline", "dossiers", "registrations", "manufacturers"],
    sidebar: ["dashboard", "regulatory"],
    canSeeNewProductBtn: true,
    canBootstrap: false,
    forbiddenApis: [
      { method: "POST", path: RA("/bootstrap"), body: {} },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve-for-submission`), body: { notes: "nope" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/submit`), body: {} },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve`), body: { registrationNumber: "X", issuedOn: new Date().toISOString() } },
      { method: "POST", path: RA("/renewals"), body: { productId: ZERO, authorityId: ZERO } },
      { method: "POST", path: RA("/operating-licenses"), body: { companyName: "X", licenseType: "X" } },
    ],
  },
  {
    code: "RA-REV",
    email: "ra.rev@cert.local",
    roleName: "Regulatory Reviewer",
    expectBadge: /perfil reviewer/i,
    raTabs: ["dashboard", "pipeline", "dossiers", "registrations"],
    sidebar: ["dashboard", "regulatory"],
    canSeeNewProductBtn: false,
    canBootstrap: false,
    forbiddenApis: [
      { method: "POST", path: RA("/products"), body: productBody },
      { method: "POST", path: RA("/dossiers"), body: { productId: ZERO, authorityId: ZERO, processType: "NewRegistration" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/transition`), body: { targetStatus: "Assembling" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve-for-submission`), body: { notes: "nope" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/submit`), body: {} },
      { method: "POST", path: RA(`/dossiers/${ZERO}/observations`), body: { description: "x", receivedOn: new Date().toISOString() } },
      { method: "POST", path: RA("/renewals"), body: { productId: ZERO, authorityId: ZERO } },
      { method: "POST", path: RA("/manufacturers"), body: { legalName: "X", countryCode: "CN" } },
      { method: "POST", path: RA("/operating-licenses"), body: { companyName: "X", licenseType: "X" } },
    ],
  },
  {
    code: "RA-APPR",
    email: "ra.appr@cert.local",
    roleName: "Regulatory Approver",
    expectBadge: /perfil approver/i,
    raTabs: ["dashboard", "pipeline", "dossiers", "registrations"],
    sidebar: ["dashboard", "regulatory"],
    canSeeNewProductBtn: false,
    canBootstrap: false,
    forbiddenApis: [
      { method: "POST", path: RA("/products"), body: productBody },
      { method: "POST", path: RA(`/dossiers/${ZERO}/transition`), body: { targetStatus: "Assembling" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/submit`), body: {} },
      { method: "POST", path: RA(`/dossiers/${ZERO}/observations`), body: { description: "x", receivedOn: new Date().toISOString() } },
      { method: "PUT", path: RA(`/dossiers/${ZERO}/requirements/${ZERO}`), body: { status: "Received" } },
    ],
  },
  {
    code: "RA-SUB",
    email: "ra.sub@cert.local",
    roleName: "Regulatory Submitter",
    expectBadge: /perfil submitter/i,
    raTabs: ["dashboard", "pipeline", "dossiers", "registrations"],
    sidebar: ["dashboard", "regulatory"],
    canSeeNewProductBtn: false,
    canBootstrap: false,
    forbiddenApis: [
      { method: "POST", path: RA("/products"), body: productBody },
      { method: "POST", path: RA(`/dossiers/${ZERO}/transition`), body: { targetStatus: "Assembling" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve-for-submission`), body: { notes: "nope" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/observations`), body: { description: "x", receivedOn: new Date().toISOString() } },
      { method: "PUT", path: RA(`/dossiers/${ZERO}/requirements/${ZERO}`), body: { status: "Received" } },
    ],
  },
  {
    code: "RA-VIEW",
    email: "ra.view@cert.local",
    roleName: "Regulatory Viewer",
    expectBadge: /perfil viewer/i,
    raTabs: ["dashboard", "portfolio", "pipeline", "dossiers", "registrations", "alerts"],
    sidebar: ["dashboard", "regulatory"],
    canSeeNewProductBtn: false,
    canBootstrap: false,
    forbiddenApis: [
      { method: "POST", path: RA("/bootstrap"), body: {} },
      { method: "POST", path: RA("/products"), body: productBody },
      { method: "POST", path: RA("/dossiers"), body: { productId: ZERO, authorityId: ZERO, processType: "NewRegistration" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/transition`), body: { targetStatus: "Assembling" } },
      { method: "PUT", path: RA(`/dossiers/${ZERO}/requirements/${ZERO}`), body: { status: "Received" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve-for-submission`), body: { notes: "nope" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/submit`), body: {} },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve`), body: { registrationNumber: "X", issuedOn: new Date().toISOString() } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/observations`), body: { description: "x", receivedOn: new Date().toISOString() } },
      { method: "POST", path: RA("/renewals"), body: { productId: ZERO, authorityId: ZERO } },
      { method: "POST", path: RA("/manufacturers"), body: { legalName: "X", countryCode: "CN" } },
      { method: "POST", path: RA("/operating-licenses"), body: { companyName: "X", licenseType: "X" } },
      { method: "PUT", path: RA("/sod-settings"), body: {} },
    ],
  },
  {
    code: "QM",
    email: "ra.qm@cert.local",
    roleName: "Quality Manager",
    expectBadge: /perfil qm/i,
    raTabs: ["dashboard", "dossiers", "registrations"],
    sidebar: ["dashboard", "audit-trail", "regulatory", "documents", "technical-sheets", "audits", "capa", "risks", "indicators", "reports"],
    canSeeNewProductBtn: false,
    canBootstrap: false,
    forbiddenApis: [
      { method: "POST", path: RA("/bootstrap"), body: {} },
      { method: "POST", path: RA("/products"), body: productBody },
      { method: "POST", path: RA("/dossiers"), body: { productId: ZERO, authorityId: ZERO, processType: "NewRegistration" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/transition`), body: { targetStatus: "Assembling" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/approve-for-submission`), body: { notes: "nope" } },
      { method: "POST", path: RA(`/dossiers/${ZERO}/submit`), body: {} },
    ],
  },
];

type Evidence = {
  role: string;
  email: string;
  pass: boolean;
  checks: Array<{ id: string; pass: boolean; detail: string }>;
  consoleErrors: string[];
  pageErrors: string[];
};

async function login(page: Page, email: string) {
  await page.goto("/");
  await page.evaluate(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
  await page.reload();
  await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 20000 });
  await page.fill("#email", email);
  await page.click("#login-form button[type=submit]");
  const org = page.locator('input[name="organizationId"]');
  const passwordField = page.locator("#password");
  await Promise.race([
    org.first().waitFor({ state: "visible", timeout: 20000 }).catch(() => {}),
    passwordField.waitFor({ state: "visible", timeout: 20000 }).catch(() => {}),
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
  await page.waitForSelector("aside.sidebar", { timeout: 45000 });
}

async function api(page: Page, method: string, apiPath: string, body?: object) {
  return page.evaluate(
    async ({ method, apiPath, body }) => {
      const token = localStorage.getItem("c360.token");
      const res = await fetch(`/api/v1${apiPath}`, {
        method,
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: body ? JSON.stringify(body) : undefined,
      });
      const text = await res.text();
      return { status: res.status, text: text.slice(0, 240) };
    },
    { method, apiPath, body }
  );
}

function isNoiseConsole(msg: ConsoleMessage) {
  const text = msg.text();
  if (/favicon|Download the React DevTools|Third-party cookie/i.test(text)) return true;
  if (/Failed to load resource: the server responded with a status of (400|401|403|404)/i.test(text)) return true;
  return false;
}

// RA tab keys → data-view values rendered in #ra-nav
const ALL_TAB_KEYS = ["dashboard", "portfolio", "pipeline", "dossiers", "registrations", "manufacturers", "licenses", "alerts", "import", "config", "sod"];

test("Manual roles — certification matrix (manual = source of truth)", async ({ page }) => {
  test.setTimeout(600000);
  fs.mkdirSync(OUT, { recursive: true });
  const evidence: Evidence[] = [];
  const globalFailures: string[] = [];

  for (const role of roles) {
    const consoleErrors: string[] = [];
    const pageErrors: string[] = [];
    const onConsole = (msg: ConsoleMessage) => {
      if (msg.type() === "error" && !isNoiseConsole(msg)) consoleErrors.push(msg.text());
    };
    const onPageError = (err: Error) => pageErrors.push(err.message);
    page.on("console", onConsole);
    page.on("pageerror", onPageError);

    const checks: Evidence["checks"] = [];
    try {
      await login(page, role.email);
      checks.push({ id: "LOGIN", pass: true, detail: "shell visible" });

      // ---- Sidebar must match manual screens exactly -----------------------
      const sidebarRoutes = await page.locator("aside.sidebar .nav-button").evaluateAll(
        (els) => els.map((e) => (e as HTMLElement).dataset.route)
      );
      const sidebarSet = new Set(sidebarRoutes);
      const expectedSidebar = new Set(role.sidebar);
      const sidebarOk =
        sidebarSet.size === expectedSidebar.size && [...expectedSidebar].every((r) => sidebarSet.has(r));
      checks.push({
        id: "SIDEBAR-EXACT",
        pass: sidebarOk,
        detail: `got=[${[...sidebarSet].join(",")}] expected=[${role.sidebar.join(",")}]`,
      });

      // ---- RA console ------------------------------------------------------
      await page.evaluate(() => {
        location.hash = "#/regulatory";
      });
      await page.waitForSelector(".ra-shell", { timeout: 30000 });
      await page.waitForTimeout(400);

      if (role.expectBadge) {
        const badge = (await page.locator(".ra-badge").first().textContent()) || "";
        checks.push({ id: "BADGE", pass: role.expectBadge.test(badge), detail: badge });
      }

      // Exact RA tab set per manual
      const tabKeys = await page.locator("#ra-nav button").evaluateAll(
        (els) => els.map((e) => (e as HTMLElement).dataset.view)
      );
      const tabSet = new Set(tabKeys);
      for (const key of role.raTabs) {
        checks.push({ id: `TAB-YES-${key}`, pass: tabSet.has(key), detail: tabSet.has(key) ? "visible" : `missing in [${tabKeys.join(",")}]` });
      }
      for (const key of ALL_TAB_KEYS.filter((k) => !role.raTabs.includes(k))) {
        checks.push({ id: `TAB-NO-${key}`, pass: !tabSet.has(key), detail: tabSet.has(key) ? "incorrectly visible" : "hidden ok" });
      }

      // ---- Portfolio create button (manual buttons.json new-product) -------
      if (role.raTabs.includes("portfolio")) {
        await page.click('#ra-nav [data-view="portfolio"]');
        await page.waitForTimeout(500);
        const createBtn = page.locator("#ra-new-product");
        const createVisible = await createBtn.isVisible().catch(() => false);
        checks.push({
          id: "BTN-NEW-PRODUCT",
          pass: createVisible === role.canSeeNewProductBtn,
          detail: `visible=${createVisible} expected=${role.canSeeNewProductBtn}`,
        });

        if (role.canSeeNewProductBtn && createVisible) {
          let native = 0;
          page.on("dialog", async (d) => {
            native++;
            await d.dismiss();
          });
          await createBtn.click();
          const modal = page.locator("#ra-new-product-modal .ra-modal");
          let modalOk = false;
          try {
            await expect(modal).toBeVisible({ timeout: 15000 });
            modalOk = true;
          } catch {
            modalOk = false;
          }
          checks.push({ id: "MODAL-OPEN", pass: modalOk, detail: modalOk ? "modal enterprise" : "modal missing" });
          checks.push({ id: "NO-NATIVE-PROMPT", pass: native === 0, detail: `native=${native}` });

          if (modalOk) {
            // Manual fields.json: riskClass permitido A/B/C únicamente + campo País (PA).
            const riskOptions = await page.locator("#ra-np-risk option").allTextContents();
            const riskOk = riskOptions.join(",") === "A,B,C";
            checks.push({ id: "RISK-ABC-ONLY", pass: riskOk, detail: `options=${riskOptions.join(",")}` });
            const countryVal = await page.locator("#ra-np-country").inputValue().catch(() => "");
            checks.push({ id: "FIELD-PAIS", pass: countryVal === "PA", detail: `value=${countryVal}` });

            if (role.createProductViaModal) {
              const stamp = Date.now().toString().slice(-6);
              await page.fill("#ra-np-brand", `CERT-${role.code}`);
              await page.fill("#ra-np-name", `Producto cert ${role.code} ${stamp}`);
              await page.fill("#ra-np-code", `CRT-${role.code}-${stamp}`);
              await page.selectOption("#ra-np-risk", "B");
              await page.locator("#ra-new-product-modal button[type=submit]").click();
              let created = false;
              try {
                await expect(modal).toBeHidden({ timeout: 20000 });
                created = true;
              } catch {
                created = false;
              }
              checks.push({ id: "CREATE-PRODUCT-DOSSIER", pass: created, detail: created ? "producto + expediente creados" : "modal no cerró (error API)" });
            } else {
              await page.click("#ra-new-product-modal [data-modal-cancel]");
            }
          }
        }
      }

      // ---- Bootstrap permission via API ------------------------------------
      const boot = await api(page, "POST", RA("/bootstrap"), {});
      const bootAllowed = boot.status < 300;
      checks.push({
        id: "API-BOOTSTRAP",
        pass: bootAllowed === role.canBootstrap,
        detail: `http=${boot.status} expectedAllowed=${role.canBootstrap}`,
      });

      // ---- Forbidden APIs must be denied by authorization (401/403) --------
      for (const f of role.forbiddenApis) {
        const res = await api(page, f.method, f.path, f.body);
        const denied = res.status === 401 || res.status === 403;
        checks.push({
          id: `DENY-${f.method}-${f.path.split("/").slice(-2).join("/")}`,
          pass: denied,
          detail: `http=${res.status}`,
        });
      }

      // ---- TAC screens ------------------------------------------------------
      if (role.openTac) {
        await page.evaluate(() => {
          location.hash = "#/tenant-administration";
        });
        await page.waitForTimeout(1200);
        const body = (await page.locator("#content").innerText()).slice(0, 1200);
        const ok = role.tacMustShow ? role.tacMustShow.test(body) : body.length > 20;
        checks.push({ id: "TAC-UI", pass: ok, detail: body.slice(0, 160).replace(/\s+/g, " ") });
      }
      if (role.openSecurity) {
        await page.evaluate(() => {
          location.hash = "#/security";
        });
        await page.waitForTimeout(1200);
        const body = (await page.locator("#content").innerText());
        const ok = /Security score|Política de acceso|MFA/i.test(body);
        checks.push({ id: "SECURITY-UI", pass: ok, detail: body.slice(0, 160).replace(/\s+/g, " ") });
      }

      await page.screenshot({
        path: path.join(OUT, `cert-role-${role.code.toLowerCase()}.png`),
        fullPage: true,
      });
    } catch (e: any) {
      checks.push({ id: "EXCEPTION", pass: false, detail: String(e?.message || e) });
    } finally {
      page.removeAllListeners("dialog");
      page.off("console", onConsole);
      page.off("pageerror", onPageError);
      try {
        await page.click("#logout");
        await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 15000 });
      } catch {
        /* ignore */
      }
    }

    const consoleOk = consoleErrors.length === 0 && pageErrors.length === 0;
    checks.push({
      id: "CONSOLE-CLEAN",
      pass: consoleOk,
      detail: consoleOk
        ? "no console/page errors"
        : `console=${consoleErrors.slice(0, 5).join(" || ")} page=${pageErrors.slice(0, 5).join(" || ")}`,
    });

    const pass = checks.every((c) => c.pass);
    evidence.push({ role: role.code, email: role.email, pass, checks, consoleErrors, pageErrors });
    if (!pass) {
      globalFailures.push(
        `${role.code}: ` + checks.filter((c) => !c.pass).map((c) => `${c.id}(${c.detail})`).join("; ")
      );
    }
  }

  fs.writeFileSync(path.join(OUT, "manual-vs-system-role-matrix.json"), JSON.stringify(evidence, null, 2), "utf8");

  expect(globalFailures, `Role failures:\n${globalFailures.join("\n")}`).toEqual([]);
});
