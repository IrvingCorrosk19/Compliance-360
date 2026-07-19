const path = require("path");

/** Isolated config for offline user-manual tests (no localhost app required). */
module.exports = {
  testDir: "./tests",
  testMatch: "**/user-manual.spec.js",
  timeout: 60_000,
  fullyParallel: false,
  workers: 1,
  reporter: [
    ["list"],
    ["json", { outputFile: path.join(__dirname, "../docs/user-manual/docs/manual-playwright-results.json") }],
  ],
  use: {
    headless: true,
    screenshot: "off",
    video: "off",
    trace: "off",
  },
  projects: [{ name: "chromium", use: { channel: "chrome" } }],
};
