import { test, expect, Page } from "@playwright/test";
import { TENANT, browserApi, evidenceDir, go, jwtPermissions, login, logout, saveFunctionalReport, user, type StepResult } from "../helpers";

async function uploadEvidence(page: Page, apiPath: string, filename: string) {
  return page.evaluate(async ({ apiPath, filename }) => {
    const token = localStorage.getItem("c360.token");
    const form = new FormData();
    form.append("file", new File(["%PDF-1.4\nCompliance360 certification evidence"], filename, { type: "application/pdf" }));
    const response = await fetch(`/api/v1${apiPath}`, {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: form,
    });
    const text = await response.text();
    return { status: response.status, body: text ? JSON.parse(text) : null };
  }, { apiPath, filename });
}

test("QM — aprobación QMS y decisión externa CT/RS", async ({ page }) => {
  test.setTimeout(600_000);
  const steps: StepResult[] = [];
  const out = evidenceDir("QM");
  const ra = `/tenants/${TENANT}/regulatory`;
  const stamp = Date.now().toString().slice(-8);

  // Preparar un documento real con un usuario independiente Document Controller.
  const tac = user("Tenant Administrator");
  await login(page, TENANT, tac.email, tac.password);
  const center = await browserApi(page, "GET", `/tenants/${TENANT}/administration-center`);
  const documentRoleId = (center.body as any)?.users?.roles?.find((role: any) => role.name === "Document Controller")?.id;
  expect(documentRoleId).toBeTruthy();
  const controllerEmail = `doc.recert.${stamp}@cert.local`;
  const controllerPassword = "DocumentStart!2026";
  const createdController = await browserApi(page, "POST", `/tenants/${TENANT}/users`, {
    email: controllerEmail,
    fullName: `Document Controller Recert ${stamp}`,
    initialPassword: controllerPassword,
    roleId: documentRoleId,
    forcePasswordChange: false,
    changeReason: "Preparación certificación QM",
  });
  expect(createdController.ok).toBeTruthy();
  await logout(page);

  await login(page, TENANT, controllerEmail, controllerPassword);
  const type = await browserApi(page, "POST", `/tenants/${TENANT}/documents/types`, {
    name: `Tipo Recert ${stamp}`,
    code: `T${stamp}`,
    retentionDays: 365,
  });
  const category = await browserApi(page, "POST", `/tenants/${TENANT}/documents/categories`, {
    name: `Categoría Recert ${stamp}`,
    code: `C${stamp}`,
  });
  expect(type.ok && category.ok, `type=${type.status}:${type.text}; category=${category.status}:${category.text}`).toBeTruthy();
  const document = await browserApi(page, "POST", `/tenants/${TENANT}/documents`, {
    documentTypeId: (type.body as any).id,
    categoryId: (category.body as any).id,
    title: `Procedimiento QMS ${stamp}`,
    code: `DOC-QM-${stamp}`,
  });
  expect(document.ok).toBeTruthy();
  const documentId = (document.body as any).id as string;
  const version = await browserApi(page, "POST", `/tenants/${TENANT}/documents/${documentId}/versions`, {
    storedFileId: crypto.randomUUID(),
    changeSummary: "Versión sometida a aprobación QM",
  });
  expect(version.ok).toBeTruthy();
  const submittedDocument = await browserApi(page, "POST", `/tenants/${TENANT}/documents/${documentId}/submit`);
  expect(submittedDocument.ok).toBeTruthy();
  const selfApprove = await browserApi(page, "POST", `/tenants/${TENANT}/documents/${documentId}/decision`, {
    decision: 0,
    comments: "Intento incompatible Document Controller",
  });
  steps.push({
    step: "QMS-SOD-UPLOADER-NO-APPROVE",
    expected: "Document Controller recibe 403",
    actual: `HTTP ${selfApprove.status}`,
    pass: selfApprove.status === 403,
  });
  await logout(page);

  // Preparar un expediente real hasta Submitted usando identidades separadas.
  const specialist = user("Regulatory Specialist");
  await login(page, TENANT, specialist.email, specialist.password);
  const product = await browserApi(page, "POST", `${ra}/products`, {
    countryCode: "PA",
    category: "Dispositivo médico",
    brand: `QM Brand ${stamp}`,
    regulatoryName: `Producto decisión QM ${stamp}`,
    catalogCode: `QM-${stamp}`,
    riskClass: "A",
    currency: "USD",
  });
  const authorities = await browserApi(page, "GET", `${ra}/authorities`);
  const authorityId = (authorities.body as any[]).find(authority => authority.code === "MINSA")?.id;
  const dossier = await browserApi(page, "POST", `${ra}/dossiers`, {
    productId: (product.body as any).id,
    authorityId,
    processType: "NewRegistration",
    comments: "Expediente trazable para certificación QM",
    currency: "USD",
  });
  const dossierId = (dossier.body as any).id as string;
  for (const targetStatus of ["WaitingManufacturerDocuments", "DocumentsReceived", "Assembling"]) {
    const transition = await browserApi(page, "POST", `${ra}/dossiers/${dossierId}/transition`, {
      targetStatus,
      waiverReason: targetStatus === "DocumentsReceived" ? "Recepción controlada para certificación QM" : null,
    });
    expect(transition.ok).toBeTruthy();
  }
  let preparation = await browserApi(page, "GET", `${ra}/dossiers/${dossierId}`);
  for (const requirement of (preparation.body as any).requirements.filter((item: any) => item.isRequired)) {
    const evidence = await uploadEvidence(page, `${ra}/dossiers/${dossierId}/evidence`, `qm-prep-${requirement.code}-${stamp}.pdf`);
    expect(evidence.status).toBeLessThan(300);
    const received = await browserApi(page, "PUT", `${ra}/dossiers/${dossierId}/requirements/${requirement.id}`, {
      status: "Received",
      notes: "Evidencia de preparación controlada",
      storedFileId: evidence.body.id,
    });
    expect(received.ok).toBeTruthy();
    preparation = received;
  }
  const startedReview = await browserApi(page, "POST", `/api/v2/tenants/${TENANT}/regulatory/dossiers/${dossierId}/technical-review/start`, {
    expectedRevision: (preparation.body as any).revision,
    reason: "Preparación completa para certificación QM",
  });
  expect(startedReview.ok, startedReview.text).toBeTruthy();
  await logout(page);

  const reviewer = user("Regulatory Reviewer");
  await login(page, TENANT, reviewer.email, reviewer.password);
  const detail = await browserApi(page, "GET", `${ra}/dossiers/${dossierId}`);
  for (const requirement of (detail.body as any).requirements.filter((item: any) => item.isRequired)) {
    const accepted = await browserApi(page, "PUT", `${ra}/dossiers/${dossierId}/requirements/${requirement.id}`, {
      status: "Accepted",
      notes: "Revisión técnica independiente para certificación QM",
    });
    expect(accepted.ok).toBeTruthy();
  }
  const reviewedDetail = await browserApi(page, "GET", `${ra}/dossiers/${dossierId}`);
  const completedReview = await browserApi(page, "POST", `/api/v2/tenants/${TENANT}/regulatory/dossiers/${dossierId}/technical-review/complete`, {
    expectedRevision: (reviewedDetail.body as any).revision,
    correctionRequestId: null,
    reason: "Revisión técnica independiente completa para certificación QM",
  });
  expect(completedReview.ok, completedReview.text).toBeTruthy();
  await logout(page);

  const approver = user("Regulatory Approver");
  await login(page, TENANT, approver.email, approver.password);
  const approvedInternal = await browserApi(page, "POST", `${ra}/dossiers/${dossierId}/approve-for-submission`, {
    notes: "Aprobación interna independiente",
  });
  expect(approvedInternal.ok).toBeTruthy();
  await logout(page);

  const submitter = user("Regulatory Submitter");
  await login(page, TENANT, submitter.email, submitter.password);
  const proof = await uploadEvidence(page, `${ra}/dossiers/${dossierId}/evidence`, `qm-submission-${stamp}.pdf`);
  expect(proof.status).toBeLessThan(300);
  const submitted = await browserApi(page, "POST", `${ra}/dossiers/${dossierId}/submit`, {
    procedureNumber: `QM-TRAM-${stamp}`,
    externalNumber: `QM-EXT-${stamp}`,
    submittedOn: new Date().toISOString(),
    proofStoredFileId: proof.body.id,
  });
  expect(submitted.ok).toBeTruthy();
  await logout(page);

  // Certificación funcional individual de Quality Manager.
  const qm = user("Quality Manager");
  await login(page, TENANT, qm.email, qm.password);
  steps.push({ step: "LOGIN", expected: "Shell autenticado", actual: "Shell visible", pass: true });
  const permissions = await jwtPermissions(page);
  for (const permission of [
    "DOCUMENT.APPROVE", "CAPA.APPROVE", "RISK.APPROVE", "TECHNICALSHEET.APPROVE",
    "REGULATORY.DOSSIER.APPROVE", "REGULATORY.REGISTRATION.MANAGE",
  ]) {
    steps.push({
      step: `PERMISSION-${permission}`,
      expected: "Permiso presente",
      actual: permissions.includes(permission) ? "presente" : "ausente",
      pass: permissions.includes(permission),
    });
  }

  await go(page, "documents");
  await expect(page.locator("main")).toContainText("Document Management");
  const documentRow = page.locator("tr", { hasText: `DOC-QM-${stamp}` });
  await expect(documentRow.locator(`[data-document-approve="${documentId}"]`)).toBeVisible();
  await documentRow.locator(`[data-document-approve="${documentId}"]`).click();
  await expect(page.locator("tr", { hasText: `DOC-QM-${stamp}` })).toContainText(/Approved|Aprobado/, { timeout: 30_000 });
  const documentList = await browserApi(page, "GET", `/tenants/${TENANT}/documents?searchText=DOC-QM-${stamp}&page=1&pageSize=10`);
  const persistedDocument = (documentList.body as any)?.items?.find((item: any) => item.id === documentId);
  steps.push({
    step: "QMS-DOCUMENT-APPROVE",
    expected: "Documento aprobado y persistido por identidad QM",
    actual: `status=${persistedDocument?.status ?? "n/a"}`,
    pass: String(persistedDocument?.status).toLowerCase().includes("approved"),
  });

  for (const route of ["technical-sheets", "capa", "risks"]) {
    await go(page, route);
    const denied = await page.locator("main").getByText(/Acceso denegado|Access denied/i).count();
    steps.push({
      step: `QMS-READ-${route}`,
      expected: "Pantalla QMS consultable",
      actual: denied === 0 ? "consultable" : "denegada",
      pass: denied === 0,
    });
  }

  await go(page, "regulatory");
  await page.locator('#ra-nav [data-view="dossiers"]').click();
  await page.evaluate((id) => { (window as any).__raOpenDossier = id; }, dossierId);
  await page.locator('#ra-nav [data-view="dossiers"]').click();
  await expect(page.locator("#ra-approve-ext")).toBeVisible({ timeout: 30_000 });
  await page.locator("#ra-approve-ext").click();
  const modal = page.locator("#ra-approve-ext-modal .ra-modal");
  await expect(modal).toBeVisible();
  const registrationNumber = `QM-CTRS-${stamp}`;
  await page.fill('#ra-approve-ext-modal input[name="registrationNumber"]', registrationNumber);
  await page.locator('#ra-approve-ext-modal input[name="resolution"]').setInputFiles({
    name: `qm-resolution-${stamp}.pdf`,
    mimeType: "application/pdf",
    buffer: Buffer.from(`%PDF-1.4\nQuality Manager resolution ${stamp}\n`),
  });
  await page.fill('#ra-approve-ext-modal textarea[name="notes"]', "Decisión externa MINSA registrada por QM");
  await page.locator("#ra-approve-ext-modal button[type=submit]").click();
  await expect(modal).toBeHidden({ timeout: 30_000 });
  const finalDossier = await browserApi(page, "GET", `${ra}/dossiers/${dossierId}`);
  const registrations = await browserApi(page, "GET", `${ra}/registrations?searchText=${registrationNumber}`);
  const registration = (registrations.body as any[]).find(item => item.registrationNumber === registrationNumber);
  steps.push({
    step: "EXTERNAL-DECISION-CTRS",
    expected: "Decisión externa, resolución, vigencia y CT/RS persistidos",
    actual: `status=${(finalDossier.body as any).status}; registration=${registration?.registrationNumber ?? "NO"}`,
    pass: ["Approved", "Closed"].includes((finalDossier.body as any).status) && !!registration,
  });

  const forbiddenPrepare = await browserApi(page, "POST", `${ra}/dossiers`, {
    productId: (product.body as any).id,
    authorityId,
    processType: "NewRegistration",
  });
  const forbiddenSubmit = await browserApi(page, "POST", `${ra}/dossiers/${dossierId}/submit`, {});
  steps.push({
    step: "DENY-PREPARE",
    expected: "HTTP 401/403",
    actual: `HTTP ${forbiddenPrepare.status}`,
    pass: forbiddenPrepare.status === 401 || forbiddenPrepare.status === 403,
  });
  steps.push({
    step: "DENY-SUBMIT",
    expected: "HTTP 401/403",
    actual: `HTTP ${forbiddenSubmit.status}`,
    pass: forbiddenSubmit.status === 401 || forbiddenSubmit.status === 403,
  });

  const audit = await browserApi(page, "POST", `/tenants/${TENANT}/audit/search`, { page: 1, pageSize: 100 });
  const auditItems = (audit.body as any)?.items || [];
  const qmAudit = auditItems.some((item: any) =>
    String(item.entityId || "").toLowerCase() === dossierId.toLowerCase() ||
    String(item.entityId || "").toLowerCase() === documentId.toLowerCase()
  );
  steps.push({
    step: "AUDIT",
    expected: "Aprobaciones QMS/externa registradas",
    actual: `eventos=${auditItems.length}; evidenciaEntidades=${qmAudit}`,
    pass: audit.ok && qmAudit,
  });

  await page.screenshot({ path: `${out}\\qm-final.png`, fullPage: true });
  await saveFunctionalReport("QM", qm.email, steps, page, { tenantId: TENANT, dossierId, documentId, registrationNumber });
  expect(steps.filter(step => !step.pass)).toEqual([]);
  await logout(page);
});
