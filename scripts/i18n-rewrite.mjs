/**
 * Rewrites hardcoded UI Spanish/English literals in SPA JS to t("Semantic.Key").
 * Uses locales/es.json + docs/i18n/03_KEY_MAPPING.json reverse maps.
 */
import fs from "fs";
import path from "path";

const root = process.cwd();
const es = JSON.parse(fs.readFileSync(path.join(root, "src/Compliance360.Web/wwwroot/locales/es.json"), "utf8"));
const en = JSON.parse(fs.readFileSync(path.join(root, "src/Compliance360.Web/wwwroot/locales/en.json"), "utf8"));

// Prefer longer phrases first to avoid partial replacements
const pairs = Object.keys(es)
  .map(key => ({ key, es: es[key], en: en[key] }))
  .filter(p => typeof p.es === "string" && p.es.length >= 3)
  .filter(p => !/^c360\.|^REGULATORY\.|^[a-f0-9-]{36}$/i.test(p.es))
  .sort((a, b) => b.es.length - a.es.length);

const targets = [
  "src/Compliance360.Web/wwwroot/app.js",
  "src/Compliance360.Web/wwwroot/regulatory-affairs.js",
  "src/Compliance360.Web/wwwroot/form-template-builder.js"
].map(f => path.join(root, f)).filter(f => fs.existsSync(f));

function escapeRegExp(s) {
  return s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function rewriteFile(filePath) {
  let src = fs.readFileSync(filePath, "utf8");
  const original = src;
  let replacements = 0;

  // Skip replacing inside the old i18n bootstrap until we replace that block manually
  for (const { key, es: esText, en: enText } of pairs) {
    const candidates = [esText];
    if (enText && enText !== esText) candidates.push(enText);

    for (const text of candidates) {
      if (text.length < 3) continue;
      // Inside template literals: >text< or "text" used as UI — replace quoted/backtick occurrences
      // Pattern 1: >LITERAL<
      const angle = new RegExp(`(>[\\s]*)(${escapeRegExp(text)})([\\s]*<)`, "g");
      src = src.replace(angle, (m, a, _t, b) => {
        replacements++;
        return `${a}\${t("${key}")}${b}`;
      });

      // Pattern 2: placeholder="LITERAL" title="LITERAL" aria-label="LITERAL"
      const attr = new RegExp(`((?:placeholder|title|aria-label)=["'])(${escapeRegExp(text)})(["'])`, "g");
      src = src.replace(attr, (m, a, _t, b) => {
        replacements++;
        return `${a}\${t("${key}")}${b}`;
      });

      // Pattern 3: toast("LITERAL" or toast('LITERAL'
      const toast = new RegExp(`(toast\\(\\s*)(["'])(${escapeRegExp(text)})\\2`, "g");
      src = src.replace(toast, (m, a, q) => {
        replacements++;
        return `${a}t("${key}")`;
      });

      // Pattern 4: return "LITERAL" for short UI messages in throw/return string — careful: only exact full-string quotes of length>=8
      if (text.length >= 12) {
        const full = new RegExp(`((?:=|:|\\(|return)\\s*)(["'\`])(${escapeRegExp(text)})\\2`, "g");
        src = src.replace(full, (m, a, q) => {
          // Don't touch import paths / selectors
          if (text.includes("/") || text.includes(".")) return m;
          replacements++;
          return `${a}t("${key}")`;
        });
      }
    }
  }

  if (src !== original) {
    fs.writeFileSync(filePath, src);
  }
  return { file: path.relative(root, filePath), replacements, changed: src !== original };
}

const results = targets.map(rewriteFile);
fs.mkdirSync(path.join(root, "docs/i18n"), { recursive: true });
fs.writeFileSync(path.join(root, "docs/i18n/04_REWRITE_RESULTS.json"), JSON.stringify(results, null, 2));
console.log(JSON.stringify(results, null, 2));
