import { test, expect, Page } from "@playwright/test";
import * as path from "path";
import * as fs from "fs";

const TENANT = "82af3877-2786-4d39-bce8-c981101c771d";
const PASS = "OwnerStart!2026";
const users = {
  spec: "ra.spec@cert.local",
  rev: "ra.rev@cert.local",
  appr: "ra.appr@cert.local",
  sub: "ra.sub@cert.local",
  view: "ra.view@cert.local",
};

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
      form.append("file", new File(["%PDF-1.4\nCompliance360 E2E evidence"], filename, { type: "application/pdf" }));
      const res = await fetch(`/api/v1${apiPath}`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
        body: form,
      });
      const text = await res.text();
      return { status: res.status, body: text ? JSON.parse(text) : null };
    },
    { apiPath, filename }
  );
}

test("SoD browser multi-role happy path + negatives", async ({ page }) => {
  const steps: Step[] = [];
  const ra = `/tenants/${TENANT}/regulatory`;
  const out = path.join(__dirname, "..", "..", "docs", "regulatory-affairs", "security", "evidence");
  fs.mkdirSync(out, { recursive: true });

  // Specialist prepares
  await login(page, users.spec);
  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  await page.waitForSelector(".ra-shell", { timeout: 30000 });
  steps.push({
    step: "SPEC-UI",
    pass: (await page.locator(".ra-badge").first().textContent())?.includes("specialist") === true,
    detail: await page.locator(".ra-badge").first().textContent() || "",
  });

  const prod = await api(page, "POST", `${ra}/products`, {
    countryCode: "PA",
    category: "Insumos Medicos",
    brand: "BRW",
    regulatoryName: `Browser SOD ${Date.now()}`,
    catalogCode: `BRW-${Date.now().toString().slice(-6)}`,
    riskClass: "A",
    currency: "USD",
  });
  const auths = await api(page, "GET", `${ra}/authorities`);
  const authorityId = (auths.body as any[]).find((a) => a.code === "MINSA")?.id || (auths.body as any[])[0]?.id;
  const dos = await api(page, "POST", `${ra}/dossiers`, {
    productId: prod.body.id,
    authorityId,
    processType: "NewRegistration",
    comments: "browser sod",
    currency: "USD",
  });
  const dossierId = dos.body.id as string;
  steps.push({ step: "SPEC-CREATE", pass: dos.status < 300, detail: `status=${dos.body.status}` });

  const reqId = (dos.body.requirements as any[]).find((r) => r.isCritical)?.id;
  const self = await api(page, "PUT", `${ra}/dossiers/${dossierId}/requirements/${reqId}`, {
    status: "Accepted",
    notes: "self",
    storedFileId: crypto.randomUUID(),
  });
  steps.push({ step: "SPEC-SELF-REVIEW-DENIED", pass: self.status >= 400, detail: `http=${self.status}` });

  const noApprove = await api(page, "POST", `${ra}/dossiers/${dossierId}/approve-for-submission`, { notes: "x" });
  steps.push({ step: "SPEC-NO-INTERNAL", pass: noApprove.status >= 400, detail: `http=${noApprove.status}` });

  for (const st of ["WaitingManufacturerDocuments", "DocumentsReceived", "Assembling"]) {
    const tr = await api(page, "POST", `${ra}/dossiers/${dossierId}/transition`, {
      targetStatus: st,
      waiverReason: st === "DocumentsReceived" ? "Recepcion browser SoD" : null,
    });
    steps.push({
      step: `SPEC-TRANSITION-${st}`,
      pass: tr.status < 300 && tr.body?.status === st,
      detail: `http=${tr.status};status=${tr.body?.status}`,
    });
  }
  let prepared = await api(page, "GET", `${ra}/dossiers/${dossierId}`);
  for (const requirement of (prepared.body.requirements as any[]).filter((item) => item.isRequired)) {
    const evidence = await uploadEvidence(page, `${ra}/dossiers/${dossierId}/evidence`, `sod-prep-${requirement.code}.pdf`);
    const received = await api(page, "PUT", `${ra}/dossiers/${dossierId}/requirements/${requirement.id}`, {
      status: "Received",
      notes: "Evidencia controlada SoD",
      storedFileId: evidence.body.id,
    });
    prepared = received;
  }
  const startedReview = await api(page, "POST", `/api/v2/tenants/${TENANT}/regulatory/dossiers/${dossierId}/technical-review/start`, {
    expectedRevision: prepared.body.revision,
    reason: "Preparación completa en escenario SoD",
  });
  steps.push({ step: "SPEC-TRANSITION-UnderTechnicalReview", pass: startedReview.status < 300 && startedReview.body?.status === "UnderTechnicalReview", detail: `http=${startedReview.status};status=${startedReview.body?.status}` });

  await page.screenshot({ path: path.join(out, "browser-specialist.png"), fullPage: true });
  await logout(page);

  // Reviewer
  await login(page, users.rev);
  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  await page.waitForSelector(".ra-shell", { timeout: 30000 });
  const det = await api(page, "GET", `${ra}/dossiers/${dossierId}`);
  for (const r of (det.body.requirements as any[]).filter((x) => x.isRequired)) {
    await api(page, "PUT", `${ra}/dossiers/${dossierId}/requirements/${r.id}`, {
      status: "Accepted",
      notes: "browser review",
    });
  }
  const reviewed = await api(page, "GET", `${ra}/dossiers/${dossierId}`);
  const completeReview = await api(page, "POST", `/api/v2/tenants/${TENANT}/regulatory/dossiers/${dossierId}/technical-review/complete`, {
    expectedRevision: reviewed.body.revision,
    correctionRequestId: null,
    reason: "Revisión técnica completa en escenario SoD",
  });
  steps.push({ step: "REV-COMPLETE-REVIEW", pass: completeReview.status < 300 && completeReview.body.status === "ReadyForSubmission", detail: `http=${completeReview.status};status=${completeReview.body.status}` });
  const revApprove = await api(page, "POST", `${ra}/dossiers/${dossierId}/approve-for-submission`, { notes: "no" });
  steps.push({ step: "REV-NO-APPROVE", pass: revApprove.status >= 400, detail: `http=${revApprove.status}` });
  const revSubmit = await api(page, "POST", `${ra}/dossiers/${dossierId}/submit`, {});
  steps.push({ step: "REV-NO-SUBMIT", pass: revSubmit.status >= 400, detail: `http=${revSubmit.status}` });
  await page.screenshot({ path: path.join(out, "browser-reviewer.png"), fullPage: true });
  await logout(page);

  // Approver
  await login(page, users.appr);
  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  await page.waitForSelector(".ra-shell", { timeout: 30000 });
  const ai = await api(page, "POST", `${ra}/dossiers/${dossierId}/approve-for-submission`, {
    notes: "browser internal",
  });
  steps.push({
    step: "APPR-INTERNAL",
    pass: ai.status < 300 && ai.body.status === "ApprovedForSubmission",
    detail: `status=${ai.body.status}`,
  });
  const apprSub = await api(page, "POST", `${ra}/dossiers/${dossierId}/submit`, {});
  steps.push({ step: "APPR-NO-SUBMIT", pass: apprSub.status >= 400, detail: `http=${apprSub.status}` });
  await page.screenshot({ path: path.join(out, "browser-approver.png"), fullPage: true });
  await logout(page);

  // Submitter
  await login(page, users.sub);
  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  await page.waitForSelector(".ra-shell", { timeout: 30000 });
  const noInt = await api(page, "POST", `${ra}/dossiers/${dossierId}/approve-for-submission`, { notes: "x" });
  steps.push({ step: "SUB-NO-INTERNAL", pass: noInt.status >= 400, detail: `http=${noInt.status}` });
  const proof = await uploadEvidence(page, `${ra}/dossiers/${dossierId}/evidence`, `sod-submission-${Date.now()}.pdf`);
  const sub = await api(page, "POST", `${ra}/dossiers/${dossierId}/submit`, {
    procedureNumber: `SOD-TRAM-${Date.now()}`,
    externalNumber: `SOD-EXT-${Date.now()}`,
    submittedOn: new Date().toISOString(),
    proofStoredFileId: proof.body?.id,
  });
  steps.push({
    step: "SUB-SUBMIT",
    pass: sub.status < 300 && sub.body.status === "Submitted",
    detail: `status=${sub.body.status}`,
  });
  await page.screenshot({ path: path.join(out, "browser-submitter.png"), fullPage: true });
  await logout(page);

  // Viewer: read OK + all mutations blocked
  await login(page, users.view);
  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  await page.waitForSelector(".ra-shell", { timeout: 30000 });
  const viewRead = await api(page, "GET", `${ra}/dossiers/${dossierId}`);
  steps.push({
    step: "VIEW-READ",
    pass: viewRead.status < 300 && !!viewRead.body?.id,
    detail: `http=${viewRead.status};status=${viewRead.body?.status}`,
  });
  const viewCreate = await api(page, "POST", `${ra}/dossiers`, {
    productId: prod.body.id,
    authorityId,
    processType: "NewRegistration",
  });
  steps.push({ step: "VIEW-NO-CREATE", pass: viewCreate.status >= 400, detail: `http=${viewCreate.status}` });
  const viewEdit = await api(page, "POST", `${ra}/dossiers/${dossierId}/transition`, {
    targetStatus: "Assembling",
  });
  steps.push({ step: "VIEW-NO-EDIT", pass: viewEdit.status >= 400, detail: `http=${viewEdit.status}` });
  const crit = ((viewRead.body?.requirements as any[]) || []).find((r) => r.isCritical);
  const viewReview = crit
    ? await api(page, "PUT", `${ra}/dossiers/${dossierId}/requirements/${crit.id}`, {
        status: "Accepted",
        notes: "viewer should fail",
        storedFileId: crypto.randomUUID(),
      })
    : { status: 400 };
  steps.push({ step: "VIEW-NO-REVIEW", pass: viewReview.status >= 400, detail: `http=${viewReview.status}` });
  const viewApprove = await api(page, "POST", `${ra}/dossiers/${dossierId}/approve-for-submission`, {
    notes: "viewer should fail",
  });
  steps.push({ step: "VIEW-NO-APPROVE", pass: viewApprove.status >= 400, detail: `http=${viewApprove.status}` });
  const viewSubmit = await api(page, "POST", `${ra}/dossiers/${dossierId}/submit`, {});
  steps.push({ step: "VIEW-NO-SUBMIT", pass: viewSubmit.status >= 400, detail: `http=${viewSubmit.status}` });
  const viewDelete = await api(page, "POST", `${ra}/dossiers/${dossierId}/transition`, {
    targetStatus: "Cancelled",
  });
  steps.push({ step: "VIEW-NO-DELETE", pass: viewDelete.status >= 400, detail: `http=${viewDelete.status}` });
  await page.screenshot({ path: path.join(out, "browser-viewer.png"), fullPage: true });
  await logout(page);

  const failed = steps.filter((s) => !s.pass);
  fs.writeFileSync(
    path.join(out, "browser-sod-steps.json"),
    JSON.stringify({ when: new Date().toISOString(), dossierId, failed: failed.length, steps }, null, 2)
  );
  expect(failed, JSON.stringify(failed, null, 2)).toHaveLength(0);
});
