/* Auto-generated — facts from RoleCatalog, regulatory-affairs.js, security docs */
window.C360_MANUAL = window.C360_MANUAL || {};
window.C360_MANUAL.data = {
  "roles": [
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
        "Ver Audit Trail (AUDIT.READ / TENANT.AUDIT)"
      ],
      "cannot": [
        "Crear expedientes (sin REGULATORY.DOSSIER.CREATE)",
        "Aprobar internamente para sometimiento",
        "Registrar sometimiento",
        "Registrar aprobación externa MINSA/CSS + CT/RS",
        "Sustituir al Specialist, Reviewer, Approver o Submitter en el flujo operativo"
      ],
      "flowStage": "IAM · configuración · sin operación de expediente",
      "receives": "Solicitudes de acceso y asignación de roles del equipo regulatorio.",
      "delivers": "Usuarios con el rol correcto y SoD habilitado para que el flujo Prep→Review→Approve→Submit funcione.",
      "screens": [
        "login",
        "tenant-administration",
        "security",
        "regulatory-config",
        "regulatory-sod",
        "regulatory-dashboard",
        "audit-trail"
      ],
      "tutorials": [
        "tac-login",
        "tac-users",
        "tac-roles",
        "tac-configure-ra",
        "tac-sod",
        "tac-no-operate",
        "tac-logout"
      ],
      "file": "tenant-administrator.html"
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
        "Ver SoD Settings y lecturas del módulo"
      ],
      "cannot": [
        "Aprobar internamente (APPROVE_FOR_SUBMISSION)",
        "Registrar sometimiento (SUBMIT)",
        "Registrar decisión externa / CT/RS (APPROVE)",
        "Operar el expediente como Specialist por defecto"
      ],
      "flowStage": "Configuración previa al flujo del expediente",
      "receives": "Archivo REGUTRACK y reglas SoD del Tenant Administrator.",
      "delivers": "Catálogos, autoridades, packs publicados e importaciones listas para el equipo operativo.",
      "screens": [
        "login",
        "regulatory-config",
        "regulatory-import",
        "regulatory-sod",
        "regulatory-manufacturers",
        "regulatory-licenses",
        "regulatory-alerts",
        "regulatory-dashboard"
      ],
      "tutorials": [
        "adm-bootstrap",
        "adm-authority",
        "adm-import",
        "adm-alerts",
        "adm-sod-view"
      ],
      "file": "regulatory-administrator.html"
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
        "SoD manage y emergency override auditado"
      ],
      "cannot": [
        "Sustituir Specialist + Approver + Submitter a la vez sin override",
        "Representar que la aprobación interna es la de MINSA/CSS"
      ],
      "flowStage": "Observación → Respuesta → Decisión externa → CT/RS → Renovación",
      "receives": "Expedientes sometidos por el Submitter; respuestas del Specialist a observaciones.",
      "delivers": "CT/RS activo y renovaciones iniciadas; excepciones SoD auditadas.",
      "screens": [
        "login",
        "regulatory-dashboard",
        "regulatory-pipeline",
        "regulatory-dossiers",
        "regulatory-registrations",
        "regulatory-alerts",
        "regulatory-sod"
      ],
      "tutorials": [
        "mgr-dashboard",
        "mgr-observe",
        "mgr-external-approve",
        "mgr-renewal",
        "mgr-breakglass"
      ],
      "file": "regulatory-manager.html"
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
        "Ver CT/RS y dashboard en lectura"
      ],
      "cannot": [
        "Revisar su propio expediente (PreventSelfReview / SoD)",
        "Aprobar internamente para sometimiento",
        "Registrar sometimiento",
        "Registrar aprobación externa / crear CT/RS"
      ],
      "flowStage": "Preparación (Planning → ReadyForSubmission) y respuesta a observaciones",
      "receives": "Packs/autoridades del Administrator; devoluciones del Reviewer; observaciones del Manager.",
      "delivers": "Expediente técnicamente completo listo para el Reviewer.",
      "screens": [
        "login",
        "regulatory-portfolio",
        "regulatory-dossiers",
        "regulatory-manufacturers",
        "regulatory-pipeline",
        "regulatory-dashboard"
      ],
      "tutorials": [
        "spec-product",
        "spec-mfr",
        "spec-dossier",
        "spec-checklist",
        "spec-ready",
        "spec-respond-obs",
        "spec-fix-return"
      ],
      "file": "regulatory-specialist.html"
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
        "Consultar dashboard y CT/RS en lectura"
      ],
      "cannot": [
        "Crear productos/expedientes",
        "Aprobar internamente para sometimiento",
        "Registrar sometimiento",
        "Preparar el mismo caso si PreventSelfReview está activo"
      ],
      "flowStage": "Revisión técnica (antes de ReadyForSubmission / hacia Approver)",
      "receives": "Expedientes armados por el Specialist.",
      "delivers": "Requisitos aceptados; expediente listo para Declarar técnicamente completo / Approver.",
      "screens": [
        "login",
        "regulatory-dossiers",
        "regulatory-pipeline",
        "regulatory-dashboard"
      ],
      "tutorials": [
        "rev-queue",
        "rev-accept",
        "rev-reject",
        "rev-return"
      ],
      "file": "regulatory-reviewer.html"
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
        "Consultar lecturas RA"
      ],
      "cannot": [
        "Preparar el expediente",
        "Registrar sometimiento (SeparateApproverAndSubmitter)",
        "Registrar aprobación externa / CT/RS"
      ],
      "flowStage": "Aprobación interna (ReadyForSubmission → ApprovedForSubmission)",
      "receives": "Expedientes declarados técnicamente completos.",
      "delivers": "Expediente con estado «Aprobado internamente para sometimiento» para el Submitter.",
      "screens": [
        "login",
        "regulatory-dossiers",
        "regulatory-pipeline",
        "regulatory-dashboard"
      ],
      "tutorials": [
        "appr-queue",
        "appr-approve",
        "appr-why-no-submit"
      ],
      "file": "regulatory-approver.html"
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
        "Consultar lecturas RA"
      ],
      "cannot": [
        "Aprobar internamente",
        "Registrar decisión externa / CT/RS",
        "Preparar o revisar requisitos"
      ],
      "flowStage": "Sometimiento (ApprovedForSubmission → Submitted)",
      "receives": "Expedientes con aprobación interna del Approver.",
      "delivers": "Expediente «Sometido ante autoridad» para seguimiento del Manager.",
      "screens": [
        "login",
        "regulatory-dossiers",
        "regulatory-pipeline",
        "regulatory-dashboard"
      ],
      "tutorials": [
        "sub-queue",
        "sub-submit",
        "sub-why-no-approve"
      ],
      "file": "regulatory-submitter.html"
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
        "Consultar sin modificar"
      ],
      "cannot": [
        "Cualquier botón de mutación (Nuevo, Aprobar, Someter, Stage, Bootstrap, etc.)"
      ],
      "flowStage": "Consulta transversal (sin etapa de escritura)",
      "receives": "Visibilidad del estado del flujo después de cada rol operativo.",
      "delivers": "Información; no entrega cambios al siguiente rol.",
      "screens": [
        "login",
        "regulatory-dashboard",
        "regulatory-portfolio",
        "regulatory-pipeline",
        "regulatory-dossiers",
        "regulatory-registrations",
        "regulatory-alerts"
      ],
      "tutorials": [
        "view-read",
        "view-no-edit"
      ],
      "file": "regulatory-viewer.html"
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
        "Ver Audit Trail"
      ],
      "cannot": [
        "Preparar expedientes (sin CREATE/UPDATE de preparación por defecto)",
        "Sustituir al Regulatory Manager en supervisión diaria salvo su permiso de APPROVE"
      ],
      "flowStage": "Decisión externa / CT/RS (junto o en apoyo al Manager)",
      "receives": "Expedientes sometidos listos para decisión de autoridad.",
      "delivers": "Registro de aprobación externa y CT/RS; trazabilidad QMS.",
      "screens": [
        "login",
        "regulatory-dossiers",
        "regulatory-registrations",
        "regulatory-dashboard",
        "audit-trail"
      ],
      "tutorials": [
        "qm-external",
        "qm-limits",
        "qm-audit"
      ],
      "file": "quality-manager.html"
    }
  ],
  "screens": [
    {
      "id": "login",
      "name": "Inicio de sesión",
      "route": "#/login",
      "module": "Auth",
      "objective": "Identificar usuario y obtener token JWT para el tenant."
    },
    {
      "id": "tenant-administration",
      "name": "Tenant Administration",
      "route": "#/tenant-administration",
      "module": "Enterprise",
      "objective": "Gestionar usuarios, roles y perfil del tenant."
    },
    {
      "id": "security",
      "name": "Security",
      "route": "#/security",
      "module": "Enterprise",
      "objective": "Configuración de seguridad del tenant (MFA/SSO según permisos)."
    },
    {
      "id": "audit-trail",
      "name": "Audit Trail",
      "route": "#/audit-trail",
      "module": "Command Center",
      "objective": "Consultar eventos de auditoría."
    },
    {
      "id": "regulatory-shell",
      "name": "Consola de Asuntos Regulatorios",
      "route": "#/regulatory",
      "module": "Regulatory Affairs",
      "objective": "Contenedor de todas las vistas RA."
    },
    {
      "id": "regulatory-dashboard",
      "name": "Dashboard RA",
      "route": "#/regulatory → Dashboard",
      "module": "Regulatory Affairs",
      "objective": "Métricas de productos, CT activos, en trámite, requisitos críticos, detenidos >14d, por vencer."
    },
    {
      "id": "regulatory-portfolio",
      "name": "Portafolio",
      "route": "#/regulatory → Portafolio",
      "module": "Regulatory Affairs",
      "objective": "Listar productos y crear producto + expediente."
    },
    {
      "id": "regulatory-pipeline",
      "name": "Pipeline",
      "route": "#/regulatory → Pipeline",
      "module": "Regulatory Affairs",
      "objective": "Kanban por estado del expediente + Vencido + Renovación."
    },
    {
      "id": "regulatory-dossiers",
      "name": "Expedientes",
      "route": "#/regulatory → Expedientes",
      "module": "Regulatory Affairs",
      "objective": "Listar y abrir el workspace del expediente."
    },
    {
      "id": "regulatory-dossier-detail",
      "name": "Workspace del expediente",
      "route": "#/regulatory → Expedientes → detalle",
      "module": "Regulatory Affairs",
      "objective": "Ejecutar prep, revisión, aprobación interna, sometimiento, observación y aprobación externa."
    },
    {
      "id": "regulatory-registrations",
      "name": "Registros CT/RS",
      "route": "#/regulatory → Registros CT/RS",
      "module": "Regulatory Affairs",
      "objective": "Consultar números CT/RS, emisión y vencimiento."
    },
    {
      "id": "regulatory-manufacturers",
      "name": "Fabricantes",
      "route": "#/regulatory → Fabricantes",
      "module": "Regulatory Affairs",
      "objective": "Alta de fabricante y certificados."
    },
    {
      "id": "regulatory-licenses",
      "name": "Licencias",
      "route": "#/regulatory → Licencias",
      "module": "Regulatory Affairs",
      "objective": "Registrar licencias operativas."
    },
    {
      "id": "regulatory-alerts",
      "name": "Alertas",
      "route": "#/regulatory → Alertas",
      "module": "Regulatory Affairs",
      "objective": "Evaluar alertas de vencimiento / riesgo operativo."
    },
    {
      "id": "regulatory-import",
      "name": "Importación",
      "route": "#/regulatory → Importación",
      "module": "Regulatory Affairs",
      "objective": "Stage XLSX del libro REGUTRACK."
    },
    {
      "id": "regulatory-config",
      "name": "Configuración",
      "route": "#/regulatory → Configuración",
      "module": "Regulatory Affairs",
      "objective": "Bootstrap regulatorio (autoridades + pack)."
    },
    {
      "id": "regulatory-sod",
      "name": "SoD Settings",
      "route": "#/regulatory → SoD Settings",
      "module": "Regulatory Affairs",
      "objective": "Consultar política de segregación de funciones."
    }
  ],
  "fields": [
    {
      "id": "email",
      "screen": "login",
      "label": "Correo",
      "tech": "email",
      "type": "email",
      "required": true,
      "purpose": "Identifica al usuario en el tenant.",
      "example": "ra.spec@cert.local",
      "invalid": "texto sin @",
      "format": "email",
      "max": 256,
      "roles": [
        "*"
      ]
    },
    {
      "id": "password",
      "screen": "login",
      "label": "Contraseña",
      "tech": "password",
      "type": "password",
      "required": true,
      "purpose": "Autenticación.",
      "example": "(la asignada por TAC)",
      "invalid": "vacía",
      "format": "secreto",
      "roles": [
        "*"
      ]
    },
    {
      "id": "brand",
      "screen": "regulatory-portfolio",
      "label": "Marca",
      "tech": "brand",
      "type": "text",
      "required": true,
      "purpose": "Marca comercial del dispositivo.",
      "example": "DEMO",
      "invalid": "(vacío)",
      "max": 120,
      "roles": [
        "regulatory-specialist",
        "regulatory-administrator"
      ]
    },
    {
      "id": "regulatoryName",
      "screen": "regulatory-portfolio",
      "label": "Nombre regulatorio",
      "tech": "regulatoryName",
      "type": "text",
      "required": true,
      "purpose": "Nombre oficial del producto para expediente y CT/RS. Puede diferir del comercial.",
      "example": "PRODUCTO DEMO",
      "invalid": "(vacío)",
      "max": 320,
      "roles": [
        "regulatory-specialist",
        "regulatory-administrator"
      ]
    },
    {
      "id": "catalogCode",
      "screen": "regulatory-portfolio",
      "label": "Código catálogo",
      "tech": "catalogCode",
      "type": "code",
      "required": true,
      "purpose": "Código único por tenant. Duplicado genera error.",
      "example": "CAT-123456",
      "invalid": "código ya existente",
      "max": 120,
      "roles": [
        "regulatory-specialist",
        "regulatory-administrator"
      ]
    },
    {
      "id": "riskClass",
      "screen": "regulatory-portfolio",
      "label": "Clase de riesgo",
      "tech": "riskClass",
      "type": "select",
      "required": true,
      "purpose": "Clase A, B o C del dispositivo. No es el módulo Risk Management. Influye en Requirement Pack.",
      "example": "A",
      "invalid": "valor fuera de A/B/C",
      "allowed": [
        "A",
        "B",
        "C"
      ],
      "roles": [
        "regulatory-specialist",
        "regulatory-administrator"
      ]
    },
    {
      "id": "countryCode",
      "screen": "regulatory-portfolio",
      "label": "País",
      "tech": "countryCode",
      "type": "text",
      "required": true,
      "purpose": "Código de país del producto (UI demo usa PA).",
      "example": "PA",
      "invalid": "(vacío)",
      "roles": [
        "regulatory-specialist"
      ]
    },
    {
      "id": "mfrLegalName",
      "screen": "regulatory-manufacturers",
      "label": "Nombre legal fabricante",
      "tech": "legalName",
      "type": "text",
      "required": true,
      "purpose": "Razón social del fabricante.",
      "example": "Acme Medical Co.",
      "invalid": "(vacío)",
      "max": 220,
      "roles": [
        "regulatory-specialist",
        "regulatory-administrator"
      ]
    },
    {
      "id": "mfrCountry",
      "screen": "regulatory-manufacturers",
      "label": "País",
      "tech": "countryCode",
      "type": "text",
      "required": true,
      "purpose": "País del fabricante.",
      "example": "CN",
      "invalid": "(vacío)",
      "roles": [
        "regulatory-specialist",
        "regulatory-administrator"
      ]
    },
    {
      "id": "obsDescription",
      "screen": "regulatory-dossier-detail",
      "label": "Descripción de la observación",
      "tech": "description",
      "type": "textarea",
      "required": true,
      "purpose": "Texto de la observación emitida por la autoridad.",
      "example": "Falta actualización de literatura técnica",
      "invalid": "(vacío)",
      "max": 4000,
      "roles": [
        "regulatory-manager"
      ]
    },
    {
      "id": "ctrsNumber",
      "screen": "regulatory-dossier-detail",
      "label": "Número CT/RS",
      "tech": "registrationNumber",
      "type": "text",
      "required": true,
      "purpose": "Número emitido por la autoridad. No lo genera Compliance 360.",
      "example": "MQ-4521-07-26",
      "invalid": "(vacío)",
      "max": 120,
      "roles": [
        "regulatory-manager",
        "quality-manager"
      ]
    },
    {
      "id": "ctrsExpires",
      "screen": "regulatory-dossier-detail",
      "label": "Vencimiento ISO",
      "tech": "expiresOn",
      "type": "date",
      "required": true,
      "purpose": "Fecha de vencimiento del CT/RS (YYYY-MM-DD).",
      "example": "2029-07-13",
      "invalid": "2029/07/13 o fecha anterior a emisión",
      "format": "YYYY-MM-DD",
      "roles": [
        "regulatory-manager",
        "quality-manager"
      ]
    },
    {
      "id": "licenseCompany",
      "screen": "regulatory-licenses",
      "label": "Compañía",
      "tech": "companyName",
      "type": "text",
      "required": true,
      "purpose": "Nombre de la empresa titular de la licencia operativa.",
      "example": "Irving Corro S.A",
      "invalid": "(vacío)",
      "roles": [
        "regulatory-administrator"
      ]
    },
    {
      "id": "licenseType",
      "screen": "regulatory-licenses",
      "label": "Tipo de licencia",
      "tech": "licenseType",
      "type": "text",
      "required": true,
      "purpose": "Tipo/descripción de la licencia operativa.",
      "example": "Distribución de dispositivos médicos",
      "invalid": "(vacío)",
      "roles": [
        "regulatory-administrator"
      ]
    },
    {
      "id": "opportunityAmount",
      "screen": "regulatory-dashboard",
      "label": "Opportunity Amount",
      "tech": "opportunityAmount",
      "type": "currency",
      "required": false,
      "purpose": "Valor comercial asociado al producto/expediente. No cambia la decisión de autoridad; alimenta priorización y dashboard.",
      "example": "15000.00",
      "invalid": "texto no numérico",
      "roles": [
        "regulatory-specialist",
        "regulatory-manager"
      ]
    },
    {
      "id": "maxReception",
      "screen": "regulatory-dossier-detail",
      "label": "Fecha máxima de recepción",
      "tech": "maximumReceptionOn",
      "type": "datetime",
      "required": false,
      "purpose": "Límite para recibir documentos del fabricante. Puede generar alertas si se vence en estado Espera docs fábrica.",
      "example": "2026-08-01T00:00:00Z",
      "invalid": "fecha mal formada",
      "roles": [
        "regulatory-specialist"
      ]
    }
  ],
  "buttons": [
    {
      "id": "login-submit",
      "screen": "login",
      "label": "Iniciar sesion / Completar login seguro",
      "action": "POST /api/v1/auth/login",
      "roles": [
        "*"
      ],
      "pre": "Credenciales válidas",
      "result": "Token + redirección al dashboard"
    },
    {
      "id": "new-product",
      "screen": "regulatory-portfolio",
      "label": "Nuevo producto + expediente",
      "action": "POST product + POST dossier",
      "roles": [
        "regulatory-specialist",
        "regulatory-administrator"
      ],
      "perm": "PRODUCT.MANAGE + DOSSIER.CREATE",
      "pre": "Bootstrap/autoridad disponible",
      "result": "Producto en portafolio y expediente en Planning"
    },
    {
      "id": "ask-docs",
      "screen": "regulatory-dossier-detail",
      "label": "Pedir docs fábrica",
      "action": "POST …/transition → WaitingManufacturerDocuments",
      "roles": [
        "regulatory-specialist"
      ],
      "perm": "DOSSIER.UPDATE",
      "pre": "Estado Planning (u permitido)",
      "result": "Estado Espera docs fábrica"
    },
    {
      "id": "docs-received",
      "screen": "regulatory-dossier-detail",
      "label": "Docs recibidos",
      "action": "POST …/transition → DocumentsReceived",
      "roles": [
        "regulatory-specialist"
      ],
      "perm": "DOSSIER.UPDATE",
      "result": "Estado Docs recibidos"
    },
    {
      "id": "assemble",
      "screen": "regulatory-dossier-detail",
      "label": "Armar",
      "action": "POST …/transition → Assembling",
      "roles": [
        "regulatory-specialist"
      ],
      "perm": "DOSSIER.UPDATE",
      "result": "Estado Armando expediente"
    },
    {
      "id": "declare-ready",
      "screen": "regulatory-dossier-detail",
      "label": "Declarar técnicamente completo",
      "action": "POST …/transition → ReadyForSubmission",
      "roles": [
        "regulatory-specialist"
      ],
      "perm": "DOSSIER.UPDATE",
      "result": "Listo para aprobación interna"
    },
    {
      "id": "mark-received",
      "screen": "regulatory-dossier-detail",
      "label": "Marcar recibido",
      "action": "PUT requirement status Received",
      "roles": [
        "regulatory-specialist"
      ],
      "perm": "REQUIREMENT.MANAGE / UPDATE",
      "result": "Requisito marcado recibido"
    },
    {
      "id": "accept-req",
      "screen": "regulatory-dossier-detail",
      "label": "Aceptar",
      "action": "PUT requirement status Accepted",
      "roles": [
        "regulatory-reviewer"
      ],
      "perm": "DOSSIER.REVIEW",
      "result": "Requisito aceptado"
    },
    {
      "id": "reject-req",
      "screen": "regulatory-dossier-detail",
      "label": "Rechazar",
      "action": "PUT requirement status Rejected",
      "roles": [
        "regulatory-reviewer"
      ],
      "perm": "DOSSIER.REVIEW",
      "result": "Requisito rechazado; Specialist corrige"
    },
    {
      "id": "approve-internal",
      "screen": "regulatory-dossier-detail",
      "label": "Aprobar internamente para sometimiento",
      "action": "POST …/approve-for-submission",
      "roles": [
        "regulatory-approver"
      ],
      "perm": "APPROVE_FOR_SUBMISSION",
      "pre": "ReadyForSubmission",
      "result": "ApprovedForSubmission — NO es aprobación MINSA/CSS"
    },
    {
      "id": "submit",
      "screen": "regulatory-dossier-detail",
      "label": "Registrar sometimiento",
      "action": "POST …/submit",
      "roles": [
        "regulatory-submitter"
      ],
      "perm": "DOSSIER.SUBMIT",
      "pre": "ApprovedForSubmission (SoD ON)",
      "result": "Submitted — sometimiento registrado"
    },
    {
      "id": "observe",
      "screen": "regulatory-dossier-detail",
      "label": "Registrar observación autoridad",
      "action": "POST …/observations",
      "roles": [
        "regulatory-manager"
      ],
      "perm": "OBSERVATION.MANAGE",
      "pre": "Submitted / UnderAuthorityReview",
      "result": "Observed — observación recibida de la autoridad"
    },
    {
      "id": "respond-obs",
      "screen": "regulatory-dossier-detail",
      "label": "Responder",
      "action": "POST …/observations/{id}/respond",
      "roles": [
        "regulatory-specialist"
      ],
      "perm": "OBSERVATION.MANAGE",
      "pre": "Observación abierta",
      "result": "CorrectingObservation / respuesta registrada"
    },
    {
      "id": "approve-ext",
      "screen": "regulatory-dossier-detail",
      "label": "Registrar aprobación MINSA/CSS + CT/RS",
      "action": "POST …/approve",
      "roles": [
        "regulatory-manager",
        "quality-manager"
      ],
      "perm": "DOSSIER.APPROVE",
      "pre": "Número CT/RS + vencimiento",
      "result": "Approved + CT/RS activo"
    },
    {
      "id": "add-mfr",
      "screen": "regulatory-manufacturers",
      "label": "Alta fabricante",
      "action": "POST manufacturer",
      "roles": [
        "regulatory-specialist",
        "regulatory-administrator"
      ],
      "perm": "MANUFACTURER_DOCUMENT.MANAGE",
      "result": "Fabricante listado (+ cert ISO13485 en flujo demo)"
    },
    {
      "id": "add-lic",
      "screen": "regulatory-licenses",
      "label": "Nueva licencia",
      "action": "POST license",
      "roles": [
        "regulatory-administrator"
      ],
      "perm": "LICENSE.MANAGE",
      "result": "Licencia en listado"
    },
    {
      "id": "stage-xlsx",
      "screen": "regulatory-import",
      "label": "Stage XLSX",
      "action": "POST import stage",
      "roles": [
        "regulatory-administrator"
      ],
      "perm": "CONFIGURE",
      "result": "Job de importación en staging"
    },
    {
      "id": "bootstrap",
      "screen": "regulatory-config",
      "label": "Bootstrap regulatorio",
      "action": "Seed authorities + pack",
      "roles": [
        "regulatory-administrator",
        "tenant-administrator"
      ],
      "perm": "CONFIGURE",
      "result": "MINSA/CSS + pack de requisitos"
    },
    {
      "id": "back-dossiers",
      "screen": "regulatory-dossier-detail",
      "label": "← Expedientes",
      "action": "Volver al listado",
      "roles": [
        "*"
      ],
      "result": "Vista Expedientes"
    }
  ],
  "workflow": {
    "title": "Flujo certificado del expediente",
    "steps": [
      {
        "id": "prep",
        "status": "Planning…ReadyForSubmission",
        "label": "Preparación",
        "role": "Regulatory Specialist",
        "ui": [
          "Pedir docs fábrica",
          "Docs recibidos",
          "Armar",
          "Marcar recibido",
          "Declarar técnicamente completo"
        ]
      },
      {
        "id": "review",
        "status": "Assembling / ReadyForSubmission",
        "label": "Revisión técnica",
        "role": "Regulatory Reviewer",
        "ui": [
          "Aceptar",
          "Rechazar"
        ]
      },
      {
        "id": "internal",
        "status": "ReadyForSubmission → ApprovedForSubmission",
        "label": "Aprobación interna para sometimiento",
        "role": "Regulatory Approver",
        "ui": [
          "Aprobar internamente para sometimiento"
        ]
      },
      {
        "id": "submit",
        "status": "ApprovedForSubmission → Submitted",
        "label": "Sometimiento",
        "role": "Regulatory Submitter",
        "ui": [
          "Registrar sometimiento"
        ]
      },
      {
        "id": "authority",
        "status": "Submitted → UnderAuthorityReview",
        "label": "Revisión de autoridad",
        "role": "Regulatory Manager (seguimiento)",
        "ui": []
      },
      {
        "id": "obs",
        "status": "Observed",
        "label": "Observación",
        "role": "Regulatory Manager",
        "ui": [
          "Registrar observación autoridad"
        ]
      },
      {
        "id": "response",
        "status": "CorrectingObservation",
        "label": "Respuesta",
        "role": "Regulatory Specialist",
        "ui": [
          "Responder"
        ]
      },
      {
        "id": "resubmit",
        "status": "Resubmitted",
        "label": "Resometimiento",
        "role": "Regulatory Submitter / transición",
        "ui": [
          "Registrar sometimiento / transition Resubmitted"
        ]
      },
      {
        "id": "external",
        "status": "Approved",
        "label": "Decisión externa",
        "role": "Regulatory Manager / Quality Manager",
        "ui": [
          "Registrar aprobación MINSA/CSS + CT/RS"
        ]
      },
      {
        "id": "ctrs",
        "status": "SanitaryRegistration",
        "label": "CT/RS activo",
        "role": "Manager / QM",
        "ui": [
          "Registros CT/RS"
        ]
      },
      {
        "id": "renewal",
        "status": "Renovacion (pipeline)",
        "label": "Renovación",
        "role": "Regulatory Manager",
        "ui": [
          "Pipeline Renovacion / StartRenewal"
        ]
      }
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
      "Rejected": "Rechazo externo de la autoridad"
    }
  },
  "glossary": [
    {
      "term": "Tenant",
      "def": "Organización aislada en Compliance 360 (ej. Irving Corro S.A).",
      "example": "TenantId en el JWT",
      "screens": [
        "login",
        "tenant-administration"
      ],
      "roles": [
        "tenant-administrator"
      ]
    },
    {
      "term": "Regulatory Affairs",
      "def": "Módulo de asuntos regulatorios de dispositivos médicos.",
      "example": "Menú Regulatory Management",
      "screens": [
        "regulatory-shell"
      ],
      "roles": [
        "*"
      ]
    },
    {
      "term": "Expediente / Dossier",
      "def": "Caso de registro ante una autoridad (MINSA/CSS).",
      "example": "Caso CASE-…",
      "screens": [
        "regulatory-dossiers"
      ],
      "roles": [
        "regulatory-specialist"
      ]
    },
    {
      "term": "Requirement Pack",
      "def": "Plantilla publicada de requisitos (ej. pack bootstrap con 22 ítems).",
      "example": "Bootstrap regulatorio",
      "screens": [
        "regulatory-config"
      ],
      "roles": [
        "regulatory-administrator"
      ]
    },
    {
      "term": "Requisito",
      "def": "Ítem del checklist del expediente que se marca, acepta o rechaza.",
      "example": "Literatura técnica",
      "screens": [
        "regulatory-dossier-detail"
      ],
      "roles": [
        "regulatory-reviewer"
      ]
    },
    {
      "term": "Fabricante",
      "def": "Perfil del fabricante y sus certificados (ej. ISO 13485).",
      "example": "Alta fabricante",
      "screens": [
        "regulatory-manufacturers"
      ],
      "roles": [
        "regulatory-specialist"
      ]
    },
    {
      "term": "Autoridad",
      "def": "Entidad reguladora (MINSA, CSS u otra configurada).",
      "example": "Código MINSA",
      "screens": [
        "regulatory-config"
      ],
      "roles": [
        "regulatory-administrator"
      ]
    },
    {
      "term": "MINSA",
      "def": "Ministerio de Salud — autoridad típica en el bootstrap.",
      "example": "Aprobación MINSA/CSS",
      "screens": [
        "regulatory-dossier-detail"
      ],
      "roles": [
        "regulatory-manager"
      ]
    },
    {
      "term": "CSS",
      "def": "Caja de Seguro Social — autoridad típica en el bootstrap.",
      "example": "Autoridad CSS",
      "screens": [
        "regulatory-config"
      ],
      "roles": [
        "regulatory-administrator"
      ]
    },
    {
      "term": "CT/RS",
      "def": "Certificado / Registro Sanitario emitido por la autoridad. Se registra tras la aprobación externa.",
      "example": "MQ-4521-07-26",
      "screens": [
        "regulatory-registrations"
      ],
      "roles": [
        "regulatory-manager"
      ]
    },
    {
      "term": "Clase de riesgo",
      "def": "Clasificación A/B/C del dispositivo. Distinta del módulo Risk Management.",
      "example": "A",
      "screens": [
        "regulatory-portfolio"
      ],
      "roles": [
        "regulatory-specialist"
      ]
    },
    {
      "term": "Aprobación interna",
      "def": "Autorización interna para poder someter. Estado ApprovedForSubmission. No es decisión de MINSA/CSS.",
      "example": "Aprobar internamente para sometimiento",
      "screens": [
        "regulatory-dossier-detail"
      ],
      "roles": [
        "regulatory-approver"
      ]
    },
    {
      "term": "Sometimiento",
      "def": "Registro de que el expediente fue enviado a la autoridad.",
      "example": "Registrar sometimiento",
      "screens": [
        "regulatory-dossier-detail"
      ],
      "roles": [
        "regulatory-submitter"
      ]
    },
    {
      "term": "Observación",
      "def": "Hallazgo de la autoridad que debe responderse.",
      "example": "Registrar observación autoridad",
      "screens": [
        "regulatory-dossier-detail"
      ],
      "roles": [
        "regulatory-manager"
      ]
    },
    {
      "term": "Resometimiento",
      "def": "Nuevo envío tras corregir una observación.",
      "example": "Estado Resubmitted",
      "screens": [
        "regulatory-pipeline"
      ],
      "roles": [
        "regulatory-submitter"
      ]
    },
    {
      "term": "Decisión externa",
      "def": "Aprobación o rechazo emitido por la autoridad, registrado en el sistema.",
      "example": "Registrar aprobación MINSA/CSS + CT/RS",
      "screens": [
        "regulatory-dossier-detail"
      ],
      "roles": [
        "regulatory-manager"
      ]
    },
    {
      "term": "Renovación",
      "def": "Inicio de un nuevo ciclo cuando el CT/RS vence o está por vencer.",
      "example": "Columna Renovacion en Pipeline",
      "screens": [
        "regulatory-pipeline"
      ],
      "roles": [
        "regulatory-manager"
      ]
    },
    {
      "term": "Licencia operativa",
      "def": "Licencia de la empresa (no el CT/RS del producto).",
      "example": "Nueva licencia",
      "screens": [
        "regulatory-licenses"
      ],
      "roles": [
        "regulatory-administrator"
      ]
    },
    {
      "term": "SoD",
      "def": "Segregation of Duties: impide que la misma persona prepare, revise, apruebe y someta.",
      "example": "SoD Settings",
      "screens": [
        "regulatory-sod"
      ],
      "roles": [
        "tenant-administrator"
      ]
    },
    {
      "term": "RBAC",
      "def": "Control de acceso por roles y permisos atómicos.",
      "example": "REGULATORY.DOSSIER.SUBMIT",
      "screens": [
        "tenant-administration"
      ],
      "roles": [
        "tenant-administrator"
      ]
    },
    {
      "term": "Pipeline",
      "def": "Vista kanban del estado de todos los expedientes.",
      "example": "Columnas Planning…Approved",
      "screens": [
        "regulatory-pipeline"
      ],
      "roles": [
        "regulatory-manager"
      ]
    },
    {
      "term": "Portfolio / Portafolio",
      "def": "Lista de productos del tenant.",
      "example": "Nuevo producto + expediente",
      "screens": [
        "regulatory-portfolio"
      ],
      "roles": [
        "regulatory-specialist"
      ]
    },
    {
      "term": "Aging",
      "def": "Expedientes detenidos más de 14 días (métrica del dashboard).",
      "example": "Detenidos >14d",
      "screens": [
        "regulatory-dashboard"
      ],
      "roles": [
        "regulatory-manager"
      ]
    },
    {
      "term": "Alert",
      "def": "Aviso de vencimiento o condición operativa evaluada por el sistema.",
      "example": "Vista Alertas",
      "screens": [
        "regulatory-alerts"
      ],
      "roles": [
        "regulatory-viewer"
      ]
    },
    {
      "term": "Audit Trail",
      "def": "Registro histórico de acciones del sistema.",
      "example": "#/audit-trail",
      "screens": [
        "audit-trail"
      ],
      "roles": [
        "quality-manager"
      ]
    },
    {
      "term": "REGUTRACK",
      "def": "Libro Excel histórico que Compliance 360 reemplaza vía importación y consola RA.",
      "example": "Stage XLSX",
      "screens": [
        "regulatory-import"
      ],
      "roles": [
        "regulatory-administrator"
      ]
    }
  ],
  "errors": [
    {
      "id": "empty-required",
      "title": "Campo obligatorio vacío",
      "why": "El API/UI rechaza crear sin Marca, Nombre regulatorio o Código catálogo.",
      "fix": "Complete el prompt con un valor no vacío.",
      "who": "Regulatory Specialist"
    },
    {
      "id": "dup-catalog",
      "title": "Código catálogo duplicado",
      "why": "Índice único TenantId+CatalogCode.",
      "fix": "Use otro código de catálogo.",
      "who": "Regulatory Specialist"
    },
    {
      "id": "self-review",
      "title": "Specialist intenta revisar su propio expediente",
      "why": "SoD PreventSelfReview.",
      "fix": "Otro usuario con rol Reviewer debe aceptar/rechazar.",
      "who": "Regulatory Reviewer"
    },
    {
      "id": "rev-approve",
      "title": "Reviewer intenta aprobar internamente",
      "why": "Sin permiso APPROVE_FOR_SUBMISSION.",
      "fix": "El Approver ejecuta «Aprobar internamente para sometimiento».",
      "who": "Regulatory Approver"
    },
    {
      "id": "appr-submit",
      "title": "Approver intenta someter",
      "why": "SeparateApproverAndSubmitter / sin SUBMIT.",
      "fix": "El Submitter registra el sometimiento.",
      "who": "Regulatory Submitter"
    },
    {
      "id": "sub-approve",
      "title": "Submitter intenta aprobar internamente",
      "why": "Sin APPROVE_FOR_SUBMISSION.",
      "fix": "Solo el Approver otorga la autorización interna.",
      "who": "Regulatory Approver"
    },
    {
      "id": "viewer-edit",
      "title": "Viewer intenta editar",
      "why": "Solo permisos READ.",
      "fix": "Solicite un rol operativo al Tenant Administrator.",
      "who": "Tenant Administrator"
    },
    {
      "id": "tac-operate",
      "title": "Tenant Administrator intenta operar el expediente",
      "why": "Sin CREATE/SUBMIT/APPROVE por defecto.",
      "fix": "Asigne roles RA operativos a usuarios dedicados.",
      "who": "Tenant Administrator"
    },
    {
      "id": "submit-without-internal",
      "title": "Someter sin aprobación interna",
      "why": "Con SoD ON el estado debe ser ApprovedForSubmission.",
      "fix": "Complete la aprobación interna primero.",
      "who": "Regulatory Approver → Submitter"
    },
    {
      "id": "ext-without-ctrs",
      "title": "Aprobar externamente sin número CT/RS",
      "why": "El prompt exige Número CT/RS obligatorio.",
      "fix": "Ingrese el número emitido por la autoridad y el vencimiento ISO.",
      "who": "Regulatory Manager"
    },
    {
      "id": "session",
      "title": "Sesión expirada",
      "why": "JWT inválido o ausente.",
      "fix": "Vuelva a #/login e inicie sesión.",
      "who": "Cualquier usuario"
    }
  ],
  "tutorials": {
    "tac-login": {
      "title": "Iniciar sesión",
      "role": "tenant-administrator",
      "steps": [
        "Abra http://localhost:5272",
        "Si no está autenticado, irá a #/login",
        "Escriba su correo de Tenant Administrator (ej. irvingcorrosk19@gmail.com)",
        "Escriba su contraseña",
        "Presione «Iniciar sesion» o «Completar login seguro»",
        "Verifique que el sidebar muestre Compliance 360 y su tenant"
      ],
      "result": "Dashboard / menú Enterprise visible",
      "next": "Puede ir a Tenant Administration"
    },
    "tac-users": {
      "title": "Crear / activar usuarios",
      "role": "tenant-administrator",
      "steps": [
        "Menú lateral → Tenant Administration (#/tenant-administration)",
        "Abra la sección de usuarios",
        "Cree el usuario con correo corporativo",
        "Active o desactive según corresponda"
      ],
      "result": "Usuario listado en el tenant",
      "next": "Asignar roles"
    },
    "tac-roles": {
      "title": "Asignar roles",
      "role": "tenant-administrator",
      "steps": [
        "En Tenant Administration abra roles/usuarios",
        "Asigne exactamente un rol operativo RA por persona cuando SoD lo requiera",
        "Ejemplo: un usuario = Regulatory Specialist; otro = Regulatory Reviewer",
        "No combine Specialist+Approver+Submitter en la misma persona sin override"
      ],
      "result": "Permisos persistidos (RBAC)",
      "next": "Configurar SoD / Bootstrap"
    },
    "tac-configure-ra": {
      "title": "Ver configuración Regulatory Affairs",
      "role": "tenant-administrator",
      "steps": [
        "Menú → Regulatory Management (#/regulatory)",
        "Abra Configuración",
        "Presione «Bootstrap regulatorio» si aún no hay autoridades/pack",
        "Confirme el mensaje de éxito (toast Bootstrap OK)"
      ],
      "result": "Autoridades MINSA/CSS y pack disponibles",
      "next": "Revisar SoD Settings"
    },
    "tac-sod": {
      "title": "Consultar SoD",
      "role": "tenant-administrator",
      "steps": [
        "En #/regulatory abra SoD Settings",
        "Lea la política JSON (PreventSelfReview, SeparateApproverAndSubmitter, etc.)",
        "Los cambios vía API requieren REGULATORY.SOD.MANAGE"
      ],
      "result": "Comprende las barreras SoD",
      "next": "Verificar que no opera expedientes"
    },
    "tac-no-operate": {
      "title": "Verificar que no opera el expediente",
      "role": "tenant-administrator",
      "steps": [
        "Abra Portafolio",
        "Observe que el botón «Nuevo producto + expediente» no aparece sin PRODUCT.MANAGE+DOSSIER.CREATE",
        "Abra un expediente: no verá Aprobar internamente / Registrar sometimiento / Aprobación externa"
      ],
      "result": "SoD respetado para TAC",
      "next": "Cerrar sesión"
    },
    "tac-logout": {
      "title": "Cerrar sesión",
      "role": "tenant-administrator",
      "steps": [
        "Use la acción de logout de la aplicación",
        "Confirme redirección a #/login"
      ],
      "result": "Sesión terminada",
      "next": "—"
    },
    "adm-bootstrap": {
      "title": "Bootstrap regulatorio",
      "role": "regulatory-administrator",
      "steps": [
        "Login con rol Regulatory Administrator",
        "Menú → Regulatory Management → Configuración",
        "Presione «Bootstrap regulatorio»",
        "Espere toast «Bootstrap OK»"
      ],
      "result": "Autoridades y pack listos",
      "next": "Importar REGUTRACK"
    },
    "adm-authority": {
      "title": "Crear / verificar autoridades",
      "role": "regulatory-administrator",
      "steps": [
        "Tras el bootstrap, las autoridades MINSA y CSS quedan disponibles",
        "Se usan al crear expedientes (selección de autoridad en el flujo de dossier)"
      ],
      "result": "Autoridad utilizable en prep",
      "next": "Importación"
    },
    "adm-import": {
      "title": "Importación REGUTRACK",
      "role": "regulatory-administrator",
      "steps": [
        "Vaya a Importación",
        "Seleccione el archivo XLSX (contrato REGUTRACK)",
        "Presione «Stage XLSX»",
        "Revise el job listado y errores de staging"
      ],
      "result": "Stage OK / jobs visibles",
      "next": "Resolver filas con error"
    },
    "adm-alerts": {
      "title": "Consultar alertas",
      "role": "regulatory-administrator",
      "steps": [
        "Abra Alertas",
        "Revise alertType y message de la evaluación"
      ],
      "result": "Lista de alertas",
      "next": "—"
    },
    "adm-sod-view": {
      "title": "Ver SoD Settings",
      "role": "regulatory-administrator",
      "steps": [
        "Abra SoD Settings",
        "Confirme políticas activas"
      ],
      "result": "Política visible",
      "next": "—"
    },
    "spec-product": {
      "title": "Crear producto + expediente",
      "role": "regulatory-specialist",
      "steps": [
        "Login como Regulatory Specialist",
        "Menú → Regulatory Management → Portafolio",
        "Presione «Nuevo producto + expediente»",
        "En Marca escriba p.ej. DEMO",
        "En Nombre regulatorio escriba el nombre oficial del producto",
        "En Código catálogo escriba un código único CAT-######",
        "Confirme los prompts; el sistema crea producto (país PA, categoría, clase A en flujo demo) y expediente en Planning"
      ],
      "result": "Fila en Portafolio + expediente en Prep",
      "next": "Completar checklist"
    },
    "spec-mfr": {
      "title": "Alta fabricante",
      "role": "regulatory-specialist",
      "steps": [
        "Abra Fabricantes",
        "Presione «Alta fabricante»",
        "Nombre legal: razón social",
        "País: p.ej. CN"
      ],
      "result": "Fabricante listado",
      "next": "Pedir docs fábrica"
    },
    "spec-dossier": {
      "title": "Abrir expediente y pedir docs",
      "role": "regulatory-specialist",
      "steps": [
        "Abra Expedientes o haga clic en el caso del Pipeline",
        "Presione «Pedir docs fábrica»",
        "Observe el estado «Espera docs fábrica»"
      ],
      "result": "WaitingManufacturerDocuments",
      "next": "Marcar requisitos"
    },
    "spec-checklist": {
      "title": "Marcar requisitos recibidos",
      "role": "regulatory-specialist",
      "steps": [
        "En el workspace del expediente revise la lista de requisitos del pack",
        "Para cada documento recibido presione «Marcar recibido»",
        "Use «Docs recibidos» y luego «Armar» según avance"
      ],
      "result": "Requisitos en progreso / Assembling",
      "next": "Esperar Reviewer"
    },
    "spec-ready": {
      "title": "Declarar técnicamente completo",
      "role": "regulatory-specialist",
      "steps": [
        "Cuando la preparación esté lista y la revisión lo permita",
        "Presione «Declarar técnicamente completo»",
        "El estado pasa a ReadyForSubmission"
      ],
      "result": "Listo para el Approver",
      "next": "Approver interno"
    },
    "spec-respond-obs": {
      "title": "Responder observación",
      "role": "regulatory-specialist",
      "steps": [
        "Cuando el Manager registre una observación, abra el expediente",
        "En Observaciones presione «Responder»",
        "Documente la corrección según el API de respond"
      ],
      "result": "Respuesta registrada",
      "next": "Resometimiento"
    },
    "spec-fix-return": {
      "title": "Corregir expediente devuelto",
      "role": "regulatory-specialist",
      "steps": [
        "Si el Reviewer rechazó un requisito, abra el ítem",
        "Corrija la evidencia / datos",
        "Vuelva a marcar recibido para nueva revisión"
      ],
      "result": "Listo para re-revisión",
      "next": "Reviewer"
    },
    "rev-queue": {
      "title": "Ver cola de revisión",
      "role": "regulatory-reviewer",
      "steps": [
        "Login como Regulatory Reviewer",
        "Abra Pipeline o Expedientes",
        "Identifique casos en Armando / Técnicamente completo",
        "Haga clic en el caso"
      ],
      "result": "Workspace abierto",
      "next": "Aceptar/Rechazar"
    },
    "rev-accept": {
      "title": "Aceptar requisito",
      "role": "regulatory-reviewer",
      "steps": [
        "Revise el requisito y su evidencia",
        "Presione «Aceptar»",
        "Confirme que el estado del requisito pasa a Accepted"
      ],
      "result": "Requisito aceptado",
      "next": "Siguiente requisito"
    },
    "rev-reject": {
      "title": "Rechazar requisito",
      "role": "regulatory-reviewer",
      "steps": [
        "Si falta información, presione «Rechazar»",
        "El Specialist debe corregir",
        "Usted no aprueba internamente ni somete"
      ],
      "result": "Requisito Rejected",
      "next": "Specialist corrige"
    },
    "rev-return": {
      "title": "Entregar a aprobación",
      "role": "regulatory-reviewer",
      "steps": [
        "Cuando los requisitos críticos estén Accepted",
        "El Specialist declara técnicamente completo",
        "El Approver toma el caso — usted no presiona Aprobar internamente"
      ],
      "result": "Cola del Approver",
      "next": "Regulatory Approver"
    },
    "appr-queue": {
      "title": "Cola de aprobación interna",
      "role": "regulatory-approver",
      "steps": [
        "Login Approver",
        "Pipeline → columna Técnicamente completo / ReadyForSubmission",
        "Abra el expediente"
      ],
      "result": "Botón Aprobar internamente visible",
      "next": "Aprobar"
    },
    "appr-approve": {
      "title": "Aprobar internamente",
      "role": "regulatory-approver",
      "steps": [
        "Revise resumen y requisitos críticos",
        "Presione «Aprobar internamente para sometimiento»",
        "Lea el nuevo estado: «Aprobado internamente para sometimiento»",
        "Esto NO es la aprobación de MINSA/CSS"
      ],
      "result": "ApprovedForSubmission",
      "next": "Submitter"
    },
    "appr-why-no-submit": {
      "title": "Por qué no puedo someter",
      "role": "regulatory-approver",
      "steps": [
        "Busque el botón «Registrar sometimiento» — no debe aparecer sin SUBMIT",
        "SoD SeparateApproverAndSubmitter obliga a otra persona"
      ],
      "result": "Comprende la separación",
      "next": "—"
    },
    "sub-queue": {
      "title": "Expedientes autorizados",
      "role": "regulatory-submitter",
      "steps": [
        "Login Submitter",
        "Pipeline → Aprobado internamente para sometimiento",
        "Abra el expediente"
      ],
      "result": "Botón Registrar sometimiento visible",
      "next": "Someter"
    },
    "sub-submit": {
      "title": "Registrar sometimiento",
      "role": "regulatory-submitter",
      "steps": [
        "Presione «Registrar sometimiento»",
        "Confirme la acción",
        "Estado: «Sometido ante autoridad»"
      ],
      "result": "Submitted",
      "next": "Manager sigue la autoridad"
    },
    "sub-why-no-approve": {
      "title": "Por qué no apruebo internamente",
      "role": "regulatory-submitter",
      "steps": [
        "Sin permiso APPROVE_FOR_SUBMISSION el botón no aparece",
        "Su función es registrar el envío real, no la autorización interna"
      ],
      "result": "Separación clara",
      "next": "—"
    },
    "mgr-dashboard": {
      "title": "Supervisar dashboard y pipeline",
      "role": "regulatory-manager",
      "steps": [
        "Abra Dashboard: Productos, CT activos, En trámite, Req. críticos, Detenidos >14d, Por vencer",
        "Abra Pipeline y revise columnas incluidas Observed / Renovacion"
      ],
      "result": "Visión operativa",
      "next": "Observación"
    },
    "mgr-observe": {
      "title": "Registrar observación de autoridad",
      "role": "regulatory-manager",
      "steps": [
        "Abra expediente Submitted / UnderAuthorityReview",
        "Presione «Registrar observación autoridad»",
        "Escriba la descripción exacta de la observación",
        "Estado: Observed — «Observación recibida de la autoridad»"
      ],
      "result": "Specialist puede Responder",
      "next": "Esperar respuesta"
    },
    "mgr-external-approve": {
      "title": "Registrar decisión externa + CT/RS",
      "role": "regulatory-manager",
      "steps": [
        "Cuando la autoridad apruebe, abra el expediente",
        "Presione «Registrar aprobación MINSA/CSS + CT/RS»",
        "Número CT/RS: el emitido por la autoridad (obligatorio)",
        "Vencimiento ISO: YYYY-MM-DD",
        "Estado: Aprobación registrada de MINSA/CSS (externa) + CT/RS activo"
      ],
      "result": "Registro en Registros CT/RS",
      "next": "Vigencia / Renovación"
    },
    "mgr-renewal": {
      "title": "Iniciar / ver renovación",
      "role": "regulatory-manager",
      "steps": [
        "En Pipeline revise la columna Renovacion",
        "Use el flujo de renovación (StartRenewal / nuevo proceso renew) según permiso REGISTRATION.MANAGE"
      ],
      "result": "Ciclo de renovación visible",
      "next": "—"
    },
    "mgr-breakglass": {
      "title": "Break Glass SoD",
      "role": "regulatory-manager",
      "steps": [
        "Solo con REGULATORY.SOD.EMERGENCY_OVERRIDE",
        "Cualquier override queda auditado",
        "No use override para saltar rutinariamente Specialist/Approver/Submitter"
      ],
      "result": "Excepción auditada",
      "next": "—"
    },
    "view-read": {
      "title": "Consultar sin modificar",
      "role": "regulatory-viewer",
      "steps": [
        "Recorra Dashboard, Portafolio, Pipeline, Expedientes, CT/RS, Alertas",
        "Abra un expediente en solo lectura"
      ],
      "result": "Información visible",
      "next": "—"
    },
    "view-no-edit": {
      "title": "Entender bloqueo de edición",
      "role": "regulatory-viewer",
      "steps": [
        "Verifique ausencia de botones Nuevo / Aprobar / Someter / Stage",
        "Si necesita operar, solicite rol al TAC"
      ],
      "result": "Límites claros",
      "next": "—"
    },
    "qm-external": {
      "title": "Registrar CT/RS (QM)",
      "role": "quality-manager",
      "steps": [
        "Login Quality Manager",
        "Abra expediente listo para decisión externa",
        "Presione «Registrar aprobación MINSA/CSS + CT/RS» si tiene DOSSIER.APPROVE",
        "Capture Número CT/RS y vencimiento"
      ],
      "result": "CT/RS registrado",
      "next": "Audit Trail"
    },
    "qm-limits": {
      "title": "Límites vs Regulatory Manager",
      "role": "quality-manager",
      "steps": [
        "Usted no prepara dossiers por defecto",
        "Su foco QMS + registro externo; la supervisión diaria del pipeline es del Manager"
      ],
      "result": "Roles no confundidos",
      "next": "—"
    },
    "qm-audit": {
      "title": "Consultar auditoría",
      "role": "quality-manager",
      "steps": [
        "Menú → Audit Trail",
        "Busque eventos REGULATORY relevantes"
      ],
      "result": "Trazabilidad",
      "next": "—"
    }
  },
  "appUrl": "http://localhost:5272",
  "raRoute": "#/regulatory",
  "loginRoute": "#/login",
  "version": "1.0.0-certified-flow"
};
