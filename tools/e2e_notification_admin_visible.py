from e2e_visible_role_runner import run_role


if __name__ == "__main__":
    raise SystemExit(run_role(
        role_key="notification-admin",
        role_name="Notification Admin",
        report_file="12_NOTIFICATION_ADMIN_E2E_REPORT.md",
        email="notification.admin@alimentos-premium.test",
        password="NotificationAdmin!2026",
        steps=[
            {
                "name": "Configurar SMTP de prueba",
                "route": "configuration",
                "expect": "Provider Administration",
                "clicks": [
                    {"selector": "#create-email-provider", "expect": "Email provider"},
                ],
            },
            {"name": "Ver auditoria", "route": "audit-trail", "expect": "Audit Trail"},
        ],
    ))
