import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./tests",
  timeout: 60_000,
  expect: { timeout: 10_000 },
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: [
    ["list"],
    ["html", { outputFolder: "../artifacts/e2e/html-report", open: "never" }],
    ["json", { outputFile: "../artifacts/e2e/results.json" }],
  ],
  outputDir: "../artifacts/e2e/test-output",
  use: {
    baseURL: "http://localhost:5272",
    headless: true,
    screenshot: "on",
    video: "on",
    trace: "on",
    actionTimeout: 15_000,
  },
  projects: [
    { name: "chromium", use: { ...devices["Desktop Chrome"], channel: "chrome" } },
  ],
});
