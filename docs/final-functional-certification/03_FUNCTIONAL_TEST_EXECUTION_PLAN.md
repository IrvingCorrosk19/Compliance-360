# 03 — FUNCTIONAL TEST EXECUTION PLAN

**Program:** Final Enterprise Functional Certification  
**Date:** 2026-07-05  
**Tenant ops:** `ddcaf211-afe0-44a0-9c90-4fbda8fc4871`  
**Platform tenant:** `dc7c46ee-cb25-4ed5-b0b4-800788f7f626`

---

## 1. Gates técnicos (pre-requisito)

| # | Gate | Comando | Esperado |
|---|---|---|---|
| G1 | Clean | `dotnet clean Compliance360.sln` | Exit 0 |
| G2 | Restore | `dotnet restore Compliance360.sln` | Exit 0 |
| G3 | Build Release | `dotnet build -c Release` | 0 errors, 0 warnings |
| G4 | Unit tests | `dotnet test -c Release` | 238/238 PASS |
| G5 | Health | `GET /health` | 200 Healthy |
| G6 | Bootstrap | Log "Ready for Functional Testing" | OK |

---

## 2. Suite Playwright (headful)

**Config:** `e2e/playwright.config.ts` — `headless: false`, `channel: chrome`, reporter certificación en tiempo real.

### 2.1 roles.spec.ts (14 tests — RBAC & navegación)

| # | Rol | Quién | Pasos | Resultado esperado |
|---|---|---|---|---|
| R01 | Platform Administrator | Playwright | Login → nav visible → manage affordances → landing → logout | superadmin + TAC visible; no business modules |
| R02 | Tenant Administrator | Playwright | Login → TAC visible → logout | dashboard, TAC, audit-trail |
| R03 | Tenant Security Admin | Playwright | Login → security visible | security, audit-trail |
| R04 | Document Controller | Playwright | Login → documents + create form | manage documents=true |
| R05 | Quality Manager | Playwright | Login → modules visible, no create | manage documents=false |
| R06 | Auditor | Playwright | Login → audits + create, no CAPA form | manage audits=true, capa=false |
| R07 | Supplier Manager | Playwright | Login → suppliers + create | manage suppliers=true |
| R08 | CAPA Manager | Playwright | Login → capa + create | manage capa=true |
| R09 | Risk Manager | Playwright | Login → risks + create | manage risks=true |
| R10 | Indicators Manager | Playwright | Login → indicators + create | manage indicators=true |
| R11 | Reporting Manager | Playwright | Login → reports visible | reports in nav |
| R12 | Storage Admin | Playwright | Login → configuration storage button | storage button, no email |
| R13 | Notification Admin | Playwright | Login → configuration email button | email button, no storage |
| R14 | Viewer | Playwright | Login → all read modules, no forms | read-only all modules |

### 2.2 functional.spec.ts (15 tests — flujos funcionales)

| # | ID | Rol | Proceso | Pasos | Esperado |
|---|---|---|---|---|---|
| F01 | Platform Admin | Platform center + tenant tab | Login → superadmin → tenants tab → API 403 documents | PASS |
| F02 | Tenant Admin | TAC + users | Login → TAC → users tab → API list users 200 | PASS |
| F03 | Security Admin | Security workspace | Login → create enterprise item | PASS |
| F04 | Document Controller | Document SoD create | Login → create document → API approve 403 | PASS |
| F05 | Quality Manager | Document SoD approve | Login → no create form → has APPROVE perm | PASS |
| F06 | Auditor | Audit + SoD CAPA | Login → create audit → no CAPA form | PASS |
| F07 | Supplier Manager | Create supplier | Login → create supplier | PASS |
| F08 | CAPA Manager | Create CAPA + SoD | Login → create CAPA → no APPROVE perm | PASS |
| F09 | Risk Manager | Create risk | Login → create risk | PASS |
| F10 | Indicators Manager | Create indicator | Login → create indicator | PASS |
| F11 | Reporting Manager | Report center | Login → seed reports → execute | PASS |
| F12 | Storage Admin | Storage provider | Login → create storage provider | PASS |
| F13 | Notification Admin | Email provider | Login → create email provider | PASS |
| F14 | Viewer | Read-only | Login → 6 modules no form → API create 403 | PASS |
| F15 | Support Operator | Break-glass | Login → SUPPORT.ACCESS → limited menu | PASS |

---

## 3. Customer Journey API (23 pasos)

**Script:** `scripts/customer_journey.ps1`  
**Actor:** Platform Admin → Tenant Admin → roles operativos  
**Ciclo:** Tenant create → activate → company → branding → security → storage → notifications → users → roles → documents (full lifecycle) → audits → CAPA (5-Why, Ishikawa, closure) → risks → indicators → reports → dashboards → audit trail → logout

---

## 4. Probes de seguridad

| Probe | Actor | Acción | Esperado |
|---|---|---|---|
| S1 | Platform Admin | GET cross-tenant documents | 403 |
| S2 | Document Controller | POST document approve self | 403 |
| S3 | Viewer | POST create document | 403 |
| S4 | Storage Admin | No email provider button | SoD |
| S5 | Notification Admin | No storage provider button | SoD |

---

## 5. Dependencias entre tests

```
G1-G6 (gates) → roles.spec.ts (14) → functional.spec.ts (15, F04-F05 serial)
                → customer_journey.ps1 (23)
                → Reportes 04-10
```

F04 debe ejecutarse antes de F05 (cadena SoD documentos).

---

## 6. Datos de prueba

| Recurso | Valor |
|---|---|
| Platform admin | admin@compliance360.local / OwnerStart!2026 |
| Tenant users | *@alimentos-premium.test / Premium!2026 |
| Support | support@compliance360.local / OwnerStart!2026 |
| testdata.json | `e2e/testdata.json` |

---

## 7. Evidencias por test

- Screenshot (on)
- Video (on)
- Trace (on)
- `functional-summary.json` por rol
- `summary.json` por rol RBAC
- HTML report en `artifacts/e2e/html-report`

---

## 8. Criterio de parada

Si cualquier test FAIL → detener → capturar evidencia → generar `ROOT_CAUSE_ANALYSIS_xxxx.md` → corregir → rebuild → retest flujo completo.

---

*Plan de ejecución definido. Proceder a FASE 3.*
