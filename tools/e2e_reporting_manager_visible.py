from e2e_visible_role_runner import run_role


if __name__ == "__main__":
    raise SystemExit(run_role(
        role_key="reporting-manager",
        role_name="Reporting Manager",
        report_file="10_REPORTING_MANAGER_E2E_REPORT.md",
        email="reporting.manager@alimentos-premium.test",
        password="ReportingMgr!2026",
        steps=[
            {"name": "Abrir Report Center", "route": "reports", "expect": "Reports"},
            {"name": "Consultar indicadores", "route": "indicators", "expect": "Indicators"},
            {"name": "Consultar riesgos", "route": "risks", "expect": "Risk Management"},
            {"name": "Ver auditoria", "route": "audit-trail", "expect": "Audit Trail"},
        ],
    ))
