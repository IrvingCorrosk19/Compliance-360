import psycopg2

conn = psycopg2.connect(
    host="localhost", port=5432, dbname="compliance360",
    user="postgres", password="Panama2020$"
)
cur = conn.cursor()

print("=== TABLES ===")
cur.execute("""
SELECT tablename FROM pg_tables
WHERE tablename IN ('regulatory_sod_settings','registration_dossiers','dossier_requirements')
ORDER BY 1
""")
for r in cur.fetchall():
    print(r[0])

print("=== regulatory_sod_settings columns ===")
cur.execute("""
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'regulatory_sod_settings'
ORDER BY ordinal_position
""")
for r in cur.fetchall():
    print(f"{r[0]} | {r[1]} | null={r[2]}")

print("=== sod indexes ===")
cur.execute("SELECT indexname, indexdef FROM pg_indexes WHERE tablename='regulatory_sod_settings'")
for r in cur.fetchall():
    print(r[0], "=>", r[1])

print("=== dossier SoD columns ===")
cur.execute("""
SELECT column_name, data_type FROM information_schema.columns
WHERE table_name='registration_dossiers'
  AND column_name ILIKE '%internal%'
ORDER BY 1
""")
for r in cur.fetchall():
    print(r)

print("=== requirement SoD columns ===")
cur.execute("""
SELECT column_name, data_type FROM information_schema.columns
WHERE table_name='dossier_requirements'
  AND column_name ILIKE '%last%'
ORDER BY 1
""")
for r in cur.fetchall():
    print(r)

print("=== row counts (data intact) ===")
for t in ["registration_dossiers", "sanitary_registrations", "medical_device_products", "roles", "role_permissions"]:
    cur.execute(f'SELECT COUNT(*) FROM "{t}"' if False else f"SELECT COUNT(*) FROM {t}")
    print(t, cur.fetchone()[0])

print("=== migrations history (tail) ===")
cur.execute('SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY 1 DESC LIMIT 5')
for r in cur.fetchall():
    print(r[0])

conn.close()
print("OK")
