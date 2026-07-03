\pset pager off
select u."Email", u."Status", u."ForcePasswordChangeRequired", r."Name"
from compliance360.users u
left join compliance360.user_roles ur on ur."UserId" = u."Id"
left join compliance360.roles r on r."Id" = ur."RoleId"
where u."TenantId" = 'a82ff95e-c46f-4902-a7da-8aa26fd3f767';
