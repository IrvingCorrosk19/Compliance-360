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
cur.execute("SELECT COUNT(*) FROM notification_messages")
print("notification_messages_count", cur.fetchone()[0])
cur.execute(
    'SELECT "Subject", "Channel", "Status", "CreatedAtUtc" '
    'FROM notification_messages ORDER BY "CreatedAtUtc" DESC LIMIT 8'
)
for row in cur.fetchall():
    print(row)
conn.close()
