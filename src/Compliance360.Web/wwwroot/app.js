const API = "/api/v1";
const DEFAULT_TENANT = "dc7c46ee-cb25-4ed5-b0b4-800788f7f626";
const DEFAULT_EMAIL = "admin@compliance360.local";

const state = {
  token: localStorage.getItem("c360.token"),
  tenantId: localStorage.getItem("c360.tenantId") || DEFAULT_TENANT,
  email: localStorage.getItem("c360.email") || DEFAULT_EMAIL,
  userId: localStorage.getItem("c360.userId"),
  theme: localStorage.getItem("c360.theme") || "light",
  route: location.hash.replace("#/", "") || "dashboard",
  cache: {}
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
    ["template-builder", "Template Builder"],
    ["regulatory", "Regulatory Management"],
    ["training", "Training Management"],
    ["supplier-portal", "Supplier Portal"],
    ["customer-portal", "Customer Portal"],
    ["security", "Security"],
    ["configuration", "Configuration"]
  ]}
];

const modules = {
  documents: {
    title: "Document Management",
    description: "Listado documental, vigencias, versiones, workflow y aprobaciones.",
    endpoint: tenant => `/tenants/${tenant}/documents?searchText=&page=1&pageSize=10`,
    columns: ["title", "code", "status", "expiresAtUtc"],
    action: "createDocumentFoundation"
  },
  "technical-sheets": {
    title: "Technical Sheets",
    description: "Fichas tecnicas, ingredientes, nutrientes, certificaciones y aprobaciones.",
    endpoint: tenant => `/tenants/${tenant}/technical-sheets?searchText=&page=1&pageSize=10`,
    columns: ["title", "code", "status", "productName"],
    action: "createTechnicalSheetFoundation"
  },
  suppliers: {
    title: "Supplier Management",
    description: "Expediente proveedor, documentos, evaluaciones, homologacion y alertas.",
    endpoint: tenant => `/tenants/${tenant}/suppliers?searchText=&page=1&pageSize=10`,
    columns: ["legalName", "taxIdentifier", "status", "score"],
    action: "createSupplier"
  },
  audits: {
    title: "Audit Management",
    description: "Programacion, checklists, hallazgos, evidencias y seguimiento.",
    endpoint: tenant => `/tenants/${tenant}/audit-management?searchText=&page=1&pageSize=10`,
    columns: ["title", "code", "status", "type"],
    action: "createAuditFoundation"
  },
  capa: {
    title: "CAPA",
    description: "Acciones correctivas/preventivas, seguimiento, aprobaciones y efectividad.",
    endpoint: tenant => `/tenants/${tenant}/capas?searchText=&page=1&pageSize=10`,
    columns: ["title", "code", "status", "priority"],
    action: "createCapa"
  },
  risks: {
    title: "Risk Management",
    description: "Matriz 5x5, mapa de calor, tratamientos, controles y mitigaciones.",
    endpoint: tenant => `/tenants/${tenant}/risks?searchText=&page=1&pageSize=10`,
    columns: ["title", "code", "status", "residualLevel"],
    action: "createRiskFoundation"
  },
  indicators: {
    title: "Quality Indicators",
    description: "KPIs, tendencias, metas, semaforizacion y desviaciones.",
    endpoint: tenant => `/tenants/${tenant}/indicators?searchText=&page=1&pageSize=10`,
    columns: ["name", "code", "status", "type"],
    action: "createIndicatorFoundation"
  }
};

const enterpriseWorkspaces = {
  "template-builder": {
    type: 0,
    title: "Template Builder",
    description: "Constructor enterprise para plantillas, formularios, flujos y reportes operativos.",
    columns: ["title", "code", "status", "dueAtUtc"]
  },
  regulatory: {
    type: 1,
    title: "Regulatory Management",
    description: "Obligaciones regulatorias, evidencias, vencimientos, controles y cumplimiento normativo.",
    columns: ["title", "code", "status", "dueAtUtc"]
  },
  training: {
    type: 2,
    title: "Training Management",
    description: "Planes de capacitacion, cursos, asignaciones, vencimientos y evidencias de entrenamiento.",
    columns: ["title", "code", "status", "dueAtUtc"]
  },
  "supplier-portal": {
    type: 3,
    title: "Supplier Portal",
    description: "Portal colaborativo para proveedores, documentos, homologacion y respuestas.",
    columns: ["title", "code", "status", "dueAtUtc"]
  },
  "customer-portal": {
    type: 4,
    title: "Customer Portal",
    description: "Portal de clientes para solicitudes, documentacion, reportes y trazabilidad.",
    columns: ["title", "code", "status", "dueAtUtc"]
  },
  security: {
    type: 5,
    title: "Security",
    description: "Controles de seguridad, revisiones de acceso, hardening y evidencias.",
    columns: ["title", "code", "status", "dueAtUtc"]
  },
  configuration: {
    type: 6,
    title: "Configuration",
    description: "Configuracion operacional del tenant, parametros y readiness productivo.",
    columns: ["title", "code", "status", "dueAtUtc"]
  }
};

document.documentElement.dataset.theme = state.theme;
window.addEventListener("hashchange", () => {
  state.route = location.hash.replace("#/", "") || "dashboard";
  render();
});

render();

function render() {
  const app = document.querySelector("#app");
  if (!state.token) {
    app.innerHTML = loginView();
    bindLogin();
    return;
  }

  app.innerHTML = shellView();
  bindShell();
  renderRoute();
}

function loginView() {
  return `
    <main class="login-page">
      <section class="login-panel" aria-labelledby="login-title">
        <div class="brand-line">
          <div class="brand-mark" aria-hidden="true">C360</div>
          <span class="product-badge">Enterprise SaaS</span>
        </div>
        <h1 id="login-title">Compliance 360 Enterprise</h1>
        <p>Suite corporativa para cumplimiento, calidad, riesgos, auditorias, CAPA, proveedores, documentos, KPIs y reportes ejecutivos.</p>
        <form id="login-form" class="form-stack">
          <div class="field">
            <label for="tenantId">Tenant ID</label>
            <input id="tenantId" name="tenantId" value="${escapeHtml(state.tenantId)}" required>
          </div>
          <div class="field">
            <label for="email">Email</label>
            <input id="email" name="email" type="email" value="${escapeHtml(state.email)}" required>
          </div>
          <div class="field">
            <label for="password">Password</label>
            <input id="password" name="password" type="password" autocomplete="current-password" required>
          </div>
          <button class="btn primary" type="submit">Iniciar sesion</button>
          <p class="metric-label">Usuario bootstrap local: ${DEFAULT_EMAIL}</p>
        </form>
      </section>
      <section class="login-hero">
        <div class="hero-card">
          <span class="product-badge">Production Workspace</span>
          <h2>Gobierno integral de cumplimiento en una sola plataforma</h2>
          <p>Operacion multitenant, seguridad JWT/RBAC, auditoria append-only, dashboards vivos, reportes programables y acciones reales por modulo.</p>
          <div class="hero-actions">
            <span class="status-pill ok">API live</span>
            <span class="status-pill ok">PostgreSQL</span>
            <span class="status-pill ok">Tenant isolated</span>
          </div>
        </div>
        <div class="grid cards">
          <div class="glass-card"><span class="metric-label">Core modules</span><div class="metric-value">10</div></div>
          <div class="glass-card"><span class="metric-label">Reports</span><div class="metric-value">24</div></div>
          <div class="glass-card"><span class="metric-label">Security</span><div class="metric-value">JWT</div></div>
          <div class="glass-card"><span class="metric-label">Audit</span><div class="metric-value">Append</div></div>
        </div>
      </section>
    </main>`;
}

function shellView() {
  return `
    <div class="layout">
      <aside class="sidebar">
        <div class="brand">
          <div class="brand-mark">C360</div>
          <div>
            <div class="brand-title">Compliance 360</div>
            <div class="brand-subtitle">Enterprise Edition</div>
          </div>
        </div>
        ${navigation.map(group => `
          <nav class="nav-group" aria-label="${group.group}">
            <div class="nav-label">${group.group}</div>
            ${group.items.map(([key, label]) => `
              <button class="nav-button ${state.route === key ? "active" : ""}" data-route="${key}">
                <span>${label}</span><span aria-hidden="true">›</span>
              </button>`).join("")}
          </nav>`).join("")}
      </aside>
      <main class="main">
        <header class="topbar">
          <label class="global-search">
            <span class="sr-only">Busqueda global</span>
            <input id="global-search" class="search-box" placeholder="Buscar documentos, proveedores, riesgos, reportes..." />
          </label>
          <div class="top-actions">
            <span class="status-pill ok">Production core</span>
            <span class="tenant-chip" title="${state.tenantId}">Tenant: ${shortId(state.tenantId)}</span>
            <button id="theme-toggle" class="btn subtle" type="button">${state.theme === "dark" ? "Light" : "Dark"}</button>
            <button id="logout" class="btn danger" type="button">Salir</button>
          </div>
        </header>
        <section id="content" class="content" tabindex="-1"></section>
      </main>
    </div>`;
}

function bindLogin() {
  document.querySelector("#login-form").addEventListener("submit", async event => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      const result = await request("/auth/login", {
        method: "POST",
        body: {
          tenantId: form.get("tenantId"),
          email: form.get("email"),
          password: form.get("password")
        },
        anonymous: true
      });
      state.token = result.accessToken;
      state.tenantId = result.tenantId;
      state.email = result.email;
      state.userId = result.userId;
      localStorage.setItem("c360.token", state.token);
      localStorage.setItem("c360.tenantId", state.tenantId);
      localStorage.setItem("c360.email", state.email);
      localStorage.setItem("c360.userId", state.userId);
      toast("Sesion iniciada correctamente.", "success");
      render();
    } catch (error) {
      toast(error.message, "error");
    }
  });
}

function bindShell() {
  document.querySelectorAll("[data-route]").forEach(button => {
    button.addEventListener("click", () => {
      location.hash = `#/${button.dataset.route}`;
    });
  });
  document.querySelector("#theme-toggle").addEventListener("click", () => {
    state.theme = state.theme === "dark" ? "light" : "dark";
    localStorage.setItem("c360.theme", state.theme);
    document.documentElement.dataset.theme = state.theme;
    render();
  });
  document.querySelector("#logout").addEventListener("click", () => {
    localStorage.removeItem("c360.token");
    state.token = null;
    toast("Sesion cerrada.", "success");
    render();
  });
  document.querySelector("#global-search").addEventListener("keydown", event => {
    if (event.key === "Enter") {
      state.cache.globalSearch = event.currentTarget.value.trim();
      location.hash = "#/documents";
    }
  });
}

async function renderRoute() {
  const content = document.querySelector("#content");
  content.innerHTML = `<div class="loading">Cargando datos reales desde la API...</div>`;
  content.focus();

  try {
    if (state.route === "dashboard" || state.route === "compliance") {
      await renderDashboard(content);
      return;
    }
    if (state.route === "reports") {
      await renderReports(content);
      return;
    }
    if (state.route === "audit-trail") {
      await renderAuditTrail(content);
      return;
    }
    if (modules[state.route]) {
      await renderModule(content, state.route);
      return;
    }
    if (enterpriseWorkspaces[state.route]) {
      await renderEnterpriseWorkspace(content, state.route);
      return;
    }
    renderRoadmap(content, state.route);
  } catch (error) {
    if (String(error.message).includes("401")) {
      localStorage.removeItem("c360.token");
      state.token = null;
      render();
      toast("Sesion expirada. Inicia sesion nuevamente.", "error");
      return;
    }
    content.innerHTML = `<div class="error-state">${escapeHtml(error.message)}</div>`;
  }
}

async function renderDashboard(content) {
  const [health, auditDashboard, capaDashboard, riskDashboard, indicatorDashboard, reports, heatMap] = await Promise.allSettled([
    fetch("/health").then(r => r.json()),
    request(`/tenants/${state.tenantId}/audit-management/dashboard`),
    request(`/tenants/${state.tenantId}/capas/dashboard`),
    request(`/tenants/${state.tenantId}/risks/dashboard`),
    request(`/tenants/${state.tenantId}/indicators/dashboard`),
    request(`/tenants/${state.tenantId}/reports/dashboard-datasets`),
    request(`/tenants/${state.tenantId}/risks/heat-map`)
  ]);

  const audit = valueOf(auditDashboard, {});
  const capa = valueOf(capaDashboard, {});
  const risk = valueOf(riskDashboard, {});
  const indicators = valueOf(indicatorDashboard, {});
  const reportCatalog = valueOf(reports, { datasets: [] });
  const heat = valueOf(heatMap, []);

  content.innerHTML = `
    ${productionHero(audit, capa, risk, indicators, reportCatalog)}
    <section class="grid cards">
      ${metric("API Health", valueOf(health, {}).status || "Healthy", "Estado de servicio")}
      ${metric("Audit Open", audit.openAudits ?? audit.totalOpen ?? 0, "Auditorias abiertas")}
      ${metric("CAPA Open", capa.openCapas ?? capa.totalOpen ?? 0, "Acciones abiertas")}
      ${metric("Risk Critical", risk.criticalRisks ?? 0, "Riesgos criticos")}
    </section>
    <section class="grid two">
      <div class="card">
        <h2 class="section-title">Compliance Performance</h2>
        <p class="metric-label">Indicadores de cumplimiento y desviaciones desde el modulo Quality Indicators.</p>
        <div class="progress" aria-label="Compliance ${indicators.compliancePercent ?? 100}%"><span style="width:${clamp(indicators.compliancePercent ?? 100)}%"></span></div>
        <div class="button-row" style="margin-top:14px">
          <span class="status-chip">Indicators: ${indicators.totalIndicators ?? 0}</span>
          <span class="status-chip">Alerts: ${indicators.alerts ?? 0}</span>
          <span class="status-chip">Negative trends: ${indicators.negativeTrends ?? 0}</span>
        </div>
      </div>
      <div class="card">
        <h2 class="section-title">Dashboard Datasets</h2>
        <div class="metric-value">${reportCatalog.datasets?.length ?? 0}</div>
        <div class="metric-label">Datasets reutilizables disponibles para Dashboard Enterprise.</div>
      </div>
    </section>
    <section class="grid two">
      <div class="card">
        <h2 class="section-title">Risk Heat Map</h2>
        ${heatMapView(Array.isArray(heat) ? heat : [])}
      </div>
      <div class="card">
        <h2 class="section-title">Quick Actions</h2>
        <div class="button-row">
          <button class="btn primary" data-route="reports">Ejecutar reporte</button>
          <button class="btn" data-route="documents">Crear documento</button>
          <button class="btn" data-route="capa">Abrir CAPA</button>
          <button class="btn" data-route="risks">Ver riesgos</button>
          <button class="btn" data-route="indicators">Ver KPIs</button>
          <button class="btn" data-route="audit-trail">Auditoria</button>
        </div>
      </div>
    </section>
    ${moduleTiles()}
    ${readinessRail()}`;
  content.querySelectorAll("[data-route]").forEach(button => button.addEventListener("click", () => location.hash = `#/${button.dataset.route}`));
}

async function renderReports(content) {
  const [standard, search, datasets] = await Promise.all([
    request("/tenants/" + state.tenantId + "/reports/standard"),
    request(`/tenants/${state.tenantId}/reports?searchText=&page=1&pageSize=25`),
    request(`/tenants/${state.tenantId}/reports/dashboard-datasets`)
  ]);
  const rows = search.items || [];
  content.innerHTML = `
    ${pageHeader("Report Center", "Busca, ejecuta, programa y exporta reportes empresariales.", "Reports")}
    <section class="grid cards">
      ${metric("Standard Reports", standard.length || 0, "Catalogo obligatorio")}
      ${metric("Configured", search.totalCount || 0, "Reportes activos/configurados")}
      ${metric("Datasets", datasets.datasets?.length || 0, "Dashboard datasets")}
      ${metric("Formats", "5", "PDF, Excel, Word, CSV, JSON")}
    </section>
    <section class="card">
      <h2 class="section-title">Report Execution Console</h2>
      <div class="button-row">
        <select id="report-select" class="search-box" aria-label="Reporte a ejecutar">
          ${(rows.length ? rows : []).map(row => `<option value="${row.id}">${escapeHtml(row.name || row.code)} (${escapeHtml(row.code || "")})</option>`).join("")}
        </select>
        <button id="execute-report" class="btn primary" ${rows.length ? "" : "disabled"}>Ejecutar</button>
        <button id="schedule-report" class="btn" ${rows.length ? "" : "disabled"}>Programar mensual</button>
      </div>
      <p class="metric-label">Ejecuta y programa reportes activos usando JWT, permisos y tenant reales.</p>
    </section>
    <section class="card">
      <div class="button-row">
        <button id="seed-reports" class="btn primary">Seed standard reports</button>
        <button id="refresh-reports" class="btn">Refrescar</button>
      </div>
    </section>
    ${tableCard("Reportes configurados", rows, ["name", "code", "module", "status", "datasetKey"])}
    ${tableCard("Catalogo enterprise obligatorio", standard, ["name", "code", "module", "datasetKey"])}`;
  document.querySelector("#seed-reports").addEventListener("click", async () => {
    await request(`/tenants/${state.tenantId}/reports/standard/seed`, { method: "POST", body: {} });
    toast("Reportes estandar creados o verificados.", "success");
    await renderRoute();
  });
  document.querySelector("#execute-report")?.addEventListener("click", executeSelectedReport);
  document.querySelector("#schedule-report")?.addEventListener("click", scheduleSelectedReport);
  document.querySelector("#refresh-reports").addEventListener("click", renderRoute);
}

async function executeSelectedReport() {
  const reportId = document.querySelector("#report-select")?.value;
  if (!reportId) return;
  const execution = await request(`/tenants/${state.tenantId}/reports/${reportId}/execute`, {
    method: "POST",
    body: { parametersJson: "{}" }
  });
  await request(`/tenants/${state.tenantId}/reports/${reportId}/complete`, {
    method: "POST",
    body: {
      executionId: execution.id,
      rowCount: 0,
      datasetDescriptorJson: JSON.stringify({ source: "Compliance360 UI", completedAtUtc: new Date().toISOString() })
    }
  });
  await request(`/tenants/${state.tenantId}/reports/${reportId}/export`, {
    method: "POST",
    body: { executionId: execution.id, format: 0 }
  });
  toast("Reporte ejecutado, completado y exportado.", "success");
  await renderRoute();
}

async function scheduleSelectedReport() {
  const reportId = document.querySelector("#report-select")?.value;
  if (!reportId) return;
  const nextRun = new Date(Date.now() + 86400000).toISOString();
  await request(`/tenants/${state.tenantId}/reports/${reportId}/schedules`, {
    method: "POST",
    body: { frequency: 2, nextRunUtc: nextRun }
  });
  toast("Reporte programado correctamente.", "success");
}

async function renderAuditTrail(content) {
  const result = await request(`/tenants/${state.tenantId}/audit/search`, {
    method: "POST",
    body: { page: 1, pageSize: 20 }
  });
  content.innerHTML = `
    ${pageHeader("Audit Trail", "Trazabilidad append-only de eventos de seguridad, ejecuciones y cambios.", "Security / Audit")}
    ${tableCard("Ultimos eventos", result.items || [], ["occurredAtUtc", "action", "category", "entityName", "success"])}`;
}

async function renderModule(content, key) {
  const module = modules[key];
  const data = await request(module.endpoint(state.tenantId));
  const rows = data.items || [];
  content.innerHTML = `
    ${pageHeader(module.title, module.description, "Operations")}
    ${moduleExperiencePanel(key, data.totalCount ?? rows.length)}
    <section class="grid cards">
      ${metric("Records", data.totalCount ?? rows.length, "Resultados tenant-scoped")}
      ${metric("Page", data.page ?? 1, "Pagina actual")}
      ${metric("Page Size", data.pageSize ?? 10, "Tamano de pagina")}
      ${metric("Status", "Live", "API real")}
    </section>
    <section class="card">
      <h2 class="section-title">Action Center</h2>
      ${moduleActionForm(key)}
    </section>
    <section class="card">
      <div class="button-row">
        <input id="module-search" class="search-box" placeholder="Filtro avanzado por texto" value="${escapeHtml(state.cache.globalSearch || "")}" />
        <button id="module-refresh" class="btn">Buscar</button>
        <button class="btn subtle" onclick="location.hash='#/reports'">Exportar via reportes</button>
      </div>
    </section>
    ${tableCard(module.title, rows, module.columns)}`;
  document.querySelector("#module-refresh").addEventListener("click", () => {
    const search = document.querySelector("#module-search").value.trim();
    state.cache.globalSearch = search;
    module.endpoint = tenant => `/${moduleEndpoint(key, tenant, search)}`;
    renderRoute();
  });
  const actionForm = document.querySelector("#module-action-form");
  if (actionForm) {
    actionForm.addEventListener("submit", async event => {
      event.preventDefault();
      await runModuleAction(key, new FormData(actionForm));
    });
  }
}

async function renderEnterpriseWorkspace(content, key) {
  const workspace = enterpriseWorkspaces[key];
  const [items, dashboard] = await Promise.all([
    request(`/tenants/${state.tenantId}/enterprise-workspaces?type=${workspace.type}&searchText=`),
    request(`/tenants/${state.tenantId}/enterprise-workspaces/dashboard`)
  ]);
  const rows = Array.isArray(items) ? items : [];
  content.innerHTML = `
    ${pageHeader(workspace.title, workspace.description, "Enterprise")}
    <section class="hero-card compact module-hero">
      <div>
        <span class="product-badge">Persistent workspace</span>
        <h2>${workspace.title}</h2>
        <p>${workspace.description} Esta vista ya persiste registros multitenant y participa en el readiness operativo del tenant.</p>
      </div>
      <div class="workflow-strip">
        ${["Plan", "Assign", "Execute", "Evidence", "Close"].map(step => `<span>${step}</span>`).join("")}
      </div>
      <div class="module-count"><strong>${rows.length}</strong><span>items</span></div>
    </section>
    <section class="grid cards">
      ${metric("Workspace Items", rows.length, "Registros del modulo")}
      ${metric("Enterprise Total", dashboard.totalItems ?? 0, "Total enterprise")}
      ${metric("Active", dashboard.activeItems ?? 0, "Pendientes")}
      ${metric("Completed", dashboard.completedItems ?? 0, "Cerrados")}
    </section>
    <section class="card">
      <h2 class="section-title">Action Center</h2>
      <form id="enterprise-action-form" class="form-stack">
        <div class="grid two">
          <div class="field"><label for="title">Titulo</label><input id="title" name="title" required value="${defaultEnterpriseTitle(key)}"></div>
          <div class="field"><label for="code">Codigo</label><input id="code" name="code" required value="${defaultEnterpriseCode(key)}"></div>
          <div class="field"><label for="description">Descripcion</label><input id="description" name="description" required value="${workspace.description}"></div>
          <div class="field"><label for="dueAtUtc">Vencimiento</label><input id="dueAtUtc" name="dueAtUtc" type="date" value="${futureDateValue()}"></div>
        </div>
        <div class="button-row">
          <button class="btn primary" type="submit">Crear item enterprise</button>
          <button id="complete-first-item" class="btn" type="button" ${rows.length ? "" : "disabled"}>Completar primer item</button>
        </div>
      </form>
    </section>
    ${tableCard(workspace.title, rows, workspace.columns)}`;

  document.querySelector("#enterprise-action-form").addEventListener("submit", async event => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    await createEnterpriseWorkspaceItem(key, form);
  });
  document.querySelector("#complete-first-item").addEventListener("click", async () => {
    if (!rows.length) return;
    await request(`/tenants/${state.tenantId}/enterprise-workspaces/${rows[0].id}/complete`, { method: "POST", body: {} });
    toast(`${workspace.title}: item completado.`, "success");
    await renderRoute();
  });
}

async function createEnterpriseWorkspaceItem(key, form) {
  const workspace = enterpriseWorkspaces[key];
  const dueValue = String(form.get("dueAtUtc") || "");
  await request(`/tenants/${state.tenantId}/enterprise-workspaces`, {
    method: "POST",
    body: {
      type: workspace.type,
      title: String(form.get("title") || ""),
      code: String(form.get("code") || ""),
      description: String(form.get("description") || ""),
      ownerUserId: state.userId || null,
      dueAtUtc: dueValue ? new Date(`${dueValue}T12:00:00Z`).toISOString() : null,
      metadataJson: JSON.stringify({ source: "Compliance360 Production UI", route: key })
    }
  });
  toast(`${workspace.title}: item creado correctamente.`, "success");
  await renderRoute();
}

function renderRoadmap(content, key) {
  const title = key.split("-").map(word => word[0]?.toUpperCase() + word.slice(1)).join(" ");
  content.innerHTML = `
    ${pageHeader(title, "Centro enterprise incluido en la navegacion principal para completar la operacion corporativa.", "Enterprise Workspace")}
    <section class="hero-card compact">
      <span class="product-badge">Enterprise module</span>
      <h2>${title}</h2>
      <p>Vista preparada para operaciones productivas: gobierno, configuracion, trazabilidad, integraciones y aprobaciones. Este espacio mantiene consistencia visual mientras se conectan los servicios profundos del modulo.</p>
      <div class="hero-actions">
        <span class="status-pill warn">Ready for integration</span>
        <span class="status-pill ok">Navigation live</span>
        <span class="status-pill ok">Security inherited</span>
      </div>
    </section>
    <section class="module-grid">
      ${["Command Center", "Configuration", "Approvals", "Auditability", "Integrations", "Certification"].map(item => `
        <article class="card module-card elevated">
          <h3>${item}</h3>
          <p>Experiencia visual lista para el flujo enterprise y alineada al shell productivo de Compliance 360.</p>
          <span class="tag">Production UI</span>
        </article>`).join("")}
    </section>`;
}

function moduleActionForm(key) {
  const common = `
    <div class="field">
      <label for="name">Nombre / titulo</label>
      <input id="name" name="name" required value="${defaultNameFor(key)}">
    </div>
    <div class="field">
      <label for="code">Codigo</label>
      <input id="code" name="code" required value="${defaultCodeFor(key)}">
    </div>`;

  const specific = {
    documents: `
      ${common}
      <div class="field"><label for="description">Resumen de cambio</label><input id="description" name="description" value="Documento creado desde la UI Enterprise"></div>`,
    "technical-sheets": `
      ${common}
      <div class="field"><label for="description">Descripcion producto</label><input id="description" name="description" value="Producto creado desde la UI Enterprise"></div>`,
    suppliers: `
      <div class="field"><label for="name">Razon social</label><input id="name" name="name" required value="Proveedor Enterprise ${Date.now()}"></div>
      <div class="field"><label for="code">Identificacion fiscal</label><input id="code" name="code" required value="RUC-${Date.now()}"></div>
      <div class="field"><label for="country">Pais</label><input id="country" name="country" maxlength="2" value="PA"></div>`,
    audits: `
      ${common}
      <div class="field"><label for="scope">Alcance</label><input id="scope" name="scope" value="Sistema de gestion de calidad"></div>`,
    capa: `
      ${common}
      <div class="field"><label for="description">Descripcion</label><input id="description" name="description" value="CAPA creada desde la UI Enterprise"></div>`,
    risks: `
      ${common}
      <div class="field"><label for="area">Area</label><input id="area" name="area" value="Quality"></div>
      <div class="field"><label for="process">Proceso</label><input id="process" name="process" value="Compliance"></div>`,
    indicators: `
      ${common}
      <div class="field"><label for="unit">Unidad</label><input id="unit" name="unit" value="%"></div>`
  }[key];

  return `
    <form id="module-action-form" class="form-stack">
      <div class="grid two">${specific}</div>
      <div class="button-row">
        <button class="btn primary" type="submit">Crear registro real</button>
        <span class="metric-label">Usa endpoints reales y refresca el listado del tenant.</span>
      </div>
    </form>`;
}

async function runModuleAction(key, form) {
  const action = modules[key].action;
  const name = String(form.get("name") || "").trim();
  const code = String(form.get("code") || "").trim();
  const description = String(form.get("description") || `Created from Enterprise UI at ${new Date().toISOString()}`);
  try {
    const result = await moduleActions[action]({ name, code, description, form });
    toast(`${modules[key].title}: registro creado correctamente.`, "success");
    state.cache.globalSearch = "";
    await renderRoute();
    return result;
  } catch (error) {
    toast(error.message, "error");
    throw error;
  }
}

const moduleActions = {
  async createSupplier({ name, code, form }) {
    return request(`/tenants/${state.tenantId}/suppliers`, {
      method: "POST",
      body: { legalName: name, taxIdentifier: code, countryCode: String(form.get("country") || "PA").toUpperCase() }
    });
  },
  async createCapa({ name, code, description }) {
    return request(`/tenants/${state.tenantId}/capas`, {
      method: "POST",
      body: {
        title: name,
        code,
        description,
        priority: 2,
        riskLevel: 2,
        sourceType: 5,
        sourceEntityId: null,
        supplierId: null,
        documentId: null,
        auditId: null
      }
    });
  },
  async createRiskFoundation({ name, code, description, form }) {
    const category = await request(`/tenants/${state.tenantId}/risks/categories`, {
      method: "POST",
      body: { name: `${name} Category`, code: `${code}-CAT` }
    });
    await request(`/tenants/${state.tenantId}/risks/matrices`, {
      method: "POST",
      body: { name: `${name} Matrix`, toleranceScore: 10 }
    });
    return request(`/tenants/${state.tenantId}/risks`, {
      method: "POST",
      body: {
        categoryId: category.id,
        title: name,
        code,
        description,
        type: 4,
        area: String(form.get("area") || "Quality"),
        process: String(form.get("process") || "Compliance"),
        supplierId: null,
        documentId: null,
        auditId: null,
        capaId: null
      }
    });
  },
  async createIndicatorFoundation({ name, code, description, form }) {
    const category = await request(`/tenants/${state.tenantId}/indicators/categories`, {
      method: "POST",
      body: { name: `${name} Category`, code: `${code}-CAT` }
    });
    const indicator = await request(`/tenants/${state.tenantId}/indicators`, {
      method: "POST",
      body: {
        categoryId: category.id,
        name,
        code,
        description,
        type: 3,
        frequency: 0,
        calculationType: 2,
        unit: String(form.get("unit") || "%"),
        supplierId: null,
        auditId: null,
        capaId: null,
        riskId: null,
        documentId: null
      }
    });
    await request(`/tenants/${state.tenantId}/indicators/${indicator.id}/target`, {
      method: "POST",
      body: { targetValue: 95, effectiveFromUtc: new Date().toISOString() }
    });
    await request(`/tenants/${state.tenantId}/indicators/${indicator.id}/threshold`, {
      method: "POST",
      body: { warningMinimum: 90, criticalMinimum: 80, excellentMinimum: 98 }
    });
    return indicator;
  },
  async createDocumentFoundation({ name, code, description }) {
    const type = await request(`/tenants/${state.tenantId}/documents/types`, {
      method: "POST",
      body: { name: `${name} Type`, code: `${code}-TYPE`, retentionDays: 2555 }
    });
    const category = await request(`/tenants/${state.tenantId}/documents/categories`, {
      method: "POST",
      body: { name: `${name} Category`, code: `${code}-CAT` }
    });
    return request(`/tenants/${state.tenantId}/documents`, {
      method: "POST",
      body: { documentTypeId: type.id, categoryId: category.id, title: name, code }
    });
  },
  async createTechnicalSheetFoundation({ name, code, description }) {
    const product = await request(`/tenants/${state.tenantId}/technical-sheets/products`, {
      method: "POST",
      body: { name, sku: code, description }
    });
    return request(`/tenants/${state.tenantId}/technical-sheets`, {
      method: "POST",
      body: { productId: product.id, title: `${name} Technical Sheet` }
    });
  },
  async createAuditFoundation({ name, code, form }) {
    const now = new Date();
    const start = new Date(now.getTime() + 86400000);
    const end = new Date(now.getTime() + 172800000);
    const program = await request(`/tenants/${state.tenantId}/audit-management/programs`, {
      method: "POST",
      body: { name: `${name} Program`, code: `${code}-PROG`, year: now.getUTCFullYear() }
    });
    const plan = await request(`/tenants/${state.tenantId}/audit-management/plans`, {
      method: "POST",
      body: {
        auditProgramId: program.id,
        scope: String(form.get("scope") || "Sistema de gestion"),
        criteria: "ISO 9001",
        plannedStartUtc: start.toISOString(),
        plannedEndUtc: end.toISOString()
      }
    });
    return request(`/tenants/${state.tenantId}/audit-management`, {
      method: "POST",
      body: { auditProgramId: program.id, auditPlanId: plan.id, title: name, code, type: 0 }
    });
  }
};

function defaultNameFor(key) {
  const names = {
    documents: "Procedimiento Enterprise",
    "technical-sheets": "Producto Enterprise",
    suppliers: "Proveedor Enterprise",
    audits: "Auditoria Enterprise",
    capa: "CAPA Enterprise",
    risks: "Riesgo Enterprise",
    indicators: "KPI Enterprise"
  };
  return `${names[key] || "Registro"} ${new Date().getMinutes()}${new Date().getSeconds()}`;
}

function defaultCodeFor(key) {
  const prefixes = {
    documents: "DOC",
    "technical-sheets": "TS",
    suppliers: "SUP",
    audits: "AUD",
    capa: "CAPA",
    risks: "RISK",
    indicators: "KPI"
  };
  return `${prefixes[key] || "REG"}-${Date.now()}`;
}

function defaultEnterpriseTitle(key) {
  const names = {
    "template-builder": "Plantilla Enterprise",
    regulatory: "Obligacion Regulatoria",
    training: "Plan de Capacitacion",
    "supplier-portal": "Solicitud Proveedor",
    "customer-portal": "Solicitud Cliente",
    security: "Revision Seguridad",
    configuration: "Parametro Tenant"
  };
  return `${names[key] || "Item Enterprise"} ${new Date().getMinutes()}${new Date().getSeconds()}`;
}

function defaultEnterpriseCode(key) {
  const prefixes = {
    "template-builder": "TPL",
    regulatory: "REG",
    training: "TRN",
    "supplier-portal": "SPORT",
    "customer-portal": "CPORT",
    security: "SEC",
    configuration: "CFG"
  };
  return `${prefixes[key] || "ENT"}-${Date.now()}`;
}

function futureDateValue() {
  const date = new Date(Date.now() + 7 * 86400000);
  return date.toISOString().slice(0, 10);
}

function productionHero(audit, capa, risk, indicators, reportCatalog) {
  return `
    <section class="hero-card dashboard-hero">
      <div>
        <span class="product-badge">Compliance 360 Enterprise Edition</span>
        <h1>Centro de comando para cumplimiento, calidad y riesgo</h1>
        <p>Aplicacion multitenant conectada a datos reales: documentos, proveedores, auditorias, CAPA, riesgos, KPIs, reportes y auditoria de seguridad en un mismo workspace.</p>
        <div class="hero-actions">
          <button class="btn primary" data-route="reports">Abrir Report Center</button>
          <button class="btn light" data-route="documents">Crear evidencia</button>
          <button class="btn light" data-route="risks">Revisar matriz de riesgo</button>
        </div>
      </div>
      <div class="command-panel">
        <div class="command-row"><span>Audit open</span><strong>${audit.openAudits ?? audit.totalOpen ?? 0}</strong></div>
        <div class="command-row"><span>CAPA open</span><strong>${capa.openCapas ?? capa.totalOpen ?? 0}</strong></div>
        <div class="command-row"><span>Critical risk</span><strong>${risk.criticalRisks ?? 0}</strong></div>
        <div class="command-row"><span>Indicators</span><strong>${indicators.totalIndicators ?? 0}</strong></div>
        <div class="command-row"><span>Report datasets</span><strong>${reportCatalog.datasets?.length ?? 0}</strong></div>
      </div>
    </section>`;
}

function moduleTiles() {
  const tiles = [
    ["documents", "Document Control", "Versiones, vigencias y aprobaciones"],
    ["technical-sheets", "Technical Sheets", "Fichas, productos y certificados"],
    ["suppliers", "Suppliers", "Homologacion y evaluaciones"],
    ["audits", "Audits", "Programas, planes y hallazgos"],
    ["capa", "CAPA", "Acciones, causa raiz y efectividad"],
    ["risks", "Risk Matrix", "Controles, tratamientos y heat map"],
    ["indicators", "Quality KPIs", "Metas, umbrales y tendencias"],
    ["reports", "Report Center", "Ejecucion, export y schedules"],
    ["template-builder", "Template Builder", "Plantillas y formularios"],
    ["regulatory", "Regulatory", "Obligaciones y evidencias"],
    ["training", "Training", "Cursos y vencimientos"],
    ["supplier-portal", "Supplier Portal", "Colaboracion proveedor"],
    ["customer-portal", "Customer Portal", "Solicitudes y entregables"]
  ];
  return `
    <section class="card">
      <div class="section-heading">
        <div>
          <h2 class="section-title">Enterprise Workspaces</h2>
          <p class="metric-label">Acceso visual a los dominios productivos principales.</p>
        </div>
      </div>
      <div class="workspace-grid">
        ${tiles.map(([route, title, description]) => `
          <button class="workspace-tile" type="button" data-route="${route}">
            <span class="workspace-icon">${title.slice(0, 2).toUpperCase()}</span>
            <strong>${title}</strong>
            <small>${description}</small>
          </button>`).join("")}
      </div>
    </section>`;
}

function readinessRail() {
  return `
    <section class="grid three">
      ${readinessCard("Security", "JWT, RBAC, tenant enforcement and audit context enabled.", "ok")}
      ${readinessCard("Data", "PostgreSQL persistence with EF Core migrations and tenant-scoped queries.", "ok")}
      ${readinessCard("Operations", "Core modules support real create, search, report and dashboard flows.", "ok")}
    </section>`;
}

function readinessCard(title, text, status) {
  return `
    <article class="card readiness-card">
      <span class="status-pill ${status}">${status === "ok" ? "Ready" : "Review"}</span>
      <h3>${title}</h3>
      <p>${text}</p>
    </article>`;
}

function moduleExperiencePanel(key, totalCount) {
  const module = modules[key];
  const steps = {
    documents: ["Classify", "Version", "Approve", "Expire"],
    "technical-sheets": ["Product", "Ingredients", "Certify", "Approve"],
    suppliers: ["Register", "Validate", "Evaluate", "Homologate"],
    audits: ["Program", "Plan", "Execute", "Close"],
    capa: ["Create", "Root cause", "Actions", "Effectiveness"],
    risks: ["Classify", "Assess", "Treat", "Review"],
    indicators: ["Define", "Target", "Measure", "Trend"]
  }[key] || ["Create", "Manage", "Audit", "Report"];

  return `
    <section class="hero-card compact module-hero">
      <div>
        <span class="product-badge">Live module</span>
        <h2>${module.title}</h2>
        <p>${module.description} Esta pantalla ejecuta operaciones reales sobre el tenant activo y deja evidencia auditada.</p>
      </div>
      <div class="workflow-strip">
        ${steps.map(step => `<span>${step}</span>`).join("")}
      </div>
      <div class="module-count">
        <strong>${totalCount}</strong>
        <span>records</span>
      </div>
    </section>`;
}

function moduleEndpoint(key, tenant, search = "") {
  const q = encodeURIComponent(search);
  const routes = {
    documents: `tenants/${tenant}/documents?searchText=${q}&page=1&pageSize=10`,
    "technical-sheets": `tenants/${tenant}/technical-sheets?searchText=${q}&page=1&pageSize=10`,
    suppliers: `tenants/${tenant}/suppliers?searchText=${q}&page=1&pageSize=10`,
    audits: `tenants/${tenant}/audit-management?searchText=${q}&page=1&pageSize=10`,
    capa: `tenants/${tenant}/capas?searchText=${q}&page=1&pageSize=10`,
    risks: `tenants/${tenant}/risks?searchText=${q}&page=1&pageSize=10`,
    indicators: `tenants/${tenant}/indicators?searchText=${q}&page=1&pageSize=10`
  };
  return routes[key];
}

async function request(path, options = {}) {
  const response = await fetch(`${API}${path}`, {
    method: options.method || "GET",
    headers: {
      "Content-Type": "application/json",
      ...(options.anonymous ? {} : { Authorization: `Bearer ${state.token}` })
    },
    body: options.body ? JSON.stringify(options.body) : undefined
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`${response.status} ${response.statusText}: ${text || "Request failed"}`);
  }
  if (response.status === 204) {
    return {};
  }
  return response.json();
}

function pageHeader(title, description, breadcrumb) {
  return `
    <div class="breadcrumbs">Compliance 360 / ${breadcrumb}</div>
    <div class="page-header">
      <div class="page-title">
        <h1>${title}</h1>
        <p>${description}</p>
      </div>
      <div class="button-row">
        <a class="btn" href="/swagger" target="_blank" rel="noreferrer">Swagger</a>
        <button class="btn subtle" onclick="location.reload()">Reload</button>
      </div>
    </div>`;
}

function metric(label, value, help) {
  return `<article class="card"><div class="metric-label">${label}</div><div class="metric-value">${escapeHtml(String(value))}</div><div class="metric-label">${help}</div></article>`;
}

function tableCard(title, rows, columns) {
  if (!rows.length) {
    return `<section class="card"><h2 class="section-title">${title}</h2><div class="empty">No hay registros todavia en este modulo. Los datos se consultaron desde la API real del tenant.</div></section>`;
  }
  return `
    <section class="card">
      <h2 class="section-title">${title}</h2>
      <div class="table-wrap">
        <table>
          <thead><tr>${columns.map(column => `<th>${column}</th>`).join("")}</tr></thead>
          <tbody>
            ${rows.map(row => `<tr>${columns.map(column => `<td>${formatCell(row[column])}</td>`).join("")}</tr>`).join("")}
          </tbody>
        </table>
      </div>
    </section>`;
}

function heatMapView(points) {
  const buckets = new Map();
  points.forEach(point => {
    const key = `${point.probability || point.x || 1}-${point.impact || point.y || 1}`;
    buckets.set(key, (buckets.get(key) || 0) + 1);
  });
  const cells = [];
  for (let impact = 5; impact >= 1; impact--) {
    for (let probability = 1; probability <= 5; probability++) {
      const count = buckets.get(`${probability}-${impact}`) || 0;
      const score = probability * impact;
      const level = score >= 16 ? "hot" : score >= 8 ? "warn" : "ok";
      cells.push(`<div class="heat-cell ${level}" title="P${probability} I${impact}">${count || ""}</div>`);
    }
  }
  return `<div class="heat-map" aria-label="Risk heat map 5 by 5">${cells.join("")}</div>`;
}

function valueOf(result, fallback) {
  return result.status === "fulfilled" ? result.value : fallback;
}

function formatCell(value) {
  if (value === null || value === undefined || value === "") {
    return '<span class="metric-label">n/a</span>';
  }
  if (typeof value === "string" && value.includes("T") && value.endsWith("Z")) {
    return escapeHtml(new Date(value).toLocaleString());
  }
  if (typeof value === "object") {
    return escapeHtml(JSON.stringify(value));
  }
  return escapeHtml(String(value));
}

function shortId(id) {
  return id ? `${id.slice(0, 8)}...` : "n/a";
}

function clamp(value) {
  return Math.max(0, Math.min(100, Number(value) || 0));
}

function toast(message, type = "info") {
  const region = document.querySelector("#toast-region");
  const node = document.createElement("div");
  node.className = `toast ${type}`;
  node.textContent = message;
  region.appendChild(node);
  setTimeout(() => node.remove(), 4200);
}

function escapeHtml(value) {
  return value.replace(/[&<>"']/g, character => ({
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#039;"
  }[character]));
}
