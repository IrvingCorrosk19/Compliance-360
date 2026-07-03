# 07 — E2E Certification — Compliance 360

Date: 2026-07-03 · Tool: Playwright (real Chrome) · Suite: `e2e/tests/*.spec.ts`.

## Result

**29 / 29 tests passed** (RBAC navigation + functional flows), re-run **after** the architectural EF
fixes with **no regression**. Runtime ~3 min, single worker.

## Coverage (15 roles)

Platform Administrator, Tenant Administrator, Tenant Security Administrator, Document Controller,
Quality Manager, Auditor, Supplier Manager, CAPA Manager, Risk Manager, Indicators Manager, Reporting
Manager, Storage Administrator, Notification Administrator, Viewer, Support Operator (break-glass).

Each role is validated for: login, correct menu/navigation visibility, write vs read-only affordances,
data creation in its modules, and SoD enforcement (e.g. Document Controller creates + submits for review
but cannot self-approve; CAPA Manager creates but cannot approve closure; Viewer cannot write).

## Evidence

Playwright HTML + JSON reports, screenshots, traces and console/network capture under `artifacts/e2e/`;
per-role and global reports under `docs/e2e/`.

## Gap note (honest)

The automated E2E suite emphasises create + permission/SoD enforcement. Deep multi-step business
lifecycle transitions (e.g. CAPA classify→root-cause→actions→effectiveness→closure) are validated
separately via the UAT API probes in §08 (which uncovered and fixed the systemic add-child-on-update
defect). Extending the browser suite to cover full closure chains is recommended (risks register).

## Verdict

**E2E PASS.**
