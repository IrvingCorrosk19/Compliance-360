from e2e_visible_role_runner import run_role


if __name__ == "__main__":
    raise SystemExit(run_role(
        role_key="auditor",
        role_name="Auditor",
        report_file="05_AUDITOR_E2E_REPORT.md",
        email="auditor@alimentos-premium.test",
        password="AuditorRole!2026",
        steps=[
            {"name": "Abrir Audit Management", "route": "audits", "expect": "Audit Management", "create": True, "title": "Auditoria BPM E2E", "code_prefix": "AUD-E2E", "description": "Auditoria visible E2E"},
            {"name": "Crear CAPA desde hallazgo", "route": "capa", "expect": "CAPA", "create": True, "title": "CAPA Hallazgo Auditor E2E", "code_prefix": "AUD-CAPA", "description": "CAPA originada en auditoria E2E"},
            {"name": "Ver auditoria", "route": "audit-trail", "expect": "Audit Trail"},
        ],
    ))
