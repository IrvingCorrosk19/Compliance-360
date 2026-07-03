# Compliance 360 — E2E Functional Certification Test Plan

> **FASE 0 — Plan obligatorio.** Este documento debe existir y ser aprobado antes de
> ejecutar cualquier prueba. Fuente de verdad RBAC: `RBAC_ROLE_CATALOG.md`,
> `RBAC_PERMISSION_CATALOG.md`, `RoleCatalog.cs`, `RbacCatalog.cs`.

Fecha: 2026-07-02 · Entorno: Development · Navegador: Chromium (headless con trazas/screenshots).

---

## 1. Objetivo de pruebas

Validar Compliance 360 de extremo a extremo como lo haría un usuario real en el
navegador, por rol, verificando login, navegación, creación/edición/consulta de datos,
enforcement de permisos (RBAC), segregación de funciones (SoD), aislamiento multi‑tenant,
auditoría, mensajes/errores amigables y logout. No se desarrollan nuevas funcionalidades.

## 2. Alcance

**Incluye:** UI web (SPA `app.js`) + API `/api/v1/*` + PostgreSQL local, para los 15 roles
listados. Validaciones transversales (menús, botones, consola, HTTP, loading, tenant isolation).

**Excluye (documentado como PENDING THIRD-PARTY CONFIGURATION, no FAIL):** SMTP real
(Gmail/M365/SendGrid/Mailgun/Resend/SES), almacenamiento en nube (Azure Blob/S3/MinIO externo),
SSO/OIDC/SAML, LDAP/AD, IA externa, pasarelas de pago y firma digital externa.

## 3. Roles a probar (mapeo al catálogo oficial)

| # | Rol solicitado | Rol en catálogo (`RoleCatalog.cs`) | Scope |
|---|----------------|-------------------------------------|-------|
| 1 | Platform Administrator / SuperAdmin | **Platform Administrator** | Platform |
| 2 | Tenant Administrator | **Tenant Administrator** | Tenant |
| 3 | Tenant Security Administrator | **Tenant Security Administrator** | Tenant |
| 4 | Document Controller | **Document Controller** | Tenant |
| 5 | Quality Manager | **Quality Manager** | Tenant |
| 6 | Auditor | **Auditor** | Tenant |
| 7 | Supplier Manager | **Supplier Manager** | Tenant |
| 8 | CAPA Manager | **CAPA Manager** | Tenant |
| 9 | Risk Manager | **Risk Manager** | Tenant |
| 10 | Indicators Manager | **Indicators Manager** | Tenant |
| 11 | Reporting Manager | **Reporting Manager** | Tenant |
| 12 | Storage Administrator | **Storage Administrator** | Tenant |
| 13 | Notification Administrator | **Notification Administrator** | Tenant |
| 14 | Viewer | **Viewer** | Tenant |
| 15 | Support Operator / Break Glass | **Support Operator** | Platform |

Todos existen 1:1 en el catálogo; no hay renombres pendientes. Roles adicionales del
catálogo no solicitados explícitamente (Platform Operations, Platform Security) se
verifican de forma indirecta vía enforcement de permisos.

## 4. Flujos por rol

Cada flujo sigue el guion de la solicitud (FLUJO 1..15). Estructura común por rol:
`Login → Navegación/menú → Acción(es) de negocio → Verificación de permisos (positiva y
negativa) → Auditoría → Logout`. Detalle operativo en cada reporte `docs/e2e/NN_*.md`.

Puntos SoD clave a demostrar en navegador **y** en base de datos:
- Document Controller **crea** documentos pero **no** ve/usa acción Aprobar.
- Quality Manager **aprueba** pero **no** crea datos de negocio.
- Auditor **no** puede gestionar/cerrar CAPA.
- CAPA Manager gestiona CAPA pero **no** aprueba cierre final.
- Storage Administrator ⟂ Notification Administrator (mutuamente excluyentes).
- Viewer solo lectura (sin crear/editar/eliminar/aprobar/configurar).
- Platform Administrator **no** opera datos de negocio del tenant (espera 403).

## 5. Datos de prueba

Empresa ficticia: **Alimentos Premium Panamá S.A.** · País: Panamá · Industria: Alimentos/BPM/HACCP
· Email: `calidad@alimentos-premium.test` · Tel: `+507 6000-0000` · RUC ficticio: `RUC-155999999-2-2026 DV 33`.

Usuarios por rol (patrón): `<rol>@alimentos-premium.test`, contraseña temporal
`Premium!2026`, con cambio forzado deshabilitado para pruebas. Credencial plataforma:
`admin@compliance360.local` / `OwnerStart!2026` / tenant `dc7c46ee-cb25-4ed5-b0b4-800788f7f626`.

## 6. Orden de ejecución

1. FASE 1 (entorno) → 2. FASE 2 (tenant + usuarios) → 3. Flujos 1–3 (plataforma/admin/seguridad)
→ 4. Flujos 4–11 (negocio) → 5. Flujos 12–15 (infraestructura/viewer/break‑glass) → 6. Reportes.

## 7. Precondiciones

- App levantada en `http://localhost:5272`, health `200`.
- Bootstrap ejecutado: catálogo de permisos, roles de plataforma, Platform Administrator.
- PostgreSQL 18 accesible (`localhost:5432`, schema `compliance360`).
- Tenant de prueba creado con los 13 roles auto‑provisionados.

## 8. Dependencias

- .NET 9 SDK, PostgreSQL 18 (`C:\Program Files\PostgreSQL\18\bin`), navegador Chromium.
- Cadena de conexión ya configurada (user secrets). No se vuelve a solicitar.

## 9. Criterios PASS/FAIL

**PASS (por rol):** flujo completo termina; datos guardados y consultables; permisos correctos
(positivos y negativos); auditoría registra acciones críticas; sin errores de consola ni HTTP
inesperados; logout OK; reporte generado.
**FAIL:** cualquier error crítico no corregido, permiso incorrecto, dato no persistido,
stack trace visible o ruptura de aislamiento de tenant.
**PENDING THIRD-PARTY:** funcionalidad que requiere credencial/servicio externo no configurado.

## 10. Casos que dependen de terceros

Envío real de correo, storage en nube, SSO/LDAP, IA externa, pago, firma digital. Se prueban
hasta el límite local (configuración/plantilla/simulación) y el envío real se marca PENDING.

## 11. Evidencia a capturar

En `artifacts/e2e/<rol>/`: screenshots por paso clave, trace/video si disponible, console logs,
network logs, request/response relevantes. Evidencia de BD vía scripts en `scripts/`.

## 12. Estrategia de corrección si algo falla

Detener flujo → capturar evidencia (screenshot/console/network) → diagnosticar causa raíz →
corregir código → `dotnet build` → `dotnet test` → reiniciar app → repetir el flujo completo del
rol desde el inicio → documentar causa raíz, archivo/línea, corrección, riesgo y resultado.

## 13. Riesgos

- Flujos de negocio profundos (CAPA 5‑Why/Ishikawa, heatmap de riesgo) pueden no estar
  completamente implementados en UI → se documentan como límite funcional, no como fallo RBAC.
- MFA/force‑password‑change puede bloquear login interactivo → se ajusta en datos de prueba.
- Servicios de terceros no configurados → PENDING, no FAIL.
- Estabilidad del proceso al levantar la app en background (visto exit abrupto previo).

## 14. Repetición tras corrección

Cada corrección obliga a: build verde + tests verdes + reinicio de app + re‑ejecución completa
del flujo del rol afectado (no parcial), actualizando su reporte y la evidencia asociada.

---

### Entregables

- 15 reportes: `docs/e2e/01_..._E2E_REPORT.md` … `docs/e2e/15_SUPPORT_OPERATOR_E2E_REPORT.md`.
- Reporte global: `docs/e2e/COMPLIANCE360_E2E_FUNCTIONAL_CERTIFICATION_REPORT.md`.
- Veredicto: `E2E FUNCTIONAL CERTIFICATION PASSED` o `PASSED WITH THIRD-PARTY PENDING ITEMS`.
  Nunca "Production Ready".
