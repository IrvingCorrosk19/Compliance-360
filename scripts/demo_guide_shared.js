/* Shared premium UX — embedded in both demo guides */
window.C360Guide = {
  esc(s) { const d = document.createElement("div"); d.textContent = s ?? ""; return d.innerHTML; },

  phaseForBlock(data, id) {
    return (data.journey?.phases || []).find(p => p.blocks.includes(id));
  },

  progressRing(pct, size = 36) {
    const r = (size - 6) / 2;
    const c = 2 * Math.PI * r;
    const off = c - (pct / 100) * c;
    return `<svg width="${size}" height="${size}" viewBox="0 0 ${size} ${size}" class="progress-ring" aria-hidden="true">
      <circle cx="${size/2}" cy="${size/2}" r="${r}" fill="none" stroke="rgba(0,0,0,.08)" stroke-width="4"/>
      <circle cx="${size/2}" cy="${size/2}" r="${r}" fill="none" stroke="url(#rg)" stroke-width="4" stroke-dasharray="${c}" stroke-dashoffset="${off}" stroke-linecap="round" transform="rotate(-90 ${size/2} ${size/2})"/>
      <defs><linearGradient id="rg" x1="0" y1="0" x2="1" y2="1"><stop offset="0%" stop-color="#0f4c81"/><stop offset="100%" stop-color="#d4af37"/></linearGradient></defs>
      <text x="50%" y="54%" text-anchor="middle" font-size="9" font-weight="800" fill="#0f4c81">${pct}%</text></svg>`;
  },

  journeySvg(data, activeBlockId) {
    const phases = data.journey?.phases || [];
    if (!phases.length) return "";
    const w = Math.min(phases.length * 88, 880);
    return `<div class="journey-svg-wrap" style="overflow-x:auto;padding:.5rem 0"><svg viewBox="0 0 ${phases.length * 88} 72" width="${w}" height="72" class="journey-svg">
      ${phases.map((p, i) => {
        const active = p.blocks.includes(activeBlockId);
        const done = p.blocks.every(bid => window.C360Guide._doneSet?.has(bid));
        const x = i * 88 + 8;
        const fill = active ? "#1769aa" : done ? "#0f766e" : "#e2e8f0";
        const text = active ? "#fff" : done ? "#fff" : "#64748b";
        return `<g class="journey-node" data-phase-idx="${p.blocks[0]-1}" style="cursor:pointer">
          <rect x="${x}" y="8" width="72" height="44" rx="10" fill="${fill}" stroke="${active?"#d4af37":"transparent"}" stroke-width="2"/>
          <text x="${x+36}" y="26" text-anchor="middle" font-size="8" font-weight="700" fill="${text}">F${p.order}</text>
          <text x="${x+36}" y="40" text-anchor="middle" font-size="6.5" fill="${text}">${p.label.split(" ")[0]}</text>
          ${i < phases.length - 1 ? `<line x1="${x+72}" y1="30" x2="${x+88}" y2="30" stroke="#d4af37" stroke-width="2" marker-end="url(#arr)"/>` : ""}
        </g>`;
      }).join("")}
      <defs><marker id="arr" markerWidth="6" markerHeight="6" refX="5" refY="3" orient="auto"><path d="M0,0 L6,3 L0,6 Z" fill="#d4af37"/></marker></defs>
    </svg></div>`;
  },

  bindJourneySvg(root, onGo) {
    root.querySelectorAll(".journey-node").forEach(g => g.addEventListener("click", () => onGo(+g.dataset.phaseIdx)));
  },

  spotlightHtml(text) {
    return `<div class="spotlight-bar" id="spotlightBar"><span class="spotlight-pulse"></span><span id="spotlightText">${this.esc(text)}</span><button class="spotlight-btn" id="btnSpotlightNext">Siguiente paso →</button></div>`;
  },

  toast(el, msg) {
    if (!el) return;
    el.textContent = msg;
    el.classList.add("show");
    setTimeout(() => el.classList.remove("show"), 2400);
  },

  celebrate(el) {
    if (!el) return;
    el.classList.add("celebrate-flash");
    setTimeout(() => el.classList.remove("celebrate-flash"), 700);
  },

  copyText(text, toastEl) {
    if (navigator.clipboard) navigator.clipboard.writeText(text).then(() => this.toast(toastEl, "Copiado al portapapeles"));
  },

  credCard(role, cred, copyFn) {
    if (!cred) return "";
    return `<div class="cred-card premium"><div class="cred-head"><strong>🔑 ${this.esc(role)}</strong><button type="button" class="cred-copy-btn" data-copy-role="${this.esc(role)}">Copiar todo</button></div>
      <div class="cred-grid"><div><span>Tenant</span><code>${this.esc(cred.tenantId)}</code></div>
      <div><span>Email</span><code>${this.esc(cred.email)}</code></div>
      <div><span>Password</span><code>${this.esc(cred.password)}</code></div></div></div>`;
  }
};
