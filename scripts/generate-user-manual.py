#!/usr/bin/env python3
"""Generate Compliance 360 interactive role-based user manual (offline-capable).

Sources (facts only):
- RoleCatalog.cs / RbacCatalog.cs
- regulatory-affairs.js (views, STATUS_LABELS, buttons)
- docs/regulatory-affairs/security/04, 17, 21, 24
"""
from __future__ import annotations

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "docs" / "user-manual"
ROOT.mkdir(parents=True, exist_ok=True)

# ---------------------------------------------------------------------------
# Canonical data from certified system
# ---------------------------------------------------------------------------

ROLES = [
    {
        "id": "tenant-administrator",
        "name": "Tenant Administrator",
        "short": "TAC",
        "color": "#6366f1",
        "icon": "shield",
        "purpose": "Administra el tenant: usuarios, roles y configuración general. No opera expedientes regulatorios por defecto (SoD).",
        "can": [
            "Crear y desactivar usuarios (TENANT.USERS)",
            "Asignar roles (TENANT.ROLES / RBAC.MANAGE)",
            "Configurar branding y perfil del tenant",
            "Ejecutar Bootstrap regulatorio / CONFIGURE (REGULATORY.CONFIGURE)",
            "Ver SoD Settings (REGULATORY.SOD.MANAGE)",
            "Consultar lecturas RA (productos, expedientes, CT/RS, licencias, dashboard)",
            "Ver Audit Trail (AUDIT.READ / TENANT.AUDIT)",
        ],
        "cannot": [
            "Crear expedientes (sin REGULATORY.DOSSIER.CREATE)",
            "Aprobar internamente para sometimiento",
            "Registrar sometimiento",
            "Registrar aprobación externa MINSA/CSS + CT/RS",
            "Sustituir al Specialist, Reviewer, Approver o Submitter en el flujo operativo",
        ],
        "flowStage": "IAM · configuración · sin operación de expediente",
        "receives": "Solicitudes de acceso y asignación de roles del equipo regulatorio.",
        "delivers": "Usuarios con el rol correcto y SoD habilitado para que el flujo Prep→Review→Approve→Submit funcione.",
        "screens": ["login", "tenant-administration", "security", "regulatory-config", "regulatory-sod", "regulatory-dashboard", "audit-trail"],
        "tutorials": ["tac-login", "tac-users", "tac-roles", "tac-configure-ra", "tac-sod", "tac-no-operate", "tac-logout"],
        "file": "tenant-administrator.html",
    },
    {
        "id": "regulatory-administrator",
        "name": "Regulatory Administrator",
        "short": "RA-ADM",
        "color": "#0ea5e9",
        "icon": "sliders",
        "purpose": "Configura el módulo Regulatory Affairs: autoridades, packs, alertas e importación REGUTRACK.",
        "can": [
            "Bootstrap regulatorio (autoridades MINSA/CSS + pack de requisitos)",
            "Importación Stage XLSX REGUTRACK (REGULATORY.CONFIGURE)",
            "Gestionar productos y fabricantes (MANAGE)",
            "Gestionar licencias operativas",
            "Ver SoD Settings y lecturas del módulo",
        ],
        "cannot": [
            "Aprobar internamente (APPROVE_FOR_SUBMISSION)",
            "Registrar sometimiento (SUBMIT)",
            "Registrar decisión externa / CT/RS (APPROVE)",
            "Operar el expediente como Specialist por defecto",
        ],
        "flowStage": "Configuración previa al flujo del expediente",
        "receives": "Archivo REGUTRACK y reglas SoD del Tenant Administrator.",
        "delivers": "Catálogos, autoridades, packs publicados e importaciones listas para el equipo operativo.",
        "screens": ["login", "regulatory-config", "regulatory-import", "regulatory-sod", "regulatory-manufacturers", "regulatory-licenses", "regulatory-alerts", "regulatory-dashboard"],
        "tutorials": ["adm-bootstrap", "adm-authority", "adm-import", "adm-alerts", "adm-sod-view"],
        "file": "regulatory-administrator.html",
    },
    {
        "id": "regulatory-manager",
        "name": "Regulatory Manager",
        "short": "RA-MGR",
        "color": "#f59e0b",
        "icon": "briefcase",
        "purpose": "Supervisa el pipeline, registra observaciones de autoridad y la decisión externa que crea el CT/RS.",
        "can": [
            "Ver dashboard, pipeline, portafolio y aging",
            "Actualizar expedientes (DOSSIER.UPDATE)",
            "Registrar observación de autoridad (OBSERVATION.MANAGE)",
            "Registrar aprobación externa MINSA/CSS + CT/RS (DOSSIER.APPROVE)",
            "Gestionar registros y renovaciones (REGISTRATION.MANAGE)",
            "SoD manage y emergency override auditado",
        ],
        "cannot": [
            "Sustituir Specialist + Approver + Submitter a la vez sin override",
            "Representar que la aprobación interna es la de MINSA/CSS",
        ],
        "flowStage": "Observación → Respuesta → Decisión externa → CT/RS → Renovación",
        "receives": "Expedientes sometidos por el Submitter; respuestas del Specialist a observaciones.",
        "delivers": "CT/RS activo y renovaciones iniciadas; excepciones SoD auditadas.",
        "screens": ["login", "regulatory-dashboard", "regulatory-pipeline", "regulatory-dossiers", "regulatory-registrations", "regulatory-alerts", "regulatory-sod"],
        "tutorials": ["mgr-dashboard", "mgr-observe", "mgr-external-approve", "mgr-renewal", "mgr-breakglass"],
        "file": "regulatory-manager.html",
    },
    {
        "id": "regulatory-specialist",
        "name": "Regulatory Specialist",
        "short": "RA-SPEC",
        "color": "#22c55e",
        "icon": "file-plus",
        "purpose": "Prepara productos y expedientes, solicita documentos, completa requisitos y responde observaciones.",
        "can": [
            "Crear producto + expediente (PRODUCT.MANAGE + DOSSIER.CREATE)",
            "Transiciones de preparación: Pedir docs, Docs recibidos, Armar, Declarar técnicamente completo",
            "Marcar requisito recibido",
            "Alta fabricante y certificados",
            "Responder observación de autoridad",
            "Ver CT/RS y dashboard en lectura",
        ],
        "cannot": [
            "Revisar su propio expediente (PreventSelfReview / SoD)",
            "Aprobar internamente para sometimiento",
            "Registrar sometimiento",
            "Registrar aprobación externa / crear CT/RS",
        ],
        "flowStage": "Preparación (Planning → ReadyForSubmission) y respuesta a observaciones",
        "receives": "Packs/autoridades del Administrator; devoluciones del Reviewer; observaciones del Manager.",
        "delivers": "Expediente técnicamente completo listo para el Reviewer.",
        "screens": ["login", "regulatory-portfolio", "regulatory-dossiers", "regulatory-manufacturers", "regulatory-pipeline", "regulatory-dashboard"],
        "tutorials": ["spec-product", "spec-mfr", "spec-dossier", "spec-checklist", "spec-ready", "spec-respond-obs", "spec-fix-return"],
        "file": "regulatory-specialist.html",
    },
    {
        "id": "regulatory-reviewer",
        "name": "Regulatory Reviewer",
        "short": "RA-REV",
        "color": "#a855f7",
        "icon": "search-check",
        "purpose": "Revisa técnicamente cada requisito: Aceptar o Rechazar. No prepara ni aprueba internamente.",
        "can": [
            "Abrir expedientes en revisión (DOSSIER.READ + REVIEW)",
            "Aceptar requisito",
            "Rechazar requisito (con comentario)",
            "Consultar dashboard y CT/RS en lectura",
        ],
        "cannot": [
            "Crear productos/expedientes",
            "Aprobar internamente para sometimiento",
            "Registrar sometimiento",
            "Preparar el mismo caso si PreventSelfReview está activo",
        ],
        "flowStage": "Revisión técnica (antes de ReadyForSubmission / hacia Approver)",
        "receives": "Expedientes armados por el Specialist.",
        "delivers": "Requisitos aceptados; expediente listo para Declarar técnicamente completo / Approver.",
        "screens": ["login", "regulatory-dossiers", "regulatory-pipeline", "regulatory-dashboard"],
        "tutorials": ["rev-queue", "rev-accept", "rev-reject", "rev-return"],
        "file": "regulatory-reviewer.html",
    },
    {
        "id": "regulatory-approver",
        "name": "Regulatory Approver",
        "short": "RA-APPR",
        "color": "#eab308",
        "icon": "badge-check",
        "purpose": "Autoriza internamente que el expediente pueda ser sometido. No representa a MINSA/CSS.",
        "can": [
            "Ver expedientes en estado Técnicamente completo",
            "Presionar «Aprobar internamente para sometimiento» (APPROVE_FOR_SUBMISSION)",
            "Consultar lecturas RA",
        ],
        "cannot": [
            "Preparar el expediente",
            "Registrar sometimiento (SeparateApproverAndSubmitter)",
            "Registrar aprobación externa / CT/RS",
        ],
        "flowStage": "Aprobación interna (ReadyForSubmission → ApprovedForSubmission)",
        "receives": "Expedientes declarados técnicamente completos.",
        "delivers": "Expediente con estado «Aprobado internamente para sometimiento» para el Submitter.",
        "screens": ["login", "regulatory-dossiers", "regulatory-pipeline", "regulatory-dashboard"],
        "tutorials": ["appr-queue", "appr-approve", "appr-why-no-submit"],
        "file": "regulatory-approver.html",
    },
    {
        "id": "regulatory-submitter",
        "name": "Regulatory Submitter",
        "short": "RA-SUB",
        "color": "#ef4444",
        "icon": "send",
        "purpose": "Registra el sometimiento real ante la autoridad. No otorga la aprobación interna.",
        "can": [
            "Ver expedientes «Aprobado internamente para sometimiento»",
            "Presionar «Registrar sometimiento» (DOSSIER.SUBMIT)",
            "Consultar lecturas RA",
        ],
        "cannot": [
            "Aprobar internamente",
            "Registrar decisión externa / CT/RS",
            "Preparar o revisar requisitos",
        ],
        "flowStage": "Sometimiento (ApprovedForSubmission → Submitted)",
        "receives": "Expedientes con aprobación interna del Approver.",
        "delivers": "Expediente «Sometido ante autoridad» para seguimiento del Manager.",
        "screens": ["login", "regulatory-dossiers", "regulatory-pipeline", "regulatory-dashboard"],
        "tutorials": ["sub-queue", "sub-submit", "sub-why-no-approve"],
        "file": "regulatory-submitter.html",
    },
    {
        "id": "regulatory-viewer",
        "name": "Regulatory Viewer",
        "short": "RA-VIEW",
        "color": "#64748b",
        "icon": "eye",
        "purpose": "Solo lectura del portafolio regulatorio, pipeline y CT/RS.",
        "can": [
            "Ver Dashboard, Portafolio, Pipeline, Expedientes, CT/RS, Alertas",
            "Consultar sin modificar",
        ],
        "cannot": [
            "Cualquier botón de mutación (Nuevo, Aprobar, Someter, Stage, Bootstrap, etc.)",
        ],
        "flowStage": "Consulta transversal (sin etapa de escritura)",
        "receives": "Visibilidad del estado del flujo después de cada rol operativo.",
        "delivers": "Información; no entrega cambios al siguiente rol.",
        "screens": ["login", "regulatory-dashboard", "regulatory-portfolio", "regulatory-pipeline", "regulatory-dossiers", "regulatory-registrations", "regulatory-alerts"],
        "tutorials": ["view-read", "view-no-edit"],
        "file": "regulatory-viewer.html",
    },
    {
        "id": "quality-manager",
        "name": "Quality Manager",
        "short": "QM",
        "color": "#14b8a6",
        "icon": "award",
        "purpose": "Coordinación QMS transversal. Puede registrar la decisión externa CT/RS; no prepara expedientes.",
        "can": [
            "Registrar aprobación externa / CT/RS (DOSSIER.APPROVE + REGISTRATION.MANAGE)",
            "Leer expedientes y reportes RA",
            "Aprobar documentos, CAPA, riesgos y fichas técnicas (módulos QMS)",
            "Ver Audit Trail",
        ],
        "cannot": [
            "Preparar expedientes (sin CREATE/UPDATE de preparación por defecto)",
            "Sustituir al Regulatory Manager en supervisión diaria salvo su permiso de APPROVE",
        ],
        "flowStage": "Decisión externa / CT/RS (junto o en apoyo al Manager)",
        "receives": "Expedientes sometidos listos para decisión de autoridad.",
        "delivers": "Registro de aprobación externa y CT/RS; trazabilidad QMS.",
        "screens": ["login", "regulatory-dossiers", "regulatory-registrations", "regulatory-dashboard", "audit-trail"],
        "tutorials": ["qm-external", "qm-limits", "qm-audit"],
        "file": "quality-manager.html",
    },
]

SCREENS = [
    {"id": "login", "name": "Inicio de sesión", "route": "#/login", "module": "Auth", "objective": "Identificar usuario y obtener token JWT para el tenant."},
    {"id": "tenant-administration", "name": "Tenant Administration", "route": "#/tenant-administration", "module": "Enterprise", "objective": "Gestionar usuarios, roles y perfil del tenant."},
    {"id": "security", "name": "Security", "route": "#/security", "module": "Enterprise", "objective": "Configuración de seguridad del tenant (MFA/SSO según permisos)."},
    {"id": "audit-trail", "name": "Audit Trail", "route": "#/audit-trail", "module": "Command Center", "objective": "Consultar eventos de auditoría."},
    {"id": "regulatory-shell", "name": "Consola de Asuntos Regulatorios", "route": "#/regulatory", "module": "Regulatory Affairs", "objective": "Contenedor de todas las vistas RA."},
    {"id": "regulatory-dashboard", "name": "Dashboard RA", "route": "#/regulatory → Dashboard", "module": "Regulatory Affairs", "objective": "Métricas de productos, CT activos, en trámite, requisitos críticos, detenidos >14d, por vencer."},
    {"id": "regulatory-portfolio", "name": "Portafolio", "route": "#/regulatory → Portafolio", "module": "Regulatory Affairs", "objective": "Listar productos y crear producto + expediente."},
    {"id": "regulatory-pipeline", "name": "Pipeline", "route": "#/regulatory → Pipeline", "module": "Regulatory Affairs", "objective": "Kanban por estado del expediente + Vencido + Renovación."},
    {"id": "regulatory-dossiers", "name": "Expedientes", "route": "#/regulatory → Expedientes", "module": "Regulatory Affairs", "objective": "Listar y abrir el workspace del expediente."},
    {"id": "regulatory-dossier-detail", "name": "Workspace del expediente", "route": "#/regulatory → Expedientes → detalle", "module": "Regulatory Affairs", "objective": "Ejecutar prep, revisión, aprobación interna, sometimiento, observación y aprobación externa."},
    {"id": "regulatory-registrations", "name": "Registros CT/RS", "route": "#/regulatory → Registros CT/RS", "module": "Regulatory Affairs", "objective": "Consultar números CT/RS, emisión y vencimiento."},
    {"id": "regulatory-manufacturers", "name": "Fabricantes", "route": "#/regulatory → Fabricantes", "module": "Regulatory Affairs", "objective": "Alta de fabricante y certificados."},
    {"id": "regulatory-licenses", "name": "Licencias", "route": "#/regulatory → Licencias", "module": "Regulatory Affairs", "objective": "Registrar licencias operativas."},
    {"id": "regulatory-alerts", "name": "Alertas", "route": "#/regulatory → Alertas", "module": "Regulatory Affairs", "objective": "Evaluar alertas de vencimiento / riesgo operativo."},
    {"id": "regulatory-import", "name": "Importación", "route": "#/regulatory → Importación", "module": "Regulatory Affairs", "objective": "Stage XLSX del libro REGUTRACK."},
    {"id": "regulatory-config", "name": "Configuración", "route": "#/regulatory → Configuración", "module": "Regulatory Affairs", "objective": "Bootstrap regulatorio (autoridades + pack)."},
    {"id": "regulatory-sod", "name": "SoD Settings", "route": "#/regulatory → SoD Settings", "module": "Regulatory Affairs", "objective": "Consultar política de segregación de funciones."},
]

FIELDS = [
    {"id": "email", "screen": "login", "label": "Correo", "tech": "email", "type": "email", "required": True, "purpose": "Identifica al usuario en el tenant.", "example": "ra.spec@cert.local", "invalid": "texto sin @", "format": "email", "max": 256, "roles": ["*"]},
    {"id": "password", "screen": "login", "label": "Contraseña", "tech": "password", "type": "password", "required": True, "purpose": "Autenticación.", "example": "(la asignada por TAC)", "invalid": "vacía", "format": "secreto", "roles": ["*"]},
    {"id": "brand", "screen": "regulatory-portfolio", "label": "Marca", "tech": "brand", "type": "text", "required": True, "purpose": "Marca comercial del dispositivo.", "example": "DEMO", "invalid": "(vacío)", "max": 120, "roles": ["regulatory-specialist", "regulatory-administrator"]},
    {"id": "regulatoryName", "screen": "regulatory-portfolio", "label": "Nombre regulatorio", "tech": "regulatoryName", "type": "text", "required": True, "purpose": "Nombre oficial del producto para expediente y CT/RS. Puede diferir del comercial.", "example": "PRODUCTO DEMO", "invalid": "(vacío)", "max": 320, "roles": ["regulatory-specialist", "regulatory-administrator"]},
    {"id": "catalogCode", "screen": "regulatory-portfolio", "label": "Código catálogo", "tech": "catalogCode", "type": "code", "required": True, "purpose": "Código único por tenant. Duplicado genera error.", "example": "CAT-123456", "invalid": "código ya existente", "max": 120, "roles": ["regulatory-specialist", "regulatory-administrator"]},
    {"id": "riskClass", "screen": "regulatory-portfolio", "label": "Clase de riesgo", "tech": "riskClass", "type": "select", "required": True, "purpose": "Clase A, B o C del dispositivo. No es el módulo Risk Management. Influye en Requirement Pack.", "example": "A", "invalid": "valor fuera de A/B/C", "allowed": ["A", "B", "C"], "roles": ["regulatory-specialist", "regulatory-administrator"]},
    {"id": "countryCode", "screen": "regulatory-portfolio", "label": "País", "tech": "countryCode", "type": "text", "required": True, "purpose": "Código de país del producto (UI demo usa PA).", "example": "PA", "invalid": "(vacío)", "roles": ["regulatory-specialist"]},
    {"id": "mfrLegalName", "screen": "regulatory-manufacturers", "label": "Nombre legal fabricante", "tech": "legalName", "type": "text", "required": True, "purpose": "Razón social del fabricante.", "example": "Acme Medical Co.", "invalid": "(vacío)", "max": 220, "roles": ["regulatory-specialist", "regulatory-administrator"]},
    {"id": "mfrCountry", "screen": "regulatory-manufacturers", "label": "País", "tech": "countryCode", "type": "text", "required": True, "purpose": "País del fabricante.", "example": "CN", "invalid": "(vacío)", "roles": ["regulatory-specialist", "regulatory-administrator"]},
    {"id": "obsDescription", "screen": "regulatory-dossier-detail", "label": "Descripción de la observación", "tech": "description", "type": "textarea", "required": True, "purpose": "Texto de la observación emitida por la autoridad.", "example": "Falta actualización de literatura técnica", "invalid": "(vacío)", "max": 4000, "roles": ["regulatory-manager"]},
    {"id": "ctrsNumber", "screen": "regulatory-dossier-detail", "label": "Número CT/RS", "tech": "registrationNumber", "type": "text", "required": True, "purpose": "Número emitido por la autoridad. No lo genera Compliance 360.", "example": "MQ-4521-07-26", "invalid": "(vacío)", "max": 120, "roles": ["regulatory-manager", "quality-manager"]},
    {"id": "ctrsExpires", "screen": "regulatory-dossier-detail", "label": "Vencimiento ISO", "tech": "expiresOn", "type": "date", "required": True, "purpose": "Fecha de vencimiento del CT/RS (YYYY-MM-DD).", "example": "2029-07-13", "invalid": "2029/07/13 o fecha anterior a emisión", "format": "YYYY-MM-DD", "roles": ["regulatory-manager", "quality-manager"]},
    {"id": "licenseCompany", "screen": "regulatory-licenses", "label": "Compañía", "tech": "companyName", "type": "text", "required": True, "purpose": "Nombre de la empresa titular de la licencia operativa.", "example": "Irving Corro S.A", "invalid": "(vacío)", "roles": ["regulatory-administrator"]},
    {"id": "licenseType", "screen": "regulatory-licenses", "label": "Tipo de licencia", "tech": "licenseType", "type": "text", "required": True, "purpose": "Tipo/descripción de la licencia operativa.", "example": "Distribución de dispositivos médicos", "invalid": "(vacío)", "roles": ["regulatory-administrator"]},
    {"id": "opportunityAmount", "screen": "regulatory-dashboard", "label": "Opportunity Amount", "tech": "opportunityAmount", "type": "currency", "required": False, "purpose": "Valor comercial asociado al producto/expediente. No cambia la decisión de autoridad; alimenta priorización y dashboard.", "example": "15000.00", "invalid": "texto no numérico", "roles": ["regulatory-specialist", "regulatory-manager"]},
    {"id": "maxReception", "screen": "regulatory-dossier-detail", "label": "Fecha máxima de recepción", "tech": "maximumReceptionOn", "type": "datetime", "required": False, "purpose": "Límite para recibir documentos del fabricante. Puede generar alertas si se vence en estado Espera docs fábrica.", "example": "2026-08-01T00:00:00Z", "invalid": "fecha mal formada", "roles": ["regulatory-specialist"]},
]

BUTTONS = [
    {"id": "login-submit", "screen": "login", "label": "Iniciar sesion / Completar login seguro", "action": "POST /api/v1/auth/login", "roles": ["*"], "pre": "Credenciales válidas", "result": "Token + redirección al dashboard"},
    {"id": "new-product", "screen": "regulatory-portfolio", "label": "Nuevo producto + expediente", "action": "POST product + POST dossier", "roles": ["regulatory-specialist", "regulatory-administrator"], "perm": "PRODUCT.MANAGE + DOSSIER.CREATE", "pre": "Bootstrap/autoridad disponible", "result": "Producto en portafolio y expediente en Planning"},
    {"id": "ask-docs", "screen": "regulatory-dossier-detail", "label": "Pedir docs fábrica", "action": "POST …/transition → WaitingManufacturerDocuments", "roles": ["regulatory-specialist"], "perm": "DOSSIER.UPDATE", "pre": "Estado Planning (u permitido)", "result": "Estado Espera docs fábrica"},
    {"id": "docs-received", "screen": "regulatory-dossier-detail", "label": "Docs recibidos", "action": "POST …/transition → DocumentsReceived", "roles": ["regulatory-specialist"], "perm": "DOSSIER.UPDATE", "result": "Estado Docs recibidos"},
    {"id": "assemble", "screen": "regulatory-dossier-detail", "label": "Armar", "action": "POST …/transition → Assembling", "roles": ["regulatory-specialist"], "perm": "DOSSIER.UPDATE", "result": "Estado Armando expediente"},
    {"id": "declare-ready", "screen": "regulatory-dossier-detail", "label": "Declarar técnicamente completo", "action": "POST …/transition → ReadyForSubmission", "roles": ["regulatory-specialist"], "perm": "DOSSIER.UPDATE", "result": "Listo para aprobación interna"},
    {"id": "mark-received", "screen": "regulatory-dossier-detail", "label": "Marcar recibido", "action": "PUT requirement status Received", "roles": ["regulatory-specialist"], "perm": "REQUIREMENT.MANAGE / UPDATE", "result": "Requisito marcado recibido"},
    {"id": "accept-req", "screen": "regulatory-dossier-detail", "label": "Aceptar", "action": "PUT requirement status Accepted", "roles": ["regulatory-reviewer"], "perm": "DOSSIER.REVIEW", "result": "Requisito aceptado"},
    {"id": "reject-req", "screen": "regulatory-dossier-detail", "label": "Rechazar", "action": "PUT requirement status Rejected", "roles": ["regulatory-reviewer"], "perm": "DOSSIER.REVIEW", "result": "Requisito rechazado; Specialist corrige"},
    {"id": "approve-internal", "screen": "regulatory-dossier-detail", "label": "Aprobar internamente para sometimiento", "action": "POST …/approve-for-submission", "roles": ["regulatory-approver"], "perm": "APPROVE_FOR_SUBMISSION", "pre": "ReadyForSubmission", "result": "ApprovedForSubmission — NO es aprobación MINSA/CSS"},
    {"id": "submit", "screen": "regulatory-dossier-detail", "label": "Registrar sometimiento", "action": "POST …/submit", "roles": ["regulatory-submitter"], "perm": "DOSSIER.SUBMIT", "pre": "ApprovedForSubmission (SoD ON)", "result": "Submitted — sometimiento registrado"},
    {"id": "observe", "screen": "regulatory-dossier-detail", "label": "Registrar observación autoridad", "action": "POST …/observations", "roles": ["regulatory-manager"], "perm": "OBSERVATION.MANAGE", "pre": "Submitted / UnderAuthorityReview", "result": "Observed — observación recibida de la autoridad"},
    {"id": "respond-obs", "screen": "regulatory-dossier-detail", "label": "Responder", "action": "POST …/observations/{id}/respond", "roles": ["regulatory-specialist"], "perm": "OBSERVATION.MANAGE", "pre": "Observación abierta", "result": "CorrectingObservation / respuesta registrada"},
    {"id": "approve-ext", "screen": "regulatory-dossier-detail", "label": "Registrar aprobación MINSA/CSS + CT/RS", "action": "POST …/approve", "roles": ["regulatory-manager", "quality-manager"], "perm": "DOSSIER.APPROVE", "pre": "Número CT/RS + vencimiento", "result": "Approved + CT/RS activo"},
    {"id": "add-mfr", "screen": "regulatory-manufacturers", "label": "Alta fabricante", "action": "POST manufacturer", "roles": ["regulatory-specialist", "regulatory-administrator"], "perm": "MANUFACTURER_DOCUMENT.MANAGE", "result": "Fabricante listado (+ cert ISO13485 en flujo demo)"},
    {"id": "add-lic", "screen": "regulatory-licenses", "label": "Nueva licencia", "action": "POST license", "roles": ["regulatory-administrator"], "perm": "LICENSE.MANAGE", "result": "Licencia en listado"},
    {"id": "stage-xlsx", "screen": "regulatory-import", "label": "Stage XLSX", "action": "POST import stage", "roles": ["regulatory-administrator"], "perm": "CONFIGURE", "result": "Job de importación en staging"},
    {"id": "bootstrap", "screen": "regulatory-config", "label": "Bootstrap regulatorio", "action": "Seed authorities + pack", "roles": ["regulatory-administrator", "tenant-administrator"], "perm": "CONFIGURE", "result": "MINSA/CSS + pack de requisitos"},
    {"id": "back-dossiers", "screen": "regulatory-dossier-detail", "label": "← Expedientes", "action": "Volver al listado", "roles": ["*"], "result": "Vista Expedientes"},
]

WORKFLOW = {
    "title": "Flujo certificado del expediente",
    "steps": [
        {"id": "prep", "status": "Planning…ReadyForSubmission", "label": "Preparación", "role": "Regulatory Specialist", "ui": ["Pedir docs fábrica", "Docs recibidos", "Armar", "Marcar recibido", "Declarar técnicamente completo"]},
        {"id": "review", "status": "Assembling / ReadyForSubmission", "label": "Revisión técnica", "role": "Regulatory Reviewer", "ui": ["Aceptar", "Rechazar"]},
        {"id": "internal", "status": "ReadyForSubmission → ApprovedForSubmission", "label": "Aprobación interna para sometimiento", "role": "Regulatory Approver", "ui": ["Aprobar internamente para sometimiento"]},
        {"id": "submit", "status": "ApprovedForSubmission → Submitted", "label": "Sometimiento", "role": "Regulatory Submitter", "ui": ["Registrar sometimiento"]},
        {"id": "authority", "status": "Submitted → UnderAuthorityReview", "label": "Revisión de autoridad", "role": "Regulatory Manager (seguimiento)", "ui": []},
        {"id": "obs", "status": "Observed", "label": "Observación", "role": "Regulatory Manager", "ui": ["Registrar observación autoridad"]},
        {"id": "response", "status": "CorrectingObservation", "label": "Respuesta", "role": "Regulatory Specialist", "ui": ["Responder"]},
        {"id": "resubmit", "status": "Resubmitted", "label": "Resometimiento", "role": "Regulatory Submitter / transición", "ui": ["Registrar sometimiento / transition Resubmitted"]},
        {"id": "external", "status": "Approved", "label": "Decisión externa", "role": "Regulatory Manager / Quality Manager", "ui": ["Registrar aprobación MINSA/CSS + CT/RS"]},
        {"id": "ctrs", "status": "SanitaryRegistration", "label": "CT/RS activo", "role": "Manager / QM", "ui": ["Registros CT/RS"]},
        {"id": "renewal", "status": "Renovacion (pipeline)", "label": "Renovación", "role": "Regulatory Manager", "ui": ["Pipeline Renovacion / StartRenewal"]},
    ],
    "statusLabels": {
        "Draft": "Borrador",
        "Planning": "Planificación / Preparación",
        "WaitingManufacturerDocuments": "Espera docs fábrica",
        "DocumentsReceived": "Docs recibidos",
        "Assembling": "Armando expediente",
        "ReadyForSubmission": "Técnicamente completo / Listo para aprobación interna",
        "ApprovedForSubmission": "Aprobado internamente para sometimiento",
        "Submitted": "Sometido ante autoridad",
        "UnderAuthorityReview": "En revisión de la autoridad",
        "Observed": "Observado por autoridad",
        "CorrectingObservation": "Corrigiendo observación",
        "Resubmitted": "Resometido",
        "Approved": "Aprobación registrada de MINSA/CSS (externa)",
        "Rejected": "Rechazo externo de la autoridad",
    },
}

GLOSSARY = [
    {"term": "Tenant", "def": "Organización aislada en Compliance 360 (ej. Irving Corro S.A).", "example": "TenantId en el JWT", "screens": ["login", "tenant-administration"], "roles": ["tenant-administrator"]},
    {"term": "Regulatory Affairs", "def": "Módulo de asuntos regulatorios de dispositivos médicos.", "example": "Menú Regulatory Management", "screens": ["regulatory-shell"], "roles": ["*"]},
    {"term": "Expediente / Dossier", "def": "Caso de registro ante una autoridad (MINSA/CSS).", "example": "Caso CASE-…", "screens": ["regulatory-dossiers"], "roles": ["regulatory-specialist"]},
    {"term": "Requirement Pack", "def": "Plantilla publicada de requisitos (ej. pack bootstrap con 22 ítems).", "example": "Bootstrap regulatorio", "screens": ["regulatory-config"], "roles": ["regulatory-administrator"]},
    {"term": "Requisito", "def": "Ítem del checklist del expediente que se marca, acepta o rechaza.", "example": "Literatura técnica", "screens": ["regulatory-dossier-detail"], "roles": ["regulatory-reviewer"]},
    {"term": "Fabricante", "def": "Perfil del fabricante y sus certificados (ej. ISO 13485).", "example": "Alta fabricante", "screens": ["regulatory-manufacturers"], "roles": ["regulatory-specialist"]},
    {"term": "Autoridad", "def": "Entidad reguladora (MINSA, CSS u otra configurada).", "example": "Código MINSA", "screens": ["regulatory-config"], "roles": ["regulatory-administrator"]},
    {"term": "MINSA", "def": "Ministerio de Salud — autoridad típica en el bootstrap.", "example": "Aprobación MINSA/CSS", "screens": ["regulatory-dossier-detail"], "roles": ["regulatory-manager"]},
    {"term": "CSS", "def": "Caja de Seguro Social — autoridad típica en el bootstrap.", "example": "Autoridad CSS", "screens": ["regulatory-config"], "roles": ["regulatory-administrator"]},
    {"term": "CT/RS", "def": "Certificado / Registro Sanitario emitido por la autoridad. Se registra tras la aprobación externa.", "example": "MQ-4521-07-26", "screens": ["regulatory-registrations"], "roles": ["regulatory-manager"]},
    {"term": "Clase de riesgo", "def": "Clasificación A/B/C del dispositivo. Distinta del módulo Risk Management.", "example": "A", "screens": ["regulatory-portfolio"], "roles": ["regulatory-specialist"]},
    {"term": "Aprobación interna", "def": "Autorización interna para poder someter. Estado ApprovedForSubmission. No es decisión de MINSA/CSS.", "example": "Aprobar internamente para sometimiento", "screens": ["regulatory-dossier-detail"], "roles": ["regulatory-approver"]},
    {"term": "Sometimiento", "def": "Registro de que el expediente fue enviado a la autoridad.", "example": "Registrar sometimiento", "screens": ["regulatory-dossier-detail"], "roles": ["regulatory-submitter"]},
    {"term": "Observación", "def": "Hallazgo de la autoridad que debe responderse.", "example": "Registrar observación autoridad", "screens": ["regulatory-dossier-detail"], "roles": ["regulatory-manager"]},
    {"term": "Resometimiento", "def": "Nuevo envío tras corregir una observación.", "example": "Estado Resubmitted", "screens": ["regulatory-pipeline"], "roles": ["regulatory-submitter"]},
    {"term": "Decisión externa", "def": "Aprobación o rechazo emitido por la autoridad, registrado en el sistema.", "example": "Registrar aprobación MINSA/CSS + CT/RS", "screens": ["regulatory-dossier-detail"], "roles": ["regulatory-manager"]},
    {"term": "Renovación", "def": "Inicio de un nuevo ciclo cuando el CT/RS vence o está por vencer.", "example": "Columna Renovacion en Pipeline", "screens": ["regulatory-pipeline"], "roles": ["regulatory-manager"]},
    {"term": "Licencia operativa", "def": "Licencia de la empresa (no el CT/RS del producto).", "example": "Nueva licencia", "screens": ["regulatory-licenses"], "roles": ["regulatory-administrator"]},
    {"term": "SoD", "def": "Segregation of Duties: impide que la misma persona prepare, revise, apruebe y someta.", "example": "SoD Settings", "screens": ["regulatory-sod"], "roles": ["tenant-administrator"]},
    {"term": "RBAC", "def": "Control de acceso por roles y permisos atómicos.", "example": "REGULATORY.DOSSIER.SUBMIT", "screens": ["tenant-administration"], "roles": ["tenant-administrator"]},
    {"term": "Pipeline", "def": "Vista kanban del estado de todos los expedientes.", "example": "Columnas Planning…Approved", "screens": ["regulatory-pipeline"], "roles": ["regulatory-manager"]},
    {"term": "Portfolio / Portafolio", "def": "Lista de productos del tenant.", "example": "Nuevo producto + expediente", "screens": ["regulatory-portfolio"], "roles": ["regulatory-specialist"]},
    {"term": "Aging", "def": "Expedientes detenidos más de 14 días (métrica del dashboard).", "example": "Detenidos >14d", "screens": ["regulatory-dashboard"], "roles": ["regulatory-manager"]},
    {"term": "Alert", "def": "Aviso de vencimiento o condición operativa evaluada por el sistema.", "example": "Vista Alertas", "screens": ["regulatory-alerts"], "roles": ["regulatory-viewer"]},
    {"term": "Audit Trail", "def": "Registro histórico de acciones del sistema.", "example": "#/audit-trail", "screens": ["audit-trail"], "roles": ["quality-manager"]},
    {"term": "REGUTRACK", "def": "Libro Excel histórico que Compliance 360 reemplaza vía importación y consola RA.", "example": "Stage XLSX", "screens": ["regulatory-import"], "roles": ["regulatory-administrator"]},
]

ERRORS = [
    {"id": "empty-required", "title": "Campo obligatorio vacío", "why": "El API/UI rechaza crear sin Marca, Nombre regulatorio o Código catálogo.", "fix": "Complete el prompt con un valor no vacío.", "who": "Regulatory Specialist"},
    {"id": "dup-catalog", "title": "Código catálogo duplicado", "why": "Índice único TenantId+CatalogCode.", "fix": "Use otro código de catálogo.", "who": "Regulatory Specialist"},
    {"id": "self-review", "title": "Specialist intenta revisar su propio expediente", "why": "SoD PreventSelfReview.", "fix": "Otro usuario con rol Reviewer debe aceptar/rechazar.", "who": "Regulatory Reviewer"},
    {"id": "rev-approve", "title": "Reviewer intenta aprobar internamente", "why": "Sin permiso APPROVE_FOR_SUBMISSION.", "fix": "El Approver ejecuta «Aprobar internamente para sometimiento».", "who": "Regulatory Approver"},
    {"id": "appr-submit", "title": "Approver intenta someter", "why": "SeparateApproverAndSubmitter / sin SUBMIT.", "fix": "El Submitter registra el sometimiento.", "who": "Regulatory Submitter"},
    {"id": "sub-approve", "title": "Submitter intenta aprobar internamente", "why": "Sin APPROVE_FOR_SUBMISSION.", "fix": "Solo el Approver otorga la autorización interna.", "who": "Regulatory Approver"},
    {"id": "viewer-edit", "title": "Viewer intenta editar", "why": "Solo permisos READ.", "fix": "Solicite un rol operativo al Tenant Administrator.", "who": "Tenant Administrator"},
    {"id": "tac-operate", "title": "Tenant Administrator intenta operar el expediente", "why": "Sin CREATE/SUBMIT/APPROVE por defecto.", "fix": "Asigne roles RA operativos a usuarios dedicados.", "who": "Tenant Administrator"},
    {"id": "submit-without-internal", "title": "Someter sin aprobación interna", "why": "Con SoD ON el estado debe ser ApprovedForSubmission.", "fix": "Complete la aprobación interna primero.", "who": "Regulatory Approver → Submitter"},
    {"id": "ext-without-ctrs", "title": "Aprobar externamente sin número CT/RS", "why": "El prompt exige Número CT/RS obligatorio.", "fix": "Ingrese el número emitido por la autoridad y el vencimiento ISO.", "who": "Regulatory Manager"},
    {"id": "session", "title": "Sesión expirada", "why": "JWT inválido o ausente.", "fix": "Vuelva a #/login e inicie sesión.", "who": "Cualquier usuario"},
]

TUTORIALS = {
    "tac-login": {"title": "Iniciar sesión", "role": "tenant-administrator", "steps": [
        "Abra http://localhost:5272",
        "Si no está autenticado, irá a #/login",
        "Escriba su correo de Tenant Administrator (ej. irvingcorrosk19@gmail.com)",
        "Escriba su contraseña",
        "Presione «Iniciar sesion» o «Completar login seguro»",
        "Verifique que el sidebar muestre Compliance 360 y su tenant",
    ], "result": "Dashboard / menú Enterprise visible", "next": "Puede ir a Tenant Administration"},
    "tac-users": {"title": "Crear / activar usuarios", "role": "tenant-administrator", "steps": [
        "Menú lateral → Tenant Administration (#/tenant-administration)",
        "Abra la sección de usuarios",
        "Cree el usuario con correo corporativo",
        "Active o desactive según corresponda",
    ], "result": "Usuario listado en el tenant", "next": "Asignar roles"},
    "tac-roles": {"title": "Asignar roles", "role": "tenant-administrator", "steps": [
        "En Tenant Administration abra roles/usuarios",
        "Asigne exactamente un rol operativo RA por persona cuando SoD lo requiera",
        "Ejemplo: un usuario = Regulatory Specialist; otro = Regulatory Reviewer",
        "No combine Specialist+Approver+Submitter en la misma persona sin override",
    ], "result": "Permisos persistidos (RBAC)", "next": "Configurar SoD / Bootstrap"},
    "tac-configure-ra": {"title": "Ver configuración Regulatory Affairs", "role": "tenant-administrator", "steps": [
        "Menú → Regulatory Management (#/regulatory)",
        "Abra Configuración",
        "Presione «Bootstrap regulatorio» si aún no hay autoridades/pack",
        "Confirme el mensaje de éxito (toast Bootstrap OK)",
    ], "result": "Autoridades MINSA/CSS y pack disponibles", "next": "Revisar SoD Settings"},
    "tac-sod": {"title": "Consultar SoD", "role": "tenant-administrator", "steps": [
        "En #/regulatory abra SoD Settings",
        "Lea la política JSON (PreventSelfReview, SeparateApproverAndSubmitter, etc.)",
        "Los cambios vía API requieren REGULATORY.SOD.MANAGE",
    ], "result": "Comprende las barreras SoD", "next": "Verificar que no opera expedientes"},
    "tac-no-operate": {"title": "Verificar que no opera el expediente", "role": "tenant-administrator", "steps": [
        "Abra Portafolio",
        "Observe que el botón «Nuevo producto + expediente» no aparece sin PRODUCT.MANAGE+DOSSIER.CREATE",
        "Abra un expediente: no verá Aprobar internamente / Registrar sometimiento / Aprobación externa",
    ], "result": "SoD respetado para TAC", "next": "Cerrar sesión"},
    "tac-logout": {"title": "Cerrar sesión", "role": "tenant-administrator", "steps": [
        "Use la acción de logout de la aplicación",
        "Confirme redirección a #/login",
    ], "result": "Sesión terminada", "next": "—"},
    "adm-bootstrap": {"title": "Bootstrap regulatorio", "role": "regulatory-administrator", "steps": [
        "Login con rol Regulatory Administrator",
        "Menú → Regulatory Management → Configuración",
        "Presione «Bootstrap regulatorio»",
        "Espere toast «Bootstrap OK»",
    ], "result": "Autoridades y pack listos", "next": "Importar REGUTRACK"},
    "adm-authority": {"title": "Crear / verificar autoridades", "role": "regulatory-administrator", "steps": [
        "Tras el bootstrap, las autoridades MINSA y CSS quedan disponibles",
        "Se usan al crear expedientes (selección de autoridad en el flujo de dossier)",
    ], "result": "Autoridad utilizable en prep", "next": "Importación"},
    "adm-import": {"title": "Importación REGUTRACK", "role": "regulatory-administrator", "steps": [
        "Vaya a Importación",
        "Seleccione el archivo XLSX (contrato REGUTRACK)",
        "Presione «Stage XLSX»",
        "Revise el job listado y errores de staging",
    ], "result": "Stage OK / jobs visibles", "next": "Resolver filas con error"},
    "adm-alerts": {"title": "Consultar alertas", "role": "regulatory-administrator", "steps": [
        "Abra Alertas",
        "Revise alertType y message de la evaluación",
    ], "result": "Lista de alertas", "next": "—"},
    "adm-sod-view": {"title": "Ver SoD Settings", "role": "regulatory-administrator", "steps": [
        "Abra SoD Settings",
        "Confirme políticas activas",
    ], "result": "Política visible", "next": "—"},
    "spec-product": {"title": "Crear producto + expediente", "role": "regulatory-specialist", "steps": [
        "Login como Regulatory Specialist",
        "Menú → Regulatory Management → Portafolio",
        "Presione «Nuevo producto + expediente»",
        "En Marca escriba p.ej. DEMO",
        "En Nombre regulatorio escriba el nombre oficial del producto",
        "En Código catálogo escriba un código único CAT-######",
        "Confirme los prompts; el sistema crea producto (país PA, categoría, clase A en flujo demo) y expediente en Planning",
    ], "result": "Fila en Portafolio + expediente en Prep", "next": "Completar checklist"},
    "spec-mfr": {"title": "Alta fabricante", "role": "regulatory-specialist", "steps": [
        "Abra Fabricantes",
        "Presione «Alta fabricante»",
        "Nombre legal: razón social",
        "País: p.ej. CN",
    ], "result": "Fabricante listado", "next": "Pedir docs fábrica"},
    "spec-dossier": {"title": "Abrir expediente y pedir docs", "role": "regulatory-specialist", "steps": [
        "Abra Expedientes o haga clic en el caso del Pipeline",
        "Presione «Pedir docs fábrica»",
        "Observe el estado «Espera docs fábrica»",
    ], "result": "WaitingManufacturerDocuments", "next": "Marcar requisitos"},
    "spec-checklist": {"title": "Marcar requisitos recibidos", "role": "regulatory-specialist", "steps": [
        "En el workspace del expediente revise la lista de requisitos del pack",
        "Para cada documento recibido presione «Marcar recibido»",
        "Use «Docs recibidos» y luego «Armar» según avance",
    ], "result": "Requisitos en progreso / Assembling", "next": "Esperar Reviewer"},
    "spec-ready": {"title": "Declarar técnicamente completo", "role": "regulatory-specialist", "steps": [
        "Cuando la preparación esté lista y la revisión lo permita",
        "Presione «Declarar técnicamente completo»",
        "El estado pasa a ReadyForSubmission",
    ], "result": "Listo para el Approver", "next": "Approver interno"},
    "spec-respond-obs": {"title": "Responder observación", "role": "regulatory-specialist", "steps": [
        "Cuando el Manager registre una observación, abra el expediente",
        "En Observaciones presione «Responder»",
        "Documente la corrección según el API de respond",
    ], "result": "Respuesta registrada", "next": "Resometimiento"},
    "spec-fix-return": {"title": "Corregir expediente devuelto", "role": "regulatory-specialist", "steps": [
        "Si el Reviewer rechazó un requisito, abra el ítem",
        "Corrija la evidencia / datos",
        "Vuelva a marcar recibido para nueva revisión",
    ], "result": "Listo para re-revisión", "next": "Reviewer"},
    "rev-queue": {"title": "Ver cola de revisión", "role": "regulatory-reviewer", "steps": [
        "Login como Regulatory Reviewer",
        "Abra Pipeline o Expedientes",
        "Identifique casos en Armando / Técnicamente completo",
        "Haga clic en el caso",
    ], "result": "Workspace abierto", "next": "Aceptar/Rechazar"},
    "rev-accept": {"title": "Aceptar requisito", "role": "regulatory-reviewer", "steps": [
        "Revise el requisito y su evidencia",
        "Presione «Aceptar»",
        "Confirme que el estado del requisito pasa a Accepted",
    ], "result": "Requisito aceptado", "next": "Siguiente requisito"},
    "rev-reject": {"title": "Rechazar requisito", "role": "regulatory-reviewer", "steps": [
        "Si falta información, presione «Rechazar»",
        "El Specialist debe corregir",
        "Usted no aprueba internamente ni somete",
    ], "result": "Requisito Rejected", "next": "Specialist corrige"},
    "rev-return": {"title": "Entregar a aprobación", "role": "regulatory-reviewer", "steps": [
        "Cuando los requisitos críticos estén Accepted",
        "El Specialist declara técnicamente completo",
        "El Approver toma el caso — usted no presiona Aprobar internamente",
    ], "result": "Cola del Approver", "next": "Regulatory Approver"},
    "appr-queue": {"title": "Cola de aprobación interna", "role": "regulatory-approver", "steps": [
        "Login Approver",
        "Pipeline → columna Técnicamente completo / ReadyForSubmission",
        "Abra el expediente",
    ], "result": "Botón Aprobar internamente visible", "next": "Aprobar"},
    "appr-approve": {"title": "Aprobar internamente", "role": "regulatory-approver", "steps": [
        "Revise resumen y requisitos críticos",
        "Presione «Aprobar internamente para sometimiento»",
        "Lea el nuevo estado: «Aprobado internamente para sometimiento»",
        "Esto NO es la aprobación de MINSA/CSS",
    ], "result": "ApprovedForSubmission", "next": "Submitter"},
    "appr-why-no-submit": {"title": "Por qué no puedo someter", "role": "regulatory-approver", "steps": [
        "Busque el botón «Registrar sometimiento» — no debe aparecer sin SUBMIT",
        "SoD SeparateApproverAndSubmitter obliga a otra persona",
    ], "result": "Comprende la separación", "next": "—"},
    "sub-queue": {"title": "Expedientes autorizados", "role": "regulatory-submitter", "steps": [
        "Login Submitter",
        "Pipeline → Aprobado internamente para sometimiento",
        "Abra el expediente",
    ], "result": "Botón Registrar sometimiento visible", "next": "Someter"},
    "sub-submit": {"title": "Registrar sometimiento", "role": "regulatory-submitter", "steps": [
        "Presione «Registrar sometimiento»",
        "Confirme la acción",
        "Estado: «Sometido ante autoridad»",
    ], "result": "Submitted", "next": "Manager sigue la autoridad"},
    "sub-why-no-approve": {"title": "Por qué no apruebo internamente", "role": "regulatory-submitter", "steps": [
        "Sin permiso APPROVE_FOR_SUBMISSION el botón no aparece",
        "Su función es registrar el envío real, no la autorización interna",
    ], "result": "Separación clara", "next": "—"},
    "mgr-dashboard": {"title": "Supervisar dashboard y pipeline", "role": "regulatory-manager", "steps": [
        "Abra Dashboard: Productos, CT activos, En trámite, Req. críticos, Detenidos >14d, Por vencer",
        "Abra Pipeline y revise columnas incluidas Observed / Renovacion",
    ], "result": "Visión operativa", "next": "Observación"},
    "mgr-observe": {"title": "Registrar observación de autoridad", "role": "regulatory-manager", "steps": [
        "Abra expediente Submitted / UnderAuthorityReview",
        "Presione «Registrar observación autoridad»",
        "Escriba la descripción exacta de la observación",
        "Estado: Observed — «Observación recibida de la autoridad»",
    ], "result": "Specialist puede Responder", "next": "Esperar respuesta"},
    "mgr-external-approve": {"title": "Registrar decisión externa + CT/RS", "role": "regulatory-manager", "steps": [
        "Cuando la autoridad apruebe, abra el expediente",
        "Presione «Registrar aprobación MINSA/CSS + CT/RS»",
        "Número CT/RS: el emitido por la autoridad (obligatorio)",
        "Vencimiento ISO: YYYY-MM-DD",
        "Estado: Aprobación registrada de MINSA/CSS (externa) + CT/RS activo",
    ], "result": "Registro en Registros CT/RS", "next": "Vigencia / Renovación"},
    "mgr-renewal": {"title": "Iniciar / ver renovación", "role": "regulatory-manager", "steps": [
        "En Pipeline revise la columna Renovacion",
        "Use el flujo de renovación (StartRenewal / nuevo proceso renew) según permiso REGISTRATION.MANAGE",
    ], "result": "Ciclo de renovación visible", "next": "—"},
    "mgr-breakglass": {"title": "Break Glass SoD", "role": "regulatory-manager", "steps": [
        "Solo con REGULATORY.SOD.EMERGENCY_OVERRIDE",
        "Cualquier override queda auditado",
        "No use override para saltar rutinariamente Specialist/Approver/Submitter",
    ], "result": "Excepción auditada", "next": "—"},
    "view-read": {"title": "Consultar sin modificar", "role": "regulatory-viewer", "steps": [
        "Recorra Dashboard, Portafolio, Pipeline, Expedientes, CT/RS, Alertas",
        "Abra un expediente en solo lectura",
    ], "result": "Información visible", "next": "—"},
    "view-no-edit": {"title": "Entender bloqueo de edición", "role": "regulatory-viewer", "steps": [
        "Verifique ausencia de botones Nuevo / Aprobar / Someter / Stage",
        "Si necesita operar, solicite rol al TAC",
    ], "result": "Límites claros", "next": "—"},
    "qm-external": {"title": "Registrar CT/RS (QM)", "role": "quality-manager", "steps": [
        "Login Quality Manager",
        "Abra expediente listo para decisión externa",
        "Presione «Registrar aprobación MINSA/CSS + CT/RS» si tiene DOSSIER.APPROVE",
        "Capture Número CT/RS y vencimiento",
    ], "result": "CT/RS registrado", "next": "Audit Trail"},
    "qm-limits": {"title": "Límites vs Regulatory Manager", "role": "quality-manager", "steps": [
        "Usted no prepara dossiers por defecto",
        "Su foco QMS + registro externo; la supervisión diaria del pipeline es del Manager",
    ], "result": "Roles no confundidos", "next": "—"},
    "qm-audit": {"title": "Consultar auditoría", "role": "quality-manager", "steps": [
        "Menú → Audit Trail",
        "Busque eventos REGULATORY relevantes",
    ], "result": "Trazabilidad", "next": "—"},
}

def write(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content.strip() + "\n", encoding="utf-8")


def dump_json(name: str, obj) -> None:
    write(ROOT / "data" / name, json.dumps(obj, ensure_ascii=False, indent=2))


MANUAL_CSS = r"""
:root {
  --bg: #f4f7fb;
  --bg-elev: #ffffff;
  --text: #0f172a;
  --muted: #64748b;
  --line: #e2e8f0;
  --brand: #0ea5e9;
  --brand-2: #0369a1;
  --accent: #22c55e;
  --warn: #f59e0b;
  --danger: #ef4444;
  --sidebar: #0b1220;
  --sidebar-text: #e2e8f0;
  --marker: #0ea5e9;
  --shadow: 0 18px 50px rgba(15, 23, 42, .08);
  --radius: 16px;
  --font: "Segoe UI", "Candara", "Gill Sans", sans-serif;
  --mono: ui-monospace, "Cascadia Code", Consolas, monospace;
}
[data-theme="dark"] {
  --bg: #020617;
  --bg-elev: #0f172a;
  --text: #e2e8f0;
  --muted: #94a3b8;
  --line: #1e293b;
  --sidebar: #020617;
  --sidebar-text: #e2e8f0;
  --shadow: 0 18px 50px rgba(0,0,0,.45);
}
* { box-sizing: border-box; }
html { scroll-behavior: smooth; }
body {
  margin: 0; font-family: var(--font); color: var(--text); background:
    radial-gradient(1200px 600px at 10% -10%, rgba(14,165,233,.18), transparent 55%),
    radial-gradient(900px 500px at 100% 0%, rgba(34,197,94,.12), transparent 50%),
    var(--bg);
  min-height: 100vh; line-height: 1.5;
}
a { color: var(--brand-2); }
[data-theme="dark"] a { color: #7dd3fc; }
.skip { position:absolute; left:-9999px; }
.skip:focus { left:1rem; top:1rem; background:#fff; padding:.5rem 1rem; z-index:99; }
.app-header {
  position: sticky; top: 0; z-index: 40; backdrop-filter: blur(12px);
  background: color-mix(in srgb, var(--bg-elev) 86%, transparent);
  border-bottom: 1px solid var(--line);
  display:flex; gap:1rem; align-items:center; justify-content:space-between;
  padding: .75rem 1.25rem;
}
.brand-lockup { display:flex; gap:.75rem; align-items:center; text-decoration:none; color:inherit; }
.brand-mark {
  width:42px; height:42px; border-radius:12px; display:grid; place-items:center;
  background: linear-gradient(145deg, #0ea5e9, #0369a1); color:#fff; font-weight:800; letter-spacing:.02em;
}
.brand-title { font-weight: 800; font-size: 1.05rem; }
.brand-sub { color: var(--muted); font-size: .78rem; }
.header-actions { display:flex; gap:.5rem; flex-wrap:wrap; align-items:center; }
.btn {
  border:1px solid var(--line); background: var(--bg-elev); color: var(--text);
  border-radius: 999px; padding: .45rem .9rem; cursor:pointer; font: inherit;
}
.btn:hover, .btn:focus-visible { outline: 2px solid var(--brand); outline-offset: 2px; }
.btn.primary { background: linear-gradient(135deg, #0ea5e9, #0284c7); color:#04111f; border:0; font-weight:700; }
.btn.ghost { background: transparent; }
.layout { display:grid; grid-template-columns: 280px 1fr; min-height: calc(100vh - 64px); }
@media (max-width: 960px) { .layout { grid-template-columns: 1fr; } .sidebar { position: static; } }
.sidebar {
  background: var(--sidebar); color: var(--sidebar-text); padding: 1rem; border-right: 1px solid var(--line);
}
.sidebar a { color: #cbd5e1; text-decoration:none; display:block; padding:.45rem .6rem; border-radius:.55rem; }
.sidebar a:hover, .sidebar a[aria-current="page"] { background: rgba(14,165,233,.2); color:#fff; }
.sidebar h3 { font-size:.72rem; text-transform:uppercase; letter-spacing:.08em; color:#94a3b8; margin:1rem 0 .4rem; }
.main { padding: 1.25rem clamp(1rem, 3vw, 2rem) 3rem; }
.hero {
  display:grid; gap:1.25rem; grid-template-columns: 1.2fr .8fr; align-items:stretch;
  margin-bottom: 1.5rem;
}
@media (max-width: 900px) { .hero { grid-template-columns: 1fr; } }
.hero-card, .card {
  background: var(--bg-elev); border:1px solid var(--line); border-radius: var(--radius);
  box-shadow: var(--shadow); padding: 1.25rem 1.4rem;
}
.hero h1 { font-size: clamp(1.8rem, 4vw, 2.8rem); line-height:1.1; margin:.2rem 0 .6rem; }
.lead { color: var(--muted); font-size: 1.05rem; max-width: 52ch; }
.role-grid { display:grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap:1rem; }
.role-card {
  background: var(--bg-elev); border:1px solid var(--line); border-radius: var(--radius);
  padding:1rem; display:flex; flex-direction:column; gap:.55rem; box-shadow: var(--shadow);
  border-top: 4px solid var(--role-color, var(--brand));
}
.role-card h3 { margin:0; }
.pill { display:inline-flex; align-items:center; gap:.35rem; padding:.15rem .55rem; border-radius:999px;
  background: color-mix(in srgb, var(--role-color, var(--brand)) 18%, transparent); font-size:.75rem; font-weight:600; }
.muted { color: var(--muted); }
.list-tight { margin:.2rem 0; padding-left: 1.1rem; }
.list-tight li { margin:.2rem 0; }
.progress-ring {
  height: 10px; background: var(--line); border-radius:999px; overflow:hidden;
}
.progress-ring > span { display:block; height:100%; background: linear-gradient(90deg, #22c55e, #0ea5e9); width:0%; transition: width .35s ease; }
.search-box { width:min(420px,100%); display:flex; gap:.4rem; }
.search-box input {
  flex:1; border:1px solid var(--line); border-radius:999px; padding:.5rem .9rem;
  background: var(--bg-elev); color: var(--text); font: inherit;
}
.sim-shell {
  display:grid; grid-template-columns: 220px 1fr; gap:0; border:1px solid var(--line);
  border-radius: var(--radius); overflow:hidden; background: var(--bg-elev); min-height: 480px;
}
@media (max-width: 800px) { .sim-shell { grid-template-columns: 1fr; } }
.sim-nav { background:#0b1220; color:#e2e8f0; padding:.75rem; }
.sim-nav button {
  width:100%; text-align:left; margin:.2rem 0; border:1px solid rgba(148,163,184,.35);
  background:rgba(15,23,42,.55); color:#e2e8f0; padding:.45rem .7rem; border-radius:.55rem; cursor:pointer;
}
.sim-nav button.active { background:#0ea5e9; color:#0f172a; font-weight:700; }
.sim-body { padding:1rem; position:relative; }
.sim-topbar {
  display:flex; justify-content:space-between; gap:.75rem; align-items:center;
  border-bottom:1px solid var(--line); padding-bottom:.75rem; margin-bottom:.75rem;
}
.marker {
  position:absolute; width:26px; height:26px; border-radius:50%; background: var(--marker); color:#04111f;
  font-weight:800; font-size:.78rem; display:grid; place-items:center; cursor:pointer; border:2px solid #fff;
  box-shadow: 0 4px 14px rgba(14,165,233,.45);
}
.marker:focus-visible { outline: 3px solid #fbbf24; }
.panel {
  position: sticky; top: 76px; background: var(--bg-elev); border:1px solid var(--line);
  border-radius: var(--radius); padding:1rem; box-shadow: var(--shadow);
}
.flow {
  display:flex; flex-wrap:wrap; gap:.4rem; align-items:center; margin: .75rem 0 1.25rem;
}
.flow .step {
  background: var(--bg-elev); border:1px solid var(--line); border-radius:999px; padding:.35rem .7rem; font-size:.8rem;
}
.flow .step.current { border-color: var(--brand); background: color-mix(in srgb, var(--brand) 15%, var(--bg-elev)); font-weight:700; }
.flow .arrow { color: var(--muted); }
.table { width:100%; border-collapse: collapse; font-size:.9rem; }
.table th, .table td { border-bottom:1px solid var(--line); padding:.55rem .4rem; text-align:left; vertical-align:top; }
.badge { display:inline-block; padding:.1rem .45rem; border-radius:999px; background:rgba(14,165,233,.15); color:var(--brand-2); font-size:.72rem; }
[data-theme="dark"] .badge { color:#7dd3fc; }
.toast {
  position: fixed; right: 1rem; bottom: 1rem; background:#022c22; color:#ecfdf5; padding:.8rem 1rem;
  border-radius: .75rem; box-shadow: var(--shadow); z-index: 80; display:none;
}
.toast.show { display:block; animation: rise .35s ease; }
@keyframes rise { from { transform: translateY(12px); opacity:0;} to { transform:none; opacity:1;} }
@media (prefers-reduced-motion: reduce) {
  * { animation: none !important; transition: none !important; }
}
.checklist label { display:flex; gap:.55rem; align-items:flex-start; margin:.35rem 0; }
.kbd { font-family: var(--mono); font-size:.78rem; background: var(--line); padding:.05rem .35rem; border-radius:.35rem; }
.footer-note { margin-top:2rem; color:var(--muted); font-size:.85rem; }
.dialog-backdrop {
  position:fixed; inset:0; background:rgba(2,6,23,.55); display:none; place-items:center; z-index:70; padding:1rem;
}
.dialog-backdrop.open { display:grid; }
.dialog {
  width:min(560px,100%); background:var(--bg-elev); border-radius: var(--radius); padding:1.2rem;
  border:1px solid var(--line); box-shadow: var(--shadow);
}
.section-title { margin: 1.75rem 0 .6rem; font-size: 1.35rem; }
.tabs { display:flex; flex-wrap:wrap; gap:.4rem; margin:.75rem 0; }
.tabs button[aria-selected="true"] { background: var(--brand); color:#04111f; border-color: transparent; font-weight:700; }
"""

LIGHT_THEME = """/* light defaults live in manual.css :root */\nbody[data-theme="light"] { color-scheme: light; }\n"""
DARK_THEME = """/* dark overrides live in manual.css [data-theme=dark] */\nbody[data-theme="dark"] { color-scheme: dark; }\n"""

DATA_JS_HEAD = """/* Auto-generated — facts from RoleCatalog, regulatory-affairs.js, security docs */
window.C360_MANUAL = window.C360_MANUAL || {};
"""


def build_data_js() -> str:
    payload = {
        "roles": ROLES,
        "screens": SCREENS,
        "fields": FIELDS,
        "buttons": BUTTONS,
        "workflow": WORKFLOW,
        "glossary": GLOSSARY,
        "errors": ERRORS,
        "tutorials": TUTORIALS,
        "appUrl": "http://localhost:5272",
        "raRoute": "#/regulatory",
        "loginRoute": "#/login",
        "version": "1.0.0-certified-flow",
    }
    return DATA_JS_HEAD + "window.C360_MANUAL.data = " + json.dumps(payload, ensure_ascii=False, indent=2) + ";\n"


PROGRESS_JS = r"""
window.C360_MANUAL = window.C360_MANUAL || {};
(function () {
  const KEY = "c360.manual.progress.v1";
  function load() {
    try { return JSON.parse(localStorage.getItem(KEY) || "{}"); } catch { return {}; }
  }
  function save(state) { localStorage.setItem(KEY, JSON.stringify(state)); }
  function ensure() {
    const s = load();
    s.roles = s.roles || {};
    s.theme = s.theme || "system";
    s.lastRole = s.lastRole || null;
    s.lastChapter = s.lastChapter || null;
    s.completedTutorials = s.completedTutorials || [];
    s.completedSimSteps = s.completedSimSteps || [];
    s.checklist = s.checklist || {};
    return s;
  }
  function roleProgress(roleId) {
    const s = ensure();
    const role = (window.C360_MANUAL.data.roles || []).find(r => r.id === roleId);
    if (!role) return 0;
    const tuts = role.tutorials || [];
    const done = tuts.filter(t => s.completedTutorials.includes(t)).length;
    const checks = Object.values(s.checklist[roleId] || {}).filter(Boolean).length;
    const total = tuts.length + 10;
    return Math.round(((done + checks) / total) * 100);
  }
  function overall() {
    const roles = window.C360_MANUAL.data.roles || [];
    if (!roles.length) return 0;
    return Math.round(roles.reduce((a, r) => a + roleProgress(r.id), 0) / roles.length);
  }
  function markTutorial(id) {
    const s = ensure();
    if (!s.completedTutorials.includes(id)) s.completedTutorials.push(id);
    save(s);
    window.dispatchEvent(new Event("c360-progress"));
  }
  function setTheme(theme) {
    const s = ensure();
    s.theme = theme;
    save(s);
    applyTheme();
  }
  function applyTheme() {
    const s = ensure();
    let theme = s.theme;
    if (theme === "system") {
      theme = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }
    document.documentElement.setAttribute("data-theme", theme);
    document.body && document.body.setAttribute("data-theme", theme);
  }
  function reset() {
    localStorage.removeItem(KEY);
    window.dispatchEvent(new Event("c360-progress"));
  }
  function setLast(roleId, chapter) {
    const s = ensure();
    s.lastRole = roleId;
    s.lastChapter = chapter;
    save(s);
  }
  function toggleCheck(roleId, key, val) {
    const s = ensure();
    s.checklist[roleId] = s.checklist[roleId] || {};
    s.checklist[roleId][key] = !!val;
    save(s);
    window.dispatchEvent(new Event("c360-progress"));
  }
  window.C360_MANUAL.progress = {
    load: ensure, save, roleProgress, overall, markTutorial, setTheme, applyTheme, reset, setLast, toggleCheck, KEY
  };
})();
"""

SEARCH_JS = r"""
window.C360_MANUAL = window.C360_MANUAL || {};
(function () {
  function normalize(s) { return String(s || "").toLowerCase().normalize("NFD").replace(/\p{Diacritic}/gu, ""); }
  function search(q) {
    const data = window.C360_MANUAL.data;
    const n = normalize(q).trim();
    if (!n || n.length < 2) return [];
    const out = [];
    (data.roles || []).forEach(r => {
      const blob = normalize([r.name, r.purpose, ...(r.can||[]), ...(r.cannot||[])].join(" "));
      if (blob.includes(n)) out.push({ type: "Rol", title: r.name, href: "roles/" + r.file, snippet: r.purpose });
    });
    (data.screens || []).forEach(s => {
      const blob = normalize([s.name, s.route, s.objective].join(" "));
      if (blob.includes(n)) out.push({ type: "Pantalla", title: s.name, href: "index.html#screens", snippet: s.route + " — " + s.objective });
    });
    (data.fields || []).forEach(f => {
      const blob = normalize([f.label, f.purpose, f.example, f.tech].join(" "));
      if (blob.includes(n)) out.push({ type: "Campo", title: f.label, href: "index.html#fields", snippet: f.purpose });
    });
    (data.buttons || []).forEach(b => {
      const blob = normalize([b.label, b.action, b.result].join(" "));
      if (blob.includes(n)) out.push({ type: "Botón", title: b.label, href: "index.html#buttons", snippet: b.result });
    });
    (data.glossary || []).forEach(g => {
      const blob = normalize([g.term, g.def, g.example].join(" "));
      if (blob.includes(n)) out.push({ type: "Glosario", title: g.term, href: "index.html#glossary", snippet: g.def });
    });
    Object.entries(data.tutorials || {}).forEach(([id, t]) => {
      const blob = normalize([t.title, ...(t.steps||[])].join(" "));
      if (blob.includes(n)) out.push({ type: "Tutorial", title: t.title, href: "roles/" + ((data.roles.find(r => r.id === t.role)||{}).file || "regulatory-specialist.html") + "#" + id, snippet: (t.steps||[])[0] || "" });
    });
    (data.errors || []).forEach(e => {
      const blob = normalize([e.title, e.why, e.fix].join(" "));
      if (blob.includes(n)) out.push({ type: "Error", title: e.title, href: "index.html#errors", snippet: e.fix });
    });
    return out.slice(0, 40);
  }
  window.C360_MANUAL.search = search;
})();
"""

SIMULATOR_JS = r"""
window.C360_MANUAL = window.C360_MANUAL || {};
(function () {
  const STEPS = [
    { id: 1, role: "Regulatory Specialist", title: "Crear producto + expediente", action: "new-product" },
    { id: 2, role: "Regulatory Specialist", title: "Pedir docs fábrica", action: "ask-docs" },
    { id: 3, role: "Regulatory Specialist", title: "Marcar requisito recibido", action: "mark-received" },
    { id: 4, role: "Regulatory Specialist", title: "Armar expediente", action: "assemble" },
    { id: 5, role: "Regulatory Reviewer", title: "Rechazar un requisito", action: "reject-req" },
    { id: 6, role: "Regulatory Specialist", title: "Corregir y marcar de nuevo", action: "mark-received" },
    { id: 7, role: "Regulatory Reviewer", title: "Aceptar requisito", action: "accept-req" },
    { id: 8, role: "Regulatory Specialist", title: "Declarar técnicamente completo", action: "declare-ready" },
    { id: 9, role: "Regulatory Approver", title: "Aprobar internamente para sometimiento", action: "approve-internal" },
    { id: 10, role: "Regulatory Submitter", title: "Registrar sometimiento", action: "submit" },
    { id: 11, role: "Regulatory Manager", title: "Registrar observación de autoridad", action: "observe" },
    { id: 12, role: "Regulatory Specialist", title: "Responder observación", action: "respond-obs" },
    { id: 13, role: "Regulatory Submitter", title: "Resometimiento (estado Resubmitted / nuevo submit)", action: "submit" },
    { id: 14, role: "Regulatory Manager", title: "Registrar aprobación externa + CT/RS", action: "approve-ext" },
    { id: 15, role: "Regulatory Manager", title: "Ver CT/RS activo e iniciar renovación", action: "renewal" }
  ];

  function stateMachine() {
    return {
      status: "Planning",
      product: null,
      requirements: [{ id: "R1", name: "Literatura técnica", status: "Pending", critical: true }],
      observations: [],
      ctrs: null,
      log: []
    };
  }

  function render(root) {
    if (!root) return;
    let stepIdx = 0;
    let sim = stateMachine();
    const labels = (window.C360_MANUAL.data.workflow || {}).statusLabels || {};

    function paint() {
      const step = STEPS[stepIdx];
      root.innerHTML = `
        <div class="card" id="guided-sim">
          <div style="display:flex;justify-content:space-between;gap:1rem;flex-wrap:wrap;align-items:center">
            <div>
              <div class="pill" style="--role-color:#0ea5e9">Simulación guiada del expediente</div>
              <h2 style="margin:.4rem 0">Paso ${step.id} de ${STEPS.length}: ${step.title}</h2>
              <p class="muted"><strong>Ahora actúas como ${step.role}.</strong> En el sistema real cada rol usa una cuenta distinta (SoD).</p>
            </div>
            <div style="min-width:180px">
              <div class="muted" style="font-size:.8rem">Progreso</div>
              <div class="progress-ring"><span style="width:${Math.round((stepIdx/(STEPS.length-1))*100)}%"></span></div>
            </div>
          </div>
          <div class="flow" aria-label="Estado actual">
            <span class="step current">${labels[sim.status] || sim.status}</span>
            ${sim.ctrs ? `<span class="step">CT/RS: ${sim.ctrs}</span>` : ""}
          </div>
          <div class="sim-shell" style="margin-top:.75rem">
            <aside class="sim-nav" aria-label="Menú RA simulado">
              <div class="brand-title" style="margin-bottom:.5rem">Consola RA</div>
              ${["Dashboard","Portafolio","Pipeline","Expedientes","Registros CT/RS"].map((x,i)=>`<button type="button" class="${i===3?'active':''}">${x}</button>`).join("")}
            </aside>
            <div class="sim-body">
              <div class="sim-topbar">
                <div><strong>Expediente DEMO-001</strong><div class="muted">Tenant lab · Rol: ${step.role}</div></div>
                <span class="badge">${labels[sim.status] || sim.status}</span>
              </div>
              <ul class="list-tight">
                ${sim.requirements.map(r => `<li>${r.critical?'⚠ ':''}${r.name}: <strong>${r.status}</strong></li>`).join("")}
              </ul>
              <div class="ra-actions" style="display:flex;flex-wrap:wrap;gap:.4rem;margin-top:.8rem">
                <button type="button" class="btn primary" id="sim-do">${step.title}</button>
                <button type="button" class="btn" id="sim-next" ${stepIdx>=STEPS.length-1?"disabled":""}>Siguiente paso</button>
                <button type="button" class="btn ghost" id="sim-reset">Reiniciar simulación</button>
              </div>
              <p class="muted" style="margin-top:.8rem">Historial: ${sim.log.slice(-4).join(" → ") || "—"}</p>
            </div>
          </div>
        </div>`;
      root.querySelector("#sim-do").onclick = () => {
        apply(step);
        const p = window.C360_MANUAL.progress;
        const st = p.load();
        if (!st.completedSimSteps.includes(step.id)) st.completedSimSteps.push(step.id);
        p.save(st);
        window.C360_MANUAL.ui?.toast("Acción simulada: " + step.title, "success");
        paint();
      };
      root.querySelector("#sim-next").onclick = () => { if (stepIdx < STEPS.length - 1) { stepIdx++; paint(); } };
      root.querySelector("#sim-reset").onclick = () => { stepIdx = 0; sim = stateMachine(); paint(); };
    }

    function apply(step) {
      const map = {
        "new-product": () => { sim.product = "PRODUCTO DEMO"; sim.status = "Planning"; },
        "ask-docs": () => { sim.status = "WaitingManufacturerDocuments"; },
        "mark-received": () => { sim.requirements[0].status = "Received"; sim.status = "DocumentsReceived"; },
        "assemble": () => { sim.status = "Assembling"; },
        "reject-req": () => { sim.requirements[0].status = "Rejected"; },
        "accept-req": () => { sim.requirements[0].status = "Accepted"; },
        "declare-ready": () => { sim.status = "ReadyForSubmission"; },
        "approve-internal": () => { sim.status = "ApprovedForSubmission"; },
        "submit": () => { sim.status = sim.status === "CorrectingObservation" || sim.log.includes("observe") ? "Resubmitted" : "Submitted"; },
        "observe": () => { sim.status = "Observed"; sim.observations.push("Falta literatura"); },
        "respond-obs": () => { sim.status = "CorrectingObservation"; },
        "approve-ext": () => { sim.status = "Approved"; sim.ctrs = "MQ-4521-07-26"; },
        "renewal": () => { sim.log.push("Renovación iniciada"); }
      };
      (map[step.action] || (()=>{}))();
      sim.log.push(step.title);
    }

    paint();
  }

  window.C360_MANUAL.simulator = { STEPS, render };
})();
"""

MANUAL_JS = r"""
window.C360_MANUAL = window.C360_MANUAL || {};
(function () {
  const M = window.C360_MANUAL;

  function toast(msg) {
    let el = document.getElementById("c360-toast");
    if (!el) {
      el = document.createElement("div");
      el.id = "c360-toast";
      el.className = "toast";
      el.setAttribute("role", "status");
      document.body.appendChild(el);
    }
    el.textContent = msg;
    el.classList.add("show");
    setTimeout(() => el.classList.remove("show"), 2800);
  }

  function qs(sel, root) { return (root || document).querySelector(sel); }
  function qsa(sel, root) { return [...(root || document).querySelectorAll(sel)]; }

  function renderRoleCards(container) {
    if (!container) return;
    container.innerHTML = M.data.roles.map(r => `
      <article class="role-card" style="--role-color:${r.color}" data-role="${r.id}">
        <div class="pill">${r.short}</div>
        <h3>${r.name}</h3>
        <p class="muted">${r.purpose}</p>
        <p><strong>Puede:</strong></p>
        <ul class="list-tight">${r.can.slice(0,3).map(x=>`<li>${x}</li>`).join("")}</ul>
        <p><strong>No puede:</strong></p>
        <ul class="list-tight">${r.cannot.slice(0,2).map(x=>`<li>${x}</li>`).join("")}</ul>
        <p class="muted"><strong>Etapa:</strong> ${r.flowStage}</p>
        <a class="btn primary" href="roles/${r.file}">Entrar al manual</a>
      </article>`).join("");
  }

  function renderFlow(container) {
    if (!container) return;
    const steps = M.data.workflow.steps;
    container.innerHTML = steps.map((s,i) =>
      `<span class="step" title="${s.role}">${s.label}</span>${i<steps.length-1?'<span class="arrow">→</span>':''}`
    ).join("");
  }

  function bindSearch() {
    const input = qs("#global-search");
    const out = qs("#search-results");
    if (!input || !out) return;
    const run = () => {
      const hits = M.search(input.value);
      if (!hits.length) { out.innerHTML = input.value.trim().length>1 ? "<p class='muted'>Sin resultados.</p>" : ""; return; }
      const groups = {};
      hits.forEach(h => { (groups[h.type]=groups[h.type]||[]).push(h); });
      out.innerHTML = Object.entries(groups).map(([type, items]) => `
        <div class="card" style="margin:.5rem 0">
          <h3>${type}</h3>
          <ul class="list-tight">${items.map(i => `<li><a href="${i.href}"><strong>${i.title}</strong></a> — <span class="muted">${i.snippet||""}</span></li>`).join("")}</ul>
        </div>`).join("");
    };
    input.addEventListener("input", run);
    input.addEventListener("keydown", e => { if (e.key === "Enter") { e.preventDefault(); run(); } });
  }

  function bindTheme() {
    qsa("[data-theme-set]").forEach(btn => {
      btn.addEventListener("click", () => {
        M.progress.setTheme(btn.getAttribute("data-theme-set"));
        toast("Tema: " + btn.getAttribute("data-theme-set"));
      });
    });
    M.progress.applyTheme();
  }

  function updateProgressUI() {
    qsa("[data-progress-overall]").forEach(el => {
      const pct = M.progress.overall();
      el.textContent = pct + "%";
      const bar = el.parentElement?.querySelector(".progress-ring > span");
      if (bar) bar.style.width = pct + "%";
    });
    qsa("[data-progress-role]").forEach(el => {
      const id = el.getAttribute("data-progress-role");
      const pct = M.progress.roleProgress(id);
      el.textContent = pct + "%";
      const bar = el.parentElement?.querySelector(".progress-ring > span");
      if (bar) bar.style.width = pct + "%";
    });
    const last = M.progress.load();
    const lastEl = qs("#last-chapter");
    if (lastEl) lastEl.textContent = last.lastRole ? `${last.lastRole}${last.lastChapter ? " · " + last.lastChapter : ""}` : "Ninguno aún";
  }

  function bindMarkers(root) {
    const panel = qs("#marker-panel", root) || qs("#marker-panel");
    qsa(".marker", root).forEach(m => {
      m.addEventListener("click", () => showMarker(m, panel));
      m.addEventListener("keydown", e => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); showMarker(m, panel); } });
    });
    const next = qs("#marker-next");
    if (next) next.onclick = () => {
      const all = qsa(".marker");
      const cur = all.findIndex(x => x.getAttribute("aria-current") === "true");
      const nxt = all[(cur + 1) % all.length];
      if (nxt) { nxt.focus(); showMarker(nxt, panel); }
    };
  }

  function showMarker(m, panel) {
    qsa(".marker").forEach(x => x.removeAttribute("aria-current"));
    m.setAttribute("aria-current", "true");
    if (!panel) return;
    panel.innerHTML = `
      <h3>${m.dataset.name || "Elemento"}</h3>
      <p>${m.dataset.what || ""}</p>
      <p><strong>Cuándo:</strong> ${m.dataset.when || "—"}</p>
      <p><strong>Rol:</strong> ${m.dataset.role || "—"}</p>
      <p><strong>Al usarlo:</strong> ${m.dataset.result || "—"}</p>
      <p><strong>Error posible:</strong> ${m.dataset.error || "—"}</p>
      <p><strong>Recomendación:</strong> ${m.dataset.tip || "—"}</p>
      <button type="button" class="btn primary" id="marker-next">Siguiente elemento</button>`;
    bindMarkers(panel.parentElement);
  }

  function renderGlossary() {
    const el = qs("#glossary-list");
    if (!el) return;
    el.innerHTML = M.data.glossary.map(g => `
      <details class="card" style="margin:.4rem 0">
        <summary><strong>${g.term}</strong></summary>
        <p>${g.def}</p>
        <p class="muted">Ejemplo: ${g.example}</p>
        <p class="muted">Pantallas: ${(g.screens||[]).join(", ")}</p>
      </details>`).join("");
  }

  function renderErrors() {
    const el = qs("#error-list");
    if (!el) return;
    el.innerHTML = M.data.errors.map(e => `
      <article class="card" style="margin:.5rem 0;border-left:4px solid var(--danger)">
        <h3>${e.title}</h3>
        <p><strong>Qué ocurrió / por qué:</strong> ${e.why}</p>
        <p><strong>Cómo corregir:</strong> ${e.fix}</p>
        <p><strong>Quién debe actuar:</strong> ${e.who}</p>
      </article>`).join("");
  }

  function renderDictionaries() {
    const fields = qs("#fields-table");
    if (fields) {
      fields.innerHTML = `<tr><th>Pantalla</th><th>Campo</th><th>Tipo</th><th>Oblig.</th><th>Para qué</th><th>Ejemplo</th></tr>` +
        M.data.fields.map(f => `<tr><td>${f.screen}</td><td>${f.label}</td><td>${f.type}</td><td>${f.required?"Sí":"No"}</td><td>${f.purpose}</td><td><code>${f.example||""}</code></td></tr>`).join("");
    }
    const buttons = qs("#buttons-table");
    if (buttons) {
      buttons.innerHTML = `<tr><th>Pantalla</th><th>Botón</th><th>Acción</th><th>Resultado</th><th>Permiso</th></tr>` +
        M.data.buttons.map(b => `<tr><td>${b.screen}</td><td>${b.label}</td><td>${b.action}</td><td>${b.result}</td><td>${b.perm||"—"}</td></tr>`).join("");
    }
    const screens = qs("#screens-table");
    if (screens) {
      screens.innerHTML = `<tr><th>Nombre</th><th>Ruta</th><th>Módulo</th><th>Objetivo</th></tr>` +
        M.data.screens.map(s => `<tr><td>${s.name}</td><td><code>${s.route}</code></td><td>${s.module}</td><td>${s.objective}</td></tr>`).join("");
    }
  }

  function openDialog(id) {
    const d = qs(id);
    if (!d) return;
    d.classList.add("open");
    const close = () => d.classList.remove("open");
    d.onclick = e => { if (e.target === d) close(); };
    qs("[data-close]", d)?.addEventListener("click", close);
    document.addEventListener("keydown", function esc(ev) {
      if (ev.key === "Escape") { close(); document.removeEventListener("keydown", esc); }
    });
  }

  function initIndex() {
    renderRoleCards(qs("#role-grid"));
    renderFlow(qs("#flow-strip"));
    renderGlossary();
    renderErrors();
    renderDictionaries();
    bindSearch();
    bindTheme();
    updateProgressUI();
    window.addEventListener("c360-progress", updateProgressUI);
    qs("#btn-reset-learning")?.addEventListener("click", () => {
      if (confirm("¿Reiniciar todo el progreso de aprendizaje?")) { M.progress.reset(); updateProgressUI(); toast("Progreso reiniciado"); }
    });
    qs("#btn-start")?.addEventListener("click", () => qs("#roles")?.scrollIntoView({ behavior: "smooth" }));
    M.simulator.render(qs("#simulator-root"));
    bindMarkers(document);
  }

  function initRolePage(roleId) {
    const role = M.data.roles.find(r => r.id === roleId);
    if (!role) return;
    M.progress.setLast(roleId, "intro");
    bindTheme();
    updateProgressUI();
    window.addEventListener("c360-progress", updateProgressUI);
    bindMarkers(document);
    bindSearch();

    const tutRoot = qs("#tutorials");
    if (tutRoot) {
      tutRoot.innerHTML = (role.tutorials || []).map(id => {
        const t = M.data.tutorials[id];
        if (!t) return "";
        return `<article class="card" id="${id}" style="margin:.75rem 0">
          <h3>${t.title}</h3>
          <p class="muted">Rol: ${role.name}</p>
          <ol>${(t.steps||[]).map(s=>`<li>${s}</li>`).join("")}</ol>
          <p><strong>Resultado esperado:</strong> ${t.result}</p>
          <p><strong>Qué sigue:</strong> ${t.next}</p>
          <button type="button" class="btn primary" data-complete-tutorial="${id}">Marcar tutorial completado</button>
        </article>`;
      }).join("");
      qsa("[data-complete-tutorial]").forEach(btn => btn.addEventListener("click", () => {
        M.progress.markTutorial(btn.getAttribute("data-complete-tutorial"));
        toast("Tutorial guardado en su progreso");
        updateProgressUI();
      }));
    }

    const checks = [
      "Comprendo mi responsabilidad",
      "Sé iniciar sesión",
      "Sé encontrar mis tareas",
      "Sé completar formularios",
      "Sé qué campos son obligatorios",
      "Sé interpretar estados",
      "Sé qué acciones no puedo realizar",
      "Sé quién continúa el proceso",
      "Sé reconocer errores",
      "Sé cerrar sesión"
    ];
    const chk = qs("#role-checklist");
    if (chk) {
      const st = M.progress.load().checklist[roleId] || {};
      chk.innerHTML = checks.map((c,i) => {
        const key = "c"+i;
        return `<label><input type="checkbox" data-check="${key}" ${st[key]?"checked":""}> ${c}</label>`;
      }).join("");
      qsa("[data-check]", chk).forEach(inp => inp.addEventListener("change", () => {
        M.progress.toggleCheck(roleId, inp.getAttribute("data-check"), inp.checked);
        updateProgressUI();
      }));
    }

    M.simulator.render(qs("#simulator-root"));

    // Error practice buttons
    qsa("[data-error-id]").forEach(btn => btn.addEventListener("click", () => {
      const e = M.data.errors.find(x => x.id === btn.getAttribute("data-error-id"));
      if (!e) return;
      openDialog("#error-dialog");
      const body = qs("#error-dialog-body");
      if (body) body.innerHTML = `<h3>${e.title}</h3><p>${e.why}</p><p><strong>Corrección:</strong> ${e.fix}</p><p><strong>Quién:</strong> ${e.who}</p>`;
    }));
  }

  M.ui = { toast, initIndex, initRolePage, bindMarkers, openDialog };
})();
"""


def role_html(role: dict) -> str:
    screens = [s for s in SCREENS if s["id"] in role["screens"] or s["id"].startswith("regulatory")]
    screens = [s for s in SCREENS if s["id"] in role["screens"]]
    fields = [f for f in FIELDS if "*" in (f.get("roles") or []) or role["id"] in (f.get("roles") or [])]
    buttons = [b for b in BUTTONS if "*" in (b.get("roles") or []) or role["id"] in (b.get("roles") or [])]
    role_errors = [e for e in ERRORS if role["name"].split()[-1].lower() in e["who"].lower() or role["short"].lower() in e["who"].lower() or "Cualquier" in e["who"] or role["name"] in e["who"]]
    if not role_errors:
        role_errors = ERRORS[:4]

    markers = """
      <button type="button" class="marker" style="top:70px;left:40px" tabindex="0" data-name="Menú lateral RA" data-what="Navega entre Dashboard, Portafolio, Pipeline, Expedientes, CT/RS, etc." data-when="Al entrar a #/regulatory" data-role="{name}" data-result="Cambia la vista interna sin salir de #/regulatory" data-error="Si no ve una pestaña, faltan permisos" data-tip="Empiece por la vista de su cola de trabajo">1</button>
      <button type="button" class="marker" style="top:70px;right:80px" tabindex="0" data-name="Tenant / usuario" data-what="Muestra el contexto del tenant autenticado" data-when="Siempre tras login" data-role="{name}" data-result="Confirma que opera en el tenant correcto" data-error="Sesión expirada → #/login" data-tip="Verifique el tenant antes de crear datos">2</button>
      <button type="button" class="marker" style="top:140px;left:260px" tabindex="0" data-name="Área de trabajo" data-what="Tabla, kanban o detalle del expediente según la vista" data-when="Tras elegir una opción del menú RA" data-role="{name}" data-result="Ve casos y acciones permitidas a su rol" data-error="Botones ausentes = Sin permiso SoD/RBAC" data-tip="Haga clic en una fila para abrir el expediente">3</button>
    """.format(name=role["name"])

    return f"""<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>{role["name"]} · Manual Compliance 360</title>
  <link rel="stylesheet" href="../assets/css/manual.css" />
  <link rel="stylesheet" href="../assets/css/light-theme.css" />
  <link rel="stylesheet" href="../assets/css/dark-theme.css" />
</head>
<body data-theme="light">
  <a class="skip" href="#main">Saltar al contenido</a>
  <header class="app-header">
    <a class="brand-lockup" href="../index.html">
      <div class="brand-mark">C360</div>
      <div>
        <div class="brand-title">Compliance 360</div>
        <div class="brand-sub">Manual · {role["name"]}</div>
      </div>
    </a>
    <div class="header-actions">
      <span class="pill" style="--role-color:{role["color"]}">{role["short"]}</span>
      <span data-progress-role="{role["id"]}">0%</span>
      <button type="button" class="btn" data-theme-set="light">Claro</button>
      <button type="button" class="btn" data-theme-set="dark">Oscuro</button>
      <button type="button" class="btn" data-theme-set="system">Sistema</button>
      <a class="btn" href="../index.html">Inicio</a>
    </div>
  </header>
  <div class="layout">
    <nav class="sidebar" aria-label="Capítulos">
      <h3>Capítulos</h3>
      <a href="#who">1. ¿Quién soy?</a>
      <a href="#responsibility">2. Responsabilidad</a>
      <a href="#can">3. Qué puedo hacer</a>
      <a href="#cannot">4. Qué no puedo</a>
      <a href="#handoff">5–6. Recibo / Entrego</a>
      <a href="#daily">7. Flujo diario</a>
      <a href="#screens">8. Mis pantallas</a>
      <a href="#tasks">9. Tareas frecuentes</a>
      <a href="#common-errors">10. Errores comunes</a>
      <a href="#sim-ui">Pantalla simulada</a>
      <a href="#tutorials">Tutoriales</a>
      <a href="#simulator-root">Simulador</a>
      <a href="#role-checklist">Checklist</a>
      <h3>Acceso real</h3>
      <p class="muted" style="font-size:.8rem">App: http://localhost:5272<br/>Login: #/login<br/>RA: #/regulatory</p>
    </nav>
    <main id="main" class="main">
      <section id="who" class="card">
        <div class="pill" style="--role-color:{role["color"]}">{role["short"]}</div>
        <h1>{role["name"]}</h1>
        <p class="lead">{role["purpose"]}</p>
        <p class="muted">Progreso de este rol: <strong data-progress-role="{role["id"]}">0%</strong></p>
        <div class="progress-ring"><span></span></div>
      </section>

      <h2 class="section-title" id="responsibility">¿Cuál es mi responsabilidad?</h2>
      <div class="card"><p>Usted participa en: <strong>{role["flowStage"]}</strong>.</p>
      <div class="flow" id="role-flow"></div></div>

      <h2 class="section-title" id="can">¿Qué puedo hacer?</h2>
      <div class="card"><ul class="list-tight">{"".join(f"<li>{x}</li>" for x in role["can"])}</ul></div>

      <h2 class="section-title" id="cannot">¿Qué no puedo hacer?</h2>
      <div class="card"><ul class="list-tight">{"".join(f"<li>{x}</li>" for x in role["cannot"])}</ul>
      <p class="muted">Si un botón no aparece, normalmente falta el permiso o SoD lo bloquea. No es un fallo de la pantalla.</p></div>

      <h2 class="section-title" id="handoff">¿Qué recibo y qué entrego?</h2>
      <div class="card">
        <p><strong>Recibo:</strong> {role["receives"]}</p>
        <p><strong>Entrego:</strong> {role["delivers"]}</p>
      </div>

      <h2 class="section-title" id="daily">Mi flujo de trabajo diario</h2>
      <div class="card">
        <ol>
          <li>Inicie sesión en <code>http://localhost:5272</code> → <code>#/login</code>.</li>
          <li>Abra <strong>Regulatory Management</strong> → <code>#/regulatory</code>.</li>
          <li>Vaya a la vista de su cola (Pipeline / Expedientes / Configuración según rol).</li>
          <li>Ejecute solo las acciones de su etapa.</li>
          <li>Confirme el estado en español (ej. «Aprobado internamente para sometimiento» ≠ «Aprobación registrada de MINSA/CSS (externa)»).</li>
        </ol>
      </div>

      <h2 class="section-title" id="screens">Mis pantallas</h2>
      <div class="role-grid">
        {"".join(f'<article class="card"><h3>{s["name"]}</h3><p class="muted"><code>{s["route"]}</code></p><p>{s["objective"]}</p><p><strong>Cómo llegar:</strong> Menú lateral → {s["route"]}</p></article>' for s in screens)}
      </div>

      <h2 class="section-title" id="tasks">Tareas frecuentes y campos</h2>
      <div class="card">
        <h3>Campos que usará</h3>
        <table class="table"><thead><tr><th>Campo</th><th>Obligatorio</th><th>Qué escribir</th><th>Ejemplo</th></tr></thead>
        <tbody>{"".join(f"<tr><td>{f['label']}</td><td>{'Sí' if f['required'] else 'No'}</td><td>{f['purpose']}</td><td><code>{f.get('example','')}</code></td></tr>" for f in fields) or "<tr><td colspan=4>Principalmente consulta; sin formularios de escritura en este rol.</td></tr>"}</tbody></table>
        <h3>Botones de su rol</h3>
        <table class="table"><thead><tr><th>Botón</th><th>Resultado</th><th>Permiso</th></tr></thead>
        <tbody>{"".join(f"<tr><td>{b['label']}</td><td>{b['result']}</td><td>{b.get('perm','—')}</td></tr>" for b in buttons)}</tbody></table>
      </div>

      <h2 class="section-title" id="common-errors">Errores comunes</h2>
      <div class="card">
        {"".join(f'<p><button type="button" class="btn" data-error-id="{e["id"]}">Simular: {e["title"]}</button></p>' for e in role_errors)}
      </div>

      <h2 class="section-title" id="sim-ui">Pantalla recreada (interactiva)</h2>
      <div class="card">
        <p>Réplica visual de la Consola de Asuntos Regulatorios. Haga clic en los marcadores numerados.</p>
        <div class="sim-shell" style="position:relative">
          <aside class="sim-nav">
            <div style="font-weight:700;margin-bottom:.5rem">C360 · RA</div>
            <button type="button" class="active">Expedientes</button>
            <button type="button">Dashboard</button>
            <button type="button">Pipeline</button>
            <button type="button">Portafolio</button>
            <button type="button">Registros CT/RS</button>
          </aside>
          <div class="sim-body" style="position:relative;min-height:320px">
            <div class="sim-topbar">
              <div><strong>Consola de Asuntos Regulatorios</strong><div class="muted">Rol activo: {role["name"]}</div></div>
              <span class="badge">Lab tenant</span>
            </div>
            <table class="table">
              <thead><tr><th>Caso</th><th>Estado</th><th>Proceso</th></tr></thead>
              <tbody>
                <tr><td>DEMO-001</td><td>Planificación / Preparación</td><td>Registro inicial</td></tr>
                <tr><td>DEMO-002</td><td>Aprobado internamente para sometimiento</td><td>Registro inicial</td></tr>
              </tbody>
            </table>
            {markers}
          </div>
        </div>
        <div class="panel" id="marker-panel" style="margin-top:1rem">
          <p class="muted">Seleccione un marcador (1–3) para ver la explicación.</p>
        </div>
      </div>

      <h2 class="section-title">Tutoriales paso a paso</h2>
      <div id="tutorials"></div>

      <h2 class="section-title">Simulación guiada</h2>
      <div id="simulator-root"></div>

      <h2 class="section-title">Checklist de finalización</h2>
      <div class="card checklist" id="role-checklist"></div>

      <p class="footer-note">Fuente: RoleCatalog.cs, regulatory-affairs.js, docs/regulatory-affairs/security (04, 17, 21, 24). No documenta el modelo anterior.</p>
    </main>
  </div>

  <div class="dialog-backdrop" id="error-dialog" role="dialog" aria-modal="true">
    <div class="dialog">
      <div id="error-dialog-body"></div>
      <p style="margin-top:1rem"><button type="button" class="btn primary" data-close>Cerrar</button></p>
    </div>
  </div>

  <script src="../assets/js/data.js"></script>
  <script src="../assets/js/progress.js"></script>
  <script src="../assets/js/search.js"></script>
  <script src="../assets/js/simulator.js"></script>
  <script src="../assets/js/manual.js"></script>
  <!--ROLE_BOOTSTRAP-->
</body>
</html>
"""


INDEX_HTML = """<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Compliance 360 · Manual Interactivo por Rol</title>
  <link rel="stylesheet" href="assets/css/manual.css" />
  <link rel="stylesheet" href="assets/css/light-theme.css" />
  <link rel="stylesheet" href="assets/css/dark-theme.css" />
</head>
<body data-theme="light">
  <a class="skip" href="#main">Saltar al contenido</a>
  <header class="app-header">
    <a class="brand-lockup" href="index.html">
      <div class="brand-mark">C360</div>
      <div>
        <div class="brand-title">Compliance 360</div>
        <div class="brand-sub">Regulatory Affairs · Manual Interactivo por Rol</div>
      </div>
    </a>
    <div class="header-actions">
      <form class="search-box" role="search" onsubmit="return false;">
        <label class="skip" for="global-search">Buscar</label>
        <input id="global-search" type="search" placeholder="¿Cómo creo un expediente? ¿Qué es CT/RS?" autocomplete="off" />
      </form>
      <button type="button" class="btn" data-theme-set="light" aria-label="Tema claro">Claro</button>
      <button type="button" class="btn" data-theme-set="dark" aria-label="Tema oscuro">Oscuro</button>
      <button type="button" class="btn" data-theme-set="system">Sistema</button>
      <button type="button" class="btn" id="btn-reset-learning">Reiniciar aprendizaje</button>
    </div>
  </header>

  <main id="main" class="main">
    <section class="hero">
      <div class="hero-card">
        <div class="pill" style="--role-color:#0ea5e9">Capacitación premium · Sin backend</div>
        <h1>Manual Interactivo por Rol</h1>
        <p class="lead">Aprende paso a paso a gestionar productos, expedientes, revisiones, sometimientos y CT/RS. Diseñado para personas que nunca han usado Compliance 360 ni Regulatory Affairs.</p>
        <p class="muted">App real: <code>http://localhost:5272</code> · Login <code>#/login</code> · Consola RA <code>#/regulatory</code></p>
        <div style="display:flex;gap:.6rem;flex-wrap:wrap;margin-top:1rem">
          <button type="button" class="btn primary" id="btn-start">Comenzar</button>
          <a class="btn" href="#simulator-root">Abrir simulador de expediente</a>
          <a class="btn" href="#errors">Aprender con errores</a>
        </div>
        <div style="margin-top:1.25rem">
          <div class="muted">Progreso general: <strong data-progress-overall>0%</strong></div>
          <div class="progress-ring"><span></span></div>
          <p class="muted">Último capítulo: <span id="last-chapter">Ninguno aún</span></p>
        </div>
      </div>
      <div class="hero-card">
        <h2 style="margin-top:0">Flujo certificado</h2>
        <p class="muted">Preparación → Revisión técnica → Aprobación interna para sometimiento → Sometimiento → Revisión de autoridad → Observación → Respuesta → Resometimiento → Decisión externa → CT/RS → Vigencia → Renovación</p>
        <div class="flow" id="flow-strip"></div>
        <p><strong>Nunca confunda:</strong></p>
        <ul class="list-tight">
          <li><em>Aprobado internamente para sometimiento</em> = autorización interna (Approver).</li>
          <li><em>Sometimiento registrado</em> = envío a la autoridad (Submitter).</li>
          <li><em>Aprobación registrada de MINSA/CSS (externa)</em> + <em>CT/RS activo</em> = decisión de autoridad (Manager/QM).</li>
        </ul>
      </div>
    </section>

    <div id="search-results" aria-live="polite"></div>

    <h2 class="section-title" id="roles">¿Cuál es tu rol?</h2>
    <p class="muted">Elija la tarjeta de su rol. Cada manual explica exactamente qué botones verá y cuáles no.</p>
    <div class="role-grid" id="role-grid"></div>

    <h2 class="section-title">Simulación guiada del expediente</h2>
    <div id="simulator-root"></div>

    <h2 class="section-title" id="errors">Aprender con errores</h2>
    <div id="error-list"></div>

    <h2 class="section-title" id="glossary">Glosario</h2>
    <div id="glossary-list"></div>

    <h2 class="section-title" id="screens">Inventario de pantallas</h2>
    <div class="card" style="overflow:auto"><table class="table" id="screens-table"></table></div>

    <h2 class="section-title" id="fields">Diccionario de campos</h2>
    <div class="card" style="overflow:auto"><table class="table" id="fields-table"></table></div>

    <h2 class="section-title" id="buttons">Diccionario de botones</h2>
    <div class="card" style="overflow:auto"><table class="table" id="buttons-table"></table></div>

    <p class="footer-note">Manual autocontenido (file://). Fuentes: RoleCatalog / RbacCatalog / regulatory-affairs.js / docs/regulatory-affairs/security. REGUTRACK se reemplaza vía Importación Stage XLSX + consola RA.</p>
  </main>

  <script src="assets/js/data.js"></script>
  <script src="assets/js/progress.js"></script>
  <script src="assets/js/search.js"></script>
  <script src="assets/js/simulator.js"></script>
  <script src="assets/js/manual.js"></script>
  <script>
    document.addEventListener("DOMContentLoaded", () => {
      window.C360_MANUAL.ui.initIndex();
    });
  </script>
</body>
</html>
"""


def md_traceability() -> str:
    rows = []
    for r in ROLES:
        for s_id in r["screens"]:
            s = next((x for x in SCREENS if x["id"] == s_id), None)
            if not s:
                continue
            rows.append(f"| {r['name']} | {s['name']} | `{s['route']}` | `wwwroot/regulatory-affairs.js` / `app.js` | (ver RoleCatalog) | navigate | {(r['tutorials'] or ['—'])[0]} | security/21 |")
    return """# TRACEABILITY_MATRIX

| Role | Manual Screen | System Route | Source File | Permission | Action | Tutorial | Evidence |
|------|---------------|--------------|-------------|------------|--------|----------|----------|
""" + "\n".join(rows) + "\n"


def md_fields() -> str:
    lines = ["# FIELD_DICTIONARY", "", "| Screen | Field | Type | Required | Purpose | Validation | Example | Role |", "|--------|-------|------|----------|---------|------------|---------|------|"]
    for f in FIELDS:
        roles = ", ".join(f.get("roles") or [])
        lines.append(f"| {f['screen']} | {f['label']} | {f['type']} | {f['required']} | {f['purpose']} | invalid: {f.get('invalid','')} | {f.get('example','')} | {roles} |")
    return "\n".join(lines) + "\n"


def md_buttons() -> str:
    lines = ["# BUTTON_DICTIONARY", "", "| Screen | Button | Action | Role | Permission | Precondition | Result |", "|--------|--------|--------|------|------------|--------------|--------|"]
    for b in BUTTONS:
        roles = ", ".join(b.get("roles") or [])
        lines.append(f"| {b['screen']} | {b['label']} | {b['action']} | {roles} | {b.get('perm','—')} | {b.get('pre','—')} | {b['result']} |")
    return "\n".join(lines) + "\n"


def md_screens() -> str:
    lines = ["# SCREEN_INVENTORY", "", "| Name | Route | Module | Objective |", "|------|-------|--------|-----------|"]
    for s in SCREENS:
        lines.append(f"| {s['name']} | `{s['route']}` | {s['module']} | {s['objective']} |")
    return "\n".join(lines) + "\n"


def md_roles() -> str:
    lines = ["# ROLE_FUNCTION_MATRIX", "", "| Role | Purpose | Can | Cannot | Flow stage |", "|------|---------|-----|--------|------------|"]
    for r in ROLES:
        lines.append(f"| {r['name']} | {r['purpose']} | {'; '.join(r['can'][:2])}… | {'; '.join(r['cannot'][:2])}… | {r['flowStage']} |")
    return "\n".join(lines) + "\n"


def md_workflow() -> str:
    lines = ["# WORKFLOW_GUIDE", "", WORKFLOW["title"], "", "```", "Preparación → Revisión técnica → Aprobación interna para sometimiento → Sometimiento → Revisión de autoridad → Observación → Respuesta → Resometimiento → Decisión externa → CT/RS → Vigencia → Renovación", "```", ""]
    for s in WORKFLOW["steps"]:
        lines.append(f"## {s['label']}")
        lines.append(f"- Estado: `{s['status']}`")
        lines.append(f"- Rol: {s['role']}")
        lines.append(f"- UI: {', '.join(s['ui']) if s['ui'] else '(seguimiento / transición)'}")
        lines.append("")
    lines.append("## Etiquetas UI (regulatory-affairs.js STATUS_LABELS)")
    for k, v in WORKFLOW["statusLabels"].items():
        lines.append(f"- `{k}` → {v}")
    return "\n".join(lines) + "\n"


README = """# Manual Interactivo por Rol — Compliance 360 Regulatory Affairs

## Cómo abrir

1. Abra `docs/user-manual/index.html` con doble clic (protocolo `file://`).
2. Elija tema Claro / Oscuro / Sistema.
3. Seleccione su rol y siga los capítulos + tutoriales.
4. Practique en el **Simulador guiado** (no requiere API).

## App real (laboratorio)

- URL: `http://localhost:5272`
- Login: `#/login`
- Consola RA: `#/regulatory`

## Contenido

- 9 roles certificados (TAC + 8 RA/QM)
- Pantallas, campos y botones alineados a `regulatory-affairs.js` y `RoleCatalog`
- Flujo certificado Prep → … → CT/RS → Renovación
- Buscador, glosario, errores SoD, progreso en `localStorage`

## No inventa

No documenta pantallas, permisos ni flujos fuera del modelo certificado actual.
"""


def main() -> None:
    (ROOT / "assets/css").mkdir(parents=True, exist_ok=True)
    (ROOT / "assets/js").mkdir(parents=True, exist_ok=True)
    (ROOT / "assets/icons").mkdir(parents=True, exist_ok=True)
    (ROOT / "assets/images").mkdir(parents=True, exist_ok=True)
    (ROOT / "roles").mkdir(parents=True, exist_ok=True)
    (ROOT / "data").mkdir(parents=True, exist_ok=True)
    (ROOT / "docs").mkdir(parents=True, exist_ok=True)
    (ROOT / "e2e").mkdir(parents=True, exist_ok=True)

    write(ROOT / "assets/css/manual.css", MANUAL_CSS)
    write(ROOT / "assets/css/light-theme.css", LIGHT_THEME)
    write(ROOT / "assets/css/dark-theme.css", DARK_THEME)
    write(ROOT / "assets/js/data.js", build_data_js())
    write(ROOT / "assets/js/progress.js", PROGRESS_JS)
    write(ROOT / "assets/js/search.js", SEARCH_JS)
    write(ROOT / "assets/js/simulator.js", SIMULATOR_JS)
    write(ROOT / "assets/js/manual.js", MANUAL_JS)
    write(ROOT / "index.html", INDEX_HTML)
    write(ROOT / "README.md", README)

    dump_json("roles.json", ROLES)
    dump_json("screens.json", SCREENS)
    dump_json("fields.json", FIELDS)
    dump_json("buttons.json", BUTTONS)
    dump_json("workflows.json", WORKFLOW)

    for role in ROLES:
        boot = f"""  <script>
    document.addEventListener("DOMContentLoaded", function () {{
      var flow = document.getElementById("role-flow");
      var roleName = {json.dumps(role["name"])};
      if (flow && window.C360_MANUAL && window.C360_MANUAL.data && window.C360_MANUAL.data.workflow) {{
        var steps = window.C360_MANUAL.data.workflow.steps;
        flow.innerHTML = steps.map(function (s, i) {{
          var cur = s.role.indexOf(roleName) >= 0;
          return '<span class="step' + (cur ? ' current' : '') + '">' + s.label + '</span>' +
            (i < steps.length - 1 ? '<span class="arrow">→</span>' : '');
        }}).join("");
      }}
      window.C360_MANUAL.ui.initRolePage({json.dumps(role["id"])});
    }});
  </script>"""
        html = role_html(role).replace("<!--ROLE_BOOTSTRAP-->", boot)
        write(ROOT / "roles" / role["file"], html)

    write(ROOT / "docs/TRACEABILITY_MATRIX.md", md_traceability())
    write(ROOT / "docs/FIELD_DICTIONARY.md", md_fields())
    write(ROOT / "docs/BUTTON_DICTIONARY.md", md_buttons())
    write(ROOT / "docs/SCREEN_INVENTORY.md", md_screens())
    write(ROOT / "docs/ROLE_FUNCTION_MATRIX.md", md_roles())
    write(ROOT / "docs/WORKFLOW_GUIDE.md", md_workflow())

    # placeholder icon
    write(ROOT / "assets/icons/README.md", "Iconos SVG inline en el HTML/CSS del manual. Marca C360 en header.\n")

    print(f"Generated manual at {ROOT}")
    print(f"Roles: {len(ROLES)} | Screens: {len(SCREENS)} | Fields: {len(FIELDS)} | Buttons: {len(BUTTONS)}")


if __name__ == "__main__":
    main()
