update compliance360.tenants
set "RequireMfa" = false
where "Id" = 'a82ff95e-c46f-4902-a7da-8aa26fd3f767';
update compliance360.users
set "ForcePasswordChangeRequired" = false
where "TenantId" = 'a82ff95e-c46f-4902-a7da-8aa26fd3f767';
