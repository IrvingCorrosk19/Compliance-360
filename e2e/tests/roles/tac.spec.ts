import { test, expect } from "@playwright/test";
import { TENANT, browserApi, evidenceDir, login, logout, saveFunctionalReport, user, type StepResult } from "../helpers";

test("TAC — administración completa y bloqueo regulatorio", async ({ page }) => {
  test.setTimeout(300_000);
  const account = user("Tenant Administrator");
  const out = evidenceDir("TAC");
  const steps: StepResult[] = [];
  const stamp = Date.now().toString().slice(-8);
  const initialEmail = `tac.recert.${stamp}@cert.local`;
  const editedEmail = `tac.recert.edited.${stamp}@cert.local`;
  const password = "RecertUser!2026";

  await login(page, TENANT, account.email, account.password);
  steps.push({ step: "LOGIN", expected: "Shell autenticado", actual: "Shell visible", pass: true });

  await page.evaluate(() => { location.hash = "#/tenant-administration"; });
  await expect(page.locator(".tenant-admin-shell")).toBeVisible({ timeout: 30_000 });

  await page.locator('.tenant-tab[data-tab="users"]').click();
  await expect(page.locator("#tenant-user-form")).toBeVisible();
  await page.fill("#newUserEmail", initialEmail);
  await page.fill("#newUserFullName", `Usuario Recert TAC ${stamp}`);
  await page.fill("#newUserInitialPassword", password);
  await page.fill("#newUserConfirmPassword", password);
  await page.locator('#tenant-user-form input[name="forcePasswordChange"]').uncheck();
  await page.locator("#tenant-user-form button[type=submit]").click();
  await expect(page.locator("tr", { hasText: initialEmail })).toBeVisible({ timeout: 30_000 });
  steps.push({ step: "CREATE-USER", expected: "Usuario persistido", actual: initialEmail, pass: true });

  let row = page.locator("tr", { hasText: initialEmail });
  await row.locator("[data-user-edit]").click();
  await expect(page.locator("#tenant-user-edit-dialog")).toBeVisible();
  await page.fill("#tenantEditUserEmail", editedEmail);
  await page.fill("#tenantEditUserName", `Usuario Recert Editado ${stamp}`);
  await page.locator("#tenant-user-edit-dialog button[type=submit]").click();
  row = page.locator("tr", { hasText: editedEmail });
  await expect(row).toContainText(`Usuario Recert Editado ${stamp}`, { timeout: 30_000 });
  steps.push({ step: "EDIT-USER", expected: "Correo y nombre actualizados", actual: editedEmail, pass: true });

  await page.locator('.tenant-tab[data-tab="rbac"]').click();
  await expect(page.locator("#tenant-role-assign-form")).toBeVisible();
  const userId = await page.locator("#assignUserId option", { hasText: editedEmail }).getAttribute("value");
  const viewerRoleId = await page.locator("#assignRoleId option", { hasText: "Regulatory Viewer" }).getAttribute("value");
  expect(userId).toBeTruthy();
  expect(viewerRoleId).toBeTruthy();
  await page.selectOption("#assignUserId", userId!);
  await page.selectOption("#assignRoleId", viewerRoleId!);
  await page.locator("#tenant-role-assign-form button[type=submit]").click();

  await page.locator('.tenant-tab[data-tab="users"]').click();
  row = page.locator("tr", { hasText: editedEmail });
  await expect(row).toContainText("Regulatory Viewer", { timeout: 30_000 });
  steps.push({ step: "ASSIGN-ROLE", expected: "Regulatory Viewer asignado", actual: "Rol visible en usuario", pass: true });

  await row.locator('[data-user-action="Disabled"]').click();
  row = page.locator("tr", { hasText: editedEmail });
  await expect(row).toContainText(/Inactive|Inactivo/, { timeout: 30_000 });
  steps.push({ step: "DISABLE-USER", expected: "Estado Disabled", actual: "Inactive/Inactivo (Disabled)", pass: true });

  await row.locator('[data-user-action="Active"]').click();
  row = page.locator("tr", { hasText: editedEmail });
  await expect(row).toContainText(/Active|Activo/, { timeout: 30_000 });
  steps.push({ step: "REACTIVATE-USER", expected: "Estado Active", actual: "Active/Activo", pass: true });

  await row.locator(`[data-user-role-revoke][data-role-id="${viewerRoleId}"]`).click();
  row = page.locator("tr", { hasText: editedEmail });
  await expect(row).not.toContainText("Regulatory Viewer", { timeout: 30_000 });
  steps.push({ step: "REVOKE-ROLE", expected: "Rol retirado", actual: "Rol ausente", pass: true });

  await page.locator('.tenant-tab[data-tab="general"]').click();
  await expect(page.locator("#tenant-general-form")).toBeVisible();
  const city = `Panamá Recert ${stamp}`;
  await page.fill("#tenantCity", city);
  await page.fill("#generalChangeReason", "Recertificación funcional TAC");
  await page.locator("#tenant-general-form button[type=submit]").click();
  await expect(page.locator("#tenantCity")).toHaveValue(city, { timeout: 30_000 });
  steps.push({ step: "TENANT-PROFILE", expected: "Perfil persistido", actual: city, pass: true });

  await page.locator('.tenant-tab[data-tab="branding"]').click();
  await expect(page.locator("#tenant-branding-form")).toBeVisible();
  const footer = `Compliance 360 · RECERT ${stamp}`;
  await page.fill('#tenant-branding-form input[name="footerText"]', footer);
  await page.fill("#brandingChangeReason", "Recertificación funcional TAC");
  await page.locator("#tenant-branding-form button[type=submit]").click();
  await expect(page.locator('#tenant-branding-form input[name="footerText"]')).toHaveValue(footer, { timeout: 30_000 });
  steps.push({ step: "TENANT-BRANDING", expected: "Branding persistido", actual: footer, pass: true });

  const ra = `/tenants/${TENANT}/regulatory`;
  const forbidden = [
    ["CREATE-DOSSIER", "POST", `${ra}/dossiers`, {}],
    ["APPROVE-INTERNAL", "POST", `${ra}/dossiers/00000000-0000-0000-0000-000000000001/approve-for-submission`, { notes: "forbidden" }],
    ["SUBMIT", "POST", `${ra}/dossiers/00000000-0000-0000-0000-000000000001/submit`, {}],
    ["DECISION-EXTERNAL", "POST", `${ra}/dossiers/00000000-0000-0000-0000-000000000001/approve`, {}],
  ] as const;
  for (const [id, method, url, body] of forbidden) {
    const result = await browserApi(page, method, url, body);
    steps.push({
      step: `DENY-${id}`,
      expected: "HTTP 401/403 sin mutación",
      actual: `HTTP ${result.status}`,
      pass: result.status === 401 || result.status === 403,
    });
  }

  const audit = await browserApi(page, "POST", `/tenants/${TENANT}/audit/search`, { page: 1, pageSize: 100 });
  const auditItems = (audit.body as any)?.items || [];
  const audited = auditItems.some((item: any) =>
    String(item.entityName || "").toLowerCase().includes("user") ||
    String(item.entityName || "").toLowerCase().includes("tenant")
  );
  steps.push({
    step: "AUDIT",
    expected: "Cambios TAC presentes en auditoría append-only",
    actual: `eventos=${auditItems.length}; tenant/user=${audited}`,
    pass: audit.ok && audited,
  });

  await page.screenshot({ path: `${out}\\tac-final.png`, fullPage: true });
  await saveFunctionalReport("TAC", account.email, steps, page, { testUser: editedEmail, tenantId: TENANT });
  expect(steps.filter(step => !step.pass)).toEqual([]);
  await logout(page);
});
