-- Purge all tenants except platform bootstrap (Compliance 360 / compliance360)
-- Keeps: dc7c46ee-cb25-4ed5-b0b4-800788f7f626 + admin@compliance360.local
\set ON_ERROR_STOP on
BEGIN;

DO $$
DECLARE
  keep CONSTANT uuid := 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626';
  r record;
  sql text;
  deleted bigint;
BEGIN
  PERFORM set_config('session_replication_role', 'replica', true);

  FOR r IN
    SELECT c.table_name
    FROM information_schema.columns c
    WHERE c.table_schema = 'compliance360'
      AND c.column_name = 'TenantId'
      AND c.table_name <> 'tenants'
    ORDER BY c.table_name
  LOOP
    sql := format('DELETE FROM compliance360.%I WHERE "TenantId" <> %L', r.table_name, keep);
    EXECUTE sql;
    GET DIAGNOSTICS deleted = ROW_COUNT;
    RAISE NOTICE 'Deleted % rows from %', deleted, r.table_name;
  END LOOP;

  DELETE FROM compliance360.tenants WHERE "Id" <> keep;
  GET DIAGNOSTICS deleted = ROW_COUNT;
  RAISE NOTICE 'Deleted % tenant rows', deleted;

  PERFORM set_config('session_replication_role', 'origin', true);
END $$;

-- Validation (must pass before commit)
DO $$
DECLARE
  t_count int;
  u_count int;
  admin_email text;
BEGIN
  SELECT count(*) INTO t_count FROM compliance360.tenants;
  IF t_count <> 1 THEN
    RAISE EXCEPTION 'Expected 1 tenant, found %', t_count;
  END IF;

  SELECT count(*) INTO u_count FROM compliance360.users WHERE "TenantId" = 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626';
  IF u_count < 1 THEN
    RAISE EXCEPTION 'Platform tenant has no users (found %)', u_count;
  END IF;

  SELECT "Email" INTO admin_email FROM compliance360.users
  WHERE "TenantId" = 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626' AND "Email" = 'admin@compliance360.local'
  LIMIT 1;
  IF admin_email IS NULL THEN
    RAISE EXCEPTION 'Platform admin admin@compliance360.local not found';
  END IF;
END $$;

COMMIT;

SELECT "Id", "Name", "Slug", "Status"::text FROM compliance360.tenants;
SELECT count(*) AS tenant_count FROM compliance360.tenants;
SELECT "Email", "TenantId" FROM compliance360.users ORDER BY "Email";
