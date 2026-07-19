# Reporte de calidad — Documentación de pruebas funcionales manuales

**Programa:** Compliance 360 Manual Functional Testing  
**Versión documentación:** 1.0  
**Fecha generación:** 2026-07-09  
**Autor:** Generación asistida — revisión humana requerida

---

## 1. Resumen ejecutivo

Este reporte evalúa la **calidad, completitud y alineación con la aplicación real** del conjunto de manuales en `docs/manual-functional-testing/`. El programa cubre 17 roles/escenarios operativos, 12 procesos E2E, plantillas de resultados/defectos y checklist de cierre.

| Métrica | Valor |
|---------|-------|
| Documentos en el programa | 22 (00–21) |
| Manuales por rol | 16 (01–16) |
| Casos de prueba documentados | 204 |
| Idioma | Español |
| URL base documentada | `http://localhost:5272` |

---

## 2. Inventario de documentos

| Archivo | Estado | Casos | Secciones 1–12 |
|---------|--------|-------|----------------|
| 00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md | Existente (no sobrescrito) | — | N/A (roadmap) |
| 01_PLATFORM_ADMINISTRATOR_FUNCTIONAL_TESTS.md | Completo | 10 | Sí |
| 02_TENANT_ADMINISTRATOR_FUNCTIONAL_TESTS.md | Completo | 12 | Sí |
| 03_TENANT_SECURITY_ADMINISTRATOR_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 04_STORAGE_ADMINISTRATOR_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 05_NOTIFICATION_ADMINISTRATOR_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 06_DOCUMENT_CONTROLLER_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 07_QUALITY_MANAGER_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 08_TECHNICAL_SHEETS_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 09_SUPPLIER_MANAGER_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 10_AUDITOR_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 11_CAPA_MANAGER_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 12_RISK_MANAGER_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 13_INDICATORS_MANAGER_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 14_REPORTING_MANAGER_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 15_VIEWER_FUNCTIONAL_TESTS.md | Creado | 14 | Sí |
| 16_SUPPORT_OPERATOR_FUNCTIONAL_TESTS.md | Creado | 12 | Sí |
| 17_END_TO_END_BUSINESS_PROCESS_TESTS.md | Creado | 12 | Sí |
| 18_FUNCTIONAL_TEST_RESULTS_TEMPLATE.md | Expandido | — | Plantilla |
| 19_DEFECT_REPORT_TEMPLATE.md | Expandido | — | Plantilla |
| 20_FINAL_MANUAL_TESTING_CHECKLIST.md | Expandido | — | Checklist |
| 21_DOCUMENTATION_QUALITY_REPORT.md | Este documento | — | Reporte |

---

## 3. Alineación con hechos de la aplicación

| Requisito | Cobertura en docs | Fuente verificada |
|-----------|-------------------|-------------------|
| URL localhost:5272 | Todos los manuales | Roadmap, app launch |
| Login v2: Siguiente, Continuar, Iniciar sesion | Sección login en cada manual | `app.js` LOGIN_V2 |
| Legacy c360.login.v2=false | 00, 01, 17 E2E-009 | Roadmap §3.5 |
| Rutas hash documentadas | 00 §6 + manuales por módulo | `app.js` routes |
| #module-action-form, Crear registro real | 06–14 | `app.js` L2575+ |
| Campos documento/supplier/CAPA/risk/audit/indicator | Manuales 06–13 | `app.js` L2540+ |
| Configuration: Crear Storage Local, Email SMTP, Probar | 04, 05 | `app.js` L945+ |
| Tenant admin tabs | 02, 03 | `app.js` TAC panels |
| Guardar seguridad, campos MFA/lockout/IP | 03 | `app.js` L1702+ |
| Report Center: Ejecutar, Programar, Seed, Exportar | 14, 17 | `app.js` L1042+ |
| Modo solo lectura Viewer | 15, 07 | `app.js` L2584+ |
| support@, PLATFORM.SUPPORT.ACCESS, no break-glass UI | 16 | RoleCatalog, roadmap §11 |
| e2e @alimentos-premium.test | 02–16 | `e2e_provision.ps1` |
| Platform tenant dc7c46ee... | 01, 16 | provision script |
| Sin contraseñas en docs | Todos → testdata/appsettings | Política programa |
| QM DOCUMENT.APPROVE vía Swagger | 07 TC-QM-DOC-003 | FoundationEndpoints |
| TECHNICALSHEET.CREATE no en rol estándar | 08, 02 TC-TA-007 | RoleCatalog |
| customer_journey.ps1 | 07, 17 E2E-008 | scripts/ |

**Veredicto alineación:** ALTA — documentación refleja implementación actual con limitaciones UI explícitas.

---

## 4. Estructura estándar por manual (Secciones 1–12)

Cada manual 01–17 incluye:

1. Propósito del rol/módulo  
2. Precondiciones  
3. Credenciales de prueba  
4. Datos de prueba  
5. Casos de prueba (TC-ROL-NNN / E2E-NNN)  
6. Validaciones negativas  
7. Validación visual  
8. Permisos esperados  
9. Auditoría  
10. Criterio de aprobación del rol  
11. Qué aprendí  
12. Referencias y extensiones API  

**Cumplimiento:** 17/17 manuales operativos.

---

## 5. Conteo de casos por documento

| Documento | Prefijo | Casos |
|-----------|---------|-------|
| 01 Platform Administrator | TC-PA | 10 |
| 02 Tenant Administrator | TC-TA | 12 |
| 03 Tenant Security Administrator | TC-TSA | 12 |
| 04 Storage Administrator | TC-SA | 12 |
| 05 Notification Administrator | TC-NA | 12 |
| 06 Document Controller | TC-DC | 12 |
| 07 Quality Manager | TC-QM | 12 |
| 08 Technical Sheets | TC-TS | 12 |
| 09 Supplier Manager | TC-SM | 12 |
| 10 Auditor | TC-AU | 12 |
| 11 CAPA Manager | TC-CM | 12 |
| 12 Risk Manager | TC-RM | 12 |
| 13 Indicators Manager | TC-IM | 12 |
| 14 Reporting Manager | TC-RP | 12 |
| 15 Viewer | TC-VW | 14 |
| 16 Support Operator | TC-SO | 12 |
| 17 E2E Business Process | E2E | 12 |
| **TOTAL** | | **204** |

---

## 6. Cobertura por tipo de prueba

| Tipo | Casos aprox. | Ejemplos |
|------|--------------|----------|
| Positivo / happy path | ~90 | TC-DC-003, TC-SM-003 |
| Negativo / validación | ~35 | TC-PA-004, TC-DC-005 |
| Permisos / SoD | ~30 | TC-QM-002, TC-VW-011 |
| Seguridad | ~15 | TC-PA-005, TC-SO-005 |
| Extensión API | ~10 | TC-QM-DOC-003, TC-TS-004 |
| E2E multi-rol | 12 | E2E-001..012 |
| Auditoría | Transversal | Cada caso incluye expectativa |

---

## 7. Gaps conocidos y mitigaciones documentadas

| Gap | Mitigación en docs |
|-----|-------------------|
| UI parcial aprobación documentos | Swagger POST /decision (07) |
| UI parcial CAPA 5-Why/Ishikawa | customer_journey.ps1 (11, 17) |
| TECHNICALSHEET.CREATE no estándar | TC-TA-007 + API (08) |
| SMTP real no disponible local | PENDING THIRD-PARTY (05) |
| No UI break-glass Support | Documentado como limitación (16, 00 §11) |
| Azure Blob storage | PENDING THIRD-PARTY (04) |

---

## 8. Recomendaciones de mejora (próxima versión)

1. **Screenshots de referencia** embebidos o en `docs/manual-functional-testing/assets/` por pantalla clave.
2. **Matriz RBAC completa** rol × permiso × ruta como anexo 22.
3. **Automatización parcial** — exportar casos críticos a Playwright usando mismos selectores (`#module-action-form`).
4. **Versión en inglés** si el programa se expande a equipos globales.
5. **Sincronización CI** — validar que rutas en `app.js` no cambien sin actualizar manuales (script de lint docs).

---

## 9. Criterios de aceptación del paquete documental

| Criterio | Estado |
|----------|--------|
| Todos los roles 01–16 tienen manual | Cumplido |
| Mínimo 8 casos por rol | Cumplido (12–14) |
| Pasos con nombres exactos de botones | Cumplido |
| Sin contraseñas en texto | Cumplido |
| Orden de ejecución en roadmap | Cumplido (00) |
| Plantillas 18–20 utilizables | Cumplido |
| E2E y journey referenciados | Cumplido |
| Español | Cumplido |

---

## 10. Veredicto de calidad documental

| Dimensión | Calificación (1–5) | Notas |
|-----------|-------------------|-------|
| Completitud | 5 | 204 casos, 22 documentos |
| Precisión técnica | 5 | Alineado con app.js, RoleCatalog, scripts |
| Usabilidad para tester manual | 4 | Falta screenshots; pasos detallados |
| Trazabilidad auditoría | 5 | Expectativa por caso |
| Mantenibilidad | 4 | Generator script disponible |

**Veredicto global:** **APTO PARA EJECUCIÓN DEL PROGRAMA DE PRUEBAS** — sujeto a revisión humana y ejecución piloto de TC-PA-001.

---

## 11. Próximo paso recomendado

1. Ejecutar piloto: TC-PA-001 → TC-TA-001 → TC-DC-003 → TC-QM-DOC-003.  
2. Registrar en `18_FUNCTIONAL_TEST_RESULTS_TEMPLATE.md`.  
3. Actualizar este reporte con resultados del piloto (fecha, ejecutor, desviaciones).

---

## 12. Referencias

- Generador parcial: `scripts/generate_mft_manuals_full.py`
- Generador original: `scripts/generate_functional_test_manuals.py`
- Código UI: `src/Compliance360.Web/wwwroot/app.js`
- Roles: `src/Compliance360.Domain/Identity/RoleCatalog.cs`
- Provision: `scripts/e2e_provision.ps1`
- Journey: `scripts/customer_journey.ps1`
