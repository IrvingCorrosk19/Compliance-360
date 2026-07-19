import psycopg2

conn = psycopg2.connect(
    host="localhost",
    port=5432,
    dbname="compliance360",
    user="postgres",
    password="Panama2020$",
    options="-c search_path=compliance360,public",
)
cur = conn.cursor()
cur.execute(
    """
    SELECT "Action", COUNT(*)
    FROM audit_logs
    WHERE "Action" ILIKE '%Regulatory%' OR "Action" ILIKE '%SoD%' OR "Action" ILIKE '%Internal%'
    GROUP BY 1
    ORDER BY 2 DESC
    LIMIT 30
    """
)
print("actions:")
for r in cur.fetchall():
    print(r)

cur.execute(
    """
    SELECT "Action", "EntityName", "OccurredAtUtc"
    FROM audit_logs
    WHERE "Action" ILIKE '%SoD%' OR "Action" ILIKE '%InternalApproval%'
    ORDER BY "OccurredAtUtc" DESC
    LIMIT 15
    """
)
print("recent sod/internal:")
for r in cur.fetchall():
    print(r)

cur.execute(
    """
    SELECT "CaseNumber", "Status", "InternallyApprovedByUserId"
    FROM registration_dossiers
    WHERE "InternallyApprovedByUserId" IS NOT NULL
    ORDER BY "UpdatedAtUtc" DESC NULLS LAST
    LIMIT 8
    """
)
print("internal approvals persisted:")
for r in cur.fetchall():
    print(r)
conn.close()
