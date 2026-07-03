select "Id","Name","Slug","Status" from compliance360.tenants order by "Name";
select u."Email", u."TenantId", u."ForcePasswordChangeRequired", u."Status" from compliance360.users u order by u."Email";
