from e2e_visible_role_runner import run_role


if __name__ == "__main__":
    raise SystemExit(run_role(
        role_key="storage-admin",
        role_name="Storage Admin",
        report_file="11_STORAGE_ADMIN_E2E_REPORT.md",
        email="storage.admin@alimentos-premium.test",
        password="StorageAdmin!2026",
        steps=[
            {
                "name": "Crear y probar storage local",
                "route": "configuration",
                "expect": "Provider Administration",
                "clicks": [
                    {"selector": "#create-storage-provider", "expect": "Storage provider"},
                    {"selector": "#test-first-storage-provider", "expect": "Storage provider test ejecutado"},
                ],
            },
            {"name": "Ver auditoria", "route": "audit-trail", "expect": "Audit Trail"},
        ],
    ))
