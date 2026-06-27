from e2e_visible_role_runner import run_role


if __name__ == "__main__":
    raise SystemExit(run_role(
        role_key="risk-manager",
        role_name="Risk Manager",
        report_file="08_RISK_MANAGER_E2E_REPORT.md",
        email="risk.manager@alimentos-premium.test",
        password="RiskMgr!2026",
        steps=[
            {"name": "Crear riesgo", "route": "risks", "expect": "Risk Management", "create": True, "title": "Riesgo Inocuidad E2E", "code_prefix": "RISK-E2E", "description": "Probabilidad, impacto, tratamiento y control E2E"},
            {"name": "Consultar CAPA", "route": "capa", "expect": "CAPA"},
            {"name": "Consultar reportes", "route": "reports", "expect": "Reports"},
            {"name": "Ver auditoria", "route": "audit-trail", "expect": "Audit Trail"},
        ],
    ))
