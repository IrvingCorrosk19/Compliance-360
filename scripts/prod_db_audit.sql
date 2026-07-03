\echo '== Applied migrations =='
select "MigrationId" from "__EFMigrationsHistory" order by "MigrationId";

\echo '== Table count in schema =='
select count(*) as tables from information_schema.tables where table_schema = 'compliance360';

\echo '== Foreign keys =='
select count(*) as foreign_keys from information_schema.table_constraints where table_schema = 'compliance360' and constraint_type = 'FOREIGN KEY';

\echo '== Indexes =='
select count(*) as indexes from pg_indexes where schemaname = 'compliance360';

\echo '== Unique constraints =='
select count(*) as unique_constraints from information_schema.table_constraints where table_schema = 'compliance360' and constraint_type = 'UNIQUE';

\echo '== Tenant-scoped tables missing TenantId (should be only global/system tables) =='
select t.table_name
from information_schema.tables t
where t.table_schema = 'compliance360'
  and t.table_type = 'BASE TABLE'
  and not exists (
    select 1 from information_schema.columns c
    where c.table_schema = 'compliance360' and c.table_name = t.table_name and c.column_name = 'TenantId'
  )
order by t.table_name;
