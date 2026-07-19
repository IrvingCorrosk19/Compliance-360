import psycopg2

TENANT = "82af3877-2786-4d39-bce8-c981101c771d"
conn = psycopg2.connect(
    host="localhost", port=5432, dbname="compliance360",
    user="postgres", password="Panama2020$",
    options="-c search_path=compliance360,public",
)
cur = conn.cursor()
print("=== RA roles Irving ===")
cur.execute(
    """
    SELECT r."Name", COUNT(rp."PermissionId") AS grants
    FROM roles r
    LEFT JOIN role_permissions rp ON rp."RoleId"=r."Id" AND rp."TenantId"=r."TenantId"
    WHERE r."TenantId"=%s AND r."Name" ILIKE 'Regulatory%%'
    GROUP BY r."Name"
    ORDER BY 1
    """,
    (TENANT,),
)
for r in cur.fetchall():
    print(r)

print("=== critical perms present? ===")
cur.execute(
    """
    SELECT "Code" FROM permissions
    WHERE "Code" LIKE 'REGULATORY.DOSSIER.%%' OR "Code" LIKE 'REGULATORY.SOD.%%'
    ORDER BY 1
    """
)
for r in cur.fetchall():
    print(r[0])

print("=== TAC grants (sample) ===")
cur.execute(
    """
    SELECT p."Code"
    FROM roles r
    JOIN role_permissions rp ON rp."RoleId"=r."Id"
    JOIN permissions p ON p."Id"=rp."PermissionId"
    WHERE r."TenantId"=%s AND r."Name"='Tenant Administrator' AND p."Code" LIKE 'REGULATORY%%'
    ORDER BY 1
    """,
    (TENANT,),
)
for r in cur.fetchall():
    print(r[0])
conn.close()
