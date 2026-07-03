\pset pager off
\echo === distinct permission codes ===
select "Code" from compliance360.permissions order by "Code";
\echo === roles ===
select r."TenantId", r."Name", count(rp."PermissionId") as grants
from compliance360.roles r
left join compliance360.role_permissions rp on rp."RoleId" = r."Id"
group by r."TenantId", r."Name"
order by r."TenantId", r."Name";
\echo === users with roles ===
select ur."TenantId", ur."UserId", r."Name"
from compliance360.user_roles ur
join compliance360.roles r on r."Id" = ur."RoleId"
order by ur."TenantId";
