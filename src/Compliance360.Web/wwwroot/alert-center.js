(() => {
  const view = {
    page: 1,
    pageSize: 25,
    state: "",
    favorite: "",
    search: "",
    selected: new Set()
  };

  function tokenPayload() {
    try {
      const token = localStorage.getItem("c360.token") || "";
      const segment = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
      return JSON.parse(atob(segment));
    } catch {
      return {};
    }
  }

  function tenantId() {
    const payload = tokenPayload();
    return payload.tenant_id || payload.TenantId || payload.tenantId;
  }

  function escapeHtml(value) {
    return String(value ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#039;");
  }

  async function api(path, options = {}) {
    const token = localStorage.getItem("c360.token");
    const response = await fetch(`/api/v2/tenants/${tenantId()}/alert-center${path}`, {
      method: options.method || "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`
      },
      body: options.body ? JSON.stringify(options.body) : undefined
    });
    if (!response.ok) {
      const text = await response.text();
      let message = "No se pudo completar la operación.";
      try {
        const payload = JSON.parse(text);
        message = payload.detail || payload.title || payload.error || message;
      } catch {
        if (text && text.length < 200) message = text;
      }
      throw new Error(message);
    }
    return response.status === 204 ? {} : response.json();
  }

  function queryString() {
    const parameters = new URLSearchParams({
      page: String(view.page),
      pageSize: String(view.pageSize)
    });
    if (view.state) parameters.set("state", view.state);
    if (view.favorite) parameters.set("favorite", view.favorite);
    if (view.search) parameters.set("search", view.search);
    return parameters.toString();
  }

  function priorityLabel(priority) {
    const labels = { Low: "Baja", Normal: "Normal", High: "Alta", Critical: "Crítica" };
    return labels[priority] || priority;
  }

  function stateLabel(state) {
    const labels = { Unread: "No leída", Read: "Leída", Archived: "Archivada", Deleted: "Eliminada" };
    return labels[state] || state;
  }

  function can(permission) {
    return typeof window.hasPermission === "function" && window.hasPermission(permission);
  }

  function formatDate(value) {
    if (!value) return "—";
    return new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" }).format(new Date(value));
  }

  function inboxItem(item) {
    return `
      <article class="alert-inbox-item ${item.state === "Unread" ? "unread" : ""}" data-inbox-id="${escapeHtml(item.id)}">
        <label class="alert-inbox-select">
          <input type="checkbox" data-select-inbox="${escapeHtml(item.id)}" ${view.selected.has(item.id) ? "checked" : ""} />
          <span class="sr-only">Seleccionar ${escapeHtml(item.subject)}</span>
        </label>
        <button class="alert-inbox-main" type="button" data-inbox-action="MarkRead" data-inbox-id="${escapeHtml(item.id)}">
          <span class="alert-inbox-title">
            <strong>${escapeHtml(item.subject)}</strong>
            <span class="status-pill ${item.priority === "Critical" ? "danger" : item.priority === "High" ? "warn" : ""}">${escapeHtml(priorityLabel(item.priority))}</span>
          </span>
          <span class="alert-inbox-preview">${escapeHtml(item.body)}</span>
          <span class="alert-inbox-meta">${escapeHtml(item.channel)} · ${escapeHtml(stateLabel(item.state))} · ${escapeHtml(formatDate(item.receivedAtUtc))}</span>
        </button>
        <div class="alert-inbox-actions">
          <button class="btn subtle small" type="button" data-inbox-action="${item.isFavorite ? "Unfavorite" : "Favorite"}" data-inbox-id="${escapeHtml(item.id)}" aria-label="${item.isFavorite ? "Quitar favorito" : "Marcar favorito"}">${item.isFavorite ? "★" : "☆"}</button>
          <button class="btn subtle small" type="button" data-inbox-action="${item.state === "Archived" ? "MarkRead" : "Archive"}" data-inbox-id="${escapeHtml(item.id)}">${item.state === "Archived" ? "Restaurar" : "Archivar"}</button>
        </div>
      </article>`;
  }

  async function renderAlertCenter(content) {
    content.innerHTML = `
      <section class="module-page alert-center-page" aria-busy="true">
        <div class="page-heading">
          <div><span class="eyebrow">Enterprise Alert Center</span><h1>Inbox</h1><p>Cargando notificaciones persistentes…</p></div>
        </div>
        <div class="skeleton-grid">${"<span class=\"skeleton-card\"></span>".repeat(4)}</div>
      </section>`;

    const [counts, page] = await Promise.all([
      api("/inbox/counts"),
      api(`/inbox?${queryString()}`)
    ]);

    const totalPages = Math.max(1, Math.ceil(page.total / page.pageSize));
    content.innerHTML = `
      <section class="module-page alert-center-page">
        <div class="page-heading alert-center-heading">
          <div>
            <span class="eyebrow">Enterprise Alert Center</span>
            <h1>Inbox de notificaciones</h1>
            <p>Alertas regulatorias persistentes, aisladas por tenant y usuario.</p>
          </div>
          <div class="button-row">
            ${can("NOTIFICATION.TEMPLATE") || can("NOTIFICATION.ADMIN") ? `<button id="alert-open-templates" class="btn" type="button">Template Center</button>` : ""}
            ${can("NOTIFICATION.MANAGE") ? `<button id="alert-open-rules" class="btn" type="button">Reglas</button>` : ""}
            ${can("NOTIFICATION.MANAGE") ? `<button id="alert-open-operations" class="btn" type="button">Operaciones</button>` : ""}
            ${can("NOTIFICATION.ADMIN") ? `<button id="alert-open-providers" class="btn" type="button">Proveedores</button>` : ""}
            ${can("NOTIFICATION.MANAGE") ? `<button id="alert-open-recipients" class="btn" type="button">Destinatarios</button>` : ""}
            <button id="alert-refresh" class="btn" type="button">Actualizar</button>
            <button id="alert-mark-selected" class="btn" type="button" ${view.selected.size ? "" : "disabled"}>Marcar selección leída</button>
            <button id="alert-mark-all" class="btn primary" type="button" ${counts.unread ? "" : "disabled"}>Marcar todas leídas</button>
          </div>
        </div>

        <div class="metric-grid alert-center-metrics">
          <article class="metric-card"><span>No leídas</span><strong>${counts.unread}</strong></article>
          <article class="metric-card"><span>Leídas</span><strong>${counts.read}</strong></article>
          <article class="metric-card"><span>Archivadas</span><strong>${counts.archived}</strong></article>
          <article class="metric-card"><span>Favoritas</span><strong>${counts.favorites}</strong></article>
        </div>

        <form id="alert-inbox-filters" class="alert-inbox-filters">
          <label>Buscar<input name="search" type="search" value="${escapeHtml(view.search)}" placeholder="Asunto o contenido" /></label>
          <label>Estado<select name="state">
            <option value="">Activas</option>
            ${["Unread", "Read", "Archived", "Deleted"].map(value => `<option value="${value}" ${view.state === value ? "selected" : ""}>${stateLabel(value)}</option>`).join("")}
          </select></label>
          <label>Favoritos<select name="favorite">
            <option value="">Todos</option>
            <option value="true" ${view.favorite === "true" ? "selected" : ""}>Solo favoritos</option>
            <option value="false" ${view.favorite === "false" ? "selected" : ""}>No favoritos</option>
          </select></label>
          <label>Por página<select name="pageSize">
            ${[10, 25, 50, 100].map(value => `<option value="${value}" ${view.pageSize === value ? "selected" : ""}>${value}</option>`).join("")}
          </select></label>
          <button class="btn primary" type="submit">Aplicar filtros</button>
        </form>

        <div class="alert-inbox-toolbar">
          <label><input id="alert-select-page" type="checkbox" /> Seleccionar página</label>
          <span>${page.total} notificaciones · página ${page.page} de ${totalPages}</span>
        </div>

        <div class="alert-inbox-list">
          ${page.items.length
            ? page.items.map(inboxItem).join("")
            : `<div class="empty-state"><h2>Sin notificaciones</h2><p>No hay elementos que coincidan con los filtros seleccionados.</p></div>`}
        </div>

        <nav class="alert-pagination" aria-label="Paginación del inbox">
          <button id="alert-previous" class="btn" type="button" ${page.page <= 1 ? "disabled" : ""}>Anterior</button>
          <span>Página ${page.page} de ${totalPages}</span>
          <button id="alert-next" class="btn" type="button" ${page.page >= totalPages ? "disabled" : ""}>Siguiente</button>
        </nav>
      </section>`;

    bindInbox(content, page);
  }

  function bindInbox(content, page) {
    content.querySelector("#alert-open-templates")?.addEventListener("click", () => renderTemplateCenter(content));
    content.querySelector("#alert-open-rules")?.addEventListener("click", () => renderRuleCenter(content));
    content.querySelector("#alert-open-operations")?.addEventListener("click", () => renderOperationsConsole(content));
    content.querySelector("#alert-open-providers")?.addEventListener("click", () => renderProviderCenter(content));
    content.querySelector("#alert-open-recipients")?.addEventListener("click", () => renderRecipientCenter(content));
    content.querySelector("#alert-refresh")?.addEventListener("click", () => renderAlertCenter(content));
    content.querySelector("#alert-inbox-filters")?.addEventListener("submit", event => {
      event.preventDefault();
      const form = new FormData(event.currentTarget);
      view.search = String(form.get("search") || "").trim();
      view.state = String(form.get("state") || "");
      view.favorite = String(form.get("favorite") || "");
      view.pageSize = Number(form.get("pageSize") || 25);
      view.page = 1;
      view.selected.clear();
      renderAlertCenter(content);
    });
    content.querySelector("#alert-previous")?.addEventListener("click", () => { view.page--; renderAlertCenter(content); });
    content.querySelector("#alert-next")?.addEventListener("click", () => { view.page++; renderAlertCenter(content); });
    content.querySelector("#alert-select-page")?.addEventListener("change", event => {
      page.items.forEach(item => event.currentTarget.checked ? view.selected.add(item.id) : view.selected.delete(item.id));
      renderAlertCenter(content);
    });
    content.querySelectorAll("[data-select-inbox]").forEach(input => input.addEventListener("change", event => {
      const id = event.currentTarget.dataset.selectInbox;
      event.currentTarget.checked ? view.selected.add(id) : view.selected.delete(id);
      const button = content.querySelector("#alert-mark-selected");
      if (button) button.disabled = view.selected.size === 0;
    }));
    content.querySelectorAll("[data-inbox-action]").forEach(button => button.addEventListener("click", async event => {
      const target = event.currentTarget;
      target.disabled = true;
      try {
        await api(`/inbox/${target.dataset.inboxId}/actions`, {
          method: "POST",
          body: { action: target.dataset.inboxAction }
        });
        await renderAlertCenter(content);
      } catch (error) {
        target.disabled = false;
        window.alert(error.message);
      }
    }));
    content.querySelector("#alert-mark-selected")?.addEventListener("click", () => bulkMarkRead(content, false));
    content.querySelector("#alert-mark-all")?.addEventListener("click", () => bulkMarkRead(content, true));
  }

  async function bulkMarkRead(content, all) {
    if (!all && view.selected.size === 0) return;
    await api("/inbox/actions/bulk", {
      method: "POST",
      body: {
        inboxItemIds: all ? [] : Array.from(view.selected),
        action: "MarkRead",
        all
      }
    });
    view.selected.clear();
    await renderAlertCenter(content);
  }

  async function renderTemplateCenter(content) {
    content.innerHTML = `<section class="module-page alert-center-page"><div class="page-heading"><div><span class="eyebrow">Alert Center</span><h1>Template Center</h1><p>Cargando versiones…</p></div></div></section>`;
    const page = await api("/templates?page=1&pageSize=100");
    const templateOptions = Array.from(new Map(page.items.map(item => [item.templateId, item])).values());
    content.innerHTML = `
      <section class="module-page alert-center-page">
        <div class="page-heading">
          <div><span class="eyebrow">Alert Center · Configuración funcional</span><h1>Template Center</h1><p>Versiones inmutables, sanitización HTML y aprobación maker-checker.</p></div>
          <div class="button-row"><button id="template-back-inbox" class="btn" type="button">Inbox</button>${can("NOTIFICATION.MANAGE") ? `<button id="template-open-rules" class="btn" type="button">Reglas</button>` : ""}<button id="template-new-version" class="btn primary" type="button">${templateOptions.length ? "Nueva versión" : "Nueva plantilla"}</button></div>
        </div>
        <div class="metric-grid">
          <article class="metric-card"><span>Versiones</span><strong>${page.total}</strong></article>
          <article class="metric-card"><span>Publicadas</span><strong>${page.items.filter(item => item.lifecycle === "Published").length}</strong></article>
          <article class="metric-card"><span>En revisión</span><strong>${page.items.filter(item => item.lifecycle === "Review").length}</strong></article>
          <article class="metric-card"><span>Borradores</span><strong>${page.items.filter(item => item.lifecycle === "Draft").length}</strong></article>
        </div>
        <div class="table-wrap">
          <table class="enterprise-table">
            <thead><tr><th>Código</th><th>Canal</th><th>Locale</th><th>Versión</th><th>Asunto</th><th>Estado</th><th>Acciones</th></tr></thead>
            <tbody>${page.items.map(item => `
              <tr>
                <td><strong>${escapeHtml(item.code)}</strong></td><td>${escapeHtml(item.channel)}</td><td>${escapeHtml(item.locale)}</td>
                <td>v${item.version}</td><td>${escapeHtml(item.subject)}</td><td><span class="status-pill">${escapeHtml(item.lifecycle)}</span></td>
                <td><div class="button-row compact">
                  <button class="btn subtle small" data-template-preview="${item.versionId}" type="button">Preview</button>
                  ${lifecycleButton(item)}
                  <button class="btn subtle small" data-template-duplicate="${item.versionId}" data-template-locale="${escapeHtml(item.locale)}" type="button">Duplicar</button>
                </div></td>
              </tr>`).join("") || `<tr><td colspan="7"><div class="empty-state"><h2>Sin versiones</h2><p>Cree primero una plantilla base desde Notification Administration.</p></div></td></tr>`}</tbody>
          </table>
        </div>
        <dialog id="template-editor-dialog" class="enterprise-dialog"></dialog>
        <dialog id="template-preview-dialog" class="enterprise-dialog"></dialog>
      </section>`;

    bindTemplateCenter(content, templateOptions);
  }

  function lifecycleButton(item) {
    const next = {
      Draft: ["SubmitForReview", "Enviar a revisión"],
      Review: item.isReviewed ? ["Approve", "Aprobar"] : ["RecordReview", "Registrar revisión"],
      Approved: ["Publish", "Publicar"],
      Published: ["Retire", "Retirar"],
      Retired: ["Archive", "Archivar"]
    }[item.lifecycle];
    return next ? `<button class="btn small" data-template-action="${next[0]}" data-template-version="${item.versionId}" type="button">${next[1]}</button>` : "";
  }

  function bindTemplateCenter(content, templateOptions) {
    content.querySelector("#template-back-inbox")?.addEventListener("click", () => renderAlertCenter(content));
    content.querySelector("#template-open-rules")?.addEventListener("click", () => renderRuleCenter(content));
    content.querySelector("#template-new-version")?.addEventListener("click", () => openTemplateEditor(content, templateOptions));
    content.querySelectorAll("[data-template-action]").forEach(button => button.addEventListener("click", async event => {
      const target = event.currentTarget;
      target.disabled = true;
      try {
        await api(`/template-versions/${target.dataset.templateVersion}/actions`, { method: "POST", body: { action: target.dataset.templateAction } });
        await renderTemplateCenter(content);
      } catch (error) {
        target.disabled = false;
        window.alert(error.message);
      }
    }));
    content.querySelectorAll("[data-template-duplicate]").forEach(button => button.addEventListener("click", async event => {
      const target = event.currentTarget;
      await api(`/template-versions/${target.dataset.templateDuplicate}/duplicate`, { method: "POST", body: { locale: target.dataset.templateLocale } });
      await renderTemplateCenter(content);
    }));
    content.querySelectorAll("[data-template-preview]").forEach(button => button.addEventListener("click", event => previewTemplate(content, event.currentTarget.dataset.templatePreview)));
  }

  function openTemplateEditor(content, templateOptions) {
    const dialog = content.querySelector("#template-editor-dialog");
    dialog.innerHTML = `
      <form id="template-version-form" class="template-editor" method="dialog">
        <div class="page-heading"><div><span class="eyebrow">Nueva versión inmutable</span><h2>Editor de plantilla</h2></div><button class="btn subtle" value="cancel">Cerrar</button></div>
        <div class="grid two">
          <label>Plantilla base<select name="templateId"><option value="">Crear nueva plantilla base</option>${templateOptions.map(item => `<option value="${item.templateId}">${escapeHtml(item.code)} · ${escapeHtml(item.channel)}</option>`).join("")}</select></label>
          <label>Locale<input name="locale" value="es-PA" maxlength="20" required /></label>
        </div>
        <div class="grid two">
          <label>Código (para plantilla nueva)<input name="code" maxlength="120" placeholder="DOSSIER_EXPIRING" /></label>
          <label>Canal (para plantilla nueva)<select name="channel"><option value="Email">Email</option><option value="InApp">In-app</option><option value="Sms">SMS</option><option value="WhatsApp">WhatsApp</option><option value="Push">Push</option></select></label>
        </div>
        <label>Asunto<input name="subject" maxlength="250" required placeholder="Vencimiento de {{Document.Name}}" /></label>
        <div class="template-editor-tabs"><button id="template-visual-tab" class="btn small" type="button">Editor visual</button><button id="template-html-tab" class="btn subtle small" type="button">HTML</button></div>
        <div id="template-visual-editor" class="template-visual-editor" contenteditable="true" role="textbox" aria-label="Contenido visual"><p>Escriba el contenido de la notificación.</p></div>
        <textarea id="template-html-editor" name="htmlBody" rows="14" hidden required></textarea>
        <label>Texto plano<textarea name="textBody" rows="5"></textarea></label>
        <p class="field-hint">Variables: use tokens como {{TenantName}}, {{Dossier.Code}} o {{User.Name}}. El HTML activo se elimina al guardar.</p>
        <div class="button-row"><button id="template-save-version" class="btn primary" type="submit">Guardar borrador</button></div>
      </form>`;
    const visual = dialog.querySelector("#template-visual-editor");
    const html = dialog.querySelector("#template-html-editor");
    dialog.querySelector("#template-visual-tab").addEventListener("click", () => { html.hidden = true; visual.hidden = false; visual.innerHTML = html.value || visual.innerHTML; });
    dialog.querySelector("#template-html-tab").addEventListener("click", () => { html.value = visual.innerHTML; visual.hidden = true; html.hidden = false; });
    dialog.querySelector("#template-version-form").addEventListener("submit", async event => {
      event.preventDefault();
      const form = new FormData(event.currentTarget);
      html.value = visual.hidden ? html.value : visual.innerHTML;
      const templateId = String(form.get("templateId") || "");
      const endpoint = templateId ? `/templates/${templateId}/versions` : "/templates";
      const payload = {
        locale: String(form.get("locale")),
        subject: String(form.get("subject")),
        htmlBody: html.value,
        textBody: String(form.get("textBody") || "") || null,
        brandingJson: null
      };
      if (!templateId) {
        payload.code = String(form.get("code") || "").trim();
        payload.channel = String(form.get("channel"));
        if (!payload.code) {
          throw new Error("El código es obligatorio para crear una plantilla base.");
        }
      }
      await api(endpoint, {
        method: "POST",
        body: payload
      });
      dialog.close();
      await renderTemplateCenter(content);
    });
    dialog.showModal();
  }

  async function previewTemplate(content, versionId) {
    const detail = await api(`/template-versions/${versionId}`);
    const values = Object.fromEntries((detail.variables || []).map(variable => [variable, `[${variable}]`]));
    const preview = await api(`/template-versions/${versionId}/preview`, { method: "POST", body: { variables: values, branding: null } });
    const dialog = content.querySelector("#template-preview-dialog");
    dialog.innerHTML = `
      <div class="template-preview">
        <div class="page-heading"><div><span class="eyebrow">Preview sanitizado</span><h2>${escapeHtml(preview.subject)}</h2></div><button id="template-preview-close" class="btn">Cerrar</button></div>
        <iframe title="Preview de plantilla" sandbox="" srcdoc="${escapeHtml(preview.htmlBody)}"></iframe>
        <details><summary>Texto plano</summary><pre>${escapeHtml(preview.textBody || "")}</pre></details>
      </div>`;
    dialog.querySelector("#template-preview-close").addEventListener("click", () => dialog.close());
    dialog.showModal();
  }

  async function renderRuleCenter(content) {
    content.innerHTML = `<section class="module-page alert-center-page"><div class="page-heading"><div><span class="eyebrow">Alert Center</span><h1>Motor de reglas</h1><p>Cargando catálogo administrable…</p></div></div></section>`;
    const [rules, eventTypes, templates] = await Promise.all([
      api("/rules?page=1&pageSize=100"),
      api("/event-types"),
      api("/templates?page=1&pageSize=100")
    ]);
    content.innerHTML = `
      <section class="module-page alert-center-page">
        <div class="page-heading">
          <div><span class="eyebrow">Alert Center · Automatización funcional</span><h1>Reglas de alerta</h1><p>Condiciones versionadas, deduplicación, silencio, SLA y aprobación maker-checker.</p></div>
          <div class="button-row"><button id="rules-back-inbox" class="btn" type="button">Inbox</button><button id="rules-open-templates" class="btn" type="button">Plantillas</button><button id="rules-open-schedules" class="btn" type="button">Programaciones</button><button id="rules-new" class="btn primary" type="button">Crear regla</button></div>
        </div>
        <div class="metric-grid">
          <article class="metric-card"><span>Reglas</span><strong>${rules.total}</strong></article>
          <article class="metric-card"><span>Publicadas</span><strong>${rules.items.filter(item => item.lifecycle === "Published").length}</strong></article>
          <article class="metric-card"><span>Deshabilitadas</span><strong>${rules.items.filter(item => item.lifecycle === "Disabled").length}</strong></article>
          <article class="metric-card"><span>Eventos disponibles</span><strong>${eventTypes.filter(item => item.isActive).length}</strong></article>
        </div>
        <div class="table-wrap">
          <table class="enterprise-table">
            <thead><tr><th>Código</th><th>Nombre</th><th>Evento</th><th>Prioridad</th><th>Estado</th><th>Versión</th><th>Acciones</th></tr></thead>
            <tbody>${rules.items.map(item => {
              const eventType = eventTypes.find(event => event.id === item.eventTypeId);
              return `<tr>
                <td><strong>${escapeHtml(item.code)}</strong></td><td>${escapeHtml(item.name)}</td><td>${escapeHtml(eventType?.name || item.eventTypeId)}</td>
                <td>${escapeHtml(priorityLabel(item.priority))}</td><td><span class="status-pill">${escapeHtml(item.lifecycle)}</span></td><td>${item.currentPublishedVersionId ? "Publicada" : "Borrador"}</td>
                <td><button class="btn subtle small" type="button" data-rule-detail="${item.id}">Ver versiones</button></td>
              </tr>`;
            }).join("") || `<tr><td colspan="7"><div class="empty-state"><h2>Sin reglas</h2><p>Use el wizard para crear y simular la primera regla.</p></div></td></tr>`}</tbody>
          </table>
        </div>
        <dialog id="rule-wizard-dialog" class="enterprise-dialog"></dialog>
        <dialog id="rule-detail-dialog" class="enterprise-dialog"></dialog>
      </section>`;
    content.querySelector("#rules-back-inbox")?.addEventListener("click", () => renderAlertCenter(content));
    content.querySelector("#rules-open-templates")?.addEventListener("click", () => renderTemplateCenter(content));
    content.querySelector("#rules-open-schedules")?.addEventListener("click", () => renderScheduleCenter(content));
    content.querySelector("#rules-new")?.addEventListener("click", () => openRuleWizard(content, eventTypes, templates.items));
    content.querySelectorAll("[data-rule-detail]").forEach(button => button.addEventListener("click", () => openRuleDetail(content, button.dataset.ruleDetail)));
  }

  function openRuleWizard(content, eventTypes, templates) {
    const dialog = content.querySelector("#rule-wizard-dialog");
    const defaultCondition = JSON.stringify({
      type: "Compare",
      left: { type: "VariableReference", path: "status" },
      operator: "Equal",
      right: { type: "Constant", value: "Overdue" }
    }, null, 2);
    dialog.innerHTML = `
      <form id="rule-wizard-form" class="template-editor">
        <div class="page-heading"><div><span class="eyebrow">Wizard de alerta</span><h2>Nueva regla administrable</h2></div><button id="rule-wizard-close" class="btn subtle" type="button">Cerrar</button></div>
        <fieldset><legend>1. Evento y propósito</legend>
          <div class="grid two">
            <label>Evento<select name="eventTypeId" required>${eventTypes.filter(item => item.isActive).map(item => `<option value="${item.id}">${escapeHtml(item.module)} · ${escapeHtml(item.name)}</option>`).join("")}</select></label>
            <label>Prioridad<select name="priority"><option>Normal</option><option>Low</option><option>High</option><option>Critical</option></select></label>
            <label>Código<input name="code" maxlength="160" required placeholder="DOSSIER_OVERDUE" /></label>
            <label>Nombre<input name="name" maxlength="200" required placeholder="Dossier vencido" /></label>
          </div>
          <label>Descripción<textarea name="description" rows="2" maxlength="2000" required></textarea></label>
        </fieldset>
        <fieldset><legend>2. Condición SI</legend>
          <p class="field-hint">Use All, Any y Not para agrupaciones; Compare, Exists, IsNull e IsEmpty para condiciones.</p>
          <label>Árbol de condición<textarea name="conditionJson" rows="12" required>${escapeHtml(defaultCondition)}</textarea></label>
        </fieldset>
        <fieldset><legend>3. ENTONCES: destinatarios y canales</legend>
          <div class="grid two">
            <div>
              <div class="grid three"><label>Tipo<select id="rule-recipient-kind">${["Owner","Creator","Responsible","Reviewer","Approver","Submitter","Supervisor","Role","Group","Department","ExplicitUser","ExternalEmail","DistributionList","Subscription"].map(value => `<option>${value}</option>`).join("")}</select></label><label>Entrega<select id="rule-recipient-routing"><option>To</option><option>Cc</option><option>Bcc</option></select></label><label>ID/correo/tópico<input id="rule-recipient-value" /></label></div>
              <button id="rule-add-recipient" class="btn small" type="button">Agregar destinatario</button>
              <label>Destinatarios configurables<textarea id="rule-recipient-json" name="recipientRulesJson" rows="6" required>[{"kind":"Owner","routing":"To"}]</textarea></label>
            </div>
            <div>
              <div class="grid two"><label>Plantilla publicada<select id="rule-channel-template">${templates.filter(item => item.lifecycle === "Published").map(item => `<option value="${escapeHtml(item.code)}" data-channel="${escapeHtml(item.channel)}">${escapeHtml(item.code)} · ${escapeHtml(item.channel)} · ${escapeHtml(item.locale)}</option>`).join("")}</select></label><label>Locale<input id="rule-channel-locale" value="es-PA" /></label></div>
              <button id="rule-add-channel" class="btn small" type="button" ${templates.some(item => item.lifecycle === "Published") ? "" : "disabled"}>Agregar canal</button>
              <label>Canales y plantillas<textarea id="rule-channel-json" name="channelPoliciesJson" rows="6" required>[]</textarea></label>
            </div>
          </div>
        </fieldset>
        <fieldset><legend>4. Controles operativos</legend>
          <div class="grid three">
            <label>Clave dedupe<input name="dedupeExpression" value="{{eventType}}:{{entityId}}" maxlength="500" required /></label>
            <label>Silencio (min)<input name="silenceWindowMinutes" type="number" min="0" value="60" required /></label>
            <label>SLA (min)<input name="slaMinutes" type="number" min="1" value="1440" /></label>
          </div>
          <label>Política de dato desconocido<select name="unknownPolicy"><option>TreatAsFalse</option><option>TreatAsTrue</option><option>FailEvaluation</option></select></label>
        </fieldset>
        <fieldset><legend>5. Simulación obligatoria antes de guardar</legend>
          <label>Payload de ejemplo<textarea name="eventPayloadJson" rows="7" required>{"status":"Overdue","entityId":"00000000-0000-0000-0000-000000000001"}</textarea></label>
          <output id="rule-simulation-result" class="field-hint">Aún no simulada.</output>
        </fieldset>
        <div class="button-row"><button id="rule-simulate" class="btn" type="button">Simular</button><button id="rule-save" class="btn primary" type="submit" disabled>Guardar borrador</button></div>
      </form>`;
    let simulationPassed = false;
    const formElement = dialog.querySelector("#rule-wizard-form");
    dialog.querySelector("#rule-wizard-close").addEventListener("click", () => dialog.close());
    dialog.querySelector("#rule-add-recipient").addEventListener("click", () => {
      const textarea = dialog.querySelector("#rule-recipient-json");
      const items = JSON.parse(textarea.value || "[]");
      const value = dialog.querySelector("#rule-recipient-value").value.trim();
      items.push({
        kind: dialog.querySelector("#rule-recipient-kind").value,
        routing: dialog.querySelector("#rule-recipient-routing").value,
        value: value || null,
        respectPreferences: true
      });
      textarea.value = JSON.stringify(items, null, 2);
    });
    dialog.querySelector("#rule-add-channel").addEventListener("click", () => {
      const textarea = dialog.querySelector("#rule-channel-json");
      const items = JSON.parse(textarea.value || "[]");
      const selected = dialog.querySelector("#rule-channel-template").selectedOptions[0];
      items.push({
        channel: selected.dataset.channel,
        templateCode: selected.value,
        locale: dialog.querySelector("#rule-channel-locale").value || null,
        enabled: true
      });
      textarea.value = JSON.stringify(items, null, 2);
    });
    dialog.querySelector("#rule-simulate").addEventListener("click", async () => {
      const form = new FormData(formElement);
      try {
        const result = await api("/rules/simulate", {
          method: "POST",
          body: {
            conditionJson: String(form.get("conditionJson")),
            unknownPolicy: String(form.get("unknownPolicy")),
            eventPayloadJson: String(form.get("eventPayloadJson"))
          }
        });
        simulationPassed = true;
        dialog.querySelector("#rule-save").disabled = false;
        dialog.querySelector("#rule-simulation-result").textContent = `${result.matched ? "COINCIDE" : "NO COINCIDE"} · ${result.rawValue} · ${result.nodesEvaluated} nodos`;
      } catch (error) {
        simulationPassed = false;
        dialog.querySelector("#rule-save").disabled = true;
        dialog.querySelector("#rule-simulation-result").textContent = error.message;
      }
    });
    formElement.addEventListener("submit", async event => {
      event.preventDefault();
      if (!simulationPassed) return;
      const form = new FormData(formElement);
      await api("/rules", {
        method: "POST",
        body: {
          eventTypeId: String(form.get("eventTypeId")),
          code: String(form.get("code")),
          name: String(form.get("name")),
          description: String(form.get("description")),
          ownerUserId: null,
          priority: String(form.get("priority")),
          conditionJson: String(form.get("conditionJson")),
          recipientRulesJson: String(form.get("recipientRulesJson")),
          channelPoliciesJson: String(form.get("channelPoliciesJson")),
          dedupeExpression: String(form.get("dedupeExpression")),
          silenceWindowMinutes: Number(form.get("silenceWindowMinutes")),
          slaMinutes: Number(form.get("slaMinutes")) || null,
          unknownPolicy: String(form.get("unknownPolicy"))
        }
      });
      dialog.close();
      await renderRuleCenter(content);
    });
    dialog.showModal();
  }

  async function openRuleDetail(content, definitionId) {
    const detail = await api(`/rules/${definitionId}`);
    const dialog = content.querySelector("#rule-detail-dialog");
    dialog.innerHTML = `
      <div class="template-editor">
        <div class="page-heading"><div><span class="eyebrow">${escapeHtml(detail.definition.code)}</span><h2>${escapeHtml(detail.definition.name)}</h2><p>${escapeHtml(detail.definition.description)}</p></div><button id="rule-detail-close" class="btn">Cerrar</button></div>
        <div class="table-wrap"><table class="enterprise-table"><thead><tr><th>Versión</th><th>Estado</th><th>Silencio</th><th>SLA</th><th>Acción</th></tr></thead><tbody>
          ${detail.versions.map(version => `<tr><td>v${version.version}</td><td>${escapeHtml(version.lifecycle)}</td><td>${version.silenceWindowMinutes} min</td><td>${version.slaMinutes || "—"}</td><td><div class="button-row compact"><button class="btn subtle small" data-rule-recipient-preview="${version.id}" type="button">Vista previa destinatarios</button>${ruleLifecycleButton(detail.definition, version)}</div></td></tr>`).join("")}
        </tbody></table></div>
      </div>`;
    dialog.querySelector("#rule-detail-close").addEventListener("click", () => dialog.close());
    dialog.querySelectorAll("[data-rule-action]").forEach(button => button.addEventListener("click", async () => {
      await api(`/rules/${definitionId}/versions/${button.dataset.ruleVersion}/actions`, { method: "POST", body: { action: button.dataset.ruleAction } });
      dialog.close();
      await renderRuleCenter(content);
    }));
    dialog.querySelectorAll("[data-rule-recipient-preview]").forEach(button => button.addEventListener("click", async () => {
      try {
        const preview = await api(`/rules/${definitionId}/versions/${button.dataset.ruleRecipientPreview}/recipients/preview`, {
          method: "POST",
          body: { channel: "InApp", topic: detail.definition.code, relationshipUsers: { Owner: detail.definition.ownerUserId } }
        });
        const list = [...preview.to, ...preview.cc, ...preview.bcc].map(item => `${item.routing}: ${item.displayName} <${item.address}>`).join("\n");
        window.alert(list || `Sin destinatarios.\n${preview.warnings.join("\n")}`);
      } catch (error) {
        window.alert(error.message);
      }
    }));
    dialog.showModal();
  }

  function ruleLifecycleButton(definition, version) {
    const action = {
      Draft: ["submit", "Enviar a revisión"],
      Review: version.reviewedByUserId ? ["approve", "Aprobar"] : ["review", "Registrar revisión"],
      Approved: ["publish", "Publicar"]
    }[version.lifecycle];
    if (action) return `<button class="btn small" data-rule-action="${action[0]}" data-rule-version="${version.id}" type="button">${action[1]}</button>`;
    if (version.lifecycle === "Published" && definition.lifecycle === "Published") return `<button class="btn small" data-rule-action="disable" data-rule-version="${version.id}" type="button">Deshabilitar</button>`;
    if (version.lifecycle === "Published" && definition.lifecycle === "Disabled") return `<button class="btn small" data-rule-action="enable" data-rule-version="${version.id}" type="button">Activar</button>`;
    return "—";
  }

  async function renderScheduleCenter(content) {
    content.innerHTML = `<section class="module-page alert-center-page"><div class="page-heading"><div><span class="eyebrow">Alert Center</span><h1>Scheduler</h1><p>Cargando programaciones durables…</p></div></div></section>`;
    const [schedules, rules] = await Promise.all([
      api("/schedules"),
      api("/rules?lifecycle=Published&page=1&pageSize=100")
    ]);
    content.innerHTML = `
      <section class="module-page alert-center-page">
        <div class="page-heading">
          <div><span class="eyebrow">Alert Center · Ejecución durable</span><h1>Programaciones</h1><p>Cron validado, zona horaria, horario laboral, festivos, quiet hours y catch-up.</p></div>
          <div class="button-row"><button id="schedule-back-rules" class="btn" type="button">Reglas</button><button id="schedule-new" class="btn primary" type="button" ${rules.items.length ? "" : "disabled"}>Nueva programación</button></div>
        </div>
        <div class="metric-grid">
          <article class="metric-card"><span>Total</span><strong>${schedules.length}</strong></article>
          <article class="metric-card"><span>Activas</span><strong>${schedules.filter(item => item.isActive).length}</strong></article>
          <article class="metric-card"><span>Digest diario</span><strong>${schedules.filter(item => item.digest === "Daily").length}</strong></article>
          <article class="metric-card"><span>Digest semanal</span><strong>${schedules.filter(item => item.digest === "Weekly").length}</strong></article>
        </div>
        <div class="table-wrap"><table class="enterprise-table">
          <thead><tr><th>Código</th><th>Nombre</th><th>Cron</th><th>Zona horaria</th><th>Próxima ejecución</th><th>Catch-up</th><th>Estado</th><th>Acción</th></tr></thead>
          <tbody>${schedules.map(item => `<tr>
            <td><strong>${escapeHtml(item.code)}</strong></td><td>${escapeHtml(item.name)}</td><td><code>${escapeHtml(item.cronExpression)}</code></td>
            <td>${escapeHtml(item.timeZoneId)}</td><td>${escapeHtml(formatDate(item.nextExecutionAtUtc))}</td><td>${escapeHtml(item.catchUpPolicy)}</td>
            <td><span class="status-pill">${item.isActive ? "Activa" : "Deshabilitada"}</span></td>
            <td><button class="btn small" type="button" data-schedule-state="${item.id}" data-schedule-active="${item.isActive}">${item.isActive ? "Deshabilitar" : "Activar"}</button></td>
          </tr>`).join("") || `<tr><td colspan="8"><div class="empty-state"><h2>Sin programaciones</h2><p>Publique una regla y configure su calendario de ejecución.</p></div></td></tr>`}</tbody>
        </table></div>
        <dialog id="schedule-wizard-dialog" class="enterprise-dialog"></dialog>
      </section>`;
    content.querySelector("#schedule-back-rules").addEventListener("click", () => renderRuleCenter(content));
    content.querySelector("#schedule-new")?.addEventListener("click", () => openScheduleWizard(content, rules.items));
    content.querySelectorAll("[data-schedule-state]").forEach(button => button.addEventListener("click", async () => {
      await api(`/schedules/${button.dataset.scheduleState}/state`, {
        method: "POST",
        body: { isActive: button.dataset.scheduleActive !== "true" }
      });
      await renderScheduleCenter(content);
    }));
  }

  function openScheduleWizard(content, rules) {
    const dialog = content.querySelector("#schedule-wizard-dialog");
    const defaultZone = Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC";
    dialog.innerHTML = `
      <form id="schedule-wizard-form" class="template-editor">
        <div class="page-heading"><div><span class="eyebrow">Wizard de programación</span><h2>Programar una regla</h2></div><button id="schedule-close" class="btn subtle" type="button">Cerrar</button></div>
        <div class="grid two">
          <label>Regla publicada<select name="definitionId" required>${rules.map(rule => `<option value="${rule.id}">${escapeHtml(rule.code)} · ${escapeHtml(rule.name)}</option>`).join("")}</select></label>
          <label>Frecuencia<select id="schedule-preset" name="preset"><option value="0 8 * * *">Diaria 08:00</option><option value="0 8 * * 1">Semanal lunes 08:00</option><option value="0 8 1 * *">Mensual día 1</option><option value="">Personalizada</option></select></label>
          <label>Código<input name="code" maxlength="160" required /></label>
          <label>Nombre<input name="name" maxlength="200" required /></label>
          <label>Expresión cron<input id="schedule-cron" name="cronExpression" value="0 8 * * *" maxlength="120" required /></label>
          <label>Zona horaria<input name="timeZoneId" value="${escapeHtml(defaultZone)}" maxlength="120" required /></label>
        </div>
        <fieldset><legend>Horario laboral y festivos</legend>
          <div class="grid three"><label>Inicio<input name="businessStart" type="time" value="08:00" /></label><label>Fin<input name="businessEnd" type="time" value="17:00" /></label><label>Festivos ISO<textarea name="holidays" rows="2" placeholder="2026-01-01, 2026-12-25"></textarea></label></div>
          <label>Días laborables<input name="businessDays" value="Monday,Tuesday,Wednesday,Thursday,Friday" /></label>
        </fieldset>
        <fieldset><legend>Quiet hours y recuperación</legend>
          <div class="grid three"><label>Quiet desde<input name="quietStart" type="time" value="22:00" /></label><label>Quiet hasta<input name="quietEnd" type="time" value="06:00" /></label>
          <label>Catch-up<select name="catchUpPolicy"><option>RunLatest</option><option>RunAll</option><option>Skip</option></select></label></div>
          <div class="grid two"><label>Máximo catch-up<input name="maxCatchUpExecutions" type="number" min="1" max="1000" value="24" /></label><label>Digest<select name="digest"><option>None</option><option>Daily</option><option>Weekly</option></select></label></div>
        </fieldset>
        <output id="schedule-preview-result" class="field-hint">Valide la programación antes de guardarla.</output>
        <div class="button-row"><button id="schedule-preview" class="btn" type="button">Vista previa</button><button id="schedule-save" class="btn primary" type="submit" disabled>Guardar</button></div>
      </form>`;
    const formElement = dialog.querySelector("#schedule-wizard-form");
    dialog.querySelector("#schedule-close").addEventListener("click", () => dialog.close());
    dialog.querySelector("#schedule-preset").addEventListener("change", event => {
      if (event.currentTarget.value) dialog.querySelector("#schedule-cron").value = event.currentTarget.value;
    });
    const payload = () => {
      const form = new FormData(formElement);
      const days = String(form.get("businessDays") || "").split(",").map(item => item.trim()).filter(Boolean);
      const holidays = String(form.get("holidays") || "").split(",").map(item => item.trim()).filter(Boolean);
      return {
        definitionId: String(form.get("definitionId")),
        code: String(form.get("code")),
        name: String(form.get("name")),
        cronExpression: String(form.get("cronExpression")),
        timeZoneId: String(form.get("timeZoneId")),
        businessCalendarJson: JSON.stringify({ days, holidays, start: String(form.get("businessStart") || ""), end: String(form.get("businessEnd") || "") }),
        quietHoursJson: JSON.stringify({ start: String(form.get("quietStart") || ""), end: String(form.get("quietEnd") || "") }),
        catchUpPolicy: String(form.get("catchUpPolicy")),
        maxCatchUpExecutions: Number(form.get("maxCatchUpExecutions")),
        digest: String(form.get("digest"))
      };
    };
    dialog.querySelector("#schedule-preview").addEventListener("click", async () => {
      try {
        const value = payload();
        const preview = await api("/schedules/preview", { method: "POST", body: { ...value, count: 5, fromUtc: null } });
        dialog.querySelector("#schedule-preview-result").innerHTML = preview.occurrences.map(item => `${escapeHtml(formatDate(item.effectiveAtUtc))} <small>${escapeHtml(item.reason)}</small>`).join("<br>");
        dialog.querySelector("#schedule-save").disabled = false;
      } catch (error) {
        dialog.querySelector("#schedule-save").disabled = true;
        dialog.querySelector("#schedule-preview-result").textContent = error.message;
      }
    });
    formElement.addEventListener("submit", async event => {
      event.preventDefault();
      await api("/schedules", { method: "POST", body: payload() });
      dialog.close();
      await renderScheduleCenter(content);
    });
    dialog.showModal();
  }

  async function renderOperationsConsole(content, filters = {}) {
    content.innerHTML = `<section class="module-page alert-center-page"><div class="page-heading"><div><span class="eyebrow">Alert Center</span><h1>Consola operativa</h1><p>Cargando telemetría real…</p></div></div></section>`;
    const parameters = new URLSearchParams({ page: "1", pageSize: "100" });
    Object.entries(filters).forEach(([key, value]) => { if (value) parameters.set(key, value); });
    const [dashboard, messages] = await Promise.all([
      api("/operations/dashboard"),
      api(`/operations/messages?${parameters}`)
    ]);
    content.innerHTML = `
      <section class="module-page alert-center-page">
        <div class="page-heading">
          <div><span class="eyebrow">Alert Center · Observabilidad y control</span><h1>Consola operativa</h1><p>Backlog, latencia, throughput, workers y trazabilidad de extremo a extremo.</p></div>
          <div class="button-row"><button id="ops-back-inbox" class="btn" type="button">Inbox</button><button id="ops-export" class="btn" type="button">Exportar CSV</button><button id="ops-refresh" class="btn primary" type="button">Actualizar</button></div>
        </div>
        <div class="metric-grid">
          <article class="metric-card"><span>Pendientes</span><strong>${dashboard.pending}</strong><small>Antigüedad ${dashboard.oldestBacklogMinutes.toFixed(1)} min</small></article>
          <article class="metric-card"><span>Procesando</span><strong>${dashboard.processing}</strong><small>${dashboard.activeWorkers} workers activos</small></article>
          <article class="metric-card"><span>Entregadas</span><strong>${dashboard.delivered}</strong><small>${dashboard.throughputLastHour}/hora</small></article>
          <article class="metric-card"><span>Fallidas</span><strong>${dashboard.failed}</strong><small>${dashboard.deadLetters} dead letters</small></article>
          <article class="metric-card"><span>Reintentos</span><strong>${dashboard.retried}</strong></article>
          <article class="metric-card"><span>Canceladas</span><strong>${dashboard.cancelled}</strong></article>
          <article class="metric-card"><span>Latencia promedio</span><strong>${Math.round(dashboard.averageLatencyMilliseconds)} ms</strong></article>
          <article class="metric-card"><span>Throughput 24h</span><strong>${dashboard.throughputLast24Hours}</strong></article>
        </div>
        <form id="ops-filters" class="alert-inbox-filters">
          <label>Buscar<input name="search" value="${escapeHtml(filters.search || "")}" placeholder="Asunto, destinatario o idempotencia" /></label>
          <label>Estado<select name="status"><option value="">Todos</option>${["Queued","Processing","Sent","Delivered","Failed","Retried","Cancelled","DeadLetter"].map(value => `<option value="${value}" ${filters.status === value ? "selected" : ""}>${value}</option>`).join("")}</select></label>
          <label>Canal<select name="channel"><option value="">Todos</option>${["InApp","Email","Sms","WhatsApp","Push","Webhook"].map(value => `<option value="${value}" ${filters.channel === value ? "selected" : ""}>${value}</option>`).join("")}</select></label>
          <button class="btn primary" type="submit">Aplicar</button>
        </form>
        <div class="table-wrap"><table class="enterprise-table">
          <thead><tr><th>Fecha</th><th>Estado</th><th>Canal</th><th>Asunto</th><th>Destinatario</th><th>Proveedor</th><th>Reintentos</th><th>Acciones</th></tr></thead>
          <tbody>${messages.items.map(item => `<tr>
            <td>${escapeHtml(formatDate(item.queuedAtUtc))}</td><td><span class="status-pill ${item.status === "Failed" || item.status === "DeadLetter" ? "danger" : ""}">${escapeHtml(item.status)}</span></td>
            <td>${escapeHtml(item.channel)}</td><td>${escapeHtml(item.subject)}</td><td>${escapeHtml(item.recipient)}</td><td>${escapeHtml(item.provider || "—")}</td><td>${item.retryCount}</td>
            <td><div class="button-row compact"><button class="btn subtle small" type="button" data-ops-detail="${item.id}">Detalle</button>${operationButton(item)}</div></td>
          </tr>`).join("") || `<tr><td colspan="8"><div class="empty-state"><h2>Sin mensajes</h2><p>No hay resultados para los filtros seleccionados.</p></div></td></tr>`}</tbody>
        </table></div>
        <dialog id="ops-detail-dialog" class="enterprise-dialog"></dialog>
      </section>`;
    content.querySelector("#ops-back-inbox").addEventListener("click", () => renderAlertCenter(content));
    content.querySelector("#ops-refresh").addEventListener("click", () => renderOperationsConsole(content, filters));
    content.querySelector("#ops-filters").addEventListener("submit", event => {
      event.preventDefault();
      const form = new FormData(event.currentTarget);
      renderOperationsConsole(content, Object.fromEntries(form.entries()));
    });
    content.querySelector("#ops-export").addEventListener("click", () => exportOperations(parameters));
    content.querySelectorAll("[data-ops-detail]").forEach(button => button.addEventListener("click", () => openOperationsDetail(content, button.dataset.opsDetail)));
    content.querySelectorAll("[data-ops-action]").forEach(button => button.addEventListener("click", async () => {
      const action = button.dataset.opsAction;
      if (!window.confirm(`¿Confirma la operación ${action}?`)) return;
      await api(`/operations/messages/${button.dataset.opsMessage}/actions`, { method: "POST", body: { action } });
      await renderOperationsConsole(content, filters);
    }));
  }

  async function renderProviderCenter(content) {
    content.innerHTML = `<section class="module-page alert-center-page"><div class="page-heading"><div><span class="eyebrow">Alert Center</span><h1>Provider Center</h1><p>Cargando configuración cifrada…</p></div></div></section>`;
    const providers = await api("/providers");
    content.innerHTML = `
      <section class="module-page alert-center-page">
        <div class="page-heading">
          <div><span class="eyebrow">Alert Center · Canales externos</span><h1>Provider Center</h1><p>Prioridad, failover, rate limiting, circuit breaker y secretos protegidos por tenant.</p></div>
          <div class="button-row"><button id="providers-back-inbox" class="btn" type="button">Inbox</button><button id="providers-new" class="btn primary" type="button">Configurar proveedor</button></div>
        </div>
        <div class="metric-grid">
          <article class="metric-card"><span>Configurados</span><strong>${providers.length}</strong></article>
          <article class="metric-card"><span>Habilitados</span><strong>${providers.filter(item => item.isEnabled).length}</strong></article>
          <article class="metric-card"><span>Circuitos abiertos</span><strong>${providers.filter(item => item.circuitOpenUntilUtc && new Date(item.circuitOpenUntilUtc) > new Date()).length}</strong></article>
          <article class="metric-card"><span>Con vault</span><strong>${providers.filter(item => item.usesVaultReference).length}</strong></article>
        </div>
        <div class="table-wrap"><table class="enterprise-table">
          <thead><tr><th>Prioridad</th><th>Proveedor</th><th>Nombre</th><th>Autenticación</th><th>Remitente</th><th>Rate/min</th><th>Salud</th><th>Secretos</th><th>Acciones</th></tr></thead>
          <tbody>${providers.map(item => `<tr>
            <td>${item.priority}</td><td><strong>${escapeHtml(item.provider)}</strong></td><td>${escapeHtml(item.name)}</td><td>${escapeHtml(item.authentication)}</td>
            <td>${escapeHtml(item.fromAddress)}</td><td>${item.rateLimitPerMinute}</td>
            <td><span class="status-pill ${item.circuitOpenUntilUtc && new Date(item.circuitOpenUntilUtc) > new Date() ? "danger" : ""}">${item.isEnabled ? (item.circuitOpenUntilUtc ? "Circuit breaker" : "Habilitado") : "Deshabilitado"}</span></td>
            <td>${escapeHtml(item.secretMask)}${item.usesVaultReference ? " · vault" : ""}</td>
            <td><div class="button-row compact"><button class="btn subtle small" data-provider-test="${item.id}" type="button">Probar conexión</button><button class="btn small" data-provider-send="${item.id}" type="button">Envío sandbox</button></div></td>
          </tr>`).join("") || `<tr><td colspan="9"><div class="empty-state"><h2>Sin proveedores</h2><p>Configure SMTP, Microsoft 365, SendGrid, Mailgun, Resend o Amazon SES.</p></div></td></tr>`}</tbody>
        </table></div>
        <dialog id="provider-wizard-dialog" class="enterprise-dialog"></dialog>
        <dialog id="provider-test-dialog" class="enterprise-dialog"></dialog>
      </section>`;
    content.querySelector("#providers-back-inbox").addEventListener("click", () => renderAlertCenter(content));
    content.querySelector("#providers-new").addEventListener("click", () => openProviderWizard(content));
    content.querySelectorAll("[data-provider-test]").forEach(button => button.addEventListener("click", async () => {
      const result = await api(`/providers/${button.dataset.providerTest}/connection-test`, { method: "POST" });
      showProviderResult(content, "Prueba de conexión", result);
    }));
    content.querySelectorAll("[data-provider-send]").forEach(button => button.addEventListener("click", () => openProviderSandbox(content, button.dataset.providerSend)));
  }

  async function renderRecipientCenter(content) {
    const directory = await api("/recipient-directory");
    const cards = (title, items, empty) => `<article class="panel"><h2>${title}</h2>${items.map(item => `<div class="alert-inbox-item"><div><strong>${escapeHtml(item.name)}</strong>${item.address ? `<small>${escapeHtml(item.address)}</small>` : ""}</div><code>${item.id}</code></div>`).join("") || `<p class="field-hint">${empty}</p>`}</article>`;
    content.innerHTML = `
      <section class="module-page alert-center-page">
        <div class="page-heading"><div><span class="eyebrow">Alert Center · Resolución</span><h1>Directorio de destinatarios</h1><p>Audiencias por relación, grupo, departamento, lista, usuario y correo externo autorizado.</p></div><button id="recipients-back" class="btn">Inbox</button></div>
        <div class="grid two">
          ${cards("Grupos", directory.groups, "Sin grupos configurados.")}
          ${cards("Departamentos", directory.departments, "Sin departamentos configurados.")}
          ${cards("Listas de distribución", directory.distributionLists, "Sin listas configuradas.")}
          ${cards("Externos autorizados", directory.externalRecipients, "Sin destinatarios externos autorizados.")}
        </div>
        <article class="panel"><h2>Fallback del tenant</h2><p>${directory.fallback ? `${escapeHtml(directory.fallback.mode)} · ${escapeHtml(directory.fallback.routing)} · ${escapeHtml(directory.fallback.targetId || "sin destino")}` : "No configurado"}</p>${can("NOTIFICATION.ADMIN") ? `<form id="recipient-fallback-form" class="grid three"><label>Modo<select name="mode"><option>None</option><option>User</option><option>Role</option><option>Group</option><option>Department</option><option>DistributionList</option></select></label><label>ID destino<input name="targetId" /></label><label>Entrega<select name="routing"><option>To</option><option>Cc</option><option>Bcc</option></select></label><button class="btn">Guardar fallback</button></form>` : ""}</article>
        <div class="grid two">
          <form id="recipient-group-form" class="panel"><h2>Nuevo grupo</h2><label>Nombre<input name="name" required maxlength="160" /></label><button class="btn primary">Crear grupo</button></form>
          <form id="recipient-department-form" class="panel"><h2>Nuevo departamento</h2><label>Nombre<input name="name" required maxlength="160" /></label><button class="btn primary">Crear departamento</button></form>
          <form id="recipient-list-form" class="panel"><h2>Nueva lista</h2><label>Nombre<input name="name" required maxlength="160" /></label><button class="btn primary">Crear lista</button></form>
          ${can("NOTIFICATION.ADMIN") ? `<form id="recipient-external-form" class="panel"><h2>Autorizar correo externo</h2><label>Nombre<input name="displayName" required /></label><label>Correo<input name="email" type="email" required /></label><button class="btn primary">Autorizar</button></form>` : ""}
        </div>
        <article class="panel"><h2>Asignaciones</h2><div class="grid two">
          <form id="recipient-group-member-form"><label>Grupo<select name="groupId">${directory.groups.map(item => `<option value="${item.id}">${escapeHtml(item.name)}</option>`).join("")}</select></label><label>ID de usuario<input name="userId" required /></label><button class="btn">Agregar a grupo</button></form>
          <form id="recipient-list-member-form"><label>Lista<select name="listId">${directory.distributionLists.map(item => `<option value="${item.id}">${escapeHtml(item.name)}</option>`).join("")}</select></label><label>ID usuario (o vacío si externo)<input name="userId" /></label><label>ID externo autorizado<input name="externalRecipientId" /></label><button class="btn">Agregar a lista</button></form>
        </div></article>
      </section>`;
    content.querySelector("#recipients-back").addEventListener("click", () => renderAlertCenter(content));
    bindSimpleCreate(content, "#recipient-group-form", "/recipient-groups", "POST");
    bindSimpleCreate(content, "#recipient-department-form", "/recipient-departments", "POST");
    bindSimpleCreate(content, "#recipient-list-form", "/distribution-lists", "POST");
    bindSimpleCreate(content, "#recipient-external-form", "/authorized-external-recipients", "POST");
    content.querySelector("#recipient-fallback-form")?.addEventListener("submit", async event => {
      event.preventDefault();
      const form = new FormData(event.currentTarget);
      await api("/recipient-fallback", { method: "PUT", body: {
        mode: form.get("mode"),
        targetId: String(form.get("targetId") || "") || null,
        routing: form.get("routing")
      } });
      await renderRecipientCenter(content);
    });
    content.querySelector("#recipient-group-member-form")?.addEventListener("submit", async event => {
      event.preventDefault();
      const form = new FormData(event.currentTarget);
      await api(`/recipient-groups/${form.get("groupId")}/members`, { method: "POST", body: { userId: form.get("userId") } });
      await renderRecipientCenter(content);
    });
    content.querySelector("#recipient-list-member-form")?.addEventListener("submit", async event => {
      event.preventDefault();
      const form = new FormData(event.currentTarget);
      await api(`/distribution-lists/${form.get("listId")}/members`, { method: "POST", body: {
        userId: String(form.get("userId") || "") || null,
        externalRecipientId: String(form.get("externalRecipientId") || "") || null
      } });
      await renderRecipientCenter(content);
    });
  }

  function bindSimpleCreate(content, selector, path, method) {
    content.querySelector(selector)?.addEventListener("submit", async event => {
      event.preventDefault();
      await api(path, { method, body: Object.fromEntries(new FormData(event.currentTarget).entries()) });
      await renderRecipientCenter(content);
    });
  }

  function openProviderWizard(content) {
    const dialog = content.querySelector("#provider-wizard-dialog");
    dialog.innerHTML = `
      <form id="provider-wizard-form" class="template-editor">
        <div class="page-heading"><div><span class="eyebrow">Wizard seguro</span><h2>Configurar proveedor</h2><p>Los secretos se cifran al guardar y nunca regresan a la interfaz.</p></div><button id="provider-close" class="btn subtle" type="button">Cerrar</button></div>
        <fieldset><legend>1. Proveedor y prioridad</legend><div class="grid three">
          <label>Proveedor<select id="provider-kind" name="provider"><option>Smtp</option><option>Microsoft365</option><option>SendGrid</option><option>Mailgun</option><option>Resend</option><option>AmazonSes</option><option>GmailSmtp</option><option>ExchangeOnline</option></select></label>
          <label>Nombre<input name="name" maxlength="120" required /></label><label>Prioridad<input name="priority" type="number" min="1" max="100" value="1" required /></label>
        </div><label><input name="isEnabled" type="checkbox" checked /> Habilitado para failover</label></fieldset>
        <fieldset><legend>2. Autenticación</legend><div class="grid three">
          <label>Mecanismo<select id="provider-auth" name="authentication"><option>UsernamePassword</option><option>ApiKey</option><option>OAuth2ClientCredentials</option><option>AwsSignatureV4</option><option>VaultReference</option></select></label>
          <label>Usuario / Access key<input name="username" autocomplete="off" /></label><label>Secreto / API key<input name="secret" type="password" autocomplete="new-password" /></label>
          <label>Client ID<input name="clientId" /></label><label>Tenant Directory ID<input name="directoryTenantId" /></label><label>Referencia vault<input name="vaultReference" placeholder="vault://..." /></label>
        </div></fieldset>
        <fieldset><legend>3. Endpoint y remitente</legend><div class="grid three">
          <label>Host<input name="host" /></label><label>Puerto<input name="port" type="number" min="1" max="65535" /></label><label>Base URL<input name="baseUrl" type="url" /></label>
          <label>Dominio Mailgun<input name="domain" /></label><label>Región AWS<input name="region" value="us-east-1" /></label><label><input name="useSsl" type="checkbox" checked /> TLS/SSL</label>
          <label>Correo remitente<input name="fromAddress" type="email" required /></label><label>Nombre remitente<input name="fromName" /></label>
        </div></fieldset>
        <fieldset><legend>4. Resiliencia</legend><div class="grid three"><label>Límite/min<input name="rateLimitPerMinute" type="number" min="1" value="60" /></label><label>Umbral fallas<input name="circuitFailureThreshold" type="number" min="1" value="5" /></label><label>Apertura (seg)<input name="circuitBreakSeconds" type="number" min="10" value="300" /></label></div></fieldset>
        <div class="button-row"><button class="btn primary" type="submit">Guardar cifrado</button></div>
      </form>`;
    const kind = dialog.querySelector("#provider-kind");
    const auth = dialog.querySelector("#provider-auth");
    const chooseAuth = () => {
      auth.value = kind.value === "AmazonSes" ? "AwsSignatureV4"
        : kind.value === "Microsoft365" || kind.value === "ExchangeOnline" ? "OAuth2ClientCredentials"
        : kind.value === "Smtp" || kind.value === "GmailSmtp" ? "UsernamePassword" : "ApiKey";
    };
    kind.addEventListener("change", chooseAuth);
    dialog.querySelector("#provider-close").addEventListener("click", () => dialog.close());
    dialog.querySelector("#provider-wizard-form").addEventListener("submit", async event => {
      event.preventDefault();
      const form = new FormData(event.currentTarget);
      await api("/providers", {
        method: "POST",
        body: {
          providerId: null,
          provider: String(form.get("provider")),
          name: String(form.get("name")),
          priority: Number(form.get("priority")),
          isEnabled: form.get("isEnabled") === "on",
          authentication: String(form.get("authentication")),
          fromAddress: String(form.get("fromAddress")),
          fromName: String(form.get("fromName") || "") || null,
          settings: {
            host: String(form.get("host") || "") || null,
            port: Number(form.get("port")) || null,
            username: String(form.get("username") || "") || null,
            secret: String(form.get("secret") || "") || null,
            baseUrl: String(form.get("baseUrl") || "") || null,
            domain: String(form.get("domain") || "") || null,
            useSsl: form.get("useSsl") === "on",
            clientId: String(form.get("clientId") || "") || null,
            directoryTenantId: String(form.get("directoryTenantId") || "") || null,
            region: String(form.get("region") || "") || null,
            vaultReference: String(form.get("vaultReference") || "") || null
          },
          rateLimitPerMinute: Number(form.get("rateLimitPerMinute")),
          circuitFailureThreshold: Number(form.get("circuitFailureThreshold")),
          circuitBreakSeconds: Number(form.get("circuitBreakSeconds"))
        }
      });
      dialog.close();
      await renderProviderCenter(content);
    });
    chooseAuth();
    dialog.showModal();
  }

  function openProviderSandbox(content, providerId) {
    const dialog = content.querySelector("#provider-test-dialog");
    dialog.innerHTML = `<form id="provider-sandbox-form" class="template-editor"><div class="page-heading"><div><span class="eyebrow">Sandbox</span><h2>Prueba real de envío</h2></div><button id="provider-sandbox-close" class="btn" type="button">Cerrar</button></div><label>Destinatario<input name="recipient" type="email" required /></label><label>Asunto<input name="subject" value="Compliance 360 provider sandbox test" /></label><label>Contenido<textarea name="body" rows="5"><p>Mensaje de prueba del Alert Center Enterprise.</p></textarea></label><button class="btn primary" type="submit">Enviar prueba</button></form>`;
    dialog.querySelector("#provider-sandbox-close").addEventListener("click", () => dialog.close());
    dialog.querySelector("#provider-sandbox-form").addEventListener("submit", async event => {
      event.preventDefault();
      const form = new FormData(event.currentTarget);
      const result = await api(`/providers/${providerId}/sandbox-send`, { method: "POST", body: Object.fromEntries(form.entries()) });
      showProviderResult(content, "Resultado de envío sandbox", result);
    });
    dialog.showModal();
  }

  function showProviderResult(content, title, result) {
    const dialog = content.querySelector("#provider-test-dialog");
    dialog.innerHTML = `<div class="template-editor"><div class="page-heading"><div><span class="eyebrow">${result.success ? "PASS" : "FAIL"}</span><h2>${escapeHtml(title)}</h2></div><button id="provider-result-close" class="btn">Cerrar</button></div><div class="callout ${result.success ? "success" : "danger"}">${escapeHtml(result.message)}</div>${result.providerMessageId ? `<p>ID proveedor: <code>${escapeHtml(result.providerMessageId)}</code></p>` : ""}</div>`;
    dialog.querySelector("#provider-result-close").addEventListener("click", () => dialog.close());
    if (!dialog.open) dialog.showModal();
  }

  function operationButton(item) {
    if (item.status === "Failed") return `<button class="btn small" type="button" data-ops-action="retry" data-ops-message="${item.id}">Retry</button>`;
    if (item.status === "DeadLetter") return `<button class="btn small" type="button" data-ops-action="reprocess" data-ops-message="${item.id}">Reprocesar</button>`;
    if (item.status === "Queued" || item.status === "Retried") return `<button class="btn small" type="button" data-ops-action="cancel" data-ops-message="${item.id}">Cancelar</button>`;
    if (item.status === "Sent" || item.status === "Delivered") return `<button class="btn small" type="button" data-ops-action="resend" data-ops-message="${item.id}">Reenviar</button>`;
    return "";
  }

  async function openOperationsDetail(content, messageId) {
    const detail = await api(`/operations/messages/${messageId}`);
    const message = detail.message;
    const dialog = content.querySelector("#ops-detail-dialog");
    dialog.innerHTML = `
      <div class="template-editor">
        <div class="page-heading"><div><span class="eyebrow">${escapeHtml(message.id)}</span><h2>${escapeHtml(message.subject)}</h2><p>${escapeHtml(message.status)} · ${escapeHtml(message.channel)} · ${escapeHtml(message.recipient)}</p></div><button id="ops-detail-close" class="btn">Cerrar</button></div>
        <div class="grid two">
          <article class="metric-card"><span>Regla / versión</span><strong>${escapeHtml(message.alertDefinitionId || "—")}</strong><small>${escapeHtml(message.alertDefinitionVersionId || "")}</small></article>
          <article class="metric-card"><span>Ocurrencia / plantilla</span><strong>${escapeHtml(message.alertOccurrenceId || "—")}</strong><small>${escapeHtml(message.templateId || "")}</small></article>
        </div>
        <h3>Historial</h3>
        <div class="table-wrap"><table class="enterprise-table"><thead><tr><th>Fecha</th><th>Estado</th><th>Detalle</th></tr></thead><tbody>
          ${detail.history.map(item => `<tr><td>${escapeHtml(formatDate(item.occurredAtUtc))}</td><td>${escapeHtml(item.status)}</td><td>${escapeHtml(item.detail)}</td></tr>`).join("") || `<tr><td colspan="3">Sin eventos.</td></tr>`}
        </tbody></table></div>
        ${message.failureReason ? `<div class="callout danger"><strong>Falla:</strong> ${escapeHtml(message.failureReason)}</div>` : ""}
      </div>`;
    dialog.querySelector("#ops-detail-close").addEventListener("click", () => dialog.close());
    dialog.showModal();
  }

  async function exportOperations(parameters) {
    const token = localStorage.getItem("c360.token");
    const response = await fetch(`/api/v2/tenants/${tenantId()}/alert-center/operations/export.csv?${parameters}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    if (!response.ok) throw new Error("No se pudo exportar la consola operativa.");
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = `alert-center-${new Date().toISOString().slice(0, 10)}.csv`;
    anchor.click();
    URL.revokeObjectURL(url);
  }

  window.renderAlertCenter = renderAlertCenter;
  window.getAlertCenterCounts = () => api("/inbox/counts");
})();
