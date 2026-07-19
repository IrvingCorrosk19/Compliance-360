import fs from "fs";
import path from "path";

const root = path.resolve("src/Compliance360.Web/wwwroot");
const files = ["app.js", "regulatory-affairs.js", "form-template-builder.js", "index.html"]
  .map(f => path.join(root, f))
  .filter(f => fs.existsSync(f));

const re = /(['"`])((?:\\.|(?!\1).){2,200}?)\1/gs;
const skip = /^(api|c360\.|REGULATORY\.|TENANT\.|PLATFORM\.|http|\/|#|data-|btn |card |nav-|ra-|cs-|flex|grid|display|color|font|margin|padding|px|rem|%|true|false|null|undefined|GET|POST|PUT|PATCH|DELETE|application\/|text\/|image\/|[0-9.]+|[a-f0-9-]{36})$/i;
const uiHint = /[áéíóúñÁÉÍÓÚÑ¿¡]|\b(Guardar|Cancelar|Editar|Eliminar|Buscar|Iniciar|Cerrar|Usuario|Contraseña|Sesión|Administración|Configuración|Producto|Expediente|Requerido|Inválido|Error|Éxito|Bienvenido|Idioma|Roles|Permisos|Crear|Actualizar|Eliminar|Seleccione|Sin |No |Debe |Campo|Password|Email|Login|Save|Cancel|Delete|Search|Settings|Welcome|Required|Invalid)\b/i;

const found = [];
for (const file of files) {
  const text = fs.readFileSync(file, "utf8");
  const lines = text.split(/\r?\n/);
  lines.forEach((line, i) => {
    if (line.includes("translateText(") || line.includes("t(") && line.includes("Common.")) return;
    let m;
    const local = /(['"`])((?:\\.|(?!\1)[^\\\n]){2,180})\1/g;
    while ((m = local.exec(line))) {
      let t = m[2].replace(/\\'/g, "'").replace(/\\"/g, '"').replace(/\\n/g, " ").trim();
      if (t.length < 3 || skip.test(t) || t.includes("${")) continue;
      if (!uiHint.test(t) && !/[A-Za-zÁÉÍÓÚáéíóúñÑ ]{8,}/.test(t)) continue;
      // skip CSS-ish
      if (/[{};]|^\.|^\w+:\s/.test(t) && !uiHint.test(t)) continue;
      found.push({
        file: path.relative(process.cwd(), file).replace(/\\/g, "/"),
        line: i + 1,
        text: t.slice(0, 160)
      });
    }
  });
}

const unique = [];
const seen = new Set();
for (const item of found) {
  const k = item.text.toLowerCase();
  if (seen.has(k)) continue;
  seen.add(k);
  unique.push(item);
}

function suggestKey(text) {
  const base = text
    .normalize("NFD").replace(/[\u0300-\u036f]/g, "")
    .replace(/[^a-zA-Z0-9]+/g, " ")
    .trim()
    .split(/\s+/)
    .slice(0, 6)
    .map(w => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase())
    .join("");
  if (/guardar|save/i.test(text)) return "Common.Save";
  if (/cancelar|cancel/i.test(text)) return "Common.Cancel";
  if (/buscar|search/i.test(text)) return "Common.Search";
  if (/editar|edit/i.test(text)) return "Common.Edit";
  if (/eliminar|delete/i.test(text)) return "Common.Delete";
  if (/idioma|language/i.test(text)) return "Settings.Language";
  if (/iniciar ses/i.test(text)) return "Login.SignIn";
  if (/cerrar ses|sign out|salir/i.test(text)) return "Common.SignOut";
  if (/contrase|password/i.test(text)) return "Login.Password";
  if (/correo|email/i.test(text)) return "Users.Email";
  return `Ui.${base || "Text"}`;
}

const report = unique.map(item => ({
  ...item,
  suggestedKey: suggestKey(item.text)
}));

fs.mkdirSync("docs/i18n", { recursive: true });
fs.writeFileSync("docs/i18n/01_HARDCODED_STRINGS_SCAN.json", JSON.stringify(report, null, 2));
fs.writeFileSync("docs/i18n/01_HARDCODED_STRINGS_SCAN.md", [
  "# Hardcoded UI strings scan",
  "",
  `Total unique candidates: **${report.length}**`,
  "",
  "| File | Line | Text | Suggested key |",
  "|---|---:|---|---|",
  ...report.slice(0, 500).map(r => `| \`${r.file}\` | ${r.line} | ${r.text.replace(/\|/g, "\\|")} | \`${r.suggestedKey}\` |`)
].join("\n"));

console.log(`Scanned ${files.length} files → ${report.length} unique UI string candidates`);
console.log("Wrote docs/i18n/01_HARDCODED_STRINGS_SCAN.{json,md}");
