import fs from "fs";

function check(file) {
  try {
    fs.readFileSync(file, "utf8");
    return true;
  } catch {
    return false;
  }
}

for (const f of [
  "src/Compliance360.Web/wwwroot/i18n.js",
  "src/Compliance360.Web/wwwroot/app.js",
  "src/Compliance360.Web/wwwroot/regulatory-affairs.js"
]) {
  console.log("exists", f, check(f));
}

const es = JSON.parse(fs.readFileSync("src/Compliance360.Web/wwwroot/locales/es.json", "utf8"));
const en = JSON.parse(fs.readFileSync("src/Compliance360.Web/wwwroot/locales/en.json", "utf8"));
const esk = Object.keys(es);
const enk = Object.keys(en);
const missingEn = esk.filter(k => !(k in en));
const missingEs = enk.filter(k => !(k in es));
const app = fs.readFileSync("src/Compliance360.Web/wwwroot/app.js", "utf8");
const ra = fs.readFileSync("src/Compliance360.Web/wwwroot/regulatory-affairs.js", "utf8");
const tCalls = (app.match(/t\("/g) || []).length
  + (ra.match(/t\("/g) || []).length
  + (ra.match(/window\.t\?/g) || []).length;
const hard = [...app.matchAll(/>([A-Za-záéíóúñÁÉÍÓÚÑ][^<{$\n]{8,100})</g)]
  .map(m => m[1])
  .filter(s => /[áéíóúñÁÉÍÓÚÑ]|Guardar|Cancelar|Sesion|Usuario|Administr|Contrase/i.test(s));

fs.writeFileSync("docs/i18n/05_TRANSLATED_COVERAGE.md", `# Coverage

- Locale keys es/en: **${esk.length}** / **${enk.length}**
- Missing en: ${missingEn.length}
- Missing es: ${missingEs.length}
- Explicit t()/window.t calls: **${tCalls}**
- Remaining Spanish HTML candidates in app.js: **${hard.length}**

Critical surfaces migrated with semantic keys: Login, MFA, Nav, Shell actions, RA toasts, language selector, date formatting.
`);

fs.writeFileSync(
  "docs/i18n/06_PENDING_STRINGS.md",
  ["# Pending UI strings (candidates)", "", `Count: ${hard.length}`, "", ...[...new Set(hard)].slice(0, 120).map(s => `- ${s}`)].join("\n")
);

console.log({ keys: esk.length, missingEn: missingEn.length, tCalls, pending: hard.length });
