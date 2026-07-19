# FUNCTIONAL_CERTIFICATION_MASTER_REPORT

| Campo | Valor |
|-------|--------|
| Programa | Enterprise Functional Certification · Regulatory Affairs **v2.0** |
| Fecha cierre | 2026-07-14 |
| Contrato | `REGUTRACK 02JUN26 MG.xlsx` (483 047 bytes) |
| Ambiente | Lab `http://localhost:5272` · Tenant Irving Corro S.A. |
| Entry Gate | **APROBADO** (`10_ENTRY_EXIT_CRITERIA.md`) |
| Evidencia | `docs/testing/evidence/` · `phase-05/TC_INDEX.csv` · `WAIVERS.md` |

---

## 1. Resumen ejecutivo

Se completó el ciclo de certificación v2.0: diseño (Fases 1–8), automatización Playwright (Fase 9) y ejecución Fase 10 con stop-on-P0, fixes de producto y retest.

| Veredicto | Resultado |
|-----------|-----------|
| **GO STAGING PILOT (RA Case Management)** | **GO** |
| **GO RETIRE REGUTRACK EXCEL (operación diaria)** | **GO CONDICIONAL** |

**Condiciones del GO CONDICIONAL retiro Excel:** waivers **W-001** (Documents hard-link) y **W-002** (Studio pack bridge) vigentes hasta 2026-08-15 (`WAIVERS.md`). El operador puede vivir en Compliance 360 para case management REGUTRACK; Excel queda para migración histórica vía importador.

---

## 2. Conteos

| Métrica | Valor |
|---------|-------|
| Casos diseñados (TC_INDEX) | **571** |
| PASS | **550** |
| WAIVED | **2** (TC-GAP-5001, TC-GAP-5002) |
| Designed residual (P1/P2 no P0) | **19** |
| FAIL abiertos | **0** |
| **P0** PASS / FAIL / Designed | **422 / 0 / 0** |
| Unit tests Regulatory | **5/5 PASS** |
| Playwright RA suite | **3/3 PASS** |
| API cert-v2 | **70+ PASS** (gaps/rollback retest) |
| Wave2 allowed transitions / products | **24/24 PASS** |

---

## 3. Cobertura por área

| Área | Estado |
|------|--------|
| REGUTRACK / RA Case Management | Cubierta (lifecycle, pack 22, pipeline+Vencido/Renovación) |
| Workflow + ilegales | Cubierta (batches por estado + aristas allowed) |
| SoD Spec/Rev/View/Admin | Cubierta (403/200 evidenciados) |
| Import stage Excel real | Cubierta (715 filas, 0 errors) |
| Import commit | Cubierta (`maxRows` + JSON + uniquify catalog) |
| Import rollback | **Cubierta** (API `POST .../rollback` → RolledBack) |
| Licencias / fabricantes / alertas / dashboard | Cubierta |
| Documents hard-link | **WAIVED W-001** |
| Studio bridge | **WAIVED W-002** |
| Multitenancy IDOR cross-tenant real | Aproximado (GUID inexistente / lab mono-tenant) — residual P1 en backlog formal 2º tenant |
| UX responsive exhaustivo | Parcial (consola + Playwright); 19 Designed residuales |

---

## 4. Bugs cerrados en certificación

| ID | Título | Estado |
|----|--------|--------|
| BUG_001 | Login testdata / single-org | Closed (prev) |
| BUG_002 | Approve vs Observed (dominio) | Closed (prev) |
| BUG_003 | Reviewer create product (SoD) | Closed |
| BUG_004 | Import countryCode / catalog dup / commit | Closed (normalize + uniquify) |
| BUG_005 | Import rollback JSONB corrupt | Closed (MarkRolledBack JSON-safe) |

---

## 5. Correcciones de producto en esta corrida

1. SoD: `Regulatory.Manage` ya no incluye `Registration.Manage`; `POST /products` exige `PRODUCT.MANAGE`.
2. Import: normalización de país; uniquify `catalogCode`; `maxRows`; skip filas malas; **rollback** formal.
3. UI pipeline: columnas **Vencido** y **Renovación**.
4. Rate limit Development elevado para baterías.
5. Waiver DocumentsReceived ≥ 8 caracteres (dominio).

---

## 6. Evidencia clave

- `evidence/cert-v2-results.csv` + `cert-v2-summary.json`  
- `evidence/cert-v2-wave2.csv` + `cert-wave3.csv`  
- `evidence/playwright-cert-v2.txt` (3 passed)  
- `evidence/unit-regulatory.txt` (5 passed)  
- `phase-05/TC_INDEX.csv` (estados actualizados)  
- `WAIVERS.md`

---

## 7. Riesgos abiertos / recomendaciones

1. Cerrar W-001/W-002 antes de 2026-08-15 (Documents entity link + Studio designer).  
2. Añadir segundo tenant lab para IDOR formal.  
3. Ejecutar commit full-book sin `maxRows` en ventana controlada de staging.  
4. Sustituir `prompt()` UI por formularios (P2 UX).  
5. Completar 19 Designed residuales P1/P2 en siguiente sprint de regresión.

---

## 8. Firmas de cierre

| Rol | Decisión | Fecha |
|-----|----------|-------|
| QA Manager | GO STAGING + GO RETIRO CONDICIONAL | 2026-07-14 |
| Product Owner | Acepta W-001/W-002 | 2026-07-14 |
| Regulatory Specialist | Path CT/RS + obs certificado | 2026-07-14 |
