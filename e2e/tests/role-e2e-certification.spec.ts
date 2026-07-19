import { test, expect, Page } from "@playwright/test";
import * as fs from "fs";
import * as path from "path";

/**
 * CERTIFICACIÓN FUNCIONAL E2E OBLIGATORIA — recorrido real por rol.
 * Manual = fuente de verdad (docs/user-manual/data/*.json).
 * Cada paso registra: id, acción, esperado (manual), obtenido (sistema),
 * PASS/FAIL, tiempo (ms), captura y observaciones.
 */

const TENANT = "82af3877-2786-4d39-bce8-c981101c771d";
const OTHER_TENANT = "dc7c46ee-cb25-4ed5-b0b4-800788f7f626";
const PASS_PWD = "OwnerStart!2026";
const OUT = path.join(__dirname, "..", "..", "docs", "certification", "evidence", "role-e2e");
const RA = `/tenants/${TENANT}/regulatory`;
const ZERO = "00000000-0000-0000-0000-000000000001";

type StepRecord = {
  id: string;
  action: string;
  expected: string;
  actual: string;
  pass: boolean;
  ms: number;
  screenshot?: string;
  notes?: string;
};

class Journey {
  steps: StepRecord[] = [];
  constructor(public role: string, public email: string, public page: Page) {}

  async step(
    id: string,
    action: string,
    expected: string,
    fn: () => Promise<{ actual: string; pass: boolean; notes?: string; shot?: boolean }>
  ) {
    const t0 = Date.now();
    let rec: StepRecord;
    try {
      const r = await fn();
      rec = { id, action, expected, actual: r.actual, pass: r.pass, ms: Date.now() - t0, notes: r.notes };
      if (r.shot) {
        const file = `${this.role.toLowerCase()}-${id.toLowerCase()}.png`;
        await this.page.screenshot({ path: path.join(OUT, file), fullPage: true }).catch(() => {});
        rec.screenshot = file;
      }
    } catch (e: any) {
      rec = { id, action, expected, actual: `EXCEPTION: ${String(e?.message || e).slice(0, 200)}`, pass: false, ms: Date.now() - t0 };
      const file = `${this.role.toLowerCase()}-${id.toLowerCase()}-error.png`;
      await this.page.screenshot({ path: path.join(OUT, file), fullPage: true }).catch(() => {});
      rec.screenshot = file;
    }
    this.steps.push(rec);
    return rec;
  }

  save() {
    fs.mkdirSync(OUT, { recursive: true });
    fs.writeFileSync(
      path.join(OUT, `journey-${this.role.toLowerCase()}.json`),
      JSON.stringify({ role: this.role, email: this.email, when: new Date().toISOString(), pass: this.steps.every(s => s.pass), steps: this.steps }, null, 2)
    );
  }
}

async function login(page: Page, email: string) {
  await page.goto("/");
  await page.evaluate(() => { localStorage.clear(); sessionStorage.clear(); });
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
  await page.fill("#password", PASS_PWD);
  await page.click("#login-form button[type=submit]");
  await page.waitForSelector("aside.sidebar", { timeout: 45000 });
}

async function logout(page: Page) {
  await page.click("#logout");
  await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 20000 });
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
      try { parsed = JSON.parse(text); } catch { /* raw */ }
      return { status: res.status, body: parsed as any };
    },
    { method, apiPath, body }
  );
}

async function gotoRegulatory(page: Page) {
  await page.evaluate(() => { location.hash = "#/regulatory"; });
  await page.waitForSelector(".ra-shell", { timeout: 30000 });
  await page.waitForTimeout(400);
}

async function openDossier(page: Page, dossierId: string) {
  const dossiersButton = page.locator('#ra-nav [data-view="dossiers"]');
  await expect(dossiersButton).toBeEnabled({ timeout: 20000 });
  const detailLoaded = page.waitForResponse((response) =>
    response.request().method() === "GET"
    && new URL(response.url()).pathname.endsWith(`/regulatory/dossiers/${dossierId}`)
    && response.ok()
  );
  await page.evaluate((id) => {
    (window as any).__raOpenDossier = id;
  }, dossierId);
  await Promise.all([detailLoaded, dossiersButton.click()]);
  await expect(dossiersButton).toBeEnabled({ timeout: 20000 });
  await page.waitForSelector("#ra-back", { timeout: 20000 });
}

async function raTabs(page: Page): Promise<string[]> {
  return page.locator("#ra-nav button").evaluateAll(els => els.map(e => (e as HTMLElement).dataset.view || ""));
}

async function sidebarRoutes(page: Page): Promise<string[]> {
  return page.locator("aside.sidebar .nav-button").evaluateAll(els => els.map(e => (e as HTMLElement).dataset.route || ""));
}

async function statusOf(page: Page, dossierId: string): Promise<string> {
  const res = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
  return res.body?.status || `http=${res.status}`;
}

function setEq(a: string[], b: string[]) {
  return a.length === b.length && a.every(x => b.includes(x));
}

async function commonShellChecks(j: Journey, page: Page, expectedSidebar: string[], expectedTabs: string[]) {
  await j.step("SHELL-SIDEBAR", "Validar sidebar (menús visibles y ocultos)", `Sidebar exacto: [${expectedSidebar.join(", ")}]`, async () => {
    const got = await sidebarRoutes(page);
    return { actual: `[${got.join(", ")}]`, pass: setEq(got, expectedSidebar) };
  });
  await j.step("SHELL-NAVBAR", "Validar navbar (sesión, rol, tenant, breadcrumb)", "Chip de sesión con rol + tenant + breadcrumb visible", async () => {
    const chip = await page.locator(".session-chip").innerText().catch(() => "");
    const tenant = await page.locator(".tenant-chip").innerText().catch(() => "");
    const crumb = await page.locator(".topbar-context").innerText().catch(() => "");
    const ok = chip.length > 0 && tenant.includes("Tenant") && crumb.length > 0;
    return { actual: `chip="${chip.replace(/\s+/g, " ").slice(0, 60)}" tenant="${tenant}"`, pass: ok };
  });
  await j.step("DASHBOARD-KPI", "Validar dashboard (widgets/KPIs/cards)", "Dashboard con tarjetas métricas visibles", async () => {
    await page.evaluate(() => { location.hash = "#/dashboard"; });
    await page.waitForTimeout(1500);
    const cards = await page.locator("#content .card, #content .metric, #content .grid").count();
    return { actual: `bloques=${cards}`, pass: cards > 0, shot: true };
  });
  await j.step("RA-TABS", "Validar pestañas RA visibles/ocultas", `Tabs exactos: [${expectedTabs.join(", ")}]`, async () => {
    await gotoRegulatory(page);
    const got = await raTabs(page);
    return { actual: `[${got.join(", ")}]`, pass: setEq(got, expectedTabs), shot: true };
  });
}

async function negativeUrlProbe(j: Journey, page: Page, route: string) {
  await j.step(`NEG-URL-${route}`, `Acceso por URL directa #/${route}`, "Pantalla bloqueada (Acceso denegado) — manual no asigna esta pantalla al rol", async () => {
    await page.evaluate((r) => { location.hash = `#/${r}`; }, route);
    await page.waitForTimeout(1200);
    const denied = (await page.locator('[data-testid="access-denied"]').count()) > 0;
    const body = (await page.locator("#content").innerText().catch(() => "")).slice(0, 80).replace(/\s+/g, " ");
    return { actual: denied ? "Acceso denegado renderizado" : `Contenido: ${body}`, pass: denied, shot: denied };
  });
}

async function negativeApiProbes(j: Journey, page: Page, probes: Array<{ id: string; method: string; path: string; body?: object }>) {
  for (const p of probes) {
    await j.step(`NEG-API-${p.id}`, `${p.method} ${p.path}`, "HTTP 401/403 (política de permisos)", async () => {
      const res = await api(page, p.method, p.path, p.body);
      return { actual: `http=${res.status}`, pass: res.status === 401 || res.status === 403 };
    });
  }
}

async function jsForceProbe(j: Journey, page: Page, buttonId: string, label: string) {
  await j.step(`NEG-JS-${buttonId}`, `Forzar "${label}" vía JavaScript`, "Botón inexistente en DOM (oculto por RBAC) — no ejecutable", async () => {
    const exists = await page.evaluate((id) => !!document.getElementById(id), buttonId);
    return { actual: exists ? "botón presente (violación)" : "botón ausente del DOM", pass: !exists };
  });
}

// ---------------------------------------------------------------------------

test.describe.serial("Role E2E certification", () => {
  let dossierId = "";
  let catalogCode = "";

  test("RA Specialist — recorrido E2E completo", async ({ page }) => {
    test.setTimeout(300000);
    const j = new Journey("RA-SPEC", "ra.spec@cert.local", page);

    await j.step("LOGIN", "Login con credenciales del laboratorio", "Autenticación exitosa y shell visible", async () => {
      await login(page, j.email);
      return { actual: "shell visible", pass: true, shot: true };
    });

    await commonShellChecks(j, page, ["dashboard", "regulatory"],
      ["dashboard", "portfolio", "pipeline", "dossiers", "registrations", "manufacturers"]);

    await j.step("CREATE-PRODUCT-DOSSIER", "Crear producto + expediente (modal enterprise)", "Producto y expediente creados; modal con campos del manual (marca, nombre, código, riesgo A/B/C, país PA, autoridad)", async () => {
      await gotoRegulatory(page);
      await page.click('#ra-nav [data-view="portfolio"]');
      await page.waitForSelector("#ra-new-product", { timeout: 20000 });
      await page.click("#ra-new-product");
      await expect(page.locator("#ra-new-product-modal .ra-modal")).toBeVisible({ timeout: 15000 });
      const risks = await page.locator("#ra-np-risk option").allTextContents();
      const country = await page.locator("#ra-np-country").inputValue();
      catalogCode = `E2E-${Date.now().toString().slice(-6)}`;
      await page.fill("#ra-np-brand", "E2ECERT");
      await page.fill("#ra-np-name", `Producto E2E ${catalogCode}`);
      await page.fill("#ra-np-code", catalogCode);
      await page.selectOption("#ra-np-risk", "B");
      await page.locator("#ra-new-product-modal button[type=submit]").click();
      await expect(page.locator("#ra-new-product-modal .ra-modal")).toBeHidden({ timeout: 20000 });
      const products = await api(page, "GET", `${RA}/products?searchText=${catalogCode}`);
      const product = (products.body as any[]).find((x: any) => x.catalogCode === catalogCode);
      const dossiers = await api(page, "GET", `${RA}/dossiers`);
      const d = (dossiers.body as any[]).find((x: any) => x.productId === product?.id);
      dossierId = d?.id || "";
      return {
        actual: `riesgo=[${risks.join(",")}] país=${country} producto=${product ? "OK" : "NO"} expediente=${d?.status || "NO"}`,
        pass: risks.join(",") === "A,B,C" && country === "PA" && !!product && !!dossierId,
        shot: true,
      };
    });

    await j.step("UPLOAD-REQUIREMENTS", "Cargar requisitos/documentos (Marcar recibido con evidencia)", "Requisitos pasan a Received con storedFileId", async () => {
      await openDossier(page, dossierId);
      const pendingIds: string[] = await page.evaluate(async (args) => {
        const token = localStorage.getItem("c360.token");
        const res = await fetch(`/api/v1${args.path}`, { headers: { Authorization: `Bearer ${token}` } });
        const d = await res.json();
        return (d.requirements || []).filter((r: any) => r.isRequired && r.status === "Pending").map((r: any) => r.id);
      }, { path: `${RA}/dossiers/${dossierId}` });
      for (const reqId of pendingIds) {
        await page.locator(`input[data-req-file="${reqId}"]`).setInputFiles({
          name: `requirement-${reqId}.pdf`,
          mimeType: "application/pdf",
          buffer: Buffer.from(`%PDF-1.4\nCompliance360 requirement ${reqId}\n`),
        });
        await page.locator(`button[data-prep="${reqId}"]`).click();
        await expect.poll(async () => {
          const detail = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
          return (detail.body.requirements as any[]).find((r: any) => r.id === reqId)?.status;
        }, { timeout: 15000 }).toBe("Received");
        const requirementCard = page.locator("#ra-reqs .ra-req").filter({
          has: page.locator(`input[data-req-file="${reqId}"]`),
        });
        await expect(requirementCard.locator(".ra-req-header > .ra-badge")).toContainText("Received", { timeout: 20000 });
      }
      const det = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
      const received = (det.body.requirements as any[]).filter((r: any) => r.status === "Received").length;
      return { actual: `requisitos obligatorios Received=${received}/${pendingIds.length}`, pass: received >= pendingIds.length, shot: true };
    });

    await j.step("EDIT-DATES", "Editar mientras esté permitido (fechas plan)", "PUT /dates permitido para Specialist (DOSSIER.UPDATE)", async () => {
      const res = await api(page, "PUT", `${RA}/dossiers/${dossierId}/dates`, {
        estimatedSubmissionOn: new Date(Date.now() + 86400000 * 30).toISOString(),
      });
      return { actual: `http=${res.status}`, pass: res.status < 300 };
    });

    await j.step("SEND-TO-REVIEW", "Transiciones de preparación y envío a revisión", "Draft→…→UnderTechnicalReview", async () => {
      await openDossier(page, dossierId);
      for (const target of ["Planning", "WaitingManufacturerDocuments", "DocumentsReceived", "Assembling"]) {
        await page.locator(`button[data-next="${target}"]`).click();
        await page.waitForTimeout(900);
      }
      await page.locator("#ra-send-technical-review").click();
      await page.waitForTimeout(900);
      const st = await statusOf(page, dossierId);
      return { actual: `status=${st}`, pass: st === "UnderTechnicalReview", shot: true };
    });

    await j.step("SOD-SELF-REVIEW", "SoD: intentar auto-aceptar requisito propio", "Denegado (PreventSelfReview)", async () => {
      const det = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
      const req = (det.body.requirements as any[]).find((r: any) => r.isCritical);
      const res = await api(page, "PUT", `${RA}/dossiers/${dossierId}/requirements/${req.id}`, {
        status: "Accepted", notes: "self", storedFileId: crypto.randomUUID(),
      });
      return { actual: `http=${res.status}`, pass: res.status >= 400 };
    });

    await j.step("NO-APPROVE-BTN", "Verificar que NO pueda aprobar (UI)", "Botón de aprobación interna ausente", async () => {
      await openDossier(page, dossierId);
      const count = await page.locator("#ra-approve-internal").count();
      return { actual: `botones=${count}`, pass: count === 0 };
    });
    await j.step("NO-SUBMIT-BTN", "Verificar que NO pueda someter (UI)", "Botón de sometimiento ausente", async () => {
      const count = await page.locator("#ra-submit").count();
      return { actual: `botones=${count}`, pass: count === 0 };
    });

    await negativeApiProbes(j, page, [
      { id: "APPROVE-INT", method: "POST", path: `${RA}/dossiers/${dossierId}/approve-for-submission`, body: { notes: "x" } },
      { id: "SUBMIT", method: "POST", path: `${RA}/dossiers/${dossierId}/submit`, body: {} },
      { id: "APPROVE-EXT", method: "POST", path: `${RA}/dossiers/${dossierId}/approve`, body: { registrationNumber: "X", issuedOn: new Date().toISOString() } },
      { id: "BOOTSTRAP", method: "POST", path: `${RA}/bootstrap`, body: {} },
      { id: "LICENSE", method: "POST", path: `${RA}/operating-licenses`, body: { companyName: "X", licenseType: "X" } },
    ]);
    await negativeUrlProbe(j, page, "tenant-administration");
    await negativeUrlProbe(j, page, "security");
    await negativeUrlProbe(j, page, "audit-trail");

    await j.step("TENANT-ISOLATION", "Intentar leer el expediente usando otro tenantId", "HTTP 401/403 y ningún dato expuesto", async () => {
      const result = await api(page, "GET", `/tenants/${OTHER_TENANT}/regulatory/dossiers/${dossierId}`);
      return { actual: `http=${result.status}`, pass: result.status === 401 || result.status === 403 };
    });

    await j.step("HISTORY", "Historial/auditoría del expediente", "Historial con eventos registrados", async () => {
      await gotoRegulatory(page);
      await openDossier(page, dossierId);
      const hist = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
      const events = (hist.body.history || []).length;
      return { actual: `eventos=${events}`, pass: events > 0 };
    });

    await j.step("LOGOUT", "Cerrar sesión", "Retorno a pantalla de login", async () => {
      await logout(page);
      return { actual: "login visible", pass: true };
    });

    j.save();
    const failed = j.steps.filter(s => !s.pass);
    expect(failed, JSON.stringify(failed, null, 2)).toHaveLength(0);
  });

  test("RA Reviewer — recorrido E2E completo", async ({ page }) => {
    test.setTimeout(300000);
    const j = new Journey("RA-REV", "ra.rev@cert.local", page);
    expect(dossierId, "requires specialist dossier").toBeTruthy();

    await j.step("LOGIN", "Login", "Autenticación exitosa", async () => {
      await login(page, j.email);
      return { actual: "shell visible", pass: true, shot: true };
    });

    await commonShellChecks(j, page, ["dashboard", "regulatory"],
      ["dashboard", "pipeline", "dossiers", "registrations"]);

    await j.step("OPEN-DOSSIER", "Abrir expediente recibido", "Detalle visible con requisitos y botones Aceptar/Rechazar", async () => {
      await gotoRegulatory(page);
      await openDossier(page, dossierId);
      const accept = await page.locator("button[data-accept]").count();
      const reject = await page.locator("button[data-reject]").count();
      return { actual: `aceptar=${accept} rechazar=${reject}`, pass: accept > 0 && reject > 0, shot: true };
    });

    await j.step("RETURN-REQ", "Devolver un requisito (Rechazar con comentario)", "Requisito no crítico queda Rejected", async () => {
      const det = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
      const nonCrit = (det.body.requirements as any[]).find((r: any) => !r.isCritical && r.status !== "Accepted");
      const btn = page.locator(`button[data-reject="${nonCrit.id}"]`);
      await btn.click();
      const rejectModal = page.locator("#ra-reject-requirement-modal .ra-modal");
      await expect(rejectModal).toBeVisible({ timeout: 15000 });
      await page.fill(
        '#ra-reject-requirement-modal textarea[name="notes"]',
        "Evidencia insuficiente: adjuntar documento vigente y legible."
      );
      await page.locator("#ra-reject-requirement-modal button[type=submit]").click();
      await expect(rejectModal).toBeHidden({ timeout: 20000 });
      const det2 = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
      const st = (det2.body.requirements as any[]).find((r: any) => r.id === nonCrit.id)?.status;
      return { actual: `status=${st}`, pass: st === "Rejected" };
    });

    await j.step("REVIEW-ACCEPT-ALL", "Revisar documentos y aprobar revisión (con comentarios)", "Todos los requisitos obligatorios Accepted", async () => {
      for (let guard = 0; guard < 40; guard++) {
        const pending = await page.evaluate(async (args) => {
          const token = localStorage.getItem("c360.token");
          const res = await fetch(`/api/v1${args.path}`, { headers: { Authorization: `Bearer ${token}` } });
          const d = await res.json();
          return (d.requirements || []).filter((r: any) => r.isRequired && !["Accepted", "Waived", "NotRequired"].includes(r.status)).map((r: any) => r.id);
        }, { path: `${RA}/dossiers/${dossierId}` });
        if (!pending.length) break;
        const btn = page.locator(`button[data-accept="${pending[0]}"]`);
        if (await btn.count()) {
          await btn.click();
          await page.waitForTimeout(700);
        } else {
          await api(page, "PUT", `${RA}/dossiers/${dossierId}/requirements/${pending[0]}`, {
            status: "Accepted", notes: "Revisión técnica aceptada (comentario reviewer)",
          });
          await openDossier(page, dossierId);
        }
      }
      const det = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
      const pendingRequired = (det.body.requirements as any[]).filter((r: any) => r.isRequired && !["Accepted", "Waived", "NotRequired"].includes(r.status)).length;
      return { actual: `obligatorios pendientes=${pendingRequired}`, pass: pendingRequired === 0, shot: true };
    });

    await j.step("COMPLETE-TECHNICAL-REVIEW", "Completar revisión técnica", "ReadyForSubmission mediante endpoint V2 autorizado", async () => {
      const det = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
      const result = await page.evaluate(async ({ tenantId, dossierId, revision }) => {
        const token = localStorage.getItem("c360.token");
        const response = await fetch(`/api/v2/tenants/${tenantId}/regulatory/dossiers/${dossierId}/technical-review/complete`, {
          method: "POST",
          headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
          body: JSON.stringify({
            expectedRevision: revision,
            correctionRequestId: null,
            reason: "Revisión técnica completa por rol Reviewer"
          })
        });
        const text = await response.text();
        return { status: response.status, body: text ? JSON.parse(text) : null };
      }, { tenantId: TENANT, dossierId, revision: det.body.revision });
      return { actual: `http=${result.status};status=${result.body?.status}`, pass: result.status === 200 && result.body?.status === "ReadyForSubmission", shot: true };
    });

    await j.step("NO-APPROVE-BTN", "Confirmar que NO puede aprobar internamente (UI)", "Botón ausente", async () => {
      await openDossier(page, dossierId);
      const count = await page.locator("#ra-approve-internal").count();
      return { actual: `botones=${count}`, pass: count === 0 };
    });
    await j.step("NO-SUBMIT-BTN", "Confirmar que NO puede someter (UI)", "Botón ausente", async () => {
      const count = await page.locator("#ra-submit").count();
      return { actual: `botones=${count}`, pass: count === 0 };
    });

    await negativeApiProbes(j, page, [
      { id: "APPROVE-INT", method: "POST", path: `${RA}/dossiers/${dossierId}/approve-for-submission`, body: { notes: "x" } },
      { id: "SUBMIT", method: "POST", path: `${RA}/dossiers/${dossierId}/submit`, body: {} },
      { id: "CREATE-PRODUCT", method: "POST", path: `${RA}/products`, body: { countryCode: "PA", category: "X", brand: "X", regulatoryName: "X", catalogCode: `NO-${Date.now()}`, riskClass: "A", currency: "USD" } },
      { id: "CREATE-DOSSIER", method: "POST", path: `${RA}/dossiers`, body: { productId: ZERO, authorityId: ZERO, processType: "NewRegistration" } },
      { id: "TRANSITION", method: "POST", path: `${RA}/dossiers/${dossierId}/transition`, body: { targetStatus: "Assembling" } },
      { id: "OBSERVE", method: "POST", path: `${RA}/dossiers/${dossierId}/observations`, body: { description: "x", receivedOn: new Date().toISOString() } },
    ]);
    await negativeUrlProbe(j, page, "tenant-administration");
    await jsForceProbe(j, page, "ra-new-product", "Nuevo producto + expediente");

    await j.step("LOGOUT", "Cerrar sesión", "Login visible", async () => {
      await logout(page);
      return { actual: "login visible", pass: true };
    });

    j.save();
    const failed = j.steps.filter(s => !s.pass);
    expect(failed, JSON.stringify(failed, null, 2)).toHaveLength(0);
  });

  test("RA Approver — recorrido E2E completo", async ({ page }) => {
    test.setTimeout(300000);
    const j = new Journey("RA-APPR", "ra.appr@cert.local", page);
    expect(dossierId, "requires reviewed dossier").toBeTruthy();

    await j.step("LOGIN", "Login", "Autenticación exitosa", async () => {
      await login(page, j.email);
      return { actual: "shell visible", pass: true, shot: true };
    });

    await commonShellChecks(j, page, ["dashboard", "regulatory"],
      ["dashboard", "pipeline", "dossiers", "registrations"]);

    await j.step("VALIDATE-STATE", "Revisar expediente y validar estado", "Estado ReadyForSubmission con flujo visual", async () => {
      await gotoRegulatory(page);
      await openDossier(page, dossierId);
      const st = await statusOf(page, dossierId);
      const flow = await page.locator(".ra-flow, .ra-badge").count();
      return { actual: `status=${st} indicadores=${flow}`, pass: st === "ReadyForSubmission" && flow > 0, shot: true };
    });

    await j.step("NO-EDIT-RESTRICTED", "Confirmar que NO puede modificar información restringida", "PUT requisitos/fechas y transición → 401/403", async () => {
      const det = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
      const req = (det.body.requirements as any[])[0];
      const r1 = await api(page, "PUT", `${RA}/dossiers/${dossierId}/requirements/${req.id}`, { status: "Received" });
      const r2 = await api(page, "PUT", `${RA}/dossiers/${dossierId}/dates`, { estimatedSubmissionOn: new Date().toISOString() });
      const r3 = await api(page, "POST", `${RA}/dossiers/${dossierId}/transition`, { targetStatus: "Assembling" });
      const all = [r1.status, r2.status, r3.status];
      return { actual: `http=[${all.join(",")}]`, pass: all.every(s => s === 401 || s === 403) };
    });

    await j.step("APPROVE-INTERNAL", "Aprobar internamente (UI)", "Estado pasa a ApprovedForSubmission", async () => {
      await expect(page.locator("#ra-approve-internal")).toBeVisible({ timeout: 15000 });
      await page.click("#ra-approve-internal");
      await expect.poll(async () => statusOf(page, dossierId), { timeout: 20000 }).toBe("ApprovedForSubmission");
      return { actual: "status=ApprovedForSubmission", pass: true, shot: true };
    });

    await j.step("NO-SUBMIT-BTN", "Confirmar que NO puede someter (UI)", "Botón ausente tras aprobar", async () => {
      await openDossier(page, dossierId);
      const count = await page.locator("#ra-submit").count();
      return { actual: `botones=${count}`, pass: count === 0 };
    });

    await negativeApiProbes(j, page, [
      { id: "SUBMIT", method: "POST", path: `${RA}/dossiers/${dossierId}/submit`, body: {} },
      { id: "APPROVE-EXT", method: "POST", path: `${RA}/dossiers/${dossierId}/approve`, body: { registrationNumber: "X", issuedOn: new Date().toISOString() } },
      { id: "CREATE-PRODUCT", method: "POST", path: `${RA}/products`, body: { countryCode: "PA", category: "X", brand: "X", regulatoryName: "X", catalogCode: `NO-${Date.now()}`, riskClass: "A", currency: "USD" } },
      { id: "OBSERVE", method: "POST", path: `${RA}/dossiers/${dossierId}/observations`, body: { description: "x", receivedOn: new Date().toISOString() } },
    ]);
    await negativeUrlProbe(j, page, "tenant-administration");
    await jsForceProbe(j, page, "ra-submit", "Registrar sometimiento");

    await j.step("LOGOUT", "Cerrar sesión", "Login visible", async () => {
      await logout(page);
      return { actual: "login visible", pass: true };
    });

    j.save();
    const failed = j.steps.filter(s => !s.pass);
    expect(failed, JSON.stringify(failed, null, 2)).toHaveLength(0);
  });

  test("RA Submitter — recorrido E2E completo", async ({ page }) => {
    test.setTimeout(300000);
    const j = new Journey("RA-SUB", "ra.sub@cert.local", page);
    expect(dossierId, "requires approved dossier").toBeTruthy();

    await j.step("LOGIN", "Login", "Autenticación exitosa", async () => {
      await login(page, j.email);
      return { actual: "shell visible", pass: true, shot: true };
    });

    await commonShellChecks(j, page, ["dashboard", "regulatory"],
      ["dashboard", "pipeline", "dossiers", "registrations"]);

    await j.step("FIND-APPROVED", "Buscar expediente aprobado internamente", "Expediente en ApprovedForSubmission accesible", async () => {
      await gotoRegulatory(page);
      const st = await statusOf(page, dossierId);
      return { actual: `status=${st}`, pass: st === "ApprovedForSubmission" };
    });

    await j.step("SUBMIT", "Someter a la autoridad (registra fecha de sometimiento)", "Estado Submitted + submittedOn registrado", async () => {
      await openDossier(page, dossierId);
      await expect(page.locator("#ra-submit")).toBeVisible({ timeout: 15000 });
      await page.click("#ra-submit");
      const submitModal = page.locator("#ra-submit-modal .ra-modal");
      await expect(submitModal).toBeVisible({ timeout: 15000 });
      const submissionStamp = Date.now();
      await page.fill('#ra-submit-modal input[name="procedureNumber"]', `TRAM-${submissionStamp}`);
      await page.fill('#ra-submit-modal input[name="externalNumber"]', `EXT-${submissionStamp}`);
      await page.locator('#ra-submit-modal input[name="proof"]').setInputFiles({
        name: `submission-${submissionStamp}.pdf`,
        mimeType: "application/pdf",
        buffer: Buffer.from("%PDF-1.4\nCompliance360 submission evidence\n"),
      });
      await page.locator("#ra-submit-modal button[type=submit]").click();
      await expect(submitModal).toBeHidden({ timeout: 20000 });
      await expect.poll(async () => statusOf(page, dossierId), { timeout: 20000 }).toBe("Submitted");
      const det = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
      return {
        actual: `status=${det.body.status} submittedOn=${String(det.body.submittedOn || "").slice(0, 10)}`,
        pass: det.body.status === "Submitted" && !!det.body.submittedOn,
        shot: true,
        notes: "El Submitter registra trámite, número externo, fecha y comprobante; el Manager/QM registra después la resolución CT/RS.",
      };
    });

    await j.step("NO-EDIT-LOCKED", "Confirmar que NO puede editar información bloqueada", "Requisitos/fechas/transiciones → 401/403", async () => {
      const det = await api(page, "GET", `${RA}/dossiers/${dossierId}`);
      const req = (det.body.requirements as any[])[0];
      const r1 = await api(page, "PUT", `${RA}/dossiers/${dossierId}/requirements/${req.id}`, { status: "Received" });
      const r2 = await api(page, "PUT", `${RA}/dossiers/${dossierId}/dates`, { estimatedApprovalOn: new Date().toISOString() });
      const r3 = await api(page, "POST", `${RA}/dossiers/${dossierId}/transition`, { targetStatus: "Assembling" });
      const all = [r1.status, r2.status, r3.status];
      return { actual: `http=[${all.join(",")}]`, pass: all.every(s => s === 401 || s === 403) };
    });

    await negativeApiProbes(j, page, [
      { id: "APPROVE-INT", method: "POST", path: `${RA}/dossiers/${dossierId}/approve-for-submission`, body: { notes: "x" } },
      { id: "APPROVE-EXT", method: "POST", path: `${RA}/dossiers/${dossierId}/approve`, body: { registrationNumber: "X", issuedOn: new Date().toISOString() } },
      { id: "OBSERVE", method: "POST", path: `${RA}/dossiers/${dossierId}/observations`, body: { description: "x", receivedOn: new Date().toISOString() } },
      { id: "CREATE-DOSSIER", method: "POST", path: `${RA}/dossiers`, body: { productId: ZERO, authorityId: ZERO, processType: "NewRegistration" } },
    ]);
    await negativeUrlProbe(j, page, "tenant-administration");
    await jsForceProbe(j, page, "ra-approve-internal", "Aprobar internamente");

    await j.step("LOGOUT", "Cerrar sesión", "Login visible", async () => {
      await logout(page);
      return { actual: "login visible", pass: true };
    });

    j.save();
    const failed = j.steps.filter(s => !s.pass);
    expect(failed, JSON.stringify(failed, null, 2)).toHaveLength(0);
  });

  test("RA Manager — decisión externa (registro de trámite/resolución/fechas)", async ({ page }) => {
    test.setTimeout(300000);
    const j = new Journey("RA-MGR", "ra.mgr@cert.local", page);
    expect(dossierId, "requires submitted dossier").toBeTruthy();

    await j.step("LOGIN", "Login", "Autenticación exitosa", async () => {
      await login(page, j.email);
      return { actual: "shell visible", pass: true, shot: true };
    });

    await j.step("EXTERNAL-DECISION", "Registrar número CT/RS, resolución y fechas (modal)", "Estado Approved/Closed + CT/RS persistido", async () => {
      await gotoRegulatory(page);
      await openDossier(page, dossierId);
      await page.click("#ra-approve-ext");
      await expect(page.locator("#ra-approve-ext-modal .ra-modal")).toBeVisible({ timeout: 15000 });
      const ctrs = `MQ-E2E-${Date.now().toString().slice(-5)}`;
      await page.fill('#ra-approve-ext-modal input[name="registrationNumber"]', ctrs);
      await page.fill('#ra-approve-ext-modal textarea[name="notes"]', "Resolución favorable de la autoridad (E2E)");
      await page.locator('#ra-approve-ext-modal input[name="resolution"]').setInputFiles({
        name: `resolution-${ctrs}.pdf`,
        mimeType: "application/pdf",
        buffer: Buffer.from(`%PDF-1.4\nCompliance360 resolution ${ctrs}\n`),
      });
      await page.locator("#ra-approve-ext-modal button[type=submit]").click();
      await expect(page.locator("#ra-approve-ext-modal .ra-modal")).toBeHidden({ timeout: 20000 });
      await page.waitForTimeout(900);
      const st = await statusOf(page, dossierId);
      const regs = await api(page, "GET", `${RA}/registrations?searchText=${ctrs}`);
      const found = (regs.body as any[]).some((r: any) => r.registrationNumber === ctrs);
      return { actual: `status=${st} ctrs=${found ? ctrs : "NO"}`, pass: (st === "Approved" || st === "Closed") && found, shot: true };
    });

    await j.step("LOGOUT", "Cerrar sesión", "Login visible", async () => {
      await logout(page);
      return { actual: "login visible", pass: true };
    });

    j.save();
    const failed = j.steps.filter(s => !s.pass);
    expect(failed, JSON.stringify(failed, null, 2)).toHaveLength(0);
  });

  test("RA Viewer — recorrido E2E completo (solo lectura)", async ({ page }) => {
    test.setTimeout(300000);
    const j = new Journey("RA-VIEW", "ra.view@cert.local", page);
    expect(dossierId, "requires dossier").toBeTruthy();

    await j.step("LOGIN", "Login", "Autenticación exitosa", async () => {
      await login(page, j.email);
      return { actual: "shell visible", pass: true, shot: true };
    });

    await commonShellChecks(j, page, ["dashboard", "regulatory"],
      ["dashboard", "portfolio", "pipeline", "dossiers", "registrations", "alerts"]);

    await j.step("BROWSE-ALL", "Navegar por todos los módulos permitidos", "Cada pestaña permitida renderiza sin error", async () => {
      await gotoRegulatory(page);
      const results: string[] = [];
      for (const v of ["dashboard", "portfolio", "pipeline", "dossiers", "registrations", "alerts"]) {
        await page.click(`#ra-nav [data-view="${v}"]`);
        await page.waitForTimeout(700);
        const text = await page.locator("#ra-body").innerText().catch(() => "");
        results.push(`${v}=${text.toLowerCase().includes("error") ? "ERROR" : "OK"}`);
      }
      return { actual: results.join(" "), pass: results.every(r => r.endsWith("OK")), shot: true };
    });

    await j.step("OPEN-RECORD", "Abrir registro (expediente en lectura)", "Detalle visible sin acciones de mutación", async () => {
      await openDossier(page, dossierId);
      const mutBtns = await page.locator("button[data-next], button[data-prep], button[data-accept], button[data-reject], #ra-approve-internal, #ra-submit, #ra-observe, #ra-approve-ext").count();
      const noActions = (await page.locator(".ra-actions em").count()) > 0;
      return { actual: `botones mutación=${mutBtns} aviso sin acciones=${noActions}`, pass: mutBtns === 0, shot: true };
    });

    await j.step("SEARCH-FILTER", "Buscar y filtrar registros (API de lectura)", "Búsqueda por texto retorna 200", async () => {
      const r1 = await api(page, "GET", `${RA}/dossiers?searchText=E2E`);
      const r2 = await api(page, "GET", `${RA}/products?searchText=${catalogCode || "E2E"}`);
      const r3 = await api(page, "GET", `${RA}/registrations?searchText=MQ`);
      return { actual: `http=[${r1.status},${r2.status},${r3.status}]`, pass: [r1, r2, r3].every(r => r.status === 200) };
    });

    await j.step("NO-EXPORT", "Exportar", "Manual no concede exportación al Viewer → no debe existir control de exportación", async () => {
      const exportBtns = await page.locator("#ra-body button", { hasText: /export/i }).count();
      return { actual: `botones export=${exportBtns}`, pass: exportBtns === 0, notes: "roles.json (regulatory-viewer) no incluye exportar en 'Qué puedo hacer'." };
    });

    // Negativas: crear/editar/eliminar/aprobar/revisar/someter/importar/estados
    await negativeApiProbes(j, page, [
      { id: "CREATE", method: "POST", path: `${RA}/products`, body: { countryCode: "PA", category: "X", brand: "X", regulatoryName: "X", catalogCode: `NO-${Date.now()}`, riskClass: "A", currency: "USD" } },
      { id: "CREATE-DOSSIER", method: "POST", path: `${RA}/dossiers`, body: { productId: ZERO, authorityId: ZERO, processType: "NewRegistration" } },
      { id: "EDIT", method: "PUT", path: `${RA}/dossiers/${dossierId}/dates`, body: { estimatedSubmissionOn: new Date().toISOString() } },
      { id: "REVIEW", method: "PUT", path: `${RA}/dossiers/${dossierId}/requirements/${ZERO}`, body: { status: "Accepted" } },
      { id: "APPROVE-INT", method: "POST", path: `${RA}/dossiers/${dossierId}/approve-for-submission`, body: { notes: "x" } },
      { id: "SUBMIT", method: "POST", path: `${RA}/dossiers/${dossierId}/submit`, body: {} },
      { id: "APPROVE-EXT", method: "POST", path: `${RA}/dossiers/${dossierId}/approve`, body: { registrationNumber: "X", issuedOn: new Date().toISOString() } },
      { id: "STATE", method: "POST", path: `${RA}/dossiers/${dossierId}/transition`, body: { targetStatus: "Cancelled" } },
      { id: "IMPORT", method: "POST", path: `${RA}/imports/stage`, body: { sourceFileName: "x.xlsx", rowsJson: "[]" } },
      { id: "OBSERVE", method: "POST", path: `${RA}/dossiers/${dossierId}/observations`, body: { description: "x", receivedOn: new Date().toISOString() } },
      { id: "SOD", method: "PUT", path: `${RA}/sod-settings`, body: {} },
      { id: "MFR", method: "POST", path: `${RA}/manufacturers`, body: { legalName: "X", countryCode: "CN" } },
      { id: "LICENSE", method: "POST", path: `${RA}/operating-licenses`, body: { companyName: "X", licenseType: "X" } },
      { id: "TAMPER-ID", method: "PUT", path: `${RA}/dossiers/${ZERO}/dates`, body: { estimatedSubmissionOn: new Date().toISOString() } },
    ]);

    // Negativas: URL directa a pantallas ocultas
    await negativeUrlProbe(j, page, "tenant-administration");
    await negativeUrlProbe(j, page, "security");
    await negativeUrlProbe(j, page, "audit-trail");
    await negativeUrlProbe(j, page, "superadmin-platform");
    await negativeUrlProbe(j, page, "configuration");

    // Negativas: forzar por JavaScript botones ocultos
    await jsForceProbe(j, page, "ra-new-product", "Nuevo producto");
    await jsForceProbe(j, page, "ra-add-mfr", "Alta fabricante");
    await jsForceProbe(j, page, "ra-add-lic", "Nueva licencia");

    await j.step("NEG-JS-FETCH", "Forzar mutación vía JavaScript (fetch manipulado con IDs alterados)", "Backend deniega 401/403 aunque el request se construya manualmente", async () => {
      const res = await page.evaluate(async (args) => {
        const token = localStorage.getItem("c360.token");
        const r = await fetch(`/api/v1${args.ra}/dossiers/${args.id}/transition`, {
          method: "POST",
          headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}`, "X-Forged": "true" },
          body: JSON.stringify({ targetStatus: "Approved" }),
        });
        return r.status;
      }, { ra: RA, id: dossierId });
      return { actual: `http=${res}`, pass: res === 401 || res === 403 };
    });

    await j.step("LOGOUT", "Cerrar sesión", "Login visible", async () => {
      await logout(page);
      return { actual: "login visible", pass: true };
    });

    j.save();
    const failed = j.steps.filter(s => !s.pass);
    expect(failed, JSON.stringify(failed, null, 2)).toHaveLength(0);
  });
});
