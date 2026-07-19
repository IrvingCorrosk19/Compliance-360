import psycopg2

conn = psycopg2.connect(
    host="localhost",
    port=5432,
    dbname="compliance360",
    user="postgres",
    password="Panama2020$",
)
cur = conn.cursor()
cur.execute(
    """
    SELECT name, setting
    FROM pg_settings
    WHERE name IN (
      'random_page_cost','work_mem','maintenance_work_mem',
      'effective_cache_size','default_statistics_target','effective_io_concurrency'
    )
    ORDER BY 1
    """
)
print("TUNABLES")
for r in cur.fetchall():
    print(r)

cur.execute(
    """
    SELECT tablename, indexname
    FROM pg_indexes
    WHERE schemaname='compliance360'
      AND tablename IN (
        'medical_device_products','registration_dossiers','sanitary_registrations',
        'manufacturer_profiles','manufacturer_certificates','operating_licenses',
        'dossier_requirements','authority_observations','regutrack_import_jobs'
      )
    ORDER BY 1, 2
    """
)
rows = cur.fetchall()
print(f"INDEXES {len(rows)}")
for t, i in rows:
    print(f"  {t}: {i}")

tenant = "82af3877-2786-4d39-bce8-c981101c771d"
cur.execute(
    f"""
    EXPLAIN (FORMAT TEXT)
    SELECT COUNT(1)
    FROM compliance360.medical_device_products
    WHERE "TenantId" = '{tenant}' AND NOT "IsDeleted"
    """
)
print("EXPLAIN products")
for (line,) in cur.fetchall():
    print(line)

cur.execute(
    f"""
    EXPLAIN (FORMAT TEXT)
    SELECT COUNT(1)
    FROM compliance360.registration_dossiers
    WHERE "TenantId" = '{tenant}' AND NOT "IsDeleted"
    """
)
print("EXPLAIN dossiers")
for (line,) in cur.fetchall():
    print(line)

conn.close()
