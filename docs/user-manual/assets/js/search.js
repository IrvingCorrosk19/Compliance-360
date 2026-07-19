window.C360_MANUAL = window.C360_MANUAL || {};
(function () {
  function normalize(s) { return String(s || "").toLowerCase().normalize("NFD").replace(/\p{Diacritic}/gu, ""); }
  function basePrefix() {
    return location.pathname.replace(/\\/g, "/").includes("/roles/") ? "../" : "";
  }
  function search(q) {
    const data = window.C360_MANUAL.data;
    const n = normalize(q).trim();
    if (!n || n.length < 2) return [];
    const out = [];
    const p = basePrefix();
    (data.roles || []).forEach(r => {
      const blob = normalize([r.name, r.purpose, ...(r.can||[]), ...(r.cannot||[])].join(" "));
      if (blob.includes(n)) out.push({ type: "Rol", title: r.name, href: p + "roles/" + r.file, snippet: r.purpose });
    });
    (data.screens || []).forEach(s => {
      const blob = normalize([s.name, s.route, s.objective].join(" "));
      if (blob.includes(n)) out.push({ type: "Pantalla", title: s.name, href: p + "index.html#screens", snippet: s.route + " — " + s.objective });
    });
    (data.fields || []).forEach(f => {
      const blob = normalize([f.label, f.purpose, f.example, f.tech].join(" "));
      if (blob.includes(n)) out.push({ type: "Campo", title: f.label, href: p + "index.html#fields", snippet: f.purpose });
    });
    (data.buttons || []).forEach(b => {
      const blob = normalize([b.label, b.action, b.result].join(" "));
      if (blob.includes(n)) out.push({ type: "Botón", title: b.label, href: p + "index.html#buttons", snippet: b.result });
    });
    (data.glossary || []).forEach(g => {
      const blob = normalize([g.term, g.def, g.example].join(" "));
      if (blob.includes(n)) out.push({ type: "Glosario", title: g.term, href: p + "index.html#glossary", snippet: g.def });
    });
    Object.entries(data.tutorials || {}).forEach(([id, t]) => {
      const blob = normalize([t.title, ...(t.steps||[])].join(" "));
      if (blob.includes(n)) out.push({ type: "Tutorial", title: t.title, href: p + "roles/" + ((data.roles.find(r => r.id === t.role)||{}).file || "regulatory-specialist.html") + "#" + id, snippet: (t.steps||[])[0] || "" });
    });
    (data.errors || []).forEach(e => {
      const blob = normalize([e.title, e.why, e.fix].join(" "));
      if (blob.includes(n)) out.push({ type: "Error", title: e.title, href: p + "index.html#errors", snippet: e.fix });
    });
    return out.slice(0, 40);
  }
  window.C360_MANUAL.search = search;
})();
