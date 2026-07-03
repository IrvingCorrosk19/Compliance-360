\pset pager off
select r."Id", r."Name"
from compliance360.roles r
where r."TenantId" = 'e5b5c5ed-b661-43ef-b706-6e10546553bc'
  and r."Name" in ('Document Controller','Quality Manager','Storage Administrator','Notification Administrator','Auditor')
order by r."Name";
