const FALLBACK_DATA = {
  modules: [
    "Dashboard","Document Management","Technical Sheets","Suppliers","CAPA","Risks","Audits","Reports",
    "Notifications","Storage","Users","Roles","Permissions","Settings","Tenant Administration",
    "Platform Administration","Indicators","Approval Workflows","Profile"
  ],
  roles: [
    {"name":"Platform Admin","level":"Estratégico","summary":"Gobierno de plataforma y tenants"},
    {"name":"Tenant Admin","level":"Administrativo","summary":"Administra configuración y usuarios tenant"},
    {"name":"Quality Manager","level":"Operativo","summary":"Conduce calidad y aprobaciones"},
    {"name":"Document Controller","level":"Operativo","summary":"Gestiona ciclo documental"},
    {"name":"Auditor","level":"Control","summary":"Ejecuta auditorías y hallazgos"},
    {"name":"Risk Manager","level":"Control","summary":"Gestiona riesgos y tratamiento"},
    {"name":"Supplier Manager","level":"Operativo","summary":"Evalúa y homologa proveedores"},
    {"name":"CAPA Manager","level":"Operativo","summary":"Gestiona acciones correctivas"},
    {"name":"Reporting Manager","level":"Analítico","summary":"Publica reportes ejecutivos"},
    {"name":"Storage Admin","level":"Infraestructura","summary":"Controla proveedores de storage"},
    {"name":"Notification Admin","level":"Infraestructura","summary":"Configura canales y plantillas"},
    {"name":"Indicators Manager","level":"Analítico","summary":"Define y monitorea KPIs"},
    {"name":"Viewer","level":"Lectura","summary":"Consulta transversal sin edición"}
  ],
  kpis: [
    {"label":"Usuarios activos","value":"148"},
    {"label":"Documentos controlados","value":"1,284"},
    {"label":"CAPA abiertas","value":"12"},
    {"label":"Riesgos críticos","value":"3"}
  ],
  dashboard: { auditOpen: 4, capaOpen: 12, criticalRisk: 3, indicators: 18, datasets: 9, compliancePercent: 94 },
  records: {
    "Document Management": [
      { code: "BPM-LIM-001", title: "Limpieza de línea", status: "Published", expiresAtUtc: "2027-03-15" },
      { code: "POE-CCP-014", title: "Control CCP temperatura", status: "InReview", expiresAtUtc: "2026-11-20" }
    ],
    "CAPA": [{ code: "CAPA-2026-014", title: "Desviación temperatura CCP", status: "Open", dueDate: "2026-08-15" }],
    "Risks": [{ code: "RSK-CC-001", title: "Contaminación cruzada", probability: "Medium", impact: "High", status: "Active" }],
    "Audits": [{ code: "AUD-INT-2026-01", title: "Auditoría interna ISO 9001", status: "InProgress", findings: 2 }],
    "Reports": [{ code: "RPT-CAPA-OPEN", title: "CAPA abiertas por área", status: "Active", format: "PDF" }],
    "Indicators": [{ code: "KPI-NC-RATE", title: "Tasa NC", target: "<= 1.0%", current: "1.2%", status: "Alert" }],
    "Tenant Administration": [{ code: "USR-001", title: "quality@empresa.com", status: "Active", role: "Quality Manager" }]
  }
};

const navigation = [
  { group: "Command Center", items: [
    ["dashboard", "Executive Dashboard"],
    ["compliance", "Compliance Dashboard"],
    ["reports", "Report Center"],
    ["audit-trail", "Audit Trail"]
  ]},
  { group: "Operations", items: [
    ["documents", "Document Management"],
    ["technical-sheets", "Technical Sheets"],
    ["suppliers", "Supplier Management"],
    ["audits", "Audit Management"],
    ["capa", "CAPA"],
    ["risks", "Risk Management"],
    ["indicators", "Quality Indicators"]
  ]},
  { group: "Enterprise", items: [
    ["tenant-administration", "Tenant Administration"],
    ["security", "Security"],
    ["configuration", "Configuration"]
  ]},
  { group: "Demo Enablement", items: [
    ["wizard", "Wizard Principal"],
    ["roles", "Seleccionar Rol"],
    ["training", "Entrenamiento por Rol"],
    ["certification", "Certificación"],
    ["ia", "Asistente IA"],
    ["favorites", "Favoritos"]
  ]}
];

const ROUTE_MODULE_MAP = {
  documents: "Document Management",
  "technical-sheets": "Technical Sheets",
  suppliers: "Suppliers",
  audits: "Audits",
  capa: "CAPA",
  risks: "Risks",
  indicators: "Indicators",
  reports: "Reports",
  "tenant-administration": "Tenant Administration",
  security: "Security",
  configuration: "Configuration"
};

const ROLE_NAV = {
  "Quality Manager": ["dashboard", "compliance", "reports", "audit-trail", "documents", "technical-sheets", "capa", "risks", "indicators", "audits"],
  "Document Controller": ["dashboard", "reports", "audit-trail", "documents"],
  "Auditor": ["dashboard", "audits", "capa", "documents", "suppliers", "reports", "audit-trail"],
  "CAPA Manager": ["dashboard", "capa", "risks", "audits", "reports", "audit-trail"],
  "Risk Manager": ["dashboard", "risks", "indicators", "audits", "reports", "audit-trail"],
  "Supplier Manager": ["dashboard", "suppliers", "documents", "reports", "audit-trail"],
  "Reporting Manager": ["dashboard", "reports", "indicators", "risks", "capa", "audits", "audit-trail"],
  "Indicators Manager": ["dashboard", "indicators", "risks", "reports", "audit-trail"],
  "Tenant Admin": ["dashboard", "tenant-administration", "audit-trail"],
  "Storage Admin": ["dashboard", "configuration", "audit-trail"],
  "Notification Admin": ["dashboard", "configuration", "audit-trail"],
  "Viewer": ["dashboard", "documents", "technical-sheets", "suppliers", "audits", "capa", "risks", "indicators", "reports", "audit-trail"]
};

const routeMeta = navigation.flatMap(g => g.items.map(([key, title]) => ({
  key, title, group: g.group,
  process: {
    wizard: "Recorrido guiado del producto",
    roles: "Asignación de contexto operativo",
    training: "Aprendizaje funcional guiado",
    certification: "Validación de aprendizaje por rol",
    dashboard: "Monitoreo ejecutivo",
    compliance: "Cumplimiento normativo consolidado",
    reports: "Consolidación y exportación",
    "audit-trail": "Trazabilidad y auditoría del sistema",
    documents: "Control documental y versiones",
    "technical-sheets": "Fichas técnicas de producto",
    suppliers: "Homologación y evaluación de proveedores",
    audits: "Programas y hallazgos de auditoría",
    capa: "Acciones correctivas y preventivas",
    risks: "Matriz y tratamiento de riesgos",
    indicators: "KPIs y metas de calidad",
    "tenant-administration": "Usuarios, roles y configuración tenant",
    security: "Controles de seguridad del tenant",
    configuration: "Storage, notificaciones e integraciones",
    ia: "Ayuda contextual simulada",
    favorites: "Acceso rápido"
  }[key] || "Flujo funcional"
})));

const MODULE_CONFIG = {
  "Document Management": {
    title: "Document Management",
    description: "Listado documental, vigencias, versiones, workflow y aprobaciones.",
    steps: ["Classify", "Version", "Approve", "Expire"],
    columns: ["code", "title", "status", "expiresAtUtc"],
    labels: { code: "Code", title: "Title", status: "Status", expiresAtUtc: "Expires" },
    route: "documents"
  },
  "Technical Sheets": {
    title: "Technical Sheets",
    description: "Fichas técnicas, ingredientes, nutrientes, certificaciones y aprobaciones.",
    steps: ["Product", "Ingredients", "Certify", "Approve"],
    columns: ["code", "title", "status", "productName"],
    labels: { code: "Code", title: "Title", status: "Status", productName: "Product" },
    route: "technical-sheets",
    readOnlyRoles: ["Document Controller", "Quality Manager", "Viewer"]
  },
  "Suppliers": {
    title: "Supplier Management",
    description: "Expediente proveedor, documentos, evaluaciones, homologación y alertas.",
    steps: ["Register", "Evaluate", "Homologate", "Monitor"],
    columns: ["code", "title", "status", "score"],
    labels: { code: "Tax ID", title: "Legal Name", status: "Status", score: "Score" },
    route: "suppliers"
  },
  "CAPA": {
    title: "CAPA",
    description: "Acciones correctivas, causa raíz, seguimiento y cierre por eficacia.",
    steps: ["Create", "Root cause", "Actions", "Effectiveness"],
    columns: ["code", "title", "status", "dueDate"],
    labels: { code: "Code", title: "Issue", status: "Status", dueDate: "Due Date" }
  },
  "Risks": {
    title: "Risk Management",
    description: "Registro de riesgos, evaluación, tratamiento y matriz de calor.",
    steps: ["Classify", "Assess", "Treat", "Review"],
    columns: ["code", "title", "probability", "impact", "status"],
    labels: { code: "Code", title: "Risk", probability: "Probability", impact: "Impact", status: "Status" }
  },
  "Audits": {
    title: "Audit Management",
    description: "Programas, planes, ejecución, hallazgos y trazabilidad.",
    steps: ["Program", "Plan", "Execute", "Close"],
    columns: ["code", "title", "status", "findings"],
    labels: { code: "Code", title: "Audit", status: "Status", findings: "Findings" }
  },
  "Reports": {
    title: "Report Center",
    description: "Busca, ejecuta, programa y exporta reportes empresariales.",
    steps: ["Select", "Filter", "Execute", "Export"],
    columns: ["code", "title", "status", "format"],
    labels: { code: "Code", title: "Report", status: "Status", format: "Format" }
  },
  "Indicators": {
    title: "Quality Indicators",
    description: "KPIs, metas, umbrales y tendencias de cumplimiento.",
    steps: ["Define", "Target", "Measure", "Trend"],
    columns: ["code", "title", "target", "current", "status"],
    labels: { code: "Code", title: "Indicator", target: "Target", current: "Current", status: "Status" }
  },
  "Tenant Administration": {
    title: "Tenant Administration",
    description: "Usuarios, roles, permisos y configuración del tenant.",
    steps: ["Users", "Roles", "Permissions", "Settings"],
    columns: ["code", "title", "status", "role"],
    labels: { code: "User", title: "Email", status: "Status", role: "Role" },
    route: "tenant-administration"
  },
  "Security": {
    title: "Security",
    description: "Controles de seguridad, revisiones de acceso y evidencias.",
    steps: ["Review", "Harden", "Monitor", "Audit"],
    columns: ["code", "title", "status", "dueDate"],
    labels: { code: "Code", title: "Control", status: "Status", dueDate: "Due" },
    route: "security"
  },
  "Configuration": {
    title: "Configuration",
    description: "Proveedores de storage, notificaciones y readiness productivo.",
    steps: ["Storage", "Email", "Test", "Activate"],
    columns: ["code", "title", "status", "provider"],
    labels: { code: "Code", title: "Provider", status: "Status", provider: "Type" },
    route: "configuration"
  }
};

const WORKSPACE_TILES = [
  ["Document Management", "Document Control", "Versiones, vigencias y aprobaciones"],
  ["Technical Sheets", "Technical Sheets", "Productos, ingredientes y certificaciones"],
  ["Suppliers", "Suppliers", "Homologación y evaluaciones"],
  ["CAPA", "CAPA", "Acciones, causa raíz y eficacia"],
  ["Risks", "Risk Matrix", "Controles, tratamientos y heat map"],
  ["Audits", "Audits", "Programas, planes y hallazgos"],
  ["Reports", "Report Center", "Ejecución, export y schedules"],
  ["Indicators", "Quality KPIs", "Metas, umbrales y tendencias"]
];

const moduleDeps = {
  "Document Management": ["Approval Workflows", "Notifications", "Reports"],
  "Audits": ["CAPA", "Risks", "Reports"],
  "CAPA": ["Audits", "Indicators", "Reports"],
  "Risks": ["Indicators", "Reports", "Dashboard"],
  "Suppliers": ["Technical Sheets", "Documents", "Reports"],
  "Tenant Administration": ["Users", "Roles", "Permissions", "Settings"]
};

const expl = {
  wizard: "Recorrido guiado del producto: valor, arquitectura, multi-tenant, login enterprise y rol.",
  modules: "Simulación visual de módulos con formularios, grids, filtros, dependencias y acciones comerciales.",
  roles: "Selección de rol para mostrar responsabilidades, límites SoD, KPIs y colaboración inter-áreas.",
  dashboard: "Vista ejecutiva simulada con alertas, tendencias y foco en cumplimiento.",
  wizards: "Automatización guiada de procesos críticos con validaciones por etapa.",
  reports: "Centro analítico con consolidación, filtros y exportación comercial simulada.",
  ia: "Asistente virtual simulado que explica objetivo, beneficios, entradas y salidas por pantalla.",
  favorites: "Atajos guardados para demos largas y presentaciones ejecutivas.",
  training: "Ruta de formación por rol con lenguaje funcional, qué hace cada pantalla y cómo llenar cada input.",
  certification: "Evaluación guiada con checklist y quiz para validar adopción funcional por rol."
};

const ROLE_PATHS = {
  "Quality Manager": ["Dashboard", "Document Management", "CAPA", "Risks", "Reports"],
  "Document Controller": ["Dashboard", "Document Management", "Approval Workflows", "Reports"],
  "Auditor": ["Dashboard", "Audits", "CAPA", "Reports"],
  "CAPA Manager": ["Dashboard", "CAPA", "Risks", "Indicators", "Reports"],
  "Risk Manager": ["Dashboard", "Risks", "Indicators", "Reports"],
  "Reporting Manager": ["Dashboard", "Reports", "Indicators", "Document Management"],
  "Tenant Admin": ["Dashboard", "Tenant Administration", "Users", "Roles", "Permissions", "Settings"],
  "Viewer": ["Dashboard", "Document Management", "CAPA", "Risks", "Reports"]
};

const SCREEN_GUIDES = {
  "Document Management": {
    purpose: "Permite crear, clasificar, revisar y publicar documentos controlados.",
    usedBy: "Document Controller (principal), Quality Manager (aprobación), Viewer (consulta).",
    when: "Cuando se crea o actualiza un procedimiento, instructivo o formato.",
    next: "Pasa a revisión/aprobación y luego a publicación controlada."
  },
  "CAPA": {
    purpose: "Gestiona acciones correctivas y preventivas con trazabilidad completa.",
    usedBy: "CAPA Manager y Quality Manager.",
    when: "Después de un hallazgo, no conformidad o desviación.",
    next: "Seguimiento de eficacia y cierre formal."
  },
  "Risks": {
    purpose: "Registra riesgos operativos, evalúa impacto/probabilidad y plan de tratamiento.",
    usedBy: "Risk Manager y Quality Manager.",
    when: "Durante evaluación periódica o ante cambios de proceso.",
    next: "Monitoreo en dashboard e indicadores."
  },
  "Audits": {
    purpose: "Planifica auditorías, registra hallazgos y genera trazabilidad para acciones correctivas.",
    usedBy: "Auditor, Quality Manager.",
    when: "Ciclos de auditoría interna, cliente o certificación.",
    next: "Hallazgos conectan a CAPA y reportes ejecutivos."
  },
  "Technical Sheets": {
    purpose: "Documenta especificaciones técnicas de producto con trazabilidad y aprobación.",
    usedBy: "Tenant Admin (con permiso CREATE), Quality Manager (aprobación), Viewer (consulta).",
    when: "Al lanzar o actualizar un producto alimenticio o industrial.",
    next: "Aprobación por Quality Manager y publicación controlada."
  },
  "Suppliers": {
    purpose: "Gestiona expediente, evaluaciones y homologación de proveedores.",
    usedBy: "Supplier Manager y Quality Manager.",
    when: "Onboarding o reevaluación periódica de proveedores.",
    next: "Homologación y monitoreo en reportes."
  },
  "Reports": {
    purpose: "Consolida información transversal para gerencia, comité y auditoría externa.",
    usedBy: "Reporting Manager, Tenant Admin, Quality Manager.",
    when: "Cierres semanales/mensuales o revisiones ejecutivas.",
    next: "Exportación y distribución para toma de decisiones."
  },
  "Security": {
    purpose: "Administra controles de seguridad, MFA y revisiones de acceso.",
    usedBy: "Tenant Security Administrator.",
    when: "Hardening, auditorías de seguridad o incidentes.",
    next: "Evidencias en audit trail."
  },
  "Configuration": {
    purpose: "Configura storage, notificaciones e integraciones del tenant.",
    usedBy: "Storage Admin, Notification Admin, Tenant Admin.",
    when: "Onboarding técnico o cambios de infraestructura.",
    next: "Pruebas de conectividad y activación productiva."
  },
  "Tenant Administration": {
    purpose: "Administra usuarios, roles, permisos y configuración del tenant.",
    usedBy: "Tenant Admin.",
    when: "Onboarding, cambios organizacionales y revisiones de seguridad.",
    next: "Impacta todo el acceso funcional por RBAC."
  },
  "Indicators": {
    purpose: "Define KPIs, metas y alertas para medir desempeño de cumplimiento.",
    usedBy: "Indicators Manager, Risk Manager, Reporting Manager.",
    when: "Seguimiento periódico de objetivos y mejora continua.",
    next: "Alimenta dashboard y reportes ejecutivos."
  }
};

const INPUT_DICTIONARY = {
  "Document Management": [
    { input: "Code", funcional: "Identificador único del documento", ejemplo: "BPM-LIM-001", regla: "No repetir códigos; formato corporativo." },
    { input: "Title", funcional: "Nombre claro del documento", ejemplo: "Limpieza de línea de empaque", regla: "Debe describir el proceso sin ambigüedad." },
    { input: "Category", funcional: "Clasificación funcional", ejemplo: "POE / Instructivo", regla: "Afecta reportes y flujos de aprobación." },
    { input: "Owner", funcional: "Responsable del contenido", ejemplo: "Document Controller", regla: "Debe ser un rol habilitado para mantenimiento." }
  ],
  "CAPA": [
    { input: "Issue", funcional: "Problema detectado", ejemplo: "Desviación en temperatura CCP", regla: "Debe vincularse a evidencia o hallazgo." },
    { input: "Root Cause", funcional: "Causa raíz", ejemplo: "Falla en calibración de sensor", regla: "Basada en análisis (5 Whys/Ishikawa)." },
    { input: "Action Owner", funcional: "Responsable de ejecución", ejemplo: "Jefe de Producción", regla: "Con fecha objetivo y seguimiento." },
    { input: "Due Date", funcional: "Fecha de cumplimiento", ejemplo: "2026-08-15", regla: "Debe respetar SLA interno." }
  ],
  "Risks": [
    { input: "Risk Name", funcional: "Nombre del riesgo", ejemplo: "Contaminación cruzada", regla: "Debe reflejar evento real y entendible." },
    { input: "Probability", funcional: "Probabilidad de ocurrencia", ejemplo: "Media", regla: "Consistente con matriz corporativa." },
    { input: "Impact", funcional: "Impacto en negocio/calidad", ejemplo: "Alto", regla: "Alineado a criterios de severidad." },
    { input: "Mitigation Plan", funcional: "Plan de tratamiento", ejemplo: "Barreras sanitarias + verificación diaria", regla: "Accionable y medible." }
  ],
  "Audits": [
    { input: "Audit Code", funcional: "Código único de auditoría", ejemplo: "AUD-INT-2026-01", regla: "No duplicar y mantener nomenclatura." },
    { input: "Scope", funcional: "Alcance auditado", ejemplo: "Línea de empaque 1", regla: "Definir procesos/áreas incluidas." },
    { input: "Auditor", funcional: "Responsable de ejecución", ejemplo: "Auditor Interno", regla: "Mantener independencia funcional." },
    { input: "Finding", funcional: "Hallazgo detectado", ejemplo: "Registro incompleto de limpieza", regla: "Debe incluir evidencia objetiva." }
  ],
  "Reports": [
    { input: "Report Type", funcional: "Tipo de reporte", ejemplo: "CAPA abiertas", regla: "Alineado con objetivo del comité." },
    { input: "Date Range", funcional: "Periodo de análisis", ejemplo: "Últimos 30 días", regla: "Definir desde/hasta consistente." },
    { input: "Audience", funcional: "Audiencia destinataria", ejemplo: "Gerencia de Calidad", regla: "Ajustar nivel de detalle." },
    { input: "Export Format", funcional: "Formato de salida", ejemplo: "PDF", regla: "Seleccionar según uso final." }
  ],
  "Tenant Administration": [
    { input: "User Email", funcional: "Correo corporativo del usuario", ejemplo: "quality@empresa.com", regla: "Debe ser único en el tenant." },
    { input: "Role Assignment", funcional: "Rol asignado", ejemplo: "Quality Manager", regla: "Respetar segregación de funciones." },
    { input: "Status", funcional: "Estado de acceso", ejemplo: "Activo", regla: "Desactivar cuando ya no aplica acceso." },
    { input: "Permission Scope", funcional: "Alcance de permisos", ejemplo: "Read/Approve", regla: "Aplicar mínimo privilegio." }
  ],
  "Indicators": [
    { input: "Indicator Code", funcional: "Código del KPI", ejemplo: "KPI-NC-RATE", regla: "Único por indicador." },
    { input: "Target", funcional: "Meta esperada", ejemplo: "<= 1.0%", regla: "Debe ser medible y realista." },
    { input: "Threshold", funcional: "Umbral de alerta", ejemplo: "1.2%", regla: "Define disparo de alertas." },
    { input: "Frequency", funcional: "Periodicidad de medición", ejemplo: "Semanal", regla: "Consistencia para comparabilidad." }
  ],
  "Technical Sheets": [
    { input: "Nombre / titulo", funcional: "Nombre de la ficha técnica", ejemplo: "Ficha Tecnica Yogurt Natural", regla: "Debe identificar el producto." },
    { input: "Codigo", funcional: "SKU o código corporativo", ejemplo: "TS-MFT-001", regla: "Único por producto." },
    { input: "Descripcion producto", funcional: "Descripción del producto", ejemplo: "Producto lácteo pasteurizado", regla: "Alineada a etiquetado." }
  ],
  "Suppliers": [
    { input: "Legal Name", funcional: "Razón social", ejemplo: "Proveedor Alimentos S.A.", regla: "Coincide con documento fiscal." },
    { input: "Tax Identifier", funcional: "RUC/NIT", ejemplo: "155612345-2-2020", regla: "Único en el tenant." },
    { input: "Country", funcional: "País de origen", ejemplo: "PA", regla: "Código ISO." }
  ]
};

const state = {
  theme: localStorage.getItem("c360.demo.theme") || "light",
  route: "wizard",
  wizardStep: 1,
  role: null,
  activeModule: null,
  moduleSearch: "",
  presenting: false,
  presentTimer: null,
  presentIdx: 0,
  search: "",
  favorites: JSON.parse(localStorage.getItem("c360.demo.favorites") || "[]"),
  certification: JSON.parse(localStorage.getItem("c360.demo.certification") || "{}"),
  certResult: null,
  data: FALLBACK_DATA
};

const CERT_CHECKLIST = {
  "Quality Manager": [
    "Interpretar alertas del Dashboard",
    "Explicar objetivo de Document Management",
    "Completar 2 inputs de CAPA correctamente",
    "Explicar cómo prioriza riesgos",
    "Exportar un reporte ejecutivo simulado"
  ],
  "Document Controller": [
    "Crear documento con código único",
    "Seleccionar categoría funcional correcta",
    "Explicar flujo de aprobación",
    "Interpretar estado de publicación",
    "Generar salida de reporte documental"
  ],
  "Auditor": [
    "Definir alcance de auditoría",
    "Registrar hallazgo funcional",
    "Conectar hallazgo a CAPA",
    "Explicar evidencia requerida",
    "Presentar estado de auditoría en reporte"
  ],
  "CAPA Manager": [
    "Registrar problema y causa raíz",
    "Definir responsable de acción",
    "Asignar fecha objetivo",
    "Explicar cierre por eficacia",
    "Mostrar trazabilidad en reporte"
  ],
  "Risk Manager": [
    "Registrar riesgo operativo",
    "Asignar probabilidad/impacto",
    "Definir mitigación accionable",
    "Relacionar riesgo con KPI",
    "Mostrar impacto en dashboard"
  ]
};

const CERT_QUIZ = {
  "Quality Manager": [
    { q: "¿Para qué sirve CAPA?", options: ["Registrar acciones correctivas", "Crear usuarios", "Configurar storage"], answer: 0 },
    { q: "¿Qué busca Risks?", options: ["Diseño UI", "Control de riesgos operativos", "Cambiar branding"], answer: 1 },
    { q: "¿Qué salida clave genera Reports?", options: ["Exportables ejecutivos", "Reset de contraseña", "Nueva base de datos"], answer: 0 }
  ],
  "Document Controller": [
    { q: "¿Qué representa Code?", options: ["ID visual único", "Color del dashboard", "Tipo de usuario"], answer: 0 },
    { q: "Category impacta...", options: ["Flujo y reportes", "Solo el logo", "Nada funcional"], answer: 0 },
    { q: "Después de crear documento sigue...", options: ["Aprobación/publicación", "Eliminar tenant", "Cerrar sesión"], answer: 0 }
  ]
};

function $(id) { return document.getElementById(id); }
function initials(v) { return String(v).split(" ").map(w => w[0]).join("").slice(0, 2).toUpperCase(); }
function setTheme() {
  document.documentElement.dataset.theme = state.theme;
  localStorage.setItem("c360.demo.theme", state.theme);
  $("btnTheme").textContent = state.theme === "dark" ? "☀️" : "🌙";
}
function kpi(label, value) {
  return `<article class="card metric-card"><div class="metric-label">${label}</div><div class="metric-value pulse">${value}</div></article>`;
}
function metric(label, value, help) {
  return `<article class="card metric-card"><div class="metric-label">${label}</div><div class="metric-value">${value}</div><div class="metric-label">${help}</div></article>`;
}
function pageHeader(title, description, breadcrumb) {
  return `
    <div class="breadcrumbs">Compliance 360 / ${breadcrumb}</div>
    <div class="page-header">
      <div class="page-title">
        <span class="product-badge">${currentMeta().group}</span>
        <h1>${title}</h1>
        <p>${description}</p>
      </div>
      <div class="button-row">
        <button class="btn subtle" type="button" data-route="dashboard">Dashboard</button>
        <button class="btn subtle" type="button" data-route="reports">Reportes</button>
        <button class="btn subtle" type="button" data-action="reload">Recargar</button>
      </div>
    </div>`;
}
function tableCard(title, rows, columns, labels, showActions = true) {
  if (!rows.length) {
    return `<section class="card"><div class="section-heading"><div><h2 class="section-title">${title}</h2><p class="metric-label">Datos simulados tenant-scoped.</p></div><span class="status-pill warn">0 registros</span></div><div class="empty-state"><strong>No hay registros todavía.</strong><p>Usa el Action Center para crear el primer item.</p></div></section>`;
  }
  const heads = columns.map(c => labels?.[c] || c);
  const actionCol = showActions ? "<th>Acción</th>" : "";
  const actionCells = showActions ? `<td><button class="btn subtle" type="button">Editar</button></td>` : "";
  return `<section class="card">
    <div class="section-heading"><div><h2 class="section-title">${title}</h2><p class="metric-label">${rows.length} registros visibles en esta vista.</p></div><span class="status-pill ok">Demo data</span></div>
    <div class="table-wrap"><table class="table"><thead><tr>${heads.map(h => `<th>${h}</th>`).join("")}${actionCol}</tr></thead><tbody>
    ${rows.map(row => `<tr>${columns.map(c => `<td>${row[c] ?? "n/a"}</td>`).join("")}${showActions ? actionCells : ""}</tr>`).join("")}
    </tbody></table></div></section>`;
}
function productionHero(d) {
  return `<section class="hero-card dashboard-hero">
    <div>
      <span class="product-badge">Compliance 360 Enterprise Edition</span>
      <h1>Centro de comando para cumplimiento, calidad y riesgo</h1>
      <p>Aplicación multitenant simulada: documentos, auditorías, CAPA, riesgos, KPIs, reportes y trazabilidad en un mismo workspace.</p>
      <div class="hero-actions">
        <button class="btn light" type="button" data-route="reports">Abrir Report Center</button>
        <button class="btn light" type="button" data-open-module="Document Management">Crear evidencia</button>
        <button class="btn light" type="button" data-open-module="Risks">Revisar matriz de riesgo</button>
      </div>
    </div>
    <div class="command-panel">
      <div class="command-row"><span>Audit open</span><strong>${d.auditOpen ?? 0}</strong></div>
      <div class="command-row"><span>CAPA open</span><strong>${d.capaOpen ?? 0}</strong></div>
      <div class="command-row"><span>Critical risk</span><strong>${d.criticalRisk ?? 0}</strong></div>
      <div class="command-row"><span>Indicators</span><strong>${d.indicators ?? 0}</strong></div>
      <div class="command-row"><span>Report datasets</span><strong>${d.datasets ?? 0}</strong></div>
    </div>
  </section>`;
}
function moduleExperiencePanel(name, total) {
  const cfg = MODULE_CONFIG[name] || { title: name, description: "Módulo operativo simulado.", steps: ["Create", "Manage", "Audit", "Report"] };
  return `<section class="hero-card compact module-hero">
    <div><span class="product-badge">Live module</span><h2>${cfg.title}</h2><p>${cfg.description} Pantalla simulada alineada al producto real.</p></div>
    <div class="workflow-strip">${cfg.steps.map(s => `<span>${s}</span>`).join("")}</div>
    <div class="module-count"><strong>${total}</strong><span>records</span></div>
  </section>`;
}
function moduleTilesHtml() {
  return `<section class="card"><div class="section-heading"><div><h2 class="section-title">Enterprise Workspaces</h2><p class="metric-label">Acceso visual a los dominios productivos principales.</p></div></div>
    <div class="workspace-grid">${WORKSPACE_TILES.map(([mod, title, desc]) => `
      <button class="workspace-tile" type="button" data-open-module="${mod}">
        <span class="workspace-icon">${title.slice(0, 2).toUpperCase()}</span><strong>${title}</strong><small>${desc}</small>
      </button>`).join("")}</div></section>`;
}
function heatMapView() {
  const cells = [];
  for (let impact = 5; impact >= 1; impact--) {
    for (let prob = 1; prob <= 5; prob++) {
      const score = prob * impact;
      const level = score >= 16 ? "hot" : score >= 8 ? "warn" : "ok";
      const count = score === 12 || score === 15 ? 2 : score === 20 ? 1 : "";
      cells.push(`<div class="heat-cell ${level}">${count}</div>`);
    }
  }
  return `<div class="heat-map">${cells.join("")}</div>`;
}
function toast(msg, kind = "info") {
  const region = $("toast-region");
  if (!region) return;
  const el = document.createElement("div");
  el.className = `toast ${kind === "success" || kind === "error" ? kind : ""}`;
  el.textContent = msg;
  region.appendChild(el);
  setTimeout(() => el.remove(), 3200);
}
function saveFavorites() { localStorage.setItem("c360.demo.favorites", JSON.stringify(state.favorites)); }
function saveCertification() { localStorage.setItem("c360.demo.certification", JSON.stringify(state.certification)); }
function isFavorite(route) { return state.favorites.includes(route); }
function toggleFavorite(route) {
  if (isFavorite(route)) state.favorites = state.favorites.filter(r => r !== route);
  else state.favorites.push(route);
  saveFavorites();
  render();
}

async function loadData() {
  try {
    const resp = await fetch("./assets/data/mock-data.json", { cache: "no-cache" });
    if (resp.ok) state.data = await resp.json();
  } catch (_e) {
    state.data = FALLBACK_DATA;
  }
}


function renderNav() {
  const nav = $("mainNav");
  const q = state.search.toLowerCase().trim();
  const allowed = state.role && ROLE_NAV[state.role] ? new Set(ROLE_NAV[state.role]) : null;
  nav.innerHTML = navigation.map(group => {
    const items = group.items.filter(([key, title]) => {
      if (group.group === "Demo Enablement" && key === "favorites" && !state.favorites.length) return false;
      if (allowed && !["Demo Enablement"].includes(group.group) && !allowed.has(key)) return false;
      return title.toLowerCase().includes(q) || group.group.toLowerCase().includes(q);
    });
    if (!items.length) return "";
    return `<nav class="nav-group" aria-label="${group.group}">
      <div class="nav-label">${group.group}</div>
      ${items.map(([key, title]) => `
        <div class="nav-row">
          <button class="nav-button ${state.route === key && !state.activeModule ? "active" : ""}" data-route="${key}" title="${title}">
            <span class="nav-icon">${initials(title)}</span><span>${title}</span><span aria-hidden="true">›</span>
          </button>
          <button class="fav-btn" title="Favorito" data-fav="${key}">${isFavorite(key) ? "★" : "☆"}</button>
        </div>`).join("")}
    </nav>`;
  }).join("");
  nav.querySelectorAll("[data-route]").forEach(b => b.onclick = () => navigateRoute(b.dataset.route));
  nav.querySelectorAll("[data-fav]").forEach(b => b.onclick = () => toggleFavorite(b.dataset.fav));
}

function navigateRoute(route) {
  state.route = route;
  state.activeModule = ROUTE_MODULE_MAP[route] || null;
  state.moduleSearch = "";
  render();
}

function currentMeta() { return routeMeta.find(r => r.key === state.route) || { title: state.route, group: "General", process: "Flujo funcional" }; }

function renderWizard() {
  if (state.wizardStep === 4) return renderLoginDemo();
  const steps = {
    1: "¿Qué es Compliance 360? Beneficios, usuarios, empresas, casos de uso y dolores que resuelve.",
    2: "Arquitectura general: Usuarios -> Roles -> Permisos -> Módulos -> Procesos -> Auditoría -> Reportes.",
    3: "MultiTenant: cada empresa tiene su espacio, gobierno central y operación aislada.",
    5: "Seleccionar rol y activar recorrido completo de responsabilidades, límites y KPIs."
  };
  return `<section class="panel">
    <h3>Wizard Principal · Paso ${state.wizardStep}/5</h3>
    <p>${steps[state.wizardStep]}</p>
    <div class="flow-chain">Usuarios ➜ Roles ➜ Permisos ➜ Módulos ➜ Procesos ➜ Auditoría ➜ Reportes</div>
    <div class="toolbar">
      <button class="btn secondary" id="wizPrev">Anterior</button>
      <button class="btn primary" id="wizNext">Siguiente</button>
    </div>
  </section>`;
}

function renderLoginDemo() {
  return `<section class="login-page panel-login">
    <section class="login-panel">
      <div class="brand-line"><div class="brand-mark">C360</div><span class="product-badge">Enterprise Login</span></div>
      <h1>Compliance 360 Enterprise</h1>
      <p>Ingresa tu correo corporativo. Sin GUID de tenant visible.</p>
      <form class="form-stack" id="demoLoginForm">
        <div class="field"><label>Correo electrónico</label><input type="email" value="quality@empresa.com" /></div>
        <button class="btn primary" type="submit">Siguiente</button>
      </form>
    </section>
    <section class="login-hero">
      <div class="hero-card compact">
        <span class="product-badge">Tenant transparente</span>
        <h2>Login Enterprise sin GUIDs técnicos</h2>
        <p>Email ➜ Organización ➜ Password ➜ MFA ➜ Dashboard</p>
        <div class="hero-strip"><span>Multi-tenant</span><span>RBAC</span><span>Audit-ready</span></div>
      </div>
    </section>
    <div class="toolbar wizard-nav">
      <button class="btn secondary" id="wizPrev">Anterior</button>
      <button class="btn primary" id="wizNext">Siguiente</button>
    </div>
  </section>`;
}

function renderRoles() {
  return `<section class="panel"><h3>Seleccionar Rol</h3><div class="grid cols-3">
    ${state.data.roles.map(r => `<button class="card role-card ${state.role === r.name ? "active" : ""}" data-role="${r.name}" title="${r.summary}">
      <strong>${r.name}</strong><small>${r.level}</small><p>${r.summary}</p></button>`).join("")}
  </div></section>`;
}

function renderCompliance() {
  const d = state.data.dashboard || {};
  return `${pageHeader("Compliance Dashboard", "Vista consolidada de cumplimiento normativo y desviaciones.", "Command Center")}
    <section class="grid cards">
      ${metric("Compliance %", (d.compliancePercent ?? 94) + "%", "Índice global")}
      ${metric("Open CAPA", d.capaOpen ?? 12, "Acciones abiertas")}
      ${metric("Critical Risks", d.criticalRisk ?? 3, "Riesgos críticos")}
      ${metric("Audit Findings", d.auditOpen ?? 4, "Hallazgos abiertos")}
    </section>
    <section class="grid two">
      <div class="card"><h2 class="section-title">Obligaciones regulatorias</h2><p class="metric-label">ISO 9001, ISO 22000, BPM y trazabilidad documental simulada.</p></div>
      <div class="card"><h2 class="section-title">Alertas activas</h2><p class="metric-label">2 documentos por vencer · 1 proveedor en reevaluación · 1 CAPA vencida.</p></div>
    </section>`;
}

function renderAuditTrail() {
  const rows = [
    { action: "TechnicalSheetCreated", entity: "Technical Sheet", user: "tenantadmin@empresa.com", when: "2026-07-09 19:40" },
    { action: "DocumentApproved", entity: "BPM-LIM-001", user: "quality@empresa.com", when: "2026-07-09 18:22" },
    { action: "CapaCreated", entity: "CAPA-2026-014", user: "capa@empresa.com", when: "2026-07-09 17:05" },
    { action: "RbacPermissionGranted", entity: "TECHNICALSHEET.CREATE", user: "tenantadmin@empresa.com", when: "2026-07-09 16:58" }
  ];
  return `${pageHeader("Audit Trail", "Trazabilidad de acciones críticas del tenant.", "Command Center")}
    ${tableCard("Eventos recientes", rows, ["action", "entity", "user", "when"], { action: "Action", entity: "Entity", user: "User", when: "When" })}
    <div class="button-row"><button class="btn primary" type="button" onclick="toast('Exportación CSV simulada.', 'success')">Exportar</button></div>`;
}

function renderEnterpriseWorkspace(name) {
  const cfg = MODULE_CONFIG[name] || { title: name, description: "Workspace enterprise simulado.", steps: ["Configure", "Review", "Activate"] };
  const records = state.data.records?.[name] || [
    { code: "CFG-001", title: name + " item", status: "Active", provider: "Local" }
  ];
  return renderModuleDetail(name, records, cfg);
}

function renderDashboard() {
  const d = state.data.dashboard || { auditOpen: 4, capaOpen: 12, criticalRisk: 3, indicators: 18, datasets: 9, compliancePercent: 94 };
  const roleText = state.role || "Rol no seleccionado";
  return `
    ${pageHeader("Executive Dashboard", `Vista ejecutiva simulada para ${roleText}.`, "Command Center")}
    ${productionHero(d)}
    <section class="grid cards">
      ${metric("API Health", "Healthy", "Estado de servicio")}
      ${metric("Audit Open", d.auditOpen, "Auditorías abiertas")}
      ${metric("CAPA Open", d.capaOpen, "Acciones abiertas")}
      ${metric("Risk Critical", d.criticalRisk, "Riesgos críticos")}
    </section>
    <section class="grid two">
      <div class="card">
        <h2 class="section-title">Compliance Performance</h2>
        <p class="metric-label">Indicadores de cumplimiento y desviaciones.</p>
        <progress class="progress" value="${d.compliancePercent}" max="100"></progress>
        <div class="button-row section-actions">
          <span class="status-pill">Indicators: ${d.indicators}</span>
          <span class="status-pill warn">Alerts: 2</span>
        </div>
      </div>
      <div class="card">
        <h2 class="section-title">Dashboard Datasets</h2>
        <div class="metric-value">${d.datasets}</div>
        <div class="metric-label">Datasets reutilizables para reportes ejecutivos.</div>
      </div>
    </section>
    <section class="grid two">
      <div class="card"><h2 class="section-title">Risk Heat Map</h2>${heatMapView()}</div>
      <div class="card">
        <h2 class="section-title">Quick Actions</h2>
        <div class="button-row">
          <button class="btn primary" type="button" data-route="reports">Ejecutar reporte</button>
          <button class="btn subtle" type="button" data-open-module="Document Management">Documentos</button>
          <button class="btn subtle" type="button" data-open-module="CAPA">Abrir CAPA</button>
          <button class="btn subtle" type="button" data-open-module="Risks">Ver riesgos</button>
        </div>
      </div>
    </section>
    ${moduleTilesHtml()}
    <section class="grid three">
      <article class="card readiness-card"><span class="status-pill ok">Ready</span><h3>Security</h3><p>JWT, RBAC, tenant enforcement y auditoría simulada.</p></article>
      <article class="card readiness-card"><span class="status-pill ok">Ready</span><h3>Data</h3><p>Datos mock tenant-scoped para entrenamiento y demo.</p></article>
      <article class="card readiness-card"><span class="status-pill ok">Ready</span><h3>Operations</h3><p>Módulos con flujos create, search, report y dashboard.</p></article>
    </section>`;
}

function renderModules() {
  return `${pageHeader("Enterprise Workspaces", "Acceso visual a dominios productivos simulados.", "Operations")}
    ${moduleTilesHtml()}
    <section class="card">
      <h2 class="section-title">Todos los módulos</h2>
      <div class="toolbar"><input id="moduleFilter" class="search-box" placeholder="Filtrar módulos..." /></div>
      <div id="moduleGrid" class="workspace-grid">${state.data.modules.map(m => `
        <button class="workspace-tile" type="button" data-open-module="${m}">
          <span class="workspace-icon">${initials(m)}</span><strong>${m}</strong><small>Pantalla interactiva simulada</small>
        </button>`).join("")}
      </div>
    </section>`;
}

function renderTraining() {
  const role = state.role || "Quality Manager";
  const path = ROLE_PATHS[role] || ROLE_PATHS["Quality Manager"];
  return `<section class="panel">
    <h3>Entrenamiento por Rol · ${role}</h3>
    <p>Ruta recomendada para familiarizarse con la herramienta en lenguaje funcional.</p>
    <div class="flow-chain">${path.join(" ➜ ")}</div>
    <div class="grid cols-2">
      ${path.map((screen, idx) => `<article class="card">
        <strong>Paso ${idx + 1}: ${screen}</strong>
        <p>${SCREEN_GUIDES[screen]?.purpose || "Pantalla operativa para ejecutar tareas del rol."}</p>
        <button class="btn secondary" data-open-module="${screen}">Practicar pantalla</button>
      </article>`).join("")}
    </div>
    <div class="card training-note">
      <strong>Resultado esperado del entrenamiento</strong>
      <p>Al finalizar, la persona debe explicar qué hace cada pantalla, qué llenar en cada input y cuál es el siguiente paso del proceso.</p>
    </div>
  </section>`;
}

function renderModuleDetail(name, recordsOverride, cfgOverride) {
  const cfg = cfgOverride || MODULE_CONFIG[name];
  const records = recordsOverride || (state.data.records?.[name] || []).filter(r =>
    !state.moduleSearch || Object.values(r).some(v => String(v).toLowerCase().includes(state.moduleSearch.toLowerCase()))
  );
  const deps = moduleDeps[name] || ["Dashboard", "Reports"];
  const guide = SCREEN_GUIDES[name];
  const dict = INPUT_DICTIONARY[name] || [];
  const cols = cfg?.columns || ["code", "title", "status"];
  const labels = cfg?.labels || {};
  const readOnly = cfg?.readOnlyRoles?.includes(state.role);
  const actionFields = readOnly ? "" : dict.slice(0, 3).map(d =>
    `<div class="field"><label>${d.input}</label><input placeholder="${d.ejemplo}" /></div>`
  ).join("");
  return `
    ${pageHeader(cfg?.title || name, cfg?.description || "Módulo operativo simulado.", cfg?.route ? "Operations" : "Enterprise")}
    ${moduleExperiencePanel(name, records.length)}
    <section class="grid cards">
      ${metric("Records", records.length, "Resultados tenant-scoped")}
      ${metric("Page", "1", "Página actual")}
      ${metric("Page Size", "10", "Tamaño de página")}
      ${metric("Mode", readOnly ? "Read-only" : "Manage", readOnly ? "Sin crear registro" : "Action Center activo")}
    </section>
    ${readOnly ? `<section class="card"><span class="status-pill warn">Modo solo lectura</span><p class="metric-label">Este rol no tiene permiso de creación en este módulo (SoD/RBAC simulado).</p></section>` : `
    <section class="card">
      <h2 class="section-title">Action Center</h2>
      <form class="form-stack" id="module-action-form">
        <div class="grid two">${actionFields || `<div class="field"><label>Title</label><input id="name" placeholder="Nuevo registro" /></div><div class="field"><label>Code</label><input id="code" placeholder="COD-001" /></div>`}</div>
        <div class="button-row"><button class="btn primary" type="submit">Crear registro real</button><button class="btn subtle" type="button" data-map>Dependencias</button></div>
      </form>
    </section>`}
    <section class="card">
      <div class="button-row">
        <input id="moduleSearchInput" class="search-box" placeholder="Filtro avanzado por texto" value="${state.moduleSearch}" />
        <button class="btn" id="moduleSearchBtn" type="button">Buscar</button>
        <button class="btn subtle" type="button" data-route="reports">Exportar via reportes</button>
        <button class="btn subtle" type="button" id="backToModules" data-route="modules">← Volver</button>
      </div>
    </section>
    ${tableCard(cfg?.title || name, records, cols, labels)}
    <div class="deps-line"><strong>Dependencias:</strong> ${deps.join(" ➜ ")}</div>
    ${guide ? `<div class="card training-note">
      <strong>¿Qué hace esta pantalla?</strong>
      <p><strong>Objetivo:</strong> ${guide.purpose}</p>
      <p><strong>Quién la usa:</strong> ${guide.usedBy}</p>
      <p><strong>Cuándo se usa:</strong> ${guide.when}</p>
      <p><strong>Qué sigue:</strong> ${guide.next}</p>
    </div>` : ""}
    ${dict.length ? `<div class="card"><strong>Diccionario funcional de inputs</strong>
      ${tableCard("Inputs", dict.map(d => ({ input: d.input, funcional: d.funcional, ejemplo: d.ejemplo, regla: d.regla })), ["input", "funcional", "ejemplo", "regla"], { input: "Input", funcional: "¿Qué significa?", ejemplo: "Ejemplo", regla: "Regla funcional" }, false)}</div>` : ""}`;
}

function renderWizards() {
  const ws = ["Documento","Auditoría","Proveedor","CAPA","Riesgo","Ficha Técnica","Usuarios","Roles","Tenant"];
  return `<section class="panel"><h3>Wizards de Proceso</h3><div class="grid cols-3">
    ${ws.map(w => `<article class="card"><strong>${w}</strong><p>Paso 1 ➜ 2 ➜ 3 ➜ 4 ➜ 5 ➜ 6</p><button class="btn secondary" data-wizard="${w}">Ejecutar Wizard</button></article>`).join("")}
  </div></section>`;
}

function renderReports() {
  const rows = state.data.records?.Reports || [];
  return `${pageHeader("Report Center", "Busca, ejecuta, programa y exporta reportes empresariales.", "Reports")}
    <section class="grid cards">
      ${metric("Standard Reports", "12", "Catálogo obligatorio")}
      ${metric("Configured", rows.length, "Reportes activos")}
      ${metric("Datasets", state.data.dashboard?.datasets ?? 9, "Dashboard datasets")}
      ${metric("Formats", "5", "PDF, Excel, Word, CSV, JSON")}
    </section>
    <section class="card">
      <h2 class="section-title">Report Execution Console</h2>
      <div class="button-row">
        <select id="reportSelect" class="search-box">${rows.map(r => `<option>${r.title}</option>`).join("") || "<option>Sin reportes</option>"}</select>
        <button class="btn primary" id="executeReport" type="button">Ejecutar</button>
        <button class="btn" id="scheduleReport" type="button">Programar mensual</button>
      </div>
    </section>
    <section class="card">
      <h2 class="section-title">Exportación</h2>
      <div class="toolbar">
        <button class="btn secondary" data-export="Excel">Exportar Excel</button>
        <button class="btn secondary" data-export="PDF">Exportar PDF</button>
        <button class="btn secondary" data-export="Word">Exportar Word</button>
      </div>
      <div id="exportFeed" class="feed"></div>
    </section>
    ${tableCard("Reportes configurados", rows, ["code", "title", "status", "format"], MODULE_CONFIG.Reports.labels)}`;
}

function renderIA() {
  return `<section class="panel"><h3>Asistente IA (simulado)</h3>
    <p>Pregunta sugerida: "¿Qué hace esta pantalla y qué rol la usa?"</p>
    <div class="toolbar"><input id="iaInput" placeholder="Escribe tu pregunta..." /><button class="btn primary" id="iaAsk">Preguntar</button></div>
    <div id="iaOutput" class="card">Respuesta simulada: esta pantalla centraliza entradas, reglas y salidas del proceso para cumplimiento auditable.</div>
  </section>`;
}

function renderFavorites() {
  const favRoutes = routeMeta.filter(r => state.favorites.includes(r.key));
  return `<section class="panel"><h3>Favoritos</h3>
    ${favRoutes.length ? `<div class="grid cols-2">${favRoutes.map(r => `<button class="card btn secondary" data-route="${r.key}"><strong>${r.title}</strong><p>${r.group}</p></button>`).join("")}</div>` : "<p>No hay favoritos aún.</p>"}
  </section>`;
}

function renderCertification() {
  const role = state.role || "Quality Manager";
  const checklist = CERT_CHECKLIST[role] || CERT_CHECKLIST["Quality Manager"];
  const quiz = CERT_QUIZ[role] || CERT_QUIZ["Quality Manager"];
  const checked = state.certification[role] || [];
  const pct = Math.round((checked.length / checklist.length) * 100);
  return `<section class="panel">
    <h3>Certificación funcional · ${role}</h3>
    <p>Completa checklist y evaluación para validar dominio práctico del rol.</p>
    <div class="cert-progress"><strong>Progreso:</strong> ${checked.length}/${checklist.length} · ${pct}%</div>
    <div class="card">
      <strong>Checklist de desempeño</strong>
      <div class="checklist">
        ${checklist.map((item, i) => `<label class="${checked.includes(i) ? "done" : ""}">
          <input type="checkbox" data-cert-role="${role}" data-cert-idx="${i}" ${checked.includes(i) ? "checked" : ""}/>
          <span>${item}</span>
        </label>`).join("")}
      </div>
    </div>
    <div class="card">
      <strong>Evaluación final (3 preguntas)</strong>
      <div class="quiz-wrap">
        ${quiz.map((it, qi) => `<div class="quiz-item">
          <p><strong>${qi + 1}.</strong> ${it.q}</p>
          <div class="quiz-options">${it.options.map((op, oi) =>
            `<label><input type="radio" name="quiz_${qi}" value="${oi}" /> ${op}</label>`).join("")}
          </div>
        </div>`).join("")}
      </div>
      <div class="toolbar">
        <button class="btn primary" id="btnCertEvaluate">Evaluar</button>
        <button class="btn secondary" id="btnCertReset">Reiniciar</button>
      </div>
      <div id="certResult" class="feed">${state.certResult ? state.certResult : ""}</div>
    </div>
  </section>`;
}

function renderPresentationControls() {
  return `<div class="present-controls">
    <button class="btn secondary" id="pPrev" title="Pantalla anterior">⟨ Anterior</button>
    <button class="btn secondary" id="pPause" title="Pausar/Continuar">${state.presenting ? "Pausar" : "Continuar"}</button>
    <button class="btn secondary" id="pNext" title="Pantalla siguiente">Siguiente ⟩</button>
  </div>`;
}

function render() {
  setTheme();
  renderNav();
  const meta = state.activeModule
    ? { title: state.activeModule, group: "Operations", process: "Módulo operativo simulado" }
    : currentMeta();
  $("topbarContext").textContent = `${meta.group} / ${meta.title}`;
  $("topbarTitle").textContent = state.role ? `${state.role} · Compliance 360 Enterprise` : "Compliance 360 Enterprise";
  $("tenantChip").textContent = state.role ? `Tenant Demo · ${state.role}` : "Tenant Demo · Enterprise";
  $("sidebarRole").textContent = state.role || "Rol no seleccionado";
  $("breadcrumb").innerHTML = `Inicio / ${meta.group} / <strong>${meta.title}</strong> <span class="process-pill" title="Proceso funcional">${meta.process}</span>`;
  $("wizardBanner").classList.toggle("hidden", state.route !== "wizard");
  $("wizardBanner").textContent = `Recorrido guiado activo · Paso ${state.wizardStep}/5`;
  $("dashboardKpis").innerHTML = (state.route === "dashboard" || state.activeModule) ? "" : state.data.kpis.map(k => kpi(k.label, k.value)).join("");
  const switcher = $("quickSwitcher");
  if (switcher) {
    switcher.innerHTML = routeMeta.map(r => `<option value="${r.key}" ${state.route === r.key && !state.activeModule ? "selected" : ""}>${r.group} / ${r.title}</option>`).join("");
  }
  let html = "";
  if (state.activeModule) html = renderModuleDetail(state.activeModule);
  else if (state.route === "wizard") html += renderWizard();
  else if (state.route === "roles") html += renderRoles();
  else if (state.route === "training") html += renderTraining();
  else if (state.route === "certification") html += renderCertification();
  else if (state.route === "dashboard") html += renderDashboard();
  else if (state.route === "compliance") html += renderCompliance();
  else if (state.route === "audit-trail") html += renderAuditTrail();
  else if (state.route === "reports") html += renderReports();
  else if (state.route === "tenant-administration") html += renderEnterpriseWorkspace("Tenant Administration");
  else if (state.route === "security") html += renderEnterpriseWorkspace("Security");
  else if (state.route === "configuration") html += renderEnterpriseWorkspace("Configuration");
  else if (state.route === "ia") html += renderIA();
  else if (state.route === "favorites") html += renderFavorites();
  html += renderPresentationControls();
  $("viewHost").innerHTML = html;
  $("copilotText").textContent = state.activeModule
    ? `${state.activeModule}: pantalla simulada alineada al producto real con Action Center, métricas y tabla.`
    : (expl[state.route] || "Pantalla demo interactiva.");
  bindView();
}

function bindView() {
  $("btnTheme").onclick = () => { state.theme = state.theme === "dark" ? "light" : "dark"; render(); };
  $("btnWizardStart").onclick = () => { state.route = "wizard"; state.wizardStep = 1; state.activeModule = null; render(); };
  $("globalSearch").oninput = e => { state.search = e.target.value || ""; renderNav(); };
  $("quickSearch")?.addEventListener("keydown", e => {
    if (e.key === "Enter") {
      const q = e.target.value.toLowerCase();
      const mod = state.data.modules.find(m => m.toLowerCase().includes(q));
      if (mod) { openModule(mod); return; }
      const route = routeMeta.find(r => r.title.toLowerCase().includes(q));
      if (route) { navigateRoute(route.key); return; }
    }
  });
  $("quickSwitcher")?.addEventListener("change", e => navigateRoute(e.target.value));
  $("wizPrev")?.addEventListener("click", () => { state.wizardStep = Math.max(1, state.wizardStep - 1); render(); });
  $("wizNext")?.addEventListener("click", () => {
    state.wizardStep = Math.min(5, state.wizardStep + 1);
    if (state.wizardStep === 5) { state.route = "roles"; state.activeModule = null; }
    render();
  });
  $("demoLoginForm")?.addEventListener("submit", e => { e.preventDefault(); toast("Organización resuelta automáticamente.", "success"); });
  document.querySelectorAll("[data-role]").forEach(b => b.onclick = () => { state.role = b.dataset.role; state.route = "dashboard"; state.activeModule = null; render(); toast(`Rol activo: ${b.dataset.role}`, "success"); });
  document.querySelectorAll("[data-open-module]").forEach(b => b.onclick = () => openModule(b.dataset.openModule));
  $("moduleFilter")?.addEventListener("input", e => {
    const q = e.target.value.toLowerCase();
    $("moduleGrid")?.querySelectorAll(".workspace-tile").forEach(c => { c.style.display = c.textContent.toLowerCase().includes(q) ? "grid" : "none"; });
  });
  $("moduleSearchBtn")?.addEventListener("click", () => {
    state.moduleSearch = $("moduleSearchInput")?.value || "";
    render();
  });
  document.querySelector("#module-action-form, #moduleActionForm")?.addEventListener("submit", e => { e.preventDefault(); toast("Registro creado (simulado).", "success"); });
  $("executeReport")?.addEventListener("click", () => simulateExport("PDF"));
  $("scheduleReport")?.addEventListener("click", () => toast("Reporte programado mensualmente (simulado).", "success"));
  document.querySelectorAll("[data-wizard]").forEach(b => b.onclick = () => openWizardModal(b.dataset.wizard));
  $("btnWhat").onclick = () => openWhatModal();
  $("btnPresent").onclick = togglePresentation;
  $("btnTour").onclick = startTour;
  $("pPrev")?.addEventListener("click", () => movePresentation(-1));
  $("pNext")?.addEventListener("click", () => movePresentation(1));
  $("pPause")?.addEventListener("click", () => togglePresentation(true));
  document.querySelectorAll("[data-export]").forEach(b => b.onclick = () => simulateExport(b.dataset.export));
  $("iaAsk")?.addEventListener("click", askIA);
  document.querySelectorAll("[data-route]").forEach(b => b.onclick = () => navigateRoute(b.dataset.route));
  document.querySelectorAll("[data-action='reload']").forEach(b => b.onclick = () => { state.activeModule = null; render(); toast("Vista recargada.", "success"); });
  document.querySelector("[data-map]")?.addEventListener("click", () => openDependencyModal());
  document.querySelectorAll("[data-cert-role]").forEach(cb => {
    cb.addEventListener("change", e => {
      const role = e.target.dataset.certRole;
      const idx = Number(e.target.dataset.certIdx);
      const arr = state.certification[role] || [];
      if (e.target.checked && !arr.includes(idx)) arr.push(idx);
      if (!e.target.checked) state.certification[role] = arr.filter(x => x !== idx);
      else state.certification[role] = arr.sort((a, b) => a - b);
      saveCertification();
      render();
    });
  });
  $("btnCertEvaluate")?.addEventListener("click", evaluateCertification);
  $("btnCertReset")?.addEventListener("click", resetCertification);
}

function openModule(name) {
  const route = Object.entries(ROUTE_MODULE_MAP).find(([, mod]) => mod === name)?.[0];
  state.activeModule = name;
  state.moduleSearch = "";
  if (route) state.route = route;
  render();
}

function openWizardModal(name) {
  const modal = $("modal");
  modal.innerHTML = `<h3>Wizard ${name}</h3><p>Paso 1 Información -> Paso 2 Clasificación -> Paso 3 Adjuntos -> Paso 4 Revisión -> Paso 5 Aprobación -> Paso 6 Publicado.</p><div class="toolbar"><button class="btn secondary" id="mPrev">Anterior</button><button class="btn primary" id="mNext">Siguiente</button><button class="btn ghost" id="mClose">Cerrar</button></div>`;
  let step = 1;
  const t = () => modal.querySelector("p").textContent = `${name} · Paso ${step}/6 simulado con validaciones visuales, checklist y trazabilidad.`;
  modal.showModal();
  modal.querySelector("#mPrev").onclick = () => { step = Math.max(1, step - 1); t(); };
  modal.querySelector("#mNext").onclick = () => { step = Math.min(6, step + 1); t(); };
  modal.querySelector("#mClose").onclick = () => modal.close();
}

function openWhatModal() {
  const modal = $("modal");
  const meta = currentMeta();
  modal.innerHTML = `<h3>¿Qué hace esta pantalla?</h3>
  <p>${expl[state.route] || "Pantalla funcional de la plataforma."}</p>
  <ul><li><strong>Objetivo:</strong> ${meta.process}.</li><li><strong>Beneficio:</strong> trazabilidad y control operativo.</li><li><strong>Entradas:</strong> datos simulados de usuarios/procesos.</li><li><strong>Salidas:</strong> indicadores, reportes y estados.</li><li><strong>Usuarios:</strong> roles operativos y ejecutivos.</li></ul>
  <button class="btn primary" id="mOk">Entendido</button>`;
  modal.showModal();
  modal.querySelector("#mOk").onclick = () => modal.close();
}

function simulateExport(kind) {
  const host = $("exportFeed");
  if (!host) return;
  const time = new Date().toLocaleTimeString();
  host.innerHTML = `<div class="card">Exportación ${kind} generada exitosamente (${time}) · archivo simulado listo para descarga.</div>`;
}

function askIA() {
  const input = $("iaInput");
  const out = $("iaOutput");
  const q = (input?.value || "").trim();
  if (!out) return;
  if (!q) {
    out.textContent = "Haz una pregunta sobre proceso, rol, módulo o KPI.";
    return;
  }
  out.textContent = `Respuesta simulada: "${q}" se aborda en ${currentMeta().title}. Recomendación: iniciar en Dashboard, revisar dependencias de módulo y finalizar con reporte ejecutivo.`;
}

function openDependencyModal() {
  const modal = $("modal");
  const lines = Object.entries(moduleDeps).map(([k, v]) => `<li><strong>${k}:</strong> ${v.join(" ➜ ")}</li>`).join("");
  modal.innerHTML = `<h3>Mapa de dependencias entre módulos</h3><ul>${lines}</ul><button class="btn primary" id="mCloseDep">Cerrar</button>`;
  modal.showModal();
  modal.querySelector("#mCloseDep").onclick = () => modal.close();
}

function evaluateCertification() {
  const role = state.role || "Quality Manager";
  const quiz = CERT_QUIZ[role] || CERT_QUIZ["Quality Manager"];
  let score = 0;
  quiz.forEach((it, qi) => {
    const selected = document.querySelector(`input[name="quiz_${qi}"]:checked`);
    if (selected && Number(selected.value) === it.answer) score++;
  });
  const checklistDone = (state.certification[role] || []).length === (CERT_CHECKLIST[role] || CERT_CHECKLIST["Quality Manager"]).length;
  const pct = Math.round((score / quiz.length) * 100);
  const pass = pct >= 70 && checklistDone;
  const badge = pass ? "🏅 Certificado Funcional" : "📘 En Progreso";
  state.certResult = `<div class="card ${pass ? "cert-pass" : "cert-progressing"}">
    <strong>${badge}</strong>
    <p>Resultado Quiz: ${score}/${quiz.length} (${pct}%)</p>
    <p>Checklist: ${checklistDone ? "Completo" : "Incompleto"}</p>
    <p>${pass ? "Listo para operación supervisada." : "Completa checklist o refuerza conceptos y vuelve a evaluar."}</p>
  </div>`;
  render();
}

function resetCertification() {
  const role = state.role || "Quality Manager";
  state.certification[role] = [];
  state.certResult = null;
  saveCertification();
  render();
}

function togglePresentation(fromControls = false) {
  state.presenting = !state.presenting;
  $("btnPresent").textContent = state.presenting ? "⏸ Pausar" : "▶ Presentación";
  clearInterval(state.presentTimer);
  if (!state.presenting) { if (fromControls) render(); return; }
  const sequence = ["wizard","roles","training","certification","dashboard","modules","wizards","reports","ia"];
  state.presentTimer = setInterval(() => {
    state.route = sequence[state.presentIdx % sequence.length];
    if (state.route === "wizard") state.wizardStep = (state.wizardStep % 5) + 1;
    state.presentIdx++;
    render();
  }, 3200);
  if (fromControls) render();
}

function movePresentation(delta) {
  const sequence = ["wizard","roles","training","certification","dashboard","modules","wizards","reports","ia"];
  const idx = Math.max(0, sequence.indexOf(state.route));
  const next = (idx + delta + sequence.length) % sequence.length;
  state.route = sequence[next];
  render();
}

function startTour() {
  const steps = [
    { sel: ".search-wrap", txt: "Buscador global simulado para módulos y procesos." },
    { sel: ".kpi-grid", txt: "KPIs animados para narrativa ejecutiva." },
    { sel: "#viewHost", txt: "Vista principal interactiva por módulo, rol y wizard." },
    { sel: "#copilotPanel", txt: "Panel lateral tipo Copilot con explicación funcional." }
  ];
  let i = 0;
  document.querySelector(".tour-tip")?.remove();
  const show = () => {
    document.querySelector(".tour-tip")?.remove();
    if (i >= steps.length) return;
    const el = document.querySelector(steps[i].sel);
    if (!el) return;
    const r = el.getBoundingClientRect();
    const tip = document.createElement("div");
    tip.className = "tour-tip";
    tip.innerHTML = `${steps[i].txt}<div class="toolbar tour-toolbar"><button class="btn secondary" id="tPrev">Anterior</button><button class="btn secondary" id="tNext">Siguiente</button></div>`;
    document.body.appendChild(tip);
    tip.style.top = `${Math.max(10, r.top + 10)}px`;
    tip.style.left = `${Math.max(10, r.left + 10)}px`;
    tip.querySelector("#tPrev").onclick = () => { i = Math.max(0, i - 1); show(); };
    tip.querySelector("#tNext").onclick = () => { i++; show(); };
  };
  show();
}

loadData().finally(render);

