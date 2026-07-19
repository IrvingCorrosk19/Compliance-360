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
