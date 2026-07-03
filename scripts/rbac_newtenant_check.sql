\pset pager off
select r."Name", count(rp."PermissionId") as grants
from compliance360.roles r
left join compliance360.role_permissions rp on rp."RoleId" = r."Id"
where r."TenantId" = 'e5b5c5ed-b661-43ef-b706-6e10546553bc'
group by r."Name"
order by r."Name";
