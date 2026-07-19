window.C360_MANUAL = window.C360_MANUAL || {};
(function () {
  const KEY = "c360.manual.progress.v1";
  function load() {
    try { return JSON.parse(localStorage.getItem(KEY) || "{}"); } catch { return {}; }
  }
  function save(state) { localStorage.setItem(KEY, JSON.stringify(state)); }
  function ensure() {
    const s = load();
    s.roles = s.roles || {};
    s.theme = s.theme || "system";
    s.lastRole = s.lastRole || null;
    s.lastChapter = s.lastChapter || null;
    s.completedTutorials = s.completedTutorials || [];
    s.completedSimSteps = s.completedSimSteps || [];
    s.checklist = s.checklist || {};
    return s;
  }
  function roleProgress(roleId) {
    const s = ensure();
    const role = (window.C360_MANUAL.data.roles || []).find(r => r.id === roleId);
    if (!role) return 0;
    const tuts = role.tutorials || [];
    const done = tuts.filter(t => s.completedTutorials.includes(t)).length;
    const checks = Object.values(s.checklist[roleId] || {}).filter(Boolean).length;
    const total = tuts.length + 10;
    return Math.round(((done + checks) / total) * 100);
  }
  function overall() {
    const roles = window.C360_MANUAL.data.roles || [];
    if (!roles.length) return 0;
    return Math.round(roles.reduce((a, r) => a + roleProgress(r.id), 0) / roles.length);
  }
  function markTutorial(id) {
    const s = ensure();
    if (!s.completedTutorials.includes(id)) s.completedTutorials.push(id);
    save(s);
    window.dispatchEvent(new Event("c360-progress"));
  }
  function setTheme(theme) {
    const s = ensure();
    s.theme = theme;
    save(s);
    applyTheme();
  }
  function applyTheme() {
    const s = ensure();
    let theme = s.theme;
    if (theme === "system") {
      theme = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }
    document.documentElement.setAttribute("data-theme", theme);
    document.body && document.body.setAttribute("data-theme", theme);
  }
  function reset() {
    localStorage.removeItem(KEY);
    window.dispatchEvent(new Event("c360-progress"));
  }
  function setLast(roleId, chapter) {
    const s = ensure();
    s.lastRole = roleId;
    s.lastChapter = chapter;
    save(s);
  }
  function toggleCheck(roleId, key, val) {
    const s = ensure();
    s.checklist[roleId] = s.checklist[roleId] || {};
    s.checklist[roleId][key] = !!val;
    save(s);
    window.dispatchEvent(new Event("c360-progress"));
  }
  window.C360_MANUAL.progress = {
    load: ensure, save, roleProgress, overall, markTutorial, setTheme, applyTheme, reset, setLast, toggleCheck, KEY
  };
})();
