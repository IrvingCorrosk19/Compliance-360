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
for table in ("notification_messages", "notifications"):
    cur.execute(
        "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name=%s)",
        (table,),
    )
    print(table, "exists", cur.fetchone()[0])
cur.execute(
    """
    SELECT table_name FROM information_schema.tables
    WHERE table_schema='compliance360' AND table_name ILIKE '%notif%'
    ORDER BY 1
    """
)
print("notif tables:", cur.fetchall())
conn.close()
