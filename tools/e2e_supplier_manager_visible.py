from e2e_visible_role_runner import run_role


if __name__ == "__main__":
    raise SystemExit(run_role(
        role_key="supplier-manager",
        role_name="Supplier Manager",
        report_file="06_SUPPLIER_MANAGER_E2E_REPORT.md",
        email="supplier.manager@alimentos-premium.test",
        password="SupplierMgr!2026",
        steps=[
            {"name": "Abrir Supplier Management", "route": "suppliers", "expect": "Supplier Management", "create": True, "title": "Proveedor BPM E2E", "code_prefix": "RUC-SUP", "description": "Proveedor visible E2E"},
            {"name": "Crear CAPA proveedor", "route": "capa", "expect": "CAPA", "create": True, "title": "CAPA Proveedor E2E", "code_prefix": "SUP-CAPA", "description": "Seguimiento proveedor E2E"},
            {"name": "Ver auditoria", "route": "audit-trail", "expect": "Audit Trail"},
        ],
    ))
