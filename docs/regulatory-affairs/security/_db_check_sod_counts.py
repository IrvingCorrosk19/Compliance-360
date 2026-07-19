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
for t in [
    "registration_dossiers",
    "sanitary_registrations",
    "medical_device_products",
    "roles",
    "role_permissions",
]:
    cur.execute(f"SELECT COUNT(*) FROM {t}")
    print(t, cur.fetchone()[0])
cur.execute('SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY 1 DESC LIMIT 5')
print("migrations_tail:")
for r in cur.fetchall():
    print(" ", r[0])
cur.execute("SELECT COUNT(*) FROM regulatory_sod_settings")
print("sod_settings_rows", cur.fetchone()[0])
conn.close()
