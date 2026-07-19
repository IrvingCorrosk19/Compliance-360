/* Responsive audit: valida overflow horizontal, drawer movil y card-view de tablas
   en multiples resoluciones. Uso: node scripts/responsive-audit.js */
const { chromium } = require("@playwright/test");
const fs = require("fs");
const path = require("path");

const BASE = "http://localhost:5272";
const DATA = JSON.parse(fs.readFileSync(path.join(__dirname, "..", "testdata.json"), "utf-8").replace(/^\uFEFF/, ""));
const OUT = path.join(__dirname, "..", "..", "artifacts", "responsive");
fs.mkdirSync(OUT, { recursive: true });

const VIEWPORTS = [
  { name: "mobile-360", width: 360, height: 800 },
  { name: "iphone-390", width: 390, height: 844 },
  { name: "mobile-lg-480", width: 480, height: 900 },
  { name: "tablet-768", width: 768, height: 1024 },
  { name: "tablet-ls-1024", width: 1024, height: 768 },
  { name: "laptop-1366", width: 1366, height: 768 },
  { name: "desktop-1920", width: 1920, height: 1080 },
  { name: "2k-2560", width: 2560, height: 1440 }
];

const results = [];
function record(viewport, check, pass, detail = "") {
  results.push({ viewport, check, pass, detail });
  console.log(`${pass ? "PASS" : "FAIL"} [${viewport}] ${check}${detail ? " - " + detail : ""}`);
}

async function noHorizontalOverflow(page, viewport, label) {
  const metrics = await page.evaluate(() => ({
    scrollWidth: document.documentElement.scrollWidth,
    innerWidth: window.innerWidth,
    bodyScroll: document.body.scrollWidth
  }));
  const pass = metrics.scrollWidth <= metrics.innerWidth + 1 && metrics.bodyScroll <= metrics.innerWidth + 1;
  record(viewport, `sin scroll horizontal (${label})`, pass, pass ? "" : JSON.stringify(metrics));
}

async function login(page) {
  await page.goto(BASE + "/");
  await page.evaluate(() => { localStorage.clear(); sessionStorage.clear(); });
  await page.reload();
  await page.waitForSelector("#login-form", { timeout: 20000 });
  await page.fill("#email", DATA.users[0].email);
  await page.click("#login-form button[type=submit]");
  const orgRadio = page.locator('input[name="organizationId"]');
  const passwordField = page.locator("#password");
  await Promise.race([
    orgRadio.first().waitFor({ state: "visible", timeout: 20000 }).catch(() => {}),
    passwordField.waitFor({ state: "visible", timeout: 20000 }).catch(() => {})
  ]);
  if (await orgRadio.count()) {
    await orgRadio.first().check();
    await page.click("#login-form button[type=submit]");
  }
  await passwordField.waitFor({ state: "visible", timeout: 20000 });
  await page.fill("#password", DATA.users[0].password);
  await page.click("#login-form button[type=submit]");
  await page.waitForSelector("aside.sidebar", { timeout: 45000 });
  await page.waitForTimeout(1500);
}

(async () => {
  const browser = await chromium.launch({ headless: true });

  for (const vp of VIEWPORTS) {
    const context = await browser.newContext({ viewport: { width: vp.width, height: vp.height } });
    const page = await context.newPage();
    const isMobile = vp.width < 768;
    const isTablet = vp.width >= 768 && vp.width < 1024;

    // 1) Login page
    await page.goto(BASE + "/");
    await page.waitForSelector("#login-form", { timeout: 20000 });
    await noHorizontalOverflow(page, vp.name, "login");
    await page.screenshot({ path: path.join(OUT, `${vp.name}-login.png`), fullPage: false });

    // 2) Shell + dashboard
    await login(page);
    await noHorizontalOverflow(page, vp.name, "dashboard");
    await page.screenshot({ path: path.join(OUT, `${vp.name}-dashboard.png`), fullPage: false });

    // 3) Sidebar behavior
    if (isMobile) {
      const toggleVisible = await page.locator("#menu-toggle").isVisible();
      record(vp.name, "hamburguesa visible", toggleVisible);
      const sidebarHidden = await page.evaluate(() => {
        const sb = document.querySelector("#app-sidebar");
        return sb.getBoundingClientRect().right <= 0;
      });
      record(vp.name, "sidebar oculto por defecto (drawer)", sidebarHidden);

      await page.click("#menu-toggle");
      await page.waitForTimeout(400);
      const drawerOpen = await page.evaluate(() => document.querySelector(".layout").classList.contains("drawer-open"));
      record(vp.name, "drawer abre con hamburguesa", drawerOpen);
      await page.screenshot({ path: path.join(OUT, `${vp.name}-drawer.png`) });

      await page.keyboard.press("Escape");
      await page.waitForTimeout(400);
      const closedByEsc = await page.evaluate(() => !document.querySelector(".layout").classList.contains("drawer-open"));
      record(vp.name, "drawer cierra con ESC", closedByEsc);

      await page.click("#menu-toggle");
      await page.waitForTimeout(400);
      await page.click("#sidebar-close");
      await page.waitForTimeout(400);
      const closedByX = await page.evaluate(() => !document.querySelector(".layout").classList.contains("drawer-open"));
      record(vp.name, "drawer cierra con X", closedByX);

      await page.click("#menu-toggle");
      await page.waitForTimeout(400);
      await page.mouse.click(vp.width - 10, vp.height / 2);
      await page.waitForTimeout(400);
      const closedByOutside = await page.evaluate(() => !document.querySelector(".layout").classList.contains("drawer-open"));
      record(vp.name, "drawer cierra tocando fuera", closedByOutside);
    } else if (isTablet) {
      const railWidth = await page.evaluate(() => document.querySelector("#app-sidebar").getBoundingClientRect().width);
      record(vp.name, "sidebar rail colapsado en tablet", railWidth < 120, `width=${Math.round(railWidth)}`);
    } else {
      const sidebarWidth = await page.evaluate(() => document.querySelector("#app-sidebar").getBoundingClientRect().width);
      record(vp.name, "sidebar completo en desktop", sidebarWidth >= 240, `width=${Math.round(sidebarWidth)}`);
    }

    // 4) Regulatory module (tables)
    await page.evaluate(() => { location.hash = "#/regulatory"; });
    await page.waitForTimeout(3500);
    await noHorizontalOverflow(page, vp.name, "regulatory");
    await page.screenshot({ path: path.join(OUT, `${vp.name}-regulatory.png`), fullPage: false });

    // 5) Tenant administration
    await page.evaluate(() => { location.hash = "#/tenant-administration"; });
    await page.waitForTimeout(3500);
    await noHorizontalOverflow(page, vp.name, "tenant-admin");
    await page.screenshot({ path: path.join(OUT, `${vp.name}-tenant-admin.png`), fullPage: false });

    if (isMobile) {
      const dataLabelCount = await page.evaluate(() => document.querySelectorAll("td[data-label]").length);
      record(vp.name, "tablas con data-label (card view)", dataLabelCount > 0, `celdas=${dataLabelCount}`);
    }

    await context.close();
  }

  await browser.close();
  const failed = results.filter(r => !r.pass);
  fs.writeFileSync(path.join(OUT, "responsive-audit-results.json"), JSON.stringify(results, null, 2));
  console.log(`\nTOTAL: ${results.length} checks, ${failed.length} fallos`);
  process.exit(failed.length ? 1 : 0);
})();
