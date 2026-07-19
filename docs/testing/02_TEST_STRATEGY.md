# 02 — Test Strategy
## Enterprise Functional Certification · Regulatory Affairs · v2.0

| Campo | Valor |
|-------|--------|
| Estado | DISEÑO — sin ejecución |
| Norma de evidencia | ALCOA+ (ver `01`) |

---

## 1. Principios

1. **Truth source:** Excel REGUTRACK + matriz de cobertura + sistema **as-built** (API/UI/dominio). Nunca invenciones.
2. **Diseño antes de código de prueba:** Fases 1–8 → Gate → 9 (automation) → 10 (ejecución).
3. **Humano primero para P0/P1:** automatización no sustituye certificación manual crítica.
4. **Stop on P0:** un FAIL P0 detiene la cola hasta fix + retest PASS.
5. **SoD de prueba:** cada rol RA se certifica en sesión aislada (no mezclar tokens).
6. **Regression ≠ Certification:** Playwright cubre regresiones recurrentes tras PASS manual.

---

## 2. Pirámide de pruebas (orden de valor)

```
                 ┌─────────────────────┐
                 │  Certificación GO   │  Master Report + firmas
                 │  (humana + evidencia)│
                 └──────────▲──────────┘
                 ┌──────────┴──────────┐
                 │ Playwright E2E RA   │  Fase 9 — regresión
                 └──────────▲──────────┘
                 ┌──────────┴──────────┐
                 │ Manual UI enterprise│  Fase 10 — clicks reales
                 └──────────▲──────────┘
                 ┌──────────┴──────────┐
                 │ API contract / RBAC │  Misma Fase 10 (Postman/script)
                 └──────────▲──────────┘
                 ┌──────────┴──────────┐
                 │ Unit / Domain RA    │  Gate E7 (`dotnet test` Regulatory)
                 └─────────────────────┘
```

---

## 3. Técnicas por tipo

| Tipo | Técnica | Dónde |
|------|---------|--------|
| Funcional positivo | Escenario end-to-end + TC atómicos | Fases 5–6, 10 |
| Funcional negativo | Equivalencia, boundary, abuso | Fase 7 |
| Workflow | Tabla de estados × transiciones (allowed + illegal) | Dominio + TC-WF-* |
| RBAC / SoD | Matriz rol × acción (allow/deny) | Fases 3, 8 |
| Datos | REGUTRACK real + seeds sintéticos controlados | `06` |
| Import | Stage XLSX completo; commit con control de volumen (`maxRows` + corrida full documentada) | TC-RA-09xx |
| Alertas | Data-driven thresholds 90…0 | TC-RA-06xx |
| UX / NFR | Heurística + scripts F5/back/doble-clic/responsive | `04`, TC-UX-* |
| Seguridad | 401, IDOR cross-tenant, prompt injection URL | TC-SEC-* |
| Regresión | Playwright post-aprobación | Fase 9 |

---

## 4. Priorización (risk-based)

Ver `08_RISK_BASED_TESTING.md`.

| Prioridad | Cobertura de ejecución (Fase 10) |
|-----------|-----------------------------------|
| P0 | **100%** antes de cualquier GO |
| P1 | **100%** antes de GO retiro Excel (waiver excepcional firmado) |
| P2 | 100% diseño; ejecución completa o muestreo documentado ≥80% con justificación auditor |
| P3 | Diseño completo; ejecución best-effort documentada |

---

## 5. Entornos

Ver `05_TEST_ENVIRONMENT.md`. Lab local cualificado → staging → (marco) producción.

---

## 6. Datos y usuarios

Ver `06_TEST_DATA.md` y `07_TEST_USERS.md`.  
Usuarios RA dedicados por rol; password política ≥12 con complejidad; **sin** reutilizar credenciales de producción real.

---

## 7. Defect management

1. STOP si P0.  
2. Crear `docs/testing/bugs/BUG_xxx.md` (plantilla en `12`).  
3. Fix → build → retest del TC + smoke vecinos.  
4. Cerrar solo con evidencia de PASS.  
5. No SKIPPED.

---

## 8. Automatización (Fase 9) — reglas

- Solo TC aprobados y trazados.  
- Credenciales vía config de test — **no** hardcode de secretos de producción.  
- Assertion sobre comportamiento observable (UI + API), no mocks del dominio crítico.  
- Fallo Playwright en TC ya PASS-manual = **regresión P0/P1**, no “flaky ignore”.

---

## 9. Criterio de “suficiencia de evidencia” para GO

No es “PASSaron N casos”. Es:

- G1–G12 de `10` satisfechos.  
- Cobertura Excel sin Pendiente crítico sin WAIVER.  
- SoD Specialist≠Approve y Reviewer≠ProductManage demostrados.  
- Import del Excel real stage 0 errors + commit evidenciado (volumen y límites documentados).  
- Master Report con veredicto explícito.
