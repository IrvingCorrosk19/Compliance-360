-- Provision Support Operator user on platform tenant (idempotent)
insert into compliance360.users ("Id", "TenantId", "Email", "NormalizedEmail", "FullName", "PasswordHash", "Status", "ForcePasswordChangeRequired", "MfaEnabled", "AccessFailedCount", "CreatedAtUtc", "UpdatedAtUtc")
select gen_random_uuid(), 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626', 'support@compliance360.local', upper('support@compliance360.local'), 'Support Operator - Break Glass',
       (select "PasswordHash" from compliance360.users where "TenantId" = 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626' and "Email" = 'admin@compliance360.local' limit 1),
       'Active', false, false, 0, now() at time zone 'utc', now() at time zone 'utc'
where not exists (select 1 from compliance360.users where "TenantId" = 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626' and "Email" = 'support@compliance360.local');

insert into compliance360.user_roles ("Id", "TenantId", "UserId", "RoleId", "CreatedAtUtc")
select gen_random_uuid(), 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626', u."Id", r."Id", now() at time zone 'utc'
from compliance360.users u
cross join compliance360.roles r
where u."Email" = 'support@compliance360.local'
  and u."TenantId" = 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626'
  and r."TenantId" = 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626'
  and r."Name" = 'Support Operator'
  and not exists (
    select 1 from compliance360.user_roles ur
    where ur."TenantId" = u."TenantId" and ur."UserId" = u."Id" and ur."RoleId" = r."Id"
  );

update compliance360.users set "ForcePasswordChangeRequired" = false, "Status" = 'Active'
where "TenantId" = 'dc7c46ee-cb25-4ed5-b0b4-800788f7f626' and "Email" = 'support@compliance360.local';
