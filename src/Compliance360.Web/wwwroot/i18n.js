(function (window, document) {
  "use strict";

  const STORAGE_KEY = "c360.language";
  const SUPPORTED_LANGUAGES = ["es", "en"];
  const localeCache = new Map();
  let language = detectLanguage();
  let dictionary = {};
  let reverseMap = new Map();

  function normalizeLanguage(value) {
    return SUPPORTED_LANGUAGES.includes(value) ? value : "es";
  }

  function readCookie(name) {
    const prefix = `${encodeURIComponent(name)}=`;
    return document.cookie
      .split(";")
      .map(item => item.trim())
      .find(item => item.startsWith(prefix))
      ?.slice(prefix.length);
  }

  function detectLanguage() {
    const stored = localStorage.getItem(STORAGE_KEY)
      || sessionStorage.getItem(STORAGE_KEY)
      || readCookie(STORAGE_KEY);

    if (SUPPORTED_LANGUAGES.includes(stored)) return stored;
    return navigator.language?.toLowerCase().startsWith("en") ? "en" : "es";
  }

  function rebuildReverseMap() {
    const next = new Map();
    for (const payload of localeCache.values()) {
      if (!payload || typeof payload.then === "function") continue;
      Object.entries(payload).forEach(([key, value]) => {
        if (typeof value === "string" && value.trim()) next.set(value.trim(), key);
      });
    }
    reverseMap = next;
    window.__I18N_REVERSE = reverseMap;
  }

  function resolveKeyFromText(text) {
    return reverseMap.get(String(text || "").trim()) || null;
  }

  async function loadLocale(lang) {
    const normalized = normalizeLanguage(lang);
    if (localeCache.has(normalized) && typeof localeCache.get(normalized).then !== "function") {
      return localeCache.get(normalized);
    }
    const response = await fetch(`/locales/${normalized}.json`, { credentials: "same-origin" });
    if (!response.ok) throw new Error(`Unable to load locale "${normalized}".`);
    const locale = Object.freeze(await response.json());
    localeCache.set(normalized, locale);
    rebuildReverseMap();
    return locale;
  }

  function t(key, params) {
    const value = dictionary[key];
    if (typeof value !== "string") {
      if (isDevelopment()) console.warn(`[I18n] Missing translation: ${key}`);
      return key;
    }
    return value.replace(/\{\{(\w+)\}\}/g, (_, name) =>
      params?.[name] === undefined || params?.[name] === null ? `{{${name}}}` : String(params[name])
    );
  }

  function isDevelopment() {
    return /localhost|127\.0\.0\.1/.test(window.location.hostname)
      || document.documentElement.dataset.environment === "development";
  }

  function applyDom(root) {
    const scope = root || document;
    const apply = (selector, callback) => {
      const elements = [];
      if (scope instanceof Element && scope.matches?.(selector)) elements.push(scope);
      if (scope.querySelectorAll) elements.push(...scope.querySelectorAll(selector));
      elements.forEach(callback);
    };

    apply("[data-i18n]", element => {
      const value = t(element.dataset.i18n);
      if (element.textContent !== value) element.textContent = value;
    });
    apply("[data-i18n-placeholder]", element => {
      const value = t(element.dataset.i18nPlaceholder);
      if (element.placeholder !== value) element.placeholder = value;
    });
    apply("[data-i18n-title]", element => {
      const value = t(element.dataset.i18nTitle);
      if (element.title !== value) element.title = value;
    });
    apply("[data-i18n-aria]", element => {
      const value = t(element.dataset.i18nAria);
      if (element.getAttribute("aria-label") !== value) element.setAttribute("aria-label", value);
    });
    return scope;
  }

  function localeCode() {
    return language === "en" ? "en-US" : "es-PA";
  }

  function formatDate(value, options) {
    return new Intl.DateTimeFormat(localeCode(), options).format(new Date(value));
  }

  function formatNumber(value, options) {
    return new Intl.NumberFormat(localeCode(), options).format(value);
  }

  function formatCurrency(value, currency, options) {
    return new Intl.NumberFormat(localeCode(), {
      style: "currency",
      currency: currency || "USD",
      ...options
    }).format(value);
  }

  async function setLanguage(lang) {
    const normalized = normalizeLanguage(lang);
    // Load active + peer locale so reverse-map can translate leftover source strings.
    const [locale] = await Promise.all([
      loadLocale(normalized),
      loadLocale(normalized === "en" ? "es" : "en")
    ]);
    language = normalized;
    dictionary = locale;
    localStorage.setItem(STORAGE_KEY, normalized);
    sessionStorage.setItem(STORAGE_KEY, normalized);
    document.cookie = `${encodeURIComponent(STORAGE_KEY)}=${normalized}; path=/; max-age=31536000; samesite=lax`;
    document.documentElement.lang = normalized;
    rebuildReverseMap();
    applyDom(document);
    window.dispatchEvent(new CustomEvent("c360:languagechange", { detail: { language: normalized } }));
    return normalized;
  }

  function getLanguage() {
    return language;
  }

  function languageSelectorHtml() {
    return `<select class="c360-language-selector" data-c360-language-selector aria-label="${t("Settings.Language")}">`
      + `<option value="es"${language === "es" ? " selected" : ""}>🇪🇸 Español</option>`
      + `<option value="en"${language === "en" ? " selected" : ""}>🇺🇸 English</option>`
      + "</select>";
  }

  function bindLanguageSelector(onChange) {
    const selectors = document.querySelectorAll("[data-c360-language-selector], #language-selector");
    selectors.forEach(selector => {
      selector.value = language;
      if (selector.dataset.c360LanguageBound) return;
      selector.dataset.c360LanguageBound = "true";
      selector.addEventListener("change", async event => {
        const selectedLanguage = await setLanguage(event.target.value);
        selectors.forEach(other => { other.value = selectedLanguage; });
        onChange?.(selectedLanguage);
      });
    });
    return selectors;
  }

  const ready = setLanguage(language);

  window.I18n = Object.freeze({
    t,
    loadLocale,
    setLanguage,
    getLanguage,
    applyDom,
    formatDate,
    formatNumber,
    formatCurrency,
    languageSelectorHtml,
    bindLanguageSelector,
    resolveKeyFromText,
    ready
  });
})(window, document);
