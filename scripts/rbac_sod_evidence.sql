\echo '== SoD matrix for a sample business tenant =='
with sample as (
  select "TenantId" as tid
  from compliance360.roles
  where "TenantId" is not null
    and "TenantId" <> 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626'
  group by "TenantId"
  order by count(*) desc
  limit 1
),
role_perms as (
  select r."Name" as role_name, p."Code" as code
  from compliance360.roles r
  join compliance360.role_permissions rp on rp."RoleId" = r."Id"
  join compliance360.permissions p on p."Id" = rp."PermissionId"
  where r."TenantId" = (select tid from sample)
)
select
  role_name,
  bool_or(code = 'DOCUMENT.CREATE')    as doc_create,
  bool_or(code = 'DOCUMENT.APPROVE')   as doc_approve,
  bool_or(code = 'CAPA.MANAGE')        as capa_manage,
  bool_or(code = 'CAPA.APPROVE')       as capa_approve,
  bool_or(code = 'CAPA.CLOSE')         as capa_close,
  bool_or(code = 'STORAGE.CREATE')     as storage_create,
  bool_or(code = 'NOTIFICATION.ADMIN') as notif_admin
from role_perms
where role_name in ('Document Controller','Quality Manager','Auditor','CAPA Manager','Storage Administrator','Notification Administrator','Viewer')
group by role_name
order by role_name;
