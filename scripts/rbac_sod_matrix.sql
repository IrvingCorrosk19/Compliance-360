\pset pager off
-- Effective permission grants per catalog role (this is exactly what feeds JWT claims).
-- Verifies Segregation of Duties invariants without needing interactive login.
with role_perms as (
  select r."Name" as role_name, p."Code" as code
  from compliance360.roles r
  join compliance360.role_permissions rp on rp."RoleId" = r."Id"
  join compliance360.permissions p on p."Id" = rp."PermissionId"
  where r."TenantId" = 'a82ff95e-c46f-4902-a7da-8aa26fd3f767'
)
select
  role_name,
  bool_or(code = 'DOCUMENT.CREATE')       as doc_create,
  bool_or(code = 'DOCUMENT.APPROVE')      as doc_approve,
  bool_or(code = 'CAPA.MANAGE')           as capa_manage,
  bool_or(code = 'CAPA.APPROVE')          as capa_approve,
  bool_or(code = 'CAPA.CLOSE')            as capa_close,
  bool_or(code = 'STORAGE.CREATE')        as storage_create,
  bool_or(code = 'NOTIFICATION.ADMIN')    as notif_admin
from role_perms
where role_name in ('Document Controller','Quality Manager','Auditor','CAPA Manager','Storage Administrator','Notification Administrator')
group by role_name
order by role_name;
