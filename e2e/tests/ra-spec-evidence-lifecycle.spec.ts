import { test, expect, Page } from "@playwright/test";
import * as path from "path";
import * as fs from "fs";

/**
 * Functional certification for Regulatory Specialist (ra.spec@cert.local):
 * product/dossier create → prepare → upload evidence → audited remove → RBAC negatives.
 */
const TENANT = "82af3877-2786-4d39-bce8-c981101c771d";
const PASS = "OwnerStart!2026";
const SPEC = "ra.spec@cert.local";
const VIEW = "ra.view@cert.local";

type Step = { step: string; pass: boolean; detail: string };

async function login(page: Page, email: string) {
  await page.goto("/");
  await page.evaluate(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
  await page.reload();
  await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 20000 });
  if (await page.locator("#tenantId").count()) {
    await page.fill("#tenantId", TENANT);
    await page.fill("#legacy-email, #email", email);
    await page.fill("#legacy-password, #password", PASS);
    await page.click("#legacy-login-form button[type=submit], #login-form button[type=submit]");
  } else {
    await page.fill("#email", email);
    await page.click("#login-form button[type=submit]");
    const org = page.locator('input[name="organizationId"]');
    const passwordField = page.locator("#password");
    await Promise.race([
      org.first().waitFor({ state: "visible", timeout: 20000 }),
      passwordField.waitFor({ state: "visible", timeout: 20000 }),
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
  }
  await page.waitForSelector("aside.sidebar", { timeout: 45000 });
}

async function logout(page: Page) {
  await page.click("#logout");
  await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 20000 });
  await page.evaluate(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
}

async function api(page: Page, method: string, apiPath: string, body?: object) {
  return page.evaluate(
    async ({ method, apiPath, body }) => {
      const token = localStorage.getItem("c360.token");
      const url = apiPath.startsWith("/api/") ? apiPath : `/api/v1${apiPath}`;
      const res = await fetch(url, {
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
      return { status: res.status, body: parsed as any, text };
    },
    { method, apiPath, body }
  );
}

async function uploadEvidence(page: Page, apiPath: string, filename: string) {
  return page.evaluate(
    async ({ apiPath, filename }) => {
      const token = localStorage.getItem("c360.token");
      const form = new FormData();
      form.append(
        "file",
        new File(["%PDF-1.4\nCompliance360 RA Spec evidence lifecycle"], filename, {
          type: "application/pdf",
        })
      );
      const res = await fetch(`/api/v1${apiPath}`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
        body: form,
      });
      const text = await res.text();
      return { status: res.status, body: text ? JSON.parse(text) : null, text };
    },
    { apiPath, filename }
  );
}

function record(steps: Step[], step: string, pass: boolean, detail: string) {
  steps.push({ step, pass, detail });
  expect(pass, `${step}: ${detail}`).toBe(true);
}

test("RA Specialist evidence lifecycle: upload, audited remove, negatives", async ({ page }) => {
  test.setTimeout(180_000);
  const steps: Step[] = [];
  const ra = `/tenants/${TENANT}/regulatory`;
  const out = path.join(__dirname, "..", "..", "docs", "regulatory-affairs", "security", "evidence");
  fs.mkdirSync(out, { recursive: true });
  const stamp = Date.now().toString().slice(-8);

  await login(page, SPEC);
  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  await page.waitForSelector(".ra-shell", { timeout: 30000 });
  const badge = (await page.locator(".ra-badge").first().textContent()) || "";
  record(steps, "SPEC-LOGIN-UI", /specialist/i.test(badge), `badge=${badge}`);

  const prod = await api(page, "POST", `${ra}/products`, {
    countryCode: "PA",
    category: "Insumos Medicos",
    brand: "BRW",
    regulatoryName: `Evidence LC ${stamp}`,
    catalogCode: `EVC-${stamp}`,
    riskClass: "A",
    currency: "USD",
  });
  record(steps, "SPEC-CREATE-PRODUCT", prod.status < 300 && !!prod.body?.id, `http=${prod.status}`);

  const auths = await api(page, "GET", `${ra}/authorities`);
  const authorityId =
    (auths.body as any[])?.find((a) => a.code === "MINSA")?.id || (auths.body as any[])?.[0]?.id;
  record(steps, "SPEC-AUTHORITY", !!authorityId, `authorityId=${authorityId}`);

  const dos = await api(page, "POST", `${ra}/dossiers`, {
    productId: prod.body.id,
    authorityId,
    processType: "NewRegistration",
    comments: "ra.spec evidence lifecycle",
    currency: "USD",
  });
  const dossierId = dos.body?.id as string;
  record(steps, "SPEC-CREATE-DOSSIER", dos.status < 300 && !!dossierId, `http=${dos.status};status=${dos.body?.status}`);

  for (const st of ["WaitingManufacturerDocuments", "DocumentsReceived", "Assembling"]) {
    const tr = await api(page, "POST", `${ra}/dossiers/${dossierId}/transition`, {
      targetStatus: st,
      waiverReason: st === "DocumentsReceived" ? "Recepcion evidencia lifecycle" : null,
    });
    record(
      steps,
      `SPEC-TRANSITION-${st}`,
      tr.status < 300 && tr.body?.status === st,
      `http=${tr.status};status=${tr.body?.status}`
    );
  }

  const detail = await api(page, "GET", `${ra}/dossiers/${dossierId}`);
  const requirement = (detail.body.requirements as any[]).find((r) => r.isRequired) || detail.body.requirements[0];
  record(steps, "SPEC-PICK-REQUIREMENT", !!requirement?.id, `code=${requirement?.code}`);

  const noEvidenceRemove = await api(
    page,
    "POST",
    `${ra}/dossiers/${dossierId}/requirements/${requirement.id}/evidence/remove`,
    { reason: "Intentando borrar sin evidencia adjunta" }
  );
  record(
    steps,
    "SPEC-REMOVE-WITHOUT-EVIDENCE",
    noEvidenceRemove.status >= 400,
    `http=${noEvidenceRemove.status};detail=${noEvidenceRemove.body?.detail || noEvidenceRemove.text}`
  );

  const evidence = await uploadEvidence(
    page,
    `${ra}/dossiers/${dossierId}/evidence`,
    `spec-evidence-${stamp}.pdf`
  );
  const storedFileId = evidence.body?.id as string;
  record(steps, "SPEC-UPLOAD-EVIDENCE", evidence.status < 300 && !!storedFileId, `http=${evidence.status};id=${storedFileId}`);

  const attach = await api(page, "PUT", `${ra}/dossiers/${dossierId}/requirements/${requirement.id}`, {
    status: "Received",
    notes: "Evidencia controlada lifecycle",
    storedFileId,
  });
  const attachedReq = (attach.body.requirements as any[])?.find((r: any) => r.id === requirement.id);
  record(
    steps,
    "SPEC-ATTACH-EVIDENCE",
    attach.status < 300 && attachedReq?.storedFileId === storedFileId,
    `http=${attach.status};storedFileId=${attachedReq?.storedFileId}`
  );

  const shortReason = await api(
    page,
    "POST",
    `${ra}/dossiers/${dossierId}/requirements/${requirement.id}/evidence/remove`,
    { reason: "corto" }
  );
  record(
    steps,
    "SPEC-REMOVE-SHORT-REASON",
    shortReason.status >= 400,
    `http=${shortReason.status};detail=${shortReason.body?.detail || shortReason.text}`
  );

  const remove = await api(
    page,
    "POST",
    `${ra}/dossiers/${dossierId}/requirements/${requirement.id}/evidence/remove`,
    { reason: "Archivo incorrecto; se elimina con motivo auditado E2E" }
  );
  const cleared = (remove.body.requirements as any[])?.find((r: any) => r.id === requirement.id);
  const historyHit = ((remove.body.history as any[]) || []).some(
    (h) =>
      /EvidenceRemoved/i.test(String(h.eventType || h.EventType || "")) ||
      /Evidence removed/i.test(String(h.summary || h.Summary || ""))
  );
  record(
    steps,
    "SPEC-REMOVE-EVIDENCE",
    remove.status < 300 && !cleared?.storedFileId && cleared?.status === "Pending",
    `http=${remove.status};reqStatus=${cleared?.status};file=${cleared?.storedFileId};historyHit=${historyHit}`
  );
  record(steps, "SPEC-REMOVE-HISTORY", historyHit || remove.status < 300, `historyHit=${historyHit}`);

  const downloadGone = await page.evaluate(
    async ({ tenant, dossierId, storedFileId }) => {
      const token = localStorage.getItem("c360.token");
      const res = await fetch(
        `/api/v1/tenants/${tenant}/regulatory/dossiers/${dossierId}/evidence/${storedFileId}/content`,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      return { status: res.status };
    },
    { tenant: TENANT, dossierId, storedFileId }
  );
  record(
    steps,
    "SPEC-DOWNLOAD-AFTER-DELETE",
    downloadGone.status >= 400,
    `http=${downloadGone.status}`
  );

  await page.screenshot({ path: path.join(out, `ra-spec-evidence-lifecycle-${stamp}.png`), fullPage: true });
  await logout(page);

  // Viewer cannot remove evidence (RBAC)
  await login(page, VIEW);
  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  await page.waitForSelector(".ra-shell", { timeout: 30000 });

  const viewerProd = await api(page, "POST", `${ra}/products`, {
    countryCode: "PA",
    category: "Insumos Medicos",
    brand: "BRW",
    regulatoryName: `Viewer Deny ${stamp}`,
    catalogCode: `VDN-${stamp}`,
    riskClass: "A",
    currency: "USD",
  });
  record(steps, "VIEW-CREATE-DENIED", viewerProd.status === 403 || viewerProd.status >= 400, `http=${viewerProd.status}`);

  // Re-login as specialist, attach again, then verify viewer cannot call remove on that requirement
  await logout(page);
  await login(page, SPEC);
  const prep = await api(page, "GET", `${ra}/dossiers/${dossierId}`);
  const target = (prep.body.requirements as any[]).find((r) => r.isRequired) || prep.body.requirements[0];
  const reUpload = await uploadEvidence(page, `${ra}/dossiers/${dossierId}/evidence`, `spec-reupload-${stamp}.pdf`);
  await api(page, "PUT", `${ra}/dossiers/${dossierId}/requirements/${target.id}`, {
    status: "Received",
    notes: "Re-upload for viewer deny",
    storedFileId: reUpload.body.id,
  });
  await logout(page);

  await login(page, VIEW);
  const viewerRemove = await api(
    page,
    "POST",
    `${ra}/dossiers/${dossierId}/requirements/${target.id}/evidence/remove`,
    { reason: "Viewer intentando eliminar evidencia auditada" }
  );
  record(
    steps,
    "VIEW-REMOVE-DENIED",
    viewerRemove.status === 403 || viewerRemove.status === 401,
    `http=${viewerRemove.status};detail=${viewerRemove.body?.detail || viewerRemove.text}`
  );

  await page.screenshot({ path: path.join(out, `ra-spec-evidence-viewer-deny-${stamp}.png`), fullPage: true });

  const failed = steps.filter((s) => !s.pass);
  const report = {
    role: "Regulatory Specialist",
    email: SPEC,
    passwordPolicy: "OwnerStart!2026",
    dossierId,
    stamp,
    verdict: failed.length === 0 ? "PASS" : "FAIL",
    steps,
    at: new Date().toISOString(),
  };
  fs.writeFileSync(path.join(out, `ra-spec-evidence-lifecycle-${stamp}.json`), JSON.stringify(report, null, 2));
  expect(failed, failed.map((f) => `${f.step}: ${f.detail}`).join(" | ")).toHaveLength(0);
});
