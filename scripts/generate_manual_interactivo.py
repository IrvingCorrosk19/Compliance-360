#!/usr/bin/env python3
"""Generate Compliance 360 interactive manual HTML (self-contained, no CDN)."""
from __future__ import annotations

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "docs" / "manual-interactivo"
OUT_HTML = OUT_DIR / "COMPLIANCE360_MANUAL_INTERACTIVO_POR_ROLES_Y_FLUJOS.html"

# ---------------------------------------------------------------------------
# MANUAL_DATA — single source of truth (verified against app.js, RoleCatalog.cs, MFT)
# ---------------------------------------------------------------------------

def _step(n: int, text: str) -> dict:
    return {"n": n, "text": text}


def _tc(
    tid: str,
    name: str,
    *,
    objective: str,
    priority: str,
    kind: str,
    preconditions: str,
    route: str,
    menu: str,
    screen: str,
    steps: list[str],
    fields: list[dict] | None = None,
    expected: str,
    audit: str,
    handoff: str = "",
    ui_gap: str = "",
) -> dict:
    return {
        "id": tid,
        "name": name,
        "objective": objective,
        "priority": priority,
        "kind": kind,
        "preconditions": preconditions,
        "route": route,
        "menu": menu,
        "screen": screen,
        "steps": steps,
        "fields": fields or [],
        "expected": expected,
        "audit": audit,
        "handoff": handoff,
        "ui_gap": ui_gap,
    }


MANUAL_DATA: dict = {
    "meta": {
        "title": "Compliance 360 — Manual Interactivo por Roles y Flujos",
        "version": "1.0.0",
        "generated_by": "scripts/generate_manual_interactivo.py",
        "base_url": "http://localhost:5272",
        "storage_key": "c360.manual.interactivo.v1",
        "language": "es",
    },
    "company": {
        "name": "Alimentos Premium Panamá S.A.",
        "legal_name": "Alimentos Premium Panama, S.A.",
        "commercial_name": "Alimentos Premium",
        "industry": "Manufactura de alimentos",
        "country": "Panamá",
        "country_code": "PA",
        "currency": "USD",
        "tax_id": "RUC-155999999-2-2026",
        "standards": ["ISO 9001", "ISO 22000", "HACCP", "BPM"],
        "slug": "alimentos-premium-pa",
    },
    "tenants": {
        "platform": {
            "id": "dc7c46ee-cb25-4ed5-b0b4-800788f7f626",
            "label": "Tenant plataforma",
        },
        "operations": {
            "id": "ddcaf211-afe0-44a0-9c90-4fbda8fc4871",
            "label": "Alimentos Premium Panamá S.A.",
        },
    },
    "login_v2": {
        "steps": [
            {"field": "Correo electronico", "button": "Siguiente"},
            {"field": "Organizacion (si aplica)", "button": "Continuar"},
            {"field": "Contrasena", "button": "Iniciar sesion"},
        ],
        "note": "Texto exacto desde app.js. Contraseñas en e2e/testdata.json — no incluidas en este manual.",
    },
    "ui_labels": {
        "create_button": "Crear registro real",
        "readonly_heading": "Modo solo lectura",
        "logout": "Salir",
        "modes": {
            "learn": "Aprendizaje",
            "execute": "Ejecución",
            "test": "Prueba",
            "present": "Presentación",
        },
        "status": {
            "PASS": "PASS",
            "FAIL": "FAIL",
            "PENDING": "PENDING",
            "NOT_EXECUTED": "NOT_EXECUTED",
        },
    },
    "navigation": [
        {
            "group": "Command Center",
            "items": [
                ["dashboard", "Executive Dashboard"],
                ["compliance", "Compliance Dashboard"],
                ["reports", "Report Center"],
                ["audit-trail", "Audit Trail"],
            ],
        },
        {
            "group": "Operations",
            "items": [
                ["superadmin-platform", "SuperAdmin Platform"],
                ["documents", "Document Management"],
                ["technical-sheets", "Technical Sheets"],
                ["suppliers", "Supplier Management"],
                ["audits", "Audit Management"],
                ["capa", "CAPA"],
                ["risks", "Risk Management"],
                ["indicators", "Quality Indicators"],
            ],
        },
        {
            "group": "Enterprise",
            "items": [
                ["superadmin-platform", "SuperAdmin Platform"],
                ["tenant-administration", "Tenant Administration"],
                ["template-builder", "Template Builder"],
                ["regulatory", "Regulatory Management"],
                ["training", "Training Management"],
                ["supplier-portal", "Supplier Portal"],
                ["customer-portal", "Customer Portal"],
                ["security", "Security"],
                ["configuration", "Configuration"],
            ],
        },
    ],
    "master_sequence": [
        {"n": 1, "title": "Preparación entorno", "detail": "dotnet run, PostgreSQL, e2e_provision.ps1, carpeta artifacts/"},
        {"n": 2, "title": "Platform Admin — salud plataforma", "detail": "Login v2 → #/superadmin-platform, métricas visibles"},
        {"n": 3, "title": "Crear tenant (opcional)", "detail": "Tab Tenants → Crear tenant con slug único"},
        {"n": 4, "title": "Activar tenant", "detail": "Tenant Administration → Estado → Activar"},
        {"n": 5, "title": "Tenant Admin — información general", "detail": "#/tenant-administration → Guardar informacion general"},
        {"n": 6, "title": "Branding", "detail": "Tab Branding → Guardar branding"},
        {"n": 7, "title": "Tenant Security — políticas", "detail": "#/security o TAC Seguridad → Guardar seguridad"},
        {"n": 8, "title": "Usuarios especialistas", "detail": "TAC Usuarios: security@, storage@, notifications@, doccontrol@"},
        {"n": 9, "title": "RBAC — permisos", "detail": "Roles & Permisos; TECHNICALSHEET.CREATE si aplica fichas"},
        {"n": 10, "title": "Storage provider local", "detail": "#/configuration → Crear Storage Local → Probar primer Storage Provider"},
        {"n": 11, "title": "Notification provider", "detail": "#/configuration → Crear Email SMTP (real = PENDING third-party)"},
        {"n": 12, "title": "Documentos", "detail": "doccontrol@ → #/documents → Crear registro real BPM-LIM-001"},
        {"n": 13, "title": "Fichas técnicas", "detail": "Grant CREATE → #/technical-sheets → TS-MFT-001"},
        {"n": 14, "title": "Proveedores", "detail": "supplier@ → #/suppliers → E2E-SUP-001"},
        {"n": 15, "title": "Auditorías", "detail": "auditor@ → #/audits → E2E-AUD-001"},
        {"n": 16, "title": "CAPA", "detail": "capa@ → #/capa → CAPA-2026-014"},
        {"n": 17, "title": "Riesgos", "detail": "risk@ → #/risks → RSK-CC-001"},
        {"n": 18, "title": "Indicadores", "detail": "indicators@ → #/indicators → KPI-NC-RATE"},
        {"n": 19, "title": "Reportes", "detail": "reporting@ → #/reports → Seed standard reports → Ejecutar"},
        {"n": 20, "title": "Viewer read-only", "detail": "viewer@ → Modo solo lectura en módulos operativos"},
        {"n": 21, "title": "Support Operator", "detail": "support@ → JWT/API; sin UI break-glass dedicada"},
        {"n": 22, "title": "Audit trail + cierre", "detail": "#/audit-trail → export JSON resultados manual"},
    ],
    "cross_flows": [
        {
            "id": "CF-01",
            "name": "Onboarding cliente",
            "roles": ["Platform Administrator", "Tenant Administrator"],
            "summary": "Plataforma crea tenant → Tenant Admin configura empresa, usuarios y activa.",
            "steps": [
                "Platform Admin: login v2 admin@compliance360.local → #/superadmin-platform → Crear tenant",
                "Anotar Tenant ID; Salir",
                "Tenant Admin: login tenantadmin@ → #/tenant-administration → Guardar informacion general",
                "Tab Usuarios → crear especialistas; Tab Estado → Activar",
            ],
            "audit": "TenantCreated → UserCreated → TenantStatusChanged",
        },
        {
            "id": "CF-02",
            "name": "Ciclo documental",
            "roles": ["Document Controller", "Quality Manager"],
            "summary": "Crear documento → aprobar (API QM) → verificar Approved.",
            "steps": [
                "doccontrol@: #/documents → Crear registro real BPM-LIM-001",
                "Salir → quality@: #/documents → Modo solo lectura para create",
                "API POST /documents/{id}/decision Approved (Swagger)",
                "Verificar listado + #/audit-trail DocumentApproved",
            ],
            "audit": "DocumentCreated → DocumentApproved",
            "ui_gap": "Quality Manager aprueba documentos vía API/Swagger, no botón UI dedicado.",
        },
        {
            "id": "CF-03",
            "name": "Fichas técnicas",
            "roles": ["Tenant Administrator", "Quality Manager"],
            "summary": "Grant TECHNICALSHEET.CREATE → crear ficha → aprobar QM vía API.",
            "steps": [
                "tenantadmin@: TAC Roles → otorgar TECHNICALSHEET.CREATE",
                "Re-login → #/technical-sheets → TS-MFT-001 → Crear registro real",
                "quality@: aprobar vía API decision endpoint",
            ],
            "audit": "RbacPermissionGranted → TechnicalSheetCreated → TechnicalSheetApproved",
            "ui_gap": "Ningún rol tenant estándar incluye TECHNICALSHEET.CREATE.",
        },
        {
            "id": "CF-04",
            "name": "Proveedor y NC",
            "roles": ["Supplier Manager", "CAPA Manager"],
            "summary": "Homologar proveedor → registrar NC → CAPA vinculada.",
            "steps": [
                "supplier@: #/suppliers → E2E-SUP-001 → Crear registro real",
                "Evaluación/homologación según UI disponible",
                "capa@: #/capa → CAPA-2026-014 relacionada al proveedor",
            ],
            "audit": "SupplierCreated → CapaCreated",
        },
        {
            "id": "CF-05",
            "name": "Auditoría → CAPA → cierre QM",
            "roles": ["Auditor", "CAPA Manager", "Quality Manager"],
            "summary": "Auditoría con hallazgo → CAPA PendingApproval → cierre QM API.",
            "steps": [
                "auditor@: #/audits → E2E-AUD-001",
                "capa@: #/capa → avanzar a PendingApproval",
                "quality@: cerrar/aprobar vía API (manual 07 TC-QM-CAPA-*)",
            ],
            "audit": "AuditCreated → CapaCreated → CapaApproved/Closed",
            "ui_gap": "Ciclo audit program→close parcial en UI; customer_journey.ps1 completa API.",
        },
        {
            "id": "CF-06",
            "name": "Riesgo → aprobación QM",
            "roles": ["Risk Manager", "Quality Manager"],
            "summary": "Registrar riesgo → submit → QM approve/close API.",
            "steps": [
                "risk@: #/risks → RSK-CC-001 → Crear registro real",
                "Submit a aprobación (UI o API)",
                "quality@: POST decision/close riesgo",
            ],
            "audit": "RiskCreated → RiskApproved → RiskClosed",
            "ui_gap": "Quality Manager no crea riesgos; aprueba vía API.",
        },
        {
            "id": "CF-07",
            "name": "Indicador → dashboard",
            "roles": ["Indicators Manager", "Reporting Manager"],
            "summary": "Medición KPI → visible en dashboards/reportes.",
            "steps": [
                "indicators@: #/indicators → KPI-NC-RATE",
                "Registrar medición según UI",
                "reporting@: #/reports → ejecutar reporte con indicadores",
                "#/dashboard o #/compliance verificar métricas",
            ],
            "audit": "IndicatorCreated → ReportExecuted",
        },
        {
            "id": "CF-08",
            "name": "Reporte ejecutivo",
            "roles": ["Reporting Manager", "Quality Manager"],
            "summary": "Seed reports → ejecutar → Exportar via reportes.",
            "steps": [
                "reporting@: #/reports → Seed standard reports",
                "Ejecutar reporte ejecutivo/compliance",
                "Exportar via reportes o export desde Report Center",
            ],
            "audit": "ReportSeeded → ReportExecuted → ReportExported",
        },
    ],
    "negative_validations": [
        {"id": "NEG-401", "title": "Login credenciales inválidas", "detail": "Contraseña incorrecta → mensaje genérico anti-enumeración; HTTP 401."},
        {"id": "NEG-403", "title": "Platform Admin cross-tenant", "detail": "GET /api/v1/tenants/{opsTenant}/documents sin SUPPORT → 403."},
        {"id": "NEG-403-V", "title": "Viewer POST create", "detail": "viewer@ DevTools POST document → 403 AuthorizationDenied."},
        {"id": "NEG-400-SLUG", "title": "Slug duplicado", "detail": "Crear tenant con slug existente → HTTP 400, no 500."},
        {"id": "NEG-400-TAX", "title": "Tax ID duplicado", "detail": "RUC repetido → validación clara."},
        {"id": "NEG-LOCKOUT", "title": "Lockout tenant", "detail": "TSA lockoutMaxFailedAttempts=3 → 3 fallos bloquean login."},
        {"id": "NEG-SOD-DC", "title": "Document Controller approve", "detail": "doccontrol@ sin DOCUMENT.APPROVE → no puede aprobar."},
        {"id": "NEG-TS-CREATE", "title": "Sin TECHNICALSHEET.CREATE", "detail": "doccontrol@ #/technical-sheets → Modo solo lectura."},
    ],
    "third_party_pending": [
        {"id": "TP-SMTP", "title": "SMTP real", "detail": "Crear Email SMTP con credenciales reales → PENDING hasta servidor accesible."},
        {"id": "TP-BLOB", "title": "Azure Blob Storage", "detail": "Storage provider cloud → PENDING sin connection string."},
        {"id": "TP-OIDC", "title": "SSO OIDC", "detail": "Tab SSO en TAC → PENDING sin IdP corporativo."},
        {"id": "TP-WEBHOOK", "title": "Webhooks externos", "detail": "Endpoint receptor real → PENDING en entorno local."},
        {"id": "TP-MFA-HW", "title": "MFA hardware", "detail": "TOTP/app real → opcional; tenant test con MFA desactivado vía SQL."},
    ],
    "ui_gaps": [
        "Quality Manager: aprobaciones documento/CAPA/riesgo/ficha vía API Swagger, no botones UI completos.",
        "Support Operator: permiso PLATFORM.SUPPORT.ACCESS; no existe pantalla break-glass dedicada.",
        "TECHNICALSHEET.CREATE: no en roles tenant estándar; Tenant Admin debe otorgar en RBAC.",
        "SMTP producción: configuración third-party → marcar PENDING, no FAIL.",
        "CAPA 5-Why / Ishikawa: sin UI dedicada; customer_journey.ps1 cubre API.",
        "Upload adjuntos documento: parcial en UI; scripts API para evidencia.",
    ],
}


def build_roles() -> list[dict]:
    """Build 17 role sections with identity + 4-6 verified test cases each."""
    ops = MANUAL_DATA["tenants"]["operations"]["id"]
    plat = MANUAL_DATA["tenants"]["platform"]["id"]
    base = MANUAL_DATA["meta"]["base_url"]

    def role(
        key: str,
        name: str,
        scope: str,
        email: str | None,
        tenant_id: str,
        purpose: str,
        permissions: list[str],
        landing: str,
        menu_focus: str,
        handoffs: list[str],
        cases: list[dict],
        notes: str = "",
    ) -> dict:
        return {
            "key": key,
            "name": name,
            "scope": scope,
            "email": email,
            "tenant_id": tenant_id,
            "purpose": purpose,
            "permissions": permissions,
            "landing_route": landing,
            "menu_focus": menu_focus,
            "handoffs": handoffs,
            "notes": notes,
            "test_cases": cases,
        }

    return [
        role(
            "platform-administrator",
            "Platform Administrator",
            "platform",
            "admin@compliance360.local",
            plat,
            "Administra la plataforma SaaS: tenants, licencias, observabilidad. No opera datos de negocio del tenant cliente.",
            ["PLATFORM.TENANT.*", "PLATFORM.AUDIT.EXPORT", "PLATFORM.CONFIGURATION.MANAGE"],
            "#/superadmin-platform",
            "Operations/Enterprise → SuperAdmin Platform",
            ["Tenant Administrator (entrega tenant)", "Support Operator"],
            [
                _tc("TC-PA-001", "Login Platform Administrator", objective="Validar acceso shell plataforma", priority="Alta", kind="Positivo",
                    preconditions="Bootstrap OK", route=f"{base}/", menu="Login Enterprise", screen="Login v2",
                    steps=["Abrir URL base", "Correo admin@compliance360.local → Siguiente", "Continuar (tenant plataforma)", "Contraseña desde appsettings → Iniciar sesion", "Verificar toast Sesion iniciada correctamente"],
                    expected="Sesión activa; menú orientado SuperAdmin Platform", audit="AuthenticationSuccess", handoff="—"),
                _tc("TC-PA-002", "SuperAdmin Platform Center", objective="Centro gobierno global", priority="Alta", kind="Positivo",
                    preconditions="TC-PA-001 PASS", route="#/superadmin-platform", menu="SuperAdmin Platform", screen="SuperAdmin Platform Center",
                    steps=["Clic SuperAdmin Platform", "Esperar loader", "Confirmar h1 SuperAdmin Platform Center", "Ver métricas Tenants, Usuarios, Documentos"],
                    expected="Pantalla sin 5xx; métricas visibles", audit="Viewed platform center"),
                _tc("TC-PA-003", "Crear tenant", objective="Onboarding cliente", priority="Crítica", kind="Positivo",
                    preconditions="TC-PA-002 PASS", route="#/superadmin-platform", menu="Tab Tenants", screen="Formulario Crear Tenant",
                    steps=["Tab Tenants", "Tenant Name, Slug único, RUC, PA, USD", "Admin Email owner@cliente-manual-test.test", "Crear tenant", "Verificar fila en fleet"],
                    fields=[{"name": "Slug", "value": "cliente-manual-test-{fecha}"}, {"name": "Pais", "value": "PA"}],
                    expected="Tenant creado; admin inicial provisionado", audit="TenantCreated; UserCreated", handoff="Tenant Administrator"),
                _tc("TC-PA-004", "Aislamiento cross-tenant 403", objective="Sin acceso datos negocio sin soporte", priority="Alta", kind="Seguridad",
                    preconditions=f"Tenant negocio {ops}", route="#/superadmin-platform", menu="—", screen="DevTools Network",
                    steps=[f"GET /api/v1/tenants/{ops}/documents", "Observar HTTP 403"],
                    expected="403 Forbidden", audit="AuthorizationDenied"),
                _tc("TC-PA-005", "Exportar auditoría global CSV", objective="Evidencia gobierno", priority="Media", kind="Positivo",
                    preconditions="TC-PA-002 PASS", route="#/superadmin-platform", menu="Barra platform-command", screen="Export",
                    steps=["Exportar auditoria global CSV", "Confirmar descarga"],
                    expected="CSV o toast éxito", audit="AuditExported"),
                _tc("TC-PA-006", "Logout Salir", objective="Cierre seguro", priority="Media", kind="Positivo",
                    preconditions="Sesión activa", route="cualquiera", menu="Topbar", screen="Salir",
                    steps=["Presionar Salir", "Confirmar pantalla login"],
                    expected="Sesión terminada", audit="Logout"),
            ],
        ),
        role(
            "platform-operations",
            "Platform Operations",
            "platform",
            None,
            plat,
            "Operaciones día a día: lifecycle tenant, providers, módulos, licencias. Sin PLATFORM.TENANT.DELETE.",
            ["PLATFORM.TENANT.READ/CREATE/UPDATE", "PLATFORM.PROVIDER.MANAGE", "PLATFORM.LICENSE.MANAGE"],
            "#/superadmin-platform",
            "SuperAdmin Platform (alcance operativo)",
            ["Platform Administrator", "Tenant Administrator"],
            [
                _tc("TC-PO-001", "Verificar rol en catálogo", objective="Rol existe en RoleCatalog.cs", priority="Alta", kind="Referencia",
                    preconditions="—", route="—", menu="—", screen="RoleCatalog.cs",
                    steps=["Confirmar Platform Operations en RoleCatalog", "15 permisos operativos"],
                    expected="Definición code-first verificada", audit="—",
                    ui_gap="Sin usuario E2E por defecto; asignar rol en tenant plataforma."),
                _tc("TC-PO-002", "SuperAdmin lectura tenants", objective="Listar fleet sin delete", priority="Alta", kind="Positivo",
                    preconditions="Usuario con rol Platform Operations provisionado", route="#/superadmin-platform", menu="Tab Tenants", screen="Tenant fleet",
                    steps=["Login → #/superadmin-platform", "Tab Tenants", "Ver listado"],
                    expected="Tenants visibles; sin delete destructivo", audit="PlatformTenantRead"),
                _tc("TC-PO-003", "Gestionar providers", objective="Providers plataforma", priority="Media", kind="Positivo",
                    preconditions="TC-PO-002", route="#/superadmin-platform", menu="Tab Providers", screen="Providers",
                    steps=["Tab Providers", "Revisar configuración"],
                    expected="Providers consultables/gestionables según permiso", audit="PlatformProviderManage"),
                _tc("TC-PO-004", "Observabilidad", objective="Métricas plataforma", priority="Media", kind="Positivo",
                    preconditions="TC-PO-002", route="#/superadmin-platform", menu="Tab Observability", screen="Observability",
                    steps=["Tab Observability", "Ver métricas"],
                    expected="Datos observabilidad visibles", audit="PlatformObservabilityRead"),
                _tc("TC-PO-005", "Sin módulos negocio tenant", objective="SoD plataforma", priority="Alta", kind="Seguridad",
                    preconditions="Sesión Platform Operations", route="#/documents", menu="Sidebar", screen="Navegación",
                    steps=["Verificar sidebar", "documents/capa no accesibles habitualmente"],
                    expected="Sin acceso operativo tenant negocio en menú", audit="—"),
            ],
            notes="Sin usuario demo E2E; reutilizar casos TC-PA-002 con alcance reducido.",
        ),
        role(
            "platform-security",
            "Platform Security",
            "platform",
            None,
            plat,
            "Seguridad global y auditoría plataforma. PLATFORM.SECURITY.MANAGE, AUDIT.EXPORT.",
            ["PLATFORM.SECURITY.MANAGE", "PLATFORM.AUDIT.READ", "PLATFORM.AUDIT.EXPORT"],
            "#/superadmin-platform",
            "SuperAdmin Platform → Seguridad Global / Auditoría Global",
            ["Platform Administrator", "Support Operator"],
            [
                _tc("TC-PS-001", "Rol en catálogo", objective="Verificar RoleCatalog", priority="Alta", kind="Referencia",
                    preconditions="—", route="—", menu="—", screen="RoleCatalog.cs",
                    steps=["Platform Security: 6 permisos seguridad/audit"],
                    expected="Definición verificada", audit="—",
                    ui_gap="Sin usuario E2E por defecto."),
                _tc("TC-PS-002", "Seguridad Global tab", objective="Config seguridad plataforma", priority="Alta", kind="Positivo",
                    preconditions="Usuario Platform Security", route="#/superadmin-platform", menu="Tab Seguridad Global", screen="Seguridad Global",
                    steps=["Login → SuperAdmin", "Tab Seguridad Global", "Revisar políticas"],
                    expected="Pantalla accesible", audit="PlatformSecurityManage"),
                _tc("TC-PS-003", "Auditoría Global", objective="Trail plataforma", priority="Alta", kind="Positivo",
                    preconditions="TC-PS-002", route="#/superadmin-platform", menu="Tab Auditoria Global", screen="Auditoría",
                    steps=["Tab Auditoria Global", "Filtrar eventos"],
                    expected="Eventos auditables visibles", audit="PlatformAuditRead"),
                _tc("TC-PS-004", "Export CSV auditoría", objective="Export compliance", priority="Media", kind="Positivo",
                    preconditions="TC-PS-003", route="#/superadmin-platform", menu="Export", screen="CSV",
                    steps=["Exportar auditoria global CSV"],
                    expected="Export exitoso", audit="PlatformAuditExport"),
                _tc("TC-PS-005", "Sin crear tenant", objective="Alcance restringido vs PA", priority="Media", kind="Negativo",
                    preconditions="Sesión Platform Security", route="#/superadmin-platform", menu="Tenants", screen="Crear tenant",
                    steps=["Verificar ausencia Crear tenant o 403"],
                    expected="No lifecycle tenant completo", audit="AuthorizationDenied"),
            ],
            notes="Sin usuario demo; derivado de permisos catálogo.",
        ),
        role(
            "support-operator",
            "Support Operator",
            "platform",
            "support@compliance360.local",
            plat,
            "Break-glass con PLATFORM.SUPPORT.ACCESS. Acceso auditado tenant vía JWT/API.",
            ["PLATFORM.SUPPORT.ACCESS", "PLATFORM.AUDIT.READ"],
            "#/superadmin-platform",
            "SuperAdmin Platform (vista restringida)",
            ["Platform Administrator", "Tenant operativo"],
            [
                _tc("TC-SO-001", "Login Support Operator", objective="Sesión tenant plataforma", priority="Alta", kind="Positivo",
                    preconditions="e2e_provision_support.sql", route=f"{base}/", menu="Login", screen="Enterprise Login",
                    steps=["support@compliance360.local → Siguiente → Continuar → Iniciar sesion", f"Tenant {plat}"],
                    expected="Sesión plataforma, no Alimentos Premium", audit="AuthenticationSuccess"),
                _tc("TC-SO-002", "Menú limitado", objective="Sin UI break-glass", priority="Alta", kind="Referencia",
                    preconditions="TC-SO-001", route="#/superadmin-platform", menu="Sidebar", screen="Navegación",
                    steps=["Revisar sidebar", "SuperAdmin accesible", "Sin pantalla break-glass dedicada"],
                    expected="Vista gobierno plataforma únicamente", audit="—",
                    ui_gap="NO existe UI break-glass dedicada."),
                _tc("TC-SO-003", "SuperAdmin Center", objective="Métricas globales", priority="Alta", kind="Positivo",
                    preconditions="TC-SO-001", route="#/superadmin-platform", menu="SuperAdmin Platform", screen="Center",
                    steps=["Navegar #/superadmin-platform", "Ver métricas"],
                    expected="Carga sin 5xx", audit="Viewed platform center"),
                _tc("TC-SO-004", "Sin Crear tenant", objective="Permiso reducido", priority="Media", kind="Negativo",
                    preconditions="TC-SO-003", route="#/superadmin-platform", menu="Tenants", screen="Crear tenant",
                    steps=["Intentar Crear tenant", "Documentar ausencia o 403"],
                    expected="Alcance menor que Platform Admin", audit="AuthorizationDenied"),
                _tc("TC-SO-005", "API soporte tenant negocio", objective="PLATFORM.SUPPORT.ACCESS", priority="Crítica", kind="API",
                    preconditions=f"Tenant {ops}", route="API", menu="DevTools", screen="Swagger/Network",
                    steps=[f"GET /api/v1/tenants/{ops}/documents con token support@", "Comparar vs PA 403"],
                    expected="200 si break-glass activo en backend", audit="SupportAccessGranted",
                    ui_gap="Validación principal vía API, no UI."),
                _tc("TC-SO-006", "Audit trail soporte", objective="Eventos auditados", priority="Media", kind="Positivo",
                    preconditions="TC-SO-005", route="#/audit-trail", menu="Audit Trail", screen="Eventos",
                    steps=["#/audit-trail", "Buscar eventos soporte"],
                    expected="Acciones support auditadas", audit="PlatformAuditRead"),
            ],
            notes="8/12 casos dependen API/Swagger; sin UI break-glass.",
        ),
        role(
            "tenant-administrator",
            "Tenant Administrator",
            "tenant",
            "tenantadmin@alimentos-premium.test",
            ops,
            "Administra tenant: perfil, usuarios, roles, settings. No opera datos negocio por defecto.",
            ["TENANT.USERS", "TENANT.ROLES", "RBAC.MANAGE", "TENANT.UPDATE"],
            "#/tenant-administration",
            "Enterprise → Tenant Administration",
            ["Platform Administrator", "Todos los roles tenant"],
            [
                _tc("TC-TA-001", "Login Tenant Admin", objective="Acceso TAC", priority="Alta", kind="Positivo",
                    preconditions="e2e_provision.ps1", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["tenantadmin@ → Siguiente", "Alimentos Premium → Continuar", "Iniciar sesion"],
                    expected="TAC accesible", audit="AuthenticationSuccess"),
                _tc("TC-TA-002", "Información general", objective="Datos empresa", priority="Alta", kind="Positivo",
                    preconditions="TC-TA-001", route="#/tenant-administration", menu="Tab Informacion General", screen="TAC",
                    steps=["Tab Informacion General", "Verificar Alimentos Premium Panamá S.A.", "Guardar informacion general"],
                    fields=[{"name": "Razon Social", "value": "Alimentos Premium Panama, S.A."}, {"name": "Pais", "value": "PA"}],
                    expected="Datos persistidos", audit="TenantUpdated"),
                _tc("TC-TA-003", "Crear usuario", objective="Invitar especialista", priority="Alta", kind="Positivo",
                    preconditions="TC-TA-002", route="#/tenant-administration", menu="Tab Usuarios", screen="Usuarios",
                    steps=["Tab Usuarios", "Crear / Invitar usuario", "Asignar rol Document Controller"],
                    expected="Usuario creado", audit="UserCreated", handoff="Document Controller"),
                _tc("TC-TA-004", "RBAC grant TECHNICALSHEET.CREATE", objective="Habilitar fichas técnicas", priority="Crítica", kind="Positivo",
                    preconditions="TC-TA-003", route="#/tenant-administration", menu="Roles & Permisos", screen="RBAC",
                    steps=["Editar rol", "Agregar TECHNICALSHEET.CREATE", "Guardar", "Re-login usuario afectado"],
                    expected="Permiso otorgado", audit="RbacPermissionGranted", handoff="Technical Sheets",
                    ui_gap="CREATE no está en roles estándar."),
                _tc("TC-TA-005", "Activar tenant", objective="Estado Active", priority="Alta", kind="Positivo",
                    preconditions="Config completa", route="#/tenant-administration", menu="Tab Estado", screen="Lifecycle",
                    steps=["Tab Estado", "Activar"],
                    expected="Tenant Active", audit="TenantStatusChanged"),
                _tc("TC-TA-006", "Logout Salir", objective="Cierre sesión", priority="Baja", kind="Positivo",
                    preconditions="Sesión activa", route="—", menu="Topbar", screen="Salir",
                    steps=["Salir"], expected="Login screen", audit="Logout"),
            ],
        ),
        role(
            "tenant-security-administrator",
            "Tenant Security Administrator",
            "tenant",
            "security@alimentos-premium.test",
            ops,
            "Seguridad tenant: MFA, SSO, domains, webhooks, API keys.",
            ["TENANT.SECURITY", "TENANT.DOMAINS", "TENANT.SSO", "TENANT.APIKEYS"],
            "#/security",
            "Enterprise → Security",
            ["Tenant Administrator", "Viewer (lockout tests)"],
            [
                _tc("TC-TSA-001", "Login TSA", objective="Acceso security workspace", priority="Alta", kind="Positivo",
                    preconditions="Usuario provisionado", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["security@ → Siguiente → Continuar → Iniciar sesion"],
                    expected="#/security accesible", audit="AuthenticationSuccess"),
                _tc("TC-TSA-002", "Políticas lockout", objective="Guardar seguridad", priority="Alta", kind="Positivo",
                    preconditions="TC-TSA-001", route="#/tenant-administration", menu="Tab Seguridad", screen="Seguridad",
                    steps=["lockoutMaxFailedAttempts=3", "Guardar seguridad"],
                    expected="Política persistida", audit="TenantSecurityUpdated"),
                _tc("TC-TSA-003", "Workspace Security", objective="Enterprise security", priority="Media", kind="Positivo",
                    preconditions="TC-TSA-001", route="#/security", menu="Enterprise → Security", screen="Security workspace",
                    steps=["#/security", "Revisar items enterprise"],
                    expected="Workspace carga", audit="—"),
                _tc("TC-TSA-004", "Dominios", objective="Tenant domains", priority="Media", kind="Positivo",
                    preconditions="TC-TSA-001", route="#/tenant-administration", menu="Tab Dominios", screen="Dominios",
                    steps=["Agregar dominio alimentos-premium.test", "Guardar dominio"],
                    expected="Dominio registrado", audit="TenantDomainCreated"),
                _tc("TC-TSA-005", "Lockout E2E", objective="Validación negativa login", priority="Alta", kind="Seguridad",
                    preconditions="TC-TSA-002", route=f"{base}/", menu="Login", screen="Lockout",
                    steps=["Salir", "viewer@ 3 contraseñas incorrectas", "Verificar lockout"],
                    expected="Cuenta bloqueada temporalmente", audit="AccountLocked", handoff="Restaurar lockout=5"),
            ],
        ),
        role(
            "document-controller",
            "Document Controller",
            "tenant",
            "doccontrol@alimentos-premium.test",
            ops,
            "Crea y mantiene documentos controlados. SoD: no aprueba.",
            ["DOCUMENT.CREATE", "DOCUMENT.UPDATE", "DOCUMENT.READ"],
            "#/documents",
            "Operations → Document Management",
            ["Quality Manager (aprobación API)"],
            [
                _tc("TC-DC-001", "Login Document Controller", objective="Acceso módulo", priority="Alta", kind="Positivo",
                    preconditions="Infra lista", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["doccontrol@ → login v2 completo"],
                    expected="#/documents visible", audit="AuthenticationSuccess"),
                _tc("TC-DC-002", "Listado documentos", objective="Consulta documental", priority="Alta", kind="Positivo",
                    preconditions="TC-DC-001", route="#/documents", menu="Document Management", screen="Listado",
                    steps=["#/documents", "Esperar tabla", "Buscar en search-box"],
                    expected="Listado carga", audit="DocumentRead"),
                _tc("TC-DC-003", "Crear documento", objective="Registro real", priority="Crítica", kind="Positivo",
                    preconditions="TC-DC-002", route="#/documents", menu="Action Center", screen="#module-action-form",
                    steps=["Formulario name, code, description", "BPM-LIM-001", "Crear registro real", "Verificar listado"],
                    fields=[{"name": "code", "value": "BPM-LIM-001"}, {"name": "name", "value": "Procedimiento Limpieza MFT"}],
                    expected="Documento Draft creado", audit="DocumentCreated", handoff="Quality Manager"),
                _tc("TC-DC-004", "Sin aprobar", objective="SoD", priority="Alta", kind="Negativo",
                    preconditions="TC-DC-003", route="#/documents", menu="—", screen="Acciones",
                    steps=["Verificar ausencia botón aprobar en UI"],
                    expected="Sin DOCUMENT.APPROVE en UI", audit="—"),
                _tc("TC-DC-005", "Technical sheets read-only", objective="Sin TS CREATE", priority="Media", kind="Negativo",
                    preconditions="TC-DC-001", route="#/technical-sheets", menu="Technical Sheets", screen="Modo solo lectura",
                    steps=["#/technical-sheets", "Ver Modo solo lectura", "Sin Crear registro real"],
                    expected="Solo lectura", audit="—", ui_gap="TECHNICALSHEET.CREATE no en rol estándar."),
            ],
        ),
        role(
            "quality-manager",
            "Quality Manager",
            "tenant",
            "quality@alimentos-premium.test",
            ops,
            "Aprobador/coordinador calidad. Aprueba vía API. No crea en UI (SoD).",
            ["DOCUMENT.APPROVE", "CAPA.APPROVE", "RISK.APPROVE", "TECHNICALSHEET.APPROVE"],
            "#/documents",
            "Operations (lectura) + API Swagger",
            ["Document Controller", "CAPA Manager", "Risk Manager"],
            [
                _tc("TC-QM-001", "Login Quality Manager", objective="Sesión QM", priority="Alta", kind="Positivo",
                    preconditions="Usuarios provisionados", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["quality@ → login v2"],
                    expected="Módulos lectura visibles", audit="AuthenticationSuccess"),
                _tc("TC-QM-002", "Documents Modo solo lectura", objective="SoD create", priority="Alta", kind="Positivo",
                    preconditions="TC-QM-001", route="#/documents", menu="Document Management", screen="Action Center",
                    steps=["#/documents", "Ver Modo solo lectura", "Sin Crear registro real"],
                    expected="UI create bloqueada", audit="—"),
                _tc("TC-QM-DOC-003", "Aprobar documento API", objective="Aprobación documental", priority="Crítica", kind="API",
                    preconditions="TC-DC-003 documentId", route="Swagger", menu="API", screen="POST .../decision",
                    steps=["Bearer token quality@", "POST /documents/{id}/decision Approved", "Recargar #/documents"],
                    fields=[{"name": "decision", "value": "Approved"}, {"name": "comments", "value": "Aprobado MFT QM"}],
                    expected="Estado Approved", audit="DocumentApproved",
                    ui_gap="QM approve vía API, no botón UI dedicado.", handoff="Viewer/Reporting"),
                _tc("TC-QM-CAPA-004", "Cerrar CAPA API", objective="Cierre CAPA", priority="Crítica", kind="API",
                    preconditions="CAPA PendingApproval", route="Swagger", menu="API", screen="CAPA decision",
                    steps=["Token QM", "Aprobar/cerrar CAPA vía API", "Verificar #/capa Closed"],
                    expected="CAPA cerrada", audit="CapaApproved", ui_gap="Cierre QM vía API."),
                _tc("TC-QM-RISK-005", "Aprobar riesgo API", objective="Workflow riesgo", priority="Alta", kind="API",
                    preconditions="Riesgo en workflow", route="Swagger", menu="API", screen="Risk decision",
                    steps=["POST approve/close riesgo", "Verificar #/risks"],
                    expected="Riesgo aprobado/cerrado", audit="RiskApproved", ui_gap="QM approve vía API."),
            ],
            notes="Todas las aprobaciones críticas: extensión API documentada en manual 07.",
        ),
        role(
            "auditor",
            "Auditor",
            "tenant",
            "auditor@alimentos-premium.test",
            ops,
            "Planifica y ejecuta auditorías. SoD: no cierra CAPAs de hallazgos.",
            ["AUDITMANAGEMENT.MANAGE", "AUDITMANAGEMENT.READ"],
            "#/audits",
            "Operations → Audit Management",
            ["CAPA Manager"],
            [
                _tc("TC-AU-001", "Login Auditor", objective="Acceso auditorías", priority="Alta", kind="Positivo",
                    preconditions="—", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["auditor@ → login v2"], expected="#/audits", audit="AuthenticationSuccess"),
                _tc("TC-AU-002", "Listado auditorías", objective="Consulta", priority="Alta", kind="Positivo",
                    preconditions="TC-AU-001", route="#/audits", menu="Audit Management", screen="Listado",
                    steps=["#/audits", "Ver tabla"], expected="Listado OK", audit="AuditManagementRead"),
                _tc("TC-AU-003", "Crear auditoría", objective="Registro real", priority="Crítica", kind="Positivo",
                    preconditions="TC-AU-002", route="#/audits", menu="Action Center", screen="#module-action-form",
                    steps=["name, code, scope", "E2E-AUD-001", "Crear registro real"],
                    fields=[{"name": "code", "value": "E2E-AUD-001"}],
                    expected="Auditoría creada", audit="AuditCreated", handoff="CAPA Manager"),
                _tc("TC-AU-004", "Sin cerrar CAPA", objective="SoD", priority="Alta", kind="Negativo",
                    preconditions="TC-AU-001", route="#/capa", menu="CAPA", screen="Permisos",
                    steps=["#/capa", "Verificar sin CAPA.MANAGE completo"],
                    expected="No gestiona cierre CAPA", audit="—"),
                _tc("TC-AU-005", "Audit trail", objective="Trazabilidad", priority="Media", kind="Positivo",
                    preconditions="TC-AU-003", route="#/audit-trail", menu="Audit Trail", screen="Eventos",
                    steps=["Buscar AuditCreated"], expected="Evento visible", audit="AuditRead"),
            ],
        ),
        role(
            "supplier-manager",
            "Supplier Manager",
            "tenant",
            "supplier@alimentos-premium.test",
            ops,
            "Gestiona proveedores, evaluaciones y homologación.",
            ["SUPPLIER.CREATE", "SUPPLIER.UPDATE", "SUPPLIER.APPROVE"],
            "#/suppliers",
            "Operations → Supplier Management",
            ["CAPA Manager (NC)"],
            [
                _tc("TC-SM-001", "Login Supplier Manager", objective="Acceso proveedores", priority="Crítica", kind="Positivo",
                    preconditions="e2e_provision", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["supplier@ → login v2"], expected="#/suppliers", audit="AuthenticationSuccess"),
                _tc("TC-SM-002", "Listado proveedores", objective="Consulta", priority="Alta", kind="Positivo",
                    preconditions="TC-SM-001", route="#/suppliers", menu="Supplier Management", screen="Listado",
                    steps=["#/suppliers"], expected="Tabla proveedores", audit="SupplierRead"),
                _tc("TC-SM-003", "Crear proveedor", objective="Registro real", priority="Crítica", kind="Positivo",
                    preconditions="TC-SM-002", route="#/suppliers", menu="Action Center", screen="#module-action-form",
                    steps=["name, code (RUC), country", "E2E-SUP-001", "Crear registro real"],
                    fields=[{"name": "code", "value": "E2E-SUP-001"}, {"name": "country", "value": "PA"}],
                    expected="Proveedor creado", audit="SupplierCreated"),
                _tc("TC-SM-004", "Homologación", objective="Evaluación", priority="Media", kind="Positivo",
                    preconditions="TC-SM-003", route="#/suppliers", menu="Detalle", screen="Evaluación",
                    steps=["Abrir proveedor", "Registrar evaluación según UI"],
                    expected="Estado homologación actualizado", audit="SupplierEvaluated"),
                _tc("TC-SM-005", "Logout Salir", objective="Cierre", priority="Baja", kind="Positivo",
                    preconditions="Sesión", route="—", menu="Salir", screen="Topbar",
                    steps=["Salir"], expected="Login", audit="Logout"),
            ],
        ),
        role(
            "capa-manager",
            "CAPA Manager",
            "tenant",
            "capa@alimentos-premium.test",
            ops,
            "Gestiona CAPA excepto aprobación final (QM).",
            ["CAPA.MANAGE", "CAPA.READ"],
            "#/capa",
            "Operations → CAPA",
            ["Quality Manager (cierre)"],
            [
                _tc("TC-CM-001", "Login CAPA Manager", objective="Acceso CAPA", priority="Alta", kind="Positivo",
                    preconditions="—", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["capa@ → login v2"], expected="#/capa", audit="AuthenticationSuccess"),
                _tc("TC-CM-002", "Crear CAPA", objective="Registro real", priority="Crítica", kind="Positivo",
                    preconditions="TC-CM-001", route="#/capa", menu="Action Center", screen="#module-action-form",
                    steps=["CAPA-2026-014", "Crear registro real"],
                    fields=[{"name": "code", "value": "CAPA-2026-014"}],
                    expected="CAPA Draft/Open", audit="CapaCreated", handoff="Quality Manager"),
                _tc("TC-CM-003", "Avanzar workflow", objective="PendingApproval", priority="Alta", kind="Positivo",
                    preconditions="TC-CM-002", route="#/capa", menu="Workflow", screen="CAPA detail",
                    steps=["Clasificar", "Acciones correctivas según UI/API", "PendingApproval"],
                    expected="Estado PendingApproval", audit="CapaStatusChanged",
                    ui_gap="5-Why/Ishikawa sin UI; usar customer_journey.ps1"),
                _tc("TC-CM-004", "Sin aprobar cierre", objective="SoD", priority="Alta", kind="Negativo",
                    preconditions="TC-CM-003", route="#/capa", menu="—", screen="Acciones",
                    steps=["Verificar sin CAPA.APPROVE en UI"],
                    expected="No cierra CAPA", audit="—"),
                _tc("TC-CM-005", "Audit trail CAPA", objective="Trazabilidad", priority="Media", kind="Positivo",
                    preconditions="TC-CM-002", route="#/audit-trail", menu="Audit Trail", screen="Eventos",
                    steps=["CapaCreated en trail"], expected="Evento visible", audit="AuditRead"),
            ],
        ),
        role(
            "risk-manager",
            "Risk Manager",
            "tenant",
            "risk@alimentos-premium.test",
            ops,
            "Registro riesgos, tratamientos, controles. Aprobación QM.",
            ["RISK.MANAGE", "RISK.READ"],
            "#/risks",
            "Operations → Risk Management",
            ["Quality Manager", "Indicators Manager"],
            [
                _tc("TC-RM-001", "Login Risk Manager", objective="Acceso riesgos", priority="Alta", kind="Positivo",
                    preconditions="—", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["risk@ → login v2"], expected="#/risks", audit="AuthenticationSuccess"),
                _tc("TC-RM-002", "Crear riesgo", objective="Registro real", priority="Crítica", kind="Positivo",
                    preconditions="TC-RM-001", route="#/risks", menu="Action Center", screen="#module-action-form",
                    steps=["RSK-CC-001, area, process", "Crear registro real"],
                    fields=[{"name": "code", "value": "RSK-CC-001"}],
                    expected="Riesgo creado", audit="RiskCreated", handoff="Quality Manager"),
                _tc("TC-RM-003", "Evaluar riesgo", objective="Inherente/residual", priority="Alta", kind="Positivo",
                    preconditions="TC-RM-002", route="#/risks", menu="Detalle", screen="Evaluación",
                    steps=["Assess según UI"], expected="Scores registrados", audit="RiskAssessed"),
                _tc("TC-RM-004", "Tratamiento", objective="Plan tratamiento", priority="Media", kind="Positivo",
                    preconditions="TC-RM-003", route="#/risks", menu="Tratamiento", screen="Controls",
                    steps=["Definir tratamiento"], expected="Tratamiento guardado", audit="RiskTreatmentUpdated"),
                _tc("TC-RM-005", "Submit aprobación", objective="Handoff QM", priority="Alta", kind="Positivo",
                    preconditions="TC-RM-004", route="#/risks", menu="Workflow", screen="Submit",
                    steps=["Submit a aprobación"], expected="Pending QM", audit="RiskSubmitted", handoff="Quality Manager"),
            ],
        ),
        role(
            "indicators-manager",
            "Indicators Manager",
            "tenant",
            "indicators@alimentos-premium.test",
            ops,
            "Indicadores calidad, fórmulas, umbrales, mediciones.",
            ["INDICATOR.MANAGE", "INDICATOR.EXPORT"],
            "#/indicators",
            "Operations → Quality Indicators",
            ["Reporting Manager", "Dashboard"],
            [
                _tc("TC-IM-001", "Login Indicators Manager", objective="Acceso indicadores", priority="Alta", kind="Positivo",
                    preconditions="—", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["indicators@ → login v2"], expected="#/indicators", audit="AuthenticationSuccess"),
                _tc("TC-IM-002", "Crear indicador", objective="Registro real", priority="Crítica", kind="Positivo",
                    preconditions="TC-IM-001", route="#/indicators", menu="Action Center", screen="#module-action-form",
                    steps=["KPI-NC-RATE, unit", "Crear registro real"],
                    fields=[{"name": "code", "value": "KPI-NC-RATE"}],
                    expected="Indicador creado", audit="IndicatorCreated"),
                _tc("TC-IM-003", "Registrar medición", objective="Dato operativo", priority="Alta", kind="Positivo",
                    preconditions="TC-IM-002", route="#/indicators", menu="Detalle", screen="Medición",
                    steps=["Agregar medición periodo"], expected="Valor calculado", audit="IndicatorMeasurementRecorded"),
                _tc("TC-IM-004", "Dashboard reflejo", objective="Visibilidad gerencial", priority="Media", kind="Positivo",
                    preconditions="TC-IM-003", route="#/dashboard", menu="Executive Dashboard", screen="KPIs",
                    steps=["#/dashboard", "Buscar KPI"], expected="Métrica visible o en reporte", audit="—"),
                _tc("TC-IM-005", "Logout Salir", objective="Cierre", priority="Baja", kind="Positivo",
                    preconditions="Sesión", route="—", menu="Salir", screen="Topbar",
                    steps=["Salir"], expected="Login", audit="Logout"),
            ],
        ),
        role(
            "reporting-manager",
            "Reporting Manager",
            "tenant",
            "reporting@alimentos-premium.test",
            ops,
            "Report Center: ejecución, export, scheduling.",
            ["REPORT.MANAGE", "REPORT.EXECUTE", "REPORT.EXPORT"],
            "#/reports",
            "Command Center → Report Center",
            ["Quality Manager", "Gerencia"],
            [
                _tc("TC-RP-001", "Login Reporting Manager", objective="Acceso reportes", priority="Alta", kind="Positivo",
                    preconditions="Datos en módulos", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["reporting@ → login v2"], expected="#/reports", audit="AuthenticationSuccess"),
                _tc("TC-RP-002", "Seed standard reports", objective="Catálogo reportes", priority="Alta", kind="Positivo",
                    preconditions="TC-RP-001", route="#/reports", menu="Report Center", screen="Seed",
                    steps=["Seed standard reports", "Confirmar listado"],
                    expected="Reportes seed OK", audit="ReportSeeded"),
                _tc("TC-RP-003", "Ejecutar reporte", objective="Generación", priority="Crítica", kind="Positivo",
                    preconditions="TC-RP-002", route="#/reports", menu="Report Center", screen="Execute",
                    steps=["Seleccionar reporte compliance/ejecutivo", "Ejecutar", "Esperar loader reportes"],
                    expected="Reporte completado", audit="ReportExecuted"),
                _tc("TC-RP-004", "Exportar via reportes", objective="Export", priority="Alta", kind="Positivo",
                    preconditions="TC-RP-003", route="#/reports", menu="Export", screen="Export",
                    steps=["Exportar via reportes"], expected="Descarga o toast", audit="ReportExported"),
                _tc("TC-RP-005", "Compliance dashboard", objective="Reporte ejecutivo", priority="Media", kind="Positivo",
                    preconditions="TC-RP-003", route="#/compliance", menu="Compliance Dashboard", screen="Dashboard",
                    steps=["#/compliance", "Ver métricas agregadas"],
                    expected="Dashboard coherente con datos", audit="—"),
            ],
        ),
        role(
            "storage-administrator",
            "Storage Administrator",
            "tenant",
            "storage@alimentos-premium.test",
            ops,
            "Storage providers. SoD: no administra notifications/SMTP.",
            ["TENANT.STORAGE", "STORAGE.CREATE", "STORAGE.UPDATE"],
            "#/configuration",
            "Enterprise → Configuration",
            ["Document Controller (adjuntos)"],
            [
                _tc("TC-ST-001", "Login Storage Admin", objective="Acceso configuration", priority="Alta", kind="Positivo",
                    preconditions="—", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["storage@ → login v2"], expected="#/configuration", audit="AuthenticationSuccess"),
                _tc("TC-ST-002", "Crear Storage Local", objective="Provider local", priority="Crítica", kind="Positivo",
                    preconditions="TC-ST-001", route="#/configuration", menu="Configuration", screen="Storage panel",
                    steps=["Crear Storage Local", "Completar formulario", "Guardar"],
                    expected="Provider configurado", audit="StorageProviderCreated"),
                _tc("TC-ST-003", "Probar primer Storage Provider", objective="Smoke test", priority="Alta", kind="Positivo",
                    preconditions="TC-ST-002", route="#/configuration", menu="Configuration", screen="Test",
                    steps=["Probar primer Storage Provider"], expected="Test OK", audit="StorageProviderTested"),
                _tc("TC-ST-004", "SoD notifications", objective="Sin SMTP admin", priority="Media", kind="Negativo",
                    preconditions="TC-ST-001", route="#/configuration", menu="Notifications panel", screen="SoD",
                    steps=["Verificar panel notifications limitado o ausente"],
                    expected="Storage Admin no administra SMTP", audit="—"),
                _tc("TC-ST-005", "Handoff documentos", objective="Adjuntos", priority="Media", kind="Positivo",
                    preconditions="TC-ST-003", route="#/documents", menu="—", screen="—",
                    steps=["doccontrol@ crea doc con adjunto si UI expone upload"],
                    expected="Archivo asociado", audit="DocumentAttachmentCreated", handoff="Document Controller"),
            ],
        ),
        role(
            "notification-administrator",
            "Notification Administrator",
            "tenant",
            "notifications@alimentos-premium.test",
            ops,
            "Notificaciones, templates, SMTP. SoD: no storage.",
            ["TENANT.NOTIFICATIONS", "NOTIFICATION.ADMIN"],
            "#/configuration",
            "Enterprise → Configuration",
            ["Todos los roles (alertas)"],
            [
                _tc("TC-NA-001", "Login Notification Admin", objective="Acceso config", priority="Alta", kind="Positivo",
                    preconditions="—", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["notifications@ → login v2"], expected="#/configuration", audit="AuthenticationSuccess"),
                _tc("TC-NA-002", "Panel notifications", objective="SMTP config", priority="Alta", kind="Positivo",
                    preconditions="TC-NA-001", route="#/configuration", menu="Notifications", screen="Email SMTP",
                    steps=["Revisar Notification providers", "Configurar SMTP SMTP demo"],
                    expected="Formulario SMTP visible", audit="—"),
                _tc("TC-NA-003", "Crear Email SMTP", objective="Provider email", priority="Crítica", kind="Positivo",
                    preconditions="TC-NA-002", route="#/configuration", menu="Crear Email SMTP", screen="Form",
                    steps=["Crear Email SMTP", "Host demo", "Guardar"],
                    expected="Provider registrado o PENDING third-party", audit="NotificationProviderCreated",
                    ui_gap="SMTP real = PENDING third-party."),
                _tc("TC-NA-004", "SoD storage", objective="Sin storage admin", priority="Media", kind="Negativo",
                    preconditions="TC-NA-001", route="#/configuration", menu="Storage panel", screen="SoD",
                    steps=["Verificar sin gestión storage completa"],
                    expected="Notification Admin no administra storage", audit="—"),
                _tc("TC-NA-005", "Templates", objective="Plantillas", priority="Media", kind="Positivo",
                    preconditions="TC-NA-003", route="#/configuration", menu="Templates", screen="Templates",
                    steps=["Revisar templates notificación"], expected="Templates listados", audit="NotificationTemplateRead"),
            ],
        ),
        role(
            "viewer",
            "Viewer",
            "tenant",
            "viewer@alimentos-premium.test",
            ops,
            "Solo lectura transversal. No create/edit/approve/configure.",
            ["TENANT.READ", "DOCUMENT.READ", "CAPA.READ", "RISK.READ", "..."],
            "#/dashboard",
            "Command Center + Operations (lectura)",
            ["—"],
            [
                _tc("TC-VW-001", "Login Viewer", objective="Acceso lectura", priority="Alta", kind="Positivo",
                    preconditions="Datos existentes", route=f"{base}/", menu="Login", screen="Login v2",
                    steps=["viewer@ → login v2"], expected="Dashboard visible", audit="AuthenticationSuccess"),
                _tc("TC-VW-002", "Documents read-only", objective="Modo solo lectura", priority="Alta", kind="Positivo",
                    preconditions="TC-VW-001", route="#/documents", menu="Document Management", screen="Modo solo lectura",
                    steps=["#/documents", "Modo solo lectura", "Sin Crear registro real"],
                    expected="Solo consulta", audit="—"),
                _tc("TC-VW-003", "CAPA read-only", objective="Sin manage", priority="Alta", kind="Positivo",
                    preconditions="TC-VW-001", route="#/capa", menu="CAPA", screen="Modo solo lectura",
                    steps=["#/capa", "Verificar read-only"], expected="Sin create", audit="—"),
                _tc("TC-VW-004", "POST create 403", objective="Seguridad API", priority="Crítica", kind="Negativo",
                    preconditions="TC-VW-001", route="API", menu="DevTools", screen="POST documents",
                    steps=["POST create document → 403"], expected="AuthorizationDenied", audit="AuthorizationDenied"),
                _tc("TC-VW-005", "Recorrido transversal", objective="Visibilidad gerencial", priority="Media", kind="Positivo",
                    preconditions="TC-VW-001", route="#/dashboard", menu="Múltiples", screen="Shell",
                    steps=["Recorrer dashboard, compliance, reports, risks", "Todos read-only"],
                    expected="Navegación sin errores 403 UI", audit="—"),
            ],
        ),
    ]


MANUAL_DATA["roles"] = build_roles()


CSS = """
:root{
  color-scheme:light;
  --bg:#eef2f8;--surface:#fff;--surface2:#f4f7fb;--text:#0f172a;--muted:#64748b;
  --border:#d8e0ea;--accent:#0f4c81;--accent2:#1769aa;--success:#0f766e;--warn:#b45309;
  --danger:#b91c1c;--focus:#2563eb;--shadow:0 16px 40px rgba(15,23,42,.08);
  --grad:linear-gradient(135deg,#0f4c81,#1769aa 55%,#0f766e);
  --sidebar:260px;--header:58px;--radius:12px;
  font-family:"Segoe UI",system-ui,sans-serif;
}
[data-theme=dark]{
  color-scheme:dark;
  --bg:#0b1220;--surface:#111827;--surface2:#1a2332;--text:#e5edf7;--muted:#94a3b8;
  --border:#243044;--accent:#5b9fd4;--accent2:#7eb8e8;--shadow:0 16px 40px rgba(0,0,0,.35);
}
*,*::before,*::after{box-sizing:border-box}
html,body{height:100%;margin:0;background:var(--bg);color:var(--text)}
button,input,select,textarea{font:inherit}
a{color:var(--accent2);text-decoration:none}
.shell{display:grid;grid-template-columns:var(--sidebar) 1fr;grid-template-rows:var(--header) 1fr;min-height:100vh}
.sidebar{grid-row:1/3;background:var(--surface);border-right:1px solid var(--border);display:flex;flex-direction:column;overflow:hidden}
.brand{padding:1rem;border-bottom:1px solid var(--border);display:flex;gap:.65rem;align-items:center}
.brand-mark{width:38px;height:38px;border-radius:10px;background:var(--grad);color:#fff;display:grid;place-items:center;font-size:.7rem;font-weight:800}
.brand h1{font-size:.92rem;margin:0;line-height:1.25}
.brand p{margin:0;font-size:.68rem;color:var(--muted)}
.nav-scroll{flex:1;overflow:auto;padding:.5rem}
.nav-group{margin-bottom:.75rem}
.nav-group-title{font-size:.62rem;text-transform:uppercase;letter-spacing:.08em;color:var(--muted);padding:.35rem .55rem;font-weight:700}
.nav-item{display:block;width:100%;text-align:left;border:1px solid transparent;background:transparent;color:var(--text);
  padding:.45rem .55rem;border-radius:8px;font-size:.78rem;cursor:pointer}
.nav-item:hover,.nav-item.active{background:var(--surface2);border-color:var(--border)}
.nav-item.active{border-color:var(--accent);box-shadow:inset 3px 0 0 var(--accent)}
.sidebar-foot{padding:.65rem;border-top:1px solid var(--border);font-size:.68rem;color:var(--muted)}
.header{grid-column:2;background:rgba(255,255,255,.92);border-bottom:1px solid var(--border);display:flex;align-items:center;gap:.5rem;padding:.45rem .85rem;flex-wrap:wrap;backdrop-filter:blur(12px);position:sticky;top:0;z-index:10}
[data-theme=dark] .header{background:rgba(17,24,39,.92)}
.main{grid-column:2;overflow:auto;padding:1rem 1.15rem 2rem}
.search-wrap{flex:1;min-width:180px;max-width:420px;position:relative}
.search-wrap input{width:100%;border:1px solid var(--border);background:var(--surface);border-radius:10px;padding:.45rem .65rem .45rem 1.8rem}
.search-wrap::before{content:"⌕";position:absolute;left:.55rem;top:50%;transform:translateY(-50%);color:var(--muted);font-size:.85rem}
.mode-toggle,.toolbar{display:flex;gap:.25rem;flex-wrap:wrap}
.chip-btn,.btn{border:1px solid var(--border);background:var(--surface);color:var(--text);border-radius:9px;padding:.38rem .7rem;font-size:.74rem;font-weight:650;cursor:pointer}
.chip-btn.active,.btn.primary{background:var(--grad);border-color:var(--accent);color:#fff}
.btn.danger{border-color:#fecaca;color:var(--danger)}
.btn.subtle{background:var(--surface2)}
.breadcrumbs{font-size:.72rem;color:var(--muted);margin-bottom:.65rem;display:flex;gap:.35rem;flex-wrap:wrap;align-items:center}
.breadcrumbs span::after{content:"›";margin-left:.35rem;color:var(--muted)}
.breadcrumbs span:last-child::after{content:""}
.progress-bar{height:6px;background:var(--surface2);border-radius:999px;overflow:hidden;margin:.5rem 0 1rem;border:1px solid var(--border)}
.progress-fill{height:100%;background:var(--grad);width:0;transition:width .25s}
.hero{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:1rem 1.1rem;margin-bottom:1rem;box-shadow:var(--shadow)}
.hero h2{margin:0 0 .35rem;font-size:1.15rem}
.hero p{margin:0;color:var(--muted);font-size:.86rem;line-height:1.55}
.grid{display:grid;gap:.85rem}
.grid-2{grid-template-columns:repeat(auto-fit,minmax(280px,1fr))}
.card{background:var(--surface);border:1px solid var(--border);border-radius:var(--radius);padding:.85rem;box-shadow:var(--shadow)}
.card h3{margin:0 0 .45rem;font-size:.92rem}
.card p,.card li{font-size:.8rem;color:var(--muted);line-height:1.55}
.meta-row{display:flex;flex-wrap:wrap;gap:.35rem;margin:.5rem 0}
.tag{display:inline-flex;padding:.2rem .5rem;border-radius:999px;border:1px solid var(--border);font-size:.66rem;font-weight:700;background:var(--surface2)}
.tag.pass{background:#ecfdf5;border-color:#6ee7b7;color:var(--success)}
.tag.fail{background:#fef2f2;border-color:#fca5a5;color:var(--danger)}
.tag.pending{background:#fffbeb;border-color:#fcd34d;color:var(--warn)}
.tag.ne{background:#f1f5f9;border-color:var(--border);color:var(--muted)}
.section{display:none}
.section.active{display:block}
.tc-list{display:grid;gap:.65rem}
.tc{border:1px solid var(--border);border-radius:var(--radius);overflow:hidden;background:var(--surface)}
.tc-head{display:flex;justify-content:space-between;gap:.5rem;padding:.65rem .75rem;cursor:pointer;background:var(--surface2);align-items:flex-start}
.tc-head h4{margin:0;font-size:.82rem}
.tc-body{display:none;padding:.75rem;border-top:1px solid var(--border);font-size:.78rem;line-height:1.55}
.tc.open .tc-body{display:block}
.step-nav{display:flex;gap:.35rem;margin:.65rem 0;flex-wrap:wrap}
.step-card{border:1px solid var(--border);border-radius:10px;padding:.55rem .65rem;background:var(--surface2)}
.step-card.active{border-color:var(--accent);box-shadow:inset 3px 0 0 var(--accent)}
.kbd{font-family:ui-monospace,monospace;font-size:.68rem;padding:.1rem .35rem;border:1px solid var(--border);border-radius:4px;background:var(--surface)}
.field-table{width:100%;border-collapse:collapse;font-size:.74rem;margin:.5rem 0}
.field-table th,.field-table td{border:1px solid var(--border);padding:.35rem .45rem;text-align:left}
.status-row{display:flex;gap:.35rem;flex-wrap:wrap;margin:.65rem 0}
.status-row button.active{outline:2px solid var(--focus)}
.notes{width:100%;min-height:64px;border:1px solid var(--border);border-radius:8px;padding:.45rem;background:var(--surface);color:var(--text);resize:vertical}
.gap-box{border-left:4px solid var(--warn);background:#fffbeb;padding:.55rem .65rem;border-radius:0 8px 8px 0;margin:.5rem 0;font-size:.76rem}
[data-theme=dark] .gap-box{background:#422006;color:#fde68a}
.present-mode .notes,.present-mode .status-row,.present-mode .sidebar-foot{display:none!important}
.present-mode .tc-body{display:block!important}
.execute-mode .tc-meta-learn{display:none}
.learn-mode .tc-meta-exec{display:none}
@media(max-width:900px){
  .shell{grid-template-columns:1fr;grid-template-rows:auto auto 1fr}
  .sidebar{grid-row:auto;max-height:42vh}
  .header,.main{grid-column:1}
}
@media print{
  .sidebar,.header,.mode-toggle,.toolbar,.step-nav,.status-row,.notes,.search-wrap{display:none!important}
  .shell{display:block}
  .main{padding:0}
  .section,.tc-body{display:block!important}
  .card,.hero{box-shadow:none;break-inside:avoid}
}
"""


JS = r"""
(function(){
  const DATA = __DATA__;
  const KEY = DATA.meta.storage_key;
  const $ = (s, r=document) => r.querySelector(s);
  const $$ = (s, r=document) => [...r.querySelectorAll(s)];

  let state = loadState();
  let section = 'intro';
  let stepIndex = 0;
  let flatSteps = [];

  function loadState(){
    try { return JSON.parse(localStorage.getItem(KEY)) || defaultState(); }
    catch { return defaultState(); }
  }
  function defaultState(){
    return { mode:'learn', theme:'light', results:{}, notes:{}, lastSection:'intro', lastTc:null };
  }
  function saveState(){ localStorage.setItem(KEY, JSON.stringify(state)); }

  function tcKey(id){ return id; }

  function allTestCases(){
    const list = [];
    DATA.roles.forEach(r => r.test_cases.forEach(tc => list.push({...tc, roleKey:r.key, roleName:r.name})));
    return list;
  }

  function progressStats(){
    const tcs = allTestCases();
    let pass=0,fail=0,pending=0,ne=0;
    tcs.forEach(tc=>{
      const st = state.results[tcKey(tc.id)] || 'NOT_EXECUTED';
      if(st==='PASS') pass++; else if(st==='FAIL') fail++;
      else if(st==='PENDING') pending++; else ne++;
    });
    const done = pass+fail+pending;
    return { total:tcs.length, pass, fail, pending, ne, pct: tcs.length? Math.round(done/tcs.length*100):0 };
  }

  function setTheme(t){
    state.theme = t;
    document.documentElement.setAttribute('data-theme', t==='dark'?'dark':'light');
    saveState();
  }

  function setMode(m){
    state.mode = m;
    document.body.className = m+'-mode';
    $$('.chip-btn[data-mode]').forEach(b=> b.classList.toggle('active', b.dataset.mode===m));
    saveState(); renderMain();
  }

  function navigate(sec, tcId){
    section = sec; state.lastSection = sec;
    if(tcId) state.lastTc = tcId;
    $$('.nav-item').forEach(n=> n.classList.toggle('active', n.dataset.section===sec));
    saveState(); renderMain();
  }

  function buildFlatSteps(container){
    flatSteps = [];
    container.querySelectorAll('.step-item').forEach((el,i)=>{
      flatSteps.push({el, idx:i});
    });
    stepIndex = 0;
    highlightStep();
  }

  function highlightStep(){
    flatSteps.forEach((s,i)=>{
      s.el.classList.toggle('active', i===stepIndex);
      if(i===stepIndex) s.el.scrollIntoView({block:'nearest',behavior:'smooth'});
    });
  }

  function esc(s){ const d=document.createElement('div'); d.textContent=s??''; return d.innerHTML; }

  function renderStatusButtons(tcId){
    const cur = state.results[tcKey(tcId)] || 'NOT_EXECUTED';
    return ['PASS','FAIL','PENDING','NOT_EXECUTED'].map(st=>
      `<button type="button" class="chip-btn ${cur===st?'active':''}" data-status="${st}" data-tc="${esc(tcId)}">${st}</button>`
    ).join('');
  }

  function renderTc(tc, roleName){
    const note = state.notes[tcKey(tc.id)] || '';
    const st = state.results[tcKey(tc.id)] || 'NOT_EXECUTED';
    const tagCls = st==='PASS'?'pass':st==='FAIL'?'fail':st==='PENDING'?'pending':'ne';
    const stepsHtml = tc.steps.map((s,i)=>`<div class="step-item step-card ${i===0?'active':''}"><strong>Paso ${i+1}.</strong> ${esc(s)}</div>`).join('');
    const fieldsHtml = tc.fields && tc.fields.length ? `<table class="field-table"><tr><th>Campo</th><th>Valor</th></tr>${tc.fields.map(f=>`<tr><td>${esc(f.name)}</td><td><code>${esc(f.value)}</code></td></tr>`).join('')}</table>`:'';
    const gap = tc.ui_gap ? `<div class="gap-box"><strong>Limitación UI:</strong> ${esc(tc.ui_gap)}</div>`:'';
    return `<article class="tc" data-tc-id="${esc(tc.id)}">
      <div class="tc-head" data-toggle-tc="${esc(tc.id)}">
        <div><h4>${esc(tc.id)} — ${esc(tc.name)}</h4>
          <div class="meta-row"><span class="tag">${esc(tc.priority)}</span><span class="tag">${esc(tc.kind)}</span><span class="tag ${tagCls}">${esc(st)}</span></div>
        </div>
        <span class="kbd">${esc(tc.route)}</span>
      </div>
      <div class="tc-body">
        <p><strong>Objetivo:</strong> ${esc(tc.objective)}</p>
        <div class="grid grid-2 tc-meta-learn">
          <div><strong>Ruta:</strong> <code>${esc(tc.route)}</code><br><strong>Menú:</strong> ${esc(tc.menu)}<br><strong>Pantalla:</strong> ${esc(tc.screen)}</div>
          <div><strong>Precondiciones:</strong> ${esc(tc.preconditions)}<br><strong>Auditoría:</strong> ${esc(tc.audit)}<br><strong>Handoff:</strong> ${esc(tc.handoff||'—')}</div>
        </div>
        <p class="tc-meta-exec"><strong>Esperado:</strong> ${esc(tc.expected)}</p>
        ${fieldsHtml}${gap}
        <div class="step-nav">${stepsHtml}</div>
        <p><strong>Resultado esperado:</strong> ${esc(tc.expected)}</p>
        <div class="status-row test-only">${renderStatusButtons(tc.id)}</div>
        <textarea class="notes test-only" data-note="${esc(tc.id)}" placeholder="Notas / evidencia (ruta screenshots)">${esc(note)}</textarea>
      </div>
    </article>`;
  }

  function renderMain(){
    const mainContent = $('#main-content');
    const stats = progressStats();
    $('#progressPct').textContent = stats.pct + '%';
    $('#progressFill').style.width = stats.pct + '%';
    $('#statPass').textContent = stats.pass;
    $('#statFail').textContent = stats.fail;
    $('#statPending').textContent = stats.pending;
    $('#statNe').textContent = stats.ne;

    const crumbs = section==='intro'?['Inicio']: section.startsWith('role-')? ['Roles', section.replace('role-','').replace(/-/g,' ')]: [section.replace(/-/g,' ')];
    $('#breadcrumbs').innerHTML = crumbs.map(c=>`<span>${esc(c)}</span>`).join('');

    let html = '';
    if(section==='intro'){
      html = `<div class="hero"><h2>${esc(DATA.meta.title)}</h2>
        <p>Manual interactivo verificado contra <code>app.js</code>, <code>RoleCatalog.cs</code> y manuales MFT. Empresa demo: <strong>${esc(DATA.company.name)}</strong> (${esc(DATA.company.industry)}, ${esc(DATA.company.country)}). Estándares: ${esc(DATA.company.standards.join(', '))}.</p>
        <div class="meta-row"><span class="tag">Tenant ops: ${esc(DATA.tenants.operations.id)}</span><span class="tag">Plataforma: ${esc(DATA.tenants.platform.id)}</span></div>
        <p style="margin-top:.65rem">Login v2: <strong>Siguiente</strong> → <strong>Continuar</strong> → <strong>Iniciar sesion</strong>. Botones reales: <strong>Crear registro real</strong>, <strong>Modo solo lectura</strong>, <strong>Salir</strong>.</p>
      </div>
      <div class="grid grid-2">
        <div class="card"><h3>Modos</h3><ul><li><strong>Aprendizaje</strong> — contexto completo</li><li><strong>Ejecución</strong> — pasos accionables</li><li><strong>Prueba</strong> — PASS/FAIL/PENDING + notas</li><li><strong>Presentación</strong> — vista limpia</li></ul></div>
        <div class="card"><h3>Atajos</h3><ul><li><span class="kbd">/</span> buscar</li><li><span class="kbd">←</span> <span class="kbd">→</span> paso anterior/siguiente</li></ul></div>
      </div>`;
    } else if(section==='sequence'){
      html = `<div class="hero"><h2>Secuencia maestra (22 pasos)</h2><p>Orden alineado con roadmap MFT y customer_journey.ps1.</p></div>
        <div class="tc-list">${DATA.master_sequence.map(s=>`<div class="card"><h3>Paso ${s.n}: ${esc(s.title)}</h3><p>${esc(s.detail)}</p></div>`).join('')}</div>`;
    } else if(section==='flows'){
      html = `<div class="hero"><h2>Flujos transversales (8)</h2></div>
        <div class="tc-list">${DATA.cross_flows.map(f=>`<div class="card"><h3>${esc(f.id)} — ${esc(f.name)}</h3>
          <p><strong>Roles:</strong> ${esc(f.roles.join(' → '))}</p><p>${esc(f.summary)}</p>
          <ol>${f.steps.map(st=>`<li>${esc(st)}</li>`).join('')}</ol>
          <p><strong>Auditoría:</strong> ${esc(f.audit)}</p>
          ${f.ui_gap?`<div class="gap-box">${esc(f.ui_gap)}</div>`:''}
        </div>`).join('')}</div>`;
    } else if(section==='negative'){
      html = `<div class="hero"><h2>Validaciones negativas</h2></div><div class="grid grid-2">${DATA.negative_validations.map(n=>`<div class="card"><h3>${esc(n.id)} — ${esc(n.title)}</h3><p>${esc(n.detail)}</p></div>`).join('')}</div>`;
    } else if(section==='third-party'){
      html = `<div class="hero"><h2>Dependencias third-party (PENDING)</h2><p>No marcar FAIL si falta infraestructura externa.</p></div><div class="grid grid-2">${DATA.third_party_pending.map(t=>`<div class="card"><h3>${esc(t.id)} — ${esc(t.title)}</h3><p>${esc(t.detail)}</p></div>`).join('')}</div>`;
    } else if(section==='gaps'){
      html = `<div class="hero"><h2>Limitaciones UI conocidas</h2></div><ul class="card">${DATA.ui_gaps.map(g=>`<li>${esc(g)}</li>`).join('')}</ul>`;
    } else if(section.startsWith('role-')){
      const rk = section.slice(5);
      const role = DATA.roles.find(r=>r.key===rk);
      if(!role){ html='<p>Rol no encontrado.</p>'; }
      else {
        html = `<div class="hero"><h2>${esc(role.name)}</h2>
          <p>${esc(role.purpose)}</p>
          <div class="meta-row"><span class="tag">${esc(role.scope)}</span>
            ${role.email?`<span class="tag">${esc(role.email)}</span>`:'<span class="tag">Sin usuario E2E</span>'}
            <span class="tag">${esc(role.landing_route)}</span></div>
          ${role.notes?`<div class="gap-box">${esc(role.notes)}</div>`:''}
          <p><strong>Handoffs:</strong> ${esc(role.handoffs.join(', '))}</p>
        </div>
        <div class="card"><h3>Identidad</h3>
          <table class="field-table"><tr><th>Campo</th><th>Valor</th></tr>
            <tr><td>Tenant ID</td><td><code>${esc(role.tenant_id)}</code></td></tr>
            <tr><td>Email</td><td><code>${esc(role.email||'—')}</code></td></tr>
            <tr><td>Menú foco</td><td>${esc(role.menu_focus)}</td></tr>
            <tr><td>Permisos clave</td><td>${esc(role.permissions.join(', '))}</td></tr>
          </table>
        </div>
        <h3 style="margin:1rem 0 .5rem">Casos de prueba (${role.test_cases.length})</h3>
        <div class="tc-list">${role.test_cases.map(tc=>renderTc(tc, role.name)).join('')}</div>`;
      }
    }
    mainContent.innerHTML = html;

    $$('[data-toggle-tc]').forEach(h=>{
      h.onclick=()=> h.closest('.tc').classList.toggle('open');
    });
    const openTc = state.lastTc && section.startsWith('role-') ? $(`.tc[data-tc-id="${state.lastTc}"]`) : null;
    if(openTc) openTc.classList.add('open');

    $$('.test-only').forEach(el=>{
      el.style.display = state.mode==='test' ? '' : 'none';
    });

    $$('[data-status]').forEach(btn=>{
      btn.onclick=()=>{
        state.results[tcKey(btn.dataset.tc)] = btn.dataset.status;
        saveState(); renderMain();
      };
    });
    $$('textarea[data-note]').forEach(ta=>{
      ta.oninput=()=>{ state.notes[tcKey(ta.dataset.note)] = ta.value; saveState(); };
    });

    buildFlatSteps(main);
  }

  function exportJson(){
    const payload = {
      exportedAt: new Date().toISOString(),
      manualVersion: DATA.meta.version,
      storageKey: KEY,
      progress: progressStats(),
      results: state.results,
      notes: state.notes,
    };
    const blob = new Blob([JSON.stringify(payload,null,2)], {type:'application/json'});
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = 'COMPLIANCE360_MANUAL_INTERACTIVO_RESULTADOS.json';
    a.click();
  }

  function resetProgress(){
    if(!confirm('¿Resetear progreso y resultados de prueba?')) return;
    state.results = {}; state.notes = {};
    saveState(); renderMain();
  }

  function doSearch(q){
    q = q.trim().toLowerCase();
    if(!q) return;
    for(const r of DATA.roles){
      for(const tc of r.test_cases){
        const blob = (tc.id+' '+tc.name+' '+tc.objective+' '+tc.route+' '+r.name).toLowerCase();
        if(blob.includes(q)){ navigate('role-'+r.key, tc.id); return; }
      }
    }
    const secs = ['sequence','flows','negative','third-party','gaps'];
    for(const s of secs){ if(s.includes(q)){ navigate(s); return; } }
  }

  function initShell(){
    const nav = $('#navItems');
    const items = [
      ['intro','Inicio'],['sequence','Secuencia 22 pasos'],['flows','Flujos transversales'],
      ...DATA.roles.map(r=>['role-'+r.key, r.name]),
      ['negative','Validaciones negativas'],['third-party','Third-party PENDING'],['gaps','Limitaciones UI']
    ];
    nav.innerHTML = items.map(([id,label])=>`<button type="button" class="nav-item" data-section="${id}">${esc(label)}</button>`).join('');
    $$('.nav-item').forEach(n=> n.onclick=()=> navigate(n.dataset.section));

    $$('.chip-btn[data-mode]').forEach(b=> b.onclick=()=> setMode(b.dataset.mode));
    $('#themeBtn').onclick=()=> setTheme(state.theme==='dark'?'light':'dark');
    $('#exportBtn').onclick=exportJson;
    $('#resetBtn').onclick=resetProgress;

    const search = $('#searchInput');
    search.onkeydown=(e)=>{
      if(e.key==='Enter'){ e.preventDefault(); doSearch(search.value); }
    };

    document.addEventListener('keydown',(e)=>{
      if(e.key==='/' && document.activeElement!==search){ e.preventDefault(); search.focus(); return; }
      if(['INPUT','TEXTAREA','SELECT'].includes(document.activeElement.tagName)) return;
      if(e.key==='ArrowRight' && flatSteps.length){ stepIndex=Math.min(stepIndex+1,flatSteps.length-1); highlightStep(); }
      if(e.key==='ArrowLeft' && flatSteps.length){ stepIndex=Math.max(stepIndex-1,0); highlightStep(); }
    });

    setTheme(state.theme || 'light');
    setMode(state.mode || 'learn');
    navigate(state.lastSection || 'intro');
  }

  initShell();
})();
"""


def render_html() -> str:
    data_json = json.dumps(MANUAL_DATA, ensure_ascii=False)
    js = JS.replace("__DATA__", data_json)
    nav_groups = ""
    for g in MANUAL_DATA["navigation"]:
        nav_groups += f'<div class="nav-group"><div class="nav-group-title">{g["group"]}</div>'
        seen = set()
        for key, label in g["items"]:
            if key in seen:
                continue
            seen.add(key)
            nav_groups += f'<div class="tag" style="margin:.15rem .55rem">{label} <code>#{key}</code></div>'
        nav_groups += "</div>"

    return f"""<!DOCTYPE html>
<html lang="es">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>{MANUAL_DATA["meta"]["title"]}</title>
<style>{CSS}</style>
</head>
<body>
<div class="shell">
  <aside class="sidebar">
    <div class="brand">
      <div class="brand-mark">C360</div>
      <div>
        <h1>Compliance 360</h1>
        <p>Manual interactivo · v{MANUAL_DATA["meta"]["version"]}</p>
      </div>
    </div>
    <div class="nav-scroll" id="navItems"></div>
    <div class="sidebar-foot">{MANUAL_DATA["company"]["name"]}<br>Rutas verificadas app.js</div>
  </aside>
  <header class="header">
    <div class="search-wrap"><input id="searchInput" type="search" placeholder="Buscar casos, roles, rutas… (Enter)" aria-label="Buscar"></div>
    <div class="mode-toggle" role="group" aria-label="Modo">
      <button type="button" class="chip-btn" data-mode="learn">Aprendizaje</button>
      <button type="button" class="chip-btn" data-mode="execute">Ejecución</button>
      <button type="button" class="chip-btn" data-mode="test">Prueba</button>
      <button type="button" class="chip-btn" data-mode="present">Presentación</button>
    </div>
    <div class="toolbar">
      <button type="button" class="btn subtle" id="themeBtn" title="Tema">Tema</button>
      <button type="button" class="btn primary" id="exportBtn">Exportar JSON</button>
      <button type="button" class="btn danger" id="resetBtn">Reset progreso</button>
    </div>
  </header>
  <main class="main" id="main">
    <div class="breadcrumbs" id="breadcrumbs"></div>
    <div class="progress-bar" title="Progreso pruebas"><div class="progress-fill" id="progressFill"></div></div>
    <p style="font-size:.72rem;color:var(--muted);margin:-.5rem 0 .85rem">
      Progreso: <strong id="progressPct">0%</strong> · PASS <span id="statPass">0</span> · FAIL <span id="statFail">0</span> · PENDING <span id="statPending">0</span> · NOT_EXECUTED <span id="statNe">0</span>
    </p>
    <div id="main-content"></div>
  </main>
</div>
<script type="application/json" id="manual-data">{data_json}</script>
<script>{js}</script>
</body>
</html>
"""


def write_readme():
    content = """# Manual Interactivo — Compliance 360

Manual HTML **autocontenido** para aprendizaje, ejecución y pruebas funcionales por rol.

## Archivos

| Archivo | Descripción |
|---------|-------------|
| `COMPLIANCE360_MANUAL_INTERACTIVO_POR_ROLES_Y_FLUJOS.html` | Manual interactivo (abrir con doble clic) |
| `VALIDACION_CONTENIDO_MANUAL_INTERACTIVO.md` | Trazabilidad contenido ↔ código |
| `00_ANALISIS_COMPLETO_MANUAL_INTERACTIVO.md` | Análisis previo |

## Generación

```powershell
python scripts/generate_manual_interactivo.py
```

## Uso

1. Abra el HTML en Chrome/Edge (doble clic).
2. Elija modo: **Aprendizaje**, **Ejecución**, **Prueba**, **Presentación**.
3. En modo **Prueba**, marque PASS/FAIL/PENDING/NOT_EXECUTED y notas.
4. **Exportar JSON** guarda resultados; **Reset progreso** limpia `localStorage`.
5. Atajos: `/` buscar, `←` `→` navegar pasos.

## Persistencia

Clave `localStorage`: `c360.manual.interactivo.v1`

## Datos verificados

- 17 roles (`RoleCatalog.cs`)
- Tenant Alimentos Premium Panamá S.A. (`ddcaf211-afe0-44a0-9c90-4fbda8fc4871`)
- Emails `@alimentos-premium.test` / plataforma `@compliance360.local`
- **Sin contraseñas** en el HTML — usar `e2e/testdata.json`

## Entorno

App en `http://localhost:5272` — ver `docs/manual-functional-testing/00_MASTER_FUNCTIONAL_TESTING_ROADMAP.md`.
"""
    (OUT_DIR / "README.md").write_text(content.strip() + "\n", encoding="utf-8")


def write_validation():
    roles_count = len(MANUAL_DATA["roles"])
    tc_count = sum(len(r["test_cases"]) for r in MANUAL_DATA["roles"])
    content = f"""# Validación de contenido — Manual Interactivo

**Fecha:** 2026-07-10  
**Generador:** `scripts/generate_manual_interactivo.py`  
**Salida:** `COMPLIANCE360_MANUAL_INTERACTIVO_POR_ROLES_Y_FLUJOS.html`

## Resumen

| Elemento | Cantidad | Fuente |
|----------|----------|--------|
| Roles | {roles_count} | `RoleCatalog.cs` |
| Casos de prueba | {tc_count} | Manuales MFT 01–16 |
| Secuencia maestra | 22 pasos | Roadmap MFT §9 + análisis 00 |
| Flujos transversales | 8 | Manual 17 E2E-001..008 |
| Validaciones negativas | {len(MANUAL_DATA["negative_validations"])} | MFT + app.js |
| Third-party PENDING | {len(MANUAL_DATA["third_party_pending"])} | Roadmap §7 |

## Verificación cruzada

| Requisito | Estado | Evidencia |
|-----------|--------|-----------|
| Login v2 Siguiente → Continuar → Iniciar sesion | OK | `app.js` L440–464 |
| Crear registro real | OK | `app.js` L2578 |
| Modo solo lectura | OK | `app.js` L2588 |
| Salir logout | OK | `app.js` L567 |
| 19 rutas navegación | OK | `app.js` navigation L218–246 |
| Sin CDN / deps externas | OK | HTML autocontenido |
| localStorage c360.manual.interactivo.v1 | OK | JS embebido |
| Sin contraseñas en HTML | OK | MANUAL_DATA emails only |
| QM approve vía API documentado | OK | ui_gaps + TC-QM-DOC-003 |
| Support sin break-glass UI | OK | TC-SO-002, ui_gaps |
| TECHNICALSHEET.CREATE RBAC | OK | TC-TA-004, CF-03 |
| SMTP PENDING | OK | third_party_pending TP-SMTP |

## Roles sin usuario E2E

- Platform Operations — derivado de catálogo; asignar rol manualmente en tenant plataforma.
- Platform Security — idem.

## Regeneración

Tras cambios en `app.js` o manuales MFT, actualice `MANUAL_DATA` en el script Python y ejecute:

```powershell
python scripts/generate_manual_interactivo.py
```
"""
    (OUT_DIR / "VALIDACION_CONTENIDO_MANUAL_INTERACTIVO.md").write_text(content.strip() + "\n", encoding="utf-8")


def main():
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    html = render_html()
    OUT_HTML.write_text(html, encoding="utf-8")
    write_readme()
    write_validation()
    tc_count = sum(len(r["test_cases"]) for r in MANUAL_DATA["roles"])
    print(f"Wrote {OUT_HTML.relative_to(ROOT)}")
    print(f"  Roles: {len(MANUAL_DATA['roles'])}, Test cases: {tc_count}")
    print(f"  README + VALIDACION in {OUT_DIR.relative_to(ROOT)}/")


if __name__ == "__main__":
    main()
