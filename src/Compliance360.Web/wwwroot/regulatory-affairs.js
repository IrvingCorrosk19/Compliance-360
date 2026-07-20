(() => {
  const P = {
    productManage: "REGULATORY.PRODUCT.MANAGE",
    dossierCreate: "REGULATORY.DOSSIER.CREATE",
    dossierUpdate: "REGULATORY.DOSSIER.UPDATE",
    dossierReview: "REGULATORY.DOSSIER.REVIEW",
    approveInternal: "REGULATORY.DOSSIER.APPROVE_FOR_SUBMISSION",
    submit: "REGULATORY.DOSSIER.SUBMIT",
    approveExternal: "REGULATORY.DOSSIER.APPROVE",
    reqManage: "REGULATORY.REQUIREMENT.MANAGE",
    obsManage: "REGULATORY.OBSERVATION.MANAGE",
    configure: "REGULATORY.CONFIGURE",
    sodManage: "REGULATORY.SOD.MANAGE",
    mfrManage: "REGULATORY.MANUFACTURER_DOCUMENT.MANAGE",
    licenseManage: "REGULATORY.LICENSE.MANAGE",
    reportRead: "REGULATORY.REPORT.READ",
    sodOverride: "REGULATORY.SOD.EMERGENCY_OVERRIDE"
  };

  function can(code) {
    if (typeof window.hasPermission === "function") return window.hasPermission(code);
    try {
      const token = localStorage.getItem("c360.token");
      const payload = JSON.parse(atob(token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")));
      const perms = payload.permission || [];
      const list = Array.isArray(perms) ? perms : [perms];
      return list.includes(code);
    } catch { return false; }
  }

  function roleProfile() {
    // Manual (roles.json): TAC = configura tenant sin gestionar productos;
    // RA-ADM = configura módulo RA y puede crear producto + expediente.
    if (can(P.configure) && !can(P.productManage)) return "tac";
    if (can(P.configure) && can(P.productManage)) return "admin";
    if (can(P.approveExternal) && can(P.sodManage) && !can(P.dossierCreate)) return "manager";
    if (can(P.approveExternal) && !can(P.sodManage) && !can(P.dossierCreate)) return "qm";
    if (can(P.approveInternal) && !can(P.submit) && !can(P.dossierCreate)) return "approver";
    if (can(P.submit) && !can(P.approveInternal) && !can(P.dossierCreate)) return "submitter";
    if (can(P.dossierReview) && !can(P.dossierCreate)) return "reviewer";
    if (can(P.dossierCreate)) return "specialist";
    return "viewer";
  }

  const STATUS_LABELS = {
    Draft: "Borrador",
    Planning: "Planificación / Preparación",
    WaitingManufacturerDocuments: "Espera docs fábrica",
    DocumentsReceived: "Docs recibidos",
    Assembling: "Armando expediente",
    UnderTechnicalReview: "En revisión técnica",
    CorrectionRequested: "Corrección solicitada",
    ReadyForSubmission: "Técnicamente completo / Listo para aprobación interna",
    ApprovedForSubmission: "Aprobado internamente para sometimiento",
    Submitted: "Sometido ante autoridad",
    UnderAuthorityReview: "En revisión de la autoridad",
    Observed: "Observado por autoridad",
    CorrectingObservation: "Corrigiendo observación",
    Resubmitted: "Resometido",
    Approved: "Aprobación registrada de MINSA/CSS (externa)",
    Rejected: "Rechazo externo de la autoridad",
    Cancelled: "Cancelado",
    Closed: "Cerrado",
    OnHold: "En espera"
  };

  const FLOW_STEPS = [
    "Planning", "WaitingManufacturerDocuments", "DocumentsReceived", "Assembling",
    "UnderTechnicalReview", "CorrectionRequested", "ReadyForSubmission", "ApprovedForSubmission", "Submitted", "UnderAuthorityReview",
    "Observed", "Approved"
  ];

  const ALL_VIEWS = [
    ["dashboard", "Dashboard"],
    ["portfolio", "Portafolio"],
    ["pipeline", "Pipeline"],
    ["dossiers", "Expedientes"],
    ["registrations", "Registros CT/RS"],
    ["manufacturers", "Fabricantes"],
    ["licenses", "Licencias"],
    ["alerts", "Alertas"],
    ["import", "Importación"],
    ["config", "Configuración"],
    ["sod", "SoD Settings"]
  ];

  const PIPELINE_COLUMNS = [
    "Planning", "WaitingManufacturerDocuments", "DocumentsReceived", "Assembling",
    "UnderTechnicalReview", "CorrectionRequested", "ReadyForSubmission", "ApprovedForSubmission", "Submitted", "UnderAuthorityReview",
    "Observed", "CorrectingObservation", "Approved", "Vencido", "Renovacion"
  ];

  function resolveTenantId() {
    let tenantId = window.state?.tenantId;
    if (tenantId) return tenantId;
    const token = localStorage.getItem("c360.token");
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")));
      return payload.tenant_id || payload.tid || null;
    } catch {
      return null;
    }
  }

  async function api(path, options = {}) {
    const token = localStorage.getItem("c360.token");
    const tenantId = resolveTenantId();
    const res = await fetch(`/api/v1/tenants/${tenantId}/regulatory${path}`, {
      ...options,
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
        ...(options.headers || {})
      },
      body: options.body !== undefined ? JSON.stringify(options.body) : undefined
    });
    const text = await res.text();
    let data = null;
    try { data = text ? JSON.parse(text) : null; } catch { data = { raw: text }; }
    if (!res.ok) {
      const msg = data?.detail || data?.title || data?.error || data?.Error || text || res.statusText;
      throw new Error(typeof msg === "string" ? msg : JSON.stringify(msg));
    }
    return data;
  }

  async function apiV2(path, options = {}) {
    const token = localStorage.getItem("c360.token");
    const tenantId = resolveTenantId();
    const res = await fetch(`/api/v2/tenants/${tenantId}/regulatory/dossiers${path}`, {
      ...options,
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
        ...(options.headers || {})
      },
      body: options.body !== undefined ? JSON.stringify(options.body) : undefined
    });
    const text = await res.text();
    let data = null;
    try { data = text ? JSON.parse(text) : null; } catch { data = { raw: text }; }
    if (!res.ok) {
      const msg = data?.detail || data?.title || data?.error || data?.Error || text || res.statusText;
      const error = new Error(typeof msg === "string" ? msg : JSON.stringify(msg));
      error.status = res.status;
      error.payload = data;
      throw error;
    }
    return data;
  }

  async function uploadDossierEvidence(dossierId, file) {
    if (!(file instanceof File) || file.size === 0) {
      throw new Error(tr("Regulatory.EvidenceFileRequired", "Seleccione un archivo de evidencia."));
    }
    const token = localStorage.getItem("c360.token");
    const tenantId = resolveTenantId();
    const form = new FormData();
    form.append("file", file);
    const res = await fetch(`/api/v1/tenants/${tenantId}/regulatory/dossiers/${dossierId}/evidence`, {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: form
    });
    const text = await res.text();
    let data = null;
    try { data = text ? JSON.parse(text) : null; } catch { data = { raw: text }; }
    if (!res.ok) {
      const msg = data?.detail || data?.title || data?.error || data?.Error || text || res.statusText;
      throw new Error(typeof msg === "string" ? msg : JSON.stringify(msg));
    }
    return data;
  }

  function toast(msg, type = "info") {
    if (typeof window.toast === "function") window.toast(msg, type);
    else console.log(type, msg);
  }

  function esc(s) {
    return String(s ?? "").replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
  }

  function statusLabel(st) {
    const fallbacks = {
      UnderTechnicalReview: "En revisión técnica",
      CorrectionRequested: "Corrección solicitada"
    };
    return tr(`Regulatory.Workflow.State.${st}`, STATUS_LABELS[st] || fallbacks[st] || st);
  }

  // Manual = fuente de verdad. Pestañas por rol según data/roles.json (screens)
  // más las lecturas que la lista «Qué puedo hacer» concede explícitamente.
  const PROFILE_VIEWS = {
    tac: ["dashboard", "portfolio", "dossiers", "registrations", "licenses", "config", "sod"],
    admin: ["dashboard", "portfolio", "manufacturers", "licenses", "alerts", "import", "config", "sod"],
    manager: ["dashboard", "portfolio", "pipeline", "dossiers", "registrations", "alerts", "sod"],
    specialist: ["dashboard", "portfolio", "pipeline", "dossiers", "registrations", "manufacturers"],
    reviewer: ["dashboard", "pipeline", "dossiers", "registrations"],
    approver: ["dashboard", "pipeline", "dossiers", "registrations"],
    submitter: ["dashboard", "pipeline", "dossiers", "registrations"],
    viewer: ["dashboard", "portfolio", "pipeline", "dossiers", "registrations", "alerts"],
    qm: ["dashboard", "dossiers", "registrations"]
  };

  function visibleViews() {
    const allowed = PROFILE_VIEWS[roleProfile()] || ["dashboard"];
    return ALL_VIEWS.filter(([k]) => {
      if (!allowed.includes(k)) return false;
      if (k === "import" || k === "config") return can(P.configure);
      if (k === "sod") return can(P.sodManage) || can(P.configure);
      return true;
    });
  }

  function flowStrip(current) {
    return `<div class="ra-flow">${FLOW_STEPS.map(s => {
      const active = s === current;
      const done = FLOW_STEPS.indexOf(s) < FLOW_STEPS.indexOf(current);
      const cls = active ? "is-active" : done ? "is-done" : "is-todo";
      return `<span class="ra-badge ${cls}">${esc(statusLabel(s))}</span>`;
    }).join('<span class="ra-flow-sep">→</span>')}</div>
    <p class="ra-note"><strong>Importante:</strong> «Aprobado internamente para sometimiento» ≠ «Aprobación registrada de MINSA/CSS (externa)».</p>`;
  }

  async function renderRegulatoryAffairs(content) {
    const profile = roleProfile();
    const shell = document.createElement("div");
    shell.className = "ra-shell";
    shell.innerHTML = `
      <div>
        <span class="ra-badge">Regulatory Affairs · SoD activo · perfil ${esc(profile)}</span>
        <h2 class="ra-shell-title">Consola de Asuntos Regulatorios</h2>
        <p class="ra-shell-lead">Experiencia contextual por permisos. La UI oculta acciones no autorizadas; el backend es la fuente de seguridad.</p>
      </div>
      <nav class="ra-nav" id="ra-nav"></nav>
      <div id="ra-body">Cargando…</div>`;
    content.innerHTML = "";
    content.appendChild(shell);

    if (!document.getElementById("regulatory-affairs-css")) {
      const link = document.createElement("link");
      link.id = "regulatory-affairs-css";
      link.rel = "stylesheet";
      link.href = "/regulatory-affairs.css?v=evidence-del-2";
      document.head.appendChild(link);
    }

    const nav = shell.querySelector("#ra-nav");
    const body = shell.querySelector("#ra-body");
    let view = "dashboard";
    let selectedDossierId = null;
    let renderingView = false;

    function paintNav() {
      const views = visibleViews();
      nav.innerHTML = views.map(([k, label]) =>
        `<button type="button" data-view="${k}" class="${k === view ? "active" : ""}" ${renderingView ? "disabled" : ""}>${label}</button>`).join("");
      nav.querySelectorAll("button").forEach(btn => btn.addEventListener("click", async () => {
        if (renderingView) return;
        view = btn.dataset.view;
        selectedDossierId = null;
        renderingView = true;
        paintNav();
        await refresh();
        renderingView = false;
        paintNav();
      }));
    }

    async function refresh() {
      try {
        if (view === "dashboard") await showDashboard(body);
        else if (view === "portfolio") await showPortfolio(body);
        else if (view === "pipeline") await showPipeline(body);
        else if (view === "dossiers") await showDossiers(body, selectedDossierId);
        else if (view === "registrations") await showRegistrations(body);
        else if (view === "manufacturers") await showManufacturers(body);
        else if (view === "licenses") await showLicenses(body);
        else if (view === "alerts") await showAlerts(body);
        else if (view === "import") await showImport(body);
        else if (view === "config") await showConfig(body);
        else if (view === "sod") await showSod(body);
      } catch (err) {
        body.innerHTML = `<div class="ra-card"><strong>Error</strong><p>${esc(err.message)}</p></div>`;
      }
    }

    renderingView = true;
    paintNav();
    await refresh();
    renderingView = false;
    paintNav();
  }

  async function showDashboard(body) {
    const profile = roleProfile();
    const [dash, dossiers] = await Promise.all([api("/dashboard"), api("/dossiers")]);
    const list = dossiers || [];
    const filters = {
      specialist: d => ["Planning", "WaitingManufacturerDocuments", "DocumentsReceived", "Assembling", "CorrectionRequested", "Observed", "CorrectingObservation"].includes(d.status),
      reviewer: d => ["UnderTechnicalReview", "ReadyForSubmission", "Assembling", "DocumentsReceived"].includes(d.status) || (d.status === "Planning"),
      approver: d => d.status === "ReadyForSubmission",
      submitter: d => d.status === "ApprovedForSubmission" || d.status === "Submitted",
      manager: () => true,
      admin: () => true,
      viewer: () => true
    };
    const mine = list.filter(filters[profile] || (() => true));
    body.innerHTML = `
      <div class="ra-card mb">
        <h3>Cola contextual · ${esc(profile)}</h3>
        <p>${mine.length} expediente(s) relevantes</p>
        <ul>${mine.slice(0, 12).map(d =>
          `<li><button type="button" class="ra-chip" data-open="${d.id}"><strong>${esc(d.caseNumber)}</strong> · ${esc(statusLabel(d.status))}</button></li>`
        ).join("") || "<li>Sin pendientes</li>"}</ul>
      </div>
      <div class="ra-grid">
        ${metric("Productos", dash.productsTotal)}
        ${metric("CT activos", dash.registrationsActive)}
        ${metric("En trámite", dash.dossiersInProgress)}
        ${metric("Req. críticos pend.", dash.pendingCriticalRequirements)}
        ${metric("Detenidos >14d", dash.dossiersStuckOver14Days)}
        ${metric("Por vencer", dash.registrationsExpiring)}
      </div>
      <div class="ra-card mt">
        <h3>Pipeline / cuello de botella</h3>
        <p><strong>${esc(statusLabel(dash.bottleneckStatus) || "—")}</strong> · ${esc(dash.bottleneckCount ?? 0)}</p>
        <pre class="ra-pre">${esc(JSON.stringify(dash.dossiersByStatus || {}, null, 2))}</pre>
      </div>`;
    body.querySelectorAll("[data-open]").forEach(btn => btn.addEventListener("click", () => {
      window.__raOpenDossier = btn.dataset.open;
      document.querySelector('#ra-nav [data-view="dossiers"]')?.click();
    }));
  }

  function metric(label, value) {
    return `<div class="ra-metric"><strong>${esc(value)}</strong><span>${esc(label)}</span></div>`;
  }

  async function showPortfolio(body) {
    const [products, regs] = await Promise.all([api("/products"), api("/registrations")]);
    const regByProduct = Object.fromEntries((regs || []).filter(r => r.isCurrent).map(r => [r.productId, r]));
    const canCreate = can(P.productManage) && can(P.dossierCreate);
    body.innerHTML = `
      <div class="ra-card">
        <div class="ra-actions">
          ${canCreate ? `<button class="btn primary" id="ra-new-product" type="button">Nuevo producto + expediente</button>` : ""}
        </div>
        <table class="ra-table"><thead><tr>
          <th>Producto</th><th>Marca</th><th>Catálogo</th><th>Clase</th><th>CT/RS</th><th>Vence</th><th>Comercializable</th>
        </tr></thead><tbody>
        ${(products || []).map(p => {
          const r = regByProduct[p.id];
          return `<tr><td>${esc(p.regulatoryName)}</td><td>${esc(p.brand)}</td><td>${esc(p.catalogCode)}</td><td>${esc(p.riskClass)}</td>
            <td>${esc(r?.registrationNumber || "—")}</td><td>${esc(r?.expiresOn ? String(r.expiresOn).slice(0,10) : "—")}</td>
            <td>${p.isCommercializable ? "Sí" : "No"}</td></tr>`;
        }).join("") || `<tr><td colspan="7">Sin productos</td></tr>`}
        </tbody></table>
      </div>`;
    body.querySelector("#ra-new-product")?.addEventListener("click", async () => {
      try {
        // POST /bootstrap requires REGULATORY.CONFIGURE; specialists don't have it.
        // Authorities/pack are provisioned once by an admin, so only re-ensure when allowed.
        if (can(P.configure)) {
          await ensureBootstrap();
        }
        const authorities = await api("/authorities");
        if (!Array.isArray(authorities) || authorities.length === 0) {
          toast(tr("Regulatory.BootstrapRequired", "No hay autoridades configuradas. Pida a un administrador ejecutar el bootstrap regulatorio."), "error");
          return;
        }
        openNewProductModal(authorities, () => showPortfolio(body));
      } catch (e) { toast(e.message, "error"); }
    });
  }

  function tr(key, fallback) {
    if (typeof window.t !== "function") return fallback;
    const value = window.t(key);
    return value && value !== key ? value : fallback;
  }

  // Modal enterprise genérico. fields: [{name,label,type,required,value,placeholder,maxlength,rows,options}]
  function openFormModal({ id, title, subtitle, fields, submitLabel, onSubmit }) {
    document.querySelector(`#${id}`)?.remove();
    const overlay = document.createElement("div");
    overlay.id = id;
    overlay.className = "ra-modal-overlay";
    const fieldHtml = fields.map((f, i) => {
      const fid = `${id}-f${i}`;
      const req = f.required ? " *" : "";
      let control;
      if (f.type === "textarea") {
        control = `<textarea id="${fid}" name="${esc(f.name)}" rows="${f.rows || 3}" minlength="${f.minlength || 0}" maxlength="${f.maxlength || 4000}" ${f.required ? "required" : ""} ${f.readonly ? "readonly" : ""} placeholder="${esc(f.placeholder || "")}">${esc(f.value || "")}</textarea>`;
      } else if (f.type === "user-picker") {
        const options = f.options || [];
        const emptyLabel = f.emptyLabel || tr("Regulatory.Workflow.NoOwner", "— Sin responsable —");
        const selected = String(f.value || "");
        control = `
          ${f.readonly ? `<input type="hidden" name="${esc(f.name)}" value="${esc(selected)}">` : ""}
          <input type="search" id="${fid}-filter" class="ra-user-filter" placeholder="${esc(f.placeholder || tr("Regulatory.Workflow.SearchOwner", "Buscar por nombre o correo..."))}" ${f.readonly ? "disabled" : ""} autocomplete="off">
          <select id="${fid}" ${f.readonly ? "disabled" : `name="${esc(f.name)}"`} ${f.required ? "required" : ""} class="ra-user-select" size="${Math.min(8, Math.max(5, Math.min(options.length + 1, 10)))}">
            <option value="" ${!selected ? "selected" : ""}>${esc(emptyLabel)}</option>
            ${options.map(o => `<option value="${esc(o.value)}" ${String(o.value) === selected ? "selected" : ""} data-search="${esc(`${o.label} ${o.email || ""}`.toLowerCase())}">${esc(o.label)}</option>`).join("")}
          </select>`;
      } else if (f.type === "select") {
        control = `<select id="${fid}" name="${esc(f.name)}" ${f.required ? "required" : ""} ${f.readonly ? "disabled" : ""}>${(f.options || []).map(o => `<option value="${esc(o.value)}" ${String(o.value) === String(f.value || "") ? "selected" : ""}>${esc(o.label)}</option>`).join("")}</select>${f.readonly ? `<input type="hidden" name="${esc(f.name)}" value="${esc(f.value || "")}">` : ""}`;
      } else if (f.type === "file") {
        control = `<input id="${fid}" name="${esc(f.name)}" type="file" ${f.accept ? `accept="${esc(f.accept)}"` : ""} ${f.required ? "required" : ""}>`;
      } else {
        control = `<input id="${fid}" name="${esc(f.name)}" type="${f.type || "text"}" minlength="${f.minlength || 0}" maxlength="${f.maxlength || 200}" value="${esc(f.value || "")}" ${f.required ? "required" : ""} ${f.readonly ? "readonly" : ""} placeholder="${esc(f.placeholder || "")}">`;
      }
      return `<div class="field">${f.type === "user-picker" ? `<label for="${fid}-filter">${esc(f.label)}${req}</label>` : `<label for="${fid}">${esc(f.label)}${req}</label>`}${control}</div>`;
    }).join("");
    overlay.innerHTML = `
      <form class="ra-modal form-stack" autocomplete="off" novalidate>
        <h3>${esc(title)}</h3>
        ${subtitle ? `<p class="ra-modal-subtitle">${esc(subtitle)}</p>` : ""}
        ${fieldHtml}
        <div class="button-row">
          <button class="btn subtle" type="button" data-modal-cancel>${esc(tr("Common.Cancel", "Cancelar"))}</button>
          <button class="btn primary" type="submit">${esc(submitLabel || tr("Common.Save", "Guardar"))}</button>
        </div>
      </form>`;
    document.body.appendChild(overlay);
    const form = overlay.querySelector("form");
    const submitBtn = form.querySelector("button[type=submit]");
    form.querySelectorAll(".ra-user-filter").forEach(filter => {
      const select = filter.parentElement?.querySelector("select.ra-user-select");
      if (!select) return;
      filter.addEventListener("input", () => {
        const term = filter.value.trim().toLowerCase();
        Array.from(select.options).forEach((option, index) => {
          if (index === 0) {
            option.hidden = false;
            return;
          }
          const haystack = option.dataset.search || option.textContent.toLowerCase();
          option.hidden = term.length > 0 && !haystack.includes(term);
        });
      });
    });
    const close = () => {
      document.removeEventListener("keydown", onKeyDown);
      overlay.remove();
    };
    const onKeyDown = (e) => { if (e.key === "Escape") close(); };
    document.addEventListener("keydown", onKeyDown);
    overlay.addEventListener("mousedown", (e) => { if (e.target === overlay) close(); });
    overlay.querySelector("[data-modal-cancel]").addEventListener("click", close);
    form.querySelector("input, textarea, select")?.focus();
    form.addEventListener("submit", async (e) => {
      e.preventDefault();
      if (!form.reportValidity()) return;
      const data = Object.fromEntries(new FormData(form).entries());
      submitBtn.disabled = true;
      const originalLabel = submitBtn.textContent;
      submitBtn.textContent = tr("Dashboard.Saving", "Guardando...");
      try {
        await onSubmit(data);
        close();
      } catch (err) {
        toast(err.message, "error");
        submitBtn.disabled = false;
        submitBtn.textContent = originalLabel;
      }
    });
  }

  function asDateInput(value) {
    return value ? String(value).slice(0, 10) : "";
  }

  function asIsoDate(value) {
    return value ? new Date(`${value}T12:00:00Z`).toISOString() : null;
  }

  function formatDateTime(value) {
    if (!value) return "—";
    try { return new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" }).format(new Date(value)); }
    catch { return String(value); }
  }

  function isRevisionConflict(error) {
    return [409, 412].includes(error?.status) || /revision conflict|concurren|stale|expected .*current/i.test(error?.message || "");
  }

  async function handleWorkflowError(error, reload) {
    if (!isRevisionConflict(error)) {
      toast(error.message, "error");
      return;
    }
    toast(tr("Regulatory.Workflow.ConflictReload", "El expediente cambió mientras trabajaba. Se recargó la versión actual; revise sus cambios e intente de nuevo."), "error");
    await reload();
  }

  async function sha256File(file) {
    if (!window.crypto?.subtle) {
      throw new Error(tr("Regulatory.Workflow.ShaUnavailable", "Este navegador no permite calcular SHA-256 de forma segura."));
    }
    const digest = await window.crypto.subtle.digest("SHA-256", await file.arrayBuffer());
    return Array.from(new Uint8Array(digest), byte => byte.toString(16).padStart(2, "0")).join("");
  }

  function disabledAction(label, reason) {
    return `<span class="ra-disabled-action"><button type="button" class="btn" disabled aria-describedby="ra-action-help">${esc(label)}</button><small id="ra-action-help">${esc(reason)}</small></span>`;
  }

  function openCorrectionModal({ dossierId, revision, requirements, onSaved }) {
    document.querySelector("#ra-correction-modal")?.remove();
    const overlay = document.createElement("div");
    overlay.id = "ra-correction-modal";
    overlay.className = "ra-modal-overlay";
    overlay.innerHTML = `
      <form class="ra-modal form-stack" autocomplete="off" novalidate>
        <h3>${esc(tr("Regulatory.Workflow.ReturnForCorrection", "Devolver para corrección"))}</h3>
        <p class="ra-modal-subtitle">${esc(tr("Regulatory.Workflow.CorrectionScopeHelp", "Defina un motivo, severidad y uno o más requisitos. El Specialist solo podrá modificar este alcance."))}</p>
        <div class="field">
          <label for="ra-correction-reason">${esc(tr("Regulatory.Workflow.Reason", "Motivo"))} *</label>
          <textarea id="ra-correction-reason" name="reason" rows="4" minlength="8" maxlength="2000" required></textarea>
          <small>${esc(tr("Regulatory.Workflow.ReasonMin", "Mínimo 8 caracteres."))}</small>
        </div>
        <div class="field">
          <label for="ra-correction-severity">${esc(tr("Regulatory.Workflow.Severity", "Severidad"))} *</label>
          <select id="ra-correction-severity" name="severity" required>
            ${["Low", "Medium", "High", "Critical"].map(x => `<option value="${x}">${esc(tr(`Regulatory.Workflow.Severity.${x}`, x))}</option>`).join("")}
          </select>
        </div>
        <fieldset class="ra-scope-picker">
          <legend>${esc(tr("Regulatory.Workflow.RequirementsScope", "Requisitos incluidos"))} *</legend>
          ${(requirements || []).map(r => `<label><input type="checkbox" name="requirementIds" value="${esc(r.id)}"> <strong>${esc(r.code)}</strong> ${esc(r.name)}</label>`).join("")}
        </fieldset>
        <p class="ra-inline-error" data-scope-error hidden role="alert">${esc(tr("Regulatory.Workflow.SelectRequirement", "Seleccione al menos un requisito."))}</p>
        <div class="button-row">
          <button class="btn subtle" type="button" data-modal-cancel>${esc(tr("Common.Cancel", "Cancelar"))}</button>
          <button class="btn primary" type="submit">${esc(tr("Regulatory.Workflow.CreateCorrection", "Solicitar corrección"))}</button>
        </div>
      </form>`;
    document.body.appendChild(overlay);
    const form = overlay.querySelector("form");
    const submit = form.querySelector("button[type=submit]");
    const close = () => overlay.remove();
    overlay.querySelector("[data-modal-cancel]").addEventListener("click", close);
    overlay.addEventListener("mousedown", event => { if (event.target === overlay) close(); });
    form.querySelector("textarea").focus();
    form.addEventListener("submit", async event => {
      event.preventDefault();
      const requirementIds = [...form.querySelectorAll('input[name="requirementIds"]:checked')].map(x => x.value);
      const scopeError = form.querySelector("[data-scope-error]");
      scopeError.hidden = requirementIds.length > 0;
      if (!form.reportValidity() || requirementIds.length === 0) return;
      submit.disabled = true;
      try {
        await apiV2(`/${dossierId}/corrections`, {
          method: "POST",
          body: {
            expectedRevision: revision,
            reason: form.reason.value.trim(),
            severity: form.severity.value,
            requirementIds,
            fieldPaths: [],
            documentIds: []
          }
        });
        close();
        toast(tr("Regulatory.Workflow.CorrectionCreated", "Corrección solicitada."), "success");
        await onSaved();
      } catch (error) {
        submit.disabled = false;
        await handleWorkflowError(error, onSaved);
      }
    });
  }

  function openNewProductModal(authorities, onCreated) {
    document.querySelector("#ra-new-product-modal")?.remove();
    const overlay = document.createElement("div");
    overlay.id = "ra-new-product-modal";
    overlay.className = "ra-modal-overlay";
    const suggestedCode = `CAT-${Date.now().toString().slice(-6)}`;
    overlay.innerHTML = `
      <form class="ra-modal form-stack" autocomplete="off" novalidate>
        <h3>${esc(tr("Ui.NuevoProductoExpediente", "Nuevo producto + expediente"))}</h3>
        <p class="ra-modal-subtitle">${esc(tr("Regulatory.NewProductSubtitle", "Se creará el producto y su expediente regulatorio en estado Borrador."))}</p>
        <div class="ra-modal-grid">
          <div class="field">
            <label for="ra-np-brand">${esc(tr("Ui.Marca", "Marca"))} *</label>
            <input id="ra-np-brand" name="brand" type="text" maxlength="120" required>
          </div>
          <div class="field">
            <label for="ra-np-code">${esc(tr("Regulatory.CodigoCatalogo", "Código catálogo"))} *</label>
            <input id="ra-np-code" name="catalogCode" type="text" maxlength="60" value="${esc(suggestedCode)}" required>
          </div>
        </div>
        <div class="field">
          <label for="ra-np-name">${esc(tr("Regulatory.NombreRegulatorio", "Nombre regulatorio"))} *</label>
          <input id="ra-np-name" name="regulatoryName" type="text" maxlength="200" required>
        </div>
        <div class="ra-modal-grid">
          <div class="field">
            <label for="ra-np-risk">${esc(tr("Regulatory.RiskClass", "Clase de riesgo"))}</label>
            <select id="ra-np-risk" name="riskClass">
              ${["A", "B", "C"].map(c => `<option value="${c}">${c}</option>`).join("")}
            </select>
          </div>
          <div class="field">
            <label for="ra-np-country">${esc(tr("Regulatory.Pais", "País"))} *</label>
            <input id="ra-np-country" name="countryCode" type="text" maxlength="2" value="PA" required>
          </div>
        </div>
        <div class="field">
          <label for="ra-np-authority">${esc(tr("Regulatory.Authority", "Autoridad"))} *</label>
          <select id="ra-np-authority" name="authorityId" required>
            ${authorities.map(a => `<option value="${esc(a.id)}">${esc(a.name || a.code || a.id)}</option>`).join("")}
          </select>
        </div>
        <div class="field">
          <label for="ra-np-comments">${esc(tr("Regulatory.Comments", "Comentarios"))}</label>
          <textarea id="ra-np-comments" name="comments" rows="2" maxlength="500" placeholder="${esc(tr("Common.Optional", "Opcional"))}"></textarea>
        </div>
        <div class="button-row">
          <button class="btn subtle" type="button" data-modal-cancel>${esc(tr("Common.Cancel", "Cancelar"))}</button>
          <button class="btn primary" type="submit">${esc(tr("Common.Create", "Crear"))}</button>
        </div>
      </form>`;
    document.body.appendChild(overlay);

    const form = overlay.querySelector("form");
    const submitBtn = form.querySelector("button[type=submit]");
    const close = () => {
      document.removeEventListener("keydown", onKeyDown);
      overlay.remove();
    };
    const onKeyDown = (e) => { if (e.key === "Escape") close(); };
    document.addEventListener("keydown", onKeyDown);
    overlay.addEventListener("mousedown", (e) => { if (e.target === overlay) close(); });
    overlay.querySelector("[data-modal-cancel]").addEventListener("click", close);
    form.querySelector("#ra-np-brand").focus();

    form.addEventListener("submit", async (e) => {
      e.preventDefault();
      if (!form.reportValidity()) return;
      const data = Object.fromEntries(new FormData(form).entries());
      submitBtn.disabled = true;
      const originalLabel = submitBtn.textContent;
      submitBtn.textContent = tr("Dashboard.Saving", "Guardando...");
      try {
        const product = await api("/products", {
          method: "POST",
          body: {
            countryCode: (data.countryCode || "PA").trim().toUpperCase(), category: "Insumos Médicos",
            brand: data.brand.trim(), regulatoryName: data.regulatoryName.trim(), commercialName: data.regulatoryName.trim(),
            description: null, catalogCode: data.catalogCode.trim(), internalCode: null, productType: null,
            riskClass: data.riskClass || "A", manufacturerId: null, distributorCompanyId: null,
            initiative: "NEGOCIO BASE", priority: null, salesMarketingInput: null, opportunityAmount: 0, currency: "USD"
          }
        });
        await api("/dossiers", {
          method: "POST",
          body: {
            productId: product.id, authorityId: data.authorityId, processType: "NewRegistration",
            existingRegistrationId: null, priority: null, ownerUserId: null, salesMarketingInput: null,
            opportunityAmount: 0, currency: "USD",
            comments: (data.comments || "").trim() || "Creado desde Portafolio", requirementPackId: null,
            saveAsDraft: true
          }
        });
        close();
        toast(tr("Regulatory.ProductCreated", "Producto y expediente creados"), "success");
        await onCreated();
      } catch (err) {
        toast(err.message, "error");
        submitBtn.disabled = false;
        submitBtn.textContent = originalLabel;
      }
    });
  }

  async function showPipeline(body) {
    const [dossiers, registrations] = await Promise.all([api("/dossiers"), api("/registrations")]);
    const now = Date.now();
    const expiredRegs = new Set((registrations || [])
      .filter(r => r.expiresOn && new Date(r.expiresOn).getTime() < now)
      .map(r => r.productId));
    body.innerHTML = `<div class="ra-kanban">${PIPELINE_COLUMNS.map(col => {
      let items;
      if (col === "Vencido") {
        items = (dossiers || []).filter(d => d.status === "Approved" && expiredRegs.has(d.productId));
      } else if (col === "Renovacion") {
        items = (dossiers || []).filter(d => /renew/i.test(String(d.processType || "")));
      } else {
        items = (dossiers || []).filter(d => d.status === col);
      }
      return `<div class="ra-col"><h4>${esc(statusLabel(col) || col)} (${items.length})</h4>${items.map(d =>
        `<button type="button" class="ra-chip" data-id="${d.id}"><strong>${esc(d.caseNumber)}</strong><div>${esc(statusLabel(d.status))}</div></button>`
      ).join("") || "<div class='ra-empty'>Vacío</div>"}</div>`;
    }).join("")}</div>`;
    body.querySelectorAll("[data-id]").forEach(el => el.addEventListener("click", () => {
      window.__raOpenDossier = el.dataset.id;
      document.querySelector('#ra-nav [data-view="dossiers"]')?.click();
    }));
  }

  async function showDossiers(body, forcedId) {
    const openId = forcedId || window.__raOpenDossier;
    window.__raOpenDossier = null;
    if (openId) {
      await showDossierDetail(body, openId);
      return;
    }
    const dossiers = await api("/dossiers");
    body.innerHTML = `<div class="ra-card"><table class="ra-table"><thead><tr>
      <th>Caso</th><th>Estado (humano)</th><th>Proceso</th><th>Sometido</th><th>Aprob. autoridad</th>
    </tr></thead><tbody>
    ${(dossiers || []).map(d => `<tr data-id="${d.id}"><td>${esc(d.caseNumber)}</td><td>${esc(statusLabel(d.status))}</td><td>${esc(d.processType)}</td>
      <td>${esc(d.submittedOn ? String(d.submittedOn).slice(0,10) : "—")}</td>
      <td>${esc(d.approvedOn ? String(d.approvedOn).slice(0,10) : "—")}</td></tr>`).join("")}
    </tbody></table></div>`;
    body.querySelectorAll("tr[data-id]").forEach(tr => tr.addEventListener("click", () => showDossierDetail(body, tr.dataset.id)));
  }

  async function downloadDossierEvidence(dossierId, storedFileId, fileName) {
    const token = localStorage.getItem("c360.token");
    const tenantId = resolveTenantId();
    const res = await fetch(
      `/api/v1/tenants/${tenantId}/regulatory/dossiers/${dossierId}/evidence/${storedFileId}/content`,
      { headers: { Authorization: `Bearer ${token}` } }
    );
    if (!res.ok) {
      const text = await res.text();
      let message = tr("Regulatory.EvidenceDownloadFailed", "No se pudo descargar la evidencia.");
      try {
        const payload = JSON.parse(text);
        message = payload.detail || payload.title || payload.error || message;
      } catch {
        if (text && text.length < 200) message = text;
      }
      throw new Error(message);
    }
    const blob = await res.blob();
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = fileName || `evidence-${storedFileId}.bin`;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
  }

  function evidenceStatusHtml(requirement, canDelete = false) {
    if (!requirement?.storedFileId) {
      return `<p class="ra-evidence-status is-missing"><span class="ra-badge warn">${esc(tr("Regulatory.EvidenceMissing", "Sin evidencia"))}</span> ${esc(tr("Regulatory.EvidenceMissingHelp", "Aún no hay archivo cargado para este requisito."))}</p>`;
    }
    return `<p class="ra-evidence-status is-loaded">
      <span class="ra-badge ok">${esc(tr("Regulatory.EvidenceLoaded", "Evidencia cargada"))}</span>
      <button type="button" class="btn primary" data-download-evidence="${esc(requirement.storedFileId)}" data-download-name="${esc(requirement.code || "evidence")}">${esc(tr("Regulatory.DownloadEvidence", "Descargar evidencia"))}</button>
      ${canDelete ? `<button type="button" class="btn danger" data-delete-evidence="${esc(requirement.id)}" data-delete-code="${esc(requirement.code || "")}">${esc(tr("Regulatory.DeleteEvidence", "Eliminar evidencia"))}</button>` : ""}
    </p>`;
  }

  function evidenceVersionsHtml(requirement, versions) {
    const rows = [...(versions || [])];
    if (requirement.storedFileId && !rows.some(v => v.storedFileId === requirement.storedFileId)) {
      rows.unshift({
        versionNumber: 1,
        storedFileId: requirement.storedFileId,
        fileName: tr("Regulatory.Workflow.LegacyEvidence", "Evidencia V1"),
        reason: tr("Regulatory.Workflow.LegacyEvidenceHelp", "Archivo registrado por el flujo V1."),
        sha256: null,
        isCurrent: rows.length === 0,
        status: "Active",
        source: "V1"
      });
    }
    if (rows.length === 0) return `<p class="ra-empty">${esc(tr("Regulatory.Workflow.NoEvidenceVersions", "Sin versiones de evidencia."))}</p>`;
    return `<ol class="ra-version-list">${rows.sort((a, b) => Number(b.versionNumber) - Number(a.versionNumber)).map(v => `
      <li class="${v.isCurrent ? "is-current" : ""}">
        <div><strong>V${esc(v.versionNumber)}</strong> <span class="ra-badge">${esc(v.source || "V2")}</span>
          ${v.isCurrent ? `<span class="ra-current">${esc(tr("Regulatory.Workflow.CurrentVersion", "Actual"))}</span>` : ""}
        </div>
        <span>${esc(v.fileName || "—")} · ${esc(v.status || "—")}</span>
        <small>${esc(v.reason || "—")}${v.uploadedAtUtc ? ` · ${esc(formatDateTime(v.uploadedAtUtc))}` : ""}</small>
        ${v.sha256 ? `<code title="SHA-256">${esc(v.sha256)}</code>` : ""}
        ${v.storedFileId ? `<button type="button" class="btn" data-download-evidence="${esc(v.storedFileId)}" data-download-name="${esc(v.fileName || requirement.code || "evidence")}">${esc(tr("Regulatory.DownloadEvidence", "Descargar evidencia"))}</button>` : ""}
      </li>`).join("")}</ol>`;
  }

  function workflowTimelineHtml(timeline, legacyHistory) {
    return `
      <div class="ra-timeline-groups">
        <section>
          <h5>${esc(tr("Regulatory.Workflow.TimelineV2", "Eventos Workflow V2"))}</h5>
          <ol class="ra-timeline">${(timeline || []).map(event => `
            <li>
              <time>${esc(formatDateTime(event.occurredAtUtc))}</time>
              <strong>#${esc(event.sequence)} · ${esc(event.eventType)}</strong>
              <span>${event.fromStatus || event.toStatus ? `${esc(statusLabel(event.fromStatus || "—"))} → ${esc(statusLabel(event.toStatus || "—"))}` : esc(event.field || "")}</span>
              <p>${esc(event.reason || "")}</p>
              <small>${esc(tr("Regulatory.Workflow.Actor", "Actor"))}: ${esc(event.actorRole || event.actorUserId || "—")} · CorrelationId ${esc(event.correlationId || "—")}</small>
            </li>`).join("") || `<li>${esc(tr("Regulatory.Workflow.NoV2Events", "Sin eventos V2."))}</li>`}
          </ol>
        </section>
        <section>
          <h5>${esc(tr("Regulatory.Workflow.TimelineV1", "Historial operativo V1"))}</h5>
          <ol class="ra-timeline legacy">${(legacyHistory || []).map(event => `
            <li><time>${esc(formatDateTime(event.occurredAtUtc))}</time><strong>${esc(event.eventType)}</strong><p>${esc(event.summary)}</p></li>`
          ).join("") || `<li>${esc(tr("Regulatory.Workflow.NoV1Events", "Sin eventos V1."))}</li>`}</ol>
        </section>
      </div>`;
  }

  async function showDossierDetail(body, id) {
    const d = await api(`/dossiers/${id}`);
    const [workflowResult, timelineResult] = await Promise.all([
      apiV2(`/${id}/workflow`).then(value => ({ value })).catch(error => ({ error })),
      apiV2(`/${id}/timeline`).then(value => ({ value })).catch(error => ({ error }))
    ]);
    const workflow = workflowResult.value || null;
    const timeline = timelineResult.value || [];
    const availableActions = new Set(workflow?.availableActions || []);
    const correction = workflow?.openCorrection || null;
    const correctionRequirementIds = new Set(correction?.requirementIds || []);
    // Preparation requires both product ownership and dossier update. This
    // prevents RA-MGR (operational supervision only) from acting as RA-SPEC.
    const prep = can(P.dossierUpdate) && can(P.productManage);
    const review = can(P.dossierReview);
    const apprInt = can(P.approveInternal);
    const sub = can(P.submit);
    const ext = can(P.approveExternal);
    const obs = can(P.obsManage);

    const actions = [];
    if (prep && d.status === "Draft") {
      actions.push(`<button type="button" class="btn primary" data-next="Planning">${esc(tr("Regulatory.Workflow.StartPlanning", "Iniciar planificación"))}</button>`);
    }
    if (prep && d.status === "Planning") {
      actions.push(`<button type="button" class="btn" data-next="WaitingManufacturerDocuments">${esc(tr("Regulatory.Workflow.RequestFactoryDocs", "Pedir documentos a fábrica"))}</button>`);
    }
    if (prep && d.status === "WaitingManufacturerDocuments") {
      actions.push(`<button type="button" class="btn" data-next="DocumentsReceived">${esc(tr("Regulatory.Workflow.DocumentsReceived", "Documentos recibidos"))}</button>`);
    }
    if (prep && d.status === "DocumentsReceived") {
      actions.push(`<button type="button" class="btn" data-next="Assembling">${esc(tr("Regulatory.Workflow.StartAssembly", "Iniciar armado"))}</button>`);
    }
    if (d.status === "Assembling") {
      if (prep && availableActions.has("technical-review")) {
        actions.push(`<button type="button" class="btn primary" id="ra-send-technical-review">${esc(tr("Regulatory.Workflow.SendTechnicalReview", "Enviar a revisión técnica"))}</button>`);
      } else {
        actions.push(disabledAction(
          tr("Regulatory.Workflow.SendTechnicalReview", "Enviar a revisión técnica"),
          prep ? tr("Regulatory.Workflow.RefreshActions", "La acción ya no está disponible; recargue el expediente.") : tr("Regulatory.Workflow.SpecialistPermissionRequired", "Requiere permiso de Specialist para actualizar el expediente.")
        ));
      }
    }
    if (review && d.status === "UnderTechnicalReview" && availableActions.has("return-for-correction")) {
      actions.push(`<button type="button" class="btn warning" id="ra-return-correction">${esc(tr("Regulatory.Workflow.ReturnForCorrection", "Devolver para corrección"))}</button>`);
    }
    if (review && d.status === "UnderTechnicalReview" && availableActions.has("ready-for-submission")) {
      actions.push(`<button type="button" class="btn primary" id="ra-complete-technical-review">${esc(tr("Regulatory.Workflow.CompleteTechnicalReview", "Completar revisión técnica"))}</button>`);
    }
    const hasControlledMetadataScope = d.status === "CorrectionRequested" && (correction?.fieldPaths || []).length > 0;
    if (prep && (["Draft", "Planning", "Assembling"].includes(d.status) || hasControlledMetadataScope)) {
      actions.push(`<button type="button" class="btn" id="ra-edit-metadata">${esc(tr("Regulatory.Workflow.EditMetadata", "Editar metadata"))}</button>`);
    }
    if (prep && availableActions.has("request-reopen")) {
      actions.push(`<button type="button" class="btn" id="ra-request-reopen">${esc(tr("Regulatory.Workflow.RequestReopen", "Solicitar reapertura"))}</button>`);
    }
    if (prep && availableActions.has("cancel")) {
      actions.push(`<button type="button" class="btn danger" id="ra-cancel">${esc(tr("Regulatory.Workflow.Cancel", "Cancelar expediente"))}</button>`);
    }
    if (ext && availableActions.has("archive")) {
      actions.push(`<button type="button" class="btn danger" id="ra-archive">${esc(tr("Regulatory.Workflow.Archive", "Archivar expediente"))}</button>`);
    }
    if (can(P.sodOverride) && d.status !== "Archived") {
      actions.push(`<button type="button" class="btn warning" id="ra-request-override">${esc(tr("Regulatory.Workflow.RequestOverride", "Solicitar override SoD"))}</button>`);
    }
    if (apprInt && d.status === "ReadyForSubmission") {
      actions.push(`<button type="button" class="btn primary" id="ra-approve-internal">Aprobar internamente para sometimiento</button>`);
    }
    if (sub && d.status === "ApprovedForSubmission") {
      actions.push(`<button type="button" class="btn primary" id="ra-submit">Registrar sometimiento</button>`);
    }
    if (sub && ["CorrectingObservation", "ResponseReady"].includes(d.status)) {
      actions.push(`<button type="button" class="btn primary" id="ra-resubmit">${esc(tr("Regulatory.RegisterResubmission", "Registrar resometimiento"))}</button>`);
    }
    if (obs && ["Submitted", "Resubmitted"].includes(d.status)) {
      actions.push(`<button type="button" class="btn" id="ra-start-authority-review">${esc(tr("Regulatory.StartAuthorityReview", "Iniciar seguimiento de autoridad"))}</button>`);
    }
    // Manual (buttons.json): registrar observación = Regulatory Manager (OBSERVATION.MANAGE + decisión externa).
    if (obs && ext && ["Submitted", "UnderAuthorityReview", "Resubmitted", "Observed"].includes(d.status)) {
      actions.push(`<button type="button" class="btn" id="ra-observe">Registrar observación autoridad</button>`);
    }
    if (ext && ["UnderAuthorityReview", "Resubmitted", "Submitted"].includes(d.status)) {
      actions.push(`<button type="button" class="btn" id="ra-approve-ext">Registrar aprobación MINSA/CSS + CT/RS</button>`);
    }
    if (ext && ["UnderAuthorityReview", "Resubmitted", "Submitted"].includes(d.status)) {
      actions.push(`<button type="button" class="btn danger" id="ra-reject-ext">${esc(tr("Regulatory.RegisterExternalRejection", "Registrar rechazo de autoridad"))}</button>`);
    }

    body.innerHTML = `
      <div class="ra-card">
        <button type="button" id="ra-back" class="btn">← Expedientes</button>
        <h3>${esc(d.caseNumber)} <span class="ra-badge">${esc(statusLabel(d.status))}</span></h3>
        <p>${esc(d.processType)} · Pack ${esc(d.requirementPackVersionLabel || "—")}</p>
        ${flowStrip(d.status)}
        <section class="ra-workflow-card ${workflow ? "" : "is-unavailable"}" aria-live="polite">
          <div>
            <h4>${esc(tr("Regulatory.Workflow.CardTitle", "Workflow V2"))}</h4>
            <strong>${esc(tr("Regulatory.Workflow.Revision", "Revisión"))}: ${esc(workflow?.revision ?? "—")}</strong>
          </div>
          ${workflow ? `
            <div><span>${esc(tr("Regulatory.Workflow.ServerActions", "Acciones habilitadas"))}</span>
              <div class="ra-token-list">${(workflow.availableActions || []).map(x => `<code>${esc(x)}</code>`).join("") || "—"}</div>
            </div>
            <div><span>${esc(tr("Regulatory.Workflow.Locks", "Bloqueos"))}</span>
              <div class="ra-token-list">${(workflow.locks || []).map(x => `<code>${esc(x)}</code>`).join("") || esc(tr("Regulatory.Workflow.NoLocks", "Sin bloqueos V2"))}</div>
            </div>` :
            `<p>${esc(tr("Regulatory.Workflow.Unavailable", "Workflow V2 no está disponible. La lectura V1 continúa activa, pero las acciones V2 están bloqueadas."))}</p>`}
        </section>
        ${correction ? `<aside class="ra-correction-scope">
          <h4>${esc(tr("Regulatory.Workflow.ActiveCorrection", "Corrección activa"))} · ${esc(tr(`Regulatory.Workflow.Severity.${correction.severity}`, correction.severity))}</h4>
          <p>${esc(correction.reason)}</p>
          <strong>${esc(tr("Regulatory.Workflow.LockedScope", "Alcance bloqueado"))}</strong>
          <ul>${(d.requirements || []).filter(r => correctionRequirementIds.has(r.id)).map(r => `<li>${esc(r.code)} · ${esc(r.name)}</li>`).join("")}</ul>
          <small>${esc(tr("Regulatory.Workflow.ScopeOnlyHelp", "Solo los requisitos listados admiten nuevas versiones hasta enviar la corrección."))}</small>
        </aside>` : ""}
        <div class="ra-actions">${actions.join("") || `<em>${esc(tr("Regulatory.Workflow.NoActions", "Sin acciones disponibles para su rol en este estado."))}</em>`}</div>
        <h4>${esc(tr("Regulatory.Workflow.Requirements", "Requisitos"))} (${d.requirements?.length || 0})</h4>
        <div id="ra-reqs">${(d.requirements || []).map(r => {
          const canDeleteEvidence = prep
            && !!r.storedFileId
            && !["Accepted", "Waived"].includes(r.status)
            && (
              (d.status !== "CorrectionRequested" && !["UnderTechnicalReview", "ReadyForSubmission"].includes(d.status))
              || (d.status === "CorrectionRequested" && correctionRequirementIds.has(r.id))
            );
          return `
          <article class="ra-req ${r.isCritical ? "critical" : ""} ${r.status === "Accepted" ? "ok" : ""} ${r.storedFileId ? "has-evidence" : "no-evidence"} ${d.status === "CorrectionRequested" && correctionRequirementIds.has(r.id) ? "in-scope" : ""} ${d.status === "CorrectionRequested" && !correctionRequirementIds.has(r.id) ? "is-locked" : ""}">
            <div class="ra-req-header"><div><strong>${esc(r.code)}</strong> ${esc(r.name)}</div>
            <span class="ra-badge">${esc(r.status)}${r.isCritical ? ` · ${esc(tr("Regulatory.Workflow.Critical", "crítico"))}` : ""}${r.storedFileId ? ` · ${esc(tr("Regulatory.EvidenceLoadedShort", "con archivo"))}` : ""}</span></div>
            ${evidenceStatusHtml(r, canDeleteEvidence)}
            ${d.status === "CorrectionRequested" && !correctionRequirementIds.has(r.id) ? `<p class="ra-lock-reason">🔒 ${esc(tr("Regulatory.Workflow.OutsideScope", "Bloqueado: este requisito no pertenece al alcance de la corrección activa."))}</p>` : ""}
            ${prep && !["Accepted", "Waived"].includes(r.status) && d.status !== "CorrectionRequested" && !["UnderTechnicalReview", "ReadyForSubmission"].includes(d.status) ? `
              <label class="btn file-action">
                ${esc(tr("Regulatory.SelectEvidence", "Seleccionar evidencia"))}
                <input type="file" data-req-file="${r.id}" accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg" hidden>
              </label>
              <button type="button" class="btn" data-prep="${r.id}">${r.storedFileId ? esc(tr("Regulatory.ReplaceEvidence", "Reemplazar evidencia")) : esc(tr("Regulatory.MarkReceived", "Cargar y marcar recibido"))}</button>` : ""}
            ${prep && d.status === "CorrectionRequested" && correctionRequirementIds.has(r.id) ? `<button type="button" class="btn primary" data-correction-evidence="${r.id}">${esc(tr("Regulatory.Workflow.UploadCorrectionVersion", "Cargar versión corregida"))}</button>` : ""}
            ${review && d.status === "UnderTechnicalReview" ? `<button type="button" class="btn" data-accept="${r.id}">Aceptar</button>
              <button type="button" class="btn" data-reject="${r.id}">Rechazar</button>` : ""}
            <details class="ra-evidence-versions" data-evidence-requirement="${r.id}" ${r.storedFileId ? "open" : ""}><summary>${esc(tr("Regulatory.Workflow.ViewVersions", "Consultar versiones V1/V2"))}</summary>
              <div data-evidence-list>${evidenceVersionsHtml(r, [])}</div>
            </details>
          </article>`;
        }).join("")}
        </div>
        ${d.submissionProofStoredFileId ? `<div class="ra-card ra-submission-proof">
          <h4>${esc(tr("Regulatory.SubmissionProof", "Comprobante de sometimiento"))}</h4>
          <p class="ra-evidence-status is-loaded">
            <span class="ra-badge ok">${esc(tr("Regulatory.EvidenceLoaded", "Evidencia cargada"))}</span>
            <button type="button" class="btn primary" data-download-evidence="${esc(d.submissionProofStoredFileId)}" data-download-name="submission-proof">${esc(tr("Regulatory.DownloadEvidence", "Descargar evidencia"))}</button>
          </p>
        </div>` : ""}
        ${prep && d.status === "CorrectionRequested" && correction ? `<div class="ra-actions"><button type="button" class="btn primary" id="ra-submit-correction">${esc(tr("Regulatory.Workflow.SubmitCorrection", "Enviar corrección a revisión"))}</button></div>` : ""}
        <h4>Observaciones</h4>
        <ul>${(d.observations || []).map(o => `<li>#${o.observationNumber} ${esc(o.status)} — ${esc(o.description)}
          ${obs && can(P.reqManage) && o.status !== "Closed" ? `<button type="button" class="btn" data-resp="${o.id}">Responder</button>` : ""}</li>`).join("") || "<li>Ninguna</li>"}</ul>
        <h4>${esc(tr("Regulatory.Workflow.CompleteTimeline", "Timeline completa"))}</h4>
        ${workflowTimelineHtml(timeline, d.history)}
      </div>`;

    body.querySelector("#ra-back")?.addEventListener("click", () => showDossiers(body, null));
    const reload = () => showDossierDetail(body, id);
    const bindEvidenceDownloads = (root) => {
      root.querySelectorAll("[data-download-evidence]").forEach(button => {
        if (button.dataset.bound === "1") return;
        button.dataset.bound = "1";
        button.addEventListener("click", async () => {
          try {
            button.disabled = true;
            await downloadDossierEvidence(id, button.dataset.downloadEvidence, button.dataset.downloadName || "evidence");
            toast(tr("Regulatory.EvidenceDownloadStarted", "Descarga iniciada."), "success");
          } catch (error) {
            toast(error.message, "error");
          } finally {
            button.disabled = false;
          }
        });
      });
    };
    bindEvidenceDownloads(body);
    body.querySelectorAll("[data-delete-evidence]").forEach(button => {
      button.addEventListener("click", () => {
        const requirementId = button.dataset.deleteEvidence;
        const code = button.dataset.deleteCode || "";
        openFormModal({
          id: "ra-delete-evidence-modal",
          title: tr("Regulatory.DeleteEvidence", "Eliminar evidencia"),
          subtitle: tr("Regulatory.DeleteEvidenceHelp", "La eliminación queda auditada. Indique el motivo (mínimo 8 caracteres). El archivo se marca como eliminado y el historial del expediente conserva el rastro."),
          submitLabel: tr("Regulatory.DeleteEvidenceConfirm", "Eliminar evidencia"),
          fields: [
            { name: "reason", label: tr("Regulatory.DeleteEvidenceReason", "Motivo de eliminación"), type: "textarea", required: true, minlength: 8, maxlength: 2000, value: "" }
          ],
          onSubmit: async data => {
            const reason = String(data.reason || "").trim();
            if (reason.length < 8) {
              throw new Error(tr("Regulatory.DeleteEvidenceReasonRequired", "Indique un motivo auditado de al menos 8 caracteres."));
            }
            await api(`/dossiers/${id}/requirements/${requirementId}/evidence/remove`, {
              method: "POST",
              body: { reason }
            });
            toast(tr("Regulatory.EvidenceDeleted", "Evidencia eliminada y auditada.") + (code ? ` (${code})` : ""), "success");
            await reload();
          }
        });
      });
    });
    body.querySelectorAll("details[data-evidence-requirement]").forEach(details => {
      const loadVersions = async () => {
        if (details.dataset.loaded === "true" || details.dataset.loading === "true") return;
        const requirementId = details.dataset.evidenceRequirement;
        const requirement = (d.requirements || []).find(item => item.id === requirementId);
        const list = details.querySelector("[data-evidence-list]");
        if (!requirement || !list) return;
        details.dataset.loading = "true";
        try {
          const versions = await apiV2(`/${id}/requirements/${requirementId}/evidence`);
          list.innerHTML = evidenceVersionsHtml(requirement, versions);
          bindEvidenceDownloads(list);
          details.dataset.loaded = "true";
        } catch (error) {
          list.innerHTML = `<p class="ra-empty">${esc(error.message || tr("Common.Error", "No fue posible cargar las versiones."))}</p>`;
        } finally {
          delete details.dataset.loading;
        }
      };
      details.addEventListener("toggle", () => {
        if (details.open) loadVersions();
      });
      if (details.open) loadVersions();
    });
    body.querySelector("#ra-send-technical-review")?.addEventListener("click", async () => {
      try {
        await apiV2(`/${id}/technical-review/start`, {
          method: "POST",
          body: {
            expectedRevision: workflow.revision,
            reason: tr("Regulatory.Workflow.TechnicalReviewStartReason", "Preparación completada por Specialist")
          }
        });
        toast(tr("Regulatory.Workflow.SentTechnicalReview", "Expediente enviado a revisión técnica."), "success");
        await reload();
      } catch (error) { toast(error.message, "error"); }
    });
    body.querySelector("#ra-return-correction")?.addEventListener("click", () => {
      openCorrectionModal({ dossierId: id, revision: workflow.revision, requirements: d.requirements || [], onSaved: reload });
    });
    body.querySelector("#ra-complete-technical-review")?.addEventListener("click", () => {
      openFormModal({
        id: "ra-complete-technical-review-modal",
        title: tr("Regulatory.Workflow.CompleteTechnicalReview", "Completar revisión técnica"),
        subtitle: tr("Regulatory.Workflow.CompleteTechnicalReviewHelp", "Todos los requisitos obligatorios deben estar aceptados antes de continuar."),
        submitLabel: tr("Regulatory.Workflow.CompleteTechnicalReview", "Completar revisión técnica"),
        fields: [
          { name: "reason", label: tr("Regulatory.Workflow.Reason", "Motivo"), type: "textarea", required: true, maxlength: 2000 }
        ],
        onSubmit: async data => {
          try {
            await apiV2(`/${id}/technical-review/complete`, {
              method: "POST",
              body: {
                expectedRevision: workflow.revision,
                correctionRequestId: correction?.id || null,
                reason: data.reason.trim()
              }
            });
            toast(tr("Regulatory.Workflow.TechnicalReviewCompleted", "Revisión técnica completada."), "success");
            await reload();
          } catch (error) {
            await handleWorkflowError(error, reload);
          }
        }
      });
    });
    body.querySelector("#ra-edit-metadata")?.addEventListener("click", async () => {
      const correctionFields = new Set(correction?.fieldPaths || []);
      const readonlyOutsideCorrectionScope = path =>
        hasControlledMetadataScope && !correctionFields.has(path);
      let assignees = [];
      try {
        const payload = await api("/assignees");
        assignees = Array.isArray(payload) ? payload : [];
      } catch (error) {
        toast(error.message || tr("Regulatory.Workflow.AssigneesLoadFailed", "No se pudo cargar la lista de responsables."), "error");
        return;
      }
      const currentOwnerId = d.regulatoryOwnerUserId || "";
      if (currentOwnerId && !assignees.some(user => String(user.id) === String(currentOwnerId))) {
        assignees = [
          { id: currentOwnerId, fullName: tr("Regulatory.Workflow.CurrentOwner", "Responsable actual"), email: currentOwnerId },
          ...assignees
        ];
      }
      const ownerOptions = assignees.map(user => ({
        value: user.id,
        email: user.email || "",
        label: `${user.fullName || user.email}${user.email ? ` — ${user.email}` : ""}`
      }));
      openFormModal({
        id: "ra-edit-metadata-modal",
        title: tr("Regulatory.Workflow.EditMetadata", "Editar metadata"),
        subtitle: tr("Regulatory.Workflow.MetadataHelp", "Los cambios quedan auditados contra la revisión actual del expediente."),
        submitLabel: tr("Common.Save", "Guardar"),
        fields: [
          { name: "reason", label: tr("Regulatory.Workflow.ChangeReason", "Motivo del cambio"), type: "textarea", required: true, minlength: 8, maxlength: 2000 },
          { name: "priority", label: tr("Regulatory.Workflow.Priority", "Prioridad"), value: d.priority || "", maxlength: 80, readonly: readonlyOutsideCorrectionScope("metadata.priority") },
          {
            name: "ownerUserId",
            label: tr("Regulatory.Workflow.OwnerUserId", "Responsable"),
            type: "user-picker",
            value: currentOwnerId,
            options: ownerOptions,
            placeholder: tr("Regulatory.Workflow.SearchOwner", "Buscar por nombre o correo..."),
            emptyLabel: tr("Regulatory.Workflow.NoOwner", "— Sin responsable —"),
            readonly: readonlyOutsideCorrectionScope("metadata.ownerUserId")
          },
          { name: "salesMarketingInput", label: tr("Regulatory.Workflow.SalesMarketingInput", "Información comercial/regulatoria"), type: "textarea", value: d.salesMarketingInput || "", maxlength: 2000, readonly: readonlyOutsideCorrectionScope("metadata.salesMarketingInput") },
          { name: "opportunityAmount", label: tr("Regulatory.Workflow.OpportunityAmount", "Monto de oportunidad"), type: "number", value: d.opportunityAmount ?? "", readonly: readonlyOutsideCorrectionScope("metadata.opportunityAmount") },
          { name: "currency", label: tr("Regulatory.Workflow.Currency", "Moneda"), value: d.currency || "USD", maxlength: 3, readonly: readonlyOutsideCorrectionScope("metadata.currency") },
          { name: "comments", label: tr("Regulatory.Workflow.Comments", "Comentarios"), type: "textarea", value: d.comments || "", maxlength: 2000, readonly: readonlyOutsideCorrectionScope("metadata.comments") },
          { name: "requestedFromFactoryOn", label: tr("Regulatory.Workflow.RequestedFactoryDate", "Solicitud a fábrica"), type: "date", value: asDateInput(d.requestedFromFactoryOn), readonly: readonlyOutsideCorrectionScope("metadata.requestedFromFactoryOn") },
          { name: "estimatedReceptionOn", label: tr("Regulatory.Workflow.EstimatedReception", "Recepción estimada"), type: "date", value: asDateInput(d.estimatedReceptionOn), readonly: readonlyOutsideCorrectionScope("metadata.estimatedReceptionOn") },
          { name: "maximumReceptionOn", label: tr("Regulatory.Workflow.MaximumReception", "Recepción máxima"), type: "date", value: asDateInput(d.maximumReceptionOn), readonly: readonlyOutsideCorrectionScope("metadata.maximumReceptionOn") },
          { name: "estimatedSubmissionOn", label: tr("Regulatory.Workflow.EstimatedSubmission", "Sometimiento estimado"), type: "date", value: asDateInput(d.estimatedSubmissionOn), readonly: readonlyOutsideCorrectionScope("metadata.estimatedSubmissionOn") },
          { name: "estimatedApprovalOn", label: tr("Regulatory.Workflow.EstimatedApproval", "Aprobación estimada"), type: "date", value: asDateInput(d.estimatedApprovalOn), readonly: readonlyOutsideCorrectionScope("metadata.estimatedApprovalOn") },
          { name: "targetExpirationOn", label: tr("Regulatory.Workflow.TargetExpiration", "Vencimiento objetivo"), type: "date", value: asDateInput(d.targetExpirationOn), readonly: readonlyOutsideCorrectionScope("metadata.targetExpirationOn") }
        ],
        onSubmit: async data => {
          try {
            await apiV2(`/${id}/metadata`, {
              method: "PUT",
              body: {
                expectedRevision: workflow.revision,
                reason: data.reason.trim(),
                priority: data.priority.trim() || null,
                ownerUserId: (data.ownerUserId || "").trim() || null,
                salesMarketingInput: data.salesMarketingInput.trim() || null,
                opportunityAmount: data.opportunityAmount === "" ? null : Number(data.opportunityAmount),
                currency: data.currency.trim().toUpperCase() || null,
                comments: data.comments.trim() || null,
                requestedFromFactoryOn: asIsoDate(data.requestedFromFactoryOn),
                estimatedReceptionOn: asIsoDate(data.estimatedReceptionOn),
                maximumReceptionOn: asIsoDate(data.maximumReceptionOn),
                estimatedSubmissionOn: asIsoDate(data.estimatedSubmissionOn),
                estimatedApprovalOn: asIsoDate(data.estimatedApprovalOn),
                targetExpirationOn: asIsoDate(data.targetExpirationOn),
                correctionRequestId: hasControlledMetadataScope ? correction.id : null
              }
            });
            toast(tr("Regulatory.Workflow.MetadataSaved", "Metadata actualizada."), "success");
            await reload();
          } catch (error) {
            if (isRevisionConflict(error)) await handleWorkflowError(error, reload);
            else throw error;
          }
        }
      });
    });
    body.querySelectorAll("[data-correction-evidence]").forEach(button => button.addEventListener("click", () => {
      const requirementId = button.dataset.correctionEvidence;
      openFormModal({
        id: "ra-correction-evidence-modal",
        title: tr("Regulatory.Workflow.UploadCorrectionVersion", "Cargar versión corregida"),
        subtitle: tr("Regulatory.Workflow.HashHelp", "El SHA-256 se calcula en este navegador antes de registrar la revisión V2."),
        submitLabel: tr("Regulatory.Workflow.UploadVersion", "Cargar versión"),
        fields: [
          { name: "file", label: tr("Regulatory.Workflow.File", "Archivo"), type: "file", required: true, accept: ".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg" },
          { name: "reason", label: tr("Regulatory.Workflow.VersionReason", "Motivo de la nueva versión"), type: "textarea", required: true, minlength: 8, maxlength: 2000 }
        ],
        onSubmit: async data => {
          const file = data.file;
          if (!(file instanceof File) || file.size === 0) throw new Error(tr("Regulatory.EvidenceFileRequired", "Seleccione un archivo de evidencia."));
          try {
            const sha256 = await sha256File(file);
            const stored = await uploadDossierEvidence(id, file);
            await apiV2(`/${id}/evidence`, {
              method: "POST",
              body: {
                expectedRevision: workflow.revision,
                requirementId,
                correctionRequestId: correction.id,
                documentId: null,
                storedFileId: stored.id,
                sha256,
                fileName: file.name,
                reason: data.reason.trim()
              }
            });
            toast(tr("Regulatory.Workflow.VersionUploaded", "Nueva versión registrada con SHA-256."), "success");
            await reload();
          } catch (error) {
            if (isRevisionConflict(error)) await handleWorkflowError(error, reload);
            else throw error;
          }
        }
      });
    }));
    body.querySelector("#ra-submit-correction")?.addEventListener("click", () => {
      openFormModal({
        id: "ra-submit-correction-modal",
        title: tr("Regulatory.Workflow.SubmitCorrection", "Enviar corrección a revisión"),
        subtitle: tr("Regulatory.Workflow.SubmitCorrectionHelp", "Todos los requisitos del alcance deben tener una versión activa asociada a esta corrección."),
        submitLabel: tr("Regulatory.Workflow.SubmitCorrection", "Enviar corrección a revisión"),
        fields: [
          { name: "reason", label: tr("Regulatory.Workflow.SubmissionReason", "Resumen de la corrección"), type: "textarea", required: true, minlength: 8, maxlength: 2000 }
        ],
        onSubmit: async data => {
          try {
            await apiV2(`/${id}/corrections/submit`, {
              method: "POST",
              body: {
                expectedRevision: workflow.revision,
                correctionRequestId: correction.id,
                reason: data.reason.trim(),
                requirementIds: correction.requirementIds || [],
                fieldPaths: correction.fieldPaths || [],
                documentIds: correction.documentIds || []
              }
            });
            toast(tr("Regulatory.Workflow.CorrectionSubmitted", "Corrección enviada a revisión técnica."), "success");
            await reload();
          } catch (error) {
            if (isRevisionConflict(error)) await handleWorkflowError(error, reload);
            else throw error;
          }
        }
      });
    });
    body.querySelector("#ra-request-reopen")?.addEventListener("click", () => {
      openFormModal({
        id: "ra-request-reopen-modal",
        title: tr("Regulatory.Workflow.RequestReopen", "Solicitar reapertura"),
        subtitle: tr("Regulatory.Workflow.ReopenHelp", "La reapertura requiere el proceso de gobierno y dos aprobaciones independientes."),
        submitLabel: tr("Regulatory.Workflow.SendRequest", "Enviar solicitud"),
        fields: [{ name: "reason", label: tr("Regulatory.Workflow.Reason", "Motivo"), type: "textarea", required: true, minlength: 8, maxlength: 2000 }],
        onSubmit: async data => {
          try {
            await apiV2(`/${id}/reopen-requests`, { method: "POST", body: { expectedRevision: workflow.revision, reason: data.reason.trim() } });
            toast(tr("Regulatory.Workflow.ReopenRequested", "Solicitud de reapertura registrada."), "success");
            await reload();
          } catch (error) {
            if (isRevisionConflict(error)) await handleWorkflowError(error, reload);
            else throw error;
          }
        }
      });
    });
    body.querySelector("#ra-request-override")?.addEventListener("click", () => {
      openFormModal({
        id: "ra-request-override-modal",
        title: tr("Regulatory.Workflow.RequestOverride", "Solicitar override SoD"),
        subtitle: tr("Regulatory.Workflow.OverrideHelp", "Esta solicitud no omite controles: requiere aprobaciones y consumo auditado."),
        submitLabel: tr("Regulatory.Workflow.SendRequest", "Enviar solicitud"),
        fields: [
          { name: "action", label: tr("Regulatory.Workflow.OverrideAction", "Acción exacta a exceptuar"), required: true, maxlength: 120 },
          { name: "reason", label: tr("Regulatory.Workflow.Reason", "Motivo"), type: "textarea", required: true, minlength: 8, maxlength: 2000 }
        ],
        onSubmit: async data => {
          try {
            await apiV2(`/${id}/override-requests`, { method: "POST", body: { expectedRevision: workflow.revision, action: data.action.trim(), reason: data.reason.trim() } });
            toast(tr("Regulatory.Workflow.OverrideRequested", "Solicitud de override registrada."), "success");
            await reload();
          } catch (error) {
            if (isRevisionConflict(error)) await handleWorkflowError(error, reload);
            else throw error;
          }
        }
      });
    });
    body.querySelector("#ra-archive")?.addEventListener("click", () => {
      openFormModal({
        id: "ra-archive-modal",
        title: tr("Regulatory.Workflow.Archive", "Archivar expediente"),
        subtitle: tr("Regulatory.Workflow.ArchiveHelp", "Solo se puede archivar un expediente cerrado. La acción queda auditada."),
        submitLabel: tr("Regulatory.Workflow.Archive", "Archivar expediente"),
        fields: [{ name: "reason", label: tr("Regulatory.Workflow.Reason", "Motivo"), type: "textarea", required: true, minlength: 8, maxlength: 2000 }],
        onSubmit: async data => {
          try {
            await apiV2(`/${id}/archive`, { method: "POST", body: { expectedRevision: workflow.revision, reason: data.reason.trim() } });
            toast(tr("Regulatory.Workflow.Archived", "Expediente archivado."), "success");
            await showDossiers(body, null);
          } catch (error) {
            if (isRevisionConflict(error)) await handleWorkflowError(error, reload);
            else throw error;
          }
        }
      });
    });
    body.querySelector("#ra-cancel")?.addEventListener("click", () => {
      openFormModal({
        id: "ra-cancel-modal",
        title: tr("Regulatory.Workflow.Cancel", "Cancelar expediente"),
        subtitle: tr("Regulatory.Workflow.CancelHelp", "La cancelación es lógica: conserva requisitos, documentos, historial y auditoría."),
        submitLabel: tr("Regulatory.Workflow.ConfirmCancel", "Confirmar cancelación"),
        fields: [{ name: "reason", label: tr("Regulatory.Workflow.Reason", "Motivo"), type: "textarea", required: true, minlength: 8, maxlength: 2000 }],
        onSubmit: async data => {
          try {
            await apiV2(`/${id}/cancel`, { method: "POST", body: { expectedRevision: workflow.revision, reason: data.reason.trim() } });
            toast(tr("Regulatory.Workflow.Cancelled", "Expediente cancelado sin eliminar evidencia."), "success");
            await showDossierDetail(body, id);
          } catch (error) {
            if (isRevisionConflict(error)) await handleWorkflowError(error, reload);
            else throw error;
          }
        }
      });
    });
    body.querySelectorAll("[data-next]").forEach(btn => btn.addEventListener("click", async () => {
      try {
        const waiver = btn.dataset.next === "DocumentsReceived" ? "Recepcion documentada en laboratorio SoD" : null;
        await api(`/dossiers/${id}/transition`, { method: "POST", body: { targetStatus: btn.dataset.next, waiverReason: waiver } });
        toast(window.t ? window.t("Regulatory.TransitionOk") : "Transición OK", "success");
        await showDossierDetail(body, id);
      } catch (e) { toast(e.message, "error"); }
    }));
    body.querySelectorAll("[data-prep]").forEach(btn => btn.addEventListener("click", async () => {
      try {
        const input = body.querySelector(`[data-req-file="${btn.dataset.prep}"]`);
        const file = input?.files?.[0];
        const stored = await uploadDossierEvidence(id, file);
        await api(`/dossiers/${id}/requirements/${btn.dataset.prep}`, {
          method: "PUT",
          body: {
            status: "Received",
            documentId: null,
            storedFileId: stored.id,
            notes: `Evidencia cargada: ${file.name}`
          }
        });
        toast(tr("Regulatory.EvidenceUploaded", "Evidencia cargada y requisito marcado como recibido."), "success");
        await showDossierDetail(body, id);
      } catch (e) { toast(e.message, "error"); }
    }));
    body.querySelectorAll("[data-accept]").forEach(btn => btn.addEventListener("click", async () => {
      try {
        await api(`/dossiers/${id}/requirements/${btn.dataset.accept}`, {
          method: "PUT",
          body: { status: "Accepted", documentId: null, storedFileId: null, notes: "Revisión técnica aceptada" }
        });
        await showDossierDetail(body, id);
      } catch (e) { toast(e.message, "error"); }
    }));
    body.querySelectorAll("[data-reject]").forEach(btn => btn.addEventListener("click", () => {
      openFormModal({
        id: "ra-reject-requirement-modal",
        title: tr("Regulatory.RejectRequirement", "Rechazar requisito"),
        subtitle: tr("Regulatory.RejectCommentRequired", "El comentario técnico es obligatorio y será visible para el Specialist."),
        submitLabel: tr("Regulatory.ConfirmRejection", "Confirmar rechazo"),
        fields: [
          { name: "notes", label: tr("Regulatory.ReviewComment", "Comentario de revisión"), type: "textarea", required: true, maxlength: 2000 }
        ],
        onSubmit: async (data) => {
          await api(`/dossiers/${id}/requirements/${btn.dataset.reject}`, {
            method: "PUT",
            body: { status: "Rejected", notes: data.notes.trim() }
          });
          await showDossierDetail(body, id);
        }
      });
    }));
    body.querySelector("#ra-approve-internal")?.addEventListener("click", async () => {
      try {
        await api(`/dossiers/${id}/approve-for-submission`, { method: "POST", body: { notes: "Autorización interna SoD" } });
        toast(window.t ? window.t("Regulatory.ApprovedInternal") : "Aprobado internamente para sometimiento", "success");
        await showDossierDetail(body, id);
      } catch (e) { toast(e.message, "error"); }
    });
    body.querySelector("#ra-submit")?.addEventListener("click", () => {
      openFormModal({
        id: "ra-submit-modal",
        title: tr("Regulatory.RegisterSubmission", "Registrar sometimiento"),
        subtitle: tr("Regulatory.SubmissionSubtitle", "Registre los datos reales entregados a la autoridad y adjunte el comprobante."),
        submitLabel: tr("Regulatory.ConfirmSubmission", "Confirmar sometimiento"),
        fields: [
          { name: "procedureNumber", label: tr("Regulatory.ProcedureNumber", "Número de trámite"), required: true, maxlength: 120 },
          { name: "externalNumber", label: tr("Regulatory.ExternalNumber", "Número externo"), required: true, maxlength: 120 },
          { name: "submittedOn", label: tr("Regulatory.SubmissionDate", "Fecha de sometimiento"), type: "date", required: true, value: new Date().toISOString().slice(0, 10) },
          { name: "proof", label: tr("Regulatory.SubmissionProof", "Comprobante de sometimiento"), type: "file", required: true, accept: ".pdf,.png,.jpg,.jpeg" }
        ],
        onSubmit: async (data) => {
          const stored = await uploadDossierEvidence(id, data.proof);
          await api(`/dossiers/${id}/submit`, {
            method: "POST",
            body: {
              procedureNumber: data.procedureNumber.trim(),
              externalNumber: data.externalNumber.trim(),
              submittedOn: new Date(`${data.submittedOn}T12:00:00Z`).toISOString(),
              proofStoredFileId: stored.id
            }
          });
          toast(window.t ? window.t("Regulatory.Submitted") : "Sometimiento registrado", "success");
          await showDossierDetail(body, id);
        }
      });
    });
    body.querySelector("#ra-resubmit")?.addEventListener("click", () => {
      openFormModal({
        id: "ra-resubmit-modal",
        title: tr("Regulatory.RegisterResubmission", "Registrar resometimiento"),
        subtitle: tr("Regulatory.ResubmissionSubtitle", "Registre la respuesta corregida remitida a la autoridad y adjunte el nuevo comprobante."),
        submitLabel: tr("Regulatory.ConfirmResubmission", "Confirmar resometimiento"),
        fields: [
          { name: "procedureNumber", label: tr("Regulatory.ProcedureNumber", "Número de trámite"), required: true, maxlength: 120, value: d.submissionProcedureNumber || "" },
          { name: "externalNumber", label: tr("Regulatory.ExternalNumber", "Número externo"), required: true, maxlength: 120, value: d.submissionExternalNumber || "" },
          { name: "submittedOn", label: tr("Regulatory.ResubmissionDate", "Fecha de resometimiento"), type: "date", required: true, value: new Date().toISOString().slice(0, 10) },
          { name: "proof", label: tr("Regulatory.ResubmissionProof", "Comprobante de resometimiento"), type: "file", required: true, accept: ".pdf,.png,.jpg,.jpeg" }
        ],
        onSubmit: async data => {
          const stored = await uploadDossierEvidence(id, data.proof);
          await api(`/dossiers/${id}/resubmit`, {
            method: "POST",
            body: {
              procedureNumber: data.procedureNumber.trim(),
              externalNumber: data.externalNumber.trim(),
              submittedOn: new Date(`${data.submittedOn}T12:00:00Z`).toISOString(),
              proofStoredFileId: stored.id
            }
          });
          toast(tr("Regulatory.Resubmitted", "Resometimiento registrado."), "success");
          await showDossierDetail(body, id);
        }
      });
    });
    body.querySelector("#ra-start-authority-review")?.addEventListener("click", async () => {
      try {
        await api(`/dossiers/${id}/authority-review/start`, { method: "POST", body: {} });
        toast(tr("Regulatory.AuthorityReviewStarted", "Seguimiento de autoridad iniciado."), "success");
        await showDossierDetail(body, id);
      } catch (error) {
        toast(error.message, "error");
      }
    });
    body.querySelector("#ra-reject-ext")?.addEventListener("click", () => {
      openFormModal({
        id: "ra-reject-ext-modal",
        title: tr("Regulatory.RegisterExternalRejection", "Registrar rechazo de autoridad"),
        subtitle: tr("Regulatory.ExternalRejectionSubtitle", "La decisión externa será inmutable, auditada y conservará su resolución."),
        submitLabel: tr("Regulatory.ConfirmExternalRejection", "Confirmar rechazo"),
        fields: [
          { name: "resolutionNumber", label: tr("Regulatory.ResolutionNumber", "Número de resolución"), required: true, maxlength: 120 },
          { name: "decidedOn", label: tr("Regulatory.DecisionDate", "Fecha de decisión"), type: "date", required: true, value: new Date().toISOString().slice(0, 10) },
          { name: "reason", label: tr("Regulatory.RejectionReason", "Motivo del rechazo"), type: "textarea", required: true, minlength: 8, maxlength: 2000 },
          { name: "resolution", label: tr("Regulatory.ResolutionDocument", "Resolución de la autoridad"), type: "file", required: true, accept: ".pdf,.png,.jpg,.jpeg" }
        ],
        onSubmit: async data => {
          const stored = await uploadDossierEvidence(id, data.resolution);
          await api(`/dossiers/${id}/reject`, {
            method: "POST",
            body: {
              reason: data.reason.trim(),
              resolutionNumber: data.resolutionNumber.trim(),
              decidedOn: new Date(`${data.decidedOn}T12:00:00Z`).toISOString(),
              resolutionStoredFileId: stored.id
            }
          });
          toast(tr("Regulatory.ExternalRejected", "Rechazo de autoridad registrado."), "success");
          await showDossierDetail(body, id);
        }
      });
    });
    body.querySelector("#ra-observe")?.addEventListener("click", () => {
      openFormModal({
        id: "ra-observe-modal",
        title: tr("Ui.RegistrarObservacionAutoridad", "Registrar observación autoridad"),
        subtitle: tr("Regulatory.DescripcionDeLaObservacionDeLaAutoridad", "Descripción de la observación de la autoridad"),
        submitLabel: tr("Common.Save", "Guardar"),
        fields: [
          { name: "description", label: tr("Regulatory.DescripcionDeLaObservacionDeLaAutoridad", "Descripción de la observación de la autoridad"), type: "textarea", required: true, maxlength: 4000 },
          { name: "receivedOn", label: tr("Regulatory.ObservationDate", "Fecha de recepción"), type: "date", required: true, value: new Date().toISOString().slice(0, 10) },
          { name: "dueOn", label: tr("Regulatory.ResponseDeadline", "Plazo de respuesta"), type: "date", required: true, value: new Date(Date.now() + 86400000 * 15).toISOString().slice(0, 10) },
          { name: "evidence", label: tr("Regulatory.AuthorityDocument", "Documento de la autoridad"), type: "file", required: false, accept: ".pdf,.doc,.docx,.png,.jpg,.jpeg" }
        ],
        onSubmit: async (data) => {
          if (data.evidence instanceof File && data.evidence.size > 0) {
            await uploadDossierEvidence(id, data.evidence);
          }
          await api(`/dossiers/${id}/observations`, {
            method: "POST",
            body: {
              description: data.description.trim(),
              receivedOn: new Date(`${data.receivedOn}T12:00:00Z`).toISOString(),
              dueOn: new Date(`${data.dueOn}T12:00:00Z`).toISOString(),
              responsibleUserId: null,
              requirementIds: null
            }
          });
          toast(window.t ? window.t("Regulatory.ObservationSaved") : "Observación registrada", "success");
          await showDossierDetail(body, id);
        }
      });
    });
    body.querySelectorAll("[data-resp]").forEach(btn => btn.addEventListener("click", () => {
      openFormModal({
        id: "ra-observation-response-modal",
        title: tr("Regulatory.RespondObservation", "Responder observación"),
        subtitle: tr("Regulatory.ResponseEvidenceSubtitle", "Documente la respuesta y adjunte la evidencia corregida."),
        submitLabel: tr("Regulatory.SendResponse", "Enviar respuesta"),
        fields: [
          { name: "notes", label: tr("Regulatory.ResponseNotes", "Respuesta"), type: "textarea", required: true, maxlength: 4000 },
          { name: "evidence", label: tr("Regulatory.ResponseEvidence", "Evidencia de respuesta"), type: "file", required: true, accept: ".pdf,.doc,.docx,.png,.jpg,.jpeg" }
        ],
        onSubmit: async (data) => {
          const stored = await uploadDossierEvidence(id, data.evidence);
          await api(`/dossiers/${id}/observations/${btn.dataset.resp}/respond`, {
            method: "POST",
            body: { notes: `${data.notes.trim()} [evidence:${stored.id}]`, close: false }
          });
          toast(window.t ? window.t("Regulatory.ResponseSaved") : "Respuesta registrada", "success");
          await showDossierDetail(body, id);
        }
      });
    }));
    body.querySelector("#ra-approve-ext")?.addEventListener("click", () => {
      const defaultExpiry = new Date(Date.now() + 86400000 * 365 * 3).toISOString().slice(0, 10);
      openFormModal({
        id: "ra-approve-ext-modal",
        title: tr("Ui.RegistrarAprobacionMinsaCssCtRs", "Registrar aprobación MINSA/CSS + CT/RS"),
        subtitle: tr("Regulatory.ExternalDecisionSubtitle", "El número CT/RS lo emite la autoridad; Compliance 360 no lo genera."),
        submitLabel: tr("Common.Save", "Guardar"),
        fields: [
          { name: "registrationNumber", label: tr("Ui.NumeroCtRs", "Número CT/RS"), required: true, maxlength: 120, placeholder: "MQ-4521-07-26" },
          { name: "issuedOn", label: tr("Regulatory.DecisionDate", "Fecha de decisión"), type: "date", required: true, value: new Date().toISOString().slice(0, 10) },
          { name: "expiresOn", label: tr("Regulatory.VencimientoISOYYYYMMDD", "Vencimiento ISO (AAAA-MM-DD)"), type: "date", required: true, value: defaultExpiry },
          { name: "resolution", label: tr("Regulatory.ResolutionDocument", "Resolución de la autoridad"), type: "file", required: true, accept: ".pdf,.png,.jpg,.jpeg" },
          { name: "notes", label: tr("Regulatory.Comments", "Comentarios"), type: "textarea", rows: 2, maxlength: 500 }
        ],
        onSubmit: async (data) => {
          const stored = await uploadDossierEvidence(id, data.resolution);
          await api(`/dossiers/${id}/approve`, {
            method: "POST",
            body: {
              registrationNumber: data.registrationNumber.trim(),
              issuedOn: new Date(`${data.issuedOn}T12:00:00Z`).toISOString(),
              expiresOn: data.expiresOn ? new Date(data.expiresOn).toISOString() : null,
              notes: (data.notes || "").trim() || "Decisión externa registrada",
              resolutionStoredFileId: stored.id
            }
          });
          toast(window.t ? window.t("Regulatory.ExternalApproved") : "Aprobación MINSA/CSS registrada · CT/RS activo", "success");
          await showDossierDetail(body, id);
        }
      });
    });
  }

  async function showRegistrations(body) {
    const regs = await api("/registrations");
    body.innerHTML = `<div class="ra-card"><table class="ra-table"><thead><tr>
      <th>Número CT/RS</th><th>Estado</th><th>Emisión</th><th>Vence</th><th>Días</th>
    </tr></thead><tbody>
    ${(regs || []).map(r => `<tr><td>${esc(r.registrationNumber)}</td><td>${esc(r.status)}</td>
      <td>${esc(r.issuedOn ? String(r.issuedOn).slice(0,10) : "—")}</td>
      <td>${esc(r.expiresOn ? String(r.expiresOn).slice(0,10) : "—")}</td>
      <td>${r.daysRemaining ?? "—"}</td></tr>`).join("")}
    </tbody></table></div>`;
  }

  async function showManufacturers(body) {
    const [mfrs, certs] = await Promise.all([api("/manufacturers"), api("/manufacturer-certificates")]);
    body.innerHTML = `
      <div class="ra-card">
        ${can(P.mfrManage) ? `<div class="ra-actions"><button class="btn primary" id="ra-add-mfr" type="button">Alta fabricante</button></div>` : ""}
        <h3>Fabricantes</h3>
        <ul>${(mfrs || []).map(m => `<li>${esc(m.legalName)} · ${esc(m.countryCode)}</li>`).join("") || "<li>Ninguno</li>"}</ul>
        <h3>Certificados</h3>
        <ul>${(certs || []).map(c => `<li>${esc(c.type)} ${esc(c.number)} · ${esc(c.status)}</li>`).join("") || "<li>Ninguno</li>"}</ul>
      </div>`;
    body.querySelector("#ra-add-mfr")?.addEventListener("click", () => {
      openFormModal({
        id: "ra-add-mfr-modal",
        title: tr("Ui.AltaFabricante", "Alta fabricante"),
        subtitle: tr("Regulatory.ManufacturerSubtitle", "Se registrará el fabricante con su certificado ISO 13485."),
        submitLabel: tr("Common.Create", "Crear"),
        fields: [
          { name: "legalName", label: tr("Regulatory.NombreLegalFabricante", "Nombre legal fabricante"), required: true, maxlength: 220, placeholder: "Acme Medical Co." },
          { name: "countryCode", label: tr("Regulatory.Pais", "País"), required: true, maxlength: 2, value: "CN" }
        ],
        onSubmit: async (data) => {
          const legalName = data.legalName.trim();
          const countryCode = data.countryCode.trim().toUpperCase();
          const m = await api("/manufacturers", {
            method: "POST",
            body: { manufacturerId: null, legalName, countryCode, commercialName: legalName, supplierId: null, contactEmail: null, contactPhone: null }
          });
          await api("/manufacturer-certificates", {
            method: "POST",
            body: {
              manufacturerId: m.id, type: "Iso13485", number: `ISO-${Date.now().toString().slice(-5)}`, issuedBy: "TUV",
              issuedOn: new Date().toISOString(), expiresOn: new Date(Date.now() + 86400000 * 400).toISOString(),
              country: countryCode, legalFormat: "Apostilled", apostilled: true, notarized: false, storedFileId: null, notes: null
            }
          });
          toast(window.t ? window.t("Regulatory.ManufacturerCreated") : "Fabricante creado", "success");
          await showManufacturers(body);
        }
      });
    });
  }

  async function showLicenses(body) {
    const licenses = await api("/operating-licenses");
    body.innerHTML = `
      <div class="ra-card">
        ${can(P.licenseManage) ? `<div class="ra-actions"><button class="btn primary" id="ra-add-lic" type="button">Nueva licencia</button></div>` : ""}
        <p class="ra-muted">Fechas de <strong>constitución</strong> e <strong>inicio de operaciones</strong> son metadatos de compañía (REGUTRACK CTT LICENCIAS OP filas 2–3), independientes de emisión/vencimiento de la licencia.</p>
        <table class="ra-table"><thead><tr><th>Compañía</th><th>Constitución</th><th>Inicio ops</th><th>Tipo</th><th>Número</th><th>Emisión</th><th>Vence</th><th>Estado</th></tr></thead>
        <tbody>${(licenses || []).map(l => `<tr>
          <td>${esc(l.companyName)}</td>
          <td>${esc(l.companyConstitutedOn || "—")}</td>
          <td>${esc(l.operationsStartedOn || "—")}</td>
          <td>${esc(l.licenseType)}</td>
          <td>${esc(l.licenseNumber || "—")}</td>
          <td>${esc(l.issuedOn ? String(l.issuedOn).slice(0,10) : "—")}</td>
          <td>${esc(l.expiresOn ? String(l.expiresOn).slice(0,10) : "—")}</td>
          <td>${esc(l.status)}</td></tr>`).join("")}
        </tbody></table>
      </div>`;
    body.querySelector("#ra-add-lic")?.addEventListener("click", () => {
      const defaultExpiry = new Date(Date.now() + 86400000 * 365).toISOString().slice(0, 10);
      openFormModal({
        id: "ra-add-lic-modal",
        title: tr("Ui.NuevaLicencia", "Nueva licencia"),
        subtitle: tr("Regulatory.LicenseSubtitle", "Registre la licencia operativa de la compañía titular."),
        submitLabel: tr("Common.Create", "Crear"),
        fields: [
          { name: "companyName", label: tr("Ui.Compania", "Compañía"), required: true, maxlength: 200, placeholder: "Irving Corro S.A" },
          { name: "licenseType", label: tr("Regulatory.TipoDeLicencia", "Tipo de licencia"), required: true, maxlength: 200, placeholder: "Distribución de dispositivos médicos" },
          { name: "licenseNumber", label: tr("Regulatory.NumeroDeLicencia", "Número de licencia"), maxlength: 120 },
          { name: "expiresOn", label: tr("Ui.Vencimiento", "Vencimiento"), type: "date", required: true, value: defaultExpiry }
        ],
        onSubmit: async (data) => {
          await api("/operating-licenses", {
            method: "POST",
            body: {
              companyName: data.companyName.trim(), companyId: null, licenseType: data.licenseType.trim(),
              authorityId: null, licenseNumber: (data.licenseNumber || "").trim() || null, issuedOn: null,
              expiresOn: data.expiresOn ? new Date(data.expiresOn).toISOString() : null,
              comments: "Desde RA Console", companyConstitutedOn: null, operationsStartedOn: null
            }
          });
          toast(window.t ? window.t("Regulatory.LicenseCreated") : "Licencia creada", "success");
          await showLicenses(body);
        }
      });
    });
  }

  async function showAlerts(body) {
    const [alerts, settings] = await Promise.all([
      api("/alerts/evaluate"),
      can(P.configure) ? api("/alert-settings") : Promise.resolve(null)
    ]);
    body.innerHTML = `<div class="ra-card">
      ${settings ? `
        <form id="ra-alert-settings-form" class="form-stack">
          <h3>${esc(tr("Regulatory.AlertSettings", "Configuración de alertas"))}</h3>
          <div class="field">
            <label for="ra-alert-thresholds">${esc(tr("Regulatory.AlertThresholds", "Días de anticipación"))}</label>
            <input id="ra-alert-thresholds" name="thresholdsCsv" required pattern="^\\d+(,\\s*\\d+)*$" value="${esc(settings.thresholdsCsv)}">
            <small>${esc(tr("Regulatory.AlertThresholdsHelp", "Valores separados por comas, por ejemplo: 90,60,30,15,7,1,0."))}</small>
          </div>
          <button class="btn primary" type="submit">${esc(tr("Common.Save", "Guardar"))}</button>
        </form>` : ""}
      <h3>${esc(tr("Regulatory.ActiveAlerts", "Alertas activas"))}</h3>
      <ul>${(alerts || []).map(a =>
        `<li><strong>${esc(a.alertType)}</strong> · ${esc(a.message)}</li>`).join("") || `<li>${esc(tr("Regulatory.NoNewAlerts", "Sin alertas nuevas"))}</li>`}</ul>
    </div>`;
    body.querySelector("#ra-alert-settings-form")?.addEventListener("submit", async event => {
      event.preventDefault();
      if (!event.currentTarget.reportValidity()) return;
      const button = event.currentTarget.querySelector("button[type=submit]");
      button.disabled = true;
      try {
        await api("/alert-settings", {
          method: "PUT",
          body: { thresholdsCsv: new FormData(event.currentTarget).get("thresholdsCsv") }
        });
        toast(tr("Regulatory.AlertSettingsSaved", "Configuración de alertas guardada."), "success");
        await showAlerts(body);
      } catch (error) {
        toast(error.message, "error");
        button.disabled = false;
      }
    });
  }

  async function showImport(body) {
    if (!can(P.configure)) {
      body.innerHTML = `<div class="ra-card">${window.t ? window.t("Regulatory.NoConfigure") : "Sin permiso REGULATORY.CONFIGURE"}</div>`;
      return;
    }
    const jobs = await api("/imports");
    body.innerHTML = `
      <div class="ra-card ra-import-card">
        <h3>Importador REGUTRACK</h3>
        <p class="ra-import-hint">Seleccione el Excel contractual y pulse Stage. El proceso puede tardar según el tamaño del archivo.</p>
        <div class="ra-import-controls">
          <input type="file" id="ra-xlsx" accept=".xlsx,.xlsm" />
          <button class="btn primary" id="ra-stage-xlsx" type="button">Stage XLSX</button>
        </div>
        <div id="ra-import-progress" class="ra-import-progress" hidden aria-live="polite" aria-busy="false">
          <div class="ra-import-spinner" aria-hidden="true"></div>
          <div class="ra-import-copy">
            <strong id="ra-import-title">Importando REGUTRACK…</strong>
            <p id="ra-import-step">Preparando…</p>
            <div class="ra-import-bar"><span id="ra-import-bar-fill"></span></div>
          </div>
        </div>
        <ul id="ra-import-jobs">${(jobs || []).map(j => `<li>${esc(j.sourceFileName)} · ${esc(j.status)}</li>`).join("")}</ul>
      </div>`;

    const btn = body.querySelector("#ra-stage-xlsx");
    const progress = body.querySelector("#ra-import-progress");
    const stepEl = body.querySelector("#ra-import-step");
    const titleEl = body.querySelector("#ra-import-title");
    const barFill = body.querySelector("#ra-import-bar-fill");
    const fileInput = body.querySelector("#ra-xlsx");

    const steps = [
      "Validando archivo seleccionado…",
      "Asegurando autoridades y pack de requisitos…",
      "Subiendo Excel al servidor…",
      "Parseando hojas REGUTRACK…",
      "Creando job de importación (stage)…",
      "Finalizando…"
    ];

    btn?.addEventListener("click", async () => {
      let stopLoading = () => {};
      let stepTimer = null;
      let stepIndex = 0;
      try {
        const file = fileInput?.files?.[0];
        if (!file) {
          toast(window.t ? window.t("Regulatory.SelectExcel") : "Seleccione Excel", "error");
          return;
        }

        btn.disabled = true;
        if (fileInput) fileInput.disabled = true;
        progress.hidden = false;
        progress.setAttribute("aria-busy", "true");
        titleEl.textContent = `Importando «${file.name}»…`;
        stepEl.textContent = steps[0];
        barFill.style.width = "8%";

        if (typeof window.startGlobalLoading === "function") {
          stopLoading = window.startGlobalLoading("regutrack-import", { overlay: true });
        }

        stepTimer = setInterval(() => {
          stepIndex = Math.min(stepIndex + 1, steps.length - 2);
          stepEl.textContent = steps[stepIndex];
          barFill.style.width = `${Math.min(85, 12 + stepIndex * 18)}%`;
        }, 1600);

        stepEl.textContent = steps[1];
        await ensureBootstrap();

        const token = localStorage.getItem("c360.token");
        const tenantId = resolveTenantId();
        if (!tenantId) {
          throw new Error(window.t ? window.t("Regulatory.TenantMissing") : "Tenant no identificado");
        }

        stepEl.textContent = steps[2];
        barFill.style.width = "45%";
        const fd = new FormData();
        fd.append("file", file);
        const res = await fetch(`/api/v1/tenants/${tenantId}/regulatory/imports/xlsx`, {
          method: "POST",
          headers: { Authorization: `Bearer ${token}` },
          body: fd
        });
        if (!res.ok) throw new Error(await res.text());

        clearInterval(stepTimer);
        stepTimer = null;
        stepEl.textContent = steps[steps.length - 1];
        barFill.style.width = "100%";
        titleEl.textContent = "Stage completado";

        toast(window.t ? window.t("Regulatory.StageOk") : "Stage OK", "success");
        await showImport(body);
      } catch (e) {
        if (progress) {
          progress.hidden = false;
          progress.setAttribute("aria-busy", "false");
          titleEl.textContent = "Error en la importación";
          stepEl.textContent = e.message || "No se pudo completar el stage.";
          barFill.style.width = "100%";
          barFill.classList.add("error");
        }
        toast(e.message, "error");
        btn.disabled = false;
        if (fileInput) fileInput.disabled = false;
      } finally {
        if (stepTimer) clearInterval(stepTimer);
        stopLoading();
        if (progress && !progress.hidden && !barFill.classList.contains("error")) {
          progress.setAttribute("aria-busy", "false");
        }
      }
    });
  }

  async function showConfig(body) {
    if (!can(P.configure)) {
      body.innerHTML = `<div class="ra-card">${window.t ? window.t("Regulatory.NoConfigureGeneric") : "Sin permiso de configuración"}</div>`;
      return;
    }
    body.innerHTML = `<div class="ra-card">
      <p>Autoridades MINSA/CSS y pack REGUTRACK (22 requisitos).</p>
      <button class="btn primary" id="ra-boot" type="button">Bootstrap regulatorio</button>
      <div id="ra-boot-out"></div>
    </div>`;
    body.querySelector("#ra-boot")?.addEventListener("click", async () => {
      const pack = await ensureBootstrap();
      body.querySelector("#ra-boot-out").innerHTML = `<pre>${esc(JSON.stringify(pack, null, 2))}</pre>`;
      toast(window.t ? window.t("Regulatory.BootstrapOk") : "Bootstrap OK", "success");
    });
  }

  async function showSod(body) {
    if (!can(P.sodManage) && !can(P.configure)) {
      body.innerHTML = `<div class="ra-card">Sin REGULATORY.SOD.MANAGE</div>`;
      return;
    }
    const s = await api("/sod-settings");
    body.innerHTML = `<div class="ra-card"><h3>Política SoD del tenant</h3>
      <pre>${esc(JSON.stringify(s, null, 2))}</pre>
      <p>Defaults regulados activos. Cambios vía API PUT /sod-settings.</p></div>`;
  }

  async function ensureBootstrap() {
    return api("/bootstrap", { method: "POST", body: {} });
  }

  window.renderRegulatoryAffairs = renderRegulatoryAffairs;
})();
