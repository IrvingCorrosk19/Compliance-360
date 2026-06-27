from e2e_visible_role_runner import run_role


if __name__ == "__main__":
    raise SystemExit(run_role(
        role_key="capa-manager",
        role_name="CAPA Manager",
        report_file="07_CAPA_MANAGER_E2E_REPORT.md",
        email="capa.manager@alimentos-premium.test",
        password="CapaMgr!2026",
        steps=[
            {"name": "Crear CAPA", "route": "capa", "expect": "CAPA", "create": True, "title": "CAPA E2E", "code_prefix": "CAPA-E2E", "description": "Causa raiz, accion correctiva y evidencia E2E"},
            {"name": "Consultar riesgos relacionados", "route": "risks", "expect": "Risk Management"},
            {"name": "Consultar indicadores", "route": "indicators", "expect": "Indicators"},
            {"name": "Ver auditoria", "route": "audit-trail", "expect": "Audit Trail"},
        ],
    ))
