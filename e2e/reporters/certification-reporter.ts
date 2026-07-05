import type {
  FullConfig,
  FullResult,
  Reporter,
  Suite,
  TestCase,
  TestResult,
} from "@playwright/test/reporter";

class CertificationReporter implements Reporter {
  private currentRole = "";
  private currentScreen = "";
  private currentProcess = "";

  onBegin(_config: FullConfig, suite: Suite) {
    const total = suite.allTests().length;
    console.log("\n" + "=".repeat(48));
    console.log("COMPLIANCE 360 — FINAL FUNCTIONAL CERTIFICATION");
    console.log(`Total tests: ${total} | Mode: HEADFUL (observable browser)`);
    console.log("=".repeat(48) + "\n");
  }

  onTestBegin(test: TestCase) {
    const title = test.title;
    const suite = test.parent.title;
    this.currentRole = suite.replace(/ — .*/, "").replace(/^F\d+ — /, "");
    this.currentScreen = title.split(",")[0]?.trim() || title;
    this.currentProcess = suite;
    this.printBanner("Running");
  }

  onTestEnd(test: TestCase, result: TestResult) {
    const status = result.status === "passed" ? "PASS" : result.status === "skipped" ? "SKIP" : "FAIL";
    this.printBanner(status);
    if (status === "FAIL") {
      console.log(`  Error: ${result.error?.message?.split("\n")[0] ?? "unknown"}\n`);
    }
  }

  onEnd(result: FullResult) {
    console.log("\n" + "=".repeat(48));
    console.log(`CERTIFICATION RUN COMPLETE — ${result.status.toUpperCase()}`);
    console.log("=".repeat(48) + "\n");
  }

  private printBanner(estado: string) {
    console.log("=".repeat(48));
    console.log(`ROL:       ${this.currentRole}`);
    console.log(`Pantalla:  ${this.currentScreen}`);
    console.log(`Proceso:   ${this.currentProcess}`);
    console.log(`Estado:    ${estado}`);
    console.log(`Resultado: ${estado === "Running" ? "..." : estado}`);
    console.log("=".repeat(48));
  }
}

export default CertificationReporter;
