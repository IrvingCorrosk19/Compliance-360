-- =============================================================================
-- Compliance 360 - Enterprise RBAC Rebuild - DEVELOPMENT RESET
-- =============================================================================
-- Removes ALL legacy RBAC data (monolithic permissions, invented E2E codes,
-- SUPERADMIN.* codes, ad-hoc E2E roles, temporary grants and assignments).
-- The application's RBAC provisioning service rebuilds the official catalog
-- (permissions + platform/tenant roles) and re-assigns the platform
-- administrator on the next startup / bootstrap.
--
-- SAFE FOR DEVELOPMENT ONLY. Do not run against production.
-- =============================================================================
begin;

-- 1) Drop every role-permission grant (rebuilt from the catalog templates).
delete from compliance360.role_permissions;

-- 2) Drop every user-role assignment (platform admin re-assigned by bootstrap;
--    throwaway E2E users are intentionally reset).
delete from compliance360.user_roles;

-- 3) Drop every role (platform + tenant catalog roles are re-seeded).
delete from compliance360.roles;

-- 4) Drop every permission (official catalog is re-seeded from PermissionCatalog).
delete from compliance360.permissions;

commit;

-- Post-conditions (should all be zero until the app re-provisions on startup):
select
  (select count(*) from compliance360.permissions)     as permissions,
  (select count(*) from compliance360.roles)            as roles,
  (select count(*) from compliance360.role_permissions) as role_permissions,
  (select count(*) from compliance360.user_roles)       as user_roles;
