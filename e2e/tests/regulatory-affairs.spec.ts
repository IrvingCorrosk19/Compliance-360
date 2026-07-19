import { test, expect, Page } from "@playwright/test";
import * as path from "path";
import * as fs from "fs";

/** Validated against live API 2026-07-14 (BUG_001 fix). */
const RA = {
  tenantId: "82af3877-2786-4d39-bce8-c981101c771d",
  email: "irvingcorrosk19@gmail.com",
  password: "OwnerStart!2026",
};
const RA_ADMIN = {
  tenantId: RA.tenantId,
  email: "ra.admin@cert.local",
  password: "OwnerStart!2026",
};

type Step = { step: string; expected: string; actual: string; pass: boolean };

async function loginRa(page: Page, account = RA) {
  await page.goto("/");
  await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 20000 });

  if (await page.locator("#tenantId").count()) {
    await page.fill("#tenantId", account.tenantId);
    await page.fill("#legacy-email, #email", account.email);
    await page.fill("#legacy-password, #password", account.password);
    await page.click("#legacy-login-form button[type=submit], #login-form button[type=submit]");
  } else {
    await page.fill("#email", account.email);
    await page.click("#login-form button[type=submit]");

    const org = page.locator('input[name="organizationId"]');
    const passwordField = page.locator("#password");
    await Promise.race([
      org.first().waitFor({ state: "visible", timeout: 20000 }),
      passwordField.waitFor({ state: "visible", timeout: 20000 }),
    ]);

    if (await org.count()) {
      const preferred = page.locator(`input[name="organizationId"][value="${account.tenantId}"]`);
      if (await preferred.count()) await preferred.check();
      else await org.first().check();
      await page.click("#login-form button[type=submit]");
      await passwordField.waitFor({ state: "visible", timeout: 20000 });
    }

    await page.fill("#password", account.password);
    await page.click("#login-form button[type=submit]");
  }

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
      let parsed: unknown = text;
      try {
        parsed = JSON.parse(text);
      } catch {
        /* raw */
      }
      return { status: res.status, body: parsed as Record<string, unknown> };
    },
    { method, apiPath, body }
  );
}

function saveSteps(name: string, steps: Step[]) {
  const dir = path.join(__dirname, "..", "..", "docs", "testing", "evidence");
  fs.mkdirSync(dir, { recursive: true });
  const failed = steps.filter((s) => !s.pass).length;
  const report = { name, when: new Date().toISOString(), failed, passed: steps.length - failed, steps };
  fs.writeFileSync(path.join(dir, `${name}.json`), JSON.stringify(report, null, 2));
  return report;
}

test.describe("RA — Regulatory Affairs Certification Pack (BUG_001 fixed credentials)", () => {
  test("SCN-06 lifecycle + LEGACY console", async ({ page }) => {
    test.setTimeout(180_000);
    const steps: Step[] = [];
    // TAC: console visible; operational create uses Specialist (SoD model — TAC has no REGULATORY.CREATE)
    await loginRa(page);
    steps.push({ step: "TC-AUTH-0100", expected: "sidebar", actual: "OK", pass: true });

    await page.evaluate(() => {
      location.hash = "#/regulatory";
    });
    await expect(page.locator(".ra-shell")).toBeVisible({ timeout: 20000 });
    const txt = (await page.locator(".ra-shell").textContent()) ?? "";
    const okConsole = /Case Management|Asuntos Regulatorios|Regulatory Affairs/i.test(txt);
    steps.push({ step: "TC-LEGACY-0100", expected: "RA console", actual: okConsole ? "OK" : txt.slice(0, 60), pass: okConsole });
    expect(okConsole).toBe(true);

    const boot = await api(page, "POST", `/tenants/${RA.tenantId}/regulatory/bootstrap`, {});
    steps.push({ step: "TC-RA-0001", expected: "2xx", actual: String(boot.status), pass: boot.status < 300 });
    expect(boot.status).toBeLessThan(300);

    const packs = await api(page, "GET", `/tenants/${RA.tenantId}/regulatory/requirement-packs`);
    const list = Array.isArray(packs.body) ? packs.body : [];
    const defs = (list[0] as { definitions?: unknown[] })?.definitions?.length ?? 0;
    steps.push({ step: "TC-RA-0003", expected: "22", actual: String(defs), pass: defs === 22 });
    expect(defs).toBe(22);

    const tacCreate = await api(page, "POST", `/tenants/${RA.tenantId}/regulatory/products`, {
      countryCode: "PA",
      category: "Insumos Medicos",
      brand: "TAC-DENY",
      regulatoryName: "TAC SHOULD FAIL",
      catalogCode: `TAC-${Date.now().toString().slice(-6)}`,
      riskClass: "A",
      distributorName: "Multimed",
    });
    steps.push({
      step: "TC-RA-TAC-DENY-CREATE",
      expected: "403",
      actual: String(tacCreate.status),
      pass: tacCreate.status === 403 || tacCreate.status >= 400,
    });
    expect(tacCreate.status).toBeGreaterThanOrEqual(400);

    await page.click("#logout");
    await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 20000 });
    await page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });
    const SPEC = { email: "ra.spec@cert.local", password: "OwnerStart!2026" };
    await page.fill("#tenantId", RA.tenantId).catch(() => {});
    if (await page.locator("#tenantId").count()) {
      await page.fill("#tenantId", RA.tenantId);
      await page.fill("#legacy-email, #email", SPEC.email);
      await page.fill("#legacy-password, #password", SPEC.password);
      await page.click("#legacy-login-form button[type=submit], #login-form button[type=submit]");
    } else {
      await page.fill("#email", SPEC.email);
      await page.click("#login-form button[type=submit]");
      await page.fill("#password", SPEC.password);
      await page.click("#login-form button[type=submit]");
    }
    await page.waitForSelector("aside.sidebar", { timeout: 45000 });

    const code = `CERT-PW-${Date.now().toString().slice(-8)}`;
    const product = await api(page, "POST", `/tenants/${RA.tenantId}/regulatory/products`, {
      countryCode: "PA",
      category: "Insumos Medicos",
      brand: "CERT",
      regulatoryName: "CERT PLAYWRIGHT PRODUCT",
      catalogCode: code,
      riskClass: "A",
      distributorName: "Multimed",
      opportunityAmount: 5000,
      currency: "USD",
      registeredSuppliersCount: 1,
      technicalSheetReference: "FT-CERT",
      formReference: "FORM-A",
      sourceLineNumber: 1,
    });
    steps.push({
      step: "TC-RA-0103",
      expected: "distributor Multimed",
      actual: `${product.status} ${(product.body as { distributorName?: string })?.distributorName}`,
      pass: product.status < 300 && (product.body as { distributorName?: string })?.distributorName === "Multimed",
    });
    expect(product.status).toBeLessThan(300);

    const auths = await api(page, "GET", `/tenants/${RA.tenantId}/regulatory/authorities`);
    const authorityId = (Array.isArray(auths.body) ? auths.body : [])[0]?.id as string;
    const dossier = await api(page, "POST", `/tenants/${RA.tenantId}/regulatory/dossiers`, {
      productId: (product.body as { id: string }).id,
      authorityId,
      processType: "NewRegistration",
      comments: "CERT SCN-06",
      currency: "USD",
      opportunityAmount: 5000,
    });
    const reqs = (dossier.body as { requirements?: { id: string; isCritical: boolean }[]; id: string; history?: unknown[] })?.requirements ?? [];
    steps.push({ step: "TC-RA-0300", expected: "22 reqs", actual: String(reqs.length), pass: reqs.length === 22 });
    expect(reqs.length).toBe(22);
    const hist = (dossier.body as { history?: unknown[] }).history?.length ?? 0;
    steps.push({ step: "TC-RA-0600", expected: ">=1 history", actual: String(hist), pass: hist >= 1 });

    // A requirement cannot be marked Received without dossier-owned persisted evidence.
    const firstReq = reqs[0];
    if (firstReq) {
      const up = await api(page, "PUT", `/tenants/${RA.tenantId}/regulatory/dossiers/${(dossier.body as { id: string }).id}/requirements/${firstReq.id}`, {
        status: "Received",
        notes: "CERT specialist evidence",
      });
      steps.push({
        step: "TC-RA-0301",
        expected: "4xx without persisted evidence",
        actual: String(up.status),
        pass: up.status >= 400,
      });
      expect(up.status).toBeGreaterThanOrEqual(400);
    }

    const dossierId = (dossier.body as { id: string }).id;
    const submitDenied = await api(page, "POST", `/tenants/${RA.tenantId}/regulatory/dossiers/${dossierId}/submit`, {});
    steps.push({
      step: "TC-RA-0303-SOD",
      expected: "4xx SoD block",
      actual: String(submitDenied.status),
      pass: submitDenied.status >= 400,
    });
    expect(submitDenied.status).toBeGreaterThanOrEqual(400);

    const approveDenied = await api(page, "POST", `/tenants/${RA.tenantId}/regulatory/dossiers/${dossierId}/approve`, {
      registrationNumber: `MQ-CERT-${Date.now().toString().slice(-5)}`,
      issuedOn: new Date().toISOString(),
      expiresOn: new Date(Date.now() + 864e5 * 1000).toISOString(),
      notes: "CERT",
    });
    steps.push({
      step: "TC-RA-0500-SOD",
      expected: "4xx specialist cannot external-approve",
      actual: String(approveDenied.status),
      pass: approveDenied.status >= 400,
    });
    expect(approveDenied.status).toBeGreaterThanOrEqual(400);

    steps.push({
      step: "TC-RA-JOURNEY-DELEGATED",
      expected: "Full multi-role journey in regulatory-sod-roles.spec.ts + run-final-functional-cert.ps1",
      actual: "delegated",
      pass: true,
    });

    const out = path.join(__dirname, "..", "..", "docs", "final-functional-certification", "evidence");
    fs.mkdirSync(out, { recursive: true });
    fs.writeFileSync(path.join(out, "scn06-browser-steps.json"), JSON.stringify(steps, null, 2));
    expect(steps.every((s) => s.pass)).toBe(true);
  });
  test("TC-RA-0902 Stage XLSX", async ({ page }) => {
    const steps: Step[] = [];
    await loginRa(page, RA_ADMIN);
    await page.evaluate(() => {
      location.hash = "#/regulatory";
    });
    await expect(page.locator(".ra-shell")).toBeVisible({ timeout: 20000 });
    await page.click('#ra-nav button[data-view="import"]');
    const xlsx = path.resolve(__dirname, "..", "..", "REGUTRACK 02JUN26 MG.xlsx");
    test.skip(!fs.existsSync(xlsx), "workbook missing");
    await page.setInputFiles("#ra-xlsx", xlsx);
    await page.click("#ra-stage-xlsx");
    await page.waitForTimeout(12000);
    const bodyText = (await page.locator("#ra-body").textContent()) ?? "";
    const ok = /Simulated|Validated|warn=/i.test(bodyText);
    steps.push({ step: "TC-RA-0902", expected: "staged job visible", actual: bodyText.slice(0, 180), pass: ok });
    saveSteps("TC-RA-0902-xlsx", steps);
    expect(ok).toBe(true);
  });

  test("TC-RA-0302 Submit blocked when criticals pending", async ({ page }) => {
    // Specialist login (TAC cannot CREATE)
    await page.goto("/");
    await page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });
    await page.reload();
    await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 20000 });
    if (await page.locator("#tenantId").count()) {
      await page.fill("#tenantId", RA.tenantId);
      await page.fill("#legacy-email, #email", "ra.spec@cert.local");
      await page.fill("#legacy-password, #password", "OwnerStart!2026");
      await page.click("#legacy-login-form button[type=submit], #login-form button[type=submit]");
    } else {
      await page.fill("#email", "ra.spec@cert.local");
      await page.click("#login-form button[type=submit]");
      await page.fill("#password", "OwnerStart!2026");
      await page.click("#login-form button[type=submit]");
    }
    await page.waitForSelector("aside.sidebar", { timeout: 45000 });
    await page.evaluate(() => {
      location.hash = "#/regulatory";
    });
    await api(page, "POST", `/tenants/${RA.tenantId}/regulatory/bootstrap`, {});
    const code = `CERT-BLOCK-${Date.now().toString().slice(-6)}`;
    const product = await api(page, "POST", `/tenants/${RA.tenantId}/regulatory/products`, {
      countryCode: "PA",
      category: "Insumos",
      brand: "X",
      regulatoryName: "BLOCK SUBMIT",
      catalogCode: code,
      riskClass: "B",
      currency: "USD",
    });
    expect(product.status).toBeLessThan(300);
    const auths = await api(page, "GET", `/tenants/${RA.tenantId}/regulatory/authorities`);
    const authorityId = (Array.isArray(auths.body) ? auths.body : [])[0]?.id;
    const dossier = await api(page, "POST", `/tenants/${RA.tenantId}/regulatory/dossiers`, {
      productId: (product.body as { id: string }).id,
      authorityId,
      processType: "NewRegistration",
    });
    const id = (dossier.body as { id: string }).id;
    for (const st of ["WaitingManufacturerDocuments", "DocumentsReceived", "Assembling"]) {
      await api(page, "POST", `/tenants/${RA.tenantId}/regulatory/dossiers/${id}/transition`, {
        targetStatus: st,
        waiverReason: st === "DocumentsReceived" ? "CERT waiver evidence N/A" : null,
      });
    }
    const submit = await api(page, "POST", `/tenants/${RA.tenantId}/regulatory/dossiers/${id}/submit`, {});
    const blocked = submit.status >= 400 || (submit.body as { status?: string })?.status !== "Submitted";
    saveSteps("TC-RA-0302-submit-block", [
      { step: "TC-RA-0302", expected: "submit rejected", actual: `${submit.status} ${JSON.stringify(submit.body).slice(0, 120)}`, pass: blocked },
    ]);
    expect(blocked).toBe(true);
  });
});
