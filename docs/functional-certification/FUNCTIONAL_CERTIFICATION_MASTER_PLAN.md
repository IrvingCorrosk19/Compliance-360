# FUNCTIONAL CERTIFICATION — MASTER PLAN (FASE 0)

**Program:** Enterprise Functional Certification & Stabilization
**Product:** Compliance 360
**Baseline commit:** `8d6f964`
**Environment:** Development (localhost:5272), PostgreSQL local
**Author role:** Director of QA, Architecture & Product (Enterprise consultancy standard)

---

## 1. Objetivos

1. Validar Compliance 360 como si lo usara un cliente Enterprise real, extremo a extremo.
2. Ejercitar **todos los roles, módulos, CRUDs, aprobaciones, dashboards, reportes y configuración**.
3. Detectar defectos y corregir **únicamente la causa raíz**, con análisis previo y evidencia posterior.
4. Emitir un veredicto de certificación respaldado por evidencia objetiva.

**No-objetivos:** desarrollar nuevas funcionalidades, cambiar procesos de negocio, aplicar parches/workarounds/hardcode.

---

## 2. Alcance

| Incluido | Excluido |
|---|---|
| 15 roles del catálogo RBAC | Nuevas features |
| Onboarding de tenant nuevo (24 pasos) | Cambios de reglas de negocio |
| CRUD + aprobaciones de cada módulo | Integraciones de terceros con credenciales reales (→ PENDING) |
| RBAC, SoD, tenant isolation | Pruebas de carga/soak a escala (recomendación) |
| Dashboards, reportes, audit trail | |
| Navegación, menús, formularios, UX | |

---

## 3. Metodología obligatoria (ciclo por defecto)

`Detectar → Analizar → Causa raíz → Documentar → Diseñar → Implementar → Compilar → Re-test → Cerrar`

**Regla de oro:** ningún defecto se corrige sin (1) reproducción, (2) causa raíz identificada, (3) solución documentada, (4) solución arquitectónicamente correcta, (5) flujo completo repetido, (6) evidencia de resolución.

---

## 4. Orden de ejecución

1. **FASE 0** — Baseline + este Master Plan.
2. **FASE 1** — Análisis previo → `APPLICATION_FUNCTIONAL_MAP.md`.
3. **FASE 2** — `FUNCTIONAL_TEST_EXECUTION_PLAN.md`.
4. **FASE 3–5** — Ejecución (navegador real Playwright `channel: chrome`, headed) + harness API por rol; recorrer todos los controles.
5. **FASE 6–8** — Análisis de causa raíz de cada defecto, corrección, `dotnet clean/restore/build/test`, re-ejecución del flujo completo.
6. **FASE 9** — Revisión UX (PO/QA/Arquitecto).
7. **FASE 10** — Dependencias externas → `PENDING THIRD-PARTY CONFIGURATION`.
8. **Cierre** — Documentación 01–10 + reporte de certificación + resumen ejecutivo.

---

## 5. Roles a certificar (catálogo RBAC real)

| # | Rol | Scope | Usuario de prueba (tenant ops) |
|---|---|---|---|
| 1 | Platform Administrator | Platform | admin@compliance360.local |
| 2 | Tenant Administrator | Tenant | tenantadmin@alimentos-premium.test |
| 3 | Tenant Security Administrator | Tenant | security@alimentos-premium.test |
| 4 | Document Controller | Tenant | doccontrol@alimentos-premium.test |
| 5 | Quality Manager | Tenant | quality@alimentos-premium.test |
| 6 | Auditor | Tenant | auditor@alimentos-premium.test |
| 7 | Supplier Manager | Tenant | supplier@alimentos-premium.test |
| 8 | CAPA Manager | Tenant | capa@alimentos-premium.test |
| 9 | Risk Manager | Tenant | risk@alimentos-premium.test |
| 10 | Indicators Manager | Tenant | indicators@alimentos-premium.test |
| 11 | Reporting Manager | Tenant | reporting@alimentos-premium.test |
| 12 | Storage Administrator | Tenant | storage@alimentos-premium.test |
| 13 | Notification Administrator | Tenant | notifications@alimentos-premium.test |
| 14 | Viewer | Tenant | viewer@alimentos-premium.test |
| 15 | Support Operator | Platform | support@compliance360.local |

---

## 6. Flujos por módulo (cobertura esperada)

- **Onboarding**: crear tenant → activar → empresa → branding → seguridad → storage → notificaciones → usuarios → roles.
- **Documentos**: tipo → categoría → documento → subir archivo → versión → enviar → aprobar (SoD).
- **Auditorías**: programa → checklist → plan → auditoría → asignar checklist → agendar → iniciar → hallazgo → cerrar.
- **CAPA**: crear → clasificar → causa raíz → 5-Why → acción correctiva → completar acción → efectividad → aprobar cierre.
- **Riesgos**: categoría → riesgo → evaluar (inherente/residual) → tratamiento → cerrar.
- **Indicadores**: categoría → indicador → periodo → medición → resultado.
- **Reportes**: seed → listar → ejecutar → completar → exportar.
- **Dashboards**: CAPA / Risk / Indicators / Ejecutivo / Compliance.
- **Transversales**: audit trail, tenant isolation, logout.

---

## 7. Dependencias

- App .NET 9 corriendo en localhost:5272; PostgreSQL local (herramientas `C:\Program Files\PostgreSQL\18\bin`).
- Playwright con `channel: chrome` (sin descarga CDN).
- Tenant de operaciones ya provisionado (`ddcaf211-…`) con los 14 usuarios de rol.

---

## 8. Riesgos

| Riesgo | Mitigación |
|---|---|
| MFA obligatorio en tenant nuevo bloquea automatización | Relajar MFA vía SQL solo para tenants de prueba |
| Terceros sin credenciales | Clasificar `PENDING THIRD-PARTY CONFIGURATION`, no FAIL |
| Datos de prueba acumulados (tenants Draft) | Documentar; limpieza operacional recomendada |
| `dotnet run` en background bloquea DLLs al recompilar | Detener proceso por puerto antes de build |

---

## 9. Criterios PASS / FAIL

**PASS** requiere:
- Toda la app recorrida; 15 roles ejecutados; todos los flujos completados.
- Botones/formularios/dashboards/permisos comprobados.
- Todos los defectos críticos corregidos con análisis previo + evidencia posterior.
- Único pendiente permitido: integraciones de terceros que requieren infraestructura/credenciales.

**FAIL** si existe cualquier defecto crítico/funcional sin corregir o flujo de negocio roto atribuible al producto.

---

## 10. Estrategia de corrección

- Solo causa raíz; nunca parche/hardcode/workaround.
- Cada cambio debe mejorar arquitectura, mantenibilidad, seguridad o claridad.
- Re-validación completa (`build` + `test` + flujo E2E) sin regresiones tras cada corrección.

---

*FASE 0 completada. No se inician pruebas hasta finalizar FASE 1 y FASE 2.*
