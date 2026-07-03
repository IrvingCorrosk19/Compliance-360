\pset pager off
select u."Email", u."TenantId", u."Status"
from compliance360.users u
where u."TenantId" = 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626';
\echo === user_roles ===
select ur."TenantId", ur."UserId", r."Name"
from compliance360.user_roles ur
join compliance360.roles r on r."Id" = ur."RoleId";
