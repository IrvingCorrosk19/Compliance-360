\echo '== Global counts =='
select
  (select count(*) from compliance360.permissions) as permissions,
  (select count(*) from compliance360.roles) as roles,
  (select count(*) from compliance360.role_permissions) as grants,
  (select count(*) from compliance360.user_roles) as user_roles;

\echo '== Tenants that have provisioned roles =='
select count(distinct "TenantId") as tenants_with_roles
from compliance360.roles
where "TenantId" is not null;

\echo '== Legacy residue (must all be 0) =='
select
  (select count(*) from compliance360.permissions
     where "Code" in ('DOCUMENT.MANAGE','SUPPLIER.MANAGE','WORKFLOW.MANAGE','TECHNICALSHEET.MANAGE','STORAGE.MANAGE','AUDIT.MANAGE')) as legacy_monolithic,
  (select count(*) from compliance360.permissions where "Code" like 'SUPERADMIN.%') as superadmin_codes,
  (select count(*) from compliance360.permissions where "Code" like 'QUALITY\_E2E%' escape '\' or "Code" like 'API\_RBAC%' escape '\' or "Code" like 'API\_DEBUG%' escape '\') as invented_codes;

\echo '== Duplicate permission codes (must be empty) =='
select "Code", count(*)
from compliance360.permissions
group by "Code" having count(*) > 1;
