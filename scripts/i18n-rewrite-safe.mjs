/**
 * SAFE i18n rewrite: only replaces text nodes between tags and toast()/aria/placeholder/title
 * when the literal exactly matches a locale value. Never touches class/id attributes.
 */
import fs from "fs";
import path from "path";

const root = process.cwd();
const es = JSON.parse(fs.readFileSync(path.join(root, "src/Compliance360.Web/wwwroot/locales/es.json"), "utf8"));
const en = JSON.parse(fs.readFileSync(path.join(root, "src/Compliance360.Web/wwwroot/locales/en.json"), "utf8"));

const byText = new Map();
for (const [key, value] of Object.entries(es)) {
  if (typeof value === "string" && value.length >= 3) byText.set(value, key);
}
for (const [key, value] of Object.entries(en)) {
  if (typeof value === "string" && value.length >= 3 && !byText.has(value)) byText.set(value, key);
}

const pairs = [...byText.entries()]
  .filter(([text]) => !/^[a-z0-9._-]+$/i.test(text))
  .sort((a, b) => b[0].length - a[0].length);

function escapeRegExp(s) {
  return s.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function rewrite(filePath) {
  let src = fs.readFileSync(filePath, "utf8");
  let count = 0;
  for (const [text, key] of pairs) {
    const lit = escapeRegExp(text);

    // Text between tags: >text<
    src = src.replace(new RegExp(`>(${lit})<`, "g"), (m) => {
      count++;
      return `>\${t("${key}")}<`;
    });

    // placeholder/title/aria-label="text"
    src = src.replace(new RegExp(`((?:placeholder|title|aria-label)=)(["'])(${lit})\\2`, "g"), (_m, attr, q) => {
      count++;
      return `${attr}"\${t("${key}")}"`;
    });

    // toast("text" / toast('text'
    src = src.replace(new RegExp(`(toast\\(\\s*)(["'])(${lit})\\2`, "g"), (_m, fn) => {
      count++;
      return `${fn}t("${key}")`;
    });
  }
  fs.writeFileSync(filePath, src);
  return count;
}

const files = [
  "src/Compliance360.Web/wwwroot/app.js",
  "src/Compliance360.Web/wwwroot/regulatory-affairs.js"
];

const results = {};
for (const f of files) {
  const full = path.join(root, f);
  if (fs.existsSync(full)) results[f] = rewrite(full);
}
console.log(results);
