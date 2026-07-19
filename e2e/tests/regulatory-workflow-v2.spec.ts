import { expect, Page, test } from "@playwright/test";
import { createHash, randomUUID } from "crypto";
import * as fs from "fs";
import * as path from "path";

const TENANT = "82af3877-2786-4d39-bce8-c981101c771d";
const PASSWORD = "OwnerStart!2026";
const EVIDENCE_DIR = path.resolve(__dirname, "../../docs/regulatory-workflow-v2/evidence");
const RA = `/api/v1/tenants/${TENANT}/regulatory`;
const V2 = `/api/v2/tenants/${TENANT}/regulatory/dossiers`;

const USERS = {
  specialist: "ra.spec@cert.local",
  reviewer: "ra.rev@cert.local",
  viewer: "ra.view@cert.local",
  approver: "ra.appr@cert.local",
  manager: "ra.mgr@cert.local",
  quality: "ra.qm@cert.local",
  tenantAdmin: "irvingcorrosk19@gmail.com",
  regulatoryAdmin: "ra.admin@cert.local",
} as const;

type ApiResult = { status: number; body: any; text: string };
type PhaseResult = {
  phase: string;
  passed: boolean;
  detail: string;
  at: string;
};

type Scenario = {
  stamp: string;
  productId?: string;
  dossierId?: string;
  caseNumber?: string;
  scopedRequirementId?: string;
  outOfScopeRequirementId?: string;
  correctionId?: string;
  revision?: number;
  staleConflictStatus?: number;
  terminalDossierId?: string;
  archiveDossierId?: string;
  renewalDossierId?: string;
};

test.describe.configure({ mode: "serial" });

test.describe("Regulatory Workflow V2 — real multi-role certification", () => {
  test("executes Workflow V2, RBAC, SoD, concurrency and immutable evidence", async ({ page }) => {
    test.setTimeout(10 * 60_000);
    fs.mkdirSync(EVIDENCE_DIR, { recursive: true });

    const scenario: Scenario = { stamp: Date.now().toString() };
    const phases: PhaseResult[] = [];

    const phase = async (name: string, run: () => Promise<void>) => {
      try {
        await run();
        phases.push({ phase: name, passed: true, detail: "PASS", at: new Date().toISOString() });
      } catch (error) {
        const detail = error instanceof Error ? error.stack || error.message : String(error);
        phases.push({ phase: name, passed: false, detail, at: new Date().toISOString() });
      } finally {
        writeJson("workflow-v2-execution.json", { scenario, phases });
      }
    };

    await phase("RA-SPEC UI creates product and Draft dossier", async () => {
      await login(page, USERS.specialist);
      await openRegulatory(page);

      await page.locator('#ra-nav [data-view="portfolio"]').click();
      const create = page.locator("#ra-new-product");
      await expect(create, "RA-SPEC must have the real UI create action").toBeVisible();
      await create.click();

      const modal = page.locator("#ra-new-product-modal .ra-modal");
      await expect(modal, "Product+dossier modal must exist; no API setup fallback").toBeVisible();
      const catalogCode = `WFV2-${scenario.stamp.slice(-8)}`;
      await page.fill("#ra-np-brand", "WFV2-E2E");
      await page.fill("#ra-np-name", `Workflow V2 ${scenario.stamp}`);
      await page.fill("#ra-np-code", catalogCode);
      await page.selectOption("#ra-np-risk", "B");
      await page.fill("#ra-np-comments", "Workflow V2 serial Playwright certification");
      await modal.locator('button[type="submit"]').click();
      await expect(modal).toBeHidden({ timeout: 30_000 });
      await expect(page.locator(".ra-table")).toContainText(catalogCode);

      const products = await mustApi(page, "GET", `${RA}/products`, undefined, 200);
      const product = asArray(products.body).find((x) => x.catalogCode === catalogCode);
      expect(product, `UI-created product ${catalogCode} was not persisted`).toBeTruthy();
      scenario.productId = product.id;

      const dossiers = await mustApi(page, "GET", `${RA}/dossiers`, undefined, 200);
      const dossier = asArray(dossiers.body).find((x) => x.productId === scenario.productId);
      expect(dossier, "UI-created dossier was not persisted").toBeTruthy();
      scenario.dossierId = dossier.id;

      const detail = await dossierDetail(page, scenario);
      scenario.caseNumber = detail.caseNumber;
      scenario.revision = detail.revision;
      expect(detail.status, "V2 requires creation to persist as Draft").toBe("Draft");
      expect(asArray(detail.requirements).length, "Draft must have a requirement pack").toBeGreaterThan(1);

      await screenshot(page, "01-ra-spec-created-draft.png");
    });

    await phase("RA-SPEC persists and edits V2 metadata with revision", async () => {
      await ensureRole(page, USERS.specialist);
      const dossierId = requireValue(scenario.dossierId, "dossierId from UI creation");
      const initial = await workflow(page, dossierId);
      const firstRevision = initial.revision as number;

      const first = await mustApi(page, "PUT", `${V2}/${dossierId}/metadata`, {
        expectedRevision: firstRevision,
        reason: "Initial Workflow V2 metadata save",
        priority: "High",
        salesMarketingInput: "Market access evidence required",
        opportunityAmount: 125000,
        currency: "USD",
        comments: "V2 metadata revision one",
        estimatedSubmissionOn: new Date(Date.now() + 30 * 86400_000).toISOString(),
      }, 200);
      expect(first.body.revision).toBe(firstRevision + 1);
      expect(first.body.comments).toBe("V2 metadata revision one");

      const second = await mustApi(page, "PUT", `${V2}/${dossierId}/metadata`, {
        expectedRevision: first.body.revision,
        reason: "Second Workflow V2 metadata save",
        priority: "Critical",
        salesMarketingInput: "Updated commercial and regulatory assessment",
        opportunityAmount: 150000,
        currency: "USD",
        comments: "V2 metadata revision two",
        estimatedSubmissionOn: new Date(Date.now() + 45 * 86400_000).toISOString(),
      }, 200);
      scenario.revision = second.body.revision;

      const persisted = await dossierDetail(page, scenario);
      expect(persisted.priority).toBe("Critical");
      expect(persisted.comments).toBe("V2 metadata revision two");
      expect(persisted.revision).toBe(second.body.revision);
    });

    await phase("stale expectedRevision returns exact HTTP 409", async () => {
      await ensureRole(page, USERS.specialist);
      const dossierId = requireValue(scenario.dossierId, "dossierId for stale revision check");
      const current = await workflow(page, dossierId);
      const stale = Math.max(0, Number(current.revision) - 1);
      const result = await api(page, "PUT", `${V2}/${dossierId}/metadata`, {
        expectedRevision: stale,
        reason: "Deliberately stale concurrency probe",
        comments: "THIS MUST NOT PERSIST",
      });
      scenario.staleConflictStatus = result.status;
      writeJson("stale-revision-response.json", result);
      expect(result.status, `Revision conflict returned ${result.status}: ${result.text}`).toBe(409);

      const persisted = await dossierDetail(page, scenario);
      expect(persisted.comments).toBe("V2 metadata revision two");
      expect(persisted.revision).toBe(current.revision);
    });

    await phase("RA-SPEC advances Draft to UnderTechnicalReview", async () => {
      await ensureRole(page, USERS.specialist);
      const dossierId = requireValue(scenario.dossierId, "dossierId for preparation");
      let detail = await dossierDetail(page, scenario);
      const route = [
        "Planning",
        "WaitingManufacturerDocuments",
        "DocumentsReceived",
        "Assembling",
      ];
      const currentIndex = route.indexOf(detail.status);
      const start = detail.status === "Draft" ? 0 : currentIndex + 1;
      expect(start, `Unsupported preparation starting state ${detail.status}`).toBeGreaterThanOrEqual(0);

      for (const targetStatus of route.slice(start)) {
        const transition = await mustApi(page, "POST", `${RA}/dossiers/${dossierId}/transition`, {
          targetStatus,
          waiverReason: targetStatus === "DocumentsReceived"
            ? "Controlled Workflow V2 E2E reception"
            : null,
        }, 200);
        expect(transition.body.status).toBe(targetStatus);
        detail = transition.body;
      }
      for (const requirement of asArray(detail.requirements).filter(item => item.isRequired)) {
        const bytes = Buffer.from(`%PDF-1.7\nPreparation evidence ${requirement.code} ${scenario.stamp}\n`, "utf8");
        const storedFileId = await uploadStoredFile(page, dossierId, `preparation-${requirement.code}.pdf`, bytes);
        detail = (await mustApi(page, "PUT", `${RA}/dossiers/${dossierId}/requirements/${requirement.id}`, {
          status: "Received",
          documentId: null,
          storedFileId,
          notes: "Required preparation evidence uploaded",
        }, 200)).body;
      }
      const startReview = await mustApi(page, "POST", `${V2}/${dossierId}/technical-review/start`, {
        expectedRevision: detail.revision,
        reason: "Required evidence completed by Regulatory Specialist",
      }, 200);
      detail = startReview.body;
      expect(detail.status).toBe("UnderTechnicalReview");
      scenario.revision = detail.revision;

      await openDossierUi(page, dossierId);
      await expect(page.locator(".ra-card")).toContainText(scenario.caseNumber || "");
      await screenshot(page, "02-ra-spec-under-technical-review.png");
    });

    await phase("RA-REV requests scoped correction", async () => {
      await login(page, USERS.reviewer);
      await openRegulatory(page);
      const dossierId = requireValue(scenario.dossierId, "dossierId for correction");
      const detail = await dossierDetail(page, scenario);
      expect(detail.status).toBe("UnderTechnicalReview");

      const requirements = asArray(detail.requirements);
      expect(requirements.length, "Correction test requires at least two requirements").toBeGreaterThan(1);
      scenario.scopedRequirementId = requirements[0].id;
      scenario.outOfScopeRequirementId = requirements[1].id;

      const correction = await mustApi(page, "POST", `${V2}/${dossierId}/corrections`, {
        expectedRevision: detail.revision,
        reason: "Technical evidence needs controlled replacement",
        severity: "High",
        requirementIds: [scenario.scopedRequirementId],
        fieldPaths: ["metadata.comments"],
        documentIds: [],
      }, 200);
      scenario.correctionId = correction.body.id;

      const snapshot = await workflow(page, dossierId);
      expect(snapshot.status).toBe("CorrectionRequested");
      expect(snapshot.openCorrection.id).toBe(scenario.correctionId);
      expect(snapshot.openCorrection.severity).toBe("High");
      expect(snapshot.openCorrection.requirementIds).toEqual([scenario.scopedRequirementId]);
      scenario.revision = snapshot.revision;

      await screenshot(page, "03-ra-reviewer-correction-requested.png");
    });

    await phase("RA-SPEC rejects out-of-scope evidence without persistence", async () => {
      await login(page, USERS.specialist);
      const dossierId = requireValue(scenario.dossierId, "dossierId for scope check");
      const requirementId = requireValue(scenario.outOfScopeRequirementId, "out-of-scope requirement");
      const snapshot = await workflow(page, dossierId);
      const before = await mustApi(page, "GET", `${V2}/${dossierId}/requirements/${requirementId}/evidence`, undefined, 200);

      const bytes = Buffer.from("%PDF-1.7\nWorkflow V2 forbidden out-of-scope evidence", "utf8");
      const storedFileId = await uploadStoredFile(page, dossierId, "outside-scope.pdf", bytes);
      const denied = await api(page, "POST", `${V2}/${dossierId}/evidence`, {
        expectedRevision: snapshot.revision,
        requirementId,
        correctionRequestId: scenario.correctionId,
        documentId: null,
        storedFileId,
        sha256: sha256(bytes),
        fileName: "outside-scope.txt",
        reason: "This controlled-scope mutation must be rejected",
      });
      expect(denied.status, denied.text).toBe(400);
      expect(denied.text).toMatch(/outside.*scope/i);

      const after = await mustApi(page, "GET", `${V2}/${dossierId}/requirements/${requirementId}/evidence`, undefined, 200);
      expect(after.body, "Rejected evidence must leave no V2 evidence revision").toEqual(before.body);
      expect((await workflow(page, dossierId)).revision, "Rejected evidence must not increment revision").toBe(snapshot.revision);
    });

    await phase("RA-SPEC uploads real SHA-256 V1 and V2 evidence", async () => {
      await ensureRole(page, USERS.specialist);
      const dossierId = requireValue(scenario.dossierId, "dossierId for evidence versions");
      const requirementId = requireValue(scenario.scopedRequirementId, "scoped requirement");
      const correctionId = requireValue(scenario.correctionId, "active correction");

      const v1Bytes = Buffer.from(`%PDF-1.7\nWorkflow V2 evidence V1 ${scenario.stamp}\n`, "utf8");
      const v1Stored = await uploadStoredFile(page, dossierId, "workflow-v2-evidence-v1.pdf", v1Bytes);
      let snapshot = await workflow(page, dossierId);
      const v1 = await mustApi(page, "POST", `${V2}/${dossierId}/evidence`, {
        expectedRevision: snapshot.revision,
        requirementId,
        correctionRequestId: correctionId,
        documentId: null,
        storedFileId: v1Stored,
        sha256: sha256(v1Bytes),
        fileName: "workflow-v2-evidence-v1.txt",
        reason: "Initial scoped correction evidence",
      }, 200);
      expect(v1.body.versionNumber).toBe(1);
      expect(v1.body.sha256).toBe(sha256(v1Bytes));
      expect(v1.body.isCurrent).toBe(true);

      const v2Bytes = Buffer.from(`%PDF-1.7\nWorkflow V2 evidence V2 corrected ${scenario.stamp}\n`, "utf8");
      const v2Stored = await uploadStoredFile(page, dossierId, "workflow-v2-evidence-v2.pdf", v2Bytes);
      snapshot = await workflow(page, dossierId);
      const v2 = await mustApi(page, "POST", `${V2}/${dossierId}/evidence`, {
        expectedRevision: snapshot.revision,
        requirementId,
        correctionRequestId: correctionId,
        documentId: null,
        storedFileId: v2Stored,
        sha256: sha256(v2Bytes),
        fileName: "workflow-v2-evidence-v2.txt",
        reason: "Replacement after reviewer correction request",
      }, 200);
      expect(v2.body.versionNumber).toBe(2);
      expect(v2.body.sha256).toBe(sha256(v2Bytes));

      const versions = await mustApi(page, "GET", `${V2}/${dossierId}/requirements/${requirementId}/evidence`, undefined, 200);
      expect(asArray(versions.body)).toHaveLength(2);
      const history = asArray(versions.body).sort((a, b) => a.versionNumber - b.versionNumber);
      expect(history[0]).toMatchObject({ versionNumber: 1, isCurrent: false, status: "Superseded" });
      expect(history[1]).toMatchObject({ versionNumber: 2, isCurrent: true, status: "Active" });
      expect(history[0].sha256).toBe(sha256(v1Bytes));
      expect(history[1].sha256).toBe(sha256(v2Bytes));
      scenario.revision = v2.body.revision;
      writeJson("evidence-version-history.json", history);
    });

    await phase("RA-SPEC submits correction and RA-REV completes review", async () => {
      await ensureRole(page, USERS.specialist);
      const dossierId = requireValue(scenario.dossierId, "dossierId for correction submit");
      const correctionId = requireValue(scenario.correctionId, "correctionId for correction submit");
      const requirementId = requireValue(scenario.scopedRequirementId, "scoped requirement for correction submit");
      let snapshot = await workflow(page, dossierId);

      const submitted = await mustApi(page, "POST", `${V2}/${dossierId}/corrections/submit`, {
        expectedRevision: snapshot.revision,
        correctionRequestId: correctionId,
        reason: "Corrected evidence V2 submitted for technical review",
        requirementIds: [requirementId],
        fieldPaths: ["metadata.comments"],
        documentIds: [],
      }, 200);
      expect(submitted.body.status).toBe("UnderTechnicalReview");

      await login(page, USERS.reviewer);
      const reviewDetail = await dossierDetail(page, scenario);
      for (const requirement of asArray(reviewDetail.requirements)
        .filter((item) => item.isRequired && !["Accepted", "Waived", "NotRequired"].includes(item.status))) {
        const reviewed = await mustApi(page, "PUT", `${RA}/dossiers/${dossierId}/requirements/${requirement.id}`, {
          status: "Accepted",
          documentId: null,
          storedFileId: requirement.storedFileId || randomUUID(),
          notes: "Workflow V2 technical review accepted",
        }, 200);
        expect(reviewed.body.status).toBe("UnderTechnicalReview");
      }
      snapshot = await workflow(page, dossierId);
      expect(snapshot.availableActions).toContain("ready-for-submission");

      const completed = await api(page, "POST", `${V2}/${dossierId}/technical-review/complete`, {
        expectedRevision: snapshot.revision,
        correctionRequestId: correctionId,
        reason: "Reviewer completed corrected technical review",
      });
      expect(completed.status, `Reviewer cannot complete technical review: ${completed.text}`).toBe(200);
      expect(completed.body.status).toBe("ReadyForSubmission");
      scenario.revision = completed.body.revision;
      await openDossierUi(page, dossierId);
      await screenshot(page, "04-ra-reviewer-completed-review.png");
    });

    await phase("Viewer is 403 on every V2 mutation family", async () => {
      await login(page, USERS.viewer);
      const dossierId = requireValue(scenario.dossierId, "dossierId for Viewer negatives");
      const snapshot = await workflow(page, dossierId);
      const req = requireValue(scenario.scopedRequirementId, "requirement for Viewer negatives");
      const mutations = [
        ["PUT", `${V2}/${dossierId}/metadata`, {
          expectedRevision: snapshot.revision, reason: "Viewer forbidden metadata mutation", comments: "forbidden",
        }],
        ["POST", `${V2}/${dossierId}/corrections`, {
          expectedRevision: snapshot.revision, reason: "Viewer forbidden correction request",
          severity: "Low", requirementIds: [req], fieldPaths: [], documentIds: [],
        }],
        ["POST", `${V2}/${dossierId}/evidence`, {
          expectedRevision: snapshot.revision, requirementId: req, correctionRequestId: null,
          documentId: null, storedFileId: randomUUID(), sha256: "a".repeat(64),
          fileName: "forbidden.pdf", reason: "Viewer forbidden evidence mutation",
        }],
        ["POST", `${V2}/${dossierId}/reopen-requests`, {
          expectedRevision: snapshot.revision, reason: "Viewer forbidden reopen request",
        }],
        ["POST", `${V2}/${dossierId}/override-requests`, {
          expectedRevision: snapshot.revision, action: "archive", reason: "Viewer forbidden override request",
        }],
        ["POST", `${V2}/${dossierId}/archive`, {
          expectedRevision: snapshot.revision, reason: "Viewer forbidden archive mutation",
        }],
      ] as const;

      const results = [];
      for (const [method, apiPath, body] of mutations) {
        const result = await api(page, method, apiPath, body);
        results.push({ method, apiPath, status: result.status, text: result.text });
        expect(result.status, `${method} ${apiPath}: ${result.text}`).toBe(403);
      }
      writeJson("viewer-v2-negative-results.json", results);
    });

    await phase("TAC and RA-ADM cannot review, approve or override", async () => {
      const dossierId = requireValue(scenario.dossierId, "dossierId for admin negatives");
      const req = requireValue(scenario.scopedRequirementId, "requirement for admin negatives");
      const results = [];

      for (const [role, email] of [
        ["TAC", USERS.tenantAdmin],
        ["RA-ADM", USERS.regulatoryAdmin],
      ] as const) {
        await login(page, email);
        const snapshot = await workflow(page, dossierId);
        const probes = [
          ["review", "POST", `${V2}/${dossierId}/corrections`, {
            expectedRevision: snapshot.revision, reason: `${role} forbidden technical review`,
            severity: "Low", requirementIds: [req], fieldPaths: [], documentIds: [],
          }],
          ["approve", "POST", `${RA}/dossiers/${dossierId}/approve-for-submission`, {
            notes: `${role} forbidden approval`,
          }],
          ["override", "POST", `${V2}/${dossierId}/override-requests`, {
            expectedRevision: snapshot.revision, action: "archive", reason: `${role} forbidden override request`,
          }],
        ] as const;
        for (const [action, method, apiPath, body] of probes) {
          const result = await api(page, method, apiPath, body);
          results.push({ role, action, status: result.status, text: result.text });
          expect(result.status, `${role} unexpectedly allowed ${action}: ${result.text}`).toBe(403);
        }
      }
      writeJson("admin-v2-negative-results.json", results);
    });

    await phase("timeline is append-only, sequential and complete", async () => {
      await login(page, USERS.viewer);
      const dossierId = requireValue(scenario.dossierId, "dossierId for timeline");
      const timeline = await mustApi(page, "GET", `${V2}/${dossierId}/timeline`, undefined, 200);
      const events = asArray(timeline.body);
      expect(events.length, "V2 timeline must contain all V2 mutations").toBeGreaterThanOrEqual(6);
      expect(events.map((x) => x.sequence)).toEqual(events.map((_: any, i: number) => i + 1));
      expect(events.map((x) => x.eventType)).toEqual(expect.arrayContaining([
        "MetadataUpdated",
        "CorrectionRequested",
        "EvidenceRevisionAdded",
        "CorrectionSubmitted",
      ]));
      expect(events.filter((x) => x.eventType === "EvidenceRevisionAdded")).toHaveLength(2);
      expect(events.every((x) => x.reason && x.correlationId && x.actorUserId)).toBe(true);
      writeJson("timeline.json", events);
    });

    await phase("certification-only dossier reaches Cancelled for controlled reopening", async () => {
      await login(page, USERS.specialist);
      const main = await dossierDetail(page, scenario);
      const product = (await mustApi(page, "POST", `${RA}/products`, {
        countryCode: "PA",
        category: "Certification",
        brand: "WFV2-CERT",
        regulatoryName: `Controlled Reopen ${scenario.stamp}`,
        commercialName: null,
        description: "Certification-only product for deterministic terminal-state testing",
        catalogCode: `REOPEN-${scenario.stamp.slice(-8)}`,
        internalCode: null,
        productType: "MedicalDevice",
        riskClass: "B",
        manufacturerId: null,
        distributorCompanyId: null,
        distributorName: null,
        initiative: "ReleaseCertification",
        priority: "High",
        salesMarketingInput: null,
        opportunityAmount: null,
        currency: "USD",
        registeredSuppliersCount: null,
        technicalSheetReference: null,
        formReference: null,
        sourceLineNumber: null,
      }, 200)).body;
      const terminal = (await mustApi(page, "POST", `${RA}/dossiers`, {
        productId: product.id,
        authorityId: main.authorityId,
        processType: "NewRegistration",
        existingRegistrationId: null,
        priority: "High",
        ownerUserId: null,
        salesMarketingInput: "Release certification only",
        opportunityAmount: null,
        currency: "USD",
        comments: "Deterministic controlled-reopen fixture",
        requirementPackId: main.requirementPackId,
        saveAsDraft: true,
      }, 200)).body;
      expect(terminal.status).toBe("Draft");
      const cancelled = await mustApi(page, "POST", `${V2}/${terminal.id}/cancel`, {
        expectedRevision: terminal.revision,
        reason: "Certification-only cancellation for controlled reopening",
      }, 200);
      expect(cancelled.body.status).toBe("Cancelled");
      scenario.terminalDossierId = terminal.id;
    });

    await phase("main dossier produces CT/RS and a renewal certification case", async () => {
      const dossierId = requireValue(scenario.dossierId, "main dossier for CT/RS");
      await login(page, USERS.approver);
      const internal = await mustApi(page, "POST", `${RA}/dossiers/${dossierId}/approve-for-submission`, {
        notes: "Independent internal approval for release certification",
      }, 200);
      expect(internal.body.status).toBe("ApprovedForSubmission");

      await login(page, "ra.sub@cert.local");
      const submissionProof = await uploadStoredFile(
        page,
        dossierId,
        `submission-${scenario.stamp}.pdf`,
        Buffer.from(`%PDF-1.7\nSubmission proof ${scenario.stamp}\n`, "utf8"));
      const submitted = await mustApi(page, "POST", `${RA}/dossiers/${dossierId}/submit`, {
        procedureNumber: `PROC-${scenario.stamp}`,
        externalNumber: `EXT-${scenario.stamp}`,
        submittedOn: new Date().toISOString(),
        proofStoredFileId: submissionProof,
      }, 200);
      expect(submitted.body.status).toBe("Submitted");

      await login(page, USERS.manager);
      const authorityReview = await mustApi(page, "POST", `${RA}/dossiers/${dossierId}/authority-review/start`, undefined, 200);
      expect(authorityReview.body.status).toBe("UnderAuthorityReview");
      const resolutionProof = await uploadStoredFile(
        page,
        dossierId,
        `resolution-${scenario.stamp}.pdf`,
        Buffer.from(`%PDF-1.7\nAuthority resolution ${scenario.stamp}\n`, "utf8"));
      await mustApi(page, "POST", `${RA}/dossiers/${dossierId}/approve`, {
        registrationNumber: `CTRS-${scenario.stamp}`,
        issuedOn: new Date().toISOString(),
        expiresOn: new Date(Date.now() + 365 * 86400_000).toISOString(),
        notes: "Certification CT/RS authority decision",
        resolutionStoredFileId: resolutionProof,
      }, 200);
      const closed = await dossierDetail(page, scenario);
      expect(closed.status).toBe("Closed");
      scenario.archiveDossierId = dossierId;

      const renewal = await mustApi(page, "POST", `${RA}/renewals`, {
        productId: requireValue(scenario.productId, "productId for renewal"),
        authorityId: closed.authorityId,
        requirementPackId: closed.requirementPackId,
      }, 200);
      expect(renewal.body.processType).toBe("Renewal");
      expect(renewal.body.existingRegistrationId).toBeTruthy();
      scenario.renewalDossierId = renewal.body.id;
    });

    await phase("reopen enforces requester separation and two distinct approvers", async () => {
      await login(page, USERS.specialist);
      const terminalDossierId = requireValue(scenario.terminalDossierId, "certification-only terminal dossier");
      let snapshot = await workflow(page, terminalDossierId);

      const request = await mustApi(page, "POST", `${V2}/${terminalDossierId}/reopen-requests`, {
        expectedRevision: snapshot.revision,
        reason: "Controlled E2E reopening requires independent approvals",
      }, 200);
      const requestId = request.body.id;

      snapshot = await workflow(page, terminalDossierId);
      const self = await api(page, "POST", `${V2}/${terminalDossierId}/reopen-requests/${requestId}/approve-first`, {
        expectedRevision: snapshot.revision,
        reason: "Requester must never approve own reopening",
      });
      expect(self.status, self.text).toBe(400);
      expect(self.text).toMatch(/requester|cannot approve/i);

      await login(page, USERS.manager);
      snapshot = await workflow(page, terminalDossierId);
      const first = await mustApi(page, "POST", `${V2}/${terminalDossierId}/reopen-requests/${requestId}/approve-first`, {
        expectedRevision: snapshot.revision,
        reason: "Independent first governance approval",
      }, 200);
      expect(first.body.approvalCount).toBe(1);

      snapshot = await workflow(page, terminalDossierId);
      const duplicate = await api(page, "POST", `${V2}/${terminalDossierId}/reopen-requests/${requestId}/approve-final`, {
        expectedRevision: snapshot.revision,
        reason: "Same approver must not satisfy second approval",
      });
      expect(duplicate.status, duplicate.text).toBe(400);

      await login(page, USERS.quality);
      snapshot = await workflow(page, terminalDossierId);
      const second = await mustApi(page, "POST", `${V2}/${terminalDossierId}/reopen-requests/${requestId}/approve-final`, {
        expectedRevision: snapshot.revision,
        reason: "Distinct second governance approval",
      }, 200);
      expect(second.body).toMatchObject({ approvalCount: 2, status: "Approved" });

      snapshot = await workflow(page, terminalDossierId);
      const executed = await mustApi(page, "POST", `${V2}/${terminalDossierId}/reopen-requests/${requestId}/execute`, {
        expectedRevision: snapshot.revision,
        reason: "Execute approved controlled reopening",
      }, 200);
      expect(executed.body.status).toBe("CorrectionRequested");
    });

    await phase("override enforces requester separation and two distinct approvers", async () => {
      await login(page, USERS.manager);
      const dossierId = requireValue(scenario.dossierId, "dossierId for override governance");
      let snapshot = await workflow(page, dossierId);
      const request = await mustApi(page, "POST", `${V2}/${dossierId}/override-requests`, {
        expectedRevision: snapshot.revision,
        action: "archive",
        reason: "Controlled override requires two independent approvers",
      }, 200);
      const requestId = request.body.id;

      snapshot = await workflow(page, dossierId);
      const self = await api(page, "POST", `${V2}/${dossierId}/override-requests/${requestId}/approve`, {
        expectedRevision: snapshot.revision,
        reason: "Requester cannot approve own override",
      });
      expect(self.status, self.text).toBe(400);

      await login(page, USERS.quality);
      snapshot = await workflow(page, dossierId);
      const first = await mustApi(page, "POST", `${V2}/${dossierId}/override-requests/${requestId}/approve-first`, {
        expectedRevision: snapshot.revision,
        reason: "Independent first override approval",
      }, 200);
      expect(first.body.approvalCount).toBe(1);

      await login(page, USERS.approver);
      snapshot = await workflow(page, dossierId);
      const second = await mustApi(page, "POST", `${V2}/${dossierId}/override-requests/${requestId}/approve-final`, {
        expectedRevision: snapshot.revision,
        reason: "Distinct second override approval",
      }, 200);
      expect(second.body).toMatchObject({ approvalCount: 2, status: "Approved" });
    });

    await phase("soft archive preserves the complete timeline", async () => {
      await login(page, USERS.quality);
      const archiveDossierId = requireValue(scenario.archiveDossierId, "closed certification dossier");
      const before = asArray((await mustApi(page, "GET", `${V2}/${archiveDossierId}/timeline`, undefined, 200)).body);
      const snapshot = await workflow(page, archiveDossierId);

      await mustApi(page, "POST", `${V2}/${archiveDossierId}/archive`, {
        expectedRevision: snapshot.revision,
        reason: "Soft archive after terminal Workflow V2 completion",
      }, 200);

      const after = asArray((await mustApi(page, "GET", `${V2}/${archiveDossierId}/timeline`, undefined, 200)).body);
      expect(after.length).toBe(before.length + 1);
      expect(after.slice(0, before.length)).toEqual(before);
      expect(after.at(-1)?.eventType).toBe("DossierArchived");
      expect(after.map((x) => x.sequence)).toEqual(after.map((_: any, i: number) => i + 1));
      writeJson("archived-timeline.json", after);
    });

    writeJson("workflow-v2-final.json", {
      generatedAt: new Date().toISOString(),
      tenantId: TENANT,
      scenario,
      passed: phases.every((x) => x.passed),
      phases,
    });
    const failures = phases.filter((x) => !x.passed);
    expect(failures, `Workflow V2 capability failures:\n${JSON.stringify(failures, null, 2)}`).toEqual([]);
  });
});

async function login(page: Page, email: string): Promise<void> {
  await page.goto("/");
  await page.evaluate(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
  await page.reload();
  await page.waitForSelector("#login-form, #legacy-login-form", { timeout: 20_000 });

  if (await page.locator("#tenantId").count()) {
    await page.fill("#tenantId", TENANT);
    await page.fill("#legacy-email, #email", email);
    await page.fill("#legacy-password, #password", PASSWORD);
    await page.click('#legacy-login-form button[type="submit"], #login-form button[type="submit"]');
  } else {
    await page.fill("#email", email);
    await page.click('#login-form button[type="submit"]');
    const organization = page.locator('input[name="organizationId"]');
    const password = page.locator("#password");
    await Promise.race([
      organization.first().waitFor({ state: "visible", timeout: 20_000 }),
      password.waitFor({ state: "visible", timeout: 20_000 }),
    ]);
    if (await organization.count()) {
      const tenant = page.locator(`input[name="organizationId"][value="${TENANT}"]`);
      expect(await tenant.count(), `${email} is not assigned to tenant ${TENANT}`).toBeGreaterThan(0);
      await tenant.check();
      await page.click('#login-form button[type="submit"]');
    }
    await password.waitFor({ state: "visible", timeout: 20_000 });
    await password.fill(PASSWORD);
    await page.click('#login-form button[type="submit"]');
  }
  await page.waitForSelector("aside.sidebar", { timeout: 45_000 });
  await expect(page.locator("aside.sidebar")).toBeVisible();
  await page.evaluate((roleEmail) => sessionStorage.setItem("e2e.activeRole", roleEmail), email);
}

async function ensureRole(page: Page, email: string): Promise<void> {
  const active = await page.evaluate(() => sessionStorage.getItem("e2e.activeRole")).catch(() => null);
  if (active !== email) await login(page, email);
}

async function openRegulatory(page: Page): Promise<void> {
  await page.evaluate(() => {
    location.hash = "#/regulatory";
  });
  await page.waitForSelector(".ra-shell", { timeout: 30_000 });
}

async function openDossierUi(page: Page, dossierId: string): Promise<void> {
  await openRegulatory(page);
  await page.evaluate((id) => {
    (window as any).__raOpenDossier = id;
  }, dossierId);
  await page.locator('#ra-nav [data-view="dossiers"]').click();
  await expect(page.locator("#ra-back")).toBeVisible();
}

async function api(page: Page, method: string, apiPath: string, body?: unknown): Promise<ApiResult> {
  return page.evaluate(async ({ method, apiPath, body }) => {
    const token = localStorage.getItem("c360.token");
    const response = await fetch(apiPath, {
      method,
      headers: {
        Authorization: `Bearer ${token}`,
        ...(body === undefined ? {} : { "Content-Type": "application/json" }),
      },
      body: body === undefined ? undefined : JSON.stringify(body),
    });
    const text = await response.text();
    let parsed: any = null;
    try {
      parsed = text ? JSON.parse(text) : null;
    } catch {
      parsed = text;
    }
    return { status: response.status, body: parsed, text };
  }, { method, apiPath, body });
}

async function mustApi(
  page: Page,
  method: string,
  apiPath: string,
  body: unknown,
  expectedStatus: number,
): Promise<ApiResult> {
  const result = await api(page, method, apiPath, body);
  expect(result.status, `${method} ${apiPath}\n${result.text}`).toBe(expectedStatus);
  return result;
}

async function workflow(page: Page, dossierId: string): Promise<any> {
  return (await mustApi(page, "GET", `${V2}/${dossierId}/workflow`, undefined, 200)).body;
}

async function dossierDetail(page: Page, scenario: Scenario): Promise<any> {
  const dossierId = requireValue(scenario.dossierId, "dossierId");
  return (await mustApi(page, "GET", `${RA}/dossiers/${dossierId}`, undefined, 200)).body;
}

async function uploadStoredFile(
  page: Page,
  dossierId: string,
  fileName: string,
  bytes: Buffer,
): Promise<string> {
  const result = await page.evaluate(async ({ apiPath, fileName, bytes }) => {
    const token = localStorage.getItem("c360.token");
    const form = new FormData();
    form.append("file", new File([new Uint8Array(bytes)], fileName, { type: "application/pdf" }));
    const response = await fetch(apiPath, {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: form,
    });
    const text = await response.text();
    let body: any = null;
    try {
      body = text ? JSON.parse(text) : null;
    } catch {
      body = text;
    }
    return { status: response.status, body, text };
  }, {
    apiPath: `${RA}/dossiers/${dossierId}/evidence`,
    fileName,
    bytes: Array.from(bytes),
  });
  expect(result.status, `Physical evidence upload failed: ${result.text}`).toBe(200);
  expect(result.body?.id, "Storage upload did not return a stored file id").toBeTruthy();
  return result.body.id;
}

function sha256(bytes: Buffer): string {
  return createHash("sha256").update(bytes).digest("hex");
}

function asArray(value: any): any[] {
  expect(Array.isArray(value), `Expected array, received ${JSON.stringify(value)}`).toBe(true);
  return value as any[];
}

function requireValue<T>(value: T | undefined | null, label: string): T {
  if (value === undefined || value === null || value === "") {
    throw new Error(`Prerequisite unavailable: ${label}. This phase is not skipped.`);
  }
  return value;
}

async function screenshot(page: Page, fileName: string): Promise<void> {
  await page.screenshot({ path: path.join(EVIDENCE_DIR, fileName), fullPage: true });
}

function writeJson(fileName: string, value: unknown): void {
  fs.writeFileSync(path.join(EVIDENCE_DIR, fileName), JSON.stringify(value, null, 2));
}
