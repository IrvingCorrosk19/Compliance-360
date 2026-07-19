/** @type {import('@playwright/test').PlaywrightTestConfig} */
const path = require("path");

module.exports = {
  testDir: "./",
  timeout: 60_000,
  fullyParallel: false,
  workers: 1,
  reporter: [["list"], ["json", { outputFile: path.join(__dirname, "../docs/manual-playwright-results.json") }]],
  use: {
    headless: true,
    screenshot: "off",
    video: "off",
    trace: "off",
  },
  projects: [{ name: "chromium", use: { channel: "chrome" } }],
};
