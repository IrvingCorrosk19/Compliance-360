import psycopg2

TENANT = "82af3877-2786-4d39-bce8-c981101c771d"
roles = [
    "Regulatory Administrator",
    "Regulatory Manager",
    "Regulatory Specialist",
    "Regulatory Reviewer",
    "Regulatory Approver",
    "Regulatory Submitter",
    "Regulatory Viewer",
    "Tenant Administrator",
    "Quality Manager",
]
conn = psycopg2.connect(
    host="localhost", port=5432, dbname="compliance360",
    user="postgres", password="Panama2020$",
    options="-c search_path=compliance360,public",
)
cur = conn.cursor()
for name in roles:
    cur.execute(
        """
        SELECT p."Code" FROM roles r
        JOIN role_permissions rp ON rp."RoleId"=r."Id"
        JOIN permissions p ON p."Id"=rp."PermissionId"
        WHERE r."TenantId"=%s AND r."Name"=%s AND p."Code" LIKE 'REGULATORY%%'
        ORDER BY 1
        """,
        (TENANT, name),
    )
    codes = [r[0] for r in cur.fetchall()]
    print(f"\n## {name} ({len(codes)})")
    for c in codes:
        print(" ", c)
conn.close()
