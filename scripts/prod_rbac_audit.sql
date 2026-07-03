\echo '== Roles with permission counts =='
select r."Name" as role, count(rp."PermissionId") as perms
from compliance360.roles r
left join compliance360.role_permissions rp on rp."RoleId" = r."Id"
group by r."Name"
order by r."Name";

\echo '== SoD: any role that can both CREATE and APPROVE documents (should be empty) =='
select r."Name"
from compliance360.roles r
where exists (select 1 from compliance360.role_permissions rp join compliance360.permissions p on p."Id"=rp."PermissionId" where rp."RoleId"=r."Id" and p."Code"='DOCUMENT.CREATE')
  and exists (select 1 from compliance360.role_permissions rp join compliance360.permissions p on p."Id"=rp."PermissionId" where rp."RoleId"=r."Id" and p."Code"='DOCUMENT.APPROVE');

\echo '== SoD: any role with BOTH storage admin and notification admin (should be empty) =='
select r."Name"
from compliance360.roles r
where exists (select 1 from compliance360.role_permissions rp join compliance360.permissions p on p."Id"=rp."PermissionId" where rp."RoleId"=r."Id" and p."Code" like 'STORAGE.%')
  and exists (select 1 from compliance360.role_permissions rp join compliance360.permissions p on p."Id"=rp."PermissionId" where rp."RoleId"=r."Id" and p."Code" like 'NOTIFICATION.%');

\echo '== Orphan permission grants (role_permissions pointing to nonexistent permission) =='
select count(*) as orphan_grants from compliance360.role_permissions rp
where not exists (select 1 from compliance360.permissions p where p."Id"=rp."PermissionId");

\echo '== Total permissions in catalog =='
select count(*) as total_permissions from compliance360.permissions;
