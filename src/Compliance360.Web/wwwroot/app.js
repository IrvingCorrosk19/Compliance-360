const API = "/api/v1";
const DEFAULT_TENANT = "dc7c46ee-cb25-4ed5-b0b4-800788f7f626";
const DEFAULT_EMAIL = "admin@compliance360.local";
const LOGIN_V2_ENABLED = localStorage.getItem("c360.login.v2") !== "false";

const state = {
  token: localStorage.getItem("c360.token"),
  permissions: permissionsFromToken(localStorage.getItem("c360.token")),
  role: roleFromToken(localStorage.getItem("c360.token")) || localStorage.getItem("c360.role"),
  displayName: displayNameFromToken(localStorage.getItem("c360.token")) || localStorage.getItem("c360.displayName"),
  tenantId: localStorage.getItem("c360.tenantId") || DEFAULT_TENANT,
  email: localStorage.getItem("c360.email") || DEFAULT_EMAIL,
  userId: localStorage.getItem("c360.userId"),
  theme: localStorage.getItem("c360.theme") || "light",
  language: detectInitialLanguage(),
  translations: {},
  route: location.hash.replace("#/", "") || "dashboard",
  mfaChallenge: null,
  auth: {
    step: "email",
    resolverToken: null,
    organizations: [],
    preselectedOrganizationId: null,
    selectedOrganizationId: null,
    rememberMe: localStorage.getItem("c360.rememberMe") === "true"
  },
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
  },
  sessionEnding: false
};
window.state = state;

function t(key, params) {
  return window.I18n?.t(key, params) ?? key;
}
window.t = t;

function detectInitialLanguage() {
  return window.I18n?.getLanguage?.() || (navigator.language?.toLowerCase().startsWith("en") ? "en" : "es");
}

async function initializeI18n() {
  await window.I18n.ready;
  state.language = window.I18n.getLanguage();
  if (state.token && isAccessTokenExpired(state.token)) {
    endSessionGracefully("expired", { silentBoot: true });
    return;
  }
  if (state.token && state.tenantId) {
    try {
      const prefs = await request(`/tenants/${state.tenantId}/users/me/preferences`);
      if (prefs?.preferredLanguage === "es" || prefs?.preferredLanguage === "en") {
        await window.I18n.setLanguage(prefs.preferredLanguage);
        state.language = prefs.preferredLanguage;
      }
    } catch (error) {
      if (String(error?.message || "").includes("401") || /sesi[oó]n expir/i.test(String(error?.message || ""))) {
        endSessionGracefully("expired");
        return;
      }
      /* preferences are optional on first boot */
    }
  }
  scheduleSessionWatch();
}

async function loadLanguage(language) {
  await window.I18n.setLanguage(language);
  state.language = window.I18n.getLanguage();
}

let translateDomRunning = false;

function translateDom(root = document) {
  translateDomRunning = true;
  try {
    window.I18n?.applyDom(root);
    const reverse = window.__I18N_REVERSE;
    if (!(reverse instanceof Map)) return;
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, {
      acceptNode(node) {
        if (!node.nodeValue?.trim()) return NodeFilter.FILTER_REJECT;
        if (["SCRIPT", "STYLE", "TEXTAREA", "INPUT", "SELECT", "OPTION"].includes(node.parentElement?.tagName)) {
          return NodeFilter.FILTER_REJECT;
        }
        return NodeFilter.FILTER_ACCEPT;
      }
    });
    const nodes = [];
    while (walker.nextNode()) nodes.push(walker.currentNode);
    nodes.forEach(node => {
      const original = node.nodeValue;
      const trimmed = original.trim();
      const key = reverse.get(trimmed);
      if (!key) return;
      const translated = t(key);
      if (translated && translated !== key && translated !== trimmed) {
        node.nodeValue = original.replace(trimmed, translated);
      }
    });
    root.querySelectorAll?.("option").forEach(option => {
      const text = option.textContent.trim();
      const key = reverse.get(text);
      if (!key) return;
      const translated = t(key);
      if (translated && translated !== key && translated !== text) option.textContent = translated;
    });
    root.querySelectorAll?.("[placeholder], [title], [aria-label]").forEach(element => {
      ["placeholder", "title", "aria-label"].forEach(attribute => {
        const value = element.getAttribute(attribute);
        if (!value) return;
        const key = reverse.get(value.trim());
        if (!key) return;
        const translated = t(key);
        if (translated && translated !== key) element.setAttribute(attribute, translated);
      });
    });
  } finally {
    // Discard mutation records produced by this pass so the observer
    // does not re-trigger itself in a loop.
    if (typeof translateObserver !== "undefined") translateObserver.takeRecords();
    translateDomRunning = false;
  }
}

// Modules like the RA console and Compliance Studio re-render themselves
// without going through renderRoute, so translate any DOM they inject.
let translateObserverTimer = null;
const translateObserver = new MutationObserver(mutations => {
  if (translateDomRunning) return;
  const hasNewContent = mutations.some(m => m.addedNodes.length > 0);
  if (!hasNewContent) return;
  clearTimeout(translateObserverTimer);
  translateObserverTimer = setTimeout(() => {
    const content = document.querySelector("#content");
    if (content) translateDom(content);
  }, 80);
});

function startTranslateObserver() {
  const app = document.querySelector("#app");
  if (!app || app.dataset.i18nObserved) return;
  app.dataset.i18nObserved = "true";
  translateObserver.observe(app, { childList: true, subtree: true });
}

function languageSelectorView(compact = false) {
  return `
    <label class="${compact ? "language-switch compact" : "language-switch"}">
      <span>${t("Settings.Language")}</span>
      ${window.I18n.languageSelectorHtml(compact)}
    </label>`;
}

function bindLanguageSelector() {
  window.I18n.bindLanguageSelector(async language => {
    state.language = language;
    if (state.token && state.tenantId) {
      try {
        await request(`/tenants/${state.tenantId}/users/me/preferences`, {
          method: "PUT",
          body: { preferredLanguage: language }
        });
      } catch {
        /* local preference still applied */
      }
    }
    render();
    toast(t("Common.LanguageUpdated"), "success");
  });
}

function decodeTokenPayload(token) {
  if (!token) return null;
  try {
    return JSON.parse(atob(token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")));
  } catch {
    return null;
  }
}

function permissionsFromToken(token) {
  const payload = decodeTokenPayload(token);
  if (!payload) return [];
  const permissions = payload.permission || [];
  return Array.isArray(permissions) ? permissions : [permissions];
}

function roleFromToken(token) {
  const payload = decodeTokenPayload(token);
  if (!payload) return null;
  const role = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
    || payload.role
    || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role"];
  if (Array.isArray(role)) return role[0] || null;
  return role || null;
}

function displayNameFromToken(token) {
  const payload = decodeTokenPayload(token);
  if (!payload) return null;
  return payload.name || payload.unique_name || payload.fullName || null;
}

function passwordFieldHtml(id, name, labelText) {
  return `
    <div class="field">
      <label for="${id}">${labelText}</label>
      <div class="password-field">
        <input id="${id}" name="${name}" type="password" autocomplete="current-password" required>
        <button type="button" class="password-toggle" data-password-toggle="${id}" aria-label="${t("Login.ShowPassword")}" aria-pressed="false" title="${t("Login.ShowPassword")}">
          <svg class="eye-open" viewBox="0 0 24 24" width="20" height="20" aria-hidden="true"><path fill="currentColor" d="M12 5c-5 0-9.27 3.11-11 7 1.73 3.89 6 7 11 7s9.27-3.11 11-7c-1.73-3.89-6-7-11-7zm0 12a5 5 0 1 1 0-10 5 5 0 0 1 0 10zm0-8a3 3 0 1 0 0 6 3 3 0 0 0 0-6z"/></svg>
          <svg class="eye-closed" viewBox="0 0 24 24" width="20" height="20" aria-hidden="true" hidden><path fill="currentColor" d="M2.1 3.51 3.5 2.1l18.4 18.4-1.41 1.41-3.06-3.06A11.7 11.7 0 0 1 12 19c-5 0-9.27-3.11-11-7a13.2 13.2 0 0 1 5.12-5.55L2.1 3.51zM12 7a5 5 0 0 1 4.9 6.1l-1.56-1.56A3 3 0 0 0 12.5 9.2L12 7zm0-2c5 0 9.27 3.11 11 7a13.3 13.3 0 0 1-3.48 4.55l-1.45-1.45A11.2 11.2 0 0 0 21.1 12C19.37 8.11 15.1 5 12 5c-.7 0-1.38.07-2.04.2L8.4 3.64A12.7 12.7 0 0 1 12 5z"/></svg>
        </button>
      </div>
    </div>`;
}

function bindPasswordToggles(root = document) {
  root.querySelectorAll("[data-password-toggle]").forEach(button => {
    if (button.dataset.bound === "1") return;
    button.dataset.bound = "1";
    button.addEventListener("click", () => {
      const input = document.getElementById(button.dataset.passwordToggle);
      if (!input) return;
      const show = input.type === "password";
      input.type = show ? "text" : "password";
      button.setAttribute("aria-pressed", show ? "true" : "false");
      button.setAttribute("aria-label", show ? t("Login.HidePassword") : t("Login.ShowPassword"));
      button.setAttribute("title", show ? t("Login.HidePassword") : t("Login.ShowPassword"));
      const open = button.querySelector(".eye-open");
      const closed = button.querySelector(".eye-closed");
      if (open) open.hidden = show;
      if (closed) closed.hidden = !show;
    });
  });
}

function currentSessionLabel() {
  const name = state.displayName || state.email || t("Session.User");
  const role = state.role || t("Session.NoRole");
  return { name, role, email: state.email || "" };
}

function hasPermission(code) {
  return state.permissions.includes(code);
}

function hasAnyPermission(codes) {
  return codes.some(code => hasPermission(code));
}

window.hasPermission = hasPermission;
window.hasAnyPermission = hasAnyPermission;
window.permissionsFromToken = permissionsFromToken;

const routePermissions = {
  dashboard: ["TENANT.READ", "REGULATORY.REPORT.READ"],
  "alert-center": ["NOTIFICATION.READ", "NOTIFICATION.MANAGE", "NOTIFICATION.ADMIN", "TENANT.NOTIFICATIONS"],
  "audit-trail": ["AUDIT.READ", "TENANT.AUDIT"],
  "superadmin-platform": ["PLATFORM.DASHBOARD.READ"],
  "tenant-administration": ["PLATFORM.TENANT.READ", "TENANT.USERS", "TENANT.ROLES", "TENANT.UPDATE"],
  regulatory: ["REGULATORY.DOSSIER.READ", "REGULATORY.PRODUCT.READ", "REGULATORY.REPORT.READ", "REGULATORY.CONFIGURE"],
  documents: ["DOCUMENT.READ"],
  "technical-sheets": ["TECHNICALSHEET.READ"],
  suppliers: ["SUPPLIER.READ"],
  "supplier-portal": ["SUPPLIER.READ"],
  audits: ["AUDITMANAGEMENT.READ"],
  capa: ["CAPA.READ"],
  risks: ["RISK.READ"],
  indicators: ["INDICATOR.READ"],
  reports: ["REPORT.READ"],
  // Manual: pantalla Security pertenece al Tenant Administrator (lectura "según permisos").
  security: ["TENANT.SECURITY", "TENANT.USERS"],
  configuration: ["TENANT.STORAGE", "STORAGE.READ", "TENANT.NOTIFICATIONS", "NOTIFICATION.READ", "NOTIFICATION.ADMIN"]
};

const routeManagePermissions = {
  configuration: ["TENANT.STORAGE", "STORAGE.CREATE", "STORAGE.UPDATE", "TENANT.NOTIFICATIONS", "NOTIFICATION.ADMIN"],
  documents: ["DOCUMENT.CREATE", "DOCUMENT.UPDATE"],
  "technical-sheets": ["TECHNICALSHEET.CREATE", "TECHNICALSHEET.UPDATE"],
  suppliers: ["SUPPLIER.CREATE", "SUPPLIER.UPDATE"],
  audits: ["AUDITMANAGEMENT.MANAGE"],
  capa: ["CAPA.MANAGE"],
  risks: ["RISK.MANAGE"],
  indicators: ["INDICATOR.MANAGE"]
};

function canNavigate(route) {
  return !routePermissions[route] || hasAnyPermission(routePermissions[route]);
}

function canManageRoute(route) {
  return Boolean(routeManagePermissions[route] && hasAnyPermission(routeManagePermissions[route]));
}

const loadingMessages = {
  default: ["Cargando informacion...", "Sincronizando informacion...", "Procesando solicitud..."],
  login: ["Validando credenciales...", "Cargando perfil...", "Preparando entorno..."],
  dashboard: ["Preparando dashboard...", "Calculando indicadores...", "Analizando datos..."],
  "alert-center": ["Cargando Alert Center...", "Consultando inbox...", "Sincronizando alertas..."],
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
  save: ["Guardando cambios...", "Actualizando informacion...", "Procesando solicitud..."],
  regulatory: ["Cargando Regulatory Affairs...", "Consultando portafolio...", "Preparando consola RA..."],
  "regutrack-import": [
    "Leyendo archivo REGUTRACK...",
    "Validando hojas y columnas...",
    "Preparando filas para stage...",
    "Enviando al servidor...",
    "Registrando job de importación...",
    "Esto puede tardar con archivos grandes..."
  ]
};

const navigation = [
  { group: "Nav.CommandCenter", items: [
    ["dashboard", "Nav.Dashboard"],
    ["alert-center", "AlertCenter.Title"],
    ["audit-trail", "Tac.Audit"]
  ]},
  { group: "Nav.Regulatory", items: [
    ["regulatory", "Regulatory.Title"]
  ]},
  { group: "Nav.Quality", items: [
    ["documents", "Nav.Documents"],
    ["technical-sheets", "Nav.TechnicalSheets"],
    ["suppliers", "Nav.Suppliers"],
    ["supplier-portal", "Nav.SupplierPortal"],
    ["audits", "Nav.Audits"],
    ["capa", "Nav.Capa"],
    ["risks", "Nav.Risks"],
    ["indicators", "Nav.Indicators"],
    ["reports", "Nav.Reports"]
  ]},
  { group: "Nav.Administration", items: [
    ["superadmin-platform", "Nav.Platform"],
    ["tenant-administration", "Tac.Title"],
    ["security", "Dashboard.Security2"],
    ["configuration", "Settings.Configuration"]
  ]}
];

const routeMetadata = Object.fromEntries(navigation.flatMap(group =>
  group.items.map(([key, labelKey]) => [key, { labelKey, groupKey: group.group, initials: initials(key) }])
));

function navLabel(key) {
  const meta = routeMetadata[key];
  return meta ? t(meta.labelKey) : key;
}

function navGroupLabel(groupKey) {
  return t(groupKey);
}

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
    title: "[LEGACY] Regulatory Workspace Tracker",
    description: "Legacy tracker. No usar como expediente. La operación vive en RA Console (#/regulatory).",
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

initializeI18n().then(render);

function render() {
  ensureLoadingHost();
  startTranslateObserver();
  const app = document.querySelector("#app");
  if (!state.token && state.mfaChallenge) {
    app.innerHTML = mfaChallengeView();
    bindMfaChallenge();
    bindLanguageSelector();
    translateDom(app);
    return;
  }

  if (!state.token) {
    app.innerHTML = loginView();
    bindLogin();
    bindLanguageSelector();
    translateDom(app);
    return;
  }

  app.innerHTML = shellView();
  bindShell();
  bindLanguageSelector();
  translateDom(app);
  renderRoute();
}

function mfaChallengeView() {
  return `
    <main class="login-page">
      <section class="login-panel" aria-labelledby="mfa-title">
        <div class="brand-line">
          <div class="brand-mark" aria-hidden="true">C360</div>
          <span class="product-badge">MFA</span>
          ${languageSelectorView(true)}
        </div>
        <h1 id="mfa-title">${t("MFA.Title")}</h1>
        <p>${t("MFA.Hint")}</p>
        <form id="mfa-form" class="form-stack">
          <div class="field">
            <label for="verificationCode">${t("MFA.Code")}</label>
            <input id="verificationCode" name="verificationCode" inputmode="numeric" autocomplete="one-time-code" maxlength="6" required>
          </div>
          <button class="btn primary" type="submit">${t("MFA.Submit")}</button>
          <button id="cancel-mfa" class="btn subtle" type="button">${t("Common.Cancel")}</button>
        </form>
      </section>
      <section class="login-hero">
        <div class="hero-card">
          <span class="product-badge">MFA</span>
          <h2>${t("MFA.Title")}</h2>
          <p>${t("MFA.Hint")}</p>
        </div>
      </section>
    </main>`;
}

function loginView() {
  if (!LOGIN_V2_ENABLED) return legacyLoginView();
  const step = state.auth.step;
  const selectedOrg = state.auth.organizations.find(organization => organization.id === state.auth.selectedOrganizationId);
  const subtitle = step === "email"
    ? t("Login.EmailStepHint")
    : step === "organization"
      ? t("Login.OrganizationStepHint")
      : t("Login.PasswordStepHint");
  const orgHint = selectedOrg ? `<div class="org-selected-chip"><strong>${escapeHtml(selectedOrg.name)}</strong></div>` : "";
  return `
    <main class="login-page">
      <section class="login-panel" aria-labelledby="login-title">
        <div class="brand-line">
          <div class="brand-mark" aria-hidden="true">C360</div>
          <span class="product-badge">${t("Brand.Edition")}</span>
          ${languageSelectorView(true)}
        </div>
        <h1 id="login-title">${t("Login.Title")}</h1>
        <p>${subtitle}</p>
        ${orgHint}
        <form id="login-form" class="form-stack">
          ${step === "email" ? `
            <div class="field">
              <label for="email">${t("Login.Email")}</label>
              <input id="email" name="email" type="email" value="${escapeHtml(state.email || DEFAULT_EMAIL)}" autocomplete="username" required>
            </div>
            <button class="btn primary" type="submit">${t("Login.Next")}</button>
          ` : ""}
          ${step === "organization" ? `
            <div class="org-list" role="radiogroup" aria-label="${t("Login.SelectOrganization")}">
              ${state.auth.organizations.map(organization => `
                <label class="org-option ${organization.id === state.auth.selectedOrganizationId ? "active" : ""}">
                  <input type="radio" name="organizationId" value="${organization.id}" ${organization.id === state.auth.selectedOrganizationId ? "checked" : ""}>
                  <span class="org-logo">${initials(organization.name)}</span>
                  <span><strong>${escapeHtml(organization.name)}</strong><small>${escapeHtml(organization.description || t("Brand.Name"))}</small></span>
                </label>`).join("")}
            </div>
            <div class="button-row">
              <button class="btn subtle" type="button" id="back-to-email">${t("Login.Back")}</button>
              <button class="btn primary" type="submit">${t("Login.Continue")}</button>
            </div>
          ` : ""}
          ${step === "password" ? `
            ${passwordFieldHtml("password", "password", t("Login.Password"))}
            <label class="remember-line"><input type="checkbox" id="rememberMe" ${state.auth.rememberMe ? "checked" : ""}> ${t("Login.RememberMe")}</label>
            <div class="button-row">
              <button class="btn subtle" type="button" id="back-to-org">${state.auth.organizations.length > 1 ? t("Login.ChangeOrganization") : t("Login.ChangeEmail")}</button>
              <button class="btn primary" type="submit">${t("Login.SignIn")}</button>
            </div>
            <button class="btn subtle" type="button" id="forgotPasswordBtn">${t("Login.ForgotPassword")}</button>
          ` : ""}
        </form>
      </section>
      <section class="login-hero">
        <div class="hero-card">
          <span class="product-badge">${t("Brand.Edition")}</span>
          <h2>${t("Login.Title")}</h2>
          <p>${t("Login.EmailStepHint")}</p>
          <div class="hero-actions">
            <span class="status-pill ok">API</span>
            <span class="status-pill ok">PostgreSQL</span>
            <span class="status-pill ok">RBAC</span>
          </div>
        </div>
      </section>
    </main>`;
}

function legacyLoginView() {
  return `
    <main class="login-page">
      <section class="login-panel" aria-labelledby="login-title">
        <div class="brand-line">
          <div class="brand-mark" aria-hidden="true">C360</div>
          <span class="product-badge">${t("Brand.Edition")}</span>
          ${languageSelectorView(true)}
        </div>
        <h1 id="login-title">${t("Login.Title")}</h1>
        <form id="legacy-login-form" class="form-stack">
          <div class="field"><label for="tenantId">Tenant ID</label><input id="tenantId" name="tenantId" value="${escapeHtml(DEFAULT_TENANT)}" required></div>
          <div class="field"><label for="legacy-email">${t("Common.Email")}</label><input id="legacy-email" name="email" type="email" value="${escapeHtml(DEFAULT_EMAIL)}" required></div>
          ${passwordFieldHtml("legacy-password", "password", t("Common.Password"))}
          <button class="btn primary" type="submit">${t("Login.SignIn")}</button>
        </form>
      </section>
    </main>`;
}

function shellView() {
  const collapsed = localStorage.getItem("c360.sidebar") === "collapsed";
  const session = currentSessionLabel();
  return `
    <div class="layout ${collapsed ? "sidebar-collapsed" : ""}">
      <div class="sidebar-backdrop" id="sidebar-backdrop" aria-hidden="true"></div>
      <aside class="sidebar" id="app-sidebar" aria-label="${t("Nav.CommandCenter")}">
        <div class="sidebar-head">
          <div class="brand">
            <div class="brand-mark">C360</div>
            <div>
              <div class="brand-title">${t("Brand.Name")}</div>
              <div class="brand-subtitle">${t("Brand.Edition")}</div>
            </div>
          </div>
          <button id="sidebar-collapse" class="sidebar-collapse" type="button" aria-label="${t("Common.Menu")}" title="${t("Common.Menu")}">⟨⟩</button>
          <button id="sidebar-close" class="sidebar-close" type="button" aria-label="${t("Common.Close")}">✕</button>
        </div>
        ${languageSelectorView(true)}
        <section class="sidebar-status" aria-label="Estado de la plataforma">
          <span class="status-pill ok">Live</span>
          <strong>Production Core 100%</strong>
          <small>Tenant activo, API segura y datos persistentes.</small>
        </section>
        <section class="sidebar-session" aria-label="${t("Session.Title")}">
          <span class="session-avatar" aria-hidden="true">${initials(session.name)}</span>
          <div>
            <strong title="${escapeHtml(session.email)}">${escapeHtml(session.name)}</strong>
            <small>${escapeHtml(session.role)}</small>
          </div>
        </section>
        ${navigation.map(group => ({ ...group, items: group.items.filter(([key]) => canNavigate(key)) })).filter(group => group.items.length).map(group => `
          <nav class="nav-group" aria-label="${navGroupLabel(group.group)}">
            <div class="nav-label">${navGroupLabel(group.group)}</div>
            ${group.items.map(([key]) => `
              <button class="nav-button ${state.route === key ? "active" : ""}" data-route="${key}">
                <span class="nav-icon">${initials(navLabel(key))}</span>
                <span>${navLabel(key)}</span>
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
          <button id="menu-toggle" class="menu-toggle" type="button" aria-label="${t("Common.Menu")}" aria-controls="app-sidebar" aria-expanded="false">☰</button>
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
              ${navigation.flatMap(group => group.items.filter(([key]) => canNavigate(key)).map(([key]) => `<option value="${key}" ${state.route === key ? "selected" : ""}>${navGroupLabel(group.group)} / ${navLabel(key)}</option>`)).join("")}
            </select>
            <datalist id="route-options">
              ${navigation.flatMap(group => group.items.filter(([key]) => canNavigate(key)).map(([key]) => `<option value="${navLabel(key)}"></option>`)).join("")}
            </datalist>
          </div>
          <div class="top-actions">
            ${canNavigate("alert-center") ? `<button id="notification-bell" class="notification-bell" type="button" aria-label="Abrir Alert Center" title="Alert Center">
              <span aria-hidden="true">🔔</span><span id="notification-badge" class="notification-badge" hidden>0</span>
            </button>` : ""}
            <span class="session-chip" title="${escapeHtml(session.email)} · ${escapeHtml(session.role)}">
              <span class="session-chip-name">${escapeHtml(session.name)}</span>
              <span class="session-chip-role">${escapeHtml(session.role)}</span>
            </span>
            <span class="status-pill ok">Production core</span>
            <span class="tenant-chip" title="${state.tenantId}">Tenant: ${shortId(state.tenantId)}</span>
            <button id="theme-toggle" class="btn subtle" type="button">${state.theme === "dark" ? t("Common.Light") : t("Common.Dark")}</button>
            <button id="logout" class="btn danger" type="button">${t("Common.SignOut")}</button>
          </div>
        </header>
        <section id="content" class="content" tabindex="-1"></section>
      </main>
    </div>`;
}

function bindLogin() {
  if (!LOGIN_V2_ENABLED) {
    bindLegacyLogin();
    return;
  }
  document.querySelector("#login-form").addEventListener("submit", async event => {
    event.preventDefault();
    const button = event.currentTarget.querySelector("button[type='submit']");
    const form = new FormData(event.currentTarget);
    try {
      const step = state.auth.step;
      if (step === "email") {
        setLoadingButton(button, true, "Identificando...");
        const email = String(form.get("email") || "").trim();
        const identify = await request("/auth/identify", {
          method: "POST",
          body: { email },
          anonymous: true,
          loadingContext: "login",
          overlay: true
        });
        state.email = email;
        state.auth.resolverToken = identify.resolverToken;
        state.auth.organizations = identify.organizations || [];
        state.auth.preselectedOrganizationId = identify.preselectedOrganizationId || null;
        state.auth.selectedOrganizationId = identify.preselectedOrganizationId || null;
        if (!state.auth.organizations.length) {
          toast("No encontramos una organizacion para este correo. Verifica que el administrador te haya creado en el tenant.", "error");
          return;
        }
        if (identify.requiresOrganizationSelection) {
          state.auth.step = "organization";
        } else {
          state.auth.step = "password";
        }
        render();
        return;
      }
      if (step === "organization") {
        state.auth.selectedOrganizationId = form.get("organizationId");
        state.auth.step = "password";
        render();
        return;
      }
      setLoadingButton(button, true, "Validando...");
      const result = await request("/auth/login", {
        method: "POST",
        body: {
          email: state.email,
          password: form.get("password"),
          resolverToken: state.auth.resolverToken,
          organizationId: state.auth.selectedOrganizationId || null,
          rememberMe: document.querySelector("#rememberMe")?.checked || false
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
      completeLogin(result);
    } catch (error) {
      const message = friendlyErrorMessage(error);
      toast(message, "error");
      if (/identificaci[oó]n|organizaci[oó]n para este correo|expir/i.test(message)) {
        state.auth.step = "email";
        state.auth.resolverToken = null;
        render();
      }
    } finally {
      setLoadingButton(button, false);
    }
  });
  document.querySelector("#back-to-email")?.addEventListener("click", () => {
    state.auth.step = "email";
    render();
  });
  document.querySelector("#back-to-org")?.addEventListener("click", () => {
    state.auth.step = state.auth.organizations.length > 1 ? "organization" : "email";
    render();
  });
  document.querySelector("#forgotPasswordBtn")?.addEventListener("click", () => {
    toast(t("Login.ForgotPasswordHint"), "info");
  });
  bindPasswordToggles();
}

function bindLegacyLogin() {
  document.querySelector("#legacy-login-form").addEventListener("submit", async event => {
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
        render();
        return;
      }
      completeLogin(result);
    } catch (error) {
      toast(friendlyErrorMessage(error), "error");
    } finally {
      setLoadingButton(button, false);
    }
  });
  bindPasswordToggles();
}

function completeLogin(result) {
  state.token = result.accessToken;
  state.permissions = permissionsFromToken(result.accessToken);
  state.role = roleFromToken(result.accessToken);
  state.displayName = displayNameFromToken(result.accessToken) || result.email || null;
  state.tenantId = result.tenantId;
  state.email = result.email;
  state.userId = result.userId;
  state.auth.step = "email";
  state.auth.resolverToken = null;
  state.auth.organizations = [];
  state.auth.preselectedOrganizationId = null;
  state.auth.selectedOrganizationId = null;
  state.auth.rememberMe = document.querySelector("#rememberMe")?.checked || false;
  state.sessionEnding = false;
  localStorage.setItem("c360.token", state.token);
  localStorage.setItem("c360.tenantId", state.tenantId);
  localStorage.setItem("c360.email", state.email);
  localStorage.setItem("c360.userId", state.userId);
  if (state.role) localStorage.setItem("c360.role", state.role);
  else localStorage.removeItem("c360.role");
  if (state.displayName) localStorage.setItem("c360.displayName", state.displayName);
  else localStorage.removeItem("c360.displayName");
  localStorage.setItem("c360.rememberMe", state.auth.rememberMe ? "true" : "false");
  toast(t("Login.SignedIn"), "success");
  scheduleSessionWatch();
  render();
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
      completeLogin(result);
      toast("MFA validado. Sesion segura iniciada.", "success");
    } catch (error) {
      toast(friendlyErrorMessage(error), "error");
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
  bindResponsiveShell();
  bindNotificationBell();
  document.querySelectorAll("[data-route]").forEach(button => {
    button.addEventListener("click", () => {
      closeSidebarDrawer();
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
    clearSessionLocalState();
    toast(t("Login.SignedOut"), "success");
    location.hash = "#/login";
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
  document.querySelector("#menu-toggle")?.addEventListener("click", () => openSidebarDrawer());
  document.querySelector("#sidebar-close")?.addEventListener("click", () => closeSidebarDrawer());
  document.querySelector("#sidebar-backdrop")?.addEventListener("click", () => closeSidebarDrawer());
  document.querySelector("#sidebar-collapse")?.addEventListener("click", () => {
    const layout = document.querySelector(".layout");
    if (!layout) return;
    if (window.matchMedia("(max-width: 1023px)").matches) {
      layout.classList.remove("sidebar-collapsed");
      layout.classList.toggle("sidebar-expanded");
      return;
    }
    const collapsed = layout.classList.toggle("sidebar-collapsed");
    localStorage.setItem("c360.sidebar", collapsed ? "collapsed" : "open");
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

function bindNotificationBell() {
  const bell = document.querySelector("#notification-bell");
  if (!bell) return;
  bell.addEventListener("click", () => {
    location.hash = "#/alert-center";
  });
  ensureAlertCenter()
    .then(() => window.getAlertCenterCounts())
    .then(counts => {
      const badge = document.querySelector("#notification-badge");
      if (!badge) return;
      badge.textContent = counts.unread > 99 ? "99+" : String(counts.unread || 0);
      badge.hidden = !counts.unread;
      bell.setAttribute("aria-label", `Abrir Alert Center; ${counts.unread || 0} no leídas`);
    })
    .catch(() => {
      bell.classList.add("degraded");
      bell.title = "Alert Center temporalmente no disponible";
    });
}

const mobileDrawerQuery = window.matchMedia("(max-width: 767px)");

function bindResponsiveShell() {
  syncDrawerMode();
  if (!bindResponsiveShell.bound) {
    bindResponsiveShell.bound = true;
    mobileDrawerQuery.addEventListener("change", syncDrawerMode);
    document.addEventListener("keydown", event => {
      if (event.key === "Escape") closeSidebarDrawer();
    });
    const app = document.querySelector("#app");
    let pending = false;
    const observer = new MutationObserver(() => {
      if (pending) return;
      pending = true;
      requestAnimationFrame(() => {
        pending = false;
        enhanceResponsiveTables(app);
      });
    });
    observer.observe(app, { childList: true, subtree: true });
    enhanceResponsiveTables(app);
  }
}

function enhanceResponsiveTables(root = document) {
  root.querySelectorAll("table").forEach(table => {
    if (table.classList.contains("skeleton-table")) return;
    const headers = Array.from(table.querySelectorAll("thead th")).map(th => th.textContent.trim());
    if (!headers.length) return;
    table.querySelectorAll("tbody tr").forEach(row => {
      Array.from(row.children).forEach((cell, index) => {
        if (cell.tagName !== "TD" || cell.hasAttribute("data-label") || cell.hasAttribute("colspan")) return;
        if (headers[index]) cell.setAttribute("data-label", headers[index]);
      });
    });
  });
}

function syncDrawerMode() {
  const layout = document.querySelector(".layout");
  if (!layout) return;
  layout.classList.toggle("drawer-mode", mobileDrawerQuery.matches);
  if (mobileDrawerQuery.matches) {
    layout.classList.remove("sidebar-collapsed", "sidebar-expanded");
  } else {
    closeSidebarDrawer();
    layout.classList.toggle("sidebar-collapsed", localStorage.getItem("c360.sidebar") === "collapsed");
  }
}

function openSidebarDrawer() {
  const layout = document.querySelector(".layout");
  if (!layout) return;
  layout.classList.add("drawer-open");
  document.querySelector("#menu-toggle")?.setAttribute("aria-expanded", "true");
  document.body.style.overflow = "hidden";
  document.querySelector("#app-sidebar .nav-button")?.focus();
}

function closeSidebarDrawer() {
  const layout = document.querySelector(".layout");
  if (!layout || !layout.classList.contains("drawer-open")) return;
  layout.classList.remove("drawer-open");
  document.querySelector("#menu-toggle")?.setAttribute("aria-expanded", "false");
  document.body.style.overflow = "";
}

function renderAccessDenied(content) {
  content.innerHTML = `
    <section class="card" data-testid="access-denied">
      <h2 class="section-title">${t("Common.AccessDenied")}</h2>
      <p class="metric-label">${t("Common.AccessDeniedDetail")}</p>
      <div class="button-row"><button class="btn primary" data-route="dashboard">Ir al Dashboard</button></div>
    </section>`;
  content.querySelector("[data-route]")?.addEventListener("click", () => location.hash = "#/dashboard");
}

async function renderRoute() {
  const content = document.querySelector("#content");
  content.innerHTML = loadingView(state.route);
  content.focus();
  const stopLoading = startGlobalLoading(state.route, { overlay: false });

  try {
    // Defensa en profundidad: URL directa a pantallas fuera del contrato del
    // manual se bloquea en cliente; el backend ya deniega los datos (401/403).
    if (!canNavigate(state.route)) {
      renderAccessDenied(content);
      return;
    }
    if (state.route === "dashboard" || state.route === "compliance") {
      await renderDashboard(content);
      return;
    }
    if (state.route === "security") {
      await renderSecurityCenter(content);
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
    if (state.route === "alert-center") {
      await ensureAlertCenter();
      await window.renderAlertCenter(content);
      return;
    }
    if (state.route === "reports") {
      await renderReports(content);
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
    if (state.route === "regulatory") {
      await ensureRegulatoryAffairs();
      await window.renderRegulatoryAffairs(content);
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
    if (isSessionExpiredError(error)) {
      endSessionGracefully("expired");
      return;
    }
    content.innerHTML = errorView(error.message);
    content.querySelector("#retry-route")?.addEventListener("click", renderRoute);
  } finally {
    stopLoading();
    translateDom(content);
  }
}

async function renderDashboard(content) {
  const canRegulatory = hasAnyPermission(["REGULATORY.REPORT.READ", "REGULATORY.DOSSIER.READ"]);
  const dashboardRequests = {
    health: fetch("/health").then(r => r.json()),
    regulatory: canRegulatory ? request(`/tenants/${state.tenantId}/regulatory/dashboard`) : Promise.resolve({})
  };
  const dashboardResults = Object.fromEntries(await Promise.all(
    Object.entries(dashboardRequests).map(async ([key, promise]) => [key, await promiseSettled(promise)])
  ));

  const regulatory = valueOf(dashboardResults.regulatory, {});

  content.innerHTML = `
    <section class="hero-card compact module-hero">
      <div>
        <span class="product-badge">REGUTRACK Replacement</span>
        <h2>Compliance 360 — Regulatory Affairs</h2>
        <p>Centro operativo para portafolio, expedientes, pipeline, importación REGUTRACK y licencias.</p>
      </div>
      <div class="button-row">
        ${canRegulatory ? `<button class="btn primary" data-route="regulatory">Abrir consola RA</button>` : ""}
        ${canNavigate("audit-trail") ? `<button class="btn" data-route="audit-trail">Bitácora</button>` : ""}
      </div>
    </section>
    <section class="grid cards">
      ${metric("API Health", statusLabel(valueOf(dashboardResults.health, {}).status || "Healthy"), "Estado de servicio")}
      ${metric("Expedientes activos", regulatory.activeDossiers ?? regulatory.openDossiers ?? 0, "Casos en curso")}
      ${metric("Productos", regulatory.totalProducts ?? regulatory.products ?? 0, "Portafolio")}
      ${metric("Alertas", regulatory.openAlerts ?? regulatory.alerts ?? 0, "Vencimientos y bloqueos")}
    </section>
    <section class="grid two">
      <div class="card">
        <h2 class="section-title">Operación diaria</h2>
        <p class="metric-label">Use la consola Regulatory Affairs para reemplazar el Excel REGUTRACK en operación diaria.</p>
        <div class="button-row section-actions">
          ${canNavigate("regulatory") ? quickRouteButton("regulatory", "Regulatory Management", "primary") : ""}
          ${canNavigate("tenant-administration") ? quickRouteButton("tenant-administration", "Administración") : ""}
        </div>
      </div>
      <div class="card">
        <h2 class="section-title">Migración histórica</h2>
        <p class="metric-label">Importe REGUTRACK 02JUN26 MG.xlsx desde la consola RA cuando necesite cargar histórico.</p>
      </div>
    </section>`;
  content.querySelectorAll("[data-route]").forEach(button => button.addEventListener("click", () => location.hash = `#/${button.dataset.route}`));
}

// Manual: pantalla "Security" (#/security) — configuración de seguridad del tenant,
// MFA/SSO según permisos. Lectura para Tenant Administrator; edición vive en el
// panel Seguridad del TAC Center para quien tenga TENANT.SECURITY.
async function renderSecurityCenter(content) {
  const center = await request(`/tenants/${state.tenantId}/administration-center`);
  const settings = center.tenant?.settings || {};
  const sso = center.ssoConfigurations || [];
  const canEditSecurity = hasAnyPermission(["TENANT.SECURITY"]);
  content.innerHTML = `
    ${pageHeader("Security", "Configuración de seguridad del tenant (MFA/SSO según permisos).", "Enterprise / Administration")}
    <section class="grid cards">
      ${metric("Security score", `${settings.securityScore ?? "—"}/100`, "Evaluación del tenant")}
      ${metric("MFA obligatorio", settings.requireMfa ? "Sí" : "No", "Autenticación multifactor")}
      ${metric("Session timeout", `${settings.sessionTimeoutMinutes ?? "—"} min`, "Expiración de sesión")}
      ${metric("Lockout", `${settings.lockoutMaxFailedAttempts ?? "—"} intentos / ${settings.lockoutMinutes ?? "—"} min`, "Bloqueo de cuenta")}
    </section>
    <section class="grid two">
      <div class="card">
        <h2 class="section-title">Política de acceso</h2>
        <ul class="list-tight">
          <li>Password expiration: <strong>${settings.passwordExpirationDays ?? 0} días</strong></li>
          <li>Trusted devices: <strong>${settings.trustedDevicesEnabled ? "Habilitado" : "Deshabilitado"}</strong></li>
          <li>SSO configurado: <strong>${sso.length ? `${sso.length} proveedor(es)` : "No"}</strong></li>
        </ul>
      </div>
      <div class="card">
        <h2 class="section-title">Gestión</h2>
        <p class="metric-label">${canEditSecurity
          ? "Tiene permiso TENANT.SECURITY: edite la política desde Tenant Administration → Seguridad."
          : "Vista de solo lectura. La edición de la política requiere el permiso TENANT.SECURITY."}</p>
        ${canNavigate("tenant-administration") ? `<div class="button-row"><button class="btn" data-route="tenant-administration">Abrir Tenant Administration</button></div>` : ""}
      </div>
    </section>`;
  content.querySelectorAll("[data-route]").forEach(button => button.addEventListener("click", () => location.hash = `#/${button.dataset.route}`));
}

async function renderIntegrationsAdministration(content) {
  const canStorage = hasAnyPermission(["STORAGE.READ", "STORAGE.CREATE", "STORAGE.UPDATE", "TENANT.STORAGE"]);
  const canNotifications = hasAnyPermission(["NOTIFICATION.READ", "NOTIFICATION.ADMIN", "NOTIFICATION.MANAGE", "TENANT.NOTIFICATIONS"]);
  const [health, notificationDashboard, storageProviders] = await Promise.allSettled([
    fetch("/health/ready").then(response => {
      if (!response.ok) throw new Error(`Health ${response.status}`);
      return response.json();
    }),
    canNotifications ? request(`/tenants/${state.tenantId}/notifications/dashboard`) : Promise.resolve({}),
    canStorage ? request(`/tenants/${state.tenantId}/storage/providers`) : Promise.resolve([])
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
          ${hasAnyPermission(["STORAGE.CREATE", "STORAGE.UPDATE", "TENANT.STORAGE"]) ? `<button class="btn primary" id="create-storage-provider" type="button">Crear Storage Local</button>` : ""}
          ${hasAnyPermission(["NOTIFICATION.ADMIN", "NOTIFICATION.MANAGE", "TENANT.NOTIFICATIONS"]) ? `<button class="btn" id="create-email-provider" type="button">Crear Email SMTP</button>` : ""}
        </div>
      </div>
      <div class="metric-grid">
        <article class="metric-card"><span>Health Status</span><strong>${healthText}</strong></article>
        ${canNotifications ? `<article class="metric-card"><span>Notifications Sent</span><strong>${notification.sent ?? 0}</strong></article>
        <article class="metric-card"><span>Delivery Rate</span><strong>${notification.deliveryRatePercent ?? 0}%</strong></article>
        <article class="metric-card"><span>Dead Letters</span><strong>${notification.deadLetters ?? 0}</strong></article>` : ""}
      </div>
      <div class="grid two">
        ${canNotifications ? `<article class="panel">
          <h2>Email Providers</h2>
          <p>SMTP, Gmail SMTP, Microsoft 365, Exchange Online, SendGrid, Mailgun, Resend y Amazon SES configurables por tenant.</p>
          ${tableCard("Email provider health", Object.entries(notification.providerHealth ?? {}).map(([provider, ok]) => ({ provider, status: ok ? "Saludable" : "Requiere configuracion" })), ["provider", "status"])}
        </article>` : ""}
        ${canStorage ? `<article class="panel">
          <h2>Storage Providers</h2>
          <p>Local, Azure Blob, AWS S3, MinIO, Google Cloud Storage y SFTP con prioridad/failover por tenant.</p>
          ${tableCard("Storage provider configurations", storage, ["provider", "name", "containerName", "priority", "isDefault", "isEnabled", "lastHealthStatus"])}
        </article>` : ""}
      </div>
      ${canStorage ? `<article class="panel">
        <h2>Connection Test & Failover</h2>
        <p>Usa prioridad ascendente: proveedor principal, secundario y terciario. Si el principal falla, el backend prueba el siguiente y registra AuditLog/Observability.</p>
        <button class="btn" id="test-first-storage-provider" type="button" ${storage.length ? "" : "disabled"}>Probar primer Storage Provider</button>
      </article>` : ""}
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
  const canApproveDocuments = key === "documents" && hasPermission("DOCUMENT.APPROVE");
  const displayRows = canApproveDocuments
    ? rows.map(row => ({
        ...row,
        actions: `__html:${String(row.status) === "InReview"
          ? `<button class="btn small primary" type="button" data-document-approve="${safe(row.id)}">${safe(t("Common.Approve") || "Aprobar")}</button>`
          : `<span class="metric-label">${safe(displayLabel(row.status))}</span>`}`
      }))
    : rows;
  const columns = canApproveDocuments ? [...module.columns, "actions"] : module.columns;
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
    ${tableCard(module.title, displayRows, columns)}`;
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
  document.querySelectorAll("[data-document-approve]").forEach(button => {
    button.addEventListener("click", async () => {
      try {
        setLoadingButton(button, true, t("Common.Processing") || "Procesando...");
        await request(`/tenants/${state.tenantId}/documents/${button.dataset.documentApprove}/decision`, {
          method: "POST",
          loadingContext: "save",
          body: { decision: 0, comments: "Aprobado por Quality Manager desde Document Management" }
        });
        toast(t("Common.Saved") || "Documento aprobado.", "success");
        await renderRoute();
      } catch (error) {
        toast(friendlyErrorMessage(error), "error");
      } finally {
        setLoadingButton(button, false);
      }
    });
  });
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
      ${metric("Productos RA", metrics.documents || 0, "Medical devices")}
      ${metric("Expedientes", metrics.audits || 0, "Registration dossiers")}
      ${metric("Fabricantes", metrics.capas || 0, "Manufacturer profiles")}
      ${metric("Imports", metrics.risks || 0, "REGUTRACK jobs")}
      ${metric("Licencias OP", metrics.indicators || 0, "Operating licenses")}
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
        <span class="status-pill ok">PLATFORM.TENANT.CREATE</span>
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
    ...(tenant.status === 0 || tenant.status === "Draft" ? [{ severity: "warning", title: "Tenant en Draft", message: "Complete onboarding y active el tenant para uso productivo." }] : []),
    ...(health.overallStatus && health.overallStatus !== "Healthy" && health.overallStatus !== 0 ? [{ severity: "warning", title: "Health Center requiere atencion", message: "Revisar senales operativas degradadas o faltantes." }] : []),
    ...(hasAnyPermission(["TENANT.DOMAINS"]) && !(center.domains || []).length ? [{ severity: "warning", title: "Sin dominios configurados", message: "Agregar un dominio default o custom antes de onboarding enterprise." }] : []),
    ...(hasAnyPermission(["TENANT.SSO"]) && !(center.ssoConfigurations || []).length ? [{ severity: "info", title: "Sin SSO configurado", message: "Los tenants enterprise normalmente requieren OIDC, SAML, LDAP o Active Directory." }] : [])
  ];
  const storageGb = ((metrics.storageBytes || 0) / 1024 / 1024 / 1024).toFixed(2);
  const usedUsers = `${metrics.users || 0}/${tenant.subscription.maxUsers}`;
  const storageUse = `${storageGb}/${tenant.subscription.maxStorageGb} GB`;
  const statusName = tenantStatusName(tenant.status);
  const planName = subscriptionPlanName(tenant.subscription.plan);
  const subscriptionStatusName = subscriptionStateName(tenant.subscription.status);
  const tabs = tenantAdminTabs();
  const panelCtx = { tenant, center, metrics, health, timeline };
  const savedTab = localStorage.getItem("c360.tenantTab");
  const activeTenantTab = tabs.some(t => t.key === savedTab) ? savedTab : (tabs[0]?.key || "general");

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
        ${tabs.length
          ? tabs.map((tab, index) => `<button class="tenant-tab ${tab.key === activeTenantTab ? "active" : ""}" type="button" data-tab="${tab.key}"><span>${index + 1}</span>${tab.label}</button>`).join("")
          : `<p class="metric-label">No tiene pestañas habilitadas para su rol.</p>`}
      </aside>
      <div class="tenant-tab-panels">
        ${tabs.map(tab => tenantAdminPanelHtml(tab.key, panelCtx)).join("")}
      </div>
    </section>`;

  applyTenantActiveTab(activeTenantTab);
  bindTenantAdministrationCenter(tenant, center);
}
function tenantAdministrationNavigation(tenant) {
  const isPlatformOperator = hasAnyPermission(["PLATFORM.DASHBOARD.READ", "PLATFORM.TENANT.READ", "PLATFORM.TENANT.CREATE"]);
  if (!isPlatformOperator) {
    return `
    <section class="platform-command" aria-label="Contexto del tenant">
      <span class="metric-label">${t("Ui.CurrentOrganization")}: ${safe(tenant.commercialName || tenant.name)}</span>
    </section>`;
  }
  const cameFromPlatform = localStorage.getItem("c360.tenantReturn") === "superadmin-platform";
  return `
    <section class="platform-command" aria-label="Navegacion de tenants">
      <button class="btn" type="button" id="back-to-platform-tenants">${cameFromPlatform ? "Volver a Tenants" : "Ver todos los tenants"}</button>
      <button class="btn" type="button" id="back-to-superadmin-platform">SuperAdmin Platform</button>
      <span class="metric-label">Tenant actual: ${safe(tenant.commercialName || tenant.name)} · ${shortId(tenant.id)}</span>
    </section>`;
}

/** Pestañas del TAC filtradas por permiso real del rol (no mostrar lo que el usuario no puede operar). */
function tenantAdminTabs() {
  const all = [
    { key: "general", label: "Informacion General", perms: ["TENANT.UPDATE"] },
    { key: "branding", label: "Branding", perms: ["TENANT.BRANDING"] },
    { key: "security", label: "Seguridad", perms: ["TENANT.SECURITY"] },
    { key: "users", label: "Usuarios", perms: ["TENANT.USERS"] },
    { key: "rbac", label: "Roles & Permisos", perms: ["RBAC.MANAGE", "TENANT.ROLES"] },
    { key: "licensing", label: "Licenciamiento", perms: ["TENANT.BILLING"] },
    { key: "domains", label: "Dominios", perms: ["TENANT.DOMAINS"] },
    { key: "sso", label: "SSO", perms: ["TENANT.SSO"] },
    { key: "apikeys", label: "API Keys", perms: ["TENANT.API_KEYS"] },
    { key: "webhooks", label: "Webhooks", perms: ["TENANT.WEBHOOKS"] },
    { key: "storage", label: "Storage", perms: ["TENANT.STORAGE", "STORAGE.READ", "STORAGE.CREATE"] },
    { key: "notifications", label: "Notificaciones", perms: ["TENANT.NOTIFICATIONS", "NOTIFICATION.READ", "NOTIFICATION.ADMIN"] },
    { key: "health", label: "Health & Backups", perms: ["TENANT.HEALTH", "TENANT.BACKUP"] },
    { key: "audit", label: "Auditoria", perms: ["TENANT.AUDIT"] },
    { key: "state", label: "Estado", perms: ["PLATFORM.TENANT.STATUS", "PLATFORM.TENANT.UPDATE"] }
  ];
  return all.filter(tab => hasAnyPermission(tab.perms));
}

function tenantAdminPanelHtml(key, ctx) {
  const { tenant, center, metrics, health, timeline } = ctx;
  switch (key) {
    case "general": return tenantGeneralPanel(tenant);
    case "branding": return tenantBrandingPanel(tenant);
    case "security": return tenantSecurityPanel(tenant);
    case "users": return tenantUsersPanel(center.users || { users: [], roles: [] }, tenant, metrics);
    case "rbac": return tenantRbacPanel(center.users || { users: [], roles: [] });
    case "licensing": return tenantLicensingPanel(tenant, metrics, center.license);
    case "domains": return tenantDomainsPanel(center.domains || []);
    case "sso": return tenantSsoPanel(center.ssoConfigurations || []);
    case "apikeys": return tenantApiKeysPanel(center.apiCredentials || []);
    case "webhooks": return tenantWebhooksPanel(center.webhooks || []);
    case "storage": return tenantStoragePanel(metrics);
    case "notifications": return tenantNotificationsPanel(metrics);
    case "health": return tenantHealthPanel(health);
    case "audit": return tenantAuditPanel(timeline);
    case "state": return tenantStatePanel(tenant);
    default: return "";
  }
}

function tenantGeneralPanel(tenant) {
  return `
    <section class="tenant-panel" data-panel="general">
      <div class="section-heading"><div><h2 class="section-title">Informacion General</h2><p class="metric-label">Campos empresariales editables con TenantId y CreatedAt inmutables.</p></div><span class="status-pill ok">Auditable</span></div>
      <form id="tenant-general-form" class="form-stack" autocomplete="off">
        <div class="grid two">
          ${inputField("name", "Tenant Name", tenant.name, "text", false, true, { id: "tenantName", maxLength: 180, autocomplete: "organization", help: "Nombre interno del tenant. No puede estar vacío." })}
          ${inputField("legalName", "Razon Social", tenant.legalName, "text", false, true, { id: "tenantLegalName", maxLength: 220, help: "Nombre legal que aparece en contratos y facturación." })}
          ${inputField("commercialName", "Nombre Comercial", tenant.commercialName, "text", false, true, { id: "tenantCommercialName", maxLength: 180, help: "Nombre visible para usuarios del cliente." })}
          ${inputField("taxIdentifier", "RUC / Tax ID", tenant.taxIdentifier, "text", false, true, { id: "tenantTaxIdentifier", maxLength: 80, pattern: "^[A-Za-z0-9][A-Za-z0-9.\\-]{2,78}[A-Za-z0-9]$", title: "Use letras, números, guiones o puntos. No use símbolos especiales.", help: "Identificación fiscal. Se normaliza a mayúsculas." })}
          ${inputField("industry", "Industria", tenant.industry || "Compliance", "text", false, true, { id: "tenantIndustry", maxLength: 120, help: "Sector principal del cliente." })}
          ${selectField("countryCode", "Pais", tenant.countryCode, countryOptions(), "Código ISO del país. Panamá = PA.")}
          ${selectField("currency", "Moneda", tenant.currency, currencyOptions(), "Moneda ISO-4217 usada para el tenant.")}
          ${inputField("phone", "Telefono", tenant.phone || "", "tel", false, false, { id: "tenantPhone", placeholder: "+507 6000-0000", pattern: "^\\+?(?:\\d| |\\(|\\)|-){7,40}(?:\\s?(?:ext\\.?|x)\\s?\\d{1,8})?$", title: "Use un teléfono válido. Ejemplo: +507 6000-0000", help: "Puede incluir código de país y extensión." })}
          ${inputField("email", "Correo", tenant.email || "", "email", false, false, { id: "tenantContactEmail", maxLength: 180, placeholder: "contacto@empresa.com", autocomplete: "off", help: "Correo de contacto de la empresa (no el login del usuario). En tenant nuevo puede estar vacío." })}
          ${inputField("website", "Sitio Web", tenant.website || "", "url", false, false, { id: "tenantWebsite", maxLength: 250, placeholder: "https://empresa.com", help: "Debe iniciar con http:// o https://." })}
          ${inputField("city", "Ciudad", tenant.city || "", "text", false, false, { id: "tenantCity", maxLength: 120 })}
          ${inputField("province", "Provincia", tenant.province || "", "text", false, false, { id: "tenantProvince", maxLength: 120 })}
          ${inputField("postalCode", "Codigo Postal", tenant.postalCode || "", "text", false, false, { id: "tenantPostalCode", maxLength: 20, help: "Formato dependiente del país." })}
          ${inputField("addressLine1", "Direccion", tenant.addressLine1 || "", "text", false, false, { id: "tenantAddressLine1", maxLength: 220 })}
        </div>
        <div class="field"><label for="tenantDescription">Descripcion</label><textarea id="tenantDescription" name="description" rows="3" autocomplete="off">${safe(tenant.description || "")}</textarea></div>
        <div class="field"><label for="generalChangeReason">Motivo del cambio</label><input id="generalChangeReason" name="changeReason" placeholder="Opcional, queda en auditoria" autocomplete="off"></div>
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
      <div class="section-heading"><div><h2 class="section-title">User Administration</h2><p class="metric-label">Crear, bloquear, desbloquear, restablecer contraseña, reset MFA, roles y sesiones por tenant.</p></div><span class="status-pill ok">${users.length}/${metrics.users || 0}</span></div>
      <form id="tenant-user-form" class="form-stack" autocomplete="off">
        <div class="grid two">
          ${inputField("email", "Email del usuario", "", "email", false, true, { id: "newUserEmail", placeholder: "usuario@empresa.com", autocomplete: "off", help: "Correo de login de la persona. No use el nombre del tenant." })}
          ${inputField("fullName", "Nombre completo", "", "text", false, true, { id: "newUserFullName", autocomplete: "off" })}
          ${inputField("initialPassword", "Contraseña inicial", "", "password", false, true, { id: "newUserInitialPassword", minLength: 12, autocomplete: "new-password", help: "Mínimo 12 caracteres con mayúscula, minúscula, número y símbolo." })}
          ${inputField("confirmPassword", "Confirmar contraseña", "", "password", false, true, { id: "newUserConfirmPassword", minLength: 12, autocomplete: "new-password", help: "Debe coincidir con la contraseña inicial." })}
          <div class="field"><label for="newUserRoleId">Rol inicial</label><select id="newUserRoleId" name="roleId"><option value="">Sin rol</option>${roles.map(role => `<option value="${role.id}" ${role.name === "Tenant Administrator" ? "selected" : ""}>${safe(role.name)}</option>`).join("")}</select><small>Para el primer usuario del tenant elija Tenant Administrator.</small></div>
          <label class="toggle-row"><input type="checkbox" name="forcePasswordChange" checked> Forzar cambio de contraseña en el primer login</label>
          ${inputField("changeReason", "Motivo", "Alta operativa TAC", "text", false, false, { id: "newUserChangeReason", autocomplete: "off" })}
        </div>
        <button class="btn primary" type="submit">Crear / Invitar usuario</button>
      </form>
      ${tableCard("Usuarios del tenant", users.map(user => ({
        email: user.email,
        name: user.fullName,
        roles: `__html:${(user.roleIds || []).map(roleId => {
          const role = roles.find(item => item.id === roleId);
          return `<span class="status-pill">${safe(role?.name || roleId)}</span> <button class="btn small danger" type="button" data-user-role-revoke="${user.id}" data-role-id="${roleId}">Retirar</button>`;
        }).join(" ") || "Sin rol"}`,
        status: enumName(["Invited", "Active", "Disabled", "Locked"], user.status),
        mfa: user.mfaEnabled ? "Enabled" : "Disabled",
        lastLogin: formatDate(user.lastLoginAtUtc),
        sessions: (user.sessions || []).filter(session => session.isActive).length,
        actions: `__html:<button class="btn small" type="button" data-user-edit="${user.id}" data-user-email="${safe(user.email)}" data-user-name="${safe(user.fullName)}">Editar</button> <button class="btn small" type="button" data-user-password="${user.id}" data-user-email="${safe(user.email)}">Restablecer contraseña</button> <button class="btn small" data-user-action="Active" data-user-id="${user.id}">Reactivar</button> <button class="btn small" data-user-action="Disabled" data-user-id="${user.id}">Desactivar</button>${hasAnyPermission(["TENANT.SECURITY"]) ? ` <button class="btn small" data-user-mfa="${user.id}">Reset MFA</button>` : ""} <button class="btn small" data-user-sessions="${user.id}">Cerrar sesiones</button>`
      })), ["email", "name", "roles", "status", "mfa", "lastLogin", "sessions", "actions"])}
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
    const password = String(body.initialPassword || "");
    const confirmPassword = String(body.confirmPassword || "");
    if (password !== confirmPassword) {
      throw new Error("La contraseña y su confirmación no coinciden.");
    }
    delete body.confirmPassword;
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
  bindTenantAction("[data-user-action]", button => request(`/tenants/${state.tenantId}/users/${button.dataset.userId}/status`, { method: "PATCH", body: { status: button.dataset.userAction === "Active" ? 1 : 3, changeReason: "Status update from TAC UI" }, loadingContext: "save" }));
  bindTenantAction("[data-user-role-revoke]", button => request(
    `/tenants/${state.tenantId}/users/${button.dataset.userRoleRevoke}/roles/${button.dataset.roleId}?changeReason=Revoked%20from%20TAC%20UI`,
    { method: "DELETE", loadingContext: "save" }
  ));
  document.querySelectorAll("[data-user-edit]").forEach(button => {
    button.addEventListener("click", () => openTenantUserEditDialog(
      button.dataset.userEdit,
      button.dataset.userEmail || "",
      button.dataset.userName || ""
    ));
  });
  document.querySelectorAll("[data-user-password]").forEach(button => {
    button.addEventListener("click", () => openAdminPasswordResetDialog(button.dataset.userPassword, button.dataset.userEmail || ""));
  });
  bindEnterpriseFieldValidation();
}

function applyTenantActiveTab(tabKey) {
  const tabs = tenantAdminTabs();
  const selected = tabs.some(tab => tab.key === tabKey) ? tabKey : (tabs[0]?.key || "");
  if (!selected) return;
  localStorage.setItem("c360.tenantTab", selected);
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
      } catch (error) {
        toast(friendlyErrorMessage(error), "error");
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
    } catch (error) {
      toast(friendlyErrorMessage(error), "error");
    } finally {
      setLoadingButton(button, false);
    }
  });
}

function openTenantUserEditDialog(userId, email, fullName) {
  document.querySelector("#tenant-user-edit-dialog")?.remove();
  const overlay = document.createElement("div");
  overlay.id = "tenant-user-edit-dialog";
  overlay.className = "admin-password-dialog-overlay";
  overlay.innerHTML = `
    <form class="admin-password-dialog form-stack" autocomplete="off">
      <h3>Editar usuario</h3>
      <div class="field">
        <label for="tenantEditUserEmail">Correo</label>
        <input id="tenantEditUserEmail" name="email" type="email" maxlength="256" value="${safe(email)}" required>
      </div>
      <div class="field">
        <label for="tenantEditUserName">Nombre completo</label>
        <input id="tenantEditUserName" name="fullName" type="text" maxlength="180" value="${safe(fullName)}" required>
      </div>
      <div class="field">
        <label for="tenantEditUserReason">Motivo</label>
        <input id="tenantEditUserReason" name="changeReason" type="text" value="Actualización de usuario por TAC">
      </div>
      <div class="button-row">
        <button class="btn subtle" type="button" data-dialog-cancel>Cancelar</button>
        <button class="btn primary" type="submit">Guardar usuario</button>
      </div>
    </form>`;
  document.body.appendChild(overlay);
  const close = () => overlay.remove();
  overlay.querySelector("[data-dialog-cancel]")?.addEventListener("click", close);
  overlay.addEventListener("click", event => {
    if (event.target === overlay) close();
  });
  overlay.querySelector("form")?.addEventListener("submit", async event => {
    event.preventDefault();
    const button = event.currentTarget.querySelector("button[type='submit']");
    const form = new FormData(event.currentTarget);
    try {
      setLoadingButton(button, true, "Guardando...");
      await request(`/tenants/${state.tenantId}/users/${userId}`, {
        method: "PUT",
        loadingContext: "save",
        body: {
          email: String(form.get("email") || "").trim(),
          fullName: String(form.get("fullName") || "").trim(),
          changeReason: String(form.get("changeReason") || "").trim() || null
        }
      });
      close();
      toast("Usuario actualizado y auditado.", "success");
      await renderRoute();
    } catch (error) {
      toast(friendlyErrorMessage(error), "error");
    } finally {
      setLoadingButton(button, false);
    }
  });
  overlay.querySelector("input")?.focus();
}

function openAdminPasswordResetDialog(userId, userEmail) {
  document.querySelector("#admin-password-dialog")?.remove();
  const overlay = document.createElement("div");
  overlay.id = "admin-password-dialog";
  overlay.className = "admin-password-dialog-overlay";
  overlay.innerHTML = `
    <form class="admin-password-dialog form-stack" autocomplete="off">
      <h3>Restablecer contraseña</h3>
      <p class="metric-label">Usuario: <strong>${safe(userEmail || userId)}</strong>. Entregue la nueva contraseña de forma segura. Se cerrarán las sesiones activas.</p>
      <div class="field">
        <label for="adminResetPassword">Nueva contraseña</label>
        <input id="adminResetPassword" name="newPassword" type="password" minlength="12" required autocomplete="new-password">
        <small>Mínimo 12 caracteres con mayúscula, minúscula, número y símbolo.</small>
      </div>
      <div class="field">
        <label for="adminResetConfirmPassword">Confirmar contraseña</label>
        <input id="adminResetConfirmPassword" name="confirmPassword" type="password" minlength="12" required autocomplete="new-password">
      </div>
      <label class="toggle-row"><input type="checkbox" name="forcePasswordChange" checked> Forzar cambio en el próximo login</label>
      <div class="field">
        <label for="adminResetReason">Motivo</label>
        <input id="adminResetReason" name="changeReason" type="text" value="Restablecimiento por administrador TAC" autocomplete="off">
      </div>
      <div class="button-row">
        <button class="btn subtle" type="button" data-dialog-cancel>Cancelar</button>
        <button class="btn primary" type="submit">Restablecer</button>
      </div>
    </form>`;
  document.body.appendChild(overlay);

  const form = overlay.querySelector("form");
  const passwordInput = overlay.querySelector("#adminResetPassword");
  const confirmInput = overlay.querySelector("#adminResetConfirmPassword");

  const syncValidity = () => {
    const password = passwordInput.value || "";
    const confirm = confirmInput.value || "";
    passwordInput.setCustomValidity(
      password && !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{12,}$/.test(password)
        ? "Debe tener 12 caracteres, mayúscula, minúscula, número y símbolo."
        : ""
    );
    confirmInput.setCustomValidity(confirm && confirm !== password ? "Las contraseñas no coinciden." : "");
  };
  passwordInput.addEventListener("input", syncValidity);
  confirmInput.addEventListener("input", syncValidity);

  overlay.querySelector("[data-dialog-cancel]")?.addEventListener("click", () => overlay.remove());
  overlay.addEventListener("click", event => {
    if (event.target === overlay) {
      overlay.remove();
    }
  });

  form.addEventListener("submit", async event => {
    event.preventDefault();
    syncValidity();
    if (!form.reportValidity()) {
      return;
    }
    const data = new FormData(form);
    const button = form.querySelector("button[type='submit']");
    try {
      setLoadingButton(button, true, "Restableciendo...");
      await request(`/tenants/${state.tenantId}/users/${userId}/reset-password`, {
        method: "POST",
        loadingContext: "save",
        body: {
          newPassword: String(data.get("newPassword") || ""),
          forcePasswordChange: data.has("forcePasswordChange"),
          changeReason: String(data.get("changeReason") || "Restablecimiento por administrador TAC")
        }
      });
      overlay.remove();
      toast("Contraseña restablecida. Sesiones cerradas.", "success");
      await renderRoute();
    } catch (error) {
      toast(friendlyErrorMessage(error), "error");
    } finally {
      setLoadingButton(button, false);
    }
  });
}

function inputField(name, label, value, type = "text", disabled = false, required = false, options = {}) {
  const fieldId = options.id || name;
  const helpId = `${fieldId}-help`;
  const min = options.min !== undefined ? ` min="${safe(options.min)}"` : type === "number" ? " min=\"0\"" : "";
  const max = options.max !== undefined ? ` max="${safe(options.max)}"` : "";
  const maxLength = options.maxLength !== undefined ? ` maxlength="${safe(options.maxLength)}"` : "";
  const minLength = options.minLength !== undefined ? ` minlength="${safe(options.minLength)}"` : "";
  const pattern = options.pattern ? ` pattern="${safe(options.pattern)}"` : name.toLowerCase().includes("color") && type !== "color" ? " pattern=\"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$\"" : "";
  const title = options.title ? ` title="${safe(options.title)}"` : "";
  const placeholder = options.placeholder ? ` placeholder="${safe(options.placeholder)}"` : "";
  const autocomplete = options.autocomplete ? ` autocomplete="${safe(options.autocomplete)}"` : type === "password" ? " autocomplete=\"new-password\"" : " autocomplete=\"off\"";
  const describedBy = options.help ? ` aria-describedby="${helpId}"` : "";
  const help = options.help ? `<small id="${helpId}">${safe(options.help)}</small>` : "";
  return `<div class="field"><label for="${fieldId}">${safe(label)}</label><input id="${fieldId}" name="${name}" type="${type}" value="${safe(value ?? "")}" ${disabled ? "disabled" : ""} ${required ? "required" : ""}${min}${max}${maxLength}${minLength}${pattern}${title}${placeholder}${autocomplete}${describedBy}>${help}</div>`;
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
  } else if (field.name === "confirmPassword") {
    const passwordField = field.form?.querySelector('[name="initialPassword"], [name="newPassword"]');
    if (value && passwordField && value !== passwordField.value) {
      message = "Las contraseñas no coinciden.";
    }
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
  if (!value) return "n/a";
  return window.I18n.formatDate(value, {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit"
  });
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
  const label = navLabel(key) || modules[key]?.title || key;
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
    "template-builder": "Compliance Studio",
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
    ["template-builder", "Compliance Studio", "Form Engine visual"],
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
      const detail = parseApiErrorDetail(text, response.status);
      if (response.status === 401 && !options.anonymous) {
        endSessionGracefully("expired");
      }
      throw new Error(detail);
    }
    if (response.status === 204) {
      return {};
    }
    return response.json();
  } finally {
    stopLoading();
  }
}

function parseApiErrorDetail(text, status) {
  if (!text) {
    return status === 401
      ? "Sesion expirada. Inicia sesion nuevamente."
      : "No se pudo completar la solicitud. Intenta de nuevo.";
  }
  const raw = String(text).trim();
  const jsonStart = raw.indexOf("{");
  const candidate = jsonStart >= 0 ? raw.slice(jsonStart) : raw;
  try {
    const payload = JSON.parse(candidate);
    const detail = payload.detail || payload.title || payload.error || payload.message;
    if (detail && typeof detail === "string") {
      return detail;
    }
  } catch {
    // plain text body
  }
  if (raw.length < 220 && !raw.startsWith("{") && !raw.startsWith("<") && !/^\d{3}\s/.test(raw)) {
    return raw;
  }
  return status === 400
    ? "No se pudo validar la solicitud. Revisa los datos e intenta de nuevo."
    : `Error ${status}. Intenta de nuevo o contacta al administrador.`;
}

function friendlyErrorMessage(error) {
  return parseApiErrorDetail(String(error?.message || error || "Error inesperado"), 400);
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
window.startGlobalLoading = startGlobalLoading;

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
  const raw = String(message || "");
  const key = window.I18n?.resolveKeyFromText?.(raw);
  node.textContent = key ? t(key) : raw;
  region.appendChild(node);
  setTimeout(() => node.remove(), 4200);
}
window.toast = toast;

let sessionWatchTimer = null;

function tokenExpiresAtMs(token) {
  const payload = decodeTokenPayload(token);
  if (!payload?.exp) return null;
  return Number(payload.exp) * 1000;
}

function isAccessTokenExpired(token) {
  const expiresAt = tokenExpiresAtMs(token);
  return expiresAt != null && Date.now() >= expiresAt - 250;
}

function isSessionExpiredError(error) {
  const message = String(error?.message || error || "");
  return message.includes("401") || /sesi[oó]n expir/i.test(message) || /session expired/i.test(message);
}

function clearSessionLocalState() {
  if (sessionWatchTimer) {
    clearTimeout(sessionWatchTimer);
    sessionWatchTimer = null;
  }
  localStorage.removeItem("c360.token");
  localStorage.removeItem("c360.tenantId");
  localStorage.removeItem("c360.email");
  localStorage.removeItem("c360.userId");
  localStorage.removeItem("c360.role");
  localStorage.removeItem("c360.displayName");
  sessionStorage.clear();
  state.token = null;
  state.permissions = [];
  state.role = null;
  state.displayName = null;
  state.tenantId = null;
  state.email = null;
  state.userId = null;
  state.mfaChallenge = null;
}

function scheduleSessionWatch() {
  if (sessionWatchTimer) {
    clearTimeout(sessionWatchTimer);
    sessionWatchTimer = null;
  }
  if (!state.token) return;
  const expiresAt = tokenExpiresAtMs(state.token);
  if (!expiresAt) return;
  const delay = Math.max(0, expiresAt - Date.now() - 400);
  sessionWatchTimer = setTimeout(() => endSessionGracefully("expired"), delay);
}

function endSessionGracefully(reason = "expired", options = {}) {
  if (state.sessionEnding) return;
  state.sessionEnding = true;
  clearSessionLocalState();
  showSessionEndedDialog(reason, options);
}

function showSessionEndedDialog(reason = "expired", options = {}) {
  document.querySelector("#session-ended-overlay")?.remove();

  const overlay = document.createElement("div");
  overlay.id = "session-ended-overlay";
  overlay.className = "session-ended-overlay";
  overlay.setAttribute("role", "dialog");
  overlay.setAttribute("aria-modal", "true");
  overlay.setAttribute("aria-labelledby", "session-ended-title");

  const title = t("Login.SessionEndedTitle");
  const body = reason === "expired"
    ? t("Login.SessionEndedBody")
    : t("Login.SessionEndedBodyGeneric");
  const cta = t("Login.SessionEndedContinue");

  overlay.innerHTML = `
    <div class="session-ended-card">
      <div class="session-ended-glow" aria-hidden="true"></div>
      <div class="session-ended-icon" aria-hidden="true">
        <svg viewBox="0 0 64 64" width="56" height="56" fill="none">
          <circle cx="32" cy="32" r="30" stroke="currentColor" stroke-width="2" opacity="0.25"/>
          <path d="M32 18v18" stroke="currentColor" stroke-width="3.5" stroke-linecap="round"/>
          <circle cx="32" cy="44" r="2.5" fill="currentColor"/>
        </svg>
      </div>
      <p class="session-ended-eyebrow">${escapeHtml(t("Login.Title"))}</p>
      <h2 id="session-ended-title">${escapeHtml(title)}</h2>
      <p class="session-ended-body">${escapeHtml(body)}</p>
      <button type="button" class="btn primary session-ended-cta" id="session-ended-continue">${escapeHtml(cta)}</button>
    </div>`;

  document.body.appendChild(overlay);

  const finish = () => {
    overlay.classList.add("is-leaving");
    setTimeout(() => {
      overlay.remove();
      state.sessionEnding = false;
      location.hash = "#/login";
      render();
    }, 280);
  };

  overlay.querySelector("#session-ended-continue")?.addEventListener("click", finish);
  if (!options.silentBoot) {
    requestAnimationFrame(() => overlay.classList.add("is-visible"));
  } else {
    overlay.classList.add("is-visible");
  }

  // Auto-continue after a short pause so the message is readable, then logout UI.
  setTimeout(finish, 6500);
}
window.endSessionGracefully = endSessionGracefully;

async function ensureFormTemplateBuilder() {
  if (typeof window.renderFormTemplateBuilder === "function") {
    return;
  }

  await new Promise((resolve, reject) => {
    const existing = document.querySelector('script[data-cs-builder="1"]');
    if (existing) {
      existing.addEventListener("load", () => resolve(), { once: true });
      existing.addEventListener("error", () => reject(new Error("No se pudo cargar form-template-builder.js")), { once: true });
      return;
    }
    const script = document.createElement("script");
    script.src = "/form-template-builder.js?v=i18n-2";
    script.async = false;
    script.dataset.csBuilder = "1";
    script.onload = () => resolve();
    script.onerror = () => reject(new Error("No se pudo cargar /form-template-builder.js (404 o bloqueo)."));
    document.head.appendChild(script);
  });

  if (typeof window.renderFormTemplateBuilder !== "function") {
    throw new Error("Compliance Studio no inicializó renderFormTemplateBuilder. Recarga forzada (Ctrl+F5).");
  }
}

async function ensureRegulatoryAffairs() {
  if (typeof window.renderRegulatoryAffairs === "function") {
    return;
  }
  await new Promise((resolve, reject) => {
    const existing = document.querySelector('script[data-ra-console="1"]');
    if (existing) {
      existing.addEventListener("load", () => resolve(), { once: true });
      existing.addEventListener("error", () => reject(new Error("No se pudo cargar regulatory-affairs.js")), { once: true });
      return;
    }
    const script = document.createElement("script");
    script.src = "/regulatory-affairs.js?v=owner-picker-1";
    script.async = false;
    script.dataset.raConsole = "1";
    script.onload = () => resolve();
    script.onerror = () => reject(new Error("No se pudo cargar /regulatory-affairs.js"));
    document.head.appendChild(script);
  });
  if (typeof window.renderRegulatoryAffairs !== "function") {
    throw new Error("RA Console no inicializó renderRegulatoryAffairs.");
  }
}

async function ensureAlertCenter() {
  if (typeof window.renderAlertCenter === "function") {
    return;
  }
  await new Promise((resolve, reject) => {
    const existing = document.querySelector('script[data-alert-center="1"]');
    if (existing) {
      existing.addEventListener("load", () => resolve(), { once: true });
      existing.addEventListener("error", () => reject(new Error("No se pudo cargar alert-center.js")), { once: true });
      return;
    }
    const script = document.createElement("script");
    script.src = "/alert-center.js?v=foundation-1";
    script.async = false;
    script.dataset.alertCenter = "1";
    script.onload = () => resolve();
    script.onerror = () => reject(new Error("No se pudo cargar /alert-center.js"));
    document.head.appendChild(script);
  });
  if (typeof window.renderAlertCenter !== "function") {
    throw new Error("Alert Center no inicializó renderAlertCenter.");
  }
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
  return navLabel(state.route) || enterpriseWorkspaces[state.route]?.title || "Workspace";
}

function currentRouteGroup() {
  return routeMetadata[state.route] ? navGroupLabel(routeMetadata[state.route].groupKey) : (enterpriseWorkspaces[state.route] ? "Enterprise" : "Workspace");
}

function routeFromLabel(value) {
  if (!value) return null;
  const normalized = value.toLowerCase();
  const match = Object.entries(routeMetadata).find(([key]) => navLabel(key).toLowerCase() === normalized);
  if (match) return match[0];
  const partial = Object.entries(routeMetadata).find(([key]) => navLabel(key).toLowerCase().includes(normalized));
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
