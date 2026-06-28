const API = "/api/v1";
const DEFAULT_TENANT = "dc7c46ee-cb25-4ed5-b0b4-800788f7f626";
const DEFAULT_EMAIL = "admin@compliance360.local";

const state = {
  token: localStorage.getItem("c360.token"),
  permissions: permissionsFromToken(localStorage.getItem("c360.token")),
  tenantId: localStorage.getItem("c360.tenantId") || DEFAULT_TENANT,
  email: localStorage.getItem("c360.email") || DEFAULT_EMAIL,
  userId: localStorage.getItem("c360.userId"),
  theme: localStorage.getItem("c360.theme") || "light",
  route: location.hash.replace("#/", "") || "dashboard",
  mfaChallenge: null,
  cache: {},
  loading: {
    active: 0,
    messageTimer: null,
    overlayTimer: null,
    messageIndex: 0
  },
  loadingConfig: {
    animations: localStorage.getItem("c360.loading.animations") !== "false",
    skeleton: localStorage.getItem("c360.loading.skeleton") !== "false",
    messages: localStorage.getItem("c360.loading.messages") !== "false",
    reducedMotion: window.matchMedia("(prefers-reduced-motion: reduce)").matches
  }
};

function permissionsFromToken(token) {
  if (!token) return [];
  try {
    const payload = JSON.parse(atob(token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")));
    const permissions = payload.permission || [];
    return Array.isArray(permissions) ? permissions : [permissions];
  } catch {
    return [];
  }
}

function hasPermission(code) {
  return state.permissions.includes(code);
}

function hasAnyPermission(codes) {
  return codes.some(code => hasPermission(code));
}

const routePermissions = {
  dashboard: ["TENANT.READ"],
  compliance: ["TENANT.READ"],
  reports: ["REPORT.READ", "REPORT.EXECUTE"],
  "audit-trail": ["AUDIT.READ"],
  documents: ["DOCUMENT.READ", "DOCUMENT.MANAGE"],
  "technical-sheets": ["TECHNICALSHEET.READ", "TECHNICALSHEET.MANAGE"],
  suppliers: ["SUPPLIER.READ", "SUPPLIER.MANAGE"],
  audits: ["AUDITMANAGEMENT.MANAGE"],
  capa: ["CAPA.READ", "CAPA.MANAGE"],
  risks: ["RISK.READ", "RISK.MANAGE"],
  indicators: ["INDICATOR.READ", "INDICATOR.MANAGE"],
  "superadmin-platform": ["SUPERADMIN.DASHBOARD"],
  "tenant-administration": ["TENANT.USERS", "TENANT.ROLES", "TENANT.UPDATE"],
  "template-builder": ["TENANT.UPDATE"],
  regulatory: ["TENANT.READ"],
  training: ["TENANT.READ"],
  "supplier-portal": ["SUPPLIER.READ", "SUPPLIER.MANAGE"],
  "customer-portal": ["TENANT.READ"],
  security: ["TENANT.SECURITY"],
  configuration: ["STORAGE.MANAGE", "NOTIFICATION.MANAGE", "NOTIFICATION.ADMIN"]
};

const routeManagePermissions = {
  documents: "DOCUMENT.MANAGE",
  "technical-sheets": "TECHNICALSHEET.MANAGE",
  suppliers: "SUPPLIER.MANAGE",
  audits: "AUDITMANAGEMENT.MANAGE",
  capa: "CAPA.MANAGE",
  risks: "RISK.MANAGE",
  indicators: "INDICATOR.MANAGE",
  configuration: "STORAGE.MANAGE"
};

function canNavigate(route) {
  return !routePermissions[route] || hasAnyPermission(routePermissions[route]);
}

function canManageRoute(route) {
  return Boolean(routeManagePermissions[route] && hasPermission(routeManagePermissions[route]));
}

const loadingMessages = {
  default: ["Cargando informacion...", "Sincronizando informacion...", "Procesando solicitud..."],
  login: ["Validando credenciales...", "Cargando perfil...", "Preparando entorno..."],
  dashboard: ["Preparando dashboard...", "Calculando indicadores...", "Analizando datos..."],
  documents: ["Consultando documentos...", "Preparando listado documental...", "Sincronizando informacion..."],
  "technical-sheets": ["Preparando ficha tecnica...", "Consultando productos...", "Cargando informacion tecnica..."],
  suppliers: ["Cargando proveedores...", "Consultando evaluaciones...", "Sincronizando informacion..."],
  audits: ["Cargando auditorias...", "Consultando hallazgos...", "Preparando evidencias..."],
  capa: ["Cargando CAPA...", "Analizando acciones abiertas...", "Actualizando informacion..."],
  risks: ["Analizando datos...", "Calculando matriz de riesgos...", "Preparando controles..."],
  indicators: ["Calculando indicadores...", "Consultando tendencias...", "Preparando metricas..."],
  reports: ["Preparando datos...", "Consultando base de datos...", "Generando documento...", "Aplicando formato...", "Finalizando..."],
  "tenant-administration": ["Cargando Tenant Administration Center...", "Calculando salud del tenant...", "Preparando consola Enterprise..."],
  "superadmin-platform": ["Cargando SuperAdmin Platform Center...", "Calculando salud global...", "Preparando consola de plataforma..."],
  configuration: ["Aplicando configuracion...", "Consultando proveedores...", "Verificando integraciones..."],
  upload: ["Subiendo archivos...", "Calculando tiempo restante...", "Validando evidencia..."],
  export: ["Generando PDF...", "Exportando Excel...", "Preparando descarga..."],
  save: ["Guardando cambios...", "Actualizando informacion...", "Procesando solicitud..."]
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
    ["superadmin-platform", "SuperAdmin Platform"],
    ["tenant-administration", "Tenant Administration"],
    ["template-builder", "Template Builder"],
    ["regulatory", "Regulatory Management"],
    ["training", "Training Management"],
    ["supplier-portal", "Supplier Portal"],
    ["customer-portal", "Customer Portal"],
    ["security", "Security"],
    ["configuration", "Configuration"]
  ]}
];

const routeMetadata = Object.fromEntries(navigation.flatMap(group =>
  group.items.map(([key, label]) => [key, { label, group: group.group, initials: initials(label) }])
));

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
  ensureLoadingHost();
  const app = document.querySelector("#app");
  if (!state.token && state.mfaChallenge) {
    app.innerHTML = mfaChallengeView();
    bindMfaChallenge();
    return;
  }

  if (!state.token) {
    app.innerHTML = loginView();
    bindLogin();
    return;
  }

  app.innerHTML = shellView();
  bindShell();
  renderRoute();
}

function mfaChallengeView() {
  return `
    <main class="login-page">
      <section class="login-panel" aria-labelledby="mfa-title">
        <div class="brand-line">
          <div class="brand-mark" aria-hidden="true">C360</div>
          <span class="product-badge">MFA Required</span>
        </div>
        <h1 id="mfa-title">Verificacion de segundo factor</h1>
        <p>El tenant o el usuario requiere MFA. Ingresa el codigo TOTP para emitir el token final de sesion.</p>
        <form id="mfa-form" class="form-stack">
          <div class="field">
            <label for="verificationCode">Codigo de 6 digitos</label>
            <input id="verificationCode" name="verificationCode" inputmode="numeric" autocomplete="one-time-code" maxlength="6" required>
          </div>
          <button class="btn primary" type="submit">Completar login seguro</button>
          <button id="cancel-mfa" class="btn subtle" type="button">Cancelar</button>
        </form>
      </section>
      <section class="login-hero">
        <div class="hero-card">
          <span class="product-badge">Zero token before MFA</span>
          <h2>JWT bloqueado hasta completar el challenge</h2>
          <p>Compliance 360 protege el acceso productivo con challenge firmado y auditoria de MFA requerida, exitosa o fallida.</p>
        </div>
      </section>
    </main>`;
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
            <input id="tenantId" name="tenantId" value="${escapeHtml(DEFAULT_TENANT)}" required>
          </div>
          <div class="field">
            <label for="email">Email</label>
            <input id="email" name="email" type="email" value="${escapeHtml(DEFAULT_EMAIL)}" required>
          </div>
          <div class="field">
            <label for="password">Password</label>
            <input id="password" name="password" type="password" autocomplete="current-password" required>
          </div>
          <button class="btn primary" type="submit">Iniciar sesion</button>
          <p class="metric-label">Development: utilice las credenciales definidas en BootstrapSuperAdmin. La contrasena no se expone en JavaScript.</p>
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
        <section class="sidebar-status" aria-label="Estado de la plataforma">
          <span class="status-pill ok">Live</span>
          <strong>Production Core 100%</strong>
          <small>Tenant activo, API segura y datos persistentes.</small>
        </section>
        ${navigation.map(group => ({ ...group, items: group.items.filter(([key]) => canNavigate(key)) })).filter(group => group.items.length).map(group => `
          <nav class="nav-group" aria-label="${group.group}">
            <div class="nav-label">${group.group}</div>
            ${group.items.map(([key, label]) => `
              <button class="nav-button ${state.route === key ? "active" : ""}" data-route="${key}">
                <span class="nav-icon">${initials(label)}</span>
                <span>${label}</span>
                <span aria-hidden="true">›</span>
              </button>`).join("")}
          </nav>`).join("")}
        <footer class="sidebar-footer">
          <span>ISO aligned</span>
          <strong>9001 / 37301 / 31000</strong>
        </footer>
      </aside>
      <main class="main">
        <header class="topbar">
          <div class="topbar-context">
            <div class="breadcrumbs compact">Compliance 360 / ${escapeHtml(currentRouteGroup())}</div>
            <strong>${escapeHtml(currentRouteLabel())}</strong>
          </div>
          <div class="topbar-command">
            <label class="global-search">
              <span class="sr-only">Busqueda global</span>
              <input id="global-search" class="search-box" list="route-options" placeholder="Buscar o escribir Enter para documentos..." />
            </label>
            <select id="quick-switcher" class="quick-switcher" aria-label="Ir a modulo">
              ${navigation.flatMap(group => group.items.map(([key, label]) => `<option value="${key}" ${state.route === key ? "selected" : ""}>${group.group} / ${label}</option>`)).join("")}
            </select>
            <datalist id="route-options">
              ${navigation.flatMap(group => group.items.map(([, label]) => `<option value="${label}"></option>`)).join("")}
            </datalist>
          </div>
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
    const button = event.currentTarget.querySelector("button[type='submit']");
    const form = new FormData(event.currentTarget);
    try {
      setLoadingButton(button, true, "Validando...");
      const result = await request("/auth/login", {
        method: "POST",
        body: {
          tenantId: form.get("tenantId"),
          email: form.get("email"),
          password: form.get("password")
        },
        anonymous: true,
        loadingContext: "login",
        overlay: true
      });
      if (result.mfaRequired) {
        state.mfaChallenge = {
          challengeToken: result.mfaChallengeToken,
          method: result.mfaMethod ?? 0,
          tenantId: result.tenantId,
          email: result.email
        };
        toast("MFA requerido. Completa el segundo factor.", "info");
        render();
        return;
      }

      state.token = result.accessToken;
      state.permissions = permissionsFromToken(result.accessToken);
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
    } finally {
      setLoadingButton(button, false);
    }
  });
}

function bindMfaChallenge() {
  document.querySelector("#mfa-form").addEventListener("submit", async event => {
    event.preventDefault();
    const button = event.currentTarget.querySelector("button[type='submit']");
    const form = new FormData(event.currentTarget);
    try {
      setLoadingButton(button, true, "Verificando...");
      const result = await request("/auth/mfa/complete", {
        method: "POST",
        body: {
          challengeToken: state.mfaChallenge.challengeToken,
          method: state.mfaChallenge.method,
          verificationCode: form.get("verificationCode")
        },
        anonymous: true,
        loadingContext: "login",
        overlay: true
      });
      state.mfaChallenge = null;
      state.token = result.accessToken;
      state.permissions = permissionsFromToken(result.accessToken);
      state.tenantId = result.tenantId;
      state.email = result.email;
      state.userId = result.userId;
      localStorage.setItem("c360.token", state.token);
      localStorage.setItem("c360.tenantId", state.tenantId);
      localStorage.setItem("c360.email", state.email);
      localStorage.setItem("c360.userId", state.userId);
      toast("MFA validado. Sesion segura iniciada.", "success");
      render();
    } catch (error) {
      toast(error.message, "error");
    } finally {
      setLoadingButton(button, false);
    }
  });
  document.querySelector("#cancel-mfa").addEventListener("click", () => {
    state.mfaChallenge = null;
    toast("Challenge MFA cancelado.", "info");
    render();
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
    state.permissions = [];
    toast("Sesion cerrada.", "success");
    render();
  });
  document.querySelector("#global-search").addEventListener("keydown", event => {
    if (event.key === "Enter") {
      const value = event.currentTarget.value.trim();
      const route = routeFromLabel(value);
      if (route) {
        location.hash = `#/${route}`;
        return;
      }
      state.cache.globalSearch = value;
      location.hash = "#/documents";
    }
  });
  document.querySelector("#quick-switcher").addEventListener("change", event => {
    location.hash = `#/${event.currentTarget.value}`;
  });
  document.querySelector("#content").addEventListener("click", event => {
    const target = event.target.closest("[data-route], [data-action]");
    if (!target) return;
    if (target.dataset.route) {
      showPageTransition(target.dataset.route);
      location.hash = `#/${target.dataset.route}`;
      return;
    }
    if (target.dataset.action === "reload") {
      location.reload();
    }
  });
}

async function renderRoute() {
  const content = document.querySelector("#content");
  content.innerHTML = loadingView(state.route);
  content.focus();
  const stopLoading = startGlobalLoading(state.route, { overlay: false });

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
    if (state.route === "configuration") {
      await renderIntegrationsAdministration(content);
      return;
    }
    if (state.route === "tenant-administration") {
      await renderTenantAdministrationCenter(content);
      return;
    }
    if (state.route === "superadmin-platform") {
      await renderSuperAdminPlatformCenter(content);
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
      state.permissions = [];
      render();
      toast("Sesion expirada. Inicia sesion nuevamente.", "error");
      return;
    }
    content.innerHTML = errorView(error.message);
    content.querySelector("#retry-route")?.addEventListener("click", renderRoute);
  } finally {
    stopLoading();
  }
}

async function renderDashboard(content) {
  const dashboardRequests = {
    health: fetch("/health").then(r => r.json()),
    auditDashboard: hasPermission("AUDITMANAGEMENT.MANAGE") ? request(`/tenants/${state.tenantId}/audit-management/dashboard`) : Promise.resolve({}),
    capaDashboard: hasAnyPermission(["CAPA.READ", "CAPA.MANAGE"]) ? request(`/tenants/${state.tenantId}/capas/dashboard`) : Promise.resolve({}),
    riskDashboard: hasAnyPermission(["RISK.READ", "RISK.MANAGE"]) ? request(`/tenants/${state.tenantId}/risks/dashboard`) : Promise.resolve({}),
    indicatorDashboard: hasAnyPermission(["INDICATOR.READ", "INDICATOR.MANAGE"]) ? request(`/tenants/${state.tenantId}/indicators/dashboard`) : Promise.resolve({}),
    reports: hasAnyPermission(["REPORT.READ", "REPORT.EXECUTE"]) ? request(`/tenants/${state.tenantId}/reports/dashboard-datasets`) : Promise.resolve({ datasets: [] }),
    heatMap: hasAnyPermission(["RISK.READ", "RISK.MANAGE"]) ? request(`/tenants/${state.tenantId}/risks/heat-map`) : Promise.resolve([])
  };
  const dashboardResults = Object.fromEntries(await Promise.all(
    Object.entries(dashboardRequests).map(async ([key, promise]) => [key, await promiseSettled(promise)])
  ));

  const audit = valueOf(dashboardResults.auditDashboard, {});
  const capa = valueOf(dashboardResults.capaDashboard, {});
  const risk = valueOf(dashboardResults.riskDashboard, {});
  const indicators = valueOf(dashboardResults.indicatorDashboard, {});
  const reportCatalog = valueOf(dashboardResults.reports, { datasets: [] });
  const heat = valueOf(dashboardResults.heatMap, []);

  content.innerHTML = `
    ${productionHero(audit, capa, risk, indicators, reportCatalog)}
    <section class="grid cards">
      ${metric("API Health", statusLabel(valueOf(dashboardResults.health, {}).status || "Healthy"), "Estado de servicio")}
      ${metric("Audit Open", audit.openAudits ?? audit.totalOpen ?? 0, "Auditorias abiertas")}
      ${metric("CAPA Open", capa.openCapas ?? capa.totalOpen ?? 0, "Acciones abiertas")}
      ${metric("Risk Critical", risk.criticalRisks ?? 0, "Riesgos criticos")}
    </section>
    <section class="grid two">
      <div class="card">
        <h2 class="section-title">Compliance Performance</h2>
        <p class="metric-label">Indicadores de cumplimiento y desviaciones desde el modulo Quality Indicators.</p>
        <progress class="progress" value="${clamp(indicators.compliancePercent ?? 100)}" max="100" aria-label="Compliance ${indicators.compliancePercent ?? 100}%"></progress>
        <div class="button-row section-actions">
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
          ${quickRouteButton("reports", "Ejecutar reporte", "primary")}
          ${quickRouteButton("documents", "Documentos")}
          ${quickRouteButton("capa", "Abrir CAPA")}
          ${quickRouteButton("risks", "Ver riesgos")}
          ${quickRouteButton("indicators", "Ver KPIs")}
          ${quickRouteButton("audit-trail", "Auditoria")}
        </div>
      </div>
    </section>
    ${moduleTiles()}
    ${readinessRail()}`;
  content.querySelectorAll("[data-route]").forEach(button => button.addEventListener("click", () => location.hash = `#/${button.dataset.route}`));
}

async function renderIntegrationsAdministration(content) {
  const [health, notificationDashboard, storageProviders] = await Promise.allSettled([
    request("/health/ready"),
    request(`/tenants/${state.tenantId}/notifications/dashboard`),
    request(`/tenants/${state.tenantId}/storage/providers`)
  ]);
  const healthText = health.status === "fulfilled" ? "Ready endpoint disponible" : "Ready endpoint degradado";
  const notification = notificationDashboard.status === "fulfilled" ? notificationDashboard.value : {};
  const storage = storageProviders.status === "fulfilled" ? storageProviders.value : [];
  content.innerHTML = `
    <section class="module-page">
      <div class="module-hero">
        <div>
          <p class="eyebrow">Configuracion / Integraciones</p>
          <h1>Provider Administration</h1>
          <p>Email Providers, Storage Providers, Health Status, Connection Test, Usage Statistics, Failover Configuration y Provider Priority por tenant.</p>
        </div>
        <div class="hero-actions">
          <button class="btn primary" id="create-storage-provider" type="button">Crear Storage Local</button>
          <button class="btn" id="create-email-provider" type="button">Crear Email SMTP</button>
        </div>
      </div>
      <div class="metric-grid">
        <article class="metric-card"><span>Health Status</span><strong>${healthText}</strong></article>
        <article class="metric-card"><span>Notifications Sent</span><strong>${notification.sent ?? 0}</strong></article>
        <article class="metric-card"><span>Delivery Rate</span><strong>${notification.deliveryRatePercent ?? 0}%</strong></article>
        <article class="metric-card"><span>Dead Letters</span><strong>${notification.deadLetters ?? 0}</strong></article>
      </div>
      <div class="grid two">
        <article class="panel">
          <h2>Email Providers</h2>
          <p>SMTP, Gmail SMTP, Microsoft 365, Exchange Online, SendGrid, Mailgun, Resend y Amazon SES configurables por tenant.</p>
          ${tableCard("Email provider health", Object.entries(notification.providerHealth ?? {}).map(([provider, ok]) => ({ provider, status: ok ? "Saludable" : "Requiere configuracion" })), ["provider", "status"])}
        </article>
        <article class="panel">
          <h2>Storage Providers</h2>
          <p>Local, Azure Blob, AWS S3, MinIO, Google Cloud Storage y SFTP con prioridad/failover por tenant.</p>
          ${tableCard("Storage provider configurations", storage, ["provider", "name", "containerName", "priority", "isDefault", "isEnabled", "lastHealthStatus"])}
        </article>
      </div>
      <article class="panel">
        <h2>Connection Test & Failover</h2>
        <p>Usa prioridad ascendente: proveedor principal, secundario y terciario. Si el principal falla, el backend prueba el siguiente y registra AuditLog/Observability.</p>
        <button class="btn" id="test-first-storage-provider" type="button" ${storage.length ? "" : "disabled"}>Probar primer Storage Provider</button>
      </article>
    </section>
  `;
  content.querySelector("#create-storage-provider")?.addEventListener("click", event => createDefaultStorageProvider(event.currentTarget));
  content.querySelector("#create-email-provider")?.addEventListener("click", event => createDefaultEmailProvider(event.currentTarget));
  content.querySelector("#test-first-storage-provider")?.addEventListener("click", async () => {
    if (!storage.length) return;
    const button = content.querySelector("#test-first-storage-provider");
    try {
      setLoadingButton(button, true, "Probando...");
      await request(`/tenants/${state.tenantId}/storage/providers/${storage[0].id}/test`, { method: "POST", body: {}, loadingContext: "configuration" });
      toast("Storage provider test ejecutado", "success");
      await renderRoute();
    } finally {
      setLoadingButton(button, false);
    }
  });
}

async function createDefaultStorageProvider(button) {
  try {
    setLoadingButton(button, true, "Guardando...");
    await request(`/tenants/${state.tenantId}/storage/providers`, {
      method: "POST",
      loadingContext: "configuration",
      body: {
        provider: 0,
        name: "Local Primary",
        containerName: "tenant-documents",
        priority: 1,
        isDefault: true,
        isEnabled: true,
        settingsJson: JSON.stringify({ rootPath: "storage" })
      }
    });
    toast("Storage provider creado", "success");
    await renderRoute();
  } finally {
    setLoadingButton(button, false);
  }
}

async function createDefaultEmailProvider(button) {
  try {
    setLoadingButton(button, true, "Guardando...");
    await request(`/tenants/${state.tenantId}/notifications/providers`, {
      method: "POST",
      loadingContext: "configuration",
      body: {
        provider: 0,
        name: "SMTP Primary",
        priority: 1,
        isDefault: true,
        isEnabled: false
      }
    });
    toast("Email provider creado", "success");
    await renderRoute();
  } finally {
    setLoadingButton(button, false);
  }
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
  document.querySelector("#seed-reports").addEventListener("click", async event => {
    try {
      setLoadingButton(event.currentTarget, true, "Generando...");
      await request(`/tenants/${state.tenantId}/reports/standard/seed`, { method: "POST", body: {}, loadingContext: "reports" });
      toast("Reportes estandar creados o verificados.", "success");
      await renderRoute();
    } finally {
      setLoadingButton(event.currentTarget, false);
    }
  });
  document.querySelector("#execute-report")?.addEventListener("click", event => executeSelectedReport(event.currentTarget));
  document.querySelector("#schedule-report")?.addEventListener("click", event => scheduleSelectedReport(event.currentTarget));
  document.querySelector("#refresh-reports").addEventListener("click", renderRoute);
}

async function executeSelectedReport(button) {
  const reportId = document.querySelector("#report-select")?.value;
  if (!reportId) return;
  try {
    setLoadingButton(button, true, "Generando...");
    showProgressBanner("reports");
    const execution = await request(`/tenants/${state.tenantId}/reports/${reportId}/execute`, {
      method: "POST",
      loadingContext: "reports",
      body: { parametersJson: "{}" }
    });
    await request(`/tenants/${state.tenantId}/reports/${reportId}/complete`, {
      method: "POST",
      loadingContext: "reports",
      body: {
        executionId: execution.id,
        rowCount: 0,
        datasetDescriptorJson: JSON.stringify({ source: "Compliance360 UI", completedAtUtc: new Date().toISOString() })
      }
    });
    await request(`/tenants/${state.tenantId}/reports/${reportId}/export`, {
      method: "POST",
      loadingContext: "export",
      body: { executionId: execution.id, format: 0 }
    });
    toast("Reporte ejecutado, completado y exportado.", "success");
    await renderRoute();
  } finally {
    hideProgressBanner();
    setLoadingButton(button, false);
  }
}

async function scheduleSelectedReport(button) {
  const reportId = document.querySelector("#report-select")?.value;
  if (!reportId) return;
  try {
    setLoadingButton(button, true, "Programando...");
    const nextRun = new Date(Date.now() + 86400000).toISOString();
    await request(`/tenants/${state.tenantId}/reports/${reportId}/schedules`, {
      method: "POST",
      loadingContext: "reports",
      body: { frequency: 2, nextRunUtc: nextRun }
    });
    toast("Reporte programado correctamente.", "success");
  } finally {
    setLoadingButton(button, false);
  }
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
  const canManage = canManageRoute(key);
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
      ${canManage ? moduleActionForm(key) : readOnlyNotice(key)}
    </section>
    <section class="card">
      <div class="button-row">
        <input id="module-search" class="search-box" placeholder="Filtro avanzado por texto" value="${escapeHtml(state.cache.globalSearch || "")}" />
        <button id="module-refresh" class="btn">Buscar</button>
        <button class="btn subtle" type="button" data-route="reports">Exportar via reportes</button>
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
      const button = actionForm.querySelector("button[type='submit']");
      try {
        setLoadingButton(button, true, "Guardando...");
        await runModuleAction(key, new FormData(actionForm));
      } finally {
        setLoadingButton(button, false);
      }
    });
  }
}

async function renderSuperAdminPlatformCenter(content) {
  const center = await request("/superadmin/platform-center", { loadingContext: "superadmin-platform" });
  const metrics = center.metrics || {};
  const alerts = center.alerts || [];
  const tenants = center.tenants || [];
  const providers = center.providers || [];
  const licenses = center.licenses || [];
  const modules = center.modules || [];
  const health = center.health || [];
  const backups = center.backups || [];
  const database = center.database || [];
  const timeline = center.auditTimeline || [];
  const quickActions = center.quickActions || [];
  const globalHealth = metrics.globalHealth || "Unknown";
  const activePlatformTab = localStorage.getItem("c360.platformTab") || "executive";

  content.innerHTML = `
    ${pageHeader("SuperAdmin Platform Center", "Consola global para administrar y observar toda la plataforma Compliance 360.", "Platform / Global Administration")}
    <section class="tenant-hero platform-hero">
      <div>
        <span class="product-badge">Global SuperAdmin</span>
        <h2>Centro de Gobierno Global Compliance 360</h2>
        <p>Tenants, licencias, proveedores, seguridad, observabilidad, base de datos, backups, DevOps, auditoria e IA con datos del backend.</p>
      </div>
      <div class="tenant-health">
        <span class="status-pill ${globalHealth === "Healthy" ? "ok" : globalHealth === "Unhealthy" ? "danger" : "warn"}">${safe(statusLabel(globalHealth))}</span>
        <strong>${metrics.tenants || 0} tenants</strong>
        <small>Generado ${formatDate(metrics.generatedAtUtc)}</small>
      </div>
    </section>
    <section class="platform-command">
      <input id="platform-search" placeholder="Buscar globalmente: tenant, proveedor, licencia, auditoria, modulo..." aria-label="Busqueda global de plataforma">
      <button id="export-platform-audit" class="btn" type="button">Exportar auditoria global CSV</button>
    </section>
    <section class="grid cards tenant-dashboard-grid">
      ${metric("Tenants", metrics.tenants || 0, `Activos ${metrics.activeTenants || 0}`)}
      ${metric("Trial", metrics.trialTenants || 0, "Onboarding")}
      ${metric("Suspendidos", metrics.suspendedTenants || 0, "Governance")}
      ${metric("Archivados", metrics.archivedTenants || 0, "Retention")}
      ${metric("Usuarios", metrics.totalUsers || 0, `Activos ${metrics.activeUsers || 0}`)}
      ${metric("Documentos", metrics.documents || 0, "Global")}
      ${metric("Auditorias", metrics.audits || 0, "Managed audits")}
      ${metric("CAPA", metrics.capas || 0, "Corrective actions")}
      ${metric("Riesgos", metrics.risks || 0, "Risk register")}
      ${metric("Indicadores", metrics.indicators || 0, "KPIs")}
      ${metric("Storage Used", formatBytes(metrics.storageUsedBytes || 0), `Cap ${formatBytes(metrics.storageBytes || 0)}`)}
      ${metric("SMTP Providers", metrics.smtpProviders || 0, "Notifications")}
      ${metric("Storage Providers", metrics.storageProviders || 0, "Object storage")}
      ${metric("API Requests", metrics.apiRequests || 0, "Audit-observed")}
      ${metric("Errores", metrics.errors || 0, "Audit/notifications")}
      ${metric("Background Jobs", metrics.backgroundJobs || 0, "Retry backlog")}
      ${metric("Licencias", metrics.licenses || 0, "Tenant licenses")}
      ${metric("Alertas", metrics.alerts || alerts.length, "Global")}
    </section>
    ${superAdminAlerts(alerts)}
    <section class="tenant-admin-shell platform-shell">
      <aside class="tenant-tabs" aria-label="SuperAdmin Platform tabs">
        ${superAdminTabs().map((tab, index) => `<button class="tenant-tab ${tab.key === activePlatformTab ? "active" : ""}" type="button" data-tab="${tab.key}"><span>${index + 1}</span>${tab.label}</button>`).join("")}
      </aside>
      <div class="tenant-tab-panels">
        ${superAdminPanel("executive", "Executive Dashboard", quickActionsPanel(quickActions) + platformConsumptionPanel(metrics), activePlatformTab === "executive")}
        ${superAdminPanel("tenants", "Tenants", createTenantPanel() + tableCard("Tenant fleet", tenants.map(tenant => ({
          name: tenant.name,
          slug: tenant.slug,
          status: tenant.status,
          plan: tenant.plan,
          users: tenant.users,
          storage: formatBytes(tenant.storageBytes),
          created: formatDate(tenant.createdAtUtc),
          actions: `__html:<button class="btn small" data-platform-tenant="${tenant.id}">Abrir TAC</button>`
        })), ["name", "slug", "status", "plan", "users", "storage", "created", "actions"]), activePlatformTab === "tenants")}
        ${superAdminPanel("licenses", "Licencias", tableCard("Licencias y consumo", licenses.map(license => ({
          tenant: license.tenantName,
          plan: license.plan,
          status: license.status,
          users: `${license.usersUsed}/${license.maxUsers}`,
          storage: `${formatBytes(license.storageUsedBytes)} / ${license.maxStorageGb} GB`,
          expires: license.expiresOn || "n/a",
          renewal: license.renewalDate || "n/a"
        })), ["tenant", "plan", "status", "users", "storage", "expires", "renewal"]), activePlatformTab === "licenses")}
        ${superAdminPanel("modules", "Modulos", tableCard("Matriz de modulos por tenant", modules.slice(0, 100).map(item => ({ tenant: item.tenantName, module: item.module, enabled: item.enabled ? "Activo" : "Inactivo", source: item.source, health: statusLabel(item.health) })), ["tenant", "module", "enabled", "source", "health"]), activePlatformTab === "modules")}
        ${superAdminPanel("providers", "Providers", tableCard("Providers globales tenant-scoped", providers.map(provider => ({
          tenant: provider.tenantName,
          type: provider.type,
          provider: provider.provider,
          name: provider.name,
          enabled: provider.isEnabled ? "Activo" : "Inactivo",
          health: statusLabel(provider.health),
          last: formatDate(provider.lastHealthCheckAtUtc)
        })), ["tenant", "type", "provider", "name", "enabled", "health", "last"]), activePlatformTab === "providers")}
        ${superAdminPanel("security", "Seguridad Global", platformSecurityPanel(), activePlatformTab === "security")}
        ${superAdminPanel("observability", "Observability", tableCard("Health, trazas y servicios de fondo", health.map(signal => ({ component: signal.component, status: statusLabel(signal.status), message: signal.message, tenant: shortId(signal.tenantId), checked: formatDate(signal.checkedAtUtc) })), ["component", "status", "message", "tenant", "checked"]), activePlatformTab === "observability")}
        ${superAdminPanel("audit", "Auditoria Global", superAdminTimeline(timeline), activePlatformTab === "audit")}
        ${superAdminPanel("database", "Base de Datos", tableCard("Monitoreo PostgreSQL", database.map(item => ({ metric: item.name, value: item.value, status: statusLabel(item.status), description: item.description })), ["metric", "value", "status", "description"]), activePlatformTab === "database")}
        ${superAdminPanel("ai", "IA", platformAiPanel(globalHealth), activePlatformTab === "ai")}
        ${superAdminPanel("configuration", "Configuracion Global", platformGlobalConfigurationPanel(), activePlatformTab === "configuration")}
        ${superAdminPanel("backups", "Backups", tableCard("Backups registrados", backups.map(backup => ({ tenant: backup.tenantName, kind: backup.backupKind, result: backup.result, completed: formatDate(backup.completedAtUtc), duration: backup.duration, size: formatBytes(backup.sizeBytes), rpo: backup.rpo, rto: backup.rto })), ["tenant", "kind", "result", "completed", "duration", "size", "rpo", "rto"]), activePlatformTab === "backups")}
        ${superAdminPanel("devops", "DevOps", platformDevOpsPanel(database), activePlatformTab === "devops")}
      </div>
    </section>`;

  bindSuperAdminPlatformCenter();
}

function superAdminTabs() {
  return [
    { key: "executive", label: "Executive" },
    { key: "tenants", label: "Tenants" },
    { key: "licenses", label: "Licencias" },
    { key: "modules", label: "Modulos" },
    { key: "providers", label: "Providers" },
    { key: "security", label: "Seguridad" },
    { key: "observability", label: "Observability" },
    { key: "audit", label: "Auditoria" },
    { key: "database", label: "Database" },
    { key: "ai", label: "IA" },
    { key: "configuration", label: "Configuracion" },
    { key: "backups", label: "Backups" },
    { key: "devops", label: "DevOps" }
  ];
}

function superAdminPanel(key, title, body, active = false) {
  return `<section class="tenant-panel ${active ? "active" : ""}" data-panel="${key}"><div class="section-heading"><div><h2 class="section-title">${safe(title)}</h2><p class="metric-label">Administracion global de plataforma.</p></div><span class="status-pill ok">Backend activo</span></div>${body}</section>`;
}

function quickActionsPanel(actions) {
  return `<div class="grid cards">${actions.map(action => `<article class="card"><span class="product-badge">${safe(action.permission)}</span><h3>${safe(action.label)}</h3><p>${safe(action.description)}</p><button class="btn small" data-quick-action="${safe(action.key)}" data-route-target="${safe(action.route)}">Ejecutar</button></article>`).join("")}</div>`;
}

function createTenantPanel() {
  return `
    <section id="create-tenant-panel" class="card">
      <div class="section-heading">
        <div>
          <h2 class="section-title">Crear Tenant</h2>
          <p class="metric-label">Alta enterprise de tenant. Al guardar se abrira automaticamente el Tenant Administration Center del nuevo tenant.</p>
        </div>
        <span class="status-pill ok">SUPERADMIN.TENANTS.CREATE</span>
      </div>
      <form id="create-tenant-form" class="form-stack">
        <div class="grid two">
          ${inputField("newTenantName", "Tenant Name", "Cliente Demo Compliance", "text", false, true, { maxLength: 180, help: "Nombre interno del tenant." })}
          ${inputField("newTenantSlug", "Slug", "cliente-demo-compliance", "text", false, true, { maxLength: 80, pattern: "^[a-z0-9]+(?:-[a-z0-9]+)*$", title: "Use minusculas, numeros y guiones.", help: "Identificador URL-friendly. Debe ser unico." })}
          ${inputField("newTenantLegalName", "Razon Social", "Cliente Demo Compliance S.A.", "text", false, true, { maxLength: 220 })}
          ${inputField("newTenantCommercialName", "Nombre Comercial", "Cliente Demo", "text", false, true, { maxLength: 180 })}
          ${inputField("newTenantTaxIdentifier", "RUC / Tax ID", `DEMO-${Date.now().toString().slice(-6)}`, "text", false, true, { maxLength: 80, pattern: "^[A-Za-z0-9][A-Za-z0-9.\\-]{2,78}[A-Za-z0-9]$", title: "Use letras, numeros, guiones o puntos." })}
          ${selectField("newTenantCountryCode", "Pais", "PA", countryOptions(), "Codigo ISO del pais.")}
          ${selectField("newTenantCurrency", "Moneda", "USD", currencyOptions(), "Moneda ISO-4217.")}
        </div>
        <div class="grid two">
          <button class="btn primary" type="submit">Crear tenant</button>
          <button class="btn" type="button" id="cancel-create-tenant">Cancelar</button>
        </div>
      </form>
    </section>`;
}

function bindCreateTenantForm() {
  const form = document.querySelector("#create-tenant-form");
  if (!form) {
    return;
  }

  const name = form.querySelector("#newTenantName");
  const slug = form.querySelector("#newTenantSlug");
  name?.addEventListener("input", () => {
    if (!slug.dataset.touched) {
      slug.value = slugifyTenantName(name.value);
    }
  });
  slug?.addEventListener("input", () => {
    slug.dataset.touched = "true";
  });

  document.querySelector("#cancel-create-tenant")?.addEventListener("click", () => {
    document.querySelector("#create-tenant-panel")?.setAttribute("hidden", "");
  });

  form.addEventListener("submit", async event => {
    event.preventDefault();
    if (!form.reportValidity()) {
      return;
    }

    const button = form.querySelector("button[type='submit']");
    const formData = new FormData(form);
    try {
      setLoadingButton(button, true, "Creando...");
      const tenant = await request("/tenants", {
        method: "POST",
        loadingContext: "save",
        overlay: true,
        body: {
          name: formData.get("newTenantName"),
          slug: formData.get("newTenantSlug"),
          legalName: formData.get("newTenantLegalName"),
          commercialName: formData.get("newTenantCommercialName"),
          taxIdentifier: formData.get("newTenantTaxIdentifier"),
          countryCode: formData.get("newTenantCountryCode"),
          currency: formData.get("newTenantCurrency")
        }
      });
      state.tenantId = tenant.id;
      localStorage.setItem("c360.tenantId", state.tenantId);
      toast(`Tenant ${tenant.name} creado correctamente.`, "success");
      location.hash = "#/tenant-administration";
    } catch (error) {
      toast(error.message, "error");
    } finally {
      setLoadingButton(button, false);
    }
  });
}

function slugifyTenantName(value) {
  return String(value || "")
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 80);
}

function platformConsumptionPanel(metrics) {
  const storagePercent = metrics.storageBytes ? Math.min(100, Math.round((metrics.storageUsedBytes || 0) * 100 / metrics.storageBytes)) : 0;
  const activeTenantPercent = metrics.tenants ? Math.round((metrics.activeTenants || 0) * 100 / metrics.tenants) : 0;
  return `<div class="grid two"><section class="card"><h3>Tenant Health Mix</h3><div class="progress-track"><span class="progress-bar dynamic" data-width="${activeTenantPercent}"></span></div><p>${activeTenantPercent}% tenants activos</p></section><section class="card"><h3>Storage Consumption</h3><div class="progress-track"><span class="progress-bar dynamic" data-width="${storagePercent}"></span></div><p>${storagePercent}% storage utilizado</p></section></div>`;
}

function platformSecurityPanel() {
  const rows = [
    { area: "Politicas de password", status: "Configurado", source: "Opciones de infraestructura" },
    { area: "MFA", status: "Aplicado por tenant/usuario", source: "Servicios Identity + MFA" },
    { area: "JWT", status: "Configurado", source: "JwtOptions" },
    { area: "Refresh Tokens", status: "Trazado", source: "Tabla RefreshTokens" },
    { area: "Rate Limiting", status: "Activo", source: "Politica API" },
    { area: "CORS", status: "Configurado", source: "AllowedOrigins" },
    { area: "Headers", status: "Activo", source: "SecurityHeadersMiddleware" },
    { area: "Secrets/Vault", status: "Externalizado", source: "Solo referencias a secretos" }
  ];
  return tableCard("Global Security Policies", rows, ["area", "status", "source"]);
}

function platformAiPanel(globalHealth) {
  const rows = [
    { capability: "Proveedores IA", status: "Sin registros", evidence: "No existen registros de proveedores IA" },
    { capability: "Prompts", status: "Sin registros", evidence: "No existe registro de prompts" },
    { capability: "Tokens/Costos", status: "Sin registros", evidence: "No existen registros de consumo IA" },
    { capability: "Fallback", status: "Sin registros", evidence: "No existen reglas de fallback IA" },
    { capability: "Health", status: statusLabel(globalHealth), evidence: "Resumen de salud de plataforma" }
  ];
  return tableCard("AI Administration Read Model", rows, ["capability", "status", "evidence"]);
}

function platformGlobalConfigurationPanel() {
  const rows = [
    { item: "Empresa propietaria", value: "Compliance 360", status: "Configurado en identidad de producto" },
    { item: "Branding global", value: "C360 Enterprise", status: "Shell frontend" },
    { item: "SMTP global", value: "Modelo de proveedor por tenant", status: "Gestionado por tenant" },
    { item: "Storage global", value: "Modelo de proveedor por tenant", status: "Gestionado por tenant" },
    { item: "Politicas globales", value: "Middleware de seguridad + opciones", status: "Aplicado por backend" }
  ];
  return tableCard("Configuracion global", rows, ["item", "value", "status"]);
}

function platformDevOpsPanel(database) {
  const provider = database.find(item => item.name === "Provider")?.value || "EF Core";
  const rows = [
    { item: "Build", value: "Validado por dotnet build", status: "Disponible" },
    { item: "Version", value: "Compliance360 Web", status: "Runtime" },
    { item: "Deploys", value: "CI/CD externo", status: "Solo lectura" },
    { item: "Migraciones", value: provider, status: "Trazado" },
    { item: "Rollback", value: "Controlado fuera de la UI", status: "Gobernado" }
  ];
  return tableCard("DevOps read model", rows, ["item", "value", "status"]);
}

function superAdminTimeline(timeline) {
  const rows = timeline.length ? timeline : [];
  return `<div class="tenant-timeline">${rows.map(item => `<article class="timeline-item"><span></span><div><strong>${safe(item.action)} · ${safe(item.entityName)} · ${safe(item.tenantName || "Plataforma")}</strong><p>${formatDate(item.occurredAtUtc)} · Usuario ${shortId(item.userId)} · IP ${safe(item.ipAddress || "n/a")} · Corr ${safe(item.correlationId || "n/a")}</p><small>${safe(item.metadataJson || "Evento de auditoria global")}</small></div></article>`).join("") || `<div class="empty-state"><strong>No hay eventos de auditoria.</strong><p>El timeline global esta vacio.</p></div>`}</div>`;
}

function superAdminAlerts(alerts) {
  const items = alerts.length ? alerts : [{ severity: "ok", title: "Sin alertas criticas globales", message: "El read model de plataforma no detecto problemas criticos.", area: "Plataforma" }];
  return `<section class="tenant-alerts">${items.map(alert => `<article class="tenant-alert ${safe(alert.severity)}"><strong>${safe(alert.title)}</strong><p>${safe(alert.message)}</p><small>${safe(alert.area || "Plataforma")}</small></article>`).join("")}</section>`;
}

function bindSuperAdminPlatformCenter() {
  document.querySelectorAll(".tenant-tab").forEach(tab => {
    tab.addEventListener("click", () => {
      localStorage.setItem("c360.platformTab", tab.dataset.tab);
      document.querySelectorAll(".tenant-tab").forEach(item => item.classList.toggle("active", item === tab));
      document.querySelectorAll(".tenant-panel").forEach(panel => panel.classList.toggle("active", panel.dataset.panel === tab.dataset.tab));
    });
  });

  document.querySelectorAll(".progress-bar.dynamic").forEach(bar => {
    bar.style.width = `${Math.max(0, Math.min(100, Number(bar.dataset.width || 0)))}%`;
  });

  document.querySelector("#platform-search")?.addEventListener("input", event => {
    const text = event.currentTarget.value.toLowerCase();
    document.querySelectorAll(".tenant-tab-panels table tbody tr").forEach(row => {
      row.classList.toggle("hidden", text.length > 1 && !row.textContent.toLowerCase().includes(text));
    });
  });

  document.querySelectorAll("[data-platform-tenant]").forEach(button => {
    button.addEventListener("click", event => {
      state.tenantId = event.currentTarget.dataset.platformTenant;
      localStorage.setItem("c360.tenantId", state.tenantId);
      localStorage.setItem("c360.platformTab", "tenants");
      localStorage.setItem("c360.tenantReturn", "superadmin-platform");
      location.hash = "#/tenant-administration";
    });
  });

  document.querySelectorAll("[data-quick-action]").forEach(button => {
    button.addEventListener("click", event => {
      if (event.currentTarget.dataset.quickAction === "tenant-create") {
        localStorage.setItem("c360.platformTab", "tenants");
        document.querySelectorAll(".tenant-tab").forEach(item => item.classList.toggle("active", item.dataset.tab === "tenants"));
        document.querySelectorAll(".tenant-panel").forEach(panel => panel.classList.toggle("active", panel.dataset.panel === "tenants"));
        const panel = document.querySelector("#create-tenant-panel");
        panel?.removeAttribute("hidden");
        panel?.scrollIntoView({ behavior: "smooth", block: "start" });
        document.querySelector("#newTenantName")?.focus();
        return;
      }

      const target = event.currentTarget.dataset.routeTarget || "";
      if (target.startsWith("#/")) {
        location.hash = target;
      } else if (target.startsWith("#")) {
        document.querySelector(`[data-panel="${target.slice(1)}"]`)?.scrollIntoView({ behavior: "smooth", block: "start" });
      }
    });
  });

  bindCreateTenantForm();

  document.querySelector("#export-platform-audit")?.addEventListener("click", async () => {
    await downloadProtectedText("/superadmin/platform-center/audit-timeline/export?page=1&pageSize=250", "superadmin-global-audit.csv");
  });
}

async function renderTenantAdministrationCenter(content) {
  const center = await request(`/tenants/${state.tenantId}/administration-center`, { loadingContext: "tenant-administration" });
  const tenant = center.tenant;
  const metrics = center.metrics || {};
  const timeline = center.timeline || [];
  const health = center.health || { overallStatus: "Unknown", signals: [], backups: [] };
  const alerts = [
    ...(health.overallStatus && health.overallStatus !== "Healthy" && health.overallStatus !== 0 ? [{ severity: "warning", title: "Health Center requiere atencion", message: "Revisar senales operativas degradadas o faltantes." }] : []),
    ...((center.domains || []).length ? [] : [{ severity: "warning", title: "Sin dominios configurados", message: "Agregar un dominio default o custom antes de onboarding enterprise." }]),
    ...((center.ssoConfigurations || []).length ? [] : [{ severity: "info", title: "Sin SSO configurado", message: "Los tenants enterprise normalmente requieren OIDC, SAML, LDAP o Active Directory." }])
  ];
  const storageGb = ((metrics.storageBytes || 0) / 1024 / 1024 / 1024).toFixed(2);
  const usedUsers = `${metrics.users || 0}/${tenant.subscription.maxUsers}`;
  const storageUse = `${storageGb}/${tenant.subscription.maxStorageGb} GB`;
  const statusName = tenantStatusName(tenant.status);
  const planName = subscriptionPlanName(tenant.subscription.plan);
  const subscriptionStatusName = subscriptionStateName(tenant.subscription.status);
  const activeTenantTab = localStorage.getItem("c360.tenantTab") || "general";

  content.innerHTML = `
    ${pageHeader("Tenant Administration Center", "Centro enterprise para administrar identidad comercial, seguridad, licenciamiento, integraciones, auditoria y estado operativo del tenant.", "Enterprise / Administration")}
    ${tenantAdministrationNavigation(tenant)}
    <section class="tenant-hero">
      <div>
        <span class="product-badge">Tenant Core</span>
        <h2>${safe(tenant.commercialName || tenant.name)}</h2>
        <p>${safe(tenant.legalName)} · ${safe(tenant.taxIdentifier)} · ${safe(tenant.countryCode)} · ${safe(tenant.currency)}</p>
      </div>
      <div class="tenant-health">
        <span class="status-pill ${statusName === "Activo" ? "ok" : statusName === "Archivado" ? "danger" : "warn"}">${safe(statusName)}</span>
        <strong>${metrics.health ? "Saludable" : "Requiere atencion"}</strong>
        <small>Security score ${tenant.settings.securityScore}/100</small>
      </div>
    </section>
    <section class="grid cards tenant-dashboard-grid">
      ${metric("Tenant", shortId(tenant.id), "Identificador inmutable")}
      ${metric("Estado", statusName, "Lifecycle SaaS")}
      ${metric("Plan", planName, "Suscripcion")}
      ${metric("Licencia", subscriptionStatusName, "Estado comercial")}
      ${metric("Usuarios", usedUsers, "Activos: " + (metrics.activeUsers || 0))}
      ${metric("Storage", storageUse, "Uso real del tenant")}
      ${metric("Documentos", metrics.documents || 0, "Control documental")}
      ${metric("Proveedores", metrics.suppliers || 0, "Supplier Management")}
      ${metric("Auditorias", metrics.audits || 0, "Audit Management")}
      ${metric("CAPA", metrics.capas || 0, "Acciones abiertas/cerradas")}
      ${metric("Riesgos", metrics.risks || 0, "Risk Management")}
      ${metric("Indicadores", metrics.indicators || 0, "Quality KPIs")}
      ${metric("Notificaciones", metrics.notifications || 0, "Mensajes tenant")}
      ${metric("Health", metrics.health ? "OK" : "Review", "Estado operativo")}
      ${metric("Ultimo Backup", formatDate(metrics.lastBackupAtUtc), "Pendiente proveedor backup")}
      ${metric("Ultimo Login", formatDate(metrics.lastLoginAtUtc), "Actividad reciente")}
    </section>
    ${tenantAlerts(alerts)}
    <section class="tenant-admin-shell">
      <aside class="tenant-tabs" aria-label="Tenant Administration tabs">
        ${tenantAdminTabs().map((tab, index) => `<button class="tenant-tab ${index === 0 ? "active" : ""}" type="button" data-tab="${tab.key}"><span>${index + 1}</span>${tab.label}</button>`).join("")}
      </aside>
      <div class="tenant-tab-panels">
        ${tenantGeneralPanel(tenant)}
        ${tenantBrandingPanel(tenant)}
        ${tenantSecurityPanel(tenant)}
        ${tenantUsersPanel(center.users || { users: [], roles: [] }, tenant, metrics)}
        ${tenantRbacPanel(center.users || { users: [], roles: [] })}
        ${tenantLicensingPanel(tenant, metrics, center.license)}
        ${tenantDomainsPanel(center.domains || [])}
        ${tenantSsoPanel(center.ssoConfigurations || [])}
        ${tenantApiKeysPanel(center.apiCredentials || [])}
        ${tenantWebhooksPanel(center.webhooks || [])}
        ${tenantStoragePanel(metrics)}
        ${tenantNotificationsPanel(metrics)}
        ${tenantHealthPanel(health)}
        ${tenantAuditPanel(timeline)}
        ${tenantStatePanel(tenant)}
      </div>
    </section>`;

  applyTenantActiveTab(activeTenantTab);
  bindTenantAdministrationCenter(tenant, center);
}

function tenantAdministrationNavigation(tenant) {
  const cameFromPlatform = localStorage.getItem("c360.tenantReturn") === "superadmin-platform";
  return `
    <section class="platform-command" aria-label="Navegacion de tenants">
      <button class="btn" type="button" id="back-to-platform-tenants">${cameFromPlatform ? "Volver a Tenants" : "Ver todos los tenants"}</button>
      <button class="btn" type="button" id="back-to-superadmin-platform">SuperAdmin Platform</button>
      <span class="metric-label">Tenant actual: ${safe(tenant.commercialName || tenant.name)} · ${shortId(tenant.id)}</span>
    </section>`;
}

function tenantAdminTabs() {
  return [
    { key: "general", label: "Informacion General" },
    { key: "branding", label: "Branding" },
    { key: "security", label: "Seguridad" },
    { key: "users", label: "Usuarios" },
    { key: "rbac", label: "Roles & Permisos" },
    { key: "licensing", label: "Licenciamiento" },
    { key: "domains", label: "Dominios" },
    { key: "sso", label: "SSO" },
    { key: "apikeys", label: "API Keys" },
    { key: "webhooks", label: "Webhooks" },
    { key: "storage", label: "Storage" },
    { key: "notifications", label: "Notificaciones" },
    { key: "health", label: "Health & Backups" },
    { key: "audit", label: "Auditoria" },
    { key: "state", label: "Estado" }
  ];
}

function tenantGeneralPanel(tenant) {
  return `
    <section class="tenant-panel active" data-panel="general">
      <div class="section-heading"><div><h2 class="section-title">Informacion General</h2><p class="metric-label">Campos empresariales editables con TenantId y CreatedAt inmutables.</p></div><span class="status-pill ok">Auditable</span></div>
      <form id="tenant-general-form" class="form-stack">
        <div class="grid two">
          ${inputField("name", "Tenant Name", tenant.name, "text", false, true, { maxLength: 180, help: "Nombre interno del tenant. No puede estar vacío." })}
          ${inputField("legalName", "Razon Social", tenant.legalName, "text", false, true, { maxLength: 220, help: "Nombre legal que aparece en contratos y facturación." })}
          ${inputField("commercialName", "Nombre Comercial", tenant.commercialName, "text", false, true, { maxLength: 180, help: "Nombre visible para usuarios del cliente." })}
          ${inputField("taxIdentifier", "RUC / Tax ID", tenant.taxIdentifier, "text", false, true, { maxLength: 80, pattern: "^[A-Za-z0-9][A-Za-z0-9.\\-]{2,78}[A-Za-z0-9]$", title: "Use letras, números, guiones o puntos. No use símbolos especiales.", help: "Identificación fiscal. Se normaliza a mayúsculas." })}
          ${inputField("industry", "Industria", tenant.industry || "Compliance", "text", false, true, { maxLength: 120, help: "Sector principal del cliente." })}
          ${selectField("countryCode", "Pais", tenant.countryCode, countryOptions(), "Código ISO del país. Panamá = PA.")}
          ${selectField("currency", "Moneda", tenant.currency, currencyOptions(), "Moneda ISO-4217 usada para el tenant.")}
          ${inputField("phone", "Telefono", tenant.phone || "", "tel", false, false, { placeholder: "+507 6000-0000", pattern: "^\\+?(?:\\d| |\\(|\\)|-){7,40}(?:\\s?(?:ext\\.?|x)\\s?\\d{1,8})?$", title: "Use un teléfono válido. Ejemplo: +507 6000-0000", help: "Puede incluir código de país y extensión." })}
          ${inputField("email", "Correo", tenant.email || "", "email", false, false, { maxLength: 180, placeholder: "contacto@empresa.com", help: "Correo corporativo del tenant. Se normaliza a minúsculas." })}
          ${inputField("website", "Sitio Web", tenant.website || "", "url", false, false, { maxLength: 250, placeholder: "https://empresa.com", help: "Debe iniciar con http:// o https://." })}
          ${inputField("city", "Ciudad", tenant.city || "", "text", false, false, { maxLength: 120 })}
          ${inputField("province", "Provincia", tenant.province || "", "text", false, false, { maxLength: 120 })}
          ${inputField("postalCode", "Codigo Postal", tenant.postalCode || "", "text", false, false, { maxLength: 20, help: "Formato dependiente del país." })}
          ${inputField("addressLine1", "Direccion", tenant.addressLine1 || "", "text", false, false, { maxLength: 220 })}
        </div>
        <div class="field"><label for="description">Descripcion</label><textarea id="description" name="description" rows="3">${safe(tenant.description || "")}</textarea></div>
        <div class="field"><label for="generalChangeReason">Motivo del cambio</label><input id="generalChangeReason" name="changeReason" placeholder="Opcional, queda en auditoria"></div>
        <div class="button-row"><button class="btn primary" type="submit">Guardar informacion general</button><span class="tag">TenantId inmutable: ${shortId(tenant.id)}</span></div>
      </form>
    </section>`;
}

function tenantBrandingPanel(tenant) {
  return `
    <section class="tenant-panel" data-panel="branding">
      <div class="section-heading"><div><h2 class="section-title">Branding</h2><p class="metric-label">Identidad visual del tenant, sin estilos inline y con soporte dark mode.</p></div><span class="status-pill ok">Premium UI</span></div>
      <form id="tenant-branding-form" class="form-stack">
        <div class="tenant-brand-preview">
          <div class="brand-token">${safe((tenant.branding.displayName || tenant.name).slice(0, 2).toUpperCase())}</div>
          <div><strong>${safe(tenant.branding.displayName)}</strong><p>${safe(tenant.branding.footerText || "Compliance 360")}</p></div>
        </div>
        <div class="grid two">
          ${inputField("displayName", "Nombre mostrado", tenant.branding.displayName, "text", false, true, { maxLength: 180 })}
          ${inputField("logoUri", "Logo", tenant.branding.logoUri || "", "url", false, false, { placeholder: "https://empresa.com/logo.png", help: "URL absoluta del logo. Para archivos, valide peso, dimensiones y MIME antes de usarlo en producción." })}
          ${inputField("faviconUri", "Favicon", tenant.branding.faviconUri || "", "url", false, false, { placeholder: "https://empresa.com/favicon.ico" })}
          ${inputField("primaryColor", "Color primario", tenant.branding.primaryColor, "color", false, true, { help: "Seleccione un color. Se guarda como HEX." })}
          ${inputField("secondaryColor", "Color secundario", tenant.branding.secondaryColor, "color", false, true, { help: "Seleccione un color secundario. Se guarda como HEX." })}
          ${selectField("theme", "Tema", tenant.branding.theme || "System", [["System", "Sistema"], ["Light", "Claro"], ["Dark", "Oscuro"]], "Tema visual permitido.")}
          ${inputField("loginBackgroundUri", "Pantalla Login", tenant.branding.loginBackgroundUri || "", "url", false, false, { placeholder: "https://empresa.com/login-background.jpg" })}
          ${inputField("corporateEmail", "Correo corporativo", tenant.branding.corporateEmail || "", "email", false, false, { placeholder: "soporte@empresa.com" })}
          ${inputField("footerText", "Pie de pagina", tenant.branding.footerText || "Compliance 360", "text", false, false, { maxLength: 160 })}
        </div>
        <div class="field"><label for="brandingChangeReason">Motivo del cambio</label><input id="brandingChangeReason" name="changeReason" placeholder="Opcional"></div>
        <button class="btn primary" type="submit">Guardar branding</button>
      </form>
    </section>`;
}

function tenantSecurityPanel(tenant) {
  return `
    <section class="tenant-panel" data-panel="security">
      <div class="section-heading"><div><h2 class="section-title">Seguridad</h2><p class="metric-label">MFA, password policy, sesiones, lockout, IP whitelist y trusted devices.</p></div><span class="status-pill ${tenant.settings.securityScore >= 80 ? "ok" : "warn"}">${tenant.settings.securityScore}/100</span></div>
      <form id="tenant-security-form" class="form-stack">
        <div class="grid two">
          <label class="toggle-row"><input type="checkbox" name="requireMfa" ${tenant.settings.requireMfa ? "checked" : ""}> MFA obligatorio</label>
          <label class="toggle-row"><input type="checkbox" name="trustedDevicesEnabled" ${tenant.settings.trustedDevicesEnabled ? "checked" : ""}> Trusted Devices</label>
          ${inputField("sessionTimeoutMinutes", "Session Timeout (min)", tenant.settings.sessionTimeoutMinutes, "number", false, true, { min: 5, max: 1440, help: "Rango permitido: 5 a 1440 minutos." })}
          ${inputField("passwordExpirationDays", "Password Expiration (dias)", tenant.settings.passwordExpirationDays, "number", false, true, { min: 0, max: 730, help: "Use 0 para no expirar contraseñas." })}
          ${inputField("lockoutMaxFailedAttempts", "Lockout intentos", tenant.settings.lockoutMaxFailedAttempts, "number", false, true, { min: 1, max: 25 })}
          ${inputField("lockoutMinutes", "Lockout minutos", tenant.settings.lockoutMinutes, "number", false, true, { min: 1, max: 1440 })}
          ${inputField("securityScore", "Security Score", tenant.settings.securityScore, "number", false, true, { min: 0, max: 100 })}
        </div>
        <div class="field"><label for="ipWhitelist">IP Whitelist</label><textarea id="ipWhitelist" name="ipWhitelist" rows="3" placeholder="192.168.1.0/24, 10.0.0.1/32" aria-describedby="ipWhitelist-help">${safe(tenant.settings.ipWhitelist || "")}</textarea><small id="ipWhitelist-help">Lista CIDR separada por comas. Déjelo vacío para no restringir por IP.</small></div>
        <div class="field"><label for="securityChangeReason">Motivo del cambio</label><input id="securityChangeReason" name="changeReason" placeholder="Opcional"></div>
        <button class="btn primary" type="submit">Guardar seguridad</button>
      </form>
    </section>`;
}

function tenantUsersPanel(userState, tenant, metrics) {
  const users = userState.users || [];
  const roles = userState.roles || [];
  return `
    <section class="tenant-panel" data-panel="users">
      <div class="section-heading"><div><h2 class="section-title">User Administration</h2><p class="metric-label">Crear, bloquear, desbloquear, reset MFA, roles y sesiones por tenant.</p></div><span class="status-pill ok">${users.length}/${metrics.users || 0}</span></div>
      <form id="tenant-user-form" class="form-stack">
        <div class="grid two">
          ${inputField("email", "Email", "", "email", false, true)}
          ${inputField("fullName", "Nombre completo", "", "text", false, true)}
          ${inputField("initialPassword", "Password inicial", "", "password", false, true, { minLength: 12, help: "Mínimo 12 caracteres con mayúscula, minúscula, número y símbolo." })}
          <div class="field"><label for="roleId">Rol inicial</label><select id="roleId" name="roleId"><option value="">Sin rol</option>${roles.map(role => `<option value="${role.id}">${safe(role.name)}</option>`).join("")}</select></div>
          <label class="toggle-row"><input type="checkbox" name="forcePasswordChange" checked> Forzar cambio de password</label>
          ${inputField("changeReason", "Motivo", "Alta operativa TAC")}
        </div>
        <button class="btn primary" type="submit">Crear / Invitar usuario</button>
      </form>
      ${tableCard("Usuarios del tenant", users.map(user => ({
        email: user.email,
        name: user.fullName,
        status: enumName(["Invited", "Active", "Disabled", "Locked"], user.status),
        mfa: user.mfaEnabled ? "Enabled" : "Disabled",
        lastLogin: formatDate(user.lastLoginAtUtc),
        sessions: (user.sessions || []).filter(session => session.isActive).length,
        actions: `__html:<button class="btn small" data-user-action="Active" data-user-id="${user.id}">Unlock</button> <button class="btn small" data-user-action="Disabled" data-user-id="${user.id}">Disable</button> <button class="btn small" data-user-mfa="${user.id}">Reset MFA</button> <button class="btn small" data-user-sessions="${user.id}">Close Sessions</button>`
      })), ["email", "name", "status", "mfa", "lastLogin", "sessions", "actions"])}
    </section>`;
}

function tenantRbacPanel(userState) {
  const users = userState.users || [];
  const roles = userState.roles || [];
  return `
    <section class="tenant-panel" data-panel="rbac">
      <div class="section-heading">
        <div>
          <h2 class="section-title">RBAC - Roles y Permisos</h2>
          <p class="metric-label">Crear roles, crear permisos y otorgarlos sin salir del navegador.</p>
        </div>
        <span class="status-pill ok">RBAC.MANAGE</span>
      </div>
      <div class="grid two">
        <form id="tenant-role-form" class="form-stack card">
          <h3>Crear rol</h3>
          ${inputField("name", "Nombre del rol", "Quality Manager", "text", false, true, { maxLength: 120 })}
          <label class="toggle-row"><input type="checkbox" name="isSystemRole"> Rol de sistema</label>
          <button class="btn primary" type="submit">Crear rol</button>
        </form>
        <form id="tenant-permission-grant-form" class="form-stack card">
          <h3>Crear permiso y otorgarlo</h3>
          <div class="field"><label for="permissionRoleId">Rol</label><select id="permissionRoleId" name="roleId" required><option value="">Seleccione rol</option>${roles.map(role => `<option value="${role.id}">${safe(role.name)}</option>`).join("")}</select></div>
          ${inputField("module", "Modulo", "DOCUMENT", "text", false, true, { maxLength: 80, pattern: "^[A-Za-z][A-Za-z0-9_]{1,79}$", title: "Use letras, numeros o guion bajo. Ejemplo: DOCUMENT." })}
          <div class="field"><label for="action">Accion</label><select id="action" name="action" required>${permissionActionOptions()}</select></div>
          ${inputField("description", "Descripcion", "Permiso creado desde TAC RBAC", "text", false, true, { maxLength: 250 })}
          <button class="btn primary" type="submit">Crear permiso y otorgar</button>
        </form>
      </div>
      <form id="tenant-role-assign-form" class="form-stack card">
        <h3>Asignar rol a usuario</h3>
        <div class="grid two">
          <div class="field"><label for="assignUserId">Usuario</label><select id="assignUserId" name="userId" required><option value="">Seleccione usuario</option>${users.map(user => `<option value="${user.id}">${safe(user.email)} - ${safe(user.fullName)}</option>`).join("")}</select></div>
          <div class="field"><label for="assignRoleId">Rol</label><select id="assignRoleId" name="roleId" required><option value="">Seleccione rol</option>${roles.map(role => `<option value="${role.id}">${safe(role.name)}</option>`).join("")}</select></div>
        </div>
        <button class="btn primary" type="submit">Asignar rol</button>
      </form>
      ${tableCard("Roles del tenant", roles.map(role => ({
        name: role.name,
        system: role.isSystemRole ? "Si" : "No",
        id: shortId(role.id)
      })), ["name", "system", "id"])}
    </section>`;
}

function tenantLicensingPanel(tenant, metrics) {
  const subscriptionStatusName = subscriptionStateName(tenant.subscription.status);
  return `
    <section class="tenant-panel" data-panel="licensing">
      <div class="section-heading"><div><h2 class="section-title">Licenciamiento</h2><p class="metric-label">Campos sensibles protegidos por TENANT.BILLING.</p></div><span class="status-pill ok">${safe(subscriptionStatusName)}</span></div>
      <form id="tenant-licensing-form" class="form-stack">
        <div class="grid two">
          <div class="field"><label for="plan">Plan</label><select id="plan" name="plan">${enumOptions(["Starter", "Professional", "Enterprise", "Dedicated"], tenant.subscription.plan)}</select></div>
          <div class="field"><label for="subscriptionStatus">Estado licencia</label><select id="subscriptionStatus" name="status">${enumOptions(["Trial", "Active", "PastDue", "Suspended", "Cancelled"], tenant.subscription.status)}</select></div>
          ${inputField("maxUsers", "Usuarios contratados", tenant.subscription.maxUsers, "number")}
          ${inputField("usersUsed", "Usuarios usados", metrics.users || 0, "number", true)}
          ${inputField("maxStorageGb", "Storage contratado (GB)", tenant.subscription.maxStorageGb, "number")}
          ${inputField("storageUsed", "Storage usado (bytes)", metrics.storageBytes || 0, "number", true)}
          ${inputField("expiresOn", "Fecha expiracion", tenant.subscription.expiresOn || "", "date")}
        </div>
        <div class="field"><label for="billingChangeReason">Motivo del cambio</label><input id="billingChangeReason" name="changeReason" placeholder="Opcional"></div>
        <button class="btn primary" type="submit">Guardar licenciamiento</button>
      </form>
    </section>`;
}

function tenantStoragePanel(metrics) {
  const rows = ["Azure Blob", "AWS S3", "MinIO", "Local"].map(provider => ({ provider, status: "Supported", configured: metrics.storageProviders || 0 }));
  return `<section class="tenant-panel" data-panel="storage">${tableCard("Storage providers", rows, ["provider", "status", "configured"])}<div class="button-row"><button class="btn" data-route="configuration">Abrir Provider Administration</button></div></section>`;
}

function tenantNotificationsPanel(metrics) {
  const rows = ["Gmail", "Microsoft 365", "SendGrid", "Mailgun", "Resend"].map(provider => ({ provider, status: "Supported", configured: metrics.notificationProviders || 0 }));
  return `<section class="tenant-panel" data-panel="notifications">${tableCard("Notification providers", rows, ["provider", "status", "configured"])}<div class="button-row"><button class="btn" data-route="configuration">Configurar SMTP / Failover</button></div></section>`;
}

function tenantDomainsPanel(domains) {
  return `
    <section class="tenant-panel" data-panel="domains">
      <div class="section-heading"><div><h2 class="section-title">Dominios</h2><p class="metric-label">Principal, secundarios, subdominios, aliases, DNS, certificados, HTTPS y redirecciones.</p></div><span class="status-pill ok">TENANT.DOMAINS</span></div>
      <form id="tenant-domain-form" class="form-stack">
        <div class="grid two">
          ${inputField("hostName", "Dominio", "", "text", false, true)}
          <div class="field"><label for="domainKind">Tipo</label><select id="domainKind" name="kind">${enumOptions(["Default", "PrimaryCustom", "SecondaryCustom", "Subdomain", "Alias"], 1)}</select></div>
          <label class="toggle-row"><input type="checkbox" name="isDefault"> Dominio por defecto</label>
          ${inputField("redirectToHostName", "Redirecciona a", "")}
          ${inputField("changeReason", "Motivo", "Alta de dominio enterprise")}
        </div>
        <button class="btn primary" type="submit">Guardar dominio</button>
      </form>
      ${tableCard("Dominios configurados", domains.map(domain => ({
        host: domain.hostName,
        kind: enumName(["Default", "PrimaryCustom", "SecondaryCustom", "Subdomain", "Alias"], domain.kind),
        status: enumName(["PendingVerification", "Verified", "DnsFailed", "CertificateFailed", "Disabled"], domain.status),
        dns: domain.dnsStatus,
        cert: enumName(["NotRequested", "Pending", "Issued", "Expired", "Failed"], domain.certificateStatus),
        https: domain.httpsEnabled ? "Enabled" : "Disabled",
        token: domain.verificationToken,
        actions: `__html:<button class="btn small danger" data-domain-disable="${domain.id}">Disable</button>`
      })), ["host", "kind", "status", "dns", "cert", "https", "token", "actions"])}
    </section>`;
}

function tenantSsoPanel(configurations) {
  return `
    <section class="tenant-panel" data-panel="sso">
      <div class="section-heading"><div><h2 class="section-title">SSO Enterprise</h2><p class="metric-label">OIDC, OAuth2, SAML, LDAP, Active Directory, mappings, JIT y SCIM readiness.</p></div><span class="status-pill ok">TENANT.SSO</span></div>
      <form id="tenant-sso-form" class="form-stack">
        <div class="grid two">
          <div class="field"><label for="ssoProvider">Provider</label><select id="ssoProvider" name="provider">${enumOptions(["Oidc", "OAuth2", "Saml", "Ldap", "ActiveDirectory"], 0)}</select></div>
          ${inputField("name", "Nombre", "Corporate SSO", "text", false, true)}
          ${inputField("authority", "Authority / Issuer", "", "text", false, true)}
          ${inputField("metadataUrl", "Metadata URL", "", "url")}
          ${inputField("clientId", "Client ID", "", "text", false, true)}
          ${inputField("secretReference", "Secret reference", "")}
          ${inputField("certificateThumbprint", "Certificate thumbprint", "")}
          <label class="toggle-row"><input type="checkbox" name="jitProvisioningEnabled" checked> JIT provisioning</label>
          <label class="toggle-row"><input type="checkbox" name="scimEnabled"> SCIM habilitado</label>
          <label class="toggle-row"><input type="checkbox" name="enabled" checked> Enabled</label>
        </div>
        <div class="field"><label>Claims Mapping JSON</label><textarea name="claimsMappingJson" rows="3">{"email":"email","name":"name"}</textarea></div>
        <div class="field"><label>Role Mapping JSON</label><textarea name="roleMappingJson" rows="3">{"ComplianceAdmin":"Administrator"}</textarea></div>
        ${inputField("changeReason", "Motivo", "Configuracion SSO enterprise")}
        <button class="btn primary" type="submit">Guardar SSO</button>
      </form>
      ${tableCard("SSO configurado", configurations.map(sso => ({
        provider: enumName(["Oidc", "OAuth2", "Saml", "Ldap", "ActiveDirectory"], sso.provider),
        name: sso.name,
        status: enumName(["Draft", "Enabled", "Disabled", "Failed", "Revoked"], sso.status),
        health: enumName(["Healthy", "Degraded", "Unhealthy", "Unknown"], sso.healthStatus),
        jit: sso.jitProvisioningEnabled ? "Yes" : "No",
        scim: sso.scimEnabled ? "Yes" : "No",
        actions: `__html:<button class="btn small" data-sso-test="${sso.id}">Test</button>`
      })), ["provider", "name", "status", "health", "jit", "scim", "actions"])}
    </section>`;
}

function tenantApiKeysPanel(apiKeys) {
  return `
    <section class="tenant-panel" data-panel="apikeys">
      <div class="section-heading"><div><h2 class="section-title">API Keys & Service Accounts</h2><p class="metric-label">Scopes, expiracion, rotacion, revocacion y secretos hasheados.</p></div><span class="status-pill ok">TENANT.API_KEYS</span></div>
      <form id="tenant-api-key-form" class="form-stack">
        <div class="grid two">
          ${inputField("name", "Service account", "", "text", false, true)}
          ${inputField("plainTextSecret", "Secret inicial", "", "password", false, true)}
          ${inputField("scopes", "Scopes", "tenant.read tenant.audit")}
          ${inputField("expiresAtUtc", "Expira", "", "datetime-local")}
          ${inputField("changeReason", "Motivo", "Alta API key")}
        </div>
        <button class="btn primary" type="submit">Crear API Key</button>
      </form>
      ${tableCard("Credenciales", apiKeys.map(key => ({
        name: key.name,
        prefix: key.keyPrefix,
        scopes: key.scopes,
        expires: formatDate(key.expiresAtUtc),
        status: enumName(["Active", "Expired", "Revoked", "Rotated"], key.status),
        lastUsed: formatDate(key.lastUsedAtUtc),
        actions: `__html:<button class="btn small" data-api-rotate="${key.id}">Rotate</button> <button class="btn small danger" data-api-revoke="${key.id}">Revoke</button>`
      })), ["name", "prefix", "scopes", "expires", "status", "lastUsed", "actions"])}
    </section>`;
}

function tenantWebhooksPanel(webhooks) {
  return `
    <section class="tenant-panel" data-panel="webhooks">
      <div class="section-heading"><div><h2 class="section-title">Webhooks</h2><p class="metric-label">Eventos, firmas, retry, dead letter, estado e historial de pruebas.</p></div><span class="status-pill ok">TENANT.WEBHOOKS</span></div>
      <form id="tenant-webhook-form" class="form-stack">
        <div class="grid two">
          ${inputField("name", "Nombre", "", "text", false, true)}
          ${inputField("url", "Endpoint HTTPS", "", "url", false, true)}
          ${inputField("events", "Eventos", "tenant.updated,audit.created")}
          ${inputField("plainTextSecret", "Signing secret", "", "password", false, true)}
          ${inputField("maxRetries", "Retries", 5, "number")}
          <label class="toggle-row"><input type="checkbox" name="enabled" checked> Enabled</label>
          ${inputField("changeReason", "Motivo", "Alta webhook")}
        </div>
        <button class="btn primary" type="submit">Guardar webhook</button>
      </form>
      ${tableCard("Endpoints", webhooks.map(hook => ({
        name: hook.name,
        url: hook.url,
        events: hook.events,
        status: enumName(["Draft", "Enabled", "Disabled", "Failed", "Revoked"], hook.status),
        last: enumName(["Pending", "Succeeded", "Failed", "Retrying", "DeadLetter"], hook.lastDeliveryStatus),
        retries: hook.maxRetries,
        actions: `__html:<button class="btn small" data-webhook-test="${hook.id}">Test</button> <button class="btn small danger" data-webhook-disable="${hook.id}">Disable</button>`
      })), ["name", "url", "events", "status", "last", "retries", "actions"])}
    </section>`;
}

function tenantHealthPanel(health) {
  const signals = health.signals || [];
  const backups = health.backups || [];
  return `
    <section class="tenant-panel" data-panel="health">
      <div class="section-heading"><div><h2 class="section-title">Health Center & Backups</h2><p class="metric-label">DB, SMTP, Storage, Providers, Jobs, Queues, Integraciones, licencia, espacio, backups y OpenTelemetry.</p></div><span class="status-pill ${health.overallStatus === "Healthy" || health.overallStatus === 0 ? "ok" : "warn"}">${safe(enumName(["Healthy", "Degraded", "Unhealthy", "Unknown"], health.overallStatus))}</span></div>
      ${tableCard("Health checks", signals.map(signal => ({
        component: signal.component,
        status: enumName(["Healthy", "Degraded", "Unhealthy", "Unknown"], signal.status),
        message: signal.message,
        checked: formatDate(signal.checkedAtUtc),
        duration: signal.duration || "n/a"
      })), ["component", "status", "message", "checked", "duration"])}
      <form id="tenant-backup-form" class="form-stack">
        <div class="grid two">
          ${inputField("backupKind", "Tipo", "Database")}
          ${inputField("result", "Resultado", "Succeeded")}
          ${inputField("startedAtUtc", "Inicio UTC", dateTimeLocalValue(Date.now() - 60000), "datetime-local")}
          ${inputField("completedAtUtc", "Fin UTC", dateTimeLocalValue(Date.now()), "datetime-local")}
          ${inputField("sizeBytes", "Tamaño bytes", 0, "number")}
          ${inputField("rpoMinutes", "RPO minutos", 60, "number")}
          ${inputField("rtoMinutes", "RTO minutos", 240, "number")}
          ${inputField("message", "Mensaje", "Backup verification recorded")}
        </div>
        <button class="btn primary" type="submit">Registrar backup</button>
      </form>
      ${tableCard("Backups", backups.map(backup => ({
        kind: backup.backupKind,
        result: backup.result,
        completed: formatDate(backup.completedAtUtc),
        size: backup.sizeBytes,
        rpo: backup.rpo,
        rto: backup.rto,
        message: backup.message
      })), ["kind", "result", "completed", "size", "rpo", "rto", "message"])}
    </section>`;
}

function tenantAuditPanel(timeline) {
  return `
    <section class="tenant-panel" data-panel="audit">
      <div class="section-heading"><div><h2 class="section-title">Auditoria</h2><p class="metric-label">Timeline con usuario, accion, IP, CorrelationId y snapshots cuando el interceptor EF los captura.</p></div><button id="export-tenant-audit" class="btn" type="button">Exportar</button></div>
      <div class="tenant-timeline">
        ${(timeline.length ? timeline : [{ action: "TenantChanged", occurredAtUtc: new Date().toISOString(), entityName: "Tenant", correlationId: "n/a", metadataJson: "No timeline data yet" }]).map(item => `
          <article class="timeline-item">
            <span></span>
            <div><strong>${safe(item.action)} · ${safe(item.entityName)}</strong><p>${formatDate(item.occurredAtUtc)} · User ${shortId(item.userId)} · IP ${safe(item.ipAddress || "n/a")} · Corr ${safe(item.correlationId || "n/a")}</p><small>${safe(item.metadataJson || item.afterValuesJson || "Cambio auditado")}</small></div>
          </article>`).join("")}
      </div>
    </section>`;
}

function tenantStatePanel(tenant) {
  const statusName = tenantStatusName(tenant.status);
  return `
    <section class="tenant-panel" data-panel="state">
      <div class="section-heading"><div><h2 class="section-title">Estado</h2><p class="metric-label">Lifecycle Draft, Trial, Active, Suspended, Archived y Restore con permisos especiales.</p></div><span class="status-pill warn">TENANT.STATUS</span></div>
      <div class="tenant-state-flow"><span>Draft</span><span>Trial</span><span>Active</span><span>Suspended</span><span>Archived</span><span>Restore</span></div>
      <div class="button-row">
        <button class="btn" type="button" data-tenant-state="trial">Mover a Trial</button>
        <button class="btn primary" type="button" data-tenant-state="activate">Activar</button>
        <button class="btn" type="button" data-tenant-state="suspend">Suspender</button>
        <button class="btn danger" type="button" data-tenant-state="archive">Archivar</button>
        <button class="btn" type="button" data-tenant-state="restore">Restaurar</button>
      </div>
      <div class="empty-state"><strong>Campos inmutables protegidos</strong><p>TenantId, CreatedAt, CreatedBy, Audit Trail e historial no se exponen como editables. Estado actual: ${safe(statusName)}.</p></div>
    </section>`;
}

function bindTenantAdministrationCenter(tenant, center) {
  document.querySelector("#back-to-platform-tenants")?.addEventListener("click", () => {
    localStorage.setItem("c360.platformTab", "tenants");
    localStorage.removeItem("c360.tenantReturn");
    location.hash = "#/superadmin-platform";
  });

  document.querySelector("#back-to-superadmin-platform")?.addEventListener("click", () => {
    localStorage.setItem("c360.platformTab", "executive");
    localStorage.removeItem("c360.tenantReturn");
    location.hash = "#/superadmin-platform";
  });

  document.querySelectorAll(".tenant-tab").forEach(tab => {
    tab.addEventListener("click", () => {
      localStorage.setItem("c360.tenantTab", tab.dataset.tab);
      applyTenantActiveTab(tab.dataset.tab);
    });
  });

  bindTenantForm("#tenant-general-form", event => request(`/tenants/${state.tenantId}/general-information`, { method: "PUT", loadingContext: "save", body: formObject(new FormData(event.currentTarget)) }));
  bindTenantForm("#tenant-branding-form", event => request(`/tenants/${state.tenantId}/branding`, { method: "PUT", loadingContext: "save", body: formObject(new FormData(event.currentTarget)) }));
  bindTenantForm("#tenant-security-form", event => {
    const form = new FormData(event.currentTarget);
    const body = formObject(form);
    body.requireMfa = form.has("requireMfa");
    body.trustedDevicesEnabled = form.has("trustedDevicesEnabled");
    ["sessionTimeoutMinutes", "passwordExpirationDays", "lockoutMaxFailedAttempts", "lockoutMinutes", "securityScore"].forEach(key => body[key] = Number(body[key]));
    return request(`/tenants/${state.tenantId}/security`, { method: "PUT", loadingContext: "save", body });
  });
  bindTenantForm("#tenant-licensing-form", event => {
    const form = new FormData(event.currentTarget);
    return request(`/tenants/${state.tenantId}/subscription`, {
      method: "PUT",
      loadingContext: "save",
      body: {
        plan: Number(form.get("plan")),
        maxUsers: Number(form.get("maxUsers")),
        maxStorageGb: Number(form.get("maxStorageGb")),
        status: Number(form.get("status")),
        expiresOn: form.get("expiresOn") || null,
        changeReason: form.get("changeReason") || null
      }
    });
  });

  bindTenantForm("#tenant-domain-form", event => {
    const form = new FormData(event.currentTarget);
    const body = formObject(form);
    body.kind = Number(body.kind);
    body.isDefault = form.has("isDefault");
    return request(`/tenants/${state.tenantId}/domains`, { method: "PUT", loadingContext: "save", body });
  });

  bindTenantForm("#tenant-sso-form", event => {
    const form = new FormData(event.currentTarget);
    const body = formObject(form);
    body.provider = Number(body.provider);
    body.jitProvisioningEnabled = form.has("jitProvisioningEnabled");
    body.scimEnabled = form.has("scimEnabled");
    body.enabled = form.has("enabled");
    return request(`/tenants/${state.tenantId}/sso`, { method: "PUT", loadingContext: "save", body });
  });

  bindTenantForm("#tenant-api-key-form", event => request(`/tenants/${state.tenantId}/api-keys`, { method: "POST", loadingContext: "save", body: formObject(new FormData(event.currentTarget)) }));

  bindTenantForm("#tenant-webhook-form", event => {
    const form = new FormData(event.currentTarget);
    const body = formObject(form);
    body.maxRetries = Number(body.maxRetries);
    body.enabled = form.has("enabled");
    return request(`/tenants/${state.tenantId}/webhooks`, { method: "PUT", loadingContext: "save", body });
  });

  bindTenantForm("#tenant-backup-form", event => {
    const body = formObject(new FormData(event.currentTarget));
    body.sizeBytes = Number(body.sizeBytes);
    body.rpoMinutes = Number(body.rpoMinutes);
    body.rtoMinutes = Number(body.rtoMinutes);
    return request(`/tenants/${state.tenantId}/backups`, { method: "POST", loadingContext: "save", body });
  });

  bindTenantForm("#tenant-user-form", event => {
    const form = new FormData(event.currentTarget);
    const body = formObject(form);
    body.forcePasswordChange = form.has("forcePasswordChange");
    return request(`/tenants/${state.tenantId}/users`, { method: "POST", loadingContext: "save", body });
  });

  bindTenantForm("#tenant-role-form", event => {
    const form = new FormData(event.currentTarget);
    return request(`/tenants/${state.tenantId}/rbac/roles`, {
      method: "POST",
      loadingContext: "save",
      body: {
        name: String(form.get("name") || "").trim(),
        isSystemRole: form.has("isSystemRole")
      }
    });
  });

  bindTenantForm("#tenant-permission-grant-form", async event => {
    const form = new FormData(event.currentTarget);
    const permission = await request(`/tenants/${state.tenantId}/rbac/permissions`, {
      method: "POST",
      loadingContext: "save",
      body: {
        module: String(form.get("module") || "").trim().toUpperCase(),
        action: Number(form.get("action")),
        description: String(form.get("description") || "").trim()
      }
    });
    return request(`/tenants/${state.tenantId}/rbac/permissions/grant`, {
      method: "POST",
      loadingContext: "save",
      body: {
        roleId: form.get("roleId"),
        permissionId: permission.id
      }
    });
  });

  bindTenantForm("#tenant-role-assign-form", event => {
    const form = new FormData(event.currentTarget);
    return request(`/tenants/${state.tenantId}/rbac/roles/assign`, {
      method: "POST",
      loadingContext: "save",
      body: {
        userId: form.get("userId"),
        roleId: form.get("roleId")
      }
    });
  });

  document.querySelectorAll("[data-tenant-state]").forEach(button => {
    button.addEventListener("click", async event => {
      try {
        setLoadingButton(event.currentTarget, true, "Actualizando...");
        await request(`/tenants/${state.tenantId}/${event.currentTarget.dataset.tenantState}`, { method: "POST", body: {}, loadingContext: "save" });
        toast("Estado del tenant actualizado.", "success");
        await renderRoute();
      } finally {
        setLoadingButton(event.currentTarget, false);
      }
    });
  });

  document.querySelector("#export-tenant-audit")?.addEventListener("click", async () => {
    await downloadProtectedText(`/tenants/${state.tenantId}/audit-timeline/export?page=1&pageSize=200`, "tenant-audit-timeline.csv");
  });

  bindTenantAction("[data-domain-disable]", button => request(`/tenants/${state.tenantId}/domains/${button.dataset.domainDisable}?changeReason=Disabled%20from%20TAC%20UI`, { method: "DELETE", loadingContext: "save" }));
  bindTenantAction("[data-sso-test]", button => request(`/tenants/${state.tenantId}/sso/${button.dataset.ssoTest}/test`, { method: "POST", body: { changeReason: "Connection test from TAC UI" }, loadingContext: "save" }));
  bindTenantAction("[data-api-revoke]", button => request(`/tenants/${state.tenantId}/api-keys/${button.dataset.apiRevoke}?changeReason=Revoked%20from%20TAC%20UI`, { method: "DELETE", loadingContext: "save" }));
  bindTenantAction("[data-api-rotate]", button => request(`/tenants/${state.tenantId}/api-keys/${button.dataset.apiRotate}/rotate`, { method: "POST", body: { plainTextSecret: crypto.randomUUID(), expiresAtUtc: null, changeReason: "Rotated from TAC UI" }, loadingContext: "save" }));
  bindTenantAction("[data-webhook-test]", button => request(`/tenants/${state.tenantId}/webhooks/${button.dataset.webhookTest}/test`, { method: "POST", body: { changeReason: "Webhook test from TAC UI" }, loadingContext: "save" }));
  bindTenantAction("[data-webhook-disable]", button => request(`/tenants/${state.tenantId}/webhooks/${button.dataset.webhookDisable}?changeReason=Disabled%20from%20TAC%20UI`, { method: "DELETE", loadingContext: "save" }));
  bindTenantAction("[data-user-mfa]", button => request(`/tenants/${state.tenantId}/users/${button.dataset.userMfa}/reset-mfa`, { method: "POST", body: { changeReason: "Reset MFA from TAC UI" }, loadingContext: "save" }));
  bindTenantAction("[data-user-sessions]", button => request(`/tenants/${state.tenantId}/users/${button.dataset.userSessions}/sessions/close`, { method: "POST", body: { changeReason: "Closed sessions from TAC UI" }, loadingContext: "save" }));
  bindTenantAction("[data-user-action]", button => request(`/tenants/${state.tenantId}/users/${button.dataset.userId}/status`, { method: "PATCH", body: { status: button.dataset.userAction === "Active" ? 1 : 2, changeReason: "Status update from TAC UI" }, loadingContext: "save" }));
  bindEnterpriseFieldValidation();
}

function applyTenantActiveTab(tabKey) {
  const selected = tenantAdminTabs().some(tab => tab.key === tabKey) ? tabKey : "general";
  document.querySelectorAll(".tenant-tab").forEach(tab => tab.classList.toggle("active", tab.dataset.tab === selected));
  document.querySelectorAll(".tenant-panel").forEach(panel => panel.classList.toggle("active", panel.dataset.panel === selected));
}

function bindTenantAction(selector, action) {
  document.querySelectorAll(selector).forEach(button => {
    button.addEventListener("click", async event => {
      try {
        setLoadingButton(event.currentTarget, true, "Procesando...");
        await action(event.currentTarget);
        toast("Accion ejecutada y auditada.", "success");
        await renderRoute();
      } finally {
        setLoadingButton(event.currentTarget, false);
      }
    });
  });
}

function bindTenantForm(selector, submit) {
  document.querySelector(selector)?.addEventListener("submit", async event => {
    event.preventDefault();
    const button = event.currentTarget.querySelector("button[type='submit']");
    try {
      setLoadingButton(button, true, "Guardando...");
      await submit(event);
      toast("Cambios guardados y auditados.", "success");
      await renderRoute();
    } finally {
      setLoadingButton(button, false);
    }
  });
}

function inputField(name, label, value, type = "text", disabled = false, required = false, options = {}) {
  const helpId = `${name}-help`;
  const min = options.min !== undefined ? ` min="${safe(options.min)}"` : type === "number" ? " min=\"0\"" : "";
  const max = options.max !== undefined ? ` max="${safe(options.max)}"` : "";
  const maxLength = options.maxLength !== undefined ? ` maxlength="${safe(options.maxLength)}"` : "";
  const minLength = options.minLength !== undefined ? ` minlength="${safe(options.minLength)}"` : "";
  const pattern = options.pattern ? ` pattern="${safe(options.pattern)}"` : name.toLowerCase().includes("color") && type !== "color" ? " pattern=\"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$\"" : "";
  const title = options.title ? ` title="${safe(options.title)}"` : "";
  const placeholder = options.placeholder ? ` placeholder="${safe(options.placeholder)}"` : "";
  const describedBy = options.help ? ` aria-describedby="${helpId}"` : "";
  const help = options.help ? `<small id="${helpId}">${safe(options.help)}</small>` : "";
  return `<div class="field"><label for="${name}">${safe(label)}</label><input id="${name}" name="${name}" type="${type}" value="${safe(value ?? "")}" ${disabled ? "disabled" : ""} ${required ? "required" : ""}${min}${max}${maxLength}${minLength}${pattern}${title}${placeholder}${describedBy}>${help}</div>`;
}

function selectField(name, label, selected, options, help = "") {
  const helpId = `${name}-help`;
  const describedBy = help ? ` aria-describedby="${helpId}"` : "";
  const helpMarkup = help ? `<small id="${helpId}">${safe(help)}</small>` : "";
  return `<div class="field"><label for="${name}">${safe(label)}</label><select id="${name}" name="${name}"${describedBy}>${options.map(([value, text]) => `<option value="${safe(value)}" ${String(selected) === String(value) ? "selected" : ""}>${safe(text)}</option>`).join("")}</select>${helpMarkup}</div>`;
}

function countryOptions() {
  return [
    ["PA", "Panamá"],
    ["US", "Estados Unidos"],
    ["MX", "México"],
    ["CO", "Colombia"],
    ["CR", "Costa Rica"],
    ["DO", "República Dominicana"],
    ["GT", "Guatemala"],
    ["ES", "España"]
  ];
}

function currencyOptions() {
  return [
    ["USD", "USD - Dólar estadounidense"],
    ["PAB", "PAB - Balboa panameño"],
    ["MXN", "MXN - Peso mexicano"],
    ["COP", "COP - Peso colombiano"],
    ["CRC", "CRC - Colón costarricense"],
    ["DOP", "DOP - Peso dominicano"],
    ["EUR", "EUR - Euro"]
  ];
}

function enumOptions(labels, selected) {
  return labels.map((label, index) => `<option value="${index}" ${String(selected) === label || Number(selected) === index ? "selected" : ""}>${safe(label)}</option>`).join("");
}

function permissionActionOptions(selected = 7) {
  return enumOptions(["Read", "Create", "Update", "Approve", "Reject", "Delete", "Export", "Manage"], selected);
}

function formObject(form) {
  return Object.fromEntries([...form.entries()].map(([key, value]) => {
    const normalized = typeof value === "string" ? value.trim() : value;
    return [key, normalized === "" ? null : normalized];
  }));
}

function bindEnterpriseFieldValidation() {
  document.querySelectorAll(".tenant-panel input, .tenant-panel textarea, .tenant-panel select").forEach(field => {
    const validate = () => validateEnterpriseField(field);
    field.addEventListener("input", validate);
    field.addEventListener("blur", validate);
  });
}

function validateEnterpriseField(field) {
  if (!field || typeof field.setCustomValidity !== "function") {
    return;
  }

  const value = (field.value || "").trim();
  let message = "";

  if (field.required && !value) {
    message = "No puede estar vacío.";
  } else if (field.type === "email" && value && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.toLowerCase())) {
    message = "No es un correo electrónico válido.";
  } else if (field.type === "url" && value && !/^https?:\/\/[^\s]+\.[^\s]+$/i.test(value)) {
    message = "Debe ser una URL válida que inicie con http:// o https://.";
  } else if (field.name === "hostName" && value && !/^(?=.{1,253}$)([a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,63}$/i.test(value)) {
    message = "Debe ser un dominio DNS válido, sin http:// ni rutas.";
  } else if (field.name === "ipWhitelist" && value && !value.split(",").every(item => /^\s*(?:\d{1,3}\.){3}\d{1,3}\/(?:[0-9]|[1-2][0-9]|3[0-2])\s*$/.test(item))) {
    message = "Use rangos CIDR válidos separados por comas. Ejemplo: 192.168.1.0/24.";
  } else if ((field.name === "claimsMappingJson" || field.name === "roleMappingJson") && value) {
    try {
      JSON.parse(value);
    } catch {
      message = "Debe ser JSON válido.";
    }
  } else if (field.name === "initialPassword" && value && !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{12,}$/.test(value)) {
    message = "Debe tener 12 caracteres, mayúscula, minúscula, número y símbolo.";
  }

  field.setCustomValidity(message);
}

function tenantAlerts(alerts) {
  if (!alerts.length) {
    return `<section class="tenant-alerts"><article class="tenant-alert ok"><strong>Sin alertas criticas</strong><p>El Tenant Administration Center no detecta bloqueos inmediatos.</p></article></section>`;
  }

  return `<section class="tenant-alerts">${alerts.map(alert => `<article class="tenant-alert ${safe(alert.severity)}"><strong>${safe(alert.title)}</strong><p>${safe(alert.message)}</p></article>`).join("")}</section>`;
}

function formatDate(value) {
  return value ? new Date(value).toLocaleString() : "n/a";
}

function formatBytes(value) {
  const bytes = Number(value || 0);
  if (bytes < 1024) {
    return `${bytes} B`;
  }

  const units = ["KB", "MB", "GB", "TB"];
  let size = bytes / 1024;
  let index = 0;
  while (size >= 1024 && index < units.length - 1) {
    size /= 1024;
    index++;
  }

  return `${size.toFixed(2)} ${units[index]}`;
}

function statusLabel(value) {
  const labels = {
    Healthy: "Saludable",
    Unhealthy: "No saludable",
    Degraded: "Degradado",
    Unknown: "Desconocido",
    Disabled: "Inactivo",
    Available: "Disponible",
    Warning: "Advertencia",
    Critical: "Critico"
  };
  return labels[value] || value || "n/a";
}

function tenantStatusName(value) {
  return enumName(["Draft", "Trial", "Active", "Suspended", "Archived"], value);
}

function subscriptionPlanName(value) {
  return enumName(["Starter", "Professional", "Enterprise", "Dedicated"], value);
}

function subscriptionStateName(value) {
  return enumName(["Trial", "Active", "PastDue", "Suspended", "Cancelled"], value);
}

function enumName(labels, value) {
  if (typeof value === "number") {
    return displayLabel(labels[value] || String(value));
  }

  return displayLabel(String(value ?? "n/a"));
}

function displayLabel(value) {
  const labels = {
    Active: "Activo",
    Completed: "Completado",
    Approved: "Aprobado",
    Live: "Activo",
    Healthy: "Saludable",
    Degraded: "Degradado",
    Unhealthy: "No saludable",
    Unknown: "Desconocido",
    Disabled: "Inactivo",
    Locked: "Bloqueado",
    Invited: "Invitado",
    Enabled: "Habilitado",
    Draft: "Borrador",
    Failed: "Fallido",
    Revoked: "Revocado",
    Pending: "Pendiente",
    Succeeded: "Exitoso",
    Retrying: "Reintentando",
    DeadLetter: "Buzon muerto",
    PendingVerification: "Verificacion pendiente",
    Verified: "Verificado",
    DnsFailed: "DNS fallido",
    CertificateFailed: "Certificado fallido",
    NotRequested: "No solicitado",
    Issued: "Emitido",
    Expired: "Expirado",
    Default: "Principal",
    PrimaryCustom: "Personalizado principal",
    SecondaryCustom: "Personalizado secundario",
    Subdomain: "Subdominio",
    Alias: "Alias",
    Trial: "Prueba",
    Archived: "Archivado",
    PastDue: "Vencido",
    Suspended: "Suspendido",
    Cancelled: "Cancelado"
  };
  return labels[value] || value;
}

function safe(value) {
  return escapeHtml(String(value ?? ""));
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
    const button = event.currentTarget.querySelector("button[type='submit']");
    try {
      setLoadingButton(button, true, "Guardando...");
      await createEnterpriseWorkspaceItem(key, form);
    } finally {
      setLoadingButton(button, false);
    }
  });
  document.querySelector("#complete-first-item").addEventListener("click", async event => {
    if (!rows.length) return;
    try {
      setLoadingButton(event.currentTarget, true, "Completando...");
      await request(`/tenants/${state.tenantId}/enterprise-workspaces/${rows[0].id}/complete`, { method: "POST", body: {}, loadingContext: "save" });
      toast(`${workspace.title}: item completado.`, "success");
      await renderRoute();
    } finally {
      setLoadingButton(event.currentTarget, false);
    }
  });
}

async function createEnterpriseWorkspaceItem(key, form) {
  const workspace = enterpriseWorkspaces[key];
  const dueValue = String(form.get("dueAtUtc") || "");
  await request(`/tenants/${state.tenantId}/enterprise-workspaces`, {
    method: "POST",
    loadingContext: "save",
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

function readOnlyNotice(key) {
  const label = routeMetadata[key]?.label || modules[key]?.title || key;
  return `
    <div class="empty-state">
      <h3>Modo solo lectura</h3>
      <p>Tu rol puede consultar ${safe(label)}, pero no crear, editar, aprobar ni eliminar registros en este modulo.</p>
    </div>`;
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
      loadingContext: "suppliers",
      body: { legalName: name, taxIdentifier: code, countryCode: String(form.get("country") || "PA").toUpperCase() }
    });
  },
  async createCapa({ name, code, description }) {
    return request(`/tenants/${state.tenantId}/capas`, {
      method: "POST",
      loadingContext: "capa",
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
      loadingContext: "risks",
      body: { name: `${name} Category`, code: `${code}-CAT` }
    });
    await request(`/tenants/${state.tenantId}/risks/matrices`, {
      method: "POST",
      loadingContext: "risks",
      body: { name: `${name} Matrix`, toleranceScore: 10 }
    });
    return request(`/tenants/${state.tenantId}/risks`, {
      method: "POST",
      loadingContext: "risks",
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
      loadingContext: "indicators",
      body: { name: `${name} Category`, code: `${code}-CAT` }
    });
    const indicator = await request(`/tenants/${state.tenantId}/indicators`, {
      method: "POST",
      loadingContext: "indicators",
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
      loadingContext: "indicators",
      body: { targetValue: 95, effectiveFromUtc: new Date().toISOString() }
    });
    await request(`/tenants/${state.tenantId}/indicators/${indicator.id}/threshold`, {
      method: "POST",
      loadingContext: "indicators",
      body: { warningMinimum: 90, criticalMinimum: 80, excellentMinimum: 98 }
    });
    return indicator;
  },
  async createDocumentFoundation({ name, code, description }) {
    const type = await request(`/tenants/${state.tenantId}/documents/types`, {
      method: "POST",
      loadingContext: "documents",
      body: { name: `${name} Type`, code: `${code}-TYPE`, retentionDays: 2555 }
    });
    const category = await request(`/tenants/${state.tenantId}/documents/categories`, {
      method: "POST",
      loadingContext: "documents",
      body: { name: `${name} Category`, code: `${code}-CAT` }
    });
    return request(`/tenants/${state.tenantId}/documents`, {
      method: "POST",
      loadingContext: "documents",
      body: { documentTypeId: type.id, categoryId: category.id, title: name, code }
    });
  },
  async createTechnicalSheetFoundation({ name, code, description }) {
    const product = await request(`/tenants/${state.tenantId}/technical-sheets/products`, {
      method: "POST",
      loadingContext: "technical-sheets",
      body: { name, sku: code, description }
    });
    return request(`/tenants/${state.tenantId}/technical-sheets`, {
      method: "POST",
      loadingContext: "technical-sheets",
      body: { productId: product.id, title: `${name} Technical Sheet` }
    });
  },
  async createAuditFoundation({ name, code, form }) {
    const now = new Date();
    const start = new Date(now.getTime() + 86400000);
    const end = new Date(now.getTime() + 172800000);
    const program = await request(`/tenants/${state.tenantId}/audit-management/programs`, {
      method: "POST",
      loadingContext: "audits",
      body: { name: `${name} Program`, code: `${code}-PROG`, year: now.getUTCFullYear() }
    });
    const plan = await request(`/tenants/${state.tenantId}/audit-management/plans`, {
      method: "POST",
      loadingContext: "audits",
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
      loadingContext: "audits",
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

function dateTimeLocalValue(value) {
  const date = new Date(value);
  const offsetMs = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

function productionHero(audit, capa, risk, indicators, reportCatalog) {
  return `
    <section class="hero-card dashboard-hero">
      <div>
        <span class="product-badge">Compliance 360 Enterprise Edition</span>
        <h1>Centro de comando para cumplimiento, calidad y riesgo</h1>
        <p>Aplicacion multitenant conectada a datos reales: documentos, proveedores, auditorias, CAPA, riesgos, KPIs, reportes y auditoria de seguridad en un mismo workspace.</p>
        <div class="hero-actions">
          ${quickRouteButton("reports", "Abrir Report Center", "primary")}
          ${quickRouteButton("documents", "Crear evidencia", "light")}
          ${quickRouteButton("risks", "Revisar matriz de riesgo", "light")}
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
  const visibleTiles = tiles.filter(([route]) => canNavigate(route));
  return `
    <section class="card">
      <div class="section-heading">
        <div>
          <h2 class="section-title">Enterprise Workspaces</h2>
          <p class="metric-label">Acceso visual a los dominios productivos principales.</p>
        </div>
      </div>
      <div class="workspace-grid">
        ${visibleTiles.map(([route, title, description]) => `
          <button class="workspace-tile" type="button" data-route="${route}">
            <span class="workspace-icon">${title.slice(0, 2).toUpperCase()}</span>
            <strong>${title}</strong>
            <small>${description}</small>
          </button>`).join("")}
      </div>
    </section>`;
}

function quickRouteButton(route, label, tone = "") {
  if (!canNavigate(route)) return "";
  return `<button class="btn ${tone}" data-route="${route}">${safe(label)}</button>`;
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
  const method = options.method || "GET";
  const stopLoading = options.silent
    ? () => {}
    : startGlobalLoading(options.loadingContext || loadingContextFromPath(path, method), {
      overlay: options.overlay ?? method !== "GET"
    });

  try {
    const response = await fetch(`${API}${path}`, {
      method,
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
  } finally {
    stopLoading();
  }
}

async function downloadProtectedText(path, fileName) {
  const response = await fetch(`${API}${path}`, {
    headers: { Authorization: `Bearer ${state.token}` }
  });
  if (!response.ok) {
    throw new Error(`${response.status} ${response.statusText}: ${await response.text()}`);
  }

  const blob = await response.blob();
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
  toast("Archivo exportado correctamente.", "success");
}

function ensureLoadingHost() {
  if (document.querySelector("#global-loading-host")) return;
  const host = document.createElement("div");
  host.id = "global-loading-host";
  host.innerHTML = `
    <div id="top-progress" class="top-progress" aria-hidden="true"><span></span></div>
    <div id="progress-banner" class="progress-banner" role="status" aria-live="polite" hidden></div>
    <div id="global-loading-overlay" class="global-loading-overlay" role="status" aria-live="polite" aria-atomic="true" aria-hidden="true">
      <div class="global-loading-card">
        <div class="loading-logo" aria-hidden="true">
          <span>C360</span>
          <i></i>
        </div>
        <div class="loading-copy">
          <strong id="global-loading-message">Procesando solicitud...</strong>
          <p>Compliance 360 esta trabajando de forma segura.</p>
          <div class="indeterminate-bar" aria-hidden="true"><span></span></div>
        </div>
      </div>
    </div>`;
  document.body.appendChild(host);
}

function startGlobalLoading(context = "default", options = {}) {
  ensureLoadingHost();
  state.loading.active += 1;
  const topProgress = document.querySelector("#top-progress");
  const overlay = document.querySelector("#global-loading-overlay");
  const message = document.querySelector("#global-loading-message");
  const messages = resolveLoadingMessages(context);
  const shouldAnimate = state.loadingConfig.animations && !state.loadingConfig.reducedMotion;

  document.body.setAttribute("aria-busy", "true");
  topProgress?.classList.add("active");
  if (!shouldAnimate) {
    document.documentElement.dataset.reducedLoading = "true";
  }
  updateLoadingMessage(message, messages);

  clearInterval(state.loading.messageTimer);
  if (state.loadingConfig.messages && messages.length > 1) {
    state.loading.messageTimer = setInterval(() => updateLoadingMessage(message, messages), 1200);
  }

  clearTimeout(state.loading.overlayTimer);
  if (options.overlay) {
    state.loading.overlayTimer = setTimeout(() => {
      if (state.loading.active > 0) {
        overlay?.classList.add("active");
        overlay?.setAttribute("aria-hidden", "false");
      }
    }, 140);
  }

  return () => {
    state.loading.active = Math.max(0, state.loading.active - 1);
    if (state.loading.active > 0) return;
    clearTimeout(state.loading.overlayTimer);
    clearInterval(state.loading.messageTimer);
    topProgress?.classList.remove("active");
    overlay?.classList.remove("active");
    overlay?.setAttribute("aria-hidden", "true");
    document.body.removeAttribute("aria-busy");
  };
}

function updateLoadingMessage(node, messages) {
  if (!node) return;
  const nextMessage = messages[state.loading.messageIndex % messages.length];
  state.loading.messageIndex += 1;
  node.textContent = nextMessage;
}

function resolveLoadingMessages(context) {
  if (!state.loadingConfig.messages) return ["Procesando solicitud..."];
  return loadingMessages[context] || loadingMessages.default;
}

function loadingContextFromPath(path, method) {
  if (path.includes("/auth/")) return "login";
  if (path.includes("/reports")) return method === "GET" ? "reports" : "reports";
  if (path.includes("/documents")) return "documents";
  if (path.includes("/technical-sheets")) return "technical-sheets";
  if (path.includes("/suppliers")) return "suppliers";
  if (path.includes("/audit-management") || path.includes("/audit/")) return "audits";
  if (path.includes("/capas")) return "capa";
  if (path.includes("/risks")) return "risks";
  if (path.includes("/indicators")) return "indicators";
  if (path.includes("/storage") || path.includes("/notifications")) return "configuration";
  return method === "GET" ? state.route : "save";
}

function showPageTransition(route) {
  const content = document.querySelector("#content");
  if (!content) return;
  content.classList.add("page-transition-out");
}

function showProgressBanner(context) {
  ensureLoadingHost();
  const banner = document.querySelector("#progress-banner");
  if (!banner) return;
  const steps = resolveLoadingMessages(context);
  banner.hidden = false;
  banner.innerHTML = `
    <div>
      <strong>${escapeHtml(steps[0])}</strong>
      <span>${steps.map(step => `<i>${escapeHtml(step)}</i>`).join("")}</span>
    </div>
    <div class="indeterminate-bar"><span></span></div>`;
}

function hideProgressBanner() {
  const banner = document.querySelector("#progress-banner");
  if (banner) banner.hidden = true;
}

function setLoadingButton(button, isLoading, text) {
  if (!button) return;
  if (isLoading) {
    button.dataset.originalText = button.dataset.originalText || button.textContent.trim();
    button.disabled = true;
    button.classList.add("btn-loading");
    button.innerHTML = `<span class="btn-progress" aria-hidden="true"></span><span>${escapeHtml(text || "Procesando...")}</span>`;
    return;
  }
  button.disabled = false;
  button.classList.remove("btn-loading");
  if (button.dataset.originalText) {
    button.textContent = button.dataset.originalText;
    delete button.dataset.originalText;
  }
}

function loadingView(route = "default") {
  return `
    <section class="loading-panel modern" aria-live="polite" aria-label="Cargando ${escapeHtml(currentRouteLabel())}">
      <div class="loading-logo compact" aria-hidden="true"><span>C360</span><i></i></div>
      <div>
        <strong>${escapeHtml(resolveLoadingMessages(route)[0])}</strong>
        <p>Consultando API v1, permisos y persistencia PostgreSQL.</p>
        <div class="indeterminate-bar" aria-hidden="true"><span></span></div>
      </div>
    </section>
    ${loadingPage(route)}`;
}

function loadingPage(route) {
  if (!state.loadingConfig.skeleton) return "";
  if (route === "dashboard" || route === "compliance") return loadingDashboard();
  if (route === "reports") return loadingReport();
  if (route === "configuration") return `${loadingCards(4)}${loadingTable(["provider", "name", "status", "priority"])}`;
  if (modules[route] || enterpriseWorkspaces[route] || route === "audit-trail") return `${loadingCards(4)}${loadingTable(modules[route]?.columns || enterpriseWorkspaces[route]?.columns || ["fecha", "accion", "modulo", "estado"])}`;
  return loadingCards(6);
}

function loadingDashboard() {
  return `
    <section class="grid cards">${Array.from({ length: 4 }, (_, index) => skeletonCard(`Dashboard card ${index + 1}`)).join("")}</section>
    <section class="grid two">
      ${skeletonChart("Compliance performance")}
      ${skeletonChart("Risk heat map")}
    </section>`;
}

function loadingReport() {
  return `
    ${loadingCards(4)}
    <section class="card loading-report">
      <h2 class="section-title">Generacion de reportes</h2>
      ${resolveLoadingMessages("reports").map(step => `<div class="report-step"><span></span>${escapeHtml(step)}</div>`).join("")}
    </section>
    ${loadingTable(["name", "code", "module", "status", "dataset"])}`;
}

function loadingCards(count) {
  return `<section class="grid cards">${Array.from({ length: count }, (_, index) => skeletonCard(`Cargando tarjeta ${index + 1}`)).join("")}</section>`;
}

function skeletonCard(label = "Cargando tarjeta") {
  return `
    <article class="skeleton-card" aria-label="${escapeHtml(label)}">
      <span></span><strong></strong><em></em>
    </article>`;
}

function skeletonChart(label) {
  return `
    <article class="card skeleton-chart" aria-label="${escapeHtml(label)}">
      <span></span>
      <div>${Array.from({ length: 12 }, () => "<i></i>").join("")}</div>
    </article>`;
}

function loadingTable(columns) {
  return `
    <section class="card">
      <div class="section-heading">
        <div>
          <h2 class="section-title">Cargando registros</h2>
          <p class="metric-label">Preparando filas tenant-scoped.</p>
        </div>
        <span class="status-pill warn">Loading</span>
      </div>
      <div class="table-wrap">
        <table class="skeleton-table" aria-label="Tabla cargando">
          <thead><tr>${columns.map(column => `<th>${escapeHtml(column)}</th>`).join("")}</tr></thead>
          <tbody>
            ${Array.from({ length: 6 }, () => `<tr>${columns.map(() => "<td><span></span></td>").join("")}</tr>`).join("")}
          </tbody>
        </table>
      </div>
    </section>`;
}

function loadingUpload(fileName = "Documento_Procedimiento.pdf", percent = 65) {
  const progress = Math.round(clamp(percent) / 5) * 5;
  return `
    <div class="loading-upload" role="status" aria-label="Subiendo ${escapeHtml(fileName)}">
      <strong>${escapeHtml(fileName)}</strong>
      <div class="upload-track upload-progress-${progress}"><span></span></div>
      <div><span>${progress}%</span><span>3 segundos restantes</span></div>
    </div>`;
}

function pageHeader(title, description, breadcrumb) {
  return `
    <div class="breadcrumbs">Compliance 360 / ${breadcrumb}</div>
    <div class="page-header">
      <div class="page-title">
        <span class="product-badge">${escapeHtml(currentRouteGroup())}</span>
        <h1>${title}</h1>
        <p>${description}</p>
      </div>
      <div class="button-row">
        <button class="btn subtle" type="button" data-route="dashboard">Dashboard</button>
        <button class="btn subtle" type="button" data-route="reports">Reportes</button>
        <a class="btn" href="/swagger" target="_blank" rel="noreferrer">Swagger</a>
        <button class="btn subtle" type="button" data-action="reload">Recargar</button>
      </div>
    </div>`;
}

function metric(label, value, help) {
  return `
    <article class="card metric-card">
      <div class="metric-label">${label}</div>
      <div class="metric-value">${escapeHtml(String(value))}</div>
      <div class="metric-label">${help}</div>
    </article>`;
}

function tableCard(title, rows, columns) {
  if (!rows.length) {
    return `
      <section class="card">
        <div class="section-heading">
          <div>
            <h2 class="section-title">${title}</h2>
            <p class="metric-label">Datos tenant-scoped consultados en tiempo real.</p>
          </div>
          <span class="status-pill warn">0 registros</span>
        </div>
        <div class="empty-state">
          <strong>No hay registros todavia.</strong>
          <p>Usa el Action Center superior para crear el primer item y validar el flujo end-to-end.</p>
        </div>
      </section>`;
  }
  return `
    <section class="card">
      <div class="section-heading">
        <div>
          <h2 class="section-title">${title}</h2>
          <p class="metric-label">${rows.length} registros visibles en esta vista.</p>
        </div>
        <span class="status-pill ok">Live data</span>
      </div>
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

async function promiseSettled(promise) {
  try {
    return { status: "fulfilled", value: await promise };
  } catch (reason) {
    return { status: "rejected", reason };
  }
}

function formatCell(value) {
  if (value === null || value === undefined || value === "") {
    return '<span class="metric-label">n/a</span>';
  }
  if (typeof value === "string" && value.includes("T") && value.endsWith("Z")) {
    return escapeHtml(new Date(value).toLocaleString());
  }
  if (typeof value === "string" && value.startsWith("__html:")) {
    return value.slice(7);
  }
  if (typeof value === "string" && ["Active", "Completed", "Approved", "Live", "Healthy"].includes(value)) {
    return `<span class="status-pill ok">${escapeHtml(displayLabel(value))}</span>`;
  }
  if (typeof value === "number" && value >= 0 && value <= 10) {
    return `<span class="tag">${value}</span>`;
  }
  if (typeof value === "object") {
    return escapeHtml(JSON.stringify(value));
  }
  return escapeHtml(displayLabel(String(value)));
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

function errorView(message) {
  return `
    <section class="error-state rich">
      <strong>No se pudo cargar esta vista.</strong>
      <p>${escapeHtml(message)}</p>
      <div class="button-row">
        <button id="retry-route" class="btn primary" type="button">Reintentar</button>
        <button class="btn" type="button" data-route="dashboard">Volver al Dashboard</button>
      </div>
    </section>`;
}

function currentRouteLabel() {
  return routeMetadata[state.route]?.label || enterpriseWorkspaces[state.route]?.title || "Workspace";
}

function currentRouteGroup() {
  return routeMetadata[state.route]?.group || (enterpriseWorkspaces[state.route] ? "Enterprise" : "Workspace");
}

function routeFromLabel(value) {
  if (!value) return null;
  const normalized = value.toLowerCase();
  const match = Object.entries(routeMetadata).find(([, metadata]) => metadata.label.toLowerCase() === normalized);
  if (match) return match[0];
  const partial = Object.entries(routeMetadata).find(([, metadata]) => metadata.label.toLowerCase().includes(normalized));
  return partial?.[0] || null;
}

function initials(label) {
  return label.split(/\s+/).filter(Boolean).slice(0, 2).map(part => part[0]).join("").toUpperCase();
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
