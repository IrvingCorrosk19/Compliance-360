from e2e_visible_role_runner import run_role


if __name__ == "__main__":
    raise SystemExit(run_role(
        role_key="indicators-manager",
        role_name="Indicators Manager",
        report_file="09_INDICATORS_MANAGER_E2E_REPORT.md",
        email="indicators.manager@alimentos-premium.test",
        password="IndicatorsMgr!2026",
        steps=[
            {"name": "Crear indicador", "route": "indicators", "expect": "Indicators", "create": True, "title": "Indicador BPM E2E", "code_prefix": "KPI-E2E", "description": "Formula, meta, umbrales y tendencia E2E"},
            {"name": "Consultar reportes", "route": "reports", "expect": "Reports"},
            {"name": "Consultar riesgos", "route": "risks", "expect": "Risk Management"},
            {"name": "Ver auditoria", "route": "audit-trail", "expect": "Audit Trail"},
        ],
    ))
