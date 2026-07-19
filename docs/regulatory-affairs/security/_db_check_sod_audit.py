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
    'SELECT "TenantId", "PreventSelfReview", "PreventSelfApproval", '
    '"SeparateApproverAndSubmitter", "RequireInternalApprovalBeforeSubmission" '
    "FROM regulatory_sod_settings"
)
print("sod_settings:", cur.fetchall())
cur.execute(
    'SELECT "Action"::text, COUNT(*) FROM audit_logs '
    'WHERE "Action"::text LIKE \'%RegulatorySoD%\' OR "Action"::text LIKE \'%InternalApproval%\' '
    "OR \"Action\"::int >= 99 GROUP BY 1 ORDER BY 2 DESC LIMIT 20"
)
try:
    print("audits:", cur.fetchall())
except Exception as ex:
    print("audit query failed", ex)
    cur.execute(
        "SELECT data_type FROM information_schema.columns "
        "WHERE table_name='audit_logs' AND column_name='Action'"
    )
    print("action type", cur.fetchall())
    cur.execute(
        'SELECT "Action", COUNT(*) FROM audit_logs GROUP BY 1 ORDER BY 2 DESC LIMIT 15'
    )
    print(cur.fetchall())
conn.close()
