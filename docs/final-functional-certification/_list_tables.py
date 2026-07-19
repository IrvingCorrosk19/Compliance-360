import psycopg2

c = psycopg2.connect(
    host="localhost", port=5432, dbname="compliance360", user="postgres", password="Panama2020$"
)
cur = c.cursor()
cur.execute(
    """
    SELECT table_schema, table_name
    FROM information_schema.tables
    WHERE table_type='BASE TABLE'
      AND (
        table_name ILIKE '%medical%'
        OR table_name ILIKE '%sanitary%'
        OR table_name ILIKE '%dossier%'
        OR table_name ILIKE '%regulat%'
        OR table_name ILIKE '%operating%'
        OR table_name ILIKE '%manufacturer%'
      )
    ORDER BY 1, 2
    """
)
for a, b in cur.fetchall():
    print(f"{a}.{b}")
c.close()
