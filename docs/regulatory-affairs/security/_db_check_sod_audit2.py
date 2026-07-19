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
print("sod_settings OK", cur.execute(
    'SELECT "TenantId","PreventSelfReview","SeparateApproverAndSubmitter","RequireInternalApprovalBeforeSubmission" FROM regulatory_sod_settings'
) or cur.fetchall())
cur.execute('SELECT "Action", COUNT(*) FROM audit_logs WHERE "Action" IN (91,92,93,94,95,99,100,101) GROUP BY 1 ORDER BY 1')
print("audit action counts (recent codes):", cur.fetchall())
cur.execute(
    'SELECT "Action","EntityName","OccurredAtUtc" FROM audit_logs WHERE "Action" IN (99,100,101) ORDER BY "OccurredAtUtc" DESC LIMIT 10'
)
print("sod/internal audits:")
for r in cur.fetchall():
    print(r)
cur.execute(
    'SELECT "CaseNumber","Status","InternallyApprovedByUserId" IS NOT NULL AS has_internal '
    'FROM registration_dossiers WHERE "Status"::text LIKE \'%%Approved%%\' OR "InternallyApprovedByUserId" IS NOT NULL '
    'ORDER BY "UpdatedAtUtc" DESC NULLS LAST LIMIT 8'
)
print("dossier statuses sample:")
for r in cur.fetchall():
    print(r)
conn.close()
