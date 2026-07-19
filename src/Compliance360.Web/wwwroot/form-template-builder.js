/* Compliance Studio — commercial Form Engine (Template Builder) */
(function (global) {
  "use strict";

  try {
  const SCHEMA_VERSION = 2;
  const AUTOSAVE_MS = 12000;
  const MAX_HISTORY = 60;

  const COMPONENT_LIBRARY = [
    { type: "text", label: "Texto", group: "Entrada" },
    { type: "number", label: "Número", group: "Entrada" },
    { type: "decimal", label: "Decimal", group: "Entrada" },
    { type: "email", label: "Correo", group: "Entrada" },
    { type: "phone", label: "Teléfono", group: "Entrada" },
    { type: "date", label: "Fecha", group: "Entrada" },
    { type: "time", label: "Hora", group: "Entrada" },
    { type: "datetime", label: "Fecha/Hora", group: "Entrada" },
    { type: "checkbox", label: "Checkbox", group: "Entrada" },
    { type: "radio", label: "Radio", group: "Entrada" },
    { type: "switch", label: "Switch", group: "Entrada" },
    { type: "select", label: "Lista", group: "Entrada" },
    { type: "multiselect", label: "MultiLista", group: "Entrada" },
    { type: "textarea", label: "Textarea", group: "Entrada" },
    { type: "richtext", label: "Rich Text", group: "Entrada" },
    { type: "file", label: "Archivo", group: "Medios" },
    { type: "image", label: "Imagen", group: "Medios" },
    { type: "gallery", label: "Galería", group: "Medios" },
    { type: "video", label: "Video", group: "Medios" },
    { type: "pdf", label: "PDF", group: "Medios" },
    { type: "documents", label: "Documentos", group: "Medios" },
    { type: "evidence", label: "Evidencias", group: "Medios" },
    { type: "signature", label: "Firma", group: "Firma" },
    { type: "auditorSignature", label: t("Studio.FirmaAuditor"), group: "Firma" },
    { type: "responsibleSignature", label: t("Studio.FirmaResponsable"), group: "Firma" },
    { type: "table", label: "Tabla", group: "Layout" },
    { type: "grid", label: "Grid", group: "Layout" },
    { type: "panel", label: "Panel", group: "Layout" },
    { type: "card", label: "Card", group: "Layout" },
    { type: "accordion", label: "Accordion", group: "Layout" },
    { type: "tabs", label: "Tabs", group: "Layout" },
    { type: "container", label: "Contenedor", group: "Layout" },
    { type: "group", label: "Grupo", group: "Layout" },
    { type: "repeater", label: "Repetidor", group: "Layout" },
    { type: "wizard", label: "Wizard", group: "Layout" },
    { type: "stepper", label: "Stepper", group: "Layout" },
    { type: "splitView", label: "Split View", group: "Layout" },
    { type: "masterDetail", label: t("Studio.MasterDetail"), group: "Layout" },
    { type: "heading", label: "Título", group: "Decoración" },
    { type: "subheading", label: "Subtítulo", group: "Decoración" },
    { type: "label", label: "Etiqueta", group: "Decoración" },
    { type: "separator", label: "Separador", group: "Decoración" },
    { type: "html", label: "HTML", group: "Decoración" },
    { type: "markdown", label: "Markdown", group: "Decoración" },
    { type: "hyperlink", label: t("Studio.Hipervinculo"), group: "Decoración" },
    { type: "button", label: "Botón", group: "Decoración" },
    { type: "location", label: "Ubicación", group: t("Studio.CampoAvanzado") },
    { type: "map", label: "Mapa", group: t("Studio.CampoAvanzado") },
    { type: "qr", label: "QR", group: t("Studio.CampoAvanzado") },
    { type: "barcode", label: "Barcode", group: t("Studio.CampoAvanzado") },
    { type: "rating", label: "Rating", group: t("Studio.CampoAvanzado") },
    { type: "semaphore", label: "Semáforo", group: t("Studio.CampoAvanzado") },
    { type: "status", label: "Estado", group: t("Studio.CampoAvanzado") },
    { type: "progress", label: "Progreso", group: t("Studio.CampoAvanzado") },
    { type: "timeline", label: "Timeline", group: t("Studio.CampoAvanzado") },
    { type: "calculated", label: "Calculado", group: t("Studio.CampoAvanzado") }
  ];

  const KIND_OPTIONS = [
    ["Generic", 0], ["Audit", 1], ["Inspection", 2], ["Evaluation", 3], ["Capa", 4],
    ["Risk", 5], ["Control", 6], ["Checklist", 7], ["Regulatory", 8],
    ["InternalProcess", 9], ["Investigation", 10], ["ActionPlan", 11], ["Document", 12]
  ];

  const STARTERS = [
    { id: "blank", name: "En blanco", kind: 0, desc: "Canvas vacío listo para diseñar.", schema: null },
    { id: "iso-audit", name: t("Studio.AuditoriaISO"), kind: 1, desc: "Hallazgos, evidencias y firmas.", category: "ISO" },
    { id: "capa", name: "CAPA", kind: 4, desc: "Causa raíz, acciones y verificación.", category: "CAPA" },
    { id: "checklist", name: "Checklist", kind: 7, desc: "Ítems conformes / no conformes.", category: "Operaciones" },
    { id: "risk", name: "Riesgos", kind: 5, desc: "Probabilidad, impacto y controles.", category: "Riesgo" },
    { id: "incident", name: "Incidentes", kind: 10, desc: "Clasificación, impacto y timeline.", category: t("Studio.Investigacion") },
    { id: "inspection", name: "Inspección", kind: 2, desc: "Recorrido, fotos y ubicación.", category: "Campo" },
    { id: "evaluation", name: "Evaluación", kind: 3, desc: "Criterios, rating y semáforo.", category: "Calidad" },
    { id: "regulatory", name: "Regulatorio", kind: 8, desc: "Campos normativos y documentos.", category: "Compliance" },
    { id: "docs", name: t("Studio.Documentacion"), kind: 12, desc: "Metadatos y adjuntos.", category: "Documentos" }
  ];

  const OPERATORS = ["Equals", "NotEquals", "Contains", "StartsWith", "EndsWith", "Between", "In", "NotIn", "IsEmpty", "IsNotEmpty"];
  const ACTIONS = ["Show", "Hide", "Require", "Enable", "Disable", "RequestSignature", "RequestEvidence", "BlockSave", "Notify", "SetValue"];

  let studio = emptyStudio();
  let autosaveTimer = null;
  let keyBound = false;

  function emptyStudio() {
    return {
      mode: "home",
      list: [],
      current: null,
      schema: emptySchema(),
      selectedIds: [],
      clipboard: [],
      previewMode: "desktop",
      rightTab: "props",
      dirty: false,
      saving: false,
      lastSavedAt: null,
      theme: localStorage.getItem("cs-theme") || "dark",
      zoom: 1,
      snap: true,
      history: [],
      future: [],
      canManage: false,
      paletteFilter: ""
    };
  }

  function emptySchema(kindLabel) {
    return {
      schemaVersion: SCHEMA_VERSION,
      meta: { name: "", kind: kindLabel || "Generic", locale: "es" },
      layout: { mode: "grid", columns: 12, gap: 12 },
      components: [],
      rules: [],
      expressions: [],
      workflow: {
        steps: [
          { id: "start", type: "Start", label: "Inicio" },
          { id: "review", type: "Review", label: "Revisión" },
          { id: "approval", type: "Approval", label: "Aprobación" },
          { id: "sign", type: "Signature", label: "Firma" },
          { id: "close", type: "Close", label: "Cierre" }
        ]
      },
      theme: { mode: "system", density: "comfortable" }
    };
  }

  function uid(prefix) {
    return `${prefix}_${Math.random().toString(36).slice(2, 10)}`;
  }

  function esc(value) {
    const raw = String(value ?? "");
    if (typeof escapeHtml === "function") return escapeHtml(raw);
    return raw
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function defaultComponent(type) {
    const meta = COMPONENT_LIBRARY.find((c) => c.type === type) || { label: type };
    const needsOptions = ["select", "radio", "multiselect", "status", "semaphore"].includes(type);
    return {
      id: uid("c"),
      type,
      name: `${type}_${Date.now().toString(36)}`,
      label: meta.label,
      description: "",
      placeholder: "",
      tooltip: "",
      required: false,
      readOnly: false,
      visible: true,
      editable: true,
      defaultValue: "",
      pattern: "",
      minLength: null,
      maxLength: null,
      width: type === "heading" || type === "separator" ? "full" : "full",
      height: "auto",
      color: "",
      icon: "",
      alignment: "start",
      colSpan: 12,
      order: studio.schema.components.length,
      options: needsOptions ? ["Opción A", "Opción B", "Opción C"] : [],
      expression: type === "calculated" ? "NOW()" : "",
      audit: true,
      permissions: { roles: [], claim: "" },
      metadata: {},
      children: ["panel", "card", "tabs", "accordion", "container", "group", "repeater", "wizard", "grid"].includes(type) ? [] : undefined
    };
  }

  function normalizeSchema(raw) {
    if (!raw || typeof raw !== "object") return emptySchema();
    if (Number(raw.schemaVersion) === 2 && Array.isArray(raw.components)) {
      return {
        ...emptySchema(),
        ...raw,
        schemaVersion: SCHEMA_VERSION,
        components: raw.components,
        rules: Array.isArray(raw.rules) ? raw.rules : [],
        expressions: Array.isArray(raw.expressions) ? raw.expressions : [],
        workflow: raw.workflow || emptySchema().workflow,
        layout: raw.layout || emptySchema().layout,
        meta: raw.meta || emptySchema().meta,
        theme: raw.theme || emptySchema().theme
      };
    }
    const migrated = emptySchema(raw.meta?.kind);
    migrated.meta = { ...migrated.meta, ...(raw.meta || {}) };
    migrated.components = (raw.fields || []).map((f, idx) => ({
      ...defaultComponent(f.type || "text"),
      ...f,
      id: f.id || uid("c"),
      order: f.order ?? idx,
      colSpan: f.width === "half" ? 6 : f.width === "third" ? 4 : 12
    }));
    migrated.rules = (raw.rules || []).map((r) => ({
      id: r.id || uid("r"),
      name: r.name || "Regla",
      logic: "AND",
      conditions: [{ fieldId: r.whenFieldId, operator: capitalize(r.operator || "Equals"), value: r.value }],
      elseConditions: [],
      actions: [{ type: capitalize(r.thenAction || "Show"), targetId: r.thenFieldId }],
      elseActions: []
    }));
    return migrated;
  }

  function capitalize(s) {
    const v = String(s || "");
    return v ? v.charAt(0).toUpperCase() + v.slice(1) : v;
  }

  function parseSchema(json) {
    try {
      return normalizeSchema(JSON.parse(json || "{}"));
    } catch {
      return emptySchema();
    }
  }

  function serializeSchema() {
    return JSON.stringify(studio.schema);
  }

  function pushHistory() {
    studio.history.push(serializeSchema());
    if (studio.history.length > MAX_HISTORY) studio.history.shift();
    studio.future = [];
  }

  function undo() {
    if (!studio.history.length) return;
    studio.future.push(serializeSchema());
    studio.schema = parseSchema(studio.history.pop());
    studio.dirty = true;
    paintStudio();
  }

  function redo() {
    if (!studio.future.length) return;
    studio.history.push(serializeSchema());
    studio.schema = parseSchema(studio.future.pop());
    studio.dirty = true;
    paintStudio();
  }

  function markDirty() {
    studio.dirty = true;
    updateChromeStatus();
    scheduleAutosave();
  }

  function scheduleAutosave() {
    if (!studio.canManage || !studio.current) return;
    clearTimeout(autosaveTimer);
    autosaveTimer = setTimeout(() => {
      if (studio.dirty && !studio.saving) saveDraft(true);
    }, AUTOSAVE_MS);
  }

  function starterSchema(starterId) {
    if (starterId === "blank") return emptySchema();
    const map = {
      "iso-audit": [
        ["heading", "Auditoría ISO"],
        ["select", "Resultado"],
        ["textarea", "Hallazgo"],
        ["evidence", "Evidencia"],
        ["auditorSignature", "Firma auditor"],
        ["responsibleSignature", "Firma responsable"]
      ],
      capa: [
        ["heading", "Acción CAPA"],
        ["select", "Tipo"],
        ["textarea", "Descripción"],
        ["textarea", "Causa raíz"],
        ["repeater", "Acciones"],
        ["date", "Fecha verificación"],
        ["responsibleSignature", "Firma cierre"]
      ],
      checklist: [
        ["heading", "Checklist"],
        ["text", "Área"],
        ["repeater", "Ítems"],
        ["semaphore", "Estado general"],
        ["signature", "Firma"]
      ],
      risk: [
        ["heading", "Evaluación de riesgo"],
        ["text", "Riesgo"],
        ["number", "Probabilidad"],
        ["number", "Impacto"],
        ["calculated", "Nivel"],
        ["textarea", "Controles"]
      ],
      incident: [
        ["heading", "Incidente"],
        ["datetime", "Ocurrencia"],
        ["select", "Severidad"],
        ["textarea", "Descripción"],
        ["timeline", "Línea de tiempo"],
        ["evidence", "Evidencias"]
      ],
      inspection: [
        ["heading", "Inspección"],
        ["location", "Ubicación"],
        ["gallery", "Fotos"],
        ["checklist", "Puntos"],
        ["signature", "Inspector"]
      ],
      evaluation: [
        ["heading", "Evaluación"],
        ["rating", "Puntaje"],
        ["semaphore", "Semáforo"],
        ["textarea", "Observaciones"]
      ],
      regulatory: [
        ["heading", "Formulario regulatorio"],
        ["text", "Norma"],
        ["documents", "Documentos"],
        ["pdf", "Adjunto PDF"],
        ["auditorSignature", "Firma cumplimiento"]
      ],
      docs: [
        ["heading", "Metadatos documentales"],
        ["text", "Título"],
        ["select", "Clasificación"],
        ["file", "Archivo"],
        ["date", "Vigencia"]
      ]
    };
    const schema = emptySchema();
    const rows = map[starterId] || [];
    schema.components = rows.map(([type, label], idx) => {
      const c = defaultComponent(type === "checklist" ? "repeater" : type);
      c.label = label;
      c.order = idx;
      if (type === "Resultado" || label === "Resultado") c.options = ["Conforme", "Observación", "Crítico"];
      if (label === "Severidad") c.options = ["Baja", "Media", "Alta", "Crítica"];
      if (label === "Tipo") c.options = ["Correctiva", "Preventiva"];
      if (label === "Nivel") c.expression = t("Studio.ProbabilidadImpacto");
      return c;
    });
    if (starterId === "iso-audit") {
      schema.rules.push({
        id: uid("r"),
        name: t("Studio.CriticoExigeEvidence"),
        logic: "AND",
        conditions: [{ fieldId: schema.components[1]?.id, operator: "Equals", value: "Crítico" }],
        elseConditions: [],
        actions: [
          { type: "Show", targetId: schema.components[3]?.id },
          { type: "Require", targetId: schema.components[3]?.id },
          { type: t("Studio.RequestSignature"), targetId: schema.components[4]?.id }
        ],
        elseActions: [{ type: "Hide", targetId: schema.components[3]?.id }]
      });
    }
    schema.expressions.push({
      id: uid("e"),
      name: t("Studio.MarcaTemporal"),
      formula: "NOW()",
      targetComponentId: null
    });
    return schema;
  }

  function mountHost() {
    ensureStudioStyles();
    let host = document.getElementById("compliance-studio-root");
    if (!host) {
      host = document.createElement("div");
      host.id = "compliance-studio-root";
      host.style.cssText = "position:fixed;inset:0;z-index:120;overflow:auto;";
      document.body.appendChild(host);
    }
    return host;
  }

  function ensureStudioStyles() {
    if (document.getElementById("compliance-studio-css")) return;
    const link = document.createElement("link");
    link.id = "compliance-studio-css";
    link.rel = "stylesheet";
    link.href = "/form-builder-studio.css?v=studio-2";
    document.head.appendChild(link);
  }

  function unmountHost() {
    clearTimeout(autosaveTimer);
    document.getElementById("compliance-studio-root")?.remove();
  }

  async function renderFormTemplateBuilder(content) {
    try {
      unmountHost();
      studio = emptyStudio();
      studio.canManage = hasAnyPermission(["TEMPLATE.MANAGE"]);
      if (!hasAnyPermission(["TEMPLATE.MANAGE", "TEMPLATE.READ"])) {
        content.innerHTML = pageHeader(t("Dashboard.ComplianceStudio"), "Sin permiso TEMPLATE.READ / TEMPLATE.MANAGE.", "Enterprise");
        return;
      }
      content.innerHTML = `<div class="card"><p class="metric-label">Abriendo Compliance Studio…</p></div>`;
      studio.list = (await request(`/tenants/${state.tenantId}/form-templates`, { silent: true })) || [];
      if (!Array.isArray(studio.list)) studio.list = [];
      bindKeys();
      paintHome();
      content.innerHTML = `<div class="card"><p class="metric-label">Compliance Studio activo. Si no ve el diseñador, pulse F5.</p></div>`;
    } catch (err) {
      console.error(t("Studio.ComplianceStudioOpenFailed"), err);
      content.innerHTML = `<section class="error-state rich"><strong>No se pudo abrir Compliance Studio.</strong><p>${esc(err && err.message ? err.message : err)}</p></section>`;
      throw err;
    }
  }

  function bindKeys() {
    if (keyBound) return;
    keyBound = true;
    document.addEventListener("keydown", (ev) => {
      if (!document.getElementById("compliance-studio-root")) return;
      const tag = (ev.target && ev.target.tagName) || "";
      const typing = tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT" || ev.target?.isContentEditable;
      if ((ev.ctrlKey || ev.metaKey) && ev.key.toLowerCase() === "s") {
        ev.preventDefault();
        if (studio.canManage && studio.current) saveDraft(false);
      }
      if (typing) return;
      if ((ev.ctrlKey || ev.metaKey) && ev.key.toLowerCase() === "z") {
        ev.preventDefault();
        undo();
      }
      if ((ev.ctrlKey || ev.metaKey) && (ev.key.toLowerCase() === "y" || (ev.shiftKey && ev.key.toLowerCase() === "z"))) {
        ev.preventDefault();
        redo();
      }
      if ((ev.ctrlKey || ev.metaKey) && ev.key.toLowerCase() === "d") {
        ev.preventDefault();
        duplicateSelected();
      }
      if ((ev.ctrlKey || ev.metaKey) && ev.key.toLowerCase() === "c") {
        copySelected();
      }
      if ((ev.ctrlKey || ev.metaKey) && ev.key.toLowerCase() === "v") {
        pasteClipboard();
      }
      if (ev.key === "Delete" || ev.key === "Backspace") {
        deleteSelected();
      }
      if (ev.key === "Escape") {
        studio.selectedIds = [];
        paintStudio();
      }
    });
  }

  function paintHome() {
    studio.mode = "home";
    const host = mountHost();
    const q = (studio.homeQuery || "").toLowerCase();
    const rows = studio.list.filter((t) => !q || `${t.name} ${t.code} ${t.category}`.toLowerCase().includes(q));
    host.innerHTML = `
      <div class="cs-home" role="main" aria-label="${t("Dashboard.ComplianceStudio")}">
        <div class="cs-home-head">
          <div class="cs-brand"><span class="cs-brand-mark">CS</span> ${t("Dashboard.ComplianceStudio")}</div>
          <h1>Form Engine Enterprise</h1>
          <p>Constructor visual de formularios, auditorías, CAPA, checklists, riesgos e inspecciones. Diseñe una vez, publique y reutilice en toda la plataforma.</p>
          <div class="cs-home-actions">
            ${studio.canManage ? `<button class="cs-btn primary" type="button" id="cs-new-blank">Nueva desde cero</button>` : ""}
            <button class="cs-btn" type="button" id="cs-exit">Volver al workspace</button>
            <input class="cs-search" style="max-width:280px;margin:0" id="cs-home-search" placeholder="Buscar plantillas…" value="${esc(studio.homeQuery || "")}">
          </div>
        </div>
        <h2 class="cs-section-title" style="max-width:1100px;margin:0 auto;padding:.5rem 0">Plantillas base</h2>
        <div class="cs-cards">
          ${STARTERS.map((s) => `
            <article class="cs-card cs-starter" data-starter="${s.id}" tabindex="0" role="button">
              <div class="cs-pill">${esc(s.category || "Starter")}</div>
              <h3>${esc(s.name)}</h3>
              <p>${esc(s.desc)}</p>
            </article>`).join("")}
        </div>
        <h2 class="cs-section-title" style="max-width:1100px;margin:1.5rem auto 0;padding:.5rem 0">Biblioteca del tenant (${rows.length})</h2>
        <div class="cs-cards">
          ${rows.length ? rows.map((t) => `
            <article class="cs-card" data-open="${t.id}" tabindex="0" role="button">
              <div class="cs-pill ${String(t.status).includes("Published") || t.status === 1 ? "ok" : ""}">${esc(t.status)} · v${esc(t.publishedVersionNumber || "draft")}</div>
              <h3>${esc(t.name)}</h3>
              <p>${esc(t.code)} · ${esc(t.category)} · ${esc(kindLabel(t.kind))}</p>
            </article>`).join("") : `<p style="color:#93a4bd">Aún no hay plantillas. Elija un starter para comenzar.</p>`}
        </div>
      </div>`;
    host.querySelector("#cs-exit")?.addEventListener("click", exitStudio);
    host.querySelector("#cs-new-blank")?.addEventListener("click", () => createFromStarter("blank"));
    host.querySelector("#cs-home-search")?.addEventListener("input", (e) => {
      studio.homeQuery = e.target.value;
      paintHome();
    });
    host.querySelectorAll("[data-starter]").forEach((el) => {
      el.addEventListener("click", () => createFromStarter(el.dataset.starter));
      el.addEventListener("keydown", (ev) => { if (ev.key === "Enter") createFromStarter(el.dataset.starter); });
    });
    host.querySelectorAll("[data-open]").forEach((el) => {
      el.addEventListener("click", () => openTemplate(el.dataset.open));
      el.addEventListener("keydown", (ev) => { if (ev.key === "Enter") openTemplate(el.dataset.open); });
    });
  }

  function kindLabel(kind) {
    if (typeof kind === "string" && Number.isNaN(Number(kind))) return kind;
    return KIND_OPTIONS.find(([, v]) => v === Number(kind))?.[0] || String(kind);
  }

  function exitStudio() {
    unmountHost();
    location.hash = "#/tenant-administration";
  }

  async function createFromStarter(starterId) {
    if (!studio.canManage) return;
    const starter = STARTERS.find((s) => s.id === starterId) || STARTERS[0];
    const name = prompt(t("Studio.NombreDeLaPlantilla"), starter.name);
    if (!name) return;
    const code = prompt(t("Studio.CodigoUnico"), `TPL-${Date.now().toString().slice(-6)}`);
    if (!code) return;
    const schema = starterSchema(starterId);
    schema.meta.name = name;
    schema.meta.kind = KIND_OPTIONS.find(([, v]) => v === starter.kind)?.[0] || "Generic";
    const created = await request(`/tenants/${state.tenantId}/form-templates`, {
      method: "POST",
      loadingContext: "save",
      body: {
        name,
        code,
        category: starter.category || "General",
        kind: starter.kind,
        description: starter.desc,
        initialSchemaJson: JSON.stringify(schema)
      }
    });
    toast(t("Studio.PlantillaCreadaEnComplianceStudio"), "success");
    studio.list = (await request(`/tenants/${state.tenantId}/form-templates`)) || [];
    if (created?.id) await openTemplate(created.id);
  }

  async function openTemplate(id) {
    const detail = await request(`/tenants/${state.tenantId}/form-templates/${id}`, { loadingContext: "tenant-administration" });
    studio.current = detail;
    studio.schema = parseSchema(detail.workingVersion?.schemaJson);
    studio.selectedIds = studio.schema.components[0] ? [studio.schema.components[0].id] : [];
    studio.dirty = false;
    studio.history = [];
    studio.future = [];
    studio.mode = "design";
    paintStudio();
  }

  function selectedComponents() {
    return studio.schema.components.filter((c) => studio.selectedIds.includes(c.id));
  }

  function paintStudio() {
    if (studio.mode !== "design" || !studio.current) {
      paintHome();
      return;
    }
    const host = mountHost();
    const t = studio.current;
    const can = studio.canManage;
    host.innerHTML = `
      <div class="cs-root" data-cs-theme="${esc(studio.theme)}" role="application" aria-label="${t("Studio.DisenadorComplianceStudio")}">
        <header class="cs-top">
          <div class="cs-brand"><span class="cs-brand-mark">CS</span> Studio</div>
          <div class="cs-sep"></div>
          <button class="cs-btn" type="button" id="cs-back" title="${t("Studio.Biblioteca")}">←</button>
          <strong style="font-size:.92rem">${esc(t.name)}</strong>
          <span class="cs-pill">${esc(t.code)}</span>
          <span class="cs-pill ${studio.dirty ? "warn" : "ok"}" id="cs-dirty">${studio.dirty ? "Cambios sin guardar" : t("Studio.Sincronizado")}</span>
          <span class="cs-pill">${esc(String(t.status))} · v${esc(t.workingVersion?.versionNumber || "—")}</span>
          <div class="cs-grow"></div>
          <button class="cs-btn" type="button" id="cs-undo" ${studio.history.length ? "" : "disabled"} title="Ctrl+Z">Deshacer</button>
          <button class="cs-btn" type="button" id="cs-redo" ${studio.future.length ? "" : "disabled"} title="Ctrl+Y">Rehacer</button>
          <button class="cs-btn" type="button" id="cs-zoom-out">−</button>
          <span class="cs-pill">${Math.round(studio.zoom * 100)}%</span>
          <button class="cs-btn" type="button" id="cs-zoom-in">+</button>
          <button class="cs-btn" type="button" id="cs-theme">${studio.theme === "dark" ? "Claro" : "Oscuro"}</button>
          <button class="cs-btn" type="button" data-preview="desktop">Desktop</button>
          <button class="cs-btn" type="button" data-preview="tablet">Tablet</button>
          <button class="cs-btn" type="button" data-preview="mobile">Móvil</button>
          <button class="cs-btn" type="button" data-preview="landscape">Landscape</button>
          ${can ? `<button class="cs-btn" type="button" id="cs-dup">Duplicar</button>
          <button class="cs-btn" type="button" id="cs-save">${t("Common.Save")}</button>
          <button class="cs-btn success" type="button" id="cs-publish">Publicar</button>` : ""}
        </header>
        <div class="cs-body">
          <aside class="cs-left" aria-label="${t("Studio.BibliotecaDeComponentes")}">
            <div class="cs-section-title">Componentes</div>
            <input class="cs-search" id="cs-palette-search" placeholder="Filtrar…" value="${esc(studio.paletteFilter)}">
            ${renderPalette(can)}
            <div class="cs-section-title">Versiones</div>
            <div style="padding:0 .85rem .85rem;display:grid;gap:.35rem">
              ${(t.versions || []).map((v) => `
                <div style="display:flex;justify-content:space-between;gap:.35rem;align-items:center;font-size:.72rem;color:var(--cs-muted)">
                  <span>${esc(v.versionNumber)} · ${v.isPublished ? "Publicada" : "Borrador"}</span>
                  ${can ? `<button class="cs-btn" type="button" data-restore="${v.id}" style="padding:.15rem .35rem">Restaurar</button>` : ""}
                </div>`).join("") || "<span style='font-size:.72rem;color:var(--cs-muted)'>—</span>"}
            </div>
          </aside>
          <main class="cs-center" id="cs-canvas" aria-label="Canvas">
            <div class="cs-canvas-inner" id="cs-canvas-inner" style="transform:scale(${studio.zoom})">
              ${studio.previewMode !== "design" && studio.previewMode ? "" : ""}
              <div id="cs-nodes">${renderCanvas(can)}</div>
              <div class="cs-preview-frame ${esc(studio.previewMode)}" id="cs-preview">${renderPreview()}</div>
            </div>
          </main>
          <aside class="cs-right" aria-label="${t("Studio.Inspector")}">
            <div class="cs-tabs">
              ${["props", "rules", "expr", "flow", "meta"].map((tab) => `
                <button type="button" class="${studio.rightTab === tab ? "on" : ""}" data-tab="${tab}">${
                  ({ props: "Propiedades", rules: "Reglas", expr: "Expresiones", flow: "Workflow", meta: "Plantilla" })[tab]
                }</button>`).join("")}
            </div>
            <div class="cs-props" id="cs-inspector">${renderInspector(can)}</div>
          </aside>
        </div>
        <footer class="cs-footer">
          <span>Tenant ${esc(state.tenantId)}</span>
          <span>·</span>
          <span>${studio.schema.components.length} componentes</span>
          <span>·</span>
          <span>${studio.schema.rules.length} reglas</span>
          <span>·</span>
          <span>${studio.schema.expressions.length} expresiones</span>
          <span>·</span>
          <span>Snap ${studio.snap ? "ON" : "OFF"}</span>
          <span>·</span>
          <span id="cs-save-clock">${studio.lastSavedAt ? `Autosave ${new Date(studio.lastSavedAt).toLocaleTimeString()}` : t("Common.AutosaveListo")}</span>
          <span class="cs-grow"></span>
          <span>Ctrl+S guardar · Ctrl+Z/Y historial · Del eliminar · Ctrl+D duplicar</span>
        </footer>
      </div>`;
    wireStudio(host);
  }

  function renderPalette(can) {
    const q = (studio.paletteFilter || "").toLowerCase();
    const items = COMPONENT_LIBRARY.filter((c) => !q || `${c.label} ${c.type} ${c.group}`.toLowerCase().includes(q));
    const groups = [...new Set(items.map((i) => i.group))];
    if (!can) return `<p style="padding:.85rem;color:var(--cs-muted);font-size:.8rem">${t("Dashboard.SoloLectura")}</p>`;
    return groups.map((g) => `
      <div class="cs-section-title">${esc(g)}</div>
      <div class="cs-palette">
        ${items.filter((i) => i.group === g).map((i) => `
          <button type="button" class="cs-palette-item" draggable="true" data-add="${i.type}" title="${esc(i.label)}">
            <strong>${esc(i.label)}</strong>${esc(i.type)}
          </button>`).join("")}
      </div>`).join("");
  }

  function renderCanvas(can) {
    const comps = [...studio.schema.components].sort((a, b) => (a.order || 0) - (b.order || 0));
    if (!comps.length) {
      return `<div class="cs-drop-hint" id="cs-drop-zone">Arrastre componentes aquí o pulse en la biblioteca.<br><small>Grid de 12 columnas · guías inteligentes · DnD</small></div>`;
    }
    return `<div id="cs-drop-zone">${comps.map((c) => {
      const selected = studio.selectedIds.includes(c.id);
      return `<div class="cs-node ${selected ? "selected" : ""}" data-node="${c.id}" draggable="${can}" style="grid-column: span ${c.colSpan || 12}">
        ${can ? `<div class="cs-node-actions">
          <button type="button" data-dup="${c.id}">Dup</button>
          <button type="button" data-del="${c.id}">Del</button>
        </div>` : ""}
        <div class="cs-node-label">${esc(c.label)}${c.required ? " *" : ""}</div>
        <div class="cs-node-meta">${esc(c.type)} · ${esc(c.name)} · col ${c.colSpan || 12}${c.expression ? " · calc" : ""}</div>
      </div>`;
    }).join("")}</div>`;
  }

  function renderPreview() {
    const comps = [...studio.schema.components].sort((a, b) => (a.order || 0) - (b.order || 0)).filter((c) => c.visible !== false);
    return `<h3 style="margin-top:0">Vista previa · ${esc(studio.previewMode)}</h3>
      <div class="cs-grid-2">${comps.map((c) => previewField(c)).join("") || "<p>Sin componentes</p>"}</div>`;
  }

  function previewField(c) {
    const common = `placeholder="${esc(c.placeholder || "")}" ${c.readOnly || c.editable === false ? "readonly disabled" : ""} title="${esc(c.tooltip || "")}"`;
    let control = `<input type="text" ${common} value="${esc(c.defaultValue || "")}">`;
    switch (c.type) {
      case "textarea": case "richtext": case "markdown": control = `<textarea ${common}></textarea>`; break;
      case "number": case "decimal": case "progress": case "rating": control = `<input type="number" ${common}>`; break;
      case "email": control = `<input type="email" ${common}>`; break;
      case "date": control = `<input type="date" ${common}>`; break;
      case "time": control = `<input type="time" ${common}>`; break;
      case "datetime": control = `<input type="datetime-local" ${common}>`; break;
      case "checkbox": case "switch": control = `<label><input type="checkbox"> ${esc(c.label)}</label>`; break;
      case "select": case "multiselect": case "status": case "semaphore":
        control = `<select ${c.type === "multiselect" ? "multiple" : ""}>${(c.options || []).map((o) => `<option>${esc(o)}</option>`).join("")}</select>`; break;
      case "radio": control = (c.options || []).map((o) => `<label><input type="radio" name="${esc(c.name)}"> ${esc(o)}</label>`).join(" "); break;
      case "heading": return `<div><h3>${esc(c.label)}</h3></div>`;
      case "subheading": return `<div><h4>${esc(c.label)}</h4></div>`;
      case "separator": return `<div><hr></div>`;
      case "label": return `<div><p>${esc(c.label)}</p></div>`;
      case "signature": case "auditorSignature": case "responsibleSignature":
        control = `<div style="border:1px dashed #94a3b8;padding:1.4rem;text-align:center;border-radius:8px">Área de firma</div>`; break;
      case "file": case "image": case "evidence": case "gallery": case "documents": case "pdf": case "video":
        control = t("Studio.InputTypeFile"); break;
      case "location": case "map": control = `<input type="text" placeholder="Lat, Lng" ${common}>`; break;
      case "calculated": control = `<input type="text" readonly value="${esc(c.expression || "")}">`; break;
      case "button": control = `<button type="button">${esc(c.label)}</button>`; break;
      case "hyperlink": control = `<a href="#">${esc(c.label)}</a>`; break;
      case "panel": case "card": case "container": case "group": case "tabs": case "accordion": case "wizard": case "stepper": case "grid": case "table": case "repeater": case "splitView": case "masterDetail": case "timeline":
        control = `<div style="border:1px dashed #cbd5e1;padding:.75rem;border-radius:8px;color:#64748b">${esc(c.type)} · layout</div>`; break;
      default: break;
    }
    if (["checkbox", "switch"].includes(c.type)) return `<div>${control}<small>${esc(c.description || "")}</small></div>`;
    return `<div><label style="font-size:.78rem;font-weight:700">${esc(c.label)}${c.required ? " *" : ""}</label>${control}<small style="color:#64748b">${esc(c.description || c.tooltip || "")}</small></div>`;
  }

  function renderInspector(can) {
    if (studio.rightTab === "meta") return renderMeta(can);
    if (studio.rightTab === "rules") return renderRules(can);
    if (studio.rightTab === "expr") return renderExpressions(can);
    if (studio.rightTab === "flow") return renderWorkflow(can);
    const c = selectedComponents()[0];
    if (!c) return `<p style="color:var(--cs-muted)">Seleccione un componente en el canvas.</p>`;
    if (!can) return `<p>${esc(c.label)} · ${esc(c.type)}</p>`;
    return `
      <label>Label<input id="p-label" value="${esc(c.label)}"></label>
      <label>${t("Common.Name")}<input id="p-name" value="${esc(c.name)}"></label>
      <label>${t("Studio.Descripcion")}<input id="p-desc" value="${esc(c.description || "")}"></label>
      <label>Placeholder<input id="p-ph" value="${esc(c.placeholder || "")}"></label>
      <label>Tooltip<input id="p-tip" value="${esc(c.tooltip || "")}"></label>
      <label>Valor default<input id="p-def" value="${esc(c.defaultValue || "")}"></label>
      <label>Regex<input id="p-pat" value="${esc(c.pattern || "")}"></label>
      <label>Min length<input id="p-min" type="number" value="${c.minLength ?? ""}"></label>
      <label>Max length<input id="p-max" type="number" value="${c.maxLength ?? ""}"></label>
      <label>Col span (1-12)<input id="p-col" type="number" min="1" max="12" value="${c.colSpan || 12}"></label>
      <label>Alineación<select id="p-align"><option value="start" ${c.alignment === "start" ? "selected" : ""}>Inicio</option><option value="center" ${c.alignment === "center" ? "selected" : ""}>Centro</option><option value="end" ${c.alignment === "end" ? "selected" : ""}>Fin</option></select></label>
      <label>Color<input id="p-color" value="${esc(c.color || "")}"></label>
      <label>Icono<input id="p-icon" value="${esc(c.icon || "")}"></label>
      <label>${t("Studio.Expresion")}<input id="p-expr" value="${esc(c.expression || "")}" placeholder="NOW() / TOTAL * 1.16"></label>
      <label>Opciones (coma)<input id="p-opt" value="${esc((c.options || []).join(", "))}"></label>
      <label>Claim permiso<input id="p-claim" value="${esc(c.permissions?.claim || "")}"></label>
      <label><input type="checkbox" id="p-req" ${c.required ? "checked" : ""}> ${t("Common.Required")}</label>
      <label><input type="checkbox" id="p-ro" ${c.readOnly ? "checked" : ""}> ${t("Dashboard.SoloLectura")}</label>
      <label><input type="checkbox" id="p-vis" ${c.visible !== false ? "checked" : ""}> Visible</label>
      <label><input type="checkbox" id="p-ed" ${c.editable !== false ? "checked" : ""}> Editable</label>
      <label><input type="checkbox" id="p-aud" ${c.audit !== false ? "checked" : ""}> Auditoría</label>
      <button class="cs-btn primary" type="button" id="p-apply">Aplicar</button>`;
  }

  function renderMeta(can) {
    const t = studio.current;
    return `
      <label>${t("Common.Name")}<input id="m-name" value="${esc(t.name)}" ${can ? "" : "disabled"}></label>
      <label>Categoría<input id="m-cat" value="${esc(t.category)}" ${can ? "" : "disabled"}></label>
      <label>Tipo<select id="m-kind" ${can ? "" : "disabled"}>
        ${KIND_OPTIONS.map(([l, v]) => `<option value="${v}" ${Number(t.kind) === v || t.kind === l ? "selected" : ""}>${l}</option>`).join("")}
      </select></label>
      <label>${t("Studio.Descripcion")}<textarea id="m-desc" rows="3" ${can ? "" : "disabled"}>${esc(t.description || "")}</textarea></label>
      ${can ? `<button class="cs-btn primary" type="button" id="m-apply">Actualizar cabecera</button>
      <button class="cs-btn" type="button" id="m-archive">Archivar</button>` : ""}`;
  }

  function renderRules(can) {
    const comps = studio.schema.components;
    const options = comps.map((c) => `<option value="${c.id}">${esc(c.label)}</option>`).join("");
    return `
      ${(studio.schema.rules || []).map((r, i) => `
        <div style="border:1px solid var(--cs-line);border-radius:10px;padding:.55rem;margin-bottom:.45rem">
          <strong style="font-size:.8rem">${esc(r.name || "Regla")}</strong>
          <div style="font-size:.7rem;color:var(--cs-muted);margin-top:.25rem">
            ${esc(r.logic || "AND")} ${(r.conditions || []).map((c) => `${esc(c.operator)} "${esc(c.value)}"`).join(" · ")}
            → ${(r.actions || []).map((a) => esc(a.type)).join(", ")}
          </div>
          ${can ? `<button class="cs-btn" type="button" data-rule-del="${i}" style="margin-top:.35rem">${t("Common.Delete")}</button>` : ""}
        </div>`).join("") || `<p style="color:var(--cs-muted);font-size:.8rem">Sin reglas.</p>`}
      ${can ? `
        <div class="cs-section-title" style="padding:0">Nueva regla</div>
        <label>${t("Common.Name")}<input id="r-name" value="${t("Studio.ReglaCondicional")}"></label>
        <label>Lógica<select id="r-logic"><option>AND</option><option>OR</option><option>NOT</option></select></label>
        <label>${t("Studio.Campo")}<select id="r-field">${options}</select></label>
        <label>Operador<select id="r-op">${OPERATORS.map((o) => `<option>${o}</option>`).join("")}</select></label>
        <label>Valor<input id="r-val" placeholder="${t("Studio.Critico")}"></label>
        <label>Acción<select id="r-act">${ACTIONS.map((a) => `<option>${a}</option>`).join("")}</select></label>
        <label>Objetivo<select id="r-target">${options}</select></label>
        <label>ELSE acción<select id="r-else"><option value="">(ninguna)</option>${ACTIONS.map((a) => `<option>${a}</option>`).join("")}</select></label>
        <button class="cs-btn primary" type="button" id="r-add">Agregar regla</button>` : ""}`;
  }

  function renderExpressions(can) {
    return `
      <p style="font-size:.75rem;color:var(--cs-muted)">Funciones: NOW(), TODAY(), CURRENT_USER, CURRENT_TENANT, + − * /, concat.</p>
      ${(studio.schema.expressions || []).map((e, i) => `
        <div style="border:1px solid var(--cs-line);border-radius:8px;padding:.45rem;margin-bottom:.35rem;font-size:.75rem">
          <strong>${esc(e.name)}</strong><div style="color:var(--cs-muted)">${esc(e.formula)}</div>
          ${can ? `<button class="cs-btn" type="button" data-expr-del="${i}">${t("Common.Remove")}</button>` : ""}
        </div>`).join("") || "<p style='color:var(--cs-muted);font-size:.8rem'>Sin expresiones.</p>"}
      ${can ? `
        <label>${t("Common.Name")}<input id="e-name" value="Total"></label>
        <label>Fórmula<input id="e-formula" placeholder="${t("Studio.Subtotal116")}"></label>
        <label>Destino<select id="e-target"><option value="">(ninguno)</option>${studio.schema.components.map((c) => `<option value="${c.id}">${esc(c.label)}</option>`).join("")}</select></label>
        <button class="cs-btn primary" type="button" id="e-add">Agregar expresión</button>` : ""}`;
  }

  function renderWorkflow(can) {
    return `
      <p style="font-size:.75rem;color:var(--cs-muted)">Flujo embebido en el schema (notificaciones/escalamientos en runtime).</p>
      ${(studio.schema.workflow?.steps || []).map((s, i) => `
        <label>${esc(s.type)}<input data-flow-label="${i}" value="${esc(s.label)}" ${can ? "" : "disabled"}></label>
      `).join("")}
      ${can ? `<button class="cs-btn primary" type="button" id="flow-apply">Aplicar workflow</button>` : ""}`;
  }

  function wireStudio(host) {
    host.querySelector("#cs-back")?.addEventListener("click", () => { studio.mode = "home"; paintHome(); });
    host.querySelector("#cs-undo")?.addEventListener("click", undo);
    host.querySelector("#cs-redo")?.addEventListener("click", redo);
    host.querySelector("#cs-zoom-in")?.addEventListener("click", () => { studio.zoom = Math.min(1.5, studio.zoom + 0.1); paintStudio(); });
    host.querySelector("#cs-zoom-out")?.addEventListener("click", () => { studio.zoom = Math.max(0.7, studio.zoom - 0.1); paintStudio(); });
    host.querySelector("#cs-theme")?.addEventListener("click", () => {
      studio.theme = studio.theme === "dark" ? "light" : "dark";
      localStorage.setItem("cs-theme", studio.theme);
      paintStudio();
    });
    host.querySelector("#cs-save")?.addEventListener("click", () => saveDraft(false));
    host.querySelector("#cs-publish")?.addEventListener("click", publishTemplate);
    host.querySelector("#cs-dup")?.addEventListener("click", duplicateTemplate);
    host.querySelectorAll("[data-preview]").forEach((btn) => btn.addEventListener("click", () => {
      studio.previewMode = btn.dataset.preview;
      paintStudio();
    }));
    host.querySelector("#cs-palette-search")?.addEventListener("input", (e) => {
      studio.paletteFilter = e.target.value;
      paintStudio();
    });
    host.querySelectorAll("[data-tab]").forEach((btn) => btn.addEventListener("click", () => {
      studio.rightTab = btn.dataset.tab;
      paintStudio();
    }));
    host.querySelectorAll("[data-add]").forEach((btn) => {
      btn.addEventListener("click", () => addComponent(btn.dataset.add));
      btn.addEventListener("dragstart", (ev) => {
        ev.dataTransfer.setData("text/cs-type", btn.dataset.add);
        ev.dataTransfer.effectAllowed = "copy";
      });
    });
    const drop = host.querySelector("#cs-drop-zone") || host.querySelector("#cs-canvas");
    drop?.addEventListener("dragover", (ev) => { ev.preventDefault(); });
    drop?.addEventListener("drop", (ev) => {
      ev.preventDefault();
      const type = ev.dataTransfer.getData("text/cs-type");
      const nodeId = ev.dataTransfer.getData("text/cs-node");
      if (type) addComponent(type);
      else if (nodeId) reorderToEnd(nodeId);
    });
    host.querySelectorAll("[data-node]").forEach((el) => {
      el.addEventListener("click", (ev) => {
        if (ev.shiftKey) {
          if (studio.selectedIds.includes(el.dataset.node)) {
            studio.selectedIds = studio.selectedIds.filter((id) => id !== el.dataset.node);
          } else studio.selectedIds.push(el.dataset.node);
        } else studio.selectedIds = [el.dataset.node];
        studio.rightTab = "props";
        paintStudio();
      });
      el.addEventListener("dragstart", (ev) => {
        ev.dataTransfer.setData("text/cs-node", el.dataset.node);
        el.classList.add("dragging");
      });
      el.addEventListener("dragend", () => el.classList.remove("dragging"));
      el.addEventListener("dragover", (ev) => ev.preventDefault());
      el.addEventListener("drop", (ev) => {
        ev.preventDefault();
        ev.stopPropagation();
        const from = ev.dataTransfer.getData("text/cs-node");
        const to = el.dataset.node;
        if (from && to && from !== to) reorderRelative(from, to);
      });
    });
    host.querySelectorAll("[data-del]").forEach((btn) => btn.addEventListener("click", (ev) => {
      ev.stopPropagation();
      studio.selectedIds = [btn.dataset.del];
      deleteSelected();
    }));
    host.querySelectorAll("[data-dup]").forEach((btn) => btn.addEventListener("click", (ev) => {
      ev.stopPropagation();
      studio.selectedIds = [btn.dataset.dup];
      duplicateSelected();
    }));
    host.querySelectorAll("[data-restore]").forEach((btn) => btn.addEventListener("click", () => restoreVersion(btn.dataset.restore)));
    host.querySelector("#p-apply")?.addEventListener("click", applyProps);
    host.querySelector("#m-apply")?.addEventListener("click", saveHeader);
    host.querySelector("#m-archive")?.addEventListener("click", archiveTemplate);
    host.querySelector("#r-add")?.addEventListener("click", addRule);
    host.querySelectorAll("[data-rule-del]").forEach((btn) => btn.addEventListener("click", () => {
      pushHistory();
      studio.schema.rules.splice(Number(btn.dataset.ruleDel), 1);
      markDirty();
      paintStudio();
    }));
    host.querySelector("#e-add")?.addEventListener("click", addExpression);
    host.querySelectorAll("[data-expr-del]").forEach((btn) => btn.addEventListener("click", () => {
      pushHistory();
      studio.schema.expressions.splice(Number(btn.dataset.exprDel), 1);
      markDirty();
      paintStudio();
    }));
    host.querySelector("#flow-apply")?.addEventListener("click", () => {
      pushHistory();
      host.querySelectorAll("[data-flow-label]").forEach((input) => {
        const i = Number(input.dataset.flowLabel);
        if (studio.schema.workflow.steps[i]) studio.schema.workflow.steps[i].label = input.value;
      });
      markDirty();
      toast(t("Studio.WorkflowActualizadoEnSchema"), "success");
    });
  }

  function addComponent(type) {
    if (!studio.canManage) return;
    pushHistory();
    const c = defaultComponent(type);
    studio.schema.components.push(c);
    studio.selectedIds = [c.id];
    studio.rightTab = "props";
    markDirty();
    paintStudio();
  }

  function reorderRelative(fromId, toId) {
    pushHistory();
    const arr = [...studio.schema.components].sort((a, b) => (a.order || 0) - (b.order || 0));
    const from = arr.findIndex((c) => c.id === fromId);
    const to = arr.findIndex((c) => c.id === toId);
    if (from < 0 || to < 0) return;
    const [item] = arr.splice(from, 1);
    arr.splice(to, 0, item);
    arr.forEach((c, idx) => { c.order = idx; });
    studio.schema.components = arr;
    markDirty();
    paintStudio();
  }

  function reorderToEnd(nodeId) {
    pushHistory();
    const arr = [...studio.schema.components].sort((a, b) => (a.order || 0) - (b.order || 0));
    const from = arr.findIndex((c) => c.id === nodeId);
    if (from < 0) return;
    const [item] = arr.splice(from, 1);
    arr.push(item);
    arr.forEach((c, idx) => { c.order = idx; });
    studio.schema.components = arr;
    markDirty();
    paintStudio();
  }

  function deleteSelected() {
    if (!studio.canManage || !studio.selectedIds.length) return;
    pushHistory();
    studio.schema.components = studio.schema.components.filter((c) => !studio.selectedIds.includes(c.id));
    studio.selectedIds = [];
    markDirty();
    paintStudio();
  }

  function duplicateSelected() {
    if (!studio.canManage || !studio.selectedIds.length) return;
    pushHistory();
    const clones = [];
    selectedComponents().forEach((c) => {
      const copy = { ...JSON.parse(JSON.stringify(c)), id: uid("c"), name: `${c.name}_copy`, order: studio.schema.components.length + clones.length };
      clones.push(copy);
    });
    studio.schema.components.push(...clones);
    studio.selectedIds = clones.map((c) => c.id);
    markDirty();
    paintStudio();
  }

  function copySelected() {
    studio.clipboard = selectedComponents().map((c) => JSON.parse(JSON.stringify(c)));
  }

  function pasteClipboard() {
    if (!studio.canManage || !studio.clipboard.length) return;
    pushHistory();
    const clones = studio.clipboard.map((c) => ({ ...JSON.parse(JSON.stringify(c)), id: uid("c"), name: `${c.name}_paste`, order: studio.schema.components.length }));
    clones.forEach((c, i) => { c.order = studio.schema.components.length + i; });
    studio.schema.components.push(...clones);
    studio.selectedIds = clones.map((c) => c.id);
    markDirty();
    paintStudio();
  }

  function applyProps() {
    const c = selectedComponents()[0];
    if (!c) return;
    pushHistory();
    c.label = document.getElementById("p-label").value;
    c.name = document.getElementById("p-name").value;
    c.description = document.getElementById("p-desc").value;
    c.placeholder = document.getElementById("p-ph").value;
    c.tooltip = document.getElementById("p-tip").value;
    c.defaultValue = document.getElementById("p-def").value;
    c.pattern = document.getElementById("p-pat").value;
    c.minLength = valOrNull("p-min");
    c.maxLength = valOrNull("p-max");
    c.colSpan = Math.min(12, Math.max(1, Number(document.getElementById("p-col").value || 12)));
    c.width = c.colSpan <= 4 ? "third" : c.colSpan <= 6 ? "half" : "full";
    c.alignment = document.getElementById("p-align").value;
    c.color = document.getElementById("p-color").value;
    c.icon = document.getElementById("p-icon").value;
    c.expression = document.getElementById("p-expr").value;
    c.options = document.getElementById("p-opt").value.split(",").map((s) => s.trim()).filter(Boolean);
    c.permissions = { ...(c.permissions || {}), claim: document.getElementById("p-claim").value };
    c.required = document.getElementById("p-req").checked;
    c.readOnly = document.getElementById("p-ro").checked;
    c.visible = document.getElementById("p-vis").checked;
    c.editable = document.getElementById("p-ed").checked;
    c.audit = document.getElementById("p-aud").checked;
    markDirty();
    paintStudio();
  }

  function valOrNull(id) {
    const v = document.getElementById(id)?.value;
    return v === "" || v == null ? null : Number(v);
  }

  function addRule() {
    pushHistory();
    const elseAct = document.getElementById("r-else").value;
    studio.schema.rules.push({
      id: uid("r"),
      name: document.getElementById("r-name").value || "Regla",
      logic: document.getElementById("r-logic").value,
      conditions: [{
        fieldId: document.getElementById("r-field").value,
        operator: document.getElementById("r-op").value,
        value: document.getElementById("r-val").value
      }],
      elseConditions: [],
      actions: [{ type: document.getElementById("r-act").value, targetId: document.getElementById("r-target").value }],
      elseActions: elseAct ? [{ type: elseAct, targetId: document.getElementById("r-target").value }] : []
    });
    markDirty();
    paintStudio();
  }

  function addExpression() {
    pushHistory();
    studio.schema.expressions.push({
      id: uid("e"),
      name: document.getElementById("e-name").value || "Expresión",
      formula: document.getElementById("e-formula").value || "NOW()",
      targetComponentId: document.getElementById("e-target").value || null
    });
    markDirty();
    paintStudio();
  }

  async function saveHeader() {
    const detail = await request(`/tenants/${state.tenantId}/form-templates/${studio.current.id}/header`, {
      method: "PUT",
      loadingContext: "save",
      body: {
        name: document.getElementById("m-name").value,
        category: document.getElementById("m-cat").value,
        kind: Number(document.getElementById("m-kind").value || 0),
        description: document.getElementById("m-desc").value
      }
    });
    studio.current = detail;
    toast(t("Studio.CabeceraActualizada"), "success");
    paintStudio();
  }

  function updateChromeStatus() {
    const el = document.getElementById("cs-dirty");
    if (el) {
      el.textContent = studio.dirty ? "Cambios sin guardar" : t("Studio.Sincronizado");
      el.className = `cs-pill ${studio.dirty ? "warn" : "ok"}`;
    }
  }

  async function saveDraft(silent) {
    if (!studio.current || !studio.canManage || studio.saving) return;
    studio.saving = true;
    try {
      const detail = await request(`/tenants/${state.tenantId}/form-templates/${studio.current.id}/draft`, {
        method: "PUT",
        loadingContext: silent ? undefined : "save",
        body: {
          versionId: studio.current.workingVersion?.id,
          schemaJson: serializeSchema(),
          changeLog: silent ? "Autosave Compliance Studio" : t("Studio.GuardadoManualComplianceStudio")
        }
      });
      studio.current = detail;
      studio.schema = parseSchema(detail.workingVersion?.schemaJson);
      studio.dirty = false;
      studio.lastSavedAt = Date.now();
      if (!silent) toast(t("Studio.BorradorGuardado"), "success");
      updateChromeStatus();
      const clock = document.getElementById("cs-save-clock");
      if (clock) clock.textContent = `Autosave ${new Date(studio.lastSavedAt).toLocaleTimeString()}`;
    } finally {
      studio.saving = false;
    }
  }

  async function publishTemplate() {
    if (studio.dirty) await saveDraft(true);
    const detail = await request(`/tenants/${state.tenantId}/form-templates/${studio.current.id}/publish`, {
      method: "POST",
      loadingContext: "save",
      body: { versionId: studio.current.workingVersion?.id }
    });
    studio.current = detail;
    studio.dirty = false;
    toast(t("Studio.PublicadaDisponibleViaFormEngineParaAuditsCAPARiskEInspection"), "success");
    studio.list = (await request(`/tenants/${state.tenantId}/form-templates`)) || [];
    paintStudio();
  }

  async function duplicateTemplate() {
    const name = prompt(t("Studio.NombreDeLaCopia"), `${studio.current.name} (copia)`);
    if (!name) return;
    const code = prompt(t("Studio.CodigoNuevo"), `${studio.current.code}-COPY`);
    if (!code) return;
    const detail = await request(`/tenants/${state.tenantId}/form-templates/${studio.current.id}/duplicate`, {
      method: "POST",
      loadingContext: "save",
      body: { newName: name, newCode: code }
    });
    toast(t("Studio.PlantillaDuplicada"), "success");
    studio.list = (await request(`/tenants/${state.tenantId}/form-templates`)) || [];
    if (detail?.id) await openTemplate(detail.id);
  }

  async function restoreVersion(versionId) {
    if (!confirm("¿Restaurar esta versión como nuevo borrador de trabajo?")) return;
    const detail = await request(`/tenants/${state.tenantId}/form-templates/${studio.current.id}/versions/${versionId}/restore`, {
      method: "POST",
      loadingContext: "save",
      body: {}
    });
    studio.current = detail;
    studio.schema = parseSchema(detail.workingVersion?.schemaJson);
    studio.dirty = false;
    toast(t("Studio.VersionRestaurada"), "success");
    paintStudio();
  }

  async function archiveTemplate() {
    if (!confirm("¿Archivar esta plantilla?")) return;
    await request(`/tenants/${state.tenantId}/form-templates/${studio.current.id}/archive`, { method: "POST", loadingContext: "save", body: {} });
    toast(t("Studio.Archivada"), "success");
    studio.list = (await request(`/tenants/${state.tenantId}/form-templates`)) || [];
    paintHome();
  }

  /** Integration helper for other modules (Audits, CAPA, Risk, …). */
  async function getPublishedTemplates(kind) {
    const q = kind == null ? "" : `?kind=${encodeURIComponent(kind)}`;
    return (await request(`/tenants/${state.tenantId}/form-templates/published${q}`)) || [];
  }

  global.renderFormTemplateBuilder = renderFormTemplateBuilder;
  global.ComplianceStudio = {
    getPublishedTemplates,
    componentLibrary: COMPONENT_LIBRARY,
    parseSchema,
    unmount: unmountHost
  };
  } catch (err) {
    console.error(t("Studio.ComplianceStudioFailedToInitialize"), err);
    global.renderFormTemplateBuilder = async function (content) {
      content.innerHTML = `<section class="error-state rich"><strong>Compliance Studio no pudo inicializar.</strong><p>${String(err && err.message || err)}</p></section>`;
    };
  }
})(window);
