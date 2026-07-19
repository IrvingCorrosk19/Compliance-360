#!/usr/bin/env python3
"""Generate unified visual step-by-step guide for all 17 Compliance 360 roles."""
from __future__ import annotations

import importlib.util
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "docs" / "manual-interactivo" / "COMPLIANCE360_GUIA_VISUAL_TODOS_LOS_ROLES.html"

# Load roles from existing manual generator
_spec = importlib.util.spec_from_file_location(
    "manual_gen", ROOT / "scripts" / "generate_manual_interactivo.py"
)
_manual = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_manual)
ROLES = _manual.MANUAL_DATA["roles"]
COMPANY = _manual.MANUAL_DATA["company"]
TENANTS = _manual.MANUAL_DATA["tenants"]

PLATFORM_ORG = "Compliance 360 Platform"
TENANT_ORG = COMPANY["commercial_name"]
APP_URL = "http://localhost:5272"

MODULES = {
    "documents": {
        "label": "Document Management",
        "desc": "Listado documental, vigencias, versiones, workflow y aprobaciones.",
        "form_fields": [
            {"id": "name", "label": "Nombre / titulo", "value": "Procedimiento Limpieza MFT", "required": True},
            {"id": "code", "label": "Codigo", "value": "BPM-LIM-001", "required": True, "hotspot": True},
            {"id": "description", "label": "Resumen de cambio", "value": "Documento creado desde la UI Enterprise"},
        ],
    },
    "technical-sheets": {
        "label": "Technical Sheets",
        "desc": "Fichas tecnicas, ingredientes, nutrientes, certificaciones y aprobaciones.",
        "form_fields": [
            {"id": "name", "label": "Nombre / titulo", "value": "Harina premium", "required": True},
            {"id": "code", "label": "Codigo", "value": "TS-MFT-001", "required": True, "hotspot": True},
            {"id": "description", "label": "Descripcion producto", "value": "Producto creado desde la UI Enterprise"},
        ],
    },
    "suppliers": {
        "label": "Supplier Management",
        "desc": "Expediente proveedor, documentos, evaluaciones, homologacion y alertas.",
        "form_fields": [
            {"id": "name", "label": "Razon social", "value": "Proveedor Demo S.A.", "required": True},
            {"id": "code", "label": "Identificacion fiscal", "value": "E2E-SUP-001", "required": True, "hotspot": True},
            {"id": "country", "label": "Pais", "value": "PA", "required": True},
        ],
    },
    "audits": {
        "label": "Audit Management",
        "desc": "Programacion, checklists, hallazgos, evidencias y seguimiento.",
        "form_fields": [
            {"id": "name", "label": "Nombre / titulo", "value": "Auditoría interna BPM", "required": True},
            {"id": "code", "label": "Codigo", "value": "E2E-AUD-001", "required": True, "hotspot": True},
            {"id": "scope", "label": "Alcance", "value": "Sistema de gestion de calidad"},
        ],
    },
    "capa": {
        "label": "CAPA",
        "desc": "Acciones correctivas/preventivas, seguimiento, aprobaciones y efectividad.",
        "form_fields": [
            {"id": "name", "label": "Nombre / titulo", "value": "NC empaque", "required": True},
            {"id": "code", "label": "Codigo", "value": "CAPA-2026-014", "required": True, "hotspot": True},
            {"id": "description", "label": "Descripcion", "value": "CAPA creada desde la UI Enterprise"},
        ],
    },
    "risks": {
        "label": "Risk Management",
        "desc": "Matriz 5x5, mapa de calor, tratamientos, controles y mitigaciones.",
        "form_fields": [
            {"id": "name", "label": "Nombre / titulo", "value": "Contaminación cruzada", "required": True},
            {"id": "code", "label": "Codigo", "value": "RSK-CC-001", "required": True, "hotspot": True},
            {"id": "area", "label": "Area", "value": "Quality"},
            {"id": "process", "label": "Proceso", "value": "Compliance"},
        ],
    },
    "indicators": {
        "label": "Quality Indicators",
        "desc": "KPIs, tendencias, metas, semaforizacion y desviaciones.",
        "form_fields": [
            {"id": "name", "label": "Nombre / titulo", "value": "Tasa NC", "required": True},
            {"id": "code", "label": "Codigo", "value": "KPI-NC-RATE", "required": True, "hotspot": True},
            {"id": "unit", "label": "Unidad", "value": "%"},
        ],
    },
}

# Formularios adicionales — réplica de app.js (create-tenant-form, tenant-*-form, enterprise-action-form)
FORMS = {
    "create_tenant": {
        "title": "Crear Tenant",
        "subtitle": "Alta enterprise de tenant. Al guardar se abrira automaticamente el Tenant Administration Center del nuevo tenant.",
        "submit": "Crear tenant",
        "fields": [
            {"id": "newTenantName", "label": "Tenant Name", "value": "Alimentos Premium Panama", "required": True, "hotspot": True,
             "help": "Nombre interno del tenant."},
            {"id": "newTenantSlug", "label": "Slug", "value": "alimentos-premium-pa", "required": True,
             "help": "Identificador URL-friendly. Debe ser unico."},
            {"id": "newTenantLegalName", "label": "Razon Social", "value": COMPANY["legal_name"], "required": True},
            {"id": "newTenantCommercialName", "label": "Nombre Comercial", "value": COMPANY["commercial_name"], "required": True},
            {"id": "newTenantTaxIdentifier", "label": "RUC / Tax ID", "value": COMPANY["tax_id"], "required": True},
            {"id": "newTenantCountryCode", "label": "Pais", "value": "PA", "type": "select",
             "options": [{"v": "PA", "l": "Panama (PA)"}, {"v": "US", "l": "United States (US)"}]},
            {"id": "newTenantCurrency", "label": "Moneda", "value": "USD", "type": "select",
             "options": [{"v": "USD", "l": "USD"}, {"v": "PAB", "l": "PAB"}]},
        ],
    },
    "tac_general": {
        "title": "Informacion General",
        "subtitle": "Campos empresariales editables con TenantId y CreatedAt inmutables.",
        "submit": "Guardar informacion general",
        "fields": [
            {"id": "name", "label": "Tenant Name", "value": COMPANY["commercial_name"], "required": True},
            {"id": "legalName", "label": "Razon Social", "value": COMPANY["legal_name"], "required": True, "hotspot": True},
            {"id": "commercialName", "label": "Nombre Comercial", "value": COMPANY["commercial_name"], "required": True},
            {"id": "taxIdentifier", "label": "RUC / Tax ID", "value": COMPANY["tax_id"], "required": True},
            {"id": "industry", "label": "Industria", "value": COMPANY["industry"], "required": True},
            {"id": "countryCode", "label": "Pais", "value": "PA", "type": "select",
             "options": [{"v": "PA", "l": "Panama (PA)"}]},
            {"id": "currency", "label": "Moneda", "value": "USD", "type": "select",
             "options": [{"v": "USD", "l": "USD"}]},
            {"id": "phone", "label": "Telefono", "value": "+507 6000-0000"},
            {"id": "email", "label": "Correo", "value": "contacto@alimentos-premium.test"},
            {"id": "website", "label": "Sitio Web", "value": "https://alimentos-premium.test"},
            {"id": "city", "label": "Ciudad", "value": "Panama"},
            {"id": "generalChangeReason", "label": "Motivo del cambio", "value": "", "help": "Opcional, queda en auditoria"},
        ],
    },
    "tac_branding": {
        "title": "Branding",
        "subtitle": "Identidad visual del tenant, sin estilos inline y con soporte dark mode.",
        "submit": "Guardar branding",
        "fields": [
            {"id": "displayName", "label": "Nombre mostrado", "value": COMPANY["commercial_name"], "required": True, "hotspot": True},
            {"id": "logoUri", "label": "Logo", "value": "https://empresa.com/logo.png"},
            {"id": "primaryColor", "label": "Color primario", "value": "#1769aa", "type": "color", "required": True},
            {"id": "secondaryColor", "label": "Color secundario", "value": "#0f766e", "type": "color", "required": True},
            {"id": "footerText", "label": "Pie de pagina", "value": "Compliance 360"},
        ],
    },
    "tac_security": {
        "title": "Seguridad",
        "subtitle": "MFA, password policy, sesiones, lockout, IP whitelist y trusted devices.",
        "submit": "Guardar seguridad",
        "fields": [
            {"id": "requireMfa", "label": "MFA obligatorio", "type": "checkbox", "value": False},
            {"id": "sessionTimeoutMinutes", "label": "Session Timeout (min)", "value": "60", "required": True, "hotspot": True},
            {"id": "lockoutMaxFailedAttempts", "label": "Lockout intentos", "value": "5", "required": True},
            {"id": "lockoutMinutes", "label": "Lockout minutos", "value": "15", "required": True},
            {"id": "securityChangeReason", "label": "Motivo del cambio", "value": "Configuracion demo"},
        ],
    },
    "tac_users": {
        "title": "User Administration",
        "subtitle": "Crear, bloquear, desbloquear, reset MFA, roles y sesiones por tenant.",
        "submit": "Crear / Invitar usuario",
        "fields": [
            {"id": "email", "label": "Email", "value": "doccontrol@alimentos-premium.test", "required": True, "hotspot": True},
            {"id": "fullName", "label": "Nombre completo", "value": "Document Controller Demo", "required": True},
            {"id": "initialPassword", "label": "Password inicial", "value": "••••••••••••", "type": "password", "required": True,
             "help": "Minimo 12 caracteres con mayuscula, minuscula, numero y simbolo."},
            {"id": "roleId", "label": "Rol inicial", "value": "Document Controller", "type": "select",
             "options": [{"v": "dc", "l": "Document Controller"}, {"v": "viewer", "l": "Viewer"}]},
            {"id": "changeReason", "label": "Motivo", "value": "Alta operativa TAC"},
        ],
    },
    "tac_rbac": {
        "title": "RBAC - Roles y Permisos",
        "subtitle": "Crear permiso y otorgarlo sin salir del navegador.",
        "submit": "Crear permiso y otorgar",
        "fields": [
            {"id": "permissionRoleId", "label": "Rol", "value": "Document Controller", "type": "select", "required": True, "hotspot": True,
             "options": [{"v": "dc", "l": "Document Controller"}]},
            {"id": "module", "label": "Modulo", "value": "TECHNICALSHEET", "required": True},
            {"id": "action", "label": "Accion", "value": "CREATE", "type": "select",
             "options": [{"v": "CREATE", "l": "Create"}, {"v": "READ", "l": "Read"}]},
            {"id": "description", "label": "Descripcion", "value": "Permiso creado desde TAC RBAC", "required": True},
        ],
    },
    "tac_domains": {
        "title": "Dominios",
        "subtitle": "Principal, secundarios, subdominios, aliases, DNS, certificados, HTTPS y redirecciones.",
        "submit": "Guardar dominio",
        "fields": [
            {"id": "hostName", "label": "Dominio", "value": "alimentos-premium.test", "required": True, "hotspot": True},
            {"id": "domainKind", "label": "Tipo", "value": "PrimaryCustom", "type": "select",
             "options": [{"v": "PrimaryCustom", "l": "PrimaryCustom"}]},
            {"id": "changeReason", "label": "Motivo", "value": "Alta de dominio enterprise"},
        ],
    },
    "enterprise_security": {
        "title": "Security",
        "submit": "Crear item enterprise",
        "fields": [
            {"id": "title", "label": "Titulo", "value": "Política acceso", "required": True, "hotspot": True},
            {"id": "code", "label": "Codigo", "value": "SEC-001", "required": True},
            {"id": "description", "label": "Descripcion", "value": "Controles de seguridad, revisiones de acceso, hardening y evidencias.", "required": True},
            {"id": "dueAtUtc", "label": "Vencimiento", "value": "2026-12-31", "type": "date"},
        ],
    },
}

SUPERADMIN_TABS = [
    ["executive", "Executive"], ["tenants", "Tenants"], ["licenses", "Licencias"], ["modules", "Modulos"],
    ["providers", "Providers"], ["security", "Seguridad"], ["observability", "Observability"],
    ["audit", "Auditoria"], ["database", "Database"], ["ai", "IA"], ["configuration", "Configuracion"],
    ["backups", "Backups"], ["devops", "DevOps"],
]

TAC_TABS_LIST = [
    ["general", "Informacion General"], ["branding", "Branding"], ["security", "Seguridad"],
    ["users", "Usuarios"], ["rbac", "Roles & Permisos"], ["licensing", "Licenciamiento"],
    ["domains", "Dominios"], ["sso", "SSO"], ["apikeys", "API Keys"], ["webhooks", "Webhooks"],
    ["storage", "Storage"], ["notifications", "Notificaciones"], ["health", "Health & Backups"],
    ["audit", "Auditoria"], ["state", "Estado"],
]

TAC_FORM_BY_TAB = {
    "general": "tac_general",
    "branding": "tac_branding",
    "security": "tac_security",
    "users": "tac_users",
    "rbac": "tac_rbac",
    "domains": "tac_domains",
}


def fields_table(fields: list[dict]) -> str:
    rows = "".join(
        f"<tr><td><strong>{f['label']}</strong></td>"
        f"<td><code>{f.get('value') or '—'}</code></td>"
        f"<td>{'Sí *' if f.get('required') else 'Opcional'}</td></tr>"
        for f in fields
    )
    return (
        '<table class="fields-table"><thead><tr>'
        "<th>Campo en pantalla (igual que la app)</th><th>Valor demo</th><th>Obligatorio</th>"
        f"</tr></thead><tbody>{rows}</tbody></table>"
    )


def coach_with_fields(button: str, why: str, after: str, fields: list[dict], **kwargs) -> str:
    return guide_btn(button, why, after, **kwargs) + fields_table(fields)


def step(
    sid: str,
    phase: str,
    route: str,
    title: str,
    coach: str,
    mock: dict,
) -> dict:
    return {"id": sid, "phase": phase, "route": route, "title": title, "coach": coach, "mock": mock}


def route_label(route_key: str) -> str:
    for group in NAVIGATION:
        for key, label in group["items"]:
            if key == route_key:
                return label
    return route_key.replace("-", " ").title()


def guide_btn(
    button: str,
    why: str,
    after: str,
    *,
    where: str = "",
    fill: str = "",
    steps: list[tuple[str, str]] | None = None,
    extra: str = "",
) -> str:
    """Coach para principiantes: qué botón, por qué, qué sigue."""
    loc = (
        f" en <strong>{where}</strong>"
        if where
        else " resaltado con <strong>anillo dorado</strong> en la pantalla izquierda"
    )
    html = [
        '<div class="coach-guide">',
        f'<div class="coach-block what"><h5>1 · Qué presionar</h5>'
        f'<p>Pulse <span class="btn-ref">{button}</span>{loc}.</p></div>',
        f'<div class="coach-block why"><h5>2 · Por qué</h5><p>{why}</p></div>',
    ]
    if fill:
        html.append(
            f'<div class="coach-block where"><h5>Antes de pulsar — complete</h5><p>{fill}</p></div>'
        )
    if steps:
        steps_html = "".join(
            f'<div class="action-step{" active" if i == 0 else ""}">'
            f'<span class="action-num">{i + 1}</span><strong>{label}</strong>: {detail}</div>'
            for i, (label, detail) in enumerate(steps)
        )
        html.append(f'<div class="coach-block where"><h5>Datos a ingresar</h5>{steps_html}</div>')
    html.append(
        f'<div class="coach-block next"><h5>3 · Qué pasa después</h5><p>{after}</p></div>'
    )
    if extra:
        html.append(extra)
    html.append("</div>")
    return "".join(html)


TAC_TABS = {
    "general": "Información General",
    "branding": "Branding",
    "security": "Seguridad",
    "users": "Usuarios",
    "rbac": "Roles & Permisos",
    "domains": "Dominios",
    "state": "Estado",
}

TAC_COACH = {
    "general": (
        "Guardar informacion general",
        "Registra la razón social, país y datos legales del tenant. Sin esto el tenant no puede operar con datos correctos.",
        "Los datos quedan guardados. Puede continuar con branding, usuarios o activación.",
    ),
    "branding": (
        "Guardar branding",
        "Personaliza logo y colores que verán los usuarios al iniciar sesión.",
        "El login y la interfaz muestran la identidad de su empresa.",
    ),
    "security": (
        "Guardar seguridad",
        "Define políticas como intentos fallidos de login (lockout) y reglas de acceso.",
        "Las políticas se aplican a todos los usuarios del tenant de inmediato.",
    ),
    "users": (
        "Crear / Invitar usuario",
        "Agrega personas al tenant y les asigna un rol (Document Controller, Viewer, etc.).",
        "El usuario recibe acceso y puede iniciar sesión con su correo corporativo.",
    ),
    "rbac": (
        "Otorgar permiso",
        "Da permisos extra a un rol (ej. TECHNICALSHEET.CREATE) que no vienen por defecto.",
        "El usuario afectado debe cerrar sesión y volver a entrar para ver el nuevo acceso.",
    ),
    "domains": (
        "Guardar dominio",
        "Vincula un dominio corporativo para login SSO o resolución automática de organización.",
        "El dominio queda registrado; la validación DNS puede quedar pendiente en entornos demo.",
    ),
    "state": (
        "Activar",
        "Pone el tenant en estado <strong>Active</strong> para que los usuarios operen módulos de negocio.",
        "El tenant queda operativo. Handoff a Storage/Notification Admin si faltan proveedores.",
    ),
}


def tac_coach(tab: str, extra: str = "") -> str:
    btn, why, after = TAC_COACH[tab]
    where = f"pestaña «{TAC_TABS[tab]}» del TAC"
    if tab == "state":
        return (
            guide_btn(btn, why, after, where=where, extra=extra)
            + '<table class="fields-table"><thead><tr><th>Botón en pantalla</th><th>Acción</th></tr></thead>'
            "<tbody><tr><td><strong>Activar</strong></td><td>Pasa tenant a Active</td></tr>"
            "<tr><td>Mover a Trial</td><td>Estado trial</td></tr>"
            "<tr><td>Suspender / Archivar</td><td>Solo admin con permiso</td></tr></tbody></table>"
        )
    form_key = TAC_FORM_BY_TAB.get(tab)
    if form_key:
        return coach_with_fields(btn, why, after, FORMS[form_key]["fields"], where=where, extra=extra)
    return guide_btn(btn, why, after, where=where, extra=extra)


def superadmin_coach(tab: str, *, filled: bool = False, extra: str = "") -> str:
    if tab == "executive":
        return guide_btn(
            "Executive",
            "Muestra KPIs globales: cuántos tenants, usuarios y alertas hay en la plataforma.",
            "Use las otras pestañas para crear tenants o revisar seguridad.",
            where="pestaña 1 del SuperAdmin Platform Center",
            extra=extra,
        )
    if tab == "tenants":
        fields = list(FORMS["create_tenant"]["fields"])
        if not filled:
            fields[0] = {**fields[0], "value": "Cliente Demo Compliance"}
            fields[1] = {**fields[1], "value": "cliente-demo-compliance"}
        return coach_with_fields(
            "Crear tenant",
            "Provisiona un cliente nuevo: complete el formulario igual que en la app (#create-tenant-form).",
            "El tenant aparece en la flota y redirige al TAC automáticamente.",
            fields,
            where="pestaña Tenants → formulario Crear Tenant",
            extra=extra,
        )
    if tab == "security":
        return guide_btn(
            "Seguridad",
            "Configura políticas de seguridad a nivel plataforma (SSO global, OAuth, etc.).",
            "Los cambios aplican a toda la SaaS, no a un tenant individual.",
            where="pestaña Seguridad del SuperAdmin",
            extra=extra,
        )
    if tab == "audit":
        return guide_btn(
            "Auditoria",
            "Consulta quién hizo qué a nivel plataforma — trazabilidad para compliance.",
            "Puede filtrar eventos y exportar CSV si tiene permiso.",
            where="pestaña Auditoria del SuperAdmin",
            extra=extra,
        )
    if tab == "observability":
        return guide_btn(
            "Observability",
            "Revisa salud técnica: métricas, latencia y estado de servicios.",
            "Sirve para detectar problemas antes de que afecten clientes.",
            where="pestaña Observability del SuperAdmin",
            extra=extra,
        )
    return guide_btn(
        tab.title(),
        f"Panel {tab} del centro de gobierno de plataforma.",
        "Continúe con Siguiente → para el siguiente paso del recorrido.",
        where="SuperAdmin Platform Center",
        extra=extra,
    )


def reports_coach(action: str) -> str:
    meta = {
        "seed": (
            "Seed standard reports",
            "Carga plantillas de reportes estándar (compliance, ejecutivo) en el catálogo del tenant.",
            "Los reportes aparecen listos para ejecutar en la tabla del Report Center.",
        ),
        "execute": (
            "Ejecutar reporte",
            "Genera el reporte con datos actuales del tenant (documentos, CAPA, riesgos…).",
            "Verá el resultado en pantalla o en historial de ejecuciones.",
        ),
        "export": (
            "Exportar via reportes",
            "Descarga el reporte en PDF/Excel para compartir con auditoría o gerencia.",
            "El archivo queda en su equipo; la acción queda registrada en Audit Trail.",
        ),
    }
    btn, why, after = meta[action]
    return guide_btn(btn, why, after, where="Report Center → botón principal")


def config_coach(panel: str) -> str:
    if panel == "storage":
        return guide_btn(
            "Crear Storage Local",
            "Configura dónde se guardan archivos y documentos adjuntos del tenant.",
            "El tenant puede subir evidencias. Pruebe la conexión en el paso siguiente.",
            where="tarjeta Storage Providers",
        )
    if panel == "storage_test":
        return guide_btn(
            "Probar primer Storage Provider",
            "Verifica que el almacenamiento responde antes de que usuarios suban archivos reales.",
            "Si pasa la prueba, el provider queda listo. Si falla, revise la configuración.",
            where="debajo del provider creado",
        )
    if panel == "email":
        return guide_btn(
            "Crear Email SMTP",
            "Configura el servidor de correo para invitaciones, alertas y notificaciones.",
            "En desarrollo puede quedar PENDING si no hay SMTP real (Mailhog).",
            where="tarjeta Email Providers",
            extra='<div class="warn-box">SMTP de producción = tercero externo; marque PENDING, no error.</div>',
        )
    return guide_btn(
        "Crear Storage Local / Crear Email SMTP",
        "Cada tenant necesita almacenamiento y correo antes de operar con adjuntos e invitaciones.",
        "Handoff: Storage Admin configura storage; Notification Admin configura SMTP.",
        where="Configuration → Provider Administration",
    )


def api_coach(topic: str) -> str:
    topics = {
        "support-access": (
            "Swagger / DevTools",
            "Support Operator accede a datos de tenant vía API con PLATFORM.SUPPORT.ACCESS — no hay pantalla break-glass.",
            "Compare: Platform Admin recibe 403; Support con token correcto recibe 200. Todo queda auditado.",
        ),
        "document-approve": (
            "Swagger — POST decision",
            "Quality Manager aprueba documentos pero no los crea (SoD). La UI aún no tiene botón dedicado.",
            "En Swagger: POST /documents/{id}/decision con Bearer token de quality@. Documento pasa a Approved.",
        ),
        "capa-approve": (
            "Swagger — cerrar CAPA",
            "Solo Quality Manager puede cerrar/aprobar CAPA; CAPA Manager la prepara pero no cierra.",
            "POST al endpoint de decisión CAPA. Estado cambia a cerrado; evento en Audit Trail.",
        ),
        "viewer-403": (
            "No pulse Crear — prueba en Swagger",
            "Viewer solo lee. Intentar crear debe fallar para proteger integridad de datos.",
            "POST create vía API → HTTP 403 AuthorizationDenied. En UI no verá botón Crear registro real.",
        ),
    }
    default = (
        "Consulte manual MFT",
        "Esta acción no tiene pantalla UI en la versión actual.",
        "Use Swagger con token Bearer del rol indicado en el manual interactivo.",
    )
    btn, why, after = topics.get(topic, default)
    return guide_btn(btn, why, after, where="Swagger UI o DevTools → Network", extra=
        '<div class="warn-box">En la app real pida ayuda a su admin la primera vez.</div>')


def intro(role: dict, extra: str = "") -> dict:
    email_line = (
        f'<code>{role["email"]}</code>'
        if role.get("email")
        else "<em>Sin usuario E2E — asigne el rol en TAC o use admin@ como referencia visual.</em>"
    )
    notes = f'<div class="warn-box">{extra}</div>' if extra else ""
    start = guide_btn(
        "Siguiente →",
        f"Esta guía le enseña, paso a paso, las tareas de <strong>{role['name']}</strong> "
        "sin necesidad de conocer Compliance 360 antes.",
        "Verá pantallas iguales a la app real. Los <strong>anillos dorados</strong> marcan dónde hacer clic. "
        "Use el panel derecho en cada paso: qué botón, por qué y qué sigue.",
        where="barra superior derecha de esta guía (no en la pantalla simulada)",
    )
    return step(
        "intro",
        "Introducción",
        "#/selector",
        f"Rol: {role['name']}",
        f"""{start}
        <div class="script-card"><strong>{role['name']}</strong> — {role['purpose']}</div>
        <div class="role-card"><h4>Identidad</h4>Email: {email_line}<br>
        Ámbito: <strong>{role['scope']}</strong><br>
        Ruta ancla: <code>{role['landing_route']}</code><br>
        Menú: {role['menu_focus']}</div>
        <div class="role-card"><h4>Permisos clave</h4>{', '.join(role['permissions'][:4])}{'…' if len(role['permissions'])>4 else ''}</div>
        <div class="role-card"><h4>Handoffs</h4>{', '.join(role['handoffs']) or '—'}</div>
        {notes}
        <div class="warn-box">Contraseña demo: <code>e2e/testdata.json</code> — pídala a IT si no la tiene.</div>""",
        {"type": "intro", "roleName": role["name"], "scope": role["scope"]},
    )


def login_block(role: dict) -> list[dict]:
    email = role.get("email") or "admin@compliance360.local"
    org = PLATFORM_ORG if role["scope"] == "platform" else TENANT_ORG
    org_desc = (
        "Tenant plataforma · gobierno global"
        if role["scope"] == "platform"
        else f"Tenant operaciones · {COMPANY['slug']}"
    )
    return [
        step(
            "login-email",
            "Login v2 · Paso 1",
            "#/login",
            "Correo corporativo — Siguiente",
            guide_btn(
                "Siguiente",
                "El sistema identifica a qué organización(es) pertenece usted. No necesita escribir el ID del tenant.",
                "Aparece la lista de organizaciones disponibles para su correo.",
                where="debajo del campo Correo electrónico",
                fill=f'Escriba su correo corporativo: <code>{email}</code>',
            ),
            {"type": "login", "step": "email", "email": email},
        ),
        step(
            "login-org",
            "Login v2 · Paso 2",
            "#/login",
            "Organización — Continuar",
            guide_btn(
                "Continuar",
                f"Confirma en qué empresa/tenant va a trabajar hoy. Debe elegir <strong>{org}</strong>.",
                "Pasa a la pantalla de contraseña con la organización ya seleccionada.",
                where="tarjeta de organización resaltada → botones inferiores",
                fill=f'Seleccione la fila: <strong>{org}</strong> ({org_desc})',
            ),
            {"type": "login", "step": "organization", "org": org, "orgDesc": org_desc},
        ),
        step(
            "login-password",
            "Login v2 · Paso 3",
            "#/login",
            "Contraseña — Iniciar sesion",
            guide_btn(
                "Iniciar sesion",
                "Valida su identidad y abre la sesión en el tenant elegido.",
                "Entra al shell principal: menú lateral, barra superior y módulo de trabajo. "
                "Verá un mensaje de éxito en la app real.",
                where="botón azul inferior derecho",
                fill='Escriba su contraseña (demo: <code>e2e/testdata.json</code>). '
                "Opcional: marque «Recordarme en este dispositivo».",
            ),
            {"type": "login", "step": "password", "org": org},
        ),
    ]


def shell_step(role: dict, active: str, coach_extra: str) -> dict:
    chip = "dc7c46ee" if role["scope"] == "platform" else "ddcaf211"
    label = route_label(active)
    coach = guide_btn(
        label,
        f"Es su pantalla de trabajo como <strong>{role['name']}</strong>. "
        "El menú lateral muestra <em>solo</em> los módulos que su rol puede abrir.",
        f"En la app real: clic en «{label}» en el sidebar. "
        f"Aquí pulse <strong>Siguiente →</strong> para continuar el recorrido guiado.",
        where="menú lateral izquierdo (sidebar)",
        extra=coach_extra,
    )
    return step(
        "shell",
        "Post-login",
        f"#/{active}",
        "Shell — qué verás al entrar",
        coach,
        {"type": "shell", "activeRoute": active, "tenantChip": chip},
    )


def module_list(role: dict, mod_key: str, readonly: bool = False) -> dict:
    m = MODULES[mod_key]
    mode = "readonly" if readonly else "list"
    label = route_label(mod_key)
    if readonly:
        coach = guide_btn(
            label,
            f"Su rol puede <strong>consultar</strong> {m['label']} pero no crear registros (SoD o permiso READ).",
            "Verá el aviso «Modo solo lectura» y no aparecerá el botón Crear registro real.",
            where=f"sidebar → {label}",
            extra='<div class="warn-box">Si necesita crear, pida permiso a Tenant Administrator.</div>',
        )
    else:
        coach = guide_btn(
            label,
            f"Desde aquí gestiona el listado de {m['label'].lower()}. Es el punto de entrada diario de su rol.",
            "En el siguiente paso verá el formulario para crear un registro nuevo.",
            where=f"sidebar → Operations → {label}",
        )
    return step(
        f"module-{mod_key}-list",
        m["label"],
        f"#/{mod_key}",
        f"{m['label']} — listado",
        f'<div class="script-card">{m["desc"]}</div>{coach}',
        {"type": "module", "module": mod_key, "mode": mode},
    )


def module_create(role: dict, mod_key: str) -> dict:
    m = MODULES[mod_key]
    coach = coach_with_fields(
        "Crear registro real",
        f"Complete el formulario <code>#module-action-form</code> con los mismos campos que la app.",
        "Pulse Crear registro real → el registro aparece en la tabla y queda en Audit Trail.",
        m["form_fields"],
        where="Action Center → formulario inferior",
    )
    return step(
        f"module-{mod_key}-create",
        m["label"],
        f"#/{mod_key}",
        "Crear registro real",
        coach,
        {"type": "module", "module": mod_key, "mode": "create"},
    )


def audit_trail_coach(event: str = "DocumentCreated") -> str:
    return guide_btn(
        "Audit Trail",
        "Permite demostrar quién hizo qué y cuándo — requisito clave en auditorías ISO/BPM.",
        f"Busque el evento <code>{event}</code> de su usuario. Confirma que la acción quedó registrada.",
        where="sidebar → Command Center → Audit Trail",
    )


def finish(role: dict, items: list[str]) -> dict:
    lis = "".join(f'<label><input type="checkbox" checked disabled> {x}</label>' for x in items)
    coach = guide_btn(
        "Siguiente → (o repita el flujo)",
        f"Completó el recorrido visual de <strong>{role['name']}</strong>. "
        "Ahora debe practicar en la app real con las mismas secuencias.",
        f"Abra <code>{APP_URL}</code>, repita login y ejecute las mismas acciones. "
        "Si algo no coincide, consulte a su administrador.",
        where="barra superior — o reinicie eligiendo otro rol",
    )
    return step(
        "finish",
        "Cierre",
        role["landing_route"],
        f"Checklist — {role['name']}",
        f"""{coach}
        <div class="script-card">Marque mentalmente cada logro:</div>
        <div style="display:grid;gap:.45rem;font-size:.88rem">{lis}</div>""",
        {"type": "finish", "roleName": role["name"], "items": items},
    )


ROLE_MODULE_MAP = {
    "document-controller": "documents",
    "auditor": "audits",
    "supplier-manager": "suppliers",
    "capa-manager": "capa",
    "risk-manager": "risks",
}


def build_flow(role: dict) -> list[dict]:
    k = role["key"]
    flows: list[dict] = [intro(role, role.get("notes", ""))]

    if k == "platform-administrator":
        flows += login_block(role)
        flows += [
            shell_step(role, "superadmin-platform", ""),
            step("sa-exec", "Platform Admin", "#/superadmin-platform", "SuperAdmin — Executive",
                 superadmin_coach("executive"),
                 {"type": "superadmin", "tab": "executive"}),
            step("sa-tenants", "Crear tenant", "#/superadmin-platform", "Tab Tenants — Crear Tenant",
                 superadmin_coach("tenants"),
                 {"type": "superadmin", "tab": "tenants", "filled": False}),
            step("sa-tenants-fill", "Crear tenant", "#/superadmin-platform", "Ejemplo Alimentos Premium",
                 superadmin_coach("tenants", filled=True,
                                  extra='<div class="script-card">Ejemplo alineado con e2e_provision.ps1</div>'),
                 {"type": "superadmin", "tab": "tenants", "filled": True}),
            step("tac-nav", "TAC", "#/tenant-administration", "Redirección al TAC",
                 guide_btn(
                     "Tenant Administration",
                     "Tras crear un tenant, debe configurarlo antes de que usuarios operen.",
                     "Abre el Tenant Administration Center (TAC) con pestañas numeradas.",
                     where="sidebar Enterprise o redirección automática post-create",
                 ),
                 {"type": "tac", "tab": "general"}),
            step("tac-general", "TAC", "#/tenant-administration", "Información General",
                 tac_coach("general"),
                 {"type": "tac", "tab": "general"}),
            step("tac-users", "TAC", "#/tenant-administration", "Usuarios y RBAC",
                 tac_coach("users", extra=
                     '<div class="warn-box">Invite especialistas y otorgue TECHNICALSHEET.CREATE si usará fichas técnicas.</div>'),
                 {"type": "tac", "tab": "users"}),
            step("tac-state", "TAC", "#/tenant-administration", "Activar tenant",
                 tac_coach("state"),
                 {"type": "tac", "tab": "state"}),
            step("config-next", "Post-config", "#/configuration", "Storage y SMTP",
                 config_coach("both"),
                 {"type": "configuration", "panel": "both"}),
        ]
        flows.append(finish(role, ["Login Platform Admin", "SuperAdmin revisado", "Tenant creado", "TAC configurado", "Tenant activado"]))

    elif k == "tenant-administrator":
        flows += login_block(role)
        flows += [
            shell_step(role, "tenant-administration", ""),
            step("tac-gen", "TAC", "#/tenant-administration", "Información General",
                 tac_coach("general"),
                 {"type": "tac", "tab": "general"}),
            step("tac-brand", "TAC", "#/tenant-administration", "Branding",
                 tac_coach("branding"),
                 {"type": "tac", "tab": "branding"}),
            step("tac-sec", "TAC", "#/tenant-administration", "Seguridad",
                 tac_coach("security"),
                 {"type": "tac", "tab": "security"}),
            step("tac-users", "TAC", "#/tenant-administration", "Usuarios",
                 tac_coach("users"),
                 {"type": "tac", "tab": "users"}),
            step("tac-rbac", "TAC", "#/tenant-administration", "RBAC",
                 tac_coach("rbac", extra=
                     '<div class="warn-box">Otorgue <code>TECHNICALSHEET.CREATE</code> si el tenant usará fichas técnicas.</div>'),
                 {"type": "tac", "tab": "rbac"}),
            step("tac-state", "TAC", "#/tenant-administration", "Activar",
                 tac_coach("state"),
                 {"type": "tac", "tab": "state"}),
        ]
        flows.append(finish(role, ["Login TAC", "Datos empresa", "Usuarios creados", "RBAC OK", "Tenant Active"]))

    elif k in ("platform-operations", "platform-security"):
        proxy = 'Use <code>admin@compliance360.local</code> con rol asignado manualmente para practicar en app real.'
        flows[0] = intro(role, proxy)
        flows += login_block({**role, "email": "admin@compliance360.local"})
        tab = "tenants" if k == "platform-operations" else "security"
        flows += [
            shell_step(role, "superadmin-platform", ""),
            step("sa-tab", role["name"], "#/superadmin-platform", f"Tab {tab.title()}",
                 superadmin_coach(tab, extra=
                     '<div class="script-card">Alcance operativo limitado vs Platform Administrator.</div>'),
                 {"type": "superadmin", "tab": tab}),
            step("sa-obs", role["name"], "#/superadmin-platform", "Observabilidad / Auditoría",
                 superadmin_coach("observability" if k == "platform-operations" else "audit"),
                 {"type": "superadmin", "tab": "observability" if k == "platform-operations" else "audit"}),
        ]
        flows.append(finish(role, ["Rol verificado en catálogo", "SuperAdmin accesible", "Alcance SoD respetado"]))

    elif k == "support-operator":
        flows += login_block(role)
        flows += [
            shell_step(role, "superadmin-platform",
                       '<div class="warn-box">No existe pantalla break-glass dedicada — acceso tenant vía API.</div>'),
            step("sa-lim", "Support", "#/superadmin-platform", "Vista restringida",
                 superadmin_coach("executive", extra=
                     '<div class="script-card">Vista gobierno plataforma — sin Crear tenant.</div>'),
                 {"type": "superadmin", "tab": "executive"}),
            step("api-note", "Support", "API", "Acceso tenant vía API",
                 api_coach("support-access"),
                 {"type": "api_note", "topic": "support-access"}),
            step("audit", "Support", "#/audit-trail", "Audit Trail",
                 audit_trail_coach("SupportAccessGranted"),
                 {"type": "audit_trail"}),
        ]
        flows.append(finish(role, ["Login support@", "SuperAdmin OK", "Sin Crear tenant", "API auditada"]))

    elif k == "tenant-security-administrator":
        flows += login_block(role)
        flows += [
            shell_step(role, "security", ""),
            step("sec-ws", "Security", "#/security", "Workspace Security",
                 coach_with_fields(
                     "Crear item enterprise",
                     "Formulario <code>#enterprise-action-form</code> — mismos campos que Security en la app.",
                     "El ítem queda en el workspace Security para revisión y auditoría.",
                     FORMS["enterprise_security"]["fields"],
                     where="Enterprise → Security → Action Center",
                 ),
                 {"type": "enterprise", "workspace": "security"}),
            step("tac-sec", "TAC", "#/tenant-administration", "Políticas seguridad",
                 tac_coach("security", extra=
                     '<div class="script-card">Configure lockout y políticas desde TAC también.</div>'),
                 {"type": "tac", "tab": "security"}),
            step("tac-dom", "TAC", "#/tenant-administration", "Dominios",
                 tac_coach("domains"),
                 {"type": "tac", "tab": "domains"}),
        ]
        flows.append(finish(role, ["Login security@", "Políticas guardadas", "Dominio configurado"]))

    elif k == "quality-manager":
        flows += login_block(role)
        flows += [
            module_list(role, "documents", readonly=True),
            step("qm-api-doc", "Quality Manager", "Swagger", "Aprobar documento (API)",
                 api_coach("document-approve"),
                 {"type": "api_note", "topic": "document-approve"}),
            module_list(role, "capa", readonly=True),
            step("qm-api-capa", "Quality Manager", "Swagger", "Cerrar CAPA (API)",
                 api_coach("capa-approve"),
                 {"type": "api_note", "topic": "capa-approve"}),
        ]
        flows.append(finish(role, ["Login quality@", "SoD create OK", "Documento aprobado API", "CAPA cerrada API"]))

    elif k == "viewer":
        flows += login_block(role)
        flows += [
            shell_step(role, "dashboard", ""),
            module_list(role, "documents", readonly=True),
            module_list(role, "capa", readonly=True),
            module_list(role, "risks", readonly=True),
            step("vw-api", "Viewer", "API", "POST create → 403",
                 api_coach("viewer-403"),
                 {"type": "api_note", "topic": "viewer-403"}),
        ]
        flows.append(finish(role, ["Login viewer@", "Modo solo lectura en módulos", "API bloqueada"]))

    elif k == "reporting-manager":
        flows += login_block(role)
        flows += [
            shell_step(role, "reports", ""),
            step("rp-seed", "Reports", "#/reports", "Seed standard reports",
                 reports_coach("seed"),
                 {"type": "reports", "action": "seed"}),
            step("rp-exec", "Reports", "#/reports", "Ejecutar reporte",
                 reports_coach("execute"),
                 {"type": "reports", "action": "execute"}),
            step("rp-exp", "Reports", "#/reports", "Exportar",
                 reports_coach("export"),
                 {"type": "reports", "action": "export"}),
            step("rp-dash", "Reports", "#/compliance", "Compliance Dashboard",
                 guide_btn(
                     "Compliance Dashboard",
                     "Muestra KPIs agregados para gerencia — valida que los reportes reflejan datos reales.",
                     "Revise métricas de compliance y compare con el reporte exportado.",
                     where="sidebar → Command Center → Compliance Dashboard",
                 ),
                 {"type": "dashboard", "variant": "compliance"}),
        ]
        flows.append(finish(role, ["Login reporting@", "Reports seeded", "Reporte ejecutado", "Export OK"]))

    elif k == "storage-administrator":
        flows += login_block(role)
        flows += [
            shell_step(role, "configuration", ""),
            step("st-create", "Storage", "#/configuration", "Crear Storage Local",
                 config_coach("storage"),
                 {"type": "configuration", "panel": "storage"}),
            step("st-test", "Storage", "#/configuration", "Probar Storage Provider",
                 config_coach("storage_test"),
                 {"type": "configuration", "panel": "storage_test"}),
        ]
        flows.append(finish(role, ["Login storage@", "Storage Local OK", "Test provider OK"]))

    elif k == "notification-administrator":
        flows += login_block(role)
        flows += [
            shell_step(role, "configuration", ""),
            step("na-smtp", "Notifications", "#/configuration", "Crear Email SMTP",
                 config_coach("email"),
                 {"type": "configuration", "panel": "email"}),
        ]
        flows.append(finish(role, ["Login notifications@", "SMTP configurado o PENDING"]))

    elif k == "indicators-manager":
        flows += login_block(role)
        mod = "indicators"
        flows += [
            shell_step(role, mod, ""),
            module_list(role, mod),
            module_create(role, mod),
            step("im-measure", "Indicators", "#/indicators", "Registrar medición",
                 guide_btn(
                     "Agregar medición",
                     "Un indicador sin mediciones no sirve — registra el valor del periodo (mes, trimestre…).",
                     "La medición alimenta dashboards y reportes de compliance.",
                     where="detalle del indicador → Action Center",
                 ),
                 {"type": "module", "module": mod, "mode": "detail"}),
            step("im-dash", "Indicators", "#/dashboard", "Dashboard KPI",
                 guide_btn(
                     "Executive Dashboard",
                     "Confirma que el KPI que registró se refleja en las métricas ejecutivas.",
                     "Si no aparece, revise umbrales del indicador o espere sincronización.",
                     where="sidebar → Command Center → Executive Dashboard",
                 ),
                 {"type": "dashboard", "variant": "executive"}),
        ]
        flows.append(finish(role, ["Login indicators@", "KPI creado", "Medición registrada"]))

    elif k in ROLE_MODULE_MAP:
        mod = ROLE_MODULE_MAP[k]
        flows += login_block(role)
        flows += [shell_step(role, mod, "")]
        flows.append(module_list(role, mod))
        flows.append(module_create(role, mod))
        if k == "document-controller":
            flows.append(module_list(role, "technical-sheets", readonly=True))
        if k == "auditor":
            flows.append(step("au-sod", "Auditor", "#/capa", "SoD — no cierra CAPA",
                              guide_btn(
                                  "CAPA (solo lectura)",
                                  "Auditor puede ver CAPA vinculadas a hallazgos pero no cerrarlas (SoD).",
                                  "El cierre lo hace Quality Manager. Usted documenta hallazgos en auditorías.",
                                  where="sidebar → CAPA",
                                  extra='<div class="warn-box">No pulse acciones de cierre — su rol no lo permite.</div>',
                              ),
                              {"type": "module", "module": "capa", "mode": "readonly"}))
        if k == "capa-manager":
            flows.append(step("cm-wf", "CAPA", "#/capa", "PendingApproval",
                              guide_btn(
                                  "Avanzar workflow",
                                  "CAPA Manager lleva la acción hasta PendingApproval pero no la cierra.",
                                  "Quality Manager aprueba/cierra vía API. Usted verifica estado en Audit Trail.",
                                  where="detalle CAPA → botones de workflow",
                              ),
                              {"type": "module", "module": "capa", "mode": "workflow"}))
        if k == "risk-manager":
            flows.append(step("rm-wf", "Risk", "#/risks", "Submit aprobación QM",
                              guide_btn(
                                  "Submit / Enviar a aprobación",
                                  "Registra el riesgo y lo envía al Quality Manager para aprobación (SoD).",
                                  "QM aprueba vía API. El riesgo queda en estado pendiente hasta entonces.",
                                  where="detalle del riesgo → acción Submit",
                              ),
                              {"type": "module", "module": "risks", "mode": "workflow"}))
        flows.append(step("audit-trail", role["name"], "#/audit-trail", "Audit Trail",
                          audit_trail_coach(),
                          {"type": "audit_trail"}))
        tc_names = [tc["name"] for tc in role["test_cases"][:3]]
        flows.append(finish(role, ["Login OK"] + tc_names))

    else:
        flows += login_block(role)
        flows.append(shell_step(role, role["landing_route"].replace("#/", ""), ""))
        flows.append(finish(role, [f"Flujo {role['name']} completado"]))

    return flows


# Mirrors app.js routePermissions + navigation (shell sidebar filtering).
ROUTE_PERMISSIONS: dict[str, list[str]] = {
    "dashboard": ["TENANT.READ"],
    "compliance": ["TENANT.READ"],
    "reports": ["REPORT.READ", "REPORT.EXECUTE", "REPORT.MANAGE"],
    "audit-trail": ["AUDIT.READ", "TENANT.AUDIT"],
    "documents": ["DOCUMENT.READ", "DOCUMENT.CREATE", "DOCUMENT.UPDATE", "DOCUMENT.APPROVE"],
    "technical-sheets": [
        "TECHNICALSHEET.READ",
        "TECHNICALSHEET.CREATE",
        "TECHNICALSHEET.UPDATE",
        "TECHNICALSHEET.APPROVE",
    ],
    "suppliers": ["SUPPLIER.READ", "SUPPLIER.CREATE", "SUPPLIER.UPDATE", "SUPPLIER.APPROVE"],
    "audits": ["AUDITMANAGEMENT.READ", "AUDITMANAGEMENT.MANAGE"],
    "capa": ["CAPA.READ", "CAPA.MANAGE", "CAPA.APPROVE"],
    "risks": ["RISK.READ", "RISK.MANAGE", "RISK.APPROVE"],
    "indicators": ["INDICATOR.READ", "INDICATOR.MANAGE"],
    "superadmin-platform": ["PLATFORM.DASHBOARD.READ"],
    "tenant-administration": [
        "PLATFORM.TENANT.READ",
        "TENANT.USERS",
        "TENANT.ROLES",
        "TENANT.UPDATE",
    ],
    "template-builder": ["TENANT.UPDATE"],
    "regulatory": ["TENANT.READ"],
    "training": ["TENANT.READ"],
    "supplier-portal": ["SUPPLIER.READ", "SUPPLIER.CREATE", "SUPPLIER.UPDATE", "SUPPLIER.APPROVE"],
    "customer-portal": ["TENANT.READ"],
    "security": ["TENANT.SECURITY"],
    "configuration": [
        "TENANT.STORAGE",
        "STORAGE.READ",
        "TENANT.NOTIFICATIONS",
        "NOTIFICATION.READ",
        "NOTIFICATION.ADMIN",
    ],
}

NAVIGATION: list[dict] = [
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
]


def _parse_permission_constants() -> dict[str, str]:
    text = (ROOT / "src/Compliance360.Domain/Identity/RbacCatalog.cs").read_text(encoding="utf-8")
    return dict(re.findall(r'public const string (\w+) = "([^"]+)";', text))


def _parse_role_permissions_by_name() -> dict[str, frozenset[str]]:
    perm_const = _parse_permission_constants()
    cs = (ROOT / "src/Compliance360.Domain/Identity/RoleCatalog.cs").read_text(encoding="utf-8")
    role_names = dict(re.findall(r'public const string (\w+) = "([^"]+)";', cs.split("Definitions")[0]))
    roles: dict[str, frozenset[str]] = {}
    for m in re.finditer(
        r"new\(\s*(\w+),\s*\1,\s*RoleScope\.\w+,\s*\"[^\"]*\",\s*\[(.*?)\]\s*\)",
        cs,
        re.DOTALL,
    ):
        display = role_names.get(m.group(1), m.group(1))
        codes = [
            perm_const[t]
            for t in re.findall(r"P\.(\w+)", m.group(2))
            if t in perm_const
        ]
        roles[display] = frozenset(codes)
    return roles


ROLE_PERMISSIONS_BY_NAME = _parse_role_permissions_by_name()


def can_navigate(route: str, permissions: set[str]) -> bool:
    reqs = ROUTE_PERMISSIONS.get(route)
    if not reqs:
        return True
    return any(p in permissions for p in reqs)


def compute_allowed_navigation(permissions: set[str]) -> list[dict]:
    groups: list[dict] = []
    for group in NAVIGATION:
        items = [[route, label] for route, label in group["items"] if can_navigate(route, permissions)]
        if items:
            groups.append({"g": group["group"], "items": items})
    return groups


def build_all_flows() -> dict:
    return {
        r["key"]: {
            "meta": {
                "key": r["key"],
                "name": r["name"],
                "scope": r["scope"],
                "email": r.get("email"),
                "landing": r["landing_route"],
                "allowedNavigation": compute_allowed_navigation(
                    set(ROLE_PERMISSIONS_BY_NAME.get(r["name"], frozenset()))
                ),
            },
            "steps": build_flow(r),
        }
        for r in ROLES
    }


ROLE_FLOWS = build_all_flows()

# Read JS template from admin guide - we'll embed a self-contained renderer
JS_RENDER = r"""
function esc(s){const d=document.createElement('div');d.textContent=s??'';return d.innerHTML;}
const DEMO={company:__COMPANY__,tenants:__TENANTS__,appUrl:__APP_URL__};
const __FORMS__=__FORMS_JSON__;
const __SUPERADMIN_TABS__=__SUPERADMIN_TABS_JSON__;
const __TAC_TABS__=__TAC_TABS_JSON__;

function loginMock(step,ctx){
  const email=ctx.email||'user@example.test';
  const org=ctx.org||'Alimentos Premium';
  const orgDesc=ctx.orgDesc||'Tenant operaciones';
  const blocks={
    email:`<div class="field hotspot-ring"><label for="email">Correo electronico</label><input id="email" name="email" type="email" value="${esc(email)}" readonly></div><button class="btn primary hotspot-ring" type="button">Siguiente</button>`,
    organization:`<div class="org-list" role="radiogroup" aria-label="Selecciona tu organizacion"><label class="org-option active hotspot-ring"><input type="radio" name="organizationId" checked disabled><span class="org-logo">${org.slice(0,2).toUpperCase()}</span><span><strong>${esc(org)}</strong><small>${esc(orgDesc)}</small></span></label></div><div class="button-row"><button class="btn subtle" type="button">Atras</button><button class="btn primary hotspot-ring" type="button">Continuar</button></div>`,
    password:`<div class="org-selected-chip"><strong>${esc(org)}</strong></div><div class="field hotspot-ring"><label for="password">Contrasena</label><input id="password" name="password" type="password" value="••••••••" readonly></div><label class="remember-line"><input type="checkbox" checked disabled> Recordarme en este dispositivo</label><div class="button-row"><button class="btn subtle" type="button">Cambiar organizacion</button><button class="btn primary hotspot-ring" type="button">Iniciar sesion</button></div>`
  };
  const sub={email:'Ingresa tu correo corporativo para identificar tus organizaciones.',organization:'Selecciona la organizacion para continuar.',password:'Ingresa tu contrasena para continuar.'};
  return `<main class="login-page"><section class="login-panel"><div class="brand-line"><div class="brand-mark">C360</div><span class="product-badge">Enterprise Login</span></div><h1>Compliance 360 Enterprise</h1><p>${sub[step]}</p><form class="form-stack">${blocks[step]||''}</form></section><section class="login-hero"><div class="hero-card"><span class="product-badge">Tenant transparente</span><h2>Login Enterprise sin GUIDs tecnicos</h2><p>Organizacion resuelta automaticamente.</p></div><div class="grid cards">${['EMAIL','ORG','PASS'].map(x=>`<div class="glass-card"><span class="metric-label">Modo</span><div class="metric-value">${step.toUpperCase()}</div></div>`).join('')}</div></section></main>`;
}

const NAV=__NAV__;

function sidebar(active,navGroups){
  const groups=(navGroups&&navGroups.length)?navGroups:NAV;
  return groups.map(gr=>`<nav class="nav-group"><div class="nav-label">${gr.g}</div>${gr.items.map(([k,l])=>`<button class="nav-button ${active===k?'active':''}" type="button"><span class="nav-icon">${l.split(' ').map(w=>w[0]).join('').slice(0,2)}</span><span>${l}</span><span>›</span></button>`).join('')}</nav>`).join('');
}

function routeLabel(active,navGroups){
  const groups=(navGroups&&navGroups.length)?navGroups:NAV;
  return groups.flatMap(g=>g.items).find(i=>i[0]===active)?.[1]||active;
}

function shellWrap(active,chip,body,navGroups){
  const label=routeLabel(active,navGroups);
  return `<div class="layout"><aside class="sidebar"><div class="brand"><div class="brand-mark">C360</div><div><div class="brand-title">Compliance 360</div><div class="brand-subtitle">Enterprise Edition</div></div></div><section class="sidebar-status"><span class="status-pill ok">Live</span><strong>Production Core 100%</strong></section>${sidebar(active,navGroups)}</aside><main class="main"><header class="topbar"><div><div class="breadcrumbs compact">Compliance 360</div><strong>${esc(label)}</strong></div><input class="search-box" placeholder="Buscar o escribir Enter para documentos..." readonly><div class="top-actions"><span class="tenant-chip">Tenant: ${chip}</span><button class="btn danger">Salir</button></div></header><section class="content">${body}</section></main></div>`;
}

function renderFormFields(fields){
  if(!fields||!fields.length) return '';
  return fields.map(f=>{
    const ring=f.hotspot?' hotspot-ring':'';
    if(f.type==='select'){
      const opts=(f.options||[]).map(o=>`<option value="${esc(o.v)}" ${String(o.v)===String(f.value)?'selected':''}>${esc(o.l||o.v)}</option>`).join('');
      return `<div class="field${ring}"><label for="${esc(f.id)}">${esc(f.label)}</label><select id="${esc(f.id)}" name="${esc(f.id)}" disabled>${opts}</select>${f.help?`<small>${esc(f.help)}</small>`:''}</div>`;
    }
    if(f.type==='checkbox'){
      return `<label class="toggle-row${ring}"><input type="checkbox" ${f.value?'checked':''} disabled> ${esc(f.label)}</label>`;
    }
    const t=f.type==='password'?'password':f.type==='color'?'color':f.type==='date'?'date':f.type==='email'?'email':f.type==='tel'?'tel':f.type==='url'?'url':'text';
    return `<div class="field${ring}"><label for="${esc(f.id)}">${esc(f.label)}</label><input id="${esc(f.id)}" name="${esc(f.id)}" type="${t}" value="${esc(String(f.value??''))}" readonly ${f.required?'required':''}>${f.help?`<small>${esc(f.help)}</small>`:''}</div>`;
  }).join('');
}

function formPanelHtml(formKey){
  const F=__FORMS__[formKey];
  if(!F) return '';
  return `<section class="tenant-panel active"><div class="section-heading"><div><h2 class="section-title">${esc(F.title)}</h2>${F.subtitle?`<p class="metric-label">${esc(F.subtitle)}</p>`:''}</div><span class="status-pill ok">Backend activo</span></div><form id="${esc(formKey)}-form" class="form-stack"><div class="grid two">${renderFormFields(F.fields)}</div><button class="btn primary hotspot-ring" type="button">${esc(F.submit)}</button></form></section>`;
}

function modulePage(mod,mode){
  const M=__MODULES__[mod]||{label:mod,desc:'',form_fields:[]};
  const ro=mode==='readonly'?`<div class="empty-state"><h3>Modo solo lectura</h3><p>Tu rol puede consultar ${esc(M.label)}, pero no crear, editar, aprobar ni eliminar registros en este modulo.</p></div>`:'';
  const form=mode==='create'?`<form id="module-action-form" class="form-stack"><div class="grid two">${renderFormFields(M.form_fields)}</div><div class="button-row"><button class="btn primary hotspot-ring" type="button">Crear registro real</button><span class="metric-label">Usa endpoints reales y refresca el listado del tenant.</span></div></form>`:'';
  return `<section class="module-page"><div class="module-hero"><div><p class="eyebrow">Operations</p><h1>${esc(M.label)}</h1><p>${esc(M.desc)}</p></div><div class="workflow-strip">${['Plan','Assign','Execute','Evidence','Close'].map(s=>`<span>${s}</span>`).join('')}</div></div>${ro}<section class="card"><h2 class="section-title">Action Center</h2>${form||'<p style="color:var(--muted);font-size:.9rem">Tabla de registros del tenant.</p>'}</section></section>`;
}

function superAdminMock(tab,filled,navGroups){
  const tabs=__SUPERADMIN_TABS__;
  const nav=tabs.map(([k,l],i)=>`<button class="tenant-tab ${k===tab?'active':''}" type="button"><span>${i+1}</span>${l}</button>`).join('');
  let panel='';
  if(tab==='executive') panel=`<div class="grid cards" style="grid-template-columns:repeat(4,1fr)">${[['Tenants','1'],['Usuarios','2'],['Alertas','0']].map(([a,b])=>`<article class="metric-card"><span>${a}</span><strong>${b}</strong></article>`).join('')}</div>`;
  else if(tab==='tenants'){
    const fields=(__FORMS__.create_tenant.fields||[]).map(f=>filled?f:(f.id==='newTenantName'?{...f,value:'Cliente Demo Compliance'}:f.id==='newTenantSlug'?{...f,value:'cliente-demo-compliance'}:f));
    panel=`<section id="create-tenant-panel" class="card"><div class="section-heading"><div><h2 class="section-title">Crear Tenant</h2><p class="metric-label">Alta enterprise de tenant. Al guardar se abrira automaticamente el Tenant Administration Center del nuevo tenant.</p></div><span class="status-pill ok">PLATFORM.TENANT.CREATE</span></div><form id="create-tenant-form" class="form-stack"><div class="grid two">${renderFormFields(fields)}</div><div class="grid two"><button class="btn primary hotspot-ring" type="button">Crear tenant</button><button class="btn" type="button">Cancelar</button></div></form></section>`;
  }
  else panel=`<section class="card"><h2 class="section-title">${esc(tab)}</h2><p style="color:var(--muted)">Panel SuperAdmin — datos del backend.</p></section>`;
  return shellWrap('superadmin-platform','dc7c46ee',`<div class="page-header"><div class="page-title"><h1>SuperAdmin Platform Center</h1><p>Consola global Compliance 360.</p></div></div><section class="tenant-admin-shell"><aside class="tenant-tabs">${nav}</aside><div>${panel}</div></section>`,navGroups);
}

function tacMock(tab,navGroups){
  const tabs=__TAC_TABS__;
  const nav=tabs.map(([k,l],i)=>`<button class="tenant-tab ${k===tab?'active':''}" type="button"><span>${i+1}</span>${l}</button>`).join('');
  const formMap={general:'tac_general',branding:'tac_branding',security:'tac_security',users:'tac_users',rbac:'tac_rbac',domains:'tac_domains'};
  let body='';
  if(tab==='state'){
    body=`<section class="tenant-panel active" data-panel="state"><div class="section-heading"><div><h2 class="section-title">Estado</h2><p class="metric-label">Lifecycle Draft, Trial, Active, Suspended, Archived y Restore con permisos especiales.</p></div><span class="status-pill warn">TENANT.STATUS</span></div><div class="tenant-state-flow"><span>Draft</span><span>Trial</span><span>Active</span><span>Suspended</span><span>Archived</span><span>Restore</span></div><div class="button-row"><button class="btn" type="button">Mover a Trial</button><button class="btn primary hotspot-ring" type="button">Activar</button><button class="btn" type="button">Suspender</button><button class="btn danger" type="button">Archivar</button><button class="btn" type="button">Restaurar</button></div></section>`;
  } else if(formMap[tab]) body=formPanelHtml(formMap[tab]);
  else body=`<section class="tenant-panel active"><h2 class="section-title">${esc(tabs.find(t=>t[0]===tab)?.[1]||tab)}</h2><p class="metric-label">Pestaña disponible en la app — abra TAC en ${esc(DEMO.appUrl)} para el detalle completo.</p></section>`;
  return shellWrap('tenant-administration','ddcaf211',`<div class="page-header"><h1>Tenant Administration Center</h1></div><section class="tenant-admin-shell"><aside class="tenant-tabs">${nav}</aside>${body}</section>`,navGroups);
}

function configMock(panel,navGroups){
  const storage=panel==='storage'||panel==='both'||panel==='storage_test';
  const email=panel==='email'||panel==='both';
  const testBtn=panel==='storage_test'?'<button class="btn hotspot-ring" id="test-first-storage-provider" type="button" style="margin-top:8px">Probar primer Storage Provider</button>':'';
  return shellWrap('configuration','ddcaf211',`<section class="module-page"><div class="module-hero"><div><p class="eyebrow">Configuracion / Integraciones</p><h1>Provider Administration</h1><p>Email Providers, Storage Providers, Health Status, Connection Test, Usage Statistics, Failover Configuration y Provider Priority por tenant.</p></div><div class="hero-actions">${storage?'<button class="btn primary hotspot-ring" type="button">Crear Storage Local</button>':''}${email?'<button class="btn hotspot-ring" type="button">Crear Email SMTP</button>':''}</div></div><div class="metric-grid">${storage?'<article class="card hotspot-ring"><h3>Storage Providers</h3><p class="metric-label">Local · priority 1 · default</p>'+testBtn+'</article>':''}${email?'<article class="card hotspot-ring"><h3>Email Providers</h3><p class="metric-label">SMTP · Mailhog en dev</p></article>':''}</div></section>`,navGroups);
}

function enterpriseMock(navGroups){
  const F=__FORMS__.enterprise_security;
  return shellWrap('security','ddcaf211',`<section class="hero-card compact module-hero"><div><span class="product-badge">Persistent workspace</span><h2>${esc(F.title)}</h2><p>Controles de seguridad, revisiones de acceso, hardening y evidencias.</p></div></section><section class="card"><h2 class="section-title">Action Center</h2><form id="enterprise-action-form" class="form-stack"><div class="grid two">${renderFormFields(F.fields)}</div><button class="btn primary hotspot-ring" type="button">${esc(F.submit)}</button></form></section>`,navGroups);
}

function reportsMock(action,navGroups){
  const labels={seed:'Seed standard reports',execute:'Ejecutar reporte',export:'Exportar via reportes'};
  return shellWrap('reports','ddcaf211',`<section class="card"><h1>Report Center</h1><p style="color:var(--muted)">Catálogo, ejecución y exportación.</p><button class="btn primary hotspot-ring">${labels[action]||'Ejecutar'}</button></section>`,navGroups);
}

function renderMock(mock,roleMeta){
  const nav=roleMeta.allowedNavigation;
  const ctx={email:roleMeta.email||'admin@compliance360.local',org:roleMeta.scope==='platform'? 'Compliance 360 Platform':DEMO.company.commercial_name,orgDesc:roleMeta.scope==='platform'?'Tenant plataforma':DEMO.company.slug};
  switch(mock.type){
    case 'intro': return `<div class="intro-full"><article class="intro-card"><span class="product-badge">${esc(mock.roleName)}</span><h1>Guía visual paso a paso</h1><p style="color:var(--muted)">Pulse Siguiente para comenzar el recorrido.</p></article></div>`;
    case 'login': Object.assign(ctx,mock); return loginMock(mock.step,ctx);
    case 'shell': return shellWrap(mock.activeRoute,mock.tenantChip,'<section class="card"><h2>Bienvenido</h2><p>Navegue al módulo indicado en el panel coach.</p></section>',nav);
    case 'module': return shellWrap(mock.module,'ddcaf211',modulePage(mock.module,mock.mode),nav);
    case 'superadmin': return superAdminMock(mock.tab,mock.filled,nav);
    case 'tac': return tacMock(mock.tab,nav);
    case 'configuration': return configMock(mock.panel,nav);
    case 'reports': return reportsMock(mock.action,nav);
    case 'dashboard': return shellWrap(mock.variant==='compliance'?'compliance':'dashboard','ddcaf211',`<section class="card"><h1>${mock.variant==='compliance'?'Compliance Dashboard':'Executive Dashboard'}</h1><p>KPIs y métricas agregadas.</p></section>`,nav);
    case 'enterprise': return enterpriseMock(nav);
    case 'audit_trail': return shellWrap('audit-trail','ddcaf211','<section class="card"><h1>Audit Trail</h1><table class="data-table"><tr><th>Accion</th><th>Usuario</th></tr><tr><td>DocumentCreated</td><td>doccontrol@</td></tr></table></section>',nav);
    case 'api_note': return `<div class="intro-full"><article class="intro-card"><span class="product-badge warn">API / Swagger</span><h1>${esc(mock.topic)}</h1><p>Esta acción se valida en Swagger o DevTools — no hay pantalla UI dedicada en la versión actual.</p><p style="margin-top:1rem;font-size:.85rem;color:var(--muted)">Use token Bearer del rol y endpoint documentado en manuales MFT.</p></article></div>`;
    case 'finish': return `<div class="intro-full"><article class="intro-card"><h1>Flujo completado</h1><p><strong>${esc(mock.roleName)}</strong></p><ul style="margin-top:1rem;line-height:1.8">${(mock.items||[]).map(i=>`<li>${esc(i)}</li>`).join('')}</ul></article></div>`;
    default: return '<p>Mock no definido</p>';
  }
}
""".replace("__COMPANY__", json.dumps(COMPANY, ensure_ascii=False)).replace(
    "__TENANTS__", json.dumps(TENANTS, ensure_ascii=False)
).replace("__APP_URL__", json.dumps(APP_URL)).replace(
    "__MODULES__", json.dumps(MODULES, ensure_ascii=False)
).replace("__FORMS_JSON__", json.dumps(FORMS, ensure_ascii=False)
).replace("__SUPERADMIN_TABS_JSON__", json.dumps(SUPERADMIN_TABS, ensure_ascii=False)
).replace("__TAC_TABS_JSON__", json.dumps(TAC_TABS_LIST, ensure_ascii=False)
).replace("__NAV__", json.dumps(
    [{"g": g["group"], "items": g["items"]} for g in NAVIGATION],
    ensure_ascii=False,
))

CSS = open(ROOT / "docs" / "manual-interactivo" / "COMPLIANCE360_GUIA_ADMIN_PASO_A_PASO.html", encoding="utf-8").read()
CSS = CSS.split("<style>")[1].split("</style>")[0]
# extend with role picker
CSS += """
.role-picker-screen{min-height:calc(100vh - 120px);padding:1.25rem 1.5rem;overflow:auto}
.role-picker-screen h1{font-size:1.45rem;color:var(--accent-strong);margin-bottom:.35rem}
.role-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(220px,1fr));gap:.75rem;margin-top:1rem}
.role-card-pick{border:1px solid var(--border);border-radius:var(--radius);padding:1rem;background:var(--surface);cursor:pointer;text-align:left;transition:transform .15s,box-shadow .15s,border-color .15s}
.role-card-pick:hover{transform:translateY(-2px);box-shadow:var(--shadow);border-color:var(--accent)}
.role-card-pick.platform{border-left:4px solid var(--accent)}
.role-card-pick.tenant{border-left:4px solid var(--success)}
.role-card-pick h3{font-size:.92rem;margin:0 0 .35rem;color:var(--accent-strong)}
.role-card-pick p{font-size:.78rem;color:var(--muted);margin:0;line-height:1.45}
.role-card-pick code{font-size:.7rem}
.back-role{margin-right:auto}
.coach-guide{display:grid;gap:.65rem;margin-bottom:.5rem}
.coach-block{border-radius:12px;padding:.85rem;font-size:.84rem;line-height:1.55;border:1px solid var(--border)}
.coach-block h5{font-size:.68rem;text-transform:uppercase;letter-spacing:.06em;margin:0 0 .35rem;color:var(--muted);font-weight:800}
.coach-block p{margin:0}
.coach-block.what{background:#eff6ff;border-color:#93c5fd;border-left:4px solid var(--accent)}
.coach-block.why{background:#f0fdf4;border-color:#86efac;border-left:4px solid var(--success)}
.coach-block.next{background:#fffbeb;border-color:#fde68a;border-left:4px solid var(--warning)}
.coach-block.where{background:var(--surface-muted);border-left:4px solid var(--muted)}
.btn-ref{display:inline-block;background:var(--accent);color:#fff;padding:.12rem .5rem;border-radius:6px;font-size:.78rem;font-weight:700;white-space:nowrap}
.product-badge.warn{background:#fff7ed;border-color:#fdba74;color:#9a3412}
.fields-table{width:100%;border-collapse:collapse;font-size:.78rem;margin-top:.5rem}
.fields-table th{background:var(--surface-muted);text-align:left;padding:6px 8px;border:1px solid var(--border);font-size:.65rem;text-transform:uppercase;color:var(--muted)}
.fields-table td{padding:6px 8px;border:1px solid var(--border);vertical-align:top}
.fields-table code{font-size:.72rem}
.empty-state{background:var(--surface-muted);border:1px dashed var(--border);border-radius:12px;padding:1rem;margin-bottom:1rem}
.empty-state h3{margin:0 0 .35rem;font-size:.95rem;color:var(--accent-strong)}
.empty-state p{margin:0;font-size:.85rem;color:var(--muted);line-height:1.5}
.toggle-row{display:flex;align-items:center;gap:8px;font-size:.85rem;padding:.35rem 0}
.metric-grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(240px,1fr));gap:12px}
.eyebrow{font-size:11px;text-transform:uppercase;letter-spacing:.08em;color:var(--muted);margin:0 0 4px}
.module-hero{display:flex;justify-content:space-between;gap:16px;padding:18px;border-radius:var(--radius);background:linear-gradient(135deg,rgba(23,105,170,.08),rgba(15,118,110,.06));border:1px solid var(--border);margin-bottom:18px;flex-wrap:wrap}
.workflow-strip{display:flex;gap:6px;flex-wrap:wrap;font-size:11px}
.workflow-strip span{padding:4px 8px;border-radius:999px;border:1px solid var(--border);background:var(--surface)}
"""

APP_JS = r"""
(function(){
  const FLOWS=__FLOWS__;
  const KEY='c360.guia.visual.v1';
  let roleKey=null, idx=0, autoTimer=null;

  function loadState(){
    try{return JSON.parse(localStorage.getItem(KEY)||'{}');}catch(e){return {};}
  }
  function saveState(){
    localStorage.setItem(KEY,JSON.stringify({roleKey,idx}));
  }

  function roleMeta(){return FLOWS[roleKey]?.meta||{};}
  function steps(){return FLOWS[roleKey]?.steps||[];}

  function renderPicker(){
    document.getElementById('routeChip').textContent='#/selector';
    document.getElementById('routeBadge').textContent='Elegir rol';
    document.getElementById('coachPhase').textContent='Selector';
    document.getElementById('coachTitle').textContent='Elija un rol para comenzar';
    document.getElementById('coachBody').innerHTML=`<div class="coach-guide"><div class="coach-block what"><h5>Qué hacer</h5><p>Haga clic en la tarjeta de <strong>su rol</strong> (Plataforma o Tenant).</p></div><div class="coach-block why"><h5>Por qué</h5><p>Cada rol tiene un recorrido distinto: login, menú permitido y tareas de su puesto.</p></div><div class="coach-block next"><h5>Después</h5><p>En cada paso verá: <strong>1 Qué presionar</strong> · <strong>2 Por qué</strong> · <strong>3 Qué pasa después</strong>. Los anillos dorados marcan el botón en la pantalla izquierda.</p></div></div><div class="warn-box">Contraseñas demo: <code>e2e/testdata.json</code>. App real: ejecute <code>e2e_provision.ps1</code> antes.</div>`;
    document.getElementById('progressText').textContent='Seleccione rol';
    document.getElementById('stepRail').innerHTML='';
    document.getElementById('btnPrev').disabled=true;
    document.getElementById('btnNext').textContent='Seleccione un rol ↑';
    const groups={platform:[],tenant:[]};
    Object.values(FLOWS).forEach(f=>groups[f.meta.scope].push(f.meta));
    const mkCard=m=>`<button type="button" class="role-card-pick ${m.scope}" data-role="${m.key}"><h3>${m.name}</h3><p>${m.email?`<code>${m.email}</code><br>`:''}${m.landing}</p></button>`;
    document.getElementById('mockApp').innerHTML=`<div class="role-picker-screen"><h1>Compliance 360 — Guía visual por rol</h1><p style="color:var(--muted)">HTML autocontenido · doble clic · sin servidor</p><h2 style="font-size:.85rem;margin:1.25rem 0 .35rem;color:var(--accent-strong)">Plataforma (${groups.platform.length})</h2><div class="role-grid">${groups.platform.map(mkCard).join('')}</div><h2 style="font-size:.85rem;margin:1.25rem 0 .35rem;color:var(--success)">Tenant operaciones (${groups.tenant.length})</h2><div class="role-grid">${groups.tenant.map(mkCard).join('')}</div></div>`;
    document.querySelectorAll('.role-card-pick').forEach(b=>b.onclick=()=>selectRole(b.dataset.role));
  }

  function selectRole(key){
    roleKey=key; idx=0; saveState(); renderStep(0);
  }

  function renderStep(i){
    if(!roleKey){renderPicker();return;}
    const sts=steps();
    idx=Math.max(0,Math.min(sts.length-1,i));
    const s=sts[idx];
    document.getElementById('mockApp').innerHTML=renderMock(s.mock,roleMeta());
    document.getElementById('coachPhase').textContent=s.phase;
    document.getElementById('coachTitle').textContent=s.title;
    document.getElementById('coachBody').innerHTML=s.coach;
    document.getElementById('routeChip').textContent=s.route;
    document.getElementById('routeBadge').textContent=s.route;
    document.getElementById('progressText').textContent=`${roleMeta().name} · Paso ${idx+1} de ${sts.length}`;
    document.getElementById('btnPrev').disabled=false;
    document.getElementById('btnNext').textContent=idx>=sts.length-1?'Fin ✓':'Siguiente →';
    document.getElementById('stepRail').innerHTML=sts.map((st,n)=>`<button type="button" class="step-dot ${n===idx?'active':''} ${n<idx?'done':''}" data-i="${n}" title="${st.title.replace(/"/g,'')}">${n+1}</button>`).join('');
    document.querySelectorAll('.step-dot').forEach(d=>d.onclick=()=>{stopAuto();renderStep(+d.dataset.i);});
    document.getElementById('mockFrame').scrollTop=0;
    saveState();
  }

  function stopAuto(){if(autoTimer){clearInterval(autoTimer);autoTimer=null;document.getElementById('btnAuto').textContent='▶ Auto';}}
  function toggleAuto(){
    if(!roleKey)return;
    if(autoTimer){stopAuto();return;}
    document.getElementById('btnAuto').textContent='⏸ Pausar';
    autoTimer=setInterval(()=>{if(idx>=steps().length-1){stopAuto();return;}renderStep(idx+1);},4000);
  }

  document.getElementById('btnPrev').onclick=()=>{
    stopAuto();
    if(!roleKey)return;
    if(idx===0){roleKey=null;renderPicker();saveState();return;}
    renderStep(idx-1);
  };
  document.getElementById('btnNext').onclick=()=>{
    stopAuto();
    if(!roleKey){return;}
    if(idx<steps().length-1)renderStep(idx+1);
  };
  document.getElementById('btnAuto').onclick=toggleAuto;
  document.addEventListener('keydown',e=>{
    if(e.key==='ArrowLeft'){stopAuto();document.getElementById('btnPrev').click();}
    if(e.key==='ArrowRight'){stopAuto();if(roleKey&&idx<steps().length-1)renderStep(idx+1);}
  });

  const st=loadState();
  if(st.roleKey&&FLOWS[st.roleKey]){roleKey=st.roleKey;renderStep(st.idx||0);}else renderPicker();
})();
""".replace("__FLOWS__", json.dumps(ROLE_FLOWS, ensure_ascii=False))

HTML = f"""<!DOCTYPE html>
<html lang="es">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Compliance 360 — Guía Visual Todos los Roles</title>
<style>
{CSS}
</style>
</head>
<body>
<div class="guide-shell">
  <header class="guide-header">
    <div class="guide-brand"><div class="guide-brand-mark">C360</div>Guía visual por rol</div>
    <span class="chip gold">17 roles · HTML autocontenido</span>
    <span class="chip route" id="routeChip">#/selector</span>
    <nav class="guide-nav">
      <button type="button" class="gbtn back-role" id="btnPrev">← Anterior</button>
      <button type="button" class="gbtn primary" id="btnNext">Siguiente →</button>
    </nav>
  </header>
  <div class="step-rail" id="stepRail"></div>
  <div class="guide-body">
    <div class="mock-frame" id="mockFrame">
      <span class="route-badge" id="routeBadge">Selector</span>
      <span class="demo-tag">Réplica visual · app.js</span>
      <div class="mock-inner" id="mockApp"></div>
    </div>
    <aside class="coach">
      <div class="coach-head">
        <div class="coach-phase" id="coachPhase">Selector</div>
        <h2 id="coachTitle">Elija un rol</h2>
      </div>
      <div class="coach-scroll" id="coachBody"></div>
      <footer class="coach-foot">
        <span class="progress-text" id="progressText">—</span>
        <button type="button" class="gbtn" id="btnAuto">▶ Auto</button>
      </footer>
    </aside>
  </div>
</div>
<script>
{JS_RENDER}
</script>
<script>
{APP_JS}
</script>
</body>
</html>
"""


def main():
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(HTML, encoding="utf-8")
    total_steps = sum(len(v["steps"]) for v in ROLE_FLOWS.values())
    print(f"Wrote {OUT.relative_to(ROOT)}")
    print(f"  Roles: {len(ROLE_FLOWS)}, Steps total: {total_steps}")


if __name__ == "__main__":
    main()
